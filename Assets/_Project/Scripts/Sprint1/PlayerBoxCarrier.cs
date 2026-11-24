using System;
using ChoDoLa.Contracts;
using ChoDoLa.Player;
using UnityEngine;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Holds the player's currently carried blind box in first-person view with stylized two-hand low-poly arms.
    /// Players must physically walk from Nhà Niêm Phong across the street to Tiệm Mở Hòm and place the box on the table.
    /// </summary>
    public class PlayerBoxCarrier : MonoBehaviour
    {
        private const int IgnoreRaycastLayer = 2;

        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private ArtifactInspector3D inspector3D;

        private BoxInstanceView _carriedBox;
        private Transform _camTransform;
        private Transform _carryRig;
        private Transform _boxMount;
        private GameObject _carriedVisual;

        private Vector3 _lastPlayerWorldPos;
        private bool _hasLastPos;
        private float _walkBobPhase;
        private float _pickupBlend = 1f;

        public bool IsCarryingBox => _carriedBox != null;
        public BoxInstanceView CarriedBox => _carriedBox;

        public event Action<BoxInstanceView> OnCarriedBoxChanged;

        public void Configure(FirstPersonController fpc, ArtifactInspector3D inspector)
        {
            playerController = fpc;
            inspector3D = inspector;
        }

        public void PickupBox(BoxInstanceView box)
        {
            if (box == null) return;
            ClearCarriedVisualOnly();

            _carriedBox = box;
            _pickupBlend = 0f;
            EnsureCarryRig();

            if (_boxMount != null)
            {
                _carriedVisual = new GameObject($"CarriedBox_{box.BoxInstanceId}");
                _carriedVisual.transform.SetParent(_boxMount, false);
                _carriedVisual.transform.localPosition = Vector3.zero;
                _carriedVisual.transform.localRotation = Quaternion.Euler(4f, 0f, 0f);
                _carriedVisual.transform.localScale = Vector3.one * 0.62f;

                MarketVisualFactory.CreateBoxModel(_carriedVisual.transform, box.VisualVariantKey, out _, out _);
                StripCollidersAndSetIgnoreRaycast(_carriedVisual);
            }

            if (_carryRig != null)
            {
                _carryRig.gameObject.SetActive(true);
            }

            OnCarriedBoxChanged?.Invoke(_carriedBox);
        }

        public BoxInstanceView ReleaseCarriedBox()
        {
            BoxInstanceView box = _carriedBox;
            ClearCarriedBox();
            return box;
        }

        public void ClearCarriedBox()
        {
            bool hadBox = _carriedBox != null;
            _carriedBox = null;
            ClearCarriedVisualOnly();
            if (_carryRig != null)
            {
                _carryRig.gameObject.SetActive(false);
            }
            if (hadBox)
            {
                OnCarriedBoxChanged?.Invoke(null);
            }
        }

        private void ClearCarriedVisualOnly()
        {
            if (_carriedVisual != null)
            {
                Destroy(_carriedVisual);
                _carriedVisual = null;
            }
        }

        private void LateUpdate()
        {
            if (_carryRig == null)
            {
                if (_carriedBox == null) return;
                EnsureCarryRig();
                if (_carryRig == null) return;
            }

            bool inspecting = inspector3D != null && inspector3D.IsInspecting;
            bool shouldShow = _carriedBox != null && !inspecting;
            if (_carryRig.gameObject.activeSelf != shouldShow)
            {
                _carryRig.gameObject.SetActive(shouldShow);
            }

            if (!shouldShow) return;

            _pickupBlend = Mathf.Clamp01(_pickupBlend + Time.deltaTime * 4.5f);
            float easeOut = 1f - Mathf.Pow(1f - _pickupBlend, 3f);

            float speedFactor = 0f;
            Transform moveRoot = playerController != null ? playerController.transform : _camTransform;
            if (moveRoot != null)
            {
                Vector3 pos = moveRoot.position;
                if (_hasLastPos)
                {
                    Vector3 delta = pos - _lastPlayerWorldPos;
                    delta.y = 0f;
                    float horizSpeed = Time.deltaTime > 0.0001f ? delta.magnitude / Time.deltaTime : 0f;
                    speedFactor = Mathf.Clamp01(horizSpeed / 4.2f);
                }
                _lastPlayerWorldPos = pos;
                _hasLastPos = true;
            }

            _walkBobPhase += Time.deltaTime * (2.2f + speedFactor * 8.5f);
            float swayX = Mathf.Cos(_walkBobPhase * 0.5f) * (0.003f + speedFactor * 0.014f);
            float bobY = Mathf.Sin(_walkBobPhase) * (0.004f + speedFactor * 0.015f);

            float baseY = Mathf.Lerp(-0.46f, -0.21f, easeOut);
            _carryRig.localPosition = new Vector3(swayX, baseY + bobY, 0.54f);
            _carryRig.localRotation = Quaternion.Euler(2f * speedFactor * Mathf.Sin(_walkBobPhase), 0f, -swayX * 42f);
        }

        private void EnsureCarryRig()
        {
            if (_carryRig != null) return;

            Camera mainCam = Camera.main;
            if (mainCam == null) return;
            _camTransform = mainCam.transform;

            GameObject rigGo = new GameObject("FirstPerson_CarryBoxRig");
            rigGo.transform.SetParent(_camTransform, false);
            rigGo.transform.localPosition = new Vector3(0f, -0.21f, 0.54f);
            rigGo.layer = IgnoreRaycastLayer;
            _carryRig = rigGo.transform;

            GameObject mountGo = new GameObject("CarriedBox_Mount");
            mountGo.transform.SetParent(_carryRig, false);
            mountGo.transform.localPosition = new Vector3(0f, -0.055f, 0.02f);
            mountGo.layer = IgnoreRaycastLayer;
            _boxMount = mountGo.transform;

            BuildStylizedArms(_carryRig);
            _carryRig.gameObject.SetActive(false);
        }

        private static void BuildStylizedArms(Transform parent)
        {
            GameObject fbxHands = MarketVisualFactory.InstantiateBlenderModel("LP_FP_CarryHands", parent);
            if (fbxHands != null)
            {
                fbxHands.transform.localPosition = Vector3.zero;
                fbxHands.transform.localRotation = Quaternion.identity;
                fbxHands.transform.localScale = Vector3.one;
                StripCollidersAndSetIgnoreRaycast(parent.gameObject);
                return;
            }

            Material sleeveMat = MarketVisualFactory.GetMaterial("LP_CarrySleeve", new Color(0.12f, 0.33f, 0.34f), 0.0f, 0.14f);
            Material cuffMat = MarketVisualFactory.GetMaterial("LP_CarryCuff", new Color(0.96f, 0.94f, 0.88f), 0.0f, 0.14f);
            Material skinMat = MarketVisualFactory.GetMaterial("LP_CarrySkin", new Color(0.93f, 0.75f, 0.58f), 0.0f, 0.16f);

            for (int side = -1; side <= 1; side += 2)
            {
                string label = side < 0 ? "Left" : "Right";
                GameObject armPivot = new GameObject($"{label}Arm_Carry");
                armPivot.transform.SetParent(parent, false);
                armPivot.transform.localPosition = new Vector3(side * 0.215f, -0.02f, -0.06f);
                armPivot.transform.localRotation = Quaternion.Euler(-16f, -side * 16f, side * 8f);
                armPivot.layer = IgnoreRaycastLayer;

                // Stylized Low-Poly Upper Arm + Cuff + Hand cradling left/right sides of the box
                MarketVisualFactory.CreatePrimitive(
                    PrimitiveType.Cube, armPivot.transform, $"{label}_Forearm",
                    new Vector3(side * 0.035f, -0.04f, -0.15f), new Vector3(0.088f, 0.092f, 0.30f),
                    sleeveMat, removeCollider: true);

                MarketVisualFactory.CreatePrimitive(
                    PrimitiveType.Cube, armPivot.transform, $"{label}_Cuff",
                    new Vector3(side * 0.012f, -0.01f, -0.01f), new Vector3(0.092f, 0.096f, 0.055f),
                    cuffMat, removeCollider: true);

                MarketVisualFactory.CreatePrimitive(
                    PrimitiveType.Cube, armPivot.transform, $"{label}_Palm",
                    new Vector3(-side * 0.012f, 0.01f, 0.065f), new Vector3(0.056f, 0.086f, 0.125f),
                    skinMat, removeCollider: true);

                MarketVisualFactory.CreatePrimitive(
                    PrimitiveType.Cube, armPivot.transform, $"{label}_Thumb",
                    new Vector3(-side * 0.035f, 0.058f, 0.075f), new Vector3(0.045f, 0.030f, 0.065f),
                    skinMat, new Vector3(12f, -side * 14f, 0f), removeCollider: true);
            }

            StripCollidersAndSetIgnoreRaycast(parent.gameObject);
        }

        private static void StripCollidersAndSetIgnoreRaycast(GameObject root)
        {
            if (root == null) return;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = IgnoreRaycastLayer;
            }
            foreach (Collider col in root.GetComponentsInChildren<Collider>(true))
            {
                Destroy(col);
            }
        }
    }
}

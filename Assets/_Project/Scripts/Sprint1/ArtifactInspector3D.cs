using UnityEngine;
using ChoDoLa.Contracts;
using ChoDoLa.Interaction;
using ChoDoLa.Player;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Handles 3D close-up inspection of an opened artifact: smooth camera focus,
    /// 360-degree mouse-drag rotation, scroll zoom, and clean camera restoration.
    /// </summary>
    public class ArtifactInspector3D : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform inspectAnchor;
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private PlayerInteractor playerInteractor;
        [SerializeField] private Sprint1HudController hudController;

        [Header("Inspection Framing")]
        [SerializeField] private float defaultDistance = 0.62f;
        [SerializeField] private float minDistance = 0.36f;
        [SerializeField] private float maxDistance = 1.05f;
        [SerializeField] private float rotateSpeed = 0.45f;
        [SerializeField] private float zoomSpeed = 0.08f;
        [SerializeField] private float cameraLerpSpeed = 10f;

        private Camera _cam;
        private Transform _originalCamParent;
        private Vector3 _originalCamLocalPos;
        private Quaternion _originalCamLocalRot;

        private GameObject _currentModel;
        private ArtifactInstanceView _currentArtifact;
        private GameObject _inspectRigLights;
        private float _currentDistance;
        private float _targetYaw = 20f;
        private float _targetPitch = -12f;
        private bool _isInspecting;

        public bool IsInspecting => _isInspecting;
        public ArtifactInstanceView CurrentArtifact => _currentArtifact;

        public void Configure(Transform anchor, FirstPersonController fpc, PlayerInteractor interactor, Sprint1HudController hud)
        {
            inspectAnchor = anchor;
            playerController = fpc;
            playerInteractor = interactor;
            hudController = hud;
        }

        private void OnDisable()
        {
            ClearInspectLights();
        }

        public void BeginInspect(ArtifactInstanceView artifact)
        {
            if (artifact == null || inspectAnchor == null) return;

            if (_isInspecting)
            {
                ClearModel();
                ClearInspectLights();
            }

            _cam = Camera.main;
            if (_cam != null && !_isInspecting)
            {
                _originalCamParent = _cam.transform.parent;
                _originalCamLocalPos = _cam.transform.localPosition;
                _originalCamLocalRot = _cam.transform.localRotation;
                _cam.transform.SetParent(null, true);
            }

            _currentArtifact = artifact;
            _currentDistance = defaultDistance;
            _targetYaw = 22f;
            _targetPitch = -10f;
            _isInspecting = true;

            _currentModel = MarketVisualFactory.CreateArtifactModel(
                inspectAnchor,
                artifact.VisualPrefabKey,
                artifact.Quality,
                artifact.Authenticity);

            _currentModel.transform.localPosition = new Vector3(0f, 0.14f, 0f);
            _currentModel.transform.localRotation = Quaternion.Euler(_targetPitch, _targetYaw, 0f);

            SpawnInspectLights(artifact.Rarity);

            if (playerController != null) playerController.SetInputLocked(true);
            if (playerInteractor != null) playerInteractor.SetInteractionLocked(true);
            if (hudController != null) hudController.ShowInspectPanel(artifact);
        }

        public void EndInspect(bool hidePanel = true)
        {
            if (!_isInspecting) return;

            _isInspecting = false;
            ClearModel();
            ClearInspectLights();
            _currentArtifact = null;

            if (_cam != null && _originalCamParent != null)
            {
                _cam.transform.SetParent(_originalCamParent, false);
                _cam.transform.localPosition = _originalCamLocalPos;
                _cam.transform.localRotation = _originalCamLocalRot;
            }

            if (playerController != null) playerController.SetInputLocked(false);
            if (playerInteractor != null) playerInteractor.SetInteractionLocked(false);
            if (hidePanel && hudController != null) hudController.CloseAllModals();
        }

        private void SpawnInspectLights(RarityTier rarity)
        {
            ClearInspectLights();
            if (inspectAnchor == null) return;

            _inspectRigLights = new GameObject("Inspect_KeyRim_Lights");
            _inspectRigLights.transform.SetParent(inspectAnchor, false);

            GameObject keyGo = new GameObject("Inspect_KeyLight");
            keyGo.transform.SetParent(_inspectRigLights.transform, false);
            keyGo.transform.localPosition = new Vector3(0.32f, 0.50f, -0.46f);
            Light keyL = keyGo.AddComponent<Light>();
            keyL.type = LightType.Point;
            keyL.color = new Color(1.0f, 0.94f, 0.84f);
            keyL.intensity = 0.65f;
            keyL.range = 1.8f;

            GameObject rimGo = new GameObject("Inspect_RimLight");
            rimGo.transform.SetParent(_inspectRigLights.transform, false);
            rimGo.transform.localPosition = new Vector3(-0.34f, 0.42f, 0.42f);
            Light rimL = rimGo.AddComponent<Light>();
            rimL.type = LightType.Point;
            rimL.color = rarity switch
            {
                RarityTier.Uncommon => new Color(0.32f, 0.90f, 0.56f),
                RarityTier.Rare => new Color(0.28f, 0.85f, 1.0f),
                RarityTier.Mythic => new Color(1.0f, 0.80f, 0.25f),
                _ => new Color(0.88f, 0.90f, 0.95f)
            };
            rimL.intensity = 0.55f;
            rimL.range = 1.6f;
        }

        private void ClearInspectLights()
        {
            if (_inspectRigLights != null)
            {
                Destroy(_inspectRigLights);
                _inspectRigLights = null;
            }
        }

        private void ClearModel()
        {
            if (_currentModel != null)
            {
                Destroy(_currentModel);
                _currentModel = null;
            }
        }

        private void Update()
        {
            if (!_isInspecting || _currentModel == null || inspectAnchor == null) return;

            HandleRotationInput();
            HandleZoomInput();
            UpdateCameraTransform();
        }

        private void HandleRotationInput()
        {
            bool dragging = false;
            Vector2 delta = Vector2.zero;
            Vector2 keyAxis = Vector2.zero;
            bool resetPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                dragging = true;
                delta = Mouse.current.delta.ReadValue();
            }
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) keyAxis.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) keyAxis.x += 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) keyAxis.y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) keyAxis.y -= 1f;
                resetPressed = Keyboard.current.rKey.wasPressedThisFrame;
            }
#else
            if (Input.GetMouseButton(0))
            {
                dragging = true;
                delta = new Vector2(Input.GetAxis("Mouse X") * 15f, Input.GetAxis("Mouse Y") * 15f);
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) keyAxis.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) keyAxis.x += 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) keyAxis.y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) keyAxis.y -= 1f;
            resetPressed = Input.GetKeyDown(KeyCode.R);
#endif

            if (resetPressed)
            {
                _targetYaw = 22f;
                _targetPitch = -10f;
                _currentDistance = defaultDistance;
            }
            else if (dragging)
            {
                _targetYaw -= delta.x * rotateSpeed;
                _targetPitch = Mathf.Clamp(_targetPitch + delta.y * rotateSpeed, -75f, 75f);
            }
            else if (keyAxis.sqrMagnitude > 0.01f)
            {
                _targetYaw -= keyAxis.x * 95f * Time.deltaTime;
                _targetPitch = Mathf.Clamp(_targetPitch + keyAxis.y * 85f * Time.deltaTime, -75f, 75f);
            }
            else
            {
                // Gentle idle turntable when idle
                _targetYaw += Time.deltaTime * 10f;
            }

            Quaternion targetRot = Quaternion.Euler(_targetPitch, _targetYaw, 0f);
            _currentModel.transform.localRotation = Quaternion.Slerp(_currentModel.transform.localRotation, targetRot, Time.deltaTime * 14f);
        }

        private void HandleZoomInput()
        {
            float scroll = 0f;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                scroll = Mouse.current.scroll.ReadValue().y * 0.01f;
            }
#else
            scroll = Input.GetAxis("Mouse ScrollWheel") * 10f;
#endif
            if (Mathf.Abs(scroll) > 0.001f)
            {
                _currentDistance = Mathf.Clamp(_currentDistance - scroll * zoomSpeed, minDistance, maxDistance);
            }
        }

        private void UpdateCameraTransform()
        {
            if (_cam == null) return;

            Vector3 pivotCenter = inspectAnchor.position + new Vector3(0f, 0.24f, 0f);
            Vector3 desiredPos = pivotCenter - inspectAnchor.forward * _currentDistance + Vector3.up * 0.14f;
            Quaternion desiredRot = Quaternion.LookRotation((pivotCenter - desiredPos).normalized, Vector3.up);

            _cam.transform.position = Vector3.Lerp(_cam.transform.position, desiredPos, Time.deltaTime * cameraLerpSpeed);
            _cam.transform.rotation = Quaternion.Slerp(_cam.transform.rotation, desiredRot, Time.deltaTime * cameraLerpSpeed);
        }
    }
}

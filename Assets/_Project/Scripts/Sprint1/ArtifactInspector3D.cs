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

        public void BeginInspect(ArtifactInstanceView artifact)
        {
            if (artifact == null || inspectAnchor == null) return;

            if (_isInspecting)
            {
                ClearModel();
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

            if (playerController != null) playerController.SetInputLocked(true);
            if (playerInteractor != null) playerInteractor.SetInteractionLocked(true);
            if (hudController != null) hudController.ShowInspectPanel(artifact);
        }

        public void EndInspect(bool hidePanel = true)
        {
            if (!_isInspecting) return;

            _isInspecting = false;
            ClearModel();
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

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                dragging = true;
                delta = Mouse.current.delta.ReadValue();
            }
#else
            if (Input.GetMouseButton(0))
            {
                dragging = true;
                delta = new Vector2(Input.GetAxis("Mouse X") * 15f, Input.GetAxis("Mouse Y") * 15f);
            }
#endif

            if (dragging)
            {
                _targetYaw -= delta.x * rotateSpeed;
                _targetPitch = Mathf.Clamp(_targetPitch + delta.y * rotateSpeed, -75f, 75f);
            }
            else
            {
                // Gentle idle turntable when not dragging
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

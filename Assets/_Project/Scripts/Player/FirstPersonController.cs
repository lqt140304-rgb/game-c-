using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.Player
{
    /// <summary>
    /// Minimal First-Person controller for Sprint 0 Play Mode verification.
    /// Supports both Unity Input System and legacy Input Manager.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Look")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float maxPitch = 80f;

        private CharacterController _controller;
        private float _pitch;
        private float _verticalVelocity;
        private bool _isInputLocked;

        public bool IsInputLocked => _isInputLocked;

        public void SetInputLocked(bool locked)
        {
            _isInputLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraPivot == null && Camera.main != null)
            {
                cameraPivot = Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (_isInputLocked) return;
            HandleCursorToggle();
            HandleLook();
            HandleMove();
        }

        private void HandleCursorToggle()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
#else
            if (Input.GetKeyDown(KeyCode.Escape))
#endif
            {
                bool locked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }
        }

        private void HandleLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked || cameraPivot == null) return;

            Vector2 lookDelta = ReadLookDelta();
            transform.Rotate(Vector3.up, lookDelta.x * mouseSensitivity);

            _pitch = Mathf.Clamp(_pitch - lookDelta.y * mouseSensitivity, -maxPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void HandleMove()
        {
            Vector2 moveInput = ReadMoveInput();
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            if (move.sqrMagnitude > 1f) move.Normalize();

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            _verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = move * moveSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        private static Vector2 ReadMoveInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) return Vector2.zero;
            float x = 0f;
            float y = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
            return new Vector2(x, y);
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }

        private static Vector2 ReadLookDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null) return Vector2.zero;
            return Mouse.current.delta.ReadValue();
#else
            return new Vector2(Input.GetAxis("Mouse X") * 15f, Input.GetAxis("Mouse Y") * 15f);
#endif
        }
    }
}

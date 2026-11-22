using System;
using UnityEngine;
using ChoDoLa.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.Interaction
{
    /// <summary>
    /// Raycasts from the First-Person camera to detect and trigger IInteractable objects.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactDistance = 3.0f;
        [SerializeField] private LayerMask interactLayerMask = ~0;

        [Header("UI")]
        [SerializeField] private Sprint0HudView hudView;

        private IInteractable _currentTarget;
        private bool _isInteractionLocked;

        public event Action<string> OnPromptChanged;

        public void SetInteractionLocked(bool locked)
        {
            _isInteractionLocked = locked;
            if (locked)
            {
                _currentTarget = null;
                PublishPrompt(string.Empty);
            }
        }

        private void Reset()
        {
            playerCamera = Camera.main;
        }

        private void Update()
        {
            if (_isInteractionLocked) return;

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null) return;
            }

            DetectTarget();

            if (_currentTarget != null && _currentTarget.CanInteract && IsInteractPressed())
            {
                _currentTarget.Interact();
            }
        }

        private void DetectTarget()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    _currentTarget = interactable;
                    PublishPrompt(interactable.CanInteract ? $"[E] {interactable.PromptText}" : string.Empty);
                    return;
                }
            }

            _currentTarget = null;
            PublishPrompt(string.Empty);
        }

        private void PublishPrompt(string prompt)
        {
            if (hudView != null) hudView.SetInteractPrompt(prompt);
            OnPromptChanged?.Invoke(prompt);
        }

        private static bool IsInteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }
    }
}

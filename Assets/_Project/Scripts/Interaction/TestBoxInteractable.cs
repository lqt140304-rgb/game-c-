using UnityEngine;

namespace ChoDoLa.Interaction
{
    /// <summary>
    /// Minimal Sprint 0 placeholder interactable for testing Play Mode before 3D models arrive.
    /// </summary>
    public class TestBoxInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string boxLabel = "Hộp Mù Mẫu (Sprint 0)";
        [SerializeField] private Color interactedColor = new Color(0.95f, 0.75f, 0.25f);

        private bool _opened;

        public string PromptText => _opened ? $"{boxLabel} (Đã mở)" : $"Mở {boxLabel}";
        public bool CanInteract => !_opened;

        public void Interact()
        {
            if (_opened) return;
            _opened = true;

            Renderer meshRenderer = GetComponentInChildren<Renderer>();
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = interactedColor;
            }

            Debug.Log($"[Sprint0] Interacted with {boxLabel}");
        }
    }
}

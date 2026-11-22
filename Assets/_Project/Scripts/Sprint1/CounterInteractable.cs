using UnityEngine;
using ChoDoLa.Interaction;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Interactable for Dì Mận's Sealing Counter (Quầy Niêm Phong).
    /// </summary>
    public class CounterInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Sprint1HudController hudController;

        public string PromptText => "Quầy Niêm Phong (Dì Mận) — Chọn & Mua Hộp Mù";
        public bool CanInteract => hudController != null;

        public void Configure(Sprint1HudController hud)
        {
            hudController = hud;
        }

        public void Interact()
        {
            if (hudController != null)
            {
                hudController.OpenCounterModal(null);
            }
        }
    }
}

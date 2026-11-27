using UnityEngine;
using ChoDoLa.Interaction;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Interactable kiosk for Kho Tạo Tác — Kim Khuy (Tact Artifacts, Combos & Heirlooms).
    /// </summary>
    public class KimKhuyInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Sprint1HudController hudController;

        public string PromptText => "Kho Tạo Tác (Kim Khuy) — Mua & Trang Bị Cộng Hưởng";
        public bool CanInteract => hudController != null;

        public void Configure(Sprint1HudController hud)
        {
            hudController = hud;
        }

        public void Interact()
        {
            if (hudController != null)
            {
                hudController.OpenTactShopModal();
            }
        }
    }
}

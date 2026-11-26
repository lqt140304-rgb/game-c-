using UnityEngine;
using ChoDoLa.Interaction;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Interactable for the Inn Ledger / Debt Desk (Anh Bảy & Kết thúc ngày).
    /// </summary>
    public class InnLedgerInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Sprint1HudController hudController;

        public string PromptText => "Sổ Phòng Trọ — Trả Nợ (Anh Bảy) & Kết Thúc Ngày";
        public bool CanInteract => hudController != null;

        public void Configure(Sprint1HudController hud)
        {
            hudController = hud;
        }

        public void Interact()
        {
            if (hudController != null)
            {
                hudController.OpenLedgerModal(startOnDebtTab: true);
            }
        }
    }
}

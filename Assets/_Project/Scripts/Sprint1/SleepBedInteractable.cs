using ChoDoLa.Interaction;
using UnityEngine;

namespace ChoDoLa.Sprint1
{
    [DisallowMultipleComponent]
    public sealed class SleepBedInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Sprint1HudController hudController;

        public string PromptText => CanInteract ? "Ngủ đến sáng" : "Có thể ngủ sau 18:00";
        public bool CanInteract => hudController != null && hudController.CanSleepAtBed;

        private void Start()
        {
            if (hudController == null) hudController = FindFirstObjectByType<Sprint1HudController>();
            if (hudController != null) hudController.EnableSleepAtBed();
        }

        public void Interact()
        {
            if (CanInteract) hudController.SleepAtBed();
        }
    }
}

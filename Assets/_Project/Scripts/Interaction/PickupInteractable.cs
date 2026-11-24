using UnityEngine;

namespace ChoDoLa.Interaction
{
    /// <summary>
    /// A world object the player can pick up with [E]. It moves into the player's hand
    /// (see <see cref="HeldItemViewmodel"/>) and returns to the world when the player puts it away.
    /// </summary>
    public class PickupInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string itemId = "V01";
        [SerializeField] private string displayName = "Hiện vật";
        [Tooltip("Model shown in the hand. Leave empty to use this object's own visuals.")]
        [SerializeField] private GameObject heldVisualPrefab;
        [SerializeField] private HeldItemViewmodel viewmodel;

        public string PromptText => $"Cầm {displayName}";

        public bool CanInteract => viewmodel != null && !viewmodel.HasItem;

        public void Interact()
        {
            if (!CanInteract) return;

            GameObject source = heldVisualPrefab != null ? heldVisualPrefab : gameObject;
            viewmodel.Equip(source, itemId, OnPutAway);

            if (viewmodel.HasItem)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnPutAway()
        {
            gameObject.SetActive(true);
        }
    }
}

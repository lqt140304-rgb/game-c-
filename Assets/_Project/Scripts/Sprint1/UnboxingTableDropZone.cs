using ChoDoLa.Contracts;
using ChoDoLa.Interaction;
using UnityEngine;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Interactable drop zone on the Velvet Unboxing Table inside Tiệm Mở Hòm.
    /// Active for raycasting only while the player is physically carrying a box with both hands.
    /// Stored in its own file so Unity serializes and persists the MonoBehaviour cleanly across Play Mode.
    /// </summary>
    public class UnboxingTableDropZone : MonoBehaviour, IInteractable
    {
        private OpenTableManager _tableManager;
        private PlayerBoxCarrier _carrier;
        private BoxCollider _interactCollider;

        public string PromptText => _carrier != null && _carrier.CarriedBox != null
            ? (_tableManager != null && _tableManager.HasFreeSlot()
                ? $"Đặt {_carrier.CarriedBox.DisplayName} lên Bàn Mở"
                : "Bàn Mở đang đầy (hãy mở bớt hộp trên bàn)")
            : string.Empty;

        public bool CanInteract => _carrier != null && _carrier.IsCarryingBox && _tableManager != null && _tableManager.HasFreeSlot();

        public void Configure(OpenTableManager tableMgr, PlayerBoxCarrier carrier)
        {
            _tableManager = tableMgr;
            _carrier = carrier;
            EnsureSync();
        }

        private void Awake()
        {
            EnsureSync();
        }

        private void Start()
        {
            if (_tableManager == null) _tableManager = FindAnyObjectByType<OpenTableManager>();
            if (_carrier == null) _carrier = FindAnyObjectByType<PlayerBoxCarrier>();
            EnsureSync();
        }

        private void Update()
        {
            EnsureSync();
        }

        private void EnsureSync()
        {
            if (_interactCollider == null) _interactCollider = GetComponent<BoxCollider>();
            if (_interactCollider == null) return;
            bool shouldEnable = _carrier != null && _carrier.IsCarryingBox;
            if (_interactCollider.enabled != shouldEnable)
            {
                _interactCollider.enabled = shouldEnable;
            }
        }

        public void Interact()
        {
            if (!CanInteract) return;
            BoxInstanceView box = _carrier.ReleaseCarriedBox();
            if (box != null)
            {
                _tableManager.PlaceCarriedBoxOnTable(box);
            }
        }
    }
}

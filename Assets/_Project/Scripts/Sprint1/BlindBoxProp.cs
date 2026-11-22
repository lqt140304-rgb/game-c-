using System.Collections;
using UnityEngine;
using ChoDoLa.Contracts;
using ChoDoLa.Interaction;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Interactive 3D blind box. Can act either as a Counter Display offer (opens Counter UI)
    /// or a Purchased Box on the Opening Table (plays 3D unboxing animation and inspects artifact).
    /// </summary>
    public class BlindBoxProp : MonoBehaviour, IInteractable
    {
        [Header("Mode")]
        [SerializeField] private bool isCounterDisplay;
        [SerializeField] private string offerId;
        [SerializeField] private string displayLabel;
        [SerializeField] private int displayPrice;

        [Header("Runtime References")]
        [SerializeField] private Sprint1MarketService marketService;
        [SerializeField] private Sprint1HudController hudController;

        private string _boxInstanceId;
        private Transform _lidPivot;
        private Transform _sealPart;
        private bool _isOpening;
        private bool _isOpened;

        public string BoxInstanceId => _boxInstanceId;

        public string PromptText
        {
            get
            {
                if (isCounterDisplay)
                {
                    return $"Xem / Mua {displayLabel} ({displayPrice} xu)";
                }
                return _isOpened ? string.Empty : $"Tháo niêm phong & Mở {displayLabel}";
            }
        }

        public bool CanInteract => isCounterDisplay || (!_isOpening && !_isOpened);

        public void ConfigureCounterDisplay(string id, string label, int price, Sprint1MarketService service, Sprint1HudController hud)
        {
            isCounterDisplay = true;
            offerId = id;
            displayLabel = label;
            displayPrice = price;
            marketService = service;
            hudController = hud;
        }

        public void InitializePurchasedBox(BoxInstanceView box, Sprint1MarketService service, Sprint1HudController hud, Transform lidPivot, Transform sealPart)
        {
            isCounterDisplay = false;
            _boxInstanceId = box.BoxInstanceId;
            displayLabel = box.DisplayName;
            displayPrice = box.PricePaid;
            marketService = service;
            hudController = hud;
            _lidPivot = lidPivot;
            _sealPart = sealPart;
            _isOpening = false;
            _isOpened = false;
        }

        public void Interact()
        {
            if (isCounterDisplay)
            {
                if (hudController != null)
                {
                    hudController.OpenCounterModal(offerId);
                }
                return;
            }

            if (_isOpening || _isOpened || marketService == null) return;
            marketService.RequestOpenBox(_boxInstanceId);
        }

        public IEnumerator PlayUnboxAnimation()
        {
            _isOpening = true;

            Vector3 baseLocalPos = transform.localPosition;

            // Phase 1: Subtle rattle & unsealing
            float t = 0f;
            while (t < 0.28f)
            {
                t += Time.deltaTime;
                float shake = Mathf.Sin(t * 55f) * 0.008f;
                transform.localPosition = baseLocalPos + new Vector3(shake, 0f, 0f);
                yield return null;
            }
            transform.localPosition = baseLocalPos;

            if (_sealPart != null)
            {
                _sealPart.gameObject.SetActive(false);
            }

            // Phase 2: Hinged lid opens 110 degrees
            t = 0f;
            const float openDuration = 0.42f;
            while (t < openDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / openDuration);
                if (_lidPivot != null)
                {
                    _lidPivot.localRotation = Quaternion.Euler(p * 110f, 0f, 0f);
                }
                yield return null;
            }

            _isOpened = true;
            _isOpening = false;
        }
    }
}

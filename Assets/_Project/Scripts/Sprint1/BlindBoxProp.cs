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
                    return $"Xem mẫu {displayLabel} ({displayPrice} xu) — Gặp Dì Mận để chốt mua";
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
                    hudController.InspectDisplayOffer(offerId, displayLabel, displayPrice);
                }
                return;
            }

            if (_isOpening || _isOpened || marketService == null) return;
            marketService.RequestOpenBox(_boxInstanceId);
        }

        public IEnumerator PlayUnboxAnimation(RarityTier rarity = RarityTier.Common)
        {
            _isOpening = true;

            Vector3 baseLocalPos = transform.localPosition;
            Quaternion baseLidRot = _lidPivot != null ? _lidPivot.localRotation : Quaternion.identity;

            // Phase 1: Unseal rattle + wax seal pops forward off the clasp
            float t = 0f;
            Vector3 sealStartPos = _sealPart != null ? _sealPart.localPosition : Vector3.zero;
            Quaternion sealStartRot = _sealPart != null ? _sealPart.localRotation : Quaternion.identity;
            while (t < 0.36f)
            {
                t += Time.deltaTime;
                float u1 = Mathf.Clamp01(t / 0.36f);
                float shake = Mathf.Sin(t * 58f) * 0.007f * (1f - u1 * 0.6f);
                transform.localPosition = baseLocalPos + new Vector3(shake, 0f, 0f);
                if (_sealPart != null)
                {
                    _sealPart.localPosition = sealStartPos + new Vector3(0f, 0.04f * Mathf.Sin(u1 * Mathf.PI) - 0.14f * u1 * u1, -0.10f * u1);
                    _sealPart.localRotation = sealStartRot * Quaternion.Euler(u1 * 140f, 0f, u1 * 45f);
                    _sealPart.localScale = Vector3.one * Mathf.Lerp(1f, 0.25f, u1);
                }
                yield return null;
            }
            transform.localPosition = baseLocalPos;

            if (_sealPart != null)
            {
                _sealPart.gameObject.SetActive(false);
            }

            // Rarity Glow, Rising Inner Orb & Halo Ring reveal from inside the plush velvet box bed
            Color rarityCol = GetRarityGlowColor(rarity);
            GameObject glowGo = new GameObject("Unbox_RarityGlow");
            glowGo.transform.SetParent(transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            Light glowLight = glowGo.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.color = rarityCol;
            glowLight.range = 3.2f;
            glowLight.intensity = 0f;

            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Unbox_InnerCoreOrb";
            orb.transform.SetParent(transform, false);
            orb.transform.localPosition = new Vector3(0f, 0.16f, 0f);
            orb.transform.localScale = Vector3.one * 0.04f;
            Collider orbCol = orb.GetComponent<Collider>();
            if (orbCol != null) Destroy(orbCol);
            Renderer orbRend = orb.GetComponent<Renderer>();
            if (orbRend != null)
            {
                orbRend.sharedMaterial = MarketVisualFactory.GetMaterial($"UnboxOrb_{rarity}", rarityCol, 0.2f, 0.7f, rarityCol * 1.15f);
            }

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Unbox_RarityHalo";
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.14f, 0f);
            ring.transform.localScale = new Vector3(0.18f, 0.006f, 0.18f);
            Collider ringCol = ring.GetComponent<Collider>();
            if (ringCol != null) Destroy(ringCol);
            Renderer ringRend = ring.GetComponent<Renderer>();
            if (ringRend != null)
            {
                ringRend.sharedMaterial = MarketVisualFactory.GetMaterial($"UnboxGlow_{rarity}", rarityCol, 0.1f, 0.6f, rarityCol * 1.05f);
            }

            // Phase 2: Hinged lid opens backward smoothly relative to baseLidRot while soft glow orb rises
            t = 0f;
            const float openDuration = 0.52f;
            float peakIntensity = rarity == RarityTier.Mythic ? 1.10f : (rarity == RarityTier.Rare ? 0.80f : 0.55f);
            while (t < openDuration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / openDuration);
                float easeOut = 1f - Mathf.Pow(1f - u, 3f);
                float overshoot = Mathf.Sin(u * Mathf.PI) * 0.04f;
                float angleDeg = (easeOut + overshoot) * 102f;
                if (_lidPivot != null)
                {
                    _lidPivot.localRotation = baseLidRot * Quaternion.Euler(angleDeg, 0f, 0f);
                }

                glowLight.intensity = Mathf.Sin(u * Mathf.PI * 0.85f) * peakIntensity;
                orb.transform.localPosition = new Vector3(0f, 0.16f + easeOut * 0.22f, 0f);
                orb.transform.localScale = Vector3.one * Mathf.Lerp(0.04f, 0.11f, Mathf.Sin(u * Mathf.PI * 0.85f));
                ring.transform.localPosition = new Vector3(0f, 0.14f + easeOut * 0.24f, 0f);
                float s = Mathf.Lerp(0.18f, rarity == RarityTier.Mythic ? 0.36f : 0.28f, easeOut);
                ring.transform.localScale = new Vector3(s, 0.006f, s);
                yield return null;
            }
            if (_lidPivot != null)
            {
                _lidPivot.localRotation = baseLidRot * Quaternion.Euler(102f, 0f, 0f);
            }
            Destroy(orb);

            StartCoroutine(PlayBurstSparks(rarityCol, rarity == RarityTier.Mythic ? 16 : (rarity == RarityTier.Rare ? 12 : 8)));

            _isOpened = true;
            _isOpening = false;
        }

        private IEnumerator PlayBurstSparks(Color rarityCol, int count)
        {
            Material sparkMat = MarketVisualFactory.GetMaterial($"UnboxSpark_{rarityCol}", rarityCol, 0.1f, 0.7f, rarityCol * 2.2f);
            Transform[] sparks = new Transform[count];
            Vector3[] velocities = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sp.name = $"Unbox_Spark_{i}";
                sp.transform.SetParent(transform, false);
                sp.transform.localPosition = new Vector3(0f, 0.14f, 0f);
                sp.transform.localScale = Vector3.one * 0.022f;
                Collider col = sp.GetComponent<Collider>();
                if (col != null) Destroy(col);
                Renderer r = sp.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = sparkMat;

                float angle = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.25f, 0.25f);
                float speedH = Random.Range(0.28f, 0.56f);
                velocities[i] = new Vector3(Mathf.Cos(angle) * speedH, Random.Range(0.55f, 0.95f), Mathf.Sin(angle) * speedH);
                sparks[i] = sp.transform;
            }

            float elapsed = 0f;
            const float dur = 0.65f;
            while (elapsed < dur)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                float u = Mathf.Clamp01(elapsed / dur);
                for (int i = 0; i < count; i++)
                {
                    if (sparks[i] == null) continue;
                    velocities[i].y -= 1.35f * dt;
                    sparks[i].localPosition += velocities[i] * dt;
                    sparks[i].Rotate(240f * dt, 180f * dt, 0f, Space.Self);
                    sparks[i].localScale = Vector3.one * (0.024f * (1f - u * u));
                }
                yield return null;
            }

            for (int i = 0; i < count; i++)
            {
                if (sparks[i] != null) Destroy(sparks[i].gameObject);
            }
        }

        private static Color GetRarityGlowColor(RarityTier r) => r switch
        {
            RarityTier.Uncommon => new Color(0.24f, 0.88f, 0.48f),
            RarityTier.Rare => new Color(0.22f, 0.82f, 1.0f),
            RarityTier.Mythic => new Color(1.0f, 0.78f, 0.22f),
            _ => new Color(0.95f, 0.90f, 0.78f)
        };
    }
}

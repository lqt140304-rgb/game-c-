using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_NhatBan's own extras (decoration only, never touches the rolled tier):
    /// - every roulette step drops a tiny puff of sakura petals (the "tick" you can see);
    /// - on the lock a sakura shower falls from above the doll, bigger for higher tiers;
    /// - Rác: the Daruma tumbles to one side and springs back upright ("nanakorobi yaoki", comedic);
    /// - Rất hiếm / Huyền thoại: taiko beats, each one a red-gold light pulse and a ring of petals.
    /// Put it on the profile's customFxPrefab (NhatBanFxSetup does that) or on the chest rig.
    /// </summary>
    public class NhatBanHook : ChestFxHook
    {
        [SerializeField] private Color petalColor = new Color(1f, 0.72f, 0.82f);
        [SerializeField] private Color drumLightColor = new Color(1f, 0.45f, 0.25f);
        [SerializeField] private int petalsPerStep = 3;
        [SerializeField] private int showerPetals = 40;
        [SerializeField, Tooltip("Height of the shower above the chest mouth, metres.")] private float showerHeight = 0.6f;
        [SerializeField] private float showerRadius = 0.35f;
        [SerializeField, Tooltip("Rác: the chest tips over and rights itself. Turn off if it fights BoxOpenSequence.")] private bool tumbleOnTrash = true;
        [SerializeField] private float tumbleAngle = 35f;
        [SerializeField] private float tumbleSeconds = 1.1f;
        [SerializeField] private float drumBeatGap = 0.2f;

        private ParticleSystem _puff;
        private ParticleSystem _shower;
        private ChestFxProfile _puffStyle;
        private ChestFxProfile _showerStyle;
        private Light _drumLight;
        private Coroutine _tumble;
        private Coroutine _drums;
        private Transform _tumbled;
        private Quaternion _tumbleStart;

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            EnsureParticles();
            _puff.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
            ChestFxFactory.SetParticleColor(_puff, color, _puffStyle, 1f);
            _puff.Play();
            _puff.Emit(petalsPerStep);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            EnsureParticles();
            _shower.transform.SetPositionAndRotation(reveal.MouthPosition + Vector3.up * showerHeight, Quaternion.identity);
            ChestFxFactory.SetParticleColor(_shower, color, _showerStyle, 1f);
            _shower.Play();
            _shower.Emit(Mathf.RoundToInt(showerPetals * (0.5f + 1.5f * tier.Rank01())));

            if (tier == ChestTier.Rac && tumbleOnTrash)
            {
                StopTumble();
                _tumble = StartCoroutine(Tumble(reveal.transform));
            }
            if (tier >= ChestTier.RatHiem)
            {
                if (_drums != null) StopCoroutine(_drums);
                _drums = StartCoroutine(Drums(reveal, color, tier == ChestTier.HuyenThoai ? 5 : 3));
            }
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            StopTumble();
            if (_drums != null) StopCoroutine(_drums);
            _drums = null;
            if (_drumLight != null) _drumLight.intensity = 0f;
        }

        private void OnDestroy()
        {
            StopTumble();
            DestroyParticles(ref _puff);
            DestroyParticles(ref _shower);
            if (_puffStyle != null) Destroy(_puffStyle);
            if (_showerStyle != null) Destroy(_showerStyle);
            if (_drumLight != null) Destroy(_drumLight.gameObject);
        }

        // ------------------------------------------------------------------ Daruma tumble (Rác)

        private IEnumerator Tumble(Transform chest)
        {
            _tumbled = chest;
            _tumbleStart = chest.localRotation;
            float side = Random.Range(0, 2) == 0 ? 1f : -1f;
            float t = 0f;
            while (t < tumbleSeconds)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / tumbleSeconds);
                // Tip over fast (first 30%), lie there a beat, then spring up with a damped wobble.
                float angle;
                if (k < 0.3f) angle = Mathf.SmoothStep(0f, 1f, k / 0.3f);
                else if (k < 0.45f) angle = 1f;
                else
                {
                    float u = (k - 0.45f) / 0.55f;
                    angle = Mathf.Exp(-5f * u) * Mathf.Cos(u * Mathf.PI * 3.5f);
                }
                chest.localRotation = _tumbleStart * Quaternion.AngleAxis(angle * tumbleAngle * side, Vector3.forward);
                yield return null;
            }
            chest.localRotation = _tumbleStart;
            _tumbled = null;
            _tumble = null;
        }

        private void StopTumble()
        {
            if (_tumble != null) StopCoroutine(_tumble);
            _tumble = null;
            if (_tumbled != null) _tumbled.localRotation = _tumbleStart;
            _tumbled = null;
        }

        // ------------------------------------------------------------------ Taiko beats (Rất hiếm, Huyền thoại)

        private IEnumerator Drums(ChestRevealSequence reveal, Color tierColor, int beats)
        {
            if (_drumLight == null)
            {
                GameObject go = new GameObject("NhatBanDrumLight");
                go.hideFlags = HideFlags.DontSave;
                _drumLight = go.AddComponent<Light>();
                _drumLight.type = LightType.Point;
                _drumLight.range = 2.5f;
                _drumLight.shadows = LightShadows.None;
            }
            _drumLight.transform.position = reveal.MouthPosition + Vector3.up * 0.2f;
            _drumLight.color = Color.Lerp(drumLightColor, tierColor, 0.4f);
            for (int b = 0; b < beats; b++)
            {
                yield return new WaitForSeconds(drumBeatGap);
                float peak = b == beats - 1 ? 5f : 3f;   // last beat ("dodon!") hits harder
                _shower.transform.SetPositionAndRotation(reveal.MouthPosition, Quaternion.identity);
                _shower.Emit(b == beats - 1 ? 30 : 12);
                float t = 0f;
                while (t < drumBeatGap * 0.9f)
                {
                    t += Time.deltaTime;
                    _drumLight.intensity = peak * Mathf.Exp(-6f * t / drumBeatGap);
                    yield return null;
                }
            }
            _drumLight.intensity = 0f;
            _drums = null;
        }

        // ------------------------------------------------------------------ Particles

        private void EnsureParticles()
        {
            if (_puff == null)
            {
                _puffStyle = Style(ParticleDirection.Swirl, 0.25f, 0.026f, 1.2f, 0f);
                _puff = ChestFxFactory.CreateParticles("NhatBanPetalPuff", null, _puffStyle, 0.5f, true);
            }
            if (_shower == null)
            {
                _showerStyle = Style(ParticleDirection.Radial, 0.15f, 0.04f, 2.4f, 0.08f);
                _shower = ChestFxFactory.CreateParticles("NhatBanSakuraShower", null, _showerStyle, showerRadius * 4f, true);
                ParticleSystem.ShapeModule shape = _shower.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = showerRadius;
                shape.rotation = new Vector3(90f, 0f, 0f);
                ParticleSystem.RotationOverLifetimeModule rot = _shower.rotationOverLifetime;
                rot.enabled = true;   // petals flutter as they fall
                rot.z = new ParticleSystem.MinMaxCurve(-2f, 2f);
            }
        }

        private ChestFxProfile Style(ParticleDirection dir, float speed, float size, float life, float gravity)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = ParticleLook.SoftDot;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.burstGravity = gravity;
            s.secondaryColor = petalColor;
            s.secondaryMix = 0.85f;
            return s;
        }

        private static void DestroyParticles(ref ParticleSystem ps)
        {
            if (ps == null) return;
            Destroy(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            Destroy(ps.gameObject);
            ps = null;
        }
    }
}

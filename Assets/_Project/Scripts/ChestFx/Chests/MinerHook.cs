using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Miner extras: a warm miner's lamp above the chest that flickers during the roulette (a sharp dip on
    /// every pick tick), orange pick sparks on each tick, dark rock chunks thrown up and falling at the lock, gem
    /// glints in the tier colour from Hiếm up, and for Huyền thoại a "dynamite" lamp stutter. Decoration only.
    /// Put it on the chest root (MinerFxSetup's second menu does that).
    /// </summary>
    public class MinerHook : ChestFxHook
    {
        [SerializeField] private Color lampColor = new Color(1f, 0.62f, 0.25f);
        [SerializeField] private float lampIntensity = 1.6f;
        [SerializeField] private float lampHeight = 0.45f;
        [SerializeField] private int sparksPerTick = 5;
        [SerializeField] private int rockChunks = 14;
        [SerializeField] private int gemGlints = 18;
        [SerializeField] private Color rockColor = new Color(0.22f, 0.19f, 0.16f);
        [SerializeField] private Color sparkColor = new Color(1f, 0.7f, 0.3f);

        private Light _lamp;
        private ParticleSystem _sparks, _rocks, _gems;
        private ChestFxProfile _sparkStyle, _rockStyle, _gemStyle;
        private float _dip;          // 1 right after a tick, decays
        private float _blast;        // seconds of stutter left
        private bool _lampOn;

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            Build();
            _lamp.transform.position = mouth + reveal.transform.up * lampHeight;
            _sparks.transform.SetPositionAndRotation(mouth, reveal.transform.rotation);
            _rocks.transform.SetPositionAndRotation(mouth, reveal.transform.rotation);
            _gems.transform.SetPositionAndRotation(mouth, reveal.transform.rotation);
            _lamp.color = lampColor;
            _lamp.enabled = true;
            _lampOn = true;
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (_sparks == null) return;
            _dip = 1f;
            ChestFxFactory.SetParticleColor(_sparks, sparkColor, _sparkStyle, 1f);
            _sparks.Emit(sparksPerTick);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_rocks == null) return;
            float k = 1f + tier.Rank01() * 1.5f;
            ChestFxFactory.SetParticleColor(_rocks, rockColor, _rockStyle, 1f);
            _rocks.Emit(Mathf.RoundToInt(rockChunks * k));
            if (tier >= ChestTier.Hiem)
            {
                ChestFxFactory.SetParticleColor(_gems, color, _gemStyle, 1.4f);
                _gems.Emit(Mathf.RoundToInt(gemGlints * k));
            }
            _lamp.color = Color.Lerp(lampColor, color, 0.5f);
            _blast = tier == ChestTier.HuyenThoai ? 0.8f : 0f;
            _dip = 1f;
        }

        public override void OnRevealed(ChestRevealSequence reveal) => _lampOn = false;

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Update()
        {
            if (_lamp == null) return;
            if (!_lampOn)
            {
                // fade out after the reveal
                _lamp.intensity = Mathf.Max(0f, _lamp.intensity - Time.deltaTime * lampIntensity);
                if (_lamp.intensity <= 0f) _lamp.enabled = false;
                return;
            }
            float t = Time.time;
            float flicker = 0.85f + 0.15f * Mathf.PerlinNoise(t * 9f, 0.37f);
            _dip = Mathf.Max(0f, _dip - Time.deltaTime * 9f);
            float level = lampIntensity * flicker * (1f - 0.55f * _dip);
            if (_blast > 0f)
            {
                _blast -= Time.deltaTime;
                // hard on/off stutter, like the lamp shaking from a distant blast
                level *= Mathf.PerlinNoise(t * 40f, 1.3f) > 0.5f ? 3f : 0.15f;
            }
            _lamp.intensity = level;
        }

        private void Build()
        {
            if (_lamp != null) return;
            GameObject lampGo = new GameObject("MinerLamp");
            lampGo.transform.SetParent(transform, true);
            _lamp = lampGo.AddComponent<Light>();
            _lamp.type = LightType.Point;
            _lamp.range = 2.2f;
            _lamp.shadows = LightShadows.None;
            _lamp.intensity = 0f;

            _sparkStyle = Style(ParticleLook.Streak, ParticleDirection.Radial, 1.3f, 0.012f, 0.25f, Color.white, 0.3f);
            _sparks = ChestFxFactory.CreateParticles("MinerSparks", null, _sparkStyle, 0.5f, true);

            _rockStyle = Style(ParticleLook.SoftDot, ParticleDirection.Fountain, 1.1f, 0.04f, 1.3f, new Color(0.35f, 0.3f, 0.25f), 0.5f);
            _rocks = ChestFxFactory.CreateParticles("MinerRocks", null, _rockStyle, 0.5f, true);
            ParticleSystem.MainModule rm = _rocks.main;
            rm.gravityModifier = 2.2f;

            _gemStyle = Style(ParticleLook.Sparkle, ParticleDirection.Fountain, 0.7f, 0.03f, 1.1f, Color.white, 0.35f);
            _gems = ChestFxFactory.CreateParticles("MinerGems", null, _gemStyle, 0.5f, true);
            ParticleSystem.MainModule gm = _gems.main;
            gm.gravityModifier = 0.9f;

            foreach (ParticleSystem ps in new[] { _sparks, _rocks, _gems }) ps.Play();
        }

        private static ChestFxProfile Style(ParticleLook look, ParticleDirection dir, float speed, float size, float life, Color secondary, float mix)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.secondaryColor = secondary;
            s.secondaryMix = mix;
            return s;
        }

        private void Cleanup()
        {
            foreach (ParticleSystem ps in new[] { _sparks, _rocks, _gems })
            {
                if (ps == null) continue;
                Destroy(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
                Destroy(ps.gameObject);
            }
            foreach (ChestFxProfile s in new[] { _sparkStyle, _rockStyle, _gemStyle }) if (s != null) Destroy(s);
            if (_lamp != null) Destroy(_lamp.gameObject);
            _sparks = _rocks = _gems = null;
            _sparkStyle = _rockStyle = _gemStyle = null;
            _lamp = null;
            _lampOn = false;
            _blast = 0f;
        }
    }
}

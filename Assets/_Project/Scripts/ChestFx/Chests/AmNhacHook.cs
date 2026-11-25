using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_AmNhac extra: a ring of neon equaliser bars round the jukebox. During the roulette the bars jump to
    /// the height of the tier being shown (like a VU meter) in alternating pink/teal; on the lock they snap to the
    /// rolled tier and pulse on a 120 BPM beat while a neon point light blinks. From Rất hiếm up a crowd cheers.
    /// Decoration only: never touches the rolled tier.
    /// </summary>
    public class AmNhacHook : ChestFxHook
    {
        [SerializeField] private int barCount = 8;
        [SerializeField] private float radius = 0.32f;
        [SerializeField] private float barWidth = 0.035f;
        [SerializeField] private float maxBarHeight = 0.22f;
        [SerializeField] private float bpm = 120f;
        [SerializeField] private float lightIntensity = 2.5f;
        [SerializeField] private AudioClip crowdClip;
        [SerializeField, Range(0f, 1f)] private float crowdVolume = 0.55f;
        [SerializeField] private ChestTier crowdMinimumTier = ChestTier.RatHiem;

        private Transform _root;
        private Transform[] _bars;
        private Material[] _mats;
        private float[] _target;
        private float[] _height;
        private Light _light;
        private bool _locked;
        private ChestTier _tier;
        private float _lockTime;
        private int _step;

        public void SetCrowdClip(AudioClip clip) => crowdClip = clip;

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            Build(reveal);
            _root.position = mouth;
            _locked = false;
            _step = 0;
            _root.gameObject.SetActive(true);
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (_bars == null) return;
            _step++;
            float level = 0.25f + 0.75f * shown.Rank01();
            for (int i = 0; i < _bars.Length; i++)
            {
                // Random-looking but deterministic spread so it reads as an equaliser, not a wall.
                float jitter = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin((i + 1) * 12.9898f + _step * 4.1414f));
                _target[i] = level * jitter;
                SetBarColor(i, (i + _step) % 2 == 0 ? AmNhacFxPreset.NeonPink : AmNhacFxPreset.NeonTeal, color);
            }
            if (_light != null) { _light.color = _step % 2 == 0 ? AmNhacFxPreset.NeonPink : AmNhacFxPreset.NeonTeal; _light.intensity = lightIntensity * 0.6f; }
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_bars == null) return;
            _locked = true;
            _tier = tier;
            _lockTime = Time.time;
            for (int i = 0; i < _bars.Length; i++) SetBarColor(i, color, i % 2 == 0 ? AmNhacFxPreset.NeonPink : AmNhacFxPreset.NeonTeal);
            if (tier >= crowdMinimumTier) reveal.PlayOneShot(crowdClip, crowdVolume * (tier == ChestTier.HuyenThoai ? 1f : 0.7f));
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Update()
        {
            if (_bars == null || !_root.gameObject.activeSelf) return;
            if (_locked)
            {
                float beat = (Time.time - _lockTime) * bpm / 60f;
                float pulse = Cube(1f - (beat - (float)System.Math.Floor(beat)));   // sharp hit, decay
                // Rác: bars sag and die out (the music stopped). Higher tiers: everything bounces harder.
                float level = _tier == ChestTier.Rac
                    ? Mathf.Max(0.03f, 0.3f - (Time.time - _lockTime) * 0.25f)
                    : 0.35f + 0.65f * _tier.Rank01();
                for (int i = 0; i < _bars.Length; i++)
                {
                    float wave = 0.6f + 0.4f * Mathf.Sin(beat * Mathf.PI + i * 0.8f);
                    _target[i] = _tier == ChestTier.Rac ? level : level * (0.5f * wave + 0.5f * pulse);
                }
                if (_light != null)
                {
                    _light.color = ((int)System.Math.Floor(beat)) % 2 == 0 ? AmNhacFxPreset.NeonPink : AmNhacFxPreset.NeonTeal;
                    _light.intensity = _tier == ChestTier.Rac ? 0f : lightIntensity * (0.3f + 0.7f * pulse) * (0.5f + _tier.Rank01());
                }
            }
            for (int i = 0; i < _bars.Length; i++)
            {
                _height[i] = Mathf.Lerp(_height[i], _target[i], 1f - Mathf.Exp(-25f * Time.deltaTime));
                float h = Mathf.Max(0.005f, _height[i] * maxBarHeight);
                Transform b = _bars[i];
                b.localScale = new Vector3(barWidth, h, barWidth);
                Vector3 lp = b.localPosition;
                lp.y = h * 0.5f;
                b.localPosition = lp;
            }
            if (!_locked && _light != null) _light.intensity = Mathf.Lerp(_light.intensity, 0f, 1f - Mathf.Exp(-6f * Time.deltaTime));
        }

        private void Build(ChestRevealSequence reveal)
        {
            if (_root != null) return;
            _root = new GameObject("AmNhacEqualizer").transform;
            _root.SetParent(reveal.transform, false);
            _root.rotation = reveal.transform.rotation;
            _bars = new Transform[barCount];
            _mats = new Material[barCount];
            _target = new float[barCount];
            _height = new float[barCount];
            for (int i = 0; i < barCount; i++)
            {
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = "Bar" + i;
                Collider col = bar.GetComponent<Collider>();
                if (col != null) Destroy(col);
                float a = i * Mathf.PI * 2f / barCount;
                bar.transform.SetParent(_root, false);
                bar.transform.localPosition = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                bar.transform.localScale = new Vector3(barWidth, 0.005f, barWidth);
                _mats[i] = ChestFxFactory.CreateUnlit(AmNhacFxPreset.NeonPink, false, "AmNhacBar");
                bar.GetComponent<Renderer>().sharedMaterial = _mats[i];
                _bars[i] = bar.transform;
            }
            GameObject lightGo = new GameObject("AmNhacNeonLight");
            lightGo.transform.SetParent(_root, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            _light = lightGo.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.range = 1.5f;
            _light.intensity = 0f;
        }

        private static float Cube(float x) => x * x * x;

        private void SetBarColor(int i, Color a, Color b) => ChestFxFactory.SetColor(_mats[i], Color.Lerp(a, b, 0.35f) * 1.4f);

        private void Cleanup()
        {
            if (_mats != null) foreach (Material m in _mats) if (m != null) Destroy(m);
            if (_root != null) Destroy(_root.gameObject);
            _root = null; _bars = null; _mats = null; _light = null; _locked = false;
        }
    }
}

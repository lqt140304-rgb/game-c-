using UnityEngine;
using ChoDoLa.Interaction;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Mimic's own extras. Put it on the chest rig (next to ChestRevealSequence); MimicFxSetup does that.
    /// - Bite: listens to BoxOpenSequence.onBite (the bite intro) and plays the chomp + a teeth-chatter tail.
    /// - Heartbeat: every roulette step the whole chest swells a little and relaxes, like a beating heart.
    /// - Drool: a slime drip falls from the lip now and then during the roulette (more often as it slows).
    /// - Lock: a chomp pulse; Rác "vomits" a heap of green slime, Huyền thoại swells hugely before the roar.
    /// Only decorates: never changes the rolled tier.
    /// </summary>
    public class MimicHook : ChestFxHook
    {
        public AudioClip biteClip;
        public AudioClip chatterClip;
        public AudioClip droolClip;
        [Range(0f, 1f)] public float biteVolume = 1f;
        [Range(0f, 1f)] public float droolVolume = 0.35f;

        [SerializeField] private float heartbeatScale = 0.035f;
        [SerializeField] private float heartbeatDecay = 9f;
        [SerializeField, Range(0f, 1f)] private float dripChancePerStep = 0.25f;
        [SerializeField] private int vomitBlobs = 90;
        [SerializeField] private Color slime = new Color(0.62f, 0.95f, 0.25f);

        private BoxOpenSequence _box;
        private AudioSource _audio;
        private Vector3 _baseScale;
        private bool _hasBase;
        private float _pulse;
        private ParticleSystem _drip;
        private ParticleSystem _vomit;
        private ChestFxProfile _dripStyle;
        private ChestFxProfile _vomitStyle;
        private readonly System.Random _rng = new System.Random();

        private void Awake()
        {
            _box = GetComponent<BoxOpenSequence>();
            _audio = GetComponent<AudioSource>();
            if (_box != null) _box.onBite.AddListener(HandleBite);
        }

        private void OnDestroy()
        {
            if (_box != null) _box.onBite.RemoveListener(HandleBite);
            Cleanup();
        }

        private void HandleBite()
        {
            Play(biteClip, biteVolume, 1f);
            Play(chatterClip, biteVolume * 0.35f, 1.15f);
            Pulse(heartbeatScale * 2.5f);
        }

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            CaptureScale();
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            Pulse(heartbeatScale);
            if (_rng.NextDouble() < dripChancePerStep)
            {
                EnsureDrip();
                _drip.transform.position = reveal.MouthPosition;
                ChestFxFactory.SetParticleColor(_drip, Color.Lerp(color, slime, 0.75f), _dripStyle, 1f);
                _drip.Play();
                _drip.Emit(1 + _rng.Next(2));
                if (_rng.NextDouble() < 0.3) Play(droolClip, droolVolume, 0.85f + (float)_rng.NextDouble() * 0.3f);
            }
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            Pulse(heartbeatScale * (2f + 3f * tier.Rank01()));   // the roar swells the whole chest
            if (tier == ChestTier.Rac)
            {
                EnsureVomit();
                _vomit.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
                ChestFxFactory.SetParticleColor(_vomit, slime, _vomitStyle, 1f);
                _vomit.Play();
                _vomit.Emit(vomitBlobs);
            }
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            _pulse = 0f;
            if (_hasBase) transform.localScale = _baseScale;
            Cleanup();
        }

        private void Update()
        {
            if (!_hasBase) return;
            // instant attack, exponential release: a thump, not a wobble
            _pulse = Mathf.Max(0f, _pulse - _pulse * heartbeatDecay * Time.deltaTime);
            transform.localScale = _baseScale * (1f + _pulse);
        }

        private void CaptureScale()
        {
            if (_hasBase) return;
            _baseScale = transform.localScale;
            _hasBase = true;
        }

        private void Pulse(float amount)
        {
            CaptureScale();
            _pulse = Mathf.Max(_pulse, amount);
        }

        private void Play(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || _audio == null) return;
            float old = _audio.pitch;
            _audio.pitch = pitch;
            _audio.PlayOneShot(clip, volume);
            _audio.pitch = old;   // PlayOneShot latches the pitch at call time
        }

        private void EnsureDrip()
        {
            if (_drip != null) return;
            _dripStyle = Style(ParticleDirection.Up, 0.01f, 0.03f, 1.4f, 0.2f);
            _drip = ChestFxFactory.CreateParticles("MimicDrool", null, _dripStyle, 0.6f, true);
            ParticleSystem.MainModule main = _drip.main;
            main.gravityModifier = 0.9f;
            ParticleSystem.ShapeModule shape = _drip.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.18f;
            shape.rotation = new Vector3(90f, 0f, 0f);
        }

        private void EnsureVomit()
        {
            if (_vomit != null) return;
            _vomitStyle = Style(ParticleDirection.Fountain, 1.1f, 0.05f, 1.3f, 0.5f);
            _vomitStyle.burstGravity = 2f;
            _vomit = ChestFxFactory.CreateParticles("MimicVomit", null, _vomitStyle, 0.6f, true);
        }

        private ChestFxProfile Style(ParticleDirection dir, float speed, float size, float life, float mix)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = ParticleLook.SoftDot;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.secondaryColor = new Color(0.9f, 1f, 0.6f);
            s.secondaryMix = mix;
            return s;
        }

        private void Cleanup()
        {
            Kill(ref _drip);
            Kill(ref _vomit);
            if (_dripStyle != null) Destroy(_dripStyle);
            if (_vomitStyle != null) Destroy(_vomitStyle);
            _dripStyle = _vomitStyle = null;
        }

        private static void Kill(ref ParticleSystem ps)
        {
            if (ps == null) return;
            Destroy(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            Destroy(ps.gameObject);
            ps = null;
        }
    }
}

using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_RacQuy extras, built in code:
    /// - roulette: a yellow-green stink cloud seeps out of the lid and a handful of black flies orbit the bin
    ///   (gurgle + fly buzz; one rolling can half-way through);
    /// - lock Rác: a big stink puff, extra flies, a cartoon fart (the lock clip already has the sad trombone);
    /// - lock Hiếm and up: the flies scatter (cleared), the stink stops;
    /// - lock Rất hiếm / Huyền thoại: a column of white-gold sparkles shoots straight up ("gold in the garbage"),
    ///   bigger and longer for Huyền thoại, under the holy choir lock sound.
    /// Decoration only: never touches the rolled tier. Clips are assigned by Editor/RacQuyFxSetup.cs (on the
    /// customFxPrefab); empty clips are skipped.
    /// </summary>
    public class RacQuyHook : ChestFxHook
    {
        [Header("Sounds (filled by Tools/Cho Do La/Create Rac Quy Chest FX Profile)")]
        public AudioClip gurgleClip;
        public AudioClip fliesClip;
        public AudioClip canRollClip;
        public AudioClip fartClip;
        [Range(0f, 1f)] public float fxVolume = 0.55f;

        [Header("Look")]
        [SerializeField] private Color stinkColor = new Color(0.55f, 0.72f, 0.12f);
        [SerializeField] private Color flyColor = new Color(0.04f, 0.04f, 0.03f);
        [SerializeField] private Color holyColor = new Color(1f, 0.93f, 0.62f);
        [SerializeField] private int flies = 7;
        [SerializeField] private float radius = 0.22f;
        [Tooltip("Roulette step on which the can rolls.")]
        [SerializeField] private int canRollStep = 7;

        private ChestFxProfile _stinkStyle, _flyStyle, _holyStyle;
        private ParticleSystem _stink, _flies, _holy;
        private Material _flyMat;
        private int _steps;

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            Build();
            _steps = 0;
            Place(reveal, mouth);

            ParticleSystem.EmissionModule em = _stink.emission;
            em.rateOverTime = 10f;
            ChestFxFactory.SetParticleColor(_stink, stinkColor, _stinkStyle, 0.8f);
            _stink.Play();

            _flies.Play();
            _flies.Emit(flies);

            reveal.PlayOneShot(gurgleClip, fxVolume);
            reveal.PlayOneShot(fliesClip, fxVolume * 0.7f);
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            _steps++;
            if (_steps == canRollStep) reveal.PlayOneShot(canRollClip, fxVolume * 0.6f, 1.1f);
            // The stink takes a little of the colour being shown, so the cloud flickers with the roulette.
            if (_stink != null) ChestFxFactory.SetParticleColor(_stink, Color.Lerp(stinkColor, color, 0.3f), _stinkStyle, 0.8f);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_stink == null) return;
            Place(reveal, reveal.MouthPosition);
            ParticleSystem.EmissionModule em = _stink.emission;
            em.rateOverTime = 0f;

            if (tier == ChestTier.Rac)
            {
                ChestFxFactory.SetParticleColor(_stink, stinkColor, _stinkStyle, 1f);
                _stink.Emit(45);
                _flies.Emit(flies);
                reveal.PlayOneShot(fartClip, fxVolume, 0.95f);
                return;
            }
            if (tier >= ChestTier.Hiem) _flies.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (tier >= ChestTier.RatHiem)
            {
                bool legend = tier == ChestTier.HuyenThoai;
                ChestFxFactory.SetParticleColor(_holy, Color.Lerp(holyColor, color, 0.25f), _holyStyle, legend ? 1.6f : 1.1f);
                ParticleSystem.EmissionModule hem = _holy.emission;
                hem.rateOverTime = legend ? 90f : 40f;
                _holy.Play();
                _holy.Emit(legend ? 70 : 30);
                StartCoroutine(StopHolyAfter(legend ? 2.6f : 1.2f));
            }
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private System.Collections.IEnumerator StopHolyAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (_holy != null)
            {
                ParticleSystem.EmissionModule hem = _holy.emission;
                hem.rateOverTime = 0f;
            }
        }

        private void Place(ChestRevealSequence reveal, Vector3 mouth)
        {
            Quaternion r = reveal.transform.rotation;
            _stink.transform.SetPositionAndRotation(mouth, r);
            _flies.transform.SetPositionAndRotation(mouth + Vector3.up * 0.05f, r);
            _holy.transform.SetPositionAndRotation(mouth, r);
        }

        private void Build()
        {
            if (_stink != null) return;

            // Stink: fat slow puffs, wide wobble.
            _stinkStyle = Style(ParticleLook.SoftDot, ParticleDirection.Swirl, 0.12f, 0.09f, 2.4f, stinkColor, 0.5f);
            _stink = ChestFxFactory.CreateParticles("RacQuy_Stink", null, _stinkStyle, radius * 4f, false);

            // Flies: tiny opaque black dots on a long orbit (an additive material would make black invisible).
            _flyStyle = Style(ParticleLook.SoftDot, ParticleDirection.Swirl, 0.02f, 0.012f, 3.4f, flyColor, 0f);
            _flies = ChestFxFactory.CreateParticles("RacQuy_Flies", null, _flyStyle, radius * 4f, true);
            ParticleSystem.MainModule fm = _flies.main;
            fm.gravityModifier = 0f;
            ParticleSystem.ShapeModule fs = _flies.shape;
            fs.shapeType = ParticleSystemShapeType.Circle;
            fs.radius = radius;
            fs.rotation = new Vector3(90f, 0f, 0f);
            ParticleSystem.VelocityOverLifetimeModule fv = _flies.velocityOverLifetime;
            fv.enabled = true;
            fv.space = ParticleSystemSimulationSpace.Local;
            fv.x = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
            fv.y = new ParticleSystem.MinMaxCurve(-0.04f, 0.06f);
            fv.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
            fv.orbitalY = new ParticleSystem.MinMaxCurve(3.5f, 6f);
            fv.radial = new ParticleSystem.MinMaxCurve(0f);
            ParticleSystemRenderer fr = _flies.GetComponent<ParticleSystemRenderer>();
            Object.Destroy(fr.sharedMaterial);
            _flyMat = ChestFxFactory.CreateUnlit(flyColor, false, "RacQuy_Fly_Mat");
            fr.sharedMaterial = _flyMat;

            // Holy column: sparkles shooting straight up in a narrow cone.
            _holyStyle = Style(ParticleLook.Sparkle, ParticleDirection.Up, 1.3f, 0.03f, 1.3f, Color.white, 0.6f);
            _holy = ChestFxFactory.CreateParticles("RacQuy_Holy", null, _holyStyle, radius * 4f, false);
            ParticleSystem.ShapeModule hs = _holy.shape;
            hs.angle = 4f;
            hs.radius = radius * 0.35f;
            ParticleSystem.EmissionModule hem = _holy.emission;
            hem.rateOverTime = 0f;
        }

        private static ChestFxProfile Style(ParticleLook look, ParticleDirection dir, float speed, float size, float life, Color second, float mix)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.secondaryColor = second;
            s.secondaryMix = mix;
            return s;
        }

        private void Cleanup()
        {
            StopAllCoroutines();
            Kill(ref _stink);
            Kill(ref _flies);
            Kill(ref _holy);
            if (_flyMat != null) Destroy(_flyMat);
            if (_stinkStyle != null) Destroy(_stinkStyle);
            if (_flyStyle != null) Destroy(_flyStyle);
            if (_holyStyle != null) Destroy(_holyStyle);
            _flyMat = null;
            _stinkStyle = _flyStyle = _holyStyle = null;
        }

        private void Kill(ref ParticleSystem ps)
        {
            if (ps == null) return;
            Material m = ps.GetComponent<ParticleSystemRenderer>().sharedMaterial;
            if (m != null && m != _flyMat) Destroy(m);
            Destroy(ps.gameObject);
            ps = null;
        }
    }
}

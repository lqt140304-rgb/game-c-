using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Magic extra effect.
    /// - Every roulette step: a small "resonance ring" of crystal sparkles pops round the chest rim in the colour
    ///   shown (the chest rings like a glass bell with each tap).
    /// - Lock: Rác = the spell fizzles (a few dull sparks drop to the floor); Thường+ = a flat rune circle of
    ///   sparkles spreads out from the chest; Rất hiếm+ also a second ring a moment later (echo), Huyền thoại a
    ///   column of sparks rising out of the mouth.
    /// Decoration only, never changes the tier.
    /// </summary>
    public class MagicHook : ChestFxHook
    {
        [SerializeField] private float rimRadius = 0.22f;
        [SerializeField] private int sparksPerStep = 10;
        [SerializeField] private int runeSparks = 70;
        [SerializeField] private float echoDelay = 0.35f;

        private ParticleSystem _rim, _rune, _fizzle, _column;
        private ChestFxProfile _rimStyle, _runeStyle, _fizzleStyle, _columnStyle;
        private Color _echoColor;
        private int _echoSparks;
        private float _echoAt = -1f;

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            if (_rim == null)
            {
                _rimStyle = Style(ParticleLook.Sparkle, ParticleDirection.Radial, 0.12f, 0.02f, 0.5f, 0f);
                _rim = Ring("MagicResonance", _rimStyle, rimRadius);
            }
            Place(_rim, reveal);
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (_rim == null) OnRouletteStart(reveal, reveal.MouthPosition);
            ChestFxFactory.SetParticleColor(_rim, color, _rimStyle, 1.2f);
            _rim.Play();
            _rim.Emit(sparksPerStep + shown.Rank());
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (tier == ChestTier.Rac)
            {
                if (_fizzle == null)
                {
                    _fizzleStyle = Style(ParticleLook.SoftDot, ParticleDirection.Radial, 0.25f, 0.018f, 1.1f, 1.4f);
                    _fizzle = ChestFxFactory.CreateParticles("MagicFizzle", null, _fizzleStyle, 0.5f, true);
                }
                Place(_fizzle, reveal);
                ChestFxFactory.SetParticleColor(_fizzle, color, _fizzleStyle, 0.6f);
                _fizzle.Play();
                _fizzle.Emit(14);
                return;
            }

            if (_rune == null)
            {
                _runeStyle = Style(ParticleLook.Sparkle, ParticleDirection.Radial, 0.6f, 0.03f, 1.2f, 0f);
                _rune = Ring("MagicRuneCircle", _runeStyle, 0.05f);
            }
            Place(_rune, reveal);
            ChestFxFactory.SetParticleColor(_rune, color, _runeStyle, 1.5f);
            _rune.Play();
            int count = Mathf.RoundToInt(runeSparks * (0.6f + tier.Rank01()));
            _rune.Emit(count);

            if (tier >= ChestTier.RatHiem)
            {
                _echoColor = color;
                _echoSparks = count / 2;
                _echoAt = Time.time + echoDelay;
            }

            if (tier == ChestTier.HuyenThoai)
            {
                if (_column == null)
                {
                    _columnStyle = Style(ParticleLook.Streak, ParticleDirection.Up, 1.1f, 0.025f, 1.3f, -0.1f);
                    _column = ChestFxFactory.CreateParticles("MagicColumn", null, _columnStyle, 0.3f, true);
                }
                Place(_column, reveal);
                ChestFxFactory.SetParticleColor(_column, color, _columnStyle, 2f);
                _column.Play();
                _column.Emit(90);
            }
        }

        private void Update()
        {
            if (_echoAt < 0f || Time.time < _echoAt || _rune == null) return;
            _echoAt = -1f;
            ChestFxFactory.SetParticleColor(_rune, Color.Lerp(_echoColor, Color.white, 0.35f), _runeStyle, 1.2f);
            _rune.Emit(_echoSparks);
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        // ------------------------------------------------------------------ Helpers

        private static ChestFxProfile Style(ParticleLook look, ParticleDirection dir, float speed, float size, float life, float gravity)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.burstGravity = gravity;
            s.secondaryColor = new Color(0.85f, 0.7f, 1f);
            s.secondaryMix = 0.45f;
            return s;
        }

        /// <summary>Burst system emitting from a flat circle (sparks fly outward in the horizontal plane).</summary>
        private static ParticleSystem Ring(string name, ChestFxProfile style, float radius)
        {
            ParticleSystem ps = ChestFxFactory.CreateParticles(name, null, style, radius * 4f, true);
            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            shape.rotation = new Vector3(90f, 0f, 0f);
            return ps;
        }

        private static void Place(ParticleSystem ps, ChestRevealSequence reveal)
        {
            ps.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
        }

        private void Cleanup()
        {
            _echoAt = -1f;
            Kill(ref _rim, ref _rimStyle);
            Kill(ref _rune, ref _runeStyle);
            Kill(ref _fizzle, ref _fizzleStyle);
            Kill(ref _column, ref _columnStyle);
        }

        private static void Kill(ref ParticleSystem ps, ref ChestFxProfile style)
        {
            if (ps != null)
            {
                Destroy(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
                Destroy(ps.gameObject);
            }
            if (style != null) Destroy(style);
            ps = null;
            style = null;
        }
    }
}

using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TrungHoa extra: a tiny firecracker pop (red streaks) on every roulette tick, a firecracker burst at
    /// the lock (bigger with the tier), and for Rất hiếm / Huyền thoại red sky lanterns rising slowly round the
    /// cauldron. Put it on the chest (TrungHoaFxSetup menu does it) or on the profile's customFxPrefab.
    /// Decoration only: never changes the rolled tier.
    /// </summary>
    public class TrungHoaHook : ChestFxHook
    {
        [SerializeField] private Color firecrackerRed = new Color(1f, 0.18f, 0.1f);
        [SerializeField] private Color gold = new Color(1f, 0.8f, 0.3f);
        [SerializeField] private Color lanternRed = new Color(1f, 0.35f, 0.15f);
        [SerializeField] private int popsPerTick = 3;
        [SerializeField] private int burstBase = 25;
        [SerializeField] private ChestTier lanternMinimumTier = ChestTier.RatHiem;
        [SerializeField] private int lanterns = 8;
        [SerializeField] private float lanternRadius = 0.35f;

        private ParticleSystem _pops, _lanterns;
        private ChestFxProfile _popStyle, _lanternStyle;

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            EnsurePops();
            _pops.transform.position = reveal.MouthPosition;
            ChestFxFactory.SetParticleColor(_pops, firecrackerRed, _popStyle, 1f);
            _pops.Play();
            _pops.Emit(popsPerTick);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (tier == ChestTier.Rac) return; // the abacus sound is the whole joke
            EnsurePops();
            _pops.transform.position = reveal.MouthPosition;
            ChestFxFactory.SetParticleColor(_pops, Color.Lerp(firecrackerRed, color, 0.3f), _popStyle, 1.3f);
            _pops.Play();
            _pops.Emit(Mathf.RoundToInt(burstBase * (1f + 2f * tier.Rank01())));

            if (tier < lanternMinimumTier) return;
            EnsureLanterns();
            _lanterns.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
            ChestFxFactory.SetParticleColor(_lanterns, lanternRed, _lanternStyle, 1f);
            _lanterns.Play();
            _lanterns.Emit(tier == ChestTier.HuyenThoai ? lanterns * 2 : lanterns);
        }

        private void EnsurePops()
        {
            if (_pops != null) return;
            _popStyle = Style(ParticleLook.Streak, ParticleDirection.Radial, 0.9f, 0.018f, 0.45f, 1.2f);
            _popStyle.secondaryColor = gold;
            _popStyle.secondaryMix = 0.5f;
            _pops = ChestFxFactory.CreateParticles("TrungHoaFirecracker", null, _popStyle, 0.4f, true);
        }

        private void EnsureLanterns()
        {
            if (_lanterns != null) return;
            _lanternStyle = Style(ParticleLook.SoftDot, ParticleDirection.Up, 0.12f, 0.06f, 3.5f, -0.04f);
            _lanternStyle.secondaryColor = gold;
            _lanternStyle.secondaryMix = 0.25f;
            _lanterns = ChestFxFactory.CreateParticles("TrungHoaLanterns", null, _lanternStyle, lanternRadius * 4f, true);
            ParticleSystem.ShapeModule shape = _lanterns.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = lanternRadius;
            shape.rotation = new Vector3(90f, 0f, 0f);
        }

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
            return s;
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            Kill(ref _pops, ref _popStyle);
            Kill(ref _lanterns, ref _lanternStyle);
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

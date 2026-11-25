using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_HaiTac extras (decoration only, never changes the tier):
    /// - every roulette tick: a small splash of spray in the shown colour (waves slapping the hull);
    /// - lock: a geyser of gold coins out of the open skull, more per tier; Hiếm+ a distant cannon (Rất hiếm: two,
    ///   Huyền thoại: three); Huyền thoại also a "tsunami" ring of sea foam that rushes outward;
    /// - Rác: a seagull squawks at you once the box is open.
    /// Clips are filled by Tools/Cho Do La/Create Hai Tac Chest FX Profile (or drag them in).
    /// </summary>
    public class HaiTacHook : ChestFxHook
    {
        [SerializeField] private AudioClip cannonClip;
        [SerializeField] private AudioClip coinsClip;
        [SerializeField] private AudioClip seagullClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
        [SerializeField] private Color coinColor = new Color(1f, 0.82f, 0.3f);
        [SerializeField] private int coinsBase = 12;
        [SerializeField] private int coinsPerTier = 14;
        [SerializeField] private int splashPerTick = 4;
        [SerializeField] private int tsunamiDroplets = 160;

        private ParticleSystem _spray, _coins, _tsunami;
        private ChestFxProfile _sprayStyle, _coinStyle, _tsunamiStyle;
        private readonly System.Random _rng = new System.Random();

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            if (_spray == null)
            {
                _sprayStyle = Style(ParticleLook.SoftDot, ParticleDirection.Radial, 0.35f, 0.02f, 0.6f, HaiTacFxPreset.SeaFoam, 0.6f);
                _spray = ChestFxFactory.CreateParticles("HaiTacSpray", null, _sprayStyle, 1.2f, true);
                ParticleSystem.ShapeModule shape = _spray.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.16f;
                shape.rotation = new Vector3(90f, 0f, 0f);
            }
            _spray.transform.SetPositionAndRotation(mouth, reveal.transform.rotation);
            _spray.Play();
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (_spray == null) return;
            ChestFxFactory.SetParticleColor(_spray, color, _sprayStyle, 0.8f);
            _spray.Emit(splashPerTick);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_coins == null)
            {
                _coinStyle = Style(ParticleLook.Coin, ParticleDirection.Fountain, 0.9f, 0.032f, 1.6f, Color.white, 0.2f);
                _coinStyle.burstGravity = 1f;
                _coins = ChestFxFactory.CreateParticles("HaiTacCoins", null, _coinStyle, 1.2f, true);
            }
            _coins.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
            ChestFxFactory.SetParticleColor(_coins, coinColor, _coinStyle, 1f);
            _coins.Play();
            _coins.Emit(coinsBase + coinsPerTier * tier.Rank());
            if (tier >= ChestTier.Thuong) reveal.PlayOneShot(coinsClip, volume * 0.8f);

            if (tier >= ChestTier.Hiem)
            {
                int shots = tier == ChestTier.HuyenThoai ? 3 : tier == ChestTier.RatHiem ? 2 : 1;
                StartCoroutine(Cannons(reveal, shots));
            }
            if (tier == ChestTier.HuyenThoai) Tsunami(reveal, color);
        }

        public override void OnRevealed(ChestRevealSequence reveal)
        {
            if (reveal.RolledTier == ChestTier.Rac) reveal.PlayOneShot(seagullClip, volume, 1.1f);
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            StopAllCoroutines();
            Cleanup();
        }

        private void OnDestroy() => Cleanup();

        private IEnumerator Cannons(ChestRevealSequence reveal, int shots)
        {
            for (int i = 0; i < shots; i++)
            {
                yield return new WaitForSeconds(0.35f + 0.45f * i);
                // Each shot a bit lower and further away, like a broadside rolling past.
                reveal.PlayOneShot(cannonClip, volume * (1f - 0.18f * i), 1f - 0.07f * i - 0.04f * (float)_rng.NextDouble());
            }
        }

        private void Tsunami(ChestRevealSequence reveal, Color tierColor)
        {
            if (_tsunami == null)
            {
                _tsunamiStyle = Style(ParticleLook.Streak, ParticleDirection.Radial, 1.6f, 0.03f, 1.1f, HaiTacFxPreset.SeaFoam, 0.7f);
                _tsunamiStyle.burstGravity = 0.6f;
                _tsunami = ChestFxFactory.CreateParticles("HaiTacTsunami", null, _tsunamiStyle, 2.5f, true);
                ParticleSystem.ShapeModule shape = _tsunami.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;   // a flat ring rushing outward
                shape.radius = 0.25f;
                shape.rotation = new Vector3(90f, 0f, 0f);
            }
            Vector3 basePos = reveal.transform.position + reveal.transform.up * 0.03f;
            _tsunami.transform.SetPositionAndRotation(basePos, reveal.transform.rotation);
            ChestFxFactory.SetParticleColor(_tsunami, tierColor, _tsunamiStyle, 1.2f);
            _tsunami.Play();
            _tsunami.Emit(tsunamiDroplets);
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
            Kill(ref _spray, ref _sprayStyle);
            Kill(ref _coins, ref _coinStyle);
            Kill(ref _tsunami, ref _tsunamiStyle);
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

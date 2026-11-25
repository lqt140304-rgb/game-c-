using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TheThao extra: kickoff whistle when the roulette starts; from Hiếm up, two confetti cannons fire from
    /// the left and right of the chest (like the stands); Huyền thoại adds an air horn and three staggered firework bursts.
    /// Put it on the chest object (TheThaoFxSetup adds it to the selected object and fills the clips).
    /// </summary>
    public class TheThaoHook : ChestFxHook
    {
        public AudioClip kickoffClip;
        public AudioClip airHornClip;
        [Range(0f, 1f)] public float extraVolume = 0.6f;
        [SerializeField] private ChestTier cannonTier = ChestTier.Hiem;
        [SerializeField] private float cannonOffset = 0.35f;
        [SerializeField] private int cannonCount = 35;
        [SerializeField] private Color[] confettiColors =
        {
            new Color(1f, 0.55f, 0.1f), new Color(0.12f, 0.2f, 0.45f), new Color(1f, 0.85f, 0.2f), Color.white,
        };

        private ParticleSystem _cannon, _firework;
        private ChestFxProfile _cannonStyle, _fireStyle;

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            if (kickoffClip != null) reveal.PlayOneShot(kickoffClip, extraVolume);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (tier < cannonTier) return;
            Build();
            Vector3 side = reveal.transform.right * cannonOffset;
            FireCannon(reveal.MouthPosition - side, Quaternion.Euler(0f, 0f, -25f), tier);
            FireCannon(reveal.MouthPosition + side, Quaternion.Euler(0f, 0f, 25f), tier);
            if (tier == ChestTier.HuyenThoai)
            {
                if (airHornClip != null) reveal.PlayOneShot(airHornClip, extraVolume);
                StartCoroutine(Fireworks(reveal, color));
            }
        }

        private void FireCannon(Vector3 at, Quaternion tilt, ChestTier tier)
        {
            _cannon.transform.SetPositionAndRotation(at, transform.rotation * tilt);
            Color c = confettiColors[Random.Range(0, confettiColors.Length)];
            ChestFxFactory.SetParticleColor(_cannon, c, _cannonStyle, 1f);
            _cannon.Play();
            _cannon.Emit(Mathf.RoundToInt(cannonCount * (1f + tier.Rank01())));
        }

        private IEnumerator Fireworks(ChestRevealSequence reveal, Color tierColor)
        {
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(0.35f);
                if (_firework == null) yield break;
                Vector3 at = reveal.MouthPosition + reveal.transform.up * (0.6f + 0.15f * i)
                             + reveal.transform.right * ((i - 1) * 0.3f);
                _firework.transform.SetPositionAndRotation(at, Quaternion.identity);
                ChestFxFactory.SetParticleColor(_firework, i == 1 ? Color.white : tierColor, _fireStyle, 1.5f);
                _firework.Play();
                _firework.Emit(60);
            }
        }

        private void Build()
        {
            if (_cannon != null) return;
            _cannonStyle = Style(ParticleLook.Streak, ParticleDirection.Up, 1.6f, 0.02f, 1.5f, 0.9f);
            _cannon = ChestFxFactory.CreateParticles("TheThaoCannon", null, _cannonStyle, 1f, true);
            ParticleSystem.ShapeModule shape = _cannon.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.03f;

            _fireStyle = Style(ParticleLook.Sparkle, ParticleDirection.Radial, 0.9f, 0.03f, 1.1f, 0.25f);
            _firework = ChestFxFactory.CreateParticles("TheThaoFirework", null, _fireStyle, 1f, true);
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
            s.secondaryColor = Color.white;
            s.secondaryMix = 0.35f;
            return s;
        }

        public override void OnReset(ChestRevealSequence reveal) { StopAllCoroutines(); Cleanup(); }

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            Kill(ref _cannon);
            Kill(ref _firework);
            if (_cannonStyle != null) Destroy(_cannonStyle);
            if (_fireStyle != null) Destroy(_fireStyle);
            _cannonStyle = _fireStyle = null;
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

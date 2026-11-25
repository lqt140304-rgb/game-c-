using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Example of a chest-specific extra (Chest_Golden): when the tier locks on Hiếm or higher, gold sparkles
    /// twinkle around the chest rim for a moment. Shows how a later session plugs a custom effect into the core:
    /// subclass ChestFxHook and put it on the chest (or on the profile's customFxPrefab).
    /// </summary>
    public class GoldenGlintHook : ChestFxHook
    {
        [SerializeField] private ChestTier minimumTier = ChestTier.Hiem;
        [SerializeField] private Color sparkleColor = new Color(1f, 0.85f, 0.4f);
        [SerializeField] private int sparkles = 30;
        [SerializeField] private float radius = 0.3f;

        private ParticleSystem _ps;
        private ChestFxProfile _style;

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (tier < minimumTier) return;
            if (_ps == null)
            {
                _style = ScriptableObject.CreateInstance<ChestFxProfile>();
                _style.hideFlags = HideFlags.DontSave;
                _style.particleLook = ParticleLook.Sparkle;
                _style.particleDirection = ParticleDirection.Radial;
                _style.particleSpeed = 0.05f;
                _style.particleSize = 0.035f;
                _style.particleLifetime = 0.9f;
                _style.secondaryColor = Color.white;
                _style.secondaryMix = 0.4f;
                _ps = ChestFxFactory.CreateParticles("GoldenGlint", null, _style, radius * 4f, true);
                ParticleSystem.ShapeModule shape = _ps.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = radius;
                shape.rotation = new Vector3(90f, 0f, 0f);
            }
            _ps.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
            ChestFxFactory.SetParticleColor(_ps, sparkleColor, _style, 1f);
            _ps.Play();
            _ps.Emit(Mathf.RoundToInt(sparkles * (1f + tier.Rank01())));
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            if (_ps != null)
            {
                Destroy(_ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
                Destroy(_ps.gameObject);
            }
            if (_style != null) Destroy(_style);
            _ps = null;
            _style = null;
        }
    }
}

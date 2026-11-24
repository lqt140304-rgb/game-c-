using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Game extra: square, non-rotating "pixel" particles. Every roulette step pops (rank + 1) pixels of the
    /// shown colour straight up, like a score counter. On the lock: Rác drops a few grey pixels that fall
    /// ("game over"), higher tiers explode into a pixel burst; Huyền thoại fires a second wave 0.25 s later.
    /// Put it on the chest root (Tools/Cho Do La/Add Game Pixel Hook To Selected) or on the profile's customFxPrefab.
    /// </summary>
    public class GamePixelHook : ChestFxHook
    {
        [SerializeField] private float pixelSize = 0.03f;
        [SerializeField] private int burstPerRank = 22;
        [SerializeField] private Color gameOverColor = new Color(0.45f, 0.45f, 0.45f);

        private ParticleSystem _ps;
        private ChestFxProfile _style;
        private Material _material;
        private Color _lockColor;
        private int _secondWave;

        private void Build(ChestRevealSequence reveal)
        {
            if (_ps != null) return;
            _style = ScriptableObject.CreateInstance<ChestFxProfile>();
            _style.hideFlags = HideFlags.DontSave;
            _style.particleDirection = ParticleDirection.Radial;
            _style.particleSpeed = 0.5f;
            _style.particleSize = pixelSize;
            _style.particleLifetime = 0.8f;
            _style.secondaryColor = Color.white;
            _style.secondaryMix = 0.2f;
            _ps = ChestFxFactory.CreateParticles("GamePixels", null, _style, 0.3f, true);

            ParticleSystem.MainModule main = _ps.main;
            main.startRotation = 0f;   // pixels stay axis-aligned
            ParticleSystem.SizeOverLifetimeModule size = _ps.sizeOverLifetime;
            size.enabled = false;      // pixels do not shrink smoothly; they just vanish

            // Plain white texture = a hard-edged square.
            ParticleSystemRenderer r = _ps.GetComponent<ParticleSystemRenderer>();
            if (r.sharedMaterial != null) Destroy(r.sharedMaterial);
            _material = ChestFxFactory.CreateAdditive(Texture2D.whiteTexture, "GamePixel");
            r.sharedMaterial = _material;
        }

        private void Place(ChestRevealSequence reveal, bool upward)
        {
            _ps.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
            ParticleSystem.ShapeModule shape = _ps.shape;
            if (upward)
            {
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 6f;
                shape.radius = 0.04f;
                shape.rotation = new Vector3(-90f, 0f, 0f);
            }
            else
            {
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.05f;
            }
        }

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            Build(reveal);
            _secondWave = 0;
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            Build(reveal);
            Place(reveal, true);
            ParticleSystem.MainModule main = _ps.main;
            main.gravityModifier = 0f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.35f);
            ChestFxFactory.SetParticleColor(_ps, color, _style, 1f);
            _ps.Play();
            _ps.Emit(shown.Rank() + 1);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            Build(reveal);
            Place(reveal, tier == ChestTier.Rac);
            ParticleSystem.MainModule main = _ps.main;
            _ps.Play();
            if (tier == ChestTier.Rac)
            {
                // Game over: a handful of grey pixels hop out and drop.
                main.gravityModifier = 1.2f;
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
                ChestFxFactory.SetParticleColor(_ps, gameOverColor, _style, 0.6f);
                _ps.Emit(10);
                return;
            }
            main.gravityModifier = 0.4f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.3f);
            ChestFxFactory.SetParticleColor(_ps, color, _style, 1f + tier.Rank01());
            _ps.Emit(burstPerRank * tier.Rank());
            if (tier == ChestTier.HuyenThoai)
            {
                _lockColor = color;
                _secondWave = burstPerRank * 3;
                StartCoroutine(SecondWave());
            }
        }

        private IEnumerator SecondWave()
        {
            yield return new WaitForSeconds(0.25f);
            if (_ps == null || _secondWave <= 0) yield break;
            ChestFxFactory.SetParticleColor(_ps, Color.Lerp(_lockColor, Color.white, 0.5f), _style, 2f);
            _ps.Emit(_secondWave);
            _secondWave = 0;
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            StopAllCoroutines();
            Cleanup();
        }

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            if (_ps != null) Destroy(_ps.gameObject);
            if (_material != null) Destroy(_material);
            if (_style != null) Destroy(_style);
            _ps = null;
            _material = null;
            _style = null;
        }
    }
}

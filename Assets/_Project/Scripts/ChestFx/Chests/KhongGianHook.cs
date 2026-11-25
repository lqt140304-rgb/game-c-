using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_KhongGian extras, all built in code:
    /// - roulette: a slow star field around the capsule + a flat sonar ring that pings out from the base on every
    ///   colour step (ring colour = shown tier);
    /// - lock: Rác = "Houston, we have a problem" (grey sparks that fall, radio static); Thường+ = shooting stars
    ///   raining diagonally (count grows with the tier); Hiếm+ = orange retro-rocket puff under the heat shield;
    ///   Huyền thoại = three huge slow sonar echoes ("endless echo").
    /// Decoration only: never touches the rolled tier.
    /// </summary>
    public class KhongGianHook : ChestFxHook
    {
        [Header("Sounds (filled by Tools/Cho Do La/Create Khong Gian Chest FX Profile)")]
        public AudioClip staticClip;
        public AudioClip meteorClip;
        [Range(0f, 1f)] public float staticVolume = 0.6f;
        [Range(0f, 1f)] public float meteorVolume = 0.4f;

        [Header("Look")]
        [SerializeField] private Color starColor = new Color(0.75f, 0.9f, 1f);
        [SerializeField] private Color thrusterColor = new Color(1f, 0.55f, 0.15f);   // heat shield orange
        [SerializeField] private Color junkColor = new Color(0.55f, 0.5f, 0.45f);
        [SerializeField] private float sonarRadius = 0.55f;
        [SerializeField] private float sonarSeconds = 0.6f;
        [SerializeField] private int meteorsPerTier = 6;

        private class Pulse { public GameObject go; public Material mat; public Color color; public float start, duration, from, to; }

        private readonly List<Pulse> _pulses = new List<Pulse>();
        private Mesh _ringMesh;
        private ParticleSystem _stars, _meteors, _thruster, _junk;
        private readonly List<ChestFxProfile> _styles = new List<ChestFxProfile>();
        private ChestFxProfile _starStyle, _meteorStyle, _thrusterStyle, _junkStyle;

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            EnsureSystems(reveal);
            _stars.transform.SetPositionAndRotation(mouth, Quaternion.identity);
            ChestFxFactory.SetParticleColor(_stars, starColor, _starStyle, 1f);
            _stars.Play();
            _stars.Emit(25);   // the sky is already full when the hatch cracks open
            reveal.PlayOneShot(staticClip, staticVolume);
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            SpawnPulse(reveal.transform.position, color, sonarRadius, sonarSeconds, 0.55f);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            EnsureSystems(reveal);
            if (_stars != null) _stars.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            Vector3 mouth = reveal.MouthPosition;
            int rank = tier.Rank();

            if (tier == ChestTier.Rac)
            {
                // Houston, we have a problem: a sad sputter of grey sparks dropping onto the floor.
                _junk.transform.SetPositionAndRotation(mouth, Quaternion.identity);
                ChestFxFactory.SetParticleColor(_junk, junkColor, _junkStyle, 0.7f);
                _junk.Play();
                _junk.Emit(18);
                StartCoroutine(Sputter(reveal));
                return;
            }

            // Shooting stars: start above and to the side, rain diagonally across the capsule.
            _meteors.transform.SetPositionAndRotation(mouth + new Vector3(-0.6f, 1.2f, 0f), Quaternion.Euler(125f, 90f, 0f));
            ChestFxFactory.SetParticleColor(_meteors, color, _meteorStyle, 1f);
            _meteors.Play();
            _meteors.Emit(meteorsPerTier * rank);
            StartCoroutine(MeteorSounds(reveal, rank));

            if (tier >= ChestTier.Hiem)
            {
                // Retro rockets under the heat shield.
                _thruster.transform.SetPositionAndRotation(reveal.transform.position, Quaternion.Euler(90f, 0f, 0f));
                ChestFxFactory.SetParticleColor(_thruster, thrusterColor, _thrusterStyle, 1f);
                _thruster.Play();
                _thruster.Emit(20 + 15 * (rank - 2));
            }

            // Lock ping, then for Huyền thoại the endless echo.
            SpawnPulse(reveal.transform.position, color, sonarRadius * 1.6f, 0.9f, 0.9f);
            if (tier == ChestTier.HuyenThoai) StartCoroutine(Echoes(reveal, color));
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Update()
        {
            float now = Time.time;
            for (int i = _pulses.Count - 1; i >= 0; i--)
            {
                Pulse p = _pulses[i];
                float t = Mathf.Clamp01((now - p.start) / p.duration);
                float r = Mathf.Lerp(p.from, p.to, 1f - (1f - t) * (1f - t));   // ease out
                p.go.transform.localScale = new Vector3(r, 1f, r);
                Color c = p.color;
                c.a = p.color.a * (1f - t);
                ChestFxFactory.SetColor(p.mat, c);
                if (t >= 1f)
                {
                    Destroy(p.mat);
                    Destroy(p.go);
                    _pulses.RemoveAt(i);
                }
            }
        }

        // ------------------------------------------------------------------ Helpers

        private IEnumerator Sputter(ChestRevealSequence reveal)
        {
            // Two crackles a beat apart: the radio tries, fails, tries again.
            yield return new WaitForSeconds(0.35f);
            reveal.PlayOneShot(staticClip, staticVolume * 0.8f, 0.85f);
            SpawnPulse(reveal.transform.position, junkColor, sonarRadius * 0.6f, 0.4f, 0.4f);
            yield return new WaitForSeconds(0.45f);
            reveal.PlayOneShot(staticClip, staticVolume * 0.6f, 0.7f);
        }

        private IEnumerator MeteorSounds(ChestRevealSequence reveal, int rank)
        {
            for (int i = 0; i < rank; i++)
            {
                yield return new WaitForSeconds(0.15f + 0.22f * i);
                reveal.PlayOneShot(meteorClip, meteorVolume, 0.9f + 0.12f * i);
            }
        }

        private IEnumerator Echoes(ChestRevealSequence reveal, Color color)
        {
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(0.45f);
                Color c = color;
                c.a = 0.7f - 0.2f * i;
                SpawnPulse(reveal.transform.position, c, sonarRadius * (2.2f + i), 1.6f + 0.4f * i, 1f);
            }
        }

        private void SpawnPulse(Vector3 position, Color color, float radius, float seconds, float alpha)
        {
            if (_ringMesh == null) _ringMesh = ChestFxFactory.CreateRingMesh(0.85f);
            GameObject go = new GameObject("KhongGian_Sonar") { hideFlags = HideFlags.DontSave };
            go.transform.SetParent(transform, true);
            go.transform.SetPositionAndRotation(position + Vector3.up * 0.01f, Quaternion.identity);
            go.AddComponent<MeshFilter>().sharedMesh = _ringMesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            Material mat = ChestFxFactory.CreateAdditive(null, "KhongGian_Sonar_Mat");
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Color c = color;
            c.a = alpha;
            _pulses.Add(new Pulse { go = go, mat = mat, color = c, start = Time.time, duration = seconds, from = radius * 0.15f, to = radius });
            if (_pulses.Count > 24)
            {
                Destroy(_pulses[0].mat);
                Destroy(_pulses[0].go);
                _pulses.RemoveAt(0);
            }
        }

        private ChestFxProfile Style(ParticleLook look, ParticleDirection dir, float speed, float size, float life, float gravity, Color secondary, float mix)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.burstGravity = gravity;
            s.secondaryColor = secondary;
            s.secondaryMix = mix;
            s.ambientRate = 0f;
            _styles.Add(s);
            return s;
        }

        private void EnsureSystems(ChestRevealSequence reveal)
        {
            if (_stars != null) return;

            // Star field: tiny twinkles drifting very slowly in a big sphere (ambient, emits while the roulette runs).
            _starStyle = Style(ParticleLook.Sparkle, ParticleDirection.Radial, 0.03f, 0.012f, 2.5f, 0f, Color.white, 0.5f);
            _starStyle.ambientRate = 14f;
            _stars = ChestFxFactory.CreateParticles("KhongGian_Stars", transform, _starStyle, 1.6f, false);
            ParticleSystem.ShapeModule s = _stars.shape;
            s.shapeType = ParticleSystemShapeType.Sphere;
            s.radius = 0.7f;

            // Shooting stars: narrow cone, long streaks.
            _meteorStyle = Style(ParticleLook.Streak, ParticleDirection.Up, 1.8f, 0.02f, 0.9f, 0f, Color.white, 0.5f);
            _meteors = ChestFxFactory.CreateParticles("KhongGian_Meteors", transform, _meteorStyle, 1.2f, true);
            ParticleSystem.ShapeModule m = _meteors.shape;
            m.shapeType = ParticleSystemShapeType.Cone;
            m.angle = 12f;
            m.radius = 0.35f;
            m.rotation = Vector3.zero;

            // Retro rockets: soft orange puffs blowing down and out from the base.
            _thrusterStyle = Style(ParticleLook.SoftDot, ParticleDirection.Up, 0.7f, 0.06f, 0.7f, -0.2f, new Color(1f, 0.9f, 0.6f), 0.35f);
            _thruster = ChestFxFactory.CreateParticles("KhongGian_Thruster", transform, _thrusterStyle, 1f, true);
            ParticleSystem.ShapeModule t = _thruster.shape;
            t.shapeType = ParticleSystemShapeType.Cone;
            t.angle = 35f;
            t.radius = 0.15f;
            t.rotation = Vector3.zero;

            // Rác sparks: fall with gravity, dull.
            _junkStyle = Style(ParticleLook.SoftDot, ParticleDirection.Fountain, 0.5f, 0.02f, 1f, 1.2f, new Color(0.3f, 0.3f, 0.3f), 0.5f);
            _junk = ChestFxFactory.CreateParticles("KhongGian_Junk", transform, _junkStyle, 0.6f, true);
        }

        private void Cleanup()
        {
            StopAllCoroutines();
            foreach (Pulse p in _pulses)
            {
                if (p.mat != null) Destroy(p.mat);
                if (p.go != null) Destroy(p.go);
            }
            _pulses.Clear();
            DestroySystem(ref _stars);
            DestroySystem(ref _meteors);
            DestroySystem(ref _thruster);
            DestroySystem(ref _junk);
            foreach (ChestFxProfile s in _styles) if (s != null) Destroy(s);
            _styles.Clear();
            if (_ringMesh != null) Destroy(_ringMesh);
            _ringMesh = null;
        }

        private static void DestroySystem(ref ParticleSystem ps)
        {
            if (ps == null) return;
            ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
            if (r != null && r.sharedMaterial != null) Destroy(r.sharedMaterial);
            Destroy(ps.gameObject);
            ps = null;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_ThienCo extras, all built in code:
    /// - Roulette: two flat cyan gears over the chest mouth. The big one jumps exactly one tooth per roulette
    ///   tick (ratchet), the small one counter-rotates; the teeth glow in the colour being shown.
    /// - Lock: the "lock rings" open one after another, one ring per tier rank (Thường 1 ... Huyền thoại 4),
    ///   each popping upward and outward, then a thruster column of cyan streaks fires straight up.
    ///   Huyền thoại adds a core flash (radial sparkles). Rác: the gear jams, shakes, turns grey and drops dull sparks.
    /// Decorates only, never changes the tier.
    /// </summary>
    public class ThienCoHook : ChestFxHook
    {
        [SerializeField] private int teeth = 16;
        [SerializeField] private float gearRadius = 0.2f;
        [SerializeField] private float ringInterval = 0.13f;
        [SerializeField] private float ringLife = 0.55f;
        [SerializeField] private int thrusterBase = 25;
        [SerializeField] private Color cyan = new Color(0.25f, 0.95f, 1f);
        [SerializeField] private Color jamGrey = new Color(0.45f, 0.42f, 0.38f);

        private class Ring { public Transform t; public Material m; public float start; public float height; }

        private Transform _root;
        private Transform _bigGear, _smallGear;
        private Material _bigMat, _smallMat;
        private Mesh _gearMesh, _ringMesh;
        private readonly List<Ring> _rings = new List<Ring>();
        private ParticleSystem _thruster, _sparks;
        private ChestFxProfile _thrusterStyle, _sparkStyle;

        private float _bigAngle, _bigTarget, _smallAngle;
        private float _jamUntil = -1f, _fadeStart = -1f;
        private Color _gearColor;
        private int _pendingRings;
        private float _nextRingTime;
        private ChestTier _lockedTier;

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            Build(reveal);
            _root.SetPositionAndRotation(mouth + reveal.transform.up * 0.02f, reveal.transform.rotation);
            _root.gameObject.SetActive(true);
            _bigAngle = _bigTarget = _smallAngle = 0f;
            _jamUntil = _fadeStart = -1f;
            _pendingRings = 0;
            _gearColor = cyan;
            SetGearAlpha(1f);
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (_root == null) return;
            _bigTarget += 360f / teeth;                       // one tooth per tick
            _gearColor = Color.Lerp(cyan, color, 0.6f);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_root == null) return;
            _lockedTier = tier;
            if (tier == ChestTier.Rac)
            {
                _jamUntil = Time.time + 0.9f;
                _gearColor = jamGrey;
                _sparks.transform.position = _root.position;
                ChestFxFactory.SetParticleColor(_sparks, jamGrey, _sparkStyle, 0.7f);
                _sparks.Play();
                _sparks.Emit(14);
                _fadeStart = Time.time + 1.1f;
                return;
            }
            _gearColor = Color.Lerp(color, Color.white, 0.3f);
            _pendingRings = tier.Rank();                      // 1..4 locks open in a row
            _nextRingTime = Time.time;
            _fadeStart = Time.time + ringInterval * _pendingRings + 0.4f;
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            if (_root != null) _root.gameObject.SetActive(false);
            foreach (Ring r in _rings) r.t.gameObject.SetActive(false);
            _pendingRings = 0;
        }

        private void Update()
        {
            if (_root == null || !_root.gameObject.activeSelf) return;
            float dt = Time.deltaTime;

            if (Time.time < _jamUntil)
            {
                // jammed: tries to turn, snaps back, shakes
                float shake = Mathf.Sin(Time.time * 70f) * 4f;
                _bigGear.localRotation = Quaternion.Euler(0f, _bigAngle + shake, 0f);
                _smallGear.localRotation = Quaternion.Euler(0f, _smallAngle - shake * 2f, 0f);
                _root.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 50f) * 0.03f);
            }
            else
            {
                // ratchet: snaps quickly to the next tooth instead of turning smoothly
                float prev = _bigAngle;
                _bigAngle = Mathf.Min(_bigAngle + 900f * dt, _bigTarget);   // target only grows
                _smallAngle -= (_bigAngle - prev) * 2f;       // half the teeth: turns twice as fast
                _bigGear.localRotation = Quaternion.Euler(0f, _bigAngle, 0f);
                _smallGear.localRotation = Quaternion.Euler(0f, _smallAngle, 0f);
                _root.localScale = Vector3.one;
            }

            float alpha = 1f;
            if (_fadeStart > 0f) alpha = 1f - Mathf.Clamp01((Time.time - _fadeStart) / 0.6f);
            SetGearAlpha(alpha);
            if (_fadeStart > 0f && alpha <= 0f && _pendingRings == 0 && !AnyRingAlive()) _root.gameObject.SetActive(false);

            while (_pendingRings > 0 && Time.time >= _nextRingTime)
            {
                int index = _lockedTier.Rank() - _pendingRings;   // 0 = first lock
                SpawnRing(index);
                _pendingRings--;
                _nextRingTime += ringInterval;
                if (_pendingRings == 0) FireThruster();
            }
            UpdateRings();
        }

        // ------------------------------------------------------------------ Lock rings and thruster

        private void SpawnRing(int index)
        {
            Ring r = _rings.Find(x => !x.t.gameObject.activeSelf);
            if (r == null)
            {
                GameObject go = new GameObject("ThienCo_LockRing");
                go.AddComponent<MeshFilter>(); go.AddComponent<MeshRenderer>();
                go.hideFlags = HideFlags.DontSave;
                go.transform.SetParent(_root, false);
                go.GetComponent<MeshFilter>().sharedMesh = _ringMesh;
                r = new Ring { t = go.transform, m = ChestFxFactory.CreateAdditive(null, "ThienCo_Ring") };
                go.GetComponent<MeshRenderer>().sharedMaterial = r.m;
                _rings.Add(r);
            }
            r.start = Time.time;
            r.height = 0.05f + index * 0.07f;             // stacked like the stages of a rocket
            r.t.gameObject.SetActive(true);
        }

        private void UpdateRings()
        {
            foreach (Ring r in _rings)
            {
                if (!r.t.gameObject.activeSelf) continue;
                float k = (Time.time - r.start) / ringLife;
                if (k >= 1f) { r.t.gameObject.SetActive(false); continue; }
                float ease = 1f - (1f - k) * (1f - k);
                r.t.localPosition = Vector3.up * (r.height * ease);
                r.t.localScale = Vector3.one * gearRadius * Mathf.Lerp(0.9f, 1.8f, ease);
                Color c = Color.Lerp(Color.white, cyan, k);
                c.a = 1f - k;
                ChestFxFactory.SetColor(r.m, c);
            }
        }

        private bool AnyRingAlive()
        {
            foreach (Ring r in _rings) if (r.t.gameObject.activeSelf) return true;
            return false;
        }

        private void FireThruster()
        {
            _thruster.transform.position = _root.position;
            ChestFxFactory.SetParticleColor(_thruster, cyan, _thrusterStyle, 1f);
            _thruster.Play();
            _thruster.Emit(Mathf.RoundToInt(thrusterBase * (1f + 2f * _lockedTier.Rank01())));
            if (_lockedTier == ChestTier.HuyenThoai)
            {
                // energy core flash: white-cyan sparkles all around
                _sparks.transform.position = _root.position + Vector3.up * 0.15f;
                ChestFxFactory.SetParticleColor(_sparks, Color.Lerp(cyan, Color.white, 0.5f), _sparkStyle, 1.5f);
                _sparks.Play();
                _sparks.Emit(60);
            }
        }

        // ------------------------------------------------------------------ Building

        private void Build(ChestRevealSequence reveal)
        {
            if (_root != null) return;
            _root = new GameObject("ThienCo_Fx").transform;
            _root.gameObject.hideFlags = HideFlags.DontSave;

            _gearMesh = CreateGearMesh(teeth, 0.62f, 0.82f, 1f);
            _ringMesh = ChestFxFactory.CreateRingMesh(0.85f);
            _bigGear = MakeGear("ThienCo_GearBig", _gearMesh, gearRadius, Vector3.zero, out _bigMat);
            _smallGear = MakeGear("ThienCo_GearSmall", CreateGearMesh(teeth / 2, 0.5f, 0.78f, 1f), gearRadius * 0.5f,
                                  new Vector3(gearRadius * 1.38f, 0.01f, 0f), out _smallMat);

            _thrusterStyle = ScriptableObject.CreateInstance<ChestFxProfile>();
            _thrusterStyle.hideFlags = HideFlags.DontSave;
            _thrusterStyle.particleLook = ParticleLook.Streak;
            _thrusterStyle.particleDirection = ParticleDirection.Up;
            _thrusterStyle.particleSpeed = 2.4f;
            _thrusterStyle.particleSize = 0.02f;
            _thrusterStyle.particleLifetime = 0.55f;
            _thrusterStyle.secondaryColor = Color.white;
            _thrusterStyle.secondaryMix = 0.35f;
            _thruster = ChestFxFactory.CreateParticles("ThienCo_Thruster", null, _thrusterStyle, gearRadius * 2f, true);
            ParticleSystem.ShapeModule shape = _thruster.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 4f;                                   // narrow exhaust column
            shape.radius = gearRadius * 0.25f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            _sparkStyle = ScriptableObject.CreateInstance<ChestFxProfile>();
            _sparkStyle.hideFlags = HideFlags.DontSave;
            _sparkStyle.particleLook = ParticleLook.Sparkle;
            _sparkStyle.particleDirection = ParticleDirection.Radial;
            _sparkStyle.particleSpeed = 0.5f;
            _sparkStyle.particleSize = 0.03f;
            _sparkStyle.particleLifetime = 0.8f;
            _sparkStyle.burstGravity = 0.8f;
            _sparkStyle.secondaryColor = Color.white;
            _sparkStyle.secondaryMix = 0.3f;
            _sparks = ChestFxFactory.CreateParticles("ThienCo_Sparks", null, _sparkStyle, gearRadius * 2f, true);
        }

        private Transform MakeGear(string name, Mesh mesh, float scale, Vector3 offset, out Material mat)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<MeshFilter>(); go.AddComponent<MeshRenderer>();
            go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(_root, false);
            go.transform.localPosition = offset;
            go.transform.localScale = Vector3.one * scale;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            mat = ChestFxFactory.CreateAdditive(null, name);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go.transform;
        }

        private void SetGearAlpha(float a)
        {
            if (_bigMat == null) return;
            Color c = _gearColor; c.a = a * 0.9f;
            ChestFxFactory.SetColor(_bigMat, c);
            c.a = a * 0.7f;
            ChestFxFactory.SetColor(_smallMat, c);
        }

        /// <summary>Flat toothed ring in the XZ plane (outer radius 1). Vertex colour alpha fades the hub.</summary>
        private static Mesh CreateGearMesh(int toothCount, float hub, float root, float tip)
        {
            int steps = toothCount * 4;                         // root, rise, tip, fall per tooth
            List<Vector3> verts = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<int> tris = new List<int>();
            for (int i = 0; i <= steps; i++)
            {
                float a = i * Mathf.PI * 2f / steps;
                Vector3 d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                float outer = (i % 4 == 1 || i % 4 == 2) ? tip : root;
                verts.Add(d * hub); colors.Add(new Color(1f, 1f, 1f, 0.15f));
                verts.Add(d * outer); colors.Add(Color.white);
                if (i == steps) break;
                int b = i * 2;
                tris.AddRange(new[] { b, b + 2, b + 1, b + 1, b + 2, b + 3 });
                tris.AddRange(new[] { b, b + 1, b + 2, b + 1, b + 3, b + 2 });   // both faces
            }
            Mesh mesh = new Mesh { name = "ThienCo_Gear", hideFlags = HideFlags.DontSave };
            mesh.SetVertices(verts);
            mesh.SetColors(colors);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private void OnDestroy()
        {
            if (_root != null) Destroy(_root.gameObject);
            DestroyIfSet(_bigMat); DestroyIfSet(_smallMat);
            foreach (Ring r in _rings) DestroyIfSet(r.m);
            if (_smallGear != null) DestroyIfSet(_smallGear.GetComponent<MeshFilter>().sharedMesh);
            DestroyIfSet(_gearMesh); DestroyIfSet(_ringMesh);
            DestroyParticles(_thruster); DestroyParticles(_sparks);
            DestroyIfSet(_thrusterStyle); DestroyIfSet(_sparkStyle);
        }

        private static void DestroyParticles(ParticleSystem ps)
        {
            if (ps == null) return;
            Destroy(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            Destroy(ps.gameObject);
        }

        private static void DestroyIfSet(Object o) { if (o != null) Destroy(o); }
    }
}

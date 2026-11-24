using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_CoPhong extras: a thin incense thread rises from the jar while the colours turn; at the lock an ink
    /// wash spreads on the ground under the jar (black ink, a tier-coloured rim), Rác spills ink drops that fall,
    /// and from Hiếm up a red seal stamp (dấu son) presses in above the silhouette. Decoration only.
    /// </summary>
    public class CoPhongHook : ChestFxHook
    {
        [SerializeField] private Color inkColor = new Color(0.05f, 0.05f, 0.06f);
        [SerializeField] private Color sealColor = new Color(0.78f, 0.12f, 0.09f);
        [SerializeField] private Color smokeColor = new Color(0.55f, 0.55f, 0.5f);
        [SerializeField] private float washRadius = 0.45f;
        [SerializeField] private float sealSize = 0.07f;

        private ParticleSystem _smoke, _spill;
        private ChestFxProfile _smokeStyle, _spillStyle;
        private GameObject _wash, _washRim, _seal;
        private Material _inkMat, _rimMat, _sealMat;
        private Mesh _ringMesh, _discMesh, _quadMesh;

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            if (_smoke == null)
            {
                _smokeStyle = Style(ParticleLook.SoftDot, ParticleDirection.Up, 0.08f, 0.018f, 3f);
                _smokeStyle.ambientRate = 10f;
                _smoke = ChestFxFactory.CreateParticles("CoPhongIncense", null, _smokeStyle, 0.3f, false);
                ParticleSystem.ShapeModule shape = _smoke.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.01f;
                shape.rotation = new Vector3(-90f, 0f, 0f);
            }
            _smoke.transform.SetPositionAndRotation(mouth, reveal.transform.rotation);
            ChestFxFactory.SetParticleColor(_smoke, smokeColor, _smokeStyle, 0.5f);
            _smoke.Play();
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_smoke != null) _smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            StopAllCoroutines();
            StartCoroutine(InkWash(reveal, tier, color));
            if (tier == ChestTier.Rac) SpillInk(reveal, color);
            if (tier >= ChestTier.Hiem) StartCoroutine(Seal(reveal, tier));
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        // Ink spreads fast then slows (like ink on rice paper), holds, then sinks away. Opaque unlit, so it shrinks instead of fading.
        private IEnumerator InkWash(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_wash == null)
            {
                _discMesh = ChestFxFactory.CreateRingMesh(0f, 40);
                _ringMesh = ChestFxFactory.CreateRingMesh(0.9f, 40);
                _inkMat = ChestFxFactory.CreateUnlit(inkColor, false, "CoPhongInk");
                _rimMat = ChestFxFactory.CreateUnlit(color, false, "CoPhongInkRim");
                _wash = MakeMesh("CoPhongInkWash", _discMesh, _inkMat);
                _washRim = MakeMesh("CoPhongInkRim", _ringMesh, _rimMat);
            }
            ChestFxFactory.SetColor(_rimMat, color);
            Vector3 basePos = reveal.transform.position + Vector3.up * 0.004f;
            _wash.transform.position = basePos;
            _washRim.transform.position = basePos + Vector3.up * 0.001f;
            _wash.SetActive(true);
            _washRim.SetActive(true);
            float radius = washRadius * Mathf.Lerp(0.7f, 1.4f, tier.Rank01());
            float t = 0f;
            while (t < 4f)
            {
                t += Time.deltaTime;
                float k = 1f - Mathf.Clamp01(t / 1.4f);
                float grow = 1f - k * k * k;
                float sink = t > 2.8f ? 1f - (t - 2.8f) / 1.2f : 1f;
                float r = Mathf.Max(0.001f, radius * grow * sink);
                _wash.transform.localScale = new Vector3(r, 1f, r * 0.92f);   // slightly uneven, like a real blot
                _washRim.transform.localScale = new Vector3(r * 1.04f, 1f, r * 0.96f);
                yield return null;
            }
            _wash.SetActive(false);
            _washRim.SetActive(false);
        }

        // Rác: a few fat ink drops jump out of the jar and fall (comic "mực đổ").
        private void SpillInk(ChestRevealSequence reveal, Color color)
        {
            if (_spill == null)
            {
                _spillStyle = Style(ParticleLook.SoftDot, ParticleDirection.Fountain, 0.7f, 0.035f, 1.3f);
                _spillStyle.burstGravity = 1.2f;
                _spill = ChestFxFactory.CreateParticles("CoPhongInkSpill", null, _spillStyle, 0.4f, true);
            }
            _spill.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
            ChestFxFactory.SetParticleColor(_spill, color, _spillStyle, 0.6f);
            _spill.Play();
            _spill.Emit(14);
        }

        // Dấu son: a red square presses down with a little overshoot above the item, then stays until reset.
        private IEnumerator Seal(ChestRevealSequence reveal, ChestTier tier)
        {
            yield return new WaitForSeconds(0.9f);   // after the silhouette has popped out
            if (_seal == null)
            {
                _quadMesh = ChestFxFactory.CreateQuadMesh();
                _sealMat = ChestFxFactory.CreateUnlit(sealColor, false, "CoPhongSeal");
                _seal = MakeMesh("CoPhongSeal", _quadMesh, _sealMat);
            }
            Camera cam = Camera.main;
            Vector3 side = cam != null ? cam.transform.right : reveal.transform.right;
            _seal.transform.position = reveal.MouthPosition + Vector3.up * 0.42f + side * 0.2f;
            if (cam != null) _seal.transform.rotation = Quaternion.LookRotation(_seal.transform.position - cam.transform.position) * Quaternion.Euler(0f, 0f, -8f);
            _seal.SetActive(true);
            float size = sealSize * (tier == ChestTier.HuyenThoai ? 1.5f : tier == ChestTier.RatHiem ? 1.2f : 1f);
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / 0.35f);
                float s = u < 0.6f ? Mathf.Lerp(2.2f, 0.85f, u / 0.6f) : Mathf.Lerp(0.85f, 1f, (u - 0.6f) / 0.4f);   // stamp: big → press → settle
                _seal.transform.localScale = Vector3.one * size * s;
                yield return null;
            }
        }

        private static ChestFxProfile Style(ParticleLook look, ParticleDirection dir, float speed, float size, float life)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.secondaryColor = Color.white;
            s.secondaryMix = 0.15f;
            return s;
        }

        private static GameObject MakeMesh(string name, Mesh mesh, Material mat)
        {
            GameObject go = new GameObject(name) { hideFlags = HideFlags.DontSave };
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            go.SetActive(false);
            return go;
        }

        private void Cleanup()
        {
            StopAllCoroutines();
            if (_smoke != null) { Destroy(_smoke.GetComponent<ParticleSystemRenderer>().sharedMaterial); Destroy(_smoke.gameObject); }
            if (_spill != null) { Destroy(_spill.GetComponent<ParticleSystemRenderer>().sharedMaterial); Destroy(_spill.gameObject); }
            foreach (Object o in new Object[] { _smokeStyle, _spillStyle, _wash, _washRim, _seal, _inkMat, _rimMat, _sealMat, _ringMesh, _discMesh, _quadMesh })
                if (o != null) Destroy(o);
            _smoke = _spill = null;
            _smokeStyle = _spillStyle = null;
            _wash = _washRim = _seal = null;
            _inkMat = _rimMat = _sealMat = null;
            _ringMesh = _discMesh = _quadMesh = null;
        }
    }
}

using UnityEngine;

namespace ChoDoLa.Interaction
{
    /// <summary>
    /// A scroll that really rolls and unrolls. The paper is a procedural strip bent along an
    /// Archimedean spiral around a core roller, so the roll shrinks smoothly while the flat part grows.
    /// Everything (paper, two rollers, end bar, cord, wax seal) is built at runtime, no model needed.
    ///
    /// Layout (local space): the sheet faces -Z (toward a viewer looking along +Z), the roller axis is Y,
    /// the core roller sits at the left (X = 0) and the end bar moves to the right as the scroll opens.
    /// The maths is mirrored 1:1 by the three.js preview used to render the reference screenshots.
    /// </summary>
    public class ScrollUnroll : MonoBehaviour
    {
        [Header("Size (metres)")]
        [SerializeField] private float width = 0.20f;
        [SerializeField] private float length = 0.36f;
        [SerializeField] private float paperThickness = 0.003f;
        [SerializeField] private float coreRadius = 0.012f;
        [SerializeField] private float rodRadius = 0.008f;

        [Header("Look")]
        [Tooltip("Texture of the paper; stretched along the whole length. Text starts at the end bar side.")]
        [SerializeField] private Texture2D paperTexture;
        [SerializeField] private Color paperColor = new Color(0.93f, 0.87f, 0.74f);
        [SerializeField] private Color woodColor = new Color(0.42f, 0.27f, 0.14f);
        [SerializeField] private Color knobColor = new Color(0.23f, 0.14f, 0.07f);
        [SerializeField] private Color sealColor = new Color(0.61f, 0.11f, 0.11f);
        [SerializeField] private Color cordColor = new Color(0.69f, 0.13f, 0.13f);

        [Header("Animation")]
        [SerializeField, Range(0f, 1f)] private float openAmount;
        [SerializeField] private float toggleSeconds = 1.6f;
        [Tooltip("The cord and wax seal are removed once the scroll is opened past this amount.")]
        [SerializeField] private float sealBreakAt = 0.12f;

        private bool _built;
        private float _target;

        private Transform _visual;      // everything lives here; shifted so the scroll stays centred
        private Transform _core;
        private Transform _bar;
        private Transform _cord;
        private Mesh _cordMesh;
        private Vector3[] _cordVertices;
        private const int CordRingSegments = 48;
        private const int CordTubeSegments = 12;
        private const float CordTubeRadius = 0.0028f;
        private Transform _seal;
        private Mesh _paperMesh;
        private Vector3[] _vertices;
        private int _segments;

        /// <summary>0 = closed, 1 = fully open.</summary>
        public float OpenAmount => openAmount;
        public bool IsOpen => _target > 0.5f;

        // ---------------------------------------------------------------- Public API

        public void Toggle()
        {
            EnsureBuilt();
            _target = _target > 0.5f ? 0f : 1f;
        }

        public void SetOpenImmediate(float amount)
        {
            EnsureBuilt();
            openAmount = Mathf.Clamp01(amount);
            _target = openAmount;
            Apply(Mathf.SmoothStep(0f, 1f, openAmount));
        }

        // ---------------------------------------------------------------- Unity

        private void Awake()
        {
            EnsureBuilt();
            _target = openAmount;
            Apply(Mathf.SmoothStep(0f, 1f, openAmount));
        }

        private void Update()
        {
            if (Mathf.Approximately(openAmount, _target)) return;

            openAmount = Mathf.MoveTowards(openAmount, _target, Time.deltaTime / Mathf.Max(0.05f, toggleSeconds));
            Apply(Mathf.SmoothStep(0f, 1f, openAmount));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_built) Apply(Mathf.SmoothStep(0f, 1f, openAmount));
        }
#endif

        // ---------------------------------------------------------------- Spiral maths (see preview for the same code)

        private float RolledOuterRadius(float rolledLength)
        {
            return Mathf.Sqrt(coreRadius * coreRadius + paperThickness * rolledLength / Mathf.PI);
        }

        /// <summary>Winding angle (radians) of a piece of paper of arc length <paramref name="a"/> wound on the core.</summary>
        private float WindAngle(float a)
        {
            if (a <= 0f) return 0f;
            return (-coreRadius + Mathf.Sqrt(coreRadius * coreRadius + paperThickness * a / Mathf.PI)) * 2f * Mathf.PI / paperThickness;
        }

        private float RadiusAt(float a)
        {
            return coreRadius + paperThickness * WindAngle(a) / (2f * Mathf.PI);
        }

        /// <summary>
        /// Point on the paper profile (x along the unroll direction, py = height above the flat plane),
        /// <paramref name="p"/> metres of paper away from the end bar.
        /// </summary>
        private Vector2 Profile(float p, float flat, float rolled, float outerRadius)
        {
            if (p <= flat) return new Vector2(flat - p, 0f);

            float s = p - flat;
            float aIn = rolled - s;
            float phi = Mathf.PI / 2f + (WindAngle(rolled) - WindAngle(aIn));
            float r = RadiusAt(aIn);
            return new Vector2(r * Mathf.Cos(phi), -outerRadius + r * Mathf.Sin(phi));
        }

        // ---------------------------------------------------------------- Build and update

        private void EnsureBuilt()
        {
            if (_built) return;
            _built = true;

            // A clone (for example the held copy) already contains the visuals of the original: drop them and rebuild.
            Transform old = transform.Find("ScrollVisual");
            if (old != null)
            {
                old.name = "ScrollVisual_Old";
                old.gameObject.SetActive(false);
                Destroy(old.gameObject);
            }

            _visual = new GameObject("ScrollVisual").transform;
            _visual.SetParent(transform, false);

            // Paper
            _segments = Mathf.Max(60, Mathf.RoundToInt(length / 0.0015f));
            _paperMesh = BuildPaperMesh();
            GameObject paper = new GameObject("Paper");
            paper.transform.SetParent(_visual, false);
            paper.AddComponent<MeshFilter>().sharedMesh = _paperMesh;
            paper.AddComponent<MeshRenderer>().sharedMaterial = MakeMaterial(paperColor, paperTexture, true, 0.15f);

            Material wood = MakeMaterial(woodColor, null, false, 0.4f);
            Material knob = MakeMaterial(knobColor, null, false, 0.4f);

            // Core roller and end bar (axis along Y), with knobs at both ends
            _core = new GameObject("CoreRoller").transform;
            _core.SetParent(_visual, false);
            MakeCylinder("Shaft", _core, coreRadius, width + 0.03f, wood, 0f);
            MakeCylinder("KnobTop", _core, coreRadius * 1.35f, 0.012f, knob, width / 2f + 0.021f);
            MakeCylinder("KnobBottom", _core, coreRadius * 1.35f, 0.012f, knob, -(width / 2f + 0.021f));

            _bar = new GameObject("EndBar").transform;
            _bar.SetParent(_visual, false);
            MakeCylinder("Shaft", _bar, rodRadius, width + 0.02f, wood, 0f);
            MakeCylinder("KnobTop", _bar, rodRadius * 1.3f, 0.01f, knob, width / 2f + 0.015f);
            MakeCylinder("KnobBottom", _bar, rodRadius * 1.3f, 0.01f, knob, -(width / 2f + 0.015f));

            // Cord (a ring around the closed roll) and wax seal
            GameObject cord = new GameObject("Cord");
            cord.transform.SetParent(_visual, false);
            _cordMesh = BuildTorusMesh();
            cord.AddComponent<MeshFilter>().sharedMesh = _cordMesh;
            cord.AddComponent<MeshRenderer>().sharedMaterial = MakeMaterial(cordColor, null, false, 0.2f);
            _cord = cord.transform;

            GameObject seal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            seal.name = "WaxSeal";
            RemoveCollider(seal);
            seal.transform.SetParent(_visual, false);
            seal.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            seal.transform.localScale = new Vector3(0.028f, 0.003f, 0.028f);
            seal.GetComponent<MeshRenderer>().sharedMaterial = MakeMaterial(sealColor, null, false, 0.6f);
            _seal = seal.transform;
        }

        private Mesh BuildPaperMesh()
        {
            int count = (_segments + 1) * 2;
            _vertices = new Vector3[count];
            Vector2[] uv = new Vector2[count];
            int[] tris = new int[_segments * 6];

            for (int i = 0; i <= _segments; i++)
            {
                float p = length * i / _segments;
                // The text on the texture starts at the right-hand edge, which is the end bar side.
                uv[i * 2] = new Vector2(1f - p / length, 0f);
                uv[i * 2 + 1] = new Vector2(1f - p / length, 1f);
            }

            for (int i = 0; i < _segments; i++)
            {
                int a = i * 2, b = a + 1, c = a + 2, d = a + 3;
                int t = i * 6;
                tris[t] = a; tris[t + 1] = c; tris[t + 2] = b;
                tris[t + 3] = b; tris[t + 4] = c; tris[t + 5] = d;
            }

            Mesh mesh = new Mesh { name = "ScrollPaper" };
            mesh.MarkDynamic();
            mesh.vertices = _vertices;
            mesh.uv = uv;
            mesh.triangles = tris;
            return mesh;
        }

        private void Apply(float u)
        {
            if (!_built) return;

            float flat = u * length;
            float rolled = length - flat;
            float outer = RolledOuterRadius(rolled);

            for (int i = 0; i <= _segments; i++)
            {
                float p = length * i / _segments;
                Vector2 pt = Profile(p, flat, rolled, outer);
                _vertices[i * 2] = new Vector3(pt.x, -width / 2f, -pt.y);
                _vertices[i * 2 + 1] = new Vector3(pt.x, width / 2f, -pt.y);
            }

            _paperMesh.vertices = _vertices;
            _paperMesh.RecalculateNormals();
            _paperMesh.RecalculateBounds();

            _core.localPosition = new Vector3(0f, 0f, outer);
            _bar.localPosition = new Vector3(flat, 0f, -rodRadius * 0.4f);

            // Cord and seal come off as the scroll starts to open.
            float k = sealBreakAt > 0f ? Mathf.Clamp01(u / sealBreakAt) : 1f;
            bool showSeal = k < 0.9f;
            _cord.gameObject.SetActive(showSeal);
            _seal.gameObject.SetActive(showSeal);
            if (showSeal)
            {
                _cord.localPosition = new Vector3(0f, 0f, outer);
                FillTorus((outer + 0.003f) * (1f + k * 0.5f));
                _seal.localPosition = new Vector3(k * 0.03f, -k * k * 0.12f, -0.003f);
                _seal.localRotation = Quaternion.Euler(90f, 0f, k * 50f);
            }

            // Keep the whole scroll centred while it opens.
            _visual.localPosition = new Vector3(-flat / 2f, 0f, 0f);
        }

        // ---------------------------------------------------------------- Helpers

        private static Transform MakeCylinder(string name, Transform parent, float radius, float height, Material material, float y)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            RemoveCollider(go);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, y, 0f);
            // The built-in cylinder is 2 m tall and 1 m wide.
            go.transform.localScale = new Vector3(radius * 2f, height / 2f, radius * 2f);
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go.transform;
        }

        private static void RemoveCollider(GameObject go)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        private static Material MakeMaterial(Color color, Texture2D texture, bool doubleSided, float smoothness)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material material = new Material(shader);
            material.color = color;
            if (texture != null)
            {
                material.mainTexture = texture;
                material.color = Color.white;
            }
            if (doubleSided && material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        private Mesh BuildTorusMesh()
        {
            _cordVertices = new Vector3[(CordRingSegments + 1) * (CordTubeSegments + 1)];
            int[] triangles = new int[CordRingSegments * CordTubeSegments * 6];

            int t = 0;
            for (int i = 0; i < CordRingSegments; i++)
            {
                for (int j = 0; j < CordTubeSegments; j++)
                {
                    int v0 = i * (CordTubeSegments + 1) + j;
                    int v1 = v0 + 1;
                    int v2 = v0 + CordTubeSegments + 1;
                    int v3 = v2 + 1;
                    triangles[t++] = v0; triangles[t++] = v2; triangles[t++] = v1;
                    triangles[t++] = v1; triangles[t++] = v2; triangles[t++] = v3;
                }
            }

            Mesh mesh = new Mesh { name = "CordTorus" };
            mesh.MarkDynamic();
            mesh.vertices = _cordVertices;
            mesh.triangles = triangles;
            _cordMesh = mesh;
            FillTorus(coreRadius);
            return mesh;
        }

        /// <summary>Rebuilds the cord as a ring of the given radius around the Y axis, keeping the cord thickness constant.</summary>
        private void FillTorus(float ringRadius)
        {
            for (int i = 0; i <= CordRingSegments; i++)
            {
                float a = i * Mathf.PI * 2f / CordRingSegments;
                Vector3 outward = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                for (int j = 0; j <= CordTubeSegments; j++)
                {
                    float b = j * Mathf.PI * 2f / CordTubeSegments;
                    _cordVertices[i * (CordTubeSegments + 1) + j] =
                        outward * ringRadius + (outward * Mathf.Cos(b) + Vector3.up * Mathf.Sin(b)) * CordTubeRadius;
                }
            }

            _cordMesh.vertices = _cordVertices;
            _cordMesh.RecalculateNormals();
            _cordMesh.RecalculateBounds();
        }
    }
}

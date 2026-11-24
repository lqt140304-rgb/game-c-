using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Pure UnityEngine.CoreModule animated flowing water system for the Central Fountain:
    /// - Scrolling tangent-space wave normal map + caustic water reflection on all 3 pool tiers
    /// - 4 parabolic arching tubular water streams from the brass spouts with flowing foam streaks
    /// - 2 translucent wavy waterfall curtains cascading between the upper bowls
    /// - 4 animated expanding impact splash foam rings on the main pool surface
    /// </summary>
    public class FountainWaterController : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float surfaceScrollU = 0.08f;
        [SerializeField] private float surfaceScrollV = 0.06f;
        [SerializeField] private float streamFlowSpeed = 2.6f;
        [SerializeField] private float cascadeFlowSpeed = 1.85f;

        private Material _waterSurfaceMat;
        private Material _streamMat;
        private Material _cascadeMat;
        private Texture2D _waveNormalMap;
        private Texture2D _streamTex;
        private Texture2D _splashRingTex;

        private readonly List<Transform> _splashRings = new List<Transform>();
        private readonly List<Material> _splashMats = new List<Material>();
        private readonly List<Mesh> _ownedMeshes = new List<Mesh>();
        private bool _initialized;

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _waveNormalMap = GenerateWaveNormalMap(128);
            _streamTex = GenerateFlowingStreamTexture(128);
            _splashRingTex = GenerateSplashRingTexture(64);

            SetupPoolSurfaces();
            SetupFlowingWaterMeshes();
        }

        private void Update()
        {
            if (!_initialized) return;

            float t = Time.time;

            if (_waterSurfaceMat != null)
            {
                Vector2 surfOff = new Vector2(t * surfaceScrollU, t * surfaceScrollV);
                SetTexOffset(_waterSurfaceMat, "_BumpMap", surfOff);
                SetTexOffset(_waterSurfaceMat, "_BaseMap", -surfOff * 0.65f);
                if (_waterSurfaceMat.HasProperty("_BumpScale"))
                {
                    _waterSurfaceMat.SetFloat("_BumpScale", 0.46f + Mathf.Sin(t * 2.1f) * 0.08f);
                }
            }

            if (_streamMat != null)
            {
                Vector2 flowOff = new Vector2(0f, t * streamFlowSpeed);
                SetTexOffset(_streamMat, "_BaseMap", flowOff);
                SetTexOffset(_streamMat, "_BumpMap", flowOff * 0.8f);
            }

            if (_cascadeMat != null)
            {
                Vector2 cascOff = new Vector2(Mathf.Sin(t * 0.7f) * 0.03f, t * cascadeFlowSpeed);
                SetTexOffset(_cascadeMat, "_BaseMap", cascOff);
                SetTexOffset(_cascadeMat, "_BumpMap", cascOff * 0.7f);
            }

            for (int i = 0; i < _splashRings.Count; i++)
            {
                float phase = Mathf.Repeat(t * 1.15f + i * 0.25f, 1.0f);
                float scale = Mathf.Lerp(0.28f, 1.05f, phase);
                _splashRings[i].localScale = new Vector3(scale, 1f, scale);

                float alpha = Mathf.Sin(phase * Mathf.PI) * 0.68f;
                Material sm = _splashMats[i];
                if (sm != null)
                {
                    Color c = new Color(0.82f, 0.96f, 1.0f, alpha);
                    if (sm.HasProperty("_BaseColor")) sm.SetColor("_BaseColor", c);
                    else sm.color = c;
                }
            }
        }

        private void OnDestroy()
        {
            if (_waveNormalMap != null) Destroy(_waveNormalMap);
            if (_streamTex != null) Destroy(_streamTex);
            if (_splashRingTex != null) Destroy(_splashRingTex);
            if (_waterSurfaceMat != null) Destroy(_waterSurfaceMat);
            if (_streamMat != null) Destroy(_streamMat);
            if (_cascadeMat != null) Destroy(_cascadeMat);
            foreach (Material m in _splashMats)
            {
                if (m != null) Destroy(m);
            }
            foreach (Mesh m in _ownedMeshes)
            {
                if (m != null) Destroy(m);
            }
        }

        private void SetupPoolSurfaces()
        {
            _waterSurfaceMat = CreateWaterLitMaterial(
                "M_FountainPool_Live",
                new Color(0.07f, 0.44f, 0.54f, 0.84f),
                null,
                _waveNormalMap,
                new Vector2(3.5f, 3.5f),
                0.96f,
                0.24f,
                0.48f,
                new Color(0.02f, 0.14f, 0.20f)
            );

            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            {
                if (r.name.StartsWith("WaterSurface"))
                {
                    r.sharedMaterial = _waterSurfaceMat;
                }
            }
        }

        private void SetupFlowingWaterMeshes()
        {
            Transform oldHolder = transform.Find("Fountain_FlowingWater");
            if (oldHolder != null) Destroy(oldHolder.gameObject);

            GameObject holder = new GameObject("Fountain_FlowingWater");
            holder.transform.SetParent(transform, false);

            _streamMat = CreateWaterLitMaterial(
                "M_FountainStream_Live",
                new Color(0.75f, 0.94f, 1.0f, 0.78f),
                _streamTex,
                _waveNormalMap,
                new Vector2(2f, 2f),
                0.94f,
                0.10f,
                0.35f,
                new Color(0.03f, 0.12f, 0.18f)
            );

            _cascadeMat = CreateWaterLitMaterial(
                "M_FountainCascade_Live",
                new Color(0.68f, 0.91f, 0.98f, 0.34f),
                _streamTex,
                _waveNormalMap,
                new Vector2(10f, 2f),
                0.92f,
                0.10f,
                0.28f,
                new Color(0.01f, 0.06f, 0.10f)
            );

            // 1. Four parabolic arching spout streams at 0, 90, 180, 270 degrees
            _splashRings.Clear();
            _splashMats.Clear();

            for (int i = 0; i < 4; i++)
            {
                float rad = i * Mathf.PI * 0.5f;
                Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));

                Mesh tubeMesh = BuildParabolicStreamMesh(dir, 0.42f, 1.31f, 1.08f, 0.67f, 0.16f);
                _ownedMeshes.Add(tubeMesh);
                CreateMeshObject(holder.transform, $"WaterStream_{i}", tubeMesh, _streamMat);

                // Splash ripple ring on main pool where stream hits
                Mesh ringMesh = BuildQuadMesh(0.64f);
                _ownedMeshes.Add(ringMesh);

                Material ringMat = CreateWaterLitMaterial(
                    $"M_SplashRing_{i}",
                    new Color(0.84f, 0.96f, 1.0f, 0.6f),
                    _splashRingTex,
                    null,
                    Vector2.one,
                    0.5f,
                    0.0f,
                    0f,
                    new Color(0.10f, 0.24f, 0.30f)
                );
                _splashMats.Add(ringMat);

                GameObject ringGo = CreateMeshObject(holder.transform, $"SplashRing_{i}", ringMesh, ringMat);
                ringGo.transform.localPosition = new Vector3(dir.x * 1.08f, 0.705f, dir.z * 1.08f);
                _splashRings.Add(ringGo.transform);
            }

            // 2. Two waterfall overflow curtains (Tier 2 -> Tier 1 and Tier 1 -> Main Basin)
            Mesh casc2 = BuildWaterfallCurtainMesh(0.46f, 2.61f, 0.56f, 1.82f, 28, 6);
            _ownedMeshes.Add(casc2);
            CreateMeshObject(holder.transform, "Cascade_Tier2", casc2, _cascadeMat);

            Mesh casc1 = BuildWaterfallCurtainMesh(0.92f, 1.83f, 1.04f, 0.67f, 36, 7);
            _ownedMeshes.Add(casc1);
            CreateMeshObject(holder.transform, "Cascade_Tier1", casc1, _cascadeMat);
        }

        private static GameObject CreateMeshObject(Transform parent, string name, Mesh mesh, Material mat)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        private static Mesh BuildParabolicStreamMesh(Vector3 dir, float rStart, float yStart, float rEnd, float yEnd, float arcLift)
        {
            const int lengthSteps = 14;
            const int radialSegs = 8;

            var verts = new Vector3[(lengthSteps + 1) * (radialSegs + 1)];
            var uvs = new Vector2[verts.Length];
            var tris = new List<int>(lengthSteps * radialSegs * 6);

            Vector3 side = new Vector3(-dir.z, 0f, dir.x).normalized;

            for (int i = 0; i <= lengthSteps; i++)
            {
                float t = (float)i / lengthSteps;
                float r = Mathf.Lerp(rStart, rEnd, t);
                float y = Mathf.Lerp(yStart, yEnd, t * t) + Mathf.Sin(t * Mathf.PI) * arcLift;
                Vector3 center = new Vector3(dir.x * r, y, dir.z * r);

                // Approximate tangent along curve for clean tube cross-section
                float dt = 0.02f;
                float tn = Mathf.Min(1f, t + dt);
                float rn = Mathf.Lerp(rStart, rEnd, tn);
                float yn = Mathf.Lerp(yStart, yEnd, tn * tn) + Mathf.Sin(tn * Mathf.PI) * arcLift;
                Vector3 next = new Vector3(dir.x * rn, yn, dir.z * rn);
                Vector3 tangent = (next - center).sqrMagnitude > 1e-6f ? (next - center).normalized : Vector3.down;
                Vector3 binorm = Vector3.Cross(tangent, side).normalized;

                float tubeRadius = Mathf.Lerp(0.016f, 0.032f, t);

                for (int j = 0; j <= radialSegs; j++)
                {
                    float ang = (float)j / radialSegs * Mathf.PI * 2f;
                    Vector3 offset = (side * Mathf.Cos(ang) + binorm * Mathf.Sin(ang)) * tubeRadius;
                    int idx = i * (radialSegs + 1) + j;
                    verts[idx] = center + offset;
                    uvs[idx] = new Vector2((float)j / radialSegs, t * 2.0f);
                }
            }

            for (int i = 0; i < lengthSteps; i++)
            {
                for (int j = 0; j < radialSegs; j++)
                {
                    int a = i * (radialSegs + 1) + j;
                    int b = a + radialSegs + 1;
                    tris.Add(a); tris.Add(b); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
                }
            }

            Mesh mesh = new Mesh { name = "ParabolicStream_Mesh" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Mesh BuildWaterfallCurtainMesh(float rTop, float yTop, float rBot, float yBot, int radialSegs, int vertSteps)
        {
            var verts = new Vector3[(vertSteps + 1) * (radialSegs + 1)];
            var uvs = new Vector2[verts.Length];
            var tris = new List<int>(vertSteps * radialSegs * 12);

            for (int i = 0; i <= vertSteps; i++)
            {
                float t = (float)i / vertSteps;
                float rBase = Mathf.Lerp(rTop, rBot, Mathf.Sqrt(t));
                float y = Mathf.Lerp(yTop, yBot, t);

                for (int j = 0; j <= radialSegs; j++)
                {
                    float u = (float)j / radialSegs;
                    float ang = u * Mathf.PI * 2f;
                    float ripple = Mathf.Sin(ang * 10f) * 0.014f * t;
                    float r = rBase + ripple;
                    int idx = i * (radialSegs + 1) + j;
                    verts[idx] = new Vector3(Mathf.Cos(ang) * r, y, Mathf.Sin(ang) * r);
                    uvs[idx] = new Vector2(u * 3f, t);
                }
            }

            for (int i = 0; i < vertSteps; i++)
            {
                for (int j = 0; j < radialSegs; j++)
                {
                    int a = i * (radialSegs + 1) + j;
                    int b = a + radialSegs + 1;
                    // Front + back faces so waterfall curtain is visible from outside and inside
                    tris.Add(a); tris.Add(b); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
                    tris.Add(a); tris.Add(a + 1); tris.Add(b);
                    tris.Add(a + 1); tris.Add(b + 1); tris.Add(b);
                }
            }

            Mesh mesh = new Mesh { name = "WaterfallCurtain_Mesh" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Mesh BuildQuadMesh(float size)
        {
            float h = size * 0.5f;
            Mesh mesh = new Mesh { name = "SplashRing_Quad" };
            mesh.SetVertices(new[]
            {
                new Vector3(-h, 0f, -h),
                new Vector3(h, 0f, -h),
                new Vector3(h, 0f, h),
                new Vector3(-h, 0f, h),
            });
            mesh.SetUVs(0, new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            });
            mesh.SetTriangles(new[] { 0, 2, 1, 0, 3, 2 }, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Material CreateWaterLitMaterial(
            string matName,
            Color baseColor,
            Texture2D baseTex,
            Texture2D normalTex,
            Vector2 tiling,
            float smoothness,
            float metallic,
            float bumpScale,
            Color emission)
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) lit = Shader.Find("Standard");

            Material mat = new Material(lit) { name = matName };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            else mat.color = baseColor;

            if (baseTex != null)
            {
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", baseTex);
                    mat.SetTextureScale("_BaseMap", tiling);
                }
                else
                {
                    mat.mainTexture = baseTex;
                    mat.mainTextureScale = tiling;
                }
            }

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            if (normalTex != null && mat.HasProperty("_BumpMap"))
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normalTex);
                mat.SetTextureScale("_BumpMap", tiling);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", bumpScale);
            }

            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);
            return mat;
        }

        private static void SetTexOffset(Material mat, string prop, Vector2 offset)
        {
            if (mat.HasProperty(prop)) mat.SetTextureOffset(prop, offset);
        }

        private static Texture2D GenerateWaveNormalMap(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;

            float[,] h = new float[size, size];
            for (int y = 0; y < size; y++)
            {
                float v = (float)y / size * Mathf.PI * 2f;
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size * Mathf.PI * 2f;
                    h[x, y] =
                        Mathf.Sin(u * 2f + v * 1f) * 0.35f +
                        Mathf.Cos(u * 3f - v * 2f) * 0.25f +
                        Mathf.Sin(u * 5f + v * 4f) * 0.15f +
                        Mathf.Cos(u * 7f - v * 6f) * 0.08f;
                }
            }

            Color[] pixels = new Color[size * size];
            const float strength = 2.4f;
            for (int y = 0; y < size; y++)
            {
                int yPrev = (y - 1 + size) % size;
                int yNext = (y + 1) % size;
                for (int x = 0; x < size; x++)
                {
                    int xPrev = (x - 1 + size) % size;
                    int xNext = (x + 1) % size;

                    float dx = (h[xNext, y] - h[xPrev, y]) * strength;
                    float dy = (h[x, yNext] - h[x, yPrev]) * strength;
                    Vector3 n = new Vector3(-dx, -dy, 1f).normalized;
                    pixels[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateFlowingStreamTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                float v = (float)y / size * Mathf.PI * 2f;
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size * Mathf.PI * 2f;
                    float streak = 0.5f + 0.5f * Mathf.Sin(u * 6f + Mathf.Sin(v * 2f) * 1.4f);
                    float foam = Mathf.Pow(Mathf.Clamp01(0.5f + 0.5f * Mathf.Cos(v * 4f + u * 2f) * streak), 2.0f);
                    Color c = Color.Lerp(new Color(0.24f, 0.72f, 0.85f, 0.52f), new Color(0.92f, 0.99f, 1.0f, 0.92f), foam * 0.8f);
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateSplashRingTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[size * size];
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float dist = Mathf.Sqrt(nx * nx + ny * ny);
                    // Ring peaks around dist = 0.72 and fades smoothly inside & outside
                    float ring = Mathf.Exp(-Mathf.Pow((dist - 0.72f) / 0.16f, 2f));
                    float coreMist = Mathf.Clamp01(1f - dist) * 0.3f;
                    float alpha = Mathf.Clamp01(ring + coreMist);
                    pixels[y * size + x] = new Color(0.90f, 0.98f, 1.0f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}

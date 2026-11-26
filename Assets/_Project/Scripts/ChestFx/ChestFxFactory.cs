using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Builds every visual of the chest reveal in code: materials, particle textures, ray and ring meshes and
    /// particle systems. Nothing here needs a prefab or a binary asset.
    /// Shaders come ONLY from <see cref="ChestShaderResolver"/> (a checked fallback chain, never the error shader).
    /// When no shader can be used, Create* returns null and the caller turns that piece of the effect OFF instead of
    /// drawing it magenta. In a player build keep at least one material using "Universal Render Pipeline/Unlit" and
    /// "Universal Render Pipeline/Particles/Unlit" in the project (or Tools/Cho Do La/Mo Ruong/Tao Vat Lieu Giu Shader),
    /// or add them to Graphics settings > Always Included Shaders.
    /// </summary>
    public static class ChestFxFactory
    {
        private static readonly Dictionary<ParticleLook, Texture2D> Textures = new Dictionary<ParticleLook, Texture2D>();

        // ------------------------------------------------------------------ Materials

        /// <summary>
        /// Opaque unlit colour (not affected by lights). cullFront = true draws only back faces (inverted-hull rim).
        /// Returns NULL when no usable shader exists (or when a front-culled rim is wanted but the shader cannot cull): callers must handle null.
        /// </summary>
        public static Material CreateUnlit(Color color, bool cullFront, string name)
        {
            Shader shader = ChestShaderResolver.Resolve(ChestShaderKind.Unlit);
            if (shader == null) return null;
            Material m = new Material(shader) { name = name, hideFlags = HideFlags.DontSave };
            if (cullFront && !m.HasProperty("_Cull"))
            {
                // Shader dự phòng không cắt được mặt trước: viền kiểu "vỏ lật" sẽ che mất phần ruột, nên bỏ viền thay vì vẽ sai.
                Object.Destroy(m);
                return null;
            }
            SetColor(m, color);
            if (m.HasProperty("_Cull")) m.SetFloat("_Cull", cullFront ? (float)CullMode.Front : (float)CullMode.Back);
            return m;
        }

        /// <summary>
        /// Additive transparent particle material (also used for rays, rings and the flash).
        /// Returns NULL when no usable shader exists: callers must switch that effect off (never render it with a null/error material).
        /// </summary>
        public static Material CreateAdditive(Texture texture, string name)
        {
            Shader shader = ChestShaderResolver.Resolve(ChestShaderKind.Additive);
            if (shader == null) return null;
            Material m = new Material(shader) { name = name, hideFlags = HideFlags.DontSave };
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);           // transparent
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 2f);               // additive
            if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)BlendMode.One);
            if (m.HasProperty("_SrcBlendAlpha")) m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            if (m.HasProperty("_DstBlendAlpha")) m.SetFloat("_DstBlendAlpha", (float)BlendMode.One);
            if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);
            if (m.HasProperty("_Cull")) m.SetFloat("_Cull", (float)CullMode.Off);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)RenderQueue.Transparent;
            if (texture != null)
            {
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", texture);
                if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", texture);
            }
            SetColor(m, Color.white);
            return m;
        }

        public static void SetColor(Material m, Color c)
        {
            if (m == null) return;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            // Shader "Legacy Shaders/Particles/Additive" nhân 2 với _TintColor: đặt nửa giá trị để ra đúng màu (alpha giữ nguyên).
            if (m.HasProperty("_TintColor")) m.SetColor("_TintColor", new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, c.a * 0.5f));
        }

        /// <summary>
        /// Gán vật liệu cho ParticleSystemRenderer. Nếu <paramref name="material"/> null thì TẮT renderer (và dừng hệ hạt) và ghi cảnh báo
        /// nêu tên <paramref name="systemName"/>, để không bao giờ có hệ hạt vẽ bằng vật liệu thiếu (màu tím). Trả true nếu gán được.
        /// </summary>
        public static bool AssignParticleMaterial(ParticleSystemRenderer renderer, Material material, string systemName)
        {
            if (renderer == null) return false;
            if (material == null)
            {
                renderer.enabled = false;
                ParticleSystem ps = renderer.GetComponent<ParticleSystem>();
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                Debug.LogWarning("[ChestFx] Hệ hạt \"" + systemName + "\" bị TẮT vì không có vật liệu/shader dùng được (pipeline: "
                    + ChestShaderResolver.PipelineName + "). Chạy Tools/Cho Do La/Mo Ruong/Kiem Tra Shader để biết shader nào thiếu.", renderer);
                return false;
            }
            renderer.sharedMaterial = material;
            renderer.enabled = true;
            return true;
        }

        // ------------------------------------------------------------------ Textures

        /// <summary>White texture with the particle's shape in alpha (cached).</summary>
        public static Texture2D GetTexture(ParticleLook look)
        {
            if (Textures.TryGetValue(look, out Texture2D cached) && cached != null) return cached;

            const int n = 64;
            Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false)
            {
                name = "ChestFx_" + look,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave,
            };
            Color32[] pixels = new Color32[n * n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2f - 1f;
                    float v = (y + 0.5f) / n * 2f - 1f;
                    float r = Mathf.Sqrt(u * u + v * v);
                    float a;
                    switch (look)
                    {
                        case ParticleLook.Sparkle:
                        {
                            float cross = Mathf.Exp(-Mathf.Abs(u) * 14f) * Mathf.Exp(-Mathf.Abs(v) * 2.2f)
                                        + Mathf.Exp(-Mathf.Abs(v) * 14f) * Mathf.Exp(-Mathf.Abs(u) * 2.2f);
                            float core = Mathf.Exp(-r * r * 30f);
                            a = Mathf.Clamp01(cross + core) * Mathf.Clamp01(1f - r);
                            break;
                        }
                        case ParticleLook.Coin:
                        {
                            float edge = Mathf.Clamp01((1f - r) * 14f);                     // soft outer edge
                            float rim = Mathf.Clamp01(1f - Mathf.Abs(r - 0.82f) * 9f);        // bright rim
                            float shine = Mathf.Clamp01(1f - Mathf.Abs(u + v + 0.35f) * 4f) * 0.35f; // diagonal glint
                            a = edge * Mathf.Clamp01(0.5f + rim * 0.5f + shine);
                            break;
                        }
                        default:
                        {
                            float d = Mathf.Clamp01(1f - r);
                            a = d * d;
                            break;
                        }
                    }
                    pixels[y * n + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            Textures[look] = tex;
            return tex;
        }

        // ------------------------------------------------------------------ Meshes

        /// <summary>
        /// Star of rays in the XY plane, tip radius 1. Vertex alpha fades from 1 at the base to 0 at the tip.
        /// Uses UV (0.5, 0.5) everywhere so a soft-dot or white texture reads as solid.
        /// </summary>
        public static Mesh CreateRayMesh(int count, RayShape shape)
        {
            List<Vector3> verts = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<int> tris = new List<int>();
            count = Mathf.Max(1, count);

            for (int i = 0; i < count; i++)
            {
                float angle = Mathf.PI * 0.5f + i * Mathf.PI * 2f / count;
                Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                Vector3 side = new Vector3(-dir.y, dir.x, 0f);
                float tip = shape == RayShape.Star && (i % 2 == 1) ? 0.55f : 1f;

                switch (shape)
                {
                    case RayShape.Stub:
                        AddQuad(verts, colors, tris, dir, side, 0.12f, 0.6f, 0.09f, 0.09f);
                        break;
                    case RayShape.Beam:
                        AddQuad(verts, colors, tris, dir, side, 0.1f, 1f, 0.045f, 0.045f);
                        break;
                    case RayShape.Diamond:
                    {
                        int b = verts.Count;
                        verts.Add(dir * 0.1f); colors.Add(new Color(1f, 1f, 1f, 1f));
                        verts.Add(dir * 0.45f + side * 0.11f); colors.Add(new Color(1f, 1f, 1f, 0.8f));
                        verts.Add(dir * 1f); colors.Add(new Color(1f, 1f, 1f, 0f));
                        verts.Add(dir * 0.45f - side * 0.11f); colors.Add(new Color(1f, 1f, 1f, 0.8f));
                        tris.AddRange(new[] { b, b + 1, b + 2, b, b + 2, b + 3 });
                        break;
                    }
                    default: // Spike, Star
                    {
                        int b = verts.Count;
                        verts.Add(dir * 0.08f + side * 0.07f); colors.Add(Color.white);
                        verts.Add(dir * tip); colors.Add(new Color(1f, 1f, 1f, 0f));
                        verts.Add(dir * 0.08f - side * 0.07f); colors.Add(Color.white);
                        tris.AddRange(new[] { b, b + 1, b + 2 });
                        break;
                    }
                }
            }

            Mesh mesh = new Mesh { name = "ChestFx_Rays_" + shape + count, hideFlags = HideFlags.DontSave };
            mesh.SetVertices(verts);
            mesh.SetColors(colors);
            mesh.SetUVs(0, Filled(verts.Count, new Vector2(0.5f, 0.5f)));
            mesh.SetTriangles(DoubleSided(tris), 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Flat ring in the XZ plane (outer radius 1), for the shockwave rings.</summary>
        public static Mesh CreateRingMesh(float innerRadius, int segments = 48)
        {
            List<Vector3> verts = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<int> tris = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                Vector3 d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                verts.Add(d * innerRadius); colors.Add(new Color(1f, 1f, 1f, 0f));
                verts.Add(d * Mathf.Lerp(innerRadius, 1f, 0.7f)); colors.Add(Color.white);
                verts.Add(d); colors.Add(new Color(1f, 1f, 1f, 0f));
                if (i == segments) break;
                int b = i * 3;
                tris.AddRange(new[] { b, b + 3, b + 1, b + 1, b + 3, b + 4, b + 1, b + 4, b + 2, b + 2, b + 4, b + 5 });
            }
            Mesh mesh = new Mesh { name = "ChestFx_Ring", hideFlags = HideFlags.DontSave };
            mesh.SetVertices(verts);
            mesh.SetColors(colors);
            mesh.SetUVs(0, Filled(verts.Count, new Vector2(0.5f, 0.5f)));
            mesh.SetTriangles(DoubleSided(tris), 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Unit quad in the XY plane with full UVs (flash billboard).</summary>
        public static Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh { name = "ChestFx_Quad", hideFlags = HideFlags.DontSave };
            mesh.SetVertices(new List<Vector3> { new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0), new Vector3(-1, 1, 0) });
            mesh.SetUVs(0, new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
            mesh.SetColors(new List<Color> { Color.white, Color.white, Color.white, Color.white });
            mesh.SetTriangles(DoubleSided(new List<int> { 0, 2, 1, 0, 3, 2 }), 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddQuad(List<Vector3> verts, List<Color> colors, List<int> tris, Vector3 dir, Vector3 side, float r0, float r1, float w0, float w1)
        {
            int b = verts.Count;
            verts.Add(dir * r0 + side * w0); colors.Add(Color.white);
            verts.Add(dir * r1 + side * w1); colors.Add(new Color(1f, 1f, 1f, 0f));
            verts.Add(dir * r1 - side * w1); colors.Add(new Color(1f, 1f, 1f, 0f));
            verts.Add(dir * r0 - side * w0); colors.Add(Color.white);
            tris.AddRange(new[] { b, b + 1, b + 2, b, b + 2, b + 3 });
        }

        private static List<Vector2> Filled(int count, Vector2 value)
        {
            List<Vector2> list = new List<Vector2>(count);
            for (int i = 0; i < count; i++) list.Add(value);
            return list;
        }

        private static List<int> DoubleSided(List<int> tris)
        {
            List<int> both = new List<int>(tris);
            for (int i = 0; i < tris.Count; i += 3)
            {
                both.Add(tris[i]);
                both.Add(tris[i + 2]);
                both.Add(tris[i + 1]);
            }
            return both;
        }

        /// <summary>
        /// CÁCH DUY NHẤT trong dự án để đặt <c>velocityOverLifetime</c> (có kiểm: Tools/CompileCheck/check_particle_velocity.sh).
        /// Unity báo lỗi "Particle Velocity curves must all be in the same mode" / "Orbital Velocity curves must all be in
        /// the same mode" khi x/y/z (hoặc orbitalX/Y/Z, hoặc orbitalOffsetX/Y/Z) ở chế độ khác nhau, kể cả khi chỉ một trục có
        /// giá trị. Hàm này đặt CẢ 9 đường cong cùng chế độ TwoConstants (min = max nghĩa là không ngẫu nhiên), nên không
        /// bao giờ lệch chế độ. <paramref name="radial"/> là đường riêng (không thuộc nhóm trên) nên để hằng số.
        /// </summary>
        public static void SetVelocity(ParticleSystem ps, Vector3 linearMin, Vector3 linearMax, Vector3 orbitalMin, Vector3 orbitalMax, float radial, ParticleSystemSimulationSpace space)
        {
            if (ps == null) return;
            ParticleSystem.VelocityOverLifetimeModule v = ps.velocityOverLifetime;
            v.enabled = true;
            v.space = space;
            v.x = new ParticleSystem.MinMaxCurve(linearMin.x, linearMax.x);
            v.y = new ParticleSystem.MinMaxCurve(linearMin.y, linearMax.y);
            v.z = new ParticleSystem.MinMaxCurve(linearMin.z, linearMax.z);
            v.orbitalX = new ParticleSystem.MinMaxCurve(orbitalMin.x, orbitalMax.x);
            v.orbitalY = new ParticleSystem.MinMaxCurve(orbitalMin.y, orbitalMax.y);
            v.orbitalZ = new ParticleSystem.MinMaxCurve(orbitalMin.z, orbitalMax.z);
            v.orbitalOffsetX = new ParticleSystem.MinMaxCurve(0f, 0f);
            v.orbitalOffsetY = new ParticleSystem.MinMaxCurve(0f, 0f);
            v.orbitalOffsetZ = new ParticleSystem.MinMaxCurve(0f, 0f);
            v.radial = new ParticleSystem.MinMaxCurve(radial);
        }

        // ------------------------------------------------------------------ Particle systems

        /// <summary>
        /// Creates a stopped particle system (world simulation space) styled by the profile.
        /// burst = true: no continuous emission, call Emit(); otherwise set the emission rate yourself.
        /// Returns NULL (after a warning naming the system) when no usable shader exists: the particles are switched off, not drawn magenta.
        /// </summary>
        public static ParticleSystem CreateParticles(string name, Transform parent, ChestFxProfile profile, float chestSize, bool burst)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            float speed = profile.particleSpeed * (burst ? 2.2f : 1f);
            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.loop = !burst;
            main.duration = burst ? 1f : 5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(profile.particleLifetime * 0.6f, profile.particleLifetime * (burst ? 1.4f : 1f));
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(profile.particleSize * 0.6f, profile.particleSize * (burst ? 1.6f : 1.2f));
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.maxParticles = 600;
            main.gravityModifier = burst ? profile.burstGravity : (profile.particleDirection == ParticleDirection.Fountain ? profile.burstGravity * 0.5f : 0f);

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            float radius = Mathf.Max(0.01f, chestSize * 0.25f);
            switch (profile.particleDirection)
            {
                case ParticleDirection.Radial:
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = radius * 0.4f;
                    break;
                case ParticleDirection.Fountain:
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.angle = burst ? 32f : 20f;
                    shape.radius = radius * 0.5f;
                    shape.rotation = new Vector3(-90f, 0f, 0f);
                    if (burst) main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.8f, speed * 1.4f);
                    break;
                default: // Up, Swirl
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.angle = burst ? 40f : 15f;
                    shape.radius = radius * 0.7f;
                    shape.rotation = new Vector3(-90f, 0f, 0f);
                    break;
            }

            if (profile.particleDirection == ParticleDirection.Swirl)
            {
                // Mọi chỗ đặt velocityOverLifetime phải đi qua SetVelocity (xem giải thích ở hàm đó).
                SetVelocity(ps, Vector3.zero, Vector3.zero, new Vector3(0f, 2.5f, 0f), new Vector3(0f, 2.5f, 0f), 0.05f, ParticleSystemSimulationSpace.Local);
            }

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.08f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(fade);

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(0.2f, 1f), new Keyframe(1f, 0.2f)));

            if (profile.particleLook == ParticleLook.Sparkle || profile.particleLook == ParticleLook.Coin)
            {
                ParticleSystem.RotationOverLifetimeModule rot = ps.rotationOverLifetime;
                rot.enabled = true;
                rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
            }

            ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
            Material mat = CreateAdditive(GetTexture(profile.particleLook == ParticleLook.Streak ? ParticleLook.SoftDot : profile.particleLook), name + "_Mat");
            if (!AssignParticleMaterial(psr, mat, name))
            {
                // Không có shader dùng được: bỏ hẳn hệ hạt này (đã cảnh báo), người gọi nhận null.
                Object.Destroy(go);
                return null;
            }
            psr.shadowCastingMode = ShadowCastingMode.Off;
            psr.receiveShadows = false;
            if (profile.particleLook == ParticleLook.Streak)
            {
                psr.renderMode = ParticleSystemRenderMode.Stretch;
                psr.lengthScale = 2.5f;
                psr.velocityScale = 0.08f;
            }
            else
            {
                psr.renderMode = ParticleSystemRenderMode.Billboard;
            }
            return ps;
        }

        /// <summary>Two-colour start colour: the tier colour and a mix toward the profile's secondary colour.</summary>
        public static void SetParticleColor(ParticleSystem ps, Color tierColor, ChestFxProfile profile, float intensity)
        {
            if (ps == null) return;
            ParticleSystem.MainModule main = ps.main;
            Color a = tierColor * intensity;
            Color b = Color.Lerp(tierColor, profile.secondaryColor, Mathf.Clamp01(profile.secondaryMix * 2f)) * intensity;
            a.a = 1f;
            b.a = 1f;
            main.startColor = new ParticleSystem.MinMaxGradient(a, b);
        }
    }
}

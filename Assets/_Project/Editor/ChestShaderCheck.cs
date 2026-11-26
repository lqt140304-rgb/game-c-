#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Công cụ tự kiểm lỗi màu TÍM (magenta) của hiệu ứng hạt/tia/vòng/bóng. KHÔNG sửa ProjectSettings hay Packages/manifest.json
    /// (chỉ ĐỌC GraphicsSettings.asset). Ba menu dưới Tools/Cho Do La/Mo Ruong:
    ///  - Kiem Tra Shader: in pipeline đang chạy, từng shader ứng viên (tìm thấy/không, hỗ trợ/không), dựng thử một hệ hạt và một mesh,
    ///    báo shader nào được dùng, và xem shader URP đã có trong "Always Included Shaders" chưa.
    ///  - Quet Vat Lieu Tim: tìm trong các cảnh đang mở (chạy cả lúc Play) mọi Renderer có vật liệu null / shader lỗi, in ĐƯỜNG DẪN
    ///    đối tượng bị tím. Bấm lúc rương đang mở để bắt đúng hệ hạt/tia/vòng lỗi.
    ///  - Tao Vat Lieu Giu Shader: tạo vài vật liệu trong Assets/_Project/Resources/ChestFx để Unity luôn đưa các shader hiệu ứng vào bản build
    ///    (shader bị gỡ khỏi bản build là nguyên nhân phổ biến của màu tím chỉ có ở bản build).
    /// </summary>
    public static class ChestShaderCheck
    {
        private const string Log = "[KiemTraShader] ";
        private const string KeepFolder = "Assets/_Project/Resources/ChestFx";

        [MenuItem("Tools/Cho Do La/Mo Ruong/Kiem Tra Shader")]
        public static void Check()
        {
            ChestShaderResolver.ResetCache();
            StringBuilder sb = new StringBuilder();
            RenderPipelineAsset pipe = GraphicsSettings.currentRenderPipeline;
            sb.AppendLine(Log + "Pipeline đang chạy: " + ChestShaderResolver.PipelineName
                + (pipe != null ? " (asset: " + pipe.name + ")" : " (chưa gán Render Pipeline Asset trong Project Settings > Graphics/Quality: shader URP sẽ TÍM)"));
            if (pipe == null)
                sb.AppendLine(Log + "LƯU Ý: dự án có gói URP nhưng không có Render Pipeline Asset đang gán, nên mọi shader 'Universal Render Pipeline/...' vẽ màu tím. "
                    + "Hiệu ứng đã tự chuyển sang shader Built-in, nhưng nên gán URP Asset (Project Settings > Graphics) cho đúng.");

            bool allOk = true;
            foreach (ChestShaderKind kind in new[] { ChestShaderKind.Unlit, ChestShaderKind.Additive, ChestShaderKind.Lit })
            {
                List<string> rows = new List<string>();
                Shader pick = ChestShaderResolver.Probe(kind, null, rows);
                sb.AppendLine(Log + "== Loại " + kind + (pick != null ? "  → CHỌN: " + pick.name : "  → KHÔNG CÓ SHADER NÀO DÙNG ĐƯỢC (phần hiệu ứng này sẽ bị tắt)"));
                foreach (string r in rows) sb.AppendLine(Log + "     " + r);
                if (pick == null && kind != ChestShaderKind.Lit) allOk = false;
            }

            sb.AppendLine(Log + TrialParticle());
            sb.AppendLine(Log + TrialMesh());
            sb.AppendLine(Log + AlwaysIncluded());

            if (allOk) Debug.Log(sb.ToString());
            else Debug.LogWarning(sb.ToString());
        }

        private static string TrialParticle()
        {
            GameObject go = new GameObject("ChestShaderTrial_Particle");
            go.hideFlags = HideFlags.DontSave;
            try
            {
                ParticleSystem ps = go.AddComponent<ParticleSystem>();
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
                Material m = ChestFxFactory.CreateAdditive(ChestFxFactory.GetTexture(ParticleLook.SoftDot), "Trial");
                bool ok = ChestFxFactory.AssignParticleMaterial(psr, m, "ChestShaderTrial_Particle");
                if (!ok) return "Hệ hạt thử: BỊ TẮT vì không có shader (xem cảnh báo).";
                string sn = psr.sharedMaterial != null && psr.sharedMaterial.shader != null ? psr.sharedMaterial.shader.name : "(null)";
                bool bad = sn.StartsWith("Hidden/") || !psr.sharedMaterial.shader.isSupported;
                return "Hệ hạt thử: vật liệu dùng shader \"" + sn + "\" → " + (bad ? "LỖI (sẽ tím)" : "ổn");
            }
            finally { Object.DestroyImmediate(go); }
        }

        private static string TrialMesh()
        {
            Material add = ChestFxFactory.CreateAdditive(ChestFxFactory.GetTexture(ParticleLook.SoftDot), "TrialAdd");
            Material unlit = ChestFxFactory.CreateUnlit(Color.white, false, "TrialUnlit");
            string a = add != null ? add.shader.name : "null (tia/vòng/chớp bị tắt)";
            string u = unlit != null ? unlit.shader.name : "null (bóng vật phẩm bị tắt)";
            if (add != null) Object.DestroyImmediate(add);
            if (unlit != null) Object.DestroyImmediate(unlit);
            return "Mesh thử: tia/vòng/chớp dùng \"" + a + "\"; bóng vật phẩm dùng \"" + u + "\".";
        }

        /// <summary>Chỉ ĐỌC ProjectSettings/GraphicsSettings.asset, không ghi.</summary>
        private static string AlwaysIncluded()
        {
            Object[] all = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (all == null || all.Length == 0) return "Không đọc được ProjectSettings/GraphicsSettings.asset (bỏ qua mục 'Always Included Shaders').";
            List<string> names = new List<string>();
            foreach (Object o in all)
            {
                if (o == null) continue;
                SerializedObject so = new SerializedObject(o);
                SerializedProperty list = so.FindProperty("m_AlwaysIncludedShaders");
                if (list == null || !list.isArray) continue;
                for (int i = 0; i < list.arraySize; i++)
                {
                    Object s = list.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (s != null) names.Add(s.name);
                }
            }
            bool hasP = names.Contains("Universal Render Pipeline/Particles/Unlit");
            bool hasU = names.Contains("Universal Render Pipeline/Unlit");
            return "Always Included Shaders: " + (names.Count == 0 ? "(trống)" : string.Join(", ", names))
                + ". URP Particles/Unlit " + (hasP ? "có" : "KHÔNG có") + ", URP Unlit " + (hasU ? "có" : "KHÔNG có")
                + (hasP && hasU ? "." : " → bản build có thể gỡ mất chúng; chạy menu 'Tao Vat Lieu Giu Shader' (không sửa ProjectSettings) hoặc thêm tay.");
        }

        [MenuItem("Tools/Cho Do La/Mo Ruong/Quet Vat Lieu Tim")]
        public static void ScanPink()
        {
            List<string> bad = new List<string>();
            int total = 0;
            foreach (Renderer r in Resources.FindObjectsOfTypeAll<Renderer>())
            {
                if (r == null || EditorUtility.IsPersistent(r) || !r.gameObject.scene.IsValid()) continue;
                total++;
                string why = null;
                Material[] mats = r.sharedMaterials;
                if (mats == null || mats.Length == 0) why = "không có vật liệu";
                else
                {
                    foreach (Material m in mats)
                    {
                        if (m == null) { why = "vật liệu null"; break; }
                        if (m.shader == null) { why = "shader null"; break; }
                        if (m.shader.name.StartsWith("Hidden/InternalErrorShader")) { why = "shader lỗi (InternalErrorShader)"; break; }
                        if (!m.shader.isSupported) { why = "shader \"" + m.shader.name + "\" không hỗ trợ (isSupported = false)"; break; }
                        if (m.shader.name.StartsWith("Universal Render Pipeline/") && GraphicsSettings.currentRenderPipeline == null)
                        { why = "shader URP \"" + m.shader.name + "\" nhưng chưa gán URP Asset"; break; }
                    }
                }
                if (why != null) bad.Add(PathOf(r.transform) + "  [" + r.GetType().Name + "]  →  " + why);
            }
            if (bad.Count == 0) Debug.Log(Log + "Quét " + total + " Renderer trong cảnh đang mở: KHÔNG thấy vật liệu lỗi.");
            else Debug.LogWarning(Log + "Quét " + total + " Renderer, " + bad.Count + " cái sẽ vẽ màu tím:\n" + string.Join("\n", bad));
        }

        private static string PathOf(Transform t)
        {
            string p = t.name;
            for (Transform c = t.parent; c != null; c = c.parent) p = c.name + "/" + p;
            return p;
        }

        [MenuItem("Tools/Cho Do La/Mo Ruong/Tao Vat Lieu Giu Shader")]
        public static void CreateKeepMaterials()
        {
            EnsureFolder(KeepFolder);
            string[][] items =
            {
                new[] { "KeepShader_UrpParticlesUnlit", "Universal Render Pipeline/Particles/Unlit", "add" },
                new[] { "KeepShader_UrpUnlit", "Universal Render Pipeline/Unlit", "opaque" },
                new[] { "KeepShader_UrpUnlitAdd", "Universal Render Pipeline/Unlit", "add" },
                new[] { "KeepShader_SpritesDefault", "Sprites/Default", "opaque" },
                new[] { "KeepShader_LegacyAdditive", "Legacy Shaders/Particles/Additive", "opaque" },
            };
            int made = 0;
            foreach (string[] it in items)
            {
                Shader sh = Shader.Find(it[1]);
                if (sh == null) { Debug.LogWarning(Log + "Bỏ qua " + it[0] + ": không tìm thấy shader \"" + it[1] + "\" trong dự án này."); continue; }
                string path = KeepFolder + "/" + it[0] + ".mat";
                Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
                bool isNew = m == null;
                if (isNew) m = new Material(sh); else m.shader = sh;
                if (it[2] == "add")
                {
                    // đúng các khoá mà ChestFxFactory.CreateAdditive bật, để biến thể này không bị gỡ
                    if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
                    if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 2f);
                    m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    m.SetOverrideTag("RenderType", "Transparent");
                    m.renderQueue = (int)RenderQueue.Transparent;
                }
                if (isNew) AssetDatabase.CreateAsset(m, path); else EditorUtility.SetDirty(m);
                made++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log(Log + "Đã tạo/cập nhật " + made + " vật liệu giữ shader trong " + KeepFolder
                + " (thư mục Resources luôn vào bản build, kéo theo shader của chúng). Không động tới ProjectSettings hay Packages.");
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
#endif

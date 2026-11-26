using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoDoLa.ChestFx
{
    /// <summary>Loại vật liệu mà hiệu ứng rương cần.</summary>
    public enum ChestShaderKind
    {
        /// <summary>Màu phẳng không ăn đèn (bóng vật phẩm, chấm bậc).</summary>
        Unlit,
        /// <summary>Trong suốt cộng sáng (hạt, tia, vòng, chớp).</summary>
        Additive,
        /// <summary>Có ăn đèn (vật Rigidbody của kịch bản vật lý).</summary>
        Lit,
    }

    /// <summary>
    /// NƠI DUY NHẤT tìm shader cho hiệu ứng rương (kiểm bằng Tools/CompileCheck/check_chest_core.sh: không file nào khác được gọi Shader.Find).
    ///
    /// Vì sao có lớp này: Unity vẽ màu TÍM (magenta) khi vật liệu dùng shader null, "Hidden/InternalErrorShader", shader bị gỡ khỏi bản build,
    /// shader của URP trong khi dự án đang chạy pipeline khác (hoặc chưa gán Render Pipeline Asset), hoặc shader không có pass hợp lệ.
    /// Mã cũ rơi thẳng vào Hidden/InternalErrorShader khi mọi shader đều thiếu, nên hiện đúng màu tím. Lớp này:
    ///  1. xét pipeline đang chạy (URP hay Built-in) và chỉ thử shader hợp pipeline đó;
    ///  2. duyệt một CHUỖI dự phòng, mỗi ứng viên phải tìm thấy, <c>isSupported</c> và không phải shader "Hidden/";
    ///  3. nếu KHÔNG shader nào dùng được thì trả null: người gọi phải TẮT phần hiệu ứng đó (không bao giờ vẽ shader lỗi);
    ///  4. ghi MỘT cảnh báo vàng nêu rõ shader nào thử và vì sao hỏng.
    /// Kết quả được nhớ lại (dọn bằng <see cref="ResetCache"/>, Editor gọi khi đổi pipeline).
    /// </summary>
    public static class ChestShaderResolver
    {
        private enum Need { Any, UrpOnly, BuiltInOnly }

        private struct Candidate
        {
            public string name;
            public Need need;
            public Candidate(string n, Need r) { name = n; need = r; }
        }

        private static readonly Dictionary<ChestShaderKind, Candidate[]> Lists = new Dictionary<ChestShaderKind, Candidate[]>
        {
            { ChestShaderKind.Unlit, new[]
                {
                    new Candidate("Universal Render Pipeline/Unlit", Need.UrpOnly),
                    new Candidate("Universal Render Pipeline/Simple Lit", Need.UrpOnly),
                    new Candidate("Unlit/Color", Need.Any),
                    new Candidate("Sprites/Default", Need.Any),
                    new Candidate("UI/Default", Need.Any),
                } },
            { ChestShaderKind.Additive, new[]
                {
                    new Candidate("Universal Render Pipeline/Particles/Unlit", Need.UrpOnly),
                    new Candidate("Universal Render Pipeline/Unlit", Need.UrpOnly),
                    new Candidate("Legacy Shaders/Particles/Additive", Need.Any),
                    new Candidate("Mobile/Particles/Additive", Need.Any),
                    new Candidate("Sprites/Default", Need.Any),
                    new Candidate("Particles/Standard Unlit", Need.BuiltInOnly),
                } },
            { ChestShaderKind.Lit, new[]
                {
                    new Candidate("Universal Render Pipeline/Lit", Need.UrpOnly),
                    new Candidate("Universal Render Pipeline/Simple Lit", Need.UrpOnly),
                    new Candidate("Standard", Need.BuiltInOnly),
                } },
        };

        private static readonly Dictionary<ChestShaderKind, Shader> Cache = new Dictionary<ChestShaderKind, Shader>();
        private static readonly HashSet<ChestShaderKind> Resolved = new HashSet<ChestShaderKind>();
        private static readonly HashSet<ChestShaderKind> Warned = new HashSet<ChestShaderKind>();

        /// <summary>Pipeline đang chạy: "URP", "Built-in" hoặc "SRP khác" (ví dụ HDRP).</summary>
        public static string PipelineName
        {
            get
            {
                RenderPipelineAsset a = GraphicsSettings.currentRenderPipeline;
                if (a == null) return "Built-in";
                return IsUrp(a) ? "URP" : "SRP khác";
            }
        }

        private static bool IsUrp(RenderPipelineAsset a)
        {
            return a != null && a.GetType().Name.IndexOf("Universal", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Quên kết quả đã nhớ (gọi sau khi đổi Render Pipeline Asset hoặc trong menu kiểm của Editor).</summary>
        public static void ResetCache()
        {
            Cache.Clear();
            Resolved.Clear();
            Warned.Clear();
        }

        /// <summary>
        /// Shader dùng được cho loại này, hoặc null nếu không có cái nào (người gọi phải tắt phần hiệu ứng, không vẽ shader lỗi).
        /// </summary>
        public static Shader Resolve(ChestShaderKind kind)
        {
            if (Resolved.Contains(kind)) { Shader c; return Cache.TryGetValue(kind, out c) ? c : null; }
            Resolved.Add(kind);

            StringBuilder why = new StringBuilder();
            Shader found = Probe(kind, why, null);
            if (found != null) Cache[kind] = found;
            else if (Warned.Add(kind))
            {
                Debug.LogWarning("[ChestFx] Không có shader nào dùng được cho loại \"" + kind + "\" (pipeline: " + PipelineName + "). "
                    + "Phần hiệu ứng dùng nó (hạt/tia/vòng/bóng) sẽ bị TẮT thay vì hiện màu tím. Đã thử: " + why
                    + "Cách sửa: giữ shader trong bản build (xem docs/superpowers/specs/2026-11-27-ruong-am-thanh-dong-bo-design.md, mục 8; "
                    + "Editor: Tools/Cho Do La/Mo Ruong/Kiem Tra Shader).");
            }
            return found;
        }

        /// <summary>
        /// Duyệt chuỗi ứng viên. <paramref name="why"/> (nếu có) nhận từng dòng "tên: kết quả"; <paramref name="rows"/> (nếu có) nhận
        /// danh sách (tên, trạng thái) để Editor in ra. Trả shader dùng được đầu tiên hoặc null.
        /// </summary>
        public static Shader Probe(ChestShaderKind kind, StringBuilder why, List<string> rows)
        {
            RenderPipelineAsset pipe = GraphicsSettings.currentRenderPipeline;
            bool urp = IsUrp(pipe);
            bool builtIn = pipe == null;
            Shader first = null;
            foreach (Candidate c in Lists[kind])
            {
                string state;
                Shader s = null;
                if (c.need == Need.UrpOnly && !urp) state = "bỏ qua (cần URP, đang chạy " + PipelineName + ")";
                else if (c.need == Need.BuiltInOnly && !builtIn) state = "bỏ qua (chỉ cho Built-in, đang chạy " + PipelineName + ")";
                else
                {
                    s = Shader.Find(c.name);
                    if (s == null) state = "KHÔNG TÌM THẤY (chưa nạp vào bản build, hoặc sai tên)";
                    else if (s.name.StartsWith("Hidden/")) { state = "là shader ẩn/lỗi"; s = null; }
                    else if (!s.isSupported) { state = "không hỗ trợ trên máy này (isSupported = false)"; s = null; }
                    else state = "DÙNG ĐƯỢC";
                }
                if (why != null) why.Append(c.name).Append(" → ").Append(state).Append("; ");
                if (rows != null) rows.Add(c.name + "  →  " + state);
                if (s != null && first == null) first = s;
            }
            return first;
        }
    }
}

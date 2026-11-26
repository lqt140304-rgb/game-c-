using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Hải tặc: con vẹt hải tặc (thân đỏ, cánh xanh lá, mỏ vàng, mắt đen, đuôi dài).
    /// Đầu/mỏ hướng +Z; cánh vỗ khi bay. Đợt 2: bay lượn zích zắc vỗ cánh, mở bằng 2 nhát mổ rồi giật cạy (ThemeFly/ThemeAction).
    /// </summary>
    public class HaiTacKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = Make();

        static KeyPreset Make()
        {
            var p = KeyPreset.Neutral("Chest_HaiTac", "HaiTac", "Assets/_Project/Audio/Chests/hai_tac/", "sfx_haitac_", "sfx_haitac_bed_loop.mp3",
                new Color(0.85f, 0.12f, 0.1f), KeyAction.Peck);
            p.keyFile = "sfx_haitac_thud.mp3";
            // Đàn hồi như sóng: Squash +10% (cả Rác lẫn các bậc khác).
            p.squashMul = 1.1f; p.racSquashMul = 1.1f;
            // Đợt 2: tiếng vẹt bay + mổ cạy khoá; sáng vàng đèn dầu; rương lắc nghiêng như thuyền.
            p.flyFile = "sfx_haitac_fly.mp3";
            p.actionFile = "sfx_haitac_action.mp3";
            p.lightColor = new Color(1f, 0.66f, 0.25f);
            p.lightMul = 0.55f;
            p.moveStyle = ChestMoveStyle.Wobble;
            return p;
        }

        // Hai trục cánh của vật (theo gốc vật, không static).
        private readonly Dictionary<Transform, Transform[]> _wings = new Dictionary<Transform, Transform[]>();

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color red = new Color(0.85f, 0.12f, 0.1f);
            Color green = new Color(0.15f, 0.7f, 0.25f);
            Color blue = new Color(0.15f, 0.4f, 0.85f);
            Color yellow = new Color(1f, 0.82f, 0.15f);
            Color black = new Color(0.05f, 0.05f, 0.05f);

            var root = new GameObject("HaiTacKey");
            Transform t = root.transform;

            // Thân đỏ, đầu đỏ ở phía +Z.
            c.Part(t, PrimitiveType.Sphere, "Than", new Vector3(0f, 0f, -0.05f), new Vector3(0.38f, 0.42f, 0.5f), red);
            c.Part(t, PrimitiveType.Sphere, "Dau", new Vector3(0f, 0.2f, 0.25f), new Vector3(0.28f, 0.28f, 0.28f), red);
            // Mỏ vàng (quặp xuống) + hai mắt đen.
            c.Part(t, PrimitiveType.Cube, "Mo", new Vector3(0f, 0.17f, 0.42f), new Vector3(0.09f, 0.12f, 0.16f), Quaternion.Euler(25f, 0f, 0f), yellow);
            c.Part(t, PrimitiveType.Sphere, "MatTrai", new Vector3(-0.11f, 0.26f, 0.35f), new Vector3(0.06f, 0.06f, 0.06f), black);
            c.Part(t, PrimitiveType.Sphere, "MatPhai", new Vector3(0.11f, 0.26f, 0.35f), new Vector3(0.06f, 0.06f, 0.06f), black);
            // Đuôi dài: một dải xanh lá + đầu đuôi xanh dương.
            c.Part(t, PrimitiveType.Cube, "Duoi", new Vector3(0f, -0.08f, -0.38f), new Vector3(0.1f, 0.05f, 0.34f), Quaternion.Euler(-12f, 0f, 0f), green);
            c.Part(t, PrimitiveType.Cube, "DuoiMut", new Vector3(0f, -0.12f, -0.52f), new Vector3(0.12f, 0.04f, 0.1f), Quaternion.Euler(-12f, 0f, 0f), blue);

            // Hai cánh xanh lá gắn vào trục (xoay quanh trục dọc thân khi vỗ).
            var pivotL = new GameObject("TrucCanhTrai").transform;
            pivotL.SetParent(t, false);
            pivotL.localPosition = new Vector3(-0.17f, 0.04f, -0.05f);
            var pivotR = new GameObject("TrucCanhPhai").transform;
            pivotR.SetParent(t, false);
            pivotR.localPosition = new Vector3(0.17f, 0.04f, -0.05f);
            c.Part(pivotL, PrimitiveType.Cube, "CanhTrai", new Vector3(-0.15f, 0f, 0f), new Vector3(0.3f, 0.04f, 0.34f), green);
            c.Part(pivotR, PrimitiveType.Cube, "CanhPhai", new Vector3(0.15f, 0f, 0f), new Vector3(0.3f, 0.04f, 0.34f), green);

            _wings[t] = new[] { pivotL, pivotR };
            return root;
        }

        private static float Smooth(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        private void Flap(Transform root, float angle)
        {
            Transform[] w;
            if (root == null || !_wings.TryGetValue(root, out w) || w[0] == null || w[1] == null) return;
            w[0].localRotation = Quaternion.Euler(0f, 0f, angle);
            w[1].localRotation = Quaternion.Euler(0f, 0f, -angle);
        }

        /// <summary>Bay: vẹt lượn zích zắc sang hai bên (biên độ nhỏ dần), nhấp nhô theo nhịp vỗ cánh, nghiêng người theo hướng lượn.</summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            System.Random rng = c.Rng(71);
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;      // lượn sang trái hay phải trước
            float zig = 2.5f + (float)rng.NextDouble() * 0.5f;    // số nửa vòng lượn
            Vector3 p0 = root.position;
            float t0 = Time.unscaledTime;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float e = u * u * (3f - 2f * u);
                Vector3 right = Vector3.Cross(c.up, c.front);
                Vector3 end = c.RestPos + c.front * (0.12f * c.S);  // dừng trước khe 0,12 S để mổ (khớp điểm đầu của ThemeAction)
                // Đường chính: thẳng từ điểm khởi tới trước khe, vồng nhẹ lên giữa đường.
                Vector3 basePos = Vector3.Lerp(p0, end, e) + c.up * (0.25f * c.S * Mathf.Sin(Mathf.PI * e));
                // Zích zắc ngang, nhỏ dần về 0 khi tới nơi.
                float damp = 1f - e;
                float ph = Mathf.PI * zig * u;
                float lat = side * Mathf.Sin(ph) * 0.35f * c.S * damp;
                // Nhấp nhô theo nhịp vỗ cánh (7 nhịp/giây).
                float flapPh = u * seconds * 7f * Mathf.PI * 2f;
                float bob = Mathf.Sin(flapPh) * 0.03f * c.S * damp;
                root.position = basePos + right * lat + c.up * bob;
                // Nghiêng theo hướng lượn (đạo hàm của lat) và chúi đầu xuống lúc gần tới.
                float roll = -side * Mathf.Cos(ph) * 28f * damp;
                float pitch = 12f * e;
                root.rotation = c.Facing * Quaternion.Euler(pitch, 0f, roll);
                Flap(root, Mathf.Sin(flapPh) * Mathf.Lerp(45f, 25f, e));
                if (u >= 1f) break;
                yield return null;
            }
        }

        /// <summary>Mở: mổ khoá hai nhát (gật đầu lao tới), rồi ngửa đầu giật mạnh như cạy, lao vào khe và thu nhỏ.</summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            float t0 = Time.unscaledTime;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float t = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float S = c.S;
                float off, pitch;
                if (t < 0.375f)
                {
                    // Hai nhát mổ từ 0,12 S: chạm khe ở t = 0,125 và 0,375 (≈ 0,04 s và 0,13 s, khớp điểm nhấn tiếng).
                    float k = t / 0.25f;
                    float back = 0.5f + 0.5f * Mathf.Cos(Mathf.PI * 2f * k);   // 1 → 0 → 1 → 0
                    off = 0.12f * S * back;
                    pitch = 30f * (1f - back);                                 // gật mỏ xuống lúc chạm
                }
                else if (t < 0.7f)
                {
                    // Giật cạy: ngửa đầu ra sau, lùi nhẹ, rồi về khe.
                    float k = Smooth(0.375f, 0.55f, t) - Smooth(0.55f, 0.7f, t);
                    off = 0.05f * S * k;
                    pitch = 30f * (1f - Smooth(0.375f, 0.45f, t)) - 35f * k;
                }
                else { off = 0f; pitch = 0f; }

                if (t < 0.7f)
                {
                    root.position = c.RestPos + c.front * off;
                    root.localScale = Vector3.one * c.keySize;
                }
                else ShrinkAtSlot(c, root, t);   // đứng đúng khe, 0,85 → 1 thu nhỏ biến mất
                root.rotation = c.Facing * Quaternion.Euler(pitch, 0f, 0f);
                Flap(root, Mathf.Sin(t * seconds * 14f * Mathf.PI * 2f) * (t < 0.7f ? 18f : 8f));
                if (t >= 1f) break;
                yield return null;
            }
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            if (root == null) return;
            Transform[] w;
            if (!_wings.TryGetValue(root, out w) || w[0] == null || w[1] == null) return;
            // Bay: vỗ nhanh biên độ lớn. Hành động mổ: cánh run nhẹ.
            float freq = InActionPhase ? 10f : 6f;
            float amp = InActionPhase ? 10f : 40f;
            float a = Mathf.Sin(t01 * freq * Mathf.PI * 2f) * amp;
            w[0].localRotation = Quaternion.Euler(0f, 0f, a);
            w[1].localRotation = Quaternion.Euler(0f, 0f, -a);
        }
    }
}

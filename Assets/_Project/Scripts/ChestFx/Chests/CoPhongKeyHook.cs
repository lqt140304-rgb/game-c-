using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Cổ Phong: LÁ BÙA PHONG ẤN (giấy vàng dài, 3 nét chữ đỏ, tua đỏ). Mặt có chữ quay ra ngoài,
    /// mặt sau (+Z) hướng về rương để dán vào khe (hành động Strike). Kế hoạch 2026-11-26, mục 6.16.
    /// </summary>
    public class CoPhongKeyHook : ChestKeyHookBase
    {
        // Thanh thoát: Squash -10% (cả Rác lẫn các bậc khác), Jump +10%.
        // Đợt 2: bay phấp phới như lá rơi, mở = dán bùa (vỗ phẳng) + nháy vàng-đỏ; rương lơ lửng (Float).
        public static readonly KeyPreset Preset = Make();

        static KeyPreset Make()
        {
            var p = KeyPreset.Neutral("Chest_CoPhong", "CoPhong", "Assets/_Project/Audio/Chests/co_phong/", "sfx_cophong_",
                "sfx_cophong_sword_hum_loop.mp3", new Color(0.95f, 0.85f, 0.4f), KeyAction.Strike);
            p.keyFile = "sfx_cophong_bamboo_knock.mp3";
            p.flyFile = "sfx_cophong_fly.mp3";
            p.actionFile = "sfx_cophong_action.mp3";
            p.squashMul = 0.9f; p.racSquashMul = 0.9f; p.jumpMul = 1.1f; p.cycleDelta = 0; p.shakeScale = 1f;
            p.lightColor = new Color(1f, 0.55f, 0.15f);   // vàng-đỏ huyền bí (ánh chu sa trên giấy vàng)
            p.lightMul = 0.7f;
            p.moveStyle = ChestMoveStyle.Float;
            return p;
        }

        public override KeyPreset GetPreset() => Preset;

        // Các khối con cần cử động của từng lá bùa đang sống (theo root, không static).
        private class Parts { public Transform paper; public Transform[] tassels; }
        private readonly Dictionary<Transform, Parts> _parts = new Dictionary<Transform, Parts>();

        protected override GameObject BuildKey(KeyContext c)
        {
            Color paperCol = new Color(0.95f, 0.85f, 0.4f);
            Color red = new Color(0.8f, 0.1f, 0.1f);

            // Dọn các bản ghi của lá bùa đã bị huỷ.
            _parts.Clear();

            GameObject root = new GameObject("Key");
            Transform rt = root.transform;
            // Tờ giấy dài (dựng đứng, mỏng theo Z; +Z là mặt hướng về rương).
            GameObject paper = c.Part(rt, PrimitiveType.Cube, "Giay", new Vector3(0f, 0.08f, 0f), new Vector3(0.4f, 0.8f, 0.03f), paperCol);
            // Viền đỏ trên và dưới.
            c.Part(paper.transform, PrimitiveType.Cube, "VienTren", new Vector3(0f, 0.46f, -0.6f), new Vector3(1f, 0.08f, 1.4f), red);
            c.Part(paper.transform, PrimitiveType.Cube, "VienDuoi", new Vector3(0f, -0.46f, -0.6f), new Vector3(1f, 0.08f, 1.4f), red);
            // 3 nét chữ đỏ xếp dọc (mỏng, nổi ở mặt -Z, mặt nhìn thấy).
            c.Part(paper.transform, PrimitiveType.Cube, "Net1", new Vector3(0f, 0.26f, -0.75f), new Vector3(0.6f, 0.1f, 0.8f), red);
            c.Part(paper.transform, PrimitiveType.Cube, "Net2", new Vector3(0f, 0.0f, -0.75f), new Vector3(0.18f, 0.3f, 0.8f), red);
            c.Part(paper.transform, PrimitiveType.Cube, "Net3", new Vector3(0f, -0.26f, -0.75f), new Vector3(0.5f, 0.1f, 0.8f), red);

            // Tua đỏ: 3 sợi buông xuống dưới tờ giấy.
            Transform[] tassels = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject t = c.Part(rt, PrimitiveType.Cube, "Tua" + i, new Vector3(-0.12f + 0.12f * i, -0.42f, 0f), new Vector3(0.03f, 0.2f, 0.02f), red);
                tassels[i] = t.transform;
            }

            _parts[rt] = new Parts { paper = paper.transform, tassels = tassels };
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            // Lúc hành động mở, ThemeAction tự lo giấy/tua; chỉ phấp phới khi bay (phòng khi bay chuẩn được dùng).
            if (!InActionPhase) Flutter(root, 1f, Time.unscaledTime);
        }

        private static float Ease(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        /// <summary>Đung đưa giấy và tua (dùng chung cho lúc bay; amount 0 = phẳng).</summary>
        private void Flutter(Transform root, float amount, float now)
        {
            Parts p;
            if (root == null || !_parts.TryGetValue(root, out p) || p.paper == null) return;
            p.paper.localRotation = Quaternion.Euler(0f, 10f * amount * Mathf.Sin(now * 14f), 6f * amount * Mathf.Sin(now * 9f));
            for (int i = 0; i < p.tassels.Length; i++)
            {
                if (p.tassels[i] == null) continue;
                float swing = 18f * Mathf.Sin(now * 11f + i * 1.3f) * amount;
                Vector3 top = new Vector3(-0.12f + 0.12f * i, -0.32f, 0f);
                Quaternion q = Quaternion.Euler(0f, 0f, swing);
                p.tassels[i].localRotation = q;
                p.tassels[i].localPosition = top + q * new Vector3(0f, -0.1f, 0f);
            }
        }

        /// <summary>
        /// Bay như lá rơi: trôi về khe theo đường cong, lắc ngang qua lại (biên độ nhỏ dần), mỗi lần đổi chiều thì nghiêng
        /// và hơi tụt xuống như lá chao; cuối cùng ổn định, quay mặt vào khe.
        /// </summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            System.Random rng = c.Rng(16);
            float phase = (float)rng.NextDouble() * Mathf.PI * 2f;
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;
            float t0 = Time.unscaledTime;
            Vector3 start = root.position;
            while (true)
            {
                float t = Time.unscaledTime - t0;
                float u = Mathf.Clamp01(t / seconds);
                c.Refresh();
                Vector3 right = Vector3.Cross(c.up, c.front);
                Vector3 end = c.RestPos;
                float e = Ease(0f, 1f, u);
                Vector3 basePos = Vector3.Lerp(start, end, e) + c.up * (Mathf.Sin(u * Mathf.PI) * 0.25f * c.S);
                float fade = 1f - Ease(0.6f, 1f, u);                       // lắc nhỏ dần, cuối đứng yên
                float sway = Mathf.Sin(u * Mathf.PI * 5f + phase) * side;  // 2,5 nhịp chao qua lại
                float dip = Mathf.Abs(Mathf.Cos(u * Mathf.PI * 5f + phase)); // tụt xuống ở hai đầu nhịp chao
                Vector3 pos = basePos + right * (sway * 0.22f * c.S * fade) - c.up * ((1f - dip) * 0.05f * c.S * fade);
                root.position = pos;
                Quaternion tilt = Quaternion.Euler(12f * fade * Mathf.Cos(u * Mathf.PI * 5f + phase), 0f, -28f * sway * fade);
                root.rotation = c.Facing * tilt;
                root.localScale = Vector3.one * c.keySize;
                Flutter(root, 0.3f + 0.7f * fade, Time.unscaledTime);
                if (u >= 1f) break;
                yield return null;
            }
        }

        /// <summary>
        /// Dán bùa: vỗ thẳng vào khe ngay đầu (0 → 0,12), giấy ép dẹt rồi phẳng lại, chữ bùa phồng lóe một nhịp,
        /// tua rung rồi lặng; cuối cùng thu nhỏ tại khe.
        /// </summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            Parts p;
            _parts.TryGetValue(root, out p);
            float t0 = Time.unscaledTime;
            Vector3 from = root.position;
            while (true)
            {
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                c.Refresh();
                float hit = Ease(0f, 0.12f, u) * Ease(0f, 0.12f, u);   // lao nhanh dần vào khe
                if (u < 0.12f)
                {
                    root.position = Vector3.LerpUnclamped(from, c.RestPos, hit);
                    root.localScale = Vector3.one * c.keySize;
                }
                else ShrinkAtSlot(c, root, u);
                root.rotation = c.Facing;
                if (p != null && p.paper != null)
                {
                    // Ép dẹt lúc chạm (0,12 → 0,3), giấy hơi loe rộng ra rồi phẳng lại.
                    float squash = Ease(0.1f, 0.14f, u) * (1f - Ease(0.14f, 0.35f, u));
                    p.paper.localRotation = Quaternion.identity;
                    p.paper.localScale = new Vector3(0.4f * (1f + 0.12f * squash), 0.8f * (1f + 0.06f * squash), 0.03f * (1f - 0.5f * squash));
                    // Chữ bùa lóe: phồng lên một nhịp (0,15 → 0,55).
                    float glow = Mathf.Sin(Mathf.PI * Ease(0.15f, 0.55f, u));
                    foreach (Transform ch in p.paper)
                    {
                        if (ch == null || !ch.name.StartsWith("Net")) continue;
                        float bx = ch.name == "Net2" ? 0.18f : (ch.name == "Net1" ? 0.6f : 0.5f);
                        float by = ch.name == "Net2" ? 0.3f : 0.1f;
                        ch.localScale = new Vector3(bx * (1f + 0.35f * glow), by * (1f + 0.35f * glow), 0.8f);
                    }
                    // Tua rung mạnh lúc chạm rồi lặng dần.
                    float shake = Ease(0.1f, 0.15f, u) * (1f - Ease(0.15f, 0.7f, u));
                    for (int i = 0; i < p.tassels.Length; i++)
                    {
                        if (p.tassels[i] == null) continue;
                        float swing = 25f * shake * Mathf.Sin(u * 60f + i * 1.7f);
                        Vector3 top = new Vector3(-0.12f + 0.12f * i, -0.32f, 0f);
                        Quaternion q = Quaternion.Euler(0f, 0f, swing);
                        p.tassels[i].localRotation = q;
                        p.tassels[i].localPosition = top + q * new Vector3(0f, -0.1f, 0f);
                    }
                }
                if (u >= 1f) break;
                yield return null;
            }
        }

        /// <summary>Nháy vàng-đỏ khi bùa dán vào (mạnh hơn mặc định một chút, tắt nhanh như ánh chữ lóe).</summary>
        protected override void OnKeyImpact(KeyContext c)
        {
            if (Reveal != null) Reveal.PulseGlow(c.preset.lightColor, c.preset.lightMul, 0.45f);
        }
    }
}

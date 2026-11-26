using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Robot: THẺ CHIP (thẻ chữ nhật dẹt xám kim loại + chip vàng + sọc mạch cyan + chân tiếp xúc vàng).
    /// Cạnh ngắn có chân tiếp xúc hướng +Z (mũi cắm vào khe). Hành động Insert. Kế hoạch 2026-11-26, mục 6.10.
    /// </summary>
    public class RobotKeyHook : ChestKeyHookBase
    {
        static readonly Color Gray = new Color(0.62f, 0.65f, 0.70f);
        static readonly Color Gold = new Color(1f, 0.80f, 0.20f);
        static readonly Color Cyan = new Color(0.20f, 0.90f, 0.95f);

        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_Robot", displayName = "Robot",
            audioFolder = "Assets/_Project/Audio/Chests/robot/", filePrefix = "sfx_robot_", bedFile = "sfx_robot_hum_loop.mp3", keyFile = "sfx_robot_slot.mp3",
            mainColor = Gray, openAction = KeyAction.Insert,
            squashMul = 0.8f, racSquashMul = 0.8f, jumpMul = 1f, cycleDelta = -1, shakeScale = 1f,   // cứng: Squash -20%, chu kỳ -1 (>= 1)
            // Đợt 2: bay kiểu drone, quẹt thẻ; sáng cyan như đèn quét; rương rung máy móc.
            flyFile = "sfx_robot_fly.mp3", actionFile = "sfx_robot_action.mp3",
            lightColor = new Color(0.20f, 0.90f, 1.00f), lightMul = 0.7f, moveStyle = ChestMoveStyle.Shake,
        };

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            GameObject root = new GameObject("RobotCard");
            Transform t = root.transform;
            c.Part(t, PrimitiveType.Cube, "The", Vector3.zero, new Vector3(0.62f, 0.07f, 0.9f), Gray);
            c.Part(t, PrimitiveType.Cube, "Chip", new Vector3(0f, 0.07f, -0.05f), new Vector3(0.26f, 0.08f, 0.26f), Gold);
            c.Part(t, PrimitiveType.Cube, "SocMach1", new Vector3(-0.2f, 0.045f, 0.2f), new Vector3(0.04f, 0.03f, 0.3f), Cyan);
            c.Part(t, PrimitiveType.Cube, "SocMach2", new Vector3(0f, 0.045f, 0.28f), new Vector3(0.04f, 0.03f, 0.2f), Cyan);
            c.Part(t, PrimitiveType.Cube, "SocMach3", new Vector3(0.2f, 0.045f, 0.2f), new Vector3(0.04f, 0.03f, 0.3f), Cyan);
            c.Part(t, PrimitiveType.Cube, "SocMach4", new Vector3(0f, 0.045f, -0.32f), new Vector3(0.44f, 0.03f, 0.04f), Cyan);
            for (int i = 0; i < 3; i++)
                c.Part(t, PrimitiveType.Cube, "Chan" + i, new Vector3(-0.17f + 0.17f * i, 0f, 0.47f), new Vector3(0.1f, 0.05f, 0.1f), Gold);
            return root;
        }

        static float Sm(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        static Vector3 Right(KeyContext c) { return Vector3.Cross(c.up, c.front); }

        /// <summary>Điểm bắt đầu quẹt: sát trước khe, lệch phải 0,2 S.</summary>
        static Vector3 SwipeStart(KeyContext c) { return c.RestPos + c.front * (0.04f * c.S) + Right(c) * (0.2f * c.S); }

        /// <summary>Bay kiểu drone: 3 đoạn lướt thẳng, giữa các đoạn dừng xoay ngoắt 90° rồi xoay về; hơi nhấp nhô.</summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            float t0 = Time.unscaledTime;
            float phase = (float)c.Rng(10).NextDouble() * 6.28f;
            while (true)
            {
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float S = c.S, H = c.H;
                Vector3 r = Right(c);
                Vector3 a = c.slotPos + c.front * (0.8f * S) + c.up * (0.5f * H) - r * (0.35f * S);
                Vector3 b = c.slotPos + c.front * (0.4f * S) + c.up * (0.25f * H) + r * (0.35f * S);
                Vector3 e = SwipeStart(c);
                Vector3 pos, dir;
                if (u < 0.3f) { pos = Vector3.Lerp(c.startPos, a, Sm(0f, 0.3f, u)); dir = a - c.startPos; }
                else if (u < 0.4f) { pos = a; dir = Vector3.zero; }
                else if (u < 0.7f) { pos = Vector3.Lerp(a, b, Sm(0.4f, 0.7f, u)); dir = b - a; }
                else if (u < 0.8f) { pos = b; dir = Vector3.zero; }
                else { pos = Vector3.Lerp(b, e, Sm(0.8f, 1f, u)); dir = e - b; }
                // nhấp nhô nhẹ như cánh quạt, tắt dần khi tới khe
                pos += c.up * (0.015f * S * Mathf.Sin(u * 40f + phase) * (1f - Sm(0.85f, 1f, u)));
                float yaw = 90f * (Sm(0.3f, 0.4f, u) - Sm(0.7f, 0.8f, u));
                // nghiêng về hướng đang lướt (drone chúi mũi), 0 khi dừng
                float side = dir.sqrMagnitude > 1e-8f ? Vector3.Dot(dir.normalized, r) : 0f;
                float moving = (u < 0.3f || (u >= 0.4f && u < 0.7f) || u >= 0.8f) ? 1f : 0f;
                float roll = -12f * side * moving * (1f - Sm(0.9f, 1f, u));
                root.position = pos;
                root.rotation = c.Facing * Quaternion.Euler(0f, yaw, roll);
                if (u >= 1f) yield break;
                yield return null;
            }
        }

        /// <summary>Quẹt thẻ qua ổ đọc (phải → giữa), đẩy chân tiếp xúc vào khe, sọc mạch nháy chạy như đèn quét, rồi thu nhỏ.</summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            Transform[] soc = { root.Find("SocMach1"), root.Find("SocMach2"), root.Find("SocMach3"), root.Find("SocMach4") };
            Vector3[] baseScale = new Vector3[soc.Length];
            for (int i = 0; i < soc.Length; i++) if (soc[i] != null) baseScale[i] = soc[i].localScale;
            Transform chip = root.Find("Chip");
            Vector3 chipScale = chip != null ? chip.localScale : Vector3.one;
            float t0 = Time.unscaledTime;
            while (true)
            {
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                if (u < 0.7f)
                {
                    Vector3 s0 = SwipeStart(c);
                    Vector3 mid = c.RestPos + c.front * (0.04f * c.S);
                    // quẹt nhanh (0 → 0,4), rồi ấn vào khe (0,45 → 0,7)
                    Vector3 pos = Vector3.Lerp(s0, mid, Sm(0f, 0.4f, u));
                    pos = Vector3.Lerp(pos, c.RestPos, Sm(0.45f, 0.7f, u));
                    root.position = pos;
                    root.localScale = Vector3.one * c.keySize;
                }
                else ShrinkAtSlot(c, root, u);
                // khựng nhẹ lúc chạm ổ đọc (rung lăn 4°)
                float jolt = 4f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(u / 0.15f));
                root.rotation = c.Facing * Quaternion.Euler(0f, 0f, jolt);
                // đèn quét: từng sọc phồng lên lần lượt
                for (int i = 0; i < soc.Length; i++)
                {
                    if (soc[i] == null) continue;
                    float k = Mathf.Clamp01(1f - Mathf.Abs(u * 5f - (0.5f + i)));
                    soc[i].localScale = new Vector3(baseScale[i].x * (1f + 0.8f * k), baseScale[i].y * (1f + 1.5f * k), baseScale[i].z);
                }
                if (chip != null) chip.localScale = chipScale * (1f + 0.15f * Sm(0.6f, 0.7f, u) * (1f - Sm(0.75f, 0.85f, u)));
                if (u >= 1f) yield break;
                yield return null;
            }
        }
    }
}

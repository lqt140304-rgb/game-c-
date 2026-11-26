using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Nhật Bản: một thanh katana (lưỡi bạc dẹt, tsuba đen, cán quấn đỏ), mũi kiếm hướng +Z.
    /// Đợt 2: bay ngang xoay chậm lấp loáng (ThemeFly), mở bằng một nhát chém ngang rồi thu kiếm (ThemeAction).
    /// Điềm tĩnh: chu kỳ −1, Squash −10%.
    /// Kế hoạch 2026-11-26, mục 6.15.
    /// </summary>
    public class NhatBanKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = MakePreset();

        private static KeyPreset MakePreset()
        {
            KeyPreset p = KeyPreset.Neutral(
                "Chest_NhatBan", "NhatBan", "Assets/_Project/Audio/Chests/nhat_ban/", "sfx_nhatban_", "sfx_nhatban_shakuhachi_loop.mp3",
                new Color(0.82f, 0.86f, 0.9f), KeyAction.Strike);
            p.keyFile = "sfx_nhatban_sheath.mp3";
            p.squashMul = 0.9f;      // điềm tĩnh: Squash −10%
            p.cycleDelta = -1;       // chu kỳ −1 (lõi kẹp tối thiểu 1)
            // Đợt 2: bay ngang xoay chậm, chém ngang một nhát; sáng trắng bạc pha hồng anh đào; rương rung (Shake).
            p.flyFile = "sfx_nhatban_fly.mp3";
            p.actionFile = "sfx_nhatban_action.mp3";
            p.lightColor = new Color(0.95f, 0.86f, 0.92f);
            p.lightMul = 0.5f;
            p.moveStyle = ChestMoveStyle.Shake;
            return p;
        }

        // Nhóm khối kiếm (con của gốc) theo từng gốc vật; không dùng static.
        private readonly Dictionary<Transform, Transform> _blade = new Dictionary<Transform, Transform>();

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color silver = new Color(0.85f, 0.88f, 0.92f);
            Color black = new Color(0.07f, 0.07f, 0.08f);
            Color red = new Color(0.78f, 0.1f, 0.12f);
            Color darkRed = new Color(0.5f, 0.06f, 0.08f);

            GameObject root = new GameObject("Key");
            GameObject sword = new GameObject("Kiem");
            sword.transform.SetParent(root.transform, false);
            Transform t = sword.transform;

            // Lưỡi dài dẹt (z -0.16 .. 0.46) + mũi vát (xoay 45° quanh X, nhô tới 0.5).
            c.Part(t, PrimitiveType.Cube, "Luoi", new Vector3(0f, 0f, 0.15f), new Vector3(0.03f, 0.09f, 0.62f), Quaternion.identity, silver);
            c.Part(t, PrimitiveType.Cube, "Mui", new Vector3(0f, 0f, 0.46f), new Vector3(0.03f, 0.063f, 0.063f), Quaternion.Euler(45f, 0f, 0f), silver);
            // Tsuba tròn đen (đĩa dẹt vuông góc với lưỡi).
            c.Part(t, PrimitiveType.Cylinder, "Tsuba", new Vector3(0f, 0f, -0.17f), new Vector3(0.2f, 0.012f, 0.2f), Quaternion.Euler(90f, 0f, 0f), black);
            // Cán quấn đỏ + 3 dải quấn sẫm + đầu cán đen.
            c.Part(t, PrimitiveType.Cube, "Can", new Vector3(0f, 0f, -0.34f), new Vector3(0.07f, 0.07f, 0.28f), Quaternion.identity, red);
            c.Part(t, PrimitiveType.Cube, "Quan1", new Vector3(0f, 0f, -0.27f), new Vector3(0.078f, 0.078f, 0.02f), Quaternion.identity, darkRed);
            c.Part(t, PrimitiveType.Cube, "Quan2", new Vector3(0f, 0f, -0.34f), new Vector3(0.078f, 0.078f, 0.02f), Quaternion.identity, darkRed);
            c.Part(t, PrimitiveType.Cube, "Quan3", new Vector3(0f, 0f, -0.41f), new Vector3(0.078f, 0.078f, 0.02f), Quaternion.identity, darkRed);
            c.Part(t, PrimitiveType.Cube, "DauCan", new Vector3(0f, 0f, -0.49f), new Vector3(0.09f, 0.09f, 0.03f), Quaternion.identity, black);

            _blade[root.transform] = t;
            return root;
        }

        private static float Sm01(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        /// <summary>Điểm thủ thế trước khe: lùi ra trước 0,25 S, hơi lệch phải để có đà chém ngang.</summary>
        private static Vector3 GuardPos(KeyContext c)
        {
            Vector3 right = Vector3.Cross(c.up, c.front);
            return c.RestPos + c.front * (0.25f * c.S) + right * (0.08f * c.S);
        }

        /// <summary>
        /// Bay: katana nằm NGANG, lướt từ điểm khởi theo đường cong thấp lệch sang bên (như được phóng ngang),
        /// tự xoay chậm quanh trục đứng gần 1,5 vòng, lưỡi lăn nhẹ quanh chính nó để ánh lấp loáng; cuối bay quay mũi vào khe
        /// và dừng ở thế thủ.
        /// </summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            Transform sword;
            _blade.TryGetValue(root, out sword);
            System.Random rng = c.Rng(1501);
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;      // lượn sang trái hay phải
            float phase = (float)rng.NextDouble() * 360f;
            float t0 = Time.unscaledTime;
            Vector3 p0 = c.startPos;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float e = Sm01(0f, 1f, u);
                Vector3 right = Vector3.Cross(c.up, c.front);
                Vector3 p2 = GuardPos(c);
                // Đường cong: lệch ngang và nhô nhẹ ở giữa đường, về 0 ở hai đầu.
                float bulge = Mathf.Sin(Mathf.PI * e);
                Vector3 pos = Vector3.Lerp(p0, p2, e) + right * (side * 0.35f * c.S * bulge) + c.up * (0.12f * c.S * bulge);
                root.position = pos;
                // Xoay chậm quanh trục đứng: 540° giảm dần về 0 (đúng hướng Facing ở cuối) + chập chờn nhỏ.
                float yaw = side * 540f * (1f - Sm01(0f, 0.92f, u));
                root.rotation = Quaternion.AngleAxis(yaw, c.up) * c.Facing;
                if (sword != null)
                {
                    // Lưỡi lăn nhẹ ±25° quanh trục dọc → mặt lưỡi bắt sáng lấp loáng; tắt dần ở cuối.
                    float roll = 25f * Mathf.Sin(phase * Mathf.Deg2Rad + u * Mathf.PI * 5f) * (1f - Sm01(0.75f, 1f, u));
                    sword.localRotation = Quaternion.Euler(0f, 0f, roll);
                    sword.localPosition = Vector3.zero;
                }
                if (u >= 1f) yield break;
                yield return null;
            }
        }

        /// <summary>
        /// Mở: một nhát chém ngang thật nhanh qua miệng khe (0–0,2: lưỡi quét +75° → −75° quanh trục đứng, kiếm lắc nghiêng),
        /// thu kiếm về thẳng và đẩy mũi vào khe như tra vào vỏ (0,2–0,6, tới c.RestPos), rồi thu nhỏ tại khe (ShrinkAtSlot).
        /// </summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            Transform sword;
            _blade.TryGetValue(root, out sword);
            float t0 = Time.unscaledTime;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float t = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                root.rotation = c.Facing;
                float swing;   // góc quét quanh trục đứng (độ)
                float tilt;    // nghiêng lưỡi khi chém
                if (t < 0.04f) { swing = Mathf.Lerp(0f, 75f, t / 0.04f); tilt = 0f; }               // vung lấy đà
                else if (t < 0.2f) { float k = Sm01(0.04f, 0.2f, t); swing = Mathf.Lerp(75f, -75f, k); tilt = 30f * Mathf.Sin(Mathf.PI * k); }
                else { float k = Sm01(0.2f, 0.55f, t); swing = Mathf.Lerp(-75f, 0f, k); tilt = 0f; }  // thu kiếm
                if (sword != null)
                {
                    sword.localRotation = Quaternion.Euler(0f, swing, tilt);
                    sword.localPosition = Vector3.zero;
                }
                if (t < 0.6f)
                {
                    // Giữ thế thủ trong lúc chém, rồi tra mũi về đúng khe.
                    float k = Sm01(0.2f, 0.6f, t);
                    root.position = Vector3.Lerp(GuardPos(c), c.RestPos, k);
                    root.localScale = Vector3.one * c.keySize;
                }
                else ShrinkAtSlot(c, root, t);
                if (t >= 1f) yield break;
                yield return null;
            }
        }

        private void OnDisable() { _blade.Clear(); }
    }
}

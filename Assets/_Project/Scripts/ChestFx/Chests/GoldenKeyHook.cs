using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Golden (kế hoạch 2026-11-26, mục 6.3; đợt 2 mục 3 dòng #3): đồng xu vàng to, mặt có chữ "$".
    /// Bay: xu đứng cạnh LĂN tít về phía rương, nảy 2 lần nhỏ dần, cuối cùng dựng mặt về rương ở trên khe.
    /// Mở: thả vào khe như máy nhận xu: xoay tít quanh trục dọc khi tụt xuống, "ting" nảy lên một nhịp rồi thu nhỏ tại khe.
    /// </summary>
    public class GoldenKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_Golden", displayName = "Golden",
            audioFolder = "Assets/_Project/Audio/Chests/golden/", filePrefix = "sfx_golden_", bedFile = "sfx_golden_spin_loop.mp3", keyFile = "sfx_golden_token.mp3",
            mainColor = new Color(1f, 0.82f, 0.15f), openAction = KeyAction.Drop,
            squashMul = 1f, racSquashMul = 1f, jumpMul = 1.2f, cycleDelta = 0, shakeScale = 1f,   // nảy mạnh: Jump +20%
            flyFile = "sfx_golden_fly.mp3", actionFile = "sfx_golden_action.mp3",
            lightColor = new Color(1f, 0.8f, 0.32f), lightMul = 0.6f,   // vàng chói nhẹ
            moveStyle = ChestMoveStyle.Bounce,
        };

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color gold = Preset.mainColor;
            Color rimGold = new Color(0.78f, 0.55f, 0.08f);
            Color mark = new Color(0.55f, 0.36f, 0.04f);
            GameObject root = new GameObject("Key");
            GameObject pivot = new GameObject("Xoay");
            pivot.transform.SetParent(root.transform, false);
            Transform p = pivot.transform;
            Quaternion alongZ = Quaternion.Euler(90f, 0f, 0f);   // trụ nằm dọc Z, mặt đồng xu quay về +Z
            c.Part(p, PrimitiveType.Cylinder, "Vien", new Vector3(0f, 0f, 0f), new Vector3(0.96f, 0.05f, 0.96f), alongZ, rimGold);
            c.Part(p, PrimitiveType.Cylinder, "Mat", new Vector3(0f, 0f, 0.05f), new Vector3(0.78f, 0.03f, 0.78f), alongZ, gold);
            c.Part(p, PrimitiveType.Cube, "DoCot", new Vector3(0f, 0f, 0.085f), new Vector3(0.07f, 0.56f, 0.03f), mark);
            c.Part(p, PrimitiveType.Cube, "DoTren", new Vector3(-0.03f, 0.15f, 0.085f), new Vector3(0.26f, 0.07f, 0.03f), mark);
            c.Part(p, PrimitiveType.Cube, "DoDuoi", new Vector3(0.03f, -0.15f, 0.085f), new Vector3(0.26f, 0.07f, 0.03f), mark);
            return root;
        }

        private static float Smooth(float a, float b, float t)
        {
            float k = Mathf.Clamp01((t - a) / (b - a));
            return k * k * (3f - 2f * k);
        }

        /// <summary>Điểm chờ của hành động: ngay trên khe một chút (xu sẽ tụt xuống khe như vào máy).</summary>
        private static Vector3 HoverAbove(KeyContext c) { return c.RestPos + c.up * (0.22f * c.S); }

        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            Transform pivot = root.Find("Xoay");
            System.Random rng = c.Rng(303);
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;           // lệch trái/phải ngẫu nhiên (đường lăn hơi cong)
            float spins = 3f + (float)rng.NextDouble();               // số vòng lăn
            Vector3 p0 = root.position;
            float t0 = Time.unscaledTime;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                Vector3 p2 = HoverAbove(c);
                Vector3 right = Vector3.Cross(c.up, c.front);
                float e = u * (2f - u);                                // nhanh đầu, chậm cuối như xu lăn chậm dần
                Vector3 pos = Vector3.Lerp(p0, p2, e);
                pos += right * (side * 0.18f * c.S * Mathf.Sin(Mathf.PI * u));
                // Rơi xuống "mặt lăn" rồi nảy 2 lần nhỏ dần; về cuối nhấc lên điểm chờ.
                float dip = 0.25f * c.H * Mathf.Sin(Mathf.PI * Mathf.Clamp01(u / 0.85f));
                float bounce = 0f;
                if (u > 0.35f && u < 0.6f) bounce = 0.12f * c.H * Mathf.Sin(Mathf.PI * (u - 0.35f) / 0.25f);
                else if (u >= 0.6f && u < 0.78f) bounce = 0.05f * c.H * Mathf.Sin(Mathf.PI * (u - 0.6f) / 0.18f);
                pos += c.up * (bounce - dip);
                root.position = pos;

                // Xu đứng cạnh, mặt quay sang ngang, lăn quanh trục mặt; 0,8 → 1 dựng mặt về rương.
                Quaternion rolling = Quaternion.LookRotation(right * side, c.up);
                float face = Smooth(0.8f, 1f, u);
                root.rotation = Quaternion.Slerp(rolling, c.Facing, face);
                if (pivot != null)
                {
                    float roll = -side * 360f * spins * e * (1f - face);
                    pivot.localRotation = Quaternion.Euler(0f, 0f, roll);
                }
                if (u >= 1f) break;
                yield return null;
            }
        }

        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            Transform pivot = root.Find("Xoay");
            float t0 = Time.unscaledTime;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                root.rotation = c.Facing;
                root.localScale = Vector3.one * c.keySize;
                if (pivot != null)
                {
                    // Xoay tít quanh trục dọc khi tụt vào khe, chậm dần và dừng mặt thẳng lúc 0,85.
                    float spin = 900f * Smooth(0f, 0.85f, u);
                    pivot.localRotation = Quaternion.Euler(0f, spin, 0f);
                }
                if (u < 0.85f)
                {
                    // 0 → 0,35: tụt nhanh xuống khe; 0,35 → 0,65: "ting" nảy lên nhỏ; 0,65 → 0,85: đứng yên ở khe.
                    float drop = Smooth(0f, 0.35f, u);
                    float ting = (u > 0.35f && u < 0.65f) ? 0.08f * c.S * Mathf.Sin(Mathf.PI * (u - 0.35f) / 0.3f) : 0f;
                    root.position = Vector3.Lerp(HoverAbove(c), c.RestPos, drop) + c.up * ting;
                }
                else ShrinkAtSlot(c, root, u);
                if (u >= 1f) break;
                yield return null;
            }
        }
    }
}

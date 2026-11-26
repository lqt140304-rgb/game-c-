using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Thể thao: quả bóng đá (cầu trắng + 5 mảng đen dẹt), hành động Strike (đá vào khe).
    /// Kế hoạch 2026-11-26, mục 6.13. Không có bộ phận cử động riêng.
    /// </summary>
    public class TheThaoKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_TheThao",
            displayName = "TheThao",
            audioFolder = "Assets/_Project/Audio/Chests/the_thao/",
            filePrefix = "sfx_thethao_",
            bedFile = "sfx_thethao_ambient.mp3", keyFile = "sfx_thethao_kick.mp3",
            mainColor = new Color(0.96f, 0.96f, 0.96f),
            openAction = KeyAction.Strike,
            squashMul = 1f,
            racSquashMul = 1f,
            jumpMul = 1.2f,   // bật như bóng: Jump +20%
            cycleDelta = 0,
            shakeScale = 1f,
            // Đợt 2: bóng lăn rồi bị sút vòng cung; sút vào khe, bật ra rồi lăn chui vào.
            flyFile = "sfx_thethao_fly.mp3",
            actionFile = "sfx_thethao_action.mp3",
            lightColor = new Color(0.85f, 1f, 0.9f),   // trắng xanh sân cỏ dưới đèn
            lightMul = 0.6f,
            moveStyle = ChestMoveStyle.Bounce,
        };

        public override KeyPreset GetPreset() => Preset;

        static float Ease(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }

        /// <summary>Bay: lăn trên "sân" (độ cao đáy rương) 0,45 s, rồi bị sút bổng vòng cung vào khe, xoáy nhanh.</summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            System.Random rng = c.Rng(1313);
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;   // lăn vào từ trái hay phải
            Quaternion spin = c.Facing;
            float t0 = Time.unscaledTime;
            Vector3 prev = root.position;
            while (true)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                c.Refresh();
                Vector3 right = Vector3.Cross(c.up, c.front).normalized;
                float r = c.keySize * 0.5f;
                Vector3 ground = c.chestBase + c.up * r;
                Vector3 rollStart = ground + c.front * (1.4f * c.S) + right * (side * 0.9f * c.S);
                Vector3 kickPt = ground + c.front * (1.1f * c.S);
                Vector3 pos;
                if (t < 0.45f)
                {
                    // lăn chậm dần, nảy nhẹ 2 cái nhỏ
                    float u = t / 0.45f;
                    float e = 1f - (1f - u) * (1f - u);
                    pos = Vector3.Lerp(rollStart, kickPt, e) + c.up * (Mathf.Abs(Mathf.Sin(u * Mathf.PI * 2f)) * 0.06f * c.S * (1f - u));
                }
                else
                {
                    // sút: parabol bổng vào khe
                    float u = Ease((t - 0.45f) / 0.55f);
                    pos = Vector3.Lerp(kickPt, c.RestPos, u) + c.up * (Mathf.Sin(u * Mathf.PI) * 0.5f * c.H);
                }
                // xoay theo quãng đi được (lăn không trượt), thêm xoáy khi bổng
                Vector3 d = pos - prev;
                if (d.sqrMagnitude > 1e-8f && r > 1e-5f)
                {
                    Vector3 axis = Vector3.Cross(c.up, d.normalized);
                    float deg = d.magnitude / r * Mathf.Rad2Deg * (t < 0.45f ? 1f : 1.6f);
                    spin = Quaternion.AngleAxis(deg, axis) * spin;
                }
                prev = pos;
                root.position = pos;
                root.rotation = spin;
                if (t >= 1f) yield break;
                yield return null;
            }
        }

        /// <summary>Mở: sút thẳng vào khe (bẹp khi chạm), bật lại ra ngoài, rồi lăn chui vào và thu nhỏ.</summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            Quaternion spin0 = root.rotation;
            float t0 = Time.unscaledTime;
            while (true)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                c.Refresh();
                Vector3 right = Vector3.Cross(c.up, c.front).normalized;
                float back;          // khoảng lùi khỏi khe tính theo tip (0 = đầu chạm khe ở RestPos)
                float squash = 0f;   // bẹp theo hướng front lúc chạm
                if (t < 0.15f) { back = 0f; squash = Mathf.Sin(t / 0.15f * Mathf.PI) * 0.25f; }
                else if (t < 0.45f) { float u = (t - 0.15f) / 0.3f; back = Mathf.Sin(u * Mathf.PI) * 1.2f; }
                else if (t < 0.85f) { float u = (t - 0.45f) / 0.4f; back = Mathf.Sin(u * Mathf.PI) * 0.35f * (1f - u); }
                else back = 0f;
                root.rotation = Quaternion.AngleAxis(-t * 540f, right) * spin0;
                if (t < 0.85f)
                {
                    root.position = c.RestPos + c.front * (c.tip * back) + c.up * (c.tip * back * 0.4f);
                    // nảy "bẹp" nhẹ lúc chạm khe
                    root.localScale = Vector3.one * (c.keySize * (1f - squash * 0.5f));
                }
                else ShrinkAtSlot(c, root, t);
                if (t >= 1f) yield break;
                yield return null;
            }
        }

        protected override GameObject BuildKey(KeyContext c)
        {
            Color white = Preset.mainColor;
            Color black = new Color(0.08f, 0.08f, 0.09f);
            GameObject root = new GameObject("TheThaoKey");
            Transform t = root.transform;

            // Quả cầu trắng, đường kính 1 (vừa hộp 1x1x1), gốc ở giữa.
            c.Part(t, PrimitiveType.Sphere, "Bong", Vector3.zero, Vector3.one, white);

            // 5 mảng đen dẹt nằm sát mặt cầu, mỗi mảng hướng theo pháp tuyến của nó (mảng đầu hướng +Z để thấy từ phía trước).
            Vector3[] normals =
            {
                new Vector3(0f, 0.25f, 1f),
                new Vector3(0f, 1f, 0f),
                new Vector3(-1f, 0.2f, -0.35f),
                new Vector3(1f, 0.2f, -0.35f),
                new Vector3(0f, -1f, 0f),
            };
            for (int i = 0; i < normals.Length; i++)
            {
                Vector3 n = normals[i].normalized;
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, n);
                c.Part(t, PrimitiveType.Cylinder, "Mang" + i, n * 0.495f, new Vector3(0.3f, 0.02f, 0.3f), rot, black);
            }
            return root;
        }
    }
}

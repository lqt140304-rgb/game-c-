using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Mimic: chiếc dép lê (mồi cho rương đói), thả rơi vào khe.
    /// Mũi dép hướng +Z, gốc ở giữa, vừa hộp 1x1x1. Không có bộ phận cử động. Kế hoạch 2026-11-26, mục 6.5.
    /// </summary>
    public class MimicKeyHook : ChestKeyHookBase
    {
        static readonly Color Sole = new Color(0.85f, 0.75f, 0.55f);   // đế be
        static readonly Color Strap = new Color(0.45f, 0.28f, 0.14f);  // quai nâu
        static readonly Color Heel = new Color(0.62f, 0.45f, 0.28f);   // gót nâu nhạt

        public static readonly KeyPreset Preset = MakePreset();

        static KeyPreset MakePreset()
        {
            KeyPreset p = KeyPreset.Neutral(
                "Chest_Mimic", "Mimic", "Assets/_Project/Audio/Chests/mimic/", "sfx_mimic_", "sfx_mimic_stomach_loop.mp3",
                Sole, KeyAction.Drop);
            p.keyFile = "sfx_mimic_gulp.mp3";
            p.squashMul = 1.15f;     // háu đói: Squash +15%
            p.racSquashMul = 1.15f;
            // Đợt 2: dép bị ném xoay tròn, rơi vào miệng rương rồi bị "nhai" nuốt.
            p.flyFile = "sfx_mimic_fly.mp3";
            p.actionFile = "sfx_mimic_action.mp3";
            p.lightColor = new Color(0.75f, 0.25f, 0.15f);   // đỏ-nâu như họng rương đói
            p.lightMul = 0.55f;
            p.moveStyle = ChestMoveStyle.Shake;               // rương rung khi nhai
            return p;
        }

        public override KeyPreset GetPreset() => Preset;

        static float Ease(float u) { u = Mathf.Clamp01(u); return u * u * (3f - 2f * u); }

        /// <summary>Bay: dép bị ném theo vòng cung, xoay phẳng tít quanh trục lên (2,5 vòng), hơi nghiêng; tới gần cuối
        /// thì chậm xoay, nằm ngang mũi hướng vào khe, lơ lửng trên khe một chút để kịp thả rơi.</summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            System.Random rng = c.Rng(5051);
            float dir = rng.NextDouble() < 0.5 ? -1f : 1f;     // chiều xoay ngẫu nhiên theo seed
            float tilt = 12f + (float)rng.NextDouble() * 10f;
            Vector3 p0 = c.startPos;
            float t0 = Time.unscaledTime;
            while (true)
            {
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float e = Ease(u);
                Vector3 p2 = c.RestPos + c.up * (0.35f * c.H);
                Vector3 lin = Vector3.Lerp(p0, p2, e);
                float arc = 4f * e * (1f - e) * 0.45f * c.S;    // vòng cung ném
                root.position = lin + c.up * arc;
                // xoay tít lúc đầu, chậm dần về 0 ở cuối
                float spin = dir * 900f * (1f - (1f - u) * (1f - u));
                float lean = tilt * (1f - e);
                root.rotation = c.Facing * Quaternion.AngleAxis(spin, Vector3.up) * Quaternion.Euler(0f, 0f, lean * dir);
                root.localScale = Vector3.one * c.keySize;
                if (u >= 1f) yield break;
                yield return null;
            }
        }

        /// <summary>Mở: 0–0,2 thả rơi xuống khe (chạm), 0,2–0,85 rương "nhai": dép rung lắc, quai đập phập phồng,
        /// bị hút dần vào; 0,85–1 thu nhỏ tại khe (ShrinkAtSlot).</summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            System.Random rng = c.Rng(5052);
            float ph = (float)rng.NextDouble() * 6.283f;
            Transform strapL = root.Find("StrapL"), strapR = root.Find("StrapR");
            Quaternion rL = strapL != null ? strapL.localRotation : Quaternion.identity;
            Quaternion rR = strapR != null ? strapR.localRotation : Quaternion.identity;
            float t0 = Time.unscaledTime;
            while (true)
            {
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                ShrinkAtSlot(c, root, u);    // đặt gốc ở khe + thu nhỏ cuối
                float drop = 1f - Ease(u / 0.2f);              // rơi từ trên xuống
                float chew = u > 0.2f && u < 0.85f ? Mathf.Sin((u - 0.2f) / 0.65f * Mathf.PI) : 0f;
                float bite = Mathf.Sin(u * 50f + ph);          // nhịp nhai nhanh
                Vector3 off = c.up * (drop * 0.35f * c.H)
                            + Vector3.Cross(c.up, c.front).normalized * (chew * bite * 0.03f * c.S);
                root.position += off;
                root.rotation = c.Facing * Quaternion.Euler(chew * bite * 10f, chew * Mathf.Sin(u * 37f + ph) * 8f, 0f);
                // bị hút: dẹt lại theo nhịp nhai (vẫn ≤ cỡ gốc)
                float squash = 1f - chew * 0.15f * (0.5f + 0.5f * bite);
                root.localScale = new Vector3(root.localScale.x, root.localScale.y * squash, root.localScale.z);
                float flap = chew * bite * 20f;
                if (strapL != null) strapL.localRotation = rL * Quaternion.Euler(flap, 0f, 0f);
                if (strapR != null) strapR.localRotation = rR * Quaternion.Euler(-flap, 0f, 0f);
                if (u >= 1f)
                {
                    if (strapL != null) strapL.localRotation = rL;
                    if (strapR != null) strapR.localRotation = rR;
                    yield break;
                }
                yield return null;
            }
        }

        protected override GameObject BuildKey(KeyContext c)
        {
            GameObject root = new GameObject("MimicSlipper");
            Transform t = root.transform;
            // đế dép: tấm dẹt dài, mũi về +Z
            c.Part(t, PrimitiveType.Cube, "Sole", new Vector3(0f, -0.05f, 0f), new Vector3(0.5f, 0.1f, 0.96f), Sole);
            // quai: hai thanh chéo vắt lên mu bàn chân
            c.Part(t, PrimitiveType.Cube, "StrapL", new Vector3(-0.15f, 0.05f, 0.2f), new Vector3(0.1f, 0.1f, 0.5f),
                Quaternion.Euler(0f, 0f, -25f), Strap);
            c.Part(t, PrimitiveType.Cube, "StrapR", new Vector3(0.15f, 0.05f, 0.2f), new Vector3(0.1f, 0.1f, 0.5f),
                Quaternion.Euler(0f, 0f, 25f), Strap);
            // hai khối gót
            c.Part(t, PrimitiveType.Cube, "HeelL", new Vector3(-0.12f, 0.03f, -0.36f), new Vector3(0.2f, 0.14f, 0.2f), Heel);
            c.Part(t, PrimitiveType.Cube, "HeelR", new Vector3(0.12f, 0.03f, -0.36f), new Vector3(0.2f, 0.14f, 0.2f), Heel);
            return root;
        }
    }
}

using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Thợ Mỏ: chiếc cuốc chim (cán gỗ + lưỡi sắt cong + hai đầu nhọn). Kế hoạch 2026-11-26, mục 6.6.
    /// Đầu cuốc ở +Z (hướng về rương), gốc ở giữa, vừa hộp 1x1x1. Đợt 2: bay lộn vòng như ném rìu, mở bằng hai nhát bổ.
    /// </summary>
    public class MinerKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = Make();

        static KeyPreset Make()
        {
            var p = KeyPreset.Neutral("Chest_Miner", "Miner", "Assets/_Project/Audio/Chests/miner/", "sfx_miner_",
                "sfx_miner_rumble_loop.mp3", new Color(0.62f, 0.64f, 0.68f), KeyAction.Strike);
            p.keyFile = "sfx_miner_thump.mp3";
            p.jumpMul = 0.8f;
            // Đợt 2: tiếng bay (cuốc quay vù vù) + tiếng bổ hai nhát, đèn mỏ cam đỏ, rương rung mọi hướng như đất đá chấn động.
            p.flyFile = "sfx_miner_fly.mp3";
            p.actionFile = "sfx_miner_action.mp3";
            p.lightColor = new Color(1f, 0.55f, 0.18f);
            p.lightMul = 0.7f;
            p.moveStyle = ChestMoveStyle.Shake;
            return p;
        }

        // Góc giơ cuốc (độ, quanh trục ngang của vật): âm = đầu cuốc ngẩng lên.
        private const float RaisedPitch = -70f;
        // Tiến độ hành động lúc hai nhát bổ chạm khe (0,35 s × 0,25 ≈ 0,09 s và × 0,65 ≈ 0,23 s).
        private const float Hit1 = 0.25f, Hit2 = 0.65f;

        private static float Ease(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        /// <summary>Điểm chờ giơ cuốc: trước khe 0,3 S, cao hơn 0,2 S.</summary>
        private static Vector3 RaisedPos(KeyContext c) { return c.RestPos + c.front * (0.3f * c.S) + c.up * (0.2f * c.S); }

        /// <summary>Bay: ném như ném rìu, cuốc lộn vòng quanh trục ngang 3 vòng theo đường cong, dừng ở thế giơ cuốc.</summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds) { return Fly(c, root, seconds); }

        private static IEnumerator Fly(KeyContext c, Transform root, float seconds)
        {
            float t0 = Time.unscaledTime;
            System.Random rng = c.Rng(606);
            float side = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.25f;   // lệch ngang nhẹ mỗi lần
            Vector3 p0 = c.startPos;
            while (true)
            {
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float e = 1f - (1f - u) * (1f - u);                       // nhanh lúc đầu, chậm dần khi tới
                Vector3 p2 = RaisedPos(c);
                Vector3 right = Vector3.Cross(c.up, c.front);
                Vector3 arc = c.up * (0.35f * c.S * Mathf.Sin(Mathf.PI * u)) + right * (side * c.S * Mathf.Sin(Mathf.PI * u));
                root.position = Vector3.LerpUnclamped(p0, p2, e) + arc;
                float pitch = RaisedPitch - 1080f * (1f - e);              // 3 vòng lộn, quay chậm dần về thế giơ
                root.rotation = c.Facing * Quaternion.Euler(pitch, 0f, 0f);
                root.localScale = Vector3.one * c.keySize;
                if (u >= 1f) yield break;
                yield return null;
            }
        }

        /// <summary>Mở: bổ xuống khe hai nhát (chạm ở 0,25 và 0,65), giữa hai nhát giật lên; mũi cuốc rung như đá vụn bắn, rồi thu nhỏ tại khe.</summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds) { return Act(c, root, seconds); }

        private static IEnumerator Act(KeyContext c, Transform root, float seconds)
        {
            float t0 = Time.unscaledTime;
            Transform tipL = root.Find("TipL"), tipR = root.Find("TipR"), core = root.Find("HeadCore");
            Vector3 tipL0 = tipL != null ? tipL.localPosition : Vector3.zero;
            Vector3 tipR0 = tipR != null ? tipR.localPosition : Vector3.zero;
            Vector3 core0 = core != null ? core.localPosition : Vector3.zero;
            while (true)
            {
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                // k: 1 = giơ cao, 0 = chạm khe.
                float k;
                if (u < Hit1) { float a = u / Hit1; k = 1f - a * a; }   // rơi nhanh dần như bổ thật
                else if (u < 0.42f) k = 0.55f * Ease(Hit1, 0.42f, u);
                else if (u < Hit2) k = 0.55f * (1f - Ease(0.42f, Hit2, u));
                else k = 0f;
                float pitch = RaisedPitch * k;
                root.rotation = c.Facing * Quaternion.Euler(pitch, 0f, 0f);

                // Rung sau mỗi nhát chạm (tắt dần ~0,05 s): mũi và lõi lưỡi giật nhẹ như đá vụn văng.
                float d1 = u >= Hit1 ? Mathf.Exp(-(u - Hit1) * 40f) : 0f;
                float d2 = u >= Hit2 ? Mathf.Exp(-(u - Hit2) * 30f) : 0f;
                float j = (d1 + d2) * Mathf.Sin(u * 180f);
                if (tipL != null) tipL.localPosition = tipL0 + new Vector3(0f, 0.04f * j, 0f);
                if (tipR != null) tipR.localPosition = tipR0 + new Vector3(0f, -0.04f * j, 0f);
                if (core != null) core.localPosition = core0 + new Vector3(0.03f * j, 0f, 0f);

                if (u < Hit2 + 0.05f)
                {
                    Vector3 rest = c.RestPos;
                    root.position = Vector3.LerpUnclamped(rest, RaisedPos(c), k);
                    root.localScale = Vector3.one * c.keySize;
                }
                else ShrinkAtSlot(c, root, u);   // giữ đúng khe, thu nhỏ từ 0,85

                if (u >= 1f) yield break;
                yield return null;
            }
        }

        /// <summary>Nháy đèn mỏ cam đỏ ở nhát 1, thêm một nháy vàng nhỏ hơn ở nhát 2.</summary>
        protected override void OnKeyImpact(KeyContext c)
        {
            base.OnKeyImpact(c);
            if (Reveal != null) StartCoroutine(SecondFlash(Reveal));
        }

        private IEnumerator SecondFlash(ChestRevealSequence reveal)
        {
            float t0 = Time.unscaledTime;
            while (Time.unscaledTime - t0 < Hit2 * ActionTime) yield return null;
            if (reveal != null) reveal.PulseGlow(new Color(1f, 0.78f, 0.35f), 0.5f, 0.35f);
        }

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            var root = new GameObject("MinerPickaxe");
            Color wood = new Color(0.45f, 0.29f, 0.14f);
            Color iron = Preset.mainColor;
            Color dark = new Color(0.30f, 0.31f, 0.34f);
            Color tip = new Color(0.85f, 0.87f, 0.9f);

            // Cán gỗ nằm dọc trục Z (hình trụ xoay 90 độ quanh X), dài ~0,95.
            c.Part(root.transform, PrimitiveType.Cylinder, "Handle", new Vector3(0f, 0f, -0.05f),
                new Vector3(0.10f, 0.475f, 0.10f), Quaternion.Euler(90f, 0f, 0f), wood);
            // Nút tròn nhỏ ở cuối cán
            c.Part(root.transform, PrimitiveType.Sphere, "HandleKnob", new Vector3(0f, 0f, -0.5f),
                new Vector3(0.14f, 0.14f, 0.14f), wood);
            // Khối giữa lưỡi sắt ôm đầu cán
            c.Part(root.transform, PrimitiveType.Cube, "HeadCore", new Vector3(0f, 0f, 0.38f),
                new Vector3(0.20f, 0.16f, 0.16f), dark);
            // Hai cánh lưỡi cong về phía trước (+Z)
            c.Part(root.transform, PrimitiveType.Cube, "ArmL", new Vector3(-0.22f, 0f, 0.41f),
                new Vector3(0.28f, 0.12f, 0.12f), Quaternion.Euler(0f, 22f, 0f), iron);
            c.Part(root.transform, PrimitiveType.Cube, "ArmR", new Vector3(0.22f, 0f, 0.41f),
                new Vector3(0.28f, 0.12f, 0.12f), Quaternion.Euler(0f, -22f, 0f), iron);
            // Hai mũi nhọn sáng ở hai đầu
            c.Part(root.transform, PrimitiveType.Cube, "TipL", new Vector3(-0.40f, 0f, 0.47f),
                new Vector3(0.14f, 0.09f, 0.09f), Quaternion.Euler(0f, 40f, 45f), tip);
            c.Part(root.transform, PrimitiveType.Cube, "TipR", new Vector3(0.40f, 0f, 0.47f),
                new Vector3(0.14f, 0.09f, 0.09f), Quaternion.Euler(0f, -40f, 45f), tip);
            return root;
        }
    }
}

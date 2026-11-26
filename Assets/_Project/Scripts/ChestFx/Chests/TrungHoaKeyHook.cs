using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Trung Hoa: ĐẦU LÂN nhỏ (đầu đỏ, hai mắt to, hàm dưới vàng, bờm tua).
    /// Mặt lân hướng +Z; hàm đóng/mở và bờm đung đưa khi bay và khi vồ (kế hoạch 2026-11-26, mục 6.14).
    /// </summary>
    public class TrungHoaKeyHook : ChestKeyHookBase
    {
        static KeyPreset Make()
        {
            var p = new KeyPreset
            {
                chestId = "Chest_TrungHoa", displayName = "TrungHoa",
                audioFolder = "Assets/_Project/Audio/Chests/trung_hoa/", filePrefix = "sfx_trunghoa_",
                bedFile = "sfx_trunghoa_lion_drum_loop.mp3", keyFile = "sfx_trunghoa_thud.mp3",
                mainColor = new Color(0.85f, 0.1f, 0.1f), openAction = KeyAction.Pounce,
                squashMul = 1f, racSquashMul = 1f, jumpMul = 1f, cycleDelta = 1, shakeScale = 1f,
            };
            // Đợt 2: tiếng bay/ngoạm riêng, sáng đỏ-vàng đèn lồng, rương nảy theo nhịp trống múa lân.
            p.flyFile = "sfx_trunghoa_fly.mp3";
            p.actionFile = "sfx_trunghoa_action.mp3";
            p.lightColor = new Color(1f, 0.42f, 0.08f);
            p.lightMul = 0.7f;
            p.moveStyle = ChestMoveStyle.Bounce;
            return p;
        }

        public static readonly KeyPreset Preset = Make();

        public override KeyPreset GetPreset() => Preset;

        sealed class Rig { public Transform jaw; public Transform[] mane; }
        // Các khối con cử động, theo root của từng vật (không dùng static).
        readonly Dictionary<Transform, Rig> _rigs = new Dictionary<Transform, Rig>();

        protected override GameObject BuildKey(KeyContext c)
        {
            Color red = Preset.mainColor;
            Color gold = new Color(1f, 0.8f, 0.15f);
            Color white = new Color(0.97f, 0.97f, 0.95f);
            Color black = new Color(0.05f, 0.05f, 0.05f);

            var root = new GameObject("TrungHoaLion");
            Transform t = root.transform;

            // Đầu đỏ + mõm nhô ra phía +Z
            c.Part(t, PrimitiveType.Cube, "Dau", new Vector3(0f, 0.12f, -0.05f), new Vector3(0.7f, 0.5f, 0.6f), red);
            c.Part(t, PrimitiveType.Cube, "Mom", new Vector3(0f, 0.05f, 0.3f), new Vector3(0.5f, 0.28f, 0.3f), red);
            c.Part(t, PrimitiveType.Sphere, "Mui", new Vector3(0f, 0.16f, 0.46f), new Vector3(0.14f, 0.12f, 0.1f), gold);
            // Hai mắt to: tròng trắng + con ngươi đen
            for (int i = -1; i <= 1; i += 2)
            {
                c.Part(t, PrimitiveType.Sphere, "Mat" + i, new Vector3(0.19f * i, 0.3f, 0.2f), new Vector3(0.22f, 0.22f, 0.12f), white);
                c.Part(t, PrimitiveType.Sphere, "Nguoi" + i, new Vector3(0.19f * i, 0.3f, 0.27f), new Vector3(0.1f, 0.1f, 0.06f), black);
            }
            c.Part(t, PrimitiveType.Cube, "Tran", new Vector3(0f, 0.43f, 0.05f), new Vector3(0.1f, 0.1f, 0.1f), gold);

            // Hàm dưới vàng, xoay quanh chốt ở sau mõm
            var jawPivot = new GameObject("HamPivot").transform;
            jawPivot.SetParent(t, false);
            jawPivot.localPosition = new Vector3(0f, -0.1f, 0.05f);
            c.Part(jawPivot, PrimitiveType.Cube, "Ham", new Vector3(0f, -0.04f, 0.25f), new Vector3(0.5f, 0.14f, 0.5f), gold);

            // Bờm tua: 5 dải sau đầu, vàng xen đỏ
            var mane = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                float a = (i - 2) * 0.32f;
                var p = new GameObject("BomPivot" + i).transform;
                p.SetParent(t, false);
                p.localPosition = new Vector3(Mathf.Sin(a) * 0.4f, 0.02f + Mathf.Cos(a) * 0.1f, -0.38f);
                c.Part(p, PrimitiveType.Cube, "Bom" + i, new Vector3(0f, -0.1f, -0.04f), new Vector3(0.1f, 0.34f, 0.06f), (i % 2 == 0) ? gold : red);
                mane[i] = p;
            }

            _rigs[t] = new Rig { jaw = jawPivot, mane = mane };
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            Rig r;
            if (root == null || !_rigs.TryGetValue(root, out r)) return;
            // Hàm ngáp: lúc bay mở khép chậm, lúc vồ cắn nhanh hơn.
            float k = InActionPhase ? 4f : 2f;
            float open = Mathf.Abs(Mathf.Sin(t01 * Mathf.PI * k));
            if (r.jaw != null) r.jaw.localRotation = Quaternion.Euler(open * 22f, 0f, 0f);
            for (int i = 0; i < r.mane.Length; i++)
                if (r.mane[i] != null)
                    r.mane[i].localRotation = Quaternion.Euler(Mathf.Sin(t01 * Mathf.PI * 6f + i) * 14f, 0f, 0f);
        }

        static float Sm01(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        void PoseRig(Transform root, float jawDeg, float maneT)
        {
            Rig r;
            if (root == null || !_rigs.TryGetValue(root, out r)) return;
            if (r.jaw != null) r.jaw.localRotation = Quaternion.Euler(jawDeg, 0f, 0f);
            for (int i = 0; i < r.mane.Length; i++)
                if (r.mane[i] != null)
                    r.mane[i].localRotation = Quaternion.Euler(Mathf.Sin(maneT + i) * 16f, 0f, 0f);
        }

        /// <summary>
        /// Bay kiểu MÚA LÂN: 3 bước nhảy theo nhịp trống (mỗi bước bật lên rồi đáp, nhún xuống), đầu lắc trái phải và
        /// gật lên xuống, bờm tung; giữa chặng ngáp một cái thật to. Kết thúc ở trước khe một chút (chờ ngoạm).
        /// </summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            var rng = c.Rng(1414);
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;
            float phase = (float)rng.NextDouble() * 6.28f;
            float t0 = Time.unscaledTime;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                Vector3 p0 = c.startPos;
                Vector3 p2 = c.RestPos + c.front * (0.25f * c.S);
                // tiến từng bậc: 3 nhảy, mỗi nhảy tiến nhanh giữa bước, chậm lúc đáp
                const int hops = 3;
                float hu = u * hops;
                int k = Mathf.Min((int)hu, hops - 1);
                float f = hu - k;
                float along = (k + Sm01(0f, 1f, f)) / hops;
                Vector3 basePos = Vector3.Lerp(p0, p2, along);
                Vector3 right = Vector3.Cross(c.up, c.front).normalized;
                float hop = Mathf.Sin(f * Mathf.PI) * (0.22f * c.S) * (1f - 0.3f * k / hops);
                float dip = -Mathf.Max(0f, Mathf.Cos(f * Mathf.PI * 2f)) * 0.03f * c.S * (1f - u);
                float sway = Mathf.Sin(u * Mathf.PI * 3f + phase) * 0.12f * c.S * (1f - u) * side;
                root.position = basePos + c.up * (hop + dip) + right * sway;
                float yaw = Mathf.Sin(u * Mathf.PI * 6f + phase) * 25f * (1f - u);
                float nod = Mathf.Sin(f * Mathf.PI * 2f) * 15f * (1f - u);
                root.rotation = c.Facing * Quaternion.Euler(-nod, yaw * side, yaw * 0.4f);
                // ngáp to ở bước giữa, hé nhẹ ở các bước còn lại
                float yawn = k == 1 ? Mathf.Sin(f * Mathf.PI) * 32f : Mathf.Sin(f * Mathf.PI) * 8f;
                PoseRig(root, yawn, u * Mathf.PI * 8f);
                if (u >= 1f) break;
                yield return null;
            }
        }

        /// <summary>
        /// Mở kiểu LÂN NGOẠM: há hàm thật rộng và vươn tới (0–0,3), đớp "cụp" vào khe (0,3–0,45), lắc đầu giằng hai cái
        /// (0,45–0,75), rồi đứng ở khe c.RestPos và thu nhỏ (ShrinkAtSlot).
        /// </summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            float t0 = Time.unscaledTime;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float lunge = Sm01(0f, 0.3f, u);
                float jaw = u < 0.3f ? Sm01(0f, 0.25f, u) * 40f : (1f - Sm01(0.3f, 0.4f, u)) * 40f;
                float shake = u > 0.45f && u < 0.75f ? Mathf.Sin((u - 0.45f) / 0.3f * Mathf.PI * 4f) * 14f * (1f - Sm01(0.6f, 0.75f, u)) : 0f;
                Vector3 hover = c.RestPos + c.front * (0.25f * c.S);
                if (u < 0.85f)
                {
                    float bite = u > 0.3f && u < 0.42f ? Mathf.Sin((u - 0.3f) / 0.12f * Mathf.PI) * 0.04f * c.S : 0f;
                    root.position = Vector3.Lerp(hover, c.RestPos, lunge) - c.front * bite;
                    root.localScale = Vector3.one * c.keySize;
                }
                else ShrinkAtSlot(c, root, u);
                float tilt = (1f - lunge) * -18f;   // ngẩng lên khi há, cúi xuống khi đớp
                root.rotation = c.Facing * Quaternion.Euler(tilt, shake, shake * 0.5f);
                PoseRig(root, jaw, u * Mathf.PI * 10f);
                if (u >= 1f) break;
                yield return null;
            }
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            base.OnReset(reveal);
            _rigs.Clear();
        }
    }
}

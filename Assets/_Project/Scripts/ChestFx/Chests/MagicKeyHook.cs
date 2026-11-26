using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Magic: tinh thể rune tím lơ lửng, có 3 mảnh nhỏ xoay quanh (kế hoạch 2026-11-26, mục 6.4; đợt 2 mục 3 dòng 4).
    /// Bay: lơ lửng uốn lượn hình chữ S, nhấp nhô nhẹ, mảnh xoay quanh nhanh dần. Mở: áp vào khe, mảnh hút vào lõi, rune
    /// bừng sáng (lõi phồng) rồi nổ chớp nhẹ (mảnh bung ra, nháy glow lần 2), thu nhỏ tại khe. Tính cách: lơ lửng nên Squash −10%.
    /// </summary>
    public class MagicKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = MakePreset();

        private static KeyPreset MakePreset()
        {
            KeyPreset p = KeyPreset.Neutral(
                "Chest_Magic", "Magic", "Assets/_Project/Audio/Chests/magic/", "sfx_magic_", "sfx_magic_rune_bed_loop.mp3",
                new Color(0.6f, 0.3f, 0.9f), KeyAction.Insert);
            p.keyFile = "sfx_magic_rune_stone.mp3";
            p.flyFile = "sfx_magic_fly.mp3";
            p.actionFile = "sfx_magic_action.mp3";
            p.lightColor = new Color(0.62f, 0.3f, 1f);   // tím
            p.lightMul = 0.55f;
            p.moveStyle = ChestMoveStyle.Float;
            p.squashMul = 0.9f;
            p.racSquashMul = 0.9f;
            return p;
        }

        private const float OrbitRadius = 0.36f;
        private static readonly Color Shard = new Color(0.85f, 0.7f, 1f);
        private static readonly Color Core = new Color(0.95f, 0.85f, 1f);
        private static readonly Color Flash = new Color(0.8f, 0.6f, 1f);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color main = Preset.mainColor;
            GameObject root = new GameObject("Key");
            Transform t = root.transform;
            // Tinh thể thoi dài theo Z (đầu nhọn về rương): hai khối dài xoay chéo nhau.
            c.Part(t, PrimitiveType.Cube, "TinhThe", Vector3.zero, new Vector3(0.24f, 0.24f, 0.7f), Quaternion.Euler(0f, 0f, 45f), main);
            c.Part(t, PrimitiveType.Cube, "TinhThe2", Vector3.zero, new Vector3(0.24f, 0.24f, 0.7f), Quaternion.identity, main * 0.85f);
            c.Part(t, PrimitiveType.Sphere, "Loi", Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f), Core);
            for (int i = 0; i < 3; i++)
            {
                c.Part(t, PrimitiveType.Cube, "Manh" + i, OrbitPos(i, 0f, OrbitRadius), new Vector3(0.12f, 0.12f, 0.12f), Quaternion.Euler(45f, 45f, 0f), Shard);
            }
            return root;
        }

        private static Vector3 OrbitPos(int i, float phase, float radius)
        {
            float a = phase + i * (Mathf.PI * 2f / 3f);
            return new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
        }

        private static float Sm(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        /// <summary>Đặt mảnh quay quanh và tinh thể tự xoay (chỉ khối con của chính vật).</summary>
        private static void Pose(Transform root, float phase, float radius, float crystalSpin, float coreScale)
        {
            float now = Time.unscaledTime;
            for (int i = 0; i < 3; i++)
            {
                Transform m = root.Find("Manh" + i);
                if (m == null) continue;
                m.localPosition = OrbitPos(i, phase, radius);
                m.localRotation = Quaternion.Euler(now * 200f, now * 140f + i * 40f, 0f);
            }
            Transform a = root.Find("TinhThe");
            Transform b = root.Find("TinhThe2");
            Quaternion spin = Quaternion.Euler(0f, 0f, crystalSpin);
            if (a != null) a.localRotation = spin * Quaternion.Euler(0f, 0f, 45f);
            if (b != null) b.localRotation = spin;
            Transform core = root.Find("Loi");
            if (core != null) core.localScale = Vector3.one * (0.2f * coreScale);
        }

        /// <summary>Bay lơ lửng uốn lượn chữ S (lắc ngang tắt dần khi tới khe), nhấp nhô nhẹ, nghiêng theo hướng lượn.</summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            return Fly(c, root, seconds);
        }

        private IEnumerator Fly(KeyContext c, Transform root, float seconds)
        {
            float t0 = Time.unscaledTime;
            System.Random rng = c.Rng(4404);
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;   // lượn sang trái hay phải trước
            float phase = (float)rng.NextDouble() * 6.28f;
            Vector3 p0 = c.startPos;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float e = u * u * (3f - 2f * u);
                float S = c.S;
                Vector3 right = Vector3.Cross(c.up, c.front);
                Vector3 target = c.RestPos;
                float fade = 1f - e;   // biên độ uốn lượn/nhấp nhô tắt dần → tới đúng khe
                float sway = side * 0.22f * S * Mathf.Sin(2f * Mathf.PI * 1.25f * u) * fade;
                float bob = 0.05f * S * Mathf.Sin(4f * Mathf.PI * u) * fade + 0.12f * S * Mathf.Sin(Mathf.PI * u);
                root.position = Vector3.Lerp(p0, target, e) + right * sway + c.up * bob;
                // Nghiêng (lăn quanh trục bay) theo đạo hàm lắc ngang; tự xoay đều.
                float roll = -side * 25f * Mathf.Cos(2f * Mathf.PI * 1.25f * u) * fade;
                root.rotation = c.Facing * Quaternion.Euler(0f, 0f, roll);
                float spinRate = Mathf.Lerp(4f, 8f, u);
                Pose(root, phase + Time.unscaledTime * spinRate, OrbitRadius, Time.unscaledTime * 60f, 1f);
                if (u >= 1f) break;
                yield return null;
            }
        }

        /// <summary>Áp vào khe: mảnh hút vào lõi, rune bừng sáng (lõi phồng), nổ chớp nhẹ (mảnh bung, nháy glow), thu nhỏ tại khe.</summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            return Act(c, root, seconds);
        }

        private IEnumerator Act(KeyContext c, Transform root, float seconds)
        {
            float t0 = Time.unscaledTime;
            bool flashed = false;
            float phase0 = Time.unscaledTime * 8f;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float t = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                // Vị trí/cỡ: đầu ở khe suốt, thu nhỏ 0,85 → 1.
                ShrinkAtSlot(c, root, t);
                // Rung nhẹ khi rune nạp năng lượng (0..0,45), tắt sau chớp.
                float shiver = 4f * Mathf.Sin(Time.unscaledTime * 90f) * (1f - Sm(0.45f, 0.6f, t));
                root.rotation = c.Facing * Quaternion.Euler(shiver, 0f, 0f);
                // Mảnh: hút vào (0..0,45), bung ra khi chớp (0,45..0,65), lùi về vòng thường.
                float radius = Mathf.Lerp(OrbitRadius, 0.08f, Sm(0f, 0.45f, t));
                radius = Mathf.Lerp(radius, 0.55f, Sm(0.45f, 0.6f, t));
                radius = Mathf.Lerp(radius, OrbitRadius, Sm(0.65f, 0.85f, t));
                // Lõi bừng sáng phồng to rồi xẹp.
                float core = 1f + 0.9f * Sm(0.05f, 0.45f, t) - 0.9f * Sm(0.5f, 0.75f, t);
                float spin = 60f * Time.unscaledTime + 540f * Sm(0f, 0.5f, t);
                Pose(root, phase0 + 30f * t, radius, spin, core);
                if (!flashed && t >= 0.45f)
                {
                    flashed = true;
                    if (Reveal != null) Reveal.PulseGlow(Flash, 0.75f, 0.4f);   // nổ chớp nhẹ
                }
                if (t >= 1f) break;
                yield return null;
            }
        }

        /// <summary>Lúc chạm khe: nháy tím dịu (rune bắt đầu sáng); chớp mạnh hơn nằm trong ThemeAction.</summary>
        protected override void OnKeyImpact(KeyContext c)
        {
            if (Reveal != null) Reveal.PulseGlow(c.preset.lightColor, c.preset.lightMul, 0.3f);
        }

        /// <summary>Dự phòng (base gọi khi không dùng móc theme): mảnh xoay quanh, tinh thể xoay chậm.</summary>
        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            if (root == null) return;
            float now = Time.unscaledTime;
            Pose(root, now * (InActionPhase ? 9f : 5f), OrbitRadius, now * 60f, 1f);
        }
    }
}

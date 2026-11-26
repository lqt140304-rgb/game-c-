using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương VietNam: HOA ĐĂNG (đế gỗ + 6 cánh sen hồng + ngọn lửa vàng ở giữa), hành động Drop
    /// (thả rơi vào khe). Kế hoạch 2026-11-26, mục 6.8. Tính cách bóp méo: mềm (Squash −10%).
    /// </summary>
    public class VietNamKeyHook : ChestKeyHookBase
    {
        static readonly Color Go = new Color(0.55f, 0.36f, 0.18f);
        static readonly Color Sen = new Color(0.97f, 0.5f, 0.7f);
        static readonly Color Lua = new Color(1f, 0.82f, 0.2f);

        public static readonly KeyPreset Preset = MakePreset();

        static KeyPreset MakePreset()
        {
            KeyPreset p = KeyPreset.Neutral(
                "Chest_VietNam", "VietNam", "Assets/_Project/Audio/Chests/viet_nam/", "sfx_vietnam_", "sfx_vietnam_dan_bau_loop.mp3",
                Sen, KeyAction.Drop);
            p.keyFile = "sfx_vietnam_thud.mp3";
            p.squashMul = 0.9f;      // mềm: Squash −10%
            p.racSquashMul = 0.9f;
            // Đợt 2: vàng ấm như ánh nến hoa đăng, nháy dịu; rương bồng bềnh như trên mặt nước.
            p.lightColor = new Color(1f, 0.74f, 0.32f);
            p.lightMul = 0.55f;
            p.moveStyle = ChestMoveStyle.Float;
            p.flyFile = "sfx_vietnam_fly.mp3";
            p.actionFile = "sfx_vietnam_action.mp3";
            return p;
        }

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            GameObject root = new GameObject("HoaDang");
            Transform t = root.transform;
            // Đế gỗ tròn dẹt
            c.Part(t, PrimitiveType.Cylinder, "De", new Vector3(0f, -0.2f, 0f), new Vector3(0.8f, 0.05f, 0.8f), Go);
            // 6 cánh sen hồng xoè quanh, nghiêng nhẹ ra ngoài
            for (int i = 0; i < 6; i++)
            {
                Quaternion around = Quaternion.Euler(0f, i * 60f, 0f);
                Vector3 pos = around * new Vector3(0f, -0.04f, 0.27f);
                Quaternion rot = around * Quaternion.Euler(25f, 0f, 0f);
                c.Part(t, PrimitiveType.Cube, "Canh" + i, pos, new Vector3(0.2f, 0.34f, 0.07f), rot, Sen);
            }
            // Ngọn lửa vàng ở giữa
            c.Part(t, PrimitiveType.Sphere, "Lua", Vector3.zero, new Vector3(0.2f, 0.3f, 0.2f), Lua);
            return root;
        }

        static float Ease(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }

        /// <summary>Đặt độ nghiêng xoè của 6 cánh sen (độ) và cỡ ngọn lửa.</summary>
        static void Pose(Transform root, float petalDeg, float flame)
        {
            for (int i = 0; i < 6; i++)
            {
                Transform p = root.Find("Canh" + i);
                if (p == null) continue;
                Quaternion around = Quaternion.Euler(0f, i * 60f, 0f);
                p.localRotation = around * Quaternion.Euler(petalDeg, 0f, 0f);
            }
            Transform lua = root.Find("Lua");
            if (lua != null) lua.localScale = new Vector3(0.2f, 0.3f, 0.2f) * flame;
        }

        /// <summary>Bay: bồng bềnh trôi như thả sông — trôi chậm dần, nhấp nhô lên xuống, lắc lư sang ngang, xoay nhẹ, nghiêng như gợn sóng, lửa lập loè.</summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            System.Random rng = c.Rng(808);
            float ph = (float)rng.NextDouble() * 6.283f;
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;
            Vector3 p0 = c.startPos;
            float t0 = Time.unscaledTime;
            while (true)
            {
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                // Trôi: nhanh lúc đầu rồi chậm dần như theo dòng nước.
                float e = 1f - (1f - u) * (1f - u) * (1f - u);
                Vector3 right = Vector3.Cross(c.up, c.front);
                float fade = 1f - Ease(Mathf.InverseLerp(0.7f, 1f, u));   // sóng lặng dần khi tới khe
                float bob = Mathf.Sin(u * 6.283f * 2f + ph) * 0.06f * c.S * fade;
                float sway = Mathf.Sin(u * 6.283f * 1.25f + ph * 0.5f) * 0.12f * c.S * side * fade;
                root.position = Vector3.Lerp(p0, c.RestPos, e) + c.up * bob + right * sway;
                float yaw = 50f * side * (1f - e);
                float rollW = Mathf.Sin(u * 6.283f * 2f + ph + 1.2f) * 9f * fade;
                float pitchW = Mathf.Cos(u * 6.283f * 1.5f + ph) * 6f * fade;
                root.rotation = c.Facing * Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.Euler(pitchW, 0f, rollW);
                root.localScale = Vector3.one * c.keySize;
                Pose(root, 25f, 1f + 0.12f * Mathf.Sin(u * 6.283f * 7f + ph));
                if (u >= 1f) yield break;
                yield return null;
            }
        }

        /// <summary>Mở: đặt nhẹ vào khe như thả hoa đăng (nhún xuống rồi nhích lên), cánh sen nở xoè ra, lửa bừng lên, rồi thu nhỏ tại khe.</summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            float t0 = Time.unscaledTime;
            while (true)
            {
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                ShrinkAtSlot(c, root, u);
                // Nhún nhẹ khi chạm (0..0,4), không lệch khỏi khe ở cuối.
                float dip = u < 0.4f ? -Mathf.Sin(Mathf.PI * u / 0.4f) * 0.03f * c.S : 0f;
                root.position += c.up * dip;
                root.rotation = c.Facing;
                float bloom = Ease(Mathf.InverseLerp(0.1f, 0.7f, u));
                float flame = 1f + 0.6f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(u / 0.85f));
                Pose(root, Mathf.Lerp(25f, 75f, bloom), flame);
                if (u >= 1f) yield break;
                yield return null;
            }
        }

        /// <summary>Nháy sáng: ấm và dịu, tắt chậm hơn chút (0,7 s) như ánh nến.</summary>
        protected override void OnKeyImpact(KeyContext c)
        {
            if (Reveal != null) Reveal.PulseGlow(c.preset.lightColor, c.preset.lightMul, 0.7f);
        }
    }
}

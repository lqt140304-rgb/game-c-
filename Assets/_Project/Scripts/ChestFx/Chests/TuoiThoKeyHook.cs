using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Tuổi thơ: VIÊN BI VE (cầu xanh trong + 3 lá xoáy nhiều màu ở lõi).
    /// Đợt 2: bay = lăn tưng tưng (3 nảy thấp dần, bi quay theo quãng lăn); mở = búng bi (lùi lấy đà xoay tít,
    /// bật mạnh vào khe, nảy lại một chút rồi chui vào). Sáng xanh dương sáng, rương nảy (Bounce).
    /// </summary>
    public class TuoiThoKeyHook : ChestKeyHookBase
    {
        private static KeyPreset Make()
        {
            KeyPreset p = KeyPreset.Neutral("Chest_TuoiTho", "TuoiTho", "Assets/_Project/Audio/Chests/tuoi_tho/", "sfx_tuoitho_",
                "sfx_tuoitho_roll_loop.mp3", new Color(0.3f, 0.7f, 0.95f), KeyAction.Drop);
            p.keyFile = "sfx_tuoitho_bonk.mp3";
            p.jumpMul = 1.1f; p.cycleDelta = 1;   // tưng tưng: chu kỳ +1, Jump +10%
            p.flyFile = "sfx_tuoitho_fly.mp3";
            p.actionFile = "sfx_tuoitho_action.mp3";
            p.lightColor = new Color(0.35f, 0.75f, 1f);   // xanh dương sáng như bi ve dưới nắng
            p.lightMul = 0.6f;
            p.moveStyle = ChestMoveStyle.Bounce;
            return p;
        }

        public static readonly KeyPreset Preset = Make();

        // Nhóm 3 lá xoáy của từng viên bi, tra theo gốc vật (không dùng static).
        private readonly Dictionary<Transform, Transform> _swirl = new Dictionary<Transform, Transform>();

        private static readonly float[] BounceEnd = { 0.42f, 0.74f, 1f };
        private static readonly float[] BounceH = { 0.45f, 0.22f, 0.09f };

        public override KeyPreset GetPreset() { return Preset; }

        protected override GameObject BuildKey(KeyContext c)
        {
            GameObject root = new GameObject("Key");
            c.Part(root.transform, PrimitiveType.Sphere, "ViBi", Vector3.zero, Vector3.one * 0.92f, Preset.mainColor);

            GameObject swirl = new GameObject("Xoay");
            swirl.transform.SetParent(root.transform, false);
            Color[] cols = { new Color(0.95f, 0.25f, 0.3f), new Color(1f, 0.85f, 0.2f), new Color(0.35f, 0.85f, 0.4f) };
            for (int i = 0; i < 3; i++)
            {
                // Lá dẹt xếp tròn quanh trục Z, hơi nhô ra khỏi mặt cầu để thấy được (vật liệu không trong suốt).
                GameObject arm = new GameObject("La" + i);
                arm.transform.SetParent(swirl.transform, false);
                arm.transform.localRotation = Quaternion.Euler(0f, 0f, i * 120f);
                c.Part(arm.transform, PrimitiveType.Cube, "Canh", new Vector3(0.24f, 0f, 0f), new Vector3(0.5f, 0.06f, 0.5f), Quaternion.Euler(0f, 0f, 20f), cols[i]);
            }
            _swirl[root.transform] = swirl.transform;
            return root;
        }

        private void SetSwirl(Transform root, float angle)
        {
            Transform sw;
            if (root != null && _swirl.TryGetValue(root, out sw) && sw != null) sw.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private static float Sm(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        /// <summary>Bay: lăn tưng tưng từ điểm khởi tới khe, 3 nảy thấp dần, bi quay theo quãng lăn.</summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            System.Random rng = c.Rng(909);
            float side = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.15f;   // lệch ngang nhẹ, khác mỗi lần
            float t0 = Time.unscaledTime;
            float roll = 0f;
            Vector3 last = root.position;
            while (true)
            {
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float S = c.S;
                Vector3 right = Vector3.Cross(c.up, c.front);
                float e = 1f - (1f - u) * (1f - u);   // nhanh đầu, chậm dần khi tới khe
                Vector3 p = Vector3.Lerp(c.startPos, c.RestPos, e) + right * (side * S * Mathf.Sin(Mathf.PI * u));
                // Nảy parabol trong từng đoạn (về 0 đúng lúc u = 1).
                float h = 0f, s0 = 0f;
                for (int i = 0; i < BounceEnd.Length; i++)
                {
                    if (u <= BounceEnd[i]) { float k = (u - s0) / (BounceEnd[i] - s0); h = BounceH[i] * 4f * k * (1f - k); break; }
                    s0 = BounceEnd[i];
                }
                p += c.up * (h * S);
                root.position = p;
                root.localScale = Vector3.one * c.keySize;
                // Lăn: quay quanh trục ngang theo quãng đi ngang (bán kính = nửa cỡ bi).
                Vector3 d = p - last;
                d -= c.up * Vector3.Dot(d, c.up);
                roll += d.magnitude / Mathf.Max(1e-4f, c.tip) * Mathf.Rad2Deg;
                last = p;
                root.rotation = Quaternion.AngleAxis(roll, right) * c.Facing;
                SetSwirl(root, 540f * u);
                if (u >= 1f) break;
                yield return null;
            }
        }

        /// <summary>Mở: búng bi. Lùi lấy đà xoay tít (0..0.25), bật mạnh vào khe (0.25..0.45), nảy lại (0.45..0.65), chui vào.</summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            float t0 = Time.unscaledTime;
            while (true)
            {
                c.Refresh();
                float t = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float S = c.S;
                float off;   // khoảng lùi khỏi khe theo hướng trước, theo S
                if (t < 0.25f) off = 0.12f * Sm(0f, 0.25f, t);
                else if (t < 0.45f) { float k = (t - 0.25f) / 0.2f; off = 0.12f * (1f - k * k); }
                else if (t < 0.65f) off = 0.04f * Mathf.Sin(Mathf.PI * (t - 0.45f) / 0.2f);
                else off = 0f;
                // Dẹt nhẹ lúc đập vào khe.
                float squash = (t > 0.42f && t < 0.52f) ? 0.15f * Mathf.Sin(Mathf.PI * (t - 0.42f) / 0.1f) : 0f;
                Vector3 right = Vector3.Cross(c.up, c.front);
                if (t < 0.85f)
                {
                    root.position = c.RestPos + c.front * (off * S);
                    root.localScale = new Vector3(1f + squash, 1f + squash, 1f - squash) * c.keySize;
                }
                else ShrinkAtSlot(c, root, t);
                float spin = 900f * Sm(0f, 0.45f, t) + 180f * Sm(0.45f, 1f, t);
                root.rotation = Quaternion.AngleAxis(-spin, right) * c.Facing;
                SetSwirl(root, 540f + 720f * t);
                if (t >= 1f) break;
                yield return null;
            }
        }

        protected override void OnKeyImpact(KeyContext c)
        {
            // Nháy ngắn, gọn như tiếng "cách" của bi chạm.
            if (Reveal != null) Reveal.PulseGlow(Preset.lightColor, Preset.lightMul, 0.35f);
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            _swirl.Clear();
            base.OnReset(reveal);
        }
    }
}

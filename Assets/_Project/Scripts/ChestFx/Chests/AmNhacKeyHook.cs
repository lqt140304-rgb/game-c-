using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương AmNhac: đĩa than. Đợt 2: đĩa lăn đứng như bánh xe (nảy nhẹ theo nhịp) rồi ngả nằm xuống trước khe;
    /// lúc mở thì đĩa quay tít, khựng lại "scratch" lùi tới rồi chìm vào khe. Sáng hồng neon, rương nảy theo nhịp (Bounce).
    /// </summary>
    public class AmNhacKeyHook : ChestKeyHookBase
    {
        // Nhún theo nhịp: chu kỳ Squash/Jump +1.
        public static readonly KeyPreset Preset = Make();

        static KeyPreset Make()
        {
            var p = KeyPreset.Neutral("Chest_AmNhac", "AmNhac", "Assets/_Project/Audio/Chests/am_nhac/", "sfx_amnhac_",
                "sfx_amnhac_drumroll.mp3", new Color(0.95f, 0.4f, 0.65f), KeyAction.Insert);
            p.keyFile = "sfx_amnhac_coin_slot.mp3";
            p.cycleDelta = 1;
            p.flyFile = "sfx_amnhac_fly.mp3";
            p.actionFile = "sfx_amnhac_action.mp3";
            p.lightColor = new Color(1f, 0.25f, 0.75f);   // hồng neon
            p.lightMul = 0.7f;
            p.moveStyle = ChestMoveStyle.Bounce;
            return p;
        }

        public override KeyPreset GetPreset() => Preset;

        /// <summary>Đĩa than dựng đứng (mặt đĩa vuông góc trục X, mép đĩa là phần "đầu" quay về +Z): đĩa đen + 3 rãnh + nhãn hồng giữa.</summary>
        protected override GameObject BuildKey(KeyContext c)
        {
            GameObject root = new GameObject("Key");
            Quaternion faceX = Quaternion.Euler(0f, 0f, 90f);   // trụ Unity dọc Y; xoay để trục đĩa dọc X
            Color black = new Color(0.07f, 0.07f, 0.08f);
            Color groove = new Color(0.22f, 0.22f, 0.25f);
            c.Part(root.transform, PrimitiveType.Cylinder, "Dia", Vector3.zero, new Vector3(0.5f, 0.030f, 0.5f), faceX, black);
            c.Part(root.transform, PrimitiveType.Cylinder, "Ranh1", Vector3.zero, new Vector3(0.43f, 0.031f, 0.43f), faceX, groove);
            c.Part(root.transform, PrimitiveType.Cylinder, "Ranh2", Vector3.zero, new Vector3(0.34f, 0.032f, 0.34f), faceX, black);
            c.Part(root.transform, PrimitiveType.Cylinder, "Ranh3", Vector3.zero, new Vector3(0.26f, 0.033f, 0.26f), faceX, groove);
            c.Part(root.transform, PrimitiveType.Cylinder, "Nhan", Vector3.zero, new Vector3(0.17f, 0.036f, 0.17f), faceX, Preset.mainColor);
            return root;
        }

        static float Smooth(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        /// <summary>Hướng đĩa nằm ngửa (trục đĩa X quay lên theo c.up) đã quay quanh trục lên một góc.</summary>
        static Quaternion Flat(KeyContext c, float spinDeg)
        {
            return Quaternion.AngleAxis(spinDeg, c.up) * c.Facing * Quaternion.Euler(0f, 0f, 90f);
        }

        /// <summary>
        /// Bay 1,0 s: 0–0,72 lăn đứng như bánh xe từ điểm khởi bay về trước khe (quay quanh trục đĩa theo quãng đường,
        /// nảy nhẹ 3 nhịp, lảo đảo nhẹ); 0,72–1,0 ngả nằm xuống, vừa xoay chậm vừa trượt về c.RestPos.
        /// </summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            System.Random rng = c.Rng(707);
            float wobblePhase = (float)rng.NextDouble() * 6.28f;
            float t = 0f;
            root.localScale = Vector3.one * c.keySize;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / seconds);
                c.Refresh();
                float roll = Smooth(0f, 0.72f, u);
                // Lăn thẳng từ điểm bay về điểm chờ (trước khe 0,6 S), sau đó trượt nốt vào RestPos.
                Vector3 wait = c.RestPos + c.front * (0.6f * c.S);
                Vector3 pos = u < 0.72f ? Vector3.Lerp(c.startPos, wait, roll) : Vector3.Lerp(wait, c.RestPos, Smooth(0.72f, 1f, u));
                float hop = Mathf.Abs(Mathf.Sin(roll * Mathf.PI * 3f)) * 0.08f * c.S * (1f - roll);
                pos += c.up * hop;
                float dist = Vector3.Distance(c.startPos, wait) * roll;
                float rollDeg = dist / Mathf.Max(0.001f, c.tip) * Mathf.Rad2Deg;
                float wob = Mathf.Sin(u * 18f + wobblePhase) * 6f * (1f - roll);
                Quaternion upright = c.Facing * Quaternion.AngleAxis(wob, Vector3.forward) * Quaternion.AngleAxis(rollDeg, Vector3.right);
                float lay = Smooth(0.72f, 1f, u);
                root.position = pos;
                root.rotation = Quaternion.Slerp(upright, Flat(c, lay * 90f), lay);
                yield return null;
            }
        }

        /// <summary>
        /// Mở 0,35 s: đĩa nằm ở khe quay tít (0–0,45), giật lùi-tới hai lần như "scratch" (0,45–0,8), rồi thu nhỏ chìm vào khe.
        /// Chỉ xoay chính đĩa (nhãn quay theo), không sinh vật mới.
        /// </summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            float t = 0f;
            float spin = 90f;
            float prev = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / seconds);
                c.Refresh();
                float dt = u - prev; prev = u;
                if (u < 0.45f) spin += dt * 2600f * (0.4f + u / 0.45f);                       // quay tít dần nhanh
                else if (u < 0.8f) spin += Mathf.Cos((u - 0.45f) / 0.35f * Mathf.PI * 4f) * dt * 3200f; // scratch lùi-tới
                else spin += dt * 900f;
                float dip = Mathf.Sin(Mathf.Clamp01(u / 0.12f) * Mathf.PI) * 4f;               // nảy nhẹ lúc "kim đặt xuống"
                root.rotation = Flat(c, spin) * Quaternion.AngleAxis(dip, Vector3.forward);
                ShrinkAtSlot(c, root, u);
                yield return null;
            }
            ShrinkAtSlot(c, root, 1f);
        }
    }
}

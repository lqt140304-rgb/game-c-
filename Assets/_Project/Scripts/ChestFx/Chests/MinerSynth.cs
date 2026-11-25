using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Âm riêng của rương Thợ Mỏ, TỔNG HỢP lúc chạy (AudioClip.Create + SetData), không cần file và không tốn credit.
    /// Mỗi âm tạo một lần rồi cache. Nhiễu dùng hạt giống cố định nên âm luôn giống nhau giữa các lần chạy.
    /// CHƯA AI NGHE THỬ: thông số chọn theo lý thuyết; nghe trong Unity rồi chỉnh hằng số ở đây.
    /// </summary>
    public static class MinerSynth
    {
        private const int Rate = 44100;
        private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();

        /// <summary>Tiếng mỏ rền (vòng lặp 2 s): nhiễu nâu trầm + 38 Hz. Pitch của nguồn phát được kéo lên dần.</summary>
        public static AudioClip Rumble => Get("miner_rumble", 2f, (d, rng) =>
        {
            float b = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                b = (b + 0.02f * W(rng)) / 1.02f;
                d[i] = b * 3.2f + 0.18f * Mathf.Sin(2f * Mathf.PI * 38f * t);
            }
            Fade(d, 0.05f, 0.05f); // giảm tiếng click ở chỗ nối vòng lặp
        });

        /// <summary>Nhát cuốc vào đá: tiếng cách + ba họa âm kim loại tắt nhanh.</summary>
        public static AudioClip PickClink => Get("miner_clink", 0.4f, (d, rng) =>
        {
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                float ring = Mathf.Sin(2f * Mathf.PI * 2600f * t) + 0.6f * Mathf.Sin(2f * Mathf.PI * 3900f * t) + 0.35f * Mathf.Sin(2f * Mathf.PI * 5300f * t);
                float click = t < 0.006f ? W(rng) : 0f;
                d[i] = 0.35f * ring * Mathf.Exp(-t * 16f) + 0.8f * click;
            }
        });

        /// <summary>Đá nứt vỡ: nhiễu lọc thấp, tắt nhanh.</summary>
        public static AudioClip RockCrack => Get("miner_crack", 0.45f, (d, rng) =>
        {
            float lp = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                lp += 0.18f * (W(rng) - lp);
                d[i] = 1.6f * lp * Mathf.Exp(-t * 9f) + (t < 0.01f ? 0.5f * W(rng) : 0f);
            }
        });

        /// <summary>Nhịp tim mỏ (thình): 60 Hz tụt cao độ. Phát lặp, khoảng cách ngắn dần để dâng độ căng.</summary>
        public static AudioClip Thump => Get("miner_thump", 0.3f, (d, rng) =>
        {
            float ph = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                ph += 2f * Mathf.PI * Mathf.Lerp(75f, 42f, t / 0.3f) / Rate;
                d[i] = 0.9f * Mathf.Sin(ph) * Mathf.Exp(-t * 13f);
            }
        });

        /// <summary>Ngòi nổ cháy xèo xèo (vòng lặp 1 s): nhiễu lọc cao + tiếng lách tách.</summary>
        public static AudioClip FuseHiss => Get("miner_fuse", 1f, (d, rng) =>
        {
            float prev = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float w = W(rng);
                float hp = w - prev; prev = w;
                float pop = rng.NextDouble() < 0.0012 ? W(rng) * 2.5f : 0f;
                d[i] = 0.22f * hp + pop * 0.5f;
            }
            Fade(d, 0.02f, 0.02f);
        });

        /// <summary>Thuốc nổ / bùng nổ: quét 90→28 Hz + nhiễu trầm, nén mềm.</summary>
        public static AudioClip Boom => Get("miner_boom", 2.4f, (d, rng) =>
        {
            float ph = 0f, lp = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                ph += 2f * Mathf.PI * Mathf.Lerp(90f, 28f, Mathf.Clamp01(t / 1.2f)) / Rate;
                lp += 0.06f * (W(rng) - lp);
                float x = 1.3f * Mathf.Sin(ph) * Mathf.Exp(-t * 1.8f) + 3.5f * lp * Mathf.Exp(-t * 2.6f) + (t < 0.02f ? W(rng) : 0f);
                d[i] = SoftClip(x);
            }
        });

        /// <summary>"Xịt" (bậc thấp): xì hơi + còi trượt tụt 900→140 Hz rung rung, kết bằng tiếng "phụt" nhỏ. Hài.</summary>
        public static AudioClip Fizzle => Get("miner_fizzle", 1.5f, (d, rng) =>
        {
            float ph = 0f, prev = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                float k = Mathf.Clamp01(t / 1.1f);
                float f = Mathf.Lerp(900f, 140f, k * k) * (1f + 0.03f * Mathf.Sin(2f * Mathf.PI * 7f * t));
                ph += 2f * Mathf.PI * f / Rate;
                float w = W(rng); float hp = w - prev; prev = w;
                float whistle = t < 1.15f ? 0.4f * Mathf.Sin(ph) * Mathf.Min(1f, t * 20f) * (1f - k * 0.5f) : 0f;
                float hiss = 0.25f * hp * Mathf.Exp(-t * 3f);
                float pft = t > 1.2f ? 0.6f * W(rng) * Mathf.Exp(-(t - 1.2f) * 25f) : 0f;
                d[i] = whistle + hiss + pft;
            }
        });

        /// <summary>Chim hoàng yến hót: hai tiếng quét lên.</summary>
        public static AudioClip Chirp => Get("miner_chirp", 0.32f, (d, rng) =>
        {
            float ph = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                float local = t < 0.14f ? t : t - 0.16f;
                bool on = t < 0.14f || (t > 0.16f && t < 0.3f);
                ph += 2f * Mathf.PI * Mathf.Lerp(2400f, 4200f, Mathf.Clamp01(local / 0.14f)) / Rate;
                d[i] = on ? 0.3f * Mathf.Sin(ph) * Mathf.Sin(Mathf.PI * Mathf.Clamp01(local / 0.14f)) : 0f;
            }
        });

        /// <summary>Chim ngất (bậc thấp): tiếng "chíp" tụt xuống.</summary>
        public static AudioClip ChirpFaint => Get("miner_chirp_faint", 0.5f, (d, rng) =>
        {
            float ph = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                ph += 2f * Mathf.PI * Mathf.Lerp(3200f, 700f, t / 0.5f) / Rate;
                d[i] = 0.28f * Mathf.Sin(ph) * Mathf.Exp(-t * 4f);
            }
        });

        /// <summary>Đá nhỏ rơi cốc lên nắp (bậc Rác).</summary>
        public static AudioClip Bonk => Get("miner_bonk", 0.25f, (d, rng) =>
        {
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                d[i] = 0.7f * Mathf.Sin(2f * Mathf.PI * 520f * t) * Mathf.Exp(-t * 30f) + (t < 0.004f ? 0.5f * W(rng) : 0f);
            }
        });

        /// <summary>Vàng cục rơi leng keng (Huyền thoại).</summary>
        public static AudioClip Nugget => Get("miner_nugget", 0.3f, (d, rng) =>
        {
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                d[i] = 0.3f * (Mathf.Sin(2f * Mathf.PI * 1850f * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * 4700f * t)) * Mathf.Exp(-t * 14f);
            }
        });

        // ------------------------------------------------------------------ helpers

        private static AudioClip Get(string name, float seconds, Action<float[], System.Random> fill)
        {
            if (Cache.TryGetValue(name, out AudioClip c) && c != null) return c;
            float[] data = new float[Mathf.Max(1, (int)(seconds * Rate))];
            fill(data, new System.Random(name.GetHashCode() ^ 0x5eed));
            for (int i = 0; i < data.Length; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
            c = AudioClip.Create(name, data.Length, 1, Rate, false);
            c.SetData(data, 0);
            Cache[name] = c;
            return c;
        }

        private static float W(System.Random r) => (float)(r.NextDouble() * 2.0 - 1.0);

        private static float SoftClip(float x) => x / (1f + Mathf.Abs(x));

        private static void Fade(float[] d, float inSec, float outSec)
        {
            int a = Mathf.Min(d.Length, (int)(inSec * Rate)), b = Mathf.Min(d.Length, (int)(outSec * Rate));
            for (int i = 0; i < a; i++) d[i] *= i / (float)a;
            for (int i = 0; i < b; i++) d[d.Length - 1 - i] *= i / (float)b;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Âm căng thẳng tổng hợp bằng mã (WAV mono 44,1 kHz tạo lúc chạy bằng AudioClip.Create), để rương nào cũng có tiếng
    /// ngay khi bấm mở dù chưa có file âm riêng. Không dùng mẫu có bản quyền. Mọi "ngẫu nhiên" ở đây là
    /// <see cref="System.Random"/> với hạt cố định, nên âm giống hệt nhau mỗi lần. Chỉ là âm tạm: rương chủ đề nên thay bằng
    /// clip riêng (ChestTensionSettings.loopClip, pulseClip, deflateClip, swellClip).
    /// CHƯA ai nghe thử các âm này (xem spec: phần "Chưa kiểm chứng").
    /// </summary>
    public static class ChestTensionSynth
    {
        public const int SampleRate = 44100;
        private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();

        /// <summary>
        /// Nền căng lặp liền mạch, dài đúng 1 giây: toàn thành phần có số chu kỳ nguyên trong 1 s (110 Hz, 111 Hz, 55 Hz,
        /// tremolo 4 Hz) nên đầu và cuối khớp pha, không có tiếng "cạch" ở chỗ lặp. Hoà âm lẻ (răng cưa) tạo cảm giác gắt.
        /// </summary>
        public static AudioClip Loop() => Get("tension_loop", () =>
        {
            int n = SampleRate;
            float[] d = new float[n];
            for (int i = 0; i < n; i++)
            {
                double t = i / (double)SampleRate;
                double v = 0.0;
                for (int h = 1; h <= 6; h++) v += Math.Sin(2.0 * Math.PI * 110.0 * h * t) / h;      // răng cưa 110 Hz
                for (int h = 1; h <= 3; h++) v += 0.8 * Math.Sin(2.0 * Math.PI * 111.0 * h * t) / h; // lệch 1 Hz: tạo nhịp phách
                v += 0.9 * Math.Sin(2.0 * Math.PI * 55.0 * t);                                       // trầm
                double trem = 0.75 + 0.25 * Math.Sin(2.0 * Math.PI * 4.0 * t);
                d[i] = (float)(v * trem);
            }
            Normalize(d, 0.6f);
            return d;
        });

        /// <summary>Tiếng "tim đập" ngắn: sóng sin trượt 130 → 48 Hz, tắt nhanh.</summary>
        public static AudioClip Pulse() => Get("tension_pulse", () =>
        {
            int n = (int)(0.2f * SampleRate);
            float[] d = new float[n];
            double phase = 0.0;
            for (int i = 0; i < n; i++)
            {
                double t = i / (double)SampleRate;
                double f = 48.0 + (130.0 - 48.0) * Math.Exp(-t * 28.0);
                phase += 2.0 * Math.PI * f / SampleRate;
                double env = Math.Exp(-t * 20.0) * Math.Min(1.0, t * 800.0);
                d[i] = (float)(Math.Sin(phase) * env);
            }
            Normalize(d, 0.9f);
            return d;
        });

        /// <summary>
        /// "Xịt" (anti-climax, bậc thấp): âm trượt rơi 640 → 70 Hz có rung (như kèn tụt giọng) cộng tiếng xì ngắn.
        /// </summary>
        public static AudioClip Fizzle() => Get("tension_fizzle", () =>
        {
            int n = (int)(0.85f * SampleRate);
            float[] d = new float[n];
            System.Random rng = new System.Random(4242);
            double phase = 0.0, lp = 0.0;
            for (int i = 0; i < n; i++)
            {
                double t = i / (double)SampleRate;
                double u = t / 0.85;
                double f = 70.0 + 570.0 * Math.Pow(1.0 - u, 2.2);
                f *= 1.0 + 0.06 * Math.Sin(2.0 * Math.PI * 7.0 * t);             // rung wah-wah
                phase += 2.0 * Math.PI * f / SampleRate;
                double tone = (Math.Sin(phase) + 0.5 * Math.Sin(phase * 2.0)) * Math.Pow(1.0 - u, 1.5) * 0.7;
                lp += 0.35 * ((rng.NextDouble() * 2.0 - 1.0) - lp);              // nhiễu lọc thấp = tiếng xì
                double hiss = lp * Math.Exp(-t * 6.0) * 0.9;
                double env = Math.Min(1.0, t * 400.0);
                d[i] = (float)((tone + hiss) * env);
            }
            Normalize(d, 0.8f);
            return d;
        });

        /// <summary>
        /// Dâng/bùng. <paramref name="intensity"/> 0..1: càng cao càng dài, càng nhiều hoà âm chồng (hợp âm 1 – 3 – 5 – 8, lấp lánh).
        /// Khoá về 3 mức (0.35 / 0.65 / 0.85 / 1) để đệm bộ nhớ.
        /// </summary>
        public static AudioClip Swell(float intensity)
        {
            float q = intensity >= 0.95f ? 1f : intensity >= 0.75f ? 0.85f : intensity >= 0.5f ? 0.65f : 0.35f;
            return Get("tension_swell_" + q, () =>
            {
                double len = 0.6 + 1.4 * q;
                int n = (int)(len * SampleRate);
                float[] d = new float[n];
                System.Random rng = new System.Random(777);
                int voices = q >= 0.85f ? 4 : q >= 0.65f ? 3 : 2;
                double[] ratios = { 1.0, 1.25, 1.5, 2.0 };
                double[] shimmerHz = new double[10];
                for (int k = 0; k < shimmerHz.Length; k++) shimmerHz[k] = 2200.0 + rng.NextDouble() * 3800.0;
                for (int i = 0; i < n; i++)
                {
                    double t = i / (double)SampleRate;
                    double u = t / len;
                    double rise = 0.85 + 0.35 * Math.Min(1.0, t / 0.25);          // vọt lên một chút lúc đầu
                    double v = 0.0;
                    for (int k = 0; k < voices; k++) v += Math.Sin(2.0 * Math.PI * 220.0 * ratios[k] * rise * t) / (1.0 + 0.3 * k);
                    double env = Math.Min(1.0, t * 90.0) * Math.Exp(-t * (3.2 - 1.6 * q));
                    double shim = 0.0;
                    if (q >= 0.65f) for (int k = 0; k < shimmerHz.Length; k++) shim += Math.Sin(2.0 * Math.PI * shimmerHz[k] * t) * Math.Exp(-t * (4.0 + k * 0.4));
                    d[i] = (float)((v * env + shim * 0.12 * q) * (1.0 - u * u));
                }
                Normalize(d, 0.85f);
                return d;
            });
        }

        /// <summary>Tiếng ầm trầm kèm theo bùng nổ (Huyền thoại): sin 62 → 28 Hz kéo dài 1,2 s.</summary>
        public static AudioClip Boom() => Get("tension_boom", () =>
        {
            int n = (int)(1.2f * SampleRate);
            float[] d = new float[n];
            double phase = 0.0;
            for (int i = 0; i < n; i++)
            {
                double t = i / (double)SampleRate;
                double f = 28.0 + 34.0 * Math.Exp(-t * 4.0);
                phase += 2.0 * Math.PI * f / SampleRate;
                d[i] = (float)(Math.Sin(phase) * Math.Exp(-t * 2.6) * Math.Min(1.0, t * 300.0));
            }
            Normalize(d, 0.95f);
            return d;
        });

        private static AudioClip Get(string key, Func<float[]> build)
        {
            AudioClip c;
            if (Cache.TryGetValue(key, out c) && c != null) return c;   // "c != null" bắt cả clip Unity đã bị huỷ
            float[] data = build();
            c = AudioClip.Create(key, data.Length, 1, SampleRate, false);
            c.SetData(data, 0);
            c.hideFlags = HideFlags.DontSave;
            Cache[key] = c;
            return c;
        }

        private static void Normalize(float[] d, float peak)
        {
            float max = 0f;
            for (int i = 0; i < d.Length; i++) max = Math.Max(max, Math.Abs(d[i]));
            if (max < 1e-6f) return;
            float k = peak / max;
            for (int i = 0; i < d.Length; i++) d[i] *= k;
        }
    }
}

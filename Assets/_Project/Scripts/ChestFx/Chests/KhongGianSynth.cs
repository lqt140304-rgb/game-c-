using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Âm riêng của rương Không Gian, tổng hợp ngay trong code (AudioClip.Create + SetData) nên không cần file:
    /// máy nổ lặp (độ căng điều khiển bằng pitch/âm lượng), tiếng nạp pin, tên lửa phóng, nổ, máy xịt, "xì hơi",
    /// dù bung, tiếng bíp radio và hợp âm chiến thắng. Mỗi clip tạo một lần rồi dùng lại (cache tĩnh).
    /// LƯU Ý: chưa nghe thử trong Unity; các tham số là ước lượng, chỉnh ở đây nếu nghe chưa hay.
    /// </summary>
    public static class KhongGianSynth
    {
        private const int Rate = 44100;
        private static AudioClip _engine, _clunk, _launch, _boom, _sputter, _deflate, _chute, _blip;
        private static readonly AudioClip[] _fanfare = new AudioClip[5];

        /// <summary>Máy nổ lặp 2 s: răng cưa 46/92 Hz + nhiễu nâu + rung pít-tông. Lặp khít (số chu kỳ nguyên).</summary>
        public static AudioClip Engine => _engine != null ? _engine : (_engine = Make("KG_Engine", 2f, (t, n) =>
        {
            float saw = Saw(t * 46f) * 0.45f + Saw(t * 92f) * 0.25f + Saw(t * 138f) * 0.1f;
            float chug = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 12f * t);   // 12 nhịp/giây, lặp khít trong 2 s
            return (saw * chug + n.Brown() * 0.5f) * 0.6f;
        }, true));

        /// <summary>Pin cắm vào: tiếng "cộp" trầm + tiếng ping kim loại.</summary>
        public static AudioClip Clunk => _clunk != null ? _clunk : (_clunk = Make("KG_Clunk", 0.35f, (t, n) =>
            Mathf.Sin(2f * Mathf.PI * (110f - 120f * t) * t) * Env(t, 0.002f, 0.08f) * 0.8f
            + Mathf.Sin(2f * Mathf.PI * 1760f * t) * Env(t, 0.001f, 0.12f) * 0.25f));

        /// <summary>Tên lửa rời bệ: nhiễu trắng lọc, cao dần, 1.2 s.</summary>
        public static AudioClip Launch => _launch != null ? _launch : (_launch = Make("KG_Launch", 1.2f, (t, n) =>
        {
            float u = t / 1.2f;
            return n.Lowpass(n.White(), 0.05f + 0.5f * u) * Mathf.Min(1f, t * 12f) * (1f - u * u) * 0.9f;
        }));

        /// <summary>Nổ: nhiễu ập vào rồi tắt dần + sóng sub trượt từ 90 xuống 30 Hz.</summary>
        public static AudioClip Boom => _boom != null ? _boom : (_boom = Make("KG_Boom", 1.6f, (t, n) =>
            n.Lowpass(n.White(), 0.25f * Mathf.Exp(-t * 2f) + 0.02f) * Mathf.Exp(-t * 3.5f) * 1.4f
            + Mathf.Sin(2f * Mathf.PI * (90f * Mathf.Exp(-t * 1.2f) + 30f) * t) * Mathf.Exp(-t * 2.2f) * 0.9f));

        /// <summary>Máy hụt hơi: 5 tiếng "pụp" thưa dần.</summary>
        public static AudioClip Sputter => _sputter != null ? _sputter : (_sputter = Make("KG_Sputter", 1.4f, (t, n) =>
        {
            float[] at = { 0f, 0.18f, 0.42f, 0.75f, 1.15f };
            float s = 0f;
            for (int i = 0; i < at.Length; i++)
            {
                float d = t - at[i];
                if (d >= 0f) s += (n.White() * 0.6f + Saw(d * 60f) * 0.6f) * Env(d, 0.003f, 0.06f) * (1f - i * 0.15f);
            }
            return s * 0.8f;
        }));

        /// <summary>Bậc Rác: "xìiiì" — răng cưa trượt từ 600 xuống 70 Hz kèm hơi xì, 1.3 s (hài, rớt đột ngột).</summary>
        public static AudioClip Deflate => _deflate != null ? _deflate : (_deflate = Make("KG_Deflate", 1.3f, (t, n) =>
        {
            float u = t / 1.3f;
            float f = Mathf.Lerp(600f, 70f, Mathf.Sqrt(u));
            float wobble = 1f + 0.08f * Mathf.Sin(2f * Mathf.PI * 9f * t);
            return (Saw(PhaseAcc(ref _defPhase, f * wobble)) * 0.35f + n.Lowpass(n.White(), 0.4f) * 0.25f) * (1f - u) * Mathf.Min(1f, t * 30f);
        }));
        private static float _defPhase;

        /// <summary>Dù bung: "phựt" ngắn.</summary>
        public static AudioClip Chute => _chute != null ? _chute : (_chute = Make("KG_Chute", 0.4f, (t, n) =>
            n.Lowpass(n.White(), 0.15f) * Env(t, 0.01f, 0.12f) * 1.2f));

        /// <summary>Bíp radio của tàu mẹ.</summary>
        public static AudioClip Blip => _blip != null ? _blip : (_blip = Make("KG_Blip", 0.25f, (t, n) =>
            Mathf.Sign(Mathf.Sin(2f * Mathf.PI * (t < 0.1f ? 1320f : 1760f) * t)) * 0.18f * Env(t, 0.005f, 0.2f)));

        /// <summary>Hợp âm rải đi lên, số nốt = bậc + 1 (Huyền thoại 5 nốt, ngân dài).</summary>
        public static AudioClip Fanfare(ChestTier tier)
        {
            int r = Mathf.Clamp(tier.Rank(), 0, 4);
            if (_fanfare[r] != null) return _fanfare[r];
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f };
            float len = 0.9f + 0.35f * r;
            return _fanfare[r] = Make("KG_Fanfare" + r, len, (t, n) =>
            {
                float s = 0f;
                for (int i = 0; i <= r; i++)
                {
                    float d = t - i * 0.11f;
                    if (d < 0f) continue;
                    float f = notes[i];
                    s += (Mathf.Sin(2f * Mathf.PI * f * d) + 0.3f * Mathf.Sin(4f * Mathf.PI * f * d)) * Env(d, 0.01f, 0.5f + 0.2f * r);
                }
                return s * 0.22f;
            });
        }

        // ------------------------------------------------------------------ Bên trong

        private delegate float Gen(float t, Noise n);

        private class Noise
        {
            private uint _s = 22222u;
            private float _brown, _lp;
            public float White() { _s ^= _s << 13; _s ^= _s >> 17; _s ^= _s << 5; return (_s & 0xFFFFFF) / 8388607.5f - 1f; }
            public float Brown() { _brown = Mathf.Clamp(_brown + White() * 0.06f, -1f, 1f); return _brown; }
            public float Lowpass(float x, float k) { _lp += (x - _lp) * Mathf.Clamp(k, 0.001f, 1f); return _lp; }
        }

        private static float Saw(float phase) => 2f * (phase - Mathf.Floor(phase)) - 1f;
        private static float PhaseAcc(ref float phase, float freq) { phase += freq / Rate; return phase; }
        private static float Env(float t, float attack, float decay) => t < attack ? t / attack : Mathf.Exp(-(t - attack) / decay);

        private static AudioClip Make(string name, float seconds, Gen gen, bool loop = false)
        {
            int count = Mathf.CeilToInt(seconds * Rate);
            float[] data = new float[count];
            Noise n = new Noise();
            _defPhase = 0f;
            float fade = loop ? 0f : 0.005f * Rate;   // tránh tiếng click cuối clip
            for (int i = 0; i < count; i++)
            {
                float v = gen(i / (float)Rate, n);
                if (!loop && i > count - fade) v *= (count - i) / fade;
                data[i] = Mathf.Clamp(v, -1f, 1f);
            }
            if (loop)
            {
                // Trộn chéo 50 ms đầu/cuối để chỗ nối không lộ (phần nhiễu không tự lặp khít).
                int x = (int)(0.05f * Rate);
                for (int i = 0; i < x; i++)
                {
                    float w = i / (float)x;
                    data[i] = data[i] * w + data[count - x + i] * (1f - w);
                }
                System.Array.Resize(ref data, count - x);
                count -= x;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, Rate, false);
            clip.SetData(data, 0);
            clip.hideFlags = HideFlags.DontSave;
            return clip;
        }
    }
}

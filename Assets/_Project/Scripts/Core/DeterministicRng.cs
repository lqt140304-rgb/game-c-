using System;

namespace ChoDoLa.Core
{
    /// <summary>
    /// Bộ sinh số ngẫu nhiên xác định theo khóa (seed, ngày, lượt, loại hộp...).
    /// Tự cài SplitMix64 để kết quả giống nhau trên mọi nền tảng (System.Random khác nhau giữa runtime).
    /// Mỗi khóa cho ra một dòng số riêng, không dùng chung bộ sinh toàn cục,
    /// nên tải lại ván không đổi kết quả (GDD mục 6).
    /// </summary>
    public sealed class DeterministicRng
    {
        private ulong _state;

        public DeterministicRng(long seed, params int[] keys)
        {
            ulong h = Mix((ulong)seed + 0x9E3779B97F4A7C15UL);
            foreach (int k in keys)
                h = Mix(h ^ ((ulong)(uint)k + 0x9E3779B97F4A7C15UL));
            _state = h;
        }

        private static ulong Mix(ulong z)
        {
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        private ulong NextU64()
        {
            _state += 0x9E3779B97F4A7C15UL;
            return Mix(_state);
        }

        /// <summary>Số thực đều trong [0, 1).</summary>
        public double NextDouble() => (NextU64() >> 11) * (1.0 / 9007199254740992.0);

        /// <summary>Số nguyên đều trong [0, max).</summary>
        public int NextInt(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            return (int)(NextDouble() * max);
        }

        /// <summary>Chọn chỉ số theo trọng số (tổng trọng số bất kỳ, không cần bằng 100).</summary>
        public int PickWeighted(double[] weights)
        {
            double total = 0;
            foreach (double w in weights) total += w;
            double x = NextDouble() * total, acc = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                acc += weights[i];
                if (x < acc) return i;
            }
            return weights.Length - 1;
        }

        /// <summary>Xáo trộn Fisher-Yates tại chỗ.</summary>
        public void Shuffle<T>(T[] arr)
        {
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = NextInt(i + 1);
                T t = arr[i]; arr[i] = arr[j]; arr[j] = t;
            }
        }
    }
}

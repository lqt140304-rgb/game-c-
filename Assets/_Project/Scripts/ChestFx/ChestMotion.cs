using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chuyển động CẢ RƯƠNG (không chỉ nắp): rung, nhảy, quay, lơ lửng, lắc, ép dẹt, bay đi; cùng ẩn/hiện và biến hình.
    /// Nhiều chuyển động chạy cùng lúc thì cộng dồn (nhảy + quay + rung...).
    ///
    /// CÁCH HOẠT ĐỘNG (quan trọng khi đọc lỗi):
    /// <see cref="BoxOpenSequence"/> ghi đè localPosition của rương MỖI KHUNG HÌNH khi đang chạy (để rung nhẹ), nên không thể
    /// "cộng thêm" một lần rồi quên. Lớp này chạy ở LateUpdate (sau khi BoxOpenSequence đã ghi) và ghi
    /// vị trí/góc/tỉ lệ = (giá trị gốc) + (độ lệch chuyển động). Giá trị gốc được nhận ra bằng cách so sánh: nếu transform vẫn đúng bằng
    /// giá trị lớp này ghi ở khung trước thì không ai đụng vào, gốc giữ nguyên; nếu khác thì ai đó (BoxOpenSequence, tay cầm của
    /// người chơi...) vừa ghi giá trị mới, lấy giá trị đó làm gốc. Nhờ vậy chạy đúng cả khi rương đang được cầm trên tay.
    /// Giới hạn: nếu một script khác chỉ ghi lại đúng giá trị cũ, hoặc đổi giá trị rất nhỏ (dưới ngưỡng so sánh của Unity),
    /// lớp này sẽ không nhận ra. <see cref="ResetAll"/> trả mọi thứ về gốc.
    ///
    /// Thêm vào rương bằng <see cref="GetOrAdd"/> (ChestRevealSequence tự làm khi có kịch bản). Chưa chạy trong Unity.
    /// </summary>
    public class ChestMotion : MonoBehaviour
    {
        /// <summary>Một chuyển động đang chạy. Giữ tham chiếu để chờ <see cref="Done"/>.</summary>
        public class Handle
        {
            public StepMotionKind kind;
            public float amplitude, frequency, duration, delay;
            public Vector3 axis;
            public bool holdEnd;
            public int seed;
            internal float time;

            public bool Done { get; internal set; }
        }

        private struct Delta
        {
            public Vector3 pos;       // thế giới
            public Quaternion rot;
            public Vector3 scale;
        }

        private readonly List<Handle> _active = new List<Handle>();
        private readonly List<Handle> _held = new List<Handle>();   // đã xong nhưng giữ trạng thái cuối
        private readonly List<Renderer> _hidden = new List<Renderer>();
        private Renderer[] _visuals = new Renderer[0];
        private GameObject _replacement;

        private bool _hasWritten;
        private Vector3 _lastPos, _basePos, _lastScale, _baseScale;
        private Quaternion _lastRot, _baseRot;
        private bool _applied;

        /// <summary>Lấy ChestMotion trên <paramref name="rig"/> hoặc thêm mới.</summary>
        public static ChestMotion GetOrAdd(GameObject rig)
        {
            ChestMotion m = rig.GetComponent<ChestMotion>();
            return m != null ? m : rig.AddComponent<ChestMotion>();
        }

        public bool IsBusy => _active.Count > 0;

        /// <summary>Ghi nhớ các Renderer của rương để ẩn/hiện sau này (gọi trước khi dựng hiệu ứng, để không ẩn nhầm hiệu ứng).</summary>
        public void CaptureVisuals()
        {
            List<Renderer> list = new List<Renderer>();
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer) continue;
                list.Add(r);
            }
            _visuals = list.ToArray();
        }

        /// <summary>Bắt đầu một chuyển động. <paramref name="seed"/> chỉ dùng cho độ rung (trang trí).</summary>
        public Handle Run(StepMotionKind kind, float amplitude, float frequency, float duration, Vector3 axis, bool holdEnd, float delay = 0f, int seed = 1)
        {
            Handle h = new Handle
            {
                kind = kind,
                amplitude = amplitude,
                frequency = frequency,
                duration = Mathf.Max(0.02f, duration),
                axis = axis.sqrMagnitude < 1e-6f ? Vector3.up : axis.normalized,
                holdEnd = holdEnd,
                delay = Mathf.Max(0f, delay),
                seed = seed,
            };
            _active.Add(h);
            return h;
        }

        /// <summary>Ẩn hoặc hiện mọi Renderer của rương (không động tới hạt hay hiệu ứng).</summary>
        public void SetVisible(bool visible)
        {
            if (visible)
            {
                foreach (Renderer r in _hidden) if (r != null) r.enabled = true;
                _hidden.Clear();
                return;
            }
            foreach (Renderer r in _visuals)
            {
                if (r != null && r.enabled) { r.enabled = false; _hidden.Add(r); }
            }
        }

        /// <summary>
        /// Biến hình: ẩn rương, đặt <paramref name="prefab"/> đúng chỗ rương đứng (cùng gốc, cùng hướng). ResetAll gỡ nó và hiện rương lại.
        /// Cắm làm con của rương để theo rương khi rương di chuyển (kể cả các chuyển động ở đây).
        /// </summary>
        public GameObject ReplaceWith(GameObject prefab, float scale, Vector3 localOffset)
        {
            if (prefab == null) return null;
            if (_replacement != null) Destroy(_replacement);
            SetVisible(false);
            _replacement = Instantiate(prefab, transform);
            _replacement.transform.localPosition = localOffset;
            _replacement.transform.localRotation = Quaternion.identity;
            _replacement.transform.localScale = Vector3.one * Mathf.Max(0.001f, scale);
            return _replacement;
        }

        /// <summary>Dừng mọi chuyển động, trả rương về đúng chỗ gốc, hiện lại, gỡ đồ biến hình.</summary>
        public void ResetAll()
        {
            _active.Clear();
            _held.Clear();
            if (_applied)
            {
                transform.localPosition = _basePos;
                transform.localRotation = _baseRot;
                transform.localScale = _baseScale;
            }
            _applied = false;
            _hasWritten = false;
            SetVisible(true);
            if (_replacement != null) { Destroy(_replacement); _replacement = null; }
        }

        private void LateUpdate()
        {
            if (_active.Count == 0 && _held.Count == 0) { ReleaseBase(); return; }

            float dt = Time.deltaTime;
            Delta total = new Delta { pos = Vector3.zero, rot = Quaternion.identity, scale = Vector3.one };

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Handle h = _active[i];
                h.time += dt;
                float local = h.time - h.delay;
                if (local < 0f) continue;
                float u = Mathf.Clamp01(local / h.duration);
                Accumulate(ref total, h, u, local);
                if (local >= h.duration)
                {
                    h.Done = true;
                    _active.RemoveAt(i);
                    if (h.holdEnd) _held.Add(h);
                }
            }
            foreach (Handle h in _held) Accumulate(ref total, h, 1f, h.duration);

            Write(total);
            if (_active.Count == 0 && _held.Count == 0) ReleaseBase();
        }

        /// <summary>Hết chuyển động (không giữ gì): trả về gốc rồi thôi can thiệp.</summary>
        private void ReleaseBase()
        {
            if (!_applied) return;
            // Nếu không ai đụng vào kể từ lần ghi cuối, trả về gốc. Nếu có ai ghi mới thì giữ giá trị của họ.
            if (transform.localPosition == _lastPos) transform.localPosition = _basePos;
            if (transform.localRotation == _lastRot) transform.localRotation = _baseRot;
            if (transform.localScale == _lastScale) transform.localScale = _baseScale;
            _applied = false;
            _hasWritten = false;
        }

        private void Write(Delta d)
        {
            Transform t = transform;
            if (_hasWritten)
            {
                // Giữ gốc cũ nếu khung trước ta là người ghi cuối; ngược lại lấy giá trị mới ai đó vừa ghi làm gốc.
                if (t.localPosition != _lastPos) _basePos = t.localPosition;
                if (t.localRotation != _lastRot) _baseRot = t.localRotation;
                if (t.localScale != _lastScale) _baseScale = t.localScale;
            }
            else
            {
                _basePos = t.localPosition;
                _baseRot = t.localRotation;
                _baseScale = t.localScale;
            }

            Vector3 local = t.parent != null ? t.parent.InverseTransformVector(d.pos) : d.pos;
            t.localPosition = _basePos + local;
            t.localRotation = _baseRot * d.rot;
            t.localScale = Vector3.Scale(_baseScale, d.scale);

            _lastPos = t.localPosition;
            _lastRot = t.localRotation;
            _lastScale = t.localScale;
            _hasWritten = true;
            _applied = true;
        }

        // ------------------------------------------------------------------ Công thức từng loại chuyển động

        private static void Accumulate(ref Delta total, Handle h, float u, float t)
        {
            switch (h.kind)
            {
                case StepMotionKind.Shake:
                {
                    // Tắt dần. Nhiễu Perlin theo thời gian (mượt), hạt = lệch pha để mỗi lần rung khác nhau nhưng lặp lại được.
                    float env = 1f - u;
                    float s = h.seed * 17.31f;
                    float f = Mathf.Max(1f, h.frequency);
                    total.pos += new Vector3(
                        (Mathf.PerlinNoise(s + t * f, 0.3f) - 0.5f) * 2f,
                        (Mathf.PerlinNoise(0.7f, s + t * f) - 0.5f) * 0.6f,
                        (Mathf.PerlinNoise(s + 5.1f, t * f) - 0.5f) * 2f) * (h.amplitude * env);
                    break;
                }
                case StepMotionKind.Jump:
                {
                    // Số lần nảy = frequency; mỗi lần thấp hơn lần trước 40%.
                    int hops = Mathf.Max(1, Mathf.RoundToInt(h.frequency));
                    float x = u * hops;
                    int idx = Mathf.Min(hops - 1, (int)x);
                    float f = x - idx;
                    float height = h.amplitude * Mathf.Pow(0.6f, idx);
                    total.pos += Vector3.up * (4f * f * (1f - f) * height);
                    break;
                }
                case StepMotionKind.Spin:
                {
                    float e = u * u * (3f - 2f * u);   // vào chậm ra chậm
                    total.rot = total.rot * Quaternion.AngleAxis(h.amplitude * e, h.axis);
                    break;
                }
                case StepMotionKind.Hover:
                {
                    // Lên trong 30% đầu, lơ lửng và nhấp nhô, hạ xuống trong 30% cuối (nếu không giữ).
                    float rise = Mathf.Clamp01(u / 0.3f);
                    rise = 1f - (1f - rise) * (1f - rise);
                    float fall = h.holdEnd ? 0f : Mathf.Clamp01((u - 0.7f) / 0.3f);
                    float y = h.amplitude * rise * (1f - fall * fall) + 0.01f * Mathf.Sin(t * 3.1f) * rise * (1f - fall);
                    total.pos += Vector3.up * y;
                    break;
                }
                case StepMotionKind.Wobble:
                {
                    float env = h.holdEnd ? 1f : 1f - u;
                    float angle = h.amplitude * Mathf.Sin(u * h.frequency * Mathf.PI * 2f) * env;
                    total.rot = total.rot * Quaternion.AngleAxis(angle, h.axis);
                    break;
                }
                case StepMotionKind.Squash:
                {
                    // Ép dẹt rồi bật lại: y co, x/z phồng, tắt dần.
                    float env = h.holdEnd ? 1f : 1f - u;
                    float k = h.amplitude * Mathf.Sin(u * h.frequency * Mathf.PI * 2f) * env;
                    total.scale = Vector3.Scale(total.scale, new Vector3(1f + k * 0.5f, 1f - k, 1f + k * 0.5f));
                    break;
                }
                default: // Fly
                {
                    float e = u * u * (3f - 2f * u);
                    total.pos += h.axis * (h.amplitude * e);
                    break;
                }
            }
        }
    }
}

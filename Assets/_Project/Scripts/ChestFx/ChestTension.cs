using System;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Cách âm "căng" kết thúc, chọn theo bậc đã quay. Chỉ quyết định ở khoảnh khắc cuối (<see cref="ChestTension.Resolve"/>):
    /// suốt đoạn căng trước đó âm giống hệt nhau ở mọi bậc, để người chơi không đoán trước được kết quả.
    /// </summary>
    public enum TensionResolution
    {
        Deflate,   // Rác: căng rồi CẮT ĐỘT NGỘT, im một nhịp, rồi xịt (anti-climax, hài)
        Soften,    // Thường: xẹp từ từ, một chút "ừ thì cũng được"
        Release,   // Hiếm: đỉnh căng được giải toả gọn
        Surge,     // Rất hiếm: dâng cao hơn, bùng lên
        Blast,     // Huyền thoại: bùng nổ, cao hơn mọi thứ trước đó, có tiếng ầm
    }

    /// <summary>
    /// Thiết lập âm căng thẳng của một rương (nằm trong <see cref="ChestFxProfile.tension"/>).
    /// Để trống clip = dùng âm tổng hợp bằng mã (<see cref="ChestTensionSynth"/>), nên rương nào cũng có tiếng ngay từ khi bấm mở.
    /// </summary>
    [Serializable]
    public class ChestTensionSettings
    {
        [Tooltip("Tắt = không có âm căng thẳng (rương chạy như bản cũ, chỉ có tiếng bắt đầu từ lúc hé nắp).")]
        public bool enabled = true;

        [Header("Âm (để trống = âm tổng hợp bằng mã)")]
        [Tooltip("Nền căng, phát lặp NGAY khi bấm mở. Cao độ và âm lượng tăng theo độ căng. Nên là âm liền mạch lặp được.")]
        public AudioClip loopClip;
        [Tooltip("Tiếng 'tim đập' ngắn, nhịp nhanh dần theo độ căng.")]
        public AudioClip pulseClip;
        [Tooltip("Tiếng xịt khi bậc thấp (Rác) sau khi âm bị cắt.")]
        public AudioClip deflateClip;
        [Tooltip("Tiếng dâng/bùng cho bậc từ Thường trở lên. Bậc cao phát to và bổ sung tiếng ầm tổng hợp.")]
        public AudioClip swellClip;

        [Header("Cường độ")]
        [Range(0f, 1f)] public float volume = 0.55f;
        [Tooltip("Cao độ của nền ở độ căng 0.")] public float pitchStart = 0.8f;
        [Tooltip("Cao độ của nền ở độ căng 1.")] public float pitchEnd = 2.1f;
        [Tooltip("Khoảng cách giữa hai nhịp tim ở độ căng 0 (giây).")] public float pulseIntervalStart = 1.0f;
        [Tooltip("Khoảng cách giữa hai nhịp tim ở độ căng 1 (giây).")] public float pulseIntervalEnd = 0.11f;
        [Tooltip("Độ căng trong lúc chờ (ví dụ chờ người chơi thả vật mở khoá) trước khi rương bắt đầu mở.")]
        [Range(0f, 1f)] public float waitingLevel = 0.18f;
        [Tooltip("Độ căng tối đa trước khoảnh khắc chốt. Nếu rương mở chậm hơn dự tính, âm giữ ở mức này (căng mà không đoán được).")]
        [Range(0.5f, 1f)] public float holdLevel = 0.96f;
        [Tooltip("0..1 tiến độ -> 0..1 độ căng. Mặc định tăng chậm rồi nhanh dần (ease-in).")]
        public AnimationCurve rampCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0.4f), new Keyframe(1f, 1f, 2.2f, 0f));

        [Header("Kết thúc theo bậc (chỉ áp dụng ở khoảnh khắc chốt)")]
        [Tooltip("Im lặng bao lâu sau khi cắt (Deflate) trước tiếng xịt, giây.")]
        public float silenceBeforeDeflate = 0.22f;
        [Tooltip("Cách kết thúc theo hạng bậc: 0 Rác, 1 Thường, 2 Hiếm, 3 Rất hiếm, 4 Huyền thoại.")]
        public TensionResolution[] resolutionByTier =
        {
            TensionResolution.Deflate, TensionResolution.Soften, TensionResolution.Release, TensionResolution.Surge, TensionResolution.Blast,
        };

        public TensionResolution ResolutionFor(ChestTier tier)
        {
            int i = (int)tier;
            if (resolutionByTier != null && i >= 0 && i < resolutionByTier.Length) return resolutionByTier[i];
            return (TensionResolution)Mathf.Clamp(i, 0, 4);
        }
    }

    /// <summary>
    /// Âm căng thẳng của một lần mở rương. Bắt đầu ở <see cref="Begin"/> (khoảnh khắc bấm mở), tăng dần độ căng
    /// (cao độ nền, âm lượng, nhịp tim) và rẽ hướng theo bậc ở <see cref="Resolve"/>.
    ///
    /// GIẤU KẾT QUẢ: lớp này KHÔNG nhận bậc cho tới <see cref="Resolve"/>, nên mọi thứ trước đó (nền, nhịp, đường cong)
    /// không thể phụ thuộc vào bậc. Dù game đã biết bậc từ lúc bấm, người chơi chỉ nghe thấy bậc ở khoảnh khắc chốt.
    /// Lớp này không đổi và không quyết định bậc; chỉ chọn âm.
    /// Không phải MonoBehaviour: <see cref="ChestRevealSequence"/> gọi <see cref="Tick"/> mỗi khung hình.
    /// </summary>
    public class ChestTension
    {
        private readonly ChestTensionSettings _s;
        private readonly GameObject _go;
        private readonly AudioSource _loop;
        private readonly AudioSource _pulse;
        private readonly AudioSource _fx;
        private readonly Action<float> _onPulse;

        private float _level;
        private float _rampFrom;
        private float _rampSeconds;
        private float _rampElapsed;
        private bool _ramping;
        private bool _resolved;
        private float _pulseTimer;
        private float _afterResolve;
        private TensionResolution _resolution;
        private float _decayLength;
        private float _peakPitch;
        private float _deflateAt = -1f;
        private bool _disposed;

        /// <summary>Độ căng hiện tại 0..1.</summary>
        public float Level => _level;
        public bool IsResolved => _resolved;
        public bool IsRamping => _ramping;

        /// <summary>
        /// Bắt đầu ngay: tạo nguồn âm và phát nền lặp ở độ căng chờ. <paramref name="spatialBlend"/> lấy theo AudioSource của rương.
        /// </summary>
        public ChestTension(ChestTensionSettings settings, Transform parent, float spatialBlend, UnityEngine.Audio.AudioMixerGroup mixer, Action<float> onPulse)
        {
            _s = settings;
            _onPulse = onPulse;
            _go = new GameObject("TensionAudio");
            _go.transform.SetParent(parent, false);
            _loop = NewSource("Loop", spatialBlend, mixer);
            _pulse = NewSource("Pulse", spatialBlend, mixer);
            _fx = NewSource("Fx", spatialBlend, mixer);

            _loop.clip = _s.loopClip != null ? _s.loopClip : ChestTensionSynth.Loop();
            _loop.loop = true;
            _level = Mathf.Clamp01(_s.waitingLevel);
            ApplyLoop(_level);
            _loop.Play();
        }

        private AudioSource NewSource(string name, float spatialBlend, UnityEngine.Audio.AudioMixerGroup mixer)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(_go.transform, false);
            AudioSource a = child.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.spatialBlend = spatialBlend;
            a.outputAudioMixerGroup = mixer;
            return a;
        }

        /// <summary>
        /// Bắt đầu tăng độ căng từ mức hiện tại lên <see cref="ChestTensionSettings.holdLevel"/> trong <paramref name="seconds"/> giây
        /// (dự tính thời gian từ lúc rương bắt đầu mở tới khoảnh khắc chốt). Nếu rương mở chậm hơn, độ căng giữ ở holdLevel.
        /// </summary>
        public void StartRamp(float seconds)
        {
            if (_resolved || _disposed) return;
            _rampFrom = _level;
            _rampSeconds = Mathf.Max(0.2f, seconds);
            _rampElapsed = 0f;
            _ramping = true;
        }

        /// <summary>Gọi mỗi khung hình (dt = Time.deltaTime).</summary>
        public void Tick(float dt)
        {
            if (_disposed) return;
            if (_resolved) { TickResolved(dt); return; }

            if (_ramping)
            {
                _rampElapsed += dt;
                float u = Mathf.Clamp01(_rampElapsed / _rampSeconds);
                float curve = _s.rampCurve != null && _s.rampCurve.length > 0 ? Mathf.Clamp01(_s.rampCurve.Evaluate(u)) : u;
                float target = Mathf.Lerp(_rampFrom, _s.holdLevel, curve);
                // Quá thời gian dự tính: bò nhẹ lên trần để vẫn thấy "căng thêm", nhưng không vượt holdLevel.
                if (u >= 1f) target = Mathf.Min(_s.holdLevel, target + 0.01f * (_rampElapsed - _rampSeconds));
                _level = target;
            }

            ApplyLoop(_level);
            TickPulse(dt);
        }

        private void ApplyLoop(float level)
        {
            // Rung nhẹ để nền không "chết": tiếng thở của căng thẳng. Hàm của thời gian, không ngẫu nhiên.
            float shiver = 0.015f * Mathf.Sin(Time.time * 9f) * level;
            _loop.pitch = Mathf.Lerp(_s.pitchStart, _s.pitchEnd, level) * (1f + shiver);
            _loop.volume = _s.volume * Mathf.Lerp(0.25f, 1f, Mathf.Pow(level, 1.2f));
        }

        private void TickPulse(float dt)
        {
            float interval = Mathf.Lerp(_s.pulseIntervalStart, _s.pulseIntervalEnd, _level);
            _pulseTimer += dt;
            if (_pulseTimer < interval) return;
            _pulseTimer = 0f;
            AudioClip clip = _s.pulseClip != null ? _s.pulseClip : ChestTensionSynth.Pulse();
            _pulse.pitch = Mathf.Lerp(0.9f, 1.6f, _level);
            _pulse.PlayOneShot(clip, _s.volume * Mathf.Lerp(0.45f, 1f, _level));
            _onPulse?.Invoke(_level);
        }

        /// <summary>
        /// Khoảnh khắc chốt: BÂY GIỜ mới dùng bậc để rẽ hướng. Trả về cách kết thúc đã chọn (để hook/UI dùng).
        /// </summary>
        public TensionResolution Resolve(ChestTier rolledTier)
        {
            if (_resolved || _disposed) return _resolution;
            _resolved = true;
            _afterResolve = 0f;
            _resolution = _s.ResolutionFor(rolledTier);

            switch (_resolution)
            {
                case TensionResolution.Deflate:
                    // Cắt đột ngột: nền và nhịp tim tắt hẳn ngay lập tức, rồi im lặng, rồi xịt.
                    _loop.Stop();
                    _pulse.Stop();
                    _deflateAt = _s.silenceBeforeDeflate;
                    _decayLength = 0f;
                    break;
                case TensionResolution.Soften:
                    _decayLength = 0.35f;
                    _peakPitch = _loop.pitch * 0.8f;
                    PlayFx(Swell(0.35f), 0.5f, 1f);
                    break;
                case TensionResolution.Release:
                    _decayLength = 0.8f;
                    _peakPitch = _loop.pitch * 1.25f;
                    PlayFx(Swell(0.65f), 0.85f, 1f);
                    break;
                case TensionResolution.Surge:
                    _decayLength = 1.2f;
                    _peakPitch = _loop.pitch * 1.5f;
                    PlayFx(Swell(0.85f), 1f, 1f);
                    break;
                default: // Blast
                    _decayLength = 1.8f;
                    _peakPitch = _loop.pitch * 1.8f;
                    PlayFx(Swell(1f), 1f, 1f);
                    PlayFx(ChestTensionSynth.Boom(), 1f, 1f);
                    break;
            }
            return _resolution;
        }

        private AudioClip Swell(float intensity) => _s.swellClip != null ? _s.swellClip : ChestTensionSynth.Swell(intensity);

        private void PlayFx(AudioClip clip, float volume, float pitch)
        {
            if (clip == null) return;
            _fx.pitch = pitch;
            _fx.PlayOneShot(clip, _s.volume * Mathf.Clamp01(volume) * 1.4f);
        }

        private void TickResolved(float dt)
        {
            _afterResolve += dt;
            if (_resolution == TensionResolution.Deflate)
            {
                if (_deflateAt >= 0f && _afterResolve >= _deflateAt)
                {
                    _deflateAt = -1f;
                    _fx.pitch = 1f;
                    _fx.PlayOneShot(_s.deflateClip != null ? _s.deflateClip : ChestTensionSynth.Fizzle(), _s.volume * 1.2f);
                }
                return;
            }

            if (_decayLength <= 0f || !_loop.isPlaying) return;
            float u = Mathf.Clamp01(_afterResolve / _decayLength);
            if (_resolution == TensionResolution.Soften)
            {
                // Xẹp: nền tụt cao độ và nhỏ dần.
                _loop.pitch = Mathf.Lerp(_loop.pitch, _peakPitch, Mathf.Clamp01(dt * 6f));
                _loop.volume = _s.volume * (1f - u);
            }
            else
            {
                // Dâng: nền vọt lên cao độ, to hơn trước khi tan, tan chậm hơn ở bậc cao.
                float rise = Mathf.Clamp01(_afterResolve / 0.25f);
                _loop.pitch = Mathf.Lerp(_loop.pitch, _peakPitch, Mathf.Clamp01(dt * 8f)) * (1f + 0.01f * rise);
                float peak = _resolution == TensionResolution.Release ? 1.1f : 1.4f;
                _loop.volume = _s.volume * Mathf.Lerp(peak, 0f, u * u);
            }
            if (u >= 1f) _loop.Stop();
        }

        /// <summary>Dừng mọi âm và huỷ nguồn âm (gọi khi reset hoặc huỷ).</summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_go != null) UnityEngine.Object.Destroy(_go);
        }
    }
}

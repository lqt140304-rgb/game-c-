using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Bảng số của lớp âm (đọc cùng doc docs/superpowers/specs/2026-11-27-ruong-am-thanh-dong-bo-design.md, mục 4 và 5).
    /// Mọi âm lượng ở đây là giá trị TRƯỚC khi nhân hệ số tổng <see cref="ChestSoundHook.volume"/>; hook nhân đúng MỘT lần khi đặt vào AudioSource.
    /// </summary>
    public static class ChestSoundPlan
    {
        /// <summary>Nền lúc bấm mở (âm lượng, cao độ).</summary>
        public const float BedStartVolume = 0.25f, BedStartPitch = 0.90f;
        /// <summary>Nền khi XONG nấc 1, 2, 3: dâng đều theo từng nấc, nấc 3 chạm trần <see cref="ChestAudioRule.BedMax"/>.</summary>
        public static readonly float[] BedVolume = { 0.30f, 0.38f, 0.45f };
        public static readonly float[] BedPitch = { 1.00f, 1.15f, 1.30f };
        /// <summary>Tiếng nhấn của nấc 1, 2, 3 (trần <see cref="ChestAudioRule.EventMax"/>) và cao độ.</summary>
        public static readonly float[] AccentVolume = { 0.25f, 0.38f, 0.50f };
        public static readonly float[] AccentPitch = { 0.90f, 1.00f, 1.12f };
        /// <summary>Tiếng chốt theo bậc Rác..Huyền thoại.</summary>
        public static readonly float[] StingVolume = { 0.55f, 0.70f, 0.80f, 0.90f, 1.00f };
        /// <summary>Nền tắt dần (bậc khác Rác) khi chốt, giây.</summary>
        public const float BedFadeOut = 0.30f;
        /// <summary>Tiếng nhấn tắt dần khi chốt hoặc khi hết nấc, giây.</summary>
        public const float AccentFadeOut = 0.08f;
        /// <summary>Rác: nền tắt phụt, lặng bấy nhiêu giây (tính từ lúc chốt) rồi mới có tiếng buồn.</summary>
        public const float RacSilence = ChestAudioRule.RacSilence;
        /// <summary>Nếu sau khi chốt bấy nhiêu giây mà bước bóp méo chưa loan báo (kịch bản thiếu bước), tiếng chốt vẫn vang (lưới an toàn).</summary>
        public const float StingFallback = 0.25f;
    }

    /// <summary>
    /// ÂM THANH CỦA MỘT LẦN MỞ RƯƠNG, trên MỘT dòng thời gian với chuyển động. Thay cho mọi nguồn âm cũ (tick, ambient, accent, whoosh,
    /// tension, tiếng của vật mở khoá): hook này là nơi DUY NHẤT phát tiếng của chuỗi mở chuẩn và chỉ phát khi có SỰ KIỆN THẬT:
    ///  - bấm mở (OnPress): nền bắt đầu;
    ///  - nấc 1, 2, 3 bắt đầu (sự kiện "nac:n" do bước chuyển động loan báo): nền dâng cao độ và âm lượng suốt đúng thời lượng thật của nấc
    ///    (đọc từ ChestScriptContext.GetMotion), và một tiếng nhấn (nếu khe không rỗng) dài không quá nấc;
    ///  - chốt bậc (OnTierLocked): tiếng nhấn tắt, nền tắt (Rác: tắt phụt, lặng 0,2 s);
    ///  - bóp méo bắt đầu (sự kiện "bop"): tiếng chốt theo bậc (Rác sau khoảng lặng; Rất hiếm và Huyền thoại ở ĐỈNH lần nảy đầu).
    /// Tối đa 3 lớp cùng lúc (nền, 1 tiếng nhấn, 1 tiếng chốt), mỗi lớp là một AudioSource riêng, KHÔNG PlayOneShot. Không coroutine,
    /// không đồng hồ riêng: mọi mốc đo bằng Time.time (cùng đồng hồ với BoxOpenSequence và ChestMotion). Tạm dừng game (timeScale 0) thì
    /// các lớp cũng tạm dừng để không lệch. Khe âm rỗng thì im, không báo lỗi. Mọi thứ dọn ở OnReset/OnDestroy.
    /// Gắn vào customFxPrefab của hồ sơ; Editor (ChestOpenSetup) điền sẵn clip theo ChestPreset.
    /// </summary>
    public class ChestSoundHook : ChestFxHook
    {
        [Header("Rương này (khoá của bảng đăng ký ChestPresetRegistry, ví dụ \"Golden\")")]
        public string presetName = "";

        [Header("Khe âm (rỗng = im)")]
        [Tooltip("Nền lặp từ lúc bấm mở tới lúc chốt.")]
        public AudioClip bedClip;
        [Tooltip("Tiếng nhấn của nấc 1, 2, 3 (đúng 3 phần tử, có thể rỗng).")]
        public AudioClip[] accentClips = new AudioClip[3];
        [Tooltip("Tiếng chốt theo bậc: Rác, Thường, Hiếm, Rất hiếm, Huyền thoại.")]
        public AudioClip[] lockClips = new AudioClip[5];

        [Range(0f, 1f), Tooltip("Hệ số tổng, nhân đúng MỘT lần vào mọi âm lượng của bảng ChestSoundPlan.")]
        public float volume = 0.8f;

        [Tooltip("Ghi mốc thời gian từng sự kiện âm vào Console để kiểm độ khớp (giây tính từ lúc bấm mở).")]
        public bool logTimeline = false;

        private ChestPreset _preset;
        private ChestRevealSequence _reveal;
        private AudioSource _bed, _accent, _sting;
        private float _pressTime;
        private bool _paused;

        // nền: dâng tuyến tính mượt từ (v0,p0) tới (v1,p1) trong rampDur giây kể từ rampT0
        private float _bedV = ChestSoundPlan.BedStartVolume, _bedP = ChestSoundPlan.BedStartPitch;
        private float _bedV0, _bedP0, _bedV1, _bedP1, _rampT0, _rampDur;
        private bool _bedOn;
        private bool _bedFading;
        private float _fadeT0, _fadeV0;

        // tiếng nhấn
        private bool _accentOn;
        private float _accentEnd;       // hết nấc hoặc hết clip
        private float _accentBaseVol;
        private bool _accentFading;
        private float _accentFadeT0, _accentFadeV0;

        // chốt
        private bool _locked;
        private int _rank = -1;
        private float _lockTime = -1f;
        private float _bopTime = -1f;
        private bool _stingScheduled;
        private float _stingAt;
        private bool _stingDone;

        private void Awake() { Warm(); }

        private void Warm()
        {
            // Nạp trước dữ liệu âm để lần phát đầu không trễ (clip chưa nạp sẽ phát chậm: một nguồn "lạc" thường gặp).
            WarmClip(bedClip);
            if (accentClips != null) foreach (AudioClip c in accentClips) WarmClip(c);
            if (lockClips != null) foreach (AudioClip c in lockClips) WarmClip(c);
        }

        private static void WarmClip(AudioClip c)
        {
            if (c != null && c.loadState == AudioDataLoadState.Unloaded) c.LoadAudioData();
        }

        // ------------------------------------------------------------------ callback của lõi

        public override void OnPress(ChestRevealSequence reveal)
        {
            CleanUp();
            _reveal = reveal;
            _preset = ChestPresetRegistry.Get(presetName) ?? new ChestPreset();
            _pressTime = Time.time;

            AudioSource chestSrc = reveal.GetComponent<AudioSource>();
            _bed = NewVoice("SoundBed", chestSrc);
            _accent = NewVoice("SoundAccent", chestSrc);
            _sting = NewVoice("SoundLock", chestSrc);

            _bedV = ChestSoundPlan.BedStartVolume;
            _bedP = ChestSoundPlan.BedStartPitch;
            _bedV0 = _bedV1 = _bedV; _bedP0 = _bedP1 = _bedP; _rampDur = 0f;
            if (bedClip != null)
            {
                _bed.clip = bedClip;
                _bed.loop = true;
                ApplyBed();
                _bed.Play();
                _bedOn = true;
            }
            Log("bấm mở: nền vào (vol " + _bedV.ToString("0.00") + ", cao độ " + _bedP.ToString("0.00") + ")");
        }

        public override void OnScriptEvent(ChestRevealSequence reveal, string eventName)
        {
            if (_reveal == null || string.IsNullOrEmpty(eventName)) return;
            if (eventName.StartsWith("nac:"))
            {
                int n;
                if (int.TryParse(eventName.Substring(4), out n) && n >= 1 && n <= 3) StartNac(reveal, eventName, n);
            }
            else if (eventName == ChestOpenScript.BopEvent)
            {
                _bopTime = Time.time;
                Log("bóp méo bắt đầu");
                TrySchedule(false);
            }
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_reveal == null || _locked) return;
            _locked = true;
            _rank = Mathf.Clamp((int)tier, 0, 4);
            _lockTime = Time.time;
            StopAccent(true);
            if (_rank == 0)
            {
                StopBed();              // Rác: nền tắt phụt
            }
            else if (_bedOn && !_bedFading)
            {
                _bedFading = true;      // bậc khác: tắt dần 0,3 s
                _fadeT0 = Time.time;
                _fadeV0 = _bedV;
            }
            Log("chốt bậc " + tier + ": nền " + (_rank == 0 ? "tắt phụt" : "tắt dần"));
            TrySchedule(false);
        }

        public override void OnReset(ChestRevealSequence reveal) { CleanUp(); }

        private void OnDestroy() { CleanUp(); }

        private void OnDisable() { StopAll(); }

        // ------------------------------------------------------------------ từng nấc

        private void StartNac(ChestRevealSequence reveal, string eventName, int n)
        {
            int i = n - 1;
            float dur = ChestOpenScript.NacFallbackSeconds[i];
            ChestMotion.Handle h = reveal.Context != null ? reveal.Context.GetMotion(eventName) : null;
            if (h != null) dur = Mathf.Max(0.2f, h.duration);   // thời lượng THẬT của chuyển động (lõi nối tới giai đoạn kế)

            // nền: dâng từ giá trị hiện tại tới đích của nấc này, trải đều trong đúng thời lượng nấc
            _bedV0 = _bedV; _bedP0 = _bedP;
            _bedV1 = ChestSoundPlan.BedVolume[i];
            _bedP1 = ChestSoundPlan.BedPitch[i];
            _rampT0 = Time.time;
            _rampDur = dur;

            // tiếng nhấn: 1 lần lúc nấc bắt đầu, cao độ và âm lượng theo độ mạnh của nấc, không dài hơn nấc
            AudioClip clip = accentClips != null && i < accentClips.Length ? accentClips[i] : null;
            if (clip != null && _accent != null)
            {
                StopAccent(false);
                float pitch = ChestSoundPlan.AccentPitch[i];
                _accentBaseVol = Mathf.Min(ChestSoundPlan.AccentVolume[i] * _preset.accentGain, ChestAudioRule.EventMax);
                _accent.clip = clip;
                _accent.pitch = pitch;
                _accent.volume = Out(_accentBaseVol);
                _accent.loop = false;
                _accent.Play();
                _accentOn = true;
                _accentFading = false;
                _accentEnd = Time.time + Mathf.Min(dur, clip.length / Mathf.Max(0.1f, pitch));
            }
            Log("nấc " + n + " bắt đầu, dài " + dur.ToString("0.00") + " s, nền → vol " + _bedV1.ToString("0.00") + " cao độ " + _bedP1.ToString("0.00")
                + (clip != null ? ", nhấn " + clip.name : ", nhấn rỗng"));
        }

        // ------------------------------------------------------------------ tiếng chốt

        /// <summary>
        /// Lên lịch tiếng chốt khi đã biết bậc (OnTierLocked) và đã thấy bóp méo bắt đầu (sự kiện bop). Nếu bóp méo không loan báo
        /// (<paramref name="fallback"/> = true sau StingFallback giây) thì vẫn vang để không bao giờ mất tiếng chốt.
        /// </summary>
        private void TrySchedule(bool fallback)
        {
            if (_stingScheduled || !_locked) return;
            if (_bopTime < 0f && !fallback) return;
            float baseTime = _bopTime >= 0f ? _bopTime : Time.time;
            float at;
            if (_rank == 0) at = Mathf.Max(baseTime, _lockTime + ChestSoundPlan.RacSilence);   // nền tắt phụt, lặng 0,2 s rồi tiếng buồn
            else at = baseTime + ChestOpenScript.FirstApexSeconds(_rank, _preset);             // bậc cao: nảy tới đỉnh rồi vang
            _stingScheduled = true;
            _stingAt = at;
            Log("tiếng chốt hẹn lúc +" + (at - _pressTime).ToString("0.00") + " s" + (fallback ? " (lưới an toàn: thiếu bước bóp méo)" : ""));
        }

        private void PlaySting()
        {
            _stingDone = true;
            AudioClip clip = lockClips != null && _rank >= 0 && _rank < lockClips.Length ? lockClips[_rank] : null;
            if (clip == null || _sting == null) return;
            float v = Mathf.Min(ChestSoundPlan.StingVolume[_rank] * _preset.lockGain, ChestAudioRule.StingMax);
            _sting.clip = clip;
            _sting.pitch = 1f;
            _sting.loop = false;
            _sting.volume = Out(v);
            _sting.Play();
            Log("tiếng chốt vang: " + clip.name + " (" + clip.length.ToString("0.00") + " s, vol " + v.ToString("0.00") + ")");
        }

        // ------------------------------------------------------------------ mỗi khung

        private void Update()
        {
            if (_reveal == null) return;

            // Tạm dừng game thì tạm dừng cả âm (âm không được chạy trong lúc chuyển động đứng).
            bool paused = Time.timeScale <= 0.001f;
            if (paused != _paused)
            {
                _paused = paused;
                foreach (AudioSource s in new[] { _bed, _accent, _sting })
                {
                    if (s == null) continue;
                    if (paused) s.Pause(); else s.UnPause();
                }
            }
            if (paused) return;

            float now = Time.time;

            // nền
            if (_bedOn && _bed != null)
            {
                if (_bedFading)
                {
                    float u = Mathf.Clamp01((now - _fadeT0) / ChestSoundPlan.BedFadeOut);
                    _bedV = _fadeV0 * (1f - u);
                    ApplyBed();
                    if (u >= 1f) StopBed();
                }
                else if (_rampDur > 0f)
                {
                    float u = Mathf.Clamp01((now - _rampT0) / _rampDur);
                    u = u * u * (3f - 2f * u);
                    _bedV = Mathf.Lerp(_bedV0, _bedV1, u);
                    _bedP = Mathf.Lerp(_bedP0, _bedP1, u);
                    ApplyBed();
                }
            }

            // tiếng nhấn: tắt dần cuối nấc (hoặc cuối clip), rồi dừng
            if (_accentOn && _accent != null)
            {
                if (_accentFading)
                {
                    float u = Mathf.Clamp01((now - _accentFadeT0) / ChestSoundPlan.AccentFadeOut);
                    _accent.volume = _accentFadeV0 * (1f - u);
                    if (u >= 1f) StopAccent(false);
                }
                else if (!_accent.isPlaying) StopAccent(false);
                else if (now >= _accentEnd) StopAccent(true);
            }

            // tiếng chốt
            if (_locked && !_stingScheduled && now >= _lockTime + ChestSoundPlan.StingFallback) TrySchedule(true);
            if (_stingScheduled && !_stingDone && now >= _stingAt) PlaySting();
        }

        // ------------------------------------------------------------------ tiện ích

        /// <summary>Nhân hệ số tổng đúng MỘT lần. Mọi AudioSource.volume của hook đi qua đây.</summary>
        private float Out(float v) { return Mathf.Clamp01(v) * volume; }

        private void ApplyBed()
        {
            if (_bed == null) return;
            float gain = _preset != null ? _preset.bedGain : 1f;
            _bed.volume = Out(Mathf.Min(_bedV * gain, ChestAudioRule.BedMax));
            _bed.pitch = _bedP;
        }

        private AudioSource NewVoice(string voiceName, AudioSource chestSrc)
        {
            GameObject go = new GameObject(voiceName);
            go.transform.SetParent(_reveal.transform, false);
            AudioSource a = go.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.loop = false;
            a.spatialBlend = chestSrc != null ? chestSrc.spatialBlend : 0.5f;
            a.outputAudioMixerGroup = chestSrc != null ? chestSrc.outputAudioMixerGroup : null;
            return a;
        }

        private void StopBed()
        {
            _bedOn = false;
            _bedFading = false;
            if (_bed != null) _bed.Stop();
        }

        /// <summary>fade = true: tắt dần 0,08 s rồi dừng; false: dừng ngay.</summary>
        private void StopAccent(bool fade)
        {
            if (_accent == null) { _accentOn = false; return; }
            if (fade && _accentOn && _accent.isPlaying && !_accentFading)
            {
                _accentFading = true;
                _accentFadeT0 = Time.time;
                _accentFadeV0 = _accent.volume;
                return;
            }
            _accent.Stop();
            _accentOn = false;
            _accentFading = false;
        }

        private void StopAll()
        {
            if (_bed != null) _bed.Stop();
            if (_accent != null) _accent.Stop();
            if (_sting != null) _sting.Stop();
            _bedOn = false;
            _bedFading = false;
            _accentOn = false;
            _accentFading = false;
        }

        private void CleanUp()
        {
            StopAll();
            if (_bed != null) Destroy(_bed.gameObject);
            if (_accent != null) Destroy(_accent.gameObject);
            if (_sting != null) Destroy(_sting.gameObject);
            _bed = _accent = _sting = null;
            _reveal = null;
            _locked = false;
            _rank = -1;
            _lockTime = -1f;
            _bopTime = -1f;
            _stingScheduled = false;
            _stingDone = false;
            _paused = false;
            _bedV = ChestSoundPlan.BedStartVolume;
            _bedP = ChestSoundPlan.BedStartPitch;
        }

        private void Log(string what)
        {
            if (logTimeline) Debug.Log("[ChestSound] +" + (Time.time - _pressTime).ToString("0.00") + " s  " + what, this);
        }
    }
}

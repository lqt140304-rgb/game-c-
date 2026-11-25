using System.Collections;
using System.Collections.Generic;
using ChoDoLa.Interaction;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Kịch bản "ĐÊM HỘI LÀNG" của Chest_VietNam, đã thu gọn theo kế hoạch 2026-11-25 (mục 5.8). Chỉ trang trí: KHÔNG đọc/đổi bậc
    /// trước khi ChestRevealSequence báo OnTierLocked; vòng màu 5 bậc và bóng vật phẩm vẫn do lõi làm.
    ///
    /// Dòng thời gian (t = 0 là lúc bấm mở). S = cạnh lớn nhất của khung bao rương:
    /// - 0 s: rương bập bềnh nhẹ (dịch ≤ 0,02 S, nghiêng ≤ 5°, giống nhau mọi bậc). Đàn bầu ngân (lặp, dâng dần) + trống/mõ
    ///   (nhịp nhanh dần 0,75 s → 0,15 s). Hoa đăng bay vào miệng rương: <see cref="autoLanterns"/> đèn tự thả, xuất phát ở
    ///   vòng bán kính 1,4 S, cao ngang miệng, bay cong (Bezier 3 điểm) 1,2 s vào miệng, rồi thu nhỏ về 0 và ẩn. Đèn thứ i
    ///   vào lúc (0,15 + 0,15 i) × T. Mỗi đèn vào: rương chao nhẹ + tiếng plop (qua TryGate).
    /// - Tương tác: phím L (hoặc <see cref="OfferLantern"/>) = thả thêm 1 đèn, tối đa <see cref="maxPlayerLanterns"/>.
    /// - Khoá bậc: Rác = đàn bầu + trống dừng ngay, lặng 0,2 s, 1 tiếng xịt, rương hụt rồi lên, ếch nhảy lên nắp "ộp".
    ///   Thường = 1 tiếng chốt + 1 mõ lẻ, 1 đèn trồi khỏi miệng bay lên 1,5 S. Hiếm trở lên = tiếng chốt + trống đồng, 1/1/2/3
    ///   đèn trời trồi khỏi miệng bay lên 2,5 S, rương nhảy khỏi vị trí (Huyền thoại lơ lửng 1,6 s).
    /// </summary>
    public class VietNamHoiLangHook : ChestFxHook
    {
        [Header("Âm (VietNamHoiLangSetup tự gắn từ Audio/Chests/viet_nam/, theo Tools/Audio/elevenlabs_chest_manifest.json)")]
        [Tooltip("Nền: đàn bầu (lặp).")] public AudioClip droneClip;
        [Tooltip("Nhịp: trống.")] public AudioClip drumClip;
        [Tooltip("Nhịp: mõ chen nhịp lẻ; Thường: tiếng mõ lẻ lúc chốt (HĐ).")] public AudioClip moClip;
        [Tooltip("Sự kiện: đèn vào miệng rương.")] public AudioClip plopClip;
        [Tooltip("Tiếng xịt của Rác (TC Rác).")] public AudioClip deflateClip;
        [Tooltip("Đuôi của Rác: ếch kêu.")] public AudioClip frogClip;
        [Tooltip("HĐ bậc Hiếm trở lên: trống đồng.")] public AudioClip boomClip;
        [Tooltip("TC theo bậc [Rác, Thường, Hiếm, Rất hiếm, Huyền thoại] = sfx_vietnam_lock_*.")] public AudioClip[] stingClips = new AudioClip[5];
        [Range(0f, 1f)] public float masterVolume = 0.8f;

        [Header("Nhịp")]
        [Tooltip("Ước lượng giây từ lúc bấm tới lúc khoá khi không đọc được giai đoạn của BoxOpenSequence.")]
        [SerializeField] private float fallbackLockSeconds = 6.5f;
        [SerializeField] private float beatSlow = 0.75f;
        [SerializeField] private float beatFast = 0.15f;

        [Header("Hoa đăng và tương tác")]
        [SerializeField] private int autoLanterns = 5;
        [SerializeField] private int maxPlayerLanterns = 2;
        [SerializeField] private int maxLights = 1;
        [SerializeField] private float lanternRadiusS = 1.4f;
        [Tooltip("0 = mỗi lần một khác. Chỉ ảnh hưởng trang trí.")]
        [SerializeField] private int seed;

        private enum Phase { Idle, Build, Ending }

        private const float FlightSeconds = 1.2f;
        private const float MaxTiltDegrees = 8f;

        private ChestRevealSequence _reveal;
        private Transform _chest;
        private Phase _phase = Phase.Idle;
        private System.Random _rng;
        private float _t, _expectedLock, _nextBeat, _lastEventTime = -999f;
        private int _beat, _autoSpawned, _offers, _lights;
        private float _waterY, _chestRadius, _chestHeight, _size = 0.6f;
        private Vector3 _center;

        private readonly List<Object> _owned = new List<Object>();
        private readonly List<GameObject> _world = new List<GameObject>();
        private readonly List<VietNamHoiLangFloater> _floaters = new List<VietNamHoiLangFloater>();
        private readonly HashSet<VietNamHoiLangFloater> _byPlayer = new HashSet<VietNamHoiLangFloater>();
        private AudioSource _drone, _beatSrc, _sfx;
        private ParticleSystem _fireflies;
        private ChestFxProfile _fireflyStyle;

        // Tư thế cả rương: cộng thêm lên tư thế BoxOpenSequence ghi (xem LateUpdate).
        private Vector3 _baseLocalPos;
        private Quaternion _baseLocalRot;
        private Vector3 _appliedPos;
        private Quaternion _appliedRot = Quaternion.identity;
        private Vector3 _lastWrittenPos;
        private bool _poseActive;
        private float _lift, _liftTarget, _liftSpeed = 1f;
        private Vector2 _tiltKick, _tiltKickVel;
        private float _bobAmp, _tiltAmp = 1.5f;

        public void Own(Object o) { if (o != null) _owned.Add(o); }

        /// <summary>Miệng rương (điểm hoa đăng bay vào / đèn trời trồi ra).</summary>
        public Vector3 MouthPoint => _reveal != null ? _reveal.MouthPosition : transform.position;

        /// <summary>Âm lượng sau khi nhân <see cref="masterVolume"/> MỘT lần, không vượt trần <paramref name="cap"/> (A6).</summary>
        private float Vol(float want, float cap) => Mathf.Min(want, cap) * Mathf.Clamp01(masterVolume / 0.8f);

        // ------------------------------------------------------------------ Bắt đầu NGAY khi bấm mở

        private void Start()
        {
            _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null || !_reveal.IsPlaying) return; // prefab đặt tay trong cảnh, không chạy.
            _chest = _reveal.transform;
            _rng = new System.Random(seed != 0 ? seed : System.Environment.TickCount);
            MeasureChest();
            _expectedLock = EstimateLockSeconds();
            _bobAmp = 0.004f * _size;
            BuildStage();
            _phase = Phase.Build;
            _nextBeat = 0.15f;
        }

        private void MeasureChest()
        {
            Bounds b = new Bounds(_chest.position, Vector3.one * 0.6f);
            bool any = false;
            foreach (Renderer r in _chest.GetComponentsInChildren<Renderer>())
            {
                if (r == null || r.transform.IsChildOf(transform) || r is ParticleSystemRenderer) continue;
                if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            }
            _center = new Vector3(b.center.x, 0f, b.center.z);
            _chestHeight = Mathf.Max(0.2f, b.size.y);
            _chestRadius = Mathf.Max(0.2f, Mathf.Max(b.extents.x, b.extents.z));
            _size = Mathf.Max(0.2f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)));
            _waterY = b.min.y + _chestHeight * 0.12f; // chỉ còn là mốc độ cao (không còn mặt nước)
            _center.y = _waterY;
        }

        private float EstimateLockSeconds()
        {
            BoxOpenSequence box = _reveal.Box;
            if (box == null || box.Stages == null || box.Stages.Count < 4) return fallbackLockSeconds;
            float s = 0f;
            for (int i = 0; i < 4; i++) s += box.Stages[i].travel + (i < 3 ? box.Stages[i].hold : 0f);
            return Mathf.Max(2.5f, s + 1.2f); // lõi thường kéo dài "Hé nắp" để đủ vòng quay.
        }

        private void BuildStage()
        {
            _drone = MakeSource("HoiLangDrone", true);
            _beatSrc = MakeSource("HoiLangBeat", false);
            _sfx = MakeSource("HoiLangSfx", false);
            if (droneClip != null)
            {
                _drone.clip = droneClip;
                _drone.pitch = 0.85f;
                _drone.volume = Vol(0.15f, ChestAudioRule.BedMax);
                _drone.Play();
            }

            _fireflyStyle = ScriptableObject.CreateInstance<ChestFxProfile>();
            _fireflyStyle.hideFlags = HideFlags.DontSave;
            _fireflyStyle.particleLook = ParticleLook.SoftDot;
            _fireflyStyle.particleDirection = ParticleDirection.Swirl;
            _fireflyStyle.particleSpeed = 0.25f;
            _fireflyStyle.particleSize = 0.03f;
            _fireflyStyle.particleLifetime = 2.2f;
            _fireflyStyle.burstGravity = -0.03f;
            _fireflyStyle.secondaryColor = new Color(0.7f, 1f, 0.4f);
            _fireflyStyle.secondaryMix = 0.5f;
            _owned.Add(_fireflyStyle);
            _fireflies = ChestFxFactory.CreateParticles("HoiLangFireflies", null, _fireflyStyle, _chestRadius * 2f, true);
            ChestFxFactory.SetParticleColor(_fireflies, new Color(0.85f, 1f, 0.45f), _fireflyStyle, 1f);
            _world.Add(_fireflies.gameObject);

            _baseLocalPos = _chest.localPosition;
            _baseLocalRot = _chest.localRotation;
            _lastWrittenPos = _chest.localPosition;
            _poseActive = true;
        }

        private AudioSource MakeSource(string name, bool loop)
        {
            GameObject go = new GameObject(name);
            go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(transform, false);
            AudioSource s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = loop;
            s.spatialBlend = 0.3f;
            return s;
        }

        // ------------------------------------------------------------------ Dâng nhịp

        private void Update()
        {
            if (_phase == Phase.Idle) return;
            _t += Time.deltaTime;
            ReadInput();
            if (_phase != Phase.Build) return;

            float k = Tension();
            _drone.pitch = Mathf.Lerp(0.85f, 1.35f, k); // +59% tới lúc chốt
            _drone.volume = Vol(Mathf.Lerp(0.15f, 0.45f, k), ChestAudioRule.BedMax);
            _bobAmp = Mathf.Lerp(0.004f, 0.015f, k) * _size; // ≤ 0,02 S
            _tiltAmp = Mathf.Lerp(1.5f, 5f, k);               // ≤ 5°

            if (_t >= _nextBeat)
            {
                _beat++;
                _nextBeat = _t + Mathf.Lerp(beatSlow, beatFast, Mathf.Pow(k, 1.3f));
                bool drum = k < 0.4f || _beat % 2 == 0;
                AudioClip c = drum ? drumClip : moClip;
                if (c != null) _beatSrc.PlayOneShot(c, Vol(Mathf.Lerp(0.25f, 0.55f, k), ChestAudioRule.BeatMax));
                if (drum) _tiltKick += new Vector2((float)_rng.NextDouble() - 0.5f, (float)_rng.NextDouble() - 0.5f) * k * 2f;
            }

            // Đèn tự thả: đèn thứ i vào miệng lúc (0,15 + 0,15 i) T, bay mất FlightSeconds nên xuất phát sớm hơn (không sớm hơn 0,05 s).
            while (_autoSpawned < autoLanterns && _t >= Mathf.Max(0.05f, (0.15f + 0.15f * _autoSpawned) * _expectedLock - FlightSeconds))
            {
                _autoSpawned++;
                SpawnLantern(false);
            }

            if (_fireflies != null)
            {
                _fireflies.transform.position = _reveal.MouthPosition;
                if ((float)_rng.NextDouble() < k * 0.6f) _fireflies.Emit(1 + Mathf.RoundToInt(k * 2f)); // ≤ 3 hạt/khung
            }
        }

        /// <summary>0 → 1 theo thời gian tới lúc khoá dự kiến; mỗi đèn người chơi thả cộng thêm.</summary>
        private float Tension() => Mathf.Clamp01(_t / _expectedLock + _offers * 0.05f);

        private void ReadInput()
        {
            if (_phase != Phase.Build) return;
            bool pressed;
#if ENABLE_INPUT_SYSTEM
            pressed = Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame;
#else
            pressed = Input.GetKeyDown(KeyCode.L);
#endif
            if (pressed) OfferLantern();
        }

        /// <summary>
        /// Người chơi thả thêm một hoa đăng (bay vào miệng rương ngay, vào sau 1,2 s). Tối đa <see cref="maxPlayerLanterns"/> lần.
        /// Gọi được từ UI của game. Chỉ có tác dụng trước lúc khoá bậc.
        /// </summary>
        public void OfferLantern()
        {
            if (_phase != Phase.Build || _offers >= maxPlayerLanterns) return;
            _offers++;
            SpawnLantern(true);
        }

        private void SpawnLantern(bool byPlayer)
        {
            bool light = _lights < maxLights;
            if (light) _lights++;
            float size = Mathf.Clamp(_size * 0.33f, 0.1f, 0.3f);
            VietNamHoiLangFloater f = VietNamHoiLangProps.Lantern(MouthPoint, size, light, this, _rng);
            f.transform.rotation = Quaternion.Euler(0f, (float)_rng.NextDouble() * 360f, 0f);
            // Vòng bán kính 1,4 S quanh tâm rương, cao ngang miệng. Đèn tự thả rải đều quanh vòng, đèn người chơi ở phía trước.
            float a;
            if (byPlayer)
            {
                Vector3 fwd = _reveal.Box != null ? _chest.TransformDirection(_reveal.Box.FrontDirection) : _chest.forward;
                fwd.y = 0f;
                a = Mathf.Atan2(fwd.z, fwd.x) + ((float)_rng.NextDouble() - 0.5f) * 1.2f;
            }
            else a = (_autoSpawned * 2.399f + (float)_rng.NextDouble() * 0.5f); // góc vàng: rải đều
            Vector3 from = _center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (lanternRadiusS * _size);
            from.y = MouthPoint.y;
            f.FlyIn(from, 0.4f * _size, FlightSeconds);
            _floaters.Add(f);
            _world.Add(f.gameObject);
            if (byPlayer) _byPlayer.Add(f);
        }

        /// <summary>Đèn vừa tới miệng rương: rương chao nhẹ về hướng đèn tới, 1 tiếng plop (gác bằng TryGate).</summary>
        public void OnLanternArrived(VietNamHoiLangFloater f)
        {
            if (_phase != Phase.Build || f == null) return;
            Vector3 d = _chest.InverseTransformDirection(f.FlyFrom - _center);
            d.y = 0f;
            if (d.sqrMagnitude > 1e-6f) _tiltKick += new Vector2(-d.z, d.x).normalized * 1.2f;
            if (plopClip != null && ChestAudioRule.TryGate(ref _lastEventTime, ChestAudioRule.EventGap))
                _sfx.PlayOneShot(plopClip, Vol(_byPlayer.Contains(f) ? 0.45f : 0.35f, ChestAudioRule.EventMax));
        }

        // ------------------------------------------------------------------ Khoá bậc: rẽ nhánh

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_phase != Phase.Build) return;
            _phase = Phase.Ending;
            if (_beatSrc != null) _beatSrc.Stop(); // nhịp dừng ngay (không fade) ở mọi bậc
            if (_fireflies != null) _fireflies.Stop();
            if (tier <= ChestTier.Thuong) StartCoroutine(LowEnding(tier));
            else StartCoroutine(HighEnding(tier, color));
        }

        private AudioClip Sting(ChestTier tier)
        {
            int i = Mathf.Clamp(tier.Rank(), 0, 4);
            return stingClips != null && i < stingClips.Length ? stingClips[i] : null;
        }

        private IEnumerator LowEnding(ChestTier tier)
        {
            if (tier == ChestTier.Rac)
            {
                // Nền + nhịp dừng ngay → lặng 0,2 s → đúng 1 tiếng xịt; không phát gì khác trong 0,6 s kế tiếp.
                _drone.Stop();
                yield return new WaitForSeconds(ChestAudioRule.RacSilence);
                AudioClip xit = deflateClip != null ? deflateClip : Sting(tier);
                if (xit != null) _sfx.PlayOneShot(xit, Vol(0.9f, ChestAudioRule.StingMax));
                SetLift(-_chestHeight * 0.15f, 3f);
                yield return new WaitForSeconds(0.3f);
                SetLift(0f, 2.5f);
                yield return new WaitForSeconds(0.2f);
                yield return FrogRoutine(); // nhảy 0,6 s, "ộp" lúc đáp (> 0,8 s sau xịt)
                yield break;
            }

            // Thường: nền fade 0,3 s; TC 0,6 + HĐ mõ lẻ 0,4; 1 đèn trời bay lên 1,5 S.
            Sting1(Sting(tier), 0.6f);
            if (moClip != null) _reveal.PlayOneShot(moClip, Vol(0.4f, ChestAudioRule.ActionMax), 0.7f);
            StartCoroutine(SkyLanterns(1, 1.5f, 1.0f));
            float v0 = _drone.volume;
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                _drone.volume = Mathf.Lerp(v0, 0f, t / 0.3f);
                yield return null;
            }
            _drone.Stop();
            _bobAmp = 0.003f * _size;
            _tiltAmp = 1f;
            SetLift(-_chestHeight * 0.06f, 2f);
            yield return new WaitForSeconds(0.6f);
            SetLift(0f, 1f);
        }

        private void Sting1(AudioClip c, float vol)
        {
            if (c != null) _sfx.PlayOneShot(c, Vol(vol, ChestAudioRule.StingMax));
        }

        /// <summary>Ếch nhảy từ 1,2 S lên nắp theo đường ném (tính bằng mã, không Rigidbody/collider), "ộp" lúc đáp, rồi biến mất.</summary>
        private IEnumerator FrogRoutine()
        {
            float fs = Mathf.Clamp(_size / 0.6f, 0.5f, 2f);
            Vector3 from = _center + _chest.right * (1.2f * _size);
            from.y = _waterY;
            GameObject frog = VietNamHoiLangProps.Part(PrimitiveType.Sphere, null, Vector3.zero, new Vector3(0.1f, 0.07f, 0.12f) * fs, new Color(0.35f, 0.6f, 0.2f), this, "HoiLangEch");
            frog.transform.position = from;
            _world.Add(frog);
            Vector3 target = _reveal.MouthPosition + Vector3.up * 0.1f * _chestHeight;
            const float T = 0.6f;
            Vector3 v = (target - from) / T - 0.5f * Physics.gravity * T;
            for (float t = 0f; t < T; t += Time.deltaTime)
            {
                frog.transform.position = from + v * t + 0.5f * Physics.gravity * t * t;
                yield return null;
            }
            frog.transform.position = _reveal.MouthPosition + Vector3.up * 0.1f * _chestHeight;
            if (frogClip != null) _sfx.PlayOneShot(frogClip, Vol(0.4f, ChestAudioRule.TailMax));
            _tiltKick += new Vector2(2f, -1f);
            yield return new WaitForSeconds(1.0f);
            if (frog != null) { _world.Remove(frog); Destroy(frog); }
        }

        private IEnumerator HighEnding(ChestTier tier, Color tierColor)
        {
            int rank = tier.Rank();
            _drone.Stop();
            // TC + HĐ (đúng 2 tiếng trong 0,3 s đầu). Hiếm: 0,85 / 0,7; Rất hiếm, Huyền thoại: 1,0 / 0,8.
            Sting1(Sting(tier), rank <= 2 ? 0.85f : 1f);
            if (boomClip != null) _sfx.PlayOneShot(boomClip, Vol(rank <= 2 ? 0.7f : 0.8f, ChestAudioRule.ActionMax));

            // Hiếm 1 · Rất hiếm 2 · Huyền thoại 3 đèn trời trồi từ miệng rương, bay lên 2,5 S.
            StartCoroutine(SkyLanterns(Mathf.Max(1, rank - 1), 2.5f, 1.3f));
            if (_fireflies != null)
            {
                ChestFxFactory.SetParticleColor(_fireflies, Color.Lerp(new Color(0.85f, 1f, 0.45f), tierColor, 0.5f), _fireflyStyle, 1.4f);
                _fireflies.Emit(rank >= 4 ? 120 : 20 + 10 * rank); // ≤ 60 một lần, Huyền thoại ≤ 160
            }

            // Cả rương nhảy (≤ 0,65 H), Huyền thoại lơ lửng 1,6 s.
            SetLift(_chestHeight * (0.25f + 0.1f * rank), 6f);
            float hover = tier == ChestTier.HuyenThoai ? 1.6f : 0.9f;
            for (float t = 0f; t < hover; t += Time.deltaTime)
            {
                if (_fireflies != null)
                {
                    _fireflies.transform.position = _reveal.MouthPosition - Vector3.up * _chestHeight * 0.6f;
                    if (_rng.NextDouble() < 0.5) _fireflies.Emit(2);
                }
                yield return null;
            }
            SetLift(_chestHeight * 0.05f, 1.2f);
            yield return new WaitForSeconds(0.6f);
            SetLift(0f, 1f);
        }

        /// <summary>
        /// <paramref name="count"/> đèn trời trồi khỏi miệng rương (cách nhau 0,25 s), bay thẳng lên <paramref name="heightS"/> × S
        /// với tốc độ cố định <paramref name="speedS"/> × S/giây rồi mờ dần. Dùng lại đèn đã ẩn; thiếu thì dựng thêm (không Light).
        /// </summary>
        private IEnumerator SkyLanterns(int count, float heightS, float speedS)
        {
            for (int i = 0; i < count; i++)
            {
                VietNamHoiLangFloater f = null;
                foreach (VietNamHoiLangFloater c in _floaters) if (c != null && c.Hidden) { f = c; break; }
                if (f == null)
                {
                    f = VietNamHoiLangProps.Lantern(MouthPoint, Mathf.Clamp(_size * 0.33f, 0.1f, 0.3f), false, this, _rng);
                    _floaters.Add(f);
                    _world.Add(f.gameObject);
                }
                f.Emerge(heightS * _size, speedS * _size);
                yield return new WaitForSeconds(0.25f);
            }
        }

        // ------------------------------------------------------------------ Tư thế cả rương

        private void SetLift(float target, float speed)
        {
            _liftTarget = target;
            _liftSpeed = Mathf.Max(0.1f, speed);
        }

        private void LateUpdate()
        {
            if (!_poseActive || _chest == null) return;
            float dt = Time.deltaTime;

            // BoxOpenSequence ghi đè localPosition mỗi khung khi đang chạy giai đoạn; khi nó không ghi, gỡ phần ta cộng lần trước.
            if (_chest.localPosition == _lastWrittenPos) _chest.localPosition -= _appliedPos;
            _chest.localRotation = _chest.localRotation * Quaternion.Inverse(_appliedRot);

            _lift = Mathf.MoveTowards(_lift, _liftTarget, _liftSpeed * dt * Mathf.Max(0.05f, Mathf.Abs(_liftTarget - _lift) + 0.05f) * 3f);
            // Lò xo cho cú chao (đèn vào, trống, ếch đáp).
            _tiltKickVel += (-_tiltKick * 30f - _tiltKickVel * 5f) * dt;
            _tiltKick += _tiltKickVel * dt;
            if (_phase == Phase.Build)
            {
                _tiltKick.x = Mathf.Clamp(_tiltKick.x, -2.5f, 2.5f);
                _tiltKick.y = Mathf.Clamp(_tiltKick.y, -2.5f, 2.5f);
            }

            float bob = Mathf.Sin(_t * 2.1f) * _bobAmp + Mathf.Sin(_t * 3.3f + 1f) * _bobAmp * 0.4f;
            float rollX = (Mathf.PerlinNoise(_t * 0.7f, 0.1f) - 0.5f) * 2f * _tiltAmp + _tiltKick.x;
            float rollZ = (Mathf.PerlinNoise(0.3f, _t * 0.7f) - 0.5f) * 2f * _tiltAmp + _tiltKick.y;
            if (_phase == Phase.Build) // trước chốt: nghiêng ≤ 8°, giống nhau mọi bậc
            {
                rollX = Mathf.Clamp(rollX, -MaxTiltDegrees, MaxTiltDegrees);
                rollZ = Mathf.Clamp(rollZ, -MaxTiltDegrees, MaxTiltDegrees);
            }

            _appliedPos = new Vector3(0f, bob + _lift, 0f);
            _appliedRot = Quaternion.Euler(rollX, 0f, rollZ);
            _chest.localPosition += _appliedPos;
            _chest.localRotation = _chest.localRotation * _appliedRot;
            _lastWrittenPos = _chest.localPosition;
        }

        private void RestorePose()
        {
            if (!_poseActive || _chest == null) return;
            _poseActive = false;
            _chest.localRotation = _chest.localRotation * Quaternion.Inverse(_appliedRot);
            if (_chest.localPosition == _lastWrittenPos) _chest.localPosition -= _appliedPos;
            _appliedPos = Vector3.zero;
            _appliedRot = Quaternion.identity;
        }

        // ------------------------------------------------------------------ Dọn

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            StopAllCoroutines();
            _phase = Phase.Idle;
            RestorePose();
            if (_drone != null) _drone.Stop();
            if (_beatSrc != null) _beatSrc.Stop();
            if (_sfx != null) _sfx.Stop();
            foreach (GameObject g in _world) if (g != null) Destroy(g);
            _world.Clear();
            _floaters.Clear();
            _byPlayer.Clear();
            if (_fireflies != null)
            {
                Destroy(_fireflies.GetComponent<ParticleSystemRenderer>().sharedMaterial);
                _fireflies = null;
            }
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
        }
    }
}

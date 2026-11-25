using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_RacQuy — kịch bản "MÈO HOANG LẬT THÙNG RÁC". Spec: docs/superpowers/specs/2026-11-25-kichban-rac-quy-design.md
    ///
    /// - Ngay khi bấm mở (prefab hook được sinh trong ChestRevealSequence.Play → OnEnable/Update thấy IsPlaying):
    ///   vòng trống "thùng rỗng + rác lạch cạch" bắt đầu, pitch và âm lượng dâng dần; ruồi vo ve; một con mèo chợ
    ///   (dựng bằng khối cơ bản) đi vào, rình, ngoáy mông.
    /// - Người chơi có thể NÉM XƯƠNG CÁ (phím F / chuột phải): xương là Rigidbody thật bay theo cung từ camera, rơi cạnh
    ///   thùng; mèo chạy tới gặm rồi "hăng máu" → cú nhảy mạnh hơn, rác văng nhiều hơn. Chỉ trang trí.
    /// - Khi vòng quay màu bắt đầu: mèo phóng lên miệng thùng; mỗi bước màu thùng giật nghiêng, một món rác (Rigidbody)
    ///   bật ra. Kết quả GIỮ BÍ MẬT: tới lúc khoá màu mọi bậc trông như nhau.
    /// - Khoá màu: Rác = cơn căng "xì hơi" (pitch rơi tụt + tiếng xịt), mèo chán bỏ đi, chỉ rớt một chiếc giày;
    ///   Thường/Hiếm = thùng đổ nghiêng, rác văng, mèo xù lông; Rất hiếm/Huyền thoại = nổ "BÙM", thùng gần lật, rác bắn tung,
    ///   mèo bị hất văng (Rigidbody, xoay vòng) kêu thất thanh, cột sáng + một tia lóe giữa đống rác.
    /// Bất biến: không đọc/ghi gì ngoài tham số tier mà ChestRevealSequence đưa; ngẫu nhiên riêng (System.Random theo
    /// decorationSeed) chỉ để trang trí. Thùng chỉ bị XOAY (BoxOpenSequence giữ vị trí gốc để rung), trả lại khi Reset.
    /// </summary>
    public class RacQuyHook : ChestFxHook
    {
        [Header("Âm cũ (Tools/Cho Do La/Create Rac Quy Chest FX Profile)")]
        public AudioClip gurgleClip;
        public AudioClip fliesClip;
        public AudioClip canRollClip;
        public AudioClip fartClip;

        [Header("Âm mèo (Audio/Chests/rac_quy/meo, tạo bằng Tools/Audio/make_racquy_meo_sfx.py)")]
        public AudioClip buildLoopClip;
        public AudioClip meowClip;
        public AudioClip meowWantsClip;
        public AudioClip yowlClip;
        public AudioClip hissClip;
        public AudioClip clatterClip;
        public AudioClip clankClip;
        public AudioClip deflateClip;
        public AudioClip boomClip;
        public AudioClip throwClip;
        [Tooltip("TC theo bậc (Rác..Huyền thoại): sfx_racquy_lock_*.mp3. Hook tự phát (A4/A11). Bậc Rác phát deflateClip thay vì ô 0.")]
        public AudioClip[] stingClips = new AudioClip[5];
        [Range(0f, 1f)] public float fxVolume = 0.8f;

        // Nền dâng (A2): âm lượng 0,15 → 0,45 (nhân fxVolume một lần), cao độ +50% tới lúc chốt.
        private const float BedVolStart = 0.15f, BedVolMax = ChestAudioRule.BedMax;
        private const float BedPitchStart = 1f, BedPitchMax = 1.5f, BedRamp = 7f;
        private const int MaxPreLockTrash = 10;

        [Header("Tương tác")]
        [Tooltip("Cho phép ném xương cá dụ mèo (F hoặc chuột phải) trong lúc mở.")]
        [SerializeField] private bool allowBoneThrow = true;
        [Tooltip("0 = mỗi lần một khác. Chỉ ảnh hưởng trang trí.")]
        [SerializeField] private int decorationSeed;
        [SerializeField] private int maxTrash = 40;

        [Header("Màu")]
        [SerializeField] private Color catColor = new Color(0.85f, 0.5f, 0.18f);   // mèo mướp cam
        [SerializeField] private Color stinkColor = new Color(0.55f, 0.72f, 0.12f);
        [SerializeField] private Color flyColor = new Color(0.04f, 0.04f, 0.03f);
        [SerializeField] private Color holyColor = new Color(1f, 0.93f, 0.62f);

        // ---------------------------------------------------------------- trạng thái
        private ChestRevealSequence _reveal;
        private System.Random _rng;
        private bool _started, _rouletteOn, _locked;
        private float _startTime;
        private float _size = 0.4f;               // cỡ thùng (m), đo từ renderer
        private Vector3 _base;                    // tâm đáy thùng (world)
        private Quaternion _baseRot;
        private bool _hasBaseRot;
        private float _tilt, _tiltVel, _tiltTarget, _wobble;
        private Vector3 _tiltAxis = Vector3.right;
        private float _boost = 1f;                // >1 khi mèo đã gặm xương

        private AudioSource _build;
        private float _lastEvent = -999f;         // mốc ChestAudioRule.TryGate cho tiếng lẻ trước chốt
        private int _preLockTrash;                // rác đã rớt trước chốt (trần MaxPreLockTrash)

        private Transform _cat, _catTail, _catHead;
        private Rigidbody _catBody;
        private Transform _bone;
        private Coroutine _catRoutine;
        private GameObject _floor;
        private Transform _glint;
        private readonly List<GameObject> _trash = new List<GameObject>();
        private readonly List<Object> _owned = new List<Object>();
        private readonly Dictionary<Color, Material> _mats = new Dictionary<Color, Material>();

        private ChestFxProfile _stinkStyle, _flyStyle, _holyStyle;
        private ParticleSystem _stink, _flies, _holy;

        // ---------------------------------------------------------------- vào cuộc ngay khi bấm

        private void OnEnable()
        {
            if (_reveal == null && transform.parent != null) _reveal = transform.parent.GetComponent<ChestRevealSequence>();
            if (_reveal == null) _reveal = GetComponent<ChestRevealSequence>();
        }

        private void Update()
        {
            if (_reveal == null) return;
            if (!_started && _reveal.IsPlaying) BeginOnPress();
            if (!_started) return;

            UpdateBuildAudio();
            UpdateTilt();
            if (allowBoneThrow && !_locked && _bone == null && ThrowPressed()) ThrowBone();
        }

        private void BeginOnPress()
        {
            _started = true;
            _startTime = Time.time;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);
            Measure();
            BuildParticles();

            _build = gameObject.AddComponent<AudioSource>();
            _build.playOnAwake = false;
            _build.loop = true;
            _build.clip = buildLoopClip;
            _build.pitch = BedPitchStart;
            _build.volume = BedVolStart * fxVolume;
            _build.spatialBlend = 0f;
            if (buildLoopClip != null) _build.Play();

            PlayEvent(fliesClip, 0.3f);
            _flies.transform.position = _reveal.MouthPosition + Vector3.up * _size * 0.15f;
            _flies.Play();
            _flies.Emit(5);

            BuildCat();
            _catRoutine = StartCoroutine(CatIntro());
        }

        // ---------------------------------------------------------------- callback của lõi

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            _reveal = reveal;
            if (!_started) BeginOnPress();
            _rouletteOn = true;
            Place(mouth);
            ParticleSystem.EmissionModule em = _stink.emission;
            em.rateOverTime = 8f;
            ChestFxFactory.SetParticleColor(_stink, stinkColor, _stinkStyle, 0.7f);
            _stink.Play();
            PlayEvent(gurgleClip, 0.5f);
            if (_catRoutine != null) StopCoroutine(_catRoutine);
            _catRoutine = StartCoroutine(CatLeapOntoBin());
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (!_rouletteOn || _locked) return;
            // Mèo cào: thùng giật nghiêng theo một hướng ngẫu nhiên, mạnh dần theo thời gian.
            float t = Time.time - _startTime;
            Kick(Mathf.Lerp(4f, 11f, Mathf.Clamp01(t / 7f)) * _boost);
            // Trần rác trước chốt (T3): tối đa MaxPreLockTrash món; vẫn tiêu một số ngẫu nhiên để dãy hạt giống không đổi.
            if (_rng.NextDouble() < 0.45 && _preLockTrash < MaxPreLockTrash) { SpawnTrash(1, 0.9f, false); _preLockTrash++; }
            if (_rng.NextDouble() < 0.3) PlayEvent(clankClip, 0.35f, 0.8f + (float)_rng.NextDouble() * 0.6f);
            if (_stink != null) ChestFxFactory.SetParticleColor(_stink, Color.Lerp(stinkColor, color, 0.25f), _stinkStyle, 0.7f);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (!_started) BeginOnPress();
            _locked = true;
            StopBuild();            // A4: nền dừng ngay, không fade
            Place(reveal.MouthPosition);
            ParticleSystem.EmissionModule em = _stink.emission;
            em.rateOverTime = 0f;
            if (_catRoutine != null) StopCoroutine(_catRoutine);

            switch (tier)
            {
                case ChestTier.Rac:
                    // A4 Rác: nền dừng ngay, lặng 0,2 s, một tiếng xịt, không gì khác trong 0,6 s.
                    Kick(-5f);
                    PlayLater(deflateClip, 0.8f, ChestAudioRule.RacSilence);
                    SpawnTrash(1, 0.6f, false, shoe: true);
                    ChestFxFactory.SetParticleColor(_stink, stinkColor, _stinkStyle, 1f);
                    _stink.Emit(45);
                    _flies.Emit(8);
                    _catRoutine = StartCoroutine(CatBored());
                    break;
                case ChestTier.Thuong:
                    PlaySting(1, 0.7f);
                    reveal.PlayOneShot(clatterClip, fxVolume * 0.5f);      // HĐ
                    Tip(40f);
                    SpawnTrash(8, 1.4f, true);
                    _catRoutine = StartCoroutine(CatHopOffWithBone());
                    break;
                case ChestTier.Hiem:
                    PlaySting(2, 0.85f);
                    reveal.PlayOneShot(clatterClip, fxVolume * 0.7f);      // HĐ
                    PlayLater(hissClip, 0.4f, 0.5f);                       // đuôi
                    Tip(58f);
                    SpawnTrash(14, 1.9f, true);
                    _flies.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    _catRoutine = StartCoroutine(CatHiss());
                    StartCoroutine(Glint(color, 1f));
                    break;
                default: // Rất hiếm, Huyền thoại: dâng tới đỉnh rồi bùng nổ.
                    bool legend = tier == ChestTier.HuyenThoai;
                    PlaySting(legend ? 4 : 3, 1f);
                    reveal.PlayOneShot(boomClip, fxVolume * 0.8f);         // HĐ
                    PlayLater(clatterClip, 0.5f, 0.3f);
                    PlayLater(yowlClip, 0.4f, 0.7f);
                    Tip(legend ? 85f : 70f);
                    SpawnTrash(legend ? 32 : 22, legend ? 3.4f : 2.6f, true);
                    _flies.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    LaunchCat(legend ? 1.5f : 1.1f);
                    Holy(color, legend);
                    StartCoroutine(Glint(color, legend ? 1.8f : 1.3f));
                    break;
            }
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        // ---------------------------------------------------------------- âm

        private void UpdateBuildAudio()
        {
            if (_build == null || _locked || !_build.isPlaying) return;
            float u = Mathf.Clamp01((Time.time - _startTime) / BedRamp);
            _build.pitch = Mathf.Lerp(BedPitchStart, BedPitchMax, u);
            _build.volume = Mathf.Lerp(BedVolStart, BedVolMax, u) * fxVolume;
        }

        private void StopBuild()
        {
            if (_build != null) _build.Stop();
        }

        /// <summary>Tiếng lẻ trước chốt (A3): qua ChestAudioRule.TryGate, âm lượng ≤ EventMax, nhân fxVolume một lần.</summary>
        private void PlayEvent(AudioClip clip, float volume, float pitch = 1f)
        {
            if (clip == null || _reveal == null) return;
            if (!ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap)) return;
            _reveal.PlayOneShot(clip, fxVolume * Mathf.Min(volume, ChestAudioRule.EventMax), pitch);
        }

        /// <summary>TC theo bậc (A4/A11): stingClips[rank], âm lượng ≤ StingMax.</summary>
        private void PlaySting(int rank, float volume)
        {
            if (stingClips == null || rank < 0 || rank >= stingClips.Length || stingClips[rank] == null) return;
            _reveal.PlayOneShot(stingClips[rank], fxVolume * Mathf.Min(volume, ChestAudioRule.StingMax));
        }

        private void PlayLater(AudioClip clip, float volume, float delay)
        {
            if (clip != null) StartCoroutine(PlayLaterRoutine(clip, volume, delay));
        }

        private IEnumerator PlayLaterRoutine(AudioClip clip, float volume, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_reveal != null) _reveal.PlayOneShot(clip, fxVolume * volume);
        }

        // ---------------------------------------------------------------- cả thùng: nghiêng, giật, đổ

        private void Kick(float degrees)
        {
            float a = (float)_rng.NextDouble() * 360f;
            _tiltAxis = Quaternion.AngleAxis(a, Vector3.up) * Vector3.right;
            _tiltVel += degrees * 18f;
        }

        private void Tip(float degrees)
        {
            // Đổ về phía trước-ngang (ra xa con mèo) rồi lò xo dựng lại để bóng vật phẩm mọc đúng miệng thùng.
            Vector3 away = _cat != null ? Vector3.ProjectOnPlane(_base - _cat.position, Vector3.up) : _reveal.transform.forward;
            if (away.sqrMagnitude < 1e-4f) away = _reveal.transform.forward;
            _tiltAxis = Vector3.Cross(Vector3.up, away.normalized);
            _tiltTarget = degrees;
            StartCoroutine(ReleaseTip(0.35f));
        }

        private IEnumerator ReleaseTip(float after)
        {
            yield return new WaitForSeconds(after);
            _tiltTarget = 0f;
        }

        private void UpdateTilt()
        {
            if (!_hasBaseRot) return;
            // Lò xo tắt dần: góc bám theo _tiltTarget, các cú Kick cộng vận tốc.
            float k = _tiltTarget > 0f ? 60f : 90f, damp = 9f;
            _tiltVel += ((_tiltTarget - _tilt) * k - _tiltVel * damp) * Time.deltaTime;
            _tilt += _tiltVel * Time.deltaTime;
            _tilt = Mathf.Clamp(_tilt, -30f, 95f);
            _wobble = (_started && !_locked) ? Mathf.Sin((Time.time - _startTime) * 9f) * (_rouletteOn ? 1.6f : 0.6f) : 0f;
            _reveal.transform.rotation = Quaternion.AngleAxis(_tilt + _wobble, _tiltAxis) * _baseRot;
        }

        // ---------------------------------------------------------------- rác vật lý

        private void SpawnTrash(int count, float power, bool spill, bool shoe = false)
        {
            EnsureFloor();
            Vector3 mouth = _reveal.MouthPosition;
            Vector3 away = Vector3.Cross(_tiltAxis, Vector3.up).normalized;
            for (int i = 0; i < count && _trash.Count < maxTrash; i++)
            {
                int kind = shoe ? 1 : _rng.Next(4);
                GameObject g = kind == 0 ? Can() : kind == 1 ? Shoe() : kind == 2 ? FishBone() : Bag();
                g.transform.position = mouth + Random3(0.25f) * _size;
                g.transform.rotation = Quaternion.Euler((float)_rng.NextDouble() * 360f, (float)_rng.NextDouble() * 360f, 0f);
                Rigidbody rb = g.AddComponent<Rigidbody>();
                rb.mass = 0.2f;
                rb.angularDamping = 0.6f;
                Vector3 dir = spill ? (away * 0.9f + Vector3.up * 0.8f + Random3(0.5f)) : (Vector3.up + Random3(0.4f));
                rb.AddForce(dir.normalized * power * rb.mass * (0.6f + (float)_rng.NextDouble() * 0.8f) * _boost, ForceMode.Impulse);
                rb.angularVelocity = Random3(12f);
                _trash.Add(g);
            }
        }

        private GameObject Can()
        {
            Color c = _rng.NextDouble() < 0.5 ? new Color(0.75f, 0.1f, 0.1f) : new Color(0.7f, 0.72f, 0.75f);
            return Prim("Rac_Lon", PrimitiveType.Cylinder, c, new Vector3(0.07f, 0.06f, 0.07f), true);
        }

        private GameObject Shoe()
        {
            GameObject g = Prim("Rac_Giay", PrimitiveType.Cube, new Color(0.3f, 0.2f, 0.12f), new Vector3(0.07f, 0.05f, 0.16f), true);
            GameObject sole = Prim("De", PrimitiveType.Cube, new Color(0.9f, 0.88f, 0.8f), new Vector3(1.1f, 0.3f, 1.05f), false);
            sole.transform.SetParent(g.transform, false);
            sole.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            return g;
        }

        private GameObject FishBone()
        {
            Color bone = new Color(0.93f, 0.9f, 0.8f);
            GameObject g = Prim("Rac_XuongCa", PrimitiveType.Cube, bone, new Vector3(0.16f, 0.012f, 0.012f), true);
            for (int i = -2; i <= 2; i++)
            {
                GameObject rib = Prim("Suon", PrimitiveType.Cube, bone, Vector3.one, false);
                rib.transform.SetParent(g.transform, false);
                rib.transform.localPosition = new Vector3(i * 0.17f, 0f, 0f);
                rib.transform.localScale = new Vector3(0.05f, 1f, 4f);
            }
            GameObject head = Prim("Dau", PrimitiveType.Sphere, bone, Vector3.one * 0.035f, false);
            head.transform.SetParent(g.transform, false);
            head.transform.localPosition = new Vector3(0.55f, 0f, 0f);
            head.transform.localScale = new Vector3(0.25f, 3f, 3f);
            return g;
        }

        private GameObject Bag()
        {
            return Prim("Rac_Tui", PrimitiveType.Sphere, new Color(0.12f, 0.12f, 0.14f), new Vector3(0.11f, 0.09f, 0.1f), true);
        }

        private void EnsureFloor()
        {
            if (_floor != null) return;
            // Sàn vô hình để rác có chỗ lăn (nếu cảnh đã có sàn thì sàn này trùng mặt, không hại gì).
            _floor = new GameObject("RacQuy_Floor");
            BoxCollider bc = _floor.AddComponent<BoxCollider>();
            bc.size = new Vector3(4f * _size, 0.1f, 4f * _size);
            _floor.transform.position = _base - Vector3.up * 0.05f;
        }

        private IEnumerator Glint(Color tierColor, float scale)
        {
            // Món quý lẫn trong đống rác: một ngôi sao lóe ở giữa đống, lặp 3 lần.
            yield return new WaitForSeconds(0.9f);
            Vector3 heap = _base;
            int n = 0;
            foreach (GameObject g in _trash) if (g != null) { heap += g.transform.position; n++; }
            heap = n > 0 ? (heap - _base) / n : _base + Vector3.forward * _size;
            GameObject star = Prim("RacQuy_Glint", PrimitiveType.Quad, Color.Lerp(Color.white, tierColor, 0.35f), Vector3.one, false);
            _glint = star.transform;
            Material m = ChestFxFactory.CreateAdditive(ChestFxFactory.GetTexture(ParticleLook.Sparkle), "RacQuy_Glint_Mat");
            ChestFxFactory.SetColor(m, Color.Lerp(Color.white, tierColor, 0.35f));
            star.GetComponent<Renderer>().sharedMaterial = m;
            _owned.Add(m);
            for (int k = 0; k < 3 && _glint != null; k++)
            {
                for (float t = 0f; t < 0.45f && _glint != null; t += Time.deltaTime)
                {
                    float s = Mathf.Sin(t / 0.45f * Mathf.PI) * 0.25f * scale * _size;
                    _glint.position = heap + Vector3.up * 0.03f;
                    if (Camera.main != null) _glint.rotation = Quaternion.LookRotation(_glint.position - Camera.main.transform.position) * Quaternion.Euler(0f, 0f, t * 200f);
                    _glint.localScale = Vector3.one * s;
                    yield return null;
                }
                _reveal.PlayOneShot(clankClip, fxVolume * ChestAudioRule.TailMax * 0.6f, 2.2f);   // đuôi, nhỏ
                yield return new WaitForSeconds(0.25f);
            }
        }

        private void Holy(Color tierColor, bool legend)
        {
            ChestFxFactory.SetParticleColor(_holy, Color.Lerp(holyColor, tierColor, 0.25f), _holyStyle, legend ? 1.6f : 1.1f);
            ParticleSystem.EmissionModule hem = _holy.emission;
            hem.rateOverTime = legend ? 90f : 40f;
            _holy.Play();
            _holy.Emit(legend ? 70 : 30);
            StartCoroutine(StopHolyAfter(legend ? 2.6f : 1.2f));
        }

        private IEnumerator StopHolyAfter(float s)
        {
            yield return new WaitForSeconds(s);
            if (_holy == null) yield break;
            ParticleSystem.EmissionModule hem = _holy.emission;
            hem.rateOverTime = 0f;
        }

        // ---------------------------------------------------------------- xương cá (tương tác người chơi)

        private static bool ThrowPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return (Keyboard.current != null && Keyboard.current[Key.F].wasPressedThisFrame)
                || (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(1);
#endif
        }

        private void ThrowBone()
        {
            Camera cam = Camera.main;
            Vector3 from = cam != null ? cam.transform.position + cam.transform.forward * 0.3f - Vector3.up * 0.1f : _base + Vector3.up + Vector3.back;
            Vector3 side = cam != null ? Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized : Vector3.right;
            Vector3 target = _base + side * _size * 1.1f;
            GameObject g = FishBone();
            g.transform.position = from;
            Rigidbody rb = g.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            const float T = 0.7f;
            rb.linearVelocity = (target - from) / T + Vector3.up * (0.5f * 9.81f * T);
            rb.angularVelocity = new Vector3(0f, 0f, 15f);
            EnsureFloor();
            _trash.Add(g);
            _bone = g.transform;
            PlayEvent(throwClip, 0.5f);
            if (_catRoutine != null && !_rouletteOn) { StopCoroutine(_catRoutine); _catRoutine = StartCoroutine(CatChaseBone()); }
        }

        // ---------------------------------------------------------------- con mèo

        private void BuildCat()
        {
            float s = _size * 0.55f;
            Color dark = catColor * 0.6f;
            dark.a = 1f;
            _cat = new GameObject("RacQuy_Meo").transform;
            Transform body = Prim("Than", PrimitiveType.Capsule, catColor, new Vector3(0.45f, 0.5f, 0.45f) * s, false).transform;
            body.SetParent(_cat, false);
            body.localRotation = Quaternion.Euler(90f, 0f, 0f);
            body.localPosition = new Vector3(0f, 0.32f * s, 0f);
            _catHead = Prim("Dau", PrimitiveType.Sphere, catColor, Vector3.one * 0.38f * s, false).transform;
            _catHead.SetParent(_cat, false);
            _catHead.localPosition = new Vector3(0f, 0.55f * s, 0.42f * s);
            for (int i = -1; i <= 1; i += 2)
            {
                Transform ear = Prim("Tai", PrimitiveType.Cube, dark, new Vector3(0.1f, 0.14f, 0.04f) * s, false).transform;
                ear.SetParent(_catHead, false);
                ear.localPosition = new Vector3(i * 0.22f, 0.45f, 0f);
                ear.localRotation = Quaternion.Euler(0f, 0f, -i * 20f);
                ear.localScale = new Vector3(0.28f, 0.38f, 0.1f);
                Transform eye = Prim("Mat", PrimitiveType.Sphere, new Color(0.9f, 0.95f, 0.2f), Vector3.one, false).transform;
                eye.SetParent(_catHead, false);
                eye.localPosition = new Vector3(i * 0.2f, 0.1f, 0.42f);
                eye.localScale = Vector3.one * 0.16f;
            }
            _catTail = new GameObject("DuoiGoc").transform;
            _catTail.SetParent(_cat, false);
            _catTail.localPosition = new Vector3(0f, 0.4f * s, -0.42f * s);
            Transform tail = Prim("Duoi", PrimitiveType.Capsule, dark, new Vector3(0.07f, 0.25f, 0.07f) * s, false).transform;
            tail.SetParent(_catTail, false);
            tail.localPosition = new Vector3(0f, 0.22f * s, 0f);

            // Xuất phát ngoài khung, bên trái thùng nhìn từ camera.
            Vector3 side = Camera.main != null ? Vector3.ProjectOnPlane(-Camera.main.transform.right, Vector3.up).normalized : Vector3.left;
            if (side.sqrMagnitude < 1e-4f) side = Vector3.left;
            _cat.position = _base + side * _size * 2.6f;
            Face(_base);
        }

        private void Face(Vector3 p)
        {
            Vector3 d = Vector3.ProjectOnPlane(p - _cat.position, Vector3.up);
            if (d.sqrMagnitude > 1e-5f) _cat.rotation = Quaternion.LookRotation(d.normalized, Vector3.up);
        }

        private void AnimateTail(float speed, float lift)
        {
            if (_catTail == null) return;
            _catTail.localRotation = Quaternion.Euler(-lift + Mathf.Sin(Time.time * speed) * 12f, Mathf.Sin(Time.time * speed * 0.7f) * 35f, 0f);
        }

        private IEnumerator Walk(Vector3 to, float seconds)
        {
            Vector3 from = _cat.position;
            Face(to);
            for (float t = 0f; t < seconds && _cat != null; t += Time.deltaTime)
            {
                float u = t / seconds;
                _cat.position = Vector3.Lerp(from, to, u) + Vector3.up * Mathf.Abs(Mathf.Sin(u * 14f)) * _size * 0.03f;
                AnimateTail(6f, 10f);
                yield return null;
            }
        }

        private IEnumerator CatIntro()
        {
            Vector3 spot = _base + (_cat.position - _base).normalized * _size * 1.0f;
            yield return new WaitForSeconds(0.25f);     // sau tiếng ruồi ≥ 0,2 s (A3)
            PlayEvent(meowClip, 0.5f);
            yield return Walk(spot, 1.1f);
            Face(_base);
            yield return Crouch(99f);
        }

        private IEnumerator Crouch(float seconds)
        {
            // Rình: hạ thấp, ngoáy mông nhanh dần, thỉnh thoảng đẩy thùng một cái.
            float next = 0.8f;
            for (float t = 0f; t < seconds && _cat != null; t += Time.deltaTime)
            {
                float w = Mathf.Sin(Time.time * (6f + t * 3f)) * 6f;
                _cat.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(_base - _cat.position, Vector3.up).normalized, Vector3.up) * Quaternion.Euler(-6f, w, 0f);
                AnimateTail(8f + t * 2f, 30f);
                if (t > next) { Kick(2.5f * _boost); next += 0.9f; }
                yield return null;
            }
        }

        private IEnumerator CatChaseBone()
        {
            yield return new WaitForSeconds(0.5f);
            if (_bone != null)
            {
                PlayEvent(meowWantsClip, 0.5f);
                Vector3 b = _bone.position;
                b.y = _base.y;
                yield return Walk(b, 0.6f);
                if (_bone != null) { _trash.Remove(_bone.gameObject); Destroy(_bone.gameObject); }
                _boost = 1.6f;      // hăng máu
                yield return Walk(_base + (_cat.position - _base).normalized * _size, 0.5f);
            }
            Face(_base);
            yield return Crouch(99f);
        }

        private IEnumerator CatLeapOntoBin()
        {
            if (_cat == null) yield break;
            Vector3 from = _cat.position;
            Face(_base);
            for (float t = 0f; t < 0.45f && _cat != null; t += Time.deltaTime)
            {
                float u = t / 0.45f;
                Vector3 rim = _reveal.MouthPosition + (from - _base).normalized * _size * 0.3f;
                _cat.position = Vector3.Lerp(from, rim, u) + Vector3.up * Mathf.Sin(u * Mathf.PI) * _size * 0.6f;
                yield return null;
            }
            Kick(14f * _boost);
            PlayEvent(clankClip, 0.5f, 0.7f);
            // Đứng trên miệng thùng, cào xuống.
            while (_cat != null)
            {
                Vector3 rim = _reveal.MouthPosition + (from - _base).normalized * _size * 0.3f;
                _cat.position = rim + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * 11f)) * _size * 0.04f;
                Face(_reveal.MouthPosition);
                _cat.rotation *= Quaternion.Euler(35f, 0f, 0f);
                AnimateTail(14f, 50f);
                yield return null;
            }
        }

        private IEnumerator CatBored()
        {
            if (_cat == null) yield break;
            Vector3 down = _base + (_cat.position - _base).normalized * _size * 1.0f;
            down.y = _base.y;
            yield return Walk(down, 0.5f);
            yield return new WaitForSeconds(0.4f);    // ngửi... chán
            Vector3 away = _base + (down - _base).normalized * _size * 4f;
            for (float t = 0f; t < 2f && _cat != null; t += Time.deltaTime)
            {
                _cat.position = Vector3.MoveTowards(_cat.position, away, Time.deltaTime * _size * 0.8f);
                Face(away);
                AnimateTail(2f, -30f);               // đuôi cụp
                yield return null;
            }
        }

        private IEnumerator CatHopOffWithBone()
        {
            if (_cat == null) yield break;
            Vector3 down = _base + (_cat.position - _base).normalized * _size * 1.3f;
            down.y = _base.y;
            yield return Walk(down, 0.4f);
            _reveal.PlayOneShot(meowClip, fxVolume * ChestAudioRule.TailMax, 1.15f);   // đuôi
            Vector3 away = down + (down - _base).normalized * _size * 4f;
            yield return Walk(away, 1.6f);
        }

        private IEnumerator CatHiss()
        {
            if (_cat == null) yield break;
            Vector3 down = _base + (_cat.position - _base).normalized * _size * 1.4f;
            down.y = _base.y;
            yield return Walk(down, 0.35f);
            Face(_base);
            // Xù lông: phình to rồi lùi.
            Vector3 s0 = _cat.localScale;
            for (float t = 0f; t < 0.8f && _cat != null; t += Time.deltaTime)
            {
                _cat.localScale = s0 * (1f + Mathf.Sin(Mathf.Clamp01(t / 0.25f) * Mathf.PI * 0.5f) * 0.25f);
                AnimateTail(30f, 80f);
                yield return null;
            }
            yield return Walk(down + (down - _base).normalized * _size * 3f, 1.2f);
        }

        private void LaunchCat(float power)
        {
            if (_cat == null) return;
            _cat.gameObject.AddComponent<SphereCollider>().radius = _size * 0.2f;
            _catBody = _cat.gameObject.AddComponent<Rigidbody>();
            _catBody.mass = 0.5f;
            Vector3 away = Vector3.ProjectOnPlane(_cat.position - _base, Vector3.up).normalized;
            _catBody.AddForce((Vector3.up * 2.2f + away) * power * _catBody.mass * 2.2f, ForceMode.Impulse);
            _catBody.angularVelocity = Random3(14f);
        }

        // ---------------------------------------------------------------- dựng/dọn

        private void Measure()
        {
            Transform t = _reveal.transform;
            if (!_hasBaseRot) { _baseRot = t.rotation; _hasBaseRot = true; }
            Bounds b = new Bounds(t.position, Vector3.zero);
            bool has = false;
            foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer || r.transform.IsChildOf(transform)) continue;
                if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
            }
            _size = has ? Mathf.Max(0.15f, Mathf.Max(b.size.x, b.size.y, b.size.z)) : 0.4f;
            _base = has ? new Vector3(b.center.x, b.min.y, b.center.z) : t.position;
        }

        private GameObject Prim(string n, PrimitiveType type, Color c, Vector3 scale, bool keepCollider)
        {
            GameObject g = GameObject.CreatePrimitive(type);
            g.name = n;
            g.transform.localScale = scale;
            if (!keepCollider) { Collider col = g.GetComponent<Collider>(); if (col != null) Destroy(col); }
            g.GetComponent<Renderer>().sharedMaterial = Mat(c);
            return g;
        }

        private Material Mat(Color c)
        {
            if (_mats.TryGetValue(c, out Material m) && m != null) return m;
            Shader sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            m = sh != null ? new Material(sh) : ChestFxFactory.CreateUnlit(c, false, "RacQuy_Mat");
            m.color = c;
            m.hideFlags = HideFlags.DontSave;
            _mats[c] = m;
            _owned.Add(m);
            return m;
        }

        private Vector3 Random3(float r)
        {
            return new Vector3((float)_rng.NextDouble() * 2f - 1f, (float)_rng.NextDouble() * 2f - 1f, (float)_rng.NextDouble() * 2f - 1f) * r;
        }

        private void Place(Vector3 mouth)
        {
            _stink.transform.position = mouth;
            _flies.transform.position = mouth + Vector3.up * _size * 0.15f;
            _holy.transform.position = mouth;
        }

        private void BuildParticles()
        {
            if (_stink != null) return;
            _stinkStyle = Style(ParticleLook.SoftDot, ParticleDirection.Swirl, 0.12f, 0.09f, 2.4f, stinkColor, 0.5f);
            _stink = ChestFxFactory.CreateParticles("RacQuy_Stink", null, _stinkStyle, _size, false);

            _flyStyle = Style(ParticleLook.SoftDot, ParticleDirection.Swirl, 0.02f, 0.012f, 3.4f, flyColor, 0f);
            _flies = ChestFxFactory.CreateParticles("RacQuy_Flies", null, _flyStyle, _size, true);
            ParticleSystem.MainModule fm = _flies.main;
            fm.gravityModifier = 0f;
            ParticleSystem.ShapeModule fs = _flies.shape;
            fs.shapeType = ParticleSystemShapeType.Circle;
            fs.radius = _size * 0.6f;
            fs.rotation = new Vector3(90f, 0f, 0f);
            // Vận tốc hạt đặt qua ChestFxFactory.SetVelocity (cả 9 đường cong cùng chế độ, tránh lỗi Unity "must all be in the same mode").
            ChestFxFactory.SetVelocity(_flies, new Vector3(-0.05f, -0.04f, -0.05f), new Vector3(0.05f, 0.06f, 0.05f),
                new Vector3(0f, 3.5f, 0f), new Vector3(0f, 6f, 0f), 0f, ParticleSystemSimulationSpace.Local);
            ParticleSystemRenderer fr = _flies.GetComponent<ParticleSystemRenderer>();
            Destroy(fr.sharedMaterial);
            Material flyMat = ChestFxFactory.CreateUnlit(flyColor, false, "RacQuy_Fly_Mat");
            _owned.Add(flyMat);
            fr.sharedMaterial = flyMat;

            _holyStyle = Style(ParticleLook.Sparkle, ParticleDirection.Up, 1.3f, 0.03f, 1.3f, Color.white, 0.6f);
            _holy = ChestFxFactory.CreateParticles("RacQuy_Holy", null, _holyStyle, _size, false);
            ParticleSystem.ShapeModule hs = _holy.shape;
            hs.angle = 4f;
            hs.radius = _size * 0.1f;
            ParticleSystem.EmissionModule hem = _holy.emission;
            hem.rateOverTime = 0f;
        }

        private static ChestFxProfile Style(ParticleLook look, ParticleDirection dir, float speed, float size, float life, Color second, float mix)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.secondaryColor = second;
            s.secondaryMix = mix;
            return s;
        }

        private void Cleanup()
        {
            StopAllCoroutines();
            _catRoutine = null;
            StopBuild();
            if (_build != null) Destroy(_build);
            _build = null;
            if (_hasBaseRot && _reveal != null) _reveal.transform.rotation = _baseRot;
            _hasBaseRot = false;            // lần mở sau chụp lại hướng hiện tại (không giật về hướng cũ)
            _preLockTrash = 0;
            _lastEvent = -999f;
            _tilt = _tiltVel = _tiltTarget = 0f;
            foreach (GameObject g in _trash) if (g != null) Destroy(g);
            _trash.Clear();
            if (_cat != null) Destroy(_cat.gameObject);
            if (_floor != null) Destroy(_floor);
            if (_glint != null) Destroy(_glint.gameObject);
            _cat = null; _floor = null; _glint = null; _bone = null; _catBody = null;
            Kill(ref _stink);
            Kill(ref _flies);
            Kill(ref _holy);
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
            _mats.Clear();
            if (_stinkStyle != null) Destroy(_stinkStyle);
            if (_flyStyle != null) Destroy(_flyStyle);
            if (_holyStyle != null) Destroy(_holyStyle);
            _stinkStyle = _flyStyle = _holyStyle = null;
            _started = _rouletteOn = _locked = false;
            _boost = 1f;
        }

        private void Kill(ref ParticleSystem ps)
        {
            if (ps == null) return;
            Material m = ps.GetComponent<ParticleSystemRenderer>().sharedMaterial;
            if (m != null && !_owned.Contains(m)) Destroy(m);
            Destroy(ps.gameObject);
            ps = null;
        }
    }
}

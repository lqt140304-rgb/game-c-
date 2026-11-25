using System.Collections;
using System.Collections.Generic;
using ChoDoLa.Interaction;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// "Phi vụ tiếp tế" — kịch bản đặc trưng của Chest_KhongGian (spec: docs/superpowers/specs/2026-11-25-kichban-khonggian-design.md).
    /// Ngay khi bấm mở: máy nổ rì rầm và dâng dần, vùng quanh rương mất trọng lực (bu-lông, mảnh vụn trôi lên, rương
    /// lơ lửng nghiêng ngả). Tàu mẹ bay vào, thả pin nhiên liệu (người chơi bấm B để thả thêm pin — máy gầm nhanh hơn),
    /// rồi bắn một tên lửa bay vòng quanh rương, trúng nắp đúng lúc nắp bật. Đến khi bậc chốt mới biết kết cục:
    /// Rác: máy tắt phụt, im 0,2 s, tiếng xịt, trọng lực trở lại, rương rớt cái bịch, tàu ho khói lủi mất.
    /// Thường..Huyền thoại: rương nhảy bằng động cơ (lửa dưới đáy), tàu lượn chào một vòng nhỏ quanh rương rồi bay lên.
    /// Đã thu gọn theo kế hoạch 2026-11-25 (mục 5.11): bỏ thùng quà dù, đồng hồ nhiên liệu, pháo hoa; âm là file ElevenLabs.
    /// Vật lý thật: pin, mảnh vụn, tên lửa là Rigidbody (lực, va chạm với một sàn tạm). Rương không gắn
    /// Rigidbody (để không đánh nhau với BoxOpenSequence) mà dùng lò xo–giảm chấn cộng dồn lên vị trí/góc.
    /// Chỉ trang trí: không bao giờ đổi bậc; ngẫu nhiên theo hạt giống <see cref="seed"/>.
    /// </summary>
    [DefaultExecutionOrder(-500)]   // gỡ độ lệch của khung trước trước khi BoxOpenSequence chạy
    public class KhongGianMissionHook : ChestFxHook
    {
        [Header("Ngẫu nhiên (chỉ trang trí)")]
        [Tooltip("0 = lấy theo thời gian.")] public int seed;

        [Header("Tương tác")]
        [Tooltip("Phím thả thêm pin trong lúc tàu đang tiếp nhiên liệu.")] public bool allowPlayerFuel = true;
        [SerializeField] private int maxPlayerCells = 3;   // trần cứng 3 (T7), kể cả prefab cũ lưu số lớn hơn
        [SerializeField] private int autoCells = 2;        // trần cứng 2

        [Header("Âm (file ElevenLabs trong Audio/Chests/khong_gian, gán bởi Editor/KhongGianFxSetup)")]
        [Tooltip("Nền: vòng lặp máy nổ (đã có nhịp pít-tông nên không cần lớp Nhịp).")] public AudioClip engineClip;
        [Tooltip("Bíp radio (sự kiện).")] public AudioClip blipClip;
        [Tooltip("Pin vào miệng rương (sự kiện).")] public AudioClip fuelClip;
        [Tooltip("Phóng tên lửa (sự kiện).")] public AudioClip rocketClip;
        [Tooltip("Tên lửa trúng nắp (HĐ).")] public AudioClip impactClip;
        [Tooltip("Khí xì mở nắp (sự kiện, ngay sau cú trúng).")] public AudioClip hatchClip;
        [Tooltip("Rác: máy ho tắt (đuôi).")] public AudioClip sputterClip;
        [Tooltip("TC: tiếng chốt theo bậc, 5 ô Rác..Huyền thoại.")] public AudioClip[] stingClips = new AudioClip[5];
        [Range(0f, 1f)] public float sfxVolume = 0.8f;   // nhân MỘT lần duy nhất, trong Sfx()

        [Header("Nhịp (giây, khi không có BoxOpenSequence)")]
        [SerializeField] private float fallbackImpactTime = 4.5f;
        [SerializeField] private float lowGravity = 0.18f;   // phần trăm trọng lực khi "mất trọng lực"

        [Header("Màu")]
        [SerializeField] private Color hullColor = new Color(0.72f, 0.76f, 0.82f);
        [SerializeField] private Color lightColor = new Color(0.4f, 0.95f, 1f);
        [SerializeField] private Color fuelColor = new Color(0.45f, 1f, 0.35f);
        [SerializeField] private Color flameColor = new Color(1f, 0.55f, 0.15f);
        [SerializeField] private Color smokeColor = new Color(0.55f, 0.55f, 0.58f);

        // ---- trạng thái
        private ChestRevealSequence _reveal;
        private System.Random _rng;
        private float _t0, _impactAt, _size = 0.5f, _groundY, _lastEvent = -999f, _lastTail = -999f;
        private Vector3 _base, _camRight = Vector3.right, _camFwd = Vector3.forward;
        private bool _started, _impacted, _locked, _ending, _gravityBack;
        private ChestTier _tier;
        private int _fuel, _playerCells;
        private float _tension, _engineTarget = -1f, _engineVolMul = 1f, _engineFade;

        // lò xo của cả rương
        private float _offY, _velY, _hover, _tiltX, _tiltZ, _vtX, _vtZ, _stiff = 40f;
        private Vector3 _appliedPos; private Quaternion _appliedRot; private bool _hasApplied;
        private Vector3 _writtenPos; private Quaternion _writtenRot;

        // vật thể
        private readonly List<Object> _owned = new List<Object>();
        private readonly List<Rigidbody> _floaters = new List<Rigidbody>();
        private readonly List<Rigidbody> _cells = new List<Rigidbody>();
        private readonly List<float> _cellBorn = new List<float>();
        private Transform _ship;
        private Rigidbody _rocket;
        private AudioSource _engine;
        private ParticleSystem _smoke, _flame, _spark;
        private ChestFxProfile _smokeStyle, _flameStyle, _sparkStyle;

        // ------------------------------------------------------------------ Bắt đầu NGAY khi bấm mở

        private void Start()
        {
            _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null) { enabled = false; return; }
            Begin();
        }

        private void Begin()
        {
            if (_started) return;
            _started = true;
            _t0 = Time.time;
            _rng = new System.Random(seed != 0 ? seed : System.Environment.TickCount);
            Measure();
            _impactAt = _t0 + ImpactDelay();
            BuildCommon();

            // Âm bắt đầu ngay: máy nổ khởi động + bíp radio của tàu mẹ.
            autoCells = Mathf.Clamp(autoCells, 0, 2);
            maxPlayerCells = Mathf.Clamp(maxPlayerCells, 0, 3);
            if (engineClip != null)
            {
                _engine = gameObject.AddComponent<AudioSource>();
                _engine.clip = engineClip;
                _engine.loop = true;
                _engine.playOnAwake = false;
                _engine.spatialBlend = 0f;
                _engine.volume = ChestAudioRule.BedMax * 0.33f;   // 0,15 lúc đầu (A2)
                _engine.pitch = 0.8f;
                _engine.Play();
            }
            Event(blipClip, 0.4f, 1f);

            StartCoroutine(Mission());
        }

        /// <summary>Thời điểm nắp bật mạnh (stage có lidAngle lớn đầu tiên) tính từ lúc bấm — tên lửa trúng đúng lúc đó.</summary>
        private float ImpactDelay()
        {
            BoxOpenSequence box = _reveal.Box;
            if (box == null || box.Stages == null) return fallbackImpactTime;
            float t = 0f;
            foreach (BoxOpenSequence.Stage s in box.Stages)
            {
                t += s.travel;
                if (s.lidAngle >= 30f) return Mathf.Max(2.5f, t);
                t += s.hold;
            }
            return Mathf.Max(2.5f, t);
        }

        private void Measure()
        {
            Transform root = _reveal.transform;
            Transform sil = _reveal.Silhouette;
            bool any = false;
            Bounds b = new Bounds(root.position, Vector3.zero);
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer || r.transform.IsChildOf(transform)) continue;
                if (sil != null && r.transform.IsChildOf(sil)) continue;
                if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            }
            _size = any ? Mathf.Clamp(b.size.magnitude * 0.6f, 0.2f, 3f) : 0.5f;
            _groundY = any ? b.min.y : root.position.y;
            _base = new Vector3(b.center.x, _groundY, b.center.z);
            Camera cam = Camera.main;
            if (cam != null)
            {
                _camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
                _camFwd = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                if (_camFwd.sqrMagnitude < 0.01f) _camFwd = Vector3.forward;
            }
        }

        // ------------------------------------------------------------------ Kịch bản

        private IEnumerator Mission()
        {
            float lead = _impactAt - Time.time;

            // 0.0 s: mất trọng lực — rương nổi lên, mảnh vụn bốc lên trôi.
            _hover = 0.04f * _size;
            _stiff = 18f;
            SpawnFloaters(4);

            // 0.3 s: tàu mẹ bay vào (đường cong Bezier, nghiêng theo hướng bay).
            yield return new WaitForSeconds(Mathf.Min(0.3f, lead * 0.08f));
            BuildShip();
            Vector3 hoverAt = _base + Vector3.up * (_size * 1.8f);
            Vector3 from = hoverAt - _camRight * (_size * 3f);   // vào từ 3 S (T2)
            float fly = Mathf.Clamp(lead * 0.25f, 0.8f, 1.3f);
            yield return FlyShip(from, hoverAt + Vector3.up * _size * 0.6f - _camRight * _size, hoverAt, fly, true);
            Event(blipClip, 0.35f, 1.25f);

            // Tiếp nhiên liệu: pin tự rơi theo nhịp; người chơi bấm B thả thêm (đến lúc tên lửa phóng).
            float launchAt = _impactAt - Mathf.Clamp(lead * 0.4f, 1.2f, 2.2f);
            float fuelWindow = Mathf.Max(0.4f, launchAt - Time.time - 0.2f);
            for (int i = 0; i < autoCells; i++) StartCoroutine(DropCellLater(fuelWindow * (i + 0.3f) / autoCells));
            while (Time.time < launchAt)
            {
                if (allowPlayerFuel && _playerCells < maxPlayerCells && FuelPressed()) { _playerCells++; DropCell(); }
                yield return null;
            }

            // Phóng tên lửa: bay vòng quanh rương rồi lao xuống nắp, trúng đúng _impactAt.
            Event(rocketClip, 0.5f, 1f);
            LaunchRocket();
            while (!_impacted) yield return null;

            // Chờ bậc chốt (có thể đã chốt trước) rồi mới lộ kết cục.
            while (!_locked) yield return null;
            yield return Ending();
        }

        private IEnumerator Ending()
        {
            _ending = true;
            int rank = _tier.Rank();
            Color c = _reveal.ColorOf(_tier);

            // A4: Nền dừng ngay (Rác, Hiếm+) hoặc fade 0,3 s (Thường). Tiếng chốt TC luôn do hook phát (stingClips).
            if (_tier == ChestTier.Rac || rank >= 2) StopEngine(); else _engineFade = 1f / 0.3f;

            if (_tier == ChestTier.Rac)
            {
                // Rớt đột ngột: lặng 0,2 s rồi MỘT tiếng xịt; không phát gì khác trong 0,6 s sau đó.
                _engineTarget = 0f;
                _gravityBack = true;
                _hover = 0f;
                _stiff = 90f;
                _velY = -1.2f * _size;
                _vtZ += 120f;
                Emit(_smoke, _base + Vector3.up * _size * 0.5f, 25, smokeColor);
                StartCoroutine(ShipLimpsAway());
                yield return new WaitForSeconds(ChestAudioRule.RacSilence);
                Sting(0, 0.7f);
                yield return new WaitForSeconds(0.7f);
                Tail(sputterClip, 0.4f, 0.9f);   // đuôi: máy ho tắt (đã thay 5 "pụp" bằng 1 tiếng ho)
                yield break;
            }

            // Bùng nổ dâng cao: rương nhảy bằng động cơ. Tiếng nổ đã phát ở cú trúng nắp nên không có HĐ nữa.
            _engineTarget = 1.15f + 0.12f * rank;
            Sting(rank, StingVolume[rank]);
            _velY += (1.2f + 0.45f * rank) * _size;
            _hover = 0.02f * _size;
            _stiff = 30f;
            Emit(_flame, _base, 20 + 12 * rank, flameColor);
            Emit(_spark, _reveal.MouthPosition, 15 + 15 * rank, c);
            yield return new WaitForSeconds(0.35f);
            _engineTarget = 0f;
            yield return new WaitForSeconds(0.1f);   // đuôi chỉ bắt đầu từ +0,4 s (A5)
            StartCoroutine(ShipVictory(rank));
        }

        // ------------------------------------------------------------------ Hook của khung chung

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            // Lưới an toàn: nếu hook được dùng lại (không qua Start) thì vẫn chạy từ đây.
            if (_reveal == null) _reveal = reveal;
            if (!_started) Begin();
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            _tier = tier;   // chỉ đọc: kết cục trang trí theo bậc game đã quyết
            _locked = true;
        }

        public override void OnRevealed(ChestRevealSequence reveal) => StartCoroutine(FadeFloaters(2.5f));

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        // ------------------------------------------------------------------ Mỗi khung

        private void Update()
        {
            // Gỡ độ lệch khung trước (nếu BoxOpenSequence chưa ghi đè) để không cộng dồn.
            if (_hasApplied && _reveal != null)
            {
                Transform r = _reveal.transform;
                if ((r.position - _writtenPos).sqrMagnitude < 1e-10f) r.position -= _appliedPos;
                if (Quaternion.Angle(r.rotation, _writtenRot) < 0.01f) r.rotation = Quaternion.Inverse(_appliedRot) * r.rotation;
                _hasApplied = false;
            }
            if (!_started) return;

            float dt = Time.deltaTime;
            // Độ căng: dâng theo thời gian tới cú trúng + pin nạp vào.
            float prog = Mathf.Clamp01((Time.time - _t0) / Mathf.Max(0.1f, _impactAt - _t0));
            float target = Mathf.Pow(prog, 1.4f) * 0.75f + Mathf.Min(0.3f, _fuel * 0.06f);
            if (_engineTarget >= 0f) target = _engineTarget;
            _tension = Mathf.MoveTowards(_tension, target, dt * (_ending ? (_tier == ChestTier.Rac ? 0.9f : 1.6f) : 0.8f));
            if (_engine != null)
            {
                float k = Mathf.Clamp01(_tension);
                _engine.pitch = Mathf.Lerp(0.8f, 1.25f, k);                                       // +~55% tới lúc chốt (A2)
                if (_engineFade > 0f) _engineVolMul = Mathf.MoveTowards(_engineVolMul, 0f, dt * _engineFade);
                _engine.volume = Mathf.Lerp(0.15f, ChestAudioRule.BedMax, k) * _engineVolMul;     // 0,15 → 0,45 (không nhân sfxVolume: bậc A6 là trần)
                if (_engineFade > 0f && _engineVolMul <= 0f) StopEngine();
            }

            // Lò xo–giảm chấn của rương + rung theo độ căng (máy nổ).
            float rumble = _ending ? 0f : _tension * 0.006f * _size;
            _velY += (-_stiff * (_offY - _hover) - 2f * Mathf.Sqrt(_stiff) * 0.35f * _velY) * dt;
            if (_gravityBack && _offY > 0f) _velY -= 9.81f * dt;
            _offY += _velY * dt;
            if (_offY < -0.02f * _size) { _offY = -0.02f * _size; _velY = -_velY * 0.25f; }   // chạm sàn, nảy nhẹ
            float wob = _ending ? 0f : 2.5f * (1f - Mathf.Clamp01(_tension));
            float t = Time.time - _t0;
            float tx = Mathf.Sin(t * 1.3f) * wob, tz = Mathf.Sin(t * 0.9f + 1f) * wob;
            _vtX += (-25f * (_tiltX - tx) - 4f * _vtX) * dt; _tiltX += _vtX * dt;
            _vtZ += (-25f * (_tiltZ - tz) - 4f * _vtZ) * dt; _tiltZ += _vtZ * dt;
            _appliedPos = Vector3.up * _offY + new Vector3(Mathf.PerlinNoise(t * 30f, 0.1f) - 0.5f, 0f, Mathf.PerlinNoise(0.4f, t * 30f) - 0.5f) * 2f * rumble;
            _appliedRot = Quaternion.Euler(_tiltX, 0f, _tiltZ);

            UpdateCells();
        }

        private void LateUpdate()
        {
            if (!_started || _reveal == null) return;
            Transform r = _reveal.transform;
            r.position += _appliedPos;
            r.rotation = _appliedRot * r.rotation;
            _writtenPos = r.position;
            _writtenRot = r.rotation;
            _hasApplied = true;
        }

        private void FixedUpdate()
        {
            if (!_started) return;
            Vector3 g = Physics.gravity * (_gravityBack ? 1f : lowGravity);
            foreach (Rigidbody rb in _floaters) if (rb != null) rb.AddForce(g, ForceMode.Acceleration);
            foreach (Rigidbody rb in _cells) if (rb != null) rb.AddForce(Physics.gravity * lowGravity, ForceMode.Acceleration);
            if (_rocket != null) SteerRocket();
        }

        // ------------------------------------------------------------------ Tàu mẹ

        private void BuildShip()
        {
            GameObject root = Own(new GameObject("KhongGian_TauMe"));
            _ship = root.transform;
            float s = _size;
            Prim(PrimitiveType.Sphere, _ship, Vector3.zero, new Vector3(1.6f, 0.32f, 1.6f) * s, hullColor, false);
            Prim(PrimitiveType.Sphere, _ship, Vector3.up * 0.12f * s, new Vector3(0.6f, 0.4f, 0.6f) * s, lightColor * 0.8f, true);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                Prim(PrimitiveType.Sphere, _ship, new Vector3(Mathf.Cos(a), -0.02f, Mathf.Sin(a)) * 0.72f * s, Vector3.one * 0.09f * s,
                    i % 2 == 0 ? lightColor : flameColor, true);
            }
            Prim(PrimitiveType.Cylinder, _ship, Vector3.down * 0.12f * s, new Vector3(0.35f, 0.02f, 0.35f) * s, fuelColor, true);
        }

        private IEnumerator FlyShip(Vector3 a, Vector3 b, Vector3 c, float seconds, bool easeIn)
        {
            Vector3 prev = a;
            for (float t = 0f; t < 1f && _ship != null; t += Time.deltaTime / seconds)
            {
                float u = easeIn ? 1f - (1f - t) * (1f - t) * (1f - t) : t * t;
                Vector3 p = Vector3.Lerp(Vector3.Lerp(a, b, u), Vector3.Lerp(b, c, u), u);
                Vector3 vel = (p - prev) / Mathf.Max(1e-4f, Time.deltaTime);
                prev = p;
                _ship.position = p + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.03f * _size;
                _ship.rotation = Quaternion.Euler(Vector3.Dot(vel, _camFwd) * 6f / _size, Time.time * 40f, -Vector3.Dot(vel, _camRight) * 6f / _size);
                Emit(_flame, _ship.position + Vector3.down * 0.12f * _size, 1, flameColor);
                yield return null;
            }
            if (_ship != null) _ship.position = c;
            if (_ship != null) StartCoroutine(Hover(c));
        }

        private IEnumerator Hover(Vector3 at)
        {
            while (_ship != null && !_ending)
            {
                _ship.position = at + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.04f * _size;
                _ship.rotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.7f) * 3f, Time.time * 40f, Mathf.Sin(Time.time * 1.3f) * 3f);
                yield return null;
            }
        }

        private IEnumerator ShipLimpsAway()
        {
            if (_ship == null) yield break;
            bool sad = false;
            Vector3 a = _ship.position;
            for (float t = 0f; t < 1f && _ship != null; t += Time.deltaTime / 1.6f)
            {
                if (!sad && t >= 0.75f) { sad = true; Tail(blipClip, 0.3f, 0.6f); }   // bíp trầm buồn, ~1,2 s sau chốt (sau lặng + xịt)
                _ship.position = a + _camRight * (t * t * _size * 3f) + Vector3.down * Mathf.Sin(t * 14f) * 0.08f * _size - Vector3.up * t * _size * 0.6f;   // biến mất ở 3 S (T2)
                _ship.rotation = Quaternion.Euler(Mathf.Sin(t * 20f) * 12f, Time.time * 25f, 18f);
                Emit(_smoke, _ship.position, 2, smokeColor);
                yield return null;
            }
            if (_ship != null) _ship.gameObject.SetActive(false);
        }

        private IEnumerator ShipVictory(int rank)
        {
            if (_ship == null) yield break;
            Tail(blipClip, 0.4f, 1.5f);
            // Chào MỘT vòng quanh rương, bán kính 1,2 S, rồi lên (cao tối đa 3 S so với đáy rương) — T2.
            Vector3 a = _ship.position;
            Vector3 center = new Vector3(_base.x, a.y, _base.z);
            float startAng = Mathf.Atan2(Vector3.Dot(a - center, _camFwd), Vector3.Dot(a - center, _camRight));
            float startR = Mathf.Min(1.2f * _size, Vector3.ProjectOnPlane(a - center, Vector3.up).magnitude);
            float rise = Mathf.Max(0f, 3f * _size - (a.y - _groundY));
            for (float t = 0f; t < 1f && _ship != null; t += Time.deltaTime / 2f)
            {
                float ang = startAng + t * Mathf.PI * 2f;
                float r = Mathf.Lerp(startR, 1.2f * _size, Mathf.Clamp01(t * 3f));
                Vector3 orbit = (Mathf.Cos(ang) * _camRight + Mathf.Sin(ang) * _camFwd) * r;
                _ship.position = center + orbit + Vector3.up * t * t * rise;
                _ship.rotation = Quaternion.Euler(0f, Time.time * 120f, 25f * Mathf.Sin(ang));
                Emit(_flame, _ship.position, 1, flameColor);
                yield return null;
            }
            if (_ship != null) _ship.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ Pin nhiên liệu (tương tác)

        /// <summary>Thả thêm một pin (gọi được từ UI/tay cầm). Chỉ làm máy gầm nhanh hơn, không ảnh hưởng bậc.</summary>
        public void DropFuelCell()
        {
            if (_started && !_ending && !_impacted && _playerCells < maxPlayerCells) { _playerCells++; DropCell(); }
        }

        private static bool FuelPressed()
        {
#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb.bKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.B);
#endif
        }

        private IEnumerator DropCellLater(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (!_impacted) DropCell();
        }

        private void DropCell()
        {
            if (_ship == null) return;
            Vector3 start = _ship.position + Vector3.down * 0.15f * _size + Rand3() * 0.08f * _size;
            GameObject go = Prim(PrimitiveType.Cylinder, null, start, new Vector3(0.07f, 0.06f, 0.07f) * _size, fuelColor, true);
            go.name = "KhongGian_Pin";
            go.transform.rotation = Random3Rot();
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.2f;
            rb.useGravity = false;
            // Ném theo quỹ đạo đạn đạo trọng lực thấp: tới miệng rương sau ~0.9 s.
            float flight = 0.9f;
            Vector3 g = Physics.gravity * lowGravity;
            rb.linearVelocity = (_reveal.MouthPosition - start - 0.5f * g * flight * flight) / flight;
            rb.angularVelocity = Rand3() * 6f;
            _cells.Add(rb);
            _cellBorn.Add(Time.time);
        }

        private void UpdateCells()
        {
            Vector3 mouth = _reveal.MouthPosition;
            for (int i = _cells.Count - 1; i >= 0; i--)
            {
                Rigidbody rb = _cells[i];
                bool gone = rb == null;
                if (!gone && ((rb.position - mouth).magnitude < 0.22f * _size || Time.time - _cellBorn[i] > 1.6f))
                {
                    _fuel++;
                    Event(fuelClip, 0.4f, 0.95f + 0.05f * _fuel);
                    Emit(_spark, mouth, 10, fuelColor);
                    _velY -= 0.25f * _size;        // rương lún xuống nhận pin...
                    _vtX += (float)(_rng.NextDouble() - 0.5) * 60f;
                    Destroy(rb.gameObject);
                    gone = true;
                }
                if (gone) { _cells.RemoveAt(i); _cellBorn.RemoveAt(i); }
            }
        }

        // ------------------------------------------------------------------ Tên lửa

        private void LaunchRocket()
        {
            Vector3 start = _ship != null ? _ship.position + Vector3.down * 0.1f * _size : _base + Vector3.up * _size * 2f;
            GameObject go = Prim(PrimitiveType.Capsule, null, start, new Vector3(0.05f, 0.09f, 0.05f) * _size, new Color(0.9f, 0.2f, 0.15f), false);
            go.name = "KhongGian_TenLua";
            Collider col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);   // không cần va chạm vật lý: tự dẫn đường tới nắp
            _rocket = go.AddComponent<Rigidbody>();
            _rocket.useGravity = false;
            _rocket.mass = 0.3f;
            float side = _rng.NextDouble() < 0.5 ? -1f : 1f;
            _rocket.linearVelocity = (_camRight * side * 1.6f + Vector3.up * 0.8f) * _size;
        }

        private void SteerRocket()
        {
            Vector3 target = _reveal.MouthPosition + Vector3.up * 0.04f * _size;
            Vector3 p = _rocket.position;
            float rem = _impactAt - Time.time;
            Vector3 to = target - p;
            if (rem <= 0.02f || to.magnitude < 0.06f * _size) { Impact(); return; }
            // Dẫn đường: phần "về đích đúng giờ" + phần quay vòng quanh rương, nhỏ dần khi sắp tới giờ.
            Vector3 desired = to / Mathf.Max(rem, 0.05f);
            Vector3 orbit = Vector3.Cross(Vector3.up, to).normalized * _size * 1.4f * Mathf.Clamp01((rem - 0.35f) / 0.8f);
            desired += orbit + Vector3.up * _size * 0.6f * Mathf.Clamp01(rem - 0.5f);
            _rocket.AddForce((desired - _rocket.linearVelocity) * 9f, ForceMode.Acceleration);
            Vector3 v = _rocket.linearVelocity;
            if (v.sqrMagnitude > 1e-4f) _rocket.MoveRotation(Quaternion.FromToRotation(Vector3.up, v));
            Emit(_smoke, p - v.normalized * 0.06f * _size, 2, smokeColor);
            Emit(_flame, p - v.normalized * 0.05f * _size, 1, flameColor);
        }

        private void Impact()
        {
            if (_impacted) return;
            _impacted = true;
            Vector3 at = _rocket != null ? _rocket.position : _reveal.MouthPosition;
            if (_rocket != null) Destroy(_rocket.gameObject);
            _rocket = null;
            // Cú trúng giống nhau ở mọi bậc (giữ bí mật): nổ, nắp bật (BoxOpenSequence), rương bị hất, mảnh vụn văng.
            Sfx(impactClip, 1f, 1f, ChestAudioRule.ActionMax);   // HĐ duy nhất của cả màn
            StartCoroutine(HatchHissLater());
            Emit(_flame, at, 35, flameColor);
            Emit(_smoke, at, 25, smokeColor);
            _velY += 0.9f * _size;
            _vtX += (float)(_rng.NextDouble() - 0.5) * 220f;
            _vtZ += (float)(_rng.NextDouble() - 0.5) * 220f;
            foreach (Rigidbody rb in _floaters)
            {
                if (rb == null) continue;
                Vector3 d = rb.position - at;
                float k = 1f / Mathf.Max(0.3f, d.magnitude / _size);
                rb.AddForce((d.normalized + Vector3.up * 0.5f) * 1.6f * _size * k, ForceMode.VelocityChange);
                rb.angularVelocity += Rand3() * 8f;
            }
        }

        private IEnumerator HatchHissLater()
        {
            yield return new WaitForSeconds(0.25f);
            Event(hatchClip, 0.35f, 1f);
        }

        // ------------------------------------------------------------------ Mảnh vụn trôi

        private void SpawnFloaters(int n)
        {
            // Sàn tạm cho mọi Rigidbody của hiệu ứng (đặt đúng đáy rương).
            GameObject floor = Own(new GameObject("KhongGian_SanTam"));
            floor.transform.position = new Vector3(_base.x, _groundY - 0.025f, _base.z);
            BoxCollider bc = floor.AddComponent<BoxCollider>();
            bc.size = new Vector3(8f * _size, 0.05f, 8f * _size);

            for (int i = 0; i < n; i++)
            {
                float a = (float)(_rng.NextDouble() * Mathf.PI * 2f);
                float r = _size * (0.45f + 0.5f * (float)_rng.NextDouble());
                Vector3 p = _base + new Vector3(Mathf.Cos(a), 0.05f, Mathf.Sin(a)) * r;
                bool bolt = i % 3 != 0;
                Vector3 scale = bolt ? new Vector3(0.025f, 0.06f, 0.025f) * _size : Vector3.one * 0.045f * _size;
                Color col = bolt ? new Color(0.6f, 0.62f, 0.66f) : Color.Lerp(hullColor, flameColor, 0.3f);
                GameObject go = Prim(bolt ? PrimitiveType.Cylinder : PrimitiveType.Cube, null, p, scale, col, false);
                go.name = "KhongGian_ManhVun";
                go.transform.rotation = Random3Rot();
                Rigidbody rb = go.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.mass = 0.05f;
                rb.linearDamping = 0.4f;
                rb.angularDamping = 0.05f;
                // Bốc lên chậm như trong khoang không trọng lực.
                rb.linearVelocity = new Vector3(0f, (0.25f + 0.25f * (float)_rng.NextDouble()) * _size, 0f) + Rand3() * 0.05f * _size;
                rb.angularVelocity = Rand3() * 2f;
                _floaters.Add(rb);
            }
        }

        private IEnumerator FadeFloaters(float wait)
        {
            yield return new WaitForSeconds(wait);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.2f)
            {
                foreach (Rigidbody rb in _floaters) if (rb != null) rb.transform.localScale *= 1f - Mathf.Min(0.5f, Time.deltaTime * 3f);
                yield return null;
            }
            foreach (Rigidbody rb in _floaters) if (rb != null) Destroy(rb.gameObject);
            _floaters.Clear();
        }

        // ------------------------------------------------------------------ Tiện ích

        private void BuildCommon()
        {
            _smokeStyle = Style(ParticleLook.SoftDot, 0.12f, 0.09f, 1.6f, -0.05f, new Color(0.8f, 0.8f, 0.8f));
            _smoke = ChestFxFactory.CreateParticles("KhongGian_Khoi", transform, _smokeStyle, _size, true);
            _flameStyle = Style(ParticleLook.SoftDot, 0.5f, 0.05f, 0.35f, 0f, new Color(1f, 0.9f, 0.5f));
            _flame = ChestFxFactory.CreateParticles("KhongGian_Lua", transform, _flameStyle, _size, true);
            _sparkStyle = Style(ParticleLook.Sparkle, 1.2f, 0.025f, 0.9f, 0.1f, Color.white);
            _spark = ChestFxFactory.CreateParticles("KhongGian_TiaSang", transform, _sparkStyle, _size, true);
        }

        private ChestFxProfile Style(ParticleLook look, float speed, float size, float life, float gravity, Color secondary)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = ParticleDirection.Radial;   // chế độ của khung chung (orbital cùng một chế độ)
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.burstGravity = gravity;
            s.secondaryColor = secondary;
            s.secondaryMix = 0.4f;
            s.ambientRate = 0f;
            _owned.Add(s);
            return s;
        }

        private void Emit(ParticleSystem ps, Vector3 at, int count, Color c)
        {
            if (ps == null || count <= 0) return;
            ps.transform.position = at;
            ChestFxFactory.SetParticleColor(ps, c, ps == _smoke ? _smokeStyle : ps == _flame ? _flameStyle : _sparkStyle, 1f);
            ps.Emit(count);
        }

        private GameObject Prim(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Color color, bool glow)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.hideFlags = HideFlags.DontSave;
            if (parent != null) { go.transform.SetParent(parent, false); go.transform.localPosition = pos; }
            else { go.transform.position = pos; _owned.Add(go); }
            go.transform.localScale = scale;
            Renderer r = go.GetComponent<Renderer>();
            Material m = glow ? ChestFxFactory.CreateAdditive(null, "KhongGian_Glow") : ChestFxFactory.CreateUnlit(color, false, "KhongGian_Body");
            if (glow) ChestFxFactory.SetColor(m, color);
            r.sharedMaterial = OwnMat(m);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (parent != null && type != PrimitiveType.Cube && go.GetComponent<Collider>() != null && parent == _ship) Destroy(go.GetComponent<Collider>());
            return go;
        }

        private GameObject Own(GameObject go) { go.hideFlags = HideFlags.DontSave; _owned.Add(go); return go; }
        private Material OwnMat(Material m) { _owned.Add(m); return m; }
        private Vector3 Rand3() => new Vector3((float)_rng.NextDouble() - 0.5f, (float)_rng.NextDouble() - 0.5f, (float)_rng.NextDouble() - 0.5f) * 2f;
        private Quaternion Random3Rot() => Quaternion.Euler((float)_rng.NextDouble() * 360f, (float)_rng.NextDouble() * 360f, (float)_rng.NextDouble() * 360f);

        /// <summary>Tiếng chốt theo bậc, TC (A4/A11): đúng 1 clip, đúng bậc game đã quyết.</summary>
        private static readonly float[] StingVolume = { 0.7f, 0.7f, 0.85f, 1f, 1f };

        private void Sting(int rank, float volume)
        {
            AudioClip c = stingClips != null && rank >= 0 && rank < stingClips.Length ? stingClips[rank] : null;
            Sfx(c, volume, 1f, ChestAudioRule.StingMax);
        }

        /// <summary>Sự kiện lẻ trước chốt: tối đa 1 tiếng mỗi 0,2 s, ≤ 0,5 (A3).</summary>
        private void Event(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || !ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap)) return;
            Sfx(clip, volume, pitch, ChestAudioRule.EventMax);
        }

        /// <summary>Đuôi sau chốt: ≥ 0,15 s giữa hai tiếng, ≤ 0,4 (A5).</summary>
        private void Tail(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || !ChestAudioRule.TryGate(ref _lastTail, ChestAudioRule.TailGap)) return;
            Sfx(clip, volume, pitch, ChestAudioRule.TailMax);
        }

        /// <summary>Phát một lần qua lõi. <c>sfxVolume</c> được nhân đúng một lần ở đây; <paramref name="cap"/> là trần của loại tiếng (A6).</summary>
        private void Sfx(AudioClip clip, float volume, float pitch, float cap)
        {
            if (_reveal != null && clip != null) _reveal.PlayOneShot(clip, Mathf.Min(volume, cap) * sfxVolume, pitch);
        }

        private void StopEngine()
        {
            _engineFade = 0f;
            if (_engine != null) { _engine.Stop(); _engine.volume = 0f; }
        }

        private void Cleanup()
        {
            StopAllCoroutines();
            if (_hasApplied && _reveal != null)
            {
                Transform r = _reveal.transform;
                if ((r.position - _writtenPos).sqrMagnitude < 1e-10f) r.position -= _appliedPos;
                if (Quaternion.Angle(r.rotation, _writtenRot) < 0.01f) r.rotation = Quaternion.Inverse(_appliedRot) * r.rotation;
            }
            _hasApplied = false;
            if (_engine != null) { _engine.Stop(); Destroy(_engine); }
            _engine = null;
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
            foreach (Rigidbody rb in _cells) if (rb != null) Destroy(rb.gameObject);
            _cells.Clear(); _cellBorn.Clear(); _floaters.Clear();
            DestroyPs(ref _smoke); DestroyPs(ref _flame); DestroyPs(ref _spark);
            _ship = null; _rocket = null;
            _started = _impacted = _locked = _ending = _gravityBack = false;
            _offY = _velY = _hover = _tiltX = _tiltZ = _vtX = _vtZ = 0f;
            _fuel = _playerCells = 0; _tension = 0f; _engineTarget = -1f; _engineVolMul = 1f; _engineFade = 0f; _lastEvent = _lastTail = -999f;
        }

        private static void DestroyPs(ref ParticleSystem ps)
        {
            if (ps == null) return;
            ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
            if (r != null && r.sharedMaterial != null) Destroy(r.sharedMaterial);
            Destroy(ps.gameObject);
            ps = null;
        }
    }
}

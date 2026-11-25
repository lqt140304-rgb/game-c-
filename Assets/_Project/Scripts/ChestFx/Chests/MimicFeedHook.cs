using System;
using System.Collections.Generic;
using System.Reflection;
using ChoDoLa.Interaction;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// "RƯƠNG ĐÓI" — kịch bản mở riêng của Chest_Mimic (làm lại 2026-11-25, xem
    /// docs/superpowers/specs/2026-11-25-kichban-mimic-design.md).
    ///
    /// Bấm mở → rương THỨC DẬY (khịt mũi, bụng réo NGAY lập tức) → đồ lạ rơi lả tả quanh rương → rương há miệng
    /// đòi ăn và KHÔNG chịu mở tiếp cho đến khi được cho ăn đủ (BoxOpenSequence dừng ở giai đoạn "Gỡ niêm" nhờ
    /// waitForPlayer). Người chơi bấm chuột vào một món (hoặc phím F) để ném vào miệng; cũng có thể tự thả bất kỳ
    /// Rigidbody nào vào miệng. Rương có thể nhảy né lần ném đầu, nhai, nuốt, ợ. Ngồi im quá lâu thì nó tự bò/nhảy
    /// tới vồ đồ. Trong lúc vòng màu chạy nó nhai, nhịp tim nhanh dần, một con ruồi vo ve và nó nhảy đớp con ruồi.
    /// Chốt bậc: THẤP → mọi âm tắt phụt, "xì hơi" xịt, rương xẹp lép và nhổ đồ vô dụng ra; CAO → tiếng dâng lên
    /// nổ tung, rương nhảy vọt xoay tròn, đáp xuống đất hất văng mọi vật quanh nó.
    ///
    /// Vật lý thật: thân rương là một Rigidbody "proxy" (hộp va chạm vừa khít rương) — mô hình rương đi theo nó, nên
    /// không phải đổi cây đối tượng của rig. Đồ ăn, ruồi, vật của người chơi đều là Rigidbody thật.
    ///
    /// Bất biến: hook KHÔNG đổi bậc (ChestRevealSequence quyết, hook chỉ đọc bậc ở OnTierLocked). Ngẫu nhiên chỉ để
    /// trang trí (hạt giống <see cref="decorationSeed"/>). Kịch bản chỉ CHÈN thời gian chờ ở giai đoạn cho ăn —
    /// vòng màu, bóng vật phẩm, chốt bậc vẫn do ChestRevealSequence điều khiển.
    /// </summary>
    public class MimicFeedHook : ChestFxHook
    {
        // ------------------------------------------------------------------ Inspector

        [Header("Âm (Audio/Chests/mimic_feed/, MimicFeedSetup tự gán)")]
        public AudioClip wakeClip;        // khịt mũi khi vừa bấm mở
        public AudioClip stomachLoopClip; // bụng réo (vòng lặp), nhanh + cao dần theo độ căng
        public AudioClip heartbeatClip;
        public AudioClip chompClip;
        public AudioClip chewClip;
        public AudioClip gulpClip;
        public AudioClip burpSmallClip;
        public AudioClip burpBigClip;
        public AudioClip riserClip;       // dâng lên trước lúc chốt (giống nhau mọi bậc: giữ bí mật)
        public AudioClip deflateClip;     // bậc thấp: xì hơi xịt
        public AudioClip boomClip;        // bậc cao: bùng nổ
        public AudioClip spitClip;
        public AudioClip buzzLoopClip;
        public AudioClip tossClip;
        public AudioClip hopClip;
        public AudioClip giggleClip;      // lạch cạch răng khi né
        [Range(0f, 1f)] public float masterVolume = 0.9f;

        [Header("Cho ăn")]
        [Tooltip("Giai đoạn BoxOpenSequence mà rương dừng lại đòi ăn (1 = Gỡ niêm, trước vòng màu).")]
        [SerializeField] private int feedStage = 1;
        [SerializeField, Range(1, 4)] private int bitesNeeded = 2;
        [SerializeField, Range(2, 6)] private int foodCount = 4;
        [Tooltip("Giây không ai cho ăn (tính từ lúc rương đòi) trước khi rương tự bò đi vồ đồ.")]
        [SerializeField] private float autoEatAfter = 4.5f;
        [Tooltip("An toàn: chờ tối đa bấy nhiêu giây ở giai đoạn cho ăn rồi tự đi tiếp.")]
        [SerializeField] private float maxFeedWait = 14f;
        [SerializeField, Range(0f, 1f)] private float dodgeChance = 0.5f;
        [SerializeField] private float tossFlightSeconds = 0.55f;

        [Header("Vật lý")]
        [SerializeField] private float bodyMass = 6f;
        [SerializeField] private float foodMass = 0.3f;
        [Tooltip("Xung lực nhảy cú lớn (Huyền thoại), tính theo m/s vận tốc lên.")]
        [SerializeField] private float legendJumpSpeed = 4.2f;
        [SerializeField] private float shoveRadius = 1.6f;
        [SerializeField] private float shoveSpeed = 3.5f;

        [Header("Trang trí")]
        [Tooltip("0 = mỗi lần một khác. Chỉ ảnh hưởng trang trí, KHÔNG ảnh hưởng bậc.")]
        [SerializeField] private int decorationSeed;

        // ------------------------------------------------------------------ State

        private enum Phase { Idle, Feeding, Chewing, Locked }

        private ChestRevealSequence _reveal;
        private BoxOpenSequence _box;
        private System.Random _rng = new System.Random();
        private Phase _phase = Phase.Idle;
        private bool _running;
        private float _t;

        // pose
        private Vector3 _startPos, _basePos, _lastWritten, _startScale;
        private Quaternion _startRot;
        private float _size = 0.5f;
        private Vector3 _center;
        private float _bottomY;
        private Vector3 _front = Vector3.back;

        // physics
        private Rigidbody _body;
        private Vector3 _bodyHome;
        private GameObject _floor;
        private SphereCollider _mouthCol;
        private float _mouthOffUntil;
        private bool _homing;
        private bool _awaitLanding;
        private float _landingArmedAt;
        private readonly List<Collider> _chestColliders = new List<Collider>();

        // food
        private readonly List<MimicFeedFood> _foods = new List<MimicFeedFood>();
        private readonly List<MimicFeedFood> _stomach = new List<MimicFeedFood>();
        private readonly List<Material> _materials = new List<Material>();
        private int _bites;
        private int _tosses;
        private bool _dodged;
        private float _lastFeedTime;
        private bool _crawling;
        private float _nextCrawlHop;
        private MimicFeedFood _crawlTarget;
        private bool _feedDone;

        // gate (BoxOpenSequence.waitForPlayer)
        private int _gateStage = -1;
        private bool _gateOriginal;
        private float _gateReachedAt = -1f;
        private float _gateHold;
        private bool _gateAdvanced;

        // lid (jaw)
        private Transform _lid;
        private Quaternion _lidAtGate;
        private Vector3 _lidAxis = Vector3.right;
        private float _lidSign = 1f;
        private float _jaw;       // degrees, extra opening while begging
        private float _jawSnapUntil;

        // juice
        private float _sq, _sqVel, _sqTarget;
        private float _yaw, _yawFrom, _yawTo, _yawT0, _yawDur;
        private Quaternion _lean = Quaternion.identity;
        private float _tension;
        private float _nextBeat;

        // roulette
        private float _rouletteT0;
        private float _plannedLock = 2f;

        // fly
        private Transform _fly;
        private float _flyPhase;

        // audio
        private AudioSource _loopSrc, _riserSrc, _fxSrc, _buzzSrc;

        // timers
        private struct Timer { public float at; public Action act; }
        private readonly List<Timer> _timers = new List<Timer>();

        // ------------------------------------------------------------------ Unity

        private void Awake()
        {
            _reveal = GetComponent<ChestRevealSequence>();
            _box = GetComponent<BoxOpenSequence>();
            if (_box != null) _box.onStageReached.AddListener(OnBoxStage);
            _lid = transform.Find("Lid");
            ReadLidHinge();
        }

        private void OnDestroy()
        {
            if (_box != null) _box.onStageReached.RemoveListener(OnBoxStage);
            Cleanup(false);
        }

        private void Update()
        {
            if (_reveal == null) return;
            if (_reveal.IsPlaying && !_running) Begin();
            if (!_running) return;

            float dt = Time.deltaTime;
            _t += dt;
            RunTimers();
            UpdateTensionAudio(dt);
            if (_phase == Phase.Feeding) UpdateFeeding();
            UpdateJaw(dt);
            UpdateFly(dt);
            UpdateSquash(dt);
        }

        private void FixedUpdate()
        {
            if (!_running || _body == null) return;

            if (_homing)
            {
                Vector3 p = Vector3.Lerp(_body.position, _bodyHome, 0.12f);
                _body.MovePosition(p);
                if ((p - _bodyHome).sqrMagnitude < 1e-6f) _body.MovePosition(_bodyHome);
                return;
            }

            // Leash: never wander more than ~1.2 chest sizes from home (soft spring, horizontal only).
            Vector3 off = _body.position - _bodyHome;
            off.y = 0f;
            float leash = _size * 1.2f;
            if (off.magnitude > leash) _body.AddForce(-off.normalized * (off.magnitude - leash) * 60f * _body.mass);

            if (_crawling && _crawlTarget != null && !_crawlTarget.eaten && _t >= _nextCrawlHop && Grounded())
            {
                Vector3 to = _crawlTarget.transform.position - _body.position;
                to.y = 0f;
                Vector3 hop = to.normalized * Mathf.Min(1.6f, to.magnitude * 2.2f) + Vector3.up * 1.6f;
                SetBodyVelocity(hop);
                Play(hopClip, 0.45f, 0.9f + Rand(0.25f));
                Kick(0.12f);
                _nextCrawlHop = _t + 0.5f;
            }

            if (_awaitLanding && _t > _landingArmedAt && Grounded() && BodyVelocity().y <= 0.05f)
            {
                _awaitLanding = false;
                Land();
            }
        }

        private void LateUpdate()
        {
            if (!_running) return;
            // BoxOpenSequence writes localPosition (its shake) while a stage moves or holds; when it does, take that
            // as the new base. When it does not write (waiting for the player), keep the old base.
            Vector3 cur = transform.position;
            if ((cur - _lastWritten).sqrMagnitude > 1e-10f) _basePos = cur;

            Vector3 delta = _body != null ? _body.position - _bodyHome : Vector3.zero;
            float yaw = _yaw;
            if (_yawDur > 0f)
            {
                float u = Mathf.Clamp01((_t - _yawT0) / _yawDur);
                yaw = Mathf.Lerp(_yawFrom, _yawTo, 1f - (1f - u) * (1f - u) * (1f - u));
                _yaw = yaw;
                if (u >= 1f) { _yawDur = 0f; _yaw = Mathf.Repeat(_yaw, 360f); }
            }

            Vector3 v = BodyVelocity();
            v.y = 0f;
            Quaternion leanTarget = v.sqrMagnitude > 1e-4f
                ? Quaternion.AngleAxis(Mathf.Min(16f, v.magnitude * 9f), Vector3.Cross(Vector3.up, v.normalized))
                : Quaternion.identity;
            _lean = Quaternion.Slerp(_lean, leanTarget, 1f - Mathf.Exp(-10f * Time.deltaTime));

            Quaternion rot = _lean * Quaternion.AngleAxis(yaw, Vector3.up);
            transform.position = _bodyHome + delta + rot * (_basePos - _bodyHome);
            transform.rotation = rot * _startRot;
            float s = _sq;
            transform.localScale = Vector3.Scale(_startScale, new Vector3(1f + 0.5f * s, 1f - s, 1f + 0.5f * s));
            _lastWritten = transform.position;
        }

        // ------------------------------------------------------------------ ChestFxHook

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            if (!_running) Begin();
            ForceFinishFeeding();
            _phase = Phase.Chewing;
            _rouletteT0 = _t;
            _plannedLock = PlannedRouletteSeconds();
            const float riserLead = 2.6f;
            After(Mathf.Max(0f, _plannedLock - riserLead), StartRiser);
            SpawnFly();
            After(_plannedLock * (0.35f + Rand(0.15f)), SnapAtFly);
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (!_running) return;
            Kick(0.05f);
            if (_rng.NextDouble() < 0.35) Play(chewClip, 0.35f, 0.85f + Rand(0.35f));
            if (_rng.NextDouble() < 0.12 && Grounded()) { SetBodyVelocity(Vector3.up * 1.1f + Side() * Rand(0.6f, true)); }
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (!_running) return;
            _phase = Phase.Locked;
            RemoveFly();
            float rank = tier.Rank01();
            Vector3 mouth = MouthPos();

            if (rank <= 0.26f)
            {
                // THẤP: mọi thứ tắt phụt — một khoảng lặng ngắn — rồi tiếng xịt/ợ nhỏ. Rương xẹp lép.
                StopSrc(_loopSrc);
                StopSrc(_riserSrc);
                if (tier == ChestTier.Rac)
                {
                    After(0.14f, () => { Play(deflateClip, 1f, 1f); _sqTarget = 0.3f; Kick(-0.2f); });
                    After(0.75f, () => SpitStomach(true, 0.9f));
                    After(1.6f, () => { Play(burpSmallClip, 0.5f, 1.6f); Kick(0.15f); });   // nấc cụt
                    After(2.6f, () => _sqTarget = 0f);
                }
                else
                {
                    After(0.14f, () => { Play(burpSmallClip, 0.9f, 1f); Kick(0.18f); });
                    After(0.5f, () => SpitStomach(false, 0.6f));
                }
                After(0.3f, () => { if (Grounded()) SetBodyVelocity(Vector3.up * 0.7f); });
            }
            else
            {
                // CAO: tiếng dâng bùng nổ, độ mạnh theo bậc.
                FadeSrc(_loopSrc, 0.4f);
                StopSrc(_riserSrc);
                Play(boomClip, Mathf.Lerp(0.45f, 1f, (rank - 0.5f) * 2f), Mathf.Lerp(1.15f, 0.9f, rank));
                Play(burpBigClip, 0.7f, Mathf.Lerp(1.25f, 0.85f, rank));
                Kick(-0.25f);
                float jump = Mathf.Lerp(1.6f, legendJumpSpeed, (rank - 0.5f) * 2f);
                After(0.05f, () =>
                {
                    _homing = false;
                    if (_body != null) _body.isKinematic = false;
                    SetBodyVelocity(Vector3.up * jump);
                    SpinBy(tier == ChestTier.HuyenThoai ? 720f : 360f, jump * 0.38f);
                    _awaitLanding = true;
                    _landingArmedAt = _t + 0.2f;
                });
                if (rank >= 0.74f) Shove(mouth, shoveRadius * 0.6f, shoveSpeed * 0.6f);
                After(0.6f, () => SpitStomach(false, 0.7f));
            }

            After(2.6f, () => { _homing = true; if (_body != null) _body.isKinematic = true; });
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            Cleanup(true);
        }

        // ------------------------------------------------------------------ Begin / gate

        private void Begin()
        {
            Cleanup(false);
            _running = true;
            _t = 0f;
            _phase = Phase.Feeding;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : Environment.TickCount);
            _bites = _tosses = 0;
            _dodged = _feedDone = _crawling = _homing = _awaitLanding = _gateAdvanced = false;
            _gateReachedAt = -1f;
            _tension = 0.1f;
            _sq = _sqVel = _sqTarget = 0f;
            _yaw = 0f; _yawDur = 0f;
            _lean = Quaternion.identity;

            _startPos = _basePos = _lastWritten = transform.position;
            _startRot = transform.rotation;
            _startScale = transform.localScale;
            if (_box != null) _front = transform.TransformDirection(_box.FrontDirection).normalized;
            _front.y = 0f;
            if (_front.sqrMagnitude < 1e-4f) _front = Vector3.back;
            _front.Normalize();

            Measure();
            BuildAudio();
            BuildPhysics();
            ArmGate();

            // ÂM BẮT ĐẦU NGAY KHI BẤM MỞ: khịt mũi + bụng réo + tim đập chậm.
            Play(wakeClip, 0.9f, 0.95f + Rand(0.1f));
            if (_loopSrc != null && stomachLoopClip != null)
            {
                _loopSrc.clip = stomachLoopClip;
                _loopSrc.loop = true;
                _loopSrc.volume = 0.25f * masterVolume;
                _loopSrc.pitch = 0.8f;
                _loopSrc.Play();
            }
            _nextBeat = 0.35f;
            _lastFeedTime = 0f;

            // Thức dậy: giật mình nhảy lên một cái.
            After(0.12f, () => { SetBodyVelocity(Vector3.up * 1.5f); Kick(-0.15f); });
            After(0.5f, () => { Play(hopClip, 0.6f, 1f); Kick(0.22f); });

            // Đồ lạ rơi lả tả xuống quanh rương (tháo dần từ trên cao).
            for (int i = 0; i < foodCount; i++)
            {
                int k = i;
                After(0.55f + 0.28f * i, () => SpawnFood(k));
            }
        }

        private void ArmGate()
        {
            if (_box == null) return;
            IList<BoxOpenSequence.Stage> stages = _box.Stages;
            if (feedStage < 0 || feedStage >= stages.Count) return;
            _gateStage = feedStage;
            _gateOriginal = stages[feedStage].waitForPlayer;
            _gateHold = Mathf.Max(0f, stages[feedStage].hold);
            stages[feedStage].waitForPlayer = true;
        }

        private void RestoreGate()
        {
            if (_box == null || _gateStage < 0) return;
            IList<BoxOpenSequence.Stage> stages = _box.Stages;
            if (_gateStage < stages.Count) stages[_gateStage].waitForPlayer = _gateOriginal;
            _gateStage = -1;
        }

        private void OnBoxStage(int stage)
        {
            if (!_running || stage != _gateStage) return;
            _gateReachedAt = _t;
            _lastFeedTime = Mathf.Max(_lastFeedTime, _t);
            if (_lid != null) _lidAtGate = _lid.localRotation;
            if (_feedDone) After(_gateHold + 0.05f, AdvanceGate);
        }

        private bool GateWaiting => _gateReachedAt >= 0f && !_gateAdvanced && _t >= _gateReachedAt + _gateHold;

        private void AdvanceGate()
        {
            if (_gateAdvanced || _box == null) return;
            if (_gateReachedAt < 0f) return;   // not reached yet: OnBoxStage advances when it gets there
            _gateAdvanced = true;
            _jaw = 0f;
            if (_lid != null) _lid.localRotation = _lidAtGate;
            RestoreGate();
            _box.Advance();
        }

        // ------------------------------------------------------------------ Feeding

        private void UpdateFeeding()
        {
            if (_feedDone) return;

            if (TryGetPointer(out bool pressed, out Vector2 screen, out bool nearestKey))
            {
                MimicFeedFood pick = null;
                if (pressed) pick = PickFood(screen);
                if (pick == null && nearestKey) pick = NearestFood(MouthPos());
                if (pick != null) Toss(pick);
            }

            if (GateWaiting)
            {
                float waited = _t - Mathf.Max(_lastFeedTime, _gateReachedAt + _gateHold);
                if (!_crawling && waited > autoEatAfter)
                {
                    _crawlTarget = NearestFood(MouthPos());
                    if (_crawlTarget != null)
                    {
                        _crawling = true;
                        _nextCrawlHop = _t;
                        Play(giggleClip, 0.5f, 0.8f);
                    }
                }
                if (_crawling && _crawlTarget != null && !_crawlTarget.eaten &&
                    Vector3.Distance(_crawlTarget.transform.position, MouthPos()) < _size * 0.7f)
                {
                    Eat(_crawlTarget);
                }
                if (_t - (_gateReachedAt + _gateHold) > maxFeedWait) ForceFinishFeeding();
            }
        }

        private void Toss(MimicFeedFood food)
        {
            if (food == null || food.eaten) return;
            _tosses++;
            _lastFeedTime = _t;
            Play(tossClip, 0.6f, 0.9f + Rand(0.3f));
            food.TossTo(MouthPos() + Vector3.up * (_size * 0.05f), tossFlightSeconds);
            _tension = Mathf.Min(0.45f, _tension + 0.04f);

            // Lần ném đầu: rương tinh nghịch nhảy né (trang trí; món ăn vẫn còn trên đất để ném lại).
            if (!_dodged && _rng.NextDouble() < dodgeChance)
            {
                _dodged = true;
                After(tossFlightSeconds * 0.4f, () =>
                {
                    _mouthOffUntil = _t + 0.7f;
                    SetBodyVelocity(Side() * (_rng.NextDouble() < 0.5 ? -1.8f : 1.8f) + Vector3.up * 1.7f);
                    Play(giggleClip, 0.8f, 1.2f);
                    Kick(-0.12f);
                });
            }
        }

        /// <summary>MimicFeedMouth gọi khi có collider lọt vào trigger miệng.</summary>
        public void OnMouthTouched(Collider other)
        {
            if (!_running || _phase == Phase.Locked || _t < _mouthOffUntil) return;
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null || rb == _body || rb.isKinematic) return;   // kinematic = đang cầm trên tay: không giật
            if (_fly != null && rb.transform == _fly) return;
            MimicFeedFood food = rb.GetComponent<MimicFeedFood>();
            if (food == null)
            {
                food = rb.gameObject.AddComponent<MimicFeedFood>();
                food.body = rb;
                food.spawnedByHook = false;
            }
            if (food.eaten) return;
            Eat(food);
        }

        private void Eat(MimicFeedFood food)
        {
            food.Swallow();
            _foods.Remove(food);
            _stomach.Add(food);
            _lastFeedTime = _t;
            _crawling = false;
            _crawlTarget = null;
            _bites++;
            _tension = Mathf.Min(_phase == Phase.Feeding ? 0.48f : 0.95f, _tension + 0.12f);

            Play(chompClip, 1f, 0.95f + Rand(0.1f));
            _jawSnapUntil = _t + 0.12f;
            Kick(0.3f);
            for (int i = 0; i < 3; i++)
            {
                float at = 0.28f + 0.22f * i;
                After(at, () => { Play(chewClip, 0.6f, 0.9f + Rand(0.25f)); Kick(0.12f); });
            }
            After(1.05f, () => { Play(gulpClip, 0.8f, 1f); Kick(-0.08f); });

            if (_phase == Phase.Feeding && _bites >= bitesNeeded && !_feedDone)
            {
                After(1.4f, () =>
                {
                    Play(burpSmallClip, 0.6f, 1.1f);
                    FinishFeeding();
                });
            }
        }

        private void FinishFeeding()
        {
            if (_feedDone) return;
            _feedDone = true;
            _crawling = false;
            if (GateWaiting || _gateReachedAt >= 0f) After(Mathf.Max(0f, _gateReachedAt + _gateHold - _t) + 0.05f, AdvanceGate);
        }

        private void ForceFinishFeeding()
        {
            if (!_feedDone) FinishFeeding();
            if (_gateReachedAt >= 0f) AdvanceGate();
            else RestoreGate();   // roulette started without passing the gate (custom stages): just don't block
        }

        private MimicFeedFood PickFood(Vector2 screen)
        {
            Camera cam = Camera.main;
            if (cam == null) return null;
            if (Cursor.lockState == CursorLockMode.Locked) screen = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Ray ray = cam.ScreenPointToRay(new Vector3(screen.x, screen.y, 0f));
            // Bấm trúng món nào gần tia nhất (dễ bấm: bán kính rộng, không cần trúng khít collider).
            MimicFeedFood best = null;
            float bestScore = float.MaxValue;
            foreach (MimicFeedFood f in _foods)
            {
                if (f == null || f.eaten) continue;
                Vector3 p = f.transform.position - ray.origin;
                float along = Vector3.Dot(p, ray.direction);
                if (along < 0f) continue;
                float off = (p - ray.direction * along).magnitude;
                if (off < _size * 0.35f && off < bestScore) { best = f; bestScore = off; }
            }
            return best;
        }

        private MimicFeedFood NearestFood(Vector3 from)
        {
            MimicFeedFood best = null;
            float bestD = float.MaxValue;
            foreach (MimicFeedFood f in _foods)
            {
                if (f == null || f.eaten) continue;
                float d = (f.transform.position - from).sqrMagnitude;
                if (d < bestD) { bestD = d; best = f; }
            }
            return best;
        }

        private static bool TryGetPointer(out bool pressed, out Vector2 screen, out bool nearestKey)
        {
            pressed = false;
            nearestKey = false;
            screen = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            Mouse m = Mouse.current;
            if (m != null && m.leftButton.wasPressedThisFrame) { pressed = true; screen = m.position.ReadValue(); }
            Keyboard k = Keyboard.current;
            if (k != null && k.fKey.wasPressedThisFrame) nearestKey = true;
#else
            if (Input.GetMouseButtonDown(0)) { pressed = true; screen = Input.mousePosition; }
            if (Input.GetKeyDown(KeyCode.F)) nearestKey = true;
#endif
            return pressed || nearestKey;
        }

        // ------------------------------------------------------------------ Food / fly objects

        private void SpawnFood(int index)
        {
            if (!_running) return;
            // Đồ lạ ngoài chợ: dép, lon, khúc xương, quả táo... (hình khối đơn giản, màu riêng)
            PrimitiveType[] shapes = { PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Capsule, PrimitiveType.Sphere };
            Vector3[] scales = { new Vector3(0.5f, 0.12f, 1f), new Vector3(0.45f, 0.4f, 0.45f), new Vector3(0.22f, 0.5f, 0.22f), new Vector3(0.5f, 0.5f, 0.5f) };
            Color[] colors = { new Color(0.45f, 0.3f, 0.18f), new Color(0.85f, 0.15f, 0.12f), new Color(0.95f, 0.93f, 0.85f), new Color(0.4f, 0.8f, 0.2f) };
            string[] names = { "Dep", "Lon", "Xuong", "Tao" };
            int s = (index + _rng.Next(4)) % shapes.Length;

            GameObject go = GameObject.CreatePrimitive(shapes[s]);
            go.name = "MimicFeed_" + names[s];
            go.transform.localScale = scales[s] * (_size * 0.32f);
            float ang = Mathf.Lerp(-70f, 70f, foodCount <= 1 ? 0.5f : index / (float)(foodCount - 1)) + Rand(12f, true);
            Vector3 dir = Quaternion.AngleAxis(ang, Vector3.up) * _front;
            go.transform.position = new Vector3(_center.x, _bottomY, _center.z) + dir * (_size * (1.1f + Rand(0.3f))) + Vector3.up * (_size * 2.2f);
            go.transform.rotation = Quaternion.Euler(Rand(180f, true), Rand(180f, true), Rand(180f, true));
            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                Material mat = r.material;   // instance: owned, destroyed on cleanup
                mat.color = colors[s];
                _materials.Add(mat);
            }
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = foodMass;
            MimicFeedFood food = go.AddComponent<MimicFeedFood>();
            food.body = rb;
            food.spawnedByHook = true;
            IgnoreChest(go.GetComponent<Collider>());
            _foods.Add(food);
            Play(tossClip, 0.25f, 1.4f + Rand(0.3f));
        }

        private void SpawnFly()
        {
            if (_fly != null) return;
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "MimicFeed_Ruoi";
            Collider c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
            go.transform.localScale = Vector3.one * (_size * 0.06f);
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) { Material m = r.material; m.color = new Color(0.08f, 0.08f, 0.1f); _materials.Add(m); }
            _fly = go.transform;
            _flyPhase = Rand(6.28f);
            if (_buzzSrc != null && buzzLoopClip != null)
            {
                _buzzSrc.clip = buzzLoopClip;
                _buzzSrc.loop = true;
                _buzzSrc.volume = 0.35f * masterVolume;
                _buzzSrc.Play();
            }
        }

        private void UpdateFly(float dt)
        {
            if (_fly == null) return;
            _flyPhase += dt * 3.1f;
            Vector3 m = MouthPos();
            float r = _size * (0.8f + 0.15f * Mathf.Sin(_flyPhase * 2.3f));
            _fly.position = m + new Vector3(Mathf.Cos(_flyPhase) * r, _size * (0.45f + 0.2f * Mathf.Sin(_flyPhase * 3.7f)), Mathf.Sin(_flyPhase) * r);
            if (_buzzSrc != null) _buzzSrc.pitch = 1f + 0.08f * Mathf.Sin(_flyPhase * 5f);
        }

        private void SnapAtFly()
        {
            if (_fly == null || _phase != Phase.Chewing) return;
            Vector3 to = _fly.position - _body.position;
            to.y = 0f;
            SetBodyVelocity(to.normalized * Mathf.Min(2.2f, to.magnitude * 3f) + Vector3.up * 2.4f);
            Play(giggleClip, 0.6f, 1.4f);
            After(0.32f, () =>
            {
                RemoveFly();
                Play(chompClip, 1f, 1.2f);
                _jawSnapUntil = _t + 0.12f;
                Kick(0.3f);
                After(0.4f, () => Play(gulpClip, 0.6f, 1.4f));
            });
        }

        private void RemoveFly()
        {
            StopSrc(_buzzSrc);
            if (_fly != null) Destroy(_fly.gameObject);
            _fly = null;
        }

        private void SpitStomach(bool includeJunk, float strength)
        {
            Vector3 mouth = MouthPos() + Vector3.up * (_size * 0.15f);
            bool any = false;
            for (int i = _stomach.Count - 1; i >= 0; i--)
            {
                MimicFeedFood f = _stomach[i];
                if (f == null) { _stomach.RemoveAt(i); continue; }
                if (f.spawnedByHook && !includeJunk) continue;   // tiêu hoá luôn
                Vector3 dir = Quaternion.AngleAxis(Rand(40f, true), Vector3.up) * _front;
                f.SpitOut(mouth, (dir * 2.2f + Vector3.up * 2.6f) * strength);
                _stomach.RemoveAt(i);
                if (f.spawnedByHook) _foods.Add(f);
                any = true;
            }
            if (any)
            {
                Play(spitClip, 0.9f, 1f);
                Kick(-0.2f);
                if (_body != null && !_body.isKinematic) SetBodyVelocity(BodyVelocity() - _front * 0.8f);   // giật lùi do nhổ
            }
        }

        private void Land()
        {
            Play(hopClip, 1f, 0.7f);
            Play(boomClip, 0.5f, 1.5f);
            Kick(0.4f);
            Shove(_body.position, shoveRadius, shoveSpeed);
        }

        private void Shove(Vector3 center, float radius, float speed)
        {
            Collider[] hits = Physics.OverlapSphere(center, radius * Mathf.Max(0.5f, _size * 2f));
            HashSet<Rigidbody> done = new HashSet<Rigidbody>();
            foreach (Collider c in hits)
            {
                Rigidbody rb = c != null ? c.attachedRigidbody : null;
                if (rb == null || rb == _body || rb.isKinematic || !done.Add(rb)) continue;
                Vector3 d = rb.position - center;
                float k = 1f - Mathf.Clamp01(d.magnitude / radius);
                d.y = 0f;
                Vector3 push = (d.sqrMagnitude > 1e-4f ? d.normalized : _front) * speed * (0.4f + k) + Vector3.up * speed * 0.6f * (0.4f + k);
                rb.AddForce(push * rb.mass, ForceMode.Impulse);
            }
        }

        // ------------------------------------------------------------------ Physics proxy

        private void Measure()
        {
            Renderer[] rs = GetComponentsInChildren<Renderer>();
            bool has = false;
            Bounds b = new Bounds(transform.position, Vector3.one * 0.5f);
            foreach (Renderer r in rs)
            {
                if (r == null || r is ParticleSystemRenderer) continue;
                if (!has) { b = r.bounds; has = true; }
                else b.Encapsulate(r.bounds);
            }
            _center = b.center;
            _bottomY = b.min.y;
            _size = Mathf.Max(0.15f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)));
            _bodySize = b.size;
        }

        private Vector3 _bodySize = Vector3.one * 0.5f;

        private void BuildPhysics()
        {
            _chestColliders.Clear();
            _chestColliders.AddRange(GetComponentsInChildren<Collider>());

            // Sàn vô hình ngay dưới đáy rương (để rương/đồ ăn có chỗ rơi xuống dù cảnh không có sàn).
            _floor = new GameObject("MimicFeed_San");
            _floor.transform.position = new Vector3(_center.x, _bottomY - 0.05f, _center.z);
            BoxCollider fc = _floor.AddComponent<BoxCollider>();
            fc.size = new Vector3(_size * 12f, 0.1f, _size * 12f);
            IgnoreChest(fc);

            GameObject bodyGo = new GameObject("MimicFeed_Than");
            bodyGo.transform.position = _center;
            _bodyHome = _center;
            BoxCollider bc = bodyGo.AddComponent<BoxCollider>();
            bc.size = _bodySize * 0.98f;
            IgnoreChest(bc);
            _body = bodyGo.AddComponent<Rigidbody>();
            _body.mass = bodyMass;
            _body.constraints = RigidbodyConstraints.FreezeRotation;   // xoay/nghiêng là phần trang trí (LateUpdate)
            _body.interpolation = RigidbodyInterpolation.Interpolate;

            GameObject mouthGo = new GameObject("MimicFeed_Mieng");
            mouthGo.transform.SetParent(bodyGo.transform, false);
            mouthGo.transform.position = _reveal != null ? _reveal.MouthPosition : _center + Vector3.up * (_bodySize.y * 0.5f);
            _mouthCol = mouthGo.AddComponent<SphereCollider>();
            _mouthCol.isTrigger = true;
            _mouthCol.radius = _size * 0.45f;
            IgnoreChest(_mouthCol);
            MimicFeedMouth mouth = mouthGo.AddComponent<MimicFeedMouth>();
            mouth.owner = this;
        }

        private void IgnoreChest(Collider c)
        {
            if (c == null) return;
            foreach (Collider cc in _chestColliders) if (cc != null && cc != c) Physics.IgnoreCollision(c, cc, true);
        }

        private bool Grounded()
        {
            if (_body == null) return false;
            return _body.position.y - _bodyHome.y < 0.02f && Mathf.Abs(BodyVelocity().y) < 0.3f;
        }

        private Vector3 BodyVelocity()
        {
            if (_body == null) return Vector3.zero;
#if UNITY_6000_0_OR_NEWER
            return _body.linearVelocity;
#else
            return _body.velocity;
#endif
        }

        private void SetBodyVelocity(Vector3 v)
        {
            if (_body == null || _body.isKinematic) return;
#if UNITY_6000_0_OR_NEWER
            _body.linearVelocity = v;
#else
            _body.velocity = v;
#endif
            Play(hopClip, 0.3f, 1.3f + Rand(0.2f));
        }

        private Vector3 MouthPos() => _mouthCol != null ? _mouthCol.transform.position : (_reveal != null ? _reveal.MouthPosition : transform.position);

        private Vector3 Side() => Vector3.Cross(Vector3.up, _front).normalized;

        // ------------------------------------------------------------------ Jaw, squash, spin

        private void ReadLidHinge()
        {
            if (_box == null) return;
            FieldInfo axis = typeof(BoxOpenSequence).GetField("lidHingeAxis", BindingFlags.NonPublic | BindingFlags.Instance);
            if (axis != null) _lidAxis = (Vector3)axis.GetValue(_box);
            _lidSign = _box.LidOpenSign;
        }

        private void UpdateJaw(float dt)
        {
            // Chỉ điều khiển nắp khi BoxOpenSequence đang đứng chờ (nó không ghi nắp lúc đó).
            if (_lid == null || !GateWaiting) return;
            float target;
            if (_t < _jawSnapUntil) target = 0f;                                         // ngoạm!
            else if (_feedDone) target = 4f * Mathf.Abs(Mathf.Sin(_t * 9f));            // nhai nhóp nhép
            else target = 22f + 10f * Mathf.Sin(_t * 5.5f) + (_crawling ? 8f : 0f);     // há miệng đòi ăn
            _jaw = Mathf.Lerp(_jaw, target, 1f - Mathf.Exp(-(_t < _jawSnapUntil ? 40f : 9f) * dt));
            _lid.localRotation = _lidAtGate * Quaternion.AngleAxis(_lidSign * _jaw, _lidAxis);
        }

        private void Kick(float amount) => _sqVel += amount * 12f;

        private void UpdateSquash(float dt)
        {
            const float k = 220f, damp = 14f;
            _sqVel += (-(_sq - _sqTarget) * k - _sqVel * damp) * dt;
            _sq = Mathf.Clamp(_sq + _sqVel * dt, -0.35f, 0.4f);
        }

        private void SpinBy(float degrees, float seconds)
        {
            _yawFrom = _yaw;
            _yawTo = _yaw + degrees * (_rng.NextDouble() < 0.5 ? 1f : -1f);
            _yawT0 = _t;
            _yawDur = Mathf.Max(0.2f, seconds);
        }

        // ------------------------------------------------------------------ Audio

        private void BuildAudio()
        {
            GameObject go = new GameObject("MimicFeed_Am");
            go.transform.SetParent(transform, false);
            _loopSrc = NewSource(go);
            _riserSrc = NewSource(go);
            _fxSrc = NewSource(go);
            _buzzSrc = NewSource(go);
        }

        private static AudioSource NewSource(GameObject go)
        {
            AudioSource s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0.5f;
            return s;
        }

        private void UpdateTensionAudio(float dt)
        {
            if (_phase == Phase.Chewing)
            {
                float u = Mathf.Clamp01((_t - _rouletteT0) / Mathf.Max(0.5f, _plannedLock));
                _tension = Mathf.Max(_tension, 0.5f + 0.45f * u);
            }
            if (_phase != Phase.Locked)
            {
                if (_loopSrc != null && _loopSrc.isPlaying)
                {
                    _loopSrc.pitch = Mathf.Lerp(_loopSrc.pitch, 0.8f + 0.55f * _tension, 1f - Mathf.Exp(-3f * dt));
                    _loopSrc.volume = Mathf.Lerp(_loopSrc.volume, (0.25f + 0.45f * _tension) * masterVolume, 1f - Mathf.Exp(-3f * dt));
                }
                if (_t >= _nextBeat)
                {
                    Play(heartbeatClip, 0.35f + 0.55f * _tension, 0.9f + 0.25f * _tension);
                    Kick(0.03f + 0.05f * _tension);
                    _nextBeat = _t + Mathf.Lerp(0.95f, 0.26f, _tension);
                }
            }
        }

        private void StartRiser()
        {
            if (_phase != Phase.Chewing || _riserSrc == null || riserClip == null) return;
            _riserSrc.clip = riserClip;
            _riserSrc.loop = false;
            _riserSrc.volume = 0.85f * masterVolume;
            _riserSrc.pitch = 1f;
            _riserSrc.Play();
        }

        private float PlannedRouletteSeconds()
        {
            // Vòng màu bắt đầu ở giai đoạn "He nap" (2) và chốt khi tới "Bat nap" (3): hold(2) + travel(3).
            if (_box == null || _box.Stages.Count < 4) return 2.5f;
            return Mathf.Max(0.5f, _box.Stages[2].hold + _box.Stages[3].travel);
        }

        private void Play(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || _fxSrc == null) return;
            _fxSrc.pitch = pitch;
            _fxSrc.PlayOneShot(clip, volume * masterVolume);
        }

        private static void StopSrc(AudioSource s)
        {
            if (s != null) s.Stop();
        }

        private void FadeSrc(AudioSource s, float seconds)
        {
            if (s == null || !s.isPlaying) return;
            float v0 = s.volume;
            int steps = 8;
            for (int i = 1; i <= steps; i++)
            {
                float k = 1f - i / (float)steps;
                After(seconds * i / steps, () => { if (s != null) s.volume = v0 * k; if (k <= 0f && s != null) s.Stop(); });
            }
        }

        // ------------------------------------------------------------------ Timers / rng

        private void After(float seconds, Action act) => _timers.Add(new Timer { at = _t + seconds, act = act });

        private void RunTimers()
        {
            for (int i = 0; i < _timers.Count; i++)
            {
                if (_timers[i].at > _t) continue;
                Action a = _timers[i].act;
                _timers.RemoveAt(i);
                i--;
                if (!_running) return;
                a?.Invoke();
            }
        }

        private float Rand(float range, bool signed = false)
        {
            float r = (float)_rng.NextDouble();
            return signed ? (r * 2f - 1f) * range : r * range;
        }

        // ------------------------------------------------------------------ Cleanup

        private void Cleanup(bool restorePose)
        {
            bool wasRunning = _running;
            _running = false;
            _phase = Phase.Idle;
            _timers.Clear();
            RestoreGate();

            // Trả lại nguyên vẹn mọi vật của người chơi đang nằm trong bụng.
            foreach (MimicFeedFood f in _stomach)
            {
                if (f == null) continue;
                if (f.spawnedByHook) { Destroy(f.gameObject); continue; }
                f.SpitOut(MouthPos() + Vector3.up * (_size * 0.3f), Vector3.zero);
                Destroy(f);
            }
            _stomach.Clear();
            foreach (MimicFeedFood f in _foods) if (f != null && f.spawnedByHook) Destroy(f.gameObject);
            _foods.Clear();

            RemoveFly();
            if (_body != null) Destroy(_body.gameObject);
            if (_floor != null) Destroy(_floor);
            _body = null; _floor = null; _mouthCol = null;
            foreach (Material m in _materials) if (m != null) Destroy(m);
            _materials.Clear();

            if (_loopSrc != null) Destroy(_loopSrc.gameObject);   // all four sources share one child
            _loopSrc = _riserSrc = _fxSrc = _buzzSrc = null;

            if (wasRunning && restorePose)
            {
                transform.position = _startPos;
                transform.rotation = _startRot;
                transform.localScale = _startScale;
            }
        }
    }
}

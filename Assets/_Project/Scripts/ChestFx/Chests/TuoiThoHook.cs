using System.Collections.Generic;
using ChoDoLa.Interaction;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TuoiTho — "CỖ MÁY BI VE" (Rube Goldberg tuổi thơ), đã thu gọn theo kế hoạch 2026-11-25 còn 3 khâu.
    /// Bấm mở là cỗ máy chạy NGAY: búng bi → bi lăn xuống thước kẻ dốc → đổ 3 quân domino → quân cuối đổ vào chốt
    /// ná → ná dây thun bắn viên bi thứ hai vọt lên nắp rương → CẢ RƯƠNG bật nảy. Đến lúc chốt bậc mới lộ kết quả:
    /// bậc cao thì rương nhảy xoay vòng, máy bay giấy lượn tới nhào lộn thả hoa giấy; Rác thì rương xẹp "xịt",
    /// máy bay giấy chòng chành, rơi cắm đầu, vò thành cục giấy cộc vào rương rồi lăn đi.
    /// Tương tác phụ: trong lúc máy chạy, bấm chuột/chạm (hoặc phím B) để thả thêm bi ve (tối đa 4) từ trên cao
    /// xuống chỗ trỏ — bi thật (Rigidbody) có thể đổ domino sớm hay rơi trúng rương (mỗi viên trúng = một nốt
    /// mộc cầm cao dần + rương nảy nhẹ + thêm hoa giấy lúc kết). Domino/ná có "cú hích dự phòng" theo giờ
    /// để chuỗi luôn tới đích đúng nhịp dù vật lý lệch.
    /// Âm (quy tắc A1–A12): chỉ file ElevenLabs trong Audio/Chests/tuoi_tho/. Trước chốt 2 lớp: nền lăn (vòng) + nhịp
    /// mộc cầm. Chốt: Rác = dừng hết → lặng 0,2 s → 1 tiếng xịt; bậc khác = 1 tiếng chốt theo bậc (stingClips).
    /// Bất biến: chỉ trang trí. Không đọc/đổi bậc trước OnTierLocked; ngẫu nhiên lấy từ hạt giống riêng.
    /// Gắn component này lên gốc Chest_TuoiTho (cùng chỗ ChestRevealSequence). Âm gán bởi TuoiThoFxSetup.
    /// </summary>
    public class TuoiThoHook : ChestFxHook
    {
        [Header("Âm (Audio/Chests/tuoi_tho/, ElevenLabs)")]
        [Tooltip("Nền: tiếng bi lăn rì rì, đặt loop (sfx_tuoitho_roll_loop).")]
        public AudioClip rollClip;
        [Tooltip("Nhịp: một nốt mộc cầm (sfx_tuoitho_xylo), hook đổi cao độ theo ngũ cung.")]
        public AudioClip xyloClip;
        public AudioClip flickClip;
        public AudioClip dominoClip;
        public AudioClip bandClip;
        public AudioClip bonkClip;
        [Tooltip("Tiếng bi ve thả thêm (sfx_tuoitho_marbles).")]
        public AudioClip marblesClip;
        public AudioClip planeClip;
        public AudioClip crumpleClip;
        [Tooltip("TC: tiếng chốt theo bậc, 5 ô Rác→Huyền thoại (sfx_tuoitho_lock_*).")]
        public AudioClip[] stingClips = new AudioClip[5];
        [Range(0f, 1f)] public float volume = 0.8f;

        [Header("Nhịp")]
        [Tooltip("Stage của BoxOpenSequence mà bậc được chốt (khớp lockStage của ChestRevealSequence, mặc định 3).")]
        [SerializeField] private int lockStageGuess = 3;
        [Tooltip("Giây dư cộng thêm (đoạn chìa khoá bay vào trước các stage, nếu có).")]
        [SerializeField] private float extraLeadSeconds;
        [SerializeField] private int dominoCount = 3;
        [SerializeField] private int maxExtraMarbles = 4;
        [Tooltip("0 = theo thời gian; khác 0 = hạt giống cố định cho màu đồ chơi, độ lệch trang trí.")]
        [SerializeField] private int decorationSeed;

        // ---------------------------------------------------------------- trạng thái
        private enum Phase { Idle, Running, WaitLock, Finale }

        private ChestRevealSequence _reveal;
        private bool _wasPlaying;
        private Phase _phase = Phase.Idle;
        private float _t0, _T;
        private System.Random _rng;

        private Transform _root;
        private readonly List<Object> _owned = new List<Object>();
        private AudioSource _sfx, _bed;
        private float _lastEvent = -999f, _lastImpact = -999f, _lastTail = -999f;
        private float _bedFadeFrom = -1f;

        // khung toạ độ quanh rương (chụp lúc bấm mở)
        private Vector3 _ground, _c, _r, _u, _f;
        private float _s;
        private Bounds _chestBounds;

        private Rigidbody _marble, _marble2;
        private readonly List<Rigidbody> _dominoes = new List<Rigidbody>();
        private readonly List<bool> _dominoDown = new List<bool>();
        private readonly List<Rigidbody> _extras = new List<Rigidbody>();
        private readonly HashSet<Rigidbody> _extraScored = new HashSet<Rigidbody>();
        private LineRenderer _band;
        private Vector3 _bandL, _bandR, _bandRest;
        private bool _flicked, _released, _impacted;
        private float _releaseAt, _impactAt, _releaseT;
        private float _nextBeat;
        private int _beat;
        private int _combo;
        private int _dominoSounds;

        private bool _hasTier;
        private ChestTier _tier;
        private Color _tierColor;
        private float _finaleT;

        // máy bay giấy
        private Transform _plane;
        private Vector3 _planeFrom;
        private bool _crumpled, _confettiDone, _stingDone, _planeSoundDone, _bonkTailDone;
        private float _crumpleAt;
        private ParticleSystem _confetti;
        private ChestFxProfile _confettiStyle;

        // chuyển động cả rương (cộng dồn lên tư thế BoxOpenSequence đặt mỗi khung)
        private Vector3 _basePos, _writtenPos, _baseScale, _writtenScale;
        private Quaternion _baseRot, _writtenRot;
        private bool _bodyCaptured;
        private float _hopY, _hopV, _tension, _spin, _spinV, _squash, _squashV, _tilt;
        private float _sag;   // Rác: rương xẹp xuống

        private static readonly int[] Penta = { 0, 2, 4, 7, 9, 12, 14, 16, 19, 21, 24 };

        // ================================================================ hook khung

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            _tension = Mathf.Min(1f, _tension + 0.02f);   // mỗi lần đổi màu rương rùng mình thêm
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_phase == Phase.Idle) Begin(reveal);   // phòng khi bật giữa chừng
            _hasTier = true;
            _tier = tier;
            _tierColor = color;
            if (_phase == Phase.WaitLock) StartFinale();
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        // ================================================================ vòng đời

        private void Update()
        {
            if (_reveal == null) _reveal = GetComponent<ChestRevealSequence>();
            if (_reveal == null) return;

            // Bấm mở = ChestRevealSequence.Play(): IsPlaying bật lên ngay khung đó. Bắt đầu cỗ máy + âm tại đây.
            bool playing = _reveal.IsPlaying;
            if (playing && !_wasPlaying && _phase == Phase.Idle) Begin(_reveal);
            _wasPlaying = playing;

            if (_phase == Phase.Idle) return;
            float t = Time.time - _t0;
            if (_phase == Phase.Running) RunMachine(t);
            if (_phase == Phase.Running || _phase == Phase.WaitLock) { Beats(t); HandleDrop(t); }
            if (_phase == Phase.Finale) { FinaleAudio(Time.time - _finaleT); RunFinale(Time.time - _finaleT); }
            ScoreExtras();
            UpdateBand();
        }

        private void LateUpdate() => ApplyBody(Time.deltaTime);

        private void OnDestroy() => Cleanup();

        private void Begin(ChestRevealSequence reveal)
        {
            Cleanup();
            _reveal = reveal;
            _phase = Phase.Running;
            _t0 = Time.time;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);
            _T = Mathf.Clamp(EstimateLockSeconds(reveal), 3f, 9f);

            Measure();
            _root = new GameObject("TuoiTho_MayBiVe").transform;
            _root.SetPositionAndRotation(_ground, transform.rotation);
            _owned.Add(_root.gameObject);
            _sfx = _root.gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0.3f;
            _bed = _root.gameObject.AddComponent<AudioSource>();
            _bed.playOnAwake = false;
            _bed.spatialBlend = 0.3f;
            _bed.loop = true;
            _bed.clip = rollClip;
            _bed.volume = ChestAudioRule.BedMax * 0.33f * volume;
            if (rollClip != null) _bed.Play();   // NỀN: bắt đầu ngay lúc bấm, nhỏ rồi dâng dần (xem Beats)
            _lastEvent = _lastImpact = _lastTail = -999f;
            _bedFadeFrom = -1f;

            BuildPlayground();
            _releaseAt = Mathf.Max(0.8f * _T, _T - 0.6f);
            _nextBeat = 0f;
            PlayEvent(flickClip, 0.5f);   // sự kiện đầu tiên ngay lúc bấm: tiếng búng bi
        }

        private float EstimateLockSeconds(ChestRevealSequence reveal)
        {
            BoxOpenSequence box = reveal.Box;
            if (box == null || box.Stages == null) return 5f;
            float sum = extraLeadSeconds;
            IList<BoxOpenSequence.Stage> st = box.Stages;
            for (int i = 0; i < st.Count && i <= lockStageGuess; i++)
            {
                sum += st[i].travel;
                if (i < lockStageGuess) sum += st[i].hold;
            }
            return sum;
        }

        private void Measure()
        {
            Renderer[] rs = GetComponentsInChildren<Renderer>();
            Bounds b = new Bounds(transform.position, Vector3.one * 0.3f);
            bool any = false;
            foreach (Renderer r in rs)
            {
                if (r == null || !r.enabled || r is ParticleSystemRenderer || r is LineRenderer) continue;
                if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            }
            _chestBounds = b;
            _s = Mathf.Max(0.1f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)));
            _u = transform.up;
            _r = transform.right;
            _f = transform.forward;
            _c = b.center;
            _ground = new Vector3(b.center.x, b.min.y, b.center.z);
        }

        // ================================================================ dựng sân chơi

        private void BuildPlayground()
        {
            float s = _s;
            // Thước kẻ dốc ngắn, đầu dốc tựa một khối nhỏ (không còn tờ giấy, bút, hộp bút).
            Vector3 a = P(-1.15f, 0.45f, 0.55f), b2 = P(-0.75f, 0.03f, 0.55f);
            Vector3 dir = (b2 - a).normalized;
            Quaternion rq = Quaternion.LookRotation(dir, Vector3.Cross(dir, _f) * -1f);
            float len = Vector3.Distance(a, b2) + 0.1f * s;
            Box("ThuocKe", (a + b2) * 0.5f, rq, new Vector3(0.2f * s, 0.02f * s, len), new Color(0.55f, 0.85f, 1f), true);
            Box("KhoiDo", P(-1.15f, 0.225f, 0.55f) - dir * 0.03f * s, transform.rotation, new Vector3(0.2f, 0.45f, 0.2f) * s, new Color(0.9f, 0.3f, 0.35f), false);

            _marble = Sphere("BiVe", a + _u * 0.07f * s + dir * 0.05f * s, 0.06f * s, RandomToyColor(0.6f), 0.02f, TuoiThoPiece.Kind.Marble);
            _marble.isKinematic = true;   // nằm chờ cú búng

            // 3 quân domino; quân cuối đổ vào chốt ná.
            _dominoes.Clear();
            _dominoDown.Clear();
            for (int i = 0; i < dominoCount; i++)
            {
                float x = -0.6f + i * 0.15f;
                Rigidbody d = Body(Box("Domino" + i, P(x, 0.14f, 0.55f), transform.rotation,
                    new Vector3(0.035f, 0.28f, 0.14f) * s, i % 2 == 0 ? new Color(0.98f, 0.97f, 0.93f) : new Color(0.15f, 0.15f, 0.18f), true), 0.03f);
                Piece(d.gameObject, TuoiThoPiece.Kind.Domino, i);
                _dominoes.Add(d);
                _dominoDown.Add(false);
            }

            // Ná dây thun trước rương, viên bi thứ hai kéo căng sẵn.
            _bandL = P(-0.14f, 0.22f, 0.95f);
            _bandR = P(0.14f, 0.22f, 0.95f);
            Box("CocNaTrai", _bandL - _u * 0.11f * s, transform.rotation, new Vector3(0.03f, 0.22f, 0.03f) * s, new Color(0.6f, 0.4f, 0.2f), false);
            Box("CocNaPhai", _bandR - _u * 0.11f * s, transform.rotation, new Vector3(0.03f, 0.22f, 0.03f) * s, new Color(0.6f, 0.4f, 0.2f), false);
            _bandRest = P(0f, 0.2f, 1.12f);
            _marble2 = Sphere("BiNa", _bandRest, 0.06f * s, RandomToyColor(0.9f), 0.02f, TuoiThoPiece.Kind.Marble);
            _marble2.isKinematic = true;
            GameObject bandGo = new GameObject("DayThun");
            bandGo.transform.SetParent(_root, false);
            _band = bandGo.AddComponent<LineRenderer>();
            _band.positionCount = 3;
            _band.useWorldSpace = true;
            _band.widthMultiplier = 0.012f * s;
            Material bm = ChestFxFactory.CreateUnlit(new Color(0.95f, 0.75f, 0.2f), false, "TuoiThoBand");
            _owned.Add(bm);
            _band.sharedMaterial = bm;
        }

        /// <summary>Điểm theo khung rương: x sang phải, y lên, z ra trước, đơn vị = cỡ rương.</summary>
        private Vector3 P(float x, float y, float z) => _ground + (_r * x + _u * y + _f * z) * _s;

        private Color RandomToyColor(float sat)
        {
            float h = (float)_rng.NextDouble();
            return Color.HSVToRGB(h, sat, 1f);
        }

        private GameObject Prim(PrimitiveType type, string name, Color color, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(_root, true);
            if (!keepCollider) Destroy(go.GetComponent<Collider>());
            Material m = ChestFxFactory.CreateUnlit(color, false, "TuoiTho_" + name);
            _owned.Add(m);
            go.GetComponent<Renderer>().sharedMaterial = m;
            return go;
        }

        private GameObject Box(string name, Vector3 pos, Quaternion rot, Vector3 size, Color color, bool collider)
        {
            GameObject go = Prim(PrimitiveType.Cube, name, color, collider);
            go.transform.SetPositionAndRotation(pos, rot);
            go.transform.localScale = size;
            return go;
        }

        private Rigidbody Sphere(string name, Vector3 pos, float radius, Color color, float mass, TuoiThoPiece.Kind kind)
        {
            GameObject go = Prim(PrimitiveType.Sphere, name, color, true);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * radius * 2f;
            Rigidbody rb = Body(go, mass);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Piece(go, kind, 0);
            return rb;
        }

        private static Rigidbody Body(GameObject go, float mass)
        {
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.angularDamping = 0.2f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            return rb;
        }

        private void Piece(GameObject go, TuoiThoPiece.Kind kind, int index)
        {
            TuoiThoPiece p = go.AddComponent<TuoiThoPiece>();
            p.owner = this;
            p.kind = kind;
            p.index = index;
        }

        // ================================================================ chạy cỗ máy (đi trước, có cú hích dự phòng)

        private void RunMachine(float t)
        {
            float s = _s;
            if (!_flicked && t >= 0.12f)
            {
                _flicked = true;
                _marble.isKinematic = false;
                Vector3 dir = (P(-0.75f, 0.03f, 0.55f) - _marble.position).normalized;
                _marble.AddForce(dir * 0.6f * s * _marble.mass, ForceMode.Impulse);
            }

            // Domino: thời điểm dự kiến rải đều 0,25T → 0,55T; trễ quá 0,3 s thì hích ngọn cho đổ.
            for (int i = 0; i < _dominoes.Count; i++)
            {
                Rigidbody d = _dominoes[i];
                if (d == null) continue;
                bool down = Vector3.Angle(d.transform.up, _u) > 35f;
                if (down && !_dominoDown[i])
                {
                    _dominoDown[i] = true;
                    _tension = Mathf.Min(1f, _tension + 0.08f);
                }
                float planned = Mathf.Lerp(0.25f, 0.55f, _dominoes.Count > 1 ? i / (float)(_dominoes.Count - 1) : 0f) * _T;
                if (!down && t > planned + 0.3f && (i == 0 || _dominoDown[i - 1]))
                    d.AddForceAtPosition(_r * 0.25f * s * d.mass, d.position + _u * 0.12f * s, ForceMode.Impulse);
            }

            // Dây thun bật: khi quân domino cuối đã ngã (đập vào chốt ná) hoặc tới giờ dự phòng, bắn bi thứ hai lên nắp.
            bool lastDown = _dominoDown.Count == 0 || _dominoDown[_dominoDown.Count - 1];
            if (!_released && (lastDown || t >= _releaseAt))
            {
                _released = true;
                _releaseT = t;
                PlayEvent(bandClip, 0.5f);
                _marble2.isKinematic = false;
                Vector3 target = new Vector3(_c.x, _chestBounds.max.y, _c.z) + _u * 0.02f * s;
                float tf = 0.5f;
                Vector3 g = Physics.gravity;
                _marble2.linearVelocity = (target - _marble2.position) / tf - 0.5f * g * tf;
                _impactAt = t + tf;
            }

            if (_released && !_impacted && t >= _impactAt)
            {
                _impacted = true;
                // Rác đã biết bậc: không bonk (nền + nhịp dừng, lặng, rồi xịt — A4). Bậc khác: bonk làm HĐ cùng tiếng chốt.
                if (!_hasTier) PlayEvent(bonkClip, 0.5f);
                else if (_tier != ChestTier.Rac) Play(bonkClip, 0.7f);
                if (_marble2 != null) _marble2.linearVelocity = (_f * 0.6f + _u) * 1.2f * s + _r * 0.3f * s;
                Hop(0.12f);
                _squashV -= 6f;
                _phase = Phase.WaitLock;
                if (_hasTier) StartFinale();
            }
        }

        /// <summary>
        /// Nền + Nhịp từ lúc bấm (A2). Nền: vòng bi lăn, âm 0,15 → 0,35, cao độ +45% tới chốt. Nhịp: nốt mộc cầm,
        /// 0,65 s → 0,14 s, âm 0,25 → 0,55, cao dần theo ngũ cung.
        /// </summary>
        private void Beats(float t)
        {
            float k = Mathf.Clamp01(t / Mathf.Max(1f, _T));
            if (_bed != null && _bed.isPlaying && _bedFadeFrom < 0f)
            {
                _bed.volume = Mathf.Min(ChestAudioRule.BedMax, Mathf.Lerp(0.15f, 0.35f, k)) * volume;
                _bed.pitch = Mathf.Lerp(1f, 1.45f, k);
            }
            if (xyloClip == null || t < _nextBeat) return;
            int step = Mathf.Min(Penta.Length - 1, _beat / 2);
            PlayPitched(xyloClip, Mathf.Min(ChestAudioRule.BeatMax, Mathf.Lerp(0.25f, 0.55f, k)), Semi(Penta[step] - 12));
            _beat++;
            _nextBeat = t + Mathf.Lerp(0.65f, 0.14f, k * k);
            if (_phase == Phase.WaitLock) _tension = Mathf.Min(1f, _tension + 0.03f);   // treo chờ: rương rung mạnh dần
        }

        // ================================================================ tương tác phụ: thả thêm bi

        private void HandleDrop(float t)
        {
            if (t < 0.35f || _extras.Count >= maxExtraMarbles || !DropPressed(out Vector2 screen, out bool hasScreen)) return;

            Vector3 at = P(Mathf.Lerp(-1.6f, -0.3f, (float)_rng.NextDouble()), 0f, 0.55f);
            Camera cam = Camera.main;
            if (hasScreen && cam != null)
            {
                Ray ray = cam.ScreenPointToRay(new Vector3(screen.x, screen.y, 0f));
                Plane floor = new Plane(_u, _ground);
                if (floor.Raycast(ray, out float dist))
                {
                    Vector3 hit = ray.GetPoint(dist);
                    if ((hit - _ground).magnitude < 3f * _s) at = hit;   // quá xa thì bỏ, thả vào đường ray
                }
            }
            Rigidbody m = Sphere("BiThem" + _extras.Count, at + _u * 1.1f * _s, 0.055f * _s, RandomToyColor(0.8f), 0.02f, TuoiThoPiece.Kind.Marble);
            m.linearVelocity = -_u * 0.3f * _s;
            _extras.Add(m);
            if (ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap))
                PlayPitched(marblesClip, 0.4f, Semi(_extras.Count));
        }

        private bool DropPressed(out Vector2 screen, out bool hasScreen)
        {
            screen = Vector2.zero;
            hasScreen = false;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screen = Mouse.current.position.ReadValue();
                hasScreen = true;
                return true;
            }
            return Keyboard.current != null && Keyboard.current[Key.B].wasPressedThisFrame;
#else
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mp = Input.mousePosition;
                screen = new Vector2(mp.x, mp.y);
                hasScreen = true;
                return true;
            }
            return Input.GetKeyDown(KeyCode.B);
#endif
        }

        /// <summary>Bi thả thêm rơi trúng rương: nốt cao dần + rương nảy + thêm hoa giấy lúc kết (chỉ trang trí).</summary>
        private void ScoreExtras()
        {
            if (_extras.Count == 0) return;
            Bounds zone = _chestBounds;
            zone.Expand(0.08f * _s);
            foreach (Rigidbody m in _extras)
            {
                if (m == null || _extraScored.Contains(m) || !zone.Contains(m.position)) continue;
                _extraScored.Add(m);
                _combo++;
                if (ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap))
                    PlayPitched(xyloClip, 0.45f, Semi(Penta[Mathf.Min(Penta.Length - 1, 4 + _combo)]));
                Hop(0.04f + 0.01f * _combo);
                m.linearVelocity = Vector3.Reflect(m.linearVelocity, _u) * 0.6f + _f * 0.4f * _s;
            }
        }

        public void OnPieceHit(TuoiThoPiece p, float speed, Collider other)
        {
            // Sau chốt không phát tiếng va chạm (A4/A5): chỉ còn tiếng chốt + đuôi.
            if (_phase == Phase.Idle || _phase == Phase.Finale || speed < 0.15f * _s || Time.time - p.lastSound < 0.08f) return;
            if (!ChestAudioRule.TryGate(ref _lastImpact, ChestAudioRule.ImpactGap)) return;   // A3: va chạm cách nhau ≥ 0,12 s
            p.lastSound = Time.time;
            switch (p.kind)
            {
                case TuoiThoPiece.Kind.Domino:
                    // mỗi quân sau gõ cao hơn: tiếng "cách cách" dồn lên
                    PlayPitched(dominoClip, Mathf.Min(0.4f, 0.2f + speed / _s * 0.2f), Semi(Mathf.Min(14, _dominoSounds++)));
                    break;
                case TuoiThoPiece.Kind.Marble:
                    PlayPitched(flickClip, Mathf.Min(0.3f, 0.15f + speed / _s * 0.15f), 0.9f + 0.2f * (float)_rng.NextDouble());
                    break;
            }
        }

        // ================================================================ kết: chỉ sau khi có bậc

        private void StartFinale()
        {
            if (_phase == Phase.Finale) return;
            _phase = Phase.Finale;
            _finaleT = Time.time;
            _stingDone = _planeSoundDone = _bonkTailDone = false;
            float rank = _tier.Rank01();
            if (_tier == ChestTier.Rac)
            {
                // A4 Rác: dừng ngay nền + nhịp (không fade) → lặng 0,2 s → 1 tiếng xịt (phát trong RunFinale).
                if (_bed != null) _bed.Stop();
                _sagTarget = 1f;
                _tension = 0f;
            }
            else
            {
                // A4: Thường nền fade 0,3 s; bậc cao hơn dừng hẳn. TC phát ngay (đúng 1 clip theo bậc).
                if (_tier == ChestTier.Thuong && _bed != null && _bed.isPlaying) _bedFadeFrom = _bed.volume;
                else if (_bed != null) _bed.Stop();
                PlaySting(StingVolume(_tier));
                _stingDone = true;
                _winPitch = Mathf.Lerp(0.85f, 1.1f, rank);
                Hop(0.08f + 0.35f * rank);   // cao nhất: bay vọt
                if (rank >= 0.7f) _spinV = 720f * rank;   // Rất hiếm+: xoay một vòng trên không
                _tension = 0f;
                // Đồ chơi trên sân cùng nảy lên mừng (lực thật).
                foreach (Rigidbody d in _dominoes)
                    if (d != null) d.AddForce((_u * (0.5f + rank) + _f * 0.2f) * _s * d.mass * (float)(0.6 + 0.8 * _rng.NextDouble()), ForceMode.Impulse);
            }
            SpawnPlane();
        }

        private static float StingVolume(ChestTier tier)
        {
            switch (tier)
            {
                case ChestTier.Rac: return 1f;
                case ChestTier.Thuong: return 0.7f;
                case ChestTier.Hiem: return 0.85f;
                default: return 1f;
            }
        }

        private void PlaySting(float vol)
        {
            int i = Mathf.Clamp((int)_tier, 0, 4);
            if (stingClips != null && i < stingClips.Length) Play(stingClips[i], Mathf.Min(ChestAudioRule.StingMax, vol));
        }

        /// <summary>Âm của đoạn kết (A4/A5), theo giờ từ lúc chốt. Không có tiếng nào khác trong 0,6 s đầu của Rác sau tiếng xịt.</summary>
        private void FinaleAudio(float t)
        {
            // Nền Thường: fade 0,3 s rồi tắt.
            if (_bedFadeFrom >= 0f && _bed != null)
            {
                _bed.volume = _bedFadeFrom * Mathf.Clamp01(1f - t / 0.3f);
                if (t >= 0.3f) { _bed.Stop(); _bedFadeFrom = -1f; }
            }
            if (_tier == ChestTier.Rac)
            {
                if (!_stingDone && t >= ChestAudioRule.RacSilence) { _stingDone = true; PlaySting(1f); }
                // Đuôi Rác: máy bay (sau khoảng im 0,6 s kể từ tiếng xịt), rồi giấy vò + bonk cách nhau 0,15 s.
                if (!_planeSoundDone && t >= ChestAudioRule.RacSilence + 0.7f && _planeFrom != Vector3.zero)
                    TailSound(planeClip, 0.3, ref _planeSoundDone);
                if (_crumpled && !_bonkTailDone && t >= _crumpleAt + ChestAudioRule.TailGap)
                    TailSound(bonkClip, 0.3, ref _bonkTailDone);
            }
            else if (!_planeSoundDone && t >= 0.4f)
                TailSound(planeClip, 0.35, ref _planeSoundDone);
        }

        private void TailSound(AudioClip clip, double vol, ref bool flag)
        {
            flag = true;
            if (ChestAudioRule.TryGate(ref _lastTail, ChestAudioRule.TailGap))
                Play(clip, Mathf.Min(ChestAudioRule.TailMax, (float)vol));
        }

        private float _sagTarget, _winPitch = 1f;

        private void RunFinale(float t)
        {
            if (_plane == null) return;
            float rank = _tier.Rank01();
            Vector3 over = new Vector3(_c.x, _chestBounds.max.y, _c.z) + _u * 0.7f * _s;

            if (_tier == ChestTier.Rac)
            {
                if (_crumpled) return;
                // Lượn vào chòng chành rồi cắm đầu xuống nắp.
                if (t < 1.1f)
                {
                    float k = t / 1.1f;
                    Vector3 p = Vector3.Lerp(_planeFrom, over + _u * 0.2f * _s, k) + _u * Mathf.Sin(k * 9f) * 0.08f * _s;
                    Vector3 next = Vector3.Lerp(_planeFrom, over, k + 0.02f);
                    _plane.SetPositionAndRotation(p, Quaternion.LookRotation((next - p).normalized + _u * 0.01f, _u) * Quaternion.Euler(0f, 0f, Mathf.Sin(t * 14f) * 35f));
                }
                else if (t < 1.5f)
                {
                    float k = (t - 1.1f) / 0.4f;
                    Vector3 to = new Vector3(_c.x, _chestBounds.max.y, _c.z);
                    _plane.position = Vector3.Lerp(over + _u * 0.2f * _s, to, k * k);
                    _plane.rotation = Quaternion.LookRotation(-_u + _f * 0.2f, _f);
                }
                else Crumple();
                return;
            }

            // Bậc cao: lượn vòng quanh rương, nhào lộn (Rất hiếm+), thả hoa giấy ngay trên miệng rương.
            float dur = 2.4f;
            if (t > dur) { _plane.gameObject.SetActive(false); return; }
            float u = t / dur;
            float ang = Mathf.Lerp(-200f, 160f, u) * Mathf.Deg2Rad;
            Vector3 pos = over + (_r * Mathf.Cos(ang) + _f * Mathf.Sin(ang)) * 0.9f * _s + _u * Mathf.Sin(u * Mathf.PI) * 0.3f * _s;
            float loop = rank >= 0.7f ? Mathf.SmoothStep(0f, 360f, Mathf.InverseLerp(0.35f, 0.6f, u)) : 0f;
            Vector3 tangent = (-_r * Mathf.Sin(ang) + _f * Mathf.Cos(ang));
            _plane.SetPositionAndRotation(pos, Quaternion.LookRotation(tangent, _u) * Quaternion.Euler(-loop, 0f, -25f));
            if (!_confettiDone && u > 0.5f)
            {
                _confettiDone = true;
                Confetti(_plane.position, Mathf.Min(120, Mathf.RoundToInt(20 + 60 * rank + 10 * _combo)));
                if (ChestAudioRule.TryGate(ref _lastTail, ChestAudioRule.TailGap))
                    PlayPitched(xyloClip, 0.3f, Semi(12) * _winPitch);   // đuôi: một nốt mộc cầm nhỏ
            }
        }

        private void SpawnPlane()
        {
            // Máy bay giấy: lưới tam giác gập (thân + hai cánh), hai mặt.
            GameObject go = new GameObject("MayBayGiay");
            go.transform.SetParent(_root, true);
            Mesh mesh = new Mesh { name = "TuoiThoPlane" };
            Vector3[] v =
            {
                new Vector3(0f, 0f, 0.5f), new Vector3(-0.35f, 0.04f, -0.5f), new Vector3(0f, -0.02f, -0.5f),
                new Vector3(0.35f, 0.04f, -0.5f), new Vector3(0f, -0.14f, -0.5f),
            };
            int[] tris = { 0, 1, 2, 0, 2, 1, 0, 2, 3, 0, 3, 2, 0, 2, 4, 0, 4, 2 };
            mesh.vertices = v;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _owned.Add(mesh);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            Material m = ChestFxFactory.CreateUnlit(new Color(1f, 1f, 0.97f), false, "TuoiThoPlane");
            _owned.Add(m);
            go.AddComponent<MeshRenderer>().sharedMaterial = m;
            go.transform.localScale = Vector3.one * 0.35f * _s;
            _planeFrom = new Vector3(_c.x, _chestBounds.max.y, _c.z) + (-_r * 2.2f + _u * 1.2f + _f * 0.4f) * _s;
            go.transform.position = _planeFrom;
            _plane = go.transform;
            _crumpled = false;
            _confettiDone = false;
            _crumpleAt = 0f;
        }

        private void Crumple()
        {
            _crumpled = true;
            Vector3 at = _plane.position;
            _plane.gameObject.SetActive(false);
            _crumpleAt = Time.time - _finaleT;
            if (ChestAudioRule.TryGate(ref _lastTail, ChestAudioRule.TailGap)) Play(crumpleClip, 0.4f);   // bonk theo sau 0,15 s (FinaleAudio)
            Rigidbody ball = Sphere("CucGiay", at + _u * 0.05f * _s, 0.06f * _s, new Color(0.95f, 0.95f, 0.9f), 0.01f, TuoiThoPiece.Kind.Plane);
            ball.linearVelocity = (_r * 0.8f + _u * 0.9f + _f * 0.3f) * _s;
            ball.angularVelocity = _f * 10f;
            _squashV -= 4f;   // rương bị cộc một cái, xẹp thêm
        }

        private void Confetti(Vector3 at, int count)
        {
            if (_confetti == null)
            {
                _confettiStyle = ScriptableObject.CreateInstance<ChestFxProfile>();
                _confettiStyle.hideFlags = HideFlags.DontSave;
                _confettiStyle.particleLook = ParticleLook.SoftDot;
                _confettiStyle.particleDirection = ParticleDirection.Radial;
                _confettiStyle.particleSpeed = 0.5f;
                _confettiStyle.particleSize = 0.035f;
                _confettiStyle.particleLifetime = 2.2f;
                _confettiStyle.burstGravity = 0.35f;
                _confettiStyle.secondaryColor = new Color(1f, 0.9f, 0.4f);
                _confettiStyle.secondaryMix = 0.5f;
                _confetti = ChestFxFactory.CreateParticles("TuoiThoHoaGiay", _root, _confettiStyle, _s, true);
            }
            _confetti.transform.position = at;
            ChestFxFactory.SetParticleColor(_confetti, _tierColor, _confettiStyle, 1f);
            ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
            for (int i = 0; i < count; i++)
            {
                Vector3 d = new Vector3((float)_rng.NextDouble() - 0.5f, (float)_rng.NextDouble() * 0.6f, (float)_rng.NextDouble() - 0.5f);
                ep.position = at;
                ep.velocity = d * 1.6f * _s;
                ep.startColor = Color.HSVToRGB((float)_rng.NextDouble(), 0.7f, 1f);
                _confetti.Emit(ep, 1);
            }
        }

        // ================================================================ chuyển động CẢ rương

        private void Hop(float heightInChestSizes)
        {
            float g = 9.81f * 1.5f;
            _hopV = Mathf.Max(_hopV, Mathf.Sqrt(2f * g * heightInChestSizes * _s));
        }

        private void ApplyBody(float dt)
        {
            if (_phase == Phase.Idle && !_bodyCaptured) return;
            Transform tr = transform;
            // Lấy tư thế gốc: nếu ai khác (BoxOpenSequence rung rương) vừa ghi thì lấy giá trị mới, không thì giữ gốc cũ.
            if (!_bodyCaptured || tr.localPosition != _writtenPos) _basePos = tr.localPosition;
            if (!_bodyCaptured || tr.localRotation != _writtenRot) _baseRot = tr.localRotation;
            if (!_bodyCaptured || tr.localScale != _writtenScale) _baseScale = tr.localScale;
            _bodyCaptured = true;

            // Nảy: rơi tự do có trọng lực, chạm đất thì bẹp (lò xo squash).
            float g = 9.81f * 1.5f;
            _hopV -= g * dt;
            _hopY += _hopV * dt;
            if (_hopY <= 0f)
            {
                if (_hopV < -0.5f) _squashV -= Mathf.Min(8f, -_hopV * 2.5f);
                _hopY = 0f;
                _hopV = 0f;
            }
            _squashV += (-60f * _squash - 7f * _squashV) * dt;
            _squash += _squashV * dt;
            _spin += _spinV * dt;
            if (_spin >= 360f) { _spin = 0f; _spinV = 0f; }
            _sag = Mathf.MoveTowards(_sag, _sagTarget, dt * 2.5f);
            if (_phase == Phase.Finale && _tier == ChestTier.Rac && Time.time - _finaleT > 2.5f) _sagTarget = 0f;
            float shiver = _phase == Phase.Finale ? 0f : _tension;
            _tilt = (Mathf.PerlinNoise(Time.time * 30f, 1.7f) - 0.5f) * 6f * shiver * shiver;

            Vector3 upLocal = tr.parent != null ? tr.parent.InverseTransformDirection(_u) : _u;
            Vector3 jitter = new Vector3(Mathf.PerlinNoise(Time.time * 40f, 0.2f) - 0.5f, 0f, Mathf.PerlinNoise(0.9f, Time.time * 40f) - 0.5f) * 0.01f * _s * shiver;
            Vector3 pos = _basePos + upLocal * _hopY + (tr.parent != null ? tr.parent.InverseTransformVector(jitter) : jitter);
            Quaternion rot = _baseRot * Quaternion.Euler(0f, _spin, _tilt);
            float sq = Mathf.Clamp(_squash * 0.05f, -0.35f, 0.35f) - 0.25f * _sag;
            Vector3 scl = Vector3.Scale(_baseScale, new Vector3(1f - sq * 0.5f, 1f + sq, 1f - sq * 0.5f));

            tr.localPosition = _writtenPos = pos;
            tr.localRotation = _writtenRot = rot;
            tr.localScale = _writtenScale = scl;
        }

        private void RestoreBody()
        {
            if (!_bodyCaptured) return;
            Transform tr = transform;
            if (tr.localPosition == _writtenPos) tr.localPosition = _basePos;
            if (tr.localRotation == _writtenRot) tr.localRotation = _baseRot;
            if (tr.localScale == _writtenScale) tr.localScale = _baseScale;
            _bodyCaptured = false;
        }

        // ================================================================ dây thun, âm, dọn

        private void UpdateBand()
        {
            if (_band == null) return;
            Vector3 mid = !_released && _marble2 != null ? _marble2.position
                : Vector3.Lerp(_bandL, _bandR, 0.5f) + _f * Mathf.Sin((Time.time - _t0 - _releaseT) * 40f) * 0.04f * _s * Mathf.Exp(-(Time.time - _t0 - _releaseT) * 5f);
            if (!_released)
            {
                // kéo căng dần theo độ căng: bi lùi ra sau
                float k = Mathf.Clamp01((Time.time - _t0) / Mathf.Max(0.5f, _releaseAt));
                if (_marble2 != null) _marble2.MovePosition(_bandRest + _f * 0.2f * _s * k * k);
            }
            _band.SetPosition(0, _bandL);
            _band.SetPosition(1, mid);
            _band.SetPosition(2, _bandR);
        }

        private static float Semi(int semitones) => Mathf.Pow(2f, semitones / 12f);

        /// <summary>Tiếng cao độ 1: phát trên nguồn sfx của hook (đã nhân volume tổng một lần).</summary>
        private void Play(AudioClip clip, float vol)
        {
            if (clip == null || _sfx == null) return;
            _sfx.PlayOneShot(clip, vol * volume);
        }

        /// <summary>Sự kiện lẻ trước chốt: qua cổng EventGap (0,2 s), trần EventMax.</summary>
        private void PlayEvent(AudioClip clip, float vol)
        {
            if (!ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap)) return;
            Play(clip, Mathf.Min(ChestAudioRule.EventMax, vol));
        }

        /// <summary>Tiếng đổi cao độ: lõi tạo nguồn riêng (A7), không đổi pitch của nguồn đang ngân.</summary>
        private void PlayPitched(AudioClip clip, float vol, float pitch)
        {
            if (clip == null) return;
            if (_reveal != null) _reveal.PlayOneShot(clip, vol * volume, pitch);
            else Play(clip, vol);
        }

        private void Cleanup()
        {
            RestoreBody();
            if (_confetti != null)
            {
                ParticleSystemRenderer pr = _confetti.GetComponent<ParticleSystemRenderer>();
                if (pr != null && pr.sharedMaterial != null) Destroy(pr.sharedMaterial);
            }
            if (_confettiStyle != null) Destroy(_confettiStyle);
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
            _dominoes.Clear();
            _dominoDown.Clear();
            _extras.Clear();
            _extraScored.Clear();
            _confetti = null;
            _confettiStyle = null;
            _root = null;
            _plane = null;
            _band = null;
            _marble = _marble2 = null;
            _sfx = _bed = null;
            _phase = Phase.Idle;
            _wasPlaying = false;
            _flicked = _released = _impacted = _hasTier = false;
            _stingDone = _planeSoundDone = _bonkTailDone = false;
            _planeFrom = Vector3.zero;
            _bedFadeFrom = -1f;
            _releaseT = _crumpleAt = 0f;
            _hopY = _hopV = _tension = _spin = _spinV = _squash = _squashV = _tilt = _sag = _sagTarget = 0f;
            _beat = _combo = _dominoSounds = 0;
        }
    }
}

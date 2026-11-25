using System.Collections.Generic;
using ChoDoLa.Interaction;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TuoiTho — "CỖ MÁY BI VE" (Rube Goldberg tuổi thơ). Bấm mở là cỗ máy chạy NGAY:
    /// búng bi → bi lăn xuống thước kẻ kẹp giữa hai cây bút → đổ hàng domino → domino cuối tông xe đồ chơi →
    /// xe húc con quay → con quay lắc tuột chốt dây thun → ná dây thun bắn viên bi thứ hai vọt lên nắp rương →
    /// CẢ RƯƠNG bật nảy. Đến lúc chốt bậc mới lộ kết quả: bậc cao thì rương nhảy xoay vòng, máy bay giấy lượn
    /// tới nhào lộn thả hoa giấy; Rác thì còi trượt rơi, rương xẹp "xịt", máy bay giấy chòng chành, rơi cắm
    /// đầu, vò thành cục giấy cộc vào rương rồi lăn đi.
    /// Tương tác phụ: trong lúc máy chạy, bấm chuột/chạm (hoặc phím B) để thả thêm bi ve từ trên cao xuống
    /// chỗ trỏ — bi thật (Rigidbody) có thể đổ domino sớm, húc xe, hay rơi trúng rương (mỗi viên trúng = một
    /// nốt mộc cầm cao dần + rương nảy nhẹ + thêm hoa giấy lúc kết). Mỗi khâu có "cú hích dự phòng" theo giờ
    /// để chuỗi luôn tới đích đúng nhịp dù vật lý lệch.
    /// Bất biến: chỉ trang trí. Không đọc/đổi bậc trước OnTierLocked; ngẫu nhiên lấy từ hạt giống riêng.
    /// Gắn component này lên gốc Chest_TuoiTho (cùng chỗ ChestRevealSequence). Âm gán bởi TuoiThoFxSetup.
    /// </summary>
    public class TuoiThoHook : ChestFxHook
    {
        [Header("Âm (Audio/Chests/tuoi_tho/may_bi/)")]
        public AudioClip flickClip;
        public AudioClip rollClip;
        public AudioClip dominoClip;
        public AudioClip glassClip;
        public AudioClip carClip;
        public AudioClip topClip;
        public AudioClip bandClip;
        public AudioClip bonkClip;
        public AudioClip xyloClip;
        public AudioClip riserClip;
        public AudioClip failClip;
        public AudioClip winClip;
        public AudioClip planeClip;
        public AudioClip crumpleClip;
        [Range(0f, 1f)] public float volume = 0.8f;

        [Header("Nhịp")]
        [Tooltip("Stage của BoxOpenSequence mà bậc được chốt (khớp lockStage của ChestRevealSequence, mặc định 3).")]
        [SerializeField] private int lockStageGuess = 3;
        [Tooltip("Giây dư cộng thêm (đoạn chìa khoá bay vào trước các stage, nếu có).")]
        [SerializeField] private float extraLeadSeconds;
        [SerializeField] private int dominoCount = 6;
        [SerializeField] private int maxExtraMarbles = 6;
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
        private AudioSource _sfx, _riser;

        // khung toạ độ quanh rương (chụp lúc bấm mở)
        private Vector3 _ground, _c, _r, _u, _f;
        private float _s;
        private Bounds _chestBounds;

        private Rigidbody _marble, _marble2, _car, _top;
        private readonly List<Rigidbody> _dominoes = new List<Rigidbody>();
        private readonly List<bool> _dominoDown = new List<bool>();
        private readonly List<Rigidbody> _extras = new List<Rigidbody>();
        private readonly HashSet<Rigidbody> _extraScored = new HashSet<Rigidbody>();
        private LineRenderer _band;
        private Vector3 _bandL, _bandR, _bandRest;
        private bool _flicked, _carGone, _topHit, _released, _impacted;
        private float _releaseAt, _impactAt;
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
        private bool _crumpled, _confettiDone;
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
            if (_phase == Phase.Finale) RunFinale(Time.time - _finaleT);
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
            _riser = _root.gameObject.AddComponent<AudioSource>();
            _riser.playOnAwake = false;
            _riser.spatialBlend = 0.3f;

            BuildPlayground();
            _releaseAt = Mathf.Max(0.8f * _T, _T - 0.6f);
            _nextBeat = 0f;
            Play(flickClip, 0.9f);   // âm bắt đầu NGAY lúc bấm: tiếng búng bi
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
            Color paper = new Color(0.96f, 0.95f, 0.88f);
            // Tờ giấy ô li làm mặt sàn của cỗ máy (bên trái-trước rương).
            Box("ToGiay", P(-1.25f, -0.006f, 0.35f), Quaternion.identity, new Vector3(2.7f, 0.012f, 1.5f) * s, paper, false);

            // Thước kẻ dốc + hai cây bút làm ray.
            Vector3 a = P(-2.35f, 0.75f, 0.55f), b2 = P(-1.75f, 0.03f, 0.55f);
            Vector3 dir = (b2 - a).normalized;
            Quaternion rq = Quaternion.LookRotation(dir, Vector3.Cross(dir, _f) * -1f);
            float len = Vector3.Distance(a, b2) + 0.1f * s;
            Box("ThuocKe", (a + b2) * 0.5f, rq, new Vector3(0.2f * s, 0.02f * s, len), new Color(0.55f, 0.85f, 1f), true);
            Vector3 side = Vector3.Cross(dir, _u).normalized;
            Box("ButChi", (a + b2) * 0.5f + side * 0.11f * s + _u * 0.03f * s, rq, new Vector3(0.035f * s, 0.035f * s, len), new Color(1f, 0.8f, 0.2f), true);
            Box("ButBi", (a + b2) * 0.5f - side * 0.11f * s + _u * 0.03f * s, rq, new Vector3(0.035f * s, 0.035f * s, len), new Color(0.2f, 0.35f, 0.9f), true);
            // Hộp bút đỡ đầu dốc.
            Box("HopBut", a - _u * 0.4f * s - dir * 0.05f * s, transform.rotation, new Vector3(0.3f, 0.75f, 0.3f) * s, new Color(0.9f, 0.3f, 0.35f), false);

            _marble = Sphere("BiVe", a + _u * 0.07f * s + dir * 0.05f * s, 0.06f * s, RandomToyColor(0.6f), 0.02f, TuoiThoPiece.Kind.Marble);
            _marble.isKinematic = true;   // nằm chờ cú búng

            // Hàng domino nối tới xe.
            _dominoes.Clear();
            _dominoDown.Clear();
            for (int i = 0; i < dominoCount; i++)
            {
                float x = -1.55f + i * 0.17f;
                Rigidbody d = Body(Box("Domino" + i, P(x, 0.14f, 0.55f), transform.rotation,
                    new Vector3(0.035f, 0.28f, 0.14f) * s, i % 2 == 0 ? new Color(0.98f, 0.97f, 0.93f) : new Color(0.15f, 0.15f, 0.18f), true), 0.03f);
                Piece(d.gameObject, TuoiThoPiece.Kind.Domino, i);
                _dominoes.Add(d);
                _dominoDown.Add(false);
            }

            // Xe đồ chơi (thân + 4 bánh).
            GameObject car = Box("XeDoChoi", P(-0.42f, 0.07f, 0.55f), transform.rotation, new Vector3(0.24f, 0.09f, 0.13f) * s, new Color(0.95f, 0.2f, 0.15f), true);
            for (int i = 0; i < 4; i++)
            {
                GameObject w = Prim(PrimitiveType.Cylinder, "Banh" + i, new Color(0.1f, 0.1f, 0.1f), false);
                w.transform.SetParent(car.transform, false);
                w.transform.localPosition = new Vector3(i < 2 ? -0.32f : 0.32f, -0.45f, i % 2 == 0 ? -0.55f : 0.55f);
                w.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                w.transform.localScale = new Vector3(0.35f, 0.1f, 0.7f);
            }
            _car = Body(car, 0.08f);
            Piece(car, TuoiThoPiece.Kind.Car, 0);

            // Con quay đang quay sẵn (thân trụ + đầu nón giả bằng trụ nhỏ).
            GameObject top = Prim(PrimitiveType.Cylinder, "ConQuay", new Color(0.3f, 0.75f, 0.35f), true);
            top.transform.SetPositionAndRotation(P(0.62f, 0.07f, 0.62f), transform.rotation);
            top.transform.localScale = new Vector3(0.16f, 0.06f, 0.16f) * s;
            GameObject tip = Prim(PrimitiveType.Cylinder, "Coc", new Color(0.4f, 0.25f, 0.1f), false);
            tip.transform.SetParent(top.transform, false);
            tip.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            tip.transform.localScale = new Vector3(0.18f, 0.6f, 0.18f);
            _top = Body(top, 0.06f);
            _top.maxAngularVelocity = 80f;
            _top.angularVelocity = _u * 40f;
            Piece(top, TuoiThoPiece.Kind.Top, 0);

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
                Vector3 dir = (P(-1.75f, 0.03f, 0.55f) - _marble.position).normalized;
                _marble.AddForce(dir * 0.6f * s * _marble.mass, ForceMode.Impulse);
                Play(rollClip, 0.6f);
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

            // Xe: domino cuối đổ (hoặc tới giờ 0,6T) thì xe lao đi húc con quay.
            bool lastDown = _dominoDown.Count == 0 || _dominoDown[_dominoDown.Count - 1];
            if (!_carGone && (lastDown && t > 0.5f * _T || t > 0.62f * _T))
            {
                _carGone = true;
                Vector3 to = (_top.position - _car.position);
                to -= _u * Vector3.Dot(to, _u);
                _car.linearVelocity = to.normalized * Mathf.Max(0.8f * s, to.magnitude / Mathf.Max(0.3f, 0.12f * _T));
                Play(carClip, 0.7f);
            }

            // Con quay: xe chạm (hoặc tới giờ) thì quay lảo đảo, vù vù.
            if (!_topHit && _carGone && (t > 0.78f * _T || (_car.position - _top.position).magnitude < 0.22f * s))
            {
                _topHit = true;
                _top.AddForce((_f * 0.6f + _r * 0.2f) * 0.25f * s * _top.mass, ForceMode.Impulse);
                _top.AddTorque(_r * 2f * _top.mass * s, ForceMode.Impulse);
                Play(topClip, 0.6f);
                if (riserClip != null)   // còi trượt dâng: phần cuối của độ căng, khớp vào lúc bi đập nắp
                {
                    _riser.clip = riserClip;
                    _riser.pitch = Mathf.Clamp(riserClip.length / Mathf.Max(0.5f, _releaseAt + 0.5f - t), 0.7f, 2f);
                    _riser.volume = volume * 0.55f;
                    _riser.Play();
                }
            }

            // Dây thun bật: bắn bi thứ hai theo đường parabol lên nắp rương.
            if (!_released && t >= _releaseAt)
            {
                _released = true;
                Play(bandClip, 0.9f);
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
                Play(bonkClip, 1f);
                if (_marble2 != null) _marble2.linearVelocity = (_f * 0.6f + _u) * 1.2f * s + _r * 0.3f * s;
                Hop(0.12f);
                _squashV -= 6f;
                _riser.Stop();
                _phase = Phase.WaitLock;
                if (_hasTier) StartFinale();
            }
        }

        /// <summary>Nhịp mộc cầm từ lúc bấm: thưa → dày, cao dần theo thang ngũ cung (độ căng dâng).</summary>
        private void Beats(float t)
        {
            if (xyloClip == null || t < _nextBeat) return;
            float k = Mathf.Clamp01(t / Mathf.Max(1f, _T));
            int step = Mathf.Min(Penta.Length - 1, _beat / 2);
            PlayPitched(xyloClip, 0.35f + 0.3f * k, Semi(Penta[step] - 12));
            _beat++;
            _nextBeat = t + Mathf.Lerp(0.5f, 0.11f, k * k);
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
            PlayPitched(glassClip, 0.5f, Semi(_extras.Count));
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
                PlayPitched(xyloClip, 0.7f, Semi(Penta[Mathf.Min(Penta.Length - 1, 4 + _combo)]));
                Hop(0.04f + 0.01f * _combo);
                m.linearVelocity = Vector3.Reflect(m.linearVelocity, _u) * 0.6f + _f * 0.4f * _s;
            }
        }

        public void OnPieceHit(TuoiThoPiece p, float speed, Collider other)
        {
            if (_phase == Phase.Idle || speed < 0.15f * _s || Time.time - p.lastSound < 0.08f) return;
            p.lastSound = Time.time;
            switch (p.kind)
            {
                case TuoiThoPiece.Kind.Domino:
                    // mỗi quân sau gõ cao hơn: tiếng "cách cách" dồn lên
                    PlayPitched(dominoClip, Mathf.Clamp01(0.3f + speed / _s * 0.3f), Semi(Mathf.Min(14, _dominoSounds++)));
                    break;
                case TuoiThoPiece.Kind.Marble:
                    PlayPitched(glassClip, Mathf.Clamp01(0.2f + speed / _s * 0.2f), 0.9f + 0.2f * (float)_rng.NextDouble());
                    break;
                case TuoiThoPiece.Kind.Car:
                case TuoiThoPiece.Kind.Top:
                    PlayPitched(dominoClip, 0.4f, 0.7f);
                    break;
                case TuoiThoPiece.Kind.Plane:
                    PlayPitched(bonkClip, 0.5f, 1.3f);
                    break;
            }
        }

        // ================================================================ kết: chỉ sau khi có bậc

        private void StartFinale()
        {
            if (_phase == Phase.Finale) return;
            _phase = Phase.Finale;
            _finaleT = Time.time;
            _riser.Stop();
            float rank = _tier.Rank01();
            if (_tier == ChestTier.Rac)
            {
                Play(failClip, 1f);          // còi trượt rơi + xì hơi: xịt
                _sagTarget = 1f;
                _tension = 0f;
            }
            else
            {
                Play(winClip, Mathf.Lerp(0.45f, 1f, rank));
                _winPitch = Mathf.Lerp(0.85f, 1.1f, rank);
                Hop(0.08f + 0.35f * rank);   // cao nhất: bay vọt
                if (rank >= 0.7f) _spinV = 720f * rank;   // Rất hiếm+: xoay một vòng trên không
                _tension = 0f;
                // Đồ chơi trên sân cùng nảy lên mừng (lực thật).
                foreach (Rigidbody d in _dominoes)
                    if (d != null) d.AddForce((_u * (0.5f + rank) + _f * 0.2f) * _s * d.mass * (float)(0.6 + 0.8 * _rng.NextDouble()), ForceMode.Impulse);
                if (_car != null) _car.AddForce(_u * (0.6f + rank) * _s * _car.mass, ForceMode.Impulse);
            }
            SpawnPlane();
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
                Confetti(_plane.position, Mathf.RoundToInt(30 + 140 * rank + 15 * _combo));
                PlayPitched(xyloClip, 0.6f, Semi(12) * _winPitch);
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
            Play(planeClip, 0.7f);
        }

        private void Crumple()
        {
            _crumpled = true;
            Vector3 at = _plane.position;
            _plane.gameObject.SetActive(false);
            Play(crumpleClip, 0.9f);
            Play(bonkClip, 0.6f);
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
                : Vector3.Lerp(_bandL, _bandR, 0.5f) + _f * Mathf.Sin((Time.time - _t0 - _releaseAt) * 40f) * 0.04f * _s * Mathf.Exp(-(Time.time - _t0 - _releaseAt) * 5f);
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

        private void Play(AudioClip clip, float vol) => PlayPitched(clip, vol, 1f);

        private void PlayPitched(AudioClip clip, float vol, float pitch)
        {
            if (clip == null || _sfx == null) return;
            // AudioSource.PlayOneShot dùng pitch của nguồn: đổi pitch ngay trước mỗi phát (tiếng trước đó vẫn ngân).
            _sfx.pitch = pitch;
            _sfx.PlayOneShot(clip, vol * volume);
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
            _marble = _marble2 = _car = _top = null;
            _sfx = _riser = null;
            _phase = Phase.Idle;
            _wasPlaying = false;
            _flicked = _carGone = _topHit = _released = _impacted = _hasTier = false;
            _hopY = _hopV = _tension = _spin = _spinV = _squash = _squashV = _tilt = _sag = _sagTarget = 0f;
            _beat = _combo = _dominoSounds = 0;
        }
    }
}

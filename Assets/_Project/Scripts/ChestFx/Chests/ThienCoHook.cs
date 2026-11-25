using System.Collections.Generic;
using ChoDoLa.Interaction;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Kịch bản riêng Chest_ThienCo: "BỘ MÁY THIÊN CƠ" (cơ khí đồng hồ). Xem docs/superpowers/specs/2026-11-25-kichban-thienco-design.md.
    ///
    /// - Ngay khi bấm mở (prefab hook được ChestRevealSequence.Play tạo ra, Start chạy luôn): tiếng lên dây cót, bộ thoát
    ///   tích–tắc bắt đầu, nhanh dần và cao dần tới lúc chốt bậc. Rương "bị lên dây": giật theo từng tích, lún dần như lò xo nén.
    /// - Ba bánh răng đồng lộ ra trước rương, khớp giữa bị THIẾU. Bánh răng lạc rơi từ trên xuống (Rigidbody thật, nảy, keng).
    ///   Người chơi bấm chuột vào nó hoặc phím G để nhét vào khớp; không thì chim cu bật ra khỏi cửa nhỏ, gáy, và hút nó vào.
    ///   Lần nhét đầu có thể lệch răng (theo hạt giống, chỉ trang trí): kẹt, rung, bật ra rồi vào lại.
    ///   Khớp xong cả đoàn răng quay theo nhau đúng tỉ số bán kính.
    /// - Con lắc thật (Rigidbody + HingeJoint) được đẩy mỗi nhịp tắc. Đồng hồ cát: hạt đồng là Rigidbody rơi qua cổ, chồng đống.
    /// - Chuông điểm giờ ở 35 %, 65 %, 90 % thời gian chờ (1, 2, 3 tiếng) — GIỐNG NHAU mọi bậc, giữ bí mật tới lúc chốt.
    /// - Chốt bậc (OnTierLocked):
    ///   Rác: tích tắc tắt phựt, kẹt răng, LÒ XO BUNG: bánh răng thành vật thể tự do văng tứ tung, con lắc đứt rơi,
    ///        đồng hồ cát đổ nghiêng tràn hạt, rương xẹp lệch, chim cu ló ra gáy méo rồi rũ xuống. Âm rớt cụt: hết dây cót.
    ///   Thường..Huyền thoại: chuông đánh (số tiếng = hạng), lò xo bật rương nhảy lên (cao theo hạng). Rất hiếm+: hộp nhạc dâng.
    ///        Huyền thoại: chuông lớn, rương bay xoay một vòng, bánh răng tách ra quay quanh rương như mô hình thiên văn,
    ///        hạt cát nổ tung lên thành mưa đồng (lực nổ thật).
    /// Chỉ TRANG TRÍ: không đọc/đổi gì ngoài RolledTier lúc đã chốt; ngẫu nhiên dùng System.Random theo hạt giống.
    /// </summary>
    public class ThienCoHook : ChestFxHook
    {
        [Header("Âm (ThienCoFxSetup điền từ Audio/Chests/thien_co/clockwork/)")]
        public AudioClip tick;
        public AudioClip tock;
        public AudioClip wind;
        public AudioClip gearClank;
        public AudioClip meshOk;
        public AudioClip jam;
        public AudioClip bell;
        public AudioClip bellBig;
        public AudioClip sproing;
        public AudioClip winddown;
        public AudioClip cuckoo;
        public AudioClip cuckooSad;
        public AudioClip launch;
        public AudioClip sandLoop;
        public AudioClip musicBoxRise;

        [Header("Nhịp")]
        [Tooltip("Hạt giống trang trí (0 = mỗi lần một khác). Không ảnh hưởng bậc.")]
        public int decorationSeed;
        [Tooltip("Số giai đoạn BoxOpenSequence khoá màu (khớp lockStage của ChestRevealSequence, mặc định 3).")]
        public int lockStageGuess = 3;
        public float tickIntervalStart = 0.55f;
        public float tickIntervalEnd = 0.11f;
        public float volume = 0.8f;
        [Tooltip("Giây chờ người chơi nhét bánh răng trước khi chim cu tự làm (tính theo phần thời gian tới chốt).")]
        [Range(0.2f, 0.9f)] public float autoInsertAt = 0.5f;

        // ------------------------------------------------------------------ trạng thái

        private ChestRevealSequence _reveal;
        private Transform _chest;
        private System.Random _rng;
        private bool _started;
        private float _t0, _eta;
        private Transform _root;
        private readonly List<Object> _owned = new List<Object>();
        private AudioSource _sfx, _loop;

        // Khung toạ độ (thế giới) đo lúc bắt đầu
        private Vector3 _base, _right, _front;
        private float _k, _height, _halfW, _halfD;

        // Bánh răng
        private class Gear { public Transform t; public float r; public float angle; public float sign; public Rigidbody rb; public Vector3 home; }
        private Gear _a, _b, _c;
        private float _driveAngle, _driveSpeed;
        private enum Slot { Waiting, Falling, Grounded, Inserting, Jammed, Meshed, Free }
        private Slot _slot;
        private float _slotT;
        private bool _wrongFirst, _byPlayer;
        private Vector3 _slotPos;

        // Con lắc, đồng hồ cát, chim cu
        private Rigidbody _pivot, _bob;
        private HingeJoint _hinge;
        private int _kickSign = 1;
        private Transform _glass, _sandTop;
        private readonly List<Rigidbody> _grains = new List<Rigidbody>();
        private readonly List<Collider> _glassWalls = new List<Collider>();
        private float _nextGrain;
        private Transform _birdArm, _bird;
        private float _birdOut, _birdTarget, _birdDroop;

        // Âm theo nhịp
        private float _nextTick;
        private int _tickCount;
        private readonly bool[] _chimed = new bool[3];
        private bool _wound2;

        // Chốt và cú bật
        private bool _locked;
        private ChestTier _tier;
        private float _lockT;
        private readonly List<KeyValuePair<float, System.Action>> _cues = new List<KeyValuePair<float, System.Action>>();
        private float _hopY, _hopV, _spin, _spinV, _slump, _tilt;
        private bool _orrery;

        // Lệch đã áp lên gốc rương (gỡ ra đầu mỗi khung, áp lại cuối khung)
        private Vector3 _appliedPos, _appliedScale = Vector3.one;
        private Quaternion _appliedRot = Quaternion.identity;
        private float _twitch, _squash;

        // ------------------------------------------------------------------ vòng đời

        private void Start()
        {
            _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null) return;
            _chest = _reveal.transform;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);
            _t0 = Time.time;
            _eta = EstimateSecondsToLock();
            Measure();
            Build();
            _started = true;

            // ÂM BẮT ĐẦU NGAY KHI BẤM MỞ
            Play(wind, 0.8f, 1f);
            _nextTick = Time.time + 0.45f;
            if (_loop != null && sandLoop != null) { _loop.clip = sandLoop; _loop.loop = true; _loop.volume = 0f; _loop.Play(); }
            _wrongFirst = _rng.NextDouble() < 0.5;
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (!_started || _locked) return;
            _twitch += 1.2f;        // mỗi lần đổi màu: rương giật một nấc như kim giây
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (!_started || _locked) return;
            _locked = true;
            _tier = tier;
            _lockT = Time.time;
            if (_slot != Slot.Meshed && tier != ChestTier.Rac) ForceMesh();
            if (tier == ChestTier.Rac) PlanFlop(); else PlanTriumph(tier.Rank());
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            UndoChestPose();
            _started = false;
            if (_root != null) Destroy(_root.gameObject);
            _root = null;
        }

        private void OnDestroy()
        {
            UndoChestPose();
            if (_root != null) Destroy(_root.gameObject);
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
        }

        // ------------------------------------------------------------------ đo và dựng

        private float EstimateSecondsToLock()
        {
            BoxOpenSequence box = _reveal.Box;
            if (box == null || box.Stages == null || box.Stages.Count == 0) return 3f;
            int last = Mathf.Clamp(lockStageGuess, 0, box.Stages.Count - 1);
            float s = 0f;
            for (int i = 0; i <= last; i++) s += box.Stages[i].travel + (i < last ? box.Stages[i].hold : 0f);
            if (_chest.Find("Key") != null) s += 2.15f;     // đoạn chìa khoá của BoxOpenSequence
            return Mathf.Max(2.5f, s);
        }

        private void Measure()
        {
            bool has = false;
            Bounds b = new Bounds(_chest.position, Vector3.zero);
            foreach (Renderer r in _chest.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer || r.transform.IsChildOf(transform)) continue;
                if (r.gameObject.name.StartsWith("Rank") || r.gameObject.name.StartsWith("Glow")) continue;
                if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
            }
            if (!has) b = new Bounds(_chest.position + Vector3.up * 0.25f, new Vector3(0.4f, 0.5f, 0.4f));
            BoxOpenSequence box = _reveal.Box;
            Vector3 f = box != null ? _chest.TransformDirection(box.FrontDirection) : -_chest.forward;
            _front = Vector3.ProjectOnPlane(f, Vector3.up).normalized;
            if (_front.sqrMagnitude < 0.01f) _front = Vector3.back;
            _right = Vector3.Cross(Vector3.up, _front).normalized;
            _base = new Vector3(b.center.x, b.min.y, b.center.z);
            _height = b.size.y;
            _halfW = Mathf.Abs(Vector3.Dot(b.extents, new Vector3(Mathf.Abs(_right.x), Mathf.Abs(_right.y), Mathf.Abs(_right.z))));
            _halfD = Mathf.Abs(Vector3.Dot(b.extents, new Vector3(Mathf.Abs(_front.x), Mathf.Abs(_front.y), Mathf.Abs(_front.z))));
            _k = Mathf.Max(0.3f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)) / 0.5f);   // rương mẫu ~0,5 m
        }

        private Material Mat(Color c, string n) { Material m = ChestFxFactory.CreateUnlit(c, false, n); _owned.Add(m); return m; }

        private Vector3 P(float x, float y, float z) => _base + _right * x + Vector3.up * y + _front * z;

        private void Build()
        {
            _root = new GameObject("ThienCo_Clockwork").transform;
            _root.gameObject.hideFlags = HideFlags.DontSave;
            _root.position = _base;
            _root.rotation = Quaternion.LookRotation(_front, Vector3.up);

            _sfx = _root.gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false; _sfx.spatialBlend = 0.5f;
            _loop = _root.gameObject.AddComponent<AudioSource>();
            _loop.playOnAwake = false; _loop.spatialBlend = 0.5f;

            Material brass = Mat(ThienCoRig.Brass, "ThienCo_Brass");
            Material dark = Mat(ThienCoRig.DarkBrass, "ThienCo_DarkBrass");
            Material steel = Mat(ThienCoRig.Steel, "ThienCo_Steel");
            Material sand = Mat(ThienCoRig.Sand, "ThienCo_Sand");
            Material glass = ChestFxFactory.CreateAdditive(null, "ThienCo_Glass");
            ChestFxFactory.SetColor(glass, new Color(0.35f, 0.7f, 0.8f, 0.25f));
            _owned.Add(glass);

            // Sàn và thân rương vô hình để vật rơi có chỗ chạm.
            ThienCoRig.StaticBox("Floor", _root, Vector3.down * 0.02f, Quaternion.identity, new Vector3(4f, 0.04f, 4f) * _k);
            ThienCoRig.StaticBox("ChestBody", _root, Vector3.up * _height * 0.5f, Quaternion.identity,
                                 new Vector3(_halfW * 2f, _height, _halfD * 2f));

            // --- Đoàn bánh răng trên mặt trước. Khoảng cách tâm = tổng bán kính (ăn khớp).
            float rA = 0.085f * _k, rB = 0.055f * _k, rC = 0.07f * _k;
            float z = _halfD + 0.025f * _k, yLow = _height * 0.32f;
            Vector3 cA = new Vector3(-_halfW * 0.55f, yLow, z), cC = new Vector3(_halfW * 0.55f, yLow, z);
            Vector3 cB = CircleMeet(cA, rA + rB, cC, rC + rB, z);
            _a = MakeGear("GearA", rA, cA, brass, steel, 1f);
            _c = MakeGear("GearC", rC, cC, brass, steel, 1f);
            _b = MakeGear("GearB_Lost", rB, cB, brass, steel, -1f);
            _slotPos = cB;
            _b.t.gameObject.SetActive(false);
            Transform peg = ThienCoRig.Visual("SlotPeg", _root, PrimitiveType.Cylinder, null, dark);
            peg.localPosition = cB + Vector3.forward * -0.005f * _k;
            peg.localRotation = Quaternion.Euler(90f, 0f, 0f);
            peg.localScale = new Vector3(0.012f, 0.02f, 0.012f) * _k;
            _slot = Slot.Waiting;

            // --- Con lắc thật bên trái: trụ treo động học + quả lắc Rigidbody, nối HingeJoint trục = hướng trước.
            float len = Mathf.Min(_height * 0.7f, 0.3f * _k);
            Vector3 pivotLocal = new Vector3(-_halfW - 0.06f * _k, _height + 0.02f * _k, 0f);
            Transform arm = ThienCoRig.Visual("PendulumArm", _root, PrimitiveType.Cube, null, dark);
            arm.localPosition = new Vector3(-_halfW - 0.03f * _k, _height + 0.02f * _k, 0f);
            arm.localScale = new Vector3(0.07f, 0.012f, 0.012f) * _k;
            var pivotGo = new GameObject("PendulumPivot") { hideFlags = HideFlags.DontSave };
            pivotGo.transform.SetParent(_root, false);
            pivotGo.transform.localPosition = pivotLocal;
            _pivot = ThienCoRig.Body(pivotGo, 1f, true);
            var bobGo = new GameObject("PendulumBob") { hideFlags = HideFlags.DontSave };
            bobGo.transform.SetParent(_root, false);
            bobGo.transform.localPosition = pivotLocal + Vector3.down * len;
            Transform rod = ThienCoRig.Visual("Rod", bobGo.transform, PrimitiveType.Cylinder, null, brass);
            rod.localPosition = Vector3.up * len * 0.5f;
            rod.localScale = new Vector3(0.006f * _k, len * 0.5f, 0.006f * _k);
            Transform disc = ThienCoRig.Visual("Disc", bobGo.transform, PrimitiveType.Cylinder, null, brass);
            disc.localRotation = Quaternion.Euler(90f, 0f, 0f);
            disc.localScale = new Vector3(0.05f, 0.004f, 0.05f) * _k;
            _bob = ThienCoRig.Body(bobGo, 0.3f, false);
            _bob.angularDamping = 0.05f;
            _bob.linearDamping = 0.02f;
            _hinge = bobGo.AddComponent<HingeJoint>();
            _hinge.connectedBody = _pivot;
            _hinge.anchor = Vector3.up * len;
            _hinge.axis = Vector3.forward;
            _bob.AddForce(_right * 0.05f * _k, ForceMode.Impulse);

            // --- Đồng hồ cát bên phải: vỏ kính + tường va chạm khoang dưới + hạt Rigidbody rơi qua cổ.
            float hg = Mathf.Min(_height * 0.75f, 0.22f * _k), wg = hg * 0.45f;
            _glass = new GameObject("Hourglass") { hideFlags = HideFlags.DontSave }.transform;
            _glass.SetParent(_root, false);
            _glass.localPosition = new Vector3(_halfW + wg * 0.5f + 0.03f * _k, 0f, 0f);
            Transform shell = ThienCoRig.Visual("Glass", _glass, null, ThienCoRig.HourglassMesh(), glass);
            _owned.Add(shell.GetComponent<MeshFilter>().sharedMesh);
            shell.localScale = new Vector3(wg, hg, wg);
            Transform cap = ThienCoRig.Visual("CapTop", _glass, PrimitiveType.Cylinder, null, dark);
            cap.localPosition = Vector3.up * hg; cap.localScale = new Vector3(wg * 1.1f, 0.006f * _k, wg * 1.1f);
            Transform capB = ThienCoRig.Visual("CapBottom", _glass, PrimitiveType.Cylinder, null, dark);
            capB.localScale = cap.localScale;
            _sandTop = ThienCoRig.Visual("SandTop", _glass, PrimitiveType.Sphere, null, sand);
            _sandTop.localPosition = Vector3.up * hg * 0.66f;
            _sandTop.localScale = new Vector3(wg * 0.7f, hg * 0.22f, wg * 0.7f);
            float inner = wg * 0.42f, low = hg * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Quaternion q = Quaternion.Euler(0f, 90f * i, 0f);
                _glassWalls.Add(ThienCoRig.StaticBox("Wall" + i, _glass, q * (Vector3.forward * (inner + 0.004f * _k)) + Vector3.up * low * 0.5f, q,
                                                     new Vector3(inner * 2f, low, 0.008f * _k)));
            }
            _glassWalls.Add(ThienCoRig.StaticBox("GlassFloor", _glass, Vector3.up * 0.004f * _k, Quaternion.identity, new Vector3(inner * 2f, 0.008f * _k, inner * 2f)));

            // --- Chim cu: cửa nhỏ trên nóc phía trước, tay lò xo thò ra.
            _birdArm = new GameObject("CuckooArm") { hideFlags = HideFlags.DontSave }.transform;
            _birdArm.SetParent(_root, false);
            _birdArm.localPosition = new Vector3(0f, _height * 0.82f, _halfD);
            Transform door = ThienCoRig.Visual("Door", _root, PrimitiveType.Cube, null, dark);
            door.localPosition = _birdArm.localPosition + Vector3.forward * 0.004f * _k;
            door.localScale = new Vector3(0.05f, 0.06f, 0.006f) * _k;
            Transform spring = ThienCoRig.Visual("Spring", _birdArm, PrimitiveType.Cylinder, null, steel);
            spring.localRotation = Quaternion.Euler(90f, 0f, 0f);
            spring.localPosition = Vector3.forward * 0.5f;
            spring.localScale = new Vector3(0.15f, 0.5f, 0.15f);
            _bird = new GameObject("Bird") { hideFlags = HideFlags.DontSave }.transform;
            _bird.SetParent(_root, false);
            Transform body = ThienCoRig.Visual("Body", _bird, PrimitiveType.Sphere, null, Mat(new Color(0.55f, 0.32f, 0.18f), "ThienCo_Bird"));
            body.localScale = new Vector3(0.04f, 0.035f, 0.05f) * _k;
            Transform head = ThienCoRig.Visual("Head", _bird, PrimitiveType.Sphere, null, body.GetComponent<Renderer>().sharedMaterial);
            head.localPosition = new Vector3(0f, 0.025f, 0.018f) * _k; head.localScale = Vector3.one * 0.028f * _k;
            Transform beak = ThienCoRig.Visual("Beak", _bird, PrimitiveType.Cube, null, Mat(new Color(1f, 0.75f, 0.1f), "ThienCo_Beak"));
            beak.localPosition = new Vector3(0f, 0.024f, 0.036f) * _k; beak.localScale = new Vector3(0.008f, 0.006f, 0.016f) * _k;
            _bird.gameObject.SetActive(false);
            _birdArm.gameObject.SetActive(false);
        }

        private Gear MakeGear(string name, float r, Vector3 local, Material mat, Material hubMat, float sign)
        {
            int teeth = Mathf.Max(8, Mathf.RoundToInt(r / (0.011f * _k)));
            Mesh m = ThienCoRig.GearMesh(teeth, 0.22f, 0.8f, 0.18f);
            _owned.Add(m);
            var go = new GameObject(name) { hideFlags = HideFlags.DontSave };
            go.transform.SetParent(_root, false);
            go.transform.localPosition = local;
            Transform vis = ThienCoRig.Visual("Mesh", go.transform, null, m, mat);
            vis.localScale = Vector3.one * r * 1.08f;          // bán kính đỉnh hơi lớn hơn bán kính lăn
            Transform hub = ThienCoRig.Visual("Hub", go.transform, PrimitiveType.Cylinder, null, hubMat);
            hub.localRotation = Quaternion.Euler(90f, 0f, 0f);
            hub.localScale = new Vector3(r * 0.4f, r * 0.15f, r * 0.4f);
            return new Gear { t = go.transform, r = r, sign = sign, home = local };
        }

        /// <summary>Tâm bánh giữa: giao hai đường tròn (tâm A bán kính dA, tâm C bán kính dC), lấy điểm phía trên.</summary>
        private static Vector3 CircleMeet(Vector3 a, float dA, Vector3 c, float dC, float z)
        {
            Vector2 p0 = new Vector2(a.x, a.y), p1 = new Vector2(c.x, c.y);
            float d = Mathf.Max(1e-4f, (p1 - p0).magnitude);
            float aLen = (dA * dA - dC * dC + d * d) / (2f * d);
            float h = Mathf.Sqrt(Mathf.Max(0f, dA * dA - aLen * aLen));
            Vector2 mid = p0 + (p1 - p0) * (aLen / d);
            Vector2 n = new Vector2(-(p1.y - p0.y), p1.x - p0.x) / d;
            Vector2 r = mid + n * h;
            if (r.y < mid.y) r = mid - n * h;
            return new Vector3(r.x, r.y, z);
        }

        // ------------------------------------------------------------------ khung hình

        private void Update()
        {
            UndoChestPose();
            if (!_started || _root == null) return;
            float now = Time.time, dt = Time.deltaTime;
            float u = Mathf.Clamp01((now - _t0) / _eta);        // 0 lúc bấm mở → 1 lúc dự kiến chốt

            // Gốc dàn máy đi theo rương (rương đứng yên ngoài các cú giật của mình).
            if (!_locked) UpdateTension(now, u);
            UpdateSlot(now, u);
            UpdateGears(dt);
            UpdateHourglass(now, u);
            UpdateBird(dt);
            RunCues(now);
            UpdateHop(dt);
            if (_loop != null && _loop.clip != null && _slot != Slot.Free)
                _loop.volume = Mathf.MoveTowards(_loop.volume, _locked ? 0f : 0.15f + 0.25f * u, dt * 0.6f);
        }

        private void LateUpdate()
        {
            if (!_started || _chest == null) return;
            // Áp lệch lên gốc rương SAU khi BoxOpenSequence đặt vị trí (nó đặt tuyệt đối mỗi khung khi đang chạy).
            _twitch = Mathf.MoveTowards(_twitch, 0f, Time.deltaTime * 12f);
            float yaw = Mathf.Sin(_twitch * 3f) * _twitch * 0.8f;
            _appliedPos = Vector3.up * _hopY + _right * _slump * 0.02f * _k;
            _appliedRot = Quaternion.AngleAxis(yaw + _spin, Vector3.up) * Quaternion.AngleAxis(_tilt, _front);
            float sy = 1f - _squash;
            _appliedScale = new Vector3(1f + _squash * 0.5f, sy, 1f + _squash * 0.5f);
            _chest.position += _appliedPos;
            _chest.rotation = _appliedRot * _chest.rotation;
            _chest.localScale = Vector3.Scale(_chest.localScale, _appliedScale);
        }

        private void UndoChestPose()
        {
            if (_chest == null) return;
            _chest.localScale = new Vector3(_chest.localScale.x / _appliedScale.x, _chest.localScale.y / _appliedScale.y, _chest.localScale.z / _appliedScale.z);
            _chest.rotation = Quaternion.Inverse(_appliedRot) * _chest.rotation;
            _chest.position -= _appliedPos;
            _appliedPos = Vector3.zero; _appliedRot = Quaternion.identity; _appliedScale = Vector3.one;
        }

        /// <summary>Trước khi chốt: tích tắc nhanh dần, chuông điểm giờ, rương lún như lò xo đang nén. Giống nhau mọi bậc.</summary>
        private void UpdateTension(float now, float u)
        {
            _squash = Mathf.Lerp(_squash, 0.06f * u * u, Time.deltaTime * 4f);
            _driveSpeed = _slot == Slot.Meshed ? Mathf.Lerp(40f, 260f, u * u) : 0f;
            while (now >= _nextTick)
            {
                bool isTick = (_tickCount & 1) == 0;
                float pitch = Mathf.Lerp(0.95f, 1.35f, u);
                Play(isTick ? tick : tock, Mathf.Lerp(0.5f, 1f, u), pitch);
                _twitch += 0.6f + u;
                if (_slot != Slot.Meshed) _driveAngle += isTick ? 4f : -4f;   // thiếu khớp: chỉ lắc qua lại
                if (!isTick) KickPendulum(u);
                _tickCount++;
                float ease = 1f - (1f - u) * (1f - u);
                _nextTick += Mathf.Lerp(tickIntervalStart, tickIntervalEnd, ease);
            }
            float[] marks = { 0.35f, 0.65f, 0.9f };
            for (int i = 0; i < 3; i++)
            {
                if (_chimed[i] || u < marks[i]) continue;
                _chimed[i] = true;
                for (int s = 0; s <= i; s++) { int n = s; Cue(now + 0.22f * n, () => Play(bell, 0.45f, 1f + 0.12f * n)); }
            }
            if (!_wound2 && u > 0.5f) { _wound2 = true; Play(wind, 0.6f, 1.15f); }
        }

        private void KickPendulum(float u)
        {
            if (_bob == null || _hinge == null) return;
            _kickSign = -_kickSign;
            _bob.AddForce(_right * _kickSign * (0.015f + 0.03f * u) * _k, ForceMode.Impulse);
        }

        // ------------------------------------------------------------------ bánh răng lạc và khớp

        private void UpdateSlot(float now, float u)
        {
            switch (_slot)
            {
                case Slot.Waiting:
                    if (u >= 0.12f) DropLostGear();
                    break;
                case Slot.Falling:
                case Slot.Grounded:
                    if (_slot == Slot.Falling && now - _slotT > 0.8f) _slot = Slot.Grounded;
                    if (PlayerWantsInsert()) { _byPlayer = true; BeginInsert(); }
                    else if (u >= autoInsertAt) { _birdTarget = 1f; if (_birdOut > 0.9f) { Play(cuckoo, 0.6f, 1f); BeginInsert(); } }
                    break;
                case Slot.Inserting:
                {
                    float k = Mathf.Clamp01((now - _slotT) / 0.55f);
                    Vector3 from = _b.home;
                    Vector3 lift = Vector3.Lerp(from, _slotPos, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.08f * _k;
                    _b.t.localPosition = lift;
                    _b.t.localRotation = Quaternion.Slerp(_b.t.localRotation, Quaternion.identity, k);
                    if (k >= 1f)
                    {
                        if (_wrongFirst) { _wrongFirst = false; _slot = Slot.Jammed; _slotT = now; Play(jam, 0.7f, 1.1f); }
                        else Mesh(now);
                    }
                    break;
                }
                case Slot.Jammed:
                {
                    // Lệch nửa răng: rung bần bật rồi tự nảy ra và vào lại.
                    float k = now - _slotT;
                    _b.t.localPosition = _slotPos + new Vector3(Mathf.Sin(now * 90f), Mathf.Cos(now * 77f), 0f) * 0.004f * _k;
                    _twitch += Time.deltaTime * 8f;
                    if (k > 0.6f) { _b.home = _slotPos + Vector3.up * 0.03f * _k; _slotT = now; _slot = Slot.Inserting; }
                    break;
                }
            }
        }

        private void DropLostGear()
        {
            _b.t.gameObject.SetActive(true);
            _b.t.localPosition = _slotPos + Vector3.up * (0.35f * _k) + Vector3.forward * 0.12f * _k + Vector3.right * (float)(_rng.NextDouble() - 0.5) * 0.1f * _k;
            _b.t.localRotation = Quaternion.Euler((float)_rng.NextDouble() * 360f, (float)_rng.NextDouble() * 360f, 0f);
            BoxCollider col = _b.t.gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(_b.r * 2f, _b.r * 2f, _b.r * 0.4f);
            _b.rb = ThienCoRig.Body(_b.t.gameObject, 0.15f, false);
            _b.rb.AddTorque(new Vector3((float)_rng.NextDouble(), (float)_rng.NextDouble(), (float)_rng.NextDouble()) * 0.002f, ForceMode.Impulse);
            ThienCoImpact imp = _b.t.gameObject.AddComponent<ThienCoImpact>();
            imp.onHit = (i, speed) => Play(gearClank, Mathf.Clamp01(speed * 0.4f), 1f + (float)_rng.NextDouble() * 0.15f);
            _slot = Slot.Falling;
            _slotT = Time.time;
        }

        private void BeginInsert()
        {
            if (_b.rb != null) { _b.rb.isKinematic = true; _b.rb.useGravity = false; }
            _b.home = _b.t.localPosition;
            _slot = Slot.Inserting;
            _slotT = Time.time;
        }

        private void Mesh(float now)
        {
            _slot = Slot.Meshed;
            _b.t.localPosition = _slotPos;
            _b.t.localRotation = Quaternion.identity;
            Play(meshOk, 0.8f, _byPlayer ? 1.1f : 1f);
            if (_byPlayer) Play(cuckoo, 0.45f, 1.2f);     // người chơi tự làm: chim cu khen một tiếng
            _birdTarget = 0f;
            _twitch += 3f;
        }

        private void ForceMesh()
        {
            if (_slot == Slot.Waiting) { _b.t.gameObject.SetActive(true); }
            if (_b.rb != null) { _b.rb.isKinematic = true; _b.rb.useGravity = false; }
            _wrongFirst = false;
            Mesh(Time.time);
        }

        private bool PlayerWantsInsert()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current[Key.G].wasPressedThisFrame) return true;
            bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            Vector3 mouse = click ? (Vector3)Mouse.current.position.ReadValue() : Vector3.zero;
#else
            if (Input.GetKeyDown(KeyCode.G)) return true;
            bool click = Input.GetMouseButtonDown(0);
            Vector3 mouse = Input.mousePosition;
#endif
            if (!click || Camera.main == null) return false;
            return Physics.Raycast(Camera.main.ScreenPointToRay(mouse), out RaycastHit hit, 50f) && hit.collider != null &&
                   hit.collider.transform == _b.t;
        }

        private void UpdateGears(float dt)
        {
            if (_orrery)
            {
                // Huyền thoại: các bánh răng quay quanh rương như mô hình thiên văn (động học, không trọng lực).
                float t = Time.time - _lockT;
                OrbitGear(_a, t, 0f, 1.0f);
                OrbitGear(_b, t, 2.1f, 1.35f);
                OrbitGear(_c, t, 4.2f, 0.8f);
                return;
            }
            if (_slot == Slot.Free) return;
            _driveAngle += _driveSpeed * dt;
            SetGearAngle(_a, _driveAngle);
            SetGearAngle(_c, _driveAngle * _a.r / _c.r);                  // hai lần ăn khớp: cùng chiều A
            if (_slot == Slot.Meshed) SetGearAngle(_b, -_driveAngle * _a.r / _b.r);
        }

        private static void SetGearAngle(Gear g, float deg)
        {
            if (g.rb != null && !g.rb.isKinematic) return;
            g.t.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
        }

        private void OrbitGear(Gear g, float t, float phase, float speed)
        {
            float ang = phase + t * speed * 2f;
            float rad = (_halfW + 0.18f * _k) * Mathf.Min(1f, t * 1.5f);
            Vector3 c = Vector3.up * (_height * 0.6f + _hopY);
            Vector3 target = c + new Vector3(Mathf.Cos(ang) * rad, Mathf.Sin(t * 2f + phase) * 0.05f * _k, Mathf.Sin(ang) * rad);
            g.t.localPosition = Vector3.Lerp(g.t.localPosition, target, Time.deltaTime * 6f);
            g.t.localRotation = Quaternion.AngleAxis(t * 400f * speed, Vector3.forward) * Quaternion.Euler(0f, -ang * Mathf.Rad2Deg, 0f);
        }

        // ------------------------------------------------------------------ đồng hồ cát

        private void UpdateHourglass(float now, float u)
        {
            if (_glass == null || _locked || now < _nextGrain || _grains.Count >= 48) return;
            _nextGrain = now + Mathf.Lerp(0.16f, 0.05f, u);
            float hg = _sandTop.localPosition.y / 0.66f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Grain"; go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(_glass, false);
            float jitter = 0.004f * _k;
            go.transform.localPosition = Vector3.up * hg * 0.47f + new Vector3((float)(_rng.NextDouble() - 0.5) * jitter, 0f, (float)(_rng.NextDouble() - 0.5) * jitter);
            go.transform.localScale = Vector3.one * 0.016f * _k;
            Renderer r = go.GetComponent<Renderer>();
            r.sharedMaterial = _sandTop.GetComponent<Renderer>().sharedMaterial;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Rigidbody rb = ThienCoRig.Body(go, 0.002f, false);
            rb.linearDamping = 0.4f;
            _grains.Add(rb);
            float left = 1f - _grains.Count / 48f;
            _sandTop.localScale = new Vector3(_sandTop.localScale.x, hg * 0.22f * Mathf.Max(0.05f, left), _sandTop.localScale.z);
        }

        // ------------------------------------------------------------------ chim cu

        private void UpdateBird(float dt)
        {
            if (_birdArm == null) return;
            _birdOut = Mathf.MoveTowards(_birdOut, _birdTarget, dt * 3f);
            bool show = _birdOut > 0.01f;
            _birdArm.gameObject.SetActive(show);
            _bird.gameObject.SetActive(show);
            if (!show) return;
            float len = 0.12f * _k * _birdOut;
            float bounce = Mathf.Sin(Time.time * 18f) * 0.006f * _k * (1f - _birdDroop);
            _birdArm.localScale = new Vector3(0.05f * _k, 0.05f * _k, len);
            Vector3 tip = _birdArm.localPosition + Vector3.forward * len;
            _bird.localPosition = tip + Vector3.up * bounce - Vector3.up * _birdDroop * 0.05f * _k;
            _bird.localRotation = Quaternion.Euler(_birdDroop * 70f, 0f, _birdDroop * 25f);
        }

        // ------------------------------------------------------------------ chốt: kịch bản theo bậc

        private void Cue(float at, System.Action a) => _cues.Add(new KeyValuePair<float, System.Action>(at, a));

        private void RunCues(float now)
        {
            for (int i = 0; i < _cues.Count; i++)
            {
                if (_cues[i].Key > now) continue;
                System.Action a = _cues[i].Value;
                _cues.RemoveAt(i--);
                a();
            }
        }

        /// <summary>Bậc Rác: tích tắc tắt phựt, kẹt, lò xo bung văng bánh răng, mọi thứ xẹp. Âm rớt cụt.</summary>
        private void PlanFlop()
        {
            float t = Time.time;
            _driveSpeed = 0f;
            Play(jam, 0.9f, 0.85f);
            _twitch += 6f;
            Cue(t + 0.35f, () =>
            {
                Play(sproing, 1f, 1f);
                _slot = Slot.Free;
                Vector3 boom = _root.position + Vector3.up * _height * 0.3f;
                foreach (Gear g in new[] { _a, _b, _c })
                {
                    g.t.gameObject.SetActive(true);
                    if (g.t.GetComponent<Collider>() == null)
                    {
                        BoxCollider col = g.t.gameObject.AddComponent<BoxCollider>();
                        col.size = new Vector3(g.r * 2f, g.r * 2f, g.r * 0.4f);
                    }
                    g.rb = ThienCoRig.Body(g.t.gameObject, 0.15f, false);
                    if (g.t.GetComponent<ThienCoImpact>() == null)
                        g.t.gameObject.AddComponent<ThienCoImpact>().onHit = (i, s) => Play(gearClank, Mathf.Clamp01(s * 0.3f), 0.8f + (float)_rng.NextDouble() * 0.3f);
                    g.rb.AddExplosionForce(1.2f * _k, boom, 1f * _k, 0.6f, ForceMode.Impulse);
                    g.rb.AddTorque(new Vector3((float)_rng.NextDouble() - 0.5f, (float)_rng.NextDouble() - 0.5f, (float)_rng.NextDouble() - 0.5f) * 0.008f, ForceMode.Impulse);
                }
                if (_hinge != null) { Destroy(_hinge); _hinge = null; }       // con lắc đứt
                foreach (Collider w in _glassWalls) if (w != null) w.enabled = false;
                _hopV = 0.6f;
            });
            Cue(t + 0.6f, () => Play(winddown, 0.9f, 1f));
            Cue(t + 1.3f, () => { _birdTarget = 0.7f; Play(cuckooSad, 0.7f, 1f); });
            Cue(t + 2.1f, () => _birdDroop = 1f);
        }

        /// <summary>Bậc Thường..Huyền thoại: chuông = hạng, lò xo bật rương nhảy, Rất hiếm+ hộp nhạc dâng, Huyền thoại bay xoay.</summary>
        private void PlanTriumph(int rank)
        {
            float t = Time.time;
            _driveSpeed = 0f;
            float build = rank >= 3 ? 1.0f : 0.15f;
            if (rank >= 3) Play(musicBoxRise, 0.75f, 1f);
            for (int i = 0; i < rank; i++) { int n = i; Cue(t + build * 0.3f + 0.18f * n, () => Play(bell, 0.7f, 1f + 0.12f * n)); }
            float pop = t + build + 0.18f * rank;
            Cue(pop, () =>
            {
                Play(rank >= 3 ? launch : sproing, 0.9f, rank >= 3 ? 1f : 1.4f);
                _squash = 0f;
                _hopV = new[] { 0f, 0.9f, 1.4f, 2.0f, 3.2f }[rank] * Mathf.Sqrt(_k);
                if (rank >= 4) _spinV = 720f;
                foreach (Rigidbody g in _grains)
                {
                    if (g == null) continue;
                    g.AddExplosionForce((0.004f + 0.006f * rank) * _k, _glass.position, 0.3f * _k, 1.5f, ForceMode.Impulse);
                }
                if (rank >= 4) foreach (Collider w in _glassWalls) if (w != null) w.enabled = false;
            });
            if (rank >= 4)
            {
                Cue(pop + 0.1f, () => { _orrery = true; _birdTarget = 1f; });
                Cue(pop + 0.5f, () => Play(bellBig, 1f, 1f));
                Cue(pop + 1.1f, () => Play(cuckoo, 0.8f, 1f));
                Cue(pop + 1.9f, () => Play(cuckoo, 0.8f, 1.06f));
            }
            else if (rank >= 2) Cue(pop + 0.6f, () => { _birdTarget = 1f; Play(cuckoo, 0.6f, 1f); });
        }

        /// <summary>Cú bật/nảy của cả rương (mô phỏng rơi tự do + nảy), xoay khi bay, xẹp lệch nếu Rác.</summary>
        private void UpdateHop(float dt)
        {
            if (!_locked) return;
            if (_hopV != 0f || _hopY > 0f)
            {
                _hopV -= 9.81f * dt;
                _hopY += _hopV * dt;
                if (_hopY <= 0f)
                {
                    _hopY = 0f;
                    bool hard = _hopV < -1f;
                    _hopV = hard ? -_hopV * 0.35f : 0f;
                    if (hard) { Play(gearClank, 0.6f, 0.6f); _twitch += 2f; _squash = 0.08f; }
                }
            }
            if (_spinV > 0f) { float d = Mathf.Min(_spinV * dt, 360f - _spin); _spin += d; if (_spin >= 360f) { _spin = 0f; _spinV = 0f; } }
            if (_tier == ChestTier.Rac)
            {
                _slump = Mathf.MoveTowards(_slump, 1f, dt * 2f);
                _tilt = Mathf.MoveTowards(_tilt, 7f, dt * 14f);
                _squash = Mathf.MoveTowards(_squash, 0.1f, dt * 0.5f);
                if (_glass != null) _glass.localRotation = Quaternion.Slerp(_glass.localRotation, Quaternion.Euler(0f, 0f, -80f), dt * 3f);
            }
            else _squash = Mathf.MoveTowards(_squash, 0f, dt * 0.4f);
        }

        private void Play(AudioClip clip, float vol, float pitch)
        {
            if (clip == null || _sfx == null) return;
            // AudioSource.pitch áp cho cả nguồn: dùng nguồn tạm cho cao độ khác 1 để không bóp méo tiếng khác đang kêu.
            if (Mathf.Abs(pitch - 1f) < 0.01f) { _sfx.PlayOneShot(clip, vol * volume); return; }
            AudioSource s = _root.gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false; s.spatialBlend = 0.5f; s.pitch = pitch;
            s.PlayOneShot(clip, vol * volume);
            Destroy(s, clip.length / Mathf.Max(0.1f, pitch) + 0.1f);
        }
    }
}

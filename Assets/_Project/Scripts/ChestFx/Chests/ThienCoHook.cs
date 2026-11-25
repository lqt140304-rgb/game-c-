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
    /// - Con lắc thật (Rigidbody + HingeJoint) được đẩy mỗi nhịp tắc.
    /// - Chuông điểm giờ chỉ 2 mốc: 50 % (1 tiếng) và 85 % (2 tiếng) thời gian chờ — GIỐNG NHAU mọi bậc, giữ bí mật tới lúc chốt.
    /// - Chốt bậc (OnTierLocked):
    ///   Rác: tích tắc dừng, lặng 0,2 s, một tiếng xịt; LÒ XO BUNG: bánh răng thành vật thể tự do văng tứ tung, con lắc đứt rơi,
    ///        rương xẹp lệch, chim cu ló ra gáy méo rồi rũ xuống.
    ///   Thường..Huyền thoại: tiếng chốt theo bậc (stingClips), lò xo bật rương nhảy lên (cao theo hạng, theo chiều cao rương).
    ///        Rất hiếm+: thêm tiếng phóng. Huyền thoại: rương xoay một vòng, bánh răng tách ra quay quanh rương như mô hình
    ///        thiên văn (≤ 3 s rồi về chỗ), 12 bánh răng nhỏ bắn ra từ miệng rương.
    /// Đã thu gọn theo kế hoạch 2026-11-25 (bỏ đồng hồ cát, chuông còn 2 mốc, âm ElevenLabs theo A1–A11).
    /// Chỉ TRANG TRÍ: không đọc/đổi gì ngoài RolledTier lúc đã chốt; ngẫu nhiên dùng System.Random theo hạt giống.
    /// </summary>
    public class ThienCoHook : ChestFxHook
    {
        [Header("Âm (ThienCoFxSetup điền từ Audio/Chests/thien_co/*.mp3, theo Tools/Audio/elevenlabs_chest_manifest.json)")]
        [Tooltip("TC: tiếng chốt theo bậc [Rác, Thường, Hiếm, Rất hiếm, Huyền thoại] = sfx_thienco_lock_*.mp3")]
        public AudioClip[] stingClips = new AudioClip[5];
        public AudioClip tick;
        public AudioClip tock;
        public AudioClip wind;
        public AudioClip gearClank;
        public AudioClip meshOk;
        public AudioClip jam;
        public AudioClip bell;
        public AudioClip sproing;
        public AudioClip cuckoo;
        public AudioClip cuckooSad;
        public AudioClip launch;

        [Header("Nhịp")]
        [Tooltip("Hạt giống trang trí (0 = mỗi lần một khác). Không ảnh hưởng bậc.")]
        public int decorationSeed;
        [Tooltip("Số giai đoạn BoxOpenSequence khoá màu (khớp lockStage của ChestRevealSequence, mặc định 3).")]
        public int lockStageGuess = 3;
        public float tickIntervalStart = 0.65f;
        public float tickIntervalEnd = 0.14f;
        [Tooltip("Hệ số âm lượng chung, chỉ nhân MỘT lần trong Play (hồ sơ lõi đã câm).")]
        public float volume = 0.8f;
        [Tooltip("Giây chờ người chơi nhét bánh răng (kể từ lúc nó rơi) trước khi chim cu tự làm. Tối đa 3 s.")]
        [Range(0.5f, 3f)] public float autoInsertAfter = 2.2f;
        [Tooltip("Số bánh răng nhỏ bắn ra từ miệng rương ở Huyền thoại.")]
        [Range(0, 12)] public int smallGears = 12;

        // ------------------------------------------------------------------ trạng thái

        private ChestRevealSequence _reveal;
        private Transform _chest;
        private System.Random _rng;
        private bool _started;
        private float _t0, _eta;
        private Transform _root;
        private readonly List<Object> _owned = new List<Object>();
        private AudioSource _sfx;
        private float _lastEvent = -999f, _lastImpact = -999f, _lastTail = -999f;
        private int _tails;
        private float _tailFrom;

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

        // Con lắc, chim cu
        private Rigidbody _pivot, _bob;
        private HingeJoint _hinge;
        private int _kickSign = 1;
        private Transform _birdArm, _bird;
        private float _birdOut, _birdTarget, _birdDroop;

        // Âm theo nhịp
        private float _nextTick;
        private int _tickCount;
        private readonly bool[] _chimed = new bool[2];

        // Chốt và cú bật
        private bool _locked;
        private ChestTier _tier;
        private float _lockT;
        private readonly List<KeyValuePair<float, System.Action>> _cues = new List<KeyValuePair<float, System.Action>>();
        private float _hopY, _hopV, _spin, _spinV, _slump, _tilt;
        private bool _orrery;
        private float _orreryEnd;

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
            PlayEvent(wind, 0.6f, 1f);
            _nextTick = Time.time + 0.3f;
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

            Material brass = Mat(ThienCoRig.Brass, "ThienCo_Brass");
            Material dark = Mat(ThienCoRig.DarkBrass, "ThienCo_DarkBrass");
            Material steel = Mat(ThienCoRig.Steel, "ThienCo_Steel");

            // Sàn và thân rương vô hình để vật rơi có chỗ chạm (sàn chỉ đủ rộng cho vùng ≤ 1,5 S).
            ThienCoRig.StaticBox("Floor", _root, Vector3.down * 0.02f, Quaternion.identity, new Vector3(3f, 0.04f, 3f) * _k);
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
            UpdateBird(dt);
            RunCues(now);
            UpdateHop(dt);
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
                PlayBeat(isTick ? tick : tock, Mathf.Lerp(0.3f, 0.65f, u), pitch);
                _twitch += 0.6f + u;
                if (_slot != Slot.Meshed) _driveAngle += isTick ? 4f : -4f;   // thiếu khớp: chỉ lắc qua lại
                if (!isTick) KickPendulum(u);
                _tickCount++;
                float ease = 1f - (1f - u) * (1f - u);
                _nextTick += Mathf.Lerp(tickIntervalStart, tickIntervalEnd, ease);
            }
            float[] marks = { 0.5f, 0.85f };          // 2 mốc: 1 tiếng rồi 2 tiếng (cách nhau 0,22 s > cổng 0,2 s)
            for (int i = 0; i < 2; i++)
            {
                if (_chimed[i] || u < marks[i]) continue;
                _chimed[i] = true;
                for (int s = 0; s <= i; s++) { int n = s; Cue(now + 0.22f * n, () => { if (!_locked) PlayEvent(bell, 0.45f, 1f + 0.12f * n); }); }
            }
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
                    else if (now - _slotT >= autoInsertAfter) { _birdTarget = 1f; if (_birdOut > 0.9f) { PlayEvent(cuckoo, 0.6f, 1f); BeginInsert(); } }
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
                        if (_wrongFirst) { _wrongFirst = false; _slot = Slot.Jammed; _slotT = now; PlayEvent(jam, 0.6f, 1.1f); }
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
            imp.onHit = (i, speed) => PlayImpact(gearClank, Mathf.Clamp01(speed * 0.4f), 1f + (float)_rng.NextDouble() * 0.15f);
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
            if (!_locked) PlayEvent(meshOk, 0.6f, _byPlayer ? 1.1f : 1f);
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
                if (t >= _orreryEnd)
                {
                    // Hết 3 s: ba bánh răng về đúng chỗ trên mặt rương và đứng yên (vật thừa ≤ 4 s sau chốt).
                    _orrery = false;
                    RestGear(_a); RestGear(_b); RestGear(_c);
                    return;
                }
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

        private void RestGear(Gear g)
        {
            g.t.localPosition = g == _b ? _slotPos : g.home;
            g.t.localRotation = Quaternion.identity;
        }

        private void OrbitGear(Gear g, float t, float phase, float speed)
        {
            float ang = phase + t * speed * 2f;
            float rad = (_halfW + 0.18f * _k) * Mathf.Min(1f, t * 1.5f);   // vào dần, ≤ 1,5 S
            Vector3 c = Vector3.up * (_height * 0.6f + _hopY);
            Vector3 target = c + new Vector3(Mathf.Cos(ang) * rad, Mathf.Sin(t * 2f + phase) * 0.05f * _k, Mathf.Sin(ang) * rad);
            target = Vector3.Lerp(g == _b ? _slotPos : g.home, target, Mathf.Clamp01((_orreryEnd - t) / 0.6f));   // 0,6 s cuối: về chỗ cũ trên mặt rương
            g.t.localPosition = Vector3.Lerp(g.t.localPosition, target, Time.deltaTime * 6f);
            g.t.localRotation = Quaternion.AngleAxis(t * 400f * speed, Vector3.forward) * Quaternion.Euler(0f, -ang * Mathf.Rad2Deg, 0f);
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

        /// <summary>
        /// Bậc Rác (A4): tích tắc đã dừng (không đặt thêm nhịp), lặng 0,2 s, MỘT tiếng xịt (stingClips[0]) cùng lúc lò xo bung;
        /// 0,6 s sau đó không phát gì; đuôi: sproing, chim cu méo. Không có tiếng hành động.
        /// </summary>
        private void PlanFlop()
        {
            float t = Time.time;
            _driveSpeed = 0f;
            _twitch += 6f;
            _tailFrom = t + ChestAudioRule.RacSilence + 0.6f;
            Cue(t + ChestAudioRule.RacSilence, () =>
            {
                PlaySting(0);
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
                        g.t.gameObject.AddComponent<ThienCoImpact>().onHit = (i, s) => PlayImpact(gearClank, Mathf.Clamp01(s * 0.3f), 0.8f + (float)_rng.NextDouble() * 0.3f);
                    g.rb.AddExplosionForce(1.2f * _k, boom, 1f * _k, 0.6f, ForceMode.Impulse);
                    g.rb.AddTorque(new Vector3((float)_rng.NextDouble() - 0.5f, (float)_rng.NextDouble() - 0.5f, (float)_rng.NextDouble() - 0.5f) * 0.008f, ForceMode.Impulse);
                }
                if (_hinge != null) { Destroy(_hinge); _hinge = null; }       // con lắc đứt
                _hopV = 0.6f;
            });
            Cue(t + 0.9f, () => PlayTail(sproing, 0.5f, 1f));
            Cue(t + 1.3f, () => { _birdTarget = 0.7f; PlayTail(cuckooSad, 0.5f, 1f); });
            Cue(t + 2.1f, () => _birdDroop = 1f);
        }

        /// <summary>
        /// Bậc Thường..Huyền thoại (A4): đúng 1 tiếng chốt TC (stingClips[bậc]) ngay lúc chốt; Rất hiếm/Huyền thoại thêm 1 tiếng
        /// hành động (launch). Lò xo bật rương nhảy (cao theo chiều cao rương), Huyền thoại xoay một vòng, bánh răng quay quanh
        /// rương 3 s rồi về chỗ, 12 bánh răng nhỏ bắn ra từ miệng. Đuôi: chim cu (Hiếm trở lên).
        /// </summary>
        private void PlanTriumph(int rank)
        {
            float t = Time.time;
            _driveSpeed = 0f;
            _tailFrom = t + 0.4f;
            PlaySting(rank);                                      // TC
            float pop = t + (rank >= 3 ? 0.2f : 0.15f);           // HĐ (launch) vẫn trong 0,3 s đầu
            Cue(pop, () =>
            {
                if (rank >= 3) PlayAction(launch, 1f, 1f);
                _squash = 0f;
                float hopH = new[] { 0f, 0.35f, 0.55f, 0.8f, 1.15f }[rank] * _height;      // ≤ 1,2 H (T8)
                _hopV = Mathf.Sqrt(2f * 9.81f * hopH);
                if (rank >= 4) { _spinV = 720f; _orrery = true; _orreryEnd = 3f; _birdTarget = 1f; SpawnSmallGears(pop); }
            });
            if (rank >= 4)
            {
                Cue(pop + 1.1f, () => PlayTail(cuckoo, 0.5f, 1f));
                Cue(pop + 1.9f, () => PlayTail(cuckoo, 0.5f, 1.06f));
            }
            else if (rank >= 2) Cue(pop + 0.6f, () => { _birdTarget = 1f; PlayTail(cuckoo, 0.5f, 1f); });
        }

        /// <summary>Huyền thoại: "mưa đồng" bằng 12 bánh răng nhỏ (0,3 cỡ bánh A) bắn từ miệng rương, rải trong 0,5 s, có trần cứng 12.</summary>
        private void SpawnSmallGears(float at)
        {
            int n = Mathf.Clamp(smallGears, 0, 12);
            if (n == 0) return;
            float r = 0.3f * 0.085f * _k;
            Mesh m = ThienCoRig.GearMesh(8, 0.22f, 0.8f, 0.18f);
            _owned.Add(m);
            Material brass = Mat(ThienCoRig.Brass, "ThienCo_SmallGear");
            for (int i = 0; i < n; i++)
            {
                int idx = i;
                Cue(at + 0.04f * i, () =>
                {
                    if (_root == null) return;
                    var go = new GameObject("SmallGear" + idx) { hideFlags = HideFlags.DontSave };
                    go.transform.SetParent(_root, false);
                    go.transform.localPosition = new Vector3(0f, _height * 0.95f, 0f) + new Vector3((float)(_rng.NextDouble() - 0.5), 0f, (float)(_rng.NextDouble() - 0.5)) * 0.05f * _k;
                    go.transform.localRotation = Quaternion.Euler((float)_rng.NextDouble() * 360f, (float)_rng.NextDouble() * 360f, 0f);
                    Transform vis = ThienCoRig.Visual("Mesh", go.transform, null, m, brass);
                    vis.localScale = Vector3.one * r * 1.08f;
                    SphereCollider col = go.AddComponent<SphereCollider>();
                    col.radius = r * 0.9f;
                    Rigidbody rb = ThienCoRig.Body(go, 0.03f, false);
                    rb.linearDamping = 0.6f;
                    Vector3 side = _right * (float)(_rng.NextDouble() - 0.5) * 1.2f + _front * (float)(_rng.NextDouble() - 0.2) * 1.0f;
                    rb.AddForce((side + Vector3.up * (1.6f + (float)_rng.NextDouble() * 0.6f)) * Mathf.Sqrt(_k), ForceMode.VelocityChange);   // ≤ 3 S
                    rb.AddTorque(new Vector3((float)_rng.NextDouble(), (float)_rng.NextDouble(), (float)_rng.NextDouble()) * 0.0005f, ForceMode.Impulse);
                });
            }
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
                    if (hard) { PlayImpact(gearClank, 0.6f, 0.6f); _twitch += 2f; _squash = 0.08f; }
                }
            }
            if (_spinV > 0f) { float d = Mathf.Min(_spinV * dt, 360f - _spin); _spin += d; if (_spin >= 360f) { _spin = 0f; _spinV = 0f; } }
            if (_tier == ChestTier.Rac)
            {
                _slump = Mathf.MoveTowards(_slump, 1f, dt * 2f);
                _tilt = Mathf.MoveTowards(_tilt, 7f, dt * 14f);
                _squash = Mathf.MoveTowards(_squash, 0.1f, dt * 0.5f);
            }
            else _squash = Mathf.MoveTowards(_squash, 0f, dt * 0.4f);
        }

        // ------------------------------------------------------------------ âm (quy tắc A1–A11, kế hoạch 2026-11-25)

        /// <summary>
        /// Chỗ DUY NHẤT nhân <see cref="volume"/> (không nhân hai lần), rồi chặn trần theo loại tiếng. Cao độ khác 1 đi qua
        /// ChestRevealSequence.PlayOneShot (lõi tạo nguồn riêng, A7).
        /// </summary>
        private void Play(AudioClip clip, float level, float pitch, float cap)
        {
            if (clip == null || _reveal == null) return;
            float v = Mathf.Min(level * volume, cap);
            if (v <= 0.001f) return;
            _reveal.PlayOneShot(clip, v, pitch);
        }

        /// <summary>Nhịp tích–tắc (≤ 0,55).</summary>
        private void PlayBeat(AudioClip clip, float level, float pitch) => Play(clip, level, pitch, ChestAudioRule.BeatMax);

        /// <summary>Sự kiện lẻ trước chốt: tối đa 1 tiếng mỗi 0,2 s (≤ 0,5). Sau chốt: bỏ.</summary>
        private void PlayEvent(AudioClip clip, float level, float pitch)
        {
            if (_locked || !ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap)) return;
            Play(clip, level, pitch, ChestAudioRule.EventMax);
        }

        /// <summary>Va chạm vật lý: cách nhau ≥ 0,12 s, ≤ 0,4. Trước chốt = sự kiện (qua cổng 0,2 s); sau chốt = đuôi.</summary>
        private void PlayImpact(AudioClip clip, float level, float pitch)
        {
            if (!ChestAudioRule.TryGate(ref _lastImpact, ChestAudioRule.ImpactGap)) return;
            if (_locked) { PlayTail(clip, Mathf.Min(level, 0.5f), pitch); return; }
            if (!ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap)) return;
            Play(clip, level, pitch, 0.4f);
        }

        /// <summary>Đuôi: từ _tailFrom (≥ +0,4 s sau chốt) tới +3 s, mỗi 0,15 s, tối đa 6 tiếng, ≤ 0,4.</summary>
        private void PlayTail(AudioClip clip, float level, float pitch)
        {
            float now = Time.time;
            if (!_locked || now < _tailFrom || now > _lockT + 3f || _tails >= 6) return;
            if (!ChestAudioRule.TryGate(ref _lastTail, ChestAudioRule.TailGap)) return;
            _tails++;
            Play(clip, level, pitch, ChestAudioRule.TailMax);
        }

        /// <summary>HĐ (tiếng hành động, ≤ 0,8): chỉ Rất hiếm trở lên.</summary>
        private void PlayAction(AudioClip clip, float level, float pitch) => Play(clip, level, pitch, ChestAudioRule.ActionMax);

        /// <summary>TC: đúng 1 tiếng chốt theo bậc (A4: Rác 0,7 · Thường 0,7 · Hiếm 0,85 · Rất hiếm/Huyền thoại 1,0 sau khi nhân volume).</summary>
        private void PlaySting(int rank)
        {
            if (stingClips == null || rank < 0 || rank >= stingClips.Length) return;
            float target = new[] { 0.7f, 0.7f, 0.85f, 1f, 1f }[rank];
            Play(stingClips[rank], target / Mathf.Max(0.01f, volume), 1f, ChestAudioRule.StingMax);
        }
    }
}

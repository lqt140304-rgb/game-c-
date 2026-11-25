using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_CoPhong — kịch bản NGỰ KIẾM TIÊN KHÍ (thay cho CoPhongHook mực tàu cũ).
    ///
    /// Ngay khi bấm mở (ChestRevealSequence.Play): tiếng "xoẹt" rút kiếm + vòng ngân kiếm (loop) bắt đầu; rương rời
    /// đất bay lơ lửng trên mây mù; 5 thanh phi kiếm từ dưới mây vút lên xoay vòng quanh rương, càng lúc càng nhanh,
    /// ngân càng cao, rương rung càng mạnh. 4 lá bùa phong ấn dán quanh rương: lần lượt một thanh kiếm tách vòng lao
    /// vào đâm, lá bùa bật ra (Rigidbody, lực thật) và cháy thành tro.
    ///
    /// Tương tác phụ: người chơi bấm chuột trái hoặc phím G (hoặc gọi <see cref="DropSpiritStone"/>) để thả linh thạch
    /// từ trên xuống (tối đa <see cref="maxStones"/> viên mỗi lần mở) — viên đá rơi bằng trọng lực thật, nảy trên mây;
    /// chạm miệng rương thì bị hút: linh khí +1, kiếm xoay nhanh hơn.
    /// (Đã thu gọn theo kế hoạch 2026-11-25: bỏ hạc giấy và dải rồng khí.)
    ///
    /// Bí mật: mọi thứ trước khi chốt bậc KHÔNG phụ thuộc bậc. Ở OnTierLocked mới rẽ nhánh:
    ///  - Rác: ngân kiếm tắt phụt, lặng 0,2 s, một tiếng xịt; kiếm mất linh khí rơi tự do (Rigidbody) cắm xiên xuống
    ///    đất, rương rớt bịch xuống, bùa còn sót bay phấp phới chứ không cháy. Hài.
    ///  - Thường: kiếm rơi nhẹ cắm thẳng, rương hạ xuống êm.
    ///  - Hiếm trở lên: bùa còn lại cháy hết một lượt, kiếm dựng thành vương miện chĩa xuống rồi lần lượt lao vào
    ///    miệng rương; rương bật nhảy (Rất hiếm và Huyền thoại: xoay một vòng trên không; Huyền thoại: nhảy cao, sóng
    ///    xung kích đẩy văng mọi vật lý xung quanh).
    /// Chỉ trang trí: không đọc/ghi bậc trúng ngoài tham số OnTierLocked; ngẫu nhiên theo <see cref="decorationSeed"/>.
    /// Vị trí rương chỉ được CỘNG THÊM một độ lệch rồi trả về đúng chỗ (an toàn với rung của BoxOpenSequence).
    /// </summary>
    public class CoPhongNguKiemHook : ChestFxHook
    {
        [Header("Âm (gán bởi Tools/Cho Do La/Setup Co Phong Ngu Kiem)")]
        public AudioClip humLoop;
        public AudioClip drawSword, strike, stone, swordLand;
        [Tooltip("TC: tiếng chốt theo bậc (Rác, Thường, Hiếm, Rất hiếm, Huyền thoại) — sfx_cophong_lock_*.mp3")] public AudioClip[] stingClips = new AudioClip[5];
        [Range(0f, 1f)] public float volume = 0.8f;

        [Header("Dàn cảnh")]
        [Min(3)] public int swordCount = 5;
        [Min(0)] public int talismanCount = 4;
        [Min(0)] public int maxStones = 4;
        [Tooltip("0 = ngẫu nhiên mỗi lần")] public int decorationSeed;
        public Color bladeColor = new Color(0.78f, 0.88f, 0.95f);
        public Color qiColor = new Color(0.62f, 1f, 0.86f);
        public Color paperColor = new Color(0.96f, 0.82f, 0.32f);
        public Color inkRed = new Color(0.75f, 0.1f, 0.08f);
        public Color stoneColor = new Color(0.35f, 1f, 0.7f);

        private enum SwordState { Rising, Orbit, Strike, Return, Crown, Dive, Falling, Stuck, Gone }

        private class Sword
        {
            public Transform t;
            public Rigidbody rb;
            public TrailRenderer trail;
            public SwordState state;
            public float timer, angleOffset;
            public Vector3 from, to;
            public Talisman target;
        }

        private class Talisman
        {
            public Transform t;
            public Rigidbody rb;
            public Material mat;
            public bool hit, burning, popped, flutter;
            public float burn, flutterT;
            public Vector3 flutterVel;
        }

        private class Stone
        {
            public Transform t;
            public Rigidbody rb;
            public float age;
            public bool absorbing;
            public Vector3 absorbFrom;
            public float absorbT;
        }

        private ChestRevealSequence _reveal;
        private System.Random _rng;
        private bool _started, _locked;
        private ChestTier _tier;
        private float _time, _lockTime, _size, _groundY, _qi, _nextStrike, _orbitAngle;
        private float _stingAt = -1f, _quietUntil, _eventGate = -999f, _tailGate = -999f;
        private int _tailCount, _stonesUsed;
        private Vector3 _base, _mouth;
        private GameObject _root;
        private readonly List<Object> _owned = new List<Object>();
        private readonly List<Sword> _swords = new List<Sword>();
        private readonly List<Talisman> _talismans = new List<Talisman>();
        private readonly List<Stone> _stones = new List<Stone>();
        private AudioSource _hum;
        private ParticleSystem _mist, _ash, _sparks;
        private ChestFxProfile _mistStyle, _ashStyle, _sparkStyle;
        private Material _bladeMat, _hiltMat, _trailMat, _stoneMat, _shockMat;
        private Transform _shock;
        private float _shockT = -1f;

        // Chest motion: an offset ADDED to the chest each LateUpdate and removed again (see ApplyChestOffset).
        private float _lift, _liftVel, _spin, _spinVel;
        private Vector3 _appliedOffset, _writtenPos;
        private Quaternion _appliedRot = Quaternion.identity, _writtenRot = Quaternion.identity;
        private bool _hasWritten;
        private int _bounces;

        public float Qi => _qi;

        // ------------------------------------------------------------------ Lifecycle

        private void Awake() => _reveal = GetComponentInParent<ChestRevealSequence>();

        private void Update()
        {
            if (_reveal == null) _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null) return;
            // The hook is spawned inside Play(), so the very first frame IsPlaying is seen is the button press.
            if (!_started && _reveal.IsPlaying) Begin();
            if (!_started) return;

            float dt = Time.deltaTime;
            _time += dt;
            float tension = Tension();

            if (!_locked && PressedDrop()) DropSpiritStone();
            UpdateHum(dt, tension);
            UpdateSwords(dt, tension);
            UpdateTalismans(dt);
            UpdateStones(dt);
            UpdateSting();
            UpdateChestMotion(dt, tension);
            UpdateShock(dt);
        }

        private void LateUpdate()
        {
            if (_started) ApplyChestOffset();
        }

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            _reveal = reveal;
            if (!_started) Begin();
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (!_started) Begin();
            _locked = true;
            _tier = tier;
            _lockTime = _time;
            bool high = tier >= ChestTier.Hiem;

            // Âm chốt (A4): Rác = nền dừng ngay, lặng 0,2 s, rồi MỘT tiếng xịt, im thêm 0,6 s; bậc khác = nền dừng/fade
            // và TC phát ngay. Hook không phát HĐ nào (danh mục không có tiếng hành động cho rương này).
            if (_hum != null && (tier == ChestTier.Rac || high)) _hum.Stop();
            _stingAt = _time + (tier == ChestTier.Rac ? ChestAudioRule.RacSilence : 0f);
            _quietUntil = tier == ChestTier.Rac ? _stingAt + 0.6f : _time + 0.4f;

            // Swords
            for (int s = 0; s < _swords.Count; s++)
            {
                Sword sw = _swords[s];
                if (sw.state == SwordState.Gone || sw.state == SwordState.Stuck) continue;
                if (high)
                {
                    sw.state = SwordState.Crown;
                    sw.timer = -s * 0.07f;
                    sw.from = sw.t.position;
                }
                else
                {
                    // Qi gone: real physics takes over, the swords drop and stick in the ground.
                    sw.state = SwordState.Falling;
                    sw.rb.isKinematic = false;
                    sw.rb.useGravity = true;
                    Vector3 tangent = Vector3.Cross(Vector3.up, sw.t.position - _base).normalized;
                    float sloppy = tier == ChestTier.Rac ? 1f : 0.25f;
                    sw.rb.linearVelocity = tangent * Rand(0.3f, 1.2f) * sloppy * _size + Vector3.up * Rand(0.2f, 0.8f) * _size;
                    sw.rb.angularVelocity = new Vector3(Rand(-6f, 6f), Rand(-3f, 3f), Rand(-6f, 6f)) * sloppy;
                    if (sw.trail != null) sw.trail.emitting = false;
                }
            }

            // Talismans: high tiers burn every remaining seal at once; low tiers let them flutter off unburnt (the seal "held").
            foreach (Talisman ta in _talismans)
            {
                if (ta.popped || ta.burning) continue;   // already torn off by a sword (or burning)
                ta.hit = true;
                if (high) { ta.burning = true; ta.burn = Rand(0f, 0.2f); }
                else StartFlutter(ta);
            }

            // Chest: low tiers fall (Rác hard, with bounces), high tiers leap.
            if (high)
            {
                _liftVel = _size * (tier == ChestTier.HuyenThoai ? 3.4f : tier == ChestTier.RatHiem ? 2.6f : 1.9f);
                if (tier >= ChestTier.RatHiem) _spinVel = tier == ChestTier.HuyenThoai ? 720f : 360f;   // °/s; cả hai xoay đúng 1 vòng (T8 ≤ 540°)
                if (tier == ChestTier.HuyenThoai) StartShock();
                BurstSparks(_reveal.MouthPosition, color, tier == ChestTier.HuyenThoai ? 80 : 40);
            }
            else
            {
                _liftVel = tier == ChestTier.Rac ? -_size * 0.6f : 0f;
                _bounces = tier == ChestTier.Rac ? 2 : 0;
            }
        }

        public override void OnRevealed(ChestRevealSequence reveal)
        {
            if (_hum != null) _hum.Stop();
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDisable() => Cleanup();

        private void OnDestroy() => Cleanup();

        // ------------------------------------------------------------------ Setup

        private void Begin()
        {
            if (_started || _reveal == null) return;
            _started = true;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);
            _time = 0f;
            _qi = 0f;
            _orbitAngle = 0f;
            _stingAt = -1f; _quietUntil = 0f; _eventGate = -999f; _tailGate = -999f; _tailCount = 0; _stonesUsed = 0;

            Transform chest = _reveal.transform;
            Bounds b = new Bounds(chest.position, Vector3.zero);
            bool any = false;
            foreach (Renderer r in chest.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled || r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
                if (any) b.Encapsulate(r.bounds); else { b = r.bounds; any = true; }
            }
            _size = Mathf.Max(0.2f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)));
            _base = new Vector3(chest.position.x, b.min.y, chest.position.z);
            _groundY = b.min.y;
            _mouth = _reveal.MouthPosition;

            _root = Own(new GameObject("CoPhongNguKiem")) as GameObject;
            _root.hideFlags = HideFlags.DontSave;
            _root.transform.position = _base;

            _bladeMat = OwnMat(ChestFxFactory.CreateUnlit(bladeColor, false, "NguKiemBlade"));
            _hiltMat = OwnMat(ChestFxFactory.CreateUnlit(new Color(0.35f, 0.12f, 0.08f), false, "NguKiemHilt"));
            _trailMat = OwnMat(ChestFxFactory.CreateAdditive(ChestFxFactory.GetTexture(ParticleLook.SoftDot), "NguKiemTrail"));
            _stoneMat = OwnMat(ChestFxFactory.CreateUnlit(stoneColor, false, "NguKiemStone"));

            BuildGround();
            BuildMist();
            BuildSwords();
            BuildTalismans();

            _hum = _root.AddComponent<AudioSource>();
            _hum.playOnAwake = false;
            _hum.loop = true;
            _hum.clip = humLoop;
            _hum.volume = 0f;
            _hum.pitch = 0.8f;
            _hum.spatialBlend = 0f;
            if (humLoop != null) _hum.Play();
            if (ChestAudioRule.TryGate(ref _eventGate, ChestAudioRule.EventGap)) PlayCapped(drawSword, 0.5f, ChestAudioRule.EventMax);

            _nextStrike = 1.6f;
            _lift = 0f; _liftVel = 0f; _spin = 0f; _spinVel = 0f; _bounces = 0;
            _hasWritten = false;
            _appliedOffset = Vector3.zero;
            _appliedRot = Quaternion.identity;
        }

        private void BuildGround()
        {
            // Invisible floor collider under the "clouds" so stones and paper land on something (the scene may have none).
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Own(g);
            g.name = "NguKiemCloudFloor";
            g.hideFlags = HideFlags.DontSave;
            g.transform.SetParent(_root.transform, false);
            g.transform.position = new Vector3(_base.x, _groundY - 0.05f, _base.z);
            g.transform.localScale = new Vector3(_size * 8f, 0.1f, _size * 8f);
            Renderer r = g.GetComponent<Renderer>();
            if (r != null) r.enabled = false;
        }

        private void BuildMist()
        {
            _mistStyle = Style(ParticleLook.SoftDot, ParticleDirection.Radial, 0.05f, 0.35f * _size, 3.5f);
            _mistStyle.ambientRate = 18f;
            _mist = ChestFxFactory.CreateParticles("NguKiemMist", _root.transform, _mistStyle, _size * 3f, false);
            ParticleSystem.ShapeModule shape = _mist.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = _size * 1.3f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            _mist.transform.position = new Vector3(_base.x, _groundY + 0.02f, _base.z);
            ChestFxFactory.SetParticleColor(_mist, new Color(0.9f, 0.93f, 1f), _mistStyle, 0.35f);
            ParticleSystem.EmissionModule em = _mist.emission;
            em.rateOverTime = _mistStyle.ambientRate;
            _mist.Play();
            _mist.Emit(15);   // a bank of cloud already there at the press

            _ashStyle = Style(ParticleLook.SoftDot, ParticleDirection.Up, 0.15f, 0.025f, 1.6f);
            _ash = ChestFxFactory.CreateParticles("NguKiemAsh", _root.transform, _ashStyle, _size * 0.3f, true);
            ChestFxFactory.SetParticleColor(_ash, new Color(1f, 0.55f, 0.15f), _ashStyle, 0.8f);

            _sparkStyle = Style(ParticleLook.Sparkle, ParticleDirection.Radial, 1.2f, 0.04f, 1.2f);
            _sparks = ChestFxFactory.CreateParticles("NguKiemSparks", _root.transform, _sparkStyle, _size, true);
        }

        private void BuildSwords()
        {
            float L = _size * 0.55f;
            for (int i = 0; i < swordCount; i++)
            {
                GameObject root = Own(new GameObject("PhiKiem" + i)) as GameObject;
                root.hideFlags = HideFlags.DontSave;
                root.transform.SetParent(_root.transform, false);
                // Local +Z = tip direction. Blade, guard, hilt from primitives (their own colliders removed).
                Part(root.transform, new Vector3(0f, 0f, L * 0.2f), new Vector3(L * 0.07f, L * 0.016f, L * 0.8f), _bladeMat);
                Part(root.transform, new Vector3(0f, 0f, -L * 0.2f), new Vector3(L * 0.22f, L * 0.03f, L * 0.035f), _hiltMat);
                Part(root.transform, new Vector3(0f, 0f, -L * 0.32f), new Vector3(L * 0.04f, L * 0.04f, L * 0.22f), _hiltMat);
                BoxCollider col = root.AddComponent<BoxCollider>();
                col.isTrigger = true;   // swords pass through the cloud floor so they can stick IN it
                col.size = new Vector3(L * 0.1f, L * 0.03f, L);
                Rigidbody rb = root.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.mass = 0.4f;

                GameObject tip = new GameObject("Tip");
                tip.transform.SetParent(root.transform, false);
                tip.transform.localPosition = new Vector3(0f, 0f, L * 0.55f);
                TrailRenderer tr = tip.AddComponent<TrailRenderer>();
                tr.sharedMaterial = _trailMat;
                tr.time = 0.35f;
                tr.startWidth = L * 0.09f;
                tr.endWidth = 0f;
                tr.startColor = qiColor;
                tr.endColor = new Color(qiColor.r, qiColor.g, qiColor.b, 0f);
                tr.minVertexDistance = L * 0.05f;

                Sword s = new Sword { t = root.transform, rb = rb, trail = tr, state = SwordState.Rising, timer = -i * 0.09f };
                s.angleOffset = 360f * i / swordCount;
                // Start hidden under the clouds, below their orbit slot.
                s.t.position = OrbitPos(s, 0f) + Vector3.down * _size * 1.2f;
                s.t.rotation = Quaternion.LookRotation(Vector3.up);
                s.from = s.t.position;
                _swords.Add(s);
            }
        }

        private void BuildTalismans()
        {
            Bounds b = ChestBounds();
            for (int i = 0; i < talismanCount; i++)
            {
                float a = (i + 0.5f) / talismanCount * Mathf.PI * 2f + Rand(-0.15f, 0.15f);
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                float rx = b.extents.x, rz = b.extents.z;
                float k = 1f / Mathf.Max(0.001f, Mathf.Max(Mathf.Abs(dir.x) / Mathf.Max(0.01f, rx), Mathf.Abs(dir.z) / Mathf.Max(0.01f, rz)));
                Vector3 pos = new Vector3(b.center.x, b.min.y + b.size.y * Rand(0.35f, 0.65f), b.center.z) + dir * (k + 0.004f);

                GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Own(q);
                q.name = "BuaPhongAn" + i;
                q.hideFlags = HideFlags.DontSave;
                q.transform.SetParent(_root.transform, true);
                q.transform.position = pos;
                q.transform.rotation = Quaternion.LookRotation(-dir) * Quaternion.Euler(0f, 0f, Rand(-12f, 12f));
                q.transform.localScale = new Vector3(_size * 0.12f, _size * 0.3f, 1f);
                Collider c = q.GetComponent<Collider>();
                if (c != null) Destroy(c);
                BoxCollider bc = q.AddComponent<BoxCollider>();
                bc.size = new Vector3(1f, 1f, 0.02f);
                Material m = OwnMat(ChestFxFactory.CreateUnlit(paperColor, false, "NguKiemBua"));
                q.GetComponent<Renderer>().sharedMaterial = m;
                // Red ink stroke down the middle of the seal.
                GameObject stroke = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Own(stroke);
                Collider sc = stroke.GetComponent<Collider>();
                if (sc != null) Destroy(sc);
                stroke.transform.SetParent(q.transform, false);
                stroke.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                stroke.transform.localScale = new Vector3(0.22f, 0.8f, 1f);
                stroke.GetComponent<Renderer>().sharedMaterial = OwnMat(ChestFxFactory.CreateUnlit(inkRed, false, "NguKiemBuaMuc"));
                Rigidbody rb = q.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.mass = 0.02f;
                rb.linearDamping = 3f;           // paper: falls slowly, flutters
                rb.angularDamping = 0.5f;
                _talismans.Add(new Talisman { t = q.transform, rb = rb, mat = m });
            }
        }

        // ------------------------------------------------------------------ Per-frame

        // 0..1 rising suspense: time (saturating) plus qi from stones. Same for every tier until the lock.
        private float Tension()
        {
            float t = 1f - Mathf.Exp(-_time / 3.5f);
            return Mathf.Clamp01(t * 0.85f + _qi * 0.05f);
        }

        // Nền (A2): 1 vòng lặp, âm lượng 0,15 → 0,45, cao độ 0,8 → 1,2 (+50%) tới lúc chốt. `volume` nhân đúng 1 lần.
        private void UpdateHum(float dt, float tension)
        {
            if (_hum == null) return;
            if (!_locked)
            {
                _hum.volume = Mathf.Min(ChestAudioRule.BedMax, Mathf.Lerp(0.19f, 0.56f, tension) * volume);
                _hum.pitch = Mathf.Lerp(0.8f, 1.2f, tension);
                return;
            }
            // Rác và Hiếm trở lên đã Stop() ở OnTierLocked; Thường: fade 0,3 s.
            if (_tier == ChestTier.Thuong) _hum.volume = Mathf.MoveTowards(_hum.volume, 0f, dt * ChestAudioRule.BedMax / 0.3f);
            if (_hum.volume <= 0.001f && _hum.isPlaying) _hum.Stop();
        }

        // TC: đúng 1 tiếng chốt theo bậc, do hook phát (Rác trễ 0,2 s để lặng trước tiếng xịt).
        private void UpdateSting()
        {
            if (_stingAt < 0f || _time < _stingAt) return;
            _stingAt = -1f;
            int i = Mathf.Clamp(_tier.Rank(), 0, 4);
            if (stingClips == null || i >= stingClips.Length) return;
            float[] v = { 0.8f, 0.7f, 0.85f, 1f, 1f };   // Rác (xịt), Thường ≤ 0,7, Hiếm 0,85, Rất hiếm / HT 1,0
            PlayCapped(stingClips[i], v[i], ChestAudioRule.StingMax);
        }

        private Vector3 OrbitPos(Sword s, float extraHeight)
        {
            float a = (_orbitAngle + s.angleOffset) * Mathf.Deg2Rad;
            float r = _size * 0.95f;   // T2: quỹ đạo kiếm ≤ 1,2 S
            float bob = Mathf.Sin(_time * 2.3f + s.angleOffset) * _size * 0.06f;
            return _base + new Vector3(Mathf.Cos(a) * r, _size * 0.75f + _lift + bob + extraHeight, Mathf.Sin(a) * r);
        }

        private void UpdateSwords(float dt, float tension)
        {
            float speed = Mathf.Lerp(60f, 330f, tension) + _qi * 25f;   // degrees per second
            if (!_locked) _orbitAngle += speed * dt;

            // Dispatch a strike: interval shrinks with tension, always one sword at a time.
            if (!_locked && _time >= _nextStrike)
            {
                Talisman target = null;
                foreach (Talisman ta in _talismans) if (!ta.hit) { target = ta; break; }
                Sword free = null;
                foreach (Sword sw in _swords) if (sw.state == SwordState.Orbit) { free = sw; break; }
                if (target != null && free != null)
                {
                    target.hit = true;
                    free.target = target;
                    free.state = SwordState.Strike;
                    free.timer = 0f;
                    free.from = free.t.position;
                }
                _nextStrike = _time + Mathf.Lerp(1.4f, 0.55f, tension);
            }

            Vector3 mouth = _reveal.MouthPosition;
            for (int i = 0; i < _swords.Count; i++)
            {
                Sword s = _swords[i];
                s.timer += dt;
                switch (s.state)
                {
                    case SwordState.Rising:
                    {
                        float u = Mathf.Clamp01(s.timer / 0.9f);
                        Vector3 target = OrbitPos(s, 0f);
                        Vector3 p = Vector3.Lerp(s.from, target, 1f - (1f - u) * (1f - u));
                        PoseKinematic(s, p, u < 0.7f ? Vector3.up : Tangent(target));
                        if (u >= 1f) s.state = SwordState.Orbit;
                        break;
                    }
                    case SwordState.Orbit:
                    {
                        Vector3 p = OrbitPos(s, 0f);
                        PoseKinematic(s, p, Tangent(p));
                        break;
                    }
                    case SwordState.Strike:
                    {
                        float u = Mathf.Clamp01(s.timer / 0.28f);
                        Vector3 aim = s.target.t.position;
                        Vector3 p = Vector3.Lerp(s.from, aim, u * u);
                        PoseKinematic(s, p, aim - s.from);
                        if (u >= 1f)
                        {
                            HitTalisman(s.target);
                            s.state = SwordState.Return;
                            s.timer = 0f;
                            s.from = s.t.position;
                        }
                        break;
                    }
                    case SwordState.Return:
                    {
                        float u = Mathf.Clamp01(s.timer / 0.45f);
                        Vector3 target = OrbitPos(s, 0f);
                        PoseKinematic(s, Vector3.Lerp(s.from, target, Mathf.SmoothStep(0f, 1f, u)), Tangent(target));
                        if (u >= 1f) s.state = SwordState.Orbit;
                        break;
                    }
                    case SwordState.Crown:
                    {
                        // High tiers: form an upright ring above the mouth, tips down, then dive in one by one.
                        if (s.timer < 0f) break;
                        float u = Mathf.Clamp01(s.timer / 0.5f);
                        float a = (s.angleOffset + _orbitAngle * 0.2f) * Mathf.Deg2Rad;
                        Vector3 slot = mouth + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * _size * 0.45f + Vector3.up * _size * 0.6f;
                        PoseKinematic(s, Vector3.Lerp(s.from, slot, Mathf.SmoothStep(0f, 1f, u)), Vector3.Slerp(s.t.forward, Vector3.down, u));
                        if (s.timer > 0.75f + i * 0.08f)
                        {
                            s.state = SwordState.Dive;
                            s.timer = 0f;
                            s.from = s.t.position;
                        }
                        break;
                    }
                    case SwordState.Dive:
                    {
                        float u = Mathf.Clamp01(s.timer / 0.22f);
                        PoseKinematic(s, Vector3.Lerp(s.from, mouth, u * u), mouth - s.from);
                        s.t.localScale = Vector3.one * (1f - u * 0.9f);
                        if (u >= 1f)
                        {
                            s.state = SwordState.Gone;
                            s.t.gameObject.SetActive(false);
                            BurstSparks(mouth, qiColor, 8);
                            PlayTail(strike, 0.35f, 1.4f);
                        }
                        break;
                    }
                    case SwordState.Falling:
                    {
                        // Physics drives it; when the tip pierces the cloud floor, it sticks (kinematic, kept tilted).
                        Vector3 tip = s.t.position + s.t.forward * _size * 0.3f;
                        Vector3 tail = s.t.position - s.t.forward * _size * 0.3f;
                        float lowest = Mathf.Min(tip.y, tail.y);
                        if (lowest <= _groundY)
                        {
                            // Rotate so the lower end leads, push it a little into the ground, freeze.
                            Vector3 down = tip.y < tail.y ? s.t.forward : -s.t.forward;
                            s.rb.isKinematic = true;
                            s.rb.useGravity = false;
                            Vector3 lean = Vector3.Slerp(Vector3.down, down, _tier == ChestTier.Rac ? 0.7f : 0.25f).normalized;
                            s.t.rotation = Quaternion.LookRotation(lean);
                            s.t.position = new Vector3(s.t.position.x, _groundY + _size * 0.12f, s.t.position.z);
                            s.state = SwordState.Stuck;
                            s.timer = 0f;
                            PlayTail(swordLand, 0.4f, Rand(0.85f, 1.15f));
                            _ash.transform.position = new Vector3(s.t.position.x, _groundY, s.t.position.z);
                            ChestFxFactory.SetParticleColor(_ash, new Color(0.85f, 0.85f, 0.9f), _ashStyle, 0.5f);
                            _ash.Emit(6);
                        }
                        break;
                    }
                    case SwordState.Stuck:
                    {
                        // A little spring wobble after it sticks.
                        float w = Mathf.Sin(s.timer * 30f) * Mathf.Exp(-s.timer * 5f) * 8f;
                        s.t.rotation = s.t.rotation * Quaternion.Euler(w * dt * 10f, 0f, 0f);
                        break;
                    }
                }
            }
        }

        private void PoseKinematic(Sword s, Vector3 pos, Vector3 forward)
        {
            s.t.position = pos;
            if (forward.sqrMagnitude > 1e-6f) s.t.rotation = Quaternion.LookRotation(forward.normalized);
        }

        private Vector3 Tangent(Vector3 p)
        {
            Vector3 r = p - _base;
            r.y = 0f;
            return Vector3.Cross(Vector3.up, r).normalized + Vector3.down * 0.15f;
        }

        private void HitTalisman(Talisman ta)
        {
            // Nhịp (A2): âm lượng 0,25 → 0,55 theo độ căng; khoảng giữa hai cú đâm đã 1,4 → 0,55 s.
            PlayCapped(strike, Mathf.Lerp(0.3f, 0.69f, Tension()), ChestAudioRule.BeatMax, Rand(0.9f, 1.1f));
            PopTalisman(ta, 1f);
            ta.burning = true;
            ta.burn = 0f;
            _lift -= _size * 0.03f;   // the chest jolts from the blow
            _liftVel -= _size * 0.25f;
        }

        // Real physics: the seal tears off the chest and is flung out and up, spinning.
        private void PopTalisman(Talisman ta, float strength)
        {
            ta.popped = true;
            ta.t.SetParent(_root.transform, true);
            ta.rb.isKinematic = false;
            ta.rb.useGravity = true;
            Vector3 outDir = ta.t.position - _base;
            outDir.y = 0f;
            outDir = outDir.normalized;
            ta.rb.AddForce((outDir * 0.9f + Vector3.up * 1.4f) * _size * 0.06f * strength, ForceMode.Impulse);
            ta.rb.angularVelocity = new Vector3(Rand(-8f, 8f), Rand(-8f, 8f), Rand(-8f, 8f)) * strength;
        }

        private void UpdateTalismans(float dt)
        {
            foreach (Talisman ta in _talismans)
            {
                if (ta.flutter && ta.t != null) UpdateFlutter(ta, dt);
                if (!ta.burning || ta.t == null || !ta.t.gameObject.activeSelf) continue;
                ta.burn += dt / 0.9f;
                float u = Mathf.Clamp01(ta.burn);
                // paper -> flame orange -> char black, shrinking from the edges
                Color c = u < 0.4f ? Color.Lerp(paperColor, new Color(1f, 0.45f, 0.1f), u / 0.4f)
                                   : Color.Lerp(new Color(1f, 0.45f, 0.1f), new Color(0.08f, 0.07f, 0.06f), (u - 0.4f) / 0.6f);
                ChestFxFactory.SetColor(ta.mat, c);
                Vector3 sc = ta.t.localScale;
                float k = Mathf.Lerp(1f, 0.05f, u * u);
                ta.t.localScale = new Vector3(_size * 0.12f * k, _size * 0.3f * k, 1f);
                if (Rand(0f, 1f) < 0.5f)
                {
                    _ash.transform.position = ta.t.position;
                    ChestFxFactory.SetParticleColor(_ash, new Color(1f, 0.55f, 0.15f), _ashStyle, 0.8f);
                    _ash.Emit(1);
                }
                if (u >= 1f) ta.t.gameObject.SetActive(false);
            }
        }

        // Rác/Thường: bùa chưa bị đâm "còn nguyên" — bay phấp phới bằng chuyển động theo mã (không thêm Rigidbody, T3).
        private void StartFlutter(Talisman ta)
        {
            ta.popped = true;
            ta.rb.isKinematic = true;
            ta.t.SetParent(_root.transform, true);
            Vector3 outDir = ta.t.position - _base;
            outDir.y = 0f;
            outDir = outDir.normalized;
            ta.flutterVel = (outDir * 0.5f + Vector3.up * 0.9f) * _size * 0.5f;
            ta.flutterT = 0f;
            ta.flutter = true;
        }

        private void UpdateFlutter(Talisman ta, float dt)
        {
            ta.flutterT += dt;
            ta.flutterVel += Vector3.down * 9.81f * _size * 0.25f * dt;
            ta.flutterVel *= Mathf.Exp(-1.5f * dt);
            Vector3 sway = new Vector3(Mathf.Sin(ta.flutterT * 7f), 0f, Mathf.Cos(ta.flutterT * 6f)) * _size * 0.25f;
            Vector3 p = ta.t.position + (ta.flutterVel + sway) * dt;
            if (p.y <= _groundY + 0.01f) { p.y = _groundY + 0.01f; ta.flutter = false; }   // lands on the clouds and stays
            ta.t.position = p;
            ta.t.rotation = ta.t.rotation * Quaternion.Euler(new Vector3(120f, 90f, 60f) * dt);
        }

        // ------------------------------------------------------------------ Spirit stones (player)

        /// <summary>Thả một viên linh thạch từ trên xuống rương (UI có thể gọi). Chỉ trang trí: tăng linh khí, không đổi bậc.</summary>
        public void DropSpiritStone()
        {
            if (!_started || _locked || _stonesUsed >= maxStones) return;   // trần cứng mỗi lần mở (T3/T7)
            _stonesUsed++;
            Vector3 from = _reveal.MouthPosition + Vector3.up * _size * 1.3f
                           + new Vector3(Rand(-0.35f, 0.35f), 0f, Rand(-0.35f, 0.35f)) * _size;
            SpawnStone(from, Vector3.zero);
        }

        private void SpawnStone(Vector3 from, Vector3 velocity)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Own(g);
            g.name = "LinhThach";
            g.hideFlags = HideFlags.DontSave;
            g.transform.SetParent(_root.transform, true);
            g.transform.position = from;
            g.transform.localScale = new Vector3(0.7f, 1.2f, 0.7f) * _size * 0.08f;   // a long crystal-ish drop
            g.transform.rotation = Quaternion.Euler(Rand(0f, 360f), Rand(0f, 360f), 0f);
            g.GetComponent<Renderer>().sharedMaterial = _stoneMat;
            Rigidbody rb = g.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.useGravity = true;
            rb.linearVelocity = velocity;
            rb.angularVelocity = new Vector3(Rand(-4f, 4f), Rand(-4f, 4f), Rand(-4f, 4f));
            _stones.Add(new Stone { t = g.transform, rb = rb });
        }

        private void UpdateStones(float dt)
        {
            Vector3 mouth = _reveal.MouthPosition;
            float catchR = _size * 0.45f;
            for (int i = _stones.Count - 1; i >= 0; i--)
            {
                Stone s = _stones[i];
                if (s.t == null) { _stones.RemoveAt(i); continue; }
                s.age += dt;
                if (!s.absorbing)
                {
                    bool near = (s.t.position - mouth).sqrMagnitude < catchR * catchR;
                    if (near || s.age > 2.4f || _locked)
                    {
                        // Caught by the chest's pull (or, after bouncing on the clouds a while, sucked in anyway).
                        s.absorbing = true;
                        s.absorbFrom = s.t.position;
                        s.absorbT = 0f;
                        s.rb.isKinematic = true;
                    }
                    continue;
                }
                s.absorbT += dt / 0.3f;
                float u = Mathf.Clamp01(s.absorbT);
                s.t.position = Vector3.Lerp(s.absorbFrom, mouth, u * u) + Vector3.up * Mathf.Sin(u * Mathf.PI) * _size * 0.2f;
                if (u >= 1f)
                {
                    if (!_locked)
                    {
                        _qi += 1f;
                        if (ChestAudioRule.TryGate(ref _eventGate, ChestAudioRule.EventGap)) PlayCapped(stone, 0.5f, ChestAudioRule.EventMax, 1f + _qi * 0.06f);
                        _liftVel += _size * 0.5f;   // a little hop of joy
                    }
                    BurstSparks(mouth, stoneColor, 14);
                    s.t.gameObject.SetActive(false);
                    _stones.RemoveAt(i);
                }
            }
        }

        // ------------------------------------------------------------------ Whole-chest motion

        private void UpdateChestMotion(float dt, float tension)
        {
            float hover = _size * 0.35f;   // floating height above the clouds
            if (!_locked)
            {
                // Spring towards the hover height (rises over ~1 s after the press).
                float target = hover * Mathf.Clamp01(_time / 1.2f);
                _liftVel += ((target - _lift) * 30f - _liftVel * 6f) * dt;
                _lift += _liftVel * dt;
                _spin = Mathf.Sin(_time * 1.7f) * 4f * tension;   // uneasy sway
                return;
            }
            float since = _time - _lockTime;
            if (_tier >= ChestTier.Hiem)
            {
                // Ballistic leap, then a spring back down to the floor.
                if (since < 1.4f)
                {
                    _liftVel -= 9.81f * _size * 0.9f * dt;
                    _lift += _liftVel * dt;
                    if (_lift < 0f) { _lift = 0f; _liftVel = -_liftVel * 0.25f; }
                }
                else
                {
                    _liftVel += ((0f - _lift) * 40f - _liftVel * 8f) * dt;
                    _lift += _liftVel * dt;
                }
                if (_spinVel > 0f)
                {
                    float step = Mathf.Min(_spinVel * dt, 360f - _spin);
                    if (step > 0f) _spin += step; else _spinVel = 0f;
                }
                else _spin = Mathf.MoveTowards(_spin, Mathf.Round(_spin / 360f) * 360f, dt * 90f);
            }
            else
            {
                // Low tiers: gravity drops it; Rác bounces twice ("bịch, bịch").
                _liftVel -= 9.81f * _size * 1.2f * dt;
                _lift += _liftVel * dt;
                if (_lift <= 0f)
                {
                    _lift = 0f;
                    if (_bounces > 0 && _liftVel < -0.05f)
                    {
                        _bounces--;
                        _liftVel = -_liftVel * 0.35f;
                        _ash.transform.position = new Vector3(_base.x, _groundY, _base.z);
                        ChestFxFactory.SetParticleColor(_ash, new Color(0.8f, 0.8f, 0.8f), _ashStyle, 0.5f);
                        _ash.Emit(12);
                    }
                    else _liftVel = 0f;
                }
                if (_tier == ChestTier.Thuong) { _liftVel = Mathf.Max(_liftVel, -_size * 0.7f); }   // soft landing
                _spin = Mathf.MoveTowards(_spin, 0f, dt * 30f);
            }
            if (_mist != null && since > 0.2f)
            {
                ParticleSystem.EmissionModule em = _mist.emission;
                em.rateOverTime = _mistStyle.ambientRate * Mathf.Clamp01(1f - since / 2f);
            }
        }

        // Adds the hover/jump/spin offset on top of whatever the chest's pose is this frame (BoxOpenSequence may have
        // written its shake); removes our last offset first only if nobody rewrote the pose since we did.
        private void ApplyChestOffset()
        {
            Transform t = _reveal != null ? _reveal.transform : null;
            if (t == null) return;
            float jitter = _locked ? 0f : Tension() * _size * 0.008f;
            Vector3 offset = Vector3.up * _lift + new Vector3(Mathf.PerlinNoise(_time * 25f, 0.1f) - 0.5f, 0f, Mathf.PerlinNoise(0.7f, _time * 25f) - 0.5f) * 2f * jitter;
            Quaternion rot = Quaternion.AngleAxis(_spin, Vector3.up);

            Vector3 basePos = t.position;
            Quaternion baseRot = t.rotation;
            if (_hasWritten && t.position == _writtenPos) basePos = _writtenPos - _appliedOffset;
            if (_hasWritten && Quaternion.Angle(t.rotation, _writtenRot) < 0.01f) baseRot = Quaternion.Inverse(_appliedRot) * _writtenRot;
            t.SetPositionAndRotation(basePos + offset, rot * baseRot);
            _writtenPos = t.position;
            _writtenRot = t.rotation;
            _appliedOffset = offset;
            _appliedRot = rot;
            _hasWritten = true;
        }

        private void RestoreChest()
        {
            if (!_hasWritten || _reveal == null) return;
            Transform t = _reveal.transform;
            if (t.position == _writtenPos) t.position = _writtenPos - _appliedOffset;
            if (Quaternion.Angle(t.rotation, _writtenRot) < 0.01f) t.rotation = Quaternion.Inverse(_appliedRot) * _writtenRot;
            _hasWritten = false;
        }

        // ------------------------------------------------------------------ Shockwave (Huyền thoại)

        private void StartShock()
        {
            if (_shock == null)
            {
                Mesh ring = ChestFxFactory.CreateRingMesh(0.85f, 48);
                _owned.Add(ring);
                _shockMat = OwnMat(ChestFxFactory.CreateUnlit(new Color(1f, 0.9f, 0.5f), false, "NguKiemShock"));
                GameObject g = Own(new GameObject("SongXungKich")) as GameObject;
                g.hideFlags = HideFlags.DontSave;
                g.transform.SetParent(_root.transform, false);
                g.AddComponent<MeshFilter>().sharedMesh = ring;
                g.AddComponent<MeshRenderer>().sharedMaterial = _shockMat;
                _shock = g.transform;
            }
            _shock.position = new Vector3(_base.x, _groundY + 0.01f, _base.z);
            _shock.gameObject.SetActive(true);
            _shockT = 0f;
            // Physics: everything loose round the chest (stuck swords excepted) is blown outward and up.
            foreach (Talisman ta in _talismans)
                if (ta.rb != null && !ta.rb.isKinematic) ta.rb.AddForce(Blast(ta.t.position) * 0.05f, ForceMode.Impulse);
            foreach (Stone s in _stones)
                if (s.rb != null && !s.rb.isKinematic) s.rb.AddForce(Blast(s.t.position) * 0.4f, ForceMode.Impulse);
        }

        private Vector3 Blast(Vector3 p)
        {
            Vector3 d = p - _base;
            d.y = 0f;
            return (d.normalized + Vector3.up * 0.8f) * _size;
        }

        private void UpdateShock(float dt)
        {
            if (_shock == null || _shockT < 0f) return;
            _shockT += dt / 0.8f;
            float r = Mathf.Lerp(0.2f, 2.8f, 1f - (1f - _shockT) * (1f - _shockT)) * _size;
            _shock.localScale = new Vector3(r, 1f, r);
            if (_shockT >= 1f) { _shock.gameObject.SetActive(false); _shockT = -1f; }
        }

        // ------------------------------------------------------------------ Helpers

        private void BurstSparks(Vector3 at, Color c, int count)
        {
            if (_sparks == null) return;
            _sparks.transform.position = at;
            ChestFxFactory.SetParticleColor(_sparks, c, _sparkStyle, 1f);
            _sparks.Emit(count);
        }

        // `volume` của hook nhân đúng MỘT lần ở đây; kết quả không vượt trần `cap` của ChestAudioRule (A6).
        private void PlayCapped(AudioClip clip, float vol, float cap, float pitch = 1f)
        {
            if (clip != null && _reveal != null) _reveal.PlayOneShot(clip, Mathf.Min(cap, vol * volume), pitch);
        }

        // Đuôi (A5): từ +0,4 s sau chốt (Rác: sau lặng + xịt + 0,6 s), ≥ 0,15 s giữa hai tiếng, tối đa 6 tiếng, hết trước +3 s, ≤ 0,4.
        private void PlayTail(AudioClip clip, float vol, float pitch = 1f)
        {
            if (!_locked || clip == null) return;
            float since = _time - _lockTime;
            if (_time < _quietUntil || since > 3f || _tailCount >= 6) return;
            if (!ChestAudioRule.TryGate(ref _tailGate, ChestAudioRule.TailGap)) return;
            _tailCount++;
            PlayCapped(clip, vol, ChestAudioRule.TailMax, pitch);
        }

        private Bounds ChestBounds()
        {
            Bounds b = new Bounds(_base + Vector3.up * _size * 0.5f, Vector3.one * _size);
            bool any = false;
            foreach (Renderer r in _reveal.transform.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled || r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
                if (any) b.Encapsulate(r.bounds); else { b = r.bounds; any = true; }
            }
            return b;
        }

        private void Part(Transform parent, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Own(p);
            Collider c = p.GetComponent<Collider>();
            if (c != null) Destroy(c);
            p.transform.SetParent(parent, false);
            p.transform.localPosition = localPos;
            p.transform.localScale = scale;
            Renderer r = p.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private float Rand(float a, float b) => a + (float)(_rng != null ? _rng.NextDouble() : 0.5) * (b - a);

        private Object Own(Object o) { _owned.Add(o); return o; }

        private Material OwnMat(Material m) { _owned.Add(m); return m; }

        private bool PressedDrop()
        {
#if ENABLE_INPUT_SYSTEM
            return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                   || (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame);
#else
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.G);
#endif
        }

        private ChestFxProfile Style(ParticleLook look, ParticleDirection dir, float speed, float size, float life)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.secondaryColor = Color.white;
            s.secondaryMix = 0.15f;
            s.burstGravity = 0.3f;
            _owned.Add(s);
            return s;
        }

        private void Cleanup()
        {
            RestoreChest();
            if (_hum != null) _hum.Stop();
            foreach (ParticleSystem ps in new[] { _mist, _ash, _sparks })
                if (ps != null) { ParticleSystemRenderer pr = ps.GetComponent<ParticleSystemRenderer>(); if (pr != null && pr.sharedMaterial != null) Destroy(pr.sharedMaterial); }
            for (int i = _owned.Count - 1; i >= 0; i--) if (_owned[i] != null) Destroy(_owned[i]);
            _owned.Clear();
            _swords.Clear();
            _talismans.Clear();
            _stones.Clear();
            _root = null;
            _hum = null;
            _mist = _ash = _sparks = null;
            _shock = null;
            _started = _locked = false;
            _qi = 0f;
            _lift = _liftVel = _spin = _spinVel = 0f;
            _stingAt = -1f;
        }
    }
}

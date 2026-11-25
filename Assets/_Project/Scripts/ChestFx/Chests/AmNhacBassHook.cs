using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Kịch bản riêng Chest_AmNhac: "LOA BASS KẸO KÉO".
    ///
    /// Rương biến thành một cái loa sub: ngay khi game gọi <see cref="ChestRevealSequence.Play"/> (người chơi bấm mở),
    /// một nhịp trống bắt đầu, nhanh dần (96 → ~150 BPM) và dày dần (kick → hi-hat → clap). Mỗi tiếng kick:
    /// cả rương nén–bật như màng loa (scale gốc), một vòng sóng âm lan trên sàn, và mọi Rigidbody quanh rương bị
    /// đẩy nảy lên (vật lý thật: AddForce Impulse). Sự kiện phụ: một "loa kẹo kéo" (vật thể Rigidbody có bánh)
    /// tự lăn vào cạnh rương, cắm điện, rít feedback rồi đập theo nhịp. Một đĩa than lơ lửng trên rương: người chơi
    /// bấm V (hoặc click vào nó) để thả, đĩa rơi thật xuống mâm → kim đặt xuống, mở thêm lớp bass "wub". Không thả
    /// thì đĩa tự rơi ở giây ~5. Người chơi bấm B đúng phách → scratch + sóng mạnh hơn ("độ quẩy" tăng).
    ///
    /// Kết: giống hệt nhau cho mọi bậc tới lúc khóa (giữ bí mật). Khi khóa:
    ///  - Rác: đĩa dừng (record-stop), phích cắm bật ra khỏi loa kẹo kéo, loa đổ nghiêng, rương xẹp "xịt" — tắt nhạc.
    ///  - Thường: "drop" hụt: một tiếng pụp nhỏ, nhịp đi tiếp nhỏ dần.
    ///  - Hiếm / Rất hiếm / Huyền thoại: im 0,35 s (rương hút vào), rồi DROP: bùng nổ bass, đẩy văng mọi vật,
    ///    loa kẹo kéo bị hất bay, nhịp đập tối đa; Rất hiếm+ thêm đám đông, Huyền thoại thêm còi hơi.
    ///
    /// Bất biến: chỉ trang trí. Không đọc/ghi gì làm thay đổi bậc; độ quẩy của người chơi chỉ đổi độ mạnh hiệu ứng.
    /// Ngẫu nhiên (vị trí vật, góc bánh...) lấy từ hạt giống cố định <see cref="seed"/>.
    /// </summary>
    public class AmNhacBassHook : ChestFxHook
    {
        [System.Serializable]
        public class Clips
        {
            public AudioClip kick, hat, clap, wub, scratch, skip, needle, riser;
            public AudioClip drop, horn, recordStop, deflate, plugPop, wheels, feedback, crowd;
        }

        [Header("Âm (Audio/Chests/am_nhac/seq/, tạo bằng Tools/Audio/make_amnhac_sfx.py)")]
        public Clips clips = new Clips();
        [Range(0f, 1f)] public float masterVolume = 0.9f;

        [Header("Nhịp")]
        public float startBpm = 96f;
        public float topBpm = 150f;
        [Tooltip("Giây để tempo đi được ~63% quãng đường từ startBpm tới topBpm.")] public float tempoRiseSeconds = 4f;
        [Tooltip("Cửa sổ chấm 'đúng phách' khi bấm B (giây, mỗi phía).")] public float hitWindow = 0.11f;
        [Tooltip("Giây mà đĩa than tự rơi nếu người chơi chưa thả.")] public float autoDropVinylAt = 5f;
        [Tooltip("Giây loa kẹo kéo lăn vào.")] public float speakerArrivesAt = 1.2f;

        [Header("Vật lý")]
        [Tooltip("Độ cao nảy (m, trên 1 m kích thước rương) của vật quanh rương mỗi kick, lúc đầu và lúc quẩy tối đa.")]
        public Vector2 hopHeight = new Vector2(0.015f, 0.07f);
        public int propCount = 7;
        [Tooltip("Đẩy cả Rigidbody có sẵn trong cảnh quanh rương (không kinematic, không phải người chơi).")] public bool shakeSceneBodies = true;
        public int seed = 3301;

        // ---------------------------------------------------------------- runtime
        private ChestRevealSequence _reveal;
        private bool _running;
        private float _t0;
        private float _size = 0.4f;
        private Vector3 _base;             // world: chest base centre
        private Vector3 _rootScale;
        private Transform _root;           // our objects
        private AudioSource _audio;
        private System.Random _rng;

        private int _beat;
        private float _nextBeat;
        private float _lastBeat;
        private float _beatLen = 0.6f;
        private float _bpmBoost;

        private float _hype;               // 0..1, player's on-beat taps + vinyl
        private int _combo;
        private bool _vinylOn;
        private bool _riserPlayed;
        private float _lastStepTime = -1f;
        private int _steps;

        private enum End { None, Gap, Drop, Fizzle, Small }
        private End _end = End.None;
        private ChestTier _tier;
        private float _endT;
        private float _outroUntil;
        private bool _beatOn;

        // chest "speaker cone" spring
        private float _s, _v, _sTarget;

        private readonly List<Rigidbody> _bodies = new List<Rigidbody>();
        private readonly List<Rigidbody> _props = new List<Rigidbody>();
        private readonly List<Material> _mats = new List<Material>();
        private readonly List<Mesh> _meshes = new List<Mesh>();

        // loa kẹo kéo
        private Rigidbody _speaker;
        private Transform[] _cones;
        private Rigidbody _plug;
        private bool _speakerDocked;
        private Vector3 _speakerDock;

        // vinyl
        private AmNhacVinyl _vinyl;
        private Transform _record;
        private Quaternion _recordRot;
        private float _recordAngle;
        private float _scratchT = -10f;

        // rings + light
        private Transform[] _rings;
        private Material[] _ringMats;
        private float[] _ringT;
        private int _ringNext;
        private Light _light;

        public void SetClips(Clips c) => clips = c;

        private void Awake()
        {
            _reveal = GetComponentInParent<ChestRevealSequence>();
        }

        // ---------------------------------------------------------------- hooks from ChestRevealSequence

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            // The roulette slows down before it stops: once the gaps get long, play the riser. Same for every tier.
            float now = Time.time;
            if (_lastStepTime > 0f && now - _lastStepTime > 0.27f && _steps > 6 && !_riserPlayed && _running)
            {
                _riserPlayed = true;
                Play(clips.riser, 0.7f);
            }
            _lastStepTime = now;
            _steps++;
            if (_light != null) _light.color = color;
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (!_running) Begin();
            _tier = tier;
            _endT = Time.time;
            switch (tier)
            {
                case ChestTier.Rac: Fizzle(); break;
                case ChestTier.Thuong: SmallDrop(); break;
                default:
                    _end = End.Gap;           // silence, chest sucks in; Drop() fires in Update
                    _beatOn = false;
                    _sTarget = -0.16f;
                    break;
            }
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        // ---------------------------------------------------------------- start

        private void Begin()
        {
            if (_reveal == null) _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null) return;
            Cleanup();
            _running = true;
            _t0 = Time.time;
            _rng = new System.Random(seed);
            _beat = 0;
            _nextBeat = _t0 + 0.05f;     // first kick right away: sound starts on the press
            _lastBeat = _t0;
            _hype = 0f; _combo = 0; _vinylOn = false; _riserPlayed = false; _steps = 0; _lastStepTime = -1f;
            _end = End.None; _beatOn = true; _outroUntil = float.MaxValue;
            _s = _v = _sTarget = 0f;
            _speakerDocked = false;
            _scratchT = -10f;
            _recordAngle = 0f;

            Measure();
            _rootScale = _reveal.transform.localScale;
            _root = new GameObject("AmNhacBass").transform;
            _root.position = _base;
            _root.rotation = _reveal.transform.rotation;

            _audio = _root.gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;

            BuildGround();
            BuildRings();
            BuildLight();
            BuildProps();
            BuildVinyl();
            if (shakeSceneBodies) CollectSceneBodies();

            Transform box = _reveal.Box != null ? _reveal.Box.transform : _reveal.transform;
            _record = box.Find("Record");
            if (_record != null) _recordRot = _record.localRotation;
        }

        private void Measure()
        {
            bool has = false;
            Bounds b = new Bounds(_reveal.transform.position, Vector3.zero);
            foreach (Renderer r in _reveal.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer) continue;
                if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
            }
            _size = has ? Mathf.Max(0.1f, Mathf.Max(b.size.x, b.size.z)) : 0.4f;
            _base = has ? new Vector3(b.center.x, b.min.y, b.center.z) : _reveal.transform.position;
        }

        // ---------------------------------------------------------------- frame

        private void Update()
        {
            if (!_running)
            {
                // ChestFxHook has no "Play pressed" callback: watch IsPlaying so the beat starts on the press itself,
                // seconds before the lid cracks or the roulette begins.
                if (_reveal != null && _reveal.IsPlaying && !_reveal.IsLocked) Begin();
                return;
            }
            float now = Time.time;
            float t = now - _t0;

            // Safety: nobody locked after 25 s (no box, broken flow): stop.
            if (_end == End.None && t > 25f) { _beatOn = false; _outroUntil = now; }

            if (_end == End.Gap && now - _endT >= 0.35f) Drop();

            // beat clock
            float bpm = Mathf.Lerp(startBpm, topBpm, 1f - Mathf.Exp(-t / Mathf.Max(0.1f, tempoRiseSeconds))) + _bpmBoost;
            if (_end == End.Drop) bpm = topBpm + 4f;
            _beatLen = 60f / Mathf.Max(30f, bpm);
            if (_beatOn && now >= _nextBeat)
            {
                OnBeat(t);
                _lastBeat = _nextBeat;
                _nextBeat += _beatLen;
                if (_nextBeat < now) _nextBeat = now + _beatLen;   // hitch: don't machine-gun
            }
            if (_beatOn && now > _outroUntil) _beatOn = false;

            // off-beat layers (half a beat after the last one)
            float half = _lastBeat + _beatLen * 0.5f;
            if (_beatOn && _halfPending && now >= half)
            {
                _halfPending = false;
                if (_beat >= 4 || _end == End.Drop) Play(clips.hat, 0.35f + 0.25f * Intensity());
                if (_vinylOn) Play(clips.wub, 0.55f + 0.3f * Intensity());
            }

            Speaker(t);
            Vinyl(t);
            ReadInput(now);
            Spring();
            Rings();
            if (_light != null) _light.intensity = Mathf.Lerp(_light.intensity, _end == End.Fizzle ? 0f : 0.3f, 1f - Mathf.Exp(-8f * Time.deltaTime));
        }

        private bool _halfPending;

        private void LateUpdate()
        {
            if (!_running || _record == null) return;
            // The turntable spins from the very first beat (BoxOpenSequence only spins it once the glow is up).
            float rpm = _end == End.Fizzle ? 0f : 33f + 12f * Intensity();
            if (_end == End.Fizzle) _recordAngle += 200f * Mathf.Exp(-(Time.time - _endT) * 2.5f) * Time.deltaTime;
            else _recordAngle += rpm * 6f * Time.deltaTime;
            float scratch = Time.time - _scratchT < 0.32f ? Mathf.Sin((Time.time - _scratchT) * 40f) * 35f : 0f;
            _record.localRotation = _recordRot * Quaternion.AngleAxis(_recordAngle + scratch, Vector3.up);
        }

        private float Intensity()
        {
            float t = Time.time - _t0;
            float build = Mathf.Clamp01(t / 7f);
            float x = 0.55f * build + 0.45f * _hype;
            if (_end == End.Drop) x = 1f + 0.5f * _tier.Rank01();
            return x;
        }

        private void OnBeat(float t)
        {
            _beat++;
            _halfPending = true;
            float k = Intensity();
            Play(clips.kick, Mathf.Clamp01(0.45f + 0.45f * k));
            bool clapBeat = (_beat % 2 == 0) && (_beat >= 8 || _end == End.Drop);
            if (clapBeat) Play(clips.clap, 0.4f + 0.2f * k);

            // chest = speaker cone: squash on the hit, spring back past rest
            _v -= 2.2f + 2.5f * k;
            Hop(Mathf.Lerp(hopHeight.x, hopHeight.y, Mathf.Clamp01(k)), 0.15f);
            Ring(k);
            if (_light != null)
            {
                _light.color = _beat % 2 == 0 ? AmNhacFxPreset.NeonPink : AmNhacFxPreset.NeonTeal;
                _light.intensity = 1.5f + 3f * k;
            }
        }

        // ---------------------------------------------------------------- endings

        private void Fizzle()
        {
            _end = End.Fizzle;
            _beatOn = false;
            Play(clips.recordStop, 0.9f);
            Play(clips.deflate, 0.85f, 0.25f);
            Play(clips.plugPop, 0.7f, 0.55f);
            _sTarget = -0.12f;          // the speaker sags and stays saggy
            _v -= 1.5f;
            if (_plug != null)
            {
                _plug.isKinematic = false;
                _plug.AddForce((Vector3.up * 1.2f + Away(_plug.position) * 0.8f) * _plug.mass, ForceMode.Impulse);
            }
            if (_speaker != null)
            {
                // tips over: push its top sideways
                _speaker.AddForceAtPosition(_speaker.transform.right * 0.9f * _speaker.mass, _speaker.position + Vector3.up * _size * 0.5f, ForceMode.Impulse);
            }
            if (_light != null) _light.intensity = 0f;
        }

        private void SmallDrop()
        {
            _end = End.Small;
            Play(clips.deflate, 0.45f, 0.1f);       // "pụp"
            Play(clips.scratch, 0.6f);
            _scratchT = Time.time;
            _v += 2.5f;
            Hop(hopHeight.y * 0.6f, 0.3f);
            _outroUntil = Time.time + 2.2f;
        }

        private void Drop()
        {
            _end = End.Drop;
            _beatOn = true;
            _sTarget = 0f;
            _v += 9f + 6f * _tier.Rank01();
            _nextBeat = Time.time + 0.02f;
            _outroUntil = Time.time + (_tier == ChestTier.Hiem ? 2.8f : _tier == ChestTier.RatHiem ? 4f : 5.5f);
            Play(clips.drop, 1f);
            if (_tier >= ChestTier.RatHiem) Play(clips.crowd, _tier == ChestTier.HuyenThoai ? 0.8f : 0.55f, 0.3f);
            if (_tier == ChestTier.HuyenThoai) Play(clips.horn, 0.75f, 0.15f);

            float blast = 1.2f + 1.3f * _tier.Rank01();
            foreach (Rigidbody rb in _bodies)
            {
                if (rb == null || rb.isKinematic) continue;
                Vector3 dir = (Away(rb.position) * 1.1f + Vector3.up * 1.6f).normalized;
                rb.AddForce(dir * blast * Mathf.Sqrt(_size / 0.4f) * rb.mass, ForceMode.Impulse);
                rb.AddTorque(new Vector3(R(-1, 1), R(-1, 1), R(-1, 1)) * 0.05f * rb.mass, ForceMode.Impulse);
            }
            for (int i = 0; i < 3; i++) Ring(2f);
        }

        // ---------------------------------------------------------------- physics

        private void Hop(float height, float sideways)
        {
            float g = Mathf.Abs(Physics.gravity.y) > 0.1f ? Mathf.Abs(Physics.gravity.y) : 9.81f;
            float speed = Mathf.Sqrt(2f * g * height * (_size / 0.4f));
            foreach (Rigidbody rb in _bodies)
            {
                if (rb == null || rb.isKinematic) continue;
                if (rb == _speaker && !_speakerDocked) continue;
                if (Mathf.Abs(Vel(rb).y) > 0.3f) continue;          // still in the air: only hop from the floor
                Vector3 jitter = new Vector3(R(-1, 1), 0f, R(-1, 1)) * sideways * speed;
                float m = rb == _speaker ? 0.45f : 1f;                 // the heavy speaker hops less
                rb.AddForce((Vector3.up * speed + jitter) * m * rb.mass, ForceMode.Impulse);
                rb.AddTorque(new Vector3(R(-1, 1), R(-1, 1), R(-1, 1)) * 0.01f * rb.mass, ForceMode.Impulse);
            }
        }

        private void Spring()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            float stiff = _end == End.Fizzle ? 60f : 180f;
            float damp = _end == End.Fizzle ? 6f : 9f;
            _v += (-(_s - _sTarget) * stiff - _v * damp) * dt;
            _s += _v * dt;
            _s = Mathf.Clamp(_s, -0.3f, 0.35f);
            float y = 1f + _s;
            float xz = 1f - _s * 0.45f;
            _reveal.transform.localScale = new Vector3(_rootScale.x * xz, _rootScale.y * y, _rootScale.z * xz);
        }

        private void ReadInput(float now)
        {
            if (_end != End.None) return;
            if (PressedBeat())
            {
                float sinceLast = now - _lastBeat;
                float toNext = _nextBeat - now;
                float err = Mathf.Min(Mathf.Abs(sinceLast), Mathf.Abs(toNext));
                if (err <= hitWindow)
                {
                    _combo++;
                    _hype = Mathf.Clamp01(_hype + 0.07f + 0.02f * Mathf.Min(_combo, 5));
                    _bpmBoost = Mathf.Min(8f, _bpmBoost + 0.5f);
                    Play(clips.scratch, 0.6f);
                    _scratchT = now;
                    Hop(hopHeight.y * 0.5f, 0.25f);
                    Ring(1.3f);
                }
                else
                {
                    _combo = 0;
                    _hype = Mathf.Max(0f, _hype - 0.04f);
                    Play(clips.skip, 0.5f);
                }
            }
            if (_vinyl != null && !_vinyl.Dropped && PressedVinyl()) DropVinyl();
        }

        // ---------------------------------------------------------------- loa kẹo kéo (side event)

        private void Speaker(float t)
        {
            if (_speaker == null && t >= speakerArrivesAt && _end == End.None) BuildSpeaker();
            if (_speaker == null) return;
            if (!_speakerDocked)
            {
                Vector3 to = _speakerDock - _speaker.position; to.y = 0f;
                if (to.magnitude < _size * 0.08f || t > speakerArrivesAt + 3f)
                {
                    _speakerDocked = true;
                    Vector3 v0 = Vel(_speaker); v0.x = 0f; v0.z = 0f; SetVel(_speaker, v0);
                    Play(clips.feedback, 0.35f);
                }
                else
                {
                    Vector3 v = to.normalized * Mathf.Min(1.2f * _size / 0.4f, to.magnitude * 4f);
                    v.y = Vel(_speaker).y;
                    SetVel(_speaker, v);
                }
            }
            if (_cones != null)
            {
                float since = Time.time - _lastBeat;
                float pump = _speakerDocked && _end != End.Fizzle ? Mathf.Exp(-since * 12f) * (0.25f + 0.35f * Mathf.Min(1.5f, Intensity())) : 0f;
                foreach (Transform c in _cones) c.localScale = new Vector3(c.localScale.x, _coneDepth * (1f + pump * 3f), c.localScale.z);
            }
        }

        private float _coneDepth;

        // ---------------------------------------------------------------- vinyl (player interaction)

        private void Vinyl(float t)
        {
            if (_vinyl == null) return;
            if (!_vinyl.Dropped)
            {
                _vinyl.transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.Self);
                if (t >= autoDropVinylAt && _end == End.None) DropVinyl();
            }
            if (!_vinylOn) CheckVinylLanding();
        }

        /// <summary>Any <see cref="AmNhacVinyl"/> (ours, or one the player carried in) that falls onto the platter counts.</summary>
        private void CheckVinylLanding()
        {
            Vector3 mouth = _record != null ? _record.position : _reveal.MouthPosition;
            foreach (AmNhacVinyl v in AmNhacVinyl.All)
            {
                if (v == null || v.Seated) continue;
                if (!v.Dropped && v.Body != null && v.Body.isKinematic) continue;   // still held / hovering
                Vector3 d = v.transform.position - mouth;
                Vector3 flat = new Vector3(d.x, 0f, d.z);
                if (flat.magnitude < _size * 0.4f && Mathf.Abs(d.y) < _size * 0.35f && (v.Body == null || Vel(v.Body).y <= 0.05f))
                {
                    _vinylOn = true;
                    v.Seat(_record != null ? _record : _reveal.transform, mouth);
                    Play(clips.needle, 0.8f);
                    Play(clips.scratch, 0.5f, 0.12f);
                    _scratchT = Time.time + 0.12f;
                    _hype = Mathf.Clamp01(_hype + 0.3f);
                    if (_light != null) _light.intensity = 6f;
                    return;
                }
            }
        }

        private void DropVinyl()
        {
            if (_vinyl == null || _vinyl.Dropped) return;
            _vinyl.Drop();
        }

        // ---------------------------------------------------------------- building

        private void BuildGround()
        {
            GameObject g = new GameObject("AmNhacFloor");
            g.transform.SetParent(_root, false);
            g.transform.position = _base + Vector3.down * 0.025f;
            g.transform.rotation = _root.rotation;
            BoxCollider c = g.AddComponent<BoxCollider>();
            c.size = new Vector3(_size * 12f, 0.05f, _size * 12f);
        }

        private void BuildProps()
        {
            Color[] palette = { AmNhacFxPreset.NeonPink, AmNhacFxPreset.NeonTeal, new Color(1f, 0.85f, 0.2f) };
            for (int i = 0; i < propCount; i++)
            {
                float a = (i + R(-0.25f, 0.25f)) * Mathf.PI * 2f / propCount;
                float r = _size * R(0.85f, 1.3f);
                int kind = i % 3;
                GameObject go = GameObject.CreatePrimitive(kind == 0 ? PrimitiveType.Cylinder : kind == 1 ? PrimitiveType.Cube : PrimitiveType.Sphere);
                go.name = kind == 0 ? "LonNuoc" : kind == 1 ? "BangCatXet" : "Maraca";
                go.transform.SetParent(_root, false);
                Vector3 scale = kind == 0 ? new Vector3(0.07f, 0.06f, 0.07f) : kind == 1 ? new Vector3(0.11f, 0.018f, 0.07f) : Vector3.one * 0.06f;
                scale *= _size / 0.4f;
                go.transform.localScale = scale;
                go.transform.position = _base + _root.rotation * new Vector3(Mathf.Cos(a) * r, scale.y + 0.005f, Mathf.Sin(a) * r);
                go.transform.rotation = _root.rotation * Quaternion.Euler(0f, R(0f, 360f), 0f);
                if (kind == 0)
                {
                    // cylinder primitive comes with a capsule collider: swap for a box so cans stand
                    Collider old = go.GetComponent<Collider>();
                    if (old != null) DestroyImmediate(old);
                    go.AddComponent<BoxCollider>();
                }
                Paint(go, palette[i % palette.Length]);
                Rigidbody rb = go.AddComponent<Rigidbody>();
                rb.mass = 0.15f;
                _props.Add(rb);
                _bodies.Add(rb);
            }
        }

        private void BuildSpeaker()
        {
            float k = _size / 0.4f;
            float side = R(0f, 1f) < 0.5f ? -1f : 1f;
            Vector3 right = _root.rotation * Vector3.right;
            Vector3 fwd = _root.rotation * Vector3.forward;
            _speakerDock = _base + right * side * _size * 1.15f + fwd * _size * 0.15f;
            Vector3 start = _speakerDock + right * side * _size * 2.2f;

            GameObject root = new GameObject("LoaKeoKeo");
            root.transform.SetParent(_root, false);
            root.transform.position = start + Vector3.up * 0.17f * k;
            root.transform.rotation = Quaternion.LookRotation(-right * side * 0.3f + fwd, Vector3.up);

            GameObject body = Part(root.transform, PrimitiveType.Cube, "Than", new Vector3(0f, 0f, 0f), new Vector3(0.22f, 0.32f, 0.16f) * k, new Color(0.12f, 0.12f, 0.14f), true);
            BoxCollider col = body.GetComponent<BoxCollider>();
            if (col == null) body.AddComponent<BoxCollider>();
            _cones = new Transform[2];
            _coneDepth = 0.02f * k;
            for (int i = 0; i < 2; i++)
            {
                GameObject cone = Part(root.transform, PrimitiveType.Cylinder, "Mang" + i, new Vector3(0f, (i == 0 ? 0.07f : -0.06f) * k, 0.085f * k),
                    new Vector3((i == 0 ? 0.09f : 0.15f) * k, _coneDepth, (i == 0 ? 0.09f : 0.15f) * k), i == 0 ? AmNhacFxPreset.NeonTeal : AmNhacFxPreset.NeonPink, false);
                cone.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                _cones[i] = cone.transform;
            }
            Part(root.transform, PrimitiveType.Cube, "Quai", new Vector3(0f, 0.2f * k, -0.05f * k), new Vector3(0.16f, 0.015f, 0.015f) * k, Color.gray, false);
            for (int i = 0; i < 2; i++)
            {
                GameObject w = Part(root.transform, PrimitiveType.Cylinder, "Banh" + i, new Vector3((i == 0 ? -0.1f : 0.1f) * k, -0.17f * k, -0.05f * k),
                    new Vector3(0.05f, 0.012f, 0.05f) * k, Color.black, false);
                w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = 3f;
            _speaker = rb;
            _bodies.Add(rb);

            GameObject plug = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plug.name = "PhichCam";
            plug.transform.SetParent(root.transform, false);
            plug.transform.localPosition = new Vector3(0.12f * k, -0.05f * k, -0.06f * k);
            plug.transform.localScale = new Vector3(0.03f, 0.02f, 0.04f) * k;
            Paint(plug, new Color(1f, 0.85f, 0.2f));
            _plug = plug.AddComponent<Rigidbody>();
            _plug.mass = 0.05f;
            _plug.isKinematic = true;   // stays plugged until Rác
            Play(clips.wheels, 0.5f);
        }

        private void BuildVinyl()
        {
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "DiaThan";
            Collider old = disc.GetComponent<Collider>();
            if (old != null) DestroyImmediate(old);
            disc.AddComponent<BoxCollider>();
            float d = _size * 0.42f;
            disc.transform.localScale = new Vector3(d, 0.004f, d);
            Vector3 fwd = _root.rotation * Vector3.forward;
            disc.transform.position = _reveal.MouthPosition + Vector3.up * _size * 0.9f + fwd * _size * 0.2f;
            disc.transform.rotation = _root.rotation;
            disc.transform.SetParent(_root, true);
            Paint(disc, new Color(0.05f, 0.05f, 0.06f));
            GameObject label = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Collider lc = label.GetComponent<Collider>();
            if (lc != null) DestroyImmediate(lc);
            label.transform.SetParent(disc.transform, false);
            label.transform.localScale = new Vector3(0.35f, 1.2f, 0.35f);
            Paint(label, AmNhacFxPreset.NeonPink);
            Rigidbody rb = disc.AddComponent<Rigidbody>();
            rb.mass = 0.12f;
            rb.isKinematic = true;
            _vinyl = disc.AddComponent<AmNhacVinyl>();
            _bodies.Add(rb);
        }

        private void BuildRings()
        {
            const int n = 6;
            _rings = new Transform[n];
            _ringMats = new Material[n];
            _ringT = new float[n];
            Mesh mesh = ChestFxFactory.CreateRingMesh(0.82f);
            _meshes.Add(mesh);
            for (int i = 0; i < n; i++)
            {
                GameObject go = new GameObject("SongAm" + i);
                go.transform.SetParent(_root, false);
                go.transform.localPosition = Vector3.up * 0.004f;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                _ringMats[i] = ChestFxFactory.CreateAdditive(null, "AmNhacRing");
                _mats.Add(_ringMats[i]);
                mr.sharedMaterial = _ringMats[i];
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                go.SetActive(false);
                _rings[i] = go.transform;
                _ringT[i] = -10f;
            }
        }

        private void Ring(float strength)
        {
            if (_rings == null) return;
            int i = _ringNext++ % _rings.Length;
            _ringT[i] = Time.time;
            _rings[i].gameObject.SetActive(true);
            _rings[i].localScale = Vector3.one * 0.01f;
            _ringStrength[i] = strength;
        }

        private readonly float[] _ringStrength = new float[6];

        private void Rings()
        {
            if (_rings == null) return;
            for (int i = 0; i < _rings.Length; i++)
            {
                if (!_rings[i].gameObject.activeSelf) continue;
                float a = (Time.time - _ringT[i]) / 0.55f;
                if (a >= 1f) { _rings[i].gameObject.SetActive(false); continue; }
                float r = _size * (0.5f + a * (1.4f + 0.8f * _ringStrength[i]));
                _rings[i].localScale = new Vector3(r, 1f, r);
                Color c = (i % 2 == 0 ? AmNhacFxPreset.NeonPink : AmNhacFxPreset.NeonTeal) * ((1f - a) * Mathf.Min(1.5f, 0.5f + 0.5f * _ringStrength[i]));
                ChestFxFactory.SetColor(_ringMats[i], c);
            }
        }

        private void BuildLight()
        {
            GameObject g = new GameObject("AmNhacStrobe");
            g.transform.SetParent(_root, false);
            g.transform.localPosition = Vector3.up * _size * 1.2f;
            _light = g.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.range = _size * 6f;
            _light.intensity = 0f;
            _light.shadows = LightShadows.None;
        }

        private void CollectSceneBodies()
        {
            foreach (Collider c in Physics.OverlapSphere(_base, _size * 2.5f))
            {
                Rigidbody rb = c.attachedRigidbody;
                if (rb == null || rb.isKinematic || _bodies.Contains(rb)) continue;
                if (rb.transform.IsChildOf(_reveal.transform)) continue;
                if (rb.GetComponent<CharacterController>() != null || rb.mass > 5f) continue;
                _bodies.Add(rb);
            }
        }

        private GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 local, Vector3 scale, Color color, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (!keepCollider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) DestroyImmediate(c);
            }
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localScale = scale;
            Paint(go, color);
            return go;
        }

        private void Paint(GameObject go, Color c)
        {
            Material m = ChestFxFactory.CreateUnlit(c, false, "AmNhac_" + go.name);
            _mats.Add(m);
            go.GetComponent<Renderer>().sharedMaterial = m;
        }

        // ---------------------------------------------------------------- helpers

        private void Play(AudioClip clip, float volume, float delay = 0f)
        {
            if (clip == null || _audio == null) return;
            float v = Mathf.Clamp01(volume) * masterVolume;
            if (delay <= 0f) { _audio.PlayOneShot(clip, v); return; }
            // Short delayed one-shot on its own source (PlayOneShot has no delay).
            AudioSource s = _root.gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.clip = clip;
            s.volume = v;
            s.PlayDelayed(delay);
            Destroy(s, delay + clip.length + 0.1f);
        }

        private Vector3 Away(Vector3 p)
        {
            Vector3 d = p - _base; d.y = 0f;
            return d.sqrMagnitude > 1e-6f ? d.normalized : Vector3.forward;
        }

        private float R(float a, float b) => a + (float)(_rng != null ? _rng.NextDouble() : 0.5) * (b - a);

        private static Vector3 Vel(Rigidbody rb)
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }

        private static void SetVel(Rigidbody rb, Vector3 v)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = v;
#else
            rb.velocity = v;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private static bool PressedBeat() => Keyboard.current != null && Keyboard.current[Key.B].wasPressedThisFrame;
        private bool PressedVinyl()
        {
            if (Keyboard.current != null && Keyboard.current[Key.V].wasPressedThisFrame) return true;
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && ClickedVinyl(Mouse.current.position.ReadValue());
        }
#else
        private static bool PressedBeat() => Input.GetKeyDown(KeyCode.B);
        private bool PressedVinyl() => Input.GetKeyDown(KeyCode.V) || (Input.GetMouseButtonDown(0) && ClickedVinyl(Input.mousePosition));
#endif

        private bool ClickedVinyl(Vector3 screen)
        {
            Camera cam = Camera.main;
            if (cam == null || _vinyl == null) return false;
            return Physics.Raycast(cam.ScreenPointToRay(screen), out RaycastHit hit, 50f) && hit.collider != null && hit.collider.transform == _vinyl.transform;
        }

        private void Cleanup()
        {
            if (_reveal != null && _running) _reveal.transform.localScale = _rootScale;
            foreach (Material m in _mats) if (m != null) Destroy(m);
            foreach (Mesh m in _meshes) if (m != null) Destroy(m);
            _mats.Clear(); _meshes.Clear(); _bodies.Clear(); _props.Clear();
            if (_record != null) _record.localRotation = _recordRot;
            // our disc may have been re-parented onto the record when it landed
            if (_vinyl != null) Destroy(_vinyl.gameObject);
            if (_root != null) Destroy(_root.gameObject);
            _root = null; _audio = null; _speaker = null; _cones = null; _plug = null; _vinyl = null; _rings = null; _light = null; _record = null;
            _running = false;
            _end = End.None;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// "RƯƠNG BIẾN HÌNH" — kịch bản mở riêng của Chest_Robot (thay cho RobotHook cũ chỉ có mắt + bánh răng).
    ///
    /// Nhịp (rương không chìa, thời gian giai đoạn mặc định; xem docs/superpowers/specs/2026-11-25-kichban-robot-design.md):
    ///  0,0 s  BẤM MỞ: "cạch" + chirp khởi động, máy rì rì bắt đầu ở pitch thấp. Khe sạc mở trên lưng rương, thanh pin 5 ô hiện ra.
    ///  0,5–3  SẠC: bu-lông, bánh răng rơi từ trên xuống (Rigidbody thật, va chạm, bị khe hút như nam châm); một DRONE phụ tá bay
    ///         tới thả cục pin vào khe rồi bay ra đứng xem. Người chơi cũng có thể thả/ném vật có Rigidbody vào khe: vật có
    ///         <see cref="RobotChargePart"/> bị nuốt, đồ lạ thì bị "hút điện" rồi nhả văng ra. Mỗi lần trúng: keng + xẹt, cả rương
    ///         nảy lên, máy rú cao thêm một nấc.
    ///  ~3,7   KHỞI ĐỘNG (vòng màu bắt đầu): hai CHÂN duỗi ra từ đáy, rương ĐỨNG DẬY, dậm chân tại chỗ ngày càng nhanh; hai mắt nháy
    ///         theo màu vòng quay; màn chữ chạy tự kiểm hệ thống; tiếng bíp dồn dập, máy rú lên (pitch tới 1,7). Mọi bậc giống hệt
    ///         nhau tới đây: GIỮ BÍ MẬT.
    ///  ~6,5   KHOÁ BẬC:
    ///         Rác  -> XỊT: máy tụt pitch rồi tắt, chập mạch xẹt xẹt, khói, chân sụm, rương đổ nghiêng, bu-lông bung văng
    ///                 (Rigidbody), một tay thò ra rồi rũ xuống, drone bay tới chọc chọc rồi chính nó cũng hết pin rơi bịch.
    ///         Thường -> "tạm ổn": hắt hơi khói nhỏ rồi nhảy khẽ, tay đưa quà.
    ///         Hiếm+ -> BÙNG NỔ: rương bật nhảy bằng Rigidbody thật (cao dần theo bậc), Huyền thoại bay lơ lửng phun lửa ở chân;
    ///                 hai tay vươn đỡ món quà; mưa bu-lông ăn mừng; drone bay vòng quanh.
    /// Không bao giờ đọc/đổi bậc ngoài tham số <c>tier</c> mà lõi đưa vào OnTierLocked. Ngẫu nhiên chỉ để trang trí (hạt giống).
    /// Chuyển động cả rương: cộng một độ lệch vào localPosition/localRotation trong LateUpdate và gỡ ra ở Update khung sau,
    /// nên không đánh nhau với cú rung của BoxOpenSequence.
    /// </summary>
    public class RobotTransformHook : ChestFxHook
    {
        [Header("Âm (RobotTransformSetup tự điền từ Audio/Chests/robot/transform/)")]
        public AudioClip pressClip;
        public AudioClip humLoopClip;
        public AudioClip slotClip;
        public AudioClip beepClip;
        public AudioClip servoClip;
        public AudioClip typeClip;
        public AudioClip droneLoopClip;
        public AudioClip fizzleClip;
        [Tooltip("Bùng nổ cho Thường, Hiếm, Rất hiếm, Huyền thoại (4 file).")]
        public AudioClip[] blastClips = new AudioClip[4];
        [Range(0f, 1f)] public float volume = 0.8f;

        [Header("Trang trí")]
        [Tooltip("0 = mỗi lần một khác. Chỉ đổi vị trí rơi, độ văng... không đổi bậc.")]
        public int decorationSeed;
        public Color bodyMetal = new Color(0.55f, 0.6f, 0.66f);
        public Color darkMetal = new Color(0.18f, 0.2f, 0.24f);
        public Color eyeColor = new Color(0.35f, 0.95f, 1f);
        [Tooltip("Số vật tự rơi vào khe (ngoài cục pin của drone).")]
        public int autoParts = 3;

        // ------------------------------------------------------------ trạng thái
        private enum Phase { Idle, Charging, Booting, Finale }
        private Phase _phase = Phase.Idle;
        private ChestRevealSequence _reveal;
        private Transform _chest;
        private System.Random _rng;
        private float _t0, _bootT0, _lockT0;
        private float _charge;
        private ChestTier _tier;
        private bool _wasPlaying;

        // kích thước rương (đo lúc bắt đầu, toạ độ cục bộ của rương + cỡ thế giới)
        private Vector3 _localBottom, _localTop;
        private float _w = 0.5f, _d = 0.44f, _h = 0.55f;

        // độ lệch cả rương
        private Vector3 _appliedPos;
        private Quaternion _appliedRot = Quaternion.identity;
        private bool _hasApplied;
        private float _stand, _standTarget, _legLen = 0.16f;
        private float _bounce, _bounceVel;
        private float _tiltZ, _tiltZVel, _tiltTarget;
        private float _shake;
        private Rigidbody _jumpBody;
        private float _jumpY0, _jumpPrevY, _jumpOffset;
        private bool _hoverDone;

        // dựng hình
        private readonly List<Object> _owned = new List<Object>();
        private Transform _legL, _legR, _footL, _footR, _armL, _armR, _handL, _handR, _eyeL, _eyeR;
        private Material _metalMat, _darkMat, _eyeMat, _meterOff;
        private readonly List<Renderer> _meter = new List<Renderer>();
        private readonly List<Material> _meterMats = new List<Material>();
        private Light _eyeLight;
        private GameObject _socket;
        private float _arm, _armTarget;   // 0 = gập trong thân, 1 = vươn đủ
        private bool _armLimp;
        private TextMesh _screen;
        private readonly List<string> _lines = new List<string>();
        private string _typing = "";
        private float _typeClock;
        private string _scanLine;

        // vật lý phụ
        private readonly List<Rigidbody> _parts = new List<Rigidbody>();
        private Transform _drone;
        private Rigidbody _droneFallBody;
        private Transform _droneRotorA, _droneRotorB;
        private Rigidbody _droneCargo;
        private enum DroneMode { None, Arrive, Drop, Leave, Watch, Celebrate, Poke, Dead }
        private DroneMode _droneMode;
        private float _droneT;
        private Vector3 _droneWatchPos;

        // âm
        private AudioSource _hum, _droneSrc, _sfx;
        private float _beepClock;

        // hạt
        private ParticleSystem _sparks, _smoke, _flame;
        private ChestFxProfile _sparkStyle, _smokeStyle, _flameStyle;

        // ================================================================== vòng đời

        private void OnEnable()
        {
            _reveal = GetComponentInParent<ChestRevealSequence>();
            // Gắn qua customFxPrefab: được sinh ra SAU Play, nên tự bắt đầu ngay.
            if (_reveal != null && _reveal.IsPlaying && _phase == Phase.Idle) Begin();
        }

        private void Update()
        {
            RemoveOffset();
            if (_reveal == null) _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null) return;

            // Âm bắt đầu NGAY KHI BẤM MỞ: bắt cạnh lên của IsPlaying (Play() vừa được gọi), không đợi nắp hé.
            bool playing = _reveal.IsPlaying;
            if (playing && !_wasPlaying && _phase == Phase.Idle) Begin();
            _wasPlaying = playing;
            if (_phase == Phase.Idle) return;

            float dt = Time.deltaTime;
            float now = Time.time;
            UpdateAudio(now, dt);
            UpdateDrone(now, dt);
            UpdateTyping(dt);
            UpdateSprings(dt);
            UpdateJump();
            _arm = Mathf.MoveTowards(_arm, _armTarget, dt * (_armLimp ? 1.2f : 2.2f));
            _stand = Mathf.MoveTowards(_stand, _standTarget, dt * (_standTarget < _stand && _phase == Phase.Finale ? 6f : 1.4f));
        }

        private void FixedUpdate()
        {
            if (_phase != Phase.Charging && _phase != Phase.Booting) return;
            if (_socket == null) return;
            // Khe sạc hút nhẹ như nam châm, để vật rơi trúng kể cả khi rương đang cầm trên tay và lắc.
            Vector3 target = _socket.transform.position;
            for (int i = _parts.Count - 1; i >= 0; i--)
            {
                Rigidbody rb = _parts[i];
                if (rb == null) { _parts.RemoveAt(i); continue; }
                if (rb.isKinematic) continue;
                Vector3 to = target - rb.transform.position;
                float dist = to.magnitude;
                if (dist < 1.2f) rb.AddForce(to.normalized * (2.5f / Mathf.Max(0.15f, dist)) * rb.mass, ForceMode.Force);
            }
        }

        private void LateUpdate()
        {
            if (_phase == Phase.Idle || _chest == null) return;
            ApplyOffset();
            PlaceRig();
        }

        private void OnDisable() => Cleanup();
        private void OnDestroy() => Cleanup();

        // ================================================================== móc của lõi

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            _reveal = reveal;
            if (_phase == Phase.Idle) Begin();
            StartBoot();
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            SetEyes(color, _eyeLight != null && _eyeLight.intensity > 1.5f ? 0.6f : 2.6f);
            if (_scanLine != null) SetScan("QUÉT QUÀ: " + new string('▒', 1 + _rng.Next(5)));
            Kick(0.15f, (float)(_rng.NextDouble() - 0.5) * 2f);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_phase == Phase.Idle) Begin();
            _tier = tier;                 // chỉ ĐỌC bậc lõi đưa vào
            _phase = Phase.Finale;
            _lockT0 = Time.time;
            if (_socket != null) _socket.SetActive(false);
            StopAllCoroutines();
            if (tier == ChestTier.Rac) StartCoroutine(Fizzle(color));
            else StartCoroutine(Triumph(tier, color));
        }

        public override void OnSilhouetteComplete(ChestRevealSequence reveal, Transform silhouette)
        {
            if (_tier != ChestTier.Rac) _armTarget = 1f;
        }

        public override void OnRevealed(ChestRevealSequence reveal)
        {
            if (_tier == ChestTier.Rac) Type("> ...zzz (thôi, cầm tạm)");
            else Type("> GIAO QUÀ XONG :)");
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        // ================================================================== các pha

        private void Begin()
        {
            if (_reveal == null) _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null) return;
            Cleanup();
            _chest = _reveal.transform;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);
            _phase = Phase.Charging;
            _t0 = Time.time;
            _charge = 0f;
            _wasPlaying = true;
            Measure();
            BuildRig();
            BuildAudio();
            Play(pressClip, 1f, 1f);
            Type("> YÊU CẦU MỞ... CẦN SẠC PIN");
            Kick(0.5f, 0f);
            StartCoroutine(AutoFeed());
        }

        private IEnumerator AutoFeed()
        {
            yield return new WaitForSeconds(0.45f);
            SpawnPart(RobotChargePart.Kind.BuLong, 0.12f);
            yield return new WaitForSeconds(0.35f);
            LaunchDrone();
            yield return new WaitForSeconds(0.9f);
            for (int i = 1; i < autoParts; i++)
            {
                SpawnPart(i % 2 == 1 ? RobotChargePart.Kind.BanhRang : RobotChargePart.Kind.BuLong, 0.15f);
                yield return new WaitForSeconds(0.45f + 0.2f * (float)_rng.NextDouble());
            }
        }

        private void StartBoot()
        {
            if (_phase == Phase.Booting || _phase == Phase.Finale) return;
            _phase = Phase.Booting;
            _bootT0 = Time.time;
            if (_charge < 1f)
            {
                // Sạc nhanh: phần còn thiếu nạp nốt bằng một tia điện (không ảnh hưởng bậc, chỉ cho đủ thanh pin).
                Emit(_sparks, SocketPos(), 20);
                Play(slotClip, 0.7f, 1.4f);
                _charge = 1f;
            }
            UpdateMeter();
            _standTarget = 1f;
            Play(servoClip, 0.9f, 1f);
            Kick(1.2f, 0f);
            Type("> PIN 100%. KHỞI ĐỘNG RB-10");
            Type("> CHÂN TRÁI .... OK");
            Type("> CHÂN PHẢI .... OK");
            _scanLine = "QUÉT QUÀ: ▒";
            if (_droneMode == DroneMode.Arrive || _droneMode == DroneMode.Drop) ReleaseCargo();
        }

        private IEnumerator Fizzle(Color color)
        {
            _scanLine = null;
            Type("> QUÉT QUÀ: " + _tier.DisplayName().ToUpperInvariant());
            Type("> LỖI 0x00: CHẬP MẠCH!!");
            Play(fizzleClip, 1f, 1f);
            // máy tụt pitch rồi tắt hẳn
            float p0 = _hum != null ? _hum.pitch : 1f;
            for (float t = 0f; t < 0.9f; t += Time.deltaTime)
            {
                if (_hum != null) { _hum.pitch = Mathf.Lerp(p0, 0.25f, t / 0.9f); _hum.volume = Mathf.Lerp(0.35f, 0f, t / 0.9f) * volume; }
                if (_rng.NextDouble() < 0.35) { SetEyes(color, (float)_rng.NextDouble() * 3f); Emit(_sparks, EyePos(), 3); }
                _shake = 0.006f;
                yield return null;
            }
            if (_hum != null) _hum.Stop();
            _shake = 0f;
            SetEyes(new Color(0.3f, 0.3f, 0.3f), 0.15f);
            Emit(_smoke, TopPos(), 18);
            // chân sụm, rương đổ nghiêng
            _standTarget = 0f;
            _tiltTarget = 11f * (_rng.NextDouble() < 0.5 ? -1f : 1f);
            yield return new WaitForSeconds(0.2f);
            Kick(-1.6f, 0f);
            Play(slotClip, 0.6f, 0.5f);
            // bu-lông bung văng (Rigidbody thật)
            for (int i = 0; i < 4; i++) Eject(RobotChargePart.Kind.BuLong, 1.1f, false);
            yield return new WaitForSeconds(0.35f);
            // một tay thò ra rồi rũ xuống
            _armLimp = true;
            _armTarget = 0.6f;
            yield return new WaitForSeconds(0.45f);
            _armTarget = 0.25f;
            Emit(_smoke, TopPos(), 10);
            // drone tới chọc chọc
            if (_drone != null && _droneFallBody == null) { _droneMode = DroneMode.Poke; _droneT = Time.time; }
        }

        private IEnumerator Triumph(ChestTier tier, Color color)
        {
            _scanLine = null;
            int lv = Mathf.Clamp(tier.Rank(), 1, 4);
            if (tier == ChestTier.Thuong)
            {
                // "tạm ổn": hắt hơi một chút khói, rồi mới chạy
                Emit(_smoke, TopPos(), 6);
                Play(fizzleClip, 0.35f, 1.8f);
                yield return new WaitForSeconds(0.3f);
                Type("> HỆ THỐNG: TẠM ỔN");
            }
            else Type("> MỌI HỆ THỐNG: OK!");
            Type("> QUÉT QUÀ: " + tier.DisplayName().ToUpperInvariant() + (lv >= 3 ? "!!!" : "!"));
            if (_hum != null) _hum.Stop();
            Play(blastClips != null && blastClips.Length >= lv ? blastClips[lv - 1] : null, 1f, 1f);
            SetEyes(color, 4f);
            Emit(_sparks, TopPos(), 15 * (lv + 1));
            _armTarget = 1f;
            // nhảy bằng Rigidbody thật: cao dần theo bậc
            float[] heights = { 0f, 0.08f, 0.25f, 0.45f, 0.7f };
            StartJump(heights[lv]);
            if (_drone != null) { _droneMode = DroneMode.Celebrate; _droneT = Time.time; }
            int confetti = lv >= 2 ? 3 + 3 * lv : 0;
            for (int i = 0; i < confetti; i++)
            {
                Eject(i % 3 == 0 ? RobotChargePart.Kind.BanhRang : RobotChargePart.Kind.BuLong, 1.4f + 0.3f * lv, true);
                if (i % 3 == 2) yield return new WaitForSeconds(0.05f);
            }
            if (tier == ChestTier.HuyenThoai)
            {
                for (int k = 0; k < 4; k++)
                {
                    yield return new WaitForSeconds(0.12f);
                    SetEyes(k % 2 == 0 ? Color.white : color, 6f);
                }
            }
            SetEyes(color, 2.6f);
        }

        // ================================================================== vật lý: vật sạc, drone, nhảy

        private void SpawnPart(RobotChargePart.Kind kind, float jitter)
        {
            if (_chest == null) return;
            Vector3 p = SocketPos() + Vector3.up * 0.7f + new Vector3((float)(_rng.NextDouble() - 0.5) * jitter, 0f, (float)(_rng.NextDouble() - 0.5) * jitter);
            Rigidbody rb = MakePart(kind, p, true);
            rb.angularVelocity = new Vector3((float)_rng.NextDouble() * 8f, (float)_rng.NextDouble() * 8f, 0f);
        }

        private Rigidbody MakePart(RobotChargePart.Kind kind, Vector3 pos, bool chargeable)
        {
            GameObject g;
            switch (kind)
            {
                case RobotChargePart.Kind.Pin:
                    g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    g.transform.localScale = new Vector3(0.045f, 0.05f, 0.045f);
                    g.GetComponent<Renderer>().sharedMaterial = OwnMat(new Color(1f, 0.82f, 0.15f));
                    break;
                case RobotChargePart.Kind.BanhRang:
                    g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    g.transform.localScale = new Vector3(0.07f, 0.008f, 0.07f);
                    g.GetComponent<Renderer>().sharedMaterial = _metalMat;
                    break;
                default:
                    g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    g.transform.localScale = new Vector3(0.025f, 0.045f, 0.025f);
                    g.GetComponent<Renderer>().sharedMaterial = _darkMat;
                    break;
            }
            g.name = "RobotPart_" + kind;
            g.hideFlags = HideFlags.DontSave;
            g.transform.position = pos;
            Rigidbody rb = g.AddComponent<Rigidbody>();
            rb.mass = kind == RobotChargePart.Kind.Pin ? 0.25f : 0.08f;
            _owned.Add(g);
            if (chargeable)
            {
                RobotChargePart part = g.AddComponent<RobotChargePart>();
                part.kind = kind;
                part.charge = RobotChargePart.DefaultChargeFor(kind);
                _parts.Add(rb);
            }
            return rb;
        }

        /// <summary>Gọi từ khe sạc khi một Rigidbody lọt vào.</summary>
        private void OnSocketPart(Rigidbody rb)
        {
            if (_phase != Phase.Charging && _phase != Phase.Booting) return;
            RobotChargePart part = rb.GetComponent<RobotChargePart>();
            bool ours = _owned.Contains(rb.gameObject);
            float add = part != null ? part.charge : RobotChargePart.DefaultChargeFor(RobotChargePart.Kind.DoLa);
            if (part != null) part.consumed = true;
            _charge = Mathf.Clamp01(_charge + add);
            UpdateMeter();
            Play(slotClip, 0.9f, 0.85f + 0.4f * _charge + 0.1f * (float)_rng.NextDouble());
            Emit(_sparks, SocketPos(), 10 + Mathf.RoundToInt(_charge * 10f));
            Kick(0.8f + add * 2f, (float)(_rng.NextDouble() - 0.5) * 6f);
            Type(_charge >= 1f ? "> PIN ĐẦY! SẴN SÀNG" : "> NẠP +" + Mathf.RoundToInt(add * 100f) + "%  (" + Mathf.RoundToInt(_charge * 100f) + "%)");
            if (ours) StartCoroutine(Swallow(rb));
            else StartCoroutine(SpitOut(rb));   // đồ của người chơi: hút điện rồi nhả ra, không phá huỷ
        }

        private IEnumerator Swallow(Rigidbody rb)
        {
            _parts.Remove(rb);
            rb.isKinematic = true;
            Collider c = rb.GetComponent<Collider>();
            if (c != null) c.enabled = false;
            Transform t = rb.transform;
            Vector3 s0 = t.localScale;
            for (float u = 0f; u < 1f && t != null; u += Time.deltaTime / 0.18f)
            {
                t.position = Vector3.Lerp(t.position, SocketPos() - _chest.up * 0.05f, u);
                t.localScale = s0 * (1f - u);
                yield return null;
            }
            if (rb != null) { _owned.Remove(rb.gameObject); Destroy(rb.gameObject); }
        }

        private IEnumerator SpitOut(Rigidbody rb)
        {
            yield return new WaitForSeconds(0.25f);
            if (rb == null || _chest == null) yield break;
            Vector3 dir = (_chest.up * 1.6f - _chest.forward * 0.8f + _chest.right * (float)(_rng.NextDouble() - 0.5)).normalized;
            rb.AddForce(dir * 2.2f * rb.mass, ForceMode.Impulse);
            Emit(_sparks, rb.transform.position, 6);
        }

        private void Eject(RobotChargePart.Kind kind, float speed, bool celebrate)
        {
            Vector3 p = TopPos() + _chest.up * 0.02f;
            Rigidbody rb = MakePart(kind, p, false);
            Vector3 side = Quaternion.AngleAxis((float)_rng.NextDouble() * 360f, _chest.up) * _chest.forward;
            Vector3 dir = (_chest.up * (celebrate ? 2.2f : 1f) + side * (celebrate ? 0.7f : 1f)).normalized;
            rb.AddForce(dir * speed * (0.75f + 0.5f * (float)_rng.NextDouble()) * rb.mass, ForceMode.Impulse);
            rb.angularVelocity = new Vector3((float)_rng.NextDouble() * 20f, (float)_rng.NextDouble() * 20f, (float)_rng.NextDouble() * 20f);
            Destroy(rb.gameObject, 6f);   // mảnh văng tự dọn
        }

        private void LaunchDrone()
        {
            if (_chest == null) return;
            GameObject root = new GameObject("RobotDrone");
            root.hideFlags = HideFlags.DontSave;
            _owned.Add(root);
            _drone = root.transform;
            Prim(PrimitiveType.Cube, _drone, new Vector3(0f, 0f, 0f), new Vector3(0.09f, 0.03f, 0.07f), _metalMat);
            Prim(PrimitiveType.Sphere, _drone, new Vector3(0f, 0f, 0.035f), new Vector3(0.025f, 0.025f, 0.025f), _eyeMat);
            _droneRotorA = Prim(PrimitiveType.Cylinder, _drone, new Vector3(-0.06f, 0.025f, 0f), new Vector3(0.07f, 0.002f, 0.07f), _darkMat);
            _droneRotorB = Prim(PrimitiveType.Cylinder, _drone, new Vector3(0.06f, 0.025f, 0f), new Vector3(0.07f, 0.002f, 0.07f), _darkMat);
            float side = _rng.NextDouble() < 0.5 ? -1f : 1f;
            _drone.position = TopPos() + _chest.right * (1.4f * side) + Vector3.up * 0.8f;
            _droneWatchPos = TopPos() + _chest.right * (0.55f * side) + _chest.forward * 0.25f + Vector3.up * 0.35f;
            Rigidbody cargo = MakePart(RobotChargePart.Kind.Pin, _drone.position - Vector3.up * 0.05f, true);
            cargo.isKinematic = true;
            _droneCargo = cargo;
            _droneMode = DroneMode.Arrive;
            _droneT = Time.time;
            if (_droneSrc != null && droneLoopClip != null) { _droneSrc.clip = droneLoopClip; _droneSrc.volume = 0.35f * volume; _droneSrc.Play(); }
        }

        private void ReleaseCargo()
        {
            if (_droneCargo != null) { _droneCargo.isKinematic = false; _droneCargo = null; }
            _droneMode = DroneMode.Leave;
            _droneT = Time.time;
        }

        private void UpdateDrone(float now, float dt)
        {
            if (_drone == null) return;
            float spin = 2400f * dt;
            if (_droneMode != DroneMode.Dead)
            {
                if (_droneRotorA != null) _droneRotorA.Rotate(0f, spin, 0f);
                if (_droneRotorB != null) _droneRotorB.Rotate(0f, -spin, 0f);
            }
            if (_droneFallBody != null) return;   // đang rơi theo vật lý
            float t = now - _droneT;
            Vector3 hover = new Vector3(0f, Mathf.Sin(now * 9f) * 0.01f, 0f);
            Vector3 above = SocketPos() + Vector3.up * 0.45f;
            switch (_droneMode)
            {
                case DroneMode.Arrive:
                    _drone.position = Vector3.Lerp(_drone.position, above, dt * 3.5f) + hover;
                    if (t > 0.9f) { _droneMode = DroneMode.Drop; _droneT = now; }
                    break;
                case DroneMode.Drop:
                    _drone.position = Vector3.Lerp(_drone.position, above, dt * 6f) + hover;
                    if (t > 0.25f) ReleaseCargo();
                    break;
                case DroneMode.Leave:
                    _drone.position = Vector3.Lerp(_drone.position, _droneWatchPos, dt * 2.5f) + hover;
                    if (t > 1.2f) _droneMode = DroneMode.Watch;
                    break;
                case DroneMode.Watch:
                    _drone.position = Vector3.Lerp(_drone.position, _droneWatchPos, dt * 2f) + hover;
                    break;
                case DroneMode.Celebrate:
                {
                    float a = t * 5f;
                    Vector3 c = TopPos() + Vector3.up * (0.4f + _jumpOffset);
                    Vector3 target = c + new Vector3(Mathf.Cos(a), 0.1f * Mathf.Sin(a * 2f), Mathf.Sin(a)) * 0.45f;
                    _drone.position = Vector3.Lerp(_drone.position, target, dt * 6f);
                    break;
                }
                case DroneMode.Poke:
                {
                    // lao vào chọc hai cái rồi... hết pin
                    Vector3 face = TopPos() + _chest.forward * (_d * 0.6f) + Vector3.up * 0.05f;
                    Vector3 back = face + _chest.forward * 0.25f + Vector3.up * 0.1f;
                    float k = Mathf.PingPong(t * 2.2f, 1f);
                    _drone.position = Vector3.Lerp(back, face, k * k);
                    if (k > 0.97f && Time.frameCount % 2 == 0) { Kick(0.3f, 4f); Emit(_sparks, face, 2); }
                    if (t > 1.6f)
                    {
                        _droneMode = DroneMode.Dead;
                        if (_droneSrc != null) _droneSrc.Stop();
                        _droneFallBody = _drone.gameObject.AddComponent<Rigidbody>();
                        _droneFallBody.mass = 0.3f;
                        _droneFallBody.angularVelocity = new Vector3(3f, 0f, 5f);
                        Play(slotClip, 0.4f, 0.6f);
                        Emit(_smoke, _drone.position, 5);
                    }
                    break;
                }
            }
            if (_droneCargo != null) _droneCargo.transform.position = _drone.position - Vector3.up * 0.06f;
        }

        private void StartJump(float height)
        {
            if (height <= 0f || _chest == null) return;
            GameObject g = new GameObject("RobotJumpBody");
            g.hideFlags = HideFlags.DontSave;
            _owned.Add(g);
            g.transform.position = Vector3.zero;
            _jumpBody = g.AddComponent<Rigidbody>();
            _jumpBody.mass = 1f;
            _jumpBody.useGravity = true;
            _jumpY0 = 0f;
            _jumpPrevY = 0f;
            _hoverDone = _tier != ChestTier.HuyenThoai;
            // vận tốc ban đầu v = sqrt(2 g h): đúng độ cao theo trọng lực của Physics
            float v = Mathf.Sqrt(2f * 9.81f * height);
            _jumpBody.AddForce(Vector3.up * v * _jumpBody.mass, ForceMode.Impulse);
            _standTarget = 1f;
            Kick(-0.8f, 0f);
            if (_tier == ChestTier.HuyenThoai) StartCoroutine(Flame(1.6f));
        }

        private void UpdateJump()
        {
            if (_jumpBody == null) { _jumpOffset = Mathf.MoveTowards(_jumpOffset, 0f, Time.deltaTime); return; }
            float y = _jumpBody.transform.position.y - _jumpY0;
            if (!_hoverDone && y < _jumpPrevY)
            {
                // Huyền thoại: lơ lửng ở đỉnh 1 giây (đóng băng vật lý), rồi rơi tiếp
                _hoverDone = true;
                _jumpBody.isKinematic = true;
                StartCoroutine(ReleaseHover(1.0f));
            }
            if (y <= 0f && y < _jumpPrevY)
            {
                Destroy(_jumpBody.gameObject);
                _owned.Remove(_jumpBody.gameObject);
                _jumpBody = null;
                _jumpOffset = 0f;
                Kick(-2f, (float)(_rng.NextDouble() - 0.5) * 8f);   // tiếp đất: nhún mạnh
                Play(slotClip, 0.8f, 0.45f);
                Emit(_smoke, BottomPos(), 8);
                return;
            }
            _jumpPrevY = y;
            _jumpOffset = Mathf.Max(0f, y);
        }

        private IEnumerator ReleaseHover(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (_jumpBody != null) _jumpBody.isKinematic = false;
        }

        private IEnumerator Flame(float seconds)
        {
            for (float t = 0f; t < seconds; t += 0.05f)
            {
                if (_footL != null) Emit(_flame, _footL.position, 3, Quaternion.LookRotation(-_chest.up));
                if (_footR != null) Emit(_flame, _footR.position, 3, Quaternion.LookRotation(-_chest.up));
                yield return new WaitForSeconds(0.05f);
            }
        }

        // ================================================================== chuyển động cả rương

        private void Kick(float bounceVel, float tiltVel)
        {
            _bounceVel += bounceVel;
            _tiltZVel += tiltVel * 20f;
        }

        private void UpdateSprings(float dt)
        {
            dt = Mathf.Min(dt, 0.05f);
            // lò xo tắt dần cho cú nảy và cú nghiêng
            _bounceVel += (-90f * _bounce - 9f * _bounceVel) * dt;
            _bounce += _bounceVel * dt * 0.05f;
            _tiltZVel += (-80f * (_tiltZ - _tiltTarget) - 8f * _tiltZVel) * dt;
            _tiltZ += _tiltZVel * dt;
            if (_phase == Phase.Booting)
            {
                float u = Mathf.Clamp01((Time.time - _bootT0) / 2.8f);
                _shake = Mathf.Lerp(0.001f, 0.005f, u * u);   // độ căng dâng: rung mạnh dần
            }
            else if (_phase == Phase.Charging) _shake = 0.0015f * _charge;
            else if (_phase == Phase.Finale && _tier != ChestTier.Rac) _shake = Mathf.MoveTowards(_shake, 0f, dt * 0.01f);
        }

        private void RemoveOffset()
        {
            if (!_hasApplied || _chest == null) { _hasApplied = false; return; }
            _chest.localPosition -= _appliedPos;
            _chest.localRotation = _chest.localRotation * Quaternion.Inverse(_appliedRot);
            _hasApplied = false;
        }

        private void ApplyOffset()
        {
            float march = 0f;
            if (_phase == Phase.Booting)
            {
                float u = Mathf.Clamp01((Time.time - _bootT0) / 2.8f);
                march = Mathf.Abs(Mathf.Sin((Time.time - _bootT0) * Mathf.Lerp(6f, 18f, u))) * 0.012f;   // dậm chân nhanh dần
            }
            float lift = _legLen * _stand + _bounce + march;
            float n = Time.time * 37f;
            Vector3 jitter = new Vector3(Mathf.PerlinNoise(n, 0.1f) - 0.5f, 0f, Mathf.PerlinNoise(0.7f, n) - 0.5f) * 2f * _shake;
            Vector3 world = _chest.up * lift + Vector3.up * _jumpOffset + _chest.TransformDirection(jitter);
            Vector3 local = _chest.parent != null ? _chest.parent.InverseTransformVector(world) : world;
            Quaternion rot = Quaternion.Euler(0f, 0f, _tiltZ);
            _chest.localPosition += local;
            _chest.localRotation = _chest.localRotation * rot;
            _appliedPos = local;
            _appliedRot = rot;
            _hasApplied = true;
        }

        // ================================================================== dựng hình (đặt lại mỗi khung theo rương)

        private void Measure()
        {
            Renderer[] rs = _chest.GetComponentsInChildren<Renderer>();
            bool any = false;
            Bounds b = new Bounds(_chest.position, Vector3.zero);
            foreach (Renderer r in rs)
            {
                if (r == null || r is ParticleSystemRenderer) continue;
                if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            }
            if (!any) b = new Bounds(_chest.position + Vector3.up * 0.27f, new Vector3(0.5f, 0.55f, 0.44f));
            _w = Mathf.Max(0.2f, b.size.x);
            _h = Mathf.Max(0.2f, b.size.y);
            _d = Mathf.Max(0.2f, b.size.z);
            _localBottom = _chest.InverseTransformPoint(new Vector3(b.center.x, b.min.y, b.center.z));
            _localTop = _chest.InverseTransformPoint(new Vector3(b.center.x, b.max.y, b.center.z));
            _legLen = _h * 0.32f;
        }

        private Vector3 BottomPos() => _chest.TransformPoint(_localBottom);
        private Vector3 TopPos() => _chest.TransformPoint(_localTop);
        private Vector3 SocketPos() => TopPos() - _chest.forward * (_d * 0.3f) + _chest.up * 0.01f;
        private Vector3 EyePos() => Vector3.Lerp(BottomPos(), TopPos(), 0.62f) + _chest.forward * (_d * 0.5f);

        private Material OwnMat(Color c)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            Material m = s != null ? new Material(s) : ChestFxFactory.CreateUnlit(c, false, "RobotMat");
            m.hideFlags = HideFlags.DontSave;
            ChestFxFactory.SetColor(m, c);
            _owned.Add(m);
            return m;
        }

        private Transform Prim(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject g = GameObject.CreatePrimitive(type);
            g.hideFlags = HideFlags.DontSave;
            Collider c = g.GetComponent<Collider>();
            if (c != null) Destroy(c);   // hình trang trí: không va chạm
            g.GetComponent<Renderer>().sharedMaterial = mat;
            if (parent != null) g.transform.SetParent(parent, false); else _owned.Add(g);
            g.transform.localPosition = localPos;
            g.transform.localScale = scale;
            return g.transform;
        }

        private void BuildRig()
        {
            _metalMat = OwnMat(bodyMetal);
            _darkMat = OwnMat(darkMetal);
            _eyeMat = ChestFxFactory.CreateUnlit(eyeColor, false, "RobotEyeMat");
            _owned.Add(_eyeMat);
            _meterOff = ChestFxFactory.CreateUnlit(new Color(0.12f, 0.12f, 0.12f), false, "RobotMeterOff");
            _owned.Add(_meterOff);

            _legL = Prim(PrimitiveType.Cube, null, Vector3.zero, Vector3.zero, _metalMat);
            _legR = Prim(PrimitiveType.Cube, null, Vector3.zero, Vector3.zero, _metalMat);
            _footL = Prim(PrimitiveType.Cube, null, Vector3.zero, Vector3.zero, _darkMat);
            _footR = Prim(PrimitiveType.Cube, null, Vector3.zero, Vector3.zero, _darkMat);
            _armL = Prim(PrimitiveType.Cube, null, Vector3.zero, Vector3.zero, _metalMat);
            _armR = Prim(PrimitiveType.Cube, null, Vector3.zero, Vector3.zero, _metalMat);
            _handL = Prim(PrimitiveType.Cube, null, Vector3.zero, Vector3.zero, _darkMat);
            _handR = Prim(PrimitiveType.Cube, null, Vector3.zero, Vector3.zero, _darkMat);
            _eyeL = Prim(PrimitiveType.Sphere, null, Vector3.zero, Vector3.one * 0.035f, _eyeMat);
            _eyeR = Prim(PrimitiveType.Sphere, null, Vector3.zero, Vector3.one * 0.035f, _eyeMat);

            for (int i = 0; i < 5; i++)
            {
                Transform seg = Prim(PrimitiveType.Cube, null, Vector3.zero, new Vector3(0.04f, 0.02f, 0.02f), _meterOff);
                _meter.Add(seg.GetComponent<Renderer>());
            }

            GameObject l = new GameObject("RobotEyeLight");
            l.hideFlags = HideFlags.DontSave;
            _owned.Add(l);
            _eyeLight = l.AddComponent<Light>();
            _eyeLight.type = LightType.Point;
            _eyeLight.range = 0.9f;
            _eyeLight.shadows = LightShadows.None;
            SetEyes(eyeColor, 0.4f);

            // khe sạc: trigger + Rigidbody kinematic
            _socket = new GameObject("RobotChargeSocket");
            _socket.hideFlags = HideFlags.DontSave;
            _owned.Add(_socket);
            BoxCollider box = _socket.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(_w * 0.6f, 0.12f, _d * 0.4f);
            Rigidbody srb = _socket.AddComponent<Rigidbody>();
            srb.isKinematic = true;
            srb.useGravity = false;
            RobotChargeSocket sock = _socket.AddComponent<RobotChargeSocket>();
            sock.ignoreRoot = _chest;
            sock.onPart = OnSocketPart;
            Prim(PrimitiveType.Cube, _socket.transform, Vector3.zero, new Vector3(_w * 0.35f, 0.012f, 0.05f), _darkMat);   // khe nhìn thấy

            GameObject scr = new GameObject("RobotScreen");
            scr.hideFlags = HideFlags.DontSave;
            _owned.Add(scr);
            _screen = scr.AddComponent<TextMesh>();
            _screen.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_screen.font == null) _screen.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (_screen.font != null) scr.GetComponent<Renderer>().sharedMaterial = _screen.font.material;
            _screen.fontSize = 48;
            _screen.characterSize = 0.006f;
            _screen.anchor = TextAnchor.LowerCenter;
            _screen.alignment = TextAlignment.Left;
            _screen.color = new Color(0.4f, 1f, 0.55f);
            _screen.text = "";

            BuildParticles();
            UpdateMeter();
        }

        private void PlaceRig()
        {
            Vector3 up = _chest.up, right = _chest.right, fwd = _chest.forward;
            Vector3 bottom = BottomPos(), top = TopPos();
            float len = Mathf.Max(0.001f, _legLen * _stand + Mathf.Max(0f, _bounce));
            float stepT = _phase == Phase.Booting ? (Time.time - _bootT0) * Mathf.Lerp(6f, 18f, Mathf.Clamp01((Time.time - _bootT0) / 2.8f)) : 0f;
            for (int s = 0; s < 2; s++)
            {
                float side = s == 0 ? -1f : 1f;
                float step = _phase == Phase.Booting ? Mathf.Max(0f, Mathf.Sin(stepT + s * Mathf.PI)) * 0.02f : 0f;
                float l = Mathf.Max(0.001f, len - step);
                Vector3 hip = bottom + right * (side * _w * 0.26f);
                Transform leg = s == 0 ? _legL : _legR, foot = s == 0 ? _footL : _footR;
                leg.position = hip - up * (l * 0.5f);
                leg.rotation = _chest.rotation;
                leg.localScale = _stand > 0.01f ? new Vector3(0.06f, l, 0.08f) : Vector3.zero;
                foot.position = hip - up * l;
                foot.rotation = _chest.rotation;
                foot.localScale = _stand > 0.01f ? new Vector3(0.1f, 0.025f, 0.15f) : Vector3.zero;

                // tay: vai ở hông thân, vươn ra trước tới dưới bóng món quà (bậc thấp: rũ xuống)
                Vector3 shoulder = Vector3.Lerp(bottom, top, 0.45f) + right * (side * _w * 0.5f);
                Vector3 reach;
                Transform sil = _reveal != null ? _reveal.Silhouette : null;
                if (_armLimp) reach = shoulder + right * (side * 0.08f) - up * 0.2f + fwd * 0.05f;
                else if (sil != null) reach = sil.position + right * (side * 0.09f) - up * 0.05f;
                else reach = shoulder + fwd * 0.3f + up * 0.15f;
                Vector3 hand = Vector3.Lerp(shoulder, reach, _arm);
                Vector3 dir = hand - shoulder;
                float al = dir.magnitude;
                Transform arm = s == 0 ? _armL : _armR, h = s == 0 ? _handL : _handR;
                bool showArm = al > 0.01f && (!_armLimp || s == 0);
                arm.position = shoulder + dir * 0.5f;
                arm.rotation = al > 0.001f ? Quaternion.LookRotation(dir, up) : _chest.rotation;
                arm.localScale = showArm ? new Vector3(0.035f, 0.035f, al) : Vector3.zero;
                h.position = hand;
                h.rotation = arm.rotation;
                h.localScale = showArm ? new Vector3(0.06f, 0.03f, 0.05f) : Vector3.zero;

                Transform eye = s == 0 ? _eyeL : _eyeR;
                eye.position = EyePos() + right * (side * _w * 0.16f);
            }
            if (_eyeLight != null) _eyeLight.transform.position = EyePos() + fwd * 0.08f;
            for (int i = 0; i < _meter.Count; i++)
            {
                Transform m = _meter[i].transform;
                m.position = top + up * 0.03f - fwd * (_d * 0.47f) + right * ((i - 2) * 0.05f);
                m.rotation = _chest.rotation;
            }
            if (_socket != null)
            {
                _socket.transform.position = SocketPos();
                _socket.transform.rotation = _chest.rotation;
            }
            if (_screen != null)
            {
                Transform st = _screen.transform;
                st.position = top + up * (0.28f + _jumpOffset * 0.2f) + fwd * 0.05f;
                Camera cam = Camera.main;
                if (cam != null) st.rotation = Quaternion.LookRotation(st.position - cam.transform.position, Vector3.up);
            }
        }

        private void SetEyes(Color c, float intensity)
        {
            if (_eyeLight != null) { _eyeLight.color = c; _eyeLight.intensity = intensity; }
            if (_eyeMat != null) ChestFxFactory.SetColor(_eyeMat, Color.Lerp(Color.black, c, Mathf.Clamp01(0.2f + intensity / 3f)));
        }

        private void UpdateMeter()
        {
            int lit = Mathf.RoundToInt(_charge * _meter.Count);
            Color full = Color.Lerp(new Color(1f, 0.3f, 0.2f), new Color(0.3f, 1f, 0.4f), _charge);
            while (_meterMats.Count < _meter.Count)
            {
                Material m = ChestFxFactory.CreateUnlit(full, false, "RobotMeterOn");
                _owned.Add(m);
                _meterMats.Add(m);
            }
            for (int i = 0; i < _meter.Count; i++)
            {
                ChestFxFactory.SetColor(_meterMats[i], full);
                _meter[i].sharedMaterial = i < lit ? _meterMats[i] : _meterOff;
            }
        }

        // ================================================================== màn chữ chạy

        private void Type(string line)
        {
            _lines.Add(line);
        }

        private void SetScan(string s) => _scanLine = s;

        private void UpdateTyping(float dt)
        {
            if (_screen == null) return;
            _typeClock += dt;
            // gõ từng ký tự của dòng đầu hàng đợi
            while (_typeClock > 0.022f && _lines.Count > 0)
            {
                _typeClock -= 0.022f;
                string target = _lines[0];
                if (_typing.Length < target.Length)
                {
                    _typing = target.Substring(0, _typing.Length + 1);
                    if (_typing.Length % 2 == 0) Play(typeClip, 0.25f, 0.9f + 0.2f * (float)_rng.NextDouble());
                }
                else
                {
                    _history.Add(target);
                    if (_history.Count > 4) _history.RemoveAt(0);
                    _lines.RemoveAt(0);
                    _typing = "";
                }
            }
            if (_lines.Count == 0) _typeClock = 0f;
            string text = string.Join("\n", _history);
            if (_typing.Length > 0) text += (text.Length > 0 ? "\n" : "") + _typing + "_";
            if (_scanLine != null) text += (text.Length > 0 ? "\n" : "") + "> " + _scanLine;
            _screen.text = text;
        }

        private readonly List<string> _history = new List<string>();

        // ================================================================== âm

        private void BuildAudio()
        {
            _sfx = NewSource("RobotSfx", false);
            _hum = NewSource("RobotHum", true);
            _droneSrc = NewSource("RobotDroneSrc", true);
            if (humLoopClip != null)
            {
                _hum.clip = humLoopClip;
                _hum.pitch = 0.55f;
                _hum.volume = 0.25f * volume;
                _hum.Play();
            }
            _beepClock = 0f;
        }

        private AudioSource NewSource(string n, bool loop)
        {
            GameObject g = new GameObject(n);
            g.hideFlags = HideFlags.DontSave;
            g.transform.SetParent(_chest, false);
            _owned.Add(g);
            AudioSource a = g.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.loop = loop;
            a.spatialBlend = 0.6f;
            return a;
        }

        private void Play(AudioClip c, float vol, float pitch)
        {
            if (c == null || _sfx == null) return;
            if (_reveal != null && Mathf.Approximately(pitch, 1f)) { _reveal.PlayOneShot(c, vol * volume, 1f); return; }
            _sfx.pitch = pitch;
            _sfx.PlayOneShot(c, vol * volume);
        }

        private void UpdateAudio(float now, float dt)
        {
            if (_hum == null) return;
            if (_phase == Phase.Charging)
            {
                _hum.pitch = Mathf.MoveTowards(_hum.pitch, 0.55f + 0.45f * _charge, dt * 0.8f);
                _hum.volume = (0.25f + 0.1f * _charge) * volume;
            }
            else if (_phase == Phase.Booting)
            {
                // độ căng dâng: máy rú lên, bíp dồn dập, giống nhau cho mọi bậc (giữ bí mật)
                float u = Mathf.Clamp01((now - _bootT0) / 2.8f);
                _hum.pitch = Mathf.Lerp(1f, 1.7f, u * u);
                _hum.volume = Mathf.Lerp(0.35f, 0.6f, u) * volume;
                _beepClock -= dt;
                if (_beepClock <= 0f)
                {
                    _beepClock = Mathf.Lerp(0.45f, 0.07f, u);
                    Play(beepClip, 0.45f, Mathf.Lerp(0.9f, 1.6f, u));
                }
            }
        }

        // ================================================================== hạt

        private void BuildParticles()
        {
            _sparkStyle = Style(ParticleLook.Streak, ParticleDirection.Radial, 1.5f, 0.012f, 0.45f, 1f, Color.white, 0.5f);
            _sparks = ChestFxFactory.CreateParticles("RobotXSparks", null, _sparkStyle, 0.6f, true);
            ChestFxFactory.SetParticleColor(_sparks, new Color(1f, 0.85f, 0.4f), _sparkStyle, 1f);
            _smokeStyle = Style(ParticleLook.SoftDot, ParticleDirection.Up, 0.25f, 0.07f, 1.7f, 0f, new Color(0.25f, 0.25f, 0.25f), 0.8f);
            _smoke = ChestFxFactory.CreateParticles("RobotXSmoke", null, _smokeStyle, 0.6f, true);
            ChestFxFactory.SetParticleColor(_smoke, new Color(0.45f, 0.43f, 0.4f), _smokeStyle, 0.5f);
            _flameStyle = Style(ParticleLook.SoftDot, ParticleDirection.Up, 1.2f, 0.04f, 0.3f, 0f, new Color(1f, 0.9f, 0.3f), 0.6f);
            _flame = ChestFxFactory.CreateParticles("RobotXFlame", null, _flameStyle, 0.6f, true);
            ChestFxFactory.SetParticleColor(_flame, new Color(1f, 0.45f, 0.1f), _flameStyle, 1f);
        }

        private ChestFxProfile Style(ParticleLook look, ParticleDirection dir, float speed, float size, float life, float gravity, Color secondary, float mix)
        {
            ChestFxProfile p = ScriptableObject.CreateInstance<ChestFxProfile>();
            p.hideFlags = HideFlags.DontSave;
            p.particleLook = look;
            p.particleDirection = dir;
            p.particleSpeed = speed;
            p.particleSize = size;
            p.particleLifetime = life;
            p.burstGravity = gravity;
            p.secondaryColor = secondary;
            p.secondaryMix = mix;
            _owned.Add(p);
            return p;
        }

        private void Emit(ParticleSystem ps, Vector3 pos, int count)
        {
            if (ps == null) return;
            ps.transform.SetPositionAndRotation(pos, _chest != null ? _chest.rotation : Quaternion.identity);
            ps.Play();
            ps.Emit(count);
        }

        private void Emit(ParticleSystem ps, Vector3 pos, int count, Quaternion rot)
        {
            if (ps == null) return;
            // Hướng "Up" của hạt là trục y cục bộ: lửa cần phun xuống nên xoay trục y về phía rot*forward.
            ps.transform.SetPositionAndRotation(pos, rot * Quaternion.Euler(90f, 0f, 0f));
            ps.Play();
            ps.Emit(count);
        }

        // ================================================================== dọn

        private void Cleanup()
        {
            StopAllCoroutines();
            RemoveOffset();
            foreach (Object o in _owned)
            {
                if (o == null) continue;
                if (o is GameObject go)
                {
                    foreach (ParticleSystemRenderer r in go.GetComponentsInChildren<ParticleSystemRenderer>()) if (r != null) Destroy(r.sharedMaterial);
                    Destroy(go);
                }
                else Destroy(o);
            }
            foreach (ParticleSystem ps in new[] { _sparks, _smoke, _flame })
            {
                if (ps == null) continue;
                Destroy(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
                Destroy(ps.gameObject);
            }
            _owned.Clear();
            _parts.Clear();
            _meter.Clear();
            _meterMats.Clear();
            _lines.Clear();
            _history.Clear();
            _typing = "";
            _scanLine = null;
            _sparks = _smoke = _flame = null;
            _drone = null; _droneFallBody = null; _droneCargo = null; _droneMode = DroneMode.None;
            _jumpBody = null; _jumpOffset = 0f;
            _socket = null; _screen = null; _eyeLight = null;
            _stand = _standTarget = 0f; _bounce = _bounceVel = 0f; _tiltZ = _tiltZVel = _tiltTarget = 0f; _shake = 0f;
            _arm = _armTarget = 0f; _armLimp = false;
            _hum = _droneSrc = _sfx = null;
            _phase = Phase.Idle;
        }
    }
}

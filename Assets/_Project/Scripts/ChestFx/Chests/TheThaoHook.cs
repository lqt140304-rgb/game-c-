using System.Collections;
using System.Collections.Generic;
using ChoDoLa.Interaction;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TheThao — kịch bản "LOẠT LUÂN LƯU PHÚT BÙ GIỜ". Rương là khung thành.
    ///
    ///  0,0 s (ngay khi bấm mở): còi khai cuộc, khán đài rì rầm, trống cổ vũ đập chậm; khung thành mọc lên quanh rương,
    ///        trọng tài trượt vào, quả bóng thật (Rigidbody) nằm ở chấm phạt đền.
    ///  ~0,9 s: cú sút 1 trúng cột dọc (keng!), bóng nảy lung tung bằng vật lý, rương bị rung lắc.
    ///  bắt đầu quay màu: bảng thay người giơ lên, bóng thứ hai rơi từ trên xuống; cú sút 2 xoáy cong (Magnus) dội xà.
    ///  ~2 s trước khi chốt: bảng "bù giờ" giơ lên, trống dồn nhanh, tiếng riser dâng.
    ///  ~0,7 s trước khi chốt: mọi thứ lặng (chỉ còn tiếng bóng bay) → cú sút quyết định. Bí mật tới tận lúc bóng tới khung.
    ///  Chốt: Rác = bóng bay vọt qua xà ra ngoài, âm "xịt" + kèn buồn, rương xẹp xuống; Thường = bóng lăn chậm vào, vỗ tay lẹt đẹt;
    ///        Hiếm = sút căng vào lưới, rương nảy lên; Rất hiếm = siêu phẩm cong + VAR chiếu lại chậm;
    ///        Huyền thoại = như Rất hiếm + rương lộn vòng trên không, mọi quả bóng bị thổi bay, mưa bóng, kèn hơi.
    ///
    /// Tương tác phụ (chỉ trang trí): phím B = "thay người" thả thêm bóng từ trên xuống (đập vào rương thì rương nảy),
    /// phím K = sút quả bóng tự do gần nhất vào khung (có xoáy, cong thật).
    ///
    /// BẤT BIẾN: chỉ ĐỌC <see cref="ChestRevealSequence.RolledTier"/> để chọn kết cục; không đổi nó. Ngẫu nhiên chỉ trang trí,
    /// theo <see cref="decorationSeed"/>. Màu 5 bậc và bóng vật phẩm vẫn do ChestRevealSequence lo.
    /// Đặt component lên gốc rương (TheThaoFxSetup làm việc này và điền âm).
    /// </summary>
    public class TheThaoHook : ChestFxHook
    {
        [Header("Âm (TheThaoFxSetup / OnValidate tự điền từ Audio/Chests/the_thao/)")]
        public AudioClip kickoffClip;     // còi khai cuộc
        public AudioClip airHornClip;     // kèn hơi
        public AudioClip crowdLoopClip, drumClip, kickClip, bounceClip, postClip, netClip;
        public AudioClip oohClip, roarClip, awwClip, deflateClip, tromboneClip, whistleEndClip;
        public AudioClip varBeepClip, rewindClip, riserClip, clapClip;
        [Range(0f, 1f)] public float extraVolume = 0.75f;

        [Header("Tương tác")]
        [Tooltip("B = thả bóng thay người, K = sút bóng tự do gần nhất.")]
        public bool allowPlayerInput = true;
        public int maxExtraBalls = 10;
        [Tooltip("Hạt giống trang trí (0 = mỗi lần một khác). KHÔNG ảnh hưởng bậc trúng.")]
        public int decorationSeed;

        [Header("Kích thước (theo cỡ rương)")]
        [SerializeField] private float ballRadiusFactor = 0.15f;
        [SerializeField] private float spotDistanceFactor = 2.6f;

        // ------------------------------------------------------------------ trạng thái
        private ChestRevealSequence _reveal;
        private Transform _chest;
        private bool _running, _finalFired, _finished;
        private ChestTier _tier;
        private System.Random _rng;
        private float _startTime, _lockEta;
        private int _extraBalls;

        private GameObject _stage;   // gốc mọi đồ dựng (không gắn vào rương để rương rung không kéo theo)
        private readonly List<Object> _owned = new List<Object>();
        private readonly List<TheThaoBall> _balls = new List<TheThaoBall>();
        private Collider _chestCollider, _postL, _postR, _bar;
        private Transform _referee, _refArm, _subBoard, _addedBoard, _varScreen;
        private PhysicsMaterial _bouncy;
        private AudioSource _crowd, _riser;
        private float _crowdTarget, _crowdPitch = 1f;

        private Vector3 _center, _ground, _front, _right, _spot;
        private float _size, _height, _halfWidth, _ballR;

        // lò xo của cả rương (cộng lên trên BoxOpenSequence, gỡ ra mỗi khung)
        private Vector3 _hop, _hopVel, _applied, _written;
        private float _tilt, _tiltVel, _flip, _squash = 1f, _squashVel;
        private Vector3 _tiltAxis = Vector3.right;
        private Quaternion _appliedRot = Quaternion.identity, _writtenRot = Quaternion.identity;
        private Vector3 _baseScale = Vector3.one;
        private bool _scaleTouched;

        private readonly List<Vector3> _replay = new List<Vector3>();
        private TheThaoBall _finalBall;
        private Coroutine _drums;

        // ------------------------------------------------------------------ vòng đời

        private void Awake() => FindReveal();

        private void FindReveal()
        {
            if (_reveal != null) return;
            _reveal = GetComponent<ChestRevealSequence>();
            if (_reveal == null) _reveal = GetComponentInParent<ChestRevealSequence>();
        }

        private void Update()
        {
            FindReveal();
            // Bắt đầu NGAY KHI Play được gọi (bấm mở), không chờ nắp hé.
            if (!_running && _reveal != null && _reveal.IsPlaying) Begin(_reveal);
            if (!_running) return;

            if (_crowd != null)
            {
                _crowd.volume = Mathf.Lerp(_crowd.volume, _crowdTarget * extraVolume, Time.deltaTime * 4f);
                _crowd.pitch = Mathf.Lerp(_crowd.pitch, _crowdPitch, Time.deltaTime * 3f);
            }
            if (_finalBall != null && _finalBall.gameObject.activeSelf && _replay.Count < 600) _replay.Add(_finalBall.transform.position);
            if (allowPlayerInput) HandleInput();
        }

        private void LateUpdate()
        {
            if (_chest == null || !_running) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            // Lò xo-giảm chấn: rương nảy, nghiêng, nén/giãn như bị bóng đập thật.
            _hopVel += (-_hop * 180f - _hopVel * 9f) * dt;
            _hop += _hopVel * dt;
            if (_hop.y < 0f && _squash >= 0.99f && !_finished) _hop.y = Mathf.Max(_hop.y, -0.02f * _size);
            _tiltVel += (-_tilt * 160f - _tiltVel * 8f) * dt;
            _tilt += _tiltVel * dt;
            _squashVel += ((1f - _squash) * 220f - _squashVel * 11f) * dt;
            _squash += _squashVel * dt;

            // Gỡ phần đã cộng khung trước (nếu BoxOpenSequence không ghi đè), rồi cộng phần mới.
            if (_chest.localPosition == _written) _chest.localPosition -= _applied;
            if (_chest.localRotation == _writtenRot) _chest.localRotation = _chest.localRotation * Quaternion.Inverse(_appliedRot);

            Vector3 parentSpaceHop = _chest.parent != null ? _chest.parent.InverseTransformVector(_hop) : _hop;
            _applied = parentSpaceHop;
            _appliedRot = Quaternion.AngleAxis(_tilt + _flip, _tiltAxis);
            _chest.localPosition += _applied;
            _chest.localRotation = _chest.localRotation * _appliedRot;
            _written = _chest.localPosition;
            _writtenRot = _chest.localRotation;

            float s = Mathf.Clamp(_squash, 0.6f, 1.4f);
            _chest.localScale = new Vector3(_baseScale.x * (2f - s), _baseScale.y * s, _baseScale.z * (2f - s));
            _scaleTouched = true;
        }

        /// <summary>Khung lõi: gọi ngay khi bấm mở (trước nắp). Update vẫn dò IsPlaying làm dự phòng.</summary>
        public override void OnPress(ChestRevealSequence reveal)
        {
            if (!_running) Begin(reveal);
        }

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            if (!_running) Begin(reveal);
            // Giờ chốt chính xác hơn: giữ "Hé nắp" (đã kéo dài) + đường "Bật nắp".
            BoxOpenSequence box = reveal.Box;
            if (box != null && box.Stages != null && box.Stages.Count > 3)
                _lockEta = Time.time + box.Stages[2].hold + box.Stages[3].travel;
            StartCoroutine(Substitution());
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            // Dự tính lệch (vd. rương có chìa): nếu cú sút quyết định chưa diễn ra thì diễn ngay, nhanh.
            if (_running && !_finalFired) StartCoroutine(FinalShot(0.25f));
        }

        public override void OnRevealed(ChestRevealSequence reveal)
        {
            if (_running) StartCoroutine(Outro());
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        // ------------------------------------------------------------------ kịch bản

        private void Begin(ChestRevealSequence reveal)
        {
            Cleanup();
            _reveal = reveal;
            _chest = reveal.transform;
            _running = true;
            _tier = reveal.RolledTier;   // CHỈ ĐỌC
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);
            _startTime = Time.time;
            _baseScale = _chest.localScale;
            _written = _chest.localPosition;
            _writtenRot = _chest.localRotation;
            _applied = Vector3.zero;
            _appliedRot = Quaternion.identity;
            _lockEta = Time.time + EstimateLockSeconds(reveal);

            Measure();
            BuildPitch();
            StartCoroutine(Match());
        }

        /// <summary>Giây từ Play tới chốt, cộng thời gian các giai đoạn (rouletteStart=2, lock=3 như mặc định).</summary>
        private static float EstimateLockSeconds(ChestRevealSequence reveal)
        {
            BoxOpenSequence box = reveal.Box;
            if (box == null || box.Stages == null || box.Stages.Count < 4) return 2.8f;
            float sum = 0f;
            for (int i = 0; i < 3; i++) sum += box.Stages[i].travel + box.Stages[i].hold;
            return sum + box.Stages[3].travel;
        }

        private IEnumerator Match()
        {
            Play(kickoffClip, 1f);
            StartCrowd();
            _drums = StartCoroutine(Drums());
            StartCoroutine(RefereeWalkIn());
            StartCoroutine(Rise(_postL.transform.parent, 0.5f));

            yield return new WaitForSeconds(0.9f);
            // Cú sút 1: trúng cột dọc (với MỌI bậc, để giữ bí mật).
            TheThaoBall b1 = _balls.Count > 0 ? _balls[0] : SpawnBall(_spot, false);
            Kickoff(b1, PostPoint(_postL, 0.55f) + _right * (_ballR * 0.6f), 0.55f, -_right * (0.25f * _size), 0.25f * _size, TheThaoBall.Ending.ReleaseAtEnd, 0.88f);
            _crowdTarget = 0.35f;

            // Chuẩn bị cú sút quyết định: bảng bù giờ ~2 s trước chốt, sút 0,7 s trước chốt.
            while (Time.time < _lockEta - 2.0f) yield return null;
            if (_finalFired) yield break;
            _addedBoard = Board("BuGio", new Color(0.1f, 0.9f, 0.3f), _center + _right * (_halfWidth + 1.3f * _size) + Vector3.up * _height * 1.4f);
            StartCoroutine(Pop(_addedBoard, 0.35f));
            Play(riserClip, 0.8f);
            _crowdTarget = 0.6f;
            _crowdPitch = 1.08f;

            while (Time.time < _lockEta - 0.7f) yield return null;
            if (!_finalFired) yield return FinalShot(0.7f);
        }

        private IEnumerator Substitution()
        {
            // Bảng thay người giơ lên, bóng mới rơi xuống chấm phạt đền, rồi cú sút 2 xoáy dội xà.
            _subBoard = Board("ThayNguoi", new Color(1f, 0.55f, 0.1f), _spot + _right * (0.8f * _size) + Vector3.up * _height * 1.1f);
            StartCoroutine(Pop(_subBoard, 0.3f));
            TheThaoBall b2 = SpawnBall(_spot + Vector3.up * (_height * 2.2f), true);
            yield return new WaitForSeconds(0.75f);
            if (_finalFired || b2 == null) yield break;
            float side = _rng.NextDouble() < 0.5 ? 1f : -1f;
            Kickoff(b2, BarPoint(0.3f * side), 0.6f, _right * (side * 0.45f * _size), 0.4f * _size, TheThaoBall.Ending.ReleaseAtEnd, 0.9f);
            _crowdTarget = 0.45f;
        }

        private IEnumerator FinalShot(float flight)
        {
            _finalFired = true;
            // Lặng như tờ: tắt trống, khán đài nín thở. Chỉ còn tiếng sút.
            _crowdTarget = 0.06f;
            _crowdPitch = 0.92f;
            if (_riser != null) _riser.Stop();

            TheThaoBall ball = SpawnBall(_spot, false);
            _finalBall = ball;
            _replay.Clear();
            yield return null;

            Vector3 mouth = _reveal != null ? _reveal.MouthPosition : _center + Vector3.up * _height * 0.5f;
            float r = (float)_rng.NextDouble() * 2f - 1f;
            switch (_tier)
            {
                case ChestTier.Rac:
                    // Bóng bay vọt qua xà ra ngoài.
                    Kickoff(ball, _center + Vector3.up * (_height * 3.5f) - _front * (_size * 3f) + _right * (r * _size), flight * 1.6f,
                            _right * (r * 0.3f * _size), _height * 1.5f, TheThaoBall.Ending.Free, 0.55f);
                    break;
                case ChestTier.Thuong:
                    // Đá chạm đất, lăn chậm vào.
                    Kickoff(ball, mouth, flight, _right * (0.05f * _size * r), 0f, TheThaoBall.Ending.SwallowIntoMouth, 1f);
                    break;
                case ChestTier.Hiem:
                    Kickoff(ball, mouth, flight, _right * (0.15f * _size * r), 0.35f * _size, TheThaoBall.Ending.SwallowIntoMouth, 1f);
                    break;
                default:
                    // Siêu phẩm: cong hẳn ra ngoài khung rồi ngoặt vào (Rất hiếm, Huyền thoại).
                    float big = _tier == ChestTier.HuyenThoai ? 1.6f : 1.1f;
                    Kickoff(ball, mouth, flight, -_right * (big * _size), _height * (_tier == ChestTier.HuyenThoai ? 1.6f : 0.8f),
                            TheThaoBall.Ending.SwallowIntoMouth, 1f);
                    break;
            }

            if (_tier == ChestTier.Rac)
            {
                yield return new WaitForSeconds(flight * 0.8f);
                Flop();
            }
        }

        /// <summary>Bậc thấp: âm rơi đột ngột, "xịt", rương xẹp.</summary>
        private void Flop()
        {
            if (_crowd != null) _crowd.Stop();     // cắt phụt
            _crowdTarget = 0f;
            if (_drums != null) StopCoroutine(_drums);
            Play(deflateClip, 1f);
            Play(awwClip, 0.8f);
            StartCoroutine(Later(0.5f, () => Play(tromboneClip, 0.9f)));
            StartCoroutine(Later(1.6f, () => Play(whistleEndClip, 0.8f)));
            _squash = 0.8f;             // rương xẹp xuống như bóng xì hơi
            _squashVel = -1.5f;
            _hopVel += Vector3.down * 0.4f * _size;
            _tiltAxis = _front;
            _tiltVel += 90f;
            StartCoroutine(RefereeWave(false));
        }

        /// <summary>Gọi khi một quả bóng lọt vào miệng rương (bóng của cú sút quyết định).</summary>
        public void OnBallScored(TheThaoBall ball)
        {
            Play(netClip, 0.9f);
            Play(kickClip, 0.6f, 0.7f);
            int rank = (int)_tier;
            // Rương nảy lên, nén rồi giãn: mạnh dần theo bậc.
            _hopVel += Vector3.up * (0.6f + 0.6f * rank) * _size;
            _squash = 1.18f + 0.04f * rank;
            _tiltAxis = _right;
            _tiltVel += 120f + 60f * rank;
            StartCoroutine(RefereeWave(true));

            switch (_tier)
            {
                case ChestTier.Thuong:
                    _crowdTarget = 0.25f;
                    Play(clapClip, 0.7f);
                    break;
                case ChestTier.Hiem:
                    _crowdTarget = 0.7f; _crowdPitch = 1.15f;
                    Play(roarClip, 0.8f);
                    break;
                case ChestTier.RatHiem:
                    _crowdTarget = 0.85f; _crowdPitch = 1.2f;
                    Play(roarClip, 1f);
                    Play(airHornClip, 0.8f);
                    StartCoroutine(Var());
                    break;
                case ChestTier.HuyenThoai:
                    _crowdTarget = 1f; _crowdPitch = 1.25f;
                    Play(roarClip, 1f);
                    Play(airHornClip, 1f);
                    StartCoroutine(LegendaryFlip());
                    StartCoroutine(Var());
                    Blast();
                    StartCoroutine(BallRain(8));
                    break;
            }
        }

        /// <summary>Bóng chạm bất cứ thứ gì: âm nảy/cột dọc, rương bị đập thì nảy theo lực va chạm thật.</summary>
        public void OnBallHit(TheThaoBall ball, Collider other, float speed, Vector3 relVel)
        {
            if (speed < 0.15f) return;
            float v = Mathf.Clamp01(speed / 6f);
            if (other == _postL || other == _postR || other == _bar)
            {
                Play(postClip, 0.4f + 0.6f * v, 0.95f + 0.1f * (float)_rng.NextDouble());
                if (!_finalFired || _tier == ChestTier.Rac) Play(oohClip, 0.7f);
                // Khung thành gắn với rương: cột rung thì rương rung.
                _tiltAxis = _front;
                _tiltVel += (other == _postL ? 1f : -1f) * 140f * v;
                _hopVel += Vector3.up * 0.25f * v * _size;
                _crowdTarget = Mathf.Min(1f, _crowdTarget + 0.08f);
            }
            else if (other == _chestCollider)
            {
                Play(kickClip, 0.3f + 0.6f * v, 1.2f);
                Vector3 push = ball.Body != null ? ball.Body.mass * relVel : relVel * 0.4f;
                _hopVel += (Vector3.up * 0.6f * v * _size) - push * 0.02f;
                _tiltAxis = Vector3.Cross(Vector3.up, relVel.normalized);
                if (_tiltAxis.sqrMagnitude < 0.01f) _tiltAxis = _right;
                _tiltVel += 160f * v;
                _squash = Mathf.Min(_squash, 1f - 0.12f * v);
            }
            else
            {
                Play(bounceClip, 0.2f + 0.5f * v, 0.9f + 0.25f * (float)_rng.NextDouble());
            }
        }

        // ------------------------------------------------------------------ sự kiện phụ

        private IEnumerator Drums()
        {
            // Trống cổ vũ: nhịp dồn dần từ 0,7 s xuống 0,13 s cho tới cú sút quyết định.
            int n = 0;
            while (_running && !_finalFired)
            {
                float remain = Mathf.Max(0f, _lockEta - 0.7f - Time.time);
                float total = Mathf.Max(0.5f, _lockEta - 0.7f - _startTime);
                float u = 1f - Mathf.Clamp01(remain / total);
                float interval = Mathf.Lerp(0.7f, 0.13f, u * u);
                Play(drumClip, 0.45f + 0.45f * u, (n % 2 == 0 ? 1f : 1.25f) + 0.15f * u);
                n++;
                yield return new WaitForSeconds(interval);
            }
        }

        private IEnumerator Var()
        {
            // VAR: màn hình hiện, tua lại (rewind), bóng "ma" chiếu chậm 0,3x đường bóng vừa vào lưới, tít tít, xác nhận.
            yield return new WaitForSeconds(0.6f);
            if (!_running || _replay.Count < 3) yield break;
            _varScreen = Board("VAR", new Color(0.15f, 0.35f, 1f), _center - _right * (_halfWidth + 1.4f * _size) + Vector3.up * _height * 1.6f);
            StartCoroutine(Pop(_varScreen, 0.3f));
            Play(rewindClip, 0.8f);
            _crowdTarget = 0.3f;
            GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ghost.name = "TheThao_VarGhost";
            Destroy(ghost.GetComponent<Collider>());
            Own(ghost);
            ghost.transform.SetParent(_stage.transform, true);
            ghost.transform.localScale = Vector3.one * (_ballR * 2f);
            ghost.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.5f, 0.8f, 1f));
            List<Vector3> path = new List<Vector3>(_replay);
            float beepAt = 0f;
            for (float i = 0; i < path.Count - 1; i += 0.3f)
            {
                if (!_running) yield break;
                int k = (int)i;
                ghost.transform.position = Vector3.Lerp(path[k], path[k + 1], i - k);
                beepAt += Time.deltaTime;
                if (beepAt > 0.4f) { beepAt = 0f; Play(varBeepClip, 0.5f); }
                yield return null;
            }
            Play(varBeepClip, 0.8f, 1.5f);
            Play(roarClip, 0.9f);
            _crowdTarget = 0.9f;
            Destroy(ghost);
        }

        private IEnumerator LegendaryFlip()
        {
            // Rương bật lên không, lộn một vòng 360° rồi rơi xuống.
            _hopVel += Vector3.up * 2.5f * _size;
            _tiltAxis = _right;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.8f)
            {
                _flip = 360f * Mathf.SmoothStep(0f, 1f, t);
                _hop += Vector3.up * (Mathf.Sin(t * Mathf.PI) * 0.06f * _size);
                yield return null;
            }
            _flip = 0f;
            _squash = 0.75f;
            Play(bounceClip, 1f, 0.6f);
        }

        /// <summary>Huyền thoại: lực nổ thật đẩy mọi quả bóng trên sân bay toé ra.</summary>
        private void Blast()
        {
            foreach (TheThaoBall b in _balls)
            {
                if (b == null || b.Body == null || b.Swallowed) continue;
                b.Kick(Vector3.zero, Vector3.zero);
                b.Body.AddExplosionForce(6f, _center, _size * 8f, 0.6f, ForceMode.Impulse);
            }
        }

        private IEnumerator BallRain(int count)
        {
            for (int i = 0; i < count && _running; i++)
            {
                Vector3 p = _center + Vector3.up * (_height * (3f + (float)_rng.NextDouble()))
                            + _right * (((float)_rng.NextDouble() * 2f - 1f) * 1.5f * _size)
                            + _front * (((float)_rng.NextDouble() * 2f - 1f) * 1.5f * _size);
                TheThaoBall b = SpawnBall(p, true);
                if (b != null) b.Kick(Vector3.zero, Random3() * 20f);
                yield return new WaitForSeconds(0.12f);
            }
        }

        private IEnumerator RefereeWalkIn()
        {
            if (_referee == null) yield break;
            Vector3 to = _referee.position;
            Vector3 from = to + _right * (2.5f * _size);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.6f)
            {
                _referee.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t)) + Vector3.up * (Mathf.Abs(Mathf.Sin(t * 18f)) * 0.03f * _size);
                yield return null;
            }
            _referee.position = to;
        }

        private IEnumerator RefereeWave(bool goal)
        {
            if (_refArm == null) yield break;
            // Bàn thắng: giơ tay chỉ vào giữa sân; trượt: thổi còi kết thúc, tay buông.
            float target = goal ? -150f : -20f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                _refArm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, target, t));
                yield return null;
            }
        }

        private IEnumerator Outro()
        {
            // Giữ sân thêm một lúc cho bóng lăn nốt, rồi dọn.
            yield return new WaitForSeconds(_tier >= ChestTier.RatHiem ? 4.5f : 3f);
            if (_crowd != null) _crowdTarget = 0f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.6f)
            {
                if (_stage != null) _stage.transform.localScale = Vector3.one * (1f - t);
                yield return null;
            }
            _finished = true;
            DestroyPitch();
        }

        // ------------------------------------------------------------------ người chơi

        private void HandleInput()
        {
            if (Pressed(true) && _extraBalls < maxExtraBalls)
            {
                // Thay người: thả bóng từ trên ngay trên rương, đập vào rương thì rương nảy.
                _extraBalls++;
                Vector3 p = _center + Vector3.up * (_height * 2.5f) + _right * (((float)_rng.NextDouble() - 0.5f) * _size * 0.6f);
                TheThaoBall b = SpawnBall(p, true);
                if (b != null) b.Kick(Vector3.zero, Random3() * 10f);
                Play(kickoffClip, 0.35f, 1.3f);
            }
            if (Pressed(false))
            {
                TheThaoBall best = null;
                float bestD = float.MaxValue;
                foreach (TheThaoBall b in _balls)
                {
                    if (b == null || b.Swallowed || b.Flying || !b.gameObject.activeSelf) continue;
                    float d = (b.transform.position - _spot).sqrMagnitude;
                    if (d < bestD) { bestD = d; best = b; }
                }
                if (best == null) return;
                Vector3 to = (_center + Vector3.up * _height * 0.6f - best.transform.position);
                float side = _rng.NextDouble() < 0.5 ? -1f : 1f;
                best.Kick(to.normalized * 5f + Vector3.up * 1.2f, Vector3.up * (side * 30f));
                Play(kickClip, 0.8f);
            }
        }

#if ENABLE_INPUT_SYSTEM
        private static bool Pressed(bool dropKey) => Keyboard.current != null && Keyboard.current[dropKey ? Key.B : Key.K].wasPressedThisFrame;
#else
        private static bool Pressed(bool dropKey) => Input.GetKeyDown(dropKey ? KeyCode.B : KeyCode.K);
#endif

        // ------------------------------------------------------------------ dựng sân (khối cơ bản, không cần asset)

        private void Measure()
        {
            Bounds b = new Bounds(_chest.position, Vector3.one * 0.3f);
            bool first = true;
            foreach (Renderer r in _chest.GetComponentsInChildren<Renderer>())
            {
                if (r == null || r is ParticleSystemRenderer) continue;
                if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds);
            }
            _center = b.center;
            _ground = new Vector3(b.center.x, b.min.y, b.center.z);
            _height = Mathf.Max(0.1f, b.size.y);
            _size = Mathf.Max(0.1f, Mathf.Max(b.size.x, b.size.z));
            _halfWidth = _size * 0.5f;
            BoxOpenSequence box = _reveal.Box;
            Vector3 f = box != null ? _chest.TransformDirection(box.FrontDirection) : _chest.forward;
            f.y = 0f;
            _front = f.sqrMagnitude > 0.001f ? f.normalized : Vector3.forward;
            _right = Vector3.Cross(Vector3.up, _front).normalized;
            _ballR = _size * ballRadiusFactor;
            // Chấm phạt đền chéo 40° phía trước: bóng không che máy quay nhìn thẳng vào rương.
            Vector3 dir = (_front * 0.75f + _right * 0.65f).normalized;
            _spot = _ground + dir * (_size * spotDistanceFactor) + Vector3.up * _ballR;
        }

        private void BuildPitch()
        {
            _stage = new GameObject("TheThao_Pitch");
            _stage.transform.position = _ground;
            Own(_stage);

            _bouncy = new PhysicsMaterial("TheThaoBouncy")
            {
                bounciness = 0.78f, dynamicFriction = 0.35f, staticFriction = 0.4f,
                bounceCombine = PhysicsMaterialCombine.Maximum, frictionCombine = PhysicsMaterialCombine.Average,
            };
            _owned.Add(_bouncy);

            // Mặt sân vô hình (để bóng có chỗ nảy kể cả khi cảnh không có sàn).
            GameObject floor = new GameObject("Floor");
            floor.transform.SetParent(_stage.transform, false);
            BoxCollider fc = floor.AddComponent<BoxCollider>();
            fc.size = new Vector3(_size * 14f, 0.02f, _size * 14f);
            fc.center = new Vector3(0f, -0.01f, 0f);
            fc.sharedMaterial = _bouncy;

            // Va chạm của chính rương (rương không có Rigidbody: chuyển động do lò xo trong hook).
            GameObject hit = new GameObject("ChestHit");
            hit.transform.SetParent(_stage.transform, false);
            hit.transform.SetPositionAndRotation(_center, Quaternion.LookRotation(_front));
            BoxCollider hc = hit.AddComponent<BoxCollider>();
            hc.size = new Vector3(_size, _height, _size) * 0.95f;
            hc.sharedMaterial = _bouncy;
            _chestCollider = hc;

            // Khung thành: 2 cột dọc + xà ngang, bao quanh rương.
            GameObject goal = new GameObject("Goal");
            goal.transform.SetParent(_stage.transform, false);
            goal.transform.SetPositionAndRotation(_ground, Quaternion.LookRotation(_front));
            float postH = _height * 1.6f, halfW = _halfWidth + _size * 0.55f, thick = _size * 0.06f;
            _postL = Bar(goal.transform, "PostL", new Vector3(-halfW, postH * 0.5f, 0f), new Vector3(thick, postH * 0.5f, thick), Quaternion.identity);
            _postR = Bar(goal.transform, "PostR", new Vector3(halfW, postH * 0.5f, 0f), new Vector3(thick, postH * 0.5f, thick), Quaternion.identity);
            _bar = Bar(goal.transform, "Crossbar", new Vector3(0f, postH, 0f), new Vector3(thick, halfW, thick), Quaternion.Euler(0f, 0f, 90f));
            goal.transform.localScale = new Vector3(1f, 0.01f, 1f);   // mọc lên trong Rise()

            // Trọng tài: thân con nhộng đen, đầu tròn, một cánh tay xoay được.
            GameObject refRoot = new GameObject("Referee");
            refRoot.transform.SetParent(_stage.transform, false);
            refRoot.transform.position = _ground - _right * (halfW + _size * 0.9f) + _front * (_size * 0.6f);
            refRoot.transform.rotation = Quaternion.LookRotation(_right);
            float rh = _height * 1.3f;
            Deco(refRoot.transform, PrimitiveType.Capsule, new Vector3(0f, rh * 0.5f, 0f), new Vector3(rh * 0.35f, rh * 0.5f, rh * 0.35f), new Color(0.08f, 0.08f, 0.1f));
            Deco(refRoot.transform, PrimitiveType.Sphere, new Vector3(0f, rh * 1.12f, 0f), Vector3.one * rh * 0.3f, new Color(0.95f, 0.75f, 0.6f));
            Deco(refRoot.transform, PrimitiveType.Sphere, new Vector3(0f, rh * 1.05f, rh * 0.16f), Vector3.one * rh * 0.07f, new Color(0.9f, 0.9f, 0.2f));
            GameObject arm = new GameObject("Arm");
            arm.transform.SetParent(refRoot.transform, false);
            arm.transform.localPosition = new Vector3(rh * 0.2f, rh * 0.85f, 0f);
            Deco(arm.transform, PrimitiveType.Cube, new Vector3(0f, -rh * 0.22f, 0f), new Vector3(rh * 0.09f, rh * 0.45f, rh * 0.09f), new Color(0.08f, 0.08f, 0.1f));
            _referee = refRoot.transform;
            _refArm = arm.transform;

            // Quả bóng đầu tiên nằm sẵn ở chấm phạt đền.
            SpawnBall(_spot, false);
        }

        private TheThaoBall SpawnBall(Vector3 at, bool dynamic)
        {
            if (_stage == null) return null;
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "TheThao_Ball";
            go.transform.SetParent(_stage.transform, true);
            go.transform.position = at;
            go.transform.localScale = Vector3.one * (_ballR * 2f);
            go.GetComponent<Renderer>().sharedMaterial = Mat(Color.white);
            go.GetComponent<Collider>().sharedMaterial = _bouncy;
            // Đai đen quanh bóng để nhìn thấy xoáy.
            Deco(go.transform, PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.02f, 0.06f, 1.02f), new Color(0.05f, 0.05f, 0.05f));
            Deco(go.transform, PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.02f, 0.06f, 1.02f), new Color(1f, 0.55f, 0.1f), Quaternion.Euler(90f, 0f, 0f));

            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.43f;
            rb.linearDamping = 0.08f;
            rb.angularDamping = 0.3f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic = !dynamic;

            TheThaoBall ball = go.AddComponent<TheThaoBall>();
            ball.Init(this, rb);
            _balls.Add(ball);
            return ball;
        }

        private void Kickoff(TheThaoBall ball, Vector3 target, float flight, Vector3 curve, float lift, TheThaoBall.Ending ending, float release)
        {
            if (ball == null) return;
            Transform mouth = null;
            if (ending == TheThaoBall.Ending.SwallowIntoMouth && _chestCollider != null) mouth = _chestCollider.transform;
            ball.Shoot(target, flight, curve, lift, ending, mouth, release);
            Play(kickClip, 0.9f, 0.9f + 0.2f * (float)_rng.NextDouble());
        }

        private Vector3 PostPoint(Collider post, float h) => post.transform.position + Vector3.up * (_height * 1.6f * (h - 0.5f));

        private Vector3 BarPoint(float along) => _bar.transform.position + _right * (along * _halfWidth);

        private Collider Bar(Transform parent, string n, Vector3 local, Vector3 scale, Quaternion rot)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);   // có sẵn CapsuleCollider
            go.name = n;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localRotation = rot;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Mat(Color.white);
            Collider c = go.GetComponent<Collider>();
            c.sharedMaterial = _bouncy;
            return c;
        }

        private Transform Board(string n, Color color, Vector3 at)
        {
            GameObject go = Deco(_stage.transform, PrimitiveType.Cube, Vector3.zero, new Vector3(_size * 0.7f, _size * 0.45f, _size * 0.04f), color);
            go.name = "Board_" + n;
            go.transform.SetPositionAndRotation(at, Quaternion.LookRotation(_front));
            // Viền đen + "chữ số" là hai vạch trắng (bảng của trọng tài thứ tư).
            Deco(go.transform, PrimitiveType.Cube, new Vector3(-0.18f, 0f, -0.6f), new Vector3(0.12f, 0.6f, 0.2f), Color.white);
            Deco(go.transform, PrimitiveType.Cube, new Vector3(0.18f, 0f, -0.6f), new Vector3(0.12f, 0.6f, 0.2f), Color.white);
            go.transform.localScale = Vector3.zero;
            return go.transform;
        }

        private GameObject Deco(Transform parent, PrimitiveType type, Vector3 local, Vector3 scale, Color color) =>
            Deco(parent, type, local, scale, color, Quaternion.identity);

        private GameObject Deco(Transform parent, PrimitiveType type, Vector3 local, Vector3 scale, Color color, Quaternion rot)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            Collider c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localRotation = rot;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Mat(color);
            return go;
        }

        private Material Mat(Color c)
        {
            Material m = ChestFxFactory.CreateUnlit(c, false, "TheThao_Mat");
            _owned.Add(m);
            return m;
        }

        private IEnumerator Rise(Transform t, float seconds)
        {
            Vector3 full = Vector3.one;
            for (float u = 0f; u < 1f && t != null; u += Time.deltaTime / seconds)
            {
                t.localScale = new Vector3(1f, Mathf.Max(0.01f, 1f + 0.15f * Mathf.Sin(u * Mathf.PI) - (1f - u) * (1f - u)), 1f);
                yield return null;
            }
            if (t != null) t.localScale = full;
        }

        private IEnumerator Pop(Transform t, float seconds)
        {
            Vector3 target = new Vector3(_size * 0.7f, _size * 0.45f, _size * 0.04f);
            for (float u = 0f; u < 1f && t != null; u += Time.deltaTime / seconds)
            {
                float k = 1f + 0.25f * Mathf.Sin(u * Mathf.PI);
                t.localScale = target * (u * k);
                yield return null;
            }
            if (t != null) t.localScale = target;
        }

        private IEnumerator Later(float s, System.Action a)
        {
            yield return new WaitForSeconds(s);
            if (_running) a();
        }

        private Vector3 Random3() =>
            new Vector3((float)_rng.NextDouble() * 2f - 1f, (float)_rng.NextDouble() * 2f - 1f, (float)_rng.NextDouble() * 2f - 1f);

        // ------------------------------------------------------------------ âm

        private void StartCrowd()
        {
            _crowd = LoopSource("Crowd", crowdLoopClip, true);
            _riser = LoopSource("Riser", null, false);
            _crowdTarget = 0.18f;
            _crowdPitch = 1f;
        }

        private AudioSource LoopSource(string n, AudioClip clip, bool play)
        {
            if (_stage == null) return null;
            GameObject go = new GameObject("TheThao_" + n);
            go.transform.SetParent(_stage.transform, false);
            AudioSource s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = clip != null;
            s.clip = clip;
            s.volume = 0f;
            s.spatialBlend = 0f;
            if (play && clip != null) s.Play();
            return s;
        }

        private void Play(AudioClip clip, float volume, float pitch = 1f)
        {
            if (clip == null || _reveal == null) return;
            if (clip == riserClip && _riser != null)
            {
                _riser.clip = clip;
                _riser.loop = false;
                _riser.volume = volume * extraVolume;
                _riser.Play();
                return;
            }
            _reveal.PlayOneShot(clip, volume * extraVolume, pitch);
        }

        // ------------------------------------------------------------------ dọn

        private void Own(Object o) => _owned.Add(o);

        private void DestroyPitch()
        {
            for (int i = _owned.Count - 1; i >= 0; i--) if (_owned[i] != null) Destroy(_owned[i]);
            _owned.Clear();
            _balls.Clear();
            _stage = null;
            _crowd = _riser = null;
            _finalBall = null;
        }

        private void Cleanup()
        {
            StopAllCoroutines();
            DestroyPitch();
            if (_chest != null)
            {
                if (_chest.localPosition == _written) _chest.localPosition -= _applied;
                if (_chest.localRotation == _writtenRot) _chest.localRotation = _chest.localRotation * Quaternion.Inverse(_appliedRot);
                if (_scaleTouched) _chest.localScale = _baseScale;
            }
            _applied = Vector3.zero;
            _appliedRot = Quaternion.identity;
            _hop = _hopVel = Vector3.zero;
            _tilt = _tiltVel = _flip = _squashVel = 0f;
            _squash = 1f;
            _scaleTouched = false;
            _running = _finalFired = _finished = false;
            _extraBalls = 0;
            _replay.Clear();
        }

#if UNITY_EDITOR
        /// <summary>Tự điền âm còn trống từ Audio/Chests/the_thao/ (không cần sửa menu dùng chung).</summary>
        private void OnValidate() => TheThaoFxPreset.FillHookClips(this);
#endif
    }
}

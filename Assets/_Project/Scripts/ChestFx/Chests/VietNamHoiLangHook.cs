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
    /// Kịch bản "ĐÊM HỘI LÀNG" của Chest_VietNam (thay VietNamHook cũ). Chỉ trang trí: KHÔNG đọc/đổi bậc trước khi
    /// ChestRevealSequence báo OnTierLocked; vòng màu 5 bậc và bóng vật phẩm vẫn do lõi làm.
    ///
    /// Dòng thời gian (t = 0 là lúc bấm mở, tức lúc lõi Instantiate prefab này trong Play):
    /// - 0 s: rương "hạ thủy" — mặt nước hiện ra, rương bập bềnh (cả rương nhấp nhô + nghiêng). Đàn bầu ngân (lặp),
    ///   trống cái gõ thưa. Đèn hoa đăng (Rigidbody thật) lần lượt được thả ở xa, trôi tụ về vòng quanh rương, va
    ///   nhau và va rương (rương chao theo cú va).
    /// - Dâng dần tới lúc khoá: nhịp trống nhanh dần (0,75 s → 0,11 s/nhịp), mõ chen nhịp lẻ, đàn bầu lên cao độ và to dần,
    ///   đom đóm tụ dưới vành nón, rương bập bềnh mạnh dần. Giống hệt nhau mọi bậc (giữ bí mật).
    /// - Tương tác: phím L (hoặc gọi <see cref="OfferLantern"/>) = người chơi thả thêm đèn ở chỗ trỏ chuột trên mặt nước.
    ///   Mỗi đèn đẩy nhịp lên thêm một chút. Thả đủ <see cref="offersForTurtle"/> đèn trước lúc khoá thì Rùa vàng trồi
    ///   lên, bơi nửa vòng, húc rương một cái rồi lặn (sự kiện phụ, không ảnh hưởng kết quả).
    /// - Khoá bậc: Rác = "XỊT" (đàn bầu tụt, xì hơi), đèn tắt phụt, rương chìm "bloop" rồi nổi lên, ếch nhảy lên nắp kêu ộp.
    ///   Thường = trống im bặt, một tiếng mõ lẻ loi, đèn dạt ra xa. Hiếm trở lên = trống đồng nổ "BÙM", đèn bị hất tung
    ///   rồi thành đèn trời bay lên, rương nhảy khỏi mặt nước (đom đóm nâng), (bậc − 1) chim Lạc sà qua thả bánh chưng
    ///   rơi xuống nước (vật lý). Huyền thoại: rương lơ lửng lâu hơn, mọi đèn bay lên.
    /// </summary>
    public class VietNamHoiLangHook : ChestFxHook
    {
        [Header("Âm (VietNamHoiLangSetup tự gắn từ Audio/Chests/viet_nam/hoi_lang/)")]
        public AudioClip droneClip;
        public AudioClip drumClip;
        public AudioClip moClip;
        public AudioClip plopClip;
        public AudioClip deflateClip;
        public AudioClip bloopClip;
        public AudioClip frogClip;
        public AudioClip boomClip;
        public AudioClip flapClip;
        public AudioClip turtleClip;
        [Range(0f, 1f)] public float masterVolume = 0.8f;

        [Header("Nhịp")]
        [Tooltip("Ước lượng giây từ lúc bấm tới lúc khoá khi không đọc được giai đoạn của BoxOpenSequence.")]
        [SerializeField] private float fallbackLockSeconds = 6.5f;
        [SerializeField] private float beatSlow = 0.75f;
        [SerializeField] private float beatFast = 0.11f;

        [Header("Đèn và tương tác")]
        [SerializeField] private int autoLanterns = 12;
        [SerializeField] private int maxLights = 6;
        [SerializeField] private int offersForTurtle = 3;
        [SerializeField] private float waterRadius = 3.2f;
        [Tooltip("0 = mỗi lần một khác. Chỉ ảnh hưởng trang trí.")]
        [SerializeField] private int seed;

        private enum Phase { Idle, Build, Ending }

        private ChestRevealSequence _reveal;
        private Transform _chest;
        private Phase _phase = Phase.Idle;
        private System.Random _rng;
        private float _t, _expectedLock, _nextBeat, _nextLantern;
        private int _beat, _autoSpawned, _offers, _lights;
        private bool _turtleDone;
        private float _waterY, _chestRadius, _chestHeight;
        private Vector3 _center;

        private readonly List<Object> _owned = new List<Object>();
        private readonly List<GameObject> _world = new List<GameObject>();
        private readonly List<VietNamHoiLangFloater> _floaters = new List<VietNamHoiLangFloater>();
        private AudioSource _drone, _beatSrc, _sfx;
        private ParticleSystem _fireflies;
        private ChestFxProfile _fireflyStyle;
        private GameObject _bump;

        // Tư thế cả rương: cộng thêm lên tư thế BoxOpenSequence ghi (xem LateUpdate).
        private Vector3 _baseLocalPos;
        private Quaternion _baseLocalRot;
        private Vector3 _appliedPos;
        private Quaternion _appliedRot = Quaternion.identity;
        private Vector3 _lastWrittenPos;
        private bool _poseActive;
        private float _lift, _liftTarget, _liftSpeed = 1f;
        private Vector2 _tiltKick, _tiltKickVel;
        private float _bobAmp = 0.008f, _tiltAmp = 1.5f;

        public void Own(Object o) { if (o != null) _owned.Add(o); }

        // ------------------------------------------------------------------ Bắt đầu NGAY khi bấm mở

        private void Start()
        {
            _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null || !_reveal.IsPlaying) return; // prefab đặt tay trong cảnh, không chạy.
            _chest = _reveal.transform;
            _rng = new System.Random(seed != 0 ? seed : System.Environment.TickCount);
            MeasureChest();
            _expectedLock = EstimateLockSeconds();
            BuildStage();
            _phase = Phase.Build;
            _nextBeat = 0.15f;
            _nextLantern = 0.05f;
        }

        private void MeasureChest()
        {
            Bounds b = new Bounds(_chest.position, Vector3.one * 0.6f);
            bool any = false;
            foreach (Renderer r in _chest.GetComponentsInChildren<Renderer>())
            {
                if (r == null || r.transform.IsChildOf(transform) || r is ParticleSystemRenderer) continue;
                if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            }
            _center = new Vector3(b.center.x, 0f, b.center.z);
            _chestHeight = Mathf.Max(0.2f, b.size.y);
            _chestRadius = Mathf.Max(0.2f, Mathf.Max(b.extents.x, b.extents.z));
            _waterY = b.min.y + _chestHeight * 0.12f;
            _center.y = _waterY;
        }

        private float EstimateLockSeconds()
        {
            BoxOpenSequence box = _reveal.Box;
            if (box == null || box.Stages == null || box.Stages.Count < 4) return fallbackLockSeconds;
            float s = 0f;
            for (int i = 0; i < 4; i++) s += box.Stages[i].travel + (i < 3 ? box.Stages[i].hold : 0f);
            return Mathf.Max(2.5f, s + 1.2f); // lõi thường kéo dài "Hé nắp" để đủ vòng quay.
        }

        private void BuildStage()
        {
            _world.Add(VietNamHoiLangProps.WaterDisc(_center, waterRadius, this));

            // Va chạm của rương (rương thường không có collider): hộp riêng, lớp Ignore Raycast, tự gỡ khi reset.
            _bump = new GameObject("HoiLangChestBump");
            _bump.hideFlags = HideFlags.DontSave;
            _bump.layer = 2;
            _bump.transform.SetParent(_chest, false);
            _bump.transform.position = new Vector3(_center.x, _waterY, _center.z);
            BoxCollider bc = _bump.AddComponent<BoxCollider>();
            bc.size = new Vector3(_chestRadius * 1.6f, _chestHeight * 0.6f, _chestRadius * 1.6f);

            _drone = MakeSource("HoiLangDrone", true);
            _beatSrc = MakeSource("HoiLangBeat", false);
            _sfx = MakeSource("HoiLangSfx", false);
            if (droneClip != null)
            {
                _drone.clip = droneClip;
                _drone.pitch = 0.85f;
                _drone.volume = 0.12f * masterVolume;
                _drone.Play();
            }

            _fireflyStyle = ScriptableObject.CreateInstance<ChestFxProfile>();
            _fireflyStyle.hideFlags = HideFlags.DontSave;
            _fireflyStyle.particleLook = ParticleLook.SoftDot;
            _fireflyStyle.particleDirection = ParticleDirection.Swirl;
            _fireflyStyle.particleSpeed = 0.25f;
            _fireflyStyle.particleSize = 0.03f;
            _fireflyStyle.particleLifetime = 2.2f;
            _fireflyStyle.burstGravity = -0.03f;
            _fireflyStyle.secondaryColor = new Color(0.7f, 1f, 0.4f);
            _fireflyStyle.secondaryMix = 0.5f;
            _owned.Add(_fireflyStyle);
            _fireflies = ChestFxFactory.CreateParticles("HoiLangFireflies", null, _fireflyStyle, _chestRadius * 2f, true);
            ChestFxFactory.SetParticleColor(_fireflies, new Color(0.85f, 1f, 0.45f), _fireflyStyle, 1f);
            _world.Add(_fireflies.gameObject);

            _baseLocalPos = _chest.localPosition;
            _baseLocalRot = _chest.localRotation;
            _lastWrittenPos = _chest.localPosition;
            _poseActive = true;
        }

        private AudioSource MakeSource(string name, bool loop)
        {
            GameObject go = new GameObject(name);
            go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(transform, false);
            AudioSource s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = loop;
            s.spatialBlend = 0.3f;
            return s;
        }

        // ------------------------------------------------------------------ Dâng nhịp

        private void Update()
        {
            if (_phase == Phase.Idle) return;
            float dt = Time.deltaTime;
            _t += dt;
            ReadInput();
            if (_phase != Phase.Build) return;

            float k = Tension();
            _drone.pitch = Mathf.Lerp(0.85f, 1.35f, k * k);
            _drone.volume = Mathf.Lerp(0.12f, 0.55f, k) * masterVolume;
            _bobAmp = Mathf.Lerp(0.008f, 0.03f, k);
            _tiltAmp = Mathf.Lerp(1.5f, 6f, k);

            if (_t >= _nextBeat)
            {
                _beat++;
                float interval = Mathf.Lerp(beatSlow, beatFast, Mathf.Pow(k, 1.3f));
                _nextBeat = _t + interval;
                bool drum = k < 0.4f || _beat % 2 == 0;
                AudioClip c = drum ? drumClip : moClip;
                if (c != null)
                {
                    _beatSrc.pitch = drum ? Mathf.Lerp(0.9f, 1.15f, k) : 1f + (float)_rng.NextDouble() * 0.1f;
                    _beatSrc.PlayOneShot(c, Mathf.Lerp(0.35f, 0.9f, k) * masterVolume);
                }
                if (drum) _tiltKick += new Vector2((float)_rng.NextDouble() - 0.5f, (float)_rng.NextDouble() - 0.5f) * k * 4f;
            }

            if (_autoSpawned < autoLanterns && _t >= _nextLantern)
            {
                _autoSpawned++;
                _nextLantern = _t + Mathf.Lerp(0.6f, 0.22f, k);
                float a = (float)_rng.NextDouble() * Mathf.PI * 2f;
                float r = Mathf.Lerp(waterRadius * 0.65f, waterRadius * 0.9f, (float)_rng.NextDouble());
                SpawnLantern(_center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * r + Vector3.up * 0.25f, false);
            }

            if (_fireflies != null)
            {
                _fireflies.transform.position = _reveal.MouthPosition;
                if ((float)_rng.NextDouble() < k * 0.6f) _fireflies.Emit(1 + Mathf.RoundToInt(k * 2f));
            }
        }

        /// <summary>0 → 1 theo thời gian tới lúc khoá dự kiến; mỗi đèn người chơi thả cộng thêm.</summary>
        private float Tension() => Mathf.Clamp01(_t / _expectedLock + _offers * 0.05f);

        private void ReadInput()
        {
            if (_phase != Phase.Build) return;
            bool pressed;
            Vector3 mouse;
#if ENABLE_INPUT_SYSTEM
            pressed = Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame;
            mouse = Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Vector3.zero;
#else
            pressed = Input.GetKeyDown(KeyCode.L);
            mouse = Input.mousePosition;
#endif
            if (!pressed) return;
            Camera cam = Camera.main;
            Vector3? at = null;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(mouse);
                if (Mathf.Abs(ray.direction.y) > 1e-4f)
                {
                    float d = (_waterY - ray.origin.y) / ray.direction.y;
                    Vector3 p = ray.origin + ray.direction * d;
                    Vector3 flat = p - _center;
                    flat.y = 0f;
                    if (d > 0f && flat.magnitude < waterRadius) at = p;
                }
            }
            OfferLantern(at);
        }

        /// <summary>
        /// Người chơi thả một đèn hoa đăng (lễ vật). <paramref name="at"/> = điểm trên mặt nước, null = chỗ ngẫu nhiên
        /// phía trước rương. Gọi được từ UI của game. Chỉ có tác dụng trước lúc khoá bậc.
        /// </summary>
        public void OfferLantern(Vector3? at = null)
        {
            if (_phase != Phase.Build) return;
            Vector3 p;
            if (at.HasValue) p = at.Value;
            else
            {
                float a = (float)_rng.NextDouble() * Mathf.PI - Mathf.PI * 0.5f;
                Vector3 fwd = _reveal.Box != null ? _reveal.Box.FrontDirection : Vector3.forward;
                p = _center + Quaternion.AngleAxis(a * Mathf.Rad2Deg * 0.5f, Vector3.up) * _chest.TransformDirection(fwd).normalized * (_chestRadius + 0.8f);
            }
            p.y = _waterY + 0.35f;
            _offers++;
            SpawnLantern(p, true);
            if (_offers >= offersForTurtle && !_turtleDone)
            {
                _turtleDone = true;
                StartCoroutine(TurtleRoutine());
            }
        }

        private void SpawnLantern(Vector3 pos, bool byPlayer)
        {
            bool light = _lights < maxLights;
            if (light) _lights++;
            VietNamHoiLangFloater f = VietNamHoiLangProps.Lantern(pos, byPlayer ? 0.26f : 0.2f, light, this, _rng);
            f.transform.rotation = Quaternion.Euler(0f, (float)_rng.NextDouble() * 360f, 0f);
            f.WaterY = _waterY;
            f.Center = _center;
            f.GatherRadius = _chestRadius + 0.25f + (float)_rng.NextDouble() * 0.5f;
            _floaters.Add(f);
            _world.Add(f.gameObject);
            if (plopClip != null) _sfx.PlayOneShot(plopClip, (byPlayer ? 0.8f : 0.35f) * masterVolume);
            StartCoroutine(Ripple(new Vector3(pos.x, _waterY + 0.003f, pos.z), byPlayer ? 0.5f : 0.3f, 0.25f));
        }

        /// <summary>Đèn hoặc quà va rương: rương chao theo hướng cú va (cả rương nghiêng).</summary>
        public void OnFloaterHit(VietNamHoiLangFloater f, Collider other, float speed)
        {
            if (other == null || _bump == null || other.gameObject != _bump) return;
            Vector3 d = _chest.InverseTransformDirection(f.transform.position - _chest.position);
            _tiltKick += new Vector2(-d.z, d.x).normalized * Mathf.Clamp(speed * 6f, 0.5f, 6f);
        }

        // ------------------------------------------------------------------ Khoá bậc: rẽ nhánh

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_phase != Phase.Build) return;
            _phase = Phase.Ending;
            if (tier <= ChestTier.Thuong) StartCoroutine(LowEnding(tier));
            else StartCoroutine(HighEnding(tier, color));
        }

        private IEnumerator LowEnding(ChestTier tier)
        {
            // Đàn bầu tụt cao độ như xì hơi rồi tắt hẳn.
            float start = _drone.pitch, v0 = _drone.volume;
            if (tier == ChestTier.Rac && deflateClip != null) _sfx.PlayOneShot(deflateClip, 0.9f * masterVolume);
            else if (moClip != null) { _beatSrc.pitch = 0.7f; _beatSrc.PlayOneShot(moClip, 0.6f * masterVolume); }
            for (float t = 0f; t < 0.4f; t += Time.deltaTime)
            {
                _drone.pitch = Mathf.Lerp(start, 0.3f, t / 0.4f);
                _drone.volume = Mathf.Lerp(v0, 0f, t / 0.4f);
                yield return null;
            }
            _drone.Stop();
            _bobAmp = 0.006f;
            _tiltAmp = 1f;
            if (_fireflies != null) _fireflies.Stop();

            if (tier == ChestTier.Thuong)
            {
                foreach (VietNamHoiLangFloater f in _floaters) if (f != null) { f.Gather = -1f; f.SetLit(0.5f); }
                SetLift(-_chestHeight * 0.06f, 2f);
                yield return new WaitForSeconds(0.6f);
                SetLift(0f, 1f);
                yield break;
            }

            // Rác: đèn tắt phụt lần lượt, rương chìm "bloop" rồi nổi lên, ếch nhảy lên nắp.
            for (int i = 0; i < _floaters.Count; i++)
            {
                if (_floaters[i] != null) _floaters[i].SetLit(0f);
                if (i % 3 == 2) yield return new WaitForSeconds(0.03f);
            }
            if (bloopClip != null) _sfx.PlayOneShot(bloopClip, 0.8f * masterVolume);
            SetLift(-_chestHeight * 0.35f, 2.5f);
            StartCoroutine(Ripple(_center + Vector3.up * 0.003f, _chestRadius * 2.5f, 0.6f));
            yield return new WaitForSeconds(0.7f);
            SetLift(0.02f, 3f);
            yield return new WaitForSeconds(0.35f);
            SetLift(0f, 1f);
            yield return FrogRoutine();
        }

        private IEnumerator FrogRoutine()
        {
            Vector3 from = _center + _chest.right * (_chestRadius + 0.5f);
            GameObject frog = VietNamHoiLangProps.Part(PrimitiveType.Sphere, null, Vector3.zero, new Vector3(0.1f, 0.07f, 0.12f), new Color(0.35f, 0.6f, 0.2f), this, "HoiLangEch");
            frog.transform.position = from;
            frog.layer = 2;
            frog.AddComponent<SphereCollider>().radius = 0.5f;
            Rigidbody rb = frog.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            _world.Add(frog);
            Vector3 target = _reveal.MouthPosition + Vector3.up * 0.15f;
            // Vận tốc bắn để rơi đúng nắp sau 0,6 s (chuyển động ném xiên, trọng lực thật).
            float T = 0.6f;
            Vector3 v = (target - from) / T - 0.5f * Physics.gravity * T;
            rb.linearVelocity = v;
            yield return new WaitForSeconds(T);
            if (frogClip != null) _sfx.PlayOneShot(frogClip, 0.9f * masterVolume);
            _tiltKick += new Vector2(2f, -1f);
        }

        private IEnumerator HighEnding(ChestTier tier, Color tierColor)
        {
            int rank = tier.Rank();
            _drone.Stop();
            if (boomClip != null) _sfx.PlayOneShot(boomClip, masterVolume);
            if (drumClip != null) { _beatSrc.pitch = 0.6f; _beatSrc.PlayOneShot(drumClip, masterVolume); }

            // Hất tung đèn bằng lực nổ từ tâm rương, rồi thành đèn trời.
            foreach (VietNamHoiLangFloater f in _floaters)
            {
                if (f == null || f.Body == null) continue;
                Vector3 d = f.transform.position - _center;
                d.y = 0f;
                float dist = Mathf.Max(0.2f, d.magnitude);
                f.Body.AddForce((d.normalized * 1.2f + Vector3.up * (1.6f + 0.3f * rank)) / dist * 0.25f, ForceMode.Impulse);
                f.Body.AddTorque(new Vector3((float)_rng.NextDouble() - 0.5f, 0f, (float)_rng.NextDouble() - 0.5f) * 0.02f, ForceMode.Impulse);
            }
            if (_fireflies != null)
            {
                ChestFxFactory.SetParticleColor(_fireflies, Color.Lerp(new Color(0.85f, 1f, 0.45f), tierColor, 0.5f), _fireflyStyle, 1.4f);
                _fireflies.Emit(40 + 30 * rank);
            }
            StartCoroutine(Ripple(_center + Vector3.up * 0.003f, waterRadius, 0.9f));

            // Cả rương nhảy khỏi mặt nước (đom đóm nâng), Huyền thoại lơ lửng lâu hơn.
            SetLift(_chestHeight * (0.25f + 0.12f * rank), 6f);
            yield return new WaitForSeconds(0.35f);
            foreach (VietNamHoiLangFloater f in _floaters)
                if (f != null && (tier == ChestTier.HuyenThoai || _rng.NextDouble() < 0.4 + 0.15 * rank)) f.Rise();

            for (int i = 0; i < rank - 1; i++) StartCoroutine(BirdRoutine(i, rank));

            float hover = tier == ChestTier.HuyenThoai ? 2.4f : 0.9f;
            for (float t = 0f; t < hover; t += Time.deltaTime)
            {
                if (_fireflies != null)
                {
                    _fireflies.transform.position = _reveal.MouthPosition - Vector3.up * _chestHeight * 0.6f;
                    if (_rng.NextDouble() < 0.5) _fireflies.Emit(2);
                }
                yield return null;
            }
            SetLift(_chestHeight * 0.05f, 1.2f);
            yield return new WaitForSeconds(0.8f);
            SetLift(0f, 0.8f);
        }

        // ------------------------------------------------------------------ Sự kiện phụ

        private IEnumerator TurtleRoutine()
        {
            if (turtleClip != null) _sfx.PlayOneShot(turtleClip, 0.8f * masterVolume);
            GameObject turtle = VietNamHoiLangProps.Turtle(this, Mathf.Max(1f, _chestRadius * 1.6f));
            _world.Add(turtle);
            float r = _chestRadius + 0.55f;
            float a0 = (float)_rng.NextDouble() * Mathf.PI * 2f;
            const float dur = 2.6f;
            bool nudged = false;
            for (float t = 0f; t < dur && turtle != null; t += Time.deltaTime)
            {
                float u = t / dur;
                float a = a0 + u * Mathf.PI;
                float y = _waterY + Mathf.Lerp(-0.25f, 0.02f, Mathf.SmoothStep(0f, 1f, Mathf.Min(u * 4f, (1f - u) * 4f)));
                Vector3 p = _center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * r;
                p.y = y;
                Vector3 tangent = new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a));
                turtle.transform.SetPositionAndRotation(p, Quaternion.LookRotation(tangent));
                // Rẽ đèn trên đường bơi (đẩy bằng lực thật).
                foreach (VietNamHoiLangFloater f in _floaters)
                {
                    if (f == null || f.Body == null) continue;
                    Vector3 d = f.Body.position - p;
                    d.y = 0f;
                    if (d.magnitude < 0.35f) f.Body.AddForce(d.normalized * 0.4f + tangent * 0.2f, ForceMode.Acceleration);
                }
                if (!nudged && u > 0.5f)
                {
                    nudged = true;
                    Vector3 local = _chest.InverseTransformDirection(_center - p);
                    _tiltKick += new Vector2(local.z, -local.x).normalized * 9f;
                    if (drumClip != null) { _beatSrc.pitch = 1.4f; _beatSrc.PlayOneShot(drumClip, 0.5f * masterVolume); }
                    StartCoroutine(Ripple(p, 0.7f, 0.5f));
                }
                yield return null;
            }
            if (turtle != null) { _world.Remove(turtle); Destroy(turtle); }
        }

        private IEnumerator BirdRoutine(int index, int rank)
        {
            yield return new WaitForSeconds(0.25f + index * 0.35f);
            GameObject bird = VietNamHoiLangProps.Bird(this, Mathf.Max(1f, _chestRadius * 1.5f), out Transform wl, out Transform wr);
            _world.Add(bird);
            float side = index % 2 == 0 ? 1f : -1f;
            Vector3 across = Quaternion.AngleAxis(25f * index, Vector3.up) * _chest.right * side;
            Vector3 a = _center - across * 3.5f + Vector3.up * (_chestHeight + 1.8f);
            Vector3 c = _center + across * 3.5f + Vector3.up * (_chestHeight + 1.6f);
            Vector3 b = _center + Vector3.up * (_chestHeight * 1.1f + 0.2f) * 0.6f; // điểm điều khiển: sà thấp qua rương
            if (flapClip != null) _sfx.PlayOneShot(flapClip, 0.7f * masterVolume);
            const float dur = 1.6f;
            bool dropped = false;
            Vector3 prev = a;
            for (float t = 0f; t < dur && bird != null; t += Time.deltaTime)
            {
                float u = t / dur;
                Vector3 p = (1 - u) * (1 - u) * a + 2 * (1 - u) * u * b + u * u * c;
                Vector3 vel = p - prev;
                prev = p;
                if (vel.sqrMagnitude > 1e-8f) bird.transform.SetPositionAndRotation(p, Quaternion.LookRotation(vel));
                else bird.transform.position = p;
                float flap = Mathf.Sin(t * 22f) * 40f;
                wl.localRotation = Quaternion.Euler(0f, 0f, flap);
                wr.localRotation = Quaternion.Euler(0f, 0f, -flap);
                if (!dropped && u > 0.45f)
                {
                    dropped = true;
                    VietNamHoiLangFloater gift = VietNamHoiLangProps.Gift(p - Vector3.up * 0.1f, this);
                    gift.WaterY = _waterY;
                    gift.Center = _center;
                    gift.GatherRadius = _chestRadius + 0.4f;
                    gift.Body.linearVelocity = vel / Mathf.Max(1e-4f, Time.deltaTime) * 0.6f;
                    gift.Body.AddTorque(Vector3.one * 0.01f, ForceMode.Impulse);
                    _floaters.Add(gift);
                    _world.Add(gift.gameObject);
                    StartCoroutine(SplashWhenLands(gift));
                }
                yield return null;
            }
            if (bird != null) { _world.Remove(bird); Destroy(bird); }
        }

        private IEnumerator SplashWhenLands(VietNamHoiLangFloater f)
        {
            while (f != null && f.transform.position.y > _waterY + 0.03f) yield return null;
            if (f == null) yield break;
            if (plopClip != null) { _sfx.PlayOneShot(plopClip, 0.9f * masterVolume); }
            yield return Ripple(new Vector3(f.transform.position.x, _waterY + 0.003f, f.transform.position.z), 0.6f, 0.45f);
        }

        /// <summary>Gợn nước: vòng mảnh nở ra trên mặt nước rồi tắt (nhỏ, khác "sóng trống đồng" của hook cũ).</summary>
        private IEnumerator Ripple(Vector3 at, float radius, float seconds)
        {
            GameObject go = new GameObject("HoiLangRipple");
            go.hideFlags = HideFlags.DontSave;
            go.transform.position = at;
            Mesh mesh = ChestFxFactory.CreateRingMesh(0.9f);
            Material mat = ChestFxFactory.CreateAdditive(ChestFxFactory.GetTexture(ParticleLook.SoftDot), "HoiLangRipple");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            _world.Add(go);
            for (float t = 0f; t < seconds && go != null; t += Time.deltaTime)
            {
                float k = t / seconds;
                float r = Mathf.Lerp(0.03f, radius, 1f - (1f - k) * (1f - k));
                go.transform.localScale = new Vector3(r, 1f, r);
                ChestFxFactory.SetColor(mat, new Color(0.6f, 0.75f, 1f) * (1f - k) * 0.8f);
                yield return null;
            }
            _world.Remove(go);
            if (go != null) Destroy(go);
            Destroy(mesh);
            Destroy(mat);
        }

        // ------------------------------------------------------------------ Tư thế cả rương

        private void SetLift(float target, float speed)
        {
            _liftTarget = target;
            _liftSpeed = Mathf.Max(0.1f, speed);
        }

        private void LateUpdate()
        {
            if (!_poseActive || _chest == null) return;
            float dt = Time.deltaTime;

            // BoxOpenSequence ghi đè localPosition mỗi khung khi đang chạy giai đoạn; khi nó không ghi, gỡ phần ta cộng lần trước.
            if (_chest.localPosition == _lastWrittenPos) _chest.localPosition -= _appliedPos;
            _chest.localRotation = _chest.localRotation * Quaternion.Inverse(_appliedRot);

            _lift = Mathf.MoveTowards(_lift, _liftTarget, _liftSpeed * dt * Mathf.Max(0.05f, Mathf.Abs(_liftTarget - _lift) + 0.05f) * 3f);
            // Lò xo cho cú chao (va đèn, rùa húc, trống).
            _tiltKickVel += (-_tiltKick * 30f - _tiltKickVel * 5f) * dt;
            _tiltKick += _tiltKickVel * dt;

            float bob = Mathf.Sin(_t * 2.1f) * _bobAmp + Mathf.Sin(_t * 3.3f + 1f) * _bobAmp * 0.4f;
            float rollX = (Mathf.PerlinNoise(_t * 0.7f, 0.1f) - 0.5f) * 2f * _tiltAmp + _tiltKick.x;
            float rollZ = (Mathf.PerlinNoise(0.3f, _t * 0.7f) - 0.5f) * 2f * _tiltAmp + _tiltKick.y;

            _appliedPos = new Vector3(0f, bob + _lift, 0f);
            _appliedRot = Quaternion.Euler(rollX, 0f, rollZ);
            _chest.localPosition += _appliedPos;
            _chest.localRotation = _chest.localRotation * _appliedRot;
            _lastWrittenPos = _chest.localPosition;

            if (_bump != null) _bump.transform.position = new Vector3(_center.x, _waterY + _lift, _center.z);
        }

        private void RestorePose()
        {
            if (!_poseActive || _chest == null) return;
            _poseActive = false;
            _chest.localRotation = _chest.localRotation * Quaternion.Inverse(_appliedRot);
            if (_chest.localPosition == _lastWrittenPos) _chest.localPosition -= _appliedPos;
            _appliedPos = Vector3.zero;
            _appliedRot = Quaternion.identity;
        }

        // ------------------------------------------------------------------ Dọn

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            StopAllCoroutines();
            _phase = Phase.Idle;
            RestorePose();
            if (_drone != null) _drone.Stop();
            foreach (GameObject g in _world) if (g != null) Destroy(g);
            _world.Clear();
            _floaters.Clear();
            if (_fireflies != null)
            {
                Destroy(_fireflies.GetComponent<ParticleSystemRenderer>().sharedMaterial);
                _fireflies = null;
            }
            if (_bump != null) Destroy(_bump);
            _bump = null;
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_NhatBan — "NHÁT KIẾM IAI". Chỉ trang trí: KHÔNG đổi bậc đã quay (đọc reveal.RolledTier để chọn kết).
    /// Nhịp (BoxOpenSequence mặc định):
    ///  0,0 s  bấm mở: katana trong vỏ + nón lá hiện cạnh rương, tiếng sáo nền + tim taiko bắt đầu, lá anh đào lơ lửng.
    ///  ~0,9 s (giai đoạn 0): im lặng ngắn (nền + tim tạm tắt), rút kiếm, MỘT nhát chém vụt qua dây niêm
    ///         (giống nhau mọi bậc: giữ bí mật). Dây/giấy vẫn nằm yên, kiếm trôi chậm 0,35 s (chỉ hoạt hình của kiếm,
    ///         KHÔNG đụng Time.timeScale)...
    ///  +0,35 s: kiếm nhập vỏ "tách", dây + giấy niêm đứt đôi rơi vật lý, rương nảy nhẹ.
    ///  tới lúc khoá: tim dồn dần (80 → 360 nhịp/phút), rương "thở" theo nhịp, kiếm nâng lên thế jodan.
    ///  khoá bậc: Rác = tắt phụt, lặng 0,2 s, tiếng xịt; dép rơi, kiếm chém hụt, kiếm văng, nón rơi, rương xịt xẹp;
    ///            Thường..Huyền thoại = 1/2/3/5 nhát chém ĐỒNG THỜI (+ tối đa 2 khí), "tách", rương bật nhảy.
    /// Âm (kế hoạch 2026-11-25, mục 3): lõi câm; hook tự phát 2 lớp trước chốt (sáo + taiko) và 1 tiếng chốt theo bậc
    /// (<see cref="stingClips"/>), mọi âm là mp3 ElevenLabs.
    /// Tương tác phụ: bấm K (hoặc gọi <see cref="TossPersimmon"/>) ném quả hồng; vật nào có <see cref="NhatBanOffering"/>
    /// bay gần kiếm sẽ bị chém đôi (vật lý) và cộng "khí": nhiều lá hơn, sáo cao hơn, thêm nhát chém ở đoạn kết.
    /// </summary>
    public class NhatBanHook : ChestFxHook
    {
        [System.Serializable]
        public class Sounds
        {
            public AudioClip drone, heartbeat, draw, slash, sheath, ropeSnap, cutFruit;
            [Tooltip("Đuôi Rác: dép bẹp + boing.")] public AudioClip sandal;
            [Tooltip("Đuôi: nón cúi chào (koto).")] public AudioClip bow;
        }

        public Sounds sounds = new Sounds();
        [Tooltip("Tiếng chốt (TC) theo bậc: Rác, Thường, Hiếm, Rất hiếm, Huyền thoại. Hook tự phát (A4/A11).")]
        public AudioClip[] stingClips = new AudioClip[5];
        [Range(0f, 1f), Tooltip("Âm lượng tổng của hook, nhân MỘT lần cho mọi tiếng.")] public float fxVolume = 0.8f;
        private const int MaxTosses = 6;       // trần số quả hồng ném (T7)
        [Tooltip("Phím ném quả hồng cho kiếm chém (thử nghiệm).")] public KeyCode tossKey = KeyCode.K;
        public int decorationSeed;

        private ChestRevealSequence _reveal;
        private Transform _chest;
        private Bounds _bounds;
        private Vector3 _front, _right;
        private float _size, _height;
        private System.Random _rng;
        private readonly List<Object> _owned = new List<Object>();
        private readonly List<GameObject> _spawned = new List<GameObject>();
        private readonly List<Renderer> _hidden = new List<Renderer>();
        private readonly List<Rigidbody> _pending = new List<Rigidbody>();   // mảnh đã cắt, chờ "tách" mới rơi

        private NhatBanProps.Katana _katana;
        private Transform _hat;
        private Vector3 _idlePos;
        private Quaternion _idleRot;
        private Vector3 _hatPos;
        private bool _katanaBusy, _katanaGone;
        private AudioSource _droneSource;
        private ParticleSystem _petals, _cutPetals;
        private ChestFxProfile _petalStyle, _cutStyle;
        private Light _flash;
        private Collider _bodyCollider;

        private bool _started, _cutDone, _locked, _heartbeat, _hush;
        private int _tosses;
        private float _lastEvent = -999f, _quietUntil = -999f;
        private AudioSource _beatSource;
        private float _t0, _expectedLock;
        private int _ki;

        // tư thế rương (cộng thêm, trả lại khi xong)
        private Vector3 _baseScale;
        private Quaternion _baseRot;
        private Vector3 _lastOffset, _lastWritten;
        private float _hopY, _hopV, _spin, _spinTarget, _pulse;
        private Vector3 _squash = Vector3.one, _lean;
        private bool _poseOwned;

        // ------------------------------------------------------------------ vòng đời

        // Lõi mới gọi OnPress ngay trong Play(); Start là dự phòng cho lõi cũ.
        public override void OnPress(ChestRevealSequence reveal) => Begin();

        private void Start() => Begin();

        public override void OnTensionPulse(ChestRevealSequence reveal, float tension)
        {
            _pulse = Mathf.Lerp(0.015f, 0.04f, tension);
        }

        private void Begin()
        {
            if (_started || !enabled) return;
            _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null) { enabled = false; return; }
            _chest = _reveal.transform;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);
            Measure();
            BuildScene();
            _baseScale = _chest.localScale;
            _baseRot = _chest.localRotation;
            _lastWritten = _chest.localPosition;
            _poseOwned = true;
            _t0 = Time.time;
            _expectedLock = ExpectedLockSeconds();
            if (_reveal.Box != null) _reveal.Box.onStageReached.AddListener(OnStage);
            _started = true;
            StartCoroutine(Prelude());
        }

        private void OnStage(int stage)
        {
            if (stage == 0 && !_cutDone) StartCoroutine(Iai());
            if (stage == 2 && !_locked) StartCoroutine(MoveKatana(JodanPos(), Orient(Vector3.up + _right * 0.3f, _front), 0.5f));
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_locked) return;
            _locked = true;
            _heartbeat = false;
            _hush = false;
            StopAllCoroutinesExceptPose();
            // A4: nhịp tắt ngay; nền Thường fade 0,3 s, bậc khác (và Rác) dừng ngay.
            if (_beatSource != null) _beatSource.Stop();
            if (tier == ChestTier.Thuong) StartCoroutine(FadeBed(0.3f));
            else if (_droneSource != null) _droneSource.Stop();
            if (!_cutDone) ReleaseBindingNow();
            foreach (Rigidbody rb in _pending) if (rb != null && rb.isKinematic) NhatBanProps.Release(rb, _front * 0.03f, Vector3.zero);
            _pending.Clear();
            _katanaBusy = false;
            if (_katana != null) _katana.saya.gameObject.SetActive(true);
            StartCoroutine(tier == ChestTier.Rac ? FinaleRac() : Finale(tier, color));
        }

        public override void OnRevealed(ChestRevealSequence reveal)
        {
            StartCoroutine(FadeOut());
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            if (!_started) return;
            _started = false;
            StopAllCoroutines();
            if (_reveal != null && _reveal.Box != null) _reveal.Box.onStageReached.RemoveListener(OnStage);
            if (_poseOwned && _chest != null)
            {
                _chest.localScale = _baseScale;
                _chest.localRotation = _baseRot;
                if (_chest.localPosition == _lastWritten) _chest.localPosition -= _lastOffset;
            }
            _poseOwned = false;
            foreach (Renderer r in _hidden) if (r != null) r.enabled = true;
            _hidden.Clear();
            foreach (GameObject g in _spawned) if (g != null) Destroy(g);
            _spawned.Clear();
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
        }

        private void StopAllCoroutinesExceptPose() => StopAllCoroutines();   // tư thế cập nhật trong LateUpdate, không phải coroutine

        // ------------------------------------------------------------------ dựng cảnh

        private void Measure()
        {
            bool has = false;
            foreach (Renderer r in _chest.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer || r.transform.IsChildOf(transform)) continue;
                if (r.name == "Glow" || (r.transform.parent != null && r.transform.parent.name == "Glow")) continue;
                if (!has) { _bounds = r.bounds; has = true; } else _bounds.Encapsulate(r.bounds);
            }
            if (!has) _bounds = new Bounds(_chest.position + Vector3.up * 0.2f, Vector3.one * 0.4f);
            _size = Mathf.Max(0.1f, Mathf.Max(_bounds.size.x, _bounds.size.z));
            _height = Mathf.Max(0.1f, _bounds.size.y);
            Vector3 f = _reveal.Box != null ? _chest.TransformDirection(_reveal.Box.FrontDirection) : -_chest.forward;
            f.y = 0f;
            _front = f.sqrMagnitude > 1e-4f ? f.normalized : Vector3.back;
            _right = Vector3.Cross(Vector3.up, _front).normalized;
        }

        private void BuildScene()
        {
            _katana = NhatBanProps.BuildKatana(_size * 1.6f, _owned);
            _spawned.Add(_katana.root.gameObject);
            _idlePos = _bounds.center + _right * (_size * 0.85f) + _front * (_size * 0.25f) - Vector3.up * (_height * 0.35f);
            _idleRot = Orient((Vector3.up + _right * 0.15f).normalized, _front);
            _katana.root.SetPositionAndRotation(_idlePos, _idleRot);
            _katana.root.localScale = Vector3.zero;

            _hat = NhatBanProps.BuildHat(_size * 0.35f, _owned);
            _spawned.Add(_hat.gameObject);
            _hatPos = _idlePos + Vector3.up * (_katana.length * 0.95f);
            _hat.position = _hatPos;
            _hat.localScale = Vector3.zero;

            _petalStyle = Style(0.03f, 0.028f, 9f, 0f);
            _petals = ChestFxFactory.CreateParticles("NB_Petals", null, _petalStyle, _size, true);
            ParticleSystem.ShapeModule sh = _petals.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = _size * 0.9f;
            ParticleSystem.MainModule petalMain = _petals.main;
            petalMain.maxParticles = 40;   // T4: lá lơ lửng giữ ≤ 40 hạt
            _petals.transform.position = _bounds.center + Vector3.up * (_height * 0.4f);
            ChestFxFactory.SetParticleColor(_petals, NhatBanFxPreset.Sakura, _petalStyle, 1f);
            _petals.Play();
            _spawned.Add(_petals.gameObject);

            _cutStyle = Style(0.6f, 0.022f, 1.6f, 0.35f);
            _cutPetals = ChestFxFactory.CreateParticles("NB_CutPetals", null, _cutStyle, _size, true);
            ChestFxFactory.SetParticleColor(_cutPetals, NhatBanFxPreset.Sakura, _cutStyle, 1f);
            ParticleSystem.MainModule cutMain = _cutPetals.main;
            cutMain.maxParticles = 160;    // T4: trần cú Huyền thoại
            _cutPetals.Play();
            _spawned.Add(_cutPetals.gameObject);

            GameObject lg = new GameObject("NB_Flash");
            _spawned.Add(lg);
            lg.transform.position = _bounds.center + _front * _size * 0.5f;
            _flash = lg.AddComponent<Light>();
            _flash.type = LightType.Point;
            _flash.range = _size * 5f;
            _flash.intensity = 0f;
            _flash.shadows = LightShadows.None;

            // Va chạm tạm: thân rương (cho dép / nón đáp lên) và sàn nếu bên dưới trống.
            GameObject body = new GameObject("NB_BodyCollider");
            _spawned.Add(body);
            body.transform.position = _bounds.center - Vector3.up * (_height * 0.1f);
            BoxCollider bc = body.AddComponent<BoxCollider>();
            bc.size = new Vector3(_bounds.size.x, _height * 0.8f, _bounds.size.z);
            _bodyCollider = bc;
            if (!Physics.Raycast(_bounds.center, Vector3.down, out RaycastHit _, _height * 0.5f + 0.05f))
            {
                GameObject floor = new GameObject("NB_Floor");
                _spawned.Add(floor);
                floor.transform.position = new Vector3(_bounds.center.x, _bounds.min.y - 0.02f, _bounds.center.z);
                floor.AddComponent<BoxCollider>().size = new Vector3(_size * 8f, 0.04f, _size * 8f);
            }

            _droneSource = gameObject.AddComponent<AudioSource>();
            _droneSource.playOnAwake = false;
            _droneSource.loop = true;
            _droneSource.spatialBlend = 0.5f;
            _droneSource.volume = 0f;
            _droneSource.clip = sounds.drone;
            if (sounds.drone != null) _droneSource.Play();

            _beatSource = gameObject.AddComponent<AudioSource>();   // nhịp riêng: Stop() cắt cả tiếng còn ngân
            _beatSource.playOnAwake = false;
            _beatSource.spatialBlend = 0.5f;
        }

        private ChestFxProfile Style(float speed, float size, float life, float gravity)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = ParticleLook.SoftDot;
            s.particleDirection = ParticleDirection.Radial;   // không dùng Swirl/orbital
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.burstGravity = gravity;
            s.secondaryColor = new Color(1f, 0.92f, 0.95f);
            s.secondaryMix = 0.4f;
            _owned.Add(s);
            return s;
        }

        // ------------------------------------------------------------------ màn dạo đầu: tim + sáo

        private IEnumerator Prelude()
        {
            StartCoroutine(Heartbeat());
            StartCoroutine(BedRamp());
            StartCoroutine(PetalDrift());
            if (_reveal.Box == null) StartCoroutine(StandaloneCut());
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / 0.5f);
                _katana.root.localScale = Vector3.one * k;
                _hat.localScale = Vector3.one * k;
                yield return null;
            }
        }

        private IEnumerator StandaloneCut()
        {
            yield return new WaitForSeconds(0.9f);
            if (!_cutDone) StartCoroutine(Iai());
        }

        /// <summary>Lớp NHỊP (A2): taiko 0,75 s → 0,17 s giữa hai tiếng, âm lượng 0,25 → 0,55, tạm tắt lúc rút kiếm.</summary>
        private IEnumerator Heartbeat()
        {
            _heartbeat = true;
            while (_heartbeat)
            {
                float k = Mathf.Clamp01((Time.time - _t0) / _expectedLock);
                float bpm = Mathf.Lerp(80f, 360f, k * k);
                if (!_hush && sounds.heartbeat != null && _beatSource != null)
                {
                    _beatSource.pitch = Mathf.Lerp(0.9f, 1.25f, k);
                    _beatSource.PlayOneShot(sounds.heartbeat, Mathf.Min(ChestAudioRule.BeatMax, Mathf.Lerp(0.25f, 0.55f, k)) * fxVolume);
                    _pulse = Mathf.Lerp(0.015f, 0.04f, k);
                }
                yield return new WaitForSeconds(60f / bpm);
            }
        }

        /// <summary>Lớp NỀN (A2): sáo vào ≤ 0,2 s sau bấm, âm lượng 0,15 → 0,45, cao độ tăng 45% tới lúc chốt (+ khí).</summary>
        private IEnumerator BedRamp()
        {
            while (true)
            {
                float k = Mathf.Clamp01((Time.time - _t0) / _expectedLock);
                _droneSource.volume = _hush ? 0f : Mathf.Min(ChestAudioRule.BedMax, Mathf.Lerp(0.15f, 0.45f, k)) * fxVolume;
                _droneSource.pitch = Mathf.Min(1.6f, Mathf.Lerp(1f, 1.45f, k) + 0.03f * Mathf.Min(2, _ki));
                yield return null;
            }
        }

        private IEnumerator FadeBed(float seconds)
        {
            float v0 = _droneSource.volume, t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                _droneSource.volume = Mathf.Lerp(v0, 0f, t / seconds);
                yield return null;
            }
            _droneSource.Stop();
        }

        private IEnumerator PetalDrift()
        {
            while (true)
            {
                int n = 1 + _ki;
                _petals.Emit(n);
                yield return new WaitForSeconds(0.12f);
            }
        }

        // ------------------------------------------------------------------ nhát chém mở niêm (giống nhau mọi bậc)

        private IEnumerator Iai()
        {
            _cutDone = true;
            _katanaBusy = true;
            // "Ma": im lặng (nền + tim tạm tắt); cảm giác chậm chỉ nằm ở hoạt hình kiếm, không đụng Time.timeScale
            _hush = true;
            yield return Wait(0.25f);
            Event(sounds.draw, 0.5f);
            yield return Wait(0.22f);

            Transform binding = FindDeep(_chest, "Binding");
            Transform seal = FindDeep(_chest, "Seal");
            Vector3 target = binding != null ? Center(binding) : seal != null ? Center(seal) : _bounds.center;
            float reach = _katana.length * 0.8f;
            Vector3 from = target + _front * reach + _right * (_size * 0.8f) + Vector3.up * (_height * 0.15f);
            Vector3 to = target + _front * reach - _right * (_size * 0.8f) - Vector3.up * (_height * 0.15f);
            Event(sounds.slash, 0.5f);
            List<Vector3> trail = new List<Vector3>();
            yield return Dash(from, to, 0.12f, trail);
            Trail(trail, Color.white, 0.6f);
            EmitAlong(trail, 3);

            // dây / giấy niêm bị cắt nhưng còn nằm yên
            List<Rigidbody> pieces = _pending;
            Vector3 cutNormal = Vector3.Cross(to - from, _front).normalized;
            if (binding != null) Slice(binding, target, cutNormal, pieces);
            if (seal != null) Slice(seal, Center(seal), (cutNormal + _right * 0.5f).normalized, pieces);

            yield return Drift((to - from).normalized, _size * 0.2f, 0.35f);   // kiếm vung chậm 0,35 s rồi "tách"
            yield return Sheath(false);
            _hush = false;
            foreach (Rigidbody rb in pieces)
            {
                float side = Vector3.Dot(rb.position - target, cutNormal) >= 0f ? 1f : -1f;
                NhatBanProps.Release(rb, (_front * 0.25f + cutNormal * side * 0.35f + Vector3.up * 0.1f) * rb.mass,
                    new Vector3(R(-6f, 6f), R(-6f, 6f), R(-6f, 6f)));
            }
            pieces.Clear();
            _hopV = 0.6f;
            _katanaBusy = false;
            yield return Wait(0.22f);   // cách tiếng "tách" ≥ 0,2 s (A3)
            Event(sounds.ropeSnap, 0.5f);
        }

        private void ReleaseBindingNow()
        {
            _cutDone = true;
            foreach (string n in new[] { "Binding", "Seal" })
            {
                Transform t = FindDeep(_chest, n);
                if (t == null) continue;
                List<Rigidbody> p = new List<Rigidbody>();
                Slice(t, Center(t), Vector3.up, p);
                foreach (Rigidbody rb in p) NhatBanProps.Release(rb, _front * 0.03f, Vector3.zero);
            }
        }

        private void Slice(Transform part, Vector3 point, Vector3 normal, List<Rigidbody> outPieces)
        {
            foreach (Rigidbody rb in NhatBanProps.SliceToPieces(part, point, normal, _owned))
            {
                if (rb == null) continue;
                _spawned.Add(rb.gameObject);
                Collider c = rb.GetComponent<Collider>();
                if (c != null && _bodyCollider != null) Physics.IgnoreCollision(c, _bodyCollider);
                outPieces.Add(rb);
            }
            foreach (Renderer r in part.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled) continue;
                r.enabled = false;
                _hidden.Add(r);
            }
        }

        // ------------------------------------------------------------------ kết theo bậc (khớp nhịp file fin_*.wav)

        private IEnumerator FinaleRac()
        {
            // A4 Rác: nền + nhịp đã dừng ở OnTierLocked → lặng 0,2 s → 1 tiếng xịt; 0,6 s sau đó không phát gì khác.
            yield return new WaitForSeconds(ChestAudioRule.RacSilence);
            Sting(0);
            _quietUntil = Time.time + 0.6f;
            Rigidbody sandal = NhatBanProps.BuildSandal(_size * 0.45f, _owned);
            _spawned.Add(sandal.gameObject);
            Vector3 tip = _katana.root.position + _katana.root.right * _katana.length * 0.7f;
            sandal.position = tip + Vector3.up * 0.05f;
            sandal.transform.position = sandal.position;
            sandal.velocity = Vector3.down * 1.5f - _right * 0.6f;
            sandal.angularVelocity = new Vector3(R(-8f, 8f), R(-3f, 3f), R(-8f, 8f));

            // kiếm bật văng
            _katanaGone = true;
            BoxCollider kc = _katana.root.gameObject.AddComponent<BoxCollider>();
            kc.center = new Vector3(_katana.length * 0.3f, 0f, 0f);
            kc.size = new Vector3(_katana.length, _katana.length * 0.05f, _katana.length * 0.05f);
            Rigidbody krb = _katana.root.gameObject.AddComponent<Rigidbody>();
            krb.mass = 0.5f;
            krb.velocity = _right * 1.2f + Vector3.up * 1.6f + _front * 0.4f;
            krb.angularVelocity = _front * 18f;

            _squash = new Vector3(1.04f, 0.96f, 1.04f);
            yield return new WaitForSeconds(0.85f);
            Sfx(sounds.sandal, ChestAudioRule.TailMax);   // đuôi: dép bẹp + boing (≥ 1 s sau chốt)

            // nón rơi
            _hat.gameObject.AddComponent<SphereCollider>().radius = 0.3f;
            Rigidbody hrb = _hat.gameObject.AddComponent<Rigidbody>();
            hrb.mass = 0.1f;
            hrb.angularVelocity = _front * 4f;

            // rương xịt: xẹp + nghiêng theo tiếng sáo tụt
            ParticleSystem.MainModule pm = _petals.main;
            pm.gravityModifier = 0.6f;
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / 0.6f);
                _squash = Vector3.Lerp(Vector3.one, new Vector3(1.07f, 0.86f, 1.07f), k);
                _lean = new Vector3(0f, 0f, 9f * k);
                yield return null;
            }
            yield return new WaitForSeconds(0.4f);
            t = 0f;
            while (t < 1.4f)
            {
                t += Time.deltaTime;
                float k = t / 1.4f;
                float wob = Mathf.Exp(-4f * k) * Mathf.Cos(k * 18f);
                _squash = Vector3.LerpUnclamped(Vector3.one, new Vector3(1.07f, 0.86f, 1.07f), wob);
                _lean = new Vector3(0f, 0f, 9f * wob);
                yield return null;
            }
            _squash = Vector3.one;
            _lean = Vector3.zero;
        }

        private IEnumerator Finale(ChestTier tier, Color color)
        {
            int rank = tier.Rank();   // 1..4
            Sting(rank);              // TC: đúng 1 tiếng chốt theo bậc (Thường 0,7 / Hiếm 0,85 / Rất hiếm, HT 1,0); không HĐ
            int lines = (rank == 1 ? 1 : rank == 2 ? 2 : rank == 3 ? 3 : 5) + Mathf.Min(2, _ki);

            // các nhát chém ĐỒNG THỜI xuyên qua miệng rương
            Vector3 c = _reveal.MouthPosition + _front * (_size * 0.35f);
            _katana.blade.localPosition = Vector3.zero;
            float phase = R(0f, 180f);
            for (int i = 0; i < lines; i++)
            {
                float a = (phase + i * 180f / lines) * Mathf.Deg2Rad;
                Vector3 d = (_right * Mathf.Cos(a) + Vector3.up * Mathf.Sin(a)) * (_size * 0.85f);
                List<Vector3> pts = new List<Vector3> { c - d, c - d * 0.33f, c + d * 0.33f, c + d };
                Trail(pts, Color.Lerp(Color.white, color, 0.35f), 0.42f + 0.1f * rank);
                EmitAlong(pts, 1 + rank);
            }
            yield return Dash(c + _right * _size, c - _right * _size, 0.08f, null);
            _flash.color = color;
            _flash.intensity = 1.5f + rank;

            // 0,34 s: kiếm trôi chậm rồi "tách"
            yield return Drift(-_right, _size * 0.2f, 0.34f);
            yield return Sheath(true);
            _flash.intensity = 3f + 2f * rank;
            _hopV = new[] { 0f, 0.5f, 0.9f, 1.4f, 2.0f }[rank];
            _spinTarget += new[] { 0f, 0f, 0f, 90f, 360f }[rank];
            _cutPetals.transform.position = _reveal.MouthPosition;
            _cutPetals.Emit(Mathf.Min(rank == 4 ? 160 : 60, 20 * rank + 10 * Mathf.Min(2, _ki)));   // T4
            StartCoroutine(Bow());

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                _flash.intensity *= Mathf.Exp(-4f * Time.deltaTime);
                yield return null;
            }
            _flash.intensity = 0f;
        }

        private IEnumerator Bow()
        {
            Sfx(sounds.bow, ChestAudioRule.TailMax);   // đuôi: koto khi nón cúi chào
            float t = 0f;
            Quaternion r0 = _hat.rotation;
            while (t < 1f)
            {
                t += Time.deltaTime;
                _hat.rotation = r0 * Quaternion.AngleAxis(Mathf.Sin(t * Mathf.PI) * 30f, _right);
                yield return null;
            }
            _hat.rotation = r0;
        }

        private IEnumerator FadeOut()
        {
            yield return new WaitForSeconds(0.6f);
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float k = 1f - t / 0.6f;
                if (!_katanaGone && _katana.root != null) _katana.root.localScale = Vector3.one * k;
                if (_hat != null && _hat.GetComponent<Rigidbody>() == null) _hat.localScale = Vector3.one * k;
                yield return null;
            }
        }

        // ------------------------------------------------------------------ kiếm

        private IEnumerator Dash(Vector3 from, Vector3 to, float seconds, List<Vector3> trail)
        {
            Vector3 dir = (to - from).normalized;
            Quaternion rot = Orient(-_front, Vector3.Cross(dir, -_front).normalized);
            _katana.saya.gameObject.SetActive(false);
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / seconds);
                _katana.root.SetPositionAndRotation(Vector3.Lerp(from, to, k), rot);
                trail?.Add(_katana.root.position + _katana.root.right * (_katana.length * 0.7f));
                yield return null;
            }
        }

        /// <summary>Kiếm về chỗ, lưỡi trượt vào vỏ, "tách" đúng lúc chạm.</summary>
        private IEnumerator Sheath(bool tail)
        {
            _katana.root.SetPositionAndRotation(_idlePos, _idleRot);
            _katana.saya.gameObject.SetActive(true);
            _katana.root.position = _idlePos - _katana.root.right * (_katana.length * 0.5f);
            float t = 0f;
            Vector3 start = _katana.root.position;
            while (t < 0.18f)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / 0.18f);
                _katana.root.position = Vector3.Lerp(start, _idlePos, k * k);
                _katana.saya.position = _idlePos + _katana.root.right * (_katana.length * 0.36f);
                yield return null;
            }
            _katana.saya.localPosition = new Vector3(_katana.length * 0.37f, 0f, 0f);
            if (tail) Sfx(sounds.sheath, ChestAudioRule.TailMax);   // sau chốt: đuôi nhỏ
            else Event(sounds.sheath, 0.5f);
        }

        /// <summary>Kiếm trôi chậm theo <paramref name="dir"/> (ease-out): "chậm" chỉ ở hoạt hình kiếm.</summary>
        private IEnumerator Drift(Vector3 dir, float distance, float seconds)
        {
            Vector3 p0 = _katana.root.position;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);
                _katana.root.position = p0 + dir * (distance * (1f - (1f - u) * (1f - u)));
                yield return null;
            }
        }

        private IEnumerator MoveKatana(Vector3 pos, Quaternion rot, float seconds)
        {
            while (_katanaBusy) yield return null;
            Vector3 p0 = _katana.root.position;
            Quaternion r0 = _katana.root.rotation;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / seconds);
                _katana.root.SetPositionAndRotation(Vector3.Lerp(p0, pos, k), Quaternion.Slerp(r0, rot, k));
                yield return null;
            }
            _idlePos = pos;
            _idleRot = rot;
        }

        private Vector3 JodanPos() => _bounds.center + _right * (_size * 0.7f) + _front * (_size * 0.3f) + Vector3.up * (_height * 0.1f);

        /// <summary>Xoay sao cho +X = <paramref name="bladeDir"/>, +Z = mặt lưỡi hướng <paramref name="flat"/>.</summary>
        private static Quaternion Orient(Vector3 bladeDir, Vector3 flat)
        {
            Vector3 n = Vector3.ProjectOnPlane(flat, bladeDir).normalized;
            if (n.sqrMagnitude < 1e-4f) n = Vector3.Cross(bladeDir, Vector3.up).normalized;
            return Quaternion.LookRotation(n, Vector3.Cross(n, bladeDir.normalized));
        }

        // ------------------------------------------------------------------ vật dâng (quả hồng)

        private void Update()
        {
            if (!_started) return;
            if (TossPressed()) TossPersimmon();
            if (_katanaBusy || _katanaGone) return;
            float range = _katana.length * 0.9f;
            for (int i = NhatBanOffering.All.Count - 1; i >= 0; i--)
            {
                NhatBanOffering o = NhatBanOffering.All[i];
                if (o == null || o.cut) continue;
                if ((o.transform.position - _katana.root.position).sqrMagnitude > range * range) continue;
                StartCoroutine(CutOffering(o));
                break;
            }
        }

        private bool TossPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(tossKey);
#endif
        }

        /// <summary>Ném một quả hồng từ phía người chơi (camera) về phía kiếm.</summary>
        public void TossPersimmon()
        {
            if (!_started || _katanaGone || _locked || _tosses >= MaxTosses) return;
            _tosses++;
            Rigidbody rb = NhatBanProps.BuildPersimmon(_size * 0.09f, _owned);
            _spawned.Add(rb.gameObject);
            Camera cam = Camera.main;
            Vector3 from = cam != null ? cam.transform.position + cam.transform.forward * 0.3f - Vector3.up * 0.1f : _idlePos + _front * 1f + Vector3.up * 0.3f;
            rb.transform.position = from;
            Vector3 target = _katana.root.position + Vector3.up * 0.1f;
            Vector3 d = target - from;
            float time = Mathf.Clamp(d.magnitude / 3f, 0.3f, 0.9f);
            rb.velocity = d / time + Vector3.up * (0.5f * 9.81f * time);   // đường parabol tới kiếm
        }

        private IEnumerator CutOffering(NhatBanOffering o)
        {
            o.cut = true;
            _katanaBusy = true;
            Vector3 p = o.transform.position;
            Vector3 n = (Vector3.Cross(p - _katana.root.position, Vector3.up) + Vector3.up * 0.3f).normalized;
            Event(sounds.cutFruit, 0.5f, 0.95f + 0.1f * (float)_rng.NextDouble());
            List<Vector3> pts = new List<Vector3> { p - n.normalized * 0.01f + _right * _size * 0.3f, p - _right * _size * 0.3f };
            Trail(pts, new Color(1f, 0.9f, 0.6f), 0.3f);
            Rigidbody src = o.GetComponent<Rigidbody>();
            Vector3 v = src != null ? src.velocity : Vector3.zero;
            List<Rigidbody> halves = new List<Rigidbody>();
            foreach (Rigidbody rb in NhatBanProps.SliceToPieces(o.transform, p, n, _owned))
            {
                if (rb == null) continue;
                _spawned.Add(rb.gameObject);
                halves.Add(rb);
            }
            o.gameObject.SetActive(false);
            foreach (Rigidbody rb in halves)
            {
                float side = Vector3.Dot(rb.position - p, n) >= 0f ? 1f : -1f;
                NhatBanProps.Release(rb, (v * 0.3f + n * side * 0.8f) * rb.mass, new Vector3(R(-9f, 9f), R(-9f, 9f), R(-9f, 9f)));
            }
            _cutPetals.transform.position = p;
            _cutPetals.Emit(12);
            if (!_locked)
            {
                _ki += o.energy;
                _flash.color = new Color(1f, 0.75f, 0.4f);
                _flash.intensity = 1.5f;
            }
            yield return Wait(0.25f);
            _flash.intensity = 0f;
            _katanaBusy = false;
        }

        // ------------------------------------------------------------------ tư thế rương (cộng thêm lên mọi thứ khác)

        private void LateUpdate()
        {
            if (!_poseOwned || _chest == null) return;
            float dt = Time.deltaTime;
            // nảy: lò xo trọng lực, đáp đất nảy nhẹ
            if (_hopY > 0f || _hopV > 0f)
            {
                _hopV -= 9.81f * dt;
                _hopY += _hopV * dt;
                if (_hopY <= 0f) { _hopY = 0f; _hopV = _hopV < -0.6f ? -_hopV * 0.3f : 0f; }
            }
            _spin = Mathf.MoveTowards(_spin, _spinTarget, 720f * dt);
            _pulse = Mathf.MoveTowards(_pulse, 0f, 0.25f * dt);
            if (_katana != null && !_katanaGone && !_katanaBusy && _katana.root.GetComponent<Rigidbody>() == null)
                _katana.blade.localPosition = new Vector3(0f, R(-1f, 1f) * _pulse * 0.02f, 0f);   // lưỡi run theo tim

            Vector3 offset = Vector3.up * _hopY;
            Vector3 cur = _chest.localPosition;
            Vector3 basePos = cur == _lastWritten ? cur - _lastOffset : cur;
            _lastWritten = basePos + offset;
            _lastOffset = offset;
            _chest.localPosition = _lastWritten;
            _chest.localRotation = _baseRot * Quaternion.Euler(_lean) * Quaternion.AngleAxis(_spin, Vector3.up);
            _chest.localScale = Vector3.Scale(_baseScale, _squash) * (1f + _pulse);
        }

        // ------------------------------------------------------------------ tiện ích

        // ------------------------------------------------------------------ âm (một chỗ nhân fxVolume duy nhất)

        private void Sfx(AudioClip clip, float volume, float pitch = 1f)
        {
            if (clip != null && _reveal != null) _reveal.PlayOneShot(clip, volume * fxVolume, pitch);
        }

        /// <summary>Sự kiện lẻ trước chốt (A3): tối đa 1 tiếng mỗi 0,2 s, ≤ 0,5; im sau chốt Rác.</summary>
        private void Event(AudioClip clip, float volume, float pitch = 1f)
        {
            if (clip == null || _locked || Time.time < _quietUntil) return;
            if (!ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap)) return;
            Sfx(clip, Mathf.Min(ChestAudioRule.EventMax, volume), pitch);
        }

        /// <summary>TC (A4/A11): đúng một tiếng chốt theo bậc, từ <see cref="stingClips"/>.</summary>
        private void Sting(int rank)
        {
            if (stingClips == null || rank < 0 || rank >= stingClips.Length) return;
            float v = rank == 0 ? 0.8f : rank == 1 ? 0.7f : rank == 2 ? 0.85f : ChestAudioRule.StingMax;
            Sfx(stingClips[rank], v);
        }

        private static IEnumerator Wait(float unscaledSeconds)
        {
            float end = Time.unscaledTime + unscaledSeconds;
            while (Time.unscaledTime < end) yield return null;
        }

        private void Trail(List<Vector3> pts, Color color, float seconds)
        {
            if (pts.Count < 2) return;
            GameObject g = new GameObject("NB_SlashTrail");
            _spawned.Add(g);
            LineRenderer lr = g.AddComponent<LineRenderer>();
            Material m = ChestFxFactory.CreateUnlit(color, false, "NB_Slash");
            _owned.Add(m);
            lr.sharedMaterial = m;
            lr.useWorldSpace = true;
            lr.positionCount = pts.Count;
            for (int i = 0; i < pts.Count; i++) lr.SetPosition(i, pts[i]);
            lr.widthCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));
            lr.widthMultiplier = _size * 0.03f;
            StartCoroutine(ShrinkTrail(lr, seconds));
        }

        private IEnumerator ShrinkTrail(LineRenderer lr, float seconds)
        {
            float w0 = lr.widthMultiplier, t = 0f;
            while (t < seconds && lr != null)
            {
                t += Time.unscaledDeltaTime;
                lr.widthMultiplier = w0 * (1f - t / seconds);
                yield return null;
            }
            if (lr != null) lr.gameObject.SetActive(false);
        }

        private void EmitAlong(List<Vector3> pts, int perPoint)
        {
            foreach (Vector3 p in pts)
            {
                _cutPetals.transform.position = p;
                _cutPetals.Emit(perPoint);
            }
        }

        private float ExpectedLockSeconds()
        {
            if (_reveal.Box == null || _reveal.Box.Stages == null) return 3.5f;
            float sum = 0f;
            for (int i = 0; i < _reveal.Box.Stages.Count && i <= 3; i++)
                sum += _reveal.Box.Stages[i].travel + (i < 3 ? _reveal.Box.Stages[i].hold : 0f);
            return Mathf.Max(2f, sum);
        }

        private float R(float a, float b) => a + (b - a) * (float)_rng.NextDouble();

        private static Vector3 Center(Transform t)
        {
            bool has = false;
            Bounds b = new Bounds(t.position, Vector3.zero);
            foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
            {
                if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
            }
            return b.center;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform c in root)
            {
                Transform f = FindDeep(c, name);
                if (f != null) return f;
            }
            return null;
        }
    }
}

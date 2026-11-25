using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_HaiTac — kịch bản "TRỤC VỚT BẰNG ĐẠI BÁC" (docs/superpowers/specs/2026-11-25-kichban-haitac-design.md).
    ///
    /// Bấm mở: nước biển dâng nhấn chìm rương, trống trận + sóng bắt đầu NGAY (không đợi nắp). Một khẩu đại bác trên bè
    /// trôi vào, con vẹt bay tới đậu lên nòng hô "Nạp đạn!". Đạn là Rigidbody thật bay theo đường đạn đạo, va vào khối
    /// va chạm ẩn của rương (OnCollisionEnter) hoặc rơi xuống nước làm sóng xô rương. Người chơi bấm [G] để tự nạp và
    /// bắn thêm, [B] để thả thùng phao (Rigidbody nổi theo lực đẩy Archimedes, trôi vào va rương). Không bấm gì thì vẹt
    /// tự châm ngòi (luồng không bao giờ bị kẹt).
    /// Khi vòng màu chạy, ngòi phát cuối bắt đầu xì; lúc chốt bậc mới biết kết cục:
    ///   Rác: ngòi xịt, viên đạn lăn khỏi nòng rơi "tõm", nhạc tắt phụt, nước rút ọc ọc, rương nổi lên nghiêng lệch, vẹt "rác...".
    ///   Thường/Hiếm: một/hai phát nổ, rương bị hất lên, nước rút. Rất hiếm: loạt hai phát, rương lộn một vòng, thùng bay tung.
    ///   Huyền thoại: loạt ba phát, cột nước phun, rương bay cao xoay 1,5 vòng, biển rút cạn, mưa xu vàng vật lý, vẹt bay vòng.
    ///
    /// Bất biến: bậc do game quyết (ChestRevealSequence.Play); hook chỉ ĐỌC RolledTier trong OnTierLocked. Ngẫu nhiên chỉ để
    /// trang trí (hạt giống decorationSeed). Vòng màu 5 bậc và bóng vật phẩm vẫn do lõi chạy như cũ.
    /// Chuyển động cả rương: mô phỏng khối lượng–lò xo (trọng lực, lực nổi, cản nước, xung lực va chạm) cộng vào vị trí gốc
    /// trong LateUpdate, nên không đánh nhau với rung lắc của BoxOpenSequence.
    /// </summary>
    public class HaiTacSeqHook : ChestFxHook
    {
        [Header("Âm riêng (Audio/Chests/hai_tac/seq/)")]
        [Tooltip("Nhạc nền dâng dần, phát NGAY khi bấm mở (sóng + trống trận nhanh dần + dây kéo lên).")]
        public AudioClip buildClip;
        public AudioClip cannonClip, splashClip, thudClip, loadClip, fuseClip, fizzleClip, parrotClip, drainClip, barrelClip, coinClip;
        [Tooltip("Đoạn kết theo bậc: 0 Rác (xịt, kèn buồn) .. 4 Huyền thoại (loạt pháo + kèn đồng bùng nổ).")]
        public AudioClip[] endingClips = new AudioClip[5];
        [Range(0f, 1f)] public float volume = 0.85f;

        [Header("Tương tác")]
        [Tooltip("[G] nạp đạn và bắn, [B] thả thùng phao.")]
        public bool keyControls = true;
        [Tooltip("Giây (tính từ lúc bấm mở) vẹt tự bắn nếu người chơi không bắn gần đó.")]
        public float[] autoShotTimes = { 1.7f, 3.0f, 4.3f };
        public int maxPlayerShots = 6;
        public float fireCooldown = 0.5f;
        public int maxBarrels = 4;

        [Header("Biển (đơn vị: chiều cao rương)")]
        [Range(0.2f, 1.2f)] public float waterRise = 0.8f;
        public float riseSeconds = 1.1f;
        public float waveEvery = 1.35f;

        [Header("Kết theo bậc (Rác .. Huyền thoại)")]
        public float[] jumpByTier = { 0.04f, 0.18f, 0.38f, 0.7f, 1.15f };
        public float[] spinByTier = { 0f, 0f, 180f, 0f, 540f };
        public float[] flipByTier = { 0f, 0f, 0f, 360f, 0f };
        public int[] boomsByTier = { 0, 1, 1, 2, 3 };
        public int[] coinsByTier = { 0, 0, 5, 14, 30 };

        [Header("Trang trí ngẫu nhiên (không ảnh hưởng kết quả)")]
        [Tooltip("0 = mỗi lần một khác.")]
        public int decorationSeed;

        // ------------------------------------------------------------------ Trạng thái

        private ChestRevealSequence _reveal;
        private bool _wasPlaying, _running, _locked, _lockedRac;
        private float _t0;
        private System.Random _rng;
        private float Now => Time.time - _t0;

        private Transform _fx;                // gốc hiệu ứng tại đáy rương, cùng đơn vị cục bộ với rương
        private float _h = 0.5f, _r = 0.25f;  // chiều cao, nửa bề ngang rương (đơn vị cục bộ)
        private float _s = 1f;                // tỉ lệ cục bộ -> thế giới
        private Vector3 _centerLocal;

        private Vector3 _basePos, _lastWritten;
        private Quaternion _baseRot = Quaternion.identity;

        // Mô phỏng thân rương (đơn vị cục bộ)
        private Vector3 _p, _v, _tilt, _w, _tiltRest;
        private float _yEq;
        private float _water, _waterTarget, _waterSpeed;
        private bool _drained;
        private float _nextWave, _waveDir = 1f;
        private float _lastThud;

        // Cảnh vật
        private Transform _waterBody, _waterTop, _raft, _cannon, _cannonBarrel, _muzzle, _touch, _parrot, _label;
        private TextMesh _labelText;
        private Vector3 _cannonHome;
        private float _recoil;
        private GameObject _hitbox, _floor, _audioGo;
        private AudioSource _bed;
        private readonly List<Rigidbody> _bodies = new List<Rigidbody>();
        private readonly List<Object> _owned = new List<Object>();
        private ParticleSystem _splash, _smoke, _sparks, _bubbles, _geyser;
        private ChestFxProfile _splashStyle, _smokeStyle, _sparkStyle, _bubbleStyle, _geyserStyle;
        private Material _wood, _iron, _coinMat;

        private int _autoIndex, _playerShots, _barrels, _shotCount;
        private float _lastShot = -10f, _cannonReady = 0.9f;
        private bool _fuseLit;

        private enum ParrotMode { Away, FlyIn, Perch, Loop, Flop, Leave }
        private ParrotMode _parrotMode;
        private float _parrotT;
        private Vector3 _parrotFrom, _parrotPerch;
        private float _labelUntil;

        // ------------------------------------------------------------------ Điều khiển công khai (cho UI/nút bấm)

        /// <summary>Người chơi nạp đạn và bắn một phát (trước khi ngòi phát cuối được châm).</summary>
        public void Fire()
        {
            if (!_running || _fuseLit || _locked) return;
            if (_playerShots >= maxPlayerShots || Now - _lastShot < fireCooldown) return;
            _playerShots++;
            StartCoroutine(LoadAndFire(true));
        }

        /// <summary>Người chơi thả một thùng phao xuống biển.</summary>
        public void DropBarrel()
        {
            if (!_running || _locked || _barrels >= maxBarrels) return;
            _barrels++;
            float ang = (float)(_rng.NextDouble() * Mathf.PI * 2f);
            float dist = _r * (1.5f + (float)_rng.NextDouble() * 0.8f);
            Vector3 local = new Vector3(Mathf.Cos(ang) * dist, _water + _h * 1.3f, Mathf.Sin(ang) * dist);
            GameObject b = HaiTacSeqBuild.Part(PrimitiveType.Cylinder, null, Vector3.zero, new Vector3(0.36f, 0.24f, 0.36f) * _h * _s, Quaternion.Euler(0f, 0f, 90f * (float)_rng.NextDouble()), _wood, true);
            b.name = "HaiTacSeq_Barrel";
            b.transform.position = _fx.TransformPoint(local);
            AddBody(b, HaiTacSeqBody.Kind.Barrel, 3f);
            Say("Thùng phao!", 0.9f, Color.white);
        }

        // ------------------------------------------------------------------ Vòng đời

        private void Update()
        {
            if (_reveal == null) _reveal = GetComponent<ChestRevealSequence>();
            if (_reveal == null) return;
            bool playing = _reveal.IsPlaying;
            if (playing && !_wasPlaying && !_running) Begin();   // bắt ngay khung hình sau Play(): âm bắt đầu khi BẤM
            _wasPlaying = playing;
            if (!_running) return;

            if (keyControls)
            {
                if (FirePressed()) Fire();
                if (BarrelPressed()) DropBarrel();
            }

            // Vẹt tự bắn nếu người chơi im lặng (luồng không bao giờ phải chờ).
            if (!_fuseLit && !_locked && _autoIndex < autoShotTimes.Length && Now >= autoShotTimes[_autoIndex])
            {
                _autoIndex++;
                if (Now - _lastShot > 0.9f) StartCoroutine(LoadAndFire(false));
            }

            AnimateScene();
        }

        private void FixedUpdate()
        {
            if (!_running) return;
            float dt = Time.fixedDeltaTime;
            float g = 9.81f / _s;

            // Mực nước dâng / rút
            _water = Mathf.MoveTowards(_water, _waterTarget, _waterSpeed * dt);
            if (!_drained && _locked && _water <= 0.001f) _drained = true;

            // Sóng định kỳ xô rương (và thùng phao)
            if (!_drained && Now >= _nextWave)
            {
                _nextWave = Now + waveEvery * (0.75f + 0.5f * (float)_rng.NextDouble());
                _waveDir = -_waveDir;
                Vector3 dir = new Vector3(_waveDir, 0f, 0.4f * ((float)_rng.NextDouble() - 0.5f)).normalized;
                float strength = 0.35f + 0.25f * Mathf.Clamp01(_shotCount / 4f);
                _v += dir * strength * _h;
                _w += new Vector3(dir.z, 0f, -dir.x) * 55f;
                Vector3 push = _fx.TransformDirection(dir);
                foreach (Rigidbody rb in _bodies) if (rb != null && IsBelowWater(rb.position, 0.2f)) rb.AddForce(push * 0.6f * rb.mass * _s, ForceMode.Impulse);
            }

            // Thân rương: lực nổi kiểu lò xo về độ chìm cân bằng, cản nước, trọng lực khi lên khỏi mặt nước
            float submerged = Mathf.Clamp01((_water - _p.y) / (0.6f * _h));
            _v.y += ((_yEq - _p.y) * 30f * submerged - _v.y * 4f * submerged - g * (1f - submerged)) * dt;
            _v.x += (-_p.x * 14f - _v.x * (1.5f + 3f * submerged)) * dt;   // dây neo kéo về giữa
            _v.z += (-_p.z * 14f - _v.z * (1.5f + 3f * submerged)) * dt;
            _p += _v * dt;
            float floor = _drained ? 0f : -0.15f * _h;
            if (_p.y < floor)
            {
                _p.y = floor;
                if (_v.y < -0.8f * _h && Now - _lastThud > 0.25f && submerged < 0.1f)
                {
                    _lastThud = Now;
                    Play(thudClip, Mathf.Clamp01(-_v.y / (3f * _h)) * 0.9f, 0.9f + 0.2f * (float)_rng.NextDouble());
                    _w += new Vector3((float)_rng.NextDouble() - 0.5f, 0f, (float)_rng.NextDouble() - 0.5f) * 160f;
                }
                _v.y = -_v.y * 0.28f;
            }

            // Nghiêng / lộn / xoay: lò xo–giảm chấn về tư thế nghỉ (tư thế nghỉ là cả vòng 360 nên lộn xong vẫn đứng thẳng)
            bool airborne = _p.y > _water + 0.05f * _h && _p.y > 0.05f * _h;
            float k = airborne ? 6f : 38f, c = airborne ? 2.5f : 6.5f;
            _w += (-(_tilt - _tiltRest) * k - _w * c) * dt;
            _tilt += _w * dt;

            // Vật thể vật lý trong nước
            for (int i = _bodies.Count - 1; i >= 0; i--)
            {
                Rigidbody rb = _bodies[i];
                if (rb == null) { _bodies.RemoveAt(i); continue; }
                HaiTacSeqBody tag = rb.GetComponent<HaiTacSeqBody>();
                float depth = WaterWorldY() - rb.position.y;
                if (depth > 0f && !_drained)
                {
                    if (!tag.inWater)
                    {
                        tag.inWater = true;
                        Splash(rb.position, tag.kind == HaiTacSeqBody.Kind.Ball ? 10 : 6);
                        Play(tag.kind == HaiTacSeqBody.Kind.Barrel ? barrelClip : splashClip, 0.5f, 0.9f + 0.3f * (float)_rng.NextDouble());
                        if (tag.kind == HaiTacSeqBody.Kind.Ball && !tag.finale) WaterImpact(rb.position);
                    }
                    if (tag.kind == HaiTacSeqBody.Kind.Barrel)
                    {
                        float f = Mathf.Clamp01(depth / (0.25f * _h * _s));
                        rb.AddForce(Vector3.up * f * rb.mass * 9.81f * 2.2f);
                        rb.AddForce(-rb.linearVelocity * rb.mass * 1.8f);
                        Vector3 toChest = transform.position - rb.position;
                        toChest.y = 0f;
                        rb.AddForce(toChest.normalized * rb.mass * 0.6f * _s);   // dòng nước kéo thùng vào rương
                    }
                    else rb.AddForce(-rb.linearVelocity * rb.mass * 5f);   // đạn, xu: nước hãm lại
                }
                if (tag.kind == HaiTacSeqBody.Kind.Ball && Time.time - tag.bornTime > 3.5f)
                {
                    Destroy(rb.gameObject);
                    _bodies.RemoveAt(i);
                }
            }
        }

        private void LateUpdate()
        {
            if (!_running) return;
            Vector3 cur = transform.localPosition;
            if ((cur - _lastWritten).sqrMagnitude > 1e-12f) _basePos = cur;   // BoxOpenSequence vừa ghi (rung): lấy làm gốc mới
            Quaternion tilt = Quaternion.Euler(_tilt);
            Vector3 sc = transform.localScale;
            Vector3 c = Vector3.Scale(sc, _centerLocal);
            Vector3 pos = _basePos + _baseRot * Vector3.Scale(sc, _p) + _baseRot * c - (_baseRot * tilt) * c;   // nghiêng quanh tâm rương
            transform.localPosition = pos;
            transform.localRotation = _baseRot * tilt;
            _lastWritten = pos;

            if (_label != null && Camera.main != null)
                _label.rotation = Quaternion.LookRotation(_label.position - Camera.main.transform.position);
        }

        // ------------------------------------------------------------------ Móc lõi

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            _reveal = reveal;
            if (!_running) Begin();
            // Rương bắt đầu ngoi lên; ngòi của phát cuối được châm (chưa ai biết nổ hay xịt).
            _yEq = Mathf.Max(_yEq, 0.02f * _h);
            _fuseLit = true;
            Play(fuseClip, 0.7f);
            if (_parrotMode == ParrotMode.Perch) Say("Châm ngòi!", 1.2f, new Color(1f, 0.7f, 0.3f));
            StartCoroutine(FuseSparks());
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (!_running) return;
            _locked = true;
            StartCoroutine(Finale(tier, color));
        }

        public override void OnRevealed(ChestRevealSequence reveal)
        {
            if (_running) StartCoroutine(ParrotLeavesLater(1.8f));
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            StopAllCoroutines();
            Cleanup(true);
            _wasPlaying = false;
        }

        private void OnDestroy() => Cleanup(true);

        // ------------------------------------------------------------------ Mở màn (ngay khi bấm)

        private void Begin()
        {
            Cleanup(false);
            _running = true;
            _locked = _lockedRac = _drained = _fuseLit = false;
            _t0 = Time.time;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);
            _autoIndex = _playerShots = _barrels = _shotCount = 0;
            _lastShot = -10f;
            _p = _v = _tilt = _w = _tiltRest = Vector3.zero;
            _basePos = _lastWritten = transform.localPosition;
            _baseRot = transform.localRotation;
            Measure();

            _water = -0.2f * _h;
            _waterTarget = waterRise * _h;
            _waterSpeed = (_waterTarget - _water) / Mathf.Max(0.1f, riseSeconds);
            _yEq = -0.12f * _h;                 // rương bị nước nhấn chìm
            _nextWave = riseSeconds;
            _waveDir = _rng.NextDouble() < 0.5 ? -1f : 1f;

            BuildScene();

            // ÂM BẮT ĐẦU NGAY: sóng + trống trận dâng dần (đoạn này dài hơn đường tới lúc chốt; chốt bậc sẽ cắt nó).
            _bed.clip = buildClip;
            _bed.volume = volume;
            if (buildClip != null) _bed.Play();
            Play(splashClip, 0.6f, 0.7f);
            StartCoroutine(CannonRollsIn());
            StartParrot();
        }

        private void Measure()
        {
            Renderer[] rs = GetComponentsInChildren<Renderer>();
            Bounds b = new Bounds(transform.position, Vector3.one * 0.4f);
            bool any = false;
            foreach (Renderer r in rs)
            {
                if (r is ParticleSystemRenderer) continue;
                if (!any) { b = r.bounds; any = true; }
                else b.Encapsulate(r.bounds);
            }
            _s = Mathf.Max(1e-4f, transform.lossyScale.y);
            _h = Mathf.Max(0.05f, b.size.y / _s);
            _r = Mathf.Max(0.05f, 0.5f * Mathf.Max(b.size.x, b.size.z) / _s);
            _centerLocal = transform.InverseTransformPoint(b.center);

            _fx = new GameObject("HaiTacSeq_Fx").transform;
            _fx.SetParent(transform.parent, false);
            _fx.position = new Vector3(b.center.x, b.min.y, b.center.z);
            _fx.rotation = transform.rotation;
            _fx.localScale = transform.localScale;
            _owned.Add(_fx.gameObject);
        }

        private void BuildScene()
        {
            _wood = Mat(ChestFxFactory.CreateUnlit(new Color(0.42f, 0.27f, 0.14f), false, "HaiTacSeq_Wood"));
            _iron = Mat(ChestFxFactory.CreateUnlit(new Color(0.13f, 0.13f, 0.15f), false, "HaiTacSeq_Iron"));
            _coinMat = Mat(ChestFxFactory.CreateUnlit(new Color(1f, 0.8f, 0.25f), false, "HaiTacSeq_Coin"));
            Material sea = Mat(ChestFxFactory.CreateAdditive(null, "HaiTacSeq_Sea"));
            ChestFxFactory.SetColor(sea, new Color(0.03f, 0.16f, 0.22f, 1f));
            Material foam = Mat(ChestFxFactory.CreateAdditive(null, "HaiTacSeq_Foam"));
            ChestFxFactory.SetColor(foam, new Color(0.12f, 0.35f, 0.38f, 1f));

            _waterBody = HaiTacSeqBuild.Part(PrimitiveType.Cylinder, _fx, Vector3.zero, Vector3.one, Quaternion.identity, sea).transform;
            _waterTop = HaiTacSeqBuild.Part(PrimitiveType.Cylinder, _fx, Vector3.zero, Vector3.one, Quaternion.identity, foam).transform;

            // Bè + đại bác, ở trước-trái rương, quay nòng vào rương. Bắt đầu ngoài xa rồi trôi vào.
            _raft = new GameObject("HaiTacSeq_Raft").transform;
            _raft.SetParent(_fx, false);
            _cannonHome = new Vector3(-2.6f * _r, 0f, -1.1f * _r);
            _raft.localRotation = Quaternion.LookRotation(new Vector3(-_cannonHome.x, 0f, -_cannonHome.z));
            HaiTacSeqBuild.Part(PrimitiveType.Cube, _raft, new Vector3(0f, 0.02f * _h, 0f), new Vector3(1.0f * _h, 0.05f * _h, 1.1f * _h), Quaternion.identity, _wood);
            _cannon = HaiTacSeqBuild.Cannon(_raft, 0.95f * _h, _wood, _iron);
            _cannon.localPosition = new Vector3(0f, 0.04f * _h, 0f);
            _cannonBarrel = _cannon.Find("Barrel");
            _muzzle = _cannonBarrel.Find("Muzzle");
            _touch = _cannonBarrel.Find("Touch");

            // Khối va chạm ẩn của rương (Rigidbody động học) để đạn và thùng va thật.
            _hitbox = new GameObject("HaiTacSeq_Hitbox");
            _hitbox.transform.SetParent(transform, false);
            BoxCollider box = _hitbox.AddComponent<BoxCollider>();
            box.center = _centerLocal;
            box.size = new Vector3(_r * 1.8f, _h * 0.92f, _r * 1.8f);
            Rigidbody hrb = _hitbox.AddComponent<Rigidbody>();
            hrb.isKinematic = true;
            hrb.useGravity = false;
            _hitbox.AddComponent<HaiTacSeqContact>().owner = this;
            _owned.Add(_hitbox);

            // Sàn ẩn ở đáy rương: đạn, thùng, xu rơi xuống đây khi biển rút.
            _floor = HaiTacSeqBuild.Part(PrimitiveType.Cube, _fx, new Vector3(0f, -0.03f * _h, 0f), new Vector3(9f * _r, 0.06f * _h, 9f * _r), Quaternion.identity, null, true);
            _floor.name = "HaiTacSeq_Floor";
            _floor.GetComponent<Renderer>().enabled = false;

            // Hạt (chỉ là phần phụ; nhân vật chính là vật lý)
            float size = _h * _s;
            _splashStyle = Style(ParticleLook.SoftDot, ParticleDirection.Fountain, 1.1f, 0.03f, 0.9f, HaiTacFxPreset.SeaFoam, 0.7f, 1f);
            _splash = Particles("HaiTacSeq_Splash", _splashStyle, size, new Color(0.7f, 0.95f, 1f));
            _smokeStyle = Style(ParticleLook.SoftDot, ParticleDirection.Radial, 0.35f, 0.09f, 1.4f, new Color(0.5f, 0.5f, 0.5f), 0.8f, -0.05f);
            _smoke = Particles("HaiTacSeq_Smoke", _smokeStyle, size, new Color(0.55f, 0.55f, 0.55f));
            _sparkStyle = Style(ParticleLook.Sparkle, ParticleDirection.Up, 0.5f, 0.02f, 0.35f, new Color(1f, 0.9f, 0.4f), 0.5f, 0.3f);
            _sparks = Particles("HaiTacSeq_Sparks", _sparkStyle, size, new Color(1f, 0.55f, 0.15f));
            _bubbleStyle = Style(ParticleLook.SoftDot, ParticleDirection.Up, 0.25f, 0.018f, 1.1f, Color.white, 0.5f, -0.1f);
            _bubbles = Particles("HaiTacSeq_Bubbles", _bubbleStyle, size, new Color(0.75f, 0.95f, 1f));
            _geyserStyle = Style(ParticleLook.Streak, ParticleDirection.Up, 2.6f, 0.04f, 1.2f, HaiTacFxPreset.SeaFoam, 0.6f, 0.8f);
            _geyser = Particles("HaiTacSeq_Geyser", _geyserStyle, size, new Color(0.8f, 1f, 1f));

            _audioGo = new GameObject("HaiTacSeq_Audio");
            _audioGo.transform.SetParent(transform, false);
            _bed = _audioGo.AddComponent<AudioSource>();
            _bed.playOnAwake = false;
            AudioSource main = GetComponent<AudioSource>();
            if (main != null) { _bed.spatialBlend = main.spatialBlend; _bed.outputAudioMixerGroup = main.outputAudioMixerGroup; }
            _owned.Add(_audioGo);
        }

        // ------------------------------------------------------------------ Đại bác

        private IEnumerator CannonRollsIn()
        {
            Play(loadClip, 0.5f, 0.8f);
            float t = 0f;
            while (t < _cannonReady)
            {
                t += Time.deltaTime;
                float u = Mathf.SmoothStep(0f, 1f, t / _cannonReady);
                Vector3 from = _cannonHome * 1.8f;
                Vector3 p = Vector3.Lerp(from, _cannonHome, u);
                _raft.localPosition = new Vector3(p.x, _raft.localPosition.y, p.z);
                yield return null;
            }
        }

        private IEnumerator LoadAndFire(bool byPlayer)
        {
            _lastShot = Now;
            while (Now < _cannonReady) yield return null;
            Play(loadClip, 0.7f, byPlayer ? 1.1f : 1f);     // thông nòng, nhồi thuốc
            if (!byPlayer && _parrotMode == ParrotMode.Perch) Squawk(1.1f);
            yield return new WaitForSeconds(0.28f);
            if (_locked) yield break;
            _shotCount++;
            // Phát đầu luôn trượt (té nước dọa), sau đó 65% trúng rương; đều theo hạt giống trang trí.
            bool hit = _shotCount > 1 && _rng.NextDouble() < 0.65;
            Vector3 target;
            if (hit) target = transform.TransformPoint(_centerLocal + new Vector3(((float)_rng.NextDouble() - 0.5f) * _r, 0.15f * _h, 0f));
            else
            {
                float ang = (float)(_rng.NextDouble() * Mathf.PI * 2f);
                float d = _r * (1.3f + 0.6f * (float)_rng.NextDouble());
                target = _fx.TransformPoint(new Vector3(Mathf.Cos(ang) * d, _water, Mathf.Sin(ang) * d));
            }
            Shoot(target, 0.55f, false, 1f);
            if (byPlayer) Say(_shotCount + "!", 0.6f, Color.white);
        }

        /// <summary>Bắn một viên đạn Rigidbody theo đường đạn đạo tới <paramref name="target"/> trong <paramref name="flight"/> giây.</summary>
        private Rigidbody Shoot(Vector3 target, float flight, bool finale, float loud)
        {
            Vector3 m = _muzzle.position;
            Vector3 g = new Vector3(0f, -9.81f, 0f);
            Vector3 v0 = (target - m) / flight - 0.5f * g * flight;
            GameObject ball = HaiTacSeqBuild.Part(PrimitiveType.Sphere, null, Vector3.zero, Vector3.one * 0.13f * _h * _s, Quaternion.identity, _iron, true);
            ball.name = "HaiTacSeq_Ball";
            ball.transform.position = m;
            Rigidbody rb = AddBody(ball, HaiTacSeqBody.Kind.Ball, 4f);
            rb.linearVelocity = v0;
            ball.GetComponent<HaiTacSeqBody>().finale = finale;
            Boom(loud);
            return rb;
        }

        private void Boom(float loud)
        {
            Play(cannonClip, volume * loud, 0.92f + 0.16f * (float)_rng.NextDouble());
            _recoil = 1f;
            Emit(_smoke, _muzzle.position, 14);
            Emit(_sparks, _muzzle.position, 10);
            _w += new Vector3(0f, 0f, 6f);   // tiếng nổ làm rương giật nhẹ
        }

        private IEnumerator FuseSparks()
        {
            while (_fuseLit && !_locked)
            {
                Emit(_sparks, _touch.position, 2);
                yield return new WaitForSeconds(0.07f);
            }
        }

        /// <summary>Gọi từ HaiTacSeqContact khi một Rigidbody đập vào rương (va chạm PhysX thật).</summary>
        public void OnChestStruck(Rigidbody rb, Vector3 relativeVelocity)
        {
            if (!_running) return;
            HaiTacSeqBody tag = rb.GetComponent<HaiTacSeqBody>();
            if (tag == null || tag.finale || tag.kind == HaiTacSeqBody.Kind.Coin) return;
            Vector3 dirLocal = Quaternion.Inverse(_fx.rotation) * relativeVelocity / _s;   // hướng/độ lớn trong hệ của rương
            dirLocal = -dirLocal;   // relativeVelocity là của rương so với vật: đảo lại cho hướng vật lao tới
            float massRatio = tag.kind == HaiTacSeqBody.Kind.Ball ? 0.22f : 0.08f;
            _v += dirLocal * massRatio;
            _v.y += (tag.kind == HaiTacSeqBody.Kind.Ball ? 0.9f : 0.3f) * _h;
            Vector3 flat = new Vector3(dirLocal.x, 0f, dirLocal.z);
            _w += new Vector3(flat.z, 0f, -flat.x).normalized * (tag.kind == HaiTacSeqBody.Kind.Ball ? 140f : 50f);
            if (tag.kind == HaiTacSeqBody.Kind.Ball)
            {
                _yEq = Mathf.Min(_yEq + 0.05f * _h, 0.05f * _h);   // mỗi phát trúng: rương bớt nước, nổi cao hơn
                Play(thudClip, 0.9f, 0.85f + 0.2f * (float)_rng.NextDouble());
                Emit(_bubbles, transform.TransformPoint(_centerLocal), 20);
                if (_parrotMode == ParrotMode.Perch && _rng.NextDouble() < 0.6) Squawk(1.25f);
            }
            else if (Now - _lastThud > 0.3f)
            {
                _lastThud = Now;
                Play(barrelClip, 0.45f, 1.2f);
            }
        }

        private void WaterImpact(Vector3 world)
        {
            Vector3 local = _fx.InverseTransformPoint(world);
            Vector3 away = new Vector3(-local.x, 0f, -local.z);
            float d = away.magnitude;
            if (d < 1e-4f) return;
            float f = 1f / (1f + d / _r);
            _v += away.normalized * -0.9f * f * _h;   // sóng từ chỗ đạn rơi đẩy rương ra xa
            _v.y += 0.4f * f * _h;
            _w += new Vector3(-away.z, 0f, away.x).normalized * 70f * f;
        }

        // ------------------------------------------------------------------ Kết cục (chỉ ở đây mới đọc bậc)

        private IEnumerator Finale(ChestTier tier, Color color)
        {
            int rank = Mathf.Clamp(tier.Rank(), 0, 4);
            _fuseLit = false;

            if (tier == ChestTier.Rac)
            {
                // XỊT: tắt phụt nhạc, ngòi tịt, viên đạn lăn khỏi nòng rơi "tõm".
                _lockedRac = true;
                _bed.Stop();
                Play(fizzleClip, 0.9f);
                Play(Ending(0), volume);
                Emit(_smoke, _touch.position, 4);
                yield return new WaitForSeconds(0.35f);
                GameObject ball = HaiTacSeqBuild.Part(PrimitiveType.Sphere, null, Vector3.zero, Vector3.one * 0.13f * _h * _s, Quaternion.identity, _iron, true);
                ball.transform.position = _muzzle.position;
                Rigidbody rb = AddBody(ball, HaiTacSeqBody.Kind.Ball, 4f);
                rb.linearVelocity = _muzzle.forward * 0.4f * _s;
                ball.GetComponent<HaiTacSeqBody>().finale = true;
                Drain(1.9f);
                Play(drainClip, 0.8f, 0.9f);
                _yEq = 0f;
                _tiltRest = new Vector3(9f, 0f, 15f);   // nổi lên lệch hẳn một bên: hài
                _v.y += Jump(jumpByTier[0]);
                ParrotCall(tier, color);
                yield break;
            }

            // Nổ: số phát theo bậc, phát đầu trúng đúng lúc chốt.
            StartCoroutine(FadeBed(0.18f));
            Play(Ending(rank), volume);
            int booms = Mathf.Max(1, boomsByTier[rank]);
            Vector3 target = transform.TransformPoint(_centerLocal);
            Shoot(target, 0.12f, true, 1f);
            yield return new WaitForSeconds(0.08f);

            // Rương bị hất lên (độ cao, xoay, lộn theo bậc)
            Drain(rank >= 3 ? 0.45f : 0.9f);
            Play(drainClip, 0.55f, rank >= 3 ? 1.3f : 1.05f);
            _yEq = 0f;
            _v.y = Jump(jumpByTier[rank]);
            _tiltRest = new Vector3(0f, spinByTier[rank], flipByTier[rank]);
            Emit(_splash, transform.TransformPoint(_centerLocal), 18 + 10 * rank);
            if (rank == 4)
            {
                _geyser.transform.position = _fx.position;
                _geyser.transform.rotation = _fx.rotation;
                ChestFxFactory.SetParticleColor(_geyser, color, _geyserStyle, 1.2f);
                _geyser.Emit(160);
            }
            ParrotCall(tier, color);
            if (rank >= 2) BlastBarrels(rank);

            for (int i = 1; i < booms; i++)
            {
                yield return new WaitForSeconds(0.22f);
                Boom(1f - 0.12f * i);
                _w += new Vector3(0f, 90f, 0f);
                Emit(_splash, _fx.position, 25);
            }

            int coins = coinsByTier[rank] + (rank >= 3 ? 3 * _barrels : 0);
            if (coins > 0) StartCoroutine(CoinRain(coins));
        }

        private float Jump(float heightInChests) => Mathf.Sqrt(2f * (9.81f / _s) * Mathf.Max(0f, heightInChests) * _h);

        private void Drain(float seconds)
        {
            _waterTarget = -0.25f * _h;
            _waterSpeed = (_water - _waterTarget) / Mathf.Max(0.05f, seconds);
        }

        private IEnumerator FadeBed(float seconds)
        {
            float v0 = _bed.volume, t = 0f;
            while (t < seconds) { t += Time.deltaTime; _bed.volume = Mathf.Lerp(v0, 0f, t / seconds); yield return null; }
            _bed.Stop();
        }

        private void BlastBarrels(int rank)
        {
            foreach (Rigidbody rb in _bodies)
            {
                if (rb == null || rb.GetComponent<HaiTacSeqBody>().kind != HaiTacSeqBody.Kind.Barrel) continue;
                Vector3 away = rb.position - transform.position;
                away.y = 0f;
                Vector3 imp = (away.normalized * (0.8f + 0.4f * rank) + Vector3.up * (1.2f + 0.6f * rank)) * rb.mass * _s;
                rb.AddForce(imp, ForceMode.Impulse);
            }
        }

        private IEnumerator CoinRain(int count)
        {
            yield return new WaitForSeconds(0.3f);
            for (int i = 0; i < count; i++)
            {
                GameObject coin = HaiTacSeqBuild.Part(PrimitiveType.Cylinder, null, Vector3.zero, new Vector3(0.09f, 0.012f, 0.09f) * _h * _s,
                    Quaternion.Euler(90f * (float)_rng.NextDouble(), 0f, 90f * (float)_rng.NextDouble()), _coinMat, true);
                coin.name = "HaiTacSeq_Coin";
                coin.transform.position = _reveal != null ? _reveal.MouthPosition : transform.position;
                Rigidbody rb = AddBody(coin, HaiTacSeqBody.Kind.Coin, 0.05f);
                float ang = (float)(_rng.NextDouble() * Mathf.PI * 2f);
                float out1 = 0.5f + 0.9f * (float)_rng.NextDouble();
                rb.linearVelocity = new Vector3(Mathf.Cos(ang) * out1, 1.6f + 1.2f * (float)_rng.NextDouble(), Mathf.Sin(ang) * out1) * _h * _s * 2f;
                rb.angularVelocity = new Vector3((float)_rng.NextDouble(), (float)_rng.NextDouble(), (float)_rng.NextDouble()) * 20f;
                if (i % 3 == 0) Play(coinClip, 0.35f, 0.9f + 0.4f * (float)_rng.NextDouble());
                yield return new WaitForSeconds(0.035f);
            }
        }

        // ------------------------------------------------------------------ Vẹt

        private void StartParrot()
        {
            Material red = Mat(ChestFxFactory.CreateUnlit(new Color(0.85f, 0.12f, 0.1f), false, "HaiTacSeq_Red"));
            Material green = Mat(ChestFxFactory.CreateUnlit(new Color(0.1f, 0.6f, 0.25f), false, "HaiTacSeq_Green"));
            Material yellow = Mat(ChestFxFactory.CreateUnlit(new Color(1f, 0.8f, 0.15f), false, "HaiTacSeq_Yellow"));
            Material dark = Mat(ChestFxFactory.CreateUnlit(new Color(0.05f, 0.05f, 0.05f), false, "HaiTacSeq_Dark"));
            _parrot = HaiTacSeqBuild.Parrot(_fx, 1.1f * _h, red, green, yellow, dark);
            _parrotFrom = new Vector3(-4f * _r, 2.6f * _h, 2.5f * _r);
            _parrot.localPosition = _parrotFrom;
            _parrotMode = ParrotMode.FlyIn;
            _parrotT = 0f;

            _label = new GameObject("HaiTacSeq_Label").transform;
            _label.SetParent(_parrot, false);
            _label.localPosition = new Vector3(0f, 0.45f * _h, 0f);
            _labelText = _label.gameObject.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null)
            {
                _labelText.font = font;
                _label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            }
            _labelText.anchor = TextAnchor.MiddleCenter;
            _labelText.characterSize = 0.02f * _h;
            _labelText.fontSize = 64;
            _labelText.text = "";
        }

        private void ParrotCall(ChestTier tier, Color color)
        {
            int rank = tier.Rank();
            string name = tier.DisplayName();
            string text = rank == 0 ? name.ToLowerInvariant() + "... " + name.ToLowerInvariant() + "..."
                : rank >= 3 ? name.ToUpperInvariant() + new string('!', rank) : name + "!";
            Say(text, 3.5f, color);
            StartCoroutine(SquawkTimes(rank == 0 ? 1 : rank, rank == 0 ? 0.7f : 1.05f + 0.08f * rank));
            if (rank == 0) { _parrotMode = ParrotMode.Flop; _parrotT = 0f; }
            if (rank == 4) { _parrotMode = ParrotMode.Loop; _parrotT = 0f; }
        }

        private IEnumerator SquawkTimes(int n, float pitch)
        {
            for (int i = 0; i < n; i++)
            {
                Squawk(pitch * (1f + 0.04f * i));
                yield return new WaitForSeconds(0.2f);
            }
        }

        private void Squawk(float pitch) => Play(parrotClip, 0.7f, pitch * (0.95f + 0.1f * (float)_rng.NextDouble()));

        private void Say(string text, float seconds, Color color)
        {
            if (_labelText == null) return;
            _labelText.text = text;
            _labelText.color = color;
            _labelUntil = Time.time + seconds;
        }

        private IEnumerator ParrotLeavesLater(float delay)
        {
            yield return new WaitForSeconds(delay);
            _parrotMode = ParrotMode.Leave;
            _parrotT = 0f;
            _parrotFrom = _parrot != null ? _parrot.localPosition : Vector3.zero;
        }

        // ------------------------------------------------------------------ Hoạt cảnh mỗi khung

        private void AnimateScene()
        {
            float t = Now;
            // Nước: khối nước + mặt sóng
            float bottom = -0.2f * _h;
            float depth = Mathf.Max(0.001f, _water - bottom);
            float diam = 7f * _r;
            _waterBody.localScale = new Vector3(diam, depth * 0.5f, diam);
            _waterBody.localPosition = new Vector3(0f, bottom + depth * 0.5f, 0f);
            float swell = Mathf.Sin(t * 2.3f) * 0.015f * _h;
            _waterTop.localScale = new Vector3(diam * 1.01f, 0.004f * _h, diam * 1.01f);
            _waterTop.localPosition = new Vector3(0f, _water + swell, 0f);
            bool wet = _water > bottom + 0.01f * _h;
            _waterBody.gameObject.SetActive(wet);
            _waterTop.gameObject.SetActive(wet);

            // Bè nổi theo mặt nước (rút nước thì đáp xuống sàn)
            float raftY = Mathf.Max(0f, _water + swell);
            _raft.localPosition = new Vector3(_raft.localPosition.x, raftY, _raft.localPosition.z);
            _raft.localRotation = Quaternion.LookRotation(new Vector3(-_cannonHome.x, 0f, -_cannonHome.z)) * Quaternion.Euler(Mathf.Sin(t * 1.7f) * 4f * (wet ? 1f : 0f), 0f, Mathf.Sin(t * 2.1f) * 3f * (wet ? 1f : 0f));

            // Giật lùi nòng
            _recoil = Mathf.MoveTowards(_recoil, 0f, Time.deltaTime * 3f);
            _cannonBarrel.localPosition = new Vector3(0f, 0.25f * 0.95f * _h, (-0.05f - 0.22f * _recoil) * 0.95f * _h);

            // Bọt khí khi rương còn chìm
            if (!_drained && _water > _p.y + 0.3f * _h && Random.value < 0.25f) Emit(_bubbles, transform.TransformPoint(_centerLocal), 1);

            AnimateParrot();
            if (_labelText != null && Time.time > _labelUntil) _labelText.text = "";
        }

        private void AnimateParrot()
        {
            if (_parrot == null) return;
            _parrotT += Time.deltaTime;
            _parrotPerch = _raft.localPosition + _raft.localRotation * new Vector3(0f, 0.6f * _h, -0.2f * _h);
            float flap = 0f;
            switch (_parrotMode)
            {
                case ParrotMode.FlyIn:
                {
                    float u = Mathf.Clamp01(_parrotT / 1.2f);
                    Vector3 mid = (_parrotFrom + _parrotPerch) * 0.5f + Vector3.up * 0.8f * _h;
                    _parrot.localPosition = Bezier(_parrotFrom, mid, _parrotPerch, Mathf.SmoothStep(0f, 1f, u));
                    _parrot.localRotation = Quaternion.LookRotation(Flat(_parrotPerch - _parrotFrom));
                    flap = 55f * Mathf.Sin(_parrotT * 28f);
                    if (u >= 1f)
                    {
                        _parrotMode = ParrotMode.Perch;
                        Squawk(1f);
                        Say(keyControls ? "Nạp đạn! [G] bắn  [B] thả thùng" : "Nạp đạn!", 2.6f, new Color(1f, 0.9f, 0.5f));
                    }
                    break;
                }
                case ParrotMode.Perch:
                    _parrot.localPosition = _parrotPerch;
                    _parrot.localRotation = Quaternion.LookRotation(Flat(-_parrotPerch)) * Quaternion.Euler(Mathf.Sin(Time.time * 5f) * 6f, 0f, 0f);
                    flap = 10f + 8f * Mathf.Sin(Time.time * 9f);
                    break;
                case ParrotMode.Flop:   // Rác: ngã ngửa khỏi chỗ đậu rồi lồm cồm đứng dậy
                {
                    float u = Mathf.Clamp01(_parrotT / 0.4f);
                    float back = _parrotT < 1.6f ? u : Mathf.Clamp01(1f - (_parrotT - 1.6f) / 0.4f);
                    _parrot.localPosition = _parrotPerch;
                    _parrot.localRotation = Quaternion.LookRotation(Flat(-_parrotPerch)) * Quaternion.Euler(-80f * back, 0f, 0f);
                    if (_parrotT > 2.1f) _parrotMode = ParrotMode.Perch;
                    break;
                }
                case ParrotMode.Loop:   // Huyền thoại: bay vòng quanh rương hai vòng
                {
                    float a = _parrotT * Mathf.PI * 2f / 1.1f;
                    float rad = 2.2f * _r;
                    Vector3 p = new Vector3(Mathf.Cos(a) * rad, 1.6f * _h + 0.3f * _h * Mathf.Sin(a * 2f), Mathf.Sin(a) * rad);
                    _parrot.localPosition = _parrotT < 0.3f ? Vector3.Lerp(_parrotPerch, p, _parrotT / 0.3f) : p;
                    _parrot.localRotation = Quaternion.LookRotation(new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a))) * Quaternion.Euler(0f, 0f, -25f);
                    flap = 60f * Mathf.Sin(_parrotT * 30f);
                    if (_parrotT > 2.2f) _parrotMode = ParrotMode.Perch;
                    break;
                }
                case ParrotMode.Leave:
                {
                    Vector3 to = new Vector3(5f * _r, 3.5f * _h, 3f * _r);
                    float u = Mathf.Clamp01(_parrotT / 1.4f);
                    _parrot.localPosition = Vector3.Lerp(_parrotFrom, to, u * u);
                    _parrot.localRotation = Quaternion.LookRotation(Flat(to - _parrotFrom));
                    flap = 55f * Mathf.Sin(_parrotT * 28f);
                    if (u >= 1f) { _parrotMode = ParrotMode.Away; _parrot.gameObject.SetActive(false); }
                    break;
                }
            }
            Transform wl = _parrot.Find("WingL"), wr = _parrot.Find("WingR");
            if (wl != null) wl.localRotation = Quaternion.Euler(0f, 0f, flap);
            if (wr != null) wr.localRotation = Quaternion.Euler(0f, 0f, -flap);
        }

        // ------------------------------------------------------------------ Tiện ích

        private float WaterWorldY() => _fx != null ? _fx.TransformPoint(new Vector3(0f, _water, 0f)).y : float.MinValue;

        private bool IsBelowWater(Vector3 world, float marginChests) => world.y < WaterWorldY() + marginChests * _h * _s;

        private Rigidbody AddBody(GameObject go, HaiTacSeqBody.Kind kind, float mass)
        {
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.useGravity = true;
            HaiTacSeqBody tag = go.AddComponent<HaiTacSeqBody>();
            tag.kind = kind;
            tag.bornTime = Time.time;
            _bodies.Add(rb);
            return rb;
        }

        private void Splash(Vector3 world, int count) => Emit(_splash, world, count);

        private static void Emit(ParticleSystem ps, Vector3 world, int count)
        {
            if (ps == null) return;
            ps.transform.position = world;
            ps.Emit(count);
        }

        private AudioClip Ending(int rank) => endingClips != null && rank < endingClips.Length ? endingClips[rank] : null;

        private void Play(AudioClip clip, float vol, float pitch = 1f)
        {
            if (clip == null || _reveal == null) return;
            if (_lockedRac && clip == cannonClip) return;
            _reveal.PlayOneShot(clip, vol * volume, pitch);
        }

        private Material Mat(Material m) { _owned.Add(m); return m; }

        private ParticleSystem Particles(string name, ChestFxProfile style, float size, Color color)
        {
            ParticleSystem ps = ChestFxFactory.CreateParticles(name, null, style, size, true);
            ChestFxFactory.SetParticleColor(ps, color, style, 1f);
            ps.Play();
            _owned.Add(ps.gameObject);
            _owned.Add(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            _owned.Add(style);
            return ps;
        }

        private static ChestFxProfile Style(ParticleLook look, ParticleDirection dir, float speed, float size, float life, Color secondary, float mix, float gravity)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.secondaryColor = secondary;
            s.secondaryMix = mix;
            s.burstGravity = gravity;
            return s;
        }

        private static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t) => Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude < 1e-8f ? Vector3.forward : v;
        }

        private void Cleanup(bool restorePose)
        {
            if (_running && restorePose)
            {
                transform.localPosition = _basePos;
                transform.localRotation = _baseRot;
            }
            _running = false;
            foreach (Rigidbody rb in _bodies) if (rb != null) Destroy(rb.gameObject);
            _bodies.Clear();
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
            _fx = null;
            _parrot = _label = null;
            _labelText = null;
            _parrotMode = ParrotMode.Away;
        }

#if ENABLE_INPUT_SYSTEM
        private static bool FirePressed() => Keyboard.current != null && Keyboard.current[Key.G].wasPressedThisFrame;
        private static bool BarrelPressed() => Keyboard.current != null && Keyboard.current[Key.B].wasPressedThisFrame;
#else
        private static bool FirePressed() => Input.GetKeyDown(KeyCode.G);
        private static bool BarrelPressed() => Input.GetKeyDown(KeyCode.B);
#endif
    }
}

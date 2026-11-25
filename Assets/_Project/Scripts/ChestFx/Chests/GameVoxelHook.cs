using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Kịch bản riêng của Chest_Game: "THẾ GIỚI VOXEL 8-BIT".
    ///
    /// Nhịp (xem docs/superpowers/specs/2026-11-25-kichban-game-design.md):
    ///  0,0 s  Bấm mở -> "bíp" chèn xu. Rương nhảy lên lơ lửng thành khối dấu hỏi (dấu "?" ghép bằng voxel quay trên đầu).
    ///  ~0,3 s Nhạc vòng chiptune bắt đầu (nhỏ, dâng dần); nhân vật pixel chạy vào, nhảy, đội đầu húc đáy rương (tối đa 3):
    ///         rương nảy, xu vật lý bật ra. Mỗi cú húc nhạc nhanh/cao thêm một nấc.
    ///  (Đã thu gọn theo kế hoạch 2026-11-25: không phủ màn hình CRT, không đếm 3-2-1, chữ arcade là 1 dòng TextMesh trên rương.)
    ///  Roulette bắt đầu: rương VỠ thành tối đa 80 voxel có Rigidbody văng ra, nảy trên sàn; rồi bị hút về lắp lại
    ///         dần (tiến độ = thời gian roulette), voxel đổi màu theo màu đang quay, nhạc dâng cao dần.
    ///  Chốt bậc: Rác -> nhạc dừng ngay, lặng 0,2 s, 1 tiếng xịt + GAME OVER hài, nhân vật ngã sấp (vật lý), rương xẹp, voxel xám
    ///         "xịt". Bậc cao -> fanfare to dần theo bậc; Rất hiếm rương bật nhảy xoay vòng + mưa xu; Huyền thoại
    ///         trống cuộn, rương phóng lên xoay, pháo voxel cầu vồng, mưa xu dày.
    /// Tương tác phụ: [Z] thả XU (+1 mạng; Rác còn mạng thì "CONTINUE?" diễn lại - vẫn thua, vì bậc không đổi),
    /// [X] thả NẤM (+1 "power": rương phồng to, pháo/mưa xu cuối nhiều hơn). Cả hai rơi bằng vật lý vào rương.
    ///
    /// Bất biến: hook chỉ trang trí. Bậc do game quyết (ChestRevealSequence.Play); ngẫu nhiên ở đây chỉ để trang trí.
    /// Đặt lên gốc rương (Tools/Cho Do La/Game Voxel/Add Hook To Selected).
    /// Hook không sửa file dùng chung: tự phát hiện lúc bấm mở bằng reveal.IsPlaying trong Update.
    /// </summary>
    public class GameVoxelHook : ChestFxHook
    {
        [Header("Âm (Audio/Chests/game/*.mp3 ElevenLabs, gán bởi GameVoxelFxSetup)")]
        public AudioClip insertCoinClip;
        public AudioClip loopBarClip;
        public AudioClip bumpClip;
        public AudioClip jumpClip;
        public AudioClip coinClip;
        public AudioClip voxelTickClip;
        public AudioClip shatterClip;
        public AudioClip reassembleClip;
        public AudioClip powerUpClip;
        public AudioClip oneUpClip;
        [Tooltip("Tiếng chốt (TC) theo bậc: 0 Rác (GAME OVER) .. 4 Huyền thoại. Hook tự phát (A4/A11).")]
        public AudioClip[] stingClips = new AudioClip[ChestTierUtil.Count];
        [Range(0f, 1f)] public float musicVolume = 0.45f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;

        [Header("Voxel")]
        [Tooltip("Số voxel tối đa khi rương vỡ (kích thước voxel tự tính theo diện tích vỏ).")]
        [SerializeField] private int maxVoxels = 80;
        [SerializeField] private float minVoxelSize = 0.025f;
        [Tooltip("Tốc độ văng khi vỡ (m/s).")]
        [SerializeField] private float shatterSpeed = 1.6f;
        [Tooltip("Thời gian voxel bay tự do trước khi bị hút về.")]
        [SerializeField] private float freeFlightSeconds = 0.7f;
        [Tooltip("Thời lượng roulette ước tính để chia tiến độ lắp lại (roulette thật dài hơn thì chờ, ngắn hơn thì lắp nhanh lúc chốt).")]
        [SerializeField] private float expectedRouletteSeconds = 3.2f;

        [Header("Tương tác")]
        [SerializeField] private bool allowDrops = true;
        [SerializeField] private int maxPower = 3;
        [SerializeField] private bool showCrt = false;   // đã bỏ phủ màn hình (T9); giữ ô để không vỡ prefab cũ, không còn tác dụng

        // ------------------------------------------------------------------ trạng thái
        private ChestRevealSequence _reveal;
        private Transform _root;
        private bool _running;
        private bool _sawPlaying;
        private System.Random _rng;
        private Transform _fx;
        private readonly List<Object> _owned = new List<Object>();
        private readonly List<Renderer> _chestRenderers = new List<Renderer>();
        private Bounds _chestBounds;
        private float _chestH = 0.4f;
        private float _chestW = 0.46f;
        private float _floorY;

        // dịch chuyển cả rương (cộng thêm trên tư thế của BoxOpenSequence, gỡ ra mỗi Update)
        private Vector3 _offPos, _appliedPos;
        private Quaternion _offRot = Quaternion.identity, _appliedRot = Quaternion.identity;
        private Vector3 _offScale = Vector3.one, _appliedScale = Vector3.one;
        private float _hover, _hoverTarget, _bumpV, _bumpY, _spinY, _squash = 1f;
        private float _powerScale = 1f;

        // âm
        private AudioSource _music, _sfx;
        private float _musicPitchTarget = 1f;

        // nhân vật, dấu hỏi, voxel, vật rơi
        private Transform _hero;
        private Rigidbody _heroBody;
        private Transform _question;
        private readonly List<Voxel> _voxels = new List<Voxel>();
        private readonly List<Material> _voxelMats = new List<Material>();
        private readonly List<Color> _voxelMatBase = new List<Color>();
        private readonly List<Drop> _drops = new List<Drop>();
        private bool _shattered, _rebuilt;
        private float _rouletteT0 = -1f, _pullT0 = -1f;
        private Color _shownColor = Color.white;
        private int _steps;

        // chữ arcade: 1 dòng TextMesh nổi trên rương
        private Transform _label;
        private TextMesh _labelText;
        private float _labelUntil;
        private int _lives, _power;
        private ChestTier _tier;
        private bool _locked;
        // cổng âm (A3/A5)
        private float _lastEvent = -999f, _lastImpact = -999f, _lastTail = -999f, _lockTime = -999f;
        private int _tailCount;

        private struct Voxel
        {
            public Transform t;
            public Rigidbody rb;
            public Vector3 homeLocal;      // theo gốc rương
            public Quaternion homeRotLocal;
            public Vector3 pullFrom;
            public Quaternion pullFromRot;
            public float delay;            // lệch pha khi hút về (trang trí)
            public int mat;
        }

        private class Drop
        {
            public Transform t;
            public Rigidbody rb;
            public bool mushroom;
            public float born;
        }

        // ================================================================== vòng đời

        private void Update()
        {
            if (_reveal == null) _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null) return;
            _root = _reveal.transform;

            // Gỡ phần dịch chuyển đã cộng ở LateUpdate trước (BoxOpenSequence có thể ghi đè ngay sau, đều đúng).
            RemoveOffset();

            bool playing = _reveal.IsPlaying;
            if (playing && !_sawPlaying && !_running) Begin();
            _sawPlaying = playing || _running;

            if (!_running) return;
            HandleInput();
            UpdateMotion(Time.deltaTime);
            UpdateVoxels();
            UpdateDrops();
            if (_music != null) _music.pitch = Mathf.MoveTowards(_music.pitch, _musicPitchTarget, Time.deltaTime * 0.8f);
            UpdateLabel();
        }

        private void LateUpdate()
        {
            if (_root == null || !_running) return;
            _appliedPos = _offPos;
            _appliedRot = _offRot;
            _appliedScale = _offScale;
            _root.localPosition += _appliedPos;
            _root.localRotation = _root.localRotation * _appliedRot;
            _root.localScale = Vector3.Scale(_root.localScale, _appliedScale);
        }

        private void RemoveOffset()
        {
            if (_root == null) return;
            _root.localPosition -= _appliedPos;
            _root.localRotation = _root.localRotation * Quaternion.Inverse(_appliedRot);
            _root.localScale = new Vector3(_root.localScale.x / _appliedScale.x, _root.localScale.y / _appliedScale.y, _root.localScale.z / _appliedScale.z);
            _appliedPos = Vector3.zero;
            _appliedRot = Quaternion.identity;
            _appliedScale = Vector3.one;
        }

        private void Begin()
        {
            Cleanup();
            _running = true;
            _rng = new System.Random(System.Environment.TickCount);   // chỉ trang trí
            _fx = new GameObject("GameVoxelFx").transform;
            _owned.Add(_fx.gameObject);
            MeasureChest();
            BuildAudio();
            BuildFloor();
            _lives = 0;
            _power = 0;
            _powerScale = 1f;
            _locked = false;
            _shattered = _rebuilt = false;
            _rouletteT0 = _pullT0 = -1f;
            _steps = 0;
            _lastEvent = _lastImpact = _lastTail = _lockTime = -999f;
            _tailCount = 0;
            BuildLabel();
            StartCoroutine(Intro());
        }

        private void MeasureChest()
        {
            _chestRenderers.Clear();
            bool any = false;
            foreach (MeshRenderer r in _root.GetComponentsInChildren<MeshRenderer>())
            {
                if (r == null || !r.enabled) continue;
                if (r.GetComponent<MeshFilter>() == null) continue;
                string n = r.gameObject.name;
                if (n.StartsWith("Fx") || n.Contains("Silhouette") || n.Contains("Pip") || n.Contains("Ray") || n.Contains("Glow")) continue;
                _chestRenderers.Add(r);
                if (!any) { _chestBounds = r.bounds; any = true; }
                else _chestBounds.Encapsulate(r.bounds);
            }
            if (!any) _chestBounds = new Bounds(_root.position + Vector3.up * 0.2f, new Vector3(0.46f, 0.4f, 0.3f));
            _chestH = Mathf.Max(0.1f, _chestBounds.size.y);
            _chestW = Mathf.Max(0.1f, Mathf.Max(_chestBounds.size.x, _chestBounds.size.z));
            _floorY = _chestBounds.min.y;
        }

        private void BuildAudio()
        {
            _music = _fx.gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false;
            _music.loop = true;
            _music.spatialBlend = 0.3f;
            _music.volume = musicVolume * 0.33f;   // nền vào nhỏ (0,15), dâng tới musicVolume
            _sfx = _fx.gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0.3f;
            _fx.position = _chestBounds.center;
        }

        private void BuildFloor()
        {
            // Sàn vô hình đủ rộng để voxel, xu, nhân vật ngã có chỗ nảy.
            GameObject floor = new GameObject("VoxelFloor");
            floor.transform.SetParent(_fx, false);
            floor.transform.position = new Vector3(_chestBounds.center.x, _floorY - 0.05f, _chestBounds.center.z);
            BoxCollider c = floor.AddComponent<BoxCollider>();
            c.size = new Vector3(_chestW * 8f, 0.1f, _chestW * 8f);
        }

        // ================================================================== kịch bản

        private IEnumerator Intro()
        {
            // 0,0 s: âm bắt đầu NGAY khi bấm mở.
            Sfx(insertCoinClip, 1f, 0.5f);
            _hoverTarget = _chestH * 0.75f;      // rương nhảy lên làm "khối dấu hỏi"
            _bumpV = 2.2f;
            BuildQuestionMark();

            yield return new WaitForSeconds(0.3f);
            if (_music != null && loopBarClip != null)
            {
                _music.clip = loopBarClip;
                _music.pitch = _musicPitchTarget = 1f;
                _music.Play();
            }

            BuildHero();
            // Nhân vật chạy vào dưới rương và húc tới khi rương vỡ (roulette bắt đầu).
            Vector3 side = _root.right * (_chestW * 1.6f);
            Vector3 under = new Vector3(_chestBounds.center.x, _floorY, _chestBounds.center.z) + _root.forward * (-_chestW * 0.05f);
            yield return Run(under + side, under, 0.55f);
            int bumps = 0;
            while (!_shattered && !_locked && bumps < 3)
            {
                yield return HeadButt(under);
                bumps++;
                _musicPitchTarget = 1f + 0.05f * bumps;
                yield return new WaitForSeconds(0.25f);
            }
            if (!_locked && _hero != null) yield return Run(_hero.position, under - side * 0.8f, 0.35f);   // lùi ra xem
        }

        private IEnumerator Run(Vector3 from, Vector3 to, float seconds)
        {
            if (_hero == null) yield break;
            Vector3 dir = to - from;
            dir.y = 0f;
            if (dir.sqrMagnitude > 1e-6f) _hero.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            for (float t = 0f; t < seconds && _hero != null; t += Time.deltaTime)
            {
                float u = t / seconds;
                Vector3 p = Vector3.Lerp(from, to, u);
                p.y = _floorY + Mathf.Abs(Mathf.Sin(u * Mathf.PI * 6f)) * _chestH * 0.06f;   // bước chạy nhún nhảy
                _hero.position = p;
                yield return null;
            }
            if (_hero != null) _hero.position = new Vector3(to.x, _floorY, to.z);
        }

        private IEnumerator HeadButt(Vector3 under)
        {
            if (_hero == null) yield break;
            SfxAuto(jumpClip, 1f + (float)_rng.NextDouble() * 0.1f, 0.4f);
            float heroH = HeroHeight();
            float apex = Mathf.Max(0.02f, _floorY + _hover + Mathf.Max(0f, _bumpY) - heroH);   // đỉnh đầu chạm đáy rương
            float up = 0.18f;
            for (float t = 0f; t < up && _hero != null; t += Time.deltaTime)
            {
                float u = Mathf.Sin(t / up * Mathf.PI * 0.5f);
                _hero.position = new Vector3(under.x, Mathf.Lerp(_floorY, apex, u), under.z);
                yield return null;
            }
            // CỐP! rương nảy, xu bật ra
            SfxAuto(bumpClip, 1f, 0.5f);
            _bumpV = 2.6f;
            SpawnCoin(new Vector3(_chestBounds.center.x, _floorY + _hover + _chestH * 1.05f, _chestBounds.center.z), 1.1f);
            SfxImpact(coinClip, 1f + 0.05f * _steps, 0.35f);
            for (float t = 0f; t < 0.22f && _hero != null; t += Time.deltaTime)
            {
                float u = t / 0.22f;
                _hero.position = new Vector3(under.x, Mathf.Lerp(apex, _floorY, u * u), under.z);
                yield return null;
            }
        }

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            if (!_running) return;
            _rouletteT0 = Time.time;
            Shatter();
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (!_running) return;
            _steps++;
            _shownColor = color;
            float prog = RouletteProgress();
            SfxEvent(voxelTickClip, 0.9f + prog * 0.8f, 0.35f);
            _musicPitchTarget = Mathf.Max(_musicPitchTarget, 1.1f + prog * 0.45f);   // dâng dần (+≤60%)
            if (_music != null) _music.volume = musicVolume * Mathf.Lerp(0.33f, 1f, prog);   // 0,15 -> 0,45
            for (int i = 0; i < _voxelMats.Count; i++)
                ChestFxFactory.SetColor(_voxelMats[i], Color.Lerp(_voxelMatBase[i], color, 0.45f + 0.3f * prog));
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (!_running) return;
            _locked = true;
            _lockTime = Time.time;
            _tier = tier;
            StartCoroutine(Ending(tier, color));
        }

        public override void OnRevealed(ChestRevealSequence reveal)
        {
            if (!_running) return;
            StartCoroutine(FadeOut());
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            StopAllCoroutines();
            Cleanup();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            Cleanup();
        }

        // ================================================================== kết theo bậc

        private IEnumerator Ending(ChestTier tier, Color color)
        {
            // A4: nền DỪNG ngay (Stop, không fade). Trong lúc lắp nốt voxel (0,22 s) là khoảng lặng, rồi mới phát TC.
            if (_music != null) _music.Stop();
            yield return FinishRebuild(0.22f);
            _hoverTarget = 0f;
            for (int i = 0; i < _voxelMats.Count; i++) ChestFxFactory.SetColor(_voxelMats[i], color);

            if (tier == ChestTier.Rac)
            {
                yield return GameOver();
                yield break;
            }

            int rank = tier.Rank();
            Sfx(StingClip(rank), 1f, 1f);   // TC: đúng 1 tiếng chốt theo bậc (đã gồm fanfare)
            int coins = Mathf.Min(24, (rank * rank + 1) * (1 + _power));
            switch (tier)
            {
                case ChestTier.Thuong:
                    _bumpV = 1.6f;
                    break;
                case ChestTier.Hiem:
                    Say("NICE!", new Color(0.4f, 0.8f, 1f), 2f);
                    _bumpV = 2.4f;
                    break;
                case ChestTier.RatHiem:
                    Say(_lives > 0 ? $"1-UP x{_lives + 1}!!" : "1-UP!!", new Color(0.8f, 0.5f, 1f), 2f);
                    _bumpV = 3.4f;
                    StartCoroutine(Spin(360f, 0.6f));
                    break;
                default:
                    Say("HIGH SCORE!!!", new Color(1f, 0.85f, 0.2f), 2f);
                    _bumpV = 4.2f;
                    StartCoroutine(Spin(540f, 0.9f));
                    StartCoroutine(Fireworks(color));
                    break;
            }
            StartCoroutine(HeroCheer(rank));
            StartCoroutine(CoinRain(coins, rank));
            if (_lives > 0 && oneUpClip != null)
            {
                yield return new WaitForSeconds(0.6f);
                SfxAuto(oneUpClip, 1f, 0.4f);   // mạng dư thành điểm thưởng (đuôi)
            }
        }

        private IEnumerator GameOver()
        {
            // Rác: nhạc đã dừng, lặng 0,2 s (do lắp nốt voxel) rồi 1 tiếng xịt; không phát gì khác trong 0,6 s tiếp theo.
            Sfx(StingClip(0), 1f, 1f);
            Say("GAME OVER", new Color(1f, 0.3f, 0.3f), 2f);
            _squash = 0.72f;                       // rương xẹp xuống
            StartCoroutine(HeroFaint());
            yield return new WaitForSeconds(1.0f);
            SpawnSadVoxel();                       // "xịt": một voxel xám lăn ra
            if (_lives <= 0) yield break;
            // Còn mạng: "CONTINUE?" một lần - nhân vật húc lại... nhưng bậc không đổi nên vẫn thua (hài).
            yield return new WaitForSeconds(0.5f);
            _lives--;
            Say($"CONTINUE? x{_lives + 1}", Color.white, 1.2f);
            SfxAuto(oneUpClip, 1f, 0.4f);
            _squash = 1f;
            ReviveHero();
            Vector3 under = new Vector3(_chestBounds.center.x, _floorY, _chestBounds.center.z);
            if (_hero != null) yield return Run(_hero.position, under + _root.forward * (-_chestW * 0.9f), 0.3f);
            yield return HeadButt(under + _root.forward * (-_chestW * 0.9f));
        }

        private IEnumerator FadeOut()
        {
            yield return new WaitForSeconds(_tier == ChestTier.Rac ? 2.5f : 3f);
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                if (_music != null) _music.volume = Mathf.Lerp(_music.volume, 0f, t / 0.8f);
                yield return null;
            }
            Say("", Color.white, 0f);
        }

        private IEnumerator Spin(float degrees, float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                float u = t / seconds;
                _spinY = degrees * (1f - (1f - u) * (1f - u));
                yield return null;
            }
            _spinY = 0f;
        }

        private IEnumerator CoinRain(int count, int rank)
        {
            count = Mathf.Min(count, 24);
            Vector3 mouth = _reveal != null ? _reveal.MouthPosition : _chestBounds.center;
            for (int i = 0; i < count; i++)
            {
                SpawnCoin(mouth + Vector3.up * 0.02f, 1.2f + 0.25f * rank);
                if (i % 4 == 0) SfxAuto(coinClip, 1f + 0.03f * (i % 12), 0.35f);
                yield return new WaitForSeconds(Mathf.Lerp(0.12f, 0.03f, rank / 4f));
            }
        }

        private IEnumerator Fireworks(Color color)
        {
            // Pháo voxel cầu vồng: các khối có vật lý bắn lên, rơi xuống nảy trên sàn.
            Vector3 mouth = _reveal != null ? _reveal.MouthPosition : _chestBounds.center;
            const int waves = 3;   // 3 đợt x 8 khối = 24 khối (≤ 24)
            for (int w = 0; w < waves; w++)
            {
                Vector3 center = mouth + Vector3.up * (_chestH * (1.2f + 0.4f * w)) + RandomFlat() * _chestW * 0.6f;
                Material m = OwnedMat(Color.HSVToRGB((w * 0.23f) % 1f, 0.8f, 1f), "VxFirework");
                for (int i = 0; i < 8; i++)
                {
                    Rigidbody rb = MakeCube("Spark", center, _chestW * 0.05f, m, true);
                    rb.AddForce(Random3() * 1.8f, ForceMode.VelocityChange);
                    Destroy(rb.gameObject, 3f);
                }
                yield return new WaitForSeconds(0.35f);
            }
        }

        // ================================================================== rương: chuyển động cả khối

        private void UpdateMotion(float dt)
        {
            // Lơ lửng (khối dấu hỏi) + nảy kiểu lò xo khi bị húc / khi ăn mừng.
            _hover = Mathf.MoveTowards(_hover, _hoverTarget, dt * _chestH * 2.5f);
            _bumpV -= 9.8f * dt;
            _bumpY += _bumpV * dt;
            if (_bumpY < 0f)
            {
                _bumpY = 0f;
                _bumpV = Mathf.Abs(_bumpV) > 0.6f ? -_bumpV * 0.3f : 0f;
            }
            float bob = _hoverTarget > 0f ? Mathf.Sin(Time.time * 5f) * _chestH * 0.02f : 0f;
            _offPos = Vector3.up * (_hover + _bumpY * 0.25f + bob);
            if (_root.parent != null) _offPos = _root.parent.InverseTransformVector(_offPos);
            _offRot = Quaternion.AngleAxis(_spinY, Vector3.up);
            float sq = Mathf.Lerp(1f, _squash, 1f);
            _offScale = new Vector3(_powerScale * (2f - sq) * 0.5f + _powerScale * 0.5f, _powerScale * sq, _powerScale * (2f - sq) * 0.5f + _powerScale * 0.5f);

            if (_question != null)
            {
                _question.position = _chestBounds.center + Vector3.up * (_chestH * 0.95f + _hover + _bumpY * 0.25f);
                _question.rotation = Quaternion.AngleAxis(Time.time * 120f, Vector3.up);
            }
        }

        private void BuildQuestionMark()
        {
            // Dấu "?" 5x7 ghép bằng voxel (không dùng font).
            string[] rows = { ".###.", "#...#", "....#", "...#.", "..#..", ".....", "..#.." };
            float s = _chestH * 0.05f;
            _question = new GameObject("VxQuestion").transform;
            _question.SetParent(_fx, false);
            Material m = OwnedMat(new Color(1f, 0.85f, 0.2f), "VxQuestion");
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < rows[y].Length; x++)
                {
                    if (rows[y][x] != '#') continue;
                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(c.GetComponent<Collider>());
                    c.name = "q";
                    c.transform.SetParent(_question, false);
                    c.transform.localPosition = new Vector3((x - 2f) * s, (3f - y) * s, 0f);
                    c.transform.localScale = Vector3.one * s * 0.95f;
                    c.GetComponent<Renderer>().sharedMaterial = m;
                }
        }

        // ================================================================== voxel vỡ / lắp lại

        private void Shatter()
        {
            if (_shattered) return;
            _shattered = true;
            SfxAuto(shatterClip, 1f, 0.5f);
            if (_question != null) { Destroy(_question.gameObject); _question = null; }

            // Diện tích vỏ -> cỡ voxel để không vượt maxVoxels.
            float area = 0f;
            foreach (Renderer r in _chestRenderers)
            {
                Vector3 e = r.bounds.size;
                area += 2f * (e.x * e.y + e.y * e.z + e.x * e.z);
            }
            float size = Mathf.Max(minVoxelSize, Mathf.Sqrt(area / Mathf.Max(1, maxVoxels)));

            Vector3 center = _chestBounds.center;
            foreach (Renderer r in _chestRenderers)
            {
                int matStart = _voxelMats.Count;
                Color baseCol = SourceColor(r);
                for (int k = 0; k < 3; k++)   // 3 sắc độ cho mỗi phần: nhìn "pixel" hơn
                {
                    Color c = Color.Lerp(baseCol, k == 0 ? Color.black : Color.white, k == 1 ? 0f : 0.18f);
                    _voxelMats.Add(OwnedMat(c, "Vx_" + r.name));
                    _voxelMatBase.Add(c);
                }
                Bounds b = r.bounds;
                int nx = Mathf.Max(1, Mathf.RoundToInt(b.size.x / size));
                int ny = Mathf.Max(1, Mathf.RoundToInt(b.size.y / size));
                int nz = Mathf.Max(1, Mathf.RoundToInt(b.size.z / size));
                Vector3 step = new Vector3(b.size.x / nx, b.size.y / ny, b.size.z / nz);
                for (int x = 0; x < nx; x++)
                    for (int y = 0; y < ny; y++)
                        for (int z = 0; z < nz; z++)
                        {
                            bool shell = x == 0 || y == 0 || z == 0 || x == nx - 1 || y == ny - 1 || z == nz - 1;
                            if (!shell || _voxels.Count >= maxVoxels) continue;
                            Vector3 p = b.min + Vector3.Scale(step, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                            int mi = matStart + _rng.Next(3);
                            Rigidbody rb = MakeCube("Vx", p, size * 0.92f, _voxelMats[mi], true);
                            Vector3 dir = (p - center).normalized + Vector3.up * 0.8f + Random3() * 0.35f;
                            rb.AddForce(dir.normalized * shatterSpeed * (0.6f + 0.6f * (float)_rng.NextDouble()), ForceMode.VelocityChange);
                            rb.AddTorque(Random3() * 8f, ForceMode.VelocityChange);
                            _voxels.Add(new Voxel
                            {
                                t = rb.transform, rb = rb,
                                homeLocal = _root.InverseTransformPoint(p),
                                homeRotLocal = Quaternion.identity,
                                delay = (float)_rng.NextDouble() * 0.35f,
                                mat = mi,
                            });
                        }
            }
            foreach (Renderer r in _chestRenderers) if (r != null) r.enabled = false;
            _hoverTarget = _chestH * 0.25f;
        }

        private void UpdateVoxels()
        {
            if (!_shattered || _rebuilt || _voxels.Count == 0) return;
            float now = Time.time;
            if (_pullT0 < 0f)
            {
                if (now - _rouletteT0 < freeFlightSeconds) return;
                _pullT0 = now;
                SfxEvent(reassembleClip, 0.8f, 0.4f);
                for (int i = 0; i < _voxels.Count; i++)
                {
                    Voxel v = _voxels[i];
                    v.rb.isKinematic = true;
                    v.pullFrom = v.t.position;
                    v.pullFromRot = v.t.rotation;
                    _voxels[i] = v;
                }
            }
            float span = Mathf.Max(0.5f, expectedRouletteSeconds - freeFlightSeconds);
            // Lắp tới 92% theo thời gian; 8% cuối chỉ xong lúc chốt (giữ bí mật).
            float prog = Mathf.Clamp01((now - _pullT0) / span) * 0.92f;
            PoseVoxels(prog, now);
        }

        private void PoseVoxels(float prog, float now)
        {
            Quaternion rootRot = _root.rotation;
            for (int i = 0; i < _voxels.Count; i++)
            {
                Voxel v = _voxels[i];
                if (v.t == null) continue;
                float u = Mathf.Clamp01((prog - v.delay * (1f - prog)) / Mathf.Max(0.01f, 1f - v.delay * 0.5f));
                u = u * u * (3f - 2f * u);
                Vector3 home = _root.TransformPoint(v.homeLocal);
                // đường cong: voxel vồng lên rồi rơi đúng chỗ
                Vector3 p = Vector3.Lerp(v.pullFrom, home, u) + Vector3.up * Mathf.Sin(u * Mathf.PI) * _chestH * 0.5f;
                v.t.position = p;
                v.t.rotation = Quaternion.Slerp(v.pullFromRot, rootRot * v.homeRotLocal, u);
            }
        }

        private IEnumerator FinishRebuild(float seconds)
        {
            if (!_shattered || _rebuilt) yield break;
            if (_pullT0 < 0f)
            {
                _pullT0 = Time.time;
                for (int i = 0; i < _voxels.Count; i++)
                {
                    Voxel v = _voxels[i];
                    v.rb.isKinematic = true;
                    v.pullFrom = v.t.position;
                    v.pullFromRot = v.t.rotation;
                    _voxels[i] = v;
                }
            }
            float startProg = Mathf.Clamp01((Time.time - _pullT0) / Mathf.Max(0.5f, expectedRouletteSeconds - freeFlightSeconds)) * 0.92f;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                PoseVoxels(Mathf.Lerp(startProg, 1f, t / seconds), Time.time);
                yield return null;
            }
            PoseVoxels(1f, Time.time);
            _rebuilt = true;
            foreach (Renderer r in _chestRenderers) if (r != null) r.enabled = true;
            foreach (Voxel v in _voxels) if (v.t != null) Destroy(v.t.gameObject);
            _voxels.Clear();
        }

        // ================================================================== nhân vật pixel

        private float HeroHeight() => _chestH * 0.6f;

        private void BuildHero()
        {
            if (_hero != null) return;
            float h = HeroHeight();
            float u = h / 6f;   // 6 "pixel" cao
            GameObject root = new GameObject("VxHero");
            root.transform.SetParent(_fx, false);
            _hero = root.transform;
            Material suit = OwnedMat(new Color(0.15f, 0.75f, 0.65f), "VxHeroSuit");
            Material skin = OwnedMat(new Color(1f, 0.8f, 0.6f), "VxHeroSkin");
            Material visor = OwnedMat(new Color(1f, 0.45f, 0.1f), "VxHeroVisor");
            Material boot = OwnedMat(new Color(0.25f, 0.2f, 0.3f), "VxHeroBoot");
            Part(root.transform, boot, new Vector3(-0.6f, 0.5f, 0f) * u, new Vector3(0.9f, 1f, 1.2f) * u);
            Part(root.transform, boot, new Vector3(0.6f, 0.5f, 0f) * u, new Vector3(0.9f, 1f, 1.2f) * u);
            Part(root.transform, suit, new Vector3(0f, 2.25f, 0f) * u, new Vector3(2.2f, 2.5f, 1.4f) * u);
            Part(root.transform, skin, new Vector3(0f, 4.6f, 0f) * u, new Vector3(2f, 2f, 1.8f) * u);
            Part(root.transform, visor, new Vector3(0f, 4.8f, 0.85f) * u, new Vector3(1.6f, 0.6f, 0.3f) * u);
            Part(root.transform, suit, new Vector3(0f, 5.75f, 0f) * u, new Vector3(2.2f, 0.5f, 2f) * u);
            BoxCollider c = root.AddComponent<BoxCollider>();
            c.center = new Vector3(0f, 3f * u, 0f);
            c.size = new Vector3(2.2f * u, 6f * u, 1.4f * u);
            _heroBody = root.AddComponent<Rigidbody>();
            _heroBody.isKinematic = true;   // diễn bằng tay; chỉ thả vật lý khi ngã (GAME OVER)
            _heroBody.mass = 0.5f;
            _hero.position = new Vector3(_chestBounds.center.x, _floorY, _chestBounds.center.z) + _root.right * (_chestW * 1.6f);
        }

        private void Part(Transform parent, Material m, Vector3 pos, Vector3 scale)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(g.GetComponent<Collider>());
            g.name = "px";
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = m;
        }

        private IEnumerator HeroFaint()
        {
            if (_hero == null || _heroBody == null) yield break;
            // Ngã sấp bằng vật lý thật.
            _heroBody.isKinematic = false;
            _heroBody.AddForce((Vector3.up * 0.8f - _root.forward * 0.3f) * 1.2f, ForceMode.VelocityChange);
            _heroBody.AddTorque(_root.right * 6f, ForceMode.VelocityChange);
        }

        private void ReviveHero()
        {
            if (_hero == null || _heroBody == null) return;
            _heroBody.isKinematic = true;
            Vector3 p = _hero.position;
            _hero.SetPositionAndRotation(new Vector3(p.x, _floorY, p.z), Quaternion.LookRotation(_root.forward, Vector3.up));
        }

        private IEnumerator HeroCheer(int rank)
        {
            if (_hero == null) yield break;
            int jumps = 1 + rank;
            Vector3 basePos = _hero.position;
            for (int j = 0; j < jumps && _hero != null; j++)
            {
                SfxAuto(jumpClip, 1f + 0.08f * j, 0.4f);
                for (float t = 0f; t < 0.4f && _hero != null; t += Time.deltaTime)
                {
                    float u = t / 0.4f;
                    _hero.position = basePos + Vector3.up * Mathf.Sin(u * Mathf.PI) * HeroHeight() * (0.4f + 0.1f * rank);
                    _hero.rotation = Quaternion.AngleAxis(u * 360f * (rank >= 3 ? 1f : 0f), Vector3.up) * Quaternion.LookRotation(-_root.forward, Vector3.up);
                    yield return null;
                }
            }
        }

        // ================================================================== xu, nấm, voxel buồn

        private void SpawnCoin(Vector3 pos, float upSpeed)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);   // đồng xu dẹt
            g.name = "VxCoin";
            g.transform.SetParent(_fx, true);
            g.transform.position = pos;
            float d = _chestW * 0.12f;
            g.transform.localScale = new Vector3(d, d * 0.08f, d);
            g.transform.rotation = Quaternion.AngleAxis(90f, _root.right);
            g.GetComponent<Renderer>().sharedMaterial = OwnedMat(new Color(1f, 0.82f, 0.15f), "VxCoin");
            Rigidbody rb = g.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            rb.AddForce(Vector3.up * upSpeed + RandomFlat() * 0.35f, ForceMode.VelocityChange);
            rb.AddTorque(Vector3.up * 25f, ForceMode.VelocityChange);
            Destroy(g, 4f);
        }

        private void SpawnSadVoxel()
        {
            Vector3 mouth = _reveal != null ? _reveal.MouthPosition : _chestBounds.center;
            Rigidbody rb = MakeCube("VxSad", mouth, _chestW * 0.08f, OwnedMat(new Color(0.45f, 0.45f, 0.45f), "VxSad"), true);
            rb.AddForce(Vector3.up * 0.9f - _root.forward * 0.6f, ForceMode.VelocityChange);
            rb.AddTorque(Random3() * 4f, ForceMode.VelocityChange);
            Destroy(rb.gameObject, 4f);
        }

        private void HandleInput()
        {
            if (!allowDrops || _rebuilt && !_locked) return;
            if (PressedCoin()) DropItem(false);
            if (PressedMushroom()) DropItem(true);
        }

        private void DropItem(bool mushroom)
        {
            Vector3 top = _chestBounds.center + Vector3.up * (_chestH * 1.8f + _hover) + RandomFlat() * _chestW * 0.15f;
            GameObject g;
            if (mushroom)
            {
                g = new GameObject("VxMushroom");
                g.transform.position = top;
                Material cap = OwnedMat(new Color(0.9f, 0.25f, 0.3f), "VxCap");
                Material stem = OwnedMat(new Color(1f, 0.95f, 0.85f), "VxStem");
                float s = _chestW * 0.07f;
                Part(g.transform, stem, new Vector3(0f, 0f, 0f), new Vector3(1.2f, 1.4f, 1.2f) * s);
                Part(g.transform, cap, new Vector3(0f, 1.2f * s, 0f), new Vector3(2.4f, 1.2f, 2.4f) * s);
                BoxCollider c = g.AddComponent<BoxCollider>();
                c.center = new Vector3(0f, 0.5f * s, 0f);
                c.size = new Vector3(2.4f, 3f, 2.4f) * s;
            }
            else
            {
                g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                g.name = "VxDropCoin";
                g.transform.position = top;
                float d = _chestW * 0.14f;
                g.transform.localScale = new Vector3(d, d * 0.08f, d);
                g.transform.rotation = Quaternion.AngleAxis(90f, _root.right);
                g.GetComponent<Renderer>().sharedMaterial = OwnedMat(new Color(1f, 0.82f, 0.15f), "VxCoin");
            }
            g.transform.SetParent(_fx, true);
            Rigidbody rb = g.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.AddTorque(Random3() * 3f, ForceMode.VelocityChange);
            _drops.Add(new Drop { t = g.transform, rb = rb, mushroom = mushroom, born = Time.time });
            SfxAuto(jumpClip, mushroom ? 0.7f : 1.3f, 0.4f);
        }

        private void UpdateDrops()
        {
            for (int i = _drops.Count - 1; i >= 0; i--)
            {
                Drop d = _drops[i];
                if (d.t == null) { _drops.RemoveAt(i); continue; }
                Vector3 p = d.t.position;
                Vector3 c = _chestBounds.center + Vector3.up * (_hover + _bumpY * 0.25f);
                bool inside = Mathf.Abs(p.x - c.x) < _chestBounds.extents.x * 1.1f && Mathf.Abs(p.z - c.z) < _chestBounds.extents.z * 1.1f
                              && p.y < c.y + _chestBounds.extents.y * 1.05f;
                if (!inside && Time.time - d.born < 3f) continue;
                if (inside) Consume(d);
                Destroy(d.t.gameObject);
                _drops.RemoveAt(i);
            }
        }

        private void Consume(Drop d)
        {
            if (d.mushroom)
            {
                if (_power >= maxPower) return;
                _power++;
                _powerScale = 1f + 0.07f * _power;
                _bumpV = 2f;
                SfxAuto(powerUpClip, 1f, 0.5f);
            }
            else
            {
                _lives++;
                SfxAuto(oneUpClip, 1f, 0.5f);
            }
        }

        // ================================================================== chữ arcade (1 dòng TextMesh trên rương)

        private void BuildLabel()
        {
            _label = new GameObject("VxLabel").transform;
            _label.SetParent(_fx, false);
            _labelText = _label.gameObject.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null)
            {
                _labelText.font = font;
                _label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            }
            _labelText.anchor = TextAnchor.MiddleCenter;
            _labelText.characterSize = 0.02f * _chestH;
            _labelText.fontSize = 64;
            _labelText.text = "";
        }

        private void UpdateLabel()
        {
            if (_label == null || _labelText == null) return;
            _label.position = _chestBounds.center + Vector3.up * (_chestH * 1.25f + _hover + _bumpY * 0.25f);
            if (Camera.main != null)
                _label.rotation = Quaternion.LookRotation(_label.position - Camera.main.transform.position);
            if (_labelText.text.Length > 0 && Time.time > _labelUntil) _labelText.text = "";
        }

        private void Say(string text, Color color, float seconds)
        {
            if (_labelText == null) return;
            _labelText.text = text;
            _labelText.color = color;
            _labelUntil = Time.time + seconds;
        }

        // ================================================================== tiện ích

        private float RouletteProgress()
        {
            if (_rouletteT0 < 0f) return 0f;
            return Mathf.Clamp01((Time.time - _rouletteT0) / Mathf.Max(0.5f, expectedRouletteSeconds));
        }

        private AudioClip StingClip(int rank)
        {
            if (stingClips == null || rank < 0 || rank >= stingClips.Length) return null;
            return stingClips[rank];
        }

        // A3: sự kiện trước chốt, tối đa 1 tiếng mỗi 0,2 s, âm lượng ≤ 0,5.
        private void SfxEvent(AudioClip clip, float pitch, float volume)
        {
            if (clip == null || _locked) return;
            if (!ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap)) return;
            Sfx(clip, pitch, Mathf.Min(volume, ChestAudioRule.EventMax));
        }

        // A3: tiếng va chạm vật lý (xu...), cách nhau ≥ 0,12 s, âm lượng ≤ 0,4.
        private void SfxImpact(AudioClip clip, float pitch, float volume)
        {
            if (clip == null) return;
            if (_locked) { SfxTail(clip, pitch, volume); return; }
            if (!ChestAudioRule.TryGate(ref _lastImpact, ChestAudioRule.ImpactGap)) return;
            Sfx(clip, pitch, Mathf.Min(volume, 0.4f));
        }

        // A5: đuôi từ +0,4 s sau chốt, tối đa 1 tiếng mỗi 0,15 s, tổng ≤ 6, âm lượng ≤ 0,4.
        private void SfxTail(AudioClip clip, float pitch, float volume)
        {
            if (clip == null || Time.time - _lockTime < 0.4f || _tailCount >= 6) return;
            if (!ChestAudioRule.TryGate(ref _lastTail, ChestAudioRule.TailGap)) return;
            _tailCount++;
            Sfx(clip, pitch, Mathf.Min(volume, ChestAudioRule.TailMax));
        }

        private void SfxAuto(AudioClip clip, float pitch, float volume)
        {
            if (_locked) SfxTail(clip, pitch, volume);
            else SfxEvent(clip, pitch, volume);
        }

        private void Sfx(AudioClip clip, float pitch, float volume = 1f)
        {
            if (clip == null) return;
            if (_reveal != null && !Mathf.Approximately(pitch, 1f)) { _reveal.PlayOneShot(clip, sfxVolume * volume, pitch); return; }
            if (_sfx != null) _sfx.PlayOneShot(clip, sfxVolume * volume);
            else if (_reveal != null) _reveal.PlayOneShot(clip, sfxVolume * volume);
        }

        private Rigidbody MakeCube(string name, Vector3 pos, float size, Material m, bool physics)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            g.transform.SetParent(_fx, true);
            g.transform.position = pos;
            g.transform.localScale = Vector3.one * size;
            g.GetComponent<Renderer>().sharedMaterial = m;
            Rigidbody rb = g.AddComponent<Rigidbody>();
            rb.mass = 0.02f;
            rb.isKinematic = !physics;
            return rb;
        }

        private Material OwnedMat(Color c, string name)
        {
            Material m = ChestFxFactory.CreateUnlit(c, false, name);
            _owned.Add(m);
            return m;
        }

        private static Color SourceColor(Renderer r)
        {
            Material m = r != null ? r.sharedMaterial : null;
            if (m != null)
            {
                if (m.HasProperty("baseColorFactor")) return m.GetColor("baseColorFactor");   // glTFast
                if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor");
                if (m.HasProperty("_Color")) return m.GetColor("_Color");
            }
            return new Color(0.55f, 0.45f, 0.75f);
        }

        private Vector3 Random3() => new Vector3((float)_rng.NextDouble() * 2f - 1f, (float)_rng.NextDouble() * 2f - 1f, (float)_rng.NextDouble() * 2f - 1f);
        private Vector3 RandomFlat() { Vector3 v = Random3(); v.y = 0f; return v; }

        private static bool PressedCoin()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Z);
#endif
        }

        private static bool PressedMushroom()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.X);
#endif
        }

        private void Cleanup()
        {
            RemoveOffset();
            foreach (Renderer r in _chestRenderers) if (r != null) r.enabled = true;
            _chestRenderers.Clear();
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
            _voxels.Clear();
            _voxelMats.Clear();
            _voxelMatBase.Clear();
            _drops.Clear();
            _hero = null;
            _heroBody = null;
            _question = null;
            _music = _sfx = null;
            _fx = null;
            _running = false;
            _sawPlaying = _reveal != null && _reveal.IsPlaying;   // không tự chạy lại giữa chừng
            _offPos = Vector3.zero;
            _offRot = Quaternion.identity;
            _offScale = Vector3.one;
            _hover = _hoverTarget = _bumpV = _bumpY = _spinY = 0f;
            _squash = 1f;
            _label = null;
            _labelText = null;
            _lastEvent = _lastImpact = _lastTail = _lockTime = -999f;
            _tailCount = 0;
            _locked = false;
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}

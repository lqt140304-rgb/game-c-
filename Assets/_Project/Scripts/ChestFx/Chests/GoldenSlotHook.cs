using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Golden — "MÁY XÈNG KHO BÁU". Cả rương biến thành một máy đánh bạc:
    ///  • NGAY KHI BẤM MỞ: cần gạt mọc bên hông và tự kéo xuống ("cạch"), ba ô quay mọc trên nắp, vòng lặp rè rè +
    ///    tích tắc bắt đầu; pitch/âm lượng, tốc độ ô quay và độ rung cả rương dâng dần (độ căng).
    ///  • Mỗi nhịp màu của roulette: rương giật nảy nhẹ, một ô loé màu đang hiện (chỉ trang trí).
    ///  • Khi bậc đã chốt (OnTierLocked): ô 1, ô 2 dừng "thụp"; ô 3 giữ lâu hơn (giữ bí mật tới cuối).
    ///    – RÁC: ô 3 lệch màu, vòng lặp tắt hẳn, im 0,2 s rồi một tiếng xịt, rương xẹp như xì hơi, nhả 1 đồng xu
    ///      èo uột; mèo thần tài nhô lên rồi bị đẩy ngã (Rigidbody).
    ///    – THƯỜNG trở lên: ba ô trùng màu, rương nhảy bật (Huyền thoại xoay một vòng), đồng xu Rigidbody thật phun
    ///      ra từ miệng rương, rơi, nảy, va nhau kêu leng keng; mèo thần tài vẫy tay (Thường: nhảy nhẹ, 3 xu).
    ///  • TƯƠNG TÁC: phím [H] kéo cần gạt (≤ 3 lần, lần 3 tự thả xu mồi) và khe "xu mồi" trên bảng ô quay (trigger). Thả bất kỳ Rigidbody nào có <see cref="GoldenToken"/>
    ///    (hoặc tên chứa "Coin"/"Xu") vào khe — hoặc gọi <see cref="InsertToken"/> — thì mèo thần tài xuất hiện sớm,
    ///    vỗ cần gạt thêm một lần (độ căng +), và jackpot (nếu có) phun gấp rưỡi xu. KHÔNG đổi bậc.
    /// Bậc do game quyết qua ChestRevealSequence.Play; hook chỉ trang trí, mọi ngẫu nhiên dùng hạt giống riêng.
    /// Đặt lên gốc rương (menu Tools/Cho Do La/Add Golden Slot Hook To Selected Chest).
    /// </summary>
    public class GoldenSlotHook : ChestFxHook
    {
        [Header("Âm (GoldenSlotSetup tự gán từ Audio/Chests/golden/)")]
        public AudioClip leverClip;
        public AudioClip spinLoopClip;
        public AudioClip reelStopClip;
        [Tooltip("TC theo bậc (Rác…Huyền thoại): sfx_golden_lock_*.mp3. Hook tự phát đúng 1 tiếng lúc chốt.")]
        public AudioClip[] stingClips = new AudioClip[ChestTierUtil.Count];
        public AudioClip coinClip;
        public AudioClip tokenClip;

        [Header("Nhịp")]
        [Tooltip("Thời gian (giây) để độ căng tiến gần mức tối đa.")]
        [SerializeField] private float tensionRise = 2.5f;
        [SerializeField] private float reelGap = 0.35f;
        [Tooltip("Ô thứ ba giữ thêm bao lâu trước khi lộ kết quả.")]
        [SerializeField] private float suspenseHold = 0.7f;

        [Header("Vật lý")]
        [SerializeField] private int[] coinsPerTier = { 1, 3, 12, 22, 36 };
        [SerializeField] private float coinImpulse = 1.6f;
        [SerializeField] private float coinLifetime = 6f;
        [SerializeField] private float maxShake = 0.012f;
        [SerializeField] private int decorationSeed = 0;

        private static readonly Color Gold = new Color(1f, 0.78f, 0.2f);
        private static readonly Color Dull = new Color(0.35f, 0.33f, 0.3f);

        private ChestRevealSequence _reveal;
        private bool _wasPlaying, _spinning, _tokenUsed;
        private float _t0, _tensionBonus;
        private System.Random _rng;

        private GameObject _rig;
        private Transform _lever, _cat, _catPaw;
        private readonly Transform[] _reels = new Transform[3];
        private readonly Material[] _reelMats = new Material[3];
        private readonly float[] _reelSpeed = new float[3];
        private readonly List<Object> _owned = new List<Object>();
        private AudioSource _loop;

        // chuyển động áp lên cả rương: cộng/trừ phần lệch mỗi khung để không phá vị trí gốc
        private Vector3 _appliedOffset;
        private Vector3 _baseScale;
        private float _hop, _spinY, _squash;

        private float _lastEvent = -999f, _lastTail = -999f;
        private float _tailFrom = float.MaxValue, _tailUntil;
        private int _tails, _pulls;
        private float _s = 0.5f;
        private Quaternion _baseRot = Quaternion.identity;
        private const int MaxPulls = 3, MaxTails = 6;

        // ------------------------------------------------------------------ vòng đời

        private void Awake() => _reveal = GetComponentInParent<ChestRevealSequence>();

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            _reveal = reveal;
            if (_rig == null) Press(); // khi không có BoxOpenSequence, đây là lúc bấm
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (_rig == null || !_spinning) return;
            _hop = Mathf.Max(_hop, 0.006f + 0.01f * Tension());
            int i = _rng.Next(3);
            ChestFxFactory.SetColor(_reelMats[i], color);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_rig == null) Press();
            StartCoroutine(Finale(tier, color));
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Update()
        {
            // Âm và cơ cấu chạy NGAY khi game gọi Play (IsPlaying bật), không chờ nắp hay roulette.
            bool playing = _reveal != null && _reveal.IsPlaying;
            if (playing && !_wasPlaying && _rig == null) Press();
            _wasPlaying = playing;
            if (_rig == null) return;

            if (_spinning && _pulls < MaxPulls && LeverPressed()) PullByPlayer();

            float k = Tension();
            if (_spinning)
            {
                if (_loop != null)
                {
                    // A2: nền 0,15 → 0,45, cao độ tăng ~45% tới lúc chốt
                    _loop.pitch = 0.9f + 0.4f * k;
                    _loop.volume = 0.15f + 0.3f * k;
                }
                for (int i = 0; i < 3; i++) _reelSpeed[i] = 360f + 1100f * k;
            }
            for (int i = 0; i < 3; i++)
                if (_reels[i] != null) _reels[i].Rotate(_reelSpeed[i] * Time.deltaTime, 0f, 0f);

            if (_catPaw != null) _catPaw.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 9f) * 35f);
        }

        private void LateUpdate()
        {
            if (_rig == null) return;
            Transform root = _reveal != null ? _reveal.transform : transform;
            root.position -= _appliedOffset;

            float shake = _spinning ? maxShake * Tension() : 0f;
            Vector3 jitter = new Vector3(Mathf.PerlinNoise(Time.time * 31f, 0.1f) - 0.5f, 0f,
                                         Mathf.PerlinNoise(0.7f, Time.time * 29f) - 0.5f) * 2f * shake;
            _hop = Mathf.MoveTowards(_hop, 0f, Time.deltaTime * 0.12f);
            _appliedOffset = jitter + Vector3.up * _hop;
            root.position += _appliedOffset;

            _squash = Mathf.MoveTowards(_squash, 0f, Time.deltaTime * 0.6f);
            root.localScale = new Vector3(_baseScale.x * (1f + _squash * 0.5f), _baseScale.y * (1f - _squash), _baseScale.z * (1f + _squash * 0.5f));
        }

        private float Tension() => Mathf.Clamp01(1f - Mathf.Exp(-(Time.time - _t0) / tensionRise) + _tensionBonus);

        // ------------------------------------------------------------------ bấm mở

        private void Press()
        {
            Transform root = _reveal != null ? _reveal.transform : transform;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);
            _t0 = Time.time;
            _tensionBonus = 0f;
            _tokenUsed = false;
            _pulls = _tails = 0;
            _tailFrom = float.MaxValue;
            _lastEvent = _lastTail = -999f;
            _baseScale = root.localScale;
            _baseRot = root.localRotation;
            _appliedOffset = Vector3.zero;
            BuildRig(root);
            _spinning = true;
            PlayEvent(leverClip, 0.5f, 1f);
            StartCoroutine(PullLever(0.25f));
            if (spinLoopClip != null)
            {
                _loop.clip = spinLoopClip;
                _loop.loop = true;
                _loop.pitch = 0.9f;
                _loop.volume = 0.15f;
                _loop.Play();
            }
        }

        /// <summary>[H]: người chơi kéo cần (≤ 3 lần). Chỉ trang trí: căng hơn một chút, rương giật nhẹ; lần 3 thả xu mồi.</summary>
        private void PullByPlayer()
        {
            _pulls++;
            StartCoroutine(PullLever(0.2f));
            PlayEvent(leverClip, 0.5f, 1f + 0.1f * _pulls);
            _tensionBonus += 0.1f;
            _hop = Mathf.Max(_hop, 0.01f);
            if (_pulls >= MaxPulls) InsertToken();
        }

        private IEnumerator PullLever(float seconds)
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
            {
                if (_lever == null) yield break;
                _lever.localRotation = Quaternion.Euler(Mathf.Sin(t * Mathf.PI) * 70f, 0f, 0f);
                yield return null;
            }
            if (_lever != null) _lever.localRotation = Quaternion.identity;
        }

        // ------------------------------------------------------------------ kết

        private IEnumerator Finale(ChestTier tier, Color color)
        {
            bool win = tier >= ChestTier.Thuong;
            int ti = Mathf.Clamp((int)tier, 0, ChestTierUtil.Count - 1);
            _tailUntil = Time.time + 3f;                       // A5: đuôi hết trước +3 s sau chốt
            for (int i = 0; i < 2; i++)
            {
                StopReel(i, color);
                yield return new WaitForSeconds(reelGap);
            }
            // ô 3 giữ bí mật: còn quay (nền vẫn ngân, không thêm lớp nào)
            yield return new WaitForSeconds(suspenseHold * (win ? 1.3f : 1f));

            // Nền dừng ngay, không fade (A4)
            _spinning = false;
            for (int i = 0; i < 3; i++) _reelSpeed[i] = 0f;
            if (_loop != null) _loop.Stop();
            StopReel(2, win ? color : Dull, false);

            if (!win)
            {
                yield return new WaitForSeconds(ChestAudioRule.RacSilence);   // im phắc 0,2 s
                Play(StingFor(ti), 0.8f);                                      // TC Rác: một tiếng xịt
                _tailFrom = Time.time + 0.6f;                                  // 0,6 s sau đó không phát gì
                _squash = 0.18f;                                               // xì hơi
                if (_lever != null) _lever.localRotation = Quaternion.Euler(55f, 0f, 0f); // cần gạt rũ xuống
                yield return new WaitForSeconds(0.5f);
                SpawnCoins(coinsPerTier[ti], 0.35f, Gold);
                ShowCat(false);
            }
            else
            {
                float[] stingVol = { 0.8f, 0.7f, 0.85f, 1f, 1f };
                Play(StingFor(ti), stingVol[ti]);                             // TC: đúng 1 tiếng
                _tailFrom = Time.time + 0.4f;                                  // A5: đuôi từ +0,4 s
                _hop = tier == ChestTier.HuyenThoai ? 0.22f : tier == ChestTier.Thuong ? 0.05f : 0.12f;
                if (tier == ChestTier.HuyenThoai) StartCoroutine(SpinChest(0.5f));
                int n = coinsPerTier[ti];
                if (_tokenUsed) n = Mathf.RoundToInt(n * 1.5f);
                StartCoroutine(CoinFountain(Mathf.Min(40, n), color));
                ShowCat(true);
            }
        }

        private AudioClip StingFor(int tierIndex) =>
            stingClips != null && tierIndex < stingClips.Length ? stingClips[tierIndex] : null;

        private void StopReel(int i, Color c, bool sound = true)
        {
            _reelSpeed[i] = 0f;
            if (_reels[i] != null) _reels[i].localRotation = Quaternion.identity;
            ChestFxFactory.SetColor(_reelMats[i], c);
            if (sound) Play(reelStopClip, 0.5f, 0.9f + 0.1f * i);
        }

        private IEnumerator SpinChest(float seconds)
        {
            Transform root = _reveal != null ? _reveal.transform : transform;
            float done = 0f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
            {
                float target = Mathf.SmoothStep(0f, 360f, t);
                root.Rotate(0f, target - done, 0f);
                done = target;
                yield return null;
            }
            root.Rotate(0f, 360f - done, 0f);
        }

        private IEnumerator CoinFountain(int count, Color tierColor)
        {
            int perBurst = Mathf.Max(1, count / 6);
            while (count > 0)
            {
                int n = Mathf.Min(perBurst, count);
                SpawnCoins(n, 1f, Color.Lerp(Gold, tierColor, 0.25f));
                count -= n;
                yield return new WaitForSeconds(0.08f);
            }
        }

        // ------------------------------------------------------------------ xu vật lý

        private void SpawnCoins(int count, float power, Color c)
        {
            if (_rig == null) return;
            Vector3 mouth = _reveal != null ? _reveal.MouthPosition : transform.position + Vector3.up * 0.2f;
            Transform root = _reveal != null ? _reveal.transform : transform;
            Material m = ChestFxFactory.CreateUnlit(c, false, "GoldenCoin");
            _owned.Add(m);
            for (int i = 0; i < count; i++)
            {
                GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                coin.name = "GoldenSlotCoin";
                coin.transform.SetParent(_rig.transform, true);
                coin.transform.position = mouth + new Vector3(Rand(-0.08f, 0.08f), 0.02f, Rand(-0.06f, 0.06f));
                coin.transform.localScale = new Vector3(0.09f, 0.008f, 0.09f) * _s;
                coin.transform.rotation = Quaternion.Euler(Rand(0f, 360f), Rand(0f, 360f), 0f);
                coin.GetComponent<Renderer>().sharedMaterial = m;
                Rigidbody rb = coin.AddComponent<Rigidbody>();
                rb.mass = 0.02f;
                Vector3 dir = (root.up * 2.2f + new Vector3(Rand(-1f, 1f), 0f, Rand(-1f, 1f))).normalized;
                rb.AddForce(dir * coinImpulse * power * rb.mass * Rand(0.7f, 1.3f) * 1.6f * Mathf.Sqrt(_s / 0.5f), ForceMode.Impulse);
                rb.angularVelocity = new Vector3(Rand(-20f, 20f), Rand(-20f, 20f), Rand(-20f, 20f));
                coin.AddComponent<GoldenCoinClink>().owner = this;
                Destroy(coin, coinLifetime + Rand(0f, 1f));
            }
        }

        /// <summary>Đồng xu báo va chạm: phát tiếng leng keng (giới hạn mật độ, pitch theo hạt giống).</summary>
        internal void Clink(Vector3 at, float speed)
        {
            // đuôi (A5): từ +0,4 s, ≤ 6 tiếng, cách ≥ 0,15 s, trước +3 s sau chốt, âm ≤ 0,4
            if (coinClip == null || speed < 0.3f || _tails >= MaxTails) return;
            if (Time.time < _tailFrom || Time.time > _tailUntil) return;
            if (!ChestAudioRule.TryGate(ref _lastTail, Mathf.Max(ChestAudioRule.TailGap, ChestAudioRule.ImpactGap))) return;
            _tails++;
            Play(coinClip, Mathf.Min(ChestAudioRule.TailMax, speed * 0.3f), Rand(0.85f, 1.25f));
        }

        // ------------------------------------------------------------------ tương tác phụ

        /// <summary>Xu mồi lọt khe: mèo thần tài ra sớm, vỗ cần gạt thêm lần nữa. Không đổi bậc.</summary>
        public void InsertToken()
        {
            if (_rig == null || !_spinning || _tokenUsed) return;
            _tokenUsed = true;
            _tensionBonus += 0.2f;
            PlayEvent(tokenClip, 0.5f, 1f);
            ShowCat(true);
            StartCoroutine(PullLever(0.2f));
        }

        /// <summary>Thử nhanh trong Editor: thả một xu mồi vật lý từ trên khe xuống.</summary>
        [ContextMenu("Thả xu mồi (thử)")]
        public void DropTestToken()
        {
            if (_rig == null) return;
            GameObject t = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            t.name = "XuMoi";
            t.transform.position = _rig.transform.position + _rig.transform.up * 0.75f;
            t.transform.localScale = new Vector3(0.05f, 0.005f, 0.05f);
            t.AddComponent<Rigidbody>().mass = 0.02f;
            t.AddComponent<GoldenToken>();
            Destroy(t, 5f);
        }

        private void ShowCat(bool happy)
        {
            if (_cat == null) return;
            _cat.gameObject.SetActive(true);
            _catPaw.gameObject.SetActive(happy);
            if (!happy)
            {
                Rigidbody rb = _cat.gameObject.GetComponent<Rigidbody>();
                if (rb == null) rb = _cat.gameObject.AddComponent<Rigidbody>();
                rb.mass = 0.2f;
                rb.AddForce(-_cat.right * 0.15f + Vector3.up * 0.1f, ForceMode.Impulse); // bị hất ngã
            }
        }

        // ------------------------------------------------------------------ dựng hình (primitive, không cần asset)

        private void BuildRig(Transform root)
        {
            Cleanup();
            Bounds b = ChestBounds(root);
            _s = Mathf.Max(0.1f, b.size.x, b.size.y, b.size.z);
            float w = Mathf.Max(b.size.x, 0.3f);
            _rig = new GameObject("GoldenSlotRig");
            _rig.transform.SetPositionAndRotation(root.position, root.rotation);
            _owned.Add(_rig);

            Material gold = ChestFxFactory.CreateUnlit(Gold, false, "GoldenSlotGold");
            Material dark = ChestFxFactory.CreateUnlit(new Color(0.15f, 0.05f, 0.05f), false, "GoldenSlotPanel");
            _owned.Add(gold); _owned.Add(dark);

            // bảng ô quay đi theo rương (con của root) để rung/nhảy cùng
            Transform panel = Part(PrimitiveType.Cube, "Marquee", root, dark,
                root.InverseTransformPoint(b.center + root.up * (b.extents.y + 0.12f)), new Vector3(w * 0.9f, 0.16f, 0.04f), false).transform;
            for (int i = 0; i < 3; i++)
            {
                _reelMats[i] = ChestFxFactory.CreateUnlit(Gold, false, "GoldenSlotReel" + i);
                _owned.Add(_reelMats[i]);
                GameObject reel = Part(PrimitiveType.Cylinder, "Reel" + i, panel, _reelMats[i],
                    new Vector3((i - 1) * 0.3f, 0f, -0.6f), new Vector3(0.26f, 0.12f, 0.8f), false);
                reel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                _reels[i] = reel.transform;
            }
            // khe xu mồi (trigger) trên đỉnh bảng
            GameObject slot = Part(PrimitiveType.Cube, "KheXuMoi", panel, gold, new Vector3(0f, 0.6f, 0f), new Vector3(0.25f, 0.25f, 1.2f), true);
            slot.GetComponent<Collider>().isTrigger = true;
            slot.AddComponent<GoldenTokenSlot>().owner = this;

            // cần gạt bên phải
            GameObject pivot = new GameObject("LeverPivot");
            pivot.transform.SetParent(root, false);
            pivot.transform.position = b.center + root.right * (b.extents.x + 0.03f);
            _owned.Add(pivot);
            _lever = pivot.transform;
            Part(PrimitiveType.Cylinder, "LeverArm", _lever, gold, new Vector3(0f, 0.12f, 0f), new Vector3(0.02f, 0.12f, 0.02f), false);
            Part(PrimitiveType.Sphere, "LeverKnob", _lever, dark, new Vector3(0f, 0.25f, 0f), Vector3.one * 0.06f, false);

            // mèo thần tài bên trái (ẩn tới khi cần)
            GameObject cat = new GameObject("MeoThanTai");
            cat.transform.SetParent(_rig.transform, false);
            cat.transform.position = b.center - root.right * (b.extents.x + 0.12f) + Vector3.down * (b.extents.y - 0.07f);
            _cat = cat.transform;
            Part(PrimitiveType.Sphere, "Body", _cat, gold, Vector3.zero, new Vector3(0.11f, 0.13f, 0.1f), true);
            Part(PrimitiveType.Sphere, "Head", _cat, gold, new Vector3(0f, 0.1f, 0f), Vector3.one * 0.09f, false);
            GameObject paw = new GameObject("PawPivot");
            paw.transform.SetParent(_cat, false);
            paw.transform.localPosition = new Vector3(0.05f, 0.06f, 0f);
            _catPaw = paw.transform;
            Part(PrimitiveType.Sphere, "Paw", _catPaw, gold, new Vector3(0f, 0.04f, 0f), new Vector3(0.03f, 0.06f, 0.03f), false);
            cat.SetActive(false);

            // nền tạm cho xu nảy nếu dưới rương không có collider
            if (!Physics.Raycast(root.position + Vector3.up * 0.05f, Vector3.down, 0.3f))
            {
                GameObject ground = new GameObject("GoldenSlotGround");
                ground.transform.SetParent(_rig.transform, false);
                ground.transform.position = root.position;
                BoxCollider bc = ground.AddComponent<BoxCollider>();
                bc.center = new Vector3(0f, -0.05f, 0f);
                bc.size = new Vector3(3f, 0.1f, 3f);
            }

            _loop = _rig.AddComponent<AudioSource>();
            _loop.playOnAwake = false;
            _loop.spatialBlend = 0f;
        }

        private GameObject Part(PrimitiveType type, string name, Transform parent, Material m, Vector3 localPos, Vector3 localScale, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = m;
            if (!keepCollider) Destroy(go.GetComponent<Collider>());
            if (parent != null && parent.IsChildOf(_rig.transform) == false) _owned.Add(go);
            return go;
        }

        private static Bounds ChestBounds(Transform root)
        {
            Renderer[] rs = root.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(root.position + Vector3.up * 0.25f, new Vector3(0.6f, 0.5f, 0.42f));
            Bounds b = rs[0].bounds;
            foreach (Renderer r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        // ------------------------------------------------------------------ tiện ích

        private float Rand(float a, float b) => a + (float)(_rng ?? (_rng = new System.Random(1))).NextDouble() * (b - a);

        /// <summary>Sự kiện lẻ trước chốt (A3): tối đa 1 tiếng mỗi 0,2 s, âm ≤ 0,5.</summary>
        private void PlayEvent(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || !ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap)) return;
            Play(clip, Mathf.Min(volume, ChestAudioRule.EventMax), pitch);
        }

#if ENABLE_INPUT_SYSTEM
        private static bool LeverPressed() => Keyboard.current != null && Keyboard.current[Key.H].wasPressedThisFrame;
#else
        private static bool LeverPressed() => Input.GetKeyDown(KeyCode.H);
#endif

        private void Play(AudioClip clip, float volume, float pitch = 1f)
        {
            if (clip == null) return;
            if (_reveal != null) _reveal.PlayOneShot(clip, volume, pitch);
            else AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }

        private void Cleanup()
        {
            StopAllCoroutines();
            Transform root = _reveal != null ? _reveal.transform : transform;
            if (_rig != null)
            {
                root.position -= _appliedOffset;
                root.localScale = _baseScale;
                root.localRotation = _baseRot;   // SpinChest xoay trực tiếp: trả đúng góc gốc
            }
            _appliedOffset = Vector3.zero;
            _hop = _squash = 0f;
            for (int i = _owned.Count - 1; i >= 0; i--) if (_owned[i] != null) Destroy(_owned[i]);
            _owned.Clear();
            _rig = null; _lever = null; _cat = null; _catPaw = null; _loop = null;
            for (int i = 0; i < 3; i++) { _reels[i] = null; _reelMats[i] = null; _reelSpeed[i] = 0f; }
            _spinning = false;
            _pulls = 0;
        }
    }

    /// <summary>Đánh dấu một vật là "xu mồi" cho khe của <see cref="GoldenSlotHook"/>.</summary>
    public class GoldenToken : MonoBehaviour { }

    /// <summary>Khe xu mồi (trigger) trên bảng ô quay.</summary>
    public class GoldenTokenSlot : MonoBehaviour
    {
        public GoldenSlotHook owner;

        private void OnTriggerEnter(Collider other)
        {
            if (owner == null || other.attachedRigidbody == null) return;
            GameObject go = other.attachedRigidbody.gameObject;
            if (go.GetComponent<GoldenToken>() != null || go.name.Contains("Coin") || go.name.Contains("Xu"))
            {
                if (go.name == "GoldenSlotCoin") return; // xu của chính máy không tính
                owner.InsertToken();
                Destroy(go);
            }
        }
    }

    /// <summary>Gắn lên mỗi đồng xu vật lý: báo va chạm để phát tiếng leng keng.</summary>
    public class GoldenCoinClink : MonoBehaviour
    {
        public GoldenSlotHook owner;

        private void OnCollisionEnter(Collision c)
        {
            if (owner != null) owner.Clink(transform.position, c.relativeVelocity.magnitude);
        }
    }
}

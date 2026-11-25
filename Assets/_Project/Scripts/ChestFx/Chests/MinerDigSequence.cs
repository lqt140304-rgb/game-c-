using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Kịch bản ĐẶC TRƯNG của Chest_Miner: "ĐÀO RƯƠNG RA KHỎI MỎ".
    ///
    /// ĐÃ THU GỌN theo kế hoạch 2026-11-25: kịch bản tự chạy khi bấm (bước Cue "dao_mo" của ChestScript_Miner, rồi OpenBox),
    /// đoạn đào ≤ 3 s, 12 khối đá, chỉ dùng âm ElevenLabs (không còn MinerSynth).
    ///
    /// Bấm mở → ChestScript chạy cue "dao_mo" (<see cref="OnCue"/>), xong mới mở nắp: rương bị vùi trong một vỏ đá gồm nhiều
    /// khối Rigidbody thật; tiếng mỏ rền + nhịp "thình" bắt đầu NGAY. Cuốc tự đập (hoặc người chơi bấm F / chuột trái để
    /// đập nhanh hơn): mỗi nhát văng vài khối đá bằng lực, rương rung, sỏi rơi từ trần. Tương tác phụ: bấm G (hoặc ném
    /// một vật có <see cref="MinerDynamite"/> vào đá) để cắm THUỐC NỔ — ngòi cháy xèo xèo, nhịp dồn dập, nổ tung mọi khối
    /// đá còn lại, rương nảy lên rồi rơi xuống. Không ai cắm thì tới nhát áp chót tự cắm (để lần nào cũng có đoạn nổ).
    /// Sự kiện phụ: chim hoàng yến của thợ mỏ bay vào đậu trên tảng đá cạnh rương, run run theo độ căng.
    /// Đá sạch → gọi ChestRevealSequence.Play(rolledTier, item) như cũ (bánh xe màu + bóng vật phẩm GIỮ NGUYÊN), tiếng mỏ
    /// tiếp tục dâng (pitch lên theo từng tick màu) và CHỈ ở lúc chốt bậc mới lộ kết quả:
    ///   • Rác/Thường: tiếng rền TẮT PHỤT, còi "xịt" tụt, chim ngất lăn xuống (Rác: thêm hòn sỏi rơi "cốc" lên rương).
    ///   • Hiếm: tiếng rền tắt êm, chim hót.
    ///   • Rất hiếm/Huyền thoại: nổ lần hai (mạch vàng), trần sập nhẹ thêm sỏi, chim bay vòng hót;
    ///     Huyền thoại: vàng cục (Rigidbody, màu bậc) phun ra từ miệng rương rơi leng keng.
    ///
    /// Bất biến: bậc do game truyền vào, script này KHÔNG đổi nó; mọi ngẫu nhiên (số khối, vị trí, số nhát) chỉ trang trí,
    /// theo hạt giống. Là một <see cref="ChestFxHook"/> nên ChestRevealSequence tự gọi các bước sau.
    /// </summary>
    public class MinerDigSequence : ChestFxHook
    {
        [Header("Đá vùi")]
        [SerializeField] private int chunkCount = 12;
        [SerializeField] private Color rockColor = new Color(0.32f, 0.27f, 0.23f);
        [Tooltip("Khối đá to bằng bao nhiêu phần kích thước rương.")]
        [SerializeField] private float chunkScale = 0.38f;
        [SerializeField] private float chunkMass = 0.6f;
        [Tooltip("Giây sau khi văng thì khối đá thu nhỏ rồi biến mất.")]
        [SerializeField] private float debrisLifetime = 2.2f;

        [Header("Cuốc")]
        [SerializeField] private int strikesMin = 2;
        [SerializeField] private int strikesMax = 3;
        [Tooltip("Giây giữa hai nhát tự đập (người chơi bấm thì nhanh hơn).")]
        [SerializeField] private float autoStrikeInterval = 0.55f;
        [SerializeField] private float strikeImpulse = 1.6f;
        [SerializeField] private bool playerInput = true;

        [Header("Thuốc nổ")]
        [SerializeField] private float fuseSeconds = 1.0f;
        [SerializeField] private float blastImpulse = 4.5f;
        [Tooltip("Không ai cắm thì tự cắm ở nhát áp chót.")]
        [SerializeField] private bool autoDynamite = true;
        [SerializeField] private float hopHeight = 0.12f;

        [Header("Âm (ElevenLabs, thư mục Audio/Chests/miner)")]
        [Tooltip("Âm lượng tổng, nhân MỘT lần (mặc định 0,8 = đúng các trần của quy tắc âm).")]
        [Range(0f, 1f)] [SerializeField] private float volume = 0.8f;
        [Tooltip("Nền: vòng lặp tiếng mỏ rền.")] public AudioClip rumbleClip;
        [Tooltip("Nhịp: tiếng thình.")] public AudioClip thumpClip;
        public AudioClip pickClip;
        public AudioClip crumbleClip;
        public AudioClip fuseClip;
        [Tooltip("Tiếng nổ thuốc nổ trước chốt.")] public AudioClip boomClip;
        public AudioClip chirpClip;
        public AudioClip faintClip;
        public AudioClip nuggetClip;
        public AudioClip pebbleClip;
        [Tooltip("Tiếng chốt theo bậc (Rác..Huyền thoại), hook tự phát lúc OnTierLocked.")] public AudioClip[] stingClips = new AudioClip[5];

        [Header("Trang trí ngẫu nhiên (không ảnh hưởng kết quả)")]
        [Tooltip("0 = khác mỗi lần.")]
        [SerializeField] private int decorationSeed;

        private enum Phase { Idle, Digging, Revealing, Done }
        private const float BaseVolume = 0.8f;
        private Phase _phase = Phase.Idle;

        private ChestRevealSequence _reveal;
        private ChestTier _tier;
        private System.Random _rng;

        private Transform _root;                 // mọi thứ tạo ra nằm dưới đây (KHÔNG làm con của rương: tránh đo sai bounds)
        private readonly List<Rigidbody> _attached = new List<Rigidbody>();   // đá còn bám quanh rương (kinematic)
        private readonly List<Object> _owned = new List<Object>();
        private Material _rockMat, _fuseMat, _canaryMat, _nuggetMat;
        private AudioSource _loop, _oneShot;
        private Transform _pick, _dynamite, _fuseSpark, _canary;
        private Light _flash;
        private Rigidbody _canaryBody;

        private Vector3 _chestPos; private Quaternion _chestRot;
        private Bounds _bounds; private float _size;
        private int _strikesPlanned, _strikes;
        private float _nextStrike, _tension, _nextThump, _shake, _pickSwing;
        private bool _dynamitePlaced, _blasting, _strikeQueued;
        private int _steps;
        private float _lastEvent = -999f, _lastTail = -999f;
        private int _tailCount, _pebbleMade, _pebbleCap = 6;
        private bool _poseSaved;

        /// <summary>
        /// API cũ: trước đây nơi bấm phải gọi hàm này. Nay kịch bản tự chạy khi bấm (ChestScript_Miner), nên chỉ còn gọi
        /// thẳng reveal.Play (bậc chuyển nguyên vẹn).
        /// </summary>
        [System.Obsolete("Kịch bản tự chạy khi bấm: gọi reveal.Play(tier, item) là đủ.")]
        public static bool TryBegin(ChestRevealSequence reveal, ChestTier rolledTier, GameObject itemSource)
        {
            if (reveal == null) return false;
            MinerDigSequence dig = reveal.GetComponent<MinerDigSequence>();
            if (dig == null || !dig.isActiveAndEnabled) return false;
            reveal.Play(rolledTier, itemSource);
            return true;
        }

        public bool IsDigging => _phase == Phase.Digging;

        /// <summary>Cue "dao_mo" của ChestScript_Miner: đào + nổ, xong coroutine thì bước OpenBox mở nắp.</summary>
        public override IEnumerator OnCue(string cue, ChestScriptContext context)
        {
            if (cue != "dao_mo" || context == null) return null;
            return DigRoutine(context);
        }

        private IEnumerator DigRoutine(ChestScriptContext ctx)
        {
            _reveal = ctx.Reveal;
            if (_phase != Phase.Idle) Cleanup();

            _tier = ctx.Tier;   // chỉ đọc; dùng ở OnTierLocked như cũ (bậc thật lấy từ tham số của hook)
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : ctx.Seed);
            _chestPos = transform.position;
            _chestRot = transform.rotation;
            _poseSaved = true;
            MeasureChest();
            Build();

            _strikesPlanned = _rng.Next(strikesMin, strikesMax + 1);
            _strikes = 0;
            _tension = 0f;
            _steps = 0;
            _tailCount = 0; _pebbleMade = 0; _pebbleCap = 6;
            _lastEvent = _lastTail = -999f;
            _nextStrike = Time.time + 0.4f;
            _nextThump = Time.time + 0.3f;
            _dynamitePlaced = _blasting = _strikeQueued = false;
            _phase = Phase.Digging;

            // NỀN vào ngay khi bấm (≤ 0,2 s); nhịp "thình" chen vào sau 0,3 s
            if (_loop != null && rumbleClip != null)
            {
                _loop.clip = rumbleClip; _loop.loop = true; _loop.pitch = 1f; _loop.volume = Lv(0.15f, ChestAudioRule.BedMax); _loop.Play();
            }
            StartCoroutine(CanaryArrives());

            // đợi đá sạch (cuốc tự đập, thuốc nổ tự cắm) rồi trả rương về tư thế và giao cho bước OpenBox
            while (_phase == Phase.Digging && (_blasting || _attached.Count > 0)) yield return null;
            if (_phase != Phase.Digging) yield break;
            _phase = Phase.Revealing;
            transform.SetPositionAndRotation(_chestPos, _chestRot);
            yield return new WaitForSeconds(0.2f);
            if (_pick != null) _pick.gameObject.SetActive(false);
        }

        /// <summary>Một nhát cuốc (người chơi gọi; tự đập cũng đi qua đây).</summary>
        public void Strike()
        {
            if (_phase != Phase.Digging || _blasting) return;
            _strikeQueued = true;
        }

        /// <summary>Cắm thuốc nổ vào khối đá gần <paramref name="where"/> nhất (null = phía trước rương).</summary>
        public bool PlaceDynamite(Vector3? where = null)
        {
            if (_phase != Phase.Digging || _dynamitePlaced || _attached.Count == 0) return false;
            _dynamitePlaced = true;
            Vector3 target = where ?? (_bounds.center + transform.forward * _size * 0.6f);
            Rigidbody best = _attached[0];
            float bd = float.MaxValue;
            foreach (Rigidbody rb in _attached)
            {
                float dd = (rb.transform.position - target).sqrMagnitude;
                if (dd < bd) { bd = dd; best = rb; }
            }
            StartCoroutine(DynamiteRoutine(best.transform));
            return true;
        }

        // ------------------------------------------------------------------ ChestFxHook (gọi bởi ChestRevealSequence)

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (_phase != Phase.Revealing) return;
            _steps++;
            _tension = Mathf.Min(1f, _tension + 0.04f);   // dâng đều, KHÔNG phụ thuộc màu đang hiện (giữ bí mật)
            if (_steps % 4 == 0) DropPebbles(1);          // nhịp "thình" đã có ở Update: không thêm tiếng ở đây
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_phase != Phase.Revealing) return;   // đào chưa xong (không xảy ra: OpenBox đợi cue) thì bỏ qua
            _phase = Phase.Done;
            StartCoroutine(Finale(tier, color));
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            Cleanup();
        }

        private void OnDestroy() => Cleanup();

        // ------------------------------------------------------------------ Vòng lặp

        private void Update()
        {
            if (_phase == Phase.Idle) return;

            if (_phase == Phase.Digging)
            {
                if (playerInput) ReadInput();
                _tension = Mathf.Min(0.55f, _tension + Time.deltaTime * 0.06f);
                if (!_blasting && (_strikeQueued || Time.time >= _nextStrike) && _attached.Count > 0)
                {
                    _strikeQueued = false;
                    DoStrike();
                    _nextStrike = Time.time + autoStrikeInterval * Mathf.Lerp(1f, 0.7f, _tension);
                }
            }

            // độ căng → NỀN dâng (volume 0,15→0,45, cao độ +45%) + NHỊP dày dần (0,7 s → 0,15 s, volume 0,25→0,55)
            if (_phase != Phase.Done && _loop != null)
            {
                _loop.pitch = Mathf.Lerp(1f, 1.45f, _tension);
                _loop.volume = Lv(Mathf.Lerp(0.15f, 0.45f, _tension), ChestAudioRule.BedMax);
                // lúc ngòi xèo (_blasting) tạm không phát nhịp: tối đa 2 lớp kéo dài (nền + ngòi)
                if (!_blasting && Time.time >= _nextThump)
                {
                    OneShot(thumpClip, Lv(Mathf.Lerp(0.25f, 0.55f, _tension), ChestAudioRule.BeatMax));
                    _nextThump = Time.time + Mathf.Lerp(0.7f, 0.15f, _tension);
                }
            }

            // rung rương (chỉ khi đang đào: lúc mở nắp BoxOpenSequence điều khiển rương)
            if (_phase == Phase.Digging && !_hopping)
            {
                _shake = Mathf.Max(0f, _shake - Time.deltaTime * 6f);
                float a = _shake * _size * 0.025f;
                transform.position = _chestPos + new Vector3(Noise(31f, 0f), Noise(29f, 5f) * 0.4f, Noise(37f, 9f)) * a;
                transform.rotation = _chestRot * Quaternion.Euler(Noise(23f, 3f) * _shake * 2f, 0f, Noise(27f, 7f) * _shake * 2f);
            }

            // cuốc vung
            if (_pick != null)
            {
                _pickSwing = Mathf.Max(0f, _pickSwing - Time.deltaTime * 4f);
                float ang = Mathf.Lerp(-20f, 75f, _pickSwing);
                _pick.localRotation = Quaternion.Euler(ang, 0f, 0f);
            }

            // chim run theo độ căng
            if (_canary != null && _canaryBody != null && _canaryBody.isKinematic && _canaryPerched)
                _canary.position = _canaryPerch + Vector3.up * Mathf.Abs(Noise(18f, 2f)) * 0.01f * (1f + 3f * _tension);

            // tia lửa ngòi nổ nhấp nháy
            if (_flash != null) _flash.intensity = Mathf.Max(0f, _flash.intensity - Time.deltaTime * 18f);
        }

        private void ReadInput()
        {
#if ENABLE_INPUT_SYSTEM
            bool strike = (Keyboard.current != null && Keyboard.current[Key.F].wasPressedThisFrame)
                          || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
            bool tnt = Keyboard.current != null && Keyboard.current[Key.G].wasPressedThisFrame;
#else
            bool strike = Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0);
            bool tnt = Input.GetKeyDown(KeyCode.G);
#endif
            if (strike) Strike();
            if (tnt) PlaceDynamite();
        }

        // ------------------------------------------------------------------ Đào

        private void DoStrike()
        {
            _strikes++;
            _pickSwing = 1f;
            _shake = 1f;
            _tension = Mathf.Min(0.55f, _tension + 0.05f);
            bool last = _strikes >= _strikesPlanned;
            Event(last ? crumbleClip : pickClip, 0.45f, false);   // 1 sự kiện mỗi nhát (qua cổng 0,2 s)
            Flash(new Color(1f, 0.7f, 0.35f), 1.2f, _pick != null ? _pick.position : _bounds.center);

            int n = last ? _attached.Count : Mathf.Max(2, _attached.Count / (_strikesPlanned - _strikes + 1));
            Vector3 from = _pick != null ? _pick.position : _bounds.center + transform.forward * _size;
            for (int i = 0; i < n && _attached.Count > 0; i++) Release(Nearest(from), from, strikeImpulse);
            DropPebbles(1);

            if (autoDynamite && !_dynamitePlaced && _strikes >= _strikesPlanned - 1 && _attached.Count > 0) PlaceDynamite();
        }

        private IEnumerator DynamiteRoutine(Transform onRock)
        {
            _blasting = true;
            // que thuốc nổ đỏ + ngòi
            GameObject stick = Prim(PrimitiveType.Cylinder, "Dynamite", new Color(0.75f, 0.1f, 0.08f), false);
            _dynamite = stick.transform;
            _dynamite.localScale = new Vector3(0.05f, 0.09f, 0.05f) * (_size / 0.5f);
            Vector3 outward = (onRock.position - _bounds.center).normalized;
            _dynamite.position = onRock.position + outward * _size * 0.08f;
            _dynamite.rotation = Quaternion.LookRotation(outward) * Quaternion.Euler(90f, 0f, 0f);
            _dynamite.SetParent(onRock, true);

            GameObject spark = Prim(PrimitiveType.Sphere, "FuseSpark", new Color(1f, 0.85f, 0.4f), true);
            _fuseSpark = spark.transform;
            _fuseSpark.localScale = Vector3.one * _size * 0.03f;
            Event(fuseClip, 0.4f, true);   // ngòi xèo: tiếng lẻ phát một lần (nhịp tạm nghỉ trong lúc này)
            float t = 0f;
            Vector3 top = _dynamite.position + _dynamite.up * _size * 0.22f;
            while (t < fuseSeconds)
            {
                t += Time.deltaTime;
                float k = t / fuseSeconds;
                if (_dynamite == null) break;
                Vector3 tip = _dynamite.position + _dynamite.up * _size * 0.1f;
                _fuseSpark.position = Vector3.Lerp(top, tip, k) + RandIn() * _size * 0.006f;
                _fuseSpark.localScale = Vector3.one * _size * (0.02f + 0.02f * Mathf.PerlinNoise(Time.time * 30f, 0f));
                _tension = Mathf.Min(0.55f, _tension + Time.deltaTime * 0.08f);
                if (_rng.NextDouble() < 0.15) Flash(new Color(1f, 0.6f, 0.2f), 0.6f, _fuseSpark.position);
                yield return null;
            }

            // NỔ: mọi khối đá còn lại văng, rương nảy
            Vector3 c = _dynamite != null ? _dynamite.position : _bounds.center;
            Flash(new Color(1f, 0.75f, 0.4f), 6f, c);
            Event(boomClip, 0.5f, true);   // nổ thuốc nổ: trần sự kiện 0,5 (trước chốt)
            if (_fuseSpark != null) Destroy(_fuseSpark.gameObject);
            if (_dynamite != null) Destroy(_dynamite.gameObject);
            while (_attached.Count > 0) Release(_attached[_attached.Count - 1], c, blastImpulse);
            DropPebbles(1);
            yield return Hop();
            _blasting = false;
        }

        private bool _hopping;

        private IEnumerator Hop()
        {
            _hopping = true;
            float t = 0f, dur = 0.55f;
            float tilt = (float)(_rng.NextDouble() * 2.0 - 1.0) * 12f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                float y = 4f * k * (1f - k) * hopHeight * (_size / 0.5f);
                transform.position = _chestPos + Vector3.up * y;
                transform.rotation = _chestRot * Quaternion.Euler(0f, 0f, tilt * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            transform.SetPositionAndRotation(_chestPos, _chestRot);
            _shake = 1f;
            _hopping = false;
        }

        // ------------------------------------------------------------------ Kết: chỉ ở đây kết quả mới lộ ra

        private IEnumerator Finale(ChestTier tier, Color color)
        {
            // Chốt: nhịp đã dừng (phase Done). Mỗi bậc đúng 1 tiếng chốt (TC) do hook phát; khác nhau chỉ ở bậc.
            _tailCount = 0; _pebbleMade = 0;
            if (tier == ChestTier.Rac)
            {
                // XỊT: nền tắt phụt (không fade) → lặng 0,2 s → 1 tiếng xịt; 0,6 s sau đó không phát gì khác
                _pebbleCap = 1;
                if (_loop != null) _loop.Stop();
                yield return new WaitForSeconds(ChestAudioRule.RacSilence);
                Sting(0, 0.8f);
                CanaryFaint();
                yield return new WaitForSeconds(0.8f);
                Tail(faintClip, 0.35f);
                Pebble(_bounds.center + Vector3.up * _size * 1.4f, 0.12f);
                yield return new WaitForSeconds(0.35f);
                Tail(pebbleClip, 0.4f);
                yield break;
            }

            if (tier == ChestTier.Thuong)
            {
                _pebbleCap = 0;
                Sting(1, 0.7f);            // TC ≤ 0,7; chim KHÔNG ngất, vẫn đậu
                yield return FadeLoop(0.3f);
                yield break;
            }

            if (tier == ChestTier.Hiem)
            {
                _pebbleCap = 0;
                if (_loop != null) _loop.Stop();
                Sting(2, 0.85f);
                StartCoroutine(CanaryFly(color, 1));
                yield break;
            }

            // Rất hiếm / Huyền thoại: nền dừng, 1 tiếng chốt (đã chứa tiếng nổ nên KHÔNG thêm tiếng nổ nữa)
            if (_loop != null) _loop.Stop();
            bool legend = tier == ChestTier.HuyenThoai;
            _pebbleCap = legend ? 6 : 4;
            Sting(legend ? 4 : 3, 1f);
            Flash(color, 8f, _reveal.MouthPosition);
            DropPebbles(_pebbleCap);
            StartCoroutine(CanaryFly(color, legend ? 3 : 2));
            if (legend) StartCoroutine(Nuggets(color));
            yield break;
        }

        private IEnumerator FadeLoop(float seconds)
        {
            if (_loop == null) yield break;
            float v0 = _loop.volume, t = 0f;
            while (t < seconds && _loop != null)
            {
                t += Time.deltaTime;
                _loop.volume = v0 * (1f - t / seconds);
                yield return null;
            }
            if (_loop != null) _loop.Stop();
        }

        private IEnumerator Nuggets(Color color)
        {
            yield return new WaitForSeconds(0.4f);   // đuôi bắt đầu từ +0,4 s
            _nuggetMat = NewMat(Color.Lerp(new Color(1f, 0.78f, 0.2f), color, 0.35f));
            for (int i = 0; i < 10; i++)
            {
                if (_root == null) yield break;
                GameObject n = GameObject.CreatePrimitive(PrimitiveType.Cube);
                n.name = "GoldNugget";
                n.transform.SetParent(_root, true);
                n.transform.position = _reveal.MouthPosition + RandIn() * _size * 0.08f;
                n.transform.rotation = RandRot();
                n.transform.localScale = Vector3.one * _size * (0.04f + 0.03f * (float)_rng.NextDouble());
                n.GetComponent<Renderer>().sharedMaterial = _nuggetMat;
                Rigidbody rb = n.AddComponent<Rigidbody>();
                rb.mass = 0.2f;
                Vector3 dir = (Vector3.up * 2.2f + RandIn()).normalized;
                rb.AddForce(dir * 1.1f * (_size / 0.5f), ForceMode.Impulse);
                rb.angularVelocity = RandIn() * 10f;
                StartCoroutine(ShrinkAway(n.transform, 2.2f));
                if (i % 2 == 0) Tail(nuggetClip, 0.35f);   // đuôi: ≤ 6 tiếng, cách ≥ 0,15 s
                yield return new WaitForSeconds(0.08f);
            }
        }

        // ------------------------------------------------------------------ Chim hoàng yến (sự kiện phụ)

        private Vector3 _canaryPerch;
        private bool _canaryPerched;

        private IEnumerator CanaryArrives()
        {
            yield return new WaitForSeconds(0.9f);
            if (_canary == null) yield break;
            Vector3 side = transform.right * _size * 0.75f;
            _canaryPerch = new Vector3(_bounds.center.x, _bounds.min.y, _bounds.center.z) + side + Vector3.up * _size * 0.12f;
            Vector3 start = _canaryPerch + side * 1.5f + Vector3.up * _size * 1.0f;
            _canary.gameObject.SetActive(true);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.6f;
                if (_canary == null) yield break;
                _canary.position = Vector3.Lerp(start, _canaryPerch, Mathf.SmoothStep(0f, 1f, t)) + Vector3.up * Mathf.Sin(t * 30f) * 0.01f;
                yield return null;
            }
            _canaryPerched = true;
            Event(chirpClip, 0.35f, false);
        }

        private void CanaryFaint()
        {
            // chỉ hình: tiếng "ngất" là đuôi do Finale phát sau 0,8 s
            if (_canary == null || _canaryBody == null) return;
            _canaryPerched = false;
            _canaryBody.isKinematic = false;
            _canaryBody.useGravity = true;
            _canaryBody.AddForce((transform.right + Vector3.up * 0.5f) * 0.05f, ForceMode.Impulse);
            _canaryBody.angularVelocity = transform.forward * 6f;
        }

        private IEnumerator CanaryFly(Color color, int loops)
        {
            if (_canary == null) yield break;
            _canaryPerched = false;
            if (_canaryMat != null) _canaryMat.color = Color.Lerp(_canaryMat.color, color, 0.4f);
            Vector3 c = _reveal.MouthPosition;
            float t = 0f, dur = 1.2f * loops, nextChirp = 0.4f;
            while (t < dur && _canary != null)
            {
                t += Time.deltaTime;
                float a = t / 1.2f * Mathf.PI * 2f;
                float r = _size * 0.8f;
                _canary.position = c + new Vector3(Mathf.Cos(a) * r, _size * (0.3f + 0.6f * t / dur), Mathf.Sin(a) * r);
                if (t >= nextChirp) { Tail(chirpClip, 0.35f); nextChirp += 0.6f; }   // đuôi từ +0,4 s, thưa
                yield return null;
            }
        }

        // ------------------------------------------------------------------ Dựng / dọn

        private void MeasureChest()
        {
            Renderer[] rs = GetComponentsInChildren<Renderer>();
            bool has = false;
            Bounds b = new Bounds(transform.position + Vector3.up * 0.2f, Vector3.one * 0.4f);
            foreach (Renderer r in rs)
            {
                if (r == null || r is ParticleSystemRenderer) continue;
                if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
            }
            _bounds = b;
            _size = Mathf.Max(0.1f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)));
        }

        private void Build()
        {
            _root = new GameObject("MinerDig_" + name).transform;
            _root.position = _bounds.center;
            _root.gameObject.AddComponent<MinerDigLink>().owner = this;

            _rockMat = NewMat(rockColor);
            _fuseMat = null;

            // sàn tạm để đá rơi có chỗ nằm (vô hình)
            GameObject floor = new GameObject("MinerFloor");
            floor.transform.SetParent(_root, false);
            floor.transform.position = new Vector3(_bounds.center.x, _bounds.min.y - 0.025f, _bounds.center.z);
            BoxCollider fc = floor.AddComponent<BoxCollider>();
            fc.size = new Vector3(_size * 8f, 0.05f, _size * 8f);

            // vỏ đá: các khối rải trên mặt một elip bao rương, trừ đáy
            _attached.Clear();
            Vector3 ext = _bounds.extents + Vector3.one * _size * 0.08f;
            for (int i = 0; i < chunkCount; i++)
            {
                // phân bố Fibonacci trên nửa cầu trên (+ dải hông), xoay theo hạt giống
                float k = (i + 0.5f) / chunkCount;
                float y = Mathf.Lerp(-0.35f, 1f, k);
                float r = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
                float phi = i * 2.39996f + (float)_rng.NextDouble() * 0.4f;
                Vector3 dir = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
                Vector3 pos = _bounds.center + Vector3.Scale(dir, ext);
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.name = "RockChunk";
                g.transform.SetParent(_root, true);
                g.transform.position = pos;
                g.transform.rotation = Quaternion.Euler(_rng.Next(360), _rng.Next(360), _rng.Next(360));
                float s = _size * chunkScale * (0.7f + 0.6f * (float)_rng.NextDouble());
                g.transform.localScale = new Vector3(s, s * (0.6f + 0.4f * (float)_rng.NextDouble()), s * (0.8f + 0.3f * (float)_rng.NextDouble()));
                g.GetComponent<Renderer>().sharedMaterial = _rockMat;
                Rigidbody rb = g.AddComponent<Rigidbody>();
                rb.mass = chunkMass;
                rb.isKinematic = true;
                _attached.Add(rb);
            }

            // cuốc: cán + lưỡi, treo trước-trên rương
            _pick = new GameObject("Pickaxe").transform;
            _pick.SetParent(_root, false);
            _pick.position = _bounds.center + transform.forward * _size * 0.9f + Vector3.up * _size * 0.35f;
            _pick.rotation = transform.rotation;
            GameObject handle = Prim(PrimitiveType.Cylinder, "Handle", new Color(0.45f, 0.3f, 0.16f), false);
            handle.transform.SetParent(_pick, false);
            handle.transform.localScale = new Vector3(0.025f, 0.22f, 0.025f) * (_size / 0.5f);
            handle.transform.localPosition = Vector3.up * 0.2f * (_size / 0.5f);
            GameObject head = Prim(PrimitiveType.Cube, "Head", new Color(0.55f, 0.56f, 0.6f), false);
            head.transform.SetParent(_pick, false);
            head.transform.localScale = new Vector3(0.04f, 0.04f, 0.3f) * (_size / 0.5f);
            head.transform.localPosition = Vector3.up * 0.42f * (_size / 0.5f);

            // chim hoàng yến (ẩn tới khi bay vào)
            _canaryMat = NewMat(new Color(1f, 0.85f, 0.15f));
            GameObject bird = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bird.name = "Canary";
            bird.transform.SetParent(_root, true);
            bird.transform.localScale = new Vector3(0.6f, 0.5f, 0.8f) * _size * 0.12f;
            bird.GetComponent<Renderer>().sharedMaterial = _canaryMat;
            _canary = bird.transform;
            _canaryBody = bird.AddComponent<Rigidbody>();
            _canaryBody.isKinematic = true;
            _canaryBody.mass = 0.05f;
            bird.SetActive(false);
            _canaryPerched = false;

            // đèn chớp (tia lửa cuốc, nổ)
            GameObject lg = new GameObject("MinerFlash");
            lg.transform.SetParent(_root, false);
            _flash = lg.AddComponent<Light>();
            _flash.type = LightType.Point;
            _flash.range = _size * 4f;
            _flash.shadows = LightShadows.None;
            _flash.intensity = 0f;

            _loop = _root.gameObject.AddComponent<AudioSource>();
            _oneShot = _root.gameObject.AddComponent<AudioSource>();
            foreach (AudioSource a in new[] { _loop, _oneShot }) { a.playOnAwake = false; a.spatialBlend = 0f; }
        }

        private void Cleanup()
        {
            StopAllCoroutines();
            if (_poseSaved) transform.SetPositionAndRotation(_chestPos, _chestRot);   // trả rương về đúng chỗ cũ
            _poseSaved = false;
            _hopping = false;
            if (_root != null) Destroy(_root.gameObject);
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
            _attached.Clear();
            _root = null; _pick = _dynamite = _fuseSpark = _canary = null; _canaryBody = null; _flash = null;
            _loop = _oneShot = null;
            _rockMat = _fuseMat = _canaryMat = _nuggetMat = null;
            _phase = Phase.Idle;
        }

        // ------------------------------------------------------------------ tiện ích

        private Rigidbody Nearest(Vector3 p)
        {
            Rigidbody best = _attached[0];
            float bd = float.MaxValue;
            foreach (Rigidbody rb in _attached)
            {
                float d = (rb.transform.position - p).sqrMagnitude;
                if (d < bd) { bd = d; best = rb; }
            }
            return best;
        }

        private void Release(Rigidbody rb, Vector3 from, float impulse)
        {
            _attached.Remove(rb);
            if (rb == null) return;
            rb.isKinematic = false;
            rb.useGravity = true;
            Vector3 dir = (rb.transform.position - from);
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.up;
            dir = (dir.normalized + Vector3.up * 0.6f + RandIn() * 0.3f).normalized;
            rb.AddForce(dir * impulse * rb.mass * (_size / 0.5f), ForceMode.Impulse);
            rb.angularVelocity = RandIn() * 8f;
            StartCoroutine(ShrinkAway(rb.transform, debrisLifetime + 0.5f * (float)_rng.NextDouble()));
        }

        private void DropPebbles(int n)
        {
            n = Mathf.Min(n, _pebbleCap - _pebbleMade);   // trần cứng (T3): trước chốt ≤ 6 sỏi, sau chốt theo bậc
            for (int i = 0; i < n; i++)
            {
                _pebbleMade++;
                Vector3 p = _bounds.center + new Vector3((float)(_rng.NextDouble() * 2 - 1), 0f, (float)(_rng.NextDouble() * 2 - 1)) * _size
                            + Vector3.up * _size * (1.6f + (float)_rng.NextDouble());
                Pebble(p, 0.04f + 0.04f * (float)_rng.NextDouble());
            }
        }

        private Rigidbody Pebble(Vector3 pos, float relSize)
        {
            if (_root == null) return null;
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = "Pebble";
            g.transform.SetParent(_root, true);
            g.transform.position = pos;
            g.transform.rotation = RandRot();
            g.transform.localScale = Vector3.one * _size * relSize;
            g.GetComponent<Renderer>().sharedMaterial = _rockMat;
            Rigidbody rb = g.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            StartCoroutine(ShrinkAway(g.transform, debrisLifetime));
            return rb;
        }

        private IEnumerator ShrinkAway(Transform t, float after)
        {
            yield return new WaitForSeconds(after);
            if (t == null) yield break;
            Vector3 s0 = t.localScale;
            float k = 0f;
            while (k < 1f && t != null)
            {
                k += Time.deltaTime / 0.4f;
                t.localScale = s0 * (1f - k);
                yield return null;
            }
            if (t != null) Destroy(t.gameObject);
        }

        private GameObject Prim(PrimitiveType type, string n, Color c, bool unlit)
        {
            GameObject g = GameObject.CreatePrimitive(type);
            g.name = n;
            Collider col = g.GetComponent<Collider>();
            if (col != null) Destroy(col);
            g.transform.SetParent(_root, true);
            Material m = unlit ? ChestFxFactory.CreateUnlit(c, false, n) : NewMat(c);
            if (unlit) _owned.Add(m);
            g.GetComponent<Renderer>().sharedMaterial = m;
            return g;
        }

        private Material NewMat(Color c)
        {
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Material m = new Material(tmp.GetComponent<Renderer>().sharedMaterial);   // vật liệu mặc định có ánh sáng của pipeline
            Destroy(tmp);
            m.color = c;
            _owned.Add(m);
            return m;
        }

        private void Flash(Color c, float intensity, Vector3 at)
        {
            if (_flash == null) return;
            _flash.transform.position = at;
            _flash.color = c;
            _flash.intensity = Mathf.Max(_flash.intensity, intensity);
        }

        /// <summary>Mức âm cuối = mức theo quy tắc (tính ở volume mặc định 0,8) × volume/0,8, không vượt trần. Nhân volume đúng MỘT lần.</summary>
        private float Lv(float level, float cap) => Mathf.Min(cap, level * volume / BaseVolume);

        /// <summary>Phát một tiếng lẻ trên nguồn riêng (pitch luôn 1: không đổi pitch nguồn đang ngân).</summary>
        private void OneShot(AudioClip clip, float finalVolume)
        {
            if (_oneShot == null || clip == null) return;
            _oneShot.PlayOneShot(clip, finalVolume);
        }

        /// <summary>Sự kiện trước chốt: tối đa 1 tiếng mỗi 0,2 s (cổng chung), trần 0,5. <paramref name="force"/> bỏ qua cổng nhưng vẫn ghi mốc.</summary>
        private void Event(AudioClip clip, float level, bool force)
        {
            if (clip == null) return;
            if (force) _lastEvent = Time.time;
            else if (!ChestAudioRule.TryGate(ref _lastEvent, ChestAudioRule.EventGap)) return;
            OneShot(clip, Lv(level, ChestAudioRule.EventMax));
        }

        /// <summary>Tiếng đuôi sau chốt: cách ≥ 0,15 s, trần 0,4, tổng ≤ 6.</summary>
        private void Tail(AudioClip clip, float level)
        {
            if (clip == null || _tailCount >= 6) return;
            if (!ChestAudioRule.TryGate(ref _lastTail, ChestAudioRule.TailGap)) return;
            _tailCount++;
            OneShot(clip, Lv(level, ChestAudioRule.TailMax));
        }

        /// <summary>Tiếng chốt (TC) theo bậc: đúng 1 clip, trần theo A4/A6.</summary>
        private void Sting(int tierIndex, float level)
        {
            if (stingClips == null || tierIndex < 0 || tierIndex >= stingClips.Length) return;
            OneShot(stingClips[tierIndex], Lv(level, ChestAudioRule.StingMax));
        }

        // ngẫu nhiên trang trí luôn lấy từ _rng (theo hạt giống), không dùng UnityEngine.Random
        private Vector3 RandIn()
        {
            Vector3 v = new Vector3((float)(_rng.NextDouble() * 2 - 1), (float)(_rng.NextDouble() * 2 - 1), (float)(_rng.NextDouble() * 2 - 1));
            return v.sqrMagnitude > 1f ? v.normalized : v;
        }

        private Quaternion RandRot() => Quaternion.Euler(_rng.Next(360), _rng.Next(360), _rng.Next(360));

        private static float Noise(float speed, float seed) => Mathf.PerlinNoise(Time.time * speed, seed) * 2f - 1f;
    }

    /// <summary>Gắn trên gốc các vật thể đào (đá, chim…) để vật thả vào tìm được kịch bản đang chạy.</summary>
    public class MinerDigLink : MonoBehaviour
    {
        public MinerDigSequence owner;
    }

    /// <summary>
    /// Thuốc nổ người chơi THẢ/NÉM vào đá (tương tác phụ): đặt component này lên bất kỳ vật có Collider + Rigidbody.
    /// Chạm khối đá đang vùi rương → kịch bản cắm thuốc nổ ở đó và vật này biến mất.
    /// </summary>
    public class MinerDynamite : MonoBehaviour
    {
        private void OnCollisionEnter(Collision c)
        {
            if (c == null || c.collider == null) return;
            MinerDigLink link = c.collider.GetComponentInParent<MinerDigLink>();
            if (link == null || link.owner == null) return;
            if (link.owner.PlaceDynamite(c.collider.transform.position)) Destroy(gameObject);
        }
    }
}

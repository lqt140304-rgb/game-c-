using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Kịch bản riêng Chest_Magic: TRẬN PHÁP RUNE. Gắn lên gốc rương (cạnh ChestRevealSequence), thay cho MagicHook.
    ///
    /// Nhịp (giây tính từ lúc bấm mở = lúc ChestRevealSequence.Play chạy, phát hiện qua IsPlaying):
    ///   0.0  âm "tụ phép" bắt đầu ngay; rương rung nhẹ; ba viên đá rune (Rigidbody thật) rơi lần lượt xuống ba điểm
    ///        quanh rương, nảy trên sàn rồi bị lực lò xo kéo trượt vào đúng điểm (đá người chơi đã đặt sẵn thì dùng luôn).
    ///   ~0.5 đốm linh hồn (wisp) sà xuống, bay chạm từng viên: viên được chạm sáng lên, nối tia sáng thành tam giác.
    ///   ~2.2 đủ ba viên: vòng trận sáng, rương nhấc khỏi sàn, bay lên, xoay theo vòng; vòng co dần về rương,
    ///        xoay nhanh dần; đá rung lắc mạnh dần. Giống hệt nhau cho mọi bậc (giữ bí mật).
    ///   khóa bậc (OnTierLocked, do lõi quyết): âm tụ phép dừng, phát âm kết theo bậc, rồi:
    ///     Rác      : vòng tắt phụt, rương rơi bịch xuống (nảy một cái, nghiêng lắc), đá đổ nhào, wisp hắt xì rồi rớt
    ///                xuống sàn như vật thường, một làn khói xám. (xịt, hài)
    ///     Thường   : vòng thu vào rương, rương hạ êm, đá chớp sáng.
    ///     Hiếm     : như Thường + đá nảy lên một cái.
    ///     Rất hiếm : rương vọt cao thêm rồi DẬP xuống sàn: sóng xung kích hất văng đá (lực nổ thật), đèn chớp.
    ///     Huyền thoại: như Rất hiếm nhưng mạnh hơn + cột sáng màu bậc + wisp xoắn ốc bay lên theo cột.
    ///   Rương luôn về đúng chỗ cũ trong ~0.6 s sau khóa để bóng vật phẩm và nắp chạy bình thường.
    /// Đã thu gọn theo kế hoạch 2026-11-25 (mục 5.4): cột sáng thấp hơn, ít hạt hơn, wisp không có đèn riêng,
    /// rương chỉ cộng độ lệch lên vị trí BoxOpenSequence ghi, âm là .mp3 ElevenLabs phát theo quy tắc A1–A8.
    /// Không bao giờ đổi bậc; ngẫu nhiên (góc đá, độ nảy) theo hạt giống <see cref="decorationSeed"/>.
    /// </summary>
    public class MagicRuneRitual : ChestFxHook
    {
        [Header("Âm (để trống = im lặng; Tools/Cho Do La/Magic Rune/Setup On Selected Chest tự gán)")]
        [Tooltip("Nền lặp (trống + gõ đá bên trong), phát ngay lúc bấm.")]
        public AudioClip buildClip;
        public AudioClip stoneClip;
        public AudioClip wispClip;
        [Tooltip("HĐ: tiếng nổ/vỡ lúc dập xuống, chỉ Rất hiếm / Huyền thoại.")]
        public AudioClip slamClip;
        [Tooltip("TC: tiếng chốt theo bậc: 0 Rác .. 4 Huyền thoại (luôn do hook phát).")]
        public AudioClip[] stingClips = new AudioClip[5];
        [Range(0f, 1f)] public float volume = 0.8f;

        [Header("Trận")]
        [SerializeField] private float anchorRadius = 0.55f;
        [SerializeField] private float stoneSize = 0.1f;
        [SerializeField] private float dropHeight = 1.3f;
        [SerializeField] private float liftHeight = 0.28f;
        [Tooltip("Thời gian dự kiến từ lúc bấm tới lúc khóa bậc, dùng để chia nhịp độ căng.")]
        [SerializeField] private float expectedBuildSeconds = 6f;
        [SerializeField] private Color runeColor = new Color(0.55f, 0.35f, 1f);
        [Tooltip("0 = ngẫu nhiên mỗi lần. Chỉ ảnh hưởng trang trí.")]
        [SerializeField] private int decorationSeed;

        private enum Phase { Idle, Gather, Levitate, Ending, Done }

        private class Stone
        {
            public Transform t;
            public Rigidbody rb;
            public Material mat;
            public MagicRuneStone tag;
            public Vector3 anchor;
            public float dropAt;
            public bool dropped, seated, lit;
            public float seatedAt;
        }

        private ChestRevealSequence _reveal;
        private Phase _phase = Phase.Idle;
        private bool _wasPlaying;
        private System.Random _rng;
        private float _t0, _lockAt;
        private ChestTier _tier;
        private Color _tierColor;

        private Vector3 _restPos;
        private Quaternion _restRot;
        private float _floorY, _chestRadius;
        private float _spin, _spinSpeed, _lift, _liftVel;

        private readonly List<Stone> _stones = new List<Stone>();
        private readonly List<Rigidbody> _extras = new List<Rigidbody>();
        private readonly List<Object> _owned = new List<Object>();

        private Transform _root;
        private LineRenderer _ring, _tri;
        private Transform _wisp;
        private Rigidbody _wispRb;
        private int _wispTarget;
        private float _wispTouchAt;
        private Light _light;
        private AudioSource _buildSrc, _sfxSrc;
        private ParticleSystem _puff;
        private ChestFxProfile _puffStyle;
        private Transform _pillar;
        private Material _pillarMat;
        private float _ringRadius;
        private float _size = 0.5f;      // S: cạnh lớn nhất khung bao rương
        private float _slamK = -1f;
        private float _lastEvt = -999f;  // mốc cổng sự kiện âm (A3)
        private float _stingAt = -1f;    // Rác: giờ phát tiếng xịt sau 0,2 s lặng
        private AudioClip _stingClip;
        // Gốc rương = giá trị BoxOpenSequence vừa ghi nếu khác giá trị hook ghi khung trước (chỉ cộng độ lệch).
        private Vector3 _basePos, _lastPos;
        private Quaternion _baseRot, _lastRot, _wantRot;
        private bool _hasWritten;

        // ------------------------------------------------------------------ Hook

        private void Update()
        {
            if (_reveal == null) _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null) return;

            // Bấm mở = Play() bật IsPlaying ngay, trước khi nắp hay vòng màu chạy.
            bool playing = _reveal.IsPlaying;
            if (playing && !_wasPlaying && _phase == Phase.Idle) Begin(_reveal);
            _wasPlaying = playing;

            if (_phase == Phase.Idle) return;
            float dt = Time.deltaTime;
            float now = Time.time - _t0;
            UpdateStones(now);
            UpdateWisp(now, dt);
            UpdateRing(now, dt);
            UpdateAudio(now);
        }

        private void LateUpdate()
        {
            if (_phase == Phase.Idle || _reveal == null) return;
            UpdateChest(Time.time - _t0, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_phase != Phase.Gather && _phase != Phase.Levitate) return;
            // Lò xo kéo đá đã chạm sàn trượt vào đúng điểm trận (lực thật, đá vẫn va chạm được).
            foreach (Stone s in _stones)
            {
                if (!s.dropped || s.seated || s.rb == null || s.rb.isKinematic) continue;
                Vector3 to = s.anchor - s.t.position;
                to.y = 0f;
                s.rb.AddForce(to * 40f - Flat(s.rb.linearVelocity) * 4f, ForceMode.Force);
            }
            // Đá phụ người chơi ném vào: hút lên quỹ đạo quanh rương.
            Vector3 c = _restPos + Vector3.up * (_lift + 0.25f);
            foreach (Rigidbody rb in _extras)
            {
                if (rb == null) continue;
                Vector3 d = c - rb.transform.position;
                Vector3 tangent = Vector3.Cross(Vector3.up, d).normalized;
                rb.AddForce(d * 25f + tangent * 6f - rb.linearVelocity * 2f - Physics.gravity * rb.mass, ForceMode.Force);
            }
        }

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            // Dự phòng: nếu vì lý do nào đó Update chưa thấy bấm mở (vd. rương không có box), bắt đầu ở đây.
            if (_phase == Phase.Idle) Begin(reveal);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_phase == Phase.Idle) Begin(reveal);
            _tier = tier;
            _tierColor = color;
            _lockAt = Time.time - _t0;
            _phase = Phase.Ending;
            if (_buildSrc != null) _buildSrc.Stop(); // A4: nền dừng ngay, không fade
            PlaySting(tier);
            StartEnding(tier);
        }

        public override void OnRevealed(ChestRevealSequence reveal)
        {
            // Đá trận mờ dần trong lúc người chơi xem vật phẩm (Update lo phần co nhỏ).
            _phase = Phase.Done;
            foreach (Stone s in _stones) if (s.t != null && s.tag != null && s.tag.spawnedByRitual) Destroy(s.t.gameObject, 2.5f);
            if (_ring != null) Destroy(_ring.gameObject, 0.1f);
            if (_tri != null) Destroy(_tri.gameObject, 0.1f);
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        // ------------------------------------------------------------------ Bắt đầu

        private void Begin(ChestRevealSequence reveal)
        {
            Cleanup();
            _reveal = reveal;
            _phase = Phase.Gather;
            _t0 = Time.time;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);

            Transform chest = reveal.transform;
            _restPos = chest.position;
            _restRot = chest.rotation;
            MeasureChest(chest);

            _root = new GameObject("MagicRuneRitual_FX").transform;
            _owned.Add(_root.gameObject);

            _buildSrc = MakeSource("RuneBuildAudio");
            _sfxSrc = MakeSource("RuneSfxAudio");
            if (buildClip != null)
            {
                _buildSrc.clip = buildClip;
                _buildSrc.loop = true; // khóa muộn hơn độ dài clip vẫn không im giữa chừng
                _buildSrc.volume = ChestAudioRule.BedMax * 0.33f * volume; // 0,15 → 0,45 theo độ căng (UpdateAudio)
                _buildSrc.pitch = 1f;
                _buildSrc.Play();
            }
            _lastEvt = -999f;
            _stingAt = -1f;
            _stingClip = null;
            _slamK = -1f;
            _hasWritten = false;

            float r = Mathf.Max(anchorRadius, _chestRadius * 1.8f);
            _ringRadius = r * 1.15f;
            float a0 = (float)_rng.NextDouble() * 360f;
            for (int i = 0; i < 3; i++)
            {
                float a = (a0 + i * 120f) * Mathf.Deg2Rad;
                Vector3 anchor = new Vector3(_restPos.x + Mathf.Cos(a) * r, _floorY + stoneSize * 0.5f, _restPos.z + Mathf.Sin(a) * r);
                _stones.Add(ClaimOrSpawn(anchor, 0.15f + i * 0.35f));
            }

            _ring = MakeLine("RuneRing", 64, true, 0.018f);
            _tri = MakeLine("RuneLinks", 3, true, 0.01f);
            SetLineAlpha(_ring, 0f);
            SetLineAlpha(_tri, 0f);

            _light = new GameObject("RuneLight").AddComponent<Light>();
            _light.transform.SetParent(_root, false);
            _light.type = LightType.Point;
            _light.color = runeColor;
            _light.range = r * 4f;
            _light.intensity = 0f;
            _light.shadows = LightShadows.None;

            SpawnWisp();
        }

        private Stone ClaimOrSpawn(Vector3 anchor, float dropAt)
        {
            // Đá người chơi đặt sẵn gần điểm trận: dùng luôn, coi như đã rơi xong.
            foreach (MagicRuneStone m in MagicRuneStone.All)
            {
                if (m == null || m.claimedBy != null) continue;
                Vector3 p = m.transform.position;
                if (Vector3.Distance(Flat(p), Flat(anchor)) > 0.3f || Mathf.Abs(p.y - anchor.y) > 0.4f) continue;
                m.claimedBy = this;
                Renderer rr = m.GetComponentInChildren<Renderer>();
                Stone own = new Stone { t = m.transform, rb = m.GetComponent<Rigidbody>(), tag = m, anchor = anchor, dropAt = 0f, dropped = true };
                if (rr != null) { own.mat = rr.material; _owned.Add(own.mat); }
                return own;
            }

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "RuneStone";
            go.transform.localScale = new Vector3(stoneSize, stoneSize * 0.55f, stoneSize * 0.8f);
            go.transform.position = anchor + Vector3.up * dropHeight;
            go.transform.rotation = Quaternion.Euler((float)_rng.NextDouble() * 360f, (float)_rng.NextDouble() * 360f, 0f);
            Material mat = ChestFxFactory.CreateUnlit(new Color(0.18f, 0.16f, 0.24f), false, "RuneStoneMat");
            go.GetComponent<Renderer>().sharedMaterial = mat;
            _owned.Add(mat);
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.6f;
            rb.linearDamping = 0.2f;
            rb.angularDamping = 1.5f;
            rb.isKinematic = true; // chờ tới lượt rơi
            MagicRuneStone tag = go.AddComponent<MagicRuneStone>();
            tag.spawnedByRitual = true;
            tag.claimedBy = this;
            _owned.Add(go);
            return new Stone { t = go.transform, rb = rb, mat = mat, tag = tag, anchor = anchor, dropAt = dropAt };
        }

        private void SpawnWisp()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "RuneWisp";
            Collider col = go.GetComponent<Collider>();
            if (col != null) col.enabled = false; // chỉ bật va chạm khi nó rớt (Rác)
            go.transform.localScale = Vector3.one * 0.05f;
            go.transform.position = _restPos + new Vector3(0f, 1.6f, 0f);
            Material m = ChestFxFactory.CreateUnlit(Color.Lerp(runeColor, Color.white, 0.6f), false, "RuneWispMat");
            go.GetComponent<Renderer>().sharedMaterial = m;
            _owned.Add(m);
            _owned.Add(go);
            TrailRenderer tr = go.AddComponent<TrailRenderer>();
            tr.time = 0.35f;
            tr.startWidth = 0.03f;
            tr.endWidth = 0f;
            Material tm = ChestFxFactory.CreateUnlit(runeColor, false, "RuneWispTrail");
            tr.sharedMaterial = tm;
            _owned.Add(tm);
            _wisp = go.transform;
            _wispTarget = 0;
            _wispTouchAt = -1f;
        }

        // ------------------------------------------------------------------ Cập nhật

        private void UpdateStones(float now)
        {
            int lit = 0;
            foreach (Stone s in _stones)
            {
                if (s.t == null) continue;
                if (!s.dropped && now >= s.dropAt && _phase == Phase.Gather)
                {
                    s.dropped = true;
                    s.rb.isKinematic = false;
                    s.rb.angularVelocity = RandVec() * 6f;
                }
                if (s.dropped && !s.seated && (_phase == Phase.Gather || _phase == Phase.Levitate))
                {
                    bool landed = s.t.position.y <= _floorY + stoneSize * 1.2f;
                    bool close = Vector3.Distance(Flat(s.t.position), Flat(s.anchor)) < 0.06f;
                    // An toàn: không có sàn va chạm thì sau 1.6 s vẫn đặt đá vào chỗ.
                    if ((landed && close) || now > s.dropAt + 1.6f)
                    {
                        s.seated = true;
                        s.seatedAt = now;
                        if (s.rb != null) { s.rb.isKinematic = true; }
                        Event(stoneClip, 0.4f, 0.9f + 0.1f * _stones.IndexOf(s));
                    }
                }
                if (s.seated && _phase != Phase.Ending && _phase != Phase.Done)
                {
                    // Trượt nốt vào điểm, dựng thẳng, rung theo độ căng.
                    float shake = Tension(now) * 0.012f;
                    Vector3 jitter = new Vector3(Noise(now, s.anchor.x) * shake, Mathf.Abs(Noise(now, s.anchor.z)) * shake, Noise(now, s.anchor.x + 7f) * shake);
                    s.t.position = Vector3.Lerp(s.t.position, s.anchor + jitter, 0.2f);
                    s.t.rotation = Quaternion.Slerp(s.t.rotation, Quaternion.Euler(0f, _spin + _stones.IndexOf(s) * 120f, 0f), 0.1f);
                }
                if (s.lit)
                {
                    lit++;
                    float glow = 0.6f + 0.4f * Mathf.Sin(now * (4f + 14f * Tension(now)));
                    if (s.mat != null && _phase != Phase.Ending) ChestFxFactory.SetColor(s.mat, Color.Lerp(runeColor * 0.6f, Color.white, glow * Tension(now) * 0.5f));
                }
            }

            if (_phase == Phase.Gather && lit >= _stones.Count && _stones.Count > 0)
            {
                _phase = Phase.Levitate;
            }

            if (_phase == Phase.Done)
            {
                foreach (Stone s in _stones)
                    if (s.t != null && s.tag != null && s.tag.spawnedByRitual) s.t.localScale = Vector3.Lerp(s.t.localScale, Vector3.zero, Time.deltaTime * 1.5f);
            }

            // Đá phụ: viên người chơi ném vào vòng trong lúc tụ phép.
            if (_phase == Phase.Gather || _phase == Phase.Levitate)
            {
                foreach (MagicRuneStone m in MagicRuneStone.All)
                {
                    if (m == null || m.claimedBy != null) continue;
                    if (Vector3.Distance(Flat(m.transform.position), Flat(_restPos)) > _ringRadius) continue;
                    Rigidbody rb = m.GetComponent<Rigidbody>();
                    if (rb == null || rb.isKinematic) continue;
                    m.claimedBy = this;
                    _extras.Add(rb);
                    Event(wispClip, 0.35f, 1.3f);
                }
            }
        }

        private void UpdateWisp(float now, float dt)
        {
            if (_wisp == null || _wispRb != null) return;
            Vector3 target;
            if (_phase == Phase.Gather && _wispTarget < _stones.Count)
            {
                Stone s = _stones[_wispTarget];
                if (!s.seated && now < 0.5f) target = _restPos + Vector3.up * 1.2f;
                else
                {
                    target = s.t.position + Vector3.up * 0.06f;
                    if (Vector3.Distance(_wisp.position, target) < 0.05f)
                    {
                        if (_wispTouchAt < 0f) _wispTouchAt = now;
                        if (now - _wispTouchAt > 0.12f)
                        {
                            s.lit = true;
                            if (s.mat != null) ChestFxFactory.SetColor(s.mat, runeColor);
                            Event(wispClip, 0.35f, 1f + 0.12f * _wispTarget);
                            _wispTarget++;
                            _wispTouchAt = -1f;
                        }
                    }
                }
                _wisp.position = Vector3.MoveTowards(_wisp.position, target + Hover(now, 0.03f), dt * 2.2f);
                return;
            }
            if (_phase == Phase.Ending && _tier == ChestTier.HuyenThoai)
            {
                float k = now - _lockAt;
                float a = k * 9f;
                target = _restPos + new Vector3(Mathf.Cos(a) * 0.25f, Mathf.Min(0.4f + k * 1.6f, 2f * _size), Mathf.Sin(a) * 0.25f);
                _wisp.position = Vector3.Lerp(_wisp.position, target, dt * 8f);
                return;
            }
            // Bay vòng quanh rương, nhanh dần theo độ căng.
            float ang = now * (2f + 6f * Tension(now));
            target = _restPos + new Vector3(Mathf.Cos(ang) * _ringRadius * 0.8f, 0.35f + _lift, Mathf.Sin(ang) * _ringRadius * 0.8f);
            _wisp.position = Vector3.Lerp(_wisp.position, target, dt * 6f);
        }

        private void UpdateChest(float now, float dt)
        {
            float tension = Tension(now);
            if (_phase == Phase.Gather)
            {
                // Rung nhẹ ngay khi bấm: phép đang thức dậy.
                float k = 0.004f + 0.004f * Mathf.Clamp01(now);
                WriteChest(_restPos + new Vector3(Noise(now * 3f, 1f) * k, 0f, Noise(now * 3f, 2f) * k),
                    _restRot * Quaternion.Euler(Noise(now * 2f, 3f) * 1.5f, 0f, Noise(now * 2f, 4f) * 1.5f));
                return;
            }
            if (_phase == Phase.Levitate)
            {
                _lift = Mathf.Lerp(_lift, liftHeight * (0.6f + 0.4f * tension), dt * 2.5f);
                _spinSpeed = Mathf.Lerp(30f, 540f, tension * tension);
                _spin += _spinSpeed * dt;
                float wob = 2f + 6f * tension;
                Vector3 shake = new Vector3(Noise(now * 9f, 5f), Noise(now * 9f, 6f), Noise(now * 9f, 7f)) * (0.01f * tension);
                WriteChest(_restPos + Vector3.up * (_lift + Mathf.Sin(now * 3f) * 0.02f) + shake,
                    Quaternion.Euler(0f, _spin, 0f) * _restRot * Quaternion.Euler(Mathf.Sin(now * 5f) * wob, 0f, Mathf.Cos(now * 4f) * wob));
                return;
            }
            if (_phase == Phase.Ending || _phase == Phase.Done) EndingChest(now - _lockAt, dt);
        }

        private void UpdateRing(float now, float dt)
        {
            if (_ring == null) return;
            float tension = Tension(now);
            int lit = 0;
            foreach (Stone s in _stones) if (s.lit) lit++;
            float alpha = _phase == Phase.Gather ? lit / 3f * 0.6f : 1f;
            float radius = _ringRadius;
            if (_phase == Phase.Levitate) radius = Mathf.Lerp(_ringRadius, _chestRadius * 1.1f, tension * 0.8f);
            if (_phase == Phase.Ending)
            {
                float k = now - _lockAt;
                if (_tier == ChestTier.Rac)
                {
                    alpha = k < 0.35f ? (Mathf.Sin(k * 90f) > 0f ? 1f : 0.1f) : Mathf.Max(0f, 1f - (k - 0.35f) * 4f);
                    radius = _ringRadius * (1f + k * 0.3f);
                }
                else
                {
                    float c = Mathf.Clamp01(k / 0.35f);
                    radius = Mathf.Lerp(_chestRadius * 1.1f, 0.02f, c);
                    alpha = 1f - Mathf.Clamp01((k - 0.35f) * 3f);
                }
            }
            Color col = _phase == Phase.Ending ? _tierColor : Color.Lerp(runeColor, Color.white, tension * 0.4f);
            float y = _floorY + 0.01f;
            float rot = _spin * Mathf.Deg2Rad * 0.5f;
            for (int i = 0; i < 64; i++)
            {
                float a = rot + i / 64f * Mathf.PI * 2f;
                float wob = 1f + 0.03f * tension * Mathf.Sin(a * 6f + now * 10f);
                _ring.SetPosition(i, new Vector3(_restPos.x + Mathf.Cos(a) * radius * wob, y, _restPos.z + Mathf.Sin(a) * radius * wob));
            }
            _ring.widthMultiplier = 1f + 2f * tension;
            SetLineColor(_ring, col, alpha);

            if (_tri != null)
            {
                for (int i = 0; i < 3 && i < _stones.Count; i++)
                    if (_stones[i].t != null) _tri.SetPosition(i, _stones[i].t.position + Vector3.up * 0.02f);
                float triA = _phase == Phase.Ending ? alpha * 0.5f : (lit >= 3 ? 0.8f : 0f);
                SetLineColor(_tri, col, triA);
            }

            if (_light != null && _phase != Phase.Ending && _phase != Phase.Done)
            {
                _light.transform.position = _restPos + Vector3.up * 0.3f;
                _light.intensity = alpha * (0.5f + 3f * tension);
            }
        }

        // ------------------------------------------------------------------ Kết theo bậc

        private void StartEnding(ChestTier tier)
        {
            _liftVel = 0f;
            if (_light != null) _light.color = _tierColor;
            // Khóa đến sớm (đá chưa kịp rơi): thả nốt cho chúng rơi tự nhiên.
            foreach (Stone s in _stones)
                if (!s.dropped && s.rb != null) { s.dropped = true; s.rb.isKinematic = false; }
            foreach (Stone s in _stones) if (s.mat != null) ChestFxFactory.SetColor(s.mat, tier == ChestTier.Rac ? new Color(0.25f, 0.23f, 0.28f) : _tierColor);

            if (tier == ChestTier.Rac)
            {
                // Đá đổ nhào: bật vật lý, đẩy nhẹ cho lật.
                foreach (Stone s in _stones) Release(s.rb, RandVec() * 0.3f + Vector3.up * 0.4f, RandVec() * 8f);
                // Wisp hắt xì rồi rớt như hòn bi.
                if (_wisp != null)
                {
                    Collider col = _wisp.GetComponent<Collider>();
                    if (col != null) col.enabled = true;
                    _wispRb = _wisp.gameObject.AddComponent<Rigidbody>();
                    _wispRb.mass = 0.05f;
                    _wispRb.linearVelocity = RandVec() * 0.5f + Vector3.up * 0.8f;
                }
                foreach (Rigidbody rb in _extras) Release(rb, RandVec() * 0.5f, RandVec() * 4f);
                _extras.Clear();
                Puff(new Color(0.5f, 0.5f, 0.5f), 25, false);
                if (_light != null) _light.intensity = 0.3f;
                return;
            }

            if (tier == ChestTier.Hiem)
                foreach (Stone s in _stones) Release(s.rb, Vector3.up * 1.4f, RandVec() * 3f);

            if (tier >= ChestTier.RatHiem) _liftVel = tier == ChestTier.HuyenThoai ? 3.2f : 2.2f; // vọt lên trước khi dập

            if (tier == ChestTier.HuyenThoai)
            {
                GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                p.name = "RunePillar";
                Collider pc = p.GetComponent<Collider>();
                if (pc != null) Destroy(pc);
                _pillarMat = ChestFxFactory.CreateAdditive(ChestFxFactory.GetTexture(ParticleLook.SoftDot), "RunePillarMat");
                ChestFxFactory.SetColor(_pillarMat, _tierColor);
                p.GetComponent<Renderer>().sharedMaterial = _pillarMat;
                // Cột sáng cao 2,5 S (cylinder cao 2 đơn vị → scale y = 1,25 S), tâm ở 1,25 S trên rương.
                p.transform.position = _restPos + Vector3.up * (1.25f * _size);
                p.transform.localScale = new Vector3(0.04f * _size, 1.25f * _size, 0.04f * _size);
                _pillar = p.transform;
                _owned.Add(p);
                _owned.Add(_pillarMat);
            }

            foreach (Rigidbody rb in _extras) Release(rb, Vector3.up * 2f + RandVec(), RandVec() * 6f);
            _extras.Clear();
        }

        private bool _slammed;

        private void EndingChest(float k, float dt)
        {
            float yaw = _spin;
            if (_tier == ChestTier.Rac)
            {
                // Rơi tự do có nảy một lần, quay chững lại; rồi lắc nghiêng tắt dần.
                _liftVel -= 9.81f * dt;
                _lift += _liftVel * dt;
                if (_lift <= 0f)
                {
                    _lift = 0f;
                    if (_liftVel < -0.6f) { _liftVel = -_liftVel * 0.25f; Puff(new Color(0.55f, 0.5f, 0.45f), 12, false); }
                    else _liftVel = 0f;
                }
                _spinSpeed = Mathf.Lerp(_spinSpeed, 0f, dt * 12f);
                _spin += _spinSpeed * dt;
                float settle = Mathf.Exp(-k * 3f);
                Quaternion tip = Quaternion.Euler(Mathf.Sin(k * 18f) * 9f * settle, 0f, Mathf.Cos(k * 15f) * 6f * settle);
                float snap = Mathf.Clamp01((k - 0.8f) * 3f); // về đúng hướng ban đầu
                WriteChest(_restPos + Vector3.up * _lift,
                    Quaternion.Slerp(Quaternion.Euler(0f, yaw, 0f) * _restRot * tip, _restRot, snap));
                return;
            }

            if (_tier >= ChestTier.RatHiem)
            {
                // Vọt lên, khựng lại, rồi DẬP xuống.
                _liftVel -= (k < 0.25f ? 6f : 30f) * dt;
                _lift += _liftVel * dt;
                if (_lift <= 0f)
                {
                    _lift = 0f;
                    _liftVel = 0f;
                    if (!_slammed) { _slammed = true; _slamK = k; Shockwave(); }
                }
                _spin += Mathf.Lerp(_spinSpeed, 0f, Mathf.Clamp01(k * 2f)) * dt;
                Quaternion q = _slammed ? Quaternion.Slerp(_wantRot, _restRot, dt * 14f) : Quaternion.Euler(0f, _spin, 0f) * _restRot;
                Vector3 jolt = Vector3.zero;
                if (_slammed) jolt = new Vector3(0f, Mathf.Abs(Mathf.Sin(k * 40f)) * 0.01f * Mathf.Exp(-k * 6f), 0f);
                WriteChest(_restPos + Vector3.up * _lift + jolt, q);
                if (_pillar != null)
                {
                    float w = Mathf.Lerp(_pillar.localScale.x, _slammed ? 0.2f * _size : 0.06f * _size, dt * 10f);
                    float fade = _slammed && _slamK >= 0f ? Mathf.Clamp01(1f - (k - _slamK - 0.9f) / 0.6f) : 1f;
                    _pillar.localScale = new Vector3(w * fade, 1.25f * _size, w * fade);
                    if (_slammed && _slamK >= 0f && k > _slamK + 1.5f) { Destroy(_pillar.gameObject); _pillar = null; } // sống 1,5 s
                }
                if (_light != null) _light.intensity = Mathf.Lerp(_light.intensity, 1f, dt * 3f);
                return;
            }

            // Thường / Hiếm: hạ êm, thôi xoay.
            _lift = Mathf.Lerp(_lift, 0f, dt * 8f);
            _spin += Mathf.Lerp(_spinSpeed, 0f, Mathf.Clamp01(k * 3f)) * dt;
            float back = Mathf.Clamp01(k * 2.5f);
            WriteChest(_restPos + Vector3.up * _lift, Quaternion.Slerp(Quaternion.Euler(0f, _spin, 0f) * _restRot, _restRot, back));
            if (_light != null)
            {
                _light.transform.position = _restPos + Vector3.up * 0.3f;
                _light.intensity = Mathf.Lerp(4f, 0.8f, Mathf.Clamp01(k * 2f));
            }
            if (k < 0.5f) foreach (Stone s in _stones) if (s.mat != null) ChestFxFactory.SetColor(s.mat, Color.Lerp(Color.white, _tierColor, k * 2f));
        }

        private void Shockwave()
        {
            float power = _tier == ChestTier.HuyenThoai ? 7f : 4.5f;
            Vector3 c = _restPos;
            foreach (Stone s in _stones)
            {
                if (s.t == null) continue;
                Vector3 d = Flat(s.t.position - c).normalized;
                Release(s.rb, d * power + Vector3.up * power * 0.6f, RandVec() * 15f);
            }
            if (_light != null) _light.intensity = _tier == ChestTier.HuyenThoai ? 14f : 9f;
            Puff(Color.Lerp(_tierColor, Color.white, 0.4f), _tier == ChestTier.HuyenThoai ? 60 : 35, true);
            if (_tier >= ChestTier.RatHiem) Sfx(slamClip, 0.6f); // HĐ: chỉ RH/HT
        }

        // ------------------------------------------------------------------ Tiện ích

        private float Tension(float now)
        {
            if (_phase == Phase.Ending || _phase == Phase.Done) return 1f;
            float u = Mathf.Clamp01(now / Mathf.Max(1f, expectedBuildSeconds));
            return u * u * (3f - 2f * u);
        }

        private void MeasureChest(Transform chest)
        {
            Renderer[] rs = chest.GetComponentsInChildren<Renderer>();
            bool any = false;
            Bounds b = new Bounds(chest.position, Vector3.zero);
            foreach (Renderer r in rs)
            {
                if (r == null || r is ParticleSystemRenderer) continue;
                if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            }
            _floorY = any ? b.min.y : chest.position.y;
            _chestRadius = any ? Mathf.Max(b.extents.x, b.extents.z) : 0.25f;
            _size = any ? Mathf.Max(0.1f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z))) : 0.5f;
            // Sàn thật bên dưới (nếu rương không đặt sát sàn).
            if (Physics.Raycast(new Vector3(_restPos.x + _chestRadius * 2.5f, _floorY + 0.5f, _restPos.z), Vector3.down, out RaycastHit hit, 2f))
                _floorY = hit.point.y;
        }

        private AudioSource MakeSource(string n)
        {
            AudioSource s = new GameObject(n).AddComponent<AudioSource>();
            s.transform.SetParent(_root, false);
            s.transform.position = _restPos;
            s.playOnAwake = false;
            s.spatialBlend = 0.5f;
            return s;
        }

        /// <summary>Tiếng lẻ. Cao độ khác 1 thì nhờ lõi tạo nguồn riêng (A7), không đổi pitch của nguồn đang ngân.</summary>
        private void Sfx(AudioClip clip, float vol, float pitch = 1f)
        {
            if (clip == null) return;
            if (!Mathf.Approximately(pitch, 1f) && _reveal != null) { _reveal.PlayOneShot(clip, vol * volume, pitch); return; }
            if (_sfxSrc != null) _sfxSrc.PlayOneShot(clip, vol * volume);
        }

        /// <summary>Sự kiện trước chốt: tối đa 1 tiếng mỗi 0,2 s (A3), âm lượng 0,3–0,5.</summary>
        private void Event(AudioClip clip, float vol, float pitch = 1f)
        {
            if (clip == null || _phase == Phase.Ending || _phase == Phase.Done) return;
            if (!ChestAudioRule.TryGate(ref _lastEvt, ChestAudioRule.EventGap)) return;
            Sfx(clip, Mathf.Min(vol, ChestAudioRule.EventMax), pitch);
        }

        /// <summary>TC (A4): đúng 1 clip stingClips[bậc]. Rác: lặng 0,2 s rồi xịt; bậc khác phát ngay.</summary>
        private void PlaySting(ChestTier tier)
        {
            if (stingClips == null || stingClips.Length == 0) return;
            int i = Mathf.Clamp(tier.Rank(), 0, stingClips.Length - 1);
            AudioClip clip = stingClips[i];
            if (clip == null) return;
            if (tier == ChestTier.Rac) { _stingClip = clip; _stingAt = Time.time + ChestAudioRule.RacSilence; return; }
            float v = tier == ChestTier.Thuong ? 0.7f : tier == ChestTier.Hiem ? 0.85f : 1f;
            Sfx(clip, Mathf.Min(v, ChestAudioRule.StingMax));
        }

        /// <summary>Nền dâng 0,15 → 0,45 và cao độ +45% theo độ căng; tiếng xịt của Rác sau 0,2 s lặng.</summary>
        private void UpdateAudio(float now)
        {
            if (_stingAt >= 0f && Time.time >= _stingAt)
            {
                Sfx(_stingClip, 0.7f);
                _stingAt = -1f;
                _stingClip = null;
            }
            if (_buildSrc != null && _phase != Phase.Ending && _phase != Phase.Done)
            {
                float t = Tension(now);
                _buildSrc.volume = Mathf.Lerp(0.15f, ChestAudioRule.BedMax, t) * volume;
                _buildSrc.pitch = Mathf.Lerp(1f, 1.45f, t);
            }
        }

        /// <summary>
        /// Ghi vị trí/góc rương dưới dạng độ lệch so với gốc. Gốc = giá trị BoxOpenSequence vừa ghi nếu khác giá trị
        /// hook ghi khung trước (kiểu HaiTacSeqHook.LateUpdate); không thì giữ gốc cũ.
        /// </summary>
        private void WriteChest(Vector3 wantPos, Quaternion wantRot)
        {
            Transform chest = _reveal.transform;
            if (!_hasWritten || (chest.position - _lastPos).sqrMagnitude > 1e-8f || Quaternion.Angle(chest.rotation, _lastRot) > 0.01f)
            {
                _basePos = chest.position;
                _baseRot = chest.rotation;
                if (!_hasWritten) { _basePos = _restPos; _baseRot = _restRot; }
            }
            Vector3 pos = _basePos + (wantPos - _restPos);
            Quaternion rot = (wantRot * Quaternion.Inverse(_restRot)) * _baseRot;
            chest.SetPositionAndRotation(pos, rot);
            _wantRot = wantRot;
            _lastPos = chest.position;
            _lastRot = chest.rotation;
            _hasWritten = true;
        }

        private LineRenderer MakeLine(string n, int count, bool loop, float width)
        {
            LineRenderer l = new GameObject(n).AddComponent<LineRenderer>();
            l.transform.SetParent(_root, false);
            l.useWorldSpace = true;
            l.loop = loop;
            l.positionCount = count;
            l.startWidth = l.endWidth = width;
            Material m = ChestFxFactory.CreateAdditive(ChestFxFactory.GetTexture(ParticleLook.SoftDot), n + "Mat");
            l.sharedMaterial = m;
            _owned.Add(m);
            return l;
        }

        private static void SetLineAlpha(LineRenderer l, float a) => SetLineColor(l, Color.white, a);

        private static void SetLineColor(LineRenderer l, Color c, float a)
        {
            Color k = new Color(c.r, c.g, c.b, Mathf.Clamp01(a));
            l.startColor = k;
            l.endColor = k;
        }

        private void Puff(Color color, int count, bool radial)
        {
            if (_puff == null)
            {
                _puffStyle = ScriptableObject.CreateInstance<ChestFxProfile>();
                _puffStyle.hideFlags = HideFlags.DontSave;
                _puffStyle.particleLook = ParticleLook.SoftDot;
                _puffStyle.particleDirection = ParticleDirection.Radial;
                _puffStyle.particleSpeed = 0.6f;
                _puffStyle.particleSize = 0.07f;
                _puffStyle.particleLifetime = 0.9f;
                _puffStyle.burstGravity = -0.05f;
                _puff = ChestFxFactory.CreateParticles("RunePuff", _root, _puffStyle, 0.6f, true);
                ParticleSystem.ShapeModule shape = _puff.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = Mathf.Max(0.05f, _chestRadius);
                shape.rotation = new Vector3(90f, 0f, 0f);
                _owned.Add(_puffStyle);
            }
            _puff.transform.position = new Vector3(_restPos.x, _floorY + 0.03f, _restPos.z);
            ParticleSystem.MainModule main = _puff.main;
            main.startSpeed = radial ? 2.2f : 0.4f;
            ChestFxFactory.SetParticleColor(_puff, color, _puffStyle, radial ? 1.6f : 0.5f);
            _puff.Play();
            _puff.Emit(count);
        }

        private static void Release(Rigidbody rb, Vector3 impulse, Vector3 spin)
        {
            if (rb == null) return;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(impulse * rb.mass, ForceMode.Impulse);
            rb.angularVelocity = spin;
        }

        private Vector3 RandVec()
        {
            return new Vector3((float)_rng.NextDouble() * 2f - 1f, (float)_rng.NextDouble() * 2f - 1f, (float)_rng.NextDouble() * 2f - 1f);
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        private static float Noise(float t, float seed) => Mathf.PerlinNoise(t, seed) * 2f - 1f;

        private static Vector3 Hover(float t, float a) => new Vector3(Mathf.Sin(t * 7f) * a, Mathf.Sin(t * 11f) * a, Mathf.Cos(t * 9f) * a);

        private void Cleanup()
        {
            if (_reveal != null && _phase != Phase.Idle) _reveal.transform.SetPositionAndRotation(_restPos, _restRot);
            foreach (Stone s in _stones)
            {
                if (s.tag != null && !s.tag.spawnedByRitual) s.tag.claimedBy = null; // trả đá cho người chơi
            }
            foreach (Rigidbody rb in _extras)
            {
                MagicRuneStone m = rb != null ? rb.GetComponent<MagicRuneStone>() : null;
                if (m != null) m.claimedBy = null;
            }
            foreach (MagicRuneStone m in MagicRuneStone.All) if (m != null && m.claimedBy == this && !m.spawnedByRitual) m.claimedBy = null;
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
            _stones.Clear();
            _extras.Clear();
            _ring = _tri = null;
            _wisp = null;
            _wispRb = null;
            _light = null;
            _buildSrc = _sfxSrc = null;
            _puff = null;
            _puffStyle = null;
            _pillar = null;
            _root = null;
            _lift = _liftVel = _spin = _spinSpeed = 0f;
            _slammed = false;
            _slamK = -1f;
            _stingAt = -1f;
            _stingClip = null;
            _hasWritten = false;
            _phase = Phase.Idle;
        }
    }
}

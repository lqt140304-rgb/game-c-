using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TrungHoa — "ĐÊM GIAO THỪA": kịch bản mở rương riêng (spec: docs/superpowers/specs/2026-11-25-kichban-trunghoa-design.md).
    ///
    ///  0 s   BẤM MỞ: trống lân "tùng tùng" vang ngay, đầu sư tử giấy nhảy vào ngậm nén nhang; dây pháo đỏ đã nằm
    ///        quấn xoắn quanh chân rương (khối thật có va chạm).
    ///  ~1,4s Sư tử cúi châm ngòi (hoặc NGƯỜI CHƠI thả nén nhang vào đầu ngòi — có thể bắt buộc, xem requirePlayerLight).
    ///        Tia lửa bò dọc ngòi, xèo xèo; từng quả pháo nổ = Rigidbody văng ra, vỏ rơi lộp độp, rương bị hất NẢY + NGHIÊNG.
    ///  vòng quay: mỗi "tích" màu là một quả pháo nổ -> nhịp pháo nhanh dần theo vòng quay, trống lên giọng, dồn dập.
    ///  chốt bậc: quả "pháo đại" cuối dây quyết định:
    ///     Rác      : pháo đại TỊT NGÒI (xì khói), trống tắt phụt, tiếng huýt trượt xuống; một quả thăng thiên xịt
    ///                lảo đảo rơi bịch cạnh rương; sư tử hắt xì rồi ngã ngửa (Rigidbody thật).
    ///     Thường   : nổ vừa, một quả thăng thiên nhỏ, một đèn lồng rơi.
    ///     Hiếm     : rương bật cao, 1 thăng thiên nở hoa màu bậc, 3 đèn lồng.
    ///     Rất hiếm : 2 thăng thiên + chập chõa, 5 đèn lồng, sư tử múa vòng quanh.
    ///     Huyền thoại: rương phóng lên xoay tít, 3 thăng thiên rồi một quả nở CHỮ 福, 9 đèn lồng rơi đập vào rương.
    ///  Pháo thăng thiên người chơi đặt quanh rương trước lúc chốt (TrungHoaPlayerItem.BonusRocket) bay lên cùng màn cuối.
    ///
    /// BẤT BIẾN: bậc do game quyết (ChestRevealSequence.Play). Hook này chỉ ĐỌC bậc ở OnTierLocked để trang trí;
    /// trước lúc chốt mọi bậc diễn giống hệt nhau (giữ bí mật). Ngẫu nhiên chỉ dùng hạt giống trang trí riêng.
    /// </summary>
    public class TrungHoaHook : ChestFxHook
    {
        [Header("Dây pháo")]
        [SerializeField] private int crackerCount = 22;
        [Tooltip("Tốc độ tia lửa bò dọc ngòi trước khi vòng quay chạy (lần cỡ rương mỗi giây).")]
        [SerializeField] private float fuseSpeed = 0.35f;
        [SerializeField] private float lionLightDelay = 1.4f;

        [Header("Tương tác người chơi")]
        [Tooltip("Bật: hộp dừng sau bước 'Rung' tới khi ngòi được châm (người chơi thả nén nhang vào đầu ngòi). Quá thời gian chờ thì sư tử tự châm.")]
        [SerializeField] private bool requirePlayerLight;
        [SerializeField] private float playerLightTimeout = 8f;
        [SerializeField] private float incenseLightRadius = 0.12f;
        [Tooltip("Pháo thăng thiên người chơi đặt trong bán kính này (lần cỡ rương) được bắn cùng màn cuối.")]
        [SerializeField] private float bonusRocketRadius = 2.5f;

        [Header("Hạt giống trang trí (0 = ngẫu nhiên mỗi lần)")]
        [SerializeField] private int decorationSeed;

        [Header("Âm (TrungHoaFxSetup gán từ Audio/Chests/trung_hoa/fx/)")]
        public AudioClip drumLoop;        // trống lân lặp, bắt đầu NGAY khi bấm
        public AudioClip fuseHissLoop;    // ngòi xèo xèo lặp
        public AudioClip matchStrike;     // quẹt châm
        public AudioClip[] pops;          // pháo nổ lẻ
        public AudioClip bigBang;         // pháo đại
        public AudioClip fizzle;          // tịt ngòi "xììì"
        public AudioClip slideDown;       // huýt trượt xuống (hài)
        public AudioClip rocketWhistle;   // thăng thiên rít lên
        public AudioClip rocketBoom;      // thăng thiên nổ
        public AudioClip cymbal;          // chập chõa
        public AudioClip dudThud;         // pháo xịt rơi bịch
        public AudioClip sneeze;          // sư tử hắt xì
        [Range(0f, 1f)] public float masterVolume = 0.9f;

        private enum Phase { Idle, Approach, Burning, Finale }

        private ChestRevealSequence _reveal;
        private Transform _chest;
        private Phase _phase = Phase.Idle;
        private bool _lit, _rouletteRunning, _sawLock;
        private float _t;
        private System.Random _rng;
        private Bounds _bounds;
        private float _size = 0.5f;
        private Vector3 _ground, _front, _right;

        private GameObject _root;
        private TrungHoaFirecrackerString _string;
        private TrungHoaSky _sky;
        private TrungHoaLion _lion;
        private TrungHoaChestMotion _motion;
        private AudioSource _drum, _hiss;
        private int _blasted;
        private bool _gateSet, _gateOld;
        private readonly List<Vector3> _bonusPads = new List<Vector3>();

        private static readonly Color Red = new Color(1f, 0.15f, 0.08f);
        private static readonly Color Gold = new Color(1f, 0.82f, 0.3f);

        // ------------------------------------------------------------------ phát hiện lúc BẤM MỞ

        private void Update()
        {
            if (_reveal == null) _reveal = GetComponentInParent<ChestRevealSequence>();
            if (_reveal == null) return;

            // lần Play mới khi màn cũ còn: hook không nhận OnReset trong mọi đường đi, tự dọn
            if (_phase != Phase.Idle && _sawLock && _reveal.IsPlaying && !_reveal.IsLocked) Cleanup();
            if (_phase == Phase.Idle && _reveal.IsPlaying && !_reveal.IsLocked) Begin();
            if (_phase == Phase.Idle) return;

            float dt = Time.deltaTime;
            _t += dt;
            if (_phase == Phase.Approach) TickApproach();
            if (_phase == Phase.Burning) TickBurning(dt);
            TickAudio(dt);
        }

        private void Begin()
        {
            _chest = _reveal.transform;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : System.Environment.TickCount);
            _phase = Phase.Approach;
            _t = 0f;
            _lit = _rouletteRunning = _sawLock = false;
            _blasted = 0;
            _bonusPads.Clear();
            Measure();

            _root = new GameObject("TrungHoa_Show");
            _root.transform.position = _ground;

            // mặt đất + khối thay rương vô hình cho vỏ pháo, đèn lồng, sư tử ngã va chạm thật
            BoxCollider floor = new GameObject("Dat").AddComponent<BoxCollider>();
            floor.transform.SetParent(_root.transform, false);
            floor.transform.position = _ground - Vector3.up * 0.05f;
            floor.size = new Vector3(_size * 12f, 0.1f, _size * 12f);
            BoxCollider proxy = new GameObject("KhoiRuong").AddComponent<BoxCollider>();
            proxy.transform.SetParent(_root.transform, false);
            proxy.transform.SetPositionAndRotation(_bounds.center, _chest.rotation);
            proxy.size = _chest.InverseTransformVector(_bounds.size);
            proxy.size = new Vector3(Mathf.Abs(proxy.size.x), Mathf.Abs(proxy.size.y), Mathf.Abs(proxy.size.z));

            _motion = _chest.GetComponent<TrungHoaChestMotion>();
            if (_motion == null) _motion = _chest.gameObject.AddComponent<TrungHoaChestMotion>();

            float radius = Mathf.Max(_bounds.extents.x, _bounds.extents.z) + _size * 0.12f;
            _string = TrungHoaFirecrackerString.Build(_ground, _front, radius, Mathf.Max(4, crackerCount), _size, _rng, _root.transform);
            _sky = TrungHoaSky.Build(_size, _rng, _root.transform);
            _sky.Burst += OnSkyBurst;
            _sky.DudLanded += OnDudLanded;

            _lion = TrungHoaLion.Build(_size * 0.45f, _root.transform);
            _lion.transform.position = _ground + (_front * 2.6f + _right * 1.8f) * _size;
            Vector3 head = _string.FuseHead;
            _lion.HopTo(head + (_front * 0.55f + _right * 0.1f) * _size, _size * 0.35f, 0.7f, head);

            _drum = MakeLoop("TrongLan", drumLoop);
            _hiss = MakeLoop("NgoiXeo", fuseHissLoop);
            if (_drum != null) { _drum.volume = 0.35f * masterVolume; _drum.pitch = 0.92f; _drum.Play(); }

            if (requirePlayerLight && _reveal.Box != null && _reveal.Box.Stages.Count > 0)
            {
                _gateOld = _reveal.Box.Stages[0].waitForPlayer;
                _reveal.Box.Stages[0].waitForPlayer = true;
                _gateSet = true;
            }
        }

        private void Measure()
        {
            bool any = false;
            _bounds = new Bounds(_chest.position, Vector3.one * 0.3f);
            foreach (Renderer r in _chest.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer || r is LineRenderer) continue;
                if (!any) { _bounds = r.bounds; any = true; }
                else _bounds.Encapsulate(r.bounds);
            }
            _size = Mathf.Max(0.1f, Mathf.Max(_bounds.size.x, Mathf.Max(_bounds.size.y, _bounds.size.z)));
            _ground = new Vector3(_bounds.center.x, _bounds.min.y, _bounds.center.z);
            Vector3 f = _reveal.Box != null ? _chest.TransformDirection(_reveal.Box.FrontDirection) : _chest.forward;
            f.y = 0f;
            _front = f.sqrMagnitude > 1e-6f ? f.normalized : Vector3.forward;
            _right = Vector3.Cross(Vector3.up, _front);
        }

        private AudioSource MakeLoop(string n, AudioClip clip)
        {
            if (clip == null) return null;
            AudioSource a = new GameObject(n).AddComponent<AudioSource>();
            a.transform.SetParent(_root.transform, false);
            a.clip = clip;
            a.loop = true;
            a.playOnAwake = false;
            a.spatialBlend = 0f;
            return a;
        }

        // ------------------------------------------------------------------ châm ngòi

        private void TickApproach()
        {
            ScanPlayerItems();
            if (_lit) return;
            float lionAt = requirePlayerLight ? playerLightTimeout : lionLightDelay;
            if (_t >= lionAt && !_lion.Hopping)
            {
                _lion.Chomp(1f);
                Light();
            }
        }

        private void ScanPlayerItems()
        {
            for (int i = TrungHoaPlayerItem.Active.Count - 1; i >= 0; i--)
            {
                TrungHoaPlayerItem item = TrungHoaPlayerItem.Active[i];
                if (item == null || item.consumed) continue;
                if (item.kind == TrungHoaPlayerItem.Kind.Incense)
                {
                    if (!_lit && Vector3.Distance(item.TipPosition, _string.FuseHead) <= incenseLightRadius + _size * 0.05f)
                    {
                        Light();
                        _lion.Startle(_string.FuseHead, _size * 0.4f, _size * 0.2f); // sư tử giật mình vì bị cướp việc
                    }
                }
                else if (Vector3.Distance(item.transform.position, _ground) <= bonusRocketRadius * _size && _phase != Phase.Finale)
                {
                    item.consumed = true;
                    _bonusPads.Add(item.transform.position);
                    _sky.SparkBurst(item.TipPosition, Gold, Red, 10, _size * 0.4f, 0.3f); // "nhận" quả pháo
                }
            }
        }

        private void Light()
        {
            if (_lit) return;
            _lit = true;
            _phase = Phase.Burning;
            OneShot(matchStrike, 0.8f, 1f);
            _string.ShowSpark(true);
            _sky.SparkBurst(_string.FuseHead, Gold, Color.white, 14, _size * 0.5f, 0.25f);
            if (_hiss != null) { _hiss.volume = 0.5f * masterVolume; _hiss.Play(); }
            if (_gateSet && _reveal.Box != null)
            {
                _reveal.Box.Stages[0].waitForPlayer = _gateOld;
                _gateSet = false;
                _reveal.Box.Advance();
            }
            Vector3 back = _lion.transform.position + (_front * 0.5f + _right * 0.6f) * _size;
            _lion.HopTo(back, _size * 0.25f, 0.45f, _ground);
        }

        // ------------------------------------------------------------------ cháy

        private void TickBurning(float dt)
        {
            ScanPlayerItems();
            int last = _string.Crackers.Count - 1;
            // không để tia lửa tự tới pháo đại trước lúc chốt: dừng sát nó, xèo xèo chờ
            float limit = _string.Crackers[last].pathDistance - _size * 0.04f;
            float speed = fuseSpeed * _size * (_rouletteRunning ? 0.5f : 1f);
            if (_string.Burnt < limit)
            {
                int idx = _string.Burn(Mathf.Min(speed * dt, limit - _string.Burnt));
                if (idx >= 0 && idx < last) Pop(idx);
            }
            if (_lion != null && !_lion.Hopping && _rng.NextDouble() < dt * 0.6f)
                _lion.Startle(_string.SparkPosition, _size * 0.08f, _size * 0.12f);
        }

        private float Progress => Mathf.Clamp01(_blasted / (float)Mathf.Max(1, _string.Crackers.Count - 1));

        private void Pop(int idx)
        {
            Vector3 center = _bounds.center;
            Vector3 pos = _string.Blast(idx, center, 1f + Progress, _rng, false);
            _blasted++;
            _sky.SparkBurst(pos, Red, Gold, 16, _size * 1.4f, 0.3f);
            _sky.SmokePuff(pos, new Color(0.75f, 0.72f, 0.7f, 0.5f), 3, _size * 0.1f);
            OneShot(Pick(pops), Mathf.Lerp(0.45f, 0.9f, Progress), 0.9f + (float)_rng.NextDouble() * 0.3f);
            Vector3 push = center - pos; push.y = 0f;
            _motion.Kick(Mathf.Lerp(0.35f, 0.95f, Progress), push.normalized, Mathf.Lerp(25f, 70f, Progress));
            if (_rng.NextDouble() < 0.3) _lion.Startle(pos, _size * 0.12f, _size * 0.15f);
        }

        /// <summary>Vòng quay hết pháo nhỏ: rương tự giật lạch cạch (vẫn giữ nhịp).</summary>
        private void Rattle()
        {
            _motion.Kick(0.4f, new Vector3((float)_rng.NextDouble() - 0.5f, 0f, (float)_rng.NextDouble() - 0.5f), 30f);
            _sky.SparkBurst(_reveal.MouthPosition, Red, Gold, 6, _size * 0.6f, 0.25f);
            OneShot(Pick(pops), 0.35f, 1.3f);
        }

        // ------------------------------------------------------------------ nhịp của vòng quay

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            _reveal = reveal;
            if (_phase == Phase.Idle) Begin();
            _rouletteRunning = true;
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (_phase == Phase.Idle || _phase == Phase.Finale) return;
            if (!_lit) { if (requirePlayerLight && _t < playerLightTimeout) return; _lion.Chomp(1f); Light(); }
            int idx = _string.JumpToNext(false);
            if (idx >= 0) Pop(idx); else Rattle();
        }

        private void TickAudio(float dt)
        {
            if (_phase == Phase.Burning)
            {
                float p = Progress;
                if (_drum != null)
                {
                    _drum.pitch = Mathf.MoveTowards(_drum.pitch, Mathf.Lerp(0.95f, 1.35f, p), dt * 0.5f);
                    _drum.volume = Mathf.MoveTowards(_drum.volume, Mathf.Lerp(0.45f, 0.85f, p) * masterVolume, dt);
                }
                if (_hiss != null) _hiss.volume = (0.35f + 0.25f * Mathf.PerlinNoise(_t * 6f, 0.2f) + 0.2f * p) * masterVolume;
            }
        }

        // ------------------------------------------------------------------ chốt bậc: màn cuối

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_phase == Phase.Idle) return;
            if (!_lit) Light();
            _sawLock = true;
            _phase = Phase.Finale;
            StartCoroutine(tier == ChestTier.Rac ? FinaleDud() : FinaleFestive(tier, color));
        }

        private IEnumerator FinaleDud()
        {
            // trống tắt phụt, ngòi xì một hơi dài rồi tịt
            if (_drum != null) _drum.Stop();
            int last = _string.Crackers.Count - 1;
            _string.JumpToNext(true);
            OneShot(fizzle, 0.9f, 1f);
            Vector3 pos = _string.Blast(last, _bounds.center, 0f, _rng, true);
            _string.ShowSpark(false);
            _sky.SmokePuff(pos, new Color(0.45f, 0.45f, 0.45f, 0.7f), 14, _size * 0.15f);
            if (_hiss != null) StartCoroutine(Fade(_hiss, 0.6f));
            _motion.Kick(0.3f, _right, 15f);
            yield return new WaitForSeconds(0.7f);
            OneShot(slideDown, 0.85f, 1f);
            // một quả thăng thiên xịt: nhích lên, lảo đảo, rơi bịch cạnh rương
            _sky.Launch(_ground + (_right * 0.5f + _front * 0.35f) * _size * 1.4f, Red, _size, false, true);
            OneShot(rocketWhistle, 0.4f, 0.6f);
            yield return new WaitForSeconds(0.9f);
            OneShot(sneeze, 0.9f, 1f);
            _lion.Chomp(1f);
            _sky.SmokePuff(_lion.EmberPosition, new Color(0.9f, 0.9f, 0.85f, 0.6f), 8, _size * 0.3f);
            _lion.SneezeAndFall();
        }

        private IEnumerator FinaleFestive(ChestTier tier, Color tierColor)
        {
            float r01 = tier.Rank01();
            int last = _string.Crackers.Count - 1;
            _string.JumpToNext(true);
            Vector3 pos = _string.Blast(last, _bounds.center, 3f + 4f * r01, _rng, false);
            _string.ShowSpark(false);
            OneShot(bigBang, Mathf.Lerp(0.7f, 1f, r01), 1f);
            _sky.SparkBurst(pos, Red, Gold, 40 + Mathf.RoundToInt(80 * r01), _size * 2f, 0.5f);
            _sky.SmokePuff(pos, new Color(0.8f, 0.78f, 0.75f, 0.6f), 10, _size * 0.2f);
            if (_hiss != null) StartCoroutine(Fade(_hiss, 0.3f));

            // CẢ RƯƠNG bật lên: Thường nảy nhẹ ... Huyền thoại phóng cao xoay tít
            float up = Mathf.Lerp(0.9f, 2.4f, r01) * Mathf.Sqrt(_size / 0.5f);
            _motion.Kick(up, (_bounds.center - pos).normalized, Mathf.Lerp(40f, 120f, r01), tier == ChestTier.HuyenThoai ? 540f : 90f * r01);

            int rockets = tier == ChestTier.Thuong ? 1 : (int)tier - 1;
            bool phuc = tier == ChestTier.HuyenThoai;
            if (tier >= ChestTier.RatHiem) OneShot(cymbal, 0.8f, 1f);
            if (_drum != null)
            {
                if (tier <= ChestTier.Thuong) StartCoroutine(Fade(_drum, 1.2f));
                else _drum.pitch = 1.4f;
            }

            yield return new WaitForSeconds(0.35f);
            Color burstColor = Color.Lerp(tierColor, Gold, tier == ChestTier.Thuong ? 0.5f : 0.15f);
            float height = _size * Mathf.Lerp(2.2f, 4.5f, r01);
            for (int i = 0; i < rockets; i++)
            {
                float a = i / (float)Mathf.Max(1, rockets) * Mathf.PI * 2f + (float)_rng.NextDouble();
                Vector3 pad = _ground + (Mathf.Cos(a) * _right + Mathf.Sin(a) * _front) * _size * 0.9f + Vector3.up * _size * 0.05f;
                _sky.Launch(pad, burstColor, height * (0.85f + 0.3f * (float)_rng.NextDouble()), false, false, i * 0.35f);
                StartCoroutine(Delayed(i * 0.35f, () => OneShot(rocketWhistle, 0.6f, 0.9f + (float)_rng.NextDouble() * 0.25f)));
            }
            foreach (Vector3 pad in _bonusPads)
            {
                _sky.Launch(pad + Vector3.up * _size * 0.05f, Color.Lerp(burstColor, Red, 0.5f), height * 0.9f, false, false, 0.2f);
            }
            _bonusPads.Clear();
            DestroyConsumedItems();

            if (tier >= ChestTier.RatHiem) _lion.Dance(_ground, _size * 1.4f);
            else _lion.HopTo(_lion.transform.position + Vector3.up * 0.01f, _size * 0.4f, 0.5f, _ground);

            if (phuc)
            {
                yield return new WaitForSeconds(rockets * 0.35f + 0.5f);
                OneShot(rocketWhistle, 0.8f, 0.75f);
                _sky.Launch(_ground + _front * _size * 0.9f, tierColor, _size * 5f, true, false);
            }

            int lanterns = tier == ChestTier.Thuong ? 1 : tier == ChestTier.Hiem ? 3 : tier == ChestTier.RatHiem ? 5 : 9;
            yield return new WaitForSeconds(0.6f);
            for (int i = 0; i < lanterns; i++)
            {
                float a = (float)_rng.NextDouble() * Mathf.PI * 2f;
                float d = (float)_rng.NextDouble() * _size * 1.2f;
                Vector3 from = _ground + new Vector3(Mathf.Cos(a) * d, _size * (4f + (float)_rng.NextDouble() * 1.5f), Mathf.Sin(a) * d);
                _sky.DropLantern(from, tierColor);
                yield return new WaitForSeconds(0.18f + (float)_rng.NextDouble() * 0.2f);
            }
            if (_drum != null && tier > ChestTier.Thuong)
            {
                yield return new WaitForSeconds(2.5f);
                StartCoroutine(Fade(_drum, 1.5f));
            }
        }

        private void OnSkyBurst(Vector3 pos, bool big)
        {
            OneShot(rocketBoom, big ? 1f : 0.75f, big ? 0.8f : 0.95f + (float)_rng.NextDouble() * 0.2f);
            if (big) OneShot(cymbal, 0.9f, 0.9f);
        }

        private void OnDudLanded(Vector3 pos)
        {
            OneShot(dudThud, 0.8f, 1f);
            _sky.SmokePuff(pos, new Color(0.5f, 0.5f, 0.5f, 0.6f), 5, _size * 0.1f);
        }

        public override void OnRevealed(ChestRevealSequence reveal) { }

        // ------------------------------------------------------------------ tiện ích

        private void OneShot(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || _reveal == null) return;
            _reveal.PlayOneShot(clip, volume * masterVolume, pitch);
        }

        private AudioClip Pick(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return null;
            return clips[_rng.Next(clips.Length)];
        }

        private static IEnumerator Fade(AudioSource a, float seconds)
        {
            float v0 = a.volume;
            for (float t = 0f; t < seconds && a != null; t += Time.deltaTime)
            {
                a.volume = v0 * (1f - t / seconds);
                yield return null;
            }
            if (a != null) a.Stop();
        }

        private static IEnumerator Delayed(float seconds, System.Action action)
        {
            if (seconds > 0f) yield return new WaitForSeconds(seconds);
            action();
        }

        private void DestroyConsumedItems()
        {
            for (int i = TrungHoaPlayerItem.Active.Count - 1; i >= 0; i--)
            {
                TrungHoaPlayerItem item = TrungHoaPlayerItem.Active[i];
                if (item != null && item.consumed && item.kind == TrungHoaPlayerItem.Kind.BonusRocket) Destroy(item.gameObject);
            }
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            StopAllCoroutines();
            if (_gateSet && _reveal != null && _reveal.Box != null) _reveal.Box.Stages[0].waitForPlayer = _gateOld;
            _gateSet = false;
            if (_motion != null) _motion.Restore();
            if (_sky != null) { _sky.Burst -= OnSkyBurst; _sky.DudLanded -= OnDudLanded; }
            if (_root != null) Destroy(_root);
            _root = null;
            _string = null;
            _sky = null;
            _lion = null;
            _drum = _hiss = null;
            _phase = Phase.Idle;
            _lit = _rouletteRunning = _sawLock = false;
        }
    }
}

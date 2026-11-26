using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Mọi thứ một bước kịch bản hoặc một hook (<see cref="ChestFxHook.OnCue"/>) cần để làm việc: rương, bậc đã quay (chỉ đọc),
    /// bộ chuyển động, sổ dọn dẹp, sự kiện, khe nhận vật. Do <see cref="ChestRevealSequence"/> tạo mỗi lần Play.
    /// </summary>
    public class ChestScriptContext
    {
        public ChestRevealSequence Reveal { get; internal set; }
        /// <summary>Bậc game đã quay. Chỉ đọc: hiệu ứng không được đổi.</summary>
        public ChestTier Tier { get; internal set; }
        /// <summary>Chuyển động cả rương (tạo khi cần).</summary>
        public ChestMotion Motion => Reveal.Motion;
        public ChestFxScope Scope { get; internal set; }
        /// <summary>Hạt giống trang trí của lần mở này (cùng hạt = cùng chuỗi ngẫu nhiên trang trí).</summary>
        public int Seed { get; internal set; }

        private readonly Dictionary<string, float> _events = new Dictionary<string, float>();
        private Collider[] _chestColliders;

        public Transform Chest => Reveal.transform;
        public Vector3 Mouth => Reveal.MouthPosition;
        public float ChestHeight => Reveal.ChestHeight;
        public float ChestSize => Reveal.ChestSize;

        /// <summary>Số ngẫu nhiên riêng, cố định theo (hạt, salt): không phụ thuộc thứ tự chạy của các bước.</summary>
        public System.Random RngFor(int salt) => new System.Random(Seed * 7919 + salt * 104729 + 13);

        /// <summary>Vị trí thế giới của gốc + độ lệch theo hướng của rương.</summary>
        public Vector3 AnchorPosition(StepAnchor anchor, Vector3 offset)
        {
            Vector3 p;
            switch (anchor)
            {
                case StepAnchor.ChestCenter: p = Mouth - Chest.up * (ChestHeight * 0.3f); break;
                case StepAnchor.ChestBase: p = Mouth - Chest.up * (ChestHeight * 0.7f); break;
                case StepAnchor.Silhouette: p = Reveal.Silhouette != null ? Reveal.Silhouette.position : Mouth; break;
                default: p = Mouth; break;
            }
            return p + Chest.rotation * offset;
        }

        /// <summary>Collider (không trigger) của rương: để vật bắn ra bỏ qua va chạm với rương trong lúc mới ra.</summary>
        public Collider[] ChestColliders
        {
            get
            {
                if (_chestColliders == null)
                {
                    List<Collider> list = new List<Collider>();
                    foreach (Collider c in Chest.GetComponentsInChildren<Collider>()) if (!c.isTrigger) list.Add(c);
                    _chestColliders = list.ToArray();
                }
                return _chestColliders;
            }
        }

        public bool HasEvent(string name) => !string.IsNullOrEmpty(name) && _events.ContainsKey(name);

        private readonly Dictionary<string, ChestMotion.Handle> _motions = new Dictionary<string, ChestMotion.Handle>();

        /// <summary>
        /// Chuyển động cả rương đã được "loan báo" bằng <see cref="ChestStep.announce"/> (null nếu chưa có). Âm thanh dùng để biết
        /// thời lượng và biên độ THẬT của chuyển động (cùng một dòng thời gian). Được ghi TRƯỚC khi sự kiện cùng tên phát ra.
        /// </summary>
        public ChestMotion.Handle GetMotion(string announceName)
        {
            ChestMotion.Handle h;
            return !string.IsNullOrEmpty(announceName) && _motions.TryGetValue(announceName, out h) ? h : null;
        }

        internal void RegisterMotion(string announceName, ChestMotion.Handle h)
        {
            if (!string.IsNullOrEmpty(announceName)) _motions[announceName] = h;
        }

        /// <summary>Phát sự kiện (một lần là đủ: sự kiện được nhớ, bước đứng chờ sau đó vẫn thấy).</summary>
        public void Raise(string name)
        {
            if (string.IsNullOrEmpty(name) || _events.ContainsKey(name)) return;
            _events[name] = Time.time;
            Reveal.NotifyScriptEvent(name);
        }

        /// <summary>Khe nhận vật mang mã <paramref name="gateId"/> trên rương, hoặc null.</summary>
        public ChestItemSlot FindSlot(string gateId)
        {
            foreach (ChestItemSlot s in Reveal.GetComponentsInChildren<ChestItemSlot>(true))
            {
                if (s.gateId == gateId) return s;
            }
            return null;
        }

        internal void ClearEvents() { _events.Clear(); _motions.Clear(); }
    }

    /// <summary>
    /// Chạy một <see cref="ChestScript"/>: mỗi bước một coroutine, chờ điều kiện bắt đầu (bước trước, sự kiện, giai đoạn rương...), rồi làm việc.
    /// Mọi coroutine chạy trên <see cref="ChestRevealSequence"/> nên bị dừng cùng ResetReveal.
    /// Quyết định "bước nào chạy" (bật/tắt, bậc, xác suất) làm MỘT LẦN ở <see cref="Prepare"/> theo hạt giống, không phụ thuộc thời gian.
    /// </summary>
    public class ChestScriptRunner
    {
        private ChestScript _script;
        private ChestScriptContext _ctx;
        private MonoBehaviour _host;
        private bool[] _skip;
        private bool[] _started;
        private bool[] _done;
        private readonly HashSet<string> _warnedCues = new HashSet<string>();

        /// <summary>true nếu sau Prepare còn một bước OpenBox sẽ chạy: Play không được tự mở rương.</summary>
        public bool WillOpenBox { get; private set; }

        /// <summary>Quyết định bước nào chạy. <paramref name="enabledOverride"/>: bật/tắt theo id từ code (ưu tiên hơn ô enabled).</summary>
        public void Prepare(ChestScript script, ChestScriptContext ctx, Dictionary<string, bool> enabledOverride)
        {
            _script = script;
            _ctx = ctx;
            int n = script != null ? script.steps.Count : 0;
            _skip = new bool[n];
            _started = new bool[n];
            _done = new bool[n];
            WillOpenBox = false;
            System.Random roll = new System.Random(ctx.Seed ^ 0x5bd1e995);
            for (int i = 0; i < n; i++)
            {
                ChestStep s = script.steps[i];
                double r = roll.NextDouble();   // luôn tung đủ một số mỗi bước, để bật/tắt bước khác không làm lệch kết quả tung của bước này
                bool enabled = s != null && s.enabled;
                if (s != null && !string.IsNullOrEmpty(s.id) && enabledOverride != null && enabledOverride.TryGetValue(s.id, out bool o)) enabled = o;
                bool run = enabled && s.AppliesTo(ctx.Tier) && (s.chance >= 1f || r < s.chance);
                _skip[i] = !run;
                if (run && s.kind == StepKind.OpenBox) WillOpenBox = true;
            }
        }

        public void Begin(MonoBehaviour host)
        {
            _host = host;
            for (int i = 0; i < _skip.Length; i++) host.StartCoroutine(RunStep(i));
        }

        private IEnumerator RunStep(int i)
        {
            ChestStep s = _script.steps[i];
            if (_skip[i]) { _started[i] = true; _done[i] = true; yield break; }   // bỏ qua = xong ngay, không chặn bước sau

            // 1) chờ điều kiện bắt đầu
            switch (s.start)
            {
                case StepStart.AfterPrevious:
                    while (i > 0 && !_done[i - 1]) yield return null;
                    break;
                case StepStart.WithPrevious:
                    while (i > 0 && !_started[i - 1]) yield return null;
                    break;
                default:
                    string ev = StartEventName(s);
                    while (!_ctx.HasEvent(ev)) yield return null;
                    break;
            }
            if (s.delay > 0f) yield return new WaitForSeconds(s.delay);

            _started[i] = true;
            // Chuyển động cả rương tự loan báo ngay sau khi tạo bộ điều khiển (xem Execute); các bước khác loan báo ở đây.
            if (s.kind != StepKind.ChestMotion && !string.IsNullOrEmpty(s.announce)) _ctx.Raise(s.announce);
            yield return Execute(s, i);
            _done[i] = true;
        }

        private static string StartEventName(ChestStep s)
        {
            switch (s.start)
            {
                case StepStart.OnPress: return ChestEvents.Press;
                case StepStart.OnStage: return ChestEvents.Stage(s.startStage);
                case StepStart.OnRoulette: return ChestEvents.Roulette;
                case StepStart.OnLock: return ChestEvents.Lock;
                case StepStart.OnSilhouetteComplete: return ChestEvents.SilhouetteComplete;
                case StepStart.OnRevealed: return ChestEvents.Revealed;
                default: return s.startEvent;
            }
        }

        // ------------------------------------------------------------------ Thực thi từng loại bước

        private IEnumerator Execute(ChestStep s, int index)
        {
            System.Random rng = _ctx.RngFor(index);   // riêng từng bước: trang trí lặp lại được theo hạt
            switch (s.kind)
            {
                case StepKind.Wait:
                    yield return new WaitForSeconds(Mathf.Max(0f, s.duration));
                    break;

                case StepKind.Sound:
                    if (s.clip != null)
                    {
                        float pitch = Mathf.Lerp(s.pitchMin, s.pitchMax, (float)rng.NextDouble());
                        _ctx.Reveal.PlayOneShot(s.clip, s.volume, pitch);
                    }
                    if (s.duration > 0f) yield return new WaitForSeconds(s.duration);
                    break;

                case StepKind.Spawn:
                    yield return RunSpawn(s);
                    break;

                case StepKind.ChestMotion:
                {
                    float ampMul = s.ampUnit == AmpUnit.PerHalfMeterSize ? _ctx.ChestSize / 0.5f
                        : s.ampUnit == AmpUnit.ChestHeights ? _ctx.ChestHeight : 1f;
                    float dur = s.duration;
                    if (s.durationUntilStage >= 0)
                    {
                        // Nối liền với giai đoạn kế: thời lượng = phần còn lại tới giai đoạn đó theo kế hoạch của hộp (+ phần chồng).
                        float left = _ctx.Reveal.SecondsUntilStage(s.durationUntilStage);
                        if (left > 0f) dur = Mathf.Max(0.4f, left + Mathf.Max(0f, s.durationTail));
                    }
                    ChestMotion.Handle h = _ctx.Motion.Run(s.motion, s.amplitude * ampMul, s.frequency, dur, s.axis, s.holdEnd, 0f, _ctx.Seed + index);
                    if (!string.IsNullOrEmpty(s.announce))
                    {
                        _ctx.RegisterMotion(s.announce, h);   // ghi trước, rồi mới phát sự kiện: hook đọc được thời lượng thật
                        _ctx.Raise(s.announce);
                    }
                    if (s.waitForEnd) while (!h.Done) yield return null;
                    break;
                }

                case StepKind.ChestVisual:
                    switch (s.visual)
                    {
                        case StepVisualAction.Hide: _ctx.Motion.SetVisible(false); break;
                        case StepVisualAction.Show: _ctx.Motion.SetVisible(true); break;
                        default: _ctx.Motion.ReplaceWith(s.prefab, s.scale, s.offset); break;
                    }
                    break;

                case StepKind.PhysicsBurst:
                    yield return RunBurst(s, rng);
                    break;

                case StepKind.Explosion:
                    ChestPhysicsFx.Explode(_ctx.AnchorPosition(s.anchor, s.offset), s.explosionForce, s.explosionRadius, s.explosionUpwards, _ctx.Scope.Bodies, s.affectScene);
                    break;

                case StepKind.Gate:
                    yield return RunGate(s);
                    break;

                case StepKind.OpenBox:
                    _ctx.Reveal.BeginBox();
                    break;

                case StepKind.Cue:
                    yield return RunCue(s);
                    break;

                case StepKind.RaiseEvent:
                    _ctx.Raise(s.cue);
                    break;
            }
        }

        private IEnumerator RunSpawn(ChestStep s)
        {
            if (s.prefab == null) yield break;
            bool flies = s.path != null && s.path.Length > 0;
            Vector3 basePos = _ctx.AnchorPosition(s.anchor, flies ? Vector3.zero : s.offset);
            Vector3 pos = flies ? basePos + _ctx.Chest.rotation * s.path[0] : basePos;
            GameObject go = UnityEngine.Object.Instantiate(s.prefab, pos, _ctx.Chest.rotation);
            go.transform.localScale = go.transform.localScale * Mathf.Max(0.001f, s.scale);
            _ctx.Scope.Add(go);
            float life = s.lifetime > 0f ? s.lifetime : 8f;
            if (flies) life = Mathf.Max(life, s.flyDuration + 0.5f);
            UnityEngine.Object.Destroy(go, life);

            if (!flies)
            {
                if (s.waitForEnd) yield return new WaitForSeconds(Mathf.Max(0f, s.duration));
                yield break;
            }

            Vector3[] pts = new Vector3[s.path.Length];
            for (int k = 0; k < pts.Length; k++) pts[k] = basePos + _ctx.Chest.rotation * s.path[k];
            IEnumerator fly = ChestPhysicsFx.Fly(go.transform, pts, s.flyDuration, s.faceMotion);
            if (s.waitForEnd) yield return _host.StartCoroutine(fly);
            else _host.StartCoroutine(fly);
        }

        private IEnumerator RunBurst(ChestStep s, System.Random rng)
        {
            ChestPhysicsFx.BodySpec spec = new ChestPhysicsFx.BodySpec
            {
                shape = s.shape,
                prefab = s.prefab,
                color = s.bodyColor,
                size = s.bodySize,
                mass = s.mass,
                bounciness = s.bounciness,
                lifetime = s.bodyLifetime,
                ignoreAtStart = _ctx.ChestColliders,
            };
            int total = Mathf.Max(0, s.count + s.countPerTier * _ctx.Tier.Rank());
            Vector3 origin = _ctx.AnchorPosition(s.anchor, s.offset);
            Vector3 up = _ctx.Chest.up;

            if (s.spread <= 0.01f || total <= 1)
            {
                ChestPhysicsFx.Burst(_ctx.Scope, spec, total, origin, up, s.coneAngle, s.speedMin, s.speedMax, s.spin, rng);
                yield break;
            }
            float gap = s.spread / total;
            for (int k = 0; k < total; k++)
            {
                origin = _ctx.AnchorPosition(s.anchor, s.offset);   // theo rương nếu rương đang nhảy/rung
                ChestPhysicsFx.Burst(_ctx.Scope, spec, 1, origin, _ctx.Chest.up, s.coneAngle, s.speedMin, s.speedMax, s.spin, rng);
                yield return new WaitForSeconds(gap);
            }
        }

        private IEnumerator RunGate(ChestStep s)
        {
            ChestItemSlot slot = _ctx.FindSlot(s.gateId);
            if (slot == null)
            {
                Debug.LogWarning($"[ChestFx] Bước Gate '{s.id}': rương không có ChestItemSlot mã '{s.gateId}'; coi như hết giờ để kịch bản chạy tiếp.", _ctx.Reveal);
                _ctx.Raise(ChestEvents.GateTimeout(s.gateId));
                _ctx.Raise(ChestEvents.GateDone(s.gateId));
                yield break;
            }

            UnityAction<string> onWrong = _ => _ctx.Raise(ChestEvents.GateWrong(s.gateId));
            slot.onWrong.AddListener(onWrong);
            slot.Open();
            float waited = 0f;
            while (!slot.IsCompleted && (s.timeoutSeconds <= 0f || waited < s.timeoutSeconds))
            {
                waited += Time.deltaTime;
                yield return null;
            }
            slot.onWrong.RemoveListener(onWrong);
            slot.Close();
            _ctx.Raise(slot.IsCompleted ? ChestEvents.GateOk(s.gateId) : ChestEvents.GateTimeout(s.gateId));
            _ctx.Raise(ChestEvents.GateDone(s.gateId));
        }

        private IEnumerator RunCue(ChestStep s)
        {
            bool handled = false;
            foreach (ChestFxHook hook in _ctx.Reveal.Hooks)
            {
                if (hook == null) continue;
                IEnumerator e = hook.OnCue(s.cue, _ctx);
                if (e == null) continue;
                handled = true;
                yield return _host.StartCoroutine(e);
            }
            if (!handled && _warnedCues.Add(s.cue))
            {
                Debug.LogWarning($"[ChestFx] Cue '{s.cue}' không có hook nào xử lý (ChestFxHook.OnCue trả null ở mọi hook). Bước '{s.id}' bỏ qua.", _ctx.Reveal);
            }
        }
    }
}

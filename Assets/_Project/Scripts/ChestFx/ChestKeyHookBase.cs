using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Hook chung cho 17 rương (kế hoạch 2026-11-26, mục 2, 3, 4.1). Làm đúng 3 việc: (1) vật mở khoá bay tới khe rồi làm
    /// một hành động mở rồi biến mất trong 1,35 s; (2) âm nền dâng dần; (3) tiếng chốt theo bậc. Không sinh thêm vật nào,
    /// không Rigidbody/hạt/Light/UnityEngine.Random. Mọi chuyển động tính bằng thời gian thật (unscaled).
    /// Lớp con chỉ viết <see cref="GetPreset"/> và <see cref="BuildKey"/>; tuỳ chọn <see cref="AnimateAction"/> và 3 móc theme
    /// <see cref="ThemeFly"/>, <see cref="ThemeAction"/>, <see cref="OnKeyImpact"/> để bay/mở/nháy sáng đúng chủ đề.
    /// </summary>
    public abstract class ChestKeyHookBase : ChestFxHook
    {
        public const float FlightTime = 1.0f;
        public const float ActionTime = 0.35f;
        public const string CueName = "chia_khoa";
        public const string DoneEvent = "chia_khoa_xong";

        [Header("Âm")]
        public AudioClip bedClip;
        public AudioClip keyClip;
        [Tooltip("Tiếng BAY theo chủ đề, phát lúc vật bắt đầu bay (0 → 1,0 s), tắt hẳn khi hành động mở bắt đầu.")]
        public AudioClip flyClip;
        [Tooltip("Tiếng HÀNH ĐỘNG MỞ theo chủ đề, phát lúc 1,0 s. Rỗng thì dùng keyClip.")]
        public AudioClip actionClip;
        [Tooltip("Tiếng chốt theo bậc: Rác, Thường, Hiếm, Rất hiếm, Huyền thoại.")]
        public AudioClip[] stingClips = new AudioClip[5];
        [Range(0f, 1f)] public float volume = 0.8f;

        [Header("Vật mở khoá")]
        [Tooltip("Cỡ vật so với cỡ rương S.")]
        public float keyScale = 0.35f;
        public Color keyColor = Color.white;

        [Header("Khe (chỉnh tay nếu một rương vẫn lệch)")]
        [Tooltip("Dời điểm khe, đơn vị mét, theo trục của rương: x phải, y lên, z ra trước. Mặc định 0 = giữa mặt trước thân, cao 0,42 H.")]
        public Vector3 slotOffset = Vector3.zero;
        [Tooltip("Vẽ điểm khe (vàng) và hướng trước (xanh) trong Scene.")]
        public bool debugDrawSlot = false;

        private static readonly float[] StingVolume = { 0.55f, 0.7f, 0.8f, 0.9f, 1.0f };

        private ChestRevealSequence _reveal;
        private AudioSource _bed;
        private AudioSource _fly;
        private KeyContext _kc;
        private GameObject _keyRoot;
        private int _generation;
        private bool _bedOn;
        private bool _locked;
        private float _pressTime;
        private float _doneTime = -1f;
        private float _preDoneVol = 0.25f;

        /// <summary>Thông số của rương này (hành động mở, màu, tệp âm...).</summary>
        public abstract KeyPreset GetPreset();

        /// <summary>
        /// Dựng vật mở khoá. Quy ước: gốc ở giữa, trục +Z là hướng "đầu/mũi/mặt" quay về rương, vừa hộp 1x1x1.
        /// Hook tự đặt vị trí và nhân keyScale·S. Dùng <paramref name="c"/>.Part để tạo khối (≥ 4 khối).
        /// </summary>
        protected abstract GameObject BuildKey(KeyContext c);

        /// <summary>
        /// Chuyển động riêng của chính vật (vẫy đuôi, vỗ cánh, xoay...): xoay/tịnh tiến các khối con của <paramref name="root"/>.
        /// Được gọi MỖI KHUNG ở cả hai giai đoạn: bay (0 → 1,0 s) rồi hành động mở (1,0 → 1,35 s). <paramref name="t01"/> chạy 0..1
        /// TRONG giai đoạn đang chạy (khung đầu của mỗi giai đoạn ~0, khung cuối ~1); phân biệt giai đoạn bằng <see cref="InActionPhase"/>.
        /// Không được sinh thêm vật.
        /// </summary>
        protected virtual void AnimateAction(KeyContext c, Transform root, float t01) { }

        /// <summary>
        /// MÓC THEME 1 — kiểu bay riêng (xoay tít, nhảy cóc, lướt, lăn...). Trả null (mặc định) = bay vòng cung Bezier chuẩn.
        /// Nếu trả coroutine: nó tự đặt vị trí/xoay của <paramref name="root"/> mỗi khung trong <paramref name="seconds"/> giây
        /// (thời gian thật), gọi c.Refresh() mỗi khung vì rương có thể đang bay/xoay, và kết thúc với gốc vật ở gần c.RestPos
        /// (hoặc điểm chờ của hành động). Bị cắt cứng sau seconds + 0,05 s. AnimateAction KHÔNG được gọi trong lúc này.
        /// </summary>
        protected virtual IEnumerator ThemeFly(KeyContext c, Transform root, float seconds) { return null; }

        /// <summary>
        /// MÓC THEME 2 — hành động mở riêng theo chủ đề (xoay, đập, mổ, vồ, bật nắp, nhào lộn...). Trả null (mặc định) = hành
        /// động chuẩn theo preset.openAction. Nếu trả coroutine: nó THAY HOÀN TOÀN 0,35 s hành động, tự lo chuyển động của vật và
        /// các khối con, phải xong trong <paramref name="seconds"/> (bị cắt cứng sau seconds + 0,05 s) và để vật ở đúng khe
        /// (c.RestPos). Nên gọi <see cref="ShrinkAtSlot"/> ở cuối để vật thu nhỏ biến mất tại khe; hết thì lõi xoá vật.
        /// </summary>
        protected virtual IEnumerator ThemeAction(KeyContext c, Transform root, float seconds) { return null; }

        /// <summary>
        /// MÓC THEME 3 — gọi đúng lúc hành động mở chạm khe (t = 1,0 s), cùng lúc phát tiếng hành động. Mặc định nháy glow
        /// miệng rương theo preset.lightColor/lightMul trong 0,5 s. Lớp con ghi đè để đổi màu/nhịp (có thể gọi base).
        /// </summary>
        protected virtual void OnKeyImpact(KeyContext c)
        {
            if (Reveal != null && c.preset.lightMul > 0f) Reveal.PulseGlow(c.preset.lightColor, c.preset.lightMul, 0.5f);
        }

        /// <summary>Rương đang mở (để lớp con gọi PulseGlow, PlayOneShot...). Null trước OnPress.</summary>
        protected ChestRevealSequence Reveal { get { return _reveal; } }

        /// <summary>
        /// Tiện ích cho ThemeAction: đặt gốc vật sao cho ĐẦU đứng ở khe và thu nhỏ chuẩn (0 đến 0,85 giữ nguyên, 0,85 → 1 về 0).
        /// <paramref name="t01"/> là tiến độ hành động 0..1.
        /// </summary>
        protected static void ShrinkAtSlot(KeyContext c, Transform root, float t01)
        {
            float shrink = Sm(0.85f, 1f, t01);
            root.position = c.slotPos + c.front * (c.tip * (1f - shrink));
            root.localScale = Vector3.one * (c.keySize * (1f - shrink));
        }

        /// <summary>true khi đang ở giai đoạn hành động mở (1,0 → 1,35 s); false khi đang bay.</summary>
        protected bool InActionPhase { get; private set; }

        /// <summary>Hình mặc định (thân trụ + đầu tròn + 2 răng, +Z là đầu/mũi) để dự án luôn biên dịch.</summary>
        protected static GameObject DefaultKey(KeyContext c, Color color)
        {
            GameObject root = new GameObject("Key");
            Quaternion alongZ = Quaternion.Euler(90f, 0f, 0f);   // trụ Unity mặc định dọc Y; xoay để nằm dọc Z
            c.Part(root.transform, PrimitiveType.Cylinder, "Than", new Vector3(0f, 0f, -0.1f), new Vector3(0.12f, 0.3f, 0.12f), alongZ, color);
            c.Part(root.transform, PrimitiveType.Sphere, "Dau", new Vector3(0f, 0f, 0.38f), new Vector3(0.3f, 0.3f, 0.12f), color);
            c.Part(root.transform, PrimitiveType.Cube, "Rang1", new Vector3(0.09f, 0f, -0.34f), new Vector3(0.14f, 0.1f, 0.1f), color);
            c.Part(root.transform, PrimitiveType.Cube, "Rang2", new Vector3(0.09f, 0f, -0.2f), new Vector3(0.11f, 0.1f, 0.1f), color);
            return root;
        }

        // ------------------------------------------------------------------ callback của lõi

        public override void OnPress(ChestRevealSequence reveal)
        {
            _reveal = reveal;
            _locked = false;
            _doneTime = -1f;
            _pressTime = Time.unscaledTime;
            _preDoneVol = 0.25f;
            StopBed();

            AudioSource chestSrc = reveal.GetComponent<AudioSource>();
            GameObject go = new GameObject("KeyBed");
            go.transform.SetParent(reveal.transform, false);
            _bed = go.AddComponent<AudioSource>();
            _bed.playOnAwake = false;
            _bed.loop = true;
            _bed.clip = bedClip;
            _bed.spatialBlend = chestSrc != null ? chestSrc.spatialBlend : 0.5f;
            _bed.outputAudioMixerGroup = chestSrc != null ? chestSrc.outputAudioMixerGroup : null;
            _bed.pitch = 0.9f;
            _bed.volume = 0.25f * volume;
            if (bedClip != null) _bed.Play();
            _bedOn = true;
        }

        public override IEnumerator OnCue(string cue, ChestScriptContext context)
        {
            if (cue != CueName) return null;
            return KeyRoutine(context);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_locked) return;
            _locked = true;
            int rank = Mathf.Clamp((int)tier, 0, 4);
            if (rank == 0)
            {
                StopBed();
                StartCoroutine(RacSting(reveal));
            }
            else
            {
                StartCoroutine(FadeBedOut(0.3f));
                PlaySting(reveal, rank);
            }
        }

        public override void OnReset(ChestRevealSequence reveal) { CleanUp(); }

        private void OnDestroy() { CleanUp(); }

        private void Update()
        {
            if (!_bedOn || _locked || _bed == null) return;
            float now = Time.unscaledTime;
            if (_doneTime < 0f)
            {
                // Lúc chìa đang bay/mở: dâng nhẹ 0,25 → 0,30.
                float k = Mathf.Clamp01((now - _pressTime) / (FlightTime + ActionTime));
                _preDoneVol = Mathf.Lerp(0.25f, 0.30f, k);
                _bed.volume = _preDoneVol * volume;
                _bed.pitch = 0.9f;
            }
            else
            {
                // Sau chia_khoa_xong: ~5 s dâng cao độ 0,9 → 1,3 và âm lượng tới 0,45 (nhân volume đúng một lần), rồi giữ.
                float u = Mathf.Clamp01((now - _doneTime) / 5f);
                u = u * u * (3f - 2f * u);
                _bed.volume = Mathf.Lerp(_preDoneVol, 0.45f, u) * volume;
                _bed.pitch = Mathf.Lerp(0.9f, 1.3f, u);
            }
        }

        // ------------------------------------------------------------------ tiếng chốt

        private IEnumerator RacSting(ChestRevealSequence reveal)
        {
            float t0 = Time.unscaledTime;
            while (Time.unscaledTime - t0 < 0.2f) yield return null;
            PlaySting(reveal, 0);
        }

        private IEnumerator FadeBedOut(float dur)
        {
            AudioSource src = _bed;
            if (src == null) yield break;
            float v0 = src.volume;
            float t0 = Time.unscaledTime;
            while (src != null && Time.unscaledTime - t0 < dur)
            {
                src.volume = v0 * (1f - (Time.unscaledTime - t0) / dur);
                yield return null;
            }
            if (src == _bed) StopBed();
        }

        private void PlaySting(ChestRevealSequence reveal, int rank)
        {
            if (reveal == null || stingClips == null || rank >= stingClips.Length) return;
            reveal.PlayOneShot(stingClips[rank], StingVolume[rank] * volume);
        }

        private void StopBed()
        {
            _bedOn = false;
            if (_bed != null)
            {
                _bed.Stop();
                Destroy(_bed.gameObject);
            }
            _bed = null;
            StopFly();
        }

        private void StopFly()
        {
            if (_fly != null)
            {
                _fly.Stop();
                Destroy(_fly.gameObject);
            }
            _fly = null;
        }

        private void PlayFly()
        {
            StopFly();
            if (flyClip == null || _reveal == null) return;
            AudioSource chestSrc = _reveal.GetComponent<AudioSource>();
            GameObject go = new GameObject("KeyFly");
            go.transform.SetParent(_reveal.transform, false);
            _fly = go.AddComponent<AudioSource>();
            _fly.playOnAwake = false;
            _fly.loop = false;
            _fly.clip = flyClip;
            _fly.spatialBlend = chestSrc != null ? chestSrc.spatialBlend : 0.5f;
            _fly.outputAudioMixerGroup = chestSrc != null ? chestSrc.outputAudioMixerGroup : null;
            _fly.volume = 0.6f * volume;
            _fly.Play();
        }

        /// <summary>Chạy coroutine theme, cắt cứng sau limit giây; trả false nếu bị huỷ (reset) giữa chừng.</summary>
        private IEnumerator RunTheme(IEnumerator theme, int gen, GameObject root, float limit, bool[] ok)
        {
            float t0 = Time.unscaledTime;
            ok[0] = false;
            while (true)
            {
                if (gen != _generation || root == null) yield break;
                if (Time.unscaledTime - t0 > limit) break;
                bool more;
                try { more = theme.MoveNext(); }
                catch (System.Exception ex) { Debug.LogException(ex); break; }
                if (!more) break;
                yield return theme.Current;
            }
            ok[0] = gen == _generation && root != null;
        }

        private void CleanUp()
        {
            _generation++;
            StopAllCoroutines();
            StopBed();
            if (_keyRoot != null) Destroy(_keyRoot);
            _keyRoot = null;
            if (_kc != null) _kc.Dispose();
            _kc = null;
            _locked = false;
            _doneTime = -1f;
        }

        // ------------------------------------------------------------------ vật mở khoá

        private IEnumerator KeyRoutine(ChestScriptContext ctx)
        {
            int gen = _generation;
            KeyPreset preset = GetPreset();
            KeyContext c = new KeyContext(ctx, slotOffset);
            _kc = c;
            GameObject root = BuildKey(c);
            if (root == null) { c.Dispose(); _kc = null; ctx.Raise(DoneEvent); yield break; }
            _keyRoot = root;
            Transform tr = root.transform;

            float S = c.S, H = c.H;
            float size = keyScale * S;
            Vector3 baseScale = Vector3.one * size;
            KeyAction action = preset.openAction;
            // Gốc vật ở giữa, đầu +Z cách gốc 0,5·size: lùi gốc ra trước chừng ấy để ĐẦU chạm đúng khe (không chui vào thân).
            float tip = 0.5f * size;
            c.preset = preset;
            c.keySize = size;
            c.tip = tip;

            // Điểm khởi bay cố định trong thế giới; điểm đích bám rương mỗi khung (rương có thể đang lắc/cầm trên tay).
            Vector3 p0 = c.chestBase + c.front * (1.2f * S) + c.up * (0.6f * H);
            c.startPos = p0;
            tr.position = p0;
            tr.rotation = Quaternion.LookRotation(-c.front, c.up);
            tr.localScale = baseScale;

            // t = 0: tiếng bay theo chủ đề (nguồn riêng, tắt hẳn lúc 1,0 s → tối đa 3 lớp âm).
            PlayFly();

            // --- bay: móc theme hoặc vòng cung chuẩn
            InActionPhase = false;
            float t0 = Time.unscaledTime;
            bool bad = false;
            bool[] ok = new bool[1];
            IEnumerator themeFly = ThemeFly(c, tr, FlightTime);
            if (themeFly != null)
            {
                yield return RunTheme(themeFly, gen, root, FlightTime + 0.05f, ok);
                if (!ok[0]) yield break;
                // Bù giờ nếu móc xong sớm: giữ tổng 1,0 s.
                while (Time.unscaledTime - t0 < FlightTime)
                {
                    if (gen != _generation || root == null) yield break;
                    yield return null;
                }
            }
            else while (true)
            {
                if (gen != _generation || root == null) { bad = true; break; }
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / FlightTime);
                float e = u * u * (3f - 2f * u);
                float a = 1f - e;
                Vector3 p2 = HoverPos(action, c) + c.front * tip;
                float h0 = Vector3.Dot(p0, c.up), h2 = Vector3.Dot(p2, c.up);
                float peak = Mathf.Max(h0, h2) + 0.3f * S;
                Vector3 p1 = (p0 + p2) * 0.5f;
                p1 += c.up * (2f * peak - 0.5f * (h0 + h2) - Vector3.Dot(p1, c.up));
                tr.position = a * a * p0 + 2f * a * e * p1 + e * e * p2;
                tr.rotation = Quaternion.LookRotation(-c.front, c.up);
                AnimateAction(c, tr, u);
                if (u >= 1f) break;
                yield return null;
            }
            if (bad) yield break;

            // t = 1,0 s: hành động mở chạm khe → tắt tiếng bay, phát tiếng hành động (rỗng thì tiếng cắm cũ), nháy sáng chủ đề.
            StopFly();
            if (_reveal != null)
            {
                if (actionClip != null) _reveal.PlayOneShot(actionClip, 0.7f * volume);
                else _reveal.PlayOneShot(keyClip, 0.6f * volume);   // như cũ
            }
            try { OnKeyImpact(c); }
            catch (System.Exception ex) { Debug.LogException(ex); }

            // --- hành động mở: móc theme hoặc hành động chuẩn
            InActionPhase = true;
            t0 = Time.unscaledTime;
            IEnumerator themeAct = ThemeAction(c, tr, ActionTime);
            if (themeAct != null)
            {
                yield return RunTheme(themeAct, gen, root, ActionTime + 0.05f, ok);
                if (!ok[0]) { InActionPhase = false; yield break; }
                while (Time.unscaledTime - t0 < ActionTime)
                {
                    if (gen != _generation || root == null) { InActionPhase = false; yield break; }
                    yield return null;
                }
            }
            else while (true)
            {
                if (gen != _generation || root == null) { bad = true; break; }
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / ActionTime);
                c.Refresh();
                Vector3 pos; Quaternion rot; float shrink;
                ActionPose(action, u, c, HoverPos(action, c), Quaternion.LookRotation(-c.front, c.up), out pos, out rot, out shrink);
                // Thu nhỏ quanh ĐẦU vật: đầu đứng yên ở khe, gốc tiến dần vào khe → vật biến mất đúng tại khe.
                tr.position = pos + c.front * (tip * (1f - shrink));
                tr.rotation = rot;
                tr.localScale = baseScale * (1f - shrink);
                AnimateAction(c, tr, u);
                if (u >= 1f) break;
                yield return null;
            }
            InActionPhase = false;
            if (bad) yield break;

            Destroy(root);
            _keyRoot = null;
            c.Dispose();
            _kc = null;
            _doneTime = Time.unscaledTime;
            ctx.Raise(DoneEvent);
        }

        /// <summary>Điểm chờ trước hành động (chưa tính độ lùi đầu vật).</summary>
        private static Vector3 HoverPos(KeyAction action, KeyContext c)
        {
            float S = c.S;
            switch (action)
            {
                case KeyAction.Drop: return c.slotPos + c.up * (0.12f * S);
                case KeyAction.Strike: return c.slotPos + c.front * (0.25f * S);
                case KeyAction.Pounce: return c.slotPos + c.front * (0.12f * S) + c.up * (0.12f * S);
                case KeyAction.Peck: return c.slotPos + c.front * (0.15f * S);
                default: return c.slotPos;
            }
        }

        private void OnDrawGizmos()
        {
            if (!debugDrawSlot) return;
            KeyContext k = _kc;
            if (k == null)
            {
                ChestRevealSequence rv = _reveal != null ? _reveal : GetComponentInParent<ChestRevealSequence>();
                if (rv == null) rv = GetComponentInChildren<ChestRevealSequence>();
                if (rv == null) return;
                k = new KeyContext(rv.transform, rv.Box, slotOffset);
            }
            else k.Refresh();
            // Debug.DrawLine gọi trong OnDrawGizmos hiện ngay trong cửa sổ Scene (cả lúc không chạy).
            float r = Mathf.Max(0.01f, 0.04f * k.S);
            Vector3 right = Vector3.Cross(k.up, k.front);
            Debug.DrawLine(k.slotPos - right * r, k.slotPos + right * r, Color.yellow);
            Debug.DrawLine(k.slotPos - k.up * r, k.slotPos + k.up * r, Color.yellow);
            Debug.DrawLine(k.chestBase, k.slotPos, Color.yellow);
            Debug.DrawLine(k.slotPos, k.slotPos + k.front * (0.5f * k.S), Color.cyan);
            Debug.DrawLine(k.slotPos, k.slotPos + k.up * (0.25f * k.S), Color.green);
        }

        private static float Sm(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        /// <summary>Vị trí/xoay/độ thu nhỏ (0..1) của vật trong 0,35 s hành động. Cuối hành động luôn thu nhỏ về 0.</summary>
        private static void ActionPose(KeyAction action, float t, KeyContext c, Vector3 hover, Quaternion baseRot,
            out Vector3 pos, out Quaternion rot, out float shrink)
        {
            float S = c.S;
            Vector3 slot = c.slotPos;
            Vector3 dir = -c.front;
            pos = hover;
            rot = baseRot;
            shrink = Sm(0.85f, 1f, t);
            switch (action)
            {
                case KeyAction.Insert:
                {
                    // Đẩy sâu 0,1 S vào khe (0..0.5) rồi xoay 90° quanh +Z (0.4..0.85).
                    pos = slot + dir * (0.1f * S * Sm(0f, 0.5f, t));
                    rot = baseRot * Quaternion.Euler(0f, 0f, 90f * Sm(0.4f, 0.85f, t));
                    break;
                }
                case KeyAction.Drop:
                {
                    // hover cao 0,12 S: rơi (0..0.4), nảy 1 lần (0.4..0.7), nằm xuống rồi chui.
                    float y;
                    if (t < 0.4f) { float k = t / 0.4f; y = 0.12f * S * (1f - k * k); }
                    else if (t < 0.7f) y = 0.04f * S * Mathf.Sin(Mathf.PI * (t - 0.4f) / 0.3f);
                    else y = 0f;
                    pos = slot + c.up * y;
                    break;
                }
                case KeyAction.Strike:
                {
                    // hover lùi 0,25 S: lao vào (0..0.3), bật lùi 0,05 S (0.3..0.55), rồi chui vào (0.55..0.85).
                    float off;
                    if (t < 0.3f) off = Mathf.Lerp(0.25f, 0f, Sm(0f, 0.3f, t)) * S;
                    else if (t < 0.55f) off = 0.05f * S * Sm(0.3f, 0.55f, t);
                    else off = Mathf.Lerp(0.05f, -0.04f, Sm(0.55f, 0.85f, t)) * S;
                    pos = slot - dir * off;
                    break;
                }
                case KeyAction.Pounce:
                {
                    // nhảy vồ từ hover lên khe (0..0.35), đè sát, ngoáy 2 nhịp (0.35..0.85).
                    float e = Sm(0f, 0.35f, t);
                    pos = Vector3.Lerp(hover, slot, e) + c.up * (0.1f * S * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 0.35f)));
                    float wig = t > 0.35f ? Mathf.Sin(Mathf.PI * 4f * Mathf.Clamp01((t - 0.35f) / 0.5f)) * 18f : 0f;
                    rot = baseRot * Quaternion.Euler(0f, wig, 0f);
                    break;
                }
                default: // Peck
                {
                    // hover lùi 0,15 S: mổ 2 lần (0..0.7), rồi đẩy vào (0.7..0.85).
                    float off;
                    if (t < 0.7f) off = 0.15f * S * (0.5f + 0.5f * Mathf.Cos(Mathf.PI * 4f * (t / 0.7f)));
                    else off = 0.15f * S * (1f - Sm(0.7f, 0.85f, t));
                    pos = slot - dir * off;
                    break;
                }
            }
        }
    }
}

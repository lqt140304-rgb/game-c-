using System;
using System.Collections;
using System.Collections.Generic;
using ChoDoLa.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Shared core of the chest-opening effect (docs/superpowers/specs/2026-11-24-hieu-ung-mo-rong-design.md).
    /// Put it next to <see cref="BoxOpenSequence"/> on the chest root, call <see cref="Setup"/> with the tiers this
    /// chest can give, then <see cref="Play"/> with the tier THE GAME rolled and the won item's model.
    ///
    /// (a) The colours of the possible tiers flash in turn (slot machine), slow down and stop on the rolled tier.
    /// (b) Meanwhile the item's silhouette rises out of the chest, grows to full size and floats above it.
    /// (c) Sounds, particles, ray style and custom hooks come from the chest's <see cref="ChestFxProfile"/>.
    ///
    /// The result is never random here: <c>rolledTier</c> is always where the roulette stops. Randomness is only
    /// used for decoration (shuffle order, near miss, accent timing).
    /// Timing follows BoxOpenSequence.onStageReached (default: roulette from "He nap", lock at "Bat nap",
    /// silhouette full by "Mo toang"). Without a BoxOpenSequence it runs on its own clock.
    /// </summary>
    [DisallowMultipleComponent]
    public class ChestRevealSequence : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("Tier colours and shape cues. Empty = proposed defaults.")]
        [SerializeField] private TierPalette palette;
        [Tooltip("This chest's sounds and particle style. Empty = silent default.")]
        [SerializeField] private ChestFxProfile profile;
        [Tooltip("Tiers this chest can give (the roulette colours). Setup() overwrites it.")]
        [SerializeField] private List<ChestTier> possibleTiers = new List<ChestTier> { ChestTier.Rac, ChestTier.Thuong, ChestTier.Hiem, ChestTier.RatHiem, ChestTier.HuyenThoai };

        [Header("Timing with BoxOpenSequence (stage index: 0 Rung, 1 Go niem, 2 He nap, 3 Bat nap, 4 Mo toang)")]
        [Tooltip("Empty = the BoxOpenSequence on this object.")]
        [SerializeField] private BoxOpenSequence box;
        [Tooltip("Play() also starts the BoxOpenSequence.")]
        [SerializeField] private bool startBoxOnPlay = true;
        [Tooltip("Stage that starts the roulette and the silhouette (-1 = as soon as Play is called).")]
        [SerializeField] private int rouletteStartStage = 2;
        [Tooltip("Stage that locks the colour on the rolled tier (flash, rays, lock sound) and pops the silhouette out.")]
        [SerializeField] private int lockStage = 3;
        [Tooltip("Stage by which the silhouette reaches full size.")]
        [SerializeField] private int silhouetteCompleteStage = 4;
        [Tooltip("Minimum growth time after the lock; the lock stage hold is stretched if needed.")]
        [SerializeField] private float minGrowSeconds = 1.1f;

        [Header("Timing without BoxOpenSequence")]
        [SerializeField] private float standaloneRouletteSeconds = 2.8f;
        [SerializeField] private float standaloneGrowSeconds = 1.4f;

        [Header("Silhouette")]
        [Tooltip("Largest side of the finished silhouette in metres (measured from the item's renderer bounds).")]
        [SerializeField] private float targetSize = 0.3f;
        [SerializeField] private SilhouetteStyle silhouetteStyle = SilhouetteStyle.DarkTintWithRim;
        [Range(0f, 0.2f), Tooltip("Rim thickness as a fraction of the item size.")]
        [SerializeField] private float rimWidth = 0.05f;
        [Range(0f, 1f), Tooltip("Fill brightness: tier colour multiplied by this.")]
        [SerializeField] private float fillBrightness = 0.16f;
        [Range(0.001f, 1f)] [SerializeField] private float startScale = 0.04f;
        [Range(0.01f, 1f), Tooltip("Scale reached at the chest mouth while the roulette runs.")]
        [SerializeField] private float preLockScale = 0.22f;
        [Tooltip("How high the finished silhouette floats: its bottom sits this many chest-heights above the mouth.")]
        [SerializeField] private float hoverAboveMouth = 0.5f;
        [Tooltip("Degrees per second.")]
        [SerializeField] private float spinSpeed = 30f;
        [SerializeField] private float bobAmplitude = 0.01f;
        [Range(0f, 1f), Tooltip("Ghostly flicker of the rim (0 = off).")]
        [SerializeField] private float flicker = 0.25f;
        [Tooltip("Pause at full size before onSilhouetteComplete.")]
        [SerializeField] private float settleSeconds = 0.3f;

        [Header("Light, glow and cues")]
        [Tooltip("Point light inside the chest; created at runtime if empty.")]
        [SerializeField] private Light revealLight;
        [Tooltip("Base light intensity while the roulette runs.")]
        [SerializeField] private float rouletteLightIntensity = 1.2f;
        [Tooltip("Tint the \"Glow\" child of the chest with the roulette colour.")]
        [SerializeField] private bool tintGlow = true;
        [SerializeField] private float glowEmissionBoost = 2.5f;
        [Tooltip("Small orbs orbiting the chest: their number = tier rank (1 Rác .. 5 Huyền thoại). Colour-blind cue.")]
        [SerializeField] private bool showRankPips = true;

        [Header("Kịch bản (ChestScript)")]
        [Tooltip("Ghi đè kịch bản của hồ sơ (profile.script). Để trống = dùng kịch bản của hồ sơ; cả hai trống = rương chạy như cũ.")]
        [SerializeField] private ChestScript scriptOverride;

        [Header("Audio")]
        [Tooltip("Empty = the AudioSource on this object, or one is added.")]
        [SerializeField] private AudioSource audioSource;

        [Header("Decoration randomness (never affects the result)")]
        [Tooltip("0 = different decoration every time.")]
        [SerializeField] private int decorationSeed;

        [Header("Events")]
        public UnityEvent onRoulette;                    // the roulette starts
        public UnityEvent<ChestTier> onRouletteStep;     // every colour change
        public UnityEvent<ChestTier> onTierLocked;       // stopped on the rolled tier
        public UnityEvent onSilhouetteComplete;          // silhouette at full size: swap in the real item here
        public UnityEvent onRevealed;                    // box open and silhouette complete
        public UnityEvent onPress;                       // Play() vừa được gọi (bấm mở): âm căng thẳng bắt đầu cùng lúc
        public UnityEvent<string> onScriptEvent;         // mọi sự kiện kịch bản (xem ChestEvents), kể cả sự kiện tự đặt

        // ------------------------------------------------------------------ State

        private struct Step
        {
            public float time;
            public ChestTier tier;
        }

        private enum SilPhase { None, Rising, Popping, Settling, Done }

        private bool _ready;
        private bool _active;
        private bool _rouletteStarted;
        private bool _locked;
        private bool _boxOpened;
        private bool _silComplete;
        private bool _revealed;
        private ChestTier _rolled;
        private ChestTier _shown;
        private System.Random _rng;

        private Camera _camera;
        private Transform _fxRoot;
        private Vector3 _mouthLocal;      // chest mouth in chest-local space
        private float _chestHeight = 0.4f;
        private float _chestSize = 0.5f;
        private Renderer[] _glowRenderers = new Renderer[0];
        private MaterialPropertyBlock _mpb;

        private ParticleSystem _motes;
        private ParticleSystem _burst;
        private bool _createdLight;
        private float _lightBase;
        private float _lightKick;
        private readonly List<Transform> _pips = new List<Transform>();
        private Material _pipMaterial;
        private float _pipAngle;

        private ChestSilhouette _silhouette;
        private SilPhase _silPhase;
        private float _silPhaseStart, _silPhaseDuration;
        private Vector3 _silFromPos, _silToPos;
        private float _silFromScale, _silToScale;
        private float _silSpin;
        private float _silCompleteTime;

        private ChestTension _tension;
        private TensionResolution _resolution;
        private ChestScriptContext _ctx;
        private ChestScriptRunner _runner;
        private ChestFxScope _scope;
        private ChestMotion _motion;
        private bool _boxBegun;
        private readonly Dictionary<string, bool> _stepOverrides = new Dictionary<string, bool>();

        private AudioSource _tickSource;
        private GameObject _customFx;
        private readonly List<ChestFxHook> _hooks = new List<ChestFxHook>();
        private readonly Dictionary<int, float> _originalHolds = new Dictionary<int, float>();
        private readonly List<UnityEngine.Object> _owned = new List<UnityEngine.Object>();

        public ChestTier RolledTier => _rolled;
        public bool IsPlaying => _active && !_revealed;
        public bool IsLocked => _locked;
        public Transform Silhouette => _silhouette != null ? _silhouette.transform : null;
        public IReadOnlyList<ChestTier> PossibleTiers => possibleTiers;
        public TierPalette Palette => palette != null ? palette : TierPalette.Default;
        public ChestFxProfile Profile => profile != null ? profile : ChestFxProfile.Default;
        public BoxOpenSequence Box => box;

        /// <summary>Kịch bản đang dùng: scriptOverride, nếu trống thì kịch bản của hồ sơ, nếu trống nữa thì null.</summary>
        public ChestScript Script => scriptOverride != null ? scriptOverride : Profile.script;
        /// <summary>Âm căng thẳng của lần mở này (null nếu tắt hoặc chưa Play).</summary>
        public ChestTension Tension => _tension;
        /// <summary>Cách âm căng thẳng kết thúc (chỉ có nghĩa sau khi chốt).</summary>
        public TensionResolution Resolution => _resolution;
        /// <summary>Bối cảnh kịch bản của lần mở này (null trước khi Play).</summary>
        public ChestScriptContext Context => _ctx;
        public IReadOnlyList<ChestFxHook> Hooks => _hooks;
        public float ChestHeight => _chestHeight;
        public float ChestSize => _chestSize;

        /// <summary>
        /// Chuyển động cả rương. Tạo khi cần; nên gọi từ OnPress hoặc sớm hơn (nó ghi nhớ các Renderer của rương lúc được tạo để ẩn/hiện).
        /// </summary>
        public ChestMotion Motion
        {
            get
            {
                if (_motion == null)
                {
                    _motion = ChestMotion.GetOrAdd(gameObject);
                    _motion.CaptureVisuals();
                }
                return _motion;
            }
        }

        /// <summary>Bật/tắt một bước kịch bản theo id cho các lần Play sau (ví dụ bật bước thả đồng xu). Không sửa asset.</summary>
        public void SetStepEnabled(string stepId, bool enabled)
        {
            if (!string.IsNullOrEmpty(stepId)) _stepOverrides[stepId] = enabled;
        }

        /// <summary>Quên mọi SetStepEnabled.</summary>
        public void ClearStepOverrides() => _stepOverrides.Clear();

        /// <summary>Phát sự kiện kịch bản có tên (hook/game dùng để báo cho các bước đang chờ). Được nhớ cho tới ResetReveal.</summary>
        public void RaiseEvent(string eventName)
        {
            if (_ctx != null) _ctx.Raise(eventName);
        }

        internal void NotifyScriptEvent(string eventName)
        {
            onScriptEvent?.Invoke(eventName);
            foreach (ChestFxHook h in _hooks) if (h != null) h.OnScriptEvent(this, eventName);
        }

        /// <summary>World position of the chest mouth (where the glow sits).</summary>
        public Vector3 MouthPosition => transform.TransformPoint(_mouthLocal);

        // ------------------------------------------------------------------ Public API

        /// <summary>Tiers this chest can give. They become the roulette colours (sorted Rác → Huyền thoại, no duplicates).</summary>
        public void Setup(IList<ChestTier> tiers)
        {
            possibleTiers = new List<ChestTier>();
            if (tiers != null)
            {
                foreach (ChestTier t in tiers)
                {
                    if (!possibleTiers.Contains(t)) possibleTiers.Add(t);
                }
            }
            possibleTiers.Sort((a, b) => ((int)a).CompareTo((int)b));
        }

        /// <summary>Swap the profile (for example when the same component is reused for another chest).</summary>
        public void SetProfile(ChestFxProfile newProfile) => profile = newProfile;

        /// <summary>
        /// Start the reveal. <paramref name="rolledTier"/> is decided by the game (seeded roll); the roulette always
        /// stops on it. <paramref name="itemSource"/> is the won item's model (scene object or prefab asset): only its
        /// meshes are copied to make the silhouette; it is not modified.
        /// </summary>
        public void Play(ChestTier rolledTier, GameObject itemSource)
        {
            EnsureReady();
            if (_active) ResetReveal(false);

            _active = true;
            _rolled = rolledTier;
            _shown = rolledTier;
            _rng = new System.Random(decorationSeed != 0 ? decorationSeed : Environment.TickCount);

            if (!possibleTiers.Contains(rolledTier))
            {
                Debug.LogWarning($"[ChestFx] {name}: rolled tier {rolledTier} is not in this chest's possible tiers; added to the roulette.", this);
                possibleTiers.Add(rolledTier);
                possibleTiers.Sort((a, b) => ((int)a).CompareTo((int)b));
            }

            _camera = Camera.main;
            ChestScript script = Script;
            _boxBegun = false;
            // Ghi nhớ Renderer của rương TRƯỚC khi dựng hiệu ứng (để ẩn/hiện rương không đụng vào hiệu ứng).
            if (script != null && script.steps.Count > 0) _ = Motion;
            MeasureChest();
            BuildFx();
            BuildSilhouette(itemSource);
            SpawnCustomFx();

            // ÂM BẮT ĐẦU NGAY KHI BẤM MỞ: trước cả khi nắp động đậy và trước khi kịch bản chạy.
            StartTension();
            onPress?.Invoke();
            foreach (ChestFxHook h in _hooks) if (h != null) h.OnPress(this);

            _scope = new ChestFxScope();
            _ctx = new ChestScriptContext
            {
                Reveal = this,
                Tier = rolledTier,
                Scope = _scope,
                Seed = decorationSeed != 0 ? decorationSeed : Environment.TickCount,
            };
            bool deferBox = false;
            if (script != null && script.steps.Count > 0)
            {
                _runner = new ChestScriptRunner();
                _runner.Prepare(script, _ctx, _stepOverrides);
                deferBox = _runner.WillOpenBox;   // kịch bản có bước OpenBox: nó quyết lúc mở (ví dụ chờ người chơi thả chìa)
            }
            _ctx.Raise(ChestEvents.Press);
            if (_runner != null) _runner.Begin(this);
            if (!deferBox) BeginBox();
        }

        /// <summary>
        /// Bắt đầu mở rương thật sự (BoxOpenSequence + quay màu + độ căng tăng dần). Play tự gọi, trừ khi kịch bản có bước OpenBox
        /// (khi đó bước đó gọi). Gọi nhiều lần cũng chỉ chạy một lần.
        /// </summary>
        public void BeginBox()
        {
            if (!_active || _boxBegun) return;
            _boxBegun = true;
            float rampSeconds;
            if (box != null)
            {
                StretchHolds();
                rampSeconds = box.IntroSeconds + PlannedSeconds(-1, lockStage);
                if (rouletteStartStage < 0) StartRoulette(PlannedSeconds(-1, lockStage));
                if (startBoxOnPlay && !box.IsPlaying) box.Play();
            }
            else
            {
                rampSeconds = standaloneRouletteSeconds;
                StartRoulette(standaloneRouletteSeconds);
            }
            if (_tension != null) _tension.StartRamp(rampSeconds);
        }

        private void StartTension()
        {
            ChestTensionSettings ts = Profile.tension;
            if (ts == null || !ts.enabled) return;
            _tension = new ChestTension(ts, transform, audioSource != null ? audioSource.spatialBlend : 0.5f,
                audioSource != null ? audioSource.outputAudioMixerGroup : null,
                level => { foreach (ChestFxHook h in _hooks) if (h != null) h.OnTensionPulse(this, level); });
        }

        /// <summary>Remove every effect and, if asked, close the box again.</summary>
        public void ResetReveal(bool resetBox = true)
        {
            EnsureReady();
            StopAllCoroutines();
            foreach (ChestFxHook h in _hooks) if (h != null) h.OnReset(this);
            _hooks.Clear();

            if (_tension != null) { _tension.Dispose(); _tension = null; }
            _runner = null;
            if (_ctx != null) { _ctx.ClearEvents(); _ctx = null; }
            if (_scope != null) { _scope.Clear(); _scope = null; }
            if (_motion != null) _motion.ResetAll();
            _boxBegun = false;

            if (_customFx != null) Destroy(_customFx);
            if (_fxRoot != null) Destroy(_fxRoot.gameObject);
            if (_silhouette != null) Destroy(_silhouette.gameObject);
            foreach (UnityEngine.Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
            _pips.Clear();
            _customFx = null;
            _fxRoot = null;
            _silhouette = null;
            _motes = null;
            _burst = null;
            if (_createdLight) revealLight = null;
            _createdLight = false;
            if (revealLight != null) revealLight.enabled = false;

            ClearGlowTint();
            RestoreHolds();

            _active = _rouletteStarted = _locked = _boxOpened = _silComplete = _revealed = false;
            _silPhase = SilPhase.None;
            _lightBase = _lightKick = 0f;

            if (resetBox && box != null) box.ResetToClosed();
        }

        // ------------------------------------------------------------------ Unity

        private void Awake()
        {
            EnsureReady();
        }

        private void EnsureReady()
        {
            if (_ready) return;
            _ready = true;
            if (box == null) box = GetComponent<BoxOpenSequence>();
            if (box != null)
            {
                box.onStageReached.AddListener(OnBoxStage);
                box.onOpened.AddListener(OnBoxOpened);
            }
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.5f;
            }
            _mpb = new MaterialPropertyBlock();
        }

        private void OnDestroy()
        {
            if (box != null)
            {
                box.onStageReached.RemoveListener(OnBoxStage);
                box.onOpened.RemoveListener(OnBoxOpened);
            }
            if (_active) ResetReveal(false);
        }

        private void LateUpdate()
        {
            if (!_active) return;
            float dt = Time.deltaTime;

            if (_fxRoot != null)
            {
                _fxRoot.SetPositionAndRotation(MouthPosition, transform.rotation);
            }

            if (_tension != null) _tension.Tick(dt);
            UpdateLight(dt);
            UpdatePips(dt);
            UpdateSilhouette(dt);
        }

        // ------------------------------------------------------------------ BoxOpenSequence hooks

        private void OnBoxStage(int stage)
        {
            if (!_active) return;
            if (!Profile.minimalFx) PlayStageClips(stage);
            _ctx?.Raise(ChestEvents.Stage(stage));
            if (stage == rouletteStartStage) StartRoulette(PlannedSeconds(rouletteStartStage, lockStage));
            if (stage == lockStage) Lock();
        }

        private void OnBoxOpened()
        {
            if (!_active) return;
            _boxOpened = true;
            if (!_locked) Lock();   // misconfigured stages: never end without a result
            TryRevealed();
        }

        /// <summary>Seconds between stage <paramref name="from"/> being reached (-1 = Play) and stage <paramref name="to"/> being reached.</summary>
        private float PlannedSeconds(int from, int to)
        {
            if (box == null) return standaloneRouletteSeconds;
            IList<BoxOpenSequence.Stage> stages = box.Stages;
            if (stages == null || stages.Count == 0) return standaloneRouletteSeconds;
            to = Mathf.Clamp(to, 0, stages.Count - 1);
            float sum = 0f;
            if (from >= 0 && from < stages.Count) sum += stages[from].hold;
            for (int k = Mathf.Max(0, from + 1); k < to; k++) sum += stages[k].travel + stages[k].hold;
            if (to > from) sum += stages[to].travel;
            return Mathf.Max(0.1f, sum);
        }

        /// <summary>Give the roulette and the growth enough time by stretching holds (restored on reset).</summary>
        private void StretchHolds()
        {
            IList<BoxOpenSequence.Stage> stages = box.Stages;
            if (stages == null) return;

            if (rouletteStartStage >= 0 && rouletteStartStage < stages.Count && lockStage > rouletteStartStage)
            {
                float extra = Profile.minRouletteSeconds - PlannedSeconds(rouletteStartStage, lockStage);
                if (extra > 0f) AddHold(rouletteStartStage, extra);
            }
            if (lockStage >= 0 && lockStage < stages.Count && silhouetteCompleteStage > lockStage)
            {
                float extra = minGrowSeconds - PlannedSeconds(lockStage, silhouetteCompleteStage);
                if (extra > 0f) AddHold(lockStage, extra);
            }
        }

        private void AddHold(int stage, float extra)
        {
            BoxOpenSequence.Stage s = box.Stages[stage];
            if (!_originalHolds.ContainsKey(stage)) _originalHolds[stage] = s.hold;
            s.hold = _originalHolds[stage] + extra;
        }

        private void RestoreHolds()
        {
            if (box == null) { _originalHolds.Clear(); return; }
            foreach (KeyValuePair<int, float> kv in _originalHolds)
            {
                if (kv.Key < box.Stages.Count) box.Stages[kv.Key].hold = kv.Value;
            }
            _originalHolds.Clear();
        }

        // ------------------------------------------------------------------ Measure and build

        private void MeasureChest()
        {
            Transform glow = transform.Find("Glow");
            bool has = false;
            Bounds b = new Bounds(transform.position, Vector3.zero);
            Transform key = transform.Find("Key");
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer) continue;
                if (glow != null && r.transform.IsChildOf(glow)) continue;
                if (key != null && r.transform.IsChildOf(key)) continue;
                if (!has) { b = r.bounds; has = true; }
                else b.Encapsulate(r.bounds);
            }
            if (!has) b = new Bounds(transform.position + Vector3.up * 0.2f, Vector3.one * 0.4f);

            _chestHeight = Mathf.Max(0.05f, b.size.y);
            _chestSize = Mathf.Max(0.05f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)));
            Vector3 mouth = glow != null ? glow.position : new Vector3(b.center.x, b.min.y + b.size.y * 0.7f, b.center.z);
            _mouthLocal = transform.InverseTransformPoint(mouth);
            _glowRenderers = glow != null ? glow.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
        }

        private void BuildFx()
        {
            ChestFxProfile p = Profile;
            GameObject root = new GameObject("ChestRevealFx_" + name);
            _fxRoot = root.transform;
            _fxRoot.SetPositionAndRotation(MouthPosition, transform.rotation);

            if (!p.minimalFx)
            {
                _motes = ChestFxFactory.CreateParticles("Motes", _fxRoot, p, _chestSize, false);
                _burst = ChestFxFactory.CreateParticles("Burst", _fxRoot, p, _chestSize, true);
                _owned.Add(_motes.GetComponent<ParticleSystemRenderer>().sharedMaterial);
                _owned.Add(_burst.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            }

            if (revealLight == null)
            {
                GameObject lightGo = new GameObject("RevealLight");
                lightGo.transform.SetParent(_fxRoot, false);
                lightGo.transform.localPosition = Vector3.up * (_chestHeight * 0.15f);
                revealLight = lightGo.AddComponent<Light>();
                revealLight.type = LightType.Point;
                revealLight.range = _chestSize * 4f;
                revealLight.shadows = LightShadows.None;
                _createdLight = true;
            }
            revealLight.intensity = 0f;
            revealLight.enabled = false;

            if (showRankPips && !p.minimalFx)
            {
                _pipMaterial = ChestFxFactory.CreateUnlit(Color.white, false, "RankPip");
                _owned.Add(_pipMaterial);
                for (int i = 0; i < ChestTierUtil.Count; i++)
                {
                    GameObject pip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    pip.name = "RankPip" + (i + 1);
                    Collider c = pip.GetComponent<Collider>();
                    if (c != null) Destroy(c);
                    pip.transform.SetParent(_fxRoot, false);
                    pip.transform.localScale = Vector3.one * (_chestSize * 0.05f);
                    Renderer r = pip.GetComponent<Renderer>();
                    r.sharedMaterial = _pipMaterial;
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    pip.SetActive(false);
                    _pips.Add(pip.transform);
                }
            }
        }

        private void BuildSilhouette(GameObject itemSource)
        {
            _silhouette = ChestSilhouette.Build(itemSource, targetSize, silhouetteStyle, rimWidth);
            _silhouette.transform.SetPositionAndRotation(SilStartPos(), transform.rotation);
            _silhouette.transform.localScale = Vector3.one * startScale;
            _silhouette.gameObject.SetActive(false);
            _silPhase = SilPhase.None;
            _silSpin = 0f;
            Color c = ColorOf(_shown);
            SetSilhouetteColors(c, 0.5f);
        }

        private void SpawnCustomFx()
        {
            _hooks.Clear();
            _hooks.AddRange(GetComponentsInChildren<ChestFxHook>());
            if (Profile.customFxPrefab != null)
            {
                _customFx = Instantiate(Profile.customFxPrefab, transform);
                _customFx.transform.position = MouthPosition;
                foreach (ChestFxHook h in _customFx.GetComponentsInChildren<ChestFxHook>())
                {
                    if (!_hooks.Contains(h)) _hooks.Add(h);
                }
            }
        }

        // ------------------------------------------------------------------ Roulette

        private void StartRoulette(float plannedSeconds)
        {
            if (!_active || _rouletteStarted || _locked) return;
            _rouletteStarted = true;
            StartCoroutine(RouletteRoutine(Mathf.Max(0.3f, plannedSeconds)));
        }

        private IEnumerator RouletteRoutine(float duration)
        {
            ChestFxProfile p = Profile;
            List<Step> steps = BuildSchedule(duration);

            onRoulette?.Invoke();
            _ctx?.Raise(ChestEvents.Roulette);
            foreach (ChestFxHook h in _hooks) if (h != null) h.OnRouletteStart(this, MouthPosition);
            if (!p.minimalFx) PlayOneShot(p.openAmbientClip, p.ambientVolume);

            if (_motes != null)
            {
                ParticleSystem.EmissionModule em = _motes.emission;
                em.rateOverTime = p.ambientRate;
                _motes.Play();
            }
            if (revealLight != null) revealLight.enabled = true;
            _lightBase = rouletteLightIntensity;

            // The silhouette starts rising (tiny) at the same time.
            _silhouette.gameObject.SetActive(true);
            StartSilPhase(SilPhase.Rising, duration, MouthPosition, preLockScale);

            float t = 0f;
            int k = 0;
            float scheduleEnd = steps.Count > 0 ? steps[steps.Count - 1].time : 0f;
            while (!_locked)
            {
                while (k < steps.Count && steps[k].time <= t)
                {
                    ShowTier(steps[k].tier, steps.Count > 1 ? k / (float)(steps.Count - 1) : 1f, true);
                    k++;
                }

                if (k >= steps.Count)
                {
                    // Schedule finished on the rolled tier. Lock now if nothing else will, or after a safety delay
                    // (e.g. the lock stage waits for the player).
                    if (box == null || lockStage <= rouletteStartStage || t > scheduleEnd + 6f) { Lock(); break; }
                }
                t += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Times and tiers of every colour change. Built backwards from the end so the last step is ALWAYS the rolled tier.
        /// </summary>
        private List<Step> BuildSchedule(float duration)
        {
            ChestFxProfile p = Profile;
            List<ChestTier> tiers = possibleTiers;
            int n = tiers.Count;
            int rolledIndex = Mathf.Max(0, tiers.IndexOf(_rolled));

            // 1) Gap lengths: short at the start, long at the end (slot machine slow-down).
            List<float> times = new List<float>();
            float minI = Mathf.Max(0.02f, p.minInterval);
            float maxI = Mathf.Max(minI, p.maxInterval);
            float time = 0f;
            int guard = 0;
            while (time < duration && guard++ < 400)
            {
                times.Add(time);
                float curve = p.slowdownCurve != null && p.slowdownCurve.length > 0 ? Mathf.Clamp01(p.slowdownCurve.Evaluate(time / duration)) : time / duration;
                time += Mathf.Lerp(minI, maxI, curve);
            }
            if (times.Count < 2) times.Add(duration);
            float scale = duration / Mathf.Max(0.001f, times[times.Count - 1]);
            for (int i = 0; i < times.Count; i++) times[i] *= scale;   // the last change lands exactly at the end

            int count = times.Count;
            ChestTier[] order = new ChestTier[count];

            // 2) Near miss: ..., rolled, next tier (slips), rolled. Decoration only.
            bool nearMiss = n > 1 && count >= 4 && _rng.NextDouble() < p.nearMissChance;
            int tail = count - 1;
            order[tail] = _rolled;
            if (nearMiss)
            {
                int slip = rolledIndex + 1 < n ? rolledIndex + 1 : rolledIndex - 1;
                order[tail - 1] = tiers[slip];
                order[tail - 2] = _rolled;
                tail -= 2;
                // Hang a little longer on the slip so it hurts.
                times[count - 2] = Mathf.Lerp(times[count - 3], times[count - 1], 0.45f);
            }

            // 3) Fill the rest backwards so walking forward reaches order[tail].
            int idx = tiers.IndexOf(order[tail]);
            int dir = -1;   // ping-pong direction when walking backwards
            for (int i = tail - 1; i >= 0; i--)
            {
                if (n == 1) { order[i] = tiers[0]; continue; }
                switch (p.rouletteStyle)
                {
                    case RouletteStyle.PingPong:
                        if (idx + dir < 0 || idx + dir >= n) dir = -dir;
                        idx += dir;
                        break;
                    case RouletteStyle.Shuffle:
                        int next = _rng.Next(n - 1);
                        idx = next >= idx ? next + 1 : next;   // never the same twice in a row
                        break;
                    default:
                        idx = (idx - 1 + n) % n;
                        break;
                }
                order[i] = tiers[idx];
            }

            List<Step> steps = new List<Step>(count);
            for (int i = 0; i < count; i++) steps.Add(new Step { time = times[i], tier = order[i] });
            return steps;
        }

        private void ShowTier(ChestTier tier, float progress, bool tick)
        {
            _shown = tier;
            TierPalette.TierStyle style = Palette.Get(tier);
            Color c = style.color;

            SetGlowTint(c * Mathf.Lerp(0.8f, 1.2f, style.intensity / 1.8f));
            if (revealLight != null) revealLight.color = c;
            ChestFxFactory.SetParticleColor(_motes, c, Profile, 1f);
            if (!_locked) SetSilhouetteColors(c, 0.6f);
            ShowPips(tier, c);

            if (tick)
            {
                _lightKick = Mathf.Max(_lightKick, 0.9f);
                if (!Profile.minimalFx) PlayTick(progress);
                onRouletteStep?.Invoke(tier);
                foreach (ChestFxHook h in _hooks) if (h != null) h.OnRouletteStep(this, tier, c);
            }
        }

        // ------------------------------------------------------------------ Lock

        private void Lock()
        {
            if (!_active || _locked) return;
            _locked = true;
            ChestFxProfile p = Profile;
            TierPalette.TierStyle style = Palette.Get(_rolled);

            // Khoảnh khắc chốt: BÂY GIỜ âm căng thẳng mới rẽ theo bậc (xem ChestTension: trước đó nó không biết bậc).
            if (_tension != null)
            {
                _resolution = _tension.Resolve(_rolled);
                if (_resolution == TensionResolution.Deflate && audioSource != null) audioSource.Stop();   // cắt cả tiếng nền đang vang: im thật sự
                foreach (ChestFxHook h in _hooks) if (h != null) h.OnResolve(this, _rolled, _resolution);
            }

            ShowTier(_rolled, 1f, false);
            if (_motes != null)
            {
                if (!_motes.isPlaying) _motes.Play();
                ParticleSystem.EmissionModule em = _motes.emission;
                em.rateOverTime = p.ambientRate * 0.35f * style.intensity;
            }
            if (revealLight != null) revealLight.enabled = true;

            _lightKick = p.lightFlashIntensity * style.intensity;
            _lightBase = rouletteLightIntensity * style.intensity;

            if (_burst != null)
            {
                ChestFxFactory.SetParticleColor(_burst, style.color, p, 1f);
                _burst.Play();   // emission rate is 0: playing only makes the emitted particles simulate
                _burst.Emit(Mathf.RoundToInt(p.burstCount * (1f + 2f * _rolled.Rank01())));
            }

            if (!p.minimalFx)
            {
                SpawnFlash(style);
                SpawnRays(style);
                for (int i = 0; i < style.ringCount; i++) StartCoroutine(RingRoutine(style, i * 0.12f, 1.1f + 0.45f * i));

                PlayOneShot(p.LockClipFor(_rolled), p.lockVolume);
                PlayOneShot(p.whooshClip, p.whooshVolume);
                StartCoroutine(AccentRoutine());
            }

            // Pop the silhouette out of the chest.
            float grow = box != null ? Mathf.Max(minGrowSeconds, PlannedSeconds(lockStage, silhouetteCompleteStage)) : standaloneGrowSeconds;
            if (!_silhouette.gameObject.activeSelf) _silhouette.gameObject.SetActive(true);
            StartSilPhase(SilPhase.Popping, grow, SilHoverPos(), 1f);
            SetSilhouetteColors(style.color, 1f);

            onTierLocked?.Invoke(_rolled);
            _ctx?.Raise(ChestEvents.Lock);
            foreach (ChestFxHook h in _hooks) if (h != null) h.OnTierLocked(this, _rolled, style.color);
        }

        private void SpawnFlash(TierPalette.TierStyle style)
        {
            Material m = ChestFxFactory.CreateAdditive(ChestFxFactory.GetTexture(ParticleLook.SoftDot), "Flash");
            _owned.Add(m);
            Mesh quad = ChestFxFactory.CreateQuadMesh();
            _owned.Add(quad);
            Transform t = CreateMeshObject("Flash", quad, m);
            StartCoroutine(FadeRoutine(t, m, style.color, 0.45f, u => _chestSize * (0.4f + 1.4f * EaseOutCubic(u)) * Mathf.Lerp(0.7f, 1.3f, _rolled.Rank01()), true, 0f));
        }

        private void SpawnRays(TierPalette.TierStyle style)
        {
            if (style.rayCount <= 0) return;
            ChestFxProfile p = Profile;
            Material m = ChestFxFactory.CreateAdditive(Texture2D.whiteTexture, "Rays");
            _owned.Add(m);
            Mesh mesh = ChestFxFactory.CreateRayMesh(style.rayCount, style.rayShape);
            _owned.Add(mesh);
            Transform t = CreateMeshObject("Rays", mesh, m);
            float length = _chestSize * 1.3f * style.rayLength * p.rayLengthMultiplier;
            StartCoroutine(FadeRoutine(t, m, style.color, Mathf.Max(0.2f, p.rayDuration), u => length * EaseOutCubic(Mathf.Clamp01(u * 4f)), true, p.raySpin));
        }

        private IEnumerator RingRoutine(TierPalette.TierStyle style, float delay, float maxRadius)
        {
            yield return new WaitForSeconds(delay);
            Material m = ChestFxFactory.CreateAdditive(Texture2D.whiteTexture, "Ring");
            _owned.Add(m);
            Mesh mesh = ChestFxFactory.CreateRingMesh(0.72f);
            _owned.Add(mesh);
            Transform t = CreateMeshObject("Ring", mesh, m);
            yield return FadeRoutine(t, m, style.color, 0.7f, u => _chestSize * maxRadius * EaseOutCubic(u), false, 0f);
        }

        private Transform CreateMeshObject(string objName, Mesh mesh, Material material)
        {
            GameObject go = new GameObject(objName);
            go.transform.SetParent(_fxRoot, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            return go.transform;
        }

        /// <summary>Scales an fx object with <paramref name="sizeAt"/> and fades it out. billboard = faces the camera.</summary>
        private IEnumerator FadeRoutine(Transform t, Material m, Color color, float duration, Func<float, float> sizeAt, bool billboard, float spin)
        {
            float time = 0f;
            float roll = (float)_rng.NextDouble() * 360f;
            while (time < duration && t != null)
            {
                time += Time.deltaTime;
                float u = Mathf.Clamp01(time / duration);
                float size = Mathf.Max(0.0001f, sizeAt(u));
                t.localScale = Vector3.one * size;
                roll += spin * Time.deltaTime;
                if (billboard && _camera != null)
                {
                    Vector3 toCam = _camera.transform.position - t.position;
                    t.rotation = Quaternion.LookRotation(-toCam, _camera.transform.up) * Quaternion.Euler(0f, 0f, roll);
                }
                Color c = color;
                c.a = 1f - u * u;
                ChestFxFactory.SetColor(m, c);
                yield return null;
            }
            if (t != null) Destroy(t.gameObject);
        }

        private IEnumerator AccentRoutine()
        {
            ChestFxProfile p = Profile;
            if (p.accentClips == null || p.accentClips.Length == 0) yield break;
            int count = Mathf.Max(0, p.accentsBase + p.accentsPerTier * _rolled.Rank());
            float[] delays = new float[count];
            for (int i = 0; i < count; i++) delays[i] = 0.08f + (float)_rng.NextDouble() * (0.5f + 0.15f * count);
            Array.Sort(delays);
            float time = 0f;
            for (int i = 0; i < count; i++)
            {
                while (time < delays[i]) { time += Time.deltaTime; yield return null; }
                AudioClip clip = p.accentClips[_rng.Next(p.accentClips.Length)];
                PlayOneShot(clip, p.accentVolume * Mathf.Lerp(1f, 0.6f, i / (float)Mathf.Max(1, count)), 0.9f + (float)_rng.NextDouble() * 0.25f);
            }
        }

        // ------------------------------------------------------------------ Silhouette motion

        private Vector3 SilStartPos() => MouthPosition - transform.up * (_chestHeight * 0.25f);

        private Vector3 SilHoverPos() => MouthPosition + transform.up * (_chestHeight * hoverAboveMouth + targetSize * 0.5f);

        private void StartSilPhase(SilPhase phase, float duration, Vector3 to, float toScale)
        {
            if (_silhouette == null) return;
            if (_silPhase == SilPhase.None)
            {
                _silFromPos = SilStartPos();
                _silFromScale = startScale;
            }
            else
            {
                _silFromPos = _silhouette.transform.position;
                _silFromScale = _silhouette.transform.localScale.x;
            }
            _silPhase = phase;
            _silPhaseStart = Time.time;
            _silPhaseDuration = Mathf.Max(0.05f, duration);
            _silToPos = to;
            _silToScale = toScale;
        }

        private void UpdateSilhouette(float dt)
        {
            if (_silhouette == null || _silPhase == SilPhase.None) return;
            Transform t = _silhouette.transform;
            float u = Mathf.Clamp01((Time.time - _silPhaseStart) / _silPhaseDuration);
            _silSpin += spinSpeed * dt;

            Vector3 pos;
            float scale;
            switch (_silPhase)
            {
                case SilPhase.Rising:
                    // Grows slowly at the mouth with a nervous wobble while the colours spin.
                    float e = EaseInOutSine(u);
                    pos = Vector3.LerpUnclamped(_silFromPos, _silToPos, e);
                    scale = Mathf.Lerp(_silFromScale, _silToScale, e) * (1f + 0.06f * Mathf.Sin(Time.time * 17f) * (1f - u * 0.5f));
                    break;
                case SilPhase.Popping:
                    pos = Vector3.LerpUnclamped(_silFromPos, _silToPos, EaseOutCubic(u));
                    scale = Mathf.LerpUnclamped(_silFromScale, _silToScale, EaseOutBack(u, 1.4f));
                    if (u >= 1f)
                    {
                        _silPhase = SilPhase.Settling;
                        _silCompleteTime = Time.time + settleSeconds;
                    }
                    break;
                default: // Settling, Done: full size, floating
                    float bob = Mathf.Sin((Time.time - _silPhaseStart) * 2.2f) * bobAmplitude;
                    pos = _silToPos + transform.up * bob;
                    scale = 1f;
                    if (_silPhase == SilPhase.Settling && Time.time >= _silCompleteTime)
                    {
                        _silPhase = SilPhase.Done;
                        CompleteSilhouette();
                    }
                    break;
            }

            t.position = pos;
            t.rotation = transform.rotation * Quaternion.AngleAxis(_silSpin, Vector3.up);
            t.localScale = Vector3.one * Mathf.Max(0.0001f, scale);

            if (flicker > 0f)
            {
                float n = Mathf.PerlinNoise(Time.time * 9f, 0.37f);
                float glowFactor = _locked ? 1f : 0.6f;
                SetSilhouetteColors(ColorOf(_shown), glowFactor * (1f - flicker + flicker * 2f * n));
            }
        }

        private void CompleteSilhouette()
        {
            if (_silComplete) return;
            _silComplete = true;
            onSilhouetteComplete?.Invoke();
            _ctx?.Raise(ChestEvents.SilhouetteComplete);
            foreach (ChestFxHook h in _hooks) if (h != null) h.OnSilhouetteComplete(this, _silhouette.transform);
            if (box == null) _boxOpened = true;
            TryRevealed();
        }

        private void TryRevealed()
        {
            if (_revealed || !_silComplete || !_boxOpened) return;
            _revealed = true;
            onRevealed?.Invoke();
            _ctx?.Raise(ChestEvents.Revealed);
            foreach (ChestFxHook h in _hooks) if (h != null) h.OnRevealed(this);
        }

        private void SetSilhouetteColors(Color tierColor, float rimStrength)
        {
            if (_silhouette == null) return;
            Color fill = silhouetteStyle == SilhouetteStyle.BlackWithRim ? Color.black : tierColor * fillBrightness;
            fill.a = 1f;
            Color rim = Color.Lerp(Color.black, tierColor, Mathf.Clamp01(rimStrength)) * Mathf.Max(1f, rimStrength);
            rim.a = 1f;
            _silhouette.SetColors(fill, rim);
        }

        // ------------------------------------------------------------------ Light, glow, pips

        private void UpdateLight(float dt)
        {
            if (revealLight == null || !revealLight.enabled) return;
            _lightKick *= Mathf.Exp(-dt * 5f);
            revealLight.intensity = _lightBase + _lightKick;
        }

        private void ShowPips(ChestTier tier, Color c)
        {
            if (_pips.Count == 0) return;
            int count = tier.Rank() + 1;
            for (int i = 0; i < _pips.Count; i++) _pips[i].gameObject.SetActive(i < count);
            ChestFxFactory.SetColor(_pipMaterial, c);
        }

        private void UpdatePips(float dt)
        {
            if (_pips.Count == 0) return;
            _pipAngle += dt * (_locked ? 40f : 160f);
            int active = 0;
            for (int i = 0; i < _pips.Count; i++) if (_pips[i].gameObject.activeSelf) active++;
            int j = 0;
            float radius = _chestSize * 0.55f;
            for (int i = 0; i < _pips.Count; i++)
            {
                if (!_pips[i].gameObject.activeSelf) continue;
                float a = (_pipAngle + j * 360f / Mathf.Max(1, active)) * Mathf.Deg2Rad;
                _pips[i].localPosition = new Vector3(Mathf.Cos(a) * radius, _chestHeight * 0.1f, Mathf.Sin(a) * radius);
                j++;
            }
        }

        private static readonly string[] GlowColorProps = { "_BaseColor", "_Color", "baseColorFactor" };
        private static readonly string[] GlowEmissionProps = { "_EmissionColor", "emissiveFactor" };

        private void SetGlowTint(Color c)
        {
            if (!tintGlow || _glowRenderers == null) return;
            foreach (Renderer r in _glowRenderers)
            {
                if (r == null || r.sharedMaterial == null) continue;
                Material sm = r.sharedMaterial;
                r.GetPropertyBlock(_mpb);
                foreach (string prop in GlowColorProps)
                {
                    if (!sm.HasProperty(prop)) continue;
                    Color k = c;
                    k.a = sm.GetColor(prop).a;
                    _mpb.SetColor(prop, k);
                }
                foreach (string prop in GlowEmissionProps)
                {
                    if (sm.HasProperty(prop)) _mpb.SetColor(prop, c * glowEmissionBoost);
                }
                r.SetPropertyBlock(_mpb);
            }
        }

        private void ClearGlowTint()
        {
            if (_glowRenderers == null) return;
            foreach (Renderer r in _glowRenderers) if (r != null) r.SetPropertyBlock(null);
        }

        // ------------------------------------------------------------------ Audio

        private void PlayTick(float progress)
        {
            ChestFxProfile p = Profile;
            if (p.tickClips == null || p.tickClips.Length == 0) return;
            AudioClip clip = p.tickClips[_rng.Next(p.tickClips.Length)];
            if (clip == null) return;
            if (_tickSource == null)
            {
                _tickSource = new GameObject("TickAudio").AddComponent<AudioSource>();
                _tickSource.transform.SetParent(transform, false);
                _tickSource.playOnAwake = false;
                _tickSource.spatialBlend = audioSource != null ? audioSource.spatialBlend : 0.5f;
                _tickSource.outputAudioMixerGroup = audioSource != null ? audioSource.outputAudioMixerGroup : null;
            }
            _tickSource.pitch = Mathf.Lerp(p.tickPitchStart, p.tickPitchEnd, progress);
            _tickSource.PlayOneShot(clip, p.tickVolume);
        }

        private void PlayStageClips(int stage)
        {
            ChestFxProfile p = Profile;
            if (p.stageClips == null) return;
            foreach (ChestFxProfile.StageClip sc in p.stageClips)
            {
                if (sc != null && sc.stage == stage) PlayOneShot(sc.clip, sc.volume);
            }
        }

        /// <summary>Plays a clip on the chest's AudioSource. Null clips are skipped silently.</summary>
        public void PlayOneShot(AudioClip clip, float volume, float pitch = 1f)
        {
            if (clip == null || audioSource == null) return;
            if (Mathf.Approximately(pitch, 1f))
            {
                audioSource.PlayOneShot(clip, volume);
                return;
            }
            // A pitched one-shot needs its own source (pitch is per source); it cleans itself up.
            GameObject go = new GameObject("OneShot_" + clip.name);
            go.transform.SetParent(transform, false);
            AudioSource s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = audioSource.spatialBlend;
            s.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            s.pitch = pitch;
            s.PlayOneShot(clip, volume);
            Destroy(go, clip.length / Mathf.Max(0.1f, pitch) + 0.1f);
        }

        // ------------------------------------------------------------------ Helpers

        public Color ColorOf(ChestTier tier) => Palette.ColorOf(tier);

        private static float EaseOutCubic(float x)
        {
            float a = 1f - x;
            return 1f - a * a * a;
        }

        private static float EaseInOutSine(float x) => -(Mathf.Cos(Mathf.PI * x) - 1f) * 0.5f;

        private static float EaseOutBack(float x, float c1)
        {
            float c3 = c1 + 1f;
            float a = x - 1f;
            return 1f + c3 * a * a * a + c1 * a * a;
        }
    }
}

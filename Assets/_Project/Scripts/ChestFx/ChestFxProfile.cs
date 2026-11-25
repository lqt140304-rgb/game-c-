using System;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>How the roulette walks through the possible tier colours.</summary>
    public enum RouletteStyle
    {
        Cycle,     // Rác → Thường → Hiếm → ... → back to the start (slot machine)
        PingPong,  // climbs up then down like a power meter ("xếp hàng")
        Shuffle,   // jumps around at random (decoration only, the final tier is still the rolled one)
    }

    /// <summary>Look of a single particle.</summary>
    public enum ParticleLook
    {
        SoftDot,   // round glow
        Sparkle,   // four-point star
        Coin,      // flat disc with a bright rim (falling coins)
        Streak,    // stretched along its velocity
    }

    /// <summary>Main direction the particles travel.</summary>
    public enum ParticleDirection
    {
        Up,        // cone rising out of the chest mouth
        Radial,    // sphere bursting out in all directions
        Swirl,     // rising with an orbit round the chest
        Fountain,  // shoots up then falls back (pairs well with gravity)
    }

    /// <summary>
    /// Everything that makes one chest's opening look and sound different. The shared core
    /// (<see cref="ChestRevealSequence"/>) reads this; a new chest only needs a new profile (and optionally a
    /// custom prefab with <see cref="ChestFxHook"/> components). Every clip may be left empty: it is skipped.
    /// Create with Assets > Create > Cho Do La > Chest FX Profile.
    /// </summary>
    [CreateAssetMenu(menuName = "Cho Do La/Chest FX Profile", fileName = "ChestFx_New")]
    public class ChestFxProfile : ScriptableObject
    {
        [Serializable]
        public class StageClip
        {
            [Tooltip("BoxOpenSequence stage index: 0 Rung, 1 Go niem, 2 He nap, 3 Bat nap, 4 Mo toang.")] public int stage = 2;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 0.8f;
        }

        [Tooltip("Name of the chest model this profile belongs to, e.g. Chest_Golden (used by the test scene to pick a profile).")]
        public string chestId = "Chest_New";

        [Header("Audio (placeholder sounds are synthesised by Tools/Audio/make_chest_sfx.py)")]
        [Tooltip("One is picked per colour change. Empty = silent ticks.")] public AudioClip[] tickClips = new AudioClip[0];
        [Range(0f, 1f)] public float tickVolume = 0.55f;
        [Tooltip("Tick pitch at the start of the roulette; it rises to tickPitchEnd as it slows down.")] public float tickPitchStart = 0.9f;
        public float tickPitchEnd = 1.45f;
        [Tooltip("Lock sound per tier, indexed by rank: 0 Rác, 1 Thường, 2 Hiếm, 3 Rất hiếm, 4 Huyền thoại.")]
        public AudioClip[] lockClipsByTier = new AudioClip[ChestTierUtil.Count];
        [Range(0f, 1f)] public float lockVolume = 0.9f;
        [Tooltip("Plays when the silhouette pops out of the chest and grows.")] public AudioClip whooshClip;
        [Range(0f, 1f)] public float whooshVolume = 0.6f;
        [Tooltip("Bed under the roulette (one-shot, starts with the roulette).")] public AudioClip openAmbientClip;
        [Range(0f, 1f)] public float ambientVolume = 0.4f;
        [Tooltip("Extra sounds scattered after the lock (coins, bells...). Their number grows with the tier.")]
        public AudioClip[] accentClips = new AudioClip[0];
        [Range(0f, 1f)] public float accentVolume = 0.5f;
        [Tooltip("How many accents for Rác; each tier above adds accentsPerTier.")] public int accentsBase = 1;
        public int accentsPerTier = 2;
        [Tooltip("Sounds tied to BoxOpenSequence stages (creak at He nap, thud at Mo toang...).")]
        public StageClip[] stageClips = new StageClip[0];

        [Header("Particles (built in code at runtime)")]
        [Tooltip("Second colour mixed into the tier colour.")] public Color secondaryColor = Color.white;
        [Range(0f, 1f), Tooltip("0 = only the tier colour, 1 = half of the particles are the secondary colour.")] public float secondaryMix = 0.3f;
        public ParticleLook particleLook = ParticleLook.SoftDot;
        public ParticleDirection particleDirection = ParticleDirection.Up;
        [Tooltip("Particles per second while the roulette runs.")] public float ambientRate = 30f;
        [Tooltip("Particles fired when the tier locks (Rác). Multiplied up to x3 for Huyền thoại.")] public int burstCount = 40;
        public float particleSpeed = 0.6f;
        public float particleSize = 0.025f;
        public float particleLifetime = 1.2f;
        [Tooltip("Gravity on the lock burst (0 = floats, 0.5+ = falls like coins).")] public float burstGravity = 0f;

        [Header("Roulette")]
        public RouletteStyle rouletteStyle = RouletteStyle.Cycle;
        [Tooltip("Shortest gap between colour changes (start of the roulette), seconds.")] public float minInterval = 0.055f;
        [Tooltip("Longest gap (end of the roulette), seconds.")] public float maxInterval = 0.42f;
        [Tooltip("0..1 progress → 0..1 between minInterval and maxInterval. Ease-in = slows down late.")]
        public AnimationCurve slowdownCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.55f, 0.18f, 0.7f, 0.7f), new Keyframe(1f, 1f, 2.6f, 0f));
        [Range(0f, 1f), Tooltip("Chance of the near miss: slips one tier past, then comes back before stopping.")] public float nearMissChance = 0.5f;
        [Tooltip("The roulette lasts at least this long; the BoxOpenSequence hold before the lock is stretched if needed.")]
        public float minRouletteSeconds = 2.8f;

        [Header("Rays and flash")]
        public float rayLengthMultiplier = 1f;
        public float rayDuration = 1.1f;
        [Tooltip("Degrees per second the ray star spins.")] public float raySpin = 25f;
        [Tooltip("Point light peak intensity at the lock (scaled by tier).")] public float lightFlashIntensity = 6f;

        [Header("Căng thẳng: âm bắt đầu NGAY khi bấm mở rương")]
        public ChestTensionSettings tension = new ChestTensionSettings();

        [Header("Kịch bản mở rương (các bước ghép: vật lý, chuyển động cả rương, vật ngoài mở khoá, cue code)")]
        [Tooltip("Để trống = không có kịch bản, rương chạy như cũ.")]
        public ChestScript script;

        [Header("Tối giản")]
        [Tooltip("Bật = lõi bỏ hạt, chớp sáng, tia, vòng, tick, âm khoá/whoosh/ambient/stage/accent và chấm bậc. Vẫn giữ vòng đổi màu, ánh sáng glow, bóng vật phẩm và mọi callback của hook.")]
        public bool minimalFx = false;

        [Header("Custom effect")]
        [Tooltip("Spawned under the chest at Play. Any ChestFxHook on it receives the reveal callbacks.")]
        public GameObject customFxPrefab;

        public AudioClip LockClipFor(ChestTier tier)
        {
            int i = (int)tier;
            return lockClipsByTier != null && i >= 0 && i < lockClipsByTier.Length ? lockClipsByTier[i] : null;
        }

        private static ChestFxProfile _runtimeDefault;

        /// <summary>Silent profile with neutral particles, used when a chest has no profile.</summary>
        public static ChestFxProfile Default
        {
            get
            {
                if (_runtimeDefault == null)
                {
                    _runtimeDefault = CreateInstance<ChestFxProfile>();
                    _runtimeDefault.name = "ChestFxProfile (default)";
                    _runtimeDefault.hideFlags = HideFlags.DontSave;
                }
                return _runtimeDefault;
            }
        }
    }
}

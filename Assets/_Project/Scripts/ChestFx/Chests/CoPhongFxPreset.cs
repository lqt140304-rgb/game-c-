using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_CoPhong (hũ gốm men ngọc, nhãn thư pháp, phong giấy đỏ): quiet and hand-made, not festive.
    /// Ink-brush streaks swirl up slowly like incense, the colours "breathe" back and forth (PingPong) at a calm
    /// pace, few slow rays, a soft light. Sounds: bamboo-chopstick tick, ancient zither plucks per tier, a temple
    /// bell for Huyền thoại, a comic ink spill for Rác (Audio/Chests/co_phong). Extra: CoPhongHook.
    /// </summary>
    public static class CoPhongFxPreset
    {
        public const string ChestId = "Chest_CoPhong";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/co_phong/";

        public static readonly string[] TickFiles = { "sfx_cophong_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_cophong_lock_rac.mp3", "sfx_cophong_lock_thuong.mp3", "sfx_cophong_lock_hiem.mp3",
            "sfx_cophong_lock_ratHiem.mp3", "sfx_cophong_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_cophong_whoosh.mp3";
        public const string AmbientFile = "sfx_cophong_ambient.mp3";
        public static readonly string[] AccentFiles = { "sfx_cophong_brush_stroke.mp3", "sfx_cophong_water_drop.mp3", "sfx_cophong_bamboo_knock.mp3" };
        public const string SealTearFile = "sfx_cophong_seal_tear.mp3";   // stage 1 Gỡ niêm: the red paper seal
        public const string LidLiftFile = "sfx_cophong_lid_lift.mp3";     // stage 2 Hé nắp: ceramic lid scrape
        public const string LidSettleFile = "sfx_cophong_lid_settle.mp3"; // stage 4 Mở toang: lid set down

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.45f;
            p.tickPitchStart = 1.0f;       // a wooden tap should not squeak: small pitch rise only
            p.tickPitchEnd = 1.2f;
            p.lockVolume = 0.85f;
            p.whooshVolume = 0.5f;
            p.ambientVolume = 0.35f;
            p.accentVolume = 0.35f;
            p.accentsBase = 1;
            p.accentsPerTier = 1;

            p.secondaryColor = new Color(0.78f, 0.16f, 0.12f);   // son (cinnabar seal red)
            p.secondaryMix = 0.3f;
            p.particleLook = ParticleLook.Streak;                // brush strokes
            p.particleDirection = ParticleDirection.Swirl;       // drifting up round the jar like incense
            p.ambientRate = 14f;                                 // sparse: calm, not a party
            p.burstCount = 28;
            p.particleSpeed = 0.32f;
            p.particleSize = 0.022f;
            p.particleLifetime = 2.2f;
            p.burstGravity = 0f;

            p.rouletteStyle = RouletteStyle.PingPong;            // colours "breathe" up and down like a brush pressing and lifting
            p.minInterval = 0.075f;
            p.maxInterval = 0.5f;
            p.slowdownCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0.6f), new Keyframe(0.6f, 0.3f, 0.9f, 0.9f), new Keyframe(1f, 1f, 2.2f, 0f));
            p.nearMissChance = 0.25f;                            // one hesitation, rarely
            p.minRouletteSeconds = 3.0f;

            p.rayLengthMultiplier = 0.85f;
            p.rayDuration = 1.6f;
            p.raySpin = 6f;                                      // almost still
            p.lightFlashIntensity = 4f;
        }
    }
}

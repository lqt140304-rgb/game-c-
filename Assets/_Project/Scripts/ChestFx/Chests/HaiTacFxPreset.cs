using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_HaiTac (pirate captain skull on a pile of coins, the skull cap + tricorn flip back to open):
    /// "storm at sea". The roulette rocks up and down like a ship on swells (PingPong) with a wave-shaped
    /// slowdown, sea-foam particles swirl like a whirlpool, rays spin backwards, the burst falls back like spray.
    /// Clips are filled by name in Editor/HaiTacFxSetup.cs (files in Audio/Chests/hai_tac/).
    /// </summary>
    public static class HaiTacFxPreset
    {
        public const string ChestId = "Chest_HaiTac";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/hai_tac/";

        public static readonly string[] TickFiles = { "sfx_haitac_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_haitac_lock_rac.mp3", "sfx_haitac_lock_thuong.mp3", "sfx_haitac_lock_hiem.mp3",
            "sfx_haitac_lock_ratHiem.mp3", "sfx_haitac_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_haitac_whoosh_grow.mp3";
        public const string AmbientFile = "sfx_haitac_waves_bed.mp3";
        public static readonly string[] AccentFiles = { "sfx_haitac_coins.mp3", "sfx_haitac_seagull.mp3" };
        public const string LaughFile = "sfx_haitac_laugh.mp3";        // stage 0 Rung: the skull laughs while shaking
        public const string SealFile = "sfx_haitac_seal_crack.mp3";    // stage 1 Go niem: red wax seal snaps
        public const string CreakFile = "sfx_haitac_skull_creak.mp3";  // stage 2 He nap: skull cap creaks open
        public const string CannonFile = "sfx_haitac_cannon.mp3";      // HaiTacHook: Hiếm and up
        public const string CoinsFile = "sfx_haitac_coins.mp3";        // HaiTacHook: coin geyser
        public const string SeagullFile = "sfx_haitac_seagull.mp3";    // HaiTacHook: mocking gull on Rác

        public static readonly Color SeaFoam = new Color(0.35f, 0.92f, 0.85f);

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.5f;
            p.tickPitchStart = 1.1f;    // compass click starts bright...
            p.tickPitchEnd = 0.8f;      // ...and sinks as the ship slows (opposite of the other chests)
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.45f;
            p.accentVolume = 0.45f;
            p.accentsBase = 0;          // Rác: the hook's seagull mocks you instead
            p.accentsPerTier = 1;

            p.secondaryColor = SeaFoam;
            p.secondaryMix = 0.5f;
            p.particleLook = ParticleLook.SoftDot;          // foam / spray droplets
            p.particleDirection = ParticleDirection.Swirl;  // whirlpool round the skull
            p.ambientRate = 34f;
            p.burstCount = 55;
            p.particleSpeed = 0.55f;
            p.particleSize = 0.028f;
            p.particleLifetime = 1.5f;
            p.burstGravity = 0.4f;                           // spray falls back like a wave breaking

            p.rouletteStyle = RouletteStyle.PingPong;        // up and down like a ship on swells
            p.minInterval = 0.06f;
            p.maxInterval = 0.5f;
            // A swell: slows, surges forward again for a moment (the wave pushes), then slows to a stop.
            p.slowdownCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0.4f),
                new Keyframe(0.38f, 0.22f, 0.3f, 0f),
                new Keyframe(0.55f, 0.1f, 0f, 0f),
                new Keyframe(1f, 1f, 3f, 0f));
            p.nearMissChance = 0.7f;                         // pirates like to taunt
            p.minRouletteSeconds = 3.0f;

            p.rayLengthMultiplier = 1.3f;
            p.rayDuration = 1.3f;
            p.raySpin = -40f;                                // counter-clockwise, the way the whirlpool turns
            p.lightFlashIntensity = 6.5f;
        }
    }
}

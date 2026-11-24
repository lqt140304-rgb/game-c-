using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Ready-made profile values (no audio clips: the editor fills those by file name, see
    /// Editor/ChestRevealTestSetup.cs). One method per chest; the next sessions add theirs here.
    /// </summary>
    public static class ChestFxPresets
    {
        /// <summary>Placeholder sound files made by Tools/Audio/make_chest_sfx.py (in Assets/_Project/Audio/Chests/).</summary>
        public const string AudioFolder = "Assets/_Project/Audio/Chests/";

        public static readonly string[] TickFiles = { "sfx_chest_tick_a.wav", "sfx_chest_tick_b.wav" };
        public static readonly string[] LockFiles =
        {
            "sfx_chest_lock_0_rac.wav", "sfx_chest_lock_1_thuong.wav", "sfx_chest_lock_2_hiem.wav",
            "sfx_chest_lock_3_rathiem.wav", "sfx_chest_lock_4_huyenthoai.wav",
        };
        public const string WhooshFile = "sfx_chest_whoosh.wav";
        public const string AmbientFile = "sfx_chest_open_shimmer.wav";
        public const string CreakFile = "sfx_chest_lid_creak.wav";
        public const string ThudFile = "sfx_chest_lid_thud.wav";
        public static readonly string[] CoinFiles = { "sfx_coin_clink_1.wav", "sfx_coin_clink_2.wav", "sfx_coin_clink_3.wav" };

        /// <summary>Chest_Golden: gold rays, a fountain of coins that falls back, coins clinking after the lock.</summary>
        public static void ApplyGolden(ChestFxProfile p)
        {
            p.chestId = "Chest_Golden";

            p.tickVolume = 0.5f;
            p.tickPitchStart = 0.95f;
            p.tickPitchEnd = 1.55f;
            p.lockVolume = 0.9f;
            p.whooshVolume = 0.5f;
            p.ambientVolume = 0.35f;
            p.accentVolume = 0.45f;
            p.accentsBase = 1;
            p.accentsPerTier = 2;

            p.secondaryColor = new Color(1f, 0.86f, 0.35f);   // gold
            p.secondaryMix = 0.55f;
            p.particleLook = ParticleLook.Coin;
            p.particleDirection = ParticleDirection.Fountain;
            p.ambientRate = 22f;
            p.burstCount = 45;
            p.particleSpeed = 0.75f;
            p.particleSize = 0.03f;
            p.particleLifetime = 1.4f;
            p.burstGravity = 0.9f;

            p.rouletteStyle = RouletteStyle.Cycle;
            p.minInterval = 0.05f;
            p.maxInterval = 0.45f;
            p.nearMissChance = 0.55f;
            p.minRouletteSeconds = 2.8f;

            p.rayLengthMultiplier = 1.15f;
            p.rayDuration = 1.2f;
            p.raySpin = 18f;
            p.lightFlashIntensity = 7f;
        }
    }
}

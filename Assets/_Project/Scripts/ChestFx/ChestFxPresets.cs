using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Ready-made profile values (no audio clips: the editor fills those by file name, see
    /// Editor/ChestRevealTestSetup.cs). One method per chest; the next sessions add theirs here.
    /// </summary>
    public static class ChestFxPresets
    {
        /// <summary>
        /// Shared chest sounds: mp3 made with ElevenLabs (owner approved 2026-11-24, see Audio/Chests/README.md).
        /// Not listened to yet; check the ElevenLabs licence terms before release.
        /// </summary>
        public const string AudioFolder = "Assets/_Project/Audio/Chests/";

        /// <summary>Fallback: placeholder sounds synthesised by Tools/Audio/make_chest_sfx.py (used only when an mp3 is missing).</summary>
        public const string SynthFolder = "Assets/_Project/Audio/Chests/Synth/";

        public static readonly string[] TickFiles = { "sfx_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_lock_rac.mp3", "sfx_lock_thuong.mp3", "sfx_lock_hiem.mp3", "sfx_lock_ratHiem.mp3", "sfx_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_whoosh_grow.mp3";
        public const string CreakFile = "sfx_lid_creak.mp3";
        public static readonly string[] CoinFiles = { "sfx_coin_jingle.mp3" };
        /// <summary>No mp3 exists for these two yet: they come from the synth folder.</summary>
        public const string AmbientFile = "Synth/sfx_chest_open_shimmer.wav";
        public const string ThudFile = "Synth/sfx_chest_lid_thud.wav";

        /// <summary>Synth fallback for each mp3 above (same key = mp3 file name).</summary>
        public static string SynthFallback(string file)
        {
            switch (file)
            {
                case "sfx_tick.mp3": return "Synth/sfx_chest_tick_a.wav";
                case "sfx_lock_rac.mp3": return "Synth/sfx_chest_lock_0_rac.wav";
                case "sfx_lock_thuong.mp3": return "Synth/sfx_chest_lock_1_thuong.wav";
                case "sfx_lock_hiem.mp3": return "Synth/sfx_chest_lock_2_hiem.wav";
                case "sfx_lock_ratHiem.mp3": return "Synth/sfx_chest_lock_3_rathiem.wav";
                case "sfx_lock_huyenThoai.mp3": return "Synth/sfx_chest_lock_4_huyenthoai.wav";
                case "sfx_whoosh_grow.mp3": return "Synth/sfx_chest_whoosh.wav";
                case "sfx_lid_creak.mp3": return "Synth/sfx_chest_lid_creak.wav";
                case "sfx_coin_jingle.mp3": return "Synth/sfx_coin_clink_1.wav";
                default: return null;
            }
        }

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
            p.accentsPerTier = 1;   // sfx_coin_jingle is 1.2 s long, so fewer than with short clinks

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

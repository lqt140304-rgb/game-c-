using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_AmNhac (mini jukebox): neon pink/teal music notes swirling up, a "VU meter" roulette that climbs and
    /// falls (PingPong), tight hi-hat ticks, a chord or riff per tier on lock. Clips are filled by
    /// Editor/AmNhacFxSetup.cs from Audio/Chests/am_nhac/. Proposal, not yet seen or heard in Unity.
    /// </summary>
    public static class AmNhacFxPreset
    {
        public const string ChestId = "Chest_AmNhac";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/am_nhac/";

        public static readonly Color NeonPink = new Color(1f, 0.25f, 0.75f);
        public static readonly Color NeonTeal = new Color(0.15f, 1f, 0.85f);

        public static readonly string[] TickFiles = { "sfx_amnhac_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_amnhac_lock_rac.mp3", "sfx_amnhac_lock_thuong.mp3", "sfx_amnhac_lock_hiem.mp3",
            "sfx_amnhac_lock_ratHiem.mp3", "sfx_amnhac_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_amnhac_whoosh.mp3";
        public const string AmbientFile = "sfx_amnhac_drumroll.mp3";      // bed under the roulette
        public const string CoinSlotFile = "sfx_amnhac_coin_slot.mp3";    // stage 0: coin into the jukebox
        public const string NeedleDropFile = "sfx_amnhac_needle_drop.mp3"; // stage 2: needle drops as the lid cracks
        public static readonly string[] AccentFiles = { "sfx_amnhac_note_pluck.mp3" };
        public const string CrowdFile = "sfx_amnhac_crowd.mp3";           // played by AmNhacHook from Rất hiếm up

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.45f;
            p.tickPitchStart = 1.0f;     // hi-hat: keep it close to natural pitch
            p.tickPitchEnd = 1.25f;
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.45f;     // drum roll
            p.accentVolume = 0.35f;
            p.accentsBase = 0;           // Rác: no sparkle notes, just the broken record
            p.accentsPerTier = 2;

            p.secondaryColor = NeonTeal;
            p.secondaryMix = 0.6f;
            p.particleLook = ParticleLook.Sparkle;          // little twinkling "notes"
            p.particleDirection = ParticleDirection.Swirl;   // notes spiral up out of the jukebox
            p.ambientRate = 26f;
            p.burstCount = 50;
            p.particleSpeed = 0.5f;
            p.particleSize = 0.028f;
            p.particleLifetime = 1.6f;
            p.burstGravity = -0.08f;                         // notes keep drifting up

            p.rouletteStyle = RouletteStyle.PingPong;        // VU meter going up and down
            p.minInterval = 0.06f;
            p.maxInterval = 0.5f;                            // last steps on a 120 BPM beat
            p.nearMissChance = 0.65f;                        // "one more bar..."
            p.minRouletteSeconds = 3.0f;                     // matches the 3 s drum roll

            p.rayLengthMultiplier = 0.9f;
            p.rayDuration = 1.0f;
            p.raySpin = 60f;                                 // stage spotlights sweeping
            p.lightFlashIntensity = 8f;
        }
    }
}

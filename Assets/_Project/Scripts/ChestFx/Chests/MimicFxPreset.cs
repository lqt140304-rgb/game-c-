using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Mimic: a hungry chest with teeth. Roulette = erratic "đói bụng" shuffle with a heartbeat tick,
    /// particles = slimy drool blobs spat up and falling back (fountain + heavy gravity), sickly green slime as the
    /// secondary colour, short fat rays turning backwards, red-orange flash. The bite, heartbeat pulse, drool drips and
    /// the Rác "vomit" are in <see cref="MimicHook"/>. Sounds: Assets/_Project/Audio/Chests/mimic/.
    /// </summary>
    public static class MimicFxPreset
    {
        public const string AudioFolder = "Assets/_Project/Audio/Chests/mimic/";
        public static readonly string[] TickFiles = { "sfx_mimic_tick_heartbeat.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_mimic_lock_rac.mp3", "sfx_mimic_lock_thuong.mp3", "sfx_mimic_lock_hiem.mp3",
            "sfx_mimic_lock_ratHiem.mp3", "sfx_mimic_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_mimic_whoosh_spit.mp3";
        public const string AmbientFile = "sfx_mimic_growl_bed.mp3";
        public static readonly string[] AccentFiles = { "sfx_mimic_lick.mp3", "sfx_mimic_drool.mp3", "sfx_mimic_teeth_chatter.mp3" };
        public const string CreakFile = "sfx_mimic_creak_jaw.mp3";
        public const string BiteFile = "sfx_mimic_bite.mp3";
        public const string ChatterFile = "sfx_mimic_teeth_chatter.mp3";
        public const string DroolFile = "sfx_mimic_drool.mp3";

        public static readonly Color Slime = new Color(0.62f, 0.95f, 0.25f);

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = "Chest_Mimic";

            p.tickVolume = 0.6f;
            p.tickPitchStart = 0.7f;    // slow, low heartbeat...
            p.tickPitchEnd = 1.25f;     // ...that races as the roulette slows (it is about to eat)
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.45f;
            p.accentVolume = 0.4f;
            p.accentsBase = 0;          // Rác: no lick, just the burp
            p.accentsPerTier = 1;

            p.secondaryColor = Slime;
            p.secondaryMix = 0.6f;
            p.particleLook = ParticleLook.SoftDot;            // wet blobs, not sparkles
            p.particleDirection = ParticleDirection.Fountain; // spat up, falls back down
            p.ambientRate = 14f;
            p.burstCount = 55;
            p.particleSpeed = 0.9f;
            p.particleSize = 0.04f;
            p.particleLifetime = 1.1f;
            p.burstGravity = 1.6f;      // heavier than gold coins: slime splats down fast

            p.rouletteStyle = RouletteStyle.Shuffle;   // hungry, twitchy, no order
            p.minInterval = 0.06f;
            p.maxInterval = 0.5f;
            p.nearMissChance = 0.7f;    // "nuốt rồi nhả": slips past then spits back
            p.minRouletteSeconds = 3.0f;

            p.rayLengthMultiplier = 0.75f;  // short fat "teeth" rays
            p.rayDuration = 0.9f;
            p.raySpin = -40f;               // turns the other way from every other chest
            p.lightFlashIntensity = 8f;
        }
    }
}

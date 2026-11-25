using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_NhatBan (Daruma doll): pink sakura petals swirl round the egg-shaped doll, the roulette rocks back and
    /// forth like a Daruma (PingPong) with wooden temple-block ticks, the lock "paints the second eye" with a bell.
    /// Audio file names live here; Editor/NhatBanFxSetup.cs loads them. See docs/superpowers/specs/2026-11-24-hieu-ung-nhat-ban-design.md.
    /// </summary>
    public static class NhatBanFxPreset
    {
        public const string ChestId = "Chest_NhatBan";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/nhat_ban/";

        public static readonly string[] TickFiles = { "sfx_nhatban_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_nhatban_lock_rac.mp3", "sfx_nhatban_lock_thuong.mp3", "sfx_nhatban_lock_hiem.mp3",
            "sfx_nhatban_lock_ratHiem.mp3", "sfx_nhatban_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_nhatban_grow.mp3";
        public const string AmbientFile = "sfx_nhatban_ambient.mp3";
        public const string LidOpenFile = "sfx_nhatban_lid_open.mp3";
        public const string LidThudFile = "sfx_nhatban_lid_thud.mp3";
        public static readonly string[] AccentFiles = { "sfx_nhatban_chime_a.mp3", "sfx_nhatban_chime_b.mp3", "sfx_nhatban_koto_pluck.mp3" };

        /// <summary>Sakura pink (secondary colour of the petals).</summary>
        public static readonly Color Sakura = new Color(1f, 0.72f, 0.82f);

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.6f;
            p.tickPitchStart = 0.85f;   // low wood block, rises like a quickening festival beat
            p.tickPitchEnd = 1.35f;
            p.lockVolume = 0.9f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.4f;     // taiko roll + wind chimes under the roulette
            p.accentVolume = 0.4f;
            p.accentsBase = 1;          // chimes / koto after the lock: 1 for Rác ... 5 for Huyền thoại
            p.accentsPerTier = 1;

            p.secondaryColor = Sakura;
            p.secondaryMix = 0.75f;     // mostly pink petals, tinted by the tier colour
            p.particleLook = ParticleLook.SoftDot;
            p.particleDirection = ParticleDirection.Swirl;
            p.ambientRate = 26f;
            p.burstCount = 55;
            p.particleSpeed = 0.45f;    // petals drift, they do not shoot
            p.particleSize = 0.032f;
            p.particleLifetime = 1.9f;
            p.burstGravity = 0.12f;     // burst petals sink slowly

            p.rouletteStyle = RouletteStyle.PingPong;   // rocks up and down like a Daruma
            p.minInterval = 0.06f;
            p.maxInterval = 0.5f;
            p.nearMissChance = 0.65f;   // the doll "almost tips over" one tier further, then rights itself
            p.minRouletteSeconds = 3.0f;

            p.rayLengthMultiplier = 0.85f;   // short rays: the spotlight is on the bell and the eye
            p.rayDuration = 1.4f;
            p.raySpin = 8f;                  // slow, calm (sun-disc feel)
            p.lightFlashIntensity = 5.5f;
        }
    }
}

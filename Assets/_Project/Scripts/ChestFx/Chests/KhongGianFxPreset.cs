using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_KhongGian (landing capsule: bell body on 3 legs, orange heat shield, round window, antenna).
    /// Theme: radio countdown and radar scan. The roulette jumps around like a radio dial hunting for a signal
    /// (Shuffle), star dust streaks out in zero gravity (Streak + Radial, no gravity), cold cyan-white secondary
    /// colour, slow ray spin (drifting in space). KhongGianHook adds sonar rings, a star field, meteors and
    /// retro-rocket puffs. Audio files: Assets/_Project/Audio/Chests/khong_gian/ (see README there).
    /// </summary>
    public static class KhongGianFxPreset
    {
        public const string ChestId = "Chest_KhongGian";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/khong_gian/";

        public static readonly string[] TickFiles = { "sfx_khonggian_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_khonggian_lock_rac.mp3", "sfx_khonggian_lock_thuong.mp3", "sfx_khonggian_lock_hiem.mp3",
            "sfx_khonggian_lock_ratHiem.mp3", "sfx_khonggian_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_khonggian_whoosh_warp.mp3";
        public const string AmbientFile = "sfx_khonggian_countdown.mp3";
        public const string StaticFile = "sfx_khonggian_static_burst.mp3";
        public const string HatchFile = "sfx_khonggian_hatch_hiss.mp3";
        public const string LandingFile = "sfx_khonggian_landing_thud.mp3";
        public const string MeteorFile = "sfx_khonggian_meteor.mp3";

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            // Audio levels (clips are attached by Editor/KhongGianFxSetup.cs).
            p.tickVolume = 0.45f;
            p.tickPitchStart = 0.8f;     // sonar ping starts low and climbs as the scan "locks on"
            p.tickPitchEnd = 1.6f;
            p.lockVolume = 0.9f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.35f;     // radio countdown bed (3 s) under the roulette
            p.accentVolume = 0.35f;
            p.accentsBase = 0;           // Rác: no meteors, just the "Houston" sputter
            p.accentsPerTier = 1;        // up to 4 shooting stars for Huyền thoại

            // Particles: star dust in zero gravity.
            p.secondaryColor = new Color(0.55f, 0.9f, 1f);   // cold cyan-white starlight
            p.secondaryMix = 0.6f;
            p.particleLook = ParticleLook.Streak;
            p.particleDirection = ParticleDirection.Radial;
            p.ambientRate = 18f;         // sparse: space is mostly empty
            p.burstCount = 55;
            p.particleSpeed = 1.1f;      // fast streaks
            p.particleSize = 0.018f;
            p.particleLifetime = 1.6f;
            p.burstGravity = 0f;         // floats forever

            // Roulette: a dial hunting for a signal, slows down hard at the end.
            p.rouletteStyle = RouletteStyle.Shuffle;
            p.minInterval = 0.05f;
            p.maxInterval = 0.5f;
            p.slowdownCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.65f, 0.12f, 0.5f, 0.5f), new Keyframe(1f, 1f, 3.2f, 0f));
            p.nearMissChance = 0.7f;     // "signal lost... re-acquired"
            p.minRouletteSeconds = 3.0f; // matches the 3 s countdown bed

            // Rays: long, thin-feeling, drifting slowly.
            p.rayLengthMultiplier = 1.35f;
            p.rayDuration = 1.4f;
            p.raySpin = -8f;             // slow reverse spin, unlike the other chests
            p.lightFlashIntensity = 5.5f;
        }
    }
}

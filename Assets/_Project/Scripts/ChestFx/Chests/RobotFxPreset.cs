using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Robot (tin wind-up robot, the domed head is the lid): the roulette climbs up and down like a battery
    /// meter (PingPong), electric sparks (Streak, Radial, falling a little), cyan secondary colour, short rays that
    /// spin like a cog. Clips are filled by name in Editor/RobotFxSetup.cs. See RobotHook for the eye blink and cog.
    /// </summary>
    public static class RobotFxPreset
    {
        public const string ChestId = "Chest_Robot";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/robot/";

        public static readonly string[] TickFiles = { "sfx_robot_tick.mp3", "sfx_robot_tick_ratchet.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_robot_lock_rac.mp3", "sfx_robot_lock_thuong.mp3", "sfx_robot_lock_hiem.mp3",
            "sfx_robot_lock_ratHiem.mp3", "sfx_robot_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_robot_whoosh.mp3";
        public const string AmbientFile = "sfx_robot_gears.mp3";
        public static readonly string[] AccentFiles = { "sfx_robot_spark.mp3", "sfx_robot_babble.mp3", "sfx_robot_steam.mp3", "sfx_robot_servo.mp3" };
        public const string WindupFile = "sfx_robot_windup.mp3";   // stage 0 (Rung): the key is wound
        public const string SteamFile = "sfx_robot_steam.mp3";     // stage 1 (Go niem): joints release air
        public const string ServoFile = "sfx_robot_servo.mp3";     // stage 4 (Mo toang): head-lid clanks fully open

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.45f;
            p.tickPitchStart = 0.85f;
            p.tickPitchEnd = 1.7f;          // beeps rise like a computer counting up
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.4f;         // clockwork whirring under the roulette
            p.accentVolume = 0.45f;
            p.accentsBase = 1;
            p.accentsPerTier = 1;

            p.secondaryColor = new Color(0.35f, 0.95f, 1f);   // electric cyan
            p.secondaryMix = 0.6f;
            p.particleLook = ParticleLook.Streak;              // sparks
            p.particleDirection = ParticleDirection.Radial;
            p.ambientRate = 18f;
            p.burstCount = 55;
            p.particleSpeed = 1.1f;
            p.particleSize = 0.018f;
            p.particleLifetime = 0.7f;
            p.burstGravity = 0.6f;          // sparks arc and drop

            p.rouletteStyle = RouletteStyle.PingPong;          // battery meter charging up and down
            p.minInterval = 0.06f;
            p.maxInterval = 0.5f;
            p.slowdownCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.7f, 0.25f, 0.9f, 0.9f), new Keyframe(1f, 1f, 3.2f, 0f));
            p.nearMissChance = 0.65f;      // robot "glitches" one step too far, then corrects
            p.minRouletteSeconds = 3.0f;   // a bit longer to fit the wind-up

            p.rayLengthMultiplier = 0.8f;   // short stubby rays like cog teeth
            p.rayDuration = 0.9f;
            p.raySpin = 120f;
            p.lightFlashIntensity = 8f;
        }
    }
}

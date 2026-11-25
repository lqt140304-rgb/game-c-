using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_ThienCo (rocket-shaped tech artifact): precision mechanics + cyan energy. The roulette climbs and
    /// falls like a pressure gauge (PingPong) with ratchet ticks, cyan streaks shoot straight up like exhaust,
    /// rays are long, thin and spin fast. Code extras live in <see cref="ThienCoHook"/> (gear that advances one
    /// tooth per tick, lock rings opening one after another, thruster column). Clips are filled by
    /// Editor/ThienCoFxSetup.cs from Assets/_Project/Audio/Chests/thien_co/.
    /// </summary>
    public static class ThienCoFxPreset
    {
        public const string ChestId = "Chest_ThienCo";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/thien_co/";

        public static readonly string[] TickFiles = { "sfx_thienco_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_thienco_lock_rac.mp3", "sfx_thienco_lock_thuong.mp3", "sfx_thienco_lock_hiem.mp3",
            "sfx_thienco_lock_ratHiem.mp3", "sfx_thienco_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_thienco_whoosh_grow.mp3";
        public const string AmbientFile = "sfx_thienco_ambient_clockwork.mp3";
        public static readonly string[] AccentFiles = { "sfx_thienco_hiss.mp3", "sfx_thienco_servo.mp3", "sfx_thienco_ping.mp3" };
        public const string UnsealFile = "sfx_thienco_hatch_unseal.mp3";   // stage 2 He nap
        public const string HatchOpenFile = "sfx_thienco_hatch_open.mp3";  // stage 4 Mo toang

        public static readonly Color Cyan = new Color(0.25f, 0.95f, 1f);

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.3f;          // tích-tắc riêng của ThienCoHook dẫn nhịp; tick vòng quay chỉ đệm
            p.tickPitchStart = 1.1f;     // ratchet: starts tight and high...
            p.tickPitchEnd = 0.8f;       // ...and drops as the mechanism winds down (others rise)
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.6f;
            p.ambientVolume = 0.15f;
            p.accentVolume = 0.4f;
            p.accentsBase = 0;           // Rác: no accent, the jam sound is enough
            p.accentsPerTier = 1;

            p.secondaryColor = Cyan;
            p.secondaryMix = 0.7f;
            p.particleLook = ParticleLook.Streak;
            p.particleDirection = ParticleDirection.Up;
            p.ambientRate = 40f;
            p.burstCount = 55;
            p.particleSpeed = 1.3f;      // exhaust: fast and straight
            p.particleSize = 0.018f;
            p.particleLifetime = 0.7f;
            p.burstGravity = 0f;

            p.rouletteStyle = RouletteStyle.PingPong;   // pressure gauge sweeping up and down
            p.minInterval = 0.05f;
            p.maxInterval = 0.38f;
            p.slowdownCurve = new AnimationCurve(      // stays fast longer, then brakes hard (mechanical brake)
                new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.7f, 0.12f, 0.4f, 0.4f), new Keyframe(1f, 1f, 4f, 0f));
            p.nearMissChance = 0.4f;
            p.minRouletteSeconds = 3.0f;

            p.rayLengthMultiplier = 1.35f;
            p.rayDuration = 0.9f;
            p.raySpin = 60f;
            p.lightFlashIntensity = 8f;
        }
    }
}

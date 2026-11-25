using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_ThienCo (rocket-shaped tech artifact): precision mechanics + cyan energy. The roulette climbs and
    /// falls like a pressure gauge (PingPong) with ratchet ticks, cyan streaks shoot straight up like exhaust,
    /// rays are long, thin and spin fast. Code extras live in <see cref="ThienCoHook"/> (gear that advances one
    /// tooth per tick, lock rings opening one after another, thruster column). Clips are filled by
    /// Editor/ThienCoFxSetup.cs from Assets/_Project/Audio/Chests/thien_co/ (ElevenLabs .mp3 only).
    /// </summary>
    public static class ThienCoFxPreset
    {
        public const string ChestId = "Chest_ThienCo";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/thien_co/";

        /// <summary>TC theo bậc [Rác, Thường, Hiếm, Rất hiếm, Huyền thoại]: hook phát qua stingClips (A11), lõi câm (A1).</summary>
        public static readonly string[] LockFiles =
        {
            "sfx_thienco_lock_rac.mp3", "sfx_thienco_lock_thuong.mp3", "sfx_thienco_lock_hiem.mp3",
            "sfx_thienco_lock_ratHiem.mp3", "sfx_thienco_lock_huyenThoai.mp3",
        };

        public static readonly Color Cyan = new Color(0.25f, 0.95f, 1f);

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            // Âm: không chỉnh tick/nền/chốt của lõi ở đây; ChestAudioRule.ApplyToProfile (gọi trong Setup) làm lõi câm (A1).

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

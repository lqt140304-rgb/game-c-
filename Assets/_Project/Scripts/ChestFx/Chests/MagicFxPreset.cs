using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Magic (stone chest with glowing purple crystals): crystal sparkles spiral round the chest, the
    /// colours jump in random "spell flickers" (Shuffle) with a long hesitant tail, a dense slowly-turning star
    /// of rays, and a violet light that pulses. Sounds: crystal taps, crystal hum + cave whisper, stone lid.
    /// Clips are filled by Editor/MagicFxSetup.cs. Extra code effect: <see cref="MagicHook"/>.
    /// </summary>
    public static class MagicFxPreset
    {
        public const string ChestId = "Chest_Magic";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/magic/";

        public static readonly string[] TickFiles = { "sfx_magic_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_magic_lock_rac.mp3", "sfx_magic_lock_thuong.mp3", "sfx_magic_lock_hiem.mp3",
            "sfx_magic_lock_ratHiem.mp3", "sfx_magic_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_magic_whoosh_grow.mp3";
        public const string AmbientFile = "sfx_magic_crystal_hum.mp3";
        public static readonly string[] AccentFiles = { "sfx_magic_shard_tinkle.mp3" };
        public const string WhisperFile = "sfx_magic_cave_whisper.mp3";   // stage 1 (Go niem): the seal breaks
        public const string LidFile = "sfx_magic_stone_lid.mp3";          // stage 2 (He nap)
        public const string ThudFile = "sfx_magic_stone_thud.mp3";        // stage 4 (Mo toang)

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            // Crystal taps: start low, ring up high like rubbing up a glass harp.
            p.tickVolume = 0.45f;
            p.tickPitchStart = 0.8f;
            p.tickPitchEnd = 1.8f;
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.45f;
            p.accentVolume = 0.4f;
            p.accentsBase = 0;       // Rác: no tinkle (the fizzle is the joke)
            p.accentsPerTier = 1;

            p.secondaryColor = new Color(0.78f, 0.55f, 1f);   // crystal violet glow
            p.secondaryMix = 0.6f;
            p.particleLook = ParticleLook.Sparkle;
            p.particleDirection = ParticleDirection.Swirl;
            p.ambientRate = 38f;
            p.burstCount = 55;
            p.particleSpeed = 0.45f;   // slow, floating
            p.particleSize = 0.022f;
            p.particleLifetime = 1.8f;
            p.burstGravity = -0.08f;   // lock burst drifts UP like released magic

            p.rouletteStyle = RouletteStyle.Shuffle;   // flickering spell, not a slot reel
            p.minInterval = 0.06f;
            p.maxInterval = 0.5f;
            p.slowdownCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.7f, 0.12f, 0.4f, 0.4f), new Keyframe(1f, 1f, 4f, 0f));
            p.nearMissChance = 0.65f;
            p.minRouletteSeconds = 3.0f;

            p.rayLengthMultiplier = 1.3f;
            p.rayDuration = 1.5f;
            p.raySpin = -12f;           // slow counter-clockwise turn
            p.lightFlashIntensity = 8f;
        }
    }
}

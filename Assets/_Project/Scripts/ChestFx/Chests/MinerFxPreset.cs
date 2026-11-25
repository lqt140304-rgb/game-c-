using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Miner (old flat miner's crate with crossed pickaxes): rock dust and gravel that shoot up and fall back
    /// hard, a colour roulette that jumps around like a pickaxe chipping at random ore veins, short stubby rays that
    /// barely turn (cracks in rock), a warm dim lamp flash. MinerHook adds the flickering lamp, pick sparks and rock chunks.
    /// Audio files: Assets/_Project/Audio/Chests/miner/ (see README there); MinerFxSetup fills them by name.
    /// </summary>
    public static class MinerFxPreset
    {
        public const string ChestId = "Chest_Miner";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/miner/";

        public static readonly string[] TickFiles = { "sfx_miner_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_miner_lock_rac.mp3", "sfx_miner_lock_thuong.mp3", "sfx_miner_lock_hiem.mp3",
            "sfx_miner_lock_ratHiem.mp3", "sfx_miner_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_miner_whoosh.mp3";
        public const string AmbientFile = "sfx_miner_ambient.mp3";
        public const string LidFile = "sfx_miner_lid.mp3";
        public const string LanternFile = "sfx_miner_lantern.mp3";
        public const string CartFile = "sfx_miner_cart.mp3";
        public static readonly string[] AccentFiles = { "sfx_miner_pebbles.mp3" };

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.6f;
            p.tickPitchStart = 0.8f;    // low thunk of iron on stone...
            p.tickPitchEnd = 1.25f;     // ...brighter as the pick slows down
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.45f;
            p.accentVolume = 0.5f;
            p.accentsBase = 1;          // pebbles keep rattling down after the lock
            p.accentsPerTier = 1;

            p.secondaryColor = new Color(0.45f, 0.40f, 0.35f);   // rock dust grey-brown
            p.secondaryMix = 0.7f;                                // mostly dust, a little tier colour
            p.particleLook = ParticleLook.SoftDot;
            p.particleDirection = ParticleDirection.Fountain;
            p.ambientRate = 14f;
            p.burstCount = 55;
            p.particleSpeed = 0.9f;
            p.particleSize = 0.022f;
            p.particleLifetime = 1.1f;
            p.burstGravity = 1.6f;    // heavier than Golden's coins: gravel drops fast

            p.rouletteStyle = RouletteStyle.Shuffle;   // random ore veins, not a tidy slot machine
            p.minInterval = 0.07f;                     // pick rhythm, a touch slower than the default
            p.maxInterval = 0.5f;
            p.slowdownCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.6f, 0.15f, 0.5f, 0.5f), new Keyframe(1f, 1f, 3.2f, 0f));
            p.nearMissChance = 0.65f;                  // "almost hit the vein" more often
            p.minRouletteSeconds = 3.0f;

            p.rayLengthMultiplier = 0.8f;   // short, like cracks
            p.rayDuration = 0.9f;
            p.raySpin = 4f;
            p.lightFlashIntensity = 5f;     // dim lamp, the hook adds the blast stutter
        }
    }
}

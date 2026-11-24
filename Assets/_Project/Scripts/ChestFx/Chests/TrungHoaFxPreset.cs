using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TrungHoa (bronze ding cauldron): incense smoke curling up out of the cauldron, a "drum beat"
    /// PingPong roulette, slow jade-tinted rays and a warm lantern-red flash. Sounds are wired by
    /// Editor/TrungHoaFxSetup.cs (Audio/Chests/trung_hoa/). Extra firecrackers and sky lanterns: TrungHoaHook.
    /// </summary>
    public static class TrungHoaFxPreset
    {
        public const string ChestId = "Chest_TrungHoa";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/trung_hoa/";

        public static readonly string[] TickFiles = { "sfx_trunghoa_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_trunghoa_lock_rac.mp3", "sfx_trunghoa_lock_thuong.mp3", "sfx_trunghoa_lock_hiem.mp3",
            "sfx_trunghoa_lock_ratHiem.mp3", "sfx_trunghoa_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_trunghoa_whoosh.mp3";
        public const string AmbientFile = "sfx_trunghoa_ambient.mp3";
        public const string LidFile = "sfx_trunghoa_lid.mp3";
        public const string ThudFile = "sfx_trunghoa_thud.mp3";
        public static readonly string[] AccentFiles = { "sfx_trunghoa_firecracker.mp3", "sfx_trunghoa_bell.mp3" };

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            // Wooden temple-block tick: low pitch at the start, climbing like a drummer speeding up then slowing.
            p.tickVolume = 0.6f;
            p.tickPitchStart = 0.8f;
            p.tickPitchEnd = 1.3f;
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.4f;
            p.accentVolume = 0.45f;
            p.accentsBase = 0;       // Rác gets no festival, just the abacus joke
            p.accentsPerTier = 1;    // firecracker / bell are 1.5-2 s long

            // Incense smoke: soft dots drifting up slowly, long-lived, mixed with pale jade (bronze patina).
            p.secondaryColor = new Color(0.55f, 0.85f, 0.72f);
            p.secondaryMix = 0.45f;
            p.particleLook = ParticleLook.SoftDot;
            p.particleDirection = ParticleDirection.Up;
            p.ambientRate = 16f;
            p.burstCount = 34;
            p.particleSpeed = 0.28f;
            p.particleSize = 0.045f;
            p.particleLifetime = 2.6f;
            p.burstGravity = -0.05f; // smoke keeps rising a little after the lock

            // Roulette: PingPong = a festival drum climbing up the tiers and back down.
            p.rouletteStyle = RouletteStyle.PingPong;
            p.minInterval = 0.06f;
            p.maxInterval = 0.5f;
            p.nearMissChance = 0.45f;
            p.minRouletteSeconds = 3.2f;

            // Long, slowly turning rays (like a sun disc behind a temple roof) and a lantern flash.
            p.rayLengthMultiplier = 1.3f;
            p.rayDuration = 1.6f;
            p.raySpin = 8f;
            p.lightFlashIntensity = 6.5f;
        }
    }
}

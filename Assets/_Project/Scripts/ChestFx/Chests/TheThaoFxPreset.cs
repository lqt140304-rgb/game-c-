using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TheThao (split low-poly football on an orange tee): "match day". The roulette swings up and down
    /// like a power-shot meter (PingPong), ticks are ball kicks, confetti streams shoot up and fall back with gravity,
    /// rays are short and spin fast like a spinning ball. Locks go from a whiffed kick (Rác) to a whole stadium goal roar
    /// (Huyền thoại). Extras (side confetti cannons, fireworks, whistle, air horn) live in <see cref="TheThaoHook"/>.
    /// Clips are filled by Editor/TheThaoFxSetup.cs.
    /// </summary>
    public static class TheThaoFxPreset
    {
        public const string ChestId = "Chest_TheThao";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/the_thao/";

        public static readonly string[] TickFiles = { "sfx_thethao_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_thethao_lock_rac.mp3", "sfx_thethao_lock_thuong.mp3", "sfx_thethao_lock_hiem.mp3",
            "sfx_thethao_lock_ratHiem.mp3", "sfx_thethao_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_thethao_whoosh.mp3";
        public const string AmbientFile = "sfx_thethao_ambient.mp3";
        public static readonly string[] AccentFiles = { "sfx_thethao_clap.mp3", "sfx_thethao_bounce.mp3" };
        public const string LidFile = "sfx_thethao_lid.mp3";
        public const string KickoffFile = "sfx_thethao_kickoff.mp3";
        public const string AirHornFile = "sfx_thethao_airhorn.mp3";

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.6f;
            p.tickPitchStart = 0.85f;   // heavy kicks first, lighter taps as it slows
            p.tickPitchEnd = 1.3f;
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.3f;
            p.accentVolume = 0.4f;
            p.accentsBase = 0;          // Rác: nobody claps
            p.accentsPerTier = 1;

            p.secondaryColor = new Color(1f, 0.55f, 0.1f);   // tee orange
            p.secondaryMix = 0.5f;
            p.particleLook = ParticleLook.Streak;            // confetti strips
            p.particleDirection = ParticleDirection.Fountain;
            p.ambientRate = 18f;
            p.burstCount = 55;
            p.particleSpeed = 1.1f;
            p.particleSize = 0.022f;
            p.particleLifetime = 1.6f;
            p.burstGravity = 0.7f;

            p.rouletteStyle = RouletteStyle.PingPong;        // power-shot meter
            p.minInterval = 0.05f;
            p.maxInterval = 0.4f;
            p.nearMissChance = 0.65f;                         // "hit the post" moment
            p.minRouletteSeconds = 3.0f;

            p.rayLengthMultiplier = 0.85f;
            p.rayDuration = 0.9f;
            p.raySpin = 70f;
            p.lightFlashIntensity = 6.5f;
        }
    }
}

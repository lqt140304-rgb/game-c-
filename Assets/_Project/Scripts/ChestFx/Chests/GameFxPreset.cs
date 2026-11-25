using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Game (giant gamepad on a round stand): 8-bit arcade style. The roulette walks up and down like a
    /// power meter in hard steps (stepped slowdown curve), particles burst out in all directions with no spin,
    /// the rays snap without spinning, and the sounds are chiptune blips and jingles (Audio/Chests/game/).
    /// The square "pixel" particles come from <see cref="GamePixelHook"/>.
    /// </summary>
    public static class GameFxPreset
    {
        public const string ChestId = "Chest_Game";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/game/";

        public static readonly string[] TickFiles = { "sfx_game_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_game_lock_rac.mp3", "sfx_game_lock_thuong.mp3", "sfx_game_lock_hiem.mp3", "sfx_game_lock_ratHiem.mp3", "sfx_game_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_game_whoosh_grow.mp3";
        public const string AmbientFile = "sfx_game_ambient_select.mp3";
        public static readonly string[] AccentFiles = { "sfx_game_coin.mp3", "sfx_game_jump.mp3" };
        public const string ShakeFile = "sfx_game_button_click.mp3";   // stage 0 Rung: someone mashing a button
        public const string BootFile = "sfx_game_boot.mp3";            // stage 2 He nap: console powers on
        public const string StartFile = "sfx_game_press_start.mp3";    // stage 4 Mo toang: "press start"

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            // Audio: chiptune tick is very short (0.1 s), so it stays crisp at 0.06 s gaps.
            p.tickVolume = 0.45f;
            p.tickPitchStart = 1f;
            p.tickPitchEnd = 1.5f;
            p.lockVolume = 0.85f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.3f;
            p.accentVolume = 0.4f;
            p.accentsBase = 0;       // Rác: no coins, just the game-over jingle
            p.accentsPerTier = 2;    // Huyền thoại: 8 coin/jump blips

            // Particles: screen-green glow mixed into the tier colour, burst in all directions, short-lived like sprites.
            p.secondaryColor = new Color(0.35f, 1f, 0.55f);
            p.secondaryMix = 0.35f;
            p.particleLook = ParticleLook.Sparkle;
            p.particleDirection = ParticleDirection.Radial;
            p.ambientRate = 18f;
            p.burstCount = 36;
            p.particleSpeed = 0.55f;
            p.particleSize = 0.022f;
            p.particleLifetime = 0.9f;
            p.burstGravity = 0.35f;

            // Roulette: power meter (up and down), slowing in hard steps like a scoreboard.
            p.rouletteStyle = RouletteStyle.PingPong;
            p.minInterval = 0.06f;
            p.maxInterval = 0.4f;
            p.slowdownCurve = SteppedCurve();
            p.nearMissChance = 0.7f;   // "suýt nữa thì..." is the gamer experience
            p.minRouletteSeconds = 2.8f;

            // Rays: no spin (pixel art does not rotate), short and snappy.
            p.rayLengthMultiplier = 1f;
            p.rayDuration = 0.8f;
            p.raySpin = 0f;
            p.lightFlashIntensity = 6.5f;
        }

        /// <summary>Interval curve in 5 flat steps (constant tangents): the tick speed drops gear by gear.</summary>
        public static AnimationCurve SteppedCurve()
        {
            float inf = float.PositiveInfinity;
            return new AnimationCurve(
                new Keyframe(0f, 0f, inf, inf),
                new Keyframe(0.35f, 0.12f, inf, inf),
                new Keyframe(0.6f, 0.3f, inf, inf),
                new Keyframe(0.8f, 0.55f, inf, inf),
                new Keyframe(0.93f, 1f, inf, inf));
        }
    }
}

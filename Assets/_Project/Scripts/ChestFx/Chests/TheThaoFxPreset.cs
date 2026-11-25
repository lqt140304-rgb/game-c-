using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TheThao (split low-poly football on an orange tee): "match day". The roulette swings up and down
    /// like a power-shot meter (PingPong), ticks are ball kicks, confetti streams shoot up and fall back with gravity,
    /// rays are short and spin fast like a spinning ball. Locks go from a whiffed kick (Rác) to a whole stadium goal roar
    /// (Huyền thoại). Extras (kick-off whistle, air horn, drums) live in <see cref="TheThaoHook"/>.
    /// Hook clips are .mp3 (ElevenLabs) filled by Editor/TheThaoFxSetup.cs; core profile audio is empty (ChestAudioRule).
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

#if UNITY_EDITOR
        /// <summary>
        /// Gán các clip .mp3 ElevenLabs của <see cref="TheThaoHook"/> (theo ô `field` trong Tools/Audio/elevenlabs_chest_manifest.json,
        /// mục the_thao). Không nạp .wav numpy nữa (A9). <paramref name="overwrite"/> = true (menu Setup): ghi đè ô đã có;
        /// false (OnValidate): chỉ điền ô còn trống. File mp3 thiếu thì để nguyên ô (báo trong log của Setup).
        /// </summary>
        public static int FillHookClips(TheThaoHook h, bool overwrite)
        {
            if (h == null) return 0;
            int missing = 0;
            Set(ref h.kickoffClip, KickoffFile, overwrite, ref missing);
            Set(ref h.airHornClip, AirHornFile, overwrite, ref missing);
            Set(ref h.drumClip, "sfx_thethao_trong.mp3", overwrite, ref missing);
            Set(ref h.kickClip, "sfx_thethao_kick.mp3", overwrite, ref missing);
            Set(ref h.bounceClip, "sfx_thethao_bounce.mp3", overwrite, ref missing);
            Set(ref h.postClip, "sfx_thethao_post.mp3", overwrite, ref missing);
            Set(ref h.netClip, "sfx_thethao_net.mp3", overwrite, ref missing);
            Set(ref h.oohClip, "sfx_thethao_ooh.mp3", overwrite, ref missing);
            Set(ref h.roarClip, "sfx_thethao_roar.mp3", overwrite, ref missing);
            Set(ref h.awwClip, "sfx_thethao_aww.mp3", overwrite, ref missing);
            Set(ref h.riserClip, "sfx_thethao_riser.mp3", overwrite, ref missing);
            Set(ref h.clapClip, "sfx_thethao_clap.mp3", overwrite, ref missing);
            if (h.stingClips == null || h.stingClips.Length != LockFiles.Length) h.stingClips = new AudioClip[LockFiles.Length];
            for (int i = 0; i < LockFiles.Length; i++) Set(ref h.stingClips[i], LockFiles[i], overwrite, ref missing);
            return missing;
        }

        private static void Set(ref AudioClip field, string file, bool overwrite, ref int missing)
        {
            if (field != null && !overwrite) return;
            AudioClip c = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + file);
            if (c != null) field = c;
            else if (field == null) missing++;
        }
#endif

        /// <summary>A1: hồ sơ lõi gần như câm. Gọi MỖI LẦN chạy Setup.</summary>
        public static void ApplyToProfile(ChestFxProfile p) => ChestAudioRule.ApplyToProfile(p);

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;
            // Âm căng thẳng chung của lõi TẮT: TheThaoHook tự phát nhịp trống/riser và tiếng chốt theo bậc (stingClips).
            p.tension.enabled = false;

            p.tickVolume = 0.6f;
            p.tickPitchStart = 0.85f;   // heavy kicks first, lighter taps as it slows
            p.tickPitchEnd = 1.3f;
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0f;     // nền do hook lo (lõi câm, A1)
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

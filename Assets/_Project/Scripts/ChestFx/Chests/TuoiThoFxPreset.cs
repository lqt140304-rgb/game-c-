using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TuoiTho — "cỗ máy bi ve" (trò chơi tuổi thơ). Hiệu ứng chính nằm ở <see cref="TuoiThoHook"/>
    /// (vật lý: bi, domino, xe, con quay, ná dây thun, máy bay giấy). Profile này chỉ giữ phần khung (hạt, màu, vòng
    /// roulette). Mọi âm do hook phát (ChestAudioRule A1): tiếng chốt theo bậc là đồ chơi (pin hết / chuông xe đạp / còi
    /// xe đồ chơi / hộp nhạc / hội) nằm ở TuoiThoHook.stingClips, nền = tiếng bi lăn, nhịp = mộc cầm.
    /// Âm ElevenLabs ở Audio/Chests/tuoi_tho/.
    /// </summary>
    public static class TuoiThoFxPreset
    {
        public const string ChestId = "Chest_TuoiTho";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/tuoi_tho/";

        public static readonly string[] TickFiles = { "sfx_tuoitho_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_tuoitho_lock_rac.mp3", "sfx_tuoitho_lock_thuong.mp3", "sfx_tuoitho_lock_hiem.mp3",
            "sfx_tuoitho_lock_ratHiem.mp3", "sfx_tuoitho_lock_huyenThoai.mp3",
        };
        public const string GrowFile = "sfx_tuoitho_grow.mp3";
        public const string MachineFolder = AudioFolder + "may_bi/";
        public static readonly string[] AccentFiles =
        {
            "sfx_tuoitho_bells.mp3", "sfx_tuoitho_marbles.mp3", "sfx_tuoitho_frogdrum.mp3",
        };
        public const string BambooFile = "sfx_tuoitho_bamboo.mp3";     // stage 2 Hé nắp
        public const string CandyTinFile = "sfx_tuoitho_candytin.mp3"; // stage 4 Mở toang

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.6f;          // bi ve gõ nhau: ngắn, khô
            p.tickPitchStart = 1.0f;
            p.tickPitchEnd = 1.3f;
            p.lockVolume = 0.9f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0f;          // nền do TuoiThoHook lo
            p.accentVolume = 0.4f;
            p.accentsBase = 0;            // Rác: không lục lạc, chỉ tiếng pin hết
            p.accentsPerTier = 1;

            p.secondaryColor = new Color(1f, 0.55f, 0.2f);   // ánh nến cam
            p.secondaryMix = 0.6f;
            p.particleLook = ParticleLook.SoftDot;           // đom đóm
            p.particleDirection = ParticleDirection.Swirl;
            p.ambientRate = 14f;          // thưa, lững lờ
            p.burstCount = 30;
            p.particleSpeed = 0.35f;
            p.particleSize = 0.02f;
            p.particleLifetime = 2.2f;
            p.burstGravity = -0.05f;      // hơi bay lên như hơi nóng của nến

            p.rouletteStyle = RouletteStyle.PingPong;        // đèn đung đưa qua lại
            p.minInterval = 0.07f;
            p.maxInterval = 0.5f;
            p.slowdownCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0.4f), new Keyframe(0.6f, 0.3f, 0.9f, 0.9f), new Keyframe(1f, 1f, 2f, 0f));
            p.nearMissChance = 0.4f;
            p.minRouletteSeconds = 3.0f;

            p.rayLengthMultiplier = 0.9f;
            p.rayDuration = 1.6f;         // tia tan chậm như đèn tắt dần
            p.raySpin = -12f;             // xoay ngược, chậm
            p.lightFlashIntensity = 4.5f; // ấm, không chói
        }
    }
}

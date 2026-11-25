using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_NhatBan — kịch bản "NHÁT KIẾM IAI" (làm lại 2026-11-25, thay bản Daruma cũ).
    /// Một thanh katana trong vỏ hiện cạnh rương ngay khi bấm mở; tiếng tim taiko dồn dần; thời gian chậm lại,
    /// một nhát chém vụt qua, dây thừng và giấy niêm VẪN NẰM YÊN cho tới khi kiếm nhập vỏ "tách" — lúc đó mới
    /// đứt đôi rơi xuống bằng vật lý. Kết theo bậc: Rác chém hụt vào chiếc dép (kiếm văng, rương xịt), bậc cao
    /// nhiều nhát cùng lúc, rương bật nhảy xoay. Hook: <see cref="NhatBanHook"/>. Spec:
    /// docs/superpowers/specs/2026-11-25-kichban-nhat-ban-design.md. Âm: Tools/Audio/make_nhatban_sfx.py.
    /// </summary>
    public static class NhatBanFxPreset
    {
        public const string ChestId = "Chest_NhatBan";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/nhat_ban/";

        /// <summary>Tiếng chốt (TC) theo bậc, ElevenLabs theo chủ đề (A11). Hook tự phát qua <c>NhatBanHook.stingClips</c>; lõi để câm.</summary>
        public static readonly string[] StingFiles =
        {
            "sfx_nhatban_lock_rac.mp3", "sfx_nhatban_lock_thuong.mp3", "sfx_nhatban_lock_hiem.mp3",
            "sfx_nhatban_lock_ratHiem.mp3", "sfx_nhatban_lock_huyenThoai.mp3",
        };
        // Âm riêng của hook (phát từ lúc BẤM MỞ, không đợi nắp).
        // Âm riêng của hook: toàn bộ mp3 ElevenLabs (Tools/Audio/elevenlabs_chest_manifest.json, mục nhat_ban).
        public const string DroneFile = "sfx_nhatban_shakuhachi_loop.mp3";   // Nền (sáo)
        public const string HeartbeatFile = "sfx_nhatban_taiko.mp3";         // Nhịp (taiko)
        public const string DrawFile = "sfx_nhatban_katana_draw.mp3";
        public const string SlashFile = "sfx_nhatban_slash.mp3";
        public const string SheathFile = "sfx_nhatban_sheath.mp3";
        public const string RopeSnapFile = "sfx_nhatban_rope_cut.mp3";
        public const string CutFruitFile = "sfx_nhatban_slash.mp3";          // manifest không có tiếng riêng: dùng lại nhát chém (hook đổi cao độ)
        public const string SandalFile = "sfx_nhatban_sandal.mp3";           // đuôi Rác
        public const string BowFile = "sfx_nhatban_koto_pluck.mp3";          // đuôi nón cúi chào

        public static readonly Color Sakura = new Color(1f, 0.72f, 0.82f);

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.45f;
            p.tickPitchStart = 0.85f;
            p.tickPitchEnd = 1.4f;
            p.lockVolume = 1f;          // đoạn kết là cao trào: to
            p.whooshVolume = 0.45f;
            p.ambientVolume = 0f;       // nền do hook tự phát (sáo + taiko) ngay từ lúc bấm
            p.openAmbientClip = null;
            p.accentVolume = 0.3f;
            p.accentsBase = 0;
            p.accentsPerTier = 1;

            p.secondaryColor = Sakura;
            p.secondaryMix = 0.8f;
            p.particleLook = ParticleLook.SoftDot;
            p.particleDirection = ParticleDirection.Radial;   // không dùng Swirl (tránh lỗi orbital khác chế độ)
            p.ambientRate = 10f;        // hạt lõi tiết chế: phần chính là vật lý + kiếm
            p.burstCount = 35;
            p.particleSpeed = 0.35f;
            p.particleSize = 0.03f;
            p.particleLifetime = 2f;
            p.burstGravity = 0.1f;

            p.rouletteStyle = RouletteStyle.Cycle;
            p.minInterval = 0.05f;
            p.maxInterval = 0.45f;
            p.nearMissChance = 0.5f;
            p.minRouletteSeconds = 2.8f;

            p.rayLengthMultiplier = 0.8f;
            p.rayDuration = 1.2f;
            p.raySpin = 6f;
            p.lightFlashIntensity = 5f;

            // Kế hoạch 2026-11-25 (mục 5.15 + A1): lõi KHÔNG phát âm căng thẳng; sáo + taiko + tiếng chốt do NhatBanHook phát.
            p.tension.enabled = false;
        }
    }
}

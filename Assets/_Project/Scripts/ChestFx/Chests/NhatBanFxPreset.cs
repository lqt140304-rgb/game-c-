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

        // Tiếng quay màu: giữ mõ gỗ (hyoshigi) cũ.
        public static readonly string[] TickFiles = { "sfx_nhatban_tick.mp3" };

        /// <summary>Tiếng khoá theo bậc = đoạn KẾT của kịch bản (nhát chém cuối + "tách" + taiko/gong). Hình trong NhatBanHook khớp nhịp các file này.</summary>
        public static readonly string[] LockFiles =
        {
            "sfx_nhatban_iai_fin_rac.wav", "sfx_nhatban_iai_fin_thuong.wav", "sfx_nhatban_iai_fin_hiem.wav",
            "sfx_nhatban_iai_fin_ratHiem.wav", "sfx_nhatban_iai_fin_huyenThoai.wav",
        };
        public const string WhooshFile = "sfx_nhatban_grow.mp3";
        public const string LidOpenFile = "sfx_nhatban_lid_open.mp3";
        public const string LidThudFile = "sfx_nhatban_lid_thud.mp3";
        public static readonly string[] AccentFiles = { "sfx_nhatban_chime_a.mp3", "sfx_nhatban_koto_pluck.mp3" };

        // Âm riêng của hook (phát từ lúc BẤM MỞ, không đợi nắp).
        public const string DroneFile = "sfx_nhatban_iai_drone.wav";
        public const string HeartbeatFile = "sfx_nhatban_iai_heartbeat.wav";
        public const string DrawFile = "sfx_nhatban_iai_draw.wav";
        public const string SlashFile = "sfx_nhatban_iai_slash.wav";
        public const string SheathFile = "sfx_nhatban_iai_sheath.wav";
        public const string RopeSnapFile = "sfx_nhatban_iai_rope_snap.wav";
        public const string CutFruitFile = "sfx_nhatban_iai_cut_fruit.wav";

        public static readonly Color Sakura = new Color(1f, 0.72f, 0.82f);

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.45f;
            p.tickPitchStart = 0.85f;
            p.tickPitchEnd = 1.4f;
            p.lockVolume = 1f;          // đoạn kết là cao trào: to
            p.whooshVolume = 0.45f;
            p.ambientVolume = 0f;       // nền do hook tự phát (sáo + tim) ngay từ lúc bấm
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
        }
    }
}

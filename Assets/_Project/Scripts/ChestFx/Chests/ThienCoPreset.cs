namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương ThienCo (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: lò xo: Jump +20%, chu kỳ +1; rương xoay như cơ cấu.
    /// Khe âm: nền sfx_thienco_ambient_clockwork.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_thienco_gear_clank.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class ThienCoPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_ThienCo", displayName = "ThienCo",
            audioFolder = "Assets/_Project/Audio/Chests/thien_co/", filePrefix = "sfx_thienco_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Spin, shakeScale = 1f,
            squashMul = 1.0f, racSquashMul = 1.0f, jumpMul = 1.2f, cycleDelta = 1,

            // khe âm (rỗng = im)
            bedFile = "sfx_thienco_ambient_clockwork.mp3",
            accentFiles = new[] { "", "sfx_thienco_gear_clank.mp3", "sfx_thienco_gear_clank.mp3" },
            lockFiles = new[] { "sfx_thienco_lock_rac.mp3", "sfx_thienco_lock_thuong.mp3", "sfx_thienco_lock_hiem.mp3", "sfx_thienco_lock_ratHiem.mp3", "sfx_thienco_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương NhatBan (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: điềm tĩnh: chu kỳ -1 (lõi kẹp tối thiểu 1), Squash -10%.
    /// Khe âm: nền sfx_nhatban_shakuhachi_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_nhatban_sheath.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class NhatBanPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_NhatBan", displayName = "NhatBan",
            audioFolder = "Assets/_Project/Audio/Chests/nhat_ban/", filePrefix = "sfx_nhatban_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Shake, shakeScale = 1f,
            squashMul = 0.9f, racSquashMul = 1.0f, jumpMul = 1.0f, cycleDelta = -1,

            // khe âm (rỗng = im)
            bedFile = "sfx_nhatban_shakuhachi_loop.mp3",
            accentFiles = new[] { "", "sfx_nhatban_sheath.mp3", "sfx_nhatban_sheath.mp3" },
            lockFiles = new[] { "sfx_nhatban_lock_rac.mp3", "sfx_nhatban_lock_thuong.mp3", "sfx_nhatban_lock_hiem.mp3", "sfx_nhatban_lock_ratHiem.mp3", "sfx_nhatban_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

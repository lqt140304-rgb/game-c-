namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương Mimic (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: háu đói: Squash +15%.
    /// Khe âm: nền sfx_mimic_stomach_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_mimic_gulp.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class MimicPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_Mimic", displayName = "Mimic",
            audioFolder = "Assets/_Project/Audio/Chests/mimic/", filePrefix = "sfx_mimic_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Shake, shakeScale = 1f,
            squashMul = 1.15f, racSquashMul = 1.15f, jumpMul = 1.0f, cycleDelta = 0,

            // khe âm (rỗng = im)
            bedFile = "sfx_mimic_stomach_loop.mp3",
            accentFiles = new[] { "", "sfx_mimic_gulp.mp3", "sfx_mimic_gulp.mp3" },
            lockFiles = new[] { "sfx_mimic_lock_rac.mp3", "sfx_mimic_lock_thuong.mp3", "sfx_mimic_lock_hiem.mp3", "sfx_mimic_lock_ratHiem.mp3", "sfx_mimic_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

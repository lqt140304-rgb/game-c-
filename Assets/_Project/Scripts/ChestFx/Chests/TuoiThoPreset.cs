namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương TuoiTho (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: tưng tưng: chu kỳ +1, Jump +10%.
    /// Khe âm: nền sfx_tuoitho_roll_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_tuoitho_bonk.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class TuoiThoPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_TuoiTho", displayName = "TuoiTho",
            audioFolder = "Assets/_Project/Audio/Chests/tuoi_tho/", filePrefix = "sfx_tuoitho_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Bounce, shakeScale = 1f,
            squashMul = 1.0f, racSquashMul = 1.0f, jumpMul = 1.1f, cycleDelta = 1,

            // khe âm (rỗng = im)
            bedFile = "sfx_tuoitho_roll_loop.mp3",
            accentFiles = new[] { "", "sfx_tuoitho_bonk.mp3", "sfx_tuoitho_bonk.mp3" },
            lockFiles = new[] { "sfx_tuoitho_lock_rac.mp3", "sfx_tuoitho_lock_thuong.mp3", "sfx_tuoitho_lock_hiem.mp3", "sfx_tuoitho_lock_ratHiem.mp3", "sfx_tuoitho_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

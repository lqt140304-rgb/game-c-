namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương RacQuy (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: ẻo lả: Rác bẹp thêm +20%, nảy -20%; lắc như thùng rác.
    /// Khe âm: nền sfx_racquy_bed_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_racquy_clank.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class RacQuyPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_RacQuy", displayName = "RacQuy",
            audioFolder = "Assets/_Project/Audio/Chests/rac_quy/", filePrefix = "sfx_racquy_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Wobble, shakeScale = 1f,
            squashMul = 1.0f, racSquashMul = 1.2f, jumpMul = 0.8f, cycleDelta = 0,

            // khe âm (rỗng = im)
            bedFile = "sfx_racquy_bed_loop.mp3",
            accentFiles = new[] { "", "sfx_racquy_clank.mp3", "sfx_racquy_clank.mp3" },
            lockFiles = new[] { "sfx_racquy_lock_rac.mp3", "sfx_racquy_lock_thuong.mp3", "sfx_racquy_lock_hiem.mp3", "sfx_racquy_lock_ratHiem.mp3", "sfx_racquy_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

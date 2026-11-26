namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương AmNhac (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: nhún theo nhịp: chu kỳ +1.
    /// Khe âm: nền sfx_amnhac_drumroll.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_amnhac_coin_slot.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class AmNhacPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_AmNhac", displayName = "AmNhac",
            audioFolder = "Assets/_Project/Audio/Chests/am_nhac/", filePrefix = "sfx_amnhac_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Bounce, shakeScale = 1f,
            squashMul = 1.0f, racSquashMul = 1.0f, jumpMul = 1.0f, cycleDelta = 1,

            // khe âm (rỗng = im)
            bedFile = "sfx_amnhac_drumroll.mp3",
            accentFiles = new[] { "", "sfx_amnhac_coin_slot.mp3", "sfx_amnhac_coin_slot.mp3" },
            lockFiles = new[] { "sfx_amnhac_lock_rac.mp3", "sfx_amnhac_lock_thuong.mp3", "sfx_amnhac_lock_hiem.mp3", "sfx_amnhac_lock_ratHiem.mp3", "sfx_amnhac_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

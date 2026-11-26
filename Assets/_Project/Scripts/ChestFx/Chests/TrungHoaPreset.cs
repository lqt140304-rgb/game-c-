namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương TrungHoa (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: mừng hội: chu kỳ +1; nảy theo nhịp trống múa lân.
    /// Khe âm: nền sfx_trunghoa_lion_drum_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_trunghoa_thud.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class TrungHoaPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_TrungHoa", displayName = "TrungHoa",
            audioFolder = "Assets/_Project/Audio/Chests/trung_hoa/", filePrefix = "sfx_trunghoa_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Bounce, shakeScale = 1f,
            squashMul = 1.0f, racSquashMul = 1.0f, jumpMul = 1.0f, cycleDelta = 1,

            // khe âm (rỗng = im)
            bedFile = "sfx_trunghoa_lion_drum_loop.mp3",
            accentFiles = new[] { "", "sfx_trunghoa_thud.mp3", "sfx_trunghoa_thud.mp3" },
            lockFiles = new[] { "sfx_trunghoa_lock_rac.mp3", "sfx_trunghoa_lock_thuong.mp3", "sfx_trunghoa_lock_hiem.mp3", "sfx_trunghoa_lock_ratHiem.mp3", "sfx_trunghoa_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

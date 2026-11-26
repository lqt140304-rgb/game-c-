namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương VietNam (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: mềm: Squash -10%; bồng bềnh như trên nước.
    /// Khe âm: nền sfx_vietnam_dan_bau_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_vietnam_thud.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class VietNamPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_VietNam", displayName = "VietNam",
            audioFolder = "Assets/_Project/Audio/Chests/viet_nam/", filePrefix = "sfx_vietnam_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Float, shakeScale = 1f,
            squashMul = 0.9f, racSquashMul = 0.9f, jumpMul = 1.0f, cycleDelta = 0,

            // khe âm (rỗng = im)
            bedFile = "sfx_vietnam_dan_bau_loop.mp3",
            accentFiles = new[] { "", "sfx_vietnam_thud.mp3", "sfx_vietnam_thud.mp3" },
            lockFiles = new[] { "sfx_vietnam_lock_rac.mp3", "sfx_vietnam_lock_thuong.mp3", "sfx_vietnam_lock_hiem.mp3", "sfx_vietnam_lock_ratHiem.mp3", "sfx_vietnam_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

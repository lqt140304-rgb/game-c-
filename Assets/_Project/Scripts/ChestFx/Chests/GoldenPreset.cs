namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương Golden (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: nảy mạnh: Jump +20%.
    /// Khe âm: nền sfx_golden_spin_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_golden_token.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class GoldenPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_Golden", displayName = "Golden",
            audioFolder = "Assets/_Project/Audio/Chests/golden/", filePrefix = "sfx_golden_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Bounce, shakeScale = 1f,
            squashMul = 1.0f, racSquashMul = 1.0f, jumpMul = 1.2f, cycleDelta = 0,

            // khe âm (rỗng = im)
            bedFile = "sfx_golden_spin_loop.mp3",
            accentFiles = new[] { "", "sfx_golden_token.mp3", "sfx_golden_token.mp3" },
            lockFiles = new[] { "sfx_golden_lock_rac.mp3", "sfx_golden_lock_thuong.mp3", "sfx_golden_lock_hiem.mp3", "sfx_golden_lock_ratHiem.mp3", "sfx_golden_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

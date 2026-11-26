namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương HaiTac (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: đàn hồi như sóng: Squash +10%; rương lắc nghiêng như thuyền.
    /// Khe âm: nền sfx_haitac_bed_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_haitac_thud.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class HaiTacPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_HaiTac", displayName = "HaiTac",
            audioFolder = "Assets/_Project/Audio/Chests/hai_tac/", filePrefix = "sfx_haitac_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Wobble, shakeScale = 1f,
            squashMul = 1.1f, racSquashMul = 1.1f, jumpMul = 1.0f, cycleDelta = 0,

            // khe âm (rỗng = im)
            bedFile = "sfx_haitac_bed_loop.mp3",
            accentFiles = new[] { "", "sfx_haitac_thud.mp3", "sfx_haitac_thud.mp3" },
            lockFiles = new[] { "sfx_haitac_lock_rac.mp3", "sfx_haitac_lock_thuong.mp3", "sfx_haitac_lock_hiem.mp3", "sfx_haitac_lock_ratHiem.mp3", "sfx_haitac_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

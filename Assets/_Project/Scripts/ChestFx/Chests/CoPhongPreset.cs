namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương CoPhong (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: thanh thoát: Squash -10%, Jump +10%.
    /// Khe âm: nền sfx_cophong_sword_hum_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_cophong_bamboo_knock.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class CoPhongPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_CoPhong", displayName = "CoPhong",
            audioFolder = "Assets/_Project/Audio/Chests/co_phong/", filePrefix = "sfx_cophong_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Float, shakeScale = 1f,
            squashMul = 0.9f, racSquashMul = 0.9f, jumpMul = 1.1f, cycleDelta = 0,

            // khe âm (rỗng = im)
            bedFile = "sfx_cophong_sword_hum_loop.mp3",
            accentFiles = new[] { "", "sfx_cophong_bamboo_knock.mp3", "sfx_cophong_bamboo_knock.mp3" },
            lockFiles = new[] { "sfx_cophong_lock_rac.mp3", "sfx_cophong_lock_thuong.mp3", "sfx_cophong_lock_hiem.mp3", "sfx_cophong_lock_ratHiem.mp3", "sfx_cophong_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

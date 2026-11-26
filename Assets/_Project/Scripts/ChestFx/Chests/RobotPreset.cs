namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương Robot (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: cứng: Squash -20%, chu kỳ -1 (lõi kẹp tối thiểu 1).
    /// Khe âm: nền sfx_robot_hum_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_robot_slot.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class RobotPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_Robot", displayName = "Robot",
            audioFolder = "Assets/_Project/Audio/Chests/robot/", filePrefix = "sfx_robot_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Shake, shakeScale = 1f,
            squashMul = 0.8f, racSquashMul = 0.8f, jumpMul = 1.0f, cycleDelta = -1,

            // khe âm (rỗng = im)
            bedFile = "sfx_robot_hum_loop.mp3",
            accentFiles = new[] { "", "sfx_robot_slot.mp3", "sfx_robot_slot.mp3" },
            lockFiles = new[] { "sfx_robot_lock_rac.mp3", "sfx_robot_lock_thuong.mp3", "sfx_robot_lock_hiem.mp3", "sfx_robot_lock_ratHiem.mp3", "sfx_robot_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

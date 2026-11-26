namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương Miner (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: nặng: Jump -20%; rung như đất đá chấn động.
    /// Khe âm: nền sfx_miner_rumble_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_miner_thump.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class MinerPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_Miner", displayName = "Miner",
            audioFolder = "Assets/_Project/Audio/Chests/miner/", filePrefix = "sfx_miner_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Shake, shakeScale = 1f,
            squashMul = 1.0f, racSquashMul = 1.0f, jumpMul = 0.8f, cycleDelta = 0,

            // khe âm (rỗng = im)
            bedFile = "sfx_miner_rumble_loop.mp3",
            accentFiles = new[] { "", "sfx_miner_thump.mp3", "sfx_miner_thump.mp3" },
            lockFiles = new[] { "sfx_miner_lock_rac.mp3", "sfx_miner_lock_thuong.mp3", "sfx_miner_lock_hiem.mp3", "sfx_miner_lock_ratHiem.mp3", "sfx_miner_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

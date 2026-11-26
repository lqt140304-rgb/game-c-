namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương Magic (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: lơ lửng: Squash -10%.
    /// Khe âm: nền sfx_magic_rune_bed_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_magic_rune_stone.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class MagicPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_Magic", displayName = "Magic",
            audioFolder = "Assets/_Project/Audio/Chests/magic/", filePrefix = "sfx_magic_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Float, shakeScale = 1f,
            squashMul = 0.9f, racSquashMul = 0.9f, jumpMul = 1.0f, cycleDelta = 0,

            // khe âm (rỗng = im)
            bedFile = "sfx_magic_rune_bed_loop.mp3",
            accentFiles = new[] { "", "sfx_magic_rune_stone.mp3", "sfx_magic_rune_stone.mp3" },
            lockFiles = new[] { "sfx_magic_lock_rac.mp3", "sfx_magic_lock_thuong.mp3", "sfx_magic_lock_hiem.mp3", "sfx_magic_lock_ratHiem.mp3", "sfx_magic_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

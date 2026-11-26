namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương KhongGian (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: nhẹ bẫng: Jump +20%; lơ lửng không trọng lực.
    /// Khe âm: nền sfx_khonggian_engine_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_khonggian_fuel_dock.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class KhongGianPreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_KhongGian", displayName = "KhongGian",
            audioFolder = "Assets/_Project/Audio/Chests/khong_gian/", filePrefix = "sfx_khonggian_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Float, shakeScale = 1f,
            squashMul = 1.0f, racSquashMul = 1.0f, jumpMul = 1.2f, cycleDelta = 0,

            // khe âm (rỗng = im)
            bedFile = "sfx_khonggian_engine_loop.mp3",
            accentFiles = new[] { "", "sfx_khonggian_fuel_dock.mp3", "sfx_khonggian_fuel_dock.mp3" },
            lockFiles = new[] { "sfx_khonggian_lock_rac.mp3", "sfx_khonggian_lock_thuong.mp3", "sfx_khonggian_lock_hiem.mp3", "sfx_khonggian_lock_ratHiem.mp3", "sfx_khonggian_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

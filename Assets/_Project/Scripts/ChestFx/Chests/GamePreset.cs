namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// DỮ LIỆU RIÊNG của rương Game (chỉ dữ liệu, không logic; xem <see cref="ChestPreset"/>). Phiên làm rương này chỉ sửa file này.
    /// Tính cách bóp méo: giật như khung hình: chu kỳ +1.
    /// Khe âm: nền sfx_game_chip_loop.mp3; nhấn nấc 1 rỗng, nấc 2 và 3 dùng tiếng ngắn cũ sfx_game_button_click.mp3; 5 tiếng chốt theo tiền tố.
    /// </summary>
    public static class GamePreset
    {
        public static readonly ChestPreset Data = new ChestPreset
        {
            chestId = "Chest_Game", displayName = "Game",
            audioFolder = "Assets/_Project/Audio/Chests/game/", filePrefix = "sfx_game_",

            // chuyển động và bóp méo
            moveStyle = ChestMoveStyle.Bounce, shakeScale = 1f,
            squashMul = 1.0f, racSquashMul = 1.0f, jumpMul = 1.0f, cycleDelta = 1,

            // khe âm (rỗng = im)
            bedFile = "sfx_game_chip_loop.mp3",
            accentFiles = new[] { "", "sfx_game_button_click.mp3", "sfx_game_button_click.mp3" },
            lockFiles = new[] { "sfx_game_lock_rac.mp3", "sfx_game_lock_thuong.mp3", "sfx_game_lock_hiem.mp3", "sfx_game_lock_ratHiem.mp3", "sfx_game_lock_huyenThoai.mp3" },
            bedGain = 1f, accentGain = 1f, lockGain = 1f,
        };
    }
}

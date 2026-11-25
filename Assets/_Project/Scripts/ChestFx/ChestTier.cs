namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// The five item tiers (docs/superpowers/specs/2026-11-23-tier-do-trong-ruong-design.md).
    /// The numeric value is the rank (0 = lowest) and is used for "bigger effect for higher tier".
    /// </summary>
    public enum ChestTier
    {
        Rac = 0,        // Rác
        Thuong = 1,     // Thường
        Hiem = 2,       // Hiếm
        RatHiem = 3,    // Rất hiếm
        HuyenThoai = 4, // Huyền thoại
    }

    public static class ChestTierUtil
    {
        public const int Count = 5;

        public static int Rank(this ChestTier tier) => (int)tier;

        /// <summary>0 for Rác .. 1 for Huyền thoại.</summary>
        public static float Rank01(this ChestTier tier) => (int)tier / (float)(Count - 1);

        public static string DisplayName(this ChestTier tier)
        {
            switch (tier)
            {
                case ChestTier.Rac: return "Rác";
                case ChestTier.Thuong: return "Thường";
                case ChestTier.Hiem: return "Hiếm";
                case ChestTier.RatHiem: return "Rất hiếm";
                default: return "Huyền thoại";
            }
        }
    }
}

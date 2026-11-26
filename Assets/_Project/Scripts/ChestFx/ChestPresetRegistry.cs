namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// BẢNG ĐĂNG KÝ 17 rương: đã có sẵn đủ 17 dòng, KHÔNG ai cần sửa file này khi chỉnh dữ liệu một rương
    /// (chỉ sửa file <c>Chests/&lt;Tên&gt;Preset.cs</c> của rương đó). Chỉ sửa khi thêm hoặc bớt rương.
    /// </summary>
    public static class ChestPresetRegistry
    {
        public static readonly ChestPreset[] All =
        {
            GoldenPreset.Data,
            HaiTacPreset.Data,
            RacQuyPreset.Data,
            MagicPreset.Data,
            MimicPreset.Data,
            MinerPreset.Data,
            AmNhacPreset.Data,
            VietNamPreset.Data,
            TuoiThoPreset.Data,
            RobotPreset.Data,
            KhongGianPreset.Data,
            GamePreset.Data,
            TheThaoPreset.Data,
            TrungHoaPreset.Data,
            NhatBanPreset.Data,
            CoPhongPreset.Data,
            ThienCoPreset.Data,
        };

        /// <summary>Tìm theo displayName (ví dụ "Golden"); null nếu không có.</summary>
        public static ChestPreset Get(string displayName)
        {
            for (int i = 0; i < All.Length; i++)
                if (All[i] != null && All[i].displayName == displayName) return All[i];
            return null;
        }
    }
}

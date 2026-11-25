using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Hành động mở của vật mở khoá (mỗi hành động 0,35 s, xong thì vật thu nhỏ biến mất). Xem kế hoạch 2026-11-26, mục 2.</summary>
    public enum KeyAction { Insert, Drop, Strike, Pounce, Peck }

    /// <summary>
    /// Thông số riêng của MỘT rương cho chuỗi mở chuẩn (kế hoạch 2026-11-26, mục 4.3). Mỗi lớp `*KeyHook` có
    /// `public static readonly KeyPreset Preset`. Là struct thuần dữ liệu để Editor và check đọc được.
    /// </summary>
    [System.Serializable]
    public struct KeyPreset
    {
        /// <summary>Tên rương trong cảnh/hồ sơ, ví dụ "Chest_Golden". Hồ sơ lưu là ChestFx_&lt;displayName&gt;.asset.</summary>
        public string chestId;
        /// <summary>Tên ngắn không dấu dùng cho tên tệp và menu, ví dụ "Golden", "HaiTac", "RacQuy".</summary>
        public string displayName;
        /// <summary>Thư mục âm, kết thúc bằng "/", ví dụ "Assets/_Project/Audio/Chests/golden/".</summary>
        public string audioFolder;
        /// <summary>Tiền tố tệp âm, ví dụ "sfx_golden_". Tiếng chốt = prefix + "lock_{rac,thuong,hiem,ratHiem,huyenThoai}.mp3".</summary>
        public string filePrefix;
        /// <summary>Tên tệp nền vòng lặp trong audioFolder, ví dụ "sfx_golden_spin_loop.mp3".</summary>
        public string bedFile;
        /// <summary>Tên tệp tiếng cắm chìa (ngắn) trong audioFolder; rỗng = không có tiếng cắm.</summary>
        public string keyFile;
        /// <summary>Màu chính của vật mở khoá.</summary>
        public Color mainColor;
        public KeyAction openAction;
        /// <summary>Nhân biên độ Squash của 4 bậc Thường→Huyền thoại (1 = mặc định; phạm vi 0,8–1,2).</summary>
        public float squashMul;
        /// <summary>Nhân biên độ Squash riêng bậc Rác (1 = mặc định).</summary>
        public float racSquashMul;
        /// <summary>Nhân độ cao Jump (1 = mặc định). Sau khi nhân vẫn kẹp ≤ 0,6 H.</summary>
        public float jumpMul;
        /// <summary>Cộng vào số chu kỳ Squash và số lần nảy Jump (±1), kết quả tối thiểu 1.</summary>
        public int cycleDelta;
        /// <summary>Nhân biên độ rung (thường 1).</summary>
        public float shakeScale;

        /// <summary>Giá trị mặc định cho mọi hệ số (không đổi tính cách).</summary>
        public static KeyPreset Neutral(string chestId, string displayName, string audioFolder, string filePrefix, string bedFile, Color mainColor, KeyAction action)
        {
            return new KeyPreset
            {
                chestId = chestId, displayName = displayName, audioFolder = audioFolder, filePrefix = filePrefix, bedFile = bedFile,
                mainColor = mainColor, openAction = action,
                squashMul = 1f, racSquashMul = 1f, jumpMul = 1f, cycleDelta = 0, shakeScale = 1f,
            };
        }
    }

    /// <summary>Tên 5 đuôi tệp tiếng chốt theo hạng bậc 0..4 (Rác..Huyền thoại).</summary>
    public static class KeyAudioNames
    {
        public static readonly string[] LockSuffix = { "lock_rac.mp3", "lock_thuong.mp3", "lock_hiem.mp3", "lock_ratHiem.mp3", "lock_huyenThoai.mp3" };
        public const string KeyInsertSuffix = "key_insert.mp3";
    }
}

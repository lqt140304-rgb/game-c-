using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Kiểu chuyển động CẢ RƯƠNG của từng chủ đề (rương KHÔNG bị giữ cố định tại chỗ: có thể bay lên, lơ lửng, xoay, lắc mọi hướng).
    /// Shake = rung mọi hướng (x, y, z và nghiêng); Float = bay lên lơ lửng rồi hạ; Spin = xoay quanh trục đứng; Wobble = lắc nghiêng
    /// nhiều hướng; Bounce = nảy và xoay nhẹ. <see cref="ChestOpenScript"/> dựng bước tương ứng; mọi chuyển động tự trả rương về chỗ cũ.
    /// </summary>
    public enum ChestMoveStyle { Shake, Float, Spin, Wobble, Bounce }

    /// <summary>
    /// DỮ LIỆU RIÊNG CỦA MỘT RƯƠNG cho chuỗi mở chuẩn (không có logic). Mỗi rương có đúng MỘT file dữ liệu
    /// <c>Chests/&lt;Tên&gt;Preset.cs</c> (lớp tĩnh có <c>public static readonly ChestPreset Data</c>), và bảng đăng ký
    /// <see cref="ChestPresetRegistry"/> đã liệt kê đủ 17 dòng, nên phiên làm từng rương chỉ sửa file của mình và KHÔNG ai phải
    /// sửa file chung (không xung đột khi gộp). Xem docs/superpowers/specs/2026-11-27-ruong-am-thanh-dong-bo-design.md.
    ///
    /// "Khe âm" là tên tệp (không có thư mục) nằm trong <see cref="audioFolder"/>. Khe rỗng ("" hoặc null) = im, không báo lỗi;
    /// tệp ghi tên nhưng không tìm thấy = Editor cảnh báo vàng và để ô trống (rương vẫn chạy, chỉ thiếu tiếng đó).
    /// </summary>
    [System.Serializable]
    public class ChestPreset
    {
        /// <summary>Tên rương trong cảnh/hồ sơ, ví dụ "Chest_Golden".</summary>
        public string chestId;
        /// <summary>Tên ngắn không dấu, khoá của bảng đăng ký và tên tệp hồ sơ ChestFx_&lt;displayName&gt;.asset, ví dụ "Golden".</summary>
        public string displayName;
        /// <summary>Thư mục âm, kết thúc bằng "/", ví dụ "Assets/_Project/Audio/Chests/golden/".</summary>
        public string audioFolder;
        /// <summary>Tiền tố tệp âm, ví dụ "sfx_golden_".</summary>
        public string filePrefix;

        // ---- chuyển động và bóp méo (ý nghĩa như bảng trong ChestOpenScript)
        /// <summary>Kiểu chuyển động cả rương cho 3 nấc.</summary>
        public ChestMoveStyle moveStyle = ChestMoveStyle.Shake;
        /// <summary>Nhân biên độ chuyển động 3 nấc (thường 1; lõi kẹp ≤ 1,5 để lệch ngang ≤ ~0,1 S).</summary>
        public float shakeScale = 1f;
        /// <summary>Nhân biên độ Squash của 4 bậc Thường → Huyền thoại (1 = mặc định; phạm vi 0,8 – 1,2; lõi kẹp Squash ≤ 0,36).</summary>
        public float squashMul = 1f;
        /// <summary>Nhân biên độ Squash riêng bậc Rác (1 = mặc định).</summary>
        public float racSquashMul = 1f;
        /// <summary>Nhân độ cao Jump (1 = mặc định). Sau khi nhân vẫn kẹp ≤ 0,6 H.</summary>
        public float jumpMul = 1f;
        /// <summary>Cộng vào số chu kỳ Squash và số lần nảy Jump (±1), kết quả tối thiểu 1.</summary>
        public int cycleDelta = 0;

        // ---- khe âm (xem doc, mục 4)
        /// <summary>NỀN lặp, chạy từ lúc bấm mở tới lúc chốt; dâng cao độ và âm lượng theo từng nấc.</summary>
        public string bedFile = "";
        /// <summary>NHẤN nấc 1, 2, 3 (đúng 3 phần tử; có thể rỗng). Phát 1 lần lúc nấc bắt đầu, không bao giờ dài hơn nấc.</summary>
        public string[] accentFiles = { "", "", "" };
        /// <summary>TIẾNG CHỐT theo bậc: Rác, Thường, Hiếm, Rất hiếm, Huyền thoại (đúng 5 phần tử).</summary>
        public string[] lockFiles = { "", "", "", "", "" };

        // ---- độ to riêng (nhân vào bảng chung; 1 = giữ nguyên, phạm vi hợp lý 0,7 – 1,3)
        public float bedGain = 1f;
        public float accentGain = 1f;
        public float lockGain = 1f;
    }
}

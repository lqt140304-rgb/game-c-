using System.Collections.Generic;

namespace ChoDoLa.Core
{
    // View chỉ chứa thứ UI được phép thấy. KHÔNG có IsFake, hiện vật bên trong hộp, manh mối nào đúng,
    // hay giá trị kỳ vọng (O-04: người chơi tự tính; hàng giả không được lộ qua giá hiển thị).

    public sealed class BoxOfferView
    {
        public string OfferId;
        public string TypeId;
        public string DisplayName;
        public int TodayPrice;            // giá hôm nay của loại hộp (công bố đầu ngày)
        public int TagPrice;              // giá niêm yết = giá phải trả
        public double FakeRate;           // tỷ lệ hàng giả của loại hộp, hiển thị khi xem hộp
        public string FreeClueText;       // manh mối miễn phí đầu tiên (có thể sai)
        public string FreeClueOrigin;
        public bool CertainOriginBought;
        public string CertainOrigin;      // chỉ có giá trị sau khi mua lời mách
    }

    public sealed class ItemView
    {
        public string InstanceId;
        public string ItemId;
        public string DisplayName;
        public string OriginId;
        public string QualityId;
        public string[] InspectSigns;
        public int ListedPrice;           // tính như hàng thật, không làm lộ hàng giả
    }

    public sealed class RunSnapshot
    {
        public int Day;
        public int Cash;
        public int Arrears;
        public int DebtDueToday;          // kỳ của ngày + nợ tồn
        public int BoxesBoughtToday;
        public int MaxDailyBoxes;
        public int ShelfTurn;             // 1..3, 0 nếu hết lượt hoặc đã dừng mua
        public RunStatus Status;
        public string LoseReason;
        public Dictionary<string, int> TodayPrice;
    }
}

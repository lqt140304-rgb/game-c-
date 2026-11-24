using System;
using System.Collections.Generic;

namespace ChoDoLa.Core
{
    public enum RunStatus { Playing, Won, Lost }

    /// <summary>Hộp đã bày trên kệ hoặc đã mua. Kết quả bên trong định sẵn theo seed khi bày hàng.</summary>
    [Serializable]
    public sealed class BoxInstance
    {
        public string BoxId;          // "B{ngày}-{lượt}-{loại}", duy nhất trong ván
        public string TypeId;         // H1, H2, H3, RD
        public int TagPrice;          // giá niêm yết = giá hôm nay x hệ số manh mối
        public string ItemId;         // hiện vật bên trong (ẩn với UI)
        public string QualityId;      // chất lượng (UI chỉ thấy sau khi mở)
        public bool IsFake;           // ẩn với UI
        public string[] ClueOrigins;  // 3 manh mối đã xáo, mỗi cái là một nhóm nguồn gốc
        public string[] ClueTexts;    // lời văn tương ứng
        public bool CertainOriginBought;
    }

    [Serializable]
    public sealed class ItemInstance
    {
        public string InstanceId;     // "I1", "I2"... theo thứ tự mở trong ván
        public string ItemId;
        public string QualityId;
        public bool IsFake;           // chỉ lộ sau giám định hoặc khi bán (giám định ngoài phạm vi vòng 1 ngày)
    }

    /// <summary>
    /// Toàn bộ trạng thái ván là dữ liệu thuần, serialize được (JSON). Lưu ván chỉ cần chụp đối tượng này.
    /// Mọi thao tác tiền có mã giao dịch nằm trong <see cref="AppliedTx"/> và lưu cùng bản lưu.
    /// </summary>
    [Serializable]
    public sealed class RunState
    {
        public long Seed;
        public int Day = 1;
        public int Cash;
        public int Arrears;                    // nợ tồn: MỘT con số duy nhất, không tách gốc và phạt
        public RunStatus Status = RunStatus.Playing;
        public string LoseReason = "";

        public int BoxesBoughtToday;           // 0..MaxBoxesPerDay
        public bool StoppedBuyingToday;
        public Dictionary<string, int> TodayPrice = new Dictionary<string, int>();   // giá hôm nay theo loại hộp
        public List<BoxInstance> Shelf = new List<BoxInstance>();                    // hộp của lượt mua hiện tại
        public List<BoxInstance> Table = new List<BoxInstance>();                    // bàn mở: hộp đã mua, chưa mở
        public List<ItemInstance> Inventory = new List<ItemInstance>();              // kho 8 ô
        public int NextItemNumber = 1;

        public List<string> AppliedTx = new List<string>();
        public List<int> PaidDays = new List<int>();
    }

    // ----- Kết quả lệnh và sự kiện -----

    public static class Reject
    {
        public const string DailyBoxLimit = "DAILY_BOX_LIMIT";
        public const string TableFull = "TABLE_FULL";
        public const string NotEnoughCash = "NOT_ENOUGH_CASH";
        public const string InventoryFull = "INVENTORY_FULL";
        public const string BoxAlreadyOpened = "BOX_ALREADY_OPENED";
        public const string DuplicateTransaction = "DUPLICATE_TRANSACTION";
        public const string RunEnded = "RUN_ENDED";
        public const string OfferNotFound = "OFFER_NOT_FOUND";
        public const string ItemNotFound = "ITEM_NOT_FOUND";
        public const string ClueAlreadyBought = "CLUE_ALREADY_BOUGHT";
        public const string StoppedBuying = "STOPPED_BUYING";
        public const string StaleDay = "STALE_DAY";
    }

    public readonly struct CommandResult
    {
        public readonly bool Ok;
        public readonly string ReasonCode;   // null khi Ok
        private CommandResult(bool ok, string reason) { Ok = ok; ReasonCode = reason; }
        public static CommandResult Success() => new CommandResult(true, null);
        public static CommandResult Fail(string reason) => new CommandResult(false, reason);
        public override string ToString() => Ok ? "OK" : ReasonCode;
    }

    public enum GameEventKind
    {
        DayStarted, BoxBought, ClueBought, BuyingStopped, BoxOpened, ItemSold, DebtSettled, RunEnded, ActionRejected
    }

    /// <summary>Sự kiện để cầu nối Unity đọc và cập nhật cảnh/HUD. Lõi không biết gì về UI.</summary>
    public sealed class GameEvent
    {
        public GameEventKind Kind;
        public string Id;       // boxId / instanceId / reasonCode tùy loại
        public int Amount;      // số xu liên quan (giá mua, giá bán, tiền thu nợ)
        public string Detail;   // thông tin thêm (nhóm nguồn gốc thật, kết quả "next_day"/"won"/"lost"...)
    }
}

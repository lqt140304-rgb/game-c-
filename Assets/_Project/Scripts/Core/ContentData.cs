using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChoDoLa.Core
{
    // POCO phản chiếu Docs/Content/data/content.json. Chỉ khai báo phần vòng chơi 1 ngày cần,
    // phần còn lại (tạo tác, combo, dịch vụ...) bị bỏ qua cho đến khi nối Sprint 2 và 4.
    // Tên trường JSON dùng snake_case giống file nguồn. Không gán cứng số trong mã: mọi số lấy từ đây.

    public sealed class QualityDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("p")] public double P;
        [JsonProperty("price_mult")] public double PriceMult;
    }

    public sealed class RulesDef
    {
        [JsonProperty("start_cash")] public int StartCash;
        [JsonProperty("days")] public int Days;
        [JsonProperty("debt")] public int[] Debt;
        [JsonProperty("max_boxes_per_day")] public int MaxBoxesPerDay;
        [JsonProperty("penalty_rate")] public double PenaltyRate;
        [JsonProperty("lose_arrears_over")] public int LoseArrearsOver;
        [JsonProperty("inventory_slots")] public int InventorySlots;
        [JsonProperty("table_slots")] public int TableSlots;
        [JsonProperty("red_chest_from_day")] public int RedChestFromDay;
        [JsonProperty("fake_price_factor")] public double FakePriceFactor;
        [JsonProperty("quality")] public QualityDef[] Quality;
    }

    public sealed class OriginDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
    }

    public sealed class ItemDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("origin")] public string Origin;
        [JsonProperty("rarity")] public string Rarity;
        [JsonProperty("base_price")] public int BasePrice;
        [JsonProperty("inspect_signs")] public string[] InspectSigns;
    }

    public sealed class DropDef
    {
        [JsonProperty("item")] public string Item;
        [JsonProperty("weight")] public double Weight;
    }

    public sealed class BoxDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("price")] public int Price;
        [JsonProperty("fake_rate")] public double FakeRate;
        [JsonProperty("drops")] public DropDef[] Drops;
        [JsonProperty("clue_true_count")] public int ClueTrueCount;
    }

    public sealed class ShelfDef
    {
        [JsonProperty("turns_per_day")] public int TurnsPerDay;
        [JsonProperty("extra_clue_price")] public int ExtraCluePrice;
        [JsonProperty("day_price")] public DayPriceDef DayPrice;
        [JsonProperty("price_follows_clue")] public PriceFollowsClueDef PriceFollowsClue;
    }

    public sealed class DayPriceDef
    {
        [JsonProperty("swing_amplitude")] public double SwingAmplitude;
    }

    public sealed class PriceFollowsClueDef
    {
        [JsonProperty("origins_order")] public string[] OriginsOrder;
        [JsonProperty("multiplier")] public Dictionary<string, double[]> Multiplier;
    }

    public sealed class ClueRulesDef
    {
        [JsonProperty("per_box")] public int PerBox;
    }

    public sealed class ContentData
    {
        [JsonProperty("rules")] public RulesDef Rules;
        [JsonProperty("origins")] public OriginDef[] Origins;
        [JsonProperty("items")] public ItemDef[] Items;
        [JsonProperty("item_fake_rate_floor")] public Dictionary<string, double> ItemFakeRateFloor;
        [JsonProperty("boxes")] public BoxDef[] Boxes;
        [JsonProperty("clue_vocabulary")] public Dictionary<string, string[]> ClueVocabulary;
        [JsonProperty("clues")] public ClueRulesDef Clues;
        [JsonProperty("shelf")] public ShelfDef Shelf;

        public ItemDef Item(string id) => Array.Find(Items, i => i.Id == id)
            ?? throw new KeyNotFoundException("item " + id);
        public BoxDef Box(string id) => Array.Find(Boxes, b => b.Id == id)
            ?? throw new KeyNotFoundException("box " + id);
        public QualityDef Quality(string id) => Array.Find(Rules.Quality, q => q.Id == id)
            ?? throw new KeyNotFoundException("quality " + id);

        public static ContentData FromJson(string json) =>
            JsonConvert.DeserializeObject<ContentData>(json);
    }
}

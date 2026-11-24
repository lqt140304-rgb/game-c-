using System;

namespace ChoDoLa.Core
{
    /// <summary>
    /// Công thức giá (NoiDungMau.md mục 4 và 5). Dùng decimal để tránh lỗi nhị phân kiểu
    /// 230 x 0,7 = 160,99999999999997 làm tròn xuống thành 160 thay vì 161.
    /// </summary>
    public static class Pricing
    {
        /// <summary>G = giá nền x hệ số chất lượng x (0,2 nếu giả) x (1 + hiệu ứng), làm tròn XUỐNG đến xu.</summary>
        public static int SellPrice(ContentData c, string itemId, string qualityId, bool isFake, double bonus = 0.0)
        {
            decimal g = (decimal)c.Item(itemId).BasePrice
                      * (decimal)c.Quality(qualityId).PriceMult
                      * (isFake ? (decimal)c.Rules.FakePriceFactor : 1m)
                      * (1m + (decimal)bonus);
            return (int)Math.Floor(g);
        }

        /// <summary>Giá hiển thị: tính như hàng thật để không làm lộ hàng giả.</summary>
        public static int ListedPrice(ContentData c, string itemId, string qualityId, double bonus = 0.0)
            => SellPrice(c, itemId, qualityId, false, bonus);

        /// <summary>Giá hôm nay = giá gốc x (1 + u), u đều trong [-biên độ, +biên độ], làm tròn đến xu gần nhất.</summary>
        public static int TodayPrice(ContentData c, BoxDef box, double uniform01)
        {
            decimal u = ((decimal)uniform01 * 2m - 1m) * (decimal)c.Shelf.DayPrice.SwingAmplitude;
            return (int)Math.Round((decimal)box.Price * (1m + u), MidpointRounding.AwayFromZero);
        }

        /// <summary>Giá niêm yết = giá hôm nay x hệ số theo manh mối miễn phí đầu tiên, làm tròn đến xu gần nhất.</summary>
        public static int TagPrice(ContentData c, string boxTypeId, int todayPrice, string firstClueOrigin)
        {
            var f = c.Shelf.PriceFollowsClue;
            int idx = Array.IndexOf(f.OriginsOrder, firstClueOrigin);
            double mult = f.Multiplier[boxTypeId][idx];
            return (int)Math.Round((decimal)todayPrice * (decimal)mult, MidpointRounding.AwayFromZero);
        }

        /// <summary>Phạt: nợ tồn mới = nợ còn lại + ceil(nợ còn lại x tỷ lệ phạt). Phạt tính trên TOÀN BỘ nợ còn lại.</summary>
        public static int ApplyPenalty(int remaining, double rate)
            => remaining + (int)Math.Ceiling((decimal)remaining * (decimal)rate);
    }
}

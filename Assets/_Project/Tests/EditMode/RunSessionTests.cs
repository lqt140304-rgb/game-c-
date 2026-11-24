using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ChoDoLa.Core.Tests
{
    /// <summary>
    /// Đặc tả hành vi vòng chơi 1 ngày. Các ca nợ lấy từ GDD mục 2 (bảng ca kiểm thử), giá từ NoiDungMau.md mục 4 và 5.
    /// Đọc số liệu từ Docs/Content/data/content.json (thư mục làm việc của Unity Editor là gốc dự án).
    /// </summary>
    public class RunSessionTests
    {
        private static ContentData _content;
        private int _tx;

        [OneTimeSetUp]
        public void LoadContent()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Docs", "Content", "data", "content.json");
            _content = ContentData.FromJson(File.ReadAllText(path));
        }

        private string Tx() => "tx" + (++_tx);
        private static string Dump(RunSession s) => JsonConvert.SerializeObject(s.State);
        private RunSession New(long seed = 1) => new RunSession(_content, seed);

        // ----- Xác định theo seed -----

        [Test]
        public void SameSeedGivesSameShelf_DifferentSeedDiffers()
        {
            var a = New(7).Offers().Select(o => (o.OfferId, o.TagPrice, o.FreeClueText)).ToList();
            var b = New(7).Offers().Select(o => (o.OfferId, o.TagPrice, o.FreeClueText)).ToList();
            var c = New(8).Offers().Select(o => (o.OfferId, o.TagPrice, o.FreeClueText)).ToList();
            CollectionAssert.AreEqual(a, b);
            CollectionAssert.AreNotEqual(a, c);
        }

        [Test]
        public void ReloadFromSavedState_KeepsShelfAndPrices()
        {
            var s = New(3);
            Assert.IsTrue(s.BuyBox(s.Offers()[0].OfferId, Tx()).Ok);
            string json = JsonConvert.SerializeObject(s.State);
            var restored = new RunSession(_content, JsonConvert.DeserializeObject<RunState>(json));
            Assert.AreEqual(Dump(s), Dump(restored));
            CollectionAssert.AreEqual(
                s.Offers().Select(o => o.OfferId + o.TagPrice),
                restored.Offers().Select(o => o.OfferId + o.TagPrice));
        }

        // ----- Giá hôm nay, kệ -----

        [Test]
        public void TodayPriceStaysWithinPlusMinus20Percent()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var s = New(seed);
                foreach (var kv in s.Snapshot().TodayPrice)
                {
                    int basePrice = _content.Box(kv.Key).Price;
                    Assert.GreaterOrEqual(kv.Value, (int)System.Math.Round(basePrice * 0.8) - 1);
                    Assert.LessOrEqual(kv.Value, (int)System.Math.Round(basePrice * 1.2) + 1);
                }
            }
        }

        [Test]
        public void RedChestAppearsOnlyFromDay4()
        {
            var s = New();
            for (int day = 1; day <= 4; day++)
            {
                Assert.AreEqual(day == 4, s.Offers().Any(o => o.TypeId == "RD"), "ngày " + day);
                s.State.Cash = 100000;
                if (day < 4) Assert.IsTrue(s.EndDay(Tx(), day).Ok);
            }
        }

        [Test]
        public void ListedPriceDoesNotRevealFake()
        {
            Assert.AreEqual(895, Pricing.ListedPrice(_content, "V09", "nguyen"));   // như hàng thật
            Assert.Less(Pricing.SellPrice(_content, "V09", "nguyen", true),
                        Pricing.ListedPrice(_content, "V09", "nguyen"));
        }

        // ----- Mua hộp, bàn mở, giới hạn -----

        [Test]
        public void BuyBox_PaysTagPrice_AndAdvancesTurn()
        {
            var s = New();
            var offer = s.Offers()[0];
            int cash = s.State.Cash;
            Assert.IsTrue(s.BuyBox(offer.OfferId, Tx()).Ok);
            Assert.AreEqual(cash - offer.TagPrice, s.State.Cash);
            Assert.AreEqual(1, s.TableBoxes().Count);
            Assert.AreEqual(2, s.Snapshot().ShelfTurn);
            StringAssert.StartsWith("B1-2-", s.Offers()[0].OfferId);
        }

        [Test]
        public void FourthBoxInADay_IsRejectedWithDailyLimit()
        {
            var s = New();
            s.State.Cash = 100000;
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(s.BuyBox(s.Offers().First(o => o.TypeId == "H1").OfferId, Tx()).Ok);
                s.OpenBox(s.TableBoxes()[0].OfferId);
            }
            Assert.AreEqual(0, s.Offers().Count);
            var r = s.BuyBox("B1-4-H1", Tx());
            Assert.AreEqual(Reject.DailyBoxLimit, r.ReasonCode);
        }

        [Test]
        public void TableFull_RejectsNextDay_WhenThreeBoxesUnopened()
        {
            var s = New();
            s.State.Cash = 100000;
            for (int i = 0; i < 3; i++)
                Assert.IsTrue(s.BuyBox(s.Offers().First(o => o.TypeId == "H1").OfferId, Tx()).Ok);
            Assert.IsTrue(s.EndDay(Tx(), 1).Ok);
            s.State.Cash = 100000;
            var r = s.BuyBox(s.Offers().First(o => o.TypeId == "H1").OfferId, Tx());
            Assert.AreEqual(Reject.TableFull, r.ReasonCode);
        }

        [Test]
        public void NotEnoughCash_RejectsAndKeepsState()
        {
            var s = New();
            s.State.Cash = 1;
            string before = Dump(s);
            var r = s.BuyBox(s.Offers()[0].OfferId, Tx());
            Assert.AreEqual(Reject.NotEnoughCash, r.ReasonCode);
            Assert.AreEqual(before, Dump(s));
        }

        [Test]
        public void StopBuying_ClearsShelf_AndBlocksBuying()
        {
            var s = New();
            var offerId = s.Offers()[0].OfferId;
            Assert.IsTrue(s.StopBuying().Ok);
            Assert.AreEqual(0, s.Offers().Count);
            Assert.AreEqual(Reject.StoppedBuying, s.BuyBox(offerId, Tx()).ReasonCode);
        }

        // ----- Lời mách -----

        [Test]
        public void BuyClue_CostsFee_RevealsTrueOrigin_OnceOnly_PriceUnchanged()
        {
            var s = New(5);
            var offer = s.Offers()[1];
            int tag = offer.TagPrice, cash = s.State.Cash;
            Assert.IsTrue(s.BuyClue(offer.OfferId, Tx()).Ok);
            var after = s.Offers()[1];
            Assert.AreEqual(cash - _content.Shelf.ExtraCluePrice, s.State.Cash);
            Assert.AreEqual(tag, after.TagPrice);
            Assert.IsTrue(after.CertainOriginBought);
            var box = s.State.Shelf.Find(b => b.BoxId == offer.OfferId);
            Assert.AreEqual(_content.Item(box.ItemId).Origin, after.CertainOrigin);
            Assert.AreEqual(Reject.ClueAlreadyBought, s.BuyClue(offer.OfferId, Tx()).ReasonCode);
        }

        [Test]
        public void ClueCounts_MatchBoxType()
        {
            for (int seed = 0; seed < 30; seed++)
            {
                var s = New(seed);
                foreach (var b in s.State.Shelf)
                {
                    string origin = _content.Item(b.ItemId).Origin;
                    int trues = b.ClueOrigins.Count(o => o == origin);
                    Assert.AreEqual(_content.Box(b.TypeId).ClueTrueCount, trues, b.BoxId);
                    Assert.AreEqual(3, b.ClueOrigins.Length);
                }
            }
        }

        // ----- Mở hộp, kho, bán -----

        [Test]
        public void OpenBox_InventoryFull_KeepsBoxOnTable()
        {
            var s = New();
            s.State.Cash = 100000;
            Assert.IsTrue(s.BuyBox(s.Offers()[0].OfferId, Tx()).Ok);
            for (int i = 0; i < _content.Rules.InventorySlots; i++)
                s.State.Inventory.Add(new ItemInstance { InstanceId = "X" + i, ItemId = "V01", QualityId = "nguyen" });
            var r = s.OpenBox(s.TableBoxes()[0].OfferId);
            Assert.AreEqual(Reject.InventoryFull, r.ReasonCode);
            Assert.AreEqual(1, s.TableBoxes().Count);
        }

        [Test]
        public void OpenBox_MovesItemToInventory_ExactlyOnce()
        {
            var s = New();
            s.State.Cash = 100000;
            s.BuyBox(s.Offers()[0].OfferId, Tx());
            string id = s.TableBoxes()[0].OfferId;
            Assert.IsTrue(s.OpenBox(id).Ok);
            Assert.AreEqual(1, s.Inventory().Count);
            Assert.AreEqual(Reject.BoxAlreadyOpened, s.OpenBox(id).ReasonCode);
            Assert.AreEqual(1, s.Inventory().Count);
        }

        [Test]
        public void SellPrice_RoundsDown_V02Scratched_Is269()
        {
            Assert.AreEqual(269, Pricing.SellPrice(_content, "V02", "xuoc", false));
            Assert.AreEqual(161, Pricing.SellPrice(_content, "V01", "xuoc", false));   // 230 x 0,7 = 161, tránh lỗi double 160,99...
        }

        [Test]
        public void Sell_PaysRealPrice_FakeGetsTwentyPercent_AndReplayedTxIsRejected()
        {
            var s = New();
            s.State.Inventory.Add(new ItemInstance { InstanceId = "I1", ItemId = "V09", QualityId = "nguyen", IsFake = true });
            string tx = Tx();
            Assert.IsTrue(s.SellItem("I1", tx).Ok);
            Assert.AreEqual(_content.Rules.StartCash + 179, s.State.Cash);   // 895 x 0,2 = 179
            s.State.Inventory.Add(new ItemInstance { InstanceId = "I2", ItemId = "V09", QualityId = "nguyen" });
            string before = Dump(s);
            Assert.AreEqual(Reject.DuplicateTransaction, s.SellItem("I2", tx).ReasonCode);
            Assert.AreEqual(before, Dump(s));
        }

        // ----- Thu nợ (GDD mục 2) -----

        private RunSession At(int day, int cash, int arrears)
        {
            var s = New();
            s.State.Day = day; s.State.Cash = cash; s.State.Arrears = arrears;
            return s;
        }

        [Test] public void Debt_Case1_PaysInFull_NoArrears()
        {
            var s = At(1, 1000, 0);
            Assert.IsTrue(s.EndDay(Tx(), 1).Ok);
            Assert.AreEqual(800, s.State.Cash);
            Assert.AreEqual(0, s.State.Arrears);
            Assert.AreEqual(RunStatus.Playing, s.State.Status);
            Assert.AreEqual(2, s.State.Day);
        }

        [Test] public void Debt_Case2_Short50_PenaltyTen_ArrearsSixty()
        {
            var s = At(1, 150, 0);
            s.EndDay(Tx(), 1);
            Assert.AreEqual(60, s.State.Arrears);
            Assert.AreEqual(0, s.State.Cash);
        }

        [Test] public void Debt_Case3_PayPartial_PenaltyOnWholeRemaining()
        {
            var s = At(2, 100, 60);
            s.EndDay(Tx(), 2);
            Assert.AreEqual(372, s.State.Arrears);   // (410 - 100) = 310, phạt 62
        }

        [Test] public void Debt_Case4_ArrearsOver1000_LosesImmediately()
        {
            var s = At(3, 0, 800);
            s.EndDay(Tx(), 3);
            Assert.AreEqual(RunStatus.Lost, s.State.Status);
            Assert.AreEqual("arrears_over_limit", s.State.LoseReason);
        }

        [Test] public void Debt_Case5_Day7_LeftoverArrears_Loses()
        {
            var s = At(7, 2200, 50);
            s.EndDay(Tx(), 7);
            Assert.AreEqual(RunStatus.Lost, s.State.Status);
            Assert.AreEqual("arrears_on_day7", s.State.LoseReason);
        }

        [Test] public void Debt_Case6_Day7_PaidInFull_Wins()
        {
            var s = At(7, 2200, 0);
            s.EndDay(Tx(), 7);
            Assert.AreEqual(RunStatus.Won, s.State.Status);
        }

        [Test] public void Debt_Case7_Day1_ZeroCash_ArrearsTwoForty()
        {
            var s = At(1, 0, 0);
            s.EndDay(Tx(), 1);
            Assert.AreEqual(240, s.State.Arrears);   // 200 + phạt 40
        }

        [Test] public void Debt_Case8_Pays5Only_ArrearsFourEightySix()
        {
            var s = At(2, 5, 60);
            s.EndDay(Tx(), 2);
            Assert.AreEqual(486, s.State.Arrears);   // 405 + 81, không phải 485
        }

        // ----- Chống thu nợ hai lần (D-01) -----

        [Test]
        public void EndDay_DoubleClick_CollectsOnlyOnce()
        {
            var s = At(1, 1000, 0);
            Assert.IsTrue(s.EndDay(Tx(), 1).Ok);
            int cash = s.State.Cash;
            // Bấm đúp lần hai: mã giao dịch khác (sinh mới) nhưng UI vẫn nghĩ đang ở ngày 1.
            var r = s.EndDay(Tx(), 1);
            Assert.AreEqual(Reject.StaleDay, r.ReasonCode);
            Assert.AreEqual(cash, s.State.Cash);
            Assert.AreEqual(2, s.State.Day);
        }

        [Test]
        public void EndDay_SameTxReplayed_IsDuplicate()
        {
            var s = At(1, 1000, 0);
            string tx = Tx();
            Assert.IsTrue(s.EndDay(tx, 1).Ok);
            Assert.AreEqual(Reject.DuplicateTransaction, s.EndDay(tx, 2).ReasonCode);
            Assert.AreEqual(2, s.State.Day);
        }

        [Test]
        public void AfterRunEnds_CommandsAreRejected()
        {
            var s = At(7, 2200, 0);
            s.EndDay(Tx(), 7);
            Assert.AreEqual(Reject.RunEnded, s.BuyBox("B7-1-H1", Tx()).ReasonCode);
            Assert.AreEqual(Reject.RunEnded, s.EndDay(Tx(), 7).ReasonCode);
        }
    }
}

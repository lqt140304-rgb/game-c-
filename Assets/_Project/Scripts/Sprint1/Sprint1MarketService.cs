using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChoDoLa.Contracts;
using ChoDoLa.Gameplay;
using UnityEngine;

namespace ChoDoLa.Sprint1
{
    public sealed class Sprint1MarketService : MonoBehaviour, IMarketGameService
    {
        private MarketRunHost host;
        private MarketRun Run => host.Run;

        public int[] DebtSchedule => Run.DebtSchedule;
        public IReadOnlyDictionary<string, int> TodayPrices => Run.TodayPrices;
        public int ExtraCluePrice => Run.ExtraCluePrice;
        public IReadOnlyList<BoxOfferView> CurrentOffers => Run.Shelf.Select(offer => new BoxOfferView
        {
            OfferId = offer.Id,
            BoxTypeId = offer.TypeId,
            DisplayName = offer.Name,
            Price = offer.Price,
            TodayPrice = offer.TodayPrice,
            FakeRate = offer.FakeRate,
            CertainOrigin = OriginName(offer.CertainOrigin),
            ClueText = string.Join(" · ", offer.Clues),
            VisualVariantKey = "Box_" + offer.TypeId
        }).ToArray();
        public IReadOnlyList<BoxInstanceView> UnopenedBoxes => Run.Table.Select(box => ViewBox(box.Id, box.TypeId)).ToArray();
        public IReadOnlyList<ArtifactInstanceView> InventoryItems => Run.Inventory.Select(ViewItem).ToArray();

        public RunStateSnapshot CurrentRunState => new RunStateSnapshot
        {
            CurrentDay = Run.Day,
            MaxDays = DebtSchedule.Length,
            Cash = Run.Cash,
            DailyBoxesPurchased = Run.BoughtToday,
            MaxDailyBoxes = Run.DailyLimit,
            DebtDueToday = Run.DueToday,
            Arrears = Run.Arrears,
            IsRunEnded = Run.Status != "active",
            IsVictory = Run.Status == "won",
            EndReason = Run.EndReason switch
            {
                "all_debt_paid" => "Đã thanh toán tất cả các kỳ nợ.",
                "arrears_over_limit" => "Nợ tồn đã vượt giới hạn.",
                "arrears_on_last_day" => "Vẫn còn nợ khi hết ngày cuối.",
                _ => string.Empty
            },
            ActiveEvents = Array.Empty<ActiveEventView>(),
            InventoryTotalValue = Run.Inventory.Sum(item => item.ListedPrice)
        };

        public IReadOnlyList<HeirloomView> AvailableHeirlooms => Array.Empty<HeirloomView>();
        public HeirloomView ActiveHeirloom => null;
        public IReadOnlyList<TactArtifactView> TactCatalog => Array.Empty<TactArtifactView>();
        public IReadOnlyList<TactArtifactView> EquippedArtifacts => Array.Empty<TactArtifactView>();
        public IReadOnlyList<ComboView> ActiveCombos => Array.Empty<ComboView>();
        public IReadOnlyList<BikheView> BikheProgressList => Array.Empty<BikheView>();

        public event Action<RunStateSnapshot> OnRunStateChanged;
        public event Action<BoxInstanceView> OnBoxPurchased;
        public event Action<BoxInstanceView, ArtifactInstanceView> OnBoxOpened;
        public event Action OnInventoryChanged;
        public event Action<bool, int> OnDebtSettled;
        public event Action<string, string> OnActionRejected;
        public event Action<string> OnNotice;

        private void Awake() => Bind(GetComponent<MarketRunHost>() ?? gameObject.AddComponent<MarketRunHost>());
        private void Start() => RefreshState();

        internal void Bind(MarketRunHost runHost) => host = runHost;

        public bool RequestBuyClue(string offerId)
        {
            if (!Try(() => Run.BuyClue(NewTransaction(), offerId), out var result)) return false;
            RefreshState();
            var offer = CurrentOffers.First(item => item.OfferId == result.EntityId);
            OnNotice?.Invoke($"Dì Mận mách: {offer.CertainOrigin} (−{result.Amount} xu).");
            return true;
        }

        public bool RequestBuyBox(string offerId)
        {
            var selected = CurrentOffers.FirstOrDefault(offer => offer.OfferId == offerId);
            if (!Try(() => Run.BuyBox(NewTransaction(), offerId), out var result)) return false;
            var box = ViewBox(result.EntityId, selected.BoxTypeId);
            box.PricePaid = result.Amount;
            box.ClueText = selected.ClueText;
            OnBoxPurchased?.Invoke(box);
            RefreshState();
            OnNotice?.Invoke($"Đã mua {box.DisplayName} (−{result.Amount} xu). Mang hộp sang Tiệm Mở Hòm.");
            return true;
        }

        public bool RequestOpenBox(string boxInstanceId)
        {
            var box = UnopenedBoxes.FirstOrDefault(item => item.BoxInstanceId == boxInstanceId);
            if (!Try(() => Run.OpenBox(NewTransaction(), boxInstanceId), out var result)) return false;
            OnBoxOpened?.Invoke(box, ViewItem(Run.Inspect(result.EntityId)));
            RefreshState(inventoryChanged: true);
            return true;
        }

        public bool RequestSellArtifact(string artifactInstanceId)
        {
            if (!Try(() => Run.Sell(NewTransaction(), artifactInstanceId), out var result)) return false;
            RefreshState(inventoryChanged: true);
            var authenticity = result.WasFake == true ? "hàng giả" : "hàng thật";
            OnNotice?.Invoke($"Đã bán {authenticity} · +{result.Amount} xu.");
            return true;
        }

        public bool RequestKeepArtifact(string artifactInstanceId) => Run.Inspect(artifactInstanceId) != null;

        public bool RequestEndDay(int expectedDay)
        {
            var due = Run.DueToday;
            if (!Try(() => Run.EndDay(NewTransaction(), expectedDay), out var result)) return false;
            RefreshState();
            var remaining = Math.Max(0, due - result.Amount) + result.Penalty;
            OnDebtSettled?.Invoke(remaining == 0, Run.Cash);
            OnNotice?.Invoke($"Ngày {expectedDay}: đã thu {result.Amount} xu · phạt {result.Penalty} xu · nợ tồn {remaining} xu.");
            return true;
        }

        public int DevelopmentPoints => Run.DevelopmentPoints;
        public bool CanStartWithEarlyBlackChest => Run.CanStartWithEarlyBlackChest;
        public IReadOnlyList<MarketRun.SkillNode> SkillNodes => Run.SkillNodes;
        public IReadOnlyList<string> CollectedItems => Run.CollectedItems;

        public void RecordInspection(string itemId) => Try(() => Run.RecordInspection(NewTransaction(), itemId), out _);

        public bool UnlockSkill(string id) => Try(() => Run.UnlockSkill(NewTransaction(), id), out _);

        public void ResetRun() => StartNextRun(false);

        public bool StartNextRun(bool earlyBlackChest)
        {
            try
            {
                if (!host.StartNextRun(earlyBlackChest)) return false;
                RefreshState(inventoryChanged: true);
                return true;
            }
            catch (IOException) { return Reject("SAVE_FAILED", "Không thể ghi bản lưu mới."); }
            catch (UnauthorizedAccessException) { return Reject("SAVE_FAILED", "Không thể ghi bản lưu mới."); }
        }

        public bool RequestSelectHeirloom(string heirloomId) => SprintTwo();
        public bool RequestBuyTactArtifact(string tactId) => SprintTwo();
        public bool RequestEquipTactArtifact(string tactId, int slotIndex = -1) => SprintTwo();
        public bool RequestUnequipTactArtifact(int slotIndex) => SprintTwo();

        private bool SprintTwo() => Reject("NOT_AVAILABLE", "Tính năng này sẽ có ở Sprint 2.");

        private bool Try(Func<MarketRun.Result> action, out MarketRun.Result result)
        {
            result = null;
            try
            {
                result = action();
                if (result.Success) return true;
                return Reject(result.Code, result.Code switch
                {
                    "DAILY_BOX_LIMIT" => "Đã hết lượt mua hộp hôm nay.",
                    "NOT_ENOUGH_CASH" => "Bạn không đủ xu.",
                    "TABLE_FULL" => "Bàn mở đã đầy.",
                    "INVENTORY_FULL" => "Kho đầy. Hãy bán bớt hiện vật.",
                    "CLUE_LIMIT" => "Bạn đã mua lời mách cho hộp này.",
                    "STALE_DAY" => "Ngày này đã kết thúc.",
                    "RUN_ENDED" => "Ván chơi đã kết thúc.",
                    _ => "Thao tác không còn khả dụng."
                });
            }
            catch (IOException) { return Reject("SAVE_FAILED", "Không thể ghi bản lưu. Hãy thử lại."); }
            catch (UnauthorizedAccessException) { return Reject("SAVE_FAILED", "Không thể ghi bản lưu. Hãy thử lại."); }
        }

        private bool Reject(string code, string message)
        {
            OnActionRejected?.Invoke(code, message);
            return false;
        }

        private void RefreshState(bool inventoryChanged = false)
        {
            if (inventoryChanged) OnInventoryChanged?.Invoke();
            OnRunStateChanged?.Invoke(CurrentRunState);
        }

        private static string NewTransaction() => Guid.NewGuid().ToString("N");

        private static BoxInstanceView ViewBox(string id, string typeId) => new BoxInstanceView
        {
            BoxInstanceId = id,
            BoxTypeId = typeId,
            DisplayName = typeId switch
            {
                "H1" => "Hộp Gỗ Nhỏ",
                "H2" => "Hộp Kim Loại Mờ",
                "H3" => "Hộp Vải Dày",
                "RD" => "Rương Đen",
                _ => typeId
            },
            VisualVariantKey = "Box_" + typeId
        };

        private static string OriginName(string origin) => origin switch
        {
            "qua_khu" => "Quá khứ",
            "hien_dai" => "Hiện đại",
            "tuong_lai" => "Tương lai",
            "ngoai_hanh_tinh" => "Ngoài hành tinh",
            "co_tich" => "Cổ tích",
            "than_thoai" => "Thần thoại",
            _ => origin
        };

        private static ArtifactInstanceView ViewItem(MarketRun.ItemView item) => new ArtifactInstanceView
        {
            InstanceId = item.Id,
            ItemId = item.TypeId,
            DisplayName = item.Name,
            Category = OriginName(item.Origin),
            Quality = item.Quality switch
            {
                "Hỏng" => QualityTier.Damaged,
                "Xước" => QualityTier.Worn,
                _ => QualityTier.Pristine
            },
            Authenticity = AuthenticityState.Unverified,
            Rarity = item.Rarity switch
            {
                "rat_hiem" => RarityTier.Mythic,
                "hiem" => RarityTier.Rare,
                _ => RarityTier.Common
            },
            BaseValue = item.BasePrice,
            CurrentSellPrice = item.ListedPrice,
            AppraisalState = AppraisalState.Unappraised,
            VisualPrefabKey = "Item_" + item.TypeId,
            InspectClueText = string.Join(" · ", item.Signs)
        };
    }
}

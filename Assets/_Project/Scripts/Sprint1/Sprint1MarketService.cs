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
            ClockMinute = Run.DayClockMinute,
            ClockPhase = Run.ClockPhase,
            TimedDays = host.TimedDaysEnabled,
            CarryDebt = Run.CarryDebt,
            PostponeAvailable = Run.PostponeAvailable,
            ActiveHeirloomId = Run.ActiveHeirloomId,
            ActiveHeirloomName = Run.ActiveHeirloomName,
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
            InventoryTotalValue = Run.Inventory.Sum(item => item.ListedPrice),
            EquippedTactCount = Run.EquippedTacts.Count(id => id != null),
            MaxEquipSlots = Run.EquipSlotCount,
            ActiveComboCount = Run.ActiveCombos.Count,
            TotalBikheCount = 1,
            UnlockedBikheCount = Run.Bikhe1Unlocked ? 1 : 0
        };

        public IReadOnlyList<HeirloomView> AvailableHeirlooms => Array.Empty<HeirloomView>();
        public HeirloomView ActiveHeirloom => new HeirloomView { HeirloomId = Run.ActiveHeirloomId, DisplayName = Run.ActiveHeirloomName };
        public IReadOnlyList<TactArtifactView> TactCatalog
        {
            get
            {
                var slots = Run.EquippedTacts.ToList();
                return Run.TactCatalog.Select(item =>
                {
                    int slot = slots.IndexOf(item.Id);
                    return new TactArtifactView
                    {
                        TactId = item.Id,
                        DisplayName = item.Name,
                        SystemType = item.System == "co_phong" ? TactSystemType.ChronoSteam : TactSystemType.ArcaneRift,
                        Tier = item.Tier,
                        Price = item.Price,
                        IconModelName = item.Id,
                        IsOwned = item.Owned,
                        IsEquipped = slot >= 0,
                        EquippedSlotIndex = slot
                    };
                }).ToArray();
            }
        }

        public IReadOnlyList<TactArtifactView> EquippedArtifacts
        {
            get
            {
                var catalog = TactCatalog.ToDictionary(item => item.TactId);
                return Run.EquippedTacts.Select(id => id == null ? null : catalog[id]).ToArray();
            }
        }

        public IReadOnlyDictionary<string, int> TactResonance => Run.Resonance;
        public IReadOnlyList<ComboView> ActiveCombos => Run.TactCombos.Select(combo => new ComboView
        {
            ComboId = combo.Id,
            DisplayName = combo.Name,
            ConditionSummary = string.Join(" · ", combo.RequiredIds),
            EffectSummary = string.Join(" · ", combo.Effects.Select(DescribeComboEffect)),
            MissingTactIds = combo.MissingIds,
            IsActive = combo.MissingIds.Count == 0
        }).ToArray();
        public IReadOnlyList<BikheView> BikheProgressList => new[] { new BikheView
        {
            BikheId = "BK1",
            Title = "Lời Thề Bảy Ngày",
            ConditionSummary = "Hoàn thành một combo nhỏ trước ngày 5.",
            RewardSummary = "Mở ô thứ 5 đến hết ván.",
            CurrentProgress = Run.Bikhe1Unlocked ? 1 : 0,
            TargetProgress = 1,
            IsUnlocked = Run.Bikhe1Unlocked,
            IsRewardClaimed = Run.Bikhe1Unlocked
        } };

        private static string DescribeComboEffect(MarketRun.TactEffect effect) => effect.Type switch
        {
            "sell_bonus" => $"+{effect.Value:P0} giá bán",
            "extra_free_clue" => $"+{effect.Count} manh mối miễn phí",
            "free_appraisal" => "Giám định miễn phí khi dịch vụ mở",
            _ => effect.Type
        };

        public event Action<RunStateSnapshot> OnRunStateChanged;
        public event Action<BoxInstanceView> OnBoxPurchased;
        public event Action<BoxInstanceView, ArtifactInstanceView> OnBoxOpened;
        public event Action OnInventoryChanged;
        public event Action<bool, int> OnDebtSettled;
        public event Action<string, string> OnActionRejected;
        public event Action<string> OnNotice;

        private void Awake() => Bind(GetComponent<MarketRunHost>() ?? gameObject.AddComponent<MarketRunHost>());
        private void Start() => RefreshState();

        internal void Bind(MarketRunHost runHost)
        {
            if (host != null) host.ClockPhaseChanged -= HandleClockPhaseChanged;
            host = runHost;
            host.ClockPhaseChanged += HandleClockPhaseChanged;
        }

        private void OnDestroy()
        {
            if (host != null) host.ClockPhaseChanged -= HandleClockPhaseChanged;
        }

        public void EnableTimedDays() => host.EnableTimedDays();

        public bool CanSleep => host.TimedDaysEnabled && Run.IsNight && Run.Status == "active";

        private void HandleClockPhaseChanged()
        {
            RefreshState();
            if (Run.IsNight) OnNotice?.Invoke("Chợ đã đóng. Về phòng trọ nghỉ qua đêm.");
        }

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

        public bool RequestEndDay(int expectedDay, bool postpone = false)
        {
            if (!Try(() => host.TimedDaysEnabled
                ? Run.Sleep(NewTransaction(), expectedDay, postpone)
                : Run.EndDay(NewTransaction(), expectedDay, postpone), out var result)) return false;
            RefreshState();
            OnDebtSettled?.Invoke(Run.Arrears == 0, Run.Cash);
            string deferred = postpone ? $" · chuyển kỳ {Run.CarryDebt} xu sang ngày mai" : "";
            OnNotice?.Invoke($"Ngày {expectedDay}: đã thu {result.Amount} xu · phạt {result.Penalty} xu · nợ tồn {Run.Arrears} xu{deferred}.");
            return true;
        }

        public void ResetRun()
        {
            try
            {
                if (host.StartNextRun()) RefreshState(inventoryChanged: true);
            }
            catch (IOException) { Reject("SAVE_FAILED", "Không thể ghi bản lưu mới."); }
            catch (UnauthorizedAccessException) { Reject("SAVE_FAILED", "Không thể ghi bản lưu mới."); }
        }

        public bool RequestSelectHeirloom(string heirloomId) => SprintTwo();
        public bool RequestBuyTactArtifact(string tactId)
        {
            if (!Try(() => Run.BuyTact(NewTransaction(), tactId), out var result)) return false;
            RefreshState();
            OnNotice?.Invoke($"Đã mua Tạo Tác · −{result.Amount} xu.");
            return true;
        }

        public bool RequestEquipTactArtifact(string tactId, int slotIndex = -1)
        {
            if (slotIndex == -1) slotIndex = Run.EquippedTacts.ToList().IndexOf(null);
            if (slotIndex < 0) return Reject("INVALID_SLOT", "Chọn một ô trang bị.");
            bool hadBikhe = Run.Bikhe1Unlocked;
            if (!Try(() => Run.EquipTact(NewTransaction(), tactId, slotIndex), out _)) return false;
            RefreshState();
            if (!hadBikhe && Run.Bikhe1Unlocked) OnNotice?.Invoke("Bí Khế BK1 đã mở ô trang bị thứ 5.");
            return true;
        }

        public bool RequestUnequipTactArtifact(int slotIndex)
        {
            if (!Try(() => Run.UnequipTact(NewTransaction(), slotIndex), out _)) return false;
            RefreshState();
            return true;
        }

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
                    "WAIT_UNTIL_NIGHT" => "Về ngủ sau khi trời tối.",
                    "MARKET_CLOSED" => "Chợ đã đóng cửa.",
                    "POSTPONE_NOT_AVAILABLE" => "Không thể hoãn kỳ nợ này.",
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

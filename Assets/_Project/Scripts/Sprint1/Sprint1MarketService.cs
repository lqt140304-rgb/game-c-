using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ChoDoLa.Contracts;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Gameplay & economy service for Sprint 1 + Sprint 2 (Heirlooms, Tact Artifacts, Combos, Bí Khế, 7-day Save/Load).
    /// Implements IMarketGameService cleanly without duplicate transaction payouts.
    /// </summary>
    public class Sprint1MarketService : MonoBehaviour, IMarketGameService
    {
        [Serializable]
        private sealed class RunSaveData
        {
            public int CurrentDay;
            public int Cash;
            public int DailyBoxesPurchased;
            public bool IsDebtPaidToday;
            public bool IsRunEnded;
            public bool IsVictory;
            public string EndReason;
            public int NextId;
            public int TotalBoxesOpened;
            public string ActiveHeirloomId;
            public List<BoxInstanceView> Boxes = new List<BoxInstanceView>();
            public List<ArtifactInstanceView> AllArtifacts = new List<ArtifactInstanceView>();
            public List<string> OwnedTactIds = new List<string>();
            public List<string> EquippedSlotTactIds = new List<string>();
            public List<string> UnlockedBikheIds = new List<string>();
            public List<string> ClaimedTransactionIds = new List<string>();
            public List<ActiveEventView> ActiveEvents = new List<ActiveEventView>();
        }

        [Header("Run Configuration")]
        [SerializeField] private int startingCash = 1000;
        [SerializeField] private int maxDays = 7;
        [SerializeField] private int maxDailyBoxes = 3;
        [SerializeField] private int maxEquipSlots = 3;
        [SerializeField] private int randomSeed = 0;

        private System.Random _rng;
        private int _currentDay = 1;
        private int _cash;
        private int _dailyBoxesPurchased;
        private int _totalBoxesOpenedInRun;
        private bool _isDebtPaidToday;
        private bool _isRunEnded;
        private bool _isVictory;
        private string _endReason = string.Empty;
        private int _nextId = 1;

        private readonly List<BoxOfferView> _offers = new List<BoxOfferView>();
        private readonly List<BoxInstanceView> _boxes = new List<BoxInstanceView>();
        private readonly List<ArtifactInstanceView> _allArtifacts = new List<ArtifactInstanceView>();
        private readonly List<ArtifactInstanceView> _inventory = new List<ArtifactInstanceView>();
        private readonly List<ActiveEventView> _activeEvents = new List<ActiveEventView>();

        private readonly List<HeirloomView> _heirlooms = new List<HeirloomView>();
        private HeirloomView _activeHeirloom;
        private readonly List<TactArtifactView> _tactCatalog = new List<TactArtifactView>();
        private readonly List<TactArtifactView> _equippedSlots = new List<TactArtifactView>();
        private readonly List<ComboView> _combos = new List<ComboView>();
        private readonly List<BikheView> _bikheList = new List<BikheView>();
        private readonly HashSet<string> _claimedTransactionIds = new HashSet<string>();

        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "chodola_run_save.json");

        public RunStateSnapshot CurrentRunState => BuildSnapshot();
        public IReadOnlyList<BoxOfferView> CurrentOffers => _offers;
        public IReadOnlyList<BoxInstanceView> UnopenedBoxes => _boxes.FindAll(b => !b.IsOpened);
        public IReadOnlyList<ArtifactInstanceView> InventoryItems => _inventory;
        public int[] DebtSchedule => Sprint1Catalog.DebtSchedule;

        public IReadOnlyList<HeirloomView> AvailableHeirlooms => _heirlooms;
        public HeirloomView ActiveHeirloom => _activeHeirloom;
        public IReadOnlyList<TactArtifactView> TactCatalog => _tactCatalog;
        public IReadOnlyList<TactArtifactView> EquippedArtifacts => _equippedSlots;
        public IReadOnlyList<ComboView> ActiveCombos => _combos;
        public IReadOnlyList<BikheView> BikheProgressList => _bikheList;
        public bool HasSavedRun => File.Exists(SaveFilePath);

        public event Action<RunStateSnapshot> OnRunStateChanged;
        public event Action<BoxInstanceView> OnBoxPurchased;
        public event Action<BoxInstanceView, ArtifactInstanceView> OnBoxOpened;
        public event Action OnInventoryChanged;
        public event Action<bool, int> OnDebtSettled;
        public event Action<string, string> OnActionRejected;
        public event Action<string> OnNotice;
        public event Action OnTactLoadoutChanged;
        public event Action<BikheView> OnBikheUnlocked;

        private void Awake()
        {
            InitializeRun();
        }

        private void Start()
        {
            EmitAllState();
        }

        public void ResetRun()
        {
            InitializeRun();
            EmitAllState();
            OnNotice?.Invoke("Bắt đầu ván mới — Chúc bạn săn được nhiều cổ vật giá trị!");
        }

        private void InitializeRun()
        {
            int seed = randomSeed != 0 ? randomSeed : Environment.TickCount;
            _rng = new System.Random(seed);
            _currentDay = 1;
            _cash = startingCash;
            _dailyBoxesPurchased = 0;
            _totalBoxesOpenedInRun = 0;
            _isDebtPaidToday = false;
            _isRunEnded = false;
            _isVictory = false;
            _endReason = string.Empty;
            _nextId = 1;

            _boxes.Clear();
            _allArtifacts.Clear();
            _inventory.Clear();
            _claimedTransactionIds.Clear();

            InitializeSprint2Catalogs();

            _offers.Clear();
            _offers.AddRange(Sprint1Catalog.CreateDailyOffers());
            RefreshOfferPrices();

            _activeEvents.Clear();
            _activeEvents.Add(new ActiveEventView
            {
                EventId = "evt_day1",
                Title = "Phiên chợ đầu tuần",
                Description = "Dì Mận mới nhập 4 dòng hộp niêm phong. Ghé Kho Tạo Tác Kim Khuy để phối Combo."
            });
        }

        private void InitializeSprint2Catalogs()
        {
            _heirlooms.Clear();
            _heirlooms.AddRange(Sprint1Catalog.CreateHeirlooms());
            _activeHeirloom = _heirlooms.Count > 0 ? _heirlooms[0] : null;

            _tactCatalog.Clear();
            _tactCatalog.AddRange(Sprint1Catalog.CreateTactArtifacts());

            _equippedSlots.Clear();
            for (int i = 0; i < maxEquipSlots; i++)
            {
                _equippedSlots.Add(null);
            }

            _combos.Clear();
            _combos.AddRange(Sprint1Catalog.CreateCombos());

            _bikheList.Clear();
            _bikheList.AddRange(Sprint1Catalog.CreateBikheList());
        }

        private void EmitAllState()
        {
            OnTactLoadoutChanged?.Invoke();
            OnInventoryChanged?.Invoke();
            OnRunStateChanged?.Invoke(CurrentRunState);
        }

        public bool RequestBuyBox(string offerId)
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            int maxBoxes = GetEffectiveMaxDailyBoxes();
            if (_dailyBoxesPurchased >= maxBoxes)
            {
                Reject("MAX_DAILY_BOXES", $"Bạn đã mua đủ {maxBoxes} hộp trong ngày hôm nay!");
                return false;
            }

            BoxOfferView offer = _offers.Find(o => o.OfferId == offerId);
            if (offer == null)
            {
                Reject("OFFER_NOT_FOUND", "Không tìm thấy loại hộp này tại quầy.");
                return false;
            }

            if (_cash < offer.Price)
            {
                Reject("INSUFFICIENT_CASH", $"Không đủ xu! Cần {offer.Price} xu (hiện có {_cash} xu).");
                return false;
            }

            _cash -= offer.Price;
            _dailyBoxesPurchased++;

            BoxInstanceView box = new BoxInstanceView
            {
                BoxInstanceId = $"box_{_nextId++}",
                OfferId = offer.OfferId,
                BoxTypeId = offer.BoxTypeId,
                DisplayName = offer.DisplayName,
                PricePaid = offer.Price,
                VisualVariantKey = offer.VisualVariantKey,
                ClueText = offer.ClueText,
                IsOpened = false
            };

            _boxes.Add(box);
            OnBoxPurchased?.Invoke(box);
            OnRunStateChanged?.Invoke(CurrentRunState);
            OnNotice?.Invoke($"Đã mua {box.DisplayName} (-{offer.Price} xu). Hai tay đang ôm hộp — hãy mang sang Tiệm Mở Hòm bên cạnh để đặt lên bàn mở!");
            SaveRun();
            return true;
        }

        public bool RequestOpenBox(string boxInstanceId)
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            BoxInstanceView box = _boxes.Find(b => b.BoxInstanceId == boxInstanceId);
            if (box == null)
            {
                Reject("BOX_NOT_FOUND", "Không tìm thấy hộp trên bàn mở.");
                return false;
            }

            if (box.IsOpened)
            {
                Reject("BOX_ALREADY_OPENED", "Hộp này đã được mở niêm phong rồi.");
                return false;
            }

            box.IsOpened = true;
            _totalBoxesOpenedInRun++;
            ArtifactInstanceView artifact = RollArtifactForBox(box);
            _allArtifacts.Add(artifact);

            if (IsComboActive("combo_multiverse_grand") &&
                (artifact.Rarity == RarityTier.Rare || artifact.Rarity == RarityTier.Mythic))
            {
                string txId = $"grand_open_{box.BoxInstanceId}";
                if (_claimedTransactionIds.Add(txId))
                {
                    _cash += 80;
                    OnNotice?.Invoke("Đại Cộng Hưởng Dị Giới thưởng nóng +80 xu khi mở đồ Hiếm!");
                }
            }

            EvaluateCombosAndBikhe(notifyUnlock: true);
            OnBoxOpened?.Invoke(box, artifact);
            OnRunStateChanged?.Invoke(CurrentRunState);
            SaveRun();
            return true;
        }

        public bool RequestSellArtifact(string artifactInstanceId)
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            ArtifactInstanceView artifact = _allArtifacts.Find(a => a.InstanceId == artifactInstanceId);
            if (artifact == null)
            {
                Reject("ARTIFACT_NOT_FOUND", "Không tìm thấy hiện vật.");
                return false;
            }

            if (artifact.IsSold)
            {
                Reject("ALREADY_SOLD", "Hiện vật này đã được bán trước đó.");
                return false;
            }

            artifact.CurrentSellPrice = CalculateEffectiveSellPrice(artifact);
            artifact.IsSold = true;
            artifact.IsKeptInInventory = false;
            _inventory.Remove(artifact);

            _cash += artifact.CurrentSellPrice;
            OnInventoryChanged?.Invoke();
            OnRunStateChanged?.Invoke(CurrentRunState);
            OnNotice?.Invoke($"Đã bán {artifact.DisplayName} (+{artifact.CurrentSellPrice} xu)!");
            SaveRun();
            return true;
        }

        public bool RequestKeepArtifact(string artifactInstanceId)
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            ArtifactInstanceView artifact = _allArtifacts.Find(a => a.InstanceId == artifactInstanceId);
            if (artifact == null)
            {
                Reject("ARTIFACT_NOT_FOUND", "Không tìm thấy hiện vật.");
                return false;
            }

            if (artifact.IsSold)
            {
                Reject("ALREADY_SOLD", "Hiện vật đã bán, không thể cất vào kho.");
                return false;
            }

            if (artifact.IsKeptInInventory)
            {
                return true;
            }

            artifact.IsKeptInInventory = true;
            _inventory.Add(artifact);
            OnInventoryChanged?.Invoke();
            OnRunStateChanged?.Invoke(CurrentRunState);
            OnNotice?.Invoke($"Đã cất {artifact.DisplayName} vào kho (mở [Tab] để xem hoặc bán sau).");
            SaveRun();
            return true;
        }

        public bool RequestPayDebt()
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            if (_isDebtPaidToday)
            {
                Reject("DEBT_ALREADY_PAID", "Bạn đã thanh toán kỳ nợ hôm nay rồi.");
                return false;
            }

            int due = GetDebtForDay(_currentDay);
            if (_cash < due)
            {
                Reject("INSUFFICIENT_CASH", $"Thiếu xu trả nợ! Cần {due} xu (hiện có {_cash} xu).");
                OnDebtSettled?.Invoke(false, _cash);
                return false;
            }

            _cash -= due;
            _isDebtPaidToday = true;
            OnDebtSettled?.Invoke(true, _cash);
            OnRunStateChanged?.Invoke(CurrentRunState);
            OnNotice?.Invoke($"Anh Bảy đã thu đủ {due} xu kỳ nợ Ngày {_currentDay}!");
            SaveRun();
            return true;
        }

        public bool RequestEndDay()
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            if (!_isDebtPaidToday)
            {
                int due = GetDebtForDay(_currentDay);
                bool hasUnopened = _boxes.Exists(b => !b.IsOpened);
                bool hasInventory = _inventory.Count > 0;

                if (_cash >= due)
                {
                    Reject("DEBT_UNPAID", $"Hãy trả {due} xu tiền nợ hôm nay trước khi kết thúc ngày!");
                    return false;
                }

                if (hasUnopened || hasInventory)
                {
                    Reject("DEBT_UNPAID", "Chưa đủ xu trả nợ! Hãy mở hộp hoặc bán hiện vật trong kho trước.");
                    return false;
                }

                _isRunEnded = true;
                _isVictory = false;
                _endReason = $"Vỡ nợ ở Ngày {_currentDay}: thiếu {due - _cash} xu cho kỳ thu của Anh Bảy.";
                OnRunStateChanged?.Invoke(CurrentRunState);
                SaveRun();
                return true;
            }

            if (_currentDay >= maxDays)
            {
                _isRunEnded = true;
                _isVictory = true;
                _endReason = $"Hoàn thành 7 ngày trả nợ thành công! Tổng tiền mặt còn lại: {_cash} xu.";
                OnRunStateChanged?.Invoke(CurrentRunState);
                SaveRun();
                return true;
            }

            _currentDay++;
            _dailyBoxesPurchased = 0;
            _isDebtPaidToday = false;

            int newDue = GetDebtForDay(_currentDay);
            int maxBoxes = GetEffectiveMaxDailyBoxes();
            _activeEvents.Clear();
            _activeEvents.Add(new ActiveEventView
            {
                EventId = $"evt_day{_currentDay}",
                Title = $"Ngày {_currentDay}/{maxDays}",
                Description = $"Lượt mua hộp: 0/{maxBoxes}. Khoản nợ hôm nay: {newDue} xu."
            });

            OnRunStateChanged?.Invoke(CurrentRunState);
            OnNotice?.Invoke($"Sang Ngày {_currentDay}/{maxDays}! Lượt mua hộp đã đặt lại (0/{maxBoxes}).");
            SaveRun();
            return true;
        }

        public bool RequestSelectHeirloom(string heirloomId)
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            if (_currentDay > 1 || _dailyBoxesPurchased > 0 || _boxes.Count > 0)
            {
                Reject("HEIRLOOM_LOCKED", "Chỉ có thể chọn Cổ Vật Gia Truyền vào đầu Ngày 1 trước khi mua hộp!");
                return false;
            }

            HeirloomView found = _heirlooms.Find(h => h.HeirloomId == heirloomId);
            if (found == null)
            {
                Reject("HEIRLOOM_NOT_FOUND", "Không tìm thấy cổ vật gia truyền.");
                return false;
            }

            _activeHeirloom = found;
            RecalculateAllArtifactPrices();
            OnTactLoadoutChanged?.Invoke();
            OnRunStateChanged?.Invoke(CurrentRunState);
            OnNotice?.Invoke($"Đã mang theo gia bảo: {found.DisplayName}!");
            SaveRun();
            return true;
        }

        public bool RequestBuyTactArtifact(string tactId)
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            TactArtifactView tact = _tactCatalog.Find(t => t.TactId == tactId);
            if (tact == null)
            {
                Reject("TACT_NOT_FOUND", "Không tìm thấy Tạo Tác này trong Kho Kim Khuy.");
                return false;
            }

            if (tact.IsOwned)
            {
                Reject("TACT_OWNED", "Bạn đã sở hữu Tạo Tác này rồi.");
                return false;
            }

            if (_cash < tact.Price)
            {
                Reject("INSUFFICIENT_CASH", $"Không đủ xu mua {tact.DisplayName}! Cần {tact.Price} xu.");
                return false;
            }

            _cash -= tact.Price;
            tact.IsOwned = true;

            int freeSlot = _equippedSlots.FindIndex(s => s == null);
            if (freeSlot >= 0)
            {
                _equippedSlots[freeSlot] = tact;
                tact.IsEquipped = true;
                tact.EquippedSlotIndex = freeSlot;
            }

            EvaluateCombosAndBikhe(notifyUnlock: true);
            RecalculateAllArtifactPrices();
            RefreshOfferPrices();
            EmitAllState();
            OnNotice?.Invoke($"Đã mua {tact.DisplayName} (-{tact.Price} xu){(tact.IsEquipped ? $" & gắn vào Ô {tact.EquippedSlotIndex + 1}" : "")}!");
            SaveRun();
            return true;
        }

        public bool RequestEquipTactArtifact(string tactId, int slotIndex = -1)
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            TactArtifactView tact = _tactCatalog.Find(t => t.TactId == tactId);
            if (tact == null || !tact.IsOwned)
            {
                Reject("TACT_NOT_OWNED", "Bạn chưa sở hữu Tạo Tác này.");
                return false;
            }

            int targetSlot = slotIndex;
            if (targetSlot < 0 || targetSlot >= maxEquipSlots)
            {
                targetSlot = _equippedSlots.FindIndex(s => s == null);
            }

            if (targetSlot < 0)
            {
                Reject("SLOTS_FULL", $"Cả {maxEquipSlots} ô Tạo Tác đều đang đầy! Hãy tháo bớt 1 ô.");
                return false;
            }

            if (tact.IsEquipped && tact.EquippedSlotIndex >= 0 && tact.EquippedSlotIndex < maxEquipSlots)
            {
                _equippedSlots[tact.EquippedSlotIndex] = null;
            }

            if (_equippedSlots[targetSlot] != null)
            {
                _equippedSlots[targetSlot].IsEquipped = false;
                _equippedSlots[targetSlot].EquippedSlotIndex = -1;
            }

            _equippedSlots[targetSlot] = tact;
            tact.IsEquipped = true;
            tact.EquippedSlotIndex = targetSlot;

            EvaluateCombosAndBikhe(notifyUnlock: true);
            RecalculateAllArtifactPrices();
            RefreshOfferPrices();
            EmitAllState();
            OnNotice?.Invoke($"Đã trang bị {tact.DisplayName} vào Ô {targetSlot + 1}.");
            SaveRun();
            return true;
        }

        public bool RequestUnequipTactArtifact(int slotIndex)
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            if (slotIndex < 0 || slotIndex >= maxEquipSlots || _equippedSlots[slotIndex] == null)
            {
                return false;
            }

            TactArtifactView removed = _equippedSlots[slotIndex];
            removed.IsEquipped = false;
            removed.EquippedSlotIndex = -1;
            _equippedSlots[slotIndex] = null;

            EvaluateCombosAndBikhe(notifyUnlock: false);
            RecalculateAllArtifactPrices();
            RefreshOfferPrices();
            EmitAllState();
            OnNotice?.Invoke($"Đã tháo {removed.DisplayName} khỏi Ô {slotIndex + 1}.");
            SaveRun();
            return true;
        }

        public bool SaveRun()
        {
            try
            {
                var save = new RunSaveData
                {
                    CurrentDay = _currentDay,
                    Cash = _cash,
                    DailyBoxesPurchased = _dailyBoxesPurchased,
                    IsDebtPaidToday = _isDebtPaidToday,
                    IsRunEnded = _isRunEnded,
                    IsVictory = _isVictory,
                    EndReason = _endReason,
                    NextId = _nextId,
                    TotalBoxesOpened = _totalBoxesOpenedInRun,
                    ActiveHeirloomId = _activeHeirloom != null ? _activeHeirloom.HeirloomId : string.Empty
                };
                save.Boxes.AddRange(_boxes);
                save.AllArtifacts.AddRange(_allArtifacts);
                save.ActiveEvents.AddRange(_activeEvents);
                foreach (var t in _tactCatalog)
                {
                    if (t.IsOwned) save.OwnedTactIds.Add(t.TactId);
                }
                for (int i = 0; i < maxEquipSlots; i++)
                {
                    save.EquippedSlotTactIds.Add(_equippedSlots[i] != null ? _equippedSlots[i].TactId : string.Empty);
                }
                foreach (var b in _bikheList)
                {
                    if (b.IsUnlocked) save.UnlockedBikheIds.Add(b.BikheId);
                }
                foreach (string tx in _claimedTransactionIds)
                {
                    save.ClaimedTransactionIds.Add(tx);
                }

                string dir = Path.GetDirectoryName(SaveFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(SaveFilePath, JsonUtility.ToJson(save, prettyPrint: true));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ChoDoLa] SaveRun failed: {ex.Message}");
                return false;
            }
        }

        public bool LoadRun()
        {
            if (!File.Exists(SaveFilePath))
            {
                Reject("SAVE_NOT_FOUND", "Chưa có file lưu ván nào.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                RunSaveData save = JsonUtility.FromJson<RunSaveData>(json);
                if (save == null)
                {
                    Reject("SAVE_CORRUPT", "Dữ liệu lưu ván không hợp lệ.");
                    return false;
                }

                InitializeSprint2Catalogs();

                _currentDay = Mathf.Clamp(save.CurrentDay, 1, maxDays);
                _cash = Mathf.Max(0, save.Cash);
                _dailyBoxesPurchased = Mathf.Max(0, save.DailyBoxesPurchased);
                _isDebtPaidToday = save.IsDebtPaidToday;
                _isRunEnded = save.IsRunEnded;
                _isVictory = save.IsVictory;
                _endReason = save.EndReason ?? string.Empty;
                _nextId = Mathf.Max(1, save.NextId);
                _totalBoxesOpenedInRun = Mathf.Max(0, save.TotalBoxesOpened);

                if (!string.IsNullOrEmpty(save.ActiveHeirloomId))
                {
                    var hl = _heirlooms.Find(h => h.HeirloomId == save.ActiveHeirloomId);
                    if (hl != null) _activeHeirloom = hl;
                }

                _boxes.Clear();
                if (save.Boxes != null) _boxes.AddRange(save.Boxes);

                _allArtifacts.Clear();
                _inventory.Clear();
                if (save.AllArtifacts != null)
                {
                    _allArtifacts.AddRange(save.AllArtifacts);
                    foreach (var art in _allArtifacts)
                    {
                        if (art.IsKeptInInventory && !art.IsSold) _inventory.Add(art);
                    }
                }

                _activeEvents.Clear();
                if (save.ActiveEvents != null) _activeEvents.AddRange(save.ActiveEvents);

                if (save.OwnedTactIds != null)
                {
                    foreach (string tid in save.OwnedTactIds)
                    {
                        var t = _tactCatalog.Find(x => x.TactId == tid);
                        if (t != null) t.IsOwned = true;
                    }
                }

                if (save.EquippedSlotTactIds != null)
                {
                    for (int i = 0; i < maxEquipSlots && i < save.EquippedSlotTactIds.Count; i++)
                    {
                        string tid = save.EquippedSlotTactIds[i];
                        if (string.IsNullOrEmpty(tid)) continue;
                        var t = _tactCatalog.Find(x => x.TactId == tid && x.IsOwned);
                        if (t != null)
                        {
                            _equippedSlots[i] = t;
                            t.IsEquipped = true;
                            t.EquippedSlotIndex = i;
                        }
                    }
                }

                _claimedTransactionIds.Clear();
                if (save.ClaimedTransactionIds != null)
                {
                    foreach (string tx in save.ClaimedTransactionIds) _claimedTransactionIds.Add(tx);
                }

                if (save.UnlockedBikheIds != null)
                {
                    foreach (string bid in save.UnlockedBikheIds)
                    {
                        var b = _bikheList.Find(x => x.BikheId == bid);
                        if (b != null)
                        {
                            b.IsUnlocked = true;
                            b.IsRewardClaimed = _claimedTransactionIds.Contains($"bikhe_{bid}") || true;
                        }
                    }
                }

                EvaluateCombosAndBikhe(notifyUnlock: false);
                RecalculateAllArtifactPrices();
                RefreshOfferPrices();
                EmitAllState();
                OnNotice?.Invoke($"Đã tải ván lưu tại Ngày {_currentDay}/{maxDays} (Ví: {_cash} xu) — an toàn không trùng thưởng!");
                return true;
            }
            catch (Exception ex)
            {
                Reject("LOAD_FAILED", $"Lỗi khi tải ván lưu: {ex.Message}");
                return false;
            }
        }

        private ArtifactInstanceView RollArtifactForBox(BoxInstanceView box)
        {
            IReadOnlyList<ArtifactTemplate> pool = Sprint1Catalog.GetPoolForBox(box.BoxTypeId);
            ArtifactTemplate template = pool[_rng.Next(pool.Count)];
            if (HasEquipped("tact_cs_t3_bird") && pool.Count > 1 && _rng.Next(100) < 35)
            {
                ArtifactTemplate alt = pool[_rng.Next(pool.Count)];
                if (alt.DefaultRarity > template.DefaultRarity || alt.BaseValue > template.BaseValue)
                {
                    template = alt;
                }
            }

            int qRoll = _rng.Next(100) + (HasEquipped("tact_cs_t3_bird") ? 12 : 0);
            QualityTier quality = qRoll < 18 ? QualityTier.Damaged
                                : qRoll < 48 ? QualityTier.Worn
                                : qRoll < 85 ? QualityTier.Good
                                : QualityTier.Pristine;

            AuthenticityState auth = _rng.Next(100) < 75
                ? AuthenticityState.Authentic
                : AuthenticityState.Counterfeit;

            string clue = auth == AuthenticityState.Authentic
                ? template.ClueAuthentic
                : template.ClueCounterfeit;

            ArtifactInstanceView art = new ArtifactInstanceView
            {
                InstanceId = $"art_{_nextId++}",
                ItemId = template.ItemId,
                DisplayName = template.DisplayName,
                Category = template.Category,
                Quality = quality,
                Authenticity = auth,
                Rarity = template.DefaultRarity,
                BaseValue = template.BaseValue,
                CurrentSellPrice = 0,
                AppraisalState = AppraisalState.QuickChecked,
                VisualPrefabKey = template.VisualPrefabKey,
                InspectClueText = clue,
                IsKeptInInventory = false,
                IsSold = false
            };
            art.CurrentSellPrice = CalculateEffectiveSellPrice(art);
            return art;
        }

        private bool HasEquipped(string tactId)
        {
            return _equippedSlots.Exists(t => t != null && t.TactId == tactId);
        }

        private bool IsComboActive(string comboId)
        {
            ComboView c = _combos.Find(x => x.ComboId == comboId);
            return c != null && c.IsActive;
        }

        private bool IsBikheUnlocked(string bikheId)
        {
            BikheView b = _bikheList.Find(x => x.BikheId == bikheId);
            return b != null && b.IsUnlocked;
        }

        private void EvaluateCombosAndBikhe(bool notifyUnlock)
        {
            int csCount = 0;
            int arCount = 0;
            int eqCount = 0;
            foreach (TactArtifactView t in _equippedSlots)
            {
                if (t == null) continue;
                eqCount++;
                if (t.SystemType == TactSystemType.ChronoSteam) csCount++;
                else arCount++;
            }

            int activeCombos = 0;
            foreach (ComboView c in _combos)
            {
                c.IsActive = c.RequiresMixedSystems
                    ? (eqCount >= c.RequiredCount && csCount >= 1 && arCount >= 1)
                    : (c.RequiredSystem == TactSystemType.ChronoSteam ? csCount >= c.RequiredCount : arCount >= c.RequiredCount);
                if (c.IsActive) activeCombos++;
            }

            foreach (BikheView b in _bikheList)
            {
                if (b.BikheId == "bikhe_box_opener")
                {
                    b.CurrentProgress = Mathf.Min(b.TargetProgress, _totalBoxesOpenedInRun);
                    if (_totalBoxesOpenedInRun >= b.TargetProgress) b.IsUnlocked = true;
                }
                else if (b.BikheId == "bikhe_combo_smith")
                {
                    b.CurrentProgress = activeCombos > 0 || b.IsUnlocked ? 1 : 0;
                    if (activeCombos >= b.TargetProgress) b.IsUnlocked = true;
                }

                string claimKey = $"bikhe_{b.BikheId}";
                if (b.IsUnlocked && !b.IsRewardClaimed && _claimedTransactionIds.Add(claimKey))
                {
                    b.IsRewardClaimed = true;
                    int rewardCash = b.BikheId == "bikhe_box_opener" ? 250 : 200;
                    _cash += rewardCash;
                    if (notifyUnlock)
                    {
                        OnBikheUnlocked?.Invoke(b);
                        OnNotice?.Invoke($"Mở khóa {b.Title}! Thưởng nóng +{rewardCash} xu.");
                    }
                }
            }
        }

        private int CalculateEffectiveSellPrice(ArtifactInstanceView art)
        {
            float qualityMult = art.Quality switch
            {
                QualityTier.Damaged => 0.65f,
                QualityTier.Worn => 0.85f,
                QualityTier.Good => 1.10f,
                QualityTier.Pristine => 1.45f,
                _ => 1.0f
            };
            float authMult = art.Authenticity == AuthenticityState.Authentic ? 1.15f : 0.48f;

            float bonusPct = 0f;
            if (_activeHeirloom != null)
            {
                bool isSteamArt = art.Category.Contains("Cơ khí") || art.Category.Contains("Thời") || art.Category.Contains("Đồng");
                if (_activeHeirloom.HeirloomId == "heirloom_clockwork_monocle" && isSteamArt) bonusPct += 0.12f;
                if (_activeHeirloom.HeirloomId == "heirloom_rift_seal" && !isSteamArt) bonusPct += 0.12f;
            }

            if (HasEquipped("tact_cs_t1_gear")) bonusPct += 0.10f;
            if (HasEquipped("tact_cs_t2_watch")) bonusPct += 0.18f;
            if (HasEquipped("tact_cs_t3_bird")) bonusPct += 0.28f;
            if (HasEquipped("tact_ar_t1_amulet")) bonusPct += 0.08f;
            if (HasEquipped("tact_ar_t2_compass")) bonusPct += 0.15f;
            if (HasEquipped("tact_ar_t3_prism")) bonusPct += 0.26f;

            if (IsComboActive("combo_chrono_overdrive")) bonusPct += 0.15f;
            if (IsComboActive("combo_rift_convergence")) bonusPct += 0.12f;
            if (IsComboActive("combo_multiverse_grand")) bonusPct += 0.25f;

            return Mathf.Max(25, Mathf.RoundToInt(art.BaseValue * qualityMult * authMult * (1f + bonusPct)));
        }

        private void RecalculateAllArtifactPrices()
        {
            foreach (ArtifactInstanceView art in _allArtifacts)
            {
                if (!art.IsSold)
                {
                    art.CurrentSellPrice = CalculateEffectiveSellPrice(art);
                }
            }
        }

        private void RefreshOfferPrices()
        {
            List<BoxOfferView> baseOffers = Sprint1Catalog.CreateDailyOffers();
            bool hasDiscount = HasEquipped("tact_ar_t2_compass");
            for (int i = 0; i < _offers.Count && i < baseOffers.Count; i++)
            {
                _offers[i].Price = hasDiscount
                    ? Mathf.Max(50, Mathf.RoundToInt(baseOffers[i].Price * 0.85f))
                    : baseOffers[i].Price;
            }
        }

        private int GetEffectiveMaxDailyBoxes()
        {
            int cap = maxDailyBoxes;
            if (HasEquipped("tact_cs_t2_watch")) cap += 1;
            if (IsComboActive("combo_chrono_overdrive")) cap += 1;
            if (IsBikheUnlocked("bikhe_combo_smith")) cap += 1;
            return cap;
        }

        private int GetDebtForDay(int day)
        {
            int idx = Mathf.Clamp(day - 1, 0, Sprint1Catalog.DebtSchedule.Length - 1);
            int baseDebt = Sprint1Catalog.DebtSchedule[idx];

            int reduction = 0;
            if (_activeHeirloom != null && _activeHeirloom.HeirloomId == "heirloom_rift_seal") reduction += 50;
            if (HasEquipped("tact_ar_t1_amulet")) reduction += 40;
            if (HasEquipped("tact_ar_t3_prism")) reduction += 90;
            if (IsComboActive("combo_rift_convergence")) reduction += 120;

            return Mathf.Max(50, baseDebt - reduction);
        }

        private RunStateSnapshot BuildSnapshot()
        {
            int equippedCount = 0;
            foreach (var t in _equippedSlots)
            {
                if (t != null) equippedCount++;
            }

            int comboCount = 0;
            foreach (var c in _combos)
            {
                if (c.IsActive) comboCount++;
            }

            int unlockedBikhe = 0;
            foreach (var b in _bikheList)
            {
                if (b.IsUnlocked) unlockedBikhe++;
            }

            int invVal = 0;
            foreach (var item in _inventory)
            {
                if (!item.IsSold) invVal += item.CurrentSellPrice;
            }

            return new RunStateSnapshot
            {
                CurrentDay = _currentDay,
                MaxDays = maxDays,
                Cash = _cash,
                DailyBoxesPurchased = _dailyBoxesPurchased,
                MaxDailyBoxes = GetEffectiveMaxDailyBoxes(),
                DebtDueToday = GetDebtForDay(_currentDay),
                IsDebtPaidToday = _isDebtPaidToday,
                IsRunEnded = _isRunEnded,
                IsVictory = _isVictory,
                EndReason = _endReason,
                ActiveEvents = _activeEvents.AsReadOnly(),

                ActiveHeirloomId = _activeHeirloom != null ? _activeHeirloom.HeirloomId : string.Empty,
                ActiveHeirloomName = _activeHeirloom != null ? _activeHeirloom.DisplayName : "Không có",
                EquippedTactCount = equippedCount,
                MaxEquipSlots = maxEquipSlots,
                ActiveComboCount = comboCount,
                UnlockedBikheCount = unlockedBikhe,
                TotalBikheCount = _bikheList.Count,
                InventoryTotalValue = invVal,
                HasSavedRun = HasSavedRun
            };
        }

        private void Reject(string code, string message)
        {
            OnActionRejected?.Invoke(code, message);
        }
    }
}

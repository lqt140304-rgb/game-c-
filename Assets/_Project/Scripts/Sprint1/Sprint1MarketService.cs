using System;
using System.Collections.Generic;
using UnityEngine;
using ChoDoLa.Contracts;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Prototype gameplay & economy service for Sprint 1, strictly implementing IMarketGameService
    /// so Codex can swap or back it without changing 3D/UI presentation scripts.
    /// </summary>
    public class Sprint1MarketService : MonoBehaviour, IMarketGameService
    {
        [Header("Run Configuration")]
        [SerializeField] private int startingCash = 1000;
        [SerializeField] private int maxDays = 7;
        [SerializeField] private int maxDailyBoxes = 3;
        [SerializeField] private int randomSeed = 0;

        private System.Random _rng;
        private int _currentDay = 1;
        private int _cash;
        private int _dailyBoxesPurchased;
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

        public RunStateSnapshot CurrentRunState => BuildSnapshot();
        public IReadOnlyList<BoxOfferView> CurrentOffers => _offers;
        public IReadOnlyList<BoxInstanceView> UnopenedBoxes => _boxes.FindAll(b => !b.IsOpened);
        public IReadOnlyList<ArtifactInstanceView> InventoryItems => _inventory;
        public int[] DebtSchedule => Sprint1Catalog.DebtSchedule;

        public event Action<RunStateSnapshot> OnRunStateChanged;
        public event Action<BoxInstanceView> OnBoxPurchased;
        public event Action<BoxInstanceView, ArtifactInstanceView> OnBoxOpened;
        public event Action OnInventoryChanged;
        public event Action<bool, int> OnDebtSettled;
        public event Action<string, string> OnActionRejected;
        public event Action<string> OnNotice;

        private void Awake()
        {
            InitializeRun();
        }

        private void Start()
        {
            OnRunStateChanged?.Invoke(CurrentRunState);
            OnInventoryChanged?.Invoke();
        }

        public void ResetRun()
        {
            InitializeRun();
            OnRunStateChanged?.Invoke(CurrentRunState);
            OnInventoryChanged?.Invoke();
            OnNotice?.Invoke("Bắt đầu ván mới — Chúc bạn săn được nhiều cổ vật giá trị!");
        }

        private void InitializeRun()
        {
            int seed = randomSeed != 0 ? randomSeed : Environment.TickCount;
            _rng = new System.Random(seed);
            _currentDay = 1;
            _cash = startingCash;
            _dailyBoxesPurchased = 0;
            _isDebtPaidToday = false;
            _isRunEnded = false;
            _isVictory = false;
            _endReason = string.Empty;
            _nextId = 1;

            _boxes.Clear();
            _allArtifacts.Clear();
            _inventory.Clear();
            _offers.Clear();
            _offers.AddRange(Sprint1Catalog.CreateDailyOffers());

            _activeEvents.Clear();
            _activeEvents.Add(new ActiveEventView
            {
                EventId = "evt_day1",
                Title = "Phiên chợ đầu tuần",
                Description = "Dì Mận mới nhập 4 dòng hộp niêm phong. Giới hạn 3 hộp/ngày."
            });
        }

        public bool RequestBuyBox(string offerId)
        {
            if (_isRunEnded)
            {
                Reject("RUN_ENDED", "Ván chơi đã kết thúc.");
                return false;
            }

            if (_dailyBoxesPurchased >= maxDailyBoxes)
            {
                Reject("MAX_DAILY_BOXES", $"Bạn đã mua đủ {maxDailyBoxes} hộp trong ngày hôm nay!");
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
            OnNotice?.Invoke($"Đã mua {box.DisplayName} (-{offer.Price} xu). Mang ra Bàn Mở để tháo niêm phong!");
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
            ArtifactInstanceView artifact = RollArtifactForBox(box);
            _allArtifacts.Add(artifact);

            OnBoxOpened?.Invoke(box, artifact);
            OnRunStateChanged?.Invoke(CurrentRunState);
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

            artifact.IsSold = true;
            artifact.IsKeptInInventory = false;
            _inventory.Remove(artifact);

            _cash += artifact.CurrentSellPrice;
            OnInventoryChanged?.Invoke();
            OnRunStateChanged?.Invoke(CurrentRunState);
            OnNotice?.Invoke($"Đã bán {artifact.DisplayName} (+{artifact.CurrentSellPrice} xu)!");
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
            OnNotice?.Invoke($"Đã cất {artifact.DisplayName} vào kho (mở [Tab] để xem hoặc bán sau).");
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

                // Soft-lock guard: no boxes, no inventory, and cash < debt -> end run as defeat.
                _isRunEnded = true;
                _isVictory = false;
                _endReason = $"Vỡ nợ ở Ngày {_currentDay}: thiếu {due - _cash} xu cho kỳ thu của Anh Bảy.";
                OnRunStateChanged?.Invoke(CurrentRunState);
                return true;
            }

            if (_currentDay >= maxDays)
            {
                _isRunEnded = true;
                _isVictory = true;
                _endReason = $"Hoàn thành 7 ngày trả nợ! Tổng tiền mặt còn lại: {_cash} xu.";
                OnRunStateChanged?.Invoke(CurrentRunState);
                return true;
            }

            _currentDay++;
            _dailyBoxesPurchased = 0;
            _isDebtPaidToday = false;

            _activeEvents.Clear();
            _activeEvents.Add(new ActiveEventView
            {
                EventId = $"evt_day{_currentDay}",
                Title = $"Ngày {_currentDay}/7",
                Description = $"Lượt mua hộp đã đặt lại (3/3). Khoản nợ hôm nay: {GetDebtForDay(_currentDay)} xu."
            });

            OnRunStateChanged?.Invoke(CurrentRunState);
            OnNotice?.Invoke($"Sang Ngày {_currentDay}/{maxDays}! Lượt mua hộp đã đặt lại (0/{maxDailyBoxes}).");
            return true;
        }

        private ArtifactInstanceView RollArtifactForBox(BoxInstanceView box)
        {
            IReadOnlyList<ArtifactTemplate> pool = Sprint1Catalog.GetPoolForBox(box.BoxTypeId);
            ArtifactTemplate template = pool[_rng.Next(pool.Count)];

            // Roll quality
            int qRoll = _rng.Next(100);
            QualityTier quality = qRoll < 18 ? QualityTier.Damaged
                                : qRoll < 48 ? QualityTier.Worn
                                : qRoll < 85 ? QualityTier.Good
                                : QualityTier.Pristine;

            // Roll authenticity (75% authentic, 25% counterfeit)
            AuthenticityState auth = _rng.Next(100) < 75
                ? AuthenticityState.Authentic
                : AuthenticityState.Counterfeit;

            float qualityMult = quality switch
            {
                QualityTier.Damaged => 0.65f,
                QualityTier.Worn => 0.85f,
                QualityTier.Good => 1.10f,
                QualityTier.Pristine => 1.45f,
                _ => 1.0f
            };

            float authMult = auth == AuthenticityState.Authentic ? 1.15f : 0.48f;
            int sellPrice = Mathf.Max(25, Mathf.RoundToInt(template.BaseValue * qualityMult * authMult));

            string clue = auth == AuthenticityState.Authentic
                ? template.ClueAuthentic
                : template.ClueCounterfeit;

            return new ArtifactInstanceView
            {
                InstanceId = $"art_{_nextId++}",
                ItemId = template.ItemId,
                DisplayName = template.DisplayName,
                Category = template.Category,
                Quality = quality,
                Authenticity = auth,
                Rarity = template.DefaultRarity,
                BaseValue = template.BaseValue,
                CurrentSellPrice = sellPrice,
                AppraisalState = AppraisalState.QuickChecked,
                VisualPrefabKey = template.VisualPrefabKey,
                InspectClueText = clue,
                IsKeptInInventory = false,
                IsSold = false
            };
        }

        private int GetDebtForDay(int day)
        {
            int idx = Mathf.Clamp(day - 1, 0, Sprint1Catalog.DebtSchedule.Length - 1);
            return Sprint1Catalog.DebtSchedule[idx];
        }

        private RunStateSnapshot BuildSnapshot()
        {
            return new RunStateSnapshot
            {
                CurrentDay = _currentDay,
                MaxDays = maxDays,
                Cash = _cash,
                DailyBoxesPurchased = _dailyBoxesPurchased,
                MaxDailyBoxes = maxDailyBoxes,
                DebtDueToday = GetDebtForDay(_currentDay),
                IsDebtPaidToday = _isDebtPaidToday,
                IsRunEnded = _isRunEnded,
                IsVictory = _isVictory,
                EndReason = _endReason,
                ActiveEvents = _activeEvents.AsReadOnly()
            };
        }

        private void Reject(string code, string message)
        {
            OnActionRejected?.Invoke(code, message);
        }
    }
}

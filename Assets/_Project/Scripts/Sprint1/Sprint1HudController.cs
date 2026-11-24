using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ChoDoLa.Contracts;
using ChoDoLa.Interaction;
using ChoDoLa.Player;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Stylized Low-Poly "Gỗ · Đồng · Giấy dó" HUD & Transaction UI Controller.
    /// Implements all 6 screens from the approved UI design:
    /// 1) Minimal Street HUD (Hex Day, Cash/Debt, Objective Card, Hex [E] Prompt)
    /// 2) Two-Hand Carrying HUD (Live Compass toward Tiệm Mở Hòm + Bottom Carry Card)
    /// 3) Dì Mận Counter Dialogue + Hanging Paper Offer Tags
    /// 4) Giấy dó 3D Artifact Appraisal Ticket + Wax Seal Stamp
    /// 5) Two-Page Open Ledger Book + Colored Side Ribbons
    /// 6) End-of-Run Receipt + Wax Stamp
    /// </summary>
    public class Sprint1HudController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Sprint1MarketService marketService;
        [SerializeField] private ArtifactInspector3D inspector3D;
        [SerializeField] private OpenTableManager openTableManager;
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private PlayerInteractor playerInteractor;
        [SerializeField] private PlayerBoxCarrier boxCarrier;

        private Font _font;
        private Canvas _canvas;

        // HUD Elements
        private GameObject _hudRoot;
        private Text _dayNumText;
        private Text _cashText;
        private Text _cashDeltaText;
        private Text _debtText;
        private Image _debtFillImg;
        private Text _boxesText;
        private readonly Image[] _boxPips = new Image[3];

        private GameObject _objectiveCard;
        private Text _objStep1;
        private Text _objStep2;
        private Text _objStep3;

        private GameObject _compassBar;
        private RectTransform _compassMarkerRt;
        private Text _compassDistText;
        private Transform _dropZoneTarget;

        private GameObject _promptPill;
        private Text _promptText;
        private GameObject _carryBanner;
        private Text _carryTitleText;
        private Text _carrySubText;
        private GameObject _toastBanner;
        private Image _toastBg;
        private Text _toastText;
        private float _toastTimer;
        private float _deltaTimer;
        private int _lastCash = -1;

        // Counter Modal (Dì Mận Dialogue + Hanging Paper Offer Tags)
        private GameObject _counterModal;
        private Transform _counterListRoot;
        private Text _counterWalletText;
        private Text _counterQuotaText;
        private Text _dialogueQuoteText;
        private Button _dialogueBuyBtn;
        private Button _tipButton;
        private string _highlightedOfferId;
        private int _activeLedgerTab;

        // Inspect Modal (Giấy dó Appraisal Ticket)
        private GameObject _inspectModal;
        private GameObject _inspectHintsBar;
        private Text _insName;
        private Text _insCategory;
        private Text _insQuality;
        private Text _insAuth;
        private Text _insRarity;
        private Text _insClue;
        private Text _insBasePrice;
        private Text _insPrice;
        private Image _insStampRing;
        private Text _insStampText;
        private Button _insSellBtn;
        private Button _insKeepBtn;
        private ArtifactInstanceView _inspectingArtifact;

        // Ledger Book Modal
        private GameObject _ledgerModal;
        private GameObject _invTabContent;
        private GameObject _tactTabContent;
        private GameObject _bikheTabContent;
        private GameObject _debtTabContent;
        private Transform _invListRoot;
        private Transform _tactListRoot;
        private Transform _bikheListRoot;
        private Transform _debtListRoot;
        private ScrollRect _invScroll;
        private ScrollRect _tactScroll;
        private ScrollRect _bikheScroll;
        private Text _debtSummaryText;
        private int _ledgerConfirmedDay;
        private Button _endDayBtn;
        private RectTransform _tabInvRt;
        private RectTransform _tabTactRt;
        private RectTransform _tabBikheRt;
        private RectTransform _tabDebtRt;

        // Run End Receipt Modal
        private GameObject _runEndModal;
        private Text _runEndTitle;
        private Text _runEndBody;
        private Image _runEndStampRing;
        private Text _runEndStampText;
        private Transform _runEndSkillRoot;
        private Button _earlyChestButton;

        // "Gỗ · Đồng · Giấy dó" Palette
        private static readonly Color ColWalnut = new Color(0.169f, 0.106f, 0.067f, 0.95f);
        private static readonly Color ColWalnutLight = new Color(0.227f, 0.149f, 0.090f, 0.96f);
        private static readonly Color ColBookCover = new Color(0.290f, 0.173f, 0.094f, 0.98f);
        private static readonly Color ColBrass = new Color(0.831f, 0.627f, 0.263f, 1f);
        private static readonly Color ColBrassDark = new Color(0.541f, 0.392f, 0.141f, 1f);
        private static readonly Color ColGold = new Color(0.914f, 0.725f, 0.286f, 1f);
        private static readonly Color ColCream = new Color(0.961f, 0.910f, 0.800f, 1f);
        private static readonly Color ColMuted = new Color(0.76f, 0.69f, 0.58f, 1f);

        private static readonly Color ColPaper = new Color(0.937f, 0.878f, 0.741f, 0.99f);
        private static readonly Color ColPaperDark = new Color(0.886f, 0.804f, 0.624f, 0.98f);
        private static readonly Color ColPaperHighlight = new Color(0.975f, 0.925f, 0.795f, 1f);
        private static readonly Color ColInk = new Color(0.227f, 0.145f, 0.086f, 1f);
        private static readonly Color ColInkMuted = new Color(0.451f, 0.345f, 0.247f, 1f);

        private static readonly Color ColWaxRed = new Color(0.639f, 0.145f, 0.169f, 1f);
        private static readonly Color ColJade = new Color(0.243f, 0.604f, 0.463f, 1f);
        private static readonly Color ColTeal = new Color(0.180f, 0.439f, 0.420f, 1f);
        private static readonly Color ColSky = new Color(0.373f, 0.702f, 0.788f, 1f);
        private static readonly Color ColAmber = new Color(0.86f, 0.58f, 0.18f, 1f);
        private static readonly Color ColPurple = new Color(0.380f, 0.235f, 0.470f, 1f);
        private static readonly Color ColDisabled = new Color(0.56f, 0.51f, 0.45f, 1f);

        public void Configure(
            Sprint1MarketService service,
            ArtifactInspector3D inspector,
            OpenTableManager tableMgr,
            FirstPersonController fpc,
            PlayerInteractor interactor,
            PlayerBoxCarrier carrier = null)
        {
            marketService = service;
            inspector3D = inspector;
            openTableManager = tableMgr;
            playerController = fpc;
            playerInteractor = interactor;
            if (carrier != null) boxCarrier = carrier;
        }

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            BuildUiHierarchy();
        }

        private void OnEnable()
        {
            if (marketService != null)
            {
                marketService.OnRunStateChanged += HandleRunStateChanged;
                marketService.OnInventoryChanged += HandleInventoryChanged;
                marketService.OnActionRejected += HandleActionRejected;
                marketService.OnNotice += HandleNotice;
            }

            if (playerInteractor != null)
            {
                playerInteractor.OnPromptChanged += HandlePromptChanged;
            }

            if (boxCarrier != null)
            {
                boxCarrier.OnCarriedBoxChanged -= HandleCarriedBoxChanged;
                boxCarrier.OnCarriedBoxChanged += HandleCarriedBoxChanged;
            }
        }

        private void OnDisable()
        {
            if (marketService != null)
            {
                marketService.OnRunStateChanged -= HandleRunStateChanged;
                marketService.OnInventoryChanged -= HandleInventoryChanged;
                marketService.OnActionRejected -= HandleActionRejected;
                marketService.OnNotice -= HandleNotice;
            }

            if (playerInteractor != null)
            {
                playerInteractor.OnPromptChanged -= HandlePromptChanged;
            }

            if (boxCarrier != null)
            {
                boxCarrier.OnCarriedBoxChanged -= HandleCarriedBoxChanged;
            }
        }

        private void Start()
        {
            if (boxCarrier == null)
            {
                boxCarrier = FindAnyObjectByType<PlayerBoxCarrier>();
                if (boxCarrier != null)
                {
                    boxCarrier.OnCarriedBoxChanged -= HandleCarriedBoxChanged;
                    boxCarrier.OnCarriedBoxChanged += HandleCarriedBoxChanged;
                }
            }

            UnboxingTableDropZone dz = FindAnyObjectByType<UnboxingTableDropZone>();
            if (dz != null) _dropZoneTarget = dz.transform;

            if (marketService != null)
            {
                HandleRunStateChanged(marketService.CurrentRunState);
                RefreshAllLedgerTabs();
            }
            CloseAllModals();
            RefreshCarryBanner();
        }

        private void Update()
        {
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.deltaTime;
                if (_toastTimer <= 0f && _toastBanner != null)
                {
                    _toastBanner.SetActive(false);
                }
            }

            if (_deltaTimer > 0f)
            {
                _deltaTimer -= Time.deltaTime;
                if (_deltaTimer <= 0f && _cashDeltaText != null)
                {
                    _cashDeltaText.gameObject.SetActive(false);
                }
            }

            UpdateCompass();

            if (IsTabPressed())
            {
                if (_ledgerModal != null && _ledgerModal.activeSelf)
                {
                    CloseAllModals();
                }
                else if (inspector3D == null || !inspector3D.IsInspecting)
                {
                    OpenLedgerModal(startOnDebtTab: false);
                }
            }
            else if (IsEscapePressed())
            {
                if (inspector3D != null && inspector3D.IsInspecting)
                {
                    LeaveArtifactOnTable();
                }
                else if (IsAnyModalOpen())
                {
                    CloseAllModals();
                }
            }
            else if (_ledgerModal != null && _ledgerModal.activeSelf)
            {
                if (IsKeyQPressed()) SwitchLedgerTab(_activeLedgerTab == 0 ? 3 : _activeLedgerTab == 3 ? 2 : 0);
                else if (IsKeyEPressed()) SwitchLedgerTab(_activeLedgerTab == 0 ? 2 : _activeLedgerTab == 2 ? 3 : 0);
            }
            else if (_counterModal != null && _counterModal.activeSelf)
            {
                if (IsDigitPressed(1)) TryBuyHighlightedOffer();
                else if (IsDigitPressed(2)) ShowSelectedOfferClue();
                else if (IsDigitPressed(3)) CloseAllModals();
            }
        }

        public bool IsAnyModalOpen()
        {
            return (_counterModal != null && _counterModal.activeSelf)
                || (_inspectModal != null && _inspectModal.activeSelf)
                || (_ledgerModal != null && _ledgerModal.activeSelf)
                || (_runEndModal != null && _runEndModal.activeSelf);
        }

        public void OpenCounterModal(string highlightOfferId)
        {
            CloseAllModals();
            _highlightedOfferId = highlightOfferId;
            RefreshCounterModal();
            _counterModal.SetActive(true);
            RefreshHudVisibility();
            SetPlayerLocked(true);
        }

        public void ShowInspectPanel(ArtifactInstanceView artifact)
        {
            _counterModal.SetActive(false);
            _ledgerModal.SetActive(false);
            _inspectingArtifact = artifact;
            marketService?.RecordInspection(artifact.InstanceId);

            _insName.text = artifact.DisplayName;
            _insCategory.text = $"PHIẾU THẨM ĐỊNH · {artifact.Category.ToUpper()}";
            _insQuality.text = FormatQuality(artifact.Quality);
            _insQuality.color = artifact.Quality == QualityTier.Damaged ? ColWaxRed : (artifact.Quality == QualityTier.Pristine ? ColJade : ColInk);

            _insAuth.text = "Chưa giám định";
            _insAuth.color = ColInkMuted;

            _insRarity.text = FormatRarity(artifact.Rarity);
            _insRarity.color = GetInkRarityColor(artifact.Rarity);
            _insClue.text = artifact.InspectClueText;

            _insStampRing.color = ColInkMuted;
            _insStampText.color = ColInkMuted;
            _insStampText.text = "CHƯA\nRÕ";

            if (_insBasePrice != null)
            {
                _insBasePrice.text = $"Giá gốc: {artifact.BaseValue:N0} xu";
            }
            _insPrice.text = $"{artifact.CurrentSellPrice:N0} xu";

            Text sellBtnText = _insSellBtn.GetComponentInChildren<Text>();
            if (sellBtnText != null) sellBtnText.text = $"Bán ngay · niêm yết {artifact.CurrentSellPrice:N0} xu";

            _insSellBtn.onClick.RemoveAllListeners();
            _insSellBtn.onClick.AddListener(() =>
            {
                if (marketService != null && marketService.RequestSellArtifact(_inspectingArtifact.InstanceId))
                {
                    inspector3D.EndInspect();
                }
            });

            _insKeepBtn.onClick.RemoveAllListeners();
            _insKeepBtn.onClick.AddListener(() =>
            {
                if (marketService != null && marketService.RequestKeepArtifact(_inspectingArtifact.InstanceId))
                {
                    _inspectingArtifact.IsKeptInInventory = true;
                    inspector3D.EndInspect();
                }
            });

            _inspectModal.SetActive(true);
            if (_inspectHintsBar != null) _inspectHintsBar.SetActive(true);
            RefreshHudVisibility();
        }

        public void OpenLedgerModal(bool startOnDebtTab)
        {
            OpenLedgerTab(startOnDebtTab ? 3 : 0);
        }

        public void OpenTactShopModal()
        {
            ShowToast("Kho Tạo Tác sẽ mở ở Sprint 2.", isError: false);
        }

        private void OpenLedgerTab(int tabIndex)
        {
            CloseAllModals();
            RefreshAllLedgerTabs();
            _ledgerConfirmedDay = marketService != null ? marketService.CurrentRunState.CurrentDay : 0;
            _ledgerModal.SetActive(true);
            SwitchLedgerTab(tabIndex);
            ResetLedgerScrollPositions();
            RefreshHudVisibility();
            SetPlayerLocked(true);
        }

        public void CloseAllModals()
        {
            if (_counterModal != null) _counterModal.SetActive(false);
            if (_inspectModal != null) _inspectModal.SetActive(false);
            if (_inspectHintsBar != null) _inspectHintsBar.SetActive(false);
            if (_ledgerModal != null) _ledgerModal.SetActive(false);
            if (_runEndModal != null && (marketService == null || !marketService.CurrentRunState.IsRunEnded))
            {
                _runEndModal.SetActive(false);
            }

            if (inspector3D != null && inspector3D.IsInspecting)
            {
                inspector3D.EndInspect(hidePanel: false);
            }

            SetPlayerLocked(IsAnyModalOpen());
            RefreshHudVisibility();
            RefreshCarryBanner();
        }

        private void HandleCarriedBoxChanged(BoxInstanceView _)
        {
            RefreshCarryBanner();
            RefreshObjectives();
        }

        private void RefreshHudVisibility()
        {
            bool anyModal = IsAnyModalOpen();
            if (_hudRoot != null) _hudRoot.SetActive(!anyModal);
        }

        private void RefreshCarryBanner()
        {
            bool inspecting = inspector3D != null && inspector3D.IsInspecting;
            bool carrying = boxCarrier != null && boxCarrier.IsCarryingBox;
            bool anyModal = IsAnyModalOpen();
            bool showCarry = carrying && !anyModal && !inspecting;

            if (_carryBanner != null) _carryBanner.SetActive(showCarry);
            if (_compassBar != null) _compassBar.SetActive(showCarry);
            if (_objectiveCard != null) _objectiveCard.SetActive(!showCarry && !anyModal);

            if (showCarry && boxCarrier.CarriedBox != null)
            {
                if (_carryTitleText != null) _carryTitleText.text = boxCarrier.CarriedBox.DisplayName;
                if (_carrySubText != null) _carrySubText.text = "Đi chậm hơn · mang sang Bàn Khui ở Tiệm Mở Hòm";
            }
        }

        private void UpdateCompass()
        {
            if (_compassBar == null || !_compassBar.activeSelf || _compassMarkerRt == null) return;
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 targetPos = _dropZoneTarget != null ? _dropZoneTarget.position : new Vector3(-11.5f, 0f, 29.3f);
            Vector3 toTarget = targetPos - cam.transform.position;
            toTarget.y = 0f;
            float dist = toTarget.magnitude;

            if (_compassDistText != null)
            {
                _compassDistText.text = $"Bàn Khui · Tiệm Mở Hòm · {Mathf.RoundToInt(dist)}m";
            }

            Vector3 camFwd = cam.transform.forward;
            camFwd.y = 0f;
            if (camFwd.sqrMagnitude > 0.001f && toTarget.sqrMagnitude > 0.001f)
            {
                float signedAngle = Vector3.SignedAngle(camFwd.normalized, toTarget.normalized, Vector3.up);
                float norm = Mathf.Clamp(signedAngle / 90f, -1f, 1f);
                _compassMarkerRt.anchoredPosition = new Vector2(norm * 175f, -4f);
            }
        }

        private void LeaveArtifactOnTable()
        {
            ArtifactInstanceView art = _inspectingArtifact;
            if (inspector3D != null) inspector3D.EndInspect(hidePanel: false);
            CloseAllModals();
            if (openTableManager != null && art != null)
            {
                openTableManager.PlaceArtifactOnMat(art);
                ShowToast($"Đã đặt {art.DisplayName} lên thảm nhung. Nhìn vào món đồ và nhấn [E] để soi lại.", isError: false);
            }
        }

        private void SetPlayerLocked(bool locked)
        {
            if (playerController != null) playerController.SetInputLocked(locked);
            if (playerInteractor != null) playerInteractor.SetInteractionLocked(locked);
        }

        private void HandlePromptChanged(string prompt)
        {
            if (_promptPill == null || _promptText == null) return;
            bool show = !string.IsNullOrEmpty(prompt) && !IsAnyModalOpen();
            _promptPill.SetActive(show);
            _promptText.text = prompt;
        }

        private void HandleRunStateChanged(RunStateSnapshot s)
        {
            if (s == null) return;

            if (_dayNumText != null) _dayNumText.text = $"{s.CurrentDay}/{s.MaxDays}";
            if (_cashText != null) _cashText.text = $"{s.Cash:N0} xu";

            if (_lastCash >= 0 && _lastCash != s.Cash && _cashDeltaText != null)
            {
                int diff = s.Cash - _lastCash;
                _cashDeltaText.text = diff > 0 ? $"+{diff:N0}" : $"{diff:N0}";
                _cashDeltaText.color = diff > 0 ? ColJade : ColWaxRed;
                _cashDeltaText.gameObject.SetActive(true);
                _deltaTimer = 2.2f;
            }
            _lastCash = s.Cash;

            if (_debtText != null)
            {
                _debtText.text = s.IsDebtPaidToday
                    ? $"Đã trả ({s.DebtDueToday:N0} xu)"
                    : $"{s.DebtDueToday:N0} xu";
                _debtText.color = s.IsDebtPaidToday ? ColJade : ColCream;
            }
            if (_debtFillImg != null)
            {
                float ratio = s.IsDebtPaidToday ? 1f : (s.DebtDueToday > 0 ? Mathf.Clamp01((float)s.Cash / s.DebtDueToday) : 1f);
                _debtFillImg.fillAmount = ratio;
                _debtFillImg.color = s.IsDebtPaidToday ? ColJade : ColWaxRed;
            }

            if (_boxesText != null) _boxesText.text = $"{s.DailyBoxesPurchased}/{s.MaxDailyBoxes}";
            for (int i = 0; i < _boxPips.Length; i++)
            {
                if (_boxPips[i] == null) continue;
                bool used = i < s.DailyBoxesPurchased;
                _boxPips[i].color = used ? ColBrass : new Color(1f, 1f, 1f, 0.22f);
            }

            RefreshObjectives(s);

            if (_counterModal != null && _counterModal.activeSelf) RefreshCounterModal();
            if (_ledgerModal != null && _ledgerModal.activeSelf) RefreshAllLedgerTabs();

            if (s.IsRunEnded)
            {
                ShowRunEndScreen(s);
            }
        }

        private void RefreshObjectives(RunStateSnapshot s = null)
        {
            if (s == null && marketService != null) s = marketService.CurrentRunState;
            if (s == null || _objStep1 == null) return;

            bool carrying = boxCarrier != null && boxCarrier.IsCarryingBox;
            bool boughtAny = s.DailyBoxesPurchased > 0;

            _objStep1.text = boughtAny
                ? "✓  Đã mua hòm"
                : "◆  Mua hòm (Dì Mận)";
            _objStep1.color = boughtAny ? ColJade : ColCream;

            _objStep2.text = carrying
                ? "◆  Đem sang Tiệm Mở Hòm"
                : (boughtAny ? "✓  Đã sang Tiệm Mở Hòm" : "◇  Sang Tiệm Mở Hòm");
            _objStep2.color = carrying ? ColGold : (boughtAny ? ColJade : ColMuted);

            _objStep3.text = $"◇  Kết thúc ngày · Nợ {s.DebtDueToday:N0} xu [Tab]";
            _objStep3.color = ColCream;
        }

        private void HandleInventoryChanged()
        {
            if (_ledgerModal != null && _ledgerModal.activeSelf)
            {
                RefreshInventoryTab();
                RefreshDebtTab();
            }
        }

        private void HandleActionRejected(string code, string message)
        {
            ShowToast(message, isError: true);
        }

        private void HandleNotice(string message)
        {
            ShowToast(message, isError: false);
        }

        public void InspectDisplayOffer(string offerId, string displayName, int price)
        {
            _highlightedOfferId = offerId;
            string typeId = offerId switch
            {
                "offer_h1_wood" or "offer_h1_evening" => "H1",
                "offer_h2_metal" or "offer_h2_port" => "H2",
                "offer_h3_cloth" => "H3",
                "offer_rd_black" => "RD",
                _ => offerId
            };
            var offer = marketService?.CurrentOffers.FirstOrDefault(o => o.BoxTypeId == typeId);
            if (offer == null)
            {
                ShowToast("Hộp mẫu. Xem các hộp đang bán tại quầy Dì Mận.", isError: false);
                return;
            }
            _highlightedOfferId = offer.OfferId;
            string msg = $"[{offer.DisplayName} · {offer.Price:N0} xu] {offer.ClueText}";
            ShowToast(msg, isError: false);
        }

        private void ShowToast(string message, bool isError)
        {
            if (_toastBanner == null || _toastText == null || _toastBg == null) return;
            _toastText.text = message;
            _toastBg.color = isError ? ColWaxRed : ColWalnutLight;
            _toastBanner.SetActive(true);
            _toastTimer = 3.4f;
        }

        private void ShowRunEndScreen(RunStateSnapshot s)
        {
            CloseAllModals();
            _runEndTitle.text = s.IsVictory ? "HOÀN THÀNH 7 NGÀY!" : "NIÊM PHONG TIỆM";
            _runEndTitle.color = s.IsVictory ? ColJade : ColWaxRed;

            Color stCol = s.IsVictory ? ColJade : ColWaxRed;
            _runEndStampRing.color = stCol;
            _runEndStampText.color = stCol;
            _runEndStampText.text = s.IsVictory ? "ĐÃ TRẢ\nĐỦ NỢ" : "VỠ\nNỢ";

            string reason = string.IsNullOrEmpty(s.EndReason) ? "Ván chơi đã kết thúc." : s.EndReason;
            _runEndBody.text =
                $"{reason}\n\nNgày {s.CurrentDay}/{s.MaxDays}\n" +
                $"Tiền mặt: {s.Cash:N0} xu\nNợ tồn: {s.Arrears:N0} xu";

            RefreshRunEndSkills();
            _runEndModal.SetActive(true);
            RefreshHudVisibility();
            SetPlayerLocked(true);
        }

        private BoxOfferView GetSelectedOffer()
        {
            var offers = marketService?.CurrentOffers;
            return offers?.FirstOrDefault(o => o.OfferId == _highlightedOfferId) ?? offers?.FirstOrDefault();
        }

        private void TryBuyHighlightedOffer()
        {
            BoxOfferView sel = GetSelectedOffer();
            if (sel == null) return;
            if (boxCarrier != null && boxCarrier.IsCarryingBox)
            {
                ShowToast("Hai tay bạn đang ôm hộp! Hãy mang sang Tiệm Mở Hòm bên cạnh đặt lên bàn trước khi mua thêm.", isError: true);
                return;
            }
            if (marketService != null && marketService.RequestBuyBox(sel.OfferId))
            {
                CloseAllModals();
            }
        }

        private void ShowSelectedOfferClue()
        {
            BoxOfferView sel = GetSelectedOffer();
            if (sel == null) return;
            if (string.IsNullOrEmpty(sel.CertainOrigin))
                marketService.RequestBuyClue(sel.OfferId);
            else
                ShowToast($"Dì Mận mách: {sel.CertainOrigin}", isError: false);
        }

        private void RefreshCounterModal()
        {
            ClearChildren(_counterListRoot);
            if (marketService == null) return;

            RunStateSnapshot s = marketService.CurrentRunState;
            if (_counterWalletText != null) _counterWalletText.text = $"Ví: {s.Cash:N0} xu";
            if (_counterQuotaText != null)
            {
                var today = string.Join(" · ", marketService.TodayPrices.Select(entry => $"{entry.Key} {entry.Value:N0} xu"));
                _counterQuotaText.text = $"GIÁ HÔM NAY · {today}\n{s.DailyBoxesPurchased}/{s.MaxDailyBoxes} lượt đã mua";
            }

            BoxOfferView selected = GetSelectedOffer();
            if (selected != null) _highlightedOfferId = selected.OfferId;

            foreach (BoxOfferView offer in marketService.CurrentOffers)
            {
                bool isSel = offer.OfferId == _highlightedOfferId;
                Color rimCol = isSel ? ColGold : ColBrassDark;
                Color fillCol = isSel ? ColPaperHighlight : ColPaper;

                GameObject tag = CreateChamferedPanel(_counterListRoot, $"Offer_{offer.OfferId}", fillCol, rimCol, 0f, 128f, small: false);
                Button tagBtn = tag.AddComponent<Button>();
                tagBtn.targetGraphic = tag.GetComponent<Image>();
                string capturedId = offer.OfferId;
                tagBtn.onClick.AddListener(() =>
                {
                    _highlightedOfferId = capturedId;
                    RefreshCounterModal();
                });

                HorizontalLayoutGroup h = tag.AddComponent<HorizontalLayoutGroup>();
                h.padding = new RectOffset(18, 18, 14, 14);
                h.spacing = 16f;
                h.childAlignment = TextAnchor.MiddleLeft;
                h.childControlWidth = true;
                h.childControlHeight = true;
                h.childForceExpandWidth = false;

                Color badgeCol = offer.BoxTypeId == "H2" ? new Color(0.28f, 0.34f, 0.40f)
                    : (offer.BoxTypeId == "H3" ? new Color(0.18f, 0.42f, 0.36f)
                    : (offer.BoxTypeId == "RD" ? ColPurple : new Color(0.58f, 0.38f, 0.21f)));
                GameObject icon = CreateChamferedPanel(tag.transform, "CrateIcon", badgeCol, ColBrass, 64f, 64f, small: true);
                CreateText(icon.transform, offer.BoxTypeId, 19, FontStyle.Bold, ColCream).alignment = TextAnchor.MiddleCenter;

                GameObject info = CreateInfoColumn(tag.transform);
                CreateText(info.transform, offer.DisplayName, 19, FontStyle.Bold, ColInk);
                CreateText(info.transform, $"Giá hôm nay {offer.TodayPrice:N0} xu · Hàng giả {offer.FakeRate:P0}", 14, FontStyle.Bold, ColInkMuted);
                CreateText(info.transform, $"Niêm yết {offer.Price:N0} xu", 21, FontStyle.Bold, ColWaxRed);
            }

            if (_dialogueQuoteText != null && selected != null)
            {
                _dialogueQuoteText.text = $"\"{selected.DisplayName} ({selected.Price:N0} xu) — {selected.ClueText}\"" +
                    (string.IsNullOrEmpty(selected.CertainOrigin) ? "" : $" · Lời mách: {selected.CertainOrigin}");
            }

            if (_dialogueQuoteText != null && selected == null) _dialogueQuoteText.text = "Hết lượt mua hôm nay.";
            if (_tipButton != null)
            {
                _tipButton.interactable = selected != null &&
                    (s.Cash >= marketService.ExtraCluePrice || !string.IsNullOrEmpty(selected.CertainOrigin));
                _tipButton.GetComponentInChildren<Text>().text = string.IsNullOrEmpty(selected?.CertainOrigin)
                    ? $"2 · Lời mách ({marketService.ExtraCluePrice} xu)" : "2 · Xem lời mách";
            }
            if (_dialogueBuyBtn != null) _dialogueBuyBtn.interactable = selected != null;
            if (_dialogueBuyBtn != null && selected != null)
            {
                bool canAfford = s.Cash >= selected.Price && s.DailyBoxesPurchased < s.MaxDailyBoxes;
                _dialogueBuyBtn.interactable = canAfford;
                Image bImg = _dialogueBuyBtn.GetComponent<Image>();
                if (bImg != null) bImg.color = canAfford ? ColJade : ColDisabled;
                Text bt = _dialogueBuyBtn.GetComponentInChildren<Text>();
                if (bt != null) bt.text = $"1 · Mua ({selected.Price:N0} xu)";
            }
        }

        private void RefreshAllLedgerTabs()
        {
            RefreshInventoryTab();
            RefreshTactTab();
            RefreshBikheTab();
            RefreshDebtTab();
        }

        private void ResetLedgerScrollPositions()
        {
            Canvas.ForceUpdateCanvases();
            if (_invScroll != null) _invScroll.verticalNormalizedPosition = 1f;
            if (_tactScroll != null) _tactScroll.verticalNormalizedPosition = 1f;
            if (_bikheScroll != null) _bikheScroll.verticalNormalizedPosition = 1f;
        }

        private void RefreshInventoryTab()
        {
            ClearChildren(_invListRoot);
            if (marketService == null) return;

            RunStateSnapshot s = marketService.CurrentRunState;
            IReadOnlyList<ArtifactInstanceView> items = marketService.InventoryItems;
            AddSectionBanner(_invListRoot, $"KHO ĐỒ ({items.Count} món  ·  Tổng giá trị bán: {s.InventoryTotalValue:N0} xu)");

            if (items.Count == 0)
            {
                CreateText(_invListRoot, "Kho hiện vật đang trống. Khi mở hộp, hãy chọn 'Cất vào kho' để giữ lại chờ giá tốt.", 15, FontStyle.Italic, ColInkMuted);
                RebuildScrollLayout(_invListRoot);
                return;
            }

            foreach (ArtifactInstanceView item in items)
            {
                GameObject row = CreateChamferedPanel(_invListRoot, $"Inv_{item.InstanceId}", ColPaperDark, ColBrassDark, 0f, 74f, small: true);
                HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
                h.padding = new RectOffset(14, 14, 8, 8);
                h.spacing = 10f;
                h.childControlWidth = true;
                h.childControlHeight = true;
                h.childForceExpandWidth = false;

                GameObject info = CreateInfoColumn(row.transform);
                CreateText(info.transform, $"{item.DisplayName} ({item.Category})", 16, FontStyle.Bold, ColInk);
                CreateText(info.transform, $"{FormatQuality(item.Quality)} · {FormatAuth(item.Authenticity)} · {FormatRarity(item.Rarity)}", 13, FontStyle.Normal, ColInkMuted);

                string capturedId = item.InstanceId;
                CreateButton(row.transform, $"Bán (niêm yết {item.CurrentSellPrice:N0} xu)", ColJade, 175f, 44f, () =>
                {
                    marketService.RequestSellArtifact(capturedId);
                });
            }

            RebuildScrollLayout(_invListRoot);
        }

        private void RefreshTactTab()
        {
            ClearChildren(_tactListRoot);
            if (marketService == null) return;

            RunStateSnapshot s = marketService.CurrentRunState;
            RenderHeirloomSection(s);
            RenderEquipSlotsSection(s);
            RenderCombosSection(s);
            RenderTactCatalogSection(s);
            RebuildScrollLayout(_tactListRoot);
        }

        private void RenderHeirloomSection(RunStateSnapshot s)
        {
            bool canSwitch = s.CurrentDay == 1 && s.DailyBoxesPurchased == 0;
            string statusNote = marketService.ActiveHeirloom != null
                ? $"Đang mang: {marketService.ActiveHeirloom.DisplayName}"
                : (canSwitch ? "Hãy chọn 1 Gia Bảo trước khi mua hộp" : "Chưa chọn (hết hạn đầu Ngày 1)");
            AddSectionBanner(_tactListRoot, $"1. CỔ VẬT GIA TRUYỀN — {statusNote}");

            foreach (HeirloomView h in marketService.AvailableHeirlooms)
            {
                bool isSelected = marketService.ActiveHeirloom != null && marketService.ActiveHeirloom.HeirloomId == h.HeirloomId;
                GameObject card = CreateAutoCard(_tactListRoot, $"Heirloom_{h.HeirloomId}", isSelected ? ColPaperHighlight : ColPaperDark, 72f);
                HorizontalLayoutGroup row = card.AddComponent<HorizontalLayoutGroup>();
                row.padding = new RectOffset(12, 12, 8, 8);
                row.spacing = 10f;
                row.childAlignment = TextAnchor.MiddleLeft;
                row.childControlWidth = true;
                row.childControlHeight = true;
                row.childForceExpandWidth = false;

                GameObject info = CreateInfoColumn(row.transform);
                CreateText(info.transform, $"{h.DisplayName}  [{FormatSystemName(h.PreferredSystem)}]", 15, FontStyle.Bold, isSelected ? ColJade : ColInk);
                CreateText(info.transform, h.EffectSummary, 13, FontStyle.Normal, ColInk);
                CreateText(info.transform, h.LoreDesc, 12, FontStyle.Italic, ColInkMuted);

                string capturedId = h.HeirloomId;
                string btnLabel = isSelected ? "Đang Mang" : (canSwitch ? "Chọn Gia Bảo" : "Đã Khóa");
                Color btnCol = isSelected ? ColJade : (canSwitch ? ColBrassDark : ColDisabled);
                Button btn = CreateButton(card.transform, btnLabel, btnCol, 145f, 44f, () =>
                {
                    marketService.RequestSelectHeirloom(capturedId);
                });
                btn.interactable = !isSelected;
            }
        }

        private void RenderEquipSlotsSection(RunStateSnapshot s)
        {
            AddSectionBanner(_tactListRoot, $"2. Ô GẮN TẠO TÁC ({s.EquippedTactCount}/{s.MaxEquipSlots} đang kích hoạt)");

            GameObject slotRow = new GameObject("EquipSlotsRow");
            slotRow.transform.SetParent(_tactListRoot, false);
            slotRow.AddComponent<LayoutElement>().minHeight = 84f;
            HorizontalLayoutGroup h = slotRow.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 10f;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = true;

            IReadOnlyList<TactArtifactView> slots = marketService.EquippedArtifacts;
            int slotCount = Mathf.Max(s.MaxEquipSlots, slots != null ? slots.Count : 0);
            for (int i = 0; i < slotCount; i++)
            {
                TactArtifactView equipped = slots != null && i < slots.Count ? slots[i] : null;
                Color bg = equipped != null ? ColPaperHighlight : ColPaperDark;
                GameObject slotCard = CreateChamferedPanel(slotRow.transform, $"Slot_{i}", bg, equipped != null ? ColTeal : ColBrassDark, 0f, 84f, small: true);
                VerticalLayoutGroup sv = slotCard.AddComponent<VerticalLayoutGroup>();
                sv.padding = new RectOffset(10, 10, 8, 8);
                sv.spacing = 4f;
                sv.childControlWidth = true;
                sv.childControlHeight = true;
                sv.childForceExpandHeight = false;

                if (equipped == null)
                {
                    CreateText(slotCard.transform, $"Ô {i + 1}: TRỐNG", 14, FontStyle.Bold, ColInkMuted);
                    CreateText(slotCard.transform, "Mua hoặc bấm 'Trang Bị' bên dưới.", 12, FontStyle.Italic, ColInkMuted);
                }
                else
                {
                    CreateText(slotCard.transform, $"Ô {i + 1}: {equipped.DisplayName}", 14, FontStyle.Bold, ColInk);
                    CreateText(slotCard.transform, $"{FormatSystemName(equipped.SystemType)} · Bậc {equipped.Tier}", 12, FontStyle.Normal, ColTeal);
                    int capturedSlot = i;
                    CreateButton(slotCard.transform, "Tháo Ra", ColWaxRed, 0f, 28f, () =>
                    {
                        marketService.RequestUnequipTactArtifact(capturedSlot);
                    });
                }
            }
        }

        private void RenderCombosSection(RunStateSnapshot s)
        {
            IReadOnlyList<ComboView> combos = marketService.ActiveCombos;
            AddSectionBanner(_tactListRoot, $"3. TỔ HỢP CỘNG HƯỞNG ({s.ActiveComboCount}/{combos.Count} đang bật)");

            foreach (ComboView c in combos)
            {
                GameObject card = CreateAutoCard(_tactListRoot, $"Combo_{c.ComboId}", c.IsActive ? ColPaperHighlight : ColPaperDark, 58f);
                HorizontalLayoutGroup row = card.AddComponent<HorizontalLayoutGroup>();
                row.padding = new RectOffset(12, 12, 8, 8);
                row.spacing = 10f;
                row.childAlignment = TextAnchor.MiddleLeft;
                row.childControlWidth = true;
                row.childControlHeight = true;
                row.childForceExpandWidth = false;

                GameObject info = CreateInfoColumn(row.transform);
                string badge = c.IsActive ? "[ĐÃ KÍCH HOẠT]" : "[Chưa đủ ĐK]";
                Color titleCol = c.IsActive ? ColJade : ColInkMuted;
                CreateText(info.transform, $"{badge} {c.DisplayName}  ({c.ConditionSummary})", 14, FontStyle.Bold, titleCol);
                CreateText(info.transform, c.EffectSummary, 13, FontStyle.Normal, c.IsActive ? ColInk : ColInkMuted);
            }
        }

        private void RenderTactCatalogSection(RunStateSnapshot s)
        {
            AddSectionBanner(_tactListRoot, "4. KHO TẠO TÁC KIM KHUY (Mua & Trang Bị)");

            foreach (TactArtifactView t in marketService.TactCatalog)
            {
                GameObject card = CreateAutoCard(_tactListRoot, $"Tact_{t.TactId}", t.IsEquipped ? ColPaperHighlight : ColPaperDark, 72f);
                HorizontalLayoutGroup row = card.AddComponent<HorizontalLayoutGroup>();
                row.padding = new RectOffset(12, 12, 8, 8);
                row.spacing = 10f;
                row.childAlignment = TextAnchor.MiddleLeft;
                row.childControlWidth = true;
                row.childControlHeight = true;
                row.childForceExpandWidth = false;

                GameObject info = CreateInfoColumn(row.transform);
                CreateText(info.transform, $"{t.DisplayName}  [{FormatSystemName(t.SystemType)} · Bậc {t.Tier}]", 15, FontStyle.Bold, ColInk);
                CreateText(info.transform, t.EffectSummary, 13, FontStyle.Normal, ColInkMuted);

                string capturedTactId = t.TactId;
                if (!t.IsOwned)
                {
                    bool canAfford = s.Cash >= t.Price;
                    CreateButton(card.transform, $"Mua ({t.Price} xu)", canAfford ? ColJade : ColDisabled, 155f, 44f, () =>
                    {
                        marketService.RequestBuyTactArtifact(capturedTactId);
                    });
                }
                else if (t.IsEquipped)
                {
                    int capturedSlot = t.EquippedSlotIndex;
                    CreateButton(card.transform, $"Tháo (Ô {capturedSlot + 1})", ColAmber, 155f, 44f, () =>
                    {
                        marketService.RequestUnequipTactArtifact(capturedSlot);
                    });
                }
                else
                {
                    CreateButton(card.transform, "Trang Bị", ColTeal, 155f, 44f, () =>
                    {
                        marketService.RequestEquipTactArtifact(capturedTactId);
                    });
                }
            }
        }

        private void RefreshBikheTab() => RenderSkillTree(_bikheListRoot);

        private void RefreshRunEndSkills()
        {
            RenderSkillTree(_runEndSkillRoot);
            _earlyChestButton.gameObject.SetActive(marketService != null && marketService.CanStartWithEarlyBlackChest);
        }

        private void RenderSkillTree(Transform root)
        {
            if (root == null || marketService == null) return;
            ClearChildren(root);
            var nodes = marketService.SkillNodes;
            CreateText(root, $"Điểm phát triển: {marketService.DevelopmentPoints}", 19, FontStyle.Bold, ColInk);
            if (!marketService.CurrentRunState.IsRunEnded)
                CreateText(root, "Mở nút sau khi kết thúc ván.", 14, FontStyle.Normal, ColInkMuted);

            foreach (var branch in nodes.GroupBy(node => node.Branch))
            {
                CreateText(root, branch.Key, 18, FontStyle.Bold, ColWaxRed);
                foreach (var node in branch)
                {
                    string state = node.Unlocked ? "Đã mở" : $"{node.Cost} điểm";
                    CreateText(root, $"{node.Name} · {state}", 16, FontStyle.Bold, ColInk);
                    if (node.Parent != null)
                    {
                        var prerequisite = nodes.First(parent => parent.Id == node.Parent);
                        CreateText(root, $"Sau {prerequisite.Name}", 14, FontStyle.Normal, ColInkMuted);
                    }

                    CreateText(root, $"{node.Condition} ({Mathf.Min(node.Progress, node.Target)}/{node.Target})",
                        14, FontStyle.Normal, ColInkMuted);
                    if (node.Unlocked)
                        CreateText(root, node.Reward, 14, FontStyle.Normal, ColInk);
                    else if (node.Available)
                        CreateButton(root, "Mở nút", ColJade, 125f, 35f, () =>
                        {
                            if (marketService.UnlockSkill(node.Id)) RefreshRunEndSkills();
                        });
                }
            }

            if (nodes.Any(node => node.Id == "S01" && node.Unlocked))
            {
                CreateText(root, "Món đã soi", 18, FontStyle.Bold, ColWaxRed);
                foreach (var name in marketService.CollectedItems)
                    CreateText(root, name, 14, FontStyle.Normal, ColInk);
            }
        }

        private void RefreshDebtTab()
        {
            ClearChildren(_debtListRoot);
            if (marketService == null) return;

            RunStateSnapshot s = marketService.CurrentRunState;
            int[] schedule = marketService.DebtSchedule;
            _debtSummaryText.text = $"NGÀY {s.CurrentDay}: CẦN {s.DebtDueToday:N0} xu\n" +
                $"Nợ tồn: {s.Arrears:N0} xu · Ví: {s.Cash:N0} xu";

            for (int i = 0; i < schedule.Length; i++)
            {
                int dayNum = i + 1;
                bool isToday = dayNum == s.CurrentDay;
                bool isPast = dayNum < s.CurrentDay || (isToday && s.IsDebtPaidToday);

                string status = isPast ? "Đã qua kỳ" : (isToday ? "HÔM NAY" : "Chưa tới");
                Color col = isPast ? ColJade : (isToday ? ColWaxRed : ColInkMuted);

                GameObject row = CreateChamferedPanel(_debtListRoot, $"DebtDay_{dayNum}", isToday ? ColPaperHighlight : ColPaperDark, isToday ? ColWaxRed : ColBrassDark, 0f, 54f, small: true);
                HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
                h.padding = new RectOffset(18, 18, 6, 6);
                h.childAlignment = TextAnchor.MiddleLeft;
                CreateText(row.transform, isToday ? $"Ngày {dayNum} (Nay)" : $"Ngày {dayNum}", 17, isToday ? FontStyle.Bold : FontStyle.Normal, ColInk);
                int displayedDue = isToday ? s.DebtDueToday : schedule[i];
                CreateText(row.transform, $"{displayedDue:N0} xu", 17, FontStyle.Bold, ColInk);
                Text stT = CreateText(row.transform, status, 16, FontStyle.Bold, col);
                stT.alignment = TextAnchor.MiddleRight;
            }

            _endDayBtn.interactable = !s.IsRunEnded;
        }

        private void SwitchLedgerTab(int tabIndex)
        {
            _activeLedgerTab = tabIndex;
            _invTabContent.SetActive(tabIndex == 0);
            _tactTabContent.SetActive(tabIndex == 1);
            _bikheTabContent.SetActive(tabIndex == 2);
            _debtTabContent.SetActive(tabIndex == 3);

            SetRibbonOffset(_tabInvRt, tabIndex == 0);
            SetRibbonOffset(_tabTactRt, tabIndex == 1);
            SetRibbonOffset(_tabBikheRt, tabIndex == 2);
            SetRibbonOffset(_tabDebtRt, tabIndex == 3);

            if (tabIndex == 0) RebuildScrollLayout(_invListRoot);
            else if (tabIndex == 1) RebuildScrollLayout(_tactListRoot);
            else if (tabIndex == 2) RebuildScrollLayout(_bikheListRoot);
        }

        private static void SetRibbonOffset(RectTransform rt, bool active)
        {
            if (rt == null) return;
            Vector2 pos = rt.anchoredPosition;
            pos.x = active ? 14f : 0f;
            rt.anchoredPosition = pos;
        }

        // ==================== UI BUILDER HELPERS ====================

        private void BuildUiHierarchy()
        {
            GameObject canvasGo = new GameObject("Sprint1_HUD_Canvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _hudRoot = new GameObject("HUD_Root");
            _hudRoot.transform.SetParent(canvasGo.transform, false);
            StretchFull(_hudRoot);

            BuildStatusCluster(_hudRoot.transform);
            BuildObjectiveCard(_hudRoot.transform);
            BuildCrosshairAndPrompt(_hudRoot.transform);
            BuildCompassAndCarryCard(_hudRoot.transform);
            BuildBottomHints(_hudRoot.transform);

            BuildToastBanner(canvasGo.transform);
            BuildCounterModal(canvasGo.transform);
            BuildInspectModal(canvasGo.transform);
            BuildLedgerModal(canvasGo.transform);
            BuildRunEndModal(canvasGo.transform);
        }

        private void BuildStatusCluster(Transform parent)
        {
            GameObject cluster = new GameObject("StatusCluster");
            cluster.transform.SetParent(parent, false);
            RectTransform rt = cluster.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(28f, -24f);
            rt.sizeDelta = new Vector2(440f, 175f);

            // Hexagon Day Badge
            GameObject hexRim = new GameObject("DayHexRim");
            hexRim.transform.SetParent(cluster.transform, false);
            Image rimImg = hexRim.AddComponent<Image>();
            rimImg.sprite = LowPolyUiSprites.Hexagon;
            rimImg.color = ColBrass;
            RectTransform hrt = hexRim.GetComponent<RectTransform>();
            hrt.anchorMin = hrt.anchorMax = new Vector2(0f, 1f);
            hrt.pivot = new Vector2(0f, 1f);
            hrt.anchoredPosition = Vector2.zero;
            hrt.sizeDelta = new Vector2(106f, 94f);

            GameObject hexInner = new GameObject("DayHexInner");
            hexInner.transform.SetParent(hexRim.transform, false);
            Image innerImg = hexInner.AddComponent<Image>();
            innerImg.sprite = LowPolyUiSprites.Hexagon;
            innerImg.color = ColWalnut;
            RectTransform irt = hexInner.GetComponent<RectTransform>();
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(3f, 3f);
            irt.offsetMax = new Vector2(-3f, -3f);

            Text dayLbl = CreateText(hexInner.transform, "NGÀY", 13, FontStyle.Bold, ColBrass);
            dayLbl.alignment = TextAnchor.MiddleCenter;
            RectTransform dlrt = dayLbl.GetComponent<RectTransform>();
            dlrt.anchorMin = new Vector2(0f, 0.58f);
            dlrt.anchorMax = new Vector2(1f, 0.90f);
            dlrt.offsetMin = dlrt.offsetMax = Vector2.zero;

            _dayNumText = CreateText(hexInner.transform, "1/7", 30, FontStyle.Bold, ColCream);
            _dayNumText.alignment = TextAnchor.MiddleCenter;
            RectTransform dnrt = _dayNumText.GetComponent<RectTransform>();
            dnrt.anchorMin = new Vector2(0f, 0.12f);
            dnrt.anchorMax = new Vector2(1f, 0.62f);
            dnrt.offsetMin = dnrt.offsetMax = Vector2.zero;

            // Right Stack: Cash + Debt + Quota Pips
            GameObject stack = new GameObject("RightStack");
            stack.transform.SetParent(cluster.transform, false);
            RectTransform srt = stack.AddComponent<RectTransform>();
            srt.anchorMin = srt.anchorMax = new Vector2(0f, 1f);
            srt.pivot = new Vector2(0f, 1f);
            srt.anchoredPosition = new Vector2(118f, -2f);
            srt.sizeDelta = new Vector2(305f, 165f);
            VerticalLayoutGroup sv = stack.AddComponent<VerticalLayoutGroup>();
            sv.spacing = 8f;
            sv.childControlWidth = true;
            sv.childControlHeight = true;
            sv.childForceExpandWidth = false;
            sv.childForceExpandHeight = false;

            // Cash pill
            GameObject cashPill = CreateChamferedPanel(stack.transform, "CashPill", ColWalnut, ColBrass, 220f, 44f, small: true);
            HorizontalLayoutGroup ch = cashPill.AddComponent<HorizontalLayoutGroup>();
            ch.padding = new RectOffset(12, 14, 6, 6);
            ch.spacing = 10f;
            ch.childAlignment = TextAnchor.MiddleLeft;
            ch.childControlWidth = true;
            ch.childControlHeight = true;
            ch.childForceExpandWidth = false;

            GameObject coinGo = new GameObject("Coin");
            coinGo.transform.SetParent(cashPill.transform, false);
            Image coinImg = coinGo.AddComponent<Image>();
            coinImg.sprite = LowPolyUiSprites.Circle;
            coinImg.color = ColGold;
            LayoutElement cle = coinGo.AddComponent<LayoutElement>();
            cle.preferredWidth = 22f;
            cle.preferredHeight = 22f;

            _cashText = CreateSingleLineText(cashPill.transform, "900 xu", 22, FontStyle.Bold, ColCream);
            _cashDeltaText = CreateSingleLineText(cashPill.transform, "+0", 16, FontStyle.Bold, ColJade);
            _cashDeltaText.gameObject.SetActive(false);

            // Debt pill with progress bar
            GameObject debtPill = CreateChamferedPanel(stack.transform, "DebtPill", ColWalnut, ColBrassDark, 290f, 52f, small: true);
            VerticalLayoutGroup dv = debtPill.AddComponent<VerticalLayoutGroup>();
            dv.padding = new RectOffset(12, 12, 6, 7);
            dv.spacing = 5f;
            dv.childControlWidth = true;
            dv.childControlHeight = true;
            dv.childForceExpandHeight = false;

            GameObject dRow = new GameObject("DebtRow");
            dRow.transform.SetParent(debtPill.transform, false);
            dRow.AddComponent<LayoutElement>().preferredHeight = 22f;
            HorizontalLayoutGroup drh = dRow.AddComponent<HorizontalLayoutGroup>();
            drh.childControlWidth = true;
            drh.childControlHeight = true;
            drh.childForceExpandWidth = false;
            Text dLbl = CreateText(dRow.transform, "Nợ hôm nay", 15, FontStyle.Bold, new Color(0.94f, 0.56f, 0.50f));
            dLbl.GetComponent<LayoutElement>().flexibleWidth = 1f;
            _debtText = CreateSingleLineText(dRow.transform, "200 xu", 16, FontStyle.Bold, ColCream);

            GameObject barBg = new GameObject("DebtBarBg");
            barBg.transform.SetParent(debtPill.transform, false);
            barBg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
            barBg.AddComponent<LayoutElement>().preferredHeight = 8f;

            GameObject barFill = new GameObject("DebtBarFill");
            barFill.transform.SetParent(barBg.transform, false);
            _debtFillImg = barFill.AddComponent<Image>();
            _debtFillImg.type = Image.Type.Filled;
            _debtFillImg.fillMethod = Image.FillMethod.Horizontal;
            _debtFillImg.color = ColWaxRed;
            StretchFull(barFill);

            // Quota row: Hòm + 3 hex pips
            GameObject qRow = CreateChamferedPanel(stack.transform, "QuotaRow", ColWalnut, ColBrassDark, 225f, 34f, small: true);
            HorizontalLayoutGroup qh = qRow.AddComponent<HorizontalLayoutGroup>();
            qh.padding = new RectOffset(12, 12, 4, 4);
            qh.spacing = 8f;
            qh.childAlignment = TextAnchor.MiddleLeft;
            qh.childControlWidth = true;
            qh.childControlHeight = true;
            qh.childForceExpandWidth = false;

            Text qLbl = CreateSingleLineText(qRow.transform, "Hòm", 15, FontStyle.Bold, ColCream);
            AddTextShadow(qLbl.gameObject);
            for (int i = 0; i < _boxPips.Length; i++)
            {
                GameObject pip = new GameObject($"Pip_{i}");
                pip.transform.SetParent(qRow.transform, false);
                Image pImg = pip.AddComponent<Image>();
                pImg.sprite = LowPolyUiSprites.Hexagon;
                pImg.color = new Color(1f, 1f, 1f, 0.22f);
                LayoutElement ple = pip.AddComponent<LayoutElement>();
                ple.preferredWidth = 19f;
                ple.preferredHeight = 17f;
                _boxPips[i] = pImg;
            }
            _boxesText = CreateSingleLineText(qRow.transform, "0/3", 15, FontStyle.Bold, ColCream);
            AddTextShadow(_boxesText.gameObject);
        }

        private void BuildObjectiveCard(Transform parent)
        {
            _objectiveCard = CreateChamferedPanel(parent, "ObjectiveCard", ColWalnut, ColBrass, 345f, 148f, small: false);
            RectTransform rt = _objectiveCard.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-28f, -24f);
            rt.sizeDelta = new Vector2(345f, 148f);

            VerticalLayoutGroup v = _objectiveCard.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(18, 18, 14, 14);
            v.spacing = 7f;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;

            CreateText(_objectiveCard.transform, "MỤC TIÊU", 13, FontStyle.Bold, ColBrass);
            _objStep1 = CreateText(_objectiveCard.transform, "◆  Mua hòm (Dì Mận)", 16, FontStyle.Bold, ColCream);
            _objStep2 = CreateText(_objectiveCard.transform, "◇  Sang Tiệm Mở Hòm", 16, FontStyle.Normal, ColMuted);
            _objStep3 = CreateText(_objectiveCard.transform, "◇  Trả nợ 200 xu [Tab]", 16, FontStyle.Normal, ColMuted);
        }

        private void BuildCrosshairAndPrompt(Transform parent)
        {
            GameObject dot = new GameObject("Crosshair");
            dot.transform.SetParent(parent, false);
            Image dotImg = dot.AddComponent<Image>();
            dotImg.sprite = LowPolyUiSprites.Circle;
            dotImg.color = new Color(1f, 0.96f, 0.85f, 0.92f);
            RectTransform dotRt = dot.GetComponent<RectTransform>();
            dotRt.anchorMin = dotRt.anchorMax = new Vector2(0.5f, 0.5f);
            dotRt.sizeDelta = new Vector2(9f, 9f);
            AddOutline(dot, new Color(0.15f, 0.08f, 0.04f, 0.8f));

            _promptPill = CreateChamferedPanel(parent, "InteractPrompt", new Color(0.169f, 0.106f, 0.067f, 0.86f), ColBrass, 560f, 44f, small: true);
            RectTransform rt = _promptPill.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.40f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(560f, 44f);

            HorizontalLayoutGroup h = _promptPill.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(12, 16, 6, 6);
            h.spacing = 10f;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;

            CreateHexKeycap(_promptPill.transform, "E", 30f);
            _promptText = CreateSingleLineText(_promptPill.transform, "Tương tác", 17, FontStyle.Bold, ColCream);
            AddTextShadow(_promptText.gameObject);
            _promptPill.SetActive(false);
        }

        private void BuildCompassAndCarryCard(Transform parent)
        {
            // Top-center Compass Bar
            _compassBar = new GameObject("CarryCompass");
            _compassBar.transform.SetParent(parent, false);
            RectTransform crt = _compassBar.AddComponent<RectTransform>();
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.anchoredPosition = new Vector2(0f, -20f);
            crt.sizeDelta = new Vector2(440f, 64f);

            GameObject strip = CreateChamferedPanel(_compassBar.transform, "Strip", ColWalnut, ColBrass, 440f, 36f, small: true);
            RectTransform srt = strip.GetComponent<RectTransform>();
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 1f);
            srt.pivot = new Vector2(0.5f, 1f);
            srt.anchoredPosition = Vector2.zero;
            srt.sizeDelta = new Vector2(440f, 36f);

            Text ticks = CreateText(strip.transform, "B     |     |     T     |     |     N     |     |     Đ", 12, FontStyle.Bold, ColMuted);
            ticks.alignment = TextAnchor.MiddleCenter;
            StretchFull(ticks.gameObject);

            GameObject marker = new GameObject("Marker");
            marker.transform.SetParent(strip.transform, false);
            Text mTxt = marker.AddComponent<Text>();
            mTxt.font = _font;
            mTxt.text = "▼";
            mTxt.fontSize = 15;
            mTxt.fontStyle = FontStyle.Bold;
            mTxt.color = ColGold;
            mTxt.alignment = TextAnchor.UpperCenter;
            _compassMarkerRt = marker.GetComponent<RectTransform>();
            _compassMarkerRt.anchorMin = _compassMarkerRt.anchorMax = new Vector2(0.5f, 1f);
            _compassMarkerRt.sizeDelta = new Vector2(24f, 20f);
            _compassMarkerRt.anchoredPosition = new Vector2(0f, -2f);

            _compassDistText = CreateText(_compassBar.transform, "Bàn Khui · Tiệm Mở Hòm · 5m", 14, FontStyle.Bold, ColGold);
            _compassDistText.alignment = TextAnchor.MiddleCenter;
            AddTextShadow(_compassDistText.gameObject);
            RectTransform drt = _compassDistText.GetComponent<RectTransform>();
            drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0f);
            drt.pivot = new Vector2(0.5f, 0f);
            drt.anchoredPosition = Vector2.zero;
            drt.sizeDelta = new Vector2(440f, 24f);
            _compassBar.SetActive(false);

            // Bottom-center Carry Card
            _carryBanner = CreateChamferedPanel(parent, "CarryBanner", ColWalnut, ColBrass, 480f, 88f, small: false);
            RectTransform brt = _carryBanner.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.anchoredPosition = new Vector2(0f, 26f);
            brt.sizeDelta = new Vector2(480f, 88f);

            HorizontalLayoutGroup bh = _carryBanner.AddComponent<HorizontalLayoutGroup>();
            bh.padding = new RectOffset(18, 18, 12, 12);
            bh.spacing = 16f;
            bh.childAlignment = TextAnchor.MiddleLeft;
            bh.childControlWidth = true;
            bh.childControlHeight = true;
            bh.childForceExpandWidth = false;

            GameObject crateIcon = CreateChamferedPanel(_carryBanner.transform, "CarryIcon", new Color(0.58f, 0.38f, 0.21f), ColBrass, 56f, 52f, small: true);
            CreateText(crateIcon.transform, "HÒM", 14, FontStyle.Bold, ColCream).alignment = TextAnchor.MiddleCenter;

            GameObject cInfo = CreateInfoColumn(_carryBanner.transform);
            CreateText(cInfo.transform, "ĐANG ÔM HÒM", 12, FontStyle.Bold, ColBrass);
            _carryTitleText = CreateText(cInfo.transform, "Hộp Gỗ Nhỏ", 21, FontStyle.Bold, ColCream);
            _carrySubText = CreateText(cInfo.transform, "Đặt tại Bàn Khui [E]", 14, FontStyle.Bold, ColMuted);
            _carryBanner.SetActive(false);
        }

        private void BuildBottomHints(Transform parent)
        {
            GameObject hints = new GameObject("BottomHints");
            hints.transform.SetParent(parent, false);
            RectTransform rt = hints.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-28f, 24f);
            rt.sizeDelta = new Vector2(380f, 38f);

            HorizontalLayoutGroup h = hints.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 16f;
            h.childAlignment = TextAnchor.MiddleRight;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;

            CreateHexKeycap(hints.transform, "Tab", 44f);
            Text tabT = CreateSingleLineText(hints.transform, "Sổ tay", 16, FontStyle.Bold, ColCream);
            AddTextShadow(tabT.gameObject);

            CreateHexKeycap(hints.transform, "Esc", 44f);
            Text escT = CreateSingleLineText(hints.transform, "Chuột", 16, FontStyle.Bold, ColCream);
            AddTextShadow(escT.gameObject);
        }

        private void BuildToastBanner(Transform parent)
        {
            _toastBanner = CreateChamferedPanel(parent, "ToastBanner", ColWalnutLight, ColBrass, 780f, 48f, small: true);
            RectTransform rt = _toastBanner.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.84f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(780f, 48f);
            Transform inner = _toastBanner.transform.Find("InnerFill");
            _toastBg = inner != null ? inner.GetComponent<Image>() : _toastBanner.GetComponent<Image>();

            _toastText = CreateText(_toastBanner.transform, string.Empty, 16, FontStyle.Bold, ColCream);
            _toastText.alignment = TextAnchor.MiddleCenter;
            StretchFull(_toastText.gameObject, 16f);
            _toastBanner.SetActive(false);
        }

        private void BuildCounterModal(Transform parent)
        {
            _counterModal = new GameObject("CounterModal");
            _counterModal.transform.SetParent(parent, false);
            StretchFull(_counterModal);
            Image dim = _counterModal.AddComponent<Image>();
            dim.color = new Color(0.06f, 0.04f, 0.02f, 0.42f);

            // Top-right Wallet Pill
            GameObject walletPill = CreateChamferedPanel(_counterModal.transform, "WalletPill", ColWalnut, ColBrass, 190f, 40f, small: true);
            RectTransform wrt = walletPill.GetComponent<RectTransform>();
            wrt.anchorMin = wrt.anchorMax = new Vector2(1f, 1f);
            wrt.pivot = new Vector2(1f, 1f);
            wrt.anchoredPosition = new Vector2(-36f, -28f);
            wrt.sizeDelta = new Vector2(190f, 40f);
            _counterWalletText = CreateText(walletPill.transform, "Ví: 900 xu", 16, FontStyle.Bold, ColCream);
            _counterWalletText.alignment = TextAnchor.MiddleCenter;
            StretchFull(_counterWalletText.gameObject);

            // Right / Center Grid: 2-Column Big Offer Cards
            GameObject rightCol = new GameObject("OfferColumn");
            rightCol.transform.SetParent(_counterModal.transform, false);
            RectTransform rrt = rightCol.AddComponent<RectTransform>();
            rrt.anchorMin = rrt.anchorMax = new Vector2(1f, 1f);
            rrt.pivot = new Vector2(1f, 1f);
            rrt.anchoredPosition = new Vector2(-36f, -84f);
            rrt.sizeDelta = new Vector2(910f, 520f);

            VerticalLayoutGroup rv = rightCol.AddComponent<VerticalLayoutGroup>();
            rv.spacing = 12f;
            rv.childControlWidth = true;
            rv.childControlHeight = true;
            rv.childForceExpandHeight = false;

            _counterQuotaText = CreateText(rightCol.transform, "LÔ HÒM HÔM NAY · 0/3 lượt", 16, FontStyle.Bold, ColBrass);
            AddTextShadow(_counterQuotaText.gameObject);

            GameObject listObj = new GameObject("OfferList");
            listObj.transform.SetParent(rightCol.transform, false);
            listObj.AddComponent<LayoutElement>().preferredHeight = 450f;
            GridLayoutGroup gl = listObj.AddComponent<GridLayoutGroup>();
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 2;
            gl.cellSize = new Vector2(445f, 128f);
            gl.spacing = new Vector2(18f, 14f);
            _counterListRoot = listObj.transform;

            // Bottom-Left: Dì Mận Dialogue Box
            GameObject dlg = CreateChamferedPanel(_counterModal.transform, "DialogueBox", ColWalnut, ColBrass, 910f, 200f, small: false);
            RectTransform drt = dlg.GetComponent<RectTransform>();
            drt.anchorMin = drt.anchorMax = new Vector2(0f, 0f);
            drt.pivot = new Vector2(0f, 0f);
            drt.anchoredPosition = new Vector2(40f, 34f);
            drt.sizeDelta = new Vector2(910f, 200f);

            HorizontalLayoutGroup dh = dlg.AddComponent<HorizontalLayoutGroup>();
            dh.padding = new RectOffset(20, 20, 16, 16);
            dh.spacing = 18f;
            dh.childAlignment = TextAnchor.MiddleLeft;
            dh.childControlWidth = true;
            dh.childControlHeight = true;
            dh.childForceExpandWidth = false;

            // Hexagon Dì Mận Portrait Emblem
            GameObject portRim = new GameObject("PortraitRim");
            portRim.transform.SetParent(dlg.transform, false);
            Image prImg = portRim.AddComponent<Image>();
            prImg.sprite = LowPolyUiSprites.Hexagon;
            prImg.color = ColBrass;
            LayoutElement ple = portRim.AddComponent<LayoutElement>();
            ple.preferredWidth = 96f;
            ple.preferredHeight = 86f;

            GameObject portIn = new GameObject("PortraitIn");
            portIn.transform.SetParent(portRim.transform, false);
            Image piImg = portIn.AddComponent<Image>();
            piImg.sprite = LowPolyUiSprites.Hexagon;
            piImg.color = ColWaxRed;
            RectTransform pirt = portIn.GetComponent<RectTransform>();
            pirt.anchorMin = Vector2.zero;
            pirt.anchorMax = Vector2.one;
            pirt.offsetMin = new Vector2(3f, 3f);
            pirt.offsetMax = new Vector2(-3f, -3f);
            CreateText(portIn.transform, "DÌ\nMẬN", 17, FontStyle.Bold, ColCream).alignment = TextAnchor.MiddleCenter;

            GameObject dInfo = CreateInfoColumn(dlg.transform);
            CreateText(dInfo.transform, "DÌ MẬN  ·  Chủ Nhà Niêm Phong", 17, FontStyle.Bold, ColGold);
            _dialogueQuoteText = CreateText(dInfo.transform, "\"Chọn hộp ở trên rồi bấm Mua với dì nghen!\"", 16, FontStyle.Normal, ColCream);
            _dialogueQuoteText.GetComponent<LayoutElement>().minHeight = 48f;

            GameObject dBtns = new GameObject("DialogueButtons");
            dBtns.transform.SetParent(dInfo.transform, false);
            dBtns.AddComponent<LayoutElement>().preferredHeight = 48f;
            HorizontalLayoutGroup dbh = dBtns.AddComponent<HorizontalLayoutGroup>();
            dbh.spacing = 12f;
            dbh.childControlWidth = true;
            dbh.childControlHeight = true;
            dbh.childForceExpandWidth = false;

            _dialogueBuyBtn = CreateButton(dBtns.transform, "1 · Mua", ColJade, 220f, 46f, TryBuyHighlightedOffer);
            _tipButton = CreateButton(dBtns.transform, "2 · Lời mách", ColWalnutLight, 155f, 46f, ShowSelectedOfferClue);
            CreateButton(dBtns.transform, "3 · Đóng [Esc]", ColWalnutLight, 165f, 46f, CloseAllModals);

            _counterModal.SetActive(false);
        }

        private void BuildInspectModal(Transform parent)
        {
            _inspectModal = CreateChamferedPanel(parent, "InspectModal", ColPaper, ColBrass, 400f, 610f, small: false);
            RectTransform rt = _inspectModal.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-34f, -10f);
            rt.sizeDelta = new Vector2(400f, 610f);

            // Circular Wax Seal Stamp on top-right
            GameObject stampGo = new GameObject("WaxStamp");
            stampGo.transform.SetParent(_inspectModal.transform, false);
            stampGo.AddComponent<LayoutElement>().ignoreLayout = true;
            _insStampRing = stampGo.AddComponent<Image>();
            _insStampRing.sprite = LowPolyUiSprites.WaxStampRing;
            _insStampRing.color = ColJade;
            RectTransform stRt = stampGo.GetComponent<RectTransform>();
            stRt.anchorMin = stRt.anchorMax = new Vector2(1f, 1f);
            stRt.pivot = new Vector2(0.5f, 0.5f);
            stRt.anchoredPosition = new Vector2(-56f, -94f);
            stRt.sizeDelta = new Vector2(82f, 82f);
            stRt.localRotation = Quaternion.Euler(0f, 0f, 12f);

            _insStampText = CreateText(stampGo.transform, "HÀNG\nTHẬT", 13, FontStyle.Bold, ColJade);
            _insStampText.alignment = TextAnchor.MiddleCenter;
            StretchFull(_insStampText.gameObject);

            VerticalLayoutGroup v = _inspectModal.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(24, 24, 22, 20);
            v.spacing = 9f;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;

            _insCategory = CreateText(_inspectModal.transform, "PHIẾU THẨM ĐỊNH", 12, FontStyle.Bold, ColInkMuted);
            _insName = CreateText(_inspectModal.transform, "Tên Hiện Vật", 24, FontStyle.Bold, ColInk);

            GameObject statsCard = CreateChamferedPanel(_inspectModal.transform, "StatsCard", ColPaperDark, ColBrassDark, 0f, 94f, small: true);
            VerticalLayoutGroup sv = statsCard.AddComponent<VerticalLayoutGroup>();
            sv.padding = new RectOffset(12, 12, 8, 8);
            sv.spacing = 5f;
            sv.childControlWidth = true;
            sv.childControlHeight = true;
            sv.childForceExpandHeight = false;
            _insRarity = AddStatRow(statsCard.transform, "Độ hiếm:", out _);
            _insQuality = AddStatRow(statsCard.transform, "Tình trạng:", out _);
            _insAuth = AddStatRow(statsCard.transform, "Xác thực:", out _);

            AddDivider(_inspectModal.transform);
            CreateText(_inspectModal.transform, "DẤU HIỆU QUAN SÁT", 12, FontStyle.Bold, ColInkMuted);
            _insClue = CreateText(_inspectModal.transform, "...", 15, FontStyle.Italic, ColInk);
            _insClue.GetComponent<LayoutElement>().minHeight = 64f;

            AddDivider(_inspectModal.transform);
            GameObject priceRow = new GameObject("PriceRow");
            priceRow.transform.SetParent(_inspectModal.transform, false);
            priceRow.AddComponent<LayoutElement>().preferredHeight = 44f;
            HorizontalLayoutGroup ph = priceRow.AddComponent<HorizontalLayoutGroup>();
            ph.childControlWidth = true;
            ph.childControlHeight = true;
            ph.childForceExpandWidth = false;

            _insBasePrice = CreateText(priceRow.transform, "Giá gốc: 0 xu", 14, FontStyle.Normal, ColInkMuted);
            _insBasePrice.GetComponent<LayoutElement>().flexibleWidth = 1f;
            _insPrice = CreateSingleLineText(priceRow.transform, "0 xu", 24, FontStyle.Bold, ColWaxRed);

            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(_inspectModal.transform, false);
            spacer.AddComponent<LayoutElement>().flexibleHeight = 1f;

            _insSellBtn = CreateButton(_inspectModal.transform, "Bán ngay", ColJade, 0f, 46f, null);
            GameObject subBtns = new GameObject("SubActions");
            subBtns.transform.SetParent(_inspectModal.transform, false);
            subBtns.AddComponent<LayoutElement>().preferredHeight = 42f;
            HorizontalLayoutGroup sbh = subBtns.AddComponent<HorizontalLayoutGroup>();
            sbh.spacing = 8f;
            sbh.childControlWidth = true;
            sbh.childControlHeight = true;
            sbh.childForceExpandWidth = true;

            _insKeepBtn = CreateButton(subBtns.transform, "Cất vào kho", ColTeal, 0f, 42f, null);
            CreateButton(subBtns.transform, "Đặt xuống [Esc]", ColWalnutLight, 0f, 42f, LeaveArtifactOnTable);

            // Bottom-left Inspect control hints
            _inspectHintsBar = new GameObject("InspectControlHints");
            _inspectHintsBar.transform.SetParent(parent, false);
            RectTransform hrt = _inspectHintsBar.AddComponent<RectTransform>();
            hrt.anchorMin = hrt.anchorMax = new Vector2(0f, 0f);
            hrt.pivot = new Vector2(0f, 0f);
            hrt.anchoredPosition = new Vector2(30f, 24f);
            hrt.sizeDelta = new Vector2(620f, 34f);

            HorizontalLayoutGroup hh = _inspectHintsBar.AddComponent<HorizontalLayoutGroup>();
            hh.spacing = 12f;
            hh.childAlignment = TextAnchor.MiddleLeft;
            hh.childControlWidth = true;
            hh.childControlHeight = true;
            hh.childForceExpandWidth = false;

            AddHintPair(_inspectHintsBar.transform, "Chuột", "Xoay 360°");
            AddHintPair(_inspectHintsBar.transform, "Lăn", "Phóng to");
            AddHintPair(_inspectHintsBar.transform, "R", "Đặt lại góc");
            AddHintPair(_inspectHintsBar.transform, "Esc", "Đặt xuống");

            _inspectModal.SetActive(false);
            _inspectHintsBar.SetActive(false);
        }

        private void BuildLedgerModal(Transform parent)
        {
            _ledgerModal = new GameObject("LedgerModal");
            _ledgerModal.transform.SetParent(parent, false);
            StretchFull(_ledgerModal);
            _ledgerModal.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.02f, 0.55f);

            // Walnut Leather Book Cover (Bigger + Cleaner 2-Col Debt Layout)
            GameObject book = CreateChamferedPanel(_ledgerModal.transform, "BookCover", ColBookCover, ColBrass, 1100f, 700f, small: false);
            CenterRect(book, 1100f, 700f);

            // Protruding Colored Ribbon Bookmarks on Right Edge
            GameObject ribbons = new GameObject("Ribbons");
            ribbons.transform.SetParent(book.transform, false);
            RectTransform rrt = ribbons.AddComponent<RectTransform>();
            rrt.anchorMin = rrt.anchorMax = new Vector2(1f, 1f);
            rrt.pivot = new Vector2(0f, 1f);
            rrt.anchoredPosition = new Vector2(-4f, -48f);
            rrt.sizeDelta = new Vector2(148f, 280f);
            VerticalLayoutGroup rlv = ribbons.AddComponent<VerticalLayoutGroup>();
            rlv.spacing = 12f;
            rlv.childControlWidth = true;
            rlv.childControlHeight = true;
            rlv.childForceExpandHeight = false;

            _tabInvRt = CreateRibbonButton(ribbons.transform, "KHO ĐỒ", ColTeal, () => SwitchLedgerTab(0));
            _tabTactRt = CreateRibbonButton(ribbons.transform, "TẠO TÁC", ColBrassDark, () => SwitchLedgerTab(1));
            _tabBikheRt = CreateRibbonButton(ribbons.transform, "PHÁT TRIỂN", ColPurple, () => SwitchLedgerTab(2));
            _tabTactRt.gameObject.SetActive(false);
            _tabDebtRt = CreateRibbonButton(ribbons.transform, "SỔ NỢ", ColWaxRed, () => SwitchLedgerTab(3));

            // Paper Inner Page Frame
            GameObject page = CreateChamferedPanel(book.transform, "BookPage", ColPaper, ColBrassDark, 0f, 0f, small: true);
            RectTransform prt = page.GetComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(20f, 20f);
            prt.offsetMax = new Vector2(-20f, -20f);

            VerticalLayoutGroup v = page.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(26, 26, 20, 20);
            v.spacing = 12f;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;

            CreateHeaderRow(page.transform, "SỔ TAY GIA TỘC", CloseAllModals, darkInk: true);

            _invTabContent = CreateScrollArea(page.transform, "InventoryTab", 545f, out _invListRoot, out _invScroll);
            _tactTabContent = CreateScrollArea(page.transform, "TactTab", 545f, out _tactListRoot, out _tactScroll);
            _bikheTabContent = CreateScrollArea(page.transform, "BikheTab", 545f, out _bikheListRoot, out _bikheScroll);

            // 2-Column Smart Debt Tab (Left: Summary + 2x2 Big Buttons | Right: 7-Day Rows)
            _debtTabContent = new GameObject("DebtTab");
            _debtTabContent.transform.SetParent(page.transform, false);
            LayoutElement debtLe = _debtTabContent.AddComponent<LayoutElement>();
            debtLe.preferredHeight = 545f;
            debtLe.flexibleHeight = 1f;
            HorizontalLayoutGroup dhSplit = _debtTabContent.AddComponent<HorizontalLayoutGroup>();
            dhSplit.spacing = 20f;
            dhSplit.childControlWidth = true;
            dhSplit.childControlHeight = true;
            dhSplit.childForceExpandWidth = false;

            // Left Column
            GameObject dLeft = new GameObject("DebtLeftCol");
            dLeft.transform.SetParent(_debtTabContent.transform, false);
            dLeft.AddComponent<LayoutElement>().preferredWidth = 390f;
            VerticalLayoutGroup dlv = dLeft.AddComponent<VerticalLayoutGroup>();
            dlv.spacing = 14f;
            dlv.childControlWidth = true;
            dlv.childControlHeight = true;
            dlv.childForceExpandHeight = false;

            GameObject sumCard = CreateChamferedPanel(dLeft.transform, "DebtSumCard", ColPaperDark, ColBrassDark, 0f, 180f, small: true);
            VerticalLayoutGroup scv = sumCard.AddComponent<VerticalLayoutGroup>();
            scv.padding = new RectOffset(18, 18, 16, 16);
            scv.spacing = 8f;
            scv.childControlWidth = true;
            scv.childControlHeight = true;
            CreateText(sumCard.transform, "TÌNH HÌNH HÔM NAY", 13, FontStyle.Bold, ColInkMuted);
            _debtSummaryText = CreateText(sumCard.transform, "...", 20, FontStyle.Bold, ColInk);

            GameObject debtActions = new GameObject("DebtActions");
            debtActions.transform.SetParent(dLeft.transform, false);
            debtActions.AddComponent<LayoutElement>().preferredHeight = 140f;
            GridLayoutGroup dGrid = debtActions.AddComponent<GridLayoutGroup>();
            dGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            dGrid.constraintCount = 2;
            dGrid.cellSize = new Vector2(188f, 56f);
            dGrid.spacing = new Vector2(12f, 12f);

            _endDayBtn = CreateButton(debtActions.transform, "Kết thúc ngày", ColJade, 0f, 56f, () =>
            {
                if (marketService == null || !_ledgerModal.activeSelf) return;
                if (marketService.RequestEndDay(_ledgerConfirmedDay) && _ledgerModal.activeSelf) CloseAllModals();
            });

            // Right Column (7-Day Rows)
            GameObject debtRows = new GameObject("DebtRows");
            debtRows.transform.SetParent(_debtTabContent.transform, false);
            debtRows.AddComponent<LayoutElement>().flexibleWidth = 1f;
            VerticalLayoutGroup drv = debtRows.AddComponent<VerticalLayoutGroup>();
            drv.spacing = 8f;
            drv.childControlWidth = true;
            drv.childControlHeight = true;
            drv.childForceExpandHeight = false;
            _debtListRoot = debtRows.transform;

            _ledgerModal.SetActive(false);
        }

        private void BuildRunEndModal(Transform parent)
        {
            _runEndModal = new GameObject("RunEndModal");
            _runEndModal.transform.SetParent(parent, false);
            StretchFull(_runEndModal);
            _runEndModal.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.02f, 0.62f);

            GameObject receipt = CreateChamferedPanel(_runEndModal.transform, "Receipt", ColPaper, ColBrass, 500f, 460f, small: false);
            CenterRect(receipt, 660f, 680f);

            GameObject stampGo = new GameObject("EndStamp");
            stampGo.transform.SetParent(receipt.transform, false);
            stampGo.AddComponent<LayoutElement>().ignoreLayout = true;
            _runEndStampRing = stampGo.AddComponent<Image>();
            _runEndStampRing.sprite = LowPolyUiSprites.WaxStampRing;
            _runEndStampRing.color = ColJade;
            RectTransform stRt = stampGo.GetComponent<RectTransform>();
            stRt.anchorMin = stRt.anchorMax = new Vector2(1f, 0.5f);
            stRt.pivot = new Vector2(0.5f, 0.5f);
            stRt.anchoredPosition = new Vector2(-72f, 20f);
            stRt.sizeDelta = new Vector2(100f, 100f);
            stRt.localRotation = Quaternion.Euler(0f, 0f, 14f);

            _runEndStampText = CreateText(stampGo.transform, "ĐÃ TRẢ\nNỢ", 14, FontStyle.Bold, ColJade);
            _runEndStampText.alignment = TextAnchor.MiddleCenter;
            StretchFull(_runEndStampText.gameObject);

            VerticalLayoutGroup v = receipt.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(30, 30, 24, 24);
            v.spacing = 12f;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;

            CreateText(receipt.transform, "BIÊN LAI CUỐI VÁN", 12, FontStyle.Bold, ColInkMuted).alignment = TextAnchor.MiddleCenter;
            _runEndTitle = CreateText(receipt.transform, "KẾT THÚC VÁN", 28, FontStyle.Bold, ColInk);
            _runEndTitle.alignment = TextAnchor.MiddleCenter;
            AddDivider(receipt.transform);

            _runEndBody = CreateText(receipt.transform, "...", 15, FontStyle.Normal, ColInk);
            _runEndBody.GetComponent<LayoutElement>().flexibleHeight = 1f;

            CreateScrollArea(receipt.transform, "SkillTree", 375f, out _runEndSkillRoot, out _);

            GameObject actions = new GameObject("RunEndActions");
            actions.transform.SetParent(receipt.transform, false);
            actions.AddComponent<LayoutElement>().preferredHeight = 46f;
            HorizontalLayoutGroup ah = actions.AddComponent<HorizontalLayoutGroup>();
            ah.spacing = 12f;
            ah.childAlignment = TextAnchor.MiddleCenter;
            ah.childControlWidth = true;
            ah.childControlHeight = true;
            ah.childForceExpandWidth = true;

            CreateButton(actions.transform, "Chơi Lại Ván Mới", ColJade, 0f, 44f, () =>
            {
                if (marketService != null && marketService.StartNextRun(false)) CloseAllModals();
            });
            _earlyChestButton = CreateButton(actions.transform, "Ván mới · Rương Đen sớm", ColPurple, 0f, 44f, () =>
            {
                if (marketService != null && marketService.StartNextRun(true)) CloseAllModals();
            });
            _runEndModal.SetActive(false);
        }

        private GameObject CreateChamferedPanel(Transform parent, string name, Color fill, Color rim, float w, float h, bool small)
        {
            Sprite sp = small ? LowPolyUiSprites.ChamferedSmall : LowPolyUiSprites.ChamferedRect;
            GameObject outer = new GameObject(name);
            outer.transform.SetParent(parent, false);
            Image rimImg = outer.AddComponent<Image>();
            rimImg.sprite = sp;
            rimImg.type = Image.Type.Sliced;
            rimImg.color = rim;

            LayoutElement le = outer.AddComponent<LayoutElement>();
            if (w > 0f) le.preferredWidth = w;
            if (h > 0f) le.preferredHeight = h;

            GameObject inner = new GameObject("InnerFill");
            inner.transform.SetParent(outer.transform, false);
            inner.AddComponent<LayoutElement>().ignoreLayout = true;
            Image fillImg = inner.AddComponent<Image>();
            fillImg.sprite = sp;
            fillImg.type = Image.Type.Sliced;
            fillImg.color = fill;
            fillImg.raycastTarget = false;
            RectTransform irt = inner.GetComponent<RectTransform>();
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(2f, 2f);
            irt.offsetMax = new Vector2(-2f, -2f);
            return outer;
        }

        private void CreateHexKeycap(Transform parent, string keyLabel, float width)
        {
            GameObject hex = new GameObject($"Key_{keyLabel}");
            hex.transform.SetParent(parent, false);
            Image img = hex.AddComponent<Image>();
            img.sprite = LowPolyUiSprites.Hexagon;
            img.color = ColGold;
            LayoutElement le = hex.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = 28f;

            Text t = CreateText(hex.transform, keyLabel, 13, FontStyle.Bold, ColWalnut);
            t.alignment = TextAnchor.MiddleCenter;
            StretchFull(t.gameObject);
        }

        private void AddHintPair(Transform parent, string key, string desc)
        {
            CreateHexKeycap(parent, key, key.Length > 2 ? 52f : 30f);
            Text t = CreateSingleLineText(parent, desc, 13, FontStyle.Bold, ColCream);
            AddTextShadow(t.gameObject);
        }

        private Text AddStatRow(Transform parent, string label, out Text labelTxt)
        {
            GameObject row = new GameObject("StatRow");
            row.transform.SetParent(parent, false);
            row.AddComponent<LayoutElement>().preferredHeight = 22f;
            HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;

            labelTxt = CreateText(row.transform, label, 14, FontStyle.Normal, ColInkMuted);
            labelTxt.GetComponent<LayoutElement>().preferredWidth = 96f;
            return CreateText(row.transform, "...", 14, FontStyle.Bold, ColInk);
        }

        private static void AddDivider(Transform parent)
        {
            GameObject div = new GameObject("Divider");
            div.transform.SetParent(parent, false);
            div.AddComponent<Image>().color = new Color(0.227f, 0.145f, 0.086f, 0.25f);
            div.AddComponent<LayoutElement>().preferredHeight = 1.5f;
        }

        private RectTransform CreateRibbonButton(Transform parent, string label, Color bg, UnityEngine.Events.UnityAction onClick)
        {
            Button btn = CreateButton(parent, label, bg, 138f, 52f, onClick);
            return btn.GetComponent<RectTransform>();
        }

        private HorizontalLayoutGroup CreateHeaderRow(Transform parent, string title, UnityEngine.Events.UnityAction onClose, bool darkInk = false)
        {
            GameObject row = new GameObject("Header");
            row.transform.SetParent(parent, false);
            row.AddComponent<LayoutElement>().preferredHeight = 40f;
            HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;

            Text t = CreateText(row.transform, title, 20, FontStyle.Bold, darkInk ? ColInk : ColBrass);
            t.GetComponent<LayoutElement>().flexibleWidth = 1f;
            CreateButton(row.transform, "Đóng [Esc]", ColWaxRed, 118f, 36f, onClose);
            return h;
        }

        private Text CreateSingleLineText(Transform parent, string content, int fontSize, FontStyle style, Color color)
        {
            Text t = CreateText(parent, content, fontSize, style, color);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            ContentSizeFitter csf = t.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return t;
        }

        private Text CreateText(Transform parent, string content, int fontSize, FontStyle style, Color color)
        {
            GameObject go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            Text t = go.AddComponent<Text>();
            t.font = _font;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.text = content;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            go.AddComponent<LayoutElement>();
            return t;
        }

        private Button CreateButton(Transform parent, string label, Color bg, float w, float h, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject($"Btn_{label}");
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.sprite = LowPolyUiSprites.ChamferedSmall;
            img.type = Image.Type.Sliced;
            img.color = bg;
            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);

            LayoutElement le = go.AddComponent<LayoutElement>();
            if (w > 0f) le.preferredWidth = w;
            if (h > 0f) le.preferredHeight = h;

            Text t = CreateText(go.transform, label, 15, FontStyle.Bold, ColCream);
            t.alignment = TextAnchor.MiddleCenter;
            StretchFull(t.gameObject);
            return btn;
        }

        private static void StretchFull(GameObject go, float pad = 0f)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        private static void CenterRect(GameObject go, float w, float h)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void AddOutline(GameObject go, Color col)
        {
            Outline o = go.AddComponent<Outline>();
            o.effectColor = col;
            o.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private static void AddTextShadow(GameObject go)
        {
            Shadow s = go.AddComponent<Shadow>();
            s.effectColor = new Color(0f, 0f, 0f, 0.55f);
            s.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private static GameObject CreateScrollArea(Transform parent, string name, float height, out Transform contentRoot, out ScrollRect scrollRect)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.01f);
            LayoutElement le = root.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.flexibleHeight = 1f;
            root.AddComponent<RectMask2D>();

            scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 32f;
            scrollRect.viewport = root.GetComponent<RectTransform>();

            GameObject content = new GameObject("Content");
            content.transform.SetParent(root.transform, false);
            RectTransform crt = content.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = Vector2.zero;

            VerticalLayoutGroup v = content.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(2, 8, 2, 8);
            v.spacing = 8f;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = crt;
            contentRoot = content.transform;
            return root;
        }

        private static GameObject CreateAutoCard(Transform parent, string name, Color bg, float minHeight)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.sprite = LowPolyUiSprites.ChamferedSmall;
            img.type = Image.Type.Sliced;
            img.color = bg;
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = minHeight;
            return go;
        }

        private static GameObject CreateInfoColumn(Transform parent)
        {
            GameObject info = new GameObject("Info");
            info.transform.SetParent(parent, false);
            info.AddComponent<LayoutElement>().flexibleWidth = 1f;
            VerticalLayoutGroup v = info.AddComponent<VerticalLayoutGroup>();
            v.spacing = 3f;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;
            return info;
        }

        private void AddSectionBanner(Transform parent, string title)
        {
            GameObject banner = CreateChamferedPanel(parent, "SectionBanner", ColWalnutLight, ColBrassDark, 0f, 32f, small: true);
            HorizontalLayoutGroup h = banner.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(12, 12, 4, 4);
            h.childControlWidth = true;
            h.childControlHeight = true;
            CreateText(banner.transform, title, 13, FontStyle.Bold, ColBrass);
        }

        private static void RebuildScrollLayout(Transform contentRoot)
        {
            RectTransform rt = contentRoot as RectTransform;
            if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        private static Color GetInkRarityColor(RarityTier r) => r switch
        {
            RarityTier.Uncommon => ColJade,
            RarityTier.Rare => ColTeal,
            RarityTier.Mythic => ColWaxRed,
            _ => ColInk
        };

        private static string FormatSystemName(TactSystemType sys) =>
            sys == TactSystemType.ChronoSteam ? "Đồng Hồ Hơi Nước" : "Dị Giới Lõi Rift";

        private static void ClearChildren(Transform root)
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject child = root.GetChild(i).gameObject;
                child.transform.SetParent(null, false);
                Destroy(child);
            }
        }

        private static string FormatQuality(QualityTier q) => q switch
        {
            QualityTier.Damaged => "Hư hại",
            QualityTier.Worn => "Cũ kỹ",
            QualityTier.Good => "Tốt",
            QualityTier.Pristine => "Hoàn hảo",
            _ => q.ToString()
        };

        private static string FormatAuth(AuthenticityState a) => a switch
        {
            AuthenticityState.Authentic => "Hàng Thật",
            AuthenticityState.Counterfeit => "Hàng Giả",
            _ => "Chưa rõ"
        };

        private static string FormatRarity(RarityTier r) => r switch
        {
            RarityTier.Common => "Thường",
            RarityTier.Uncommon => "Ít gặp",
            RarityTier.Rare => "Hiếm",
            RarityTier.Mythic => "Huyền thoại",
            _ => r.ToString()
        };

        private static bool IsTabPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Tab);
#endif
        }

        private static bool IsEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        private static bool IsKeyQPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Q);
#endif
        }

        private static bool IsKeyEPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }

        private static bool IsDigitPressed(int n)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) return false;
            return n switch
            {
                1 => Keyboard.current.digit1Key.wasPressedThisFrame,
                2 => Keyboard.current.digit2Key.wasPressedThisFrame,
                3 => Keyboard.current.digit3Key.wasPressedThisFrame,
                _ => false
            };
#else
            return Input.GetKeyDown(KeyCode.Alpha0 + n);
#endif
        }
    }
}

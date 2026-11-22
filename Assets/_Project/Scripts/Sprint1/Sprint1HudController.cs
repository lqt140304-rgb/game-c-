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
    /// Stylized Sprint 1 HUD & Transaction UI Controller.
    /// Builds and manages Top Status Bar, Crosshair/Prompt, Toasts, Counter Modal,
    /// 3D Artifact Inspection Card, Ledger/Inventory Modal, and Run End Screen.
    /// </summary>
    public class Sprint1HudController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Sprint1MarketService marketService;
        [SerializeField] private ArtifactInspector3D inspector3D;
        [SerializeField] private OpenTableManager openTableManager;
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private PlayerInteractor playerInteractor;

        private Font _font;
        private Canvas _canvas;

        // HUD Elements
        private Text _dayText;
        private Text _cashText;
        private Text _cashDeltaText;
        private Text _debtText;
        private Text _boxesText;
        private GameObject _promptPill;
        private Text _promptText;
        private GameObject _toastBanner;
        private Image _toastBg;
        private Text _toastText;
        private float _toastTimer;
        private float _deltaTimer;
        private int _lastCash = -1;

        // Modals
        private GameObject _counterModal;
        private Transform _counterListRoot;
        private Text _counterQuotaText;
        private string _highlightedOfferId;

        private GameObject _inspectModal;
        private Text _insName;
        private Text _insCategory;
        private Text _insQuality;
        private Text _insAuth;
        private Text _insRarity;
        private Text _insClue;
        private Text _insPrice;
        private Button _insSellBtn;
        private Button _insKeepBtn;
        private ArtifactInstanceView _inspectingArtifact;

        private GameObject _ledgerModal;
        private GameObject _invTabContent;
        private GameObject _debtTabContent;
        private Transform _invListRoot;
        private Transform _debtListRoot;
        private Text _debtSummaryText;
        private Button _payDebtBtn;
        private Button _endDayBtn;
        private Image _tabInvBg;
        private Image _tabDebtBg;

        private GameObject _runEndModal;
        private Text _runEndTitle;
        private Text _runEndBody;

        // Theme Colors
        private static readonly Color ColPanelDark = new Color(0.08f, 0.07f, 0.10f, 0.94f);
        private static readonly Color ColCardBg = new Color(0.13f, 0.11f, 0.16f, 0.96f);
        private static readonly Color ColCardHighlight = new Color(0.20f, 0.16f, 0.12f, 0.98f);
        private static readonly Color ColBrass = new Color(0.82f, 0.64f, 0.25f, 1f);
        private static readonly Color ColCream = new Color(0.95f, 0.91f, 0.82f, 1f);
        private static readonly Color ColMuted = new Color(0.70f, 0.67f, 0.62f, 1f);
        private static readonly Color ColGreen = new Color(0.22f, 0.68f, 0.42f, 1f);
        private static readonly Color ColRed = new Color(0.76f, 0.24f, 0.24f, 1f);
        private static readonly Color ColAmber = new Color(0.88f, 0.62f, 0.18f, 1f);

        public void Configure(
            Sprint1MarketService service,
            ArtifactInspector3D inspector,
            OpenTableManager tableMgr,
            FirstPersonController fpc,
            PlayerInteractor interactor)
        {
            marketService = service;
            inspector3D = inspector;
            openTableManager = tableMgr;
            playerController = fpc;
            playerInteractor = interactor;
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
        }

        private void Start()
        {
            if (marketService != null)
            {
                HandleRunStateChanged(marketService.CurrentRunState);
                RefreshInventoryTab();
                RefreshDebtTab();
            }
            CloseAllModals();
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
            SetPlayerLocked(true);
        }

        public void ShowInspectPanel(ArtifactInstanceView artifact)
        {
            _counterModal.SetActive(false);
            _ledgerModal.SetActive(false);
            _inspectingArtifact = artifact;

            _insName.text = artifact.DisplayName;
            _insCategory.text = $"NHÓM: {artifact.Category.ToUpper()}";
            _insQuality.text = $"Tình trạng: {FormatQuality(artifact.Quality)}";
            _insQuality.color = artifact.Quality == QualityTier.Damaged ? ColRed : (artifact.Quality == QualityTier.Pristine ? ColGreen : ColCream);

            _insAuth.text = $"Xác thực: {FormatAuth(artifact.Authenticity)}";
            _insAuth.color = artifact.Authenticity == AuthenticityState.Authentic ? ColGreen : ColRed;

            _insRarity.text = $"Độ hiếm: {FormatRarity(artifact.Rarity)}";
            _insClue.text = artifact.InspectClueText;
            _insPrice.text = $"Giá thu mua: {artifact.CurrentSellPrice} xu";

            Text sellBtnText = _insSellBtn.GetComponentInChildren<Text>();
            if (sellBtnText != null) sellBtnText.text = $"Bán ngay (+{artifact.CurrentSellPrice} xu)";

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
                    inspector3D.EndInspect();
                }
            });

            _inspectModal.SetActive(true);
        }

        public void OpenLedgerModal(bool startOnDebtTab)
        {
            CloseAllModals();
            RefreshInventoryTab();
            RefreshDebtTab();
            _ledgerModal.SetActive(true);
            SwitchLedgerTab(startOnDebtTab);
            SetPlayerLocked(true);
        }

        public void CloseAllModals()
        {
            if (_counterModal != null) _counterModal.SetActive(false);
            if (_inspectModal != null) _inspectModal.SetActive(false);
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

            _dayText.text = $"NGÀY {s.CurrentDay}/{s.MaxDays}";
            _cashText.text = $"{s.Cash:N0} xu";

            if (_lastCash >= 0 && _lastCash != s.Cash)
            {
                int diff = s.Cash - _lastCash;
                _cashDeltaText.text = diff > 0 ? $"+{diff} xu" : $"{diff} xu";
                _cashDeltaText.color = diff > 0 ? ColGreen : ColRed;
                _cashDeltaText.gameObject.SetActive(true);
                _deltaTimer = 2.2f;
            }
            _lastCash = s.Cash;

            _debtText.text = s.IsDebtPaidToday
                ? $"Nợ hôm nay: ĐÃ TRẢ ({s.DebtDueToday} xu)"
                : $"Nợ hôm nay: {s.DebtDueToday} xu";
            _debtText.color = s.IsDebtPaidToday ? ColGreen : ColAmber;

            _boxesText.text = $"Hộp: {s.DailyBoxesPurchased}/{s.MaxDailyBoxes}";
            _boxesText.color = s.DailyBoxesPurchased >= s.MaxDailyBoxes ? ColRed : ColCream;

            if (_counterModal != null && _counterModal.activeSelf) RefreshCounterModal();
            if (_ledgerModal != null && _ledgerModal.activeSelf) RefreshDebtTab();

            if (s.IsRunEnded)
            {
                ShowRunEndScreen(s);
            }
        }

        private void HandleInventoryChanged()
        {
            if (_ledgerModal != null && _ledgerModal.activeSelf)
            {
                RefreshInventoryTab();
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

        private void ShowToast(string message, bool isError)
        {
            if (_toastBanner == null || _toastText == null || _toastBg == null) return;
            _toastText.text = message;
            _toastBg.color = isError ? new Color(0.55f, 0.12f, 0.14f, 0.95f) : new Color(0.14f, 0.24f, 0.18f, 0.95f);
            _toastBanner.SetActive(true);
            _toastTimer = 3.4f;
        }

        private void ShowRunEndScreen(RunStateSnapshot s)
        {
            CloseAllModals();
            _runEndTitle.text = s.IsVictory ? "HOÀN THÀNH 7 NGÀY!" : "VỠ NỢ!";
            _runEndTitle.color = s.IsVictory ? ColGreen : ColRed;
            _runEndBody.text = s.EndReason;
            _runEndModal.SetActive(true);
            SetPlayerLocked(true);
        }

        private void RefreshCounterModal()
        {
            ClearChildren(_counterListRoot);
            if (marketService == null) return;

            RunStateSnapshot s = marketService.CurrentRunState;
            _counterQuotaText.text = $"Lượt mua hôm nay: {s.DailyBoxesPurchased}/{s.MaxDailyBoxes}  |  Tiền mặt: {s.Cash:N0} xu";

            foreach (BoxOfferView offer in marketService.CurrentOffers)
            {
                bool isHighlighted = offer.OfferId == _highlightedOfferId;
                GameObject card = CreatePanel(_counterListRoot, $"Offer_{offer.OfferId}", isHighlighted ? ColCardHighlight : ColCardBg, 0f, 92f);
                HorizontalLayoutGroup h = card.AddComponent<HorizontalLayoutGroup>();
                h.padding = new RectOffset(14, 14, 10, 10);
                h.spacing = 12f;
                h.childControlWidth = true;
                h.childControlHeight = true;
                h.childForceExpandWidth = false;

                // Info column
                GameObject infoCol = new GameObject("Info");
                infoCol.transform.SetParent(card.transform, false);
                LayoutElement infoLe = infoCol.AddComponent<LayoutElement>();
                infoLe.flexibleWidth = 1f;
                VerticalLayoutGroup v = infoCol.AddComponent<VerticalLayoutGroup>();
                v.spacing = 4f;
                v.childControlHeight = true;
                v.childControlWidth = true;

                CreateText(infoCol.transform, $"{offer.DisplayName}  —  [{offer.PotentialCategory}]", 17, FontStyle.Bold, ColBrass);
                CreateText(infoCol.transform, offer.ClueText, 13, FontStyle.Normal, ColCream);

                // Action column
                string capturedOfferId = offer.OfferId;
                bool canAfford = s.Cash >= offer.Price && s.DailyBoxesPurchased < s.MaxDailyBoxes;
                Button btn = CreateButton(card.transform, $"Mua ({offer.Price} xu)", canAfford ? ColGreen : new Color(0.28f, 0.26f, 0.30f), 160f, 52f, () =>
                {
                    if (marketService.RequestBuyBox(capturedOfferId))
                    {
                        CloseAllModals();
                    }
                });
            }
        }

        private void RefreshInventoryTab()
        {
            ClearChildren(_invListRoot);
            if (marketService == null) return;

            IReadOnlyList<ArtifactInstanceView> items = marketService.InventoryItems;
            if (items.Count == 0)
            {
                CreateText(_invListRoot, "Kho hiện vật đang trống. Khi mở hộp, bạn có thể chọn 'Cất vào kho' để giữ lại.", 15, FontStyle.Italic, ColMuted);
                return;
            }

            foreach (ArtifactInstanceView item in items)
            {
                GameObject row = CreatePanel(_invListRoot, $"Inv_{item.InstanceId}", ColCardBg, 0f, 74f);
                HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
                h.padding = new RectOffset(12, 12, 8, 8);
                h.spacing = 10f;
                h.childControlWidth = true;
                h.childControlHeight = true;
                h.childForceExpandWidth = false;

                GameObject info = new GameObject("Info");
                info.transform.SetParent(row.transform, false);
                info.AddComponent<LayoutElement>().flexibleWidth = 1f;
                VerticalLayoutGroup v = info.AddComponent<VerticalLayoutGroup>();
                v.spacing = 3f;
                v.childControlWidth = true;
                v.childControlHeight = true;
                v.childForceExpandHeight = false;

                CreateText(info.transform, $"{item.DisplayName} ({item.Category})", 16, FontStyle.Bold, ColBrass);
                CreateText(info.transform, $"{FormatQuality(item.Quality)} · {FormatAuth(item.Authenticity)} · {FormatRarity(item.Rarity)}", 13, FontStyle.Normal, ColCream);

                string capturedId = item.InstanceId;
                CreateButton(row.transform, $"Bán (+{item.CurrentSellPrice} xu)", ColGreen, 155f, 44f, () =>
                {
                    marketService.RequestSellArtifact(capturedId);
                });
            }
        }

        private void RefreshDebtTab()
        {
            ClearChildren(_debtListRoot);
            if (marketService == null) return;

            RunStateSnapshot s = marketService.CurrentRunState;
            _debtSummaryText.text = s.IsDebtPaidToday
                ? $"Kỳ nợ Ngày {s.CurrentDay}: ĐÃ THANH TOÁN ({s.DebtDueToday} xu). Bạn có thể kết thúc ngày để sang ngày mới."
                : $"Kỳ nợ Ngày {s.CurrentDay}: CẦN TRẢ {s.DebtDueToday} xu (Tiền mặt hiện có: {s.Cash:N0} xu).";

            int[] schedule = marketService.DebtSchedule;
            for (int i = 0; i < schedule.Length; i++)
            {
                int dayNum = i + 1;
                bool isToday = dayNum == s.CurrentDay;
                bool isPast = dayNum < s.CurrentDay || (isToday && s.IsDebtPaidToday);

                string status = isPast ? "Đã trả" : (isToday ? "ĐẾN HẠN HÔM NAY" : "Chưa tới kỳ");
                Color col = isPast ? ColGreen : (isToday ? ColAmber : ColMuted);

                GameObject row = CreatePanel(_debtListRoot, $"DebtDay_{dayNum}", isToday ? ColCardHighlight : ColCardBg, 0f, 38f);
                HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
                h.padding = new RectOffset(12, 12, 4, 4);
                CreateText(row.transform, $"Ngày {dayNum}", 14, isToday ? FontStyle.Bold : FontStyle.Normal, ColCream);
                CreateText(row.transform, $"{schedule[i]:N0} xu", 14, FontStyle.Bold, ColBrass);
                CreateText(row.transform, status, 13, FontStyle.Bold, col);
            }

            _payDebtBtn.interactable = !s.IsDebtPaidToday;
        }

        private void SwitchLedgerTab(bool showDebt)
        {
            _invTabContent.SetActive(!showDebt);
            _debtTabContent.SetActive(showDebt);
            _tabInvBg.color = !showDebt ? ColBrass : ColCardBg;
            _tabDebtBg.color = showDebt ? ColBrass : ColCardBg;
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

            BuildTopBar(canvasGo.transform);
            BuildCrosshairAndPrompt(canvasGo.transform);
            BuildToastBanner(canvasGo.transform);
            BuildCounterModal(canvasGo.transform);
            BuildInspectModal(canvasGo.transform);
            BuildLedgerModal(canvasGo.transform);
            BuildRunEndModal(canvasGo.transform);
        }

        private void BuildTopBar(Transform parent)
        {
            GameObject bar = CreatePanel(parent, "TopStatusBar", ColPanelDark, 0f, 52f);
            RectTransform rt = bar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 52f);

            HorizontalLayoutGroup h = bar.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(24, 24, 8, 8);
            h.spacing = 24f;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;

            _dayText = CreateSingleLineText(bar.transform, "NGÀY 1/7", 18, FontStyle.Bold, ColBrass);
            _cashText = CreateSingleLineText(bar.transform, "1,000 xu", 18, FontStyle.Bold, ColCream);
            _cashDeltaText = CreateSingleLineText(bar.transform, "+0 xu", 16, FontStyle.Bold, ColGreen);
            _cashDeltaText.gameObject.SetActive(false);

            _debtText = CreateSingleLineText(bar.transform, "Nợ hôm nay: 200 xu", 17, FontStyle.Bold, ColAmber);
            _boxesText = CreateSingleLineText(bar.transform, "Hộp: 0/3", 17, FontStyle.Bold, ColCream);

            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(bar.transform, false);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            CreateSingleLineText(bar.transform, "[Tab] Sổ Tay & Kho  |  [E] Tương Tác  |  [Esc] Đóng / Chuột", 14, FontStyle.Normal, ColMuted);
        }

        private void BuildCrosshairAndPrompt(Transform parent)
        {
            GameObject dot = new GameObject("Crosshair");
            dot.transform.SetParent(parent, false);
            Image dotImg = dot.AddComponent<Image>();
            dotImg.color = new Color(1f, 0.92f, 0.72f, 0.85f);
            RectTransform dotRt = dot.GetComponent<RectTransform>();
            dotRt.anchorMin = dotRt.anchorMax = new Vector2(0.5f, 0.5f);
            dotRt.sizeDelta = new Vector2(6f, 6f);

            _promptPill = CreatePanel(parent, "InteractPrompt", ColPanelDark, 620f, 46f);
            RectTransform rt = _promptPill.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.14f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(620f, 46f);
            AddOutline(_promptPill, ColBrass);

            _promptText = CreateText(_promptPill.transform, "[E] Tương tác", 17, FontStyle.Bold, ColCream);
            _promptText.alignment = TextAnchor.MiddleCenter;
            RectTransform trt = _promptText.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            _promptPill.SetActive(false);
        }

        private void BuildToastBanner(Transform parent)
        {
            _toastBanner = CreatePanel(parent, "ToastBanner", ColGreen, 780f, 50f);
            RectTransform rt = _toastBanner.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.88f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(780f, 50f);
            _toastBg = _toastBanner.GetComponent<Image>();
            AddOutline(_toastBanner, ColBrass);

            _toastText = CreateText(_toastBanner.transform, string.Empty, 16, FontStyle.Bold, ColCream);
            _toastText.alignment = TextAnchor.MiddleCenter;
            RectTransform trt = _toastText.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(14f, 4f);
            trt.offsetMax = new Vector2(-14f, -4f);
            _toastBanner.SetActive(false);
        }

        private void BuildCounterModal(Transform parent)
        {
            _counterModal = CreatePanel(parent, "CounterModal", ColPanelDark, 780f, 560f);
            CenterRect(_counterModal, 780f, 560f);
            AddOutline(_counterModal, ColBrass);

            VerticalLayoutGroup v = _counterModal.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(22, 22, 18, 18);
            v.spacing = 12f;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;

            CreateHeaderRow(_counterModal.transform, "QUẦY NIÊM PHONG — DÌ MẬN", CloseAllModals);
            _counterQuotaText = CreateText(_counterModal.transform, "Lượt mua hôm nay: 0/3", 15, FontStyle.Bold, ColAmber);

            GameObject listObj = new GameObject("OfferList");
            listObj.transform.SetParent(_counterModal.transform, false);
            VerticalLayoutGroup lv = listObj.AddComponent<VerticalLayoutGroup>();
            lv.spacing = 10f;
            lv.childControlWidth = true;
            lv.childControlHeight = true;
            lv.childForceExpandHeight = false;
            _counterListRoot = listObj.transform;

            _counterModal.SetActive(false);
        }

        private void BuildInspectModal(Transform parent)
        {
            _inspectModal = CreatePanel(parent, "InspectModal", ColPanelDark, 430f, 580f);
            RectTransform rt = _inspectModal.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-32f, -12f);
            rt.sizeDelta = new Vector2(430f, 580f);
            AddOutline(_inspectModal, ColBrass);

            VerticalLayoutGroup v = _inspectModal.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(22, 22, 18, 18);
            v.spacing = 10f;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;

            _insCategory = CreateText(_inspectModal.transform, "NHÓM: CỔ VẬT", 13, FontStyle.Bold, ColBrass);
            _insName = CreateText(_inspectModal.transform, "Tên Hiện Vật", 22, FontStyle.Bold, ColCream);

            GameObject statsCard = CreatePanel(_inspectModal.transform, "StatsCard", ColCardBg, 0f, 98f);
            VerticalLayoutGroup sv = statsCard.AddComponent<VerticalLayoutGroup>();
            sv.padding = new RectOffset(12, 12, 10, 10);
            sv.spacing = 6f;
            sv.childControlWidth = true;
            sv.childControlHeight = true;
            sv.childForceExpandHeight = false;
            _insQuality = CreateText(statsCard.transform, "Tình trạng: Tốt", 15, FontStyle.Bold, ColCream);
            _insAuth = CreateText(statsCard.transform, "Xác thực: Thật", 15, FontStyle.Bold, ColGreen);
            _insRarity = CreateText(statsCard.transform, "Độ hiếm: Hiếm", 15, FontStyle.Bold, ColBrass);

            CreateText(_inspectModal.transform, "Dấu hiệu quan sát:", 14, FontStyle.Bold, ColMuted);
            _insClue = CreateText(_inspectModal.transform, "...", 15, FontStyle.Italic, ColCream);
            _insClue.GetComponent<LayoutElement>().minHeight = 56f;

            _insPrice = CreateText(_inspectModal.transform, "Giá thu mua: 0 xu", 20, FontStyle.Bold, ColBrass);

            CreateText(_inspectModal.transform, "Kéo chuột trái: Xoay 360°  |  Cuộn chuột: Zoom", 13, FontStyle.Normal, ColMuted);

            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(_inspectModal.transform, false);
            spacer.AddComponent<LayoutElement>().flexibleHeight = 1f;

            _insSellBtn = CreateButton(_inspectModal.transform, "Bán ngay", ColGreen, 0f, 48f, null);
            _insKeepBtn = CreateButton(_inspectModal.transform, "Cất vào kho", new Color(0.24f, 0.34f, 0.48f), 0f, 44f, null);
            CreateButton(_inspectModal.transform, "Đặt xuống bàn [Esc]", ColCardBg, 0f, 38f, LeaveArtifactOnTable);

            _inspectModal.SetActive(false);
        }

        private void BuildLedgerModal(Transform parent)
        {
            _ledgerModal = CreatePanel(parent, "LedgerModal", ColPanelDark, 780f, 560f);
            CenterRect(_ledgerModal, 780f, 560f);
            AddOutline(_ledgerModal, ColBrass);

            VerticalLayoutGroup v = _ledgerModal.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(22, 22, 18, 18);
            v.spacing = 12f;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;

            CreateHeaderRow(_ledgerModal.transform, "SỔ TAY & PHÒNG TRỌ", CloseAllModals);

            // Tab Bar
            GameObject tabBar = new GameObject("TabBar");
            tabBar.transform.SetParent(_ledgerModal.transform, false);
            tabBar.AddComponent<LayoutElement>().preferredHeight = 40f;
            HorizontalLayoutGroup th = tabBar.AddComponent<HorizontalLayoutGroup>();
            th.spacing = 10f;
            th.childControlWidth = true;
            th.childControlHeight = true;

            Button tabInvBtn = CreateButton(tabBar.transform, "Kho Hiện Vật", ColBrass, 0f, 40f, () => SwitchLedgerTab(false));
            Button tabDebtBtn = CreateButton(tabBar.transform, "Thu Nợ (Anh Bảy) & Qua Ngày", ColCardBg, 0f, 40f, () => SwitchLedgerTab(true));
            _tabInvBg = tabInvBtn.GetComponent<Image>();
            _tabDebtBg = tabDebtBtn.GetComponent<Image>();

            // Inventory Tab
            _invTabContent = new GameObject("InventoryTab");
            _invTabContent.transform.SetParent(_ledgerModal.transform, false);
            _invTabContent.AddComponent<LayoutElement>().preferredHeight = 390f;
            VerticalLayoutGroup iv = _invTabContent.AddComponent<VerticalLayoutGroup>();
            iv.spacing = 8f;
            iv.childControlWidth = true;
            iv.childControlHeight = true;
            iv.childForceExpandHeight = false;
            _invListRoot = _invTabContent.transform;

            // Debt Tab
            _debtTabContent = new GameObject("DebtTab");
            _debtTabContent.transform.SetParent(_ledgerModal.transform, false);
            _debtTabContent.AddComponent<LayoutElement>().preferredHeight = 390f;
            VerticalLayoutGroup dv = _debtTabContent.AddComponent<VerticalLayoutGroup>();
            dv.spacing = 8f;
            dv.childControlWidth = true;
            dv.childControlHeight = true;
            dv.childForceExpandHeight = false;

            _debtSummaryText = CreateText(_debtTabContent.transform, "...", 15, FontStyle.Bold, ColAmber);

            GameObject debtRows = new GameObject("DebtRows");
            debtRows.transform.SetParent(_debtTabContent.transform, false);
            VerticalLayoutGroup drv = debtRows.AddComponent<VerticalLayoutGroup>();
            drv.spacing = 4f;
            drv.childControlWidth = true;
            drv.childControlHeight = true;
            drv.childForceExpandHeight = false;
            _debtListRoot = debtRows.transform;

            GameObject debtActions = new GameObject("DebtActions");
            debtActions.transform.SetParent(_debtTabContent.transform, false);
            debtActions.AddComponent<LayoutElement>().preferredHeight = 48f;
            HorizontalLayoutGroup dah = debtActions.AddComponent<HorizontalLayoutGroup>();
            dah.spacing = 12f;
            dah.childControlWidth = true;
            dah.childControlHeight = true;

            _payDebtBtn = CreateButton(debtActions.transform, "Trả Nợ Hôm Nay", ColGreen, 0f, 46f, () => marketService?.RequestPayDebt());
            _endDayBtn = CreateButton(debtActions.transform, "Kết Thúc Ngày", ColBrass, 0f, 46f, () => marketService?.RequestEndDay());

            _ledgerModal.SetActive(false);
        }

        private void BuildRunEndModal(Transform parent)
        {
            _runEndModal = CreatePanel(parent, "RunEndModal", ColPanelDark, 560f, 300f);
            CenterRect(_runEndModal, 560f, 300f);
            AddOutline(_runEndModal, ColBrass);

            VerticalLayoutGroup v = _runEndModal.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(28, 28, 28, 28);
            v.spacing = 18f;
            v.childAlignment = TextAnchor.MiddleCenter;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;

            _runEndTitle = CreateText(_runEndModal.transform, "KẾT THÚC VÁN", 28, FontStyle.Bold, ColBrass);
            _runEndTitle.alignment = TextAnchor.MiddleCenter;

            _runEndBody = CreateText(_runEndModal.transform, "...", 17, FontStyle.Normal, ColCream);
            _runEndBody.alignment = TextAnchor.MiddleCenter;

            CreateButton(_runEndModal.transform, "Chơi Lại Ván Mới", ColGreen, 220f, 48f, () =>
            {
                marketService?.ResetRun();
                CloseAllModals();
            });

            _runEndModal.SetActive(false);
        }

        private HorizontalLayoutGroup CreateHeaderRow(Transform parent, string title, UnityEngine.Events.UnityAction onClose)
        {
            GameObject row = new GameObject("Header");
            row.transform.SetParent(parent, false);
            row.AddComponent<LayoutElement>().preferredHeight = 40f;
            HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;

            Text t = CreateText(row.transform, title, 21, FontStyle.Bold, ColBrass);
            t.GetComponent<LayoutElement>().flexibleWidth = 1f;
            CreateButton(row.transform, "Đóng [Esc]", ColRed, 110f, 36f, onClose);
            return h;
        }

        private GameObject CreatePanel(Transform parent, string name, Color bg, float w, float h)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = bg;
            LayoutElement le = go.AddComponent<LayoutElement>();
            if (w > 0f) le.preferredWidth = w;
            if (h > 0f) le.preferredHeight = h;
            return go;
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
            img.color = bg;
            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);

            LayoutElement le = go.AddComponent<LayoutElement>();
            if (w > 0f) le.preferredWidth = w;
            if (h > 0f) le.preferredHeight = h;

            Text t = CreateText(go.transform, label, 15, FontStyle.Bold, ColCream);
            t.alignment = TextAnchor.MiddleCenter;
            RectTransform trt = t.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            return btn;
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

        private static void ClearChildren(Transform root)
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
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
    }
}

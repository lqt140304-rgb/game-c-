using System.Collections.Generic;
using System.Linq;
using ChoDoLa.Gameplay;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.Sprint1
{
    public partial class Sprint1HudController
    {
        private readonly Queue<MarketRun.AchievementView> _achievementToasts = new Queue<MarketRun.AchievementView>();
        private GameObject _phoneModal;
        private Text _phoneStatus;
        private Text _phoneWallet;
        private GameObject _phoneHome;
        private GameObject _phoneAchievementList;
        private Transform _phoneAchievementRows;
        private ScrollRect _phoneAchievementScroll;
        private GameObject _achievementToast;
        private Text _achievementToastIcon;
        private Text _achievementToastName;
        private float _achievementToastUntil;

        private void BuildPhone(Transform root)
        {
            _phoneModal = new GameObject("Phone", typeof(RectTransform));
            _phoneModal.transform.SetParent(root, false);
            StretchFull(_phoneModal);
            _phoneModal.AddComponent<Image>().color = new Color(.05f, .06f, .06f, .64f);

            var phone = CreateChamferedPanel(_phoneModal.transform, "PhoneScreen", ColPaper, ColBrass, 460f, 670f, false);
            CenterRect(phone, 460f, 670f);
            var layout = phone.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 22, 22);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            CreateHeaderRow(phone.transform, "ĐIỆN THOẠI", CloseAllModals, darkInk: true);
            _phoneStatus = CreateText(phone.transform, "", 16, FontStyle.Bold, ColInkMuted);
            var pages = EquipmentStrip(phone.transform, "PhoneTabs", 42f, true);
            CreateButton(pages, "Trang chính", ColBrassDark, 138f, 40f, () => ShowPhonePage(false));
            CreateButton(pages, "Thành tựu", ColTeal, 138f, 40f, () => ShowPhonePage(true));

            _phoneHome = EquipmentStrip(phone.transform, "PhoneHome", 380f, false).gameObject;
            _phoneWallet = CreateText(_phoneHome.transform, "", 24, FontStyle.Bold, ColInk);
            _phoneAchievementList = CreateScrollArea(phone.transform, "PhoneAchievements", 440f,
                out _phoneAchievementRows, out _phoneAchievementScroll);
            _phoneAchievementList.SetActive(false);
            _phoneModal.SetActive(false);
        }

        private void OpenPhone()
        {
            if (_phoneModal == null || marketService == null || marketService.CurrentRunState.IsRunEnded) return;
            _phoneModal.SetActive(true);
            ShowPhonePage(false);
            UpdatePhone();
            SetPlayerLocked(true);
            RefreshHudVisibility();
        }

        private void ShowPhonePage(bool achievements)
        {
            _phoneHome.SetActive(!achievements);
            _phoneAchievementList.SetActive(achievements);
            if (achievements)
            {
                RefreshPhoneAchievements();
                Canvas.ForceUpdateCanvases();
                _phoneAchievementScroll.verticalNormalizedPosition = 1f;
            }
        }

        private void UpdatePhone()
        {
            if (_phoneModal == null || !_phoneModal.activeSelf || marketService == null) return;
            var state = marketService.CurrentRunState;
            var day = $"Ngày {state.CurrentDay}/{state.MaxDays}";
            _phoneStatus.text = state.TimedDays
                ? $"{day} · {state.ClockMinute / 60:D2}:{state.ClockMinute % 60:D2}"
                : day;
            _phoneWallet.text = $"{state.Cash:N0} xu";
        }

        private void RefreshPhoneAchievements()
        {
            if (_phoneAchievementRows == null || marketService == null) return;
            ClearChildren(_phoneAchievementRows);
            var achievements = marketService.Achievements;
            CreateText(_phoneAchievementRows, $"{achievements.Count(entry => entry.Unlocked)}/{achievements.Count} đã đạt",
                16, FontStyle.Bold, ColInkMuted);

            foreach (var achievement in achievements)
            {
                var card = CreateAutoCard(_phoneAchievementRows, achievement.Id,
                    achievement.Unlocked ? ColPaperHighlight : ColPaperDark, 62f);
                var row = card.AddComponent<HorizontalLayoutGroup>();
                row.spacing = 8f;
                row.padding = new RectOffset(9, 9, 8, 8);
                row.childAlignment = TextAnchor.MiddleLeft;
                row.childControlWidth = true;
                row.childControlHeight = true;
                row.childForceExpandWidth = false;
                CreateText(row.transform, achievement.Icon, 19, FontStyle.Bold,
                    achievement.Unlocked ? ColJade : ColInkMuted).GetComponent<LayoutElement>().preferredWidth = 30f;
                var info = CreateInfoColumn(row.transform);
                CreateText(info.transform, achievement.Name, 15, FontStyle.Bold, ColInk);
                CreateText(info.transform, achievement.Description, 12, FontStyle.Normal, ColInkMuted);
                CreateText(row.transform, $"{achievement.Progress}/{achievement.Target}", 14, FontStyle.Bold,
                    achievement.Unlocked ? ColJade : ColInkMuted).GetComponent<LayoutElement>().preferredWidth = 48f;
            }
        }

        private void BuildAchievementToast(Transform root)
        {
            _achievementToast = CreateChamferedPanel(root, "AchievementToast", ColWalnutLight, ColBrass, 324f, 68f, true);
            var rect = _achievementToast.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-24f, -30f);
            var group = _achievementToast.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            var row = _achievementToast.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(12, 12, 8, 8);
            row.spacing = 9f;
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            _achievementToastIcon = CreateText(row.transform, "", 24, FontStyle.Bold, ColGold);
            _achievementToastIcon.GetComponent<LayoutElement>().preferredWidth = 34f;
            var info = CreateInfoColumn(row.transform);
            CreateText(info.transform, "Thành tựu mới", 13, FontStyle.Normal, ColBrass);
            _achievementToastName = CreateText(info.transform, "", 16, FontStyle.Bold, ColCream);
            _achievementToast.SetActive(false);
        }

        private void QueueAchievement(MarketRun.AchievementView achievement)
        {
            _achievementToasts.Enqueue(achievement);
            if (_phoneModal != null && _phoneModal.activeSelf && _phoneAchievementList.activeSelf)
                RefreshPhoneAchievements();
        }

        private void UpdateAchievementToast()
        {
            if (_achievementToast == null) return;
            if (_achievementToast.activeSelf)
            {
                if (Time.unscaledTime < _achievementToastUntil) return;
                _achievementToast.SetActive(false);
            }

            if (_achievementToasts.Count == 0) return;
            var achievement = _achievementToasts.Dequeue();
            _achievementToastIcon.text = achievement.Icon;
            _achievementToastName.text = achievement.Name;
            _achievementToastUntil = Time.unscaledTime + 3.2f;
            _achievementToast.SetActive(true);
        }

        private static bool PhoneKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.P);
#endif
        }
    }
}

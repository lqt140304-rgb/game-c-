using System;
using System.Collections.Generic;
using System.Linq;
using ChoDoLa.Contracts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.Sprint1
{
    public partial class Sprint1HudController
    {
        private GameObject _equipmentModal;
        private Transform _equipmentSlots;
        private Transform _equipmentSystems;
        private Transform _equipmentCombos;
        private Transform _equipmentItems;
        private Transform _equipmentTooltip;
        private Text _equipmentBalance;
        private TactInspectionPreview _equipmentPreview;
        private string _selectedTactId;
        private bool _equipmentAtShop;

        private void OpenEquipment(bool atShop)
        {
            if (marketService == null || marketService.CurrentRunState.IsRunEnded || _equipmentModal == null) return;
            CloseAllModals();
            _equipmentAtShop = atShop;
            _selectedTactId = null;
            _equipmentModal.SetActive(true);
            RefreshEquipment();
            SetPlayerLocked(true);
            RefreshHudVisibility();
        }

        private void BuildEquipmentModal(Transform root)
        {
            _equipmentModal = new GameObject("TactEquipment");
            _equipmentModal.transform.SetParent(root, false);
            StretchFull(_equipmentModal);
            _equipmentModal.AddComponent<Image>().color = new Color(.06f, .05f, .04f, .84f);

            var sheet = CreateChamferedPanel(_equipmentModal.transform, "EquipmentSheet", ColPaper, ColBrass, 1050f, 660f, false);
            CenterRect(sheet, 1050f, 660f);
            var layout = sheet.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 22, 22);
            layout.spacing = 12;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            CreateHeaderRow(sheet.transform, "TRANG BỊ", CloseAllModals, darkInk: true);

            var body = EquipmentStrip(sheet.transform, "EquipmentBody", 450, true);
            var left = EquipmentStrip(body, "LeftColumn", 450, false, 300);
            _equipmentSlots = EquipmentStrip(left, "Slots", 295, false);
            _equipmentSystems = EquipmentStrip(left, "Systems", 40, true);
            _equipmentCombos = EquipmentStrip(left, "Combos", 40, true);
            _equipmentTooltip = EquipmentStrip(left, "ComboHover", 37, true);

            var preview = new GameObject("CharacterPreview", typeof(RectTransform));
            preview.transform.SetParent(body, false);
            preview.AddComponent<LayoutElement>().flexibleWidth = 1;
            var image = preview.AddComponent<RawImage>();
            image.color = Color.white;
            image.raycastTarget = false;
            _equipmentPreview = preview.AddComponent<TactInspectionPreview>();
            _equipmentPreview.Configure(image);

            _equipmentItems = EquipmentStrip(sheet.transform, "EquipmentBag", 45, true);
            _equipmentBalance = CreateText(sheet.transform, "", 14, FontStyle.Normal, ColInkMuted);
            _equipmentModal.SetActive(false);
        }

        private Transform EquipmentStrip(Transform parent, string name, float height, bool horizontal, float width = 0)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var element = go.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            if (width > 0) element.preferredWidth = width;
            HorizontalOrVerticalLayoutGroup layout = horizontal
                ? (HorizontalOrVerticalLayoutGroup)go.AddComponent<HorizontalLayoutGroup>()
                : go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 7;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return go.transform;
        }

        private void RefreshEquipment()
        {
            foreach (var root in new[] { _equipmentSlots, _equipmentSystems, _equipmentCombos, _equipmentTooltip, _equipmentItems })
                ClearChildren(root);

            var slots = marketService.EquippedArtifacts;
            var catalog = marketService.TactCatalog;
            for (int i = 0; i < slots.Count; i++)
            {
                int slot = i;
                var equipped = slots[i];
                var control = TactIcon(_equipmentSlots, equipped?.IconModelName, $"{i + 1}  —", 284,
                    equipped == null ? ColBrassDark : ColJade, () => EquipSelected(slot));
                if (equipped != null)
                {
                    Hover(control, () => ShowEquipmentText(equipped.DisplayName), ClearEquipmentHover);
                    AddEquipmentEvent(control, EventTriggerType.PointerClick, evt =>
                    {
                        if (((PointerEventData)evt).button == PointerEventData.InputButton.Right &&
                            marketService.RequestUnequipTactArtifact(slot)) RefreshEquipment();
                    });
                }
            }
            if (slots.Count == 4)
            {
                var seal = TactIcon(_equipmentSlots, "BK1", "◇  5", 284, ColDisabled, () => { });
                Hover(seal, () => ShowEquipmentText("BK1 · Hoàn thành một combo nhỏ trước ngày 5."), ClearEquipmentHover);
            }

            foreach (var system in new[] { (id: "co_phong", label: "Cổ Phong", symbol: "◈"), (id: "thien_co", label: "Thiên Cơ", symbol: "◇") })
            {
                marketService.TactResonance.TryGetValue(system.id, out int tier);
                var icon = TactIcon(_equipmentSystems, system.id, system.symbol, 43, tier > 0 ? ColTeal : ColDisabled, () => { });
                Hover(icon, () => ShowEquipmentText(tier > 0 ? $"{system.label} · Mốc {tier}" : system.label), ClearEquipmentHover);
            }

            foreach (var combo in marketService.ActiveCombos)
            {
                var icon = TactIcon(_equipmentCombos, combo.ComboId, combo.ComboId, 48,
                    combo.IsActive ? ColJade : ColDisabled, () => { });
                Hover(icon, () =>
                {
                    if (combo.IsActive) ShowEquipmentText(combo.EffectSummary);
                    else ShowRequiredIcons(combo.MissingTactIds);
                }, ClearEquipmentHover);
            }

            foreach (var item in catalog.Where(item => _equipmentAtShop || item.IsOwned && !item.IsEquipped))
            {
                var icon = TactIcon(_equipmentItems, item.IconModelName, item.TactId, 62,
                    _selectedTactId == item.TactId ? ColJade : item.IsOwned ? ColTeal : ColBrassDark,
                    () => SelectOrBuy(item));
                Hover(icon, () => ShowEquipmentText(item.IsOwned ? item.DisplayName : $"{item.DisplayName} · {item.Price:N0} xu"),
                    ClearEquipmentHover);
            }

            _equipmentBalance.text = _equipmentAtShop
                ? $"Kim Khuy · {marketService.CurrentRunState.Cash:N0} xu"
                : "";
        }

        private void SelectOrBuy(TactArtifactView item)
        {
            if (!item.IsOwned)
            {
                if (!_equipmentAtShop || !marketService.RequestBuyTactArtifact(item.TactId)) return;
            }
            _selectedTactId = item.TactId;
            RefreshEquipment();
        }

        private void EquipSelected(int slot)
        {
            string id = _selectedTactId;
            if (id == null || !marketService.RequestEquipTactArtifact(id, slot)) return;
            _selectedTactId = null;
            RefreshEquipment();
            _equipmentPreview.Inspect(id);
        }

        private GameObject TactIcon(Transform parent, string id, string fallback, float width, Color tint,
            UnityEngine.Events.UnityAction action)
        {
            var button = CreateButton(parent, fallback, tint, width, 41, action);
            var image = id == null ? null : Resources.Load<Sprite>($"TactEquipment/Icons/{id}");
            if (image != null)
            {
                button.GetComponentInChildren<Text>().enabled = false;
                var art = new GameObject("Icon", typeof(RectTransform));
                art.transform.SetParent(button.transform, false);
                CenterRect(art, 28, 28);
                var view = art.AddComponent<Image>();
                view.sprite = image;
                view.preserveAspect = true;
                view.raycastTarget = false;
            }
            return button.gameObject;
        }

        private void ShowRequiredIcons(IReadOnlyList<string> missing)
        {
            ClearEquipmentHover();
            foreach (var id in missing)
                TactIcon(_equipmentTooltip, id, id, 43, ColBrassDark, () => { }).GetComponent<Button>().interactable = false;
        }

        private void ShowEquipmentText(string text)
        {
            ClearEquipmentHover();
            CreateText(_equipmentTooltip, text, 14, FontStyle.Normal, ColInk);
        }

        private void ClearEquipmentHover() => ClearChildren(_equipmentTooltip);

        private static void Hover(GameObject go, Action enter, Action leave)
        {
            AddEquipmentEvent(go, EventTriggerType.PointerEnter, _ => enter());
            AddEquipmentEvent(go, EventTriggerType.PointerExit, _ => leave());
        }

        private static void AddEquipmentEvent(GameObject go, EventTriggerType type, Action<BaseEventData> handler)
        {
            var trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(evt => handler(evt));
            trigger.triggers.Add(entry);
        }

        private static bool EquipmentKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.I);
#endif
        }
    }
}

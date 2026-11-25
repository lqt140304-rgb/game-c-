using System.Collections.Generic;
using ChoDoLa.ChestFx;
using ChoDoLa.Player;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.Interaction
{
    /// <summary>
    /// Test menu: hold each chest model in first person and try its lid.
    /// [C] opens the list (mouse, number keys, or arrows + Enter), the chosen chest goes into the hand through
    /// <see cref="HeldItemViewmodel"/>. While holding: [O] open (BoxOpenSequence.Play, with the key intro if the
    /// model has a "Key" child), [R] close again, [F] inspect and [Q] put away (handled by the viewmodel),
    /// [ and ] previous / next chest, [L] flip which side counts as the front if the lid opens the wrong way.
    /// [P] opens with the full reveal effect (ChestRevealSequence + the chest's FX profile from <see cref="fxLibrary"/>,
    /// a random demo item), [T] cycles the tier: random, then Rác ... Huyền thoại forced.
    ///
    /// Each chest is wrapped before it is handed to the viewmodel: Template (inactive) / Facing (turns the front
    /// toward the camera) / chest model. The viewmodel only moves and scales the outer copy, so BoxOpenSequence,
    /// which sits on the chest model inside, keeps a stable local position for its shake.
    /// Fill the list with Tools/Cho Do La/Add Chest Hand Menu (or Refresh Chest List).
    /// </summary>
    public class ChestHandMenu : MonoBehaviour
    {
        [Tooltip("Chest models: imported .glb files or prefabs, named Chest_<Name>. Filled by Tools/Cho Do La/Refresh Chest List.")]
        public List<GameObject> chests = new List<GameObject>();

        [SerializeField] private HeldItemViewmodel viewmodel;

        [Header("Chests without their own BoxOpenSequence")]
        [Tooltip("Find the front from the Lid hinge (the hinge sits at the back of the chest).")]
        [SerializeField] private bool autoDetectFront = true;
        [Tooltip("Used when the front cannot be detected. glTF models face +Z.")]
        [SerializeField] private bool fallbackFrontIsPositiveZ = true;

        [Header("Full reveal [P] (filled by Tools/Cho Do La/Create All Chest FX Profiles)")]
        [SerializeField] private ChestFxLibrary fxLibrary;

        [Header("Screen")]
        [SerializeField] private bool showKeyHelp = true;

        private const string Prefix = "Chest_";

        private enum Act { Menu, Open, Close, Prev, Next, Up, Down, Confirm, Flip, FullOpen, Tier }

        private bool _menuOpen;
        private int _cursor;
        private int _heldIndex = -1;
        private int _pendingIndex = -1;
        private GameObject _heldCopy;          // the viewmodel's copy of our template (null once put away)
        private BoxOpenSequence _heldBox;      // BoxOpenSequence on that copy (null if the model has no Lid)
        private bool _flipFront;               // [L]: treat the detected front as the back
        private int _serial;
        private string _status = string.Empty;
        private Vector2 _listScroll;
        private int _forcedTier = -1;          // [T]: -1 random, else ChestTier rank
        private ChestRevealSequence _heldReveal;

        private FirstPersonController _controller;
        private bool _controllerWasEnabled;
        private CursorLockMode _previousLock;
        private bool _previousCursorVisible;

        private bool HoldingChest => _heldCopy != null && viewmodel != null && viewmodel.HasItem;

        // ---------------------------------------------------------------- Unity

        private void Awake()
        {
            if (viewmodel == null) viewmodel = GetComponent<HeldItemViewmodel>();
            if (viewmodel == null && Camera.main != null) viewmodel = Camera.main.GetComponentInChildren<HeldItemViewmodel>();
            _controller = transform.root.GetComponentInChildren<FirstPersonController>();
        }

        private void OnDisable()
        {
            if (_menuOpen) SetMenuOpen(false);
        }

        private void Update()
        {
            if (viewmodel == null) return;

            if (Pressed(Act.Menu)) SetMenuOpen(!_menuOpen);

            if (_menuOpen) HandleMenuKeys();
            else HandleHeldKeys();

            // A chest was chosen while something was in the hand: equip it once the hand is empty.
            if (_pendingIndex >= 0 && !viewmodel.HasItem) EquipNow(_pendingIndex);

            if (_heldCopy == null) _heldIndex = -1;
        }

        // ---------------------------------------------------------------- Input

        private void HandleMenuKeys()
        {
            int count = chests.Count;
            if (count == 0) return;

            if (Pressed(Act.Up)) _cursor = (_cursor - 1 + count) % count;
            if (Pressed(Act.Down)) _cursor = (_cursor + 1) % count;

            int digit = DigitPressed();
            if (digit >= 0 && digit < count) { ChooseFromMenu(digit); return; }

            if (Pressed(Act.Confirm)) { ChooseFromMenu(_cursor); return; }

            if (Pressed(Act.Prev)) Select(CurrentIndex() - 1);
            if (Pressed(Act.Next)) Select(CurrentIndex() + 1);
        }

        private void HandleHeldKeys()
        {
            if (chests.Count > 0)
            {
                if (Pressed(Act.Prev)) Select(CurrentIndex() - 1);
                if (Pressed(Act.Next)) Select(CurrentIndex() + 1);
            }

            if (Pressed(Act.Flip) && _heldIndex >= 0)
            {
                _flipFront = !_flipFront;
                _status = _flipFront ? "Đã đảo mặt trước (L). Bấm L lần nữa để trả lại." : "Mặt trước về mặc định.";
                Select(_heldIndex);   // re-equip with the other front
                return;
            }

            if (!HoldingChest) return;

            if (Pressed(Act.Open))
            {
                if (_heldBox == null)
                {
                    _status = "Rương này không có lớp Lid, không mở được.";
                }
                else if (!_heldBox.IsPlaying)
                {
                    _heldBox.ResetToClosed();   // so a second [O] replays from closed
                    _heldBox.Play();
                    _status = string.Empty;
                }
            }

            if (Pressed(Act.Close) && _heldBox != null)
            {
                if (_heldReveal != null) _heldReveal.ResetReveal();
                _heldBox.ResetToClosed();
            }

            if (Pressed(Act.Tier))
            {
                _forcedTier = _forcedTier + 1 >= ChestTierUtil.Count ? -1 : _forcedTier + 1;
                _status = "Bậc khi bấm P: " + TierLabel();
            }

            if (Pressed(Act.FullOpen)) PlayFullReveal();
        }

        /// <summary>[P]: open the held chest with the full reveal effect (roulette, silhouette, the chest's own FX).</summary>
        private void PlayFullReveal()
        {
            if (_heldBox == null) { _status = "Rương này không có lớp Lid, không mở được."; return; }
            if (fxLibrary == null)
            {
                _status = "Chưa có thư viện hiệu ứng: chạy Tools/Cho Do La/Create All Chest FX Profiles (khi cảnh đang mở), rồi Play lại.";
                return;
            }
            if (_heldReveal != null && _heldReveal.IsPlaying) return;

            ChestTier tier = _forcedTier >= 0 ? (ChestTier)_forcedTier : (ChestTier)Random.Range(0, ChestTierUtil.Count);
            GameObject item = fxLibrary.RandomItem();
            Transform rig = _heldBox.transform;
            float scale = Mathf.Max(rig.lossyScale.x, Mathf.Max(rig.lossyScale.y, rig.lossyScale.z));   // the hand shrinks the chest
            _heldReveal = fxLibrary.PlayOn(rig, chests[_heldIndex].name, tier, item, scale, out string message);
            _status = $"Mở đầy đủ: bậc {tier.DisplayName()}, món {(item != null ? item.name : "(hình cầu)")}." + (string.IsNullOrEmpty(message) ? string.Empty : "\n" + message);
        }

        private string TierLabel() => _forcedTier < 0 ? "ngẫu nhiên" : ((ChestTier)_forcedTier).DisplayName();

        private int CurrentIndex()
        {
            return _heldIndex >= 0 ? _heldIndex : _cursor;
        }

        // ---------------------------------------------------------------- Menu

        private void SetMenuOpen(bool open)
        {
            if (open == _menuOpen) return;
            _menuOpen = open;

            if (open)
            {
                _cursor = Mathf.Clamp(CurrentIndex(), 0, Mathf.Max(0, chests.Count - 1));
                _previousLock = Cursor.lockState;
                _previousCursorVisible = Cursor.visible;

                // Stop mouse look and walking (arrows move the player) while the menu is open.
                // FirstPersonController.OnDisable frees the cursor so the list can be clicked.
                if (_controller != null && _controller.enabled)
                {
                    _controllerWasEnabled = true;
                    _controller.enabled = false;
                }
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (_controllerWasEnabled && _controller != null)
                {
                    _controller.enabled = true;   // OnEnable locks the cursor again
                }
                else
                {
                    Cursor.lockState = _previousLock;
                    Cursor.visible = _previousCursorVisible;
                }
                _controllerWasEnabled = false;
            }
        }

        private void ChooseFromMenu(int index)
        {
            Select(index);
            SetMenuOpen(false);
        }

        /// <summary>Put away whatever is in the hand (if anything) and then hold chest <paramref name="index"/>.</summary>
        private void Select(int index)
        {
            int count = chests.Count;
            if (count == 0 || viewmodel == null) return;

            index = ((index % count) + count) % count;
            _cursor = index;
            _pendingIndex = index;

            if (viewmodel.HasItem) viewmodel.PutAway();   // Update equips the chest once the hand is empty
            else EquipNow(index);
        }

        // ---------------------------------------------------------------- Equip

        private void EquipNow(int index)
        {
            _pendingIndex = -1;
            if (index < 0 || index >= chests.Count) return;

            GameObject asset = chests[index];
            if (asset == null)
            {
                _status = $"Ô số {index + 1} trống (file chưa nhập được). Chạy Tools/Cho Do La/Refresh Chest List.";
                return;
            }

            string templateName = "ChestHeldTemplate_" + (++_serial);
            GameObject template = new GameObject(templateName);
            template.SetActive(false);   // keeps Awake from running on the template; it runs on the held copy
            Transform facing = new GameObject("Facing").transform;
            facing.SetParent(template.transform, false);

            GameObject model = Instantiate(asset, facing, false);
            model.name = asset.name;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            BoxOpenSequence box = model.GetComponentInChildren<BoxOpenSequence>(true);
            Vector3 frontInFacing;
            if (box != null)
            {
                // A prefab with its own BoxOpenSequence: trust its settings.
                frontInFacing = facing.InverseTransformDirection(box.transform.TransformDirection(box.FrontDirection));
                if (_flipFront) frontInFacing = -frontInFacing;
            }
            else
            {
                Transform chestRoot = FindChestRoot(model.transform);
                if (chestRoot == null)
                {
                    _status = $"{DisplayName(asset)}: không tìm thấy lớp Lid, chỉ cầm được, không mở nắp.";
                    frontInFacing = fallbackFrontIsPositiveZ ? Vector3.forward : Vector3.back;
                    if (_flipFront) frontInFacing = -frontInFacing;
                }
                else
                {
                    bool frontPositiveZ = DetectFrontPositiveZ(chestRoot);
                    if (_flipFront) frontPositiveZ = !frontPositiveZ;
                    box = chestRoot.gameObject.AddComponent<BoxOpenSequence>();
                    box.FrontDirection = frontPositiveZ ? Vector3.forward : Vector3.back;
                    // Positive angles about +X swing +Z down, so a lid reaching toward +Z opens with -1.
                    box.LidOpenSign = frontPositiveZ ? -1f : 1f;
                    frontInFacing = facing.InverseTransformDirection(chestRoot.TransformDirection(box.FrontDirection));
                }
            }

            // Turn the chest round its vertical axis so the front (the lock) faces the camera (-Z of the hand).
            float yaw = Mathf.Atan2(frontInFacing.x, frontInFacing.z) * Mathf.Rad2Deg;
            facing.localRotation = Quaternion.Euler(0f, 180f - yaw, 0f);

            viewmodel.Equip(template, ItemId(asset));
            Destroy(template);

            _heldCopy = FindChild(viewmodel.transform, templateName + "(Clone)");
            _heldBox = _heldCopy != null ? _heldCopy.GetComponentInChildren<BoxOpenSequence>(true) : null;
            _heldReveal = null;
            _heldIndex = _heldCopy != null ? index : -1;
            if (_heldCopy == null) _status = "Không cầm được rương (tay đang bận?).";
        }

        /// <summary>The node whose direct children are Body / Lid / Glow (BoxOpenSequence looks them up by name).</summary>
        private static Transform FindChestRoot(Transform model)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(model);
            while (queue.Count > 0)
            {
                Transform t = queue.Dequeue();
                if (t.Find("Lid") != null) return t;
                for (int i = 0; i < t.childCount; i++) queue.Enqueue(t.GetChild(i));
            }
            return null;
        }

        /// <summary>
        /// The lid's pivot sits on the hinge at the back, so the front is on the other side of the body centre.
        /// A Key or Seal sits at the front. Falls back to the inspector setting.
        /// </summary>
        private bool DetectFrontPositiveZ(Transform chestRoot)
        {
            if (!autoDetectFront) return fallbackFrontIsPositiveZ;

            float centreZ = BodyCentreZ(chestRoot);

            Transform lid = chestRoot.Find("Lid");
            if (lid != null && Mathf.Abs(lid.localPosition.z - centreZ) > 0.01f)
            {
                return lid.localPosition.z < centreZ;
            }

            Transform marker = chestRoot.Find("Key");
            if (marker == null) marker = chestRoot.Find("Seal");
            if (marker != null && Mathf.Abs(marker.localPosition.z - centreZ) > 0.01f)
            {
                return marker.localPosition.z > centreZ;
            }

            return fallbackFrontIsPositiveZ;
        }

        /// <summary>Z of the Body's mesh bounds centre in the chest root's space (0 if there is no Body mesh).</summary>
        private static float BodyCentreZ(Transform chestRoot)
        {
            Transform body = chestRoot.Find("Body");
            if (body == null) return 0f;

            bool any = false;
            float min = 0f, max = 0f;
            foreach (MeshFilter filter in body.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null) continue;
                Bounds b = filter.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                    float z = chestRoot.InverseTransformPoint(filter.transform.TransformPoint(corner)).z;
                    if (!any) { min = max = z; any = true; }
                    else { min = Mathf.Min(min, z); max = Mathf.Max(max, z); }
                }
            }
            return any ? (min + max) * 0.5f : 0f;
        }

        private static GameObject FindChild(Transform parent, string name)
        {
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name) return t.gameObject;
            }
            return null;
        }

        private static string DisplayName(GameObject asset)
        {
            if (asset == null) return "(trống)";
            string n = asset.name;
            return n.StartsWith(Prefix, System.StringComparison.Ordinal) ? n.Substring(Prefix.Length) : n;
        }

        /// <summary>Id given to HeldItemProfile: always starts with "Chest_" so the chest preset is used.</summary>
        private static string ItemId(GameObject asset)
        {
            string n = asset.name;
            return n.StartsWith(Prefix, System.StringComparison.Ordinal) ? n : Prefix + n;
        }

        // ---------------------------------------------------------------- GUI

        private void OnGUI()
        {
            if (showKeyHelp) DrawHelp();
            if (_menuOpen) DrawMenu();
        }

        private void DrawHelp()
        {
            const float width = 330f;
            string holding = _heldIndex >= 0 && _heldIndex < chests.Count
                ? $"Đang cầm: {DisplayName(chests[_heldIndex])} ({_heldIndex + 1}/{chests.Count})"
                : $"Chưa cầm rương ({chests.Count} rương)";
            string text = holding + "\n"
                + "[C] Menu rương    [ / ] Rương trước / sau\n"
                + "[O] Mở nắp    [R] Đóng lại    [F] Soi    [Q] Cất\n"
                + "[L] Đảo mặt trước (nếu nắp mở ngược)\n"
                + $"[P] Mở đầy đủ có hiệu ứng    [T] Bậc: {TierLabel()}";
            if (_flipFront) text += "\n(đang đảo mặt trước)";
            if (!string.IsNullOrEmpty(_status)) text += "\n" + _status;

            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                wordWrap = true,
                padding = new RectOffset(10, 10, 8, 8),
                normal = { textColor = Color.white }
            };
            float height = style.CalcHeight(new GUIContent(text), width);
            GUI.Box(new Rect(Screen.width - width - 12f, 12f, width, height), text, style);
        }

        private void DrawMenu()
        {
            const float width = 300f;
            const float rowHeight = 30f;
            float listHeight = Mathf.Max(rowHeight, chests.Count * (rowHeight + 2f));
            float height = Mathf.Min(Screen.height - 80f, listHeight + 90f);
            Rect area = new Rect(20f, 40f, width, height);

            GUI.Box(area, GUIContent.none);
            GUILayout.BeginArea(new Rect(area.x + 8f, area.y + 8f, area.width - 16f, area.height - 16f));

            GUIStyle title = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            GUILayout.Label("Chọn rương để cầm", title);
            GUILayout.Label("Chuột, phím số, hoặc ↑ ↓ rồi Enter. [C] đóng.");

            if (chests.Count == 0)
            {
                GUILayout.Label("Danh sách trống. Chạy Tools/Cho Do La/Refresh Chest List.");
            }

            _listScroll = GUILayout.BeginScrollView(_listScroll);
            Color normal = GUI.backgroundColor;
            for (int i = 0; i < chests.Count; i++)
            {
                string key = i < 9 ? (i + 1).ToString() : (i == 9 ? "0" : " ");
                string label = $"{key}.  {DisplayName(chests[i])}";
                if (i == _heldIndex) label += "   (đang cầm)";
                GUI.backgroundColor = i == _cursor ? new Color(1f, 0.85f, 0.35f) : normal;
                if (GUILayout.Button(label, GUILayout.Height(rowHeight)))
                {
                    _cursor = i;
                    ChooseFromMenu(i);
                }
            }
            GUI.backgroundColor = normal;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // ---------------------------------------------------------------- Keys

        private static bool Pressed(Act act)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard k = Keyboard.current;
            if (k == null) return false;
            switch (act)
            {
                case Act.Menu: return k.cKey.wasPressedThisFrame;
                case Act.Open: return k.oKey.wasPressedThisFrame;
                case Act.Close: return k.rKey.wasPressedThisFrame;
                case Act.Prev: return k.leftBracketKey.wasPressedThisFrame;
                case Act.Next: return k.rightBracketKey.wasPressedThisFrame;
                case Act.Up: return k.upArrowKey.wasPressedThisFrame;
                case Act.Down: return k.downArrowKey.wasPressedThisFrame;
                case Act.Confirm: return k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame;
                case Act.Flip: return k.lKey.wasPressedThisFrame;
                case Act.FullOpen: return k.pKey.wasPressedThisFrame;
                case Act.Tier: return k.tKey.wasPressedThisFrame;
            }
            return false;
#else
            switch (act)
            {
                case Act.Menu: return Input.GetKeyDown(KeyCode.C);
                case Act.Open: return Input.GetKeyDown(KeyCode.O);
                case Act.Close: return Input.GetKeyDown(KeyCode.R);
                case Act.Prev: return Input.GetKeyDown(KeyCode.LeftBracket);
                case Act.Next: return Input.GetKeyDown(KeyCode.RightBracket);
                case Act.Up: return Input.GetKeyDown(KeyCode.UpArrow);
                case Act.Down: return Input.GetKeyDown(KeyCode.DownArrow);
                case Act.Confirm: return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
                case Act.Flip: return Input.GetKeyDown(KeyCode.L);
                case Act.FullOpen: return Input.GetKeyDown(KeyCode.P);
                case Act.Tier: return Input.GetKeyDown(KeyCode.T);
            }
            return false;
#endif
        }

        /// <summary>0 for key 1 ... 8 for key 9, 9 for key 0; -1 if no digit was pressed.</summary>
        private static int DigitPressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard k = Keyboard.current;
            if (k == null) return -1;
            Key[] keys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0 };
            for (int i = 0; i < keys.Length; i++)
            {
                if (k[keys[i]].wasPressedThisFrame) return i;
            }
            return -1;
#else
            KeyCode[] keys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKeyDown(keys[i])) return i;
            }
            return -1;
#endif
        }
    }
}

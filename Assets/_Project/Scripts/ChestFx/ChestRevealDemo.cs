using System.Collections.Generic;
using ChoDoLa.Interaction;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Test bench for the chest reveal (created by Tools/Cho Do La/Add Chest Reveal Test).
    /// T = next forced tier, Y = random demo item, Space = open with the effect, R = close, C = next chest model.
    /// The tier is chosen HERE (stands in for the game's seeded roll) and passed to ChestRevealSequence.Play.
    /// Items and chests are references filled in by the editor menu: nothing is scanned at runtime.
    /// </summary>
    public class ChestRevealDemo : MonoBehaviour
    {
        [SerializeField] private ChestRevealSequence reveal;
        [Tooltip("Demo items (prefab assets of the low-poly .glb items).")]
        [SerializeField] private List<GameObject> items = new List<GameObject>();
        [Tooltip("Chest models (Chest_*.glb) for the C key.")]
        [SerializeField] private List<GameObject> chests = new List<GameObject>();
        [Tooltip("FX profiles; the one whose chestId matches the chest model name is used.")]
        [SerializeField] private List<ChestFxProfile> profiles = new List<ChestFxProfile>();
        [SerializeField] private List<ChestTier> possibleTiers = new List<ChestTier> { ChestTier.Rac, ChestTier.Thuong, ChestTier.Hiem, ChestTier.RatHiem, ChestTier.HuyenThoai };
        [SerializeField] private ChestTier forcedTier = ChestTier.Hiem;
        [SerializeField] private int itemIndex;
        [SerializeField] private int chestIndex;

        private bool _requireCoin;
        private int _dropped;
        private string _status = "Sẵn sàng";
        private GUIStyle _style;

        private void Start()
        {
            if (reveal == null) reveal = FindFirstObjectByType<ChestRevealSequence>();
            Hook();
            if (reveal != null) reveal.Setup(possibleTiers);
        }

        private void Hook()
        {
            if (reveal == null) return;
            reveal.onRoulette.AddListener(() => _status = "Đang quay màu...");
            reveal.onTierLocked.AddListener(t => _status = "Chốt: " + t.DisplayName());
            reveal.onSilhouetteComplete.AddListener(() => _status = "Bóng đã hoàn chỉnh (" + reveal.RolledTier.DisplayName() + ")");
            reveal.onRevealed.AddListener(() => _status = "Xong: " + reveal.RolledTier.DisplayName() + ". R để đóng.");
        }

        private void Update()
        {
            if (Pressed(KeyCodeT())) { forcedTier = (ChestTier)(((int)forcedTier + 1) % ChestTierUtil.Count); }
            if (Pressed(KeyCodeY()) && items.Count > 0) { itemIndex = Random.Range(0, items.Count); }
            if (Pressed(KeyCodeG())) { _requireCoin = !_requireCoin; _status = _requireCoin ? "Cổng đồng xu BẬT: Space rồi K thả xu" : "Cổng đồng xu tắt"; }
            if (Pressed(KeyCodeSpace()) && reveal != null)
            {
                reveal.ResetReveal();
                PrepareGate();
                reveal.Play(forcedTier, CurrentItem());
            }
            if (Pressed(KeyCodeK()) && reveal != null) DropOutsideItem("gold_coin", new Color(1f, 0.82f, 0.25f));
            if (Pressed(KeyCodeL()) && reveal != null) DropOutsideItem("battery", new Color(0.3f, 0.9f, 0.4f));
            if (Pressed(KeyCodeR()) && reveal != null) { reveal.ResetReveal(); _status = "Đã đóng"; }
            if (Pressed(KeyCodeC()) && chests.Count > 1) SwapChest();
        }

        /// <summary>Bật/tắt bước cổng của kịch bản Golden và bảo đảm rương có khe nhận (mã "coin", cần một "gold_coin").</summary>
        private void PrepareGate()
        {
            reveal.SetStepEnabled("cong-xu", _requireCoin);
            if (!_requireCoin) return;
            bool has = false;
            foreach (ChestItemSlot slot in reveal.GetComponentsInChildren<ChestItemSlot>(true)) if (slot.gateId == "coin") has = true;
            if (has) return;
            ChestItemSlot added = reveal.gameObject.AddComponent<ChestItemSlot>();
            added.gateId = "coin";
            added.requiredItems = new List<string> { "gold_coin" };
        }

        /// <summary>Thả một vật NGOÀI có Rigidbody từ trên miệng rương: đúng mã thì mở khoá, sai mã bị đẩy ra.</summary>
        private void DropOutsideItem(string id, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(id == "gold_coin" ? PrimitiveType.Cylinder : PrimitiveType.Cube);
            go.name = "Demo_" + id;
            go.transform.localScale = id == "gold_coin" ? new Vector3(0.08f, 0.006f, 0.08f) : new Vector3(0.06f, 0.1f, 0.06f);
            go.transform.position = reveal.MouthPosition + Vector3.up * (0.35f + 0.05f * (_dropped++ % 3));
            Renderer r = go.GetComponent<Renderer>();
            r.material.color = color;
            go.AddComponent<Rigidbody>();
            go.AddComponent<ChestSlotItem>().itemId = id;
            Destroy(go, 10f);
            _status = "Thả " + id;
        }

        private GameObject CurrentItem() => items.Count > 0 ? items[Mathf.Clamp(itemIndex, 0, items.Count - 1)] : null;

        /// <summary>Replace the chest with the next model and rebuild BoxOpenSequence + ChestRevealSequence on it.</summary>
        private void SwapChest()
        {
            chestIndex = (chestIndex + 1) % chests.Count;
            GameObject prefab = chests[chestIndex];
            if (prefab == null) return;

            Transform oldRoot = reveal != null ? reveal.transform.root : null;
            Vector3 pos = oldRoot != null ? oldRoot.position : transform.position;
            Quaternion rot = oldRoot != null ? oldRoot.rotation : transform.rotation;
            if (reveal != null) reveal.ResetReveal(false);
            if (oldRoot != null && oldRoot != transform.root) Destroy(oldRoot.gameObject);

            GameObject chest = Instantiate(prefab, pos, rot);
            chest.name = prefab.name;
            Transform rig = FindRig(chest.transform);
            if (rig.GetComponent<BoxOpenSequence>() == null) rig.gameObject.AddComponent<BoxOpenSequence>();
            reveal = rig.gameObject.AddComponent<ChestRevealSequence>();
            reveal.SetProfile(ProfileFor(prefab.name));
            reveal.Setup(possibleTiers);
            if (prefab.name.Contains("Golden")) rig.gameObject.AddComponent<GoldenGlintHook>();
            Hook();
            _status = "Rương: " + prefab.name;
        }

        private ChestFxProfile ProfileFor(string chestName)
        {
            foreach (ChestFxProfile p in profiles)
            {
                if (p != null && chestName.StartsWith(p.chestId)) return p;
            }
            return null;
        }

        /// <summary>The node that owns the "Lid" child (glTF imports wrap the model in an extra root).</summary>
        public static Transform FindRig(Transform root)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                Transform t = queue.Dequeue();
                if (t.Find("Lid") != null) return t;
                foreach (Transform child in t) queue.Enqueue(child);
            }
            return root;
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 15, wordWrap = true };
                _style.normal.textColor = Color.white;
            }
            GameObject item = CurrentItem();
            string chestName = reveal != null ? reveal.transform.root.name : "(không có rương)";
            string text =
                "THỬ HIỆU ỨNG MỞ RƯƠNG\n" +
                $"[T] Bậc ép: {forcedTier.DisplayName()}  (số chấm quay = {forcedTier.Rank() + 1})\n" +
                $"[Y] Món minh hoạ: {(item != null ? item.name : "(chưa có, dùng hình cầu)")}\n" +
                $"[G] Cổng đồng xu: {(_requireCoin ? "BẬT" : "tắt")}   [K] thả xu vàng   [L] thả pin (sai)\n" +
                "[Space] Mở rương   [R] Đóng lại" + (chests.Count > 1 ? "   [C] Đổi rương" : "") + "\n" +
                $"Rương: {chestName}\n" +
                $"Trạng thái: {_status}";
            GUI.Box(new Rect(12, 12, 480, 175), text, _style);
        }

        // ------------------------------------------------------------------ Input (both input systems)

#if ENABLE_INPUT_SYSTEM
        private static Key KeyCodeT() => Key.T;
        private static Key KeyCodeY() => Key.Y;
        private static Key KeyCodeSpace() => Key.Space;
        private static Key KeyCodeR() => Key.R;
        private static Key KeyCodeC() => Key.C;
        private static Key KeyCodeG() => Key.G;
        private static Key KeyCodeK() => Key.K;
        private static Key KeyCodeL() => Key.L;
        private static bool Pressed(Key key) => Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
#else
        private static KeyCode KeyCodeT() => KeyCode.T;
        private static KeyCode KeyCodeY() => KeyCode.Y;
        private static KeyCode KeyCodeSpace() => KeyCode.Space;
        private static KeyCode KeyCodeR() => KeyCode.R;
        private static KeyCode KeyCodeC() => KeyCode.C;
        private static KeyCode KeyCodeG() => KeyCode.G;
        private static KeyCode KeyCodeK() => KeyCode.K;
        private static KeyCode KeyCodeL() => KeyCode.L;
        private static bool Pressed(KeyCode key) => Input.GetKeyDown(key);
#endif
    }
}

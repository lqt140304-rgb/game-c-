#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Chest_Golden "Máy Xèng Kho Báu": thêm GoldenSlotHook vào rương đang chọn và gán các âm trong
    /// Audio/Chests/golden/ (tạo lại bằng Tools/Audio/make_golden_slot_sfx.py). Không đụng profile chung.
    /// </summary>
    public static class GoldenSlotSetup
    {
        public const string AudioFolder = "Assets/_Project/Audio/Chests/golden/";

        [MenuItem("Tools/Cho Do La/Add Golden Slot Hook To Selected Chest")]
        public static void AddToSelected()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) { Debug.LogWarning("[GoldenSlot] Chọn gốc rương (có ChestRevealSequence) trước."); return; }
            // Hook cũ cùng loại: GoldenGlintHook và các bước xu của script cũ bắn xu hai lần (T10).
            GoldenGlintHook old = go.GetComponent<GoldenGlintHook>();
            if (old != null) Undo.DestroyObjectImmediate(old);
            GoldenSlotHook h = go.GetComponent<GoldenSlotHook>();
            if (h == null) h = go.AddComponent<GoldenSlotHook>();
            AssignClips(h);
            if (go.GetComponent<ChestRevealSequence>() == null)
                Debug.LogWarning("[GoldenSlot] " + go.name + " chưa có ChestRevealSequence: hook đã thêm nhưng sẽ không được gọi.");
            EditorUtility.SetDirty(go);
        }

        /// <summary>Gán các clip còn trống từ thư mục âm của Golden.</summary>
        public static void AssignClips(GoldenSlotHook h)
        {
            if (h.leverClip == null) h.leverClip = Clip("lever");
            if (h.spinLoopClip == null) h.spinLoopClip = Clip("spin_loop");
            if (h.reelStopClip == null) h.reelStopClip = Clip("reel_stop");
            if (h.fizzleClip == null) h.fizzleClip = Clip("fizzle");
            if (h.jackpotClip == null) h.jackpotClip = Clip("jackpot");
            if (h.coinClip == null) h.coinClip = Clip("coin");
            if (h.tokenClip == null) h.tokenClip = Clip("token");
        }

        private static AudioClip Clip(string key) => AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "sfx_golden_" + key + ".wav");
    }
}
#endif

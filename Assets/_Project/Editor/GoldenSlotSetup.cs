#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Chest_Golden "Máy Xèng Kho Báu": thêm GoldenSlotHook vào rương đang chọn và gán các âm trong
    /// Audio/Chests/golden/ (.mp3 ElevenLabs, danh mục Tools/Audio/elevenlabs_chest_manifest.json). Không đụng profile chung.
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
            ChestRevealSequence seq = go.GetComponent<ChestRevealSequence>();
            if (seq == null)
                Debug.LogWarning("[GoldenSlot] " + go.name + " chưa có ChestRevealSequence: hook đã thêm nhưng sẽ không được gọi.");
            else TuneProfile(seq.Profile);
            EditorUtility.SetDirty(go);
        }

        /// <summary>A1: hồ sơ lõi câm, tension tắt. Gọi MỖI lần chạy menu (không chỉ khi trống). Bỏ qua hồ sơ mặc định dùng chung.</summary>
        public static void TuneProfile(ChestFxProfile p)
        {
            if (p == null || p == ChestFxProfile.Default) return;
            ChestAudioRule.ApplyToProfile(p);
            EditorUtility.SetDirty(p);
        }

        /// <summary>Gán các clip còn trống từ thư mục âm của Golden (chỉ .mp3 ElevenLabs, A9).</summary>
        public static void AssignClips(GoldenSlotHook h)
        {
            if (h.leverClip == null) h.leverClip = Clip("lever");
            if (h.spinLoopClip == null) h.spinLoopClip = Clip("spin_loop");
            if (h.reelStopClip == null) h.reelStopClip = Clip("reel_stop");
            if (h.coinClip == null) h.coinClip = Clip("coin");
            if (h.tokenClip == null) h.tokenClip = Clip("token");
            // TC theo bậc (A11): Rác, Thường, Hiếm, Rất hiếm, Huyền thoại
            string[] tiers = { "lock_rac", "lock_thuong", "lock_hiem", "lock_ratHiem", "lock_huyenThoai" };
            if (h.stingClips == null || h.stingClips.Length != tiers.Length) h.stingClips = new AudioClip[tiers.Length];
            for (int i = 0; i < tiers.Length; i++)
                if (h.stingClips[i] == null) h.stingClips[i] = Clip(tiers[i]);
        }

        private static AudioClip Clip(string key) => AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "sfx_golden_" + key + ".mp3");
    }
}
#endif

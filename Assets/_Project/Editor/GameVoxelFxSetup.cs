#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Chest_Game "VOXEL 8-BIT": tạo ChestFx_GameVoxel.asset (dựa trên GameFxPreset nhưng tắt âm chốt/nền/giai đoạn để
    /// GameVoxelHook tự lo toàn bộ nhạc) và gắn GameVoxelHook + âm ElevenLabs Audio/Chests/game/ lên rương đang chọn.
    /// </summary>
    public static class GameVoxelFxSetup
    {
        private const string ProfilePath = "Assets/_Project/ScriptableObjects/ChestFx/ChestFx_GameVoxel.asset";
        private const string Folder = "Assets/_Project/Audio/Chests/game/";

        [MenuItem("Tools/Cho Do La/Game Voxel/Add Hook To Selected")]
        public static void AddToSelected()
        {
            GameObject go = Selection.activeGameObject;
            ChestRevealSequence reveal = go != null ? go.GetComponentInParent<ChestRevealSequence>() : null;
            if (reveal == null) { Debug.LogWarning("[GameVoxel] Chọn gốc rương có ChestRevealSequence trước."); return; }
            GameVoxelHook hook = reveal.GetComponent<GameVoxelHook>();
            if (hook == null) hook = reveal.gameObject.AddComponent<GameVoxelHook>();
            GamePixelHook old = reveal.GetComponent<GamePixelHook>();
            if (old != null) old.enabled = false;   // tránh chồng hiệu ứng cũ
            FillClips(hook);
            reveal.SetProfile(CreateOrUpdateProfile());
            EditorUtility.SetDirty(hook);
            EditorUtility.SetDirty(reveal);
            Debug.Log("[GameVoxel] Đã gắn GameVoxelHook + hồ sơ ChestFx_GameVoxel lên " + reveal.name);
        }

        public static ChestFxProfile CreateOrUpdateProfile()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            if (p == null)
            {
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                GameFxPreset.Apply(p);
                p.chestId = "Chest_Game";
                AssetDatabase.CreateAsset(p, ProfilePath);
            }
            ChestAudioRule.ApplyToProfile(p);   // A1: ghi lại MỖI lần chạy menu (tension tắt, ô âm lõi trống)
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            return p;
        }

        public static void FillClips(GameVoxelHook h)
        {
            h.insertCoinClip = C("press_start"); h.loopBarClip = C("chip_loop"); h.bumpClip = C("bump");
            h.jumpClip = C("jump"); h.coinClip = C("coin"); h.voxelTickClip = C("voxel_tick");
            h.shatterClip = C("shatter"); h.reassembleClip = C("reassemble");
            h.powerUpClip = C("power_up"); h.oneUpClip = C("one_up");
            h.stingClips = new[] { C("lock_rac"), C("lock_thuong"), C("lock_hiem"), C("lock_ratHiem"), C("lock_huyenThoai") };
        }

        private static AudioClip C(string n)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + "sfx_game_" + n + ".mp3");
            if (c == null) Debug.LogWarning("[GameVoxel] Thiếu sfx_game_" + n + ".mp3 (Tools/Audio/make_elevenlabs_chest_sfx.py --chest game).");
            return c;
        }
    }
}
#endif

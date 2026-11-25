#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Chest_Game "VOXEL 8-BIT": tạo ChestFx_GameVoxel.asset (dựa trên GameFxPreset nhưng tắt âm chốt/nền/giai đoạn để
    /// GameVoxelHook tự lo toàn bộ nhạc) và gắn GameVoxelHook + âm Audio/Chests/game/voxel/ lên rương đang chọn.
    /// </summary>
    public static class GameVoxelFxSetup
    {
        private const string ProfilePath = "Assets/_Project/ScriptableObjects/ChestFx/ChestFx_GameVoxel.asset";
        private const string Folder = "Assets/_Project/Audio/Chests/game/voxel/";

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
            AssignClips(hook);
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
            p.tickClips = new AudioClip[0];                         // hook phát blip voxel
            p.lockClipsByTier = new AudioClip[ChestTierUtil.Count]; // hook phát kết theo bậc
            p.openAmbientClip = null;
            p.stageClips = new ChestFxProfile.StageClip[0];
            p.accentClips = new AudioClip[0];
            if (p.whooshClip == null) p.whooshClip = AssetDatabase.LoadAssetAtPath<AudioClip>(GameFxPreset.AudioFolder + GameFxPreset.WhooshFile);
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            return p;
        }

        private static void AssignClips(GameVoxelHook h)
        {
            h.insertCoinClip = C("insert_coin"); h.countBeepClip = C("count_beep"); h.countGoClip = C("count_go");
            h.loopBarClip = C("loop_bar"); h.bumpClip = C("bump"); h.jumpClip = C("jump"); h.coinClip = C("coin");
            h.voxelTickClip = C("voxel_tick"); h.shatterClip = C("shatter"); h.reassembleClip = C("reassemble");
            h.powerUpClip = C("power_up"); h.oneUpClip = C("one_up");
            h.endClipsByTier = new[] { C("end_0_rac"), C("end_1_thuong"), C("end_2_hiem"), C("end_3_rathiem"), C("end_4_huyenthoai") };
        }

        private static AudioClip C(string n)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + "sfx_vx_" + n + ".wav");
            if (c == null) Debug.LogWarning("[GameVoxel] Thiếu " + n + " (chạy Tools/Audio/make_game_voxel_sfx.py).");
            return c;
        }
    }
}
#endif

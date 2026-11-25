#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.EditorTools
{
    /// <summary>
    /// Tools/Cho Do La/Create The Thao Chest FX Profile: creates ChestFx_TheThao.asset (TheThaoFxPreset values), silences its core audio every run
    /// (ChestAudioRule.ApplyToProfile), and if a chest object is selected adds TheThaoHook to it with all .mp3 clips (incl. stingClips[5]).
    /// </summary>
    public static class TheThaoFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_TheThao.asset";

        [MenuItem("Tools/Cho Do La/Create The Thao Chest FX Profile")]
        public static void CreateMenu()
        {
            GameObject selected = Selection.activeGameObject;
            ChestFxProfile p = CreateOrUpdate();
            if (selected != null)
            {
                TheThaoHook hook = selected.GetComponent<TheThaoHook>();
                if (hook == null) hook = selected.AddComponent<TheThaoHook>();
                int missing = TheThaoFxPreset.FillHookClips(hook, true);   // chỉ .mp3 ElevenLabs trong Audio/Chests/the_thao/
                if (missing > 0) Debug.LogWarning($"[TheThaoFx] {missing} âm .mp3 còn thiếu (ô để trống).");
                EditorUtility.SetDirty(hook);
                Debug.Log("[TheThaoFx] TheThaoHook added to " + selected.name);
            }
            Selection.activeObject = p;
        }

        public static ChestFxProfile CreateOrUpdate()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                if (!AssetDatabase.IsValidFolder(ProfileFolder)) AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "ChestFx");
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                TheThaoFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            // A1: ghi lại hồ sơ lõi gần như câm MỖI LẦN chạy menu (tension tắt, mọi ô âm lõi trống).
            // TC theo bậc và mọi âm khác do TheThaoHook phát (stingClips[5] + các ô .mp3).
            ChestAudioRule.ApplyToProfile(p);
            if (AssetDatabase.LoadAssetAtPath<AudioClip>(TheThaoFxPreset.AudioFolder + TheThaoFxPreset.LockFiles[0]) == null)
                Debug.LogWarning("[TheThaoFx] Thiếu sfx_thethao_lock_*.mp3: TC theo bậc sẽ trống.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[TheThaoFx] Created " + ProfilePath);
            return p;
        }
    }
}
#endif

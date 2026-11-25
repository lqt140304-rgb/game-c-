#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.EditorTools
{
    /// <summary>
    /// Tools/Cho Do La/Create The Thao Chest FX Profile: creates ChestFx_TheThao.asset (TheThaoFxPreset values) or fills its
    /// missing clips, and if a chest object is selected adds TheThaoHook to it with the whistle and air horn clips.
    /// Missing chest-specific mp3s fall back to the shared set in Audio/Chests/.
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
                TheThaoFxPreset.FillHookClips(hook);   // còi, đám đông, trống, cột dọc, VAR... (Audio/Chests/the_thao/match/)
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

            int missing = 0;
            p.tickClips = Fill(p.tickClips, TheThaoFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, TheThaoFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(TheThaoFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(TheThaoFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, TheThaoFxPreset.AccentFiles, null, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 3, clip = Clip(TheThaoFxPreset.LidFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip("sfx_thethao_bounce.mp3", ChestFxPresets.ThudFile, ref missing), volume = 0.6f },
                };
            }
            if (missing > 0) Debug.LogWarning($"[TheThaoFx] {missing} sound(s) not found; the effect still runs without them.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[TheThaoFx] Created " + ProfilePath);
            return p;
        }

        /// <summary>Chest mp3 from the_thao/, else the shared file in Audio/Chests/ (null = no fallback).</summary>
        private static AudioClip Clip(string file, string sharedFallback, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(TheThaoFxPreset.AudioFolder + file);
            if (c == null && sharedFallback != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + sharedFallback);
            if (c == null) missing++;
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] files, string[] shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                string fb = shared != null && i < shared.Length ? shared[i] : null;
                result[i] = existing != null ? existing : Clip(files[i], fb, ref missing);
            }
            return result;
        }
    }
}
#endif

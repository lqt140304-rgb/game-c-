#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Nhat Ban Chest FX Profile: creates ChestFx_NhatBan.asset (NhatBanFxPreset values),
    /// the prefab ChestFx_NhatBanHook.prefab (NhatBanHook) as its customFxPrefab, and fills the clips from
    /// Audio/Chests/nhat_ban/ (falls back to the shared Audio/Chests/ set, then its Synth/ placeholders).
    /// Re-running only fills what is missing.
    /// </summary>
    public static class NhatBanFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_NhatBan.asset";
        private const string HookPrefabPath = ProfileFolder + "/ChestFx_NhatBanHook.prefab";

        [MenuItem("Tools/Cho Do La/Create Nhat Ban Chest FX Profile")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdate();
        }

        public static ChestFxProfile CreateOrUpdate()
        {
            EnsureFolder(ProfileFolder);
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                NhatBanFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, NhatBanFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, NhatBanFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(NhatBanFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(NhatBanFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, NhatBanFxPreset.AccentFiles, null, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(NhatBanFxPreset.LidOpenFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(NhatBanFxPreset.LidThudFile, ChestFxPresets.ThudFile, ref missing), volume = 0.6f },
                };
            }
            if (p.customFxPrefab == null) p.customFxPrefab = HookPrefab();
            if (missing > 0) Debug.LogWarning($"[NhatBanFx] {missing} sound(s) not found; the effect still runs without them.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[NhatBanFx] Created " + ProfilePath);
            return p;
        }

        private static GameObject HookPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(HookPrefabPath);
            if (existing != null) return existing;
            GameObject temp = new GameObject("ChestFx_NhatBanHook");
            temp.AddComponent<NhatBanHook>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, HookPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        /// <summary>Own clip from nhat_ban/, else the shared one (mp3, then its Synth/ fallback).</summary>
        private static AudioClip Clip(string own, string shared, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(NhatBanFxPreset.AudioFolder + own);
            if (c != null) return c;
            if (shared != null)
            {
                c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + shared);
                string fb = ChestFxPresets.SynthFallback(shared);
                if (c == null && fb != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + fb);
            }
            if (c != null) Debug.LogWarning($"[NhatBanFx] {own} not found; using the shared sound {shared}.");
            else missing++;
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] own, string[] shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[own.Length];
            for (int i = 0; i < own.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                string fallback = shared != null && i < shared.Length ? shared[i] : null;
                result[i] = existing != null ? existing : Clip(own[i], fallback, ref missing);
            }
            return result;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            EnsureFolder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
#endif

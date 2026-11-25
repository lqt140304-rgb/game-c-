#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Co Phong Chest FX Profile: makes ChestFx_CoPhong.asset (CoPhongFxPreset values,
    /// clips from Audio/Chests/co_phong, falling back to the shared Audio/Chests set) and a small prefab carrying
    /// CoPhongHook, set as the profile's customFxPrefab. Re-running only fills what is missing.
    /// </summary>
    public static class CoPhongFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_CoPhong.asset";
        private const string HookPrefabPath = ProfileFolder + "/ChestFx_CoPhongHook.prefab";

        [MenuItem("Tools/Cho Do La/Create Co Phong Chest FX Profile")]
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
                CoPhongFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, CoPhongFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, CoPhongFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(CoPhongFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(CoPhongFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, CoPhongFxPreset.AccentFiles, null, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 1, clip = Clip(CoPhongFxPreset.SealTearFile, null, ref missing), volume = 0.6f },
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(CoPhongFxPreset.LidLiftFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(CoPhongFxPreset.LidSettleFile, ChestFxPresets.ThudFile, ref missing), volume = 0.6f },
                };
            }

            if (p.customFxPrefab == null)
            {
                GameObject go = new GameObject("ChestFx_CoPhongHook");
                go.AddComponent<CoPhongHook>();
                p.customFxPrefab = PrefabUtility.SaveAsPrefabAsset(go, HookPrefabPath);
                Object.DestroyImmediate(go);
            }

            if (missing > 0) Debug.LogWarning($"[CoPhongFx] {missing} sound(s) not found in {CoPhongFxPreset.AudioFolder} or the shared set; those slots stay empty.");
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[CoPhongFx] Created " + ProfilePath);
            return p;
        }

        /// <summary>Own clip first; else the shared mp3; else the shared synth placeholder.</summary>
        private static AudioClip Clip(string own, string shared, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(CoPhongFxPreset.AudioFolder + own);
            if (c == null && shared != null)
            {
                c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + shared);
                string synth = c == null ? ChestFxPresets.SynthFallback(shared) : null;
                if (synth != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + synth);
                if (c != null) Debug.LogWarning($"[CoPhongFx] {own} not found; using shared {shared}.");
            }
            if (c == null) missing++;
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] own, string[] shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[own.Length];
            for (int i = 0; i < own.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                result[i] = existing != null ? existing : Clip(own[i], shared != null && i < shared.Length ? shared[i] : null, ref missing);
            }
            return result;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
#endif

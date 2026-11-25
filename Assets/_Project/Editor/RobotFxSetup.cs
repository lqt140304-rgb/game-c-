#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Robot Chest FX Profile: makes ChestFx_Robot.asset with RobotFxPreset values and fills
    /// its clips by file name from Audio/Chests/robot/ (falls back to the shared set in Audio/Chests/ when a robot
    /// mp3 is missing). Existing clips are kept. Also adds RobotHook to the selected chest if one is selected.
    /// </summary>
    public static class RobotFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_Robot.asset";

        [MenuItem("Tools/Cho Do La/Create Robot Chest FX Profile")]
        public static void CreateMenu()
        {
            ChestFxProfile p = CreateOrUpdate();
            GameObject sel = Selection.activeGameObject;
            if (sel != null && sel.GetComponent<ChestRevealSequence>() != null && sel.GetComponent<RobotHook>() == null)
            {
                sel.AddComponent<RobotHook>();
                EditorUtility.SetDirty(sel);
                Debug.Log("[RobotFx] Added RobotHook to " + sel.name);
            }
            Selection.activeObject = p;
        }

        public static ChestFxProfile CreateOrUpdate()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                RobotFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, RobotFxPreset.TickFiles, ChestFxPresets.TickFiles[0], ref missing);
            AudioClip[] locks = new AudioClip[RobotFxPreset.LockFiles.Length];
            for (int i = 0; i < locks.Length; i++)
            {
                AudioClip existing = p.lockClipsByTier != null && i < p.lockClipsByTier.Length ? p.lockClipsByTier[i] : null;
                locks[i] = existing != null ? existing : Clip(RobotFxPreset.LockFiles[i], ChestFxPresets.LockFiles[i], ref missing);
            }
            p.lockClipsByTier = locks;
            if (p.whooshClip == null) p.whooshClip = Clip(RobotFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(RobotFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, RobotFxPreset.AccentFiles, null, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 0, clip = Clip(RobotFxPreset.WindupFile, null, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 1, clip = Clip(RobotFxPreset.SteamFile, null, ref missing), volume = 0.55f },
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(RobotFxPreset.ServoFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(RobotFxPreset.ServoFile, ChestFxPresets.ThudFile, ref missing), volume = 0.6f },
                };
            }
            if (missing > 0) Debug.LogWarning($"[RobotFx] {missing} sound(s) not found in {RobotFxPreset.AudioFolder} or the shared set. The effect still runs.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[RobotFx] Created " + ProfilePath);
            return p;
        }

        /// <summary>Robot mp3, else the shared clip (sharedFile relative to Audio/Chests/, may be null).</summary>
        private static AudioClip Clip(string robotFile, string sharedFile, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(RobotFxPreset.AudioFolder + robotFile);
            if (c != null) return c;
            if (sharedFile != null)
            {
                c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + sharedFile);
                if (c != null) { Debug.LogWarning($"[RobotFx] {robotFile} not found; using shared {sharedFile}."); return c; }
            }
            missing++;
            return null;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] files, string sharedFallback, ref int missing)
        {
            AudioClip[] result = new AudioClip[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                result[i] = existing != null ? existing : Clip(files[i], sharedFallback, ref missing);
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

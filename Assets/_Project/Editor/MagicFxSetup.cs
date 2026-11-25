#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Magic Chest FX Profile: creates ScriptableObjects/ChestFx/ChestFx_Magic.asset with
    /// MagicFxPreset values, fills missing clips from Audio/Chests/magic/ (falls back to the shared set in
    /// Audio/Chests/).
    /// MagicHook is NOT put on a prefab: add the MagicHook component to the chest root (next to ChestRevealSequence).
    /// </summary>
    public static class MagicFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_Magic.asset";

        [MenuItem("Tools/Cho Do La/Create Magic Chest FX Profile")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdate();
            Debug.Log("[MagicFx] Remember: add the MagicHook component on the Chest_Magic root (next to ChestRevealSequence).");
        }

        public static ChestFxProfile CreateOrUpdate()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                MagicFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, MagicFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, MagicFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(MagicFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(MagicFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, MagicFxPreset.AccentFiles, new string[MagicFxPreset.AccentFiles.Length], ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 1, clip = Clip(MagicFxPreset.WhisperFile, null, ref missing), volume = 0.5f },
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(MagicFxPreset.LidFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(MagicFxPreset.ThudFile, ChestFxPresets.ThudFile, ref missing), volume = 0.6f },
                };
            }

            if (missing > 0) Debug.LogWarning($"[MagicFx] {missing} sound(s) not found in {MagicFxPreset.AudioFolder} or the shared set. The effect still runs.");
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[MagicFx] Created " + ProfilePath);
            return p;
        }

        /// <summary>Magic clip first, else the shared clip (Audio/Chests/), else its Synth fallback.</summary>
        private static AudioClip Clip(string magicFile, string sharedFile, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(MagicFxPreset.AudioFolder + magicFile);
            if (c != null) return c;
            if (sharedFile != null)
            {
                c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + sharedFile);
                string fb = ChestFxPresets.SynthFallback(sharedFile);
                if (c == null && fb != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + fb);
                if (c != null) Debug.LogWarning($"[MagicFx] {magicFile} not found; using shared {sharedFile}.");
            }
            if (c == null) missing++;
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] magic, string[] shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[magic.Length];
            for (int i = 0; i < magic.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                result[i] = existing != null ? existing : Clip(magic[i], i < shared.Length ? shared[i] : null, ref missing);
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

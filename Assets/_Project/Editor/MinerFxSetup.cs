#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Creates ChestFx_Miner.asset (MinerFxPreset values + the clips in Audio/Chests/miner/), and adds MinerHook to the
    /// selected chest. A missing Miner mp3 falls back to the shared chest sound of the same role (Audio/Chests/).
    /// </summary>
    public static class MinerFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_Miner.asset";

        [MenuItem("Tools/Cho Do La/Create Miner Chest FX Profile")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdate();
        }

        [MenuItem("Tools/Cho Do La/Add Miner Hook To Selected Chest")]
        public static void AddHookMenu()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) { Debug.LogWarning("[MinerFx] Select the chest root (the object with ChestRevealSequence) first."); return; }
            if (go.GetComponent<MinerHook>() == null) go.AddComponent<MinerHook>();
            ChestRevealSequence reveal = go.GetComponent<ChestRevealSequence>();
            if (reveal != null) reveal.SetProfile(CreateOrUpdate());
            else Debug.LogWarning("[MinerFx] No ChestRevealSequence on " + go.name + ": the hook is added but nothing will call it.");
            EditorUtility.SetDirty(go);
        }

        /// <summary>Creates the asset with the preset values, or fills only its missing clips if it exists.</summary>
        public static ChestFxProfile CreateOrUpdate()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                MinerFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, MinerFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, MinerFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(MinerFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(MinerFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, MinerFxPreset.AccentFiles, ChestFxPresets.CoinFiles, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 1, clip = Clip(MinerFxPreset.LanternFile, null, ref missing), volume = 0.45f },
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(MinerFxPreset.LidFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(MinerFxPreset.CartFile, null, ref missing), volume = 0.35f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Shared(ChestFxPresets.ThudFile), volume = 0.6f },
                };
            }
            if (missing > 0) Debug.LogWarning($"[MinerFx] {missing} sound(s) not found (neither Miner nor shared). The effect still runs without them.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[MinerFx] Created " + ProfilePath);
            return p;
        }

        private static AudioClip Shared(string file) => file == null ? null : AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + file);

        private static AudioClip Clip(string minerFile, string sharedFile, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(MinerFxPreset.AudioFolder + minerFile);
            if (c != null) return c;
            c = Shared(sharedFile);
            if (c != null) Debug.LogWarning($"[MinerFx] {minerFile} not found; using shared {sharedFile}.");
            else missing++;
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] files, string[] shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                result[i] = existing != null ? existing : Clip(files[i], shared != null && i < shared.Length ? shared[i] : null, ref missing);
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

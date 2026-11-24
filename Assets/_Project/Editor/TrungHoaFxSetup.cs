#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Trung Hoa Chest FX Profile: creates ChestFx_TrungHoa.asset with TrungHoaFxPreset values
    /// and wires the clips in Audio/Chests/trung_hoa/ (falls back to the shared set for any missing file).
    /// Tools/Cho Do La/Add Trung Hoa Hook To Selected: adds TrungHoaHook to the selected chest.
    /// </summary>
    public static class TrungHoaFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_TrungHoa.asset";

        [MenuItem("Tools/Cho Do La/Create Trung Hoa Chest FX Profile")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdateProfile();
        }

        [MenuItem("Tools/Cho Do La/Add Trung Hoa Hook To Selected")]
        public static void AddHookMenu()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) { Debug.LogWarning("[TrungHoaFx] Select the chest (the object with ChestRevealSequence) first."); return; }
            if (go.GetComponent<TrungHoaHook>() == null) { go.AddComponent<TrungHoaHook>(); EditorUtility.SetDirty(go); }
        }

        public static ChestFxProfile CreateOrUpdateProfile()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                TrungHoaFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, TrungHoaFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, TrungHoaFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(TrungHoaFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(TrungHoaFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, TrungHoaFxPreset.AccentFiles, ChestFxPresets.CoinFiles, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(TrungHoaFxPreset.LidFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(TrungHoaFxPreset.ThudFile, ChestFxPresets.ThudFile, ref missing), volume = 0.65f },
                };
            }
            if (missing > 0) Debug.LogWarning($"[TrungHoaFx] {missing} sound(s) not found; the effect still runs without them.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[TrungHoaFx] Created " + ProfilePath);
            return p;
        }

        /// <summary>Own clip in trung_hoa/, else the shared clip (same slot), else its synth fallback.</summary>
        private static AudioClip Clip(string own, string shared, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(TrungHoaFxPreset.AudioFolder + own);
            if (c == null && shared != null)
            {
                c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + shared);
                string synth = ChestFxPresets.SynthFallback(shared);
                if (c == null && synth != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + synth);
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
                string fallback = shared != null && shared.Length > 0 ? shared[System.Math.Min(i, shared.Length - 1)] : null;
                result[i] = existing != null ? existing : Clip(own[i], fallback, ref missing);
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

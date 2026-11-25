#if UNITY_EDITOR
using System.Reflection;
using ChoDoLa.ChestFx;
using ChoDoLa.Interaction;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Chest_Mimic FX: creates ChestFx_Mimic.asset (MimicFxPreset values + the mimic sounds, shared chest sounds where
    /// the mimic has none) and can wire a selected Mimic chest (MimicHook + bite intro on BoxOpenSequence).
    /// </summary>
    public static class MimicFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_Mimic.asset";
        private const string SharedAudio = "Assets/_Project/Audio/Chests/";

        [MenuItem("Tools/Cho Do La/Create Mimic Chest FX Profile")]
        public static void CreateProfileMenu() => Selection.activeObject = CreateOrUpdateProfile();

        /// <summary>Select the Mimic chest (its rig with BoxOpenSequence/ChestRevealSequence) in Play-test scene first.</summary>
        [MenuItem("Tools/Cho Do La/Add Mimic FX To Selected Chest")]
        public static void AddToSelected()
        {
            GameObject go = Selection.activeGameObject;
            BoxOpenSequence box = go != null ? go.GetComponent<BoxOpenSequence>() : null;
            if (box == null)
            {
                Debug.LogError("[MimicFx] Select the Mimic chest rig (the object with BoxOpenSequence) first.");
                return;
            }
            ChestFxProfile p = CreateOrUpdateProfile();
            ChestRevealSequence reveal = go.GetComponent<ChestRevealSequence>();
            if (reveal != null) reveal.SetProfile(p);

            MimicHook hook = go.GetComponent<MimicHook>();
            if (hook == null) hook = go.AddComponent<MimicHook>();
            int missing = 0;
            hook.biteClip = Clip(MimicFxPreset.BiteFile, ref missing);
            hook.chatterClip = Clip(MimicFxPreset.ChatterFile, ref missing);
            hook.droolClip = Clip(MimicFxPreset.DroolFile, ref missing);

            // biteIntro is a private serialized field (default off); turn it on for this chest only.
            FieldInfo bite = typeof(BoxOpenSequence).GetField("biteIntro", BindingFlags.NonPublic | BindingFlags.Instance);
            if (bite != null) bite.SetValue(box, true);
            else Debug.LogWarning("[MimicFx] BoxOpenSequence has no biteIntro field: copy the bite intro from branch claude/mimic-chest.");

            EditorUtility.SetDirty(hook);
            EditorUtility.SetDirty(box);
            if (missing > 0) Debug.LogWarning($"[MimicFx] {missing} hook sound(s) missing in {MimicFxPreset.AudioFolder}.");
            Debug.Log("[MimicFx] Mimic FX added: bite intro on, MimicHook, profile ChestFx_Mimic.");
        }

        public static ChestFxProfile CreateOrUpdateProfile()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            if (p == null)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                MimicFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, MimicFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, MimicFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(MimicFxPreset.WhooshFile, ref missing, ChestFxPresets.WhooshFile);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(MimicFxPreset.AmbientFile, ref missing, ChestFxPresets.AmbientFile);
            p.accentClips = Fill(p.accentClips, MimicFxPreset.AccentFiles, ChestFxPresets.CoinFiles, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(MimicFxPreset.CreakFile, ref missing, ChestFxPresets.CreakFile), volume = 0.75f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(MimicFxPreset.BiteFile, ref missing, ChestFxPresets.ThudFile), volume = 0.5f },
                };
            }
            if (missing > 0) Debug.LogWarning($"[MimicFx] {missing} mimic sound(s) not found in {MimicFxPreset.AudioFolder}; used the shared chest sounds instead where possible.");
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            return p;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] files, string[] shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                string fallback = shared != null && shared.Length > 0 ? shared[System.Math.Min(i, shared.Length - 1)] : null;
                result[i] = existing != null ? existing : Clip(files[i], ref missing, fallback);
            }
            return result;
        }

        private static AudioClip Clip(string file, ref int missing, string sharedFallback = null)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(MimicFxPreset.AudioFolder + file);
            if (c != null) return c;
            missing++;
            return sharedFallback != null ? AssetDatabase.LoadAssetAtPath<AudioClip>(SharedAudio + sharedFallback) : null;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            int slash = folder.LastIndexOf('/');
            string parent = folder.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder.Substring(slash + 1));
        }
    }
}
#endif

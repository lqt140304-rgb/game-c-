#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Chest_HaiTac FX: creates ScriptableObjects/ChestFx/ChestFx_HaiTac.asset from HaiTacFxPreset and fills the clips
    /// by file name from Audio/Chests/hai_tac/ (falls back to the shared Audio/Chests/ set where a pirate clip is missing).
    /// A second menu puts HaiTacHook (with its clips) and the profile on the selected chest.
    /// </summary>
    public static class HaiTacFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_HaiTac.asset";
        private const string Tag = "[HaiTacFx] ";

        [MenuItem("Tools/Cho Do La/Create Hai Tac Chest FX Profile")]
        public static void CreateProfileMenu()
        {
            Selection.activeObject = CreateOrUpdateProfile();
        }

        [MenuItem("Tools/Cho Do La/Add Hai Tac FX To Selected Chest")]
        public static void AddToSelectedMenu()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null)
            {
                Debug.LogError(Tag + "Select the Chest_HaiTac object (or its rig) in the scene first.");
                return;
            }
            ChestFxProfile profile = CreateOrUpdateProfile();
            Transform rig = ChestRevealDemo.FindRig(go.transform);

            ChestRevealSequence reveal = rig.GetComponent<ChestRevealSequence>();
            if (reveal != null)
            {
                SerializedObject so = new SerializedObject(reveal);
                so.FindProperty("profile").objectReferenceValue = profile;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else Debug.LogWarning(Tag + "No ChestRevealSequence on " + rig.name + ": run Tools/Cho Do La/Add Chest Reveal Test first, then this menu.");

            HaiTacHook hook = rig.GetComponent<HaiTacHook>();
            if (hook == null) hook = rig.gameObject.AddComponent<HaiTacHook>();
            int missing = 0;
            SerializedObject ho = new SerializedObject(hook);
            ho.FindProperty("cannonClip").objectReferenceValue = Clip(HaiTacFxPreset.CannonFile, null, ref missing);
            ho.FindProperty("coinsClip").objectReferenceValue = Clip(HaiTacFxPreset.CoinsFile, "sfx_coin_jingle.mp3", ref missing);
            ho.FindProperty("seagullClip").objectReferenceValue = Clip(HaiTacFxPreset.SeagullFile, null, ref missing);
            ho.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hook);
            if (missing > 0) Debug.LogWarning(Tag + missing + " hook sound(s) missing; the effect still runs.");
            Selection.activeGameObject = rig.gameObject;
            Debug.Log(Tag + "Chest_HaiTac FX set on " + rig.name + ".");
        }

        /// <summary>Creates the profile with the preset values, or fills only its missing clips if it exists.</summary>
        public static ChestFxProfile CreateOrUpdateProfile()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                HaiTacFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, HaiTacFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, HaiTacFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(HaiTacFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(HaiTacFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, HaiTacFxPreset.AccentFiles, new[] { ChestFxPresets.CoinFiles[0], null }, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 0, clip = Clip(HaiTacFxPreset.LaughFile, null, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 1, clip = Clip(HaiTacFxPreset.SealFile, null, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(HaiTacFxPreset.CreakFile, ChestFxPresets.CreakFile, ref missing), volume = 0.75f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(null, ChestFxPresets.ThudFile, ref missing), volume = 0.6f },
                };
            }
            if (missing > 0) Debug.LogWarning(Tag + missing + " sound(s) not found. The effect still runs without them.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log(Tag + "Created " + ProfilePath);
            return p;
        }

        /// <summary>Pirate clip from hai_tac/, else the shared clip from Audio/Chests/ (or its Synth fallback).</summary>
        private static AudioClip Clip(string pirateFile, string sharedFile, ref int missing)
        {
            AudioClip c = null;
            if (pirateFile != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(HaiTacFxPreset.AudioFolder + pirateFile);
            if (c == null && sharedFile != null)
            {
                c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + sharedFile);
                string synth = ChestFxPresets.SynthFallback(sharedFile);
                if (c == null && synth != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + synth);
                if (c != null && pirateFile != null) Debug.LogWarning(Tag + pirateFile + " not found; using shared " + sharedFile + ".");
            }
            if (c == null) missing++;
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] pirate, string[] shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[pirate.Length];
            for (int i = 0; i < pirate.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                string fallback = shared != null && i < shared.Length ? shared[i] : null;
                result[i] = existing != null ? existing : Clip(pirate[i], fallback, ref missing);
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

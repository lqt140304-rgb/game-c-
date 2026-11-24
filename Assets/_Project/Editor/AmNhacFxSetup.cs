#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using ChoDoLa.Interaction;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Chest_AmNhac FX profile: values from AmNhacFxPreset, clips from Audio/Chests/am_nhac/ (falls back to the
    /// shared set in Audio/Chests/ for anything missing). The second menu wires the selected chest:
    /// profile, AmNhacHook (equaliser + crowd) and BoxOpenSequence.spinRecord = true.
    /// </summary>
    public static class AmNhacFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_AmNhac.asset";

        [MenuItem("Tools/Cho Do La/Create Am Nhac Chest FX Profile")]
        public static void CreateMenu() => Selection.activeObject = CreateOrUpdate();

        [MenuItem("Tools/Cho Do La/Wire Selected Chest As Am Nhac")]
        public static void WireSelected()
        {
            GameObject go = Selection.activeGameObject;
            ChestRevealSequence reveal = go != null ? go.GetComponentInParent<ChestRevealSequence>() : null;
            if (reveal == null) reveal = go != null ? go.GetComponentInChildren<ChestRevealSequence>() : null;
            if (reveal == null)
            {
                Debug.LogError("[AmNhacFx] Select the chest that has ChestRevealSequence (run Tools/Cho Do La/Add Chest Reveal Test first).");
                return;
            }
            ChestFxProfile p = CreateOrUpdate();
            SerializedObject so = new SerializedObject(reveal);
            so.FindProperty("profile").objectReferenceValue = p;
            so.ApplyModifiedProperties();

            AmNhacHook hook = reveal.GetComponent<AmNhacHook>();
            if (hook == null) hook = Undo.AddComponent<AmNhacHook>(reveal.gameObject);
            hook.SetCrowdClip(AssetDatabase.LoadAssetAtPath<AudioClip>(AmNhacFxPreset.AudioFolder + AmNhacFxPreset.CrowdFile));
            EditorUtility.SetDirty(hook);

            BoxOpenSequence box = reveal.GetComponent<BoxOpenSequence>();
            if (box != null)
            {
                SerializedObject bso = new SerializedObject(box);
                SerializedProperty spin = bso.FindProperty("spinRecord");
                if (spin != null) { spin.boolValue = true; bso.ApplyModifiedProperties(); }
                else Debug.LogWarning("[AmNhacFx] BoxOpenSequence has no spinRecord field (branch without the record patch).");
                if (box.transform.Find("Record") == null) Debug.LogWarning("[AmNhacFx] No \"Record\" child: the record will not spin.");
            }
            Debug.Log("[AmNhacFx] " + reveal.name + " wired as Chest_AmNhac.");
        }

        public static ChestFxProfile CreateOrUpdate()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                AmNhacFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, AmNhacFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, AmNhacFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(AmNhacFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(AmNhacFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, AmNhacFxPreset.AccentFiles, null, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 0, clip = Clip(AmNhacFxPreset.CoinSlotFile, null, ref missing), volume = 0.6f },
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(AmNhacFxPreset.NeedleDropFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Shared(ChestFxPresets.ThudFile, ref missing), volume = 0.5f },
                };
            }
            if (missing > 0) Debug.LogWarning($"[AmNhacFx] {missing} sound(s) not found; the effect still runs without them.");
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[AmNhacFx] Created " + ProfilePath);
            return p;
        }

        /// <summary>Own clip from am_nhac/; if missing, the shared file in Audio/Chests/ (or its Synth/ fallback).</summary>
        private static AudioClip Clip(string own, string sharedFile, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(AmNhacFxPreset.AudioFolder + own);
            if (c != null) return c;
            if (sharedFile != null)
            {
                c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + sharedFile);
                string fb = ChestFxPresets.SynthFallback(sharedFile);
                if (c == null && fb != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + fb);
                if (c != null) { Debug.LogWarning($"[AmNhacFx] {own} not found; using shared {sharedFile}."); return c; }
            }
            missing++;
            return null;
        }

        private static AudioClip Shared(string file, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + file);
            if (c == null) missing++;
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] own, string[] shared, ref int missing)
        {
            AudioClip[] r = new AudioClip[own.Length];
            for (int i = 0; i < own.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                r[i] = existing != null ? existing : Clip(own[i], shared != null && i < shared.Length ? shared[i] : null, ref missing);
            }
            return r;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
#endif

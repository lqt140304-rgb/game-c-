#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Chest_Game FX: creates ChestFx_Game.asset from <see cref="GameFxPreset"/> and wires the chiptune clips in
    /// Audio/Chests/game/. Re-running only fills empty clip slots (your manual edits stay). The lid-thud is the
    /// shared one; every other sound is Chest_Game's own. A clip that is missing falls back to the shared set.
    /// </summary>
    public static class GameFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_Game.asset";

        [MenuItem("Tools/Cho Do La/Create Game Chest FX Profile")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdate();
        }

        [MenuItem("Tools/Cho Do La/Add Game Pixel Hook To Selected")]
        public static void AddHookToSelected()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) { Debug.LogWarning("[GameFx] Select the chest root (the object with ChestRevealSequence) first."); return; }
            if (go.GetComponent<GamePixelHook>() == null)
            {
                go.AddComponent<GamePixelHook>();
                EditorUtility.SetDirty(go);
            }
            Debug.Log("[GameFx] GamePixelHook on " + go.name);
        }

        public static ChestFxProfile CreateOrUpdate()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                GameFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, GameFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, GameFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(GameFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(GameFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, GameFxPreset.AccentFiles, null, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 0, clip = Clip(GameFxPreset.ShakeFile, null, ref missing), volume = 0.6f },
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(GameFxPreset.BootFile, ChestFxPresets.CreakFile, ref missing), volume = 0.6f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Shared(ChestFxPresets.ThudFile), volume = 0.5f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(GameFxPreset.StartFile, null, ref missing), volume = 0.5f },
                };
            }
            if (missing > 0) Debug.LogWarning($"[GameFx] {missing} sound(s) missing in {GameFxPreset.AudioFolder} and in the shared set; those slots stay silent.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[GameFx] Created " + ProfilePath + ". Add GamePixelHook to the chest root for the square pixels.");
            return p;
        }

        /// <summary>Own clip in Audio/Chests/game/, else the shared one in Audio/Chests/ (or its Synth fallback).</summary>
        private static AudioClip Clip(string own, string shared, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(GameFxPreset.AudioFolder + own);
            if (c == null && shared != null)
            {
                Debug.LogWarning($"[GameFx] {own} not found; using shared {shared}.");
                c = Shared(shared);
            }
            if (c == null) missing++;
            return c;
        }

        private static AudioClip Shared(string file)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + file);
            string fallback = c == null ? ChestFxPresets.SynthFallback(file) : null;
            if (fallback != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + fallback);
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] own, string[] shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[own.Length];
            for (int i = 0; i < own.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                string fb = shared != null && i < shared.Length ? shared[i] : null;
                result[i] = existing != null ? existing : Clip(own[i], fb, ref missing);
            }
            return result;
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

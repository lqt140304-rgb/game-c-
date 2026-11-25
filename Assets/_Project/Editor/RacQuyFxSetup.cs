#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Menu Tools/Cho Do La/Create Rac Quy Chest FX Profile: creates (or fills the missing clips of)
    /// ScriptableObjects/ChestFx/ChestFx_RacQuy.asset with <see cref="RacQuyFxPreset"/>, plus a prefab
    /// ChestFx_RacQuy_Hook.prefab holding <see cref="RacQuyHook"/> (set as the profile's customFxPrefab).
    /// Own sounds are in Audio/Chests/rac_quy/; anything missing falls back to the shared set in Audio/Chests/.
    /// </summary>
    public static class RacQuyFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_RacQuy.asset";
        private const string HookPrefabPath = ProfileFolder + "/ChestFx_RacQuy_Hook.prefab";
        private const string OwnFolder = "Assets/_Project/Audio/Chests/rac_quy/";

        private static readonly string[] OwnLockFiles =
        {
            "sfx_racquy_lock_rac.mp3", "sfx_racquy_lock_thuong.mp3", "sfx_racquy_lock_hiem.mp3",
            "sfx_racquy_lock_ratHiem.mp3", "sfx_racquy_lock_huyenThoai.mp3",
        };

        [MenuItem("Tools/Cho Do La/Create Rac Quy Chest FX Profile")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdate();
        }

        public static ChestFxProfile CreateOrUpdate()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                RacQuyFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            if (p.tickClips == null || p.tickClips.Length == 0 || p.tickClips[0] == null)
                p.tickClips = new[] { Own("sfx_racquy_tick.mp3", ChestFxPresets.TickFiles[0], ref missing) };
            if (p.lockClipsByTier == null || p.lockClipsByTier.Length != OwnLockFiles.Length) p.lockClipsByTier = new AudioClip[OwnLockFiles.Length];
            for (int i = 0; i < OwnLockFiles.Length; i++)
                if (p.lockClipsByTier[i] == null) p.lockClipsByTier[i] = Own(OwnLockFiles[i], ChestFxPresets.LockFiles[i], ref missing);
            if (p.whooshClip == null) p.whooshClip = Own("sfx_racquy_whoosh.mp3", ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Shared(ChestFxPresets.AmbientFile, ref missing);
            if (p.accentClips == null || p.accentClips.Length == 0 || p.accentClips[0] == null)
                p.accentClips = new[] { Own("sfx_racquy_can_roll.mp3", null, ref missing) };
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 1, clip = Own("sfx_racquy_tape_rip.mp3", null, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 2, clip = Own("sfx_racquy_lid_pop.mp3", ChestFxPresets.CreakFile, ref missing), volume = 0.75f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Own("sfx_racquy_lid_clang.mp3", ChestFxPresets.ThudFile, ref missing), volume = 0.65f },
                };
            }

            if (p.customFxPrefab == null || p.customFxPrefab.GetComponent<RacQuyHook>() == null)
            {
                p.customFxPrefab = CreateHookPrefab(ref missing);
            }

            if (missing > 0)
                Debug.LogWarning($"[RacQuyFx] {missing} sound(s) not found in {OwnFolder} nor the shared set. The effect still runs; those sounds are silent.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[RacQuyFx] Created " + ProfilePath);
            return p;
        }

        private static GameObject CreateHookPrefab(ref int missing)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(HookPrefabPath);
            if (existing != null && existing.GetComponent<RacQuyHook>() != null) return existing;

            GameObject temp = new GameObject("ChestFx_RacQuy_Hook");
            RacQuyHook hook = temp.AddComponent<RacQuyHook>();
            hook.gurgleClip = Own("sfx_racquy_gurgle.mp3", null, ref missing);
            hook.fliesClip = Own("sfx_racquy_flies.mp3", null, ref missing);
            hook.canRollClip = Own("sfx_racquy_can_roll.mp3", null, ref missing);
            hook.fartClip = Own("sfx_racquy_fart.mp3", null, ref missing);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, HookPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        /// <summary>Own mp3 in rac_quy/; if missing, the shared file (which itself falls back to Synth/).</summary>
        private static AudioClip Own(string file, string sharedFallback, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(OwnFolder + file);
            if (c != null) return c;
            if (sharedFallback != null)
            {
                Debug.LogWarning($"[RacQuyFx] {file} not found; using the shared {sharedFallback}.");
                return Shared(sharedFallback, ref missing);
            }
            missing++;
            return null;
        }

        private static AudioClip Shared(string file, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + file);
            if (c != null) return c;
            string fb = ChestFxPresets.SynthFallback(file);
            if (fb != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + fb);
            if (c == null) missing++;
            return c;
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

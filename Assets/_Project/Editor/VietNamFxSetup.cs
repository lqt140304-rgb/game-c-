#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Viet Nam Chest FX Profile: tạo (hoặc bổ sung clip còn thiếu cho)
    /// ScriptableObjects/ChestFx/ChestFx_VietNam.asset với VietNamFxPreset, gắn âm riêng ở Audio/Chests/viet_nam/
    /// (thiếu file nào thì dùng âm chung ở Audio/Chests/), và tạo prefab VietNamFx.prefab mang VietNamHook đặt vào customFxPrefab.
    /// </summary>
    public static class VietNamFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_VietNam.asset";
        private const string HookPrefabPath = ProfileFolder + "/VietNamFx.prefab";

        [MenuItem("Tools/Cho Do La/Create Viet Nam Chest FX Profile")]
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
                VietNamFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            p.tickClips = Fill(p.tickClips, VietNamFxPreset.TickFiles, ChestFxPresets.TickFiles[0], ref missing);
            AudioClip[] locks = new AudioClip[ChestTierUtil.Count];
            for (int i = 0; i < locks.Length; i++)
            {
                AudioClip old = p.lockClipsByTier != null && i < p.lockClipsByTier.Length ? p.lockClipsByTier[i] : null;
                locks[i] = old != null ? old : Clip(VietNamFxPreset.LockFiles[i], ChestFxPresets.LockFiles[i], ref missing);
            }
            p.lockClipsByTier = locks;
            if (p.whooshClip == null) p.whooshClip = Clip(VietNamFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(VietNamFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, VietNamFxPreset.AccentFiles, ChestFxPresets.CoinFiles[0], ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(VietNamFxPreset.CreakFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(VietNamFxPreset.ThudFile, ChestFxPresets.ThudFile, ref missing), volume = 0.6f },
                };
            }
            if (p.customFxPrefab == null) p.customFxPrefab = HookPrefab();
            if (missing > 0) Debug.LogWarning($"[VietNamFx] {missing} sound(s) not found in {VietNamFxPreset.AudioFolder} nor the shared set. The effect still runs.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[VietNamFx] Created " + ProfilePath);
            return p;
        }

        private static GameObject HookPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(HookPrefabPath);
            if (existing != null) return existing;
            GameObject temp = new GameObject("VietNamFx");
            temp.AddComponent<VietNamHook>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, HookPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        /// <summary>Âm riêng của rương; thiếu thì âm chung (mp3 rồi bản Synth).</summary>
        private static AudioClip Clip(string own, string shared, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(VietNamFxPreset.AudioFolder + own);
            if (c != null) return c;
            c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + shared);
            if (c == null)
            {
                string fb = ChestFxPresets.SynthFallback(shared);
                if (fb != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + fb);
            }
            if (c != null) Debug.LogWarning($"[VietNamFx] {own} not found; using shared {shared}.");
            else missing++;
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] files, string shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                AudioClip old = current != null && i < current.Length ? current[i] : null;
                result[i] = old != null ? old : Clip(files[i], shared, ref missing);
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

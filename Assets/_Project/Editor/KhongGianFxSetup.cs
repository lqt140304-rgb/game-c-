#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Khong Gian Chest FX Profile: creates ChestFx_KhongGian.asset (KhongGianFxPreset values),
    /// writes the profile silent (ChestAudioRule.ApplyToProfile, every run), builds the KhongGianFx prefab
    /// (chỉ KhongGianMissionHook) used as the profile's customFxPrefab and assigns the ElevenLabs .mp3 clips to the hook.
    /// </summary>
    public static class KhongGianFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_KhongGian.asset";
        private const string PrefabFolder = "Assets/_Project/Prefabs/ChestFx";
        private const string PrefabPath = PrefabFolder + "/KhongGianFx.prefab";

        [MenuItem("Tools/Cho Do La/Create Khong Gian Chest FX Profile")]
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
                KhongGianFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            // A1: hồ sơ lõi gần như câm, ghi lại MỖI LẦN chạy menu (tension.enabled = false, mọi ô âm lõi trống).
            // Tiếng chốt do KhongGianMissionHook phát qua stingClips (A11).
            ChestAudioRule.ApplyToProfile(p);
            if (p.customFxPrefab == null) p.customFxPrefab = BuildPrefab();
            EnsureMission(p.customFxPrefab, ref missing);

            if (missing > 0) Debug.LogWarning($"[KhongGianFx] {missing} sound(s) not found. The effect still runs; empty clips are skipped.");
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[KhongGianFx] Created " + ProfilePath);
            return p;
        }

        private static GameObject BuildPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null) return existing;
            EnsureFolder(PrefabFolder);
            GameObject go = new GameObject("KhongGianFx");
            go.AddComponent<KhongGianMissionHook>();   // CHỈ hook kịch bản "phi vụ tiếp tế"; KhongGianHook cũ không còn được gắn (T10)
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        /// <summary>
        /// Mỗi lần chạy menu: prefab chỉ còn KhongGianMissionHook (gỡ KhongGianHook cũ nếu có; file .cs giữ nguyên) và
        /// các ô âm của hook trỏ tới file .mp3 ElevenLabs. Ô nào đã có người gán tay thì giữ; file thiếu thì để trống.
        /// </summary>
        private static void EnsureMission(GameObject prefab, ref int missing)
        {
            if (prefab == null) return;
            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path)) return;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            KhongGianMissionHook hook = root.GetComponent<KhongGianMissionHook>();
            if (hook == null) hook = root.AddComponent<KhongGianMissionHook>();
            KhongGianHook old = root.GetComponent<KhongGianHook>();
            if (old != null) Object.DestroyImmediate(old);   // hai hook chạy song song = chồng hiệu ứng; file KhongGianHook.cs vẫn giữ

            if (hook.engineClip == null) hook.engineClip = Clip(KhongGianFxPreset.EngineFile, ref missing);
            if (hook.blipClip == null) hook.blipClip = Clip(KhongGianFxPreset.BlipFile, ref missing);
            if (hook.fuelClip == null) hook.fuelClip = Clip(KhongGianFxPreset.FuelFile, ref missing);
            if (hook.rocketClip == null) hook.rocketClip = Clip(KhongGianFxPreset.RocketFile, ref missing);
            if (hook.impactClip == null) hook.impactClip = Clip(KhongGianFxPreset.ImpactFile, ref missing);
            if (hook.hatchClip == null) hook.hatchClip = Clip(KhongGianFxPreset.HatchFile, ref missing);
            if (hook.sputterClip == null) hook.sputterClip = Clip(KhongGianFxPreset.SputterFile, ref missing);
            AudioClip[] sting = new AudioClip[KhongGianFxPreset.LockFiles.Length];
            for (int i = 0; i < sting.Length; i++)
            {
                AudioClip cur = hook.stingClips != null && i < hook.stingClips.Length ? hook.stingClips[i] : null;
                sting[i] = cur != null ? cur : Clip(KhongGianFxPreset.LockFiles[i], ref missing);
            }
            hook.stingClips = sting;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        /// <summary>Âm riêng trong khong_gian/ (không còn lấy bộ âm chung hay Synth).</summary>
        private static AudioClip Clip(string file, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(KhongGianFxPreset.AudioFolder + file);
            if (c == null) { missing++; Debug.LogWarning("[KhongGianFx] Không thấy " + file); }
            return c;
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

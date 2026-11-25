#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Khong Gian Chest FX Profile: creates ChestFx_KhongGian.asset (KhongGianFxPreset values),
    /// attaches the khong_gian sounds by file name (shared Audio/Chests set for anything missing), and builds the
    /// KhongGianFx prefab (chỉ KhongGianMissionHook) used as the profile's customFxPrefab. Re-running only fills missing clips.
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
            p.tickClips = Fill(p.tickClips, KhongGianFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, KhongGianFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(KhongGianFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(KhongGianFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, new[] { KhongGianFxPreset.MeteorFile }, new string[] { null }, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 3, clip = Clip(KhongGianFxPreset.HatchFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(KhongGianFxPreset.LandingFile, ChestFxPresets.ThudFile, ref missing), volume = 0.6f },
                };
            }
            if (p.customFxPrefab == null) p.customFxPrefab = BuildPrefab(ref missing);
            EnsureMission(p.customFxPrefab);

            if (missing > 0) Debug.LogWarning($"[KhongGianFx] {missing} sound(s) not found. The effect still runs; empty clips are skipped.");
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[KhongGianFx] Created " + ProfilePath);
            return p;
        }

        private static GameObject BuildPrefab(ref int missing)
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

        /// <summary>Prefab cũ (tạo trước khi có kịch bản mới) chưa có KhongGianMissionHook: thêm vào, giữ nguyên phần khác.</summary>
        private static void EnsureMission(GameObject prefab)
        {
            if (prefab == null) return;
            if (prefab.GetComponent<KhongGianMissionHook>() != null && prefab.GetComponent<KhongGianHook>() == null) return;
            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path)) return;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root.GetComponent<KhongGianMissionHook>() == null) root.AddComponent<KhongGianMissionHook>();
            KhongGianHook old = root.GetComponent<KhongGianHook>();
            if (old != null) Object.DestroyImmediate(old);   // hai hook chạy song song = chồng hiệu ứng; file KhongGianHook.cs vẫn giữ
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log("[KhongGianFx] Prefab " + path + " chỉ còn KhongGianMissionHook.");
        }

        /// <summary>Own sound from khong_gian/, else the shared chest sound (with its Synth fallback).</summary>
        private static AudioClip Clip(string own, string shared, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(KhongGianFxPreset.AudioFolder + own);
            if (c != null || shared == null)
            {
                if (c == null) missing++;
                return c;
            }
            Debug.LogWarning($"[KhongGianFx] {own} not found; using the shared {shared}.");
            c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + shared);
            string synth = c == null ? ChestFxPresets.SynthFallback(shared) : null;
            if (synth != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + synth);
            if (c == null) missing++;
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] own, string[] shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[own.Length];
            for (int i = 0; i < own.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                result[i] = existing != null ? existing : Clip(own[i], i < shared.Length ? shared[i] : null, ref missing);
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

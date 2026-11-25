#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Thien Co Chest FX Profile: makes ChestFx_ThienCo.asset (ThienCoFxPreset values),
    /// a ThienCo_Fx prefab carrying ThienCoHook (set as customFxPrefab) and fills the hook's ElevenLabs clips from
    /// Audio/Chests/thien_co/. The core profile is made silent every run (ChestAudioRule.ApplyToProfile, tension off).
    /// Running it again keeps the values you tuned and fills only empty hook clip slots.
    /// </summary>
    public static class ThienCoFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_ThienCo.asset";
        private const string PrefabFolder = "Assets/_Project/Prefabs/ChestFx";
        private const string PrefabPath = PrefabFolder + "/ThienCo_Fx.prefab";

        [MenuItem("Tools/Cho Do La/Create Thien Co Chest FX Profile")]
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
                ThienCoFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            // A1: hồ sơ lõi gần như câm, ghi lại MỖI LẦN chạy menu (không chỉ khi trống). Âm do ThienCoHook phát.
            ChestAudioRule.ApplyToProfile(p);
            if (p.customFxPrefab == null) p.customFxPrefab = CreateHookPrefab();
            FillHookClips(p.customFxPrefab);

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[ThienCoFx] Created " + ProfilePath);
            return p;
        }

        private static GameObject CreateHookPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null) return existing;
            EnsureFolder(PrefabFolder);
            GameObject go = new GameObject("ThienCo_Fx");
            go.AddComponent<ThienCoHook>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        /// <summary>
        /// Điền âm ElevenLabs (.mp3 trong Audio/Chests/thien_co/, theo Tools/Audio/elevenlabs_chest_manifest.json) vào ThienCoHook
        /// trên prefab (chỉ ô trống). File thiếu: để trống, ghi cảnh báo. Không nạp .wav numpy nữa (A9).
        /// </summary>
        private static void FillHookClips(GameObject prefab)
        {
            ThienCoHook h = prefab != null ? prefab.GetComponent<ThienCoHook>() : null;
            if (h == null) return;
            int missing = 0;
            if (h.stingClips == null || h.stingClips.Length != ThienCoFxPreset.LockFiles.Length) h.stingClips = new AudioClip[ThienCoFxPreset.LockFiles.Length];
            for (int i = 0; i < h.stingClips.Length; i++) h.stingClips[i] = L(h.stingClips[i], ThienCoFxPreset.LockFiles[i], ref missing);
            h.tick = L(h.tick, "sfx_thienco_tick.mp3", ref missing);
            h.tock = L(h.tock, "sfx_thienco_tock.mp3", ref missing);
            h.wind = L(h.wind, "sfx_thienco_wind.mp3", ref missing);
            h.gearClank = L(h.gearClank, "sfx_thienco_gear_clank.mp3", ref missing);
            h.meshOk = L(h.meshOk, "sfx_thienco_mesh_ok.mp3", ref missing);
            h.jam = L(h.jam, "sfx_thienco_jam.mp3", ref missing);
            h.bell = L(h.bell, "sfx_thienco_bell.mp3", ref missing);
            h.sproing = L(h.sproing, "sfx_thienco_sproing.mp3", ref missing);
            h.cuckoo = L(h.cuckoo, "sfx_thienco_cuckoo.mp3", ref missing);
            h.cuckooSad = L(h.cuckooSad, "sfx_thienco_cuckoo_sad.mp3", ref missing);
            h.launch = L(h.launch, "sfx_thienco_launch.mp3", ref missing);
            if (missing > 0) Debug.LogWarning($"[ThienCoFx] {missing} sound(s) missing in {ThienCoFxPreset.AudioFolder}; left empty.");
            EditorUtility.SetDirty(prefab);
        }

        private static AudioClip L(AudioClip cur, string file, ref int missing)
        {
            if (cur != null) return cur;
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(ThienCoFxPreset.AudioFolder + file);
            if (c == null) missing++;
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

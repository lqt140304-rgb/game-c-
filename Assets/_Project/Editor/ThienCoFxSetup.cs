#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Thien Co Chest FX Profile: makes ChestFx_ThienCo.asset (ThienCoFxPreset values),
    /// a ThienCo_Fx prefab carrying ThienCoHook (set as customFxPrefab) and fills the clips from
    /// Audio/Chests/thien_co/, falling back to the shared chest sounds when a file is missing.
    /// Running it again keeps the values you tuned and fills only empty clip slots.
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

            int fallbacks = 0;
            p.tickClips = Fill(p.tickClips, ThienCoFxPreset.TickFiles, ChestFxPresets.TickFiles, ref fallbacks);
            p.lockClipsByTier = Fill(p.lockClipsByTier, ThienCoFxPreset.LockFiles, ChestFxPresets.LockFiles, ref fallbacks);
            if (p.whooshClip == null) p.whooshClip = Clip(ThienCoFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref fallbacks);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(ThienCoFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref fallbacks);
            p.accentClips = Fill(p.accentClips, ThienCoFxPreset.AccentFiles, null, ref fallbacks);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(ThienCoFxPreset.UnsealFile, ChestFxPresets.CreakFile, ref fallbacks), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(ThienCoFxPreset.HatchOpenFile, ChestFxPresets.ThudFile, ref fallbacks), volume = 0.6f },
                };
            }
            if (p.customFxPrefab == null) p.customFxPrefab = CreateHookPrefab();
            FillHookClips(p.customFxPrefab);
            if (fallbacks > 0) Debug.LogWarning($"[ThienCoFx] {fallbacks} sound(s) missing in {ThienCoFxPreset.AudioFolder}; used the shared chest sounds (or left empty).");

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

        /// <summary>Điền âm "Bộ máy Thiên Cơ" vào ThienCoHook trên prefab (chỉ ô trống) từ thien_co/clockwork/.</summary>
        private static void FillHookClips(GameObject prefab)
        {
            ThienCoHook h = prefab != null ? prefab.GetComponent<ThienCoHook>() : null;
            if (h == null) return;
            h.tick = L(h.tick, "tick"); h.tock = L(h.tock, "tock"); h.wind = L(h.wind, "wind");
            h.gearClank = L(h.gearClank, "gear_clank"); h.meshOk = L(h.meshOk, "mesh_ok"); h.jam = L(h.jam, "jam");
            h.bell = L(h.bell, "bell"); h.bellBig = L(h.bellBig, "bell_big"); h.sproing = L(h.sproing, "sproing");
            h.winddown = L(h.winddown, "winddown"); h.cuckoo = L(h.cuckoo, "cuckoo"); h.cuckooSad = L(h.cuckooSad, "cuckoo_sad");
            h.launch = L(h.launch, "launch"); h.sandLoop = L(h.sandLoop, "sand_loop"); h.musicBoxRise = L(h.musicBoxRise, "musicbox_rise");
            EditorUtility.SetDirty(prefab);
        }

        private static AudioClip L(AudioClip cur, string n) =>
            cur != null ? cur : AssetDatabase.LoadAssetAtPath<AudioClip>(ThienCoFxPreset.AudioFolder + "clockwork/sfx_tc_" + n + ".wav");

        /// <summary>Own clip; if missing, the shared clip (ChestFxPresets folder, then its synth fallback).</summary>
        private static AudioClip Clip(string own, string shared, ref int fallbacks)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(ThienCoFxPreset.AudioFolder + own);
            if (c != null) return c;
            fallbacks++;
            if (shared == null) return null;
            c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + shared);
            if (c != null) return c;
            string synth = ChestFxPresets.SynthFallback(shared);
            return synth != null ? AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + synth) : null;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] own, string[] shared, ref int fallbacks)
        {
            AudioClip[] result = new AudioClip[own.Length];
            for (int i = 0; i < own.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                string s = shared != null && i < shared.Length ? shared[i] : null;
                result[i] = existing != null ? existing : Clip(own[i], s, ref fallbacks);
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

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ChoDoLa.ChestFx;
using ChoDoLa.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create All Chest FX Profiles: runs the profile maker of all 17 chests (Golden from the core,
    /// 16 from the per-chest <c>&lt;Name&gt;FxSetup</c> classes; existing profiles only get their missing clips filled),
    /// then writes ScriptableObjects/ChestFx/ChestFxLibrary.asset (every profile, the hooks that belong on the chest
    /// root, and every Art/Items/LowPoly/**/*.glb item) and gives it to each ChestHandMenu in the open scene,
    /// so [P] in the hand menu can play the full reveal. Nothing is scanned at runtime.
    /// </summary>
    public static class CreateAllChestFxProfiles
    {
        private const string Log = "[ChestFxAll] ";
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string HookFolder = ProfileFolder + "/HandMenu";
        private const string LibraryPath = ProfileFolder + "/ChestFxLibrary.asset";
        private const string ItemsFolder = "Assets/_Project/Art/Items/LowPoly";

        [MenuItem("Tools/Cho Do La/Create All Chest FX Profiles")]
        public static void CreateAll()
        {
            var makers = new List<KeyValuePair<string, Func<ChestFxProfile>>>
            {
                Maker("Golden", ChestRevealTestSetup.CreateOrUpdateGoldenProfile),
                Maker("Magic", MagicFxSetup.CreateOrUpdate),
                Maker("Mimic", MimicFxSetup.CreateOrUpdateProfile),
                Maker("Miner", MinerFxSetup.CreateOrUpdate),
                Maker("RacQuy", RacQuyFxSetup.CreateOrUpdate),
                Maker("HaiTac", HaiTacFxSetup.CreateOrUpdateProfile),
                Maker("AmNhac", AmNhacFxSetup.CreateOrUpdate),
                Maker("VietNam", VietNamFxSetup.CreateOrUpdate),
                Maker("TuoiTho", TuoiThoFxSetup.CreateOrUpdate),
                Maker("Robot", RobotFxSetup.CreateOrUpdate),
                Maker("KhongGian", KhongGianFxSetup.CreateOrUpdate),
                Maker("Game", GameFxSetup.CreateOrUpdate),
                Maker("TheThao", ChoDoLa.EditorTools.TheThaoFxSetup.CreateOrUpdate),
                Maker("TrungHoa", TrungHoaFxSetup.CreateOrUpdateProfile),
                Maker("NhatBan", NhatBanFxSetup.CreateOrUpdate),
                Maker("CoPhong", CoPhongFxSetup.CreateOrUpdate),
                Maker("ThienCo", ThienCoFxSetup.CreateOrUpdate),
            };

            EnsureFolder(HookFolder);
            ChestFxLibrary library = AssetDatabase.LoadAssetAtPath<ChestFxLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<ChestFxLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }
            library.entries.Clear();

            List<string> failed = new List<string>();
            foreach (var m in makers)
            {
                ChestFxProfile p = null;
                try { p = m.Value(); }
                catch (Exception ex) { Debug.LogError(Log + "Chest_" + m.Key + ": " + ex); }
                if (p == null) { failed.Add(m.Key); continue; }

                ChestFxLibrary.Entry e = new ChestFxLibrary.Entry
                {
                    chestId = string.IsNullOrEmpty(p.chestId) ? "Chest_" + m.Key : p.chestId,
                    profile = p,
                    biteIntro = m.Key == "Mimic",
                    spinRecord = m.Key == "AmNhac",
                };
                try { e.rootHookTemplate = RootHookTemplate(m.Key); }
                catch (Exception ex) { Debug.LogError(Log + "Hook Chest_" + m.Key + ": " + ex); }
                library.entries.Add(e);
            }

            library.items = LoadItems(out int itemFailures);
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();

            int menus = AssignToHandMenus(library);

            Debug.Log(Log + $"{library.entries.Count}/17 hồ sơ trong {ProfileFolder}, {library.items.Count} món minh hoạ, gán thư viện cho {menus} ChestHandMenu."
                      + (failed.Count > 0 ? " LỖI ở: " + string.Join(", ", failed) + " (xem lỗi đỏ phía trên)." : string.Empty)
                      + (itemFailures > 0 ? $" {itemFailures} món .glb không nạp được (chưa cài glTFast hoặc chưa git lfs pull)." : string.Empty)
                      + (menus == 0 ? " Cảnh chưa có ChestHandMenu: chạy Tools/Cho Do La/Add Chest Hand Menu rồi chạy lại menu này." : string.Empty));
            Selection.activeObject = library;
        }

        private static KeyValuePair<string, Func<ChestFxProfile>> Maker(string name, Func<ChestFxProfile> f) =>
            new KeyValuePair<string, Func<ChestFxProfile>>(name, f);

        // ------------------------------------------------------------------ Hooks that sit on the chest root

        /// <summary>
        /// The chests whose setup menu adds the hook to the selected chest (instead of the profile's customFxPrefab)
        /// get a small prefab here holding that hook, configured like their own "Add ... To Selected" menu does.
        /// Chests that use customFxPrefab return null (the reveal spawns their hook itself).
        /// </summary>
        private static ChestFxHook RootHookTemplate(string key)
        {
            switch (key)
            {
                case "Golden": return SaveHook<GoldenGlintHook>(key, null);
                case "Magic": return SaveHook<MagicHook>(key, null);
                case "Miner": return SaveHook<MinerHook>(key, null);
                case "Robot": return SaveHook<RobotHook>(key, null);
                case "Game": return SaveHook<GamePixelHook>(key, null);
                case "TrungHoa": return SaveHook<TrungHoaHook>(key, TrungHoaFxSetup.WireHookClips);
                case "TuoiTho": return SaveHook<TuoiThoHook>(key, null);
                case "Mimic":
                    return SaveHook<MimicHook>(key, h =>
                    {
                        h.biteClip = Clip(MimicFxPreset.AudioFolder + MimicFxPreset.BiteFile);
                        h.chatterClip = Clip(MimicFxPreset.AudioFolder + MimicFxPreset.ChatterFile);
                        h.droolClip = Clip(MimicFxPreset.AudioFolder + MimicFxPreset.DroolFile);
                    });
                case "AmNhac":
                    return SaveHook<AmNhacHook>(key, h => h.SetCrowdClip(Clip(AmNhacFxPreset.AudioFolder + AmNhacFxPreset.CrowdFile)));
                case "TheThao":
                    return SaveHook<TheThaoHook>(key, h =>
                    {
                        h.kickoffClip = Clip(TheThaoFxPreset.AudioFolder + TheThaoFxPreset.KickoffFile);
                        h.airHornClip = Clip(TheThaoFxPreset.AudioFolder + TheThaoFxPreset.AirHornFile);
                    });
                case "HaiTac":
                    return SaveHook<HaiTacHook>(key, h =>
                    {
                        SerializedObject so = new SerializedObject(h);
                        SetClip(so, "cannonClip", HaiTacFxPreset.AudioFolder + HaiTacFxPreset.CannonFile);
                        SetClip(so, "coinsClip", HaiTacFxPreset.AudioFolder + HaiTacFxPreset.CoinsFile, ChestFxPresets.AudioFolder + "sfx_coin_jingle.mp3");
                        SetClip(so, "seagullClip", HaiTacFxPreset.AudioFolder + HaiTacFxPreset.SeagullFile);
                        so.ApplyModifiedPropertiesWithoutUndo();
                    });
                default: return null;   // RacQuy, VietNam, KhongGian, NhatBan, CoPhong, ThienCo: hook in customFxPrefab
            }
        }

        private static T SaveHook<T>(string key, Action<T> configure) where T : ChestFxHook
        {
            string path = HookFolder + "/RootHook_" + key + ".prefab";
            GameObject temp = new GameObject("RootHook_" + key);
            try
            {
                T hook = temp.AddComponent<T>();
                configure?.Invoke(hook);
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(temp, path);
                return saved != null ? saved.GetComponent<T>() : null;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(temp);
            }
        }

        private static AudioClip Clip(string path)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (c == null) Debug.LogWarning(Log + "Không thấy âm " + path + " (hiệu ứng vẫn chạy, chỉ thiếu tiếng).");
            return c;
        }

        private static void SetClip(SerializedObject so, string field, string path, string fallback = null)
        {
            SerializedProperty prop = so.FindProperty(field);
            if (prop == null) { Debug.LogWarning(Log + "Hook không có trường " + field); return; }
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (c == null && fallback != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(fallback);
            if (c == null) Debug.LogWarning(Log + "Không thấy âm " + path);
            prop.objectReferenceValue = c;
        }

        // ------------------------------------------------------------------ Items, hand menus, folders

        private static List<GameObject> LoadItems(out int failures)
        {
            failures = 0;
            List<GameObject> result = new List<GameObject>();
            if (!AssetDatabase.IsValidFolder(ItemsFolder)) return result;
            string[] files = Directory.GetFiles(ItemsFolder, "*.glb", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.Ordinal);
            foreach (string file in files)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(file.Replace('\\', '/'));
                if (go != null) result.Add(go);
                else failures++;
            }
            return result;
        }

        private static int AssignToHandMenus(ChestFxLibrary library)
        {
            int count = 0;
            foreach (ChestHandMenu menu in Resources.FindObjectsOfTypeAll<ChestHandMenu>())
            {
                if (EditorUtility.IsPersistent(menu) || !menu.gameObject.scene.IsValid()) continue;   // skip prefab assets
                SerializedObject so = new SerializedObject(menu);
                SerializedProperty prop = so.FindProperty("fxLibrary");
                if (prop == null) continue;
                prop.objectReferenceValue = library;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
                count++;
            }
            return count;
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

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
    /// Tools/Cho Do La/Create All Chest FX Profiles: dựng hồ sơ của cả 17 rương bằng <see cref="ChestOpenSetup"/>
    /// (chuỗi mở chuẩn; hook âm thanh nằm trong customFxPrefab của hồ sơ), rồi ghi
    /// ScriptableObjects/ChestFx/ChestFxLibrary.asset (mọi hồ sơ và mọi món Art/Items/LowPoly/**/*.glb) và gán cho từng
    /// ChestHandMenu trong cảnh đang mở, để [P] trong menu tay chạy được cả chuỗi mở. Không quét gì lúc chạy game.
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
            // Danh sách rương lấy thẳng từ bảng đăng ký (đủ 17 dòng), không liệt kê lại ở đây.
            var makers = new List<KeyValuePair<string, Func<ChestFxProfile>>>();
            foreach (ChestPreset preset in ChestPresetRegistry.All)
            {
                string name = preset.displayName;
                makers.Add(Maker(name, () => ChestOpenSetup.CreateOrUpdate(name)));
            }

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
                    biteIntro = false,
                    spinRecord = false,
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

        // ------------------------------------------------------------------ Hook trên gốc rương

        /// <summary>Hook của mọi rương nằm trong customFxPrefab của hồ sơ, nên không có hook gắn lên gốc rương.</summary>
        private static ChestFxHook RootHookTemplate(string key) => null;

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

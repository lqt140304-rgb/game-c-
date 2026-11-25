#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ChoDoLa.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Adds the chest test menu (<see cref="ChestHandMenu"/>) to the player camera and fills its list with every
    /// Chest_* model in Assets/_Project/Art/Chests (sorted by name). Run "Create Sprint 0 Test Scene" first.
    /// "Refresh Chest List" scans the folder again, for example after more chests were merged in.
    /// </summary>
    public static class ChestHandMenuSetup
    {
        public const string ChestFolder = "Assets/_Project/Art/Chests";
        private const string Log = "[ChestHandMenu] ";
        private const string GltfPackage = "com.unity.cloud.gltfast";

        [MenuItem("Tools/Cho Do La/Add Chest Hand Menu")]
        public static void AddChestHandMenu()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError(Log + "Không có camera gắn tag MainCamera trong cảnh đang mở. Chạy Tools/Cho Do La/Create Sprint 0 Test Scene trước.");
                return;
            }

            HeldItemViewmodel viewmodel = cam.GetComponentInChildren<HeldItemViewmodel>(true);
            if (viewmodel == null)
            {
                GameObject anchor = new GameObject("HeldItemAnchor");
                anchor.transform.SetParent(cam.transform, false);
                viewmodel = anchor.AddComponent<HeldItemViewmodel>();
            }

            ChestHandMenu menu = cam.GetComponentInChildren<ChestHandMenu>(true);
            if (menu == null) menu = viewmodel.gameObject.AddComponent<ChestHandMenu>();

            SerializedObject serialized = new SerializedObject(menu);
            serialized.FindProperty("viewmodel").objectReferenceValue = viewmodel;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            int found = Fill(menu);
            EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
            Debug.Log(Log + $"Đã thêm menu cầm rương vào camera ({found} rương). Nhấn Play rồi bấm [C] để chọn rương, [O] mở nắp, [R] đóng, [F] soi, [Q] cất, [ ] đổi rương.");
        }

        [MenuItem("Tools/Cho Do La/Refresh Chest List")]
        public static void RefreshChestList()
        {
            List<ChestHandMenu> menus = new List<ChestHandMenu>();
            foreach (ChestHandMenu m in Resources.FindObjectsOfTypeAll<ChestHandMenu>())
            {
                if (EditorUtility.IsPersistent(m) || !m.gameObject.scene.IsValid()) continue;   // skip prefab assets
                menus.Add(m);
            }

            if (menus.Count == 0)
            {
                Debug.LogWarning(Log + "Cảnh đang mở chưa có ChestHandMenu. Chạy Tools/Cho Do La/Add Chest Hand Menu trước.");
                return;
            }

            foreach (ChestHandMenu m in menus)
            {
                Fill(m);
                EditorSceneManager.MarkSceneDirty(m.gameObject.scene);
            }
        }

        /// <summary>Scan the chest folder and put the result in <paramref name="menu"/>. Returns how many chests were found.</summary>
        private static int Fill(ChestHandMenu menu)
        {
            List<GameObject> chests = FindChests();
            Undo.RecordObject(menu, "Refresh Chest List");
            menu.chests = chests;
            EditorUtility.SetDirty(menu);
            return chests.Count;
        }

        /// <summary>Every Chest_*.glb / .gltf / .fbx / .prefab in the chest folder (prefab wins over a model of the same name).</summary>
        public static List<GameObject> FindChests()
        {
            List<GameObject> result = new List<GameObject>();
            if (!AssetDatabase.IsValidFolder(ChestFolder))
            {
                Debug.LogWarning(Log + $"Không có thư mục {ChestFolder}.");
                return result;
            }

            // Pick one path per chest name; a prefab (which may carry a tuned BoxOpenSequence) beats the raw model.
            Dictionary<string, string> pathByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[] { ChestFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path).ToLowerInvariant();
                if (!name.StartsWith("Chest_", StringComparison.Ordinal)) continue;
                if (ext != ".glb" && ext != ".gltf" && ext != ".fbx" && ext != ".prefab") continue;

                if (pathByName.TryGetValue(name, out string existing))
                {
                    if (ext == ".prefab") pathByName[name] = path;
                    Debug.Log(Log + $"{name}: có cả {Path.GetFileName(existing)} và {Path.GetFileName(path)}, dùng {Path.GetFileName(pathByName[name])}.");
                }
                else
                {
                    pathByName[name] = path;
                }
            }

            List<string> names = new List<string>(pathByName.Keys);
            names.Sort(StringComparer.OrdinalIgnoreCase);

            List<string> unreadable = new List<string>();
            List<string> lfsPointers = new List<string>();
            StringBuilder summary = new StringBuilder();

            foreach (string name in names)
            {
                string path = pathByName[name];
                if (IsLfsPointer(path))
                {
                    lfsPointers.Add(path);
                    continue;
                }

                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                {
                    unreadable.Add(path);
                    continue;
                }

                if (asset.GetComponentsInChildren<Renderer>(true).Length == 0)
                {
                    Debug.LogWarning(Log + $"{path} nhập ra rỗng (không có mesh). Thử chuột phải vào file > Reimport; nếu vẫn rỗng thì file hỏng hoặc chưa git lfs pull.");
                }
                else if (!HasDescendant(asset.transform, "Lid"))
                {
                    Debug.LogWarning(Log + $"{path} không có lớp \"Lid\": cầm được nhưng phím [O] sẽ không mở nắp.");
                }

                result.Add(asset);
                summary.Append(summary.Length == 0 ? string.Empty : ", ").Append(name.Substring("Chest_".Length));
            }

            if (lfsPointers.Count > 0)
            {
                Debug.LogWarning(Log + $"{lfsPointers.Count} file mới là con trỏ Git LFS (vài trăm byte), chưa có mô hình thật: {string.Join(", ", lfsPointers)}.\n"
                    + "Cài Git LFS (https://git-lfs.com), chạy \"git lfs install\" rồi \"git lfs pull\" trong thư mục dự án, quay lại Unity rồi chạy Tools/Cho Do La/Refresh Chest List.");
            }

            if (unreadable.Count > 0)
            {
                if (!IsGltfastInstalled())
                {
                    Debug.LogWarning(Log + $"Unity chưa đọc được {unreadable.Count} file .glb ({string.Join(", ", unreadable)}) vì CHƯA CÀI glTFast.\n"
                        + $"Cài: Window > Package Manager > nút + > Install package by name..., gõ {GltfPackage}, bấm Install. "
                        + "Đợi Unity nhập lại xong rồi chạy Tools/Cho Do La/Refresh Chest List. (Script này không tự sửa Packages/manifest.json.)");
                }
                else
                {
                    Debug.LogWarning(Log + $"Không nạp được: {string.Join(", ", unreadable)}. Đã có glTFast; thử chuột phải vào file > Reimport, xem lỗi nhập trong Console.");
                }
            }

            Debug.Log(Log + $"Tìm thấy {result.Count} rương trong {ChestFolder}" + (result.Count > 0 ? $": {summary}." : "."));
            return result;
        }

        private static bool HasDescendant(Transform t, string name)
        {
            foreach (Transform child in t.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name) return true;
            }
            return false;
        }

        private static bool IsLfsPointer(string assetPath)
        {
            try
            {
                FileInfo info = new FileInfo(assetPath);
                if (!info.Exists || info.Length > 1024) return false;
                return File.ReadAllText(assetPath).StartsWith("version https://git-lfs", StringComparison.Ordinal);
            }
            catch (IOException)
            {
                return false;
            }
        }

        private static bool IsGltfastInstalled()
        {
            if (File.Exists(Path.Combine("Packages", "packages-lock.json"))
                && File.ReadAllText(Path.Combine("Packages", "packages-lock.json")).Contains("\"" + GltfPackage + "\""))
            {
                return true;
            }
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "glTFast") return true;
            }
            return false;
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Dựng hồ sơ rương theo chuỗi mở chuẩn (mở nắp, chuyển động 3 nấc, bóp méo theo bậc, âm bám sự kiện). Dữ liệu từng rương lấy từ
    /// <see cref="ChestPresetRegistry"/> (file Chests/&lt;Tên&gt;Preset.cs). Mỗi lần chạy dựng lại ChestFx_&lt;Tên&gt;.asset từ đầu (giữ
    /// đường dẫn và GUID), ChestScript_&lt;Tên&gt;.asset và prefab SoundHooks/&lt;Tên&gt;SoundHook.prefab (chứa <see cref="ChestSoundHook"/>
    /// đã điền clip theo preset). Khe âm rỗng thì để trống, im, không báo lỗi; khe ghi tên mà không thấy tệp thì cảnh báo vàng.
    /// Menu: Tools/Cho Do La/Mo Ruong/Setup &lt;Tên&gt; Chest và Setup All. Cũng dọn prefab cũ của "vật mở khoá" (thư mục KeyHooks) nếu còn.
    /// </summary>
    public static class ChestOpenSetup
    {
        private const string Log = "[MoRuong] ";
        public const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        public const string HookFolder = ProfileFolder + "/SoundHooks";
        private const string OldKeyHookFolder = ProfileFolder + "/KeyHooks";

        /// <summary>Dựng lại hồ sơ của một rương (theo displayName, ví dụ "Golden").</summary>
        public static ChestFxProfile CreateOrUpdate(string displayName)
        {
            ChestPreset preset = ChestPresetRegistry.Get(displayName);
            if (preset == null)
            {
                Debug.LogError(Log + "Không có rương tên " + displayName + " trong ChestPresetRegistry.");
                return null;
            }
            EnsureFolder(ProfileFolder);
            EnsureFolder(HookFolder);
            RemoveOldKeyHooks();

            string profilePath = ProfileFolder + "/ChestFx_" + displayName + ".asset";
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(profilePath);
            bool created = p == null;
            if (created)
            {
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                AssetDatabase.CreateAsset(p, profilePath);
            }
            else
            {
                // Dựng từ đầu: ghi đè mọi trường bằng giá trị mặc định, giữ nguyên tệp và GUID.
                ChestFxProfile fresh = ScriptableObject.CreateInstance<ChestFxProfile>();
                EditorUtility.CopySerialized(fresh, p);
                UnityEngine.Object.DestroyImmediate(fresh);
            }

            p.chestId = preset.chestId;
            p.minimalFx = true;
            p.minimalGlowScale = 0.35f;
            p.minimalLightScale = 0.25f;
            p.tension.enabled = false;
            ChestAudioRule.ApplyToProfile(p);

            string scriptPath = ProfileFolder + "/ChestScript_" + displayName + ".asset";
            ChestScript script = AssetDatabase.LoadAssetAtPath<ChestScript>(scriptPath);
            if (script == null)
            {
                script = ScriptableObject.CreateInstance<ChestScript>();
                AssetDatabase.CreateAsset(script, scriptPath);
            }
            ChestOpenScript.Build(script, preset);
            EditorUtility.SetDirty(script);
            p.script = script;

            p.customFxPrefab = SaveHookPrefab(preset);

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            Debug.Log(Log + (created ? "Tạo " : "Dựng lại ") + profilePath);
            return p;
        }

        private static GameObject SaveHookPrefab(ChestPreset preset)
        {
            string path = HookFolder + "/" + preset.displayName + "SoundHook.prefab";
            GameObject temp = new GameObject(preset.displayName + "SoundHook");
            try
            {
                ChestSoundHook hook = temp.AddComponent<ChestSoundHook>();
                hook.presetName = preset.displayName;
                hook.bedClip = Clip(preset, preset.bedFile, "nền");
                hook.accentClips = new AudioClip[3];
                for (int i = 0; i < 3; i++)
                    hook.accentClips[i] = preset.accentFiles != null && i < preset.accentFiles.Length ? Clip(preset, preset.accentFiles[i], "nhấn nấc " + (i + 1)) : null;
                hook.lockClips = new AudioClip[5];
                for (int i = 0; i < 5; i++)
                    hook.lockClips[i] = preset.lockFiles != null && i < preset.lockFiles.Length ? Clip(preset, preset.lockFiles[i], "chốt bậc " + i) : null;
                return PrefabUtility.SaveAsPrefabAsset(temp, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(temp);
            }
        }

        /// <summary>Khe rỗng = null (im, không báo lỗi). Khe có tên mà không thấy tệp = cảnh báo vàng, null.</summary>
        private static AudioClip Clip(ChestPreset preset, string file, string slot)
        {
            if (string.IsNullOrEmpty(file)) return null;
            string path = preset.audioFolder + file;
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (c == null) Debug.LogWarning(Log + preset.displayName + ": khe " + slot + " ghi " + path + " nhưng không thấy tệp (để trống, rương vẫn chạy, chỉ thiếu tiếng này).");
            return c;
        }

        private static void RemoveOldKeyHooks()
        {
            if (!AssetDatabase.IsValidFolder(OldKeyHookFolder)) return;
            if (AssetDatabase.DeleteAsset(OldKeyHookFolder)) Debug.Log(Log + "Đã xoá thư mục prefab cũ của vật mở khoá: " + OldKeyHookFolder);
        }

        private static void Run(string name)
        {
            ChestFxProfile p = CreateOrUpdate(name);
            if (p != null) Selection.activeObject = p;
        }

        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup All")]
        public static void SetupAll()
        {
            CreateAllChestFxProfiles.CreateAll();
        }

        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Golden Chest")] private static void SetupGolden() { Run("Golden"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Hai Tac Chest")] private static void SetupHaiTac() { Run("HaiTac"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Rac Quy Chest")] private static void SetupRacQuy() { Run("RacQuy"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Magic Chest")] private static void SetupMagic() { Run("Magic"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Mimic Chest")] private static void SetupMimic() { Run("Mimic"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Miner Chest")] private static void SetupMiner() { Run("Miner"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Am Nhac Chest")] private static void SetupAmNhac() { Run("AmNhac"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Viet Nam Chest")] private static void SetupVietNam() { Run("VietNam"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Tuoi Tho Chest")] private static void SetupTuoiTho() { Run("TuoiTho"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Robot Chest")] private static void SetupRobot() { Run("Robot"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Khong Gian Chest")] private static void SetupKhongGian() { Run("KhongGian"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Game Chest")] private static void SetupGame() { Run("Game"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup The Thao Chest")] private static void SetupTheThao() { Run("TheThao"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Trung Hoa Chest")] private static void SetupTrungHoa() { Run("TrungHoa"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Nhat Ban Chest")] private static void SetupNhatBan() { Run("NhatBan"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Co Phong Chest")] private static void SetupCoPhong() { Run("CoPhong"); }
        [MenuItem("Tools/Cho Do La/Mo Ruong/Setup Thien Co Chest")] private static void SetupThienCo() { Run("ThienCo"); }

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

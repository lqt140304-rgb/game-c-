#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Dựng hồ sơ rương theo chuỗi mở bằng vật mở khoá (kế hoạch 2026-11-26, mục 4.4). Mỗi lần chạy dựng lại
    /// ChestFx_&lt;Tên&gt;.asset từ đầu (giữ đường dẫn và GUID), ChestScript_&lt;Tên&gt;.asset và prefab hook
    /// KeyHooks/&lt;Tên&gt;KeyHook.prefab. Âm lấy từ Preset; tệp mp3 thiếu thì ô trống và cảnh báo.
    /// Menu: Tools/Cho Do La/Chia Khoa/Setup &lt;Tên&gt; Chest và Setup All.
    /// </summary>
    public static class ChestKeySetup
    {
        private const string Log = "[ChiaKhoa] ";
        public const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        public const string HookFolder = ProfileFolder + "/KeyHooks";

        private readonly struct Entry
        {
            public readonly Type hookType;
            public readonly KeyPreset preset;
            public Entry(Type t, KeyPreset p) { hookType = t; preset = p; }
        }

        private static readonly Dictionary<string, Entry> Table = new Dictionary<string, Entry>
        {
            { "Golden", new Entry(typeof(GoldenKeyHook), GoldenKeyHook.Preset) },
            { "HaiTac", new Entry(typeof(HaiTacKeyHook), HaiTacKeyHook.Preset) },
            { "RacQuy", new Entry(typeof(RacQuyKeyHook), RacQuyKeyHook.Preset) },
            { "Magic", new Entry(typeof(MagicKeyHook), MagicKeyHook.Preset) },
            { "Mimic", new Entry(typeof(MimicKeyHook), MimicKeyHook.Preset) },
            { "Miner", new Entry(typeof(MinerKeyHook), MinerKeyHook.Preset) },
            { "AmNhac", new Entry(typeof(AmNhacKeyHook), AmNhacKeyHook.Preset) },
            { "VietNam", new Entry(typeof(VietNamKeyHook), VietNamKeyHook.Preset) },
            { "TuoiTho", new Entry(typeof(TuoiThoKeyHook), TuoiThoKeyHook.Preset) },
            { "Robot", new Entry(typeof(RobotKeyHook), RobotKeyHook.Preset) },
            { "KhongGian", new Entry(typeof(KhongGianKeyHook), KhongGianKeyHook.Preset) },
            { "Game", new Entry(typeof(GameKeyHook), GameKeyHook.Preset) },
            { "TheThao", new Entry(typeof(TheThaoKeyHook), TheThaoKeyHook.Preset) },
            { "TrungHoa", new Entry(typeof(TrungHoaKeyHook), TrungHoaKeyHook.Preset) },
            { "NhatBan", new Entry(typeof(NhatBanKeyHook), NhatBanKeyHook.Preset) },
            { "CoPhong", new Entry(typeof(CoPhongKeyHook), CoPhongKeyHook.Preset) },
            { "ThienCo", new Entry(typeof(ThienCoKeyHook), ThienCoKeyHook.Preset) },
        };

        public static readonly string[] AllNames = new[] { "Golden", "HaiTac", "RacQuy", "Magic", "Mimic", "Miner", "AmNhac", "VietNam", "TuoiTho", "Robot", "KhongGian", "Game", "TheThao", "TrungHoa", "NhatBan", "CoPhong", "ThienCo" };

        /// <summary>Dựng lại hồ sơ của một rương (theo displayName, ví dụ "Golden").</summary>
        public static ChestFxProfile CreateOrUpdate(string displayName)
        {
            if (!Table.TryGetValue(displayName, out Entry e))
            {
                Debug.LogError(Log + "Không có rương tên " + displayName);
                return null;
            }
            KeyPreset preset = e.preset;
            EnsureFolder(ProfileFolder);
            EnsureFolder(HookFolder);

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
            p.secondaryColor = preset.mainColor;
            p.tension.enabled = false;
            ChestAudioRule.ApplyToProfile(p);

            string scriptPath = ProfileFolder + "/ChestScript_" + displayName + ".asset";
            ChestScript script = AssetDatabase.LoadAssetAtPath<ChestScript>(scriptPath);
            if (script == null)
            {
                script = ScriptableObject.CreateInstance<ChestScript>();
                AssetDatabase.CreateAsset(script, scriptPath);
            }
            ChestKeyScript.Build(script, preset);
            EditorUtility.SetDirty(script);
            p.script = script;

            p.customFxPrefab = SaveHookPrefab(displayName, e);

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            Debug.Log(Log + (created ? "Tạo " : "Dựng lại ") + profilePath);
            return p;
        }

        private static GameObject SaveHookPrefab(string displayName, Entry e)
        {
            KeyPreset preset = e.preset;
            string path = HookFolder + "/" + displayName + "KeyHook.prefab";
            GameObject temp = new GameObject(displayName + "KeyHook");
            try
            {
                ChestKeyHookBase hook = (ChestKeyHookBase)temp.AddComponent(e.hookType);
                hook.keyColor = preset.mainColor;
                hook.bedClip = Clip(preset.audioFolder + preset.bedFile);
                hook.keyClip = string.IsNullOrEmpty(preset.keyFile) ? null : Clip(preset.audioFolder + preset.keyFile);
                hook.flyClip = string.IsNullOrEmpty(preset.flyFile) ? null : Clip(preset.audioFolder + preset.flyFile);
                hook.actionClip = string.IsNullOrEmpty(preset.actionFile) ? null : Clip(preset.audioFolder + preset.actionFile);
                // lightColor/lightMul/moveStyle không lưu ở hồ sơ: hook đọc thẳng từ Preset của lớp (GetPreset).
                hook.stingClips = new AudioClip[KeyAudioNames.LockSuffix.Length];
                for (int i = 0; i < hook.stingClips.Length; i++)
                    hook.stingClips[i] = Clip(preset.audioFolder + preset.filePrefix + KeyAudioNames.LockSuffix[i]);
                return PrefabUtility.SaveAsPrefabAsset(temp, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(temp);
            }
        }

        private static AudioClip Clip(string path)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (c == null) Debug.LogWarning(Log + "Không thấy âm " + path + " (để trống, rương vẫn chạy, chỉ thiếu tiếng).");
            return c;
        }

        private static void Run(string name)
        {
            ChestFxProfile p = CreateOrUpdate(name);
            if (p != null) Selection.activeObject = p;
        }

        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup All")]
        public static void SetupAll()
        {
            foreach (string n in AllNames)
            {
                try { CreateOrUpdate(n); }
                catch (Exception ex) { Debug.LogError(Log + n + ": " + ex); }
            }
            CreateAllChestFxProfiles.CreateAll();
        }

        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Golden Chest")] private static void SetupGolden() { Run("Golden"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Hai Tac Chest")] private static void SetupHaiTac() { Run("HaiTac"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Rac Quy Chest")] private static void SetupRacQuy() { Run("RacQuy"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Magic Chest")] private static void SetupMagic() { Run("Magic"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Mimic Chest")] private static void SetupMimic() { Run("Mimic"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Miner Chest")] private static void SetupMiner() { Run("Miner"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Am Nhac Chest")] private static void SetupAmNhac() { Run("AmNhac"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Viet Nam Chest")] private static void SetupVietNam() { Run("VietNam"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Tuoi Tho Chest")] private static void SetupTuoiTho() { Run("TuoiTho"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Robot Chest")] private static void SetupRobot() { Run("Robot"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Khong Gian Chest")] private static void SetupKhongGian() { Run("KhongGian"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Game Chest")] private static void SetupGame() { Run("Game"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup The Thao Chest")] private static void SetupTheThao() { Run("TheThao"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Trung Hoa Chest")] private static void SetupTrungHoa() { Run("TrungHoa"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Nhat Ban Chest")] private static void SetupNhatBan() { Run("NhatBan"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Co Phong Chest")] private static void SetupCoPhong() { Run("CoPhong"); }
        [MenuItem("Tools/Cho Do La/Chia Khoa/Setup Thien Co Chest")] private static void SetupThienCo() { Run("ThienCo"); }

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

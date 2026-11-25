#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Viet Nam Hoi Lang FX: dựng prefab VietNamHoiLangFx.prefab mang VietNamHoiLangHook (gắn sẵn
    /// âm ở Audio/Chests/viet_nam/hoi_lang/) và đặt làm customFxPrefab của ChestFx_VietNam.asset (thay prefab VietNamFx cũ).
    /// Hồ sơ chưa có thì tạo bằng VietNamFxSetup trước. Chạy lại được nhiều lần (ghi đè prefab).
    /// </summary>
    public static class VietNamHoiLangSetup
    {
        private const string PrefabPath = "Assets/_Project/ScriptableObjects/ChestFx/VietNamHoiLangFx.prefab";
        private const string AudioFolder = "Assets/_Project/Audio/Chests/viet_nam/hoi_lang/";

        [MenuItem("Tools/Cho Do La/Create Viet Nam Hoi Lang FX")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdateProfile();
        }

        /// <summary>Tạo/cập nhật hồ sơ ChestFx_VietNam với prefab Hồi Làng; trả hồ sơ (làm maker cho thư viện menu cầm rương).</summary>
        public static ChestFxProfile CreateOrUpdateProfile()
        {
            ChestFxProfile p = VietNamFxSetup.CreateOrUpdate();
            p.customFxPrefab = BuildPrefab();
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            Debug.Log("[VietNamHoiLang] ChestFx_VietNam dùng kịch bản Đêm hội làng (" + PrefabPath + ").");
            return p;
        }

        private static GameObject BuildPrefab()
        {
            GameObject temp = new GameObject("VietNamHoiLangFx");
            VietNamHoiLangHook h = temp.AddComponent<VietNamHoiLangHook>();
            int missing = 0;
            h.droneClip = Clip("hl_drone.wav", ref missing);
            h.drumClip = Clip("hl_drum.wav", ref missing);
            h.moClip = Clip("hl_mo.wav", ref missing);
            h.plopClip = Clip("hl_plop.wav", ref missing);
            h.deflateClip = Clip("hl_deflate.wav", ref missing);
            h.bloopClip = Clip("hl_bloop.wav", ref missing);
            h.frogClip = Clip("hl_frog.wav", ref missing);
            h.boomClip = Clip("hl_boom.wav", ref missing);
            h.flapClip = Clip("hl_flap.wav", ref missing);
            h.turtleClip = Clip("hl_turtle.wav", ref missing);
            if (missing > 0) Debug.LogWarning($"[VietNamHoiLang] Thiếu {missing} âm trong {AudioFolder} (chạy Tools/Audio/make_vietnam_hoilang_sfx.py). Hiệu ứng vẫn chạy, chỉ im.");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, PrefabPath);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        private static AudioClip Clip(string file, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + file);
            if (c == null) missing++;
            return c;
        }
    }
}
#endif

#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Viet Nam Hoi Lang FX: dựng prefab VietNamHoiLangFx.prefab mang VietNamHoiLangHook (gắn sẵn
    /// âm .mp3 ElevenLabs ở Audio/Chests/viet_nam/) và đặt làm customFxPrefab của ChestFx_VietNam.asset (thay prefab VietNamFx cũ).
    /// Hồ sơ chưa có thì tạo bằng VietNamFxSetup trước. Chạy lại được nhiều lần (ghi đè prefab).
    /// </summary>
    public static class VietNamHoiLangSetup
    {
        private const string PrefabPath = "Assets/_Project/ScriptableObjects/ChestFx/VietNamHoiLangFx.prefab";
        private const string AudioFolder = "Assets/_Project/Audio/Chests/viet_nam/";

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
            ChestAudioRule.ApplyToProfile(p); // MỖI lần chạy: tension tắt, âm lõi câm (A1); hook tự phát âm.
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
            // Chỉ nạp .mp3 ElevenLabs (danh mục Tools/Audio/elevenlabs_chest_manifest.json, mục viet_nam); không nạp .wav numpy nữa.
            h.droneClip = Clip("sfx_vietnam_dan_bau_loop.mp3", ref missing);
            h.drumClip = Clip("sfx_vietnam_trong.mp3", ref missing);
            h.moClip = Clip("sfx_vietnam_tick_mo.mp3", ref missing);
            h.plopClip = Clip("sfx_vietnam_lantern_in.mp3", ref missing);
            h.deflateClip = Clip("sfx_vietnam_dan_bau_xit.mp3", ref missing);
            h.frogClip = Clip("sfx_vietnam_frog.mp3", ref missing);
            h.boomClip = Clip("sfx_vietnam_trong_dong.mp3", ref missing);
            h.stingClips = new AudioClip[5];
            string[] tiers = { "rac", "thuong", "hiem", "ratHiem", "huyenThoai" };
            for (int i = 0; i < tiers.Length; i++) h.stingClips[i] = Clip("sfx_vietnam_lock_" + tiers[i] + ".mp3", ref missing);
            if (missing > 0) Debug.LogWarning($"[VietNamHoiLang] Thiếu {missing} âm trong {AudioFolder} (chạy Tools/Audio/make_elevenlabs_chest_sfx.py --chest viet_nam). Hiệu ứng vẫn chạy, chỉ im.");
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

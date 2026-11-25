#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Setup Co Phong Ngu Kiem: tạo (hoặc cập nhật) hồ sơ ChestFx_CoPhong bằng CoPhongFxSetup, rồi tạo
    /// prefab ChestFx_CoPhongNguKiem.prefab mang CoPhongNguKiemHook (gán âm ElevenLabs .mp3 trong Audio/Chests/co_phong) và đặt
    /// nó làm customFxPrefab của hồ sơ — thay prefab mực tàu cũ (CoPhongHook không bị xoá, chỉ không còn được dùng).
    /// </summary>
    public static class CoPhongNguKiemSetup
    {
        private const string PrefabPath = "Assets/_Project/ScriptableObjects/ChestFx/ChestFx_CoPhongNguKiem.prefab";
        public const string Folder = "Assets/_Project/Audio/Chests/co_phong/";

        [MenuItem("Tools/Cho Do La/Setup Co Phong Ngu Kiem")]
        public static void Run()
        {
            Selection.activeObject = CreateOrUpdateProfile();
        }

        /// <summary>Tạo/cập nhật hồ sơ ChestFx_CoPhong và đặt prefab Ngự Kiếm làm customFxPrefab; trả hồ sơ (làm maker cho thư viện menu cầm rương).</summary>
        public static ChestFxProfile CreateOrUpdateProfile()
        {
            ChestFxProfile p = CoPhongFxSetup.CreateOrUpdate();
            GameObject go = new GameObject("ChestFx_CoPhongNguKiem");
            CoPhongNguKiemHook h = go.AddComponent<CoPhongNguKiemHook>();
            h.humLoop = Clip("sword_hum_loop");
            h.drawSword = Clip("sword_draw");
            h.strike = Clip("strike");
            h.stone = Clip("spirit_stone");
            h.swordLand = Clip("sword_land");
            h.stingClips = new[] { Clip("lock_rac"), Clip("lock_thuong"), Clip("lock_hiem"), Clip("lock_ratHiem"), Clip("lock_huyenThoai") };
            p.customFxPrefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            ChestAudioRule.ApplyToProfile(p);   // A1: mỗi lần chạy menu, không chỉ khi ô trống
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            Debug.Log("[NguKiem] ChestFx_CoPhong dùng " + PrefabPath);
            return p;
        }

        private static AudioClip Clip(string n)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + "sfx_cophong_" + n + ".mp3");
            if (c == null) Debug.LogWarning("[NguKiem] thiếu âm " + n + " (chạy Tools/Audio/make_elevenlabs_chest_sfx.py --chest co_phong)");
            return c;
        }
    }
}
#endif

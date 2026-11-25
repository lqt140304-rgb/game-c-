#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Setup Co Phong Ngu Kiem: tạo (hoặc cập nhật) hồ sơ ChestFx_CoPhong bằng CoPhongFxSetup, rồi tạo
    /// prefab ChestFx_CoPhongNguKiem.prefab mang CoPhongNguKiemHook (gán âm trong Audio/Chests/co_phong/ngu_kiem) và đặt
    /// nó làm customFxPrefab của hồ sơ — thay prefab mực tàu cũ (CoPhongHook không bị xoá, chỉ không còn được dùng).
    /// </summary>
    public static class CoPhongNguKiemSetup
    {
        private const string PrefabPath = "Assets/_Project/ScriptableObjects/ChestFx/ChestFx_CoPhongNguKiem.prefab";
        public const string Folder = "Assets/_Project/Audio/Chests/co_phong/ngu_kiem/";

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
            h.humLoop = Clip("hum_loop");
            h.drawSword = Clip("draw_sword");
            h.strike = Clip("strike");
            h.stone = Clip("stone");
            h.crane = Clip("crane");
            h.swordLand = Clip("sword_land");
            h.endByTier = new[] { Clip("end_rac"), Clip("end_thuong"), Clip("end_hiem"), Clip("end_ratHiem"), Clip("end_huyenThoai") };
            p.customFxPrefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            Debug.Log("[NguKiem] ChestFx_CoPhong dùng " + PrefabPath);
            return p;
        }

        private static AudioClip Clip(string n)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + "sfx_ngukiem_" + n + ".wav");
            if (c == null) Debug.LogWarning("[NguKiem] thiếu âm " + n + " (chạy Tools/Audio/make_cophong_ngukiem_sfx.py)");
            return c;
        }
    }
}
#endif

#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Kịch bản "Trục vớt bằng đại bác" cho Chest_HaiTac (thay HaiTacHook cũ).
    /// - Tạo hồ sơ ScriptableObjects/ChestFx/ChestFx_HaiTacSeq.asset, rồi MỖI LẦN chạy menu gọi ChestAudioRule.ApplyToProfile
    ///   (lõi gần như câm, tension tắt; nền + tiếng chốt do HaiTacSeqHook phát).
    /// - Menu "Add Hai Tac Seq To Selected Chest": gắn HaiTacSeqHook (đã điền âm) + hồ sơ lên rương đang chọn, gỡ HaiTacHook cũ.
    /// Âm: Audio/Chests/hai_tac/*.mp3 (ElevenLabs, danh mục Tools/Audio/elevenlabs_chest_manifest.json, mục hai_tac).
    /// </summary>
    public static class HaiTacSeqSetup
    {
        public const string AudioFolder = "Assets/_Project/Audio/Chests/hai_tac/";
        private const string ProfilePath = "Assets/_Project/ScriptableObjects/ChestFx/ChestFx_HaiTacSeq.asset";
        private const string Tag = "[HaiTacSeq] ";

        [MenuItem("Tools/Cho Do La/Add Hai Tac Seq (Cannon) To Selected Chest")]
        public static void AddToSelectedMenu()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) { Debug.LogError(Tag + "Chọn rương Chest_HaiTac trong cảnh trước."); return; }
            Transform rig = ChestRevealDemo.FindRig(go.transform);
            ChestRevealSequence reveal = rig.GetComponent<ChestRevealSequence>();
            if (reveal == null) { Debug.LogWarning(Tag + "Chưa có ChestRevealSequence: chạy Tools/Cho Do La/Add Chest Reveal Test trước."); return; }
            SerializedObject so = new SerializedObject(reveal);
            so.FindProperty("profile").objectReferenceValue = CreateOrUpdateProfile();
            so.ApplyModifiedPropertiesWithoutUndo();

            HaiTacHook old = rig.GetComponent<HaiTacHook>();
            if (old != null) Object.DestroyImmediate(old);
            HaiTacSeqHook hook = rig.GetComponent<HaiTacSeqHook>();
            if (hook == null) hook = rig.gameObject.AddComponent<HaiTacSeqHook>();
            FillClips(hook);
            EditorUtility.SetDirty(hook);
            Selection.activeGameObject = rig.gameObject;
            Debug.Log(Tag + "Đã gắn kịch bản đại bác lên " + rig.name + ". Play, bấm mở; [G] bắn, [B] thả thùng.");
        }

        public static ChestFxProfile CreateOrUpdateProfile()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            if (p == null)
            {
                ChestFxProfile baseProfile = HaiTacFxSetup.CreateOrUpdateProfile();   // lấy phần hạt/màu của hải tặc (âm lõi bị tắt ngay sau đó)
                p = Object.Instantiate(baseProfile);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }
            ChestAudioRule.ApplyToProfile(p);   // A1: ghi lại MỖI lần chạy menu, không chỉ khi hồ sơ trống
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            return p;
        }

        public static void FillClips(HaiTacSeqHook h)
        {
            h.buildClip = C("bed_loop"); h.cannonClip = C("cannon_close"); h.splashClip = C("splash"); h.thudClip = C("thud");
            h.loadClip = C("load"); h.fuseClip = C("fuse"); h.parrotClip = C("parrot");
            h.drainClip = C("drain"); h.barrelClip = C("barrel"); h.coinClip = C("coins");
            string[] locks = { "lock_rac", "lock_thuong", "lock_hiem", "lock_ratHiem", "lock_huyenThoai" };
            h.stingClips = new AudioClip[5];
            for (int i = 0; i < 5; i++) h.stingClips[i] = C(locks[i]);
        }

        private static AudioClip C(string key)
        {
            string path = AudioFolder + "sfx_haitac_" + key + ".mp3";
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (c == null) Debug.LogWarning(Tag + "Thiếu âm " + path + " (vẫn chạy, chỉ im).");
            return c;
        }
    }
}
#endif

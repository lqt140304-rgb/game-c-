#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Kịch bản "Trục vớt bằng đại bác" cho Chest_HaiTac (thay HaiTacHook cũ).
    /// - Tạo hồ sơ ScriptableObjects/ChestFx/ChestFx_HaiTacSeq.asset: như HaiTacFxPreset nhưng BỎ âm chốt bậc và âm tiếng cười
    ///   giai đoạn 0 của lõi (đoạn kết theo bậc do HaiTacSeqHook phát, khớp với phát đại bác cuối).
    /// - Menu "Add Hai Tac Seq To Selected Chest": gắn HaiTacSeqHook (đã điền âm) + hồ sơ lên rương đang chọn, gỡ HaiTacHook cũ.
    /// Âm: Audio/Chests/hai_tac/seq/ (tạo bằng Tools/Audio/make_haitac_seq_sfx.py).
    /// </summary>
    public static class HaiTacSeqSetup
    {
        public const string AudioFolder = "Assets/_Project/Audio/Chests/hai_tac/seq/";
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
                ChestFxProfile baseProfile = HaiTacFxSetup.CreateOrUpdateProfile();   // lấy âm tick/whoosh/sóng nền của hải tặc
                p = Object.Instantiate(baseProfile);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }
            p.lockClipsByTier = new AudioClip[5];   // đoạn kết do hook phát
            if (p.stageClips != null)
                p.stageClips = System.Array.FindAll(p.stageClips, s => s.stage != 0 && s.stage != 4);
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            return p;
        }

        public static void FillClips(HaiTacSeqHook h)
        {
            h.buildClip = C("build"); h.cannonClip = C("cannon"); h.splashClip = C("splash"); h.thudClip = C("thud");
            h.loadClip = C("load"); h.fuseClip = C("fuse"); h.fizzleClip = C("fizzle"); h.parrotClip = C("parrot");
            h.drainClip = C("drain"); h.barrelClip = C("barrel"); h.coinClip = C("coin");
            string[] ends = { "0_rac", "1_thuong", "2_hiem", "3_rathiem", "4_huyenthoai" };
            h.endingClips = new AudioClip[5];
            for (int i = 0; i < 5; i++) h.endingClips[i] = C("end_" + ends[i]);
        }

        private static AudioClip C(string key)
        {
            string path = AudioFolder + "sfx_haitac_seq_" + key + ".wav";
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (c == null) Debug.LogWarning(Tag + "Thiếu âm " + path + " (vẫn chạy, chỉ im).");
            return c;
        }
    }
}
#endif

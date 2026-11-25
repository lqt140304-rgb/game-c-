#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using ChoDoLa.Interaction;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Nối kịch bản "Loa Bass Kẹo Kéo" (AmNhacBassHook) vào rương đang chọn:
    ///  - hồ sơ riêng ChestFx_AmNhacBass.asset (dựa trên AmNhacFxPreset; MỖI lần chạy menu gọi
    ///    ChestAudioRule.ApplyToProfile: tension tắt, mọi ô âm lõi trống, vì nhịp + tiếng chốt do hook phát);
    ///  - gỡ AmNhacHook cũ (vòng equaliser + đám đông) để không phát đám đông hai lần;
    ///  - tắt spinRecord của BoxOpenSequence (hook tự quay/scratch lớp Record từ phách đầu);
    ///  - nạp âm ElevenLabs .mp3 từ Audio/Chests/am_nhac/ (clips + stingClips[5]); không nạp .wav numpy nữa.
    /// Chưa chạy trong Unity (môi trường viết không có Unity): chỉ biên dịch thử bằng stub.
    /// </summary>
    public static class AmNhacBassSetup
    {
        private const string ProfilePath = "Assets/_Project/ScriptableObjects/ChestFx/ChestFx_AmNhacBass.asset";
        private static readonly string[] StingFiles = { "rac", "thuong", "hiem", "ratHiem", "huyenThoai" };

        [MenuItem("Tools/Cho Do La/Wire Selected Chest As Am Nhac (Loa Bass)")]
        public static void WireSelected()
        {
            GameObject go = Selection.activeGameObject;
            ChestRevealSequence reveal = go != null ? go.GetComponentInParent<ChestRevealSequence>() : null;
            if (reveal == null) reveal = go != null ? go.GetComponentInChildren<ChestRevealSequence>() : null;
            if (reveal == null)
            {
                Debug.LogError("[AmNhacBass] Chọn rương có ChestRevealSequence (chạy Tools/Cho Do La/Add Chest Reveal Test trước).");
                return;
            }

            ChestFxProfile p = CreateOrUpdateProfile();
            SerializedObject so = new SerializedObject(reveal);
            so.FindProperty("profile").objectReferenceValue = p;
            so.ApplyModifiedProperties();

            AmNhacHook old = reveal.GetComponent<AmNhacHook>();
            if (old != null) Undo.DestroyObjectImmediate(old);

            AmNhacBassHook hook = reveal.GetComponent<AmNhacBassHook>();
            if (hook == null) hook = Undo.AddComponent<AmNhacBassHook>(reveal.gameObject);
            FillClips(hook);
            EditorUtility.SetDirty(hook);

            BoxOpenSequence box = reveal.GetComponent<BoxOpenSequence>();
            if (box != null)
            {
                SerializedObject bso = new SerializedObject(box);
                SerializedProperty spin = bso.FindProperty("spinRecord");
                if (spin != null) { spin.boolValue = false; bso.ApplyModifiedProperties(); }
                if (box.transform.Find("Record") == null) Debug.LogWarning("[AmNhacBass] Không có lớp \"Record\": đĩa không quay/scratch, phần còn lại vẫn chạy.");
            }
            Debug.Log("[AmNhacBass] " + reveal.name + " đã nối kịch bản Loa Bass. Play → B = bấm theo nhịp, V/click = thả đĩa than.");
        }

        public static void FillClips(AmNhacBassHook hook)
        {
            hook.SetClips(LoadClips());
            hook.SetStings(LoadStings());
        }

        public static AudioClip[] LoadStings()
        {
            AudioClip[] a = new AudioClip[StingFiles.Length];
            for (int i = 0; i < a.Length; i++) a[i] = Load("lock_" + StingFiles[i]);
            return a;
        }

        public static AmNhacBassHook.Clips LoadClips()
        {
            // skip / deflate / horn: không có trong danh mục ElevenLabs (A9) -> để trống, hook bỏ qua ô trống.
            return new AmNhacBassHook.Clips
            {
                kick = Load("kick"), hat = Load("hat"), clap = Load("clap"), wub = Load("wub"),
                scratch = Load("scratch"), needle = Load("needle_drop"), riser = Load("riser"),
                drop = Load("drop"), recordStop = Load("record_stop"),
                plugPop = Load("plug_pop"), wheels = Load("wheels"), feedback = Load("feedback"),
                crowd = Load("crowd"),
            };
        }

        private static AudioClip Load(string id)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(AmNhacFxPreset.AudioFolder + "sfx_amnhac_" + id + ".mp3");
            if (c == null) Debug.LogWarning("[AmNhacBass] Thiếu âm " + id + " (chạy python3 -I Tools/Audio/make_elevenlabs_chest_sfx.py --chest am_nhac).");
            return c;
        }

        public static ChestFxProfile CreateOrUpdateProfile()
        {
            // Start from the Am Nhac profile (clips, colours), copy it, then quiet the shared layers.
            ChestFxProfile src = AmNhacFxSetup.CreateOrUpdate();
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            if (p == null)
            {
                p = Object.Instantiate(src);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }
            p.chestId = AmNhacFxPreset.ChestId;
            p.customFxPrefab = null;
            ChestAudioRule.ApplyToProfile(p);  // A1: MỖI lần chạy menu (không chỉ khi trống): tension tắt, ô âm lõi trống
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            return p;
        }
    }
}
#endif

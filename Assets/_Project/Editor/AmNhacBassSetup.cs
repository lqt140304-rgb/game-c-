#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using ChoDoLa.Interaction;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Nối kịch bản "Loa Bass Kẹo Kéo" (AmNhacBassHook) vào rương đang chọn:
    ///  - hồ sơ riêng ChestFx_AmNhacBass.asset (dựa trên AmNhacFxPreset nhưng TẮT trống nền/tick ồn của khung chung,
    ///    vì nhịp trống giờ do hook phát từ lúc bấm mở);
    ///  - gỡ AmNhacHook cũ (vòng equaliser + đám đông) để không phát đám đông hai lần;
    ///  - tắt spinRecord của BoxOpenSequence (hook tự quay/scratch lớp Record từ phách đầu);
    ///  - nạp âm từ Audio/Chests/am_nhac/seq/ (+ sfx_amnhac_crowd.mp3).
    /// Chưa chạy trong Unity (môi trường viết không có Unity): chỉ biên dịch thử bằng stub.
    /// </summary>
    public static class AmNhacBassSetup
    {
        private const string ProfilePath = "Assets/_Project/ScriptableObjects/ChestFx/ChestFx_AmNhacBass.asset";
        private const string Seq = AmNhacFxPreset.AudioFolder + "seq/";

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

        public static void FillClips(AmNhacBassHook hook) => hook.SetClips(LoadClips());

        public static AmNhacBassHook.Clips LoadClips()
        {
            return new AmNhacBassHook.Clips
            {
                kick = Load("kick"), hat = Load("hat"), clap = Load("clap"), wub = Load("wub"),
                scratch = Load("scratch"), skip = Load("skip"), needle = Load("needle"), riser = Load("riser"),
                drop = Load("drop"), horn = Load("horn"), recordStop = Load("recordstop"), deflate = Load("deflate"),
                plugPop = Load("plugpop"), wheels = Load("wheels"), feedback = Load("feedback"),
                crowd = AssetDatabase.LoadAssetAtPath<AudioClip>(AmNhacFxPreset.AudioFolder + AmNhacFxPreset.CrowdFile),
            };
        }

        private static AudioClip Load(string id)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(Seq + "sfx_amnhac_seq_" + id + ".wav");
            if (c == null) Debug.LogWarning("[AmNhacBass] Thiếu âm " + id + " (chạy python3 Tools/Audio/make_amnhac_sfx.py).");
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
            p.openAmbientClip = null;          // the hook's beat is the bed now
            p.tickVolume = 0.18f;              // roulette ticks sit under the hi-hats
            p.lockVolume = 0.55f;              // tier sting stays (tells the tier), the drop/fizzle carries the moment
            p.accentVolume = 0.2f;
            p.stageClips = new ChestFxProfile.StageClip[0];   // coin slot / needle drop are replaced by the hook's own events
            p.customFxPrefab = null;
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            return p;
        }
    }
}
#endif

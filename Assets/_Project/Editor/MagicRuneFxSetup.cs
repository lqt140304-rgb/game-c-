#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Menu dựng kịch bản "Trận pháp Rune" cho Chest_Magic (file riêng, không đụng MagicFxSetup dùng chung).
    /// - Tools/Cho Do La/Magic Rune/Setup On Selected Chest: chọn gốc Chest_Magic (có ChestRevealSequence) rồi bấm.
    ///   Thêm MagicRuneRitual, gán âm .mp3 ElevenLabs trong Audio/Chests/magic/ (theo Tools/Audio/elevenlabs_chest_manifest.json),
    ///   tắt MagicHook cũ (nếu có), và ghi hồ sơ lõi gần như câm (ChestAudioRule.ApplyToProfile) MỖI lần chạy.
    /// - Tools/Cho Do La/Magic Rune/Create Player Rune Stone: tạo một viên đá rune có Rigidbody cạnh rương đang chọn,
    ///   để thử tương tác phụ (đặt sẵn vào điểm trận, hoặc ném vào vòng khi đang tụ phép).
    /// </summary>
    public static class MagicRuneFxSetup
    {
        public const string AudioFolder = "Assets/_Project/Audio/Chests/magic/";
        public static readonly string[] StingFiles =
        {
            "sfx_magic_lock_rac.mp3", "sfx_magic_lock_thuong.mp3", "sfx_magic_lock_hiem.mp3",
            "sfx_magic_lock_ratHiem.mp3", "sfx_magic_lock_huyenThoai.mp3",
        };

        [MenuItem("Tools/Cho Do La/Magic Rune/Setup On Selected Chest")]
        public static void SetupSelected()
        {
            GameObject go = Selection.activeGameObject;
            ChestRevealSequence reveal = go != null ? go.GetComponentInParent<ChestRevealSequence>() : null;
            if (reveal == null)
            {
                Debug.LogWarning("[MagicRune] Chọn gốc Chest_Magic (object có ChestRevealSequence) trước.");
                return;
            }
            Setup(reveal.gameObject);
        }

        public static MagicRuneRitual Setup(GameObject chest)
        {
            MagicRuneRitual r = chest.GetComponent<MagicRuneRitual>();
            if (r == null) r = Undo.AddComponent<MagicRuneRitual>(chest);
            FillClips(r);

            // A1: hồ sơ lõi gần như câm, ghi lại MỖI lần chạy menu (không chỉ khi ô trống).
            ChestFxProfile p = chest.GetComponent<ChestRevealSequence>()?.Profile;
            if (p != null && p != ChestFxProfile.Default)
            {
                Undo.RecordObject(p, "Magic Rune audio rule");
                ChestAudioRule.ApplyToProfile(p);
                EditorUtility.SetDirty(p);
            }

            foreach (MagicHook old in chest.GetComponentsInChildren<MagicHook>())
            {
                Undo.RecordObject(old, "Disable MagicHook");
                old.enabled = false;
                Debug.Log("[MagicRune] Đã tắt MagicHook cũ (vòng hạt) để không chồng lên trận pháp.");
            }
            EditorUtility.SetDirty(r);
            Debug.Log("[MagicRune] Xong. Play, bấm mở Chest_Magic: âm tụ phép phải vang NGAY lúc bấm.");
            return r;
        }

        /// <summary>Gán âm ElevenLabs trong Audio/Chests/magic/ (dùng chung cho menu Setup và thư viện menu cầm rương).</summary>
        public static void FillClips(MagicRuneRitual r)
        {
            int missing = 0;
            r.buildClip = Load("sfx_magic_rune_bed_loop.mp3", ref missing);
            r.stoneClip = Load("sfx_magic_rune_stone.mp3", ref missing);
            r.wispClip = Load("sfx_magic_rune_wisp.mp3", ref missing);
            r.slamClip = Load("sfx_magic_rune_slam.mp3", ref missing);
            r.stingClips = new AudioClip[StingFiles.Length];
            for (int i = 0; i < StingFiles.Length; i++) r.stingClips[i] = Load(StingFiles[i], ref missing);
            if (missing > 0) Debug.LogWarning($"[MagicRune] Thiếu {missing} âm trong {AudioFolder} (chạy Tools/Audio/make_elevenlabs_chest_sfx.py --chest magic). Hiệu ứng vẫn chạy, chỉ im.");
        }

        [MenuItem("Tools/Cho Do La/Magic Rune/Create Player Rune Stone")]
        public static void CreateStone()
        {
            GameObject sel = Selection.activeGameObject;
            Vector3 at = sel != null ? sel.transform.position + new Vector3(0.55f, 0.05f, 0f) : Vector3.up * 0.05f;
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
            s.name = "PlayerRuneStone";
            s.transform.position = at;
            s.transform.localScale = new Vector3(0.1f, 0.055f, 0.08f);
            Rigidbody rb = s.AddComponent<Rigidbody>();
            rb.mass = 0.6f;
            s.AddComponent<MagicRuneStone>();
            Undo.RegisterCreatedObjectUndo(s, "Create Player Rune Stone");
            Selection.activeGameObject = s;
        }

        private static AudioClip Load(string file, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + file);
            if (c == null) missing++;
            return c;
        }
    }
}
#endif

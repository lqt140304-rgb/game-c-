#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Menu dựng kịch bản "Trận pháp Rune" cho Chest_Magic (file riêng, không đụng MagicFxSetup dùng chung).
    /// - Tools/Cho Do La/Magic Rune/Setup On Selected Chest: chọn gốc Chest_Magic (có ChestRevealSequence) rồi bấm.
    ///   Thêm MagicRuneRitual, gán âm trong Audio/Chests/magic_rune/, tắt MagicHook cũ (nếu có) để khỏi chồng hiệu ứng.
    /// - Tools/Cho Do La/Magic Rune/Create Player Rune Stone: tạo một viên đá rune có Rigidbody cạnh rương đang chọn,
    ///   để thử tương tác phụ (đặt sẵn vào điểm trận, hoặc ném vào vòng khi đang tụ phép).
    /// </summary>
    public static class MagicRuneFxSetup
    {
        public const string AudioFolder = "Assets/_Project/Audio/Chests/magic_rune/";
        public static readonly string[] EndFiles =
        {
            "sfx_magicrune_end_0_rac.wav", "sfx_magicrune_end_1_thuong.wav", "sfx_magicrune_end_2_hiem.wav",
            "sfx_magicrune_end_3_rathiem.wav", "sfx_magicrune_end_4_huyenthoai.wav",
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

        /// <summary>Gán âm trong Audio/Chests/magic_rune/ (dùng chung cho menu Setup và thư viện menu cầm rương).</summary>
        public static void FillClips(MagicRuneRitual r)
        {
            int missing = 0;
            r.buildClip = Load("sfx_magicrune_build.wav", ref missing);
            r.stoneClip = Load("sfx_magicrune_stone.wav", ref missing);
            r.wispClip = Load("sfx_magicrune_wisp.wav", ref missing);
            r.endClips = new AudioClip[EndFiles.Length];
            for (int i = 0; i < EndFiles.Length; i++) r.endClips[i] = Load(EndFiles[i], ref missing);
            if (missing > 0) Debug.LogWarning($"[MagicRune] Thiếu {missing} âm trong {AudioFolder} (chạy Tools/Audio/make_magic_rune_sfx.py). Hiệu ứng vẫn chạy, chỉ im.");
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

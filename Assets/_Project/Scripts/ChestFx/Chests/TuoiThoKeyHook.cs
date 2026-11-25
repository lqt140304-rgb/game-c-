using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Tuổi thơ: VIÊN BI VE (cầu xanh trong + 3 lá xoáy nhiều màu ở lõi).
    /// Hành động Drop; 3 lá xoáy quay quanh trục Z khi bay và khi rơi. Kế hoạch 2026-11-26, mục 6.9.
    /// </summary>
    public class TuoiThoKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_TuoiTho", displayName = "TuoiTho",
            audioFolder = "Assets/_Project/Audio/Chests/tuoi_tho/", filePrefix = "sfx_tuoitho_", bedFile = "sfx_tuoitho_roll_loop.mp3", keyFile = "sfx_tuoitho_bonk.mp3",
            mainColor = new Color(0.3f, 0.7f, 0.95f), openAction = KeyAction.Drop,
            squashMul = 1f, racSquashMul = 1f, jumpMul = 1.1f, cycleDelta = 1, shakeScale = 1f,   // tưng tưng: chu kỳ +1, Jump +10%
        };

        // Nhóm 3 lá xoáy của từng viên bi, tra theo gốc vật (không dùng static).
        private readonly Dictionary<Transform, Transform> _swirl = new Dictionary<Transform, Transform>();

        public override KeyPreset GetPreset() { return Preset; }

        protected override GameObject BuildKey(KeyContext c)
        {
            GameObject root = new GameObject("Key");
            c.Part(root.transform, PrimitiveType.Sphere, "ViBi", Vector3.zero, Vector3.one * 0.92f, Preset.mainColor);

            GameObject swirl = new GameObject("Xoay");
            swirl.transform.SetParent(root.transform, false);
            Color[] cols = { new Color(0.95f, 0.25f, 0.3f), new Color(1f, 0.85f, 0.2f), new Color(0.35f, 0.85f, 0.4f) };
            for (int i = 0; i < 3; i++)
            {
                // Lá dẹt xếp tròn quanh trục Z, hơi nhô ra khỏi mặt cầu để thấy được (vật liệu không trong suốt).
                GameObject arm = new GameObject("La" + i);
                arm.transform.SetParent(swirl.transform, false);
                arm.transform.localRotation = Quaternion.Euler(0f, 0f, i * 120f);
                c.Part(arm.transform, PrimitiveType.Cube, "Canh", new Vector3(0.24f, 0f, 0f), new Vector3(0.5f, 0.06f, 0.5f), Quaternion.Euler(0f, 0f, 20f), cols[i]);
            }
            _swirl[root.transform] = swirl.transform;
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            Transform sw;
            if (root == null || !_swirl.TryGetValue(root, out sw) || sw == null) return;
            // Bay: xoáy 1,5 vòng; rơi: xoáy thêm nửa vòng.
            float angle = InActionPhase ? 540f + 180f * t01 : 540f * t01;
            sw.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            _swirl.Clear();
            base.OnReset(reveal);
        }
    }
}

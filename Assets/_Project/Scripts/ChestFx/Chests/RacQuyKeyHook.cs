using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Rác Quý: con mèo mướp cam (thân, đầu, 2 tai, đuôi, 2 mắt, 4 chân), đầu hướng +Z,
    /// đuôi vẫy khi bay và khi vồ. Kế hoạch 2026-11-26, mục 6.2. Không sinh thêm vật nào.
    /// </summary>
    public class RacQuyKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_RacQuy", displayName = "RacQuy",
            audioFolder = "Assets/_Project/Audio/Chests/rac_quy/", filePrefix = "sfx_racquy_", bedFile = "sfx_racquy_bed_loop.mp3", keyFile = "sfx_racquy_clank.mp3",
            mainColor = new Color(1f, 0.6f, 0.2f), openAction = KeyAction.Pounce,
            squashMul = 1f, racSquashMul = 1.2f, jumpMul = 0.8f, cycleDelta = 0, shakeScale = 1f,
        };

        public override KeyPreset GetPreset() => Preset;

        // Đuôi của từng con mèo đang sống (theo gốc vật); không static.
        private readonly Dictionary<Transform, Transform> _tails = new Dictionary<Transform, Transform>();

        protected override GameObject BuildKey(KeyContext c)
        {
            Color cam = Preset.mainColor;
            Color camDam = new Color(0.8f, 0.4f, 0.1f);
            Color mat = new Color(0.85f, 0.85f, 0.2f);
            Color hong = new Color(1f, 0.6f, 0.65f);

            GameObject root = new GameObject("Key_Meo");
            Transform t = root.transform;
            c.Part(t, PrimitiveType.Cube, "Than", new Vector3(0f, -0.05f, -0.12f), new Vector3(0.34f, 0.3f, 0.5f), cam);
            c.Part(t, PrimitiveType.Sphere, "Dau", new Vector3(0f, 0.08f, 0.3f), new Vector3(0.36f, 0.32f, 0.3f), cam);
            c.Part(t, PrimitiveType.Cube, "TaiTrai", new Vector3(-0.12f, 0.27f, 0.28f), new Vector3(0.1f, 0.14f, 0.06f), Quaternion.Euler(0f, 0f, 15f), camDam);
            c.Part(t, PrimitiveType.Cube, "TaiPhai", new Vector3(0.12f, 0.27f, 0.28f), new Vector3(0.1f, 0.14f, 0.06f), Quaternion.Euler(0f, 0f, -15f), camDam);
            c.Part(t, PrimitiveType.Sphere, "MatTrai", new Vector3(-0.08f, 0.12f, 0.44f), new Vector3(0.07f, 0.07f, 0.04f), mat);
            c.Part(t, PrimitiveType.Sphere, "MatPhai", new Vector3(0.08f, 0.12f, 0.44f), new Vector3(0.07f, 0.07f, 0.04f), mat);
            c.Part(t, PrimitiveType.Sphere, "Mui", new Vector3(0f, 0.04f, 0.46f), new Vector3(0.05f, 0.04f, 0.04f), hong);
            c.Part(t, PrimitiveType.Cube, "ChanTruocTrai", new Vector3(-0.1f, -0.25f, 0.06f), new Vector3(0.08f, 0.16f, 0.08f), camDam);
            c.Part(t, PrimitiveType.Cube, "ChanTruocPhai", new Vector3(0.1f, -0.25f, 0.06f), new Vector3(0.08f, 0.16f, 0.08f), camDam);
            c.Part(t, PrimitiveType.Cube, "ChanSauTrai", new Vector3(-0.1f, -0.25f, -0.3f), new Vector3(0.08f, 0.16f, 0.08f), camDam);
            c.Part(t, PrimitiveType.Cube, "ChanSauPhai", new Vector3(0.1f, -0.25f, -0.3f), new Vector3(0.08f, 0.16f, 0.08f), camDam);

            // Đuôi: gốc xoay nằm ở mông, đuôi dựng cong lên sau lưng.
            GameObject pivot = new GameObject("DuoiGoc");
            pivot.transform.SetParent(t, false);
            pivot.transform.localPosition = new Vector3(0f, 0f, -0.38f);
            c.Part(pivot.transform, PrimitiveType.Cube, "Duoi1", new Vector3(0f, 0.05f, -0.06f), new Vector3(0.07f, 0.07f, 0.2f), Quaternion.Euler(-20f, 0f, 0f), cam);
            c.Part(pivot.transform, PrimitiveType.Cube, "Duoi2", new Vector3(0f, 0.2f, -0.14f), new Vector3(0.07f, 0.22f, 0.07f), camDam);
            _tails[t] = pivot.transform;
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            Transform tail;
            if (root == null || !_tails.TryGetValue(root, out tail) || tail == null) return;
            float freq = InActionPhase ? 14f : 8f;
            float amp = InActionPhase ? 40f : 25f;
            tail.localRotation = Quaternion.Euler(0f, Mathf.Sin(t01 * freq) * amp, 0f);
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            base.OnReset(reveal);
            _tails.Clear();
        }
    }
}

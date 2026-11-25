using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương ThienCo: bánh răng đồng (đĩa trụ + 8 răng + lỗ trục), quay quanh trục Z khi bay và khi thả.
    /// Kế hoạch 2026-11-26, mục 6.17. Chưa chạy trong Unity.
    /// </summary>
    public class ThienCoKeyHook : ChestKeyHookBase
    {
        const int TeethCount = 8;

        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_ThienCo", displayName = "ThienCo",
            audioFolder = "Assets/_Project/Audio/Chests/thien_co/", filePrefix = "sfx_thienco_", bedFile = "sfx_thienco_ambient_clockwork.mp3", keyFile = "sfx_thienco_gear_clank.mp3",
            mainColor = new Color(0.8f, 0.5f, 0.25f), openAction = KeyAction.Drop,
            squashMul = 1f, racSquashMul = 1f, jumpMul = 1.2f, cycleDelta = 1, shakeScale = 1f,   // lò xo: Jump +20%, chu kỳ +1
        };

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color bronze = Preset.mainColor;
            Color dark = new Color(0.18f, 0.12f, 0.08f);
            GameObject root = new GameObject("Key");
            Transform spin = new GameObject("Quay").transform;
            spin.SetParent(root.transform, false);
            Quaternion alongZ = Quaternion.Euler(90f, 0f, 0f);   // trụ nằm dọc Z: mặt đĩa hướng +Z
            c.Part(spin, PrimitiveType.Cylinder, "Dia", Vector3.zero, new Vector3(0.7f, 0.1f, 0.7f), alongZ, bronze);
            for (int i = 0; i < TeethCount; i++)
            {
                float a = i * Mathf.PI * 2f / TeethCount;
                Vector3 p = new Vector3(Mathf.Cos(a) * 0.4f, Mathf.Sin(a) * 0.4f, 0f);
                c.Part(spin, PrimitiveType.Cube, "Rang" + i, p, new Vector3(0.16f, 0.16f, 0.2f), Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg), bronze);
            }
            c.Part(spin, PrimitiveType.Cylinder, "LoTruc", new Vector3(0f, 0f, 0.02f), new Vector3(0.22f, 0.1f, 0.22f), alongZ, dark);
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            Transform spin = root != null ? root.Find("Quay") : null;
            if (spin == null) return;
            float turns = InActionPhase ? 1f : 2f;   // bay: quay 2 vòng; thả: thêm 1 vòng
            spin.localRotation = Quaternion.Euler(0f, 0f, t01 * 360f * turns);
        }
    }
}

using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Thể thao: quả bóng đá (cầu trắng + 5 mảng đen dẹt), hành động Strike (đá vào khe).
    /// Kế hoạch 2026-11-26, mục 6.13. Không có bộ phận cử động riêng.
    /// </summary>
    public class TheThaoKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_TheThao",
            displayName = "TheThao",
            audioFolder = "Assets/_Project/Audio/Chests/the_thao/",
            filePrefix = "sfx_thethao_",
            bedFile = "sfx_thethao_bed_loop.mp3",
            mainColor = new Color(0.96f, 0.96f, 0.96f),
            openAction = KeyAction.Strike,
            squashMul = 1f,
            racSquashMul = 1f,
            jumpMul = 1.2f,   // bật như bóng: Jump +20%
            cycleDelta = 0,
            shakeScale = 1f,
        };

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color white = Preset.mainColor;
            Color black = new Color(0.08f, 0.08f, 0.09f);
            GameObject root = new GameObject("TheThaoKey");
            Transform t = root.transform;

            // Quả cầu trắng, đường kính 1 (vừa hộp 1x1x1), gốc ở giữa.
            c.Part(t, PrimitiveType.Sphere, "Bong", Vector3.zero, Vector3.one, white);

            // 5 mảng đen dẹt nằm sát mặt cầu, mỗi mảng hướng theo pháp tuyến của nó (mảng đầu hướng +Z để thấy từ phía trước).
            Vector3[] normals =
            {
                new Vector3(0f, 0.25f, 1f),
                new Vector3(0f, 1f, 0f),
                new Vector3(-1f, 0.2f, -0.35f),
                new Vector3(1f, 0.2f, -0.35f),
                new Vector3(0f, -1f, 0f),
            };
            for (int i = 0; i < normals.Length; i++)
            {
                Vector3 n = normals[i].normalized;
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, n);
                c.Part(t, PrimitiveType.Cylinder, "Mang" + i, n * 0.495f, new Vector3(0.3f, 0.02f, 0.3f), rot, black);
            }
            return root;
        }
    }
}

using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương KhongGian: phi thuyền mini (thân bạc hình giọt nước, 2 cánh vây, kính cabin xanh).
    /// Mũi tàu hướng +Z. Hành động mở: Strike (lao vào khe). Tính cách: nhẹ bẫng, Jump +20%.
    /// </summary>
    public class KhongGianKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_KhongGian", displayName = "KhongGian",
            audioFolder = "Assets/_Project/Audio/Chests/khong_gian/", filePrefix = "sfx_khonggian_",
            bedFile = "sfx_khonggian_engine_loop.mp3",
            mainColor = new Color(0.78f, 0.82f, 0.88f), openAction = KeyAction.Strike,
            squashMul = 1f, racSquashMul = 1f, jumpMul = 1.2f, cycleDelta = 0, shakeScale = 1f,
        };

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color silver = Preset.mainColor;
            Color dark = new Color(0.45f, 0.5f, 0.58f);
            Color blue = new Color(0.25f, 0.6f, 1f);
            Color orange = new Color(1f, 0.55f, 0.15f);

            GameObject root = new GameObject("KhongGianShip");
            // Thân giọt nước: cầu kéo dài theo Z, mũi nhọn về +Z nhờ chóp nhỏ phía trước.
            c.Part(root.transform, PrimitiveType.Sphere, "Than", Vector3.zero, new Vector3(0.4f, 0.34f, 0.9f), silver);
            c.Part(root.transform, PrimitiveType.Sphere, "Mui", new Vector3(0f, 0f, 0.42f), new Vector3(0.18f, 0.16f, 0.3f), silver);
            // Kính cabin xanh trên lưng thân.
            c.Part(root.transform, PrimitiveType.Sphere, "Cabin", new Vector3(0f, 0.15f, 0.12f), new Vector3(0.2f, 0.16f, 0.3f), blue);
            // Hai cánh vây hai bên, hơi chúi ra sau.
            c.Part(root.transform, PrimitiveType.Cube, "CanhTrai", new Vector3(-0.3f, -0.03f, -0.18f), new Vector3(0.4f, 0.04f, 0.3f), Quaternion.Euler(0f, 20f, 0f), dark);
            c.Part(root.transform, PrimitiveType.Cube, "CanhPhai", new Vector3(0.3f, -0.03f, -0.18f), new Vector3(0.4f, 0.04f, 0.3f), Quaternion.Euler(0f, -20f, 0f), dark);
            // Vây đuôi đứng + miệng động cơ cam.
            c.Part(root.transform, PrimitiveType.Cube, "VayDuoi", new Vector3(0f, 0.17f, -0.36f), new Vector3(0.04f, 0.2f, 0.2f), dark);
            c.Part(root.transform, PrimitiveType.Sphere, "DongCo", new Vector3(0f, 0f, -0.46f), new Vector3(0.16f, 0.16f, 0.12f), orange);
            return root;
        }
    }
}

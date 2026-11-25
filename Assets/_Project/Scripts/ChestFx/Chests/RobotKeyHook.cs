using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Robot: THẺ CHIP (thẻ chữ nhật dẹt xám kim loại + chip vàng + sọc mạch cyan + chân tiếp xúc vàng).
    /// Cạnh ngắn có chân tiếp xúc hướng +Z (mũi cắm vào khe). Hành động Insert. Kế hoạch 2026-11-26, mục 6.10.
    /// </summary>
    public class RobotKeyHook : ChestKeyHookBase
    {
        static readonly Color Gray = new Color(0.62f, 0.65f, 0.70f);
        static readonly Color Gold = new Color(1f, 0.80f, 0.20f);
        static readonly Color Cyan = new Color(0.20f, 0.90f, 0.95f);

        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_Robot", displayName = "Robot",
            audioFolder = "Assets/_Project/Audio/Chests/robot/", filePrefix = "sfx_robot_", bedFile = "sfx_robot_hum_loop.mp3", keyFile = "sfx_robot_slot.mp3",
            mainColor = Gray, openAction = KeyAction.Insert,
            squashMul = 0.8f, racSquashMul = 0.8f, jumpMul = 1f, cycleDelta = -1, shakeScale = 1f,   // cứng: Squash -20%, chu kỳ -1 (>= 1)
        };

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            GameObject root = new GameObject("RobotCard");
            Transform t = root.transform;
            c.Part(t, PrimitiveType.Cube, "The", Vector3.zero, new Vector3(0.62f, 0.07f, 0.9f), Gray);
            c.Part(t, PrimitiveType.Cube, "Chip", new Vector3(0f, 0.07f, -0.05f), new Vector3(0.26f, 0.08f, 0.26f), Gold);
            c.Part(t, PrimitiveType.Cube, "SocMach1", new Vector3(-0.2f, 0.045f, 0.2f), new Vector3(0.04f, 0.03f, 0.3f), Cyan);
            c.Part(t, PrimitiveType.Cube, "SocMach2", new Vector3(0f, 0.045f, 0.28f), new Vector3(0.04f, 0.03f, 0.2f), Cyan);
            c.Part(t, PrimitiveType.Cube, "SocMach3", new Vector3(0.2f, 0.045f, 0.2f), new Vector3(0.04f, 0.03f, 0.3f), Cyan);
            c.Part(t, PrimitiveType.Cube, "SocMach4", new Vector3(0f, 0.045f, -0.32f), new Vector3(0.44f, 0.03f, 0.04f), Cyan);
            for (int i = 0; i < 3; i++)
                c.Part(t, PrimitiveType.Cube, "Chan" + i, new Vector3(-0.17f + 0.17f * i, 0f, 0.47f), new Vector3(0.1f, 0.05f, 0.1f), Gold);
            return root;
        }
    }
}

using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Thợ Mỏ: chiếc cuốc chim (cán gỗ + lưỡi sắt cong + hai đầu nhọn). Kế hoạch 2026-11-26, mục 6.6.
    /// Đầu cuốc ở +Z (hướng về rương), gốc ở giữa, vừa hộp 1x1x1. Không có bộ phận cử động riêng.
    /// </summary>
    public class MinerKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_Miner", displayName = "Miner",
            audioFolder = "Assets/_Project/Audio/Chests/miner/", filePrefix = "sfx_miner_", bedFile = "sfx_miner_rumble_loop.mp3", keyFile = "sfx_miner_thump.mp3",
            mainColor = new Color(0.62f, 0.64f, 0.68f), openAction = KeyAction.Strike,
            squashMul = 1f, racSquashMul = 1f, jumpMul = 0.8f, cycleDelta = 0, shakeScale = 1f,
        };

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            var root = new GameObject("MinerPickaxe");
            Color wood = new Color(0.45f, 0.29f, 0.14f);
            Color iron = Preset.mainColor;
            Color dark = new Color(0.30f, 0.31f, 0.34f);
            Color tip = new Color(0.85f, 0.87f, 0.9f);

            // Cán gỗ nằm dọc trục Z (hình trụ xoay 90 độ quanh X), dài ~0,95.
            c.Part(root.transform, PrimitiveType.Cylinder, "Handle", new Vector3(0f, 0f, -0.05f),
                new Vector3(0.10f, 0.475f, 0.10f), Quaternion.Euler(90f, 0f, 0f), wood);
            // Nút tròn nhỏ ở cuối cán
            c.Part(root.transform, PrimitiveType.Sphere, "HandleKnob", new Vector3(0f, 0f, -0.5f),
                new Vector3(0.14f, 0.14f, 0.14f), wood);
            // Khối giữa lưỡi sắt ôm đầu cán
            c.Part(root.transform, PrimitiveType.Cube, "HeadCore", new Vector3(0f, 0f, 0.38f),
                new Vector3(0.20f, 0.16f, 0.16f), dark);
            // Hai cánh lưỡi cong về phía trước (+Z)
            c.Part(root.transform, PrimitiveType.Cube, "ArmL", new Vector3(-0.22f, 0f, 0.41f),
                new Vector3(0.28f, 0.12f, 0.12f), Quaternion.Euler(0f, 22f, 0f), iron);
            c.Part(root.transform, PrimitiveType.Cube, "ArmR", new Vector3(0.22f, 0f, 0.41f),
                new Vector3(0.28f, 0.12f, 0.12f), Quaternion.Euler(0f, -22f, 0f), iron);
            // Hai mũi nhọn sáng ở hai đầu
            c.Part(root.transform, PrimitiveType.Cube, "TipL", new Vector3(-0.40f, 0f, 0.47f),
                new Vector3(0.14f, 0.09f, 0.09f), Quaternion.Euler(0f, 40f, 45f), tip);
            c.Part(root.transform, PrimitiveType.Cube, "TipR", new Vector3(0.40f, 0f, 0.47f),
                new Vector3(0.14f, 0.09f, 0.09f), Quaternion.Euler(0f, -40f, 45f), tip);
            return root;
        }
    }
}

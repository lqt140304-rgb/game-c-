using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương AmNhac. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class AmNhacKeyHook : ChestKeyHookBase
    {
        // Nhún theo nhịp: chu kỳ Squash/Jump +1.
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_AmNhac", displayName = "AmNhac", audioFolder = "Assets/_Project/Audio/Chests/am_nhac/",
            filePrefix = "sfx_amnhac_", bedFile = "sfx_amnhac_drumroll.mp3", keyFile = "sfx_amnhac_coin_slot.mp3",
            mainColor = new Color(0.95f, 0.4f, 0.65f), openAction = KeyAction.Insert,
            squashMul = 1f, racSquashMul = 1f, jumpMul = 1f, cycleDelta = 1, shakeScale = 1f,
        };

        public override KeyPreset GetPreset() => Preset;

        /// <summary>Đĩa than dựng đứng (mặt đĩa vuông góc trục X, mép đĩa là phần "đầu" quay về +Z): đĩa đen + 3 rãnh + nhãn hồng giữa.</summary>
        protected override GameObject BuildKey(KeyContext c)
        {
            GameObject root = new GameObject("Key");
            Quaternion faceX = Quaternion.Euler(0f, 0f, 90f);   // trụ Unity dọc Y; xoay để trục đĩa dọc X
            Color black = new Color(0.07f, 0.07f, 0.08f);
            Color groove = new Color(0.22f, 0.22f, 0.25f);
            c.Part(root.transform, PrimitiveType.Cylinder, "Dia", Vector3.zero, new Vector3(0.5f, 0.030f, 0.5f), faceX, black);
            c.Part(root.transform, PrimitiveType.Cylinder, "Ranh1", Vector3.zero, new Vector3(0.43f, 0.031f, 0.43f), faceX, groove);
            c.Part(root.transform, PrimitiveType.Cylinder, "Ranh2", Vector3.zero, new Vector3(0.34f, 0.032f, 0.34f), faceX, black);
            c.Part(root.transform, PrimitiveType.Cylinder, "Ranh3", Vector3.zero, new Vector3(0.26f, 0.033f, 0.26f), faceX, groove);
            c.Part(root.transform, PrimitiveType.Cylinder, "Nhan", Vector3.zero, new Vector3(0.17f, 0.036f, 0.17f), faceX, Preset.mainColor);
            return root;
        }
    }
}

using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Golden (kế hoạch 2026-11-26, mục 6.3): đồng xu vàng to, mặt có chữ "$", xoay tròn khi bay rồi rơi vào khe.
    /// </summary>
    public class GoldenKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_Golden", displayName = "Golden",
            audioFolder = "Assets/_Project/Audio/Chests/golden/", filePrefix = "sfx_golden_", bedFile = "sfx_golden_spin_loop.mp3", keyFile = "sfx_golden_token.mp3",
            mainColor = new Color(1f, 0.82f, 0.15f), openAction = KeyAction.Drop,
            squashMul = 1f, racSquashMul = 1f, jumpMul = 1.2f, cycleDelta = 0, shakeScale = 1f,   // nảy mạnh: Jump +20%
        };

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color gold = Preset.mainColor;
            Color rimGold = new Color(0.78f, 0.55f, 0.08f);
            Color mark = new Color(0.55f, 0.36f, 0.04f);
            GameObject root = new GameObject("Key");
            GameObject pivot = new GameObject("Xoay");
            pivot.transform.SetParent(root.transform, false);
            Transform p = pivot.transform;
            Quaternion alongZ = Quaternion.Euler(90f, 0f, 0f);   // trụ nằm dọc Z, mặt đồng xu quay về +Z
            c.Part(p, PrimitiveType.Cylinder, "Vien", new Vector3(0f, 0f, 0f), new Vector3(0.96f, 0.05f, 0.96f), alongZ, rimGold);
            c.Part(p, PrimitiveType.Cylinder, "Mat", new Vector3(0f, 0f, 0.05f), new Vector3(0.78f, 0.03f, 0.78f), alongZ, gold);
            c.Part(p, PrimitiveType.Cube, "DoCot", new Vector3(0f, 0f, 0.085f), new Vector3(0.07f, 0.56f, 0.03f), mark);
            c.Part(p, PrimitiveType.Cube, "DoTren", new Vector3(-0.03f, 0.15f, 0.085f), new Vector3(0.26f, 0.07f, 0.03f), mark);
            c.Part(p, PrimitiveType.Cube, "DoDuoi", new Vector3(0.03f, -0.15f, 0.085f), new Vector3(0.26f, 0.07f, 0.03f), mark);
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            Transform pivot = root != null ? root.Find("Xoay") : null;
            if (pivot == null) return;
            // Bay: xoay 2 vòng quanh trục dọc; hành động: mặt xu quay thẳng về rương.
            float angle = InActionPhase ? 0f : t01 * 720f;
            pivot.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}

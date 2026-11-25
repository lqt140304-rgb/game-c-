using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Mimic: chiếc dép lê (mồi cho rương đói), thả rơi vào khe.
    /// Mũi dép hướng +Z, gốc ở giữa, vừa hộp 1x1x1. Không có bộ phận cử động. Kế hoạch 2026-11-26, mục 6.5.
    /// </summary>
    public class MimicKeyHook : ChestKeyHookBase
    {
        static readonly Color Sole = new Color(0.85f, 0.75f, 0.55f);   // đế be
        static readonly Color Strap = new Color(0.45f, 0.28f, 0.14f);  // quai nâu
        static readonly Color Heel = new Color(0.62f, 0.45f, 0.28f);   // gót nâu nhạt

        public static readonly KeyPreset Preset = MakePreset();

        static KeyPreset MakePreset()
        {
            KeyPreset p = KeyPreset.Neutral(
                "Chest_Mimic", "Mimic", "Assets/_Project/Audio/Chests/mimic/", "sfx_mimic_", "sfx_mimic_stomach_loop.mp3",
                Sole, KeyAction.Drop);
            p.squashMul = 1.15f;     // háu đói: Squash +15%
            p.racSquashMul = 1.15f;
            return p;
        }

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            GameObject root = new GameObject("MimicSlipper");
            Transform t = root.transform;
            // đế dép: tấm dẹt dài, mũi về +Z
            c.Part(t, PrimitiveType.Cube, "Sole", new Vector3(0f, -0.05f, 0f), new Vector3(0.5f, 0.1f, 0.96f), Sole);
            // quai: hai thanh chéo vắt lên mu bàn chân
            c.Part(t, PrimitiveType.Cube, "StrapL", new Vector3(-0.15f, 0.05f, 0.2f), new Vector3(0.1f, 0.1f, 0.5f),
                Quaternion.Euler(0f, 0f, -25f), Strap);
            c.Part(t, PrimitiveType.Cube, "StrapR", new Vector3(0.15f, 0.05f, 0.2f), new Vector3(0.1f, 0.1f, 0.5f),
                Quaternion.Euler(0f, 0f, 25f), Strap);
            // hai khối gót
            c.Part(t, PrimitiveType.Cube, "HeelL", new Vector3(-0.12f, 0.03f, -0.36f), new Vector3(0.2f, 0.14f, 0.2f), Heel);
            c.Part(t, PrimitiveType.Cube, "HeelR", new Vector3(0.12f, 0.03f, -0.36f), new Vector3(0.2f, 0.14f, 0.2f), Heel);
            return root;
        }
    }
}

using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương Mimic. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class MimicKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_Mimic", "Mimic", "Assets/_Project/Audio/Chests/mimic/", "sfx_mimic_", "sfx_mimic_stomach_loop.mp3",
            new Color(0.85f, 0.75f, 0.55f), KeyAction.Drop);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

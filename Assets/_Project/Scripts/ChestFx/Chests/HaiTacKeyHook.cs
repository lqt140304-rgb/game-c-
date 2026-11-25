using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương HaiTac. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class HaiTacKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_HaiTac", "HaiTac", "Assets/_Project/Audio/Chests/hai_tac/", "sfx_haitac_", "sfx_haitac_bed_loop.mp3",
            new Color(0.85f, 0.15f, 0.1f), KeyAction.Peck);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

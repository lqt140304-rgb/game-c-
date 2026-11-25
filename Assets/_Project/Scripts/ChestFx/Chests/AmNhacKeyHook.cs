using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương AmNhac. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class AmNhacKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_AmNhac", "AmNhac", "Assets/_Project/Audio/Chests/am_nhac/", "sfx_amnhac_", "sfx_amnhac_bed_loop.mp3",
            new Color(0.95f, 0.4f, 0.65f), KeyAction.Insert);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

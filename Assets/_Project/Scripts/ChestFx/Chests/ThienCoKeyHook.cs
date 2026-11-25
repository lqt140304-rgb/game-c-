using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương ThienCo. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class ThienCoKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_ThienCo", "ThienCo", "Assets/_Project/Audio/Chests/thien_co/", "sfx_thienco_", "sfx_thienco_bed_loop.mp3",
            new Color(0.8f, 0.5f, 0.25f), KeyAction.Drop);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

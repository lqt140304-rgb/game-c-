using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương TheThao. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class TheThaoKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_TheThao", "TheThao", "Assets/_Project/Audio/Chests/the_thao/", "sfx_thethao_", "sfx_thethao_bed_loop.mp3",
            new Color(0.95f, 0.95f, 0.95f), KeyAction.Strike);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

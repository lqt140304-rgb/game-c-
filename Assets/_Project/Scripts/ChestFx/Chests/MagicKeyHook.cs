using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương Magic. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class MagicKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_Magic", "Magic", "Assets/_Project/Audio/Chests/magic/", "sfx_magic_", "sfx_magic_rune_bed_loop.mp3",
            new Color(0.6f, 0.3f, 0.9f), KeyAction.Insert);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

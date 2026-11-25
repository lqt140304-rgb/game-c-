using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương VietNam. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class VietNamKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_VietNam", "VietNam", "Assets/_Project/Audio/Chests/viet_nam/", "sfx_vietnam_", "sfx_vietnam_dan_bau_loop.mp3",
            new Color(0.95f, 0.55f, 0.7f), KeyAction.Drop);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

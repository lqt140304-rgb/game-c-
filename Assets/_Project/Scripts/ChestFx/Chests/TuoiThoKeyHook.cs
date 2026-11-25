using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương TuoiTho. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class TuoiThoKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_TuoiTho", "TuoiTho", "Assets/_Project/Audio/Chests/tuoi_tho/", "sfx_tuoitho_", "sfx_tuoitho_roll_loop.mp3",
            new Color(0.3f, 0.7f, 0.95f), KeyAction.Drop);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

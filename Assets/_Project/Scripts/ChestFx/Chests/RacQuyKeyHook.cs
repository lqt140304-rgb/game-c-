using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương RacQuy. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class RacQuyKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_RacQuy", "RacQuy", "Assets/_Project/Audio/Chests/rac_quy/", "sfx_racquy_", "sfx_racquy_bed_loop.mp3",
            new Color(1f, 0.6f, 0.2f), KeyAction.Pounce);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

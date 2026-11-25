using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương TrungHoa. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class TrungHoaKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_TrungHoa", "TrungHoa", "Assets/_Project/Audio/Chests/trung_hoa/", "sfx_trunghoa_", "sfx_trunghoa_lion_drum_loop.mp3",
            new Color(0.85f, 0.1f, 0.1f), KeyAction.Pounce);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương CoPhong. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class CoPhongKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_CoPhong", "CoPhong", "Assets/_Project/Audio/Chests/co_phong/", "sfx_cophong_", "sfx_cophong_sword_hum_loop.mp3",
            new Color(0.95f, 0.85f, 0.4f), KeyAction.Strike);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

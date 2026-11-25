using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương KhongGian. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class KhongGianKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_KhongGian", "KhongGian", "Assets/_Project/Audio/Chests/khong_gian/", "sfx_khonggian_", "sfx_khonggian_engine_loop.mp3",
            new Color(0.8f, 0.85f, 0.9f), KeyAction.Strike);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

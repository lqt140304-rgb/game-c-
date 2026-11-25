using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương NhatBan. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class NhatBanKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_NhatBan", "NhatBan", "Assets/_Project/Audio/Chests/nhat_ban/", "sfx_nhatban_", "sfx_nhatban_shakuhachi_loop.mp3",
            new Color(0.8f, 0.82f, 0.85f), KeyAction.Strike);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

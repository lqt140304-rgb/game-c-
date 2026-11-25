using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vật mở khoá của rương Robot. Hình mặc định chung; phiên rương sẽ thay BuildKey (kế hoạch 2026-11-26, mục 6).</summary>
    public class RobotKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = KeyPreset.Neutral(
            "Chest_Robot", "Robot", "Assets/_Project/Audio/Chests/robot/", "sfx_robot_", "sfx_robot_hum_loop.mp3",
            new Color(0.65f, 0.68f, 0.72f), KeyAction.Insert);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c) => DefaultKey(c, Preset.mainColor);
    }
}

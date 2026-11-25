using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương VietNam: HOA ĐĂNG (đế gỗ + 6 cánh sen hồng + ngọn lửa vàng ở giữa), hành động Drop
    /// (thả rơi vào khe). Kế hoạch 2026-11-26, mục 6.8. Tính cách bóp méo: mềm (Squash −10%).
    /// </summary>
    public class VietNamKeyHook : ChestKeyHookBase
    {
        static readonly Color Go = new Color(0.55f, 0.36f, 0.18f);
        static readonly Color Sen = new Color(0.97f, 0.5f, 0.7f);
        static readonly Color Lua = new Color(1f, 0.82f, 0.2f);

        public static readonly KeyPreset Preset = MakePreset();

        static KeyPreset MakePreset()
        {
            KeyPreset p = KeyPreset.Neutral(
                "Chest_VietNam", "VietNam", "Assets/_Project/Audio/Chests/viet_nam/", "sfx_vietnam_", "sfx_vietnam_dan_bau_loop.mp3",
                Sen, KeyAction.Drop);
            p.keyFile = "sfx_vietnam_thud.mp3";
            p.squashMul = 0.9f;      // mềm: Squash −10%
            p.racSquashMul = 0.9f;
            return p;
        }

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            GameObject root = new GameObject("HoaDang");
            Transform t = root.transform;
            // Đế gỗ tròn dẹt
            c.Part(t, PrimitiveType.Cylinder, "De", new Vector3(0f, -0.2f, 0f), new Vector3(0.8f, 0.05f, 0.8f), Go);
            // 6 cánh sen hồng xoè quanh, nghiêng nhẹ ra ngoài
            for (int i = 0; i < 6; i++)
            {
                Quaternion around = Quaternion.Euler(0f, i * 60f, 0f);
                Vector3 pos = around * new Vector3(0f, -0.04f, 0.27f);
                Quaternion rot = around * Quaternion.Euler(25f, 0f, 0f);
                c.Part(t, PrimitiveType.Cube, "Canh" + i, pos, new Vector3(0.2f, 0.34f, 0.07f), rot, Sen);
            }
            // Ngọn lửa vàng ở giữa
            c.Part(t, PrimitiveType.Sphere, "Lua", Vector3.zero, new Vector3(0.2f, 0.3f, 0.2f), Lua);
            return root;
        }
    }
}

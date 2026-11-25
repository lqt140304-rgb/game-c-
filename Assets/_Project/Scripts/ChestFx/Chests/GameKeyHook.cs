using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Game: chìa pixel 8-bit ghép từ cube vuông (thân 1 hàng, đầu 3x3, 2 răng), vàng + xanh.
    /// Hành động Insert. Tính cách: giật như khung hình (chu kỳ +1). Kế hoạch 2026-11-26, mục 6.12.
    /// </summary>
    public class GameKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = MakePreset();

        static KeyPreset MakePreset()
        {
            KeyPreset p = KeyPreset.Neutral(
                "Chest_Game", "Game", "Assets/_Project/Audio/Chests/game/", "sfx_game_", "sfx_game_chip_loop.mp3",
                new Color(1f, 0.85f, 0.2f), KeyAction.Insert);
            p.keyFile = "sfx_game_button_click.mp3";
            p.cycleDelta = 1;
            return p;
        }

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color gold = new Color(1f, 0.85f, 0.2f);
            Color blue = new Color(0.2f, 0.55f, 1f);
            const float px = 0.1f;
            Vector3 size = new Vector3(px, px, px);
            GameObject root = new GameObject("Key");
            Transform t = root.transform;

            // Đầu 3x3 (vòng 8 ô, ô giữa trống) ở phía -Z, mặt phẳng XY.
            for (int ix = -1; ix <= 1; ix++)
                for (int iy = -1; iy <= 1; iy++)
                {
                    if (ix == 0 && iy == 0) continue;
                    c.Part(t, PrimitiveType.Cube, "Dau_" + ix + "_" + iy, new Vector3(ix * px, iy * px, -0.4f), size, (ix + iy) % 2 == 0 ? gold : blue);
                }
            // Thân 1 hàng dọc +Z.
            for (int k = 0; k < 7; k++)
                c.Part(t, PrimitiveType.Cube, "Than" + k, new Vector3(0f, 0f, -0.3f + k * px), size, gold);
            // Mũi chìa (+Z) và 2 răng xanh.
            c.Part(t, PrimitiveType.Cube, "Mui", new Vector3(0f, 0f, 0.4f), size, blue);
            c.Part(t, PrimitiveType.Cube, "Rang1", new Vector3(px, 0f, 0.3f), size, blue);
            c.Part(t, PrimitiveType.Cube, "Rang2", new Vector3(px, 0f, 0.1f), size, blue);
            return root;
        }
    }
}

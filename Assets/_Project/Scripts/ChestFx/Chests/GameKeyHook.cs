using System.Collections;
using System.Collections.Generic;
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
            // Đợt 2: bay nhảy từng bước giật như khung hình; mở = "ăn" vào khe (pixel vỡ ra rồi ráp lại).
            p.flyFile = "sfx_game_fly.mp3";
            p.actionFile = "sfx_game_action.mp3";
            p.lightColor = new Color(0.55f, 0.85f, 0.3f);   // vàng-xanh pixel
            p.lightMul = 0.6f;
            p.moveStyle = ChestMoveStyle.Bounce;
            return p;
        }

        public override KeyPreset GetPreset() => Preset;

        // Vị trí gốc của từng ô pixel, theo gốc vật (không static: mỗi hook giữ của riêng nó).
        private readonly Dictionary<Transform, Vector3[]> _home = new Dictionary<Transform, Vector3[]>();

        private Vector3[] Home(Transform root)
        {
            Vector3[] h;
            if (_home.TryGetValue(root, out h) && h.Length == root.childCount) return h;
            _home.Remove(root);
            List<Transform> dead = new List<Transform>();
            foreach (Transform k in _home.Keys) if (k == null) dead.Add(k);
            foreach (Transform k in dead) _home.Remove(k);
            h = new Vector3[root.childCount];
            for (int i = 0; i < h.Length; i++) h[i] = root.GetChild(i).localPosition;
            _home[root] = h;
            return h;
        }

        /// <summary>Bay: 4 bước nhảy như nhân vật 8-bit, vị trí cập nhật giật theo 12 khung/giây.</summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            return FlyRoutine(c, root, seconds);
        }

        private IEnumerator FlyRoutine(KeyContext c, Transform root, float seconds)
        {
            const int hops = 4;
            const float fps = 12f;
            Vector3 p0 = c.startPos;
            float t0 = Time.unscaledTime;
            while (true)
            {
                c.Refresh();
                float el = Time.unscaledTime - t0;
                float u = Mathf.Clamp01(Mathf.Floor(el * fps) / (seconds * fps));   // giật theo khung
                if (el >= seconds) u = 1f;
                float hp = u * hops;
                int i = Mathf.Min(hops - 1, (int)hp);
                float f = hp - i;
                Vector3 a = Vector3.Lerp(p0, c.RestPos, (float)i / hops);
                Vector3 b = Vector3.Lerp(p0, c.RestPos, (float)(i + 1) / hops);
                Vector3 pos = Vector3.Lerp(a, b, f) + c.up * (0.22f * c.S * 4f * f * (1f - f));
                // Lượng tử hoá vị trí theo ô pixel cho cảm giác "lưới".
                float cell = 0.03f * c.S;
                if (u < 1f && cell > 1e-5f)
                    pos = new Vector3(Mathf.Round(pos.x / cell) * cell, Mathf.Round(pos.y / cell) * cell, Mathf.Round(pos.z / cell) * cell);
                root.position = pos;
                // Nghiêng theo bậc 15° khi nhảy lên/xuống.
                float tilt = u < 1f ? Mathf.Round((1f - 2f * f) * 2f) * 15f : 0f;
                root.rotation = c.Facing * Quaternion.Euler(-tilt, 0f, 0f);
                root.localScale = Vector3.one * c.keySize;
                if (u >= 1f) yield break;
                yield return null;
            }
        }

        /// <summary>Mở: "ăn" vào khe — các ô pixel bung ra (0..0,35), rồi ráp lại giật bậc (0,35..0,8), rồi thu nhỏ tại khe.</summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            return ActionRoutine(c, root, seconds);
        }

        private IEnumerator ActionRoutine(KeyContext c, Transform root, float seconds)
        {
            Vector3[] home = Home(root);
            int n = home.Length;
            Vector3[] dir = new Vector3[n];
            System.Random rng = c.Rng(1212);
            for (int i = 0; i < n; i++)
            {
                Vector3 d = new Vector3((float)rng.NextDouble() * 2f - 1f, (float)rng.NextDouble() * 2f - 1f, (float)rng.NextDouble() * 0.6f - 0.3f);
                dir[i] = home[i] * 0.6f + d * 0.35f;
            }
            float t0 = Time.unscaledTime;
            while (true)
            {
                c.Refresh();
                float t = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float burst;
                if (t < 0.35f) { float k = t / 0.35f; burst = 1f - (1f - k) * (1f - k); }
                else burst = 1f - Mathf.Clamp01((t - 0.35f) / 0.45f);
                if (t >= 0.35f) burst = Mathf.Ceil(burst * 4f) / 4f;   // ráp lại theo 4 bậc như khung hình
                for (int i = 0; i < n; i++)
                {
                    Transform ch = root.GetChild(i);
                    if (ch == null) continue;
                    ch.localPosition = home[i] + dir[i] * burst;
                }
                root.rotation = c.Facing;
                ShrinkAtSlot(c, root, t);
                if (t >= 1f) break;
                yield return null;
            }
            for (int i = 0; i < n && i < root.childCount; i++) root.GetChild(i).localPosition = home[i];
            _home.Remove(root);
        }

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

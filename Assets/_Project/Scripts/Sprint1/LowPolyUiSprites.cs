using UnityEngine;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Generates crisp, cached procedural 9-slice chamfered rectangles, hexagons, and circles
    /// so Unity UI matches the Stylized Low-Poly "Gỗ · Đồng · Giấy dó" art direction without external textures.
    /// </summary>
    public static class LowPolyUiSprites
    {
        private static Sprite _chamferedRect;
        private static Sprite _chamferedSmall;
        private static Sprite _hexagon;
        private static Sprite _circle;
        private static Sprite _waxRing;

        public static Sprite ChamferedRect
        {
            get
            {
                if (_chamferedRect == null) _chamferedRect = BuildChamferedSprite(48, 10);
                return _chamferedRect;
            }
        }

        public static Sprite ChamferedSmall
        {
            get
            {
                if (_chamferedSmall == null) _chamferedSmall = BuildChamferedSprite(32, 6);
                return _chamferedSmall;
            }
        }

        public static Sprite Hexagon
        {
            get
            {
                if (_hexagon == null) _hexagon = BuildHexagonSprite(64);
                return _hexagon;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (_circle == null) _circle = BuildCircleSprite(64, ringOnly: false);
                return _circle;
            }
        }

        public static Sprite WaxStampRing
        {
            get
            {
                if (_waxRing == null) _waxRing = BuildCircleSprite(84, ringOnly: true);
                return _waxRing;
            }
        }

        private static Sprite BuildChamferedSprite(int size, int chamfer)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"UI_Chamfer_{size}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Min(x, size - 1 - x);
                    int dy = Mathf.Min(y, size - 1 - y);
                    float d = (dx + dy) - chamfer;
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(d + 0.75f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            int border = chamfer + 2;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static Sprite BuildHexagonSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "UI_Hexagon",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            float rx = half * 0.94f;
            float ry = half * 0.88f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = Mathf.Abs(x - half) / rx;
                    float py = Mathf.Abs(y - half) / ry;
                    // Flat-top hexagon signed distance
                    float dist = Mathf.Max(py, px * 0.5f + py * 0.5f * 1.15f) - 1f;
                    // Better flat-top hex: |y| <= 1, 0.5*|x| + (sqrt(3)/2?): standard hex with vertices at (±1,0), (±0.5, ±1)
                    float hexVal = Mathf.Max(py, (px + py * 0.5f) * 0.86f);
                    float edge = (1f - hexVal) * half;
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(edge + 0.5f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite BuildCircleSprite(int size, bool ringOnly)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = ringOnly ? "UI_WaxRing" : "UI_Circle",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            float outerR = half - 2f;
            float innerR = outerR - 4.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float r = Vector2.Distance(new Vector2(x, y), new Vector2(half, half));
                    float a = ringOnly
                        ? Mathf.Clamp01(outerR - r + 0.5f) * Mathf.Clamp01(r - innerR + 0.5f)
                        : Mathf.Clamp01(outerR - r + 0.5f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}

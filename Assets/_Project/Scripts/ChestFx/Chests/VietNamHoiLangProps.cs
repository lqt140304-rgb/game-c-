using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Đạo cụ của kịch bản "Đêm hội làng" (Chest_VietNam), dựng bằng khối cơ bản, không cần asset:
    /// đèn hoa đăng, bánh chưng (quà chim Lạc thả), chim Lạc, rùa vàng, mặt nước. Chỉ trang trí.
    /// </summary>
    public static class VietNamHoiLangProps
    {
        public static readonly Color Water = new Color(0.03f, 0.07f, 0.16f);
        public static readonly Color Petal = new Color(1f, 0.45f, 0.62f);
        public static readonly Color PetalTip = new Color(1f, 0.82f, 0.88f);
        public static readonly Color Flame = new Color(1f, 0.78f, 0.35f);
        public static readonly Color Leaf = new Color(0.18f, 0.48f, 0.2f);
        public static readonly Color Bronze = new Color(0.85f, 0.62f, 0.3f);
        public static readonly Color TurtleGold = new Color(0.9f, 0.72f, 0.25f);

        /// <summary>Khối cơ bản không collider, vật liệu Unlit riêng (gom vào <paramref name="owner"/> để dọn).</summary>
        public static GameObject Part(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Color color, VietNamHoiLangHook owner, string name)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.hideFlags = HideFlags.DontSave;
            Collider c = go.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            Material m = ChestFxFactory.CreateUnlit(color, false, name);
            owner.Own(m);
            Renderer r = go.GetComponent<Renderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        /// <summary>Gốc rỗng có Rigidbody + BoxCollider nhỏ, nổi trên nước nhờ <see cref="VietNamHoiLangFloater"/>.</summary>
        private static VietNamHoiLangFloater FloatingRoot(string name, Vector3 pos, Vector3 colliderSize, float mass, VietNamHoiLangHook owner)
        {
            GameObject root = new GameObject(name);
            root.hideFlags = HideFlags.DontSave;
            root.layer = 2; // Ignore Raycast: không chặn tia chọn đồ của game.
            root.transform.position = pos;
            BoxCollider box = root.AddComponent<BoxCollider>();
            box.size = colliderSize;
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.linearDamping = 0.4f;
            rb.angularDamping = 1.5f;
            VietNamHoiLangFloater f = root.AddComponent<VietNamHoiLangFloater>();
            f.Init(owner, rb);
            return f;
        }

        /// <summary>Đèn hoa đăng: đế lá, 8 cánh sen nghiêng, ngọn nến sáng (có Light nếu <paramref name="withLight"/>).</summary>
        public static VietNamHoiLangFloater Lantern(Vector3 pos, float size, bool withLight, VietNamHoiLangHook owner, System.Random rng)
        {
            VietNamHoiLangFloater f = FloatingRoot("HoiLangLantern", pos, new Vector3(0.22f, 0.1f, 0.22f) * size / 0.22f, 0.06f, owner);
            Transform t = f.transform;
            Part(PrimitiveType.Cylinder, t, new Vector3(0f, -0.02f, 0f) * size / 0.22f, new Vector3(0.24f, 0.012f, 0.24f) * size / 0.22f, Leaf, owner, "Leaf");
            int petals = 8;
            float hue = (float)rng.NextDouble() * 0.15f;
            for (int i = 0; i < petals; i++)
            {
                float a = i * 360f / petals;
                GameObject p = Part(PrimitiveType.Sphere, t, Vector3.zero, new Vector3(0.05f, 0.11f, 0.025f) * size / 0.22f,
                    Color.Lerp(Petal, PetalTip, hue + (i % 2) * 0.35f), owner, "Petal");
                p.transform.localRotation = Quaternion.Euler(0f, a, 0f) * Quaternion.Euler(35f, 0f, 0f);
                p.transform.localPosition = p.transform.localRotation * new Vector3(0f, 0.04f, 0.0f) * size / 0.22f
                                            + Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, 0.05f) * size / 0.22f;
            }
            GameObject flame = Part(PrimitiveType.Sphere, t, new Vector3(0f, 0.07f, 0f) * size / 0.22f, new Vector3(0.035f, 0.06f, 0.035f) * size / 0.22f, Flame, owner, "Flame");
            Light light = null;
            if (withLight)
            {
                light = flame.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = Flame;
                light.range = 0.9f;
                light.intensity = 1.2f;
                light.shadows = LightShadows.None;
            }
            f.SetFlame(flame.GetComponent<Renderer>(), light);
            return f;
        }

        /// <summary>Bánh chưng nhỏ (lá xanh, lạt đỏ): quà chim Lạc thả, rơi bằng trọng lực rồi nổi.</summary>
        public static VietNamHoiLangFloater Gift(Vector3 pos, VietNamHoiLangHook owner)
        {
            VietNamHoiLangFloater f = FloatingRoot("HoiLangGift", pos, Vector3.one * 0.12f, 0.15f, owner);
            Part(PrimitiveType.Cube, f.transform, Vector3.zero, Vector3.one * 0.12f, Leaf, owner, "Banh");
            Part(PrimitiveType.Cube, f.transform, Vector3.zero, new Vector3(0.125f, 0.125f, 0.015f), new Color(0.85f, 0.15f, 0.1f), owner, "LatA");
            Part(PrimitiveType.Cube, f.transform, Vector3.zero, new Vector3(0.015f, 0.125f, 0.125f), new Color(0.85f, 0.15f, 0.1f), owner, "LatB");
            return f;
        }

        /// <summary>Chim Lạc: thân dài, mỏ rất dài, mào, hai cánh (trả về 2 cánh để vỗ).</summary>
        public static GameObject Bird(VietNamHoiLangHook owner, float scale, out Transform wingL, out Transform wingR)
        {
            GameObject root = new GameObject("HoiLangChimLac");
            root.hideFlags = HideFlags.DontSave;
            Transform t = root.transform;
            Part(PrimitiveType.Sphere, t, Vector3.zero, new Vector3(0.12f, 0.1f, 0.3f) * scale, Bronze, owner, "Body");
            Part(PrimitiveType.Cube, t, new Vector3(0f, 0.01f, 0.32f) * scale, new Vector3(0.018f, 0.018f, 0.36f) * scale, Bronze * 0.8f, owner, "Beak");
            Part(PrimitiveType.Cube, t, new Vector3(0f, 0.09f, 0.08f) * scale, new Vector3(0.015f, 0.12f, 0.05f) * scale, Bronze, owner, "Crest");
            Part(PrimitiveType.Cube, t, new Vector3(0f, 0f, -0.24f) * scale, new Vector3(0.12f, 0.01f, 0.16f) * scale, Bronze * 0.9f, owner, "Tail");
            wingL = new GameObject("WingL").transform;
            wingR = new GameObject("WingR").transform;
            foreach (Transform w in new[] { wingL, wingR })
            {
                w.gameObject.hideFlags = HideFlags.DontSave;
                w.SetParent(t, false);
                float side = w == wingL ? -1f : 1f;
                Part(PrimitiveType.Cube, w, new Vector3(side * 0.24f, 0f, 0f) * scale, new Vector3(0.46f, 0.01f, 0.14f) * scale, Bronze, owner, "Wing");
            }
            return root;
        }

        /// <summary>Rùa vàng: mai dẹt, đầu, bốn chân.</summary>
        public static GameObject Turtle(VietNamHoiLangHook owner, float scale)
        {
            GameObject root = new GameObject("HoiLangRuaVang");
            root.hideFlags = HideFlags.DontSave;
            Transform t = root.transform;
            Part(PrimitiveType.Sphere, t, Vector3.zero, new Vector3(0.36f, 0.12f, 0.44f) * scale, TurtleGold, owner, "Shell");
            Part(PrimitiveType.Sphere, t, new Vector3(0f, 0.02f, 0.27f) * scale, new Vector3(0.1f, 0.08f, 0.12f) * scale, TurtleGold * 0.85f, owner, "Head");
            for (int i = 0; i < 4; i++)
            {
                float sx = i % 2 == 0 ? -1f : 1f, sz = i < 2 ? 1f : -1f;
                Part(PrimitiveType.Sphere, t, new Vector3(sx * 0.17f, -0.03f, sz * 0.15f) * scale, new Vector3(0.12f, 0.03f, 0.07f) * scale, TurtleGold * 0.8f, owner, "Leg");
            }
            return root;
        }

        /// <summary>Mặt nước: đĩa phẳng tối (không collider; nổi tính bằng lực trong Floater).</summary>
        public static GameObject WaterDisc(Vector3 center, float radius, VietNamHoiLangHook owner)
        {
            GameObject w = Part(PrimitiveType.Cylinder, null, center, new Vector3(radius * 2f, 0.002f, radius * 2f), Water, owner, "HoiLangWater");
            w.transform.position = center;
            return w;
        }
    }
}

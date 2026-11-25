using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Đạo cụ của kịch bản "Đêm hội làng" (Chest_VietNam), dựng bằng khối cơ bản, không cần asset: chỉ còn đèn hoa đăng
    /// (đã bỏ bánh chưng, chim Lạc, rùa vàng, mặt nước theo kế hoạch thu gọn 2026-11-25). Chỉ trang trí.
    /// </summary>
    public static class VietNamHoiLangProps
    {
        public static readonly Color Petal = new Color(1f, 0.45f, 0.62f);
        public static readonly Color PetalTip = new Color(1f, 0.82f, 0.88f);
        public static readonly Color Flame = new Color(1f, 0.78f, 0.35f);
        public static readonly Color Leaf = new Color(0.18f, 0.48f, 0.2f);

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

        /// <summary>Gốc rỗng có Rigidbody kinematic (không collider), do <see cref="VietNamHoiLangFloater"/> điều khiển bằng mã.</summary>
        private static VietNamHoiLangFloater FloatingRoot(string name, Vector3 pos, float mass, VietNamHoiLangHook owner)
        {
            GameObject root = new GameObject(name);
            root.hideFlags = HideFlags.DontSave;
            root.layer = 2; // Ignore Raycast: không chặn tia chọn đồ của game.
            root.transform.position = pos;
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.isKinematic = true;
            rb.useGravity = false;
            VietNamHoiLangFloater f = root.AddComponent<VietNamHoiLangFloater>();
            f.Init(owner, rb);
            return f;
        }

        /// <summary>Đèn hoa đăng: đế lá, 8 cánh sen nghiêng, ngọn nến sáng (có Light nếu <paramref name="withLight"/>).</summary>
        public static VietNamHoiLangFloater Lantern(Vector3 pos, float size, bool withLight, VietNamHoiLangHook owner, System.Random rng)
        {
            VietNamHoiLangFloater f = FloatingRoot("HoiLangLantern", pos, 0.06f, owner);
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
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương KhongGian: phi thuyền mini (thân bạc hình giọt nước, 2 cánh vây, kính cabin xanh).
    /// Mũi tàu hướng +Z. Đợt 2: bay lượn nghiêng vòng chữ S như tàu vào bến (lửa động cơ phập phồng), rồi cập bến:
    /// động cơ hãm bùng lên, tàu lùi khẽ, khoá kẹp (cánh gập vào) và chui vào khe. Tính cách: nhẹ bẫng, Jump +20%.
    /// </summary>
    public class KhongGianKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_KhongGian", displayName = "KhongGian",
            audioFolder = "Assets/_Project/Audio/Chests/khong_gian/", filePrefix = "sfx_khonggian_",
            bedFile = "sfx_khonggian_engine_loop.mp3", keyFile = "sfx_khonggian_fuel_dock.mp3",
            mainColor = new Color(0.78f, 0.82f, 0.88f), openAction = KeyAction.Strike,
            squashMul = 1f, racSquashMul = 1f, jumpMul = 1.2f, cycleDelta = 0, shakeScale = 1f,
            // Đợt 2: tiếng bay/cập bến riêng, sáng xanh lam lạnh, rương lơ lửng như không trọng lực.
            flyFile = "sfx_khonggian_fly.mp3", actionFile = "sfx_khonggian_action.mp3",
            lightColor = new Color(0.35f, 0.62f, 1f), lightMul = 0.55f, moveStyle = ChestMoveStyle.Float,
        };

        // Khối con theo gốc vật (không static): 0 = động cơ, 1 = cánh trái, 2 = cánh phải.
        private readonly Dictionary<Transform, Transform[]> _parts = new Dictionary<Transform, Transform[]>();
        private static readonly Vector3 EngineScale = new Vector3(0.16f, 0.16f, 0.12f);
        private static readonly Quaternion WingL = Quaternion.Euler(0f, 20f, 0f);
        private static readonly Quaternion WingR = Quaternion.Euler(0f, -20f, 0f);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color silver = Preset.mainColor;
            Color dark = new Color(0.45f, 0.5f, 0.58f);
            Color blue = new Color(0.25f, 0.6f, 1f);
            Color orange = new Color(1f, 0.55f, 0.15f);

            GameObject root = new GameObject("KhongGianShip");
            // Thân giọt nước: cầu kéo dài theo Z, mũi nhọn về +Z nhờ chóp nhỏ phía trước.
            c.Part(root.transform, PrimitiveType.Sphere, "Than", Vector3.zero, new Vector3(0.4f, 0.34f, 0.9f), silver);
            c.Part(root.transform, PrimitiveType.Sphere, "Mui", new Vector3(0f, 0f, 0.42f), new Vector3(0.18f, 0.16f, 0.3f), silver);
            // Kính cabin xanh trên lưng thân.
            c.Part(root.transform, PrimitiveType.Sphere, "Cabin", new Vector3(0f, 0.15f, 0.12f), new Vector3(0.2f, 0.16f, 0.3f), blue);
            // Hai cánh vây hai bên, hơi chúi ra sau.
            GameObject wl = c.Part(root.transform, PrimitiveType.Cube, "CanhTrai", new Vector3(-0.3f, -0.03f, -0.18f), new Vector3(0.4f, 0.04f, 0.3f), WingL, dark);
            GameObject wr = c.Part(root.transform, PrimitiveType.Cube, "CanhPhai", new Vector3(0.3f, -0.03f, -0.18f), new Vector3(0.4f, 0.04f, 0.3f), WingR, dark);
            // Vây đuôi đứng + miệng động cơ cam.
            c.Part(root.transform, PrimitiveType.Cube, "VayDuoi", new Vector3(0f, 0.17f, -0.36f), new Vector3(0.04f, 0.2f, 0.2f), dark);
            GameObject eng = c.Part(root.transform, PrimitiveType.Sphere, "DongCo", new Vector3(0f, 0f, -0.46f), EngineScale, orange);
            _parts[root.transform] = new[] { eng.transform, wl.transform, wr.transform };
            return root;
        }

        private Transform[] PartsOf(Transform root)
        {
            Transform[] p;
            if (root == null || !_parts.TryGetValue(root, out p) || p[0] == null || p[1] == null || p[2] == null) return null;
            return p;
        }

        /// <summary>Lửa động cơ: k = 1 bình thường, &gt; 1 phụt dài ra sau.</summary>
        private static void SetFlame(Transform[] p, float k)
        {
            if (p == null) return;
            p[0].localScale = new Vector3(EngineScale.x * (0.8f + 0.2f * k), EngineScale.y * (0.8f + 0.2f * k), EngineScale.z * k);
            p[0].localPosition = new Vector3(0f, 0f, -0.46f - 0.06f * (k - 1f));
        }

        private static float Sm(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        /// <summary>Bay: lướt vòng chữ S sang ngang, nghiêng thân theo khúc cua, chậm dần vào điểm chờ trước bến.</summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            Transform[] p = PartsOf(root);
            System.Random rng = c.Rng(1101);
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;      // lượn vòng bên trái hay bên phải
            float phase = (float)rng.NextDouble() * 6.283f;     // nhịp phập phồng lửa
            Vector3 p0 = c.startPos;
            float t0 = Time.unscaledTime;
            Vector3 prev = root.position;
            float roll = 0f;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                // Chậm dần về cuối (như tàu giảm tốc vào bến): ease-out bậc 3.
                float e = 1f - (1f - u) * (1f - u) * (1f - u);
                Vector3 right = Vector3.Cross(c.up, c.front);
                Vector3 hover = c.RestPos + c.front * (0.3f * c.S);
                Vector3 line = Vector3.Lerp(p0, hover, e);
                // Lệch ngang hình chữ S (lên đỉnh giữa đường, về 0 ở hai đầu) + nhô lên nhẹ.
                float lat = side * 0.55f * c.S * Mathf.Sin(Mathf.PI * e) * (1f - 0.6f * e);
                float lift = 0.18f * c.S * Mathf.Sin(Mathf.PI * e);
                Vector3 pos = line + right * lat + c.up * lift;
                root.position = pos;

                // Hướng: mũi theo hướng chuyển động, cuối chuyến quay hẳn về rương; nghiêng thân theo vận tốc ngang.
                Vector3 vel = pos - prev;
                prev = pos;
                float latVel = Vector3.Dot(vel, right) / Mathf.Max(1e-4f, c.S * Time.unscaledDeltaTime + 1e-4f);
                roll = Mathf.Lerp(roll, Mathf.Clamp(-latVel * 18f, -40f, 40f), 0.25f);
                Quaternion face = c.Facing;
                Vector3 vf = vel - c.up * Vector3.Dot(vel, c.up);
                if (vf.sqrMagnitude > 1e-10f && u < 0.95f)
                    face = Quaternion.Slerp(Quaternion.LookRotation(vf.normalized, c.up), c.Facing, Sm(0.5f, 0.95f, u));
                root.rotation = face * Quaternion.Euler(0f, 0f, roll * (1f - Sm(0.8f, 1f, u)));

                // Lửa: phập phồng mạnh lúc tăng tốc, nhỏ dần khi lướt vào.
                float thrust = Mathf.Lerp(1.6f, 0.9f, u) + 0.15f * Mathf.Sin(phase + u * 40f);
                SetFlame(p, thrust);
                if (u >= 1f) break;
                yield return null;
            }
        }

        /// <summary>Cập bến 0,35 s: động cơ hãm bùng, tàu tiến vào khe, giật lùi khẽ, cánh gập kẹp lại, rồi chui vào khe.</summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            Transform[] p = PartsOf(root);
            float t0 = Time.unscaledTime;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                // Khoảng cách tới bến (theo S): 0,3 → 0 (0..0,35), giật lùi 0,05 (0,35..0,5), áp vào lại (0,5..0,7).
                float off;
                if (u < 0.35f) off = Mathf.Lerp(0.3f, 0f, Sm(0f, 0.35f, u));
                else if (u < 0.5f) off = 0.05f * Mathf.Sin(Mathf.PI * (u - 0.35f) / 0.15f);
                else off = 0f;
                // Rung nhẹ lúc chạm khớp (0,35..0,55).
                float jit = (u > 0.35f && u < 0.55f) ? Mathf.Sin(u * 180f) * 3f * (1f - (u - 0.35f) / 0.2f) : 0f;
                root.rotation = c.Facing * Quaternion.Euler(jit, 0f, jit * 0.5f);
                if (u < 0.85f)
                {
                    root.position = c.RestPos + c.front * (off * c.S);
                    root.localScale = Vector3.one * c.keySize;
                }
                else ShrinkAtSlot(c, root, u);

                if (p != null)
                {
                    // Động cơ hãm: bùng 2,2 lần ở 0..0,3, tắt dần về 0,3 lúc cập bến xong.
                    float flame = u < 0.3f ? Mathf.Lerp(1f, 2.2f, Sm(0f, 0.12f, u)) : Mathf.Lerp(2.2f, 0.3f, Sm(0.3f, 0.6f, u));
                    SetFlame(p, flame);
                    // Cánh gập vào như càng kẹp (0,5..0,8).
                    float fold = 55f * Sm(0.5f, 0.8f, u);
                    p[1].localRotation = WingL * Quaternion.Euler(0f, 0f, -fold);
                    p[2].localRotation = WingR * Quaternion.Euler(0f, 0f, fold);
                }
                if (u >= 1f) break;
                yield return null;
            }
            ShrinkAtSlot(c, root, 1f);
        }
    }
}

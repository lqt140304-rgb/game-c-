using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Phần "trời" của Chest_TrungHoa: pháo thăng thiên (thân Rigidbody được đẩy bằng lực phụt liên tục, nghiêng lệch
    /// ngẫu nhiên theo hạt giống), nở thành chùm hoa / chữ 福 (Phúc), xác pháo xịt rơi lại bằng vật lý, và đèn lồng đỏ
    /// rơi xuống có lực cản không khí, đập vào rương / mặt đất bật nảy thật.
    /// Mọi hạt dùng chung 2 ParticleSystem (tia lửa + khói), phát bằng EmitParams; không dùng orbital velocity.
    /// </summary>
    public class TrungHoaSky : MonoBehaviour
    {
        public delegate void BurstHandler(Vector3 position, bool big);
        public event BurstHandler Burst;           // hook nghe để phát tiếng nổ
        public event System.Action<Vector3> DudLanded;

        private class Rocket
        {
            public Rigidbody body;
            public float fuel, age, padY, height, accel;
            public Color color;
            public bool phuc, dud, done;
            public Vector3 drift;
        }

        private readonly List<Rocket> _rockets = new List<Rocket>();
        private readonly List<Material> _mats = new List<Material>();
        private readonly List<ChestFxProfile> _styles = new List<ChestFxProfile>();
        private ParticleSystem _sparks, _smoke;
        private float _size;
        private System.Random _rng;
        private Transform _cam;

        public static TrungHoaSky Build(float size, System.Random rng, Transform parent)
        {
            GameObject root = new GameObject("TrungHoa_Troi");
            root.transform.SetParent(parent, false);
            TrungHoaSky sky = root.AddComponent<TrungHoaSky>();
            sky._size = size;
            sky._rng = rng;
            sky._cam = Camera.main != null ? Camera.main.transform : null;
            sky._sparks = sky.Particles("TrungHoaSparks", ParticleLook.Streak, 0.02f, 1.2f, 0.6f);
            sky._smoke = sky.Particles("TrungHoaSmoke", ParticleLook.SoftDot, 0.08f, 1.8f, -0.05f);
            return sky;
        }

        private ParticleSystem Particles(string n, ParticleLook look, float pSize, float life, float gravity)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = ParticleDirection.Radial;
            s.particleSpeed = 1f;
            s.particleSize = pSize;
            s.particleLifetime = life;
            s.burstGravity = gravity;
            _styles.Add(s);
            ParticleSystem ps = ChestFxFactory.CreateParticles(n, transform, s, _size, true);
            ParticleSystem.MainModule main = ps.main;
            main.maxParticles = 3000;
            ps.Play();
            return ps;
        }

        private float R => (float)_rng.NextDouble();
        private Vector3 RandomDir() => new Vector3(R - 0.5f, R - 0.5f, R - 0.5f).normalized;

        // ------------------------------------------------------------------ hạt

        public void SparkBurst(Vector3 pos, Color a, Color b, int count, float speed, float life)
        {
            ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
            for (int i = 0; i < count; i++)
            {
                ep.position = pos;
                ep.velocity = RandomDir() * speed * (0.6f + 0.4f * R);
                ep.startColor = Color.Lerp(a, b, R);
                ep.startSize = _size * 0.02f * (0.6f + R);
                ep.startLifetime = life * (0.6f + 0.5f * R);
                _sparks.Emit(ep, 1);
            }
        }

        public void SmokePuff(Vector3 pos, Color c, int count, float speed)
        {
            ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
            for (int i = 0; i < count; i++)
            {
                ep.position = pos + RandomDir() * _size * 0.03f;
                Vector3 v = RandomDir() * speed; v.y = Mathf.Abs(v.y) + speed * 0.5f;
                ep.velocity = v;
                ep.startColor = c;
                ep.startSize = _size * (0.06f + 0.06f * R);
                ep.startLifetime = 1.2f + R;
                _smoke.Emit(ep, 1);
            }
        }

        // Chữ 福 (Phúc) dạng nét, toạ độ trong ô [0,1]x[0,1]; mỗi mảng là một nét gấp khúc.
        private static readonly Vector2[][] PhucStrokes =
        {
            new[] { new Vector2(0.12f, 0.96f), new Vector2(0.19f, 0.88f) },
            new[] { new Vector2(0.02f, 0.78f), new Vector2(0.29f, 0.78f), new Vector2(0.05f, 0.48f) },
            new[] { new Vector2(0.17f, 0.62f), new Vector2(0.17f, 0.02f) },
            new[] { new Vector2(0.18f, 0.55f), new Vector2(0.31f, 0.44f) },
            new[] { new Vector2(0.42f, 0.93f), new Vector2(0.98f, 0.93f) },
            new[] { new Vector2(0.5f, 0.83f), new Vector2(0.9f, 0.83f), new Vector2(0.9f, 0.63f), new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.83f) },
            new[] { new Vector2(0.45f, 0.51f), new Vector2(0.95f, 0.51f), new Vector2(0.95f, 0.03f), new Vector2(0.45f, 0.03f), new Vector2(0.45f, 0.51f) },
            new[] { new Vector2(0.7f, 0.51f), new Vector2(0.7f, 0.03f) },
            new[] { new Vector2(0.45f, 0.27f), new Vector2(0.95f, 0.27f) },
        };

        /// <summary>Hạt xếp thành chữ 福 quay mặt về camera; nở từ tâm ra rồi treo lơ lửng, rơi nhẹ.</summary>
        public void PhucGlyph(Vector3 center, float height, Color a, Color b)
        {
            Vector3 right = _cam != null ? _cam.right : Vector3.right;
            Vector3 up = _cam != null ? Vector3.ProjectOnPlane(_cam.up, right).normalized : Vector3.up;
            ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
            float spacing = height * 0.025f;
            foreach (Vector2[] stroke in PhucStrokes)
            {
                for (int k = 1; k < stroke.Length; k++)
                {
                    Vector2 p0 = stroke[k - 1], p1 = stroke[k];
                    int n = Mathf.Max(2, Mathf.CeilToInt((p1 - p0).magnitude * height / spacing));
                    for (int i = 0; i <= n; i++)
                    {
                        Vector2 q = Vector2.Lerp(p0, p1, i / (float)n) - new Vector2(0.5f, 0.5f);
                        Vector3 target = center + (right * q.x + up * q.y) * height;
                        // phóng từ tâm tới đích trong ~0,35 s; lực cản của hệ hạt nhỏ nên đặt vận tốc vừa đủ
                        ep.position = Vector3.Lerp(center, target, 0.75f);
                        ep.velocity = (target - center) * 0.7f;
                        ep.startColor = Color.Lerp(a, b, R * 0.5f + (q.y + 0.5f) * 0.5f);
                        ep.startSize = _size * 0.03f;
                        ep.startLifetime = 2.6f + R * 0.6f;
                        _sparks.Emit(ep, 1);
                    }
                }
            }
        }

        // ------------------------------------------------------------------ pháo thăng thiên

        /// <summary>Phóng một quả pháo thăng thiên từ <paramref name="pad"/>. dud = xịt: bay thấp, lảo đảo, rơi lại.</summary>
        public void Launch(Vector3 pad, Color color, float burstHeight, bool phuc, bool dud, float delay = 0f)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            g.name = dud ? "PhaoXit" : "PhaoThangThien";
            g.transform.SetParent(transform, true);
            g.transform.position = pad;
            g.transform.localScale = new Vector3(0.035f, 0.06f, 0.035f) * _size;
            Material m = ChestFxFactory.CreateUnlit(dud ? new Color(0.5f, 0.15f, 0.12f) : new Color(0.9f, 0.1f, 0.1f), false, "TrungHoaRocket");
            _mats.Add(m);
            g.GetComponent<Renderer>().sharedMaterial = m;
            Rigidbody rb = g.AddComponent<Rigidbody>();
            rb.mass = 0.02f;
            rb.linearDamping = 0.15f;
            rb.angularDamping = 2f;
            rb.isKinematic = delay > 0f;
            _rockets.Add(new Rocket
            {
                body = rb, color = color, phuc = phuc, dud = dud,
                // gia tốc ròng để lên tới độ cao nổ trong ~0,8 s (xịt: chỉ nhích ~0,4 m rồi tắt)
                fuel = dud ? 0.35f : 3f,
                age = -delay,
                padY = pad.y,
                height = burstHeight,
                accel = dud ? 6.5f : 2f * burstHeight / (0.8f * 0.8f),
                drift = new Vector3(R - 0.5f, 0f, R - 0.5f) * (dud ? 3f : 0.6f),
            });
        }

        // ------------------------------------------------------------------ đèn lồng

        /// <summary>Thả một đèn lồng đỏ từ trên cao: Rigidbody có lực cản lớn (rơi chậm, đung đưa), va rương/đất.</summary>
        public void DropLantern(Vector3 from, Color glow)
        {
            GameObject root = new GameObject("DenLong");
            root.transform.SetParent(transform, true);
            root.transform.position = from;
            root.transform.rotation = Quaternion.Euler(R * 20f - 10f, R * 360f, R * 20f - 10f);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.16f, 0.13f, 0.16f) * _size;
            Material m = ChestFxFactory.CreateUnlit(Color.Lerp(new Color(1f, 0.2f, 0.1f), glow, 0.25f), false, "TrungHoaLantern");
            _mats.Add(m);
            body.GetComponent<Renderer>().sharedMaterial = m;
            Material gold = ChestFxFactory.CreateUnlit(new Color(1f, 0.78f, 0.2f), false, "TrungHoaLanternCap");
            _mats.Add(gold);
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(cap.GetComponent<Collider>());
                cap.transform.SetParent(root.transform, false);
                cap.transform.localPosition = new Vector3(0f, i * 0.07f, 0f) * _size;
                cap.transform.localScale = new Vector3(0.09f, 0.01f, 0.09f) * _size;
                cap.GetComponent<Renderer>().sharedMaterial = gold;
            }
            GameObject tassel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(tassel.GetComponent<Collider>());
            tassel.transform.SetParent(root.transform, false);
            tassel.transform.localPosition = new Vector3(0f, -0.11f, 0f) * _size;
            tassel.transform.localScale = new Vector3(0.012f, 0.06f, 0.012f) * _size;
            tassel.GetComponent<Renderer>().sharedMaterial = gold;

            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            rb.linearDamping = 2.2f;     // giấy nhẹ: rơi chậm
            rb.angularDamping = 0.8f;
            rb.angularVelocity = new Vector3(R - 0.5f, R - 0.5f, R - 0.5f) * 2f;
            rb.linearVelocity = new Vector3(R - 0.5f, 0f, R - 0.5f) * _size * 0.6f;
            Destroy(root, 12f);
        }

        // ------------------------------------------------------------------ cập nhật

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            for (int i = _rockets.Count - 1; i >= 0; i--)
            {
                Rocket r = _rockets[i];
                if (r.done || r.body == null) { _rockets.RemoveAt(i); continue; }
                r.age += dt;
                if (r.age < 0f) continue;
                if (r.body.isKinematic) r.body.isKinematic = false;
                Vector3 pos = r.body.transform.position;
                if (r.age < r.fuel)
                {
                    // lực phụt dọc thân + chút lệch; pháo xịt thì phụt yếu, lảo đảo
                    Vector3 axis = (Vector3.up + r.drift * (r.dud ? Mathf.Sin(r.age * 25f) : 0.2f)).normalized;
                    r.body.AddForce(axis * r.body.mass * (9.81f + r.accel));
                    r.body.MoveRotation(Quaternion.FromToRotation(Vector3.up, r.body.linearVelocity.sqrMagnitude > 0.01f ? r.body.linearVelocity.normalized : axis));
                    if (Time.frameCount % 2 == 0) SmokePuff(pos, new Color(1f, 0.75f, 0.4f, 0.6f), 1, 0.05f);
                }
                if (!r.dud && (pos.y - r.padY >= r.height || r.age > 2.5f))
                {
                    Explode(r, pos);
                    continue;
                }
                if (r.dud && r.age > r.fuel + 0.15f && r.body.linearVelocity.y < 0f && pos.y < r.padY + _size * 0.6f)
                {
                    r.done = true;
                    DudLanded?.Invoke(pos);
                }
            }
        }

        private void Explode(Rocket r, Vector3 pos)
        {
            r.done = true;
            Color gold = new Color(1f, 0.85f, 0.35f);
            if (r.phuc)
            {
                SparkBurst(pos, r.color, gold, 120, _size * 3f, 1.1f);
                PhucGlyph(pos, _size * 2.2f, gold, r.color);
            }
            else
            {
                SparkBurst(pos, r.color, gold, 160, _size * 2.6f, 1.4f);
                SparkBurst(pos, Color.white, gold, 30, _size * 1.2f, 0.6f);
            }
            SmokePuff(pos, new Color(0.7f, 0.65f, 0.6f, 0.4f), 6, 0.1f);
            Burst?.Invoke(pos, r.phuc);
            Destroy(r.body.gameObject);
        }

        private void OnDestroy()
        {
            foreach (Material m in _mats) if (m != null) Destroy(m);
            foreach (ChestFxProfile s in _styles) if (s != null) Destroy(s);
            foreach (ParticleSystem ps in new[] { _sparks, _smoke })
                if (ps != null) Destroy(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            _mats.Clear();
            _styles.Clear();
        }
    }
}

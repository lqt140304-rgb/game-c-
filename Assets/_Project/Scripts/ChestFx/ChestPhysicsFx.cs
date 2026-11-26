using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Sổ của mọi vật hiệu ứng sinh ra trong một lần mở rương (xu, mảnh vỡ, phi thuyền...). <see cref="ChestRevealSequence.ResetReveal"/>
    /// gọi <see cref="Clear"/> nên không sót vật nào. Có trần số vật: vượt quá thì huỷ vật cũ nhất.
    /// </summary>
    public class ChestFxScope
    {
        public const int MaxBodies = 160;

        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly List<Rigidbody> _bodies = new List<Rigidbody>();
        private readonly List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();

        public IReadOnlyList<Rigidbody> Bodies => _bodies;

        public void Add(GameObject go)
        {
            if (go != null) _objects.Add(go);
        }

        public void AddBody(Rigidbody rb)
        {
            if (rb == null) return;
            _bodies.Add(rb);
            _objects.Add(rb.gameObject);
            while (_bodies.Count > MaxBodies)
            {
                Rigidbody old = _bodies[0];
                _bodies.RemoveAt(0);
                if (old != null) UnityEngine.Object.Destroy(old.gameObject);
            }
        }

        /// <summary>Vật liệu/lưới tự dựng, huỷ cùng scope.</summary>
        public void Own(UnityEngine.Object asset)
        {
            if (asset != null) _assets.Add(asset);
        }

        public void Clear()
        {
            foreach (GameObject go in _objects) if (go != null) UnityEngine.Object.Destroy(go);
            foreach (UnityEngine.Object a in _assets) if (a != null) UnityEngine.Object.Destroy(a);
            _objects.Clear();
            _bodies.Clear();
            _assets.Clear();
        }
    }

    /// <summary>
    /// Dọn một vật vật lý: sau <c>life</c> giây thu nhỏ rồi huỷ; rơi quá sâu thì huỷ; và trong <c>ignoreFor</c> giây đầu bỏ qua va chạm
    /// với collider của rương (vật bắn ra từ miệng rương nằm trong thể tích của rương).
    /// </summary>
    public class ChestFxJanitor : MonoBehaviour
    {
        public float life = 6f;
        public float shrinkSeconds = 0.4f;
        public float killBelowY = -50f;
        public float ignoreFor = 0.35f;
        public Collider self;
        public Collider[] ignoredWith;

        private float _age;
        private Vector3 _scale0;
        private bool _restored;

        private void Start() => _scale0 = transform.localScale;

        private void Update()
        {
            _age += Time.deltaTime;
            if (!_restored && _age >= ignoreFor) RestoreCollisions();
            if (transform.position.y < killBelowY) { Destroy(gameObject); return; }
            if (_age >= life)
            {
                float k = 1f - Mathf.Clamp01((_age - life) / Mathf.Max(0.05f, shrinkSeconds));
                transform.localScale = _scale0 * Mathf.Max(0.0001f, k);
                if (k <= 0.001f) Destroy(gameObject);
            }
        }

        private void RestoreCollisions()
        {
            _restored = true;
            if (self == null || ignoredWith == null) return;
            foreach (Collider c in ignoredWith) if (c != null) Physics.IgnoreCollision(self, c, false);
        }
    }

    /// <summary>
    /// Tiện ích vật lý THẬT cho hiệu ứng rương: sinh vật có Rigidbody (xu, bóng, hộp, hoặc prefab bất kỳ), bắn ra theo nón, lực nổ,
    /// dọn dẹp. Mọi ngẫu nhiên dùng <see cref="System.Random"/> do người gọi đưa vào (hạt giống), chỉ để trang trí.
    /// Không dùng thuộc tính đổi tên giữa các bản Unity (velocity/linearVelocity, drag/linearDamping): chỉ AddForce/AddTorque.
    /// Cần PhysicsMaterial (Unity 6; bản cũ tên PhysicMaterial). Chưa chạy trong Unity.
    /// </summary>
    public static class ChestPhysicsFx
    {
        /// <summary>Mô tả một loại vật để sinh.</summary>
        public class BodySpec
        {
            public BodyShape shape = BodyShape.Coin;
            public GameObject prefab;
            public Color color = new Color(1f, 0.82f, 0.25f);
            public float size = 0.05f;
            public float mass = 0.05f;
            public float bounciness = 0.45f;
            public float lifetime = 6f;
            public Collider[] ignoreAtStart;   // collider của rương
        }

        /// <summary>Sinh một vật có Rigidbody tại <paramref name="pos"/>; trả về Rigidbody (đã đăng ký vào scope).</summary>
        public static Rigidbody SpawnBody(ChestFxScope scope, BodySpec spec, Vector3 pos, Quaternion rot)
        {
            GameObject go = Create(spec, scope);
            if (go == null) return null;
            go.transform.SetPositionAndRotation(pos, rot);

            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.mass = Mathf.Max(0.001f, spec.mass);
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;   // vật nhỏ, nhanh: tránh xuyên sàn

            Collider col = go.GetComponentInChildren<Collider>();
            if (col == null) col = AddFittedCollider(go);
            if (col != null)
            {
                PhysicsMaterial pm = new PhysicsMaterial("ChestFxBody") { bounciness = spec.bounciness, dynamicFriction = 0.4f, staticFriction = 0.4f, bounceCombine = PhysicsMaterialCombine.Maximum };
                scope.Own(pm);
                col.sharedMaterial = pm;
            }

            ChestFxJanitor j = go.AddComponent<ChestFxJanitor>();
            j.life = spec.lifetime > 0f ? spec.lifetime : 6f;
            j.killBelowY = pos.y - 30f;
            j.self = col;
            j.ignoredWith = spec.ignoreAtStart;
            if (col != null && spec.ignoreAtStart != null)
            {
                foreach (Collider c in spec.ignoreAtStart) if (c != null && c != col) Physics.IgnoreCollision(col, c, true);
            }

            scope.AddBody(rb);
            return rb;
        }

        /// <summary>
        /// Bắn <paramref name="count"/> vật từ <paramref name="origin"/> theo nón quanh <paramref name="up"/>:
        /// tốc độ đầu ngẫu nhiên trong [speedMin, speedMax] (m/s), xoay ngẫu nhiên tới <paramref name="spin"/> rad/s.
        /// </summary>
        public static List<Rigidbody> Burst(ChestFxScope scope, BodySpec spec, int count, Vector3 origin, Vector3 up, float coneAngle,
            float speedMin, float speedMax, float spin, System.Random rng)
        {
            List<Rigidbody> result = new List<Rigidbody>();
            if (up.sqrMagnitude < 1e-6f) up = Vector3.up;
            up.Normalize();
            Vector3 side = Vector3.Cross(up, Mathf.Abs(up.x) < 0.9f ? Vector3.right : Vector3.forward).normalized;
            for (int i = 0; i < count; i++)
            {
                Rigidbody rb = SpawnBody(scope, spec, origin + RandomInDisc(rng, up, side, spec.size), RandomRotation(rng));
                if (rb == null) continue;
                result.Add(rb);
                rb.AddForce(RandomInCone(rng, up, side, coneAngle) * Lerp(speedMin, speedMax, (float)rng.NextDouble()), ForceMode.VelocityChange);
                rb.AddTorque(new Vector3(Signed(rng), Signed(rng), Signed(rng)) * spin, ForceMode.VelocityChange);
            }
            return result;
        }

        /// <summary>
        /// Lực nổ từ <paramref name="origin"/> lên các vật trong <paramref name="bodies"/>. <paramref name="affectScene"/> = true thì
        /// cũng đẩy mọi Rigidbody khác trong bán kính (đồ trong cảnh): để mặc định false trừ khi chủ đề cần.
        /// Dùng ForceMode.VelocityChange nên <paramref name="force"/> tính theo m/s, không phụ thuộc khối lượng.
        /// </summary>
        public static void Explode(Vector3 origin, float force, float radius, float upwards, IEnumerable<Rigidbody> bodies, bool affectScene)
        {
            HashSet<Rigidbody> done = new HashSet<Rigidbody>();
            if (bodies != null)
            {
                foreach (Rigidbody rb in bodies)
                {
                    if (rb == null || !done.Add(rb)) continue;
                    rb.AddExplosionForce(force, origin, radius, upwards, ForceMode.VelocityChange);
                }
            }
            if (!affectScene) return;
            foreach (Collider c in Physics.OverlapSphere(origin, radius))
            {
                Rigidbody rb = c.attachedRigidbody;
                if (rb == null || rb.isKinematic || !done.Add(rb)) continue;
                rb.AddExplosionForce(force, origin, radius, upwards, ForceMode.VelocityChange);
            }
        }

        /// <summary>
        /// Cho một vật (không dùng Rigidbody) bay qua các điểm thế giới theo đường cong mượt (Catmull-Rom) trong <paramref name="seconds"/>.
        /// Dùng làm <c>yield return</c> trong Cue: <c>yield return ChestPhysicsFx.Fly(ship, points, 3f, true);</c>
        /// </summary>
        public static IEnumerator Fly(Transform t, Vector3[] points, float seconds, bool faceMotion, Action<float> onProgress = null)
        {
            if (t == null || points == null || points.Length == 0) yield break;
            seconds = Mathf.Max(0.05f, seconds);
            float time = 0f;
            while (time < seconds && t != null)
            {
                time += Time.deltaTime;
                float u = Mathf.Clamp01(time / seconds);
                float e = u * u * (3f - 2f * u);
                Vector3 pos = CatmullRom(points, e);
                if (faceMotion)
                {
                    Vector3 ahead = CatmullRom(points, Mathf.Clamp01(e + 0.02f)) - pos;
                    if (ahead.sqrMagnitude > 1e-8f) t.rotation = Quaternion.LookRotation(ahead.normalized, Vector3.up);
                }
                t.position = pos;
                onProgress?.Invoke(u);
                yield return null;
            }
            if (t != null) t.position = points[points.Length - 1];
        }

        /// <summary>Điểm trên đường Catmull-Rom qua các điểm, u 0..1 theo đoạn (không đều theo độ dài; đủ cho hiệu ứng).</summary>
        public static Vector3 CatmullRom(Vector3[] p, float u)
        {
            if (p.Length == 1) return p[0];
            float f = Mathf.Clamp01(u) * (p.Length - 1);
            int i = Mathf.Min(p.Length - 2, (int)f);
            float s = f - i;
            Vector3 p0 = p[Mathf.Max(0, i - 1)], p1 = p[i], p2 = p[i + 1], p3 = p[Mathf.Min(p.Length - 1, i + 2)];
            float s2 = s * s, s3 = s2 * s;
            return 0.5f * ((2f * p1) + (-p0 + p2) * s + (2f * p0 - 5f * p1 + 4f * p2 - p3) * s2 + (-p0 + 3f * p1 - 3f * p2 + p3) * s3);
        }

        // ------------------------------------------------------------------ Dựng vật

        private static GameObject Create(BodySpec spec, ChestFxScope scope)
        {
            if (spec.shape == BodyShape.Prefab)
            {
                if (spec.prefab == null) { Debug.LogWarning("[ChestFx] PhysicsBurst: shape = Prefab nhưng chưa có prefab."); return null; }
                GameObject inst = UnityEngine.Object.Instantiate(spec.prefab);
                inst.transform.localScale = Vector3.one * Mathf.Max(0.001f, spec.size);
                return inst;
            }

            PrimitiveType type = spec.shape == BodyShape.Coin ? PrimitiveType.Cylinder : spec.shape == BodyShape.Cube ? PrimitiveType.Cube : PrimitiveType.Sphere;
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = "ChestFx_" + spec.shape;
            Collider def = go.GetComponent<Collider>();
            if (def != null) UnityEngine.Object.Destroy(def);   // thay collider mặc định bằng hộp/cầu vừa khít

            float s = Mathf.Max(0.002f, spec.size);
            if (spec.shape == BodyShape.Coin)
            {
                // Hình trụ mặc định cao 2, đường kính 1: ép dẹt thành đồng xu.
                go.transform.localScale = new Vector3(s, s * 0.07f, s);
                BoxCollider b = go.AddComponent<BoxCollider>();
                b.size = new Vector3(0.9f, 2f, 0.9f);
            }
            else
            {
                go.transform.localScale = Vector3.one * s;
                if (spec.shape == BodyShape.Cube) go.AddComponent<BoxCollider>();
                else go.AddComponent<SphereCollider>();
            }

            Renderer r = go.GetComponent<Renderer>();
            Material m = MakeMaterial(spec.color, spec.shape == BodyShape.Coin);
            if (m != null)
            {
                scope.Own(m);
                r.sharedMaterial = m;
            }
            else r.enabled = false;   // không có shader dùng được (đã cảnh báo): ẩn vật thay vì để vật liệu mặc định vẽ màu tím
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        private static Collider AddFittedCollider(GameObject go)
        {
            Renderer r = go.GetComponentInChildren<Renderer>();
            if (r == null) return go.AddComponent<SphereCollider>();
            BoxCollider b = go.AddComponent<BoxCollider>();
            b.center = go.transform.InverseTransformPoint(r.bounds.center);
            Vector3 ls = go.transform.lossyScale;
            b.size = new Vector3(r.bounds.size.x / Mathf.Max(1e-4f, Mathf.Abs(ls.x)), r.bounds.size.y / Mathf.Max(1e-4f, Mathf.Abs(ls.y)), r.bounds.size.z / Mathf.Max(1e-4f, Mathf.Abs(ls.z)));
            return b;
        }

        private static Material MakeMaterial(Color color, bool metallic)
        {
            Shader lit = ChestShaderResolver.Resolve(ChestShaderKind.Lit);
            if (lit == null) return ChestFxFactory.CreateUnlit(color, false, "ChestFxBody");   // có thể null: người gọi xử lý
            Material m = new Material(lit);
            m.name = "ChestFxBody";
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic ? 0.9f : 0.1f);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.7f);
            return m;
        }

        // ------------------------------------------------------------------ Ngẫu nhiên có hạt giống

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
        private static float Signed(System.Random rng) => (float)(rng.NextDouble() * 2.0 - 1.0);

        private static Quaternion RandomRotation(System.Random rng) =>
            Quaternion.Euler((float)rng.NextDouble() * 360f, (float)rng.NextDouble() * 360f, (float)rng.NextDouble() * 360f);

        private static Vector3 RandomInDisc(System.Random rng, Vector3 up, Vector3 side, float size)
        {
            float a = (float)rng.NextDouble() * Mathf.PI * 2f;
            float r = Mathf.Sqrt((float)rng.NextDouble()) * size * 0.5f;
            Vector3 side2 = Vector3.Cross(up, side);
            return side * (Mathf.Cos(a) * r) + side2 * (Mathf.Sin(a) * r);
        }

        private static Vector3 RandomInCone(System.Random rng, Vector3 up, Vector3 side, float coneAngle)
        {
            float tilt = coneAngle * Mathf.Sqrt((float)rng.NextDouble());
            float around = (float)rng.NextDouble() * 360f;
            Vector3 tilted = Quaternion.AngleAxis(tilt, side) * up;
            return Quaternion.AngleAxis(around, up) * tilted;
        }
    }
}

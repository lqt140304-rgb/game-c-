using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Đồ dựng bằng code cho kịch bản "Bộ máy Thiên Cơ": lưới bánh răng khối (có độ dày), lưới phễu đồng hồ cát,
    /// vật thể đơn giản không va chạm, vật thể có Rigidbody. Không chứa logic nhịp; xem <see cref="ThienCoHook"/>.
    /// </summary>
    public static class ThienCoRig
    {
        public static readonly Color Brass = new Color(0.86f, 0.64f, 0.26f);
        public static readonly Color DarkBrass = new Color(0.45f, 0.31f, 0.12f);
        public static readonly Color Steel = new Color(0.62f, 0.66f, 0.7f);
        public static readonly Color Sand = new Color(0.93f, 0.78f, 0.45f);

        /// <summary>
        /// Bánh răng khối trong mặt phẳng XY, trục quay là Z, bán kính đỉnh răng = 1, dày <paramref name="thickness"/>.
        /// Có lỗ trục ở giữa (bán kính <paramref name="hub"/>).
        /// </summary>
        public static Mesh GearMesh(int teeth, float hub, float root, float thickness)
        {
            int steps = teeth * 4;                       // chân, lên, đỉnh, xuống cho mỗi răng
            var v = new List<Vector3>();
            var tri = new List<int>();
            float hz = thickness * 0.5f;
            for (int i = 0; i < steps; i++)
            {
                float a = i * Mathf.PI * 2f / steps;
                var d = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
                float outer = (i % 4 == 1 || i % 4 == 2) ? 1f : root;
                v.Add(d * hub + Vector3.forward * -hz);    // 4i+0 trong, trước
                v.Add(d * outer + Vector3.forward * -hz);  // 4i+1 ngoài, trước
                v.Add(d * hub + Vector3.forward * hz);     // 4i+2 trong, sau
                v.Add(d * outer + Vector3.forward * hz);   // 4i+3 ngoài, sau
            }
            for (int i = 0; i < steps; i++)
            {
                int a = i * 4, b = ((i + 1) % steps) * 4;
                tri.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });                     // mặt trước
                tri.AddRange(new[] { a + 2, a + 3, b + 2, a + 3, b + 3, b + 2 });         // mặt sau
                tri.AddRange(new[] { a + 1, b + 1, a + 3, a + 3, b + 1, b + 3 });         // vành răng
                tri.AddRange(new[] { a, a + 2, b, b, a + 2, b + 2 });                     // lỗ trục
            }
            var m = new Mesh { name = "ThienCo_Gear3D", hideFlags = HideFlags.DontSave };
            m.SetVertices(v);
            m.SetTriangles(tri, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        /// <summary>Thân đồng hồ cát: hai nón cụt nối ở cổ, trục Y, cao 1 (0..1), rộng nhất 1 (bán kính 0,5).</summary>
        public static Mesh HourglassMesh(int segments = 20)
        {
            float[] ys = { 0f, 0.08f, 0.45f, 0.5f, 0.55f, 0.92f, 1f };
            float[] rs = { 0.5f, 0.5f, 0.12f, 0.06f, 0.12f, 0.5f, 0.5f };
            var v = new List<Vector3>();
            var tri = new List<int>();
            for (int k = 0; k < ys.Length; k++)
                for (int i = 0; i <= segments; i++)
                {
                    float a = i * Mathf.PI * 2f / segments;
                    v.Add(new Vector3(Mathf.Cos(a) * rs[k], ys[k], Mathf.Sin(a) * rs[k]));
                }
            int row = segments + 1;
            for (int k = 0; k < ys.Length - 1; k++)
                for (int i = 0; i < segments; i++)
                {
                    int a = k * row + i, b = a + row;
                    tri.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1, a, a + 1, b, a + 1, b + 1, b });   // hai mặt
                }
            var m = new Mesh { name = "ThienCo_Hourglass", hideFlags = HideFlags.DontSave };
            m.SetVertices(v);
            m.SetTriangles(tri, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        /// <summary>Vật thể chỉ để nhìn (primitive hoặc lưới riêng), không va chạm.</summary>
        public static Transform Visual(string name, Transform parent, PrimitiveType? prim, Mesh mesh, Material mat)
        {
            GameObject go;
            if (prim.HasValue)
            {
                go = GameObject.CreatePrimitive(prim.Value);
                Collider c = go.GetComponent<Collider>();
                if (c != null) Object.Destroy(c);
            }
            else
            {
                go = new GameObject();
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>();
            }
            go.name = name;
            go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(parent, false);
            Renderer r = go.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go.transform;
        }

        /// <summary>Hộp va chạm tĩnh vô hình (sàn, tường đồng hồ cát, thân rương).</summary>
        public static BoxCollider StaticBox(string name, Transform parent, Vector3 localPos, Quaternion localRot, Vector3 size)
        {
            var go = new GameObject(name) { hideFlags = HideFlags.DontSave };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            BoxCollider b = go.AddComponent<BoxCollider>();
            b.size = size;
            return b;
        }

        public static Rigidbody Body(GameObject go, float mass, bool kinematic)
        {
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.isKinematic = kinematic;
            rb.useGravity = !kinematic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            return rb;
        }
    }

    /// <summary>Gắn lên vật có Rigidbody: báo va chạm đủ mạnh để phát tiếng keng (cao độ theo lực).</summary>
    public class ThienCoImpact : MonoBehaviour
    {
        public System.Action<ThienCoImpact, float> onHit;
        public float minSpeed = 0.35f;
        private float _last;

        private void OnCollisionEnter(Collision c)
        {
            float speed = c.relativeVelocity.magnitude;
            if (speed < minSpeed || Time.time - _last < 0.08f) return;
            _last = Time.time;
            onHit?.Invoke(this, speed);
        }
    }
}

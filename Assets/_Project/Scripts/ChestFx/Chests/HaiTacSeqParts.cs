using UnityEngine;

namespace ChoDoLa.ChestFx
{
    // Các mảnh phụ của kịch bản "Trục vớt bằng đại bác" (Chest_HaiTac). Xem HaiTacSeqHook.cs và
    // docs/superpowers/specs/2026-11-25-kichban-haitac-design.md. Chỉ trang trí, không bao giờ đổi bậc.

    /// <summary>Gắn lên khối va chạm ẩn của rương: báo cho hook mỗi lần đạn/thùng đập vào rương (va chạm thật của PhysX).</summary>
    public class HaiTacSeqContact : MonoBehaviour
    {
        public HaiTacSeqHook owner;

        private void OnCollisionEnter(Collision c)
        {
            if (owner != null && c.rigidbody != null) owner.OnChestStruck(c.rigidbody, c.relativeVelocity);
        }
    }

    /// <summary>Đánh dấu vật thể vật lý do kịch bản tạo (đạn, thùng phao, xu) để hook biết cách xử lý.</summary>
    public class HaiTacSeqBody : MonoBehaviour
    {
        public enum Kind { Ball, Barrel, Coin }
        public Kind kind;
        public bool finale;          // đạn của phát cuối: lực đã được kịch bản áp, va chạm không cộng thêm
        public bool inWater;         // đã chạm mặt nước (để chỉ té nước một lần)
        public float bornTime;
    }

    /// <summary>Khối hình đơn giản dựng bằng primitive: đại bác và con vẹt. Không cần asset ngoài.</summary>
    public static class HaiTacSeqBuild
    {
        public static GameObject Part(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRot, Material mat, bool keepCollider = false)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale;
            if (!keepCollider)
            {
                Collider col = go.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);
            }
            Renderer r = go.GetComponent<Renderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;
            return go;
        }

        /// <summary>Đại bác: bệ gỗ, hai bánh, nòng gang. Trả về gốc; nòng là con tên "Barrel", miệng nòng "Muzzle", lỗ ngòi "Touch".</summary>
        public static Transform Cannon(Transform parent, float size, Material wood, Material iron)
        {
            Transform root = new GameObject("HaiTacSeq_Cannon").transform;
            root.SetParent(parent, false);
            float s = size;
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.12f * s, 0f), new Vector3(0.42f * s, 0.12f * s, 0.62f * s), Quaternion.identity, wood);
            Quaternion wheelRot = Quaternion.Euler(0f, 0f, 90f);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * 0.23f * s;
                float z = (i < 2 ? -1f : 1f) * 0.2f * s;
                Part(PrimitiveType.Cylinder, root, new Vector3(x, 0.09f * s, z), new Vector3(0.18f * s, 0.03f * s, 0.18f * s), wheelRot, wood);
            }
            Transform barrel = new GameObject("Barrel").transform;
            barrel.SetParent(root, false);
            barrel.localPosition = new Vector3(0f, 0.25f * s, -0.05f * s);
            barrel.localRotation = Quaternion.Euler(-12f, 0f, 0f);   // nòng hơi ngóc lên
            Part(PrimitiveType.Cylinder, barrel, new Vector3(0f, 0f, 0.25f * s), new Vector3(0.17f * s, 0.36f * s, 0.17f * s), Quaternion.Euler(90f, 0f, 0f), iron);
            Part(PrimitiveType.Sphere, barrel, new Vector3(0f, 0f, -0.12f * s), new Vector3(0.2f * s, 0.2f * s, 0.2f * s), Quaternion.identity, iron);
            Part(PrimitiveType.Cylinder, barrel, new Vector3(0f, 0f, 0.6f * s), new Vector3(0.21f * s, 0.03f * s, 0.21f * s), Quaternion.Euler(90f, 0f, 0f), iron);
            Transform muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(barrel, false);
            muzzle.localPosition = new Vector3(0f, 0f, 0.64f * s);
            Transform touch = new GameObject("Touch").transform;
            touch.SetParent(barrel, false);
            touch.localPosition = new Vector3(0f, 0.1f * s, -0.08f * s);
            return root;
        }

        /// <summary>Vẹt: thân, đầu, mỏ, hai cánh ("WingL"/"WingR"), đuôi. Gốc ở chân.</summary>
        public static Transform Parrot(Transform parent, float size, Material red, Material green, Material yellow, Material dark)
        {
            Transform root = new GameObject("HaiTacSeq_Parrot").transform;
            root.SetParent(parent, false);
            float s = size;
            Part(PrimitiveType.Sphere, root, new Vector3(0f, 0.12f * s, 0f), new Vector3(0.12f * s, 0.17f * s, 0.12f * s), Quaternion.Euler(-15f, 0f, 0f), red);
            Part(PrimitiveType.Sphere, root, new Vector3(0f, 0.24f * s, 0.02f * s), new Vector3(0.09f * s, 0.09f * s, 0.09f * s), Quaternion.identity, red);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.23f * s, 0.07f * s), new Vector3(0.03f * s, 0.04f * s, 0.05f * s), Quaternion.Euler(30f, 0f, 0f), yellow);
            Part(PrimitiveType.Sphere, root, new Vector3(0.03f * s, 0.255f * s, 0.045f * s), Vector3.one * 0.018f * s, Quaternion.identity, dark);
            Part(PrimitiveType.Sphere, root, new Vector3(-0.03f * s, 0.255f * s, 0.045f * s), Vector3.one * 0.018f * s, Quaternion.identity, dark);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.04f * s, -0.09f * s), new Vector3(0.05f * s, 0.02f * s, 0.16f * s), Quaternion.Euler(35f, 0f, 0f), green);
            for (int side = -1; side <= 1; side += 2)
            {
                Transform wing = new GameObject(side < 0 ? "WingL" : "WingR").transform;
                wing.SetParent(root, false);
                wing.localPosition = new Vector3(0.055f * side * s, 0.15f * s, 0f);
                Part(PrimitiveType.Cube, wing, new Vector3(0.06f * side * s, 0f, -0.01f * s), new Vector3(0.12f * s, 0.015f * s, 0.09f * s), Quaternion.identity, green);
            }
            return root;
        }
    }
}

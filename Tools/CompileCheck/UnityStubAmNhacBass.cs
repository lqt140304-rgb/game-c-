// Bổ sung stub (chỉ để biên dịch thử, không phải Unity thật) cho các API vật lý/âm thanh mà AmNhacBassHook dùng
// nhưng UnityStub.cs + UnityStubAll.cs chưa có. Chỉ dùng trong check_am_nhac_bass.sh; không sửa stub chung.
namespace UnityEngine
{
    public partial class Transform { public void Rotate(float x, float y, float z, Space s) {} }
    public partial class Rigidbody { public Vector3 position;
        public void AddTorque(Vector3 t, ForceMode m) {} public void AddForceAtPosition(Vector3 f, Vector3 p, ForceMode m) {} }
    public static partial class Physics { public static Collider[] OverlapSphere(Vector3 p, float r) => new Collider[0]; }
    public partial class AudioSource { public void PlayDelayed(float d) {} }
}

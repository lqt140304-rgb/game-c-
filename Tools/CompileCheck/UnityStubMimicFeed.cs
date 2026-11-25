// API Unity thêm cho MimicFeed* (chỉ để biên dịch thử, không phải Unity thật).
namespace UnityEngine
{
    public enum RigidbodyInterpolation { None, Interpolate, Extrapolate }
    public partial class Rigidbody { public Vector3 position; public RigidbodyInterpolation interpolation; }
    public static partial class Physics
    {
        public static Collider[] OverlapSphere(Vector3 p, float r) => new Collider[0];
        public static void IgnoreCollision(Collider a, Collider b, bool i) {}
    }
    public partial class ParticleSystemRenderer : Renderer {}
}

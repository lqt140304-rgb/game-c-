// API Unity bổ sung cho check_game_voxel.sh (chỉ để biên dịch thử, không phải Unity thật).
namespace UnityEngine
{
    public static partial class GUI { public static void DrawTextureWithTexCoords(Rect r, Texture t, Rect uv) {} }
    public partial class Rigidbody { public void AddTorque(Vector3 t, ForceMode m) {} }
}

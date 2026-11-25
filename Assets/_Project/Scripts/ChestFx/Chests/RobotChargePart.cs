using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật nạp pin cho "Rương Biến Hình" (Chest_Robot): pin, bu-lông, bánh răng. Gắn lên bất kỳ vật có Rigidbody mà
    /// người chơi có thể cầm/ném/thả; khi vật rơi vào khe sạc trên lưng rương (<see cref="RobotChargeSocket"/>) nó bị
    /// "nuốt" và cộng <see cref="charge"/> vào thanh pin. Vật có Rigidbody KHÔNG gắn component này vẫn được nhận
    /// (đồ lạ, giá trị mặc định nhỏ hơn). Chỉ trang trí: pin KHÔNG bao giờ ảnh hưởng tới bậc đã quay.
    /// </summary>
    public class RobotChargePart : MonoBehaviour
    {
        public enum Kind { Pin, BuLong, BanhRang, DoLa }

        public Kind kind = Kind.Pin;
        [Range(0f, 1f), Tooltip("Phần pin cộng thêm khi vật lọt vào khe (1 = đầy).")]
        public float charge = 0.35f;

        /// <summary>Đã bị khe nuốt (đừng đếm hai lần).</summary>
        [HideInInspector] public bool consumed;

        public static float DefaultChargeFor(Kind k)
        {
            switch (k)
            {
                case Kind.Pin: return 0.35f;
                case Kind.BanhRang: return 0.25f;
                case Kind.BuLong: return 0.15f;
                default: return 0.2f;
            }
        }
    }
}

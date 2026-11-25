using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Mảnh đồ chơi của cỗ máy bi ve (Chest_TuoiTho): báo va chạm về <see cref="TuoiThoHook"/> để phát tiếng
    /// cách/cộp đúng lúc vật lý chạm nhau. Không tự làm gì khác.
    /// </summary>
    public class TuoiThoPiece : MonoBehaviour
    {
        public enum Kind { Marble, Domino, Car, Top, Plane }

        public TuoiThoHook owner;
        public Kind kind;
        public int index;
        [System.NonSerialized] public float lastSound = -1f;

        private void OnCollisionEnter(Collision c)
        {
            if (owner != null) owner.OnPieceHit(this, c.relativeVelocity.magnitude, c.collider);
        }
    }
}

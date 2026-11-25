using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Vùng trigger ở miệng Rương Đói: báo cho <see cref="MimicFeedHook"/> khi có Rigidbody lọt vào.</summary>
    public class MimicFeedMouth : MonoBehaviour
    {
        public MimicFeedHook owner;

        private void OnTriggerEnter(Collider other)
        {
            if (owner != null && other != null) owner.OnMouthTouched(other);
        }
    }
}

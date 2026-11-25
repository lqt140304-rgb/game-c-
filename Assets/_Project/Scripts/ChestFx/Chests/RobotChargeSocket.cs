using System;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Khe sạc trên lưng Chest_Robot: một BoxCollider isTrigger (Rigidbody kinematic để trigger luôn chạy).
    /// Mọi Rigidbody không thuộc rương lọt vào sẽ được báo qua <see cref="onPart"/>. Do RobotTransformHook tạo ra lúc chạy.
    /// </summary>
    public class RobotChargeSocket : MonoBehaviour
    {
        public Action<Rigidbody> onPart;
        public Transform ignoreRoot;   // các vật con của rương (chân, tay, mảnh văng...) không tính

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null || rb.isKinematic) return;
            if (ignoreRoot != null && rb.transform.IsChildOf(ignoreRoot)) return;
            RobotChargePart part = rb.GetComponent<RobotChargePart>();
            if (part != null && part.consumed) return;
            onPart?.Invoke(rb);
        }
    }
}

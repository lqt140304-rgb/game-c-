using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Đĩa than cho kịch bản Chest_AmNhac (<see cref="AmNhacBassHook"/>). Hook tự tạo một đĩa lơ lửng; người làm cảnh
    /// cũng có thể gắn component này lên BẤT KỲ vật Rigidbody nào (vd. một đĩa than người chơi cầm): khi nó rơi vào
    /// mâm rương đang mở, hook coi như "đĩa vào máy" (kim đặt xuống, mở lớp bass wub, tăng độ quẩy). Chỉ trang trí.
    /// </summary>
    public class AmNhacVinyl : MonoBehaviour
    {
        private static readonly List<AmNhacVinyl> _all = new List<AmNhacVinyl>();
        public static IReadOnlyList<AmNhacVinyl> All => _all;

        public bool Dropped { get; private set; }
        public bool Seated { get; private set; }
        public Rigidbody Body { get; private set; }

        private void Awake() => Body = GetComponent<Rigidbody>();
        private void OnEnable() { if (!_all.Contains(this)) _all.Add(this); }
        private void OnDisable() => _all.Remove(this);

        /// <summary>Let go: gravity takes it (a small push so it wobbles on the way down).</summary>
        public void Drop()
        {
            Dropped = true;
            if (Body == null) Body = GetComponent<Rigidbody>();
            if (Body == null) return;
            Body.isKinematic = false;
            Body.AddTorque(new Vector3(0.002f, 0f, 0.0015f), ForceMode.Impulse);
        }

        /// <summary>Landed on the platter: freeze it on the record so it spins with it.</summary>
        public void Seat(Transform platter, Vector3 centre)
        {
            Seated = true;
            Dropped = true;
            if (Body != null) Body.isKinematic = true;
            transform.SetParent(platter, true);
            transform.position = centre + Vector3.up * 0.006f;
            transform.rotation = platter.rotation;
        }
    }
}

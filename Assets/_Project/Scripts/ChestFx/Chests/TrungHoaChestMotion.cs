using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TrungHoa: chuyển động CẢ RƯƠNG do pháo gây ra (nảy lên, nghiêng, xoay), tính bằng mô phỏng vật lý nhỏ
    /// (trọng lực + nảy đất có hệ số đàn hồi cho trục đứng, lò xo - giảm chấn cho độ nghiêng).
    ///
    /// Vì BoxOpenSequence ghi đè localPosition của rương trong coroutine (chạy SAU mọi Update, TRƯỚC LateUpdate),
    /// ta trừ độ lệch cũ ở Update và cộng độ lệch mới ở LateUpdate: hộp vẫn rung/mở bình thường, ta cộng thêm lên trên.
    /// Không dùng Rigidbody cho chính rương để không tranh quyền với BoxOpenSequence/ChestRevealSequence.
    /// </summary>
    public class TrungHoaChestMotion : MonoBehaviour
    {
        public float gravity = 9.81f;
        public float bounce = 0.35f;        // hệ số nảy khi rơi chạm đất
        public float tiltSpring = 60f;      // lò xo kéo rương về thẳng
        public float tiltDamping = 7f;

        private float _height, _vy;          // độ cao trên mặt đất (m) và vận tốc đứng
        private Vector2 _tilt, _tiltVel;     // nghiêng quanh X/Z (độ)
        private float _yaw, _yawVel;         // xoay quanh Y (độ), có ma sát
        private Vector3 _appliedPos;
        private Quaternion _appliedRot = Quaternion.identity;
        private bool _applied;

        /// <summary>Một cú hích: vận tốc đứng (m/s), cú nghiêng (độ/s, theo hướng ngang thế giới), cú xoay (độ/s).</summary>
        public void Kick(float upSpeed, Vector3 worldPush, float tiltSpeed, float spinSpeed = 0f)
        {
            _vy = Mathf.Max(_vy, 0f) + upSpeed;
            Vector3 local = transform.parent != null ? transform.parent.InverseTransformDirection(worldPush) : worldPush;
            // đẩy theo +X làm rương nghiêng quanh -Z, đẩy theo +Z làm nghiêng quanh +X
            _tiltVel += new Vector2(local.z, -local.x) * tiltSpeed;
            _yawVel += spinSpeed;
        }

        public bool Airborne => _height > 0.0005f || _vy > 0f;

        private void Update()
        {
            if (!_applied) return;
            // gỡ độ lệch frame trước (nếu hộp không ghi đè thì đây là trạng thái gốc)
            transform.localPosition -= _appliedPos;
            transform.localRotation = transform.localRotation * Quaternion.Inverse(_appliedRot);
            _applied = false;
        }

        private void LateUpdate()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            _vy -= gravity * dt;
            _height += _vy * dt;
            if (_height <= 0f)
            {
                _height = 0f;
                _vy = _vy < -0.4f ? -_vy * bounce : 0f;
            }
            Vector2 acc = -_tilt * tiltSpring - _tiltVel * tiltDamping;
            _tiltVel += acc * dt;
            _tilt += _tiltVel * dt;
            _yaw += _yawVel * dt;
            _yawVel *= Mathf.Exp(-3f * dt);

            _appliedPos = new Vector3(0f, _height, 0f);
            _appliedRot = Quaternion.Euler(_tilt.x, _yaw, _tilt.y);
            transform.localPosition += _appliedPos;
            transform.localRotation = transform.localRotation * _appliedRot;
            _applied = true;
        }

        /// <summary>Trả rương về đúng chỗ cũ (gọi trước khi huỷ component). Góc xoay còn dư cũng bỏ.</summary>
        public void Restore()
        {
            if (_applied)
            {
                transform.localPosition -= _appliedPos;
                transform.localRotation = transform.localRotation * Quaternion.Inverse(_appliedRot);
            }
            _applied = false;
            _height = _vy = _yaw = _yawVel = 0f;
            _tilt = _tiltVel = Vector2.zero;
        }
    }
}

using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Một món "đồ ăn" cho Rương Đói (Chest_Mimic). Gắn tự động lên đồ vứt rơi do <see cref="MimicFeedHook"/> tạo ra,
    /// và cả lên vật NGOÀI (người chơi tự thả Rigidbody vào miệng) để rương nhớ mà nhổ trả lại cuối màn.
    /// Chỉ là vật lý trang trí: không liên quan gì tới bậc trúng.
    /// </summary>
    public class MimicFeedFood : MonoBehaviour
    {
        /// <summary>true = do hook tạo ra (được xoá khi reset); false = vật của người chơi (phải trả lại nguyên vẹn).</summary>
        public bool spawnedByHook;
        public bool eaten;
        public Rigidbody body;

        private bool _wasKinematic;

        public Vector3 Velocity
        {
            get
            {
                if (body == null) return Vector3.zero;
#if UNITY_6000_0_OR_NEWER
                return body.linearVelocity;
#else
                return body.velocity;
#endif
            }
            set
            {
                if (body == null) return;
#if UNITY_6000_0_OR_NEWER
                body.linearVelocity = value;
#else
                body.velocity = value;
#endif
            }
        }

        /// <summary>Ném theo đường đạn đạo sao cho tới <paramref name="target"/> sau <paramref name="flightSeconds"/> giây.</summary>
        public void TossTo(Vector3 target, float flightSeconds)
        {
            if (body == null || eaten) return;
            body.isKinematic = false;
            float t = Mathf.Max(0.15f, flightSeconds);
            Vector3 d = target - transform.position;
            // d = v*t + 0.5*g*t^2  =>  v = (d - 0.5*g*t^2) / t
            Vector3 v = (d - 0.5f * Physics.gravity * t * t) / t;
            Velocity = v;
            body.angularVelocity = new Vector3(Random.Range(-12f, 12f), Random.Range(-12f, 12f), Random.Range(-12f, 12f));
        }

        /// <summary>Rương nuốt: tắt vật lý, giấu đi (vật của người chơi được giữ lại để nhổ trả).</summary>
        public void Swallow()
        {
            if (eaten) return;
            eaten = true;
            if (body != null)
            {
                _wasKinematic = body.isKinematic;
                Velocity = Vector3.zero;
                body.isKinematic = true;
            }
            gameObject.SetActive(false);
        }

        /// <summary>Nhổ ra từ <paramref name="mouth"/> với vận tốc <paramref name="velocity"/>.</summary>
        public void SpitOut(Vector3 mouth, Vector3 velocity)
        {
            eaten = false;
            transform.position = mouth;
            gameObject.SetActive(true);
            if (body == null) return;
            body.isKinematic = spawnedByHook ? false : _wasKinematic;
            if (!body.isKinematic)
            {
                Velocity = velocity;
                body.angularVelocity = new Vector3(Random.Range(-15f, 15f), Random.Range(-15f, 15f), Random.Range(-15f, 15f));
            }
        }
    }
}

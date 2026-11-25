using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Một quả bóng thật của rương Thể thao: Rigidbody + vật liệu nảy + lực Magnus (bóng xoáy thì cong).
    /// Hai chế độ:
    ///  - "Sút theo kịch bản" (<see cref="Shoot"/>): bóng bay theo đường cong Bezier tới đích đúng giờ (để cú sút cuối
    ///    tới miệng rương đúng lúc chốt bậc), gần tới đích thì THẢ cho vật lý thật lo phần va chạm/nảy.
    ///  - "Vật lý tự do" (<see cref="Kick"/>, bóng thay người rơi xuống): chỉ có trọng lực, nảy, xoáy Magnus.
    /// Va chạm nào cũng báo về <see cref="TheThaoHook"/> (tiếng nảy, tiếng cột dọc, rương bị đập thì nảy lên).
    /// Bóng chỉ để trang trí: không đọc/ghi bậc trúng.
    /// </summary>
    public class TheThaoBall : MonoBehaviour
    {
        public enum Ending { Free, ReleaseAtEnd, SwallowIntoMouth }

        [Tooltip("Hệ số Magnus: lực = k * (vận tốc góc x vận tốc). Lớn thì cong nhiều.")]
        public float magnus = 0.012f;

        public Rigidbody Body { get; private set; }
        public bool Swallowed { get; private set; }
        public bool Flying => _flying;

        private TheThaoHook _owner;
        private bool _flying;
        private Vector3 _p0, _p1, _p2;     // Bezier bậc 2: xuất phát, điểm uốn (tạo độ cong), đích
        private float _t, _duration, _releaseAt;
        private Ending _ending;
        private Vector3 _spin;
        private Transform _mouthTarget;
        private float _swallowT;
        private Vector3 _swallowFrom;
        private float _startScale;
        private float _lastHitSound;

        public void Init(TheThaoHook owner, Rigidbody body)
        {
            _owner = owner;
            Body = body;
            _startScale = transform.localScale.x;
        }

        /// <summary>
        /// Sút theo kịch bản: từ vị trí hiện tại tới <paramref name="target"/> trong <paramref name="duration"/> giây,
        /// cong ngang <paramref name="curve"/> mét, bổng <paramref name="lift"/> mét. <paramref name="releaseFraction"/>:
        /// tới phần này của quãng đường thì thả cho vật lý (0,85–0,95 để va chạm thật xảy ra ngay sau đó).
        /// </summary>
        public void Shoot(Vector3 target, float duration, Vector3 curveSide, float lift, Ending ending, Transform mouth = null, float releaseFraction = 0.9f)
        {
            _p0 = transform.position;
            _p2 = target;
            _p1 = (_p0 + _p2) * 0.5f + curveSide + Vector3.up * lift;
            _duration = Mathf.Max(0.1f, duration);
            _releaseAt = Mathf.Clamp(releaseFraction, 0.3f, 1f);
            _ending = ending;
            _mouthTarget = mouth;
            _t = 0f;
            _flying = true;
            // Xoáy theo hướng cong: trục quay vuông góc với hướng bay và độ cong (đúng chiều để Magnus cong tiếp sau khi thả).
            Vector3 dir = (_p2 - _p0).normalized;
            _spin = Vector3.Cross(curveSide, dir) * 40f + Vector3.Cross(Vector3.up, dir) * 8f;
            if (Body != null) { Body.isKinematic = true; }
        }

        /// <summary>Sút tự do bằng lực thật (người chơi bấm K). Có xoáy thì bóng cong nhờ Magnus.</summary>
        public void Kick(Vector3 velocity, Vector3 angularVelocity)
        {
            _flying = false;
            if (Body == null) return;
            Body.isKinematic = false;
            Body.linearVelocity = velocity;
            Body.angularVelocity = angularVelocity;
        }

        private void Update()
        {
            if (Swallowed) { UpdateSwallow(); return; }
            if (!_flying) return;

            _t += Time.deltaTime / _duration;
            float u = Mathf.Min(_t, 1f);
            Vector3 pos = Bezier(u);
            transform.position = pos;
            transform.rotation = Quaternion.AngleAxis(_spin.magnitude * 57.3f * Time.deltaTime, _spin.normalized) * transform.rotation;

            if (u >= _releaseAt)
            {
                _flying = false;
                if (_ending == Ending.SwallowIntoMouth)
                {
                    BeginSwallow();
                    return;
                }
                if (Body != null)
                {
                    Body.isKinematic = false;
                    Body.linearVelocity = BezierVelocity(u);
                    Body.angularVelocity = _spin;
                }
            }
        }

        private void FixedUpdate()
        {
            if (Body == null || Body.isKinematic || Swallowed) return;
            // Lực Magnus: bóng xoáy thì quỹ đạo cong (sút xoáy, bóng chạm cột rồi xoáy lệch đi).
            Vector3 f = Vector3.Cross(Body.angularVelocity, Body.linearVelocity) * magnus;
            Body.AddForce(f, ForceMode.Force);
        }

        private void OnCollisionEnter(Collision c)
        {
            float speed = c.relativeVelocity.magnitude;
            if (_owner != null && Time.time - _lastHitSound > 0.06f)
            {
                _lastHitSound = Time.time;
                _owner.OnBallHit(this, c.collider, speed, c.relativeVelocity);
            }
        }

        // ------------------------------------------------------------------ bóng lọt vào miệng rương (bàn thắng)

        private void BeginSwallow()
        {
            Swallowed = true;
            _swallowT = 0f;
            _swallowFrom = transform.position;
            if (Body != null) { Body.isKinematic = true; Body.detectCollisions = false; }
            if (_owner != null) _owner.OnBallScored(this);
        }

        private void UpdateSwallow()
        {
            _swallowT += Time.deltaTime / 0.25f;
            float u = Mathf.Clamp01(_swallowT);
            Vector3 to = _mouthTarget != null ? _mouthTarget.position : _p2;
            transform.position = Vector3.Lerp(_swallowFrom, to, u);
            transform.localScale = Vector3.one * (_startScale * (1f - 0.9f * u));
            if (u >= 1f) gameObject.SetActive(false);
        }

        private Vector3 Bezier(float u)
        {
            float a = 1f - u;
            return _p0 * (a * a) + _p1 * (2f * a * u) + _p2 * (u * u);
        }

        private Vector3 BezierVelocity(float u)
        {
            Vector3 d = (_p1 - _p0) * (2f * (1f - u)) + (_p2 - _p1) * (2f * u);
            return d / _duration;
        }
    }
}

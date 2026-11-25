using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật nổi của "Đêm hội làng" (đèn hoa đăng, bánh chưng): Rigidbody thật, lực nổi kiểu lò xo dưới mặt nước,
    /// cản nước, và lực "tụ hội" kéo về một vòng quanh rương (kèm xoáy nhẹ). Va vào rương thì báo hook để rương chao.
    /// Chế độ <see cref="Rising"/>: tắt trọng lực, bay lên như đèn trời rồi tự huỷ. Do VietNamHoiLangHook tạo lúc chạy.
    /// </summary>
    public class VietNamHoiLangFloater : MonoBehaviour
    {
        private VietNamHoiLangHook _owner;
        private Rigidbody _body;
        private Renderer _flame;
        private Light _light;
        private float _lit = 1f, _litTarget = 1f;
        private float _riseAge;
        private Vector3 _flameBase;

        public Rigidbody Body => _body;
        public float WaterY { get; set; }
        public Vector3 Center { get; set; }
        public float GatherRadius { get; set; } = 0.6f;
        /// <summary>0 = trôi tự do, 1 = tụ về rương, âm = dạt ra xa.</summary>
        public float Gather { get; set; } = 1f;
        public bool Rising { get; private set; }

        public void Init(VietNamHoiLangHook owner, Rigidbody body)
        {
            _owner = owner;
            _body = body;
        }

        public void SetFlame(Renderer flame, Light light)
        {
            _flame = flame;
            _light = light;
            if (flame != null) _flameBase = flame.transform.localScale;
        }

        /// <summary>Thổi tắt (bậc thấp) hoặc thắp lại.</summary>
        public void SetLit(float target) => _litTarget = Mathf.Clamp01(target);

        /// <summary>Thành đèn trời: bay lên, lượn, mờ dần.</summary>
        public void Rise()
        {
            if (Rising || _body == null) return;
            Rising = true;
            _body.useGravity = false;
            _body.linearDamping = 0.6f;
        }

        private void FixedUpdate()
        {
            if (_body == null) return;
            float dt = Time.fixedDeltaTime;
            Vector3 p = _body.position;

            if (Rising)
            {
                _riseAge += dt;
                float sway = Mathf.Sin((_riseAge + p.x * 3f) * 1.7f) * 0.4f;
                _body.AddForce(new Vector3(sway, 1.1f, sway * 0.5f), ForceMode.Acceleration);
                if (_riseAge > 7f) Destroy(gameObject);
                return;
            }

            float depth = WaterY - p.y;
            if (depth > 0f)
            {
                // Lò xo nổi (cân bằng ngập ~4 cm) + cản nước + dựng thẳng.
                _body.AddForce(Vector3.up * Mathf.Min(depth, 0.12f) * 220f, ForceMode.Acceleration);
                _body.AddForce(-_body.linearVelocity * 2.2f, ForceMode.Acceleration);
                Vector3 tiltAxis = Vector3.Cross(transform.up, Vector3.up);
                _body.AddTorque(tiltAxis * 6f - _body.angularVelocity * 0.8f, ForceMode.Acceleration);

                Vector3 d = Center - p;
                d.y = 0f;
                float dist = d.magnitude;
                if (dist > 0.001f && Gather != 0f)
                {
                    Vector3 dir = d / dist;
                    Vector3 radial = Gather > 0f ? dir * (dist - GatherRadius) * 0.9f * Gather : dir * 0.6f * Gather;
                    Vector3 swirl = Vector3.Cross(Vector3.up, dir) * 0.22f * Mathf.Abs(Gather);
                    _body.AddForce(radial + swirl, ForceMode.Acceleration);
                }
            }
        }

        private void Update()
        {
            _lit = Mathf.MoveTowards(_lit, _litTarget, Time.deltaTime * 4f);
            float flicker = 0.85f + 0.15f * Mathf.PerlinNoise(Time.time * 9f, transform.position.x * 13f);
            if (_flame != null) _flame.transform.localScale = _flameBase * Mathf.Max(0.02f, _lit * flicker);
            if (_light != null) _light.intensity = 1.2f * _lit * flicker;
        }

        private void OnCollisionEnter(Collision c)
        {
            if (_owner != null && !Rising) _owner.OnFloaterHit(this, c.collider, c.relativeVelocity.magnitude);
        }
    }
}

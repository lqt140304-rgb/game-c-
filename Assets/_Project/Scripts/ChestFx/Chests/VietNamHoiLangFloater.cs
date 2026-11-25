using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Hoa đăng của "Đêm hội làng" (đã thu gọn theo kế hoạch 2026-11-25): Rigidbody kinematic, KHÔNG còn nổi trên nước.
    /// Hai việc: (1) <see cref="FlyIn"/> — bay từ vòng 1,4 S vào miệng rương theo đường cong Bezier 3 điểm, tới nơi thì thu nhỏ
    /// về 0 rồi tắt (GameObject ẩn, giữ lại dùng lúc kết); (2) <see cref="Emerge"/> — trồi khỏi miệng rương, bay lên với tốc độ
    /// cố định như đèn trời, mờ dần rồi tự huỷ. Do VietNamHoiLangHook tạo lúc chạy.
    /// </summary>
    public class VietNamHoiLangFloater : MonoBehaviour
    {
        private enum Mode { Idle, Fly, Shrink, Grow, Rise, Fade }

        private VietNamHoiLangHook _owner;
        private Rigidbody _body;
        private Renderer _flame;
        private Light _light;
        private Vector3 _flameBase;
        private float _lit = 1f, _litTarget = 1f;

        private Mode _mode = Mode.Idle;
        private float _age, _dur, _peak;
        private Vector3 _from;
        private float _riseSpeed, _riseHeight, _riseDone;

        public Rigidbody Body => _body;
        public bool Hidden => _mode == Mode.Idle && !gameObject.activeSelf;
        public bool InFlight => _mode == Mode.Fly || _mode == Mode.Shrink;
        /// <summary>Điểm xuất phát của lượt bay vào (để hook biết đèn tới từ hướng nào).</summary>
        public Vector3 FlyFrom => _from;

        public void Init(VietNamHoiLangHook owner, Rigidbody body)
        {
            _owner = owner;
            _body = body;
            if (_body != null)
            {
                _body.isKinematic = true;
                _body.useGravity = false;
            }
        }

        public void SetFlame(Renderer flame, Light light)
        {
            _flame = flame;
            _light = light;
            if (flame != null) _flameBase = flame.transform.localScale;
        }

        /// <summary>Bay từ <paramref name="from"/> vào miệng rương trong <paramref name="duration"/> giây; đỉnh cong cao hơn miệng <paramref name="peak"/>.</summary>
        public void FlyIn(Vector3 from, float peak, float duration)
        {
            _from = from;
            _peak = peak;
            _dur = Mathf.Max(0.1f, duration);
            _age = 0f;
            _mode = Mode.Fly;
            transform.localScale = Vector3.one;
            transform.position = from;
        }

        /// <summary>Trồi khỏi miệng rương, bay thẳng lên <paramref name="height"/> với tốc độ cố định, rồi mờ dần và tự huỷ.</summary>
        public void Emerge(float height, float speed)
        {
            gameObject.SetActive(true);
            _riseHeight = Mathf.Max(0.1f, height);
            _riseSpeed = Mathf.Max(0.1f, speed);
            _riseDone = 0f;
            _from = _owner != null ? _owner.MouthPoint : transform.position;
            transform.position = _from;
            transform.localScale = Vector3.zero;
            _lit = 1f;
            _litTarget = 1f;
            _age = 0f;
            _mode = Mode.Grow;
        }

        /// <summary>Tắt hẳn ngay (dùng khi dọn).</summary>
        public void Hide()
        {
            _mode = Mode.Idle;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _age += dt;
            switch (_mode)
            {
                case Mode.Fly:
                {
                    float u = Mathf.Clamp01(_age / _dur);
                    Vector3 p0 = _from;
                    Vector3 p2 = _owner != null ? _owner.MouthPoint : _from;
                    Vector3 p1 = (p0 + p2) * 0.5f;
                    p1.y = (p0.y + p2.y) * 0.5f + 2f * _peak; // đỉnh đường cong = trung điểm + _peak
                    float v = 1f - u;
                    transform.position = v * v * p0 + 2f * v * u * p1 + u * u * p2;
                    if (u >= 1f)
                    {
                        _mode = Mode.Shrink;
                        _age = 0f;
                        if (_owner != null) _owner.OnLanternArrived(this);
                    }
                    break;
                }
                case Mode.Shrink:
                {
                    float u = Mathf.Clamp01(_age / 0.2f);
                    if (_owner != null) transform.position = _owner.MouthPoint;
                    transform.localScale = Vector3.one * (1f - u);
                    if (u >= 1f) Hide();
                    break;
                }
                case Mode.Grow:
                {
                    float u = Mathf.Clamp01(_age / 0.2f);
                    transform.position = _from;
                    transform.localScale = Vector3.one * u;
                    if (u >= 1f) _mode = Mode.Rise;
                    break;
                }
                case Mode.Rise:
                {
                    _riseDone += _riseSpeed * dt;
                    float sway = Mathf.Sin(_age * 2.2f + _from.x * 3f) * 0.05f * _riseHeight;
                    transform.position = _from + new Vector3(sway, _riseDone, sway * 0.5f);
                    if (_riseDone >= _riseHeight * 0.75f) { _mode = Mode.Fade; _age = 0f; _litTarget = 0f; }
                    break;
                }
                case Mode.Fade:
                {
                    _riseDone += _riseSpeed * dt;
                    float sway = Mathf.Sin(_age * 2.2f + _from.x * 3f) * 0.05f * _riseHeight;
                    transform.position = _from + new Vector3(sway, _riseDone, sway * 0.5f);
                    float u = Mathf.Clamp01(_age / 0.45f);
                    transform.localScale = Vector3.one * (1f - u);
                    if (u >= 1f) Destroy(gameObject);
                    break;
                }
            }

            _lit = Mathf.MoveTowards(_lit, _litTarget, dt * 4f);
            float flicker = 0.85f + 0.15f * Mathf.PerlinNoise(Time.time * 9f, transform.position.x * 13f);
            if (_flame != null) _flame.transform.localScale = _flameBase * Mathf.Max(0.02f, _lit * flicker);
            if (_light != null) _light.intensity = 1.2f * _lit * flicker;
        }
    }
}

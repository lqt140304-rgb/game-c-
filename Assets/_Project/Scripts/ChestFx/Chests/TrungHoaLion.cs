using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Đầu sư tử múa bằng giấy (dựng từ khối cơ bản, không cần asset) — nhân vật của sự kiện phụ:
    /// nhảy vào ngậm nén nhang, cúi châm ngòi, giật mình nhảy lùi theo từng tràng pháo, cuối cùng:
    /// bậc cao nhảy múa vòng quanh rương; bậc Rác hắt xì một cái rồi đổ kềnh (lúc đó mới bật Rigidbody, ngã thật).
    /// Chuyển động bằng code (cung parabol); chỉ trang trí.
    /// </summary>
    public class TrungHoaLion : MonoBehaviour
    {
        private Transform _jaw, _ember;
        private readonly List<Material> _mats = new List<Material>();
        private Vector3 _from, _to;
        private float _hopT = 1f, _hopDur = 0.4f, _hopHeight;
        private float _danceT = -1f, _danceRadius;
        private Vector3 _danceCenter;
        private float _jawOpen, _jawTarget;
        private bool _ragdoll;
        private float _size = 0.4f;

        public Vector3 EmberPosition => _ember != null ? _ember.position : transform.position;
        public bool Hopping => _hopT < 1f;

        public static TrungHoaLion Build(float size, Transform parent)
        {
            GameObject root = new GameObject("TrungHoa_SuTu");
            root.transform.SetParent(parent, false);
            TrungHoaLion lion = root.AddComponent<TrungHoaLion>();
            lion._size = size;
            Color red = new Color(0.9f, 0.12f, 0.08f), gold = new Color(1f, 0.78f, 0.2f), white = Color.white, black = new Color(0.05f, 0.05f, 0.05f);
            float s = size;
            lion.Part(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.5f, 0f) * s, new Vector3(0.8f, 0.7f, 0.7f) * s, red, "Dau");
            lion.Part(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.92f, 0.05f) * s, new Vector3(0.18f, 0.25f, 0.18f) * s, gold, "Sung");
            lion.Part(PrimitiveType.Sphere, root.transform, new Vector3(-0.2f, 0.65f, 0.36f) * s, Vector3.one * 0.22f * s, white, "MatT");
            lion.Part(PrimitiveType.Sphere, root.transform, new Vector3(0.2f, 0.65f, 0.36f) * s, Vector3.one * 0.22f * s, white, "MatP");
            lion.Part(PrimitiveType.Sphere, root.transform, new Vector3(-0.2f, 0.65f, 0.46f) * s, Vector3.one * 0.09f * s, black, "ConNguoiT");
            lion.Part(PrimitiveType.Sphere, root.transform, new Vector3(0.2f, 0.65f, 0.46f) * s, Vector3.one * 0.09f * s, black, "ConNguoiP");
            // râu/bờm: vài khối vàng
            for (int i = 0; i < 5; i++)
                lion.Part(PrimitiveType.Cube, root.transform, new Vector3(-0.36f + i * 0.18f, 0.88f, -0.1f) * s, new Vector3(0.12f, 0.12f, 0.5f) * s, gold, "Bom");
            Transform jaw = new GameObject("Ham").transform;
            jaw.SetParent(root.transform, false);
            jaw.localPosition = new Vector3(0f, 0.2f, -0.3f) * s;
            lion.Part(PrimitiveType.Cube, jaw, new Vector3(0f, -0.05f, 0.35f) * s, new Vector3(0.7f, 0.14f, 0.65f) * s, gold, "HamDuoi");
            lion._jaw = jaw;
            // nén nhang ngậm trong miệng, chĩa ra trước
            Transform stick = lion.Part(PrimitiveType.Cylinder, jaw, new Vector3(0f, 0.05f, 0.85f) * s, new Vector3(0.03f, 0.25f, 0.03f) * s, new Color(0.6f, 0.2f, 0.1f), "Nhang");
            stick.localRotation = Quaternion.Euler(90f, 0f, 0f);
            lion._ember = lion.Part(PrimitiveType.Sphere, jaw, new Vector3(0f, 0.05f, 1.1f) * s, Vector3.one * 0.06f * s, new Color(1f, 0.5f, 0.1f), "Than");
            return lion;
        }

        private Transform Part(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Color c, string n)
        {
            GameObject g = GameObject.CreatePrimitive(type);
            g.name = n;
            Destroy(g.GetComponent<Collider>());
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            Material m = ChestFxFactory.CreateUnlit(c, false, "TrungHoaLion_" + n);
            _mats.Add(m);
            g.GetComponent<Renderer>().sharedMaterial = m;
            return g.transform;
        }

        /// <summary>Nhảy (cung parabol) tới điểm mới, mặt hướng về <paramref name="lookAt"/>.</summary>
        public void HopTo(Vector3 target, float height, float duration, Vector3 lookAt)
        {
            if (_ragdoll) return;
            _danceT = -1f;
            _from = transform.position;
            _to = target;
            _hopHeight = height;
            _hopDur = Mathf.Max(0.05f, duration);
            _hopT = 0f;
            Vector3 d = lookAt - target; d.y = 0f;
            if (d.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(d.normalized, Vector3.up);
            _jawTarget = 0.6f;
        }

        public void Startle(Vector3 awayFrom, float distance, float height)
        {
            if (_ragdoll || Hopping) return;
            Vector3 away = transform.position - awayFrom; away.y = 0f;
            if (away.sqrMagnitude < 1e-6f) away = -transform.forward;
            HopTo(transform.position + away.normalized * distance, height, 0.28f, awayFrom);
        }

        public void Dance(Vector3 center, float radius)
        {
            if (_ragdoll) return;
            _danceCenter = center;
            _danceRadius = radius;
            _danceT = 0f;
            _hopT = 1f;
        }

        public void Chomp(float open) => _jawTarget = open;

        /// <summary>Hắt xì rồi đổ kềnh: bật Rigidbody, đẩy ngửa ra sau (ngã bằng vật lý thật).</summary>
        public void SneezeAndFall()
        {
            if (_ragdoll) return;
            _ragdoll = true;
            _danceT = -1f;
            _hopT = 1f;
            _jawOpen = 1f;
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.5f, 0f) * _size;
            col.size = new Vector3(0.8f, 0.7f, 0.7f) * _size;
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.AddForce(-transform.forward * 0.25f + Vector3.up * 0.4f, ForceMode.Impulse);
            rb.angularVelocity = transform.right * -7f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (_hopT < 1f)
            {
                _hopT = Mathf.Min(1f, _hopT + dt / _hopDur);
                Vector3 p = Vector3.Lerp(_from, _to, _hopT);
                p.y += 4f * _hopHeight * _hopT * (1f - _hopT);
                transform.position = p;
                if (_hopT >= 1f) _jawTarget = 0f;
            }
            else if (_danceT >= 0f)
            {
                _danceT += dt;
                float a = _danceT * 2.4f;
                Vector3 p = _danceCenter + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * _danceRadius;
                p.y += Mathf.Abs(Mathf.Sin(_danceT * 9f)) * _danceRadius * 0.25f;
                transform.position = p;
                Vector3 tangent = new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a));
                transform.rotation = Quaternion.LookRotation(tangent, Vector3.up) * Quaternion.Euler(0f, 0f, Mathf.Sin(_danceT * 6f) * 15f);
                _jawTarget = 0.5f + 0.5f * Mathf.Sin(_danceT * 12f);
            }
            if (_jaw != null)
            {
                _jawOpen = Mathf.MoveTowards(_jawOpen, _jawTarget, dt * 6f);
                _jaw.localRotation = Quaternion.Euler(_jawOpen * 35f, 0f, 0f);
            }
        }

        private void OnDestroy()
        {
            foreach (Material m in _mats) if (m != null) Destroy(m);
            _mats.Clear();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Dây pháo Tết quấn xoắn ốc quanh chân Chest_TrungHoa: một sợi ngòi (LineRenderer) nối các quả pháo đỏ
    /// (khối trụ thật có Collider). Tia lửa ngòi chạy dọc dây; khi tới quả nào thì quả đó NỔ: bật Rigidbody,
    /// nhận xung lực văng ra + xoáy, rơi xuống đất va chạm thật. Quả cuối là "pháo đại" để dành cho lúc chốt bậc.
    /// Chỉ hình ảnh/vật lý trang trí; số quả, vị trí lấy từ hạt giống trang trí.
    /// </summary>
    public class TrungHoaFirecrackerString : MonoBehaviour
    {
        public class Cracker
        {
            public Transform transform;
            public float pathDistance;   // khoảng cách dọc ngòi từ đầu ngòi tới quả này
            public bool blasted;
        }

        private readonly List<Cracker> _crackers = new List<Cracker>();
        private readonly List<Vector3> _path = new List<Vector3>();
        private readonly List<float> _pathDist = new List<float>();
        private readonly List<Material> _mats = new List<Material>();
        private LineRenderer _fuse;
        private Transform _spark;
        private float _burnt;           // quãng ngòi đã cháy (m)
        private float _size;

        public IReadOnlyList<Cracker> Crackers => _crackers;
        public Vector3 FuseHead => _path.Count > 0 ? _path[0] : transform.position;
        public Vector3 SparkPosition => _spark != null ? _spark.position : FuseHead;
        public float Burnt => _burnt;
        public float TotalLength => _pathDist.Count > 0 ? _pathDist[_pathDist.Count - 1] : 0f;

        /// <summary>Dựng dây: <paramref name="count"/> quả trên vòng xoắn 1,25 vòng quanh <paramref name="center"/> (mặt đất).</summary>
        public static TrungHoaFirecrackerString Build(Vector3 center, Vector3 front, float radius, int count, float size, System.Random rng, Transform parent)
        {
            GameObject root = new GameObject("TrungHoa_DayPhao");
            root.transform.SetParent(parent, false);
            root.transform.position = center;
            TrungHoaFirecrackerString s = root.AddComponent<TrungHoaFirecrackerString>();
            s._size = size;
            Material red = s.Mat(new Color(0.85f, 0.08f, 0.06f), "TrungHoaCracker");
            Material gold = s.Mat(new Color(1f, 0.8f, 0.25f), "TrungHoaCrackerBand");
            Material bigRed = s.Mat(new Color(1f, 0.15f, 0.1f), "TrungHoaCrackerBig");

            front.y = 0f;
            if (front.sqrMagnitude < 1e-6f) front = Vector3.forward;
            float startAngle = Mathf.Atan2(front.z, front.x);
            float lift = size * 0.012f;

            // đầu ngòi thò ra trước rương một đoạn để người chơi/sư tử châm
            Vector3 headDir = new Vector3(Mathf.Cos(startAngle), 0f, Mathf.Sin(startAngle));
            s.AddPathPoint(center + headDir * (radius * 1.45f) + Vector3.up * lift);

            const float turns = 1.25f;
            int steps = count * 3;
            int nextCracker = 1;
            for (int i = 0; i <= steps; i++)
            {
                float u = i / (float)steps;
                float a = startAngle + u * turns * Mathf.PI * 2f;
                float r = radius * Mathf.Lerp(1.15f, 0.92f, u);
                float jitter = ((float)rng.NextDouble() - 0.5f) * size * 0.02f;
                Vector3 p = center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r + jitter) + Vector3.up * lift;
                s.AddPathPoint(p);
                if ((i == nextCracker * 3 - 1 && s._crackers.Count < count - 1) || i == steps)
                {
                    bool big = i == steps;
                    Vector3 tangent = new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a));
                    s.AddCracker(p, tangent, big ? bigRed : red, gold, big);
                    nextCracker++;
                    if (s._crackers.Count >= count) break;
                }
            }

            s._fuse = root.AddComponent<LineRenderer>();
            s._fuse.useWorldSpace = true;
            s._fuse.widthMultiplier = size * 0.008f;
            s._fuse.positionCount = s._path.Count;
            s._fuse.SetPositions(s._path.ToArray());
            s._fuse.sharedMaterial = s.Mat(new Color(0.25f, 0.18f, 0.1f), "TrungHoaFuse");
            s._fuse.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "TiaLuaNgoi";
            Destroy(spark.GetComponent<Collider>());
            spark.transform.SetParent(root.transform, false);
            spark.transform.localScale = Vector3.one * size * 0.035f;
            spark.GetComponent<Renderer>().sharedMaterial = s.Mat(new Color(1f, 0.85f, 0.4f), "TrungHoaSpark");
            spark.SetActive(false);
            s._spark = spark.transform;
            s.UpdateSpark();
            return s;
        }

        private void AddPathPoint(Vector3 p)
        {
            float d = _path.Count == 0 ? 0f : _pathDist[_pathDist.Count - 1] + Vector3.Distance(_path[_path.Count - 1], p);
            _path.Add(p);
            _pathDist.Add(d);
        }

        private void AddCracker(Vector3 p, Vector3 tangent, Material body, Material band, bool big)
        {
            float len = _size * (big ? 0.16f : 0.09f), thick = _size * (big ? 0.05f : 0.028f);
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);  // giữ CapsuleCollider của primitive để va chạm thật
            c.name = big ? "PhaoDai" : "Phao";
            c.transform.SetParent(transform, true);
            c.transform.position = p + Vector3.up * thick * 0.5f;
            c.transform.rotation = Quaternion.FromToRotation(Vector3.up, tangent);
            c.transform.localScale = new Vector3(thick, len * 0.5f, thick);
            c.GetComponent<Renderer>().sharedMaterial = body;
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(b.GetComponent<Collider>());
            b.transform.SetParent(c.transform, false);
            b.transform.localScale = new Vector3(1.05f, 0.12f, 1.05f);
            b.GetComponent<Renderer>().sharedMaterial = band;
            _crackers.Add(new Cracker { transform = c.transform, pathDistance = _pathDist[_pathDist.Count - 1] });
        }

        private Material Mat(Color c, string n)
        {
            Material m = ChestFxFactory.CreateUnlit(c, false, n);
            _mats.Add(m);
            return m;
        }

        public void ShowSpark(bool on) { if (_spark != null) _spark.gameObject.SetActive(on); }

        /// <summary>Đốt ngòi thêm một đoạn (m). Trả về chỉ số quả pháo vừa bị tia lửa chạm tới (hoặc -1).</summary>
        public int Burn(float metres)
        {
            _burnt = Mathf.Min(TotalLength, _burnt + Mathf.Max(0f, metres));
            UpdateSpark();
            return NextReached();
        }

        /// <summary>Tia lửa nhảy thẳng tới quả chưa nổ kế tiếp (dùng khi vòng quay "tích" một nhịp).</summary>
        public int JumpToNext(bool includeLast)
        {
            int i = FirstUnblasted();
            if (i < 0 || (!includeLast && i == _crackers.Count - 1)) return -1;
            _burnt = Mathf.Max(_burnt, _crackers[i].pathDistance);
            UpdateSpark();
            return i;
        }

        public int FirstUnblasted()
        {
            for (int i = 0; i < _crackers.Count; i++) if (!_crackers[i].blasted) return i;
            return -1;
        }

        public int RemainingSmall()
        {
            int n = 0;
            for (int i = 0; i < _crackers.Count - 1; i++) if (!_crackers[i].blasted) n++;
            return n;
        }

        private int NextReached()
        {
            int i = FirstUnblasted();
            return i >= 0 && _burnt >= _crackers[i].pathDistance ? i : -1;
        }

        /// <summary>Quả pháo nổ: bật Rigidbody, xung lực ra xa tâm + lên trên, xoáy ngẫu nhiên (theo hạt giống).</summary>
        public Vector3 Blast(int index, Vector3 chestCenter, float strength, System.Random rng, bool dud)
        {
            Cracker c = _crackers[index];
            c.blasted = true;
            Transform t = c.transform;
            Vector3 pos = t.position;
            if (dud)
            {
                // tịt ngòi: chỉ lăn nhẹ một cái, thân pháo xám lại
                Rigidbody r0 = t.gameObject.AddComponent<Rigidbody>();
                r0.mass = 0.02f;
                r0.AddForce((pos - chestCenter).normalized * 0.004f, ForceMode.Impulse);
                Renderer rend = t.GetComponent<Renderer>();
                rend.sharedMaterial = Mat(new Color(0.35f, 0.3f, 0.28f), "TrungHoaDud");
                return pos;
            }
            Rigidbody rb = t.gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.01f;
            rb.linearDamping = 0.3f;
            Vector3 outward = pos - chestCenter; outward.y = 0f;
            outward = outward.sqrMagnitude > 1e-6f ? outward.normalized : Vector3.forward;
            float up = 0.6f + (float)rng.NextDouble() * 0.6f;
            rb.AddForce((outward * (0.5f + (float)rng.NextDouble() * 0.5f) + Vector3.up * up) * strength * 0.02f, ForceMode.Impulse);
            rb.angularVelocity = new Vector3((float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f) * 40f;
            // vỏ pháo cháy đen, tự dọn sau vài giây
            t.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.2f, 0.08f, 0.06f), "TrungHoaShell");
            Destroy(t.gameObject, 7f + (float)rng.NextDouble() * 2f);
            return pos;
        }

        private void UpdateSpark()
        {
            if (_spark == null || _path.Count == 0) return;
            int k = 1;
            while (k < _pathDist.Count - 1 && _pathDist[k] < _burnt) k++;
            float seg = Mathf.Max(1e-5f, _pathDist[k] - _pathDist[k - 1]);
            _spark.position = Vector3.Lerp(_path[k - 1], _path[k], Mathf.Clamp01((_burnt - _pathDist[k - 1]) / seg));
            // phần ngòi đã cháy biến mất: vẽ lại từ tia lửa trở đi
            if (_fuse != null)
            {
                int remaining = _path.Count - k;
                _fuse.positionCount = remaining + 1;
                _fuse.SetPosition(0, _spark.position);
                for (int i = 0; i < remaining; i++) _fuse.SetPosition(i + 1, _path[k + i]);
            }
        }

        private void OnDestroy()
        {
            foreach (Material m in _mats) if (m != null) Destroy(m);
            _mats.Clear();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật dâng cho kiếm chém (quả hồng, lá...). Chỉ vật có thành phần này mới bị kiếm chém đôi và cho "khí"
    /// (năng lượng trang trí). Vật khác của người chơi không bao giờ bị phá. Gắn lên bất kỳ vật có Rigidbody nào.
    /// </summary>
    public class NhatBanOffering : MonoBehaviour
    {
        public static readonly List<NhatBanOffering> All = new List<NhatBanOffering>();
        [Tooltip("Khí cộng thêm khi bị chém (quả = 1, lá = 1).")] public int energy = 1;
        [HideInInspector] public bool cut;

        private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        private void OnDisable() { All.Remove(this); }
    }

    /// <summary>
    /// Đạo cụ dựng bằng mã cho Chest_NhatBan (không cần mô hình ngoài): katana trong vỏ, nón lá samurai, chiếc dép,
    /// quả hồng. Cộng thêm bộ cắt lưới đôi (theo mặt phẳng) để dây thừng / giấy niêm / quả đứt thành 2 nửa vật lý.
    /// </summary>
    public static class NhatBanProps
    {
        public class Katana
        {
            public Transform root;      // gốc ở chuôi-tsuba; trục +X = hướng lưỡi
            public Transform blade;     // trượt dọc +X khi rút / nhập vỏ
            public Transform saya;      // vỏ
            public float length;
        }

        // ------------------------------------------------------------------ vật liệu

        /// <summary>Vật liệu có ánh sáng (URP Lit, rồi Standard); không có thì dùng Unlit của lõi.</summary>
        public static Material Lit(Color c, string name, List<Object> owned)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            Material m = s != null ? new Material(s) { name = name, hideFlags = HideFlags.DontSave } : ChestFxFactory.CreateUnlit(c, false, name);
            ChestFxFactory.SetColor(m, c);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.55f);
            owned.Add(m);
            return m;
        }

        private static Transform Part(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Quaternion rot, Material m)
        {
            GameObject g = GameObject.CreatePrimitive(type);
            g.name = name;
            Collider c = g.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localRotation = rot;
            g.transform.localScale = scale;
            Renderer r = g.GetComponent<Renderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return g.transform;
        }

        // ------------------------------------------------------------------ đạo cụ

        /// <summary>Katana dài <paramref name="length"/> (m), nằm dọc trục X cục bộ, chuôi ở -X.</summary>
        public static Katana BuildKatana(float length, List<Object> owned)
        {
            Material steel = Lit(new Color(0.86f, 0.88f, 0.92f), "NB_Steel", owned);
            Material black = Lit(new Color(0.06f, 0.05f, 0.06f), "NB_Lacquer", owned);
            Material gold = Lit(new Color(0.85f, 0.65f, 0.25f), "NB_Tsuba", owned);
            Material wrap = Lit(new Color(0.45f, 0.08f, 0.1f), "NB_Tsuka", owned);

            Katana k = new Katana { length = length };
            GameObject root = new GameObject("NB_Katana");
            root.hideFlags = HideFlags.DontSave;
            k.root = root.transform;
            float bl = length * 0.72f, th = length * 0.012f, w = length * 0.035f;
            Quaternion alongX = Quaternion.Euler(0f, 0f, 90f);

            // chuôi + tsuba (đứng yên ở gốc)
            Part(PrimitiveType.Cylinder, "Tsuka", k.root, new Vector3(-length * 0.13f, 0f, 0f), new Vector3(w * 0.9f, length * 0.13f, w * 0.9f), alongX, wrap);
            Part(PrimitiveType.Cylinder, "Tsuba", k.root, Vector3.zero, new Vector3(w * 2.4f, th * 0.6f, w * 2.4f), alongX, gold);

            // lưỡi (trượt ra khỏi vỏ)
            GameObject bladeGo = new GameObject("Blade");
            k.blade = bladeGo.transform;
            k.blade.SetParent(k.root, false);
            Part(PrimitiveType.Cube, "Edge", k.blade, new Vector3(bl * 0.5f, 0f, 0f), new Vector3(bl, w, th), Quaternion.identity, steel);
            Part(PrimitiveType.Cube, "Kissaki", k.blade, new Vector3(bl + w * 0.35f, -w * 0.12f, 0f), new Vector3(w * 0.8f, w * 0.75f, th), Quaternion.Euler(0f, 0f, 35f), steel);

            // vỏ
            k.saya = Part(PrimitiveType.Cylinder, "Saya", k.root, new Vector3(bl * 0.5f + th, 0f, 0f), new Vector3(w * 1.35f, bl * 0.52f, w * 1.1f), alongX, black);
            return k;
        }

        /// <summary>Nón lá samurai (jingasa): đĩa dẹt + chóp.</summary>
        public static Transform BuildHat(float radius, List<Object> owned)
        {
            Material straw = Lit(new Color(0.72f, 0.58f, 0.32f), "NB_Straw", owned);
            Material cord = Lit(new Color(0.55f, 0.1f, 0.1f), "NB_Cord", owned);
            GameObject root = new GameObject("NB_Kasa");
            root.hideFlags = HideFlags.DontSave;
            Part(PrimitiveType.Cylinder, "Brim", root.transform, Vector3.zero, new Vector3(radius * 2f, radius * 0.04f, radius * 2f), Quaternion.identity, straw);
            Part(PrimitiveType.Cylinder, "Mid", root.transform, Vector3.up * radius * 0.12f, new Vector3(radius * 1.3f, radius * 0.1f, radius * 1.3f), Quaternion.identity, straw);
            Part(PrimitiveType.Sphere, "Top", root.transform, Vector3.up * radius * 0.25f, new Vector3(radius * 0.6f, radius * 0.3f, radius * 0.6f), Quaternion.identity, straw);
            Part(PrimitiveType.Cylinder, "Band", root.transform, Vector3.up * radius * 0.06f, new Vector3(radius * 1.35f, radius * 0.03f, radius * 1.35f), Quaternion.identity, cord);
            return root.transform;
        }

        /// <summary>Chiếc dép tổ ong / xỏ ngón (đế + quai), có Rigidbody + BoxCollider.</summary>
        public static Rigidbody BuildSandal(float length, List<Object> owned)
        {
            Material sole = Lit(new Color(0.15f, 0.45f, 0.9f), "NB_SandalSole", owned);
            Material strap = Lit(new Color(0.95f, 0.95f, 0.95f), "NB_SandalStrap", owned);
            GameObject root = new GameObject("NB_Dep");
            root.hideFlags = HideFlags.DontSave;
            Part(PrimitiveType.Cube, "Sole", root.transform, Vector3.zero, new Vector3(length * 0.4f, length * 0.06f, length), Quaternion.identity, sole);
            Part(PrimitiveType.Cube, "StrapL", root.transform, new Vector3(-length * 0.1f, length * 0.06f, length * 0.12f), new Vector3(length * 0.04f, length * 0.04f, length * 0.4f), Quaternion.Euler(0f, 25f, 0f), strap);
            Part(PrimitiveType.Cube, "StrapR", root.transform, new Vector3(length * 0.1f, length * 0.06f, length * 0.12f), new Vector3(length * 0.04f, length * 0.04f, length * 0.4f), Quaternion.Euler(0f, -25f, 0f), strap);
            BoxCollider bc = root.AddComponent<BoxCollider>();
            bc.size = new Vector3(length * 0.4f, length * 0.12f, length);
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = 0.2f;
            return rb;
        }

        /// <summary>Quả hồng (kaki) có cuống lá, Rigidbody, gắn <see cref="NhatBanOffering"/>.</summary>
        public static Rigidbody BuildPersimmon(float radius, List<Object> owned)
        {
            Material fruit = Lit(new Color(1f, 0.45f, 0.08f), "NB_Kaki", owned);
            Material leaf = Lit(new Color(0.2f, 0.45f, 0.15f), "NB_KakiLeaf", owned);
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.name = "NB_Kaki";
            g.hideFlags = HideFlags.DontSave;
            g.transform.localScale = new Vector3(radius * 2f, radius * 1.6f, radius * 2f);
            g.GetComponent<Renderer>().sharedMaterial = fruit;
            Part(PrimitiveType.Cylinder, "Calyx", g.transform, Vector3.up * 0.5f, new Vector3(0.5f, 0.04f, 0.5f), Quaternion.identity, leaf);
            Rigidbody rb = g.AddComponent<Rigidbody>();
            rb.mass = 0.15f;
            g.AddComponent<NhatBanOffering>();
            return rb;
        }

        // ------------------------------------------------------------------ cắt đôi

        /// <summary>
        /// Cắt mọi lưới dưới <paramref name="source"/> theo mặt phẳng (thế giới) thành 2 vật vật lý (Rigidbody + BoxCollider,
        /// đang kinematic: gọi <see cref="Release"/> để rơi). Tam giác chia theo trọng tâm (mép cắt răng cưa nhẹ, chấp nhận).
        /// Lưới không đọc được (isReadable = false): sao nguyên lưới cho cả 2 nửa, mỗi nửa ép dẹt về phía mình.
        /// Không đụng tới <paramref name="source"/> (người gọi tự ẩn renderer).
        /// </summary>
        public static Rigidbody[] SliceToPieces(Transform source, Vector3 planePoint, Vector3 planeNormal, List<Object> owned)
        {
            GameObject a = NewPiece(source.name + "_A");
            GameObject b = NewPiece(source.name + "_B");
            Bounds ba = new Bounds(), bb = new Bounds();
            bool hasA = false, hasB = false;

            foreach (MeshFilter mf in source.GetComponentsInChildren<MeshFilter>())
            {
                Renderer r = mf.GetComponent<Renderer>();
                if (mf.sharedMesh == null || r == null || !r.enabled) continue;
                Mesh[] halves = SliceMesh(mf.sharedMesh, mf.transform, planePoint, planeNormal);
                for (int side = 0; side < 2; side++)
                {
                    Mesh m = halves[side];
                    if (m == null) continue;
                    if (m != mf.sharedMesh) owned.Add(m);   // không bao giờ huỷ lưới gốc của rương
                    GameObject host = side == 0 ? a : b;
                    GameObject g = new GameObject(mf.name);
                    g.transform.SetParent(host.transform, false);
                    g.transform.SetPositionAndRotation(mf.transform.position, mf.transform.rotation);
                    g.transform.localScale = mf.transform.lossyScale;
                    g.AddComponent<MeshFilter>().sharedMesh = m;
                    MeshRenderer mr = g.AddComponent<MeshRenderer>();
                    mr.sharedMaterials = r.sharedMaterials;
                    if (!mf.sharedMesh.isReadable)
                    {
                        // dự phòng: ép dẹt nửa này về phía của nó
                        g.transform.localScale = Vector3.Scale(g.transform.localScale, new Vector3(1f, 0.5f, 1f));
                        g.transform.position += planeNormal.normalized * (side == 0 ? 1f : -1f) * r.bounds.extents.y * 0.5f;
                    }
                    Bounds wb = new Bounds(g.transform.TransformPoint(m.bounds.center), Vector3.Scale(m.bounds.size, g.transform.lossyScale));
                    if (side == 0) { if (!hasA) { ba = wb; hasA = true; } else ba.Encapsulate(wb); }
                    else { if (!hasB) { bb = wb; hasB = true; } else bb.Encapsulate(wb); }
                }
            }

            Rigidbody ra = hasA ? Physicalize(a, ba) : Kill(a);
            Rigidbody rb = hasB ? Physicalize(b, bb) : Kill(b);
            return new[] { ra, rb };
        }

        public static void Release(Rigidbody rb, Vector3 impulse, Vector3 spin)
        {
            if (rb == null) return;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(impulse, ForceMode.Impulse);
            rb.angularVelocity = spin;
        }

        private static GameObject NewPiece(string name)
        {
            GameObject g = new GameObject("NB_Piece_" + name);
            g.hideFlags = HideFlags.DontSave;
            return g;
        }

        private static Rigidbody Kill(GameObject g)
        {
            Object.Destroy(g);
            return null;
        }

        private static Rigidbody Physicalize(GameObject g, Bounds worldBounds)
        {
            // Đưa gốc về tâm khối để vật xoay quanh trọng tâm.
            Vector3 shift = worldBounds.center - g.transform.position;
            foreach (Transform c in g.transform) c.position -= shift;
            g.transform.position = worldBounds.center;
            BoxCollider bc = g.AddComponent<BoxCollider>();
            bc.size = Vector3.Max(worldBounds.size, Vector3.one * 0.01f);
            Rigidbody rb = g.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.isKinematic = true;
            rb.useGravity = false;
            return rb;
        }

        /// <summary>Chia tam giác theo phía của trọng tâm so với mặt phẳng. Trả [phía +, phía -].</summary>
        private static Mesh[] SliceMesh(Mesh src, Transform t, Vector3 planePoint, Vector3 planeNormal)
        {
            if (!src.isReadable) return new[] { src, src };
            Vector3 p = t.InverseTransformPoint(planePoint);
            Vector3 n = t.InverseTransformDirection(planeNormal);
            Vector3[] v = src.vertices;
            Vector3[] nr = src.normals;
            Vector2[] uv = src.uv;
            Mesh[] result = new Mesh[2];
            for (int side = 0; side < 2; side++)
            {
                List<Vector3> pv = new List<Vector3>();
                List<Vector3> pn = new List<Vector3>();
                List<Vector2> pu = new List<Vector2>();
                Dictionary<int, int> map = new Dictionary<int, int>();
                List<int>[] subs = new List<int>[src.subMeshCount];
                bool any = false;
                for (int s = 0; s < src.subMeshCount; s++)
                {
                    subs[s] = new List<int>();
                    int[] tri = src.GetTriangles(s);
                    for (int i = 0; i + 2 < tri.Length; i += 3)
                    {
                        Vector3 c = (v[tri[i]] + v[tri[i + 1]] + v[tri[i + 2]]) / 3f;
                        bool positive = Vector3.Dot(c - p, n) >= 0f;
                        if (positive != (side == 0)) continue;
                        for (int k = 0; k < 3; k++)
                        {
                            int idx = tri[i + k];
                            if (!map.TryGetValue(idx, out int ni))
                            {
                                ni = pv.Count;
                                map[idx] = ni;
                                pv.Add(v[idx]);
                                if (nr.Length == v.Length) pn.Add(nr[idx]);
                                if (uv.Length == v.Length) pu.Add(uv[idx]);
                            }
                            subs[s].Add(ni);
                        }
                        any = true;
                    }
                }
                if (!any) continue;
                Mesh m = new Mesh { name = src.name + (side == 0 ? "_A" : "_B"), hideFlags = HideFlags.DontSave };
                m.SetVertices(pv);
                if (pn.Count == pv.Count) m.SetNormals(pn);
                if (pu.Count == pv.Count) m.SetUVs(0, pu);
                m.subMeshCount = subs.Length;
                for (int s = 0; s < subs.Length; s++) m.SetTriangles(subs[s], s);
                m.RecalculateBounds();
                result[side] = m;
            }
            return result;
        }
    }
}

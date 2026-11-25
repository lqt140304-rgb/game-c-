using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Đồ nghề cho MỘT lần chạy vật mở khoá (kế hoạch 2026-11-26, mục 4.1). Do <see cref="ChestKeyHookBase"/> tạo mỗi lần
    /// chạy chìa và Dispose khi xong/Reset. Vật liệu tạo qua <see cref="MakeMat"/> được dọn tự động.
    /// </summary>
    public sealed class KeyContext
    {
        /// <summary>Cỡ rương (cạnh lớn nhất), đơn vị thế giới lúc phát (đã tính thu nhỏ khi cầm trên tay).</summary>
        public float S;
        /// <summary>Chiều cao THÂN rương (thế giới, lúc phát).</summary>
        public float H;
        /// <summary>Hướng trước của rương (đơn vị, thế giới, vuông góc với <see cref="up"/>).</summary>
        public Vector3 front;
        /// <summary>Hướng lên của rương (thế giới). Khác Vector3.up khi rương đang cầm nghiêng.</summary>
        public Vector3 up;
        /// <summary>Điểm khe: sát mặt trước THÂN rương, giữa, cao <see cref="SlotHeight01"/>·H từ đáy (+ slotOffset).</summary>
        public Vector3 slotPos;
        /// <summary>Điểm giữa đáy thân rương (thế giới).</summary>
        public Vector3 chestBase;
        public Transform chest;
        public ChestScriptContext Script;

        // --- Do ChestKeyHookBase điền trước khi bay, cho các móc theme (ThemeFly/ThemeAction/OnKeyImpact) dùng.
        /// <summary>Preset của rương đang mở.</summary>
        public KeyPreset preset;
        /// <summary>Cỡ vật (keyScale·S), mét. Vật vừa hộp 1x1x1 nhân cỡ này.</summary>
        public float keySize;
        /// <summary>Nửa cỡ vật (0,5·keySize): gốc vật phải lùi ra trước khe chừng ấy để ĐẦU chạm đúng khe.</summary>
        public float tip;
        /// <summary>Điểm khởi bay cố định trong thế giới (trước rương 1,2 S, cao 0,6 H).</summary>
        public Vector3 startPos;
        /// <summary>Hướng chuẩn của vật: +Z quay vào rương, +Y theo trục lên của rương.</summary>
        public Quaternion Facing { get { return Quaternion.LookRotation(-front, up); } }
        /// <summary>Vị trí gốc vật khi ĐẦU vật nằm đúng khe (gọi Refresh trước nếu rương đang chuyển động).</summary>
        public Vector3 RestPos { get { return slotPos + front * tip; } }

        /// <summary>Độ cao khe tính từ đáy thân, theo H (gần ổ khoá).</summary>
        public const float SlotHeight01 = 0.42f;

        // Số đo trong hệ trục CỦA RƯƠNG (đo một lần lúc bắt đầu, nắp/hiệu ứng bị loại) để luôn bám rương dù rương di chuyển/xoay/thu nhỏ.
        private Vector3 _slotLocal, _baseLocal, _frontLocal, _upLocal;
        private Vector3 _sizeLocal;
        private Vector3 _offset;

        private readonly List<Material> _mats = new List<Material>();

        public KeyContext(ChestScriptContext script) : this(script, Vector3.zero) { }

        public KeyContext(ChestScriptContext script, Vector3 slotOffset)
        {
            Script = script;
            chest = script.Chest;
            ChoDoLa.Interaction.BoxOpenSequence box = script.Reveal != null ? script.Reveal.Box : null;
            Init(chest, box, slotOffset);
        }

        /// <summary>Đo không cần lần chạy (dùng cho Gizmo trong Editor).</summary>
        public KeyContext(Transform chestTr, ChoDoLa.Interaction.BoxOpenSequence box, Vector3 slotOffset)
        {
            chest = chestTr;
            Init(chestTr, box, slotOffset);
        }

        private static bool Excluded(Transform t, Transform chestTr)
        {
            // Loại: nắp (có thể đang mở), hiệu ứng ánh sáng, chìa/khoá có sẵn, niêm/dây buộc, đĩa, và mọi thứ gắn thêm khi chạy.
            for (Transform p = t; p != null && p != chestTr; p = p.parent)
            {
                string n = p.name;
                if (n == "Lid" || n == "Glow" || n == "Key" || n == "Seal" || n == "Binding" || n == "Record") return true;
                if (n.StartsWith("Key") || n.Contains("Fx") || n.Contains("FX") || n.Contains("Shadow") || n.Contains("Item")
                    || n.Contains("Particle") || n.Contains("Silhouette")) return true;
            }
            return false;
        }

        private void Init(Transform chestTr, ChoDoLa.Interaction.BoxOpenSequence box, Vector3 slotOffset)
        {
            _offset = slotOffset;

            // Hướng trước trong hệ trục rương (BoxOpenSequence.FrontDirection là hướng LOCAL), ép vuông góc trục lên local.
            _upLocal = Vector3.up;
            Vector3 f = box != null ? box.FrontDirection : Vector3.forward;
            if (box != null && box.transform != chestTr) f = chestTr.InverseTransformDirection(box.transform.TransformDirection(f));
            f.y = 0f;
            _frontLocal = f.sqrMagnitude > 1e-6f ? f.normalized : Vector3.forward;

            // Hộp bao THÂN rương trong hệ trục local (từ lưới, không phụ thuộc rương đang xoay/thu nhỏ khi cầm).
            bool any = false;
            Bounds b = new Bounds(Vector3.zero, Vector3.zero);
            Bounds all = new Bounds(Vector3.zero, Vector3.zero);
            bool anyAll = false;
            foreach (Renderer r in chestTr.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer || r is LineRenderer || r is TrailRenderer) continue;
                Bounds mb;
                SkinnedMeshRenderer smr = r as SkinnedMeshRenderer;
                if (smr != null) { if (smr.sharedMesh == null) continue; mb = smr.sharedMesh.bounds; }
                else
                {
                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    if (mf == null || mf.sharedMesh == null) continue;
                    mb = mf.sharedMesh.bounds;
                }
                bool ex = Excluded(r.transform, chestTr);
                for (int i = 0; i < 8; i++)
                {
                    Vector3 c = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 lp = chestTr.InverseTransformPoint(r.transform.TransformPoint(c));
                    if (!anyAll) { all = new Bounds(lp, Vector3.zero); anyAll = true; } else all.Encapsulate(lp);
                    if (ex) continue;
                    if (!any) { b = new Bounds(lp, Vector3.zero); any = true; } else b.Encapsulate(lp);
                }
            }
            if (!any) b = anyAll ? all : new Bounds(new Vector3(0f, 0.2f, 0f), Vector3.one * 0.4f);

            _sizeLocal = b.size;
            float halfAlong = Mathf.Abs(_frontLocal.x) * b.extents.x + Mathf.Abs(_frontLocal.z) * b.extents.z;
            _baseLocal = new Vector3(b.center.x, b.min.y, b.center.z);
            _slotLocal = _baseLocal + Vector3.up * (SlotHeight01 * b.size.y) + _frontLocal * halfAlong;
            Refresh();
        }

        /// <summary>Tính lại S/H/front/up/slotPos theo vị trí, xoay, cỡ THỰC TẾ của rương ở khung này.</summary>
        public void Refresh()
        {
            if (chest == null) return;
            Vector3 ls = chest.lossyScale;
            Vector3 sz = new Vector3(Mathf.Abs(_sizeLocal.x * ls.x), Mathf.Abs(_sizeLocal.y * ls.y), Mathf.Abs(_sizeLocal.z * ls.z));
            H = Mathf.Max(0.02f, sz.y);
            S = Mathf.Max(0.02f, Mathf.Max(sz.x, Mathf.Max(sz.y, sz.z)));
            up = chest.TransformDirection(_upLocal).normalized;
            Vector3 f = chest.TransformDirection(_frontLocal);
            f -= up * Vector3.Dot(f, up);
            front = f.sqrMagnitude > 1e-8f ? f.normalized : Vector3.forward;
            Vector3 right = Vector3.Cross(up, front);
            chestBase = chest.TransformPoint(_baseLocal);
            slotPos = chest.TransformPoint(_slotLocal) + right * _offset.x + up * _offset.y + front * _offset.z;
        }

        public void SetOffset(Vector3 slotOffset) { _offset = slotOffset; Refresh(); }

        public System.Random Rng(int salt) { return Script.RngFor(salt); }

        public Material MakeMat(Color color)
        {
            Material m = ChestFxFactory.CreateUnlit(color, false, "KeyMat");
            _mats.Add(m);
            return m;
        }

        public GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 localPos, Vector3 localScale, Quaternion localRot, Color color)
        {
            GameObject g = GameObject.CreatePrimitive(type);
            g.name = name;
            Collider col = g.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = localPos;
            g.transform.localRotation = localRot;
            g.transform.localScale = localScale;
            Renderer r = g.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = MakeMat(color);
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
            return g;
        }

        public GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 localPos, Vector3 localScale, Color color)
        {
            return Part(parent, type, name, localPos, localScale, Quaternion.identity, color);
        }

        public void Dispose()
        {
            foreach (Material m in _mats) if (m != null) Object.Destroy(m);
            _mats.Clear();
        }
    }
}

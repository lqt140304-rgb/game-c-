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
        /// <summary>Cỡ rương (cạnh lớn nhất).</summary>
        public float S;
        /// <summary>Chiều cao rương.</summary>
        public float H;
        /// <summary>Hướng trước của rương trên mặt phẳng ngang (đơn vị).</summary>
        public Vector3 front;
        /// <summary>Điểm khe: mặt trước rương, cao 0,4 H.</summary>
        public Vector3 slotPos;
        /// <summary>Điểm giữa rương trên mặt phẳng ngang, ở chân rương (y = đáy).</summary>
        public Vector3 chestBase;
        public Transform chest;
        public ChestScriptContext Script;

        private readonly List<Material> _mats = new List<Material>();

        public KeyContext(ChestScriptContext script)
        {
            Script = script;
            chest = script.Chest;
            S = Mathf.Max(0.05f, script.ChestSize);
            H = Mathf.Max(0.05f, script.ChestHeight);

            // Hướng trước: như TrungHoaHook/RacQuyHook (BoxOpenSequence.FrontDirection đổi sang thế giới, ép phẳng).
            ChoDoLa.Interaction.BoxOpenSequence box = script.Reveal != null ? script.Reveal.Box : null;
            Vector3 f = box != null ? chest.TransformDirection(box.FrontDirection) : chest.forward;
            f.y = 0f;
            front = f.sqrMagnitude > 1e-6f ? f.normalized : Vector3.forward;

            // Đo hộp bao của rương (bỏ hạt) để biết mặt trước ở đâu.
            Bounds b = new Bounds(chest.position, Vector3.zero);
            bool any = false;
            foreach (Renderer r in chest.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer || r is LineRenderer) continue;
                if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            }
            Vector3 center = any ? b.center : chest.position;
            float halfAlong = any ? Mathf.Abs(front.x) * b.extents.x + Mathf.Abs(front.z) * b.extents.z : S * 0.5f;
            float baseY = script.Mouth.y - H * 0.7f;
            chestBase = new Vector3(center.x, baseY, center.z);
            slotPos = new Vector3(center.x, baseY + 0.4f * H, center.z) + front * halfAlong;
        }

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

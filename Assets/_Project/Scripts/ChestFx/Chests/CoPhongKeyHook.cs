using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Cổ Phong: LÁ BÙA PHONG ẤN (giấy vàng dài, 3 nét chữ đỏ, tua đỏ). Mặt có chữ quay ra ngoài,
    /// mặt sau (+Z) hướng về rương để dán vào khe (hành động Strike). Kế hoạch 2026-11-26, mục 6.16.
    /// </summary>
    public class CoPhongKeyHook : ChestKeyHookBase
    {
        // Thanh thoát: Squash -10% (cả Rác lẫn các bậc khác), Jump +10%.
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_CoPhong", displayName = "CoPhong",
            audioFolder = "Assets/_Project/Audio/Chests/co_phong/", filePrefix = "sfx_cophong_",
            bedFile = "sfx_cophong_sword_hum_loop.mp3", keyFile = "sfx_cophong_bamboo_knock.mp3",
            mainColor = new Color(0.95f, 0.85f, 0.4f), openAction = KeyAction.Strike,
            squashMul = 0.9f, racSquashMul = 0.9f, jumpMul = 1.1f, cycleDelta = 0, shakeScale = 1f,
        };

        public override KeyPreset GetPreset() => Preset;

        // Các khối con cần cử động của từng lá bùa đang sống (theo root, không static).
        private class Parts { public Transform paper; public Transform[] tassels; }
        private readonly Dictionary<Transform, Parts> _parts = new Dictionary<Transform, Parts>();

        protected override GameObject BuildKey(KeyContext c)
        {
            Color paperCol = new Color(0.95f, 0.85f, 0.4f);
            Color red = new Color(0.8f, 0.1f, 0.1f);

            // Dọn các bản ghi của lá bùa đã bị huỷ.
            _parts.Clear();

            GameObject root = new GameObject("Key");
            Transform rt = root.transform;
            // Tờ giấy dài (dựng đứng, mỏng theo Z; +Z là mặt hướng về rương).
            GameObject paper = c.Part(rt, PrimitiveType.Cube, "Giay", new Vector3(0f, 0.08f, 0f), new Vector3(0.4f, 0.8f, 0.03f), paperCol);
            // Viền đỏ trên và dưới.
            c.Part(paper.transform, PrimitiveType.Cube, "VienTren", new Vector3(0f, 0.46f, -0.6f), new Vector3(1f, 0.08f, 1.4f), red);
            c.Part(paper.transform, PrimitiveType.Cube, "VienDuoi", new Vector3(0f, -0.46f, -0.6f), new Vector3(1f, 0.08f, 1.4f), red);
            // 3 nét chữ đỏ xếp dọc (mỏng, nổi ở mặt -Z, mặt nhìn thấy).
            c.Part(paper.transform, PrimitiveType.Cube, "Net1", new Vector3(0f, 0.26f, -0.75f), new Vector3(0.6f, 0.1f, 0.8f), red);
            c.Part(paper.transform, PrimitiveType.Cube, "Net2", new Vector3(0f, 0.0f, -0.75f), new Vector3(0.18f, 0.3f, 0.8f), red);
            c.Part(paper.transform, PrimitiveType.Cube, "Net3", new Vector3(0f, -0.26f, -0.75f), new Vector3(0.5f, 0.1f, 0.8f), red);

            // Tua đỏ: 3 sợi buông xuống dưới tờ giấy.
            Transform[] tassels = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject t = c.Part(rt, PrimitiveType.Cube, "Tua" + i, new Vector3(-0.12f + 0.12f * i, -0.42f, 0f), new Vector3(0.03f, 0.2f, 0.02f), red);
                tassels[i] = t.transform;
            }

            _parts[rt] = new Parts { paper = paper.transform, tassels = tassels };
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            Parts p;
            if (root == null || !_parts.TryGetValue(root, out p) || p.paper == null) return;
            float now = Time.unscaledTime;
            // Giấy phấp phới nhẹ khi bay; khi dán (hành động) thì phẳng lại.
            float flutter = InActionPhase ? 0f : 1f;
            p.paper.localRotation = Quaternion.Euler(0f, 10f * flutter * Mathf.Sin(now * 14f), 6f * flutter * Mathf.Sin(now * 9f));
            for (int i = 0; i < p.tassels.Length; i++)
            {
                if (p.tassels[i] == null) continue;
                float swing = 18f * Mathf.Sin(now * 11f + i * 1.3f) * flutter;
                // Xoay quanh đỉnh sợi tua: bù vị trí để đầu trên đứng yên.
                Vector3 top = new Vector3(-0.12f + 0.12f * i, -0.32f, 0f);
                Quaternion q = Quaternion.Euler(0f, 0f, swing);
                p.tassels[i].localRotation = q;
                p.tassels[i].localPosition = top + q * new Vector3(0f, -0.1f, 0f);
            }
        }
    }
}

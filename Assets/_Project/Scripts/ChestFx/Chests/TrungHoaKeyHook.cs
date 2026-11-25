using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Trung Hoa: ĐẦU LÂN nhỏ (đầu đỏ, hai mắt to, hàm dưới vàng, bờm tua).
    /// Mặt lân hướng +Z; hàm đóng/mở và bờm đung đưa khi bay và khi vồ (kế hoạch 2026-11-26, mục 6.14).
    /// </summary>
    public class TrungHoaKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_TrungHoa", displayName = "TrungHoa",
            audioFolder = "Assets/_Project/Audio/Chests/trung_hoa/", filePrefix = "sfx_trunghoa_",
            bedFile = "sfx_trunghoa_lion_drum_loop.mp3", keyFile = "sfx_trunghoa_thud.mp3",
            mainColor = new Color(0.85f, 0.1f, 0.1f), openAction = KeyAction.Pounce,
            squashMul = 1f, racSquashMul = 1f, jumpMul = 1f, cycleDelta = 1, shakeScale = 1f,
        };

        public override KeyPreset GetPreset() => Preset;

        sealed class Rig { public Transform jaw; public Transform[] mane; }
        // Các khối con cử động, theo root của từng vật (không dùng static).
        readonly Dictionary<Transform, Rig> _rigs = new Dictionary<Transform, Rig>();

        protected override GameObject BuildKey(KeyContext c)
        {
            Color red = Preset.mainColor;
            Color gold = new Color(1f, 0.8f, 0.15f);
            Color white = new Color(0.97f, 0.97f, 0.95f);
            Color black = new Color(0.05f, 0.05f, 0.05f);

            var root = new GameObject("TrungHoaLion");
            Transform t = root.transform;

            // Đầu đỏ + mõm nhô ra phía +Z
            c.Part(t, PrimitiveType.Cube, "Dau", new Vector3(0f, 0.12f, -0.05f), new Vector3(0.7f, 0.5f, 0.6f), red);
            c.Part(t, PrimitiveType.Cube, "Mom", new Vector3(0f, 0.05f, 0.3f), new Vector3(0.5f, 0.28f, 0.3f), red);
            c.Part(t, PrimitiveType.Sphere, "Mui", new Vector3(0f, 0.16f, 0.46f), new Vector3(0.14f, 0.12f, 0.1f), gold);
            // Hai mắt to: tròng trắng + con ngươi đen
            for (int i = -1; i <= 1; i += 2)
            {
                c.Part(t, PrimitiveType.Sphere, "Mat" + i, new Vector3(0.19f * i, 0.3f, 0.2f), new Vector3(0.22f, 0.22f, 0.12f), white);
                c.Part(t, PrimitiveType.Sphere, "Nguoi" + i, new Vector3(0.19f * i, 0.3f, 0.27f), new Vector3(0.1f, 0.1f, 0.06f), black);
            }
            c.Part(t, PrimitiveType.Cube, "Tran", new Vector3(0f, 0.43f, 0.05f), new Vector3(0.1f, 0.1f, 0.1f), gold);

            // Hàm dưới vàng, xoay quanh chốt ở sau mõm
            var jawPivot = new GameObject("HamPivot").transform;
            jawPivot.SetParent(t, false);
            jawPivot.localPosition = new Vector3(0f, -0.1f, 0.05f);
            c.Part(jawPivot, PrimitiveType.Cube, "Ham", new Vector3(0f, -0.04f, 0.25f), new Vector3(0.5f, 0.14f, 0.5f), gold);

            // Bờm tua: 5 dải sau đầu, vàng xen đỏ
            var mane = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                float a = (i - 2) * 0.32f;
                var p = new GameObject("BomPivot" + i).transform;
                p.SetParent(t, false);
                p.localPosition = new Vector3(Mathf.Sin(a) * 0.4f, 0.02f + Mathf.Cos(a) * 0.1f, -0.38f);
                c.Part(p, PrimitiveType.Cube, "Bom" + i, new Vector3(0f, -0.1f, -0.04f), new Vector3(0.1f, 0.34f, 0.06f), (i % 2 == 0) ? gold : red);
                mane[i] = p;
            }

            _rigs[t] = new Rig { jaw = jawPivot, mane = mane };
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            Rig r;
            if (root == null || !_rigs.TryGetValue(root, out r)) return;
            // Hàm ngáp: lúc bay mở khép chậm, lúc vồ cắn nhanh hơn.
            float k = InActionPhase ? 4f : 2f;
            float open = Mathf.Abs(Mathf.Sin(t01 * Mathf.PI * k));
            if (r.jaw != null) r.jaw.localRotation = Quaternion.Euler(open * 22f, 0f, 0f);
            for (int i = 0; i < r.mane.Length; i++)
                if (r.mane[i] != null)
                    r.mane[i].localRotation = Quaternion.Euler(Mathf.Sin(t01 * Mathf.PI * 6f + i) * 14f, 0f, 0f);
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            base.OnReset(reveal);
            _rigs.Clear();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Hải tặc: con vẹt hải tặc (thân đỏ, cánh xanh lá, mỏ vàng, mắt đen, đuôi dài).
    /// Đầu/mỏ hướng +Z; cánh vỗ khi bay. Hành động mở: Peck (mổ 2 lần). Kế hoạch 2026-11-26, mục 6.1.
    /// </summary>
    public class HaiTacKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_HaiTac", displayName = "HaiTac",
            audioFolder = "Assets/_Project/Audio/Chests/hai_tac/", filePrefix = "sfx_haitac_", bedFile = "sfx_haitac_bed_loop.mp3", keyFile = "sfx_haitac_thud.mp3",
            mainColor = new Color(0.85f, 0.12f, 0.1f), openAction = KeyAction.Peck,
            // Đàn hồi như sóng: Squash +10% (cả Rác lẫn các bậc khác).
            squashMul = 1.1f, racSquashMul = 1.1f, jumpMul = 1f, cycleDelta = 0, shakeScale = 1f,
        };

        // Hai trục cánh của vật (theo gốc vật, không static).
        private readonly Dictionary<Transform, Transform[]> _wings = new Dictionary<Transform, Transform[]>();

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color red = new Color(0.85f, 0.12f, 0.1f);
            Color green = new Color(0.15f, 0.7f, 0.25f);
            Color blue = new Color(0.15f, 0.4f, 0.85f);
            Color yellow = new Color(1f, 0.82f, 0.15f);
            Color black = new Color(0.05f, 0.05f, 0.05f);

            var root = new GameObject("HaiTacKey");
            Transform t = root.transform;

            // Thân đỏ, đầu đỏ ở phía +Z.
            c.Part(t, PrimitiveType.Sphere, "Than", new Vector3(0f, 0f, -0.05f), new Vector3(0.38f, 0.42f, 0.5f), red);
            c.Part(t, PrimitiveType.Sphere, "Dau", new Vector3(0f, 0.2f, 0.25f), new Vector3(0.28f, 0.28f, 0.28f), red);
            // Mỏ vàng (quặp xuống) + hai mắt đen.
            c.Part(t, PrimitiveType.Cube, "Mo", new Vector3(0f, 0.17f, 0.42f), new Vector3(0.09f, 0.12f, 0.16f), Quaternion.Euler(25f, 0f, 0f), yellow);
            c.Part(t, PrimitiveType.Sphere, "MatTrai", new Vector3(-0.11f, 0.26f, 0.35f), new Vector3(0.06f, 0.06f, 0.06f), black);
            c.Part(t, PrimitiveType.Sphere, "MatPhai", new Vector3(0.11f, 0.26f, 0.35f), new Vector3(0.06f, 0.06f, 0.06f), black);
            // Đuôi dài: một dải xanh lá + đầu đuôi xanh dương.
            c.Part(t, PrimitiveType.Cube, "Duoi", new Vector3(0f, -0.08f, -0.38f), new Vector3(0.1f, 0.05f, 0.34f), Quaternion.Euler(-12f, 0f, 0f), green);
            c.Part(t, PrimitiveType.Cube, "DuoiMut", new Vector3(0f, -0.12f, -0.52f), new Vector3(0.12f, 0.04f, 0.1f), Quaternion.Euler(-12f, 0f, 0f), blue);

            // Hai cánh xanh lá gắn vào trục (xoay quanh trục dọc thân khi vỗ).
            var pivotL = new GameObject("TrucCanhTrai").transform;
            pivotL.SetParent(t, false);
            pivotL.localPosition = new Vector3(-0.17f, 0.04f, -0.05f);
            var pivotR = new GameObject("TrucCanhPhai").transform;
            pivotR.SetParent(t, false);
            pivotR.localPosition = new Vector3(0.17f, 0.04f, -0.05f);
            c.Part(pivotL, PrimitiveType.Cube, "CanhTrai", new Vector3(-0.15f, 0f, 0f), new Vector3(0.3f, 0.04f, 0.34f), green);
            c.Part(pivotR, PrimitiveType.Cube, "CanhPhai", new Vector3(0.15f, 0f, 0f), new Vector3(0.3f, 0.04f, 0.34f), green);

            _wings[t] = new[] { pivotL, pivotR };
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            if (root == null) return;
            Transform[] w;
            if (!_wings.TryGetValue(root, out w) || w[0] == null || w[1] == null) return;
            // Bay: vỗ nhanh biên độ lớn. Hành động mổ: cánh run nhẹ.
            float freq = InActionPhase ? 10f : 6f;
            float amp = InActionPhase ? 10f : 40f;
            float a = Mathf.Sin(t01 * freq * Mathf.PI * 2f) * amp;
            w[0].localRotation = Quaternion.Euler(0f, 0f, a);
            w[1].localRotation = Quaternion.Euler(0f, 0f, -a);
        }
    }
}

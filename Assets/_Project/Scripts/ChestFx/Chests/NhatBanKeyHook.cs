using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Nhật Bản: một thanh katana (lưỡi bạc dẹt, tsuba đen, cán quấn đỏ), mũi kiếm hướng +Z.
    /// Hành động Strike; lúc chém kiếm xoay ngang (lăn quanh trục lưỡi) 90°. Điềm tĩnh: chu kỳ −1, Squash −10%.
    /// Kế hoạch 2026-11-26, mục 6.15.
    /// </summary>
    public class NhatBanKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = MakePreset();

        private static KeyPreset MakePreset()
        {
            KeyPreset p = KeyPreset.Neutral(
                "Chest_NhatBan", "NhatBan", "Assets/_Project/Audio/Chests/nhat_ban/", "sfx_nhatban_", "sfx_nhatban_shakuhachi_loop.mp3",
                new Color(0.82f, 0.86f, 0.9f), KeyAction.Strike);
            p.keyFile = "sfx_nhatban_sheath.mp3";
            p.squashMul = 0.9f;      // điềm tĩnh: Squash −10%
            p.cycleDelta = -1;       // chu kỳ −1 (lõi kẹp tối thiểu 1)
            return p;
        }

        // Nhóm khối kiếm (con của gốc) theo từng gốc vật; không dùng static.
        private readonly Dictionary<Transform, Transform> _blade = new Dictionary<Transform, Transform>();

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color silver = new Color(0.85f, 0.88f, 0.92f);
            Color black = new Color(0.07f, 0.07f, 0.08f);
            Color red = new Color(0.78f, 0.1f, 0.12f);
            Color darkRed = new Color(0.5f, 0.06f, 0.08f);

            GameObject root = new GameObject("Key");
            GameObject sword = new GameObject("Kiem");
            sword.transform.SetParent(root.transform, false);
            Transform t = sword.transform;

            // Lưỡi dài dẹt (z -0.16 .. 0.46) + mũi vát (xoay 45° quanh X, nhô tới 0.5).
            c.Part(t, PrimitiveType.Cube, "Luoi", new Vector3(0f, 0f, 0.15f), new Vector3(0.03f, 0.09f, 0.62f), Quaternion.identity, silver);
            c.Part(t, PrimitiveType.Cube, "Mui", new Vector3(0f, 0f, 0.46f), new Vector3(0.03f, 0.063f, 0.063f), Quaternion.Euler(45f, 0f, 0f), silver);
            // Tsuba tròn đen (đĩa dẹt vuông góc với lưỡi).
            c.Part(t, PrimitiveType.Cylinder, "Tsuba", new Vector3(0f, 0f, -0.17f), new Vector3(0.2f, 0.012f, 0.2f), Quaternion.Euler(90f, 0f, 0f), black);
            // Cán quấn đỏ + 3 dải quấn sẫm + đầu cán đen.
            c.Part(t, PrimitiveType.Cube, "Can", new Vector3(0f, 0f, -0.34f), new Vector3(0.07f, 0.07f, 0.28f), Quaternion.identity, red);
            c.Part(t, PrimitiveType.Cube, "Quan1", new Vector3(0f, 0f, -0.27f), new Vector3(0.078f, 0.078f, 0.02f), Quaternion.identity, darkRed);
            c.Part(t, PrimitiveType.Cube, "Quan2", new Vector3(0f, 0f, -0.34f), new Vector3(0.078f, 0.078f, 0.02f), Quaternion.identity, darkRed);
            c.Part(t, PrimitiveType.Cube, "Quan3", new Vector3(0f, 0f, -0.41f), new Vector3(0.078f, 0.078f, 0.02f), Quaternion.identity, darkRed);
            c.Part(t, PrimitiveType.Cube, "DauCan", new Vector3(0f, 0f, -0.49f), new Vector3(0.09f, 0.09f, 0.03f), Quaternion.identity, black);

            _blade[root.transform] = t;
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            Transform sword;
            if (root == null || !_blade.TryGetValue(root, out sword) || sword == null) return;
            if (InActionPhase)
            {
                // Chém: xoay ngang 90° quanh trục lưỡi trong 0,35 s hành động.
                float k = Mathf.Clamp01((t01 - 0.05f) / 0.6f);
                k = k * k * (3f - 2f * k);
                sword.localRotation = Quaternion.Euler(0f, 0f, 90f * k);
            }
            else
            {
                sword.localRotation = Quaternion.identity;
            }
        }

        private void OnDisable() { _blade.Clear(); }
    }
}

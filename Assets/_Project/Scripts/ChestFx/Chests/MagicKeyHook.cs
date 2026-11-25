using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Magic: tinh thể rune tím lơ lửng, có 3 mảnh nhỏ xoay quanh (kế hoạch 2026-11-26, mục 6.4).
    /// Hành động: Insert (cắm vào khe). Tính cách: lơ lửng nên Squash −10%.
    /// </summary>
    public class MagicKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = MakePreset();

        private static KeyPreset MakePreset()
        {
            KeyPreset p = KeyPreset.Neutral(
                "Chest_Magic", "Magic", "Assets/_Project/Audio/Chests/magic/", "sfx_magic_", "sfx_magic_rune_bed_loop.mp3",
                new Color(0.6f, 0.3f, 0.9f), KeyAction.Insert);
            p.keyFile = "sfx_magic_rune_stone.mp3";
            p.squashMul = 0.9f;
            p.racSquashMul = 0.9f;
            return p;
        }

        private const float OrbitRadius = 0.36f;
        private static readonly Color Shard = new Color(0.85f, 0.7f, 1f);
        private static readonly Color Core = new Color(0.95f, 0.85f, 1f);

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color main = Preset.mainColor;
            GameObject root = new GameObject("Key");
            Transform t = root.transform;
            // Tinh thể thoi dài theo Z (đầu nhọn về rương): hai khối dài xoay chéo nhau.
            c.Part(t, PrimitiveType.Cube, "TinhThe", Vector3.zero, new Vector3(0.24f, 0.24f, 0.7f), Quaternion.Euler(0f, 0f, 45f), main);
            c.Part(t, PrimitiveType.Cube, "TinhThe2", Vector3.zero, new Vector3(0.24f, 0.24f, 0.7f), Quaternion.identity, main * 0.85f);
            c.Part(t, PrimitiveType.Sphere, "Loi", Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f), Core);
            for (int i = 0; i < 3; i++)
            {
                c.Part(t, PrimitiveType.Cube, "Manh" + i, OrbitPos(i, 0f), new Vector3(0.12f, 0.12f, 0.12f), Quaternion.Euler(45f, 45f, 0f), Shard);
            }
            return root;
        }

        private static Vector3 OrbitPos(int i, float phase)
        {
            float a = phase + i * (Mathf.PI * 2f / 3f);
            return new Vector3(Mathf.Cos(a) * OrbitRadius, Mathf.Sin(a) * OrbitRadius, 0f);
        }

        /// <summary>3 mảnh xoay quanh tinh thể và tinh thể xoay chậm quanh trục Z; chỉ xoay/dời các khối con của chính vật.</summary>
        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            if (root == null) return;
            float now = Time.unscaledTime;
            float phase = now * (InActionPhase ? 9f : 5f);
            for (int i = 0; i < 3; i++)
            {
                Transform m = root.Find("Manh" + i);
                if (m != null)
                {
                    m.localPosition = OrbitPos(i, phase);
                    m.localRotation = Quaternion.Euler(now * 200f, now * 140f + i * 40f, 0f);
                }
            }
            Transform a = root.Find("TinhThe");
            Transform b = root.Find("TinhThe2");
            Quaternion spin = Quaternion.Euler(0f, 0f, now * 60f);
            if (a != null) a.localRotation = spin * Quaternion.Euler(0f, 0f, 45f);
            if (b != null) b.localRotation = spin;
        }
    }
}

using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương ThienCo: bánh răng đồng (đĩa trụ + 8 răng + lỗ trục), quay quanh trục Z khi bay và khi thả.
    /// Kế hoạch 2026-11-26, mục 6.17. Chưa chạy trong Unity.
    /// </summary>
    public class ThienCoKeyHook : ChestKeyHookBase
    {
        const int TeethCount = 8;

        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_ThienCo", displayName = "ThienCo",
            audioFolder = "Assets/_Project/Audio/Chests/thien_co/", filePrefix = "sfx_thienco_", bedFile = "sfx_thienco_ambient_clockwork.mp3", keyFile = "sfx_thienco_gear_clank.mp3",
            flyFile = "sfx_thienco_fly.mp3", actionFile = "sfx_thienco_action.mp3",
            lightColor = new Color(1f, 0.55f, 0.2f), lightMul = 0.6f, moveStyle = ChestMoveStyle.Spin,   // đồng cam; rương xoay như cơ cấu
            mainColor = new Color(0.8f, 0.5f, 0.25f), openAction = KeyAction.Drop,
            squashMul = 1f, racSquashMul = 1f, jumpMul = 1.2f, cycleDelta = 1, shakeScale = 1f,   // lò xo: Jump +20%, chu kỳ +1
        };

        public override KeyPreset GetPreset() => Preset;

        protected override GameObject BuildKey(KeyContext c)
        {
            Color bronze = Preset.mainColor;
            Color dark = new Color(0.18f, 0.12f, 0.08f);
            GameObject root = new GameObject("Key");
            Transform spin = new GameObject("Quay").transform;
            spin.SetParent(root.transform, false);
            Quaternion alongZ = Quaternion.Euler(90f, 0f, 0f);   // trụ nằm dọc Z: mặt đĩa hướng +Z
            c.Part(spin, PrimitiveType.Cylinder, "Dia", Vector3.zero, new Vector3(0.7f, 0.1f, 0.7f), alongZ, bronze);
            for (int i = 0; i < TeethCount; i++)
            {
                float a = i * Mathf.PI * 2f / TeethCount;
                Vector3 p = new Vector3(Mathf.Cos(a) * 0.4f, Mathf.Sin(a) * 0.4f, 0f);
                c.Part(spin, PrimitiveType.Cube, "Rang" + i, p, new Vector3(0.16f, 0.16f, 0.2f), Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg), bronze);
            }
            c.Part(spin, PrimitiveType.Cylinder, "LoTruc", new Vector3(0f, 0f, 0.02f), new Vector3(0.22f, 0.1f, 0.22f), alongZ, dark);
            return root;
        }

        static float Ease(float a, float b, float t) { float k = Mathf.Clamp01((t - a) / (b - a)); return k * k * (3f - 2f * k); }

        /// <summary>
        /// Bay: rơi xuống sàn trước rương, lăn trên vành về phía khe (góc quay = quãng lăn / bán kính), rồi bật lên khe,
        /// xoay mặt vào rương và quay tít dần.
        /// </summary>
        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            Transform spin = root.Find("Quay");
            float t = 0f, rolled = 0f, spinDeg = 0f;
            Vector3 prev = root.position;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / seconds);
                c.Refresh();
                float r = 0.45f * c.keySize;
                Vector3 side = Vector3.Cross(c.up, c.front).normalized;
                Vector3 groundRest = c.RestPos - c.up * Vector3.Dot(c.RestPos - c.chestBase, c.up) + c.up * r;
                Vector3 a = c.startPos - c.up * Vector3.Dot(c.startPos - c.chestBase, c.up) + c.up * r;
                Vector3 b = groundRest + c.front * (c.keySize * 1.3f);
                Quaternion edge = Quaternion.LookRotation(side, c.up);   // +Z ngang: đĩa đứng, lăn về rương
                Vector3 pos; Quaternion rot = edge;
                if (u < 0.15f)
                {
                    float k = u / 0.15f;
                    pos = Vector3.Lerp(c.startPos, a, k * k);   // rơi nhanh dần
                }
                else if (u < 0.65f)
                {
                    float k = Ease(0.15f, 0.65f, u);
                    float hop = Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2f)) * (1f - k) * 0.25f * c.keySize;   // nảy nhỏ khi lăn
                    pos = Vector3.Lerp(a, b, k) + c.up * hop;
                }
                else
                {
                    float k = Ease(0.65f, 1f, u);
                    pos = Vector3.Lerp(b, c.RestPos, k) + c.up * (Mathf.Sin(k * Mathf.PI) * 0.5f * c.keySize);
                    rot = Quaternion.Slerp(edge, c.Facing, k);
                }
                if (u < 0.65f) rolled += Vector3.Dot(pos - prev, c.front) * -1f / Mathf.Max(r, 1e-4f) * Mathf.Rad2Deg;
                else spinDeg += Time.unscaledDeltaTime * Mathf.Lerp(360f, 1440f, Ease(0.65f, 1f, u));   // quay tít
                prev = pos;
                root.position = pos;
                root.rotation = rot;
                if (spin != null) spin.localRotation = Quaternion.Euler(0f, 0f, rolled + spinDeg);
                yield return null;
            }
            c.Refresh();
            root.position = c.RestPos;
            root.rotation = c.Facing;
        }

        /// <summary>
        /// Mở: bánh răng hãm quay, ấn vào khe cho khớp răng, rồi nhích đúng một nấc (45°) "cạch" có giật nhẹ, xong thu nhỏ tại khe.
        /// </summary>
        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            Transform spin = root.Find("Quay");
            float startZ = spin != null ? spin.localEulerAngles.z : 0f;
            float settleZ = Mathf.Round(startZ / 45f) * 45f + 45f;   // dừng ở góc khớp răng
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / seconds);
                c.Refresh();
                ShrinkAtSlot(c, root, u);
                root.rotation = c.Facing;
                float press = 1f - Ease(0f, 0.14f, u);                       // ấn từ ngoài vào khe
                float jolt = u > 0.3f && u < 0.85f ? Mathf.Sin((u - 0.3f) / 0.55f * Mathf.PI * 3f) * (1f - (u - 0.3f) / 0.55f) : 0f;
                if (u < 0.85f) root.position += c.front * (c.keySize * (0.25f * press + 0.04f * jolt));
                if (spin != null)
                {
                    float z = Mathf.LerpAngle(startZ, settleZ, Ease(0f, 0.14f, u));
                    float notch = Ease(0.3f, 0.38f, u);                     // một nấc nhanh, gắt
                    float over = Mathf.Sin(Ease(0.38f, 0.6f, u) * Mathf.PI) * 6f;   // lố nhẹ rồi về
                    spin.localRotation = Quaternion.Euler(0f, 0f, z + 45f * notch + over);
                }
                yield return null;
            }
            ShrinkAtSlot(c, root, 1f);
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            Transform spin = root != null ? root.Find("Quay") : null;
            if (spin == null) return;
            float turns = InActionPhase ? 1f : 2f;   // bay: quay 2 vòng; thả: thêm 1 vòng
            spin.localRotation = Quaternion.Euler(0f, 0f, t01 * 360f * turns);
        }
    }
}

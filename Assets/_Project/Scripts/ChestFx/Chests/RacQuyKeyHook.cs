using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật mở khoá của rương Rác Quý: con mèo mướp cam (thân, đầu, 2 tai, đuôi, 2 mắt, 4 chân), đầu hướng +Z,
    /// đuôi vẫy khi bay và khi vồ. Kế hoạch 2026-11-26, mục 6.2. Không sinh thêm vật nào.
    /// Đợt 2: bay = nhảy cóc 4 bước (ngồi thụp lấy đà, bật lên, đáp); mở = vồ lên khe rồi cào hai nhát bằng 2 chân trước.
    /// Sáng cam ấm lúc vồ, nháy xanh lá nhạt ở nhát cào thứ hai. Cả rương lắc (Wobble) như thùng rác bị mèo lục.
    /// </summary>
    public class RacQuyKeyHook : ChestKeyHookBase
    {
        public static readonly KeyPreset Preset = new KeyPreset
        {
            chestId = "Chest_RacQuy", displayName = "RacQuy",
            audioFolder = "Assets/_Project/Audio/Chests/rac_quy/", filePrefix = "sfx_racquy_", bedFile = "sfx_racquy_bed_loop.mp3", keyFile = "sfx_racquy_clank.mp3",
            mainColor = new Color(1f, 0.6f, 0.2f), openAction = KeyAction.Pounce,
            squashMul = 1f, racSquashMul = 1.2f, jumpMul = 0.8f, cycleDelta = 0, shakeScale = 1f,
            flyFile = "sfx_racquy_fly.mp3", actionFile = "sfx_racquy_action.mp3",
            lightColor = new Color(1f, 0.62f, 0.25f), lightMul = 0.55f, moveStyle = ChestMoveStyle.Wobble,
        };

        private static readonly Color XanhLaNhat = new Color(0.6f, 0.95f, 0.45f);

        public override KeyPreset GetPreset() => Preset;

        // Đuôi của từng con mèo đang sống (theo gốc vật); không static.
        private readonly Dictionary<Transform, Transform> _tails = new Dictionary<Transform, Transform>();
        // 2 chân trước của từng con mèo (theo gốc vật), để cào; không static.
        private readonly Dictionary<Transform, Transform[]> _paws = new Dictionary<Transform, Transform[]>();

        protected override GameObject BuildKey(KeyContext c)
        {
            Color cam = Preset.mainColor;
            Color camDam = new Color(0.8f, 0.4f, 0.1f);
            Color mat = new Color(0.85f, 0.85f, 0.2f);
            Color hong = new Color(1f, 0.6f, 0.65f);

            GameObject root = new GameObject("Key_Meo");
            Transform t = root.transform;
            c.Part(t, PrimitiveType.Cube, "Than", new Vector3(0f, -0.05f, -0.12f), new Vector3(0.34f, 0.3f, 0.5f), cam);
            c.Part(t, PrimitiveType.Sphere, "Dau", new Vector3(0f, 0.08f, 0.3f), new Vector3(0.36f, 0.32f, 0.3f), cam);
            c.Part(t, PrimitiveType.Cube, "TaiTrai", new Vector3(-0.12f, 0.27f, 0.28f), new Vector3(0.1f, 0.14f, 0.06f), Quaternion.Euler(0f, 0f, 15f), camDam);
            c.Part(t, PrimitiveType.Cube, "TaiPhai", new Vector3(0.12f, 0.27f, 0.28f), new Vector3(0.1f, 0.14f, 0.06f), Quaternion.Euler(0f, 0f, -15f), camDam);
            c.Part(t, PrimitiveType.Sphere, "MatTrai", new Vector3(-0.08f, 0.12f, 0.44f), new Vector3(0.07f, 0.07f, 0.04f), mat);
            c.Part(t, PrimitiveType.Sphere, "MatPhai", new Vector3(0.08f, 0.12f, 0.44f), new Vector3(0.07f, 0.07f, 0.04f), mat);
            c.Part(t, PrimitiveType.Sphere, "Mui", new Vector3(0f, 0.04f, 0.46f), new Vector3(0.05f, 0.04f, 0.04f), hong);
            GameObject pawL = c.Part(t, PrimitiveType.Cube, "ChanTruocTrai", new Vector3(-0.1f, -0.25f, 0.06f), new Vector3(0.08f, 0.16f, 0.08f), camDam);
            GameObject pawR = c.Part(t, PrimitiveType.Cube, "ChanTruocPhai", new Vector3(0.1f, -0.25f, 0.06f), new Vector3(0.08f, 0.16f, 0.08f), camDam);
            _paws[t] = new[] { pawL.transform, pawR.transform };
            c.Part(t, PrimitiveType.Cube, "ChanSauTrai", new Vector3(-0.1f, -0.25f, -0.3f), new Vector3(0.08f, 0.16f, 0.08f), camDam);
            c.Part(t, PrimitiveType.Cube, "ChanSauPhai", new Vector3(0.1f, -0.25f, -0.3f), new Vector3(0.08f, 0.16f, 0.08f), camDam);

            // Đuôi: gốc xoay nằm ở mông, đuôi dựng cong lên sau lưng.
            GameObject pivot = new GameObject("DuoiGoc");
            pivot.transform.SetParent(t, false);
            pivot.transform.localPosition = new Vector3(0f, 0f, -0.38f);
            c.Part(pivot.transform, PrimitiveType.Cube, "Duoi1", new Vector3(0f, 0.05f, -0.06f), new Vector3(0.07f, 0.07f, 0.2f), Quaternion.Euler(-20f, 0f, 0f), cam);
            c.Part(pivot.transform, PrimitiveType.Cube, "Duoi2", new Vector3(0f, 0.2f, -0.14f), new Vector3(0.07f, 0.22f, 0.07f), camDam);
            _tails[t] = pivot.transform;
            return root;
        }

        protected override void AnimateAction(KeyContext c, Transform root, float t01)
        {
            Transform tail;
            if (root == null || !_tails.TryGetValue(root, out tail) || tail == null) return;
            float freq = InActionPhase ? 14f : 8f;
            float amp = InActionPhase ? 40f : 25f;
            tail.localRotation = Quaternion.Euler(0f, Mathf.Sin(t01 * freq) * amp, 0f);
        }

        // ------------------------------------------------------------------ đợt 2: bay nhảy cóc, vồ rồi cào

        private void WagTail(Transform root, float time, float freq, float amp)
        {
            Transform tail;
            if (_tails.TryGetValue(root, out tail) && tail != null)
                tail.localRotation = Quaternion.Euler(0f, Mathf.Sin(time * freq) * amp, 0f);
        }

        /// <summary>Điểm mèo ngồi chờ trước khi vồ: trước khe 0,15 S, thấp hơn khe 0,05 S (gốc vật).</summary>
        private static Vector3 CrouchPos(KeyContext c)
        {
            return c.RestPos + c.front * (0.15f * c.S) - c.up * (0.05f * c.S);
        }

        protected override IEnumerator ThemeFly(KeyContext c, Transform root, float seconds)
        {
            // 4 bước nhảy cóc: mỗi bước 30% ngồi thụp lấy đà (dẹt xuống), 70% bật vòng cung rồi đáp. Độ cao mỗi bước lệch nhẹ theo Rng.
            const int hops = 4;
            System.Random rng = c.Rng(2201);
            float[] hMul = new float[hops];
            for (int i = 0; i < hops; i++) hMul[i] = 0.85f + 0.3f * (float)rng.NextDouble();
            Vector3 p0 = c.startPos;
            float t0 = Time.unscaledTime;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                Vector3 p1 = CrouchPos(c);
                float seg = u * hops;
                int i = Mathf.Min(hops - 1, (int)seg);
                float k = u >= 1f ? 1f : seg - i;
                Vector3 a = Vector3.Lerp(p0, p1, (float)i / hops);
                Vector3 b = Vector3.Lerp(p0, p1, (float)(i + 1) / hops);
                Vector3 pos; float squash = 0f; float pitch = 0f;
                if (k < 0.3f)
                {
                    pos = a;
                    squash = Mathf.Sin(Mathf.PI * k / 0.3f);   // ngồi thụp rồi bung
                }
                else
                {
                    float j = (k - 0.3f) / 0.7f;
                    pos = Vector3.Lerp(a, b, j) + c.up * (4f * j * (1f - j) * 0.22f * c.S * hMul[i]);
                    pitch = Mathf.Lerp(-18f, 14f, j);             // ngóc đầu khi bật, chúi khi đáp
                }
                root.position = pos;
                root.rotation = c.Facing * Quaternion.Euler(pitch, 0f, 0f);
                float sz = c.keySize;
                root.localScale = new Vector3(sz * (1f + 0.12f * squash), sz * (1f - 0.22f * squash), sz);
                WagTail(root, u, 8f, 25f);
                if (u >= 1f) yield break;
                yield return null;
            }
        }

        protected override IEnumerator ThemeAction(KeyContext c, Transform root, float seconds)
        {
            // 0 → 0,3: vồ từ chỗ ngồi lên khe (vòng cung nhỏ, ngóc người, 2 chân trước giơ ra trước).
            // 0,3 → 0,85: cào 2 nhát (2 chân trước quét xuống, người giật nhẹ); 0,85 → 1: thu nhỏ biến mất tại khe.
            Transform[] paws;
            _paws.TryGetValue(root, out paws);
            Vector3 start = root.position;
            bool flashed = false;
            float t0 = Time.unscaledTime;
            while (true)
            {
                if (root == null) yield break;
                c.Refresh();
                float u = Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                float pawAngle;
                float pitch;
                if (u < 0.3f)
                {
                    float j = u / 0.3f;
                    float e = j * j * (3f - 2f * j);
                    root.position = Vector3.Lerp(start, c.RestPos, e) + c.up * (4f * j * (1f - j) * 0.1f * c.S);
                    root.localScale = Vector3.one * c.keySize;
                    pitch = -25f * Mathf.Sin(Mathf.PI * j);
                    pawAngle = -70f * e;                          // giơ chân ra trước
                }
                else
                {
                    ShrinkAtSlot(c, root, u);
                    float j = Mathf.Clamp01((u - 0.3f) / 0.55f);   // 2 nhát trong 0,3..0,85
                    float half = j < 0.5f ? j * 2f : (j - 0.5f) * 2f;
                    pawAngle = Mathf.Lerp(-80f, 30f, half * half);  // quét mạnh từ trên xuống
                    pitch = -10f + 8f * Mathf.Sin(Mathf.PI * half);
                    if (!flashed && j >= 0.5f)
                    {
                        flashed = true;
                        if (Reveal != null) Reveal.PulseGlow(XanhLaNhat, 0.35f, 0.3f);
                    }
                }
                root.rotation = c.Facing * Quaternion.Euler(pitch, 0f, 0f);
                if (paws != null)
                {
                    // Hai chân so le một chút cho giống cào thật.
                    if (paws[0] != null) paws[0].localRotation = Quaternion.Euler(pawAngle, 0f, 0f);
                    if (paws[1] != null) paws[1].localRotation = Quaternion.Euler(pawAngle * 0.8f - 10f, 0f, 0f);
                }
                WagTail(root, u, 14f, 40f);
                if (u >= 1f) yield break;
                yield return null;
            }
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            base.OnReset(reveal);
            _tails.Clear();
            _paws.Clear();
        }
    }
}

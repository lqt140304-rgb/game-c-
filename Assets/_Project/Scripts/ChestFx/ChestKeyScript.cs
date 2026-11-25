using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chuỗi mở CHUẨN của mọi rương dùng vật mở khoá (kế hoạch 2026-11-26, mục 2 và 4.2): cue "chia_khoa" → mở nắp → rung 3 nấc → bóp méo theo bậc.
    /// Chỉ trang trí; bậc do game quyết.
    /// </summary>
    public static class ChestKeyScript
    {
        public const string CueName = "chia_khoa";
        public const string DoneEvent = "chia_khoa_xong";

        private struct TierRow { public ChestTier tier; public string tag; public float squash; public int cycles; public float jump; }

        private static readonly TierRow[] Rows =
        {
            new TierRow { tier = ChestTier.Rac,        tag = "rac",         squash = 0.30f, cycles = 1, jump = 0f },
            new TierRow { tier = ChestTier.Thuong,     tag = "thuong",      squash = 0.10f, cycles = 1, jump = 0.08f },
            new TierRow { tier = ChestTier.Hiem,       tag = "hiem",        squash = 0.16f, cycles = 2, jump = 0.18f },
            new TierRow { tier = ChestTier.RatHiem,    tag = "rat-hiem",    squash = 0.22f, cycles = 3, jump = 0.35f },
            new TierRow { tier = ChestTier.HuyenThoai, tag = "huyen-thoai", squash = 0.30f, cycles = 4, jump = 0.60f },
        };

        public static void Build(ChestScript s, KeyPreset preset)
        {
            s.steps.Clear();
            s.description = "Chuỗi vật mở khoá chuẩn: vật mở khoá đến và mở (cue chia_khoa, 1,35 s) → mở nắp → chuyển động cả rương 3 nấc theo kiểu (rung mọi hướng/bay/xoay/lắc/nảy) → bóp méo theo bậc lúc chốt (bậc cao bay lên ngắn). "
                + "Lưu ý: lõi chốt ở stage 3 (lockStage=3) nên nấc 2 và 3 gắn vào stage 1 và 2 (kế hoạch ghi 2 và 3).";

            // a) vật mở khoá
            ChestStep cue = ChestStepBuilder.Add(s, "chia-khoa", StepKind.Cue, StepStart.OnPress);
            cue.cue = CueName;
            cue.waitForEnd = true;

            // b) mở nắp
            ChestStepBuilder.Add(s, "mo-ruong", StepKind.OpenBox, StepStart.AfterPrevious);

            // c) chuyển động cả rương 3 nấc theo preset.moveStyle (nấc 1: sau "chia_khoa_xong"; nấc 2: stage 1; nấc 3: stage 2)
            float sh = preset.shakeScale <= 0f ? 1f : Mathf.Min(preset.shakeScale, 1.5f); // trần: lệch ngang <= ~0,1 S
            BuildMoves(s, preset.moveStyle, sh);

            // d) bóp méo 5 bậc
            for (int i = 0; i < Rows.Length; i++)
            {
                TierRow r = Rows[i];
                bool rac = r.tier == ChestTier.Rac;
                float mul = rac ? preset.racSquashMul : preset.squashMul;
                float amp = Mathf.Min(r.squash * mul, 0.36f);
                int cycles = rac ? 1 : Mathf.Max(1, r.cycles + preset.cycleDelta);

                ChestStep sq;
                if (rac)
                {
                    // 0,15 s ép + 0,7 s giữ + 0,2 s trả
                    sq = ChestStepBuilder.Motion(s, "squash-" + r.tag, StepMotionKind.SquashHold, amp, 1f, 1.05f, StepStart.OnLock);
                }
                else
                {
                    float dur = 0.9f + 0.25f * (cycles - 1);
                    sq = ChestStepBuilder.Motion(s, "squash-" + r.tag, StepMotionKind.Squash, amp, cycles, dur, StepStart.OnLock);
                }
                sq.ampUnit = AmpUnit.Absolute;
                sq.minTier = r.tier; sq.maxTier = r.tier;

                if (r.jump > 0f)
                {
                    float j = Mathf.Min(r.jump * preset.jumpMul, 0.6f);
                    ChestStep jp = ChestStepBuilder.Motion(s, "jump-" + r.tag, StepMotionKind.Jump, j, cycles, 0.9f, StepStart.WithPrevious);
                    jp.ampUnit = AmpUnit.ChestHeights;
                    jp.minTier = r.tier; jp.maxTier = r.tier;
                }

                // bậc cao: bay lên ngắn "ăn mừng", tự hạ về sau 2,4 s
                if (r.tier == ChestTier.RatHiem || r.tier == ChestTier.HuyenThoai)
                {
                    float fh = r.tier == ChestTier.HuyenThoai ? 0.15f : 0.10f;
                    ChestStep fl = ChestStepBuilder.Motion(s, "bay-mung-" + r.tag, StepMotionKind.Float, fh, 0f, 2.4f, StepStart.WithPrevious);
                    fl.ampUnit = AmpUnit.ChestHeights;
                    fl.minTier = r.tier; fl.maxTier = r.tier;
                }
            }
        }

        private static ChestStep Move(ChestScript s, string id, StepMotionKind kind, float amp, float freq, float dur, int nac, AmpUnit unit, Vector3 axis)
        {
            StepStart start = nac == 1 ? StepStart.OnEvent : StepStart.OnStage;
            ChestStep st = ChestStepBuilder.Motion(s, id, kind, amp, freq, dur, start);
            if (nac == 1) st.startEvent = DoneEvent; else st.startStage = nac - 1;
            st.ampUnit = unit;
            st.axis = axis;
            return st;
        }

        private static readonly float[] NacDur = { 1.2f, 1.6f, 2.2f };

        /// <summary>Dựng 3 nấc chuyển động theo kiểu. Biên độ mét nhân S/0,5 hoặc H; góc (độ) tuyệt đối. Mọi bước tự trả rương về chỗ cũ.</summary>
        private static void BuildMoves(ChestScript s, ChestMoveStyle style, float sh)
        {
            AmpUnit S = AmpUnit.PerHalfMeterSize, H = AmpUnit.ChestHeights, A = AmpUnit.Absolute;
            for (int n = 1; n <= 3; n++)
            {
                int i = n - 1;
                float dur = NacDur[i];
                string tag = "-" + n;
                switch (style)
                {
                    case ChestMoveStyle.Float:
                    {
                        Move(s, "rung" + tag, StepMotionKind.ShakeAll, new[] { 0.006f, 0.005f, 0.006f }[i] * sh, 26f, dur, n, S, Vector3.up);
                        if (n >= 2)
                            Move(s, "bay" + tag, StepMotionKind.Float, n == 2 ? 0.15f : 0.30f, n == 2 ? 20f : 40f, dur, n, H, Vector3.up);
                        break;
                    }
                    case ChestMoveStyle.Spin:
                    {
                        // trọn vòng để khi hết không bị giật về góc cũ
                        Move(s, "xoay" + tag, StepMotionKind.Spin, new[] { 360f, 720f, 1080f }[i], 1f, dur, n, A, Vector3.up);
                        Move(s, "rung" + tag, StepMotionKind.ShakeAll, 0.005f * sh, 26f, dur, n, S, Vector3.up);
                        break;
                    }
                    case ChestMoveStyle.Wobble:
                    {
                        float deg = new[] { 6f, 10f, 15f }[i] * sh;
                        Move(s, "lac-x" + tag, StepMotionKind.Wobble, deg, 2f + n, dur, n, A, Vector3.right);
                        Move(s, "lac-z" + tag, StepMotionKind.Wobble, deg * 0.8f, 1.5f + n, dur, n, A, Vector3.forward);
                        Move(s, "rung" + tag, StepMotionKind.ShakeAll, 0.004f * sh, 26f, dur, n, S, Vector3.up);
                        break;
                    }
                    case ChestMoveStyle.Bounce:
                    {
                        Move(s, "nay" + tag, StepMotionKind.Jump, new[] { 0.06f, 0.12f, 0.20f }[i], 1 + n, dur, n, H, Vector3.up);
                        Move(s, "lac" + tag, StepMotionKind.Wobble, new[] { 4f, 6f, 9f }[i], 1 + n, dur, n, A, new Vector3(1f, 0f, 1f));
                        break;
                    }
                    default: // Shake
                    {
                        Move(s, "rung" + tag, StepMotionKind.ShakeAll, new[] { 0.008f, 0.016f, 0.028f }[i] * sh, 30f, dur, n, S, Vector3.up);
                        break;
                    }
                }
            }
        }
    }
}

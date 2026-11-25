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
            s.description = "Chuỗi vật mở khoá chuẩn: vật mở khoá đến và mở (cue chia_khoa, 1,35 s) → mở nắp → rung 3 nấc → bóp méo theo bậc lúc chốt. "
                + "Lưu ý: lõi chốt ở stage 3 (lockStage=3) nên nấc rung 2 và 3 gắn vào stage 1 và 2 (kế hoạch ghi 2 và 3).";

            // a) vật mở khoá
            ChestStep cue = ChestStepBuilder.Add(s, "chia-khoa", StepKind.Cue, StepStart.OnPress);
            cue.cue = CueName;
            cue.waitForEnd = true;

            // b) mở nắp
            ChestStepBuilder.Add(s, "mo-ruong", StepKind.OpenBox, StepStart.AfterPrevious);

            // c) rung 3 nấc (biên độ nhân S/0,5)
            float sh = preset.shakeScale <= 0f ? 1f : preset.shakeScale;
            ChestStep s1 = ChestStepBuilder.Motion(s, "rung-1", StepMotionKind.Shake, 0.004f * sh, 30f, 1.2f, StepStart.OnEvent);
            s1.startEvent = DoneEvent; s1.ampUnit = AmpUnit.PerHalfMeterSize;
            ChestStep s2 = ChestStepBuilder.Motion(s, "rung-2", StepMotionKind.Shake, 0.007f * sh, 30f, 1.6f, StepStart.OnStage);
            s2.startStage = 1; s2.ampUnit = AmpUnit.PerHalfMeterSize;
            ChestStep s3 = ChestStepBuilder.Motion(s, "rung-3", StepMotionKind.Shake, 0.011f * sh, 30f, 2.2f, StepStart.OnStage);
            s3.startStage = 2; s3.ampUnit = AmpUnit.PerHalfMeterSize;

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
            }
        }
    }
}

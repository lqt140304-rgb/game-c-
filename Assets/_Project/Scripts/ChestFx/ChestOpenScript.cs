using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chuỗi mở CHUẨN của mọi rương (tài liệu: docs/superpowers/specs/2026-11-27-ruong-am-thanh-dong-bo-design.md):
    /// bấm mở → mở nắp → chuyển động cả rương 3 nấc theo <see cref="ChestPreset.moveStyle"/> → bóp méo theo bậc lúc chốt.
    /// Chỉ trang trí; bậc do game quyết (mọi bước chỉ đọc bậc).
    ///
    /// MỘT DÒNG THỜI GIAN CHO ÂM VÀ CHUYỂN ĐỘNG: mỗi nấc và bước bóp méo tự LOAN BÁO một sự kiện (<see cref="ChestStep.announce"/>) ngay khi
    /// chuyển động thật sự bắt đầu; <see cref="ChestSoundHook"/> chỉ phát tiếng khi nhận các sự kiện đó, và đọc thời lượng thật của
    /// chuyển động từ <see cref="ChestScriptContext.GetMotion"/>. Nên không có tiếng nào chạy theo đồng hồ riêng.
    /// Các nấc nối liền nhau: nấc 1 chạy từ lúc bấm tới giai đoạn 1 của hộp, nấc 2 tới giai đoạn 2, nấc 3 tới đúng lúc chốt
    /// (<see cref="ChestStep.durationUntilStage"/>), dù lõi kéo giãn thời gian thế nào.
    /// </summary>
    public static class ChestOpenScript
    {
        /// <summary>Sự kiện "nấc n bắt đầu" (n = 1..3).</summary>
        public static string NacEvent(int nac) { return "nac:" + nac; }
        /// <summary>Sự kiện "bóp méo bắt đầu" (đúng một bước bóp méo chạy theo bậc đã quay).</summary>
        public const string BopEvent = "bop";
        /// <summary>Giai đoạn của BoxOpenSequence mà lõi chốt bậc (ChestRevealSequence.lockStage mặc định).</summary>
        public const int LockStage = 3;
        /// <summary>Giây chồng của nấc 3 sang lúc chốt để không có khoảnh khắc đứng yên giữa nấc 3 và bóp méo (bao dần về 0).</summary>
        public const float Nac3Overlap = 0.2f;
        /// <summary>Thời lượng dự phòng của 3 nấc khi lõi không tính được (không có hộp): khớp thời gian mặc định của hộp.</summary>
        public static readonly float[] NacFallbackSeconds = { 2.1f, 1.6f, 2.8f };
        /// <summary>Độ mạnh 0..1 của từng nấc: âm lượng/cao độ của nền và tiếng nhấn bám theo.</summary>
        public static readonly float[] NacIntensity = { 0.35f, 0.65f, 1.0f };
        /// <summary>Thời lượng nhịp Jump của bóp méo (giây); số lần nảy = số chu kỳ.</summary>
        public const float JumpSeconds = 1.0f;

        private struct TierRow { public ChestTier tier; public string tag; public float squash; public int cycles; public float jump; }

        private static readonly TierRow[] Rows =
        {
            new TierRow { tier = ChestTier.Rac,        tag = "rac",         squash = 0.30f, cycles = 1, jump = 0f },
            new TierRow { tier = ChestTier.Thuong,     tag = "thuong",      squash = 0.11f, cycles = 1, jump = 0.08f },
            new TierRow { tier = ChestTier.Hiem,       tag = "hiem",        squash = 0.16f, cycles = 2, jump = 0.18f },
            new TierRow { tier = ChestTier.RatHiem,    tag = "rat-hiem",    squash = 0.22f, cycles = 3, jump = 0.35f },
            new TierRow { tier = ChestTier.HuyenThoai, tag = "huyen-thoai", squash = 0.30f, cycles = 4, jump = 0.60f },
        };

        /// <summary>Số lần nảy (= số chu kỳ) của bậc <paramref name="rank"/> (0..4) với preset này, tối thiểu 1.</summary>
        public static int Cycles(int rank, ChestPreset preset)
        {
            if (rank <= 0) return 1;
            return Mathf.Max(1, Rows[Mathf.Clamp(rank, 0, 4)].cycles + (preset != null ? preset.cycleDelta : 0));
        }

        /// <summary>
        /// Giây từ lúc bóp méo bắt đầu tới lúc rương chạm ĐỈNH lần nảy đầu (bậc Rất hiếm và Huyền thoại): tiếng chốt vang đúng lúc đó
        /// ("nảy rồi vang"). Các bậc khác trả 0 (tiếng chốt vang ngay khi bóp méo bắt đầu; Rác có quy tắc lặng riêng của hook).
        /// </summary>
        public static float FirstApexSeconds(int rank, ChestPreset preset)
        {
            if (rank < 3) return 0f;
            return 0.5f * JumpSeconds / Cycles(rank, preset);
        }

        public static void Build(ChestScript s, ChestPreset preset)
        {
            s.steps.Clear();
            s.description = "Chuỗi mở chuẩn: mở nắp → chuyển động cả rương 3 nấc nối liền theo kiểu (rung mọi hướng/bay/xoay/lắc/nảy) → bóp méo theo bậc lúc chốt (bậc cao nảy rồi bay lên ngắn). "
                + "Mỗi nấc và bước bóp méo loan báo sự kiện (nac:1..3, bop) để âm thanh bám đúng chuyển động. "
                + "Lõi chốt ở stage 3 (lockStage=3) nên nấc 2 và 3 bắt đầu ở stage 1 và 2.";

            // a) mở nắp (bấm mở)
            ChestStepBuilder.Add(s, "mo-ruong", StepKind.OpenBox, StepStart.OnPress);

            // b) chuyển động cả rương 3 nấc theo preset.moveStyle
            float sh = preset.shakeScale <= 0f ? 1f : Mathf.Min(preset.shakeScale, 1.5f); // trần: lệch ngang <= ~0,1 S
            BuildMoves(s, preset.moveStyle, sh);

            // c) bóp méo 5 bậc
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
                    // hụt hơi: ép 0,17 s + giữ dẹt 0,70 s (chìm thêm chút) + trả 0,23 s (nảy dư nhẹ rồi lắng) = 1,10 s
                    sq = ChestStepBuilder.Motion(s, "squash-" + r.tag, StepMotionKind.SquashHold, amp, 1f, 1.10f, StepStart.OnLock);
                }
                else
                {
                    float dur = 0.9f + 0.25f * (cycles - 1);
                    sq = ChestStepBuilder.Motion(s, "squash-" + r.tag, StepMotionKind.Squash, amp, cycles, dur, StepStart.OnLock);
                }
                sq.ampUnit = AmpUnit.Absolute;
                sq.minTier = r.tier; sq.maxTier = r.tier;
                sq.announce = BopEvent;   // chỉ bước của bậc đã quay chạy, nên sự kiện này phát đúng một lần

                if (r.jump > 0f)
                {
                    float j = Mathf.Min(r.jump * preset.jumpMul, 0.6f);
                    ChestStep jp = ChestStepBuilder.Motion(s, "jump-" + r.tag, StepMotionKind.Jump, j, cycles, JumpSeconds, StepStart.WithPrevious);
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

        /// <summary>
        /// Một bước của nấc <paramref name="nac"/>. Nấc 1 bắt đầu lúc bấm (chờ 0,05 s để lõi kịp mở hộp), nấc 2 và 3 ở giai đoạn 1 và 2
        /// của hộp. Thời lượng nối tới giai đoạn kế (nấc 3 tới lúc chốt + <see cref="Nac3Overlap"/>); <paramref name="announce"/> chỉ đặt cho
        /// bước ĐẦU của nấc.
        /// </summary>
        private static ChestStep Move(ChestScript s, string id, StepMotionKind kind, float amp, float freq, int nac, AmpUnit unit, Vector3 axis, bool announce)
        {
            StepStart start = nac == 1 ? StepStart.OnPress : StepStart.OnStage;
            ChestStep st = ChestStepBuilder.Motion(s, id, kind, amp, freq, NacFallbackSeconds[nac - 1], start);
            if (nac == 1) st.delay = 0.05f; else st.startStage = nac - 1;
            st.durationUntilStage = nac == 3 ? LockStage : nac;
            st.durationTail = nac == 3 ? Nac3Overlap : 0f;
            st.ampUnit = unit;
            st.axis = axis;
            if (announce) st.announce = NacEvent(nac);
            return st;
        }

        /// <summary>
        /// Dựng 3 nấc chuyển động theo kiểu. Biên độ mét nhân S/0,5 hoặc H; góc (độ) tuyệt đối. Mọi bước tự trả rương về chỗ cũ.
        /// Mọi bảng số tăng dần theo nấc (nấc 3 mạnh gấp ~2 lần nấc 2 và ~4 lần nấc 1 về biên độ rung).
        /// </summary>
        private static void BuildMoves(ChestScript s, ChestMoveStyle style, float sh)
        {
            AmpUnit S = AmpUnit.PerHalfMeterSize, H = AmpUnit.ChestHeights, A = AmpUnit.Absolute;
            for (int n = 1; n <= 3; n++)
            {
                int i = n - 1;
                string tag = "-" + n;
                switch (style)
                {
                    case ChestMoveStyle.Float:
                    {
                        Move(s, "rung" + tag, StepMotionKind.ShakeAll, new[] { 0.0055f, 0.0055f, 0.0068f }[i] * sh, new[] { 24f, 26f, 29f }[i], n, S, Vector3.up, true);
                        if (n >= 2)
                            Move(s, "bay" + tag, StepMotionKind.Float, n == 2 ? 0.14f : 0.33f, n == 2 ? 18f : 44f, n, H, Vector3.up, false);
                        break;
                    }
                    case ChestMoveStyle.Spin:
                    {
                        // trọn vòng để khi hết không bị giật về góc cũ
                        Move(s, "xoay" + tag, StepMotionKind.Spin, new[] { 360f, 720f, 1080f }[i], 1f, n, A, Vector3.up, true);
                        Move(s, "rung" + tag, StepMotionKind.ShakeAll, new[] { 0.0045f, 0.005f, 0.0057f }[i] * sh, new[] { 24f, 26f, 29f }[i], n, S, Vector3.up, false);
                        break;
                    }
                    case ChestMoveStyle.Wobble:
                    {
                        float deg = new[] { 5.5f, 10f, 16.5f }[i] * sh;
                        // số chu kỳ theo thời lượng mặc định 2,1 / 1,6 / 3,0 s của 3 nấc để giữ nhịp lắc ~2 - 2,5 Hz như trước (biên độ mới là thứ tăng dần)
                        Move(s, "lac-x" + tag, StepMotionKind.Wobble, deg, new[] { 5f, 4f, 7f }[i], n, A, Vector3.right, true);
                        Move(s, "lac-z" + tag, StepMotionKind.Wobble, deg * 0.8f, new[] { 4f, 3.5f, 6f }[i], n, A, Vector3.forward, false);
                        Move(s, "rung" + tag, StepMotionKind.ShakeAll, new[] { 0.0036f, 0.004f, 0.0046f }[i] * sh, new[] { 24f, 26f, 29f }[i], n, S, Vector3.up, false);
                        break;
                    }
                    case ChestMoveStyle.Bounce:
                    {
                        // số lần nảy 4/3/5 trong 2,1/1,6/3,0 s: mỗi cú nảy dài ~0,53 - 0,6 s như trước (0,53 - 0,6)
                        Move(s, "nay" + tag, StepMotionKind.Jump, new[] { 0.055f, 0.12f, 0.22f }[i], new[] { 4f, 3f, 5f }[i], n, H, Vector3.up, true);
                        Move(s, "lac" + tag, StepMotionKind.Wobble, new[] { 3.7f, 6f, 10f }[i], new[] { 4f, 3f, 5f }[i], n, A, new Vector3(1f, 0f, 1f), false);
                        break;
                    }
                    default: // Shake
                    {
                        Move(s, "rung" + tag, StepMotionKind.ShakeAll, new[] { 0.0072f, 0.016f, 0.031f }[i] * sh, new[] { 27f, 30f, 34f }[i], n, S, Vector3.up, true);
                        break;
                    }
                }
            }
        }
    }
}

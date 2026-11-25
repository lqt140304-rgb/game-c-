using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Ready-made profile values (no audio clips: the editor fills those by file name, see
    /// Editor/ChestRevealTestSetup.cs). One method per chest; the next sessions add theirs here.
    /// </summary>
    public static class ChestFxPresets
    {
        /// <summary>
        /// Shared chest sounds: mp3 made with ElevenLabs (owner approved 2026-11-24, see Audio/Chests/README.md).
        /// Not listened to yet; check the ElevenLabs licence terms before release.
        /// </summary>
        public const string AudioFolder = "Assets/_Project/Audio/Chests/";

        /// <summary>Fallback: placeholder sounds synthesised by Tools/Audio/make_chest_sfx.py (used only when an mp3 is missing).</summary>
        public const string SynthFolder = "Assets/_Project/Audio/Chests/Synth/";

        public static readonly string[] TickFiles = { "sfx_tick.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_lock_rac.mp3", "sfx_lock_thuong.mp3", "sfx_lock_hiem.mp3", "sfx_lock_ratHiem.mp3", "sfx_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_whoosh_grow.mp3";
        public const string CreakFile = "sfx_lid_creak.mp3";
        public static readonly string[] CoinFiles = { "sfx_coin_jingle.mp3" };
        /// <summary>No mp3 exists for these two yet: they come from the synth folder.</summary>
        public const string AmbientFile = "Synth/sfx_chest_open_shimmer.wav";
        public const string ThudFile = "Synth/sfx_chest_lid_thud.wav";

        /// <summary>Synth fallback for each mp3 above (same key = mp3 file name).</summary>
        public static string SynthFallback(string file)
        {
            switch (file)
            {
                case "sfx_tick.mp3": return "Synth/sfx_chest_tick_a.wav";
                case "sfx_lock_rac.mp3": return "Synth/sfx_chest_lock_0_rac.wav";
                case "sfx_lock_thuong.mp3": return "Synth/sfx_chest_lock_1_thuong.wav";
                case "sfx_lock_hiem.mp3": return "Synth/sfx_chest_lock_2_hiem.wav";
                case "sfx_lock_ratHiem.mp3": return "Synth/sfx_chest_lock_3_rathiem.wav";
                case "sfx_lock_huyenThoai.mp3": return "Synth/sfx_chest_lock_4_huyenthoai.wav";
                case "sfx_whoosh_grow.mp3": return "Synth/sfx_chest_whoosh.wav";
                case "sfx_lid_creak.mp3": return "Synth/sfx_chest_lid_creak.wav";
                case "sfx_coin_jingle.mp3": return "Synth/sfx_coin_clink_1.wav";
                default: return null;
            }
        }

        /// <summary>Chest_Golden: gold rays, a fountain of coins that falls back, coins clinking after the lock.</summary>
        public static void ApplyGolden(ChestFxProfile p)
        {
            p.chestId = "Chest_Golden";

            p.tickVolume = 0.5f;
            p.tickPitchStart = 0.95f;
            p.tickPitchEnd = 1.55f;
            p.lockVolume = 0.9f;
            p.whooshVolume = 0.5f;
            p.ambientVolume = 0.35f;
            p.accentVolume = 0.45f;
            p.accentsBase = 1;
            p.accentsPerTier = 1;   // sfx_coin_jingle is 1.2 s long, so fewer than with short clinks

            p.secondaryColor = new Color(1f, 0.86f, 0.35f);   // gold
            p.secondaryMix = 0.55f;
            p.particleLook = ParticleLook.Coin;
            p.particleDirection = ParticleDirection.Fountain;
            p.ambientRate = 22f;
            p.burstCount = 45;
            p.particleSpeed = 0.75f;
            p.particleSize = 0.03f;
            p.particleLifetime = 1.4f;
            p.burstGravity = 0.9f;

            p.rouletteStyle = RouletteStyle.Cycle;
            p.minInterval = 0.05f;
            p.maxInterval = 0.45f;
            p.nearMissChance = 0.55f;
            p.minRouletteSeconds = 2.8f;

            p.rayLengthMultiplier = 1.15f;
            p.rayDuration = 1.2f;
            p.raySpin = 18f;
            p.lightFlashIntensity = 7f;
        }

        /// <summary>
        /// KỊCH BẢN MẪU của Chest_Golden: các rương chủ đề khác chép cách làm này (xem docs/superpowers/specs/2026-11-25-ruong-kich-ban-mo-design.md).
        /// Thứ tự bước = thứ tự đọc. Mọi bước chỉ trang trí; bậc do game quay, không bước nào đổi được.
        /// Cổng đồng xu (bước "cong-xu") TẮT mặc định: rương mở ngay như cũ. Bật bằng reveal.SetStepEnabled("cong-xu", true)
        /// rồi Play: rương chờ người chơi thả đồng xu (ChestSlotItem mã "gold_coin") vào ChestItemSlot mã "coin", sau đó mới mở.
        /// </summary>
        public static void ApplyGoldenScript(ChestScript s)
        {
            s.description = "Golden: xu bắn ra bằng vật lý theo bậc (Rác chỉ rớt vài xu và rương xẹp, Huyền thoại nổ xu, rương bay lơ lửng và xoay), rung lúc hé nắp, cổng đồng xu tuỳ chọn.";
            s.steps.Clear();

            // 1) Cổng đồng xu (tắt mặc định) rồi mở rương. Bước tắt tính là xong ngay nên OpenBox chạy luôn.
            ChestStep gate = ChestStepBuilder.Add(s, "cong-xu", StepKind.Gate, StepStart.OnPress);
            gate.enabled = false;
            gate.gateId = "coin";
            gate.timeoutSeconds = 25f;
            ChestStepBuilder.Add(s, "mo-ruong", StepKind.OpenBox, StepStart.AfterPrevious);

            // 2) Rung nhẹ lúc hé nắp (mọi bậc, giống nhau để không lộ kết quả).
            ChestStep shake = ChestStepBuilder.Motion(s, "rung-he-nap", StepMotionKind.Shake, 0.006f, 30f, 2.4f, StepStart.OnStage);
            shake.startStage = 2;

            // 3) Khoảnh khắc chốt, theo bậc.
            ChestStep sad = ChestStepBuilder.Motion(s, "rac-xep", StepMotionKind.Squash, 0.3f, 1.5f, 0.7f, StepStart.OnLock);
            sad.maxTier = ChestTier.Rac;
            ChestStep sadCoins = ChestStepBuilder.Burst(s, "rac-vai-xu", BodyShape.Coin, 3, 0, StepStart.WithPrevious, 0.1f);
            sadCoins.maxTier = ChestTier.Rac;
            sadCoins.speedMin = 0.8f; sadCoins.speedMax = 1.6f; sadCoins.coneAngle = 14f; sadCoins.spread = 0.5f;

            ChestStep few = ChestStepBuilder.Burst(s, "thuong-xu", BodyShape.Coin, 9, 0, StepStart.OnLock);
            few.minTier = ChestTier.Thuong; few.maxTier = ChestTier.Thuong;

            ChestStep hop = ChestStepBuilder.Motion(s, "hiem-nhay", StepMotionKind.Jump, 0.08f, 2f, 0.8f, StepStart.OnLock);
            hop.minTier = ChestTier.Hiem; hop.maxTier = ChestTier.RatHiem;
            ChestStep many = ChestStepBuilder.Burst(s, "hiem-xu", BodyShape.Coin, 14, 7, StepStart.WithPrevious);
            many.minTier = ChestTier.Hiem; many.maxTier = ChestTier.RatHiem;
            many.spread = 0.4f; many.speedMax = 4.6f;
            ChestStep blast = ChestStepBuilder.Add(s, "hiem-no", StepKind.Explosion, StepStart.WithPrevious, 0.15f);
            blast.minTier = ChestTier.Hiem; blast.maxTier = ChestTier.RatHiem;
            blast.explosionForce = 2.5f; blast.explosionRadius = 1.2f;

            ChestStep hover = ChestStepBuilder.Motion(s, "huyen-thoai-bay", StepMotionKind.Hover, 0.12f, 0f, 3f, StepStart.OnLock);
            hover.minTier = ChestTier.HuyenThoai;
            ChestStep spin = ChestStepBuilder.Motion(s, "huyen-thoai-xoay", StepMotionKind.Spin, 360f, 0f, 1.6f, StepStart.WithPrevious);
            spin.minTier = ChestTier.HuyenThoai;
            ChestStep storm = ChestStepBuilder.Burst(s, "huyen-thoai-xu", BodyShape.Coin, 40, 0, StepStart.WithPrevious);
            storm.minTier = ChestTier.HuyenThoai;
            storm.spread = 0.9f; storm.speedMin = 2.5f; storm.speedMax = 5.5f; storm.coneAngle = 38f;
            ChestStep boom = ChestStepBuilder.Add(s, "huyen-thoai-no", StepKind.Explosion, StepStart.WithPrevious, 0.3f);
            boom.minTier = ChestTier.HuyenThoai;
            boom.explosionForce = 4f; boom.explosionRadius = 1.6f;

            // 4) Cue gọi code của GoldenGlintHook (lấp lánh quanh miệng rương), từ Hiếm trở lên.
            ChestStep glint = ChestStepBuilder.Add(s, "lap-lanh", StepKind.Cue, StepStart.OnLock);
            glint.cue = GoldenGlintHook.CueName;
            glint.minTier = ChestTier.Hiem;
        }
    }
}

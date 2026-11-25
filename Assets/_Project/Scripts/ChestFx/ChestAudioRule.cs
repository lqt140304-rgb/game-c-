using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Quy tắc âm thanh chung cho cả 17 rương (xem docs/superpowers/specs/2026-11-25-thu-gon-hieu-ung-ruong-ke-hoach.md, mục 3).
    /// Hằng số là trần âm lượng / khoảng cách tối thiểu; hồ sơ lõi được ghi gần như câm, hook tự phát nền, nhịp, tiếng chốt.
    /// </summary>
    public static class ChestAudioRule
    {
        public const float BedMax = 0.45f;      // nền lặp
        public const float BeatMax = 0.55f;     // nhịp
        public const float EventMax = 0.5f;     // sự kiện lẻ trước chốt
        public const float ActionMax = 0.8f;    // tiếng hành động (nổ/đổ/bắn)
        public const float StingMax = 1f;       // tiếng chốt theo bậc
        public const float TailMax = 0.4f;      // đuôi sau chốt

        public const float EventGap = 0.2f;     // tối thiểu giữa hai sự kiện
        public const float ImpactGap = 0.12f;   // tối thiểu giữa hai tiếng va chạm vật lý
        public const float TailGap = 0.15f;     // tối thiểu giữa hai tiếng đuôi
        public const float RacSilence = 0.2f;   // lặng trước tiếng xịt của Rác

        /// <summary>A1: hồ sơ lõi gần như câm. Gọi MỖI LẦN chạy Setup, không chỉ khi hồ sơ trống.</summary>
        public static void ApplyToProfile(ChestFxProfile p)
        {
            if (p == null) return;
            if (p.tension == null) p.tension = new ChestTensionSettings();
            p.tension.enabled = false;
            p.tickClips = new AudioClip[0];
            p.openAmbientClip = null;
            p.accentClips = new AudioClip[0];
            p.stageClips = new ChestFxProfile.StageClip[0];
            p.whooshClip = null;
            // TC do hook phát qua stingClips (A11), nên lõi không phát tiếng chốt nào, kể cả Nhật Bản và Rác Quý.
            p.lockClipsByTier = new AudioClip[ChestTierUtil.Count];
        }

        /// <summary>
        /// Trả true và ghi <c>lastTime = Time.time</c> nếu đã qua <paramref name="gap"/> giây từ lần trước.
        /// <c>lastTime</c> khởi tạo nên là một số rất âm (vd. -999f) để lần đầu luôn qua.
        /// </summary>
        public static bool TryGate(ref float lastTime, float gap)
        {
            float now = Time.time;
            if (now - lastTime < gap) return false;
            lastTime = now;
            return true;
        }
    }
}

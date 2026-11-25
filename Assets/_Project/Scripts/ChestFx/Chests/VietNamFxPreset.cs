using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_VietNam (thúng tre đội nón lá, chim Lạc trống đồng): đom đóm vàng lượn xoáy quanh thúng, nhảy màu kiểu
    /// "nhịp trống" (PingPong lên xuống), tia mặt trời trống đồng quay chậm, mõ/phách tre làm tick, nhạc cụ dân tộc khi chốt.
    /// Clip do Editor/VietNamFxSetup.cs gắn theo tên file. Phần đậm hơn ở <see cref="VietNamHook"/>.
    /// </summary>
    public static class VietNamFxPreset
    {
        public const string ChestId = "Chest_VietNam";
        public const string AudioFolder = "Assets/_Project/Audio/Chests/viet_nam/";

        public static readonly string[] TickFiles = { "sfx_vietnam_tick_mo.mp3", "sfx_vietnam_tick_phach.mp3" };
        public static readonly string[] LockFiles =
        {
            "sfx_vietnam_lock_rac.mp3", "sfx_vietnam_lock_thuong.mp3", "sfx_vietnam_lock_hiem.mp3",
            "sfx_vietnam_lock_ratHiem.mp3", "sfx_vietnam_lock_huyenThoai.mp3",
        };
        public const string WhooshFile = "sfx_vietnam_whoosh.mp3";
        public const string AmbientFile = "sfx_vietnam_ambient.mp3";
        public static readonly string[] AccentFiles = { "sfx_vietnam_gong_small.mp3", "sfx_vietnam_dan_bau.mp3" };
        public const string CreakFile = "sfx_vietnam_lid_creak.mp3";
        public const string ThudFile = "sfx_vietnam_thud.mp3";

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.6f;
            p.tickPitchStart = 0.85f;   // mõ trầm lúc đầu, lên dần như phách giục
            p.tickPitchEnd = 1.3f;
            p.lockVolume = 0.9f;
            p.whooshVolume = 0.55f;
            p.ambientVolume = 0.3f;     // đồng lúa đêm: dế, ếch, sáo xa
            p.accentVolume = 0.4f;
            p.accentsBase = 0;          // Rác: không chiêng (để tiếng chó sủa/xe máy tự hài)
            p.accentsPerTier = 1;       // chiêng nhỏ / đàn bầu, mỗi bậc thêm một

            p.secondaryColor = new Color(1f, 0.55f, 0.7f);   // hồng cánh sen
            p.secondaryMix = 0.35f;
            p.particleLook = ParticleLook.SoftDot;            // đom đóm
            p.particleDirection = ParticleDirection.Swirl;    // lượn quanh vành nón
            p.ambientRate = 18f;
            p.burstCount = 55;
            p.particleSpeed = 0.35f;                          // bay lững lờ
            p.particleSize = 0.022f;
            p.particleLifetime = 2.2f;
            p.burstGravity = -0.08f;                          // hơi bốc lên như đèn trời

            p.rouletteStyle = RouletteStyle.PingPong;         // lên xuống như nhịp trống hội
            p.minInterval = 0.05f;
            p.maxInterval = 0.5f;
            p.nearMissChance = 0.4f;
            p.minRouletteSeconds = 3.2f;                      // dài hơn chút cho kịp nghe mõ/phách

            p.rayLengthMultiplier = 0.9f;
            p.rayDuration = 1.6f;
            p.raySpin = 8f;                                   // mặt trời trống đồng quay chậm, trang nghiêm
            p.lightFlashIntensity = 5.5f;
        }
    }
}

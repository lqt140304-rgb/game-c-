using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Loại việc một bước kịch bản làm.</summary>
    public enum StepKind
    {
        Wait,         // đợi duration giây
        Sound,        // phát clip
        Spawn,        // đặt prefab (hạt, đồ trang trí, phi thuyền...) ở một vị trí, tuỳ chọn cho bay theo đường đi
        ChestMotion,  // chuyển động CẢ RƯƠNG: rung, nhảy, quay, lơ lửng, lắc, ép dẹt, bay đi
        ChestVisual,  // ẩn / hiện rương, hoặc biến hình sang prefab khác
        PhysicsBurst, // bắn ra N vật có Rigidbody (xu, mảnh vỡ...)
        Explosion,    // lực nổ đẩy các vật vật lý của rương (và tuỳ chọn cả vật khác trong cảnh)
        Gate,         // chờ người chơi thả/đặt vật NGOÀI vào khe nhận (ChestItemSlot)
        OpenBox,      // bắt đầu mở rương (BoxOpenSequence + quay màu). Không có bước này thì rương tự mở ngay khi Play
        Cue,          // gọi code của hook (ChestFxHook.OnCue) và đợi nó chạy xong: dành cho sự kiện riêng của chủ đề
        RaiseEvent,   // phát một sự kiện có tên để bước khác chờ
    }

    /// <summary>Khi nào bước bắt đầu.</summary>
    public enum StepStart
    {
        AfterPrevious,        // khi bước đứng ngay trên đã XONG (bước bị bỏ qua tính là xong ngay)
        WithPrevious,         // cùng lúc bước đứng ngay trên BẮT ĐẦU
        OnPress,              // lúc bấm mở (Play)
        OnStage,              // khi BoxOpenSequence tới giai đoạn startStage (0 Rung, 1 Gỡ niêm, 2 Hé nắp, 3 Bật nắp, 4 Mở toang)
        OnRoulette,           // khi bắt đầu quay màu
        OnLock,               // khoảnh khắc chốt bậc
        OnSilhouetteComplete, // bóng đã đủ cỡ
        OnRevealed,           // rương mở xong và bóng hoàn chỉnh
        OnEvent,              // khi sự kiện tên startEvent đã xảy ra (RaiseEvent, gate:<id>:ok...)
    }

    /// <summary>Gốc toạ độ cho vị trí của bước.</summary>
    public enum StepAnchor { Mouth, ChestCenter, ChestBase, Silhouette }

    public enum StepMotionKind { Shake, Jump, Spin, Hover, Wobble, Squash, Fly, SquashHold }

    /// <summary>Đơn vị của biên độ ChestMotion: tuyệt đối, theo cỡ rương (mỗi 0,5 m cạnh lớn), hoặc theo chiều cao rương.</summary>
    public enum AmpUnit { Absolute, PerHalfMeterSize, ChestHeights }

    public enum StepVisualAction { Hide, Show, ReplaceWithPrefab }

    public enum BodyShape { Prefab, Coin, Ball, Cube }

    /// <summary>Tên các sự kiện dựng sẵn mà bước có thể chờ (dùng cho OnEvent hoặc hook).</summary>
    public static class ChestEvents
    {
        public const string Press = "press";
        public const string Roulette = "roulette";
        public const string Lock = "lock";
        public const string SilhouetteComplete = "silhouette";
        public const string Revealed = "revealed";
        public static string Stage(int index) => "stage:" + index;
        public static string GateOk(string id) => "gate:" + id + ":ok";
        public static string GateWrong(string id) => "gate:" + id + ":wrong";
        public static string GateTimeout(string id) => "gate:" + id + ":timeout";
        /// <summary>Phát khi cổng xong dù đúng hay hết giờ.</summary>
        public static string GateDone(string id) => "gate:" + id + ":done";
    }

    /// <summary>
    /// MỘT bước của kịch bản. Một lớp phẳng để chỉnh trong Inspector không cần editor riêng: chỉ các trường của
    /// <see cref="kind"/> đang chọn có tác dụng (tooltip ghi rõ). Dựng bằng mã thì dùng các hàm tĩnh ở <see cref="ChestStepBuilder"/>.
    /// </summary>
    [Serializable]
    public class ChestStep
    {
        [Tooltip("Tên gọi, để bật/tắt từ code (ChestRevealSequence.SetStepEnabled) và để đọc log.")]
        public string id;
        public bool enabled = true;
        public StepKind kind = StepKind.Wait;

        [Header("Khi nào chạy")]
        public StepStart start = StepStart.AfterPrevious;
        [Tooltip("OnStage: số thứ tự giai đoạn.")] public int startStage = 3;
        [Tooltip("OnEvent: tên sự kiện.")] public string startEvent;
        [Tooltip("Chờ thêm bấy nhiêu giây sau khi điều kiện bắt đầu thoả.")] public float delay;

        [Header("Điều kiện (chỉ để trang trí, không đổi kết quả)")]
        public ChestTier minTier = ChestTier.Rac;
        public ChestTier maxTier = ChestTier.HuyenThoai;
        [Range(0f, 1f), Tooltip("Xác suất chạy, tung theo hạt giống trang trí của lần mở (cùng hạt = cùng kết quả tung).")] public float chance = 1f;

        [Header("Thời lượng")]
        [Tooltip("Wait/Sound: thời lượng đợi. ChestMotion/PhysicsBurst/Spawn: xem waitForEnd.")] public float duration = 0.5f;
        [Tooltip("true = bước chỉ XONG khi hiệu ứng xong (chuyển động rương tới hết, bay tới hết đường). false = xong ngay (chạy nền).")]
        public bool waitForEnd = false;

        [Header("Vị trí (Spawn, PhysicsBurst, Explosion)")]
        public StepAnchor anchor = StepAnchor.Mouth;
        [Tooltip("Dịch so với gốc, theo hướng của rương (x phải, y lên, z tới trước theo trục của rương).")] public Vector3 offset;

        [Header("Sound")]
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.8f;
        public float pitchMin = 1f;
        public float pitchMax = 1f;

        [Header("Spawn / ChestVisual (Replace)")]
        public GameObject prefab;
        [Tooltip("Tự huỷ sau bấy nhiêu giây (0 = 8 giây).")] public float lifetime = 4f;
        [Tooltip("Spawn: để trống = đặt tại chỗ. Có điểm = bay lần lượt qua các điểm (offset theo hướng rương, từ gốc) trong flyDuration giây.")]
        public Vector3[] path = new Vector3[0];
        public float flyDuration = 3f;
        [Tooltip("Spawn bay: quay mũi theo hướng bay.")] public bool faceMotion = true;
        [Tooltip("Spawn/Replace: nhân kích thước.")] public float scale = 1f;

        [Header("ChestMotion")]
        public StepMotionKind motion = StepMotionKind.Shake;
        [Tooltip("Biên độ: Shake/Jump/Hover/Fly = mét; Spin/Wobble = độ; Squash = tỉ lệ (0.2 = 20%).")] public float amplitude = 0.02f;
        [Tooltip("Đơn vị nhân vào biên độ (chỉ ChestMotion): Absolute = nguyên giá trị; PerHalfMeterSize = nhân (cỡ rương / 0,5 m); ChestHeights = nhân chiều cao rương.")] public AmpUnit ampUnit = AmpUnit.Absolute;
        [Tooltip("Shake = tần số Hz; Jump = số lần nảy; Wobble/Squash = số chu kỳ.")] public float frequency = 24f;
        [Tooltip("Trục: Spin/Wobble quay quanh; Fly bay theo hướng này (hướng thế giới; để 0 = lên).")] public Vector3 axis = Vector3.up;
        [Tooltip("Giữ nguyên trạng thái cuối cho tới khi reset (Fly bay đi rồi ở đó, Spin dừng ở góc cuối...).")] public bool holdEnd = false;

        [Header("ChestVisual")]
        public StepVisualAction visual = StepVisualAction.Hide;

        [Header("PhysicsBurst")]
        public BodyShape shape = BodyShape.Coin;
        public Color bodyColor = new Color(1f, 0.82f, 0.25f);
        [Tooltip("Kích thước vật (mét, đường kính/cạnh) nếu dùng hình dựng sẵn.")] public float bodySize = 0.05f;
        public int count = 8;
        [Tooltip("Thêm bấy nhiêu vật cho mỗi hạng bậc (Rác = 0, Huyền thoại = 4).")] public int countPerTier = 0;
        public float speedMin = 2f;
        public float speedMax = 4f;
        [Tooltip("Góc nón bắn quanh trục đứng của rương (độ).")] public float coneAngle = 28f;
        [Tooltip("Tốc độ xoay ngẫu nhiên tối đa (rad/s).")] public float spin = 12f;
        public float mass = 0.05f;
        [Range(0f, 1f)] public float bounciness = 0.45f;
        [Tooltip("Vật tự biến mất sau bấy nhiêu giây (0 = 6 giây); thu nhỏ rồi huỷ.")] public float bodyLifetime = 6f;
        [Tooltip("Rải thời điểm bắn trong bấy nhiêu giây (0 = bắn cùng lúc). Duration của bước nên >= số này nếu waitForEnd.")] public float spread = 0f;

        [Header("Explosion")]
        public float explosionForce = 5f;
        public float explosionRadius = 1.5f;
        public float explosionUpwards = 0.6f;
        [Tooltip("true = cũng đẩy mọi Rigidbody khác trong bán kính (đồ trong cảnh). Mặc định chỉ đẩy vật của rương.")] public bool affectScene = false;

        [Header("Gate (khe nhận vật ngoài)")]
        [Tooltip("Mã của ChestItemSlot trên rương.")] public string gateId = "gate";
        [Tooltip("Hết giờ thì thôi chờ và coi như không mở khoá (0 = chờ mãi). Kịch bản vẫn chạy tiếp: kết quả rương do game quyết, không do cổng.")]
        public float timeoutSeconds = 20f;

        [Header("Cue (mã riêng của chủ đề) / RaiseEvent")]
        [Tooltip("Cue: tên gửi cho ChestFxHook.OnCue. RaiseEvent: tên sự kiện phát ra.")] public string cue;

        /// <summary>true nếu bước này áp dụng cho bậc <paramref name="tier"/> (không tính xác suất).</summary>
        public bool AppliesTo(ChestTier tier) => tier >= minTier && tier <= maxTier;
    }

    /// <summary>
    /// KỊCH BẢN MỞ RƯƠNG: danh sách bước ghép được (timeline). Mỗi rương chủ đề tạo một asset
    /// (Assets > Create > Cho Do La > Chest Script), gắn vào <see cref="ChestFxProfile.script"/>; phần phải viết bằng mã
    /// thì đặt trong một <see cref="ChestFxHook"/> và gọi bằng bước <see cref="StepKind.Cue"/>.
    /// Kịch bản chỉ DỰNG HIỆU ỨNG: không có đường nào để đổi bậc đã quay.
    /// </summary>
    [CreateAssetMenu(menuName = "Cho Do La/Chest Script", fileName = "ChestScript_New")]
    public class ChestScript : ScriptableObject
    {
        [TextArea, Tooltip("Mô tả ngắn kịch bản làm gì (để người sau đọc).")] public string description;
        public List<ChestStep> steps = new List<ChestStep>();

        /// <summary>true nếu có bước OpenBox đang bật: khi đó Play không tự mở rương, kịch bản quyết thời điểm mở.</summary>
        public bool ControlsBox
        {
            get
            {
                foreach (ChestStep s in steps) if (s != null && s.enabled && s.kind == StepKind.OpenBox) return true;
                return false;
            }
        }
    }

    /// <summary>Hàm dựng bước bằng mã (cho preset và test); mỗi hàm trả về bước để chỉnh thêm.</summary>
    public static class ChestStepBuilder
    {
        public static ChestStep Add(ChestScript s, string id, StepKind kind, StepStart start = StepStart.AfterPrevious, float delay = 0f)
        {
            ChestStep step = new ChestStep { id = id, kind = kind, start = start, delay = delay };
            s.steps.Add(step);
            return step;
        }

        public static ChestStep Motion(ChestScript s, string id, StepMotionKind motion, float amplitude, float frequency, float duration,
            StepStart start = StepStart.WithPrevious, float delay = 0f)
        {
            ChestStep st = Add(s, id, StepKind.ChestMotion, start, delay);
            st.motion = motion;
            st.amplitude = amplitude;
            st.frequency = frequency;
            st.duration = duration;
            return st;
        }

        public static ChestStep Burst(ChestScript s, string id, BodyShape shape, int count, int countPerTier, StepStart start, float delay = 0f)
        {
            ChestStep st = Add(s, id, StepKind.PhysicsBurst, start, delay);
            st.shape = shape;
            st.count = count;
            st.countPerTier = countPerTier;
            return st;
        }
    }
}

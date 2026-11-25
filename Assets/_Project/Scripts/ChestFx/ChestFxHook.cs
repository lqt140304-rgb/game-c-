using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Base class for a chest's own extra effect (phase 2: one style per chest). Put a subclass on the
    /// profile's customFxPrefab, or directly on the chest object: <see cref="ChestRevealSequence"/> finds it and
    /// calls these methods. Override only what you need. Hooks must not change the rolled tier; they only decorate.
    /// </summary>
    public abstract class ChestFxHook : MonoBehaviour
    {
        /// <summary>
        /// BẤM MỞ: ngay khi <see cref="ChestRevealSequence.Play"/> chạy, trước cả khi nắp động đậy. Âm căng thẳng bắt đầu ở đây.
        /// Bậc đã quay đã biết lúc này (reveal.RolledTier) nhưng KHÔNG nên dùng nó để đổi thứ người chơi thấy/nghe trước
        /// <see cref="OnTierLocked"/>, nếu không kết quả bị lộ sớm. Chỉ dùng để chuẩn bị.
        /// </summary>
        public virtual void OnPress(ChestRevealSequence reveal) { }

        /// <summary>Mỗi nhịp "tim đập" của âm căng thẳng, <paramref name="tension"/> 0..1 tăng dần. Dùng để nháy đèn/rung theo nhịp. Không có bậc ở đây, cố ý.</summary>
        public virtual void OnTensionPulse(ChestRevealSequence reveal, float tension) { }

        /// <summary>Khoảnh khắc chốt, sau khi âm đã rẽ theo bậc (<paramref name="resolution"/>: Deflate = xịt ... Blast = bùng nổ).</summary>
        public virtual void OnResolve(ChestRevealSequence reveal, ChestTier tier, TensionResolution resolution) { }

        /// <summary>Một sự kiện kịch bản vừa xảy ra (press, stage:N, roulette, lock, silhouette, revealed, gate:id:ok, hoặc tên tự đặt).</summary>
        public virtual void OnScriptEvent(ChestRevealSequence reveal, string eventName) { }

        /// <summary>
        /// Bước <c>Cue</c> của kịch bản gọi vào đây. Trả về một coroutine (<c>IEnumerator</c>) nếu hook này xử lý cue có tên
        /// <paramref name="cue"/>, kịch bản sẽ ĐỢI nó chạy xong rồi mới sang bước sau; trả <c>null</c> nếu không phải cue của hook này.
        /// Ví dụ rương Không gian: <c>if (cue == "phi_thuyen") return ShipRoutine(ctx);</c>
        /// </summary>
        public virtual IEnumerator OnCue(string cue, ChestScriptContext context) { return null; }

        /// <summary>The roulette starts. <paramref name="mouth"/> is the world position of the chest mouth (Glow).</summary>
        public virtual void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth) { }

        /// <summary>The roulette shows a new colour (called for every tick).</summary>
        public virtual void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color) { }

        /// <summary>The roulette stops on the rolled tier.</summary>
        public virtual void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color) { }

        /// <summary>The silhouette reached its full size.</summary>
        public virtual void OnSilhouetteComplete(ChestRevealSequence reveal, Transform silhouette) { }

        /// <summary>The box is open and the silhouette complete.</summary>
        public virtual void OnRevealed(ChestRevealSequence reveal) { }

        /// <summary>The chest was reset to closed: clean up.</summary>
        public virtual void OnReset(ChestRevealSequence reveal) { }
    }
}

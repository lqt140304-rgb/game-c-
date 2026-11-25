using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Đá rune của trận pháp Chest_Magic. Đặt component này lên một vật có Rigidbody trong cảnh để người chơi
    /// cầm/thả: viên nào nằm gần một điểm trận (trên sàn quanh rương) lúc bấm mở thì được dùng luôn thay cho đá
    /// tự rơi xuống (người chơi "tự đặt trận"); viên ném vào vòng trong lúc tụ phép thành đá phụ bay quanh rương.
    /// Chỉ trang trí, không đổi bậc. <see cref="MagicRuneRitual"/> cũng tự tạo đá có component này.
    /// </summary>
    public class MagicRuneStone : MonoBehaviour
    {
        /// <summary>Mọi viên đá đang bật trong cảnh (thay cho FindObjectsByType).</summary>
        public static readonly List<MagicRuneStone> All = new List<MagicRuneStone>();

        /// <summary>Đá do trận tạo ra (sẽ bị xóa khi reset). Đá người chơi đặt thì không bị xóa.</summary>
        [HideInInspector] public bool spawnedByRitual;

        /// <summary>Đang bị một trận giữ (không cho trận khác lấy).</summary>
        [HideInInspector] public MagicRuneRitual claimedBy;

        private void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
        }

        private void OnDisable() => All.Remove(this);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Gắn lên một vật NGOÀI mà người chơi có thể thả/đặt vào rương (chìa, đồng xu, pin...). Chỉ cần mã định danh;
    /// khe nhận <see cref="ChestItemSlot"/> so mã này với danh sách nó cần.
    /// </summary>
    public class ChestSlotItem : MonoBehaviour
    {
        [Tooltip("Mã loại vật, ví dụ key_gold, coin, battery. Khe nhận so khớp theo mã này.")]
        public string itemId = "item";
    }

    public enum SlotResult
    {
        Ignored,    // khe đang đóng (chưa tới lúc nhận) hoặc vật không có mã: không làm gì
        Accepted,   // đúng và còn cần thêm
        Completed,  // đúng và đủ: mở khoá
        Wrong,      // sai mã, hoặc sai thứ tự khi requireOrder
    }

    /// <summary>
    /// Khe nhận vật ngoài của một rương ("cổng mở khoá"). Đặt trên rương (hoặc con của rương). Có thể nhận theo hai cách:
    ///  - vật lý: vật có <see cref="ChestSlotItem"/> và Rigidbody rơi vào vùng trigger (tự tạo SphereCollider trigger nếu chưa có);
    ///  - mã: game gọi <see cref="Offer(string, GameObject)"/> (ví dụ khi người chơi "đặt đồ đang cầm vào rương").
    /// Chỉ nhận khi đang mở (<see cref="Open"/>): bước Gate của kịch bản mở nó, đóng lại khi xong.
    /// Cổng CHỈ quyết định rương có được "mở khoá" hay không để kịch bản rẽ nhánh/chạy tiếp. NÓ KHÔNG BAO GIỜ đổi bậc đã quay.
    /// Chưa chạy trong Unity.
    /// </summary>
    public class ChestItemSlot : MonoBehaviour
    {
        [Tooltip("Mã cổng, khớp với bước Gate của kịch bản.")] public string gateId = "gate";
        [Tooltip("Các vật cần để mở khoá, theo mã. Lặp mã = cần nhiều cái (ví dụ coin, coin, coin).")]
        public List<string> requiredItems = new List<string> { "coin" };
        [Tooltip("true = phải thả đúng thứ tự trong danh sách; thả sai thứ tự tính là sai và làm lại từ đầu.")] public bool requireOrder = false;
        [Tooltip("Vật đúng bị huỷ sau khi nhận (thu nhỏ vào miệng rương).")] public bool consumeAccepted = true;
        [Tooltip("Vật sai bị đẩy ra khỏi rương.")] public bool ejectWrong = true;
        [Tooltip("Lực đẩy vật sai ra (m/s).")] public float ejectSpeed = 2.5f;

        [Header("Vùng trigger tự tạo (bỏ qua nếu đã có Collider trigger trên đối tượng này)")]
        public Vector3 triggerCenter = new Vector3(0f, 0.25f, 0f);
        public float triggerRadius = 0.28f;

        [Header("Âm (tuỳ chọn, để trống = im)")]
        public AudioClip acceptClip;
        public AudioClip wrongClip;
        public AudioClip completeClip;

        [Header("Sự kiện")]
        public UnityEvent<string> onAccepted = new UnityEvent<string>();   // mã vật vừa nhận đúng
        public UnityEvent<string> onWrong = new UnityEvent<string>();      // mã vật vừa bị từ chối
        public UnityEvent onCompleted = new UnityEvent();          // đủ vật, mở khoá

        private readonly List<string> _remaining = new List<string>();
        private bool _open;
        private bool _completed;

        public bool IsOpen => _open;
        public bool IsCompleted => _completed;
        public int Accepted => requiredItems.Count - _remaining.Count;
        public int Required => requiredItems.Count;

        private void Awake() => EnsureTrigger();

        private void EnsureTrigger()
        {
            foreach (Collider c in GetComponents<Collider>()) if (c.isTrigger) return;
            SphereCollider s = gameObject.AddComponent<SphereCollider>();
            s.isTrigger = true;
            s.center = triggerCenter;
            s.radius = triggerRadius;
        }

        /// <summary>Bắt đầu nhận vật. Làm lại từ đầu mỗi lần mở.</summary>
        public void Open()
        {
            _remaining.Clear();
            _remaining.AddRange(requiredItems);
            _completed = _remaining.Count == 0;
            _open = true;
        }

        /// <summary>Ngừng nhận vật.</summary>
        public void Close() => _open = false;

        private void OnTriggerEnter(Collider other)
        {
            ChestSlotItem item = other.GetComponentInParent<ChestSlotItem>();
            if (item != null) Offer(item.itemId, item.gameObject);
        }

        /// <summary>
        /// Đưa một vật vào khe. <paramref name="obj"/> là vật trong cảnh (để thu/đẩy ra), có thể null nếu game tự xử lý vật đó.
        /// </summary>
        public SlotResult Offer(string itemId, GameObject obj = null)
        {
            if (!_open || _completed || string.IsNullOrEmpty(itemId)) return SlotResult.Ignored;

            int index = _remaining.IndexOf(itemId);
            bool ok = index >= 0 && (!requireOrder || index == 0);
            if (!ok)
            {
                if (requireOrder && index >= 0) { _remaining.Clear(); _remaining.AddRange(requiredItems); }   // sai thứ tự: làm lại
                Play(wrongClip);
                onWrong?.Invoke(itemId);
                if (ejectWrong && obj != null) Eject(obj);
                return SlotResult.Wrong;
            }

            _remaining.RemoveAt(index);
            Play(acceptClip);
            onAccepted?.Invoke(itemId);
            if (consumeAccepted && obj != null) StartCoroutine(Swallow(obj));

            if (_remaining.Count == 0)
            {
                _completed = true;
                Play(completeClip);
                onCompleted?.Invoke();
                return SlotResult.Completed;
            }
            return SlotResult.Accepted;
        }

        private void Eject(GameObject obj)
        {
            Rigidbody rb = obj.GetComponentInParent<Rigidbody>();
            if (rb == null) return;
            Vector3 away = rb.position - transform.TransformPoint(triggerCenter);
            away.y = Mathf.Abs(away.y) + 0.5f;
            rb.AddForce(away.normalized * ejectSpeed, ForceMode.VelocityChange);
        }

        private IEnumerator Swallow(GameObject obj)
        {
            Transform t = obj.transform;
            Vector3 from = t.position, to = transform.TransformPoint(triggerCenter);
            Vector3 scale0 = t.localScale;
            Rigidbody rb = obj.GetComponentInParent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            float time = 0f;
            while (time < 0.25f && t != null)
            {
                time += Time.deltaTime;
                float u = Mathf.Clamp01(time / 0.25f);
                t.position = Vector3.Lerp(from, to, u);
                t.localScale = scale0 * Mathf.Max(0.0001f, 1f - u);
                yield return null;
            }
            if (obj != null) Destroy(obj);
        }

        private void Play(AudioClip clip)
        {
            if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.TransformPoint(triggerCenter), 0.8f);
        }
    }
}

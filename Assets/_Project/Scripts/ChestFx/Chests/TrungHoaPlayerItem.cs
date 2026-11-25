using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Vật người chơi thả/đặt cạnh Chest_TrungHoa (tương tác phụ). Cần Rigidbody + Collider để rơi, ném được;
    /// menu Tools/Cho Do La/Spawn Trung Hoa Incense / Bonus Rocket tạo sẵn.
    ///  - <see cref="Kind.Incense"/> (nén nhang cháy): chạm/đặt gần đầu ngòi -> châm dây pháo ngay
    ///    (nếu hook bật requirePlayerLight thì đây là cách MỞ KHÓA: hộp đứng chờ tới khi ngòi được châm).
    ///  - <see cref="Kind.BonusRocket"/> (pháo thăng thiên thêm): đặt quanh rương trước khi chốt -> bay lên cùng màn
    ///    pháo hoa cuối. Chỉ trang trí, KHÔNG đổi bậc.
    /// Hook tìm các vật này qua danh sách tĩnh <see cref="Active"/> (đo khoảng cách, không cần callback va chạm).
    /// </summary>
    public class TrungHoaPlayerItem : MonoBehaviour
    {
        public enum Kind { Incense, BonusRocket }

        public static readonly List<TrungHoaPlayerItem> Active = new List<TrungHoaPlayerItem>();

        public Kind kind = Kind.Incense;
        [Tooltip("Đầu nhang/ngòi (điểm đo khoảng cách). Trống = chính vật này.")]
        public Transform tip;
        [HideInInspector] public bool consumed;

        public Vector3 TipPosition => tip != null ? tip.position : transform.position;

        private void OnEnable() { if (!Active.Contains(this)) Active.Add(this); }
        private void OnDisable() { Active.Remove(this); }

        /// <summary>Tạo nhanh một vật bằng khối cơ bản (dùng ở menu editor hoặc khi thử trong Play).</summary>
        public static TrungHoaPlayerItem Create(Kind kind, Vector3 position)
        {
            GameObject root = new GameObject(kind == Kind.Incense ? "TrungHoa_NenNhang" : "TrungHoa_PhaoThangThien");
            root.transform.position = position;
            GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stick.name = "Than";
            stick.transform.SetParent(root.transform, false);
            GameObject tipGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tipGo.name = "Dau";
            tipGo.transform.SetParent(root.transform, false);
            Object.Destroy(tipGo.GetComponent<Collider>());
            if (kind == Kind.Incense)
            {
                stick.transform.localScale = new Vector3(0.012f, 0.12f, 0.012f);
                stick.GetComponent<Renderer>().material = ChestFxFactory.CreateUnlit(new Color(0.75f, 0.2f, 0.12f), false, "TrungHoaIncense");
                tipGo.transform.localPosition = new Vector3(0f, 0.125f, 0f);
                tipGo.transform.localScale = Vector3.one * 0.018f;
                tipGo.GetComponent<Renderer>().material = ChestFxFactory.CreateUnlit(new Color(1f, 0.55f, 0.15f), false, "TrungHoaEmber");
            }
            else
            {
                stick.transform.localScale = new Vector3(0.03f, 0.08f, 0.03f);
                stick.GetComponent<Renderer>().material = ChestFxFactory.CreateUnlit(new Color(0.85f, 0.1f, 0.1f), false, "TrungHoaRocketBody");
                tipGo.transform.localPosition = new Vector3(0f, 0.09f, 0f);
                tipGo.transform.localScale = new Vector3(0.035f, 0.04f, 0.035f);
                tipGo.GetComponent<Renderer>().material = ChestFxFactory.CreateUnlit(new Color(1f, 0.8f, 0.25f), false, "TrungHoaRocketCap");
            }
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            rb.angularDamping = 0.5f;
            TrungHoaPlayerItem item = root.AddComponent<TrungHoaPlayerItem>();
            item.kind = kind;
            item.tip = tipGo.transform;
            return item;
        }
    }
}

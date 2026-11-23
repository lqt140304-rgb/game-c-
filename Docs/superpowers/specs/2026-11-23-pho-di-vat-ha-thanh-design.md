# Design Spec: Phố Dị Vật Hà Thành — Chợ Đồ Lạ (Chặng 1 Architectural)

**Ngày:** 2026-11-23  
**Trạng thái:** Đã duyệt (Phương án A — Quảng Trường Trung Tâm & Mạng Ngõ Chuyên Biệt)  
**Đích triển khai:** Blender 5.2.3 LTS → Unity 6 (`6000.6.9f1`, URP), góc nhìn thứ nhất (First-Person)

---

## 1. Mục tiêu Chặng 1
Chuyển đổi toàn bộ khu phố từ quy mô quầy/chòi nhỏ sang **Phố Dị Vật Hà Thành** theo đúng chuẩn **Hà Nội Dị Giới — Stylized Realism**:
1. Hoàn thiện bộ **5 Tài liệu Thiết kế Môi trường Master** trong `Docs/Art/`:
   - `ENVIRONMENT_ART_BIBLE.md`
   - `MARKET_MASTERPLAN.md`
   - `SHOP_DESIGN_SPECS.md`
   - `ASSET_PRODUCTION_LIST.md`
   - `ENVIRONMENT_QA.md`
2. Sinh **Concept Art Package** cho công trình trọng tâm Ưu tiên 1 (`Nhà Niêm Phong — Dì Mận`).
3. Dựng lại các tòa cửa hàng trọng điểm trong Blender với quy mô **lớn gấp 3–4 lần (`15–22m × 18–26m`)**, kiến trúc Hà Nội – Đông Dương khác biệt theo từng nghề nghiệp, nội thất chia rõ các khu chức năng (Khu A/B/C/D) và đưa **toàn bộ hòm mù mới + hòm mở nắp (`SM_Box_*_Open`)** vào bên trong tiệm thay vì bày sạp ngoài hiên.

---

## 2. Quy hoạch được duyệt (Phương án A)
- **Khu Phòng Trọ & Sân Thu Nợ:** `(0, 8)` (`14m × 12m × 4.5m`) — Điểm xuất phát / kết thúc ngày, sân trước cho Anh Bảy & Tí Biên Lai thu nợ.
- **1. Nhà Niêm Phong (Dì Mận) + Sảnh Khui & Kiểm Tra Cổ Vật [Ưu tiên 1]:** `(-22, 44)` (`18m × 22m × 11.2m`) — 3 tầng nhà buôn cổ, 6 bục hộp mù ở Khu A, ròng rọc treo Đại Hòm phía trên, Quầy Dì Mận + tủ hòm mở nắp ở Khu B, kho kệ cao Khu C, gác lửng chữ U Khu D, nối thông vòm sang Sảnh Khui & Kiểm Tra Cổ Vật (`OpenTableManager`, `ArtifactInspector3D`) ở cánh Nam.
- **2. Kho Tạo Tác (Kim Khuy) [Ưu tiên 2]:** `(-21, 72)` (`15m × 18m × 8.5m`) — Hiệu kim hoàn Đông Dương sang trọng, Tường Bí Khế & bục cộng hưởng trung tâm.
- **3. Viện Giám Định (Giáo Sư Lẫm) [Ưu tiên 2]:** `(+22, 44)` (`17m × 22m × 9.8m`) — Biệt thự Pháp cổ cải tạo, đại sảnh mái vòm kính, bàn thiết bị giám định trung tâm, phòng phân tích xanh lạnh & thư viện.
- **4. Xưởng Vá Víu (Chú Rỉ) [Ưu tiên 3]:** `(+26, 18)` (`16m × 20m × 9.0m`) — Nhà kho gạch đỏ–thép công nghiệp nối lối tắt sân sau Viện Giám Định.
- **5. Tiệm Cầm Cố (Bà Cầm) [Ưu tiên 3]:** `(-24, 18)` (`18m × 12m × 9.5m`) — Nhà ống mặt tiền kín trong ngõ Tây–Nam, hệ tủ trăm ngăn khóa cơ khí.
- **6. Nhà Đấu Giá (Quý Cô Chốt) [Ưu tiên 4]:** `(0, 96)` (`20m × 26m × 12.5m`) — Hội quán Đông Dương lớn nhất cuối trục quảng trường.
- **7. Nhà Đỏ (Sòng Bạc Ngược Đời) [Ưu tiên 4]:** `(+25, 72)` (`18m × 18m × 8.8m`) — Hẻm kín Đông–Bắc mở ra đại sảnh Art Deco.

---

## 3. Lộ trình Thực thi (Ưu tiên 1 trước)
- **Gói bàn giao ngay:** Bộ 5 tài liệu `.md`, Concept Art `Nhà Niêm Phong`, và nâng cấp 3D + Scene Unity cho **Phố Dị Vật Hà Thành** tập trung vào **Ưu tiên 1** (`Nhà Niêm Phong — Dì Mận` + Sảnh Khui Hòm + Quảng trường + Mặt tiền/Nội thất cơ bản các tiệm quanh phố), bỏ sạp ngoài hiên, đặt 6 bục hộp mù + 4 hòm mở nắp vào trong tiệm.

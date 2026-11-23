# SHOP DESIGN SPECS — THÔNG SỐ CHI TIẾT 7 CỬA HÀNG & PHÒNG TRỌ

**Phong cách chung:** Hà Nội Dị Giới — Stylized Realism (First-Person `1.8m`)

---

## 1. NHÀ NIÊM PHONG — DÌ MẬN (+ SẢNH KHUI HÒM) [ƯU TIÊN 1]
- **Chức năng:** Bán 6 hộp mù mỗi ngày; cánh Nam tích hợp Sảnh Khui & Kiểm Tra Cổ Vật sơ bộ.
- **Kích thước:** Rộng mặt tiền `22.0m` × Sâu `18.0m` × Cao `11.2m` (3 tầng kiến trúc).
- **Mặt tiền & Concept:**
  - Tham khảo Concept Art: [concept_1_nha_niem_phong_exterior.jpg](Concepts/concept_1_nha_niem_phong_exterior.jpg), [concept_2_nha_niem_phong_interior.jpg](Concepts/concept_2_nha_niem_phong_interior.jpg).
  - Nhà buôn Hà Nội cổ 3 tầng tường vữa vàng đất có vết ố thời gian, chân tường gạch đỏ & đá xanh, cửa gỗ lim mở rộng `4.2m × 3.6m`, ban công sắt uốn tầng 2–3, biển hiệu **`NHÀ NIÊM PHONG`** + con dấu sáp đỏ nổi khối, hệ **dầm ròng rọc giao nhận** nhô ra ở tầng 2 bên hông Bắc.

```
         ┌─────────────────────────────────────────┐
         │              KHU C: KHO PHÍA SAU        │ (X: -8.6 .. -4.5)
         │  [Kệ gỗ cao 4 tầng · Kiện hàng·Thang]   │
         ├───────────────────┬─────────────────────┤
         │ KHU B: QUẦY DÌ MẬN│   [Cổng vòm kho]    │ (X: -4.2 .. -1.8)
         │ (Tủ hộp mở nắp sau│                     │
         │  lưng · Sổ · Dấu) │                     │
  [-Y]   ├───────────────────┴─────────────────────┤  [+Y]
  SẢNH   │           KHU A: ĐẠI SẢNH TRƯNG BÀY     │
  KHUI   │  [Bục 1] [Bục 2] [Bục 3]                │ (X: -1.5 .. +7.5)
  HÒM ◄──┼── (Phía trên: Ròng rọc treo ĐẠI HÒM) ───┼──► CỬA CHÍNH (+X)
 (Nam)   │  [Bục 4] [Bục 5] [Bục 6]                │    ra Quảng Trường
         └─────────────────────────────────────────┘
         (Phía trên ở Z = 3.8m: KHU D GÁC LỬNG CHỮ U)
```

- **Chi tiết 4 Khu & Điểm nhấn:**
  - **Khu A (Sảnh Trưng Bày):** Thông tầng cao `6.8m`; đặt **6 bục gỗ–đá** tách rời cho 6 hộp bán trong ngày, khoảng hở đi quanh `≥ 2.2m`; ngay trên đầu là **Hero Asset: Hệ Ròng Rọc & Xích Đồng treo Đại Hòm Niêm Phong Dị Giới**.
  - **Khu B (Quầy Dì Mận):** Quầy gỗ lim dài `6.8m` có sổ cái, khuôn đóng dấu sáp đỏ, cân đĩa đồng; sau lưng là tủ cao trưng bày **4 mẫu Hòm Mở Nắp (`SM_Box_*_Open`)** có lót nhung/vải & đèn hắt.
  - **Khu C (Kho Phía Sau):** Hệ kệ gỗ cao 4 tầng chất kiện hàng dán tem đa thế giới, thang trượt & ròng rọc hàng nhìn xuyên từ sảnh qua 2 cổng vòm gỗ.
  - **Khu D (Gác Lửng Tầng 2):** Lan can gỗ bao chữ U ở `+3.8m`, kệ lưu trữ & đèn lồng thả ấm áp; cầu thang chặn dây nhung lịch sự.
  - **Cánh Nam — Sảnh Khui & Kiểm Tra Cổ Vật:** Thông qua cổng vòm `3.2m`, đặt Bàn Mở & Soi Cổ Vật (`OpenTableManager`) có 3 khay đặt hộp, đèn chụp đồng, kính lúp & tủ đối chiếu vỏ hòm mở nắp.
- **Vật liệu & Ánh sáng:** Gỗ lim tối, gạch Bát Tràng đỏ, vữa vàng cũ, đồng cũ, sáp đỏ; ánh sáng vàng ấm thương mại (`3000K`) rọi từng bục hộp & tràn ra vỉa hè.

---

## 2. KHO TẠO TÁC — KIM KHUY [ƯU TIÊN 2]
- **Chức năng:** Xem, mua trang sức/tạo tác (nhẫn, vòng tay, dây chuyền, vật phẩm 2 hệ) và kích hoạt Tường Bí Khế.
- **Kích thước:** Mặt tiền `18.0m` × Sâu `15.0m` × Cao `8.5m`.
- **Ngoại thất:** Hiệu kim hoàn Đông Dương sang trọng: cột thanh mảnh, ô cửa kính lớn khung đồng–sắt đen, chữ nổi kim loại **`KHO TẠO TÁC · KIM KHUY`**, họa tiết vòng tròn mắt xích đối xứng.
- **Nội thất 4 Khu:**
  - **Khu A (Phòng trưng bày chính):** Sàn đá hoa xanh ngọc–kem sạch thoáng, 6 tủ/bục kính bọc đồng đặt 6 tạo tác (tâm điểm là **bục lớn giữa phòng dành cho 3 tạo tác bán trong ngày**).
  - **Khu B (Quầy Kim Khuy):** Quầy chế tác mặt kính–nhung đen, kính lúp kim hoàn, kẹp nhíp, khay nhung cho NPC Kim Khuy trao trang sức.
  - **Khu C (Tường Bí Khế — Tường Nam):** Bảng trưng bày lớn chia ô riêng cho từng Bí Khế: món khóa tối màu khắc ký hiệu khóa cơ khí, món mở khóa sáng đèn vàng kim & mở lẫy đồng.
  - **Khu D (Phòng chế tác riêng — Phía sau):** Ngăn bằng vách kính khung đồng, thấy bên trong có máy đánh bóng huyền thuật & vòng cơ khí quay chậm.
- **Hero Asset:** **Vòng Cộng Hưởng Trang Sức 3 Lớp (`Tact_Resonance_Rings`)** đặt trên bục trung tâm.
- **Màu & Ánh sáng:** Xanh ngọc đậm, vàng kim, đen, đá hoa; Spotlight tập trung sắc nét vào từng món trang sức.

---

## 3. VIỆN GIÁM ĐỊNH — GIÁO SƯ LẪM [ƯU TIÊN 2]
- **Chức năng:** Kiểm tra sâu, phân tích vật liệu, xác thực đồ thật/giả và định giá chuẩn cổ vật.
- **Kích thước:** Mặt tiền `22.0m` × Sâu `17.0m` × Cao `9.8m`.
- **Ngoại thất:** Biệt thự Pháp cổ Hà Nội mặt tiền cân đối, tường vàng kem, cửa chớp xanh cao, mái ngói có chóp kính bát giác trung tâm, ban công sắt uốn, bậc thềm đá rộng `4.2m`, biểu tượng kính lúp đồng + biển **`VIỆN GIÁM ĐỊNH`**.
- **Nội thất 4 Khu:**
  - **Khu A (Đại sảnh giám định):** Trần cao dưới vòm kính trung tâm, tủ hồ sơ và tủ kính mẫu chuẩn đối xứng.
  - **Khu B (Bàn Giáo Sư Lẫm):** Bàn gỗ sậm đặt kính hiển vi, đèn soi quang học, dung dịch thử kim loại và sách tra cứu.
  - **Khu C (Phòng phân tích chuyên sâu — Cánh Bắc):** Giá đỡ cổ vật, vòng quét năng lượng phát ánh sáng xanh lạnh dịu (`CyanGlow`) tách biệt với sảnh chính.
  - **Khu D (Thư viện lưu trữ — Cánh Nam):** Tủ sách cao sát trần, bàn bản đồ, ngăn tủ mẫu vật có đánh nhãn.
- **Hero Asset:** **Bàn Máy Giám Định Trung Tâm** kết hợp thấu kính đồng cổ điển và vòng quét dị giới.
- **Màu & Ánh sáng:** Kem, nâu sẫm, xanh rêu, đồng; ánh sáng trắng trung tính rõ vật liệu ở sảnh + xanh lạnh ở cánh phân tích.

---

## 4. XƯỞNG VÁ VÍU — CHÚ RỈ [ƯU TIÊN 3]
- **Chức năng:** Phục chế, hàn gắn, ghép mảnh cổ vật bị hỏng/rạn nứt.
- **Kích thước:** Mặt tiền `20.0m` × Sâu `16.0m` × Cao `9.0m`.
- **Ngoại thất:** Nhà kho công nghiệp gạch đỏ trần – khung thép I, cửa xưởng cuốn cao, cửa sổ chia ô công nghiệp, ống khói & đường ống hơi nước chạy dọc mặt tiền, biển kim loại cũ **`XƯỞNG VÁ VÍU`**.
- **Nội thất 4 Khu:**
  - **Khu A (Xưởng chính):** Trần cao, dầm cầu trục vàng cũ phía trên, lối đi thoáng rộng ở giữa.
  - **Khu B (Khu phục chế tinh xảo):** Bàn sạch đặt cọ, keo gốm, đèn kẹp tập trung cho đồ gốm/dễ vỡ.
  - **Khu C (Khu sửa máy móc):** Máy hàn, tủ linh kiện, bộ phận bánh răng tháo rời.
  - **Khu D (Kho nguyên liệu):** Kệ thép phân loại rõ gỗ – đồng – thủy tinh – đá quý theo màu và thùng chứa.
- **Hero Asset:** **Bàn Phục Chế Cánh Tay Máy Đa Khớp** ở tâm xưởng dưới dầm cầu trục.
- **Màu & Ánh sáng:** Xám thép, cam gỉ, nâu dầu, xanh công nghiệp; đèn xưởng rọi cục bộ độ tương phản cao.

---

## 5. TIỆM CẦM CỐ — BÀ CẦM [ƯU TIÊN 3]
- **Chức năng:** Cầm cố cổ vật lấy tiền gấp, quản lý hợp đồng & chuộc lại đồ.
- **Kích thước:** Mặt tiền `12.0m` × Sâu `18.0m` × Cao `9.5m`.
- **Ngoại thất:** Nhà ống Hà Nội cao, mặt tiền hẹp hơn các tiệm khác để tạo cảm giác sâu & kín: cửa gỗ lim dày bọc đinh đồng, khung song sắt bảo vệ, đèn vàng trầm.
- **Nội thất 3 Khu nối tiếp chiều sâu:**
  - **Khu A (Phòng giao dịch):** Quầy gỗ nặng có khung chấn song đồng, ô chuyển đồ, sổ biên nhận, dấu triện & dãy ghế chờ sát tường.
  - **Khu B (Kho tài sản):** Tủ bảo quản có khóa, két thép lớn sau cửa song sắt nhìn thấy được độ sâu tài sản đang ký gửi.
  - **Khu C (Phòng hợp đồng — Sâu bên trong):** Bàn viết lớn, đèn bàn chao xanh, hồ sơ cho giao dịch giá trị cao.
- **Hero Asset:** **Hệ Tủ Lưu Trữ Trăm Ngăn Khóa Cơ Khí** với biển số đồng chạy kín vách hậu.
- **Màu & Ánh sáng:** Nâu đỏ sẫm, đen sắt, vàng cũ, xám kim loại; ánh sáng trầm và kín.

---

## 6. NHÀ ĐẤU GIÁ — QUÝ CÔ CHỐT [ƯU TIÊN 4]
- **Chức năng:** Đấu giá phiên đặc biệt, ký gửi hàng hiếm.
- **Kích thước:** Mặt tiền `26.0m` × Sâu `20.0m` × Cao `12.5m` (Hoành tráng nhất khu phố).
- **Ngoại thất:** Hội quán / Nhà hát Đông Dương mặt tiền rộng, cột trụ đôi lớn, tiền sảnh có mái vòm và bậc đá rộng, ban công tầng trên & chữ nổi vàng kim **`NHÀ ĐẤU GIÁ`**.
- **Nội thất 4 Khu:**
  - **Khu A (Tiền sảnh):** Quầy đăng ký, bảng lịch phiên, tủ kính giới thiệu lô hàng nổi bật.
  - **Khu B (Đại sảnh đấu giá):** Trần thông tầng cao `10m`, dãy ghế ngồi bọc nhung đỏ xếp vòng cung hướng tâm, ban công tầng 2 quan sát xuống sàn.
  - **Khu C (Bục trưng bày hiện vật):** Sân khấu bục gỗ–đá có **bàn xoay trưng bày** & chùm đèn sân khấu rọi thẳng.
  - **Khu D (Phòng xem trước hiện vật):** Tủ kính & bàn kiểm tra lô hàng cạnh sảnh.
- **Hero Asset:** **Đèn Chùm Đông Dương Đa Tầng & Sân Khấu Bục Xoay Đấu Giá**.
- **Màu & Ánh sáng:** Đỏ rượu, vàng kim, gỗ tối, kem; ánh sáng sân khấu tương phản cao.

---

## 7. NHÀ ĐỎ — SÒNG BẠC NGƯỢC ĐỜI [ƯU TIÊN 4]
- **Chức năng:** Trò chơi may rủi bằng tiền trong game.
- **Kích thước:** Hẻm vào hẹp `4.5m` mở vào đại sảnh `18.0m × 18.0m × 8.8m`.
- **Ngoại thất:** Mặt tiền kín đáo trong hẻm Đông Bắc, bảng đèn đỏ/đồng, cửa gỗ nặng dẫn qua hành lang hẹp.
- **Nội thất 3 Khu:** Hành lang hẹp tối (Khu A) → mở bất ngờ ra Đại sảnh Art Deco rộng thoáng đặt bàn cược & quầy đổi xu (Khu B) → Phòng riêng cao cấp phía sau (Khu C).
- **Hero Asset:** **Cỗ Máy Quay Vận Mệnh Dị Giới** đặt giữa đại sảnh.
- **Màu & Ánh sáng:** Đỏ sẫm, tím huyền, đen, vàng đồng; hành lang tối tương phản đại sảnh rực rỡ ánh đỏ–tím.

---

## 8. KHU PHÒNG TRỌ & SÂN THU NỢ (ANH BẢY & TÍ BIÊN LAI) [ƯU TIÊN 3]
- **Vị trí:** Đầu Nam trục phố `(0, 8)`, nhìn thẳng ra Quảng Trường Trung Tâm.
- **Phòng Trọ (`8m × 7m`):** Phòng thuê Hà Nội cũ ấm nhưng hơi chật với giường gỗ màn tuyn, bàn viết đặt Sổ Tay hành trình, bảng theo dõi nợ 7 ngày, tủ khóa cất Cổ Vật Gia Truyền.
- **Khu Sân Trước Cửa (`14m × 7m`):** Khoảng hiên & sân gạch thoáng đủ chỗ cho **Anh Bảy & Tí Biên Lai** đứng thu nợ vào cuối ngày dưới ánh đèn vàng/lạnh kịch tính.

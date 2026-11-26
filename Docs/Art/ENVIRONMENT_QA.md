# ENVIRONMENT QA CHECKLIST — PHỐ DỊ VẬT HÀ THÀNH

Mỗi công trình và khu phố chỉ được nghiệm thu khi toàn bộ hạng mục bên dưới **ĐẠT** khi test trực tiếp ở chế độ **Play (First-Person `1.8m`)**:

---

## 1. Bản sắc Kiến trúc & Silhouette
- [ ] Công trình mang ngôn ngữ Hà Nội / Đông Dương rõ nét (vữa cũ, gỗ lim, gạch đỏ, cửa chớp, mái ngói/sắt uốn), không giống nhà hộp nhựa hay kiosk tạm.
- [ ] Đứng từ Quảng Trường nhìn 360°, silhouette 7 cửa hàng phân biệt rõ rệt về chiều cao, hình khối mái, mặt tiền và chất liệu.
- [ ] Có dấu vết thời gian và lịch sử cấy ghép dị giới hợp lý (không quá sạch như đồ chơi).

## 2. Tỷ lệ First-Person & Va chạm (Colliders)
- [ ] Đi bộ qua mọi cửa chính (`≥ 3.4m × 3.2m`) mượt mà, không bị collider vô hình chặn hoặc kẹt mép tường.
- [ ] Chiều cao trần (`≥ 4.5m`, thông tầng `6.8–10m`), chiều cao quầy (`~0.95–1.05m`), bàn soi (`~0.85–0.92m`) khớp tầm mắt camera `1.62m`.
- [ ] Khoảng cách quanh 6 bục hộp mù và trước quầy NPC rộng tối thiểu `1.5–2.5m`, không bị va vấp vào đồ trang trí nhỏ.
- [ ] Không có mesh xuyên tường/xuyên sàn, hở khe nền móng hoặc vật thể lơ lửng bất hợp lý.

## 3. Gameplay & Khả năng Quan sát
- [ ] Bước tới cửa chính đã nhận diện ngay chức năng của tiệm (biển hiệu + ánh sáng dẫn hướng + hình khối nội thất chính).
- [ ] Toàn bộ **6 bục hộp mù** trong Nhà Niêm Phong, **Hòm Mở Nắp**, **Bàn Mở & Soi Cổ Vật**, và điểm tương tác NPC dễ nhìn, không bị rèm/cây/sạp hàng che khuất.
- [ ] Bổ sung raycast tương tác chuẩn: bấm `[E]` ở cự ly tự nhiên nhận đúng Prompt, không bị collider tường hoặc đồ trang trí chắn tia Raycast.

## 4. Ánh sáng & Vật liệu
- [ ] Ban ngày nội thất vẫn đủ sáng ở các vùng tương tác chính (quầy, 6 bục hộp, bàn soi); không bị mảng đen kịt hay cháy sáng Bloom gắt.
- [ ] Cửa hàng có màu nhấn & ánh sáng riêng đúng theo Art Bible (Dì Mận ấm thương mại, Kim Khuy rọi điểm sang trọng, Giám Định trung tính/xanh lạnh...).
- [ ] Vật liệu PBR dùng chung material/texture atlas hợp lý; kim loại, gỗ, đá, kính, nhung phân biệt được rõ bề mặt.

## 5. Hiệu năng & Tích hợp Kỹ thuật
- [ ] Chỉ đặt đèn dynamic đổ bóng ở nguồn sáng chủ chốt; không bật shadow mọi đèn phụ trong tiệm.
- [ ] Các đồ trang trí tĩnh không gắn Rigidbody/vật lý liên tục không cần thiết.
- [ ] Chạy menu `Tools → Cho Do La → Build Market Town Blockout` và `TownBlockoutBuilderTests` đều không báo lỗi Console/Compiler/Warning API lỗi thời.

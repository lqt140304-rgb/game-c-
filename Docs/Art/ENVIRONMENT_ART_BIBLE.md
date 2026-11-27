# ENVIRONMENT ART BIBLE — CHỢ ĐỒ LẠ (HÀ NỘI DỊ GIỚI)

**Phiên bản:** 1.0  
**Định hướng chủ đạo:** **HÀ NỘI DỊ GIỚI — STYLIZED REALISM**  
**Góc nhìn thiết kế:** 3D First-Person (`1.7–1.8 m` chiều cao nhân vật, camera `1.6–1.7 m`)  
**Engine mục tiêu:** Unity 6 (`6000.6.9f1`, URP) & Blender `5.2.3 LTS`

---

## 1. Tuyên ngôn Phong cách (Art Direction)

**Phố Dị Vật Hà Thành** kết hợp hài hòa hai trục thẩm mỹ:
1. **Nền tảng hiện thực Hà Nội & Đông Dương có lịch sử:** Nhà ống thương mại lâu đời, biệt thự Pháp cổ cải tạo, hội quán Đông Dương và nhà xưởng gạch–thép thời công nghiệp. Bề mặt luôn mang dấu vết thời gian (vết ố ẩm chân tường, rêu nhẹ, vết sứt mép bậc thềm, gỗ lim lên nước mòn tay, đồng xỉn màu).
2. **Lớp cấy ghép Dị Giới & Cơ Khí (Fantasy / Steampunk / Sci-Fi):** Các chủ tiệm tự nâng cấp công trình bằng máy móc đồng hồ hơi nước, ròng rọc cơ khí nặng, tinh thể cổ tự phát sáng tiết chế hoặc ống dẫn năng lượng.

**Nguyên tắc tối thượng:**
- **KHÔNG** thiết kế cửa hàng dạng chòi, lều bạt, kiosk tạm ngoài trời.
- **KHÔNG** lặp lại phòng hình hộp giống nhau cho mọi tiệm.
- **KHÔNG** dùng Low-Poly khối trơn đồ chơi hoặc Cartoon sặc sỡ trẻ con, cũng không đẩy Photorealistic quá nặng làm nhiễu gameplay.
- **Ưu tiên độ đọc ở tầm mắt người chơi** và sự thuyết phục của vật liệu trước số lượng đồ trang trí lắt nhắt.

---

## 2. Ngôn ngữ Kiến trúc Hà Nội – Đông Dương

Các thành phần nhận diện dùng có chọn lọc tùy nghề nghiệp từng công trình (không rập khuôn máy móc):
- **Tường & Vữa:** Vữa vàng đất / vàng kem Hà Nội ngả màu (`Mat_WeatheredPlaster`), vữa ấm (`Mat_WallCream`), gạch nung lộ vữa (`Mat_ExposedBrick`), đá cắt chân tường (`Mat_AshlarStone`).
- **Cửa & Mặt tiền:** Cửa gỗ lim dày 4 cánh, cửa chớp xanh rêu (`Mat_ShuttersGreen`), cửa kính khung thép/gỗ lớn cho hiệu kim hoàn, cửa sắt xếp/song sắt kiên cố cho tiệm cầm đồ.
- **Chi tiết tầng trên:** Ban công sắt uốn cổ điển, lam gỗ, giếng trời kính khung sắt lấy sáng tự nhiên cho sảnh rộng, biển hiệu tiếng Việt sơn son thiếp vàng hoặc chữ kim loại nổi.
- **Mặt nền & Giao thông:** Sàn gạch bông Đông Dương (`Mat_WorkshopFloor`), gạch đỏ Bát Tràng (`Mat_VnRedTile`), đá lát xám nhám (`Mat_HanoiTerrazzo`), bó vỉa đá xanh vát cạnh & mặt đường nhựa đô thị ở ngã ba phía Bắc.

---

## 3. Tiêu chuẩn Vật liệu (Stylized PBR)

| Nhóm vật liệu | Bề mặt & Chỉ số tham chiếu (URP Lit) | Ứng dụng điển hình |
| --- | --- | --- |
| **Gỗ tự nhiên / Gỗ lim cũ** | Có vân thớ gỗ & Normal Map (`Smoothness 0.25–0.42`), cạnh vát nhẹ để bắt sáng. | Cột xà, quầy buôn Dì Mận, gác lửng, tủ kính, trường kỷ, khung cửa. |
| **Kim loại (Đồng cũ, Sắt, Vàng)** | `Metallic 0.78–0.92`, `Smoothness 0.45–0.82`; đồng cũ sẫm hơn vàng kim hoàn. | Ròng rọc, xích treo hòm, ống hơi nước, chữ nổi biển hiệu, tay đòn soi cổ vật. |
| **Gốm sứ & Men ngọc** | `Smoothness 0.70–0.85`, gạch men xanh cổ vịt có mạch vữa nhám (`Smoothness 0.15`). | Chân tường phòng thí nghiệm, bình gốm trưng bày, lọ hương/trà. |
| **Đá lát & Đá hoa** | Độ nhám vừa (`Smoothness 0.14–0.32`) kèm Normal Map tạo độ lì mặt sân. | Vỉa hè quảng trường, sảnh kim hoàn, thềm biệt thự Pháp. |
| **Kính & Vải nhung** | Kính trong suốt có chọn lọc (1 lớp), nhung đỏ/xanh nhám (`Smoothness 0.10–0.18`). | Tủ trưng bày hiện vật, lót lòng hộp mù, rèm sân khấu đấu giá, đệm soi đồ. |
| **Vật liệu Dị Giới** | Phát sáng tiết chế (`Emissive` có điểm nhấn, không phủ kín tường). | Đá cổ tự tím, lõi năng lượng xanh lơ, dấu ấn sáp phát quang. |

---

## 4. Bảng màu & Hệ thống Ánh sáng

### 4.1. Bảng màu nền chung toàn khu phố
- **Nền kiến trúc:** Vàng đất (`#C9A45C`), Vàng kem cũ (`#E2D1AE`), Đỏ gạch nung (`#8E3B2E`), Xám đá xanh (`#4A5056`).
- **Gỗ & Kim loại:** Nâu gỗ lim (`#4A2E1B`), Đồng thau cũ (`#B58634`), Sắt đen (`#292C32`), Xanh rêu cửa chớp (`#2B4D36`).

### 4.2. Bản sắc Màu & Ánh sáng theo Từng Tiệm

| Công trình | Màu nhấn nhận diện | Nhiệt độ & Chiến lược Ánh sáng Nội thất |
| --- | --- | --- |
| **1. Nhà Niêm Phong (Dì Mận)** | Đỏ son sáp niêm, Nâu gỗ, Đồng cũ | Vàng ấm thương mại (`3000K`), hắt từ sảnh ra phố; đèn rọi xuống 6 bục hộp và ròng rọc trung tâm. |
| **2. Kho Tạo Tác (Kim Khuy)** | Xanh ngọc đậm, Vàng kim, Đá hoa | Ánh sáng sang trọng, `SpotLight` hội tụ trực tiếp vào từng tủ kính trang sức và Tường Bí Khế. |
| **3. Viện Giám Định (GS Lẫm)** | Kem, Xanh rêu, Trắng trung tính | Đại sảnh sáng rõ chuẩn xác (`4800K`) để đọc chất liệu cổ vật; cánh phân tích pha ánh xanh lạnh dịu. |
| **4. Xưởng Vá Víu (Chú Rỉ)** | Xám thép, Cam gỉ, Xanh công nghiệp | Ánh sáng xưởng mạnh cục bộ trên bàn phục chế & cầu trục, tương phản bóng đổ rõ. |
| **5. Tiệm Cầm Cố (Bà Cầm)** | Nâu đỏ sẫm, Đen sắt, Vàng cũ | Trầm, kín đáo, đèn chụp rọi thẳng bàn giao dịch trước song sắt, kho sau mờ ẩn. |
| **6. Nhà Đấu Giá (Quý Cô Chốt)** | Đỏ rượu vang, Vàng kim, Kem | Ánh sáng sân khấu kịch tính: đèn chùm lớn + chùm rọi thẳng vào bục xoay hiện vật trung tâm. |
| **7. Nhà Đỏ (Sòng Bạc)** | Đỏ sẫm, Tím huyền, Vàng đồng | Hành lang lối vào tối, sảnh chính đèn đỏ-tím phản chiếu mặt sàn bóng nhẹ. |
| **8. Phòng Trọ & Sân Thu Nợ** | Vàng đèn dây tóc trầm / Đêm xanh | Ấm nhưng chật ở trong phòng; buổi tối ngoài sân có đèn hắt lạnh tạo áp lực đối thoại với Anh Bảy. |

### 4.3. Quy tắc Chiếu sáng & Hiệu năng
- **Ban ngày:** Ánh nắng chính ấm tự nhiên, bóng đổ mềm vừa phải; luôn có đèn hắt phụ nhẹ để nội thất không bị đen kịt khi nhìn từ ngoài phố.
- **Ban đêm:** Cửa tiệm tỏa sáng làm mốc định hướng; không lạm dụng Bloom gắt hoặc sương mù đặc che khuất bảng giá/hiện vật.
- **Giới hạn kỹ thuật:** Đèn động có đổ bóng chỉ dùng ở các điểm trọng yếu; dùng chung Trim Sheet / vật liệu `Mat_*` cho kiến trúc modular; phân tầng chi tiết cao nhất cho cổ vật cầm soi gần.

---

## 5. Tỷ lệ First-Person & Mật độ Đồ vật

- **Nhân vật:** Cao `1.75–1.80 m`, tầm mắt `1.62 m`.
- **Cửa chính cửa tiệm:** Rộng tối thiểu `3.4–4.2 m`, cao `3.2–3.8 m` để bước vào mượt và quan sát ngay chức năng bên trong từ ngoài phố.
- **Lối đi nội thất:** Trục chính `2.2–3.5 m`, khoảng trống quanh bục hộp/bàn tương tác tối thiểu `1.5 m`, sau quầy NPC rộng `2.2–2.8 m`.
- **Quy tắc mật độ 60 / 30 / 10:** `60%` không gian kết cấu & lối đi thoáng sạch; `30%` nội thất nghề nghiệp & đồ trưng bày trên kệ/sát tường; `10%` dành cho Hero Asset trung tâm.

---

## 6. Những điều Cần Tránh (Ví dụ Đúng vs. Sai)

- **Sai:** Đặt sạp gỗ nhỏ ngoài hiên chắn trước cửa kính để bày 4 cái hộp → **Đúng:** Xóa sạp ngoài hiên, bước vào đại sảnh **Nhà Niêm Phong** có 6 bục hộp mù riêng biệt dưới hệ ròng rọc lớn và tủ hòm mở nắp sau quầy Dì Mận.
- **Sai:** Làm 4 ngôi nhà có mặt bằng hình chữ nhật `6m × 5m` giống hệt nhau → **Đúng:** Mỗi tiệm có mặt bằng `15–26m`, hình khối mái – cửa sổ – vật liệu – bố cục nội thất phân hóa hoàn toàn theo nghề nghiệp.
- **Sai:** Nhồi tràn lan đồ trang trí ra giữa lối đi → **Đúng:** Dồn kệ kho, bình gốm, tủ đồ sát tường hoặc lên gác lửng để chừa trục nhìn thẳng vào vật phẩm gameplay.

# Bước ngoặt: game hành động cypherpunk (hack and slash)

Ngày: 2026-11-26. Trạng thái: **ĐỀ XUẤT, chờ chủ dự án duyệt.** Chưa việc nào được bắt đầu cho tới khi duyệt.

Mục nào ghi **(quyết định của chủ dự án)** là điều chủ dự án đã nói trong buổi trao đổi. Mục ghi **(giả định của tôi)** là chỗ tôi điền vào, cần được xác nhận hoặc sửa.

## 1. Ý định

Chuyển CHỢ ĐỒ LẠ từ game săn hộp mù và trả nợ sang **game hành động roguelite góc nhìn thứ ba qua vai**, bối cảnh cypherpunk. Chủ dự án muốn game có chiều sâu và nhiều hành động hơn, chơi theo kiểu "hack and slash": vừa chém vừa hack, cơ thể nâng cấp được. **(quyết định của chủ dự án)**

Bản đồ đã đổi sang cypherpunk. Tầng trên là thành phố của robot. Khu ổ chuột dưới đất là nơi người chơi sống. **(quyết định của chủ dự án)**

## 2. Vòng chơi một ngày (quyết định của chủ dự án)

| Buổi | Nơi | Việc người chơi làm |
|---|---|---|
| Sáng | Tầng hầm, cứ điểm dưới lòng đất | Nâng cấp cơ thể, lắp thêm bộ phận công nghệ, chuẩn bị cho đêm |
| Tối | Mặt đất | Chiến đấu với robot, những kẻ đã nô dịch con người |

Chiến lợi phẩm của đêm là vật liệu và linh kiện để nâng cấp ở hầm vào sáng hôm sau.

## 3. Cảm giác chiến đấu (quyết định của chủ dự án)

Kết hợp hai game tham chiếu:
- **Cyberpunk 2077:** chiều sâu build. Bộ phận cơ thể và kỹ năng hack thay đổi cách chơi.
- **Ghostrunner:** nhịp nhanh, gọn. Đòn trúng là đau, né và lướt phải đúng lúc, chết nhanh (cả người chơi lẫn robot).

Nhân vật chạy bằng ba thứ: **chém**, **hack** và **bộ phận cơ thể**.

### Chém (giả định của tôi)
Cận chiến bằng vũ khí hoặc tay cơ khí. Có đòn nhẹ, đòn nặng, né, đỡ đúng lúc và lướt. Đòn đỡ đúng lúc làm lộ sơ hở cho robot.

### Hack (giả định của tôi)
Ngắm vào robot rồi nhấn phím hack: chiếm quyền, làm đứng yên, cho quay sang đánh đồng đội hoặc nổ. Hack tốn năng lượng và có thời gian hồi, nên không thể thay hoàn toàn việc chém.

### Bộ phận cơ thể (giả định của tôi)
Có năm vị trí lắp: **tay, chân, mắt, lưng, não**.

| Vị trí | Mở ra điều gì |
|---|---|
| Tay | Vũ khí, combo, lưỡi dao, khiên |
| Chân | Lướt, nhảy, né, đạp |
| Mắt | Quét, hack tầm xa, nhìn xuyên vật cản |
| Lưng | Bộ nguồn, giáp, vũ khí đeo |
| Não | Số kỹ năng hack, năng lượng, tốc độ hồi |

Năm vị trí thay thế "4 ô tạo tác" trong spec rương cũ. Con số này cần Claude 2 kiểm tra cân bằng.

## 4. Rương trong game mới (quyết định của chủ dự án)

Rương **không còn là lõi game**. Chỉ giữ các rương chủ đề có tính chiến đấu, với hai vai trò:
- **Mục tiêu chiến đấu:** một đám robot hoặc một trùm canh rương.
- **Rương thưởng:** hạ được mục tiêu thì mở rương, phần thưởng là linh kiện hoặc bộ phận cơ thể hiếm.

Mô hình, hiệu ứng mở và âm thanh đã dựng được tái dùng cho phần thưởng.

### Rương nào ở lại (giả định của tôi, chủ dự án chưa chọn)
- **Hợp mục tiêu chiến đấu:** Robot, Miner, Hải Tặc, Mimic, Game, Không gian.
- **Hợp làm phong cách khu vực hơn:** Âm nhạc, Tuổi thơ, Thể thao, Việt Nam, Trung Hoa, Nhật Bản, Cổ Phong, Thiên Cơ, Golden, Magic, Rác Quý.

Danh sách cuối cùng chờ chủ dự án chọn.

## 5. Phạm vi vertical slice (giả định của tôi)

Một bản chơi thử nhỏ nhất cho thấy được vòng chơi:
- Một hầm với bàn nâng cấp, năm vị trí cơ thể, mỗi vị trí 2 bộ phận.
- Một khu mặt đất.
- Ba loại robot và một trùm.
- Một rương mục tiêu, một rương thưởng.
- Chơi được: sáng nâng cấp, tối chiến đấu, quay về hầm với phần thưởng, thấy nhân vật mạnh hơn.

## 6. Cấu trúc ván chơi

**Không giới hạn số ngày, chơi kiểu roguelike. (quyết định của chủ dự án)** Bỏ nhịp 7 ngày của game cũ.

Hệ quả, là giả định của tôi cần xác nhận:
- Mỗi ván kéo dài đến khi người chơi **chết**. Chết thì ván kết thúc và bắt đầu ván mới.
- Độ khó **tăng theo số đêm đã sống sót**, thay cho hạn 7 ngày. Robot mạnh dần và mặt đất nguy hiểm dần.
- Có **tiến trình giữa các ván** (meta): bộ phận cơ thể đã mở khóa, kỹ năng hack mới và rương đã gặp. Cái gì giữ lại và cái gì mất khi chết cần chốt.
- Vẫn có **trùm** sau một số đêm nhất định để tạo đích đến ngắn hạn. Cần chốt trùm cuối có hay không.
- Vòng chơi một ngày (mục 2) giữ nguyên: sáng ở hầm nâng cấp, tối lên mặt đất chiến đấu.

Còn mở: chết thì giữ gì và mất gì, có trùm cuối hay chơi vô tận.

## 7. Mục tiêu, không phải mục tiêu, phản mục tiêu

- **Mục tiêu:** chiến đấu nhanh và rõ nghĩa. Mỗi bộ phận cơ thể thay đổi cảm giác chơi một cách nhìn thấy được.
- **Không phải mục tiêu:** làm lại hệ rương mù, nợ Anh Bảy, soi lỗi, trả giá, bậc đồ trong rương. Lồng tiếng. Dùng credit dịch vụ trả phí khi chưa được cho phép.
- **Phản mục tiêu (nếu xảy ra thì thất bại dù chạy được):** hack quá mạnh đến mức bỏ qua chém. Kẻ địch lặp lại nhàm. Nhân vật không đổi gì nhìn thấy sau khi nâng cấp.

## 8. Ràng buộc kỹ thuật

- Unity 6 + URP, low-poly màu trung tính, thêm ánh sáng neon cho cypherpunk (theo hướng nghệ thuật đã chốt).
- Góc nhìn thứ ba qua vai. Mã người chơi góc nhìn thứ nhất (`Player`, `HeldItemViewmodel`) không dùng cho chiến đấu. Cần nhân vật có bộ xương, hoạt ảnh và camera qua vai.
- Nhân vật phải chia theo bộ phận để hiển thị phần gắn thêm.
- Dự án chưa có bản chạy được (xem `TASKS.md`), nên rủi ro tích hợp cao.

## 9. Các hướng đã cân nhắc

| Hướng | Mô tả | Kết quả |
|---|---|---|
| A | Ngày chia hai nửa, sáng săn đồ, tối chợ | Bỏ: chủ dự án muốn đổi cách chơi lớn hơn |
| B | Hack nằm trong chiến đấu thuần hành động | Gần với hướng đã chọn |
| C | Chợ là cứ điểm, hành động là chuyến đi | Gần với hướng đã chọn |
| **Đã chọn** | Hầm là cứ điểm nâng cấp cơ thể, mặt đất là chiến đấu, rương chủ đề là mục tiêu và thưởng | Chủ dự án chọn |

## 10. Tài liệu bị thay thế nếu duyệt

Sau khi duyệt tôi sẽ đánh dấu "đã thay thế bởi spec này" ở các tài liệu về nợ, soi lỗi, trả giá, bậc đồ, mở màn và một ngày. Phần rương chủ đề và hiệu ứng mở rương giữ nguyên.

## 11. Việc để người làm quyết định

- Chọn tên và chỉ số cụ thể cho từng bộ phận cơ thể và từng loại robot.
- Chọn tên khu mặt đất.
- Cách điều khiển chi tiết (phím, tay cầm).

# Mini-game buổi sáng — đánh răng và ăn sáng

**Trạng thái: tạm duyệt để thử thao tác.** Đây là thiết kế cho hai mini-game đầu tiên. Tiến trình thành tựu và Cây Phát Triển để lại sau khi vòng chơi chính ổn định.

Thiết kế này bổ sung [nhịp một ngày](2026-11-23-mot-ngay-design.md) và chuyển bữa sáng từ phòng trọ ra các quán trong chợ theo yêu cầu mới. Luồng đề xuất là đánh răng tại phòng trọ, chọn quán ăn sáng, rồi tiếp tục vòng săn hộp. Ngày 1 giới thiệu đầy đủ. Từ ngày 2, từng hoạt động có thể chơi đầy đủ, làm nhanh hoặc bỏ qua. Kết quả đổi hình ảnh, âm thanh và lời thoại. Giá, tiền, nợ và xác suất rơi giữ theo luật gameplay.

## Đánh răng tại phòng trọ

1. Tương tác với bồn rửa, chọn bàn chải và tuýp kem riêng. Mẫu để thử gồm bàn chải thường, đầu to, hình cá sấu và kem bạc hà, dâu, trà đá. Hình dáng và âm chà theo bàn chải. Màu bọt và phản ứng sau khi súc miệng theo kem.
2. Camera tiến gần gương. Người chơi giữ chuột trái và rê bàn chải qua ba vùng răng. Chuyển động chà qua vết bám làm nó mờ dần và tạo bọt. Giữ yên bàn chải không tăng tiến độ.
3. Khi sạch cả ba vùng, nhân vật súc miệng. Rời bồn sớm vẫn tiếp tục được buổi sáng, kèm câu thoại tùy số vùng đã sạch.

Vết bẩn thường là phần chính. Điểm xuất phát để thử là khoảng **10% lượt đánh răng có một vết bám lạ**, tối đa một hình mỗi lượt. Hình dùng sprite 2D trên răng. Tất cả dùng thao tác chà chung.

| Hình 2D hiếm | Phản ứng khi chà |
| --- | --- |
| Mèo cuộn tròn | Bám mép răng, kêu meo rồi trượt xuống theo bọt |
| Tảng đá | Xuất hiện vết nứt rồi vỡ thành vài mảnh |
| San hô | Rung nhẹ như gặp sóng, sau đó trôi đi |
| Dép tổ ong | Xoay một vòng rồi bị bọt cuốn đi |

Mục tiêu sau lần hướng dẫn là 10–15 giây cho phần chà. Cần thử độ nhạy rê chuột và tần suất hình hiếm.

## Ăn sáng tại chợ

1. Người chơi tự chọn quán và gọi một món trong menu. Hai quán đề xuất cho bản thử là quán phở (phở bò, phở gà) và quán xôi (xôi gà, xôi chà bông).
2. Chủ quán mang bát hoặc đĩa ra, đặt trước mặt người chơi. Có tiếng bát chạm bàn và một câu thoại ngắn. Chỉ bắt đầu mini-game sau khi món đã đặt xuống.
3. Gắp hoặc xúc từng miếng, rê dụng cụ lên vùng miệng có phạm vi rộng. Có thể thêm ớt, vắt chanh, thổi miếng nóng hoặc uống nước. Rê dụng cụ là phương án thử đầu tiên. Nếu thao tác trở nên lặp và chậm, thử bấm chọn miếng để nhân vật tự gắp.
4. Bản thử dùng 3–4 miếng mỗi món. Mục tiêu là 10–15 giây cho phần ăn, tính riêng thời gian gọi và phục vụ.

| Tình huống | Phản ứng |
| --- | --- |
| Quá cay | Viền màn hình đỏ, camera rung nhẹ một nhịp, nhân vật há miệng thở hà hà. Chủ quán đẩy cốc nước tới. Uống nước làm hiệu ứng dịu nhanh. |
| Còn nóng | Hơi nước phả gần camera, nhân vật giật nhẹ và xuýt xoa. Thổi trước miếng tiếp theo giúp bớt nóng. |
| Vừa miệng | Tiếng nhai hoặc tiếng húp theo món, nhân vật thở ra khoan khoái. Chủ quán có thể hỏi “Mai ăn như cũ nhé?” |

Giữ phản ứng ngắn để người chơi vẫn điều khiển được. Nếu tắt rung camera, phản ứng cay vẫn đọc được qua viền đỏ, âm thanh và phụ đề.

## Phạm vi thử

- Thử chọn và đổi dụng cụ trước khi đánh răng, chà vết bẩn thường và thử cưỡng bức một hình hiếm để kiểm tra phản ứng.
- Thử cả hai quán. Kiểm tra menu, đúng món được phục vụ, ăn từng miếng, ăn cay và uống nước.
- Thử ngày đầu và một ngày sau với thao tác chơi nhanh hoặc bỏ qua. Kiểm tra người chơi vẫn vào được vòng săn hộp.
- Tỉ lệ hình hiếm, lựa chọn quán và chi tiết thao tác ăn được điều chỉnh sau Play Mode.

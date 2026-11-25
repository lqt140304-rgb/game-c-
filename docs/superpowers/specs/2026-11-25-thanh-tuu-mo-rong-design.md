# Thành tựu mở rộng — CHỢ ĐỒ LẠ

Trạng thái: thiết kế để duyệt trước khi triển khai. Sáu thành tựu đã có trong Điện thoại giữ nguyên. Tên mốc đổi combo là **Hải quay xe**, tên mốc ngủ lần đầu là **Một giấc mộng đẹp**. Đã loại Rẻ hơn lời đồn, Hỏi cho chắc và Suýt mất ngủ khỏi danh sách.

## Luật tính và hiển thị

Điện thoại → Thành tựu hiển thị icon, tên và tiến độ. Các mốc ẩn chỉ hiện sau khi đạt. Khi đạt, game lưu tiến độ cùng giao dịch liên quan rồi phát một thẻ ở góc trên bên phải. Nếu một giao dịch mở nhiều mốc, xếp thẻ theo thứ tự danh sách thành tựu và phát lần lượt, khoảng 3 giây mỗi thẻ. Thẻ không chặn chuột hay điều khiển. Tải lại không phát lại thẻ cũ.

Mỗi mốc chỉ mở một lần trên hồ sơ. Tiến độ cần tính theo ngày hoặc theo ván nằm trong bản tự lưu và đặt lại đúng khi sang ngày hoặc ván. Cây Phát Triển, phần thưởng kinh tế và thành tựu mini-game thuộc các đợt sau.

## Các mốc bổ sung

| Thành tựu | Điều kiện đo được | Thời điểm phát thẻ |
| --- | --- | --- |
| Hàng để dành | Mở hộp vào đúng ngày kế tiếp ngày mua. Dùng mã cá thể hộp để gắn đúng lần mua. | Khi mở hộp thành công. |
| Vừa khít · ẩn | Trả hết số đến hạn của ngày, gồm nợ tồn và khoản chuyển kỳ nếu có. Ví còn đúng 0 xu và không hoãn kỳ hiện tại. | Khi chốt nợ thành công. |
| Hẹn mai trả | Hoãn kỳ bằng GT1 rồi trả đủ tổng đến hạn ngày kế tiếp, gồm khoản đã hoãn. | Khi thanh toán ngày kế tiếp. |
| Mở niêm phong | Đạt BK1 và mở ô Tạo Tác thứ 5. | Khi thao tác trang bị mở BK1. |
| Hai hệ, một bộ | Cùng lúc đạt ít nhất mốc 2 Cổ Phong và mốc 2 Thiên Cơ. | Khi trang bị làm đủ cả hai hệ. |
| Một giấc mộng đẹp | Thức dậy ở phòng trọ sau lần ngủ đầu tiên dẫn đến sáng hôm sau. | Khi cảnh thức dậy hoàn tất. Cần giường và luồng thức dậy gắn vào map. |
| Biết lúc dừng | Chọn Dừng mua sau khi mua đúng một hộp trong ngày và trả hết số đến hạn hôm đó, không hoãn kỳ. | Khi chốt nợ. Cần nút Dừng mua trên UI quầy. |
| Tay trắng về nhà | Trả hết số đến hạn và bàn mở cùng kho hiện vật đều rỗng lúc chốt nợ. Tạo Tác không tính vào kho. | Khi chốt nợ. |
| Mở một, sống một ngày | Chỉ mở một hộp trong ngày. Bán hiện vật từ hộp đó cùng ngày và tiền thực nhận ít nhất bằng kỳ gốc của ngày. | Khi chốt ngày để xác nhận tổng số hộp đã mở. |
| **Hải quay xe** | Trong cùng một ngày, combo A từng bật rồi bị tắt, sau đó người chơi kích hoạt combo B khác A. Combo B phải mới bật sau lúc A tắt hoặc trong cùng lần đổi. Thêm B khi A vẫn bật thì chưa tính. Có thể tháo rồi gắn qua nhiều thao tác. | Khi combo B kích hoạt. |
| Đồng nào ra đồng nấy | Thắng sau 7 kỳ trả đủ đúng ngày, không có kỳ thiếu hoặc hoãn. | Khi ghi nhận thắng ván. |
| Ba ngày một món quen · ẩn | Mở ra cùng mã hiện vật ở ba ngày khác nhau trong một ván, không cần liên tiếp. | Khi mở đúng món vào ngày thứ ba. |
| Khách cuối phiên | Hoàn tất mua hộp, mua Tạo Tác, mua lời mách hoặc bán hiện vật trong 15 giây cuối chợ hoạt động. Chỉ tính khi đồng hồ đang bật. | Khi giao dịch thành công. Cần map đã bật đồng hồ. |

## Ca kiểm tra khi triển khai

- Hải quay xe: tắt CB1 buổi sáng rồi bật CB2 cùng ngày thì đạt. Mở đủ CB2 trong khi CB1 vẫn bật, hoặc bật CB2 sang ngày khác, thì chưa đạt.
- Vừa khít: ví 200 xu với kỳ đến hạn 200 xu và không nợ tồn thì đạt. Còn 0 xu vì chỉ trả được một phần thì chưa đạt.
- Ba ngày một món quen: ba bản sao cùng ngày chỉ tính một ngày. Tải lại sau khi mở không cộng thêm.
- Khách cuối phiên: giao dịch trước cửa sổ 15 giây hoặc bị cửa hàng từ chối đều không tính.
- Hai mốc cùng đạt trong một giao dịch phải cùng được lưu và hiện thành hai thẻ liên tiếp. Nếu ghi bản lưu thất bại, không hiện thẻ nào.

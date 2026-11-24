# Rương Rác Quý: ý tưởng hình dáng (bản dựng thử)

Ngày: 2026-11-24. Trạng thái: **dựng thử**, chưa gán ID hay chủ đề vào dữ liệu game, chủ dự án chưa duyệt.

Rương Rác Quý ra "rác quý" (giày cũ, tất thối, trứng thối, xương cá, dép tổ ong, lon bẹp, áo đấu cũ, bao thuốc rồng, vé cược nhàu) cộng chút bất ngờ. Tinh thần: rẻ, bẩn, hài, nhưng có gu. Yêu cầu chính: hình bóng **không phải hộp vuông/chữ nhật** như H1, H2, H3, RD và rương Meshy.

## Ba phương án hình bóng

1. **Thùng rác tròn có nắp vòm**: trụ tròn loe miệng, hai đai nổi, nắp vòm có quai, thân móp một bên; nhìn xa là một khối "lọ hoa ngược" có chóp, không có góc vuông nào.
2. **Thùng phuy gỉ thấp**: trụ phình bụng, ba đai, nắp phẳng có lỗ rót; hình bóng tròn mập, nhưng nhìn ngang dễ thành hình chữ nhật bo góc.
3. **Bao tải buộc miệng**: túi phình hình giọt nước, cổ thắt dây; hình bóng rất khác, nhưng không có nắp cứng xoay quanh bản lề, phải đổi cách mở và sửa `BoxOpenSequence`.

## Chọn phương án 1: thùng rác tròn

- Hình bóng khác hẳn: thân côn (đáy hẹp, miệng loe), nắp vòm với quai nhô lên, tai cầm hai bên. Nhìn ngang ra hình thang ngược có chóp, không thể nhầm với hộp.
- "Có gu" nhờ chi tiết kể chuyện chứ không nhồi đồ: thân móp một vết, băng dính bạc dán chéo ngang miệng như vá víu, chiếc tất thối vắt qua vành, xương cá thò ra mép, tem giấy "QUÝ" dán lệch như hàng hiệu rởm.
- Màu trung tính: xanh rêu xỉn của thùng rác nhựa công cộng, đai sẫm hơn, băng dính xám bạc, tất trắng ngà ố.
- Giữ được cấu trúc hiện có, không cần sửa C#.

## Cách nắp mở

- Nắp vòm xoay quanh **bản lề phía sau** (trục X, mặt trước là -Y), giống thùng rác nắp lật. Có bản lề nhỏ dựng ở sau lưng thân để khớp hình.
- `Binding` = hai dải băng dính bạc dán từ nắp xuống thân ở mặt trước: giai đoạn "Gỡ niêm" làm băng dính bung ra và rơi.
- `Seal` = chiếc tất thối vắt qua vành phía trước: rơi xuống cùng lúc (tất là "niêm phong" của rương rác).

## Hiệu ứng mở đặc trưng

- `Glow` không phải quầng vàng mà là **làn hơi xanh vàng bốc lên** (vài cục khói low-poly) **kèm ba con ruồi** bay lơ lửng trên miệng. Tất cả nằm chung trong lớp `Glow`, nên khi `BoxOpenSequence` phóng to Glow theo giai đoạn thì hơi và ruồi "nở" ra từ miệng thùng mà không cần mã mới.
- Gợi ý (chưa làm): âm thanh nắp tôn kêu "kẹt", tiếng ruồi vo ve ở `onStageReached(3)`, hạt ruồi bay thật nếu sau này muốn.

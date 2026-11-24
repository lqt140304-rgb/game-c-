# Thiết kế: hình dáng Rương Âm nhạc (dựng thử bằng Blender)

Trạng thái: **bản thử của agent, chưa được chủ dự án duyệt**. Chưa gán ID hay chủ đề vào dữ liệu game. Đồ trong rương xem [ruong-chu-de](2026-11-24-ruong-chu-de-design.md) (đĩa than, harmonica, micro cổ, kính tròn tứ quái, đàn bầu, ghi-ta đập vỡ, găng tay lấp lánh một chiếc).

## Yêu cầu

Hình bóng nhìn từ xa phải khác hẳn các rương hình hộp (H1, H2, H3, RD, Golden) và nhận ra ngay là âm nhạc. Tránh kiểu hộp vuông dán nốt nhạc.

## Ba phương án hình bóng

1. **Máy hát tự động (jukebox) mini mái vòm**: nhìn chính diện là hình bia mộ đầu tròn, hai cột đèn neon chạy dọc thân rồi uốn theo mái vòm kính, bên dưới là lưới loa.
2. **Đàn piano cánh mini**: thân cong hình cánh, nắp lớn chống nghiêng một bên, ba chân mảnh.
3. **Máy hát đĩa loa kèn (gramophone)**: đế gỗ thấp, cái loa kèn hình hoa loe to vươn lên chéo, chiếm gần hết hình bóng.

## Chọn: phương án 1, jukebox mini mái vòm

- **Hình bóng mạnh nhất từ chính diện**: đỉnh tròn hình vòm và hai cột neon cong là thứ không rương nào đang có; nhìn xa vẫn ra "máy nhạc", không cần ký hiệu nốt nhạc.
- **Mở nắp hợp với `BoxOpenSequence`**: mái vòm là một nửa trụ nằm dọc trước sau, bản lề ở mép sau, lật ngửa ra sau quanh trục X, giống hệt cách các rương khác mở. Piano cánh mở theo trục dọc (bên trái), phải đổi trục; gramophone thì cái loa kèn không phải "nắp".
- **Tinh thần sân khấu, Gen Z hài**: jukebox là "DJ bỏ xu" của quán nhậu đời cũ, có chỗ cho nút chọn bài, khe bỏ xu, đèn neon. Bên trong là đĩa than (món Thường của rương) nằm sẵn trên mâm quay.
- Màu: gỗ óc chó và kem trung tính, điểm nhấn **neon hồng cánh sen và xanh ngọc** (đèn sân khấu), crom xám.

## Cách mở và hiệu ứng riêng

1. **Gỡ niêm**: niêm là một **miếng gảy đàn (pick) đỏ** dán đè lên đường nối vòm và thân (`Seal`).
2. **Gỡ dây**: một **dây đeo ghi-ta có khóa cài** vắt qua mái vòm từ trước ra sau (`Binding`), bật ra như dây đàn đứt.
3. **Mở nắp**: mái vòm kính lật ngửa ra sau, lộ **đĩa than nằm trên mâm** và cần đọc đĩa.
4. **Hiệu ứng đặc trưng**: ngay khi nắp mở, **đĩa than bắt đầu quay** (object `Record`, quay quanh trục thẳng đứng, tốc độ theo độ sáng `glow`), quầng sáng ấm bật lên như đèn sân khấu. Gợi ý gắn trong Unity: âm thanh "kim chạm đĩa" (tiếng xẹt) ở giai đoạn mở, rồi vài nhịp nhạc.

Đĩa quay là tùy chọn **mặc định tắt** thêm vào `BoxOpenSequence` (`spinRecord`), không đổi hành vi các rương khác.

## Cấu trúc xuất

`Assets/_Project/Art/Chests/Chest_AmNhac.glb`, dựng bằng `Tools/Blender/make_chest_am_nhac.py`. Root `Chest_AmNhac`, con: `Body`, `Lid` (gốc ở bản lề sau), `Seal`, `Binding`, `Record` (gốc ở tâm đĩa), `Glow`. Chi tiết số tam giác ở [BanGiao3D](../../../Docs/Art/BanGiao3D.md).

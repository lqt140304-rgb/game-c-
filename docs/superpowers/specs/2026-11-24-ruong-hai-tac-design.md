# Thiết kế: rương Hải tặc (mô hình 3D)

Trạng thái: **bản dựng thử bằng Blender, chờ chủ dự án xem**. Chưa gán ID hay chủ đề vào dữ liệu game. Đồ trong rương xem [ruong-chu-de](2026-11-24-ruong-chu-de-design.md) (la bàn, đồng xu vàng, móc tay, cờ đầu lâu, bản đồ kho báu, trái ác quỷ nhại, mũ rơm nhại).

Yêu cầu gu: **không** dáng hộp vuông/chữ nhật (khác hẳn H1, H2, H3, RD và Chest_Golden), nhìn từ xa phải nhận ra ngay là Hải tặc, tránh rương gỗ đai sắt.

## Ba phương án hình bóng

1. **Thùng rum có vòi**: thùng tròn phình giữa nằm ngang trên giá chữ X, nắp là đáy thùng bật ra như cửa sổ tàu. Tròn và lạ, nhưng nhìn xa dễ lẫn với thùng rượu quán, chất "hải tặc" phải nhờ chi tiết nhỏ.
2. **Vỏ sò khổng lồ**: con sò quạt to có gân, nửa trên há ra sau như nắp. Mở rất tự nhiên, nhưng đọc ra là "biển, nàng tiên cá" hơn là hải tặc.
3. **Đầu lâu thuyền trưởng**: hộp sọ đội mũ ba góc, bịt một mắt, răng vàng, ngồi trên đống vàng đặt trên hai xương bắt chéo. Hình bóng là chính lá cờ Jolly Roger dựng thành khối.

## Chọn: phương án 3, "Đầu lâu thuyền trưởng"

- **Hình bóng không thể nhầm**: khối tròn của sọ, mũ ba góc nhô ba mũi, hai xương chéo bè ra dưới đáy. Không có cạnh thẳng đứng nào kiểu hộp. Đây là biểu tượng hải tặc ai cũng biết.
- **Hài đúng giọng Gen Z**: mở rương là "mở não" ông thuyền trưởng. Nắp sọ cùng cái mũ lật ra sau như nắp hũ kẹo; ổ khóa treo ngay giữa trán; một mắt bịt, mắt kia lóe vàng; một chiếc răng vàng.
- **Ăn với đồ bên trong**: đống đồng xu vàng ở đế, lá cờ đầu lâu, mũ (nhắc mũ rơm Huyền thoại).

## Nắp mở và hiệu ứng đặc trưng

- **Đường cắt** chạy ngang trán, ngay trên gờ lông mày. **Nắp** gồm đỉnh sọ và cả cái mũ ba góc, **bản lề ở sau gáy**, xoay quanh trục X; mặt trước là -Y.
- **Binding**: vòng dây thừng quấn ngang đường cắt, buộc nút ở gáy, có ổ khóa đồng treo giữa trán. **Seal**: dấu sáp đỏ in hình đầu lâu dán trên dây ở bên trái trán. **Key**: chìa khóa sắt đã cắm sẵn vào ổ (thân chìa dọc trục Y Blender), để `BoxOpenSequence` chạy đoạn chìa bay tới, cắm, xoay.
- **Glow**: quầng vàng ấm trào lên từ hộp sọ, kèm vài đồng xu vàng nằm trong quầng nên "phồng" lên cùng ánh sáng (lớp Glow ẩn bằng scale, mở là xu và ánh vàng cùng nở ra).
- **Nhịp mở** (dùng 5 giai đoạn có sẵn, không sửa C#): Rung (sọ lập cập), Gỡ niêm (dây và ổ khóa rơi khỏi trán, sáp rơi), Hé nắp (khe sáng vàng dưới vành mũ), Bật nắp, Mở toang (mũ lật hẳn ra sau, xu vàng nổi lên).
- **Ý tưởng để sau, chưa làm**: hàm dưới tách thành lớp riêng để "cười hô hố" khi rung; tiếng sóng và tiếng hải âu gắn vào `onStageReached`. Cần chủ dự án duyệt mới thêm.

## Kỹ thuật

- Dựng hoàn toàn bằng `Tools/Blender/make_chest_hai_tac.py`, không dùng Meshy.
- Low-poly, vật liệu đơn sắc: xương ngà, mũ đen nhạt, vàng làm điểm nhấn, dây thừng, đồng, sáp đỏ. Mục tiêu 1.000 đến 4.000 tam giác, cao khoảng 0,45 m.
- Xuất `Assets/_Project/Art/Chests/Chest_HaiTac.glb`, root `Chest_HaiTac` với các lớp `Body`, `Lid`, `Seal`, `Binding`, `Glow`, `Key`.

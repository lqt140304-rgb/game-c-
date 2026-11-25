# Nội dung đã chốt

Mục lục mọi quyết định chủ dự án đã chốt. Chi tiết nằm ở file được trỏ tới. Mỗi khi chốt điều mới: ghi vào spec tương ứng (hoặc tạo spec mới trong `docs/superpowers/specs/`) rồi thêm một dòng ở đây.

## Thiết kế (spec)

| Chủ đề | File | Điểm chính đã chốt |
|---|---|---|
| Rương, rác, sưu tập, thần lực | [ruong-suu-tap](../docs/superpowers/specs/2026-11-23-ruong-suu-tap-design.md) | 4 rương cơ bản; rác có công dụng ẩn; 4 ô tạo tác; cây phát triển không buff chơi |
| Mở màn | [mo-man](../docs/superpowers/specs/2026-11-23-mo-man-design.md) | Giọng Gen Z; Anh Bảy là giang hồ mạng; ngày 1 không thua ngay; ngày 1 của lần chơi đầu chỉ bán Rương Gỗ và Rương Vải Niêm |
| Một ngày | [mot-ngay](../docs/superpowers/specs/2026-11-23-mot-ngay-design.md) | Mở rương là buổi sáng; tình huống đời thường hài |
| Thần lực | [than-luc](../docs/superpowers/specs/2026-11-23-than-luc-design.md) | Chỉ phản ứng dí dỏm; chữ chạy kiểu light novel, không lồng tiếng |
| Soi lỗi và trả giá | [soi-loi-tra-gia](../docs/superpowers/specs/2026-11-23-soi-loi-tra-gia-design.md) | Soi lỗi lấy bằng chứng, trả giá kiểu Ace Attorney; biến thể chất lượng là lớp phủ, không làm mô hình riêng; người chơi tự tìm chỗ lỗi, chỉ di chuột đúng chỗ mới hiện dòng chữ ngắn ghi lỗi gì, bấm vào thì giá trừ theo lỗi; bảng món đồ cho chọn thật hay giả và hiện giá trị, thông tin cơ bản (chốt 2026-11-24) |
| Bậc đồ trong rương | [tier-do-trong-ruong](../docs/superpowers/specs/2026-11-23-tier-do-trong-ruong-design.md) | 5 bậc: Rác, Thường, Hiếm, Rất hiếm, Huyền thoại |
| Giá và tỉ lệ 4 rương cơ bản | [tier-do-trong-ruong](../docs/superpowers/specs/2026-11-23-tier-do-trong-ruong-design.md) mục 3, số đo ở [Sprint5Ruong](Balance/Sprint5Ruong.md) | Gỗ 1.400.000 đ (có Rất hiếm 1%), Vải Niêm 3.500.000 đ (rác 30%), Kim Khí 4.500.000 đ (ra cả đồ lẫn 34% tạo tác; tạo tác trùng đổi 50% giá Kho), Đen giữ 12.000.000 đ (rác 30%, Rất hiếm 58%); Huyền thoại có ở rương cơ bản, hiếm |
| Rương chủ đề | [ruong-chu-de](../docs/superpowers/specs/2026-11-24-ruong-chu-de-design.md) | Mở bằng thành tựu, có trong shop từ hôm sau; quay bậc rồi quay món; mỗi món thuộc một rương, Rác dùng chung; Rương Tổng Hợp ra đồ rương cơ bản; Huyền thoại chủ đề là biểu tượng có câu chuyện; nhại được, chỉ cần đổi nhỏ |
| Tiền tệ | [tien-te](../docs/superpowers/specs/2026-11-24-tien-te-design.md) | Tiền là đồng Việt Nam (đ); 1 xu cũ = 10.000 đ; vốn 10.000.000 đ, nợ 7 kỳ 2 đến 22 triệu đ; bước làm tròn nhỏ nhất 10.000 đ |
| Hiệu ứng mở rương (**ĐỀ XUẤT, chờ duyệt**) | [hieu-ung-mo-rong](../docs/superpowers/specs/2026-11-24-hieu-ung-mo-rong-design.md); spec riêng từng rương `docs/superpowers/specs/2026-11-24-hieu-ung-*-design.md` | Đã có mã thử: màu các bậc nhảy như vòng quay rồi dừng đúng bậc, bóng món trúng nhô ra to dần; mỗi rương một kiểu hiệu ứng và âm riêng. **Bảng màu bậc, nhịp giây và âm thanh là ĐỀ XUẤT, chờ duyệt, chưa ai nghe âm.** Chưa chốt điều gì ở mục này |

## Mỹ thuật 3D

| Chủ đề | File | Điểm chính đã chốt |
|---|---|---|
| Bàn giao mô hình | [BanGiao3D](Art/BanGiao3D.md) | Low-poly, màu trung tính; bậc cao chi tiết hơn; Meshy gộp tối đa 8 món một lượt; món lỗi gom làm lại một đợt, không làm riêng lẻ; Gậy sắt làng Gióng để thẳng |
| Rương đã duyệt (2026-11-24) | [DanhSachRuong](Art/DanhSachRuong.md) | Chủ dự án đã duyệt mô hình từng rương; GLB ở `Assets/_Project/Art/Chests/`, cách dựng ở [BanGiao3D](Art/BanGiao3D.md). Meshy (cắt nắp từ FBX): Golden (`Chest_Golden.glb`, không có spec riêng); Magic (`Chest_Magic.glb`, không có spec riêng); Mimic (`Chest_Mimic.glb`, không có spec riêng); Miner (`Chest_Miner.glb`, không có spec riêng). Dựng bằng Blender: Rác Quý (`Chest_RacQuy.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-rac-quy-design.md)); Hải tặc (`Chest_HaiTac.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-hai-tac-design.md)); Âm nhạc (`Chest_AmNhac.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-am-nhac-design.md)); Việt Nam (`Chest_VietNam.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-viet-nam-design.md)); Tuổi thơ (`Chest_TuoiTho.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-tuoi-tho-design.md)); Robot (`Chest_Robot.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-robot-design.md)); Không gian (`Chest_KhongGian.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-khong-gian-design.md)); Game (`Chest_Game.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-game-design.md)); Thể thao (`Chest_TheThao.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-the-thao-design.md)); Trung Hoa cổ (`Chest_TrungHoa.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-trung-hoa-design.md)); Nhật Bản cổ (`Chest_NhatBan.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-nhat-ban-design.md)); Cổ Phong (`Chest_CoPhong.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-co-phong-design.md)); Thiên Cơ (`Chest_ThienCo.glb`, [spec](../docs/superpowers/specs/2026-11-24-ruong-thien-co-design.md)). **Chưa gán ID chính thức hay chủ đề vào dữ liệu game** (`Docs/Content/data/content.json`). |

## Quy tắc làm việc

- Không dùng credit Meshy khi chủ dự án chưa cho phép; báo ước tính trước. Không tự quyết thay chủ dự án.
- Xem mục "Quy tắc cho agent" trong [README](../README.md).

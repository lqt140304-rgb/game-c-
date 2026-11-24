# Tổng quan kế hoạch game CHỢ ĐỒ LẠ

Bản gạch đầu dòng toàn bộ kế hoạch tới 2026-11-24. Chi tiết và trạng thái từng phần: [DaChot.md](DaChot.md). Mục nào ghi *(đề xuất)* là chưa chốt.

## Game là gì
- Roguelite săn đồ cổ, 3D góc nhìn thứ nhất, chơi đơn trên PC. Unity 6 + URP, Blender.
- Mỹ thuật: **low-poly, màu trung tính**; đồ bậc càng cao càng chi tiết; nhìn là nhận ra.
- Giọng điệu: hài phóng đại, ngôn ngữ Gen Z; lời thoại là chữ chạy kiểu light novel, không lồng tiếng.

## Câu chuyện và mở màn
- Nhân vật dồn hết tiền cá độ bóng đá, đội nhại thua phút cuối, vay nợ **Anh Bảy** (giang hồ mạng trẻ trâu).
- Có **7 ngày** để trả từng kỳ nợ; kỳ 1 là 200 xu, thu cuối ngày.
- Ngày 1 có kịch bản: hộp đầu ra rác hài (áo đấu cũ của chính mình), hộp sau ra đồ bán được; **ngày 1 không thua ngay**.
- Dì Mận dạy mua hộp ở chợ.

## Một ngày
- **Buổi sáng:** mini-game đánh răng, ăn sáng; mỗi ngày một biến thể nhỏ; từ ngày 2 có nút bỏ qua.
- **Mở rương là việc buổi sáng.**
- **Ban ngày ở chợ:** mua rương, mở, soi lỗi, bán, trả giá, dịch vụ (giám định, cầm cố, đấu giá, sòng bạc).
- **Tối:** về phòng trọ, Anh Bảy thu nợ.
- Tình huống đời thường hài xen vào (mèo chợ, chuông chợ...).
- Thua: nợ tồn vượt 1.000 xu hoặc còn nợ sau ngày 7. Thắng: trả đủ 7 kỳ.

## Rương
- **4 rương cơ bản** (mua ở quầy, để sinh tồn kiếm tiền): Gỗ (an toàn), Vải Niêm (gom bộ), Kim Khí (ra tạo tác), Đen (cược lớn).
- **Rương Tổng Hợp:** ra ngẫu nhiên đồ của các rương cơ bản.
- **Rương chủ đề:** Việt Nam, Tuổi thơ Việt Nam, Wibu, Truyền thuyết Châu Âu, Robot, Không gian, Game, Thể thao, Âm nhạc, Phim kinh dị, Ai Cập, Hải tặc, Trung Hoa, Nhật Bản (còn thêm sau).
  - Mở khóa **chỉ bằng thành tựu**; mở xong thì **có trong shop từ hôm sau**.
- **Cách mở:** quay **bậc** trước, rồi quay **món** trong bộ sưu tập của chính rương đó.
- **Mỗi món chỉ thuộc một rương**; riêng **Rác dùng chung** mọi rương.

## Bậc đồ
- 5 bậc: **Rác, Thường, Hiếm, Rất hiếm, Huyền thoại**.
- Rác: bán vài xu, chiếm ô kho, có **công dụng ẩn** (trứng thối đuổi kẻ đòi nợ...).
- Huyền thoại rương cơ bản: rẻ hơn, đồ tự phát (ví dụ vé số độc đắc, sau này đổi thưởng ở quầy vé số).
- Huyền thoại rương chủ đề: **biểu tượng có câu chuyện**, ai trong nền văn hóa đó cũng biết (Nỏ thần, Gươm Thuận Thiên, Gậy Như Ý...).
- Đồ nhại được phép sát bản gốc, chỉ cần đổi nhỏ đủ nhận ra.

## Soi lỗi và trả giá
- Cầm đồ lên (E), soi (F), cất (Q), kiểu soi súng trong Valorant.
- **Soi lỗi:** so với hồ sơ chuẩn, tìm chỗ khác chuẩn, lượt soi có hạn, soi thêm tốn tiền; chọn thật/giả và tình trạng (Nguyên, Xước, Hỏng).
- Soi sai: thật tưởng giả thì bán lỗ, giả tưởng thật thì bị phạt.
- **Trả giá kiểu Ace Attorney:** dùng bằng chứng đã soi để phản bác thẩm định viên, đẩy giá lên.
- Món thường có bán nhanh, không cần trả giá.
- Tình trạng và hàng giả là **lớp phủ** trên một mô hình gốc, không làm mô hình riêng.

## Xây lối chơi trong ván
- **Gia truyền:** có từ đầu ván, nội tại bị động.
- **Tạo tác:** có tier và hệ (Cổ Phong, Thiên Cơ), ghép combo; **4 ô** (Bí Khế BK1 mở ô thứ 5).
- **Bí Khế:** mở theo điều kiện trong ván.

## Thần lực
- Mỗi món có một nhân cách và phản ứng dí dỏm khi cầm, khi soi và đôi khi ngoài đời (chuông rung, mèo phản ứng).
- **Không đổi tiền, giá, xác suất**, không lộ thật/giả.

## Sưu tập và tiến trình giữa các ván
- Sổ Sưu Tập ghi mọi thứ từng có, kể cả rác.
- Trong ván: đủ một bộ thì có thưởng nhỏ.
- Giữa các ván: **cây phát triển** 4 nhánh (Rương, Sưu tập, Thần lực, Câu chuyện), mở bằng điều kiện sưu tập + điểm phát triển; phần thưởng không buff chơi.
- **Thành tựu** mở rương chủ đề.

## Mô hình 3D (tình trạng)
- Rương: 4 rương cơ bản làm bằng Blender, có lớp nắp mở được.
- Đồ: Rác 9, Thường 8, Hiếm 34 (còn 3 món mới), Rất hiếm 32 (đủ), Huyền thoại 24 (găng tay L19 cần hiệu ứng lấp lánh trong Unity). Chi tiết: [BanGiao3D](Art/BanGiao3D.md).
- Meshy: gộp 8 món một lượt; món lỗi gom làm lại một đợt khi đủ 8 món.
- Chưa chạy thử trong Unity (cần glTFast để đọc .glb).

## Còn chờ
- Claude 2: giá, tỉ lệ rơi, mức phạt, cân bằng 4 ô tạo tác.
- Codex: dữ liệu (rác, thành tựu, rương chủ đề, shop theo ngày, cây phát triển).
- Antigravity: cảnh chợ, giao diện, đưa mô hình vào Unity, mô hình rương chủ đề.
- Chủ dự án: thành tựu cụ thể, món còn thiếu ở các rương, chủ đề thêm.

## Cách làm việc
- Mỗi phiên một việc; điều đã chốt ghi ra .md và thêm vào [DaChot.md](DaChot.md).
- Bật Ponytail full + Superpowers đầu mỗi phiên.
- Không tốn credit hay tự quyết khi chủ dự án chưa cho phép.

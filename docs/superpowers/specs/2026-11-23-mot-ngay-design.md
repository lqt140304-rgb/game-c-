# Thiết kế: một ngày trong game, mini-game buổi sáng và tình huống đời thường (phần 3/4)

Trạng thái: **đã duyệt** (cập nhật 2026-11-24: mèo của chợ, chuông rung... vẫn là chất liệu cho tình huống; chưa có cảnh hay mô hình mèo). Chưa có gì được xây theo bản này.

Phần 1 (rương, rác, sưu tập, thần lực): [2026-11-23-ruong-suu-tap-design.md](2026-11-23-ruong-suu-tap-design.md). Phần 2 (mở màn, giọng điệu, nhân vật): [2026-11-23-mo-man-design.md](2026-11-23-mo-man-design.md). Phần 4 (nội dung thần lực chi tiết) làm sau.

## Ý định

Thêm nhịp sống đời thường vào mỗi ngày để game không dừng ở việc đập hộp, và để người chơi có khoảnh khắc thở giữa các lượt cược. Mini-game buổi sáng và tình huống hài hước là **gia vị**: làm thế giới có hồn, **không đổi cơ chế kinh tế**.

## Mục tiêu

- Mỗi ngày bắt đầu bằng một buổi sáng ở phòng trọ có mini-game ngắn.
- Mini-game không gây nhàm khi lặp lại 7 ngày và nhiều ván.
- Mỗi ngày có 1–2 tình huống đời thường hài hước, kèm vài mốc cố định tạo mạch truyện.
- Thành tựu gắn với mini-game, đạt thì mở khóa một thứ nhỏ.

## Không phải mục tiêu

- Mini-game và tình huống **không đổi tiền, giá, xác suất, nợ** hay luật thắng thua. Tắt hết thì cân bằng kinh tế vẫn y nguyên.
- Không thiết kế chi tiết hình ảnh và âm thanh từng mini-game; đó là việc dựng cảnh (Antigravity).
- Không thay hệ **biến cố** kinh tế hiện có (thay đổi nguồn cung, nhu cầu, dịch vụ). Hai thứ tách riêng.

## Ràng buộc đã có

- Vòng ngày hiện có: mở sổ ở phòng trọ, đến Quầy Niêm Phong, mở hộp ở bàn, quyết định bán/giữ/giám định/phục chế, về phòng trọ cho Anh Bảy thu nợ. (`Docs/GameDesign/README.md`, mục 2)
- Giọng điệu và nhân vật theo phần 2: hài hước phóng đại, ngôn ngữ Gen Z, lời thoại lưu trong file dữ liệu riêng.
- Thần lực chỉ là phản ứng dí dỏm, không đổi cơ chế (phần 1).

## 1. Buổi sáng

Vị trí trong vòng ngày: bước mới ngay **trước** bước "mở sổ ở phòng trọ", sau khi sang ngày.

- Hai mini-game cơ bản: **đánh răng** rồi **ăn sáng**.
- **Mỗi ngày một biến thể nhỏ:** ví dụ kem đánh răng có vị lạ, món ăn sáng khác nhau, dụng cụ khác nhau. Mục đích là tạo bất ngờ chứ không đổi luật.
- **Ngày 1 chơi đầy đủ** để làm quen (theo phần 2). **Từ ngày 2 trở đi có nút bỏ qua hoặc chơi nhanh**, người chơi muốn chơi đầy đủ vẫn được.
- Kết quả mini-game **chỉ đổi một chút lời thoại** (Dì Mận, Anh Bảy hay độc thoại của nhân vật nhắc đến việc bạn đánh răng sạch, hay ăn sáng thế nào). Không đổi số liệu.
- Độ khó thấp, thất bại không có hậu quả. Mục tiêu là vui và có cảm giác thao tác (gần với cầm và soi vật phẩm đã làm).

## 2. Thành tựu

- Có **vài thành tựu** gắn với mini-game (ví dụ đánh răng sạch cả 7 ngày, thử đủ các món ăn sáng, làm hỏng mini-game theo kiểu hài).
- Thành tựu **mở khóa một thứ nhỏ**, nối vào cây phát triển ở phần 1: nút ở nhánh **Thần lực** hoặc **Câu chuyện**, hoặc mục trong Sổ Sưu Tập. **Không có buff chơi.**
- Thành tựu ghi vào Sổ Sưu Tập, nên giữ lại giữa các ván.

## 3. Tình huống đời thường hài hước

- Mỗi ngày **rút 1–2 tình huống ngẫu nhiên** từ một kho, kèm **lựa chọn thoại nhỏ** (không đổi tiền).
- Thêm **vài mốc cố định theo ngày** để có mạch truyện. Ví dụ: ngày 4 đội bóng nhại ("Quỷ Đỏ United") lại đá và nhân vật lại bị cuốn vào; ngày 7 trận chung kết với Anh Bảy.
- Chất liệu tình huống đến từ nhân vật và nội dung đã có: Anh Bảy nhắn tin thoại khoa trương, Dì Mận bình luận hộp như bình luận viên, **mèo của chợ**, công dụng ẩn của rác (trứng thối, xương cá, giày cũ, tất thối), thần lực dí dỏm của đồ cổ.
- Một số tình huống **phụ thuộc vật phẩm đang có** (ví dụ có xương cá thì mèo của chợ xuất hiện), nối với công dụng ẩn của rác ở phần 1.
- Mỗi ván khác nhau, vì phần ngẫu nhiên đổi theo hạt giống ván chơi.

## 4. Dữ liệu và kỹ thuật (hướng đi)

- Tình huống, lời thoại và thành tựu lưu trong **file dữ liệu riêng**, không nằm trong code, theo nguyên tắc ở phần 2.
- Mini-game dựng bằng Unity trong cảnh phòng trọ (Antigravity); dữ liệu và lưu thành tựu do Codex.
- Kết quả ngẫu nhiên xác định theo (hạt giống, ngày), để lưu và tải lại ván không đổi tình huống đã rút.

## Câu hỏi còn mở

1. Hai mini-game chơi thế nào (thao tác chuột, nhấn phím, kéo thả): cần Antigravity đề xuất sau khi thử cảm giác.
2. Danh sách thành tựu cụ thể và phần thưởng của từng thành tựu: cần chốt sau khi có cây phát triển.
3. Số tình huống trong kho khởi đầu (đề xuất 15–20) và danh sách mốc cố định theo ngày.
4. Có nút "tóm tắt buổi sáng" cho từ ngày 2 đúng như đề xuất, hay cần thêm tùy chọn tắt hẳn trong cài đặt?
5. Định dạng và đường dẫn file dữ liệu tình huống, thành tựu: trao đổi với Codex trong `Docs/Content/YeuCauDuLieu.md`.

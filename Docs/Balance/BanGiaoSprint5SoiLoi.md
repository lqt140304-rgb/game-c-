# Bàn giao: Sprint 5 phần 3, đo lại soi lỗi theo luật soi mới

Viết ngày 2026-11-24. Phiên trước đã đo soi lỗi và trả giá theo luật cũ ([Sprint5SoiLoi.md](Sprint5SoiLoi.md), `QA/inspect_sim.py`, `QA/test_inspect.py`). Sau đó **chủ dự án chốt luật soi mới**, nên phiên kế tiếp cần **đo lại**. Đọc trước: [DaChot.md](../DaChot.md), spec [soi-loi-tra-gia](../../docs/superpowers/specs/2026-11-23-soi-loi-tra-gia-design.md) mục 2 "Luật soi đã chốt", rồi file này.

> **Tiền tệ:** mọi số tiền người đọc thấy ghi bằng đồng Việt Nam. Mô phỏng tính theo đơn vị 10.000 đ, in qua `QA/currency.py`. Giá và phí đề xuất là bội số 10.000 đ.

## Luật soi mới (chủ dự án chốt 2026-11-24)

- Người chơi **tự tìm chỗ lỗi**. Chỉ khi di chuột **đúng chỗ lỗi** mới hiện một dòng chữ ngắn ngay đầu chuột ghi lỗi gì. Chỗ không có lỗi thì không hiện gì, nên **không còn nhiễu**.
- **Bấm vào lỗi thì giá tiền trừ theo lỗi.**
- **Bảng** của món đồ cho chọn **thật hay giả**, và hiện **giá trị cùng thông tin cơ bản**.

## Phần còn dùng được và phần phải sửa trong bản đo cũ

- **Giữ:** nền 4 rương đã chốt (`chest_sim.FINAL`, `FIRST_RUN`), cách nạp và trả lõi (`apply`, `restore`), hồ sơ người chơi, khung đo (`inspect_final.py`, `inspect_explore.py`). Giữ cả luật thật mà khai giả chỉ nhận giá hàng giả 20%, giả mà khai thật bị tịch thu, ngày 1 miễn phạt, và tiêu chí nghiệm thu như cũ.
- **Phải bỏ:** mô hình "lượt soi trúng điểm có nghĩa hoặc chỗ nhiễu" (`one_look`, `noise_fake`), lượt soi miễn phí, phí soi thêm. Các mục này không còn đúng với luật mới. Có giới hạn thời gian soi hay không vẫn là câu hỏi mở.
- **Phải đổi:** lỗi bây giờ **trừ giá** chứ không phải bằng chứng đẩy giá lên. Trả giá kiểu Ace Attorney vẫn có trong spec, nhưng cách nó nối với lỗi tìm được cần đặt lại (xem đề xuất mô hình bên dưới).

## Đề xuất mô hình cho phiên sau (giả định của Claude 2, chưa chốt)

1. **Lỗi tình trạng thay cho hệ số tình trạng**, để giữ đúng kinh tế cũ (Nguyên ×1,0, Xước ×0,7, Hỏng ×0,3):
   - Nguyên: không có lỗi.
   - Xước: n lỗi nhẹ, tổng trừ 30%. n theo bậc: Thường 1, Hiếm 2, Rất hiếm 3, Huyền thoại 3, tức mỗi lỗi trừ 30% chia n.
   - Hỏng: các lỗi nhẹ cộng 1 vết hỏng lớn trừ 40%, tổng trừ 70%.
   - Số lỗi như vậy khớp tier-do-trong-ruong mục 4.
2. **Món giả** có thêm 1 đến 2 lỗi kiểu "dấu hàng giả" (Thường và Hiếm 1, Rất hiếm và Huyền thoại 2). Món thật có thể có 1 "dấu lạ" trông giống dấu giả, tỉ lệ để làm tham số (thử 0%, 10%, 20%). Người chơi suy ra xác suất giả theo Bayes từ các dấu đã thấy, rồi chọn thật hay giả trên bảng.
3. **Kỹ năng** = xác suất tìm ra mỗi lỗi: giỏi 95%, thường 75%, ẩu 45%, bỏ qua soi 0%.
4. **Bỏ sót lỗi:** khi cửa hàng kiểm tra lại, lỗi bị sót vẫn bị trừ, cộng thêm một khoản phạt. Đó là "giá ép khi soi thiếu"; thử 0%, 5% và 10% giá thẩm định mỗi lỗi sót.
5. **Trả giá (Hiếm trở lên):** mở màn ở giá bảng sau khi đã trừ lỗi. 3 vòng; mỗi vòng thắng với xác suất (kỹ năng trả giá) × (tỉ lệ lỗi đã tìm). Lý do: biết đủ lỗi thì phản bác được khi thẩm định viên nói quá. Thắng +8%, thua −5%, có trần.
6. **Gợi ý của Giáo Sư Lẫm:** chỉ ra một lỗi chưa tìm thấy, ưu tiên dấu giả, phí 5% giá nền.
7. **Thứ cần dò:** mức phạt khi sót lỗi, tỉ lệ dấu lạ, bước trả giá và trần, mức tịch thu. Mục tiêu: bot thường 15% đến 20%, người chơi giỏi không quá 47%, soi giỏi hơn soi thường rõ, soi thường hơn soi ẩu và hơn bỏ qua soi, ngày 1 lần chơi đầu không thua.

## Việc của phiên sau

1. Viết lại `handle` trong `QA/inspect_sim.py` theo mô hình trên, giữ API (`apply`, `restore`, `evaluate`, `day1`, `_play`, `summarize`).
2. Dò bằng `inspect_explore.py`, rồi chạy bảng cuối bằng `inspect_final.py final` (n = 2.000) và bản độ nhạy.
3. Sửa `QA/test_inspect.py` theo luật mới. Viết lại `Sprint5SoiLoi.md`: ghi bản cũ là đã thay thế, giữ lại phát hiện còn đúng, ví dụ khai giả nhầm đắt hơn khai thật nhầm, và cần có trần trả giá.
4. Cập nhật TASKS.md, commit và push lên `claude/eloquent-bell-5baoyc`.

## Câu hỏi mở cho chủ dự án

1. Còn giới hạn lượt soi hoặc thời gian soi không? Nếu soi không giới hạn thì ai kiên nhẫn cũng tìm được hết lỗi, và kỹ năng soi chỉ còn là sự kiên nhẫn.
2. Bằng chứng ở bước soi nối với trả giá thế nào, khi lỗi bây giờ là trừ giá (đề xuất ở mục 5 bên trên)?
3. Dòng chữ của "dấu lạ" trên món thật có cố ý giống dấu hàng giả không? Tỉ lệ dấu lạ quyết định soi thật giả có rủi ro hay không.
4. Người chơi có được bỏ bấm một lỗi đã thấy để giữ giá cao không? Đề xuất: được, nhưng cửa hàng kiểm tra lại thì vẫn trừ và phạt như lỗi bị sót.

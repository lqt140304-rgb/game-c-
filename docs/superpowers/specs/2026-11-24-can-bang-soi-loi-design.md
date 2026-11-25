# Cách đo: cân bằng soi lỗi và trả giá (Sprint 5 phần 3, Claude 2)

Trạng thái: **cách đo và giả định của Claude 2**, không phải luật đã chốt. Luật gốc: [soi-loi-tra-gia](2026-11-23-soi-loi-tra-gia-design.md); số lỗi theo bậc: [tier-do-trong-ruong](2026-11-23-tier-do-trong-ruong-design.md) mục 4. Kết quả: [Sprint5SoiLoi.md](../../../Docs/Balance/Sprint5SoiLoi.md). Mã: `QA/inspect_sim.py`.

## Việc cần ra số

Lượt soi miễn phí theo bậc, phí và giới hạn soi thêm, mức phạt khi khai thật một món giả, mức lỗ khi khai giả một món thật, giá ép khi soi thiếu (giá chào mở màn), bán nhanh, và giá trị của trả giá (bước cộng khi đưa đúng bằng chứng, bước trừ khi đưa sai, số vòng, trần và sàn).

## Nghiệm thu

1. Bot thường (mua rương đắt nhất mua được, soi kiểu "thường", không mua lượt soi thêm) thắng khoảng 15% đến 20%.
2. Người chơi tính toán hoàn hảo (chọn rương theo lời kỳ vọng, đọc manh mối, soi giỏi, dùng gợi ý của Giáo Sư Lẫm) không quá 47%.
3. Kỹ năng soi có giá trị rõ: cùng một cách mua rương, người soi giỏi thắng hơn người soi thường, người soi thường hơn người soi ẩu và người bỏ qua soi.
4. Soi sai bị phạt đủ để có rủi ro (phạt và lỗ thấy được trong ván, bỏ qua soi thua kém rõ) nhưng không làm game không chơi được (người soi ẩu vẫn còn đường thắng).
5. Ngày 1 lần chơi đầu không thua ngay.

## Giả định

1. **Nền giữ nguyên**: 4 rương cơ bản đã chốt (`chest_sim.FINAL`, ván thường) và lần chơi đầu (`chest_sim.FIRST_RUN`); lõi mô phỏng đã duyệt (nợ, phạt nợ 20%, giá rương dao động ±20%, manh mối, phục chế, tạo tác, combo). Chỉ thay bước "mở xong thì bán".
2. **Giá thẩm định V** = giá nền × hệ số tình trạng × hiệu ứng tạo tác, đúng bằng giá bán của lõi cũ. Mọi mức trong luật tính theo phần trăm của V, làm tròn về bội số 10.000 đ.
3. **Soi lỗi thay giám định bằng chữ.** Chứng nhận +25% của giám định cũ không còn; Giáo Sư Lẫm bán "gợi ý một lỗi" (một bằng chứng chắc đúng) với phí giám định cũ (15% giá nền, tối thiểu 200.000 đ). Có đo biến thể giữ chứng nhận.
4. **Tình trạng nhìn hình luôn đúng** (spec mục 1); người ẩu vẫn có thể khai nhầm (ghi trong hồ sơ người chơi). Khai thấp hơn thật thì bị trả theo mức khai, khai cao hơn thì bị bắt bẻ, mất một bước trả giá.
5. **Mô hình dấu hiệu** (thay cho nội dung thật mà Claude 1 và Antigravity sẽ làm): mỗi lượt soi trúng một điểm lỗi có nghĩa với xác suất h (kỹ năng), còn lại soi vào chỗ nhiễu. Điểm có nghĩa trên món giả hiện dấu giả 85%; trên món thật hiện "dấu lạ" giống giả 10%. Chỗ nhiễu hiện dấu giả 25%. Người chơi đọc nhầm với xác suất e. Người chơi biết tỉ lệ hàng giả của rương (UI hiển thị, TASKS.md) và tính xác suất giả theo Bayes.
6. **Món giả khai thật luôn bị cửa hàng phát hiện** khi kiểm tra lại (spec mục 3), nên không có cửa "khai liều mà thoát".
7. **Hồ sơ người chơi** (giả định về người, không phải luật):

| Hồ sơ | Trúng điểm có nghĩa (h) | Đọc nhầm (e) | Đưa đúng bằng chứng (c) | Nhầm tình trạng | Cách quyết | Lượt soi dùng | Soi thêm |
|---|---|---|---|---|---|---|---|
| giỏi | 90% | 3% | 95% | 0% | theo giá trị kỳ vọng | hết lượt miễn phí | có, khi đáng |
| thường (bot thường) | 65% | 10% | 75% | 5% | khai giả khi nghĩ giả trên 50% | hết lượt miễn phí | không |
| ẩu | 40% | 20% | 50% | 15% | như trên | chỉ 1 lượt | không |
| bỏ qua soi | không soi | | | | luôn khai thật | 0 | không, luôn bán nhanh |

## Không làm

- Không đo thời gian chơi thật của mini-game (chỉ đếm số lượt soi và vòng trả giá mỗi món).
- Không mô hình hóa nội dung lời thoại đối chất; "Hỏi vặn" gộp vào xác suất đưa đúng bằng chứng.
- Rương mở khóa và rương chủ đề (phần 2, chưa chốt) không nằm trong nền; độ nhạy riêng nếu cần.

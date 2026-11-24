# Sprint 1 — Kiểm thử dòng tiền mẫu (Claude 2)

> **Tiền tệ:** số tiền trong tài liệu này đã đổi sang **đồng Việt Nam** ngày 2026-11-24 (1 xu cũ = 10.000 đ, xem [bảng quy đổi](README.md#tiền-tệ)). Mô phỏng vẫn tính theo đơn vị cân bằng 10.000 đ, `QA/currency.py` đổi sang đồng khi in.

> **Cập nhật:** mô hình đã chuyển sang luật nợ tồn của Claude 1 ở [Sprint 2](Sprint2.md). Bảng dòng tiền 6 ván bên dưới là kết quả với luật cũ (thua ngay khi thiếu tiền) nên không còn tái hiện được bằng mã hiện tại.

Chưa có code gameplay, nên Claude 2 viết **mô hình tham chiếu** của luật kinh tế rồi kiểm thử nó. Codex có thể dùng các test làm đặc tả hành vi và chuyển sang C# khi triển khai.

## Cách tái hiện

```bash
cd QA
python3 -m unittest -v test_economy   # 17 test Sprint 1
python3 scenario_cashflow.py          # dòng tiền 7 ngày, seed 1–3
```

| File | Nội dung |
| --- | --- |
| `QA/economy_model.py` | Mô hình mua, mở, bán, kết thúc ngày, thu nợ. Hằng số lấy từ [Balance](README.md) |
| `QA/test_economy.py` | 17 test, ứng với BT-01 đến BT-14 và BT-16 |
| `QA/scenario_cashflow.py` | Dòng tiền mẫu cho chiến thuật thận trọng và liều, seed cố định |

## Kết quả

Cả 17 test **đạt** trên mô hình. Các ca biên Sprint 1 yêu cầu:

| Ca | Mã | Kết quả |
| --- | --- | --- |
| Mua hộp thứ 4 | BT-01, BT-02 | Từ chối, trạng thái giữ nguyên. Sang ngày đặt lại đủ 3 lượt, không cộng dồn |
| Hết tiền | BT-03, BT-04 | 990.000 đ mua hộp 1.000.000 đ bị từ chối và không mất lượt. Đúng 1.000.000 đ mua được, tiền về 0 |
| Kho đầy | BT-14 | Từ chối mua, không trừ tiền và lượt. Sức chứa 10 là **giá trị tạm** |
| Bán trùng | BT-10, BT-11, BT-13 | Chỉ cộng tiền một lần. Bán món đã bán và mở hộp đã mở bị từ chối |
| Trả nợ | BT-05 đến BT-07 | Chặn trả trước. Trả đủ và dư tiền đúng số |
| Hàng giả | BT-12 | Giả 10.000.000 đ bán còn 2.000.000 đ |
| Thắng ngày 7 | BT-16 | Đủ 65.500.000 đ thì thắng với tiền về 0 |
| Tái lập | — | Cùng seed cho cùng kết quả, khác seed cho kết quả khác |

### Dòng tiền mẫu

Chạy 6 ván (2 chiến thuật × seed 1–3, bán ngay mọi hiện vật):

| Chiến thuật | Seed | Kết quả |
| --- | --- | --- |
| Thận trọng | 1 | Thua ngày 4, thiếu 6.920.000 đ |
| Thận trọng | 2 | Thua ngày 6, tiền 13.240.000 đ so với nợ 15.000.000 đ |
| Thận trọng | 3 | Thua ngày 3, thiếu 4.160.000 đ |
| Liều | 1 | Thua ngày 3, thiếu 1.100.000 đ |
| Liều | 2 | **Thắng**, còn 23.520.000 đ |
| Liều | 3 | Thua ngày 3, thiếu 4.260.000 đ |

## Lỗi và phát hiện ghi nhận

| Mã | Mức | Mô tả | Đề xuất | Chủ xử lý |
| --- | --- | --- | --- | --- |
| D-01 | Cao | `end_day` không idempotent. Gọi hai lần liên tiếp (nhấn đúp) thu nợ của **hai ngày**, vì lần hai thuộc ngày kế tiếp (test BT-09: tiền 10.000.000 đ về 4.500.000 đ, qua ngày 3) | Chặn bấm đúp trong UI, hoặc xác nhận kết thúc ngày một lần, hoặc dùng mã thao tác để bỏ lệnh trùng | Codex, Antigravity |
| D-02 | Trung bình | Hộp chưa mở có chiếm kho không, và có giữ qua ngày không? Luật chưa nói. Mô hình tạm đếm hộp vào kho | Claude 1 chốt trong GDD mục 3 | Claude 1 |
| D-03 | Trung bình | Thiếu tiền lúc đến hạn chưa có luật (BT-08). Mô hình tạm: thua ngay, không trừ một phần | Claude 1 chốt hậu quả (mục 5 của [Balance](README.md)) | Claude 1 |
| D-04 | Thấp | Mô hình bán hiện vật ngay khi mở. Nếu người chơi giữ món chờ giá tốt, dòng tiền khác nhiều và cần ca kiểm thử riêng | Chốt cơ chế giữ hàng ở Sprint 2 | Claude 1 |
| O-01 | Cân bằng | Một lần ra hiện vật hiếm quyết định cả ván. Liều seed 2 thu 60.320.000 đ chỉ ngày 1 rồi thắng. Chiến thuật thận trọng gặp nhiều ván thua ở ngày 3 và 4 | Đo lại ở Sprint 3 với giám định, tạo tác và gia truyền. Cân nhắc giảm xác suất cực hiếm hoặc nâng vốn | Claude 2 |

## Việc Codex cần biết

- Mọi thao tác ghi (mua, mở, bán, kết thúc ngày) phải **từ chối mà không đổi trạng thái** khi vi phạm luật. Test đã kiểm bằng so sánh trạng thái trước và sau.
- Mỗi hiện vật và hộp có mã riêng để chặn thao tác trùng. Riêng `kết thúc ngày` cần cơ chế khác (D-01).
- Dùng seed truyền vào để tái lập kết quả. Mô hình dùng một bộ sinh số cho cả xác suất rơi và hàng giả, nên thứ tự gọi ảnh hưởng kết quả.

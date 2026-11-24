# Sprint 2 — Kiểm thử lịch nợ, combo, Bí Khế, tải ván (Claude 2)

> **Tiền tệ (quyết định 2026-11-24):** game dùng **đồng Việt Nam**. Các số "xu" trong tài liệu này là **đơn vị cân bằng cũ, 1 xu = 10.000 đ** (ví dụ vốn 1.000 xu = 10.000.000 đ). Tỉ lệ và phần trăm không đổi. Xem [bảng quy đổi](README.md#tiền-tệ).

Mở rộng mô hình tham chiếu của [Sprint 1](Sprint1.md). Kết quả Sprint 1 (bảng dòng tiền 6 ván) **đã lỗi thời** vì mô hình nay dùng luật nợ tồn của Claude 1.

## Nguồn luật

- **Lịch nợ:** lấy từ đề xuất của Claude 1 (GDD mục 2, commit `643ae03` trên nhánh `claude/relaxed-cerf-wgnu7l`). Nhánh này chưa gộp vào nhánh của Claude 2. Mô hình cài đúng 7 ca của Claude 1.
- **Combo và Bí Khế:** Claude 1 chưa soạn nội dung Sprint 2. Mô hình dùng **giá trị tạm** để test chạy được: 6 tạo tác thuộc 2 hệ, 3 ô trang bị, 2 món cùng hệ là combo nhỏ, 3 món là combo lớn, hai Bí Khế mẫu (hoàn thành một combo, giám định 2 hiện vật Thần thoại). Khi Claude 1 chốt, chỉ cần sửa các bảng `ARTIFACTS`, `BI_KHE`, `COMBO_*` trong `QA/economy_model.py`.

## Cách tái hiện

```bash
cd QA
python3 -m unittest -v test_economy test_sprint2   # 52 test
python3 mc_strategies.py                           # tỷ lệ thắng 3 chiến thuật
python3 variants.py                                # so sánh phương án chỉnh
```

## Kết quả kiểm thử

52 test đều **đạt**. 35 test mới của Sprint 2:

| Nhóm | Mã | Nội dung |
| --- | --- | --- |
| Lịch nợ | DL-1 đến DL-7 | Bảy ca của Claude 1: trả đúng hạn, thiếu một phần, nợ tồn cộng kỳ mới, thua vì nợ tồn vượt ngưỡng, thua ngày 7, thắng ngày 7, hiện vật chưa bán không tính |
| | DL-8 | Phạt làm tròn lên (tồn 1 xu phạt 1 xu) |
| | DL-9 | Hai kỳ liên tiếp không trả: tồn 708 xu |
| | DL-10 | Ngưỡng thua là **lớn hơn** 1.000: tồn đúng 1.000 chơi tiếp, 1.001 thua |
| | DL-11 | Trả hết nợ tồn kỳ sau thì tồn về 0 |
| | DL-12 | Thiếu tiền rồi tiền về 0, không mua được hộp (xem D-05) |
| | DL-13 | Trả trước kỳ chưa đến hạn bị từ chối |
| Combo | CB-1 đến CB-7 | Combo nhỏ, combo lớn, khác hệ không combo, tháo món hủy combo, hết ô, trang bị trùng hoặc chưa có, combo chỉ tính một lần dù đổi món |
| Bí Khế | BK-1 đến BK-4 | Chưa đạt trước khi có combo, đạt khi hoàn thành combo, chỉ đếm đúng nhóm nguồn gốc, mở khóa một lần |
| Tải ván | SL-1 đến SL-9 | Lưu và tải khớp trạng thái, kết quả hộp không đổi khi tải lại, bán một lần, mã giao dịch lặp bị chặn, nợ tồn và tiến độ Bí Khế còn nguyên, ván đã thua vẫn thua sau khi tải, đếm lượt mua |

## Lỗi và phát hiện mới

| Mã | Mức | Mô tả | Đề xuất | Chủ xử lý |
| --- | --- | --- | --- | --- |
| D-05 | Cao | **Vòng xoáy thiếu tiền.** Luật hiện thu hết tiền mặt khi thiếu. Người chơi về 0 xu nên không mua được hộp để gỡ lại. Trong 20.000 ván mô phỏng, **không ván nào** đã từng thiếu tiền mà thắng | Chừa lại một mức tối thiểu hoặc cho giữ hiện vật đã có để bán. Phương án chừa 100 xu ít tác dụng (bảng bên dưới), nên gốc vấn đề là lợi nhuận hộp thấp | Claude 1, chủ dự án |
| D-06 | Trung bình | Tải bản lưu cũ hoàn tác được giao dịch (SL-9). Tải lại kết quả hộp thì không đổi, vì kết quả đã xác định lúc mua | Chốt chính sách lưu (GDD mục 6 còn [MỞ]), ví dụ chỉ lưu tự động ở đầu ngày | Claude 1, Codex |
| D-07 | Thấp | Phạt tính trên cả phần tồn nhỏ (tồn 1 xu bị phạt 1 xu, gấp đôi) | Chấp nhận hoặc đặt phạt tối thiểu | Claude 1 |
| D-08 | Thấp | Luật hiện không cho trả nợ tồn ngoài kỳ thu. Người dư tiền không thể giảm tồn sớm để tránh phạt | Chốt có cho trả tồn trước hay không | Claude 1 |
| O-02 | Cân bằng | Nợ tồn khiến tỷ lệ thắng giảm mạnh so với Sprint 0 (xem bảng dưới) | Xem phương án chỉnh | Claude 2, chủ dự án |

## Ảnh hưởng của luật nợ tồn lên tỷ lệ thắng

20.000 ván, bán ngay mọi hiện vật, không giám định, không tạo tác, không gia truyền. Là **sàn** của người chơi mới chứ không phải mức cuối.

| Chiến thuật | Thắng | Thua vì nợ tồn vượt 1.000 | Thua ngày 7 | Từng thiếu tiền |
| --- | --- | --- | --- | --- |
| Thận trọng | 1,4% | 97,2% | 1,4% | 98,6% |
| Cân bằng | 8,9% | 89,5% | 1,6% | 91,1% |
| Liều | 18,6% | 78,7% | 2,6% | 81,3% |

So với Sprint 0 (12,9% / 28,5% / 39,9%), luật mới đẩy tỷ lệ thắng xuống hơn một nửa vì một lần thiếu tiền gần như là thua.

### So sánh phương án chỉnh

6.000 ván mỗi ô. Thận trọng, cân bằng, liều theo thứ tự.

| Phương án | Thận trọng | Cân bằng | Liều |
| --- | --- | --- | --- |
| A. Luật Claude 1 nguyên bản | 1,2% | 8,8% | 18,8% |
| B. Chừa lại 100 xu | 1,1% | 8,5% | 18,5% |
| C. Vốn 1.500 xu | 2,3% | 18,6% | 27,4% |
| D. Giá bán nhân 1,3 | 8,8% | 25,5% | 48,6% |
| E. Chừa 100 xu và giá bán nhân 1,3 | 8,1% | 25,1% | 48,3% |
| F. Chừa 100 xu và vốn 1.500 | 2,0% | 18,3% | 27,0% |
| G. Chừa 100 xu, vốn 1.500, giá bán nhân 1,3 | 14,1% | 46,6% | 61,6% |

**Đề xuất của Claude 2, chưa áp dụng:** tăng giá bán trong bảng [Balance mục 2](README.md) lên 1,3 lần (phương án D). Đây là chỉnh nhỏ nhất đem lại tác dụng lớn. Phương án G tốt hơn cho người chơi mới nhưng đổi ba thứ cùng lúc. Chưa nên áp dụng ngay vì mô phỏng này chưa có giám định, tạo tác và gia truyền, nên đo lại ở Sprint 3. Việc chừa lại 100 xu gần như không đổi gì, không nên dùng riêng.

Chiến thuật liều vẫn thắng nhiều nhất ở mọi phương án (xem O-01). Hộp có kỳ vọng lãi dương nên không có chi phí cho việc liều. Cần thêm hộp lỗ kỳ vọng hoặc chi phí thật cho rủi ro.

## Kiểm tra nội dung mẫu của Claude 1

Claude 1 đã soạn danh mục hộp và hiện vật ([Docs/Content/NoiDungMau.md](https://github.com/lqt140304-rgb/game-c-/blob/claude/relaxed-cerf-wgnu7l/Docs/Content/NoiDungMau.md), commit `d08130a`) và ghi chờ Claude 2 kiểm tra kinh tế. Giá nền chưa tính hàng giả và chất lượng:

| Hộp | Giá | Giá nền kỳ vọng | Lần giá hộp | Khoảng giá nền |
| --- | --- | --- | --- | --- |
| H1 Hộp Gỗ Nhỏ | 100 | 457 | **4,58** | 300 đến 650 |
| H2 Hộp Kim Loại Mờ | 350 | 390 | 1,11 | 150 đến 900 |
| H3 Hộp Vải Dày | 800 | 920 | 1,15 | 400 đến 1.800 |
| RD Rương Đen | 1.500 | 1.340 | **0,89** | 1.000 đến 1.800 |

Nhận xét:

1. **H1 quá lời.** Mỗi hộp 100 xu cho trung bình 457 xu, thấp nhất 300 xu, nên không bao giờ lỗ. Chỉ cần mua 3 hộp H1 mỗi ngày là đủ trả nợ nhiều ngày đầu. Cần hạ giá nền ba hiện vật quá khứ (V01 đến V03) khoảng một nửa hoặc tăng giá hộp.
2. **Rương Đen lỗ kỳ vọng** (0,89), không tính phí giám định. Nếu đây là chủ ý thì cần lý do để người chơi mua, ví dụ cơ hội ra hiện vật duy nhất. Nếu không thì tăng giá nền hoặc hạ giá rương. Xuất hiện từ ngày 4 khi nợ là 750 xu nên người chơi dễ bị cám dỗ vào hộp lỗ.
3. **H2 và H3 khá sát 1,1 đến 1,15 lần**, thấp hơn mức 1,5 lần của bảng Balance sau khi tính hàng giả. Đúng hướng "rủi ro cao hơn" nhưng với luật nợ tồn, người chơi sẽ ít dám mua.
4. Hộp chỉ ra **một** hiện vật và các món trong hộp giống nhau về giá. Mức đa dạng này thấp hơn bảng 5 mức hiếm của Balance, nên cần thống nhất một trong hai.

Mình chưa chỉnh nội dung của Claude 1. Đề nghị hai bên thống nhất bảng giá trong Sprint 3.

## Việc cần chốt

1. Chủ dự án chọn phương án chỉnh cho O-02 (khuyến nghị D rồi đo lại ở Sprint 3).
2. Claude 1 trả lời D-02 (hộp chưa mở chiếm kho), D-05 đến D-08, chính sách lưu ván.
3. Claude 1 soạn nội dung combo và Bí Khế thật để thay giá trị tạm, rồi mình chạy lại test.
4. Codex chuyển các test trong `QA/` sang C# khi triển khai nợ, combo, Bí Khế và lưu ván.

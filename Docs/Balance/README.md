# Cân bằng và kiểm thử — CHỢ ĐỒ LẠ

## Tiền tệ

**Quyết định của chủ dự án (2026-11-24): game dùng đồng Việt Nam, 1 xu cũ = 10.000 đ.** Chi tiết: [spec tiền tệ](../../docs/superpowers/specs/2026-11-24-tien-te-design.md). **Các tài liệu cân bằng trong thư mục này đã được đổi sang đồng ngày 2026-11-24** (bảng giá, nợ, lời, ngưỡng, phí; riêng bảng quy đổi dưới đây giữ cột "xu cũ" để đối chiếu). Mô phỏng và kiểm thử trong `QA/` vẫn tính theo **đơn vị cân bằng = 10.000 đ**; `QA/currency.py` đổi sang đồng khi in. Số tiền trong tài liệu mới phải ghi bằng đồng.

| Mốc | Xu cũ | Đồng |
| --- | --- | --- |
| Vốn đầu ván | 1.000 | 10.000.000 đ |
| Nợ ngày 1 đến 7 | 200 / 350 / 500 / 750 / 1.050 / 1.500 / 2.200 | 2.000.000 / 3.500.000 / 5.000.000 / 7.500.000 / 10.500.000 / 15.000.000 / 22.000.000 đ |
| Tổng nợ | 6.550 | 65.500.000 đ |
| Giá hộp H1 / H2 / H3 / Rương Đen | 100 / 350 / 800 / 1.500 | 1.000.000 / 3.500.000 / 8.000.000 / 15.000.000 đ |
| Ngưỡng thua | 1.000 | 10.000.000 đ |

Công thức: `đồng = xu cũ x 10.000`. Bước làm tròn nhỏ nhất là 10.000 đ.

Tài liệu của **Claude 2** cho Sprint 0. Mọi con số là **giả định thử nghiệm** để Codex nạp dữ liệu và để đo lại ở Sprint 3. Luật cốt lõi không đổi: tối đa 3 hộp mỗi ngày, 7 ngày, nợ chỉ thu đúng kỳ.

Quy ước trạng thái giống [GDD](../GameDesign/README.md): **[ĐÃ CHỐT]**, **[ĐỀ XUẤT]**, **[MỞ]**.

## Mục lục tài liệu Sprint 5

- [BangSoLieuSprint5.md](BangSoLieuSprint5.md) — một bảng số liệu cân bằng cho Codex nạp (kèm `Docs/Content/data/balance_sprint5.json`), có cột trạng thái ĐÃ CHỐT, ĐỀ XUẤT, GIẢ ĐỊNH.
- [CaKiemThuSprint5.md](CaKiemThuSprint5.md) — **ca kiểm thử biên Sprint 5 (BT-17 trở đi)** cho luật mới; test mẫu trên mô hình tham chiếu: `QA/test_sprint5_cases.py`.
- Báo cáo nguồn: [Sprint5Ruong](Sprint5Ruong.md), [Sprint5RuongMoKhoa](Sprint5RuongMoKhoa.md), [Sprint5SoiLoi](Sprint5SoiLoi.md), [Sprint5CayPhatTrien](Sprint5CayPhatTrien.md), bàn giao [BanGiaoSprint5](BanGiaoSprint5.md).

## 1. Giả định tiền và nợ

| Ngày | Nợ đến hạn | Tổng nợ lũy kế | Tiền đã phải kiếm thêm (so với vốn 10.000.000 đ) |
| --- | --- | --- | --- |
| 1 | 2.000.000 đ | 2.000.000 đ | 0 đ |
| 2 | 3.500.000 đ | 5.500.000 đ | 0 đ |
| 3 | 5.000.000 đ | 10.500.000 đ | 500.000 đ |
| 4 | 7.500.000 đ | 18.000.000 đ | 8.000.000 đ |
| 5 | 10.500.000 đ | 28.500.000 đ | 18.500.000 đ |
| 6 | 15.000.000 đ | 43.500.000 đ | 33.500.000 đ |
| 7 | 22.000.000 đ | 65.500.000 đ | 55.500.000 đ |

- **[ĐỀ XUẤT]** Giữ nguyên vốn 10.000.000 đ và lịch nợ trong GDD. Tổng nợ 65.500.000 đ, nên cả ván cần lãi ròng ít nhất 55.500.000 đ từ tối đa 21 hộp.
- Giới hạn 3 hộp mỗi ngày khiến lợi nhuận bị chặn trên. Nhờ vậy giá trị hộp phải đủ lớn ở các ngày cuối, nếu không ván không thể thắng dù chơi hoàn hảo.
- Khoản nợ ngày 7 (22.000.000 đ) bằng hơn 1/3 tổng nợ. Đây là điểm quyết định thắng hoặc thua nên cần theo dõi riêng ở Sprint 3.

## 2. Giả định hộp, giá bán và xác suất rơi

Giá trị là giá bán nếu hiện vật **thật**. Ba loại hộp thường theo GDD, chưa tính hộp đặc biệt.

| Độ hiếm | Hộp 1.000.000 đ | Hộp 3.500.000 đ | Hộp 8.000.000 đ |
| --- | --- | --- | --- |
| Phổ thông | 35% · 400.000 đ | 30% · 1.600.000 đ | 30% · 4.500.000 đ |
| Thường | 30% · 1.200.000 đ | 30% · 4.000.000 đ | 30% · 9.000.000 đ |
| Khá | 20% · 2.000.000 đ | 20% · 6.200.000 đ | 20% · 14.000.000 đ |
| Hiếm | 12% · 4.000.000 đ | 15% · 12.000.000 đ | 15% · 26.000.000 đ |
| Cực hiếm | 3% · 10.000.000 đ | 5% · 28.000.000 đ | 5% · 58.000.000 đ |
| **Giá trị kỳ vọng (thật)** | **1.680.000 đ** | **6.120.000 đ** | **13.650.000 đ** |

Hệ số giảm giá, **[ĐỀ XUẤT]**:

- **Hàng giả:** xác suất 15% mỗi hiện vật, bán chỉ được 20% giá trị. Giám định (Giáo Sư Lẫm) làm lộ hàng giả trước khi bán.
- Sau khi tính hàng giả, giá trị thực nhận kỳ vọng còn 1.480.000 / 5.390.000 / 12.010.000 đ. Mức hoàn vốn lần lượt là 1,48 / 1,54 / 1,50 lần giá hộp.
- Khoảng 0,5 lần giá hộp là biên để bù phí giám định, phục chế và rủi ro. Phí này **[MỞ]** vì chưa có bảng giá dịch vụ.
- Chất lượng (nguyên vẹn, chuẩn đẹp) chưa có hệ số. Giá trị trong bảng coi là chất lượng trung bình. **[MỞ]**

## 3. Kiểm tra khả thi nhanh

Mô phỏng 20.000 ván với seed cố định, bán ngay mọi hiện vật, không giám định, không tạo tác, không gia truyền. Chỉ để kiểm tra bảng có khả thi hay không, không phải cân bằng cuối.

| Chiến thuật | Cách chơi | Tỷ lệ thắng | Thua nhiều nhất |
| --- | --- | --- | --- |
| Thận trọng | Chỉ mua khi tiền còn dư sau khi chừa khoản nợ trong ngày, ưu tiên hộp 3.500.000 rồi 1.000.000 đ | 12,9% | Ngày 3 (26,7%) |
| Cân bằng | Như trên nhưng ưu tiên hộp 8.000.000 đ | 28,5% | Ngày 2 (24,5%) |
| Liều lĩnh | Dồn tiền mua hộp đắt nhất, không chừa nợ | 39,9% | Ngày 2 (21,7%) |

Nhận xét:

1. Với bảng này ván **thắng được**, nhưng người chơi không biết soi hay giám định vẫn thua phần lớn. Hợp lý vì người chơi thật có thông tin hộp, giám định, gia truyền và tạo tác. Cần đo lại khi các hệ đó có số liệu.
2. Chiến thuật liều thắng nhiều hơn chiến thuật thận trọng. Hiện tại rủi ro không bị phạt đủ vì mọi hộp đều có kỳ vọng dương. Ba hướng chỉnh để Claude 1 và chủ dự án cân nhắc: tăng phí mua, tăng xác suất hàng giả ở hộp đắt, hoặc thêm hộp lỗ kỳ vọng.
3. Rủi ro lớn nằm ở ngày 2 và 3. Vốn 10.000.000 đ và nợ nhỏ ở đầu ván nên người chơi chưa kịp tích lũy. Nếu chủ dự án muốn dễ vào ván hơn, giảm nợ ngày 3 từ 5.000.000 đ xuống 4.000.000 đ hoặc tăng vốn lên 12.000.000 đ. Mình chưa đổi vì đó là quyết định của chủ dự án.

## 4. Ca kiểm thử biên

Đầu vào lấy từ luật **[ĐÃ CHỐT]** nếu không ghi khác. Kết quả mong đợi chỉ rõ trạng thái sau thao tác.

| Mã | Tình huống | Đầu vào | Kết quả mong đợi |
| --- | --- | --- | --- |
| BT-01 | Mua hộp thứ 4 trong ngày | Đã mua 3 hộp ngày 1, tiền đủ, bấm mua hộp thứ 4 | Từ chối. Tiền, kho, số lượt mua không đổi. Quầy báo hết lượt |
| BT-02 | Đặt lại lượt mua khi sang ngày | Hết 3 lượt ngày 1, kết thúc ngày | Ngày 2 có đủ 3 lượt, lượt chưa dùng không cộng dồn |
| BT-03 | Mua khi không đủ tiền | Tiền 990.000 đ, hộp 1.000.000 đ | Từ chối, không trừ lượt mua |
| BT-04 | Mua đúng bằng số tiền còn lại | Tiền 1.000.000 đ, hộp 1.000.000 đ | Thành công, tiền 0, lượt mua giảm 1 |
| BT-05 | Trả nợ sớm | Ngày 1, thử thanh toán khoản ngày 2 | Từ chối, khoản ngày 2 không thể thanh toán, tiền không đổi |
| BT-06 | Trả đúng hạn, vừa đủ | Cuối ngày 1, tiền đúng 2.000.000 đ | Trả thành công, tiền 0, khoản ngày 1 đóng |
| BT-07 | Trả đúng hạn, dư tiền | Cuối ngày 1, tiền 6.500.000 đ | Trừ 2.000.000 đ, còn 4.500.000 đ, không trừ thêm |
| BT-08 | Thiếu tiền khi đến hạn | Cuối ngày 2, tiền 3.490.000 đ, nợ 3.500.000 đ | **[MỞ]** Chờ Claude 1 chốt hậu quả. Cho đến lúc đó phải ghi lại hành vi hiện tại để so sánh |
| BT-09 | Kết thúc ngày hai lần | Bấm kết thúc ngày liên tiếp hoặc nhấn đúp | Chỉ thu nợ một lần, chỉ sang ngày một lần |
| BT-10 | Bán trùng | Bấm bán cùng một hiện vật hai lần, hoặc bán từ hai nơi | Chỉ cộng tiền một lần. Món rời kho ngay lần đầu |
| BT-11 | Bán hiện vật đã bán | Bán món đã rời kho | Từ chối, tiền không đổi |
| BT-12 | Bán hàng giả | Hiện vật giả giá trị 10.000.000 đ, chưa giám định | Nhận 2.000.000 đ. Sau khi giám định cho thấy giả, giá hiển thị 2.000.000 đ |
| BT-13 | Mở hộp hai lần | Mở cùng một hộp đã mở | Từ chối, không sinh thêm hiện vật |
| BT-14 | Kho đầy | Kho đạt sức chứa, mua hoặc mở thêm hộp | **[MỞ]** Chờ sức chứa kho và cách xử lý món thừa |
| BT-15 | Thoát giữa thao tác | Thoát ngay sau khi mở hộp, trước khi bán | Tải lại không lặp giao dịch. Kiểm thử đầy đủ ở Sprint 2 |
| BT-16 | Thắng ngày 7 | Trả xong khoản 22.000.000 đ ngày 7 | **[MỞ]** Chờ Claude 1 chốt điều kiện thắng và tổng tài sản |

Ca kiểm thử cho luật Sprint 5 (BT-17 trở đi) nằm ở [CaKiemThuSprint5.md](CaKiemThuSprint5.md). BT-08, BT-14, BT-16 ở trên vẫn **[MỞ]**.

## 5. Luật còn thiếu cần Claude 1 chốt

1. Hậu quả khi thiếu tiền lúc đến hạn: thua ngay, cho trễ, hoặc cộng lãi. Cần chốt để viết BT-08.
2. Điều kiện thắng và thua, kể cả việc có tính tài sản ngoài tiền mặt (hiện vật còn trong kho) vào kết quả hay không.
3. Sức chứa kho và cách xử lý khi đầy (BT-14).
4. Phí giám định và phục chế, tác động của phục chế lên giá trị (mục 2).
5. Việc bán có thực hiện tức thì không, hay hiện vật phải qua quầy hoặc đấu giá. Mô phỏng ở đây giả định bán ngay.

## 6. Việc tiếp theo

- Codex dùng mục 2 làm dữ liệu mẫu. Giá trị lưu thành bảng dữ liệu để dễ chỉnh khi Sprint 3 có số đo thật.
- Sprint 1: biến các ca BT-01 đến BT-13 thành kiểm thử có thể chạy khi có dòng tiền mẫu.
- Sprint 3: chạy lại ba chiến thuật trên seed tái lập, có giám định, tạo tác và gia truyền. Ghi tỷ lệ thắng, ngày hụt tiền, tần suất combo và đề xuất chỉnh số.

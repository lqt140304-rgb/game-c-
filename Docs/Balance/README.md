# Cân bằng và kiểm thử — CHỢ ĐỒ LẠ

Tài liệu của **Claude 2** cho Sprint 0. Mọi con số là **giả định thử nghiệm** để Codex nạp dữ liệu và để đo lại ở Sprint 3. Luật cốt lõi không đổi: tối đa 3 hộp mỗi ngày, 7 ngày, nợ chỉ thu đúng kỳ.

Quy ước trạng thái giống [GDD](../GameDesign/README.md): **[ĐÃ CHỐT]**, **[ĐỀ XUẤT]**, **[MỞ]**.

## 1. Giả định tiền và nợ

| Ngày | Nợ đến hạn | Tổng nợ lũy kế | Tiền đã phải kiếm thêm (so với vốn 1.000) |
| --- | --- | --- | --- |
| 1 | 200 | 200 | 0 |
| 2 | 350 | 550 | 0 |
| 3 | 500 | 1.050 | 50 |
| 4 | 750 | 1.800 | 800 |
| 5 | 1.050 | 2.850 | 1.850 |
| 6 | 1.500 | 4.350 | 3.350 |
| 7 | 2.200 | 6.550 | 5.550 |

- **[ĐỀ XUẤT]** Giữ nguyên vốn 1.000 xu và lịch nợ trong GDD. Tổng nợ 6.550 xu, nên cả ván cần lãi ròng ít nhất 5.550 xu từ tối đa 21 hộp.
- Giới hạn 3 hộp mỗi ngày khiến lợi nhuận bị chặn trên. Nhờ vậy giá trị hộp phải đủ lớn ở các ngày cuối, nếu không ván không thể thắng dù chơi hoàn hảo.
- Khoản nợ ngày 7 (2.200 xu) bằng hơn 1/3 tổng nợ. Đây là điểm quyết định thắng hoặc thua nên cần theo dõi riêng ở Sprint 3.

## 2. Giả định hộp, giá bán và xác suất rơi

Giá trị là giá bán nếu hiện vật **thật**. Ba loại hộp thường theo GDD, chưa tính hộp đặc biệt.

| Độ hiếm | Hộp 100 xu | Hộp 350 xu | Hộp 800 xu |
| --- | --- | --- | --- |
| Phổ thông | 35% · 40 xu | 30% · 160 xu | 30% · 450 xu |
| Thường | 30% · 120 xu | 30% · 400 xu | 30% · 900 xu |
| Khá | 20% · 200 xu | 20% · 620 xu | 20% · 1.400 xu |
| Hiếm | 12% · 400 xu | 15% · 1.200 xu | 15% · 2.600 xu |
| Cực hiếm | 3% · 1.000 xu | 5% · 2.800 xu | 5% · 5.800 xu |
| **Giá trị kỳ vọng (thật)** | **168 xu** | **612 xu** | **1.365 xu** |

Hệ số giảm giá, **[ĐỀ XUẤT]**:

- **Hàng giả:** xác suất 15% mỗi hiện vật, bán chỉ được 20% giá trị. Giám định (Giáo Sư Lẫm) làm lộ hàng giả trước khi bán.
- Sau khi tính hàng giả, giá trị thực nhận kỳ vọng còn 148 / 539 / 1.201 xu. Mức hoàn vốn lần lượt là 1,48 / 1,54 / 1,50 lần giá hộp.
- Khoảng 0,5 lần giá hộp là biên để bù phí giám định, phục chế và rủi ro. Phí này **[MỞ]** vì chưa có bảng giá dịch vụ.
- Chất lượng (nguyên vẹn, chuẩn đẹp) chưa có hệ số. Giá trị trong bảng coi là chất lượng trung bình. **[MỞ]**

## 3. Kiểm tra khả thi nhanh

Mô phỏng 20.000 ván với seed cố định, bán ngay mọi hiện vật, không giám định, không tạo tác, không gia truyền. Chỉ để kiểm tra bảng có khả thi hay không, không phải cân bằng cuối.

| Chiến thuật | Cách chơi | Tỷ lệ thắng | Thua nhiều nhất |
| --- | --- | --- | --- |
| Thận trọng | Chỉ mua khi tiền còn dư sau khi chừa khoản nợ trong ngày, ưu tiên hộp 350 rồi 100 | 12,9% | Ngày 3 (26,7%) |
| Cân bằng | Như trên nhưng ưu tiên hộp 800 | 28,5% | Ngày 2 (24,5%) |
| Liều lĩnh | Dồn tiền mua hộp đắt nhất, không chừa nợ | 39,9% | Ngày 2 (21,7%) |

Nhận xét:

1. Với bảng này ván **thắng được**, nhưng người chơi không biết soi hay giám định vẫn thua phần lớn. Hợp lý vì người chơi thật có thông tin hộp, giám định, gia truyền và tạo tác. Cần đo lại khi các hệ đó có số liệu.
2. Chiến thuật liều thắng nhiều hơn chiến thuật thận trọng. Hiện tại rủi ro không bị phạt đủ vì mọi hộp đều có kỳ vọng dương. Ba hướng chỉnh để Claude 1 và chủ dự án cân nhắc: tăng phí mua, tăng xác suất hàng giả ở hộp đắt, hoặc thêm hộp lỗ kỳ vọng.
3. Rủi ro lớn nằm ở ngày 2 và 3. Vốn 1.000 xu và nợ nhỏ ở đầu ván nên người chơi chưa kịp tích lũy. Nếu chủ dự án muốn dễ vào ván hơn, giảm nợ ngày 3 từ 500 xuống 400 xu hoặc tăng vốn lên 1.200 xu. Mình chưa đổi vì đó là quyết định của chủ dự án.

## 4. Ca kiểm thử biên

Đầu vào lấy từ luật **[ĐÃ CHỐT]** nếu không ghi khác. Kết quả mong đợi chỉ rõ trạng thái sau thao tác.

| Mã | Tình huống | Đầu vào | Kết quả mong đợi |
| --- | --- | --- | --- |
| BT-01 | Mua hộp thứ 4 trong ngày | Đã mua 3 hộp ngày 1, tiền đủ, bấm mua hộp thứ 4 | Từ chối. Tiền, kho, số lượt mua không đổi. Quầy báo hết lượt |
| BT-02 | Đặt lại lượt mua khi sang ngày | Hết 3 lượt ngày 1, kết thúc ngày | Ngày 2 có đủ 3 lượt, lượt chưa dùng không cộng dồn |
| BT-03 | Mua khi không đủ tiền | Tiền 99 xu, hộp 100 xu | Từ chối, không trừ lượt mua |
| BT-04 | Mua đúng bằng số tiền còn lại | Tiền 100 xu, hộp 100 xu | Thành công, tiền 0, lượt mua giảm 1 |
| BT-05 | Trả nợ sớm | Ngày 1, thử thanh toán khoản ngày 2 | Từ chối, khoản ngày 2 không thể thanh toán, tiền không đổi |
| BT-06 | Trả đúng hạn, vừa đủ | Cuối ngày 1, tiền đúng 200 xu | Trả thành công, tiền 0, khoản ngày 1 đóng |
| BT-07 | Trả đúng hạn, dư tiền | Cuối ngày 1, tiền 650 xu | Trừ 200 xu, còn 450 xu, không trừ thêm |
| BT-08 | Thiếu tiền khi đến hạn | Cuối ngày 2, tiền 349 xu, nợ 350 xu | **[MỞ]** Chờ Claude 1 chốt hậu quả. Cho đến lúc đó phải ghi lại hành vi hiện tại để so sánh |
| BT-09 | Kết thúc ngày hai lần | Bấm kết thúc ngày liên tiếp hoặc nhấn đúp | Chỉ thu nợ một lần, chỉ sang ngày một lần |
| BT-10 | Bán trùng | Bấm bán cùng một hiện vật hai lần, hoặc bán từ hai nơi | Chỉ cộng tiền một lần. Món rời kho ngay lần đầu |
| BT-11 | Bán hiện vật đã bán | Bán món đã rời kho | Từ chối, tiền không đổi |
| BT-12 | Bán hàng giả | Hiện vật giả giá trị 1.000 xu, chưa giám định | Nhận 200 xu. Sau khi giám định cho thấy giả, giá hiển thị 200 xu |
| BT-13 | Mở hộp hai lần | Mở cùng một hộp đã mở | Từ chối, không sinh thêm hiện vật |
| BT-14 | Kho đầy | Kho đạt sức chứa, mua hoặc mở thêm hộp | **[MỞ]** Chờ sức chứa kho và cách xử lý món thừa |
| BT-15 | Thoát giữa thao tác | Thoát ngay sau khi mở hộp, trước khi bán | Tải lại không lặp giao dịch. Kiểm thử đầy đủ ở Sprint 2 |
| BT-16 | Thắng ngày 7 | Trả xong khoản 2.200 xu ngày 7 | **[MỞ]** Chờ Claude 1 chốt điều kiện thắng và tổng tài sản |

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

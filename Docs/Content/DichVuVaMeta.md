# Biến cố, dịch vụ chợ và mở khóa meta — CHỢ ĐỒ LẠ

Bản nháp của Claude 1 cho Sprint 4. Mọi con số là **[ĐỀ XUẤT]** để thử nghiệm. Claude 2 sẽ đo ảnh hưởng đến thanh khoản, nợ và tiến trình. Luật nền theo [GDD mục 2, 5 và 6](../GameDesign/README.md). Hiệu ứng tạo tác theo [GiaTruyenTaoTacBiKhe.md](GiaTruyenTaoTacBiKhe.md).

## 0. Quy tắc chung

- **Luật cốt lõi không bị thay đổi:** biến cố và dịch vụ không đổi lịch nợ, giới hạn 3 hộp mỗi ngày, ngưỡng thua 1.000 xu hay điều kiện thắng.
- **Giá bán thường (G):** giá bán hiện vật ngay ở quầy tại thời điểm giao dịch, đã tính hiệu ứng tăng giá bán của tạo tác, combo và biến cố. Đấu giá và cầm cố đều tính từ G.
- **Tiền chưa về tay không dùng để trả nợ:** tiền đang chờ đấu giá, hoặc hiện vật đang cầm, không tính vào tiền mặt khi thu nợ.
- **Mỗi giao dịch có một mã duy nhất.** Tải lại ván không được lặp lại giao dịch đã hoàn tất, và không được hủy giao dịch đang chờ.
- **Kết quả ngẫu nhiên được chốt theo seed và ghi vào lưu ván trước khi hiển thị.** Tải lại không đổi kết quả.
- **Hiện vật đang tham gia một dịch vụ (đấu giá, cầm cố)** không thể bán, giám định, phục chế hay đưa sang dịch vụ khác.

## 1. Biến cố

**Vai trò.** Làm mỗi ngày khác nhau, thay đổi nguồn cung, nhu cầu hoặc khả năng tiếp cận dịch vụ, buộc người chơi điều chỉnh kế hoạch giữ tiền hay đầu tư.

**Luồng.**
1. Đầu ngày 2 đến ngày 7, rút biến cố với xác suất 50%. Ngày 1 không có biến cố.
2. Nếu rút được, chọn ngẫu nhiên đều 1 trong 6 biến cố dưới đây. Mỗi ngày tối đa 1 biến cố.
3. Sổ tay công bố biến cố **trước khi người chơi mua hộp**.
4. Biến cố có hiệu lực trong ngày. Cuối ngày, sổ ghi biến cố kết thúc.

| Mã | Tên | Hiệu ứng trong ngày |
| --- | --- | --- |
| E1 | Chợ Phiên Cổ Vật | Hiện vật nhóm Quá khứ bán +15%. |
| E2 | Mất Điện Khu Công Nghệ | Hiện vật nhóm Hiện đại và Tương lai bán −15%. |
| E3 | Thuyền Hàng Ngoài Giới | Giá hộp H2 giảm 20% (350 xu còn 280 xu). |
| E4 | Giáo Sư Đi Vắng | Không giám định được. |
| E5 | Chú Rỉ Nghỉ Lễ | Không phục chế được. |
| E6 | Khách Sộp Đến Chợ | Hiện vật có độ hiếm Rất hiếm bán +10%. |

**Quy tắc.** Hiệu ứng tăng hoặc giảm giá bán cộng với các hiệu ứng giá bán khác theo phép cộng, đúng quy tắc ở Sprint 2.

**Điều kiện hoàn tất.** Biến cố hoàn tất khi sang ngày kế tiếp. Trạng thái "đang hiệu lực" được lưu, nên tải lại trong ngày vẫn thấy đúng biến cố.

**Ca kiểm thử.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Ngày 1 bắt đầu ván. | Không có biến cố. |
| 2 | Ngày 3 rút được E4, người chơi muốn giám định. | Không giám định được. Sổ ghi lý do. Ngày 4 giám định bình thường. |
| 3 | E1 và người chơi trang bị CP1 (+5% nhóm Quá khứ). | Bán hiện vật Quá khứ được +20% theo phép cộng. |
| 4 | Tải lại ván giữa ngày 3 đang có E3. | Vẫn thấy E3 và giá H2 là 280 xu, không rút lại biến cố. |

## 2. Đấu giá

**Vai trò.** Cách bán hiện vật giá trị cao với giá kỳ vọng tốt hơn, đổi lại tiền về chậm một ngày và có rủi ro. Người chơi phải cân nhắc với kỳ nợ sắp đến.

**Luồng.**
1. Trong ngày, gửi 1 hiện vật vào phiên đấu giá chiều và chọn giá sàn: không sàn, hoặc 1,0 G, hoặc 1,3 G.
2. Trả ngay phí đăng ký 5% G (làm tròn lên). Phí không hoàn.
3. Hiện vật bị khóa ở trạng thái "Chờ kết quả".
4. Đầu ngày kế tiếp, kết quả công bố: giá chốt = G × k, trong đó k = 0,9 / 1,1 / 1,3 / 1,5 với xác suất 20% / 40% / 30% / 10%. Giá trị kỳ vọng của k là 1,16.
5. Nếu giá chốt không thấp hơn giá sàn thì bán thành công và tiền vào ngay đầu ngày. Nếu thấp hơn, hiện vật được trả lại và phí không hoàn.

**Quy tắc.**
- Mỗi ngày tối đa 1 hiện vật gửi đấu giá.
- Không gửi đấu giá vào ngày 7, vì tiền không kịp về trước khi ván kết thúc.
- Tiền đấu giá về vào đầu ngày nên có thể dùng trả nợ cuối ngày đó.

**Điều kiện hoàn tất.** Một lần đấu giá hoàn tất khi hiện vật được bán và tiền vào đúng một lần, hoặc hiện vật được trả về. Trạng thái "Chờ kết quả" nằm trong lưu ván.

**Ca kiểm thử.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Ngày 3 gửi V10 (G = 1.800 xu), không giá sàn. | Trả phí 90 xu. Đầu ngày 4 nếu k = 1,3 thì nhận 2.340 xu. |
| 2 | Gửi V10, sàn 1,3 G, kết quả k = 1,1 (1.980 xu, thấp hơn sàn 2.340). | Không bán, hiện vật trả về, mất phí 90 xu. |
| 3 | Gửi vào ngày 7. | Bị từ chối. |
| 4 | Thiếu 300 xu khi thu nợ cuối ngày, hiện vật đang chờ kết quả. | Không dùng được. Nợ tồn tính theo luật nợ. |
| 5 | Tải lại ván sau khi gửi, trước khi có kết quả. | Hiện vật vẫn bị khóa, không gửi lần hai, phí không bị trừ lại. |

## 3. Sòng bạc

**Vai trò.** Lựa chọn rủi ro cho người chơi đang thiếu tiền hoặc muốn tăng tốc, với kỳ vọng âm nên không phải cách kiếm tiền ổn định.

**Luồng.**
1. Sòng bạc mở từ ngày 2.
2. Người chơi chọn mức cược 50, 100 hoặc 200 xu và phải có đủ tiền mặt.
3. Kết quả chốt theo seed: thắng với xác suất 45%, được nhận lại cược cộng thêm đúng bằng cược. Thua với xác suất 55%, mất cược.
4. Kỳ vọng mỗi lượt là −10% mức cược.

**Quy tắc.**
- Mỗi ngày tối đa 3 lượt. Đủ 3 lượt thì sòng đóng đến hết ngày.
- Chỉ cược bằng tiền mặt, không cược bằng hiện vật.
- Không có cược tự động hoặc cược nhiều lượt cùng lúc.

**Điều kiện hoàn tất.** Một lượt hoàn tất khi kết quả được chốt và tiền cập nhật đúng một lần. Số lượt đã chơi trong ngày được lưu.

**Ca kiểm thử.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Cược 100 xu và thắng. | Tiền tăng 100 xu so với trước khi cược. |
| 2 | Cược 100 xu và thua. | Tiền giảm 100 xu. |
| 3 | Chơi lượt thứ 4 trong ngày. | Bị từ chối. |
| 4 | Có 80 xu, chọn cược 100 xu. | Bị từ chối vì không đủ tiền. |
| 5 | Thua, tải lại ván rồi cược lại để thử đổi kết quả. | Kết quả lượt đã lưu, tải lại không đổi và không trả lại cược. |

## 4. Cầm cố

**Vai trò.** Đổi hiện vật lấy tiền mặt ngay mà không bán hẳn. Người chơi có thể giữ món quý nhưng chịu chi phí và nguy cơ mất món.

**Luồng.**
1. Đưa 1 hiện vật cho tiệm cầm đồ, nhận tiền cầm bằng 60% G (làm tròn xuống). Tiền vào ngay và có thể dùng trả nợ cùng ngày.
2. Hạn chuộc là hết ngày thứ hai sau ngày cầm. Cầm ngày 3 thì chuộc được ngày 4 và ngày 5.
3. Chuộc: trả tiền cầm nhân 1,15 (làm tròn lên), nhận lại hiện vật.
4. Quá hạn mà chưa chuộc, hiện vật mất và không hoàn lại.

**Quy tắc.**
- Tối đa 2 hiện vật đang cầm cùng lúc.
- Hiện vật đang cầm không bán, giám định, phục chế hay đưa đấu giá được.
- Hết ván (sau kỳ nợ ngày 7), hiện vật chưa chuộc coi như mất và không tính tài sản.
- Chỉ cầm hiện vật, không cầm tạo tác hay gia truyền.

**Điều kiện hoàn tất.** Một lần cầm hoàn tất khi hiện vật được chuộc hoặc bị mất, đúng một lần. Hạn chuộc nằm trong lưu ván.

**Ca kiểm thử.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Ngày 3 cầm V10 (G = 1.800 xu). | Nhận 1.080 xu. |
| 2 | Chuộc V10 vào ngày 4. | Trả 1.242 xu và nhận lại V10. |
| 3 | Không chuộc V10 trước hết ngày 5. | Mất V10. |
| 4 | Đang cầm 2 món, cầm thêm món thứ 3. | Bị từ chối. |
| 5 | Ngày 5 thiếu 300 xu trước khi thu nợ, cầm V06 (G = 900 xu). | Nhận 540 xu, dùng trả nợ cùng ngày. |
| 6 | Tải lại ván sau khi cầm. | Món vẫn ở trạng thái đang cầm, không nhận tiền lần hai. |

## 5. Mở khóa meta

**Vai trò.** Cho người chơi cảm giác tiến bộ giữa các ván bằng cách mở thêm lựa chọn và thông tin, nhưng không biến ván sau thành dễ hơn nhiều.

**Sổ Sưu Tầm.** Ghi hiện vật V01–V10 khi người chơi **soi xong lần đầu**, tính theo mã hiện vật nên nhặt lại không tính hai lần. Tiến độ lưu riêng, không mất khi thua ván.

| Mã | Tên | Nguồn mở khóa | Lựa chọn hoặc ảnh hưởng |
| --- | --- | --- | --- |
| M1 | Chọn Gia Truyền | Sưu tầm 5 hiện vật khác nhau. | Được chọn 1 trong 2 cổ vật gia truyền đầu ván, thay vì rút ngẫu nhiên. |
| M2 | Vốn Khởi Nghiệp Nhỏ | Sưu tầm đủ 10 hiện vật. | Vốn đầu ván +100 xu (1.100 xu). |
| M3 | Thử Thách Rương Đen | Thắng 1 ván. | Tùy chọn bật Rương Đen xuất hiện từ ngày 1 thay vì ngày 4. |
| M4 | Bí Khế Quen Mặt | Đạt mỗi Bí Khế ít nhất một lần. | Sổ hiện sẵn điều kiện các Bí Khế từ đầu ván. |

**Giới hạn ảnh hưởng.** Tổng tăng vốn đầu ván từ mọi mở khóa không vượt 100 xu (10% vốn gốc). Các mở khóa còn lại là lựa chọn hoặc thông tin, không tăng sức mạnh trực tiếp.

**Điều kiện hoàn tất.** Một mở khóa hoàn tất khi điều kiện đạt và được ghi vào meta, đúng một lần. Thắng một ván chỉ tính một lần cho mỗi ván, dù tải lại nhiều lần.

**Ca kiểm thử.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Soi V01 ở ván 1, rồi soi V01 ở ván 2. | Sưu tầm V01 một lần. |
| 2 | Sưu tầm đủ 5 hiện vật khác nhau. | Mở M1. Ván sau có lựa chọn gia truyền. |
| 3 | Thua ván sau khi đã mở M1. | M1 vẫn còn. |
| 4 | Thắng ván, tải lại màn kết quả nhiều lần. | Chỉ tính 1 lần thắng, mở M3 một lần. |
| 5 | Mở M2 và chọn vốn đầu ván. | Vốn 1.100 xu, không vượt giới hạn +100 xu. |

## 6. Tương tác giữa các hệ

- Biến cố E1, E2 và E6 thay đổi G, nên đổi cả giá cầm và giá chốt đấu giá.
- E4 và E5 không chặn đấu giá, cầm cố hay sòng bạc.
- Ngày 7 không đấu giá được. Cầm cố vẫn dùng được để lấy tiền trả kỳ cuối, nhưng món chưa chuộc coi như mất.
- Sòng bạc không có giới hạn theo nợ, nên người chơi có thể cược mức cao khi sắp thua. Đây là cố ý.

## 7. Cần Claude 2 kiểm tra

- Đấu giá có kỳ vọng 1,16 G trừ phí 5%, chậm một ngày. Có đủ hấp dẫn để người chơi chọn thay vì bán ngay không.
- Sòng bạc kỳ vọng −10%. Có đủ rủi ro hấp dẫn khi người chơi sắp thua không, hay nên tăng tỷ lệ thắng.
- Cầm cố 60% G, phí 15%, hạn 2 ngày. Có cho người chơi cứu được ván thiếu tiền mà không quá rẻ không.
- Vốn +100 xu của M2 có làm ván sau quá dễ không, nhất là khi H1 đang lời nhiều (xem [RaSoatGiay.md](RaSoatGiay.md)).
- Tần suất biến cố 50% mỗi ngày từ ngày 2 có đủ làm khác biệt giữa các ván, không quá dày.

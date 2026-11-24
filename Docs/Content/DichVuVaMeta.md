# Biến cố, dịch vụ chợ và mở khóa meta — CHỢ ĐỒ LẠ

Bản nháp của Claude 1 cho Sprint 4. Mọi con số là **[ĐỀ XUẤT]** để thử nghiệm. Luật nền theo [GDD mục 2, 5 và 6](../GameDesign/README.md). Hiệu ứng tạo tác theo [GiaTruyenTaoTacBiKhe.md](GiaTruyenTaoTacBiKhe.md), **đã rà theo gói điều chỉnh ở [DieuChinhSauCanBang.md](DieuChinhSauCanBang.md)** (giá tạo tác tier 3 là 5.000.000 đ, CP1 +5% mọi hiện vật, CB1 +10%, CB3 miễn phí giám định và +15% giá bán, chứng nhận giám định +25%, phí giám định 15%). Claude 2 đã đo đấu giá và cầm tạo tác theo gói này ([DoLaiDeXuat.md](../Balance/DoLaiDeXuat.md)). Sòng bạc, biến cố và meta chưa đo theo gói mới, xem mục 7.

## 0. Quy tắc chung

- **Luật cốt lõi không bị thay đổi:** biến cố và dịch vụ không đổi lịch nợ, giới hạn 3 hộp mỗi ngày, ngưỡng thua 10.000.000 đ hay điều kiện thắng.
- **Giá bán thường (G):** giá bán hiện vật ngay ở quầy tại thời điểm giao dịch. G = giá nền × hệ số chất lượng (Nguyên 1,0, Xước 0,7, Hỏng 0,3) × 0,2 nếu là hàng giả, rồi nhân (1 + tổng hiệu ứng tăng giá bán của tạo tác, combo, biến cố và chứng nhận giám định +25% cho hàng thật đã giám định). Đấu giá và cầm cố đều tính từ G.
- **Các ví dụ giá trong tài liệu này** dùng hiện vật thật, chất lượng Nguyên, chưa giám định, theo bảng giá nền đã duyệt (hệ số 0,85 nhân 1,10, xem [NoiDungMau.md](NoiDungMau.md)).
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
| E3 | Thuyền Hàng Ngoài Giới | Giá hộp H2 giảm thêm 20% so với giá hôm nay, tính sau dao động giá hằng ngày (ở giá gốc, 3.500.000 đ còn 2.800.000 đ). |
| E4 | Giáo Sư Đi Vắng | Không giám định được. |
| E5 | Chú Rỉ Nghỉ Lễ | Không phục chế được. |
| E6 | Khách Sộp Đến Chợ | Hiện vật Rất hiếm bán +10%. Độ hiếm Rất hiếm gồm V07 Tinh thể lam và V10 Mảnh vảy long, hai món giá nền cao nhất (đã ghi trong [NoiDungMau.md](NoiDungMau.md)). |

**Quy tắc.** Hiệu ứng tăng hoặc giảm giá bán cộng với các hiệu ứng giá bán khác theo phép cộng, đúng quy tắc ở Sprint 2.

**Điều kiện hoàn tất.** Biến cố hoàn tất khi sang ngày kế tiếp. Trạng thái "đang hiệu lực" được lưu, nên tải lại trong ngày vẫn thấy đúng biến cố.

**Ca kiểm thử.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Ngày 1 bắt đầu ván. | Không có biến cố. |
| 2 | Ngày 3 rút được E4, người chơi muốn giám định. | Không giám định được. Sổ ghi lý do. Ngày 4 giám định bình thường. |
| 3 | E1 (+15% nhóm Quá khứ) và người chơi trang bị CP1 (+5% mọi hiện vật). | Hiện vật Quá khứ bán thêm +20% theo phép cộng. Hiện vật nhóm khác chỉ bán thêm +5% từ CP1. |
| 4 | Tải lại ván giữa ngày 3 đang có E3. | Vẫn thấy E3 và giá H2 hôm nay giữ nguyên (ở giá gốc là 2.800.000 đ), không rút lại biến cố. |

## 2. Đấu giá

**Vai trò.** Cách bán hiện vật giá trị cao với giá kỳ vọng tốt hơn, đổi lại tiền về chậm một ngày và có rủi ro. Người chơi phải cân nhắc với kỳ nợ sắp đến.

**Luồng.**
1. Trong ngày, gửi 1 hiện vật có G từ **13.000.000 đ** trở lên vào phiên đấu giá chiều và chọn giá sàn: không sàn, hoặc 1,0 G, hoặc 1,3 G. Món rẻ hơn không được đấu giá.
2. Trả ngay phí đăng ký 5% G (làm tròn lên). Phí không hoàn.
3. Hiện vật bị khóa ở trạng thái "Chờ kết quả".
4. Đầu ngày kế tiếp, kết quả công bố: giá chốt = G × k, trong đó k = 0,9 / 1,1 / 1,3 / 1,5 với xác suất 20% / 40% / 30% / 10%. Giá trị kỳ vọng của k là 1,16. Claude 2 đo trên gói mới ([DoLaiDeXuat.md](../Balance/DoLaiDeXuat.md) mục 6): k kỳ vọng 1,16 cho bot thường thêm 3 đến 6 điểm, còn k 1,40 cho thêm khoảng 17 điểm, gần như gấp đôi tỷ lệ thắng, quá dễ so với mục tiêu. Với k 1,16, đấu giá chỉ lời với món đắt và giá sàn thấp, còn lại là cái bẫy, đúng là một đánh đổi thật.
5. Nếu giá chốt không thấp hơn giá sàn thì bán thành công và tiền vào ngay đầu ngày. Nếu thấp hơn, hiện vật được trả lại và phí không hoàn.

**Quy tắc.**
- Mỗi ngày tối đa 1 hiện vật gửi đấu giá.
- Không gửi đấu giá vào ngày 7, vì tiền không kịp về trước khi ván kết thúc.
- Tiền đấu giá về vào đầu ngày kế tiếp nên có thể dùng trả nợ cuối ngày đó, nhưng **không** dùng được trả nợ ngày gửi. Muốn có tiền cho một kỳ nợ thì phải gửi sớm một ngày. UI cần ghi rõ điều này để người chơi không thấy bị phạt oan.

**Điều kiện hoàn tất.** Một lần đấu giá hoàn tất khi hiện vật được bán và tiền vào đúng một lần, hoặc hiện vật được trả về. Trạng thái "Chờ kết quả" nằm trong lưu ván.

**Ca kiểm thử.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Ngày 3 gửi V10 thật, Nguyên, chưa giám định (G = 28.600.000 đ), không giá sàn. | Trả phí 1.430.000 đ. Đầu ngày 4 nếu k = 1,3 thì nhận 37.180.000 đ. |
| 2 | Gửi V10, sàn 1,3 G (37.180.000 đ), kết quả k = 1,1 (31.460.000 đ, thấp hơn sàn). | Không bán, hiện vật trả về, mất phí 1.430.000 đ. |
| 2b | Gửi V10, không giá sàn, kết quả k = 0,9. | Nhận 25.740.000 đ, thấp hơn giá bán thường 28.600.000 đ và còn mất phí 1.430.000 đ. |
| 3 | Gửi vào ngày 7. | Bị từ chối. |
| 3b | Gửi V09 (G = 8.950.000 đ). | Bị từ chối vì G dưới 13.000.000 đ. |
| 4 | Thiếu 3.000.000 đ khi thu nợ cuối ngày, hiện vật đang chờ kết quả. | Không dùng được. Nợ tồn tính theo luật nợ. |
| 5 | Tải lại ván sau khi gửi, trước khi có kết quả. | Hiện vật vẫn bị khóa, không gửi lần hai, phí không bị trừ lại. |

## 3. Sòng bạc

**Vai trò.** Lựa chọn rủi ro cho người chơi đang thiếu tiền hoặc muốn tăng tốc, với kỳ vọng âm nên không phải cách kiếm tiền ổn định.

**Luồng.**
1. Sòng bạc mở từ ngày 2.
2. Người chơi chọn mức cược 1.000.000, 2.500.000 hoặc 5.000.000 đ và phải có đủ tiền mặt.
3. Kết quả chốt theo seed: thắng với xác suất 45%, được nhận lại cược cộng thêm đúng bằng cược. Thua với xác suất 55%, mất cược.
4. Kỳ vọng mỗi lượt là −10% mức cược.

**Quy tắc.**
- Mỗi ngày tối đa 2 lượt. Đủ 2 lượt thì sòng đóng đến hết ngày.
- Claude 2 đo ở [Sprint 4](../Balance/Sprint4.md): sòng bạc đổi tỷ lệ thắng không quá 1,5 điểm và **không** giúp gỡ lại sau khi thiếu tiền, vì khoản thiếu lớn hơn mức cược tối đa. Sòng bạc là một canh bạc, không phải đường cứu.
- Chỉ cược bằng tiền mặt, không cược bằng hiện vật.
- Không có cược tự động hoặc cược nhiều lượt cùng lúc.

**Điều kiện hoàn tất.** Một lượt hoàn tất khi kết quả được chốt và tiền cập nhật đúng một lần. Số lượt đã chơi trong ngày được lưu.

**Ca kiểm thử.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Cược 1.000.000 đ và thắng. | Tiền tăng 1.000.000 đ so với trước khi cược. |
| 2 | Cược 1.000.000 đ và thua. | Tiền giảm 1.000.000 đ. |
| 3 | Chơi lượt thứ 3 trong ngày. | Bị từ chối. |
| 4 | Có 800.000 đ, chọn cược 1.000.000 đ. | Bị từ chối vì không đủ tiền. |
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
- Cầm được **hiện vật và tạo tác**, không cầm gia truyền. Tiền cầm tạo tác bằng 60% giá mua. Trong lúc cầm, tạo tác bị tháo khỏi ô trang bị nên mất cả hiệu ứng và combo. Đây là đánh đổi chính của cầm cố: lấy thanh khoản bằng cách tạm bỏ một combo đã đầu tư.
- Chuộc tạo tác xong, tạo tác về túi tạo tác (không chiếm kho 8 ô). Người chơi phải **trang bị lại** vào một ô trống thì mới có hiệu ứng. Quá hạn mà chưa chuộc thì mất tạo tác.
- Claude 2 đo ở [Sprint 4](../Balance/Sprint4.md) và [DoLaiDeXuat.md](../Balance/DoLaiDeXuat.md) mục 5: cầm tạo tác chỉ đổi tỷ lệ thắng từ −0,1 đến +0,9 điểm và cầm cố nói chung không phụ thuộc nhiều vào phí chuộc, nên giữ phí 15% và hạn 2 ngày. Nó vô hại nhưng chưa phải lựa chọn lớn, vì thường chỉ dùng khi sắp thiếu tiền mà lúc đó đã khá muộn. Cầm cố thú vị hơn khi người chơi có thứ đáng giá đang giữ, nhờ kho 8 ô và tạo tác ở [DieuChinhSauCanBang.md](DieuChinhSauCanBang.md).

**Tiền cầm và chuộc tạo tác** theo giá mua của gói mới (CP3 và TC3 đã giảm xuống 5.000.000 đ):

| Tạo tác | Giá mua | Tiền cầm (60%) | Chuộc (tiền cầm × 1,15, làm tròn lên) |
| --- | --- | --- | --- |
| CP1 | 1.500.000 đ | 900.000 đ | 1.040.000 đ |
| CP2 | 4.000.000 đ | 2.400.000 đ | 2.760.000 đ |
| CP3 | 5.000.000 đ | 3.000.000 đ | 3.450.000 đ |
| TC1 | 1.200.000 đ | 720.000 đ | 830.000 đ |
| TC2 | 4.500.000 đ | 2.700.000 đ | 3.110.000 đ |
| TC3 | 5.000.000 đ | 3.000.000 đ | 3.450.000 đ |

**Điều kiện hoàn tất.** Một lần cầm hoàn tất khi hiện vật được chuộc hoặc bị mất, đúng một lần. Hạn chuộc nằm trong lưu ván.

**Ca kiểm thử.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Ngày 3 cầm V10 (G = 28.600.000 đ). | Nhận 17.160.000 đ. |
| 2 | Chuộc V10 vào ngày 4. | Trả 19.740.000 đ và nhận lại V10. |
| 3 | Không chuộc V10 trước hết ngày 5. | Mất V10. |
| 4 | Đang cầm 2 món, cầm thêm món thứ 3. | Bị từ chối. |
| 5 | Ngày 5 thiếu 3.000.000 đ trước khi thu nợ, cầm V06 (G = 18.450.000 đ). | Nhận 11.070.000 đ, dùng trả nợ cùng ngày. |
| 6 | Tải lại ván sau khi cầm. | Món vẫn ở trạng thái đang cầm, không nhận tiền lần hai. |
| 7 | Cầm CP3 (giá mua 5.000.000 đ) đang trang bị. | Nhận 3.000.000 đ. CP3 bị tháo khỏi ô trang bị, mất hiệu ứng +10% giá bán hiện vật Thần thoại. Nếu đang có CB3 (CP3 và TC3) thì mất luôn CB3: giám định không còn miễn phí và mất +15% giá bán. |
| 8 | Chuộc CP3 vào ngày kế tiếp. | Trả 3.450.000 đ. CP3 về túi tạo tác. Người chơi trang bị lại thì hiệu ứng trở lại, CB3 trở lại nếu vẫn còn TC3. |
| 9 | Chuộc CP3 khi 3 ô trang bị đã đầy. | Trả 3.450.000 đ. CP3 ở trong túi tạo tác, chưa có hiệu ứng cho đến khi người chơi thay món. Không chiếm kho 8 ô. |
| 10 | Không chuộc CP3 trước hết ngày hạn. | Mất CP3, không hoàn tiền cầm. |

## 5. Mở khóa meta

**Vai trò.** Cho người chơi cảm giác tiến bộ giữa các ván bằng cách mở thêm lựa chọn và thông tin, nhưng không biến ván sau thành dễ hơn nhiều.

**Sổ Sưu Tầm.** Ghi hiện vật V01–V10 khi người chơi **soi xong lần đầu**, tính theo mã hiện vật nên nhặt lại không tính hai lần. Tiến độ lưu riêng, không mất khi thua ván.

| Mã | Tên | Nguồn mở khóa | Lựa chọn hoặc ảnh hưởng |
| --- | --- | --- | --- |
| M1 | Chọn Gia Truyền | Sưu tầm 5 hiện vật khác nhau. | Được chọn 1 trong 2 cổ vật gia truyền đầu ván, thay vì rút ngẫu nhiên. |
| M2 | Vốn Khởi Nghiệp Nhỏ | Sưu tầm đủ 10 hiện vật. | Vốn đầu ván +1.000.000 đ (11.000.000 đ). |
| M3 | Thử Thách Rương Đen | Thắng 1 ván. | Tùy chọn bật Rương Đen xuất hiện từ ngày 1 thay vì ngày 4. |
| M4 | Bí Khế Quen Mặt | Đạt mỗi Bí Khế ít nhất một lần. | Sổ hiện sẵn điều kiện các Bí Khế từ đầu ván. |

**Giới hạn ảnh hưởng.** Tổng tăng vốn đầu ván từ mọi mở khóa không vượt 1.000.000 đ (10% vốn gốc). Các mở khóa còn lại là lựa chọn hoặc thông tin, không tăng sức mạnh trực tiếp.

**Điều kiện hoàn tất.** Một mở khóa hoàn tất khi điều kiện đạt và được ghi vào meta, đúng một lần. Thắng một ván chỉ tính một lần cho mỗi ván, dù tải lại nhiều lần.

**Ca kiểm thử.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Soi V01 ở ván 1, rồi soi V01 ở ván 2. | Sưu tầm V01 một lần. |
| 2 | Sưu tầm đủ 5 hiện vật khác nhau. | Mở M1. Ván sau có lựa chọn gia truyền. |
| 3 | Thua ván sau khi đã mở M1. | M1 vẫn còn. |
| 4 | Thắng ván, tải lại màn kết quả nhiều lần. | Chỉ tính 1 lần thắng, mở M3 một lần. |
| 5 | Mở M2 và chọn vốn đầu ván. | Vốn 11.000.000 đ, không vượt giới hạn +1.000.000 đ. |

## 6. Tương tác giữa các hệ

- Biến cố E1, E2 và E6 thay đổi G, nên đổi cả giá cầm và giá chốt đấu giá.
- Hàng giả có G chỉ bằng 20% giá thật, nên cầm cố và đấu giá cho rất ít tiền (và thường dưới ngưỡng đấu giá 13.000.000 đ). Hàng thật đã giám định có G cao hơn 25%, nên cầm hoặc đấu giá được nhiều hơn. Đây là thêm một lý do để giám định món đắt trước khi đưa vào dịch vụ.
- Cầm tạo tác lấy tiền ngay nhưng mất hiệu ứng, kể cả hiệu ứng tăng giá bán. Nên bán hiện vật trước rồi mới cầm tạo tác trong cùng ngày, vì cầm trước làm các món bán sau mất phần cộng giá.
- E4 và E5 không chặn đấu giá, cầm cố hay sòng bạc.
- Ngày 7 không đấu giá được. Cầm cố vẫn dùng được để lấy tiền trả kỳ cuối, nhưng món chưa chuộc coi như mất.
- Sòng bạc không có giới hạn theo nợ, nên người chơi có thể cược mức cao khi sắp thua. Đây là cố ý.

## 7. Kết quả đo của Claude 2 và việc còn lại

Claude 2 đã đo các dịch vụ trong [Sprint 4](../Balance/Sprint4.md) bằng luật giả định, vì lúc đó luật này chưa có. Bản này đã chỉnh theo khoảng Claude 2 khuyến nghị:

| Hệ | Khuyến nghị của Claude 2 | Cách bản này đáp ứng |
| --- | --- | --- |
| Đấu giá | Ở ván khó giữ k 1,16. k 1,40 cho thêm khoảng 17 điểm, quá dễ (DoLaiDeXuat.md) | k kỳ vọng 1,16, chỉ món từ 13.000.000 đ. **Đã đo theo gói mới** |
| Sòng bạc | Không đổi cân bằng, lợi thế nhà cái 5–10% an toàn | Giữ lợi thế nhà cái 10%, mức cược 1.000.000 / 2.500.000 / 5.000.000 đ, 2 lượt mỗi ngày |
| Cầm cố | Tác động nhỏ, phí 15% là đủ. Cầm tạo tác −0,1 đến +0,9 điểm | Giữ 60%, phí 15%, thêm cầm tạo tác để có đánh đổi. **Đã đo theo gói mới** |
| Biến cố | Biên độ tối đa ±20% | Biến cố giá bán là ±15%, ±15%, +10%, thấp hơn mức này |
| Meta | Ngân sách khoảng +4.000.000 đ vốn tương đương | M2 chỉ +1.000.000 đ, các mở khóa khác là lựa chọn hoặc thông tin |

**Theo mục tiêu độ khó của chủ dự án** (xem [DieuChinhSauCanBang.md](DieuChinhSauCanBang.md)), các dịch vụ này là nguồn đánh đổi, không phải đường cứu:
- Đấu giá: giá cao hơn nhưng tiền về chậm và có thể thấp hơn giá bán thường.
- Cầm cố: có tiền ngay nhưng tạm mất combo hoặc có nguy cơ mất món.
- Sòng bạc: kỳ vọng âm, chỉ hợp khi đã tính kỹ.
- Biến cố: buộc chọn giữa bán ngay và chờ giá.

**Việc còn lại cho Claude 2:**
- Đấu giá (k 1,16, món từ 13.000.000 đ) và cầm tạo tác đã được đo theo gói mới. Sòng bạc (cược 1.000.000 / 2.500.000 / 5.000.000 đ, 2 lượt), biến cố và meta chưa đo theo gói mới.
- Khi chủ dự án duyệt, chuyển luật dịch vụ này vào `QA/services_model.py` và `QA/sprint4_sim.py`, rồi xác nhận khoảng an toàn vẫn đúng.
- Nếu chủ dự án duyệt phạt rủi ro (giá nền ×1,10, hàng giả theo hộp), đo lại đấu giá và cầm cố vì G thay đổi.

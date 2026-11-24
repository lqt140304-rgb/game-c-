# Biến cố E3 và việc chọn loại hộp (Claude 2)

> **Tiền tệ:** số tiền trong tài liệu này đã đổi sang **đồng Việt Nam** ngày 2026-11-24 (1 xu cũ = 10.000 đ, xem [bảng quy đổi](README.md#tiền-tệ)). Mô phỏng vẫn tính theo đơn vị cân bằng 10.000 đ, `QA/currency.py` đổi sang đồng khi in.

Đo ảnh hưởng của biến cố, đặc biệt **E3 Thuyền Hàng Ngoài Giới** (giá hộp H2 giảm 20%, 3.500.000 đ còn 2.800.000 đ), lên việc chọn loại hộp mỗi lượt và lên tỷ lệ thắng. Dùng luật chốt của Claude 1: biến cố 50% số ngày từ ngày 2, chọn đều trong E1 đến E6, sổ công bố **trước khi mua hộp**; mỗi lượt Dì Mận bày 1 hộp mỗi loại, người chơi chọn mua 1 hoặc dừng. Cấu hình đã duyệt (G1, λ = 0,75, manh mối miễn phí). 2.000 seed.

## Kết luận ngắn

1. **E3 chỉ dịch việc chọn hộp nhẹ:** tỷ lệ H2 trong các lần mua tăng từ 66% đến 67% lên 74% đến 76% (khoảng +7 đến +9 điểm phần trăm), H1 giảm từ 16% đến 19% xuống 7% đến 8%. Không tạo ra một quyết định mới.
2. **E3 cho lời nhẹ:** lời mỗi ngày E3 cao hơn ngày thường khoảng 25% (ví dụ thận trọng 853 so với 6.770.000 đ), và tỷ lệ thắng cao hơn +0,3 đến +1,1 điểm khi chỉ có E3.
3. **Cả sáu biến cố gộp lại gần như trung tính:** −1,4 đến +0,5 điểm. Người chơi thích nghi (tính biến cố vào giá trị kỳ vọng) không khác ngây thơ trong các biến cố chỉ đổi giá hộp như E3, và chỉ khác 0 đến 1,5 điểm với biến cố đổi giá bán (E1, E2, E6).
4. **Phát hiện lớn hơn câu hỏi:** việc chọn loại hộp gần như bị **H2 chi phối** ngay cả ngày thường (66% đến 67% lượt mua), và **Rương Đen không bao giờ được mua** (0%). Đây là vấn đề cho mục tiêu "nhiều tình huống đánh đổi" (mục 3).

## Cách tái hiện

```bash
cd QA
python3 event_sim.py        # khoảng 10 phút
```

## 1. Tỷ lệ thắng theo biến cố

Thắng%, thứ tự thận trọng / liều / combo nhỏ / combo lớn (đều có giám định).

| Cấu hình | Thận trọng | Liều | Combo nhỏ | Combo lớn |
| --- | --- | --- | --- | --- |
| Không biến cố | 38,5 | 45,2 | 45,8 | 46,4 |
| E1 đến E6, người chơi ngây thơ | 37,8 (−0,7) | 43,9 (−1,3) | 45,5 (−0,3) | 45,6 (−0,8) |
| E1 đến E6, người chơi thích nghi | 38,5 (0,0) | 43,8 (−1,4) | 46,3 (+0,5) | 46,1 (−0,3) |
| Chỉ E3 | 39,5 (+1,0) | 45,5 (+0,3) | 46,9 (+1,1) | 46,9 (+0,5) |
| Chỉ E1 | 38,5 (0,0) | 45,5 (+0,3) | 45,9 (+0,1) | 46,5 (+0,1) |
| Chỉ E2 | 38,4 (−0,1) | 44,9 (−0,3) | 45,9 (+0,1) | 46,3 (−0,1) |
| Chỉ E4 | 37,9 (−0,6) | 44,5 (−0,7) | 45,8 (0,0) | 46,9 (+0,5) |
| Chỉ E5 | 38,5 (0,0) | 44,8 (−0,4) | 45,6 (−0,2) | 46,0 (−0,4) |
| Chỉ E6 | 38,2 (−0,3) | 44,7 (−0,5) | 45,8 (0,0) | 46,2 (−0,2) |

Mọi chênh lệch nằm trong sai số (khoảng ±1,1 điểm với 2.000 seed). Biến cố không làm game dễ hay khó hơn, đúng với ý "làm ván khác nhau chứ không đổi độ khó".

## 2. E3 và việc chọn loại hộp

Tỷ lệ mỗi loại hộp trong các lần mua, ngày E3 so với ngày thường (chỉ E3, người chơi thích nghi):

| Chiến thuật | Ngày | H1 | H2 | H3 | RD | Lời mỗi ngày |
| --- | --- | --- | --- | --- | --- | --- |
| Thận trọng | E3 | 7% | **74%** | 19% | 0% | 8.530.000 đ |
| | thường | 16% | 67% | 17% | 0% | 6.770.000 đ |
| Liều | E3 | 3% | **76%** | 22% | 0% | 9.300.000 đ |
| | thường | 8% | 71% | 21% | 0% | 7.800.000 đ |
| Combo nhỏ | E3 | 8% | **74%** | 18% | 0% | 11.270.000 đ |
| | thường | 18% | 67% | 15% | 0% | 9.090.000 đ |
| Combo lớn | E3 | 8% | **74%** | 18% | 0% | 13.930.000 đ |
| | thường | 19% | 66% | 15% | 0% | 11.300.000 đ |

- Vào ngày E3, người chơi chuyển khoảng 7 đến 9 điểm phần trăm số lần mua từ H1 sang H2. H3 gần như không đổi.
- Lời mỗi ngày E3 cao hơn ngày thường khoảng 20% đến 26%. Với tiền hôm nay trả nợ cuối ngày thì đó là khoản đệm có ích nhưng không quyết định.

## 3. Phát hiện: H2 chi phối, Rương Đen là nội dung chết

Giá trị kỳ vọng và lời kỳ vọng mỗi hộp ở cấu hình đã duyệt (G1):

| Hộp | Giá | Lần giá hộp | Lời kỳ vọng mỗi hộp | Tỷ lệ được chọn (ngày thường) |
| --- | --- | --- | --- | --- |
| H1 | 1.000.000 đ | 2,50 | khoảng +1.500.000 đ | 8% đến 19% |
| **H2** | 3.500.000 đ | 1,59 | khoảng **+2.060.000 đ** | **66% đến 71%** |
| H3 | 8.000.000 đ | 1,24 | khoảng +1.920.000 đ | 15% đến 21% |
| RD | 15.000.000 đ | 1,01 | khoảng +150.000 đ | **0%** |

- Người chơi chọn hộp có **lời tuyệt đối** cao nhất mỗi lượt, và H2 luôn nhỉnh hơn H3 (206 so với 192) trong khi **rẻ hơn 2,3 lần**. H2 gần như luôn là lựa chọn đúng, nên "hộp rẻ hay đắt" không còn là quyết định.
- **Rương Đen lời gần 0** và không ai mua. Nếu giữ hộp này thì nó chỉ có ích cho người cần hiện vật cụ thể (V07 nhóm Ngoài hành tinh), không phải lựa chọn kinh tế. Mở khóa M3 (Rương Đen từ ngày 1) càng dễ thành bẫy.
- **Hệ quả cho mục tiêu "nhiều đánh đổi":** đánh đổi giữa các hộp phải phụ thuộc vào **số tiền đang có** và **rủi ro**. Hiện tại, bất kể giàu hay nghèo, người chơi chọn H2. Để hộp đắt thành lựa chọn của người đã có vốn, lời tuyệt đối của hộp đắt phải cao hơn hẳn (và rủi ro lớn hơn), ví dụ lời kỳ vọng mỗi hộp khoảng H1 1.200.000, H2 2.000.000, H3 3.200.000, RD 4.500.000 đ kèm biến thiên lớn. Mô phỏng chưa đo cấu hình này.

## 4. Đề xuất

| Hạng mục | Đề xuất |
| --- | --- |
| E3 | Giữ nguyên. Vô hại và đúng vai trò làm ngày khác nhau. Có thể tăng mức giảm lên 30% nếu muốn người chơi chuyển sang H2 mạnh hơn, nhưng H2 vốn đã là lựa chọn chủ đạo |
| Biến cố nói chung | Giữ nguyên. Trung bình trung tính, không cần giới hạn thêm |
| Rương Đen | Cần quyết định: nâng lời kỳ vọng đủ để có người mua, hoặc xem là nội dung phụ (hiện vật riêng của nó), hoặc bỏ khỏi cân bằng kinh tế |
| Cân bằng hộp | Làm cho lời tuyệt đối tăng theo giá hộp (mục 3) để chọn hộp phụ thuộc vào số tiền đang có. Chưa đo, cần Claude 1 và chủ dự án quyết định hướng trước khi Claude 2 chạy lại |

## 5. Giả định và giới hạn

- Biến cố mô phỏng như luật của Claude 1. E6 coi "Rất hiếm" là V07 và V10 (giả định).
- Người chơi thích nghi tính biến cố vào giá trị kỳ vọng khi chọn hộp (E1, E2, E6 đổi giá bán, E3 đổi giá hộp). E4, E5 chặn giám định và phục chế, người chơi chỉ không làm được các việc này.
- Chưa đo giữ hàng chờ biến cố giá cao (bot bán ngay), nên giá trị "chờ E1 để bán nhóm Quá khứ" chưa có. Đó có thể là nơi biến cố tạo quyết định thật hơn.
- Bot chọn hộp theo lời kỳ vọng tuyệt đối. Người chơi thật có thể chọn khác, ví dụ theo tâm lý tránh rủi ro.

## 6. Phát hiện

| Mã | Mức | Mô tả | Đề xuất | Chủ xử lý |
| --- | --- | --- | --- | --- |
| D-29 | Trung bình | H2 chi phối việc chọn hộp (66% đến 71% lượt mua), nên không có đánh đổi hộp rẻ hay đắt | Tăng lời tuyệt đối theo giá hộp (mục 3) | Claude 1, chủ dự án |
| D-30 | Trung bình | Rương Đen không bao giờ được chọn (0%), lời kỳ vọng gần 0 | Nâng lời, đổi vai trò hoặc bỏ khỏi cân bằng | Claude 1, chủ dự án |
| O-05 | Thấp | E3 chỉ dịch 7 đến 9 điểm phần trăm sang H2, không tạo quyết định mới | Giữ nguyên, có thể tăng mức giảm | Claude 1 |

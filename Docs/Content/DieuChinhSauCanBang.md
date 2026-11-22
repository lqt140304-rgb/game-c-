# Đề xuất điều chỉnh sau cân bằng — CHỢ ĐỒ LẠ

Claude 1 tổng hợp báo cáo của Claude 2 ([Sprint 2](../Balance/Sprint2.md), [Sprint 3](../Balance/Sprint3.md), [Sprint 4](../Balance/Sprint4.md), [DoKho](../Balance/DoKho.md), [DoLaiDeXuat](../Balance/DoLaiDeXuat.md)) và chốt cách sửa nội dung.

**Mục tiêu của chủ dự án:** không để người chơi thắng quá dễ. Game phải tạo nhiều tình huống khó, có đánh đổi và buộc tính toán kỹ.

**Quyết định của chủ dự án (2026-11-22):**

| Việc | Quyết định |
| --- | --- |
| Giám định | Chọn cách A: hàng thật đã giám định bán thêm **25%**, phí giám định **15%**. Không dùng cách phạt món chưa giám định. |
| Giá nền | Dùng bảng giá của gói, nhân thêm 1,10 vì đã duyệt phạt rủi ro (mục A, G). |
| Phạt rủi ro | **Duyệt** (mục G): hàng giả 12% / 15% / 22% / 30% theo hộp. |
| Gói tạo tác, combo, gia truyền | **Duyệt cả gói** (mục C). |
| Luật nợ | Giữ luật hiện tại: **trả đủ kỳ trong ngày, hoặc trả một phần và phần còn thiếu bị phạt 20% chuyển sang ngày sau** (mục D). |
| Kho, bàn mở, lưu ván | **Duyệt** (mục E, F). |
| Luật dịch vụ Sprint 4 | **Duyệt** ([DichVuVaMeta.md](DichVuVaMeta.md)). |
| Độ hiếm "Rất hiếm" | **V07 và V10.** |

Các file nội dung gốc đã được đồng bộ theo quyết định này: [NoiDungMau.md](NoiDungMau.md), [GiaTruyenTaoTacBiKhe.md](GiaTruyenTaoTacBiKhe.md), [DichVuVaMeta.md](DichVuVaMeta.md) và [GDD](../GameDesign/README.md). Tài liệu này giữ lại để ghi lý do và số liệu.

Mục tiêu độ khó đo bằng ba chiến thuật máy (không đọc manh mối, không canh biến cố), nên là **cận dưới**. Người chơi biết tính phải thắng cao hơn.

## Tóm tắt gói

| Hạng mục | Nội dung gốc | **Đề xuất cuối** | Nguồn đo |
| --- | --- | --- | --- |
| Giá nền | Gốc (thắng 0%) | Bảng hệ số 0,85 **nhân 1,10** (mục A, G) | DoLaiDeXuat, risk_penalty |
| Phí giám định | Không có | **15%** giá nền | DoLaiDeXuat |
| Chứng nhận | Không có | **+25%** giá bán hàng thật đã giám định | DoLaiDeXuat |
| CP1 | +5% hiện vật Quá khứ | **+5% mọi hiện vật** | DoLaiDeXuat |
| CB1 | +10% hiện vật Quá khứ | **+10% mọi hiện vật bán trong ngày** | DoLaiDeXuat |
| CP3 và TC3 | Giá 900 và 1.000 | **Giá 500 và 500** | DoLaiDeXuat |
| TC3 | Miễn phạt nợ ≤ 300 | **+10% giá bán nhóm Hiện đại, Tương lai, Ngoài hành tinh** | DoLaiDeXuat |
| CB3 | Giám định miễn phí 1 lần mỗi ván | **Giám định miễn phí không giới hạn, thêm +15% giá bán mọi hiện vật** | DoLaiDeXuat |
| GT1 | Phạt nợ 10% | **Hoãn nợ một lần (mục C)** | DoLaiDeXuat |
| Đấu giá | k kỳ vọng 1,16 | Giữ **k 1,16**, món từ 1.300 xu | DoLaiDeXuat |
| Phạt rủi ro (O-01) | Không có | **Hàng giả theo hộp 12/15/22/30% (mục G)** | Claude 1 đo, Claude 2 xác nhận (DoLaiDeXuat mục 10) |
| Kho, lưu ván | [MỞ] | Kho 8 ô, bàn mở 3 hộp, chỉ lưu tự động | Chưa đo được (bot bán ngay) |

## A. Giá nền (D-09)

Giá nền theo hệ số 0,85 của bảng Claude 2 gốc. Với gói trên, bot an toàn thắng 15%, bot liều 17%, bot đầu tư tạo tác 26–32%.

| Hiện vật | Giá nền hiện tại | **Giá đề xuất (0,85)** |
| --- | --- | --- |
| V01 Bình sứ men xanh | 300 | **210** |
| V02 Sách cổ bìa da | 500 | **350** |
| V03 Đồng hồ quả lắc cũ | 650 | **450** |
| V04 Mảnh kính đồng hồ thiên văn | 200 | **375** |
| V05 Thiết bị liên lạc hỏng | 150 | **280** |
| V06 Bộ pin lượng tử | 900 | **1.675** |
| V07 Tinh thể lam cầm tay | 1.200 | **2.450** |
| V08 Gương thủy tinh cổ tích | 1.000 | **1.445** |
| V09 Lá bùa giấy vàng | 400 | **815** |
| V10 Mảnh vảy long | 1.800 | **2.600** |

Bảng này khác [DoKho.md](../Balance/DoKho.md) vài chục xu ở một số món. Bản mới nhất của Claude 2 ([DoLaiDeXuat.md](../Balance/DoLaiDeXuat.md)) dùng bảng 0,85 nên tôi theo bảng này. **Vì đã duyệt phạt rủi ro (mục G), bảng áp dụng thực tế là bảng nhân 1,10**, ghi trong [NoiDungMau.md](NoiDungMau.md) (V01 230, V02 385, V03 495, V04 415, V05 310, V06 1.845, V07 2.695, V08 1.590, V09 895, V10 2.860).

Hệ số chất lượng (Nguyên 1,0, Xước 0,7, Hỏng 0,3, tỷ lệ 50/35/15) và hàng giả (15%, riêng V09 là 35%, bán còn 20%) lấy theo giả định của Claude 2.

## B. Giám định và phục chế (D-11)

Giá bán ban đầu không phụ thuộc giám định nên giám định vô nghĩa. Theo gói đã đo:

- **Phí giám định:** 15% giá nền, tối thiểu 20 xu.
- **Phí phục chế:** 25% giá nền, nâng chất lượng một bậc.
- **Chứng nhận:** hiện vật **thật** đã giám định được cộng **25%** giá bán, cộng theo phép cộng cùng các hiệu ứng giá bán khác. Hàng giả vẫn bán còn 20%.

Claude 2 đo rằng phí 10% làm giám định lời gần như chắc chắn, đẩy bot lên 30–42% mà chưa cần đọc manh mối, nên không còn là quyết định. **Phí 15% là điểm ngọt:** lời với món chất lượng cao, hòa vốn với món Xước, lỗ với món Hỏng. Ví dụ V09 Lá bùa (815 xu, 35% là hàng giả): phí 122 xu, nếu là hàng thật nguyên vẹn thì giá bán tăng 204 xu, nếu giả thì mất phí vô ích. Người chơi phải đọc chất lượng và manh mối để chọn món nào đáng giám định.

## C. Tạo tác, combo và gia truyền (D-10, D-12)

Theo Claude 2, CP1 +10% và CB1 +20% cùng với chứng nhận làm combo nhỏ thắng 40% và đầu tư thành bắt buộc. Tier 3 (CP3, TC3, CB3) không kịp hoàn vốn trong 7 ngày: bot chỉ mua được vào ngày 4–5, còn 2–3 ngày để thu hồi 1.900 xu.

| Mã | Hiện tại | **Đề xuất cuối** |
| --- | --- | --- |
| CP1 | +5% hiện vật Quá khứ | **+5% giá bán mọi hiện vật** |
| CB1 | +10% hiện vật Quá khứ bán trong ngày | **+10% mọi hiện vật bán trong ngày** |
| CP3 | Giá 900, +10% Thần thoại | **Giá 500**, +10% Thần thoại |
| TC3 | Giá 1.000, miễn phạt khi nợ tồn ≤ 300 | **Giá 500**, +10% giá bán hiện vật nhóm Hiện đại, Tương lai và Ngoài hành tinh |
| CB3 | Giám định miễn phí 1 lần mỗi ván | **Giám định miễn phí không giới hạn, thêm +15% giá bán mọi hiện vật** |
| GT1 | Phạt thu nợ giảm xuống 10% | **Hoãn nợ một lần (bên dưới)** |

Kết quả Claude 2: combo nhỏ và combo lớn ngang nhau (khoảng 32%), nên chọn hướng nào cũng khả thi. Combo nhỏ rẻ và chắc. Combo lớn tốn thêm nhưng có CB3 và mở BK1 sớm.

**GT1 mới: Sổ Ghi Nợ Cũ.** Một lần mỗi ván, hoãn **kỳ nợ của ngày đó** sang ngày sau. Ba điểm chốt (khớp với giả định mà Claude 2 đã đo, nên không cần đo lại):
1. Kỳ hoãn thành khoản **kỳ × 1,15** (làm tròn lên), trả cùng kỳ của ngày kế tiếp.
2. Kỳ hoãn **không bị phạt 20%** và **không tính vào ngưỡng thua 1.000 xu**.
3. Chỉ hoãn **kỳ của ngày đó**, không hoãn nợ tồn. Không hoãn được kỳ ngày 7.

Claude 2 đo: GT1 chỉ đổi tỷ lệ thắng +0,1 đến +1,8 điểm, và mạnh nhất khi dùng **sớm để có vốn đầu tư** (ngày 2, thêm khoảng 3,8 điểm cho chiến thuật săn combo). Hoãn khi đã thiếu tiền không cứu được ván, vì khoản hoãn quay lại cộng 15%. Đây là đánh đổi nhẹ đúng ý: trả thêm 15% để có vốn sớm.

Hiệu ứng thông tin (CB2, GT2, TC2, Mốc 3 Cổ Phong) giữ nguyên vì không đo được bằng xu, cần thử người chơi.

## D. Luật nợ (D-05, D-07, D-08)

**D-05, vòng xoáy thiếu tiền: giữ luật hiện tại.** Đổi ngưỡng thua (1.000, 1.500, 2.000) hoặc mức phạt (0%, 10%, 20%) cho kết quả giống hệt nhau. Chừa lại tiền khi thu nợ chỉ cho 0–5% ván gỡ lại được (đo bằng [QA/d05_recovery.py](../../QA/d05_recovery.py)). Theo mục tiêu độ khó, thiếu tiền nên là sai lầm rất đắt, nên không nới. Phạt 20% và ngưỡng 1.000 xu giữ nguyên.

**D-07, phạt tối thiểu:** chấp nhận, giữ phạt làm tròn lên.

**D-08, trả nợ tồn sớm: bỏ khỏi đề xuất.** Tôi đã đề xuất cho phép trả nợ tồn sớm như một đánh đổi, nhưng Claude 2 chỉ ra đúng là nó không đổi gì về kinh tế: cuối ngày Anh Bảy thu toàn bộ tiền mặt cho kỳ kể cả nợ tồn, nên trả sớm hay cuối ngày mất cùng số tiền. Trả sớm chỉ làm mất tiền mua hộp trong ngày. Bỏ đề xuất này.

**Ca kiểm thử bổ sung cho GT1:**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 8 | Dùng GT1 hoãn kỳ ngày 4 (750 xu). | Ngày 5 phải trả 1.050 cộng 863 (750 cộng 15%, làm tròn lên), cộng nợ tồn nếu có. Không bị phạt 20% cho phần hoãn. |
| 9 | Dùng GT1 vào kỳ ngày 7. | Từ chối. |
| 10 | Dùng GT1 lần thứ hai trong cùng ván. | Từ chối. |
| 11 | Ngày 2 thử trả trước kỳ ngày 3. | Từ chối, theo luật đã nghiệm thu. |

## E. Kho và bàn mở (D-02, BT-14)

Kho là nguồn đánh đổi tự nhiên: giữ hàng chờ giá hay bán ngay để có chỗ. Claude 2 chưa đo được vì bot bán ngay, nên cần thử người chơi.

- **Bàn mở:** chứa tối đa 3 hộp chưa mở. Hộp chưa mở không chiếm kho và giữ được qua ngày. Bàn đầy thì không mua thêm hộp.
- **Kho:** 8 ô cho hiện vật đã mở. Tạo tác không chiếm kho.
- **Kho đầy:** không mở được hộp mới cho đến khi bán, cầm hoặc đưa đấu giá bớt. Hộp vẫn ở lại bàn, không mất.
- Hiện vật đang cầm hoặc đang đấu giá không chiếm kho vì đang ở nơi khác. Mỗi món chỉ ở đúng một nơi.
- Hết ván, hiện vật còn lại không tính vào kết quả.

## F. Lưu ván (D-06)

Lưu ván **chỉ tự động**, một ô duy nhất, không có lưu tay và không tải lại bản cũ trong cùng ván.

- Lưu vào đầu mỗi ngày và sau mỗi giao dịch tiền hoàn tất.
- Thoát giữa thao tác thì khôi phục về trạng thái ngay sau giao dịch hoàn tất gần nhất.
- Kết quả ngẫu nhiên xác định theo (seed, ngày, lượt) hoặc theo mã hiện vật, theo yêu cầu của Claude 2 cho Codex.

## G. Phạt rủi ro (O-01), đã duyệt

**Vấn đề Claude 2 nêu:** chiến thuật liều có giám định thắng cao hơn thận trọng có giám định khoảng 8 điểm. Rủi ro chưa bị phạt, nên dồn tiền vào hộp đắt luôn tốt hơn.

**Đề xuất: hàng giả nhiều hơn ở hộp đắt.** Tỷ lệ hàng giả theo hộp: **H1 12%, H2 15%, H3 22%, Rương Đen 30%** (riêng V09 giữ 35%). Người chơi biết mình mở hộp nào và thấy tỷ lệ giả khi xem hộp nên tự cân: hộp đắt lời nhiều hơn nhưng cũng dễ trúng hàng giả.

Chỉ đổi tỷ lệ giả thì game khó đi nhiều (bot thắng 4–6%), nên cần **bù giá nền ×1,10**. Đo bằng [QA/risk_penalty.py](../../QA/risk_penalty.py), dùng nguyên mô hình `proposal_sim.py` của Claude 2 với gói ở mục "Tóm tắt". 4.000 seed. Giá nền đã làm tròn.

| Thắng% | Gói Claude 2 (giả 15% mọi hộp) | **Phạt rủi ro + giá ×1,10** |
| --- | --- | --- |
| Thận trọng | 15,2 | 16,7 |
| Thận trọng + giám định chọn lọc | 24,4 | 22,9 |
| Liều | 17,4 | 15,6 |
| Liều + giám định chọn lọc | 32,4 | **24,8** |
| Combo nhỏ | 26,2 | 30,9 |
| Combo nhỏ + giám định | 32,1 | 35,7 |
| Combo lớn | 26,9 | 31,9 |
| Combo lớn + giám định | 32,4 | 38,5 |
| **Chênh liều và thận trọng, cùng giám định** | **+8,0** | **+1,9** |

Giá trị kỳ vọng mỗi hộp: H1 2,50 / H2 1,59 / H3 1,24 / Rương Đen 1,01 lần giá hộp. Rương Đen gần như hòa vốn: chỉ đáng mua nếu muốn đổi lấy hiện vật hiếm nhất.

Giá nền kèm theo (×1,10, làm tròn): V01 230, V02 385, V03 495, V04 415, V05 310, V06 1.845, V07 2.695, V08 1.590, V09 895, V10 2.860.

**Kết quả Claude 2 đo lại bằng đủ chín chiến thuật** ([DoLaiDeXuat.md](../Balance/DoLaiDeXuat.md) mục 10): số liệu khớp với đo của tôi, khuyến nghị duyệt và nâng trần test lên 41. Tác dụng phụ là hướng đầu tư lên 31–38%, vẫn trong mức "đáng tiền nhưng không bắt buộc" nhưng sát giới hạn. Phương án nhẹ hơn (giả 12/15/20/25%, giá ×1,08) giữ làm dự phòng nếu bản chơi thật cho thấy đầu tư thành bắt buộc, khi đó liều có giám định hơn thận trọng có giám định khoảng 5,5 điểm.

Claude 2 cũng khuyên **cho người chơi thấy tỷ lệ hàng giả của từng hộp**, đã ghi vào luật ở [NoiDungMau.md](NoiDungMau.md).

Chưa có chiến thuật máy nào đọc manh mối, nên mức độ người chơi chọn hộp theo tỷ lệ giả chưa đo được.

## H. Việc còn lại

1. **Claude 2:** khóa test phạt rủi ro (bật `RISK_PENALTY_APPROVED` trong `QA/test_risk_penalty.py` và nâng trần lên 41), và đo lại sau khi có bản chơi thật bằng người thử.
2. **Codex và Antigravity:** nối kho 8 ô, bàn mở 3 hộp, hiển thị tỷ lệ hàng giả của hộp, lưu tự động theo luật ở mục E, F.
3. **Claude 1:** chơi thử một ván thật khi có bản chạy (Sprint 3).

## I. Kết quả duyệt

| Mục | Quyết định |
| --- | --- |
| A. Giá nền | Duyệt bảng nhân 1,10. |
| B. Giám định phí 15%, chứng nhận +25% | Duyệt. |
| C. CP1, CB1, tier 3, TC3, CB3, GT1 | Duyệt cả gói. |
| D. Luật nợ khắt khe, bỏ trả nợ tồn sớm | Duyệt. |
| E, F. Kho 8 ô, bàn mở 3 hộp, chỉ lưu tự động | Duyệt. |
| G. Phạt rủi ro theo hộp | Duyệt. |

## J. Manh mối và kệ hộp (D-26, D-27, D-28), đã duyệt

**Vấn đề Claude 2 nêu** ([KyNangManhMoi.md](../Balance/KyNangManhMoi.md)): nội dung chưa nói manh mối của một hộp cụ thể có liên quan hiện vật bên trong không, và số hộp người chơi xem được (M) chưa có trong GDD. Claude 2 phải giả định, và toàn bộ kết quả phụ thuộc vào giả định đó. Đáng lo hơn: bot đọc manh mối tối ưu thắng **57–62%**, vượt xa mục tiêu "không thắng dễ".

**Quyết định của Claude 1:**
1. **Manh mối gắn với hiện vật bên trong.** Mỗi hộp có 3 manh mối gợi nhóm nguồn gốc của hiện vật. H1 và H2 có 2 đúng 1 sai, H3 và Rương Đen chỉ 1 đúng 2 sai. Đây là giả định Claude 2 đã dùng, nay thành luật, riêng hộp đắt khó đọc hơn.
2. **Kệ hộp:** mỗi lượt mua (tối đa 3 lượt mỗi ngày), Dì Mận bày **1 hộp mỗi loại** (M = 1). Chọn 1 hộp hoặc dừng mua, không đổi hộp trong lượt.
3. **Giá niêm yết đi theo manh mối đầu tiên** với λ = 0,75. Cách này Claude 2 đã chỉ ra là đòn bẩy hiệu quả nhất.
4. **Lời mách của Dì Mận, 15 xu:** trả tiền để biết chắc nhóm nguồn gốc thật của hiện vật trong hộp, một lần mỗi hộp (chốt sau khi Claude 2 đo hai hướng, xem bên dưới). Bản đầu của tôi là mua thêm manh mối có thể sai, đã bỏ.

**Số liệu** ([QA/clue_rules.py](../../QA/clue_rules.py), mô hình `clue_sim.py` của Claude 2, bot đọc tối ưu và kén hộp τ = 1,3, 1.500 seed, cấu hình đã duyệt). Thắng% của thận trọng + giám định / liều + giám định / combo lớn + giám định:

| Cấu hình | Thắng% |
| --- | --- |
| Không đọc manh mối (bot thường) | 22,0 / 23,2 / 37,9 |
| Giá cố định, M = 3, 2/3 đúng (trước quyết định) | 57 / 62 / 59–60 (Claude 2) |
| M = 2, λ 0,75, mọi hộp 2/3 đúng | 42,1 / 48,7 / 49,8 |
| M = 2, λ 0,75, H3 và Rương Đen 1/3 đúng | 42,3 / 49,1 / 50,7 |
| M = 2, λ 1,0, H3 và Rương Đen 1/3 đúng | 34,8 / 39,7 / 45,5 |
| **M = 1, λ 0,75, H3 và Rương Đen 1/3 đúng (chọn)** | **33,9 / 38,3 / 43,5** |
| M = 2, λ 0,75, H3 và Rương Đen 1/3 đúng, người đọc cẩn thận hơn (τ = 1,15) | 29,9 / 36,7 / 45,0 |

Kết quả: người đọc tối ưu hơn bot thường 12 / 15 / 6 điểm, nằm trong khoảng "biết tính thì thắng hơn nhưng không áp đảo". Người chơi thật đọc kém hơn bot Bayes nên thấp hơn nữa.

**Giá mua thêm manh mối có thể sai** (đo sơ bộ của tôi, **đã bị thay** bằng đo của Claude 2 ở dưới; M = 1, λ 0,75):

| Giá mỗi manh mối | Thắng% | So với không mua thêm (33,9 / 38,3 / 43,5) |
| --- | --- | --- |
| 10 xu | 40,8 / 44,7 / 49,4 | +6,9 / +6,4 / +5,9 |
| **15 xu (chọn)** | 37,1 / 40,5 / 48,4 | +3,2 / +2,2 / +4,9 |
| 20 xu | 33,9 / 35,9 / 44,1 | 0 / −2,4 / +0,6 |
| 30 xu | 27,9 / 28,1 / 39,1 | −6 / −10 / −4 |

Phép đo sơ bộ này cho rằng 15 xu hơi có lợi, nhưng Claude 2 đo lại bằng mô hình sát luật hơn thì **không đúng** (xem phần D-28 bên dưới).

**Điều cần biết:**
- **Manh mối thực chất chỉ có ích rõ ở hộp H2.** H1 chỉ ra hiện vật cùng nhóm Quá khứ nên không phân biệt được món nào. H3 và Rương Đen chỉ 1 trên 3 manh mối đúng nên gần như không đổi giá. Vì vậy bảng giá niêm yết ([NoiDungMau.md](NoiDungMau.md) mục 5.3) gần như chỉ khác ở H2 (260 hoặc 395 xu thay vì 350).
- Nếu sau này muốn manh mối có ích ở nhiều hộp hơn, có thể thêm manh mối về **độ hiếm** (ở H3, độ hiếm phân biệt đúng V08, V09 và V10). Chưa làm vì chưa đo.
- Mô hình rút hộp mới cho mỗi lượt giống đúng luật kệ hộp. Phần khác biệt nhỏ: giá niêm yết lấy theo manh mối miễn phí **đầu tiên**, còn mô hình tính theo mọi manh mối miễn phí (CB2 và Mốc 3 Cổ Phong). Claude 2 cần đo lại.

**Quyết định (2026-11-22):** chủ dự án **duyệt mục J**. Claude 2 đã đo lại đúng luật kệ hộp ([KyNangManhMoi.md](../Balance/KyNangManhMoi.md)).

**Kết quả đo của Claude 2 (ưu tiên hơn số liệu sơ bộ ở trên):**
- Người chơi tính toán hoàn hảo thắng **38–46%** ở luật chốt (cận trên), chấp nhận được.
- **Đọc manh mối chỉ cộng +2 đến +4 điểm** ở λ = 0,75. Phần lớn kỹ năng đến từ **chọn loại hộp mỗi lượt** (+5 đến +17 điểm), do luật "mỗi lượt bày 1 hộp mỗi loại". Con số "34 / 38 / 44" ở trên gộp cả hai tác động.
- Không cần đổi λ: từ 0,75 lên 1,0 chỉ giảm thêm khoảng 1,5 đến 2 điểm.

**Điều chỉnh lại nhận định của tôi về giá mua thêm manh mối:** tôi từng viết 15 xu "hơi có lợi", nhưng Claude 2 đo bằng mô hình sát luật hơn thì mua thêm manh mối 15 xu **lỗ 0,6 đến 1,5 điểm**. Lợi tối đa của việc xem đủ 3 manh mối chỉ +1,4 đến +1,8 điểm. Vậy ở 15 xu mua thêm manh mối không phải một quyết định thật.

**D-28 đã chốt: hướng B, lời mách chắc đúng 15 xu.** Claude 2 đo hai hướng ([KyNangManhMoi.md](../Balance/KyNangManhMoi.md), luật chốt, λ = 0,75, 2.000 seed). Thắng% của thận trọng / liều / combo nhỏ / combo lớn, kèm số lần bot mua mỗi ván:

| Phương án | Thắng% |
| --- | --- |
| Chỉ manh mối miễn phí | 38,5 / 45,2 / 45,8 / 46,4 |
| A. thêm manh mối **có thể sai**, 10 xu | 37,5 / 44,4 / 45,6 / 46,4 |
| A. thêm manh mối có thể sai, 5 xu | 37,9 / 44,9 / 45,9 / 46,8 |
| B. **chắc đúng**, 30 xu | 39,0 / 47,1 / 47,6 / 48,2 |
| **B. chắc đúng, 15 xu (chọn)** | **40,8 / 47,9 / 48,5 / 48,9** |
| B. chắc đúng, 10 xu | 39,5 / 47,8 / 48,8 / 49,4 |

- **Hướng A không đáng ở mọi mức giá** (hòa vốn dù chỉ 5 xu), vì manh mối thêm vẫn có thể sai nên ít thông tin.
- **Hướng B có lợi và là quyết định thật:** ở 15 đến 30 xu bot chỉ mua khoảng 3 đến 4 lần mỗi ván, đúng lúc hộp nằm sát ngưỡng và thông tin sẽ đổi quyết định. Ở 10 xu trở xuống bot mua tới 10 đến 12 lần, gần như mua cho có.
- Ở 15 xu, lợi cho người chơi khoảng +2,3 đến +2,7 điểm. Trần thắng vẫn quanh 49%, gần như không tăng so với 46% khi chỉ dùng manh mối miễn phí.

Vì vậy luật chốt là: **trả 15 xu hỏi lời mách của Dì Mận để biết chắc nhóm nguồn gốc thật, một lần mỗi hộp**, giá niêm yết không đổi ([NoiDungMau.md](NoiDungMau.md) mục 5.2). `content.json` ghi `extra_clue_kind = certain_origin` và `extra_clue_per_box = 1`.

**O-04 đã chốt (2026-11-22): để người chơi tự tính.** Giao diện không hiển thị giá trị kỳ vọng của từng loại hộp, chỉ hiển thị giá niêm yết, tỷ lệ hàng giả của loại hộp và manh mối. Theo đo của Claude 2, phần lớn kỹ năng của người chơi giỏi nằm ở việc chọn loại hộp (+5 đến +17 điểm), nên quyết định này giữ đúng mục tiêu "cần tính toán kỹ". `content.json` ghi `shelf.show_expected_value = false`.

**Câu hỏi thiết kế còn lại:** để tự tính được, người chơi cần biết hộp nào có thể ra những món nào. Hiện chỉ có nhóm nguồn gốc tiềm năng của hộp. Cần quyết định sổ tay có ghi các hiện vật đã gặp theo từng loại hộp (và giữ qua các ván nhờ Sổ Sưu Tầm) hay không, để người chơi tích lũy hiểu biết thay vì đoán.

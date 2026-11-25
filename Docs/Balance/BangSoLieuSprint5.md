# Bảng số liệu cân bằng Sprint 5 (Claude 2)

Viết ngày 2026-11-25. **Một bảng để Codex nạp vào game**, gom từ [Sprint5Ruong](Sprint5Ruong.md), [Sprint5RuongMoKhoa](Sprint5RuongMoKhoa.md), [Sprint5SoiLoi](Sprint5SoiLoi.md), [Sprint5CayPhatTrien](Sprint5CayPhatTrien.md) và [DaChot](../DaChot.md). Bản máy đọc: [balance_sprint5.json](../Content/data/balance_sprint5.json) (cùng số liệu, tiền bằng đồng nguyên; **không thay** `content.json` của Claude 1). Test khóa JSON khớp mô phỏng: `QA/test_balance_data.py` (chạy dưới 1 giây). Không chạy lại mô phỏng nặng: mọi số là số đã đo.

> **Tiền tệ:** mọi số tiền ghi bằng đồng Việt Nam. Mô phỏng tính theo đơn vị 10.000 đ; JSON đã nhân sẵn. Mọi giá là bội số của 10.000 đ.

**Cột trạng thái:**

- **ĐÃ CHỐT** — chủ dự án duyệt, dẫn nguồn trong [DaChot](../DaChot.md) hoặc spec.
- **ĐỀ XUẤT** — Claude 2 đề xuất, đã đo, chưa duyệt.
- **GIẢ ĐỊNH** — giả định của mô hình, chưa kiểm tra bằng người thật hoặc chưa có dữ liệu chốt.

**Chất lượng chung (đã đo trên bảng này):** bot thường 15% đến 20%, người chơi tính toán hoàn hảo dưới 47%, ngày 1 không thua. Chi tiết số đo ở các báo cáo nguồn; mục 9 liệt kê điểm lệch giữa các báo cáo.

## 1. Luật nền (tiền, ván, shop)

| Số | Giá trị | Trạng thái | Báo cáo nguồn |
|---|---|---|---|
| Vốn đầu ván | 10.000.000 đ | ĐÃ CHỐT (DaChot: Tiền tệ) | tien-te |
| Nợ 7 kỳ | 2.000.000; 3.500.000; 5.000.000; 7.500.000; 10.500.000; 15.000.000; 22.000.000 đ | ĐÃ CHỐT (DaChot: Tiền tệ) | tien-te; content.json |
| Ngưỡng thua (nợ tồn) | trên 10.000.000 đ | ĐÃ CHỐT | content.json; Sprint5Ruong mục 3 |
| Phạt nợ trễ | 20% | ĐÃ CHỐT | content.json; Sprint5Ruong mục 2 |
| Bước làm tròn nhỏ nhất | 10.000 đ | ĐÃ CHỐT (DaChot: Tiền tệ) | tien-te |
| Giá hộp dao động trong ngày | ±20% theo từng loại rương | ĐÃ CHỐT | GiaHopDaoDong.md |
| Tối đa hộp mua mỗi ngày | 3 | ĐÃ CHỐT | README; content.json |
| Shop mỗi ngày | 4 rương cơ bản + tối đa 4 rương mở khóa hoặc chủ đề, xoay vòng theo hạt giống ván và ngày | ĐÃ CHỐT (DaChot: Rương chủ đề, 2026-11-25) | Sprint5RuongMoKhoa mục 3a |
| Rương Đen bán từ ngày | 4 | ĐÃ CHỐT (lõi mô phỏng đã duyệt) | Sprint5Ruong mục 2 |
| Số ô tạo tác | 4; BK1 mở ô thứ 5 | ĐÃ CHỐT (ruong-suu-tap; TASKS) | Sprint5Ruong mục 4 |
| Tạo tác trùng hoặc hết ô | đổi **50% giá Kho** (chủ yếu từ Kim Khí) | ĐÃ CHỐT (DaChot: Giá và tỉ lệ 4 rương cơ bản) | Sprint5Ruong mục 1 |
| Rác: số món, giá bán | 9 món dùng chung, 30.000 đ mỗi món, không giả, không giám định | ĐỀ XUẤT | Sprint5Ruong mục 1 |
| Bậc đồ | R Rác, T Thường, H Hiếm, X Rất hiếm, L Huyền thoại | ĐÃ CHỐT (DaChot: Bậc đồ trong rương) | tier-do-trong-ruong |
| Luật quay | hai bước: quay bậc, rồi quay đều một món trong bộ sưu tập của rương ở bậc đó; Rác dùng chung | ĐÃ CHỐT (DaChot: Rương chủ đề) | ruong-chu-de |

## 2. Bốn rương cơ bản

Tỉ lệ ra bậc (R, T, H, X, L), hàng giả và số manh mối đúng trên 3. Trạng thái cả bảng: **ĐÃ CHỐT** (chủ dự án duyệt 2026-11-24; ghi ở DaChot và spec `tier-do-trong-ruong` mục 3). Huyền thoại có ở cả 4 rương, hiếm (ĐÃ CHỐT).

| Rương | Giá | R | T | H | X | L | Tạo tác | Hàng giả | Manh mối đúng | Trạng thái | Báo cáo nguồn |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Gỗ | 1.400.000 đ | 18% | 79,5% | 1% | 1% | 0,5% | — | 10% | 2 trên 3 | ĐÃ CHỐT | Sprint5Ruong.md mục 1; DaChot.md |
| Vải Niêm | 3.500.000 đ | 30% | 39% | 25% | 5% | 1% | — | 15% | 2 trên 3 | ĐÃ CHỐT | Sprint5Ruong.md mục 1; DaChot.md |
| Kim Khí | 4.500.000 đ | 10% | 25% | 22% | 8% | 1% | 34% (bậc 1 60%, bậc 2 35%, bậc 3 5%; hệ CP+TC) | 0% | 1 trên 3 | ĐÃ CHỐT | Sprint5Ruong.md mục 1; DaChot.md |
| Đen | 12.000.000 đ | 30% | 3% | 4% | 58% | 5% | — | 30% | 1 trên 3 | ĐÃ CHỐT | Sprint5Ruong.md mục 1; DaChot.md |

Thu và lời kỳ vọng mỗi hộp (chưa tính dao động giá), báo cáo Sprint5Ruong mục 1: Gỗ thu 3.210.000 đ, lời 1.810.000 đ (2,29 lần giá); Vải Niêm 5.300.000 đ, lời 1.800.000 đ (1,51); Kim Khí 6.870.000 đ, lời 2.370.000 đ (1,53); Đen 12.980.000 đ, lời 980.000 đ (1,08). Trạng thái: ĐÃ CHỐT (hệ quả của bảng đã duyệt).

### Mã món mỗi rương (mã mô phỏng, chưa phải mã chính thức)

Mã `V*`, `N*`, `L04` là mã của mô phỏng (`chest_sim.NEW_ITEMS`). Mã chính thức chưa chốt (TASKS: mã tạm chưa khớp `content.json`). Bộ sưu tập theo bậc của mỗi rương: **GIẢ ĐỊNH** (do Claude 2 xếp để đo, chờ Claude 1 chốt nhóm nguồn gốc và bộ sưu tập). Kim Khí còn ra tạo tác (tạo tác tính theo giá Kho Tạo Tác, `content.json`).

| Rương | Bậc | Mã món (mã mô phỏng) | Tên và giá nền |
|---|---|---|---|
| Gỗ | Thường | V01, V03, V09 | Bình sứ men xanh 2.300.000 đ; Đồng hồ quả lắc cũ 4.950.000 đ; Lá bùa giấy vàng 8.950.000 đ |
| Gỗ | Hiếm | N01, N02 | Ấm trà đất nứt 15.500.000 đ; Hộp nữ trang gỗ khảm 17.500.000 đ |
| Gỗ | Rất hiếm | N03 | Tượng Di Lặc gỗ 27.000.000 đ |
| Gỗ | Huyền thoại | L04 | Vé số độc đắc 40.000.000 đ |
| Vải Niêm | Thường | V02, V04 | Sách cổ bìa da 3.850.000 đ; Mảnh kính thiên văn 4.150.000 đ |
| Vải Niêm | Hiếm | N04, N05 | Tranh cuộn thủy mặc 17.000.000 đ; Con dấu đồng 16.000.000 đ |
| Vải Niêm | Rất hiếm | V10 | Mảnh vảy long 28.600.000 đ |
| Vải Niêm | Huyền thoại | N13 | Bản đồ cổ vẽ tay 50.000.000 đ |
| Kim Khí | Thường | V05, N07 | Thiết bị liên lạc hỏng 3.100.000 đ; Radio bóng đèn 3.500.000 đ |
| Kim Khí | Hiếm | V06, N06 | Bộ pin lượng tử 18.450.000 đ; Máy ảnh phim cổ 19.000.000 đ |
| Kim Khí | Rất hiếm | V07 | Tinh thể lam 26.950.000 đ |
| Kim Khí | Huyền thoại | N14 | Đồng hồ bỏ túi không hết pin 50.000.000 đ |
| Đen | Thường | N08, N09 | Xúc xắc chì 2.600.000 đ; Bộ bài cũ 2.200.000 đ |
| Đen | Hiếm | N10, N11 | Bật lửa mạ vàng 16.500.000 đ; Đồng hồ xách tay 15.000.000 đ |
| Đen | Rất hiếm | N12 | Nhẫn kim cương không rõ nguồn 30.000.000 đ |
| Đen | Huyền thoại | N15 | Két sắt chưa ai mở được 70.000.000 đ |

## 3. Giá nền món (bộ sưu tập 4 rương cơ bản)

Món đã có giữ giá đang dùng của lõi mô phỏng (mã `V*`); món mới (mã `N*`, `L04`) là giá nền **ĐỀ XUẤT** của Claude 2. **Đây là điểm nhạy nhất của cả hệ:** giá món mới ±15% đưa bot thường từ 10% lên 24% (Sprint5Ruong mục 4). Khi Claude 1 hoặc chủ dự án chốt giá khác (trung bình theo bậc lệch quá ±5%), phải chạy lại `QA/test_chest_balance.py`. Báo cáo nguồn: Sprint5Ruong mục 1 và 4.

| Mã | Món | Nhóm nguồn gốc | Bậc | Giá nền | Trạng thái |
|---|---|---|---|---|---|
| V01 | Bình sứ men xanh | Quá khứ | Thường | 2.300.000 đ | ĐỀ XUẤT (giữ giá đang dùng) |
| V02 | Sách cổ bìa da | Quá khứ | Thường | 3.850.000 đ | ĐỀ XUẤT (giữ giá đang dùng) |
| V03 | Đồng hồ quả lắc cũ | Quá khứ | Thường | 4.950.000 đ | ĐỀ XUẤT (giữ giá đang dùng) |
| V04 | Mảnh kính thiên văn | Hiện đại | Thường | 4.150.000 đ | ĐỀ XUẤT (giữ giá đang dùng) |
| V05 | Thiết bị liên lạc hỏng | Tương lai | Thường | 3.100.000 đ | ĐỀ XUẤT (giữ giá đang dùng) |
| V06 | Bộ pin lượng tử | Tương lai | Hiếm | 18.450.000 đ | ĐỀ XUẤT (giữ giá đang dùng) |
| V07 | Tinh thể lam | Ngoài hành tinh | Rất hiếm | 26.950.000 đ | ĐỀ XUẤT (giữ giá đang dùng) |
| V09 | Lá bùa giấy vàng | Thần thoại | Thường | 8.950.000 đ | ĐỀ XUẤT (giữ giá đang dùng) |
| V10 | Mảnh vảy long | Thần thoại | Rất hiếm | 28.600.000 đ | ĐỀ XUẤT (giữ giá đang dùng) |
| N01 | Ấm trà đất nứt | Quá khứ | Hiếm | 15.500.000 đ | ĐỀ XUẤT (giá mới) |
| N02 | Hộp nữ trang gỗ khảm | Quá khứ | Hiếm | 17.500.000 đ | ĐỀ XUẤT (giá mới) |
| N03 | Tượng Di Lặc gỗ | Thần thoại | Rất hiếm | 27.000.000 đ | ĐỀ XUẤT (giá mới) |
| N04 | Tranh cuộn thủy mặc | Quá khứ | Hiếm | 17.000.000 đ | ĐỀ XUẤT (giá mới) |
| N05 | Con dấu đồng | Quá khứ | Hiếm | 16.000.000 đ | ĐỀ XUẤT (giá mới) |
| N06 | Máy ảnh phim cổ | Cổ tích | Hiếm | 19.000.000 đ | ĐỀ XUẤT (giá mới) |
| N07 | Radio bóng đèn | Hiện đại | Thường | 3.500.000 đ | ĐỀ XUẤT (giá mới) |
| N08 | Xúc xắc chì | Hiện đại | Thường | 2.600.000 đ | ĐỀ XUẤT (giá mới) |
| N09 | Bộ bài cũ | Quá khứ | Thường | 2.200.000 đ | ĐỀ XUẤT (giá mới) |
| N10 | Bật lửa mạ vàng | Tương lai | Hiếm | 16.500.000 đ | ĐỀ XUẤT (giá mới) |
| N11 | Đồng hồ xách tay | Hiện đại | Hiếm | 15.000.000 đ | ĐỀ XUẤT (giá mới) |
| N12 | Nhẫn kim cương không rõ nguồn | Cổ tích | Rất hiếm | 30.000.000 đ | ĐỀ XUẤT (giá mới) |
| L04 | Vé số độc đắc | Hiện đại | Huyền thoại | 40.000.000 đ | ĐỀ XUẤT (giá mới) |
| N13 | Bản đồ cổ vẽ tay | Quá khứ | Huyền thoại | 50.000.000 đ | ĐỀ XUẤT (giá mới) |
| N14 | Đồng hồ bỏ túi không hết pin | Tương lai | Huyền thoại | 50.000.000 đ | ĐỀ XUẤT (giá mới) |
| N15 | Két sắt chưa ai mở được | Ngoài hành tinh | Huyền thoại | 70.000.000 đ | ĐỀ XUẤT (giá mới) |

Huyền thoại cơ bản (ĐỀ XUẤT, Sprint5Ruong mục 1): Vé số độc đắc 40.000.000 đ, Bản đồ cổ vẽ tay 50.000.000 đ, Đồng hồ bỏ túi 50.000.000 đ, Két sắt chưa ai mở được 70.000.000 đ. Rác (9 món R01 đến R09) 30.000 đ mỗi món.

## 4. Bốn rương mở khóa

Trạng thái cả bảng: **ĐỀ XUẤT** (chưa duyệt; báo cáo Sprint5RuongMoKhoa mục 2). Đã chốt chỉ **luật mở**: rương mở khóa và chủ đề chỉ mở trong ván, không vĩnh viễn (DaChot, 2026-11-25), và luật shop ở mục 1. Tổng Hợp ra đúng các món của 4 rương cơ bản (quay đều trong hợp bộ sưu tập ở bậc vừa quay). Cổ Phong, Thiên Cơ, Rác Quý **chưa có bộ sưu tập món** (GIẢ ĐỊNH: bản đo dùng món giữ chỗ giá bằng giá trung bình bậc: Thường 3.960.000 đ, Hiếm 16.870.000 đ, Rất hiếm 28.140.000 đ, Huyền thoại 52.500.000 đ).

| Rương | Giá | R | T | H | X | L | Tạo tác | Hàng giả | Manh mối đúng | Trạng thái | Báo cáo nguồn |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Tổng Hợp | 6.000.000 đ | 16% | 39% | 28% | 14% | 3% | — | 20% | 2 trên 3 | ĐỀ XUẤT | Sprint5RuongMoKhoa.md mục 2 |
| Cổ Phong | 4.500.000 đ | 14% | 30% | 15% | 7% | 0% | 34% (bậc 1 60%, bậc 2 35%, bậc 3 5%; hệ CP) | 10% | 2 trên 3 | ĐỀ XUẤT | Sprint5RuongMoKhoa.md mục 2 |
| Thiên Cơ | 4.500.000 đ | 12% | 29% | 17% | 8% | 0% | 34% (bậc 1 60%, bậc 2 35%, bậc 3 5%; hệ TC) | 10% | 2 trên 3 | ĐỀ XUẤT | Sprint5RuongMoKhoa.md mục 2 |
| Rác Quý | 600.000 đ | 86% | 11% | 2,5% | 0% | 0,5% | — | 5% | 2 trên 3 | ĐỀ XUẤT | Sprint5RuongMoKhoa.md mục 2 |

Lời kỳ vọng mỗi hộp (Sprint5RuongMoKhoa mục 2): Tổng Hợp 1.780.000 đ (thu 1,30 lần giá), Cổ Phong 500.000 đ (1,11), Thiên Cơ 920.000 đ (1,21), Rác Quý 270.000 đ (1,45). Vai: Tổng Hợp là rương mở khóa mạnh nhất (Huyền thoại 3%); Cổ Phong và Thiên Cơ là "Kim Khí theo hệ", lời thấp có chủ ý (đắt hơn Kim Khí thì bot thường tụt còn 7% đến 11%, nên giữ 4.500.000 đ); Rác Quý là vé số rẻ. Giả định Cổ Phong và Thiên Cơ tính tạo tác trùng như Kim Khí (đổi 50% giá Kho): GIẢ ĐỊNH (câu hỏi mở 6 của báo cáo).

## 5. Khung rương chủ đề và Huyền thoại chủ đề

Một khung cho mọi chủ đề (Việt Nam, Tuổi thơ Việt Nam, Wibu, Truyền thuyết Châu Âu, Robot, Không gian...). Mở bằng thành tựu, có trong shop từ hôm sau (ĐÃ CHỐT, ruong-chu-de); chỉ mở trong ván (ĐÃ CHỐT). **Các số dưới đây: ĐỀ XUẤT** (Sprint5RuongMoKhoa mục 5).

| Số | Giá trị | Trạng thái | Báo cáo nguồn |
|---|---|---|---|
| Giá mỗi rương chủ đề | 8.000.000 đ | ĐỀ XUẤT | RuongMoKhoa mục 2 và 5 |
| Tỉ lệ bậc (R, T, H, X, L) | 13%, 30%, 30%, 24%, 3% (L là Huyền thoại chủ đề) | ĐỀ XUẤT | RuongMoKhoa mục 2 và 5 |
| Hàng giả; manh mối đúng | 20%; 1 trên 3 (đồ chủ đề cùng một nhóm nguồn gốc) | ĐỀ XUẤT | RuongMoKhoa mục 5 |
| Giá nền Huyền thoại chủ đề | 90.000.000 đ (chủ đề có nhiều Huyền thoại thì giữ trung bình quanh mức này; 3% chia đều) | ĐỀ XUẤT | RuongMoKhoa mục 5 |
| Huyền thoại chủ đề thay thế đã thử | 120.000.000 đ ở 2% vẫn trong mục tiêu; 5% hoặc 120.000.000 đ ở 3% làm rương quá hút (38% đến 44% lượt mua) | ĐỀ XUẤT (phương án phụ) | RuongMoKhoa mục 5 |
| Bộ sưu tập giữ chỗ | 2 Thường, 2 Hiếm, 2 Rất hiếm, 1 Huyền thoại chủ đề, cùng một nhóm nguồn gốc | GIẢ ĐỊNH | RuongMoKhoa mục 5 |
| Thu kỳ vọng; lời kỳ vọng mỗi hộp | 1,30 lần giá; 2.420.000 đ | ĐỀ XUẤT | RuongMoKhoa mục 2 |

## 6. Soi lỗi và trả giá

Mô hình dùng luật soi mới nhất, đo ở [Sprint5SoiLoi](Sprint5SoiLoi.md) (n = 2.000, nghiệm thu đạt: bot thường 16,8% và 15,5%, người chơi giỏi tối đa 45,9%). Spec: [soi-loi-tra-gia](../../docs/superpowers/specs/2026-11-23-soi-loi-tra-gia-design.md).

| Số | Giá trị | Trạng thái | Báo cáo nguồn |
|---|---|---|---|
| Giới hạn soi | **2 phút mỗi món** | ĐÃ CHỐT (DaChot, 2026-11-25) | SoiLoi (bảng chốt) |
| Hệ số giá theo tình trạng | Nguyên ×1,0 (50%), Xước ×0,7 (35%), Hỏng ×0,3 (15%) | ĐÃ CHỐT | soi-loi-tra-gia mục 1; content.json |
| Hàng giả bán còn | 20% giá | ĐÃ CHỐT | content.json (`fake_price_factor`) |
| Dấu lạ trên món thật | **15%**, cố ý giống dấu hàng giả | ĐÃ CHỐT (chủ dự án chốt 2026-11-25) | SoiLoi (bảng chốt) |
| Bỏ bấm lỗi đã thấy | Cho phép; người thu mua soi thấy thì trừ tiền và cộng phạt | ĐÃ CHỐT | SoiLoi (bảng chốt) |
| Nhà vá lỗi | Có; chỉ sửa lỗi đã tìm ra, lỗi sót không sửa được | ĐÃ CHỐT (có hay không) | SoiLoi (bảng chốt) |
| Phí nhà vá | lỗi nhẹ **12%** giá nền; vết hỏng lớn **30%** giá nền | ĐỀ XUẤT | SoiLoi mục 1 và 3 (rẻ 6% và 15% đẩy người giỏi lên 49,9%) |
| Nối bằng chứng với trả giá | Giữ: thắng mỗi vòng = kỹ năng trả giá × tỉ lệ lỗi đã tìm | ĐÃ CHỐT | SoiLoi (bảng chốt) |
| "Khó hơn" | Là cây khó mở và thưởng hậu hĩnh (mục 8), không hạ tỉ lệ thắng của soi lỗi | ĐÃ CHỐT | SoiLoi (bảng chốt) |
| Số lỗi nhẹ món Xước (Thường, Hiếm, Rất hiếm, Huyền thoại) | 1, 2, 3, 3; trừ tổng 30% | ĐỀ XUẤT | SoiLoi mục 1 |
| Món Hỏng | như Xước + 1 vết lớn; trừ tổng 70% (30% + 40%) | ĐỀ XUẤT | SoiLoi mục 1 |
| Số dấu hàng giả trên món giả (T, H, X, L) | 1, 1, 2, 2 | ĐỀ XUẤT | SoiLoi mục 1 |
| Phạt lỗi sót (kể cả lỗi thấy mà không bấm) | **10%** giá thẩm định mỗi lỗi, cộng trừ giá theo lỗi | ĐỀ XUẤT | SoiLoi mục 1 và 3 |
| Giả mà khai thật | Tịch thu: phạt 20%, nhận 0 đ; ngày 1 miễn phạt | ĐỀ XUẤT | SoiLoi mục 1; phạt 0% đẩy người giỏi lên 49,8% |
| Khai giả nhận giá hàng giả | 20% giá (lỗ 80% nếu hóa ra thật) | ĐỀ XUẤT | SoiLoi mục 1 |
| Trả giá: số vòng; mở màn; thắng; thua | 3 vòng; chào mở 100% giá bảng đã trừ lỗi; thắng +8%; thua −5% | ĐỀ XUẤT | SoiLoi mục 1 |
| Trả giá: sàn; **trần** | 70%; **108%** | ĐỀ XUẤT | SoiLoi mục 1. Trần là đòn bẩy mạnh nhất: 110% cho người giỏi 49,0%; dự phòng nếu muốn dư biên: 105% |
| Trả giá áp dụng từ bậc | Hiếm trở lên (Thường bán nhanh) | ĐỀ XUẤT | SoiLoi mục 1 |
| Giáo Sư Lẫm: gợi ý | chỉ ra một dấu giả chưa thấy (hoặc một lỗi chưa thấy), phí 5% giá nền, tối thiểu 200.000 đ; bỏ chứng nhận +25% | ĐỀ XUẤT | SoiLoi mục 1 |
| Xác suất tìm mỗi lỗi trong 2 phút | giỏi 95%, thường 75%, ẩu 45%, bỏ qua 0% | **GIẢ ĐỊNH** (chưa kiểm tra bằng người thật; nếu người thật tìm kém, bot thường có thể xuống dưới 15%: s = 60% cho 13,7%) | SoiLoi mục 1 và 5 |
| Kỹ năng trả giá của hồ sơ | giỏi 95%, thường 75%, ẩu 50% | GIẢ ĐỊNH | SoiLoi mục 1 |

### Lần chơi đầu

| Số | Giá trị | Trạng thái | Báo cáo nguồn |
|---|---|---|---|
| Ngày 1 lần chơi đầu chỉ bán Rương Gỗ và Rương Vải Niêm | Kim Khí không bán ngày 1; Đen vốn bán từ ngày 4 | ĐÃ CHỐT (DaChot: Mở màn, 2026-11-24) | Sprint5Ruong mục 3; mo-man mục 5 |
| Hộp đầu ra rác hài, hộp hai ra đồ Thường bán được | chỉ lần chơi đầu | ĐÃ CHỐT | mo-man |
| Ngày 1 không thể thua | nợ ngày 1 2.000.000 đ, nợ tồn tối đa 2.400.000 đ so với ngưỡng 10.000.000 đ | ĐÃ CHỐT (đo xác nhận: 0 ván thua trước ngày 3) | Sprint5Ruong mục 3; SoiLoi mục 2 |
| Lần chơi đầu được nới | chủ dự án chốt "nới" | ĐÃ CHỐT (có nới) | SoiLoi (bảng chốt) |
| Cách cài nới | `cheap`: hai hộp đầu chỉ tốn giá rương rẻ nhất; bot thường thắng ván đầu 10,1% và 10,3% | ĐỀ XUẤT (cần Codex chốt cách cài và Claude 1 xác nhận khớp mo-man) | SoiLoi mục 2 và 6 |

## 7. Ô tạo tác và Kim Khí trùng

| Số | Giá trị | Trạng thái | Báo cáo nguồn |
|---|---|---|---|
| Ô tạo tác | 4; BK1 mở ô thứ 5 | ĐÃ CHỐT | Sprint5Ruong mục 4 |
| Tạo tác từ Kim Khí | 34% lượt mở; bậc 1 60%, bậc 2 35%, bậc 3 5%; chưa có thì vào ô trống | ĐÃ CHỐT | Sprint5Ruong mục 1 |
| Tạo tác trùng hoặc hết ô | đổi 50% giá Kho Tạo Tác | ĐÃ CHỐT | DaChot; Sprint5Ruong mục 1 |
| Ảnh hưởng kinh tế của 3, 4, 5 ô | chênh dưới 1 điểm thắng; 4 ô làm bot combo lớn đạt CB3 gấp đôi (12% lên 25%) | số đo (không phải quyết định) | Sprint5Ruong mục 4 |

Vấn đề nằm ở **thiết kế**, không ở tiền: 4 ô thì 15 trên 15 bộ có combo (BanGiaoSprint5.md). Thêm tạo tác hay giữ 6 là của Claude 1 và chủ dự án.

## 8. Cây phát triển trong ván

Spec: [ruong-suu-tap](../../docs/superpowers/specs/2026-11-23-ruong-suu-tap-design.md) (mục "Đã chốt và câu hỏi mở sau khi cây chỉ trong ván"). **Đã chốt:** cây chỉ trong ván, không tiến trình vĩnh viễn, làm lại mỗi ván; điểm kiếm từ việc hằng ngày; rương mở khóa và chủ đề chỉ mở trong ván; cây khó mở; tổng thưởng có tác động lên tiền hoặc rương **không quá khoảng 1.000.000 đ**; M3 bỏ khỏi cây; M2 bỏ phần cộng vốn; nhánh Câu chuyện đổi tên **Đời sống**, tạm để trống. Cây còn **15 nút, tổng 79 điểm** (bản đo cũ 19 nút, 99 điểm).

| Số | Giá trị | Trạng thái | Báo cáo nguồn |
|---|---|---|---|
| Phạm vi cây | chỉ trong ván | ĐÃ CHỐT (DaChot, 2026-11-25) | CayPhatTrien |
| Độ khó | cây khó (giá đủ): 3 đến 4 nút mỗi ván | ĐÃ CHỐT (hướng) | CayPhatTrien mục 1 |
| Trần thưởng có tác động tiền hoặc rương | khoảng 1.000.000 đ quy đổi; mỗi 1.000.000 đ thưởng sớm cộng khoảng 2,4 điểm cho người hoàn hảo | ĐÃ CHỐT | CayPhatTrien mục 1 và 4 |
| Nguồn điểm | việc hằng ngày (mở rương, mini game) | ĐÃ CHỐT (nguồn); cách chia điểm từng việc còn mở | ruong-suu-tap |
| Điểm mỗi ngày | 3 (ván 7 ngày kiếm tối đa 21, thực tế 11,4 đến 14,0) | **GIẢ ĐỊNH** (bản đo; chưa chia theo từng việc) | CayPhatTrien mục 1 và 3 |
| Giá nút (điểm) | bảng dưới | ĐỀ XUẤT | CayPhatTrien mục 2 |
| Điều kiện mở nút | bảng dưới (Sổ tính trong ván, rỗng đầu ván) | GIẢ ĐỊNH | CayPhatTrien mục 1 và 2 |
| Nhánh Đời sống | để trống, không có nút | ĐÃ CHỐT | DaChot; ruong-suu-tap |
| M2 sau khi bỏ cộng vốn | giữ làm nút gì hoặc bỏ hẳn: **chưa chốt**; thưởng không buff do Claude 1 và chủ dự án gán | câu hỏi mở | ruong-suu-tap (Còn mở 2) |
| M3 Thử Thách Rương Đen | bỏ khỏi cây; chỉ là tùy chọn lúc vào ván nếu chủ dự án muốn | ĐÃ CHỐT | DaChot |

Giá nút "đủ" (cây khó). Giá "nửa" (chia đôi) làm người hoàn hảo vượt trần (48,0%) nên **không dùng**.

| Nhánh | Nút (cha) | Điểm | Điều kiện trong ván | Tác động |
|---|---|---|---|---|
| Rương | R1 (gốc) | 3 | book 3 | — |
| Rương | R2 (R1) | 4 | junk 2 | Rác Quý vào shop |
| Rương | R3 (R1) | 6 | origins 3 | Tổng Hợp vào shop |
| Rương | R4 (R3) | 7 | art CP:1 | Cổ Phong vào shop |
| Rương | R5 (R3) | 7 | art TC:1 | Thiên Cơ vào shop |
| Sưu tập | S1 (gốc) | 3 | book 3 | — |
| Sưu tập | S2 (S1) | 4 | book 6 | — |
| Sưu tập | M1 (S2) | 5 | book 8 | chọn GT1 hoặc GT2 |
| Sưu tập | M2 (M1) | 7 | book 10 | thưởng không buff, chưa gán |
| Sưu tập | S3 (S2) | 6 | legend 1 | — |
| Sưu tập | M4 (S3) | 7 | art_any 2 | — |
| Thần lực | T1 (gốc) | 3 | book 3 | — |
| Thần lực | T2 (T1) | 4 | origins 3 | — |
| Thần lực | T3 (T2) | 6 | book 9 | — |
| Thần lực | T4 (T3) | 7 | legend 1 | — |
| Đời sống | (để trống) | — | — | — |

Số đo cây (n = 3.000, luật soi mới nhất; CayPhatTrien mục 4): nền bot 15,0% / 16,9%, người hoàn hảo 44,1%; cây khó trong ván 44,6%; cây dễ 48,0% (vượt trần); cây khó + thưởng vốn 1.000.000 đ 47,0%, 2.000.000 đ 49,1%.

## 9. Điểm lệch cần Codex và chủ dự án biết

Quy tắc: hai báo cáo mâu thuẫn thì lấy số mới nhất và DaChot làm chuẩn. JSON và bảng trên đã theo chuẩn đó.

1. **Cây trong báo cáo cũ và trong mã mô phỏng còn 19 nút, 99 điểm, có nhánh Câu chuyện (C1 đến C4), M2 cộng vốn.** DaChot (2026-11-25) đã bỏ M3, bỏ phần cộng vốn M2, đổi Câu chuyện thành Đời sống (trống). Bảng và JSON theo DaChot: 15 nút, 79 điểm. `QA/tree_run.RUN_TREE` và `QA/test_tree.py` chưa cập nhật (test_balance_data chỉ so các nút còn lại). Giá điểm từng nút và cách chia điểm cho từng việc hằng ngày cần đo lại trên cây mới (ruong-suu-tap, Còn mở 1).
2. **Trần trả giá:** Sprint5SoiLoi.md và mã (`inspect_sim.RULES`) là **108%**. Đầu Sprint5CayPhatTrien.md ghi "trần 1,05" nhưng thân bài cùng báo cáo dùng 108%. Lấy 108%. Người chơi giỏi sát trần 47% (45,9%); nếu cần dư biên thì hạ 105%.
3. **Số nền khác nhau giữa hai báo cáo** vì n và hạt giống khác, cùng luật soi: SoiLoi (n = 2.000) bot 16,8% / 15,5%, giỏi 45,9%; CayPhatTrien (n = 3.000) bot 15,0% / 16,9%, giỏi 44,1%. Cả hai trong nhiễu (±1 điểm).
4. **Bảng rương mở khóa đo dưới luật soi cũ; đo lại ở ghi chú cuối RuongMoKhoa** (n = 3.000, luật mới): người chơi giỏi 44,3% (4 mở khóa), 42,8% (cộng 2 chủ đề); **bot 12% đến 14%, dưới mục tiêu 15%** khi có nhiều rương từ ngày 1. Báo cáo nói cây khó trong ván giữ bot ở 15,0% / 16,9%, nên vấn đề chỉ xuất hiện nếu rương mở khóa có từ ngày 1. Con số "sát trần 47,1%" của đo cũ không lặp lại.
5. **Bảng Tổng Hợp:** RuongMoKhoa mục 2 (hàng giả 20%, T 39%, H 28%, X 14%) khác bản "hàng giả 10%, Thường 43%, Hiếm 25%, Rất hiếm 13%" nhắc ở CayPhatTrien mục 6 (đề xuất cho luật soi cũ). Lấy RuongMoKhoa (khớp `chest_unlock.UNLOCK`).
6. **Rương mở khóa "chỉ mở trong ván" (DaChot)** nhưng phần lớn đo ở RuongMoKhoa giả định rương có từ ngày 1 hoặc hôm sau khi mở. Kịch bản trong ván đo ở CayPhatTrien (cây khó 44,6%).
7. **Lần chơi đầu:** DaChot chốt "ngày 1 chỉ bán Gỗ và Vải Niêm" (đo bot 8,1% đến 10,1%, Sprint5Ruong mục 3). SoiLoi đề xuất thêm cách nới `cheap` (hai hộp đầu chỉ tốn giá rương rẻ nhất). Hai cách chồng nhau, chưa đo gộp. Codex chốt cách cài; Claude 1 xác nhận khớp mo-man. Bot lần chơi đầu dưới 15% là chủ ý (lần chơi đầu khó hơn ván thường).
8. **Gỗ thu / giá (đã giải quyết 2026-11-25):** Sprint5Ruong mục 1 ghi 2,29 lần là ĐÚNG. Phép tính chéo ban đầu (2,56) quên luật Lá bùa V09 có hàng giả ít nhất 35% ở mọi rương (`content.json` `item_fake_rate_floor.V09`, trong mô phỏng `max(tỉ lệ rương, 0,35)`). Tính lại có luật này ra 2,29, khớp mô phỏng và báo cáo. JSON đã thêm `fake_rate_floor` 0,35 cho V09; Codex nhớ áp luật này khi quay hàng giả.
9. **`content.json` (Claude 1) chưa cập nhật theo bảng này:** vẫn `equip_slots` 3 (`equip_slots_with_bk1` 4; đã chốt 4 và 5), tiền theo đơn vị xu cũ, bộ hộp H1 đến H3 cũ. JSON này là nguồn cho phần cân bằng; Codex đừng trộn số hai file.
10. **Mã và tên món:** mã `V*`, `N*`, `L04`, `PC*`, `PT*`, `PR*` là mã mô phỏng. Một số tên khác nhẹ so với `content.json` (V04 "Mảnh kính thiên văn" so với "Mảnh kính đồng hồ thiên văn", V07 "Tinh thể lam" so với "Tinh thể lam cầm tay"); V08 có trong `content.json` nhưng không nằm trong bộ sưu tập nào của mô phỏng. Mã chính thức do Claude 1 và chủ dự án chốt (TASKS: mã tạm `R`, `H`, `X`, `L`).
11. **Đã chốt chưa nằm trong spec:** bảng chốt 2026-11-25 của SoiLoi (dấu lạ 15%, bỏ bấm cho phép, thêm nhà vá, nối bằng chứng, nới lần chơi đầu) mới có trong báo cáo, nay đã thêm một dòng vào DaChot.

## 10. Còn mở (không tự quyết)

- Chủ dự án duyệt bảng rương mở khóa và khung chủ đề (mục 4 và 5), và các tham số soi lỗi ĐỀ XUẤT (phí vá, phạt sót, trần 108%).
- Claude 1: bộ sưu tập Cổ Phong, Thiên Cơ, Rác Quý; giá nền món mới; nhóm nguồn gốc (nên khác nhóm giữa bậc cao và bậc thấp trong một rương); nút M2; thành tựu mở rương chủ đề.
- Claude 2 (phiên sau): đo lại cây 15 nút với cách chia điểm theo việc hằng ngày; Rác Quý nếu có nhóm nguồn gốc riêng dễ nhận ra (người chơi giỏi lên 50%).
- Chủ dự án: có muốn lần chơi đầu dễ ngang ván thường không.

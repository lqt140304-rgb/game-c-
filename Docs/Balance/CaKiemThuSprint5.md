# Ca kiểm thử biên Sprint 5 (BT-17 trở đi)

Viết ngày 2026-11-25 (Claude 2). **Để Codex viết kiểm thử tự động** cho luật Sprint 5 (nghiệm thu của Codex: "kiểm thử tự động bao phủ luật mới và lưu/tải giữ nguyên bằng chứng"). Nối tiếp bộ ca BT-01 đến BT-16 ở [README mục 4](README.md); mã bắt đầu từ **BT-17**, không trùng. Số liệu lấy từ [BangSoLieuSprint5.md](BangSoLieuSprint5.md) và [balance_sprint5.json](../Content/data/balance_sprint5.json); luật dẫn từ [DaChot](../DaChot.md) và các spec ở `docs/superpowers/specs/`.

> **Tiền tệ:** mọi số tiền bằng đồng Việt Nam. Mô hình tham chiếu tính theo đơn vị cân bằng 10.000 đ (số nguyên); các ca dưới đây ghi đồng. Mọi giá, phí, tiền bán là bội số của 10.000 đ.

## Cách đọc

**Cột "Luật"** (lấy từ cột trạng thái của BangSoLieuSprint5):

- **CHỐT** — chủ dự án đã duyệt (DaChot hoặc spec). Ca này phải đạt, đổi thì phải có quyết định mới.
- **ĐỀ XUẤT** — Claude 2 đề xuất, đã đo, chưa duyệt. **Codex biết là con số hoặc cách làm có thể đổi**: nạp số từ JSON, đừng ghi cứng trong test.
- **GIẢ ĐỊNH** / **MỞ** — mô hình tự giả định hoặc luật chưa chốt. Ca ghi hành vi hiện tại để so sánh, chưa phải yêu cầu.

**Cột "Kiểm chứng":**

- `QA/test_sprint5_cases.py::test_BTxx` — đã có test mẫu chạy trên mô hình tham chiếu (`chest_sim`, `chest_unlock`, `inspect_sim`, `balance_sprint5.json`, `currency`). Chạy `cd QA && python3 -m unittest -v test_sprint5_cases` (dưới 1 giây). Codex viết test tương đương trên game thật, đối chiếu cùng số.
- **Game** — mô hình không kiểm chứng được (giao diện, lưu và tải thật, đồng hồ 2 phút, cờ lần chơi đầu). Chỉ ghi ở đây, **không có test giả**; Codex tự viết.

Các test mẫu dùng hạt giống cố định, kết quả lặp lại được. Ngưỡng thống kê có biên rộng hơn sai số.

## (a) Ngày 1 của lần chơi đầu

Spec: [mo-man](../../docs/superpowers/specs/2026-11-23-mo-man-design.md) mục 5. Số đo: [Sprint5Ruong](Sprint5Ruong.md) mục 3.

| Mã | Tình huống | Điều kiện đầu vào | Kết quả mong đợi | Luật / nguồn | Kiểm chứng |
|---|---|---|---|---|---|
| BT-17 | Hộp đầu ra rác | Lần chơi đầu, ngày 1, mua hộp thứ nhất (Gỗ 1.400.000 đ hoặc Vải Niêm 3.500.000 đ) | Món là một trong 9 rác (R01 đến R09), không phải hàng giả, giá nền 30.000 đ. Ván khác nhau ra rác khác nhau (rác hài, không cố định một món) | CHỐT (hộp đầu ra rác, mo-man); giá rác 30.000 đ ĐỀ XUẤT (BangSo mục 1) | `test_BT17` |
| BT-18 | Hộp hai ra đồ Thường bán được | Như trên, mua hộp thứ hai | Món bậc Thường thuộc bộ sưu tập của rương đã mua (Gỗ: V01, V03, V09; Vải Niêm: V02, V04), thật, không Hỏng, giá bán lớn hơn 0 đ | CHỐT (mo-man); bộ sưu tập theo bậc GIẢ ĐỊNH (BangSo mục 2) | `test_BT18` |
| BT-19 | Hộp thứ ba ngày 1 | Mua thêm hộp thứ ba | Không sắp sẵn: quay hai bước như thường (qua nhiều ván có cả rác lẫn không rác) | CHỐT | `test_BT19` |
| BT-20 | Cửa hàng ngày 1 chỉ bán Gỗ và Vải Niêm | Lần chơi đầu, ngày 1, thử mua Kim Khí 4.500.000 đ và Rương Đen 12.000.000 đ | Chỉ Gỗ và Vải Niêm mua được. Kim Khí và Đen không mua được (nút bị khóa hoặc không bày), tiền và số lượt mua không đổi | CHỐT (DaChot: Mở màn) | `test_BT20` |
| BT-21 | Hết giới hạn: ngày 2 và ván thường | (1) Lần chơi đầu, sang ngày 2. (2) Ván thường (không phải lần chơi đầu), ngày 1 | (1) Cửa hàng bán đủ. (2) Ngày 1 bán đủ 3 rương cơ bản rẻ (Gỗ, Vải Niêm, Kim Khí; Đen vẫn từ ngày 4) và **không** có hộp sắp sẵn | CHỐT ("các ván sau bán đủ") | `test_BT21` |
| BT-22 | Từ ngày 2 hoàn toàn ngẫu nhiên | Lần chơi đầu, ngày 2, mua hộp đầu, nhiều lần (nhiều hạt giống) | Không sắp sẵn. Tỉ lệ ra rác của Gỗ xấp xỉ 18% (không phải 100%), hộp thứ hai không bị ép đồ Thường | CHỐT | `test_BT22` |
| BT-23 | Ngày 1 không thua ngay, kể cả ra rác | Lần chơi đầu, vốn 10.000.000 đ, nợ ngày 1 2.000.000 đ, người chơi mua liều | Không ván nào thua ở ngày 1 hoặc trước ngày 3. Nợ tồn tối đa về lý thuyết 2.000.000 đ + phạt 20% = 2.400.000 đ, thấp hơn ngưỡng thua 10.000.000 đ. Đường thắng còn (mô hình: bot mua liều vẫn thắng được một phần ván đầu) | CHỐT (ngày 1 không thua); số đo: Sprint5Ruong mục 3 | `test_BT23` |
| BT-24 | Nới lần chơi đầu | Lần chơi đầu, hai hộp đầu mua ở Vải Niêm 3.500.000 đ | Cách cài hiện đo (`cheap`): hai hộp đầu chỉ tốn bằng giá rương rẻ nhất, phần chênh được hoàn; hộp thứ ba không hoàn | Việc **có nới**: CHỐT. **Cách cài `cheap`: ĐỀ XUẤT, Codex có thể đổi** (BangSo mục 6, mục 9.7) | `test_BT24` |
| BT-60 | Cờ "lần chơi đầu" | Chơi hết ván một, bắt đầu ván hai; xóa dữ liệu lưu, bắt đầu lại | Ván hai: không hộp sắp sẵn, bán đủ. Dữ liệu lưu mới hoàn toàn: lại là lần chơi đầu | MỞ: DaChot chỉ chốt "không tiến trình vĩnh viễn" cho cây; cờ lần chơi đầu là trạng thái giữa các ván, chưa nói lưu ở đâu | Game |
| BT-61 | Thoát giữa ngày 1 sau hộp sắp sẵn | Mở hộp đầu (rác), lưu, thoát, tải lại | Tải lại không quay lại hộp sắp sẵn lần nữa, không sinh thêm rác; hộp thứ hai vẫn là đồ Thường | CHỐT (hộp sắp sẵn); lưu và tải: cùng nhóm BT-15 | Game |

## (b) Hàng giả

Số liệu: BangSoLieuSprint5 mục 2; luật V09: [content.json](../Content/data/content.json) `item_fake_rate_floor`, [NoiDungMau](../Content/NoiDungMau.md) mục 1.

| Mã | Tình huống | Điều kiện đầu vào | Kết quả mong đợi | Luật / nguồn | Kiểm chứng |
|---|---|---|---|---|---|
| BT-25 | Lá bùa V09 hàng giả ít nhất 35% ở mọi rương | V09 (Lá bùa giấy vàng 8.950.000 đ) quay từ Gỗ (tỉ lệ giả rương 10%), Vải Niêm (15%), Đen (30%), Tổng Hợp (20%) | Tỉ lệ giả của V09 là lớn hơn của (tỉ lệ rương, 35%): 35% ở cả bốn rương trên. Rương có tỉ lệ giả cao hơn 35% (ví dụ 50%) giữ tỉ lệ rương. Đo trên nhiều lần quay của Gỗ: khoảng 35% (không phải 10%) | Luật nội dung hiện hành của Claude 1 (content.json), **chưa có dòng riêng trong DaChot**; xem mục "Điểm lệch" 2 | `test_BT25` |
| BT-26 | Rác không có hàng giả | Mọi lần quay ra rác, ở mọi rương | Rác không bao giờ là hàng giả; không soi lỗi, không giám định | ĐỀ XUẤT (BangSo mục 1: "không giả, không giám định") | `test_BT26` |
| BT-27 | Kim Khí không giả | Mở Kim Khí 4.500.000 đ nhiều lần, kể cả khi ra tạo tác | Mọi món (V05, N07, V06, N06, V07, N14) và tạo tác đều thật. V09 không nằm trong bộ Kim Khí nên luật sàn 35% không đụng luật Kim Khí 0% | CHỐT (Kim Khí hàng giả 0%) | `test_BT27` |
| BT-28 | Tỉ lệ giả theo bảng | Một món không phải V09 ở từng rương cơ bản | Gỗ 10%, Vải Niêm 15%, Kim Khí 0%, Đen 30% | CHỐT (bảng 4 rương cơ bản) | `test_BT28` |
| BT-62 | Giả và thật nhìn giống nhau trước khi soi | Món giả và món thật cùng bậc, chưa soi | Giá hiển thị trước giám định như hàng thật (`unappraised_display_price: as_genuine`); chỉ soi lỗi hoặc giám định mới lộ dấu | CHỐT (content.json); dấu lạ trên món thật 15% cố ý giống dấu giả: CHỐT | Game |

## (c) Quay hai bước (bậc rồi món)

Spec: [ruong-chu-de](../../docs/superpowers/specs/2026-11-24-ruong-chu-de-design.md) ("Cơ chế mở rương").

| Mã | Tình huống | Điều kiện đầu vào | Kết quả mong đợi | Luật / nguồn | Kiểm chứng |
|---|---|---|---|---|---|
| BT-29 | Tỉ lệ bậc cộng 100% | Bảng tỉ lệ (R, T, H, X, L) của từng rương | Tổng bằng 100% ở mọi rương. Kim Khí và rương mở khóa ra tạo tác: tỉ lệ bậc + phần tạo tác (Kim Khí 34%) cũng bằng 100%. Ví dụ Gỗ: 18 + 79,5 + 1 + 1 + 0,5 | 4 rương cơ bản CHỐT; **4 rương mở khóa và khung chủ đề ĐỀ XUẤT, có thể đổi** (BangSo mục 4 và 5) | `test_BT29` |
| BT-30 | Mỗi món một rương, Rác dùng chung | Bộ sưu tập 4 rương cơ bản | Không món nào nằm ở hai rương cơ bản. Rác (9 món) quay được ở mọi rương, mỗi món rác có xác suất bằng nhau. Tổng Hợp ra đúng đồ của 4 rương cơ bản | CHỐT (ruong-chu-de); bộ sưu tập từng rương GIẢ ĐỊNH, chờ Claude 1; Tổng Hợp ĐỀ XUẤT | `test_BT30` |
| BT-31 | Quay hai bước | Rương Gỗ, nhiều lần quay | Bước 1 quay bậc theo bảng; bước 2 quay **đều** một món trong bậc. Xác suất mỗi món = tỉ lệ bậc chia số món của bậc (Gỗ Thường 79,5% chia 3 món = 26,5% mỗi món; Rất hiếm chỉ 1 món = 1%). Tần suất đo khớp bảng trong biên 1,2 điểm | CHỐT | `test_BT31` |
| BT-32 | Cùng hạt giống, cùng kết quả sau lưu và tải | Ván hạt giống A: quay 15 hộp, lưu, quay tiếp 25 hộp; tải bản lưu, quay lại 25 hộp | Hai đoạn 25 hộp giống hệt nhau (món, tình trạng, thật hoặc giả); chạy lại từ đầu với hạt giống A cho cùng chuỗi. Mô hình chỉ chứng minh bộ quay xác định; **lưu và tải thật phải test ở game** | Yêu cầu nghiệm thu của Codex; cách lưu (trạng thái bộ sinh số hay hạt giống + số thứ tự hộp) **MỞ**, xem mục "Điểm lệch" 7 | `test_BT32` (mô hình), BT-63 (game) |
| BT-63 | Lưu và tải thật không đổi kết quả hộp | Ngày 3, trước khi mở hộp: lưu, mở hộp (ghi kết quả), tải, mở lại hộp cùng vị trí | Cùng món, cùng tình trạng, cùng thật hoặc giả. Mua hộp theo thứ tự khác sau khi tải: kết quả có thể khác (chỉ cần chốt rõ quy tắc) | MỞ (quy tắc khóa kết quả) | Game |

## (d) Tạo tác

Giá Kho Tạo Tác ([content.json](../Content/data/content.json)): CP1 1.500.000 đ, TC1 1.200.000 đ, CP2 4.000.000 đ, TC2 4.500.000 đ, CP3 và TC3 5.000.000 đ.

| Mã | Tình huống | Điều kiện đầu vào | Kết quả mong đợi | Luật / nguồn | Kiểm chứng |
|---|---|---|---|---|---|
| BT-33 | Kim Khí ra tạo tác trùng | Đã có CP1, Kim Khí lại ra CP1 | Không trang bị lần hai; đổi tiền **50% giá Kho**: 750.000 đ (CP1), 600.000 đ (TC1), 2.000.000 đ (CP2), 2.250.000 đ (TC2), 2.500.000 đ (CP3, TC3). Lần đầu (chưa có) thì vào ô trống, không đổi tiền | CHỐT (DaChot: Giá và tỉ lệ 4 rương cơ bản) | `test_BT33` |
| BT-34 | 4 ô mặc định, ô thứ 5 sau BK1 | Đã trang bị 4 tạo tác khác nhau, chưa mở BK1; Kim Khí ra tạo tác thứ 5 mới (chưa có) | Ô thứ 5 chưa mở nên đổi tiền 50% giá Kho (hết ô cũng đổi). Sau khi mở Bí Khế BK1: trang bị được ô thứ 5. Dữ liệu: `equip_slots` 4, `equip_slots_with_bk1` 5 | CHỐT. Lưu ý `content.json` còn 3 và 4 (BangSo mục 9.9) | `test_BT34` |
| BT-35 | Chỉ Kim Khí ra tạo tác | Mở nhiều lần Kim Khí, Gỗ, Vải Niêm, Đen | Chỉ Kim Khí ra tạo tác: 34% lượt mở, bậc 1/2/3 = 60%/35%/5%. Gỗ, Vải Niêm, Đen không bao giờ ra tạo tác | CHỐT (cơ bản). Cổ Phong, Thiên Cơ (mở khóa) cũng ra tạo tác: ĐỀ XUẤT | `test_BT35` |
| BT-64 | Ảnh hưởng 4 ô lên cộng hưởng | Bộ 4 tạo tác đủ hệ | Số ô đúng, combo tính đúng với 4 ô; thêm ô thứ 5 sau BK1 không phá luật cộng hưởng. Mô hình chỉ đo kinh tế (chênh dưới 1 điểm thắng), không kiểm combo | CHỐT (số ô); vấn đề "4 ô làm combo thành mặc định" là việc thiết kế (BangSo mục 7), chưa quyết | Game |

## (e) Soi lỗi và trả giá

Spec: [soi-loi-tra-gia](../../docs/superpowers/specs/2026-11-23-soi-loi-tra-gia-design.md), số: [Sprint5SoiLoi](Sprint5SoiLoi.md), BangSo mục 6. **Mọi số dưới đây lấy đúng từ BangSo mục 6.** Nhớ rằng phí vá, phạt sót, phạt khai giả, trần trả giá, số lỗi mỗi bậc là **ĐỀ XUẤT**; test nạp số từ JSON.

| Mã | Tình huống | Điều kiện đầu vào | Kết quả mong đợi | Luật / nguồn | Kiểm chứng |
|---|---|---|---|---|---|
| BT-36 | Bảng số khớp | Mô hình và JSON | Cap 108%, floor 70%, 3 vòng, +8% / −5%, bán giả 20%, phạt 20%, ngày 1 phạt 0, phạt sót 10%, dấu lạ 15%, phí vá 12% và 30%, gợi ý 5% (tối thiểu 200.000 đ), số lỗi nhẹ 1/2/3/3, số dấu giả 1/1/2/2, giới hạn 120 giây | Hỗn hợp: xem từng dòng ở BangSo mục 6 | `test_BT36` |
| BT-65 | Giới hạn 2 phút mỗi món | Bắt đầu soi một món; để đồng hồ chạy hết 120 giây | Hết 120 giây: khóa soi, chuyển sang bước chọn thật hoặc giả với những gì đã bấm. Lỗi chưa bấm tính là lỗi sót (BT-43) | **CHỐT** giới hạn 2 phút (DaChot, 2026-11-25); hậu quả khi hết giờ: GIẢ ĐỊNH của mô hình (xem mục "Điểm lệch" 3) | Game |
| BT-66 | 2 phút tính riêng từng món | Soi món A dùng 50 giây, sang món B (cùng ngày, tối đa 3 hộp) | Món B có đủ 120 giây; thời gian không dồn từ món A, không cộng dồn món đã soi xong | CHỐT (giới hạn "mỗi món") | Game |
| BT-67 | Thoát hoặc tải lại giữa lúc soi | Còn 40 giây, lưu, thoát, tải | Đồng hồ còn 40 giây (không về 120 giây, không bị hết giờ ngay). Lỗi đã bấm giữ nguyên | MỞ (spec chưa nói); nối BT-72 | Game |
| BT-37 | Bằng chứng nối với trả giá | Món có 3 lỗi; tìm ra 0, 1, 2, 3 lỗi | Xác suất thắng một vòng = kỹ năng trả giá × (lỗi đã tìm / tổng lỗi). Tìm 0 lỗi: không trả giá được, giữ giá chào mở 100%. Càng tìm nhiều càng dễ thắng. Món không có lỗi: đủ bằng chứng | CHỐT ("giữ nối bằng chứng với trả giá") | `test_BT37` |
| BT-68 | Bằng chứng hiển thị ở màn trả giá | Bấm 2 trên 3 lỗi, vào trả giá | Có đúng 2 bằng chứng (lỗi đã bấm, không phải lỗi chỉ thấy); đưa bằng chứng đúng thì giá chào tăng, đưa sai thì giảm | CHỐT (soi-loi-tra-gia mục 4, bước 5 mục 2) | Game |
| BT-38 | Trần trả giá | Tượng Di Lặc gỗ (27.000.000 đ, Nguyên, tìm đủ bằng chứng), thắng cả 3 vòng | Giá không vượt **108%** giá đã trừ lỗi: 29.160.000 đ (nếu không có trần sẽ là 124%). Thua cả 3 vòng: 85%, không xuống dưới sàn 70% | **ĐỀ XUẤT: trần 108% có thể đổi** (dự phòng 105%; 110% đẩy người giỏi lên 49,0%, vượt mục tiêu 47%) | `test_BT38` |
| BT-39 | Món Thường bán nhanh | Bình sứ men xanh (2.300.000 đ), Thường | Giá bằng đúng giá thẩm định, không trả giá | ĐỀ XUẤT (trả giá từ bậc Hiếm trở lên) | `test_BT39` |
| BT-40 | Khai món thật là giả | Món thật (Tượng Di Lặc 27.000.000 đ), người chơi khai "giả" | Chỉ nhận 20% giá: 5.400.000 đ (lỗ 80%) | ĐỀ XUẤT (khai giả nhận 20%); hệ số 20% giá hàng giả: CHỐT | `test_BT40` |
| BT-41 | Khai món giả là thật: tịch thu | Món giả (Tượng Di Lặc, hoặc Lá bùa V09 8.950.000 đ), khai "thật", **ngày 2 trở đi** | Bị tịch thu: giá hàng giả trừ phạt 20% bằng **0 đ**. **Ngày 1 miễn phạt**: nhận giá hàng giả 20% (V09: 1.790.000 đ; Tượng Di Lặc: 5.400.000 đ) | ĐỀ XUẤT (phạt 20%, ngày 1 miễn phạt: spec soi-loi-tra-gia mục 3 và DaChot đều muốn ngày 1 nhẹ hoặc miễn) | `test_BT41` |
| BT-42 | Khai giả đúng | Món giả, khai "giả" (mọi ngày) | Nhận 20% giá, không phạt | ĐỀ XUẤT; 20%: CHỐT | `test_BT42` |
| BT-43 | Lỗi sót, thấy mà không bấm, hết giờ | Ấm trà đất nứt N01 (15.500.000 đ), Xước (2 lỗi nhẹ); người chơi tìm ra 0 lỗi | Giá thẩm định 10.850.000 đ (đã ×0,7), phạt 10% giá thẩm định mỗi lỗi sót (2 × 1.090.000 đ = 2.180.000 đ), không được trả giá: nhận 8.670.000 đ | Phạt sót 10%: ĐỀ XUẤT; "thấy mà không bấm tính là sót": CHỐT; "hết giờ tính là sót": GIẢ ĐỊNH. Mô hình **không phân biệt** chưa thấy với thấy mà không bấm | `test_BT43` |
| BT-44 | Tìm đủ lỗi, trả giá thắng | Tượng Di Lặc Xước (3 lỗi nhẹ), tìm đủ 3, thắng cả vòng | Không phạt sót. Giá thẩm định 18.890.000 đ; nhận 108% = 20.400.000 đ (làm tròn xuống bội 10.000 đ) | ĐỀ XUẤT | `test_BT44` |
| BT-45 | Nhà vá lỗi | Tượng Di Lặc Hỏng (27.000.000 đ), tìm đủ lỗi, khai thật | Chỉ vá lỗi **đã tìm ra** và khi có lời: vết lớn (trừ 40% = 10.800.000 đ) đáng vá với phí 30% giá nền = 8.100.000 đ, món lên Xước; 3 lỗi nhẹ không đáng (mỗi lỗi lấy lại 2.700.000 đ, phí 12% = 3.240.000 đ). Phí trừ vào tiền. Lỗi **sót** không vá được (tìm 0 lỗi: 0 lần vá) | **CHỐT có nhà vá lỗi**; phí 12% và 30%: ĐỀ XUẤT (6% và 15% đẩy người giỏi lên 49,9%) | `test_BT45` |
| BT-46 | Bỏ bấm một lỗi đã thấy | Món có lỗi, người chơi thấy nhưng không bấm | Người thu mua soi thấy thì trừ tiền theo lỗi và cộng phạt sót. Bỏ bấm không bao giờ có lợi hơn bấm | CHỐT (cho phép bỏ bấm; trừ tiền và cộng phạt); mức phạt 10%: ĐỀ XUẤT | `test_BT46` |
| BT-69 | Bấm lỗi: giá nhảy và không bấm trùng | Di chuột đúng chỗ lỗi, bấm; bấm lại cùng lỗi | Dòng chữ ngắn ghi lỗi chỉ hiện khi đúng chỗ lỗi (chỗ không lỗi không hiện gì); giá trừ ngay đúng một lần; bấm lại cùng lỗi không trừ thêm, không thêm bằng chứng | CHỐT (soi-loi-tra-gia, luật soi 2026-11-24) | Game |
| BT-70 | Bảng chọn thật hoặc giả | Mở bảng của món | Bảng cho chọn thật hoặc giả và hiện giá trị cùng thông tin cơ bản của món; chưa chọn thì không bán được | CHỐT (2026-11-24) | Game |
| BT-71 | Bỏ qua trả giá | Món Hiếm trở lên, người chơi bấm bán nhanh ngay | Bán giá chào mở 100% giá thẩm định (đã trừ lỗi) cộng trừ phạt sót, không vòng trả giá. "Bán nhanh giá thấp hơn" có nghĩa là không có thưởng 108% | ĐỀ XUẤT (spec mục 5: được bỏ qua; mức "thấp hơn" chưa chốt) | Game |
| BT-47 | Giáo Sư Lẫm gợi ý | Mua gợi ý cho Bình sứ 2.300.000 đ và Tượng Di Lặc 27.000.000 đ | Phí 5% giá nền, tối thiểu 200.000 đ: Bình sứ 200.000 đ (5% chỉ 115.000 đ), Tượng Di Lặc 1.350.000 đ. Gợi ý chỉ ra **một** dấu giả chưa thấy, nếu không còn thì chỉ một lỗi chưa thấy | ĐỀ XUẤT | `test_BT47` (phí); chỉ dấu: Game |
| BT-72 | Tiền phạt lớn hơn tiền mặt | Ngày 2, tiền 0 đ, khai nhầm hàng giả là thật với phạt lớn hơn tiền | Phần thiếu thành nợ tồn (không âm tiền); nợ tồn tính vào ngưỡng thua 10.000.000 đ | MỞ (hậu quả thiếu tiền BT-08 chưa chốt); mô hình làm như vậy | Game |

## (f) Cây phát triển trong ván

Spec: [ruong-suu-tap](../../docs/superpowers/specs/2026-11-23-ruong-suu-tap-design.md) (mục "Đã chốt và câu hỏi mở sau khi cây chỉ trong ván"), BangSo mục 8. Test mẫu dùng **bản JSON 15 nút, 79 điểm** (theo DaChot), không dùng `tree_run.RUN_TREE` (còn 19 nút, xem "Điểm lệch" 5).

| Mã | Tình huống | Điều kiện đầu vào | Kết quả mong đợi | Luật / nguồn | Kiểm chứng |
|---|---|---|---|---|---|
| BT-48 | Cây trống đầu mỗi ván | Bắt đầu ván mới (sau thắng, sau thua, sau tải một ván khác) | Không nút nào mở, điểm 0; điều kiện nút chỉ dựa vào ván này. Nút rẻ nhất tốn 3 điểm nên không mở được nút nào ở ngày 1 | CHỐT (cây chỉ trong ván, không tiến trình vĩnh viễn) | `test_BT48` (dữ liệu); đầu ván thật: Game |
| BT-73 | Điểm kiếm theo việc hằng ngày | Mở rương, chơi mini game buổi sáng | Điểm cộng khi hoàn thành việc; trần về lý thuyết 3 điểm mỗi ngày (21 điểm cả ván); không có điểm từ nguồn khác (không từ ván trước) | **Nguồn điểm: CHỐT** (việc hằng ngày). **Chia điểm từng việc và 3 điểm mỗi ngày: GIẢ ĐỊNH** (BangSo mục 8) | Game |
| BT-49 | Nút cha trước nút con | Cố mua nút con khi chưa mua nút cha (ví dụ M1 khi chưa có S2) | Từ chối, điểm không trừ. Mỗi nhánh có đúng một gốc; con luôn cùng nhánh với cha; không vòng | CHỐT (cha trước con); số điểm từng nút: ĐỀ XUẤT | `test_BT49` |
| BT-50 | Nhánh Đời sống trống | Mở giao diện cây | Nhánh Đời sống hiện nhưng không có nút; M3 và nhánh Câu chuyện (C1 đến C4) không còn. Cây có 15 nút, tổng 79 điểm | CHỐT (DaChot, 2026-11-25) | `test_BT50` |
| BT-51 | Không buff vượt mức đã chốt | Duyệt mọi nút | Tác động chỉ là mở rương (R2 đến R5) hoặc chọn gia truyền (M1); không nút nào cộng tiền, giá, tỉ lệ rơi hay combo. M2 không còn cộng vốn. Tổng thưởng có tác động tiền hoặc rương không quá khoảng 1.000.000 đ | CHỐT (không buff; trần khoảng 1.000.000 đ). M2 còn tác động gì: MỞ | `test_BT51` |
| BT-52 | Rương mở khóa chỉ mở trong ván | Mở nút R2, R3, R4, R5 | Rác Quý (R2), Tổng Hợp (R3), Cổ Phong (R4, cần R3), Thiên Cơ (R5, cần R3) chỉ có nhờ đúng nút đó trong ván này; ván sau lại khóa | **Luật mở: CHỐT.** Bảng rương mở khóa (giá, tỉ lệ): ĐỀ XUẤT | `test_BT52` |
| BT-53 | Cây khó | 7 ngày × 3 điểm = tối đa 21 điểm | Không mở hết 79 điểm; tối đa 6 nút theo thứ tự cha trước con; thực tế 3 đến 4 nút mỗi ván | CHỐT (cây khó); giá nút ĐỀ XUẤT | `test_BT53` |
| BT-54 | Shop xoay vòng và rương mở từ hôm sau | Có 6 rương phụ (4 mở khóa + 2 chủ đề) | Mỗi ngày bày 4 rương cơ bản (Đen từ ngày 4) và tối đa 4 rương mở khóa hoặc chủ đề; xoay vòng theo hạt giống ván và ngày (cùng hạt giống, cùng ngày: cùng danh sách; ngày khác: khác). Rương chưa tới ngày được mở thì không có trong shop | CHỐT (DaChot: Rương chủ đề, 2026-11-25) | `test_BT54` |
| BT-74 | Rương mở và chủ đề có từ ngày hôm sau | Nút mở rương (hoặc thành tựu chủ đề) hoàn thành lúc giữa ngày 3 | Rương **không** có trong shop ngày 3; có từ ngày 4. Kể cả khi thoát và tải lại giữa ngày | CHỐT ("có trong shop từ hôm sau", ruong-chu-de). Thời điểm mua nút trong ngày: MỞ | Game |
| BT-75 | Thành tựu mở rương chủ đề, chỉ trong ván | Hoàn thành thành tựu chủ đề trong ván A, bắt đầu ván B | Ván A: rương chủ đề có từ hôm sau. Ván B: rương chủ đề lại khóa (không vĩnh viễn) | CHỐT (chỉ mở trong ván); danh sách thành tựu: MỞ (Claude 1) | Game |
| BT-76 | Lưu và tải cây | Mở R1, R3 (còn 2 điểm), lưu, tải | Nút đã mở, điểm còn lại, rương đã mở khóa và ngày có hiệu lực giữ nguyên | CHỐT (nối nghiệm thu "lưu và tải") | Game |

## (g) Lưu và tải

Nối BT-09, BT-10, BT-13, BT-15 cũ. Mô hình tham chiếu **không có lưu và tải thật**, nên cả nhóm này chạy ở game (Codex). Chỉ BT-32 (bộ quay xác định) và BT-57 (đổi tiền) có test mẫu.

| Mã | Tình huống | Điều kiện đầu vào | Kết quả mong đợi | Luật / nguồn | Kiểm chứng |
|---|---|---|---|---|---|
| BT-77 | Không lặp giao dịch sau tải | Mở hộp, bán qua soi lỗi và trả giá (nhận 20.400.000 đ), lưu, tải lại, bấm bán lại | Tiền chỉ cộng một lần; món đã rời kho không bán lại được; không sinh thêm món | CHỐT (nghiệm thu Codex; nối BT-10, BT-11, BT-15) | Game |
| BT-78 | Lưu giữa lúc soi, bằng chứng giữ nguyên | Soi món, bấm 2 trên 3 lỗi, lưu, tải | Cùng 2 lỗi đã bấm (cùng thứ tự), giá đã trừ giữ nguyên, lỗi chưa bấm vẫn chưa bấm; không mất, không nhân đôi bằng chứng | CHỐT (nghiệm thu: "lưu/tải giữ nguyên bằng chứng") | Game |
| BT-79 | Lưu giữa trả giá | Trả giá xong vòng 1 (thắng, giá chào tăng lên 108%), lưu, tải | Còn 2 vòng; giá chào hiện tại và các bằng chứng đã dùng giữ nguyên; tải không cho chơi lại vòng 1 để đổi kết quả (nếu muốn chống tải-chơi-lại) | Giữ nguyên: CHỐT. Chống tải-chơi-lại: MỞ | Game |
| BT-80 | Lưu sau khi khai thật hoặc giả | Đã chọn khai giả cho một món, lưu, tải | Lựa chọn giữ nguyên (không đổi được bằng cách tải); hậu quả (phạt, tịch thu) chỉ áp một lần | CHỐT (nối BT-77) | Game |
| BT-81 | Tiền lưu theo đồng, bội 10.000 đ | Lưu ván khi tiền là 11.250.000 đ | File lưu chứa số tiền là bội số 10.000 đ (hoặc đơn vị cân bằng, thống nhất với Antigravity); tải lại ra đúng 11.250.000 đ. File có số lẻ (ví dụ 12.345 đ) bị từ chối, hoặc làm tròn đến 10.000 đ gần nhất rồi cảnh báo | Làm tròn bước 10.000 đ: CHỐT. **Từ chối hay làm tròn khi tải số lẻ: MỞ.** Mô hình tham chiếu từ chối (`currency.from_vnd`) | Game; chuyển đổi: `test_BT57` |

## (h) Tiền tệ

| Mã | Tình huống | Điều kiện đầu vào | Kết quả mong đợi | Luật / nguồn | Kiểm chứng |
|---|---|---|---|---|---|
| BT-55 | Mọi giá, phí, nợ là bội số 10.000 đ | Toàn bộ trường `*_vnd` trong JSON | Số nguyên chia hết cho 10.000. Ví dụ Gỗ 1.400.000 đ, phí gợi ý tối thiểu 200.000 đ, nợ ngày 7 22.000.000 đ | CHỐT (DaChot: Tiền tệ) | `test_BT55` |
| BT-56 | Tiền bán sau mọi phép tính | Giá thẩm định, bán giả 20%, trần 108%, phạt 10% và 20% | Kết quả mọi bước là số nguyên đơn vị 10.000 đ, không có phần lẻ nhỏ hơn 10.000 đ (làm tròn xuống như `price_round: floor`, phạt làm tròn lên) | CHỐT | `test_BT56` |
| BT-58 | Giá hộp dao động | Mọi ngày và hạt giống | Giá hộp mỗi sáng là bội số 10.000 đ, trong 80% đến 120% giá gốc (Gỗ 1.120.000 đ đến 1.680.000 đ) | CHỐT (±20%, GiaHopDaoDong) | `test_BT58` |
| BT-57 | Đổi đơn vị | 1.400.000 đ ↔ 140 đơn vị; 12.345 đ | Đổi qua lại không mất tiền với bội số 10.000 đ; số lẻ bị từ chối ở mô hình; hiển thị đúng "1.400.000 đ", "22.000.000 đ", không có chữ "xu" | CHỐT (hiển thị); xử lý số lẻ: MỞ | `test_BT57` |
| BT-82 | Hiển thị tiền trong game | Mọi màn có số tiền (cửa hàng, soi lỗi, trả giá, sổ nợ) | Định dạng "22.000.000 đ" (chấm ngăn nghìn, hậu tố đ), không còn nhãn "xu"; giao diện chật được rút gọn "22 triệu" | CHỐT (tien-te) | Game (Antigravity) |

## Điểm lệch, mâu thuẫn và luật còn thiếu

Quy tắc: hai tài liệu mâu thuẫn thì lấy DaChot và số mới nhất. Chưa quyết thay chủ dự án.

1. **Rác có hệ số tình trạng trong mô hình.** BangSo mục 1 nói rác "30.000 đ mỗi món, không giả, không giám định". Mô hình vẫn nhân tình trạng: rác Nguyên 30.000 đ, Xước 20.000 đ, **Hỏng 0 đ**. Cần chốt rác có tình trạng và soi lỗi không (nếu có, rác Hỏng không bán ra tiền; nếu không, luôn 30.000 đ). Test BT-17 chấp nhận cả hai. **Chủ: Claude 1 và chủ dự án.**
2. **Sàn hàng giả 35% của V09 không nằm trong DaChot.** Luật chỉ có ở content.json và NoiDungMau (Claude 1). Test BT-25 dựa vào đó, nhưng DaChot chưa ghi dòng riêng; chưa chốt rương nào ngoài Gỗ chứa V09 (Tổng Hợp chứa mọi món Gỗ nên có). Rương "Kim Khí không giả" và "V09 ít nhất 35% ở mọi rương" chỉ khớp vì V09 không nằm trong bộ Kim Khí; nếu sau này đưa V09 vào Kim Khí hoặc rương chủ đề, hai luật xung đột. Đã nêu ở BangSo mục 9.8.
3. **Hết giờ 2 phút chưa có hậu quả.** DaChot chốt "giới hạn 2 phút soi mỗi món" nhưng không nói hết giờ thì sao. Mô hình giả định lỗi chưa bấm là lỗi sót (phạt 10% mỗi lỗi, không trả giá được). BT-65 và BT-43 theo giả định đó, cần chủ dự án xác nhận.
4. **Thấy mà không bấm với chưa thấy.** DaChot chốt bỏ bấm lỗi đã thấy thì người thu mua trừ tiền và cộng phạt. Mô hình không phân biệt hai trường hợp: cả hai là "lỗi sót". Nếu game muốn phạt khác nhau, phải chốt số và viết ca riêng.
5. **`QA/tree_run.RUN_TREE` và `QA/test_tree.py` còn cây 19 nút, 99 điểm** (có nhánh Câu chuyện, M3 đã bỏ, M2 còn cộng vốn). Đã ghi ở BangSo mục 9.1. Test mới (BT-48 đến BT-53) dùng JSON 15 nút. `test_tree.py` vẫn chạy đạt nhưng chỉ khóa cây cũ; cần đo lại cây 15 nút với cách chia điểm theo việc hằng ngày (việc của Claude 2 phiên sau).
6. **Cách chia điểm cho việc hằng ngày chưa chốt** (BT-73): "3 điểm mỗi ngày" là giả định của mô hình. M2 sau khi bỏ cộng vốn chưa có thưởng khác; M1 cho chọn gia truyền là tác động duy nhất lên ván của nhánh Sưu tập, cần Claude 1 xác nhận không tính là "buff" (BT-51 chấp nhận M1, R2 đến R5 và mọi nút còn lại không tác động).
7. **Quy tắc khóa kết quả hộp (BT-32, BT-63).** Chỉ chốt "cùng hạt giống cho cùng kết quả". Chưa chốt kết quả mỗi hộp xác định theo (hạt giống ván, ngày, số thứ tự hộp trong ngày) hay theo trạng thái bộ sinh số. Hai cách đều qua BT-32 ở mô hình; khác nhau khi người chơi mua hộp theo thứ tự khác sau khi tải (chống tải-chơi-lại). **Chủ: Codex đề xuất, chủ dự án chốt.**
8. **Tải số tiền lẻ** (BT-57, BT-81): spec tien-te nói "làm tròn đến 10.000 đ gần nhất" cho phép tính; chưa nói về tệp lưu hỏng hoặc cũ. Mô hình từ chối; game chọn một cách và ghi vào DaChot.
9. **Cờ "lần chơi đầu"** (BT-60): trạng thái giữa các ván, trong khi DaChot chốt cây không có tiến trình vĩnh viễn. Cần chốt lưu ở đâu và khi nào xóa.
10. **Hai cách làm "dễ" lần chơi đầu chồng nhau, chưa đo gộp:** "chỉ bán Gỗ và Vải Niêm" (CHỐT) và `cheap` (ĐỀ XUẤT). BT-23 đo bản "chỉ bán Gỗ và Vải Niêm" kèm hộp sắp sẵn (không hoàn tiền); BT-24 chỉ kiểm tính toán hoàn tiền. Đã nêu ở BangSo mục 9.7.
11. **Bậc cao rương mở khóa và rương chủ đề còn ĐỀ XUẤT**, bộ sưu tập Cổ Phong, Thiên Cơ, Rác Quý chưa có (giữ chỗ). BT-29 chỉ kiểm tổng 100% với số hiện tại; đổi bảng thì đổi số trong JSON, test tự theo.
12. **Ca cũ còn MỞ ảnh hưởng ca mới:** BT-08 (thiếu tiền khi đến hạn), BT-14 (kho đầy), BT-16 (điều kiện thắng). BT-72 (phạt lớn hơn tiền mặt) phụ thuộc BT-08.
13. **Tài liệu cũ dùng "xu" và trạng thái chờ duyệt:** spec soi-loi-tra-gia còn ghi "Nợ ngày 1 chỉ 200 xu" và "Trạng thái: chờ chủ dự án duyệt"; DaChot đã chốt phần soi lỗi và đổi tiền sang đồng. Lấy DaChot; đổi nhãn là việc của Claude 1 (tien-te).
14. **Trần trả giá:** Sprint5CayPhatTrien đầu bài ghi "1,05", thân bài và mã ghi 108% (BangSo mục 9.2). Các ca dùng 108%.

## Thứ tự gợi ý cho Codex

1. Nhóm nền, luật CHỐT, ít phụ thuộc giao diện: BT-17 đến BT-23, BT-28 đến BT-35, BT-55, BT-56, BT-58.
2. Soi lỗi và trả giá (nạp số từ JSON vì phần lớn ĐỀ XUẤT): BT-36 đến BT-47, rồi các ca Game BT-65 đến BT-72.
3. Cây trong ván: BT-48 đến BT-54, BT-73 đến BT-76.
4. Lưu và tải: BT-77 đến BT-81, kèm BT-32, BT-61, BT-63, BT-67.

Chạy test mẫu: `cd QA && python3 -m unittest -v test_sprint5_cases`. Các test mô hình không gọi mô phỏng nặng (chạy dưới 1 giây).

# Thiết kế: tier đồ trong rương và danh sách món (phần 6)

Trạng thái: **chờ chủ dự án duyệt**. Chưa có gì được xây theo bản này. Mọi con số là **đề xuất khởi đầu**, chờ Claude 2 đo.

Phần 1: [ruong-suu-tap](2026-11-23-ruong-suu-tap-design.md) (4 rương, rác, sưu tập). Phần 5: [soi-loi-tra-gia](2026-11-23-soi-loi-tra-gia-design.md) (soi lỗi, trả giá).

## Ý định

Chốt rương ra **loại đồ nào** và **cụ thể là món gì**, để người chơi có cảm giác tiến bộ từ rác đến Huyền thoại và để may rủi có nhiều lớp.

## Mục tiêu

- Khung **5 bậc**: Rác, Thường, Hiếm, Rất hiếm, Huyền thoại.
- Danh sách **35 món** (mỗi bậc 6–8 món), trải đều 6 nhóm nguồn gốc.
- Bảng phân bổ bậc theo từng loại rương.

## Không phải mục tiêu

- Chưa chốt giá nền, hồ sơ soi, dấu hiệu soi và thần lực của 25 món mới.
- Chưa làm mô hình 3D (làm sau khi chuyển sang low-poly).
- Con số tỉ lệ và giá: Claude 2 chốt.

## Ràng buộc đã có

- Hiện có 10 hiện vật (V01–V10) với 3 tier thường/hiếm/rất hiếm; giữ nguyên.
- Tạo tác có 3 tier riêng (tier 1–3), **không** nằm trong 5 bậc này.
- Hàng giả là một trục khác (bán còn 20% giá), không phải bậc.
- Nhóm nguồn gốc: Quá khứ, Hiện đại, Tương lai, Ngoài hành tinh, Cổ tích, Thần thoại.

## 1. Năm bậc

| Bậc | Ý nghĩa | Giá nền tham khảo |
|---|---|---|
| Rác | Vô giá trị, có công dụng ẩn (phần 1) | 0–5 |
| Thường | Đồ cổ phổ thông, kiếm tiền đều | ~200–900 |
| Hiếm | Giá cao, dễ bị làm giả | ~1500–1900 |
| Rất hiếm | Đồ mơ ước | ~2700+ |
| Huyền thoại | Đích cuối, chỉ có khi mở khóa | Rất cao (chưa chốt) |

## 2. Danh sách 35 món

Nguyên tắc nội dung: **gần gũi, người chơi nhận ra ngay**. Pha **cân bằng hai bên**: đồ Việt Nam và truyền thuyết (VN) với đồ **nhại** phim, anime, văn hóa đại chúng (NH). Món nhại phải **gợi nhắc đủ để nhận ra nhưng không dùng tên hay hình ảnh thật** có bản quyền (giống đội "Quỷ Đỏ United" ở phần 2). Món dựa trên truyền thuyết và văn hóa dân gian Việt Nam thì dùng thẳng được.

**V** = hiện vật đã có (10 món, giữ nguyên). Các món còn lại là **25 món mới, tên chỉ là đề xuất**.

| Bậc | Món |
|---|---|
| **Rác (8)** | Giày cũ, Tất thối, Trứng thối, Xương cá (4 món bạn nêu từ đầu), Dép tổ ong cụt quai (VN), Lon nước ngọt bẹp, Áo đấu cũ của đội nhại (NH), Vé cược nhàu |
| **Thường (8)** | **V01** Bình sứ men xanh, **V02** Sách cổ bìa da, **V03** Đồng hồ quả lắc cũ, **V04** Mảnh kính đồng hồ thiên văn, **V05** Thiết bị liên lạc hỏng, **V09** Lá bùa giấy vàng, Quạt nan cũ (VN), Máy game cầm tay 8-bit (NH) |
| **Hiếm (7)** | **V06** Bộ pin lượng tử, **V08** Gương thủy tinh cổ tích, Băng cassette và máy phát (VN), Nón lá thêu hoa (VN), Cái túi thần kỳ 4 chiều (NH), Cục sạc hình chuột vàng (NH), Mũ phù thủy học việc (NH) |
| **Rất hiếm (6)** | **V07** Tinh thể lam cầm tay, **V10** Mảnh vảy long, Đàn thần (VN, truyền thuyết Thạch Sanh), Trống đồng nhỏ (VN), Kiếm sáng ba màu (NH), Quả cầu ngọc 7 sao (NH) |
| **Huyền thoại (6)** | Nỏ thần (VN, An Dương Vương), Roi sắt của vị anh hùng làng Gióng (VN), Ấn Ngọc Tỷ (VN), Chiếc nhẫn duy nhất (NH), Quyển sổ định mệnh (NH), Tấm vé số độc đắc "chưa đổi" (VN, hài) |

Trong 25 món mới có khoảng nửa VN, nửa NH. **Nhóm nguồn gốc** (Quá khứ, Hiện đại, Tương lai, Ngoài hành tinh, Cổ tích, Thần thoại) của từng món mới sẽ gán lại khi chốt danh sách, để rương Vải Niêm (ra theo bộ) vẫn có nghĩa.

## 3. Rương ra bậc nào (đề xuất)

Cột là phần trăm khối lượng ra của rương; **Kim Khí** ra tạo tác nên tách riêng.

| Rương | Rác | Thường | Hiếm | Rất hiếm | Huyền thoại |
|---|---|---|---|---|---|
| Gỗ | 20% | 70% | 10% | 0% | 0% |
| Vải Niêm (theo một bộ) | 15% | 55% | 25% | 5% | 0% |
| Đen | 30% | 20% | 25% | 25% | 0% |
| **Kim Khí** | 10% rác máy móc | — | — | — | — |

- **Kim Khí:** 90% ra tạo tác (tier 1: 60%, tier 2: 35%, tier 3: 5%), 10% rác máy móc hỏng.
- **Huyền thoại không có ở 4 rương cơ bản.** Chỉ có ở **rương mở khóa** (phần 1): Rương Tổng Hợp là rương cho Huyền thoại, đề xuất mỗi lần mở có 8–10% ra một món Huyền thoại.
- Các rương mở khóa khác (Cổ Phong, Thiên Cơ, Rác Quý) giữ như phần 1.
- Hàng giả áp dụng cho **Thường, Hiếm, Rất hiếm** (và Huyền thoại, đề xuất tỉ lệ thấp); rác không có giả.

## 4. Quan hệ với soi lỗi (phần 5)

Số lỗi cần tìm tăng theo bậc (đề xuất): Thường 1–2, Hiếm 2–3, Rất hiếm 3, Huyền thoại 3–4. Rác không soi lỗi. Món Thường có "bán nhanh", Hiếm trở lên có trả giá đầy đủ.

## 5. Dữ liệu (hướng đi)

- Mỗi món có `tier`, `origin`, `basePrice`, `fakeRate`, `profile` (hồ sơ chuẩn), `flaws[]` (các lỗi thật và nhiễu), `trinketPowers` (phần 4), `model`.
- Bảng rương ở mục 3 thành dữ liệu `drops` theo bậc thay vì liệt kê từng món, để thêm món mới không phải sửa rương.

## Câu hỏi còn mở

1. Giá nền, tỉ lệ giả, hồ sơ chuẩn và danh sách lỗi cho 25 món mới: cần viết, mỗi món một mục.
2. Tên 25 món mới có đúng ý bạn không (đặc biệt Huyền thoại)? Món nhại cần kiểm tra kỹ để **không dùng tên hay hình ảnh thật** có bản quyền.
3. Số món mỗi nhóm nguồn gốc ở mỗi bậc có cân đối cho rương Vải Niêm (theo bộ) không: cần kiểm tra.
4. Tỉ lệ ra ở mục 3 và 8–10% Huyền thoại ở Rương Tổng Hợp: **chờ Claude 2**.
5. Kim Khí có nên ra tạo tác đã có (trùng) hay chỉ ra tạo tác chưa sở hữu.
6. 25 món mới cần mô hình 3D low-poly; tổng cộng 35 hiện vật, ưu tiên theo bậc.

## Cập nhật: cách làm mô hình theo bậc (chốt)

Độ hiếm càng cao thì càng đầu tư chi tiết (ít món hơn nên đỡ tốn, và chất lượng đồng đều hơn giữa các bậc):

| Bậc | Cách làm | Mức chi tiết |
|---|---|---|
| Rác, Thường | Blender (code), miễn phí | Đơn giản, 150–750 tam giác |
| Hiếm | Blender chi tiết hơn hoặc Meshy | Làm sau |
| Rất hiếm, Huyền thoại | Meshy | Chi tiết nhất, làm sau |

Đã làm: Rác (8) và Thường (V01–V05, V09, quạt nan V11, máy game 8-bit V12) trong `Assets/_Project/Art/Items/LowPoly/Junk` và `Common`, dựng bằng `Tools/Blender/make_items_lowpoly.py`.

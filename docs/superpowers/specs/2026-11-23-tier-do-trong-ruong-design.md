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

QK = Quá khứ, HĐ = Hiện đại, TL = Tương lai, NHT = Ngoài hành tinh, CT = Cổ tích, TT = Thần thoại. **V** = hiện vật đã có.

| Bậc | Món (nhóm) |
|---|---|
| **Rác (8)** | Giày cũ, tất thối, trứng thối, xương cá, lon bia bẹp, áo đấu cũ của đội nhại, vé cược nhàu, dép tổ ong cụt quai (đều HĐ) |
| **Thường (8)** | **V01** Bình sứ men xanh (QK), **V02** Sách cổ bìa da (QK), **V03** Đồng hồ quả lắc cũ (QK), **V04** Mảnh kính đồng hồ thiên văn (HĐ), **V05** Thiết bị liên lạc hỏng (TL), **V09** Lá bùa giấy vàng (TT), Radio đèn điện tử (HĐ), Đũa phép gãy (CT) |
| **Hiếm (7)** | **V06** Bộ pin lượng tử (TL), **V08** Gương thủy tinh cổ tích (CT), Mặt nạ gỗ nghi lễ (QK), Máy ảnh phim đời đầu (HĐ), Nam châm lạ (NHT), Chuông đồng tự kêu (TT), Kính VR thế hệ cuối (TL) |
| **Rất hiếm (6)** | **V07** Tinh thể lam cầm tay (NHT), **V10** Mảnh vảy long (TT), Kiếm gỉ hiệp sĩ (QK), Hạt giống cây thần (CT), Bộ não nhân tạo hỏng (TL), Bản đồ sao (NHT) |
| **Huyền thoại (6)** | Ấn Ngọc Tỷ (QK), Trái tim robot cuối cùng (TL), Quả táo vàng (TT), Giày thủy tinh nguyên bản (CT), Phi thuyền mini của phi hành gia (NHT), Tấm vé số độc đắc "chưa đổi" (HĐ) |

Tên chỉ là gợi ý. Mỗi nhóm nguồn gốc cần ít nhất vài món ở mỗi bậc để rương Vải Niêm (ra theo bộ) và manh mối nhóm nguồn gốc vẫn có nghĩa.

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
2. Tên 25 món mới có đúng ý bạn không (đặc biệt Huyền thoại)? Bạn có muốn thêm hoặc đổi món nào?
3. Số món mỗi nhóm nguồn gốc ở mỗi bậc có cân đối cho rương Vải Niêm (theo bộ) không: cần kiểm tra.
4. Tỉ lệ ra ở mục 3 và 8–10% Huyền thoại ở Rương Tổng Hợp: **chờ Claude 2**.
5. Kim Khí có nên ra tạo tác đã có (trùng) hay chỉ ra tạo tác chưa sở hữu.
6. 25 món mới cần mô hình 3D low-poly; tổng cộng 35 hiện vật, ưu tiên theo bậc.

# Tiền tệ: đồng Việt Nam

**Quyết định của chủ dự án (2026-11-24):** tiền trong game là **đồng Việt Nam (đ)**, thay cho "xu". Quy đổi: **1 xu cũ = 10.000 đ**. Mọi tỉ lệ cân bằng (hệ số giá, phần trăm lời, phần trăm phạt) giữ nguyên, chỉ đổi đơn vị.

## Bảng quy đổi các mốc chính

| Mốc | Xu cũ | Đồng |
| --- | --- | --- |
| Vốn đầu ván | 1.000 | **10.000.000 đ** |
| Nợ ngày 1 đến 7 | 200 / 350 / 500 / 750 / 1.050 / 1.500 / 2.200 | **2.000.000 / 3.500.000 / 5.000.000 / 7.500.000 / 10.500.000 / 15.000.000 / 22.000.000 đ** |
| Tổng nợ 7 kỳ | 6.550 | **65.500.000 đ** |
| Ngưỡng thua (nợ tồn vượt) | 1.000 | **10.000.000 đ** |
| Giá hộp H1 / H2 / H3 / Rương Đen | 100 / 350 / 800 / 1.500 | **1.000.000 / 3.500.000 / 8.000.000 / 15.000.000 đ** |
| Lời mách của Dì Mận | 15 | **150.000 đ** |
| Phí giám định tối thiểu | 20 | **200.000 đ** |
| Tiền cược sòng bạc | 100 / 250 / 500 | **1.000.000 / 2.500.000 / 5.000.000 đ** |
| Ngưỡng đăng đấu giá | 1.300 | **13.000.000 đ** |

Công thức: `số đồng = số xu cũ x 10.000`. Mọi phần trăm (phí giám định 15% giá nền, phạt nợ 20%, phí đăng ký đấu giá 5%, cầm 60%, chuộc +15%) không đổi.

## Quy tắc hiển thị và làm tròn

- Hiển thị đầy đủ theo kiểu Việt Nam: dấu chấm ngăn hàng nghìn, hậu tố "đ", ví dụ **22.000.000 đ**. Giao diện chật có thể rút gọn thành "22 triệu" hoặc "22tr".
- **Bước làm tròn nhỏ nhất là 10.000 đ** (bằng 1 xu cũ). Mọi giá, phí, tiền bán đều là bội số của 10.000 đ, nên các phép "làm tròn đến xu gần nhất" cũ thành "làm tròn đến 10.000 đ gần nhất". Không phát sinh số lẻ nhỏ hơn 10.000 đ.
- Tiền trong dữ liệu và mô phỏng có thể lưu theo **đơn vị 10.000 đ** (số nguyên) để khỏi đổi logic làm tròn. Lớp hiển thị nhân 10.000 khi in ra.

## Ai làm gì

| Việc | Chủ |
| --- | --- |
| Đổi nhãn "xu" thành "đ" và số tiền trong `Docs/Content`, GDD, `content.json` | Claude 1 |
| Định dạng tiền trong lõi gameplay và lưu ván (đơn vị 10.000 đ hay đồng thô, thống nhất với Antigravity) | Codex |
| Hiển thị tiền trong giao diện, chữ chạy, sổ tay | Antigravity |
| Đơn vị trong mô phỏng, tài liệu cân bằng và kiểm thử | Claude 2 (đã làm: `QA/currency.py`, mục "Tiền tệ" trong `Docs/Balance/README.md`) |

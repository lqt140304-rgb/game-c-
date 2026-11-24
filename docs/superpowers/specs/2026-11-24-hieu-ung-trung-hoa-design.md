# Hiệu ứng và âm thanh rương Trung Hoa cổ (Chest_TrungHoa)

Ngày 2026-11-24. Trạng thái: đã viết code + tạo âm, CHƯA chạy trong Unity, CHƯA nghe.

## Ý tưởng
Đỉnh đồng ba chân rỉ xanh = "lư hương đền cổ gặp hội Tết". Khói hương bốc lên từ miệng đỉnh trong lúc
quay; mỗi nhịp đổi màu là một tiếng mõ gỗ kèm một chớp pháo tép đỏ nhỏ; vòng quay kiểu **PingPong** như
tiếng trống hội dồn lên rồi xuống. Chốt bậc = chiêng/trống, Rác là bàn tính lạch cạch tính sai + dây đàn
tranh lạc giọng. Rất hiếm/Huyền thoại thả đèn lồng đỏ bay lên quanh đỉnh. Khác rương Golden (đồng xu, Cycle,
Fountain) ở mọi trục: SoftDot khói chậm, Up, PingPong, tia dài quay rất chậm (8°/s).

Lõi chung (nhảy màu rồi dừng đúng bậc, bóng vật phẩm to dần) giữ nguyên, không sửa.

## Nhịp (ước tính, theo lõi)
| Giây | Hình | Tiếng |
|---|---|---|
| 0 | bắt đầu quay; khói hương (16 hạt/s, sống 2,6 s) | sfx_trunghoa_ambient (chuông đồng ngân, 4 s) |
| 0–3,2 | màu PingPong, khoảng cách 0,06 → 0,5 s; mỗi nhịp 3 tia pháo tép đỏ-vàng | mõ gỗ, pitch 0,8 → 1,3 |
| ~3,2 | chốt bậc: chớp đèn 6,5, tia dài x1,3 trong 1,6 s; pháo nổ 25–75 tia (trừ Rác) | lock theo bậc |
| +0 (≥ Rất hiếm) | 8 (Huyền thoại 16) đèn lồng đỏ bay lên chậm 3,5 s | |
| sau chốt | bóng vật phẩm phóng to | whoosh khói + phong linh |
| sau chốt | | 0 + 1/bậc âm đệm (pháo tép, chuông) |
| bước 2 / 4 BoxOpenSequence | | nắp đồng kéo / nắp đồng rơi |

## File
- `Assets/_Project/Scripts/ChestFx/Chests/TrungHoaFxPreset.cs` – `TrungHoaFxPreset.Apply(p)` + tên file âm.
- `Assets/_Project/Scripts/ChestFx/Chests/TrungHoaHook.cs` – pháo tép mỗi nhịp, pháo khi chốt, đèn lồng.
- `Assets/_Project/Editor/TrungHoaFxSetup.cs` – menu tạo hồ sơ, menu gắn hook vào vật đang chọn.
- `Assets/_Project/Audio/Chests/trung_hoa/` – 12 mp3 + README (bảng mô tả).
- `Tools/CompileCheck/check_trung_hoa.sh` – biên dịch thử bằng stub.
- `Assets/_Project/Art/Chests/Chest_TrungHoa.glb` – chép từ nhánh claude/chest-trung-hoa (con trỏ LFS).

## Cách thử trong Unity
1. Tools/Cho Do La/Create Trung Hoa Chest FX Profile → tạo `ScriptableObjects/ChestFx/ChestFx_TrungHoa.asset`.
2. Tools/Cho Do La/Add Chest Reveal Test (cảnh thử của lõi), thêm ChestFx_TrungHoa vào danh sách `profiles`
   của ChestRevealDemo (menu lõi chỉ điền Golden), bấm C tới Chest_TrungHoa.
3. Chọn object chứa ChestRevealSequence, Tools/Cho Do La/Add Trung Hoa Hook To Selected.
4. Play: T đổi bậc, Space mở, R đóng. Nghe từng bậc, nhìn khói/pháo/đèn lồng; chỉnh tham số trong asset.

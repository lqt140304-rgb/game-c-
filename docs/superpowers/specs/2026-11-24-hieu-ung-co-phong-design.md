# Thiết kế: hiệu ứng và âm thanh rương Cổ Phong (`Chest_CoPhong`)

Trạng thái: **đề xuất, đã dựng mã, chưa chạy trong Unity, âm chưa ai nghe**. Không đổi điểm chung đã chốt (nhảy màu bậc rồi dừng đúng bậc trúng; bóng vật phẩm to dần tới hình hoàn chỉnh): lõi `ChestRevealSequence` lo phần đó, file ở đây chỉ thêm bản sắc. Lõi: [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md).

## Ý tưởng: "tĩnh, mộc, thư pháp"

Hũ gốm men ngọc, nhãn thư pháp, dấu son, phong giấy đỏ. Khác hẳn rương Trung Hoa cổ (đồng, chuông, pháo hoa): **không ồn, không lấp lánh**, như một nét bút chậm.

- **Hạt**: `Streak` (nét bút) bay `Swirl` chậm quanh hũ như khói hương; thưa (14 hạt/s), sống 2,2 s; 30% màu son `#C72919` pha vào màu bậc.
- **Nhảy màu**: `PingPong` (màu lên rồi xuống như ngòi bút nhấn rồi nhấc), khởi đầu 0,075 s (chậm hơn mặc định 0,05), kết thúc 0,5 s, ít "suýt trượt" (25%), tối thiểu 3,0 s.
- **Tia**: ngắn (×0,85), gần như đứng yên (6°/s), tồn tại lâu (1,6 s); đèn chớp nhẹ (4).
- **Hook `CoPhongHook`** (gắn qua prefab `customFxPrefab`):
  - lúc quay: một **sợi khói hương** mảnh bốc thẳng từ miệng hũ;
  - lúc khoá: **vết mực loang** đen trên mặt đất dưới hũ (viền màu bậc), loang nhanh rồi chậm như mực thấm giấy, to theo bậc, giữ rồi lặn sau 4 s;
  - Rác: **mực đổ** – vài giọt to văng lên rồi rơi (hài);
  - Hiếm trở lên: **dấu son** vuông đỏ "đóng" xuống cạnh bóng vật phẩm (to → ấn → nảy về), to hơn ở Rất hiếm/Huyền thoại, ở lại tới khi reset.

## Nhịp giây (rương không chìa, thời gian giai đoạn mặc định)

| Giây | Việc |
|---|---|
| 0,9–2,5 | Gỡ niêm: tiếng **xé phong giấy đỏ** |
| 3,7 | Hé nắp: **nắp gốm cạ miệng hũ**, nền **khói hương + gió trúc + giọt nước**; quay màu PingPong, tick **gõ đũa tre**; sợi khói hương bốc lên |
| 3,7–6,7 | Màu "thở" lên xuống, chậm dần (tối thiểu 3,0 s, lõi kéo dài giữ "Hé nắp") |
| ~6,7 | Khoá: tiếng chốt theo bậc (đàn cổ cầm / chuông chùa), mực loang, tia chậm; **nét bút quét** khi bóng bật ra; Rác văng mực |
| ~7,6 | Hiếm+: dấu son đóng xuống |
| 6,7–8 | Âm đệm rải rác (nét bút, giọt nước, gõ ống tre): 1 + 1/bậc |
| ~7,9 | Mở toang: **nắp gốm đặt xuống** |
| ~10,7 | Mực lặn hết |

## Âm thanh

Chi tiết mô tả: [Audio/Chests/co_phong/README.md](../../../Assets/_Project/Audio/Chests/co_phong/README.md). Tick đũa tre; chốt: Rác = lọ mực đổ + dây đàn tụt giọng, Thường = 1 nốt cổ cầm, Hiếm = 2 nốt đi lên, Rất hiếm = rải 4 nốt + sáo trúc, Huyền thoại = chuông chùa xa + gió trúc + vuốt dây (5 s). Thiếu file nào, menu dùng bộ chung ở `Audio/Chests/`.

## File

- `Assets/_Project/Scripts/ChestFx/Chests/CoPhongFxPreset.cs` – giá trị hồ sơ + tên file âm.
- `Assets/_Project/Scripts/ChestFx/Chests/CoPhongHook.cs` – khói hương, mực loang, mực đổ, dấu son.
- `Assets/_Project/Editor/CoPhongFxSetup.cs` – menu `Tools/Cho Do La/Create Co Phong Chest FX Profile` → `ScriptableObjects/ChestFx/ChestFx_CoPhong.asset` + `ChestFx_CoPhongHook.prefab`.
- `Assets/_Project/Art/Chests/Chest_CoPhong.glb` – chép từ nhánh `claude/chest-co-phong`.
- `Tools/CompileCheck/check_co_phong.sh` – biên dịch thử với stub (vá tạm bản sao stub, không sửa stub chung).

## Cách thử trong Unity

1. Chạy menu `Tools/Cho Do La/Create Co Phong Chest FX Profile`.
2. `Tools/Cho Do La/Add Chest Reveal Test`, phím C tới `Chest_CoPhong` (hoặc kéo `ChestFx_CoPhong` vào `profile` của `ChestRevealSequence` trên gốc rương).
3. T đổi bậc, Space mở, R đóng: thử đủ 5 bậc, nhất là Rác (mực đổ) và Huyền thoại (dấu son lớn, chuông).
4. Kiểm: vết mực có nằm sát đáy hũ (gốc transform có thể không ở đáy), dấu son có che bóng vật phẩm không, tick có quá dày ở đầu vòng quay.

## Chưa kiểm chứng

Chưa chạy trong Unity; chỉ biên dịch với stub. Chưa nghe âm. Hạt, mực, dấu son chưa thấy tận mắt. Vết mực đặt tại `transform.position` của rương (giả định là đáy). Vật liệu Unlit đục nên mực "co lại" thay vì mờ dần.

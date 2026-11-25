# Hiệu ứng & âm thanh rương Việt Nam (Chest_VietNam)

Trạng thái: đã dựng code + âm, **chưa chạy trong Unity, chưa nghe âm**.

## Ý tưởng
Thúng tre đội nón lá, chim Lạc trống đồng → "đêm hội làng ven đồng lúa":
- **Đom đóm** (SoftDot, Swirl, chậm, sống 2,2 s) lượn quanh vành nón trong lúc quay; màu phụ hồng cánh sen.
- **Nhảy màu PingPong**: lên xuống như nhịp trống hội, khác kiểu Cycle (máy xèng) của rương Vàng.
- **Tick mõ + phách tre** (chọn ngẫu nhiên mỗi lần), pitch 0,85 → 1,3.
- **Tia** ngắn, quay rất chậm (8°/s) như mặt trời giữa mặt trống đồng.
- **Hook `VietNamHook`**: khi chốt, *sóng trống đồng* — 1 + bậc vòng sáng đồng tâm lan ra (cách 0,12 s, mỗi vòng 1,1 s, xen màu bậc/đồng thau); từ Hiếm *mưa hạt lúa* vàng (Streak, Fountain, rơi lại); từ Rất hiếm thêm *cánh sen* hồng bay lững lờ (Huyền thoại gấp đôi).

## Nhịp (giây, tính từ lúc vòng quay bắt đầu)
| t | Sự kiện |
|---|---|
| 0 | nền đồng lúa (dế, ếch, sáo xa) + đom đóm + tick mõ/phách 0,05 s/lần |
| ~2,5–3,2 | tick giãn tới 0,5 s, có thể trượt quá 1 bậc (40 %) |
| ≥3,2 | chốt: âm theo bậc + vòng trống đồng + hạt lúa/cánh sen |
| +0,12×n | các vòng trống đồng tiếp theo |
| sau chốt | bóng vật phẩm to dần + tiếng gió lá tre trượt đàn bầu; chiêng nhỏ/đàn bầu đệm (0 ở Rác, +1 mỗi bậc) |

## Bảng âm theo bậc
Rác: chó sủa xa + xe máy khục khặc (hài) · Thường: một nốt đàn bầu · Hiếm: câu sáo trúc ngũ cung · Rất hiếm: trống đồng + chiêng ngân + đàn T'rưng · Huyền thoại: trống hội dồn, tù và, chiêng lớn, dân reo.
Chi tiết mô tả: `Assets/_Project/Audio/Chests/viet_nam/README.md`.

## File
- `Assets/_Project/Scripts/ChestFx/Chests/VietNamFxPreset.cs`, `VietNamHook.cs`
- `Assets/_Project/Editor/VietNamFxSetup.cs` (menu *Tools/Cho Do La/Create Viet Nam Chest FX Profile*: tạo `ChestFx_VietNam.asset` + `VietNamFx.prefab` có hook, gắn vào `customFxPrefab`)
- `Tools/CompileCheck/check_viet_nam.sh`

## Cách thử trong Unity
1. `git lfs pull`; mở dự án, chờ import GLB + mp3.
2. Chạy menu *Create Viet Nam Chest FX Profile* → kiểm asset có đủ clip, `customFxPrefab = VietNamFx`.
3. *Add Chest Reveal Test*, chọn rig, đặt `profile` = ChestFx_VietNam (và nạp vào danh sách profiles của ChestRevealDemo), bấm C tới Chest_VietNam.
4. Play, T đổi bậc, Space mở: nghe tick có trễ không, các vòng có nằm ngang trên vành nón không (nếu đứng dọc/chìm thì chỉnh trục trong `VietNamHook.Ring`), hạt lúa có rơi đúng hướng.

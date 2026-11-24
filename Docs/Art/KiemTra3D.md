# Kiểm tra hình học và màu sắc mô hình 3D

Ngày 2026-11-24. Đã kiểm 110 file `.glb` trong `Assets/_Project/Art/Items/` bằng Blender 5.3 qua `Tools/Blender/audit_items.py`, kèm render Cycles từng món. Bảng ảnh ở `Docs/Art/KiemTra3D/sheet_*.png`.

Chạy lại (cần `git lfs pull` và `pip install bpy`):

```
python3 Tools/Blender/audit_items.py Assets/_Project/Art/Items <thư_mục_kết_quả>
```

Script ghi `report.json` (số tam giác, kích thước, cạnh hở, mặt suy biến, đỉnh rời, pháp tuyến, UV, vật liệu, thống kê màu texture) và một ảnh render cho mỗi file.

## Kết luận nhanh

| Bộ | Hình học | Màu sắc | Kết luận |
|---|---|---|---|
| LowPoly dùng chính (Junk, Common, Rare, VeryRare, Legendary, Artifacts) | Đúng kích thước thật, đáy y=0, tâm ở gốc. Có lỗ hở nhỏ và vài cạnh lật pháp tuyến (bình thường với Meshy) | Đúng màu tham chiếu, không thiếu texture, không vùng đen hay magenta | **Dùng được**, cần tối ưu 8 file nặng |
| LowPoly bản Blender (`*_Blender`) và `Boxes` | Sạch, 170–1750 tam giác | Màu phẳng, không texture | Đạt. Bản `_Blender` chỉ để so sánh |
| Bản photoreal (`Artifacts/`, `Boxes/`, `Heirlooms/`, `V01–V10`) | 60–870 nghìn tam giác | Texture 4096 | **Không dùng để build**, đúng như BanGiao3D đã ghi |

## Lỗi cần sửa

1. **8 file LowPoly nặng 37–45 MB** vì nhúng texture 4096×4096: L01–L06, X05, X06. Bốn trong số này còn quá ngân sách tam giác: L05 nỏ thần (67 nghìn), X05 trái tim robot (51 nghìn), X06 phi thuyền (25 nghìn), L01 ấn ngọc tỷ và L06 gậy sắt (24 nghìn). Nên decimate xuống dưới 10 nghìn và hạ texture xuống 1024.
2. **Texture atlas bị lặp trong từng file.** Mỗi nhóm được cắt từ một ảnh gộp, nên mọi món trong nhóm nhúng nguyên tấm atlas chung (ví dụ 8 món Legendary L07–L14 dùng chung một ảnh), dù mỗi món chỉ dùng 4–45% diện tích. Ví dụ L10 mũ đỏ và L20 băng video chỉ dùng khoảng 4%. Cách sửa: trong Unity, cho các món cùng nhóm dùng chung một texture và material (tiết kiệm nhất), hoặc bake lại texture riêng cho từng món.
3. **L21 kiếm Kusanagi nằm chéo trong mặt phẳng XZ**, khác L07 và L17 dựng thẳng theo trục Y. Khi cầm bằng `HeldItemViewmodel` sẽ bị lệch hướng. Cần xoay cho thẳng trục.
4. **Lỗ hở và pháp tuyến lật** ở bản Meshy, nặng nhất ở X05 (209 cạnh biên, 7 cạnh lật), L05 (112, 4), CP1 (90, 4), TC2 (85, 6), V02 sách cổ (91, 8) và R08 bao thuốc (87, 5). Lỗ thường nằm ở đáy hoặc mặt khuất nên không thấy khi render. Nếu bật backface culling mà thấy lỗ khi xoay hiện vật, chạy *Recalculate Outside* và *Fill Holes* trong Blender.

## Không phải lỗi (đã kiểm lại)

- **L16 nhẫn** ra ảnh trống ở lần render đầu, do camera gần hơn mặt cắt gần của Blender (nhẫn chỉ 2,5 cm). Mô hình vẫn đủ 1.686 đỉnh. Trong Unity cần chỉnh near clip của camera cầm đồ.
- **V04** trông như tấm kính có chân đế, khớp mô tả "mảnh kính đồng hồ thiên văn".
- Các vật liệu `ink`, `interior`, `keyhole`, `sole` gần đen trong bản Blender là chủ ý (mực, lòng hộp, lỗ khóa, đế giày).
- Không file nào có mặt suy biến, đỉnh rời, thiếu UV hay texture hỏng.

## Màu sắc theo bậc

Các bậc được phân biệt rõ qua độ bão hòa trung bình của texture: Rác 0,20–0,39 (bạc, cũ), Thường 0,31–0,49, Hiếm 0,26, Rất hiếm 0,38, Huyền thoại 0,18–0,51. Bậc Hiếm (0,26) trầm hơn bậc Thường. Nếu muốn người chơi nhận bậc qua màu thì nên nâng màu bậc Hiếm, hoặc dựa vào viền hay ánh sáng bậc trong UI.

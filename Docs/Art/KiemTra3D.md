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

1. *(Đã sửa, xem cuối file)* **8 file LowPoly nặng 37–45 MB** vì nhúng texture 4096×4096: L01–L06, X05, X06. Bốn trong số này còn quá ngân sách tam giác: L05 nỏ thần (67 nghìn), X05 trái tim robot (51 nghìn), X06 phi thuyền (25 nghìn), L01 ấn ngọc tỷ và L06 gậy sắt (24 nghìn). Nên decimate xuống dưới 10 nghìn và hạ texture xuống 1024.
2. **Texture atlas bị lặp trong từng file.** Mỗi nhóm được cắt từ một ảnh gộp, nên mọi món trong nhóm nhúng nguyên tấm atlas chung (ví dụ 8 món Legendary L07–L14 dùng chung một ảnh), dù mỗi món chỉ dùng 4–45% diện tích. Ví dụ L10 mũ đỏ và L20 băng video chỉ dùng khoảng 4%. Cách sửa: trong Unity, cho các món cùng nhóm dùng chung một texture và material (tiết kiệm nhất), hoặc bake lại texture riêng cho từng món.
3. *(Đã sửa)* **L21 kiếm Kusanagi nằm chéo trong mặt phẳng XZ**, khác L07 và L17 dựng thẳng theo trục Y. Khi cầm bằng `HeldItemViewmodel` sẽ bị lệch hướng. Cần xoay cho thẳng trục.
4. *(Đã sửa, xem mục Trám lỗ)* **Lỗ hở và pháp tuyến lật** ở bản Meshy, nặng nhất ở X05 (209 cạnh biên, 7 cạnh lật), L05 (112, 4), CP1 (90, 4), TC2 (85, 6), V02 sách cổ (91, 8) và R08 bao thuốc (87, 5). Lỗ thường nằm ở đáy hoặc mặt khuất nên không thấy khi render. Nếu bật backface culling mà thấy lỗ khi xoay hiện vật, chạy *Recalculate Outside* và *Fill Holes* trong Blender.

## Không phải lỗi (đã kiểm lại)

- **L16 nhẫn** ra ảnh trống ở lần render đầu, do camera gần hơn mặt cắt gần của Blender (nhẫn chỉ 2,5 cm). Mô hình vẫn đủ 1.686 đỉnh. Trong Unity cần chỉnh near clip của camera cầm đồ.
- **V04** trông như tấm kính có chân đế, khớp mô tả "mảnh kính đồng hồ thiên văn".
- Các vật liệu `ink`, `interior`, `keyhole`, `sole` gần đen trong bản Blender là chủ ý (mực, lòng hộp, lỗ khóa, đế giày).
- Không file nào có mặt suy biến, đỉnh rời, thiếu UV hay texture hỏng.

## Màu sắc theo bậc

Các bậc được phân biệt rõ qua độ bão hòa trung bình của texture: Rác 0,20–0,39 (bạc, cũ), Thường 0,31–0,49, Hiếm 0,26, Rất hiếm 0,38, Huyền thoại 0,18–0,51. Bậc Hiếm (0,26) trầm hơn bậc Thường. Nếu muốn người chơi nhận bậc qua màu thì nên nâng màu bậc Hiếm, hoặc dựa vào viền hay ánh sáng bậc trong UI.

## Đã sửa (2026-11-24)

Chạy `Tools/Blender/fix_lowpoly_meshy.py` trên 61 file LowPoly tạo bằng Meshy (Artifacts, Boxes_Meshy, Common, Junk, Legendary, Rare, VeryRare). Không động tới bản `_Blender`, `LowPoly/Boxes` và bản photoreal.

- Giảm tam giác xuống ≤ 9.000 cho 15 file vượt 10.000 (L05 từ 67 nghìn, X05 từ 51 nghìn, X06, L01, L06, L04, V04, V11, R01, X02, L13, V10, CP1…).
- Hạ texture xuống 1024×1024, lưu JPEG chất lượng 90. Mỗi file còn 0,2–1 MB, cả thư mục `LowPoly/` từ khoảng 400 MB còn 26 MB.
- Hàn đỉnh trùng, tính lại pháp tuyến hướng ra ngoài (còn 1 cạnh không nhất quán ở CP1 và X06, so với 1–8 cạnh ở hầu hết file trước đó), làm mịn theo góc 35°.
- L21 Kusanagi dựng thẳng theo trục Y, lưỡi hướng lên.
- Giữ đáy y=0, tâm ở gốc và kích thước thật.

Lỗ hở lớn (miệng bình, khe giữa các chi tiết Meshy) **không** tự trám, vì trám tự động dễ sinh mặt xấu. Các lỗ này nằm ở mặt khuất và không thấy khi render. Ảnh sau khi sửa: `KiemTra3D/sheet_sau_khi_sua.png`.

## Trám lỗ (2026-11-24)

Chạy `Tools/Blender/fill_holes_meshy.py` hai lượt trên 60 file LowPoly Meshy. **Không** chạy cho `Boxes_Meshy/H1`, vì miệng thân hộp hở là chủ ý (nắp tách rời để mở). Phân tích trước khi sửa cho thấy phần lớn "lỗ" là khe nứt và tam giác rác của Meshy, không phải lỗ tròn:

1. **Hàn khe:** gộp các đỉnh nằm trên biên cách nhau ≤ 0,4% kích thước vật (tối đa 1,5 mm), rồi khâu T-junction (đỉnh nằm sát giữa cạnh bên kia, ≤ 1,2% kích thước, tối đa 2 mm) bằng cách tách cạnh tại điểm chiếu và hàn.
2. **Xóa vây:** đảo tối đa 4 mặt, nhỏ hơn 0,2% diện tích, cắm vào cạnh có hơn 2 mặt.
3. **Trám vòng kín còn lại:** tam giác hóa đẹp, UV của mặt mới lấy trung bình từ đỉnh lân cận để màu khớp xung quanh.
4. **Pháp tuyến:** giữ nguyên hướng các mặt gốc, chỉ lật mặt mới cho cùng chiều với mặt kề. Cách tính lại toàn lưới đã thử và làm lật 276 cạnh ở X02, nên đã bỏ.

| | Trước | Sau |
|---|---|---|
| Cạnh biên (hở), tổng 60 file | 1.762 | 58 |
| File kín hoàn toàn | 2 | 52 |
| Cạnh pháp tuyến không nhất quán | 2 | 6 (đều ở X02) |

Kích thước giữ nguyên (sai lệch < 2 mm), số tam giác giảm không quá 3%. Ảnh trước và sau (trái và phải mỗi cặp): `KiemTra3D/so_sanh_tram_lo.png`.

**Còn hở**, cố ý không ép thêm vì lượt sau bắt đầu xóa vào hình thật: X02 trống đồng 17 cạnh, TC2 9, V04 8, CP2 7, TC1 5, V06 5, X05 5, CP1 2. Đây là các khe nhỏ ở mặt khuất; nếu thấy lỗ trong Unity, sửa tay trong Blender tại đúng chỗ.

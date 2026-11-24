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
2. *(Đã sửa, xem mục Loang màu)* **Texture atlas bị lặp trong từng file.** Mỗi nhóm được cắt từ một ảnh gộp, nên mọi món trong nhóm nhúng nguyên tấm atlas chung (ví dụ 8 món Legendary L07–L14 dùng chung một ảnh), dù mỗi món chỉ dùng 4–45% diện tích. Ví dụ L10 mũ đỏ và L20 băng video chỉ dùng khoảng 4%. Cách sửa: trong Unity, cho các món cùng nhóm dùng chung một texture và material (tiết kiệm nhất), hoặc bake lại texture riêng cho từng món.
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

## Loang màu (2026-11-24)

Người chơi soi hiện vật để tìm lỗi thật/giả, nên vệt màu lạ trên mô hình dễ bị hiểu nhầm là lỗi của món đồ. Đã kiểm và sửa cả 60 file LowPoly Meshy (`Artifacts`, `Common`, `Junk`, `Legendary`, `Rare`, `VeryRare`). Không đụng `*_Blender`, `LowPoly/Boxes`, `Boxes_Meshy`.

### Nguyên nhân tìm được

1. **Atlas Meshy không có khoảng đệm.** Mỗi nhóm dùng chung một ảnh gộp, cắt thành hàng trăm mảnh UV nhỏ xếp sát nhau. Khi hạ xuống 1024 và GPU lọc song tuyến hoặc mipmap, mỗi mảnh hút màu của mảnh bên cạnh (thường là của món khác). Đây là nguyên nhân chính, ví dụ các vệt đỏ sẫm dọc seam trên đỉnh mũ L10, các chấm xám xanh quanh dây đỏ CP1.
2. **Mặt trám lỗ có UV trung bình:** 228 mặt có UV vắt qua khe giữa các mảnh, lấy màu cả một dải atlas (L10 có 4 mặt phủ 19% atlas thay vì 3%). R03 có nhiều nhất (61).
3. **Decimate:** không thấy kéo UV qua seam. Mặt UV vắt đều là mặt trám lỗ. Decimate có để lại 385 mặt trong các mảnh UV tí hon (dưới 6 texel gốc), sau khi bake sẽ không trúng tâm texel nào và ăn màu đệm.
4. **Texture gốc Meshy vẽ sai** so với ảnh tham chiếu `Assets/_Project/Art/Source/ref_*.png`: L10 có mảng vàng ở hông mũ, đáy có hình xoáy xanh; L16 có mảng đen ở vòng trong; L20 có mảng hồng ở cạnh; L22 có mảng trắng… Bake lại không sửa được loại này, phải tô lại.

### Cách sửa

Chạy từng file một (máy dễ hết bộ nhớ). Bản texture gốc 2048/4096 lấy từ commit `cce9f69`:

```
git show cce9f69:<path> | git lfs smudge -- <path> > goc.glb
python3 Tools/Blender/rebake_items_meshy.py <file.glb> goc.glb [bao_cao.json]
python3 Tools/Blender/repaint_items_meshy.py <file.glb>        # chỉ 11 món có quy tắc tô lại
python3 Tools/Blender/audit_color_bleed.py audit <file.glb>
python3 Tools/Blender/audit_color_bleed.py compare <truoc.glb> <sau.glb> <anh.jpg>
```

- `rebake_items_meshy.py`: **bake lại texture riêng cho từng món.**
  - Sửa UV mặt trám lỗ: chép UV của mặt kề để màu khớp mặt kề.
  - Gộp các mảnh UV tí hon vào mảnh bên cạnh có màu gần nhất.
  - Chỉ giữ các mảnh UV của món đó, xếp lại với khoảng đệm 4–16 px ở 1024. Khoảng đệm lớn nhất mà các mảnh vẫn phủ được ít nhất 40% ảnh.
  - Bake màu từ texture gốc ở 2048 (Cycles, EMIT, margin 32 px kiểu EXTEND để có phần đệm), rồi thu nhỏ 2×2 xuống 1024 và lưu JPEG 92.
  - Khi xếp UV, bản sao hàn các đỉnh trùng hoàn toàn để mảnh UV đúng là mảnh thật của Meshy. Lưới thật không bị hàn, nên **đỉnh, tam giác, pháp tuyến, kích thước, gốc tọa độ giữ nguyên tuyệt đối**: đã đối chiếu từng tam giác, pháp tuyến lệch ≤ 0,0005.
  - L10 có thêm bước tô lại trong script này: thân mũ đỏ đều, đáy đỏ trơn.
- `repaint_items_meshy.py`: **tô lại các vùng Meshy vẽ sai, theo ảnh tham chiếu**, chỉ đổi texture. Quy tắc riêng từng món, chọn vùng theo pháp tuyến, độ cao hoặc màu:

| Món | Tô lại |
|---|---|
| L01 ngọc tỷ | Rồng vàng đều; mảng nâu dưới đáy đế thành đá xám |
| L02 táo vàng | Vệt hồng trên đỉnh; táo và lá vàng đều |
| L04 vé số | Vẽ lại mặt trước theo ref: sao trong vòng tròn, băng rôn, "16 24 33 49 56", "00 888 007 ++", mã vạch; trải theo độ dài cung vì giấy cong. Mặt sau là giấy trơn |
| L08 sổ tử thần | Vệt vàng ở mép thành giấy trắng hoặc bìa đen |
| L10 mũ đỏ | Mảng vàng, nâu, xanh thành đỏ thân mũ; đáy đỏ trơn |
| L12 cúp vàng | Vàng toàn bộ; hai vòng xanh ở chân đế dựng lại theo độ cao |
| L13 Pharaoh | Khăn trùm đầu vẽ lại sọc ngang vàng/xanh đều; giữ mặt, rắn trên trán, cổ áo |
| L16 nhẫn | Vòng trong cùng màu nhẫn, bỏ mảng đen |
| L18 mũ đen | Đen bóng, bỏ vệt xám; giữ kính đỏ sẫm và chi tiết bạc |
| L19 găng tay | Phủ kim sa bạc (lưới lục giác); vật liệu metallic 0,6, roughness 0,25 |
| L20 băng video | Viền cạnh xung quanh đen, bỏ mảng hồng/nâu; giữ nhãn trắng, cửa sổ |
| L22 bản đồ | Bỏ mảng trắng, giấy da đều màu, giữ nét mực |

- `audit_color_bleed.py` và `color_bleed_lib.py`: **kiểm tra tự động.** Mỗi tam giác lấy 7 mẫu màu texture trong vùng UV của nó (Lab):
  - **outlier**: mặt lệch ΔE76 > 20 so với *mọi* mặt kề.
  - **UV vắt**: tam giác UV giãn hơn 25 lần trung vị.
  - **loang mipN**: số mặt có màu lọc ở mức mipmap N lệch ΔE > 10 so với một texture "đệm lý tưởng". Texture đó lấy texel ngoài mảnh UV của món loang ra từ chính mảnh gần nhất, nên chỉ đo màu từ ngoài hút vào.

  Lệnh `compare` render 5 góc cận cảnh, chiếu sáng đều, đặt trước và sau cạnh nhau, ô cuối phóng to chỗ khác nhau nhiều nhất.

### Kết quả

Tổng 60 món, trước → sau:

| | Trước | Sau |
|---|---|---|
| Mặt bị loang ở mip0 (soi cận) | 1.068 | 477 |
| Mặt bị loang ở mip1 | 2.141 | 482 |
| Mặt bị loang ở mip2 (nhìn xa) | 10.577 | 506 |
| Mặt có UV vắt qua khe atlas | 228 | 16 |
| Texel cho mỗi món ở 1024 | 3–45% của atlas chung | 33–48% của texture riêng (L15, R08: 11–13%) |

Màu không đổi so với texture gốc 2048: ΔE trung vị từng mặt 0,1–0,5, p99 dưới 8 ở mọi món chưa tô lại. Dung lượng mỗi file 0,1–0,9 MB, cả 6 thư mục 21,5 MB.

**Phần còn lại, đã kiểm từng loại:**
- *Loang mip0 còn 477 mặt*: đều là mặt rất nhỏ, khoảng 1–2 texel trên UV và 1–7 mm² trên vật (trung vị cả lưới là 18 texel). Số gần như không đổi qua mip0, mip1, mip2, nên không phải hút màu theo mipmap mà là giới hạn của phép đo trên mặt nhỏ hơn texel. Không thấy khi render cận.
- *UV vắt còn 16 mặt*: là tam giác dẹt trong 3D. Sau khi xếp lại, mỗi mặt nằm trọn trong một mảnh UV riêng, không còn vắt sang vùng khác.
- *Outlier*: đối chiếu từng mặt với màu của nó trong texture gốc. Ở các món không tô lại, hầu hết khớp màu gốc trong ΔE 10, tức là chi tiết vốn có của thiết kế: chữ và viền R09 vé cược, X01, X03, L08, mặt số V03, vết nứt V01, lỗ giày R01. Chỉ còn V03 (1 mặt, 0,5 mm²), V07 (1 mặt, 1,4 mm²) và L05 (7 mặt, 43 mm² tổng) lệch gốc: đó là mặt trong mảnh UV tí hon đã được gộp sang màu mặt kề, không phải vết màu. Ở 11 món tô lại, outlier lệch gốc là do chủ ý tô: sọc mới của L13, chữ trên vé số L04, tam giác dẹt vắt qua vòng xanh L12, nét mực L22.

Bảng chi tiết từng món (khoảng đệm, tỷ lệ phủ texture, số mặt sửa UV, loang trước → sau, outlier khớp/lệch gốc, ΔE p99 so với gốc):

| Món | Khoảng đệm (px) | Phủ texture | Mặt sửa UV | Loang mip0 | Loang mip1 | Loang mip2 | UV vắt | Outlier (khớp gốc / lệch gốc) | ΔE p99 so gốc | MB |
|---|---|---|---|---|---|---|---|---|---|---|
| CP1 | 4 | 37.8% | 25 | 20 → 6 | 29 → 7 | 117 → 6 | 12 → 0 | 3 (3 / 0) | 2.84 | 0.53 |
| CP2 | 8 | 41.3% | 3 | 2 → 0 | 4 → 0 | 32 → 0 | 0 → 0 | 0 (0 / 0) | 0.63 | 0.26 |
| CP3 | 6 | 43.0% | 4 | 2 → 2 | 4 → 2 | 20 → 0 | 3 → 0 | 0 (0 / 0) | 0.78 | 0.33 |
| H01_cassette | 4 | 38.2% | 26 | 52 → 16 | 58 → 16 | 183 → 14 | 0 → 0 | 1 (1 / 0) | 3.37 | 0.37 |
| H02_non_la | 8 | 44.7% | 4 | 4 → 2 | 12 → 2 | 87 → 2 | 3 → 0 | 5 (5 / 0) | 3.32 | 0.38 |
| H03_tui_than_ky | 4 | 45.3% | 10 | 33 → 12 | 52 → 12 | 220 → 13 | 3 → 0 | 4 (4 / 0) | 2.97 | 0.34 |
| H04_cuc_sac_chuot | 4 | 43.6% | 0 | 21 → 7 | 46 → 7 | 230 → 8 | 0 → 0 | 15 (15 / 0) | 2.01 | 0.32 |
| H05_mu_phu_thuy | 8 | 48.0% | 2 | 12 → 1 | 46 → 1 | 282 → 3 | 0 → 0 | 11 (11 / 0) | 2.13 | 0.25 |
| L01_an_ngoc_ty | 4 | 35.4% | 28 | 19 → 10 | 36 → 10 | 276 → 13 | 10 → 4 | 24 (5 / 19) | 3.24 | 0.81 |
| L02_qua_tao_vang | 12 | 42.4% | 5 | 9 → 0 | 13 → 0 | 94 → 0 | 1 → 0 | 0 (0 / 0) | 2.59 | 0.46 |
| L03_giay_thuy_tinh | 4 | 41.2% | 18 | 39 → 8 | 75 → 10 | 606 → 9 | 11 → 0 | 9 (9 / 0) | 5.37 | 0.7 |
| L04_ve_so_doc_dac | 4 | 40.3% | 11 | 19 → 7 | 27 → 7 | 69 → 8 | 1 → 2 | 15 (0 / 15) | 4.13 | 0.62 |
| L05_no_than | 4 | 34.1% | 14 | 31 → 32 | 57 → 33 | 253 → 27 | 2 → 1 | 47 (40 / 7) | 5.47 | 0.8 |
| L06_gay_sat_lang_giong | 4 | 43.5% | 2 | 11 → 7 | 23 → 6 | 216 → 7 | 0 → 0 | 2 (2 / 0) | 2.59 | 0.63 |
| L07_guom_thuan_thien | 8 | 43.1% | 0 | 6 → 2 | 11 → 2 | 91 → 3 | 0 → 0 | 2 (2 / 0) | 1.15 | 0.24 |
| L08_so_tu_than | 8 | 44.4% | 1 | 27 → 2 | 59 → 2 | 202 → 2 | 1 → 0 | 93 (90 / 3) | 3.0 | 0.2 |
| L09_mu_giap_nguoi_sat | 12 | 41.2% | 1 | 7 → 1 | 16 → 2 | 95 → 2 | 0 → 0 | 17 (17 / 0) | 1.56 | 0.23 |
| L10_mu_do_chu_m | 16 | 41.2% | 4 | 2 → 0 | 2 → 0 | 18 → 0 | 4 → 0 | 11 (10 / 1) | 64.82 | 0.1 |
| L11_gay_nhu_y | 4 | 37.7% | 4 | 18 → 4 | 25 → 4 | 124 → 5 | 2 → 0 | 3 (3 / 0) | 0.92 | 0.18 |
| L12_cup_vang_the_gioi | 8 | 40.9% | 3 | 11 → 0 | 24 → 0 | 108 → 0 | 0 → 0 | 14 (3 / 11) | 0.89 | 0.34 |
| L13_mat_na_pharaoh | 4 | 41.8% | 3 | 28 → 25 | 39 → 35 | 200 → 68 | 3 → 0 | 1114 (217 / 897) | 3.49 | 0.52 |
| L14_mu_rom | 12 | 40.5% | 3 | 9 → 3 | 22 → 3 | 64 → 3 | 0 → 0 | 6 (6 / 0) | 1.63 | 0.2 |
| L15_truyen_than_dong_dat_viet | 4 | 11.1% | 10 | 21 → 12 | 37 → 9 | 134 → 7 | 5 → 1 | 7 (7 / 0) | 4.16 | 0.22 |
| L16_nhan_duy_nhat | 16 | 44.3% | 2 | 1 → 0 | 4 → 0 | 49 → 0 | 2 → 0 | 0 (0 / 0) | 0.71 | 0.15 |
| L17_guom_trong_da | 12 | 44.5% | 4 | 6 → 1 | 19 → 1 | 59 → 1 | 0 → 0 | 0 (0 / 0) | 0.48 | 0.16 |
| L18_mat_na_chua_te_bong_toi | 4 | 43.4% | 6 | 31 → 11 | 192 → 12 | 873 → 14 | 2 → 0 | 3 (3 / 0) | 2.01 | 0.36 |
| L19_gang_tay_lap_lanh | 12 | 41.8% | 0 | 2 → 2 | 12 → 2 | 86 → 1 | 0 → 0 | 1 (0 / 1) | 0.79 | 0.21 |
| L20_bang_video_7_ngay | 12 | 41.9% | 4 | 9 → 4 | 39 → 4 | 157 → 4 | 1 → 0 | 7 (7 / 0) | 1.03 | 0.15 |
| L21_than_kiem_kusanagi | 8 | 44.8% | 3 | 7 → 0 | 14 → 0 | 127 → 0 | 2 → 0 | 0 (0 / 0) | 0.5 | 0.16 |
| L22_ban_do_co | 4 | 34.2% | 16 | 22 → 6 | 99 → 8 | 438 → 13 | 8 → 0 | 6 (2 / 4) | 4.06 | 0.44 |
| R01_giay_cu | 4 | 37.3% | 9 | 25 → 13 | 54 → 13 | 243 → 11 | 1 → 0 | 1 (1 / 0) | 2.05 | 0.46 |
| R02_tat_thoi | 12 | 40.6% | 0 | 5 → 8 | 5 → 8 | 30 → 6 | 0 → 0 | 0 (0 / 0) | 0.92 | 0.18 |
| R03_trung_thoi | 6 | 45.8% | 84 | 3 → 3 | 10 → 2 | 55 → 5 | 61 → 0 | 0 (0 / 0) | 1.17 | 0.35 |
| R04_xuong_ca | 4 | 41.8% | 1 | 11 → 4 | 52 → 3 | 264 → 5 | 0 → 0 | 0 (0 / 0) | 1.73 | 0.36 |
| R05_dep_to_ong | 6 | 40.9% | 1 | 11 → 7 | 38 → 6 | 191 → 6 | 0 → 0 | 0 (0 / 0) | 0.79 | 0.38 |
| R06_lon_bep | 16 | 43.8% | 0 | 3 → 0 | 4 → 0 | 45 → 0 | 0 → 0 | 2 (2 / 0) | 1.04 | 0.18 |
| R07_ao_dau_cu | 6 | 41.1% | 3 | 28 → 4 | 57 → 4 | 365 → 3 | 0 → 0 | 5 (5 / 0) | 2.74 | 0.32 |
| R08_bao_thuoc_rong | 4 | 12.6% | 39 | 56 → 21 | 74 → 21 | 251 → 19 | 4 → 0 | 11 (11 / 0) | 6.39 | 0.37 |
| R09_ve_cuoc | 8 | 43.4% | 6 | 29 → 10 | 65 → 10 | 305 → 9 | 1 → 0 | 113 (113 / 0) | 3.2 | 0.38 |
| TC1 | 12 | 41.9% | 4 | 3 → 1 | 4 → 1 | 21 → 1 | 3 → 0 | 0 (0 / 0) | 0.71 | 0.23 |
| TC2 | 4 | 43.5% | 39 | 14 → 6 | 24 → 6 | 140 → 6 | 16 → 0 | 0 (0 / 0) | 1.55 | 0.28 |
| TC3 | 8 | 44.6% | 3 | 5 → 0 | 9 → 0 | 50 → 0 | 3 → 0 | 5 (5 / 0) | 1.09 | 0.19 |
| V01_binh_su | 16 | 43.9% | 3 | 0 → 0 | 0 → 0 | 10 → 0 | 2 → 0 | 0 (0 / 0) | 0.9 | 0.15 |
| V02_sach_co | 4 | 32.9% | 13 | 30 → 30 | 19 → 29 | 10 → 25 | 8 → 1 | 1 (1 / 0) | 7.85 | 0.33 |
| V03_dong_ho | 4 | 35.4% | 21 | 52 → 25 | 101 → 26 | 351 → 25 | 5 → 1 | 64 (63 / 1) | 7.43 | 0.46 |
| V04_kinh_thien_van | 4 | 33.9% | 26 | 37 → 27 | 56 → 28 | 228 → 30 | 4 → 0 | 51 (51 / 0) | 5.93 | 0.66 |
| V05_thiet_bi_lien_lac | 4 | 36.8% | 13 | 44 → 17 | 51 → 15 | 245 → 16 | 2 → 0 | 15 (15 / 0) | 4.02 | 0.5 |
| V06_pin_luong_tu | 4 | 38.8% | 11 | 27 → 12 | 44 → 13 | 113 → 10 | 4 → 0 | 1 (1 / 0) | 3.07 | 0.36 |
| V07_tinh_the_lam | 6 | 42.7% | 6 | 41 → 11 | 73 → 11 | 444 → 10 | 0 → 0 | 3 (2 / 1) | 1.73 | 0.44 |
| V08_guong_co_tich | 4 | 43.4% | 7 | 12 → 9 | 23 → 8 | 125 → 9 | 1 → 0 | 2 (2 / 0) | 2.73 | 0.42 |
| V09_la_bua | 8 | 40.6% | 12 | 11 → 4 | 25 → 4 | 119 → 4 | 4 → 0 | 6 (6 / 0) | 2.04 | 0.22 |
| V10_manh_vay_long | 4 | 43.2% | 2 | 12 → 6 | 21 → 6 | 154 → 7 | 0 → 0 | 1 (1 / 0) | 1.68 | 0.43 |
| V11_quat_nan | 4 | 35.5% | 9 | 34 → 19 | 51 → 20 | 175 → 21 | 2 → 1 | 5 (5 / 0) | 2.65 | 0.56 |
| V12_may_game | 16 | 41.1% | 5 | 3 → 1 | 5 → 1 | 37 → 1 | 0 → 0 | 4 (4 / 0) | 1.56 | 0.15 |
| X01_dan_than | 8 | 41.2% | 1 | 12 → 4 | 43 → 4 | 242 → 2 | 0 → 0 | 50 (50 / 0) | 2.17 | 0.36 |
| X02_trong_dong | 4 | 38.9% | 75 | 35 → 11 | 55 → 8 | 223 → 8 | 11 → 1 | 1 (1 / 0) | 2.59 | 0.59 |
| X03_kiem_sang | 8 | 45.6% | 6 | 9 → 2 | 29 → 2 | 171 → 3 | 0 → 0 | 64 (64 / 0) | 1.64 | 0.3 |
| X04_qua_cau_ngoc | 12 | 41.0% | 8 | 6 → 2 | 12 → 2 | 146 → 2 | 4 → 0 | 4 (4 / 0) | 1.41 | 0.37 |
| X05_trai_tim_robot | 4 | 33.6% | 46 | 21 → 25 | 34 → 23 | 272 → 21 | 15 → 2 | 1 (1 / 0) | 5.64 | 0.91 |
| X06_phi_thuyen_mini | 4 | 46.1% | 7 | 8 → 12 | 7 → 11 | 42 → 8 | 1 → 2 | 1 (1 / 0) | 4.98 | 0.6 |

Ảnh trước và sau từng món: `KiemTra3D/loang_mau/<tên>.jpg`. Hàng trên là bản trước (atlas 1024 chung), hàng dưới là bản sau; ô cuối phóng to vùng khác nhau nhiều nhất. Với 11 món tô lại, hàng trên là bản gốc trước mọi lần sửa.

**Trong Unity:** mỗi món giờ có texture riêng và các mảnh UV đã có phần đệm, nên để mipmap bật như mặc định. Không cần gộp material dùng chung atlas như gợi ý cũ ở mục "Lỗi cần sửa" số 2.

# Bàn giao mô hình 3D cho Antigravity

Gói 24 mô hình `.glb` do Meshy tạo, dùng để thay các `*_Placeholder` trong [Market_Test.unity](../../Assets/_Project/Scenes/Market_Test.unity) và nối vào prefab theo [DataContract.md](../Architecture/DataContract.md). Đây là PR [#5](https://github.com/lqt140304-rgb/game-c-/pull/5).

## Việc cần làm trước khi dùng

1. **Lấy file bằng Git LFS.** Cài Git LFS rồi chạy `git lfs pull`. Nếu không, mỗi `.glb` chỉ là file con trỏ vài trăm byte.
2. **Cài trình nhập glTF.** `Packages/manifest.json` hiện chưa có gói đọc `.glb`. Thêm `com.unity.cloud.gltfast` (glTFast), hoặc mở file trong Blender rồi xuất `.fbx`. Tôi chưa chỉnh manifest, vì đó là quyết định của bên dựng cảnh.
3. **Giảm poly và nén texture trước khi build.** Mỗi file nặng 40–55 MB và có 60–870 nghìn tam giác, vì texture của cả tấm gốc bị lặp lại cho từng vật. Dùng được để dựng thử, chưa dùng được để build.

## Quy ước chung

- Gốc tọa độ ở **giữa đáy**, đáy chạm y=0, trục y hướng lên.
- Kích thước tính theo đơn vị Meshy (khoảng 0,2–1,3), **chưa đồng đều giữa các nhóm**. Cần đặt lại scale trên prefab theo cỡ vật trên bàn.
- Mọi mô hình có đủ mặt sau và texture PBR. Ảnh nguồn và ảnh render 4 góc ở `Assets/_Project/Art/Source/`.

## Hộp mù (`Assets/_Project/Art/Items/Boxes/`)

| ID | Tên | File | Ghi chú |
|---|---|---|---|
| H1 | Hộp Gỗ Nhỏ | `H1.glb` | Gỗ sồi, góc đồng, quai dây thừng, dấu sáp |
| H2 | Hộp Kim Loại Mờ | `H2.glb` | Titan phay xước, có chạm khắc |
| H3 | Hộp Vải Dày | `H3.glb` | Vải chần chỉ, buộc dây, dấu sáp |
| RD | Rương Đen | `RD.glb` | Rương lacquer đen viền vàng, ổ khóa |

Chỉ có bản **nắp đóng**. Cảnh mở niêm phong cần animation riêng hoặc bản mở nắp (chưa làm).

## Hiện vật (`Assets/_Project/Art/Items/`)

| ID | Tên | File | Ghi chú |
|---|---|---|---|
| V01 | Bình sứ men xanh | `V01.glb` | |
| V02 | Sách cổ bìa da | `V02.glb` | |
| V03 | Đồng hồ quả lắc cũ | `V03.glb` | Dạng đồng hồ để bàn, quả lắc dính liền thân |
| V04 | Mảnh kính đồng hồ thiên văn | `V04.glb` | Mỏng (vật dẹt) |
| V05 | Thiết bị liên lạc hỏng | `V05.glb` | Mặt lưng đơn giản |
| V06 | Bộ pin lượng tử | `V06.glb` | Lõi cyan, cần emission |
| V07 | Tinh thể lam cầm tay | `V07.glb` | Có cán gỗ |
| V08 | Gương thủy tinh cổ tích | `V08.glb` | Mỏng, lưng tối |
| V09 | Lá bùa giấy vàng | `V09.glb` | Mỏng |
| V10 | Mảnh vảy long | `V10.glb` | Mỏng, cong |

## Tạo tác (`Assets/_Project/Art/Items/Artifacts/`)

| ID | Tên | File | Ghi chú |
|---|---|---|---|
| CP1 | Phiến khắc tre | `CP1.glb` | Nằm ngang như cuộn thư |
| CP2 | Hũ đựng đồ | `CP2.glb` | |
| CP3 | Ấn triện đồng | `CP3.glb` | |
| TC1 | Ốc vít dự phòng | `TC1.glb` | Gồm bu lông và đai ốc, một mảnh |
| TC2 | Mắt máy soi | `TC2.glb` | Mống mắt cyan, cần emission |
| TC3 | Lõi năng lượng | `TC3.glb` | Ống sáng cyan, cần emission |

## Gia truyền và Bí Khế (`Assets/_Project/Art/Items/Heirlooms/`)

| ID | Tên | File | Ghi chú |
|---|---|---|---|
| GT1 | Sổ Ghi Nợ Cũ | `GT1.glb` | Nằm ngang |
| GT2 | Đèn Dầu Thờ | `GT2.glb` | Ngọn lửa là mô hình tĩnh, nên thay bằng hiệu ứng hạt hoặc emission |
| BK1 | Lời Thề Bảy Ngày | `BK1.glb` | Cuộn giấy có dấu sáp, nằm ngang |
| BK2 | Mắt Người Sành | `BK2.glb` | **Thiếu mặt kính**, cần thêm mặt phẳng thủy tinh |

Bí Khế có thể chỉ cần icon 2D nếu không đặt lên bàn.

## Gợi ý nối vào dữ liệu

- Đặt tên prefab theo ID (`Item_V01`, `Box_H1`, `Artifact_CP1`, `Heirloom_GT1`) và dùng ID làm `VisualPrefabKey` / `VisualVariantKey`. ID khớp `Docs/Content/data/content.json`.
- Kim loại (V05, V06, TC1, TC2, TC3) ra khá tối, cần chỉnh độ sáng và độ kim loại của vật liệu hoặc thêm reflection probe.
- Vật dẹt (V04, V08, V09, V10) cần xoay khi cầm lên để không bị nhìn nghiêng.

## Chưa có

- Hộp mở nắp hoặc animation mở.
- Môi trường chợ (bàn, kệ, quầy).
- Hiệu ứng hạt, phát sáng, chuyển động. Mô hình đều là mesh tĩnh, không có xương.

## Cầm và soi vật phẩm (kiểu Valorant)

Mô hình Meshy là mesh tĩnh nên mọi chuyển động do code điều khiển. Bộ script nằm ở `Assets/_Project/Scripts/Interaction/`:

| File | Vai trò |
|---|---|
| `HeldItemViewmodel.cs` | Gắn vào object con của camera. Cầm vật ở góc dưới bên phải, lắc nhẹ khi đi và thở, nâng lên khi cầm, cất xuống khi bỏ. **F** soi, **Q** cất |
| `HeldItemProfile.cs` | Cấu hình từng món: cỡ khi cầm, góc nghiêng, và chuỗi tư thế soi. V01–V10 có sẵn tư thế riêng với đúng dòng "dấu hiệu soi" trong `content.json`, các món khác dùng chuỗi chung (đưa sát, xoay, lật xem đáy) |
| `PickupInteractable.cs` | Vật trong cảnh, nhấn **E** để cầm (qua `IInteractable` sẵn có). Cất đi thì vật quay lại chỗ cũ |
| `Editor/HeldItemDemoSetup.cs` | Menu `Tools → Cho Do La → Add Held Item Demo`: thêm viewmodel vào camera và đặt 8 vật mẫu trước mặt người chơi |

Cách thử: tạo cảnh bằng `Create Sprint 0 Test Scene`, chạy `Add Held Item Demo`, nhấn Play, nhìn vào vật rồi **E** cầm, **F** soi, **Q** cất. Nếu chưa nhập được `.glb` thì vật mẫu hiện dạng khối vuông.

**Chưa kiểm chứng:** các script này chưa được biên dịch hay chạy trong Unity (chỉ kiểm tra cú pháp). Cần Antigravity chạy thử và chỉnh `restPosition`, `targetSize`, tư thế soi cho từng món.

**Chưa có:** bàn tay (cánh tay) cầm vật, camera riêng cho vật cầm để không xuyên tường, tiếng động khi soi, và tư thế soi riêng cho hộp, tạo tác, gia truyền (hiện dùng chuỗi chung).

### Cuộn thề mở và đóng được (BK1)

`ScrollUnroll.cs` dựng cuộn thề hoàn toàn bằng code (giấy, hai trục gỗ, dây buộc, dấu sáp), không dùng file `.glb`. Giấy là dải mesh uốn theo đường xoắn ốc quanh trục nên lớp cuộn co dần khi mở. Khi cầm cuộn, **F** mở hoặc đóng thay cho động tác soi; dây và dấu sáp rơi ra lúc bắt đầu mở. Texture giấy ở `Assets/_Project/Art/Textures/scroll_parchment.png`, ảnh tham chiếu ở `Art/Source/scroll_unroll_*.png` (dựng bằng bản xem trước three.js dùng cùng công thức, **không phải ảnh chụp từ Unity**). Các món khác cần mở/đóng (sách, hộp, kính lúp) có thể áp dụng cùng hướng này.

## Hộp mù low-poly và animation mở hộp

Bản low-poly (tông trung tính, 650–880 tam giác mỗi hộp) nằm ở `Assets/_Project/Art/Items/LowPoly/Boxes/` và **thay thế bản photoreal** khi dùng trong game. Dựng bằng script Blender `Tools/Blender/make_boxes_lowpoly.py` (chạy lại là ra hộp mới, dễ chỉnh).

Mỗi hộp tách thành các lớp (object con của root `H1`, `H2`, `H3`, `RD`):

| Lớp | Vai trò |
|---|---|
| `Body` | Thân hộp, kèm lòng hộp tối |
| `Lid` | Nắp, **gốc tọa độ đặt đúng bản lề** nên chỉ cần xoay quanh trục X |
| `Seal` | Dấu sáp và dải giấy niêm |
| `Binding` | Dây buộc, thanh khóa hoặc ổ khóa (tùy hộp) |
| `Glow` | Quầng sáng nổi lên ở miệng hộp, mặc định ẩn bằng scale |

`Scripts/Interaction/BoxOpenSequence.cs` mở hộp **từ từ qua 5 giai đoạn** để tăng kịch tính: (1) Rung, (2) Gỡ niêm: dấu sáp và dây/khóa rơi ra, (3) Hé nắp: nắp he hé, ánh sáng lọt ra, (4) Bật nắp: nắp bật lên, (5) Mở toang: nắp mở hết, ánh sáng đạt đỉnh. Mỗi giai đoạn có thời gian chuyển, thời gian giữ (để hồi hộp), độ rung và độ sáng, chỉnh được trong Inspector. Có sự kiện `onStageReached(int)` để gắn âm thanh, hạt, giao diện, và `onOpened` khi xong. Có thể bật `waitForPlayer` ở một giai đoạn để chờ người chơi bấm. Gọi `Play()` để chạy.

**Chưa kiểm chứng trong Unity.** Ảnh tham chiếu `Art/Source/render_boxes_lowpoly_open.png` dựng bằng bản xem trước three.js dùng cùng dữ liệu giai đoạn. Sau khi nhập `.glb`, nếu nắp mở ngược chiều hoặc dấu sáp rơi ra phía sau, đổi `lidOpenSign` hoặc `frontDirection` trong Inspector. Bản photoreal 200–300 nghìn tam giác vẫn còn ở `Items/Boxes/` để so sánh.

## Tạo tác low-poly

**Bản dùng chính:** `Assets/_Project/Art/Items/LowPoly/Artifacts/`, tạo bằng Meshy từ một ảnh gộp 6 món (`Art/Source/artifacts_lowpoly_reference.png`), giảm poly ngay ở Meshy, 2,4–10 nghìn tam giác mỗi món. Đẹp và chi tiết hơn bản Blender, nhất là sư tử của ấn triện. Phần phát sáng của TC2, TC3 nằm trong texture, **không** tách thành lớp riêng. Ảnh 4 góc: `Art/Source/render_artifacts_lowpoly_meshy.png`.

**Bản Blender (để so sánh, không dùng):** `Assets/_Project/Art/Items/LowPoly/Artifacts_Blender/`, 400–1750 tam giác mỗi món, dựng bằng `Tools/Blender/make_artifacts_lowpoly.py`. Mỗi file có root mang ID (`CP1`…`TC3`), con `Body`, và với TC2, TC3 có thêm con `Glow` (mống mắt, ống plasma) dùng vật liệu phát sáng để gắn bloom hoặc nhấp nháy trong Unity. Gốc tọa độ ở giữa đáy. CP1, TC1, TC3 nằm ngang theo dáng tự nhiên. Ảnh 4 góc: `Art/Source/render_artifacts_lowpoly.png`.

### Thử nghiệm (không dùng): hộp gỗ H1 bằng Meshy rồi cắt nắp (`Art/Items/LowPoly/Boxes_Meshy/H1.glb`)

**Quyết định:** 4 hộp mù dùng **bản dựng bằng Blender** (`Items/LowPoly/Boxes/`), vì tách được dấu sáp, dây/khóa và quầng sáng cho animation mở hộp. Bản Meshy dưới đây chỉ để tham khảo.

Meshy tạo hộp (8 nghìn tam giác), rồi script `Tools/Blender/cut_lid_from_meshy.py` cắt mô hình theo đường nối nắp và tách `Body` + `Lid` (gốc tại bản lề phía sau). Chạy: `python cut_lid_from_meshy.py -- <meshy.glb> <ra.glb> <độ cao đường nối theo trục Y của glTF> <chiều cao mong muốn, mét>`. Với hộp này đường nối ở y = 0,43 và chiều cao 0,24 m. `BoxOpenSequence` chạy được trên file này (nắp mở đúng, thấy lòng hộp rỗng).

Giới hạn: dấu sáp và dây/khóa nằm luôn trong texture nên **không tách thành lớp** `Seal`, `Binding` được; không có `Glow`. Độ cao đường nối phải tìm tay cho từng hộp. Mặt cắt nắp và thân được trám tự động, texture ở mặt cắt không đẹp. 4 hộp **không gộp một ảnh được** (Meshy dính thành một khối), phải làm từng hộp, 30 credit mỗi hộp.

## Hiện vật Rác và Thường low-poly (17 món)

Bản dùng chính: `Assets/_Project/Art/Items/LowPoly/Junk/` (R01–R09) và `Common/` (V01–V05, V09, V11, V12), tạo bằng Meshy từ ảnh gộp có bóng đổ (`Art/Source/ref_*.png`), tách mesh rồi đặt đáy y=0, giữa gốc tọa độ, và đặt kích thước thật (mét). 1,4–17 nghìn tam giác mỗi món. Bản dựng bằng code `Junk_Blender` và `Common_Blender` giữ để so sánh, **không dùng**.

Lưu ý: tất thối và trứng thối có vạch mùi xanh nổi trên đầu, **đó là phần mesh**. Mặt thiên văn vỡ (V04) bỏ các mảnh kính rơi dưới nền. Huy hiệu áo đấu (R07) là khuôn mặt sừng cách điệu, đã chủ ý khác logo câu lạc bộ thật; bao thuốc rồng (R08) không có chữ hay thương hiệu.

## Hiện vật Hiếm low-poly (7 món)

`Assets/_Project/Art/Items/LowPoly/Rare/`, tạo bằng Meshy từ **một ảnh gộp 7 món** (`Art/Source/ref_hiem.png`), 4,5–8,2 nghìn tam giác mỗi món: `V06_pin_luong_tu`, `V08_guong_co_tich`, `H01_cassette`, `H02_non_la`, `H03_tui_than_ky`, `H04_cuc_sac_chuot`, `H05_mu_phu_thuy`. Mã `H01`–`H05` là tạm, chưa khớp `content.json`. Món nhại: túi thần kỳ (H03), cục sạc chuột (H04) và mũ phù thủy (H05) chỉ gợi nhắc chứ không dùng tên hay thiết kế gốc. Đã xuất theo cỡ thật (mét), đáy y=0.

## Hiện vật Rất hiếm low-poly (6 món)

`Assets/_Project/Art/Items/LowPoly/VeryRare/`, tạo bằng Meshy từ **một ảnh gộp 6 món** (`Art/Source/ref_rat_hiem.png`), chi tiết hơn bậc Hiếm (4–23 nghìn tam giác mỗi món): `V07_tinh_the_lam`, `V10_manh_vay_long`, `X01_dan_than`, `X02_trong_dong`, `X03_kiem_sang`, `X04_qua_cau_ngoc`, cộng `X05_trai_tim_robot` và `X06_phi_thuyen_mini` (hạ từ Huyền thoại). Mã `X01`–`X06` là tạm. **X04 (quả cầu ngọc) và X03 (kiếm sáng) gợi nhắc rất rõ nhân vật và tác phẩm nổi tiếng, cần chủ dự án duyệt về bản quyền trước khi phát hành.** Cỡ thật (mét), đáy y=0.

## Hiện vật Huyền thoại low-poly (6 trong 8 món)

`Assets/_Project/Art/Items/LowPoly/Legendary/`, làm bằng Meshy với poly và texture cao nhất: `L01_an_ngoc_ty`, `L02_qua_tao_vang`, `L03_giay_thuy_tinh`, `L04_ve_so_doc_dac`, `L05_no_than`, `L06_gay_sat_lang_giong`. Mã tạm. **Còn 2 món chưa có mô hình:** Chiếc nhẫn duy nhất và Quyển sổ định mệnh (giữ trong danh sách, chờ gom thêm món rồi làm chung một lượt Meshy). Hai món Trái tim robot và Phi thuyền mini đã **hạ xuống Rất hiếm** (`VeryRare/X05`, `X06`). Ảnh nguồn: `Art/Source/ref_huyen_thoai.png`, `ref_huyen_thoai2.png`, `ref_no_roi.png`.

**Gậy sắt làng Gióng** (`L06`): theo truyền thuyết là gậy sắt thẳng và cứng, nên mô hình để dáng thẳng, không xương, không chuyển động. Công cụ `Tools/Blender/rig_whip.py` (gắn xương và sóng uốn cho vật thẳng) và video thử `Art/Source/roi_sat_uon_luon.mp4` chỉ giữ để tham khảo, không dùng cho món này.

Lưu ý: Meshy không giữ vật trong suốt, nên trái tim robot (X05) không có chuông kính. Đệm giày thủy tinh (L03) có một vệt đỏ do texture.

## Huyền thoại theo rương (đợt 2026-11-24, low-poly)

Theo spec [rương chủ đề](../../docs/superpowers/specs/2026-11-24-ruong-chu-de-design.md). Meshy, 2 ảnh gộp 8 món, ~30 nghìn tam giác mỗi ảnh (như các bậc khác, không tăng poly). Ảnh nguồn `Art/Source/ref_huyen_thoai_chu_de_1.png`, `_2.png`; ảnh kiểm tra `render_huyen_thoai_chu_de_1.png`, `_2.png`. Kích thước đã đặt theo vật thật, đáy ở y=0.

| Mã | Món | Rương |
|---|---|---|
| L07 | Gươm Thuận Thiên | Việt Nam |
| L08 | Quyển sổ tử thần (nhại) | Wibu |
| L09 | Mũ giáp người sắt (nhại) | Robot |
| L10 | Mũ đỏ chữ M (nhại) | Game |
| L11 | Gậy Như Ý | Trung Hoa cổ |
| L12 | Cúp vàng thế giới (nhại) | Thể thao |
| L13 | Mặt nạ vàng pharaoh | Ai Cập cổ đại |
| L14 | Mũ rơm (nhại) | Hải tặc |
| L15 | Truyện tranh thần đồng đất Việt (nhại) | Tuổi thơ Việt Nam |
| L16 | Chiếc nhẫn duy nhất (nhại) | Truyền thuyết Châu Âu |
| L17 | Thanh gươm trong đá | Truyền thuyết Châu Âu |
| L18 | Mặt nạ chúa tể bóng tối (nhại) | Không gian |
| L19 | Găng tay lấp lánh (nhại) | Âm nhạc |
| L20 | Băng video "7 ngày" (nhại) | Phim kinh dị |
| L21 | Thần kiếm Kusanagi | Nhật Bản cổ |
| L22 | Bản đồ cổ vẽ tay | Vải Niêm (rương cơ bản) |

Lưu ý: găng tay L19 mất lớp kim tuyến (ra màu trắng trơn); kiếm L21 hơi cong. Đồng hồ bỏ túi (L23) và Két sắt (L24) làm ở [đợt làm lại 1](#đợt-làm-lại-1-2026-11-24).

## Rất hiếm theo rương (đợt 2026-11-24, low-poly)

Theo spec [rương chủ đề](../../docs/superpowers/specs/2026-11-24-ruong-chu-de-design.md). Meshy, 3 ảnh gộp 8 món, ~30 nghìn tam giác mỗi ảnh (không tăng poly), tốn 117 credit. File ở `Assets/_Project/Art/Items/LowPoly/VeryRare/`. Ảnh nguồn `Art/Source/ref_rat_hiem_chu_de_1.png`, `_2.png`, `_3.png` (ảnh 3 đã xóa logo thật trên giày); ảnh kiểm tra 4 góc `render_rat_hiem_chu_de_1.png`, `_2.png`. Cỡ thật (mét), đáy y=0. Mã X07–X30 cấp theo thứ tự danh sách; mã của món lỗi giữ chỗ.

| Mã | Món | Rương | Tình trạng |
|---|---|---|---|
| X07 | Tượng Di Lặc gỗ | Gỗ | Có |
| X08 | Nhẫn kim cương không rõ nguồn | Đen | Có (làm lại), cao 4 cm, đá có vệt đỏ do texture |
| X09 | Đèn ông sao | Tuổi thơ Việt Nam | Có (làm lại) |
| X10 | Đũa thủy thủ mặt trăng (nhại) | Wibu | Có (làm lại 2) |
| X11 | Sừng kỳ lân | Truyền thuyết Châu Âu | Có |
| X12 | Đũa phép cậu bé phù thủy (nhại) | Truyền thuyết Châu Âu | Có, mất giá đỡ, bề mặt hơi thô |
| X13 | Đầu mô hình Gundam (nhại) | Robot | Có |
| X14 | Lá cờ trên mặt trăng | Không gian | Có |
| X15 | Khối kim cương vuông (nhại Minecraft) | Game | Có |
| X16 | Kiếm thần của chàng áo xanh (nhại Zelda) | Game | Có, cắm trên bệ đá |
| X17 | Áo đấu số 7 (nhại) | Thể thao | Có (làm lại), không ma-nơ-canh |
| X18 | Giày "biết bay" (nhại) | Thể thao | Có (làm lại 2), đã xóa logo thật, cánh vàng bên hông |
| X19 | Đàn bầu | Âm nhạc | Có, mất quả bầu (bầu bị tách rời, đã bỏ) |
| X20 | Cây ghi-ta đập vỡ | Âm nhạc | Có, dáng đứng nghiêng |
| X21 | Búp bê ma (nhại) | Phim kinh dị | Có |
| X22 | Bóng bay đỏ (nhại) | Phim kinh dị | Có (làm lại 2) |
| X23 | Mắt Horus | Ai Cập cổ đại | Có, trên giá đỡ |
| X24 | Quan tài xác ướp mini | Ai Cập cổ đại | Có |
| X25 | Bản đồ kho báu | Hải tặc | Có (làm lại 2), cuộn tròn |
| X26 | Trái ác quỷ (nhại) | Hải tặc | Có (làm lại) |
| X27 | Thanh Long Yển Nguyệt Đao | Trung Hoa cổ | Có (làm lại), nằm ngang, dài 2,2 m |
| X28 | Vòng kim cô | Trung Hoa cổ | Có |
| X29 | Giáp samurai mini | Nhật Bản cổ | Có |
| X30 | Tranh sóng lừng (nhại Hokusai) | Nhật Bản cổ | Có, trên giá vẽ |

Ghi chú: ảnh gộp 2 chỉ còn 4/8 món (mất nhẫn trong hộp, áo trên ma-nơ-canh, bóng bay, trái ác quỷ). Giày đã xóa logo thật trong ảnh nguồn trước khi dựng.

## Đợt làm lại 1 (2026-11-24)

Một ảnh gộp 8 món, 39 credit. Ảnh nguồn `Art/Source/ref_lam_lai_1.png`, ảnh kiểm tra `render_lam_lai_1.png`. Đủ 8 món, không món nào dính.

| Mã | Món | Rương | File |
|---|---|---|---|
| L23 | Đồng hồ bỏ túi không bao giờ hết pin | Kim Khí | `Legendary/L23_dong_ho_bo_tui.glb` (nắp mở, có dây xích) |
| L24 | Két sắt chưa ai mở được | Đen | `Legendary/L24_ket_sat.glb` |
| X08, X09, X17, X26, X27 | Xem bảng Rất hiếm ở trên | | `VeryRare/` |

Găng tay lấp lánh (L19) làm lại vẫn ra trắng trơn: Meshy không giữ được kim tuyến. **Giữ bản L19 cũ**, phần lấp lánh làm bằng vật liệu (normal map hạt cườm, độ kim loại cao) hoặc hạt sáng trong Unity, không chạy Meshy thêm cho món này.

## Hiếm theo rương, đợt 1 (2026-11-24, kèm làm lại 2)

2 ảnh gộp 8 món, 78 credit. Ảnh nguồn `Art/Source/ref_hiem_chu_de_1.png`, `_2.png` (ảnh 2 đã xóa logo giày); ảnh kiểm tra `render_hiem_chu_de_1.png`, `_2.png`. File ở `Items/LowPoly/Rare/` (H) và `VeryRare/` (X10, X18, X22, X25). Mã H06–H17 cấp theo thứ tự spec.

| Mã | Món | Rương | Tình trạng |
|---|---|---|---|
| H06 | Ấm trà đất nứt | Gỗ | Có |
| H07 | Hộp nữ trang gỗ khảm | Gỗ | Có |
| H08 | Tranh cuộn thủy mặc | Vải Niêm | Có, cuộn lại nằm ngang |
| H09 | Con dấu đồng | Vải Niêm | Có, núm rồng |
| H10 | Máy ảnh phim cổ | Kim Khí | Có |
| H11 | Bật lửa mạ vàng | Đen | Có, nắp mở |
| H12 | Đồng hồ "xách tay" | Đen | Có |
| H13 | Bánh chưng Lang Liêu | Việt Nam | Có (làm lại ở đợt 2) |
| H14 | Hộp bút hai tầng | Tuổi thơ Việt Nam | Có |
| H15 | Máy nuôi gà ảo | Tuổi thơ Việt Nam | Có |
| H16 | Cánh cửa thần kỳ | Wibu | Có, mỏng |
| H17 | Robot vòm tròn bíp bíp (nhại R2) | Robot | Có, sọc cam |

## Hiếm theo rương, đợt 2 (2026-11-24, kèm làm lại H13)

2 ảnh gộp 8 món, 78 credit (số dư còn 768). Ảnh nguồn `Art/Source/ref_hiem_chu_de_3.png`, `_4.png` (ảnh 4 đã tô mờ đường kẻ ô); ảnh kiểm tra `render_hiem_chu_de_3.png`, `_4.png`. File ở `Items/LowPoly/Rare/`, 1–11 nghìn tam giác mỗi món, cỡ thật (mét), đáy y=0. Mã H18–H32 cấp theo thứ tự spec.

| Mã | Món | Rương | Tình trạng |
|---|---|---|---|
| H13 | Bánh chưng Lang Liêu | Việt Nam | Có |
| H18 | Chó robot | Robot | Có, gộp thân và đầu |
| H19 | Mũ phi hành gia | Không gian | Có, kính vàng đặc; đã tô lại vỏ trắng (bỏ vệt hồng cam) |
| H20 | Nấm đỏ (nhại) | Game | Có |
| H21 | Khối gạch dấu hỏi (nhại) | Game | Có |
| H22 | Găng thủ môn | Thể thao | Có, mỏng |
| H23 | Bóng rổ số 23 (nhại) | Thể thao | Có; đã tô lại cam đều, bỏ số thừa ở mặt sau |
| H24 | Micro cổ | Âm nhạc | Có, chân đế là một phần của micro |
| H25 | Kính tròn ca sĩ tứ quái (nhại) | Âm nhạc | Có, tròng tối đặc, gộp tròng và gọng |
| H26 | Mặt nạ khúc côn cầu (nhại) | Phim kinh dị | Có, đã bỏ giá đỡ |
| H27 | Hộp nhạc tự kêu | Phim kinh dị | Có, nắp mở, có búp bê múa |
| H28 | Tượng mèo Bastet | Ai Cập cổ đại | Có |
| H29 | Kim tự tháp mini | Ai Cập cổ đại | Có |
| H30 | Móc tay thuyền trưởng | Hải tặc | Có |
| H31 | Cờ đầu lâu | Hải tặc | Có, cờ mỏng |
| H32 | Bàn tính | Trung Hoa cổ | Có, gộp khung và hạt |

Tô lại texture (không tốn credit): script ở `Tools/Meshy/retex/` chọn mặt tam giác theo màu và hướng rồi tô lại vùng UV của chúng trên texture trong file `.glb`. Dùng cho lỗi màu nhỏ thay vì chạy lại Meshy.

## Danh sách làm lại

Không làm lại riêng lẻ. Đủ 8 món hoặc hết món khác thì hỏi chủ dự án (~39 credit mỗi ảnh gộp 8 món).

(Trống. H13 đã làm lại ở đợt 2.)

Còn lại chưa làm: 3 món Hiếm (Bình gốm Thanh Hoa, Mặt nạ quỷ Oni, Búp bê Daruma) và 29 món Thường.

Kinh nghiệm: món đặt trong hộp, trên ma-nơ-canh hoặc giá đỡ dễ bị mất hay dính; để món đứng riêng, không đế thì Meshy giữ tốt hơn. Meshy có thể xếp lại vị trí các món (không theo lưới ảnh) hoặc nhân đôi một món: xuất từng mảnh ra ảnh 4 góc để nhận diện trước khi đặt tên.

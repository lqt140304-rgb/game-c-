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

Lưu ý: găng tay L19 mất lớp kim tuyến (ra màu trắng trơn); kiếm L21 hơi cong. Chưa có: Đồng hồ bỏ túi (Kim Khí), Két sắt (Đen), gom vào đợt làm lại sau.

## Rương Golden Treasure (thử nghiệm, bản đầu tiên của rương Meshy tự mang vào)

`Assets/_Project/Art/Chests/Chest_Golden.glb`, **chưa gán chủ đề hay ID rương** vì danh sách chủ đề chưa chốt. Nguồn là file FBX Meshy do chủ dự án cung cấp (4,7 nghìn tam giác, không tốn credit mới). Tạo bằng `Tools/Blender/cut_lid_from_meshy_fbx.py`:

```
python cut_lid_from_meshy_fbx.py -- <thư mục chứa fbx và png> <ra.glb> Chest_Golden 0.03 0.42 1024 "0,-0.616,-0.12"
```

Tham số: độ cao đường nối nắp theo đơn vị nguồn (0,03, nằm giữa dải xám), chiều cao mong muốn 0,42 m, cỡ texture, và vị trí ổ khóa `x,y,z` theo đơn vị nguồn (bỏ trống thì không tạo chìa). Dùng được cho FBX khác của Meshy nếu rương nhìn từ phía có khóa hướng về -Y.

- Cấu trúc: root `Chest_Golden` gồm `Body` (3,4 nghìn tam giác), `Lid` (2,2 nghìn, **gốc ở bản lề phía sau**), `Glow` (quầng sáng vàng, ẩn bằng scale) và `Key` (chìa khóa bạc, đặt sẵn ở tư thế đã cắm vào ổ, thân chìa dọc trục Y của Blender). Mặt cắt có viền gỗ tối và một **đệm lót đỏ đô** vát cạnh (nổi lên trong thân, treo dưới nắp), thay cho mặt phẳng tô đơn sắc lúc đầu bị nứt màu.
- **Hiệu ứng chìa khóa:** `BoxOpenSequence` tự chạy khi model có con `Key` (bật/tắt bằng `keyIntro`): chìa bay từ phía trước tới (0,8 s), cắm vào ổ (0,25 s), xoay 90° (0,45 s) rồi rút ra (0,3 s) **trước** giai đoạn rung. Có sự kiện `onKeyApproach`, `onKeyInserted`, `onKeyTurned` để gắn âm thanh. Trục xoay `keyTurnAxis` mặc định `Vector3.forward` (thân chìa). Vị trí ổ khóa được tìm tay từ ảnh cận.
- **Không có `Seal` và `Binding`**: khóa và đai vàng nằm trong texture. `BoxOpenSequence` bỏ qua lớp thiếu, nên giai đoạn "Gỡ niêm" sẽ không thấy gì rơi ra. Cần chỉnh nhịp (ví dụ rút ngắn giai đoạn 2) hoặc thêm hiệu ứng hạt.
- Texture ép xuống 1024 (từ 2048), có normal, metallic/roughness gộp theo chuẩn glTF và emission cho viền vàng.
- Ảnh kiểm tra đóng/mở: `Art/Source/render_chest_golden_open.png`. Ảnh dựng bằng bộ vẽ phần mềm, tô màu theo texture, **không phải ảnh chụp từ Unity**, nên chưa kiểm chứng ánh sáng, vật liệu hay chiều mở nắp trong Unity (đổi `lidOpenSign` nếu mở ngược).
- Chưa tạo prefab, chưa gắn `BoxOpenSequence`, chưa chạy trong Unity. Đoạn mã chìa khóa mới viết, **chưa biên dịch** vì máy này không có Unity hay trình biên dịch C#.

## Rương Thiên Cơ (dựng bằng Blender, tạm)

`Assets/_Project/Art/Chests/Chest_ThienCo.glb`, **bản thử tạm**, chưa gán ID hay chủ đề vào dữ liệu game. Ý tưởng: [spec](../../docs/superpowers/specs/2026-11-24-ruong-thien-co-design.md). Dáng **tên lửa cơ khí tách đôi**: thân dưới nửa cầu có loa phụt và ba vây xuôi (đứng trên vây, không đế), nắp là mũi tên lửa thuôn nhọn có chóp ăng-ten, xích đạo là vành răng đồng, mặt trước nắp có mắt máy cyan (gợi TC2), bên trong có lõi cyan (gợi TC3).

```
pip install bpy pillow
python Tools/Blender/make_chest_thien_co.py -- Assets/_Project/Art/Chests/Chest_ThienCo.glb Assets/_Project/Art/Source/render_chest_thien_co_open.png 48 640
```
(Bỏ hai tham số cuối và đường dẫn ảnh nếu chỉ cần GLB.)

- Lớp: root `Chest_ThienCo` gồm `Body` (1.036 tam giác), `Lid` (1.056, **gốc ở bản lề phía sau**, xoay quanh X, mặt trước là -Y), `Glow` (60, đĩa cyan ở miệng, ẩn bằng scale), `Seal` (68, núm khóa xoay ở mặt trước xích đạo). Tổng **2.220 tam giác**, cao ~0,52 m (kể cả chóp ăng-ten), vật liệu đơn sắc, cyan dùng emission, không texture.
- Không có `Key`, `Binding`.
- Ảnh `Art/Source/render_chest_thien_co_open.png`: đóng, hé, mở toang, và 3 góc hình bóng; dựng ngoại tuyến bằng Cycles, **không phải ảnh từ Unity**. Chưa tạo prefab, chưa chạy trong Unity (đổi `lidOpenSign` nếu nắp mở ngược).
- Điểm còn yếu: mặt trong nắp khi mở toang còn sáng xám, đĩa Glow phẳng và to.

## Rương Magic (đá và tinh thể tím, khóa lục giác)

`Assets/_Project/Art/Chests/Chest_Magic.glb`, **chưa gán chủ đề hay ID rương** vì danh sách chủ đề chưa chốt. Nguồn: FBX Meshy do chủ dự án cung cấp ở `Art/Source/MeshyChests/Magic/` (10,1 nghìn tam giác, Git LFS; bản game giảm còn ≈5 nghìn; chạy `git lfs pull`), không tốn credit mới. Tạo bằng cùng script với Golden, thêm bốn tham số tùy chọn (màu glow, màu lót, cỡ chìa, số tam giác tối đa; bỏ trống thì ra y như Golden):

```
python cut_lid_from_meshy_fbx.py -- Assets/_Project/Art/Source/MeshyChests/Magic Assets/_Project/Art/Chests/Chest_Magic.glb Chest_Magic -0.13 0.6 1024 "0,-0.70,-0.10" "0.62,0.28,1.0" "0.05,0.02,0.11" 2.0 4300
```

- **Đường nối** z = -0,13 (đơn vị nguồn): rãnh tím giữa hai đai vàng, chỗ ít đỉnh nhất (13 đỉnh) nên các viên tinh thể không bị cắt đôi. Ổ khóa lục giác nằm vắt qua đường nối (nửa trên theo nắp), giống Golden.
- **Hướng:** cả hai mặt ±Y đều có khóa nên không cần xoay; mặt trước vẫn là -Y.
- **Kích thước:** cao 0,6 m (cả tinh thể), rộng ≈0,57 m, gần cỡ Golden (0,60 × 0,42). Cao hơn Golden vì tinh thể nhô lên.
- **Cấu trúc:** root `Chest_Magic` gồm `Body` (2,2 nghìn tam giác), `Lid` (2,8 nghìn, gốc ở bản lề sau), `Glow` (quầng sáng **tím**), `Key` (chìa bạc cỡ 2,0, to hơn Golden vì ổ khóa lớn). Đệm lót **tím sẫm**. Hiệu ứng chìa dùng nguyên `BoxOpenSequence`, không đổi code.
- **Tổng ≈5,06 nghìn tam giác:** script giảm mặt (Decimate collapse) từ 10,1 nghìn xuống 4,3 nghìn trước khi cắt, cộng đệm lót, glow, chìa. Hình dạng, viền vàng và tinh thể gần như không đổi so với bản 11 nghìn (xem ảnh kiểm tra); chỉ các mặt đá hơi thô hơn khi nhìn cận.
- Texture ép 1024, có normal và metallic/roughness. Không có `Seal`, `Binding` (khóa và đai nằm trong texture).
- Ảnh kiểm tra (dựng bằng Cycles CPU: chìa tới gần, cắm, xoay, hé nắp, đóng, mở toang): `Art/Source/render_chest_magic_open.png`. Bộ dựng: `Tools/Blender/cycles_chest_preview_setup.py` (đặt `CHEST_GLB`, `CHEST_EXPOSURE`).

**Giới hạn:**
- Chưa chạy trong Unity, chưa tạo prefab; ảnh xem trước là dựng ngoại tuyến bằng Blender, ánh sáng và vật liệu trong Unity có thể khác. Có thể cần đổi `lidOpenSign` nếu nắp mở ngược.
- Đoạn mã C# (chìa khóa) chưa biên dịch trên máy dựng.
- Script đã đổi để nhận màu, nhưng FBX gốc của Golden không nằm trong repo nên chưa chạy lại để so từng byte; giá trị mặc định giữ nguyên từng chữ.
- Nắp mở 105° nhìn khá lớn vì nắp vòm dày (6,4 nghìn tam giác); nên chỉnh nhịp hoạt ảnh khi gắn vào game.
- Mặt cắt thân là tấm lót phẳng che ~80% miệng rương, nên bên trong trông nông.
- **Lưu ý cho chủ dự án:** phong cách Magic (tím rực, đá sáng, viền vàng bóng) khác hướng low-poly màu trung tính của repo; đặt cạnh các món low-poly có thể lệch tông, cần quyết định giữ hay đổi.

## Rương Mimic (rương quái vật, thử nghiệm)

`Assets/_Project/Art/Chests/Chest_Mimic.glb`, **chưa gán chủ đề hay ID rương** vì danh sách chủ đề chưa chốt. Nguồn: FBX Meshy do chủ dự án cung cấp (`Art/Source/MeshyChests/Mimic/`, 359 nghìn tam giác, texture 4K, không tốn credit mới). Tạo bằng `Tools/Blender/make_mimic_chest.py`, xem ảnh bằng `Tools/Blender/cycles_mimic_preview.py`:

```
python make_mimic_chest.py -- <thư mục fbx+png> Assets/_Project/Art/Chests/Chest_Mimic.glb Chest_Mimic 0.42 3500 3500 2048 33
```

- **Cấu trúc:** root `Chest_Mimic` gồm `Body` (3,5 nghìn tam giác), `Lid` (3,5 nghìn, **gốc ở bản lề phía sau**, xoay quanh trục X, mặt trước là -Y, mở bằng góc âm quanh X như Golden) và `Glow` (quầng đỏ cam trong miệng, ẩn bằng scale). **Tổng 7,0 nghìn tam giác**, kích thước đóng khoảng 0,51 x 0,44 x 0,42 m. Không có `Key`, `Seal`, `Binding` (rương quái không có ổ khóa; ổ khóa trên thân và nắp nằm trong texture).
- **Cách tách nắp (phương án a):** FBX gốc đã mở sẵn (nắp mở khoảng 33° quanh bản lề), là một mesh liền. Nắp và thân nối nhau chỉ ở dải tiếp xúc rất mỏng nên tách theo **tính liên thông**: đỉnh cao hơn z = -0,2 (đơn vị nguồn) thuộc cùng khối lớn là nắp (gồm răng trên), mảnh còn lại là thân (răng nanh hai bên thân nhô cao), dải tiếp xúc chia theo lan từ hai phía. Gốc nắp đặt ở tâm hai đĩa bản lề (y = 0,625, z = -0,186 đơn vị nguồn). Sau đó xoay nắp **đóng lại 33°** để rương bắt đầu như rương thường. Ảnh so sánh cùng tư thế 33° cho thấy nắp khớp với mô hình gốc, tức tâm xoay đúng.
- **Răng khi đóng:** răng trên và răng dưới đầu mút gần chạm nhau, có vài khe tối, nhìn không thấy xuyên rõ ràng; **chưa đo xuyên bằng số** (phép đo theo pháp tuyến không tin cậy vì thân rỗng). Nếu thấy xuyên trong Unity, cho `lidOpenSign` hé 1 đến 2° hoặc chạy lại với góc đóng 31°.
- **Giảm tam giác:** Decimate Collapse riêng từng nửa (nắp 211 nghìn xuống 3,5 nghìn, thân 148 nghìn xuống 3,5 nghìn; thử 7, 5 rồi 3,5 nghìn mỗi nửa đều thấy răng vẫn rõ nên giảm dần theo yêu cầu). Răng vẫn rõ nét (ảnh `Art/Source/render_chest_mimic_decimate.png`: trước 359 nghìn, sau 7 nghìn, cùng tư thế). Bật "giữ biên UV" (`delimit UV`) cho kết quả y hệt nên không dùng. Một mảng gỗ mờ ở thân là do texture Meshy gốc.
- **Texture:** 4096 ép xuống 2048, một vật liệu, chỉ màu cơ bản (nguồn không có normal hay roughness), xuất JPEG 88 nên file glb khoảng 1,1 MB. Cycles cho thấy 2048 còn rõ răng và đai sắt.
- **Ảnh xem trước:** `Art/Source/render_chest_mimic_open.png` (đóng, hé 12°, há miệng 65°), dựng bằng Cycles ngoại tuyến, **không phải ảnh chụp từ Unity**; ánh sáng, vật liệu và chiều xoay nắp chưa kiểm chứng trong Unity. Glow trong ảnh hơi nhạt, ra màu cá hồi vì tông màu AgX.
- **Gợi ý hiệu ứng riêng:** rung mạnh, rồi nắp **há ra và đóng sập một lần như cắn** (kèm rung cả hộp và âm thanh), sau đó mới chạy các giai đoạn mở thường: hé nắp, bật nắp, há toang với `Glow` đỏ cam. Góc há toang cuối nên khoảng 60° đến 75° thay vì 105°, vì nắp dày và răng dài. Đã thêm tuỳ chọn `biteIntro` vào `BoxOpenSequence` (mặc định tắt nên các rương khác không đổi hành vi): `biteCount`, `biteOpenAngle`, `biteOpenTime`, `biteHoldOpen`, `biteSlamTime`, `biteSlamShake`, `biteRestTime` và sự kiện `onBite` (đúng lúc nắp sập, gắn âm thanh). Đoạn mã này **chưa biên dịch** vì máy này không có Unity hay trình biên dịch C#.
- **Lưu ý cho chủ dự án (phong cách):** Mimic là mô hình thực tế, có răng chi tiết, texture 2048 và tông gỗ sẫm, sắt cũ, khác hướng low-poly màu trung tính của repo. Đặt cạnh các rương và đồ khác có thể lệch phong cách; cần chủ dự án quyết định giữ hay làm lại theo hướng low-poly.
- Chưa tạo prefab, chưa gắn `BoxOpenSequence`, chưa chạy trong Unity.

## Rương Miner (thợ mỏ, rương gỗ cũ có cuốc chim chéo)

`Assets/_Project/Art/Chests/Chest_Miner.glb`, **chưa gán chủ đề hay ID rương** vì danh sách chủ đề chưa chốt. Nguồn: `Art/Source/MeshyChests/Miner/` (FBX Meshy của chủ dự án, 369.686 tam giác, không tốn credit mới). Tạo bằng cùng script với Golden, có thêm các tham số tuỳ chọn (mặc định tắt, Golden tái tạo y như cũ):

```
python cut_lid_from_meshy_fbx.py -- <thư mục fbx và png> Assets/_Project/Art/Chests/Chest_Miner.glb Chest_Miner 0.376 0.32 1024 "" --size-xy 0.85,0.42 --decimate-tris 3000 --lining 0.06,0.04,0.022 --rim 0.09,0.05,0.025 --glow 1.0,0.5,0.12
```

- **Tham số cắt:** đường nối nắp z = 0,376 (đơn vị nguồn; Z là trục đứng). Đó là chỗ mép dưới tấm nắp ván bắt đầu loe ra khỏi thân (thân thẳng đứng đến 0,375, nắp loe từ 0,376). Chủ dự án **cho phép bóp méo**, nên rương được co không đều: cao **0,32 m**, dài **0,85 m**, sâu **0,42 m** (Golden cao 0,42, rộng 0,6; nguồn gốc cao 0,96, dài 2,0, sâu 0,98, tức chiều cao bị nén khoảng 0,78 lần so với chiều dài). Hình cuốc chim và đá hơi dẹt nhưng vẫn nhìn tự nhiên. Texture 1024. Không có ổ khóa nên bỏ trống tham số vị trí khóa, **không có `Key`**.
- **Hướng:** FBX nhập vào đã có mặt trước là -Y (cuốc chim chéo, đá quặng). Mặt sau (+Y) cũng có cuốc chim, nên sau này đặt rương quay mặt nào cũng đẹp. Không cần xoay.
- **Giảm poly (Decimate kiểu Collapse, chạy trước khi cắt):** 369.686 xuống 3.000 tam giác. Sau khi cắt: `Body` 2,9 nghìn (gồm mặt cắt và đệm lót), `Lid` 0,54 nghìn, **tổng khoảng 3,4 nghìn** (chủ dự án chọn mức này, thấp hơn mục tiêu ban đầu 8 đến 15 nghìn). Đã thử ba mức: 12,8k (đẹp nhất, vân nắp còn), 6,6k và 3,4k (đang dùng). Ở 3,4k đai sắt và chân sắt còn rõ, nhưng mặt nắp và đá hơi vỡ mặt, cuốc chim hơi méo. Muốn chi tiết hơn: đổi `--decimate-tris` (6000 cho 6,6k, 12000 cho 12,8k). Texture 2048 ép xuống 1024 vẫn nét.
- **Cấu trúc:** root `Chest_Miner` gồm `Body`, `Lid` (gốc ở **bản lề phía sau**, xoay quanh trục X, tấm nắp ván phẳng dày khoảng 0,1 nguồn), `Glow` (quầng sáng cam ấm, ẩn bằng scale). Đệm lót nâu sẫm kiểu vải bố, viền mặt cắt gỗ rất tối. `BoxOpenSequence` bỏ qua lớp thiếu nên không cần `Key`, `Seal`, `Binding`; chưa kiểm tra giai đoạn "Gỡ niêm" trông thế nào khi không có lớp nào (giống Golden).
- **Tham số mới của script:** `--decimate-tris N`, `--size-xy dài,sâu` (bóp méo X/Y riêng, mét), `--lining r,g,b`, `--rim r,g,b`, `--glow r,g,b` (màu tuyến tính 0 đến 1). Cũng sửa lỗi nhỏ: đường ra chỉ là tên file không thư mục thì không còn báo lỗi. `Tools/Blender/cycles_chest_preview_setup.py` nay nhận `--glb <file>` và `--size <bề rộng m>`, chịu được rương không có `Key`.
- **Ảnh xem trước:** `Art/Source/render_chest_miner_open.png` (nguồn gốc 375 nghìn tam giác (cùng kích thước đã bóp), đóng nắp, hé nắp 22 độ, mở toang 105 độ), dựng ngoại tuyến bằng Cycles CPU trong Blender, **không phải ảnh từ Unity**.

**Giới hạn và lưu ý cho chủ dự án:**
- Chưa chạy trong Unity, chưa tạo prefab, chưa gắn `BoxOpenSequence`; đoạn mã C# (kể cả phần chìa khóa của Golden) **chưa biên dịch**. Chiều mở nắp, vật liệu và ánh sáng trong Unity chưa kiểm chứng (đổi `lidOpenSign` nếu nắp mở ngược).
- Tỉ lệ đã bóp méo theo cho phép của chủ dự án (xem trên). Muốn dẹt hơn nữa chỉ cần đổi tham số `<chiều cao>` và `--size-xy`, nhưng hình khắc sẽ méo rõ hơn.
- Nắp là một tấm ván phẳng dày, khi hé thấy bên trong chỉ là đệm lót phẳng (mô hình nguồn là khối đặc, không có lòng rương thật), giống Golden.
- Phong cách **thực tế, tối và nhiều chi tiết hơn** hướng low-poly màu trung tính của repo; đứng cạnh đồ low-poly sẽ lệch tông. Chủ dự án cần quyết có giữ hướng này cho rương Meshy hay không.
- Glow đang có lõi sáng gần trắng vàng trong ảnh dựng vì tông màu của bộ dựng; trong Unity màu sẽ khác.

## Rương Rác Quý (dựng bằng Blender)

`Assets/_Project/Art/Chests/Chest_RacQuy.glb`, **bản dựng thử, chưa gán ID hay chủ đề** vào dữ liệu game. Ý tưởng và lý do chọn hình: [spec rương Rác Quý](../../docs/superpowers/specs/2026-11-24-ruong-rac-quy-design.md). Dựng hoàn toàn bằng script, không dùng Meshy.

Hình dáng: **thùng rác tròn** thân côn loe miệng (rộng 0,35 m ở vành, cao 0,45 m tính cả quai nắp), hai đai nổi, thân móp một vết phía trước bên phải, nắp vòm có quai và một chiếc vương miện giấy rẻ tiền đội lệch, tem giấy "QUÝ" dán chéo, xương cá thò ra mép phía sau, trong lòng có lon bẹp và dép tổ ong. Màu xanh rêu xỉn, vật liệu đơn sắc, không texture.

| Lớp | Tam giác | Nội dung |
|---|---|---|
| `Body` | 1.458 | Thân, đai, tai cầm, bản lề, tem "QUÝ", xương cá, lòng tối và rác bên trong |
| `Lid` | 406 | Nắp vòm, quai, vương miện giấy. **Gốc ở bản lề phía sau**, xoay quanh trục X, mặt trước là -Y |
| `Seal` | 72 | Chiếc tất thối vắt qua vành, phía trước bên trái |
| `Binding` | 72 | Hai dải băng dính bạc dán chéo qua khe nắp ở mặt trước |
| `Glow` | 252 | Hơi thối xanh vàng (tự phát sáng nhẹ) **và ba con ruồi** trên miệng thùng; ẩn bằng scale như các hộp khác |

Tổng **2.008 tam giác** (không tính `Glow`). Không có lớp `Key`, nên `BoxOpenSequence` bỏ qua phần chìa khóa.

Hiệu ứng mở đặc trưng: hơi và ruồi nằm chung trong `Glow`, nên khi `BoxOpenSequence` phóng to `Glow` theo các giai đoạn thì hơi thối và ruồi "nở" ra từ miệng thùng, **không cần sửa C#**. Giai đoạn "Gỡ niêm" làm tất và băng dính rơi ra. Gợi ý âm thanh: nắp tôn kêu kẹt, ruồi vo ve ở `onStageReached`. Đèn trong (`innerLight`) nên đặt màu xanh vàng thay vì vàng ấm.

Chạy lại (cần `pip install bpy pillow`, Python 3.13, Blender 5.3 dạng module; các hộp `.glb` lấy qua `git lfs pull`):

```
python Tools/Blender/make_chest_rac_quy.py                         # ghi đè Chest_RacQuy.glb, in số tam giác và chiều cao
python Tools/Blender/render_chest_rac_quy_preview.py -- <thư mục tạm> 64 640   # ảnh Cycles
```

Các số chỉnh nhanh nằm ở khối `CONFIG` đầu script (số cạnh `SEGS`, chiều cao, bán kính, vết móp `DENT`, vị trí tất `SOCK_ANGLE`, bảng màu).

- Ảnh kiểm tra: `Art/Source/render_chest_rac_quy_open.png` (dựng ngoại tuyến bằng Cycles, **không phải ảnh từ Unity**): hàng 1 đóng, hé 25°, mở toang 110°; hàng 2 nhìn trước, bên phải, sau; hàng 3 hình bóng đen cạnh H1, H2, H3, RD.
- Chưa nhập vào Unity, chưa tạo prefab, chưa gắn `BoxOpenSequence`. Nắp có thể mở ngược chiều trong Unity: đổi `lidOpenSign`. Vật liệu glTF tự phát sáng của hơi thối chưa kiểm tra trong URP.

## Rương Hải tặc (dựng bằng Blender)

`Assets/_Project/Art/Chests/Chest_HaiTac.glb`, **chưa gán ID hay chủ đề vào dữ liệu game**. Dựng hoàn toàn bằng script, không dùng Meshy. Ý tưởng và lý do chọn dáng: [ruong-hai-tac](../../docs/superpowers/specs/2026-11-24-ruong-hai-tac-design.md).

- **Dáng "Đầu lâu thuyền trưởng"**: hộp sọ đội mũ ba góc viền vàng, bịt một mắt, răng vàng, ngồi trên đống xu vàng đặt trên hai xương bắt chéo. Không có mặt hộp vuông nào; hình bóng là lá cờ Jolly Roger dựng thành khối. Mở rương là "mở não": đỉnh sọ cùng cái mũ lật ra sau.
- **Số tam giác:** 2.844 (không tính Glow): Body 1.514, Lid 894, Binding 292, Key 96, Seal 48; Glow 260. Cao 0,49 m. Vật liệu đơn sắc, không texture.
- **Cấu trúc lớp** (con của root `Chest_HaiTac`):

| Lớp | Nội dung |
|---|---|
| `Body` | Phần sọ dưới đường cắt ngang trán, hàm, răng, miếng bịt mắt, đống vàng, xương chéo, miệng tối |
| `Lid` | Đỉnh sọ và mũ ba góc, **gốc ở bản lề sau gáy**, xoay quanh trục X, mặt trước là -Y |
| `Seal` | Dấu sáp đỏ trên dây thừng, bên trái trán |
| `Binding` | Vòng dây thừng quanh đường cắt (nút ở gáy) và ổ khóa đồng treo giữa trán |
| `Glow` | Quầng vàng ở miệng sọ kèm 5 đồng xu, ẩn bằng scale |
| `Key` | Chìa sắt đã cắm sẵn vào ổ khóa, thân chìa dọc trục Y của Blender, gốc ở chỗ chìa vào ổ |

- **Chạy lại:** `pip install bpy pillow` (Python 3.13, Blender 5.3), rồi
  - `python Tools/Blender/make_chest_hai_tac.py` (ra GLB, in số tam giác và chiều cao; thêm `-- <đường dẫn .glb>` để xuất chỗ khác),
  - `python Tools/Blender/render_chest_hai_tac_preview.py -- 64 640` (số mẫu Cycles, bề rộng mỗi khung).
- **Ảnh xem trước:** `Art/Source/render_chest_hai_tac_open.png`, dựng ngoại tuyến bằng Cycles (ánh sáng theo `cycles_chest_preview_setup.py`): hàng trên đóng nắp, hé nắp, mở toang; hàng dưới nhìn trước, bên cạnh, sau 3/4.
- **Dùng 5 giai đoạn sẵn có của `BoxOpenSequence`, không sửa C#.** Chưa chạy trong Unity, chưa tạo prefab. Nắp có thể mở ngược sau khi nhập: đổi `lidOpenSign`. Ổ khóa nằm cao trên trán và dây thừng là vòng kín, nên khi `Binding` rơi (dịch xuống và xoay quanh trục bản lề) cần xem trong Unity có xuyên qua mặt sọ không.

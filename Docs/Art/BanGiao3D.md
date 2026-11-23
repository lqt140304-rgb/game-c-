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

# Thiết kế: hiệu ứng và âm thanh mở rương (lõi dùng chung)

Trạng thái: **lõi đã dựng, chưa chạy trong Unity**. Điểm chung (a), (b), (c) dưới đây là **chủ dự án đã chốt**. Bảng màu bậc, số tia, nhịp giây và mọi âm thanh là **đề xuất**, chờ chủ dự án duyệt.

Liên quan: [tier-do-trong-ruong](2026-11-23-tier-do-trong-ruong-design.md) (5 bậc), [ruong-chu-de](2026-11-24-ruong-chu-de-design.md) (quay bậc rồi quay món), [BanGiao3D](../../../Docs/Art/BanGiao3D.md) (mô hình rương, `BoxOpenSequence`).

## Đã chốt với chủ dự án

Khi mở **mọi** rương:

- **(a)** Các sắc màu của những **bậc có thể ra** trong rương đó nhảy liên tục (như vòng quay, máy xèng), chậm dần, rồi **dừng đúng màu của bậc vừa quay ra**.
- **(b)** **Cùng lúc**, bóng (hình bóng) của vật phẩm xuất hiện từ trong rương, **to dần**, kết thúc ở **hình dạng hoàn chỉnh** (kích thước đầy đủ, lơ lửng trên rương).
- **(c)** Mỗi rương có **kiểu hiệu ứng và âm thanh riêng**. Phiên này chỉ dựng lõi; bộ riêng của từng rương làm ở các phiên sau.

## Kết quả do game quyết

- Bước 1 "quay bậc" (spec rương chủ đề) do **game** làm bằng hạt giống ván. Hiệu ứng **chỉ nhận** kết quả: `ChestRevealSequence.Play(rolledTier, itemSource)`.
- Lịch nhảy màu được dựng **ngược từ cuối**: bước cuối luôn là `rolledTier`, các bước trước điền lùi theo kiểu nhảy. Không có dòng mã nào trong hiệu ứng đổi `rolledTier`.
- Ngẫu nhiên (`System.Random`, hạt `decorationSeed`, 0 là mỗi lần một khác) **chỉ dùng để trang trí**: thứ tự kiểu Shuffle, có "suýt trượt" hay không, nhịp tiếng xu. Nếu bậc truyền vào không nằm trong danh sách bậc của rương, hiệu ứng thêm nó vào vòng quay và báo cảnh báo, chứ không đổi kết quả.

## Thành phần (thư mục `Assets/_Project/Scripts/ChestFx/`, namespace `ChoDoLa.ChestFx`)

| File | Vai trò |
|---|---|
| `ChestTier.cs` | enum `ChestTier` (Rac, Thuong, Hiem, RatHiem, HuyenThoai), hạng 0–4, tên hiển thị tiếng Việt |
| `TierPalette.cs` | ScriptableObject màu và dấu hình dạng của 5 bậc; để trống thì dùng giá trị mặc định (đề xuất) |
| `ChestFxProfile.cs` | ScriptableObject hồ sơ riêng một rương: âm, hạt, vòng quay, tia, prefab tuỳ biến |
| `ChestFxHook.cs` | Lớp nền cho hiệu ứng tuỳ biến bằng code (ghi đè hàm cần dùng) |
| `ChestRevealSequence.cs` | Component chính, đặt cạnh `BoxOpenSequence` trên gốc rương |
| `ChestSilhouette.cs` | Dựng bóng: chỉ chép lưới (mesh) của món, vật liệu Unlit, viền sáng kiểu "inverted hull" |
| `ChestFxFactory.cs` | Dựng vật liệu, texture hạt, lưới tia và vòng, ParticleSystem **bằng code** |
| `ChestFxPresets.cs` | Giá trị hồ sơ mẫu (hiện có `ApplyGolden`) và tên file âm |
| `GoldenGlintHook.cs` | Ví dụ hook riêng của Chest_Golden: lấp lánh vàng quanh miệng rương khi ra Hiếm trở lên |
| `ChestRevealDemo.cs` | Bàn thử (phím T, Y, Space, R, C) |
| `Editor/ChestRevealTestSetup.cs` | Menu `Tools/Cho Do La/Add Chest Reveal Test` và `Create Golden Chest FX Profile` |

`BoxOpenSequence.cs` chỉ thêm thuộc tính đọc `Stages` (để tính giờ). Rương **không** có `ChestRevealSequence` chạy y như cũ.

### API

```csharp
reveal.Setup(new[] { ChestTier.Rac, ChestTier.Thuong, ChestTier.Hiem }); // các bậc rương này có thể ra
reveal.Play(rolledTier, itemModel);   // rolledTier do game quay; itemModel chỉ dùng để dựng bóng
reveal.ResetReveal();                 // dọn hiệu ứng, đóng nắp
```

Sự kiện: `onRoulette`, `onRouletteStep(ChestTier)`, `onTierLocked(ChestTier)`, `onSilhouetteComplete` (game thay bóng bằng vật thật tại đây), `onRevealed` (nắp mở xong **và** bóng hoàn chỉnh).

## Thứ tự nhịp (mặc định)

Cắm vào `BoxOpenSequence.onStageReached`: **bắt đầu quay ở "Hé nắp" (2)**, **khoá màu ở "Bật nắp" (3)**, **bóng đủ cỡ trước "Mở toang" (4)**. Số giai đoạn chỉnh được trong Inspector (`rouletteStartStage`, `lockStage`, `silhouetteCompleteStage`). Vòng quay cần tối thiểu `minRouletteSeconds` (2,8 s); nếu thiếu, hiệu ứng **kéo dài thời gian giữ** của giai đoạn bắt đầu (chỉ khi rương có `ChestRevealSequence`, trả lại như cũ khi `ResetReveal`). Tương tự `minGrowSeconds` (1,1 s) cho bóng sau khi khoá.

Mốc giây với thời gian giai đoạn mặc định của `BoxOpenSequence`, **rương không có chìa** (Chest_Golden có `Key` nên **cộng thêm 2,15 s** cho đoạn chìa khóa):

| Giây | Sự kiện |
|---|---|
| 0,0 | `Play`: rương bắt đầu rung |
| 0,9 – 2,5 | Rung, Gỡ niêm (không đổi) |
| **3,7** | Hé nắp: **bắt đầu quay màu** (tick nhanh 0,05 s), âm nền lấp lánh, tiếng kẽo kẹt nắp, hạt bay lên theo màu, đèn trong rương nháy theo nhịp, bóng hiện ra rất nhỏ (4%) và lớn dần tới 22% ở miệng rương. Giữ "Hé nắp" được kéo từ 0,8 s lên 2,0 s |
| 3,7 – 6,5 | Màu nhảy chậm dần (khoảng tick tới 0,45 s), cao độ tick tăng dần; có thể "suýt trượt" sang bậc kế rồi lùi về |
| **6,5** | Bật nắp: **khoá màu**. Chớp sáng, tia sáng (số tia và hình theo bậc), vòng sóng (0–3 vòng), bùng hạt, tiếng chốt theo bậc, tiếng vút, tiếng xu rơi (Golden). Bóng bật ra khỏi rương |
| 6,5 – 7,7 | Bóng phóng to tới `targetSize` (0,3 m), bay lên tới khi đáy cách miệng rương nửa chiều cao rương, nảy nhẹ cuối (ease out back), xoay chậm 30°/s, viền chập chờn |
| 7,7 | Mở toang: tiếng nắp chạm |
| **8,0** | `onSilhouetteComplete` (sau 0,3 s đứng yên) |
| **8,3** | Nắp mở xong (`onOpened`) → `onRevealed` |

Không có `BoxOpenSequence`: quay 2,8 s, bóng lớn 1,4 s, rồi xong.

## Bảng màu bậc (ĐỀ XUẤT, chờ duyệt)

Mỗi bậc khác nhau **cả hình dạng**, không chỉ màu, để người mù màu một phần vẫn phân biệt được:

| Bậc | Màu đề xuất | Tia khi khoá | Vòng sóng | Số chấm quay quanh rương | Tiếng chốt |
|---|---|---|---|---|---|
| Rác | xám nâu `#8A7D6B` | 3 tia ngắn cụt | 0 | 1 | "bốp" + kèn tụt giọng (hài) |
| Thường | trắng ngà `#F3ECD6` | 4 tia thẳng (dấu cộng) | 1 | 2 | một tiếng chuông |
| Hiếm | xanh dương `#3D8BFF` | 6 tia nhọn | 1 | 3 | hai nốt chuông đi lên |
| Rất hiếm | tím `#A64DFF` | 8 tia hình thoi | 2 | 4 | rải hợp âm 4 nốt + nền |
| Huyền thoại | vàng cam `#FFA41C` | 12 tia hình sao dài ngắn xen kẽ | 3 | 5 | tiếng ầm, rải 6 nốt, hợp xướng, lấp lánh (dài nhất, 3,4 s) |

- **Số chấm** (quả cầu nhỏ quay quanh rương) = hạng bậc đang hiển thị, đổi theo từng nhịp nhảy màu, nên đọc được bậc cả trong lúc quay.
- Độ sáng cũng tách bậc: Rác tối và đục, Thường rất sáng, Huyền thoại có cường độ đèn và số hạt lớn nhất.
- Lam (Hiếm) và tím (Rất hiếm) gần nhau với người mù màu lam-vàng: phân biệt bằng số tia (6 nhọn với 8 thoi), số vòng (1 với 2) và số chấm (3 với 4).

## Bóng vật phẩm

- Chép **chỉ lưới** của món (MeshFilter, SkinnedMeshRenderer), không chép script, collider, đèn. `itemSource` có thể là prefab asset hoặc vật trong cảnh; không bị sửa.
- Kích thước: cạnh dài nhất của khối bao = `targetSize`.
- Kiểu mặc định **lõi tối màu bậc + viền sáng màu bậc** (`DarkTintWithRim`): lõi là màu bậc nhân 0,16, viền là bản phóng to 5% chỉ vẽ mặt sau (shader `Universal Render Pipeline/Unlit`, Cull Front). Có thêm `BlackWithRim` (lõi đen) và `DarkTintOnly`. Trong lúc quay, viền đổi theo màu đang nhảy; sau khi khoá, giữ màu bậc trúng.
- Hình chỉ là bóng: game tự thay bằng vật thật khi nhận `onSilhouetteComplete`.

## Âm thanh: âm thanh tạm tổng hợp bằng code

**Nhãn: âm thanh tạm tổng hợp bằng code.** Tất cả file ở `Assets/_Project/Audio/Chests/` do `Tools/Audio/make_chest_sfx.py` tạo bằng numpy (sóng sin, nhiễu, lọc), **không dùng mẫu có bản quyền, không dùng dịch vụ trả phí**. WAV mono 44,1 kHz 16-bit, tổng khoảng 1,4 MB. Thay bằng âm thật sau này chỉ cần giữ tên file hoặc kéo clip mới vào hồ sơ.

| File | Dùng cho |
|---|---|
| `sfx_chest_tick_a/b.wav` | Tick mỗi lần đổi màu (Unity nâng cao độ dần) |
| `sfx_chest_lock_0_rac` … `_4_huyenthoai.wav` | Tiếng chốt theo bậc |
| `sfx_chest_whoosh.wav` | Bóng bật ra và phóng to |
| `sfx_chest_open_shimmer.wav` | Nền lấp lánh lúc quay |
| `sfx_chest_lid_creak.wav`, `sfx_chest_lid_thud.wav` | Kẽo kẹt lúc hé nắp, tiếng nắp mở hết |
| `sfx_coin_clink_1..3.wav` | Xu lanh canh (Chest_Golden), số tiếng tăng theo bậc |

Clip nào để trống trong hồ sơ thì **bỏ qua, không lỗi**.

## Cách thêm hiệu ứng cho một rương mới

1. Thêm hàm `ApplyTenRuong(ChestFxProfile p)` vào `ChestFxPresets.cs` (hoặc tạo asset bằng *Assets > Create > Cho Do La > Chest FX Profile* rồi chỉnh tay). Đặt `chestId` = tên file GLB, ví dụ `Chest_ThienCo`.
2. Chọn kiểu: `rouletteStyle` (Cycle, PingPong "xếp hàng", Shuffle), `minInterval`/`maxInterval`/`slowdownCurve` (tốc độ), `particleLook` (SoftDot, Sparkle, Coin, Streak), `particleDirection` (Up, Radial, Swirl, Fountain), `secondaryColor`, `burstGravity`, `raySpin`…
3. Âm riêng: thêm hàm tạo âm vào `make_chest_sfx.py` (tên `sfx_<ruong>_*.wav`), kéo vào hồ sơ (`tickClips`, `lockClipsByTier`, `accentClips`, `stageClips`).
4. Hiệu ứng đặc biệt bằng code: viết lớp con `ChestFxHook` (xem `GoldenGlintHook`), gắn lên gốc rương hoặc lên prefab đặt ở `customFxPrefab`.
5. Trên gốc rương: `BoxOpenSequence` + `ChestRevealSequence` (kéo hồ sơ vào `profile`) + `AudioSource`.

## Rương sẽ cần hiệu ứng riêng (phiên sau)

Theo [ruong-chu-de](2026-11-24-ruong-chu-de-design.md) và mô hình hiện có. Ý tưởng chỉ là gợi ý, chưa duyệt.

| Rương | Mô hình | Gợi ý kiểu |
|---|---|---|
| Golden Treasure (thử nghiệm) | `Chest_Golden.glb` | **Đã có hồ sơ mẫu**: tia vàng, xu bắn lên rồi rơi, xu lanh canh |
| Thiên Cơ | `Chest_ThienCo.glb` | tia cyan, tiếng bíp máy, hạt Streak bắn lên như phụt tên lửa |
| Gỗ, Vải Niêm, Kim Khí, Đen (4 rương cơ bản) | chưa có | mộc mạc; vải xào xạc; tiếng kim loại; khói tím |
| Việt Nam, Tuổi thơ Việt Nam, Wibu, Truyền thuyết Châu Âu, Robot, Không gian, Game, Thể thao, Âm nhạc, Phim kinh dị, Ai Cập cổ đại, Hải tặc, Trung Hoa cổ, Nhật Bản cổ | các nhánh khác (sẽ gộp) | mỗi chủ đề một bộ (ví dụ trống đồng, nhạc 8-bit, cát vàng, sóng biển) |

Khoảng 17 rương; các GLB ngoài Golden và Thiên Cơ đang ở nhánh khác.

## Chưa kiểm chứng

- **Chưa chạy trong Unity** (máy dựng không có Unity). Mã đã biên dịch thử bằng `mcs` với **stub UnityEngine tự viết**, nên chỉ bắt lỗi cú pháp và kiểu, không bắt được sai khác API thật hay lỗi lúc chạy.
- Hạt, tia, vòng, bóng dựng bằng mã **chưa thấy tận mắt**; màu, độ sáng, kích thước có thể cần chỉnh.
- Âm thanh tổng hợp **chưa ai nghe**; chỉ kiểm tra định dạng và mức âm bằng script.
- Shader tìm theo tên (`Shader.Find`): trong bản build phải giữ shader `Universal Render Pipeline/Unlit` và `Universal Render Pipeline/Particles/Unlit` (Always Included Shaders), nếu không sẽ rơi về shader dự phòng.
- Màu `Glow` được tô qua MaterialPropertyBlock với các tên thuộc tính thường gặp (`_BaseColor`, `baseColorFactor`, `emissiveFactor`…); nếu vật liệu glTFast dùng tên khác, quầng sáng sẽ giữ màu gốc (đèn và hạt vẫn đổi màu).

## Câu hỏi còn mở

1. Duyệt bảng màu 5 bậc và dấu hình dạng (số tia, số vòng, số chấm).
2. Nhịp: quay 2,8 s có quá dài khi mở nhiều rương liên tiếp không? Có cần nút bấm để tua nhanh?
3. Bóng: lõi tối màu bậc + viền sáng, hay đen tuyền + viền?
4. Tiếng chốt Rác kiểu hài (kèn tụt giọng) có hợp giọng Gen Z của game không?

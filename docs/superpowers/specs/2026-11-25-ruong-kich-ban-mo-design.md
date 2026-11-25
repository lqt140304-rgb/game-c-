# Thiết kế: kịch bản mở rương (lõi viết lại)

Trạng thái: **mã đã dựng và biên dịch thử bằng stub, CHƯA chạy trong Unity, chưa ai nghe âm tổng hợp**. Liên quan: [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md) (lõi cũ: quay màu, bóng, hồ sơ), [ruong-chu-de](2026-11-24-ruong-chu-de-design.md).

## Giữ chặt
- **Bậc do game quay** (`ChestRevealSequence.Play(rolledTier, ...)`). Không có API nào của lõi mới đổi bậc. Cổng "vật ngoài" chỉ quyết kịch bản chạy tiếp hay rẽ nhánh, không quyết bậc.
- Ngẫu nhiên **chỉ để trang trí**, theo `decorationSeed` (mỗi bước có bộ ngẫu nhiên riêng theo hạt + chỉ số bước).
- **Rương không có thành phần mới chạy như cũ**: không có `ChestScript` thì không có bước nào, không thêm `ChestMotion`. Có một thay đổi cố ý: mọi rương dùng `ChestRevealSequence` giờ có **âm căng thẳng** từ lúc bấm mở (mặc định bật, tắt bằng `profile.tension.enabled = false`). Rương chỉ có `BoxOpenSequence` không đổi gì.

## 1. Âm căng thẳng (`ChestTension`)
- **Bắt đầu ngay trong `Play()`**, trước khi nắp động đậy: nền lặp + nhịp "tim đập" tăng dần (cao độ 0,8 → 2,1, âm lượng, nhịp 1,0 s → 0,11 s). Độ căng đi theo đường cong từ lúc rương bắt đầu mở tới khoảnh khắc chốt; thời gian dự tính = `BoxOpenSequence.IntroSeconds` (màn chìa/cắn, mới thêm) + thời gian các giai đoạn tới `lockStage`. Mở chậm hơn dự tính thì giữ ở `holdLevel` 0,96 (căng mà không đoán được lúc nào).
- **Rẽ theo bậc chỉ ở khoảnh khắc chốt** (`Lock()` → `ChestTension.Resolve(tier)`):

| Bậc | `TensionResolution` | Nghe thấy |
|---|---|---|
| Rác | Deflate | nền và nhịp **cắt đột ngột**, `AudioSource` chính cũng bị dừng, im 0,22 s, rồi tiếng **xịt** trượt giọng (anti-climax) |
| Thường | Soften | xẹp dần 0,35 s, một tiếng dâng nhỏ |
| Hiếm | Release | đỉnh căng được giải toả, dâng vừa |
| Rất hiếm | Surge | nền vọt cao độ, dâng lớn |
| Huyền thoại | Blast | vọt cao nhất, dâng lớn nhất, thêm tiếng ầm trầm |

- **Chọn giấu kết quả, lý do**: bậc có sẵn từ lúc bấm, nên `ChestTension` được dựng **không có tham số bậc**; chỉ `Resolve` nhận bậc. Nhờ đó không thể vô tình để nền, nhịp hay đường cong phụ thuộc bậc: mọi bậc nghe giống hệt nhau tới đúng khoảnh khắc chốt rồi mới rẽ. Phương án "âm biết trước và hé lộ dần" bị loại vì người chơi sẽ đoán được bậc trước khi màu dừng, làm mất cái hồi hộp của vòng quay.
- Âm tổng hợp bằng mã (`ChestTensionSynth`, hạt cố định) khi hồ sơ không có clip, để rương nào cũng có tiếng. Rương chủ đề nên gán clip riêng vào `profile.tension` (`loopClip`, `pulseClip`, `deflateClip`, `swellClip`) và chỉnh `resolutionByTier`. Nền nên là clip liền mạch lặp được.

## 2. Kịch bản mở rương
`ChestScript` (ScriptableObject, *Create > Cho Do La > Chest Script*) gắn vào `profile.script` (hoặc ô `scriptOverride` trên `ChestRevealSequence`) = danh sách `ChestStep`. Mỗi bước có **khi nào chạy** (`StepStart`: AfterPrevious, WithPrevious, OnPress, OnStage, OnRoulette, OnLock, OnSilhouetteComplete, OnRevealed, OnEvent + `delay`), **điều kiện trang trí** (`minTier`..`maxTier`, `chance`), và loại việc:

| `StepKind` | Làm gì |
|---|---|
| Wait, Sound | đợi; phát clip (cao độ ngẫu nhiên trong khoảng) |
| Spawn | đặt prefab ở `anchor` + `offset`; có `path` thì bay qua các điểm (Catmull-Rom) |
| ChestMotion | cả rương: Shake, Jump, Spin, Hover, Wobble, Squash, Fly (`holdEnd` giữ trạng thái cuối) |
| ChestVisual | ẩn / hiện rương, hoặc **biến hình** sang prefab khác |
| PhysicsBurst | bắn N vật có Rigidbody (xu / bóng / hộp / prefab), số lượng + `countPerTier` × hạng, rải theo `spread` |
| Explosion | lực nổ lên vật của rương (và vật khác trong cảnh nếu `affectScene`) |
| Gate | mở khe nhận vật ngoài, chờ tới khi đủ hoặc hết `timeoutSeconds` |
| OpenBox | bắt đầu mở rương. **Có bước này thì `Play` không tự mở**; không có thì mở ngay như cũ |
| Cue | gọi `ChestFxHook.OnCue(tên, context)`, kịch bản **đợi** coroutine trả về xong |
| RaiseEvent | phát sự kiện tên tự đặt |

Sự kiện dựng sẵn (`ChestEvents`): `press`, `stage:N`, `roulette`, `lock`, `silhouette`, `revealed`, `gate:<id>:ok|wrong|timeout|done`. Sự kiện được nhớ, bước đứng chờ sau đó vẫn thấy.

Quy tắc dễ vấp:
- Bước bị bỏ qua (tắt, ngoài khoảng bậc, tung xác suất trượt) tính là **xong ngay lúc bắt đầu**. Hệ quả: `WithPrevious` đặt sau một bước **bị bỏ qua vì bậc** sẽ chạy ngay lúc bấm. Nhóm các bước cùng khoảng bậc liền nhau (như `ApplyGoldenScript`) hoặc neo bằng `OnLock`/`OnEvent`.
- `SetStepEnabled(id, bool)` bật/tắt bước theo `id` cho các lần Play sau mà không sửa asset.
- Phần phải viết mã nằm trong hook (`OnCue`), không nhồi vào `ChestStep`.

### Hook (thay cho chỉ ghi đè)
`ChestFxHook` thêm: `OnPress`, `OnTensionPulse(tension)` (không có bậc, cố ý), `OnResolve(tier, resolution)`, `OnScriptEvent(name)`, `IEnumerator OnCue(cue, ctx)` (trả `null` nếu không phải cue của mình). Hàm cũ giữ nguyên.

### `ChestScriptContext` (đưa cho Cue)
`Reveal`, `Tier` (chỉ đọc), `Motion`, `Scope` (sổ dọn dẹp), `Mouth`, `ChestHeight/Size`, `AnchorPosition(anchor, offset)`, `RngFor(salt)` (ngẫu nhiên theo hạt), `Raise/HasEvent`, `FindSlot(gateId)`, `ChestColliders`.

## 3. Vật lý (`ChestPhysicsFx`, `ChestFxScope`)
`SpawnBody`, `Burst` (nón quanh trục đứng của rương, tốc độ m/s, xoay), `Explode` (`AddExplosionForce`, `VelocityChange`), `Fly` (coroutine bay qua điểm, dùng trong Cue). Vật có `ChestFxJanitor`: bỏ qua va chạm với rương 0,35 s đầu (vì sinh ra trong thể tích rương), thu nhỏ rồi huỷ sau `bodyLifetime`, huỷ khi rơi quá sâu. `ChestFxScope` giữ mọi vật + vật liệu tự dựng, trần 160 vật (huỷ cũ nhất), và `ResetReveal` dọn sạch. Không dùng `velocity`/`drag` (đổi tên giữa các bản Unity), chỉ `AddForce/AddTorque`. Dùng `PhysicsMaterial` (Unity 6).

## 4. Chuyển động cả rương (`ChestMotion`)
`BoxOpenSequence` ghi đè `localPosition` của rương mỗi khung để rung, nên `ChestMotion` chạy ở `LateUpdate` và ghi (gốc + độ lệch), nhận gốc bằng cách so với giá trị nó ghi ở khung trước (xem comment đầu file). Chạy được khi rương đang cầm tay. Hạn chế: script khác ghi lại đúng giá trị cũ thì không nhận ra. `ResetAll` trả rương về gốc, hiện lại, gỡ đồ biến hình.

## 5. Vật ngoài mở khoá (`ChestItemSlot`, `ChestSlotItem`)
Vật ngoài gắn `ChestSlotItem{itemId}`; rương có `ChestItemSlot{gateId, requiredItems, requireOrder}`. Nhận bằng vật lý (vật có Rigidbody rơi vào trigger, tự tạo `SphereCollider` trigger) hoặc bằng mã `Offer(itemId, obj)` (game gọi khi người chơi đặt đồ đang cầm). Sự kiện: `onAccepted`, `onWrong` (vật sai bị đẩy ra), `onCompleted`; chỉ nhận khi bước `Gate` đang mở. Hết giờ thì phát `gate:<id>:timeout` và **kịch bản vẫn chạy tiếp**: kết quả do game quyết, rương không bao giờ kẹt vì cổng.

## 6. Rương Golden (mẫu)
`ChestFxPresets.ApplyGoldenScript` + `GoldenGlintHook` (cue `golden_glint`). Rác: rương xẹp, rớt 3 xu; Thường: 9 xu; Hiếm/Rất hiếm: rương nhảy, 14 + 7×(hạng−2)… xu bắn rải + lực nổ nhỏ + lấp lánh; Huyền thoại: rương lơ lửng và xoay, 40 xu, nổ. Cổng đồng xu **tắt mặc định**; bật: `reveal.SetStepEnabled("cong-xu", true)` (bàn thử: phím G, thả xu K, thả vật sai L). Asset `ChestScript_Golden` do menu *Create Golden Chest FX Profile* / *Create All Chest FX Profiles* tạo (không ghi đè bản đã chỉnh).

## 7. Hạt: quy tắc velocityOverLifetime
Unity yêu cầu x/y/z, orbitalX/Y/Z, offsetX/Y/Z **cùng chế độ**. Chỉ được đặt qua `ChestFxFactory.SetVelocity` (đặt cả 9 đường cong TwoConstants). Đã rà toàn bộ `Assets`: chỉ còn 2 chỗ dùng (Swirl của factory và `RacQuyHook`), cả hai qua hàm này. `Tools/CompileCheck/check_particle_velocity.sh` (chạy trong `check_chest_fx.sh`) báo lỗi nếu ai đặt trực tiếp.

## Cách một phiên chủ đề dùng (ví dụ rương Không gian)
1. Hồ sơ riêng (đã có `KhongGianFxPreset`): thêm `tension` (clip nền radar/động cơ, `resolutionByTier`).
2. `ChestScript_KhongGian`: `OnLock` bậc ≥ Hiếm → bước `Cue "phi_thuyen"`, `waitForEnd`; bậc thấp → `Cue "thien_thach_truot"`.
3. Trong `KhongGianHook.OnCue`: `phi_thuyen` → coroutine: `Instantiate` tàu, `yield return ChestPhysicsFx.Fly(...)` bay tới, nổ máy (hạt), bắn tên lửa (`Spawn` + `Fly`), bay đi, rồi `PhysicsBurst` thả quà (`ctx.Scope`, `ChestPhysicsFx.SpawnBody`). Phần tàu là việc của phiên Không gian; lõi chỉ cung cấp khung.
4. Cổng vật ngoài (ví dụ pin): thêm `ChestItemSlot{gateId="pin"}` lên rương, bước `Gate` + `OpenBox` ở đầu kịch bản.

## Chưa kiểm chứng
- **Chưa chạy trong Unity.** Chỉ biên dịch thử bằng `mcs` với stub tự viết (`check_all.sh`, `check_chest_fx.sh`), bắt lỗi cú pháp/kiểu, không bắt sai khác API thật (đặc biệt `PhysicsMaterial`, `AudioClip.Create`, thứ tự `LateUpdate` với coroutine của `BoxOpenSequence`).
- Âm tổng hợp (nền, nhịp, xịt, dâng, ầm) **chưa ai nghe**; có thể cần chỉnh âm lượng/cao độ hoặc thay bằng clip thật. Vòng lặp nền được thiết kế liền mạch (số chu kỳ nguyên) nhưng chưa thử nghe.
- Cảm giác vật lý (tốc độ bắn, độ nảy xu, lực nổ), kích thước xu, vật liệu `Universal Render Pipeline/Lit` (cần có trong build) chưa thấy tận mắt.
- Ghi đè vị trí của `ChestMotion` khi rương đang cầm tay (`ChestHandMenu`) chưa thử.
- Âm `openAmbientClip` cũ (một lần phát) vẫn chạy song song với âm căng; ở Rác bị cắt vì `AudioSource.Stop()`, các rương khác nghe cả hai.
- Cổng nhận bằng vật lý cần vật ngoài có Rigidbody và rương/khe có trigger; chưa nối với `PlayerInteractor`/`PickupInteractable` (game gọi `Offer` khi đặt đồ).

# Kế hoạch dọn sạch hiệu ứng rương: chỉ còn rung lắc, bóp méo, âm thanh và VẬT MỞ KHOÁ

Ngày 2026-11-25 (quyết định mới của chủ dự án) · Thay thế kế hoạch [thu gọn](2026-11-25-thu-gon-hieu-ung-ruong-ke-hoach.md)
(mục 5 từng rương của bản đó **không còn dùng**; thước đo mục 2 và quy tắc âm A4/A6 vẫn là tham chiếu).
Phiên làm đọc kèm [prompt mẫu](2026-11-26-don-sach-ruong-chia-khoa-prompt-mau.md).
Giờ bàn giao: **02:00 sáng 2026-11-26 (giờ Việt Nam)**.

## 0. Quyết định của chủ dự án (nguyên văn ý)

1. Bỏ hết hiệu ứng thừa, làm **sạch**: chỉ giữ **rung lắc, bóp méo (méo mó khi mở), âm thanh**.
2. Thêm **một thứ duy nhất**: **vật mở khoá**. Không tạo hiệu ứng thừa nào khác.
3. **"Khoá" không chỉ là chìa khoá: nó có thể là BẤT CỨ THỨ GÌ liên quan chủ đề** (ví dụ con mèo ở rương Rác Quý, thanh kiếm
   Nhật ở rương Nhật Bản, con vẹt ở Hải tặc, quả bóng ở Thể thao…). Mỗi rương một vật khác nhau.
4. Vật mở khoá **tự đến và tự mở rương**, không cần người chơi làm gì (không còn tương tác tay).

> Tên trong mã vẫn là `ChestKey*` / "chìa" cho gọn; hiểu là "vật mở khoá". Mỗi vật có một **hành động mở** riêng (cắm, thả, đập/chém/đá,
> vồ, mổ…) nhưng đều nằm trong cùng khung thời gian mục 2.

Hệ quả: mọi nhân vật phụ, cảnh dựng, đạo cụ, vật lý rải (mèo, đại bác, bi, xu, đá, nước, pháo…), phím tương tác (G, B, F, H, V,
K, L, Z, X…) và hook riêng của 17 rương **bị bỏ**. Mọi rương dùng **cùng một chuỗi mở**, chỉ khác hình chìa, âm và độ
"bóp méo" nhẹ theo tính cách chủ đề.

## 1. Chỉ được có 4 thứ (và danh sách CẤM)

| Được | Giới hạn |
|---|---|
| **Vật mở khoá** | 1 vật duy nhất (đồ vật hoặc con vật), dựng bằng khối cơ bản (≥ 4 khối, nhận ra chủ đề), kích thước ≤ 0,5 S (S = cạnh lớn nhất của rương), đến chỗ rương, làm **một hành động mở**, rồi biến mất. Không có Rigidbody, hạt, đèn. |
| **Rung lắc** | Cả rương rung (`ChestMotion` Shake), mạnh dần tới lúc chốt, giống nhau mọi bậc. |
| **Bóp méo** | Cả rương ép dẹt/giãn (`ChestMotion` Squash, có thể kèm 1 cú nảy Jump nhỏ) lúc chốt theo bậc. |
| **Âm thanh** | Tối đa 3 lớp (mục 3): nền, tiếng cắm chìa, tiếng chốt theo bậc. |

Lõi vẫn chạy **vòng đổi màu 5 bậc + bóng vật phẩm to dần + ánh sáng glow quanh miệng** (đây là việc "mở rương" của game,
không phải hiệu ứng thừa). Bậc do game quyết, không đổi.

**CẤM (xoá khỏi mọi đường chạy):** nhân vật/diễn viên phụ · cảnh dựng (nước, mây, sàn, khung thành, sân…) · vật Rigidbody
rải ra (xu, rác, đá, bi, bóng…) · hệ hạt riêng của hook · Light riêng · tia/vòng/chớp sáng và hạt của lõi (mục 4.5 tắt chúng) ·
phím tương tác · chữ nổi · đổi `Time.timeScale` · âm tạo bằng mã (`MinerSynth`…) · âm numpy (`*.wav` trong thư mục con) ·
tiếng tick/nền/accent/whoosh/stage của lõi.

## 2. Chuỗi mở CHUẨN (mọi rương giống hệt, tính từ lúc bấm mở)

| Giây | Việc | Âm (tối đa 3 lớp) |
|---|---|---|
| 0,00 | Bấm mở. Vật mở khoá hiện ở **1,2 S** trước rương, cao ngang 0,6 H, quay mặt về rương | **Nền** (vòng lặp chủ đề) vào ≤ 0,2 s, nhỏ |
| 0,00 → 1,00 | Vật **bay/nhảy/lăn vòng cung** (Bezier, đỉnh cao hơn 0,25 S) tới điểm "khe" = mặt trước rương, cao 0,4 H | nền dâng nhẹ |
| 1,00 → 1,35 | **Hành động mở** (bảng dưới) 0,35 s tại điểm khe, kết thúc bằng việc vật **thu nhỏ về 0** và biến mất | **Tiếng mở khoá** (`key_insert`) 1 lần, âm 0,6 |
| 1,35 | Mốc sự kiện `chia_khoa_xong` (vật đã biến mất). Lõi bắt đầu mở nắp + vòng đổi màu (bước `OpenBox`) | |
| 1,35 → chốt | **Rung lắc** tăng dần: 3 nấc, biên độ 0,004 → 0,007 → 0,011 (mét, nhân `S/0,5`) | nền dâng cao độ 0,9 → 1,3, âm lượng 0,25 → 0,45 |
| chốt (OnLock) | **Bóp méo theo bậc** (bảng dưới) | Rác: nền **tắt phụt → lặng 0,2 s → tiếng chốt Rác**. Bậc khác: nền tắt trong 0,3 s + **tiếng chốt** (1 tiếng, to dần theo bậc) |
| chốt + 3 s | Rương trả về dáng gốc; mọi thứ im | |

Tổng từ bấm tới chốt do lõi quyết (thường 5–7 s). Vật mở khoá luôn xong trong 1,35 s nên rương không bao giờ bị kẹt.

**Hành động mở** (cột "Hành động" ở mục 6 chọn một; `ChestKeyHookBase` cài sẵn cả 5, mỗi hành động 0,35 s, xong thì vật thu nhỏ biến mất):

| Hành động | Làm gì |
|---|---|
| `Insert` | đẩy sâu 0,1 S vào khe rồi xoay 90° |
| `Drop` | thả rơi thẳng vào khe (nảy 1 lần trước khi chui vào) |
| `Strike` | lao vào khe như đập/chém/đá/dán, bật lùi 0,05 S rồi chui vào |
| `Pounce` | nhảy vồ lên mặt trước, đè sát khe, ngoáy 2 nhịp rồi chui vào |
| `Peck` | mổ/gõ 2 lần vào khe rồi chui vào |

Lớp con còn có thể ghi đè `AnimateAction(KeyContext c, Transform root, float t01)` để thêm chuyển động của **chính vật đó**
(mèo vẫy đuôi, kiếm xoay, cánh vẹt vỗ…) bằng cách xoay/tịnh tiến các khối con của nó; **không** sinh thêm vật nào.

**Bóp méo theo bậc (mặc định chung, mỗi rương được chỉnh ±20% theo tính cách ở mục 6; sau khi nhân hệ số, Jump luôn bị KẸP ≤ 0,6 H và Squash ≤ 0,36):**

| Bậc | Squash (tỉ lệ) | chu kỳ | Jump (× H) | Ghi chú |
|---|---|---|---|---|
| Rác | 0,30 ép dẹt, **giữ dẹt** 0,7 s rồi phồng lại (xem 4.2: cần kiểu `Squash` mới trong lõi) | 1 | 0 | buồn, hụt hơi |
| Thường | 0,10 | 1 | 0,08 | |
| Hiếm | 0,16 | 2 | 0,18 | |
| Rất hiếm | 0,22 | 3 | 0,35 | |
| Huyền thoại | 0,30 | 4 | 0,60 | nảy cao nhất, vẫn ≤ 0,6 H |

## 3. Âm thanh (tất cả là mp3 ElevenLabs)

Ba lớp, và **chỉ ba**:

1. **Nền** (`bedClip`, vòng lặp chủ đề): dâng cao độ 0,9 → 1,3 và âm lượng 0,25 → 0,45. Dừng ngay lúc chốt (Rác) hoặc fade 0,3 s.
2. **Tiếng cắm chìa** (`keyClip`, mới): 1 lần, âm 0,6, lúc chìa chạm khe.
3. **Tiếng chốt** (`stingClips[5]`): đúng 1 tiếng theo bậc, âm 0,55 / 0,7 / 0,8 / 0,9 / 1,0 cho Rác→HT. Rác phát sau 0,2 s lặng.

**Âm lượng:** `volume` (0,8) là hệ số tổng, nhân đúng MỘT lần vào mọi giá trị ghi ở trên (nền 0,25 → 0,45; cắm chìa 0,6; chốt 0,55 → 1,0); nguồn nền đặt `source.volume` = giá trị đã nhân, không nhân lần nữa ở `PlayOneShot`.

Không có tiếng nào khác (không tick, không accent, không bước giai đoạn, không đuôi). Không chỗ nào nhân `volume` hai lần.
Tiếng phát qua `ChestRevealSequence.PlayOneShot` (cần đổi cao độ thì dùng tham số pitch của nó, không đổi pitch nguồn đang ngân).

**File âm đã có (từ Phiên âm 1, trên nhánh `claude/chest-trim-plan`):** nền = các `*_loop.mp3`; tiếng chốt =
`sfx_<tiền tố>_lock_{rac,thuong,hiem,ratHiem,huyenThoai}.mp3` ở thư mục âm của rương.
**File cần tạo thêm (Phiên âm 2):** 17 tiếng `sfx_<tiền tố>_key_insert.mp3` + 3 nền còn thiếu cho Âm nhạc, Thể thao, Thiên Cơ.
Danh mục: `Tools/Audio/elevenlabs_chest_key_manifest.json`.

**Cập nhật:** Chủ dự án nghe thử thấy âm ElevenLabs mới chán, dùng lại âm cũ; 20 file mới vẫn nằm trong repo nhưng không dùng.
Tiếng cắm giờ lấy từ trường `keyFile` của Preset (một âm cũ ngắn của chính rương); nền Âm nhạc/Thể thao/Thiên Cơ dùng
`sfx_amnhac_drumroll.mp3`, `sfx_thethao_ambient.mp3`, `sfx_thienco_ambient_clockwork.mp3`.

## 4. Kỹ thuật chung (làm trong PHIÊN A, một phiên, TRƯỚC các phiên rương)

### 4.1 Lớp nền `ChestKeyHookBase` (file mới `Scripts/ChestFx/ChestKeyHookBase.cs`, namespace `ChoDoLa.ChestFx`)
Hook một cho cả 17 rương, kế thừa `ChestFxHook`. Việc của nó (mọi con số ở mục 2):
- `OnPress`: dựng chìa bằng `BuildKey(KeyContext ctx)` (hàm ảo do lớp con viết), bắt đầu **nền** vòng lặp.
- `OnCue("chia_khoa", ctx)` trả coroutine: bay vòng cung → hành động mở (`Preset.openAction`) → thu nhỏ → huỷ vật; phát `keyClip`; cuối cùng
  `ctx.Raise("chia_khoa_xong")`.
- Cập nhật nền dâng dần theo thời gian từ lúc chìa xong tới lúc chốt (đường cong mục 2).
- `OnTierLocked`: Rác → `Stop()` nền, `WaitForSeconds(0.2)`, phát `stingClips[0]`; bậc khác → fade nền 0,3 s, phát `stingClips[rank]`.
- `OnReset`/`OnDestroy`: dọn chìa, dừng âm, không để vật thừa.
- Ngẫu nhiên (nếu cần) dùng `ctx.RngFor(salt)`; **không** dùng `UnityEngine.Random`.
- Trường chỉnh được (có trong Inspector): `bedClip`, `keyClip`, `stingClips[5]`, `volume = 0.8`, `keyScale = 0.35`, `keyColor`.
- `KeyContext` đưa: `float S` (cỡ rương), `float H`, `Vector3 front` (hướng trước của rương, như `RacQuyHook.Measure`/`BoxOpenSequence.FrontDirection`),
  `Vector3 slotPos` (điểm khe), `Material MakeMat(Color)` (dùng `ChestFxFactory.CreateUnlit`, tự dọn), `GameObject Part(PrimitiveType, name, localPos, localScale, localRot, color)`.
- **Quy ước hình vật:** `BuildKey` trả về gốc vật với **trục +Z là hướng "đầu/mũi/mặt" quay về rương** (mũi kiếm, mặt mèo, đầu vẹt, mũi chìa), gốc toạ độ ở giữa; vật vừa trong hộp 1×1×1 (hook tự nhân `keyScale·S`).

### 4.2 Chuỗi bước chung (file mới `Scripts/ChestFx/ChestKeyScript.cs`)
`public static void Build(ChestScript s)` dựng đúng chuỗi mục 2 bằng `ChestStepBuilder` (cùng kiểu `ApplyGoldenScript`):
1. `Cue "chia_khoa"` — `OnPress`, `waitForEnd`. 2. `OpenBox` — `AfterPrevious`.
3. Rung nấc 1: `Shake 0.004 · 30 Hz · 1.2 s` — `OnEvent "chia_khoa_xong"`.
4. Rung nấc 2: `Shake 0.007` — `OnStage 2`, `duration 2.4 s`. 5. Rung nấc 3: `Shake 0.011` — `OnStage 3`... (chỉnh theo `BoxOpenSequence` mặc định để nấc 3 kéo tới sát chốt).
6. Bóp méo/nảy theo bậc: 5 cặp bước `OnLock` (`minTier == maxTier`) dùng bảng mục 2 (`Squash` + `Jump`); Rác có `holdEnd` rồi 1 bước `Squash` trả về.
**Đơn vị của lõi (đã đọc `ChestMotion`):** `Jump.amplitude` là MÉT tuyệt đối (= bảng × H × hệ số, đã kẹp), `frequency` = SỐ LẦN NẢY (mỗi lần thấp hơn 40%), nên "chu kỳ" của Jump = số lần nảy. `Squash.amplitude` là tỉ lệ, `frequency` = số chu kỳ sin (nửa đầu dẹt, nửa sau giãn). **Rác giữ dẹt:** `Squash` hiện chỉ có dạng sin nên không giữ dẹt được; Phiên A thêm một kiểu giữ (ví dụ cờ `holdFlat` hoặc `StepMotionKind.SquashHold`: ép nhanh 0,15 s, giữ, rồi trả về trong 0,2 s) và đặt nó cho bậc Rác. Biên độ rung nhân `S/0,5` được hook tính khi dựng (hoặc Phiên A thêm tham số `scaleWithChest` cho `ChestStep` nếu cần; sửa lõi chỉ trong phiên này).

### 4.3 17 lớp con (rỗng, để phiên rương điền)
Phiên A tạo **17 file** `Scripts/ChestFx/Chests/<Tên>KeyHook.cs`: `public class <Tên>KeyHook : ChestKeyHookBase` với
`protected override GameObject BuildKey(KeyContext c)` trả về một vật **mặc định chung** (thân hình trụ + đầu tròn + 2 răng, màu theo chủ đề) để dự án luôn biên dịch.
Tên lớp: `GoldenKeyHook, HaiTacKeyHook, RacQuyKeyHook, MagicKeyHook, MimicKeyHook, MinerKeyHook, AmNhacKeyHook, VietNamKeyHook, TuoiThoKeyHook, RobotKeyHook, KhongGianKeyHook, GameKeyHook, TheThaoKeyHook, TrungHoaKeyHook, NhatBanKeyHook, CoPhongKeyHook, ThienCoKeyHook`.
Mỗi lớp con có thêm `public static readonly KeyPreset Preset` (struct `KeyPreset` do phiên A định nghĩa: `chestId`, thư mục âm, tiền tố file, màu chính, `openAction` (enum 5 hành động), 5 hệ số bóp méo, `shakeScale`).

### 4.4 Thiết lập chung (Editor)
- File mới `Editor/ChestKeySetup.cs`: với mỗi rương dựng **hồ sơ** `ChestFx_<Tên>.asset` từ đầu: màu/hạt đặt theo mục 4.5, gọi `ChestAudioRule.ApplyToProfile`, `ChestKeyScript.Build` vào `profile.script`, tạo prefab hook `<Tên>KeyHook` gán vào `customFxPrefab`, nạp mp3 theo `Preset` (nền, `key_insert`, 5 `lock_*`). File mp3 thiếu → ô trống + cảnh báo, không lỗi.
- Viết lại `Editor/CreateAllChestFxProfiles.cs`: 17 maker đều gọi `ChestKeySetup`; `RootHookTemplate` trả `null` (hook nằm trong prefab của hồ sơ). Bỏ mọi tham chiếu tới `*FxSetup`, `*Setup`, hook cũ, `GoldenGlintHook`, `ChestRevealDemo` (dòng thêm hook cũ), `ChestRevealTestSetup`.
- Menu mỗi rương: `Tools/Cho Do La/Chia Khoa/Setup <Tên> Chest`, và `Setup All`.

### 4.5 Lõi tối giản (`ChestFxProfile` + `ChestRevealSequence`, chỉ phiên A được sửa)
Thêm ô `public bool minimalFx = false` trên `ChestFxProfile`. Khi `true`, `ChestRevealSequence` **bỏ**: hạt `_motes`/`_burst`, `SpawnFlash`, `SpawnRays`, `RingRoutine`, `AccentRoutine`, tick, âm khoá/whoosh/ambient/stage (đã tắt bằng A1), và giữ: vòng đổi màu, `SetGlowTint`, bóng vật phẩm, `revealLight` (đèn glow). Hồ sơ của cả 17 rương đặt `minimalFx = true`. Kiểm: rương không còn tia/vòng/hạt khi chốt.

### 4.6 Dọn mã cũ
- Phiên A xoá khỏi `ChestRevealDemo`/`ChestRevealTestSetup`/`CreateAllChestFxProfiles` mọi dòng gắn hook cũ.
- Mỗi phiên rương **xoá file của rương mình** (danh sách ở mục 6): hook, parts, synth, preset, setup, `.meta` đi kèm, thư mục âm `.wav` numpy của hook cũ, script `Tools/Audio/make_*` và script `Tools/CompileCheck/check_*` tương ứng. Lịch sử Git giữ lại bản cũ.
- Giữ nguyên: lõi (`ChestRevealSequence`, `ChestMotion`, `ChestScript*`, `ChestFxFactory`, `ChestFxProfile`, `TierPalette`, …), các mp3 ElevenLabs (cả thư mục `Audio/Chests/<rương>/*.mp3`), `ChestAudioRule`.
- Sau mỗi phiên rương: `check_all.sh` phải qua (Phiên A cập nhật danh sách file trong script kiểm cho khớp).

## 5. Các phiên

| Phiên | Việc | Phụ thuộc |
|---|---|---|
| **A – Lõi + khung chìa** | Mục 4.1–4.5, tạo 17 lớp con rỗng, viết lại CreateAll, `check_chia_khoa.sh` | không |
| **B – Âm** | Tạo 20 file theo `elevenlabs_chest_key_manifest.json` (script nhận thêm `--manifest`), xin khoá ElevenLabs | không (chạy song song A) |
| **17 phiên rương** | Mục 6: hình chìa, tinh chỉnh, xoá mã cũ | A và B đã gộp |
| **Gộp** | Gộp 19 nhánh, biên dịch, báo chủ dự án | tất cả |

## 6. MỤC TỪNG RƯƠNG

Mỗi phiên rương chỉ sửa: `Scripts/ChestFx/Chests/<Tên>KeyHook.cs` (viết `BuildKey`, điền `Preset`, tuỳ chọn `AnimateAction`), xoá file cũ liệt kê, cập nhật spec cũ của rương
(`2026-11-25-kichban-<spec>-design.md`: đầu file ghi "ĐÃ THAY bằng bản chìa khoá, xem kế hoạch 2026-11-26" và thu còn ≤ 10 dòng).
Hình vật: **≥ 4 khối cơ bản, nhìn từ xa nhận ra chủ đề, màu theo chủ đề, "đầu" hướng +Z**. Hành động mở lấy từ cột "Hành động" (điền vào `Preset.openAction`).
"Bóp méo": chỉ chỉnh 5 hệ số trong `Preset`, trong ±20% bảng mặc định, để thể hiện tính cách.

| # | Rương | Lớp | **Vật mở khoá** (hình + màu) | Hành động | Nền (mp3 có sẵn) | Tiền tố âm | Tính cách bóp méo |
|---|---|---|---|---|---|---|---|
| 6.1 | Hải tặc | `HaiTacKeyHook` | **Con vẹt hải tặc**: thân đỏ, cánh xanh lá, mỏ vàng, mắt đen, đuôi dài; cánh vỗ khi bay | `Peck` | `sfx_haitac_bed_loop` | `sfx_haitac_` | đàn hồi như sóng: Squash +10% |
| 6.2 | Rác Quý | `RacQuyKeyHook` | **Con mèo mướp cam**: thân + đầu + 2 tai + đuôi (đuôi vẫy), mắt vàng-xanh | `Pounce` | `sfx_racquy_bed_loop` | `sfx_racquy_` | ẻo lả: Rác bẹp thêm +20%, nảy −20% |
| 6.3 | Golden | `GoldenKeyHook` | **Đồng xu vàng to**: đĩa dẹt viền nổi + chữ "$" bằng 3 khối, xoay tròn khi bay | `Drop` | `sfx_golden_spin_loop` | `sfx_golden_` | nảy mạnh: Jump +20% |
| 6.4 | Magic | `MagicKeyHook` | **Tinh thể rune**: khối thoi tím lơ lửng + 3 mảnh nhỏ xoay quanh (là phần của vật) | `Insert` | `sfx_magic_rune_bed_loop` | `sfx_magic_` | lơ lửng: Squash −10% |
| 6.5 | Mimic | `MimicKeyHook` | **Chiếc dép lê** (mồi cho rương đói): đế be + quai nâu + hai khối gót | `Drop` | `sfx_mimic_stomach_loop` | `sfx_mimic_` | háu đói: Squash +15% |
| 6.6 | Thợ Mỏ | `MinerKeyHook` | **Chiếc cuốc chim**: cán gỗ + lưỡi sắt cong + đầu nhọn | `Strike` | `sfx_miner_rumble_loop` | `sfx_miner_` | nặng: Jump −20% |
| 6.7 | Âm nhạc | `AmNhacKeyHook` | **Đĩa than**: đĩa đen dẹt + nhãn hồng giữa + 3 rãnh mảnh | `Insert` | `sfx_amnhac_bed_loop` (mới) | `sfx_amnhac_` | nhún theo nhịp: chu kỳ +1 |
| 6.8 | Việt Nam | `VietNamKeyHook` | **Hoa đăng**: đế gỗ + 6 cánh sen hồng + ngọn lửa vàng ở giữa | `Drop` | `sfx_vietnam_dan_bau_loop` | `sfx_vietnam_` | mềm: Squash −10% |
| 6.9 | Tuổi thơ | `TuoiThoKeyHook` | **Viên bi ve**: cầu to trong màu + 3 lá xoáy nhiều màu bên trong | `Drop` | `sfx_tuoitho_roll_loop` | `sfx_tuoitho_` | tưng tưng: chu kỳ +1, Jump +10% |
| 6.10 | Robot | `RobotKeyHook` | **Thẻ chip**: thẻ chữ nhật dẹt xám kim loại + chip vàng + sọc mạch cyan | `Insert` | `sfx_robot_hum_loop` | `sfx_robot_` | cứng: Squash −20%, chu kỳ −1 (≥ 1) |
| 6.11 | Không gian | `KhongGianKeyHook` | **Phi thuyền mini**: thân bạc hình giọt nước + 2 cánh vây + kính cabin xanh | `Strike` | `sfx_khonggian_engine_loop` | `sfx_khonggian_` | nhẹ bẫng: Jump +20% |
| 6.12 | Game | `GameKeyHook` | **Chìa pixel 8-bit**: ghép cube vuông thành chìa (thân 1 hàng, đầu 3×3, răng 2), vàng + xanh | `Insert` | `sfx_game_chip_loop` | `sfx_game_` | giật như khung hình: chu kỳ +1 |
| 6.13 | Thể thao | `TheThaoKeyHook` | **Quả bóng đá**: cầu trắng + 5 mảng lục giác đen dẹt | `Strike` | `sfx_thethao_bed_loop` (mới) | `sfx_thethao_` | bật như bóng: Jump +20% |
| 6.14 | Trung Hoa | `TrungHoaKeyHook` | **Đầu lân nhỏ**: khối đầu đỏ + hai mắt to + hàm dưới vàng + bờm tua | `Pounce` | `sfx_trunghoa_lion_drum_loop` | `sfx_trunghoa_` | mừng hội: chu kỳ +1 |
| 6.15 | Nhật Bản | `NhatBanKeyHook` | **Thanh katana**: lưỡi dài bạc (hộp dẹt) + tsuba tròn đen + cán quấn đỏ; xoay ngang khi chém | `Strike` | `sfx_nhatban_shakuhachi_loop` | `sfx_nhatban_` | điềm tĩnh: chu kỳ −1 (≥ 1), Squash −10% |
| 6.16 | Cổ Phong | `CoPhongKeyHook` | **Lá bùa phong ấn**: tờ giấy vàng dài + 3 nét chữ đỏ bằng khối mỏng + tua đỏ | `Strike` (dán) | `sfx_cophong_sword_hum_loop` | `sfx_cophong_` | thanh thoát: Squash −10%, Jump +10% |
| 6.17 | Thiên Cơ | `ThienCoKeyHook` | **Bánh răng đồng**: đĩa trụ đồng + 8 răng quanh + lỗ trục giữa | `Drop` | `sfx_thienco_bed_loop` (mới) | `sfx_thienco_` | lò xo: Jump +20%, chu kỳ +1 |

**File cũ mỗi phiên rương xoá** (trong `Assets/_Project/Scripts/ChestFx/Chests/` trừ khi ghi khác; kèm `.meta`; và `Editor/*Setup*.cs` cùng tên; và thư mục `.wav` numpy, script `Tools/Audio/make_*`, `Tools/CompileCheck/check_*` của rương đó):

| Rương | File xoá |
|---|---|
| Hải tặc | `HaiTacHook, HaiTacSeqHook, HaiTacSeqParts, HaiTacFxPreset` · Editor `HaiTacFxSetup, HaiTacSeqSetup` · `Audio/Chests/hai_tac/seq/` · `make_haitac_seq_sfx.py` · `check_hai_tac*.sh` |
| Rác Quý | `RacQuyHook, RacQuyFxPreset` · Editor `RacQuyFxSetup` · `Audio/Chests/rac_quy/meo/` · `make_racquy_meo_sfx.py` · `check_rac_quy.sh` |
| Golden | `GoldenSlotHook` + `Scripts/ChestFx/GoldenGlintHook.cs` · Editor `GoldenSlotSetup` · `Audio/Chests/golden/*.wav` · `make_golden_slot_sfx.py` · `check_golden.sh` (và `ChestFxPresets.ApplyGoldenScript` do phiên A xoá) |
| Magic | `MagicHook, MagicFxPreset, MagicRuneRitual, MagicRuneStone` · Editor `MagicFxSetup, MagicRuneFxSetup` · `Audio/Chests/magic_rune/` · `make_magic_rune_sfx.py` · `check_magic*.sh` |
| Mimic | `MimicHook, MimicFeedHook, MimicFeedFood, MimicFeedMouth, MimicFxPreset` · Editor `MimicFxSetup, MimicFeedSetup` · `Audio/Chests/mimic_feed/` · `make_mimic_feed_sfx.py` · `check_mimic*.sh` |
| Thợ Mỏ | `MinerHook, MinerDigSequence, MinerSynth, MinerFxPreset` · Editor `MinerFxSetup, MinerDigSetup` · `check_miner*.sh` |
| Âm nhạc | `AmNhacHook, AmNhacBassHook, AmNhacVinyl, AmNhacFxPreset` · Editor `AmNhacFxSetup, AmNhacBassSetup` · `Audio/Chests/am_nhac/seq/` · `make_amnhac_sfx.py` · `check_am_nhac*.sh` |
| Việt Nam | `VietNamHook, VietNamHoiLangHook, VietNamHoiLangFloater, VietNamHoiLangProps, VietNamFxPreset` · Editor `VietNamFxSetup, VietNamHoiLangSetup` · `Audio/Chests/viet_nam/hoi_lang/` · `make_vietnam_hoilang_sfx.py` · `check_viet_nam*.sh` |
| Tuổi thơ | `TuoiThoHook, TuoiThoPiece, TuoiThoFxPreset` · Editor `TuoiThoFxSetup` · `Audio/Chests/tuoi_tho/may_bi/` · `make_tuoitho_machine_sfx.py` · `check_tuoi_tho.sh` |
| Robot | `RobotHook, RobotTransformHook, RobotChargePart, RobotChargeSocket, RobotFxPreset` · Editor `RobotFxSetup, RobotTransformSetup` · `Audio/Chests/robot/transform/` · `make_robot_sfx.py` · `check_robot*.sh` |
| Không gian | `KhongGianHook, KhongGianMissionHook, KhongGianSynth, KhongGianFxPreset` · Editor `KhongGianFxSetup` · `check_khong_gian.sh, check_khonggian_mission.sh` |
| Game | `GamePixelHook, GameVoxelHook, GameFxPreset` · Editor `GameFxSetup, GameVoxelFxSetup` · `Audio/Chests/game/voxel/` · `make_game_voxel_sfx.py` · `check_game*.sh` · `UnityStubGameVoxel.cs` |
| Thể thao | `TheThaoHook, TheThaoBall, TheThaoFxPreset` · Editor `TheThaoFxSetup` · `Audio/Chests/the_thao/match/` · `make_thethao_match_sfx.py` · `check_the_thao.sh` |
| Trung Hoa | `TrungHoaHook, TrungHoaFirecrackerString, TrungHoaSky, TrungHoaLion, TrungHoaChestMotion, TrungHoaPlayerItem, TrungHoaFxPreset` · Editor `TrungHoaFxSetup` · `Audio/Chests/trung_hoa/fx/` · `make_trunghoa_fx.py` · `check_trung_hoa.sh` |
| Nhật Bản | `NhatBanHook, NhatBanProps, NhatBanFxPreset` · Editor `NhatBanFxSetup` · `Audio/Chests/nhat_ban/*.wav` (các `sfx_nhatban_iai_*.wav`) · `make_nhatban_sfx.py` · `check_nhat_ban.sh` |
| Cổ Phong | `CoPhongHook, CoPhongNguKiemHook, CoPhongFxPreset` · Editor `CoPhongFxSetup, CoPhongNguKiemSetup` · `Audio/Chests/co_phong/ngu_kiem/` · `make_cophong_ngukiem_sfx.py` · `check_co_phong*.sh` |
| Thiên Cơ | `ThienCoHook, ThienCoRig, ThienCoFxPreset` · Editor `ThienCoFxSetup` · `Audio/Chests/thien_co/clockwork/` · `make_thienco_clockwork_sfx.py` · `check_thien_co.sh` |

**Bản chốt (mọi rương, đối chiếu khi kiểm):**

| Hạng mục | Giá trị |
|---|---|
| Diễn viên | **1** (vật mở khoá), sống ≤ 1,4 s |
| Vật Rigidbody / hạt / Light riêng | **0 / 0 / 0** |
| Phím tương tác | **0** |
| Lớp âm cùng lúc | nền + (1 tiếng cắm) + (1 tiếng chốt) ≤ 3, trước chốt ≤ 2 |
| Hook chạy | **1** (`<Tên>KeyHook`) |
| `tension.enabled` / `minimalFx` | false / true |
| Thời gian chìa | 1,35 s từ bấm tới `chia_khoa_xong` |
| Rương cùng chuỗi | rung 3 nấc → bóp méo theo bậc, mọi rương giống |
| Hình vật mở khoá | ≥ 4 khối, khác nhau giữa 17 rương; hành động mở theo cột "Hành động" |

## 7. Kiểm tra

Mỗi phiên: `bash Tools/CompileCheck/check_all.sh` + `check_chia_khoa.sh` (phiên A viết: biên dịch 17 lớp con, kiểm mỗi `Preset` đủ trường).
Chủ dự án (Unity): *Create All Chest FX Profiles* (đã đổi thành `Chia Khoa/Setup All`), mở từng rương 5 bậc:
(a) vật mở khoá đến, làm hành động mở, âm "cạch" · (b) rương rung tăng dần · (c) chốt: Rác bẹp + im rồi tiếng buồn, bậc cao nảy · (d) không còn hạt/tia/vòng/vật rải · (e) màu dừng đúng bậc, bóng vật phẩm to đủ · (f) Reset sạch.

## 8. Chưa kiểm chứng (trung thực)
- Chưa chạy Unity, chưa nghe âm nào. Con số rung/bóp méo là đề xuất, sẽ chỉnh theo cảm giác.
- `ChestMotion.Squash` và `Jump` có sẵn trong lõi nhưng chưa ai nhìn thấy kết quả khi chồng rung + bóp méo.
- Vật mở khoá (mèo, vẹt, kiếm…) dựng bằng khối cơ bản nên có thể thô; nếu muốn đẹp hơn, đặt mô hình `.glb` riêng sau.
- Việc xoá mã cũ lớn (ước ~25.000 dòng); lịch sử Git giữ bản cũ nếu cần lấy lại.

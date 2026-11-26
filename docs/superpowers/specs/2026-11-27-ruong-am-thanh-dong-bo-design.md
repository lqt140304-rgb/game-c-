# Rương: bỏ vật mở khoá, âm thanh khớp chuyển động, sửa hạt tím, chỉnh nhẹ chuyển động

Ngày 2026-11-27 · Thay phần "vật mở khoá" của [kế hoạch dọn sạch 2026-11-26](2026-11-26-don-sach-ruong-chia-khoa-ke-hoach.md)
và [đợt 2 chủ đề riêng](2026-11-26-dot2-chu-de-rieng-tung-ruong.md). Các phần còn giữ từ kế hoạch cũ: vòng đổi màu 5 bậc, bóng vật phẩm,
glow, hào quang chớp/tia/vòng, bóp méo theo bậc, quy tắc âm A4/A6. **Bậc do game quyết, không đổi; `BoxOpenSequence` không bị sửa.**

> Trung thực: mình **chưa chạy Unity và chưa nghe âm nào**. Mọi con số dưới đây là suy ra từ mã và từ độ dài file mp3; cần chủ dự án chạy thử (mục 9).

## 1. Quyết định của chủ dự án và việc đã làm

| Việc | Kết quả |
|---|---|
| A. Bỏ hẳn vật mở khoá | Đã xoá `ChestKeyHookBase`, `ChestKeyContext`, `ChestKeyTypes`, 17 lớp `*KeyHook`, cue `chia_khoa`, móc `ThemeFly/ThemeAction`, `flyFile/actionFile/keyFile`. Mỗi rương còn **một file dữ liệu** `Chests/<Tên>Preset.cs` + bảng đăng ký đủ 17 dòng. |
| B. Âm khớp chuyển động | Một dòng thời gian: mỗi nấc chuyển động và bước bóp méo **tự báo sự kiện**; hook âm `ChestSoundHook` chỉ phát tiếng khi nhận sự kiện. |
| C. Hạt tím | `ChestShaderResolver` (chuỗi shader dự phòng có kiểm, hỏng hết thì **tắt** chứ không vẽ tím) + menu kiểm trong Unity. |
| D. Chỉnh nhẹ | Vào/ra mượt hơn, bóp méo có nảy dư, nấc 1 → 2 → 3 tăng rõ hơn (mục 8). Mọi số đổi ≤ ±15%, giữ mọi kẹp an toàn. |

Chuỗi mở mới: **bấm mở → mở nắp → chuyển động cả rương 3 nấc theo `moveStyle` → (chốt bậc) bóp méo theo bậc**. Không còn bước "vật mở khoá đến" 1,35 s.

## 2. Hiện trạng: mọi tiếng trong một lần mở và vì sao "bị lạc"

Bảng này đọc từ mã ở commit trước khi sửa (`b6e2f38`). Cột "đồng hồ" quan trọng: âm và chuyển động chạy **hai đồng hồ khác nhau**.

| # | Tiếng | Phát lúc nào | Ai gọi | Đồng hồ | Còn dùng? |
|---|---|---|---|---|---|
| 1 | Nền (`KeyBed`, nguồn riêng, lặp) | bấm mở; dâng 0,25 → 0,30 trong 1,35 s đầu rồi dâng tới 0,45 và cao độ 1,3 trong **đúng 5 s cố định** kể từ lúc vật mở khoá xong | `ChestKeyHookBase.OnPress/Update` | `Time.unscaledTime` | Thay |
| 2 | Tiếng bay (`KeyFly`) | 0 → 1,0 s sau bấm | `ChestKeyHookBase.PlayFly` | thời gian thật | Bỏ |
| 3 | Tiếng hành động / tiếng cắm (`PlayOneShot` trên nguồn rương) | 1,0 s | `ChestKeyHookBase.KeyRoutine` | thời gian thật | Bỏ |
| 4 | Tiếng chốt (`PlayOneShot`) | lúc chốt; Rác sau 0,2 s | `OnTierLocked` → `PlaySting` / `RacSting` | thời gian thật | Thay |
| 5 | Nền tắt dần 0,3 s (bậc khác Rác) | lúc chốt | `FadeBedOut` (coroutine) | thời gian thật | Thay |
| 6 | Tick mỗi lần đổi màu | mỗi bước vòng màu | lõi `ChestRevealSequence.PlayTick` | `Time.deltaTime` | Tắt (xem dưới) |
| 7 | Ambient lúc bắt đầu vòng màu | giai đoạn 2 | lõi `RouletteRoutine` | `Time.deltaTime` | Tắt |
| 8 | Tiếng theo giai đoạn (`stageClips`) | mỗi giai đoạn của hộp | lõi `PlayStageClips` | sự kiện hộp | Tắt |
| 9 | Tiếng khoá + whoosh + rải accent | lúc chốt | lõi `Lock()` / `AccentRoutine` | `Time.deltaTime` | Tắt |
| 10 | Âm căng thẳng tổng hợp (3 nguồn: loop, nhịp tim, fx) | từ bấm mở tới chốt | `ChestTension` | `Time.deltaTime` | Tắt |
| 11 | Bước kịch bản `Sound` | tuỳ bước | `ChestScriptRunner` | `WaitForSeconds` | Không dùng trong chuỗi chuẩn |
| 12 | Sự kiện của `BoxOpenSequence` (`onKeyApproach`, `onKeyInserted`, `onKeyTurned`, `onBite`) | màn mở đầu của hộp | do **cảnh** nối loa vào | — | Ngoài hệ này; nếu cảnh có nối loa thì là nguồn thừa |

Mục 6 đến 10 vốn đã bị `ChestAudioRule.ApplyToProfile` làm câm (clip rỗng, `tension.enabled = false`) và `minimalFx` bỏ qua. **Không thấy chỗ nào nhân âm lượng tổng hai lần**: `volume` nhân đúng một lần ở nền và ở từng `PlayOneShot`.

Nguyên nhân khả dĩ của "âm bị lạc", xếp theo độ khả dĩ (suy từ mã, chưa đo):

1. **Nền chạy theo đồng hồ riêng 5 s, không theo nấc.** Chốt thật nằm ở khoảng 1,35 + 6,5 = 7,85 s sau bấm (lõi kéo giãn giai đoạn hé nắp cho đủ `minRouletteSeconds = 2,8 s`). Nền "dâng xong" từ giây 6,35 rồi nằm phẳng, trong lúc rương còn ở nấc 3. Không có tiếng nào đánh dấu từng nấc.
2. **Nấc có thời lượng cứng (1,2 / 1,6 / 2,2 s) không bám giai đoạn của hộp.** Nấc 1 xong lúc 2,55 s nhưng nấc 2 mới bắt đầu lúc 3,45 s (đứng yên 0,9 s); nấc 3 xong lúc 7,25 s trong khi chốt lúc 7,85 s (đứng yên 0,6 s). Tai nghe nền vẫn "căng" mà rương đứng yên.
3. **Hai đồng hồ.** Âm đo bằng thời gian thật (`unscaledTime`), còn hộp và `ChestMotion` đo bằng `Time.deltaTime` (bị Unity kẹp tối đa `maximumDeltaTime` = 0,33 s mỗi khung). Lần mở đầu tiên thường có khung hình dài (nạp shader, dựng lưới, tạo prefab hook), chuyển động chậm lại còn âm chạy tiếp, lệch vài trăm mili giây. Tạm dừng game (`timeScale = 0`) thì chuyển động đứng mà âm vẫn chạy.
4. **Tiếng chốt không bám bước bóp méo.** Bước bóp méo bắt đầu chậm 1 đến 2 khung so với tiếng chốt; Rác có 0,2 s đo bằng đồng hồ riêng; bậc cao vang ngay lúc chốt trong khi rương mới bắt đầu nảy.
5. **Clip chưa nạp khi phát lần đầu** có thể trễ (nếu import không "Preload Audio Data").

## 3. Thiết kế mới: một dòng thời gian

Nguyên tắc: **mỗi tiếng chỉ được kích bởi một sự kiện thật của chuyển động hoặc trạng thái**, không có đồng hồ riêng. Tối đa **3 lớp** cùng lúc, mỗi lớp là một `AudioSource` riêng do `ChestSoundHook` sở hữu (không `PlayOneShot`, không coroutine, mọi mốc đo bằng `Time.time`, cùng đồng hồ với hộp và `ChestMotion`).

### 3.1 Sự kiện và tiếng

| Sự kiện thật | Ai báo | Tiếng phát |
|---|---|---|
| Bấm mở | `ChestRevealSequence.Play` → `OnPress` | **Nền** vào (âm 0,25, cao độ 0,90), lặp |
| `nac:1` (nấc 1 bắt đầu) | bước chuyển động `ChestOpenScript` (trường `announce`) | Nền bắt đầu dâng tới (0,30, 1,00) suốt đúng thời lượng thật của nấc; **nhấn nấc 1** (nếu khe không rỗng) |
| `nac:2` | như trên, ở giai đoạn 1 của hộp | Nền dâng tới (0,38, 1,15); **nhấn nấc 2** |
| `nac:3` | như trên, ở giai đoạn 2 của hộp | Nền dâng tới (0,45, 1,30); **nhấn nấc 3** |
| Chốt bậc | `OnTierLocked` | Nhấn tắt trong 0,08 s. **Rác:** nền tắt phụt, lặng 0,2 s. **Bậc khác:** nền tắt dần 0,3 s |
| `bop` (bóp méo bắt đầu) | bước `squash-<bậc>` | **Tiếng chốt** theo bậc. Rác: đúng 0,2 s sau lúc chốt (tiếng buồn). Thường, Hiếm: ngay lúc bóp méo bắt đầu. Rất hiếm, Huyền thoại: lúc rương chạm **đỉnh lần nảy đầu** (nảy rồi vang) |
| Lưới an toàn | `ChestSoundHook` | Nếu sau chốt 0,25 s mà kịch bản không báo `bop` (kịch bản thiếu bước bóp méo) thì tiếng chốt vẫn vang, để không bao giờ mất tiếng chốt |

Lõi cho hook biết **thời lượng thật** của nấc qua `ChestScriptContext.GetMotion("nac:n")`, nên nền dâng đều đúng suốt nấc, và tiếng nhấn không bao giờ dài hơn nấc.

### 3.2 Nấc nối liền, tới đúng lúc chốt

Trường mới của bước kịch bản (`ChestStep`):

- `announce`: tên sự kiện báo ngay khi bước thật sự bắt đầu (với chuyển động: sau khi đã ghi bộ điều khiển để hook đọc thời lượng).
- `durationUntilStage` + `durationTail`: thời lượng = số giây còn lại tới giai đoạn đó theo kế hoạch của hộp (`ChestRevealSequence.SecondsUntilStage`, đã gồm phần lõi kéo giãn và màn mở đầu của hộp), cộng phần chồng. Nấc 1 chạy tới giai đoạn 1, nấc 2 tới giai đoạn 2, nấc 3 tới giai đoạn 3 (giai đoạn chốt) cộng 0,2 s chồng, nên **không còn khoảng đứng yên và không lệch với chốt dù lõi kéo giãn bao nhiêu**. Không tính được (không có hộp) thì dùng thời lượng dự phòng 2,1 / 1,6 / 2,8 s.

### 3.3 Tạm dừng và nạp trước

- Khi `Time.timeScale` về 0 (tạm dừng), 3 lớp âm cũng tạm dừng và tiếp tục khi mở lại; không còn cảnh âm chạy mà rương đứng.
- `ChestSoundHook.Awake` gọi `LoadAudioData()` cho mọi clip chưa nạp. Nên bật "Preload Audio Data" cho các mp3 rương (Inspector của file âm) để lần phát đầu không trễ.

### 3.4 Bảng số của lớp âm (`ChestSoundPlan`)

Mọi âm lượng ở đây là trước khi nhân hệ số tổng `volume = 0,8` (hook nhân **đúng một lần** trong hàm `Out`, và đó là chỗ duy nhất; kiểm bằng `check_chest_core.sh`).

| Lớp | Khi nào | Âm lượng | Cao độ | Trần (`ChestAudioRule`) |
|---|---|---|---|---|
| Nền | bấm mở | 0,25 | 0,90 | — |
| Nền xong nấc 1 / 2 / 3 | dâng mượt suốt nấc | 0,30 / 0,38 / 0,45 | 1,00 / 1,15 / 1,30 | 0,45 |
| Nhấn nấc 1 / 2 / 3 | lúc nấc bắt đầu | 0,25 / 0,38 / 0,50 | 0,90 / 1,00 / 1,12 | 0,50 |
| Tiếng chốt Rác → Huyền thoại | như bảng 3.1 | 0,55 / 0,70 / 0,80 / 0,90 / 1,00 | 1,0 | 1,00 |

Cộng với `bedGain`, `accentGain`, `lockGain` riêng của từng rương (mặc định 1) trong file dữ liệu.

## 4. Khe âm của mỗi rương

Mỗi rương có các **khe âm** trong file dữ liệu của nó (`Chests/<Tên>Preset.cs`). **Khe rỗng thì im, không báo lỗi.**

| Khe | Trường | Số phần tử | Mặc định (lấy từ file âm cũ có sẵn) |
|---|---|---|---|
| Nền | `bedFile` | 1 | `*_loop.mp3` của rương; riêng Âm nhạc, Thể thao, Thiên Cơ giữ nền cũ `drumroll`, `ambient`, `ambient_clockwork` (chủ dự án đã chê 3 nền ElevenLabs mới `*_bed_loop`; chúng vẫn nằm trong repo, chưa dùng) |
| Nhấn nấc 1, 2, 3 | `accentFiles` | 3 | nấc 1 **rỗng**; nấc 2 và 3 = tiếng ngắn cũ của rương (trước là "tiếng cắm", ví dụ `sfx_golden_token.mp3`) |
| Chốt Rác → Huyền thoại | `lockFiles` | 5 | `sfx_<tiền tố>lock_{rac,thuong,hiem,ratHiem,huyenThoai}.mp3` |

Các file `sfx_*_fly.mp3`, `sfx_*_action.mp3`, `sfx_*_key_insert.mp3` mới tạo **không bị xoá** và hiện không ô nào dùng (khe nhấn có thể trỏ tới chúng nếu muốn).

### 4.1 Mốc thời gian (để biết tiếng nhấn nên dài bao nhiêu)

Tính cho hộp mặc định (`BoxOpenSequence`: 5 giai đoạn, không có màn mở đầu) và hồ sơ mặc định (`minRouletteSeconds = 2,8`). Giây tính từ lúc bấm mở. **Thời điểm chốt do lõi quyết** (công thức: tổng các đoạn tới giai đoạn 3, giai đoạn 2 bị kéo giãn cho đủ 2,8 s); phần đã tính theo sự kiện nên tự co giãn theo.

| Giây | Sự kiện | Lớp âm |
|---|---|---|
| 0,00 | Bấm mở, nắp bắt đầu mở | Nền vào (0,25, 0,90) |
| 0,05 | `nac:1`, nấc 1 chạy tới giai đoạn 1, dài ≈ **2,05 s** | Nền dâng tới (0,30, 1,00); nhấn nấc 1 (mặc định rỗng) |
| 2,10 | giai đoạn 1 (Gỡ niêm): `nac:2`, dài **1,60 s** | Nền tới (0,38, 1,15); nhấn nấc 2 |
| 3,70 | giai đoạn 2 (Hé nắp): `nac:3`, dài ≈ **3,00 s** (2,80 + 0,2 chồng) | Nền tới (0,45, 1,30); nhấn nấc 3 |
| 6,50 | giai đoạn 3 (Bật nắp) = **chốt**; `bop` | Nhấn tắt; nền tắt (Rác phụt, bậc khác 0,3 s) |
| 6,50 / 6,70 | tiếng chốt: Thường, Hiếm 6,50 · Rác 6,70 | |
| ≈ 6,63 / 6,67 | tiếng chốt Huyền thoại / Rất hiếm (đỉnh nảy đầu = 0,5 · 1,0 s / số lần nảy, số lần nảy = 4 / 3 cộng `cycleDelta`) | |
| 7,70 | giai đoạn 4 (Mở toang) | |
| ≈ 8,30 | hộp mở xong | |

Thời lượng bóp méo sau chốt: Rác 1,10 s (ép 0,17 + giữ 0,70 + nhả 0,23) · Thường 0,90 · Hiếm 1,15 · Rất hiếm 1,40 · Huyền thoại 1,65 s; nhịp nảy (Jump) 1,00 s; Rất hiếm và Huyền thoại có thêm bay lên ngắn 2,4 s.

**Quy tắc cho phiên từng rương khi chọn tiếng nhấn:**

- Nấc 1 dài khoảng 2,0 s; nấc 2 khoảng 1,6 s; nấc 3 khoảng 3,0 s (co giãn theo lõi). Tiếng nhấn phát **một lần**, nên nên ngắn hơn nấc: ≤ 1,5 s cho nấc 1, 2 và ≤ 2,5 s cho nấc 3. Dài hơn nấc thì bị tắt dần 0,08 s đúng lúc hết nấc.
- Độ dài thực khi phát = độ dài file chia cho cao độ (0,90 / 1,00 / 1,12). Ví dụ `sfx_trunghoa_thud.mp3` (1,23 s) phát trọn ở cả 3 nấc.
- Tiếng chốt không bị cắt (rương lặng lại sau ≈ 1,1 đến 1,65 s, đuôi vang dài hơn là chủ ý, đặc biệt Huyền thoại 3,5 đến 5 s).
- Nền phải **lặp mượt** (độ dài trong bảng dưới); nền ngắn 3 đến 4 s sẽ lặp 2 đến 3 lần.

Độ dài file mặc định hiện tại (giây; ffprobe):

| Rương | Nền | Nhấn nấc 2 và 3 (mặc định) | Chốt Rác / Thường / Hiếm / R.Hiếm / H.Thoại |
|---|---|---|---|
| AmNhac | `amnhac_drumroll.mp3` 3,0 | `amnhac_coin_slot.mp3` 1,04 | 1,65 / 1,23 / 1,65 / 2,25 / 3,45 |
| CoPhong | `cophong_sword_hum_loop.mp3` 8,1 | `cophong_bamboo_knock.mp3` 0,63 | 2,04 / 1,65 / 2,25 / 3,03 / 5,04 |
| Game | `game_chip_loop.mp3` 8,1 | `game_button_click.mp3` 0,52 | 2,51 / 1,04 / 1,52 / 2,51 / 4,52 |
| Golden | `golden_spin_loop.mp3` 6,0 | `golden_token.mp3` 0,63 | 1,52 / 1,04 / 1,52 / 2,25 / 3,53 |
| HaiTac | `haitac_bed_loop.mp3` 8,1 | `haitac_thud.mp3` 0,84 | 1,52 / 1,04 / 1,52 / 2,04 / 3,53 |
| KhongGian | `khonggian_engine_loop.mp3` 8,1 | `khonggian_fuel_dock.mp3` 0,73 | 2,04 / 1,23 / 1,65 / 2,59 / 4,05 |
| Magic | `magic_rune_bed_loop.mp3` 8,1 | `magic_rune_stone.mp3` 0,73 | 1,52 / 1,52 / 2,04 / 2,80 / 4,05 |
| Mimic | `mimic_stomach_loop.mp3` 8,1 | `mimic_gulp.mp3` 0,63 | 2,04 / 1,23 / 1,80 / 2,51 / 3,53 |
| Miner | `miner_rumble_loop.mp3` 8,1 | `miner_thump.mp3` 0,52 | 1,65 / 1,04 / 1,52 / 2,51 / 4,05 |
| NhatBan | `nhatban_shakuhachi_loop.mp3` 8,1 | `nhatban_sheath.mp3` 0,44 | 2,25 / 1,23 / 1,80 / 2,80 / 4,05 |
| RacQuy | `racquy_bed_loop.mp3` 8,1 | `racquy_clank.mp3` 0,44 | 2,25 / 1,04 / 1,41 / 2,25 / 3,45 |
| Robot | `robot_hum_loop.mp3` 8,1 | `robot_slot.mp3` 0,73 | 1,65 / 1,04 / 1,41 / 2,04 / 3,45 |
| TheThao | `thethao_ambient.mp3` 4,0 | `thethao_kick.mp3` 0,44 | 2,04 / 1,52 / 2,51 / 3,53 / 5,04 |
| ThienCo | `thienco_ambient_clockwork.mp3` 3,5 | `thienco_gear_clank.mp3` 0,52 | 1,65 / 1,23 / 1,80 / 2,59 / 4,52 |
| TrungHoa | `trunghoa_lion_drum_loop.mp3` 8,1 | `trunghoa_thud.mp3` 1,23 | 2,04 / 1,52 / 2,51 / 3,53 / 5,04 |
| TuoiTho | `tuoitho_roll_loop.mp3` 6,0 | `tuoitho_bonk.mp3` 0,52 | 1,65 / 1,04 / 1,23 / 2,51 / 3,60 |
| VietNam | `vietnam_dan_bau_loop.mp3` 8,1 | `vietnam_thud.mp3` 0,63 | 2,51 / 1,52 / 2,04 / 3,03 / 5,04 |

(Tên file đầy đủ có tiền tố `sfx_`. Tiếng chốt Rác ghi ở cột đầu.)

## 5. Dữ liệu từng rương và bảng đăng ký (để 17 phiên sửa song song không đụng nhau)

- **Mỗi rương một file:** `Assets/_Project/Scripts/ChestFx/Chests/<Tên>Preset.cs`, lớp tĩnh `<Tên>Preset` có `public static readonly ChestPreset Data`. Chỉ dữ liệu, không logic (kiểm bằng script).
- **Bảng đăng ký:** `ChestPresetRegistry.cs`, đã có sẵn đủ 17 dòng `<Tên>Preset.Data`. **Không ai cần sửa file này.** `ChestPreset.cs`, `ChestSoundHook.cs`, `ChestOpenScript.cs` cũng là lõi dùng chung: không sửa trong phiên rương.
- 17 tên: Golden, HaiTac, RacQuy, Magic, Mimic, Miner, AmNhac, VietNam, TuoiTho, Robot, KhongGian, Game, TheThao, TrungHoa, NhatBan, CoPhong, ThienCo.

Các trường cần điền trong `Data`:

| Nhóm | Trường | Ý nghĩa |
|---|---|---|
| Nhận dạng | `chestId`, `displayName`, `audioFolder`, `filePrefix` | tên rương, khoá đăng ký, thư mục âm (kết thúc `/`), tiền tố tên file |
| Chuyển động | `moveStyle` | `Shake`, `Float`, `Spin`, `Wobble`, `Bounce` cho 3 nấc |
| | `shakeScale` | nhân biên độ 3 nấc (thường 1; lõi kẹp ≤ 1,5) |
| Bóp méo | `squashMul`, `racSquashMul`, `jumpMul`, `cycleDelta` | như kế hoạch 2026-11-26 (±20% theo tính cách; Squash kẹp ≤ 0,36, Jump kẹp ≤ 0,6 H, số chu kỳ tối thiểu 1) |
| Khe âm | `bedFile`, `accentFiles[3]`, `lockFiles[5]` | mục 4 |
| Độ to riêng | `bedGain`, `accentGain`, `lockGain` | nhân vào bảng 3.4 (mặc định 1) |

Editor: menu `Tools/Cho Do La/Mo Ruong/Setup <Tên> Chest` và `Setup All` (= `Create All Chest FX Profiles`) dựng hồ sơ, kịch bản và prefab `SoundHooks/<Tên>SoundHook.prefab` (gồm `ChestSoundHook` đã điền clip). Khe rỗng thì để trống không báo lỗi; khe có tên mà không thấy file thì cảnh báo vàng. Thư mục prefab cũ `KeyHooks` (nếu còn) bị xoá khi chạy Setup. Muốn xem mốc thời gian từng sự kiện âm trong Console, bật `logTimeline` trên `ChestSoundHook`.

## 6. Còn giữ nguyên

Vòng đổi màu 5 bậc, bóng vật phẩm, glow trong miệng, `revealLight`, hào quang chớp/tia/vòng lúc chốt, `PulseGlow` (API còn nhưng không còn ai gọi), `minimalFx = true` cho cả 17 hồ sơ. Quy tắc chung: ngẫu nhiên chỉ qua bộ có hạt giống của lõi, không Rigidbody/Light/hạt mới, không đổi `Time.timeScale`, mọi thứ dọn ở `OnReset`.

## 7. Hạt (và tia, vòng, bóng) bị tím

### 7.1 Nguyên nhân khả dĩ nhất

Màu tím (magenta) là màu Unity vẽ khi vật liệu không có shader dùng được. Mã cũ (`ChestFxFactory.FindShader`) có đường lui cuối cùng là `Shader.Find("Hidden/InternalErrorShader")`, **chính là shader tím**: chỉ cần hai shader đầu thiếu là tím. Các lý do shader thiếu, theo độ khả dĩ:

1. **Dự án chưa gán Render Pipeline Asset của URP** (Project Settings > Graphics hoặc mức Quality), dù gói URP 17.3 đã cài. Khi đó mọi shader `Universal Render Pipeline/...` đều vẽ tím, cả hạt (`Particles/Unlit`), tia/vòng/chớp (cùng shader) và bóng vật phẩm (`Unlit`). Repo chỉ có `ProjectSettings/ProjectVersion.txt` nên mình **không kiểm được** điều này.
2. **Shader bị gỡ khỏi bản build** (không vật liệu nào trong cảnh/Resources tham chiếu, không nằm trong Always Included Shaders), vì shader được tìm bằng tên lúc chạy. Dấu hiệu: tím chỉ ở bản build, trong Editor bình thường.
3. Vật liệu null: hệ hạt tạo bằng mã không được gán `sharedMaterial`, hoặc vật dựng bằng `CreatePrimitive` còn vật liệu mặc định (Standard) trong URP.
4. Thuộc tính không có trong shader: mã đã bọc bằng `HasProperty` nên an toàn; riêng shader dự phòng `Legacy Shaders/Particles/Additive` dùng `_TintColor`, mã cũ không đặt (đã thêm).

### 7.2 Đã sửa gì

- **`ChestShaderResolver`** là nơi **duy nhất** gọi `Shader.Find` (script kiểm đảm bảo). Nó xét pipeline đang chạy rồi duyệt chuỗi dự phòng, mỗi ứng viên phải tìm thấy, `isSupported`, và không phải shader `Hidden/`:
  - Hạt, tia, vòng, chớp (cộng sáng): URP `Particles/Unlit` → URP `Unlit` → `Legacy Shaders/Particles/Additive` → `Mobile/Particles/Additive` → `Sprites/Default` → `Particles/Standard Unlit` (Built-in).
  - Bóng vật phẩm, chấm bậc: URP `Unlit` → URP `Simple Lit` → `Unlit/Color` → `Sprites/Default` → `UI/Default`.
  - Vật vật lý: URP `Lit` → URP `Simple Lit` → `Standard` (Built-in).
  - Shader chỉ dành cho URP bị bỏ qua nếu pipeline đang chạy không phải URP (và ngược lại), nên không chọn nhầm shader sẽ tím.
- **Hết shader thì TẮT, không vẽ tím:** `CreateUnlit`/`CreateAdditive`/`CreateParticles` trả `null`; hệ hạt bị tắt, chớp/tia/vòng bị bỏ, bóng vật phẩm bị bỏ (viền bị bỏ riêng nếu shader dự phòng không cắt được mặt trước), vật vật lý bị ẩn.
- **Luôn gán `sharedMaterial`** cho `ParticleSystemRenderer` qua `ChestFxFactory.AssignParticleMaterial` (thiếu vật liệu thì tắt renderer và dừng hệ hạt).
- **Cảnh báo rõ:** một cảnh báo vàng nêu pipeline, từng shader đã thử và lý do hỏng; thêm cảnh báo `Hệ hạt "<tên>" bị TẮT ...` cho từng hệ hạt.

### 7.3 Chủ dự án tự kiểm trong Unity

1. Menu **`Tools/Cho Do La/Mo Ruong/Kiem Tra Shader`**: in pipeline đang chạy, từng shader ứng viên (tìm thấy/không, hỗ trợ/không), dựng thử một hệ hạt và báo shader nào được dùng, và đọc (chỉ đọc) xem shader URP đã nằm trong Always Included Shaders chưa.
   - Dòng "Pipeline đang chạy: Built-in" mà dự án có URP nghĩa là **chưa gán URP Asset**: vào Project Settings > Graphics (Default Render Pipeline) và Quality, gán URP Asset. Đây là sửa đúng, không thuộc việc của mình vì mình không động vào `ProjectSettings`.
2. Mở một rương rồi bấm menu **`Quet Vat Lieu Tim`** (chạy được lúc đang Play): in **đường dẫn** từng đối tượng bị tím (ví dụ `.../ChestRevealFx_xxx/Flash`) và lý do. Nếu thứ tím không nằm dưới `ChestRevealFx_*` thì nó không thuộc hệ rương (ví dụ `ScrollUnroll` có `Shader.Find("Universal Render Pipeline/Lit")` riêng).
3. Chỉ tím ở bản build: chạy **`Tao Vat Lieu Giu Shader`** (tạo vài vật liệu trong `Assets/_Project/Resources/ChestFx` để Unity luôn đưa shader hiệu ứng vào bản build; không sửa ProjectSettings hay manifest), build lại, rồi xem log bản build có dòng `[ChestFx] Không có shader nào dùng được...` hay không.
4. Nếu thấy cảnh báo `Hệ hạt "..." bị TẮT`: hiệu ứng đó biến mất thay vì tím, và cảnh báo đã nêu shader nào thiếu.

## 8. Chỉnh nhẹ chuyển động và bóp méo (số trước → sau)

Quy ước: mọi con số đổi tối đa ±15% (trừ chỗ ghi lý do), giữ mọi kẹp: Jump ≤ 0,6 H, Squash ≤ 0,36, lệch ngang ≤ ~0,1 S (nấc 3 kiểu Shake: 0,031 · 1,5 · S/0,5 = 0,093 S), mọi bước tự trả rương về chỗ cũ.

### 8.1 Vào/ra mượt (lõi `ChestMotion`)

| Chuyển động | Trước | Sau | Lý do |
|---|---|---|---|
| Rung mọi hướng (ShakeAll), vào / ra | 15% / 25% thời lượng, đường bậc ba | **17% / 28%**, đường bậc năm | vận tốc và gia tốc đều 0 ở hai đầu, hết giật lúc bắt đầu và kết thúc (+13% / +12%) |
| Bay lên (Float) và Hover, đoạn lên | ease-out (vận tốc lớn nhất ngay lúc bắt đầu) | smoothstep (vận tốc 0 lúc bắt đầu) | hết cú "bật" lúc nâng rương |
| Lắc (Wobble) | bắt đầu ngay | thêm 12% đầu vào mượt | không mở đầu bằng một cú lắc |

### 8.2 Bóp méo có nảy dư

| Mục | Trước | Sau | Lý do |
|---|---|---|---|
| Squash: mũ đuôi tắt dần | 1,00 (tuyến tính) | **0,87** (−13%) | cú nảy dư sau nhát ép còn rõ rồi mới lắng (đỉnh nảy ngược chu kỳ 1: 25% → 30% biên độ); biên độ đỉnh không đổi, vẫn về đúng 0 |
| Rác (SquashHold): ép / giữ / nhả | 0,15 / 0,70 / 0,20 s = 1,05 s | **0,17 / 0,70 / 0,23 s = 1,10 s** | ép chậm hơn chút, nhả chậm hơn (+13% / 0% / +15%) |
| Rác: hình dạng | ép thẳng tới 100%, giữ phẳng, nhả mượt không nảy | ép tới 92%, **chìm thêm** tới 100% trong lúc giữ (hụt hơi), nhả bằng lò xo giảm chấn: nảy dư tối đa **9,5% biên độ** (cao hơn dáng gốc 2,8%) rồi lắng | "hụt hơi" tự nhiên, không dừng cứng; đỉnh dẹt vẫn ≤ biên độ nên Squash ≤ 0,36 giữ nguyên |
| Squash Thường | 0,10 | **0,11** (+10%) | tách rõ khỏi Rác và nấc 3 |
| Nhịp nảy (Jump) của bóp méo | 0,90 s | **1,00 s** (+11%) | Huyền thoại nảy 4 lần: mỗi cú 0,225 s → 0,25 s bớt dồn dập |
| Biên độ Squash các bậc khác, độ cao Jump, `cycles` | 0,16 / 0,22 / 0,30; 0,18 / 0,35 / 0,60 H; 2 / 3 / 4 | giữ nguyên | |
| Bay mừng bậc cao | 0,10 và 0,15 H, 2,4 s | giữ nguyên | |

### 8.3 Nấc 1 → 2 → 3 tăng dần rõ hơn

Biên độ nhân `S/0,5` (mét) hoặc chiều cao `H`; thời lượng nấc nay **nối liền tới giai đoạn kế** (mặc định 2,05 / 1,60 / 3,00 s, trước 1,2 / 1,6 / 2,2 s cứng và có khoảng đứng yên giữa nấc).

| Kiểu | Số | Nấc 1 | Nấc 2 | Nấc 3 |
|---|---|---|---|---|
| **Shake** | biên độ rung (m) | 0,008 → **0,0072** (−10%) | 0,016 (giữ) | 0,028 → **0,031** (+11%) |
| | tần số (Hz) | 30 → **27** (−10%) | 30 | 30 → **34** (+13%) |
| **Float** | rung (m) | 0,006 → **0,0055** (−8%) | 0,005 → **0,0055** (+10%) | 0,006 → **0,0068** (+13%) |
| | tần số rung (Hz) | 26 → **24** (−8%) | 26 | 26 → **29** (+12%) |
| | bay lên (× H) | không có | 0,15 → **0,14** (−7%) | 0,30 → **0,33** (+10%) |
| | xoay chậm (độ) | không có | 20 → **18** (−10%) | 40 → **44** (+10%) |
| **Spin** | xoay (độ) | 360 | 720 | 1080 (giữ, phải là bội 360) |
| | rung (m) | 0,005 → **0,0045** (−10%) | 0,005 | 0,005 → **0,0057** (+14%) |
| **Wobble** | góc lắc (độ) | 6 → **5,5** (−8%) | 10 | 15 → **16,5** (+10%) |
| | rung (m) | 0,004 → **0,0036** (−10%) | 0,004 | 0,004 → **0,0046** (+15%) |
| | chu kỳ lắc x / z | 3 / 2,5 → **5 / 4** | 4 / 3,5 | 5 / 4,5 → **7 / 6** |
| **Bounce** | cao nảy (× H) | 0,06 → **0,055** (−8%) | 0,12 | 0,20 → **0,22** (+10%) |
| | lắc (độ) | 4 → **3,7** (−8%) | 6 | 9 → **10** (+11%) |
| | số lần nảy | 2 → **4** | 3 | 4 → **5** |

Ghi chú: số chu kỳ lắc và số lần nảy phải đổi theo độ dài nấc (nấc 1 dài 1,2 → 2,05 s, nấc 3 dài 2,2 → 3,0 s) để nhịp thực tế vẫn như cũ (lắc ≈ 2 đến 2,5 Hz, mỗi cú nảy ≈ 0,5 đến 0,6 s); đó là giữ nhịp, không phải đổi số. Nấc 1 nay bắt đầu ngay lúc bấm (trước là sau 1,35 s của vật mở khoá).

## 9. Chưa kiểm chứng (nói thật)

- **Chưa chạy Unity, chưa nghe.** Biên dịch bằng bản stub (`mcs`) chỉ bắt lỗi cú pháp và kiểu, không bắt lỗi API thật của Unity.
- Chưa biết dự án có gán URP Asset chưa, nên **nguyên nhân hạt tím vẫn là suy đoán có xếp hạng**; menu kiểm sẽ cho biết chắc.
- Các mốc thời gian mục 4.1 tính trên hộp và hồ sơ mặc định; hộp có màn mở đầu (`keyIntro`, `biteIntro`) hay `waitForPlayer` thì lõi cộng thêm/không tính được (rơi về thời lượng dự phòng).
- Chưa biết các nền ngắn (Âm nhạc 3,0 s, Thiên Cơ 3,5 s, Thể thao 4,0 s) có lặp mượt không; chưa nghe 17 tiếng nhấn mặc định có hợp ở cao độ 1,0 và 1,12 không (chủ dự án chỉnh bằng `accentFiles`, `accentGain`).
- `Time.time` và `Time.deltaTime` cùng tích luỹ nên khớp nhau, nhưng khung hình quá dài vẫn làm cả hộp lẫn chuyển động chậm lại cùng nhau (chuyện này là chủ ý, vì âm bám sự kiện chứ không bám giờ thật). Riêng clip chưa nạp vẫn có thể trễ lần phát đầu nếu import không bật Preload Audio Data.
- Thân lõi không còn gọi `PulseGlow`; API giữ lại để dùng sau.

## 10. Cách kiểm trong repo

```bash
bash Tools/CompileCheck/check_all.sh          # biên dịch toàn bộ Scripts + Editor bằng stub
bash Tools/CompileCheck/check_chest_core.sh   # gồm check_all + dữ liệu 17 rương, bảng đăng ký, tệp âm có thật, hook âm, shader
bash Tools/CompileCheck/check_chest_fx.sh     # check_particle_velocity + check_all
bash Tools/CompileCheck/check_particle_velocity.sh
```

`check_chest_core.sh` (đổi tên từ `check_chia_khoa.sh`) kiểm: đủ 17 file `*Preset.cs` đúng trường và không logic; bảng đăng ký đủ 17 dòng; mọi tệp âm ghi trong dữ liệu có thật; `ChestSoundHook` không dùng `PlayOneShot`, coroutine, `unscaledTime` và chỉ nhân `volume` ở một chỗ; không còn dấu vết vật mở khoá; `Shader.Find` chỉ nằm trong `ChestShaderResolver` và không còn dự phòng `InternalErrorShader`.

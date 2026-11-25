# Kế hoạch: thu gọn hiệu ứng mở rương về mức Hải tặc / Rác Quý + chuẩn hoá âm thanh

Ngày 2026-11-25 · Nhánh kế hoạch `claude/chest-trim-plan` · Người lập: phiên lập kế hoạch (không sửa mã hiệu ứng).
Người thực hiện: các phiên Sonnet, mỗi phiên một mục, dùng prompt mẫu ở
[2026-11-25-thu-gon-prompt-mau.md](2026-11-25-thu-gon-prompt-mau.md).

## 0. Tóm tắt cho chủ dự án (lời đơn giản)

- **Thước đo** lấy từ hai rương mẫu Hải tặc và Rác Quý: tối đa **2 nhân vật/đồ vật lớn** quanh rương, mọi thứ đứng
  trong vòng **1,5 lần cỡ rương**, vật rơi/văng tối đa **40 món** ở Huyền thoại (Rác ≤ 5), lúc chốt bậc chỉ **2 tiếng**
  cùng lúc, cả màn ≤ **11 giây**, 1–2 phím tương tác không bắt buộc.
- **Âm thanh**: tắt hẳn lớp "âm căng" chung và các tiếng lõi (tick, nền, tiếng xu rải, tiếng vút) ở **cả 17 rương**.
  Mỗi rương chỉ còn **một bản nhạc nền** do chính nó phát: 1 vòng nền + 1 nhịp, dâng dần từ lúc bấm. Lúc chốt:
  Rác **tắt phụt → lặng 0,2 giây → 1 tiếng xịt**; bậc cao **1 tiếng chốt + 1 tiếng nổ**.
- **Cắt nhiều nhất**: Việt Nam (bỏ cả mặt sông, rùa, chim; còn 5 hoa đăng bay vào miệng rương), Tuổi thơ (bỏ tờ giấy,
  xe, con quay, hộp bút; còn bi – 3 domino – ná), Thể thao (bỏ trọng tài, 3 bảng, VAR, mưa bóng), Không gian (gỡ hook
  cũ chạy song song, bỏ thùng dù, đồng hồ nhiên liệu, pháo hoa), Game (bỏ màn CRT phủ màn hình, voxel 440 → 80),
  Trung Hoa (thăng thiên bay quá cao, 9 đèn lồng → tối đa 3).
- **Bù thêm**: Golden (thêm phím kéo cần gạt, Thường không còn "xịt" như Rác), Thợ Mỏ (hiện kịch bản đào **không bao giờ
  tự chạy** — sửa cho chạy ngay khi bấm và rút ngắn).
- **Lỗi lớn chung**: menu "Create All Chest FX Profiles" (dùng cho menu cầm rương) vẫn gắn **hook cũ** cho 7 rương
  (Golden, Magic, Mimic, Thợ Mỏ, Robot, Game, Âm nhạc), nên khi thử bằng menu cầm rương sẽ thấy bản cũ đơn giản.
  Golden còn bị **bắn xu hai lần** (kịch bản lõi + hook máy xèng). Không gian chạy **hai hook cùng lúc**.

## 1. Cách đo (đơn vị)

- **S** = cạnh lớn nhất của khung bao rương (như `_size` trong `RacQuyHook.Measure`). **H** = chiều cao rương.
- "Tâm rương" = tâm khung bao chiếu xuống đáy. Khoảng cách tính theo mặt ngang.
- **Diễn viên** = vật dựng có hình riêng, sống lâu hơn 1 giây (nhân vật, máy móc, bảng…). KHÔNG tính: mảnh vật lý
  (xu, rác, đạn), hệ hạt, sàn/hộp va chạm vô hình, đồ gắn chặt lên chính rương (chân robot, ô quay trên nắp tính là
  "đồ của rương", không phải diễn viên).
- **Lớp âm** = một nguồn đang phát kéo dài (vòng lặp) hoặc một chuỗi tiếng lặp đều (nhịp). Tiếng lẻ = **sự kiện**.

## 2. THƯỚC ĐO (rút từ HaiTacSeqHook + RacQuyHook)

Số đo hai mẫu:

| Hạng mục | Hải tặc (`HaiTacSeqHook`) | Rác Quý (`RacQuyHook`) |
|---|---|---|
| Diễn viên | 2: bè + đại bác (cách 1,3 S), vẹt (vào từ ~2 S, bay vòng 1,1 S) | 1: mèo (vào từ 2,6 S, rình ở 1,0 S, đi ra 4 S) |
| Nền cảnh sát rương | mặt nước bán kính 1,75 S | không |
| Vật lý trước chốt | đạn ≤ 3 tự bắn (+6 người chơi, tự huỷ 3,5 s), thùng ≤ 4 | rác ~45% mỗi bước màu (~8 món) |
| Vật lý sau chốt (Rác→HT) | xu 0/0/5/14/30 (+3/thùng) | rác 1/8/14/22/32 |
| Hệ hạt riêng | 5 (lớn nhất: 160 hạt cột nước HT) | 3 (lớn nhất 70 hạt HT) |
| Đèn Light riêng | 0 | 0 |
| Tương tác | 2 phím (G bắn, B thả thùng), tự chạy nếu không bấm | 1 (F/chuột phải ném xương) |
| Thời gian | chốt ~7,4 s, kết ~2,5 s | chốt ~6,5–7 s, kết ~2 s |
| Cả rương chuyển động | chìm/trôi/bật/lộn | nghiêng/giật/đổ 40–85° |

**Thước chung cho mọi rương (con số mục tiêu):**

| Mã | Quy định |
|---|---|
| T1 | Diễn viên ngoài rương **≤ 2**. Cộng tối đa 1 "nền cảnh" sát chân rương (nước, mây, sàn giấy) bán kính **≤ 1,75 S**. |
| T2 | Vật đứng/lơ lửng: tâm **≤ 1,5 S** từ tâm rương, cao **≤ 2 S** trên đáy rương. Vật đi vào/ra: điểm xuất phát/biến mất **≤ 3 S**, đoạn ở ngoài 1,5 S kéo dài **≤ 1,2 s**. Hạt/vật bay lên lúc kết: **≤ 3 S**. |
| T3 | Rigidbody cùng lúc: trước chốt **≤ 12**; sau chốt theo bậc Rác/Thường/Hiếm/Rất hiếm/HT **≤ 5 / 10 / 16 / 25 / 40**. Mọi số lượng có trần cứng trong mã (kể cả phần thưởng từ tương tác). |
| T4 | Hệ hạt riêng **≤ 5**; một lần phát **≤ 60 hạt**, riêng cú Huyền thoại **≤ 160**. |
| T5 | Đèn Light riêng **≤ 1**. |
| T6 | Thấy/nghe thứ đầu tiên **≤ 0,2 s** sau bấm. Không chờ người chơi quá **3 s** (có tự động thay). Đoạn kết sau chốt **≤ 3 s**; mọi vật thừa biến mất/đứng yên **≤ 4 s** sau chốt (trừ vật nằm lại trên sàn như rác/xu, được phép tới Reset). Tổng màn **≤ 11 s**. |
| T7 | Tương tác **1–2 hành động**, không bắt buộc, mỗi hành động có trần số lần (≤ 6). Phím KHÔNG dùng: T, Y, Space, R, C (bàn thử) và G/K/L ở rương Golden (cổng xu của bàn thử). |
| T8 | Cả rương phải chuyển động (nảy/nghiêng/lộn). Trước chốt: dịch **≤ 0,05 S**, nghiêng **≤ 8°**, giống nhau mọi bậc. Lúc kết: nhảy **≤ 1,2 H** (HT), xoay **≤ 540°**. |
| T9 | Cấm: đổi `Time.timeScale`; phủ màn hình (CRT, UI); đẩy Rigidbody có sẵn của cảnh; chữ nổi dài hơn 1 dòng ngắn; mọi thứ "ở một nơi xa rương" (hiệu ứng phải bám rương hoặc lao vào/ra khỏi rương). |
| T10 | Mỗi rương **đúng 1 hook** chạy. Hook cũ để nguyên file nhưng không được gắn ở bất kỳ đường nào (menu, thư viện, prefab, bàn thử). |
| T11 | Bất biến giữ nguyên: bậc do game quyết (`ChestRevealSequence.Play`), hook chỉ đọc bậc ở `OnTierLocked`; ngẫu nhiên chỉ trang trí theo hạt giống của hook (KHÔNG dùng `UnityEngine.Random`); hạt có velocity/orbital chỉ đặt qua `ChestFxFactory.SetVelocity`; vòng màu 5 bậc và bóng vật phẩm do lõi chạy. |

## 3. QUY TẮC ÂM THANH CHUNG (áp cho cả 17 rương, không ngoại lệ trừ chỗ ghi rõ)

**Quyết định về `tension.enabled`: TẮT (`false`) ở cả 17 rương.** Lý do: 16/17 hook đã tự dựng nền dâng + kết theo
bậc của riêng nó; để lõi chạy thêm vòng nền + nhịp tim + tiếng dâng/ầm tổng hợp là nguồn chồng âm lớn nhất. Mimic và
Nhật Bản có sẵn nhánh mã "không có lõi" (`!CoreTension`) để tự phát nền, nên tắt là chạy được.

**A1 – Hồ sơ lõi (profile) gần như câm.** Với mọi rương, hồ sơ được ghi lại MỖI LẦN chạy menu Setup (không chỉ khi
trống):
`tension.enabled = false`, `tickClips = []`, `openAmbientClip = null`, `accentClips = []`, `stageClips = []`,
`whooshClip = null`, `lockClipsByTier = 5 ô null`. **Không ngoại lệ.** Hai rương đang dựa vào tiếng chốt của lõi
(Nhật Bản: `fin_*`; Rác Quý: `sfx_racquy_lock_*`) chuyển các clip đó vào một mảng `stingClips[5]` trên hook và hook tự
phát theo A4. Lõi chỉ còn lo hạt, màu, bóng vật phẩm.

**A2 – Mỗi rương một "bản nhạc nền" do hook phát, tối đa 2 lớp trước chốt:**
- **Nền** (1 vòng lặp): bắt đầu ≤ 0,2 s sau bấm, âm lượng 0,15 → 0,45, cao độ tăng 30–60% tới lúc chốt.
- **Nhịp** (1 chuỗi tiếng lặp: trống, tích-tắc, kick, tim…): khoảng cách 0,6–0,8 s lúc đầu → 0,12–0,18 s sát chốt,
  âm lượng 0,25 → 0,55. Rương nào nền đã có nhịp bên trong thì không cần lớp Nhịp.
- Không được có lớp thứ 3 kéo dài (tiếng đám đông, tiếng ruồi lặp, tiếng drone bay lặp, tiếng cát lặp… phải bỏ hoặc
  thay bằng 1 tiếng lẻ).

**A3 – Sự kiện trước chốt:** tối đa 1 tiếng lẻ mỗi 0,2 s (dùng chung một mốc `ChestAudioRule.TryGate`), âm lượng
0,3–0,5. Tiếng do va chạm vật lý (xu, rác, đá, bóng): cách nhau ≥ 0,12 s, âm lượng ≤ 0,4.

**A4 – Khoảnh khắc chốt (trong 0,3 s đầu sau `OnTierLocked`) tối đa 2 tiếng:**
- **TC – tiếng chốt theo bậc** (đúng 1 clip, luôn do hook phát): đoạn kết riêng của hook (`end_*`, `endingClips`,
  `blastClips`, `stingClips`…).
- **HĐ – tiếng hành động** (≤ 1): nổ/đổ/bắn… chỉ ở Thường trở lên.
- **Rác**: Nền và Nhịp **dừng ngay** (`Stop()`, không fade) → **lặng 0,2 s** → **1 tiếng xịt** (TC Rác). Không có HĐ.
  Không phát gì khác trong 0,6 s tiếp theo.
- **Thường**: Nền fade 0,3 s; TC âm lượng ≤ 0,7; HĐ tuỳ chọn ≤ 0,5.
- **Hiếm**: Nền dừng; TC 0,85; HĐ ≤ 0,7.
- **Rất hiếm / HT**: Nền dừng (hoặc vọt cao rồi tắt trong ≤ 0,4 s); TC 1,0; HĐ 0,8. Nếu đoạn kết riêng đã chứa tiếng
  nổ thì KHÔNG phát thêm HĐ nổ.

**A5 – Đuôi (từ +0,4 s sau chốt):** tối đa 1 tiếng mỗi 0,15 s, tổng ≤ 6 tiếng, âm lượng ≤ 0,4 (kêu của nhân vật, xu
leng keng, chuông nhỏ). Hết đuôi trước +3 s.

**A6 – Bậc âm lượng (sau khi nhân `volume` tổng của hook, mặc định 0,8):** Nền ≤ 0,45 < Nhịp ≤ 0,55 < Sự kiện ≤ 0,5
< HĐ ≤ 0,8 < TC ≤ 1,0. Không gì to hơn TC. Không nhân `volume` hai lần (lỗi đang có ở Hải tặc).

**A7 – Kỹ thuật:** mỗi hook tối đa 3 `AudioSource` riêng (nền, nhịp, sfx). Tiếng lẻ cần cao độ khác 1 thì phát bằng
`ChestRevealSequence.PlayOneShot(clip, vol, pitch)` (lõi tạo nguồn riêng), KHÔNG đổi `pitch` của nguồn đang ngân.

**A8 – Trình tự chuẩn của một rương (để kiểm bằng tai):** bấm → (0–0,2 s) nền vào nhỏ → nhịp chen vào → dâng đều,
không có khoảng ồn đột ngột → (chốt) Rác: im phắc rồi "xịt" / bậc cao: "chốt + nổ" → đuôi nhỏ thưa → im trước +3 s.

## 4. PHIÊN 0 – việc chung (làm TRƯỚC, một phiên riêng; các phiên rương đợi phiên này xong)

0.1 **Tạo `Assets/_Project/Scripts/ChestFx/ChestAudioRule.cs`** (static class, namespace `ChoDoLa.ChestFx`):
- hằng số ngân sách: `BedMax = 0.45f`, `BeatMax = 0.55f`, `EventMax = 0.5f`, `ActionMax = 0.8f`, `StingMax = 1f`,
  `TailMax = 0.4f`, `EventGap = 0.2f`, `ImpactGap = 0.12f`, `TailGap = 0.15f`, `RacSilence = 0.2f`;
- `public static void ApplyToProfile(ChestFxProfile p)`: đặt đúng các ô ở A1;
- `public static bool TryGate(ref float lastTime, float gap)`: trả true và cập nhật `lastTime = Time.time` nếu đã qua
  `gap` giây.
  Thêm file vào stub biên dịch nếu cần (không phụ thuộc API mới).

0.2 **`Editor/CreateAllChestFxProfiles.cs`** – nối thư viện menu cầm rương với hook KỊCH BẢN (T10):

| Rương | Maker hồ sơ | `RootHookTemplate` | Cờ thư viện |
|---|---|---|---|
| Golden | giữ `ChestRevealTestSetup.CreateOrUpdateGoldenProfile` | `SaveHook<GoldenSlotHook>(key, GoldenSlotSetup.AssignClips)` | |
| Magic | giữ | `SaveHook<MagicRuneRitual>` + gán âm: tách đoạn gán âm trong `MagicRuneFxSetup.Setup` ra `public static void FillClips(MagicRuneRitual r)` | |
| Mimic | `MimicFxSetup.CreateOrUpdateProfile` rồi áp phần chỉnh hồ sơ của `MimicFeedSetup` (tách ra hàm public) | `SaveHook<MimicFeedHook>` + gán 16 âm (tách hàm `FillClips` từ `MimicFeedSetup.AddToSelected`) | `biteIntro = false` |
| Miner | giữ | `SaveHook<MinerDigSequence>(key, null)` | |
| Robot | giữ | `SaveHook<RobotTransformHook>(key, RobotTransformSetup.Fill)` | |
| Game | `GameVoxelFxSetup.CreateOrUpdateProfile` | `SaveHook<GameVoxelHook>` + gán âm (tách hàm `FillClips` từ `GameVoxelFxSetup`) | |
| AmNhac | đổi hàm tạo hồ sơ riêng trong `AmNhacBassSetup` (quanh dòng 85) thành `public static ChestFxProfile CreateOrUpdateProfile()` và dùng nó | `SaveHook<AmNhacBassHook>` + gán âm (tách hàm từ `AmNhacBassSetup.WireSelected`) | `spinRecord = false` |
| CoPhong | sửa `CoPhongNguKiemSetup.Run` thành hàm trả hồ sơ, dùng làm maker | null (prefab) | |
| VietNam | dùng hàm tạo trong `VietNamHoiLangSetup` (đổi thành trả hồ sơ) | null (prefab `VietNamHoiLangFx`) | |
| KhongGian | giữ `KhongGianFxSetup.CreateOrUpdate` | null | |

0.3 **Gỡ đường gắn hook cũ / trùng:**
- `Scripts/ChestFx/ChestRevealDemo.cs` dòng ~118 và `Editor/ChestRevealTestSetup.cs` dòng ~99: bỏ tự thêm
  `GoldenGlintHook`.
- `ChestFxPresets.ApplyGoldenScript`: chỉ giữ 2 bước `cong-xu` (tắt mặc định) và `mo-ruong`; XOÁ các bước `rac-xep`,
  `rac-vai-xu`, `thuong-xu`, `hiem-nhay`, `hiem-xu`, `hiem-no`, `huyen-thoai-bay`, `huyen-thoai-xoay`,
  `huyen-thoai-xu`, `huyen-thoai-no`, `lap-lanh` (GoldenSlotHook đã làm xu + chuyển động; chạy cả hai = xu bắn hai
  lần và hai bộ ghi vị trí rương đánh nhau). Sửa `ChestRevealTestSetup` để hồ sơ Golden đã có script cũ được dựng lại
  script (gọi lại `ApplyGoldenScript` mỗi lần chạy menu).
- `KhongGianFxSetup`: prefab chỉ mang `KhongGianMissionHook` (xem mục Không gian).
- Mỗi menu "Add … To Selected" của rương kịch bản phải gỡ/tắt hook cũ cùng loại trên rương (Golden: `GoldenGlintHook`;
  Magic: `MagicHook` đã làm; Miner: `MinerHook` đã làm; …) — kiểm lại từng menu, thêm dòng gỡ nếu thiếu.

0.4 Biên dịch `bash Tools/CompileCheck/check_all.sh`. Commit "Phiên 0: quy tắc âm chung + nối thư viện với hook kịch bản".

## 5. MỤC TỪNG RƯƠNG

Mỗi mục: **Giữ** (ý tưởng đặc trưng, không được đổi) · **Đo hiện tại** · **CẮT** · **BÙ** · **SỬA LỖI** · **ÂM** ·
**Kiểm tra** · **Bản chốt**. Mọi mục đều làm thêm bước chung: trong Setup của rương gọi
`ChestAudioRule.ApplyToProfile(p)` (A1) mỗi lần chạy menu, và cập nhật spec `2026-11-25-kichban-<rương>-design.md`
(bảng nhịp + số mới, ghi "đã thu gọn theo kế hoạch 2026-11-25").

---

### 5.1 Hải tặc — `HaiTacSeqHook.cs`, `HaiTacSeqParts.cs`, `Editor/HaiTacSeqSetup.cs` (MẪU: chỉ rà lỗi nhỏ + âm)

**Giữ:** biển dâng, đại bác trên bè, vẹt gọi bậc, G/B, rương chìm–bật–lộn bằng mô phỏng lò xo.
**Đo hiện tại:** đạt thước hình. Âm vượt thước: ngoài nền `buildClip` của hook còn có âm căng lõi (vòng + nhịp tim +
dâng + ầm), sóng nền `sfx_haitac_waves_bed` (openAmbient, vào lúc vòng màu), tick la bàn, xu/hải âu rải (accent),
tiếng vỡ niêm + kẽo kẹt (stage 1, 2), vút bóng → 5–7 lớp.
**CẮT:** không cắt hình.
**SỬA LỖI:**
1. Âm lượng nhân hai lần: `Play()` đã nhân `volume`, nhưng `Boom()` gọi `Play(cannonClip, volume * loud, …)` và
   `Finale()` gọi `Play(Ending(…), volume)`. Đổi thành `Play(cannonClip, loud, …)` và `Play(Ending(…), 1f)`.
2. Trong `Finale` vòng `for (int i = 1; i < booms; i++) Boom(…)`: đoạn kết `end_*` ĐÃ chứa loạt nổ → thêm tham số
   `bool sound` cho `Boom`, ở vòng này truyền `false` (chỉ khói, giật nòng, té nước).
3. Rác: bỏ `Play(fizzleClip, 0.9f)` ở lúc chốt (đoạn kết 0 đã có "pfft"); `Play(drainClip…)` dời +0,6 s, âm lượng 0,5;
   `ParrotCall` cho tiếng kêu bắt đầu +0,8 s.
4. Bậc cao: `drainClip` dời +0,5 s, âm 0,4; `SquawkTimes(n…)` với `n = Mathf.Min(rank, 2)`, bắt đầu +0,8 s;
   `CoinRain` tiếng xu qua `ChestAudioRule.TryGate(ref _lastCoin, ImpactGap)`, âm 0,25.
5. `AnimateScene`: `Random.value < 0.25f` (bọt khí) → dùng `_rng.NextDouble()` (T11).
6. Thêm `public override void OnPress(ChestRevealSequence r) { _reveal = r; if (!_running) Begin(); }` (giữ dò
   `IsPlaying` làm dự phòng) để âm vào đúng khung bấm.
7. Trần xu: `int coins = Mathf.Min(40, coinsByTier[rank] + …)`.
**ÂM:** `HaiTacSeqSetup.CreateOrUpdateProfile` → `ChestAudioRule.ApplyToProfile(p)` (bỏ sóng nền, tick, accent, stage,
whoosh, âm căng). Nền = `buildClip` (đã có trống trận bên trong → không cần lớp Nhịp riêng).
**Kiểm tra:** đếm lớp âm trước chốt = 1; lúc chốt Rác = "pfft+kèn buồn" sau 0,2 s im; HT = 1 tiếng chốt (đoạn kết) +
1 phát đại bác; số xu ≤ 40.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 2 (bè-đại bác, vẹt) + nước 1,75 S |
| Rigidbody sau chốt Rác→HT | 1 / 2 / 7 / 16+thùng / ≤ 40 |
| Lớp âm trước chốt | 1 (build) |
| Tiếng lúc chốt | Rác 1 · khác 2 |
| `tension.enabled` | false |

---

### 5.2 Rác Quý — `RacQuyHook.cs`, `Editor/RacQuyFxSetup.cs` (MẪU: rà lỗi nhỏ + âm)

**Giữ:** mèo lật thùng, rác vật lý nằm lại, xương cá F/chuột phải, thùng nghiêng/đổ.
**Đo hiện tại:** đạt thước hình. Âm: vòng `buildLoopClip` + âm căng lõi + tick lõi + nền shimmer lõi + lock lõi +
accent lon lăn + stage clips + whoosh; lúc chốt HT phát `boom + clatter + yowl` cùng khung + lock + whoosh (5 tiếng).
**CẮT:** trước chốt tối đa 10 món rác: thêm đếm `_preLockTrash`, `OnRouletteStep` chỉ `SpawnTrash` khi `< 10`.
**SỬA LỖI:**
1. `_baseRot` chỉ chụp lần đầu (`if (!_hasBaseRot)`), không bao giờ chụp lại → mở lần 2 sau khi rương bị xoay (cầm
   tay) sẽ bị giật về hướng cũ. Trong `Cleanup()` sau khi trả rotation, đặt `_hasBaseRot = false`.
2. `EnsureFloor`: `bc.size = new Vector3(6f, 0.1f, 6f)` (mét cố định) → `new Vector3(4f * _size, 0.1f, 4f * _size)`.
3. `OnRouletteStep`: tiếng `clankClip` qua `ChestAudioRule.TryGate(ref _lastClank, EventGap)`.
**ÂM:** `RacQuyFxSetup.CreateOrUpdate` → `ChestAudioRule.ApplyToProfile(p)`. Thêm trường
`public AudioClip[] stingClips = new AudioClip[5]` vào `RacQuyHook`; `RacQuyFxSetup.FillClips` gán
`{null, sfx_racquy_lock_thuong, …_hiem, …_ratHiem, …_huyenThoai}` (cùng các file `OwnLockFiles` đang dùng). Hook phát
`stingClips[rank]` âm 0,85–1,0 làm TC ở Thường+. Nền = `buildLoopClip`. Lúc chốt trong `OnTierLocked`:
- Rác: giữ `_buildDive` (tụt pitch 0,4 s) → đổi thành `StopBuild()` ngay + phát `deflateClip` sau 0,2 s
  (coroutine); BỎ `fartClip`.
- Thường: `stingClips[1]` (TC 0,7) + `clatterClip` 0,5 (HĐ).
- Hiếm: `clatterClip` 0,7; `hissClip` dời +0,5 s.
- RH/HT: `boomClip` (HĐ) giữ; `clatterClip` dời +0,3 s âm 0,5; `yowlClip` dời +0,7 s âm 0,4.
- `fliesClip` lúc bấm: âm 0,3 (là sự kiện, không lặp).
**Kiểm tra:** như mục chung; rác trước chốt ≤ 10; mở 2 lần liên tiếp sau khi xoay rương → không giật hướng.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 1 (mèo) |
| Rác trước chốt / sau chốt Rác→HT | ≤ 10 / 1, 8, 14, 22, 32 (trần 40) |
| Lớp âm trước chốt | 1 (build loop) |
| Tiếng lúc chốt | Rác 1 · khác 2 |
| `tension.enabled` | false |

---

### 5.3 Golden — `GoldenSlotHook.cs`, `Editor/GoldenSlotSetup.cs` (QUÁ ĐƠN GIẢN + bị nhân đôi)

**Giữ:** máy xèng trên nắp (3 ô quay + cần gạt), ô 3 giữ bí mật, mèo thần tài, xu Rigidbody.
**Đo hiện tại:** hình gọn (đồ gắn lên rương + mèo). Nhưng: kịch bản lõi `ApplyGoldenScript` còn bắn xu/nhảy/xoay
song song (Phiên 0 gỡ); xu 1/3/14/26/45 ×1,5 khi có xu mồi (tới 67); kích thước xu cố định 0,045 m; không có phím
tương tác (xu mồi chỉ qua `InsertToken`/ContextMenu); Thường bị xử như Rác (xì hơi, mèo ngã).
**CẮT:**
1. `coinsPerTier = { 1, 3, 12, 22, 36 }`; sau nhân 1,5 vẫn `Mathf.Min(40, n)`.
2. `Clink`: khoảng cách `0.03f` → `ChestAudioRule.ImpactGap` (0,12 s), âm `Mathf.Min(0.4f, speed * 0.3f)`.
**BÙ:**
1. Phím **[H] kéo cần gạt** (Input System + Input cũ, giống mẫu `HaiTacSeqHook.FirePressed`): khi `_spinning`, tối đa
   3 lần, mỗi lần `StartCoroutine(PullLever(0.2f))`, `Play(leverClip, 0.6f, 1f + 0.1f*lần)`, `_tensionBonus += 0.1f`,
   rương giật `_hop = Max(_hop, 0.01f)`. Lần thứ 3 tự gọi `InsertToken()` (mèo ra sớm). Không dùng G/K/L (bàn thử).
2. Tách Thường khỏi "thua": `bool win = tier >= ChestTier.Thuong`; Thường: ô 3 dừng màu bậc, `jackpotClip` âm 0,5 pitch
   0,9, `_hop = 0.05f`, 3 xu, mèo vẫy tay. Rác giữ nguyên xịt (fizzle + mèo ngã).
3. Kích thước xu theo cỡ rương: `localScale = new Vector3(0.09f, 0.008f, 0.09f) * S` (S đo từ `ChestBounds`), lực nhân
   `Mathf.Sqrt(S / 0.5f)`.
**SỬA LỖI:** `SpinChest` dùng `root.Rotate` trực tiếp còn `LateUpdate` cộng/trừ vị trí — giữ, nhưng sau Reset phải trả
`localRotation` gốc: lưu `_baseRot` trong `Press()`, trả ở `Cleanup()`.
**ÂM:** Nền = `spinLoopClip` (đã có rè + tích tắc → không cần Nhịp). Chốt: `reelStopClip` ×2 trước đó là sự kiện
(cách 0,35 s, âm 0,5); TC = `fizzleClip` (Rác, sau khi `SlideLoopDown` → đổi thành `_loop.Stop()` ngay + 0,2 s im) /
`jackpotClip` (Thường+). HĐ: không (jackpot đã đủ). Đuôi: xu leng keng theo A5.
**Kiểm tra:** chỉ một bộ xu bắn ra; H kéo cần 3 lần; Rác: im phắc 0,2 s rồi kèn hụt; Thường không còn mèo ngã.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 1 (mèo) + máy xèng gắn rương |
| Xu Rác→HT | 1 / 3 / 12 / 22 / 36 (trần 40) |
| Tương tác | H kéo cần (≤ 3) + xu mồi vật lý |
| Lớp âm trước chốt | 1 |
| `tension.enabled` | false |

---

### 5.4 Magic — `MagicRuneRitual.cs`, `MagicRuneStone.cs`, `Editor/MagicRuneFxSetup.cs` (gần chuẩn, cắt nhẹ)

**Giữ:** 3 đá rune vật lý, wisp thắp đá, vòng trận co dần, rương bay xoay rồi dập/rơi.
**Đo hiện tại:** đá ở `r = max(0.55, 1.8 × nửa bề ngang)` ≈ 0,9 S (đạt); vòng 1,15r; đèn tím `range = r*4`; cột sáng HT
cao 6 m cố định (`localScale (0.02, 3, 0.02)` tại +3 m) → vượt T2; nền `build` dài 10 s; rương bị đặt `position`
tuyệt đối (không cộng dồn) → có thể đè rung của BoxOpenSequence.
**CẮT:**
1. Cột sáng HT: cao **2,5 S** (đặt tâm `_restPos + up*1.25*S`, scale `(0.04*S, 1.25*S, 0.04*S)`), sống 1,5 s.
2. `Puff` lúc dập: 80/45 → **60/35**.
3. Đèn: giữ 1 (`_light`), wisp KHÔNG có Light riêng (nếu `SpawnWisp` thêm Light thì bỏ; giữ quầng sáng bằng vật liệu
   additive). T5.
**BÙ:** không.
**SỬA LỖI:**
1. `UpdateChest`/`EndingChest` gán `chest.position = …` tuyệt đối → đổi sang cộng độ lệch kiểu `HaiTacSeqHook.LateUpdate`
   (gốc = giá trị BoxOpenSequence vừa ghi nếu khác giá trị mình ghi khung trước), trả về gốc khi Reset.
2. Nếu khoá muộn hơn độ dài `buildClip`: bật `loop = true` cho `_buildSrc` (clip 10 s) để không im giữa chừng.
**ÂM:** Nền = `buildClip` (có trống + gõ đá bên trong). Sự kiện: `stoneClip` 3 lần lúc đá rơi (0,4), `wispClip` 3 lần
(0,35). Chốt: `_buildSrc.Stop()` ngay; Rác: 0,2 s im rồi `endClips[0]`; khác: `endClips[rank]` (TC); HĐ = tiếng đá dội
trong `Shockwave()` chỉ khi RH/HT, âm 0,6.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 1 (wisp) + 3 đá trong 0,9 S |
| Rigidbody | 3 đá (+ đá người chơi ≤ 3) |
| Cột sáng HT | 2,5 S |
| Light | 1 |
| `tension.enabled` | false |

---

### 5.5 Mimic — `MimicFeedHook.cs`, `MimicFeedFood.cs`, `MimicFeedMouth.cs`, `Editor/MimicFeedSetup.cs` (LỐ: chờ quá lâu)

**Giữ:** rương đói phải được cho ăn, ném đồ vào miệng, nhai/nuốt/ợ, nhổ đồ lúc kết, bậc cao nhảy xoay hất văng.
**Đo hiện tại:** cổng chờ cho ăn 2 miếng, tự vồ sau 4,5 s, tối đa 14 s → tổng màn có thể > 20 s (vượt T6); con ruồi
bay + vòng `buzzLoopClip` (lớp âm thứ 3); `shoveRadius = 1.6f` mét cố định đẩy cả vật quanh (có thể đụng vật của cảnh);
riser vẫn phát cả khi lõi đã có tiếng dâng.
**CẮT:**
1. `bitesNeeded = 1`, `foodCount = 3`, `autoEatAfter = 2.0f`, `maxFeedWait = 5f`.
2. Bỏ hẳn con ruồi: không gọi `SpawnFly()`, bỏ `buzzLoopClip`/`_buzzSrc`, `SnapAtFly`. Thay bằng: mỗi bước màu rương
   "nhai" (`Kick`) như hiện có.
3. `Shove`: chỉ tác động Rigidbody có `MimicFeedFood` hoặc đồ đã bị nuốt (không đụng vật của cảnh – T9); bán kính
   `1.2f * _size`.
**BÙ:** không.
**SỬA LỖI:** ghi chú spec nói "Tiếng chốt/tick của profile ChestFx_Mimic vẫn phát chồng" → A1 xử lý.
**ÂM:** `MimicFeedSetup`: BỎ đoạn gán clip vào `prof.tension` và đặt `prof.tension.enabled = false` qua
`ApplyToProfile`. Khi đó hook tự chạy nhánh `!CoreTension`: Nền = `stomachLoopClip`, Nhịp = `heartbeatClip`.
`riserClip`: BỎ (nền + nhịp đã dâng). Chốt: Rác → `_loopSrc.Stop()` + dừng nhịp, 0,2 s im, `deflateClip` (TC), nhổ đồ
+0,75 s (sự kiện 0,4); Thường → `burpSmallClip` (TC 0,7); Hiếm+ → `boomClip` (HĐ) + `burpBigClip` dời +0,4 s (đuôi);
HT nổ lần 2 lúc đáp đất là đuôi (âm 0,4). `chompClip/chewClip/gulpClip/hopClip/giggleClip`: sự kiện qua `TryGate`.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 0 (rương là nhân vật) |
| Đồ ăn | 3, cần 1 miếng, tự vồ sau 2 s, tối đa chờ 5 s |
| Lớp âm trước chốt | 2 (bụng réo + tim) |
| Tổng màn | ≤ 11 s |
| `tension.enabled` | false |

---

### 5.6 Thợ Mỏ — `MinerDigSequence.cs`, `MinerSynth.cs`, `Editor/MinerDigSetup.cs` (HỎNG: kịch bản không tự chạy; quá dài)

**Giữ:** rương bị vùi trong vỏ đá Rigidbody, cuốc đập, thuốc nổ ngòi cháy, chim hoàng yến, kết theo bậc.
**Đo hiện tại:** `Begin()` chỉ chạy khi nơi bấm gọi `MinerDigSequence.TryBegin(...)` — **không nơi nào gọi**, nên trong
Unity chỉ thấy lõi (hoặc `MinerHook` cũ). Nếu chạy: đào 4–6 nhát × 0,85 s + ngòi 2,2 s + 0,35 s **trước khi**
`reveal.Play` mới bắt đầu → tổng ~15 s (vượt T6). 26 khối đá; 18 cục vàng; 14 sỏi; Light `range = S*4`.
**CẮT + SỬA (làm chạy khi bấm, dùng khung kịch bản của lõi):**
1. Hồ sơ Miner có `script` riêng `ChestScript_Miner` (tạo trong `MinerDigSetup`/`MinerFxSetup`):
   bước 1 `Cue` tên `"dao_mo"`, `start = OnPress`; bước 2 `OpenBox`, `start = AfterPrevious`.
   (Có bước OpenBox nên `Play()` không tự mở nắp — lõi đợi đào xong.)
2. `MinerDigSequence.OnCue(cue, ctx)`: `cue == "dao_mo"` → trả coroutine `DigRoutine(ctx)` = phần dựng + đào + nổ của
   `Begin()` hiện tại, NHƯNG: không gọi `_reveal.ResetReveal`, không gọi `_reveal.Play` trong `Unearthed()` (chỉ trả
   rương về tư thế và kết thúc coroutine), bậc lấy `ctx.Tier` chỉ dùng ở `OnTierLocked` như cũ.
3. Rút ngắn đào: `strikesMin = 2`, `strikesMax = 3`, `autoStrikeInterval = 0.55f`, `fuseSeconds = 1.0f`; tự cắm
   thuốc nổ ở nhát cuối. Tổng đoạn đào ≤ 3 s.
4. `chunkCount = 12`, `chunkScale = 0.38f`; vàng HT 18 → **10**; sỏi RH/HT 8/14 → **4/6**; mỗi bước màu `DropPebbles(1)`
   mỗi 3 bước → mỗi 4 bước, trần 6.
5. Giữ `TryBegin` làm API cũ nhưng đánh dấu `[System.Obsolete]` và cho nó chỉ gọi `reveal.Play` (kịch bản tự chạy).
**ÂM:** Nền = `_loop` (MinerSynth.Rumble). Nhịp = "thình" (`_nextThump`). Trước chốt `OnRouletteStep` hiện phát thêm
`Thump` mỗi bước màu → bỏ (đã có nhịp). Lúc chốt: Rác/Thường hiện cùng "xịt" → tách: Rác = `_loop.Stop()` + dừng nhịp,
0,2 s im, `Fizzle` (TC); Thường = fade 0,3 s + `Chirp` 0,6 (TC), chim KHÔNG ngất. Hiếm = `Chirp` (TC 0,85). RH/HT =
`Boom` (TC) + không thêm HĐ; tiếng vàng (`Nugget`) là đuôi qua `TryGate`.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 2 (cuốc, chim) + vỏ đá bám rương |
| Khối đá / vàng HT / sỏi | 12 / 10 / ≤ 6 |
| Đoạn đào trước mở nắp | ≤ 3 s |
| Lớp âm trước chốt | 2 (rền + thình) |
| `tension.enabled` | false |

---

### 5.7 Âm nhạc — `AmNhacBassHook.cs`, `AmNhacVinyl.cs`, `Editor/AmNhacBassSetup.cs` (ÂM HỖN LOẠN)

**Giữ:** rương là loa sub nén–bật theo kick, loa kẹo kéo lăn vào, thả đĩa than (V), bấm theo phách (B), DROP / rút phích.
**Đo hiện tại:** hình đạt (7 đồ trong 0,85–1,3 S, loa kẹo kéo, đĩa, 6 vòng sóng, 1 Light). Âm: kick + hat + clap + wub
(4 lớp nhịp) + feedback/wheels + âm căng lõi (CHƯA tắt) + tick lõi + lock lõi + accent lõi + riser → hỗn loạn.
`shakeSceneBodies = true` đẩy vật của cảnh trong 2,5 S (vi phạm T9). Sàn 12 S.
**CẮT:**
1. `shakeSceneBodies = false` mặc định, và xoá đường đặt true; `CollectSceneBodies` chỉ còn khi bật tay.
2. `propCount = 5`. Sàn `BuildGround` 12 S → 4 S.
3. Lớp nhịp: chỉ còn **kick (mọi phách) + hat (nửa phách)**; `clap` chỉ ở DROP; `wub` chỉ khi đĩa trên mâm VÀ thay hat
   (không cùng lúc hat). Tức `OnBeat`/đoạn nửa phách: `if (_vinylOn) Play(wub) else if (_beat >= 4) Play(hat)`.
4. `feedback` của loa kẹo kéo: 1 lần, âm 0,35. `wheels`: âm 0,3.
5. Đám đông (`crowd`) chỉ ở HT (bỏ ở RH); `horn` giữ HT nhưng dời +0,6 s.
**BÙ:** không.
**SỬA LỖI:** `Play(clip, volume, delay)` tạo `AudioSource` mới mỗi lần có delay mà không giới hạn → đủ dùng, nhưng
thêm trần 4 nguồn trễ cùng lúc.
**ÂM:** `AmNhacBassSetup` hồ sơ: `ApplyToProfile(p)` (thay các dòng `tickVolume = 0.18f`, `lockVolume = 0.55f`…). Nền =
kick+hat (đây là "nhịp", rương này không cần vòng nền riêng). Riser: giữ (là cú dâng cuối của nhạc) nhưng bắt đầu ≤ 1,2 s
trước chốt và âm 0,45. Chốt: Rác = `_beatOn = false` ngay, 0,2 s im, `recordStop` (TC); `deflate` dời +0,5 s âm 0,4;
`plugPop` là đuôi. Thường = `SmallDrop` giữ `scratch` (TC 0,6), bỏ `deflate` "pụp". Hiếm+ = `drop` (TC).

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 2 (loa kẹo kéo, đĩa than) + 5 đồ nhỏ |
| Lớp nhịp cùng lúc | 2 (kick + hat/wub) |
| Vật cảnh bị đẩy | 0 |
| `tension.enabled` | false |

---

### 5.8 Việt Nam — `VietNamHoiLangHook.cs`, `VietNamHoiLangFloater.cs`, `VietNamHoiLangProps.cs`, `Editor/VietNamHoiLangSetup.cs` (LỐ NHIỀU NHẤT)

**Giữ:** đêm hội: hoa đăng, trống – mõ – đàn bầu dâng dần, rương bập bềnh, Rác ếch nhảy lên nắp, bậc cao trống đồng
"BÙM" và đèn trời bay lên. **Theo ý chủ dự án: chỉ vài hoa đăng bay vào rương.**
**Đo hiện tại:** mặt nước `waterRadius = 3.2f` **mét cố định** (≈ 6–8 S); 12 đèn tự thả ở 0,65–0,9 × 3,2 m (2–2,9 m),
≤ 6 Light; rùa vàng (3 đèn người chơi); 1–3 chim Lạc thả bánh chưng; đom đóm; ếch → 4–5 diễn viên, 6 Light, vùng
hoạt động gấp 4 lần thước.
**CẮT:**
1. Bỏ mặt nước: không gọi `VietNamHoiLangProps.WaterDisc`, bỏ `Ripple`, bỏ `_waterY` làm mặt đỡ (giữ làm mốc độ cao).
2. Bỏ rùa: xoá `TurtleRoutine`, `offersForTurtle`, `turtleClip`.
3. Bỏ chim Lạc + bánh chưng: xoá `BirdRoutine`, `flapClip`, `VietNamHoiLangProps.Gift`.
4. Hoa đăng: `autoLanterns = 5`, người chơi thêm ≤ 2 (phím L giữ). Đèn KHÔNG còn nổi trên nước: tạo đèn kinematic
   (`Body.isKinematic = true`) ở vòng tròn bán kính **1,4 S**, cao ngang miệng rương, rồi **bay lượn vào miệng rương**
   theo đường cong (Bezier 3 điểm, đỉnh cao hơn miệng 0,4 S) trong 1,2 s; tới miệng thì thu nhỏ về 0 trong 0,2 s và
   tắt (giữ `GameObject` ẩn để dùng lại lúc kết). Mốc vào: đèn thứ i vào ở `(0.15 + 0.15 i) × T` (T = `_expectedLock`),
   đèn người chơi vào ngay sau 1,2 s. Mỗi đèn vào: rương chao nhẹ (`_tiltKick` hướng đèn tới), tiếng `plopClip` 0,35.
5. `maxLights = 1` (chỉ đèn đầu tiên có Light, T5). Đom đóm: chỉ phát quanh miệng (giữ), ≤ 3 hạt/khung.
6. Rương bập bềnh: giữ `_bobAmp`/`_tiltAmp` nhưng trần `_bobAmp ≤ 0.02 S`, `_tiltAmp ≤ 5°`; bỏ `HoiLangChestBump`
   (không còn đèn va).
**KẾT MỚI:**
- Rác: đàn bầu + trống dừng ngay, 0,2 s im, `deflateClip`; rương hụt xuống 0,15 H rồi lên; ếch (`FrogRoutine` giữ) nhảy
  từ 1,2 S lên nắp "ộp" +0,6 s.
- Thường: `moClip` lẻ (TC 0,6); 1 đèn trồi khỏi miệng, bay lên 1,5 S rồi tắt.
- Hiếm / RH / HT: `boomClip` (TC) + `drumClip` pitch 0,6 (HĐ 0,7); **1 / 2 / 3 đèn trời** trồi từ miệng rương, bay lên
  tới 2,5 S (dùng chế độ `Rise()` của floater nhưng kinematic, tốc độ cố định), mờ dần; rương nhảy
  `SetLift(H*(0.25+0.1*rank))`, HT lơ lửng 1,6 s (thay 2,4 s).
**ÂM:** Nền = `droneClip` (đàn bầu). Nhịp = trống/mõ hiện có. `plopClip` là sự kiện qua `TryGate`.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 1 (ếch, chỉ Rác) + hoa đăng (đồ bay vào rương) |
| Hoa đăng | 5 tự + ≤ 2 người chơi, xuất phát 1,4 S, đều vào miệng rương |
| Đèn trời lúc kết | Thường 1 · Hiếm 1 · RH 2 · HT 3, cao ≤ 2,5 S |
| Light | 1 (`maxLights = 1`: chỉ đèn đầu tiên) |
| Mặt nước / rùa / chim | bỏ |
| `tension.enabled` | false |


---

### 5.9 Tuổi thơ — `TuoiThoHook.cs`, `TuoiThoPiece.cs`, `Editor/TuoiThoFxSetup.cs` (LỐ NHIỀU)

**Giữ:** cỗ máy phản ứng dây chuyền bằng đồ chơi tuổi thơ, bi ve thật, cú đập làm cả rương nảy, máy bay giấy lộ bậc,
thả thêm bi (chuột/B).
**Đo hiện tại:** sân chơi trải từ x = −2,35 S tới +0,62 S (tờ giấy 2,7 × 1,5 S, thước dốc từ −2,35 S, hộp bút, 2 bút,
6 domino, xe, con quay, ná) → khoảng 8 đồ vật, vùng gần 2,5 S về bên trái: "dựng cả một khu vực".
**CẮT (chủ dự án: vài tác nhân là đủ):** chuỗi mới **3 khâu**: bi lăn dốc → 3 domino → ná bắn bi lên nắp.
1. `BuildPlayground`: xoá tờ giấy `ToGiay`, `HopBut`, `ButChi`, `ButBi`, xe `XeDoChoi` (+4 bánh), con quay `ConQuay`.
2. Thước dốc ngắn lại và dời gần: `a = P(-1.15f, 0.45f, 0.55f)`, `b2 = P(-0.75f, 0.03f, 0.55f)`; đỡ đầu dốc bằng một
   khối nhỏ (dùng lại màu hộp bút) cao 0,45 S.
3. `dominoCount = 3`, đặt x = −0,6 / −0,45 / −0,3 S (z = 0,55); domino cuối đổ vào **chốt ná** (bỏ khâu xe + con quay):
   khi domino cuối ngã (`_dominoDown[last]`) hoặc tới `_releaseAt` thì thả ná.
4. Ná giữ nguyên vị trí (trước rương 0,95 S). Vùng mới: x ∈ [−1,2 S; +0,2 S] → đạt T2.
5. Bỏ âm `carClip`, `topClip`, `riserClip` (còi trượt dâng gắn với con quay) — xem ÂM.
6. Bi thả thêm: `maxExtraMarbles = 4`; hoa giấy lúc kết `30 + 140·bậc + 15·combo` → `20 + 60·bậc01 + 10·combo`, trần 120.
7. Máy bay giấy: giữ; vòng lượn bán kính 0,9 S (đạt).
**SỬA LỖI:** các mốc "cú hích dự phòng" của xe/con quay xoá theo; `_releaseAt = max(0.8T, T − 0.6)` giữ.
**ÂM:** Hồ sơ `ApplyToProfile`. Nền: **không có vòng nền** hiện tại → dùng chuỗi mộc cầm `Beats()` làm Nhịp, và thêm
Nền = `rollClip` đặt `loop = true` âm 0,15 → 0,35 (tiếng lăn rì rì của cỗ máy) từ lúc bấm tới chốt. Sự kiện:
`flickClip`, `dominoClip` (mỗi quân, pitch lên 1 nửa cung, qua `TryGate` 0,12 s), `bandClip`, `bonkClip`. Chốt: Rác
= dừng nền + nhịp, 0,2 s im, `failClip` (TC); khác = `winClip` (TC), không HĐ. `planeClip` đuôi 0,35; `crumpleClip` +
`bonkClip` của Rác là đuôi (cách nhau 0,15 s).

| Bản chốt | Giá trị |
|---|---|
| Khâu cỗ máy | 3 (bi–dốc, 3 domino, ná) + máy bay giấy lúc kết |
| Vùng | trong 1,2 S bên trái + ná 0,95 S trước |
| Bi thả thêm | ≤ 4 |
| Lớp âm trước chốt | 2 (lăn + mộc cầm) |
| `tension.enabled` | false |

---

### 5.10 Robot — `RobotTransformHook.cs`, `RobotChargeSocket.cs`, `RobotChargePart.cs`, `Editor/RobotTransformSetup.cs` (hơi lố + âm 3 lớp)

**Giữ:** sạc pin qua khe lưng, drone thả pin, rương mọc chân đứng dậy dậm chân, mắt nháy, kết theo bậc, tay đỡ quà.
**Đo hiện tại:** drone (diễn viên 1) + đồ gắn rương (khe, thanh pin 5 ô, chân, tay, mắt, màn chữ). Âm: `humLoopClip` +
`droneLoopClip` + chuỗi `beepClip` + `servoClip` + `typeClip` (gõ chữ) + âm căng lõi + stage clips lõi (windup, steam).
Độ cao nhảy dùng mét cố định `{0, 0.08, 0.25, 0.45, 0.7}`.
**CẮT:**
1. Màn chữ chạy (`Type`, `UpdateTyping`, `typeClip`, `_scanLine`): bỏ phần tự kiểm hệ thống trước chốt; chỉ giữ **một
   dòng** lúc kết (`"> " + tên bậc + "!"`) hiện 2 s (T9).
2. `droneLoopClip`: bỏ vòng lặp; khi drone tới chỉ phát 1 tiếng lẻ (`servoClip` pitch 1,4, âm 0,3).
3. Ăn mừng: `confetti = lv >= 2 ? 3 + 3 * lv : 0` → `lv >= 2 ? 2 + 2 * lv : 0` (tối đa 10).
4. Bu-lông bung ở Rác: 4 → 3.
**SỬA LỖI:** độ cao nhảy theo cỡ rương: `heights = {0, 0.12f, 0.35f, 0.6f, 1.0f}` × H (thay mét).
**ÂM:** Nền = `humLoopClip`; Nhịp = `beepClip` (chỉ từ lúc đứng dậy, như hiện tại). `slotClip` khi pin vào: sự kiện.
Chốt: Rác = `_hum.Stop()` + dừng bíp, 0,2 s im, `fizzleClip` (TC); khác = `blastClips[lv-1]` (TC), không HĐ; Thường bỏ
`Play(fizzleClip, 0.35f, 1.8f)` (hắt hơi khói chỉ còn hình).

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 1 (drone) + đồ gắn rương |
| Vật rơi sạc / ăn mừng HT | 3 tự + người chơi ≤ 6 / ≤ 10 |
| Chữ | 1 dòng lúc kết |
| Lớp âm trước chốt | 2 (hum + bíp) |
| `tension.enabled` | false |

---

### 5.11 Không gian — `KhongGianMissionHook.cs`, `KhongGianSynth.cs`, `Editor/KhongGianFxSetup.cs` (LỐ: 2 hook + nhiều diễn viên)

**Giữ:** mất trọng lực quanh rương (mảnh vụn trôi), tàu mẹ tiếp nhiên liệu (B thả pin), tên lửa trúng nắp đúng lúc bật,
kết Rác máy xịt rớt bịch / bậc cao rương nhảy bằng động cơ.
**Đo hiện tại:** prefab mang **cả `KhongGianHook` cũ** (sao, vòng sonar mỗi bước màu, mưa sao băng, radio rè) **và**
`KhongGianMissionHook` (tàu, pin, tên lửa bay vòng bán kính **3,2 S**, thùng quà dù, đồng hồ nhiên liệu, 6–9 mảnh, 3
pháo hoa HT) → 4–5 diễn viên, vùng 3,2 S.
**CẮT:**
1. `KhongGianFxSetup.BuildPrefab`: KHÔNG `AddComponent<KhongGianHook>()`; khi chạy lại trên prefab cũ thì `DestroyImmediate`
   component `KhongGianHook` nếu có. Bỏ ghi `staticClip/meteorClip` cho hook cũ.
2. Bỏ thùng quà + dù: `DropCrate`, `ChuteBloom`, `ChuteFail`, `CrateLanding`, `_strings`, `_canopy` (món quà đã do bóng
   vật phẩm của lõi thể hiện). Rác thay "thùng rơi đập đất" bằng: rương rớt bịch (giữ) + tàu ho khói lủi đi (giữ).
3. Bỏ đồng hồ nhiên liệu (`gauge` trong `BuildCommon`, `_fuelFill`); thay bằng: miệng rương loé xanh mỗi pin vào (đã có).
4. Bỏ `Fireworks` HT; thay bằng tàu bay vòng chào 1 vòng quanh rương bán kính 1,2 S rồi lên (giữ `ShipVictory` nhưng
   `orbit` bán kính `0.8f + t` → `1.2f`, lên cao tối đa 3 S).
5. Tên lửa: bán kính vòng `3.2f` → **1.4f** (`SteerRocket`).
6. Mảnh trôi `SpawnFloaters(6 + _rng.Next(4))` → `SpawnFloaters(4)`. Pin tự `autoCells = 2`, người chơi `maxPlayerCells = 3`.
7. Tàu bay vào: điểm xuất phát `_camRight * 6f * S` → `3f * S`; lơ lửng ở `2.1 S` → `1.8 S`.
**ÂM:** Nền = `_engine` (máy nổ tổng hợp). Không có lớp Nhịp riêng (máy có nhịp pít-tông). Bíp radio: sự kiện. Chốt:
Rác = `_engine.Stop()` ngay, 0,2 s im, tiếng "xìiiì" trượt (TC), 5 "pụp" → 2 "pụp" (đuôi); khác = hợp âm rải (TC) +
`Impact` đã nổ trước đó nên không HĐ.

| Bản chốt | Giá trị |
|---|---|
| Hook | 1 (`KhongGianMissionHook`) |
| Diễn viên | 2 (tàu mẹ, tên lửa) |
| Mảnh trôi / pin | 4 / 2 tự + ≤ 3 người chơi |
| Vùng | tên lửa 1,4 S, tàu vào từ 3 S |
| `tension.enabled` | false |

---

### 5.12 Game — `GameVoxelHook.cs`, `Editor/GameVoxelFxSetup.cs` (LỐ: phủ màn hình + quá nhiều voxel)

**Giữ:** khối dấu hỏi 8-bit, nhân vật pixel húc đầu, rương vỡ voxel rồi tự lắp lại trong lúc quay màu, Z thả xu / X nấm.
**Đo hiện tại:** CRT phủ màn hình (`showCrt = true`, vạch quét, chớp trắng, chữ arcade) → vi phạm T9; voxel trần
`maxVoxels = 220` nhưng vòng tạo cho tới `maxVoxels * 2` = 440 Rigidbody; đếm 3-2-1 + "GAME START" mất 1,3 s; húc tối
đa 6; xu `(bậc²+1)×(1+power)` tối đa 60; pháo voxel HT.
**CẮT:**
1. `showCrt = false` mặc định và bỏ phủ màn hình hoàn toàn; chữ arcade thay bằng **1 dòng TextMesh nổi trên rương**
   (giống nhãn vẹt Hải tặc) cho các chữ "GAME OVER / NICE! / 1-UP!! / HIGH SCORE!!!", hiện 2 s.
2. Voxel: `maxVoxels = 80`, điều kiện `_voxels.Count >= maxVoxels * 2` → `>= maxVoxels`.
3. Bỏ đếm 3-2-1 (`countBeepClip`, `countGoClip`): `insertCoinClip` lúc bấm rồi nhạc vòng vào ngay ở 0,3 s.
4. Húc đầu tối đa 3; xu cuối = `Mathf.Min(24, (bậc² + 1) × (1 + power))`; pháo voxel HT ≤ 24 khối.
**BÙ:** không.
**ÂM:** Hồ sơ `ChestFx_GameVoxel`: `ApplyToProfile` (đã gần đúng; thêm tắt `whooshClip`). Nền = `loop_bar` (nhạc vòng).
Nhịp: không riêng (`voxel_tick` mỗi bước màu là sự kiện qua `TryGate`, âm 0,35). Chốt: Rác = nhạc dừng ngay (thay "tụt
băng" chậm), 0,2 s im, `end_0_rac` (TC); khác `end_<bậc>` (TC). `bump/coin/jump`: sự kiện; xu cuối là đuôi.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 1 (nhân vật pixel) + dấu "?" gắn trên rương |
| Voxel Rigidbody | ≤ 80 (tồn tại < 2 s trước khi bị hút về, không tính vào T3 sau chốt) |
| Xu HT | ≤ 24 |
| Phủ màn hình | không |
| `tension.enabled` | false |

(Ngoại lệ T3: voxel là chính thân rương vỡ ra và được hút về, cho phép 80 nhưng chỉ trong pha quay màu.)

---

### 5.13 Thể thao — `TheThaoHook.cs`, `TheThaoBall.cs`, `TheThaoFxPreset.cs`, `Editor/TheThaoFxSetup.cs` (LỐ: quá nhiều đạo cụ)

**Giữ:** rương là khung thành bị đập, bóng vật lý có xoáy cong (Magnus), cú sút quyết định lúc chốt, B thả bóng / K sút.
**Đo hiện tại:** khung thành, trọng tài, 3 bảng (thay người, bù giờ, VAR), bóng ở chấm 2,6 S, ≤ 10 bóng thêm, mưa 8 bóng
HT, VAR chiếu lại bóng ma; âm: đám đông lặp + trống + riser + còi + "ồ"… Sàn 14 S.
**CẮT:**
1. Bỏ trọng tài (`Referee` trong `BuildPitch`, `RefereeWalkIn`, `RefereeWave`).
2. Bỏ 3 bảng: `Substitution` không dựng `_subBoard` (giữ bóng 2 rơi + sút dội xà); `Match` không dựng `_addedBoard`;
   bỏ hẳn `Var()` (RH/HT kết bằng siêu phẩm cong + `airHornClip`).
3. `spotDistanceFactor = 1.6f` (chấm phạt đền gần lại). `maxExtraBalls = 4`; `BallRain(8)` HT → `BallRain(4)`.
4. Sàn `_size * 14f` → `_size * 4f`.
5. Đám đông lặp (`crowdLoopClip`): bỏ vòng lặp; chỉ còn tiếng lẻ `oohClip` (cú dội cột) và `roarClip`/`awwClip` lúc kết.
**ÂM:** `TheThaoFxPreset.Apply` đã tắt tension; thêm `ApplyToProfile`. Nền: không có vòng (đám đông bỏ) → Nhịp = trống
`Drums()` (giữ), Nền = không. Riser (`riserClip`): giữ, bắt đầu ≤ 1,2 s trước chốt, âm 0,4. "Lặng như tờ" trước cú sút
giữ (rất hợp quy tắc). Chốt: Rác = 0,2 s im (đã lặng) + `deflateClip` (TC), `tromboneClip`/`whistleEndClip` thành đuôi
(+0,6 s, +1,2 s, âm 0,35); Thường `clapClip` (TC 0,6); Hiếm `netClip` (HĐ) + `roarClip` (TC); RH/HT `netClip` + `roarClip`,
`airHornClip` đuôi +0,6 s.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 1 (khung thành bao rương) + bóng |
| Bóng | 2 kịch bản + ≤ 4 người chơi + HT mưa 4 |
| Bảng/VAR/trọng tài | bỏ |
| Lớp âm trước chốt | 1 (trống) |
| `tension.enabled` | false |

---

### 5.14 Trung Hoa — `TrungHoaHook.cs`, `TrungHoaFirecrackerString.cs`, `TrungHoaSky.cs`, `TrungHoaLion.cs`, `TrungHoaChestMotion.cs`, `TrungHoaPlayerItem.cs`, `Editor/TrungHoaFxSetup.cs` (LỐ: bay quá cao, quá nhiều)

**Giữ:** dây pháo quấn chân rương, sư tử châm ngòi, mỗi tích màu = 1 quả nổ hất rương, pháo đại cuối quyết kết, nén
nhang người chơi.
**Đo hiện tại:** 22 quả pháo (mỗi quả thành Rigidbody khi nổ); thăng thiên bay tới `S × lerp(2.2, 4.5)` và quả 福
`S × 5`; đèn lồng 1/3/5/9 rơi từ `S × (4–5,5)`; quả thưởng `BonusRocket` bán kính 2,5 m; sàn 12 S.
**CẮT:**
1. `crackerCount = 12`.
2. Thăng thiên: số `rockets` = Thường 0, Hiếm 1, RH 1, HT 1 (chính là quả 福); độ cao `S × lerp(1.8, 2.8)`; quả 福 cao
   `2.8 S`. Bỏ quả 福 riêng thêm sau (HT chỉ 1 quả và nó nở 福).
3. Đèn lồng: Thường 0 / Hiếm 1 / RH 2 / HT 3, rơi từ `2.5 S`, trong bán kính 0,8 S.
4. `BonusRocket`: bán kính nhận `bonusRocketRadius = 1.2f * S` (đổi sang theo cỡ rương), tối đa 1 quả thưởng.
5. Sàn `_size * 12f` → `_size * 4f`.
**ÂM:** Nền = `drumLoop` (trống lân), Nhịp = pháo lẻ mỗi tích (đã là nhịp tự nhiên của vòng màu) — giữ, qua
`TryGate(EventGap)` và âm ≤ 0,5. `fuseHissLoop`: là lớp thứ 3 → bỏ vòng lặp, chỉ phát `matchStrike` lúc châm. Chốt:
Rác = `_drum.Stop()` ngay, 0,2 s im, `fizzle` (TC), `slideDown` +0,5 s đuôi, `dudThud` đuôi, `sneeze` đuôi; khác =
`bigBang` (TC) + `cymbal` chỉ RH/HT (HĐ 0,6); `rocketWhistle`/`rocketBoom` là đuôi (≤ 2 tiếng).

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 1 (sư tử) + dây pháo quấn rương |
| Pháo | 12 |
| Thăng thiên / đèn lồng HT | 1 / 3, cao ≤ 2,8 S |
| Lớp âm trước chốt | 2 (trống + pháo lẻ) |
| `tension.enabled` | false |

---

### 5.15 Nhật Bản — `NhatBanHook.cs`, `NhatBanProps.cs`, `NhatBanFxPreset.cs`, `Editor/NhatBanFxSetup.cs` (gần chuẩn; bỏ làm chậm toàn cục)

**Giữ:** samurai vô hình (kiếm + nón), "chém xong mới đứt", tim taiko dồn dập, kết theo số nhát, K ném quả hồng.
**Đo hiện tại:** diễn viên gọn (kiếm, nón, cạnh rương 1,6 bề ngang). Vi phạm: `globalSlowMo = true` đổi
`Time.timeScale = 0.25` (T9, ảnh hưởng cả game). Lá anh đào lơ lửng — chưa đếm, giữ ≤ 40 hạt.
**CẮT:**
1. `globalSlowMo = false` mặc định và xoá đường đặt `Time.timeScale` (hàm quanh dòng 700–725); thay cảm giác chậm bằng:
   kiếm vung chậm trong 0,35 s rồi "tách" (chỉ hoạt hình của kiếm).
2. HT 6 nhát → **5** nhát; mỗi khí của người chơi thêm 1 nhát, trần thêm +2 (thay +3).
**ÂM:** `NhatBanFxPreset.Apply`: `p.tension.enabled = false` (thay `true`); xoá các dòng `p.tension.*` khác.
`NhatBanFxSetup`: bỏ gán `p.tension.loopClip/pulseClip` và gán `lockClipsByTier`; gọi `ApplyToProfile(p)`; thêm trường
`public AudioClip[] stingClips = new AudioClip[5]` trên `NhatBanHook`, Setup điền `sfx_nhatban_iai_fin_*` (các file
đang nằm trong `NhatBanFxPreset.LockFiles`). Khi tắt lõi, hook tự phát (nhánh `!CoreTension`): Nền = sáo `drone`,
Nhịp = tim `heartbeat` — đúng A2. Chốt trong `OnTierLocked`: dừng tim + sáo ngay; Rác: đợi 0,2 s rồi `stingClips[0]`;
bậc khác: `stingClips[rank]` ngay (TC). Không thêm HĐ (đoạn `fin_*` đã có vút/taiko/gong).

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 2 (kiếm, nón) |
| Nhát HT | 5 (+ ≤ 2 khí) |
| `Time.timeScale` | không đụng |
| Lớp âm trước chốt | 2 (sáo + tim) |
| `tension.enabled` | false |

---

### 5.16 Cổ Phong — `CoPhongNguKiemHook.cs`, `Editor/CoPhongNguKiemSetup.cs`, `Editor/CoPhongFxSetup.cs` (hơi lố)

**Giữ:** vòng phi kiếm quỹ đạo phá bùa phong ấn, rương bay lơ lửng trên mây, linh thạch (G / chuột trái).
**Đo hiện tại:** 7 kiếm + 6 bùa + dải rồng khí + hạc giấy bay ngang (sự kiện phụ) + mây 30 hạt sẵn + linh thạch ≤ 6;
âm `hum_loop` + `strike` mỗi cú đâm + `crane` + `stone` + core (tension, tick, ambient, xé niêm…).
**CẮT:**
1. Bỏ hạc giấy: xoá `UpdateCrane`, `BuildCrane`, `_craneAt`…, `crane` clip (linh thạch chỉ do người chơi thả).
2. Bỏ dải rồng khí (`BuildDragon`, `UpdateDragon`) — kiếm quỹ đạo đã là điểm nhấn; ở kết HT thay "rồng gầm" bằng chiêng
   có sẵn trong `end_huyenThoai`.
3. `swordCount = 5`, `talismanCount = 4`, `maxStones = 4`. Mây `_mist.Emit` 30 → 15.
4. Bán kính quỹ đạo kiếm ≤ 1,2 S (kiểm trong `UpdateSwords`).
**ÂM:** Nền = `hum_loop`; Nhịp = `strike` mỗi cú đâm (khoảng 1,4 → 0,55 s: đạt). Chốt: Rác = `_hum` dừng ngay (thay
"xì hơi tụt" kéo dài), 0,2 s im, `end_rac` (TC); `swordLand` lạch cạch thành đuôi qua `TryGate`; khác `end_<bậc>` (TC).
Hồ sơ: `ApplyToProfile`.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 0 ngoài kiếm/bùa bám rương |
| Kiếm / bùa / linh thạch | 5 / 4 / ≤ 4 |
| Lớp âm trước chốt | 2 (hum + strike) |
| `tension.enabled` | false |

---

### 5.17 Thiên Cơ — `ThienCoHook.cs`, `ThienCoRig.cs`, `ThienCoFxPreset.cs`, `Editor/ThienCoFxSetup.cs` (gần chuẩn; cắt đồng hồ cát)

**Giữ:** rương là bộ máy đồng hồ bị lên dây, đoàn bánh răng thiếu một bánh (G/chuột lắp), chim cu, lò xo xả.
**Đo hiện tại:** gọn (bánh răng trên mặt trước, con lắc bên trái, đồng hồ cát bên phải, chim cu trên nóc). Lố: đồng hồ
cát với **48 hạt Rigidbody 1,6 cm** (vật lý không ổn), vòng `sandLoop` (lớp âm thứ 3); chuông điểm 1+2+3 tiếng + lên dây
lần 2 → nhiều sự kiện.
**CẮT:**
1. Bỏ đồng hồ cát: phần `// --- Đồng hồ cát` trong `Build()`, `UpdateHourglass`, `_grains`, `_glassWalls`, `sandLoop`,
   và nhánh kết dùng `_grains` (dòng ~671). Kết HT "mưa đồng" thay bằng 12 bánh răng nhỏ (dùng lại `MakeGear` cỡ 0,3)
   bắn từ miệng.
2. Chuông điểm: chỉ 2 mốc (u = 0,5 → 1 tiếng; u = 0,85 → 2 tiếng), bỏ `wind` lần 2.
**ÂM:** `ThienCoFxPreset`: bỏ chỉnh tick/ambient (A1 làm hết). Nền: không có vòng → Nhịp = tích–tắc (đã đúng); thêm Nền
nhẹ = không cần (tích-tắc dày là đủ; ngoại lệ A2 cho phép 1 lớp). Chốt: Rác = tích-tắc dừng, 0,2 s im, `sproing` (TC),
`winddown` +0,6 s đuôi, `cuckooSad` +1,3 s đuôi; khác = `bell`/`bellBig` theo bậc (TC), `launch` (HĐ) chỉ RH/HT,
`musicBoxRise` RH/HT → bắt đầu ngay lúc chốt thay chuông (TC), không cộng chuông.

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 1 (chim cu) + đồng hồ gắn rương |
| Rigidbody | bánh răng 3 + con lắc 1; HT +12 bánh răng nhỏ |
| Lớp âm trước chốt | 1 (tích-tắc) |
| `tension.enabled` | false |

## 6. Bảng tổng hợp

| Rương | Mức hiện tại | Việc chính | `tension.enabled` |
|---|---|---|---|
| Hải tặc | mẫu | sửa âm lượng nhân đôi, bỏ nổ trùng, âm lõi | false |
| Rác Quý | mẫu | `_baseRot`, sàn theo cỡ, âm chốt | false |
| Golden | đơn giản + trùng | gỡ kịch bản xu trùng, thêm phím H, Thường ≠ Rác | false |
| Magic | gần chuẩn | cột sáng 2,5 S, ghi vị trí cộng dồn | false |
| Mimic | lố (chờ lâu) | 1 miếng, bỏ ruồi, không đẩy vật cảnh | false |
| Thợ Mỏ | hỏng + dài | chạy bằng Cue + OpenBox, đào ≤ 3 s, 12 đá | false |
| Âm nhạc | âm hỗn loạn | 2 lớp nhịp, không đẩy vật cảnh | false |
| Việt Nam | **lố nhất** | bỏ nước/rùa/chim, 5 hoa đăng bay vào miệng | false |
| Tuổi thơ | **lố** | 3 khâu, vùng 1,2 S | false |
| Robot | hơi lố | bỏ màn chữ + drone lặp, nhảy theo H | false |
| Không gian | lố (2 hook) | gỡ hook cũ, bỏ dù/đồng hồ/pháo hoa | false |
| Game | lố (phủ màn hình) | bỏ CRT, voxel 80 | false |
| Thể thao | lố (đạo cụ) | bỏ trọng tài/bảng/VAR | false |
| Trung Hoa | lố (bay cao) | 12 pháo, 1 thăng thiên, đèn ≤ 3 | false |
| Nhật Bản | gần chuẩn | bỏ `timeScale`, HT 5 nhát | false |
| Cổ Phong | hơi lố | bỏ hạc + rồng, 5 kiếm 4 bùa | false |
| Thiên Cơ | gần chuẩn | bỏ đồng hồ cát | false |

## 7. Thứ tự làm và kiểm

1. **Phiên 0** (mục 4) – một phiên, làm trước.
2. 17 phiên rương (mục 5.1–5.17) chạy song song được: mỗi phiên chỉ sửa file của rương mình + spec của rương mình.
   KHÔNG sửa `CreateAllChestFxProfiles.cs`, `ChestFxPresets.cs`, `ChestAudioRule.cs`, lõi (`ChestRevealSequence`…),
   `Tools/CompileCheck/UnityStubAll.cs` (nếu thiếu stub: ghi vào báo cáo, không sửa chung).
3. Mỗi phiên: `bash Tools/CompileCheck/check_all.sh` + script riêng của rương (`check_<rương>*.sh`), commit, push
   nhánh `claude/trim-<ruong>`; không mở PR.
4. Kiểm trong Unity (chủ dự án): chạy *Create All Chest FX Profiles*, mở từng rương 5 bậc. Phiếu kiểm mỗi rương:
   (a) nghe ≤ 2 lớp trước chốt; (b) Rác im phắc rồi xịt; (c) bậc cao 2 tiếng chốt; (d) không vật nào đứng xa quá 1,5 lần
   rương; (e) Reset trả rương về chỗ, không còn vật thừa; (f) màu dừng đúng bậc, bóng to đủ.

## 8. Chưa kiểm chứng (trung thực)

- Kế hoạch dựa trên **đọc mã**, không chạy Unity, không nghe âm. Số đo (S, khoảng cách) lấy từ hằng số trong mã.
- Một số tên hàm nhỏ bên trong hook (ví dụ dòng chính xác của `Time.timeScale` ở Nhật Bản, phần tạo đồng hồ nhiên
  liệu ở Không gian) đã chỉ vị trí gần đúng; phiên thực hiện tìm theo tên trong mục.
- Quyết định tắt âm căng lõi ở cả 17 rương làm lớp `ChestTension` không còn được rương nào dùng; giữ mã lõi nguyên
  (rương mới sau này có thể bật lại).

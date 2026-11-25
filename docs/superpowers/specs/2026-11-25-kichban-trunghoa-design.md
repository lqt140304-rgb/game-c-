# Kịch bản mở rương Chest_TrungHoa — "Đêm Giao Thừa"

Ngày 2026-11-25. Code: `Assets/_Project/Scripts/ChestFx/Chests/TrungHoa*.cs`, `Editor/TrungHoaFxSetup.cs`,
âm ElevenLabs `Audio/Chests/trung_hoa/*.mp3` (danh mục `Tools/Audio/elevenlabs_chest_manifest.json`, mục `trung_hoa`), kiểm `Tools/CompileCheck/check_trung_hoa.sh`.

**Đã thu gọn theo kế hoạch 2026-11-25** (mục 5.14 của `2026-11-25-thu-gon-hieu-ung-ruong-ke-hoach.md`).

## Ý tưởng
Rương không tự mở — nó bị **một dây pháo Tết quấn xoắn quanh chân** "đánh thức". Một **đầu sư tử giấy** nhảy vào ngậm
nén nhang châm ngòi; tia lửa bò dọc dây, từng quả pháo nổ văng ra (Rigidbody thật), mỗi quả **hất cả rương nảy lên và nghiêng**.
Nhịp nổ chính là nhịp vòng quay màu: mỗi "tích" = một quả pháo, nên càng gần chốt càng dồn dập. Quả **pháo đại** cuối dây
được để dành tới đúng lúc chốt bậc và quyết định cái kết.

Điểm riêng so với các rương khác: dây pháo *có hình học thật* quanh rương (ngòi cháy ngắn dần), nhân vật phụ là sư tử
(có thể bị ngã vật lý), pháo thăng thiên có thân Rigidbody được đẩy bằng lực, và chữ **福** viết bằng hạt.

## Nhịp (giây, với BoxOpenSequence mặc định)
| t | Việc |
|---|---|
| 0 | BẤM MỞ: trống lân lặp (nền, vol 0,15 → 0,45, cao độ 0,9 → 1,35) vang **ngay** (hook dò `IsPlaying` trong Update, không chờ nắp). Dây pháo + sư tử xuất hiện, sư tử nhảy tới đầu ngòi |
| ~1,4 | Sư tử châm ngòi (hoặc người chơi thả nén nhang vào đầu ngòi). Tiếng quẹt (không còn vòng ngòi xèo xèo) |
| 1,4 → vòng quay | Tia lửa bò, pháo lẻ nổ: rương nảy nhẹ, sư tử giật mình nhảy lùi |
| vòng quay | Mỗi tích = 1 quả nổ (hết pháo nhỏ thì rương tự giật lạch cạch). Trống lên giọng 0,95→1,35, to dần; pháo nổ to dần (qua cổng 0,2 s/tiếng, ≤ 0,5), rương nảy cao dần |
| chốt | Pháo đại → kết theo bậc (bên dưới). Trước thời điểm này **mọi bậc diễn giống hệt** (giữ bí mật) |

## Kết theo bậc (âm + hình) — bản đã thu gọn
Dây pháo **12 quả**, sàn va chạm `4 S`. Tiếng chốt (TC) = `stingClips[bậc]` (`sfx_trunghoa_lock_*.mp3`), HĐ = `big_bang`.
- **Rác**: trống dừng ngay, lặng 0,2 s, 1 tiếng xịt (TC); không phát gì khác 0,6 s tiếp. Pháo đại tịt ngòi; +~1 s đuôi `dud`, thăng thiên *xịt* rơi bịch, rồi sư tử hắt xì và ngã ngửa (bật Rigidbody).
- **Thường**: nổ vừa (TC 0,7 + HĐ 0,5), trống fade 0,3 s, rương nảy nhẹ. Không thăng thiên, không đèn lồng.
- **Hiếm**: rương bật cao, 1 thăng thiên (cao `lerp(1,8; 2,8) S`), 1 đèn lồng (TC 0,85 + HĐ 0,7).
- **Rất hiếm**: 1 thăng thiên, 2 đèn lồng, sư tử múa vòng quanh rương (TC 1,0 + HĐ 0,8).
- **Huyền thoại**: rương phóng lên xoay tít (540°), 1 thăng thiên duy nhất nở **chữ 福** ở `2,8 S` (~145 hạt), 3 đèn lồng.
- Đèn lồng rơi từ `2,5 S` trong bán kính `0,8 S`. Quả thưởng của người chơi: nhận trong `1,2 S`, tối đa 1 quả.
- Âm: nền = trống lân; nhịp = pháo lẻ mỗi tích; đuôi (từ +0,4 s, ≤ 6 tiếng, ≤ 0,4): thăng thiên rít/nổ, hắt xì.
  Bỏ: vòng ngòi xèo xèo, huýt trượt, chập chõa, tiếng vỏ pháo rơi (không có trong danh mục ElevenLabs).

| Bản chốt | Giá trị |
|---|---|
| Diễn viên | 1 (sư tử) + dây pháo quấn rương |
| Pháo | 12 (vỏ tự dọn sau 3–4 s) |
| Thăng thiên / đèn lồng HT | 1 / 3, cao ≤ 2,8 S |
| Lớp âm trước chốt | 2 (trống + pháo lẻ) |
| `tension.enabled` | false (`ChestAudioRule.ApplyToProfile` mỗi lần chạy menu) |

## Vật lý
- Vỏ pháo: Cylinder + CapsuleCollider, khi nổ thêm Rigidbody + xung lực ra xa tâm + xoáy, rơi va chạm đất/khối rương.
- Thăng thiên: Rigidbody đẩy bằng `AddForce` = m·(g + a) tới độ cao nổ; xịt thì lực yếu + lắc.
- Đèn lồng: Rigidbody lực cản lớn (rơi chậm), va rương/đất.
- Cả rương: `TrungHoaChestMotion` — mô phỏng trọng lực + nảy (hệ số 0,35) cho trục đứng, lò xo-giảm chấn cho nghiêng; cộng lên trên tư thế BoxOpenSequence (trừ ở Update, cộng ở LateUpdate) nên không tranh quyền. Không gắn Rigidbody vào chính rương.
- Hook tạo mặt đất BoxCollider + khối rương vô hình tạm thời; dọn hết khi reset.

## Tương tác phụ
- `TrungHoaPlayerItem.Incense` (nén nhang): đưa đầu nhang gần đầu ngòi → châm ngay. Bật `requirePlayerLight` trên hook: hộp **dừng sau bước Rung** (stage 0 `waitForPlayer`) tới khi châm → đây là "mở khóa"; quá `playerLightTimeout` (8 s) sư tử tự châm.
- `TrungHoaPlayerItem.BonusRocket`: đặt/ném quanh rương trước lúc chốt → bay lên cùng màn cuối (chỉ trang trí).
- Menu: `Tools/Cho Do La/Spawn Trung Hoa Incense`, `... Bonus Rocket`.

## Bất biến
Bậc chỉ đọc ở `OnTierLocked`; không gọi gì đổi bậc. Ngẫu nhiên dùng `System.Random` riêng (hạt `decorationSeed`). Vòng màu 5 bậc và bóng vật phẩm do ChestRevealSequence giữ nguyên.

## Thử trong Unity
1. Âm `.mp3` ElevenLabs đã có trong `Audio/Chests/trung_hoa/` (tạo bằng `Tools/Audio/make_elevenlabs_chest_sfx.py --chest trung_hoa`).
2. `Tools/Cho Do La/Create All Chest FX Profiles` (hoặc chọn rương → `Add Trung Hoa Hook To Selected` để gán âm).
3. Play, cầm Chest_TrungHoa, phím P/T như `Docs/HuongDanThuUnity.md`; thử từng bậc. Thử thêm: spawn nén nhang, kéo tới đầu ngòi.

## Chưa làm / chưa kiểm (trung thực)
- **Chưa chạy trong Unity, chưa nghe âm (âm ElevenLabs chưa ai nghe).** Chỉ biên dịch thử bằng stub (`check_trung_hoa.sh`, `check_all.sh` OK).
- Lực/độ nảy/độ cao chỉnh theo ước lượng, cần tinh chỉnh khi xem thật; chữ 福 bằng hạt có thể cần chỉnh khoảng cách/vận tốc.
- Khối rương vô hình đứng yên, không nảy theo rương.
- Nhánh lõi `claude/chest-fx-core` lúc làm chưa tồn tại trên origin → chưa nối API khung; hook đang dùng API `ChestFxHook` hiện có. Khi khung có sự kiện "bấm mở" chính thức, thay phần dò `IsPlaying` trong `TrungHoaHook.Update` bằng sự kiện đó.
- Sửa file dùng chung duy nhất: 1 dòng trong `Editor/CreateAllChestFxProfiles.cs` để prefab hook được gán âm.

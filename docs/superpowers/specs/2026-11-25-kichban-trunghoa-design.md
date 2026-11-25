# Kịch bản mở rương Chest_TrungHoa — "Đêm Giao Thừa"

Ngày 2026-11-25. Code: `Assets/_Project/Scripts/ChestFx/Chests/TrungHoa*.cs`, `Editor/TrungHoaFxSetup.cs`,
âm `Audio/Chests/trung_hoa/fx/` (tạo bằng `Tools/Audio/make_trunghoa_fx.py`), kiểm `Tools/CompileCheck/check_trung_hoa.sh`.

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
| 0 | BẤM MỞ: trống lân lặp vang **ngay** (hook dò `IsPlaying` trong Update, không chờ nắp). Dây pháo + sư tử xuất hiện, sư tử nhảy tới đầu ngòi |
| ~1,4 | Sư tử châm ngòi (hoặc người chơi thả nén nhang vào đầu ngòi). Tiếng quẹt + ngòi xèo xèo |
| 1,4 → vòng quay | Tia lửa bò, pháo lẻ nổ: rương nảy nhẹ, sư tử giật mình nhảy lùi |
| vòng quay | Mỗi tích = 1 quả nổ (hết pháo nhỏ thì rương tự giật lạch cạch). Trống lên giọng 0,95→1,35, to dần; pháo nổ to dần, rương nảy cao dần |
| chốt | Pháo đại → kết theo bậc (bên dưới). Trước thời điểm này **mọi bậc diễn giống hệt** (giữ bí mật) |

## Kết theo bậc (âm + hình)
- **Rác**: pháo đại *tịt ngòi* (xììì… pụt), trống tắt phụt, huýt trượt xuống; thăng thiên *xịt* nhích lên lảo đảo rơi bịch; sư tử hắt xì rồi ngã ngửa (bật Rigidbody).
- **Thường**: nổ vừa, rương nảy nhẹ, 1 thăng thiên nhỏ, 1 đèn lồng; trống nhỏ dần.
- **Hiếm**: rương bật cao, 1 thăng thiên nở hoa màu bậc, 3 đèn lồng.
- **Rất hiếm**: 2 thăng thiên + chập chõa, 5 đèn lồng, sư tử múa vòng quanh rương.
- **Huyền thoại**: rương phóng lên xoay tít (540°/s), 3 thăng thiên, rồi một quả nở **chữ 福**, 9 đèn lồng rơi đập vào rương.

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
1. `python3 Tools/Audio/make_trunghoa_fx.py` (đã chạy, file đã commit).
2. `Tools/Cho Do La/Create All Chest FX Profiles` (hoặc chọn rương → `Add Trung Hoa Hook To Selected` để gán âm).
3. Play, cầm Chest_TrungHoa, phím P/T như `Docs/HuongDanThuUnity.md`; thử từng bậc. Thử thêm: spawn nén nhang, kéo tới đầu ngòi.

## Chưa làm / chưa kiểm (trung thực)
- **Chưa chạy trong Unity, chưa nghe âm.** Chỉ biên dịch thử bằng stub (`check_trung_hoa.sh`, `check_all.sh` OK).
- Lực/độ nảy/độ cao chỉnh theo ước lượng, cần tinh chỉnh khi xem thật; chữ 福 bằng hạt có thể cần chỉnh khoảng cách/vận tốc.
- Khối rương vô hình đứng yên, không nảy theo rương.
- Nhánh lõi `claude/chest-fx-core` lúc làm chưa tồn tại trên origin → chưa nối API khung; hook đang dùng API `ChestFxHook` hiện có. Khi khung có sự kiện "bấm mở" chính thức, thay phần dò `IsPlaying` trong `TrungHoaHook.Update` bằng sự kiện đó.
- Sửa file dùng chung duy nhất: 1 dòng trong `Editor/CreateAllChestFxProfiles.cs` để prefab hook được gán âm.

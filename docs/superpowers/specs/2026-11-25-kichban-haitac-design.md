# Kịch bản mở rương Chest_HaiTac — "Trục vớt bằng đại bác"

Ngày: 2026-11-25 · Nhánh: `claude/chest-seq-hai-tac` · Thay cho `HaiTacHook` cũ (nắp + hạt + tiếng pháo xa).
**Đã thu gọn theo kế hoạch 2026-11-25** (mục 5.1; nhánh làm `claude/trim-hai-tac`): hình giữ nguyên, sửa lỗi nhỏ + chuẩn hoá âm.

## Ý tưởng
Bấm mở là **biển dâng lên nhấn chìm rương**. Một khẩu đại bác trên bè trôi vào, **con vẹt hải tặc** bay tới đậu trên nòng hô
"Nạp đạn!". Muốn trục vớt rương thì phải **bắn đại bác**: đạn là Rigidbody thật, va vào rương (va chạm PhysX) làm rương bật,
nghiêng, nổi dần; đạn trượt rơi xuống nước tạo sóng xô rương. Sóng định kỳ cũng đẩy rương và thùng. Khi vòng màu chạy, ngòi
**phát cuối** được châm — không ai biết nó nổ hay xịt cho tới lúc chốt bậc.

Đặc trưng riêng (không rương nào khác có): cả rương **chìm – trôi – bị bắn bật – bay/lộn – đáp đất** bằng mô phỏng khối lượng
lò xo (trọng lực, lực nổi, cản nước, xung lực), cộng nước dâng/rút, đại bác, vẹt gọi tên bậc.

## Tương tác phụ
- **[G]** nạp đạn & bắn (tối đa 6 phát, cách 0,5 s), trước khi ngòi phát cuối được châm. Cũng gọi được `HaiTacSeqHook.Fire()` từ UI.
- **[B]** thả thùng phao (tối đa 4): Rigidbody nổi theo lực đẩy, dòng nước kéo vào va rương. Bậc Hiếm+ thì thùng bị nổ tung; Rất hiếm/Huyền thoại mỗi thùng thêm 3 đồng xu rơi. `DropBarrel()`.
- Không bấm gì: vẹt tự bắn ở giây 1,7 / 3,0 / 4,3 → luồng không bao giờ kẹt.
- Sự kiện phụ: vẹt bay vào, đậu, kêu khi đạn trúng, gọi bậc; Rác thì ngã ngửa; Huyền thoại bay hai vòng quanh rương; xong thì bay đi.

## Nhịp (giây từ lúc bấm, với rương có chìa khóa và BoxOpenSequence mặc định)
| t | Hình | Âm |
|---|---|---|
| 0,0 | nước dâng trong 1,1 s, rương bị nhấn chìm (−0,12 chiều cao) | **nền lặp ElevenLabs vào ngay (OnPress)**, âm lượng 0,15 → 0,45, cao độ +45% tới lúc chốt; là lớp âm kéo dài DUY NHẤT (trống đã nằm trong nền) |
| 0–0,9 | bè + đại bác trôi vào | lạch cạch nạp đạn |
| 1,2 | vẹt đậu: "Nạp đạn! [G] bắn [B] thả thùng" | quạc |
| 1,7 / 3,0 / 4,3 | phát bắn tự động (phát đầu luôn trượt, sau đó ~65% trúng; theo hạt giống) | nổ, té nước, cộp |
| ~5,9 | lõi bắt đầu vòng màu; rương ngoi lên; ngòi phát cuối xì tia lửa; vẹt "Châm ngòi!" | ngòi xì, cuộn trống cuối nền |
| ~7,4 | **chốt bậc** → kết cục | Rác: nền tắt phụt → lặng 0,2 s → 1 tiếng xịt; bậc cao: 1 tiếng chốt + 1 phát đại bác |
| ~8,2+ | nắp mở toang, bóng vật phẩm to dần (lõi) | |

## Kết cục theo bậc (chỉ đọc `RolledTier` trong `OnTierLocked`)
| Bậc | Đại bác | Rương | Nước | Âm |
|---|---|---|---|---|
| Rác | **xịt**, viên đạn lăn khỏi nòng rơi "tõm" | nhú lên 0,04, nổi **nghiêng lệch** (9°,15°) | rút chậm 1,9 s ọc ọc | nền **tắt phụt** → lặng 0,2 s → 1 tiếng chốt Rác (`lock_rac`); +0,8 s nước rút ọc ọc (0,4) và vẹt kêu 1 tiếng; ngã ngửa; viên đạn rơi "tõm" sau +0,8 s |
| Thường | 1 phát | hất 0,18 | rút 0,9 s | TC `lock_thuong` (0,7) + 1 phát đại bác (≤ 0,5); nền fade 0,3 s |
| Hiếm | 1 phát | hất 0,38, xoay 180° | rút 0,9 s, thùng bay | TC `lock_hiem` (0,85) + 1 phát đại bác (≤ 0,7) |
| Rất hiếm | loạt 2 | hất 0,7, **lộn 360°** | rút 0,45 s, 14 xu vật lý | TC `lock_ratHiem` (1,0, đã chứa loạt nổ) + 1 phát đại bác (≤ 0,8); các nổ sau chỉ khói/giật nòng |
| Huyền thoại | loạt 3 | hất 1,15, xoay 540°, cột nước phun | **biển rút cạn**, 30 xu (+3/thùng) mưa xuống | TC `lock_huyenThoai` (1,0) + 1 phát đại bác (≤ 0,8); vẹt "HUYỀN THOẠI!!!!" bay vòng, kêu tối đa 2 tiếng từ +0,8 s |

## Bất biến giữ nguyên
- Bậc do game quyết qua `ChestRevealSequence.Play(rolledTier, ...)`; hook không đổi nó, không đọc nó trước lúc chốt.
- Ngẫu nhiên (trúng/trượt, hướng sóng, xu) chỉ trang trí, hạt giống `decorationSeed` (0 = mỗi lần khác).
- Vòng màu 5 bậc và bóng vật phẩm to dần vẫn do lõi chạy, không bị đụng.

## Cách cài (tệp)
- `Scripts/ChestFx/Chests/HaiTacSeqHook.cs`, `HaiTacSeqParts.cs` — kịch bản; `Editor/HaiTacSeqSetup.cs` — hồ sơ `ChestFx_HaiTacSeq.asset` (bỏ âm chốt bậc + tiếng cười giai đoạn 0 của lõi, vì hook phát đoạn kết) và menu.
- Âm: `Audio/Chests/hai_tac/*.mp3` (ElevenLabs, theo `Tools/Audio/elevenlabs_chest_manifest.json`, mục `hai_tac`). Hook có `stingClips[5]` (TC = `lock_*`). Các `.wav` numpy ở `seq/` không còn được gán (file để nguyên).
- Sửa tối thiểu file dùng chung: `CreateAllChestFxProfiles.cs` (HaiTac dùng hồ sơ + hook mới), `Tools/CompileCheck/UnityStubAll.cs` (thêm `Collision`, `Rigidbody.position`, `Font.material`). `check_hai_tac.sh` nay gọi `check_hai_tac_seq.sh`.
- Kỹ thuật: âm bắt đầu khi bấm nhờ hook dò cạnh `reveal.IsPlaying` trong `Update` (lõi chưa có sự kiện "OnPlay"). Chuyển động cả rương cộng vào vị trí gốc trong `LateUpdate`; nếu `BoxOpenSequence` vừa ghi vị trí (rung) thì lấy đó làm gốc mới, nên không đánh nhau. Không đụng `velocityOverLifetime` của hạt (tránh lỗi orbital khác chế độ).

## Số liệu sau khi thu gọn (kế hoạch 5.1)
| Hạng mục | Giá trị |
|---|---|
| Diễn viên | 2 (bè-đại bác, vẹt) + nước 1,75 S |
| Rigidbody sau chốt Rác→HT | 1 / 2 / 7 / 16 + thùng / tổng ≤ 40 (xu bị chặn để xu + thùng + hitbox ≤ 40) |
| Lớp âm trước chốt | 1 (nền `bed_loop`) |
| Tiếng lúc chốt (0,3 s đầu) | Rác 1 · khác 2 |
| `tension.enabled` | false (`ChestAudioRule.ApplyToProfile` mỗi lần chạy menu) |
| Sửa lỗi | hết nhân `volume` hai lần; loạt nổ thứ 2, 3 không phát tiếng; bỏ `fizzle`; vẹt ≤ 2 tiếng; bọt khí dùng `_rng`; `OnPress` bắt đầu ngay |
| Cổng âm (trong `Play`) | trước chốt: 1 tiếng/0,2 s, ≤ 0,5; đuôi: từ +0,4 s (Rác +0,75 s), cách 0,15 s, ≤ 6 tiếng, hết trước +3 s, ≤ 0,4; xu cách ≥ 0,12 s |

## Thử trong Unity
1. Theo `Docs/HuongDanThuUnity.md` đặt Chest_HaiTac vào cảnh, chạy *Tools/Cho Do La/Add Chest Reveal Test*.
2. Chọn rương → *Tools/Cho Do La/Add Hai Tac Seq (Cannon) To Selected Chest*. Hoặc *Create All Chest FX Profiles* cho menu cầm rương.
3. Play, Space để mở, T để ép bậc; trong lúc mở bấm G/B. Thử đủ 5 bậc.
4. Kiểm: âm vang ngay khi bấm; Rác nhạc tắt phụt; Huyền thoại xu rơi lên sàn ẩn; rương về đúng vị trí sau R.

## TRUNG THỰC: chưa làm được ở môi trường này
- **Chưa chạy trong Unity, chưa AI nghe âm** (cả 15 file ElevenLabs của rương này chưa nghe thử, chưa kiểm điều khoản giấy phép); độ dài nền lặp và độ vang `lock_*` so với phát đại bác chưa biết; chỉ biên dịch thử bằng stub (`check_all.sh`, `check_hai_tac_seq.sh` đều OK) — stub không bắt sai API thật.
- Cần xem: kích thước/vị trí đại bác, vẹt, nước (vật liệu additive, có thể quá tối/sáng); `TextMesh` lời vẹt (font LegacyRuntime có dấu tiếng Việt?); hitbox động học con của rương có báo va chạm ổn; khi cầm trên tay vật thể vật lý bay trong thế giới; rương bay lên khi bóng vật phẩm đã bật ra (bóng đứng yên, rương bật lên dưới nó); thời điểm chốt so với nền dâng 9 s (nền bị cắt lúc chốt nên lệch vài trăm ms vẫn ổn).
- Nền dâng theo thời gian cố định 7,4 s (không biết chính xác lúc chốt); nếu chốt sớm/muộn thì âm lượng/cao độ nền dừng ở mức cuối hoặc bị cắt.
- Trước chốt đạn người chơi tối đa 6 + 3 tự bắn + 4 thùng + hitbox có thể vượt 12 Rigidbody nếu bấm dồn; chưa thêm trần (kế hoạch bảo "không cắt hình").

## Nối với khung lõi
Nhánh `claude/chest-fx-core` **chưa tồn tại** trên origin (chỉ có `claude/chest-open-fx-core`, đã nằm trong nhánh này). Khi khung chung
có API kịch bản: (1) thay việc dò `IsPlaying` bằng sự kiện "bấm mở/OnPlay"; (2) nếu khung cho phép kéo dài/giữ giai đoạn, cho
phát cuối chờ người chơi bắn; (3) chuyển phần dịch chuyển rương sang kênh "biến đổi cả rương" của khung nếu có.

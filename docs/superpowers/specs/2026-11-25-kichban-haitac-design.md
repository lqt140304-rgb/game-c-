# Kịch bản mở rương Chest_HaiTac — "Trục vớt bằng đại bác"

Ngày: 2026-11-25 · Nhánh: `claude/chest-seq-hai-tac` · Thay cho `HaiTacHook` cũ (nắp + hạt + tiếng pháo xa).

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
| 0,0 | nước dâng trong 1,1 s, rương bị nhấn chìm (−0,12 chiều cao) | **nền dâng bắt đầu ngay**: sóng + trống trận 70→190 bpm + dây kéo lên quãng năm |
| 0–0,9 | bè + đại bác trôi vào | lạch cạch nạp đạn |
| 1,2 | vẹt đậu: "Nạp đạn! [G] bắn [B] thả thùng" | quạc |
| 1,7 / 3,0 / 4,3 | phát bắn tự động (phát đầu luôn trượt, sau đó ~65% trúng; theo hạt giống) | nổ, té nước, cộp |
| ~5,9 | lõi bắt đầu vòng màu; rương ngoi lên; ngòi phát cuối xì tia lửa; vẹt "Châm ngòi!" | ngòi xì, cuộn trống cuối nền |
| ~7,4 | **chốt bậc** → kết cục | nền bị cắt, đoạn kết theo bậc |
| ~8,2+ | nắp mở toang, bóng vật phẩm to dần (lõi) | |

## Kết cục theo bậc (chỉ đọc `RolledTier` trong `OnTierLocked`)
| Bậc | Đại bác | Rương | Nước | Âm |
|---|---|---|---|---|
| Rác | **xịt**, viên đạn lăn khỏi nòng rơi "tõm" | nhú lên 0,04, nổi **nghiêng lệch** (9°,15°) | rút chậm 1,9 s ọc ọc | nền **tắt phụt** → "pfft" + kèn buồn 4 nốt đi xuống; vẹt "rác... rác..." giọng trầm, ngã ngửa |
| Thường | 1 phát | hất 0,18 | rút 0,9 s | nổ + hợp âm trưởng 3 nốt |
| Hiếm | 1 phát | hất 0,38, xoay 180° | rút 0,9 s, thùng bay | nổ + hợp âm sáng hơn 4 nốt |
| Rất hiếm | loạt 2 | hất 0,7, **lộn 360°** | rút 0,45 s, 14 xu vật lý | 2 nổ + kèn + chũm chọe |
| Huyền thoại | loạt 3 | hất 1,15, xoay 540°, cột nước phun | **biển rút cạn**, 30 xu (+3/thùng) mưa xuống | 3 nổ + kèn đồng đầy + chũm chọe dài; vẹt "HUYỀN THOẠI!!!!" bay vòng |

## Bất biến giữ nguyên
- Bậc do game quyết qua `ChestRevealSequence.Play(rolledTier, ...)`; hook không đổi nó, không đọc nó trước lúc chốt.
- Ngẫu nhiên (trúng/trượt, hướng sóng, xu) chỉ trang trí, hạt giống `decorationSeed` (0 = mỗi lần khác).
- Vòng màu 5 bậc và bóng vật phẩm to dần vẫn do lõi chạy, không bị đụng.

## Cách cài (tệp)
- `Scripts/ChestFx/Chests/HaiTacSeqHook.cs`, `HaiTacSeqParts.cs` — kịch bản; `Editor/HaiTacSeqSetup.cs` — hồ sơ `ChestFx_HaiTacSeq.asset` (bỏ âm chốt bậc + tiếng cười giai đoạn 0 của lõi, vì hook phát đoạn kết) và menu.
- Âm: `Audio/Chests/hai_tac/seq/*.wav`, tạo bằng `python3 Tools/Audio/make_haitac_seq_sfx.py` (numpy, không mẫu).
- Sửa tối thiểu file dùng chung: `CreateAllChestFxProfiles.cs` (HaiTac dùng hồ sơ + hook mới), `Tools/CompileCheck/UnityStubAll.cs` (thêm `Collision`, `Rigidbody.position`, `Font.material`). `check_hai_tac.sh` nay gọi `check_hai_tac_seq.sh`.
- Kỹ thuật: âm bắt đầu khi bấm nhờ hook dò cạnh `reveal.IsPlaying` trong `Update` (lõi chưa có sự kiện "OnPlay"). Chuyển động cả rương cộng vào vị trí gốc trong `LateUpdate`; nếu `BoxOpenSequence` vừa ghi vị trí (rung) thì lấy đó làm gốc mới, nên không đánh nhau. Không đụng `velocityOverLifetime` của hạt (tránh lỗi orbital khác chế độ).

## Thử trong Unity
1. Theo `Docs/HuongDanThuUnity.md` đặt Chest_HaiTac vào cảnh, chạy *Tools/Cho Do La/Add Chest Reveal Test*.
2. Chọn rương → *Tools/Cho Do La/Add Hai Tac Seq (Cannon) To Selected Chest*. Hoặc *Create All Chest FX Profiles* cho menu cầm rương.
3. Play, Space để mở, T để ép bậc; trong lúc mở bấm G/B. Thử đủ 5 bậc.
4. Kiểm: âm vang ngay khi bấm; Rác nhạc tắt phụt; Huyền thoại xu rơi lên sàn ẩn; rương về đúng vị trí sau R.

## TRUNG THỰC: chưa làm được ở môi trường này
- **Chưa chạy trong Unity, chưa nghe âm** trong game; chỉ biên dịch thử bằng stub (`check_all.sh`, `check_hai_tac_seq.sh` đều OK) — stub không bắt sai API thật.
- Cần xem: kích thước/vị trí đại bác, vẹt, nước (vật liệu additive, có thể quá tối/sáng); `TextMesh` lời vẹt (font LegacyRuntime có dấu tiếng Việt?); hitbox động học con của rương có báo va chạm ổn; khi cầm trên tay vật thể vật lý bay trong thế giới; rương bay lên khi bóng vật phẩm đã bật ra (bóng đứng yên, rương bật lên dưới nó); thời điểm chốt so với nền dâng 9 s (nền bị cắt lúc chốt nên lệch vài trăm ms vẫn ổn).
- Âm là tổng hợp tạm (sine/nhiễu), nên thay bằng bản thu/ElevenLabs cùng tên file nếu chủ dự án muốn.

## Nối với khung lõi
Nhánh `claude/chest-fx-core` **chưa tồn tại** trên origin (chỉ có `claude/chest-open-fx-core`, đã nằm trong nhánh này). Khi khung chung
có API kịch bản: (1) thay việc dò `IsPlaying` bằng sự kiện "bấm mở/OnPlay"; (2) nếu khung cho phép kéo dài/giữ giai đoạn, cho
phát cuối chờ người chơi bắn; (3) chuyển phần dịch chuyển rương sang kênh "biến đổi cả rương" của khung nếu có.

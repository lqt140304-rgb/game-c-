# Thiết kế: hiệu ứng và âm rương Thợ Mỏ (Chest_Miner)

Trạng thái: **đề xuất, chưa chạy trong Unity, chưa nghe âm**. Giữ nguyên điểm chung đã chốt ở [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md) (màu nhảy rồi dừng đúng bậc; bóng to dần tới hình hoàn chỉnh) — lõi lo phần đó, file này chỉ là bản sắc riêng.

## Ý tưởng

Rương gỗ cũ của thợ mỏ, cuốc chim chéo: mở rương như **đào trúng mạch quặng** trong hầm tối.

- **Vòng quay Shuffle** (nhảy lung tung như cuốc bổ trúng mạch ngẫu nhiên), nhịp 0,07 → 0,5 s, chậm mạnh ở cuối, "suýt trượt" 65%. Tick = tiếng mũi cuốc gõ đá, cao độ 0,8 → 1,25.
- **Đèn hầm mỏ** (MinerHook): đèn điểm vàng cam treo trên miệng rương, chập chờn (Perlin), **sụt sáng mỗi nhát tick** như đèn rung khi cuốc bổ; mỗi tick bắn 5 tia lửa Streak cam.
- **Hạt lõi**: bụi đá xám nâu (70%) pha màu bậc, Fountain, trọng lực 1,6 (rơi nhanh hơn xu của Golden).
- **Khi khoá**: đá vụn tối bắn lên rồi rơi (trọng lực 2,2, số lượng ×1–2,5 theo bậc); từ Hiếm trở lên thêm **ánh ngọc** Sparkle màu bậc; Huyền thoại: đèn **giật tắt/bật 0,8 s** như rung do nổ mìn xa (khớp tiếng nổ mìn). Tia ngắn (×0,8), gần như không xoay (4°/s) như vết nứt đá. Đèn chớp lõi dịu (5).
- Sau `onRevealed` đèn tắt dần.

## Nhịp (rương không chìa, theo bảng lõi)

| Giây | Việc |
|---|---|
| 0,9–2,5 | Gỡ niêm: tiếng đèn dầu kẽo kẹt/xèo |
| 3,7 | Hé nắp: nắp gỗ nghiến sạn, nền hầm mỏ (nước nhỏ giọt, gỗ chống kêu), đèn bật, bắt đầu quay (≥3,0 s) |
| 3,7–6,7 | tick cuốc + tia lửa + đèn sụt nhịp |
| ~6,7 | Khoá: tiếng chốt theo bậc, đá vụn, ánh ngọc (≥Hiếm), giật đèn (Huyền thoại), vút bụi đá khi bóng phóng to, sỏi rơi lách tách (1 + bậc lần) |
| ~7,9 | Mở toang: thud chung + xe goòng lăn nhỏ |

## Âm

Xem bảng đủ ở `Assets/_Project/Audio/Chests/miner/README.md` (12 file ElevenLabs). Chốt: Rác = cuốc trúng sỏi "tonk" + xì hơi (hài); Thường = một tiếng keng; Hiếm = hai keng lên + chuông pha lê; Rất hiếm = vách đá nứt lộ ngọc; Huyền thoại = nổ mìn xa + hợp âm pha lê vang hang.

## File

- `Scripts/ChestFx/Chests/MinerFxPreset.cs` — `MinerFxPreset.Apply(p)` + tên file âm.
- `Scripts/ChestFx/Chests/MinerHook.cs` — đèn, tia lửa, đá vụn, ánh ngọc.
- `Editor/MinerFxSetup.cs` — menu *Tools/Cho Do La/Create Miner Chest FX Profile* và *Add Miner Hook To Selected Chest*.
- `Tools/CompileCheck/check_miner.sh` — biên dịch thử với stub.

## Cách thử trong Unity

1. `git lfs pull` (Chest_Miner.glb đang là con trỏ LFS).
2. *Tools/Cho Do La/Add Chest Reveal Test* → bấm C tới Chest_Miner (hoặc kéo GLB vào cảnh, thêm BoxOpenSequence + ChestRevealSequence + AudioSource).
3. Chọn gốc rương → *Tools/Cho Do La/Add Miner Hook To Selected Chest* (tạo `ChestFx_Miner.asset`, gắn hồ sơ và hook).
4. Play, T đổi bậc, Space mở. Nghe tick có rõ ở nhịp 0,07 s không; xem đèn có quá tối/sáng, đá vụn có rơi đúng không.

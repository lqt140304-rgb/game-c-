# Thiết kế: hiệu ứng và âm thanh rương Magic (`Chest_Magic`)

Trạng thái: **đề xuất, chưa chạy trong Unity, chưa ai nghe âm**. Dựa trên lõi [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md); không đổi điểm chung đã chốt (màu bậc nhảy rồi dừng đúng bậc trúng, bóng vật phẩm to dần).

## Ý tưởng: "rương đá hát"

Rương đá có tinh thể tím. Khi mở, tinh thể **ngân như chuông thuỷ tinh**: mỗi lần màu nhảy là một tiếng gõ pha lê và một **vòng tia lấp lánh bật ra quanh miệng rương** (màu đang hiện). Hạt tinh thể **xoáy** quanh rương rồi lơ lửng bay lên (ma thuật được giải phóng, không rơi như xu Golden). Màu nhảy kiểu **Shuffle** (chập chờn như phép đang chọn, không phải vòng xèng), đoạn cuối **ngập ngừng lâu** (đường chậm dần dốc ở 30% cuối) và hay "suýt trượt" (65%).

Khác các rương khác: Golden = xu rơi theo Cycle; Magic = xoáy lên, Shuffle, tia quay **ngược chiều chậm**, vòng cộng hưởng mỗi tick, vòng rune lan trên mặt phẳng khi chốt, và Rác thì **phép xịt** (vài đốm xám rơi bịch + tiếng xì hơi hài).

## Thành phần

| File | Vai trò |
|---|---|
| `Scripts/ChestFx/Chests/MagicFxPreset.cs` | `MagicFxPreset.Apply(p)`: giá trị hồ sơ + tên file âm |
| `Scripts/ChestFx/Chests/MagicHook.cs` | Hook: vòng cộng hưởng mỗi tick; khi chốt: phép xịt (Rác), vòng rune (Thường+), vòng vọng sau 0,35 s (Rất hiếm+), cột tia bắn lên (Huyền thoại) |
| `Editor/MagicFxSetup.cs` | Menu `Tools/Cho Do La/Create Magic Chest FX Profile` → `ScriptableObjects/ChestFx/ChestFx_Magic.asset` |
| `Audio/Chests/magic/*.mp3` + `README.md` | 12 âm ElevenLabs |
| `Tools/CompileCheck/check_magic.sh` | Biên dịch thử bằng stub |

Giá trị chính: Sparkle + Swirl, phụ tím `#C78CFF` trộn 0,6, 38 hạt/s, nổ 55 hạt, tốc 0,45, sống 1,8 s, trọng lực nổ −0,08 (bay lên); tick 0,06 → 0,5 s, quay tối thiểu 3,0 s; tia ×1,3, 1,5 s, quay −12°/s; đèn 8.

## Nhịp giây (rương có chìa: cộng 2,15 s như Golden nếu GLB có `Key`)

| Giây | Hình | Âm |
|---|---|---|
| 0,0 – 0,9 | rương rung (lõi) | — |
| 0,9 | Gỡ niêm | gió thì thầm hang động (2,5 s) |
| **3,7** | Hé nắp: bắt đầu Shuffle; hạt Sparkle xoáy lên; mỗi tick một vòng 10+ tia nhỏ quanh miệng rương | tiếng nắp đá nghiến; nền tinh thể ngân (4 s); tick pha lê, cao độ 0,8 → 1,8 |
| 3,7 – 6,7 | màu chập chờn nhanh, 30% cuối chậm hẳn, có thể suýt trượt | — |
| **6,7** | Chốt: tia quay ngược, chớp tím; Rác = 14 đốm xám rơi; Thường+ = vòng rune lan ngang (42–112 tia); Rất hiếm+ vòng vọng lúc 7,05; Huyền thoại thêm cột 90 vệt bay lên | tiếng chốt theo bậc; 0–4 tiếng mảnh pha lê leng keng |
| 6,7 – 7,9 | bóng vật phẩm to dần (lõi) | whoosh ma thuật (1,4 s) |
| 7,9 | Mở toang | phiến đá đặt xuống + ngân |
| ~8,5 | `onRevealed` | — |

(Giây tính theo bảng lõi; `minRouletteSeconds` 3,0 nên chốt muộn hơn Golden ~0,2 s.)

## Âm

Xem [Audio/Chests/magic/README.md](../../../Assets/_Project/Audio/Chests/magic/README.md). Chỗ thiếu mp3 riêng thì menu lấy bộ chung `Audio/Chests/` (rồi `Synth/`).

## Cách thử trong Unity

1. Mở dự án, chờ import (GLB cần glTFast; `git lfs pull` nếu `Chest_Magic.glb` chỉ 132 byte).
2. `Tools/Cho Do La/Create Magic Chest FX Profile`.
3. `Tools/Cho Do La/Add Chest Reveal Test`, trong cảnh bấm C tới Chest_Magic (hoặc kéo GLB vào). Trên gốc rương: kéo `ChestFx_Magic` vào `profile` của `ChestRevealSequence`, **thêm component `MagicHook`**.
4. Play: T đổi bậc, Space mở, R đóng. Thử đủ 5 bậc, đặc biệt Rác (phép xịt) và Huyền thoại (cột + vòng vọng).
5. Nghe: tick có quá đục khi dồn 0,06 s? Nền 4 s có tắt trước khi chốt (quay 3 s) — nếu hụt thì cần clip dài hơn hoặc loop.

## Chưa kiểm chứng / câu hỏi

- Chưa chạy Unity; chỉ biên dịch với stub. Hạt chưa thấy tận mắt (kích thước vòng `rimRadius` 0,22 m đoán theo rương ~0,5 m).
- `burstGravity` âm: chưa chắc trông đẹp; nếu hạt bay vọt quá thì đặt 0.
- GLB trong nhánh này chỉ là con trỏ LFS (máy dựng không tải được nội dung LFS).

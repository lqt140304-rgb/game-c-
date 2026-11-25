# Hiệu ứng + âm thanh rương Mimic (Chest_Mimic)

Bản sắc: **rương đói bụng có răng**. Không lấp lánh, không vàng: tim đập, nhỏ dãi, cắn, ợ, gầm.
Lõi chung (màu nhảy rồi dừng đúng bậc, bóng vật phẩm to dần) giữ nguyên.

## File
- `Assets/_Project/Scripts/ChestFx/Chests/MimicFxPreset.cs` — `MimicFxPreset.Apply(p)` + tên file âm.
- `Assets/_Project/Scripts/ChestFx/Chests/MimicHook.cs` — hook bằng code (cắn, tim đập, dãi, nôn).
- `Assets/_Project/Editor/MimicFxSetup.cs` — menu `Tools/Cho Do La/Create Mimic Chest FX Profile` và `Tools/Cho Do La/Add Mimic FX To Selected Chest`.
- `Assets/_Project/Audio/Chests/mimic/` — 13 âm `sfx_mimic_*.mp3` + README.
- `Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs` — **chép nguyên phần biteIntro/onBite từ nhánh `claude/mimic-chest`** (mặc định tắt).
- `Tools/CompileCheck/check_mimic.sh` — biên dịch thử bằng stub.

## Khác các rương khác thế nào
| Mục | Mimic |
|---|---|
| rouletteStyle | Shuffle (giật cục, đói, không thứ tự), nearMiss 0,7 ("nuốt rồi nhả") |
| tick | tiếng tim đập ướt, pitch 0,7 → 1,25 (tim đập nhanh dần khi sắp "ăn") |
| particleLook / direction | SoftDot (cục nhớt) / Fountain, gravity 1,6 (phun lên rồi rơi bẹp) |
| màu phụ | xanh nhớt (0,62; 0,95; 0,25), mix 0,6 |
| tia | ngắn (x0,75), quay NGƯỢC chiều (-40°/s) như răng xoay |
| đèn | 8 (đỏ cam của quầng sáng mẫu + màu bậc) |
| hook | mỗi tick rương phồng 3,5 % rồi xẹp (tim đập); 25 % tick nhỏ một giọt dãi từ mép; khi chốt phồng 2–5 lần; Rác nôn ra 90 cục nhớt xanh |

## Nhịp giây (mặc định BoxOpenSequence + lõi)
| t | Sự kiện | Hình | Âm |
|---|---|---|---|
| 0,00 | bite intro: nắp há 32° (0,16 s), giữ 0,10 s | — | — |
| 0,33 | nắp sập (0,07 s) → `onBite` | rương giật 1,2 cm + phồng | sfx_mimic_bite + teeth_chatter |
| 0,33–0,78 | nghỉ | rung tắt dần | — |
| sau đó | giai đoạn 1 Rung, 2 Hé nắp | | creak_jaw ở Hé nắp |
| Hé nắp | vòng quay ≥ 3,0 s | màu nhảy Shuffle, rương đập như tim, dãi nhỏ giọt | growl_bed + tick tim đập, drool thỉnh thoảng |
| Bật nắp | chốt bậc | tia, nhớt phun, (Rác: nôn) | lock_<bậc> + whoosh_spit |
| sau chốt | accent: Rác 0, mỗi bậc +1 | | lick / drool / chatter |
| Mở toang | | | bite (0,5) như ngậm lại |

## Cách thử trong Unity
1. Tools/Cho Do La/Add Chest Reveal Test, nhấn C tới Chest_Mimic (hoặc đặt Mimic làm rương đầu).
2. Chọn rig của Mimic (object có BoxOpenSequence) → Tools/Cho Do La/Add Mimic FX To Selected Chest.
3. Play, T chọn bậc, Space mở. Nghe tiếng cắn trước khi rung; thử Rác (nôn) và Huyền thoại (gầm).
4. Nếu nắp lật sai chiều trong bite: chỉnh `lidOpenSign` / `biteOpenAngle` trên BoxOpenSequence.

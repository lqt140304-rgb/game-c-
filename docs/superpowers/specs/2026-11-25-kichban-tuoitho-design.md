# Kịch bản mở rương Chest_TuoiTho — "Cỗ máy bi ve"

Ngày 2026-11-25, đã thu gọn theo kế hoạch 2026-11-25. Mã: `Scripts/ChestFx/Chests/TuoiThoHook.cs`, `TuoiThoPiece.cs`;
editor `Editor/TuoiThoFxSetup.cs`; âm ElevenLabs `Audio/Chests/tuoi_tho/*.mp3` (danh mục `Tools/Audio/elevenlabs_chest_manifest.json`).

## Ý tưởng (đã thu gọn theo kế hoạch 2026-11-25)
Cỗ máy Rube Goldberg tuổi thơ chạy bằng **Rigidbody thật** cạnh rương, nay chỉ còn **3 khâu**:
búng bi → bi lăn xuống thước kẻ dốc ngắn (đầu dốc tựa một khối nhỏ) → **3 quân domino** đổ → quân cuối đổ vào chốt ná →
ná dây thun bắn viên bi thứ hai theo parabol lên nắp → **cả rương nảy**. Kết lộ bậc bằng máy bay giấy.
Đã bỏ: tờ giấy ô li, hộp bút, 2 bút, xe đồ chơi, con quay, còi trượt dâng. Vùng chơi nằm trong 1,2 S bên trái rương + ná
0,95 S phía trước (S = cạnh lớn nhất khung bao rương). Cơ chế riêng: chuỗi phản ứng vật lý + người chơi thả bi đổi đường đi.

## Nhịp (T = giây từ lúc bấm tới stage chốt bậc, ước từ BoxOpenSequence, kẹp 3–9 s; mặc định ≈ 5,3 s)
| Thời điểm | Sự kiện | Âm |
|---|---|---|
| 0 (bấm mở) | sân chơi hiện ra | **Nền** `roll_loop` (vòng, 0,15 → 0,35, cao độ +45% tới chốt) + tiếng búng bi `flick` + **Nhịp** mộc cầm `xylo` (0,65 s → 0,14 s, 0,25 → 0,55, ngũ cung) |
| 0,12 s | bi lăn | (nền) |
| 0,25T→0,55T | domino đổ (trễ >0,3 s thì có cú hích dự phòng) | `domino` mỗi quân cao thêm 1 nửa cung (cách nhau ≥ 0,12 s); rương rùng mình tăng dần |
| quân cuối ngã (hoặc max(0,8T, T−0,6)) | dây thun bật | `band` |
| +0,5 s | bi đập nắp, rương nảy | `bonk` — chưa lộ bậc; nếu chưa chốt thì rương rung căng chờ |
| chốt bậc (sau cú đập) | kết | xem dưới |

## Kết theo bậc (chỉ sau OnTierLocked; trước đó mọi bậc giống hệt)
- **Rác**: nền + nhịp **dừng ngay** → lặng 0,2 s → 1 tiếng xịt (`lock_rac`, pin hết); rương xẹp ~2,5 s rồi phồng lại; máy bay
  giấy chòng chành, cắm đầu, vò thành cục giấy cộc vào rương rồi lăn đi. Đuôi: `plane` (~0,9 s), `crumple`, +0,15 s `bonk`.
- **Thường→Huyền thoại**: 1 tiếng chốt `lock_<bậc>` (0,7 / 0,85 / 1 / 1), Thường: nền fade 0,3 s, bậc cao hơn: nền dừng hẳn;
  không có tiếng hành động riêng ngoài `bonk` lúc đập nắp. Rương nhảy cao 0,08→0,43 cỡ rương, Rất hiếm+ xoay một vòng trên không;
  domino bật nảy (lực thật); máy bay lượn vòng bán kính 0,9 S, Rất hiếm+ nhào lộn, thả hoa giấy (20 + 60·bậc + 10·combo, trần 120 hạt).
  Đuôi: `plane` (+0,4 s, 0,35) và một nốt mộc cầm nhỏ lúc hoa giấy.
- Màu 5 bậc nhảy/dừng và bóng vật phẩm to dần vẫn do ChestRevealSequence chạy song song, không đụng tới.

## Tương tác phụ
Trong lúc máy chạy: bấm chuột (hoặc phím B) thả thêm bi ve từ trên cao xuống chỗ trỏ trên mặt bàn (**tối đa 4**), tiếng `marbles`.
Bi thật có thể đổ domino sớm. Bi rơi trúng rương = combo: nốt cao dần + rương nảy nhẹ + thêm hoa giấy lúc kết. Không ảnh hưởng bậc.

## Âm (A1–A12)
Chỉ file ElevenLabs `.mp3` trong `Audio/Chests/tuoi_tho/` (không còn `may_bi/*.wav` numpy). Hồ sơ lõi câm
(`ChestAudioRule.ApplyToProfile` mỗi lần chạy menu; `tension.enabled = false`). Lớp kéo dài trước chốt: 2 (nền lăn + nhịp mộc cầm).
Tiếng chốt: `stingClips[5]` = `sfx_tuoitho_lock_*`. Ba nguồn tối đa: nền (`_bed`), sfx (`_sfx`), cộng nguồn tạm của
`ChestRevealSequence.PlayOneShot` cho tiếng đổi cao độ.

## Bảng số (bản chốt)
| Hạng mục | Giá trị |
|---|---|
| Khâu cỗ máy | 3 (bi–dốc, 3 domino, ná) + máy bay giấy lúc kết |
| Diễn viên ngoài rương | 1 (máy bay giấy lúc kết); cỗ máy là đạo cụ sát rương |
| Vùng | trong 1,2 S bên trái + ná 0,95 S trước |
| Rigidbody trước chốt | tối đa 9 (bi, bi ná, 3 domino, ≤ 4 bi thả thêm) |
| Bi thả thêm | ≤ 4 |
| Lớp âm trước chốt | 2 (lăn + mộc cầm) |
| `tension.enabled` | false |

## Bất biến
Bậc do `ChestRevealSequence.Play(rolledTier, …)`; hook không đọc bậc trước `OnTierLocked`, không đổi gì của khung.
Ngẫu nhiên (màu bi, lực trang trí) từ `decorationSeed` riêng. Chuyển động rương được cộng lên tư thế mà
BoxOpenSequence ghi mỗi khung (không giành quyền), trả về nguyên trạng khi reset.

## Cách thử trong Unity
1. `Tools/Cho Do La/Create Tuoi Tho Chest FX Profile` (cập nhật profile: bỏ âm nền, bỏ âm stage 2).
2. Chọn gốc Chest_TuoiTho (có ChestRevealSequence) → `Tools/Cho Do La/Add Tuoi Tho Marble Machine`.
3. Chạy cảnh thử (ChestRevealDemo, phím T/Y…), thử đủ 5 bậc; bấm chuột lúc máy chạy để thả bi.
4. Chỉnh `lockStageGuess`/`extraLeadSeconds` nếu cú đập không rơi sát lúc chốt; chỉnh kích thước trong `BuildPlayground`.

## Trung thực: chưa làm / chưa kiểm
- **Chưa chạy trong Unity, chưa ai nghe âm** (ElevenLabs, chưa kiểm điều khoản giấy phép). Chỉ biên dịch qua stub
  (`Tools/CompileCheck/check_tuoi_tho.sh`, `check_all.sh`), không chứng minh API thật/vật lý đúng.
- Quân domino cuối **không thật sự chạm** chốt ná (ná cách 0,95 S phía trước): ná bật khi domino cuối ngã hoặc tới giờ dự phòng.
- Vị trí đạo cụ mới (dốc −1,15 S → −0,75 S, domino −0,6/−0,45/−0,3 S) chưa thử: bi có thể không đủ đà đổ quân đầu, đã có cú hích dự phòng.
- Cỗ máy có thể xong sớm hơn lúc chốt tới vài giây (T lớn): rương rung căng chờ, nền + nhịp tiếp tục dâng.
- Menu cầm rương (`CreateAllChestFxProfiles`, không thuộc mục này) gắn `TuoiThoHook` không gán âm — cần gọi `Add Tuoi Tho Marble Machine` hoặc sửa thư viện riêng.
- Vật liệu dùng `ChestFxFactory.CreateUnlit` (không bóng đổ).

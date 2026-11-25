# Kịch bản mở rương Chest_TuoiTho — "Cỗ máy bi ve"

Ngày 2026-11-25. Thay hẳn bản cũ (nến + đom đóm). Mã: `Scripts/ChestFx/Chests/TuoiThoHook.cs`, `TuoiThoPiece.cs`;
editor `Editor/TuoiThoFxSetup.cs`; âm `Audio/Chests/tuoi_tho/may_bi/` (tạo bằng `Tools/Audio/make_tuoitho_machine_sfx.py`).

## Ý tưởng
Một cỗ máy Rube Goldberg trên bàn học, dựng từ đồ chơi tuổi thơ, chạy bằng **Rigidbody thật** cạnh rương:
búng bi → bi lăn xuống thước kẻ kẹp giữa bút chì/bút bi (đầu dốc tựa hộp bút) → đổ hàng domino trên tờ giấy ô li →
domino cuối tông xe đồ chơi → xe húc con quay đang quay → còi trượt dâng → ná dây thun bắn viên bi thứ hai theo
parabol lên nắp → **cả rương nảy**. Kết lộ bậc bằng máy bay giấy.
Cơ chế riêng (không rương nào khác có): chuỗi phản ứng vật lý nhiều khâu + người chơi thả bi đổi đường đi.

## Nhịp (T = giây từ lúc bấm tới stage chốt bậc, ước từ BoxOpenSequence, kẹp 3–9 s; mặc định ≈ 5,3 s)
| Thời điểm | Sự kiện | Âm |
|---|---|---|
| 0 (bấm mở) | sân chơi hiện ra | búng bi "tách" + nhịp mộc cầm bắt đầu (0,5 s/nốt, ngũ cung) |
| 0,12 s | bi lăn | tiếng lăn |
| 0,25T→0,55T | domino đổ (trễ >0,3 s thì có cú hích dự phòng) | "cách" mỗi quân cao thêm 1 nửa cung; rương rùng mình tăng dần |
| ~0,5–0,62T | xe lao | rừừừ |
| ~0,78T | con quay bị húc, lảo đảo | vù vù + **còi trượt dâng** (pitch chỉnh cho khớp tới cú đập) |
| max(0,8T, T−0,6) | dây thun bật | "tưng"; nhịp mộc cầm đã dồn tới 0,11 s/nốt |
| +0,5 s | bi đập nắp, rương nảy | "cộp", còi tắt — chưa lộ bậc; nếu chưa chốt thì rương rung căng chờ |
| chốt bậc (sau cú đập) | kết | xem dưới |

## Kết theo bậc (chỉ sau OnTierLocked; trước đó mọi bậc giống hệt)
- **Rác**: còi trượt rơi + xì hơi "xịt"; rương xẹp (bẹp dọc) ~2,5 s rồi phồng lại; máy bay giấy chòng chành, cắm đầu,
  vò thành cục giấy cộc vào rương rồi lăn đi (Rigidbody).
- **Thường→Huyền thoại**: quét mộc cầm + kèn giấy, âm lượng/pitch theo bậc; rương nhảy cao 0,08→0,43 cỡ rương,
  Rất hiếm+ xoay một vòng trên không; đồ chơi trên bàn bật nảy (lực thật); máy bay lượn vòng, Rất hiếm+ nhào lộn,
  thả hoa giấy (30 + 140·bậc + 15·combo hạt).
- Màu 5 bậc nhảy/dừng và bóng vật phẩm to dần vẫn do ChestRevealSequence chạy song song, không đụng tới.

## Tương tác phụ
Trong lúc máy chạy: bấm chuột (hoặc phím B) thả thêm bi ve từ trên cao xuống chỗ trỏ trên mặt bàn (tối đa 6).
Bi thật có thể đổ domino sớm, đẩy xe… Bi rơi trúng rương = combo: nốt cao dần + rương nảy nhẹ + thêm hoa giấy lúc kết.
Không ảnh hưởng bậc.

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
- **Chưa chạy trong Unity, chưa ai nghe âm.** Chỉ biên dịch qua stub (`Tools/CompileCheck/check_tuoi_tho.sh`, `check_all.sh`),
  không chứng minh API thật/vật lý đúng. Vị trí đồ chơi đặt theo cỡ rương, có thể chồng lên vật khác trong cảnh.
- Vật lý thật có thể lệch: đã có cú hích dự phòng theo giờ cho domino/xe/con quay/ná, cú đập nắp tính theo giờ.
- Vật liệu dùng `ChestFxFactory.CreateUnlit` (không bóng đổ). Âm là WAV tổng hợp numpy, có thể cần thay bản thu thật.
- Nhánh `claude/chest-fx-core` chưa tồn tại lúc làm (fetch báo không có ref) → chưa nối vào khung chung. Khi có: cần
  một sự kiện "bấm mở" chính thức (hiện hook dò `reveal.IsPlaying` bật lên) và lấy giờ chốt bậc thật thay `lockStageGuess`.

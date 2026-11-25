# Kịch bản mở rương Thợ Mỏ (Chest_Miner) — "Đào rương ra khỏi mỏ"

Trạng thái: **đã viết code, biên dịch stub qua; CHƯA chạy trong Unity, CHƯA ai nghe âm.**

## Ý tưởng
Rương không "mở nắp" ngay mà bị **vùi trong vỏ đá**. Người chơi (hoặc cuốc tự đập) phá đá từng nhát; đá là khối
**Rigidbody thật** văng ra bằng lực, lăn trên sàn rồi tan. Đặc trưng riêng: **thuốc nổ ngòi cháy** — cắm vào đá, ngòi
xèo xèo, nổ tung mọi khối còn lại, **cả rương nảy lên rồi rơi xuống**. Nhân vật phụ: **chim hoàng yến** của thợ mỏ
(đúng tích "chim báo khí mỏ") bay vào đậu cạnh rương, run theo độ căng; kết quả quyết định số phận nó.

## Nhịp (giây, tự chạy không bấm gì; số nhát 4–6 theo hạt giống)
| t | Việc | Âm |
|---|---|---|
| 0 | Bấm mở → `MinerDigSequence.Begin` dựng ~26 khối đá quanh rương, cuốc hiện | **Ngay lập tức**: tiếng mỏ rền (loop) + tiếng đá nứt + nhịp "thình" 0,9 s |
| 0,55 → ~3 | Mỗi ~0,85 s (nhanh dần) một nhát: cuốc vung, tia chớp, 2+ khối văng, rương rung, sỏi rơi từ trần | cách kim loại + nứt; rền lên pitch, thình dày dần |
| nhát áp chót | Không ai cắm thì tự cắm thuốc nổ | ngòi xèo (pitch lên) , thình nhanh gấp đôi |
| +2,2 | NỔ: mọi đá còn lại văng, rương nảy (0,55 s), đáp "thịch" | boom trầm |
| +0,35 | Đá sạch → `ChestRevealSequence.Play(rolledTier, item)` (khung chung: nắp, bánh xe 5 màu, bóng vật phẩm to dần — **giữ nguyên**) | rền tiếp tục dâng theo **từng tick màu** (giống nhau mọi bậc → giữ bí mật) |
| chốt bậc | Kết quả lộ ra | xem dưới |

## Âm theo bậc (chỉ tại `OnTierLocked`)
- **Rác / Thường — XỊT:** tiếng rền **tắt phụt**, còi trượt tụt 900→140 Hz + "phụt"; chim ngất, rơi lăn (Rigidbody).
  Rác thêm: một hòn sỏi rơi "cốc" lên rương.
- **Hiếm:** rền tắt êm 0,4 s, chim hót, bay 1 vòng.
- **Rất hiếm:** nổ lần hai (mạch vàng), đèn chớp màu bậc, sập trần 8 sỏi, chim bay 2 vòng hót.
- **Huyền thoại:** như trên, 14 sỏi, chim 3 vòng, **18 cục vàng Rigidbody** phun từ miệng rương rơi leng keng.

Mọi âm của kịch bản được **tổng hợp lúc chạy** trong `MinerSynth.cs` (không file, không credit). Âm khóa/tick của hồ sơ
`ChestFx_Miner` (thư mục `Audio/Chests/miner/`) vẫn chạy chồng như cũ.

## Tương tác
- **F hoặc chuột trái**: đập thêm nhát (nhanh hơn tự đập). **G**: cắm thuốc nổ ngay (đi tắt).
- **Thả/ném vật**: gắn `MinerDynamite` lên vật bất kỳ có Collider + Rigidbody; chạm khối đá → cắm thuốc nổ ở đó.
- API: `MinerDigSequence.TryBegin(reveal, tier, item)`, `Strike()`, `PlaceDynamite(Vector3?)`.

## Bất biến
- Bậc chỉ được **chuyển tiếp nguyên vẹn** vào `ChestRevealSequence.Play`; kịch bản không đọc/đổi xác suất.
- Ngẫu nhiên trang trí dùng `System.Random(decorationSeed)` (0 = khác mỗi lần); không dùng `UnityEngine.Random`.
- Bánh xe màu + bóng vật phẩm là của khung chung, không đụng.
- Không dùng ParticleSystem mới (tránh lỗi orbitalX/Y/Z): hiệu ứng là khối vật lý + đèn.

## File
- `Assets/_Project/Scripts/ChestFx/Chests/MinerDigSequence.cs` (kịch bản, là `ChestFxHook`; kèm `MinerDigLink`, `MinerDynamite`)
- `Assets/_Project/Scripts/ChestFx/Chests/MinerSynth.cs` (âm tổng hợp)
- `Assets/_Project/Editor/MinerDigSetup.cs` (menu *Tools/Cho Do La/Add Miner Dig Sequence To Selected Chest*: gắn, gỡ `MinerHook` cũ, gán hồ sơ)
- `Tools/CompileCheck/check_miner_dig.sh` (check_miner.sh chuyển sang đây); thêm `AudioClip.Create/SetData`, `Collision` vào `UnityStubAll.cs`.

## Cần nối sau (chưa làm — phụ thuộc phiên lõi)
Nhánh `claude/chest-fx-core` **chưa có trên remote** khi viết (2026-11-25). Hiện nơi bấm mở (`ChestRevealDemo`,
menu cầm rương) gọi thẳng `reveal.Play(...)` nên **kịch bản đào chưa tự chạy**. Cần một trong hai:
1. Khung lõi có điểm "intro trước Play" (vd. hook `OnOpenPressed` có thể trì hoãn Play) → `MinerDigSequence` đăng ký vào đó; hoặc
2. Sửa tối thiểu nơi gọi: `if (!MinerDigSequence.TryBegin(reveal, tier, item)) reveal.Play(tier, item);`.

## Cách thử trong Unity
1. Mở cảnh thử rương (Tools/Cho Do La/Add Chest Reveal Test), chọn Chest_Miner trong Hierarchy.
2. Tools/Cho Do La/Add Miner Dig Sequence To Selected Chest.
3. Tạm sửa chỗ Space trong `ChestRevealDemo` theo mục "Cần nối sau" (2), hoặc gọi `TryBegin` từ một script thử.
4. Play, Space: nghe âm ngay khi bấm; F/chuột đập, G nổ. Thử đủ 5 bậc bằng phím T.
5. Kiểm: đá có rơi trên sàn (có sàn vô hình tạm), rương về đúng vị trí trước khi mở nắp, bóng vật phẩm to dần đúng.

## Rủi ro / chưa kiểm
- Kích thước khối đá/cuốc/chim tính theo kích thước rương — chưa nhìn bằng mắt.
- Vòng lặp tiếng rền có fade 50 ms ở chỗ nối → có thể nghe "phập phồng" mỗi 2 s.
- Đặt `pitch` cho nguồn one-shot ảnh hưởng cả âm đang vang.
- `MinerHook` cũ vẫn có thể được sinh qua `customFxPrefab` của hồ sơ (CreateAllChestFxProfiles) → đá hạt + đèn chồng thêm; xóa prefab đó nếu thấy rối.

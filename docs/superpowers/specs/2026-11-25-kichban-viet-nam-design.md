# Kịch bản mở rương: Chest_VietNam — "ĐÊM HỘI LÀNG"

Trạng thái: **đã viết code, biên dịch thử bằng stub (OK), CHƯA chạy trong Unity, CHƯA ai nghe âm.**
Thay hook cũ `VietNamHook` (sóng trống đồng + mưa hạt lúa) bằng `VietNamHoiLangHook`. File cũ giữ nguyên, không còn được gắn.

## Ý tưởng

Rương thúng tre đội nón lá được "hạ thủy" giữa đêm hội. Đèn hoa đăng **thật về vật lý** (Rigidbody, lực nổi, va chạm)
trôi tụ quanh rương; cả rương bập bềnh và chao khi bị đèn va. Nhịp trống – mõ – đàn bầu dâng dần. Lúc chốt bậc,
câu chuyện rẽ nhánh: bậc thấp xịt và chìm (hài), bậc cao trống đồng nổ, đèn thành đèn trời, chim Lạc thả quà.
Không ai khác dùng nước/nổi/đèn trôi làm cơ chế chính.

## Nhịp (rương không chìa, giai đoạn mặc định; khoá dự kiến ≈ 6,5–7 s)

| Giây | Sự kiện |
|---|---|
| 0 (bấm mở) | Mặt nước hiện ra; rương bập bềnh nhẹ (±0,8 cm, nghiêng ±1,5°). Đàn bầu ngân lặp (pitch 0,85, nhỏ). Trống cái gõ 0,75 s/nhịp. Đèn hoa đăng thả ở xa (12 đèn, ≤ 6 có Light), mỗi đèn "tõm" + gợn nước |
| 0 → khoá | Độ căng k = t / thời gian khoá dự kiến (+0,05 mỗi đèn người chơi thả). Nhịp nhanh dần tới 0,11 s, từ k > 0,4 mõ chen nhịp lẻ; đàn bầu lên cao độ 1,35 và to dần; đom đóm tụ ở miệng rương; rương lắc mạnh dần (±3 cm, ±6°), mỗi tiếng trống đá rương một cú |
| Khoá: **Rác** | Đàn bầu tụt cao độ "xì hơi" + tiếng XỊT; đèn tắt phụt lần lượt; rương chìm 35% chiều cao "bloop" rồi nổi; ếch (Rigidbody, ném xiên) nhảy lên nắp kêu "ộp" |
| Khoá: **Thường** | Trống im bặt, một tiếng mõ trầm lẻ loi, đàn bầu tắt; đèn mờ và dạt ra xa; rương hụt nhẹ |
| Khoá: **Hiếm / Rất hiếm / Huyền thoại** | Trống đồng + cồng "BÙM"; đèn bị hất tung bằng xung lực; rương nhảy khỏi nước (0,37–0,73 × cao) do đom đóm nâng; 40–60% đèn (HT: tất cả) thành đèn trời bay lên; 1/2/3 chim Lạc sà qua, mỗi con thả một bánh chưng rơi xuống nước (trọng lực thật, tõm, rồi nổi). HT lơ lửng 2,4 s thay vì 0,9 s |

Âm lock riêng của hồ sơ (`sfx_vietnam_lock_*`) và vòng màu/bóng của lõi vẫn chạy chồng lên như cũ.
Trước khi khoá mọi thứ giống nhau ở mọi bậc → giữ bí mật.

## Tương tác phụ

- **Phím L** (Input System hoặc Input cũ): người chơi thả thêm một đèn hoa đăng ở chỗ chuột trỏ trên mặt nước (ngoài
  mặt nước thì trước mặt rương). Game có thể gọi `VietNamHoiLangHook.OfferLantern()` từ nút UI.
- Thả đủ **3 đèn** trước lúc khoá → **Rùa vàng** trồi lên, bơi nửa vòng quanh rương (rẽ đèn bằng lực), húc rương một cú rồi lặn.
- Không cái nào ảnh hưởng bậc.

## Bất biến

- Hook không đọc bậc trước `OnTierLocked`, không gọi gì đổi kết quả. Ngẫu nhiên (`System.Random`, trường `seed`, 0 = mỗi lần khác) chỉ cho vị trí đèn, màu cánh, góc.
- Vòng màu 5 bậc và bóng vật phẩm do `ChestRevealSequence` làm, không đụng. Bóng đi theo miệng rương nên bay/chìm cùng rương.
- Không sửa file dùng chung trừ stub biên dịch `Tools/CompileCheck/UnityStubAll.cs` (thêm `ForceMode.Acceleration/VelocityChange`, `Rigidbody.AddTorque/position`, `Collision`).

## Kỹ thuật

- `Chests/VietNamHoiLangHook.cs` — đạo diễn; `VietNamHoiLangFloater.cs` — vật nổi (lò xo nổi, cản nước, tụ vòng + xoáy, chế độ đèn trời); `VietNamHoiLangProps.cs` — dựng đạo cụ bằng khối cơ bản.
- Tư thế cả rương: **cộng thêm** lên localPosition/localRotation mà `BoxOpenSequence` ghi mỗi khung; khi nó không ghi thì gỡ phần cộng cũ. Reset trả về đúng tư thế.
- Collider rương tạm (`HoiLangChestBump`) và mọi đạo cụ ở lớp 2 (Ignore Raycast) để không chặn tia chọn đồ.
- Âm: `Audio/Chests/viet_nam/hoi_lang/hl_*.wav`, tạo bằng `python3 Tools/Audio/make_vietnam_hoilang_sfx.py` (numpy, hạt giống cố định).
- Particle: chỉ dùng `ChestFxFactory.CreateParticles` (không đặt orbitalX/Y/Z).

## Cách thử trong Unity

1. Menu **Tools/Cho Do La/Create Viet Nam Hoi Lang FX** (tạo hồ sơ nếu chưa có, dựng `VietNamHoiLangFx.prefab`, gắn làm customFxPrefab).
2. Mở cảnh thử rương như `Docs/HuongDanThuUnity.md`, chọn Chest_VietNam, phím T/P để mở với từng bậc.
3. Trong lúc dâng nhịp bấm **L** ba lần → rùa vàng.
4. Cần kiểm: rương có trôi lệch sau reset không; đèn có rơi xuyên nước (tăng lực lò xo 220) không; số Light; âm lặp đàn bầu có tiếng "tách" ở điểm nối không; cân bằng âm lượng với âm lock.

## Chưa làm / nối sau

- Nhánh `claude/chest-fx-core` **chưa có trên origin** lúc viết (2026-11-25) nên chưa nối API khung. Khi có: chuyển phần
  "bắt đầu khi bấm" từ `Start()` sang sự kiện khung (nếu khung có OnPlay/OnOpenPressed), và phần đoán giờ khoá
  `EstimateLockSeconds()` sang thời lượng khung cung cấp.
- Chưa chạy Unity, chưa nghe âm tổng hợp, chưa đo hiệu năng.

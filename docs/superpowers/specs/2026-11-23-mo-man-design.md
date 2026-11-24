# Thiết kế: mở màn và giọng điệu (phần 2/4)

Trạng thái: **chờ chủ dự án duyệt**. Chưa có gì được xây theo bản này.

**Cập nhật 2026-11-24:** phong cách ảnh là low-poly màu trung tính; mô hình 3D của mọi món đã có ở `Assets/_Project/Art/Items/LowPoly/`; áo đấu cũ của đội nhại (R07) đã dựng, huy hiệu là khuôn mặt sừng cách điệu, khác logo thật.

Phần 1 (rương, rác, sưu tập, thần lực) ở [2026-11-23-ruong-suu-tap-design.md](2026-11-23-ruong-suu-tap-design.md). Phần 3 (một ngày trong game, mini-game buổi sáng) và phần 4 (nội dung thần lực chi tiết) làm sau.

## Ý định

Người chơi vào game là hiểu ngay: mình mắc nợ, phải trả 7 kỳ, cách mua hộp để kiếm tiền, và game này **hài**. Mở màn không cần cutscene dài, vì **buổi sáng đầu tiên chính là mở màn**: vừa kể chuyện, vừa dạy luật, vừa giới thiệu mini-game đời thường.

## Mục tiêu

- Kể được vì sao nhân vật mắc nợ trong khoảng 30 giây.
- Dạy luật mua hộp ở ngày 1 mà không cần bảng hướng dẫn dài.
- Đảm bảo ngày 1 không thể thua, nhưng người chơi vẫn thấy may rủi (kể cả ra rác).
- Đặt giọng điệu hài hước phóng đại, ngôn ngữ Gen Z, ngay từ phút đầu.

## Không phải mục tiêu

- Không viết toàn bộ kịch bản và lời thoại cả 7 ngày; bản này chỉ viết mở màn và vài dòng mẫu cho mỗi nhân vật.
- Không thiết kế chi tiết mini-game đánh răng và ăn sáng (phần 3).
- Không đổi luật nợ, giá hay xác suất của game. Hộp ngày 1 được sắp sẵn chỉ cho riêng lần chơi đầu.

## Ràng buộc đã có

- 7 ngày, mỗi ngày kết thúc ở phòng trọ, Anh Bảy thu nợ đúng kỳ vào cuối ngày. Kỳ 1 là 200 xu, tiền đầu ván 1000 xu.
- Mỗi ngày tối đa 3 lượt mua, mỗi lượt 1 hộp mỗi loại, 1 manh mối miễn phí và lời mách của Dì Mận 15 xu.
- Bản chơi thử chỉ có phòng trọ, một khu chợ nhỏ, bàn mở/soi, Dì Mận, Kim Khuy và Anh Bảy. (`Docs/GameDesign/README.md`)
- **Hướng nghệ thuật hiện tại: low-poly, màu trung tính** (bạn đã đổi từ phố đá photoreal, rồi chỉnh từ "tươi sáng" về "trung tính"). Thiết kế thị trấn `2026-11-22-market-town-design.md` (phố đá châu Âu cổ, đèn ấm) viết trước thay đổi này và **chưa được cập nhật**, cần chủ dự án và Antigravity xác nhận.

## 1. Câu chuyện nền

Nhân vật cá độ bóng đá, dồn hết tiền vào **đội mình mê**, đội thua phút cuối nên mất trắng, rồi vay Anh Bảy để "gỡ" và ký giấy nợ 7 kỳ.

- Dùng **đội hư cấu nhại** (đề xuất tên "Quỷ Đỏ United": áo đỏ, huy hiệu quỷ). **Không dùng tên hay huy hiệu câu lạc bộ thật** để tránh rủi ro bản quyền.
- Dịch vụ sòng bạc có sẵn trong game vẫn giữ, nhưng **không** gắn với nguồn gốc món nợ.

## 2. Buổi sáng đầu tiên (3–5 phút)

1. **Hồi tưởng ngắn (20–30 giây):** tối qua xem bóng đá, dồn hết tiền, đội thua phút cuối, cả quán im lặng, vay Anh Bảy, ký giấy nợ. Vài khung hình hài, không phải cutscene dài.
2. **Thức dậy ở phòng trọ**, vẫn cầm tờ vé cược nhàu. Mini-game **đánh răng**, rồi **ăn sáng** (chi tiết ở phần 3).
3. **Anh Bảy gửi giấy nợ:** kỳ 1 là 200 xu, thu cuối ngày. Hình thức: tin nhắn thoại khoa trương kèm giấy nhét qua khe cửa. Đủ để người chơi hiểu luật mà không cần bảng hướng dẫn.
4. **Ra chợ**, Dì Mận dạy mua hộp bằng đoạn thoại ngắn: 3 lượt, 1 hộp mỗi loại, manh mối, giá niêm yết.
5. **Hộp đầu tiên được sắp sẵn:** hộp thứ nhất ra **rác hài** (tốt nhất là áo đấu hoặc khăn cổ vũ cũ của chính nhân vật), hộp thứ hai ra đồ thường bán được. Dạy rằng rương có thể ra rác, mà người chơi vẫn qua ngày 1.
   - **Chốt 2026-11-24:** ngày 1 của lần chơi đầu, cửa hàng **chỉ bán Rương Gỗ và Rương Vải Niêm** (Kim Khí và Đen chưa bán). Số đo ở [Sprint5Ruong](../../../Docs/Balance/Sprint5Ruong.md) mục 3: không ván nào thua trước ngày 3, đường thắng còn (bot thường 8% đến 10%, người chơi giỏi đến 36%). Các ván sau bán đủ như thường.
6. **Cuối ngày về phòng trọ**, Anh Bảy thu 200 xu. Từ ngày 2, mọi thứ ra hoàn toàn ngẫu nhiên.

## 3. Giọng điệu

- Hài hước **phóng đại**, ngôn ngữ **Gen Z** (tiếng lóng mạng, meme, viết tắt), thoại ngắn như tin nhắn và bình luận.
- Phóng đại ở **lời thoại, biểu cảm, tình huống**, không phải ở hình ảnh: hình ảnh giữ low-poly màu trung tính.
- Chuyện nhỏ kể như đại sự (đôi tất thối thành "vũ khí sinh học", thua cược thành "bi kịch quốc gia").
- Tiếng lóng lỗi thời nhanh, nên **toàn bộ lời thoại nằm trong file dữ liệu riêng** (ngoài code) để thay lóng mà không đụng game.

## 4. Nhân vật (đề xuất)

| Nhân vật | Vai và giọng |
|---|---|
| **Người chơi** | Không nói thành tiếng, có độc thoại nội tâm hiện chữ kiểu meme, luôn tự biện hộ khoa trương ("không phải thua, là đóng góp cho tương lai Quỷ Đỏ United") |
| **Anh Bảy** | **Giang hồ mạng "trẻ trâu"** kiểu Việt Nam, nhại hình tượng quen thuộc trên mạng: ảnh đại diện ngầu lòi, nick dài kèm biểu tượng lửa và rồng, tự xưng "đại ca". Đòi nợ bằng tin nhắn thoại và status khoa trương, nghiêm túc với màn "dằn mặt" nhưng thực ra khá nhát, hay bị phản dame hài hước, vẫn lịch sự bất đắc dĩ khi thu tiền đúng kỳ. **Nhân vật hư cấu nhại, không ám chỉ người thật.** |
| **Dì Mận** | Bình luận từng hộp như bình luận viên bóng đá mạng, hay hét "GÓC CHẾT!" mỗi lần mở hộp. Gắn thẳng vào mạch cá độ |
| **Kim Khuy** | Nghiêm túc tuyệt đối với món đồ ngớ ngẩn, nói về ốc vít như chuyên gia review |

Nhân vật Anh Bảy là **đe dọa vui**, không đe dọa thật: không có bạo lực, không có nội dung gợi cảm giác bị đe dọa nghiêm trọng, để tránh lệch tone và vấn đề nội dung.

## Câu hỏi còn mở

1. Tên chính thức của đội nhại ("Quỷ Đỏ United" là đề xuất) và tên nick của Anh Bảy.
2. Độ dài cutscene hồi tưởng cuối cùng: 20–30 giây là ước lượng, cần xem khi có hình.
3. Cách dạy luật ở bước 4 do Dì Mận nói hoàn toàn, hay có thêm gợi ý trên màn hình (Antigravity).
4. Dữ liệu lời thoại: cần chọn định dạng và đặt ở đâu trong `Assets/_Project/` hoặc `Docs/Content/`, trao đổi với Codex.
5. Hộp ngày 1 sắp sẵn cần cờ "kịch bản" trong dữ liệu hộp, trao đổi với Codex.

# Thiết kế: thần lực của đồ cổ (phần 4/4)

Trạng thái: **chờ chủ dự án duyệt**. Chưa có gì được xây theo bản này.

Phần 1: [2026-11-23-ruong-suu-tap-design.md](2026-11-23-ruong-suu-tap-design.md) (mục 5 đã chốt nguyên tắc thần lực). Phần 2: [2026-11-23-mo-man-design.md](2026-11-23-mo-man-design.md) (giọng điệu Gen Z). Phần 3: [2026-11-23-mot-ngay-design.md](2026-11-23-mot-ngay-design.md).

## Ý định

Làm cho đồ quý giá có hồn: mở ra một món đồ cổ là thấy nó có tính cách, biết nói, biết phản ứng, chứ không chỉ là một con số giá. Thần lực chỉ là **tương tác thú vị**, không phải sức mạnh chơi.

## Mục tiêu

- Một khung chung để mọi món đồ có thần lực theo cùng một cách, dễ thêm món mới.
- Phản ứng cho **10 hiện vật V01–V10** viết đầy đủ làm mẫu giọng.
- Phản ứng gồm **lời thoại hiện chữ kèm hiệu ứng và âm thanh**.

## Không phải mục tiêu

- **Không đổi tiền, giá, xác suất, nợ hay luật thắng thua.**
- **Không tiết lộ thông tin có giá trị chơi:** lời thoại và hiệu ứng **không được cho biết** món đồ thật hay giả, tình trạng, hay giá. Dấu hiệu soi thật sự (từ `content.json`) vẫn là nguồn duy nhất để người chơi suy đoán. Món giả cũng có thần lực như món thật, để thần lực không thành đường tắt.
- Chưa viết thần lực cho tạo tác, gia truyền, Bí Khế, hộp và rác: làm sau theo khung này.

## Ràng buộc đã có

- Giọng điệu hài hước phóng đại, ngôn ngữ Gen Z, lời thoại nằm trong file dữ liệu riêng (phần 2).
- Cầm và soi bằng `HeldItemViewmodel`: F soi, Q cất (đã làm, chưa chạy thử trong Unity).
- Dấu hiệu soi của V01–V10 nằm trong `Docs/Content/data/content.json`, trường `inspect_signs`.

## 1. Khung chung

Mỗi món có **một nhân cách** (giọng nói riêng) và tối đa **ba loại phản ứng**:

| Loại | Khi nào xảy ra | Thể hiện |
|---|---|---|
| **Khi cầm** | Vừa nhặt món đồ lên | Một câu thoại ngắn + hiệu ứng/âm thanh nhỏ |
| **Khi soi** | Mỗi lần bấm F, ở một tư thế soi hoặc dấu hiệu soi | Câu thoại hợp với dấu hiệu đó, hiệu ứng riêng |
| **Ngoài đời thật** | Hiếm, theo điều kiện (nơi, NPC gần, vật đang có) | Sự kiện nhỏ trong cảnh chợ (chuông rung, mèo phản ứng, đèn chớp) |

**Quy tắc viết**
- Mỗi món **đúng một nhân cách**, câu ngắn như tin nhắn, tối đa khoảng 12 từ mỗi câu.
- Mỗi loại phản ứng có **2–3 biến thể** để không lặp lại khi cầm nhiều lần; chọn theo hạt giống ván, không ngẫu nhiên thật, để lưu và tải lại không đổi.
- Hiệu ứng nhẹ, hợp chất phố đá cổ kỳ ảo: ánh sáng ấm, rung nhẹ, âm thanh ngắn. **Không** hiệu ứng lòe loẹt.
- Không câu thoại nào ám chỉ thật/giả, tình trạng hay giá.
- Sự kiện ngoài đời xác suất thấp, chỉ xảy ra tối đa một lần mỗi món mỗi ngày.

## 2. Mười hiện vật mẫu

| ID | Món | Nhân cách | Khi cầm | Khi soi | Ngoài đời (hiếm) |
|---|---|---|---|---|---|
| V01 | Bình sứ men xanh | Cụ già mê thơ, hay làm màu | "Ôi, đã lâu rồi chưa ai nâng niu ta như vậy..." Âm chuông gốm khe khẽ | Soi miệng bình: ngâm vần thơ nửa chừng. Soi đáy: "Đừng nhìn đáy, ngại lắm." | Đặt cạnh gương, hai món cãi nhau ai đẹp hơn |
| V02 | Sách cổ bìa da | Học bá mọt sách, hay đính chính | "Cẩn thận gáy sách nha, mình mỏng manh lắm." Tiếng lật trang | Soi chữ mờ: "Mục lục mà, chưa mất đâu, chỉ là hơi... mờ thôi." | Bụi bay thành một dòng chữ lúc mở |
| V03 | Đồng hồ quả lắc cũ | Người trực ca, kiêu "chuẩn từng giây" | "Giờ nào rồi? À, giờ phải trả nợ." Tiếng tích tắc | Soi kim và mặt kính: tự khen đúng giờ dù nó chạy chậm | Chuông đồng hồ điểm lúc Anh Bảy nhắn tin |
| V04 | Mảnh kính đồng hồ thiên văn | Nhà thiên văn bị vỡ, buồn nhẹ | "Mình từng nhìn thấy cả bầu trời đó." Ánh lấp lánh như sao | Soi khung số và mép kính: kể chuyện vỡ như một vụ kiện | Phản chiếu một ngôi sao khi đêm xuống |
| V05 | Thiết bị liên lạc hỏng | Người máy đứt sóng, nhây | "A lô? Nghe rõ không? Chắc là không." Tiếng nhiễu sóng | Soi màn hình vỡ và mã máy: tự báo lỗi hài | Tự bắt được một tin nhắn lạ trong chợ |
| V06 | Bộ pin lượng tử | Dân gym tương lai, khoe năng lượng | "Pin 100% nha, tin mình đi." Ánh cyan nhịp thở | Soi nhãn và linh kiện: khoe cơ bắp điện | Đèn quầy gần đó nhấp nháy theo nhịp |
| V07 | Tinh thể lam cầm tay | Ngoài hành tinh dễ thương, ngây ngô | "Xin chào, loài người! Cho xem nhà." Ánh xanh dịu | Soi khi xoay: sáng yếu đúng như dấu hiệu, thì thầm tiếng lạ | Làm chuông chợ rung và mèo ngẩng đầu |
| V08 | Gương thủy tinh cổ tích | Nữ hoàng kiêu, hay phán xét | "Ồ, ai đây? Hơi mệt mỏi nhỉ." Ánh bạc lướt qua | Soi góc lệch: chê bạn "đứng sai tư thế rồi" | Đặt cạnh bình sứ thì cãi nhau, hoặc khen Dì Mận |
| V09 | Lá bùa giấy vàng | Thầy bói mạng, mê tín và thân thiện | "Hôm nay hợp tuổi mua hộp thứ hai đó." (**lời bói không ảnh hưởng game**) Tiếng giấy sột soạt | Soi mực và nét chữ: phán một câu mơ hồ, đúng kiểu bói | Chữ đỏ trên bùa đổi nét nhẹ khi có người nói dối |
| V10 | Mảnh vảy long | Rồng đã nghỉ hưu, già dặn, khoe khoang | "Ngày xưa ta phun lửa, giờ chỉ còn cái vảy." Ánh kim đổi màu | Soi vết cắt và ánh kim: kể khoác lác về thời oanh liệt | Làm ấm tay người cầm, hoặc làm mèo tránh xa |

Lưu ý quan trọng:
- Lời bói của V09 là **thuần hài**, không dự đoán và không thay đổi xác suất hộp nào.
- "Chữ đỏ đổi nét khi có người nói dối" chỉ là hiệu ứng ngoài đời, xảy ra ngẫu nhiên và **không** phản ánh sự thật nào.

## 3. Dữ liệu (hướng đi)

- Mỗi món có một bản ghi thần lực: `itemId`, `persona`, `holdLines[]`, `inspectLines{}` (theo dấu hiệu soi hoặc tư thế), `worldEvents[]` (kèm điều kiện), `fx` (tên hiệu ứng và âm thanh).
- Toàn bộ chữ nằm trong **file dữ liệu riêng**, không nằm trong code, để thay lóng Gen Z mà không đụng game.
- Điều kiện `worldEvents` dùng các thông tin đã có: món đang cầm, món khác trong kho, NPC gần, thời điểm trong ngày.

## 4. Nối với các phần khác

- **Phần 1:** sưu tập đủ một bộ mở thêm khoảnh khắc thần lực; nhánh Thần lực của cây phát triển mở thêm câu thoại và sự kiện ngoài đời.
- **Phần 2:** nhân cách của từng món dùng cùng giọng điệu hài hước phóng đại.
- **Phần 3:** thần lực ngoài đời có thể thành chất liệu cho tình huống đời thường (mèo chợ, chuông rung).
- **Cầm và soi:** phản ứng "khi cầm" và "khi soi" gắn vào các tư thế soi của `HeldItemViewmodel`.

## Câu hỏi còn mở

1. Hiệu ứng và âm thanh từng món: cần danh sách hiệu ứng thật (Antigravity) và nguồn âm thanh (chưa có).
2. Có lồng tiếng cho đồ cổ hay chỉ hiện chữ kèm âm thanh ngắn? Đề xuất: chỉ chữ + âm thanh, lồng tiếng để sau.
3. Xác suất chính xác của sự kiện ngoài đời và quy tắc không lặp (đề xuất tối đa một lần mỗi món mỗi ngày): cần chốt cùng Claude 2.
4. Thần lực cho tạo tác, gia truyền, Bí Khế, hộp và rác: làm ở lượt sau theo khung này.
5. Định dạng dữ liệu thần lực và cách kích hoạt điều kiện ngoài đời: trao đổi với Codex trong `Docs/Content/YeuCauDuLieu.md`.

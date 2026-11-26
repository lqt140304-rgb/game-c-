# Bước ngoặt: game hành động cypherpunk (hack and slash)

Ngày: 2026-11-26. Trạng thái: **ĐỀ XUẤT, chờ chủ dự án duyệt.** Chưa việc nào được bắt đầu cho tới khi duyệt.

Mục nào ghi **(quyết định của chủ dự án)** là điều chủ dự án đã nói trong buổi trao đổi. Mục ghi **(giả định của tôi)** là chỗ tôi điền vào, cần được xác nhận hoặc sửa.

## 1. Ý định

Chuyển CHỢ ĐỒ LẠ từ game săn hộp mù và trả nợ sang **game hành động roguelite góc nhìn thứ ba qua vai**, bối cảnh cypherpunk. Chủ dự án muốn game có chiều sâu và nhiều hành động hơn, chơi theo kiểu "hack and slash": vừa chém vừa hack, cơ thể nâng cấp được. **(quyết định của chủ dự án)**

Bản đồ đã đổi sang cypherpunk. Tầng trên là thành phố của robot. Khu ổ chuột dưới đất là nơi người chơi sống. **(quyết định của chủ dự án)**

## 2. Vòng chơi một ngày (quyết định của chủ dự án)

| Buổi | Nơi | Việc người chơi làm |
|---|---|---|
| Sáng | Tầng hầm, cứ điểm dưới lòng đất | Nâng cấp cơ thể, lắp thêm bộ phận công nghệ, chuẩn bị cho đêm |
| Tối | Mặt đất | Chiến đấu với robot, những kẻ đã nô dịch con người |

Chiến lợi phẩm của đêm là vật liệu và linh kiện để nâng cấp ở hầm vào sáng hôm sau.

## 3. Cảm giác chiến đấu (quyết định của chủ dự án)

Kết hợp hai game tham chiếu:
- **Cyberpunk 2077:** chiều sâu build. Bộ phận cơ thể và kỹ năng hack thay đổi cách chơi.
- **Ghostrunner:** nhịp nhanh, gọn. Đòn trúng là đau, né và lướt phải đúng lúc, chết nhanh (cả người chơi lẫn robot).

Nhân vật chạy bằng ba thứ: **chém**, **hack** và **bộ phận cơ thể**.

### Chém (giả định của tôi)
Cận chiến bằng vũ khí hoặc tay cơ khí. Có đòn nhẹ, đòn nặng, né, đỡ đúng lúc và lướt. Đòn đỡ đúng lúc làm lộ sơ hở cho robot.

### Hack (giả định của tôi)
Ngắm vào robot rồi nhấn phím hack: chiếm quyền, làm đứng yên, cho quay sang đánh đồng đội hoặc nổ. Hack tốn năng lượng và có thời gian hồi, nên không thể thay hoàn toàn việc chém.

### Cảm giác đã tay (quyết định của chủ dự án: bắt buộc, ưu tiên cao nhất)

Chiến đấu phải **đã tay**. Cảm giác này quan trọng hơn số lượng kỹ năng. Mỗi đòn và mỗi kỹ năng mới phải qua kiểm tra cảm giác trước khi được tính là xong.

Các kỹ thuật tôi đề xuất (giả định của tôi):
- Hit stop: game khựng 50–100 ms khi đòn trúng, đòn nặng khựng lâu hơn.
- Robot phản ứng rõ khi bị đánh: giật, văng linh kiện, tia lửa, đòn kết liễu cắt hoặc làm vỡ.
- Âm thanh va chạm riêng cho từng loại đòn.
- Rung màn hình và camera vừa phải, không che tầm nhìn qua vai.
- Điều khiển phản hồi tức thì: có input buffer, hủy đòn để né.
- Cửa sổ đỡ đúng lúc ngắn, đỡ đúng thì có hiệu ứng riêng và robot lộ sơ hở.
- Rung tay cầm nếu có.

Hệ quả cho kế hoạch: việc đầu tiên là một **phòng thử chiến đấu** (một nhân vật, một robot, ba đòn) để chỉnh cảm giác trước khi thêm kỹ năng. Cảm giác chỉ kiểm tra được khi chơi, nên mục này nghiệm thu bằng chủ dự án chơi thử, không bằng đọc tài liệu.

### Bộ phận cơ thể (quyết định của chủ dự án: 5 vị trí, 1 lõi và 4 bộ phận nâng cấp)

Có **một lõi** và **bốn bộ phận nâng cấp**. Phần đặt tên là giả định của tôi:

| Vị trí | Loại | Mở ra điều gì |
|---|---|---|
| Lõi | Lõi | Năng lượng, số kỹ năng hack, tốc độ hồi. Nâng lõi là nâng sức mạnh nền của nhân vật |
| Tay | Bộ phận | Vũ khí, combo, lưỡi dao, khiên |
| Chân | Bộ phận | Lướt, nhảy, né, đạp |
| Mắt | Bộ phận | Quét, hack tầm xa, nhìn xuyên vật cản |
| Lưng | Bộ phận | Bộ nguồn, giáp, vũ khí đeo |

Năm vị trí thay thế "4 ô tạo tác" trong spec rương cũ. Con số này cần Claude 2 kiểm tra cân bằng.

### Kỹ năng và nguồn gốc (quyết định của chủ dự án: kết hợp hai nguồn)

- **Bộ phận ở hầm (cố định, có chủ đích):** buổi sáng người chơi chọn lắp bộ phận cho lõi, tay, chân, mắt, lưng. Mỗi bộ phận mang kỹ năng chính.
- **Kỹ năng phụ nhặt ngẫu nhiên giữa trận:** nhặt khi chiến đấu ở mặt đất (từ robot, hòm, trùm phụ). Mỗi ván ra một tổ hợp khác nhau, nên cùng bộ phận vẫn cho build khác.

Khi chết, mất cả bộ phận lẫn kỹ năng phụ, theo luật "chết mất hết" ở mục 6.

Đề xuất của tôi (giả định, cần bạn sửa) để dựng thử trong bản chơi thử:
- **Tay:** lưỡi dao (combo nhanh) và búa cơ khí (đòn nặng, phá giáp).
- **Chân:** lướt xuyên robot và đạp tường bật lại.
- **Mắt:** quét điểm yếu và đánh dấu nhiều mục tiêu để hack cùng lúc.
- **Lưng:** khiên năng lượng và drone phụ.
- **Lõi:** kỹ năng tối thượng khi đầy năng lượng.
- **Kỹ năng phụ nhặt (mẫu):** quá nhiệt (hack làm robot nóng, đòn tiếp theo nổ lan), kéo xích, nổ liên hoàn, hồi năng lượng khi đỡ đúng.
- **Quy tắc ghép:** kỹ năng phụ phải tương tác được với kỹ năng bộ phận, ví dụ hack quá nhiệt rồi dùng xích kéo robot vào đám đông. Nhờ vậy có combo mà người chơi tự khám phá.
- **Mỗi kỹ năng phải có cảm giác riêng** (âm thanh, hiệu ứng, phản hồi), theo mục "Cảm giác đã tay".
- **Kẻ địch ép đổi kỹ năng:** robot miễn nhiễm hack, robot chỉ bị búa phá giáp được, để không có một kỹ năng thắng mọi thứ.

Số lượng cụ thể cho bản chơi thử: mỗi bộ phận 2 lựa chọn, khoảng 4 kỹ năng phụ.

## 4. Hòm trong game mới (quyết định của chủ dự án)

**Bỏ hết rương cũ.** Tạo **hòm mới** kiểu hòm chiến đấu, thiết kế lại cho game này. Rương không còn là lõi game.

Hòm có hai vai trò (giả định của tôi, theo ý "rương thưởng và mục tiêu chiến đấu" chủ dự án nói trước đó):
- **Mục tiêu chiến đấu:** một đám robot hoặc một trùm canh hòm.
- **Hòm thưởng:** hạ được mục tiêu thì mở hòm, phần thưởng là vật liệu hoặc bộ phận cơ thể hiếm.

Mô hình, hiệu ứng mở và âm thanh của các rương cũ **không dùng nữa**. Cần dựng hòm mới. Số loại hòm và hình dạng cần chủ dự án chốt.

## 5. Phạm vi vertical slice (giả định của tôi)

Một bản chơi thử nhỏ nhất cho thấy được vòng chơi:
- Một hầm với bàn nâng cấp, một lõi và bốn bộ phận, mỗi vị trí 2 lựa chọn.
- Một khu mặt đất.
- Ba loại robot, một trùm phụ và một trùm cuối.
- Một loại hòm mới làm mục tiêu và phần thưởng.
- Chơi được: sáng nâng cấp, tối chiến đấu, quay về hầm với phần thưởng, thấy nhân vật mạnh hơn, chết thì làm lại từ đầu.

## 6. Cấu trúc ván chơi (quyết định của chủ dự án)

**Không giới hạn số ngày, chơi kiểu roguelike.** Bỏ nhịp 7 ngày của game cũ.

- Mỗi ván kéo dài đến khi người chơi **chết**.
- **Khi chết, mất hết trang bị và kiếm lại từ đầu.**
- **Có trùm cuối.** Hạ được là thắng ván.
- **Có các trùm phụ** (quyết định của chủ dự án) nằm giữa đường tới trùm cuối.
- Vòng chơi một ngày (mục 2) giữ nguyên: sáng ở hầm nâng cấp, tối lên mặt đất chiến đấu.

Giả định của tôi: độ khó tăng theo số đêm đã sống sót, để người chơi tiến dần tới trùm cuối. Số đêm tới trùm cuối, số trùm phụ và đêm nào gặp trùm phụ cần được chốt.

Vai trò trùm phụ (giả định của tôi): mỗi trùm phụ canh một hòm thưởng. Hạ trùm phụ thì mở hòm và nhận bộ phận hoặc vật liệu hiếm, nên nó là cột mốc nâng sức mạnh trước khi đến trùm cuối. Mỗi trùm phụ nên ép người chơi dùng một kỹ năng khác nhau (ví dụ một trùm phải hack mới đánh được, một trùm phải né và đỡ đúng nhịp), để không lặp lại nhau.

Còn mở: chủ dự án nói chết là mất hết trang bị. Chưa rõ có tiến trình nào giữ lại giữa các ván (ví dụ mở khóa loại bộ phận mới) hay không. Tôi để mặc định là **không có**, cho tới khi chủ dự án nói khác.

## 6A. Hệ thống vũ khí (giả định của tôi, chủ dự án mới nêu chủ đề)

- **Vũ khí chính là bộ phận Tay.** Mỗi loại có bộ đòn riêng: lưỡi dao (combo nhanh, nhẹ), búa cơ khí (đòn nặng, phá giáp), dây xích (kéo robot và kéo mình tới robot).
- **Vũ khí phụ nằm ở bộ phận Lưng:** pháo vai, drone phụ.
- **Mỗi vũ khí mang một nhãn hệ** (nhiệt, điện, xung). Nhãn hệ ghép với kỹ năng hack, ví dụ hack quá nhiệt rồi đòn nhiệt thì nổ lan. Đây là nguồn combo chính.
- **Nâng cấp vũ khí ở hầm** bằng vật liệu nhặt được (mục 2). Vũ khí hiếm rơi từ trùm phụ.
- Khi chết, mất vũ khí như mọi trang bị khác.
- Mỗi vũ khí phải qua kiểm tra "đã tay" riêng, vì cảm giác phụ thuộc nhiều vào loại vũ khí.

Cần chốt: số loại vũ khí cho bản chơi thử (đề xuất 2 loại tay), có vũ khí tầm xa hay không.

## 6B. Trùm phụ và trùm cuối (giả định của tôi)

Trùm là robot cai quản việc nô dịch con người. Mỗi trùm phụ ép người chơi dùng một kỹ năng khác nhau:

| Trùm | Vai trò | Cách đánh |
|---|---|---|
| Quản Đốc Xưởng (phụ) | Giáp nặng, đòn chậm | Phải phá giáp bằng búa hoặc hack làm yếu |
| Người Giám Sát (phụ) | Nhiều drone, nhìn từ xa | Phải quét và hack drone, né radar |
| Kẻ Săn (phụ) | Nhanh, ra đòn dồn | Phải đỡ đúng lúc, né sát |
| Hệ Thống Trung Tâm (cuối) | Nhiều giai đoạn, dùng lại đòn của các trùm phụ | Phải dùng đủ chém, hack, đỡ, phá giáp |

Trùm phụ canh hòm thưởng (mục 4), hạ xong mở hòm và rơi vũ khí hoặc bộ phận hiếm. Tên và số lượng chỉ là tạm. Cần chốt số trùm phụ (đề xuất 3 cho bản đầy đủ, 1 cho bản chơi thử).

## 6C. Thiết kế bản đồ (giả định của tôi)

- **Hầm (cứ điểm):** một không gian cố định, mở rộng theo nâng cấp. Có bàn nâng cấp, nơi gắn bộ phận, nơi chọn khu cho đêm.
- **Mặt đất:** chia **ba vùng**, mỗi vùng kết thúc bằng một trùm phụ, và vùng cuối dẫn tới trùm cuối ở tháp trung tâm.
  1. Khu phố ổ chuột bị chiếm (robot tuần tra yếu, người bị nô dịch làm việc).
  2. Khu công nghiệp (robot nặng, bẫy, máy móc).
  3. Tháp trung tâm (robot mạnh, neon dày đặc, trùm cuối).
- **Mỗi đêm là một chặng** trong vùng: người chơi đi từ điểm vào tới mục tiêu (hòm, trùm phụ hoặc lối ra) rồi chạy về hầm.
- **Kết cấu roguelike:** mỗi chặng ghép từ các khối phố hoặc phòng dựng sẵn theo thứ tự ngẫu nhiên, để mỗi ván khác nhau mà vẫn kiểm soát được cảm giác.
- Người bị nô dịch có thể giải cứu giữa chặng làm mục phụ (thưởng vật liệu). Đây chỉ là đề xuất.

Cần chốt: dựng bản đồ ghép ngẫu nhiên hay thiết kế tay từng chặng.

## 7. Mục tiêu, không phải mục tiêu, phản mục tiêu

- **Mục tiêu:** chiến đấu nhanh và rõ nghĩa. Mỗi bộ phận cơ thể thay đổi cảm giác chơi một cách nhìn thấy được.
- **Không phải mục tiêu:** làm lại hệ rương mù, nợ Anh Bảy, soi lỗi, trả giá, bậc đồ trong rương. Lồng tiếng. Dùng credit dịch vụ trả phí khi chưa được cho phép.
- **Phản mục tiêu (nếu xảy ra thì thất bại dù chạy được):** hack quá mạnh đến mức bỏ qua chém. Kẻ địch lặp lại nhàm. Nhân vật không đổi gì nhìn thấy sau khi nâng cấp. Đòn đánh nhẹ tênh, thiếu phản hồi, dù kỹ năng đa dạng.

## 8. Ràng buộc kỹ thuật

- Unity 6 + URP, low-poly màu trung tính, thêm ánh sáng neon cho cypherpunk (theo hướng nghệ thuật đã chốt).
- Góc nhìn thứ ba qua vai. Mã người chơi góc nhìn thứ nhất (`Player`, `HeldItemViewmodel`) không dùng cho chiến đấu. Cần nhân vật có bộ xương, hoạt ảnh và camera qua vai.
- Nhân vật phải chia theo bộ phận để hiển thị phần gắn thêm.
- Dự án chưa có bản chạy được (xem `TASKS.md`), nên rủi ro tích hợp cao.

## 9. Các hướng đã cân nhắc

| Hướng | Mô tả | Kết quả |
|---|---|---|
| A | Ngày chia hai nửa, sáng săn đồ, tối chợ | Bỏ: chủ dự án muốn đổi cách chơi lớn hơn |
| B | Hack nằm trong chiến đấu thuần hành động | Gần với hướng đã chọn |
| C | Chợ là cứ điểm, hành động là chuyến đi | Gần với hướng đã chọn |
| **Đã chọn** | Hầm là cứ điểm nâng cấp cơ thể, mặt đất là chiến đấu, hòm mới là mục tiêu và thưởng | Chủ dự án chọn |

## 10. Tài liệu bị thay thế nếu duyệt

Sau khi duyệt tôi sẽ đánh dấu "đã thay thế bởi spec này" ở các tài liệu về nợ, soi lỗi, trả giá, bậc đồ, mở màn và một ngày. Bỏ cả các spec rương chủ đề, hiệu ứng mở rương, kịch bản mở rương và mô hình rương cũ.

## 11. Việc để người làm quyết định

- Số kỹ năng phụ cuối cùng, tần suất rơi, và có thể chọn một trong vài kỹ năng hay nhận ngẫu nhiên hoàn toàn.
- Chọn tên và chỉ số cụ thể cho lõi, từng bộ phận cơ thể và từng loại robot.
- Số đêm tới trùm cuối, số trùm phụ, đêm nào gặp trùm phụ.
- Chọn tên khu mặt đất.
- Cách điều khiển chi tiết (phím, tay cầm).

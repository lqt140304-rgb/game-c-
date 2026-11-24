# Thiết kế: soi lỗi và trả giá (phần 5)

Trạng thái: **chờ chủ dự án duyệt**. Chưa có gì được xây theo bản này.

Phần 1: [ruong-suu-tap](2026-11-23-ruong-suu-tap-design.md). Phần 2: [mo-man](2026-11-23-mo-man-design.md). Phần 3: [mot-ngay](2026-11-23-mot-ngay-design.md). Phần 4: [than-luc](2026-11-23-than-luc-design.md).

Bản này **thay thế** cách giám định bằng chữ hiện có trong `Docs/GameDesign/README.md` (mục 2 và 4) bằng hai mini-game: **soi lỗi** và **trả giá**. Khác phần 1–4, đây là **thay đổi vòng chơi cốt lõi** và kéo theo cân bằng kinh tế.

## Định hướng nghệ thuật mới (ghi nhận)

Game chuyển sang **low-poly, màu trung tính** (thay cho photoreal; bạn thử "tươi sáng" rồi chỉnh về trung tính). Hệ quả cho bản này: chi tiết nhỏ mất đi, nên **dấu hiệu phải đọc được bằng hình khối và màu**, và số lỗi mỗi món phải ít và rõ. Mô hình 3D của 35 món đã làm lại theo low-poly (xem phần 6).

## Ý định

Biến việc mở hòm và kiểm tra đồ thành **trò chơi quan sát**: so món đồ với hồ sơ chuẩn, tìm lỗi, đoán thật giả và tình trạng, rồi mang đi bán và thuyết phục thẩm định viên. Người giỏi quan sát được thưởng, người ẩu bị phạt.

## Mục tiêu

- Soi lỗi là mini-game có thắng thua, không chỉ đọc chữ.
- Bằng chứng tìm ở bước soi được dùng ở bước trả giá, nên hai mini-game là một chuỗi.
- Soi sai có hậu quả rõ: bán lỗ hoặc bị phạt.
- Rút gọn cho món rẻ để không phải làm hai mini-game dài mỗi hộp.

## Không phải mục tiêu

- Chưa chốt con số: mức phạt, mức lỗ, phí soi thêm, giá ép (thuộc Claude 2).
- Chưa thiết kế chi tiết giao diện, hình ảnh, âm thanh của hai mini-game (Antigravity).
- Chưa viết kịch bản đối chất cụ thể.

## Ràng buộc đã có

- Ba trục chất lượng: **tình trạng** (Nguyên 50% ×1,0, Xước 35% ×0,7, Hỏng 15% ×0,3), **xác thực** (hàng giả bán còn 20% giá), **độ hiếm**. (`content.json`, mục `rules`.)
- Nợ ngày 1 chỉ 200 xu, vốn đầu ván 1000 xu, tối đa 3 hộp mỗi ngày, ngày 1 không được thua ngay (phần 2).
- Cầm và soi vật bằng `HeldItemViewmodel` (đã làm, chưa chạy thử trong Unity).
- Dịch vụ giám định của Giáo Sư Lẫm đã có (trả phí 15%, tối thiểu 20 xu).

## 1. Độ tin cậy của dấu hiệu

- **Tình trạng nhìn hình luôn đúng.** Nguyên: màu tươi sạch bóng. Xước: vài vệt trầy, mẻ cạnh. Hỏng: nứt lớn, thiếu mảnh, màu xỉn. Thể hiện bằng **lớp phủ chồng lên cùng một mô hình**, không cần mô hình mới.
- **Dấu hiệu xác thực là gợi ý có thể sai.** Món thật cũng có "dấu lạ"; món giả đôi khi trông sạch. Dấu hiệu giả gồm sai màu men, chữ khắc ngược, vật liệu bóng như nhựa, tem hoặc mã sản xuất hàng loạt.
- **Độ hiếm** thể hiện bằng viền sáng hoặc chi tiết quý, nhưng không quyết định thật giả.

## Cách làm biến thể chất lượng (đã chốt)

**Không** tạo từng bản chất lượng (Nguyên, Xước, Hỏng, giả) bằng Meshy vì tốn (~140 lượt, ~4000 credit), không kiểm soát được chỗ lỗi và hình dáng không nhất quán giữa các bản.

- Mỗi món có **một mô hình gốc** (Meshy, low-poly).
- Tình trạng Xước và Hỏng, và dấu hiệu hàng giả, là **lớp phủ và biến đổi màu trong Unity** chồng lên mô hình gốc.
- Điểm soi lỗi đặt ở **vị trí cố định** trên mô hình gốc, nên số lỗi và chỗ lỗi khớp với hồ sơ chữ.
- Meshy chỉ dùng thêm, nếu cần, cho một số bản **gãy hoặc vỡ** của món Hiếm trở lên (vì lớp phủ không biểu diễn được phần bị mất). Chưa quyết, sẽ thử sau.

## 2. Mini-game soi lỗi

1. **Hồ sơ vật (bằng chữ):** tên, màu sắc, hoa văn, chi tiết **chuẩn** của loại đồ đó.
2. Người chơi **cầm xoay** vật và so sánh với hồ sơ để tìm chỗ **khác chuẩn**. Mỗi chỗ khác là một **lỗi**; lỗi có thể thật hoặc **nhiễu**.
3. Mỗi lần soi một điểm tốn **một lượt soi**. Mỗi vật có **số lượt soi giới hạn** (đề xuất 3–4).
4. **Soi thêm phải trả giá** (xu). Hết lượt mà không trả thêm thì phải chọn phân loại với những gì đã thấy.
5. Mỗi lỗi tìm ra thành một **bằng chứng**, lưu lại để dùng ở bước trả giá.
6. **Chọn phân loại:** thật hay giả, và tình trạng Nguyên, Xước hay Hỏng.
7. Có thể trả phí cho Giáo Sư Lẫm để **mua gợi ý** một lỗi, tận dụng dịch vụ có sẵn.

## 3. Hậu quả phân loại

| Thực tế | Người chơi chọn | Kết quả |
|---|---|---|
| Thật | Giả | **Bán lỗ**: chỉ nhận giá hàng giả |
| Giả | Thật | **Bị phạt** khi cửa hàng kiểm tra lại |
| Đúng phân loại | Đúng | Giá đúng, có lợi thế khi trả giá |
| Sai tình trạng | (Nguyên ↔ Xước ↔ Hỏng) | Giá lệch theo hệ số tình trạng, nhỏ hơn mức phạt thật giả |

- **Soi không đủ** (ít bằng chứng) khiến thẩm định viên **ép giá**: ít "đạn" thì khó phản biện.
- **Ngày 1** giảm nhẹ hoặc miễn phạt để người chơi không thua ngay.
- Mức phạt, mức lỗ, giá ép: Claude 2 đo.

## 4. Mini-game trả giá (kiểu Ace Attorney)

Người chơi mang đồ đến **cửa hàng đồ cổ**, thẩm định viên kiểm tra lại. Sau đó đến **màn trả giá** dựa trên đối chất:

- Thẩm định viên đưa ra **nhận định** (ví dụ "đây là hàng giả", "tình trạng xước nặng").
- Người chơi **Hỏi vặn** từng nhận định để lộ mâu thuẫn, hoặc **Đưa bằng chứng** đã thu ở bước soi để phản bác.
- Mỗi lần đưa đúng bằng chứng thì giá chào tăng; đưa sai thì mất uy tín và giá giảm.
- Có vài **vòng** (đề xuất 2–3 vòng mỗi món). Hết vòng thì chốt giá.
- Lời thoại dùng **chữ chạy kiểu light novel** (xem phần 4), giọng Gen Z, thẩm định viên và người chơi đối đáp dí dỏm.

## 5. Rút gọn để không nặng

- **Món thường:** chỉ soi nhanh một lỗi và có **bán nhanh** (giá cố định thấp hơn, không trả giá).
- **Món hiếm, rất hiếm, hoặc nghi giả:** soi đầy đủ và trả giá đầy đủ.
- Người chơi có thể **bỏ qua** trả giá (bán nhanh) bất cứ lúc nào, chấp nhận giá thấp hơn.
- Mỗi ngày chỉ tối đa 3 hộp, nên tổng thời gian soi và trả giá cần khống chế (đề xuất mục tiêu vài phút mỗi ngày).

## 6. Nối với các phần khác

- **Phần 1:** thần lực và rác có thể tham gia: rác không cần soi lỗi (vứt hoặc dùng).
- **Phần 2:** ngày 1 an toàn.
- **Phần 3:** thẩm định viên có thể là nhân vật mới hoặc Giáo Sư Lẫm; tình huống hài tiếp tục.
- **Phần 4:** nhân cách đồ cổ có thể "chen vào" khi soi và trả giá bằng chữ chạy.

## Câu hỏi còn mở

1. Thẩm định viên ở cửa hàng đồ cổ là ai: nhân vật mới hay Giáo Sư Lẫm? Có cần thêm cửa hàng mới, hay dùng quầy giám định hiện có?
2. Giá trị cụ thể: số lượt soi, phí soi thêm, mức phạt giả-thành-thật, mức lỗ thật-thành-giả, mức ép giá: **chờ Claude 2**. Cần mô phỏng để game không quá bấp bênh. *Claude 2 đã đo và đề xuất (2026-11-24, chờ duyệt):* [Sprint5SoiLoi.md](../../../Docs/Balance/Sprint5SoiLoi.md).
3. Lớp phủ tình trạng và biến đổi bản giả trên mô hình low-poly: cách làm cụ thể (shader hay texture) do Antigravity quyết.
4. Có tùy chọn bỏ qua hoàn toàn soi lỗi (chọn nhanh, chấp nhận rủi ro) hay không.
5. Cách lưu bằng chứng và kết quả soi trong dữ liệu ván: trao đổi với Codex trong `Docs/Content/YeuCauDuLieu.md`.

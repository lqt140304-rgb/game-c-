# Thiết kế: rương chủ đề (bổ sung cho phần tier)

Trạng thái: **nháp theo lời chủ dự án, chờ duyệt danh sách chủ đề**. Chưa có gì được xây. Bản này **thay đổi** cách mở khóa Rương Việt Nam và Rương Nhại ở [tier-do-trong-ruong](2026-11-23-tier-do-trong-ruong-design.md) (mục "Rương riêng mở khóa bằng cây phát triển").

## Ý định

Mỗi chủ đề là một **hệ sinh thái riêng**: một rương, một nhóm đồ, một món Huyền thoại của riêng chủ đề đó. Người chơi săn thành tựu để mở rương, rồi theo đuổi món Huyền thoại ở cuối chủ đề.

## Đã chốt với chủ dự án

1. **Chỉ cần thành tựu để mở khóa**, không tốn điểm phát triển.
2. **Sau khi mở khóa, rương xuất hiện trong shop rương vào ngày hôm sau.**
13. **Shop rương mỗi ngày (chốt 2026-11-25):** luôn bày đủ 4 rương cơ bản, cộng **tối đa 4 rương mở khóa hoặc rương chủ đề**. Có nhiều hơn 4 rương đã mở thì mỗi ngày chọn 4 rương, xoay vòng theo hạt giống ván và ngày (lưu và tải lại không đổi). Số đo: [Sprint5RuongMoKhoa](../../../Docs/Balance/Sprint5RuongMoKhoa.md).
3. **Rương Nhại tách ra, không gộp chung.** Nhóm Nhại cũ chia thành các chủ đề rõ ràng.
4. **Mỗi chủ đề có món Huyền thoại riêng**, chỉ ra từ rương chủ đề đó.
5. **Mọi loại rương đều có bộ sưu tập riêng** (cả 4 rương cơ bản, rương mở khóa và rương chủ đề).
6. **Cơ chế mở rương: chọn bậc trước, rồi mới quay ra món** (xem mục dưới).
7. Các chủ đề chủ dự án nêu: **Việt Nam, Wibu, truyền thuyết Châu Âu, mô hình, robot**. **Việt Nam vẫn là một chủ đề.** Danh sách sẽ còn thêm, chủ dự án chưa chọn.
8. **Tiêu chí món Huyền thoại:** là **biểu tượng (icon)** của chủ đề, **có câu chuyện** (thần thoại, truyền thuyết, sự tích), và **người trong nền văn hóa đó ai cũng biết**, đủ tầm ảnh hưởng và phẩm giá để đứng ở bậc cao nhất. Ví dụ mẫu: **Nỏ thần** của Việt Nam (truyện An Dương Vương, người Việt nào cũng biết).
9. **Rương Tổng Hợp** ra ngẫu nhiên các món trong **bộ sưu tập của các rương thường** (4 rương cơ bản), **không** ra món của rương chủ đề. Điều này **thay** ghi chú cũ "Rương Tổng Hợp là rương cho Huyền thoại" ở spec tier.
10. **Rương thường cũng có món Huyền thoại**, nhưng **cơ bản hơn, giá thấp hơn** Huyền thoại của rương chủ đề và rương đặc biệt. Rương thường để **sinh tồn**: mở để kiếm tiền. Đồ ở đây tự phát, không cần nhiều câu chuyện; người chơi tự dệt câu chuyện cho nó. Tiêu chí "biểu tượng, có truyền thuyết" ở mục 8 chỉ áp dụng cho Huyền thoại của rương chủ đề.
11. **Vé số độc đắc giữ ở Huyền thoại** (Huyền thoại của rương thường). Sau này có thể thêm cơ chế mang vé đến **quầy vé số để rút thưởng** (chưa thiết kế).
12. **Bậc Rác dùng chung** cho mọi rương (ngoại lệ). **Các bậc còn lại khóa chặt theo rương:** mỗi món chỉ thuộc **một** rương. Món của chủ đề chỉ ra ở rương chủ đề đó; món của từng rương cơ bản chỉ ra ở rương đó. **Ngoại lệ duy nhất:** Rương Tổng Hợp ra được món của các rương cơ bản (mục 9).

## Cơ chế mở rương (đã chốt)

1. **Bước 1, quay bậc:** mỗi rương có bảng tỉ lệ bậc riêng (Rác, Thường, Hiếm, Rất hiếm, Huyền thoại). Quay ra một bậc.
2. **Bước 2, quay món:** trong bộ sưu tập của **chính rương đó**, lấy các món thuộc bậc vừa quay, quay ra một món.
3. Mỗi món chỉ thuộc bộ sưu tập của một rương (mục 12), trừ Rương Tổng Hợp.
4. Món Huyền thoại riêng của chủ đề chỉ nằm trong bộ sưu tập của rương chủ đề đó.
5. Cả hai bước dùng hạt giống ván, để lưu và tải lại cho kết quả không đổi.

Tỉ lệ bậc của từng rương và trọng số từng món: Claude 2.

## Khung chung của một rương chủ đề

| Mục | Nội dung |
|---|---|
| Ra gì | Ưu tiên đồ của chủ đề, có thể lẫn rác. Món Huyền thoại riêng chỉ ra ở rương này |
| Mở khóa | Một **thành tựu** gắn với chủ đề (đề xuất, chưa chốt từng cái) |
| Xuất hiện | Mở khóa hôm nay thì có trong shop rương **từ ngày mai**; shop bày tối đa 4 rương mở khóa hoặc chủ đề mỗi ngày, xoay vòng (mục 13) |
| Giá, tỉ lệ ra | Claude 2 cân bằng |
| Tác dụng khác | Không buff chơi, giữ nguyên nguyên tắc ở phần 1 |

## Chủ đề dự kiến (chờ duyệt)

| Chủ đề | Rương | Huyền thoại riêng | Thành tựu mở | Đồ có sẵn có thể thuộc về |
|---|---|---|---|---|
| Wibu | Rương Wibu | chưa chọn | chưa chọn | kiếm sáng (X03), quả cầu ngọc (X04), mũ phù thủy (H05)? |
| Truyền thuyết Châu Âu | Rương Truyền thuyết Châu Âu | chưa chọn | chưa chọn | giày thủy tinh (L03), quả táo vàng (L02)? |
| Mô hình | Rương Mô hình | chưa chọn | chưa chọn | chưa có |
| Robot | Rương Robot | chưa chọn | chưa chọn | trái tim robot (X05), phi thuyền mini (X06)? |
| Việt Nam | Rương Việt Nam | chưa chọn | chưa chọn | nỏ thần (L05), gậy sắt (L06), ấn ngọc tỷ (L01), vé số (L04)? |

Dấu "?" là gợi ý ghép đồ đã có mô hình, **chưa chốt**. Món Huyền thoại riêng của từng chủ đề có thể là món sẵn có hoặc món làm mới; mô hình mới sẽ gom vào một lượt Meshy chung để đỡ tốn credit.

## Danh sách rương và món theo bậc (đề xuất, chờ duyệt)

Chủ dự án đồng ý danh sách rương chủ đề gợi ý (2026-11-24). Món có mã (V, H, X, L, R) là **đã có mô hình**; món không mã là **đề xuất mới, chưa có mô hình**. "(nhại)" là bản gợi nhắc, không dùng tên hay logo thật.

### Rác: dùng chung cho mọi rương

**Ngoại lệ của luật khóa chặt:** bậc Rác là **một kho chung**, rương nào quay ra Rác cũng lấy từ kho này. Gồm 9 món: Giày cũ R01, Tất thối R02, Trứng thối R03, Xương cá R04, Dép tổ ong R05, Lon bẹp R06, Áo đấu cũ R07, Bao thuốc rồng R08, Vé cược nhàu R09. Có thể thêm rác sau.

### Rương cơ bản (Huyền thoại cơ bản, rẻ hơn, không cần truyền thuyết)

| Rương | Thường | Hiếm | Rất hiếm | Huyền thoại |
|---|---|---|---|---|
| Gỗ | Bình sứ V01, Đồng hồ quả lắc V03, Lá bùa V09 | Ấm trà đất nứt, Hộp nữ trang gỗ khảm | Tượng Di Lặc gỗ | Vé số độc đắc L04 |
| Vải Niêm | Sách cổ V02, Mảnh kính thiên văn V04 | Tranh cuộn thủy mặc, Con dấu đồng | Mảnh vảy long V10 | Bản đồ cổ vẽ tay |
| Kim Khí (+ tạo tác) | Thiết bị liên lạc V05, Radio bóng đèn | Pin lượng tử V06, Máy ảnh phim cổ | Tinh thể lam V07 | Đồng hồ bỏ túi không bao giờ hết pin |
| Đen | Xúc xắc chì, Bộ bài cũ | Bật lửa mạ vàng, Đồng hồ "xách tay" | Nhẫn kim cương không rõ nguồn | Két sắt chưa ai mở được |

### Rương chủ đề (Huyền thoại là biểu tượng, có câu chuyện)

**Chốt với chủ dự án:** món phải **mang tính biểu tượng** của chủ đề, người chơi nhìn là nhận ra. Chấp nhận sát bản quyền; chỉ cần **đổi nhỏ** (màu, chi tiết, tên) đủ để không phải bản gốc mà vẫn nhận ra.

| Rương | Thường | Hiếm | Rất hiếm | Huyền thoại |
|---|---|---|---|---|
| Việt Nam | Quạt nan V11, Guốc mộc | Cassette H01, Nón lá H02, Bánh chưng Lang Liêu | Đàn thần X01, Trống đồng X02 | Ấn Ngọc Tỷ L01, Nỏ thần L05, Gậy sắt làng Gióng L06, Gươm Thuận Thiên (Hồ Gươm) |
| Tuổi thơ Việt Nam | Bi ve, Tò he | Hộp bút hai tầng, Máy nuôi gà ảo | Đèn ông sao | Truyện tranh thần đồng đất Việt tập 1 (nhại) |
| Wibu | Băng đô làng ninja, Bát mì ramen (nhại Naruto) | Túi thần kỳ H03, Cục sạc chuột H04, Cánh cửa thần kỳ | Quả cầu ngọc X04, Đũa thủy thủ mặt trăng | Quyển sổ tử thần (nhại) |
| Truyền thuyết Châu Âu | Hạt đậu thần, Guồng quay sợi | Gương thần V08, Mũ phù thủy H05 | Sừng kỳ lân, Đũa phép cậu bé phù thủy (nhại) | Táo vàng L02, Giày thủy tinh L03, Chiếc nhẫn duy nhất (nhại), Thanh gươm trong đá |
| Robot | Robot dây cót, Tay robot thiếc | Robot vòm tròn bíp bíp (nhại R2), Chó robot | Trái tim lò phản ứng X05, Đầu mô hình Gundam (nhại) | Mũ giáp đỏ vàng của người sắt (nhại) |
| Không gian | Đá mặt trăng, Thức ăn phi hành gia | Mũ phi hành gia | Phi thuyền mini X06, Kiếm sáng X03, Lá cờ trên mặt trăng | Mặt nạ thở của chúa tể bóng tối (nhại) |
| Game (gộp Mô hình) | Máy game 8-bit V12, Băng game | Nấm đỏ, Khối gạch dấu hỏi (nhại) | Khối kim cương vuông (nhại), Kiếm thần của chàng áo xanh (nhại) | Mũ đỏ chữ M của anh thợ sửa ống nước (nhại) |
| Thể thao | Giày bóng đá, Huy chương | Găng thủ môn, Bóng rổ chữ ký số 23 (nhại) | Áo đấu số 7 (nhại), Giày "biết bay" (nhại) | Cúp vàng thế giới (nhại) |
| Âm nhạc | Đĩa than, Kèn harmonica | Micro cổ, Kính tròn ca sĩ tứ quái (nhại) | Đàn bầu, Cây ghi-ta đập vỡ trên sân khấu | Găng tay lấp lánh một chiếc của ông hoàng nhạc pop (nhại) |
| Phim kinh dị | Nến đen, Búp bê vải | Mặt nạ khúc côn cầu (nhại), Hộp nhạc tự kêu | Búp bê ma (nhại), Bóng bay đỏ (nhại) | Băng video "7 ngày" (nhại) |
| Ai Cập cổ đại | Bọ hung, Giấy cói | Tượng mèo Bastet, Kim tự tháp mini | Mắt Horus, Quan tài xác ướp mini | Mặt nạ vàng pharaoh |
| Hải tặc | La bàn, Đồng xu vàng | Móc tay thuyền trưởng, Cờ đầu lâu | Bản đồ kho báu, Trái ác quỷ (nhại) | Mũ rơm (nhại) |
| Trung Hoa cổ | Quạt lông Gia Cát, Đèn lồng đỏ | Bàn tính, Bình gốm Thanh Hoa | Thanh Long Yển Nguyệt Đao, Vòng kim cô | Gậy Như Ý |
| Nhật Bản cổ | Mèo vẫy tay, Quạt giấy | Mặt nạ quỷ Oni, Búp bê Daruma | Giáp samurai mini, Tranh sóng lừng | Thần kiếm Kusanagi |

**Thay đổi so với bảng chia cũ:** Mô hình gộp vào Game; Kiếm sáng chuyển sang Không gian; Áo đấu cũ và Vé cược thành rác chung. **Số món mới cần mô hình:** khoảng 85. Gom tối đa 8 món mỗi lượt Meshy, ước tính sẽ báo trước khi chạy.

## Hệ quả cần xử lý

- Đồ nhại hiện có (áo đấu, máy game 8-bit, túi thần kỳ, cục sạc chuột, mũ phù thủy, kiếm sáng, quả cầu ngọc) cần phân vào các chủ đề mới.
- Số món Huyền thoại sẽ **tăng**: hiện 8 (2 chưa có mô hình), cộng món riêng của từng chủ đề. Rương Tổng Hợp có thể không còn là nơi duy nhất ra Huyền thoại.
- Cây phát triển: nhánh Rương bỏ nút Rương Nhại, Rương Việt Nam chuyển sang điều kiện thành tựu nếu chủ dự án đồng ý.
- Cần hệ **thành tựu** trong dữ liệu (`id`, `điều kiện`, `rương mở`), và shop rương có danh sách hôm nay (Codex).
- Mô hình 3D cho từng rương chủ đề (Antigravity và Blender): chưa có.

## Câu hỏi còn mở

1. Chủ đề bổ sung ngoài 5 chủ đề trên (chủ dự án sẽ nghĩ thêm).
2. Món Huyền thoại cụ thể của từng chủ đề (theo tiêu chí ở mục 8), và đồ hiện có thuộc chủ đề nào. Vé số đã chốt giữ (mục 11). Cần chia 38 món hiện có vào từng rương (mỗi món một rương), và tách Huyền thoại hiện có thành loại của rương thường và loại của rương chủ đề.
3. Thành tựu mở khóa từng rương.
4. ~~Rương Tổng Hợp còn ra Huyền thoại không~~ Đã chốt ở mục 9. Rương thường có Huyền thoại cơ bản (mục 10).
6. Tỉ lệ và giá: Claude 2.

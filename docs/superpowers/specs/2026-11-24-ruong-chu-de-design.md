# Thiết kế: rương chủ đề (bổ sung cho phần tier)

Trạng thái: **nháp theo lời chủ dự án, chờ duyệt danh sách chủ đề**. Chưa có gì được xây. Bản này **thay đổi** cách mở khóa Rương Việt Nam và Rương Nhại ở [tier-do-trong-ruong](2026-11-23-tier-do-trong-ruong-design.md) (mục "Rương riêng mở khóa bằng cây phát triển").

## Ý định

Mỗi chủ đề là một **hệ sinh thái riêng**: một rương, một nhóm đồ, một món Huyền thoại của riêng chủ đề đó. Người chơi săn thành tựu để mở rương, rồi theo đuổi món Huyền thoại ở cuối chủ đề.

## Đã chốt với chủ dự án

1. **Chỉ cần thành tựu để mở khóa**, không tốn điểm phát triển.
2. **Sau khi mở khóa, rương xuất hiện trong shop rương vào ngày hôm sau.**
3. **Rương Nhại tách ra, không gộp chung.** Nhóm Nhại cũ chia thành các chủ đề rõ ràng.
4. **Mỗi chủ đề có món Huyền thoại riêng**, chỉ ra từ rương chủ đề đó.
5. **Mọi loại rương đều có bộ sưu tập riêng** (cả 4 rương cơ bản, rương mở khóa và rương chủ đề).
6. **Cơ chế mở rương: chọn bậc trước, rồi mới quay ra món** (xem mục dưới).
7. Các chủ đề chủ dự án nêu: **Việt Nam, Wibu, truyền thuyết Châu Âu, mô hình, robot**. **Việt Nam vẫn là một chủ đề.** Danh sách sẽ còn thêm, chủ dự án chưa chọn.
8. **Tiêu chí món Huyền thoại:** là **biểu tượng (icon)** của chủ đề, **có câu chuyện** (thần thoại, truyền thuyết, sự tích), và **người trong nền văn hóa đó ai cũng biết**, đủ tầm ảnh hưởng và phẩm giá để đứng ở bậc cao nhất. Ví dụ mẫu: **Nỏ thần** của Việt Nam (truyện An Dương Vương, người Việt nào cũng biết).
9. **Rương Tổng Hợp** ra ngẫu nhiên các món trong **bộ sưu tập của các rương thường** (4 rương cơ bản), **không** ra món của rương chủ đề. Điều này **thay** ghi chú cũ "Rương Tổng Hợp là rương cho Huyền thoại" ở spec tier.
10. **Rương thường cũng có món Huyền thoại**, nhưng **cơ bản hơn, giá thấp hơn** Huyền thoại của rương chủ đề và rương đặc biệt. Rương thường để **sinh tồn**: mở để kiếm tiền. Đồ ở đây tự phát, không cần nhiều câu chuyện; người chơi tự dệt câu chuyện cho nó. Tiêu chí "biểu tượng, có truyền thuyết" ở mục 8 chỉ áp dụng cho Huyền thoại của rương chủ đề.
11. **Vé số độc đắc giữ ở Huyền thoại** (Huyền thoại của rương thường). Sau này có thể thêm cơ chế mang vé đến **quầy vé số để rút thưởng** (chưa thiết kế).
12. **Bậc Rác có kho chung** cho mọi rương (ngoại lệ); **từ 2026-11-24 mỗi rương chủ đề có thêm rác riêng** (mục 13). **Các bậc còn lại khóa chặt theo rương:** mỗi món chỉ thuộc **một** rương. Món của chủ đề chỉ ra ở rương chủ đề đó; món của từng rương cơ bản chỉ ra ở rương đó. **Ngoại lệ duy nhất:** Rương Tổng Hợp ra được món của các rương cơ bản (mục 9).

13. **Rác theo chủ đề (chốt 2026-11-24, phương án A):** rương chủ đề quay ra Rác thì lấy từ **kho chung cộng rác riêng của chủ đề đó**; rương cơ bản chỉ lấy từ kho chung. Rác riêng mang tính **châm biếm**, không na ná đồ ở bậc khác (trừ cặp nguyên/hư ở mục 14). Danh sách ở mục "Rác riêng của rương chủ đề".
14. **Ghép cặp bản nguyên và bản hư hỏng nặng (chốt 2026-11-24):** một món có thể có **bản nguyên ở bậc cao** và **bản hư hỏng nặng ở Rác**, là hai món riêng, hai mô hình riêng, cùng một rương. Đây là **ngoại lệ** của quy tắc "biến thể chất lượng là lớp phủ, không làm mô hình riêng" ở [soi-loi-tra-gia](2026-11-23-soi-loi-tra-gia-design.md), chỉ dùng khi vết hư đổi hình khối (gãy, móp, mất mảnh), không dùng cho hư chỉ đổi màu.

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
| Xuất hiện | Mở khóa hôm nay thì có trong shop rương **từ ngày mai** |
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

### Rác: kho chung cho mọi rương

**Ngoại lệ của luật khóa chặt:** bậc Rác có **một kho chung**, rương nào quay ra Rác cũng lấy từ kho này. Gồm 9 món: Giày cũ R01, Tất thối R02, Trứng thối R03, Xương cá R04, Dép tổ ong R05, Lon bẹp R06, Áo đấu cũ R07, Bao thuốc rồng R08, Vé cược nhàu R09. Có thể thêm rác sau. Rương chủ đề có thêm rác riêng, xem mục dưới.

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
| Việt Nam | Quạt nan V11, Guốc mộc, Sổ hộ khẩu giấy | Cassette H01, Nón lá H02, Bánh chưng Lang Liêu | Đàn thần X01, Trống đồng X02 | Ấn Ngọc Tỷ L01, Nỏ thần L05, Gậy sắt làng Gióng L06, Gươm Thuận Thiên (Hồ Gươm) |
| Tuổi thơ Việt Nam | Bi ve, Tò he | Hộp bút hai tầng, Máy nuôi gà ảo | Đèn ông sao | Truyện tranh thần đồng đất Việt tập 1 (nhại) |
| Wibu | Băng đô làng ninja, Bát mì ramen (nhại Naruto) | Túi thần kỳ H03, Cục sạc chuột H04, Cánh cửa thần kỳ | Quả cầu ngọc X04, Đũa thủy thủ mặt trăng | Quyển sổ tử thần (nhại) |
| Truyền thuyết Châu Âu | Hạt đậu thần, Guồng quay sợi | Gương thần V08, Mũ phù thủy H05 | Sừng kỳ lân, Đũa phép cậu bé phù thủy (nhại) | Táo vàng L02, Giày thủy tinh L03, Chiếc nhẫn duy nhất (nhại), Thanh gươm trong đá |
| Robot | Robot dây cót, Tay robot thiếc | Robot vòm tròn bíp bíp (nhại R2), Chó robot | Trái tim lò phản ứng X05, Đầu mô hình Gundam (nhại) | Mũ giáp đỏ vàng của người sắt (nhại) |
| Không gian | Đá mặt trăng, Thức ăn phi hành gia | Mũ phi hành gia, Ảnh Polaroid "người ngoài hành tinh", Mảnh vệ tinh thật | Phi thuyền mini X06, Kiếm sáng X03, Lá cờ trên mặt trăng | Mặt nạ thở của chúa tể bóng tối (nhại) |
| Game (gộp Mô hình) | Máy game 8-bit V12, Băng game, Tay cầm game kiểu cũ | Nấm đỏ, Khối gạch dấu hỏi (nhại) | Khối kim cương vuông (nhại), Kiếm thần của chàng áo xanh (nhại) | Mũ đỏ chữ M của anh thợ sửa ống nước (nhại) |
| Thể thao | Giày bóng đá, Huy chương | Găng thủ môn, Bóng rổ chữ ký số 23 (nhại), Băng đội trưởng có chữ ký | Áo đấu số 7 (nhại), Giày "biết bay" (nhại) | Cúp vàng thế giới (nhại) |
| Âm nhạc | Đĩa than, Kèn harmonica, Lightstick | Micro cổ, Kính tròn ca sĩ tứ quái (nhại) | Đàn bầu, Cây ghi-ta đập vỡ trên sân khấu | Găng tay lấp lánh một chiếc của ông hoàng nhạc pop (nhại) |
| Phim kinh dị | Nến đen, Búp bê vải | Mặt nạ khúc côn cầu (nhại), Hộp nhạc tự kêu, Bàn cầu cơ nguyên vẹn | Búp bê ma (nhại), Bóng bay đỏ (nhại) | Băng video "7 ngày" (nhại) |
| Ai Cập cổ đại | Bọ hung, Giấy cói, Tượng nhân sư bằng đá | Tượng mèo Bastet, Kim tự tháp mini | Mắt Horus, Quan tài xác ướp mini | Mặt nạ vàng pharaoh |
| Hải tặc | La bàn, Đồng xu vàng | Móc tay thuyền trưởng, Cờ đầu lâu, Chân gỗ thuyền trưởng | Bản đồ kho báu, Trái ác quỷ (nhại) | Mũ rơm (nhại) |
| Trung Hoa cổ | Quạt lông Gia Cát, Đèn lồng đỏ, Đũa đỏ đầu mạ vàng | Bàn tính, Bình gốm Thanh Hoa | Thanh Long Yển Nguyệt Đao, Vòng kim cô | Gậy Như Ý |
| Nhật Bản cổ | Mèo vẫy tay, Quạt giấy, Hộp bento sơn mài, Ô giấy dầu | Mặt nạ quỷ Oni, Búp bê Daruma | Giáp samurai mini, Tranh sóng lừng | Thần kiếm Kusanagi |

**Thêm 2026-11-24 (chủ dự án duyệt):** 7 món Thường (Sổ hộ khẩu giấy, Tay cầm game kiểu cũ, Lightstick, Tượng nhân sư bằng đá, Đũa đỏ đầu mạ vàng, Hộp bento sơn mài, Ô giấy dầu) và 5 món Hiếm (Ảnh Polaroid "người ngoài hành tinh", Mảnh vệ tinh thật, Băng đội trưởng có chữ ký, Bàn cầu cơ nguyên vẹn, Chân gỗ thuyền trưởng), lên bậc từ ý tưởng rác. Chưa có mô hình.

### Rác riêng của rương chủ đề (chốt 2026-11-24)

Chưa có mô hình. "(hư)" là dạng hư hỏng nặng, nhìn là biết đồ bỏ. "(cặp: X)" là bản hư của món X ở bậc cao (mục 14). Món phẳng (tờ, vé, ảnh, quẻ) cho cuộn, gấp hoặc nhàu để Meshy giữ được độ dày; đồ thủy tinh làm bằng gốm.

| Rương | Rác riêng | Hình dáng |
|---|---|---|
| Việt Nam | Mũ bảo hiểm "nồi cơm điện" (hư) | Nhựa mỏng đen bóng, dáng nửa quả bóng, lưỡi trai ngắn, quai dây dù khóa nhựa, sọc caro trắng xám dọc giữa (không dùng họa tiết hãng thật), không lót xốp; lưỡi trai nứt, vỏ trầy, đứt một quai |
| | Biển "Cấm đổ rác" | Tấm tôn trắng chữ đỏ viết tay, gỉ góc, đinh cong, túi nilon đen buộc ở chân biển |
| | Hóa đơn tiền điện tháng 6 | Giấy in nhiệt dài cuộn quăn, số tiền khoanh bút đỏ |
| Tuổi thơ Việt Nam | Que kem đã liếm sạch; Sổ liên lạc chữ ký giả của bố; Vỏ gói bim bim không có quà | Que gỗ dẹt có vết cắn; sổ mỏng bìa xanh, chữ ký nguệch ngoạc; gói nilon sặc sỡ xé toạc, không nhãn hãng |
| Wibu | Gối ôm in waifu bạc màu; Poster anime rách đôi; Figure (hư) | Gối dài ố màu; cuộn giấy rách hai nửa; tượng nhựa trên đế tròn, gãy tay, mất đầu, lem sơn |
| Truyền thuyết Châu Âu | Ếch bông hôn mãi không thành hoàng tử; Mũi Pinocchio gãy; Bát cháo nguội của cô bé tóc vàng | Thú bông đội vương miện giấy méo; que gỗ gãy đôi đầu hồng; bát gỗ thìa cắm đứng |
| Robot | Mớ dây cáp rối; Sạc dự phòng (hư); Bàn phím (hư) | Cáp đen rối nhiều đầu cắm; hộp phồng, cháy sém một góc; bàn phím ngả vàng rụng nhiều phím |
| Không gian | Mũ giấy bạc chống sóng não; Mảnh vệ tinh cháy nát (cặp: Mảnh vệ tinh thật) | Mũ chóp giấy nhôm nhăn; tấm pin mặt trời cháy sém, cong, lòi dây |
| Game | Thẻ nhớ mất save; Đĩa game lậu (hư); Tay cầm đứt dây (cặp: Tay cầm game kiểu cũ) | Thẻ nhựa nhãn viết tay "ĐỪNG XÓA"; đĩa nứt trong hộp vỡ; tay cầm xám dây đứt tưa |
| Thể thao | Vé xem bóng đá giả; Còi méo; Băng đội trưởng giãn chun (cặp: Băng đội trưởng có chữ ký) | Vé in nhòe rách góc; còi kim loại méo dây đỏ; băng vải vàng chữ C chun giãn |
| Âm nhạc | Vé concert bị hủy; Loa kẹo kéo (hư); Lightstick vỡ (cặp: Lightstick) | Vé đóng dấu đỏ "HỦY"; loa vali nhựa đen móp, tay kéo gãy, lưới thủng; que nhựa đầu tròn nứt, nắp pin mất |
| Phim kinh dị | Bộ tóc giả (hư); Chuông gió kêu lúc 3 giờ sáng; Bàn cầu cơ gãy đôi (cặp: Bàn cầu cơ nguyên vẹn) | Tóc đen dài bết rối rụng mảng; chuông kim loại dây rối; bảng gỗ chữ cái gãy đôi |
| Ai Cập cổ đại | Lọ cát sa mạc; Lạc đà bông (hư); Tượng nhân sư thạch cao gãy mũi (cặp: Tượng nhân sư bằng đá) | Lọ gốm nút bần; lạc đà bông xẹp, rách chỉ lòi bông; tượng lưu niệm trắng gãy mũi |
| Hải tặc | Chai rỗng không có thư (hư); Miếng bịt mắt rách; Chân gỗ mọt (cặp: Chân gỗ thuyền trưởng) | Chai gốm xanh sứt miệng bám hà; vải đen có dây rách mép; chân gỗ tiện đầy lỗ mọt |
| Trung Hoa cổ | Bánh bao mốc (hư); Bao lì xì rỗng; Đũa gãy (cặp: Đũa đỏ đầu mạ vàng) | Bánh trắng đốm mốc xanh lõm một bên; bao đỏ chữ vàng mở toang; một chiếc đũa gãy |
| Nhật Bản cổ | Quẻ xăm "đại hung"; Hộp bento nứt (cặp: Hộp bento sơn mài); Ô giấy rách (cặp: Ô giấy dầu) | Giấy dài gấp buộc vào nhánh cây khô; hộp sơn mài nứt nắp lệch; ô giấy đỏ rách xơ, cán tre gãy |

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

# Rà soát trên giấy — CHỢ ĐỒ LẠ

Claude 1 chơi thử **trên giấy**, bằng cách đối chiếu GDD, [nội dung mẫu](NoiDungMau.md) và [gia truyền, tạo tác, Bí Khế](GiaTruyenTaoTacBiKhe.md). Đây **chưa phải** chơi thử bản chạy. Mục Sprint 3 của Claude 1 chỉ nghiệm thu khi có phản hồi từ một ván hoàn chỉnh trong bản chơi.

## 1. Kinh tế nháp của các hộp

Giá trị kỳ vọng tính theo **giá nền** và tỷ lệ rơi nháp, chưa nhân hệ số chất lượng, xác thực và độ hiếm. Claude 2 sẽ định nghĩa các hệ số này.

| Hộp | Giá | Giá nền kỳ vọng | Tỷ số |
| --- | --- | --- | --- |
| H1 Hộp Gỗ Nhỏ | 100 xu | 457,5 xu | 4,6 lần |
| H2 Hộp Kim Loại Mờ | 350 xu | 390 xu | 1,1 lần |
| H3 Hộp Vải Dày | 800 xu | 920 xu | 1,15 lần |
| RD Rương Đen | 1.500 xu | 1.340 xu | 0,89 lần |

## 2. Vấn đề phát hiện

1. **H1 quá lời.** Nếu hệ số trung bình gần 1 thì mua 3 hộp H1 mỗi ngày tốn 300 xu và thu khoảng 1.370 xu, đủ trả nợ cả 7 ngày mà không cần H2 hay H3. Cần hệ số định giá thấp hơn nhiều, hoặc tăng giá H1, hoặc giảm giá nền V01–V03.
2. **RD lỗ ở giá nền.** Rương Đen kỳ vọng thấp hơn giá mua. Cần hệ số độ hiếm đủ lớn hoặc hiệu ứng riêng để người chơi muốn mua.
3. **Manh mối vô dụng sau khi mua.** Bản nháp trước cho xem 2 manh mối còn lại qua giám định, nhưng giám định xảy ra sau khi mua, nên manh mối không còn giúp quyết định. Đã sửa: manh mối thêm mua tại quầy trước khi mua, 30 xu mỗi manh mối.
4. **Hiệu ứng trùng nhau.** Mốc 2 Cổ Phong (giảm phí giám định) giống hệt CP2, nên chọn CP1 và CP2 thì Mốc 2 không thêm gì. TC3 và Mốc 3 Thiên Cơ cùng giảm phạt nợ xuống 10% giống GT1, nên TC3 giá 1.000 xu không đáng mua. Đã sửa bằng cách đổi hiệu ứng.
5. **Quy tắc cộng dồn mơ hồ.** Câu "hiệu ứng cùng loại không cộng dồn" khiến CP1 (+5% giá bán) mất tác dụng khi có CB1 (+10%). Đã sửa: giảm phạt và giảm phí không cộng dồn, tăng giá bán cộng dồn theo phép cộng.
6. **Ca kiểm thử 5 của luật nợ khó hiểu.** Đã sửa để nêu rõ tổng phải trả là 2.250 xu.

## 3. Đã sửa trong nội dung

- Quy tắc cộng dồn và giới hạn 3 manh mối mỗi hộp trong [GiaTruyenTaoTacBiKhe.md](GiaTruyenTaoTacBiKhe.md).
- Đổi Mốc 2 và Mốc 3 của hai hệ thành tăng giá bán, tránh trùng với các tạo tác.
- Đổi TC3 thành miễn phạt (0%) khi nợ tồn không quá 300 xu, khác với GT1 giảm 10% ở mọi mức.
- Chuyển manh mối thêm sang bước xem hộp trước khi mua, đổi GT2, CB2 và Mốc 3 Cổ Phong theo đó.

## 4. Cần người khác quyết định

- **Claude 2:** định nghĩa hệ số giá bán và kiểm tra lại H1, H3 và RD. Chọn giá manh mối 30 xu có hợp lý không.
- **Chủ dự án:** duyệt quy tắc cộng dồn mới và việc đưa manh mối thêm vào bước mua.
- **Cả nhóm:** chơi thử một ván hoàn chỉnh khi có bản chạy, rồi ghi phản hồi về nhịp, độ rõ thông tin và quyết định giữ tiền hay đầu tư.

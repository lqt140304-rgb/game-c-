# Thiết kế đo: cân bằng rương mở khóa và rương chủ đề (Sprint 5 phần 2, Claude 2)

Trạng thái: **cách đo và giả định của Claude 2, chưa có gì chủ dự án chốt**. Kết quả ở [Sprint5RuongMoKhoa.md](../../../Docs/Balance/Sprint5RuongMoKhoa.md).

## Mục tiêu

Giá và tỉ lệ ra bậc cho Rương Tổng Hợp, Cổ Phong, Thiên Cơ, Rác Quý và một **khung chung** cho rương chủ đề (Việt Nam, Wibu, Truyền thuyết Châu Âu, Robot...), gồm tỉ lệ Huyền thoại chủ đề và giá nền Huyền thoại chủ đề (đắt hơn Huyền thoại cơ bản 40 đến 70 triệu đ).

Tiêu chí (rương mở khóa không buff chơi): **thêm lựa chọn và đánh đổi, không làm game dễ hơn rõ rệt.**

- Bot thường (mua rương đắt nhất mua được) vẫn khoảng 15% đến 20% khi rương mới có trong shop.
- Người chơi tính toán hoàn hảo không quá 47%.
- Không rương nào áp đảo lượt chọn (không quá 50%).
- Mỗi rương mới có lý do để mua: người chơi giỏi chọn nó ở một tỉ lệ đáng kể, hoặc nó có vai riêng (giá rẻ, Huyền thoại, tạo tác theo hệ).

## Cách đo (không đổi lõi đã duyệt)

`QA/chest_unlock.py` nạp bảng 4 rương cơ bản của phần 1 (`chest_sim.FINAL`, giữ nguyên) rồi **thêm** rương mới (mã U1, U2...) vào cùng lõi mô phỏng. Hai thay đổi duy nhất ở lõi: thứ tự rương của bot và danh sách rương người chơi giỏi xem mỗi lượt lấy theo các rương đang có trong shop (xếp theo giá gốc giảm dần) thay vì 4 mã cố định.

## Giả định (spec chưa có, ghi thành câu hỏi mở)

1. **Shop:** rương mở khóa có từ ngày 1 của các ván sau (đã mở ở ván trước); ván mở khóa thì có từ ngày hôm sau. Đo cả hai: có từ ngày 1 (khó giữ cân bằng nhất) và từ ngày 3.
2. **Số rương trong shop:** spec chưa nói shop hiện bao nhiêu rương một ngày. Đo các kịch bản 4 cơ bản + 1, + 2, + tất cả rương mở khóa, + 1 đến 3 rương chủ đề.
3. **Bộ sưu tập của Cổ Phong, Thiên Cơ, Rác Quý** chưa có món: dùng món giữ chỗ có giá bằng giá trung bình bậc của rương cơ bản.
4. **Cổ Phong / Thiên Cơ** ra tạo tác của đúng hệ đó (luật trùng như Kim Khí: đổi 50% giá Kho).
5. **Rương chủ đề:** đồ chủ đề cùng một nhóm nguồn gốc (nhóm của chủ đề), nên manh mối chỉ phân biệt được rác với đồ chủ đề.
6. **Rương Tổng Hợp:** quay bậc, rồi quay đều một món trong hợp các bộ sưu tập của 4 rương cơ bản ở bậc đó (ruong-chu-de mục 9).

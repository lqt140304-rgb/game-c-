# Quy trình Meshy: ảnh gộp 8 món → tách mô hình

Tốn ~9 credit (ảnh) + ~30 credit (3D) mỗi lượt. Không chạy khi chủ dự án chưa cho phép. Proxy cloud tự gắn khóa Meshy.

Chạy trong thư mục làm việc tạm (scratchpad), không trong repo:

1. Ảnh gộp: `python3 Tools/Meshy/gen_img.py out.png "1) ..., 2) ..., ... 8) ..."` (8 món, hàng trên trái→phải rồi hàng dưới). Món nhỏ (rác) gộp được **12 hoặc 14 món** một lượt cùng giá: thêm tham số `12` (lưới 4×3, ảnh 4:3) hoặc `14` (lưới 5×3, ảnh 16:9) ở cuối. Xem ảnh; có đường kẻ ô (mỗi ô nền một tông) thì chạy `python3 Tools/Meshy/flatten.py vào.png ra.png <số cột> <số hàng>` (làm phẳng nền từng ô, tô mờ đường nối) trước khi dựng 3D. Ảnh ra kiểu tranh vẽ có nét viền, nhiều món phẳng thì tạo lại ảnh (9 credit) thay vì dựng 3D: Meshy có thể mất gần hết món. Món bị nhân đôi thì tô nền đè bản thừa trước khi dựng.
2. Dựng 3D low-poly: `python3 Tools/Meshy/img_to_3d.py out.png TAG` → `TAG_all.glb` (30 nghìn tam giác, như các bậc khác).
3. Liệt kê mảnh: `python3 Tools/Meshy/comps.py TAG` (cần `pip install trimesh scipy`). Mảnh lớn = món; thường x tăng theo cột, z âm là hàng trên, nhưng Meshy có lúc xếp lại vị trí hoặc dựng hai hàng đứng: xuất mọi mảnh ra rồi xem 4 góc để nhận diện.
4. Xuất từng món: `python3 Tools/Meshy/export.py TAG '{"<mảnh>":["X07_ten",kich_thuoc_m],...}'` → `items_ht/*.glb` (tâm giữa, đáy y=0, cạnh dài nhất = kích thước). Món tách mảnh thì ghi khóa `"1+2"` để gộp. Cần `comps.py` cùng thư mục chạy.
5. Xem 4 góc: chép glb vào `preview/glb/`, `npm i three` trong `preview/`, rồi `node preview/shot.js X07_ten ...` → `ht_<id>.png`.

Món lỗi (mất, dính nhau, không nhận ra): **không làm lại riêng lẻ**; ghi vào danh sách làm lại trong `Docs/Art/BanGiao3D.md`, chỉ chạy khi đủ 8 món hoặc không còn món nào khác.

Mẹo: để mỗi món đứng riêng, không hộp, không ma-nơ-canh, không giá đỡ (dễ bị mất hoặc dính). Logo thật trong ảnh nguồn thì xóa (inpaint) trước khi dựng 3D. Kim tuyến, đồ trong suốt không giữ được.

Lỗi màu nhỏ (vệt, nhòe texture): tô lại tại chỗ bằng `Tools/Meshy/retex/` (`python3 fix_helm.py vao.glb ra.glb`, xem `fix_ball.py` làm mẫu), không tốn credit.

## Mẫu tải từ thư viện cộng đồng Meshy (không tốn credit dựng)

Theo trang thư viện của Meshy, mô hình cộng đồng cấp phép **CC0** (dùng thương mại, không cần ghi tên). Nguồn: meshy.ai/tags/* và trung tâm trợ giúp Meshy (kiểm tra 2026-11-24, các trang hướng dẫn chưa khớp nhau). Lưu ý:

- CC0 chỉ phủ quyền của người đăng; mẫu giống nhân vật, logo, thương hiệu thật vẫn vướng bản quyền bên thứ ba. Món nhại vẫn phải đổi nhỏ.
- Xem nhãn giấy phép trên trang từng mẫu trước khi tải; gói miễn phí giới hạn lượt tải.
- Mô hình tự tạo: gói trả phí sở hữu toàn quyền; gói miễn phí là CC BY 4.0 (ghi nguồn Meshy).

Quy trình: chủ dự án tải `.glb` vào `Assets/_Project/Art/Source/meshy_cong_dong/` kèm file `NGUON.md` ghi link trang từng mẫu (bằng chứng giấy phép). Agent chuẩn hóa: giảm poly theo bậc, đặt cỡ thật, đáy y=0, tâm giữa, tô lại màu trung tính (`retex/`), rồi xuất ảnh 4 góc. Hợp nhất cho món Meshy hay làm mất (đồ phẳng, trong suốt).

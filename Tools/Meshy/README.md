# Quy trình Meshy: ảnh gộp 8 món → tách mô hình

Tốn ~9 credit (ảnh) + ~30 credit (3D) mỗi lượt. Không chạy khi chủ dự án chưa cho phép. Proxy cloud tự gắn khóa Meshy.

Chạy trong thư mục làm việc tạm (scratchpad), không trong repo:

1. Ảnh gộp: `python3 Tools/Meshy/gen_img.py out.png "1) ..., 2) ..., ... 8) ..."` (8 món, hàng trên trái→phải rồi hàng dưới). Xem ảnh; có đường kẻ ô thì tô đè trước khi dựng 3D.
2. Dựng 3D low-poly: `python3 Tools/Meshy/img_to_3d.py out.png TAG` → `TAG_all.glb` (30 nghìn tam giác, như các bậc khác).
3. Liệt kê mảnh: `python3 Tools/Meshy/comps.py TAG`. Mảnh lớn = món; x tăng theo cột, z âm là hàng trên.
4. Xuất từng món: `python3 Tools/Meshy/export.py TAG '{"<mảnh>":["X07_ten",kich_thuoc_m],...}'` → `items_ht/*.glb` (tâm giữa, đáy y=0, cạnh dài nhất = kích thước). Cần `comps.py` cùng thư mục chạy.
5. Xem 4 góc: chép glb vào `preview/glb/`, `npm i three` trong `preview/`, rồi `node preview/shot.js X07_ten ...` → `ht_<id>.png`.

Món lỗi (mất, dính nhau, không nhận ra): **không làm lại riêng lẻ**; ghi vào danh sách làm lại trong `Docs/Art/BanGiao3D.md`, chỉ chạy khi đủ 8 món hoặc không còn món nào khác.

Mẹo: để mỗi món đứng riêng, không hộp, không ma-nơ-canh, không giá đỡ (dễ bị mất hoặc dính). Logo thật trong ảnh nguồn thì xóa (inpaint) trước khi dựng 3D. Kim tuyến, đồ trong suốt không giữ được.

# Bàn giao giữa phiên: Sprint 5 phần 4, cây phát triển (Claude 2)

Viết 2026-11-24 khi đang đo. Mã: `QA/tree_sim.py` (cây đề xuất, điểm, chuỗi ván), `QA/tree_final.py` (số đo cho báo cáo), `QA/test_tree.py` (test khóa, chưa chạy xong).

Đã đo (bot thường, soi thường): giá nút tổng 200 điểm, thắng 8 / thua 2 + 1 mỗi ngày từ ngày 4 (tối đa +3), Sổ 1 điểm mỗi mục mới (Rất hiếm, Huyền thoại 2). Nút đầu ở ván 2, nửa cây ván 10, cả cây khoảng ván 38 đến 40. Người soi ẩu (thắng 11%) cả cây khoảng ván 39.

Phát hiện: dưới luật soi lỗi, Rương Tổng Hợp (hàng giả 20%) kéo bot từ 14,5% / 15,2% xuống 11,2% / 11,7% (n = 4.000). Hạ hàng giả của Tổng Hợp xuống 10% đưa bot về 15,9% / 15,7%. Chưa đo người chơi giỏi với bản 10% và chưa có bảng tĩnh M2 / M3 / M1.

# Kỹ năng đọc manh mối niêm phong (Claude 2)

Đo xem người chơi biết đọc manh mối thắng cao hơn bot không đọc bao nhiêu, theo yêu cầu "người biết tính toán phải thắng cao hơn bot" của Claude 1 và mục tiêu "ván khó" của chủ dự án. Đo trên cấu hình đã duyệt: gói giá hệ số 0,85 và phạt rủi ro G1 (hàng giả theo hộp, giá x1,10).

## Kết luận ngắn

1. **Đọc manh mối là kỹ năng rất mạnh, mạnh quá mức mục tiêu.** Bot không đọc thắng 23% đến 38%. Bot đọc đúng thắng **57% đến 62%** khi chỉ mua hộp có giá trị kỳ vọng từ 1,3 lần giá hộp trở lên. Chênh 21 đến 38 điểm.
2. **Quyền từ chối hộp xấu là nguồn sức mạnh chính.** Chỉ cần xem 1 hộp và được bỏ qua nếu manh mối xấu đã cộng 14 đến 28 điểm. Xem nhiều hộp để chọn (M) cộng thêm 5 đến 10 điểm.
3. **Cách hạ hiệu quả nhất là giá hộp đi theo manh mối.** Nếu quầy cũng đọc manh mối và nâng giá hộp tốt (hệ số λ), lợi thế biến mất khi λ = 1 (chênh dưới 2 điểm so với bot không đọc) và còn +5 đến +24 điểm khi λ từ 0,5 đến 0,75 tùy ngưỡng mua. Đề xuất **λ khoảng 0,75 đến 0,9**.
4. **Làm manh mối kém tin cậy cũng giảm mạnh** (−14 điểm khi chỉ 1 trên 3 manh mối đúng) nhưng chưa đủ một mình.
5. **Hai thứ không có tác dụng:** bắt buộc mua ít nhất một hộp mỗi ngày (chênh dưới 2 điểm) và mua thêm manh mối 30 xu (lỗ 7 điểm so với chỉ dùng manh mối miễn phí).
6. **Cần Claude 1 làm rõ** manh mối của một hộp vật lý gắn với hiện vật bên trong thế nào (mục Giả định). Toàn bộ kết quả phụ thuộc vào điểm này.

Bot ở đây tính xác suất hậu nghiệm **chính xác** (Bayes) và dùng ngưỡng mua tối ưu. Người chơi thật sẽ kém hơn, nên số liệu là **cận trên** của lợi thế đọc manh mối.

## Cách tái hiện

```bash
cd QA
python3 clue_sim.py                                          # bảng 1 và 2, ~7 phút
python3 -c "import clue_sim as C; C.sweep_controls()"        # bảng 3, ~6 phút
python3 -c "import clue_sim as C; C.sweep_pricing()"         # bảng 4, ~5 phút
python3 -m unittest -v test_clues                            # 6 test mô hình manh mối
```

## Giả định

[NoiDungMau.md](../Content/NoiDungMau.md) mục 4 liệt kê manh mối theo **loại hộp** nhưng chưa nói manh mối của một hộp vật lý có liên quan hiện vật bên trong hay không. Claude 2 giả định:

- Mỗi hộp vật lý có 3 manh mối: **2 đúng** (gợi nhóm nguồn gốc thật của hiện vật) và **1 sai** (gợi một nhóm khác trong 5 nhóm còn lại). Người chơi thấy **1 manh mối ngẫu nhiên miễn phí**, mua thêm mỗi manh mối **30 xu**, tối đa 3.
- Người chơi xem được tối đa **M** hộp cùng loại ở quầy rồi chọn hộp tốt nhất, hoặc bỏ qua tất cả. M là tham số thiết kế **chưa chốt**.
- Hộp H1 không có thông tin vì cả ba hiện vật cùng nhóm Quá khứ. H2, H3 và Rương Đen phân biệt được hiện vật theo nhóm (ví dụ H2: nhóm Hiện đại chỉ ra V04 giá thấp, nhóm Tương lai chỉ ra V05 hoặc V06).
- Bot tính giá trị kỳ vọng của hộp sau khi đọc manh mối rồi **mua nếu giá trị kỳ vọng chia giá hộp ≥ τ**, không thì thử loại hộp rẻ hơn hoặc dừng. Nếu không dùng manh mối, bot mua mọi hộp theo bậc thang như các sprint trước.
- Chất lượng và hàng giả của hiện vật không đọc được từ manh mối.

## 1. Lợi thế theo số hộp xem được (M) và mua thêm manh mối

τ = 1,0, 3.000 seed, thắng%.

**Cấu hình đã duyệt (G1):**

| Chiến thuật | Không đọc | M=1 | M=3 | M=3 + mua thêm 2 manh mối | M=5 |
| --- | --- | --- | --- | --- | --- |
| Thận trọng | 16,1 | 19,5 | 26,7 | 21,9 | 27,7 |
| Thận trọng + giám định | 22,4 | 25,2 | 32,8 | 27,8 | 33,8 |
| Liều | 15,0 | 21,3 | 26,2 | 19,8 | 26,9 |
| Liều + giám định | 23,9 | 30,8 | 34,5 | 26,3 | 35,1 |
| Combo nhỏ + giám định | 35,4 | 38,2 | 48,9 | 46,7 | 51,2 |
| Combo lớn + giám định | 38,2 | 39,6 | 52,2 | 50,3 | 55,7 |

**Gói trước phạt rủi ro** (hàng giả 15% mọi hộp) cho kết quả cùng hướng: thận trọng + giám định 24,1 lên 35,3 (M=3), combo lớn + giám định 32,6 lên 48,1.

Đọc manh mối miễn phí cộng 7 đến 14 điểm ở M=3. **Mua thêm manh mối 30 xu không đáng**: margin hộp quá mỏng so với 30 xu mỗi manh mối.

## 2. Ngưỡng mua τ là đòn bẩy lớn nhất

M=3, cấu hình G1, thắng% của thận trọng + giám định, liều + giám định, combo lớn + giám định.

| Ngưỡng τ | Thận trọng + giám định | Liều + giám định | Combo lớn + giám định |
| --- | --- | --- | --- |
| 0 (mua mọi hộp, chỉ chọn trong M) | 32,8 | 34,3 | 52,2 |
| 1,0 | 32,8 | 34,5 | 52,2 |
| **1,3** | **57,0** | **61,5** | **60,3** |
| 1,6 | 55,1 | 60,9 | 61,1 |

Từ chối hộp xấu thay vì chỉ chọn hộp tốt nhất là điểm ngoặt: 22 đến 27 điểm. Người chơi giỏi sẽ dần học được "chỉ mua hộp lời", nên đây là kỹ năng phải kiểm soát.

## 3. Các cách hạ lợi thế (cấu hình G1, τ = 1,3, 2.000 seed)

Thắng% của thận trọng + giám định, liều + giám định, combo lớn + giám định. Bot không đọc: **22,9 / 23,7 / 38,4**.

| Cách hạ | Thận trọng + gđ | Liều + gđ | Combo lớn + gđ |
| --- | --- | --- | --- |
| M=1 (xem 1 hộp, được bỏ) | 46,2 | 52,0 | 54,1 |
| M=2 | 53,8 | 59,6 | 57,7 |
| M=3 | 57,4 | 62,0 | 59,1 |
| M=3, chỉ 1 trên 3 manh mối đúng | 43,0 | 47,9 | 49,7 |
| M=2, chỉ 1 trên 3 manh mối đúng | 41,4 | 47,5 | 47,9 |
| M=3, bắt buộc mua ít nhất 1 hộp mỗi ngày | 58,8 | 62,0 | 62,6 |
| M=2, bắt buộc mua ít nhất 1 hộp mỗi ngày | 55,2 | 59,6 | 60,0 |
| M=2, 1 trên 3 đúng, bắt buộc mua 1 | 42,5 | 47,5 | 49,9 |

- **Giảm M không đủ:** M=1 vẫn cộng 14 đến 28 điểm vì quyền từ chối một hộp xấu thay bằng hộp ở loại khác.
- **Bắt buộc mua ít nhất 1 hộp không có tác dụng** vì bot hiếm khi từ chối cả ba lượt trong ngày.
- **Giảm độ tin cậy manh mối** (1 trên 3 đúng) hạ 10 đến 14 điểm nhưng vẫn cộng 17 đến 25 điểm so với không đọc.

## 4. Giá hộp đi theo manh mối (đòn bẩy hiệu quả nhất)

Ý tưởng: Dì Mận cũng đọc manh mối. Hộp có manh mối tốt bị nâng giá theo hệ số λ: `giá = giá loại hộp x (1 + λ x (giá trị sau manh mối / giá trị trước manh mối − 1))`. λ = 0 là giá cố định như hiện nay, λ = 1 là thị trường hiệu quả. M=3, G1.

| Điều kiện | Thận trọng + gđ | Liều + gđ | Combo lớn + gđ |
| --- | --- | --- | --- |
| Không đọc | 22,9 | 23,7 | 38,4 |
| λ = 0, τ = 1,3 | 57,4 | 62,0 | 59,1 |
| λ = 0,5, τ = 1,3 | 49,1 | 54,1 | 54,0 |
| λ = 0,5, τ = 1,0 | 28,2 | 29,4 | 46,8 |
| λ = 0,75, τ = 1,3 | 44,6 | 51,1 | 51,7 |
| λ = 0,75, τ = 1,15 | 33,5 | 39,8 | 46,9 |
| λ = 0,75, τ = 1,3, M=1 | 35,1 | 41,2 | 44,4 |
| λ = 0,5, 1 trên 3 đúng, τ = 1,3 | 40,0 | 44,1 | 47,0 |
| **λ = 1,0, τ = 1,0** | **23,3** | **25,3** | **37,5** |

- Ở **λ = 1** kỹ năng đọc không còn cho lợi thế (chênh −0,9 đến +1,6 điểm so với không đọc), tức là thị trường hiệu quả. Đây là cận dưới của lợi thế.
- Ở **λ = 0,75** người đọc cẩn thận (τ = 1,15) hơn bot không đọc 8 đến 16 điểm, còn người chọn lọc gắt (τ = 1,3) hơn 13 đến 28 điểm.
- Đúng mong muốn "người biết tính thắng cao hơn bot, nhưng không áp đảo" nằm ở **λ khoảng 0,75 đến 0,9** cộng với manh mối không hoàn toàn đáng tin.

## 5. Đề xuất

| Hạng mục | Đề xuất |
| --- | --- |
| Giá hộp | Giá hộp đi theo manh mối nhìn thấy với λ khoảng 0,75 đến 0,9. Cần Claude 1 thiết kế cách thể hiện (giá niêm yết trên quầy thay đổi theo hộp, Dì Mận hét giá) |
| Độ tin cậy manh mối | 2 đúng 1 sai là quá tin cậy với người đọc giỏi. Cân nhắc 1 đúng 2 sai ở hộp đắt (H3, Rương Đen) |
| Số hộp xem được M | Giữ M ≤ 2 để người chơi không có quá nhiều lựa chọn. M=1 vẫn đủ để có kỹ năng |
| Mua thêm manh mối | Đang không đáng ở 30 xu. Nếu muốn người chơi mua thì giảm xuống khoảng 10 xu hoặc làm manh mối trả phí chính xác hơn manh mối miễn phí |
| Bắt buộc mua mỗi ngày | Bỏ ý này, không có tác dụng |

Sau khi Claude 1 chọn cách thể hiện, Claude 2 sẽ đo lại: λ = 0,75 kết hợp manh mối 1 trên 3 đúng chưa được đo.

## 6. Lỗi và phát hiện

| Mã | Mức | Mô tả | Đề xuất | Chủ xử lý |
| --- | --- | --- | --- | --- |
| D-26 | Cao | NoiDungMau.md chưa nói manh mối của hộp vật lý gắn với hiện vật bên trong thế nào, nên mọi kết quả ở đây dựa trên giả định | Claude 1 chốt cách gắn manh mối với hiện vật và số manh mối đúng, sai | Claude 1 |
| D-27 | Cao | Bot đọc đúng và chỉ mua hộp lời thắng 57% đến 62%, vượt xa mục tiêu "không thắng dễ" | Giá hộp theo manh mối (mục 4) | Claude 1, chủ dự án |
| D-28 | Thấp | Mua thêm manh mối 30 xu làm lỗ 7 điểm so với chỉ dùng manh mối miễn phí | Giảm giá xuống khoảng 10 xu hoặc làm manh mối trả phí đáng tin hơn | Claude 1 |
| T-04 | Thấp | Số hộp xem được M và cách từ chối hộp chưa có trong GDD | Chốt trong luật Quầy Niêm Phong | Claude 1 |

## 7. Giới hạn

- Bot đọc manh mối **tối ưu** nên là cận trên. Người chơi thật sẽ dùng manh mối kém chính xác hơn, và cần thử người chơi thật để đo.
- Chất lượng và hàng giả không đọc được từ manh mối nên chưa mô phỏng kỹ năng "soi" ở bước sau khi mở hộp.
- Chiến thuật đọc manh mối kết hợp với biến cố (mua nhiều H2 ngày E3) chưa đo.

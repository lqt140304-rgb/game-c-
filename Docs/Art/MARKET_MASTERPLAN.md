# MARKET MASTERPLAN — PHỐ DỊ VẬT HÀ THÀNH

**Phương án được duyệt:** **Phương án A — Quảng Trường Trung Tâm & Mạng Ngõ Chuyên Biệt**  
**Quy mô tổng thể:** `150m × 165m` (`X ∈ [-75, +75]`, `Z ∈ [0, 165]`)  
**Hệ trục Unity:** `+X` Đông, `+Z` Bắc, `+Y` hướng lên (đơn vị mét)

---

## 1. Cấu trúc Không gian & Sơ đồ Tổng thể

```
                        [ĐẠI LỘ & NGÃ 3 HÀ NỘI (Z = 116..165m)]
                                           │
                     ┌─────────────────────┴─────────────────────┐
                     │      6. NHÀ ĐẤU GIÁ (Quý Cô Chốt)         │ (0, 96)
                     │      Hội quán Đông Dương 26m × 20m        │ Ưu tiên 4
                     └─────────────────────┬─────────────────────┘
                                           │ (Trục Bắc)
 ┌─────────────────────────┐               │               ┌─────────────────────────┐
 │ 2. KHO TẠO TÁC          │               │               │ 7. NHÀ ĐỎ (Sòng Bạc)    │ (+25, 72)
 │    (Kim Khuy) (-21, 72) │               │               │    (Hẻm kín Đông-Bắc)    │ Ưu tiên 4
 │   15m × 18m (Ưu tiên 2) │               │               │   18m × 18m (Sảnh sâu)  │
 └────────────┬────────────┘               │               └────────────┬────────────┘
              │                            ▼                            │
 ┌────────────┴────────────┐      ┌─────────────────┐      ┌────────────┴────────────┐
 │ 1. NHÀ NIÊM PHONG       │      │   QUẢNG TRƯỜNG  │      │ 3. VIỆN GIÁM ĐỊNH       │ (+22, 44)
 │    (Dì Mận) (-22, 44)   │◄────►│   TRUNG TÂM     │◄────►│    (Giáo Sư Lẫm)        │ Ưu tiên 2
 │  + SẢNH KHUI & SOI HÒM  │      │   (0, 44)       │      │ Biệt thự Pháp 17m × 22m │
 │   18m × 22m (Ưu tiên 1) │      │  (Đài & Cây cổ) │      └────────────┬────────────┘
 └────────────┬────────────┘      └────────┬────────┘                   │ (Lối tắt sân sau)
              │ (Ngõ Tây-Nam)              │               ┌────────────┴────────────┐
 ┌────────────┴────────────┐               │               │ 4. XƯỞNG VÁ VÍU         │ (+26, 18)
 │ 5. TIỆM CẦM CỐ (Bà Cầm) │               │               │    (Chú Rỉ) · Ưu tiên 3 │
 │    (-24, 18) · Ưu tiên 3│               │               │ Nhà kho 16m × 20m       │
 └────────────┬────────────┘               │               └─────────────────────────┘
              └──────────────┐             │
                             ▼             ▼
                     ┌─────────────────────────────┐
                     │  8. SÂN THU NỢ & PHÒNG TRỌ  │ (0, 8)
                     │  (Điểm xuất phát / Trả nợ)  │ Ưu tiên 3
                     └─────────────────────────────┘
```

---

## 2. Bảng Tọa độ, Quy mô & Liên kết Công trình

| Mã | Công trình / Khu vực | Tâm `(X, Z)` | Mặt tiền nhìn hướng | Kích thước `(Sâu × Rộng × Cao)` | Ưu tiên |
| --- | --- | --- | --- | --- | --- |
| **0** | **Quảng Trường Trung Tâm** | `(0, 44)` | Trung tâm tụ điểm | `26m × 28m` (lòng phố & vỉa hè) | **1** |
| **1** | **Nhà Niêm Phong (Dì Mận) + Sảnh Khui Hòm** | `(-22, 45)` | Đông (`+X`) | `18.0m × 22.0m × 11.2m` | **1** |
| **2** | **Kho Tạo Tác (Kim Khuy)** | `(-21, 73)` | Đông (`+X`) | `15.0m × 18.0m × 8.5m` | **2** |
| **3** | **Viện Giám Định (Giáo Sư Lẫm)** | `(+22, 45)` | Tây (`-X`) | `17.0m × 22.0m × 9.8m` | **2** |
| **4** | **Xưởng Vá Víu (Chú Rỉ)** | `(+22, 19)` | Tây (`-X`) | `16.0m × 20.0m × 9.0m` | **3** |
| **5** | **Tiệm Cầm Cố (Bà Cầm)** | `(-22, 19)` | Đông (`+X`) | `18.0m × 12.0m × 9.5m` | **3** |
| **6** | **Nhà Đấu Giá (Quý Cô Chốt)** | `(0, 98)` | Nam (`-Z`) | `20.0m × 26.0m × 12.5m` | **4** |
| **7** | **Nhà Đỏ (Sòng Bạc Ngược Đời)** | `(+23, 73)` | Tây (`-X`, qua hẻm kín) | `18.0m × 18.0m × 8.8m` | **4** |
| **8** | **Phòng Trọ & Sân Thu Nợ (Anh Bảy)** | `(0, 8)` | Bắc (`+Z`) | `14.0m × 12.0m × 4.5m` | **3** |

---

## 3. Luồng Di Chuyển & Góc Nhìn Trọng Điểm (First-Person)

1. **Vòng lặp Cốt lõi (Ưu tiên 1 — `3–8 giây` thao tác):**
   - Xuất phát từ **Sân Phòng Trọ `(0, 12)`** nhìn thẳng dọc trục Bắc: trước mắt là **Quảng Trường Trung Tâm**, bên trái nổi bật mặt tiền 3 tầng vàng đất & biển hiệu đỏ **NHÀ NIÊM PHONG**, đối diện bên phải là biệt thự Pháp kem vàng **VIỆN GIÁM ĐỊNH**, xa cuối trục phố là mái vòm uy nghi của **NHÀ ĐẤU GIÁ**.
   - Vào **Nhà Niêm Phong**: soi 6 bục hộp ở Khu A → mua ở Khu B (Dì Mận) → bước ngay sang cánh Nam thông vòm để khui & soi cổ vật tại **Bàn Mở & Kiểm Tra** (không cần chạy vòng ra đường lớn).
2. **Tuyến Mở rộng (Ưu tiên 2–4):**
   - **Băng ngang Quảng Trường (`24m`):** Mang đồ từ Sảnh Khui Hòm sang thẳng **Viện Giám Định (Giáo Sư Lẫm)** hoặc chếch Tây Bắc vào **Kho Tạo Tác (Kim Khuy)**.
   - **Lối tắt Sân Sau (`Z = 31–33m`, phía Đông):** Từ sân bên Viện Giám Định đi thẳng xuống **Xưởng Vá Víu (Chú Rỉ)** để phục chế đồ hỏng.
   - **Ngõ Cầm Đồ (`Z = 28–32m`, phía Tây):** Rẽ từ Nhà Niêm Phong xuống **Tiệm Cầm Cố (Bà Cầm)** hoặc thẳng về **Sân Thu Nợ** cuối ngày gặp **Anh Bảy & Tí Biên Lai**.
   - **Hẻm Đèn Đỏ (`Z = 60–82m`, Đông Bắc):** Lối vào hẹp, đèn đỏ trầm dẫn vào **Nhà Đỏ** mà không phá vỡ sự trang nghiêm của quảng trường chính.

---

## 4. Hạ tầng Phố & Ngã Ba Ô Tô Phía Bắc
- **Trục phố đi bộ Phố Dị Vật:** `Z = 0..112m`, lòng phố rộng `20–24m` (vỉa hè hai bên rộng `4.2m` nâng cao `+0.09m`, bó vỉa đá xanh).
- **Ngã 3 Đại lộ Hà Nội (Phía Bắc):** `Z = 112..165m`, nối ra mặt đường lớn phía ngoài khu phố cổ dị giới, tạo chân trời đô thị Hà Nội.

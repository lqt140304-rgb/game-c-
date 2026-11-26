# Tài liệu thiết kế — Thị trấn Chợ xuyên thế giới (Greybox Blockout)

**Ngày:** 2026-11-22  
**Dự án:** CHỢ ĐỒ LẠ (`lqt140304-rgb/game-c-`)  
**Phụ trách:** Antigravity (3D & Tích hợp)  
**Trạng thái:** Chờ duyệt thiết kế (trước khi lập kế hoạch triển khai)  
**Cập nhật 2026-11-26:** Chủ dự án đổi phong cách sang **thành phố hiện đại Việt Nam** (hẻm nhỏ, chợ trời đồ cũ, xe máy, bảng hiệu, dây điện). Phần "phố đá kiểu châu Âu cổ" ở mục 2 không còn đúng. Kích thước, bố cục, `ShopSlot` và cách dựng greybox vẫn áp dụng.

---

## 1. Mục đích & Lý do (Intent & Why)

Xây dựng không gian **thị trấn Chợ xuyên thế giới** ở góc nhìn thứ nhất (First-Person) trong **Unity 6.3 LTS (URP)** làm nền tảng cho vòng chơi 7 ngày của *CHỢ ĐỒ LẠ*.  

Thay vì dựng thẳng model 3D chi tiết ngay (dễ sai tỷ lệ đi bộ, sai góc che khuất và khó sửa khi thay asset), giai đoạn này dựng **Modular Greybox Blockout** bằng hệ thống Prefab chuẩn kích thước và Editor Scene Generator. Cách làm này giúp:
- Kiểm tra ngay cảm giác đi bộ, độ dốc cầu thang/cầu vượt, góc che khuất của dãy nhà và tầm nhìn tới các điểm nhấn trong Play Mode.
- Chừa sẵn các ô đất trống (`ShopSlot`) cho từng quầy/cửa hàng sẽ làm ở bước sau mà không làm xáo trộn bố cục thị trấn.
- Dễ dàng thay thế từng khối greybox bằng mesh Blender hoặc Meshy/Hyper3D mà giữ nguyên collider, vị trí và ánh sáng.

---

## 2. Mục tiêu, Ngoài phạm vi & Anti-goals

### Mục tiêu (Goals)
1. **Một style nền thống nhất (80/20):** Toàn bộ thị trấn giữ **phố đá kiểu châu Âu cổ** làm nền chủ đạo (~80%: đường lát đá cuội, tường đá/gạch rêu, khung gỗ tối, mái ngói dốc, ánh đèn lồng vàng ấm). Các chi tiết **đồ cũ, steampunk, sci-fi và phép thuật** (~20%) xuất hiện **rải rác, ngẫu nhiên và tiết chế** trên cùng dãy phố, không chia khu cứng.
2. **Che khuất có chủ đích & Khám phá:** Dãy nhà ở và cổng vòm tiền cảnh che bớt các ngõ rẽ để người chơi bước qua vòm hoặc rẽ góc nhà mới thấy các ô quầy ẩn. Mọi ngõ đều thông vòng lặp lại trục chính (không tạo ngõ cụt gây mệt khi lặp 7 ngày).
3. **Tầng cao & Cầu vượt:** Có cầu thang/dốc đá dẫn lên ban công tầng 2 và cầu vượt đá bắc ngang phố (đi được cả bên dưới gầm vòm lẫn bên trên cầu).
4. **Hệ thống điểm nhấn (Landmarks):** 1 điểm nhấn chính cao vượt mái nhà để định hướng toàn thị trấn + 3 điểm nhấn phụ dẫn mắt vào các ngõ khuất và tầng cao.
5. **Chừa ô cửa hàng (`ShopSlot`):** Đặt sẵn 8 ô đất trống đánh dấu rõ hướng mặt tiền và kích thước ở cả trục chính, hẻm khuất và tầng 2.

### Ngoài phạm vi (Non-goals)
- **Chưa dựng chi tiết các cửa hàng** (nội thất, quầy hàng, biển hiệu riêng của Dì Mận, Kim Khuy, Anh Bảy, Giám định, Phục chế, Đấu giá, Sòng bạc, Cầm cố).
- **Chưa tạo mô hình 3D nhân vật** (do chủ dự án tự tạo bằng Meshy và Hyper3D).
- **Chưa triển khai logic kinh tế, kho đồ, lưu ván** (thuộc phạm vi Codex theo [DataContract.md](../../Docs/Architecture/DataContract.md)).

### Anti-goals (Tránh tuyệt đối)
- Chia khu vực tách biệt theo chủ đề (góc steampunk riêng, góc sci-fi riêng) làm gãy tính thống nhất của thị trấn.
- Thị trấn quá rộng hoặc nhiều ngõ cụt khiến người chơi mất nhiều thời gian chạy bộ mỗi ngày.
- Nhồi quá nhiều hiệu ứng phát sáng neon/phép thuật làm mất chất phố đá châu Âu cổ.

---

## 3. Ràng buộc kỹ thuật & Tỷ lệ First-Person

- **Engine & Pipeline:** Unity 6.3 LTS (`6000.3.x`), URP, Windows PC.
- **Tương thích Player hiện có:** [`FirstPersonController`](../../Assets/_Project/Scripts/Player/FirstPersonController.cs) (`CharacterController` cao `1.8m`, bán kính `0.35m`, tầm mắt camera `~1.55m`).
- **Thông số không gian chuẩn (bắt buộc để đi bộ mượt):**
  - **Quy mô toàn khu chợ:** Gọn trong khoảng **45m × 55m** (chạy từ Phòng trọ tới cuối thị trấn mất ~10–15 giây).
  - **Trục phố chính:** Rộng **5.5m – 6.0m**.
  - **Ngõ phụ / Hẻm vòm:** Rộng **3.2m – 3.5m** (đủ rộng cho First-Person rẽ góc thoải mái).
  - **Cao độ tầng 2 & Cầu vượt:** Mặt sàn tầng 2 ở cao độ **`+3.8m`**, gầm cầu vượt/cổng vòm cao tối thiểu **`3.2m`** so với mặt đường.
  - **Cầu thang / Dốc lên tầng 2:** Độ dốc **`≤ 28°`** hoặc chia bậc nhỏ khớp `stepOffset = 0.35m` (có collider dốc mượt bên dưới để không bị giật camera).
  - **Ô đất trống (`ShopSlot`):** Chuẩn **`4.0m × 3.0m`** (quầy/tiệm vừa) và **`5.0m × 4.0m`** (dịch vụ lớn), kèm mũi tên chỉ hướng mặt tiền (`Forward`).

---

## 4. Các hướng tiếp cận đã cân nhắc (Approaches)

| Hướng tiếp cận | Cách làm | Đánh đổi | Kết luận |
| :--- | :--- | :--- | :--- |
| **Hướng 1: Modular Greybox Blockout + Editor Script** | Xây bộ Prefab modular chuẩn kích thước (tường đá, nhà che, vòm, cầu vượt, điểm nhấn, `ShopSlot`) và script Editor tự sinh scene trong Unity. | Giai đoạn đầu hiển thị bằng khối hình học phân màu, chưa có texture/mesh chi tiết. | **ĐƯỢC CHỌN** — Chuẩn tỷ lệ First-Person, test Play Mode ngay, thay mesh Blender/Meshy vào Prefab rất nhanh ở bước sau. |
| **Hướng 2: ProBuilder thủ công trong Unity** | Nặn trực tiếp từng ngôi nhà, mái vòm bằng ProBuilder trên giao diện Unity Editor. | Phải thao tác tay hoàn toàn trên máy có Unity, khó chuẩn hóa để thay mesh Blender hàng loạt. | Loại — Không tái lập được từ repo và tốn công sửa UV/collider khi lên đồ 3D thật. |
| **Hướng 3: Chờ đủ model 3D mới dựng cảnh** | Chỉ giữ tài liệu mô tả, đợi đủ asset kiến trúc từ Blender/Meshy mới xếp vào scene. | Không kiểm tra được tầm nhìn, góc che khuất và độ dốc cầu thang trong Play Mode từ sớm. | Loại — Rủi ro sai tỷ lệ và chậm nhịp tích hợp với Codex. |

---

## 5. Kiến trúc & Quy hoạch thị trấn

### 5.1. Sơ đồ mặt bằng & Cao độ

```text
        (Vòng hẻm Tây) <-- [ LANDMARK CHÍNH: THÁP / CỔNG XUYÊN THẾ GIỚI ] --> (Vòng hẻm Đông)
        [ShopSlot_07]         (Vòm tầng trệt đi xuyên qua được)
              |                                |
  [Thang Tây ↑] [ShopSlot_08 · T2] == [ CẦU VƯỢT ĐÁ +3.8m ] == [ShopSlot_06 · T2] [Thang Đông ↑]
      (Ngõ khuất Tây)              (Điểm nhấn phụ 1)               (Sân hiên Đông)
      [ShopSlot_05]                        |                              |
      [Nhà che Tây]                  (Trục phố đá)                 [Nhà che Đông]
              |                            |                              |
      (Hẻm vòm Tây) ---> [ OpenTable_Zone 4×4m · ngoài trời ] <--- (Ngõ Giếng Cổ Tự)
      [ShopSlot_03]                                         (Landmark phụ 2) [ShopSlot_04]
              |                            |                              |
      [Nhà che tiền cảnh]    [ShopSlot_01] [ShopSlot_02]       [Cổng vòm Hologram]
                                           |                    (Landmark phụ 3)
                             [ HIÊN PHÒNG TRỌ · Inn_Spawn ]
                              (Spawn đầu/cuối ngày, ngoài trời)
```

### 5.1b. Quyết định đã chốt sau rà soát (2026-11-22)

- **Phòng trọ (`Inn_Spawn`):** Hiên trọ mở ngoài trời trước mặt tiền đóng. Chưa dựng nội thất phòng ở giai đoạn này.
- **Bảng ô cửa hàng:**

  | Ô | Vị trí | Kích thước |
  | --- | --- | --- |
  | `ShopSlot_01`, `ShopSlot_02` | Trục phố chính, gần phòng trọ | 4.0m × 3.0m |
  | `ShopSlot_03` | Hẻm vòm Tây | 4.0m × 3.0m |
  | `ShopSlot_04` | Ngõ Giếng Cổ Tự (Đông) | 5.0m × 4.0m |
  | `ShopSlot_05` | Ngõ khuất Tây | 4.0m × 3.0m |
  | `ShopSlot_06` | Sân hiên tầng 2 phía Đông | 5.0m × 4.0m |
  | `ShopSlot_07` | Ngõ khuất Tây Bắc (sau tháp) | 4.0m × 3.0m |
  | `ShopSlot_08` | Sân hiên tầng 2 phía Tây | 5.0m × 4.0m |

  Ô `5×4m` dành cho dịch vụ lớn (ví dụ Đấu giá, Sòng bạc, Kho Tạo Tác). Việc gán NPC vào ô cụ thể để sau.
- **Landmark chính:** Tầng trệt có vòm cổng đi xuyên qua được (thông thủy ≥ 3.2m), dẫn vòng ra hai hẻm Đông/Tây phía sau tháp. Không có ngõ cụt.
- **Lối lên tầng 2:** Có cầu thang ở cả hai phía Đông và Tây. Hai sân hiên nối với nhau qua cầu vượt, tạo thành vòng kín trên cao.
- **`OpenTable_Zone`:** Khoảng sân mở ngoài trời 4m × 4m ở trung tâm, không mái che, để giữ tầm nhìn thẳng lên tháp. Đặt lại `Counter_NiemPhong_Placeholder` và `Table_OpenBox_Placeholder` của Sprint 0 tại đây.

### 5.2. Hệ thống 4 Điểm nhấn (Landmarks)
1. **Chính — Tháp Đồng Hồ / Cổng Xuyên Thế Giới** (cuối trục Bắc, cao ~14–16m): Thân tháp đá châu Âu cổ, gắn vòng bánh răng đồng (steampunk), lõi cổng năng lượng mờ (sci-fi) và vòng cổ tự xoay chậm (phép thuật). Nhìn thấy từ hầu hết các điểm để người chơi luôn định hướng được.
2. **Phụ 1 — Cầu vượt đá liên hoàn** (bắc ngang trục chính ở cao độ `+3.8m`): Vòm đá nối hai dãy nhà tầng 2, chạy dọc bên hông là đường ống đồng xì hơi nhẹ và dây cáp năng lượng phát sáng dịu.
3. **Phụ 2 — Giếng / Đài Cổ Tự** (tại ngã rẽ phía Đông): Thành giếng đá rêu có khắc rune phát sáng ấm/xanh dịu, dẫn mắt người chơi rẽ vào khu ngõ khuất phía sau nhà che.
4. **Phụ 3 — Cổng vòm Biển Hologram cũ** (lối vào hẻm Đông Nam): Cổng đá vòm treo một tấm biển hologram hơi nhiễu/rỉ sét, gợi ý lối tắt đi vòng ra sau dãy nhà.

### 5.3. Quy tắc phân bổ chi tiết 80/20 (Không chia khu)
- **80% Nền Phố đá Châu Âu cổ:** Đá lát đường, tường đá khối lớn, cột gỗ tối, mái ngói dốc, lan can sắt rèn, đèn lồng ánh vàng ấm.
- **20% Chi tiết dị giới rải rác (gắn qua các Socket trên Prefab nhà/tường):**
  - *Steampunk:* Ống đồng, van xả, đồng hồ áp suất gắn rải rác ở góc tường và gầm cầu vượt.
  - *Sci-fi:* Hộp cáp năng lượng, ăng-ten nhỏ trên mái ngói, biển hiệu hologram mờ ở một vài cổng vòm.
  - *Phép thuật:* Đá rune khắc trên cột đá, lồng đèn tinh thể lơ lửng tại một vài ngã rẽ.
- **Quy tắc mật độ:** Mỗi mặt tiền nhà chỉ mang tối đa **1–2 chi tiết dị giới**, tránh gom cùng một loại vào một góc để không tạo cảm giác bị "chia khu".

### 5.4. Hệ thống Prefab Modular & Phân màu Greybox
Tất cả blockout lưu trong `Assets/_Project/Prefabs/Town/` (chia thành `Architecture/`, `Verticality/`, `Landmarks/`, `AccentSockets/`, `Slots/`):
- **Xám đá / Nâu rêu:** Tường nhà, mặt đường đá, vòm cổng.
- **Nâu gỗ / Đỏ ngói trầm:** Khung nhà, mái dốc, lan can cầu vượt.
- **Vàng đồng / Xanh lam dịu / Tím cổ tự:** Các khối đại diện cho chi tiết steampunk, sci-fi, phép thuật và điểm nhấn.
- **Xanh lá bán trong suốt (Marker):** 8 ô `ShopSlot` (`ShopSlot_01` → `ShopSlot_08`) + vùng `Inn_Spawn` (Phòng trọ) + `OpenTable_Zone` (Bàn mở/soi trung tâm). Các marker này chỉ hiển thị trong Editor/Greybox và có thể tắt renderer khi lên đồ thật.

### 5.5. Ánh sáng & Không khí URP
- **Ánh sáng chính:** Directional Light tông ấm chiều tà / đèn chợ (`#FFE2B8`, góc nghiêng tạo bóng đổ rõ qua cầu vượt và khe nhà).
- **Ánh sáng điểm (Point Light):** Đèn đường gắn dọc trục phố và trong hẻm vòm để hẻm khuất vẫn nhìn rõ đường đi nhưng có chiều sâu tối–sáng rõ rệt.

---

## 6. Phần để mở cho bước triển khai (Left to the Builder)

- Vị trí và chiều cao cụ thể của từng khối nhà phụ lấp khoảng trống ngoại vi (background skyline) để chặn tầm nhìn ra ngoài map.
- Số lượng bậc thang chính xác và thông số `MeshCollider` / `BoxCollider` cho cầu thang và dốc tầng 2.
- Cách tổ chức parent GameObject trong scene `Assets/_Project/Scenes/Market_Town.unity` (kèm giữ nguyên compatibility với `Player` và `HUD_Canvas` từ Sprint 0).
- Tinh chỉnh cường độ ánh sáng URP / Fog nhẹ để đạt cảm giác ấm áp, có chiều sâu.

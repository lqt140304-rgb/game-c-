# ASSET PRODUCTION LIST — PHỐ DỊ VẬT HÀ THÀNH

Danh mục mô hình 3D chia đúng 6 nhóm chuẩn hóa để dùng chung vật liệu/trim sheet trong Unity URP:

---

## 1. Modular Architecture Kit (Kiến trúc Lắp ghép — Dùng chung toàn khu phố)
- `MOD_Wall_Plaster_4m` / `MOD_Wall_Plaster_Door_4m`: Mảng tường vữa vàng Hà Nội cũ (`4m × 3.8m`).
- `MOD_Wall_BrickBase_4m`: Chân tường gạch nung & đá xanh chống ẩm (`4m × 1.2m`).
- `MOD_Wall_Ashlar_4m`: Tường đá cắt kiểu biệt thự Pháp cổ.
- `MOD_Wall_IndustrialBrick_4m`: Tường gạch đỏ & cột thép chữ I cho Xưởng Vá Víu.
- `MOD_Window_ShutterGreen`: Cửa chớp gỗ xanh rêu 2 cánh + khung vữa nổi.
- `MOD_Window_ShopGlass_Brass`: Ô cửa kính lớn hiệu kim hoàn bọc khung đồng.
- `MOD_Door_HanoiShop_Double`: Cửa gỗ lim 2–4 cánh mở rộng `3.6–4.2m`.
- `MOD_Balcony_WroughtIron_4m`: Lan can ban công sắt uốn Đông Dương.
- `MOD_Roof_Terracotta_Slope` / `MOD_Roof_FrenchSlate`: Mái ngói đất nung & mái đá phiến có ống khói / ô kính.
- `MOD_Mezzanine_WoodDeck` & `MOD_Railing_WoodBaluster`: Sàn gác lửng, cột gỗ lim & con tiện lan can.
- `MOD_Road_SidewalkCurb_6m` / `MOD_Plaza_GroundTile`: Mặt vỉa hè terrazzo, bó vỉa đá xanh, gạch đỏ Bát Tràng.

## 2. Furniture (Nội thất Lớn theo Tiệm)
- `FUR_Counter_DiMan_Merchant`: Quầy buôn gỗ lim dài `6.8m` (Nhà Niêm Phong).
- `FUR_Cabinet_HighGrid_Display`: Hệ tủ gỗ cao nhiều ô trưng bày hòm mở nắp & cổ vật.
- `FUR_Rack_Warehouse_4Tier`: Giá kệ kho hàng nặng 4 tầng (Khu C Nhà Niêm Phong & Kho Chú Rỉ).
- `FUR_Vitrine_KimKhuy_Gold`: Tủ kính chân đá hoa viền vàng kim (Kho Tạo Tác).
- `FUR_Desk_Appraisal_Scholar`: Bàn nghiên cứu gỗ sậm (Viện Giám Định).
- `FUR_Table_Inspection_OpenBox`: Bàn khui & soi cổ vật đệm nhung đỏ có đèn rọi.
- `FUR_Counter_Pawn_IronGrille`: Quầy giao dịch có khung song sắt (Tiệm Cầm Cố).
- `FUR_Chair_Indochine_TruongKy` / `FUR_Seat_AuctionVelvet`: Bộ trường kỷ Hà Nội & ghế nhung rạp đấu giá.
- `FUR_Room_InnBed_LedgerDesk`: Giường gỗ cũ, tủ áo & bàn sổ nợ (Phòng Trọ).

## 3. Gameplay Props (Vật phẩm Tương tác & Dữ liệu — Chi tiết cao)
- **4 Hòm Mù Đóng & Rỗng Lòng (`LidPivot` + `SealPart`):**
  - `SM_Box_WoodCrate.fbx` (`WoodCrate_Old`)
  - `SM_Box_BrassChest.fbx` (`BrassChest_Steam`)
  - `SM_Box_RunicArcane.fbx` (`RunicBox_Arcane`)
  - `SM_Box_HoloSciFi.fbx` (`HoloPod_SciFi`)
- **4 Hòm Mở Nắp Trưng Bày (Lót vải/nhung/đệm bên trong):**
  - `SM_Box_WoodCrate_Open.fbx`, `SM_Box_BrassChest_Open.fbx`, `SM_Box_RunicArcane_Open.fbx`, `SM_Box_HoloSciFi_Open.fbx`.
- **10 Hiện Vật / 6 Tạo Tác (Chi tiết soi gần `ArtifactInspector3D`):**
  - `SM_Art_AmberFossil.fbx`, `SM_Art_JadeAmulet.fbx`, `SM_Art_Chalice.fbx`, `SM_Art_PocketWatch.fbx`, `SM_Art_GearCore.fbx`, `SM_Art_AutomatonBird.fbx`, `SM_Art_StarlightCompass.fbx`, `SM_Art_RunicMask.fbx`, `SM_Art_HoloCube.fbx`, `SM_Art_VoidPrism.fbx`.
- **Thiết bị Tương tác Tiệm:** Bục trưng bày hộp có bảng giá (`PED_BlindBox_Display`), Sổ Cái Thu Nợ, Khuôn Đóng Dấu Sáp Đỏ.

## 4. Decorative Props (Trang trí Đời thường & Nghề nghiệp — Đặt có chọn lọc)
- `PROP_Crate_CargoStacked` (kiện hàng đóng dấu thế giới đặt trong kho & ròng rọc).
- `PROP_Scale_BrassBalance` & `PROP_WaxSeal_StampSet` (cân đồng & bộ sáp niêm phong trên quầy).
- `PROP_Jewelry_LoupeTools` (dụng cụ kim hoàn trong tủ/bàn Kim Khuy).
- `PROP_Lab_Microscope_Vials` & `PROP_Scroll_AncientMap` (kính hiển vi, ống nghiệm, cuộn bản đồ cổ).
- `PROP_Wrench_GearParts` & `PROP_Pottery_BrokenShard` (dụng cụ sửa & mảnh gốm ở Xưởng Chú Rỉ).
- `PROP_PottedPlant_Terracotta`, `PROP_BambooBasket`, `PROP_PlasticStool/Table` (đặt gọn ở góc sân/vỉa hè phụ, không đặt trước cửa chính cản đường).

## 5. Lighting Fixtures (Hệ thống Đèn theo Tiệm)
- `LGT_Lantern_HanoiSilk` (đèn lồng vải vàng ấm treo ở gác lửng & mặt tiền Nhà Niêm Phong).
- `LGT_Pendant_BrassShade` (đèn chụp đồng rọi thẳng bàn soi & bàn giao dịch).
- `LGT_Spot_VitrineMuseum` (đèn rọi nhỏ gắn trong tủ kính Kim Khuy & Viện Giám Định).
- `LGT_Lamp_IndustrialCage` (đèn lồng sắt công nghiệp cho Xưởng Chú Rỉ).
- `LGT_Street_ColonialLamp` (`SM_StreetLantern.fbx` — đèn đường cột gang dọc quảng trường).
- `LGT_Chandelier_AuctionGrand` (đèn chùm lớn hội quán Đấu Giá).

## 6. Shop-Specific Hero Assets (Điểm nhấn Độc bản mỗi Cửa hàng)
1. **Nhà Niêm Phong:** `HERO_PulleyHoist_GiantSealedCrate` — Hệ dầm ròng rọc sắt–gỗ & xích đồng treo Đại Hòm Niêm Phong Dị Giới giữa thông tầng Khu A.
2. **Kho Tạo Tác:** `HERO_KimKhuy_ResonanceDevice` + `HERO_Bikhe_ProgressionWall` — Vòng cơ khí cộng hưởng trung tâm & Tường Bí Khế đổi trạng thái cơ học/ánh sáng.
3. **Viện Giám Định:** `HERO_Appraisal_OculusTable` — Bàn thiết bị giám định trung tâm tích hợp thấu kính đồng + vòng quét dị giới dưới vòm kính.
4. **Xưởng Vá Víu:** `HERO_Restoration_RoboArmBench` — Bàn phục chế trung tâm với nhiều cánh tay máy khớp cơ khí dưới cầu trục.
5. **Tiệm Cầm Cố:** `HERO_Pawn_100DrawerVaultWall` — Hệ tủ trăm ngăn khóa cơ khí & biển số đồng phía sau song sắt.
6. **Nhà Đấu Giá:** `HERO_Auction_TurntableStage` — Sân khấu bục xoay trung tâm + đèn chùm lớn.
7. **Nhà Đỏ:** `HERO_RedHouse_FateWheelMachine` — Cỗ máy trò chơi dị giới trung tâm đại sảnh.

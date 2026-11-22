# Thiết kế Quảng trường Đài Phun Nước, 2 Quán Dạng Nhà & Bộ Asset Blender (Sprint 1)

**Ngày:** 2026-11-23  
**Nhánh:** `feat/market-town-blockout`  
**Công cụ 3D:** Blender `5.2.3 LTS` (`D:\blender\blender.exe`) → xuất `.fbx` sang Unity 6 (`6000.6.9f1`, URP)

---

## 1. Mục tiêu
1. Chuyển toàn bộ tài sản trung tâm của Sprint 1 từ khối primitive Unity sang **mô hình Blender chi tiết cao** (vát cạnh/bevel, tiện tròn, mái ngói xếp lớp, smooth shading, nhiều material slot).
2. Biến khu vực `OpenTable_Zone` (`X=0, Z=22`) thành **Đài phun nước trung tâm** của quảng trường.
3. Tách hoạt động mua và mở hộp thành **2 ngôi nhà riêng biệt đối diện nhau** qua quảng trường đài phun nước — người chơi **phải đi vào bên trong nhà** mới tương tác mua hộp hoặc mở/soi hiện vật.
4. Gắn **tấm biển chỉ dẫn mua sắm cỡ lớn** (khung gỗ viền đồng + chữ rõ nét) phía trên cửa chính và biển vẫy bên hông để từ xa người chơi nhận biết ngay chức năng từng nhà.
5. Nới rộng không gian quảng trường và sắp xếp **các tòa kiến trúc phụ xen kẽ** giữa các tòa nhà chính để thị trấn có chiều sâu.

---

## 2. Quy hoạch Quảng trường & Không gian (`TownLayout.cs`)

- **Quảng trường trung tâm:** Nới rộng quanh `Z ∈ [13, 31]`, `X ∈ [-10.5, 10.5]` (lối đi rộng rãi quanh đài phun nước, không cản trở trục Bắc-Nam).
- **Đài phun nước trung tâm (`Fountain_Central`):** Đặt tại `(0, 0, 22)`, đường kính ~`3.8m`.
- **Nhà 1 — Quán Bán Hộp Mù (`Shop_BlindBox_House`, Dì Mận):**
  - Vị trí: `(-6.8, 0, 22)`, quay mặt hướng Đông (`+X`) nhìn thẳng sang đài phun nước.
  - Kích thước ngoài: rộng mặt tiền `6.0m` (`Z`), sâu `5.0m` (`X`), cao tường `3.4m` + mái dốc lên `5.2m`.
  - Cửa chính rộng `2.4m × 2.8m` (không đặt collider chắn ngang lối đi), bên trong đặt **Quầy Niêm Phong**, 4 hộp mẫu trưng bày, đèn lồng ấm và kệ hàng.
  - Biển hiệu lớn: **`QUẦY NIÊM PHONG · BÁN HỘP MÙ`** (`~3.4m × 0.95m`) gắn trên mặt tiền + biển treo vươn ra quảng trường.
- **Nhà 2 — Tiệm Mở & Soi Hiện Vật (`Shop_OpenBox_House`):**
  - Vị trí: `(+6.8, 0, 22)`, quay mặt hướng Tây (`-X`), **đối diện trực tiếp** Quán Bán Hộp Mù qua đài phun nước.
  - Kích thước tương đương (`6.0m × 5.0m`), cửa chính rộng `2.4m × 2.8m`.
  - Bên trong đặt **Bàn Mở & Soi Hiện Vật** (thảm nhung đỏ, đèn bàn đồng `SpotLight`, kính lúp, 3 ô đặt hộp), giá gỗ trưng bày và đèn trần ấm.
  - Biển hiệu lớn: **`TIỆM MỞ & SOI HIỆN VẬT`** (`~3.4m × 0.95m`) gắn trên mặt tiền + biển treo vươn ra quảng trường.
- **Tòa kiến trúc xen kẽ & Luật kiểm tra:**
  - Điều chỉnh các khối `Building` / `CoverBuilding` quanh `Z ∈ [14, 30]` lùi ra biên phù hợp và bổ sung các khối nhà phụ thấp tầng/mái hiên xen kẽ giữa các tòa nhà chính.
  - Giữ nguyên 8 `ShopSlotMarker` (`ShopSlot_01`…`ShopSlot_08`) và đảm bảo toàn bộ test của `TownLayoutValidator` tiếp tục PASS.

---

## 3. Pipeline Blender 5.2.3 LTS → Unity URP

### 3.1. Sinh `.fbx` tự động (`Tools/Blender/`)
- Viết các script Python `bpy` chạy headless qua:
  `"D:\blender\blender.exe" --background --python Tools/Blender/generate_all.py`
- Xuất file `.fbx` vào `Assets/_Project/Art/Blender/` với chuẩn:
  - Trục Unity (`-Z` forward, `+Y` up), tỉ lệ `1.0`, `Apply Transform`.
  - Đặt tên material slot thống nhất (`Mat_Stone`, `Mat_WoodLight`, `Mat_WoodDark`, `Mat_RoofTile`, `Mat_Brass`, `Mat_Gold`, `Mat_Iron`, `Mat_Velvet`, `Mat_WaxRed`, `Mat_DialWhite`, `Mat_Jade`, `Mat_Amber`, `Mat_Obsidian`, `Mat_Water`, `Mat_CyanGlow`, `Mat_ArcaneGlow`, `Mat_WarmGlow`, `Mat_Parchment`).

### 3.2. Danh sách Model cần xuất
1. **Quảng trường & Kiến trúc:**
   - `SM_Fountain_Central.fbx`: Bể bát giác đá vát cạnh, trụ 2 tầng chén nước tiện tròn, vòi đồng, node `WaterSurface` riêng.
   - `SM_House_BlindBox.fbx` & `SM_House_OpenBox.fbx`: Ngôi nhà 4 tường (tường mặt tiền tách 2 bên + lanh tô trên cửa để người chơi đi lọt vào trong), mái ngói xếp lớp, cửa sổ, dầm gỗ, khung biển hiệu lớn trên mặt tiền và biển treo ngang.
2. **4 Hộp mù nắp rời (`Boxes/`):**
   - `SM_Box_WoodCrate.fbx`, `SM_Box_BrassChest.fbx`, `SM_Box_RunicArcane.fbx`, `SM_Box_HoloSciFi.fbx`.
   - Mỗi hộp giữ đúng hierarchy `LidPivot` (bản lề sau) và `SealPart` (tem/chốt trước) để `BlindBoxProp.PlayUnboxAnimation()` hoạt động chính xác.
3. **10 Hiện vật chi tiết cao (`Artifacts/`):**
   - `SM_Art_PocketWatch.fbx`, `SM_Art_Chalice.fbx`, `SM_Art_HoloCube.fbx`, `SM_Art_RunicMask.fbx`, `SM_Art_AmberFossil.fbx`, `SM_Art_StarlightCompass.fbx`, `SM_Art_JadeAmulet.fbx`, `SM_Art_GearCore.fbx`, `SM_Art_AutomatonBird.fbx`, `SM_Art_VoidPrism.fbx`.

---

## 4. Tích hợp Unity (`ChoDoLa.Editor` & `ChoDoLa.Sprint1`)

- **Collider cho 2 ngôi nhà:** Gắn `BoxCollider` cho 4 tường (tách trái/phải/trên cửa trước), sàn nhà và mái để `CharacterController` của người chơi **đi thẳng qua cửa vào bên trong nhà** mượt mà, không bị kẹt.
- **Biển hiệu lớn:** Ngoài khung biển gỗ/đồng từ Blender, gắn thêm `WorldSpace Canvas` / `TextMesh` sắc nét ngay trên mặt biển hiệu chính và biển treo để chữ tiếng Việt có dấu đọc rõ từ xa.
- **Đài phun nước:** Gắn material nước xanh ngọc bóng URP cho `WaterSurface`, thêm `Light` hắt ấm và `ParticleSystem` tia nước rơi nhẹ trong bể.
- **Hộp & Hiện vật:** `MarketVisualFactory` load `.fbx` từ `Assets/_Project/Art/Blender/` (có fallback sang primitive nếu thiếu), giữ nguyên toàn bộ logic `Sprint1MarketService`, `BlindBoxProp`, `ArtifactInspector3D`, `OpenTableManager`, `Sprint1HudController`.

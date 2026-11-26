# Danh sách rương 3D (tích hợp 2026-11-24)

Bảng tổng hợp các rương đã được chủ dự án **duyệt mô hình** và gộp vào nhánh `claude/chests-integration`. **Chưa gán ID chính thức hay chủ đề vào dữ liệu game** (`Docs/Content/data/content.json`). Cách dựng và lệnh chạy lại từng rương: [BanGiao3D](BanGiao3D.md). Mục lục quyết định: [DaChot](../DaChot.md).

Số liệu dưới đây đo tự động bằng Blender 5.3 (module `bpy`, Python 3.13) khi nhập từng GLB trong `Assets/_Project/Art/Chests/`:
- **Tam giác**: tổng mọi lớp, **kể cả `Glow`** (nên có thể lớn hơn số "không tính Glow" ghi trong BanGiao3D một chút).
- **Kích thước**: rộng (X) × sâu (Y) × cao (Z), mét, đo trên hình bao các lớp trừ `Glow`, ở tư thế đóng.
- **Bản lề**: gốc của `Lid` so với hai mép trước/sau của nắp theo trục Y (mặt trước là -Y). ✓ nghĩa là gốc nằm ở mép sau (≥ 85% từ mép trước).

| # | Rương | File GLB | Nguồn | Tam giác | Kích thước (m) | Lớp (tam giác) | Bản lề nắp | Ảnh xem trước | Nhánh nguồn | Điểm yếu phiên tạo đã nêu |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Golden Treasure | `Chest_Golden.glb` | Meshy | 6.068 | 0,60 × 0,49 × 0,42 | `Body` 3.445, `Lid` 2.191, `Glow` 80, `Key` 352 | ✓ y=0,21 (nắp -0,21…0,21) | [ảnh](../../Assets/_Project/Art/Source/render_chest_golden_open.png) | `claude/eager-bardeen-kmg5yc (nhánh gốc)` | Không có Seal/Binding nên giai đoạn "Gỡ niêm" không thấy gì rơi; FBX nguồn không nằm trong repo; phong cách texture thực tế. |
| 2 | Magic | `Chest_Magic.glb` | Meshy | 5.490 | 0,59 × 0,57 × 0,60 | `Body` 2.241, `Lid` 2.817, `Glow` 80, `Key` 352 | ✓ y=0,24 (nắp -0,24…0,24) | [ảnh](../../Assets/_Project/Art/Source/render_chest_magic_open.png) | `claude/magic-chest` | Nắp vòm dày, mở 105° trông to; lòng rương chỉ là tấm lót phẳng nên nông; mặt đá hơi thô khi nhìn cận; tông tím rực lệch hướng low-poly trung tính. |
| 3 | Mimic | `Chest_Mimic.glb` | Meshy | 7.080 | 0,52 × 0,44 × 0,42 | `Body` 3.500, `Lid` 3.500, `Glow` 80 | ✓ y=0,18 (nắp -0,22…0,22) | [ảnh](../../Assets/_Project/Art/Source/render_chest_mimic_open.png) | `claude/mimic-chest` | Răng trên/dưới khi đóng chưa đo xuyên bằng số; góc há cuối nên 60–75°; Glow trong ảnh hơi nhạt; phong cách thực tế lệch hướng low-poly. Cần bật `biteIntro`. |
| 4 | Miner | `Chest_Miner.glb` | Meshy | 3.528 | 0,85 × 0,42 × 0,32 | `Body` 2.904, `Lid` 544, `Glow` 80 | ✓ y=0,21 (nắp -0,21…0,21) | [ảnh](../../Assets/_Project/Art/Source/render_chest_miner_open.png) | `claude/miner-chest` | Ở 3,4 nghìn tam giác mặt nắp và đá hơi vỡ, cuốc chim hơi méo; nắp ván phẳng, lòng chỉ là đệm phẳng; tỉ lệ đã bóp méo (được phép); tông tối, thực tế. |
| 5 | Rác Quý | `Chest_RacQuy.glb` | Blender | 2.260 | 0,41 × 0,37 × 0,45 | `Body` 1.458, `Lid` 406, `Glow` 252, `Seal` 72, `Binding` 72 | ✓ y=0,18 (nắp -0,18…0,18) | [ảnh](../../Assets/_Project/Art/Source/render_chest_rac_quy_open.png) | `claude/chest-rac-quy` | Vật liệu phát sáng của hơi thối chưa kiểm trong URP; `innerLight` nên đặt màu xanh vàng. |
| 6 | Hải tặc | `Chest_HaiTac.glb` | Blender | 3.104 | 0,49 × 0,47 × 0,53 | `Body` 1.514, `Lid` 894, `Glow` 260, `Seal` 48, `Binding` 292, `Key` 96 | ✓ y=0,15 (nắp -0,27…0,19) | [ảnh](../../Assets/_Project/Art/Source/render_chest_hai_tac_open.png) | `claude/chest-hai-tac` | Dây thừng là vòng kín, ổ khóa cao trên trán: khi `Binding` rơi cần xem trong Unity có xuyên mặt sọ không. |
| 7 | Âm nhạc | `Chest_AmNhac.glb` | Blender | 3.820 | 0,34 × 0,28 × 0,47 | `Body` 1.724, `Lid` 1.216, `Glow` 20, `Seal` 72, `Binding` 300, `Record` 488 | ✓ y=0,07 (nắp -0,15…0,06) | [ảnh](../../Assets/_Project/Art/Source/render_chest_am_nhac_open.png) | `claude/chest-am-nhac` | Có lớp `Record`; cần bật `spinRecord` để đĩa quay. |
| 8 | Việt Nam | `Chest_VietNam.glb` | Blender | 2.962 | 0,50 × 0,50 × 0,37 | `Body` 996, `Lid` 1.766, `Glow` 20, `Seal` 24, `Binding` 156 | ✓ y=0,24 (nắp -0,25…0,25) | [ảnh](../../Assets/_Project/Art/Source/render_chest_viet_nam_open.png) | `claude/chest-viet-nam` | Phiên tạo không nêu điểm yếu riêng. |
| 9 | Tuổi thơ Việt Nam | `Chest_TuoiTho.glb` | Blender | 654 | 0,74 × 0,57 × 0,33 | `Body` 508, `Lid` 120, `Glow` 10, `Seal` 16 | ✓ y=0,25 (nắp -0,31…0,25) | [ảnh](../../Assets/_Project/Art/Source/render_chest_tuoi_tho_open.png) | `claude/chest-tuoi-tho` | Phiên tạo không nêu điểm yếu riêng (đã sửa glow xuyên nắp). |
| 10 | Robot | `Chest_Robot.glb` | Blender | 2.354 | 0,50 × 0,44 × 0,55 | `Body` 1.340, `Lid` 910, `Glow` 104 | ✓ y=0,15 (nắp -0,15…0,16) | [ảnh](../../Assets/_Project/Art/Source/render_chest_robot_open.png) | `claude/chest-robot` | Phiên tạo không nêu điểm yếu riêng. |
| 11 | Không gian | `Chest_KhongGian.glb` | Blender | 860 | 0,54 × 0,52 × 0,53 | `Body` 564, `Lid` 228, `Glow` 44, `Binding` 24 | ✓ y=0,14 (nắp -0,13…0,13) | [ảnh](../../Assets/_Project/Art/Source/render_chest_khong_gian_open.png) | `claude/chest-khong-gian` | Phiên tạo không nêu điểm yếu riêng. |
| 12 | Game | `Chest_Game.glb` | Blender | 964 | 0,46 × 0,40 × 0,30 | `Body` 328, `Lid` 556, `Glow` 20, `Seal` 48, `Binding` 12 | ✓ y=0,05 (nắp -0,16…0,05) | [ảnh](../../Assets/_Project/Art/Source/render_chest_game_open.png) | `claude/chest-game` | Nắp đã nghiêng sẵn 52° nên góc mở chỉ nên khoảng 80–90°. |
| 13 | Thể thao | `Chest_TheThao.glb` | Blender | 1.068 | 0,36 × 0,38 × 0,38 | `Body` 510, `Lid` 242, `Glow` 80, `Seal` 52, `Binding` 184 | ✓ y=0,17 (nắp -0,17…0,17) | [ảnh](../../Assets/_Project/Art/Source/render_chest_the_thao_open.png) | `claude/chest-the-thao` | Phiên tạo không nêu điểm yếu riêng. |
| 14 | Trung Hoa cổ | `Chest_TrungHoa.glb` | Blender | 1.760 | 0,40 × 0,37 × 0,50 | `Body` 1.102, `Lid` 342, `Glow` 60, `Seal` 128, `Binding` 128 | ✓ y=0,17 (nắp -0,16…0,16) | [ảnh](../../Assets/_Project/Art/Source/render_chest_trung_hoa_open.png) | `claude/chest-trung-hoa` | Phiên tạo không nêu điểm yếu riêng. |
| 15 | Nhật Bản cổ | `Chest_NhatBan.glb` | Blender | 1.774 | 0,41 × 0,39 × 0,45 | `Body` 966, `Lid` 504, `Glow` 80, `Seal` 24, `Binding` 200 | ✓ y=0,16 (nắp -0,17…0,17) | [ảnh](../../Assets/_Project/Art/Source/render_chest_nhat_ban_open.png) | `claude/chest-nhat-ban` | Phiên tạo không nêu điểm yếu riêng. |
| 16 | Cổ Phong | `Chest_CoPhong.glb` | Blender | 1.934 | 0,43 × 0,37 × 0,44 | `Body` 1.230, `Lid` 350, `Glow` 18, `Seal` 24, `Binding` 312 | ✓ y=0,11 (nắp -0,10…0,11) | [ảnh](../../Assets/_Project/Art/Source/render_chest_co_phong_open.png) | `claude/chest-co-phong` | Phiên tạo không nêu điểm yếu riêng. |
| 17 | Thiên Cơ | `Chest_ThienCo.glb` | Blender | 2.220 | 0,38 × 0,40 × 0,52 | `Body` 1.036, `Lid` 1.056, `Glow` 60, `Seal` 68 | ✓ y=0,15 (nắp -0,16…0,17) | [ảnh](../../Assets/_Project/Art/Source/render_chest_thien_co_open.png) | `claude/eager-bardeen-kmg5yc (nhánh gốc; claude/chest-thien-co trỏ cùng commit, không có gì mới)` | Mặt trong nắp khi mở toang còn sáng xám; đĩa Glow phẳng và to. |
| 18 | **v1 H1** Thùng Gỗ | `Chest_ThungGo.glb` | Blender | 1.052 | 0,50 × 0,40 × 0,31 | `Body` 682, `Lid` 144, `Glow` 2, `Seal` 68, `Binding` 156 | ✓ y=0,17 (nắp -0,18…0,18) | [ảnh](../../Assets/_Project/Art/Source/render_chest_thung_go_open.png) | `claude/youthful-knuth-yhr3zt` | Chưa duyệt. Gỗ đơn sắc, không texture; dây nhựa là vòng kín nên khi `Binding` rơi cần xem có xuyên nắp không; `Glow` là tấm phẳng. |
| 19 | **v1 H2** Hộp Tôn | `Chest_HopTon.glb` | Blender | 1.230 | 0,53 × 0,38 × 0,27 | `Body` 790, `Lid` 304, `Glow` 2, `Seal` 38, `Binding` 96 | ✓ y=0,17 (nắp -0,17…0,17), bản lề ở **mép dưới nắp** | [ảnh](../../Assets/_Project/Art/Source/render_chest_hop_ton_open.png) | `claude/youthful-knuth-yhr3zt` | Chưa duyệt. Chữ tem nhỏ, khó đọc ở xa; gỉ là mảng phẳng dán nổi 1,5 mm. |
| 20 | **v1 H3** Bao Tải | `Chest_BaoTai.glb` | Blender | 726 | 0,42 × 0,38 × 0,41 | `Body` 292, `Lid` 168, `Glow` 8, `Seal` 26, `Binding` 232 | ✓ y=0,15 (nắp -0,14…0,14), bản lề ở chân váy nắp | [ảnh](../../Assets/_Project/Art/Source/render_chest_bao_tai_open.png) | `claude/youthful-knuth-yhr3zt` | Chưa duyệt. Dáng hơi tròn như túi/hũ; cao 0,41 m vì chóp nút buộc. |
| 21 | **v1 RD** Vali Đen | `Chest_ValiDen.glb` | Blender | 1.045 | 0,51 × 0,39 × 0,35 | `Body` 302, `Lid` 313, `Glow` 2, `Seal` 68, `Binding` 360 | ✓ y=0,17 (nắp -0,18…0,18) | [ảnh](../../Assets/_Project/Art/Source/render_chest_vali_den_open.png) | `claude/youthful-knuth-yhr3zt` | Chưa duyệt. Dày 0,35 m để đạt cỡ tối thiểu nên giống hộp dụng cụ hơn vali dẹt. |

Kết quả: cả 17 file đầu có root `Chest_<Tên>` duy nhất, đều có `Body`, `Lid`, `Glow`, và gốc `Lid` nằm ở bản lề phía sau.

Bốn dòng 18–21 là **4 rương v1 thay cho 17 rương trên** (bối cảnh hẻm Việt Nam, xem [TASKS](../../TASKS.md) bước 5); 17 rương cũ chỉ còn để tham khảo. Cả 4 có root `Chest_<Tên>` duy nhất, đủ `Body`, `Lid`, `Glow`, `Seal`, `Binding`, gốc `Lid` ở bản lề phía sau, nắp mở lên đúng khi xoay góc âm quanh X (kiểm bằng ảnh Cycles).

## Kiểm tra khi tích hợp

- `git lfs ls-files` liệt kê đủ 17 `Chest_*.glb` và nguồn Meshy (Magic, Mimic, Miner); không file nào còn là con trỏ LFS (mọi GLB bắt đầu bằng `glTF`).
- `python -m py_compile` cho toàn bộ 31 script trong `Tools/Blender/`: đều qua.
- `cut_lid_from_meshy_fbx.py` (bản gộp Magic + Miner): chạy lại đúng lệnh của Magic và Miner trong BanGiao3D cho ra GLB có phần JSON giống hệt file đã commit, dữ liệu số chỉ lệch nhiễu float ≤ 6e-8. Lệnh kiểu Golden (7 tham số, dùng tạm FBX Magic vì FBX Golden không có trong repo) cho cùng số tam giác và cùng JSON với script thời Golden.
- `cycles_chest_preview_setup.py` (bản gộp): dựng thử được Golden (mặc định), Magic (`CHEST_GLB`, `CHEST_EXPOSURE`) và Miner (`--glb`, `--size`, không có `Key`).
- Mọi ảnh `render_chest_*.png` mà BanGiao3D nhắc tới đều tồn tại.
- `BoxOpenSequence.cs`: cú pháp hợp lệ khi phân tích bằng tree-sitter C#, không trùng thành viên; **chưa biên dịch** (máy tích hợp không có Unity hay trình biên dịch C#).

## Chưa làm / chưa kiểm

- Chưa nhập vào Unity, chưa tạo prefab, chưa gắn `BoxOpenSequence` cho rương nào; chiều mở nắp (`lidOpenSign`), vật liệu và ánh sáng trong Unity chưa kiểm chứng.
- Ảnh xem trước đều dựng ngoại tuyến (Blender Cycles hoặc bộ vẽ phần mềm), không phải ảnh từ Unity.
- Chưa gán ID chính thức hay chủ đề vào dữ liệu game.

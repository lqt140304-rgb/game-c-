# Hướng dẫn thử rương trong Unity (nhánh `claude/chests-all`)

Tài liệu cho người **không rành Unity**. Làm lần lượt từ trên xuống. Mỗi bước có phần "Nên thấy gì" để biết bước đó đã đúng chưa. Gặp lỗi thì xem mục [Xử lý lỗi](#7-xử-lý-lỗi) và gửi lại theo [mẫu báo lỗi](#9-mẫu-báo-lỗi).

Nhánh này gộp: 17 mô hình rương, lõi hiệu ứng mở rương (vòng quay màu bậc + bóng món), hiệu ứng và âm thanh riêng của từng rương, và menu cầm rương trên tay.

> **Lưu ý:** bảng màu bậc, nhịp giây và toàn bộ âm thanh là **đề xuất, chờ duyệt**. Chưa ai nghe âm, chưa ai chạy trong Unity. Thứ bạn thấy và nghe trong lần thử này chính là để duyệt.

---

## 1. Chuẩn bị máy (làm một lần)

### 1.1. Cài Git LFS và tải mô hình thật

Các file `.glb` (mô hình 3D) được lưu bằng **Git LFS**. Thiếu bước này, mỗi file `.glb` chỉ là một file chữ vài trăm byte và Unity không mở được.

1. Tải và cài Git LFS: <https://git-lfs.com> (Windows: chạy file cài đặt; macOS: `brew install git-lfs`).
2. Mở cửa sổ dòng lệnh (Windows: Git Bash hoặc PowerShell) **trong thư mục dự án**, chạy:

```bash
git lfs install
git fetch origin claude/chests-all
git checkout claude/chests-all
git lfs pull
```

**Nên thấy gì:** lệnh `git lfs pull` tải vài chục MB. Kiểm tra nhanh: file `Assets/_Project/Art/Chests/Chest_Golden.glb` phải nặng khoảng **4,6 MB** (không phải vài trăm byte). Lệnh `git lfs ls-files` liệt kê các file, mỗi dòng có dấu `*` (đã tải) chứ không phải `-` (mới là con trỏ).

### 1.2. Cài Unity 6000.6.9f1

1. Cài **Unity Hub**: <https://unity.com/download>.
2. Trong Unity Hub: `Installs → Install Editor → Archive` (hoặc tab phiên bản), chọn đúng **6000.6.9f1**. Dự án ghi phiên bản này trong `ProjectSettings/ProjectVersion.txt`; mở bằng bản khác Unity sẽ hỏi nâng cấp, nên tránh.
3. Module: không bắt buộc gì thêm để thử trong Editor. Nên tick **Windows/Mac Build Support** tương ứng máy bạn nếu sau này muốn build (không cần cho lần thử này).

### 1.3. Mở dự án

1. Unity Hub → `Projects → Add → Add project from disk`, chọn thư mục gốc của repo (thư mục có `Assets/`, `Packages/`, `ProjectSettings/`).
2. Bấm vào dự án để mở. Lần đầu Unity nhập toàn bộ tài nguyên, có thể mất **5–15 phút**.

**Nên thấy gì:** Unity mở cửa sổ chính. Dưới cùng là thanh tiến trình nhập; chờ đến khi biến mất.

---

## 2. Thêm gói đọc `.glb` (glTFast)

Unity không tự đọc `.glb`. Cần gói **glTFast**:

1. Menu trên cùng: `Window → Package Manager`.
2. Bấm nút **`+`** ở góc trên bên trái → chọn dòng **`Install package by technical name...`** (dòng thứ 4 trong menu; không chọn "from disk", "tarball" hay "git URL").
3. Ô **Name** gõ: `com.unity.cloud.gltfast` (ô Version để trống) → **Install**. Nếu ô có sẵn số phiên bản, cứ để nguyên.
4. Chờ Unity cài và nhập lại các `.glb` (vài phút).

Nếu Package Manager báo **thiếu gói phụ thuộc** (ví dụ dòng đỏ nhắc tới `Newtonsoft` / `com.unity.nuget.newtonsoft-json`), lặp lại bước 2–3 với tên `com.unity.nuget.newtonsoft-json`. Bình thường glTFast tự kéo gói phụ thuộc, không cần làm gì thêm.

**Nên thấy gì:** trong cửa sổ `Project`, mở `Assets/_Project/Art/Chests/`: mỗi file `Chest_*.glb` có mũi tên nhỏ bên cạnh (bấm vào thấy các phần `Body`, `Lid`, `Glow`...). Nếu file chỉ có biểu tượng trang giấy trắng, glTFast chưa cài xong hoặc file chưa `git lfs pull`.

---

## 3. Chờ biên dịch và đọc lỗi trong Console

Sau mỗi lần cài gói hay đổi nhánh, Unity **biên dịch lại code** (biểu tượng xoay xoay ở góc dưới bên phải).

1. Mở Console: `Window → General → Console` (phím tắt `Ctrl+Shift+C`).
2. Ba nút ở góc phải Console bật/tắt: lỗi (đỏ), cảnh báo (vàng), thông báo (trắng). **Bật cả ba.**
3. **Dòng đỏ** = lỗi. Nếu có lỗi biên dịch (thường bắt đầu bằng `Assets/...cs(số,số): error CS...`), menu `Tools → Cho Do La` sẽ **không hiện** hoặc không chạy. Bấm vào dòng đỏ để xem đầy đủ, rồi **chép nguyên văn** gửi lại (xem [mẫu báo lỗi](#9-mẫu-báo-lỗi)).
4. Nút **Clear** xoá các dòng cũ, để chỉ thấy lỗi của lần chạy mới.

**Nên thấy gì:** không có dòng đỏ. Cảnh báo vàng về âm thanh hay về file chưa có là bình thường; cứ đọc qua.

---

## 4. Dựng cảnh thử (làm theo đúng thứ tự)

Tất cả nằm trong menu trên cùng **`Tools → Cho Do La`**.

### 4.1. `Tools → Cho Do La → Create Sprint 0 Test Scene`

Tạo cảnh chợ thử `Market_Test` có người chơi góc nhìn thứ nhất và camera `MainCamera`.

**Nên thấy gì:** cửa sổ `Hierarchy` (bên trái) có người chơi và camera; cửa sổ `Scene` có sàn và vài vật.

### 4.2. `Tools → Cho Do La → Add Chest Hand Menu`

Gắn menu cầm rương vào camera và nạp danh sách rương.

**Nên thấy gì:** Console in `[ChestHandMenu] Tìm thấy 17 rương trong Assets/_Project/Art/Chests: AmNhac, CoPhong, Game, Golden, HaiTac, KhongGian, Magic, Mimic, Miner, NhatBan, RacQuy, Robot, TheThao, ThienCo, TrungHoa, TuoiTho, VietNam.` Ít hơn 17 thì xem cảnh báo vàng ngay phía trên (thiếu glTFast, file con trỏ LFS...).

### 4.3. `Tools → Cho Do La → Create All Chest FX Profiles`

Tạo 17 hồ sơ hiệu ứng (`ChestFx_<Tên>.asset`) trong `Assets/_Project/ScriptableObjects/ChestFx/`, một thư viện `ChestFxLibrary.asset` (hồ sơ + món minh hoạ), các prefab hook nhỏ trong `ScriptableObjects/ChestFx/HandMenu/`, rồi gán thư viện cho menu cầm rương trong cảnh. **Phải chạy sau bước 4.2** (cần có menu cầm rương trong cảnh để gán). Chạy lại bao nhiêu lần cũng được: hồ sơ đã có chỉ được điền thêm âm còn thiếu.

**Nên thấy gì:** Console in `[ChestFxAll] 17/17 hồ sơ trong Assets/_Project/ScriptableObjects/ChestFx, 87 món minh hoạ, gán thư viện cho 1 ChestHandMenu.` (số món có thể khác). Có thể có vài cảnh báo vàng kiểu `... not found; using shared ...`: một âm riêng bị thiếu và đã thay bằng âm chung; không chặn việc thử.

### 4.4. Lưu cảnh

Bấm **`Ctrl+S`** (macOS `Cmd+S`). Không lưu thì lần mở sau mất menu và thư viện.

---

## 5. Chơi thử

Bấm nút **Play** (tam giác ở giữa trên cùng). Bấm vào cửa sổ `Game` để nó nhận phím. Bấm Play lần nữa để dừng.

### 5.1. Các phím

| Phím | Việc | Ghi chú |
|---|---|---|
| **C** | Mở/đóng bảng chọn rương | Chọn bằng chuột, phím số `1`–`9`, `0` (rương thứ 10), hoặc `↑` `↓` rồi Enter |
| **[** / **]** | Rương trước / sau | Đang cầm thì tự cất rồi đổi |
| **O** | Mở nắp thường (không hiệu ứng màu) | Rương có lớp `Key` (Golden, Magic, Hải tặc) có màn chìa khóa bay vào trước |
| **P** | **Mở đầy đủ có hiệu ứng**: vòng quay màu bậc, bóng món nhô ra lớn dần, hiệu ứng và âm riêng của rương | Món minh hoạ chọn ngẫu nhiên mỗi lần |
| **T** | Đổi bậc dùng cho phím P: `ngẫu nhiên → Rác → Thường → Hiếm → Rất hiếm → Huyền thoại → ngẫu nhiên` | Chỉ có tác dụng khi đang cầm rương; bậc hiện ở bảng góc phải |
| **R** | Đóng nắp và xoá hiệu ứng | |
| **F** | Soi rương (đưa ra giữa màn hình, xoay các mặt) | |
| **Q** | Cất rương | |
| **L** | Đảo mặt trước (khi nắp mở ngược hoặc chìa bay tới từ phía sau) | |
| **W A S D**, chuột | Đi lại, nhìn quanh | Tắt khi bảng chọn đang mở |

Bảng nhắc phím, rương đang cầm và trạng thái hiện ở **góc trên bên phải** màn hình Game.

### 5.2. Nên thấy gì

- **Bấm C rồi chọn một rương:** rương bay vào tay, phía dưới bên phải màn hình, **ổ khóa quay về phía bạn**. Mọi rương được thu nhỏ để cạnh dài nhất khoảng **0,26 m** (rương thật cao 0,3–0,6 m; Miner và Tuổi thơ là rương dẹt, rộng 0,74–0,85 m).
- **Bấm O:** rương rung, niêm/dây (nếu có) bung, nắp hé, bật rồi **mở toang ra phía sau, xa bạn**. Nếu nắp xuyên xuống thân hoặc mở về phía bạn → bấm **L**, rồi O lại.
  - Theo dữ liệu trong file, cả 17 rương đều có bản lề ở phía −Z, nên menu nhận mặt trước là +Z và đặt `lidOpenSign = −1`. Điều này **chưa được nhìn thấy trong Unity**; nếu sai, phím L đảo lại (tương đương `lidOpenSign = +1`).
- **Bấm P:** nắp mở như O, đồng thời trong lòng rương lóe sáng đổi màu liên tục (vòng quay bậc), các chấm nhỏ bay quanh rương (số chấm = hạng bậc), rồi **dừng đúng bậc** đã chọn (ngẫu nhiên hoặc bậc ép bằng T): chớp sáng, tia, hạt bung ra, âm khóa; một **bóng đen viền màu bậc** của món đồ nhô lên khỏi miệng rương và lớn dần. Đồng thời là hiệu ứng riêng của rương (bảng dưới). Toàn bộ mất vài giây (vòng quay tối thiểu khoảng 3 s, tuỳ hồ sơ từng rương). Bấm R để đóng, P để chạy lại.
- **Màu bậc (đề xuất):** xem `docs/superpowers/specs/2026-11-24-hieu-ung-mo-rong-design.md`.

### 5.3. Rương nào có gì riêng

Mọi rương đều có lõi chung (vòng quay màu, bóng món, hạt, tia, âm khóa theo bậc). Phần riêng dưới đây chỉ trang trí, không đổi bậc. Spec từng rương: `docs/superpowers/specs/2026-11-24-hieu-ung-<tên>-design.md`.

| Rương | Hiệu ứng nổi bật khi bấm P | Tùy chọn riêng |
|---|---|---|
| Golden | Lấp lánh vàng quanh miệng rương từ Hiếm | Có màn chìa khóa (`keyIntro`, bật sẵn) |
| Magic | Vòng pha lê kêu theo mỗi nhịp quay; vòng phù văn khi chốt, Rác thì phép xì hơi | Có màn chìa khóa |
| Mimic | Rương "đập tim" theo nhịp quay, nhỏ dãi; khi chốt nôn ra chất nhờn | **`biteIntro`**: trước khi mở, nắp đớp một cái kèm tiếng nhai. Phím P **tự bật** cho Mimic. Muốn bật cho phím O hay rương đặt trong cảnh: chọn object có `BoxOpenSequence` → Inspector → tick `Bite Intro` (hoặc menu `Tools → Cho Do La → Add Mimic FX To Selected Chest`) |
| Miner | Đèn thợ mỏ chập chờn, tia lửa cuốc, đá văng, đá quý lấp lánh từ Hiếm | |
| Rác Quý | Mây thối và ruồi bay quanh; Rác thì "xì hơi"; bậc cao ruồi tan, cột sáng thánh | |
| Hải tặc | Bọt sóng mỗi nhịp; mạch vàng phun khi chốt, đại bác từ Hiếm, sóng thần ở Huyền thoại; Rác thì mòng biển chế giễu | Có màn chìa khóa |
| Âm nhạc | Vòng cột equalizer neon nhảy theo bậc, đèn nhấp nháy 120 BPM; đám đông reo từ Rất hiếm | **`spinRecord`**: đĩa than quay theo độ sáng. Phím P **tự bật**. Bật cho phím O: chọn object có `BoxOpenSequence` → tick `Spin Record` (hoặc `Tools → Cho Do La → Wire Selected Chest As Am Nhac`) |
| Việt Nam | Sóng trống đồng lan ra (số vòng = bậc), mưa hạt lúa từ Hiếm, cánh sen từ Rất hiếm | |
| Tuổi thơ | Nến lồng đèn chập chờn, đom đóm bay thành ngôi sao năm cánh; Rác thì nến tắt phụt | |
| Robot | Mắt nhấp nháy màu bậc, bánh răng quay chậm dần; Rác chập mạch bốc khói | |
| Không gian | Sao trôi, vòng sonar mỗi nhịp; mưa sao băng, khói tên lửa; Rác "Houston..." | |
| Game | Hạt "pixel" vuông nhảy lên như bộ đếm điểm; Rác "game over" xám | |
| Thể thao | Còi khai cuộc; pháo giấy hai bên từ Hiếm; Huyền thoại có còi hơi và pháo hoa | |
| Trung Hoa cổ | Pháo nổ lách tách mỗi nhịp, tràng pháo khi chốt, đèn trời từ Rất hiếm | |
| Nhật Bản cổ | Cánh anh đào rơi; Rác thì Daruma lăn ngã rồi bật dậy; trống taiko ở bậc cao | Daruma lăn có thể "giằng co" với rương trên tay, xem [rủi ro](#8-những-gì-chưa-kiểm-được-và-rủi-ro) |
| Cổ Phong | Khói nhang, vệt mực loang dưới đáy, dấu son đỏ từ Hiếm | |
| Thiên Cơ | Hai bánh răng xanh nhảy từng răng theo nhịp, vòng khóa bật lần lượt, cột lửa đẩy | |

`keyIntro` (màn chìa khóa) bật sẵn cho mọi rương có lớp `Key`; rương không có `Key` thì bỏ qua. `biteIntro` và `spinRecord` **mặc định tắt** cho mọi rương; chỉ phím P bật chúng cho đúng Mimic và Âm nhạc.

### 5.4. Thử hiệu ứng trên bục (không cầm tay, tuỳ chọn)

`Tools → Cho Do La → Add Chest Reveal Test` đặt Chest_Golden lên bục trước camera, cỡ thật. Phím của bàn thử này: **T** bậc ép, **Y** món khác, **Space** mở, **R** đóng, **C** đổi rương. Bàn thử này cũng dùng phím C và T, nên **đừng dùng chung một cảnh với menu cầm rương** (hai thứ cùng phản ứng một phím). Muốn thử, tạo cảnh mới bằng `Create Sprint 0 Test Scene` (lưu cảnh cũ trước).

---

## 6. Phím P làm gì bên trong (để chỉnh khi cần)

- Gắn `ChestRevealSequence` lên rương đang cầm (nếu chưa có), nạp hồ sơ có `chestId` trùng tên rương từ `ChestFxLibrary.asset`, gắn hook riêng của rương (với các rương mà menu riêng của phiên đó gắn hook lên gốc rương: Golden, Magic, Mimic, Miner, Hải tặc, Âm nhạc, Robot, Game, Thể thao, Trung Hoa, Tuổi thơ; sáu rương còn lại tự sinh hook qua `customFxPrefab` của hồ sơ).
- Chọn bậc (ngẫu nhiên hoặc theo T) và một món ngẫu nhiên trong `Art/Items/LowPoly/**/*.glb` (danh sách điền sẵn lúc chạy menu 4.3, không quét lúc chạy game).
- Cỡ bóng món (`targetSize`, mặc định 0,3 m) được **nhân theo tỉ lệ thu nhỏ của rương trên tay** (khoảng 0,4–0,6), nên bóng không to hơn rương.
- Code: `Scripts/ChestFx/ChestFxLibrary.cs`, `Scripts/Interaction/ChestHandMenu.cs` (hàm `PlayFullReveal`), menu `Editor/CreateAllChestFxProfiles.cs`.

---

## 7. Xử lý lỗi

| Hiện tượng | Nguyên nhân thường gặp | Cách sửa |
|---|---|---|
| File `.glb` chỉ **vài trăm byte**, Console báo "con trỏ Git LFS" | Chưa `git lfs pull` | Làm lại mục 1.1, quay lại Unity, `Tools → Cho Do La → Refresh Chest List` |
| Menu `Tools → Cho Do La` **không có** | Code lỗi biên dịch (dòng đỏ trong Console) | Chép dòng đỏ đầu tiên gửi lại |
| Console: "CHƯA CÀI glTFast" / rương không hiện | Thiếu gói | Làm lại mục 2, rồi `Refresh Chest List` |
| **Không thấy rương trong bảng chọn**, hoặc ít hơn 17 | Danh sách cũ, hoặc file nhập lỗi | `Tools → Cho Do La → Refresh Chest List`, đọc cảnh báo vàng |
| Rương **màu hồng** hoặc **đen kịt** | Vật liệu dùng shader không hợp URP | Trong `Project`, mở mũi tên của file `.glb`, chọn vật liệu → Inspector → `Shader` chọn `Universal Render Pipeline/Lit` (hoặc một shader của glTFast, nhóm `Shader Graphs/glTF-...`). Nếu không sửa được vật liệu trong file `.glb`: chuột phải vật liệu → `Extract`/tạo vật liệu mới, gán lại. Chỉ tối thì thêm đèn |
| **Không nghe tiếng** | Không có `AudioListener`, âm lượng tắt, hoặc nút Mute | Cửa sổ Game: tắt nút **Mute Audio** trên thanh trên cùng. Hierarchy: camera phải có component `Audio Listener` (chỉ một cái trong cảnh). Rương sau khi bấm P có `Audio Source` (tự thêm); Console báo "not found" là thiếu file âm |
| Bấm **P** báo "Chưa có thư viện hiệu ứng" | Chạy 4.3 trước 4.2, hoặc chưa lưu cảnh | Dừng Play, chạy lại `Create All Chest FX Profiles`, `Ctrl+S`, Play |
| Bấm **P** báo "Chưa có hồ sơ hiệu ứng cho ..." | Hồ sơ rương đó tạo lỗi | Chạy lại 4.3, gửi dòng đỏ `[ChestFxAll] ...` |
| **Nắp mở ngược** | Mặt trước nhận sai | Bấm **L** khi đang cầm, rồi O/P. Ghi lại tên rương để sửa cố định |
| Hiệu ứng **ở lại chỗ cũ** khi đi | Một số hạt của hook riêng sinh ở toạ độ thế giới | Đứng yên khi bấm P (xem mục 8) |
| Phím **không ăn** | Cửa sổ Game chưa được chọn, hoặc bảng chọn C đang mở | Bấm chuột vào cửa sổ Game; bấm C để đóng bảng |

---

## 8. Những gì chưa kiểm được và rủi ro

Đã kiểm ngoài Unity: mã C# biên dịch được (với **bản giả lập** UnityEngine, chỉ bắt lỗi cú pháp và kiểu, không bắt lỗi hành vi); 17 GLB nhập được bằng Blender và có đủ lớp `Body`/`Lid`/`Glow`; 216 file mp3 đọc được bằng ffprobe.

**Chưa kiểm:** chưa chạy trong Unity; chưa ai nghe âm; hạt, tia, bóng món chưa ai thấy tận mắt; chiều mở nắp chỉ suy từ toạ độ trong file.

Rủi ro nhiều khả năng gặp:

- **Hiệu ứng trên tay:** hạt, đèn và hook riêng được thiết kế cho rương cỡ thật đặt trên bàn. Trên tay rương nhỏ đi một nửa, nên vài hiệu ứng (vòng sóng, đèn trời, pháo giấy, bánh răng) có thể **to quá** so với rương, chạm camera hoặc bị camera cắt. Bóng món đã tỉ lệ theo. Hạt của lõi chỉ có **vùng phát** tỉ lệ theo cỡ rương; cỡ hạt, tốc độ hạt, độ dài tia và hook riêng thì **không** tỉ lệ.
- **Đi lại khi đang chạy P:** gốc hiệu ứng lõi được gắn theo rương, nhưng các hạt riêng của từng hook có thể ở lại toạ độ thế giới. Đứng yên khi thử.
- **Nhật Bản (Rác):** Daruma lăn ngã có thể giằng co với tư thế cầm tay. **Mimic:** "đập tim" phóng to cả rương trên tay.
- **Cất rương (Q) giữa lúc hiệu ứng đang chạy** có thể để sót vài hạt hoặc đèn trong cảnh; bấm R trước khi Q.
- **Âm thanh:** `Audio/Chests/sfx_tick.mp3` (tiếng tick chung, dùng cho Golden) dài **0,52 s** trong khi nhịp quay nhanh nhất là 0,055 s, nên tiếng tick có thể chồng lên nhau, nghe như tiếng rè. Các tick riêng của 16 rương dài 0,13 s.
- Phím **T** của menu cầm rương trùng phím T của bàn thử `Add Chest Reveal Test` (mục 5.4).

---

## 9. Danh sách kiểm (đánh dấu khi xong)

- [ ] 1. `git lfs pull` xong; `Chest_Golden.glb` nặng khoảng 4,6 MB.
- [ ] 2. Mở dự án bằng Unity 6000.6.9f1, Console không có dòng đỏ.
- [ ] 3. Cài `com.unity.cloud.gltfast`; các `.glb` có mũi tên mở ra được.
- [ ] 4. `Create Sprint 0 Test Scene` tạo cảnh, có camera và người chơi.
- [ ] 5. `Add Chest Hand Menu` báo **17 rương**.
- [ ] 6. `Create All Chest FX Profiles` báo **17/17 hồ sơ**, đã `Ctrl+S`.
- [ ] 7. Play, bấm C, cầm được cả 17 rương, ổ khóa quay về phía mình.
- [ ] 8. Bấm O: nắp mở ra phía sau ở cả 17 rương (ghi lại rương nào phải bấm L).
- [ ] 9. Bấm P: thấy vòng quay màu dừng đúng bậc chọn bằng T, bóng món nhô lên, hiệu ứng riêng của rương.
- [ ] 10. Nghe được âm (tick, khóa, âm riêng) và ghi nhận xét về âm, màu bậc, nhịp giây để duyệt.

---

## 10. Mẫu báo lỗi

Chép mẫu này, điền, gửi lại:

```
Nhánh: claude/chests-all (commit: <git rev-parse --short HEAD>)
Unity: 6000.6.9f1    Hệ điều hành: <Windows/macOS ...>
Bước: <ví dụ 4.3 Create All Chest FX Profiles / 5 bấm P>
Rương: <ví dụ Chest_Mimic>    Bậc (T): <ví dụ Huyền thoại / ngẫu nhiên>
Hiện tượng: <thấy gì, khác gì so với "Nên thấy gì">
Console (chép nguyên văn dòng đỏ/vàng đầu tiên, bấm vào dòng để thấy đủ):
<dán ở đây>
Ảnh chụp hoặc video ngắn (nếu có): <đính kèm>
```

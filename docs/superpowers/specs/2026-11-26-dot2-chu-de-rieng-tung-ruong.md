# Đợt 2: mỗi rương một cách mở, ánh sáng và âm thanh riêng theo chủ đề

> **2026-11-27: phần "vật mở khoá" của tài liệu này ĐÃ BỊ BỎ** (chủ dự án quyết định). Đã gỡ mã chìa khoá; mỗi rương còn một file dữ liệu `Chests/<Tên>Preset.cs`, âm thanh bám sự kiện chuyển động, xem [2026-11-27-ruong-am-thanh-dong-bo-design.md](2026-11-27-ruong-am-thanh-dong-bo-design.md). Các phần về rung lắc, bóp méo, vòng đổi màu, hào quang vẫn dùng làm tham chiếu.


Đọc kèm [kế hoạch](2026-11-26-don-sach-ruong-chia-khoa-ke-hoach.md) (mục 2, 3, 4.1, 6). Yêu cầu của chủ dự án sau khi chạy thử trong Unity:

- "Chìa khoá" không có nghĩa là cắm vào rồi vặn. Cách bay và cách mở phải **hợp chủ đề** từng rương.
- Rương KHÔNG cố định tại chỗ: rung/bay/xoay/lắc mọi hướng (đã có khung, chọn bằng `Preset.moveStyle`).
- Ánh sáng và âm thanh làm lại **theo chủ đề và theo từng thời điểm**; âm tạo bằng ElevenLabs.
- Giữ hào quang bản cũ; ánh sáng glow trong miệng rương đã dịu (đừng làm chói trở lại).

## 1. Việc của MỖI phiên rương (làm đủ 5 mục)

Chỉ sửa: `Assets/_Project/Scripts/ChestFx/Chests/<Lớp>.cs`, file manifest riêng `Tools/Audio/manifests/chest_<tên>.json`, các mp3 mới trong thư mục âm của rương. KHÔNG sửa lõi, `ChestKeyHookBase`, `ChestKeyContext`, `ChestKeyScript`, `ChestKeySetup`, `ChestKeyTypes`, `ChestRevealSequence`, kế hoạch, README, UnityStub*.cs.

1. **Cách bay riêng** (`ThemeFly`, 1,0 s): vật di chuyển đúng bản tính của nó, không chỉ vòng cung Bezier (vẹt vỗ cánh lượn zích zắc, bóng đá lăn rồi nảy, hoa đăng bồng bềnh trôi, phi thuyền lướt nghiêng, bánh răng lăn tít...). Trả `null` nếu thật sự thấy cách bay chuẩn là đủ, nhưng ưu tiên làm riêng.
2. **Cách mở riêng** (`ThemeAction`, 0,35 s): hành động hợp chủ đề thay cho "cắm rồi vặn" (xem bảng mục 3). Chỉ cử động khối con của chính vật (xoay, tịnh tiến, co dãn), KHÔNG sinh thêm vật nào; kết thúc đúng ở khe (`c.RestPos`) rồi gọi `ShrinkAtSlot` để thu nhỏ.
3. **Ánh sáng theo chủ đề**: đặt `lightColor` và `lightMul` (0,3–0,9) trong `Preset`; nếu cần nháy riêng thì ghi đè `OnKeyImpact` (gọi `Reveal.PulseGlow(màu, độ mạnh, giây)`; KHÔNG tạo Light/hạt mới, màu mỗi kênh ≤ 1).
4. **Chuyển động cả rương**: đặt `Preset.moveStyle` (gợi ý ở bảng mục 3; đổi được nếu có lý do, ghi vào báo cáo).
5. **Âm thanh ElevenLabs, 2 tiếng riêng** (mục 2).

Giữ chặt: tổng 1,35 s, sự kiện `chia_khoa_xong`, không thêm vật/hạt/đèn/phím, không `UnityEngine.Random` (dùng `c.Rng`), không Rigidbody, mọi thứ dọn ở `OnReset` (base lo). Không đổi 5 tiếng `lock_*` và nền cũ (chủ dự án thích âm cũ cho phần đó).
Đọc `ChestKeyHookBase.cs` (chữ ký `ThemeFly/ThemeAction/OnKeyImpact/ShrinkAtSlot`) và `ChestKeyContext.cs` (`S, H, front, up, slotPos, RestPos, tip, keySize, Facing, Part, Rng`) trước khi viết. Đọc một lớp khác đã có `AnimateAction` làm mẫu cách giữ tham chiếu khối con theo gốc vật (dictionary theo root, không static).

## 2. Âm thanh (ElevenLabs): 2 tiếng riêng mỗi rương

| Tệp | Thời điểm | Độ dài | Việc |
|---|---|---|---|
| `<tiền tố>fly.mp3` | 0,0 → 1,0 s, lúc vật bay tới | ≤ 1,0 s | tiếng bay/di chuyển của CHÍNH vật theo chủ đề (vỗ cánh, lăn, vút, bồng bềnh...) |
| `<tiền tố>action.mp3` | 1,0 → 1,35 s, lúc chạm/mở | ≤ 1,3 s | tiếng hành động mở theo chủ đề (mổ, đập, cào, chém, dán, nghiến răng cưa...), có điểm nhấn rõ ở 0,05–0,2 s đầu để khớp lúc chạm |

Cách làm:
1. Tạo `Tools/Audio/manifests/chest_<tên>.json` đúng định dạng của `Tools/Audio/elevenlabs_chest_key_manifest.json` (đọc để bắt chước): khoá = tên thư mục âm của rương, `prefix`, `hook`, `items` gồm 2 mục `key`: `key_fly` (`role: "fly"`, `field: "flyClip"`) và `key_action` (`role: "action"`, `field: "actionClip"`), `file` = `<tiền tố>fly.mp3` / `<tiền tố>action.mp3` (ví dụ `sfx_haitac_fly.mp3`), `loop: false`, `seconds` = độ dài dự định (0,8 và 1,2).
2. **Viết prompt cụ thể, tiếng Anh, tả ÂM THANH vật lý theo thời gian** (chất liệu, hành động, nhịp, không nhạc, không lời nói trừ khi vẹt/mèo kêu), ví dụ thay vì "parrot sound" viết "a parrot flapping wings fast with two short squawks, then a quick light beak tap"; ghi rõ điểm nhấn ở đầu cho `action`. Tránh prompt chung chung dễ ra âm chán.
3. Chạy: `ELEVENLABS_API_KEY=dummy python3 -I Tools/Audio/make_elevenlabs_chest_sfx.py --manifest Tools/Audio/manifests/chest_<tên>.json` (proxy tự chèn khoá). Mỗi file tối đa **2 lần** tạo (lần 2 chỉ khi lần 1 hỏng/quá im/sai độ dài; xoá file cũ trước khi tạo lại, ghi vào báo cáo). Dùng `--dry-run` để xem trước.
4. Kiểm bằng ffmpeg: độ dài, âm lượng (`volumedetect`). Cắt về độ dài tối đa của bảng bằng ffmpeg (fade out 10 ms); chuẩn hoá âm lượng để trung bình khoảng −20 đến −16 dB và đỉnh ≤ −1 dB (`ffmpeg -af loudnorm` hoặc `volume=`); tiếng quá nhỏ (< −30 dB trung bình) phải tăng.
5. Điền `flyFile`/`actionFile` trong `Preset` của lớp (sau khi tạo `Preset = KeyPreset.Neutral(...)` thì gán: `Preset.flyFile = "..."` cần struct khả biến: viết `static KeyPreset Make() { var p = KeyPreset.Neutral(...); p.flyFile = ...; return p; }` rồi `public static readonly KeyPreset Preset = Make();`; xem các rương đã làm tương tự với `keyFile`).
6. Commit mp3 + manifest.

## 3. Bảng chủ đề từng rương (gợi ý; chốt trong phiên, ghi rõ lựa chọn trong báo cáo)

| # | Rương | Vật | Cách bay (ThemeFly) | Cách mở (ThemeAction) | Sáng (lightColor) | moveStyle gợi ý | Tiền tố, thư mục âm |
|---|---|---|---|---|---|---|---|
| 1 | Hải tặc | vẹt | lượn zích zắc, cánh vỗ | mổ khoá hai nhát rồi giật mạnh như cạy | vàng đèn dầu | Wobble | `sfx_haitac_`, hai_tac |
| 2 | Rác Quý | mèo | nhảy cóc từng bước | vồ lên, cào hai cái vào khe | cam ấm / xanh lá nhạt | Wobble | `sfx_racquy_`, rac_quy |
| 3 | Golden | xu vàng | lăn tít như đồng xu quay, nảy | thả vào khe xoay rồi "ting" nảy trong máy | vàng chói nhẹ | Bounce | `sfx_golden_`, golden |
| 4 | Magic | tinh thể rune | lơ lửng bay uốn lượn, mảnh xoay quanh | áp vào khe, rune bừng sáng rồi nổ chớp nhẹ | tím | Float | `sfx_magic_`, magic |
| 5 | Mimic | dép lê | bị ném xoay tròn | thả rơi, rương "nhai" (dép rung bị hút vào) | đỏ-nâu | Shake | `sfx_mimic_`, mimic |
| 6 | Thợ Mỏ | cuốc chim | quay vòng như ném rìu | bổ xuống khe hai lần, đá vụn rung | cam đỏ / vàng đèn mỏ | Shake | `sfx_miner_`, miner |
| 7 | Âm nhạc | đĩa than | lăn đứng như bánh xe rồi ngả xuống | đĩa xoay tít, kim đặt xuống, "scratch" | hồng neon | Bounce | `sfx_amnhac_`, am_nhac |
| 8 | Việt Nam | hoa đăng | bồng bềnh trôi như thả sông | chạm khe nhẹ như đặt hoa đăng, cánh sen nở | vàng ấm | Float | `sfx_vietnam_`, viet_nam |
| 9 | Tuổi thơ | bi ve | lăn tưng tưng | búng bi (xoay rồi bật mạnh vào khe) | xanh dương sáng | Bounce | `sfx_tuoitho_`, tuoi_tho |
| 10 | Robot | thẻ chip | lướt thẳng, xoay 90° như drone | quẹt thẻ vào ổ đọc, đèn quét | cyan | Shake | `sfx_robot_`, robot |
| 11 | Không gian | phi thuyền | lướt nghiêng như bay vào bến | cập bến, nổ nhẹ động cơ hãm | xanh lam lạnh | Float | `sfx_khonggian_`, khong_gian |
| 12 | Game | chìa pixel | nhảy từng bước giật như khung hình | "ăn" vào khe (pixel vỡ ra rồi ráp lại) | vàng/xanh pixel | Bounce | `sfx_game_`, game |
| 13 | Thể thao | bóng đá | lăn rồi sút vòng cung | sút vào khe, bật lại rồi chui vào | trắng xanh sân | Bounce | `sfx_thethao_`, the_thao |
| 14 | Trung Hoa | đầu lân | nhảy múa lân, ngáp | há miệng ngoạm khoá, nghiến "cụp" | đỏ-vàng | Bounce | `sfx_trunghoa_`, trung_hoa |
| 15 | Nhật Bản | katana | bay ngang xoay chậm, lấp loáng | rút kiếm vung chém ngang một nhát, thu kiếm | trắng bạc / hồng anh đào | Shake (nhẹ) | `sfx_nhatban_`, nhat_ban |
| 16 | Cổ Phong | lá bùa | phấp phới bay như lá | dán lên (vỗ phẳng), chữ bùa lóe | vàng-đỏ huyền bí | Float | `sfx_cophong_`, co_phong |
| 17 | Thiên Cơ | bánh răng | lăn rồi quay tít | khớp vào cơ cấu, quay một nấc "cạch" | đồng cam | Spin | `sfx_thienco_`, thien_co |

(Tiền tố/thư mục: đối chiếu lại bằng `ls Assets/_Project/Audio/Chests` và `Preset` hiện có của lớp.)

## 4. Kiểm và giao

- `bash Tools/CompileCheck/check_all.sh` và `bash Tools/CompileCheck/check_chia_khoa.sh` phải qua.
- Commit trong worktree (lời commit tiếng Việt; cuối lời commit đúng 2 dòng `Co-Authored-By: Claude Opus <noreply@anthropic.com>` và `Claude-Session: https://claude.ai/code/session_01QKQCdbR9QHdsqnzntYD5oF`). KHÔNG push, KHÔNG mở PR.
- Báo cáo ≤ 12 dòng: cách bay + cách mở đã chọn, màu sáng + moveStyle, 2 prompt âm đã dùng và độ dài/âm lượng đo được, số lần tạo lại, tên nhánh worktree, điều chưa kiểm (chưa chạy Unity, chưa nghe).

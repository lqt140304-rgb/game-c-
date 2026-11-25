# Prompt mẫu: dọn sạch hiệu ứng rương + chìa khoá

Dùng kèm [kế hoạch](2026-11-26-don-sach-ruong-chia-khoa-ke-hoach.md). Lịch: **02:00 sáng 2026-11-26 (giờ Việt Nam)** mở **Phiên A** và **Phiên B**
(song song). Khi cả hai xong và đã gộp vào `claude/chest-trim-plan`, mở **17 phiên rương**. Cuối cùng **Phiên gộp**.
Mọi phiên: tiếng Việt, lời đơn giản khi nói với chủ dự án, không mở PR, không nói tên mô hình trong commit.

---

## Phiên A – Lõi + khung chìa (nhánh `claude/key-core`)

```
Game Unity "Chợ Đồ Lạ" (repo game-c-). Trả lời và viết tài liệu bằng tiếng Việt, lời đơn giản.

Việc: làm MỤC 4 "Kỹ thuật chung" (4.1 → 4.6) và MỤC 5 dòng "Phiên A" trong
docs/superpowers/specs/2026-11-26-don-sach-ruong-chia-khoa-ke-hoach.md. Đọc mục 0–3 trước để hiểu vì sao. Thiết kế đã chốt:
KHÔNG tự nghĩ lại, không thêm hiệu ứng ngoài kế hoạch.

Bạn đang ở nhánh tách từ claude/chest-trim-plan. Push lên claude/key-core.
Nếu máy chưa có mcs: sudo apt-get install -y mono-mcs.

Phải làm:
1. ChestKeyHookBase.cs (mục 4.1), ChestKeyScript.cs (4.2), 17 lớp con rỗng + KeyPreset (4.3), ChestKeySetup.cs + viết lại
   CreateAllChestFxProfiles.cs (4.4), cờ minimalFx trong lõi (4.5), gỡ mọi dòng gắn hook cũ khỏi ChestRevealDemo/ChestRevealTestSetup (4.6),
   xoá ChestFxPresets.ApplyGoldenScript.
2. Tools/CompileCheck/check_chia_khoa.sh; cập nhật check_all.sh cho khớp (các file hook cũ vẫn còn trong repo và vẫn phải biên dịch được
   cho tới khi từng phiên rương xoá chúng: KHÔNG xoá chúng ở phiên này).
3. Dự án phải biên dịch và 17 rương đều chạy chuỗi chuẩn bằng chìa mặc định (chỉ kiểm bằng mã/stub).

Giữ chặt: bậc do game quyết; chỉ phiên này được sửa lõi (ChestFxProfile, ChestRevealSequence, ChestScript, ChestMotion nếu cần);
ngẫu nhiên chỉ dùng ctx.RngFor, không UnityEngine.Random; hạt velocity chỉ qua ChestFxFactory.SetVelocity; mọi thứ dọn sạch ở OnReset.

Kiểm: bash Tools/CompileCheck/check_all.sh, check_chia_khoa.sh, check_particle_velocity.sh đều qua.
Commit (lời commit tiếng Việt), push claude/key-core. Báo cáo ≤ 12 dòng: đã làm gì, điểm nào khác kế hoạch, KeyContext/KeyPreset chốt ra sao.
```

## Phiên B – Âm chìa khoá (nhánh `claude/key-audio`)

```
Game Unity "Chợ Đồ Lạ" (repo game-c-). Trả lời bằng tiếng Việt, lời đơn giản.

Việc: tạo 20 file âm bằng ElevenLabs theo danh mục Tools/Audio/elevenlabs_chest_key_manifest.json (17 tiếng cắm chìa "key_insert" + 3 nền
còn thiếu cho Âm nhạc, Thể thao, Thiên Cơ), theo mục 3 của docs/superpowers/specs/2026-11-26-don-sach-ruong-chia-khoa-ke-hoach.md.
Chủ dự án đã cho phép dùng ElevenLabs: mỗi file tạo MỘT lần, đã có thì bỏ qua.

Bạn đang ở nhánh tách từ claude/chest-trim-plan. Push lên claude/key-audio.

1. Sửa Tools/Audio/make_elevenlabs_chest_sfx.py thêm tham số --manifest <đường dẫn> (mặc định như cũ).
2. Xin khoá ElevenLabs cho host api.elevenlabs.io bằng request_credential (header xi-api-key, không prefix); đặt ELEVENLABS_API_KEY=dummy.
   Không in/commit khoá. Nếu chủ dự án không cấp khoá: dừng, báo lại.
3. python3 -I Tools/Audio/make_elevenlabs_chest_sfx.py --manifest Tools/Audio/elevenlabs_chest_key_manifest.json --dry-run, rồi chạy thật.
4. Kiểm mỗi file: tồn tại, > 1 KB, độ dài gần đúng. Tiếng cắm chìa cần ngắn gọn (≤ 1,6 s): cắt bằng ffmpeg nếu dài hơn, fade 10 ms.
   Nền (loop) kiểm không lặng ở hai đầu.
5. Không sửa mã C#. Commit .mp3 + README thư mục âm + script, push claude/key-audio.
Báo cáo ≤ 10 dòng: số file tạo/lỗi/bỏ qua, rương nào thiếu; nhắc chủ dự án CHƯA AI NGHE, cần nghe thử và kiểm điều khoản giấy phép ElevenLabs.
```

---

## Phiên rương (mẫu, thay `<...>`)

```
Game Unity "Chợ Đồ Lạ" (repo game-c-). Trả lời và viết tài liệu bằng tiếng Việt, lời đơn giản khi nói với chủ dự án.

Việc: làm đúng MỤC 6.<N> "<TÊN RƯƠNG>" trong docs/superpowers/specs/2026-11-26-don-sach-ruong-chia-khoa-ke-hoach.md. Thiết kế đã chốt:
KHÔNG tự nghĩ lại, không thêm hiệu ứng. Đọc trước mục 0–3 (chỉ còn rung lắc, bóp méo, âm, chìa khoá) và mục 4.1 (KeyContext, quy ước hình chìa).

Bạn đang ở nhánh tách từ claude/chest-trim-plan (đã gộp Phiên A + B). Làm và push lên claude/key-<ruong>.
Nếu máy chưa có mcs: sudo apt-get install -y mono-mcs.

Phải làm, theo thứ tự:
1. Viết `BuildKey` trong Scripts/ChestFx/Chests/<LỚP>.cs đúng hình chìa ở bảng mục 6 (≥ 4 khối cơ bản, màu theo chủ đề, trục dài +Z,
   nhìn từ xa nhận ra chủ đề, khác hẳn chìa các rương khác).
2. Điền `Preset`: màu, thư mục + tiền tố âm, tên file nền (đã có sẵn trong Audio/Chests/<thư mục>/), hệ số bóp méo ±20% theo cột "Tính cách".
   KHÔNG đổi chuỗi chuẩn (thời gian, nấc rung, thứ tự).
3. Xoá mọi file cũ của rương này theo bảng "File cũ mỗi phiên rương xoá" (cả .meta, Editor setup, thư mục âm numpy, script make_*/check_*).
   Sau khi xoá, dự án vẫn phải biên dịch (không tham chiếu đến file đã xoá; nếu file chung đang tham chiếu thì ghi vào báo cáo, KHÔNG sửa file chung).
4. Thu spec cũ của rương (docs/superpowers/specs/2026-11-25-kichban-<spec>-design.md) còn ≤ 10 dòng: ghi "ĐÃ THAY bằng bản chìa khoá,
   xem kế hoạch 2026-11-26", mô tả chìa và nhịp mới, mục "chưa kiểm" trung thực (chưa chạy Unity, chưa nghe).

Chỉ được sửa: file của rương này (liệt kê ở mục 6), `<LỚP>.cs`, spec cũ của rương. KHÔNG sửa: lõi, ChestKeyHookBase, ChestKeyScript, ChestKeySetup,
CreateAllChestFxProfiles, ChestAudioRule, UnityStubAll.cs (thiếu thì ghi vào báo cáo).

Giữ chặt: không thêm vật/hạt/đèn/phím nào; ngẫu nhiên chỉ ctx.RngFor; bậc do game quyết; mọi thứ dọn sạch ở OnReset.

Kiểm: bash Tools/CompileCheck/check_all.sh và check_chia_khoa.sh qua. Tự đối chiếu "Bản chốt" mục 6 bằng cách đọc lại mã
(1 diễn viên, 0 Rigidbody/hạt/Light, 0 phím, 1 hook).
Commit (lời commit tiếng Việt), push claude/key-<ruong>, KHÔNG mở PR.
Báo cáo ≤ 10 dòng: hình chìa (mô tả khối + màu), hệ số bóp méo đã chọn, file đã xoá, điều không làm được.
```

### Bảng thay thế

| `<N>` | `<TÊN RƯƠNG>` | `<LỚP>` | `<ruong>` | `<spec>` |
|---|---|---|---|---|
| 1 | Hải tặc | HaiTacKeyHook | hai-tac | haitac |
| 2 | Rác Quý | RacQuyKeyHook | rac-quy | rac-quy |
| 3 | Golden | GoldenKeyHook | golden | golden |
| 4 | Magic | MagicKeyHook | magic | magic-rune |
| 5 | Mimic | MimicKeyHook | mimic | mimic |
| 6 | Thợ Mỏ | MinerKeyHook | miner | miner |
| 7 | Âm nhạc | AmNhacKeyHook | am-nhac | am-nhac |
| 8 | Việt Nam | VietNamKeyHook | viet-nam | viet-nam |
| 9 | Tuổi thơ | TuoiThoKeyHook | tuoi-tho | tuoitho |
| 10 | Robot | RobotKeyHook | robot | robot |
| 11 | Không gian | KhongGianKeyHook | khong-gian | khonggian |
| 12 | Game | GameKeyHook | game | game |
| 13 | Thể thao | TheThaoKeyHook | the-thao | the-thao |
| 14 | Trung Hoa | TrungHoaKeyHook | trung-hoa | trunghoa |
| 15 | Nhật Bản | NhatBanKeyHook | nhat-ban | nhat-ban |
| 16 | Cổ Phong | CoPhongKeyHook | co-phong | co-phong |
| 17 | Thiên Cơ | ThienCoKeyHook | thien-co | thienco |

## Phiên gộp (cuối cùng)

```
Gộp lần lượt claude/key-core, claude/key-audio, rồi 17 nhánh claude/key-<ruong> vào claude/chest-trim-plan. Sau mỗi lần gộp chạy
bash Tools/CompileCheck/check_all.sh (cài mono-mcs nếu thiếu). Xung đột chỉ chấp nhận ở file spec/stub; xung đột ở mã thì dừng và báo.
Đối chiếu "Bản chốt" mục 6 bằng grep: không còn tham chiếu tới hook cũ, không còn UnityEngine.Random trong hook mới, không còn phím tương tác,
không còn file *.wav numpy của rương. Push, không mở PR. Báo cáo ≤ 10 dòng.
```

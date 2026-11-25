# Prompt mẫu cho phiên thực hiện "thu gọn hiệu ứng rương"

Dùng kèm [2026-11-25-thu-gon-hieu-ung-ruong-ke-hoach.md](2026-11-25-thu-gon-hieu-ung-ruong-ke-hoach.md).
Thứ tự: chạy **Phiên 0** trước, rồi **Phiên âm ElevenLabs** (một phiên). Khi cả hai đã đẩy lên, chạy các phiên rương
(song song được), mỗi phiên
dùng prompt "Phiên rương" với `<MỤC>`, `<TÊN RƯƠNG>`, `<ruong>` thay đúng theo bảng cuối file.

---

## Prompt Phiên 0 (việc chung)

```
Game Unity "Chợ Đồ Lạ" (repo game-c-). Trả lời và viết tài liệu bằng tiếng Việt, lời đơn giản khi nói với chủ dự án.

Việc: làm MỤC 4 "PHIÊN 0 – việc chung" trong docs/superpowers/specs/2026-11-25-thu-gon-hieu-ung-ruong-ke-hoach.md,
đúng từng bước 0.1 → 0.4. Đọc mục 2 (thước đo) và mục 3 (quy tắc âm thanh) trước để hiểu vì sao.

Bắt đầu từ nhánh claude/chest-trim-plan (git fetch origin claude/chest-trim-plan), tạo nhánh claude/trim-phien-0.

Giữ chặt:
- Không đổi bậc do game quyết; không sửa ChestRevealSequence, ChestTension, ChestScriptRunner.
- Ngẫu nhiên chỉ trang trí theo hạt giống; hạt có velocity/orbital chỉ đặt qua ChestFxFactory.SetVelocity.
- Không xoá file hook cũ; chỉ gỡ chúng khỏi mọi đường gắn (menu, thư viện, prefab, bàn thử).
- Không làm việc của các mục 5.x (từng rương) trừ khi mục 4 ghi rõ.

Kiểm: bash Tools/CompileCheck/check_all.sh phải qua (nếu thiếu stub cho API đã có trong Unity, được thêm vào
Tools/CompileCheck/UnityStubAll.cs và ghi rõ trong báo cáo).
Commit (lời commit tiếng Việt, nêu từng bước 0.x), push nhánh claude/trim-phien-0, KHÔNG mở PR.
Báo cáo cuối ≤ 10 dòng: đã làm gì, bước nào không làm được và vì sao, lệnh kiểm đã chạy và kết quả.
```

---

## Prompt Phiên âm ElevenLabs (sau Phiên 0, trước các phiên rương)

```
Game Unity "Chợ Đồ Lạ" (repo game-c-). Trả lời bằng tiếng Việt, lời đơn giản.

Việc: tạo âm theo chủ đề cho 17 rương bằng ElevenLabs, theo mục 3b của
docs/superpowers/specs/2026-11-25-thu-gon-hieu-ung-ruong-ke-hoach.md và danh mục Tools/Audio/elevenlabs_chest_manifest.json.
Chủ dự án đã cho phép dùng ElevenLabs (tốn credit): mỗi file tạo MỘT lần, không tạo lại file đã có.

Nhánh: git fetch origin claude/trim-phien-0 && git checkout -b claude/trim-am-elevenlabs FETCH_HEAD

1. Xin khoá ElevenLabs cho host api.elevenlabs.io (request_credential, header xi-api-key), đặt ELEVENLABS_API_KEY=dummy.
   Không bao giờ in hay commit khoá.
2. python3 -I Tools/Audio/make_elevenlabs_chest_sfx.py --dry-run  (xem danh sách), rồi chạy thật từng rương:
   --chest <thư mục>. Lỗi thì ghi lại, chạy tiếp rương khác.
3. Tiếng có seconds < 0,5 trong danh mục: cắt bằng ffmpeg (nếu có) xuống đúng độ dài + fade 10 ms, ghi chú vào README.
4. Kiểm mỗi file: tồn tại, > 1 KB, độ dài gần đúng (ffprobe nếu có). Vòng nền (loop) kiểm không có khoảng lặng ở hai đầu.
5. Không sửa mã C#. Commit các .mp3 + README.md của từng thư mục âm, push claude/trim-am-elevenlabs, KHÔNG mở PR.
Báo cáo ≤ 10 dòng: số file đã tạo / lỗi / bỏ qua, rương nào thiếu, và nhắc chủ dự án: CHƯA AI NGHE, cần nghe thử và
kiểm điều khoản giấy phép ElevenLabs.
```

---

## Prompt Phiên rương (mẫu, thay `<...>`)

```
Game Unity "Chợ Đồ Lạ" (repo game-c-). Trả lời và viết tài liệu bằng tiếng Việt, lời đơn giản khi nói với chủ dự án.

Việc: làm đúng MỤC <MỤC> "<TÊN RƯƠNG>" trong
docs/superpowers/specs/2026-11-25-thu-gon-hieu-ung-ruong-ke-hoach.md. Thiết kế đã chốt: KHÔNG tự nghĩ lại thiết kế,
không thêm hiệu ứng ngoài mục BÙ. Đọc trước mục 2 (THƯỚC ĐO T1–T11) và mục 3 (QUY TẮC ÂM THANH A1–A8), rồi làm lần
lượt: CẮT → BÙ → SỬA LỖI → ÂM, cuối cùng đối chiếu ô "Bản chốt" (mọi con số phải khớp).

Nhánh: git fetch origin claude/trim-am-elevenlabs && git checkout -b claude/trim-<ruong> FETCH_HEAD
(nếu nhánh này chưa có: dừng, báo lại, không tự làm Phiên 0 hay Phiên âm).

Âm (mục 3b A9–A12): gán cho hook đúng các file trong Tools/Audio/elevenlabs_chest_manifest.json, mục của rương này
(ô `field`; `file` là tên mp3 trong Assets/_Project/Audio/Chests/<thư mục>/). Thêm mảng `stingClips[5]` trên hook nếu
chưa có và phát nó làm tiếng chốt (TC). Setup của rương nạp các .mp3 này (không nạp .wav numpy nữa). Âm tạo bằng mã
(MinerSynth/KhongGianSynth) thay bằng ô AudioClip mới. File nào thiếu (Phiên âm lỗi): để ô trống, ghi vào báo cáo.

Chỉ được sửa: các file liệt kê ở tiêu đề mục <MỤC>, file Editor/*Setup*.cs và *FxPreset.cs của chính rương này,
spec docs/superpowers/specs/2026-11-25-kichban-<spec>-design.md, và script âm Tools/Audio/make_<…>.py của rương này
nếu mục yêu cầu. KHÔNG sửa: CreateAllChestFxProfiles.cs, ChestFxPresets.cs, ChestAudioRule.cs, các file lõi trong
Scripts/ChestFx/*.cs, UnityStubAll.cs (thiếu stub thì ghi vào báo cáo).

Giữ chặt:
- Ý tưởng đặc trưng ở dòng "Giữ" của mục: không được mất.
- Bậc do game quyết; hook chỉ đọc bậc ở OnTierLocked; trước lúc chốt mọi bậc trông và nghe giống nhau.
- Ngẫu nhiên chỉ trang trí theo hạt giống của hook (không dùng UnityEngine.Random).
- Hạt có velocity/orbital chỉ đặt qua ChestFxFactory.SetVelocity.
- Reset (OnReset/OnDestroy) phải dọn sạch mọi vật đã tạo và trả rương về đúng vị trí/góc/tỉ lệ.
- Trong Setup của rương gọi ChestAudioRule.ApplyToProfile(p) MỖI lần chạy menu (không chỉ khi ô trống), để
  profile.tension.enabled = false và các ô âm lõi trống theo A1.

Âm: tự kiểm bằng cách đọc lại mã từ lúc bấm tới +3 s sau chốt và liệt kê trong báo cáo: (1) các lớp kéo dài trước chốt
(≤ 2), (2) các tiếng phát trong 0,3 s đầu sau chốt ở từng bậc (Rác 1, khác ≤ 2), (3) không chỗ nào nhân volume hai lần.

Kiểm biên dịch: bash Tools/CompileCheck/check_all.sh và script riêng Tools/CompileCheck/<check>.sh đều phải qua.
Cập nhật spec của rương: bảng nhịp + số mới, ghi "đã thu gọn theo kế hoạch 2026-11-25", và mục "chưa kiểm" trung thực
(chưa chạy Unity, chưa nghe).
Commit (lời commit tiếng Việt), push nhánh claude/trim-<ruong>, KHÔNG mở PR.

Báo cáo cuối (≤ 12 dòng, lời đơn giản): đã cắt gì, bù gì, sửa lỗi gì; bảng "Bản chốt" với số thực tế sau khi sửa;
liệt kê âm như trên; việc nào không làm được và vì sao.
```

---

## Bảng thay thế cho từng phiên

| `<MỤC>` | `<TÊN RƯƠNG>` | `<ruong>` (nhánh) | `<spec>` | `<check>` |
|---|---|---|---|---|
| 5.1 | Hải tặc | hai-tac | haitac | check_hai_tac_seq |
| 5.2 | Rác Quý | rac-quy | rac-quy | check_rac_quy |
| 5.3 | Golden | golden | golden | check_golden |
| 5.4 | Magic | magic | magic-rune | check_magic_rune |
| 5.5 | Mimic | mimic | mimic | check_mimic_feed |
| 5.6 | Thợ Mỏ | miner | miner | check_miner_dig |
| 5.7 | Âm nhạc | am-nhac | am-nhac | check_am_nhac_bass |
| 5.8 | Việt Nam | viet-nam | viet-nam | check_viet_nam_hoi_lang |
| 5.9 | Tuổi thơ | tuoi-tho | tuoitho | check_tuoi_tho |
| 5.10 | Robot | robot | robot | check_robot_bienhinh |
| 5.11 | Không gian | khong-gian | khonggian | check_khonggian_mission |
| 5.12 | Game | game | game | check_game_voxel |
| 5.13 | Thể thao | the-thao | the-thao | check_the_thao |
| 5.14 | Trung Hoa | trung-hoa | trunghoa | check_trung_hoa |
| 5.15 | Nhật Bản | nhat-ban | nhat-ban | check_nhat_ban |
| 5.16 | Cổ Phong | co-phong | co-phong | check_co_phong_ngu_kiem |
| 5.17 | Thiên Cơ | thien-co | thienco | check_thien_co |

## Phiên gộp (sau cùng, tuỳ chọn)

```
Gộp lần lượt các nhánh claude/trim-* vào một nhánh claude/trim-all (bắt đầu từ claude/trim-phien-0). Mỗi lần gộp chạy
bash Tools/CompileCheck/check_all.sh. Xung đột chỉ được phép ở file spec/stub; nếu xung đột ở mã hook thì dừng và báo.
Cuối cùng đối chiếu mục 6 "Bảng tổng hợp" của kế hoạch: rương nào còn lệch thì liệt kê. Push, không mở PR.
```

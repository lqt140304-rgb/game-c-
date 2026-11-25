# Âm thanh rương Nhật Bản cổ (Chest_NhatBan, búp bê Daruma)

**CHƯA AI NGHE THỬ.** Tạo bằng ElevenLabs Sound Effects (chủ dự án cho phép 2026-11-24), mỗi mô tả tạo một lần, `prompt_influence` 0,6, mp3 44,1 kHz 128 kbps. Kiểm tra điều khoản giấy phép ElevenLabs trước khi phát hành. Không dùng nhạc có bản quyền.

| File | Dùng ở | Dài (s) | Mô tả gửi đi (tiếng Anh) |
|---|---|---|---|
| sfx_nhatban_tick.mp3 | tick vòng quay (mõ gỗ) | 0,13* | Single dry wooden temple block knock, hollow wood clack, very short and sharp, no reverb (tạo 0,5 s, cắt `ffmpeg -t 0.1` + fade 30 ms) |
| sfx_nhatban_lock_rac.mp3 | chốt Rác (hài) | 2,25 | Comedic: a round wooden doll wobbles, tumbles over with a hollow thunk, rolls, then springs back upright with a cartoon boing, playful |
| sfx_nhatban_lock_thuong.mp3 | chốt Thường | 1,23 | Single soft wooden clapper strike followed by one gentle plucked koto string note, calm Japanese style |
| sfx_nhatban_lock_hiem.mp3 | chốt Hiếm ("tô mắt") | 1,80 | A clear small temple bell ding with a bright plucked shamisen flourish, sparkling and pleasant |
| sfx_nhatban_lock_ratHiem.mp3 | chốt Rất hiếm | 2,80 | Deep resonant temple bowl bell strike, shimmering wind chimes cascade and a rising koto glissando, magical |
| sfx_nhatban_lock_huyenThoai.mp3 | chốt Huyền thoại | 4,05 | Epic festival taiko drum ensemble crescendo, powerful thundering drum hits, crowd shouting a triumphant hey, big gong finish |
| sfx_nhatban_grow.mp3 | bóng vật phẩm phóng to | 1,41 | Airy whoosh of a paper fan swinging open with swirling cherry blossom petals and soft rising shimmer |
| sfx_nhatban_ambient.mp3 | nền dưới vòng quay | 3,53 | Building tension: rapid taiko drum roll slowly accelerating, light wind chimes tinkling in a breeze, Japanese festival anticipation |
| sfx_nhatban_lid_open.mp3 | giai đoạn 2 Hé nắp | 0,84 | Lightweight lacquered wooden lid lifting off a round papier mache doll, hollow wooden pop and soft scrape |
| sfx_nhatban_lid_thud.mp3 | giai đoạn 4 Mở toang | 0,73 | Hollow wooden doll top settling down with a soft knock, small taiko tap |
| sfx_nhatban_chime_a.mp3 | âm đệm sau khi chốt | 1,04 | Single glass wind chime bell tinkle, delicate summer breeze chime |
| sfx_nhatban_chime_b.mp3 | âm đệm sau khi chốt | 1,04 | Two quick tinkles of a small Japanese glass wind bell, light and airy |
| sfx_nhatban_koto_pluck.mp3 | âm đệm sau khi chốt | 0,91 | One bright plucked koto string note with gentle pitch bend, short decay |

\* Đoạn cắt 0,1 s; ffprobe báo 0,13 s do khung mp3 (phần thừa là im lặng sau fade).

Lần gọi song song đầu tiên bị 429 (quá số yêu cầu đồng thời) với 6 mô tả; gọi lại tuần tự một lần thì được. Không có âm nào tổng hợp bằng Python.
| `sfx_nhatban_shakuhachi_loop.mp3` | 8 | Japanese shakuhachi flute, breathy slow tense melody, seamless loop |
| `sfx_nhatban_taiko.mp3` | 0.6 | single deep taiko drum hit like a heartbeat |
| `sfx_nhatban_katana_draw.mp3` | 1.0 | katana being drawn from its scabbard, sharp metallic shing |
| `sfx_nhatban_slash.mp3` | 0.5 | fast katana slash swoosh |
| `sfx_nhatban_sheath.mp3` | 0.4 | katana snapping back into its scabbard, crisp click |
| `sfx_nhatban_rope_cut.mp3` | 0.8 | rope cut and paper seal falling softly |
| `sfx_nhatban_sandal.mp3` | 0.8 | wooden sandal slapping down then a cartoon boing |

Cắt bằng ffmpeg (ElevenLabs tối thiểu 0,5 s) về đúng độ dài + fade vào/ra 10 ms: `sfx_nhatban_sheath.mp3` (0.4 s).

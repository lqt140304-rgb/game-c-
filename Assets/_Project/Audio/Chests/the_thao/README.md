# Âm thanh rương Thể thao (Chest_TheThao)

Tạo bằng ElevenLabs sound-generation (chủ dự án cho phép 2026-11-24), `prompt_influence` 0.6, mỗi mô tả tạo một lần.
**CHƯA AI NGHE THỬ.** Kiểm điều khoản giấy phép ElevenLabs trước khi phát hành.

| File | Dùng ở | Dài (s) | Mô tả gửi đi |
|---|---|---|---|
| sfx_thethao_tick.mp3 | tick vòng quay | 0,13 | "single crisp soccer ball kick thump, short dry leather hit, close up" (tạo 0,5 s, cắt `-t 0.1` + fade) |
| sfx_thethao_lock_rac.mp3 | chốt Rác | 2,0 | "comedic missed soccer kick, foot whiffs air then a sad deflating ball squeak and a small crowd groan" |
| sfx_thethao_lock_thuong.mp3 | chốt Thường | 1,5 | "single short referee whistle blow, light polite applause from a small crowd" |
| sfx_thethao_lock_hiem.mp3 | chốt Hiếm | 2,5 | "two sharp referee whistle blasts followed by an excited stadium crowd cheer" |
| sfx_thethao_lock_ratHiem.mp3 | chốt Rất hiếm | 3,5 | "ball hits the back of the net, swish of the goal net, then a huge stadium crowd roar with air horns" |
| sfx_thethao_lock_huyenThoai.mp3 | chốt Huyền thoại | 5,0 | "massive stadium crowd erupting in a deafening goal celebration roar, drums, air horns and fireworks bursting overhead" |
| sfx_thethao_whoosh.mp3 | bóng vật phẩm phóng to | 1,2 | "soccer ball flying fast toward the camera, rising air whoosh with a spinning ball hum" |
| sfx_thethao_ambient.mp3 | nền vòng quay | 4,0 | "tense stadium crowd murmuring and drumming rhythmically, anticipation building before a penalty kick" |
| sfx_thethao_clap.mp3 | accent sau chốt | 1,5 | "stadium crowd rhythmic clapping three times in unison" |
| sfx_thethao_bounce.mp3 | accent + giai đoạn Mở toang | 1,0 | "soccer ball bouncing twice on hard ground, rubbery leather bounce" |
| sfx_thethao_lid.mp3 | giai đoạn Bật nắp | 1,2 | "rubbery leather ball seam unzipping and popping open with a puff of air" |
| sfx_thethao_kickoff.mp3 | TheThaoHook, đầu vòng quay | 1,2 | "referee whistle short chirp to start the match, sneakers squeaking on a court" |
| sfx_thethao_airhorn.mp3 | TheThaoHook, Huyền thoại | 1,0 | "short stadium air horn blast" |
| `sfx_thethao_trong.mp3` | 0.5 | big stadium bass drum, single hit |
| `sfx_thethao_kick.mp3` | 0.4 | football kicked hard, leather thump |
| `sfx_thethao_post.mp3` | 0.8 | football hitting a metal goalpost, loud clang |
| `sfx_thethao_net.mp3` | 0.8 | football smashing into the goal net, swish |
| `sfx_thethao_ooh.mp3` | 1.2 | stadium crowd going oooh at a near miss |
| `sfx_thethao_roar.mp3` | 2.0 | stadium crowd roaring after a goal |
| `sfx_thethao_aww.mp3` | 1.2 | stadium crowd groaning aww in disappointment |
| `sfx_thethao_riser.mp3` | 1.5 | stadium crowd murmur rising with tension |

Cắt bằng ffmpeg (ElevenLabs tối thiểu 0,5 s) về đúng độ dài + fade vào/ra 10 ms: `sfx_thethao_kick.mp3` (0.4 s).
| `sfx_thethao_key_insert.mp3` | 1.5 | a soccer ball kicked hard hitting a wooden box, leather thump and a short crowd gasp |
| `sfx_thethao_bed_loop.mp3` | 8 | stadium crowd murmur with a slow steady drum beat getting tense, seamless loop |

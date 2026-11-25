# Âm riêng rương Tuổi thơ Việt Nam (Chest_TuoiTho)

Tạo bằng ElevenLabs Sound Effects ngày 2026-11-24 (mỗi mô tả tạo một lần, `prompt_influence` 0,6). **CHƯA AI NGHE THỬ.** Cần chủ dự án kiểm điều khoản giấy phép ElevenLabs theo gói trước khi phát hành. Không dùng nhạc có bản quyền.

| File | Dài (s) | Dùng cho | Mô tả đã gửi (tiếng Anh) |
|---|---|---|---|
| `sfx_tuoitho_tick.mp3` | 0,13 | tick đổi màu | "Two small glass marbles clicking together once, single sharp tiny clack, dry, close up" (tạo 0,5 s, cắt `-t 0.1` + fade 30 ms) |
| `sfx_tuoitho_lock_rac.mp3` | 1,65 | chốt Rác (hài) | "Cheap plastic toy running out of battery, electronic melody slowing down and pitching down into a sad dying beep, comedic" |
| `sfx_tuoitho_lock_thuong.mp3` | 1,04 | chốt Thường | "Single bright bicycle bell ring, one ding, outdoor" |
| `sfx_tuoitho_lock_hiem.mp3` | 1,23 | chốt Hiếm | "Toy car horn honking twice, cheerful squeaky plastic beep beep" |
| `sfx_tuoitho_lock_ratHiem.mp3` | 2,51 | chốt Rất hiếm | "Small wind-up music box playing a short sparkling rising melody of five notes, warm and nostalgic" |
| `sfx_tuoitho_lock_huyenThoai.mp3` | 3,60 | chốt Huyền thoại | "Festive lantern night celebration swell: small frog drum rolling, little bells jingling, warm music box chord, children laughing happily in the distance" |
| `sfx_tuoitho_grow.mp3` | 1,41 | bóng phóng to | "Soft paper lantern unfolding and swelling with a warm airy whoosh rising, gentle shimmer" |
| `sfx_tuoitho_ambient.mp3` | 4,05 | nền lúc quay | "Warm night street at a mid-autumn festival, soft candle crackle, distant small hand drum tapping rhythm and tiny bells, crickets" |
| `sfx_tuoitho_bells.mp3` | 0,84 | âm đệm sau chốt | "Small cluster of tiny jingle bells shaken once, light" |
| `sfx_tuoitho_marbles.mp3` | 1,23 | âm đệm sau chốt | "Handful of glass marbles dropping and rolling on a tile floor, clinking" |
| `sfx_tuoitho_frogdrum.mp3` | 0,84 | âm đệm sau chốt | "Small wooden toy frog drum, quick tak tak tak rattle, playful" |
| `sfx_tuoitho_bamboo.mp3` | 1,04 | giai đoạn 2 Hé nắp | "Bamboo stick creaking and paper lantern rustling as it is lifted" |
| `sfx_tuoitho_candytin.mp3` | 1,04 | giai đoạn 4 Mở toang | "Old tin candy box lid popping open, metallic pop and light rattle of hard candies inside" |

Mọi file < 70 KB. Lần gọi song song đầu bị 429 (giới hạn tần suất) cho 7 file chưa tạo được; 7 file đó được gửi lại tuần tự đúng một lần, cùng mô tả.
| `sfx_tuoitho_roll_loop.mp3` | 6 | glass marble rolling continuously along a wooden ruler, soft rumble, seamless loop |
| `sfx_tuoitho_xylo.mp3` | 0.6 | single wooden xylophone note, bright, middle C |
| `sfx_tuoitho_flick.mp3` | 0.3 | finger flicking a glass marble, sharp click |
| `sfx_tuoitho_domino.mp3` | 0.3 | single plastic domino tile falling and clacking |
| `sfx_tuoitho_band.mp3` | 0.6 | rubber band slingshot twang |
| `sfx_tuoitho_bonk.mp3` | 0.5 | glass marble bonking on a wooden lid, cartoon bonk |
| `sfx_tuoitho_plane.mp3` | 1.0 | paper airplane gliding past, light airy whoosh |
| `sfx_tuoitho_crumple.mp3` | 0.8 | sheet of paper crumpled into a ball |

Cắt bằng ffmpeg (ElevenLabs tối thiểu 0,5 s) về đúng độ dài + fade vào/ra 10 ms: `sfx_tuoitho_flick.mp3` (0.3 s), `sfx_tuoitho_domino.mp3` (0.3 s).

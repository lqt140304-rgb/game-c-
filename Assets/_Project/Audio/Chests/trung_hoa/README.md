# Âm rương Trung Hoa cổ (Chest_TrungHoa)

Tạo bằng ElevenLabs sound-generation ngày 2026-11-24 (prompt_influence 0.6), mỗi mô tả tạo một lần.
**CHƯA AI NGHE THỬ.** Kiểm điều khoản giấy phép ElevenLabs trước khi phát hành.

| File | Dài (s) | Dùng ở | Mô tả gửi đi |
|---|---|---|---|
| sfx_trunghoa_tick.mp3 | 0,13 | tick vòng quay | single dry hollow wooden temple block knock, very short sharp click, no reverb (tạo 0,5 s, cắt `ffmpeg -t 0.1` + fade 30 ms) |
| sfx_trunghoa_lock_rac.mp3 | 2,0 | chốt Rác | wooden abacus beads clattering chaotically then a sad comedic plucked zither out-of-tune descending note, funny fail |
| sfx_trunghoa_lock_thuong.mp3 | 1,5 | chốt Thường | single plucked Chinese zither string with a soft small hand gong tap |
| sfx_trunghoa_lock_hiem.mp3 | 2,5 | chốt Hiếm | bright bronze gong strike with zither glissando upward, festive |
| sfx_trunghoa_lock_ratHiem.mp3 | 3,5 | chốt Rất hiếm | large bronze gong crash with festival drums rolling and firecrackers popping |
| sfx_trunghoa_lock_huyenThoai.mp3 | 5,0 | chốt Huyền thoại | grand ancient imperial court orchestra fanfare, huge temple bell ringing, big gong, drums and bamboo flutes, triumphant |
| sfx_trunghoa_whoosh.mp3 | 1,5 | bóng vật phẩm phóng to | soft rising whoosh of incense smoke with shimmering wind chimes swelling up |
| sfx_trunghoa_ambient.mp3 | 4,0 | nền vòng quay | mysterious ancient temple ambience, low bronze singing bowl hum with distant zither notes |
| sfx_trunghoa_firecracker.mp3 | 1,5 | âm đệm sau chốt | string of small firecrackers popping rapidly |
| sfx_trunghoa_bell.mp3 | 2,0 | âm đệm sau chốt | small bronze temple bell ding with long shimmering ring |
| sfx_trunghoa_lid.mp3 | 1,5 | bước 2 (Hé nắp) | heavy bronze cauldron lid scraping and lifting, metallic grind |
| sfx_trunghoa_thud.mp3 | 1,2 | bước 4 (Mở toang) | heavy bronze lid dropping on stone with resonant metallic clang |

Ghi chú: lần gọi song song đầu bị 429 (giới hạn 6 yêu cầu đồng thời); các file trên lấy từ lần gọi tuần tự, không tạo lại hàng loạt.
| `sfx_trunghoa_lion_drum_loop.mp3` | 8 | Chinese lion dance drums and cymbals, steady festive rhythm, seamless loop |
| `sfx_trunghoa_pop.mp3` | 0.4 | single small firecracker pop |
| `sfx_trunghoa_big_bang.mp3` | 1.5 | huge Chinese firecracker explosion, loud bang with crackle |
| `sfx_trunghoa_dud.mp3` | 1.5 | firecracker fuse fizzling out with a weak puff, dud |
| `sfx_trunghoa_match.mp3` | 0.6 | incense stick lighting a fuse, sizzle start |
| `sfx_trunghoa_rocket.mp3` | 1.2 | firework rocket whistling upward |
| `sfx_trunghoa_rocket_burst.mp3` | 1.5 | firework bursting in the sky, crackling sparkle |
| `sfx_trunghoa_sneeze.mp3` | 0.6 | cartoon lion sneeze, comedic achoo |

Cắt bằng ffmpeg (ElevenLabs tối thiểu 0,5 s) về đúng độ dài + fade vào/ra 10 ms: `sfx_trunghoa_pop.mp3` (0.4 s).

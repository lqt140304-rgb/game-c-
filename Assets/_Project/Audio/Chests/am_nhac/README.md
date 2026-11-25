# Âm thanh rương Âm nhạc (Chest_AmNhac)

Tạo bằng ElevenLabs Sound Generation (`mp3_44100_128`, `prompt_influence` 0,6) ngày 2026-11-24, mỗi mô tả tạo một lần.
**CHƯA AI NGHE THỬ.** Cần chủ dự án kiểm giấy phép ElevenLabs trước khi phát hành.

| File | Dài (s) | Dùng cho | Mô tả gửi dịch vụ |
|---|---|---|---|
| `sfx_amnhac_tick.mp3` | 0,13 | tick vòng quay | single tight closed hi-hat tick, crisp drum machine, dry, very short, no reverb (tạo 0,5 s, cắt `-t 0.1` + fade 30 ms bằng ffmpeg) |
| `sfx_amnhac_lock_rac.mp3` | 1,6 | chốt Rác (hài) | vinyl record needle scratching and skipping, comedic broken record stutter then a deflated out of tune kazoo note, funny fail |
| `sfx_amnhac_lock_thuong.mp3` | 1,2 | chốt Thường | single clean electric piano major chord stab, warm, short, pleasant |
| `sfx_amnhac_lock_hiem.mp3` | 1,6 | chốt Hiếm | funky clean electric guitar two-chord strum rising, bright, short groovy lick |
| `sfx_amnhac_lock_ratHiem.mp3` | 2,2 | chốt Rất hiếm | neon synthwave arpeggio rising with big snare hit and shimmering pad, retro eighties, exciting |
| `sfx_amnhac_lock_huyenThoai.mp3` | 3,4 | chốt Huyền thoại | epic rock electric guitar solo riff with power chord finale, big drum fill, crash cymbal and stadium crowd cheering |
| `sfx_amnhac_whoosh.mp3` | 1,5 | bóng phóng to | tape rewind whoosh rising into a bright synth riser swell, music power up |
| `sfx_amnhac_drumroll.mp3` | 3,0 | nền lúc quay | jukebox drum roll building suspense on snare with subtle vinyl crackle, steady crescendo |
| `sfx_amnhac_coin_slot.mp3` | 1,0 | giai đoạn 0 (Rung) | coin dropping into a vintage jukebox slot, mechanical click and whirr |
| `sfx_amnhac_needle_drop.mp3` | 1,0 | giai đoạn 2 (Hé nắp) | vinyl record needle drop with warm crackle and hiss, turntable starting to spin |
| `sfx_amnhac_note_pluck.mp3` | 0,8 | accent sau chốt (0/2/4/6/8 lần theo bậc) | single bright synth pluck note with short echo, musical sparkle |
| `sfx_amnhac_crowd.mp3` | 2,0 | AmNhacHook, từ Rất hiếm | small excited concert crowd cheering and whistling, short applause burst |

Tiếng nắp chạm (giai đoạn 4) dùng bộ chung `../Synth/sfx_chest_lid_thud.wav`.
Ghi chú: lượt gửi song song đầu tiên bị 429 (giới hạn đồng thời) ở 7 mô tả; các mô tả đó được gửi lại tuần tự một lần và thành công. Không tạo lại mô tả nào đã thành công.
| `sfx_amnhac_kick.mp3` | 0.5 | punchy deep 808 kick drum, single hit, dry |
| `sfx_amnhac_hat.mp3` | 0.3 | crisp closed hi-hat, single hit |
| `sfx_amnhac_clap.mp3` | 0.4 | tight electronic hand clap, single hit |
| `sfx_amnhac_wub.mp3` | 0.5 | single dubstep bass wobble wub, short |
| `sfx_amnhac_scratch.mp3` | 0.6 | DJ vinyl record scratch, quick |
| `sfx_amnhac_record_stop.mp3` | 1.2 | vinyl record slowing down to a stop, turntable power off |
| `sfx_amnhac_drop.mp3` | 2.0 | massive EDM bass drop impact with sub boom |
| `sfx_amnhac_riser.mp3` | 1.2 | short EDM white noise riser building up |
| `sfx_amnhac_plug_pop.mp3` | 0.6 | cable yanked out of a speaker, pop and electric fizz |
| `sfx_amnhac_wheels.mp3` | 1.5 | plastic wheels of a portable speaker rolling on pavement |
| `sfx_amnhac_feedback.mp3` | 0.8 | short speaker microphone feedback squeal, quiet |

Cắt bằng ffmpeg (ElevenLabs tối thiểu 0,5 s) về đúng độ dài + fade vào/ra 10 ms: `sfx_amnhac_hat.mp3` (0.3 s), `sfx_amnhac_clap.mp3` (0.4 s).
| `sfx_amnhac_key_insert.mp3` | 1.5 | a vinyl record dropping onto a turntable with a needle landing, warm crackle and a bass thump |
| `sfx_amnhac_bed_loop.mp3` | 8 | steady four-on-the-floor sub bass club beat with a soft hi-hat, building mood, no melody, seamless loop |

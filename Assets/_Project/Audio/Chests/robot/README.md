# Âm rương Robot (ElevenLabs Sound Effects)

Tạo ngày 2026-11-24 bằng `/v1/sound-generation` (mp3 44,1 kHz 128 kbps, `prompt_influence` 0,6), mỗi mô tả tạo **một lần** (lượt đầu chạy song song bị 429 với 8 file, các file đó gửi lại tuần tự và chỉ thành công một lần). Hai tick tạo 0,5 s rồi cắt bằng `ffmpeg -t 0.1` kèm fade 20 ms (ffprobe báo 0,13 s do khung mp3).

**CHƯA AI NGHE THỬ.** Nghe và thay file cùng tên nếu không hợp. **Giấy phép ElevenLabs:** chủ dự án tự kiểm theo gói trước khi phát hành.

| File | Dài (s) | Dùng cho | Mô tả gửi dịch vụ |
|---|---|---|---|
| `sfx_robot_tick.mp3` | 0,13 | tick (bíp) | single tiny crisp retro computer beep blip, very short square wave, dry, no reverb |
| `sfx_robot_tick_ratchet.mp3` | 0,13 | tick (cót) | single short metallic wind-up toy key ratchet click, tin toy clockwork, dry, very short |
| `sfx_robot_lock_rac.mp3` | 1,65 | chốt Rác (hài, "hụt hơi") | comedic broken tin toy robot powering down, wheezing spring unwinding, sad descending beep boop, sputter and clank |
| `sfx_robot_lock_thuong.mp3` | 1,04 | chốt Thường | friendly cheerful two-tone robot beep confirmation, small tin toy robot, clean, short |
| `sfx_robot_lock_hiem.mp3` | 1,41 | chốt Hiếm | toy robot happy rising three-note electronic chirp with metallic gear click, bright |
| `sfx_robot_lock_ratHiem.mp3` | 2,04 | chốt Rất hiếm | tin robot transforming, quick servo whirs and gears clicking, ending in a shimmering electronic synth arpeggio rising |
| `sfx_robot_lock_huyenThoai.mp3` | 3,45 | chốt Huyền thoại | epic retro robot boot-up startup sequence, rising synth sweep into a big triumphant electronic chord with electric zap sparks and metallic clang |
| `sfx_robot_whoosh.mp3` | 1,52 | bóng phóng to | rising electronic charging hum with servo whir, a hologram projecting and growing, sci-fi toy |
| `sfx_robot_gears.mp3` | 3,03 | nền lúc quay | small metal gears and clockwork whirring and ticking inside a tin toy robot, mechanical loop |
| `sfx_robot_windup.mp3` | 2,04 | giai đoạn 0 Rung | winding up a tin toy clockwork key, ratcheting clicks, small metal spring tightening |
| `sfx_robot_steam.mp3` | 0,84 | giai đoạn 1 Gỡ niêm + đệm | short pneumatic hiss puff of air release from a small robot joint |
| `sfx_robot_servo.mp3` | 0,73 | giai đoạn 2 Hé nắp, 4 Mở toang + đệm | small toy robot servo motor whir and metal joint squeak clank |
| `sfx_robot_spark.mp3` | 0,63 | đệm sau chốt | short crackling electric spark zap, small electrical arc |
| `sfx_robot_babble.mp3` | 1,23 | đệm sau chốt | cute silly robot voice babbling gibberish beeps and boops, toy robot talking, no words |
| `sfx_robot_hum_loop.mp3` | 8 | small robot power core humming, electric whir, seamless loop |
| `sfx_robot_beep.mp3` | 0.25 | short cute robot beep |
| `sfx_robot_press.mp3` | 0.8 | robot power button click followed by a cheerful startup chirp |
| `sfx_robot_slot.mp3` | 0.7 | metal part clicking into a socket with a small electric zap |
| `sfx_robot_thruster.mp3` | 1.2 | small rocket thrusters igniting under a jumping robot, burst |

Cắt bằng ffmpeg (ElevenLabs tối thiểu 0,5 s) về đúng độ dài + fade vào/ra 10 ms: `sfx_robot_beep.mp3` (0.25 s).
| `sfx_robot_key_insert.mp3` | 1.5 | a robot keycard swiped into a scanner, quick electronic beep then a servo latch click |

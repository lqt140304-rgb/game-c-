# Âm thanh rương Game (Chest_Game)

Tạo bằng ElevenLabs sound-generation ngày 2026-11-24 (`prompt_influence` 0,6, mp3 44,1 kHz 128 kbps), mỗi mô tả một lần.
**CHƯA AI NGHE THỬ.** Không dùng giai điệu có sẵn hay tên game thật. Cần chủ dự án kiểm giấy phép ElevenLabs theo gói trước khi phát hành.

| File | Dài (s) | Dùng cho | Mô tả gửi đi (tiếng Anh) |
|---|---|---|---|
| `sfx_game_tick.mp3` | ~0,1 | tick mỗi lần đổi màu | "single short 8-bit square wave menu cursor blip, retro video game, dry, one beep" (tạo 0,5 s, bỏ khoảng lặng đầu, cắt 0,1 s, fade 20 ms bằng ffmpeg) |
| `sfx_game_lock_rac.mp3` | 2,5 | chốt Rác | "funny 8-bit chiptune game over jingle, descending sad square wave notes ending in a deflating buzz, comedic, retro console" |
| `sfx_game_lock_thuong.mp3` | 1,0 | chốt Thường | "short 8-bit chiptune item get jingle, two quick cheerful square wave notes, retro video game" |
| `sfx_game_lock_hiem.mp3` | 1,5 | chốt Hiếm | "8-bit chiptune level up jingle, quick rising three note arpeggio, bright square wave, retro console" |
| `sfx_game_lock_ratHiem.mp3` | 2,5 | chốt Rất hiếm | "8-bit chiptune power up fanfare, fast rising arpeggio with sparkle, square and triangle waves, triumphant retro video game" |
| `sfx_game_lock_huyenThoai.mp3` | 4,5 | chốt Huyền thoại | "epic 8-bit chiptune victory fanfare, stage clear celebration, rising arpeggios, triumphant final chord with noise drum roll, retro console" |
| `sfx_game_whoosh_grow.mp3` | 1,3 | bóng vật phẩm phóng to | "8-bit chiptune rising pitch sweep, power up grow sound, square wave sliding upward, retro video game" |
| `sfx_game_ambient_select.mp3` | 3,0 | nền khi quay màu | "8-bit chiptune tense waiting loop, fast pulsing square wave bass and arpeggio, retro game bonus roulette music" |
| `sfx_game_coin.mp3` | 0,5 | accent sau khi chốt | "8-bit retro coin pickup sound, two quick high square wave notes" |
| `sfx_game_jump.mp3` | 0,5 | accent sau khi chốt | "8-bit retro jump sound, quick upward square wave slide" |
| `sfx_game_button_click.mp3` | 0,5 | giai đoạn 0 Rung | "plastic game controller button press click, tactile, close up" |
| `sfx_game_boot.mp3` | 1,2 | giai đoạn 2 Hé nắp | "8-bit retro console power on boot sound, short rising chime with static" |
| `sfx_game_press_start.mp3` | 0,8 | giai đoạn 4 Mở toang | "8-bit retro game start confirm sound, short bright chime, press start button" |

Tiếng nắp chạm (giai đoạn 4) dùng bộ chung `Synth/sfx_chest_lid_thud.wav`. Nhiều yêu cầu đầu bị 429 (giới hạn 6 yêu cầu đồng thời chia chung giữa các phiên) và được gửi lại; không file nào tạo hai lần thành công.
| `sfx_game_chip_loop.mp3` | 8 | upbeat 8-bit chiptune game music, square wave arpeggio, seamless loop |
| `sfx_game_bump.mp3` | 0.3 | 8-bit block bump sound |
| `sfx_game_shatter.mp3` | 1.0 | 8-bit explosion of many pixel blocks breaking apart |
| `sfx_game_reassemble.mp3` | 1.2 | 8-bit rising arpeggio, pieces snapping back together |
| `sfx_game_voxel_tick.mp3` | 0.15 | tiny 8-bit blip |
| `sfx_game_one_up.mp3` | 0.8 | 8-bit extra life jingle |
| `sfx_game_power_up.mp3` | 0.8 | 8-bit power up rising sound |

Cắt bằng ffmpeg (ElevenLabs tối thiểu 0,5 s) về đúng độ dài + fade vào/ra 10 ms: `sfx_game_bump.mp3` (0.3 s), `sfx_game_voxel_tick.mp3` (0.15 s).
| `sfx_game_key_insert.mp3` | 1.5 | 8-bit key pickup chime followed by a short 8-bit lock-open blip |

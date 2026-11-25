# Âm thanh rương Không gian (Chest_KhongGian)

Tạo bằng ElevenLabs sound-generation (`prompt_influence` 0,6), ngày 2026-11-24, mỗi mô tả một lần (vài yêu cầu đầu bị 429 do giới hạn đồng thời rồi gửi lại, không có bản thừa). **CHƯA AI NGHE THỬ.** Cần chủ dự án kiểm điều khoản giấy phép ElevenLabs trước khi phát hành.

| File | Dài (s) | Dùng cho | Mô tả đã gửi (tiếng Anh) |
|---|---|---|---|
| `sfx_khonggian_tick.mp3` | 0,13 | tick mỗi lần đổi màu | "Single short submarine sonar ping blip, clean electronic radar beep, very short dry attack" (tạo 0,5 s, cắt `ffmpeg -t 0.1` + fade 30 ms) |
| `sfx_khonggian_lock_rac.mp3` | 2,0 | chốt Rác ("Houston, có sự cố") | "Comedic spaceship malfunction: sputtering radio static, a sad deflating electronic warble falling in pitch, a small clunk, funny failure" |
| `sfx_khonggian_lock_thuong.mp3` | 1,2 | chốt Thường | "Single clean spaceship console confirmation chime, soft synth bleep with short echo" |
| `sfx_khonggian_lock_hiem.mp3` | 1,6 | chốt Hiếm | "Two rising sci-fi synth tones, alien communicator signal received, shimmering echo" |
| `sfx_khonggian_lock_ratHiem.mp3` | 2,6 | chốt Rất hiếm | "Mysterious alien mothership tones, four note ascending theremin-like melody with deep pulsing synth pad and spacey reverb" |
| `sfx_khonggian_lock_huyenThoai.mp3` | 4,0 | chốt Huyền thoại | "Epic cosmic orchestral swell, deep space rumble, choir and brass rising, alien ship hum, bright sparkle, enormous infinite reverb tail" |
| `sfx_khonggian_whoosh_warp.mp3` | 1,4 | bóng vật phẩm phóng to | "Sci-fi warp jump whoosh, rising energy beam materializing object, teleport shimmer" |
| `sfx_khonggian_countdown.mp3` | 3,0 | nền dưới vòng quay | "Radio transmission countdown bed: crackling radio static, soft electronic beeps every half second, low space hum, no voice" |
| `sfx_khonggian_static_burst.mp3` | 0,8 | hook: đầu vòng quay, Rác (2 lần, hạ cao độ) | "Short burst of radio static crackle and squelch, walkie talkie" |
| `sfx_khonggian_hatch_hiss.mp3` | 1,5 | giai đoạn 3 Bật nắp (khí xì cửa khoang) | "Spacecraft hatch unlocking with pressurized air hiss and mechanical clunk" |
| `sfx_khonggian_landing_thud.mp3` | 1,2 | giai đoạn 4 Mở toang | "Space capsule landing legs touching down, heavy metallic thud with servo whir" |
| `sfx_khonggian_meteor.mp3` | 1,0 | accent sau khoá + hook (sao băng, số lần = hạng bậc) | "Small shooting star streak, quick sparkling whoosh passing by, airy and magical" |

Thiếu file nào thì `Tools/Cho Do La/Create Khong Gian Chest FX Profile` lấy bộ âm chung ở `Audio/Chests/` (rồi `Synth/`).
| `sfx_khonggian_engine_loop.mp3` | 8 | spaceship engine idling, low rumbling thrusters with piston pulse, seamless loop |
| `sfx_khonggian_radio_blip.mp3` | 0.4 | short sci-fi radio blip beep |
| `sfx_khonggian_fuel_dock.mp3` | 0.7 | metal fuel canister docking into a ship with an energy zap |
| `sfx_khonggian_rocket_launch.mp3` | 1.2 | small guided missile launching, whoosh with fizz |
| `sfx_khonggian_rocket_hit.mp3` | 1.5 | small missile hitting a metal hatch, sharp explosion |
| `sfx_khonggian_engine_die.mp3` | 1.5 | spaceship engine sputtering and dying with puffs, comedic |

Cắt bằng ffmpeg (ElevenLabs tối thiểu 0,5 s) về đúng độ dài + fade vào/ra 10 ms: `sfx_khonggian_radio_blip.mp3` (0.4 s).

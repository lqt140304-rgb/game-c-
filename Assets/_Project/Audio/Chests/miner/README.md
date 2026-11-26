# Âm riêng rương Thợ Mỏ (Chest_Miner)

Tạo bằng ElevenLabs sound-generation (prompt_influence 0,6) ngày 2026-11-24, mỗi mô tả một lần (7 mô tả lần đầu bị 429 do giới hạn đồng thời, gửi lại đúng mô tả đó một lần nữa). **CHƯA AI NGHE THỬ.** Cần chủ dự án kiểm điều khoản giấy phép ElevenLabs trước khi phát hành.

| File | Giây | Dùng cho | Mô tả gửi đi |
|---|---|---|---|
| sfx_miner_tick.mp3 | 0,13 (tạo 0,5 rồi cắt -t 0.1 + fade) | tick mỗi lần nhảy màu | Single sharp dry click of a steel pickaxe tip striking hard rock, very short, no reverb |
| sfx_miner_lock_rac.mp3 | 1,6 | chốt Rác (hài) | Comedic sad mining sound: a pickaxe hits a pebble with a dull tonk, then a small rock rolls away and a tiny squeaky deflating whistle |
| sfx_miner_lock_thuong.mp3 | 1,0 | chốt Thường | A single clear pickaxe clang on stone with a short echo in a cave |
| sfx_miner_lock_hiem.mp3 | 1,5 | chốt Hiếm | Two rising metallic pickaxe clinks on rock followed by a crystal chime, cave echo |
| sfx_miner_lock_ratHiem.mp3 | 2,5 | chốt Rất hiếm | Rock wall cracking open and crumbling, revealing a sparkling gem with a shimmering crystal arpeggio, deep cave reverb |
| sfx_miner_lock_huyenThoai.mp3 | 4,0 | chốt Huyền thoại | Big distant dynamite blast rumbling through a mine tunnel, rocks tumbling down, then a bright triumphant shimmering crystal chord ringing out in a huge cavern |
| sfx_miner_whoosh.mp3 | 1,2 | bóng vật phẩm phóng to | Swirling whoosh of rock dust and gravel rising upward out of a wooden crate, airy |
| sfx_miner_ambient.mp3 | 4,0 | nền lúc quay | Underground mine ambience: dripping water, distant creaking timber supports, faint rumbling, hollow cave tone |
| sfx_miner_pebbles.mp3 | 1,2 | accent sau khi chốt (số lần tăng theo bậc) | Handful of small pebbles and gravel falling and bouncing on a wooden floor |
| sfx_miner_lantern.mp3 | 1,5 | giai đoạn 1 (Gỡ niêm) | Old oil mining lantern flickering with a small metal handle squeak and sizzle |
| sfx_miner_lid.mp3 | 1,8 | giai đoạn 2 (Hé nắp) | Heavy old wooden crate lid creaking open with dusty grinding of grit in the hinges |
| sfx_miner_cart.mp3 | 1,5 | giai đoạn 4 (Mở toang), nhỏ | Rusty mine cart wheels rolling briefly on iron rails then a bump |

Dùng chung (không làm riêng): `../Synth/sfx_chest_lid_thud.wav` ở giai đoạn 4. Thiếu file Miner nào thì `MinerFxSetup` lấy âm chung cùng vai trò.
| `sfx_miner_rumble_loop.mp3` | 8 | deep underground mine rumble, low creaking timber supports, distant water drips, seamless loop |
| `sfx_miner_thump.mp3` | 0.5 | deep muffled underground thump like a heartbeat in rock |
| `sfx_miner_pick.mp3` | 0.6 | pickaxe striking hard rock, metallic clink and crack |
| `sfx_miner_crumble.mp3` | 1.0 | chunks of rock breaking off and tumbling |
| `sfx_miner_fuse.mp3` | 2.0 | dynamite fuse sizzling and spitting sparks |
| `sfx_miner_dynamite.mp3` | 2.0 | dynamite explosion in a mine tunnel, rocks blasting apart, rumble |
| `sfx_miner_canary.mp3` | 0.8 | canary bird chirping happily |
| `sfx_miner_canary_faint.mp3` | 0.8 | tiny bird letting out a weak squeak and falling, comedic |
| `sfx_miner_nugget.mp3` | 0.4 | gold nugget clinking on rock |

Cắt bằng ffmpeg (ElevenLabs tối thiểu 0,5 s) về đúng độ dài + fade vào/ra 10 ms: `sfx_miner_nugget.mp3` (0.4 s).
| `sfx_miner_key_insert.mp3` | 1.5 | a steel pickaxe striking a rock face once, loud metallic clang with rock crack and falling pebbles |
| `sfx_miner_fly.mp3` | 0.8 | a heavy iron pickaxe thrown end over end through the air, three fast rhythmic whoosh whoosh whoosh spins that slow down slightly, airy swish of a wooden handle, no music, no voice |
| `sfx_miner_action.mp3` | 1.2 | starts instantly with a sharp metal pickaxe striking hard rock, loud clang at the very beginning, a second clang strike a quarter second later, then small stone chips and gravel rattling and trickling down, short dusty tail, no music, no voice |

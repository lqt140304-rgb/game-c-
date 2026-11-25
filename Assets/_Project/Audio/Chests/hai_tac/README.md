# Âm thanh rương Hải tặc (Chest_HaiTac)

Tạo bằng ElevenLabs Sound Generation (`/v1/sound-generation`, mp3 44,1 kHz 128 kbps, `prompt_influence` 0,6) ngày 2026-11-24, theo cho phép của chủ dự án. Mỗi mô tả tạo **một lần** (5 file lần đầu bị lỗi 429 vì gọi song song, đã gọi lại đúng một lần).

**CHƯA AI NGHE THỬ.** Nghe và thay file cùng tên nếu không hợp. Chủ dự án cần kiểm điều khoản giấy phép ElevenLabs trước khi phát hành.

| File | Dài (s) | Dùng ở | Mô tả gửi dịch vụ |
|---|---|---|---|
| `sfx_haitac_tick.mp3` | 0,13 | tick vòng quay (tạo 0,5 s, cắt `-t 0.1` + fade) | single tiny brass compass needle click, small metallic tick, dry, no reverb |
| `sfx_haitac_lock_rac.mp3` | 1,5 | chốt Rác | wet soggy squelch of an old boot pulled out of water, followed by a comedic out-of-tune accordion playing three sad descending notes |
| `sfx_haitac_lock_thuong.mp3` | 1,0 | chốt Thường | single clean ship brass bell ding, short |
| `sfx_haitac_lock_hiem.mp3` | 1,5 | chốt Hiếm | two rising ship bell dings followed by a small ocean wave splash |
| `sfx_haitac_lock_ratHiem.mp3` | 2,0 | chốt Rất hiếm | lively sea shanty fiddle flourish rising quickly, ending with a shower of gold coins |
| `sfx_haitac_lock_huyenThoai.mp3` | 3,5 | chốt Huyền thoại | huge ocean wave crashing like a tsunami roar, then a male sailor choir shouting a triumphant heave ho chord, epic |
| `sfx_haitac_whoosh_grow.mp3` | 1,5 | bóng vật phẩm phóng to | rising swirling sea spray whoosh, water whirlpool swelling upward |
| `sfx_haitac_waves_bed.mp3` | 4,0 | nền dưới vòng quay | gentle ocean waves lapping against a wooden ship hull, distant seagulls |
| `sfx_haitac_laugh.mp3` | 2,0 | giai đoạn Rung: đầu lâu cười | deep raspy cartoon skeleton pirate laughing ha ha ha with rattling bones, comedic |
| `sfx_haitac_seal_crack.mp3` | 0,6 | giai đoạn Gỡ niêm: dấu sáp đỏ vỡ | red wax seal cracking and snapping apart, short |
| `sfx_haitac_skull_creak.mp3` | 1,6 | giai đoạn Hé nắp: nắp sọ kẽo kẹt | bony skull cap creaking open slowly like a rusty wooden hinge |
| `sfx_haitac_cannon.mp3` | 2,0 | HaiTacHook: đại bác (Hiếm 1, Rất hiếm 2, Huyền thoại 3 phát) | distant cannon boom across the open sea, low rolling rumble |
| `sfx_haitac_coins.mp3` | 1,2 | accent + HaiTacHook: xu vàng phun | handful of gold doubloons spilling and bouncing on a wooden deck |
| `sfx_haitac_seagull.mp3` | 1,0 | accent + HaiTacHook: hải âu chế giễu khi ra Rác | a single seagull squawking cry |

Tiếng nắp mở hết (giai đoạn Mở toang) dùng bộ chung: `../Synth/sfx_chest_lid_thud.wav`.

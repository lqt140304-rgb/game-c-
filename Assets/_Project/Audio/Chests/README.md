# Âm thanh rương (ElevenLabs Sound Effects)

Bộ âm dùng chung cho hiệu ứng mở rương, tạo bằng API **ElevenLabs Sound Generation** (`/v1/sound-generation`, mp3 44,1 kHz 128 kbps) theo cho phép của chủ dự án (2026-11-24). Mỗi lần tạo tốn credit ElevenLabs, nên **không tạo lại** khi chưa cần; đổi âm thì thay file cùng tên.

**Chưa ai nghe thử.** File được tạo tự động từ mô tả bên dưới, người dựng chưa nghe. Nghe và thay nếu không hợp.
**Bản quyền:** cần chủ dự án tự kiểm điều khoản giấy phép theo gói ElevenLabs đang dùng trước khi phát hành.

| File | Dài (s) | Mô tả gửi cho dịch vụ |
|---|---|---|
| `sfx_tick.mp3` | 0,5 | short crisp wooden tick click, game UI roulette tick, dry, no reverb |
| `sfx_lock_rac.mp3` | 1,3 | dull low wet thud followed by a sad comedic wah-wah trombone, game result trash item, short |
| `sfx_lock_thuong.mp3` | 1,0 | single soft pleasant bell chime note, game result common item, clean, short |
| `sfx_lock_hiem.mp3` | 1,4 | bright two-note sparkling chime, game result rare item, glittering, short |
| `sfx_lock_ratHiem.mp3` | 1,8 | magical shimmering rising harp arpeggio with airy reverb, game result very rare item, whoosh sparkle |
| `sfx_lock_huyenThoai.mp3` | 2,5 | epic orchestral hit with choir swell and golden shimmering cymbal, legendary item reveal, triumphant |
| `sfx_whoosh_grow.mp3` | 1,5 | rising magical whoosh swell with sparkle, a glowing object growing and floating up out of a chest |
| `sfx_lid_creak.mp3` | 1,5 | old wooden treasure chest lid creaking open slowly, hinge squeak |
| `sfx_coin_jingle.mp3` | 1,2 | gold coins jingling and clinking in a pile, treasure, short (cho rương Golden) |

Âm riêng từng rương sẽ thêm ở các phiên sau, đặt tên `sfx_<tên rương>_<việc>.mp3`.

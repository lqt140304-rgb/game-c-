# Âm riêng rương Rác Quý (`Chest_RacQuy`)

Tạo bằng ElevenLabs Sound Generation (`/v1/sound-generation`, mp3 44,1 kHz 128 kbps, `prompt_influence` 0,6), ngày 2026-11-24, theo cho phép của chủ dự án. Mỗi mô tả tạo **một lần** (vài file bị lỗi 429 "quá nhiều yêu cầu đồng thời" ở lượt đầu nên gửi lại đúng một lần).

**CHƯA AI NGHE THỬ.** Nghe và thay file cùng tên nếu không hợp. **Giấy phép:** chủ dự án tự kiểm điều khoản gói ElevenLabs trước khi phát hành.

| File | Dài (s) | Dùng ở | Mô tả gửi cho dịch vụ |
|---|---|---|---|
| `sfx_racquy_tick.mp3` | 0,13 | tick vòng quay | single tiny hollow tin can tap, short dry metallic tick, no reverb (tạo 0,5 s, cắt `ffmpeg -t 0.1` + fade 20 ms) |
| `sfx_racquy_lock_rac.mp3` | 2,2 | chốt Rác | comedic sad trombone wah wah wah descending followed by a tired exasperated human sigh, cartoon fail |
| `sfx_racquy_lock_thuong.mp3` | 1,0 | chốt Thường | single empty tin can dropped on pavement, bright clank and small bounce, comedic |
| `sfx_racquy_lock_hiem.mp3` | 1,4 | chốt Hiếm | two glass bottles clinking together in a cheerful rising pair, followed by a small sparkle |
| `sfx_racquy_lock_ratHiem.mp3` | 2,2 | chốt Rất hiếm | rattling trash can lid spinning on the floor and settling, then a magical rising harp glissando shimmer |
| `sfx_racquy_lock_huyenThoai.mp3` | 3,4 | chốt Huyền thoại | garbage bin lid clatters open then a heavenly angelic choir sings a sustained holy ahh with bright shimmering bells, divine revelation |
| `sfx_racquy_whoosh.mp3` | 1,4 | bóng phóng to | wet squelchy rising whoosh, something sliding up out of a pile of garbage with a cartoon pop at the end |
| `sfx_racquy_gurgle.mp3` | 2,5 | hook: đầu vòng quay | thick bubbling gurgle of rotten sludge in a metal bin, low wet bubbles, gross and comedic |
| `sfx_racquy_flies.mp3` | 3,0 | hook: đầu vòng quay | a few houseflies buzzing around in circles close by, buzzing loop |
| `sfx_racquy_can_roll.mp3` | 1,6 | hook (giữa vòng quay) + accent sau chốt | crushed aluminium can rolling and rattling across a hard floor then stopping |
| `sfx_racquy_fart.mp3` | 0,7 | hook: chốt Rác | short comedic cartoon fart squeak, silly game sound |
| `sfx_racquy_tape_rip.mp3` | 0,8 | giai đoạn 1 Gỡ niêm | duct tape being ripped off quickly, sticky tearing sound |
| `sfx_racquy_lid_pop.mp3` | 1,2 | giai đoạn 2 Hé nắp | dented metal trash can lid popping loose with a hollow bong and a puff of escaping gas |
| `sfx_racquy_lid_clang.mp3` | 1,3 | giai đoạn 4 Mở toang | metal trash can lid falling and clanging on the ground, wobbling to a stop |

Âm nền lúc quay dùng bộ chung (`Synth/sfx_chest_open_shimmer.wav`).

## meo/ — kịch bản "Mèo hoang lật thùng rác" (2026-11-25)

WAV tổng hợp bằng `Tools/Audio/make_racquy_meo_sfx.py` (numpy, không mẫu, không bản quyền). **Chưa ai nghe.** Thay bằng âm thật cùng tên nếu cần.
build_loop (lặp 2 s, code tăng pitch/âm lượng), meow, meow_wants, yowl, hiss, clatter, clank, deflate (bậc Rác "xịt"), boom (bậc cao), throw.
| `sfx_racquy_bed_loop.mp3` | 8 | junk rattling inside a dented metal trash bin, tin cans clinking and lid tapping in a playful nervous rhythm, seamless loop |
| `sfx_racquy_meow.mp3` | 1.0 | orange street cat meowing curiously, single meow |
| `sfx_racquy_meow_wants.mp3` | 1.0 | hungry cat begging meow, eager and cute |
| `sfx_racquy_yowl.mp3` | 1.2 | cat screeching yowl as it is flung into the air, comedic |
| `sfx_racquy_hiss.mp3` | 0.8 | angry cat hissing |
| `sfx_racquy_clatter.mp3` | 1.5 | metal trash bin tipping over, cans bottles and junk clattering across the ground |
| `sfx_racquy_clank.mp3` | 0.4 | single empty tin can clank, short |
| `sfx_racquy_deflate.mp3` | 1.2 | comedic balloon deflating with a sad descending squeal and a tiny pop at the end |
| `sfx_racquy_boom.mp3` | 1.5 | muffled cartoon explosion inside a trash bin, boomy and funny |
| `sfx_racquy_throw.mp3` | 0.6 | quick whoosh of a small object thrown through the air |

Cắt bằng ffmpeg (ElevenLabs tối thiểu 0,5 s) về đúng độ dài + fade vào/ra 10 ms: `sfx_racquy_clank.mp3` (0.4 s).
| `sfx_racquy_key_insert.mp3` | 1.5 | an orange alley cat meowing then pouncing and scratching on a metal trash bin lid, soft thud and quick claw scratches |

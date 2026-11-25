# Âm kịch bản luân lưu (TheThaoHook)

Tổng hợp bằng `Tools/Audio/make_thethao_match_sfx.py` (numpy, không mẫu thu, tất định). **CHƯA AI NGHE THỬ** — có thể thay bằng bản thu thật cùng tên file.
`TheThaoFxPreset.FillHookClips` (gọi từ OnValidate và menu TheThaoFxSetup) tự điền vào hook.

| File | Lúc phát |
|---|---|
| whistle_start | ngay khi bấm mở (còi khai cuộc); phím B (cao hơn) |
| crowd_loop | nền suốt trận, to/cao dần; bậc Rác bị **cắt phụt** |
| drum | trống cổ vũ, nhịp dồn 0,7 s → 0,13 s tới cú sút quyết định |
| riser | ~2 s trước chốt, dâng lên; tắt khi sút |
| kick / bounce / post_clang / net_swish | sút, nảy, chạm cột/xà, vào lưới |
| crowd_ooh | bóng chạm cột (suýt vào) |
| crowd_roar / airhorn | bàn thắng Hiếm+ / Rất hiếm+ (airhorn ưu tiên mp3 ElevenLabs có sẵn) |
| crowd_aww / deflate / sad_trombone / whistle_end | kết thúc bậc Rác (xịt, hài) |
| var_beep / rewind | VAR chiếu lại (Rất hiếm, Huyền thoại) |

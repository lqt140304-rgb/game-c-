# Âm "Rương Đói" (Chest_Mimic, MimicFeedHook)

Tổng hợp bằng code (numpy + scipy, không mẫu, không dịch vụ trả phí, không vướng bản quyền):
`python3 Tools/Audio/make_mimic_feed_sfx.py`. Hạt giống cố định → chạy lại ra y hệt.

**CHƯA AI NGHE THỬ.** Chỉ kiểm bằng code (độ dài, đỉnh -1 dB). Âm tổng hợp nghe sẽ "điện tử"; nếu dở có thể
thay file cùng tên (ví dụ tạo ElevenLabs như thư mục `mimic/`).

| File | Dài | Dùng khi |
|---|---|---|
| sfx_mf_wake.wav | 0,7 | NGAY khi bấm mở: khịt mũi + gầm |
| sfx_mf_stomach_loop.wav | 4,0 (lặp) | bụng réo suốt màn, cao + to dần theo độ căng |
| sfx_mf_heartbeat.wav | 0,4 | tim đập, nhịp 0,95 s → 0,26 s |
| sfx_mf_chomp / chew / gulp | 0,2–0,35 | ngoạm, nhai, nuốt |
| sfx_mf_burp_small / burp_big | 0,45 / 1,3 | ợ |
| sfx_mf_riser.wav | 4,0 | dâng lên trước chốt (giống nhau mọi bậc) |
| sfx_mf_deflate.wav | 1,4 | bậc THẤP: xì hơi xịt tụt cao độ |
| sfx_mf_boom.wav | 2,5 | bậc CAO: nổ (và cú đáp đất) |
| sfx_mf_spit / toss / hop / giggle | ngắn | nhổ, ném, nhảy, lạch cạch răng khi né |
| sfx_mf_buzz_loop.wav | 1,0 (lặp) | con ruồi |

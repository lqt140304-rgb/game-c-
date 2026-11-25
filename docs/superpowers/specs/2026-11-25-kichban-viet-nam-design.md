# Kịch bản mở rương: Chest_VietNam — "ĐÊM HỘI LÀNG"

Trạng thái: **đã thu gọn theo kế hoạch 2026-11-25 (mục 5.8)**, biên dịch thử bằng stub (OK), **CHƯA chạy trong Unity, CHƯA ai nghe âm.**
Thay hook cũ `VietNamHook` bằng `VietNamHoiLangHook`. File cũ giữ nguyên, không còn được gắn.

## Ý tưởng

Rương thúng tre đội nón lá giữa đêm hội. Vài **hoa đăng** bay cong từ quanh rương vào miệng rương; cả rương bập bềnh nhẹ và chao
mỗi khi có đèn vào. Trống – mõ – đàn bầu dâng dần. Lúc chốt bậc, câu chuyện rẽ nhánh: Rác thì đàn bầu tắt, im, "xịt", rương hụt
xuống và ếch nhảy lên nắp "ộp" (hài); bậc cao thì trống đồng "BÙM", đèn trời trồi khỏi miệng rương bay lên, rương nhảy lên.

**Đã bỏ** (quá lố so với thước đo): mặt nước, rùa vàng, chim Lạc + bánh chưng, gợn nước, 12 đèn trôi vật lý, cục va của rương.

## Nhịp (khoá dự kiến T ≈ 6,5–7 s; S = cạnh lớn nhất khung bao rương, H = chiều cao)

| Giây | Sự kiện |
|---|---|
| 0 (bấm mở) | Đàn bầu ngân (lặp, vol 0,15, pitch 0,85) bắt đầu ngay. Trống gõ 0,75 s/nhịp (vol 0,25). Rương bập bềnh (dịch ≤ 0,015 S, nghiêng ≤ 5°) |
| 0,05 → 0,75 T | **5 hoa đăng tự thả**: xuất phát ở vòng 1,4 S quanh rương, cao ngang miệng, bay cong (Bezier 3 điểm, đỉnh cao hơn miệng 0,4 S) 1,2 s vào miệng, thu nhỏ về 0 trong 0,2 s rồi ẩn. Đèn thứ i **vào miệng** lúc (0,15 + 0,15 i) × T. Mỗi đèn vào: rương chao nhẹ + tiếng `plopClip` 0,35 (qua `TryGate`, cách ≥ 0,2 s) |
| 0 → khoá | Độ căng k = t / T (+0,05 mỗi đèn người chơi). Nhịp nhanh dần 0,75 → 0,15 s (vol 0,25 → 0,55); từ k > 0,4 mõ chen nhịp lẻ; đàn bầu pitch 0,85 → 1,35 (+59%), vol 0,15 → 0,45; đom đóm quanh miệng (≤ 3 hạt/khung). Giống hệt nhau mọi bậc |
| Khoá: **Rác** | Đàn bầu + trống **dừng ngay** → lặng 0,2 s → **1 tiếng xịt** (`deflateClip` 0,9) → rương hụt xuống 0,15 H rồi lên → ếch nhảy từ 1,2 S lên nắp (0,6 s) kêu "ộp" (đuôi 0,4), ở lại 1 s rồi biến mất. Không có HĐ, không đèn trời |
| Khoá: **Thường** | Nhịp dừng ngay, đàn bầu fade 0,3 s. TC = `stingClips[1]` 0,6 + HĐ mõ lẻ `moClip` pitch 0,7 vol 0,4. **1 đèn** trồi khỏi miệng, bay lên 1,5 S rồi mờ tắt. Rương hụt 0,06 H |
| Khoá: **Hiếm / Rất hiếm / HT** | Nhịp + đàn bầu dừng ngay. TC = `stingClips[bậc]` (0,85 / 1,0 / 1,0) + HĐ `boomClip` trống đồng (0,7 / 0,8 / 0,8). **1 / 2 / 3 đèn trời** trồi khỏi miệng (cách nhau 0,25 s), bay lên 2,5 S với tốc độ cố định rồi mờ. Rương nhảy `H × (0,25 + 0,1 × hạng)`, Huyền thoại lơ lửng 1,6 s (các bậc khác 0,9 s). Đom đóm nổ 40 / 50 / 120 hạt |

## Tương tác phụ

- **Phím L** (hoặc `VietNamHoiLangHook.OfferLantern()` từ nút UI): thả thêm 1 hoa đăng, bay vào miệng rương ngay (vào sau 1,2 s), **tối đa 2 lần**.
  Mỗi đèn đẩy độ căng thêm 0,05. Không ảnh hưởng bậc.

## Bất biến

- Hook không đọc bậc trước `OnTierLocked`. Ngẫu nhiên (`System.Random`, trường `seed`, 0 = mỗi lần khác) chỉ cho góc xuất phát và màu cánh đèn.
- Vòng màu 5 bậc và bóng vật phẩm do `ChestRevealSequence` làm. Hạt đom đóm chỉ qua `ChestFxFactory.CreateParticles`, không đặt orbital/velocity.
- Reset (`OnReset`/`OnDestroy`) dừng mọi âm, huỷ mọi đạo cụ (đèn, ếch, đom đóm, vật liệu) và trả rương về đúng tư thế.
- `profile.tension.enabled = false`; `VietNamHoiLangSetup.CreateOrUpdateProfile` gọi `ChestAudioRule.ApplyToProfile` MỖI lần chạy menu.

## Bản chốt (số thực tế sau khi sửa)

| Hạng mục | Giá trị |
|---|---|
| Diễn viên | 1 (ếch, chỉ Rác) + hoa đăng (đồ bay vào rương) |
| Hoa đăng | 5 tự + ≤ 2 người chơi, xuất phát 1,4 S, đều vào miệng rương |
| Đèn trời lúc kết | Thường 1 · Hiếm 1 · RH 2 · HT 3, cao ≤ 2,5 S |
| Light | 1 (`maxLights = 1`: chỉ đèn đầu tiên) |
| Rigidbody | trước chốt ≤ 7 (đèn kinematic, không tính vật lý); không còn mặt nước / rùa / chim / collider |
| Hạt | 1 hệ đom đóm; nổ ≤ 60 (HT 120) |
| Mặt nước / rùa / chim | bỏ |
| Tổng màn | chốt ≈ 6,5–7 s + kết ≤ 3 s |
| `tension.enabled` | false |

## Âm (chỉ .mp3 ElevenLabs, `Audio/Chests/viet_nam/`)

| Ô trên hook | File | Vai trò |
|---|---|---|
| `droneClip` | `sfx_vietnam_dan_bau_loop.mp3` | Nền (lặp) |
| `drumClip`, `moClip` | `sfx_vietnam_trong.mp3`, `sfx_vietnam_tick_mo.mp3` | Nhịp |
| `plopClip` | `sfx_vietnam_lantern_in.mp3` | Sự kiện (đèn vào) |
| `stingClips[0..4]` | `sfx_vietnam_lock_*.mp3` | TC theo bậc (Thường→HT; Rác dùng `deflateClip`, `stingClips[0]` chỉ là dự phòng) |
| `deflateClip` | `sfx_vietnam_dan_bau_xit.mp3` | TC Rác (xịt) |
| `boomClip` | `sfx_vietnam_trong_dong.mp3` | HĐ Hiếm trở lên |
| `frogClip` | `sfx_vietnam_frog.mp3` | Đuôi Rác |

Lớp kéo dài trước chốt: 2 (nền + nhịp). Hook dùng đúng 3 `AudioSource` (nền, nhịp, sfx); tiếng mõ cần pitch 0,7 phát qua
`ChestRevealSequence.PlayOneShot` (không đổi pitch nguồn đang ngân). `volume` tổng nhân một lần trong `Vol()` và bị chặn bởi trần `ChestAudioRule`.

## Kỹ thuật

- `Chests/VietNamHoiLangHook.cs` — đạo diễn; `VietNamHoiLangFloater.cs` — hoa đăng (bay Bezier vào miệng, thu nhỏ/ẩn, hoặc trồi lên làm đèn trời); `VietNamHoiLangProps.cs` — dựng đèn bằng khối cơ bản.
- Tư thế cả rương: cộng thêm lên localPosition/localRotation mà `BoxOpenSequence` ghi mỗi khung; trước chốt nghiêng kẹp ≤ 8°.
- Kiểm biên dịch: `bash Tools/CompileCheck/check_all.sh` và `bash Tools/CompileCheck/check_viet_nam_hoi_lang.sh` (cần `mcs`).

## Cách thử trong Unity

1. Menu **Tools/Cho Do La/Create Viet Nam Hoi Lang FX** (dựng `VietNamHoiLangFx.prefab` nạp `.mp3`, gắn làm customFxPrefab, ghi lại hồ sơ câm).
2. Mở cảnh thử rương, chọn Chest_VietNam, phím P/T mở với từng bậc. Bấm **L** tối đa 2 lần để thả thêm đèn.

## Chưa kiểm (trung thực)

- **Chưa chạy Unity**, chưa nghe âm (mọi file ElevenLabs chưa ai nghe; cần kiểm giấy phép), chưa đo hiệu năng.
- Cần xem bằng mắt: đường bay Bezier có chui qua thân rương không; đèn trời trồi đúng miệng khi rương nhảy; ếch đáp đúng nắp; rương không trôi lệch sau reset.
- Cần nghe: vòng `dan_bau_loop` có "tách" ở điểm nối không; Rác có thật sự im 0,2 s rồi xịt; cân bằng nền/nhịp/chốt.
- Rác phát `deflateClip` làm TC (đúng mục 5.8) nên `stingClips[0]` (lock_rac) không phát; nếu muốn dùng lock_rac thì đổi một dòng ở `LowEnding`.
- Tiếng trống 0,6 s ở nhịp 0,15 s sát chốt sẽ chồng lên nhau trong cùng một nguồn; nghe thử xem có đục không.

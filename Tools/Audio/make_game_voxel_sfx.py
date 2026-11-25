#!/usr/bin/env python3
"""Tạo bộ âm chiptune 8-bit cho kịch bản mở rương Chest_Game "VOXEL 8-BIT".

Toàn bộ dựng bằng sóng vuông / tam giác / nhiễu (numpy), không mẫu, không dịch vụ trả phí, không giai điệu có sẵn.
Ghi WAV mono 22,05 kHz 16-bit (thô kiểu máy 8-bit) vào Assets/_Project/Audio/Chests/game/voxel/.
Tất định (hạt giống cố định). CHƯA AI NGHE THỬ.

    python3 Tools/Audio/make_game_voxel_sfx.py [thư_mục_ra]
"""
import os
import sys
import wave

import numpy as np

SR = 22050
RNG = np.random.default_rng(20261125)


def n2f(n):
    """Số nốt MIDI -> tần số."""
    return 440.0 * 2 ** ((n - 69) / 12)


def t_axis(sec):
    return np.arange(int(SR * sec)) / SR


def square(freq, sec, duty=0.5):
    t = t_axis(sec)
    if np.ndim(freq) == 0:
        ph = (freq * t) % 1.0
    else:
        ph = np.cumsum(freq) / SR % 1.0
    return np.where(ph < duty, 1.0, -1.0)


def tri(freq, sec):
    t = t_axis(sec)
    ph = (freq * t) % 1.0 if np.ndim(freq) == 0 else np.cumsum(freq) / SR % 1.0
    return 4 * np.abs(ph - 0.5) - 1


def lfsr_noise(sec, rate=8000):
    """Nhiễu kiểu chip: giá trị ngẫu nhiên giữ theo bước (rate thấp = trầm, rè)."""
    n = int(SR * sec)
    step = max(1, int(SR / rate))
    v = RNG.choice([-1.0, 1.0], n // step + 1)
    return np.repeat(v, step)[:n]


def env(sec, attack=0.003, release=0.04):
    n = int(SR * sec)
    e = np.ones(n)
    a = min(n, int(SR * attack))
    r = min(n, int(SR * release))
    if a > 0:
        e[:a] = np.linspace(0, 1, a)
    if r > 0:
        e[-r:] *= np.linspace(1, 0, r)
    return e


def decay(sec, tau):
    return np.exp(-t_axis(sec) / tau)


def note(n, sec, wave_fn=square, duty=0.5, vol=0.5):
    f = n2f(n)
    x = wave_fn(f, sec, duty) if wave_fn is square else wave_fn(f, sec)
    return x * env(sec) * vol


def seq(notes, step, **kw):
    """Chuỗi nốt đều nhau; None = nghỉ."""
    out = []
    for n in notes:
        out.append(np.zeros(int(SR * step)) if n is None else note(n, step, **kw))
    return np.concatenate(out)


def mix(*parts):
    n = max(len(p) for p in parts)
    y = np.zeros(n)
    for p in parts:
        y[:len(p)] += p
    return y


def pad(x, sec):
    return np.concatenate([x, np.zeros(int(SR * sec))])


def crush(x, bits=6):
    q = 2 ** (bits - 1)
    return np.round(x * q) / q


def save(path, x):
    x = np.asarray(x, dtype=float)
    peak = np.max(np.abs(x)) or 1.0
    x = crush(x / peak * 0.85, 8)
    data = (x * 32767).astype(np.int16)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())
    print(f"{os.path.basename(path)}: {len(x) / SR:.2f}s")


# ------------------------------------------------------------------ các âm

def insert_coin():
    # bấm mở: "bíp-bíp" chèn xu, hai nốt nhảy quãng 4
    return seq([83, 88], 0.07, duty=0.25, vol=0.6)


def count_beep():
    return note(72, 0.12, duty=0.5, vol=0.6)


def count_go():
    return mix(note(84, 0.45, duty=0.25, vol=0.5), note(72, 0.45, wave_fn=tri, vol=0.5))


def loop_bar():
    # 1 ô nhịp 8 nốt móc kép (0,96 s): bass tam giác + rải hợp âm vuông. Không là giai điệu có sẵn.
    step = 0.06
    arp = [60, 64, 67, 72, 67, 64] * 2 + [62, 65, 69, 74]
    bass = [36, None, 36, None, 43, None, 43, None]
    a = seq(arp, step, duty=0.125, vol=0.25)
    b = seq(bass, step * 2, wave_fn=tri, vol=0.6)
    hat = np.concatenate([lfsr_noise(0.02, 12000) * decay(0.02, 0.006) * 0.25 if i % 2 == 0 else np.zeros(int(SR * 0.02))
                          for i in range(8)])
    hat = np.concatenate([np.concatenate([h, np.zeros(int(SR * (0.12 - 0.02)))]) for h in np.split(hat, 8)])
    return mix(a, b, hat)


def bump():
    # húc đầu vào khối: tiếng "cốp" trầm + nhiễu ngắn
    f = np.linspace(220, 70, int(SR * 0.12))
    body = square(f, 0.12, 0.5) * decay(0.12, 0.04) * 0.7
    return mix(body, lfsr_noise(0.05, 3000) * decay(0.05, 0.015) * 0.5)


def jump():
    f = np.linspace(n2f(62), n2f(79), int(SR * 0.18))
    return square(f, 0.18, 0.25) * env(0.18) * 0.5


def coin():
    return np.concatenate([note(83, 0.06, duty=0.25), note(88, 0.30, duty=0.25) * decay(0.30, 0.12)])


def voxel_tick():
    return note(76, 0.04, duty=0.125, vol=0.5)


def shatter():
    # vỡ thành voxel: nhiễu rè hạ dần + tiếng vuông rơi
    x = lfsr_noise(0.6, 4000) * decay(0.6, 0.15)
    f = np.linspace(600, 60, int(SR * 0.6))
    return mix(x * 0.7, square(f, 0.6, 0.5) * decay(0.6, 0.2) * 0.4)


def reassemble():
    # các khối bay về: quét lên lắp bậc thang (glissando theo nửa cung)
    notes = list(range(48, 84, 2))
    return seq(notes, 0.035, duty=0.25, vol=0.45)


def power_up():
    # "nấm": rải lên 3 quãng tám, kiểu nâng cấp (tự soạn)
    notes = []
    for o in (0, 5, 10):
        notes += [55 + o, 59 + o, 62 + o]
    return seq(notes, 0.045, duty=0.5, vol=0.5)


def one_up():
    # "xu thêm mạng": 6 nốt nhảy lên, tự soạn
    return seq([76, 79, 88, 84, 86, 91], 0.09, duty=0.25, vol=0.5)


def end_rac():
    # GAME OVER hài: đi xuống, chậm dần, rồi "xịt" (nhiễu rè trầm bị bóp tắt)
    a = seq([67, 66, 65], 0.22, duty=0.5, vol=0.5)
    f = np.linspace(n2f(64), n2f(40), int(SR * 0.9))
    slide = square(f, 0.9, 0.5) * env(0.9, release=0.2) * 0.5
    pfft = lfsr_noise(0.5, 900) * decay(0.5, 0.12) * 0.6
    return np.concatenate([a, slide, np.zeros(int(SR * 0.08)), pfft])


def end_tier(rank):
    """Bậc 1..4 (Thường..Huyền thoại): fanfare to dần theo bậc."""
    base = 72
    if rank == 1:  # Thường: 2 nốt
        return np.concatenate([note(base, 0.1, duty=0.25), note(base + 7, 0.4, duty=0.25) * decay(0.4, 0.2)])
    if rank == 2:  # Hiếm: rải 3 nốt + hợp âm
        a = seq([base, base + 4, base + 7], 0.08, duty=0.25, vol=0.5)
        ch = mix(note(base + 12, 0.6, duty=0.25, vol=0.35), note(base + 7, 0.6, wave_fn=tri, vol=0.4))
        return np.concatenate([a, ch * decay(0.6, 0.35)])
    if rank == 3:  # Rất hiếm: rải nhanh 2 quãng tám + trống
        a = seq([base + i for i in (0, 4, 7, 12, 16, 19, 24)], 0.06, duty=0.125, vol=0.5)
        ch = mix(note(base + 12, 1.0, duty=0.25, vol=0.35), note(base + 16, 1.0, duty=0.5, vol=0.25),
                 note(base - 12, 1.0, wave_fn=tri, vol=0.5))
        drum = lfsr_noise(0.3, 2500) * decay(0.3, 0.08) * 0.5
        return np.concatenate([a, mix(ch * decay(1.0, 0.5), drum)])
    # Huyền thoại: trống cuộn dâng -> nổ -> fanfare 3 hợp âm -> tia lửa
    roll_t = 0.8
    roll = lfsr_noise(roll_t, 5000) * np.linspace(0.1, 0.8, int(SR * roll_t))
    roll *= (np.sin(2 * np.pi * np.linspace(6, 30, int(SR * roll_t)).cumsum() / SR) > 0)
    boom = lfsr_noise(0.6, 1500) * decay(0.6, 0.2)
    chords = []
    for root, dur in ((base, 0.18), (base + 5, 0.18), (base + 7, 0.18), (base + 12, 1.4)):
        c = mix(note(root, dur, duty=0.25, vol=0.3), note(root + 4, dur, duty=0.5, vol=0.25),
                note(root + 7, dur, duty=0.125, vol=0.25), note(root - 24, dur, wave_fn=tri, vol=0.5))
        if dur > 1:
            c *= decay(dur, 0.8)
        chords.append(c)
    fan = np.concatenate(chords)
    sparkle = np.concatenate([note(int(RNG.integers(88, 100)), 0.03, duty=0.125, vol=0.25) for _ in range(30)])
    return np.concatenate([roll, mix(boom, fan, np.concatenate([np.zeros(int(SR * 0.75)), sparkle]))])


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(here, "../../Assets/_Project/Audio/Chests/game/voxel")
    os.makedirs(out, exist_ok=True)
    clips = {
        "sfx_vx_insert_coin": insert_coin(),
        "sfx_vx_count_beep": count_beep(),
        "sfx_vx_count_go": count_go(),
        "sfx_vx_loop_bar": loop_bar(),
        "sfx_vx_bump": bump(),
        "sfx_vx_jump": jump(),
        "sfx_vx_coin": coin(),
        "sfx_vx_voxel_tick": voxel_tick(),
        "sfx_vx_shatter": shatter(),
        "sfx_vx_reassemble": reassemble(),
        "sfx_vx_power_up": power_up(),
        "sfx_vx_one_up": one_up(),
        "sfx_vx_end_0_rac": end_rac(),
        "sfx_vx_end_1_thuong": end_tier(1),
        "sfx_vx_end_2_hiem": end_tier(2),
        "sfx_vx_end_3_rathiem": end_tier(3),
        "sfx_vx_end_4_huyenthoai": end_tier(4),
    }
    for name, x in clips.items():
        save(os.path.join(out, name + ".wav"), x)


if __name__ == "__main__":
    main()

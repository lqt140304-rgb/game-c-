#!/usr/bin/env python3
"""Tạo âm riêng cho kịch bản "Trận pháp Rune" của Chest_Magic (tổng hợp bằng code, không mẫu, không bản quyền).

    python3 Tools/Audio/make_magic_rune_sfx.py [thư_mục_ra]

Mặc định ghi vào Assets/_Project/Audio/Chests/magic_rune/ (WAV mono 44,1 kHz 16-bit, hạt giống cố định).
CHƯA AI NGHE THỬ trong game: nghe và chỉnh tham số bên dưới nếu không hợp.

- sfx_magicrune_build.wav : 10 s "tụ phép" phát NGAY KHI BẤM MỞ. Giống hệt nhau cho mọi bậc (giữ bí mật):
  trống trầm + nhịp đá rune gõ nhanh dần + tiếng ù dâng cao độ + gió xoáy to dần.
- sfx_magicrune_stone.wav : một viên rune rơi đúng chỗ (cạch đá + chuông pha lê).
- sfx_magicrune_wisp.wav  : tiếng líu ríu của đốm linh hồn.
- sfx_magicrune_end_<n>_<bậc>.wav : kết theo bậc. 0 Rác = "xịt" hài (tụt cao độ + bụp + kêu ré),
  1..4 dâng dần tới bùng nổ (Huyền thoại: nổ trầm + hợp âm trưởng + lấp lánh dài).
"""
import os
import sys
import wave

import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)


def t_axis(s):
    return np.arange(int(SR * s)) / SR


def sweep(f0, f1, s, curve=1.0):
    t = t_axis(s) / s
    f = f0 + (f1 - f0) * t ** curve
    return np.sin(2 * np.pi * np.cumsum(f) / SR)


def env(s, attack, decay):
    t = t_axis(s)
    return np.clip(t / max(attack, 1e-4), 0, 1) * np.exp(-t / decay)


def lp(x, k):
    """Low-pass rẻ: trung bình trượt k mẫu."""
    k = max(1, int(k))
    return np.convolve(x, np.ones(k) / k, mode="same")


def chime(freq, s=1.2, decay=0.35, partials=(1, 2.76, 5.4)):
    out = np.zeros(int(SR * s))
    for i, p in enumerate(partials):
        out += np.sin(2 * np.pi * freq * p * t_axis(s)) * env(s, 0.002, decay / (1 + i)) / (1 + i)
    return out


def knock(s=0.25, freq=140):
    n = lp(RNG.uniform(-1, 1, int(SR * s)), 18) * env(s, 0.001, 0.03) * 2
    return n + sweep(freq * 1.6, freq, s) * env(s, 0.001, 0.06)


def put(dst, src, at, gain=1.0):
    i = int(at * SR)
    if i >= len(dst):
        return
    n = min(len(src), len(dst) - i)
    dst[i:i + n] += src[:n] * gain


def write(path, x, peak=0.9):
    x = x / max(1e-9, np.max(np.abs(x))) * peak
    fade = min(len(x), int(SR * 0.01))
    x[-fade:] *= np.linspace(1, 0, fade)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((x * 32767).astype(np.int16).tobytes())
    print("wrote", path, f"{len(x) / SR:.2f}s")


def build(seconds=10.0):
    n = int(SR * seconds)
    t = t_axis(seconds)
    u = t / seconds
    out = np.zeros(n)
    # ù trầm dâng cao độ (55 -> 165 Hz) và to dần
    out += sweep(55, 165, seconds, 1.6) * (0.25 + 0.5 * u) * 0.6
    out += sweep(82.5, 247, seconds, 1.6) * (0.1 + 0.4 * u ** 2) * 0.35
    # gió xoáy: nhiễu lọc, sáng dần
    wind = RNG.uniform(-1, 1, n)
    wind = lp(wind, 40) * (1 - u) + lp(wind, 6) * u
    out += wind * (0.05 + 0.5 * u ** 2) * (0.7 + 0.3 * np.sin(2 * np.pi * (1 + 6 * u) * t))
    # nhịp: khoảng cách từ 0.7 s rút còn 0.07 s; cao độ chuông leo theo thang ngũ cung
    scale = [0, 3, 5, 7, 10]
    at, k = 0.0, 0
    while at < seconds - 0.05:
        uu = at / seconds
        put(out, knock(0.25, 90 + 60 * uu), at, 0.6 + 0.4 * uu)
        step = scale[k % 5] + 12 * (k // 5 % 3)
        put(out, chime(330 * 2 ** (step / 12), 0.6, 0.12), at, 0.15 + 0.35 * uu)
        at += 0.7 * (1 - uu) ** 1.6 + 0.07
        k += 1
    return out


def stone():
    s = 0.9
    x = knock(0.3, 110) * 1.2
    x = np.pad(x, (0, int(SR * s) - len(x)))
    put(x, chime(660, 0.8, 0.25), 0.03, 0.5)
    put(x, chime(990, 0.8, 0.18), 0.05, 0.25)
    return x


def wisp():
    s = 0.7
    f = 1300 + 500 * np.sin(2 * np.pi * 14 * t_axis(s)) + 900 * t_axis(s) / s
    return np.sin(2 * np.pi * np.cumsum(f) / SR) * env(s, 0.02, 0.25)


def end_rac():
    s = 1.8
    x = np.zeros(int(SR * s))
    # năng lượng tụt cao độ như bóng xì hơi
    dive = sweep(700, 60, 0.7, 0.5) * np.linspace(1, 0.2, int(SR * 0.7))
    flutter = 1 + 0.5 * np.sin(2 * np.pi * 28 * t_axis(0.7))
    put(x, dive * flutter, 0.0, 0.6)
    # "bụp" nhỏ khô
    put(x, knock(0.2, 220), 0.72, 0.8)
    # tiếng ré tí hon như đồ chơi cao su
    put(x, sweep(1500, 1900, 0.12) * env(0.12, 0.005, 0.05), 1.0, 0.25)
    put(x, sweep(1300, 900, 0.18) * env(0.18, 0.005, 0.08), 1.14, 0.2)
    return x


def end_up(tier):
    lens = {1: 1.4, 2: 1.8, 3: 2.4, 4: 3.6}
    s = lens[tier]
    x = np.zeros(int(SR * s))
    root = 440
    notes = {1: [0], 2: [0, 7], 3: [0, 4, 7, 12], 4: [0, 4, 7, 12, 16, 19]}[tier]
    for i, st in enumerate(notes):
        put(x, chime(root * 2 ** (st / 12), s, 0.4 + 0.15 * tier), 0.06 * i, 0.6)
    if tier >= 2:  # lấp lánh
        sp = RNG.uniform(-1, 1, len(x))
        sp = sp - lp(sp, 4)
        x += sp * env(s, 0.02, 0.25 * tier) * 0.15 * tier
    if tier >= 3:  # nổ trầm
        boom = sweep(120, 35, 0.9, 0.5) * env(0.9, 0.002, 0.25)
        put(x, boom, 0.0, 0.5 + 0.4 * (tier - 3))
        put(x, lp(RNG.uniform(-1, 1, int(SR * 0.6)), 8) * env(0.6, 0.001, 0.12), 0.0, 0.6)
    if tier == 4:  # hợp âm trưởng phồng dần (như dàn hợp xướng tổng hợp)
        pad = np.zeros(len(x))
        for st in (0, 4, 7, 12):
            for det in (-0.004, 0.0, 0.004):
                pad += np.sin(2 * np.pi * 220 * 2 ** (st / 12) * (1 + det) * t_axis(s))
        sw = np.clip(t_axis(s) / 0.6, 0, 1) * np.exp(-np.clip(t_axis(s) - 1.6, 0, None) / 0.9)
        x += lp(pad, 3) * sw * 0.12
    return x


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(here, "..", "..", "Assets", "_Project", "Audio", "Chests", "magic_rune")
    os.makedirs(out, exist_ok=True)
    write(os.path.join(out, "sfx_magicrune_build.wav"), build())
    write(os.path.join(out, "sfx_magicrune_stone.wav"), stone())
    write(os.path.join(out, "sfx_magicrune_wisp.wav"), wisp(), 0.5)
    write(os.path.join(out, "sfx_magicrune_end_0_rac.wav"), end_rac(), 0.75)
    names = {1: "thuong", 2: "hiem", 3: "rathiem", 4: "huyenthoai"}
    for k, v in names.items():
        write(os.path.join(out, f"sfx_magicrune_end_{k}_{v}.wav"), end_up(k), 0.6 + 0.1 * k)


if __name__ == "__main__":
    main()

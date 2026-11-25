#!/usr/bin/env python3
"""Tạo âm riêng cho Chest_Golden "Máy Xèng Kho Báu" (tổng hợp bằng numpy, không mẫu, không bản quyền).

    python3 Tools/Audio/make_golden_slot_sfx.py [thư_mục_ra]

Mặc định ghi vào Assets/_Project/Audio/Chests/golden/ (WAV mono 44.1 kHz 16-bit, hạt giống cố định).
Các file:
  sfx_golden_lever.wav     tiếng gạt cần: kim loại "cạch" + lò xo bật (phát NGAY khi bấm mở)
  sfx_golden_spin_loop.wav vòng lặp ô quay rè rè + tích tắc (hook tăng pitch/volume dần -> độ căng)
  sfx_golden_reel_stop.wav tiếng "thụp" một ô dừng
  sfx_golden_fizzle.wav    bậc THẤP: kèn hụt "uơ-uơ-uơơơ" đi xuống + xì hơi
  sfx_golden_jackpot.wav   bậc CAO: hợp âm đi lên + chuông dồn dập
  sfx_golden_coin.wav      một đồng xu chạm đất (phát khi xu vật lý va chạm)
  sfx_golden_token.wav     xu mồi lọt khe (tương tác phụ)
"""
import os
import sys
import wave

import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)


def t_axis(s):
    return np.arange(int(SR * s)) / SR


def env(n, a, r):
    e = np.ones(n)
    na, nr = int(SR * a), int(SR * r)
    if na: e[:na] = np.linspace(0, 1, na)
    if nr: e[-nr:] *= np.linspace(1, 0, nr)
    return e


def bell(f, s, decay=6.0):
    t = t_axis(s)
    x = sum(np.sin(2 * np.pi * f * k * t) / k ** 1.3 for k in (1, 2.76, 5.4))
    return x * np.exp(-decay * t)


def save(path, x):
    x = x / (np.max(np.abs(x)) + 1e-9) * 0.9
    with wave.open(path, "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x * 32767).astype(np.int16).tobytes())


def lever():
    t = t_axis(0.6)
    clank = RNG.standard_normal(len(t)) * np.exp(-40 * t)
    ring = bell(620, 0.6, 9) * 0.6
    spring = np.sin(2 * np.pi * (180 + 900 * t) * t) * np.exp(-7 * t) * 0.4
    return clank * 0.7 + ring + spring


def spin_loop():
    s = 1.0  # đúng 1 giây, 8 tích -> lặp liền mạch
    t = t_axis(s)
    whirr = np.sin(2 * np.pi * 95 * t) * (0.6 + 0.4 * np.sin(2 * np.pi * 16 * t))
    noise = RNG.standard_normal(len(t)) * 0.15
    ticks = np.zeros(len(t))
    for i in range(8):
        k = int(i * SR / 8)
        n = int(0.03 * SR)
        ticks[k:k + n] += np.sin(2 * np.pi * 2400 * t[:n]) * np.exp(-150 * t[:n])
    return whirr * 0.4 + noise + ticks


def reel_stop():
    t = t_axis(0.35)
    return np.sin(2 * np.pi * (140 - 80 * t) * t) * np.exp(-14 * t) + RNG.standard_normal(len(t)) * np.exp(-60 * t) * 0.5


def fizzle():
    parts = []
    for f0, d in ((392, 0.32), (370, 0.32), (349, 0.32), (330, 1.1)):
        t = t_axis(d)
        vib = 1 + (0.03 * np.sin(2 * np.pi * 6 * t) if d > 1 else 0)
        f = f0 * vib * (1 - 0.12 * t / d if d > 1 else 1)
        ph = 2 * np.pi * np.cumsum(f) / SR
        x = sum(np.sin(k * ph) / k for k in range(1, 7))
        parts.append(x * env(len(t), 0.02, 0.08) * 0.5)
    tail = t_axis(0.7)
    hiss = RNG.standard_normal(len(tail)) * np.exp(-4 * tail) * 0.35
    return np.concatenate(parts + [hiss])


def jackpot():
    out = np.zeros(int(SR * 2.6))
    for i, f in enumerate((523, 659, 784, 1047, 1319)):
        b = bell(f, 1.6, 3)
        k = int(i * 0.09 * SR)
        out[k:k + len(b)] += b * 0.6
    t = t_axis(2.6)
    chord = sum(np.sin(2 * np.pi * f * t) for f in (523, 659, 784, 1047)) * env(len(t), 0.4, 1.2) * 0.25
    out += chord
    for i in range(28):  # chuông dồn dập như xu tuôn
        b = bell(RNG.uniform(1800, 3600), 0.3, 18)
        k = int(RNG.uniform(0.3, 2.2) * SR)
        out[k:k + len(b)] += b * 0.3
    return out


def coin():
    return bell(2900, 0.4, 12) + bell(4100, 0.4, 16) * 0.5


def token():
    t = t_axis(0.5)
    slide = np.sin(2 * np.pi * (900 - 500 * t) * t) * np.exp(-6 * t) * 0.4
    drop = np.zeros(len(t)); k = int(0.25 * SR)
    b = bell(1500, 0.25, 14); drop[k:k + len(b)] = b[:len(drop) - k]
    return slide + drop


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(here, "..", "..", "Assets", "_Project", "Audio", "Chests", "golden")
    os.makedirs(out, exist_ok=True)
    for name, fn in (("lever", lever), ("spin_loop", spin_loop), ("reel_stop", reel_stop), ("fizzle", fizzle),
                     ("jackpot", jackpot), ("coin", coin), ("token", token)):
        save(os.path.join(out, f"sfx_golden_{name}.wav"), fn())
        print("wrote", name)


if __name__ == "__main__":
    main()

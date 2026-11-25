#!/usr/bin/env python3
"""Tổng hợp âm riêng cho kịch bản "Mèo hoang lật thùng rác" của Chest_RacQuy.

Chỉ dùng sóng sin, nhiễu và lọc đơn giản (numpy), không mẫu thu âm, không dịch vụ trả phí -> không vướng bản quyền.
Ghi vào Assets/_Project/Audio/Chests/rac_quy/meo/ (WAV mono 44,1 kHz 16-bit). Hạt giống cố định.

    python3 Tools/Audio/make_racquy_meo_sfx.py [thu_muc_ra]

CHƯA AI NGHE THỬ các file này: chỉ kiểm độ dài/đỉnh bằng code. Nghe rồi chỉnh tham số bên dưới.
"""
import os
import sys
import wave

import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)


def t_axis(s):
    return np.arange(int(SR * s)) / SR


def noise(s):
    return RNG.uniform(-1, 1, int(SR * s))


def lp(x, cutoff):
    """Lọc thông thấp một cực (cutoff có thể là mảng)."""
    c = np.broadcast_to(np.asarray(cutoff, dtype=float), x.shape)
    a = np.exp(-2 * np.pi * c / SR)
    y = np.empty_like(x)
    z = 0.0
    for i in range(len(x)):
        z = (1 - a[i]) * x[i] + a[i] * z
        y[i] = z
    return y


def hp(x, cutoff):
    return x - lp(x, cutoff)


def osc(freq, s, kind="sin"):
    f = np.broadcast_to(np.asarray(freq, dtype=float), (int(SR * s),))
    ph = 2 * np.pi * np.cumsum(f) / SR
    if kind == "saw":
        return 2 * ((ph / (2 * np.pi)) % 1.0) - 1
    return np.sin(ph)


def env(s, attack, decay):
    t = t_axis(s)
    return np.clip(t / max(attack, 1e-5), 0, 1) * np.exp(-t / decay)


def place(buf, x, at):
    i = int(at * SR)
    n = min(len(x), len(buf) - i)
    if n > 0:
        buf[i:i + n] += x[:n]


def clank(freq, s=0.18):
    """Tiếng lon thiếc: vài họa âm không điều hoà tắt nhanh + chút nhiễu đầu."""
    x = sum(a * osc(freq * r, s) for r, a in ((1, 1), (2.31, .6), (3.9, .4), (5.7, .25)))
    x = x * env(s, 0.001, 0.035) + hp(noise(s), 2000) * env(s, 0.0005, 0.008) * 0.8
    return x


def meow(s, f0, f1, f2, bright=1.0):
    """Mèo kêu: răng cưa trượt cao độ qua lọc formant mở rồi đóng ("m-e-o")."""
    t = t_axis(s)
    u = t / s
    f = np.interp(u, [0, .35, 1], [f0, f1, f2]) * (1 + 0.012 * np.sin(2 * np.pi * 6 * t))
    src = osc(f, s, "saw")
    cut = np.interp(u, [0, .3, .7, 1], [500, 2600 * bright, 1800, 600])
    x = lp(lp(src, cut), cut * 1.3)
    a = np.interp(u, [0, .08, .8, 1], [0, 1, .8, 0])
    return x * a


def write(path, x, peak_db=-1.0):
    x = x / (np.max(np.abs(x)) + 1e-9) * 10 ** (peak_db / 20)
    n = min(len(x), int(0.004 * SR))
    x[:n] *= np.linspace(0, 1, n)
    x[-n:] *= np.linspace(1, 0, n)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((np.clip(x, -1, 1) * 32767).astype("<i2").tobytes())
    print(f"{os.path.basename(path)}: {len(x) / SR:.2f} s")


# ---------------------------------------------------------------- các âm

def build_loop():
    """Vòng lặp 2 s (lặp liền mạch): 4 nhịp trống thùng rỗng + lạch cạch rác. Code tăng pitch/âm lượng dần."""
    s = 2.0
    b = np.zeros(int(SR * s))
    for k in range(4):
        th = osc(np.linspace(110, 55, int(SR * .25)), .25) * env(.25, .002, .07)
        place(b, th * (1.0 if k % 2 == 0 else .7), k * .5)
    for k in range(14):
        place(b, clank(RNG.uniform(700, 1900), .12) * RNG.uniform(.15, .35), RNG.uniform(0, s - .13))
    b += lp(noise(s), 300) * .08          # rì rào nền
    return b


def hiss():
    s = .8
    return bandpass_noise(s, 3500) * np.interp(t_axis(s) / s, [0, .1, .7, 1], [0, 1, .7, 0])


def bandpass_noise(s, c):
    return hp(lp(noise(s), c * 1.5), c * .6)


def clatter():
    s = 1.4
    b = np.zeros(int(SR * s))
    for k in range(26):
        at = (RNG.uniform(0, 1) ** 1.8) * (s - .2)
        place(b, clank(RNG.uniform(400, 2200)) * RNG.uniform(.3, 1), at)
    th = osc(np.linspace(90, 40, int(SR * .4)), .4) * env(.4, .002, .12)
    place(b, th * 1.5, 0)
    return b


def deflate():
    """Bậc thấp: cả cơn căng 'xì hơi' -> tiếng rít tụt giọng rồi 'phụt' ướt."""
    s = 1.3
    b = np.zeros(int(SR * s))
    sq = osc(np.geomspace(900, 140, int(SR * .9)), .9, "saw")
    sq = lp(sq, 1500) * (1 + .5 * np.sin(2 * np.pi * 23 * t_axis(.9))) * np.linspace(1, .5, int(SR * .9))
    place(b, sq * .6, 0)
    pf = lp(noise(.35), 600) * env(.35, .005, .09) + osc(70, .35) * env(.35, .003, .08) * .6
    place(b, pf * 1.4, .92)
    return b


def boom():
    """Bậc cao: nổ trầm + rền + lấp lánh."""
    s = 2.6
    b = np.zeros(int(SR * s))
    place(b, osc(np.geomspace(120, 35, int(SR * 1.5)), 1.5) * env(1.5, .003, .5) * 1.2, 0)
    place(b, lp(noise(1.2), np.geomspace(6000, 300, int(SR * 1.2))) * env(1.2, .002, .3), 0)
    for k in range(18):
        f = RNG.choice([1318, 1568, 1760, 2093, 2637, 3136])
        place(b, osc(f, .6) * env(.6, .002, .18) * .25, .15 + k * .1 + RNG.uniform(0, .05))
    return b


def throw():
    s = .35
    return bandpass_noise(s, 1800) * np.sin(np.pi * t_axis(s) / s) ** 2


def main():
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(
        os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Audio", "Chests", "rac_quy", "meo")
    os.makedirs(out, exist_ok=True)
    jobs = {
        "sfx_racquy_meo_build_loop.wav": build_loop(),
        "sfx_racquy_meo_meow.wav": meow(.55, 550, 780, 480),
        "sfx_racquy_meo_meow_wants.wav": meow(.9, 450, 900, 700, 1.2),
        "sfx_racquy_meo_yowl.wav": meow(1.1, 600, 1300, 1500, 1.5),
        "sfx_racquy_meo_hiss.wav": hiss(),
        "sfx_racquy_meo_clatter.wav": clatter(),
        "sfx_racquy_meo_clank.wav": clank(1100),
        "sfx_racquy_meo_deflate.wav": deflate(),
        "sfx_racquy_meo_boom.wav": boom(),
        "sfx_racquy_meo_throw.wav": throw(),
    }
    for name, x in jobs.items():
        write(os.path.join(out, name), x)


if __name__ == "__main__":
    main()

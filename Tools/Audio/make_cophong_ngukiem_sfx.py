#!/usr/bin/env python3
"""Âm thanh riêng cho kịch bản NGỰ KIẾM TIÊN KHÍ của Chest_CoPhong (tổng hợp bằng numpy, không mẫu, không bản quyền).

    python3 Tools/Audio/make_cophong_ngukiem_sfx.py [thư_mục_ra]

Mặc định ghi vào Assets/_Project/Audio/Chests/co_phong/ngu_kiem/ (WAV mono 44.1 kHz 16-bit, hạt giống cố định).
Dùng lại khối dựng của make_chest_sfx.py (cùng thư mục). Tên file khớp CoPhongNguKiemHook / CoPhongNguKiemSetup.

Nhịp âm: bấm mở -> "rút kiếm" (ngân kim loại) + vòng ngân kiếm (loop, Unity nâng cao độ/âm lượng dần) ->
mỗi lần kiếm đâm bùa một tiếng "phập + lửa giấy" -> cuối: Rác = xịt + kiếm rơi lạch cạch; bậc cao = dâng rồi nổ.
"""
import os
import sys
import wave

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import make_chest_sfx as m  # noqa: E402

SR = m.SR
RNG = np.random.default_rng(20261125)


def glide(f0, f1, seconds, curve=1.0):
    """Sine whose pitch slides f0 -> f1 (curve > 1: slow start)."""
    u = np.linspace(0, 1, int(SR * seconds)) ** curve
    return m.sine(f0 + (f1 - f0) * u, seconds)


def write_loop(path, x, peak_db=-3.0):
    """Like m.write but without the fade, so the loop is seamless (every partial has whole cycles)."""
    x = m.normalise(x, peak_db)
    data = (np.clip(x, -1, 1) * 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())
    print(f"  {os.path.basename(path):34s} {len(x) / SR:5.2f} s (loop)")


# ---------------------------------------------------------------- sounds

def hum_loop():
    """Vòng ngân kiếm: 2 s, mọi tần số là bội của 0.5 Hz nên lặp liền. Unity kéo pitch 0.8 -> 1.5 theo độ căng."""
    d = 2.0
    t = m.t_axis(d)
    x = (np.sin(2 * np.pi * 220 * t) + 0.55 * np.sin(2 * np.pi * 330.5 * t) + 0.35 * np.sin(2 * np.pi * 441 * t)
         + 0.18 * np.sin(2 * np.pi * 662 * t) + 0.08 * np.sin(2 * np.pi * 1323.5 * t))
    wob = 0.75 + 0.25 * np.sin(2 * np.pi * 1.0 * t)          # thở chậm 1 Hz (2 chu kỳ / vòng)
    swish = 0.5 + 0.5 * np.sin(2 * np.pi * 4.0 * t) ** 2       # tiếng kiếm lướt qua tai 4 lần / giây
    return x * wob * (0.7 + 0.3 * swish)


def draw_sword():
    """Bấm mở: 'xoẹt' rút kiếm (nhiễu lọc quét lên) + ngân kim loại cao."""
    d = 1.3
    n = m.noise(d) * m.env_exp(d, 0.25, attack=0.05)
    sweep = m.bandpass(n, np.linspace(900, 6500, len(n)), 4.0)
    ring = m.bell(1568, d, 0.6, detune=0.002) + 0.5 * m.bell(2349, d, 0.4)
    return m.mix(d, (0, sweep, 1.0), (0.12, ring, 0.55))


def strike():
    """Kiếm đâm bùa: phập (thấp, ngắn) + lửa giấy lép bép."""
    d = 0.9
    thock = m.sine(140, d) * m.env_exp(d, 0.05) + 0.6 * m.onepole_lp(m.noise(d), 1200) * m.env_exp(d, 0.03)
    crackle = np.zeros(int(SR * d))
    for _ in range(26):
        i = int(RNG.uniform(0.05, 0.8) * SR)
        k = int(0.004 * SR)
        crackle[i:i + k] += RNG.uniform(0.3, 1.0) * RNG.uniform(-1, 1, k) * np.linspace(1, 0, k)
    flame = m.bandpass(m.noise(d), 2500, 1.2) * m.env_exp(d, 0.35, attack=0.08)
    return m.mix(d, (0, thock, 1.0), (0.03, crackle, 0.7), (0.02, flame, 0.4))


def stone():
    """Linh thạch rơi vào rương: hai tiếng pha lê."""
    d = 0.9
    return m.mix(d, (0, m.bell(2093, d, 0.35), 0.8), (0.07, m.bell(3136, d, 0.3), 0.6), (0.0, m.sine(523, d) * m.env_exp(d, 0.4), 0.3))


def crane():
    """Hạc giấy bay ngang: tiếng huýt như sáo trúc, lên rồi xuống."""
    d = 0.8
    f = 880 + 260 * np.sin(np.linspace(0, np.pi, int(SR * d)))
    breath = m.bandpass(m.noise(d), 1800, 3.0) * 0.15
    env = np.sin(np.linspace(0, np.pi, int(SR * d))) ** 1.5
    return (m.sine(f, d) + 0.2 * m.sine(f * 2, d) + breath) * env


def sword_land():
    """Một thanh kiếm cắm xuống đất: phập + rung lò xo."""
    d = 0.5
    thud = m.onepole_lp(m.noise(d), 600) * m.env_exp(d, 0.04)
    boing = m.sine(260 + 30 * np.sin(2 * np.pi * 18 * m.t_axis(d)), d) * m.env_exp(d, 0.18)
    return m.mix(d, (0, thud, 1.0), (0, boing, 0.45))


def end_rac():
    """Rác: vòng ngân sụp như bong bóng xì hơi ('pfffft') rồi ba tiếng kiếm rơi lạch cạch. Hài, đột ngột."""
    d = 2.4
    deflate = glide(660, 70, 0.9, curve=0.6) * np.linspace(1, 0.2, int(SR * 0.9))
    pff = m.bandpass(m.noise(0.6), np.linspace(1500, 300, int(SR * 0.6)), 2.0) * m.env_exp(0.6, 0.3)
    clank = lambda f: m.bell(f, 0.5, 0.08, partials=((1.0, 1.0), (2.3, 0.6), (3.9, 0.3)))
    return m.mix(d, (0, deflate, 0.9), (0.15, pff, 0.6), (1.05, clank(720), 0.7), (1.3, clank(610), 0.6),
                 (1.62, clank(820), 0.5), (1.9, m.sine(98, 0.4) * m.env_exp(0.4, 0.12), 0.4))


def _rise(seconds, f0, f1):
    n = int(SR * seconds)
    tone = glide(f0, f1, seconds, curve=1.6) + 0.4 * glide(f0 * 1.5, f1 * 1.5, seconds, curve=1.6)
    air = m.bandpass(m.noise(seconds), np.linspace(400, 4000, n), 2.5) * 0.5
    return (tone + air) * np.linspace(0.2, 1.0, n) ** 2


def _boom(seconds, size):
    low = glide(90 * size, 38, seconds, curve=0.5) * m.env_exp(seconds, 0.5 * size)
    burst = m.onepole_lp(m.noise(seconds), 2500) * m.env_exp(seconds, 0.12 * size)
    return low + 0.7 * burst


def end_thuong():
    d = 1.8
    return m.mix(d, (0, _rise(0.6, 330, 523), 0.5), (0.6, m.bell(784, 1.2, 0.5), 0.8))


def end_hiem():
    d = 2.4
    chord = m.bell(659, 1.8, 0.8) + 0.8 * m.bell(988, 1.8, 0.7)
    return m.mix(d, (0, _rise(0.8, 220, 880), 0.6), (0.8, _boom(1.2, 0.6), 0.5), (0.8, chord, 0.7))


def end_rathiem():
    d = 2.9
    chord = m.bell(523, 2.2, 1.0) + 0.8 * m.bell(784, 2.2, 0.9) + 0.6 * m.bell(1047, 2.2, 0.8)
    return m.mix(d, (0, _rise(1.0, 165, 990), 0.7), (1.0, _boom(1.6, 0.85), 0.8), (1.0, chord, 0.7))


def end_huyenthoai():
    """Huyền thoại: dâng dài, nổ trầm, chiêng lớn ngân, rồng gầm (gliss thấp có rung)."""
    d = 4.2
    gong = m.bell(110, 3.2, 2.2, partials=((1.0, 1.0), (1.48, 0.7), (2.1, 0.5), (2.9, 0.35), (4.1, 0.2)), detune=0.003)
    roar_t = m.t_axis(1.4)
    roar = m.bandpass(m.noise(1.4), 260 + 120 * np.sin(2 * np.pi * 7 * roar_t), 2.0) * np.sin(np.linspace(0, np.pi, len(roar_t)))
    sparkle = m.bell(2637, 2.5, 0.9) + m.bell(3520, 2.5, 0.7)
    return m.mix(d, (0, _rise(1.3, 110, 1320), 0.8), (1.3, _boom(2.4, 1.2), 1.0), (1.3, gong, 0.9),
                 (0.55, roar, 0.5), (1.5, sparkle, 0.35))


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(here, "..", "..", "Assets", "_Project", "Audio", "Chests", "co_phong", "ngu_kiem")
    out = os.path.abspath(out)
    os.makedirs(out, exist_ok=True)
    print("->", out)
    write_loop(os.path.join(out, "sfx_ngukiem_hum_loop.wav"), hum_loop())
    for name, fn in [("draw_sword", draw_sword), ("strike", strike), ("stone", stone), ("crane", crane),
                     ("sword_land", sword_land), ("end_rac", end_rac), ("end_thuong", end_thuong),
                     ("end_hiem", end_hiem), ("end_ratHiem", end_rathiem), ("end_huyenThoai", end_huyenthoai)]:
        m.write(os.path.join(out, f"sfx_ngukiem_{name}.wav"), fn())


if __name__ == "__main__":
    main()

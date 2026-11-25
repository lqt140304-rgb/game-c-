#!/usr/bin/env python3
"""Tổng hợp âm riêng cho kịch bản "Nhát kiếm Iai" của Chest_NhatBan (chỉ numpy, không mẫu, không dịch vụ trả phí).

    python3 Tools/Audio/make_nhatban_sfx.py [thu_muc_ra]

Mặc định ghi vào Assets/_Project/Audio/Chests/nhat_ban/ (WAV mono 44,1 kHz 16-bit), hạt giống cố định.
Tên file là thứ NhatBanFxPreset / NhatBanHook mong đợi. CHƯA AI NGHE THỬ: nghe trong Unity rồi chỉnh.
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


def env(s, decay, attack=0.002):
    t = t_axis(s)
    return np.clip(t / max(attack, 1e-6), 0, 1) * np.exp(-t / decay)


def band(x, lo, hi):
    """Lọc dải bằng FFT (nhanh, không pha trộn trễ)."""
    f = np.fft.rfft(x)
    fr = np.fft.rfftfreq(len(x), 1 / SR)
    f[(fr < lo) | (fr > hi)] = 0
    return np.fft.irfft(f, len(x))


def sweep_band(x, lo0, hi0, lo1, hi1, chunks=48):
    """Dải lọc trượt theo thời gian (ghép từng khúc có chồng mép)."""
    n = len(x)
    out = np.zeros(n)
    step = n // chunks
    win = np.hanning(step * 2)
    for i in range(chunks):
        a = max(0, i * step - step // 2)
        seg = x[a:a + step * 2]
        if len(seg) < 8:
            continue
        k = i / max(1, chunks - 1)
        lo, hi = lo0 + (lo1 - lo0) * k, hi0 + (hi1 - hi0) * k
        out[a:a + len(seg)] += band(seg, lo, hi) * win[:len(seg)]
    return out


def sine(freq, s, phase=0.0):
    t = t_axis(s)
    if np.ndim(freq) == 0:
        return np.sin(2 * np.pi * freq * t + phase)
    return np.sin(2 * np.pi * np.cumsum(freq) / SR + phase)


def place(total, *parts):
    """parts = (thời điểm s, mảng, độ lớn)."""
    out = np.zeros(int(SR * total))
    for at, x, g in parts:
        a = int(SR * at)
        b = min(len(out), a + len(x))
        if b > a:
            out[a:b] += x[:b - a] * g
    return out


def fade(x, fi=0.002, fo=0.05):
    x = x.copy()
    a, b = int(SR * fi), int(SR * fo)
    if a:
        x[:a] *= np.linspace(0, 1, a)
    if b:
        x[-b:] *= np.linspace(1, 0, b)
    return x


def write(path, x, peak_db=-1.0):
    x = fade(x)
    m = np.max(np.abs(x)) or 1.0
    x = x / m * 10 ** (peak_db / 20)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((x * 32767).astype(np.int16).tobytes())
    print(f"{os.path.basename(path)}  {len(x) / SR:.2f}s")


# ---------------------------------------------------------------- nhạc cụ

def taiko(f0=72, s=0.9, decay=0.22):
    t = t_axis(s)
    body = np.sin(2 * np.pi * np.cumsum(f0 * (1 + 0.8 * np.exp(-t / 0.03))) / SR) * env(s, decay, 0.001)
    skin = band(noise(s), 150, 1200) * env(s, 0.03, 0.0005)
    return body + 0.5 * skin


def koto(freq, s=1.6, bright=0.5):
    """Karplus-Strong: dây gảy."""
    n = int(SR * s)
    p = max(2, int(SR / freq))
    buf = RNG.uniform(-1, 1, p)
    out = np.empty(n)
    for i in range(n):
        v = buf[i % p]
        out[i] = v
        buf[i % p] = 0.996 * (bright * v + (1 - bright) * 0.5 * (v + buf[(i + 1) % p]))
    return out * env(s, s * 0.45, 0.001)


def gong(f0=110, s=4.0):
    parts = [(1.0, 1.0), (1.52, 0.6), (2.31, 0.45), (2.94, 0.3), (4.13, 0.2), (5.4, 0.12)]
    t = t_axis(s)
    swell = np.clip(t / 0.35, 0, 1) ** 0.5
    x = sum(a * np.sin(2 * np.pi * f0 * r * t * (1 + 0.004 * np.sin(2 * np.pi * 0.7 * t))) * np.exp(-t / (s * 0.35 / r ** 0.3))
            for r, a in parts)
    return x * swell + band(noise(s), 200, 3000) * env(s, 0.05) * 0.3


def ring(freq, s, decay):
    t = t_axis(s)
    return sum(a * np.sin(2 * np.pi * freq * r * t) * np.exp(-t / (decay / r ** 0.5))
               for r, a in [(1, 1), (2.76, 0.5), (5.4, 0.3), (8.9, 0.15)])


# ---------------------------------------------------------------- các âm

def drone():
    """Hơi sáo shakuhachi + nền trầm, 4 s, vòng lặp liền mạch (chồng mép)."""
    s = 4.5
    t = t_axis(s)
    vib = 1 + 0.006 * np.sin(2 * np.pi * 5.2 * t)
    tone = sine(293.66 * vib, s) * 0.35 + sine(146.8, s) * 0.25 + sine(73.4, s) * 0.3
    breath = band(noise(s), 500, 2400) * (0.55 + 0.45 * np.sin(2 * np.pi * 0.25 * t) ** 2)
    x = tone * (0.7 + 0.3 * np.sin(2 * np.pi * 0.5 * t)) + breath * 0.35
    loop = int(SR * 4.0)
    xf = int(SR * 0.5)
    y = x[:loop].copy()
    y[:xf] = y[:xf] * np.linspace(0, 1, xf) + x[loop:loop + xf] * np.linspace(1, 0, xf)
    return y


def draw():
    """Rút kiếm: tiếng thép miết lên, kết bằng một tiếng ngân cao."""
    s = 0.9
    scrape = sweep_band(noise(s), 2000, 4000, 5000, 9000) * np.clip(t_axis(s) / 0.5, 0, 1) * env(s, 0.6)
    return place(1.6, (0, scrape, 0.8), (0.5, ring(2350, 1.1, 0.5), 0.5))


def slash():
    s = 0.4
    t = t_axis(s)
    shape = np.sin(np.pi * np.clip(t / s, 0, 1)) ** 2
    return sweep_band(noise(s), 3000, 9000, 300, 1500, 24) * shape


def sheath():
    """'Tách' kiếm nhập vỏ: cạch gỗ khô + ngân thép rất ngắn."""
    click = band(noise(0.05), 1500, 7000) * env(0.05, 0.006, 0.0003)
    knock = taiko(420, 0.15, 0.03)
    return place(0.7, (0, click, 1.0), (0.003, knock, 0.4), (0.0, ring(3100, 0.6, 0.12), 0.25))


def rope_snap():
    crack = band(noise(0.08), 800, 6000) * env(0.08, 0.012, 0.0003)
    return place(0.8, (0, crack, 1.0), (0.32, taiko(140, 0.4, 0.06), 0.5), (0.45, taiko(180, 0.3, 0.05), 0.35))


def cut_fruit():
    wet = band(noise(0.25), 300, 2500) * env(0.25, 0.05, 0.002)
    return place(0.6, (0, slash()[:int(SR * 0.25)], 0.6), (0.12, wet, 0.9), (0.14, ring(1800, 0.4, 0.15), 0.2))


def heartbeat():
    """Hai nhịp trống 'thịch-thịch' (dùng chung cho mọi bậc, pitch tăng dần trong mã)."""
    return place(0.7, (0, taiko(64, 0.5, 0.16), 1.0), (0.17, taiko(58, 0.5, 0.14), 0.7))


def fin_rac():
    """Kiếm chém hụt vào chiếc dép: 'boing' thép rung + dép 'bẹp' + sáo tụt giọng (xịt)."""
    t = t_axis(1.0)
    boing = np.sin(2 * np.pi * np.cumsum(330 * (1 + 0.25 * np.sin(2 * np.pi * 9 * t) * np.exp(-t / 0.4))) / SR) * env(1.0, 0.35)
    slap = band(noise(0.12), 400, 3500) * env(0.12, 0.02, 0.0005)
    st = t_axis(1.3)
    slide = sine(520 * np.exp(-st * 1.1), 1.3) * np.clip(st / 0.05, 0, 1) * np.exp(-st / 0.9)
    breath = band(noise(1.3), 400, 1800) * np.exp(-st / 0.6) * 0.3
    return place(2.4, (0, slap, 1.0), (0.05, boing, 0.7), (0.9, slide + breath, 0.6))


def fin(n_slash, gong_f, big):
    """Bậc Thường..Huyền thoại: n nhát chém (cùng lúc → chồng sát nhau), 'tách', rồi taiko/koto/gong."""
    total = 2.2 + 2.6 * big
    parts = []
    for i in range(n_slash):
        parts.append((0.03 * i, slash(), 0.7 / (1 + 0.3 * i)))
    parts.append((0.42, sheath(), 0.9))
    if big >= 0.3:
        for i, d in enumerate([0.55, 0.72] if big < 0.9 else [0.55, 0.66, 0.77, 0.88, 1.05]):
            parts.append((d, taiko(70 - 3 * i, 0.9, 0.25), 0.9))
    notes = [587.3, 659.3, 784.0, 880.0, 1174.7]
    for i in range(1 + n_slash):
        parts.append((0.55 + 0.09 * i, koto(notes[min(i, len(notes) - 1)], 1.6), 0.35))
    if gong_f:
        parts.append((0.6 if big < 0.9 else 1.05, gong(gong_f, 3.5 + big), 0.5 + 0.5 * big))
    if big >= 0.9:
        parts.append((1.05, sweep_band(noise(2.5), 2000, 8000, 6000, 14000) * env(2.5, 1.0, 0.02), 0.25))
    return place(total, *parts)


def main():
    root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(root, "Assets/_Project/Audio/Chests/nhat_ban")
    os.makedirs(out, exist_ok=True)
    w = lambda n, x: write(os.path.join(out, n), x)
    w("sfx_nhatban_iai_drone.wav", drone())
    w("sfx_nhatban_iai_heartbeat.wav", heartbeat())
    w("sfx_nhatban_iai_draw.wav", draw())
    w("sfx_nhatban_iai_slash.wav", slash())
    w("sfx_nhatban_iai_sheath.wav", sheath())
    w("sfx_nhatban_iai_rope_snap.wav", rope_snap())
    w("sfx_nhatban_iai_cut_fruit.wav", cut_fruit())
    w("sfx_nhatban_iai_fin_rac.wav", fin_rac())
    w("sfx_nhatban_iai_fin_thuong.wav", fin(1, 0, 0.0))
    w("sfx_nhatban_iai_fin_hiem.wav", fin(2, 0, 0.3))
    w("sfx_nhatban_iai_fin_ratHiem.wav", fin(3, 98, 0.6))
    w("sfx_nhatban_iai_fin_huyenThoai.wav", fin(6, 73.4, 1.0))


if __name__ == "__main__":
    main()

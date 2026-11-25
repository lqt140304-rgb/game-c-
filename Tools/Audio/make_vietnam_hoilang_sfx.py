#!/usr/bin/env python3
"""Tổng hợp âm riêng cho kịch bản "Đêm hội làng" của Chest_VietNam (chỉ numpy, không mẫu, không bản quyền).

    python3 Tools/Audio/make_vietnam_hoilang_sfx.py [thư_mục_ra]

Mặc định ghi vào Assets/_Project/Audio/Chests/viet_nam/hoi_lang/ (WAV mono 44,1 kHz 16-bit), hạt giống cố định.
Tên file là thứ VietNamHoiLangSetup tìm. Đây là âm TẠM do code tạo, CHƯA AI NGHE THỬ trong game.
"""
import os
import sys
import wave

import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)


def t_axis(s):
    return np.arange(int(SR * s)) / SR


def env(s, decay, attack=0.003):
    t = t_axis(s)
    return np.clip(t / attack, 0, 1) * np.exp(-t / decay)


def osc(freq, s, kind="sin"):
    f = np.broadcast_to(np.asarray(freq, float), t_axis(s).shape)
    ph = 2 * np.pi * np.cumsum(f) / SR
    if kind == "saw":
        return 2 * ((ph / (2 * np.pi)) % 1.0) - 1
    return np.sin(ph)


def lowpass(x, cutoff):
    # Lọc một cực, cutoff cố định (đủ cho âm ngắn).
    a = np.exp(-2 * np.pi * cutoff / SR)
    y = np.empty_like(x)
    z = 0.0
    for i, v in enumerate(x):
        z = (1 - a) * v + a * z
        y[i] = z
    return y


def noise(s):
    return RNG.uniform(-1, 1, int(SR * s))


def norm(x, peak=0.89):
    m = np.max(np.abs(x))
    return x if m == 0 else x * (peak / m)


def write(folder, name, x):
    x = np.clip(norm(x), -1, 1)
    with wave.open(os.path.join(folder, name), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((x * 32767).astype("<i2").tobytes())
    print(f"{name}: {len(x) / SR:.2f} s")


def drone():
    """Đàn bầu ngân, 4 s, lặp liền mạch (196 Hz * 4 s = số chu kỳ nguyên, rung 5 Hz cũng nguyên)."""
    s, f0 = 4.0, 196.0
    t = t_axis(s)
    vib = 1 + 0.006 * np.sin(2 * np.pi * 5 * t)
    x = sum(a * osc(f0 * k * vib, s) for k, a in [(1, 1.0), (2, 0.5), (3, 0.28), (4, 0.12), (5, 0.06)])
    x += 0.35 * osc(f0 / 2, s)  # trầm nền
    x *= 0.85 + 0.15 * np.sin(2 * np.pi * 0.5 * t)  # thở chậm, 2 chu kỳ / 4 s
    return x


def drum():
    """Trống cái: thân trầm tụt cao độ + tiếng da."""
    s = 0.7
    t = t_axis(s)
    body = osc(55 + 60 * np.exp(-t / 0.05), s) * env(s, 0.22)
    skin = lowpass(noise(s), 1800) * env(s, 0.025)
    return body + 0.6 * skin


def mo():
    """Mõ gỗ: hai thành phần cộng hưởng ngắn."""
    s = 0.18
    return (osc(820, s) + 0.5 * osc(1370, s)) * env(s, 0.03, 0.001)


def plop():
    """Thả đèn xuống nước: bọt nhỏ vút lên."""
    s = 0.35
    t = t_axis(s)
    return osc(280 + 1400 * t, s) * env(s, 0.07) + 0.15 * lowpass(noise(s), 900) * env(s, 0.03)


def deflate():
    """XỊT: bóng xì hơi, cao độ tụt, rung lắc — kết thúc hài cho bậc thấp."""
    s = 1.3
    t = t_axis(s)
    f = 330 * np.exp(-t / 0.55) + 55
    f = f * (1 + 0.08 * np.sin(2 * np.pi * (14 + 10 * t) * t))
    x = osc(f, s, "saw") * 0.6 + lowpass(noise(s), 2500) * 0.5
    x = lowpass(x, 1600)
    a = np.clip(t / 0.02, 0, 1) * np.clip((s - t) / 0.25, 0, 1)
    return x * a


def bloop():
    """Rương chìm: ba bọt to tụt dần."""
    s = 0.9
    out = np.zeros(int(SR * s))
    for i, (start, f) in enumerate([(0.0, 420), (0.22, 330), (0.45, 240)]):
        seg = 0.3
        t = t_axis(seg)
        b = osc(f * (1 + 1.5 * t), seg) * env(seg, 0.08)
        k = int(start * SR)
        out[k:k + len(b)] += b * (1 - 0.2 * i)
    return out


def frog():
    """Ếch kêu "ộp" hai tiếng."""
    s = 0.5
    out = np.zeros(int(SR * s))
    for start in (0.0, 0.2):
        seg = 0.16
        t = t_axis(seg)
        b = osc(150 + 40 * np.sin(2 * np.pi * 30 * t), seg, "saw") * env(seg, 0.06, 0.01)
        b = lowpass(b, 700)
        k = int(start * SR)
        out[k:k + len(b)] += b
    return out


def boom():
    """Trống đồng + cồng ngân dài: cao trào bậc cao."""
    s = 3.5
    t = t_axis(s)
    low = osc(48 + 40 * np.exp(-t / 0.08), s) * env(s, 0.6)
    partials = [(1.0, 220), (0.6, 331), (0.45, 467), (0.3, 612), (0.25, 845), (0.15, 1120)]
    gong = sum(a * osc(f * (1 + 0.002 * np.sin(2 * np.pi * 3 * t)), s) for a, f in partials)
    gong *= np.clip(t / 0.15, 0, 1) * np.exp(-t / 1.4)
    crack = lowpass(noise(s), 3000) * env(s, 0.04)
    return 1.2 * low + 0.5 * gong + 0.5 * crack


def flap():
    """Chim Lạc vút qua: gió phồng lên rồi tắt, có nhịp vỗ cánh."""
    s = 0.8
    t = t_axis(s)
    x = lowpass(noise(s), 1200) * np.sin(np.pi * t / s) ** 2
    return x * (0.6 + 0.4 * np.abs(np.sin(2 * np.pi * 6 * t)))


def turtle():
    """Rùa vàng trồi lên: nước cuộn trầm + bọt."""
    s = 1.4
    t = t_axis(s)
    x = lowpass(noise(s), 400) * np.sin(np.pi * t / s)
    for start in (0.3, 0.6, 0.8, 1.0):
        seg = 0.2
        b = osc(200 + 900 * t_axis(seg), seg) * env(seg, 0.05)
        k = int(start * SR)
        x[k:k + len(b)] += 0.5 * b
    return x


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    folder = sys.argv[1] if len(sys.argv) > 1 else os.path.join(
        here, "..", "..", "Assets", "_Project", "Audio", "Chests", "viet_nam", "hoi_lang")
    os.makedirs(folder, exist_ok=True)
    for name, fn in [("hl_drone.wav", drone), ("hl_drum.wav", drum), ("hl_mo.wav", mo), ("hl_plop.wav", plop),
                     ("hl_deflate.wav", deflate), ("hl_bloop.wav", bloop), ("hl_frog.wav", frog),
                     ("hl_boom.wav", boom), ("hl_flap.wav", flap), ("hl_turtle.wav", turtle)]:
        write(folder, name, fn())


if __name__ == "__main__":
    main()

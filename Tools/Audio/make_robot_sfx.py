#!/usr/bin/env python3
"""Âm tổng hợp riêng cho kịch bản "Rương Biến Hình" (Chest_Robot).

Không mẫu âm, không dịch vụ trả phí: chỉ sóng sin/vuông/răng cưa + nhiễu (numpy), nên không vướng bản quyền.
Ghi vào Assets/_Project/Audio/Chests/robot/transform/ (WAV mono 44,1 kHz 16-bit). Cố định hạt giống -> chạy lại ra y hệt.

    python3 Tools/Audio/make_robot_sfx.py [thư_mục_ra]

CHƯA AI NGHE THỬ các file này: chỉ kiểm bằng số (độ dài, đỉnh biên độ). Nghe rồi chỉnh thông số bên dưới.
"""
import os
import sys
import wave

import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)


def t_axis(s):
    return np.arange(int(SR * s)) / SR


def osc(freq, s, kind="sin"):
    """freq: số hoặc mảng (quét tần)."""
    n = int(SR * s)
    f = np.full(n, freq, dtype=float) if np.ndim(freq) == 0 else np.asarray(freq, dtype=float)[:n]
    ph = 2 * np.pi * np.cumsum(f) / SR
    if kind == "sin":
        return np.sin(ph)
    if kind == "sq":
        return np.sign(np.sin(ph)) * 0.6
    if kind == "saw":
        return ((ph / (2 * np.pi)) % 1.0) * 2 - 1
    raise ValueError(kind)


def noise(s):
    return RNG.uniform(-1, 1, int(SR * s))


def lp(x, cutoff):
    a = np.exp(-2 * np.pi * cutoff / SR)
    y = np.empty_like(x)
    acc = 0.0
    for i, v in enumerate(x):
        acc = (1 - a) * v + a * acc
        y[i] = acc
    return y


def env(s, attack=0.003, decay=0.2):
    t = t_axis(s)
    return np.clip(t / max(attack, 1e-6), 0, 1) * np.exp(-t / decay)


def fade(x, fin=0.005, fout=0.02):
    n_in, n_out = int(SR * fin), int(SR * fout)
    x = x.copy()
    if n_in:
        x[:n_in] *= np.linspace(0, 1, n_in)
    if n_out:
        x[-n_out:] *= np.linspace(1, 0, n_out)
    return x


def mix(*parts):
    n = max(len(p) for p in parts)
    out = np.zeros(n)
    for p in parts:
        out[: len(p)] += p
    return out


def at(x, sec, total):
    out = np.zeros(int(SR * total))
    i = int(SR * sec)
    m = min(len(x), len(out) - i)
    out[i: i + m] = x[:m]
    return out


def norm(x, peak=0.85):
    m = np.max(np.abs(x))
    return x if m == 0 else x * (peak / m)


def write(folder, name, x):
    x = np.clip(norm(x), -1, 1)
    path = os.path.join(folder, name)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((x * 32767).astype("<i2").tobytes())
    print(f"{name}: {len(x) / SR:.2f}s")


# ------------------------------------------------------------------ các âm

def press():
    """Ngay khi bấm mở: 'cạch' công tắc + tiếng chirp khởi động hai nốt."""
    s = 0.6
    click = lp(noise(0.03), 3000) * env(0.03, 0.0005, 0.008)
    thunk = osc(90, 0.15) * env(0.15, 0.001, 0.04)
    chirp1 = osc(880, 0.08, "sq") * env(0.08, 0.002, 0.05)
    chirp2 = osc(1320, 0.12, "sq") * env(0.12, 0.002, 0.07)
    return fade(mix(at(click, 0, s), at(thunk, 0, s), at(chirp1 * 0.4, 0.12, s), at(chirp2 * 0.4, 0.22, s)))


def hum_loop():
    """Tiếng máy rì rì (lặp). Code nâng pitch 0,55 -> 1,7 theo mức pin: độ căng dâng."""
    s = 2.0  # 110 Hz * 2 s = số chu kỳ nguyên -> lặp không vấp
    base = osc(110, s, "saw") * 0.35 + osc(220, s) * 0.2 + osc(55, s) * 0.3
    buzz = lp(noise(s), 400) * 0.15
    wob = 0.85 + 0.15 * np.sin(2 * np.pi * 4 * t_axis(s))  # 4 Hz: nguyên chu kỳ trong 2 s
    return lp(base + buzz, 1800) * wob


def slot_clunk():
    """Pin/bu-lông rơi trúng khe: 'keng' kim loại + xẹt điện."""
    s = 0.7
    ring = (osc(620, s) * 0.5 + osc(1490, s) * 0.3 + osc(2710, s) * 0.2) * env(s, 0.001, 0.12)
    thud = osc(140, 0.2) * env(0.2, 0.001, 0.04)
    zap = osc(np.linspace(2400, 600, int(SR * 0.18)), 0.18, "sq") * env(0.18, 0.001, 0.06) * 0.4
    return fade(mix(ring, thud, at(zap, 0.05, s)))


def beep():
    """Tiếng 'bíp' tim đập của hệ thống; code dồn nhịp và nâng pitch."""
    return fade(osc(1000, 0.09, "sq") * env(0.09, 0.002, 0.05), 0.002, 0.01)


def servo():
    """Chân duỗi ra / thân gập: mô-tơ rít lên rồi khựng."""
    s = 1.2
    f = np.concatenate([np.linspace(180, 520, int(SR * 0.9)), np.full(int(SR * 0.3), 520)])
    whine = osc(f, s, "saw") * 0.4 + osc(f * 2, s) * 0.2
    e = np.ones(int(SR * s))
    e[int(SR * 0.9):] = np.linspace(1, 0, int(SR * s) - int(SR * 0.9))
    clack = at(lp(noise(0.05), 2500) * env(0.05, 0.0005, 0.01), 0.9, s)
    return fade(lp(whine, 2500) * e + clack * 1.2)


def type_tick():
    """Tiếng gõ chữ của màn hình tự kiểm."""
    return fade(lp(noise(0.025), 5000) * env(0.025, 0.0005, 0.006) + osc(2200, 0.025, "sq") * env(0.025, 0.0005, 0.005) * 0.3, 0.0005, 0.005)


def drone_loop():
    """Con drone phụ tá bay tới: cánh quạt vo vo (lặp)."""
    s = 1.0
    blade = osc(150, s, "saw") * (0.6 + 0.4 * np.sin(2 * np.pi * 30 * t_axis(s)))
    return lp(blade + lp(noise(s), 900) * 0.3, 1200)


def fizzle():
    """BẬC THẤP: xịt. Máy đang rú bỗng tụt pitch, chập 'xẹt xẹt', 'bụp' rồi một tiếng 'bwoo-oop' buồn cười."""
    s = 2.4
    down = osc(np.geomspace(440, 40, int(SR * 1.0)), 1.0, "saw") * np.linspace(1, 0.2, int(SR * 1.0)) * 0.5
    crack = np.zeros(int(SR * s))
    for k in range(9):
        c = lp(noise(0.04), 4000) * env(0.04, 0.0005, 0.01)
        crack += at(c * RNG.uniform(0.4, 1.0), 0.2 + k * 0.09 + RNG.uniform(0, 0.04), s)
    pop = osc(70, 0.25) * env(0.25, 0.001, 0.05) + lp(noise(0.25), 800) * env(0.25, 0.001, 0.06)
    sad = osc(np.concatenate([np.full(int(SR * 0.25), 330), np.linspace(330, 196, int(SR * 0.55))]), 0.8, "sq")
    sad = lp(sad, 1500) * env(0.8, 0.02, 0.5) * 0.35
    return fade(mix(at(down, 0, s), crack, at(pop, 1.0, s), at(sad, 1.35, s)))


def blast(level):
    """BẬC CAO: rú lên rồi bùng nổ. level 1..4 = Thường..Huyền thoại: càng cao càng dài, nhiều nốt, nhiều ồn sáng."""
    rise_s = 0.35 + 0.15 * level
    s = rise_s + 1.2 + 0.4 * level
    rise = osc(np.geomspace(200, 900 + 300 * level, int(SR * rise_s)), rise_s, "saw") * np.linspace(0.2, 1, int(SR * rise_s)) * 0.4
    root = 261.6 * (2 ** (level / 12 * 2))
    chord_freqs = [1, 1.25, 1.5, 2, 2.5, 3][: 2 + level]
    tail = s - rise_s
    chord = sum(osc(root * r, tail, "sq") * 0.18 + osc(root * r * 2, tail) * 0.1 for r in chord_freqs)
    chord = lp(chord, 3500) * env(tail, 0.005, 0.35 + 0.25 * level)
    boom = (osc(60, 0.6) * 0.8 + lp(noise(0.6), 1500 + 800 * level) * 0.6) * env(0.6, 0.001, 0.12 + 0.05 * level)
    sparkle = np.zeros(int(SR * tail))
    for k in range(4 * level):
        f = root * RNG.choice([4, 5, 6, 8])
        sparkle += at(osc(f, 0.15) * env(0.15, 0.001, 0.05) * 0.15, RNG.uniform(0.05, tail * 0.7), tail)
    return fade(mix(at(rise, 0, s), at(chord + sparkle, rise_s, s), at(boom * (0.5 + 0.15 * level), rise_s, s)))


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(here, "..", "..", "Assets", "_Project", "Audio", "Chests", "robot", "transform")
    os.makedirs(out, exist_ok=True)
    write(out, "sfx_robotx_press.wav", press())
    write(out, "sfx_robotx_hum_loop.wav", hum_loop())
    write(out, "sfx_robotx_slot.wav", slot_clunk())
    write(out, "sfx_robotx_beep.wav", beep())
    write(out, "sfx_robotx_servo.wav", servo())
    write(out, "sfx_robotx_type.wav", type_tick())
    write(out, "sfx_robotx_drone_loop.wav", drone_loop())
    write(out, "sfx_robotx_fizzle.wav", fizzle())
    for lv, name in enumerate(["thuong", "hiem", "rathiem", "huyenthoai"], start=1):
        write(out, f"sfx_robotx_blast_{lv}_{name}.wav", blast(lv))


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""Âm thanh riêng kịch bản "Bộ máy Thiên Cơ" (Chest_ThienCo), tổng hợp hoàn toàn bằng numpy (không mẫu, không bản quyền).

    python3 Tools/Audio/make_thienco_clockwork_sfx.py [thư_mục_ra]

Mặc định ghi vào Assets/_Project/Audio/Chests/thien_co/clockwork/ (WAV mono 44,1 kHz 16-bit). Hạt giống cố định
nên chạy lại cho ra file giống hệt. Tên file khớp ThienCoClockworkAudio.Files (C#).
"""
import os
import sys
import wave

import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)


def t_axis(s):
    return np.arange(int(SR * s)) / SR


def env(s, decay, attack=0.002):
    t = t_axis(s)
    return np.clip(t / max(attack, 1e-6), 0, 1) * np.exp(-t / decay)


def noise(s):
    return RNG.uniform(-1, 1, int(SR * s))


def lowpass(x, cutoff):
    a = np.exp(-2 * np.pi * cutoff / SR)
    y = np.zeros_like(x)
    acc = 0.0
    for i, v in enumerate(x):
        acc = (1 - a) * v + a * acc
        y[i] = acc
    return y


def highpass(x, cutoff):
    return x - lowpass(x, cutoff)


def tone(freqs, s, phase=0.0):
    """freqs: số hoặc mảng tần số theo thời gian (quét cao độ)."""
    f = np.broadcast_to(np.asarray(freqs, dtype=float), (int(SR * s),))
    return np.sin(2 * np.pi * np.cumsum(f) / SR + phase)


def bell(f0, s, decay, partials=((1, 1), (2.76, 0.5), (5.4, 0.25), (8.93, 0.12), (0.5, 0.35))):
    out = np.zeros(int(SR * s))
    for ratio, amp in partials:
        out += amp * tone(f0 * ratio, s) * env(s, decay / (0.6 + ratio * 0.25), 0.001)
    return out


def click(s, cutoff_hi, decay, body_hz=None):
    x = highpass(noise(s), cutoff_hi) * env(s, decay, 0.0005)
    if body_hz:
        x += 0.6 * tone(body_hz, s) * env(s, decay * 2.5, 0.0005)
    return x


def pad(x, s):
    out = np.zeros(int(SR * s))
    out[:min(len(x), len(out))] = x[:len(out)]
    return out


def place(dst, x, at):
    i = int(at * SR)
    n = min(len(x), len(dst) - i)
    if n > 0:
        dst[i:i + n] += x[:n]


def write(folder, name, x, peak=0.9):
    x = np.asarray(x, dtype=float)
    m = np.max(np.abs(x)) or 1.0
    x = x / m * peak
    fade = min(len(x), int(0.004 * SR))
    x[-fade:] *= np.linspace(1, 0, fade)
    path = os.path.join(folder, name)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((x * 32767).astype("<i2").tobytes())
    print(f"{name:34s} {len(x) / SR:5.2f} s")


def main():
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(
        os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Audio", "Chests", "thien_co", "clockwork")
    os.makedirs(out, exist_ok=True)

    # Tích / tắc của bộ thoát (escapement): tích cao khô, tắc trầm hơn có thân gỗ.
    write(out, "sfx_tc_tick.wav", click(0.09, 2500, 0.006, 3100))
    write(out, "sfx_tc_tock.wav", click(0.11, 900, 0.009, 1250))

    # Lên dây cót: chuỗi lách cách nhanh của con cóc (pawl) + tiếng rít kim loại nhẹ.
    s = 0.5
    x = np.zeros(int(SR * s))
    for k in range(14):
        place(x, click(0.03, 3000, 0.003, 2600 + 40 * k) * (0.5 + k / 28), 0.02 + k * 0.033)
    x += 0.08 * tone(np.linspace(700, 1100, int(SR * s)), s) * env(s, 0.3, 0.05)
    write(out, "sfx_tc_wind.wav", x)

    # Bánh răng rơi chạm bàn / ăn khớp: tiếng keng ngắn kim loại.
    write(out, "sfx_tc_gear_clank.wav", bell(1480, 0.5, 0.12, ((1, 1), (2.31, 0.7), (3.9, 0.4), (6.1, 0.2))) + 0.5 * click(0.5, 1500, 0.01))
    # Ăn khớp đúng: "cách" chắc + răng lăn.
    x = pad(click(0.06, 1200, 0.006, 900), 0.45)
    for k in range(5):
        place(x, 0.4 * click(0.03, 3500, 0.002, 3000), 0.07 + k * 0.05)
    write(out, "sfx_tc_mesh_ok.wav", x)
    # Kẹt răng: rè rè nghiến, rung tần số thấp.
    s = 0.7
    t = t_axis(s)
    grind = highpass(noise(s), 600) * (0.5 + 0.5 * np.sign(np.sin(2 * np.pi * 34 * t))) * env(s, 0.35, 0.01)
    write(out, "sfx_tc_jam.wav", grind + 0.4 * tone(95, s) * env(s, 0.3))

    # Chuông điểm giờ (nhỏ, sáng) và chuông lớn (trầm, ngân dài).
    write(out, "sfx_tc_bell.wav", bell(1046.5, 1.6, 0.9))
    write(out, "sfx_tc_bell_big.wav", bell(261.6, 4.0, 2.6) + 0.4 * bell(523.25, 4.0, 1.8))

    # Lò xo bung (hài): "bô-iiing" cao độ rung lắc rồi tắt.
    s = 1.0
    t = t_axis(s)
    f = 180 + 260 * np.exp(-t * 3) + 60 * np.sin(2 * np.pi * 11 * t) * np.exp(-t * 2.5)
    write(out, "sfx_tc_sproing.wav", tone(f, s) * env(s, 0.35, 0.003) + 0.3 * click(s, 1500, 0.01))

    # Hết dây cót (xịt): tích tắc chậm dần, cao độ tụt, kết bằng một tiếng "phựt" nhỏ.
    s = 2.2
    x = np.zeros(int(SR * s))
    at, gap, pitch = 0.0, 0.11, 1.0
    for k in range(11):
        place(x, click(0.08, 1800 * pitch, 0.006, 2400 * pitch) * (1 - k / 14), at)
        at += gap
        gap *= 1.22
        pitch *= 0.9
        if at > 1.7:
            break
    place(x, 0.7 * lowpass(noise(0.25), 500) * env(0.25, 0.08, 0.01), 1.85)
    write(out, "sfx_tc_winddown.wav", x)

    # Chim cu: hai nốt sáo (cu-cu); bản buồn: nốt hai tụt lệch tông rồi rè.
    def whistle(f, s, slide=0.0):
        tt = t_axis(s)
        ff = f * (1 + slide * tt / s) * (1 + 0.006 * np.sin(2 * np.pi * 6 * tt))
        e = np.clip(tt / 0.03, 0, 1) * np.clip((s - tt) / 0.06, 0, 1)
        return (tone(ff, s) + 0.15 * tone(ff * 2, s) + 0.03 * lowpass(noise(s), 3000)) * e
    x = pad(whistle(784, 0.22), 0.65)
    place(x, whistle(622, 0.3), 0.3)
    write(out, "sfx_tc_cuckoo.wav", x)
    x = pad(whistle(784, 0.2), 1.0)
    place(x, whistle(560, 0.6, slide=-0.45) * np.linspace(1, 0.3, int(SR * 0.6)), 0.28)
    write(out, "sfx_tc_cuckoo_sad.wav", x)

    # Bắn lò xo lên (bậc cao): ống quét cao độ + gió.
    s = 1.1
    t = t_axis(s)
    write(out, "sfx_tc_launch.wav",
          0.6 * tone(150 * np.exp(t * 2.2), s) * env(s, 0.5, 0.01) + highpass(noise(s), 1200) * np.clip(t / 0.4, 0, 1) * np.exp(-t * 2.5))

    # Cát chảy (vòng lặp mềm).
    s = 2.0
    x = lowpass(highpass(noise(s), 2500), 7000) * 0.6
    grains = np.zeros(int(SR * s))
    for _ in range(140):
        place(grains, click(0.01, 4000, 0.001) * RNG.uniform(0.1, 0.5), RNG.uniform(0, s - 0.02))
    write(out, "sfx_tc_sand_loop.wav", x + grains, peak=0.5)

    # Hộp nhạc dâng (bậc cao, trước tiếng chuông lớn): rải hợp âm Đô trưởng đi lên, nhanh dần.
    notes = [523.25, 659.25, 783.99, 1046.5, 1318.5, 1568.0, 2093.0, 2637.0, 3136.0]
    s = 2.0
    x = np.zeros(int(SR * s))
    at = 0.0
    for k, nf in enumerate(notes):
        place(x, bell(nf, 0.9, 0.5, ((1, 1), (3.0, 0.2), (5.2, 0.05))) * (0.5 + k / 18), at)
        at += 0.16 * (0.88 ** k)
    write(out, "sfx_tc_musicbox_rise.wav", x)


if __name__ == "__main__":
    main()

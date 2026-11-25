#!/usr/bin/env python3
"""Âm riêng cho kịch bản "Trục vớt bằng đại bác" (Chest_HaiTac). Tổng hợp bằng numpy (sin, nhiễu, lọc), không mẫu,
không bản quyền. Ghi WAV mono 44.1 kHz vào Assets/_Project/Audio/Chests/hai_tac/seq/. Cố định hạt giống.

    python3 Tools/Audio/make_haitac_seq_sfx.py [thư_mục_ra]
"""
import os, sys, wave
import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)

def t(sec): return np.arange(int(SR * sec)) / SR
def noise(sec): return RNG.uniform(-1, 1, int(SR * sec))
def lp(x, a):
    """Low-pass một cực nhanh bằng lfilter thủ công qua cumsum xấp xỉ (a: 0..1, nhỏ = tối)."""
    y = np.empty_like(x); z = 0.0
    for i, v in enumerate(x):
        z += a * (v - z); y[i] = z
    return y
def env(sec, attack, decay):
    tt = t(sec); return np.clip(tt / max(attack, 1e-4), 0, 1) * np.exp(-tt / decay)
def tone(freqs, sec):
    f = np.broadcast_to(np.asarray(freqs, float), t(sec).shape)
    return np.sin(2 * np.pi * np.cumsum(f) / SR)
def saw(freqs, sec):
    f = np.broadcast_to(np.asarray(freqs, float), t(sec).shape)
    ph = np.cumsum(f) / SR; return 2 * (ph - np.floor(ph + 0.5))
def place(buf, x, at):
    i = int(at * SR); n = min(len(x), len(buf) - i)
    if n > 0: buf[i:i + n] += x[:n]
def norm(x, peak=0.9):
    m = np.max(np.abs(x)); return x * (peak / m) if m > 0 else x
def write(folder, name, x):
    x = np.clip(norm(x), -1, 1)
    fade = min(len(x), 400); x[-fade:] *= np.linspace(1, 0, fade)
    with wave.open(os.path.join(folder, name), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x * 32767).astype(np.int16).tobytes())
    print(name, f"{len(x) / SR:.2f}s")

def drum(sec=0.5, f0=90):
    return tone(f0 * (1 + 1.5 * np.exp(-t(sec) / 0.03)), sec) * env(sec, 0.002, 0.16) + 0.3 * lp(noise(sec), 0.2) * env(sec, 0.001, 0.03)

def cannon(sec=1.6):
    body = lp(noise(sec), 0.05) * env(sec, 0.003, 0.35) * 3
    thump = tone(55 * (1 + 2 * np.exp(-t(sec) / 0.05)), sec) * env(sec, 0.002, 0.3)
    crack = noise(sec) * env(sec, 0.0005, 0.015)
    return body + thump + 0.6 * crack

def splash(sec=0.9):
    return lp(noise(sec), 0.35) * env(sec, 0.01, 0.18) + 0.5 * lp(noise(sec), 0.08) * env(sec, 0.03, 0.3)

def brass(freq, sec, bright=1.0):
    vib = 1 + 0.006 * np.sin(2 * np.pi * 5.5 * t(sec))
    x = saw(freq * vib, sec) + 0.5 * saw(freq * 1.003 * vib, sec)
    a = np.clip(t(sec) / 0.06, 0, 1) * np.clip((sec - t(sec)) / 0.15, 0, 1)
    return lp(x, 0.08 * bright) * a

def build(sec=9.0):
    """Nền dâng: sóng + trống trận nhanh dần (70→190 bpm) + dây kéo lên một quãng năm + cuộn trống cuối. Không giải quyết."""
    out = np.zeros(int(SR * sec))
    tt = t(sec)
    waves = lp(noise(sec), 0.02) * (0.6 + 0.4 * np.sin(2 * np.pi * 0.35 * tt)) * 1.5
    out += waves * np.linspace(0.6, 1.0, len(tt))
    beat, bpm = 0.0, 70.0
    while beat < sec - 0.3:
        place(out, drum(0.5, 85) * (0.5 + 0.5 * beat / sec), beat)
        bpm = 70 + 120 * (beat / sec) ** 1.4
        beat += 60.0 / bpm
    f = 110 * 2 ** ((7 / 12) * (tt / sec) ** 1.6)          # A2 kéo lên E3
    drone = (saw(f, sec) + saw(f * 1.5, sec) * 0.5) * (tt / sec) ** 1.5
    out += lp(drone, 0.03 + 0.05 * 1) * 0.9
    roll_start = sec - 1.6
    k = roll_start
    while k < sec:
        place(out, lp(noise(0.06), 0.5) * env(0.06, 0.001, 0.02) * (0.4 + (k - roll_start)), k); k += 0.045
    return out

def ending(rank):
    if rank == 0:   # xịt + kèn buồn "uỳnh-uỳnh-uỳnh-uỳnh..."
        sec = 3.2; out = np.zeros(int(SR * sec))
        place(out, lp(noise(0.5), 0.3) * env(0.5, 0.01, 0.12) * 0.5, 0.0)   # pfft
        for i, (n, d) in enumerate([(311, 0.45), (293, 0.45), (277, 0.45), (262, 1.3)]):
            fr = n * (1 + (0.02 * np.sin(2 * np.pi * 6 * t(d)) if i == 3 else 0))
            place(out, brass(fr, d, 0.6) * 0.6, 0.35 + i * 0.48)
        return out
    sec = 2.2 + 0.5 * rank; out = np.zeros(int(SR * sec))
    for b in range(max(1, [0, 1, 1, 2, 3][rank])):
        place(out, cannon() * (1 - 0.15 * b), b * 0.22)
    root = [0, 196, 220, 247, 262][rank]
    chord = [1, 1.25, 1.5, 2] if rank >= 2 else [1, 1.25, 1.5]
    start = 0.25
    for i, m in enumerate(chord):                  # arpeggio đi lên
        place(out, brass(root * m, 0.3, 1 + 0.3 * rank) * 0.35, start + i * 0.09)
    hold = 0.8 + 0.4 * rank
    for m in chord + ([3, 4] if rank == 4 else []):   # hợp âm giữ, càng cao càng sáng
        place(out, brass(root * m, hold, 1 + 0.4 * rank) * 0.3, start + 0.09 * len(chord))
    if rank >= 3:   # chũm chọe
        place(out, noise(2.0) * env(2.0, 0.005, 0.6) * 0.35 * (rank - 2), start + 0.09 * len(chord))
    return out

def main():
    folder = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "../../Assets/_Project/Audio/Chests/hai_tac/seq")
    os.makedirs(folder, exist_ok=True)
    write(folder, "sfx_haitac_seq_build.wav", build())
    write(folder, "sfx_haitac_seq_cannon.wav", cannon())
    write(folder, "sfx_haitac_seq_splash.wav", splash())
    write(folder, "sfx_haitac_seq_thud.wav", tone(140 * (1 + np.exp(-t(0.4) / 0.02)), 0.4) * env(0.4, 0.001, 0.08) + 0.4 * lp(noise(0.4), 0.2) * env(0.4, 0.001, 0.04))
    load = np.zeros(int(SR * 0.6)); place(load, lp(noise(0.15), 0.4) * env(0.15, 0.002, 0.05), 0); place(load, drum(0.3, 160) * 0.7, 0.25)
    write(folder, "sfx_haitac_seq_load.wav", load)
    write(folder, "sfx_haitac_seq_fuse.wav", (noise(2.2) - lp(noise(2.2), 0.3)) * np.clip(t(2.2) / 0.05, 0, 1) * (0.7 + 0.3 * np.sin(2 * np.pi * 13 * t(2.2))))
    write(folder, "sfx_haitac_seq_fizzle.wav", (noise(0.8) - lp(noise(0.8), 0.3)) * env(0.8, 0.005, 0.2) * np.linspace(1, 0.2, int(SR * 0.8)))
    sq = tone(1400 + 900 * np.sin(np.pi * t(0.28) / 0.28) + 120 * np.sin(2 * np.pi * 40 * t(0.28)), 0.28)
    write(folder, "sfx_haitac_seq_parrot.wav", np.sign(sq) * np.abs(sq) ** 0.5 * env(0.28, 0.01, 0.12))
    g = np.zeros(int(SR * 1.8))
    for k in np.arange(0, 1.6, 0.11):
        place(g, tone(300 + 500 * RNG.random() + 400 * t(0.08) / 0.08, 0.08) * env(0.08, 0.002, 0.03) * 0.6, k)
    write(folder, "sfx_haitac_seq_drain.wav", g + 0.4 * lp(noise(1.8), 0.05) * np.linspace(1, 0, len(g)))
    write(folder, "sfx_haitac_seq_barrel.wav", tone(220, 0.3) * env(0.3, 0.001, 0.05) + tone(330, 0.3) * env(0.3, 0.001, 0.03) * 0.5)
    write(folder, "sfx_haitac_seq_coin.wav", (tone(2900, 0.35) + tone(4100, 0.35) * 0.6) * env(0.35, 0.001, 0.09))
    names = ["rac", "thuong", "hiem", "rathiem", "huyenthoai"]
    for r in range(5):
        write(folder, f"sfx_haitac_seq_end_{r}_{names[r]}.wav", ending(r))

if __name__ == "__main__":
    main()

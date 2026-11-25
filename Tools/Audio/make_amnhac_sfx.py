#!/usr/bin/env python3
"""Tổng hợp bộ âm RIÊNG cho kịch bản Chest_AmNhac "Loa Bass Kẹo Kéo" (numpy, không mẫu, không bản quyền).

    python3 Tools/Audio/make_amnhac_sfx.py [thư_mục_ra]

Mặc định ghi vào Assets/_Project/Audio/Chests/am_nhac/seq/ (WAV mono 44,1 kHz 16-bit, hạt giống cố định).
Các file này được AmNhacBassHook phát lần lượt theo nhịp (mỗi phách một kick...), nên mỗi file ngắn.
CHƯA AI NGHE THỬ: chỉ kiểm bằng số (độ dài, đỉnh không vượt 0 dBFS).
"""
import os
import sys
import wave

import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)


def t_axis(sec):
    return np.arange(int(SR * sec)) / SR


def env(sec, decay, attack=0.002):
    t = t_axis(sec)
    return np.clip(t / max(attack, 1e-6), 0, 1) * np.exp(-t / decay)


def sweep(f0, f1, sec, curve=1.0):
    """Sine whose frequency glides from f0 to f1 (exponential glide)."""
    t = t_axis(sec)
    k = (t / sec) ** curve
    f = f0 * (f1 / f0) ** k
    return np.sin(2 * np.pi * np.cumsum(f) / SR)


def lowpass(x, cutoff):
    a = np.broadcast_to(np.exp(-2 * np.pi * np.asarray(cutoff, dtype=float) / SR), x.shape)
    y = np.zeros_like(x)
    acc = 0.0
    for i, v in enumerate(x):
        acc = (1 - a[i]) * v + a[i] * acc
        y[i] = acc
    return y


def highpass(x, cutoff):
    return x - lowpass(x, cutoff)


def noise(sec):
    return RNG.uniform(-1, 1, int(SR * sec))


def norm(x, peak=0.9):
    m = np.max(np.abs(x))
    return x if m == 0 else x * (peak / m)


def soft(x, drive):
    return np.tanh(x * drive) / np.tanh(drive)


# ---------------------------------------------------------------- sounds

def kick():
    sec = 0.45
    body = sweep(150, 45, sec, 0.35) * env(sec, 0.16)
    click = highpass(noise(0.01), 2000) * 0.4
    body[: click.size] += click
    return norm(soft(body, 2.0))


def hat():
    sec = 0.09
    return norm(highpass(noise(sec), 7000) * env(sec, 0.025), 0.6)


def clap():
    sec = 0.25
    n = highpass(lowpass(noise(sec), 5000), 900)
    e = np.zeros(int(SR * sec))
    for d in (0.0, 0.012, 0.024):
        s = int(d * SR)
        part = env(sec, 0.05 if d < 0.02 else 0.09)[: e.size - s]
        e[s:s + part.size] += part
    return norm(n * e, 0.75)


def sub_wub():
    """Off-beat bass 'wub' unlocked when the vinyl lands on the platter."""
    sec = 0.32
    t = t_axis(sec)
    f = 55.0
    saw = 2 * ((t * f) % 1) - 1
    lfo = 0.5 + 0.5 * np.sin(2 * np.pi * 9 * t)
    x = lowpass(saw, 300 + 900 * lfo) + 0.6 * np.sin(2 * np.pi * f * t)
    return norm(soft(x, 1.8) * env(sec, 0.2, 0.01), 0.85)


def scratch():
    """Vinyl scratch: band noise + tone whose pitch goes forward-back-forward."""
    sec = 0.32
    t = t_axis(sec)
    speed = np.sin(2 * np.pi * 3.2 * t)              # hand moving the record
    f = 300 + 900 * np.abs(speed)
    tone = np.sin(2 * np.pi * np.cumsum(f) / SR)
    n = highpass(lowpass(noise(sec), 3500), 400)
    x = (0.6 * tone + 0.8 * n) * (0.3 + 0.7 * np.abs(speed)) * env(sec, 0.25, 0.005)
    return norm(x, 0.7)


def skip():
    """Short needle skip, for an off-beat tap."""
    sec = 0.12
    return norm(highpass(noise(sec), 1500) * env(sec, 0.02) + 0.3 * sweep(900, 300, sec) * env(sec, 0.03), 0.5)


def needle_land():
    sec = 0.6
    thump = sweep(120, 60, sec) * env(sec, 0.05)
    crackle = np.zeros(int(SR * sec))
    idx = RNG.integers(0, crackle.size, 60)
    crackle[idx] = RNG.uniform(-1, 1, idx.size)
    crackle = lowpass(crackle, 4000) * 4
    return norm(thump + crackle * np.linspace(1, 0.4, crackle.size), 0.8)


def riser():
    """1.2 s white-noise riser + rising tone: the last stretch before the lock (for every tier: no spoiler)."""
    sec = 1.2
    t = t_axis(sec)
    k = t / sec
    n = noise(sec)
    # crude rising band: high-pass moves up by blending
    x = (1 - k) * lowpass(n, 2500) + k * highpass(n, 3000)
    tone = sweep(200, 1600, sec, 1.6) * 0.35
    return norm((x * 0.6 + tone) * (k ** 1.5), 0.8)


def drop_boom():
    """The DROP (high tiers): huge sub boom + distorted crunch + air."""
    sec = 2.4
    sub = sweep(90, 32, sec, 0.25) * env(sec, 0.9, 0.003)
    crunch = soft(sweep(180, 50, 0.4) * env(0.4, 0.12), 6.0)
    air = highpass(noise(sec), 5000) * env(sec, 0.5) * 0.25
    x = sub * 1.0 + air
    x[: crunch.size] += crunch * 0.7
    return norm(soft(x, 1.6), 0.95)


def air_horn():
    """Party horn stab, three detuned saws, used only for Huyền thoại after the drop."""
    sec = 1.1
    t = t_axis(sec)
    x = np.zeros_like(t)
    for f in (466.0, 470.0, 587.0, 699.0):
        x += 2 * ((t * f) % 1) - 1
    e = np.clip(t / 0.02, 0, 1) * np.where(t < 0.85, 1.0, np.exp(-(t - 0.85) / 0.06))
    # three stabs: ta-ta-taaa
    gate = np.ones_like(t)
    gate[(t > 0.13) & (t < 0.18)] = 0
    gate[(t > 0.31) & (t < 0.36)] = 0
    return norm(lowpass(x, 3500) * e * gate, 0.7)


def record_stop():
    """Rác: the turntable is switched off, everything slows to a groan."""
    sec = 1.3
    t = t_axis(sec)
    k = t / sec
    f = 110 * (1 - k) ** 2 + 8
    x = np.sin(2 * np.pi * np.cumsum(f) / SR) + 0.5 * np.sin(4 * np.pi * np.cumsum(f) / SR)
    return norm(soft(x, 1.5) * (1 - k) ** 0.6, 0.8)


def deflate():
    """Rác: 'xịt' — the bass speaker sags, a farty puff of air out of the port."""
    sec = 0.9
    t = t_axis(sec)
    f = 140 * (1 - 0.6 * t / sec) + 25 * np.sin(2 * np.pi * 22 * t)   # wobbly buzz
    buzz = soft(np.sin(2 * np.pi * np.cumsum(f) / SR), 3.0)
    puff = lowpass(noise(sec), 1200)
    e = np.clip(t / 0.03, 0, 1) * np.exp(-t / 0.35)
    return norm((0.7 * buzz + 0.6 * puff) * e, 0.75)


def plug_pop():
    sec = 0.25
    return norm(sweep(1400, 300, sec) * env(sec, 0.03) + highpass(noise(sec), 2500) * env(sec, 0.01), 0.6)


def wheels():
    """Loa kẹo kéo rolling in: rumble with a squeaky wheel (loop-free, 1.6 s)."""
    sec = 1.6
    t = t_axis(sec)
    rumble = lowpass(noise(sec), 180) * 3
    squeak = np.sin(2 * np.pi * (1900 + 120 * np.sin(2 * np.pi * 3 * t)) * t) * (np.sin(2 * np.pi * 2.6 * t) > 0.8)
    e = np.clip(t / 0.2, 0, 1) * np.clip((sec - t) / 0.3, 0, 1)
    return norm((rumble + 0.15 * squeak) * e, 0.55)


def feedback():
    """Mic feedback whine as the loa kẹo kéo powers on (short, not painful)."""
    sec = 0.7
    t = t_axis(sec)
    x = np.sin(2 * np.pi * 2350 * t) * np.clip(t / 0.3, 0, 1) * np.exp(-np.maximum(0, t - 0.45) / 0.05)
    return norm(x, 0.35)


SOUNDS = {
    "sfx_amnhac_seq_kick.wav": kick,
    "sfx_amnhac_seq_hat.wav": hat,
    "sfx_amnhac_seq_clap.wav": clap,
    "sfx_amnhac_seq_wub.wav": sub_wub,
    "sfx_amnhac_seq_scratch.wav": scratch,
    "sfx_amnhac_seq_skip.wav": skip,
    "sfx_amnhac_seq_needle.wav": needle_land,
    "sfx_amnhac_seq_riser.wav": riser,
    "sfx_amnhac_seq_drop.wav": drop_boom,
    "sfx_amnhac_seq_horn.wav": air_horn,
    "sfx_amnhac_seq_recordstop.wav": record_stop,
    "sfx_amnhac_seq_deflate.wav": deflate,
    "sfx_amnhac_seq_plugpop.wav": plug_pop,
    "sfx_amnhac_seq_wheels.wav": wheels,
    "sfx_amnhac_seq_feedback.wav": feedback,
}


def write(path, x):
    data = (np.clip(x, -1, 1) * 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(here, "..", "..", "Assets", "_Project", "Audio", "Chests", "am_nhac", "seq")
    os.makedirs(out, exist_ok=True)
    for name, fn in SOUNDS.items():
        x = fn()
        write(os.path.join(out, name), x)
        print(f"{name}: {x.size / SR:.2f} s, peak {np.max(np.abs(x)):.2f}")


if __name__ == "__main__":
    main()

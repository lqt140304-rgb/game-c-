#!/usr/bin/env python3
"""Synthesise the FALLBACK chest-opening sounds (âm thanh tạm tổng hợp bằng code).

The sounds actually used are the ElevenLabs mp3 files in Assets/_Project/Audio/Chests/ (owner approved).
This script is kept as a free fallback: it writes only into the Synth/ subfolder, never over the mp3 files.

No samples, no paid service: every sound is built from sine waves, noise and simple filters with numpy,
so there is no copyright question. Replace them with real recordings later; the file names are what
ChestFxPresets / the Chest FX profile expect.

    python3 Tools/Audio/make_chest_sfx.py [output_dir]

Default output: Assets/_Project/Audio/Chests/Synth/ (WAV, mono, 44.1 kHz, 16-bit). Deterministic (fixed seed).
Needs numpy only (the wave module is in the standard library).
"""
import os
import sys
import wave

import numpy as np

SR = 44100
RNG = np.random.default_rng(20261124)


# ---------------------------------------------------------------- building blocks

def t_axis(seconds):
    return np.arange(int(SR * seconds)) / SR


def env_exp(seconds, decay, attack=0.002):
    """Fast attack, exponential decay (decay = time constant in seconds)."""
    t = t_axis(seconds)
    a = np.clip(t / max(attack, 1e-6), 0, 1)
    return a * np.exp(-t / decay)


def sine(freq, seconds, phase=0.0):
    t = t_axis(seconds)
    if np.ndim(freq) == 0:
        return np.sin(2 * np.pi * freq * t + phase)
    return np.sin(2 * np.pi * np.cumsum(freq) / SR + phase)


def noise(seconds):
    return RNG.uniform(-1, 1, int(SR * seconds))


def onepole_lp(x, cutoff):
    """One-pole low-pass. cutoff may be an array (sweep)."""
    cutoff = np.broadcast_to(np.asarray(cutoff, dtype=float), x.shape)
    a = np.exp(-2 * np.pi * cutoff / SR)
    y = np.empty_like(x)
    z = 0.0
    for i in range(len(x)):
        z = (1 - a[i]) * x[i] + a[i] * z
        y[i] = z
    return y


def bandpass(x, center, q):
    """RBJ biquad band-pass (constant 0 dB peak). center may be an array (sweep)."""
    center = np.broadcast_to(np.asarray(center, dtype=float), x.shape)
    y = np.zeros_like(x)
    x1 = x2 = y1 = y2 = 0.0
    for i in range(len(x)):
        w0 = 2 * np.pi * min(center[i], SR * 0.45) / SR
        alpha = np.sin(w0) / (2 * q)
        cw = np.cos(w0)
        a0 = 1 + alpha
        b0, b2 = alpha / a0, -alpha / a0
        a1, a2 = -2 * cw / a0, (1 - alpha) / a0
        yi = b0 * x[i] + b2 * x2 - a1 * y1 - a2 * y2
        x2, x1 = x1, x[i]
        y2, y1 = y1, yi
        y[i] = yi
    return y


def bell(freq, seconds, decay, partials=((1.0, 1.0), (2.76, 0.45), (5.40, 0.25), (8.93, 0.12)), detune=0.0):
    """Struck-metal tone: inharmonic partials, higher ones die faster."""
    out = np.zeros(int(SR * seconds))
    for ratio, amp in partials:
        f = freq * ratio * (1 + detune * RNG.uniform(-1, 1))
        out += amp * sine(f, seconds, RNG.uniform(0, 6.28)) * env_exp(seconds, decay / (1 + 0.6 * (ratio - 1)))
    return out


def mix(length_s, *parts):
    """parts = (start_seconds, signal, gain)."""
    out = np.zeros(int(SR * length_s))
    for start, sig, gain in parts:
        i = int(SR * start)
        n = min(len(sig), len(out) - i)
        if n > 0:
            out[i:i + n] += gain * sig[:n]
    return out


def fade(x, fade_in=0.002, fade_out=0.05):
    n_in = max(1, int(SR * fade_in))
    n_out = max(1, int(SR * fade_out))
    x = x.copy()
    x[:n_in] *= np.linspace(0, 1, n_in)
    x[-n_out:] *= np.linspace(1, 0, n_out)
    return x


def normalise(x, peak_db=-1.0):
    peak = np.max(np.abs(x)) or 1.0
    return x / peak * (10 ** (peak_db / 20))


def write(path, x, peak_db=-1.0):
    x = normalise(fade(x), peak_db)
    data = (np.clip(x, -1, 1) * 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())
    print(f"  {os.path.basename(path):34s} {len(x) / SR:5.2f} s  {os.path.getsize(path) / 1024:6.1f} KB")


# ---------------------------------------------------------------- sounds

def tick(freq):
    """Roulette tick: a tiny click plus a short wooden/metal pip. Pitch is raised in Unity as it slows."""
    d = 0.06
    click = noise(d) * env_exp(d, 0.003)
    pip = sine(freq, d) * env_exp(d, 0.012) + 0.4 * sine(freq * 2.01, d) * env_exp(d, 0.006)
    return bandpass(click, freq * 1.5, 2.0) * 0.8 + pip


def lock_rac():
    """Rác: a dull 'bonk' and a sad two-note slide down (comedic)."""
    L = 1.0
    thud = onepole_lp(noise(0.2), 300) * env_exp(0.2, 0.05) * 3
    body = sine(110, 0.25) * env_exp(0.25, 0.08)
    f = np.concatenate([np.full(int(SR * 0.22), 330.0), np.linspace(311, 233, int(SR * 0.5))])
    wah = sine(f, len(f) / SR) * np.concatenate([env_exp(0.22, 0.15), env_exp(0.5, 0.25)]) * 0.5
    wah = onepole_lp(np.sign(wah) * np.abs(wah) ** 0.7, 1400)   # a bit buzzy, kazoo-like
    return mix(L, (0, thud, 0.9), (0, body, 0.8), (0.12, wah, 0.6))


def lock_thuong():
    """Thường: one clear bell 'ding'."""
    return mix(1.1, (0, bell(880, 1.1, 0.45), 1.0), (0, noise(0.02) * env_exp(0.02, 0.004), 0.3))


def lock_hiem():
    """Hiếm: two-note chime going up a major third, slight shimmer."""
    return mix(1.5,
               (0.0, bell(784, 1.4, 0.5), 0.8),
               (0.12, bell(988, 1.3, 0.6), 0.9),
               (0.12, onepole_lp(noise(1.2), 6000) * env_exp(1.2, 0.3) * 0.08, 1.0))


def lock_rathiem():
    """Rất hiếm: arpeggio up a major triad + octave, soft pad under it."""
    notes = [659.3, 830.6, 987.8, 1318.5]
    parts = [(0.09 * i, bell(f, 1.8 - 0.09 * i, 0.7), 0.7) for i, f in enumerate(notes)]
    pad_t = 1.9
    pad = sum(sine(f * (1 + d), pad_t) for f in (329.6, 415.3, 493.9) for d in (-0.003, 0.003))
    pad = pad * np.minimum(t_axis(pad_t) / 0.25, 1) * np.exp(-t_axis(pad_t) / 0.9) * 0.12
    return mix(2.0, *parts, (0, pad, 1.0))


def lock_huyenthoai():
    """Huyền thoại: low boom, bright arpeggio, wide 'choir' chord and sparkle tail. The longest and biggest."""
    L = 3.4
    boom = mix(0.9,
               (0, sine(np.linspace(90, 40, int(SR * 0.9)), 0.9) * env_exp(0.9, 0.3), 1.2),
               (0, onepole_lp(noise(0.5), 200) * env_exp(0.5, 0.08), 1.5))
    notes = [523.3, 659.3, 784.0, 1046.5, 1318.5, 1568.0]
    arp = [(0.15 + 0.07 * i, bell(f, 2.4, 0.9), 0.55) for i, f in enumerate(notes)]
    # "Choir": stacked detuned saw-ish waves through a low-pass, slow swell.
    ct = 3.0
    chord = np.zeros(int(SR * ct))
    for f in (261.6, 329.6, 392.0, 523.3):
        for d in (-0.006, -0.002, 0.002, 0.006):
            ph = (f * (1 + d) * t_axis(ct) + RNG.uniform()) % 1.0
            chord += 2 * ph - 1
    chord = onepole_lp(onepole_lp(chord, 1800), 2400)
    swell = np.minimum(t_axis(ct) / 0.5, 1) * np.exp(-np.maximum(t_axis(ct) - 0.8, 0) / 1.0)
    chord = chord * swell * 0.05
    sparkle = np.zeros(int(SR * 2.6))
    for _ in range(26):
        s = int(RNG.uniform(0, 2.2) * SR)
        b = bell(RNG.uniform(2500, 5200), 0.3, 0.08, partials=((1, 1), (2.4, 0.3)))
        n = min(len(b), len(sparkle) - s)
        sparkle[s:s + n] += b[:n] * RNG.uniform(0.2, 0.5)
    return mix(L, (0, boom, 1.0), *arp, (0.1, chord, 1.0), (0.4, sparkle, 0.35))


def whoosh():
    """Rising filtered-noise sweep while the silhouette grows."""
    L = 0.9
    t = t_axis(L)
    centre = 300 * (12 ** (t / L))
    shape = np.sin(np.pi * np.clip(t / L, 0, 1)) ** 1.5
    return bandpass(noise(L), centre, 1.4) * shape


def coin(seed_freq):
    """Coin clink: two very short metallic hits a few ms apart."""
    L = 0.45
    parts = ((1.0, 1.0), (1.59, 0.6), (2.14, 0.45), (2.9, 0.3), (3.7, 0.2))
    hit = bell(seed_freq, L, 0.12, partials=parts, detune=0.01)
    hit2 = bell(seed_freq * 1.07, L, 0.08, partials=parts, detune=0.01)
    tick_ = bandpass(noise(0.01), 6000, 1.0) * env_exp(0.01, 0.002)
    return mix(L, (0, tick_, 0.6), (0, hit, 0.8), (0.035, hit2, 0.5))


def lid_creak():
    """Old hinge creak: stick-slip pulse train with a wandering rate through resonant filters."""
    L = 1.0
    t = t_axis(L)
    rate = 55 + 30 * np.sin(2 * np.pi * 1.3 * t) + 20 * np.sin(2 * np.pi * 3.7 * t + 1)
    phase = np.cumsum(rate) / SR
    pulses = (np.diff(np.floor(phase), prepend=0) > 0).astype(float) * RNG.uniform(0.6, 1.0, len(t))
    body = bandpass(pulses, 700, 6) * 3 + bandpass(pulses, 1650, 8) * 2 + bandpass(pulses, 2900, 10)
    shape = np.minimum(t / 0.1, 1) * np.minimum((L - t) / 0.2, 1)
    return body * shape


def lid_thud():
    """Lid hitting fully open: a soft wooden knock."""
    L = 0.4
    knock = bandpass(noise(L), 180, 1.5) * env_exp(L, 0.05) * 4
    tone = sine(140, L) * env_exp(L, 0.07)
    return knock + tone


def open_shimmer():
    """Bed under the roulette: airy shimmer that swells, with a slow tremolo."""
    L = 2.6
    t = t_axis(L)
    air = bandpass(noise(L), 4200, 0.8) * 0.5
    tones = sum(sine(f, L, RNG.uniform(0, 6)) * (0.5 + 0.5 * np.sin(2 * np.pi * r * t)) for f, r in ((1760, 5.1), (2217, 6.3), (2637, 7.2)))
    shape = np.minimum(t / 0.8, 1) * np.minimum((L - t) / 0.6, 1)
    return (air + tones * 0.12) * shape


# ---------------------------------------------------------------- main

def main():
    repo = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(repo, "Assets", "_Project", "Audio", "Chests", "Synth")
    os.makedirs(out, exist_ok=True)
    if any(f.endswith(".mp3") for f in os.listdir(out)):
        sys.exit("Refusing to write: " + out + " holds the ElevenLabs mp3 files. Use the Synth/ folder.")
    print("Writing placeholder chest sounds to", out)

    sounds = [
        ("sfx_chest_tick_a.wav", tick(1900), -6),
        ("sfx_chest_tick_b.wav", tick(2150), -6),
        ("sfx_chest_lock_0_rac.wav", lock_rac(), -3),
        ("sfx_chest_lock_1_thuong.wav", lock_thuong(), -3),
        ("sfx_chest_lock_2_hiem.wav", lock_hiem(), -2),
        ("sfx_chest_lock_3_rathiem.wav", lock_rathiem(), -1.5),
        ("sfx_chest_lock_4_huyenthoai.wav", lock_huyenthoai(), -1),
        ("sfx_chest_whoosh.wav", whoosh(), -4),
        ("sfx_chest_open_shimmer.wav", open_shimmer(), -8),
        ("sfx_chest_lid_creak.wav", lid_creak(), -5),
        ("sfx_chest_lid_thud.wav", lid_thud(), -4),
        ("sfx_coin_clink_1.wav", coin(2800), -5),
        ("sfx_coin_clink_2.wav", coin(3150), -5),
        ("sfx_coin_clink_3.wav", coin(3500), -5),
    ]
    for name, signal, peak in sounds:
        write(os.path.join(out, name), signal, peak)


if __name__ == "__main__":
    main()

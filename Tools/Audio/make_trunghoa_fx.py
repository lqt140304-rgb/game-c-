#!/usr/bin/env python3
"""Âm tổng hợp riêng cho kịch bản Chest_TrungHoa "Đêm Giao Thừa" (TrungHoaHook).

Chỉ sóng sin + nhiễu + lọc đơn giản bằng numpy, hạt giống cố định -> không vướng bản quyền, tạo lại y hệt.
    python3 Tools/Audio/make_trunghoa_fx.py [thư_mục_ra]
Mặc định: Assets/_Project/Audio/Chests/trung_hoa/fx/ (WAV mono 44,1 kHz 16-bit).
"""
import os, sys, wave
import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)

def t(s): return np.arange(int(SR * s)) / SR
def noise(s): return RNG.uniform(-1, 1, int(SR * s))
def lowpass(x, a):  # một cực, a in (0,1): nhỏ = tối
    y = np.empty_like(x); acc = 0.0
    for i, v in enumerate(x): acc += a * (v - acc); y[i] = acc
    return y
def highpass(x, a): return x - lowpass(x, a)
def env(s, decay, attack=0.002):
    tt = t(s); return np.clip(tt / attack, 0, 1) * np.exp(-tt / decay)
def pad(x, s):
    n = int(SR * s); return np.pad(x, (0, max(0, n - len(x))))[:n]
def mix_at(buf, x, at):
    i = int(SR * at); n = min(len(x), len(buf) - i)
    if n > 0: buf[i:i + n] += x[:n]
def norm(x, peak=0.9):
    m = np.max(np.abs(x)); return x * (peak / m) if m > 0 else x

def pop(dur=0.25, bright=0.5):
    x = noise(dur) * env(dur, 0.025, 0.0005)
    x = highpass(lowpass(x, bright), 0.02)
    x += np.sin(2 * np.pi * 90 * t(dur)) * env(dur, 0.03) * 0.6   # cú đấm thấp
    return x

def drum_hit(f=95, dur=0.4):
    tt = t(dur); f_t = f * (1 + 0.6 * np.exp(-tt / 0.03))
    return np.sin(2 * np.pi * np.cumsum(f_t) / SR) * env(dur, 0.12) + lowpass(noise(dur), 0.2) * env(dur, 0.01) * 0.3

def cymbal(dur=1.6, decay=0.5):
    x = highpass(noise(dur), 0.3)
    tt = t(dur)
    for f in (3150, 4370, 5610, 6830): x += 0.15 * np.sign(np.sin(2 * np.pi * f * tt))
    return x * env(dur, decay)

def drum_loop():
    # 2 nhịp 4/4 ở 120 bpm = 2,0 s; trống lân: TÙNG . tùng TÙNG | . tùng TÙNG chập — lặp liền mạch
    L = 2.0; buf = np.zeros(int(SR * L)); beat = 0.25
    pattern = [(0, 1.0, 1), (2, 0.5, 1), (3, 1.0, 1), (5, 0.5, 1), (6, 1.0, 1), (7, 0.6, 0)]
    for step, vol, kind in pattern:
        at = step * beat
        mix_at(buf, drum_hit(92 if kind else 92, 0.45) * vol, at)
        if kind == 0 or step in (3, 6): mix_at(buf, cymbal(0.3, 0.06) * 0.25 * vol, at)
    # đuôi tràn đầu vòng: cộng phần thừa vào đầu để lặp không khựng
    return norm(buf, 0.8)

def fuse_hiss():
    L = 2.0; x = highpass(noise(L), 0.35) * (0.6 + 0.4 * lowpass(noise(L), 0.002) * 8)
    crackle = np.zeros(int(SR * L))
    for _ in range(40): mix_at(crackle, pop(0.02, 0.9) * RNG.uniform(0.2, 0.6), RNG.uniform(0, L - 0.03))
    x = x * 0.4 + crackle
    fade = int(SR * 0.05); x[:fade] *= np.linspace(0, 1, fade); x[-fade:] *= np.linspace(1, 0, fade)
    return norm(x, 0.6)

def match():
    x = highpass(noise(0.5), 0.2) * env(0.5, 0.12, 0.01)
    return norm(x + pop(0.5, 0.6) * 0.3, 0.7)

def big_bang():
    d = 1.5; x = pop(d, 0.6) * 1.0
    x += lowpass(noise(d), 0.05) * env(d, 0.35) * 0.8
    x += np.sin(2 * np.pi * 55 * t(d)) * env(d, 0.25)
    return norm(x, 0.95)

def fizzle():
    d = 1.6; tt = t(d)
    x = highpass(noise(d), 0.25) * np.exp(-tt / 0.6) * (1 - np.exp(-tt / 0.02))
    x = lowpass(x, 0.6) * np.clip(1.2 - tt / d, 0, 1)
    tail = pop(0.12, 0.1) * 0.25  # "pụt" nhỏ xíu cuối cùng
    out = pad(x, d + 0.2); mix_at(out, tail, d - 0.05)
    return norm(out, 0.7)

def slide_down():
    d = 1.2; tt = t(d); f = 900 * np.exp(-tt * 1.4)
    vib = 1 + 0.02 * np.sin(2 * np.pi * 6 * tt)
    x = np.sin(2 * np.pi * np.cumsum(f * vib) / SR) * np.clip(tt / 0.03, 0, 1) * np.clip((d - tt) / 0.2, 0, 1)
    return norm(x, 0.6)

def whistle():
    d = 1.0; tt = t(d); f = 1200 + 2200 * (tt / d) ** 1.4
    x = np.sin(2 * np.pi * np.cumsum(f) / SR) * 0.5 + highpass(noise(d), 0.3) * 0.25
    return norm(x * np.clip(tt / 0.05, 0, 1) * np.clip((d - tt) / 0.08, 0, 1), 0.6)

def boom():
    d = 2.2; x = big_bang()[: int(SR * 0.6)]
    out = pad(x * 0.8, d)
    crackle = np.zeros(int(SR * d))
    for _ in range(60): mix_at(crackle, pop(0.03, 0.9) * RNG.uniform(0.1, 0.4), RNG.uniform(0.3, d - 0.05))
    return norm(out + crackle, 0.9)

def dud_thud():
    d = 0.5; return norm(drum_hit(70, d) * 0.8 + lowpass(noise(d), 0.1) * env(d, 0.04), 0.7)

def sneeze():
    d = 0.9; tt = t(d)
    ah = np.sin(2 * np.pi * np.cumsum(300 + 250 * tt) / SR) * np.clip(tt / 0.3, 0, 1) * (tt < 0.45)
    choo = highpass(noise(d), 0.3) * env(d, 0.12, 0.005)
    out = pad(ah * 0.3, d); mix_at(out, choo[: int(SR * 0.45)], 0.45)
    return norm(out, 0.7)

def write(path, x):
    x = np.clip(x, -1, 1); data = (x * 32767).astype('<i2').tobytes()
    with wave.open(path, 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes(data)

def main():
    root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(root, 'Assets/_Project/Audio/Chests/trung_hoa/fx')
    os.makedirs(out, exist_ok=True)
    sounds = {
        'sfx_trunghoa_fx_drum_loop': drum_loop(), 'sfx_trunghoa_fx_fuse_hiss': fuse_hiss(),
        'sfx_trunghoa_fx_match': match(), 'sfx_trunghoa_fx_big_bang': big_bang(),
        'sfx_trunghoa_fx_fizzle': fizzle(), 'sfx_trunghoa_fx_slide_down': slide_down(),
        'sfx_trunghoa_fx_rocket_whistle': whistle(), 'sfx_trunghoa_fx_rocket_boom': boom(),
        'sfx_trunghoa_fx_cymbal': norm(cymbal(), 0.7), 'sfx_trunghoa_fx_dud_thud': dud_thud(),
        'sfx_trunghoa_fx_sneeze': sneeze(),
    }
    for i, b in enumerate((0.35, 0.5, 0.7)): sounds[f'sfx_trunghoa_fx_pop_{i + 1}'] = norm(pop(0.3, b), 0.85)
    for n, x in sounds.items():
        write(os.path.join(out, n + '.wav'), x); print(n, f'{len(x) / SR:.2f}s')

if __name__ == '__main__':
    main()

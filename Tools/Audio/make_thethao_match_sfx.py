#!/usr/bin/env python3
"""Âm cho kịch bản luân lưu của Chest_TheThao (TheThaoHook). Tổng hợp thuần bằng numpy: sóng sin, nhiễu, lọc.
Không mẫu thu âm, không dịch vụ trả phí -> không vướng bản quyền. Tất định (hạt cố định).

    python3 Tools/Audio/make_thethao_match_sfx.py [thư_mục_ra]

Mặc định: Assets/_Project/Audio/Chests/the_thao/match/ (WAV mono 44,1 kHz 16-bit). CHƯA AI NGHE THỬ trong game.
"""
import os, sys, wave
import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)

def t(s): return np.arange(int(SR * s)) / SR
def noise(s): return RNG.uniform(-1, 1, int(SR * s))
def lp(x, fc):
    a = np.exp(-2 * np.pi * fc / SR); y = np.zeros_like(x); p = 0.0
    for i, v in enumerate(x): p = (1 - a) * v + a * p; y[i] = p
    return y
def bp(x, fc, q=4.0):
    # biquad band-pass
    w = 2 * np.pi * fc / SR; al = np.sin(w) / (2 * q)
    b0, b2 = al, -al; a0, a1, a2 = 1 + al, -2 * np.cos(w), 1 - al
    y = np.zeros_like(x); x1 = x2 = y1 = y2 = 0.0
    for i, v in enumerate(x):
        o = (b0 * v + b2 * x2 - a1 * y1 - a2 * y2) / a0
        x2, x1, y2, y1 = x1, v, y1, o; y[i] = o
    return y
def env(s, a, d): tt = t(s); return np.clip(tt / max(a, 1e-4), 0, 1) * np.exp(-tt / d)
def fade(x, i=0.003, o=0.03):
    n = len(x); fi, fo = int(SR * i), int(SR * o)
    x = x.copy(); x[:fi] *= np.linspace(0, 1, fi); x[n - fo:] *= np.linspace(1, 0, fo); return x
def sweep(f0, f1, s, shape=np.sin):
    f = np.linspace(f0, f1, int(SR * s)); return shape(2 * np.pi * np.cumsum(f) / SR)
def norm(x, db=-1.0): return x / (np.max(np.abs(x)) + 1e-9) * 10 ** (db / 20)
def write(d, name, x, db=-1.0):
    x = np.clip(norm(fade(x), db), -1, 1)
    with wave.open(os.path.join(d, name), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes((x * 32767).astype("<i2").tobytes())
    print(f"{name:22s} {len(x)/SR:4.2f}s")

def whistle(s, trill=28):
    # còi trọng tài: ~2,9 kHz rung bởi hạt bi (FM nhanh) + hơi thở
    tt = t(s); f = 2900 + 120 * np.sin(2 * np.pi * trill * tt)
    x = np.sin(2 * np.pi * np.cumsum(f) / SR) + 0.3 * np.sin(4 * np.pi * np.cumsum(f) / SR)
    x = x * (1 + 0.5 * np.sin(2 * np.pi * trill * tt)) + 0.15 * bp(noise(s), 2900, 2)
    return x * np.clip(tt / 0.02, 0, 1) * np.clip((s - tt) / 0.05, 0, 1)
def crowd(s, center=500, q=0.8):
    # đám đông: nhiễu lọc dải rộng + nhiều "giọng" formant lắc lư
    x = bp(noise(s), center, q) + 0.5 * bp(noise(s), center * 2.2, 1.5)
    return x * (1 + 0.25 * lp(noise(s), 3) * 20)
def vowel(s, f0a, f0b, formants):
    # "ồ"/"ồ ồ" nhiều người: vài giọng sawtooth lệch nhau qua formant
    out = np.zeros(int(SR * s))
    for k in range(9):
        det = 1 + RNG.uniform(-0.04, 0.04)
        f = np.linspace(f0a, f0b, len(out)) * det
        saw = 2 * ((np.cumsum(f) / SR) % 1) - 1
        out += sum(bp(saw, fm, 6) * g for fm, g in formants)
    return out + 0.3 * bp(noise(s), formants[0][0], 1)

def main():
    d = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "../../Assets/_Project/Audio/Chests/the_thao/match")
    os.makedirs(d, exist_ok=True)
    write(d, "whistle_start.wav", whistle(0.55))
    ws = np.zeros(int(SR * 1.3))
    for st, ln in ((0, .18), (.3, .18), (.6, .6)): seg = whistle(ln); i = int(SR * st); ws[i:i + len(seg)] += seg
    write(d, "whistle_end.wav", ws)
    # đám đông nền 4 s, nối vòng: trộn chéo đầu/cuối
    c = crowd(4.5); n = int(SR * 0.5); c[:n] = c[:n] * np.linspace(0, 1, n) + c[-n:] * np.linspace(1, 0, n)
    write(d, "crowd_loop.wav", c[:int(SR * 4.0)], -6)
    # trống cổ vũ: trống cái + mặt da
    s = 0.35; tt = t(s)
    drum = np.sin(2 * np.pi * np.cumsum(np.linspace(110, 55, len(tt))) / SR) * env(s, .002, .12) + 0.3 * lp(noise(s), 1500) * env(s, .001, .03)
    write(d, "drum.wav", drum)
    s = 0.18; kick = np.sin(2 * np.pi * np.cumsum(np.linspace(180, 60, int(SR * s))) / SR) * env(s, .001, .04) + 0.6 * bp(noise(s), 1200, 1) * env(s, .001, .015)
    write(d, "kick.wav", kick)
    s = 0.22; bounce = np.sin(2 * np.pi * np.cumsum(np.linspace(260, 140, int(SR * s))) / SR) * env(s, .001, .05) + 0.2 * bp(noise(s), 800, 2) * env(s, .001, .01)
    write(d, "bounce.wav", bounce)
    s = 1.6; tt = t(s)   # cột dọc kim loại: các hoạ âm không điều hoà, tắt chậm
    post = sum(g * np.sin(2 * np.pi * f * tt) * np.exp(-tt / dec) for f, g, dec in ((523, 1, .7), (1386, .6, .4), (2620, .4, .25), (4110, .25, .12))) + 0.5 * bp(noise(s), 3000, 1) * env(s, .001, .01)
    write(d, "post_clang.wav", post)
    s = 0.7
    net = lp(noise(s), 6000) * env(s, .03, .2) * (1 + np.sin(2 * np.pi * 9 * t(s)) * .3)
    write(d, "net_swish.wav", net)
    s = 1.4; ooh = vowel(s, 190, 150, ((400, 1), (800, .4))) * np.sin(np.pi * np.clip(t(s) / s, 0, 1)) ** .7
    write(d, "crowd_ooh.wav", ooh)
    s = 1.8; aww = vowel(s, 230, 120, ((650, 1), (1100, .5))) * np.clip(t(s) / .1, 0, 1) * np.exp(-t(s) / 1.0)
    write(d, "crowd_aww.wav", aww)
    s = 3.5; tt = t(s); roar = (crowd(s, 900, .5) + 0.5 * vowel(s, 260, 300, ((700, 1), (1200, .6)))) * np.clip(tt / .15, 0, 1) * np.exp(-np.clip(tt - 1.5, 0, None) / 1.2)
    write(d, "crowd_roar.wav", roar)
    s = 1.3; tt = t(s)   # xì hơi: nhiễu hẹp tụt cao độ + tiếng "pffft" rung
    defl = sweep(900, 140, s) * (1 + 0.6 * np.sin(2 * np.pi * 35 * tt)) * np.exp(-tt / .7) * .5 + lp(noise(s), 2500) * np.exp(-tt / .4) * .6
    write(d, "deflate.wav", defl)
    # kèn buồn "wah wah wah waaah"
    tr = []
    for f, ln in ((311, .35), (294, .35), (277, .35), (262, 1.1)):
        tt = t(ln); fv = f * (1 + (0.02 * np.sin(2 * np.pi * 5 * tt) if ln > 1 else 0))
        ph = np.cumsum(np.full(len(tt), 1.0) * fv) / SR
        saw = 2 * (ph % 1) - 1
        tone = lp(saw, 1100) * np.clip(tt / .03, 0, 1) * np.clip((ln - tt) / .05, 0, 1)
        tr.append(tone)
    write(d, "sad_trombone.wav", np.concatenate(tr))
    s = 0.12; write(d, "var_beep.wav", np.sin(2 * np.pi * 1760 * t(s)) * np.clip((s - t(s)) / .02, 0, 1), -6)
    s = 0.9; write(d, "rewind.wav", sweep(2500, 300, s, lambda p: np.sign(np.sin(p))) * 0.4 * np.sin(np.pi * t(s) / s) + 0.3 * lp(noise(s), 4000) * np.sin(np.pi * t(s) / s))
    s = 2.2; tt = t(s)   # riser: nhiễu + sin trượt lên, to dần
    write(d, "riser.wav", (0.6 * sweep(200, 1400, s) + bp(noise(s), 1500, .7)) * (tt / s) ** 2)
    s = 1.0; tt = t(s); horn = sum(np.sign(np.sin(2 * np.pi * f * tt)) for f in (233, 294, 349)); write(d, "airhorn.wav", lp(horn, 2500) * np.clip(tt / .02, 0, 1) * np.clip((s - tt) / .1, 0, 1))

if __name__ == "__main__":
    main()

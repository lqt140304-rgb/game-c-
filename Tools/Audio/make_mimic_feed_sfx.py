#!/usr/bin/env python3
"""Âm tổng hợp (numpy, không mẫu, không bản quyền) cho kịch bản "Rương Đói" (Chest_Mimic, MimicFeedHook).

    python3 Tools/Audio/make_mimic_feed_sfx.py [thư_mục_ra]

Mặc định: Assets/_Project/Audio/Chests/mimic_feed/ (WAV mono 44,1 kHz 16-bit). Hạt giống cố định.
CHƯA AI NGHE THỬ bằng tai: chỉ kiểm độ dài/đỉnh bằng code.
"""
import os, sys, wave
import numpy as np
from scipy.signal import lfilter, butter  # cần: pip install numpy scipy

SR = 44100
R = np.random.default_rng(20261125)

def t(s): return np.arange(int(SR * s)) / SR
def noise(s): return R.uniform(-1, 1, int(SR * s))
def env(s, decay, attack=0.003):
    x = t(s); return np.clip(x / attack, 0, 1) * np.exp(-x / decay)
def osc(freq, s, kind="sin"):
    f = np.broadcast_to(np.asarray(freq, float), (int(SR * s),))
    ph = 2 * np.pi * np.cumsum(f) / SR
    if kind == "saw": return 2 * ((ph / (2 * np.pi)) % 1) - 1
    if kind == "sq": return np.sign(np.sin(ph))
    return np.sin(ph)
def lp(x, fc, order=2):
    if np.ndim(fc) > 0:  # quét tần cắt: lọc từng khối 1024 mẫu
        o = np.zeros_like(x); zi = None; B = 1024
        for i in range(0, len(x), B):
            b, a = butter(order, min(float(fc[i]), SR * 0.45) / (SR / 2), "low")
            if zi is None: zi = np.zeros(max(len(a), len(b)) - 1)
            o[i:i+B], zi = lfilter(b, a, x[i:i+B], zi=zi)
        return o
    b, a = butter(order, min(fc, SR * 0.45) / (SR / 2), "low"); return lfilter(b, a, x)
def bp(x, lo, hi):
    b, a = butter(2, [lo / (SR / 2), min(hi, SR * 0.45) / (SR / 2)], "band"); return lfilter(b, a, x)
def mix(L, *parts):
    o = np.zeros(int(SR * L))
    for st, sig, g in parts:
        i = int(SR * st); n = min(len(sig), len(o) - i)
        if n > 0: o[i:i + n] += g * sig[:n]
    return o
def write(d, name, x, peak_db=-1.0, loop=False):
    x = x - np.mean(x)
    if not loop:
        n = int(SR * 0.004); x[:n] *= np.linspace(0, 1, n); m = int(SR * 0.04); x[-m:] *= np.linspace(1, 0, m)
    x = x / (np.max(np.abs(x)) or 1) * 10 ** (peak_db / 20)
    p = os.path.join(d, name)
    with wave.open(p, "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((np.clip(x, -1, 1) * 32767).astype("<i2").tobytes())
    print(f"  {name:28s} {len(x)/SR:5.2f} s")

def burp(s, f0, f1):
    f = np.linspace(f0, f1, int(SR * s)) * (1 + 0.06 * np.sin(2 * np.pi * 23 * t(s)))
    x = osc(f, s, "saw") * (0.6 + 0.4 * np.abs(np.sin(2 * np.pi * 11 * t(s))))
    x = bp(x, 150, 900) + 0.3 * lp(noise(s), 500)
    return x * np.clip(t(s) / 0.03, 0, 1) * np.clip((s - t(s)) / 0.15, 0, 1)

def main():
    d = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "../../Assets/_Project/Audio/Chests/mimic_feed")
    os.makedirs(d, exist_ok=True)
    # khịt mũi thức dậy (NGAY khi bấm): tiếng thở mạnh + gầm ngắn trầm
    s = 0.7
    snort = bp(noise(s), 300, 1800) * env(s, 0.12, 0.02)
    grunt = lp(osc(np.linspace(140, 80, int(SR*s)), s, "saw"), 600) * env(s, 0.25, 0.05)
    write(d, "sfx_mf_wake.wav", mix(s, (0, snort, 1.0), (0.12, grunt, 0.8)))
    # bụng réo (vòng lặp 4 s, nối liền): ồ ồ trầm + ục ục
    s = 4.0; x = t(s)
    rumble = lp(noise(s), 120, 4) * 3
    gurgle = sum(osc(90 + 40 * np.sin(2*np.pi*(0.5+k*0.25)*x), s) * (np.sin(2*np.pi*(1.0+0.5*k)*x) > 0.6) for k in range(4)) * 0.25
    y = rumble + lp(gurgle, 400)
    write(d, "sfx_mf_stomach_loop.wav", y * (0.8 + 0.2 * np.sin(2*np.pi*0.25*x)), -3, loop=True)
    # tim: lub-dub
    s = 0.4; th = lambda: lp(osc(55, 0.15), 200) * env(0.15, 0.04)
    write(d, "sfx_mf_heartbeat.wav", mix(s, (0, th(), 1), (0.16, th(), 0.7)))
    # ngoạm: tách gỗ + thịch
    s = 0.35
    write(d, "sfx_mf_chomp.wav", mix(s, (0, bp(noise(0.05), 1500, 6000) * env(0.05, 0.008), 1),
                                       (0.005, lp(osc(np.linspace(160, 50, int(SR*.3)), .3), 300) * env(.3, .07), 1.2)))
    # nhai giòn
    s = 0.22; cr = sum(np.roll(bp(noise(s), 800, 5000) * env(s, 0.015), int(SR * k * 0.04)) * (0.8 ** k) for k in range(4))
    write(d, "sfx_mf_chew.wav", cr + 0.5 * lp(noise(s), 300) * env(s, 0.06), -4)
    # nuốt "ực": tiếng bọt rơi tần số
    s = 0.35
    write(d, "sfx_mf_gulp.wav", lp(osc(np.linspace(420, 120, int(SR*s)) ** 1, s), 900) * env(s, 0.12, 0.02))
    write(d, "sfx_mf_burp_small.wav", burp(0.45, 160, 120))
    write(d, "sfx_mf_burp_big.wav", burp(1.3, 120, 70))
    # dâng (4 s, như nhau mọi bậc): quét tần lên + nhiễu phồng, giữ đỉnh 0,6 s cuối
    s = 4.0; x = t(s); u = np.clip(x / 3.4, 0, 1)
    f = 60 * (2 ** (u * 3.3))
    rise = osc(f, s, "saw") * 0.5 + osc(f * 1.5, s, "saw") * 0.3
    rise = lp(rise, 300 + 4000 * u ** 2) + bp(noise(s), 500, 6000) * u ** 3 * 0.6
    trem = 1 + 0.25 * np.sin(2*np.pi*(4 + 14*u)*x)
    write(d, "sfx_mf_riser.wav", rise * (0.15 + 0.85 * u ** 1.5) * trem, -2)
    # xì hơi xịt (bậc thấp): sóng vuông rung rung tụt cao độ
    s = 1.4; x = t(s)
    f = 220 * 2 ** (-2.2 * x / s)
    pf = osc(f, s, "sq") * (0.5 + 0.5 * np.sin(2*np.pi*31*x))
    write(d, "sfx_mf_deflate.wav", lp(pf, 1400) * np.clip((s - x) / 0.3, 0, 1) + 0.15 * bp(noise(s), 800, 3000) * env(s, 0.5))
    # bùng nổ (bậc cao): nứt + nổ trầm + đuôi lấp lánh
    s = 2.5
    boom = lp(noise(s), 160, 4) * env(s, 0.5, 0.004) * 4 + osc(np.linspace(70, 35, int(SR*s)), s) * env(s, 0.7)
    crack = bp(noise(0.12), 2000, 9000) * env(0.12, 0.03)
    sparkle = sum(osc(1800 * 1.26 ** k, s) * env(s, 0.4 + 0.1*k) for k in range(5)) * 0.06
    write(d, "sfx_mf_boom.wav", mix(s, (0, boom, 1), (0, crack, 0.8), (0.05, sparkle, 1)))
    # nhổ "phụt"
    s = 0.35
    write(d, "sfx_mf_spit.wav", bp(noise(s), 600, 4000) * env(s, 0.06, 0.01) + 0.6 * lp(osc(np.linspace(300, 700, int(SR*s)), s), 1200) * env(s, 0.08))
    # ruồi vo ve (vòng lặp 1 s)
    s = 1.0; x = t(s)
    write(d, "sfx_mf_buzz_loop.wav", bp(osc(180 + 6*np.sin(2*np.pi*3*x), s, "saw"), 150, 3000) * (0.8 + 0.2*np.sin(2*np.pi*9*x)), -6, loop=True)
    # ném vụt
    s = 0.35
    write(d, "sfx_mf_toss.wav", bp(noise(s), 400, 3000) * np.sin(np.pi * t(s) / s) ** 2, -6)
    # nhảy/đáp: thịch gỗ
    s = 0.3
    write(d, "sfx_mf_hop.wav", lp(noise(s), 400) * env(s, 0.05) * 2 + osc(90, s) * env(s, 0.08))
    # cười khúc khích bằng răng: lạch cạch nhanh
    s = 0.6
    g = mix(s, *[(k * 0.045, bp(noise(0.03), 1500, 7000) * env(0.03, 0.006), 1 - k * 0.06) for k in range(12)])
    write(d, "sfx_mf_giggle.wav", g)

if __name__ == "__main__":
    main()

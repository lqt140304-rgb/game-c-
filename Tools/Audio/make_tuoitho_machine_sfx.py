#!/usr/bin/env python3
"""Tổng hợp âm riêng cho "cỗ máy bi ve" của Chest_TuoiTho (chỉ numpy, không mẫu, không bản quyền).

    python3 Tools/Audio/make_tuoitho_machine_sfx.py [thư_mục_ra]

Mặc định ghi vào Assets/_Project/Audio/Chests/tuoi_tho/may_bi/ (WAV mono 44.1 kHz 16-bit, hạt giống cố định).
CHƯA AI NGHE THỬ: tham số chỉnh theo lý thuyết, cần chủ dự án nghe lại trong Unity.
"""
import os, sys, wave
import numpy as np

SR = 44100
RNG = np.random.default_rng(20261125)

def t_axis(s): return np.arange(int(SR * s)) / SR
def env(s, decay, attack=0.002):
    t = t_axis(s); return np.clip(t / attack, 0, 1) * np.exp(-t / decay)
def noise(s): return RNG.uniform(-1, 1, int(SR * s))
def lowpass(x, a):
    y = np.empty_like(x); acc = 0.0
    for i, v in enumerate(x): acc += a * (v - acc); y[i] = acc
    return y
def sweep(f0, f1, s, curve=1.0):
    t = t_axis(s); f = f0 + (f1 - f0) * (t / s) ** curve
    return np.sin(2 * np.pi * np.cumsum(f) / SR)
def pad(x, s):
    out = np.zeros(int(SR * s)); out[:len(x)] += x[:len(out)]; return out
def write(folder, name, x, peak=0.85):
    x = x / (np.max(np.abs(x)) + 1e-9) * peak
    fade = min(len(x), int(0.01 * SR)); x[-fade:] *= np.linspace(1, 0, fade)
    with wave.open(os.path.join(folder, name), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x * 32767).astype("<i2").tobytes())
    print(f"{name}: {len(x)/SR:.2f}s")

def glass(freq, s=0.25):          # bi ve gõ: vài hoạ âm không điều hoà, tắt nhanh
    return sum(np.sin(2*np.pi*freq*k*t_axis(s)) * env(s, 0.03/k) for k in (1, 2.76, 5.4))
def wood(freq, s=0.12):           # domino gỗ: tiếng "cách" khô
    return np.sin(2*np.pi*freq*t_axis(s)) * env(s, 0.012) + lowpass(noise(s), 0.4) * env(s, 0.006)
def xylo(freq, s=0.9):            # đàn mộc cầm đồ chơi
    t = t_axis(s)
    return (np.sin(2*np.pi*freq*t) * env(s, 0.35) + 0.4*np.sin(2*np.pi*freq*4*t) * env(s, 0.05)
            + 0.15*np.sin(2*np.pi*freq*9.8*t) * env(s, 0.015))

def main():
    folder = sys.argv[1] if len(sys.argv) > 1 else os.path.join(
        os.path.dirname(__file__), "..", "..", "Assets/_Project/Audio/Chests/tuoi_tho/may_bi")
    os.makedirs(folder, exist_ok=True)
    # búng bi: tiếng móng tay "tách" + bi va
    flick = pad(lowpass(noise(0.03), 0.7) * env(0.03, 0.004), 0.4)
    flick += pad(np.zeros(int(0.03*SR)), 0.4) + np.concatenate([np.zeros(int(0.03*SR)), glass(2300, 0.37)])
    write(folder, "sfx_tt_flick.wav", flick)
    # bi lăn trên thước: tiếng ù rung lăn tăn
    s = 1.4; t = t_axis(s)
    roll = lowpass(noise(s), 0.05) * (0.6 + 0.4*np.sin(2*np.pi*23*t)) * np.minimum(1, t/0.1) * np.minimum(1, (s-t)/0.3)
    write(folder, "sfx_tt_roll.wav", roll, 0.6)
    write(folder, "sfx_tt_domino.wav", wood(1400))
    write(folder, "sfx_tt_glass.wav", glass(2600))
    # xe đồ chơi kéo trớn: "rừừừ" cao dần
    s = 1.2; t = t_axis(s)
    car = np.sign(sweep(40, 160, s, 0.7)) * 0.4 + sweep(300, 900, s, 0.7) * 0.3
    car = lowpass(car, 0.25) * np.minimum(1, t/0.05) * np.minimum(1, (s-t)/0.2)
    write(folder, "sfx_tt_car.wav", car, 0.6)
    # con quay: tiếng vù vù có nhịp lắc
    s = 2.0; t = t_axis(s)
    top = (sweep(520, 430, s) + 0.5*sweep(1040, 860, s)) * (0.6 + 0.4*np.sin(2*np.pi*(9 - 3*t/s)*t))
    write(folder, "sfx_tt_top.wav", top * np.minimum(1, t/0.05) * np.minimum(1, (s-t)/0.5), 0.5)
    # dây thun bật: "tưng" uốn cao độ xuống
    s = 0.6; t = t_axis(s)
    band = sweep(260, 140, s, 0.3) * env(s, 0.18) + lowpass(noise(s), 0.5) * env(s, 0.01)
    write(folder, "sfx_tt_band.wav", band)
    # bi đập nắp rương: "cộp" gỗ rỗng
    write(folder, "sfx_tt_bonk.wav", wood(320, 0.35) + 0.6*np.sin(2*np.pi*180*t_axis(0.35))*env(0.35, 0.08))
    write(folder, "sfx_tt_xylo.wav", xylo(523.25))   # C5, script đổi pitch theo thang ngũ cung
    # còi ống trượt (slide whistle) dâng: độ căng 3,5 s
    s = 3.5; t = t_axis(s)
    rise = sweep(400, 1500, s, 1.6) * (1 + 0.03*np.sin(2*np.pi*6*t)) * np.minimum(1, t/0.2)
    write(folder, "sfx_tt_riser.wav", rise + 0.1*lowpass(noise(s), 0.1), 0.45)
    # bậc thấp: còi trượt rơi xuống + tiếng xì hơi bong bóng (xịt)
    s = 1.6; t = t_axis(s)
    fall = sweep(1400, 180, 0.7, 0.5) * np.minimum(1, (0.7 - t_axis(0.7))/0.05)
    fart = np.sign(sweep(95, 60, 0.9)) * lowpass(noise(0.9)*0.5 + 1, 0.3) * env(0.9, 0.35)
    write(folder, "sfx_tt_fail.wav", np.concatenate([fall, lowpass(fart, 0.2)]), 0.7)
    # bậc cao: còi lên đỉnh + quét mộc cầm + kèn giấy sinh nhật (party horn)
    s = 2.4; win = np.zeros(int(SR*s))
    for i, semi in enumerate([0, 2, 4, 7, 9, 12, 14, 16, 19, 24]):
        n = xylo(523.25 * 2**(semi/12), 1.2); o = int(i*0.06*SR); win[o:o+len(n)] += n[:len(win)-o]
    horn = np.sign(sweep(440, 470, 1.0)) * 0.25 * np.minimum(1, t_axis(1.0)/0.04) * np.minimum(1, (1.0-t_axis(1.0))/0.2)
    o = int(0.62*SR); win[o:o+len(horn)] += lowpass(horn, 0.3)
    write(folder, "sfx_tt_win.wav", win)
    # máy bay giấy: tiếng giấy phập phồng lướt gió
    s = 1.6; t = t_axis(s)
    write(folder, "sfx_tt_plane.wav", lowpass(noise(s), 0.08) * (0.5 + 0.5*np.sin(2*np.pi*14*t)**2) * np.sin(np.pi*t/s), 0.5)
    # vò giấy
    s = 0.7; crumple = noise(s) * (RNG.uniform(0, 1, int(SR*s)) > 0.97) ; crumple = lowpass(crumple, 0.5) + lowpass(noise(s), 0.3)*0.15
    write(folder, "sfx_tt_crumple.wav", crumple * np.minimum(1, (s - t_axis(s))/0.2), 0.6)

if __name__ == "__main__":
    main()

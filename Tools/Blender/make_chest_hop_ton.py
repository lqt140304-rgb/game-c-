"""Rương H2 Hộp Kim Loại Mờ: hộp tôn sơn xám mờ, khóa gài + ổ khóa nhỏ, tem chữ Việt, vết gỉ. Dựng bằng bpy, không texture.

Chạy: python make_chest_hop_ton.py [-- <file .glb ra>]   (mặc định Assets/_Project/Art/Chests/Chest_HopTon.glb)
Cấu trúc: Chest_HopTon -> Body, Lid (gốc ở bản lề sau, ở mép dưới nắp, xoay quanh X, mở bằng góc âm), Glow, Seal (băng keo), Binding (khóa gài + ổ khóa).
Mặt trước -Y. Kích thước ~0,50 x 0,34 x 0,28 m.
"""
import math

import chest_v1_lib as L

OUT = L.out_path("Chest_HopTon.glb")

W, D, C = 0.46, 0.32, 0.016     # thân: rộng, sâu, cạnh vát
HB = 0.185                       # miệng thân
WALL = 0.008
LW, LD = W + 0.02, D + 0.02      # nắp trùm ra ngoài thân
LZ0, LZ1 = HB - 0.035, HB + 0.075  # mép dưới và mặt trên nắp
PIVOT = (0.0, LD / 2, LZ0)

PALETTE = {
    "tin": {"c": (0.46, 0.47, 0.48), "r": 0.75, "m": 0.35}, "tin_dk": {"c": (0.34, 0.35, 0.37), "r": 0.8, "m": 0.35},
    "inside": {"c": (0.10, 0.10, 0.11), "r": 1.0}, "rust": {"c": (0.52, 0.28, 0.14), "r": 0.95},
    "rust_dk": {"c": (0.36, 0.19, 0.10), "r": 0.95}, "paper": {"c": (0.84, 0.80, 0.66), "r": 0.95},
    "news": {"c": (0.72, 0.70, 0.64), "r": 0.95}, "red": {"c": (0.72, 0.16, 0.14), "r": 0.6},
    "yellow": {"c": (0.86, 0.72, 0.25), "r": 0.6}, "ink": {"c": (0.12, 0.11, 0.10), "r": 0.9},
    "brass": {"c": (0.70, 0.55, 0.22), "r": 0.4, "m": 0.8}, "steel": {"c": (0.60, 0.61, 0.63), "r": 0.4, "m": 0.8},
    "tape": {"c": (0.66, 0.50, 0.30), "r": 0.8}, "glow": {"c": (0.70, 0.92, 1.0), "r": 0.5, "e": 2.0},
}
S = L.Scene(PALETTE)

# ---------------- Body ----------------
b = S.part("Body")
mats = {"outer": "tin", "rim": "tin_dk", "inner": "inside", "bottom": "tin_dk"}
L.shell(b, mats, W, D, C, 0.0, HB, WALL, floor_z=0.012)
# hai gân nổi quanh thân
for z in (0.05, 0.12):
    prof = [(W, z), (W + 0.012, z + 0.005), (W + 0.012, z + 0.017), (W, z + 0.022)]
    rings = [L.ring_at(L.rrect(w_, D + (w_ - W), C + (w_ - W) / 2), zz) for w_, zz in prof]
    for k in range(3):
        b.ring_band("tin", rings[k], rings[k + 1])
# giấy báo lót trong hộp
b.box("news", (-0.17, -0.10, 0.012), (0.10, 0.05, 0.016), tf=None)
b.box("news", (-0.05, -0.05, 0.016), (0.18, 0.11, 0.019))
b.cyl("news", (-0.12, 0.08, 0.025), (0.12, 0.1, 0.09), 0.022, n=5, r2=0.012, caps=False)
# quai xách hai bên: tai đỡ và thanh chữ U
for sx in (-1, 1):
    xo = sx * (W / 2 + 0.006)
    for yy in (-0.05, 0.05):
        b.box("steel", (xo - 0.004, yy - 0.008, 0.095), (xo + 0.004, yy + 0.008, 0.125))
    xs = sx * (W / 2 + 0.03)
    for a, c in (((-0.05, 0.12), (-0.05, 0.09)), ((-0.05, 0.09), (0.05, 0.09)), ((0.05, 0.09), (0.05, 0.12))):
        b.beam("steel", (xs, a[0], a[1]), (xs, c[0], c[1]), 0.008, 0.008, up=(sx, 0, 0))
# tem chữ Việt phía trước thân (viền đỏ, nền giấy)
yf = -D / 2 - 0.0035
b.box("red", (-0.205, yf - 0.001, 0.025), (-0.02, yf, 0.1))
b.box("paper", (-0.2, yf - 0.002, 0.03), (-0.025, yf - 0.001, 0.095))
b.text("ink", "HÀNG THANH LÝ", 0.017, (-0.1125, yf - 0.0035, 0.076), facing="-y")
b.text("red", "KHÔNG ĐỔI TRẢ", 0.014, (-0.1125, yf - 0.0035, 0.047), facing="-y")
# gỉ: mảng nâu cam ở góc đáy, quanh gân và mép
rs = [((0.20, yf - 0.001, 0.04), (0, -1, 0), 0.030, 1), ((-0.22, yf - 0.001, 0.14), (0, -1, 0), 0.026, 2),
      ((0.12, yf - 0.001, 0.14), (0, -1, 0), 0.020, 3), ((W / 2 + 0.0, 0.08, 0.05), (1, 0, 0), 0.028, 4),
      ((-W / 2 - 0.0, -0.06, 0.15), (-1, 0, 0), 0.026, 5), ((0.1, D / 2 + 0.0, 0.05), (0, 1, 0), 0.030, 6)]
for c, n, r, s in rs:
    L.patch(b, "rust", c, n, r, n=7, seed=s)
    L.patch(b, "rust_dk", c, n, r * 0.5, n=6, seed=s + 10, flat_off=0.0028)
# bản lề: hai ống ở phía sau
for sx in (-0.15, 0.15):
    b.cyl("steel", (sx - 0.03, PIVOT[1] + 0.004, PIVOT[2]), (sx + 0.03, PIVOT[1] + 0.004, PIVOT[2]), 0.008, n=6)
# móc chờ khóa gài trên thân (phía trước)
b.box("steel", (-0.025, -D / 2 - 0.006, HB - 0.07), (0.025, -D / 2, HB - 0.02))
b.box("brass", (-0.008, -D / 2 - 0.022, HB - 0.055), (0.008, -D / 2 - 0.006, HB - 0.047))
body = b.build()

# ---------------- Lid ----------------
l = S.part("Lid")
lm = {"outer": "tin", "rim": "tin_dk", "inner": "inside"}
# vỏ nắp: úp ngược (đáy là mặt trên). Dựng bằng vành ngoài + mặt trên kín và vành trong rỗng
o0 = L.ring_at(L.rrect(LW, LD, C + 0.004), LZ0)
o1 = L.ring_at(L.rrect(LW, LD, C + 0.004), LZ1 - 0.01)
o2 = L.ring_at(L.rrect(LW - 0.03, LD - 0.03, C + 0.004), LZ1)
l.ring_band("tin", o0, o1)
l.ring_band("tin", o1, o2)
l.poly("tin", o2, toward=(0, 0, 1))
# mép dưới nắp: vành mỏng rồi tường trong
i0 = L.ring_at(L.rrect(LW - 2 * WALL, LD - 2 * WALL, C), LZ0)
i1 = L.ring_at(L.rrect(LW - 2 * WALL, LD - 2 * WALL, C), LZ1 - 0.012)
for i in range(8):
    j = (i + 1) % 8
    l.poly("tin_dk", [o0[i], i0[i], i0[j], o0[j]], toward=(0, 0, -1))
    mid = (i0[i] + i0[j]) / 2
    l.poly("inside", [i0[i], i0[j], i1[j], i1[i]], toward=(-mid.x, -mid.y, 0))
l.poly("inside", i1[::-1], toward=(0, 0, -1))
# tấm dập nổi trên mặt nắp + nhãn "DỄ VỠ"
zt = LZ1
pan = L.ring_at(L.rrect(LW - 0.1, LD - 0.1, 0.012), zt)
pan2 = L.ring_at(L.rrect(LW - 0.1, LD - 0.1, 0.012), zt + 0.003)
l.ring_band("tin_dk", pan, pan2)
l.poly("tin_dk", pan2, toward=(0, 0, 1))
l.box("yellow", (-0.17, -0.105, zt + 0.0031), (-0.03, -0.045, zt + 0.0041))
l.text("red", "DỄ VỠ", 0.028, (-0.10, -0.075, zt + 0.0052), facing="+z")
l.box("red", (0.07, -0.09, zt + 0.0031), (0.11, -0.05, zt + 0.0036))
# gỉ trên nắp và hông nắp
for c, n, r, s in (((0.17, 0.09, zt + 0.0005), (0, 0, 1), 0.04, 21), ((-0.15, 0.1, zt + 0.0005), (0, 0, 1), 0.028, 22),
                   ((LW / 2 + 0.0, -0.03, LZ0 + 0.04), (1, 0, 0), 0.03, 23)):
    L.patch(l, "rust", c, n, r, n=7, seed=s)
    L.patch(l, "rust_dk", c, n, r * 0.5, n=6, seed=s + 10, flat_off=0.0028)
# hai tay bản lề phía sau
for sx in (-0.15, 0.15):
    l.box("steel", (sx - 0.03, LD / 2, LZ0 + 0.004), (sx + 0.03, LD / 2 + 0.004, LZ0 + 0.05))
lid = l.build(PIVOT)

# ---------------- Binding: khóa gài + ổ khóa đồng ----------------
bd = S.part("Binding")
yL = -LD / 2
bd.box("steel", (-0.03, yL - 0.005, LZ0 - 0.005), (0.03, yL, LZ0 + 0.075))      # lưỡi gài trên nắp
bd.box("steel", (-0.03, yL - 0.008, LZ0 - 0.03), (0.03, yL - 0.005, LZ0 - 0.005))  # đầu lưỡi gài chồng lên móc
bd.cyl("brass", (-0.012, yL - 0.018, LZ0 + 0.04), (0.012, yL - 0.018, LZ0 + 0.04), 0.007, n=6)
# ổ khóa đồng treo vào móc: thân + quai
bd.box("brass", (-0.02, -D / 2 - 0.036, HB - 0.12), (0.02, -D / 2 - 0.022, HB - 0.07))
bd.beam("steel", (-0.011, -D / 2 - 0.03, HB - 0.07), (-0.011, -D / 2 - 0.03, HB - 0.052), 0.005, 0.005)
bd.beam("steel", (0.011, -D / 2 - 0.03, HB - 0.07), (0.011, -D / 2 - 0.03, HB - 0.052), 0.005, 0.005)
bd.beam("steel", (-0.011, -D / 2 - 0.03, HB - 0.052), (0.011, -D / 2 - 0.03, HB - 0.052), 0.005, 0.005)
binding = bd.build()

# ---------------- Seal: băng keo nâu dán qua khe nắp (phải) ----------------
sl = S.part("Seal")
sl.box("tape", (0.12, yL - 0.0035, LZ0 + 0.03), (0.17, yL - 0.0005, LZ0 + 0.07))
sl.poly("tape", [(0.12, yL - 0.0035, LZ0 + 0.03), (0.17, yL - 0.0035, LZ0 + 0.03), (0.17, -D / 2 - 0.0045, LZ0 - 0.01),
                 (0.12, -D / 2 - 0.0045, LZ0 - 0.01)], toward=(0, -1, 0))
sl.box("tape", (0.12, -D / 2 - 0.006, LZ0 - 0.06), (0.17, -D / 2 - 0.003, LZ0 - 0.01))
sl.box("red", (0.125, -D / 2 - 0.0075, LZ0 - 0.045), (0.165, -D / 2 - 0.006, LZ0 - 0.03))
seal = sl.build((0.145, yL, LZ0))

# ---------------- Glow ----------------
g = S.part("Glow")
zg = HB - 0.012
g.poly("glow", [(-0.18, -0.12, zg), (0.18, -0.12, zg), (0.18, 0.12, zg), (-0.18, 0.12, zg)], toward=(0, 0, 1))
glow = g.build((0, 0, zg))

L.export(S, "Chest_HopTon", [body, lid, glow, seal, binding], out=OUT)

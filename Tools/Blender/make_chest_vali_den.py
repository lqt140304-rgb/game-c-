"""Rương RD Rương Đen: vali da đen cũ, viền kim loại bóng, hai khóa số, thẻ hành lý "?" trông đáng ngờ. Dựng bằng bpy.

Chạy: python make_chest_vali_den.py [-- <file .glb ra>]   (mặc định Assets/_Project/Art/Chests/Chest_ValiDen.glb)
Cấu trúc: Chest_ValiDen -> Body, Lid (gốc ở bản lề sau, ở mép khe, xoay quanh X, mở bằng góc âm), Glow,
Seal (băng keo dán qua khe + thẻ hành lý), Binding (hai khóa số + lưỡi gài). Mặt trước -Y. ~0,52 x 0,36 x 0,32 m.
"""
import math

import chest_v1_lib as L

OUT = L.out_path("Chest_ValiDen.glb")

W, D, C = 0.50, 0.34, 0.022
HB, HL = 0.18, 0.105
WALL = 0.010
PIVOT = (0.0, D / 2, HB)

PALETTE = {
    "leather": {"c": (0.07, 0.07, 0.08), "r": 0.55}, "leather_dk": {"c": (0.04, 0.04, 0.05), "r": 0.6},
    "strap": {"c": (0.15, 0.10, 0.07), "r": 0.6}, "lining": {"c": (0.22, 0.05, 0.07), "r": 0.95},
    "chrome": {"c": (0.78, 0.78, 0.80), "r": 0.22, "m": 1.0}, "brass": {"c": (0.80, 0.62, 0.25), "r": 0.3, "m": 1.0},
    "ivory": {"c": (0.90, 0.88, 0.78), "r": 0.6}, "scuff": {"c": (0.20, 0.20, 0.21), "r": 0.8},
    "tape": {"c": (0.85, 0.72, 0.18), "r": 0.6}, "tape_dk": {"c": (0.12, 0.11, 0.08), "r": 0.7},
    "tag": {"c": (0.88, 0.84, 0.70), "r": 0.9}, "red": {"c": (0.75, 0.13, 0.11), "r": 0.6},
    "sticker_b": {"c": (0.28, 0.40, 0.58), "r": 0.8}, "sticker_w": {"c": (0.80, 0.80, 0.76), "r": 0.8},
    "glow": {"c": (1.0, 0.50, 0.30), "r": 0.5, "e": 2.0},
}
S = L.Scene(PALETTE)


def metal_rim(part, w, d, c, z0, z1, grow=0.004, mat="chrome"):
    a = L.ring_at(L.rrect(w + 2 * grow, d + 2 * grow, c + grow), z0)
    b = L.ring_at(L.rrect(w + 2 * grow, d + 2 * grow, c + grow), z1)
    part.ring_band(mat, a, b)


# ---------------- Body ----------------
b = S.part("Body")
L.shell(b, {"outer": "leather", "rim": "lining", "inner": "lining", "bottom": "leather_dk"}, W, D, C, 0.0, HB, WALL, floor_z=0.014)
metal_rim(b, W, D, C, HB - 0.016, HB + 0.0)          # viền bóng sát miệng thân
metal_rim(b, W, D, C, 0.0, 0.014)                    # viền bóng sát đáy
# bốn góc đáy bịt kim loại (chân vali) và bốn góc thân
for sx in (-1, 1):
    for sy in (-1, 1):
        cx, cy = sx * (W / 2 - 0.02), sy * (D / 2 - 0.02)
        b.box("chrome", (cx - 0.026, cy - 0.026, 0.0), (cx + 0.026, cy + 0.026, 0.03))
# đai da dọc thân (hai bên) và nẹp da quanh miệng
for sx in (-0.19, 0.19):
    b.box("strap", (sx - 0.02, -D / 2 - 0.003, 0.03), (sx + 0.02, -D / 2 + 0.0, HB - 0.02))
    b.box("strap", (sx - 0.02, D / 2 + 0.0, 0.03), (sx + 0.02, D / 2 + 0.003, HB - 0.02))
# tấm bịt khóa phía trước (đế khóa số), hai bên
for sx in (-0.15, 0.15):
    b.box("chrome", (sx - 0.04, -D / 2 - 0.007, HB - 0.07), (sx + 0.04, -D / 2 - 0.001, HB - 0.008))
# vết xước và mòn lớp da
for c, n, r, s in (((-0.20, -D / 2 - 0.0005, 0.05), (0, -1, 0), 0.04, 1), ((0.23, -0.05, 0.06), (1, 0, 0), 0.035, 2),
                   ((-0.1, -D / 2 - 0.0005, 0.03), (0, -1, 0), 0.025, 3)):
    L.patch(b, "scuff", c, n, r, n=6, seed=s, squash=0.5)
# bản lề: tay dưới + ống
for sx in (-0.16, 0.16):
    b.box("chrome", (sx - 0.035, D / 2 + 0.0, HB - 0.055), (sx + 0.035, D / 2 + 0.006, HB))
    b.cyl("chrome", (sx - 0.035, D / 2 + 0.008, HB), (sx + 0.035, D / 2 + 0.008, HB), 0.009, n=6)
body = b.build()

# ---------------- Lid ----------------
l = S.part("Lid")
Z1 = HB + HL
o0 = L.ring_at(L.rrect(W, D, C), HB)
o1 = L.ring_at(L.rrect(W, D, C), Z1 - 0.025)
o2 = L.ring_at(L.rrect(W - 0.06, D - 0.06, C), Z1)
l.ring_band("leather", o0, o1)
l.ring_band("leather", o1, o2)
l.poly("leather", o2, toward=(0, 0, 1))
i0 = L.ring_at(L.rrect(W - 2 * WALL, D - 2 * WALL, C - WALL), HB)
i1 = L.ring_at(L.rrect(W - 2 * WALL, D - 2 * WALL, C - WALL), Z1 - 0.03)
for i in range(8):
    j = (i + 1) % 8
    l.poly("lining", [o0[i], i0[i], i0[j], o0[j]], toward=(0, 0, -1))
    mid = (i0[i] + i0[j]) / 2
    l.poly("lining", [i0[i], i0[j], i1[j], i1[i]], toward=(-mid.x, -mid.y, 0))
l.poly("lining", i1[::-1], toward=(0, 0, -1))
metal_rim(l, W, D, C, HB, HB + 0.016)                # viền bóng sát khe, nửa nắp
metal_rim(l, W - 0.06, D - 0.06, C, Z1 - 0.01, Z1 + 0.0005, grow=0.003)  # viền quanh mặt nắp
# bốn góc trên nắp bịt kim loại
for sx in (-1, 1):
    for sy in (-1, 1):
        cx, cy = sx * (W / 2 - 0.02), sy * (D / 2 - 0.02)
        l.box("chrome", (cx - 0.026, cy - 0.026, HB + 0.02), (cx + 0.026, cy + 0.026, HB + 0.055))
# quai xách da trên mặt nắp: hai chân kim loại và vòm da
hz = Z1
for sx in (-0.08, 0.08):
    l.box("chrome", (sx - 0.014, -0.02, hz), (sx + 0.014, 0.02, hz + 0.012))
arc = [(-0.08, 0.0), (-0.078, 0.028), (-0.05, 0.043), (0.05, 0.043), (0.078, 0.028), (0.08, 0.0)]
for a, c in zip(arc[:-1], arc[1:]):
    l.beam("strap", (a[0], 0, hz + 0.012 + a[1]), (c[0], 0, hz + 0.012 + c[1]), 0.022, 0.012, up=(0, 1, 0))
# nhãn dán cũ ở góc nắp và vết xước
l.box("sticker_b", (0.13, -0.12, Z1 + 0.0005), (0.20, -0.07, Z1 + 0.0015))
l.box("sticker_w", (0.14, -0.11, Z1 + 0.0015), (0.19, -0.08, Z1 + 0.0022))
l.box("red", (0.155, -0.102, Z1 + 0.0022), (0.175, -0.088, Z1 + 0.0028))
L.patch(l, "scuff", (-0.15, -0.09, Z1), (0, 0, 1), 0.05, n=7, seed=5, squash=0.6)
L.patch(l, "scuff", (0.05, 0.1, Z1), (0, 0, 1), 0.03, n=6, seed=6, squash=0.6)
# tay bản lề trên
for sx in (-0.16, 0.16):
    l.box("chrome", (sx - 0.035, D / 2 + 0.0, HB), (sx + 0.035, D / 2 + 0.006, HB + 0.055))
lid = l.build(PIVOT)

# ---------------- Binding: hai khóa số + lưỡi gài ----------------
bd = S.part("Binding")
yF = -D / 2 - 0.007
for sx in (-0.15, 0.15):
    bd.box("chrome", (sx - 0.028, yF - 0.006, HB + 0.005), (sx + 0.028, yF, HB + 0.055))      # lưỡi gài trên nắp
    bd.box("brass", (sx - 0.026, yF - 0.016, HB - 0.058), (sx + 0.026, yF - 0.001, HB - 0.012))  # thân khóa số
    for k in (-1, 0, 1):                                                                        # ba bánh số
        cx = sx + k * 0.016
        bd.cyl("chrome", (cx - 0.006, yF - 0.012, HB - 0.035), (cx + 0.006, yF - 0.012, HB - 0.035), 0.014, n=8)
        bd.box("ivory", (cx - 0.004, yF - 0.0265, HB - 0.039), (cx + 0.004, yF - 0.0235, HB - 0.031))
    bd.cyl("brass", (sx, yF - 0.01, HB + 0.02), (sx, yF - 0.017, HB + 0.02), 0.006, n=6)
binding = bd.build()

# ---------------- Seal: băng keo vàng đen dán qua khe + thẻ hành lý "?" ----------------
sl = S.part("Seal")
yS = -D / 2 - 0.003
sl.box("tape", (-0.03, yS - 0.002, HB - 0.075), (0.03, yS, HB + 0.075))
for k in range(5):
    z = HB - 0.065 + k * 0.03
    sl.poly("tape_dk", [(-0.03, yS - 0.0025, z), (0.03, yS - 0.0025, z + 0.012), (0.03, yS - 0.0025, z + 0.02), (-0.03, yS - 0.0025, z + 0.008)],
            toward=(0, -1, 0))
# thẻ hành lý nằm trên mặt nắp, buộc dây vào chân quai
tz = Z1
sl.box("tag", (0.12, 0.0, tz), (0.18, 0.075, tz + 0.003))
sl.text("red", "?", 0.05, (0.15, 0.037, tz + 0.0045), facing="+z")
sl.beam("strap", (0.094, 0.0, tz + 0.006), (0.13, 0.02, tz + 0.004), 0.004, 0.004)
seal = sl.build((0.15, 0.037, tz))

# ---------------- Glow ----------------
g = S.part("Glow")
zg = HB - 0.012
g.poly("glow", [(-0.20, -0.12, zg), (0.20, -0.12, zg), (0.20, 0.12, zg), (-0.20, 0.12, zg)], toward=(0, 0, 1))
glow = g.build((0, 0, zg))

L.export(S, "Chest_ValiDen", [body, lid, glow, seal, binding], out=OUT)

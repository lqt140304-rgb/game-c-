"""Rương H1 Hộp Gỗ Nhỏ: thùng gỗ cũ đóng đinh, buộc dây nhựa đỏ xanh, độn rơm. Dựng bằng bpy, không texture.

Chạy: python make_chest_thung_go.py [-- <file .glb ra>]   (mặc định Assets/_Project/Art/Chests/Chest_ThungGo.glb)
Cấu trúc: Chest_ThungGo -> Body, Lid (gốc ở bản lề sau, xoay quanh X, mở bằng góc âm), Glow, Seal (băng giấy niêm), Binding (hai dây nhựa).
Mặt trước -Y. Kích thước ~0,50 x 0,34 x 0,31 m.
"""
import math
import random


import chest_v1_lib as L

OUT = L.out_path("Chest_ThungGo.glb")

W, D = 0.46, 0.34          # rộng (X), sâu (Y) của thân
T = 0.02                   # độ dày ván
Z0, ZB = 0.03, 0.245       # đáy thân (trên hai thanh trượt), miệng thân
LID_T, CLEAT = 0.025, 0.02
PIVOT = (0.0, D / 2, ZB)

PALETTE = {
    "wood_a": {"c": (0.55, 0.42, 0.28), "r": 0.9}, "wood_b": {"c": (0.47, 0.35, 0.23), "r": 0.9},
    "wood_c": {"c": (0.60, 0.48, 0.33), "r": 0.9}, "wood_dk": {"c": (0.34, 0.25, 0.17), "r": 0.9},
    "inside": {"c": (0.16, 0.11, 0.08), "r": 1.0}, "nail": {"c": (0.30, 0.30, 0.32), "r": 0.5, "m": 0.6},
    "straw": {"c": (0.78, 0.66, 0.34), "r": 0.95}, "ink": {"c": (0.12, 0.10, 0.09), "r": 0.9},
    "cord_red": {"c": (0.72, 0.16, 0.14), "r": 0.5}, "cord_blue": {"c": (0.16, 0.34, 0.66), "r": 0.5},
    "rope": {"c": (0.62, 0.52, 0.34), "r": 0.95}, "paper": {"c": (0.82, 0.76, 0.60), "r": 0.95},
    "stamp": {"c": (0.70, 0.18, 0.14), "r": 0.6}, "glow": {"c": (1.0, 0.78, 0.40), "r": 0.5, "e": 2.0},
}

S = L.Scene(PALETTE)
rng = random.Random(7)
WOODS = ["wood_a", "wood_b", "wood_c"]

# ---------------- Body ----------------
b = S.part("Body")
# hai thanh trượt dưới đáy
for sx in (-0.17, 0.17):
    b.box("wood_dk", (sx - 0.03, -D / 2 - 0.01, 0.0), (sx + 0.03, D / 2 + 0.01, Z0))
# sàn tối
b.box("inside", (-W / 2 + T, -D / 2 + T, Z0), (W / 2 - T, D / 2 - T, Z0 + 0.012))
# ván thân: 3 tầng, mỗi mặt một tầng ván lệch sắc
n_pl = 3
gap = 0.004
ph = (ZB - Z0 - gap * (n_pl - 1)) / n_pl
for k in range(n_pl):
    za = Z0 + k * (ph + gap)
    zb = za + ph + rng.uniform(-0.002, 0.002)
    for side in (-1, 1):  # trước / sau: ván dài suốt chiều rộng
        j = rng.uniform(-0.002, 0.002)
        y0 = side * (D / 2 - T) if side > 0 else -D / 2
        b.box(rng.choice(WOODS), (-W / 2, y0 + j, za), (W / 2, y0 + T + j, zb))
    for sx in (-1, 1):  # hai bên: ván kẹp giữa trước và sau
        x0 = sx * (W / 2 - T) if sx > 0 else -W / 2
        b.box(rng.choice(WOODS), (x0, -D / 2 + T, za), (x0 + T, D / 2 - T, zb))
# bốn nẹp góc đứng bên ngoài
for sx in (-1, 1):
    for sy in (-1, 1):
        cx, cy = sx * (W / 2 - 0.004), sy * (D / 2 - 0.004)
        b.box("wood_dk", (cx - 0.021, cy - 0.021, Z0), (cx + 0.021, cy + 0.021, ZB))
# đinh: hàng ba cái trên mỗi nẹp góc phía trước (mặt trước và mặt hông)
zs = [Z0 + 0.045, (Z0 + ZB) / 2, ZB - 0.045]
for sx in (-1, 1):
    cx, cy = sx * (W / 2 - 0.004), -(D / 2 - 0.004)
    for z in zs:
        b.box("nail", (cx - 0.006, cy - 0.021 - 0.003, z - 0.006), (cx + 0.006, cy - 0.021, z + 0.006))
        b.box("nail", (cx + sx * 0.021, cy - 0.006, z - 0.006), (cx + sx * 0.021 + sx * 0.003, cy + 0.006, z + 0.006))
# chữ "ĐỒ CŨ" in sơn đen trên ván giữa mặt trước, lệch phải
zmid = Z0 + ph + gap + ph / 2
b.text("ink", "ĐỒ CŨ", 0.036, (-0.15, -D / 2 - 0.0015, zmid), facing="-y")
# quai dây thừng hai bên: ô khoét tối và quai chữ U
for sx in (-1, 1):
    xo = sx * (W / 2)
    xs = xo + sx * 0.006
    pts = [(-0.07, zmid + 0.012), (-0.075, zmid - 0.03), (0.0, zmid - 0.045), (0.075, zmid - 0.03), (0.07, zmid + 0.012)]
    for a, c in zip(pts[:-1], pts[1:]):
        b.beam("rope", (xs, a[0], a[1]), (xs, c[0], c[1]), 0.012, 0.012, up=(sx, 0, 0))
    for yy in (-0.07, 0.07):
        b.box("wood_dk", (xo, yy - 0.012, zmid + 0.004), (xo + sx * 0.012, yy + 0.012, zmid + 0.02))
# rơm độn thò ra trong lòng thùng (nón ba cạnh nằm nghiêng)
for i in range(11):
    x, y = rng.uniform(-0.17, 0.17), rng.uniform(-0.11, 0.11)
    h = rng.uniform(0.05, 0.11)
    tip = (x + rng.uniform(-0.05, 0.05), y + rng.uniform(-0.05, 0.05), ZB - 0.02 + rng.uniform(-0.01, 0.02))
    b.cyl("straw", (x, y, Z0 + 0.012), tip, 0.007, n=3, r2=0.0014, caps=False)
body = b.build()

# ---------------- Lid ----------------
lid = S.part("Lid")
n_boards = 3
bw = (D + 0.02 - 0.004 * (n_boards - 1)) / n_boards
for k in range(n_boards):
    ya = -D / 2 - 0.01 + k * (bw + 0.004)
    lid.box(WOODS[k % 3], (-W / 2 - 0.01, ya, ZB), (W / 2 + 0.01, ya + bw, ZB + LID_T))
# hai thanh nẹp ngang trên nắp
for sx in (-0.16, 0.16):
    lid.box("wood_dk", (sx - 0.0225, -D / 2 - 0.0, ZB + LID_T), (sx + 0.0225, D / 2, ZB + LID_T + CLEAT))
    for yy in (-0.09, 0.09):
        lid.box("nail", (sx - 0.006, yy - 0.006, ZB + LID_T + CLEAT), (sx + 0.006, yy + 0.006, ZB + LID_T + CLEAT + 0.003))
# viền chốt dưới nắp để nắp nằm khít miệng thùng
lid.box("wood_dk", (-W / 2 + T + 0.004, -D / 2 + T + 0.004, ZB - 0.012), (W / 2 - T - 0.004, D / 2 - T - 0.004, ZB))
# hai bản lề dây da phía sau
for sx in (-0.15, 0.15):
    lid.box("wood_dk", (sx - 0.025, D / 2 - 0.004, ZB - 0.03), (sx + 0.025, D / 2 + 0.004, ZB + LID_T))
lid = lid.build(PIVOT)

# ---------------- Binding: hai vòng dây nhựa quanh thùng ----------------
bd = S.part("Binding")
CT = 0.011
yf, yb = -D / 2 - 0.012, D / 2 + 0.012
zt = ZB + LID_T + 0.006
zbot = Z0 - 0.012
for x, m in ((-0.075, "cord_red"), (0.075, "cord_blue")):
    x0, x1 = x - 0.008, x + 0.008
    bd.box(m, (x0, yf - CT * 0.0, zbot), (x1, yf + CT, zt + CT))            # cạnh trước
    bd.box(m, (x0, yb - CT, zbot), (x1, yb, zt + CT))                         # cạnh sau
    bd.box(m, (x0, yf, zt), (x1, yb, zt + CT))                                # trên nắp
    bd.box(m, (x0, yf, zbot), (x1, yb, zbot + CT))                            # dưới đáy
# nút thắt phía trước, hai đuôi dây rủ
bd.box("cord_red", (-0.095, yf - 0.014, ZB - 0.02), (-0.055, yf, ZB + 0.02))
bd.beam("cord_red", (-0.08, yf - 0.012, ZB - 0.02), (-0.10, yf - 0.03, ZB - 0.09), 0.008, 0.005, up=(0, 1, 0))
bd.beam("cord_red", (-0.07, yf - 0.012, ZB - 0.02), (-0.045, yf - 0.025, ZB - 0.075), 0.008, 0.005, up=(0, 1, 0))
bd.box("cord_blue", (0.055, yf - 0.014, ZB + 0.01), (0.095, yf, ZB + 0.05))
bd.beam("cord_blue", (0.07, yf - 0.012, ZB + 0.01), (0.09, yf - 0.03, ZB - 0.06), 0.008, 0.005, up=(0, 1, 0))
binding = bd.build()

# ---------------- Seal: băng giấy niêm có dấu đỏ ----------------
sl = S.part("Seal")
yS = -D / 2 - 0.003
sl.box("paper", (0.135, yS - 0.002, ZB - 0.07), (0.20, yS, ZB + 0.06))
sl.cyl("stamp", (0.1675, yS - 0.002, ZB - 0.005), (0.1675, yS - 0.006, ZB - 0.005), 0.022, n=8)
sl.box("ink", (0.15, yS - 0.0035, ZB + 0.035), (0.185, yS - 0.0025, ZB + 0.040))
sl.box("ink", (0.15, yS - 0.0035, ZB - 0.052), (0.185, yS - 0.0025, ZB - 0.047))
seal = sl.build((0.1675, yS, ZB))

# ---------------- Glow: quầng sáng ở miệng thùng ----------------
g = S.part("Glow")
zg = ZB - 0.012
g.poly("glow", [(-0.17, -0.12, zg), (0.17, -0.12, zg), (0.17, 0.12, zg), (-0.17, 0.12, zg)], toward=(0, 0, 1))
glow = g.build((0, 0, zg))

L.export(S, "Chest_ThungGo", [body, lid, glow, seal, binding], out=OUT)

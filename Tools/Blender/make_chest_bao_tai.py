"""Rương H3 Hộp Vải Dày: bao tải vải bố buộc dây rút, miệng túi là nắp mở được, thêu bùa, dán bùa giấy vàng. Dựng bằng bpy.

Chạy: python make_chest_bao_tai.py [-- <file .glb ra>]   (mặc định Assets/_Project/Art/Chests/Chest_BaoTai.glb)
Cấu trúc: Chest_BaoTai -> Body (thân túi, miếng vá, bùa thêu), Lid (miệng túi xếp nếp, gốc ở bản lề sau chân nắp, xoay quanh X, mở bằng góc âm),
Glow, Seal (lá bùa giấy vàng dán qua khe), Binding (dây rút đỏ có nút và hai đuôi). Mặt trước -Y. ~0,42 x 0,37 x 0,38 m.
"""
import math
import random

import chest_v1_lib as L
from mathutils import Vector

OUT = L.out_path("Chest_BaoTai.glb")
N = 12
SC = (1.0, 0.88)           # túi hơi dẹt theo chiều sâu
rng = random.Random(11)
FOLD = [rng.uniform(-0.02, 0.02) for _ in range(N)]

BODY_PROF = [(0.12, 0.0), (0.18, 0.025), (0.205, 0.10), (0.21, 0.17), (0.195, 0.24), (0.15, 0.285), (0.125, 0.305)]
SKIRT_PROF = [(0.166, 0.285), (0.158, 0.30), (0.145, 0.322), (0.12, 0.345), (0.07, 0.368)]
APEX_Z = 0.382
Z_SEAM = 0.285
PIVOT = (0.0, 0.166 * SC[1], Z_SEAM)

PALETTE = {
    "sack": {"c": (0.64, 0.52, 0.34), "r": 1.0}, "sack_dk": {"c": (0.50, 0.40, 0.26), "r": 1.0},
    "inside": {"c": (0.14, 0.10, 0.08), "r": 1.0}, "thread": {"c": (0.30, 0.22, 0.14), "r": 1.0},
    "patch_blue": {"c": (0.34, 0.42, 0.50), "r": 0.95}, "patch_brn": {"c": (0.42, 0.30, 0.22), "r": 0.95},
    "cloth_red": {"c": (0.62, 0.15, 0.14), "r": 0.9}, "gold": {"c": (0.88, 0.70, 0.28), "r": 0.6},
    "cord": {"c": (0.55, 0.10, 0.10), "r": 0.8}, "paper": {"c": (0.90, 0.78, 0.38), "r": 0.95},
    "ink_red": {"c": (0.68, 0.12, 0.10), "r": 0.7}, "glow": {"c": (1.0, 0.82, 0.50), "r": 0.5, "e": 2.0},
}
S = L.Scene(PALETTE)


def interp(prof, z):
    for (r0, z0), (r1, z1) in zip(prof[:-1], prof[1:]):
        if z0 <= z <= z1:
            return r0 + (r1 - r0) * (z - z0) / (z1 - z0)
    return None


def r_closed(z):
    """Bán kính ngoài của cả túi (thân hoặc váy nắp, lấy lớn hơn) khi nắp đóng."""
    a = interp(BODY_PROF, z)
    b = interp(SKIRT_PROF, z)
    return max(a or 0.0, b or 0.0)


def pt(theta, z, off):
    """Điểm trên mặt túi: theta = -90° là mặt trước."""
    r = r_closed(z) + off
    return Vector((r * SC[0] * math.cos(theta), r * SC[1] * math.sin(theta), z))


def surf_patch(part, mat, th0, z0, w, h, off, nu=2, nv=3):
    """Hình chữ nhật cong theo mặt túi: tâm (th0, z0), rộng w (m cung) cao h."""
    r0 = r_closed(z0) * SC[0]
    for iu in range(nu):
        for iv in range(nv):
            def p(a, b):
                return pt(th0 + (-w / 2 + w * a / nu) / r0, z0 - h / 2 + h * b / nv, off)
            part.poly(mat, [p(iu, iv), p(iu + 1, iv), p(iu + 1, iv + 1), p(iu, iv + 1)], away=(0, 0, z0))


def surf_ring(part, mat, th0, z0, R, off, n=8, rot=0.0):
    """Đĩa/đa giác đều bán kính R dán lên mặt túi (quạt từ tâm)."""
    r0 = r_closed(z0) * SC[0]
    ring = [pt(th0 + R * math.cos(rot + 2 * math.pi * i / n) / r0, z0 + R * math.sin(rot + 2 * math.pi * i / n), off) for i in range(n)]
    c = pt(th0, z0, off)
    for i in range(n):
        part.poly(mat, [c, ring[i], ring[(i + 1) % n]], away=(0, 0, z0))


def surf_annulus(part, mat, th0, z0, R0, R1, off, n=8):
    r0 = r_closed(z0) * SC[0]

    def rg(R):
        return [pt(th0 + R * math.cos(2 * math.pi * i / n) / r0, z0 + R * math.sin(2 * math.pi * i / n), off) for i in range(n)]
    a, b = rg(R0), rg(R1)
    for i in range(n):
        j = (i + 1) % n
        part.poly(mat, [a[i], a[j], b[j], b[i]], away=(0, 0, z0))


FRONT = -math.pi / 2
# ---------------- Body ----------------
b = S.part("Body")
jit = lambda i, z: FOLD[i]
rings = b.lathe("sack", BODY_PROF, N, scale=SC, jitter=jit)
# xen kẽ hai sắc để thấy nếp gấp
top = rings[-1]
inner_top = [Vector((v.x * 0.93, v.y * 0.93, v.z)) for v in top]
inner_dn = [Vector((v.x * 0.9, v.y * 0.9, 0.2)) for v in top]
for i in range(N):
    j = (i + 1) % N
    b.poly("sack_dk", [top[i], top[j], inner_top[j], inner_top[i]], toward=(0, 0, 1))
    mid = (inner_top[i] + inner_top[j]) / 2
    b.poly("inside", [inner_top[i], inner_top[j], inner_dn[j], inner_dn[i]], toward=(-mid.x, -mid.y, 0))
b.poly("inside", inner_dn, toward=(0, 0, 1))
b.poly("sack_dk", rings[0][::-1], toward=(0, 0, -1))
# đường may dọc thân (hai bên) và đường chỉ quanh đáy
for th in (FRONT + math.radians(62), FRONT - math.radians(62) + math.radians(180) * 0):
    surf_patch(b, "thread", th, 0.15, 0.012, 0.19, 0.004, nu=1, nv=4)
# miếng vá xanh và nâu, viền chỉ sẫm
surf_patch(b, "thread", FRONT - math.radians(52), 0.10, 0.100, 0.092, 0.006, 1, 2)
surf_patch(b, "patch_blue", FRONT - math.radians(52), 0.10, 0.086, 0.078, 0.0085, 1, 2)
surf_patch(b, "thread", FRONT + math.radians(38), 0.065, 0.084, 0.070, 0.006, 1, 2)
surf_patch(b, "patch_brn", FRONT + math.radians(38), 0.065, 0.070, 0.056, 0.0085, 1, 2)
# bùa thêu chính giữa mặt trước: đĩa vải đỏ, vòng vàng, tâm vàng, tám vạch bát quái
ZE = 0.165
surf_ring(b, "cloth_red", FRONT, ZE, 0.078, 0.007, n=8, rot=math.radians(22.5))
surf_annulus(b, "gold", FRONT, ZE, 0.050, 0.060, 0.0085, n=8)
surf_ring(b, "gold", FRONT, ZE, 0.018, 0.0085, n=8)
for k in range(8):
    a = k * math.pi / 4
    r0 = r_closed(ZE) * SC[0]
    c0 = (0.028, 0.044)
    p = lambda rr, da: pt(FRONT + (rr * math.cos(a) + da * -math.sin(a)) / r0, ZE + rr * math.sin(a) + da * math.cos(a), 0.0085)
    b.poly("gold", [p(c0[0], -0.004), p(c0[1], -0.004), p(c0[1], 0.004), p(c0[0], 0.004)], away=(0, 0, ZE))
body = b.build()

# ---------------- Lid: miệng túi xếp nếp ----------------
l = S.part("Lid")
SK_FOLD = [(0.012 if i % 2 == 0 else -0.012) for i in range(N)]
rings = l.lathe("sack", SKIRT_PROF, N, scale=SC, jitter=lambda i, z: SK_FOLD[i] * (1.4 if z < 0.31 else 0.8))
ap = Vector((0, 0, APEX_Z))
for i in range(N):
    j = (i + 1) % N
    l.poly("sack_dk" if i % 2 else "sack", [rings[-1][i], rings[-1][j], ap], away=(0, 0, 0.30))
# mép dưới váy nắp và lòng nắp tối
bot = rings[0]
bot_in = [Vector((v.x * 0.9, v.y * 0.9, v.z + 0.004)) for v in bot]
for i in range(N):
    j = (i + 1) % N
    l.poly("sack_dk", [bot[i], bot_in[i], bot_in[j], bot[j]], toward=(0, 0, -1))
    l.poly("inside", [bot_in[i], bot_in[j], Vector((0, 0, 0.32))], toward=(0, 0, -1))
# chóp nút buộc và hai tai vải
l.cyl("sack_dk", (0, 0, APEX_Z - 0.004), (0, 0, APEX_Z + 0.028), 0.014, n=5, r2=0.005)
for sx in (-1, 1):
    l.poly("sack", [(sx * 0.012, 0, APEX_Z + 0.012), (sx * 0.05, -0.006, APEX_Z + 0.03), (sx * 0.034, 0.012, APEX_Z + 0.004)], toward=(0, -1, 0))
    l.poly("sack_dk", [(sx * 0.012, 0, APEX_Z + 0.012), (sx * 0.034, 0.012, APEX_Z + 0.004), (sx * 0.05, -0.006, APEX_Z + 0.03)], toward=(0, 1, 0))
lid = l.build(PIVOT)

# ---------------- Binding: dây rút đỏ quanh cổ túi, nút và hai đuôi ----------------
bd = S.part("Binding")
ZR = 0.308
R_CORD = interp(SKIRT_PROF, ZR) + 0.0045
n_c = N
cr = [Vector((R_CORD * SC[0] * math.cos(2 * math.pi * i / n_c), R_CORD * SC[1] * math.sin(2 * math.pi * i / n_c), ZR)) for i in range(n_c)]
for i in range(n_c):
    j = (i + 1) % n_c
    bd.beam("cord", cr[i], cr[j], 0.011, 0.011, up=(0, 0, 1))
fp = Vector((0.0, -R_CORD * SC[1] - 0.004, ZR))
bd.cyl("cord", fp + Vector((0, 0.002, 0)), fp + Vector((0, -0.016, 0)), 0.014, n=6)
bd.beam("cord", fp + Vector((-0.005, -0.012, 0)), fp + Vector((-0.045, -0.035, -0.07)), 0.009, 0.006, up=(0, -1, 0))
bd.beam("cord", fp + Vector((0.005, -0.012, 0)), fp + Vector((0.03, -0.03, -0.085)), 0.009, 0.006, up=(0, -1, 0))
bd.cyl("gold", fp + Vector((-0.045, -0.035, -0.07)), fp + Vector((-0.049, -0.037, -0.086)), 0.008, n=5)
bd.cyl("gold", fp + Vector((0.03, -0.03, -0.085)), fp + Vector((0.033, -0.032, -0.10)), 0.008, n=5)
binding = bd.build()

# ---------------- Seal: lá bùa giấy vàng dán qua khe nắp, bên phải ----------------
sl = S.part("Seal")
TH = FRONT + math.radians(32)
surf_patch(sl, "paper", TH, 0.285, 0.06, 0.15, 0.0075, nu=1, nv=6)
for z, wd in ((0.335, 0.036), (0.305, 0.03), (0.275, 0.036), (0.25, 0.026)):
    surf_patch(sl, "ink_red", TH, z, wd, 0.008, 0.0087, nu=1, nv=1)
surf_ring(sl, "ink_red", TH, 0.225, 0.011, 0.0087, n=6)
seal = sl.build((pt(TH, 0.285, 0.0075).x, pt(TH, 0.285, 0.0075).y, 0.285))

# ---------------- Glow ----------------
g = S.part("Glow")
zg = 0.285
ring_g = [Vector((0.105 * SC[0] * math.cos(2 * math.pi * i / 8), 0.105 * SC[1] * math.sin(2 * math.pi * i / 8), zg)) for i in range(8)]
g.fan("glow", (0, 0, zg), ring_g, toward=(0, 0, 1))
glow = g.build((0, 0, zg))

L.export(S, "Chest_BaoTai", [body, lid, glow, seal, binding], out=OUT)

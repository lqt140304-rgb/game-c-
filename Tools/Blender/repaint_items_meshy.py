"""To lai cac vung texture Meshy ve sai so voi anh tham chieu (Docs/Art/KiemTra3D.md, muc "Loang mau").

Chay SAU rebake_items_meshy.py, moi file mot lan (ghi de):
    python3 Tools/Blender/repaint_items_meshy.py <file.glb>
Chi tac dong len texture; hinh hoc, UV, phap tuyen giu nguyen.

Moi mon co danh sach buoc trong RULES. Moi buoc chon texel theo:
  faces : ham tren dac trung mat (F.nx/ny/nz phap tuyen the gioi, F.h do cao 0..1 tu day,
          F.r ban kinh ngang 0..1 tinh tu truc giua, F.rad = phap tuyen . huong ra ngoai tu truc)
  texels: ham (lab, pos) tren tung texel da chon (mau Lab va vi tri 3D)
va mot hanh dong:
  tint   : dua ve mau dich ("auto" = mau chu dao cua mon), giu `keep` phan bien thien do sang;
  stripes: soc ngang deu theo do cao (`count` soc, ti le `duty` mau thu hai);
  bands  : dung lai cac vong mau theo do cao (vd. vong xanh chan cup) tu ti le texel `is_band`;
  snap   : moi texel lay mau bang mau gan nhat (Lab, sau lam mo 5x5), giu `keep` do sang;
  ticket : ve lai noi dung ve so (L04) chieu thang len mat truoc;
  sequins: phu kim sa lap lanh (L19) theo luoi luc giac trong khong gian UV.
Toa do Blender: Z len, mat truoc nhin tu -Y.
"""
import bpy, sys, os, tempfile
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from color_bleed_lib import mesh_arrays, image_array, srgb_to_lab, lab_to_srgb, dilate, uv_mask

GOLD = (214, 170, 62)


def rgb(c):
    return np.array(c, np.float32) / 255


def lab_of(c):
    return srgb_to_lab(rgb(c)[None])[0]


def hue(lab):
    return np.degrees(np.arctan2(lab[:, 2], lab[:, 1])) % 360


def chroma(lab):
    return np.hypot(lab[:, 1], lab[:, 2])


RULES = {
    # an ngoc ty (ref_huyen_thoai.png): rong vang tron; day de da co mang nau -> da xam
    "L01_an_ngoc_ty": [
        dict(faces=lambda F: F.h > 0.6, texels=lambda lab, p: ~((hue(lab) > 120) & (hue(lab) < 220) & (chroma(lab) > 12)),
             act="tint", color=GOLD, keep=0.45),
        dict(faces=lambda F: F.h < 0.45, texels=lambda lab, p: (hue(lab) > 20) & (hue(lab) < 90) & (chroma(lab) > 10),
             act="tint", color=(158, 150, 140), keep=0.5),
        dict(faces=lambda F: F.nz < -0.7, act="tint", color=(150, 143, 134), keep=0.25),
    ],
    # qua tao vang: phan tao + la vang deu (vet hong tren dinh)
    "L02_qua_tao_vang": [
        dict(faces=lambda F: F.h > 0.33, act="tint", color=GOLD, keep=0.25),
    ],
    # ve so: ve lai noi dung mat truoc theo ref (sao, bang ron, day so, ma vach); mat sau giay tron
    "L04_ve_so_doc_dac": [
        dict(faces=lambda F: F.ny > -0.3, act="tint", color=(226, 206, 160), keep=0.0),
        dict(faces=lambda F: F.ny <= -0.3, act="ticket"),
    ],
    # so tu than: vet vang o mep -> giay trang (sang) hoac bia den (toi)
    "L08_so_tu_than": [
        dict(texels=lambda lab, p: (hue(lab) > 50) & (hue(lab) < 115) & (chroma(lab) > 14) & (lab[:, 0] > 55),
             act="tint", color=(224, 221, 210), keep=0.2),
        dict(texels=lambda lab, p: (hue(lab) > 50) & (hue(lab) < 115) & (chroma(lab) > 14) & (lab[:, 0] <= 55),
             act="tint", color=(26, 26, 29), keep=0.2),
    ],
    # cup vang: toan bo vang; chan de: hai vong xanh la dung lai theo do cao (gon, khong rang cua)
    "L12_cup_vang_the_gioi": [
        dict(faces=lambda F: F.h < 0.2, act="bands", color=(40, 125, 62), other=GOLD,
             is_band=lambda lab: (hue(lab) > 110) & (hue(lab) < 210) & (chroma(lab) > 10)),
        dict(faces=lambda F: F.h >= 0.2, act="tint", color=GOLD, keep=0.3),
    ],
    # mat na pharaoh: khan trum dau (tru mat) ve lai soc ngang vang / xanh lam deu quanh dau nhu ref
    "L13_mat_na_pharaoh": [
        dict(faces=lambda F: (F.h > 0.42) & ~((F.ny < -0.3) & (np.abs(F.x) < 0.3) & (F.h < 0.85))
                             & ~((F.ny < -0.2) & (np.abs(F.x) < 0.14)),
             act="stripes", colors=[GOLD, (36, 58, 142)], count=15, duty=0.45, keep=0.0),
    ],
    # nhan: vong trong cung mau nhan, phang (mang den o mat trong va mep)
    "L16_nhan_duy_nhat": [
        dict(faces=lambda F: F.rad < -0.2, act="tint", color="auto", keep=0.0),
        dict(texels=lambda lab, p: lab[:, 0] < 60, act="tint", color="auto", keep=0.0),
    ],
    # mu den: den bong, xam toi, kinh do sam, chi tiet bac
    "L18_mat_na_chua_te_bong_toi": [
        dict(act="snap", keep=0.35, palette=[(20, 20, 23), (62, 62, 68), (96, 24, 24), (158, 158, 164)]),
    ],
    # gang tay lap lanh: dinh kim sa bac
    "L19_gang_tay_lap_lanh": [dict(act="sequins")],
    # bang video (nam ngang): vong canh xung quanh den, giu nhan trang/cua so; vet hong/nau -> den
    "L20_bang_video_7_ngay": [
        dict(faces=lambda F: np.abs(F.nz) < 0.6, texels=lambda lab, p: (chroma(lab) > 6) | (lab[:, 0] < 40),
             act="tint", color=(24, 24, 27), keep=0.15),
        dict(texels=lambda lab, p: (chroma(lab) > 10) & (hue(lab) < 100) & (lab[:, 0] > 25),
             act="tint", color=(24, 24, 27), keep=0.15),
    ],
    # ban do: bo mang trang, giay da cu deu mau, giu net muc
    "L22_ban_do_co": [
        dict(texels=lambda lab, p: lab[:, 0] > 45, act="tint", color=(204, 176, 128), keep=0.3),
    ],
    # qua cau ngoc (ref_rat_hiem.png): cam trong deu, 6 sao do chi o mat truoc, de go nau.
    # Meshy ve san mang cam sam loang (bong khuc xa), manh sao lac o dinh/mat sau, dia xam trong de.
    "X04_qua_cau_ngoc": [
        dict(faces=lambda F: F.h > 0.3,
             texels=lambda lab, p: ~((chroma(lab) > 35) & ((hue(lab) < 45) | (hue(lab) > 340)) & (lab[:, 1] > 35)),
             act="tint", color=(240, 146, 34), keep=0.15),
        dict(faces=lambda F: (F.h > 0.3) & (F.ny > -0.35),
             texels=lambda lab, p: (chroma(lab) > 35) & (lab[:, 1] > 35),
             act="tint", color=(240, 146, 34), keep=0.15),
        dict(faces=lambda F: F.h <= 0.3,
             texels=lambda lab, p: (chroma(lab) > 38) & (hue(lab) > 40) & (hue(lab) < 90),
             act="tint", color=(240, 146, 34), keep=0.15),
        dict(faces=lambda F: F.h <= 0.3,
             texels=lambda lab, p: (chroma(lab) < 16) | ((hue(lab) > 110) & (hue(lab) < 280)),
             act="tint", color=(112, 76, 50), keep=0.3),
        dict(faces=lambda F: (F.h <= 0.3) & (F.r < 0.35), act="tint", color=(112, 76, 50), keep=0.3),  # dia giua de
    ],
}


class Faces:
    def __init__(self, P, T):
        A, B, C = P[T[:, 0]], P[T[:, 1]], P[T[:, 2]]
        n = np.cross(B - A, C - A); n /= np.linalg.norm(n, axis=1, keepdims=True) + 1e-12
        c = (A + B + C) / 3
        mn, mx = P.min(0), P.max(0)
        self.nx, self.ny, self.nz = n.T
        self.h = (c[:, 2] - mn[2]) / max(mx[2] - mn[2], 1e-9)
        self.x = (c[:, 0] - (mn[0] + mx[0]) / 2) / max((mx[0] - mn[0]) / 2, 1e-9)
        ctr = (mn + mx) / 2
        d = c[:, :2] - ctr[:2]; r = np.linalg.norm(d, axis=1)
        self.r = r / max(r.max(), 1e-9)
        self.rad = (n[:, 0] * d[:, 0] + n[:, 1] * d[:, 1]) / np.maximum(r, 1e-9)


def texel_map(P, T, UV, h, w):
    """fid (h,w) chi so mat cua moi texel (-1 neu ngoai), pos (h,w,3) vi tri 3D.
    Luot 1 nhan ca texel cat bien (cach canh <= 0,7 texel), luot 2 ghi de bang phan trong chinh xac."""
    fid = -np.ones((h, w), np.int64); pos = np.zeros((h, w, 3), np.float32)
    px = UV * np.array([w, h]) - 0.5
    for exact in (False, True):
        for f, t in enumerate(px):
            x0, y0 = np.floor(t.min(0) - 1).astype(int); x1, y1 = np.ceil(t.max(0) + 1).astype(int)
            x0, y0 = max(x0, 0), max(y0, 0); x1, y1 = min(x1, w - 1), min(y1, h - 1)
            if x1 < x0 or y1 < y0:
                continue
            gx, gy = np.meshgrid(np.arange(x0, x1 + 1), np.arange(y0, y1 + 1))
            a, b, c = t
            d = (b[1] - c[1]) * (a[0] - c[0]) + (c[0] - b[0]) * (a[1] - c[1])
            if abs(d) < 1e-9:
                continue
            l1 = ((b[1] - c[1]) * (gx - c[0]) + (c[0] - b[0]) * (gy - c[1])) / d
            l2 = ((c[1] - a[1]) * (gx - c[0]) + (a[0] - c[0]) * (gy - c[1])) / d
            l3 = 1 - l1 - l2
            if exact:
                e1 = e2 = e3 = 0.0
            else:  # l_i = khoang cach toi canh doi dien / chieu cao -> 0,7 texel
                e1 = -0.7 * np.linalg.norm(b - c) / abs(d)
                e2 = -0.7 * np.linalg.norm(c - a) / abs(d)
                e3 = -0.7 * np.linalg.norm(a - b) / abs(d)
            m = (l1 >= e1) & (l2 >= e2) & (l3 >= e3)
            if not exact:
                m &= fid[y0:y1 + 1, x0:x1 + 1] < 0
            if not m.any():
                continue
            ys, xs = gy[m], gx[m]
            fid[ys, xs] = f
            Q = P[T[f]]
            k1, k2, k3 = (np.clip(l, 0, 1)[m] for l in (l1, l2, l3))
            ks = np.maximum(k1 + k2 + k3, 1e-9)
            pos[ys, xs] = ((k1 / ks)[:, None] * Q[0] + (k2 / ks)[:, None] * Q[1] + (k3 / ks)[:, None] * Q[2])
    return fid, pos


def tint(lab, color, keep):
    t = lab_of(color)
    out = np.tile(t, (len(lab), 1))
    out[:, 0] += keep * (lab[:, 0] - np.median(lab[:, 0]))
    return out


def snap(lab_img, sel, palette, keep):
    """lab_img (h,w,3), sel (h,w) bool."""
    k = np.pad(lab_img, ((2, 2), (2, 2), (0, 0)), mode="edge")
    rr = range(-2, 3)
    blur = sum(k[2 + dy:k.shape[0] - 2 + dy, 2 + dx:k.shape[1] - 2 + dx] for dy in rr for dx in rr) / 25
    pal = np.array([lab_of(c) for c in palette])
    b = blur[sel]
    d = ((b[:, None, :] - pal[None]) * np.array([0.6, 1, 1])) ** 2
    idx = d.sum(2).argmin(1)
    out = pal[idx].copy()
    out[:, 0] += keep * (lab_img[sel][:, 0] - b[:, 0])
    return out


def arc_param(a, y, at, nb=40):
    """Tham so 0..1 theo do dai cung cua mat cong y(a) (trung vi theo khoang a), tai cac gia tri `at`."""
    zs = np.linspace(a.min(), a.max(), nb + 1)
    yb = np.array([np.median(y[(a >= lo) & (a <= hi)]) if ((a >= lo) & (a <= hi)).any() else np.nan
                   for lo, hi in zip(zs[:-1], zs[1:])])
    zc = (zs[:-1] + zs[1:]) / 2; ok = ~np.isnan(yb); yb = np.interp(zc, zc[ok], yb[ok])
    yb = np.convolve(np.pad(yb, 2, mode="edge"), np.ones(5) / 5, mode="valid")
    arc = np.concatenate([[0], np.cumsum(np.hypot(np.diff(zc), np.diff(yb)))])
    return np.interp(at, zc, arc) / max(arc[-1], 1e-9)


def ticket_design(W=600, H=900):
    from PIL import Image, ImageDraw, ImageFont
    fdir = "/usr/share/fonts/truetype/dejavu/"
    def font(n, s):
        try:
            return ImageFont.truetype(fdir + n, s)
        except OSError:
            return ImageFont.load_default()
    paper, ink, gold, gold_d = (228, 208, 162), (58, 46, 34), (206, 160, 52), (160, 118, 36)
    im = Image.new("RGB", (W, H), paper); d = ImageDraw.Draw(im)
    rng = np.random.default_rng(4)
    arr = np.asarray(im).astype(np.float32)
    yy, xx = np.mgrid[0:H, 0:W]
    vign = 1 - 0.10 * (((xx - W / 2) / (W / 2)) ** 2 + ((yy - H / 2) / (H / 2)) ** 2)
    arr *= vign[..., None]; arr += rng.normal(0, 2.5, arr.shape)
    im = Image.fromarray(np.clip(arr, 0, 255).astype(np.uint8)); d = ImageDraw.Draw(im)
    m = 34
    d.rectangle([m, m, W - m, H - m], outline=gold_d, width=6)
    d.rectangle([m + 14, m + 14, W - m - 14, H - m - 14], outline=gold, width=3)
    cx, cy, R = W // 2, int(H * 0.27), int(W * 0.25)
    d.ellipse([cx - R, cy - R, cx + R, cy + R], outline=gold_d, width=12)
    import math
    star = [(cx + (R * 0.85 if i % 2 == 0 else R * 0.36) * math.sin(math.pi * i / 5),
             cy - (R * 0.85 if i % 2 == 0 else R * 0.36) * math.cos(math.pi * i / 5)) for i in range(10)]
    d.polygon(star, fill=gold, outline=gold_d)
    by = int(H * 0.52)
    d.polygon([(60, by), (W - 60, by), (W - 90, by + 40), (W - 60, by + 80), (60, by + 80), (90, by + 40)], fill=gold, outline=gold_d)
    d.rectangle([110, by + 14, W - 110, by + 66], fill=(232, 196, 84))
    f1 = font("DejaVuSansMono-Bold.ttf", 52); f2 = font("DejaVuSansMono-Bold.ttf", 44)
    d.text((cx, int(H * 0.68)), "16 24 33 49 56", font=f1, fill=ink, anchor="mm")
    d.text((cx, int(H * 0.76)), "00 888 007 ++", font=f2, fill=ink, anchor="mm")
    x = 90; y0 = int(H * 0.82); rng2 = np.random.default_rng(7)
    while x < W - 90:
        wbar = int(rng2.integers(3, 12)); d.rectangle([x, y0, x + wbar, y0 + 70], fill=ink); x += wbar + int(rng2.integers(4, 10))
    return np.asarray(im).astype(np.float32) / 255


def sequins(UV_sel_xy, seed=3):
    """Mau kim sa cho cac texel (toa do texel x,y). Luoi luc giac buoc 9 texel."""
    s = 9.0
    x, y = UV_sel_xy[:, 0], UV_sel_xy[:, 1]
    row = np.round(y / (s * 0.866))
    xs = x - (row % 2) * s / 2
    col = np.round(xs / s)
    cxp = col * s + (row % 2) * s / 2; cyp = row * s * 0.866
    dist = np.hypot(x - cxp, y - cyp)
    h = (np.sin(col * 12.9898 + row * 78.233 + seed) * 43758.5453) % 1.0
    base = np.where(h[:, None] > 0.88, rgb((255, 255, 255)), rgb((212, 215, 224)) * (0.8 + 0.3 * h[:, None]))
    base = np.where(((h > 0.55) & (h < 0.62))[:, None], rgb((205, 220, 240)), base)
    disc = np.clip((4.3 - dist) / 0.9, 0, 1)[:, None]
    spec = np.clip(1 - np.hypot(x - cxp + 1.5, y - cyp + 1.5) / 1.8, 0, 1)[:, None]
    gap = rgb((150, 152, 160))
    return np.clip(gap * (1 - disc) + base * disc + 0.5 * spec * disc, 0, 1)


def process(path):
    name = os.path.splitext(os.path.basename(path))[0]
    steps = RULES[name]
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=path)
    o = [o for o in bpy.context.scene.objects if o.type == "MESH"][0]
    node = next(n for n in o.active_material.node_tree.nodes if n.type == "TEX_IMAGE")
    tex = image_array(node.image); h, w = tex.shape[:2]
    P, T, UV = mesh_arrays(o)
    F = Faces(P, T)
    fid, pos = texel_map(P, T, UV, h, w)
    inside = fid >= 0
    lab_img = srgb_to_lab(tex)
    changed = np.zeros((h, w), bool)
    for st in steps:
        fsel = st["faces"](F) if "faces" in st else np.ones(len(T), bool)
        sel = inside & fsel[np.maximum(fid, 0)]
        if "texels" in st:
            tsel = st["texels"](lab_img[sel], pos[sel])
            m = np.zeros(sel.sum(), bool); m[:] = tsel; sel2 = np.zeros_like(sel); sel2[sel] = m; sel = sel2
        if not sel.any():
            continue
        if st["act"] == "tint":
            col = st["color"]
            if col == "auto":  # mau chu dao cua mon (trung vi texel ngoai vung chon)
                rest = inside & ~sel
                col = tuple(np.clip(lab_to_srgb(np.median(lab_img[rest], 0)[None])[0] * 255, 0, 255))
            lab_img[sel] = tint(lab_img[sel], col, st["keep"])
        elif st["act"] == "stripes":
            z = pos[sel][:, 2]; z0, z1 = z.min(), z.max()
            ph = ((z - z0) / max(z1 - z0, 1e-9) * st["count"]) % 1.0
            idx = (ph < st["duty"]).astype(int)
            pal = np.array([lab_of(c) for c in st["colors"]])
            out = pal[idx].copy(); L = lab_img[sel][:, 0]
            out[:, 0] += st["keep"] * (L - np.median(L))
            lab_img[sel] = out
        elif st["act"] == "bands":
            z = pos[sel][:, 2]; isb = st["is_band"](lab_img[sel])
            nb = 160; edges = np.linspace(z.min(), z.max() + 1e-9, nb + 1)
            bi = np.clip(np.digitize(z, edges) - 1, 0, nb - 1)
            frac = np.bincount(bi, isb, nb) / np.maximum(np.bincount(bi, minlength=nb), 1)
            frac = np.convolve(frac, np.ones(5) / 5, mode="same")
            band = frac[bi] > 0.5
            out = np.where(band[:, None], lab_of(st["color"]), lab_of(st["other"]))
            lab_img[sel] = out
        elif st["act"] == "snap":
            lab_img[sel] = snap(lab_img, sel, st["palette"], st["keep"])
        elif st["act"] == "ticket":
            des = ticket_design(); Hd, Wd = des.shape[:2]
            p = pos[sel]; q = P[np.unique(T[fsel])]
            u = (p[:, 0] - q[:, 0].min()) / max(np.ptp(q[:, 0]), 1e-9)
            # chieu doc: do dai cung tren mat giay (phan duoi bi cong); chieu ngang: chieu thang
            v = arc_param(q[:, 2], q[:, 1], p[:, 2])
            c = des[np.clip(((1 - v) * (Hd - 1)).astype(int), 0, Hd - 1), np.clip((u * (Wd - 1)).astype(int), 0, Wd - 1)]
            lab_img[sel] = srgb_to_lab(c)
        elif st["act"] == "sequins":
            ys, xs = np.nonzero(sel)
            lab_img[sel] = srgb_to_lab(sequins(np.stack([xs, ys], 1).astype(np.float32)))
        changed |= sel
    out = lab_to_srgb(lab_img)
    out = np.where(changed[..., None], out, tex)
    out = dilate(out, uv_mask(UV, h, w) | inside, 16)
    tmp = tempfile.mkdtemp(); png = os.path.join(tmp, "Image_0.png")
    from PIL import Image
    Image.fromarray((np.clip(out[::-1], 0, 1) * 255 + 0.5).astype(np.uint8)).save(png)
    img = bpy.data.images.load(png); img.name = "Image_0"
    node.image = img
    if name == "L19_gang_tay_lap_lanh":  # kim sa: phan chieu manh hon
        bsdf = next(n for n in o.active_material.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
        bsdf.inputs["Metallic"].default_value = 0.6
        bsdf.inputs["Roughness"].default_value = 0.25
    bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", export_image_format="JPEG",
                              export_jpeg_quality=92, export_yup=True)
    print("REPAINT", name, "texel", int(changed.sum()), flush=True)


if __name__ == "__main__":
    for p in sys.argv[1:]:
        process(p)

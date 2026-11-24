"""Ham dung chung cho kiem tra loang mau (audit_color_bleed.py) va bake lai texture
(rebake_items_meshy.py). Xem Docs/Art/KiemTra3D.md, muc "Loang mau".

Moi ham lam viec tren mang numpy lay ra tu Blender: vi tri dinh (the gioi), tam giac,
UV theo goc tam giac va texture sRGB (H, W, 3), hang 0 la day anh (quy uoc cua Blender).
"""
import json, struct
import numpy as np

# 7 diem lay mau trong moi tam giac (toa do trong tam)
BARY = np.array([[1 / 3, 1 / 3, 1 / 3],
                 [2 / 3, 1 / 6, 1 / 6], [1 / 6, 2 / 3, 1 / 6], [1 / 6, 1 / 6, 2 / 3],
                 [1 / 6, 5 / 12, 5 / 12], [5 / 12, 1 / 6, 5 / 12], [5 / 12, 5 / 12, 1 / 6]])


# ---------------------------------------------------------------- doc du lieu

def mesh_arrays(o, uv_name=None):
    """Tra ve (P, T, UV): P (V,3) vi tri the gioi, T (F,3) chi so dinh, UV (F,3,2)."""
    me = o.data
    me.calc_loop_triangles()
    nv, nf = len(me.vertices), len(me.loop_triangles)
    co = np.empty(nv * 3, np.float64); me.vertices.foreach_get("co", co)
    co = co.reshape(-1, 3)
    M = np.array(o.matrix_world)
    P = co @ M[:3, :3].T + M[:3, 3]
    T = np.empty(nf * 3, np.int64); me.loop_triangles.foreach_get("vertices", T)
    L = np.empty(nf * 3, np.int64); me.loop_triangles.foreach_get("loops", L)
    layer = me.uv_layers[uv_name] if uv_name else me.uv_layers.active
    uv = np.empty(len(me.loops) * 2, np.float64); layer.data.foreach_get("uv", uv)
    uv = uv.reshape(-1, 2)
    return P, T.reshape(-1, 3), uv[L.reshape(-1, 3)]


def image_array(img):
    w, h = img.size
    px = np.empty(w * h * 4, np.float32); img.pixels.foreach_get(px)
    return px.reshape(h, w, 4)[:, :, :3].copy()


def glb_base_color_bytes(path):
    """Lay anh base color nhung trong file .glb (doc thang chunk JSON/BIN, khong can Blender)."""
    data = open(path, "rb").read()
    jl = struct.unpack_from("<I", data, 12)[0]
    js = json.loads(data[20:20 + jl])
    bin_off = 20 + jl + 8
    mat = js["materials"][0]["pbrMetallicRoughness"]["baseColorTexture"]["index"]
    img = js["images"][js["textures"][mat]["source"]]
    bv = js["bufferViews"][img["bufferView"]]
    b = data[bin_off + bv.get("byteOffset", 0): bin_off + bv.get("byteOffset", 0) + bv["byteLength"]]
    return b, img.get("mimeType", "image/png")


# ---------------------------------------------------------------- mau

def srgb_to_lab(c):
    c = np.clip(c, 0, 1)
    lin = np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)
    M = np.array([[0.4124, 0.3576, 0.1805], [0.2126, 0.7152, 0.0722], [0.0193, 0.1192, 0.9505]])
    xyz = lin @ M.T / np.array([0.95047, 1.0, 1.08883])
    f = np.where(xyz > 0.008856, np.cbrt(xyz), 7.787 * xyz + 16 / 116)
    return np.stack([116 * f[..., 1] - 16, 500 * (f[..., 0] - f[..., 1]), 200 * (f[..., 1] - f[..., 2])], -1)


def lab_to_srgb(lab):
    fy = (lab[..., 0] + 16) / 116; fx = fy + lab[..., 1] / 500; fz = fy - lab[..., 2] / 200
    f = np.stack([fx, fy, fz], -1)
    xyz = np.where(f > 0.2069, f ** 3, (f - 16 / 116) / 7.787) * np.array([0.95047, 1.0, 1.08883])
    Mi = np.linalg.inv(np.array([[0.4124, 0.3576, 0.1805], [0.2126, 0.7152, 0.0722], [0.0193, 0.1192, 0.9505]]))
    lin = np.clip(xyz @ Mi.T, 0, 1)
    return np.where(lin <= 0.0031308, lin * 12.92, 1.055 * lin ** (1 / 2.4) - 0.055).astype(np.float32)


def delta_e(a, b):
    return np.linalg.norm(a - b, axis=-1)


# ---------------------------------------------------------------- texture

def bilinear(img, uv):
    """Lay mau song tuyen (giong GPU) tai uv (N,2), kep o bien."""
    h, w = img.shape[:2]
    x = np.clip(uv[:, 0] * w - 0.5, 0, w - 1)
    y = np.clip(uv[:, 1] * h - 0.5, 0, h - 1)
    x0 = np.floor(x).astype(int); y0 = np.floor(y).astype(int)
    x1 = np.minimum(x0 + 1, w - 1); y1 = np.minimum(y0 + 1, h - 1)
    fx = (x - x0)[:, None]; fy = (y - y0)[:, None]
    if img.ndim == 2:
        img = img[:, :, None]
    r = (img[y0, x0] * (1 - fx) * (1 - fy) + img[y0, x1] * fx * (1 - fy)
         + img[y1, x0] * (1 - fx) * fy + img[y1, x1] * fx * fy)
    return r


def box_down(img, s):
    if s == 1:
        return img
    h, w = img.shape[:2]
    return img[:h // s * s, :w // s * s].reshape(h // s, s, w // s, s, *img.shape[2:]).mean((1, 3))


def uv_mask(UV, h, w):
    """Mat na cac texel (tam texel) nam trong mot tam giac UV bat ky."""
    m = np.zeros((h, w), bool)
    px = UV * np.array([w, h]) - 0.5
    for t in px:
        x0, y0 = np.floor(t.min(0)).astype(int); x1, y1 = np.ceil(t.max(0)).astype(int)
        x0, y0 = max(x0, 0), max(y0, 0); x1, y1 = min(x1, w - 1), min(y1, h - 1)
        if x1 < x0 or y1 < y0:
            continue
        gx, gy = np.meshgrid(np.arange(x0, x1 + 1), np.arange(y0, y1 + 1))
        a, b, c = t
        d = (b[1] - c[1]) * (a[0] - c[0]) + (c[0] - b[0]) * (a[1] - c[1])
        if abs(d) < 1e-12:
            continue
        l1 = ((b[1] - c[1]) * (gx - c[0]) + (c[0] - b[0]) * (gy - c[1])) / d
        l2 = ((c[1] - a[1]) * (gx - c[0]) + (a[0] - c[0]) * (gy - c[1])) / d
        e = -1e-6
        m[y0:y1 + 1, x0:x1 + 1] |= (l1 >= e) & (l2 >= e) & (1 - l1 - l2 >= e)
    return m


def dilate(tex, mask, steps):
    """Loang mau texel trong mat na ra ngoai `steps` vong (trung binh lang gieng da biet)."""
    t = np.where(mask[..., None], tex, 0).astype(np.float32)
    k = mask.astype(np.float32)
    for _ in range(steps):
        if k.all():
            break
        pt = np.pad(t * k[..., None], ((1, 1), (1, 1), (0, 0))); pk = np.pad(k, 1)
        st = sum(pt[1 + dy:pt.shape[0] - 1 + dy, 1 + dx:pt.shape[1] - 1 + dx]
                 for dy in (-1, 0, 1) for dx in (-1, 0, 1))
        sk = sum(pk[1 + dy:pk.shape[0] - 1 + dy, 1 + dx:pk.shape[1] - 1 + dx]
                 for dy in (-1, 0, 1) for dx in (-1, 0, 1))
        new = (k == 0) & (sk > 0)
        t[new] = st[new] / sk[new][:, None]
        k = np.where(new, 1.0, k)
    # phan con lai (xa moi dao) giu mau that: khong anh huong vi khong bao gio duoc lay mau
    return np.where(k[..., None] > 0, t, tex)


def sample_points(UV):
    """(F,7,2) diem lay mau trong moi tam giac UV."""
    return np.einsum("kj,fjd->fkd", BARY, UV)


# ---------------------------------------------------------------- luoi

def weld_index(P, tol=1e-5):
    """Gop dinh trung vi tri (glTF tach dinh o seam UV) -> chi so dinh da han."""
    key = np.round(P / tol).astype(np.int64)
    _, inv = np.unique(key, axis=0, return_inverse=True)
    return inv.ravel()


def face_neighbors(T, weld):
    """Danh sach mat ke (chung canh 3D) cho moi tam giac."""
    W = weld[T]
    edges = {}
    for f, (a, b, c) in enumerate(W):
        for u, v in ((a, b), (b, c), (c, a)):
            edges.setdefault((min(u, v), max(u, v)), []).append(f)
    nb = [[] for _ in range(len(T))]
    for fs in edges.values():
        if 1 < len(fs) <= 4:
            for f in fs:
                nb[f].extend(g for g in fs if g != f)
    return nb, edges


def tri_area(A):
    """Dien tich tam giac (F,3,d), d = 2 hoac 3."""
    u, v = A[:, 1] - A[:, 0], A[:, 2] - A[:, 0]
    if A.shape[2] == 2:
        return 0.5 * np.abs(u[:, 0] * v[:, 1] - u[:, 1] * v[:, 0])
    return 0.5 * np.linalg.norm(np.cross(u, v), axis=1)


# ---------------------------------------------------------------- phan tich

def face_colors(tex, UV):
    pts = sample_points(UV).reshape(-1, 2)
    lab = srgb_to_lab(bilinear(tex, pts)).reshape(len(UV), 7, 3)
    return lab.mean(1), lab


def analyze(P, T, UV, tex, levels=(1, 2, 4, 8)):
    """Tinh cac chi so loang mau cho mot luoi + texture cua no.

    - outlier: mat co mau lech manh (dE76 > 20) voi MOI mat ke, mat ke >= 2.
    - stretch: tam giac UV gian > 25 lan trung vi (UV vat qua khe giua cac dao) va phu > 40 texel.
    - bleed[s]: o muc mipmap 1/s, sai khac giua mau loc tu texture that va tu texture "dem ly tuong"
      (texel ngoai cac dao UV cua mon nay duoc loang ra tu chinh dao gan nhat). Lon -> GPU dang lay
      mau cua dao ben canh / nen atlas (thieu padding hoac dilation).
    """
    h, w = tex.shape[:2]
    weld = weld_index(P)
    nb, _ = face_neighbors(T, weld)
    col, _ = face_colors(tex, UV)
    a3 = tri_area(P[T]); auv = tri_area(UV) * w * h
    res = {"faces": len(T), "tex": [w, h]}

    out = []
    for f, n in enumerate(nb):
        if len(n) >= 2:
            d = delta_e(col[n], col[f])
            if d.min() > 20:
                out.append((f, float(d.min()), float(np.median(d))))
    res["outlier_idx"] = [o[0] for o in out]
    res["outlier"] = len(out)
    res["outlier_area_mm2"] = round(float(a3[[o[0] for o in out]].sum()) * 1e6, 1) if out else 0.0

    ratio = auv / np.maximum(a3, 1e-14)
    med = np.median(ratio[auv > 0.01]) if (auv > 0.01).any() else 1
    st = np.where((ratio > 25 * med) & (auv > 40))[0]
    res["stretch_idx"] = st.tolist(); res["stretch"] = int(len(st))

    mask = uv_mask(UV, h, w)
    res["uv_cover_pct"] = round(float(mask.mean()) * 100, 1)
    # texel cat bien dao (tam nam ngoai) van chua mau cua dao -> coi la thuoc dao
    m1 = np.pad(mask, 1)
    edge = np.zeros_like(mask)
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            edge |= m1[1 + dy:m1.shape[0] - 1 + dy, 1 + dx:m1.shape[1] - 1 + dx]
    ideal = dilate(tex, edge, 24)
    pts = sample_points(UV).reshape(-1, 2)
    # them diem sat cac dinh tam giac: loang mau thuong o sat bien dao
    edge_pts = np.concatenate([pts, (0.92 * UV + 0.08 * UV.mean(1, keepdims=True)).reshape(-1, 2)])
    bleed = {}
    for s in levels:
        c = bilinear(box_down(tex, s), edge_pts)
        ci = bilinear(box_down(ideal, s), edge_pts)
        d = delta_e(srgb_to_lab(c), srgb_to_lab(ci))
        d = np.concatenate([d[:len(pts)].reshape(-1, 7), d[len(pts):].reshape(-1, 3)], 1).max(1)
        bleed[s] = d
        res[f"bleed_mip{int(np.log2(s))}"] = int((d > 10).sum())
        res[f"bleed_mip{int(np.log2(s))}_p99"] = round(float(np.percentile(d, 99)), 1)
    res["_bleed"] = bleed
    res["_col"] = col
    res["_nb"] = nb
    return res

# R16 goi om: in hinh co gai anime bac mau len mat tren (chieu tu tren xuong vao vung UV rieng).
# Chay: python3 in_goi_om.py vao.glb ref_goi_om_anime.png ra.glb
# Anh mau la anh chup nghieng: nan phoi canh 4 goc mat tren goi thanh hinh chu nhat, tach net muc
# (anh / anh lam mo) roi nhan len mau vai cua goi. Texture moi = texture cu (trai) + hinh in (phai).
import sys, numpy as np, trimesh
from PIL import Image, ImageFilter
src, ref, dst = sys.argv[1:4]
s = trimesh.load(src); m = list(s.geometry.values())[0]
old = m.visual.material.baseColorTexture.convert("RGB"); OW, OH = old.size
# 4 goc mat tren goi trong anh mau (dau, dau, chan, chan) -> chu nhat 340 x 1000 (dau o tren)
quad = [(215, 88), (312, 117), (120, 302), (26, 256)]
PW, PH = 340, 1000
def coeffs(dst_pts, src_pts):
    A = []; B = []
    for (x, y), (u, v) in zip(dst_pts, src_pts):
        A += [[x, y, 1, 0, 0, 0, -u * x, -u * y], [0, 0, 0, x, y, 1, -v * x, -v * y]]; B += [u, v]
    return np.linalg.solve(np.array(A, float), np.array(B, float))
r = Image.open(ref).convert("RGB")
w = r.transform((PW, PH), Image.PERSPECTIVE, tuple(coeffs([(0, 0), (PW, 0), (PW, PH), (0, PH)], quad)), Image.BICUBIC)
a = np.asarray(w).astype(float) + 1
bg = np.asarray(w.filter(ImageFilter.GaussianBlur(25))).astype(float) + 1
ink = np.clip(a / bg, 0, 1.05)
ink = 1 - (1 - np.minimum(ink, 1)) * 2.2          # dam net hon mot chut
ink = np.clip(ink, 0, 1)
# mep 6% quanh vien: mo dan ve 1 (khong in sat mep)
yy, xx = np.mgrid[0:PH, 0:PW]
edge = np.minimum.reduce([xx / PW, 1 - xx / PW, yy / PH, 1 - yy / PH]) / 0.06
ink = 1 - (1 - ink) * np.clip(edge, 0, 1)[..., None]
up = m.face_normals[:, 1] > 0.3
cols = []
uv0 = m.visual.uv
ot = np.asarray(old).astype(float)
for f in m.faces[up]:
    u, v = uv0[f].mean(0); cols.append(ot[int((1 - v) * (OH - 1)), int(u * (OW - 1))])
cream = np.median(cols, 0); print("faces up", up.sum(), "cream", cream.round())
# mat ben: xoa vet soc do-xanh va cham toi sot tu ban Meshy -> mau vai
from recolor import facemasks, paint
P = facemasks(m, OW, OH)
def clean(px):
    mx = px.max(1); sa = (mx - px.min(1)) / np.maximum(mx, 1)
    bad = (sa > 0.45) | (mx < 150) | (px[:, 2] > px[:, 0] - 10); out = px.copy(); out[bad] = cream; return out
ot = paint(ot, [P[i] for i in np.where(~up)[0]], clean); old = Image.fromarray(ot.astype(np.uint8))
TW, TH = 1024, 2048
inkb = np.asarray(Image.fromarray((ink * 255).astype(np.uint8)).resize((TW, TH), Image.BICUBIC)).astype(float) / 255
pr = cream * inkb
# texture gop: cu (OW x OH) ben trai, hinh in (TW x TH) ben phai
new = Image.new("RGB", (OW + TW, max(OH, TH)), tuple(int(c) for c in cream))
new.paste(old, (0, 0)); new.paste(Image.fromarray(np.clip(pr, 0, 255).astype(np.uint8)), (OW, 0))
NW, NH = new.size
m.unmerge_vertices()
uv = m.visual.uv.copy()
uv[:, 0] *= OW / NW; uv[:, 1] = 1 - (1 - uv[:, 1]) * OH / NH
x, z = m.vertices[:, 0], m.vertices[:, 2]
lo, hi = m.bounds
# dau (dinh anh) o x lon; be ngang cua hinh in theo z
vi = m.faces[up].ravel()
t = (z[vi] - lo[2]) / (hi[2] - lo[2]); l = (hi[0] - x[vi]) / (hi[0] - lo[0])
uv[vi, 0] = (OW + t * TW) / NW
uv[vi, 1] = 1 - (l * TH) / NH
mat = m.visual.material; mat.baseColorTexture = new
m.visual = trimesh.visual.TextureVisuals(uv=uv, material=mat)
m.export(dst); print("ok", NW, NH)

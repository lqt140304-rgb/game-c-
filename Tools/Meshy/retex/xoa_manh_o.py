# V48 o giay dau: xoa manh thua sot tu hop bento ngay duoi tan (phia can), giu tan, chop o va can tre.
# Chay: python3 xoa_manh_o.py vao.glb ra.glb
# Truc o = truc can tre (luoi thu 2). Moi mat tan co toa do doc truc a va ban kinh r; mat do (giay dau)
# cho mep duoi a_low(r) cua tan theo tung khoang r; mat nao nam duoi mep do qua 1,2 cm (gan tam 0,2 cm; phia can) la manh thua.
import sys, numpy as np, trimesh
from recolor import facemasks, face_mean, hsv, paint
from PIL import Image
src, dst = sys.argv[1:3]
s = trimesh.load(src)
g, h = s.geometry['o_tan.glb'], s.geometry['o_tan.glb_1']
hc = h.vertices.mean(0); ax = np.linalg.svd(h.vertices - hc)[2][0]
c = g.triangles_center
if (c.mean(0) - hc) @ ax < 0: ax = -ax
a = (c - hc) @ ax; r = np.linalg.norm((c - hc) - np.outer(a, ax), axis=1)
tex = np.asarray(g.visual.material.baseColorTexture.convert('RGB')).astype(float); H, W = tex.shape[:2]
cols = np.array([face_mean(tex, p) for p in facemasks(g, W, H)])
hu, sa, v = hsv(cols); red = ((hu < 20) | (hu > 340)) & (sa > 0.4)
bins = np.linspace(0, r.max() + 1e-6, 23); idx = np.digitize(r, bins) - 1
low = np.array([np.percentile(a[red & (idx == k)], 10) if (red & (idx == k)).sum() > 5 else np.nan for k in range(22)])
ok = ~np.isnan(low); low = np.interp(np.arange(22), np.where(ok)[0], low[ok])   # dien khoang trong (gan tam, it mat do)
low[: int(np.argmax(ok))] = low[int(np.argmax(ok))]
near = r < 0.45 * r.max()   # gan tam: siet hon (manh thua day dac o day)
bad = a < low[idx] - np.where(near, 0.002, 0.012)
hub = (r < 0.3 * r.max()) & (a < low[idx] + 0.01) & ~red   # manh xanh xam, trang lom chom quanh chop duoi
bad |= hub
blue = (((hu > 170) & (hu < 270) & (sa > 0.12)) | ((sa < 0.15) & (v < 0.75))) & (a < low[idx] + 0.02)   # manh xanh xam con lai duoi tan
bad |= blue
nd = g.face_normals @ ax
bad |= (np.abs(nd) < 0.35) & (r < 0.3 * r.max()) & (a < low[idx] + 0.02)
bad |= (r < 0.3 * r.max()) & (a < low[idx] + 0.004) & (nd > -0.5)   # mat khong up xuong (phap tuyen loi) sat duoi chop   # manh dung nghieng (vay lom chom) quanh chop duoi
print('xoa', bad.sum(), 'tren', len(g.faces))
# phan con lai gan chop duoi tan, khong phai giay do: to mau giay do toi (chim vao mat duoi tan)
rest = ~bad & ~red & (r < 0.35 * r.max()) & (a < low[idx] + 0.02)
Pm = facemasks(g, W, H)
paper = np.median(cols[red], 0); print('giay', paper.round())
tex = paint(tex, [Pm[i] for i in np.where(rest)[0]], lambda px: paper * (0.6 + 0.3 * px.mean(1, keepdims=True) / 255))
def unblue(px):  # tung diem anh: xanh xam -> giay do toi, giu do
    mx = px.max(1); sp = (mx - px.min(1)) / np.maximum(mx, 1); b = (px[:, 2] > px[:, 0] - 5) | (sp < 0.18)
    out = px.copy(); out[b] = paper * (0.6 + 0.3 * px[b].mean(1, keepdims=True) / 255); return out
zone = ~bad & (r < 0.45 * r.max())
tex = paint(tex, [Pm[i] for i in np.where(zone)[0]], unblue)
# giay dau: mat do (theo tam mat) xoa vet vang/kem lem -> do giay deu, giu sang toi
def repaint(px):
    mx = px.max(1); sp = (mx - px.min(1)) / np.maximum(mx, 1)
    hh = np.degrees(np.arctan2(np.sqrt(3) * (px[:, 1] - px[:, 2]), 2 * px[:, 0] - px[:, 1] - px[:, 2])) % 360
    b = ((hh > 22) & (hh < 75)) | (sp < 0.35)
    out = px.copy(); out[b] = paper * (0.85 + 0.25 * mx[b, None] / 255); return out
tex = paint(tex, [Pm[i] for i in np.where(red & ~bad)[0]], repaint)
g.visual.material.baseColorTexture = Image.fromarray(np.clip(tex, 0, 255).astype(np.uint8)); print('to tre', rest.sum())
keep = g.submesh([np.where(~bad)[0]], append=True)
t = keep.copy(); t.merge_vertices(merge_tex=True, merge_norm=True, digits_vertex=4)   # bo vun le con sot
cc = trimesh.graph.connected_components(t.face_adjacency, min_len=1)
big = np.concatenate([q for q in cc if len(q) >= 30]); print('vun le bo', len(keep.faces) - len(big))
keep = keep.submesh([np.sort(big)], append=True)
s.geometry['o_tan.glb'] = keep
# khoen tre (runner) quanh can ngay duoi chop, che lo sau khi xoa: non cut, mau tre
a0 = np.percentile(a[red & (r < 0.25 * r.max())], 20) if (red & (r < 0.25 * r.max())).any() else low[0]
Rh = np.linalg.norm((h.vertices - hc) - np.outer((h.vertices - hc) @ ax, ax), axis=1).max()
L = 0.09 * r.max()
k = trimesh.creation.cylinder(radius=1.0, height=L, sections=24)
v = k.vertices.copy(); top = v[:, 2] > 0
v[:, :2] *= np.where(top, 0.22 * r.max(), 1.6 * Rh)[:, None]   # dau to sat tan, dau nho om can
k.vertices = v
R3 = trimesh.geometry.align_vectors([0, 0, 1], ax)
ang = np.arctan2(k.triangles_center[:, 1], k.triangles_center[:, 0])   # nan xen ke sang toi (truoc khi xoay)
fc = np.where((np.floor((ang + np.pi) / (2 * np.pi) * 24) % 2 == 0)[:, None], [188, 150, 92, 255], [118, 84, 44, 255])   # mau theo can tre
k.visual = trimesh.visual.ColorVisuals(k, face_colors=fc.astype(np.uint8))
k.apply_transform(R3); k.apply_translation(hc + ax * (a0 - L / 2))
s.add_geometry(k, geom_name='khoen')

s.export(dst)

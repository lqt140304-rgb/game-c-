# Dung mon don gian bang Blender (bpy) khi Meshy lam hong, khong ton credit.
# Chay: python3 dung_mon.py <ma> ra.glb [vao.glb]   (can: pip install bpy)
#  V44 Lightstick (dau tron hong phat sang nhe, can xam co gan)
#  V47 Hop bento son mai 2 tang (den, vien do, huy hieu vang)
#  V48 O giay dau: lap can tre vao tan o lay tu Meshy (vao.glb), nam nghieng tren san
#  H37 Manh ve tinh that: than boc giay vang, 2 tam pin xanh, chao, ang-ten
#  H38 Bang doi truong co chu ky: vong vai vang dung, chu C den, chu ky but da
#  H39 Ban cau co nguyen ven: bang go chu cai khac chay, con tro hinh tim
# Kich thuoc that (met), tam giua, day y=0 (glTF).
import sys, math, random, bpy, bmesh, numpy as np
from mathutils import Vector, Matrix
from PIL import Image, ImageDraw, ImageFont

code, dst = sys.argv[1], sys.argv[2]
src = sys.argv[3] if len(sys.argv) > 3 else None
rng = random.Random(11)
bpy.ops.wm.read_factory_settings(use_empty=True)
FONT = "/usr/share/fonts/truetype/dejavu/DejaVuSerif-Bold.ttf"


# ---------- vat lieu ----------
def image_from_pil(name, im):
    a = np.asarray(im.convert("RGB")).astype(np.float32) / 255.0
    H, W = a.shape[:2]
    img = bpy.data.images.new(name, W, H)
    rgba = np.ones((H, W, 4), np.float32)
    rgba[..., :3] = a[::-1]
    img.pixels.foreach_set(rgba.ravel())
    img.pack()
    return img


def mat(name, color=(1, 1, 1), rough=0.8, metal=0.0, im=None, emit=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    b = nt.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = (*color, 1)
    b.inputs["Roughness"].default_value = rough
    b.inputs["Metallic"].default_value = metal
    if im is not None:
        t = nt.nodes.new("ShaderNodeTexImage")
        t.image = image_from_pil(name, im)
        nt.links.new(t.outputs["Color"], b.inputs["Base Color"])
    if emit:
        b.inputs["Emission Color"].default_value = (*color, 1)
        b.inputs["Emission Strength"].default_value = emit
    return m


def wood_img(W, H, base, dark=0.75, seed=1):
    r = random.Random(seed)
    a = np.ones((H, W, 3)) * np.array(base) * 255
    for y in range(H):
        a[y] *= 1 - (1 - dark) * (0.5 + 0.5 * math.sin(y * 0.35 + 3 * math.sin(y * 0.05) + r.random() * 0.4)) * 0.5
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8))


# ---------- luoi ----------
def add_obj(bm, name, material):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob)
    ob.data.materials.append(material)
    return ob


def box(size, loc=(0, 0, 0), bevel=0.0):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.scale(bm, vec=Vector(size), verts=bm.verts)
    if bevel:
        bmesh.ops.bevel(bm, geom=bm.edges[:] + bm.verts[:], offset=bevel, segments=1, affect="EDGES")
    bmesh.ops.translate(bm, vec=Vector(loc), verts=bm.verts)
    return bm


def cyl(r1, r2, depth, seg=12, loc=(0, 0, 0), caps=True):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=caps, segments=seg, radius1=r1, radius2=r2, depth=depth)
    bmesh.ops.translate(bm, vec=Vector(loc), verts=bm.verts)
    return bm


def uv_box(bm, s=1.0):
    """Chieu UV theo truc troi nhat cua phap tuyen (texture lap)."""
    uv = bm.loops.layers.uv.verify()
    for f in bm.faces:
        n = [abs(c) for c in f.normal]
        ax = n.index(max(n))
        for lp in f.loops:
            c = lp.vert.co
            a, b = [(c.y, c.z), (c.x, c.z), (c.x, c.y)][ax]
            lp[uv].uv = (a * s, b * s)


def uv_top(bm, lo, hi):
    """Chieu tu tren xuong, chuan hoa theo khung lo..hi (texture phu kin mat tren)."""
    uv = bm.loops.layers.uv.verify()
    for f in bm.faces:
        for lp in f.loops:
            c = lp.vert.co
            lp[uv].uv = ((c.x - lo[0]) / (hi[0] - lo[0]), (c.y - lo[1]) / (hi[1] - lo[1]))


def uv_cyl(bm, z0, z1):
    """Quanh truc Z: u = goc, v = chieu cao; sua mat vat qua duong noi."""
    uv = bm.loops.layers.uv.verify()
    for f in bm.faces:
        us = [(math.atan2(lp.vert.co.y, lp.vert.co.x) / (2 * math.pi)) % 1.0 for lp in f.loops]
        if max(us) - min(us) > 0.5:
            us = [u + 1 if u < 0.5 else u for u in us]
        for lp, u in zip(f.loops, us):
            lp[uv].uv = (u, (lp.vert.co.z - z0) / (z1 - z0))


def join(obs):
    bpy.ops.object.select_all(action="DESELECT")
    for o in obs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = obs[0]
    bpy.ops.object.join()
    return obs[0]


def verts(ob):
    return np.array([ob.matrix_world @ v.co for v in ob.data.vertices])


def rest_two_ends(ob):
    """Mon dai nam tren san (truc dai = X): nghieng quanh Y de ca hai dau cung cham san."""
    best = None
    for a in np.radians(np.arange(-45, 45.5, 0.5)):
        R = Matrix.Rotation(a, 4, "Y")
        co = np.array([R @ Vector(v) for v in verts(ob)])
        x0, x1 = co[:, 0].min(), co[:, 0].max()
        L = x1 - x0
        lo = co[:, 2].min()
        a_end = co[co[:, 0] < x0 + 0.3 * L, 2].min()
        b_end = co[co[:, 0] > x1 - 0.1 * L, 2].min()
        cost = (a_end - lo) + (b_end - lo)
        if best is None or cost < best[0]:
            best = (cost, a)
    ob.data.transform(Matrix.Rotation(best[1], 4, "Y"))


def finish(ob, longest=None):
    bpy.context.view_layer.objects.active = ob
    ob.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    co = verts(ob)
    if longest:
        ob.data.transform(Matrix.Scale(longest / (co.max(0) - co.min(0)).max(), 4))
        co = verts(ob)
    lo, hi = co.min(0), co.max(0)
    ob.data.transform(Matrix.Translation((-(lo[0] + hi[0]) / 2, -(lo[1] + hi[1]) / 2, -lo[2])))
    ob.data.update()


# ---------- tung mon ----------
if code == "V44":  # lightstick: truc X, dau tron o x am
    rib = Image.new("RGB", (8, 64), (110, 110, 118))
    d = ImageDraw.Draw(rib)
    for y in range(0, 64, 8):
        d.rectangle((0, y, 7, y + 2), fill=(70, 70, 76))
    bm = cyl(0.016, 0.014, 0.17, 12, (0, 0, 0.085 + 0.02))
    uv_cyl(bm, 0.02, 0.19)
    can = add_obj(bm, "can", mat("can", im=rib, rough=0.6))
    co = add_obj(cyl(0.022, 0.018, 0.02, 12, (0, 0, 0.01)), "co", mat("co", (0.75, 0.25, 0.6), 0.5))
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, subdivisions=2, radius=0.042)
    bmesh.ops.translate(bm, vec=Vector((0, 0, -0.035)), verts=bm.verts)
    dau = add_obj(bm, "dau", mat("dau", (0.85, 0.3, 0.75), 0.45, emit=0.15))
    ob = join([can, co, dau])
    ob.data.transform(Matrix.Rotation(math.radians(90), 4, "Y"))  # Z -> X
    rest_two_ends(ob)
    ob.data.transform(Matrix.Rotation(math.radians(25), 4, "Z"))
    finish(ob)

elif code == "V47":  # bento 18 x 18 cm, 2 tang + nap
    den = mat("son_den", (0.04, 0.035, 0.035), 0.5)
    do = mat("vien_do", (0.62, 0.07, 0.05), 0.35)
    vang = mat("vang", (0.85, 0.65, 0.22), 0.35, 0.8)
    parts = [add_obj(box((0.18, 0.18, 0.055), (0, 0, 0.0275), 0.004), "t1", den),
             add_obj(box((0.18, 0.18, 0.055), (0, 0, 0.0835), 0.004), "t2", den),
             add_obj(box((0.186, 0.186, 0.028), (0, 0, 0.125), 0.006), "nap", den),
             add_obj(box((0.183, 0.183, 0.006), (0, 0, 0.0555)), "vien1", do),
             add_obj(box((0.183, 0.183, 0.006), (0, 0, 0.1115)), "vien2", do),
             add_obj(cyl(0.032, 0.032, 0.002, 16, (0, 0, 0.14)), "huy", vang)]
    finish(join(parts))

elif code == "V48":  # o giay dau: tan Meshy + can tre
    bpy.ops.import_scene.gltf(filepath=src)
    tan = [o for o in bpy.context.scene.objects if o.type == "MESH"][0]
    bpy.context.view_layer.objects.active = tan
    tan.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    P = verts(tan)
    c = P.mean(0)
    n = np.linalg.svd(P - c)[2][2]
    rad = np.linalg.norm((P - c) - np.outer((P - c) @ n, n), axis=1)
    R = np.percentile(rad, 98)
    hub, rim = P[rad < 0.15 * R].mean(0), P[rad > 0.85 * R].mean(0)
    if (rim - hub) @ n < 0:
        n = -n  # n huong tu chop o ve phia vanh (phia can)
    tre = wood_img(16, 64, (0.78, 0.62, 0.38), 0.85, 3)
    d = ImageDraw.Draw(tre)
    for y in (12, 34, 56):  # dot tre
        d.rectangle((0, y, 15, y + 2), fill=(120, 90, 50))
    L = 1.7 * R
    bm = cyl(0.03 * R, 0.035 * R, L, 10, (0, 0, L / 2 - 0.08 * R))
    uv_cyl(bm, -0.08 * R, L)
    can = add_obj(bm, "can", mat("tre", im=tre, rough=0.7))
    q = Vector((0, 0, 1)).rotation_difference(Vector(n))
    can.data.transform(q.to_matrix().to_4x4())
    can.data.transform(Matrix.Translation(Vector(hub)))
    ob = join([tan, can])
    # truc o -> X roi nam nghieng cho vanh va dau can cham san
    co = verts(ob)
    q2 = Vector(n).rotation_difference(Vector((1, 0, 0)))
    ob.data.transform(q2.to_matrix().to_4x4())
    finish(ob, 0.85)
    rest_two_ends(ob)
    finish(ob)

elif code == "H37":  # ve tinh: than 14 x 14 x 18 cm boc giay vang
    im = Image.new("RGB", (128, 128))
    d = ImageDraw.Draw(im)
    for _ in range(160):  # nep nhan giay vang
        x, y = rng.randrange(128), rng.randrange(128)
        t = rng.uniform(0.7, 1.15)
        d.polygon([(x, y), (x + rng.randint(-30, 30), y + rng.randint(5, 30)), (x + rng.randint(5, 30), y + rng.randint(-30, 30))],
                  fill=tuple(int(min(255, v * t)) for v in (215, 165, 55)))
    bm = box((0.14, 0.14, 0.18), (0, 0, 0.09), 0.004)
    uv_box(bm, 6)
    than = add_obj(bm, "than", mat("giay_vang", im=im, rough=0.35, metal=0.7))
    pin = Image.new("RGB", (256, 128), (190, 190, 200))
    d = ImageDraw.Draw(pin)
    for i in range(8):
        for j in range(4):
            d.rectangle((4 + i * 31, 4 + j * 31, 31 + i * 31, 31 + j * 31), fill=(35, 60, 150))
    xam = mat("xam", (0.7, 0.7, 0.72), 0.4, 0.6)
    parts = [than]
    for s in (1, -1):
        cx = s * 0.26
        bm = box((0.30, 0.13, 0.008), (cx, 0, 0.10))
        uv_top(bm, (cx - 0.15, -0.065), (cx + 0.15, 0.065))
        parts.append(add_obj(bm, "pin", mat("pin", im=pin, rough=0.6)))
        r = cyl(0.006, 0.006, 0.04, 8, (0, 0, 0))
        bmesh.ops.rotate(r, verts=r.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(90), 3, "Y"))
        bmesh.ops.translate(r, vec=Vector((s * 0.09, 0, 0.10)), verts=r.verts)
        parts.append(add_obj(r, "thanh", xam))
    parts.append(add_obj(cyl(0.065, 0.012, 0.03, 16, (0.0, 0.0, 0.195)), "chao", mat("chao", (0.9, 0.9, 0.88), 0.5)))
    parts.append(add_obj(cyl(0.003, 0.002, 0.09, 6, (0.04, 0.04, 0.225)), "angten", xam))
    finish(join(parts))

elif code == "H38":  # bang doi truong: vong r 5,2 cm, cao 8 cm, vai 3 mm
    W, H = 1024, 256
    im = Image.new("RGB", (W, H), (240, 205, 40))
    d = ImageDraw.Draw(im)
    for y in (6, H - 12):  # vien det
        d.rectangle((0, y, W, y + 6), fill=(215, 180, 30))
    f = ImageFont.truetype(FONT, 200)
    d.text((140, H // 2), "C", font=f, fill=(20, 20, 20), anchor="mm")
    pts = []  # chu ky but da: cac vong chu nghieng noi nhau
    for i in range(160):
        t = i * 0.16
        amp = 38 * (1.3 if i < 30 else 0.8 + 0.2 * math.sin(i * 0.07))
        pts.append((300 + 2.1 * i + 16 * math.cos(t * 1.9), 135 + amp * math.sin(t * 1.9) + 10 * math.sin(t * 0.4)))
    d.line(pts, fill=(15, 15, 25), width=6, joint="curve")
    d.line([(300, 200), (650, 182)], fill=(15, 15, 25), width=5)
    bm = cyl(0.056, 0.056, 0.065, 24, (0, 0, 0.0325), caps=False)
    bmesh.ops.scale(bm, vec=Vector((1.0, 0.8, 1.0)), verts=bm.verts)  # vai mem, vong hoi det
    uv_cyl(bm, 0, 0.065)
    ob = add_obj(bm, "bang", mat("vai_vang", im=im, rough=0.85))
    m = ob.modifiers.new("day", "SOLIDIFY")
    m.thickness = 0.003
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.modifier_apply(modifier="day")
    finish(ob)

elif code == "H39":  # ban cau co 30 x 20 x 1,5 cm, con tro tim 9 cm
    W, H = 1200, 800
    im = wood_img(W, H, (0.52, 0.33, 0.18), 0.8, 5).convert("RGB")
    d = ImageDraw.Draw(im)
    ink = (20, 10, 5)
    d.rounded_rectangle((25, 25, W - 25, H - 25), 30, outline=ink, width=6)
    f, fs = ImageFont.truetype(FONT, 92), ImageFont.truetype(FONT, 68)
    for row, (txt, rr, cy) in enumerate((("ABCDEFGHIJKLM", 520, 790), ("NOPQRSTUVWXYZ", 380, 790))):
        for i, ch in enumerate(txt):  # chu chay theo vong cung
            a = math.radians(-150 + 120 * i / (len(txt) - 1))
            d.text((W / 2 + rr * math.cos(a), cy + rr * math.sin(a)), ch, font=f, fill=ink, anchor="mm")
    d.text((W / 2, 600), "1 2 3 4 5 6 7 8 9 0", font=fs, fill=ink, anchor="mm")
    d.text((140, 95), "YES", font=fs, fill=ink, anchor="mm")
    d.text((W - 140, 95), "NO", font=fs, fill=ink, anchor="mm")
    d.text((W / 2, 715), "GOOD BYE", font=fs, fill=ink, anchor="mm")
    bm = box((0.30, 0.20, 0.015), (0, 0, 0.0075), 0.004)
    uv_top(bm, (-0.15, -0.10), (0.15, 0.10))
    ban = add_obj(bm, "ban", mat("go_chu", im=im, rough=0.75))
    pts = []  # duong tim, dinh tim chi ve phia tren ban
    for i in range(28):
        t = 2 * math.pi * i / 28
        pts.append((16 * math.sin(t) ** 3, 13 * math.cos(t) - 5 * math.cos(2 * t) - 2 * math.cos(3 * t) - math.cos(4 * t)))
    k = 0.09 / 34.0
    bm = bmesh.new()
    face = bm.faces.new([bm.verts.new((x * k, y * k, 0)) for x, y in pts])
    ext = bmesh.ops.extrude_face_region(bm, geom=[face])
    for v in ext["geom"]:
        if isinstance(v, bmesh.types.BMVert):
            v.co.z += 0.008
    bmesh.ops.triangulate(bm, faces=bm.faces[:])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(160), 3, "Z"))
    bmesh.ops.translate(bm, vec=Vector((0.02, -0.02, 0.0155)), verts=bm.verts)
    uv_box(bm, 8)
    tro = add_obj(bm, "tro", mat("go_tro", im=wood_img(64, 64, (0.75, 0.55, 0.32), 0.8, 9), rough=0.6))
    finish(join([ban, tro]))

bpy.ops.object.select_all(action="SELECT")
bpy.ops.export_scene.gltf(filepath=dst, export_format="GLB", use_selection=True)
ob = [o for o in bpy.context.scene.objects if o.type == "MESH"][0]
print(code, "tris", sum(len(p.vertices) - 2 for p in ob.data.polygons), "size", np.round(verts(ob).max(0) - verts(ob).min(0), 3))

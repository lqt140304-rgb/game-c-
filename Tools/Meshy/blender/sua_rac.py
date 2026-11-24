# Sua hinh dang mon Rac bang Blender (bpy), khong ton credit.
# Chay: python3 sua_rac.py <ma> vao.glb ra.glb   (vao.glb bo qua voi mon dung moi: R13, R20)
# Can: pip install bpy (Blender dang module Python).
#  R13 que kem: dung lai que go da liem sach (Meshy ve ca cay kem), dau bo tron, vet can, vet kem sot.
#  R20 mui Pinocchio: dung lai mui go thuon nhon gay doi, mat gay lom chom, dau hong (ban Meshy giong que diem).
#  R16 goi om: xoay thang theo truc dai, lam day (ban Meshy det, nam cheo).
#  R30 ve vo vien: giam luoi con ~1/4 (ban Meshy ~9,9 nghin tam giac).
import sys, math, random, bpy, bmesh, numpy as np
from mathutils import Vector, Matrix

code, src, dst = sys.argv[1:4]
random.seed(7)
bpy.ops.wm.read_factory_settings(use_empty=True)


def make_image(name, px):  # px: H x W x 3 (0..1), hang 0 = tren cung
    H, W = px.shape[:2]
    img = bpy.data.images.new(name, W, H)
    rgba = np.ones((H, W, 4), np.float32)
    rgba[..., :3] = px[::-1]  # Blender luu tu duoi len
    img.pixels.foreach_set(rgba.ravel())
    img.pack()
    return img


def textured_material(img):
    mat = bpy.data.materials.new("mat")
    mat.use_nodes = True
    nt = mat.node_tree
    bsdf = nt.nodes["Principled BSDF"]
    bsdf.inputs["Roughness"].default_value = 0.8
    bsdf.inputs["Metallic"].default_value = 0.0
    tex = nt.nodes.new("ShaderNodeTexImage")
    tex.image = img
    tex.interpolation = "Closest"
    nt.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
    return mat


def obj_from_bm(bm, name):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob)
    return ob


def finish(ob):
    """Tam giua, day y=0 (truc Y cua glTF = truc Z Blender)."""
    me = ob.data
    co = np.array([v.co[:] for v in me.vertices])
    lo, hi = co.min(0), co.max(0)
    off = Vector((-(lo[0] + hi[0]) / 2, -(lo[1] + hi[1]) / 2, -lo[2]))
    me.transform(Matrix.Translation(off))
    me.update()


def wood_strip(W, H, base, grain=0.06):
    px = np.ones((H, W, 3)) * np.array(base)
    for x in range(W):  # van go doc theo chieu dai
        px[:, x] *= 1 - grain * (0.5 + 0.5 * math.sin(x * 1.7 + random.random()))
    return px


if code == "R13":
    # Que kem 114 x 10 x 3 mm, hai dau bo tron, 2 vet can o dau tren.
    L, Wd, T, seg = 0.114, 0.010, 0.003, 6
    r = Wd / 2
    pts = []
    for i in range(seg + 1):  # dau duoi (y am)
        a = math.pi + math.pi * i / seg
        pts.append((r * math.cos(a), -L / 2 + r + r * math.sin(a)))
    top = []
    for i in range(seg + 1):  # dau tren (y duong)
        a = math.pi * i / seg
        top.append((r * math.cos(a), L / 2 - r + r * math.sin(a)))
    # vet can: lom vao o canh phai va trai gan dau tren
    for side, y0 in ((1, 0.030), (-1, 0.038)):
        pts_bite = [(side * r, y0 - 0.004), (side * r * 0.35, y0 - 0.0015), (side * r * 0.45, y0 + 0.001), (side * r, y0 + 0.004)]
        if side == 1:
            pts += pts_bite
        else:
            top_after = pts_bite[::-1]
    outline = pts + top + top_after
    bm = bmesh.new()
    vs = [bm.verts.new((x, y, 0)) for x, y in outline]
    f = bm.faces.new(vs)
    ext = bmesh.ops.extrude_face_region(bm, geom=[f])
    for v in ext["geom"]:
        if isinstance(v, bmesh.types.BMVert):
            v.co.z += T
    bmesh.ops.triangulate(bm, faces=bm.faces[:])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    # UV chieu tu tren: u theo be ngang, v theo chieu dai
    uv = bm.loops.layers.uv.new()
    for fa in bm.faces:
        for lp in fa.loops:
            lp[uv].uv = ((lp.vert.co.x + r) / (2 * r), (lp.vert.co.y + L / 2) / L)
    ob = obj_from_bm(bm, "R13_que_kem")
    px = wood_strip(16, 128, (0.78, 0.62, 0.40))
    px[:28] = px[:28] * 0.6 + np.array([0.45, 0.25, 0.16]) * 0.4  # vet kem socola sot o dau tren, nhat
    for y in range(28, 40):
        k = (40 - y) / 12 * 0.4
        px[y] = px[y] * (1 - k) + np.array([0.55, 0.32, 0.22]) * k
    px[118:] *= 0.9  # dau cam hoi ban
    ob.data.materials.append(textured_material(make_image("r13", px)))
    ob.rotation_euler = (0, 0, math.radians(35))

elif code == "R20":
    # Mui go dai 25 cm thuon nhon, 8 canh, gay o 55%, hai doan nam lech nhau.
    L, r0, r1, n = 0.25, 0.012, 0.004, 8
    cut = 0.55

    def piece(z0, z1, jag_lo, jag_hi, name):
        bm = bmesh.new()
        rings = []
        steps = 6
        for s in range(steps + 1):
            t = z0 + (z1 - z0) * s / steps
            rad = r0 + (r1 - r0) * t
            ring = []
            for k in range(n):
                a = 2 * math.pi * k / n
                zz = t * L
                if (s == 0 and jag_lo) or (s == steps and jag_hi):
                    zz += random.uniform(-0.006, 0.006)  # mat gay lom chom
                ring.append(bm.verts.new((rad * math.cos(a), rad * math.sin(a), zz)))
            rings.append(ring)
        for a_, b_ in zip(rings, rings[1:]):
            for k in range(n):
                bm.faces.new((a_[k], a_[(k + 1) % n], b_[(k + 1) % n], b_[k]))
        bm.faces.new(rings[0][::-1])
        last = rings[-1]
        if z1 >= 1.0:  # dau mui: chop tron
            tip = bm.verts.new((0, 0, L + 0.004))
            for k in range(n):
                bm.faces.new((last[k], last[(k + 1) % n], tip))
        else:
            bm.faces.new(last)
        bmesh.ops.triangulate(bm, faces=bm.faces[:])
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
        uv = bm.loops.layers.uv.new()
        for fa in bm.faces:
            for lp in fa.loops:
                c = lp.vert.co
                lp[uv].uv = ((math.atan2(c.y, c.x) / (2 * math.pi)) % 1.0, min(max(c.z / (L + 0.004), 0), 1))
        return obj_from_bm(bm, name)

    a = piece(0.0, cut, False, True, "goc")
    b = piece(cut, 1.0, True, False, "ngon")
    # tach doan ngon ra, xoay lech
    b.location = (0.012, 0.0, 0.018)
    b.rotation_euler = (0, 0, 0)
    b.data.transform(Matrix.Translation((0, 0, -cut * L)))
    b.data.transform(Matrix.Rotation(math.radians(14), 4, "Y"))
    b.data.transform(Matrix.Translation((0, 0, cut * L)))
    px = wood_strip(16, 128, (0.70, 0.52, 0.33))
    for y in range(0, 14):  # dau mui hong (hang tren = v cao = dau mui)
        k = (14 - y) / 14
        px[y] = px[y] * (1 - k) + np.array([0.86, 0.48, 0.48]) * k
    mat = textured_material(make_image("r20", px))
    for o in (a, b):
        o.data.materials.append(mat)
    bpy.ops.object.select_all(action="DESELECT")
    a.select_set(True); b.select_set(True)
    bpy.context.view_layer.objects.active = a
    bpy.ops.object.join()
    ob = a
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    # nam ngang tren san: truc dai Z -> X
    ob.data.transform(Matrix.Rotation(math.radians(90), 4, "Y"))
    ob.data.transform(Matrix.Rotation(math.radians(30), 4, "Z"))

else:
    bpy.ops.import_scene.gltf(filepath=src)
    ob = [o for o in bpy.context.scene.objects if o.type == "MESH"][0]
    bpy.context.view_layer.objects.active = ob
    ob.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    if code == "R16":
        co = np.array([v.co[:] for v in ob.data.vertices])
        xy = co[:, :2] - co[:, :2].mean(0)
        w, e = np.linalg.eigh(np.cov(xy.T))
        ang = math.atan2(e[1, 1], e[0, 1])
        ob.data.transform(Matrix.Rotation(-ang, 4, "Z"))  # truc dai -> X
        co = np.array([v.co[:] for v in ob.data.vertices])
        ext = co.max(0) - co.min(0)
        ob.data.transform(Matrix.Diagonal((1.0 / ext[0], 0.34 / ext[1], 0.20 / ext[2], 1)))  # 1 m x 34 cm x 20 cm
    elif code == "R30":
        m = ob.modifiers.new("giam", "DECIMATE")
        m.ratio = 0.25
        bpy.ops.object.modifier_apply(modifier="giam")
    ob.data.update()

finish(ob)
bpy.ops.object.select_all(action="SELECT")
bpy.ops.export_scene.gltf(filepath=dst, export_format="GLB", use_selection=True)
print(code, "tris", sum(len(p.vertices) - 2 for p in ob.data.polygons))

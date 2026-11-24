"""Rương Thể thao (bản tạm): quả bóng đá tách đôi trên chân đế phát bóng. Dựng hoàn toàn bằng bpy.

Chạy:  python make_chest_the_thao.py -- <ra.glb> [<ảnh_xem_trước.png> [samples] [cỡ_ô]]
Cấu trúc GLB: Chest_TheThao (root) -> Body (nửa dưới bóng + chân đế), Lid (nửa trên, gốc ở bản lề sau),
Binding (đai thun đỏ quanh đường xích đạo), Seal (huy hiệu dán ở mặt trước nắp), Glow (quầng sáng ở miệng).
Gốc tọa độ ở đáy giữa, mét, Blender Z lên, mặt trước là -Y. Nắp mở = xoay âm quanh trục X.
"""
import math, sys, os
import bpy, bmesh, mathutils
from mathutils import Vector

A = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
OUT = A[0] if A else "Chest_TheThao.glb"
PREVIEW = A[1] if len(A) > 1 else None
SAMPLES = int(A[2]) if len(A) > 2 else 48
CELL = int(A[3]) if len(A) > 3 else 480

R = 0.17                 # bán kính bóng
TEE_H = 0.065            # chân đế phát bóng
CZ = TEE_H + R - 0.025   # tâm bóng (bóng lún nhẹ vào chân đế)
SEAM = CZ + 0.005        # đường tách nắp, hơi trên xích đạo

PALETTE = {  # sRGB
    "leather": (0.88, 0.86, 0.81), "patch": (0.15, 0.16, 0.19), "interior": (0.07, 0.06, 0.06),
    "tee": (0.88, 0.45, 0.12), "tee_dk": (0.55, 0.27, 0.08), "strap": (0.20, 0.24, 0.32),
    "steel": (0.55, 0.57, 0.60), "badge": (0.92, 0.74, 0.22), "glow": (1.0, 0.82, 0.45),
}
_m = {}


def mat(n):
    if n in _m:
        return _m[n]
    m = bpy.data.materials.new(n); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    c = tuple(x ** 2.2 for x in PALETTE[n])
    b.inputs["Base Color"].default_value = (*c, 1)
    b.inputs["Roughness"].default_value = 0.45 if n in ("leather", "patch", "badge", "steel") else 0.8
    if n in ("steel", "badge"):
        b.inputs["Metallic"].default_value = 0.8
    if n == "glow":
        b.inputs["Emission Color"].default_value = (*c, 1)
        b.inputs["Emission Strength"].default_value = 2.0
    _m[n] = m
    return m


def obj_from_bm(bm, name, mats):
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    for n in mats:
        me.materials.append(mat(n))
    for p in me.polygons:
        p.use_smooth = False
    o = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(o)
    return o


def ball_half(upper):
    """Nửa quả bóng icosphere chia 2, mảng ngũ giác tô tối; mặt cắt là lòng tối."""
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, subdivisions=3, radius=R)
    rot = mathutils.Euler((math.radians(18), 0, math.radians(10))).to_matrix()
    bmesh.ops.rotate(bm, verts=bm.verts, matrix=rot)
    tmp = bmesh.new(); bmesh.ops.create_icosphere(tmp, subdivisions=1, radius=1)  # 12 đỉnh = 12 mảng ngũ giác
    dirs = [rot @ v.co.normalized() for v in tmp.verts]; tmp.free()
    cen = [(f, f.calc_center_median().normalized()) for f in bm.faces]
    for d in dirs:  # 5 mặt quanh mỗi đỉnh icosahedron gốc = một mảng ngũ giác
        for f, _ in sorted(cen, key=lambda t: -t[1].dot(d))[:5]:
            f.material_index = 1
    bmesh.ops.translate(bm, verts=bm.verts, vec=(0, 0, CZ))
    geom = bm.verts[:] + bm.edges[:] + bm.faces[:]
    res = bmesh.ops.bisect_plane(bm, geom=geom, plane_co=(0, 0, SEAM), plane_no=(0, 0, 1),
                                 clear_inner=upper, clear_outer=not upper)
    edges = [e for e in res["geom_cut"] if isinstance(e, bmesh.types.BMEdge)]
    fill = bmesh.ops.holes_fill(bm, edges=edges, sides=0)
    for f in fill["faces"]:
        f.material_index = 2
        if len(f.verts) > 4:
            bmesh.ops.triangulate(bm, faces=[f])
    bm.normal_update()
    return obj_from_bm(bm, "Lid" if upper else "BallLow", ["leather", "patch", "interior"])


def prim(kind, name, material, loc=(0, 0, 0), rot=(0, 0, 0), scale=(1, 1, 1), **kw):
    getattr(bpy.ops.mesh, f"primitive_{kind}_add")(location=loc, rotation=rot, **kw)
    o = bpy.context.active_object; o.name = name; o.scale = scale
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    o.data.materials.append(mat(material))
    for p in o.data.polygons:
        p.use_smooth = False
    return o


def join(objs, name):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    if len(objs) > 1:
        bpy.ops.object.join()
    o = bpy.context.active_object; o.name = name
    return o


def set_origin(o, p):
    p = Vector(p)
    for v in o.data.vertices:
        v.co -= p
    o.location = p


def build():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    body = [ball_half(False)]
    # chân đế phát bóng: nón loe 8 cạnh + vành + 3 ngạnh đỡ bóng
    body.append(prim("cone", "tee", "tee", loc=(0, 0, TEE_H / 2), vertices=8, radius1=0.13, radius2=0.075, depth=TEE_H))
    body.append(prim("cylinder", "teebase", "tee_dk", loc=(0, 0, 0.006), vertices=8, radius=0.135, depth=0.012))
    for i in range(3):
        a = math.radians(90 + i * 120)
        body.append(prim("cone", f"prong{i}", "tee", loc=(math.cos(a) * 0.07, math.sin(a) * 0.07, TEE_H + 0.012),
                         vertices=4, radius1=0.016, radius2=0.006, depth=0.03))
    # đường chỉ khâu viền miệng (chỉ ở thân)
    body.append(prim("torus", "rim", "patch", loc=(0, 0, SEAM - 0.004), major_radius=R * 0.997, minor_radius=0.006,
                     major_segments=20, minor_segments=4))
    Body = join(body, "Body")
    Lid = ball_half(True)
    set_origin(Lid, (0, R * 0.98, SEAM))
    # đai thun đỏ quanh đường nối + khóa thép phía trước
    bind = [prim("torus", "strap", "strap", loc=(0, 0, SEAM), major_radius=R + 0.004, minor_radius=0.008,
                 major_segments=20, minor_segments=4, scale=(1, 1, 1.6))]
    bind.append(prim("cube", "buckle", "steel", loc=(0, -R - 0.016, SEAM), size=1, scale=(0.05, 0.012, 0.045)))
    bind.append(prim("cube", "buckle_in", "strap", loc=(0, -R - 0.022, SEAM), size=1, scale=(0.03, 0.006, 0.02)))
    Bind = join(bind, "Binding"); set_origin(Bind, (0, 0, SEAM))
    # huy hiệu dán: ngũ giác vàng trên nền xanh than trên mặt trước nắp
    sd = Vector((0, -math.cos(math.radians(40)), math.sin(math.radians(40))))
    sp = Vector((0, 0, CZ)) + sd * (R + 0.004)
    star = prim("cylinder", "star", "badge", loc=sp, vertices=5, radius=0.034, depth=0.008,
                rot=sd.to_track_quat("Z", "Y").to_euler())
    disc = prim("cylinder", "starbg", "strap", loc=sp - sd * 0.002, vertices=10, radius=0.042, depth=0.006,
                rot=sd.to_track_quat("Z", "Y").to_euler())
    Seal = join([star, disc], "Seal"); set_origin(Seal, sp)
    glow = prim("uv_sphere", "Glow", "glow", loc=(0, 0, SEAM + 0.01), segments=10, ring_count=5, radius=0.07,
                scale=(1, 1, 0.45))
    set_origin(glow, (0, 0, SEAM + 0.01))
    root = bpy.data.objects.new("Chest_TheThao", None); bpy.context.scene.collection.objects.link(root)
    tris = {}
    for o in (Body, Lid, Bind, Seal, glow):
        o.parent = root
        tris[o.name] = sum(len(p.vertices) - 2 for p in o.data.polygons)
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for c in root.children:
        c.select_set(True)
    os.makedirs(os.path.dirname(os.path.abspath(OUT)), exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=OUT, export_format="GLB", use_selection=True, export_apply=True)
    print("tris:", tris, "total (trừ Glow):", sum(v for k, v in tris.items() if k != "Glow"))
    zs = [(o.matrix_world @ v.co).z for o in (Body, Lid) for v in o.data.vertices]
    print("cao (m): %.3f" % max(zs))
    return root


def preview(path):
    from PIL import Image
    sc = bpy.context.scene; OB = bpy.data.objects
    lid, glow = OB["Lid"], OB["Glow"]; g0 = tuple(glow.scale)
    sc.render.engine = "CYCLES"; sc.cycles.device = "CPU"; sc.cycles.samples = SAMPLES; sc.cycles.use_denoising = True
    sc.render.resolution_x = CELL; sc.render.resolution_y = int(CELL * 0.85)
    w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True
    bg = w.node_tree.nodes["Background"]; bg.inputs["Color"].default_value = (0.10, 0.095, 0.13, 1); bg.inputs["Strength"].default_value = 0.35
    bpy.ops.mesh.primitive_plane_add(size=6); fl = bpy.context.active_object
    fm = bpy.data.materials.new("f"); fm.use_nodes = True
    fm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.07, 0.065, 0.085, 1); fl.data.materials.append(fm)

    def light(loc, e, size, col):
        d = bpy.data.lights.new("a", "AREA"); d.energy = e; d.size = size; d.color = col
        o = bpy.data.objects.new("a", d); sc.collection.objects.link(o); o.location = loc
        o.rotation_euler = (Vector((0, 0, 0.2)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
    light((-0.8, -1.0, 1.4), 140, 1.2, (1, .93, .85)); light((1.0, 0.6, 1.0), 40, 1.5, (.7, .8, 1)); light((0, 0.9, 1.2), 30, 1.0, (1, 1, 1))
    pt = bpy.data.lights.new("in", "POINT"); pt.color = (1, .8, .45); pl = bpy.data.objects.new("in", pt)
    sc.collection.objects.link(pl); pl.location = (0, 0, SEAM + 0.05)
    cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera = cam; cam.data.lens = 50
    shots = [("đóng", 0, 0, 24, 18, False), ("hé", 28, 0.5, 24, 18, False), ("mở toang", 105, 1, 24, 30, False),
             ("bóng: trước", 0, 0, 0, 4, True), ("bóng: cạnh", 0, 0, 90, 4, True), ("bóng: 3/4 sau", 0, 0, 145, 12, True)]
    tiles = []
    for i, (lbl, ang, gl, az, el, sil) in enumerate(shots):
        lid.rotation_euler = (math.radians(-ang), 0, 0)
        glow.scale = tuple(s * max(1e-4, gl) for s in g0); glow.hide_render = gl < 0.01; pt.energy = 6 * gl
        for o in sc.objects:
            if o.type == "MESH" and o.name != "Plane":
                o.visible_shadow = True
        if sil:  # hình bóng: vật đen trên nền sáng
            bg.inputs["Color"].default_value = (0.85, 0.85, 0.85, 1); bg.inputs["Strength"].default_value = 1
            fl.hide_render = True
            for o in sc.objects:
                if o.type == "LIGHT":
                    o.hide_render = True
            for n in ("Body", "Lid", "Binding", "Seal"):
                OB[n].active_material  # noqa
            sil_mat = bpy.data.materials.get("sil") or bpy.data.materials.new("sil")
            sil_mat.use_nodes = True; sil_mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0, 0, 0, 1)
            sil_mat.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = 1
            for n in ("Body", "Lid", "Binding", "Seal"):
                o = OB[n]; o["mats"] = [m.name for m in o.data.materials]
                for k in range(len(o.data.materials)):
                    o.data.materials[k] = sil_mat
        a, e = math.radians(az), math.radians(el)
        tgt = Vector((0, 0, 0.2)); dist = 1.35
        cam.location = tgt + Vector((math.sin(a) * math.cos(e), -math.cos(a) * math.cos(e), math.sin(e))) * dist
        cam.rotation_euler = (tgt - cam.location).to_track_quat("-Z", "Y").to_euler()
        f = os.path.join(os.path.dirname(os.path.abspath(path)), f"_tt_{i}.png")
        sc.render.filepath = f; bpy.ops.render.render(write_still=True); tiles.append(f)
    ims = [Image.open(t).convert("RGB") for t in tiles]
    W, H = ims[0].size
    sheet = Image.new("RGB", (W * 3, H * 2))
    for i, im in enumerate(ims):
        sheet.paste(im, ((i % 3) * W, (i // 3) * H))
    sheet.save(path)
    for t in tiles:
        os.remove(t)
    print("preview ->", path)


build()
if PREVIEW:
    preview(PREVIEW)

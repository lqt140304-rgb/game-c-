"""Rương Game (bản tạm): rương hình nấm đỏ chấm trắng, mũ nấm là nắp. Dựng hoàn toàn bằng bpy.

Chạy:  python make_chest_game.py -- <thư mục ra>                (xuất Chest_Game.glb)
       python make_chest_game.py -- <thư mục ra> --render <png> [samples]   (thêm ảnh xem trước Cycles)
Cấu trúc: Chest_Game (root) -> Body, Lid (gốc ở bản lề sau, xoay quanh X, mặt trước -Y), Glow, Seal, Binding.
Đơn vị mét, Blender Z lên (glTF tự đổi sang Y lên).
"""
import math, os, sys
import bpy
from mathutils import Vector

args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else ["."]
OUT = args[0]
RENDER = args[args.index("--render") + 1] if "--render" in args else None
SAMPLES = int(args[args.index("--render") + 2]) if RENDER and len(args) > args.index("--render") + 2 else 48
os.makedirs(OUT, exist_ok=True)
R90 = math.radians(90)
bpy.ops.wm.read_factory_settings(use_empty=True)

PALETTE = {  # sRGB
    "stem": (0.86, 0.80, 0.68), "stem_dk": (0.70, 0.63, 0.52), "cap": (0.72, 0.18, 0.14), "cap_dk": (0.50, 0.12, 0.10),
    "spot": (0.93, 0.90, 0.84), "gill": (0.62, 0.52, 0.42), "eye": (0.08, 0.07, 0.07), "cheek": (0.85, 0.50, 0.45),
    "cart": (0.42, 0.43, 0.45), "cart_lbl": (0.82, 0.74, 0.42), "slot": (0.12, 0.11, 0.11), "btn": (0.30, 0.55, 0.40),
    "grass": (0.30, 0.42, 0.24), "interior": (0.07, 0.055, 0.045), "glow": (1.0, 0.82, 0.45),
}
_mats = {}


def mat(name):
    if name not in _mats:
        m = bpy.data.materials.new(name)
        m.use_nodes = True
        b = m.node_tree.nodes["Principled BSDF"]
        c = tuple(x ** 2.2 for x in PALETTE[name])
        b.inputs["Base Color"].default_value = (*c, 1)
        b.inputs["Roughness"].default_value = 0.85
        if name == "glow":
            b.inputs["Emission Color"].default_value = (*c, 1)
            b.inputs["Emission Strength"].default_value = 2.0
        _mats[name] = m
    return _mats[name]


def finish(o, material):
    bpy.context.view_layer.objects.active = o
    o.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    o.data.materials.clear()
    o.data.materials.append(mat(material))
    for p in o.data.polygons:
        p.use_smooth = False
    o.select_set(False)
    return o


def cone(r1, r2, depth, loc, material, verts=16, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=depth, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def box(size, loc, material, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = size
    return finish(o, material)


def ico(radius, loc, material, scale=(1, 1, 1), sub=1, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, radius=radius, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = scale
    return finish(o, material)


def dome(radius, zscale, loc, material, seg=16, rings=8):
    """Upper half of a UV sphere, squashed: the mushroom cap."""
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=radius, location=loc)
    o = bpy.context.active_object
    o.scale = (1, 1, zscale)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.bisect(plane_co=loc, plane_no=(0, 0, 1), clear_inner=True, use_fill=True)
    bpy.ops.object.mode_set(mode="OBJECT")
    return finish(o, material)


# ------------------------------------------------------------------ build
H = 0.20                    # height of the stem rim (the mouth)
CAP_R, CAP_Z = 0.23, 0.62   # cap radius and vertical squash
parts = {"Body": [], "Lid": [], "Seal": [], "Binding": [], "Glow": []}
add = lambda layer, o: parts[layer].append(o)

# Body: a slightly bulging stem on a grassy tuft
add("Body", cone(0.17, 0.17, 0.025, (0, 0, 0.0125), "grass", verts=10))
for i in range(10):                                                   # tufts of grass
    a = i * math.tau / 10 + 0.2
    add("Body", cone(0.025, 0.0, 0.05, (math.cos(a) * 0.16, math.sin(a) * 0.16, 0.045), "grass", verts=4))
add("Body", cone(0.13, 0.145, 0.10, (0, 0, 0.075), "stem", verts=16))
add("Body", cone(0.145, 0.125, 0.075, (0, 0, 0.1625), "stem", verts=16))
add("Body", cone(0.133, 0.133, 0.008, (0, 0, H - 0.004), "stem_dk", verts=16))   # rim
add("Body", cone(0.118, 0.118, 0.004, (0, 0, H - 0.001), "interior", verts=16))  # dark mouth
# face on the front (-Y)
for sx in (-1, 1):
    add("Body", ico(0.016, (sx * 0.045, -0.140, 0.120), "eye", scale=(0.7, 0.5, 1.4)))
    add("Body", ico(0.016, (sx * 0.078, -0.128, 0.095), "cheek", scale=(1.2, 0.4, 0.7)))
add("Body", box((0.03, 0.01, 0.006), (0, -0.142, 0.088), "eye"))     # small smile
# cartridge slot on the front, below the face
add("Body", box((0.09, 0.02, 0.016), (0, -0.138, 0.045), "slot"))
# D-pad and buttons on the side (left) and right
add("Body", box((0.012, 0.045, 0.014), (-0.142, 0, 0.13), "slot"))
add("Body", box((0.012, 0.014, 0.045), (-0.142, 0, 0.13), "slot"))
for dy, dz in ((-0.018, 0.12), (0.018, 0.14)):
    add("Body", cone(0.012, 0.012, 0.012, (0.142, dy, dz), "btn", verts=8, rot=(0, R90, 0)))

# Lid: the cap with spots and gills underneath
add("Lid", dome(CAP_R, CAP_Z, (0, 0, H + 0.004), "cap"))
add("Lid", cone(CAP_R - 0.004, 0.13, 0.014, (0, 0, H + 0.004 - 0.007), "gill", verts=16))
spots = [(0.0, 0.0, 0.055), (35, 50, 0.04), (110, 48, 0.034), (190, 52, 0.045), (260, 46, 0.03), (320, 60, 0.032),
         (70, 72, 0.026), (150, 75, 0.024), (235, 74, 0.028)]
for az, pol, r in spots:  # azimuth (deg, 0 = +X), polar angle from top (deg)
    a, p = math.radians(az - 90), math.radians(pol)    # -90 so the first ring spot sits towards the front
    n = Vector((math.sin(p) * math.cos(a), math.sin(p) * math.sin(a), math.cos(p) / CAP_Z)).normalized()
    pos = Vector((math.sin(p) * math.cos(a) * CAP_R, math.sin(p) * math.sin(a) * CAP_R, H + 0.004 + math.cos(p) * CAP_R * CAP_Z))
    rot = n.to_track_quat("Z", "Y").to_euler()
    add("Lid", ico(r, pos, "spot", scale=(1, 1, 0.22), rot=rot))
add("Lid", cone(0.012, 0.016, 0.03, (0, -CAP_R + 0.01, H - 0.01), "cap_dk", verts=6))   # catch tab at the front
hinge = (0, 0.133, H)
for sx in (-1, 1):
    add("Body", box((0.03, 0.02, 0.02), (sx * 0.035, 0.138, H - 0.008), "cap_dk"))     # hinge knuckles

# Binding: a game cartridge plugged in the slot (pops out when unsealing)
add("Binding", box((0.08, 0.02, 0.07), (0, -0.140, 0.075), "cart"))
add("Binding", box((0.06, 0.004, 0.035), (0, -0.151, 0.082), "cart_lbl"))
add("Binding", box((0.08, 0.02, 0.008), (0, -0.140, 0.114), "cart"))
# Seal: round sticker with a green "A" button bridging cap tab and stem
add("Seal", cone(0.024, 0.024, 0.006, (0, -0.150, H - 0.014), "spot", verts=10, rot=(R90, 0, 0)))
add("Seal", cone(0.015, 0.015, 0.006, (0, -0.154, H - 0.014), "btn", verts=8, rot=(R90, 0, 0)))
# Glow
add("Glow", ico(0.07, (0, 0, H), "glow", scale=(1, 1, 0.5)))


def join(objs):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    return bpy.context.active_object


def set_origin(o, p):
    bpy.context.scene.cursor.location = p
    bpy.ops.object.select_all(action="DESELECT")
    o.select_set(True)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")


root = bpy.data.objects.new("Chest_Game", None)
bpy.context.scene.collection.objects.link(root)
tris = {}
L = {}
for layer, objs in parts.items():
    o = join(objs)
    o.name = layer
    if layer == "Lid":
        set_origin(o, hinge)
    elif layer != "Body":
        set_origin(o, sum((Vector(c) for c in o.bound_box), Vector()) / 8 + o.location)
    o.parent = root
    tris[layer] = sum(len(p.vertices) - 2 for p in o.data.polygons)
    L[layer] = o
bpy.ops.object.select_all(action="DESELECT")
root.select_set(True)
for c in root.children:
    c.select_set(True)
path = os.path.join(OUT, "Chest_Game.glb")
bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
print("triangles:", tris, "total without glow:", sum(v for k, v in tris.items() if k != "Glow"), "->", path)
zmax = max((L["Lid"].matrix_world @ Vector(c)).z for c in L["Lid"].bound_box)
print("height m:", round(zmax, 3))

# ------------------------------------------------------------------ preview (Cycles)
if RENDER:
    from PIL import Image
    sc = bpy.context.scene
    sc.render.engine = "CYCLES"; sc.cycles.device = "CPU"; sc.cycles.samples = SAMPLES; sc.cycles.use_denoising = True
    sc.render.resolution_x, sc.render.resolution_y = 640, 480
    w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True
    w.node_tree.nodes["Background"].inputs["Color"].default_value = (0.10, 0.095, 0.13, 1)
    w.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.35
    bpy.ops.mesh.primitive_plane_add(size=6, location=(0, 0, 0))
    floor = bpy.context.active_object
    fm = bpy.data.materials.new("f"); fm.use_nodes = True
    fm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.07, 0.065, 0.085, 1)
    bpy.context.active_object.data.materials.append(fm)

    def light(loc, energy, size, color=(1, 1, 1)):
        d = bpy.data.lights.new("a", "AREA"); d.energy = energy; d.size = size; d.color = color
        o = bpy.data.objects.new("a", d); sc.collection.objects.link(o); o.location = loc
        o.rotation_euler = (Vector((0, 0, 0.2)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
    light((-0.8, -1.0, 1.4), 140, 1.2, (1, .93, .85)); light((1.0, 0.6, 1.0), 40, 1.5, (.7, .8, 1)); light((0, 0.9, 1.2), 30, 1.0)
    pt = bpy.data.lights.new("in", "POINT"); pt.color = (1, .8, .45); pt.shadow_soft_size = 0.03
    po = bpy.data.objects.new("in", pt); sc.collection.objects.link(po); po.location = (0, 0, H + 0.04)
    cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera = cam
    cam.data.lens = 50
    glow, lid, gs0 = L["Glow"], L["Lid"], tuple(L["Glow"].scale)
    lid.rotation_mode = "XYZ"
    tiles = []

    def shot(name, lid_deg, glw, az, elev=0.42, dist=1.25, silhouette=False):
        lid.rotation_euler = (math.radians(-lid_deg), 0, 0)   # negative X turns the front of the cap up and back
        glow.scale = tuple(s * max(1e-4, glw) for s in gs0); glow.hide_render = glw < 0.01
        pt.energy = 4 * glw
        a = math.radians(az); tgt = Vector((0, 0, 0.18))
        cam.location = tgt + Vector((math.sin(a) * dist, -math.cos(a) * dist, elev * dist))
        cam.rotation_euler = (tgt - cam.location).to_track_quat("-Z", "Y").to_euler()
        floor.hide_render = silhouette; sc.render.film_transparent = silhouette
        f = os.path.join(OUT, f"_tile_{name}.png"); sc.render.filepath = f
        bpy.ops.render.render(write_still=True)
        im = Image.open(f).convert("RGB")
        if silhouette:   # threshold against the dark background -> black shape on white
            a = Image.open(f).getchannel("A"); im = a.point(lambda v: 0 if v > 128 else 235).convert("RGB")
        tiles.append(im); os.remove(f)

    shot("closed", 0, 0, 28)
    shot("ajar", 25, 0.5, 28)
    shot("open", 100, 1.0, 28)
    for i, az in enumerate((0, 90, 45)):
        shot(f"sil{i}", 0, 0, az, elev=0.05, dist=1.4, silhouette=True)
    sheet = Image.new("RGB", (640 * 3, 480 * 2))
    for i, t in enumerate(tiles):
        sheet.paste(t, ((i % 3) * 640, (i // 3) * 480))
    sheet.save(RENDER)
    print("preview ->", RENDER)

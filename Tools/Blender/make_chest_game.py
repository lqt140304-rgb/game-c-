"""Rương Game (bản tạm): tay cầm game khổng lồ dựng nghiêng trên chân đế, nửa vỏ trên là nắp. Dựng hoàn toàn bằng bpy.

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
    "shell": (0.62, 0.62, 0.64), "shell_lt": (0.78, 0.77, 0.76), "shell_dk": (0.28, 0.29, 0.31),
    "btn_r": (0.72, 0.22, 0.18), "btn_y": (0.85, 0.70, 0.25), "btn_b": (0.25, 0.42, 0.65),
    "stem": (0.86, 0.80, 0.68), "stem_dk": (0.70, 0.63, 0.52), "cap": (0.72, 0.18, 0.14), "cap_dk": (0.50, 0.12, 0.10),
    "spot": (0.93, 0.90, 0.84), "gill": (0.62, 0.52, 0.42), "eye": (0.08, 0.07, 0.07), "cheek": (0.85, 0.50, 0.45),
    "cart": (0.42, 0.43, 0.45), "cart_lbl": (0.82, 0.74, 0.42), "slot": (0.12, 0.11, 0.11), "btn_g": (0.30, 0.55, 0.40),
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
# The controller is modelled lying flat (face up, grips towards -Y, seam at z = SEAM, hinge on the back edge),
# then every layer is tilted by TILT about X (back edge up) and lifted onto a kickstand.
import bmesh
from mathutils import Matrix
SEAM, TOP, TILT = 0.05, 0.095, math.radians(52)
parts = {"Body": [], "Lid": [], "Seal": [], "Binding": [], "Glow": []}
add = lambda layer, o: parts[layer].append(o)


def outline():
    """Top view of the pad: rounded centre plate (+ two grips added separately)."""
    pts, w, d, r = [], 0.17, 0.095, 0.05
    for cx, cy, a0 in ((w - r, d - r, 0), (-w + r, d - r, 90), (-w + r, -d + r, 180), (w - r, -d + r, 270)):
        for i in range(5):
            a = math.radians(a0 + i * 22.5)
            pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return pts


def prism(pts, z0, z1, material):
    me = bpy.data.meshes.new("p"); bm = bmesh.new()
    f = bm.faces.new([bm.verts.new((x, y, z0)) for x, y in pts])
    bmesh.ops.reverse_faces(bm, faces=[f])
    ext = bmesh.ops.extrude_face_region(bm, geom=[f])
    bmesh.ops.translate(bm, vec=(0, 0, z1 - z0), verts=[v for v in ext["geom"] if isinstance(v, bmesh.types.BMVert)])
    bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new("p", me); bpy.context.scene.collection.objects.link(o)
    return finish(o, material)


def shell(z0, z1, material, inset_z):
    out = [prism(outline(), z0, z1, material)]
    for sx in (-1, 1):  # grips: squashed 10-sided cylinders angled outwards
        out.append(cone(0.075, 0.075, z1 - z0, (sx * 0.15, -0.09, (z0 + z1) / 2), material, verts=10))
        o = out[-1]; o.data.transform(Matrix.Translation((sx * 0.15, -0.09, 0)) @ Matrix.Rotation(sx * 0.35, 4, "Z")
                                      @ Matrix.Scale(1.35, 4, (0, 1, 0)) @ Matrix.Translation((-sx * 0.15, 0.09, 0)))
    out.append(prism([(x * 0.86, y * 0.8) for x, y in outline()], inset_z - 0.002, inset_z + 0.002, "interior"))
    return out


for o in shell(0.0, SEAM, "shell", SEAM): add("Body", o)
for o in shell(SEAM, TOP, "shell_lt", SEAM): add("Lid", o)
# face of the lid: D-pad, four face buttons, start/select, two sticks
add("Lid", box((0.075, 0.024, 0.014), (-0.14, -0.005, TOP + 0.007), "slot"))
add("Lid", box((0.024, 0.075, 0.014), (-0.14, -0.005, TOP + 0.007), "slot"))
for (dx, dy), m in zip(((0, 0.032), (0.032, 0), (0, -0.032), (-0.032, 0)), ("btn_y", "btn_r", "btn_g", "btn_b")):
    add("Lid", cone(0.016, 0.014, 0.016, (0.14 + dx, -0.005 + dy, TOP + 0.008), m, verts=8))
for sx in (-1, 1):
    add("Lid", box((0.03, 0.011, 0.008), (sx * 0.03, 0.035, TOP + 0.004), "slot", rot=(0, 0, sx * 0.3)))
    add("Lid", cone(0.03, 0.026, 0.012, (sx * 0.065, -0.075, TOP + 0.006), "slot", verts=10))
    add("Lid", cone(0.022, 0.024, 0.022, (sx * 0.065, -0.075, TOP + 0.022), "shell_dk", verts=10))
add("Lid", cone(0.012, 0.012, 0.004, (0, -0.02, TOP + 0.002), "btn_r", verts=8))         # home light
# body: shoulder bumpers, cable port, hinge knuckles at the back edge
for sx in (-1, 1):
    add("Body", box((0.09, 0.03, 0.035), (sx * 0.11, 0.098, SEAM - 0.02), "shell_dk", rot=(0, 0, -sx * 0.12)))
add("Body", box((0.05, 0.02, 0.02), (0, 0.1, SEAM - 0.015), "slot"))
hinge = Vector((0, 0.095, SEAM))
# Binding: a clasp across the front seam; Seal: round sticker on it
add("Binding", box((0.05, 0.02, 0.06), (0, -0.1, SEAM), "shell_dk"))
add("Seal", cone(0.022, 0.022, 0.006, (0, -0.112, SEAM), "spot", verts=10, rot=(R90, 0, 0)))
add("Seal", box((0.022, 0.004, 0.008), (0, -0.116, SEAM), "btn_r"))
add("Glow", ico(0.08, (0, 0, SEAM + 0.01), "glow", scale=(1.6, 0.9, 0.35)))

# tilt everything, then sit it on the floor with a kickstand under the raised back
M = Matrix.Rotation(TILT, 4, "X")
for objs in parts.values():
    for o in objs: o.data.transform(M)
lift = -min((v.co.z for objs in parts.values() for o in objs for v in o.data.vertices)) + 0.025
T = Matrix.Translation((0, 0.03, lift))
for objs in parts.values():
    for o in objs: o.data.transform(T)
hinge = T @ M @ hinge
add("Body", cone(0.2, 0.19, 0.025, (0, 0.03, 0.0125), "shell_dk", verts=12))               # round base
for sx in (-1, 1):                                                                         # kickstand struts
    top = T @ M @ Vector((sx * 0.1, 0.04, 0.005))
    bot = Vector((sx * 0.1, 0.03 + 0.12, 0.02))
    mid, v = (top + bot) / 2, top - bot
    add("Body", box((0.03, 0.025, v.length), mid, "shell_dk", rot=v.to_track_quat("Z", "Y").to_euler()))


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
    po = bpy.data.objects.new("in", pt); sc.collection.objects.link(po); po.location = hinge + Vector((0, -0.12, 0.08))
    cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera = cam
    cam.data.lens = 50
    glow, lid, gs0 = L["Glow"], L["Lid"], tuple(L["Glow"].scale)
    lid.rotation_mode = "XYZ"
    tiles = []

    def shot(name, lid_deg, glw, az, elev=0.42, dist=1.25, silhouette=False):
        lid.rotation_euler = (math.radians(-lid_deg), 0, 0)   # negative X turns the front of the cap up and back
        glow.scale = tuple(s * max(1e-4, glw) for s in gs0); glow.hide_render = glw < 0.01
        pt.energy = 4 * glw
        a = math.radians(az); tgt = Vector((0, 0, 0.16))
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
    shot("open", 85, 1.0, 28)
    for i, az in enumerate((0, 90, 45)):
        shot(f"sil{i}", 0, 0, az, elev=0.05, dist=1.4, silhouette=True)
    sheet = Image.new("RGB", (640 * 3, 480 * 2))
    for i, t in enumerate(tiles):
        sheet.paste(t, ((i % 3) * 640, (i // 3) * 480))
    sheet.save(RENDER)
    print("preview ->", RENDER)

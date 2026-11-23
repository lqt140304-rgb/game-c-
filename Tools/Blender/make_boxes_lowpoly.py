"""Low-poly neutral mystery boxes (H1 H2 H3 RD), built procedurally with Blender (bpy). Run: python make_boxes_lowpoly.py -- <output dir>. Output: one GLB per box + a preview render (needs a GPU; skip if headless)."""
import math, random, sys, os
import bpy

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "."
os.makedirs(OUT, exist_ok=True)

PALETTE = {
    "wood":      (0.50, 0.33, 0.19),
    "wood_dark": (0.33, 0.21, 0.12),
    "rope":      (0.72, 0.62, 0.44),
    "seal":      (0.52, 0.20, 0.15),
    "slate":     (0.30, 0.34, 0.39),
    "slate_dk":  (0.19, 0.22, 0.26),
    "steel":     (0.48, 0.51, 0.55),
    "canvas":    (0.74, 0.67, 0.52),
    "cord":      (0.28, 0.20, 0.14),
    "chest":     (0.30, 0.20, 0.15),
    "brass":     (0.62, 0.52, 0.22),
}
_mats = {}


def mat(name):
    if name in _mats:
        return _mats[name]
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes["Principled BSDF"]
    c = tuple(x ** 2.2 for x in PALETTE[name])   # palette is written in sRGB; Blender and glTF expect linear
    bsdf.inputs["Base Color"].default_value = (c[0], c[1], c[2], 1.0)
    bsdf.inputs["Roughness"].default_value = 0.9
    bsdf.inputs["Metallic"].default_value = 0.0
    m.diffuse_color = (c[0], c[1], c[2], 1.0)
    _mats[name] = m
    return m


def finish(obj, material):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    obj.data.materials.clear()
    obj.data.materials.append(mat(material))
    for p in obj.data.polygons:
        p.use_smooth = False
    obj.select_set(False)
    return obj


def box(size, loc, material, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = size
    return finish(o, material)


def cyl(radius, depth, loc, material, verts=8, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def ico(radius, loc, material):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=radius, location=loc)
    return finish(bpy.context.active_object, material)


def torus(major, minor, loc, material, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, major_segments=8, minor_segments=4, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def jitter(obj, amount, seed):
    random.seed(seed)
    for v in obj.data.vertices:
        v.co.x += random.uniform(-amount, amount)
        v.co.y += random.uniform(-amount, amount)
        v.co.z += random.uniform(-amount, amount)


def seal(loc, rot=(0, 0, 0)):
    return cyl(0.022, 0.01, loc, "seal", verts=8, rot=rot)


# ---------------------------------------------------------------- the four boxes (bottom centre at the origin)
def build_h1():
    w, d, h = 0.30, 0.30, 0.24
    parts = [box((w, d, h), (0, 0, h / 2), "wood")]
    parts.append(box((w + 0.02, d + 0.02, 0.035), (0, 0, h + 0.012), "wood_dark"))           # lid
    for sx in (-1, 1):
        for sy in (-1, 1):
            parts.append(box((0.04, 0.04, h + 0.03), (sx * (w / 2), sy * (d / 2), (h + 0.03) / 2), "wood_dark"))  # corner posts
    parts.append(box((0.02, d + 0.03, 0.03), (0, 0, h * 0.45), "wood_dark"))                    # plank band
    parts.append(box((w + 0.03, 0.02, 0.03), (0, 0, h * 0.2), "wood_dark"))
    # braced front plank (diagonal)
    parts.append(box((0.38, 0.016, 0.03), (0, -d / 2 - 0.004, h * 0.5), "wood_dark", rot=(0, math.radians(38), 0)))
    # rope handle on the side
    parts.append(torus(0.04, 0.007, (-w / 2 - 0.012, 0, h * 0.62), "rope", rot=(0, math.radians(90), 0)))
    parts.append(seal((0, 0, h + 0.035)))
    return "H1", parts


def build_h2():
    w, d, h = 0.34, 0.26, 0.22
    parts = [box((w, d, h), (0, 0, h / 2), "slate")]
    parts.append(box((w + 0.012, d + 0.012, 0.05), (0, 0, h + 0.015), "slate_dk"))              # lid
    for sx in (-1, 1):                                                                           # strap bands
        parts.append(box((0.03, d + 0.02, h + 0.02), (sx * 0.11, 0, (h + 0.02) / 2), "steel"))
    parts.append(box((0.06, 0.02, 0.07), (0, -d / 2 - 0.01, h * 0.82), "steel"))                # latch
    parts.append(box((0.03, 0.022, 0.03), (0, -d / 2 - 0.012, h * 0.65), "slate_dk"))           # keyhole plate
    for sx in (-1, 1):
        for sy in (-1, 1):
            parts.append(ico(0.012, (sx * (w / 2 - 0.02), sy * (d / 2 + 0.002), h * 0.18), "steel"))
            parts.append(ico(0.012, (sx * (w / 2 - 0.02), sy * (d / 2 + 0.002), h * 0.82), "steel"))
    parts.append(seal((0.0, -d / 2 - 0.016, h * 0.46), rot=(math.radians(90), 0, 0)))
    return "H2", parts


def build_h3():
    w, d, h = 0.30, 0.30, 0.20
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, h / 2))
    body = bpy.context.active_object
    body.scale = (w, d, h)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.subdivide(number_cuts=2)
    bpy.ops.object.mode_set(mode="OBJECT")
    jitter(body, 0.012, 3)
    finish(body, "canvas")
    parts = [body]
    parts.append(box((w + 0.03, 0.03, h + 0.03), (0, 0, h / 2), "cord"))                         # cord across (front-back)
    parts.append(box((0.03, d + 0.03, h + 0.03), (0, 0, h / 2), "cord"))                         # cord across (left-right)
    parts.append(box((0.07, 0.018, 0.018), (0.032, 0, h + 0.02), "cord", rot=(0, 0, math.radians(30))))   # bow loops
    parts.append(box((0.07, 0.018, 0.018), (-0.032, 0, h + 0.02), "cord", rot=(0, 0, math.radians(-30))))
    parts.append(seal((0, 0, h + 0.012)))
    return "H3", parts


def build_rd():
    w, d, h = 0.38, 0.26, 0.17
    parts = [box((w, d, h), (0, 0, h / 2), "chest")]
    parts.append(cyl(d / 2, w, (0, 0, h), "chest", verts=10, rot=(0, math.radians(90), 0)))     # barrel lid along X
    for x in (-0.14, 0.0, 0.14):                                                                  # brass bands over the lid
        parts.append(cyl(d / 2 + 0.008, 0.03, (x, 0, h), "brass", verts=10, rot=(0, math.radians(90), 0)))
        parts.append(box((0.03, d + 0.012, h), (x, 0, h / 2), "brass"))
    for sx in (-1, 1):                                                                            # corner straps (not full end plates)
        for sy in (-1, 1):
            parts.append(box((0.03, 0.03, h + 0.012), (sx * (w / 2), sy * (d / 2 - 0.005), (h + 0.012) / 2), "brass"))
    parts.append(box((0.055, 0.03, 0.05), (0, -d / 2 - 0.018, h * 0.78), "brass"))               # padlock body
    parts.append(torus(0.016, 0.005, (0, -d / 2 - 0.018, h * 0.78 + 0.034), "brass", rot=(math.radians(90), 0, 0)))
    parts.append(seal((0.075, -d / 2 - 0.012, h * 0.55), rot=(math.radians(90), 0, 0)))
    return "RD", parts


def join(parts):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    return bpy.context.active_object


def lowest_z_to_zero(obj):
    zs = [v.co.z for v in obj.data.vertices]
    for v in obj.data.vertices:
        v.co.z -= min(zs)


# ---------------------------------------------------------------- build, export, arrange for preview
bpy.ops.wm.read_factory_settings(use_empty=True)
results = []
for builder in (build_h1, build_h2, build_h3, build_rd):
    name, parts = builder()
    obj = join(parts)
    obj.name = name
    lowest_z_to_zero(obj)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    path = os.path.join(OUT, f"{name}.glb")
    bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
    tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    print(name, "triangles:", tris, "->", path)
    results.append(obj)

for i, obj in enumerate(results):
    obj.location = ((i % 2) * 0.6 - 0.3, -(i // 2) * 0.55 + 0.25, 0)

# ---------------------------------------------------------------- preview render (Workbench: fast, no GPU)
scene = bpy.context.scene
scene.render.engine = "BLENDER_WORKBENCH"
scene.render.resolution_x, scene.render.resolution_y = 1100, 1100
sh = scene.display.shading
sh.light = "STUDIO"
sh.color_type = "MATERIAL"
sh.show_cavity = False
sh.show_object_outline = False
scene.display.render_aa = "8"
scene.world = bpy.data.worlds.new("w")
scene.world.color = (0.93, 0.93, 0.93)
cam_data = bpy.data.cameras.new("cam")
cam = bpy.data.objects.new("cam", cam_data)
scene.collection.objects.link(cam)
cam.location = (0.0, -1.55, 1.15)
direction = (0.0, 0.0, 0.1)
import mathutils
d = mathutils.Vector(direction) - cam.location
cam.rotation_euler = d.to_track_quat("-Z", "Y").to_euler()
cam_data.lens = 50
scene.camera = cam
scene.render.filepath = os.path.join(OUT, "preview.png")
bpy.ops.render.render(write_still=True)
print("preview ->", scene.render.filepath)

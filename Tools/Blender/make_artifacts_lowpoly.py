"""Low-poly neutral artifacts (CP1 CP2 CP3 TC1 TC2 TC3), built procedurally with Blender (bpy).

Run: python make_artifacts_lowpoly.py -- <output dir>
One GLB per artifact. Bottom centre at the origin, metres, Blender Z up (glTF export converts to Y up).
Parts that should glow in game are a separate child object named "Glow" (emissive material).
"""
import math, random, sys, os
import bpy, mathutils

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "."
os.makedirs(OUT, exist_ok=True)
R90 = math.radians(90)

PALETTE = {  # sRGB, converted to linear in mat()
    "bamboo": (0.72, 0.58, 0.34), "bamboo_dk": (0.52, 0.40, 0.22), "ink": (0.12, 0.10, 0.09), "silk": (0.55, 0.16, 0.13),
    "clay": (0.47, 0.42, 0.33), "clay_dk": (0.36, 0.32, 0.25), "cloth": (0.80, 0.75, 0.63), "hemp": (0.62, 0.52, 0.35),
    "bronze": (0.38, 0.44, 0.36), "bronze_dk": (0.27, 0.32, 0.26), "bronze_hi": (0.55, 0.50, 0.33),
    "steel": (0.62, 0.64, 0.66), "steel_dk": (0.40, 0.42, 0.45), "gunmetal": (0.27, 0.29, 0.32),
    "glow": (0.45, 0.85, 0.85), "glass": (0.70, 0.86, 0.86),
}
_mats = {}


def mat(name):
    if name in _mats:
        return _mats[name]
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes["Principled BSDF"]
    c = tuple(x ** 2.2 for x in PALETTE[name])
    bsdf.inputs["Base Color"].default_value = (c[0], c[1], c[2], 1.0)
    bsdf.inputs["Roughness"].default_value = 0.85
    if name == "glow":
        bsdf.inputs["Emission Color"].default_value = (c[0], c[1], c[2], 1.0)
        bsdf.inputs["Emission Strength"].default_value = 2.5
    _mats[name] = m
    return m


def finish(obj, material):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
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


def cyl(radius, depth, loc, material, verts=8, rot=(0, 0, 0), r2=None):
    if r2 is None:
        bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=loc, rotation=rot)
    else:
        bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=radius, radius2=r2, depth=depth, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def ico(radius, loc, material, sub=1, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, radius=radius, location=loc)
    o = bpy.context.active_object
    o.scale = scale
    return finish(o, material)


def uvs(radius, loc, material, seg=10, rings=6, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=radius, location=loc)
    o = bpy.context.active_object
    o.scale = scale
    return finish(o, material)


def torus(major, minor, loc, material, rot=(0, 0, 0), seg=10):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, major_segments=seg, minor_segments=4, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


# ------------------------------------------------------------------------------------------ the six artifacts
def build_cp1():   # Phiến khắc tre: a rolled bundle of bamboo slips tied with a silk cord, lying on its side
    parts, glow = [], []
    L, r = 0.22, 0.055                      # length along X, roll radius
    n = 14
    for i in range(n):                      # slips arranged in a spiral-ish ring
        a = i / n * 2 * math.pi
        rr = r * (0.75 + 0.25 * (i % 3) / 2)
        y, z = math.cos(a) * rr, math.sin(a) * rr + r + 0.004
        parts.append(box((L, 0.022, 0.008), (0, y, z), "bamboo" if i % 2 else "bamboo_dk", rot=(a + R90, 0, 0)))
    parts.append(cyl(r * 0.6, L - 0.004, (0, 0, r + 0.004), "bamboo_dk", verts=8, rot=(0, R90, 0)))   # inner core
    for x in (-0.05, 0.05):                 # silk cord wraps
        parts.append(torus(r + 0.006, 0.005, (x, 0, r + 0.004), "silk", rot=(0, R90, 0), seg=12))
    parts.append(ico(0.012, (0.05, -0.0, 2 * r + 0.01), "silk"))                                    # knot
    parts.append(box((0.008, 0.006, 0.05), (0.058, -0.01, 2 * r - 0.012), "silk", rot=(math.radians(20), 0, 0)))
    for i in range(5):                      # carved characters (dark marks) on the outer slip
        parts.append(box((0.012, 0.003, 0.012), (-0.08 + i * 0.03, -r - 0.004, r + 0.004), "ink"))
    return "CP1", parts, glow


def build_cp2():   # Hũ đựng đồ: round glazed storage jar with a cloth-wrapped lid tied with hemp rope
    parts, glow = [], []
    parts.append(cyl(0.06, 0.02, (0, 0, 0.01), "clay_dk", verts=12))                                  # foot
    parts.append(uvs(0.11, (0, 0, 0.11), "clay", seg=14, rings=8, scale=(1, 1, 0.95)))                # belly
    parts.append(cyl(0.085, 0.03, (0, 0, 0.17), "clay", verts=14, r2=0.065))                          # shoulder
    parts.append(cyl(0.066, 0.025, (0, 0, 0.196), "clay_dk", verts=14))                               # neck
    for i in range(3):                       # incised glaze rings
        parts.append(torus(0.11 * math.cos(math.radians(-15 + i * 15)) + 0.001, 0.003, (0, 0, 0.11 + 0.11 * math.sin(math.radians(-15 + i * 15)) * 0.95), "clay_dk", seg=14))
    parts.append(cyl(0.078, 0.03, (0, 0, 0.222), "cloth", verts=12, r2=0.07))                         # cloth cap
    random.seed(4)
    for i in range(8):                       # cloth flaps hanging over the neck
        a = i / 8 * 2 * math.pi
        parts.append(box((0.05, 0.006, 0.05), (math.cos(a) * 0.075, math.sin(a) * 0.075, 0.2), "cloth", rot=(0, math.radians(random.uniform(-12, 12)), a + R90)))
    parts.append(torus(0.071, 0.006, (0, 0, 0.205), "hemp", seg=12))                                  # rope
    parts.append(ico(0.012, (0.07, -0.03, 0.205), "hemp"))
    for s in (-1, 1):
        parts.append(box((0.008, 0.008, 0.055), (0.075 + 0.008 * s, -0.035, 0.178), "hemp", rot=(0, math.radians(12 * s), 0)))
    return "CP2", parts, glow


def build_cp3():   # Ấn triện đồng: bronze seal stamp with a guardian lion on a square base
    parts, glow = [], []
    parts.append(box((0.13, 0.13, 0.08), (0, 0, 0.04), "bronze"))                                     # base
    parts.append(box((0.14, 0.14, 0.012), (0, 0, 0.006), "bronze_dk"))                                # stamp face
    parts.append(box((0.135, 0.135, 0.01), (0, 0, 0.085), "bronze_dk"))                               # top rim
    # blocky guardian lion, sitting, facing -Y
    parts.append(box((0.07, 0.08, 0.06), (0, 0.01, 0.12), "bronze"))                                  # haunches
    parts.append(box((0.06, 0.05, 0.08), (0, -0.015, 0.16), "bronze", rot=(math.radians(-12), 0, 0)))  # chest
    for sx in (-1, 1):
        parts.append(box((0.018, 0.02, 0.07), (sx * 0.022, -0.04, 0.125), "bronze_dk"))               # front legs
        parts.append(ico(0.013, (sx * 0.022, -0.05, 0.095), "bronze_hi"))                              # paws
    parts.append(ico(0.045, (0, -0.02, 0.215), "bronze_dk", sub=1))                                    # mane
    parts.append(box((0.05, 0.045, 0.045), (0, -0.045, 0.215), "bronze"))                             # face
    parts.append(box((0.03, 0.02, 0.018), (0, -0.07, 0.205), "bronze_hi"))                            # muzzle
    for sx in (-1, 1):
        parts.append(ico(0.008, (sx * 0.013, -0.068, 0.228), "bronze_hi"))                            # eyes
        parts.append(ico(0.012, (sx * 0.025, -0.02, 0.255), "bronze_dk"))                             # ears / curls
    for i in range(4):                       # curly mane bumps
        a = i / 4 * math.pi - R90
        parts.append(ico(0.014, (math.cos(a) * 0.04, 0.0 + math.sin(a) * 0.01, 0.2 + 0.02 * (i % 2)), "bronze_dk"))
    parts.append(box((0.012, 0.05, 0.012), (0, 0.055, 0.14), "bronze", rot=(math.radians(-40), 0, 0)))  # tail
    parts.append(ico(0.015, (0, 0.07, 0.165), "bronze_dk"))
    for i in range(4):                       # engraved pattern on the base sides
        parts.append(box((0.025, 0.004, 0.025), (-0.04 + i * 0.027, -0.067, 0.045), "bronze_dk", rot=(0, math.radians(45), 0)))
    return "CP3", parts, glow


def build_tc1():   # Ốc vít dự phòng: oversized hex bolt with a nut threaded on, lying on its side
    parts, glow = [], []
    L, r = 0.16, 0.022
    z = 0.04
    parts.append(cyl(0.04, 0.03, (-L / 2, 0, z), "steel", verts=6, rot=(0, R90, 0)))                 # hex head
    parts.append(cyl(0.032, 0.006, (-L / 2 + 0.018, 0, z), "steel_dk", verts=6, rot=(0, R90, 0)))    # washer face
    parts.append(cyl(r, L, (0.0, 0, z), "steel_dk", verts=10, rot=(0, R90, 0)))                      # shank
    for i in range(9):                       # threads
        parts.append(torus(r + 0.001, 0.0035, (0.0 + i * 0.009, 0, z), "steel", rot=(0, R90, 0), seg=10))
    parts.append(cyl(0.036, 0.024, (0.03, 0, z), "steel", verts=6, rot=(0, R90, 0)))                  # nut
    parts.append(cyl(0.012, 0.012, (L / 2 + 0.004, 0, z), "steel", verts=8, rot=(0, R90, 0)))        # chamfered tip
    parts.append(box((0.012, 0.045, 0.006), (-L / 2 - 0.016, 0, z + 0.0), "gunmetal", rot=(R90, 0, 0)))  # slot mark on head
    return "TC1", parts, glow


def build_tc2():   # Mắt máy soi: spherical scanner eye in a segmented shell with a glowing iris
    parts, glow = [], []
    R = 0.08
    c = (0, 0, R + 0.02)
    parts.append(cyl(0.05, 0.02, (0, 0, 0.01), "gunmetal", verts=8))                                  # stand
    parts.append(uvs(R, c, "steel_dk", seg=12, rings=8))                                              # shell
    for dz in (-0.05, 0.05):                  # segment seams: horizontal belts above and below the lens
        parts.append(torus(math.sqrt(R * R - dz * dz) + 0.002, 0.004, (0, 0, c[2] + dz), "gunmetal", seg=14))
    parts.append(torus(R * 0.62, 0.012, (0, -R * 0.78, c[2]), "steel", rot=(R90, 0, 0), seg=12))     # lens ring
    parts.append(cyl(R * 0.55, 0.01, (0, -R * 0.8, c[2]), "gunmetal", verts=12, rot=(R90, 0, 0)))    # lens backing
    glow.append(cyl(R * 0.42, 0.012, (0, -R * 0.85, c[2]), "glow", verts=12, rot=(R90, 0, 0)))        # iris
    parts.append(cyl(R * 0.16, 0.014, (0, -R * 0.87, c[2]), "gunmetal", verts=8, rot=(R90, 0, 0)))   # pupil
    for sx in (-1, 1):                        # side clamps
        parts.append(box((0.02, 0.035, 0.05), (sx * (R + 0.006), 0, c[2]), "steel"))
    return "TC2", parts, glow


def build_tc3():   # Lõi năng lượng: capsule with a glowing plasma tube between two machined caps, lying on its side
    parts, glow = [], []
    L, r = 0.2, 0.035
    z = r + 0.012
    for sx in (-1, 1):
        parts.append(cyl(r + 0.014, 0.04, (sx * (L / 2 - 0.02), 0, z), "gunmetal", verts=10, rot=(0, R90, 0)))
        parts.append(torus(r + 0.012, 0.004, (sx * (L / 2 - 0.035), 0, z), "steel", rot=(0, R90, 0), seg=10))
        parts.append(cyl(r * 0.55, 0.012, (sx * (L / 2 + 0.003), 0, z), "steel", verts=8, rot=(0, R90, 0)))
        for i in range(4):                     # bolts around the cap
            a = i / 4 * 2 * math.pi + math.pi / 4
            parts.append(ico(0.005, (sx * (L / 2 + 0.004), math.cos(a) * r * 0.85, z + math.sin(a) * r * 0.85), "steel_dk"))
    parts.append(cyl(r, L - 0.08, (0, 0, z), "glass", verts=10, rot=(0, R90, 0)))                    # glass sleeve
    glow.append(cyl(r * 0.55, L - 0.08, (0, 0, z), "glow", verts=8, rot=(0, R90, 0)))                 # plasma
    for sy in (-1, 1):                        # support rails
        parts.append(box((L - 0.08, 0.008, 0.008), (0, sy * (r + 0.002), z - r * 0.4), "steel_dk"))
    return "TC3", parts, glow


# ------------------------------------------------------------------------------------------ assemble and export
def join(parts):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    return bpy.context.active_object


def clear_scene():
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    for m in list(bpy.data.meshes):
        bpy.data.meshes.remove(m)


bpy.ops.wm.read_factory_settings(use_empty=True)
for builder in (build_cp1, build_cp2, build_cp3, build_tc1, build_tc2, build_tc3):
    name, parts, glow = builder()
    root = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(root)
    body = join(parts)
    body.name = "Body"
    minz = min(v.co.z for v in body.data.vertices)
    for v in body.data.vertices:
        v.co.z -= minz
    body.parent = root
    tris = sum(len(p.vertices) - 2 for p in body.data.polygons)
    if glow:
        g = join(glow)
        g.name = "Glow"
        for v in g.data.vertices:
            v.co.z -= minz
        g.parent = root
        tris += sum(len(p.vertices) - 2 for p in g.data.polygons)
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for c in root.children:
        c.select_set(True)
    bpy.context.view_layer.objects.active = root
    path = os.path.join(OUT, f"{name}.glb")
    bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
    print(name, "triangles:", tris, "->", path)
    clear_scene()

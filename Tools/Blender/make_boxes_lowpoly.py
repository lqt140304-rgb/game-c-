"""Detailed low-poly neutral mystery boxes (H1 H2 H3 RD), built procedurally with Blender (bpy).

Run: python make_boxes_lowpoly.py -- <output dir>
Each box is exported as one GLB with separate layers so it can be animated:
  <ID> (root) -> Body, Lid (origin on the hinge), Seal, Binding (cord / latch / lock), Glow (light inside the box)
Bottom centre of the box is at the origin, metres, Blender Z up (glTF export converts to Y up).
"""
import bpy, mathutils

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "."
os.makedirs(OUT, exist_ok=True)
R90 = math.radians(90)

PALETTE = {  # written in sRGB, converted to linear below
    "wood": (0.50, 0.33, 0.19), "wood_light": (0.58, 0.40, 0.23), "wood_dark": (0.33, 0.21, 0.12),
    "rope": (0.72, 0.62, 0.44), "seal": (0.52, 0.20, 0.15), "paper": (0.82, 0.76, 0.60),
    "slate": (0.30, 0.34, 0.39), "slate_dk": (0.19, 0.22, 0.26), "steel": (0.48, 0.51, 0.55), "iron": (0.22, 0.24, 0.27),
    "canvas": (0.74, 0.67, 0.52), "canvas_dk": (0.58, 0.50, 0.37), "cord": (0.28, 0.20, 0.14),
    "chest": (0.30, 0.20, 0.15), "chest_light": (0.38, 0.26, 0.19), "brass": (0.62, 0.52, 0.22), "keyhole": (0.07, 0.06, 0.06),
    "glow": (1.0, 0.82, 0.45), "interior": (0.07, 0.055, 0.045),
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
    bsdf.inputs["Roughness"].default_value = 0.9
    bsdf.inputs["Metallic"].default_value = 0.0
    if name == "glow":
        bsdf.inputs["Emission Color"].default_value = (c[0], c[1], c[2], 1.0)
        bsdf.inputs["Emission Strength"].default_value = 2.0
    _mats[name] = m
    return m


def finish(obj, material):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)   # mesh now lives in world coordinates
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


def half_cyl(radius, depth, loc, material, verts=12):
    """Upper half of a cylinder lying along X (a barrel lid): cut off everything below the axis."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=loc, rotation=(0, R90, 0))
    o = bpy.context.active_object
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.bisect(plane_co=(0, 0, loc[2]), plane_no=(0, 0, 1), clear_inner=True, use_fill=True)
    bpy.ops.object.mode_set(mode="OBJECT")
    return finish(o, material)


def ico(radius, loc, material, sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, radius=radius, location=loc)
    return finish(bpy.context.active_object, material)


def torus(major, minor, loc, material, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, major_segments=8, minor_segments=4, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def lumpy_cube(size, loc, material, cuts, amount, seed):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o = bpy.context.active_object
    o.scale = size
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.subdivide(number_cuts=cuts)
    bpy.ops.object.mode_set(mode="OBJECT")
    random.seed(seed)
    for v in o.data.vertices:
        if v.co.z > loc[2] - size[2] / 2 + 0.002:       # keep the bottom flat so the box stands
            v.co.x += random.uniform(-amount, amount)
            v.co.y += random.uniform(-amount, amount)
            v.co.z += random.uniform(-amount, amount)
    return finish(o, material)


class Layers:
    def __init__(self):
        self.parts = {"Body": [], "Lid": [], "Seal": [], "Binding": [], "Glow": []}

    def add(self, layer, obj):
        self.parts[layer].append(obj)
        return obj


def seal_at(L, loc, rot=(R90, 0, 0)):
    L.add("Seal", cyl(0.024, 0.009, loc, "seal", verts=8, rot=rot))
    ring_loc = (loc[0], loc[1] - 0.003, loc[2])
    L.add("Seal", cyl(0.014, 0.012, ring_loc, "paper", verts=8, rot=rot))
    L.add("Seal", box((0.012, 0.004, 0.045), (loc[0] + 0.012, loc[1] - 0.001, loc[2] - 0.04), "paper"))
    L.add("Seal", box((0.012, 0.004, 0.04), (loc[0] - 0.012, loc[1] - 0.001, loc[2] - 0.038), "paper", rot=(0, math.radians(8), 0)))


# ------------------------------------------------------------------------------------------ the four boxes
def build_h1():                                   # small natural wooden crate
    L = Layers()
    w, d, h = 0.30, 0.30, 0.24
    L.add("Body", box((w - 0.012, d - 0.012, h), (0, 0, h / 2), "wood_dark"))
    for i in range(4):                            # boards on all four sides
        z = 0.03 + i * 0.056
        m = "wood" if i % 2 == 0 else "wood_light"
        for sy in (-1, 1):
            L.add("Body", box((w - 0.02, 0.012, 0.05), (0, sy * (d / 2 + 0.002), z), m))
        for sx in (-1, 1):
            L.add("Body", box((0.012, d - 0.02, 0.05), (sx * (w / 2 + 0.002), 0, z), m))
    for sx in (-1, 1):
        for sy in (-1, 1):
            L.add("Body", box((0.038, 0.038, h + 0.01), (sx * w / 2, sy * d / 2, (h + 0.01) / 2), "wood_dark"))      # corner posts
            L.add("Body", box((0.05, 0.05, 0.012), (sx * (w / 2 - 0.006), sy * (d / 2 - 0.006), h - 0.002), "iron"))  # corner brackets
    for sy in (-1, 1):
        L.add("Body", box((w + 0.02, 0.035, 0.02), (0, sy * 0.095, 0.01), "wood_dark"))                              # skids
    for i in range(4):                            # nail heads
        for sx in (-1, 1):
            L.add("Body", cyl(0.006, 0.006, (sx * (w / 2 - 0.035), -d / 2 - 0.01, 0.03 + i * 0.056), "iron", verts=6, rot=(R90, 0, 0)))
    for sx in (-1, 1):                            # rope handles
        L.add("Body", torus(0.04, 0.007, (sx * (w / 2 + 0.014), 0, h * 0.6), "rope", rot=(0, R90, 0)))
    # lid
    L.add("Lid", box((w + 0.016, d + 0.016, 0.03), (0, 0, h + 0.015), "wood"))
    for x in (-0.09, 0.0, 0.09):
        L.add("Lid", box((0.032, d + 0.016, 0.016), (x, 0, h + 0.038), "wood_dark"))
    for sx in (-1, 1):
        for sy in (-1, 1):
            L.add("Lid", box((0.045, 0.045, 0.008), (sx * (w / 2 + 0.002), sy * (d / 2 + 0.002), h + 0.033), "iron"))
    # iron strap + paper tag across the front seam, wax seal on top of it
    L.add("Binding", box((0.05, 0.014, 0.17), (0, -d / 2 - 0.012, h * 0.82), "iron"))
    for z in (h * 0.82 + 0.05, h * 0.82 - 0.05):
        L.add("Binding", cyl(0.007, 0.006, (0, -d / 2 - 0.02, z), "wood_dark", verts=6, rot=(R90, 0, 0)))
    L.add("Binding", box((0.05, 0.004, 0.07), (0.075, -d / 2 - 0.016, h * 0.5), "paper"))
    L.add("Binding", box((0.004, 0.004, 0.04), (0.075, -d / 2 - 0.016, h * 0.5 + 0.055), "cord"))
    seal_at(L, (0, -d / 2 - 0.022, h * 0.98))
    L.add("Body", box((w - 0.07, d - 0.07, 0.004), (0, 0, h + 0.002), "interior"))   # dark opening
    L.add("Glow", ico(0.07, (0, 0, h - 0.004), "glow"))
    return "H1", L, (0, d / 2, h)


def build_h2():                                   # slate metal lockbox
    L = Layers()
    w, d, h = 0.34, 0.26, 0.22
    L.add("Body", box((w + 0.012, d + 0.012, 0.03), (0, 0, 0.015), "slate_dk"))
    L.add("Body", box((w, d, h - 0.03), (0, 0, 0.03 + (h - 0.03) / 2), "slate"))
    for sx in (-1, 1):
        for sy in (-1, 1):
            L.add("Body", box((0.04, 0.04, h - 0.02), (sx * w / 2, sy * d / 2, (h - 0.02) / 2 + 0.01), "steel"))
    for sy in (-1, 1):                            # raised panels with rivets
        L.add("Body", box((w - 0.1, 0.008, h - 0.09), (0, sy * (d / 2 + 0.004), h / 2), "slate_dk"))
        for sx in (-1, 1):
            for z in (0.06, h - 0.06):
                L.add("Body", ico(0.008, (sx * (w / 2 - 0.065), sy * (d / 2 + 0.009), z), "steel"))
    L.add("Body", box((0.07, 0.006, 0.07), (0, -d / 2 - 0.011, h / 2 - 0.01), "steel", rot=(0, math.radians(45), 0)))   # engraved diamond
    for sx in (-1, 1):                            # side handles
        L.add("Body", box((0.012, 0.09, 0.014), (sx * (w / 2 + 0.024), 0, h * 0.55), "steel"))
        for sy in (-1, 1):
            L.add("Body", box((0.02, 0.014, 0.014), (sx * (w / 2 + 0.014), sy * 0.04, h * 0.55), "steel"))
    for x in (-0.1, 0.1):                         # hinges
        L.add("Body", cyl(0.012, 0.04, (x, d / 2 + 0.008, h), "steel", verts=8, rot=(0, R90, 0)))
    # lid
    L.add("Lid", box((w + 0.012, d + 0.012, 0.04), (0, 0, h + 0.02), "slate_dk"))
    L.add("Lid", box((w - 0.06, d - 0.06, 0.012), (0, 0, h + 0.046), "slate"))
    L.add("Lid", box((0.08, 0.08, 0.006), (0, 0, h + 0.054), "steel", rot=(0, 0, math.radians(45))))
    L.add("Lid", box((0.1, 0.014, 0.014), (0, 0, h + 0.075), "steel"))
    for sx in (-1, 1):
        L.add("Lid", box((0.014, 0.014, 0.03), (sx * 0.045, 0, h + 0.062), "steel"))
    for sx in (-1, 1):
        L.add("Lid", ico(0.01, (sx * (w / 2 - 0.02), -d / 2 - 0.004, h + 0.02), "steel"))
    # clasp + padlock
    L.add("Binding", box((0.06, 0.014, 0.09), (0, -d / 2 - 0.012, h * 0.9), "steel"))
    L.add("Binding", torus(0.014, 0.004, (0, -d / 2 - 0.02, h * 0.74), "steel", rot=(R90, 0, 0)))
    L.add("Binding", box((0.05, 0.02, 0.045), (0, -d / 2 - 0.026, h * 0.6), "brass"))
    L.add("Binding", torus(0.016, 0.0045, (0, -d / 2 - 0.026, h * 0.6 + 0.032), "brass", rot=(R90, 0, 0)))
    L.add("Binding", box((0.008, 0.006, 0.014), (0, -d / 2 - 0.038, h * 0.6), "keyhole"))
    seal_at(L, (0.105, -d / 2 - 0.014, h * 0.97))
    L.add("Body", box((w - 0.07, d - 0.07, 0.004), (0, 0, h + 0.002), "interior"))
    L.add("Glow", ico(0.07, (0, 0, h - 0.004), "glow"))
    return "H2", L, (0, d / 2 + 0.008, h)


def build_h3():                                   # canvas wrapped parcel
    L = Layers()
    w, d, h = 0.30, 0.30, 0.17
    L.add("Body", lumpy_cube((w, d, h), (0, 0, h / 2), "canvas", 3, 0.011, 3))
    L.add("Body", box((w + 0.02, d + 0.02, 0.03), (0, 0, 0.015), "canvas_dk"))
    for i, x in enumerate((-0.09, 0.0, 0.09)):    # fold ridges on the front
        L.add("Body", box((0.014, 0.012, 0.11), (x, -d / 2 - 0.004, 0.08), "canvas_dk", rot=(0, math.radians(18 - 18 * i), 0)))
    for sx in (-1, 1):
        L.add("Body", box((0.012, 0.012, 0.11), (sx * (w / 2 + 0.004), 0.05, 0.08), "canvas_dk", rot=(math.radians(12 * sx), 0, 0)))
    for i in range(6):                            # stitches along the bottom seam
        L.add("Body", box((0.012, 0.006, 0.006), (-0.12 + i * 0.048, -d / 2 - 0.008, 0.035), "cord"))
    L.add("Lid", lumpy_cube((w - 0.01, d - 0.01, 0.05), (0, 0, h + 0.02), "canvas", 2, 0.009, 11))
    L.add("Lid", box((0.36, 0.014, 0.014), (0, 0, h + 0.047), "canvas_dk", rot=(0, 0, math.radians(45))))
    # cord, bow and a paper tag
    L.add("Binding", box((w + 0.026, 0.03, h + 0.05), (0, 0, (h + 0.05) / 2 + 0.002), "cord"))
    L.add("Binding", box((0.03, d + 0.026, h + 0.05), (0, 0, (h + 0.05) / 2 + 0.002), "cord"))
    L.add("Binding", ico(0.022, (0, 0, h + 0.05), "cord"))
    for s in (-1, 1):
        L.add("Binding", box((0.07, 0.02, 0.018), (s * 0.04, 0, h + 0.06), "cord", rot=(0, 0, math.radians(30 * s))))
        L.add("Binding", box((0.016, 0.01, 0.075), (s * 0.02, -0.02, h + 0.02), "cord", rot=(0, 0, math.radians(15 * s))))
    L.add("Binding", box((0.05, 0.004, 0.065), (0.08, -d / 2 - 0.03, 0.095), "paper"))
    L.add("Binding", box((0.004, 0.004, 0.04), (0.08, -d / 2 - 0.03, 0.14), "cord"))
    seal_at(L, (-0.07, -d / 2 - 0.022, h + 0.02), rot=(R90, 0, 0))
    L.add("Body", box((w - 0.1, d - 0.1, 0.004), (0, 0, h + 0.016), "interior"))
    L.add("Glow", ico(0.07, (0, 0, h + 0.006), "glow"))
    return "H3", L, (0, d / 2, h)


def build_rd():                                   # dark treasure chest with a barrel lid
    L = Layers()
    w, d, h = 0.38, 0.26, 0.17
    L.add("Body", box((w, d, h), (0, 0, h / 2), "chest"))
    for i in range(3):                            # planks on the front and back
        for sy in (-1, 1):
            L.add("Body", box((w - 0.02, 0.01, 0.045), (0, sy * (d / 2 + 0.003), 0.03 + i * 0.052), "chest_light" if i % 2 == 0 else "chest"))
    for sx in (-1, 1):
        for sy in (-1, 1):
            L.add("Body", box((0.03, 0.03, h + 0.012), (sx * (w / 2), sy * (d / 2 - 0.004), (h + 0.012) / 2), "brass"))
            L.add("Body", box((0.045, 0.045, 0.025), (sx * (w / 2 - 0.02), sy * (d / 2 - 0.02), 0.0125), "brass"))    # feet
    for x in (-0.14, 0.14):
        for sy in (-1, 1):
            L.add("Body", box((0.028, 0.008, h), (x, sy * (d / 2 + 0.006), h / 2), "brass"))
    for sx in (-1, 1):                            # side ring handles
        L.add("Body", cyl(0.012, 0.01, (sx * (w / 2 + 0.006), 0, h * 0.55), "brass", verts=8, rot=(0, R90, 0)))
        L.add("Body", torus(0.022, 0.005, (sx * (w / 2 + 0.018), 0, h * 0.55 - 0.012), "brass", rot=(0, R90, 0)))
    L.add("Lid", half_cyl(d / 2, w, (0, 0, h), "chest"))
    for x in (-0.14, 0.0, 0.14):
        L.add("Lid", half_cyl(d / 2 + 0.008, 0.03, (x, 0, h), "brass"))
    for sx in (-1, 1):
        L.add("Lid", half_cyl(d / 2 + 0.004, 0.012, (sx * (w / 2), 0, h), "brass"))
    L.add("Lid", box((w, 0.02, 0.016), (0, -d / 2 - 0.004, h + 0.005), "brass"))
    # lock plate and padlock
    L.add("Binding", box((0.07, 0.014, 0.07), (0, -d / 2 - 0.014, h * 0.92), "brass"))
    L.add("Binding", box((0.012, 0.016, 0.025), (0, -d / 2 - 0.022, h * 0.9), "keyhole"))
    L.add("Binding", box((0.06, 0.026, 0.055), (0, -d / 2 - 0.03, h * 0.6), "brass"))
    L.add("Binding", torus(0.017, 0.005, (0, -d / 2 - 0.03, h * 0.6 + 0.036), "brass", rot=(R90, 0, 0)))
    L.add("Binding", box((0.008, 0.006, 0.016), (0, -d / 2 - 0.044, h * 0.6), "keyhole"))
    seal_at(L, (0.1, -d / 2 - 0.016, h * 0.97))
    L.add("Body", box((w - 0.07, d - 0.07, 0.004), (0, 0, h + 0.002), "interior"))
    L.add("Glow", ico(0.07, (0, 0, h - 0.004), "glow"))
    return "RD", L, (0, d / 2, h)


# ------------------------------------------------------------------------------------------ assemble and export
def join(parts):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    return bpy.context.active_object


def set_origin(obj, point):
    p = mathutils.Vector(point)
    for v in obj.data.vertices:
        v.co -= p
    obj.location = p


def bbox_centre(obj):
    xs = [v.co for v in obj.data.vertices]
    lo = mathutils.Vector((min(c.x for c in xs), min(c.y for c in xs), min(c.z for c in xs)))
    hi = mathutils.Vector((max(c.x for c in xs), max(c.y for c in xs), max(c.z for c in xs)))
    return (lo + hi) / 2


def clear_scene():
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    for m in list(bpy.data.meshes):
        bpy.data.meshes.remove(m)


bpy.ops.wm.read_factory_settings(use_empty=True)
total = {}
for builder in (build_h1, build_h2, build_h3, build_rd):
    name, L, hinge = builder()
    root = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(root)
    tris = 0
    for layer, parts in L.parts.items():
        if not parts:
            continue
        o = join(parts)
        o.name = layer
        if layer == "Lid":
            set_origin(o, hinge)
        elif layer in ("Seal", "Binding", "Glow"):
            set_origin(o, bbox_centre(o))
        o.parent = root
        tris += sum(len(p.vertices) - 2 for p in o.data.polygons) if layer != "Glow" else 0
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for c in root.children:
        c.select_set(True)
    bpy.context.view_layer.objects.active = root
    path = os.path.join(OUT, f"{name}.glb")
    bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
    total[name] = tris
    print(name, "triangles (without glow):", tris, "->", path)
    clear_scene()

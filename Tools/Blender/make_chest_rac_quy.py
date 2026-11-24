"""Rương Rác Quý: a dented round trash can with a domed flip lid, low-poly, built procedurally with Blender (bpy).

Run: python make_chest_rac_quy.py -- [output .glb]
     (default: Assets/_Project/Art/Chests/Chest_RacQuy.glb next to this repo)
Layers (children of root Chest_RacQuy), same contract as make_boxes_lowpoly.py so BoxOpenSequence.cs works:
  Body    dented tapered can, bands, side handles, hinge, sticker "QUÝ", fish bone poking out, junk inside
  Lid     domed lid with handle and a cheap paper crown; ORIGIN ON THE BACK HINGE, rotate about X (front is -Y)
  Seal    a smelly sock hung over the front rim (falls off at the "Go niem" stage)
  Binding two strips of silver duct tape crossing the front seam
  Glow    green-yellow stink puffs + three flies above the mouth (scaled from 0 by BoxOpenSequence)
Bottom centre at the origin, metres, Blender Z up (glTF export converts to Y up). Flat colours, no image textures.
Tweak the numbers in the CONFIG block and run again.
"""
import math, random, sys, os
import bpy, mathutils

HERE = os.path.dirname(os.path.abspath(__file__))
DEFAULT_OUT = os.path.normpath(os.path.join(HERE, "..", "..", "Assets", "_Project", "Art", "Chests", "Chest_RacQuy.glb"))
ARGS = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
OUT = ARGS[0] if ARGS else DEFAULT_OUT
os.makedirs(os.path.dirname(OUT), exist_ok=True)
R90 = math.radians(90)

# ------------------------------------------------------------------------------------------ CONFIG
SEGS = 20                 # sides of the can (low-poly look)
H = 0.345                 # height of the rim
R_BOT, R_TOP = 0.128, 0.158
DENT = dict(angle=-50, z=0.16, radius=0.11, depth=0.034)   # angle in degrees around Z, -90 = straight front (-Y)
HINGE = (0.0, 0.176, H + 0.004)
SOCK_ANGLE = -42          # degrees the sock is moved round from the front (negative = towards -X, left)

PALETTE = {  # sRGB, converted to linear below
    "can": (0.40, 0.45, 0.33), "can_dk": (0.27, 0.31, 0.23), "can_in": (0.16, 0.18, 0.14),
    "lid": (0.44, 0.49, 0.36), "metal": (0.46, 0.47, 0.47), "interior": (0.05, 0.05, 0.04),
    "tape": (0.56, 0.57, 0.57), "tape_dk": (0.42, 0.43, 0.43),
    "sock": (0.74, 0.68, 0.54), "sock_stripe": (0.45, 0.47, 0.52), "stain": (0.45, 0.36, 0.20),
    "paper": (0.86, 0.80, 0.62), "ink": (0.14, 0.12, 0.10), "crown": (0.76, 0.62, 0.28),
    "bone": (0.86, 0.84, 0.76), "canjunk": (0.62, 0.30, 0.24), "slipper": (0.36, 0.55, 0.62),
    "stink": (0.56, 0.74, 0.30), "fly": (0.08, 0.08, 0.07), "wing": (0.80, 0.86, 0.88),
}
EMISSIVE = {"stink": 0.7}
_mats = {}


def mat(name):
    if name in _mats:
        return _mats[name]
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes["Principled BSDF"]
    c = tuple(x ** 2.2 for x in PALETTE[name])
    bsdf.inputs["Base Color"].default_value = (c[0], c[1], c[2], 1.0)
    bsdf.inputs["Roughness"].default_value = 0.55 if name in ("metal", "tape") else 0.9
    bsdf.inputs["Metallic"].default_value = 0.0
    if name in EMISSIVE:
        bsdf.inputs["Emission Color"].default_value = (c[0], c[1], c[2], 1.0)
        bsdf.inputs["Emission Strength"].default_value = EMISSIVE[name]
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


def cyl(radius, depth, loc, material, verts=8, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def cone(r1, r2, depth, loc, material, verts=4, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=depth, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def ico(radius, loc, material, sub=1, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, radius=radius, location=loc)
    o = bpy.context.active_object
    o.scale = scale
    return finish(o, material)


def lathe(profile, material, segs=SEGS, wobble=0.0, seed=0, dent=None):
    """Revolve a (radius, z) profile around Z. radius 0 = a single pole vertex. Optional wobble and dent."""
    random.seed(seed)
    verts, rings = [], []
    for r, z in profile:
        if r <= 1e-6:
            rings.append([len(verts)])
            verts.append((0.0, 0.0, z))
            continue
        ring = []
        for i in range(segs):
            a = 2 * math.pi * i / segs
            rr = r + (random.uniform(-wobble, wobble) if wobble else 0.0)
            ring.append(len(verts))
            verts.append((rr * math.cos(a), rr * math.sin(a), z))
        rings.append(ring)
    faces = []
    for ra, rb in zip(rings, rings[1:]):
        if len(ra) == 1 and len(rb) == 1:
            continue
        if len(ra) == 1:
            faces += [(ra[0], rb[i], rb[(i + 1) % segs]) for i in range(segs)]
        elif len(rb) == 1:
            faces += [(ra[i], rb[0], ra[(i + 1) % segs]) for i in range(segs)]
        else:
            faces += [(ra[i], ra[(i + 1) % segs], rb[(i + 1) % segs], rb[i]) for i in range(segs)]
    me = bpy.data.meshes.new("lathe")
    me.from_pydata(verts, [], faces)
    me.update()
    o = bpy.data.objects.new("lathe", me)
    bpy.context.scene.collection.objects.link(o)
    if dent:
        apply_dent(o, **dent)
    me.validate()
    o.select_set(True)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    return finish(o, material)


def apply_dent(o, angle, z, radius, depth):
    """Push vertices inwards around one spot of the wall: the 'someone kicked this can' dent."""
    a0 = math.radians(angle)
    centre = mathutils.Vector((math.cos(a0) * R_TOP, math.sin(a0) * R_TOP, z))
    for v in o.data.vertices:
        rxy = math.hypot(v.co.x, v.co.y)
        if rxy < 1e-6:
            continue
        p = mathutils.Vector((v.co.x / rxy * R_TOP, v.co.y / rxy * R_TOP, v.co.z))
        d = (p - centre).length
        if d < radius:
            k = (1 - d / radius) ** 1.5
            s = (rxy - depth * k) / rxy
            v.co.x *= s
            v.co.y *= s


def spin_z(o, deg):
    """Rotate a finished part around the can's axis (world Z)."""
    m = mathutils.Matrix.Rotation(math.radians(deg), 4, "Z")
    o.data.transform(m)
    o.data.update()


def r_at(z):
    return R_BOT + (R_TOP - R_BOT) * z / H


class Layers:
    def __init__(self):
        self.parts = {"Body": [], "Lid": [], "Seal": [], "Binding": [], "Glow": []}

    def add(self, layer, obj):
        self.parts[layer].append(obj)
        return obj


# ------------------------------------------------------------------------------------------ the can
def build():
    L = Layers()
    # --- Body: tapered can with a rolled rim and a short inner wall down to the dark "inside"
    prof = [(0.0, 0.0), (R_BOT - 0.006, 0.0), (R_BOT, 0.012)]
    for z in (0.05, 0.10, 0.15, 0.20, 0.25, 0.30, H - 0.018):
        prof.append((r_at(z), z))
    prof += [(R_TOP + 0.010, H - 0.012), (R_TOP + 0.010, H), (R_TOP - 0.008, H), (R_TOP - 0.012, H - 0.04)]
    L.add("Body", lathe(prof, "can", wobble=0.0015, seed=7, dent=DENT))
    L.add("Body", lathe([(R_TOP - 0.012, H - 0.04), (R_TOP - 0.02, H - 0.06), (0.0, H - 0.06)], "interior", seed=1))
    for zb in (0.105, 0.235):                                      # two raised bands
        r = r_at(zb)
        L.add("Body", lathe([(r - 0.003, zb - 0.014), (r + 0.007, zb - 0.009), (r + 0.007, zb + 0.009), (r - 0.003, zb + 0.014)],
                            "can_dk", seed=int(zb * 1000), dent=DENT))
    L.add("Body", lathe([(R_BOT - 0.004, 0.0), (R_BOT + 0.006, 0.004), (R_BOT + 0.006, 0.02), (R_BOT - 0.002, 0.026)], "can_dk", seed=3))
    for sx in (-1, 1):                                             # side handles (open grips)
        x = sx * (r_at(0.28) + 0.006)
        for sy in (-1, 1):
            L.add("Body", box((0.022, 0.012, 0.03), (x + sx * 0.008, sy * 0.032, 0.28), "can_dk"))
        L.add("Body", box((0.014, 0.078, 0.014), (x + sx * 0.022, 0, 0.268), "can_dk"))
    for sx in (-1, 1):                                             # back hinge knuckles + bracket
        L.add("Body", cyl(0.008, 0.03, (sx * 0.045, HINGE[1], HINGE[2] - 0.004), "metal", verts=8, rot=(0, R90, 0)))
        L.add("Body", box((0.03, 0.006, 0.03), (sx * 0.045, HINGE[1] - 0.008, H - 0.02), "metal"))
    # crooked paper sticker "QUÝ" on the front, like a fake luxury tag
    zs = 0.175
    ys = -(r_at(zs) + 0.003)
    tilt = math.atan((R_TOP - R_BOT) / H)
    L.add("Body", cyl(0.045, 0.003, (0.0, ys, zs), "paper", verts=6, rot=(R90 + tilt, 0, math.radians(0))))
    L.add("Body", text_mesh("QUÝ", 0.03, (0.0, ys - 0.002, zs - 0.01), (R90 + tilt, 0, 0), "ink", spin=math.radians(-8)))
    # fish bone poking out from under the lid, back right
    a = math.radians(35)
    base = mathutils.Vector((math.cos(a) * (R_TOP + 0.01), math.sin(a) * (R_TOP + 0.01), H - 0.004))
    out = mathutils.Vector((math.cos(a), math.sin(a), -0.55)).normalized()
    rot = out.to_track_quat("X", "Z").to_euler()
    centre = base + out * 0.05
    L.add("Body", box((0.10, 0.006, 0.006), tuple(centre), "bone", rot=rot))
    M = rot.to_matrix()
    for i in range(4):
        p = base + out * (0.025 + i * 0.02)
        for s in (-1, 1):
            rib = M @ mathutils.Vector((0, s * 0.012, 0))
            L.add("Body", box((0.004, 0.026, 0.004), tuple(p + rib), "bone", rot=(rot.x, rot.y, rot.z + s * 0.5)))
    tip = base + out * 0.108
    L.add("Body", cone(0.016, 0.002, 0.008, tuple(tip), "bone", verts=3, rot=(rot.x, rot.y + R90, rot.z)))
    # junk visible once the lid is open: a crushed can and a honeycomb slipper
    L.add("Body", cyl(0.028, 0.03, (0.05, -0.03, H - 0.05), "canjunk", verts=8, rot=(math.radians(70), 0, math.radians(30))))
    L.add("Body", box((0.05, 0.11, 0.012), (-0.05, 0.02, H - 0.052), "slipper", rot=(math.radians(-14), math.radians(10), math.radians(-25))))

    # --- Lid: domed cap with a skirt, a bar handle and a cheap paper crown
    lid_prof = [(R_TOP + 0.016, H - 0.012), (R_TOP + 0.016, H + 0.008), (R_TOP + 0.006, H + 0.016), (R_TOP - 0.03, H + 0.036),
                (0.08, H + 0.052), (0.0, H + 0.058)]
    L.add("Lid", lathe(lid_prof, "lid", wobble=0.002, seed=11))
    L.add("Lid", lathe([(R_TOP + 0.014, H - 0.012), (R_TOP - 0.006, H - 0.012), (0.0, H - 0.004)], "can_in", seed=12))
    L.add("Lid", lathe([(R_TOP + 0.018, H - 0.004), (R_TOP + 0.020, H + 0.004), (R_TOP + 0.010, H + 0.012)], "can_dk", seed=13))
    top = H + 0.058
    for sx in (-1, 1):
        L.add("Lid", box((0.014, 0.016, 0.03), (sx * 0.042, 0, top + 0.012), "can_dk"))
    L.add("Lid", box((0.104, 0.02, 0.014), (0, 0, top + 0.03), "can_dk"))
    crown_c = (0.07, 0.035, top + 0.002)                            # crown leaning on the handle
    crown_rot = (math.radians(-8), math.radians(-22), 0)
    crown = [lathe([(0.032, -0.012), (0.034, 0.012)], "crown", segs=10, seed=21)]
    for i in range(5):
        aa = 2 * math.pi * i / 5
        crown.append(cone(0.011, 0.0, 0.022, (math.cos(aa) * 0.033, math.sin(aa) * 0.033, 0.023), "crown", verts=4))
    for c in crown:
        c.rotation_euler = crown_rot
        c.location = crown_c
        L.add("Lid", finish(c, "crown"))

    # --- Seal: a smelly sock hung over the rim, front left (built facing -Y, then spun around the can)
    xs, yo = 0.0, -(R_TOP + 0.022)
    first = len(L.parts["Seal"])
    L.add("Seal", box((0.046, 0.018, 0.06), (xs, yo, H - 0.02), "sock"))
    L.add("Seal", box((0.048, 0.02, 0.012), (xs, yo - 0.001, H - 0.002), "sock_stripe"))
    L.add("Seal", box((0.042, 0.016, 0.07), (xs + 0.004, yo + 0.002, H - 0.08), "sock", rot=(0, math.radians(-8), 0)))
    L.add("Seal", box((0.07, 0.018, 0.034), (xs + 0.03, yo + 0.004, H - 0.125), "sock", rot=(0, math.radians(-14), 0)))
    L.add("Seal", box((0.02, 0.004, 0.02), (xs - 0.002, yo - 0.011, H - 0.07), "stain", rot=(0, math.radians(20), 0)))
    L.add("Seal", box((0.016, 0.004, 0.014), (xs + 0.048, yo - 0.007, H - 0.13), "stain"))
    for o in L.parts["Seal"][first:]:
        spin_z(o, SOCK_ANGLE)

    # --- Binding: two strips of duct tape crossing the front seam (lid dome -> skirt -> body)
    for s in (-1, 1):
        spin = math.radians(18 * s)
        xo = 0.035 * s
        L.add("Binding", box((0.034, 0.004, 0.07), (xo, -(R_TOP + 0.019), H - 0.022), "tape", rot=(math.radians(-5), spin, 0)))
        L.add("Binding", box((0.034, 0.05, 0.004), (xo * 0.6, -(R_TOP - 0.006), H + 0.03), "tape", rot=(math.radians(-30), 0, spin * 0.5)))
        L.add("Binding", box((0.034, 0.004, 0.016), (xo - 0.018 * s, -(R_TOP + 0.024), H - 0.062), "tape_dk", rot=(math.radians(-25), spin, 0)))

    # --- Glow: stink puffs rising from the mouth + three flies
    for i, (x, y, z, r) in enumerate(((0, 0, H + 0.02, 0.06), (0.05, 0.03, H + 0.06, 0.045), (-0.05, -0.02, H + 0.07, 0.042),
                                      (0.025, -0.02, H + 0.12, 0.035), (-0.02, 0.03, H + 0.16, 0.028), (0.01, 0.0, H + 0.2, 0.02))):
        L.add("Glow", ico(r, (x, y, z), "stink", sub=1, scale=(1, 1, 0.8)))
    for (x, y, z, yaw) in ((0.13, -0.06, H + 0.17, 20), (-0.11, 0.04, H + 0.21, -50), (0.04, -0.12, H + 0.25, 110)):
        L.add("Glow", ico(0.013, (x, y, z), "fly", sub=1, scale=(1.4, 1, 0.9)))
        for s in (-1, 1):
            L.add("Glow", box((0.018, 0.026, 0.002), (x - 0.003, y + s * 0.016, z + 0.01), "wing",
                              rot=(math.radians(25 * s), 0, math.radians(yaw))))
    return L


def text_mesh(txt, size, loc, rot, material, spin=0.0):
    bpy.ops.object.text_add(location=(0, 0, 0))
    t = bpy.context.active_object
    t.data.body = txt
    t.data.size = size
    t.data.align_x = "CENTER"
    t.data.align_y = "CENTER"
    t.data.extrude = 0.0006
    t.data.resolution_u = 2
    t.rotation_euler = (0, 0, spin)
    bpy.ops.object.convert(target="MESH")
    o = bpy.context.active_object
    bpy.ops.object.transform_apply(rotation=True)
    o.rotation_euler = rot
    o.location = loc
    return finish(o, material)


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


bpy.ops.wm.read_factory_settings(use_empty=True)
L = build()
root = bpy.data.objects.new("Chest_RacQuy", None)
bpy.context.scene.collection.objects.link(root)
tris = {}
for layer, parts in L.parts.items():
    o = join(parts)
    o.name = layer
    o.data.name = layer
    if layer == "Lid":
        set_origin(o, HINGE)
    elif layer in ("Seal", "Binding", "Glow"):
        set_origin(o, bbox_centre(o))
    o.parent = root
    tris[layer] = sum(len(p.vertices) - 2 for p in o.data.polygons)
bpy.ops.object.select_all(action="DESELECT")
root.select_set(True)
for c in root.children:
    c.select_set(True)
bpy.context.view_layer.objects.active = root
bpy.ops.export_scene.gltf(filepath=OUT, export_format="GLB", use_selection=True, export_apply=True)
zs = [(o.matrix_world @ mathutils.Vector(b)).z for o in root.children if o.name != "Glow" for b in o.bound_box]
print("Chest_RacQuy triangles per layer:", tris)
print("total without Glow:", sum(v for k, v in tris.items() if k != "Glow"), " height: %.3f m" % max(zs), "->", OUT)

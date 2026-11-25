"""Pirate theme chest "Chest_HaiTac": a captain's skull in a tricorne hat, sitting on a pile of gold on crossed bones.
Built procedurally with Blender (bpy), no Meshy. Design notes: docs/superpowers/specs/2026-11-24-ruong-hai-tac-design.md

Run: python make_chest_hai_tac.py -- <output .glb>
     (default: Assets/_Project/Art/Chests/Chest_HaiTac.glb next to this repo)
Layers (children of the root "Chest_HaiTac"), same contract as the other boxes so BoxOpenSequence.cs works:
  Body    skull below the forehead cut, jaw, teeth, eye patch, gold pile, crossbones, dark opening
  Lid     skull cap + tricorne hat, origin on the hinge at the back of the head, rotates around X, front is -Y
  Seal    red wax seal on the rope
  Binding rope around the forehead cut + brass padlock hanging in the middle of the forehead
  Glow    warm gold light at the opening with a few coins in it (hidden by scale until the box opens)
  Key     iron key already inserted in the padlock, shaft along Blender Y
Bottom centre at the origin, metres, Blender Z up (glTF export converts to Y up).
"""
import math, random, sys, os
import bpy, bmesh, mathutils

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv and len(sys.argv) > sys.argv.index("--") + 1 \
    else os.path.join(HERE, "..", "..", "Assets", "_Project", "Art", "Chests", "Chest_HaiTac.glb")
OUT = os.path.abspath(OUT)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
R90 = math.radians(90)

PALETTE = {  # sRGB, converted to linear below
    "bone": (0.86, 0.80, 0.66), "bone_dk": (0.70, 0.63, 0.49), "socket": (0.08, 0.06, 0.05),
    "hat": (0.15, 0.14, 0.15), "hat_dk": (0.09, 0.085, 0.09), "gold": (0.86, 0.66, 0.22), "gold_dk": (0.66, 0.48, 0.16),
    "rope": (0.72, 0.62, 0.44), "brass": (0.62, 0.52, 0.22), "keyhole": (0.07, 0.06, 0.06), "iron": (0.40, 0.42, 0.45),
    "seal": (0.52, 0.20, 0.15), "feather": (0.56, 0.20, 0.16), "patch": (0.10, 0.09, 0.09),
    "glow": (1.0, 0.82, 0.45), "interior": (0.07, 0.055, 0.045),
}
METAL = {"gold", "gold_dk", "brass", "iron"}
_mats = {}


def mat(name):
    if name in _mats:
        return _mats[name]
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes["Principled BSDF"]
    c = tuple(x ** 2.2 for x in PALETTE[name])
    bsdf.inputs["Base Color"].default_value = (c[0], c[1], c[2], 1.0)
    bsdf.inputs["Roughness"].default_value = 0.45 if name in METAL else 0.85
    bsdf.inputs["Metallic"].default_value = 0.6 if name in METAL else 0.0
    if name == "glow":
        bsdf.inputs["Emission Color"].default_value = (c[0], c[1], c[2], 1.0)
        bsdf.inputs["Emission Strength"].default_value = 2.0
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


def cyl(radius, depth, loc, material, verts=8, rot=(0, 0, 0), scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = scale
    return finish(o, material)


def cone(r_bottom, r_top, depth, loc, material, verts=8, rot=(0, 0, 0), scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r_bottom, radius2=r_top, depth=depth, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = scale
    return finish(o, material)


def ico(radius, loc, material, sub=1, scale=(1, 1, 1), rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, radius=radius, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = scale
    return finish(o, material)


def torus(major, minor, loc, material, rot=(0, 0, 0), scale=(1, 1, 1), seg=8):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, major_segments=seg, minor_segments=4,
                                     location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = scale
    return finish(o, material)


def loft(loops, material, thickness=0.0, closed=True):
    """Quad strips between consecutive loops of points (all loops the same length); optional solidify."""
    me = bpy.data.meshes.new("loft")
    bm = bmesh.new()
    vs = [[bm.verts.new(p) for p in loop] for loop in loops]
    n = len(loops[0])
    for a, b in zip(vs, vs[1:]):
        for i in range(n if closed else n - 1):
            j = (i + 1) % n
            bm.faces.new((a[i], a[j], b[j], b[i]))
    bm.normal_update()
    bm.to_mesh(me)
    bm.free()
    o = bpy.data.objects.new("loft", me)
    bpy.context.scene.collection.objects.link(o)
    if thickness:
        mod = o.modifiers.new("solid", "SOLIDIFY")
        mod.thickness = thickness
        mod.offset = 0
        bpy.context.view_layer.objects.active = o
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return finish(o, material)


class Layers:
    def __init__(self):
        self.parts = {"Body": [], "Lid": [], "Seal": [], "Binding": [], "Glow": [], "Key": []}

    def add(self, layer, obj):
        self.parts[layer].append(obj)
        return obj


# ------------------------------------------------------------------------------------------ skull geometry
CZ, CR, SX, SY = 0.28, 0.14, 0.95, 1.12          # cranium centre height, radius, x / y squash
CUT = CZ + 0.04                                   # forehead cut: Lid above, Body below


def rad(z):
    return math.sqrt(max(CR * CR - (z - CZ) ** 2, 0.0))


def on_head(t, z, out=0.0):
    """Point on the cranium surface at height z, angle t around Z (t = -90 deg is the front, -Y), pushed out by `out`."""
    r = rad(z)
    return ((SX * r + out) * math.cos(t), (SY * r + out) * math.sin(t), z)


def arc_band(L, layer, z, t0, t1, segs, size_up, thick, material, out=0.004):
    """A strap made of small boxes following the head surface at height z, from angle t0 to t1 (degrees)."""
    for i in range(segs):
        a = math.radians(t0 + (t1 - t0) * (i + 0.5) / segs)
        b0 = on_head(math.radians(t0 + (t1 - t0) * i / segs), z, out)
        b1 = on_head(math.radians(t0 + (t1 - t0) * (i + 1) / segs), z, out)
        length = math.dist(b0, b1) + 0.004
        mid = on_head(a, z, out)
        yaw = math.atan2(b1[1] - b0[1], b1[0] - b0[0])
        L.add(layer, box((length, thick, size_up), mid, material, rot=(0, 0, yaw)))


def cranium_half(above):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=10, radius=CR, location=(0, 0, CZ))
    o = bpy.context.active_object
    o.scale = (SX, SY, 1.0)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.bisect(plane_co=(0, 0, CUT), plane_no=(0, 0, 1), use_fill=True,
                        clear_inner=above, clear_outer=not above)
    bpy.ops.object.mode_set(mode="OBJECT")
    return finish(o, "bone")


def build():
    L = Layers()
    random.seed(7)

    # ---- base: crossed bones under a pile of gold coins
    for s in (-1, 1):
        a = math.radians(38 * s)
        L.add("Body", cyl(0.021, 0.54, (0, 0, 0.026), "bone", verts=8, rot=(0, R90, a)))
        for e in (-1, 1):
            cx, cy = e * 0.27 * math.cos(a), e * 0.27 * math.sin(a)
            px, py = -math.sin(a) * 0.017, math.cos(a) * 0.017
            for k in (-1, 1):
                L.add("Body", ico(0.024, (cx + k * px, cy + k * py, 0.028), "bone", sub=1))
    L.add("Body", ico(0.16, (0, 0.0, 0.035), "gold_dk", sub=2, scale=(1.1, 1.0, 0.45)))
    for i in range(16):                           # loose coins on the pile
        ang = random.uniform(0, 2 * math.pi)
        d = random.uniform(0.08, 0.165)
        x, y = 1.08 * d * math.cos(ang), d * math.sin(ang)
        h = 0.035 + 0.16 * 0.45 * math.sqrt(max(0.0, 1 - (x / 0.176) ** 2 - (y / 0.16) ** 2)) + 0.002
        if abs(x) < 0.09 and y < 0 and y > -0.16:     # keep the jaw area clear
            continue
        L.add("Body", cyl(0.024, 0.006, (x, y, h), "gold", verts=10,
                          rot=(random.uniform(-0.5, 0.5), random.uniform(-0.5, 0.5), 0)))
    for x, y, z, rx in ((0.13, -0.15, 0.03, 1.2), (-0.16, -0.12, 0.03, 1.35)):   # a couple of coins standing on edge
        L.add("Body", cyl(0.026, 0.007, (x, y, z), "gold", verts=10, rot=(rx, 0, random.uniform(-0.6, 0.6))))

    # ---- skull below the cut
    L.add("Body", cranium_half(above=False))
    L.add("Body", cone(0.072, 0.10, 0.12, (0, -0.062, CZ - 0.135), "bone", verts=8, rot=(0, 0, math.radians(22.5)),
                       scale=(1.0, 0.95, 1.0)))                              # jaw / muzzle
    for s in (-1, 1):
        L.add("Body", ico(0.036, (s * 0.088, -0.1, CZ - 0.11), "bone", sub=1))     # cheekbones
        L.add("Body", ico(0.038, (s * 0.056, -0.124, CZ - 0.022), "socket", sub=1, scale=(1, 0.6, 1)))   # eye sockets
    L.add("Body", cyl(0.022, 0.03, (0, -0.138, CZ - 0.07), "socket", verts=3, rot=(R90, math.radians(30), 0)))   # nose
    L.add("Body", ico(0.009, (-0.056, -0.15, CZ - 0.02), "gold", sub=1))     # glint in the open eye
    arc_band(L, "Body", CZ + 0.012, -138, -42, 6, 0.02, 0.022, "bone_dk", out=0.0)   # brow ridge

    def jaw_front(z):                              # front face of the octagonal jaw at height z
        zb = CZ - 0.135 - 0.06
        r = 0.072 + 0.028 * (z - zb) / 0.12
        return -0.062 - 0.95 * r * math.cos(math.radians(22.5))

    zu, zl = CZ - 0.125, CZ - 0.168
    L.add("Body", box((0.12, 0.012, 0.018), (0, jaw_front(zu - 0.02) + 0.002, zu - 0.021), "socket"))   # mouth slit
    for i in range(6):
        x = -0.05 + i * 0.02
        L.add("Body", box((0.016, 0.012, 0.022), (x, jaw_front(zu) - 0.003, zu), "gold" if i == 3 else "bone"))
        L.add("Body", box((0.016, 0.012, 0.02), (x, jaw_front(zl) - 0.003, zl), "bone"))
    # eye patch over the right eye (+X), strap running round the side of the head
    L.add("Body", cyl(0.04, 0.012, (0.06, -0.144, CZ - 0.024), "patch", verts=8, rot=(R90 - 0.2, 0, 0.38)))
    arc_band(L, "Body", CZ - 0.01, -58, 95, 9, 0.012, 0.006, "patch", out=0.003)
    L.add("Body", cyl(SX * rad(CUT) - 0.016, 0.004, (0, 0, CUT + 0.002), "interior", verts=16,
                      scale=(1, SY / SX, 1)))     # dark opening

    # ---- lid: skull cap + tricorne hat (hinge at the back of the cut)
    L.add("Lid", cranium_half(above=True))
    L.add("Lid", cyl(SX * rad(CUT) - 0.016, 0.004, (0, 0, CUT - 0.002), "interior", verts=16, scale=(1, SY / SX, 1)))
    zb = CUT + 0.035                               # brim base height
    N = 24
    inner, mid, outer, trim_lo, trim_hi = [], [], [], [], []
    for i in range(N):
        phi = 2 * math.pi * i / N                  # 0 = front corner
        dx, dy = math.sin(phi), -math.cos(phi)
        c3 = math.cos(3 * phi)
        ro = 0.15 + 0.09 * (1 + c3) / 2            # corners stick out, sides fold up against the crown
        lift = 0.095 * (1 - c3) / 2
        ri = 0.125
        rm = ri + (ro - ri) * 0.55
        inner.append((ri * dx, 1.1 * ri * dy, zb))
        mid.append((rm * dx, 1.1 * rm * dy, zb + lift * 0.35))
        outer.append((ro * dx, 1.1 * ro * dy, zb + lift))
        trim_lo.append(((ro + 0.004) * dx, 1.1 * (ro + 0.004) * dy, zb + lift - 0.01))
        trim_hi.append(((ro + 0.004) * dx, 1.1 * (ro + 0.004) * dy, zb + lift + 0.008))
    L.add("Lid", loft([inner, mid, outer], "hat", thickness=0.009))
    L.add("Lid", loft([trim_lo, trim_hi], "gold", thickness=0.006))
    L.add("Lid", cone(0.132, 0.112, 0.12, (0, 0, zb + 0.045), "hat", verts=12, scale=(1, 1.1, 1)))   # crown
    L.add("Lid", cone(0.113, 0.09, 0.02, (0, 0, zb + 0.114), "hat_dk", verts=12, scale=(1, 1.1, 1)))  # dented top
    L.add("Lid", cyl(0.134, 0.022, (0, 0, zb + 0.008), "hat_dk", verts=12, scale=(1, 1.1, 1)))       # hat band
    # skull-and-bones badge on the front of the crown (the hat knows what it is)
    fy = -1.1 * 0.121 - 0.004
    L.add("Lid", ico(0.019, (0, fy - 0.002, zb + 0.07), "bone", sub=1, scale=(1, 0.45, 1.05)))
    for s in (-1, 1):
        L.add("Lid", box((0.06, 0.006, 0.008), (0, fy + 0.001, zb + 0.07), "bone", rot=(0, math.radians(35 * s), 0)))
    L.add("Lid", ico(0.02, (0.1, 0.05, zb + 0.105), "feather", sub=1, scale=(0.5, 3.2, 0.9),
                     rot=(math.radians(28), math.radians(-25), math.radians(18))))      # feather on the left side

    # ---- binding: rope around the cut, knot at the back, brass padlock on the forehead
    rx, ry = SX * rad(CUT) + 0.006, SY * rad(CUT) + 0.006
    L.add("Binding", torus(1.0, 0.06, (0, 0, CUT), "rope", seg=20, scale=(rx, ry, 0.13)))
    L.add("Binding", ico(0.018, (0, ry + 0.01, CUT), "rope", sub=1))
    for s in (-1, 1):
        L.add("Binding", box((0.012, 0.012, 0.06), (s * 0.014, ry + 0.016, CUT - 0.035), "rope", rot=(0, math.radians(14 * s), 0)))
    fy = -ry - 0.004
    L.add("Binding", torus(0.015, 0.0045, (0, fy - 0.006, CUT - 0.012), "brass", rot=(R90, 0, 0)))   # shackle
    lock_y, lock_z = fy - 0.016, CUT - 0.045
    L.add("Binding", box((0.044, 0.022, 0.04), (0, lock_y, lock_z), "brass"))
    L.add("Binding", box((0.008, 0.004, 0.014), (0, lock_y - 0.012, lock_z - 0.002), "keyhole"))

    # ---- seal: red wax on the rope, left of the forehead
    t = math.radians(-152)
    p = on_head(t, CUT, 0.014)
    L.add("Seal", cyl(0.022, 0.009, p, "seal", verts=8, rot=(R90, 0, t + R90)))
    L.add("Seal", ico(0.008, on_head(t, CUT, 0.02), "bone", sub=1, scale=(1, 0.5, 1)))   # tiny skull stamp

    # ---- key: inserted in the padlock, shaft along Y
    ky = lock_y - 0.011
    L.add("Key", cyl(0.0042, 0.03, (0, ky - 0.012, lock_z - 0.002), "iron", verts=6, rot=(R90, 0, 0)))
    L.add("Key", torus(0.014, 0.004, (0, ky - 0.04, lock_z - 0.002), "iron", rot=(0, R90, 0)))
    L.add("Key", box((0.003, 0.008, 0.008), (0, ky - 0.026, lock_z - 0.002), "iron"))

    # ---- glow: warm light at the opening with a few coins floating in it
    L.add("Glow", ico(0.1, (0, 0, CUT + 0.01), "glow", sub=2, scale=(1, 1.15, 0.55)))
    for x, y, z, r in ((0.0, -0.02, 0.07, 0.3), (0.045, 0.03, 0.05, -0.6), (-0.05, 0.02, 0.04, 0.8),
                       (0.02, 0.06, 0.09, -1.1), (-0.03, -0.05, 0.1, 1.3)):
        L.add("Glow", cyl(0.022, 0.006, (x, y, CUT + z), "gold", verts=10, rot=(r, r * 0.5, 0)))

    hinge = (0, SY * rad(CUT), CUT)
    return "Chest_HaiTac", L, hinge


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
name, L, hinge = build()
root = bpy.data.objects.new(name, None)
bpy.context.scene.collection.objects.link(root)
tris = {}
for layer, parts in L.parts.items():
    if not parts:
        continue
    o = join(parts)
    o.name = layer
    o.data.name = layer
    if layer == "Lid":
        set_origin(o, hinge)
    elif layer == "Key":
        lock = [v.co for v in o.data.vertices]
        set_origin(o, (0, max(c.y for c in lock), bbox_centre(o).z))     # pivot where the shaft enters the lock
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
zs = [(c.matrix_world @ v.co).z for c in root.children if c.name != "Glow" for v in c.data.vertices]
print("triangles per layer:", tris)
print("total without glow:", sum(v for k, v in tris.items() if k != "Glow"), "height: %.3f m" % max(zs), "->", OUT)

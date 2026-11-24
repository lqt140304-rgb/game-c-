"""Rương Âm nhạc: a mini arched jukebox, built procedurally with Blender (bpy). No Meshy, no image textures.

Run: python make_chest_am_nhac.py -- <output dir>      (writes <output dir>/Chest_AmNhac.glb)
Layers (children of the root Chest_AmNhac), same contract as the other chests for BoxOpenSequence:
  Body     cabinet, speaker grille, buttons, neon pillars, turntable platter and tone arm
  Lid      arched glass dome + skirt, origin on the back hinge, opens by rotating about X
  Seal     red guitar pick stuck over the front seam
  Binding  leather guitar strap with a chrome buckle wrapped over the dome
  Record   the vinyl on the platter, origin at its centre: spin it about the up axis (Unity local Y)
  Glow     warm light ball inside the mouth, hidden by scale
Bottom centre at the origin, metres, Blender Z up and front facing -Y (glTF export converts to Y up).
"""
import math, sys, os
import bpy, bmesh, mathutils

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "."
os.makedirs(OUT, exist_ok=True)
R90 = math.radians(90)

PALETTE = {  # sRGB, converted to linear below. (colour, roughness, metallic, emission strength)
    "walnut": ((0.40, 0.24, 0.14), 0.75, 0.0, 0), "walnut_dk": ((0.25, 0.15, 0.09), 0.8, 0.0, 0),
    "cream": ((0.88, 0.82, 0.68), 0.7, 0.0, 0), "chrome": ((0.74, 0.76, 0.79), 0.35, 0.6, 0),
    "glass": ((0.13, 0.24, 0.28), 0.15, 0.0, 0), "felt": ((0.14, 0.09, 0.08), 0.95, 0.0, 0),
    "vinyl": ((0.05, 0.05, 0.06), 0.35, 0.0, 0), "groove": ((0.12, 0.12, 0.14), 0.3, 0.0, 0),
    "label_red": ((0.78, 0.16, 0.16), 0.6, 0.0, 0), "label_cream": ((0.92, 0.86, 0.70), 0.6, 0.0, 0),
    "leather": ((0.45, 0.26, 0.14), 0.85, 0.0, 0), "pick": ((0.82, 0.13, 0.16), 0.4, 0.0, 0),
    "amber": ((0.95, 0.62, 0.18), 0.5, 0.0, 0.6),
    "neon_pink": ((1.0, 0.28, 0.62), 0.4, 0.0, 1.2), "neon_cyan": ((0.25, 0.92, 0.95), 0.4, 0.0, 1.2),
    "glow": ((1.0, 0.82, 0.45), 0.9, 0.0, 2.0),
}
_mats = {}


def mat(name):
    if name in _mats:
        return _mats[name]
    col, rough, metal, emit = PALETTE[name]
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes["Principled BSDF"]
    c = tuple(x ** 2.2 for x in col)
    bsdf.inputs["Base Color"].default_value = (c[0], c[1], c[2], 1.0)
    bsdf.inputs["Roughness"].default_value = rough
    bsdf.inputs["Metallic"].default_value = metal
    if emit:
        bsdf.inputs["Emission Color"].default_value = (c[0], c[1], c[2], 1.0)
        bsdf.inputs["Emission Strength"].default_value = emit
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


def from_bmesh(bm, material, name="part"):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    o = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(o)
    return finish(o, material)


def box(size, loc, material, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = size
    return finish(o, material)


def cyl(radius, depth, loc, material, verts=12, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def ico(radius, loc, material, sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, radius=radius, location=loc)
    return finish(bpy.context.active_object, material)


def torus(major, minor, loc, material, rot=(0, 0, 0), seg=16, mseg=6, keep_above=None):
    """Torus; keep_above=z cuts away everything below that height (an arch)."""
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, major_segments=seg, minor_segments=mseg, location=loc, rotation=rot)
    o = bpy.context.active_object
    if keep_above is not None:
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.mesh.bisect(plane_co=(0, 0, keep_above), plane_no=(0, 0, 1), clear_inner=True, use_fill=True)
        bpy.ops.object.mode_set(mode="OBJECT")
    return finish(o, material)


def prism(pts, a0, a1, material, plane="xy"):
    """Extrude a closed 2D polygon. plane 'xy': pts are (x, y), extruded along Z from a0 to a1.
    plane 'xz': pts are (x, z), extruded along Y from a0 to a1 (front-view profiles like the arch)."""
    def p3(u, v, a):
        return (u, v, a) if plane == "xy" else (u, a, v)
    bm = bmesh.new()
    lo = [bm.verts.new(p3(u, v, a0)) for u, v in pts]
    hi = [bm.verts.new(p3(u, v, a1)) for u, v in pts]
    n = len(pts)
    bm.faces.new(lo[::-1])
    bm.faces.new(hi)
    for i in range(n):
        j = (i + 1) % n
        bm.faces.new((lo[i], lo[j], hi[j], hi[i]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return from_bmesh(bm, material)


def ring(outer, inner, z0, z1, material):
    """Closed band between two XY loops with the same vertex count (a frame with a hole), extruded along Z."""
    bm = bmesh.new()
    n = len(outer)
    ob = [bm.verts.new((x, y, z0)) for x, y in outer]
    ot = [bm.verts.new((x, y, z1)) for x, y in outer]
    ib = [bm.verts.new((x, y, z0)) for x, y in inner]
    it = [bm.verts.new((x, y, z1)) for x, y in inner]
    for i in range(n):
        j = (i + 1) % n
        bm.faces.new((ob[i], ob[j], ot[j], ot[i]))
        bm.faces.new((it[i], it[j], ib[j], ib[i]))
        bm.faces.new((ot[i], ot[j], it[j], it[i]))
        bm.faces.new((ib[i], ib[j], ob[j], ob[i]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return from_bmesh(bm, material)


def loft(sections, material):
    """Skin closed loops of 3D points (all the same vertex count) in order, capping both ends."""
    bm = bmesh.new()
    rows = [[bm.verts.new(p) for p in sec] for sec in sections]
    n = len(sections[0])
    bm.faces.new(rows[0][::-1])
    bm.faces.new(rows[-1])
    for a, b in zip(rows, rows[1:]):
        for i in range(n):
            j = (i + 1) % n
            bm.faces.new((a[i], a[j], b[j], b[i]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return from_bmesh(bm, material)


def arc(r, a0, a1, segs, cx=0.0, cz=0.0):
    return [(cx + r * math.cos(a0 + (a1 - a0) * i / segs), cz + r * math.sin(a0 + (a1 - a0) * i / segs)) for i in range(segs + 1)]


class Layers:
    def __init__(self):
        self.parts = {"Body": [], "Lid": [], "Seal": [], "Binding": [], "Record": [], "Glow": []}

    def add(self, layer, obj):
        self.parts[layer].append(obj)
        return obj


# ------------------------------------------------------------------------------------------ dimensions
HW = 0.15          # half width of the cabinet
HD = 0.11          # half depth at the floor (the back slopes in towards the top)
HDT = 0.06         # back of the cabinet at the lid seam
BULGE = 0.028      # how far the front bows out in the middle
HB = 0.25          # top of the cabinet (lid seam)
SK = 0.028         # height of the lid skirt that hides the record
R = 0.155          # dome radius (front view arch)
ZC = HB + SK       # dome axis height
SEG = 14


def y_front(x, off=0.0):
    """Front surface of the bowed cabinet at x (negative Y), pushed out by off."""
    t = max(-1.0, min(1.0, x / HW))
    return -HD - BULGE * (1 - t * t) - off


def plan(off=0.0, back=HD, segs=SEG):
    """Top view outline of the cabinet, grown by off on every side; back = Y of the back wall."""
    w, d = HW + off, back + off
    front = [(w - 2 * w * i / segs, None) for i in range(segs + 1)]
    front = [(x, y_front(x * HW / w, off)) for x, _ in front]
    return front + [(-w, d), (w, d)]


def back_y(z):
    return HD - (HD - HDT) * (z - 0.035) / (HB - 0.035)


def front_strip(x0, x1, z0, z1, off0, off1, material, segs=8):
    """Thin curved panel lying on the bowed front between x0..x1, standing off0..off1 out from the surface."""
    xs = [x0 + (x1 - x0) * i / segs for i in range(segs + 1)]
    pts = [(x, y_front(x, off1)) for x in xs] + [(x, y_front(x, off0)) for x in reversed(xs)]
    return prism(pts, z0, z1, material, "xy")


# ------------------------------------------------------------------------------------------ the jukebox
def build():
    L = Layers()
    # --- cabinet
    L.add("Body", prism(plan(0.018), 0.0, 0.035, "walnut_dk"))                      # plinth
    for sx in (-1, 1):
        for y in (-0.095, 0.09):
            L.add("Body", cyl(0.016, 0.012, (sx * 0.13, y, 0.006), "chrome", verts=8))  # feet
    L.add("Body", loft([[(x, y, 0.035) for x, y in plan()], [(x, y, HB) for x, y in plan(0, HDT)]], "walnut"))   # sloped back
    L.add("Body", prism(plan(0.006, back_y(HB - 0.012)), HB - 0.012, HB, "walnut_dk"))  # top lip
    slope = math.atan2(HD - HDT, HB - 0.035)
    for i in range(5):                                                                # vent slats on the back
        z = 0.08 + i * 0.03
        L.add("Body", box((0.17, 0.006, 0.01), (0, back_y(z) + 0.002, z), "chrome", rot=(-slope, 0, 0)))
    L.add("Body", box((0.08, 0.004, 0.03), (0, back_y(0.225) + 0.001, 0.225), "cream", rot=(-slope, 0, 0)))   # maker plate
    L.add("Body", prism(plan(0.006), 0.035, 0.05, "chrome"))                         # chrome kick strip
    # rounded front pillars with pink neon tubes
    for sx in (-1, 1):
        x = sx * (HW - 0.012)
        L.add("Body", cyl(0.032, HB - 0.035, (x, -HD + 0.005, (HB + 0.035) / 2), "walnut_dk", verts=12))
        L.add("Body", cyl(0.011, HB - 0.06, (x, -HD - 0.027, (HB + 0.05) / 2 + 0.005), "neon_pink", verts=8))
        L.add("Body", cyl(0.016, 0.012, (x, -HD - 0.027, 0.057), "chrome", verts=8))
        L.add("Body", cyl(0.016, 0.012, (x, -HD - 0.027, HB - 0.006), "chrome", verts=8))
    # cream speaker panel with chrome grille bars
    L.add("Body", front_strip(-0.1, 0.1, 0.065, 0.17, 0.0, 0.004, "cream"))
    for i in range(6):
        z = 0.078 + i * 0.016
        L.add("Body", front_strip(-0.088, 0.088, z, z + 0.006, 0.004, 0.009, "chrome"))
    L.add("Body", front_strip(-0.004, 0.004, 0.072, 0.165, 0.004, 0.011, "walnut_dk", segs=1))   # centre divider
    # selector strip: chrome band, a row of coloured buttons, coin slot
    L.add("Body", front_strip(-0.105, 0.105, 0.18, 0.235, 0.0, 0.004, "chrome"))
    cols = ["cream", "neon_pink", "amber", "neon_cyan", "cream", "neon_pink", "amber"]
    for i, c in enumerate(cols):
        x = -0.084 + i * 0.022
        if abs(x) < 0.001:
            continue
        L.add("Body", box((0.014, 0.012, 0.014), (x, y_front(x, 0.008), 0.196), c))
    L.add("Body", front_strip(-0.07, 0.07, 0.212, 0.229, 0.004, 0.007, "glass"))        # song card window
    L.add("Body", box((0.026, 0.008, 0.03), (0, y_front(0, 0.008), 0.196), "walnut_dk"))   # coin plate
    L.add("Body", box((0.004, 0.006, 0.018), (0, y_front(0, 0.012), 0.196), "felt"))       # coin slot
    # side chrome stripes
    for sx in (-1, 1):
        L.add("Body", box((0.006, 0.17, 0.012), (sx * (HW + 0.002), -0.01, 0.15), "chrome"))
    # deck: felt top, chrome platter, tone arm (the record itself is its own layer)
    L.add("Body", prism(plan(-0.016, HDT), HB - 0.002, HB + 0.001, "felt"))
    rc = (-0.022, -0.036)
    L.add("Body", cyl(0.082, 0.006, (rc[0], rc[1], HB + 0.004), "chrome", verts=24))
    piv = (0.112, 0.03)
    L.add("Body", cyl(0.014, 0.012, (piv[0], piv[1], HB + 0.007), "chrome", verts=10))
    tip = (rc[0] + 0.058, rc[1] + 0.024)
    dx, dy = tip[0] - piv[0], tip[1] - piv[1]
    ln = math.hypot(dx, dy)
    L.add("Body", box((0.006, ln, 0.005), ((piv[0] + tip[0]) / 2, (piv[1] + tip[1]) / 2, HB + 0.016), "chrome",
                      rot=(0, 0, math.atan2(dy, dx) - R90)))
    L.add("Body", box((0.012, 0.018, 0.006), (tip[0], tip[1], HB + 0.015), "walnut_dk", rot=(0, 0, math.atan2(dy, dx) - R90)))
    for sx in (-1, 1):                                                                # back hinges
        L.add("Body", cyl(0.008, 0.04, (sx * 0.08, HDT + 0.006, HB), "chrome", verts=8, rot=(0, R90, 0)))

    # --- record (spins)
    L.add("Record", cyl(0.077, 0.004, (rc[0], rc[1], HB + 0.009), "vinyl", verts=28))
    for r in (0.066, 0.052):
        L.add("Record", torus(r, 0.0016, (rc[0], rc[1], HB + 0.011), "groove", seg=24, mseg=3))
    L.add("Record", cyl(0.028, 0.002, (rc[0], rc[1], HB + 0.0115), "label_red", verts=16))
    L.add("Record", box((0.054, 0.012, 0.0024), (rc[0], rc[1] + 0.008, HB + 0.0125), "label_cream"))   # stripe shows the spin
    L.add("Record", cyl(0.003, 0.012, (rc[0], rc[1], HB + 0.014), "chrome", verts=6))

    # --- lid: skirt (hides the record) + arched dome
    L.add("Lid", ring(plan(0.004, HDT), plan(-0.01, HDT), HB, HB + SK, "walnut_dk"))
    def dome_sec(y, r, zc=ZC):
        return [(x, y, z) for x, z in arc(r, 0, math.pi, 16, 0, zc)]
    back = HDT + 0.004                                                                # dome rounds off towards the back
    L.add("Lid", loft([dome_sec(-HD - 0.006, R), dome_sec(back - 0.05, R), dome_sec(back - 0.018, R - 0.02),
                       dome_sec(back, R - 0.06)], "walnut"))
    L.add("Lid", prism(arc(R - 0.016, 0, math.pi, 16, 0, ZC), -HD - 0.016, -HD - 0.006, "glass", "xz"))       # front window
    band = arc(R + 0.004, 0, math.pi, 16, 0, ZC) + arc(R - 0.014, math.pi, 0, 16, 0, ZC)
    L.add("Lid", prism(band, -HD - 0.02, -HD - 0.012, "chrome", "xz"))                  # chrome bezel
    L.add("Lid", torus(R - 0.03, 0.0095, (0, -HD - 0.024, ZC), "neon_cyan", rot=(R90, 0, 0), seg=24, keep_above=ZC))
    L.add("Lid", torus(R - 0.052, 0.006, (0, -HD - 0.02, ZC), "neon_pink", rot=(R90, 0, 0), seg=20, keep_above=ZC))
    for sx in (-1, 1):                                                                # pink pillars carried across the skirt
        L.add("Lid", cyl(0.011, SK, (sx * (HW - 0.012), -HD - 0.027, HB + SK / 2), "neon_pink", verts=8))
    cream_band = arc(R + 0.003, 0, math.pi, 16, 0, ZC) + arc(R - 0.004, math.pi, 0, 16, 0, ZC)
    for y in (-0.085, -0.045):                                                           # cream stripes over the dome
        L.add("Lid", prism(cream_band, y, y + 0.018, "cream", "xz"))
    # little record inside the front window (a jukebox shows what it plays)
    L.add("Lid", cyl(0.05, 0.004, (0, -HD - 0.017, ZC + 0.055), "vinyl", verts=20, rot=(R90, 0, 0)))
    L.add("Lid", cyl(0.018, 0.005, (0, -HD - 0.018, ZC + 0.055), "label_red", verts=12, rot=(R90, 0, 0)))
    # chrome crest on top
    L.add("Lid", prism([(-0.032, ZC + R - 0.004), (0.032, ZC + R - 0.004), (0.012, ZC + R + 0.026), (-0.012, ZC + R + 0.026)],
                       -0.06, -0.02, "chrome", "xz"))
    L.add("Lid", ico(0.011, (0, -0.04, ZC + R + 0.03), "amber"))

    # --- binding: leather guitar strap over the dome and down both sides, chrome buckle on top
    sw = 0.022
    strap = arc(R + 0.011, 0, math.pi, 18, 0, ZC) + arc(R + 0.003, math.pi, 0, 18, 0, ZC)
    SY = 0.004                                                                        # strap position (front edge, Y)
    L.add("Binding", prism(strap, SY, SY + sw, "leather", "xz"))
    for sx in (-1, 1):
        L.add("Binding", box((0.008, sw, ZC - 0.07), (sx * (R + 0.007), SY + sw / 2, (ZC + 0.07) / 2), "leather"))
        L.add("Binding", box((0.012, sw + 0.006, 0.012), (sx * (R + 0.009), SY + sw / 2, 0.075), "chrome"))
    top = ZC + R + 0.011
    L.add("Binding", ring([(-0.02, SY - 0.008), (0.02, SY - 0.008), (0.02, SY + sw + 0.008), (-0.02, SY + sw + 0.008)],
                          [(-0.013, SY - 0.002), (0.013, SY - 0.002), (0.013, SY + sw + 0.002), (-0.013, SY + sw + 0.002)],
                          top - 0.002, top + 0.006, "chrome"))
    L.add("Binding", box((0.003, sw + 0.012, 0.004), (0, SY + sw / 2, top + 0.006), "chrome"))
    for i in range(5):                                                                # stitching dots
        a = math.radians(30 + i * 30)
        L.add("Binding", box((0.006, 0.003, 0.006), ((R + 0.012) * math.cos(a), SY - 0.002, ZC + (R + 0.012) * math.sin(a)), "cream",
                             rot=(0, -a, 0)))

    # --- seal: red guitar pick stuck over the lid seam
    pick = [(0.0, HB - 0.034)] + [(0.032 * math.cos(a), HB + 0.008 + 0.022 * math.sin(a))
                                  for a in [math.radians(d) for d in (-20, 10, 40, 70, 90, 110, 140, 170, 200)]]
    yf = y_front(0, 0.0) - 0.011
    L.add("Seal", prism(pick, yf - 0.004, yf, "pick", "xz"))
    L.add("Seal", cyl(0.009, 0.003, (0, yf - 0.005, HB + 0.006), "cream", verts=10, rot=(R90, 0, 0)))

    # --- glow
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=0.06, location=(rc[0], rc[1], HB + 0.014))
    bpy.context.active_object.scale = (1, 1, 0.45)                                   # flat halo over the record
    L.add("Glow", finish(bpy.context.active_object, "glow"))
    return L, (0, HDT + 0.006, HB), (rc[0], rc[1], HB + 0.009)


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


def tri_count(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


bpy.ops.wm.read_factory_settings(use_empty=True)
L, hinge, record_centre = build()
root = bpy.data.objects.new("Chest_AmNhac", None)
bpy.context.scene.collection.objects.link(root)
total = 0
for layer, parts in L.parts.items():
    o = join(parts)
    o.name = layer
    o.data.name = layer
    if layer == "Lid":
        set_origin(o, hinge)
    elif layer == "Record":
        set_origin(o, record_centre)
    elif layer in ("Seal", "Binding", "Glow"):
        set_origin(o, bbox_centre(o))
    o.parent = root
    n = tri_count(o)
    print(f"  {layer:8s} {n:5d} tris")
    total += n if layer != "Glow" else 0
bpy.ops.object.select_all(action="DESELECT")
root.select_set(True)
for c in root.children:
    c.select_set(True)
bpy.context.view_layer.objects.active = root
path = os.path.join(OUT, "Chest_AmNhac.glb")
bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
print("Chest_AmNhac triangles (without glow):", total, "->", path)

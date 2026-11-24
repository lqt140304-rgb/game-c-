"""Rương Nhật Bản cổ (bản tạm): búp bê Daruma, chỏm đầu là nắp.

Chạy:  python Tools/Blender/make_chest_nhat_ban.py      (cần `pip install bpy`)
Xuất:  Assets/_Project/Art/Chests/Chest_NhatBan.glb
Lớp:   Chest_NhatBan (root) -> Body, Lid (gốc ở bản lề sau, xoay trục X), Binding, Seal, Glow
Đáy giữa ở gốc toạ độ, mét, Blender Z lên, mặt trước là -Y.
"""
import math, os
import bpy, bmesh, mathutils

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Chests", "Chest_NhatBan.glb")
SEG = 20
CUT = 0.30          # height of the lid seam

PALETTE = {
    "lacquer": (0.62, 0.17, 0.13), "lacquer_dk": (0.40, 0.10, 0.08), "face": (0.90, 0.85, 0.74),
    "ink": (0.06, 0.05, 0.05), "gold": (0.78, 0.60, 0.26), "wood": (0.13, 0.10, 0.08),
    "paper": (0.88, 0.82, 0.66), "interior": (0.08, 0.05, 0.04), "glow": (1.0, 0.82, 0.45),
}
_m = {}


def mat(n):
    if n in _m:
        return _m[n]
    m = bpy.data.materials.new(n); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]; c = tuple(x ** 2.2 for x in PALETTE[n])
    b.inputs["Base Color"].default_value = (*c, 1)
    b.inputs["Roughness"].default_value = 0.35 if n in ("lacquer", "lacquer_dk") else 0.8
    b.inputs["Metallic"].default_value = 0.8 if n == "gold" else 0.0
    if n == "glow":
        b.inputs["Emission Color"].default_value = (*c, 1); b.inputs["Emission Strength"].default_value = 2.0
    _m[n] = m
    return m


def lathe(name, prof, material, seg=SEG, sy=1.0):
    """Surface of revolution from (radius, z) points; radius 0 collapses to a pole."""
    bm = bmesh.new(); rings = []
    for r, z in prof:
        if r == 0:
            rings.append([bm.verts.new((0, 0, z))])
        else:
            rings.append([bm.verts.new((r * math.cos(2 * math.pi * i / seg), r * sy * math.sin(2 * math.pi * i / seg), z)) for i in range(seg)])
    for a, b in zip(rings, rings[1:]):
        for i in range(seg):
            j = (i + 1) % seg
            q = [v for v in (a[i % len(a)], a[j % len(a)], b[j % len(b)], b[i % len(b)])]
            q = list(dict.fromkeys(q))
            if len(q) >= 3:
                bm.faces.new(q)
    me = bpy.data.meshes.new(name); bm.normal_update(); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(o)
    me.materials.append(mat(material))
    return o


def prim(op, material, loc, rot=(0, 0, 0), scale=(1, 1, 1), **kw):
    op(location=loc, rotation=rot, **kw)
    o = bpy.context.active_object; o.scale = scale
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    o.data.materials.clear(); o.data.materials.append(mat(material))
    for p in o.data.polygons:
        p.use_smooth = False
    o.select_set(False)
    return o


def front(z, x=0.0, inset=0.0):
    """Point on the egg surface at the front (-Y) for height z."""
    r = body_r(z)
    return (x, -math.sqrt(max(r * r - x * x, 0)) * 0.92 + inset, z)


BODY = [(0, 0.035), (0.13, 0.035), (0.175, 0.07), (0.20, 0.13), (0.205, 0.19), (0.195, 0.25), (0.172, CUT)]
LIDP = [(0.172, CUT), (0.145, 0.345), (0.105, 0.385), (0.055, 0.408), (0, 0.415)]


def body_r(z):
    pts = BODY[1:] + LIDP[1:]
    for (r0, z0), (r1, z1) in zip(pts, pts[1:]):
        if z0 <= z <= z1:
            return r0 + (r1 - r0) * (z - z0) / (z1 - z0)
    return 0.1


L = {"Body": [], "Lid": [], "Binding": [], "Seal": [], "Glow": []}
bpy.ops.wm.read_factory_settings(use_empty=True)

# --- body: lacquered egg, hollow mouth with an inner lip, round black wood base
L["Body"].append(lathe("shell", BODY + [(0.150, CUT), (0.150, 0.26), (0.11, 0.17), (0, 0.16)], "lacquer", sy=0.92))
L["Body"].append(lathe("inner", [(0, 0.162), (0.12, 0.17), (0.148, 0.255)], "interior", sy=0.92))
L["Body"].append(lathe("base", [(0, 0.0), (0.15, 0.0), (0.155, 0.02), (0.135, 0.038), (0, 0.038)], "wood", seg=12))
# face: decals wrapped onto the curved egg (no flat plates), so it reads as a Daruma from far away:
# cream face framed by a gold trim, big round eyes (left painted, right still blank: wish not granted yet),
# thick "crane" brows, curly "turtle" moustache and a beard fringe.
def surf(u, z, off):
    """Map (arc length u along the front, height z) onto the egg surface, pushed out by off."""
    r = body_r(z) + off
    phi = u / max(r, 1e-3)
    return (r * math.sin(phi), -0.92 * r * math.cos(phi), z)


def decal(name, pts2d, faces, material, off):
    """2D shape (u, v) in metres, wrapped onto the surface; v is height."""
    me = bpy.data.meshes.new(name)
    me.from_pydata([surf(u, v, off) for u, v in pts2d], [], faces)
    o = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(o)
    me.materials.append(mat(material))
    return o


def ellipse(cu, cv, a, b, material, off, n=16, rings=3, hole=0.0):
    """Disc (or ring when hole > 0) subdivided in rings so it bends with the surface."""
    pts, faces = [], []
    radii = [hole + (1 - hole) * k / rings for k in range(rings + 1)]
    start = 0
    if hole == 0:
        pts.append((cu, cv)); radii = radii[1:]
    for k, t in enumerate(radii):
        for i in range(n):
            ang = 2 * math.pi * i / n
            pts.append((cu + a * t * math.cos(ang), cv + b * t * math.sin(ang)))
    if hole == 0:
        faces += [(0, 1 + i, 1 + (i + 1) % n) for i in range(n)]
        start = 1
    for k in range(len(radii) - 1):
        a0, b0 = start + k * n, start + (k + 1) * n
        faces += [(a0 + i, b0 + i, b0 + (i + 1) % n, a0 + (i + 1) % n) for i in range(n)]
    return decal("e", pts, faces, material, off)


def stroke(centre, width, material, off):
    """Thick brush stroke along a 2D polyline; width tapers at both ends."""
    pts, faces, m = [], [], len(centre)
    for i, (u, v) in enumerate(centre):
        u0, v0 = centre[max(i - 1, 0)]; u1, v1 = centre[min(i + 1, m - 1)]
        du, dv = u1 - u0, v1 - v0; ln = math.hypot(du, dv) or 1
        w = width * (0.35 + 0.65 * math.sin(math.pi * (i + 0.5) / m))
        pts += [(u - dv / ln * w / 2, v + du / ln * w / 2), (u + dv / ln * w / 2, v - du / ln * w / 2)]
    faces = [(2 * i, 2 * i + 1, 2 * i + 3, 2 * i + 2) for i in range(m - 1)]
    return decal("s", pts, faces, material, off)


def arc(cu, cv, r, a0, a1, n=7, sy=1.0):
    return [(cu + r * math.cos(math.radians(a0 + (a1 - a0) * i / (n - 1))), cv + sy * r * math.sin(math.radians(a0 + (a1 - a0) * i / (n - 1)))) for i in range(n)]


fz = 0.19
B = L["Body"]
B.append(ellipse(0, fz, 0.105, 0.072, "face", 0.002, n=20, rings=3))                    # cream face
B.append(ellipse(0, fz, 0.118, 0.084, "gold", 0.0035, n=20, rings=1, hole=0.885))        # gold frame
for sx, painted in ((-1, True), (1, False)):
    eu = sx * 0.042
    B.append(ellipse(eu, fz + 0.008, 0.026, 0.026, "ink", 0.004, n=12, rings=1, hole=0.8))   # eye outline
    if painted:
        B.append(ellipse(eu, fz + 0.008, 0.016, 0.016, "ink", 0.005, n=10, rings=1))          # painted pupil
    # crane brows: thick sweeping arcs that flick upward at the outer end
    brow = [(sx * u, fz + 0.045 + 0.012 * math.sin(math.pi * k / 6) + (0.01 if k > 4 else 0))
            for k, u in enumerate([0.012, 0.024, 0.036, 0.048, 0.06, 0.07, 0.078])]
    B.append(stroke(brow, 0.016, "ink", 0.005))
    # turtle moustache: curl from the nose outward and up into a swirl
    must = [(sx * 0.006, fz - 0.026)] + [(sx * u, v) for u, v in arc(0.034, fz - 0.024, 0.024, 200, 360, 6)][1:] \
        + [(sx * u, v) for u, v in arc(0.050, fz - 0.016, 0.009, 0, 200, 5)][1:]
    B.append(stroke(must, 0.010, "ink", 0.005))
# nose (a chin-beard fringe was tried and read as skull teeth, so it was dropped)
B.append(ellipse(0, fz - 0.012, 0.008, 0.006, "lacquer", 0.005, n=8, rings=1))
# gold belly emblem: round medallion with a stylised "fortune" mark, wrapped too
bz = 0.085
B.append(ellipse(0, bz, 0.034, 0.03, "gold", 0.003, n=12, rings=2))
B.append(stroke([(-0.016, bz + 0.012), (0.016, bz + 0.012)], 0.006, "lacquer_dk", 0.0045))
B.append(stroke([(0, bz + 0.02), (0, bz - 0.02)], 0.006, "lacquer_dk", 0.0045))
B.append(stroke(arc(0, bz - 0.004, 0.013, 200, 340, 5), 0.006, "lacquer_dk", 0.0045))
# gold side swirls (stylised cheek clouds)
for sx in (-1, 1):
    L["Body"].append(prim(bpy.ops.mesh.primitive_torus_add, "gold", (sx * 0.19, 0, 0.17), rot=(0, math.radians(90), 0), major_radius=0.03, minor_radius=0.006, major_segments=10, minor_segments=4))

# --- lid: crown of the head, with a top knot and gold rim
L["Lid"].append(lathe("crown", LIDP + [], "lacquer", sy=0.92))
L["Lid"].append(lathe("lid_under", [(0.172, CUT), (0.147, CUT), (0.147, CUT - 0.015), (0.13, CUT - 0.015), (0, CUT - 0.015)], "lacquer_dk", sy=0.92))
L["Lid"].append(prim(bpy.ops.mesh.primitive_torus_add, "gold", (0, 0, CUT + 0.005), scale=(1, 0.92, 1), major_radius=0.172, minor_radius=0.009, major_segments=SEG, minor_segments=4))
L["Lid"].append(prim(bpy.ops.mesh.primitive_uv_sphere_add, "lacquer_dk", (0, 0.0, 0.43), scale=(1, 1, 0.7), segments=8, ring_count=5, radius=0.03))

# --- binding: gold cord knotted at the back, tassels hang behind
L["Binding"].append(prim(bpy.ops.mesh.primitive_torus_add, "gold", (0, 0, CUT - 0.012), scale=(1, 0.92, 1), major_radius=0.18, minor_radius=0.007, major_segments=SEG, minor_segments=4))
for sx in (-1, 1):
    L["Binding"].append(prim(bpy.ops.mesh.primitive_cone_add, "gold", (sx * 0.03, 0.185, CUT - 0.06), rot=(math.radians(-10), 0, 0), vertices=6, radius1=0.014, radius2=0.004, depth=0.07))

# --- seal: paper ofuda strip across the seam at the front
sz = CUT + 0.0
L["Seal"].append(prim(bpy.ops.mesh.primitive_cube_add, "paper", (0.0, -0.175 * 0.92 - 0.004, sz), rot=(math.radians(14), 0, 0), scale=(0.022, 0.003, 0.06), size=2))
L["Seal"].append(prim(bpy.ops.mesh.primitive_cube_add, "lacquer", (0.0, -0.175 * 0.92 - 0.0075, sz), rot=(math.radians(14), 0, 0), scale=(0.01, 0.002, 0.01), size=2))

# --- glow at the mouth
L["Glow"].append(prim(bpy.ops.mesh.primitive_ico_sphere_add, "glow", (0, 0, CUT - 0.02), scale=(1, 0.92, 0.45), subdivisions=2, radius=0.13))


def join(parts):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    return bpy.context.active_object


def set_origin(o, p):
    p = mathutils.Vector(p)
    for v in o.data.vertices:
        v.co -= p
    o.location = p


root = bpy.data.objects.new("Chest_NhatBan", None); bpy.context.scene.collection.objects.link(root)
tris = 0
for layer, parts in L.items():
    o = join(parts); o.name = layer
    if layer == "Lid":
        set_origin(o, (0, 0.172 * 0.92, CUT))           # hinge at the back of the seam
    elif layer != "Body":
        xs = [v.co for v in o.data.vertices]
        set_origin(o, sum(xs, mathutils.Vector()) / len(xs))
    o.parent = root
    if layer != "Glow":
        tris += sum(len(p.vertices) - 2 for p in o.data.polygons)
bpy.ops.object.select_all(action="DESELECT")
root.select_set(True)
for c in root.children:
    c.select_set(True)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
bpy.ops.export_scene.gltf(filepath=OUT, export_format="GLB", use_selection=True, export_apply=True)
print("Chest_NhatBan triangles (without glow):", tris, "->", OUT)

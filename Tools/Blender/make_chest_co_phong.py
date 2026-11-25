"""Rương Cổ Phong (bản tạm): hũ gốm men ngọc bụng tròn, nắp vòm có núm đồng, đai tre, nhãn thư pháp, phong giấy đỏ.

Run: python make_chest_co_phong.py -- <output dir>      (pip install bpy; Blender 5.x as a module)
Output: <out>/Chest_CoPhong.glb
  Chest_CoPhong (root) -> Body, Lid (origin on the back hinge, rotates about X, front is -Y),
                          Seal (red paper strip over the lid seam), Binding (bamboo bands), Glow (disc in the mouth)
Bottom centre at the origin, metres, Blender Z up (glTF export converts to Y up).
"""
import math, sys, os
import bpy, bmesh
from mathutils import Vector, Matrix

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "."
os.makedirs(OUT, exist_ok=True)
SEG = 18  # lathe segments

PALETTE = {  # sRGB
    "celadon": (0.56, 0.63, 0.53), "celadon_dk": (0.40, 0.47, 0.40), "clay": (0.55, 0.42, 0.31),
    "bronze": (0.50, 0.38, 0.20), "bamboo": (0.66, 0.58, 0.36), "bamboo_dk": (0.47, 0.40, 0.23),
    "paper": (0.84, 0.78, 0.63), "ink": (0.08, 0.07, 0.07), "seal": (0.62, 0.15, 0.12),
    "interior": (0.10, 0.09, 0.07), "glow": (1.0, 0.82, 0.48),
}
_mats = {}


def mat(name):
    if name not in _mats:
        m = bpy.data.materials.new(name)
        m.use_nodes = True
        b = m.node_tree.nodes["Principled BSDF"]
        c = tuple(x ** 2.2 for x in PALETTE[name])
        b.inputs["Base Color"].default_value = (*c, 1)
        b.inputs["Roughness"].default_value = {"celadon": 0.35, "celadon_dk": 0.4, "bronze": 0.45}.get(name, 0.85)
        b.inputs["Metallic"].default_value = 0.6 if name == "bronze" else 0.0
        if name == "glow":
            b.inputs["Emission Color"].default_value = (*c, 1)
            b.inputs["Emission Strength"].default_value = 2.0
        _mats[name] = m
    return _mats[name]


def obj_from_bm(bm, material, name="part"):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    me.materials.append(mat(material))
    o = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(o)
    return o


def lathe(profile, material, seg=SEG, cap_bottom=False, cap_top=False, phase=0.0):
    """profile: list of (r, z) bottom->top; revolve around Z."""
    bm = bmesh.new()
    rings = []
    for r, z in profile:
        ring = []
        for i in range(seg):
            a = 2 * math.pi * i / seg + phase
            ring.append(bm.verts.new((r * math.cos(a), r * math.sin(a), z)))
        rings.append(ring)
    for k in range(len(rings) - 1):
        A, B = rings[k], rings[k + 1]
        for i in range(seg):
            j = (i + 1) % seg
            bm.faces.new((A[i], A[j], B[j], B[i]))
    if cap_bottom:
        bm.faces.new(list(reversed(rings[0])))
    if cap_top:
        bm.faces.new(rings[-1])
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
    return obj_from_bm(bm, material)


def ring(r_mid, z, thick, height, material, seg=SEG):
    """A band (short tube with thickness) around Z."""
    ri, ro = r_mid - thick / 2, r_mid + thick / 2
    return lathe([(ri, z - height / 2), (ro, z - height / 2), (ro, z + height / 2), (ri, z + height / 2), (ri, z - height / 2)], material, seg)


def cube(size, loc, material, rot=(0, 0, 0)):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1)
    M = Matrix.Translation(loc) @ Matrix.Rotation(rot[2], 4, "Z") @ Matrix.Rotation(rot[1], 4, "Y") @ Matrix.Rotation(rot[0], 4, "X") \
        @ Matrix.Diagonal((*size, 1))
    bmesh.ops.transform(bm, matrix=M, verts=bm.verts)
    return obj_from_bm(bm, material)


def cyl(r, depth, loc, material, axis="Z", seg=8):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=seg, radius1=r, radius2=r, depth=depth)
    R = {"Z": Matrix(), "X": Matrix.Rotation(math.pi / 2, 4, "Y"), "Y": Matrix.Rotation(math.pi / 2, 4, "X")}[axis]
    bmesh.ops.transform(bm, matrix=Matrix.Translation(loc) @ R, verts=bm.verts)
    return obj_from_bm(bm, material)


def on_surface(r_at, z, angle_deg=-90.0):
    """point on the body surface at height z, facing direction angle (−90° = front −Y)."""
    a = math.radians(angle_deg)
    return Vector((r_at * math.cos(a), r_at * math.sin(a), z))


# ---------------------------------------------------------------- build
bpy.ops.wm.read_factory_settings(use_empty=True)
P = {"Body": [], "Lid": [], "Seal": [], "Binding": [], "Glow": []}
MOUTH_Z, MOUTH_R = 0.335, 0.088

# body: foot ring, round belly, shoulder, short neck, lip; inner wall + floor (open mouth)
P["Body"].append(lathe([
    (0.0, 0.0), (0.095, 0.0), (0.105, 0.018), (0.098, 0.026), (0.13, 0.05), (0.165, 0.10), (0.182, 0.155),
    (0.178, 0.205), (0.158, 0.255), (0.122, 0.292), (0.092, 0.306), (0.088, 0.322), (0.097, 0.33), (MOUTH_R + 0.004, MOUTH_Z),
    (MOUTH_R - 0.012, MOUTH_Z), (MOUTH_R - 0.014, 0.30), (0.0, 0.24)], "celadon"))
P["Body"].append(lathe([(0.105, 0.0), (0.108, 0.018), (0.101, 0.026)], "clay"))   # unglazed foot (raw clay)
P["Body"].append(lathe([(0.0, 0.245), (MOUTH_R - 0.015, 0.302)], "interior", cap_bottom=False))  # dark inside
P["Body"].append(ring(0.185, 0.18, 0.006, 0.006, "celadon_dk"))   # incised belly lines
P["Body"].append(ring(0.124, 0.29, 0.008, 0.008, "bronze"))       # bronze shoulder band
# back hinge: two lugs on body, pin
HY = MOUTH_R + 0.014
for x in (-0.03, 0.03):
    P["Body"].append(cube((0.018, 0.022, 0.03), (x, HY, MOUTH_Z - 0.012), "bronze"))
P["Body"].append(cyl(0.006, 0.09, (0, HY + 0.004, MOUTH_Z + 0.004), "bronze", axis="X", seg=6))
# two bronze side ears (loop handles) on the shoulder — breaks the round silhouette
for ang in (0, 180):
    p = on_surface(0.16, 0.255, ang)
    o = (Vector((math.cos(math.radians(ang)), math.sin(math.radians(ang)), 0)))
    P["Body"].append(cube((0.03, 0.022, 0.02), p + o * 0.012, "bronze", rot=(0, 0, math.radians(ang))))
    tor = ring(  # ring handle
        0.024, 0, 0.007, 0.007, "bronze", seg=10)
    tor.matrix_world = Matrix.Translation(p + o * 0.03 + Vector((0, 0, -0.022))) @ Matrix.Rotation(math.radians(ang), 4, "Z") @ Matrix.Rotation(math.pi / 2, 4, "X")
    P["Body"].append(tor)
# calligraphy label on the front of the belly (paper plaque + ink strokes)
lab_y = -0.183
P["Body"].append(cube((0.07, 0.006, 0.11), (0, lab_y + 0.002, 0.15), "paper", rot=(math.radians(-3), 0, 0)))
for dx, dz, w, h, r in [(0, 0.035, 0.035, 0.007, 0.1), (0.005, 0.012, 0.008, 0.03, 0), (-0.012, -0.005, 0.03, 0.006, -0.2),
                         (0.01, -0.025, 0.006, 0.03, 0.3), (-0.01, -0.035, 0.022, 0.006, 0.1)]:
    P["Body"].append(cube((w, 0.004, h), (dx, lab_y - 0.002, 0.15 + dz), "ink", rot=(0, r, 0)))
P["Body"].append(cube((0.016, 0.004, 0.016), (0.022, lab_y - 0.002, 0.108), "seal"))  # red name chop

# lid: flanged dome + bronze knob in the shape of a little seal stamp
P["Lid"].append(lathe([
    (MOUTH_R - 0.018, MOUTH_Z - 0.012), (MOUTH_R - 0.016, MOUTH_Z), (MOUTH_R + 0.012, MOUTH_Z + 0.001), (MOUTH_R + 0.014, MOUTH_Z + 0.012),
    (0.085, 0.36), (0.06, 0.385), (0.03, 0.396), (0.0, 0.398)], "celadon", cap_bottom=True))
P["Lid"].append(lathe([(0.026, 0.396), (0.03, 0.402), (0.02, 0.41), (0.014, 0.418), (0.024, 0.43), (0.02, 0.44), (0.0, 0.443)], "bronze", seg=8))
P["Lid"].append(cube((0.02, 0.016, 0.012), (0, MOUTH_R + 0.01, MOUTH_Z + 0.006), "bronze"))  # hinge leaf on lid

# binding: two bamboo bands around belly with knotted tie at front
for z, rr in ((0.11, 0.172), (0.225, 0.172)):
    P["Binding"].append(ring(rr + 0.004, z, 0.008, 0.016, "bamboo", seg=SEG))
P["Binding"].append(cube((0.012, 0.01, 0.13), (0.065, -0.155, 0.168), "bamboo_dk", rot=(0, 0, math.radians(20))))
P["Binding"].append(cube((0.022, 0.014, 0.02), (0.07, -0.168, 0.225), "bamboo_dk"))

# seal: red paper strip over the lid seam at the front, with a stamped square
P["Seal"].append(cube((0.026, 0.004, 0.07), (0, -MOUTH_R - 0.019, MOUTH_Z - 0.004), "seal", rot=(math.radians(-16), 0, 0)))
P["Seal"].append(cube((0.016, 0.003, 0.016), (0, -MOUTH_R - 0.023, MOUTH_Z - 0.008), "paper", rot=(math.radians(-16), 0, 0)))

# glow: disc in the mouth
P["Glow"].append(lathe([(0.0, MOUTH_Z - 0.01), (MOUTH_R - 0.016, MOUTH_Z - 0.01)], "glow", cap_bottom=False))

HINGE = Vector((0, HY + 0.004, MOUTH_Z + 0.004))


def join(parts, name):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    if len(parts) > 1:
        bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = name
    o.data.name = name
    return o


def set_origin(o, p):
    o.data.transform(Matrix.Translation(-p))
    o.location = p


root = bpy.data.objects.new("Chest_CoPhong", None)
bpy.context.scene.collection.objects.link(root)
tris = 0
for layer, parts in P.items():
    o = join(parts, layer)
    for f in o.data.polygons:
        f.use_smooth = layer in ("Body", "Lid") and f.material_index == 0
    if layer == "Lid":
        set_origin(o, HINGE)
    elif layer != "Body":
        c = sum((Vector(v) for v in o.bound_box), Vector()) / 8
        set_origin(o, c)
    o.parent = root
    if layer != "Glow":
        tris += sum(len(p.vertices) - 2 for p in o.data.polygons)

bpy.ops.object.select_all(action="DESELECT")
root.select_set(True)
for c in root.children:
    c.select_set(True)
path = os.path.join(OUT, "Chest_CoPhong.glb")
bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
print("Chest_CoPhong triangles (without glow):", tris, "->", path)

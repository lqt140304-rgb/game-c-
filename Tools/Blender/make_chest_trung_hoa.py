"""Chest_TrungHoa (temporary): a bronze three-legged ding cauldron with a domed lid and a tiny tiled-roof knob.

Run: python make_chest_trung_hoa.py -- <output dir>
Layers: Chest_TrungHoa (root) -> Body, Lid (origin on back hinge, rotates about X, front is -Y), Glow, Seal, Binding.
Bottom centre at origin, metres, Blender Z up (glTF export converts to Y up). Flat colours, no textures.
"""
import math, sys, os
import bpy, bmesh, mathutils

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "."
os.makedirs(OUT, exist_ok=True)
PALETTE = {
    "bronze": (0.36, 0.42, 0.35), "bronze_dk": (0.22, 0.27, 0.23), "gilt": (0.70, 0.56, 0.28),
    "red": (0.62, 0.13, 0.10), "cord": (0.55, 0.45, 0.30), "tile": (0.20, 0.23, 0.22),
    "glow": (1.0, 0.82, 0.45), "interior": (0.06, 0.05, 0.04),
}
_m = {}
def mat(n):
    if n in _m: return _m[n]
    m = bpy.data.materials.new(n); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]; c = tuple(x ** 2.2 for x in PALETTE[n])
    b.inputs["Base Color"].default_value = (*c, 1)
    b.inputs["Roughness"].default_value = 0.55 if n in ("bronze", "bronze_dk", "gilt") else 0.85
    b.inputs["Metallic"].default_value = 0.6 if n in ("bronze", "bronze_dk", "gilt") else 0.0
    if n == "glow":
        b.inputs["Emission Color"].default_value = (*c, 1); b.inputs["Emission Strength"].default_value = 1.2
    _m[n] = m; return m

def mk(bm, material):
    me = bpy.data.meshes.new("m"); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new("o", me); bpy.context.scene.collection.objects.link(o)
    o.data.materials.append(mat(material)); return o

def lathe(prof, material, seg=16, cap_top=False, cap_bot=False, loc=(0, 0, 0), sx=1.0):
    """Revolve a list of (r, z) points around Z."""
    bm = bmesh.new(); rings = []
    for r, z in prof:
        rings.append([bm.verts.new((loc[0] + r * sx * math.cos(2 * math.pi * i / seg), loc[1] + r * math.sin(2 * math.pi * i / seg), loc[2] + z)) for i in range(seg)])
    for a, b in zip(rings, rings[1:]):
        for i in range(seg):
            j = (i + 1) % seg; bm.faces.new((a[i], a[j], b[j], b[i]))
    if cap_bot: bm.faces.new(list(reversed(rings[0])))
    if cap_top: bm.faces.new(rings[-1])
    return mk(bm, material)

def prim(kind, material, loc, rot=(0, 0, 0), scale=(1, 1, 1), **kw):
    getattr(bpy.ops.mesh, f"primitive_{kind}_add")(location=loc, rotation=rot, **kw)
    o = bpy.context.active_object; o.scale = scale
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    o.data.materials.clear(); o.data.materials.append(mat(material)); return o

L = {"Body": [], "Lid": [], "Glow": [], "Seal": [], "Binding": []}
RIM_Z, RIM_R = 0.30, 0.165

# ---- Body: bulging bowl on three legs
L["Body"].append(lathe([(0.0, 0.115), (0.10, 0.115), (0.145, 0.15), (0.168, 0.21), (0.170, 0.25), (0.162, 0.285),
                        (0.178, 0.292), (0.178, RIM_Z), (0.150, RIM_Z), (0.148, 0.20), (0.0, 0.20)], "bronze", seg=16))
L["Body"].append(lathe([(0.171, 0.215), (0.176, 0.222), (0.176, 0.252), (0.171, 0.258)], "gilt", seg=16))   # pattern band
L["Body"].append(lathe([(0.149, 0.201), (0.0, 0.201)], "interior", seg=16))
for k in range(12):                                                    # raised studs on the band
    a = 2 * math.pi * (k + 0.5) / 12
    L["Body"].append(prim("cube", "bronze_dk", (0.18 * math.cos(a), 0.18 * math.sin(a), 0.237), rot=(0, 0, a), scale=(0.008, 0.014, 0.012), size=1))
for k, a_deg in enumerate((-90 - 60, -90 + 60, 90)):                  # two legs front, one at the back
    a = math.radians(a_deg); r1 = 0.122; c, sn = math.cos(a), math.sin(a)
    # leg grows out of the belly: wide flared root buried in the bowl, slim waist, flared hoof foot
    L["Body"].append(lathe([(0.030, 0.0), (0.034, 0.010), (0.022, 0.030), (0.018, 0.070), (0.026, 0.115), (0.044, 0.150), (0.040, 0.175)],
                           "bronze", seg=8, cap_bot=True, loc=(r1 * c, r1 * sn, 0)))
    # taotie mask plate where the leg meets the bowl, facing outward
    L["Body"].append(prim("cube", "bronze_dk", (0.152 * c, 0.152 * sn, 0.150), rot=(0, 0, a), scale=(0.012, 0.050, 0.034), size=1))
    for dz in (-0.01, 0.01):
        L["Body"].append(prim("cube", "gilt", (0.159 * c, 0.159 * sn, 0.150 + dz), rot=(0, 0, a), scale=(0.004, 0.034, 0.005), size=1))
for sx in (-1, 1):                                                     # upright ears outboard of the rim
    x = sx * 0.19
    for dy in (-0.045, 0.045):
        L["Body"].append(prim("cube", "bronze", (x, dy, 0.335), scale=(0.018, 0.016, 0.085), size=1))
    L["Body"].append(prim("cube", "bronze", (x, 0, 0.385), scale=(0.022, 0.11, 0.022), size=1))
    L["Body"].append(prim("cube", "bronze", (sx * 0.178, 0, 0.297), scale=(0.03, 0.11, 0.012), size=1))

# ---- Lid: dome sitting inside the mouth, tiled-roof knob on top
L["Lid"].append(lathe([(0.147, RIM_Z - 0.01), (0.158, RIM_Z + 0.004), (0.150, 0.33), (0.120, 0.36), (0.07, 0.38), (0.035, 0.385)],
                      "bronze", seg=16, cap_top=True))
L["Lid"].append(lathe([(0.160, RIM_Z + 0.004), (0.162, RIM_Z + 0.012), (0.150, RIM_Z + 0.018)], "gilt", seg=16))
L["Lid"].append(lathe([(0.028, 0.384), (0.022, 0.415)], "gilt", seg=8))                 # neck
def roof(z, half, h, eave):
    """Four-sided roof with upturned corners (low poly pavilion roof)."""
    bm = bmesh.new()
    top = bm.verts.new((0, 0, z + h)); ring = []
    for i in range(8):
        a = math.pi / 4 * i; corner = i % 2 == 0
        r = half * (1.45 if corner else 1.0); a2 = a + math.pi / 4
        ring.append(bm.verts.new((r * math.cos(a2), r * math.sin(a2), z + (eave if corner else 0))))
    for i in range(8): bm.faces.new((ring[i], ring[(i + 1) % 8], top))
    bm.faces.new(list(reversed(ring)))
    return mk(bm, "tile")
L["Lid"].append(roof(0.415, 0.048, 0.03, 0.014))
L["Lid"].append(prim("cube", "red", (0, 0, 0.452), scale=(0.022, 0.022, 0.022), size=1))  # little pavilion core
L["Lid"].append(roof(0.462, 0.030, 0.022, 0.010))
L["Lid"].append(prim("uv_sphere", "gilt", (0, 0, 0.492), segments=8, ring_count=4, radius=0.011))

# ---- Seal: red knot with tassel on the lid front
fy = -0.152
L["Seal"].append(prim("uv_sphere", "red", (0, fy, 0.322), scale=(0.022, 0.012, 0.018), segments=8, ring_count=5, radius=1))
for sx in (-1, 1):
    L["Seal"].append(prim("cube", "red", (sx * 0.02, fy, 0.322), rot=(0, sx * 0.6, 0), scale=(0.024, 0.008, 0.012), size=1))
L["Seal"].append(prim("cone", "red", (0, fy - 0.022, 0.265), vertices=6, radius1=0.014, radius2=0.004, depth=0.07))
L["Seal"].append(prim("cylinder", "gilt", (0, fy - 0.022, 0.302), vertices=6, radius=0.007, depth=0.012))

# ---- Binding: hemp cord ear to ear over the lid
pts = [(-0.19, 0, 0.385)] + [(0.17 * math.cos(t), 0, 0.30 + 0.115 * math.sin(t)) for t in [math.pi * (1 - i / 8) for i in range(1, 8)]] + [(0.19, 0, 0.385)]
for p, q in zip(pts, pts[1:]):
    p, q = mathutils.Vector(p), mathutils.Vector(q); d = q - p
    L["Binding"].append(prim("cylinder", "cord", (p + q) / 2, rot=d.to_track_quat("Z", "Y").to_euler(), vertices=5, radius=0.006, depth=d.length))

# ---- Glow: disc in the mouth
L["Glow"].append(prim("cylinder", "glow", (0, 0, RIM_Z - 0.012), vertices=16, radius=0.14, depth=0.004))

# ------------------------------------------------------------------ assemble and export
def join(parts):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts: p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1: bpy.ops.object.join()
    return bpy.context.active_object

def set_origin(o, p):
    p = mathutils.Vector(p)
    for v in o.data.vertices: v.co -= p
    o.location = p

root = bpy.data.objects.new("Chest_TrungHoa", None); bpy.context.scene.collection.objects.link(root)
tris = 0
for layer, parts in L.items():
    o = join(parts); o.name = layer; o.data.name = layer
    for f in o.data.polygons: f.use_smooth = False
    if layer == "Lid": set_origin(o, (0, RIM_R, RIM_Z))
    elif layer != "Body":
        vs = [v.co for v in o.data.vertices]
        set_origin(o, sum(vs, mathutils.Vector()) / len(vs))
    o.parent = root
    n = sum(len(p.vertices) - 2 for p in o.data.polygons); print(layer, n)
    if layer != "Glow": tris += n
bpy.ops.object.select_all(action="DESELECT"); root.select_set(True)
for c in root.children: c.select_set(True)
path = os.path.join(OUT, "Chest_TrungHoa.glb")
bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
print("Chest_TrungHoa triangles (without glow):", tris, "->", path)

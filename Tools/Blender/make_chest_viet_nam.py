"""Rương Việt Nam (bản tạm): thúng tre đội nón lá, dựng thủ tục bằng Blender (bpy).

Chạy: python Tools/Blender/make_chest_viet_nam.py -- Assets/_Project/Art/Chests
Ra Chest_VietNam.glb: root Chest_VietNam -> Body, Lid (gốc ở bản lề sau, xoay quanh X), Binding (quai nón đỏ),
Seal (tem "Phúc"), Glow (quầng sáng trong thúng). Đáy ở gốc tọa độ, mét, Z lên, mặt trước là -Y.
"""
import math, sys, os
import bpy, mathutils

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "."
os.makedirs(OUT, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)

PALETTE = {
    "tre": (0.66, 0.52, 0.32), "tre_dk": (0.47, 0.35, 0.20), "tre_lt": (0.76, 0.64, 0.42),
    "la": (0.86, 0.80, 0.62), "la_dk": (0.70, 0.63, 0.45), "lua": (0.62, 0.13, 0.12),
    "giay": (0.75, 0.16, 0.12), "vang": (0.86, 0.68, 0.25), "interior": (0.10, 0.07, 0.04),
    "glow": (1.0, 0.82, 0.45),
}
_m = {}


def mat(n):
    if n in _m:
        return _m[n]
    m = bpy.data.materials.new(n); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]; c = tuple(x ** 2.2 for x in PALETTE[n])
    b.inputs["Base Color"].default_value = (*c, 1); b.inputs["Roughness"].default_value = 0.85
    if n == "glow":
        b.inputs["Emission Color"].default_value = (*c, 1); b.inputs["Emission Strength"].default_value = 2.0
    _m[n] = m
    return m


def fin(o, n):
    bpy.context.view_layer.objects.active = o; o.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    o.data.materials.clear(); o.data.materials.append(mat(n))
    for p in o.data.polygons:
        p.use_smooth = False
    o.select_set(False)
    return o


def cone(r1, r2, d, loc, n, v=16, rot=(0, 0, 0), cap=True):
    bpy.ops.mesh.primitive_cone_add(vertices=v, radius1=r1, radius2=r2, depth=d, location=loc, rotation=rot,
                                    end_fill_type="NGON" if cap else "NOTHING")
    return fin(bpy.context.active_object, n)


def torus(R, r, loc, n, rot=(0, 0, 0), mj=16, mn=4):
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, major_segments=mj, minor_segments=mn,
                                     location=loc, rotation=rot)
    return fin(bpy.context.active_object, n)


def box(s, loc, n, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    o = bpy.context.active_object; o.scale = s
    return fin(o, n)


def ico(r, loc, n):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=r, location=loc)
    return fin(bpy.context.active_object, n)


P = {"Body": [], "Lid": [], "Binding": [], "Seal": [], "Glow": []}
add = lambda k, o: P[k].append(o)

# ---- thúng (Body): bát tre loe, đan thành các băng xen màu
RB, RT, H = 0.15, 0.20, 0.19
bands = 6
for i in range(bands):
    z0, z1 = H * i / bands, H * (i + 1) / bands
    r0 = RB + (RT - RB) * (i / bands) ** 0.7; r1 = RB + (RT - RB) * ((i + 1) / bands) ** 0.7
    o = cone(r0, r1, z1 - z0, (0, 0, (z0 + z1) / 2), "tre" if i % 2 else "tre_dk", v=18, cap=(i == 0),
             rot=(0, 0, math.radians(10 * (i % 2))))
    add("Body", o)
for i in range(18):                                           # nan dọc
    a = 2 * math.pi * i / 18
    add("Body", box((0.012, 0.006, H), (math.cos(a) * 0.181, math.sin(a) * 0.181, H / 2), "tre_lt",
                    rot=(0, math.atan((RT - RB) / H), a)))   # nghiêng theo độ loe
add("Body", torus(RT + 0.004, 0.014, (0, 0, H), "tre_dk", mj=20))          # vành miệng
add("Body", torus(RB + 0.01, 0.012, (0, 0, 0.012), "tre_dk", mj=18))       # vành đáy
add("Body", cone(RT - 0.006, RT - 0.006, 0.004, (0, 0, H - 0.03), "interior", v=18))  # lòng tối
for sx in (-1, 1):                                            # quai thúng hai bên
    add("Body", torus(0.03, 0.007, (sx * (RT + 0.012), 0, H - 0.035), "tre_dk", rot=(math.radians(90), 0, math.radians(90)), mj=10))

# ---- nón lá (Lid): nón thấp phủ trùm miệng, có các vòng nan
LR, LH, LZ = 0.245, 0.17, H + 0.006
add("Lid", cone(LR, 0.0, LH, (0, 0, LZ + LH / 2), "la", v=20))
add("Lid", cone(LR - 0.004, LR - 0.004, 0.006, (0, 0, LZ + 0.001), "la_dk", v=20))   # đáy nón
for t in (0.18, 0.42, 0.66):
    add("Lid", torus(LR * (1 - t) + 0.002, 0.0035, (0, 0, LZ + LH * t), "la_dk", mj=20, mn=3))
add("Lid", torus(LR, 0.006, (0, 0, LZ + 0.002), "tre_dk", mj=20, mn=4))       # vành nón
add("Lid", ico(0.012, (0, 0, LZ + LH - 0.004), "vang"))                        # chóp

# ---- quai nón lụa đỏ (Binding): hai dải từ đỉnh vòng xuống trước, nút ở mặt trước
for sx in (-1, 1):
    a = math.atan2(LH, LR)
    add("Binding", box((0.018, 0.004, math.hypot(LR, LH) * 0.95), (sx * 0.035, -LR * 0.5, LZ + LH * 0.5 + 0.004), "lua",
                       rot=(-(math.pi / 2 - a), 0, sx * math.radians(8))))
    add("Binding", box((0.016, 0.004, 0.08), (sx * 0.02, -RT - 0.018, H - 0.03), "lua", rot=(0, sx * math.radians(18), 0)))
add("Binding", ico(0.016, (0, -LR + 0.01, LZ + 0.004), "lua"))

# ---- tem "Phúc" (Seal): hình thoi giấy đỏ viền vàng trên thân trước
add("Seal", box((0.06, 0.004, 0.06), (0, -0.182, 0.095), "giay", rot=(math.radians(-14), math.radians(45), 0)))
add("Seal", box((0.03, 0.006, 0.03), (0, -0.184, 0.095), "vang", rot=(math.radians(-14), math.radians(45), 0)))

add("Glow", ico(0.08, (0, 0, H - 0.01), "glow"))


def join(parts):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    return bpy.context.active_object


def set_origin(o, pt):
    p = mathutils.Vector(pt)
    for v in o.data.vertices:
        v.co -= p
    o.location = p


root = bpy.data.objects.new("Chest_VietNam", None); bpy.context.scene.collection.objects.link(root)
tris = {}
for k, parts in P.items():
    o = join(parts); o.name = k; o.data.name = k
    if k == "Lid":
        set_origin(o, (0, LR, LZ))           # bản lề: mép sau vành nón
    elif k != "Body":
        vs = [v.co for v in o.data.vertices]
        set_origin(o, sum(vs, mathutils.Vector()) / len(vs))
    o.parent = root
    tris[k] = sum(len(p.vertices) - 2 for p in o.data.polygons)
bpy.ops.object.select_all(action="SELECT")
path = os.path.join(OUT, "Chest_VietNam.glb")
bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
print("TRIS", tris, "total(no glow)", sum(v for k, v in tris.items() if k != "Glow"), "->", path)

"""Rương Không gian (bản thử tạm): khoang hạ cánh hình chuông đứng trên 3 chân, dựng thuần bằng bpy.

Run: python make_chest_khong_gian.py -- <output dir>      (mặc định Assets/_Project/Art/Chests)
GLB: Chest_KhongGian (root) -> Body, Lid (gốc ở bản lề sau, xoay quanh X), Binding (chốt cửa), Glow (quầng sáng miệng).
Đáy ở gốc tọa độ, mét, Z lên trong Blender (glTF đổi sang Y lên). Mặt trước là -Y.
"""
import math, sys, os
import bpy, mathutils

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "Assets/_Project/Art/Chests"
os.makedirs(OUT, exist_ok=True)
R90 = math.radians(90)
bpy.ops.wm.read_factory_settings(use_empty=True)

PALETTE = {  # sRGB
    "hull": (0.80, 0.79, 0.75), "hull_dk": (0.52, 0.53, 0.55), "shield": (0.80, 0.42, 0.16),
    "metal": (0.36, 0.38, 0.42), "glass": (0.12, 0.20, 0.30), "accent": (0.85, 0.66, 0.20),
    "interior": (0.05, 0.05, 0.08), "glow": (0.55, 0.85, 1.0),
}
_mats = {}


def mat(name):
    if name in _mats:
        return _mats[name]
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    c = tuple(x ** 2.2 for x in PALETTE[name])
    b.inputs["Base Color"].default_value = (*c, 1.0)
    b.inputs["Roughness"].default_value = 0.25 if name == "glass" else 0.7
    b.inputs["Metallic"].default_value = 0.6 if name == "metal" else 0.0
    if name == "glow":
        b.inputs["Emission Color"].default_value = (*c, 1.0)
        b.inputs["Emission Strength"].default_value = 2.0
    _mats[name] = m
    return m


def finish(o, material):
    bpy.context.view_layer.objects.active = o
    o.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    o.data.materials.clear(); o.data.materials.append(mat(material))
    for p in o.data.polygons:
        p.use_smooth = False
    o.select_set(False)
    return o


def cone(r1, r2, z0, z1, material, verts=12, loc=(0, 0), rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=z1 - z0,
                                    location=(loc[0], loc[1], (z0 + z1) / 2), rotation=rot)
    return finish(bpy.context.active_object, material)


def cyl(r, depth, loc, material, verts=10, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=depth, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def box(size, loc, material, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    o = bpy.context.active_object; o.scale = size
    return finish(o, material)


def ico(r, loc, material, sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, radius=r, location=loc)
    return finish(bpy.context.active_object, material)


def beam(a, b, r, material, verts=6):
    a, b = mathutils.Vector(a), mathutils.Vector(b)
    d = b - a
    q = d.to_track_quat("Z", "Y")
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=d.length, location=(a + b) / 2,
                                        rotation=q.to_euler())
    return finish(bpy.context.active_object, material)


def r_at(z):  # bán kính thân chuông tại độ cao z
    return 0.20 + (0.085 - 0.20) * (z - Z_BASE) / (Z_TOP - Z_BASE)


Z_BASE, Z_SEAM, Z_TOP = 0.10, 0.27, 0.38
L = {"Body": [], "Lid": [], "Binding": [], "Glow": []}

# ---- Body: tấm chắn nhiệt, thân chuông dưới, chân hạ cánh, cửa sổ trước
L["Body"].append(cone(0.205, 0.215, Z_BASE - 0.03, Z_BASE, "shield", 12))            # tấm chắn nhiệt cam
L["Body"].append(cone(0.16, 0.205, Z_BASE - 0.045, Z_BASE - 0.03, "hull_dk", 12))
L["Body"].append(cone(r_at(Z_BASE), r_at(Z_SEAM), Z_BASE, Z_SEAM, "hull", 12))
L["Body"].append(cone(r_at(Z_SEAM) + 0.006, r_at(Z_SEAM) + 0.006, Z_SEAM - 0.012, Z_SEAM, "metal", 12))  # vành miệng
L["Body"].append(cyl(r_at(Z_SEAM) - 0.01, 0.004, (0, 0, Z_SEAM - 0.014), "interior", 12))           # lòng tối
L["Body"].append(cone(r_at(0.155) + 0.004, r_at(0.165) + 0.004, 0.15, 0.17, "hull_dk", 12))          # đai giữa
# cửa sổ tròn mặt trước (-Y), nghiêng theo thân
tilt = math.atan2(0.20 - 0.085, Z_TOP - Z_BASE)
zw = 0.21; yw = -(r_at(zw) + 0.002)
L["Body"].append(cyl(0.042, 0.012, (0, yw, zw), "metal", 10, rot=(R90 - tilt, 0, 0)))
L["Body"].append(cyl(0.032, 0.014, (0, yw - 0.001, zw), "glass", 10, rot=(R90 - tilt, 0, 0)))
# sọc vàng hai bên kiểu cờ hiệu
for sx in (-1, 1):
    z = 0.215
    L["Body"].append(box((0.012, 0.05, 0.022), (sx * (r_at(z) + 0.001), 0, z), "accent", rot=(0, sx * tilt, 0)))
# 3 chân hạ cánh xoè: một sau, hai trước chéo
for ang in (90, 210, 330):
    a = math.radians(ang); c, s = math.cos(a), math.sin(a)
    top = (c * 0.15, s * 0.15, Z_BASE + 0.03)
    foot = (c * 0.27, s * 0.27, 0.012)
    L["Body"].append(beam(top, foot, 0.011, "metal"))
    L["Body"].append(beam((c * 0.13, s * 0.13, Z_BASE - 0.04), (c * 0.235, s * 0.235, 0.06), 0.007, "metal"))
    L["Body"].append(cone(0.035, 0.022, 0.0, 0.014, "hull_dk", 8, loc=(foot[0], foot[1])))

# ---- Lid: chóp chuông + mũi + ăng-ten + cửa sổ nhỏ
L["Lid"].append(cone(r_at(Z_SEAM) + 0.004, r_at(Z_TOP), Z_SEAM, Z_TOP, "hull", 12))
L["Lid"].append(cone(r_at(Z_TOP), 0.055, Z_TOP, Z_TOP + 0.025, "hull_dk", 12))
L["Lid"].append(cyl(0.045, 0.02, (0, 0, Z_TOP + 0.035), "metal", 10))
L["Lid"].append(beam((0.02, 0.01, Z_TOP + 0.04), (0.06, 0.03, Z_TOP + 0.13), 0.004, "metal", 5))
L["Lid"].append(ico(0.012, (0.06, 0.03, Z_TOP + 0.135), "accent"))
zl = 0.325; yl = -(r_at(zl) + 0.003)
L["Lid"].append(cyl(0.022, 0.01, (0, yl, zl), "glass", 8, rot=(R90 - tilt, 0, 0)))
for sx in (-1, 1):  # 2 ống đẩy nhỏ hai bên
    z = 0.31
    L["Lid"].append(cyl(0.012, 0.03, (sx * (r_at(z) + 0.006), 0.02, z), "metal", 6, rot=(0, R90, 0)))

# ---- Binding: chốt cửa trước (bản lề phía sau)
L["Binding"].append(box((0.05, 0.016, 0.03), (0, -(r_at(Z_SEAM) + 0.012), Z_SEAM), "accent"))
L["Binding"].append(box((0.06, 0.02, 0.012), (0, r_at(Z_SEAM) + 0.008, Z_SEAM), "metal"))  # bản lề

# ---- Glow: quầng sáng ở miệng
L["Glow"].append(cyl(r_at(Z_SEAM) - 0.02, 0.01, (0, 0, Z_SEAM - 0.005), "glow", 12))


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


root = bpy.data.objects.new("Chest_KhongGian", None)
bpy.context.scene.collection.objects.link(root)
tris = 0
for layer, parts in L.items():
    o = join(parts); o.name = layer
    if layer == "Lid":
        set_origin(o, (0, r_at(Z_SEAM) + 0.008, Z_SEAM))     # bản lề sau
    elif layer != "Body":
        cs = [v.co for v in o.data.vertices]
        set_origin(o, sum(cs, mathutils.Vector()) / len(cs))
    o.parent = root
    n = sum(len(p.vertices) - 2 for p in o.data.polygons)
    print(layer, n); tris += n if layer != "Glow" else 0
bpy.ops.object.select_all(action="SELECT")
path = os.path.join(OUT, "Chest_KhongGian.glb")
bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
print("Chest_KhongGian triangles (without glow):", tris, "->", path)

"""Rương Tuổi thơ Việt Nam (bản thử tạm): đèn ông sao năm cánh nằm ngửa, dựng hoàn toàn bằng bpy.

Chạy: python make_chest_tuoi_tho.py -- <file .glb ra>
  (mặc định Assets/_Project/Art/Chests/Chest_TuoiTho.glb)
Cấu trúc GLB: Chest_TuoiTho (root) -> Body, Lid (gốc ở bản lề sau, xoay quanh X), Glow (nến ở miệng), Seal (tem dán phía trước).
Đáy ở gốc tọa độ, mét, Blender Z lên (xuất glTF tự đổi sang Y lên). Mặt trước là -Y (một cánh sao chĩa ra trước).
"""
import math, os, sys
import bpy, bmesh, mathutils
from mathutils import Vector

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv and len(sys.argv) > sys.argv.index("--") + 1 \
    else os.path.join(HERE, "..", "..", "Assets", "_Project", "Art", "Chests", "Chest_TuoiTho.glb")

R_OUT, R_IN = 0.30, 0.135      # bán kính đầu cánh và chân cánh
H_BODY, H_RIM, H_APEX = 0.17, 0.03, 0.12  # thân, viền nắp, chóp nắp
WALL = 0.018

PALETTE = {  # sRGB
    "paper_red": (0.66, 0.20, 0.16), "paper_red_dk": (0.48, 0.14, 0.12), "paper_yel": (0.80, 0.58, 0.22),
    "bamboo": (0.74, 0.62, 0.40), "bamboo_dk": (0.52, 0.42, 0.26), "interior": (0.10, 0.06, 0.05),
    "tassel": (0.90, 0.78, 0.40), "seal": (0.86, 0.82, 0.70), "seal_ink": (0.55, 0.15, 0.12),
    "candle": (0.92, 0.88, 0.78), "glow": (1.0, 0.78, 0.40),
}
_mats = {}


def mat(name):
    if name not in _mats:
        m = bpy.data.materials.new(name)
        m.use_nodes = True
        b = m.node_tree.nodes["Principled BSDF"]
        c = tuple(x ** 2.2 for x in PALETTE[name])
        b.inputs["Base Color"].default_value = (*c, 1)
        b.inputs["Roughness"].default_value = 0.55 if name.startswith("paper") else 0.85
        if name == "glow":
            b.inputs["Emission Color"].default_value = (*c, 1)
            b.inputs["Emission Strength"].default_value = 2.0
        _mats[name] = m
    return _mats[name]


TIP_LIFT = 0.05   # đầu cánh vuốt mỏng: đáy nhấc lên, đỉnh hạ xuống như đèn ông sao thật


def star(r_out, r_in, z=0.0, zt=None):
    """10 đỉnh, cánh đầu tiên chĩa -Y. zt: độ cao riêng cho đầu cánh (mặc định = z)."""
    pts = []
    for i in range(10):
        a = math.radians(-90 + 36 * i)
        r = r_out if i % 2 == 0 else r_in
        pts.append(Vector((r * math.cos(a), r * math.sin(a), z if (zt is None or i % 2) else zt)))
    return pts


def mesh_obj(name, build, material_slots):
    """build(bm, mi) tạo mặt; mi(name) trả chỉ số vật liệu."""
    me = bpy.data.meshes.new(name)
    bm = bmesh.new()
    idx = {}

    def mi(n):
        if n not in idx:
            idx[n] = len(idx)
        return idx[n]
    build(bm, mi)
    bm.to_mesh(me)
    bm.free()
    for n in sorted(idx, key=idx.get):
        me.materials.append(mat(n))
    o = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(o)
    return o


def ring(bm, pts):
    return [bm.verts.new(p) for p in pts]


def bridge(bm, a, b, m, flip=False):
    n = len(a)
    for i in range(n):
        j = (i + 1) % n
        f = bm.faces.new((a[i], a[j], b[j], b[i]) if not flip else (b[i], b[j], a[j], a[i]))
        f.material_index = m


def cap(bm, vs, m, flip=False):
    """Mặt sao lõm: chia quạt từ tâm để tam giác đúng."""
    c = bm.verts.new(sum((v.co for v in vs), Vector()) / len(vs))
    n = len(vs)
    for i in range(n):
        j = (i + 1) % n
        f = bm.faces.new((c, vs[j], vs[i]) if not flip else (c, vs[i], vs[j]))
        f.material_index = m


def build_body(bm, mi):
    bot = ring(bm, star(R_OUT, R_IN, 0.0, TIP_LIFT))
    top = ring(bm, star(R_OUT, R_IN, H_BODY, H_BODY - TIP_LIFT))
    k = (R_OUT - WALL * 2.2) / R_OUT
    itop = ring(bm, star(R_OUT * k, R_IN - WALL, H_BODY, H_BODY - TIP_LIFT))
    ibot = ring(bm, star(R_OUT * k, R_IN - WALL, 0.025))
    for i in range(10):  # tô xen kẽ hai sắc đỏ cho thấy nếp gấp giấy
        j = (i + 1) % 10
        f = bm.faces.new((bot[i], bot[j], top[j], top[i]))
        f.material_index = mi("paper_red" if i % 2 == 0 else "paper_red_dk")
    bridge(bm, top, itop, mi("bamboo"))
    bridge(bm, itop, ibot, mi("interior"))
    cap(bm, ibot, mi("interior"))
    cap(bm, bot, mi("bamboo_dk"), flip=True)


def build_lid(bm, mi):
    z0 = H_BODY + 0.004
    bot = ring(bm, star(R_OUT, R_IN, z0, z0 - TIP_LIFT))
    top = ring(bm, star(R_OUT, R_IN, z0 + H_RIM, z0 + H_RIM - TIP_LIFT))
    for i in range(10):
        j = (i + 1) % 10
        f = bm.faces.new((bot[i], bot[j], top[j], top[i]))
        f.material_index = mi("bamboo")
    apex = bm.verts.new((0, 0, z0 + H_RIM + H_APEX))
    for i in range(10):  # mỗi cánh hai mặt: một đỏ một vàng như đèn thật
        j = (i + 1) % 10
        f = bm.faces.new((top[i], top[j], apex))
        f.material_index = mi("paper_red" if i % 2 == 0 else "paper_yel")
    cap(bm, bot, mi("interior"), flip=True)


def stick(a, b, r, material, verts=5):
    """Thanh tre từ a tới b."""
    a, b = Vector(a), Vector(b)
    d = b - a
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=d.length, location=(a + b) / 2)
    o = bpy.context.active_object
    o.rotation_mode = "QUATERNION"
    o.rotation_quaternion = d.to_track_quat("Z", "Y")
    return finish(o, material)


def finish(o, material):
    bpy.ops.object.select_all(action="DESELECT")
    o.select_set(True)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    o.data.materials.clear()
    o.data.materials.append(mat(material))
    return o


def cone(loc, r, depth, material, verts=5, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r, radius2=0, depth=depth, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def join(parts, name):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = name
    for p in o.data.polygons:
        p.use_smooth = False
    return o


def set_origin(o, p):
    p = Vector(p)
    for v in o.data.vertices:
        v.co -= p
    o.location = p


bpy.ops.wm.read_factory_settings(use_empty=True)

# ---- Body: thân sao + khung tre viền dưới/trên + tua giấy đầu cánh
body = [mesh_obj("BodyShell", build_body, None)]
for z, zt in ((0.008, TIP_LIFT + 0.008), (H_BODY - 0.004, H_BODY - TIP_LIFT - 0.004)):
    pts = star(R_OUT + 0.004, R_IN + 0.004, z, zt)
    for i in range(10):
        body.append(stick(pts[i], pts[(i + 1) % 10], 0.007, "bamboo"))
for i, p in enumerate(star(R_OUT, R_IN, H_BODY - TIP_LIFT)):
    if i % 2 == 0:  # tua giấy vàng treo ở mỗi đầu cánh
        d = Vector((p.x, p.y, 0)).normalized()
        body.append(cone((p.x + d.x * 0.012, p.y + d.y * 0.012, TIP_LIFT - 0.01), 0.016, 0.07, "tassel",
                         rot=(math.pi, 0, 0)))
        body.append(stick((p.x, p.y, TIP_LIFT + 0.01), (p.x + d.x * 0.012, p.y + d.y * 0.012, TIP_LIFT + 0.025),
                          0.003, "bamboo_dk", 4))
body = join(body, "Body")

# ---- Lid: chóp sao + gân tre
z0 = H_BODY + 0.004
apex = (0, 0, z0 + H_RIM + H_APEX)
lid = [mesh_obj("LidShell", build_lid, None)]
for i, p in enumerate(star(R_OUT + 0.003, R_IN + 0.003, z0 + H_RIM, z0 + H_RIM - TIP_LIFT)):
    if i % 2 == 0:
        lid.append(stick(p, apex, 0.006, "bamboo"))
lid = join(lid, "Lid")
HINGE = (0, R_OUT * math.sin(math.radians(54)) + 0.003, H_BODY - TIP_LIFT)
set_origin(lid, HINGE)

# ---- Glow: ngôi sao phẳng phát sáng ở miệng + ngọn nến
def build_glow(bm, mi):
    cap(bm, ring(bm, star(R_OUT * 0.78, R_IN * 0.85, H_BODY - 0.012)), mi("glow"))
glow = mesh_obj("Glow", build_glow, None)
set_origin(glow, (0, 0, H_BODY - 0.012))

body2 = [body, stick((0, 0, 0.025), (0, 0, 0.11), 0.018, "candle", 6), cone((0, 0, 0.125), 0.008, 0.03, "glow", 5)]
# Cán tre cầm như đèn thật: buộc vào đầu cánh sau bên trái, chĩa chéo ra ngoài và lên (gắn thân, nắp mở tự do)
tip = star(R_OUT, R_IN, H_BODY / 2)[6]
d = Vector((tip.x, tip.y, 0)).normalized()
z_mid = (TIP_LIFT + H_BODY - TIP_LIFT) / 2
base = Vector((tip.x, tip.y, z_mid)) - d * 0.03
end = base + d * 0.30 + Vector((0, 0, 0.26))
body2.append(stick(base, end, 0.009, "bamboo_dk", 6))
body2.append(stick(base + d * 0.015 - Vector((0, 0, 0.012)), base + d * 0.015 + Vector((0, 0, 0.012)), 0.016, "tassel", 6))  # nút dây buộc
body = join(body2, "Body")

# ---- Seal: tem giấy dán ngang khe nắp ở mũi cánh trước
ZS = H_BODY - TIP_LIFT
bpy.ops.mesh.primitive_plane_add(size=1, location=(0, -R_OUT - 0.006, ZS), rotation=(math.radians(90), 0, 0))
s = bpy.context.active_object
s.scale = (0.035, 0.07, 1)
seal = finish(s, "seal")
dot = cone((0, -R_OUT - 0.008, ZS), 0.013, 0.004, "seal_ink", 8, rot=(math.radians(90), 0, 0))
seal = join([seal, dot], "Seal")
set_origin(seal, sum((v.co for v in seal.data.vertices), Vector()) / len(seal.data.vertices))

# ---- Root và xuất
root = bpy.data.objects.new("Chest_TuoiTho", None)
bpy.context.scene.collection.objects.link(root)
tris = {}
for o in (body, lid, glow, seal):
    o.parent = root
    tris[o.name] = sum(len(p.vertices) - 2 for p in o.data.polygons)
bpy.ops.object.select_all(action="DESELECT")
root.select_set(True)
for c in root.children:
    c.select_set(True)
os.makedirs(os.path.dirname(os.path.abspath(OUT)), exist_ok=True)
bpy.ops.export_scene.gltf(filepath=OUT, export_format="GLB", use_selection=True, export_apply=True)
print("triangles:", tris, "total:", sum(tris.values()), "->", OUT)

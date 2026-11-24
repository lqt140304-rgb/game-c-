"""Low-poly neutral items for the lower tiers (junk and common), built procedurally with Blender (bpy).

Run: python make_items_lowpoly.py -- <output dir>
One GLB per item named <ID>_<slug>.glb, root object = ID, child "Body". Bottom centre at the origin, metres,
Blender Z up (glTF export converts to Y up). The detail level rises with rarity: junk is deliberately crude.
"""
import math, random, sys, os
import bpy, mathutils

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "."
os.makedirs(OUT, exist_ok=True)
R90 = math.radians(90)
D = math.radians

PALETTE = {  # sRGB, converted to linear in mat()
    "sole": (0.20, 0.19, 0.18), "leather": (0.35, 0.24, 0.17), "leather_lt": (0.45, 0.33, 0.24), "lace": (0.78, 0.74, 0.62),
    "sock": (0.66, 0.62, 0.55), "sock_dk": (0.45, 0.42, 0.38), "stink": (0.50, 0.58, 0.30),
    "egg": (0.78, 0.76, 0.62), "egg_rot": (0.55, 0.58, 0.36), "bone": (0.80, 0.76, 0.64),
    "rubber": (0.40, 0.47, 0.30), "rubber_dk": (0.30, 0.36, 0.22),
    "can_red": (0.55, 0.22, 0.19), "can_sil": (0.65, 0.66, 0.68), "jersey": (0.58, 0.18, 0.16), "jersey_dk": (0.40, 0.12, 0.11),
    "ticket": (0.82, 0.78, 0.64), "ticket_red": (0.60, 0.25, 0.20),
    "glaze": (0.30, 0.42, 0.52), "glaze_dk": (0.20, 0.30, 0.38), "gold": (0.68, 0.56, 0.26),
    "book": (0.36, 0.24, 0.17), "book_lt": (0.46, 0.33, 0.23), "pages": (0.80, 0.74, 0.58),
    "clock_wood": (0.40, 0.27, 0.17), "clock_wood_lt": (0.50, 0.35, 0.22), "face": (0.82, 0.78, 0.65), "ink": (0.12, 0.10, 0.09),
    "talis": (0.80, 0.68, 0.30), "red_ink": (0.60, 0.16, 0.14),
    "screen": (0.32, 0.38, 0.27), "plastic": (0.62, 0.62, 0.58), "plastic_dk": (0.36, 0.37, 0.38), "btn": (0.52, 0.22, 0.24),
    "glass_t": (0.62, 0.78, 0.78), "wire_r": (0.55, 0.20, 0.18), "wire_b": (0.22, 0.32, 0.50), "wire_y": (0.70, 0.60, 0.25),
    "bamboo": (0.72, 0.58, 0.34), "bamboo_dk": (0.52, 0.40, 0.22), "hemp": (0.62, 0.52, 0.35), "brass_dk": (0.45, 0.38, 0.18),
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
    bsdf.inputs["Roughness"].default_value = 0.85
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


def cyl(radius, depth, loc, material, verts=8, rot=(0, 0, 0), r2=None, scale=(1, 1, 1)):
    if r2 is None:
        bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=loc, rotation=rot)
    else:
        bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=radius, radius2=r2, depth=depth, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = scale
    return finish(o, material)


def uvs(radius, loc, material, seg=10, rings=6, scale=(1, 1, 1), rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=radius, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = scale
    return finish(o, material)


def ico(radius, loc, material, sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, radius=radius, location=loc)
    return finish(bpy.context.active_object, material)


def torus(major, minor, loc, material, rot=(0, 0, 0), seg=10):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, major_segments=seg, minor_segments=4, location=loc, rotation=rot)
    return finish(bpy.context.active_object, material)


def lumpy(size, loc, material, cuts, amount, seed, keep_bottom=True):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o = bpy.context.active_object
    o.scale = size
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.subdivide(number_cuts=cuts)
    bpy.ops.object.mode_set(mode="OBJECT")
    random.seed(seed)
    low = loc[2] - size[2] / 2 + 0.001
    for v in o.data.vertices:
        if keep_bottom and v.co.z <= low:
            continue
        v.co.x += random.uniform(-amount, amount)
        v.co.y += random.uniform(-amount, amount)
        v.co.z += random.uniform(-amount, amount)
    return finish(o, material)


def disc_cut(radius, height, loc, material, x_cut, verts=16):
    """A disc standing on its edge (axis = Y), cut along the plane x = x_cut keeping the left part (a broken dial)."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=height, location=loc, rotation=(R90, 0, 0))
    o = bpy.context.active_object
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.bisect(plane_co=(x_cut, 0, 0), plane_no=(1, 0, 0), clear_outer=True, use_fill=True)
    bpy.ops.object.mode_set(mode="OBJECT")
    return finish(o, material)


def half_cyl_y(radius, depth, loc, material, verts=12):
    """Upper half of a cylinder lying along Y (the arched top of a clock)."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=loc, rotation=(R90, 0, 0))
    o = bpy.context.active_object
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.bisect(plane_co=(0, 0, loc[2]), plane_no=(0, 0, 1), clear_inner=True, use_fill=True)
    bpy.ops.object.mode_set(mode="OBJECT")
    return finish(o, material)


def stink(x, y, z0):
    """Three wavy green stink lines rising above a smelly item."""
    out = []
    for i, dx in enumerate((-0.025, 0.0, 0.025)):
        for k in range(4):
            out.append(box((0.006, 0.006, 0.016), (x + dx + (0.006 if k % 2 else -0.006), y, z0 + i * 0.002 + k * 0.016), "stink"))
    return out


# ================================================================================================ JUNK (tier 0, crude)
def build_r01():   # giày cũ
    p = [box((0.09, 0.26, 0.022), (0, 0, 0.011), "sole"),
         box((0.085, 0.09, 0.07), (0, 0.075, 0.057), "leather"),
         box((0.085, 0.1, 0.05), (0, 0.0, 0.047), "leather"),
         uvs(0.045, (0, -0.09, 0.05), "leather_lt", seg=8, rings=5, scale=(1, 1.3, 0.7)),
         box((0.065, 0.06, 0.02), (0, 0.085, 0.1), "sole"),
         box((0.05, 0.02, 0.07), (0, 0.03, 0.085), "leather_lt", rot=(D(-25), 0, 0)),
         box((0.092, 0.2, 0.006), (0, -0.01, 0.024), "lace"),
         box((0.02, 0.02, 0.008), (0.012, -0.12, 0.075), "sole")]          # hole in the toe
    for y in (0.012, -0.015, -0.042):
        p.append(box((0.06, 0.008, 0.008), (0, y, 0.077), "lace", rot=(0, 0, D(10 if y > 0 else -10))))
    return p


def build_r02():   # tất thối
    p = [cyl(0.032, 0.11, (0, 0.05, 0.075), "sock", verts=8),
         cyl(0.032, 0.13, (0, -0.02, 0.032), "sock", verts=8, rot=(R90, 0, 0)),
         ico(0.034, (0, 0.05, 0.03), "sock_dk"),
         torus(0.033, 0.006, (0, 0.05, 0.1), "sock_dk", seg=8),
         torus(0.033, 0.006, (0, 0.05, 0.07), "sock_dk", seg=8),
         box((0.016, 0.016, 0.01), (0, -0.085, 0.045), "ink")]            # hole at the toe
    return p + stink(0.0, 0.05, 0.14)


def build_r03():   # trứng thối
    p = [uvs(0.042, (0, 0, 0.052), "egg_rot", seg=8, rings=6, scale=(0.8, 0.8, 1.15)),
         uvs(0.02, (0.018, -0.02, 0.07), "egg", seg=6, rings=4, scale=(1, 0.6, 1)),
         box((0.004, 0.004, 0.05), (0.02, -0.034, 0.055), "ink", rot=(0, D(14), D(8)))]   # crack
    return p + stink(0.0, 0.0, 0.11)


def build_r04():   # xương cá
    p = [box((0.01, 0.2, 0.008), (0, 0, 0.012), "bone"),
         box((0.045, 0.05, 0.016), (0, -0.115, 0.012), "bone"),
         box((0.018, 0.01, 0.01), (0, -0.095, 0.012), "ink"),
         ico(0.006, (0, -0.125, 0.021), "ink")]
    for i in range(6):
        y = -0.07 + i * 0.026
        for s in (-1, 1):
            p.append(box((0.055, 0.006, 0.006), (s * 0.026, y, 0.012), "bone", rot=(0, 0, D(-18 * s))))
    for s in (-1, 1):
        p.append(box((0.05, 0.008, 0.005), (s * 0.022, 0.118, 0.012), "bone", rot=(0, 0, D(28 * s))))
    return p


def build_r05():   # dép tổ ong cụt quai: green rubber sandal, V-shaped thong with one strap snapped
    p = [box((0.085, 0.23, 0.02), (0, 0.0, 0.01), "rubber"),
         uvs(0.0425, (0, -0.115, 0.01), "rubber", seg=8, rings=3, scale=(1, 1, 0.5)),
         uvs(0.0425, (0, 0.115, 0.01), "rubber", seg=8, rings=3, scale=(1, 1, 0.5)),
         cyl(0.01, 0.05, (0, -0.07, 0.04), "rubber_dk", verts=6),                                  # toe post
         box((0.012, 0.012, 0.05), (-0.037, 0.0, 0.04), "rubber_dk"),                              # left strap anchor
         box((0.012, 0.012, 0.03), (0.037, 0.0, 0.03), "rubber_dk"),                               # right strap anchor
         box((0.014, 0.088, 0.008), (-0.0185, -0.035, 0.066), "rubber_dk", rot=(0, 0, D(-25))),    # left strap, intact
         box((0.014, 0.035, 0.008), (0.012, -0.075, 0.066), "rubber_dk", rot=(0, 0, D(28))),       # right strap, snapped short
         box((0.014, 0.014, 0.006), (0.027, -0.058, 0.05), "rubber_dk", rot=(0, D(30), D(28)))]   # torn tail
    for r, y in enumerate((-0.075, -0.04, -0.005, 0.03, 0.065, 0.1)):                              # honeycomb dents
        for x in ((-0.024, 0.0, 0.024) if r % 2 == 0 else (-0.012, 0.012)):
            p.append(cyl(0.013, 0.005, (x, y + 0.01, 0.02), "rubber_dk", verts=6))
    return p

def build_r06():   # lon nước ngọt bẹp: a can lying on its side with a crushed middle
    p = [cyl(0.033, 0.06, (0, -0.05, 0.035), "can_red", verts=12, rot=(R90, 0, 0)),                      # intact bottom half
         cyl(0.033, 0.07, (0, 0.0, 0.03), "can_red", verts=12, rot=(R90, 0, 0), scale=(1.25, 1, 0.7)),     # crushed middle (overlaps both halves)
         cyl(0.033, 0.06, (0, 0.05, 0.035), "can_red", verts=12, rot=(R90, 0, 0)),                        # top half
         cyl(0.027, 0.012, (0, 0.087, 0.035), "can_sil", verts=12, rot=(R90, 0, 0)),                      # silver neck
         cyl(0.03, 0.006, (0, 0.096, 0.035), "can_sil", verts=12, rot=(R90, 0, 0)),
         torus(0.03, 0.003, (0, 0.099, 0.035), "can_sil", rot=(R90, 0, 0), seg=12),
         box((0.016, 0.012, 0.004), (0.004, 0.102, 0.052), "can_sil"),                                   # pull tab
         cyl(0.027, 0.01, (0, -0.083, 0.035), "can_sil", verts=12, rot=(R90, 0, 0)),                      # bottom rim
         box((0.012, 0.03, 0.022), (0.04, 0.0, 0.03), "can_red", rot=(0, 0, D(15))),                      # dent bulge
         box((0.045, 0.012, 0.003), (0, -0.05, 0.0695), "can_sil"),                                       # white label stripe sits on the can
         box((0.045, 0.012, 0.003), (0, 0.05, 0.0695), "can_sil")]
    return p


def build_r07():   # áo đấu cũ của đội nhại: a T-shirt lying flat so it reads as a shirt
    p = [lumpy((0.17, 0.2, 0.024), (0, 0.0, 0.012), "jersey", 2, 0.003, 5),
         lumpy((0.07, 0.085, 0.02), (0.115, 0.085, 0.012), "jersey", 1, 0.003, 6, ),
         lumpy((0.07, 0.085, 0.02), (-0.115, 0.085, 0.012), "jersey", 1, 0.003, 7),
         box((0.08, 0.03, 0.006), (0, -0.095, 0.026), "jersey_dk"),                                   # hem
         box((0.07, 0.014, 0.006), (0.115, 0.14, 0.024), "jersey_dk", rot=(0, 0, D(-30))),             # sleeve cuffs
         box((0.07, 0.014, 0.006), (-0.115, 0.14, 0.024), "jersey_dk", rot=(0, 0, D(30))),
         box((0.05, 0.016, 0.006), (0, 0.1, 0.026), "jersey_dk"),                                      # collar
         box((0.03, 0.03, 0.006), (0, 0.098, 0.027), "ink", rot=(0, 0, D(45))),                        # neck opening
         cyl(0.016, 0.004, (-0.04, 0.045, 0.026), "jersey_dk", verts=10),                              # emblem
         ico(0.007, (-0.04, 0.045, 0.029), "gold"),
         box((0.016, 0.004, 0.032), (-0.01, -0.02, 0.026), "gold", rot=(D(0), 0, 0)),                  # number 7
         box((0.032, 0.004, 0.012), (0.006, -0.002, 0.026), "gold")]
    p[-2] = box((0.012, 0.034, 0.004), (-0.012, -0.03, 0.026), "gold", rot=(0, 0, D(-18)))
    p[-1] = box((0.036, 0.012, 0.004), (0.0, -0.01, 0.026), "gold")
    return p

def build_r08():   # vé cược nhàu: a ticket with a red band, text lines and a torn corner
    p = [lumpy((0.1, 0.05, 0.006), (0, 0, 0.004), "ticket", 3, 0.0015, 9),
         box((0.016, 0.052, 0.003), (0.032, 0, 0.0095), "ticket_red"),
         box((0.05, 0.006, 0.003), (-0.015, 0.014, 0.0095), "ink"), box((0.04, 0.006, 0.003), (-0.02, 0.0, 0.0095), "ink"),
         box((0.03, 0.006, 0.003), (-0.025, -0.014, 0.0095), "ink"),
         box((0.014, 0.014, 0.004), (0.054, 0.027, 0.008), "ticket", rot=(0, 0, D(40)))]               # torn-off corner
    return p


def build_v01():   # bình sứ men xanh
    p = [cyl(0.04, 0.014, (0, 0, 0.007), "glaze_dk", verts=12),
         uvs(0.07, (0, 0, 0.085), "glaze", seg=14, rings=8, scale=(1, 1, 1.15)),
         cyl(0.034, 0.05, (0, 0, 0.17), "glaze", verts=12, r2=0.022),
         cyl(0.022, 0.03, (0, 0, 0.208), "glaze", verts=12),
         torus(0.026, 0.0055, (0, 0, 0.226), "gold", seg=12),
         torus(0.058, 0.004, (0, 0, 0.048), "gold", seg=14), torus(0.07, 0.004, (0, 0, 0.1), "gold", seg=14)]
    random.seed(2)
    for i in range(9):                                                                              # crackle lines
        a = D(-60 + i * 15)
        p.append(box((0.003, 0.003, 0.03), (math.sin(a) * 0.07, -math.cos(a) * 0.07, 0.085 + random.uniform(-0.02, 0.03)), "glaze_dk", rot=(0, 0, a)))
    return p


def build_v02():   # sách cổ bìa da
    p = [box((0.17, 0.23, 0.008), (0, 0, 0.004), "book"),
         box((0.16, 0.22, 0.04), (0.005, 0, 0.028), "pages"),
         box((0.17, 0.23, 0.008), (0, 0, 0.056), "book"),
         box((0.014, 0.23, 0.06), (-0.083, 0, 0.03), "book"),
         box((0.062, 0.08, 0.002), (0.0, 0, 0.0605), "book_lt"),
         box((0.03, 0.03, 0.002), (0.0, 0, 0.0615), "gold", rot=(0, 0, D(45))),
         box((0.012, 0.03, 0.012), (0.088, 0, 0.03), "gold")]
    for y in (-0.07, 0.0, 0.07):
        p.append(box((0.016, 0.01, 0.062), (-0.085, y, 0.03), "gold"))
    for sx in (-1, 1):
        for sy in (-1, 1):
            p.append(box((0.04, 0.04, 0.003), (sx * 0.075, sy * 0.105, 0.0615), "gold"))
    return p


def build_v03():   # đồng hồ quả lắc cũ (dạng đồng hồ để bàn)
    p = [box((0.13, 0.075, 0.015), (0, 0, 0.0075), "clock_wood"),
         box((0.12, 0.07, 0.17), (0, 0, 0.1), "clock_wood_lt"),
         half_cyl_y(0.06, 0.07, (0, 0, 0.185), "clock_wood_lt"),
         box((0.014, 0.074, 0.17), (-0.067, 0, 0.1), "clock_wood"), box((0.014, 0.074, 0.17), (0.067, 0, 0.1), "clock_wood"),
         cyl(0.045, 0.006, (0, -0.037, 0.18), "face", verts=14, rot=(R90, 0, 0)),
         torus(0.047, 0.004, (0, -0.039, 0.18), "gold", rot=(R90, 0, 0), seg=14),
         box((0.004, 0.004, 0.03), (0, -0.04, 0.192), "ink"), box((0.02, 0.004, 0.004), (0.008, -0.04, 0.18), "ink"),
         box((0.07, 0.004, 0.065), (0, -0.036, 0.075), "ink"),
         box((0.004, 0.004, 0.06), (0, -0.04, 0.075), "gold"), cyl(0.014, 0.005, (0, -0.041, 0.048), "gold", verts=10, rot=(R90, 0, 0)),
         ico(0.01, (0, 0, 0.255), "gold")]
    for i in range(12):
        a = D(i * 30)
        p.append(box((0.004, 0.003, 0.006), (math.sin(a) * 0.036, -0.04, 0.18 + math.cos(a) * 0.036), "ink"))
    return p


def build_v04():   # mảnh kính đồng hồ thiên văn (mặt đồng hồ vỡ)
    z = 0.13
    p = [box((0.13, 0.05, 0.014), (0, 0, 0.007), "clock_wood"), box((0.02, 0.03, 0.05), (-0.02, 0, 0.04), "clock_wood"),
         disc_cut(0.11, 0.012, (0, 0, z), "gold", 0.03),
         disc_cut(0.095, 0.016, (0, 0, z), "face", 0.03),
         disc_cut(0.07, 0.02, (0, 0, z), "glaze_dk", 0.025),
         disc_cut(0.04, 0.024, (0, 0, z), "gold", 0.02),
         box((0.004, 0.004, 0.06), (-0.01, -0.014, z + 0.02), "ink", rot=(0, D(-25), 0))]
    for s, (x, zz, r) in enumerate(((0.045, z + 0.05, 20), (0.05, z - 0.02, -30), (0.04, z - 0.075, 10))):
        p.append(box((0.03, 0.004, 0.05), (x, -0.012, zz), "glass_t", rot=(0, D(r), 0)))             # glass shards at the break
    for i in range(10):                                                                                # zodiac marks
        a = D(100 + i * 16)
        p.append(box((0.008, 0.004, 0.012), (math.cos(a) * 0.082, -0.011, z + math.sin(a) * 0.082), "ink", rot=(0, a, 0)))
    return p


def build_v05():   # thiết bị liên lạc hỏng
    p = [box((0.09, 0.03, 0.02), (0, 0, 0.01), "plastic_dk"),
         box((0.07, 0.016, 0.13), (0, 0, 0.085), "plastic", rot=(D(-8), 0, 0)),
         box((0.06, 0.004, 0.1), (0, -0.01, 0.09), "ink", rot=(D(-8), 0, 0)),
         box((0.05, 0.004, 0.075), (0, -0.0125, 0.1), "screen", rot=(D(-8), 0, 0)),
         cyl(0.004, 0.05, (0.025, 0.0, 0.17), "plastic_dk", verts=6),
         box((0.014, 0.006, 0.006), (-0.02, -0.011, 0.038), "btn"), box((0.014, 0.006, 0.006), (0.0, -0.011, 0.038), "plastic_dk"),
         box((0.014, 0.006, 0.006), (0.02, -0.011, 0.038), "plastic_dk")]
    for (x, z, r) in ((-0.01, 0.1, 35), (0.012, 0.095, -50), (0.0, 0.11, 80)):                          # cracks
        p.append(box((0.003, 0.003, 0.05), (x, -0.0145, z), "face", rot=(D(-8), D(r), 0)))
    for i, c in enumerate(("wire_r", "wire_b", "wire_y")):                                              # loose wires
        p.append(box((0.004, 0.004, 0.04), (-0.015 + i * 0.015, -0.002, 0.012 - 0.0), c, rot=(D(40), 0, D(-15 + 15 * i))))
    return p


def build_v09():   # lá bùa giấy vàng
    p = [box((0.07, 0.05, 0.012), (0, 0, 0.006), "clock_wood"), box((0.012, 0.012, 0.2), (0, 0.0, 0.112), "clock_wood"),
         box((0.1, 0.012, 0.012), (0, 0, 0.212), "clock_wood"),
         box((0.065, 0.004, 0.15), (0, -0.01, 0.13), "talis"),
         torus(0.01, 0.003, (0, -0.013, 0.2), "red_ink", rot=(R90, 0, 0), seg=8),
         box((0.004, 0.004, 0.03), (-0.025, -0.012, 0.05), "red_ink"), box((0.004, 0.004, 0.03), (0.025, -0.012, 0.05), "red_ink")]
    random.seed(8)
    for r in range(5):
        p.append(box((random.choice((0.03, 0.04, 0.025)), 0.003, 0.012), (random.uniform(-0.008, 0.008), -0.0125, 0.18 - r * 0.026), "red_ink"))
    p.append(box((0.02, 0.003, 0.02), (0.016, -0.0125, 0.075), "red_ink"))
    return p


def build_v11():   # quạt nan cũ
    p = []
    pivot = (0, 0.0, 0.012)
    for k in range(11):
        a = D(-60 + k * 12)
        x, y = math.sin(a), math.cos(a)
        p.append(box((0.011, 0.17, 0.004), (pivot[0] + x * 0.095, pivot[1] + y * 0.095, 0.012), "bamboo" if k % 2 else "bamboo_dk", rot=(0, 0, -a)))
    for r in (0.09, 0.135, 0.17):
        for k in range(10):
            a = D(-54 + k * 12)
            p.append(box((0.03, 0.006, 0.005), (math.sin(a) * r, math.cos(a) * r, 0.0145), "hemp" if r < 0.17 else "bamboo_dk", rot=(0, 0, -a)))
    p.append(box((0.022, 0.08, 0.012), (0, -0.035, 0.012), "bamboo_dk"))
    p.append(cyl(0.012, 0.014, (0, 0.0, 0.017), "gold", verts=8))
    return p


def build_v12():   # máy game cầm tay 8-bit
    p = [box((0.075, 0.022, 0.125), (0, 0, 0.0675), "plastic", rot=(D(-6), 0, 0)),
         box((0.06, 0.004, 0.05), (0, -0.0125, 0.09), "plastic_dk", rot=(D(-6), 0, 0)),
         box((0.044, 0.004, 0.036), (0, -0.0145, 0.092), "screen", rot=(D(-6), 0, 0)),
         box((0.014, 0.006, 0.004), (-0.022, -0.012, 0.052), "plastic_dk", rot=(D(-6), 0, 0)),
         box((0.004, 0.006, 0.014), (-0.022, -0.012, 0.052), "plastic_dk", rot=(D(-6), 0, 0)),
         cyl(0.007, 0.006, (0.012, -0.0125, 0.052), "btn", verts=8, rot=(R90, 0, 0)),
         cyl(0.007, 0.006, (0.026, -0.0125, 0.058), "btn", verts=8, rot=(R90, 0, 0)),
         box((0.012, 0.005, 0.004), (-0.008, -0.0125, 0.028), "plastic_dk", rot=(D(-6), 0, D(-20))),
         box((0.012, 0.005, 0.004), (0.01, -0.0125, 0.028), "plastic_dk", rot=(D(-6), 0, D(-20))),
         box((0.016, 0.012, 0.02), (0.0, 0.015, 0.14), "plastic_dk")]
    for i in range(4):
        p.append(box((0.003, 0.004, 0.012), (0.02 + i * 0.006, -0.0125, 0.024), "plastic_dk", rot=(D(-6), 0, D(-30))))
    p.append(box((0.04, 0.024, 0.014), (0, 0, 0.007), "plastic_dk"))
    return p


ITEMS = [("R01", "giay_cu", build_r01), ("R02", "tat_thoi", build_r02), ("R03", "trung_thoi", build_r03), ("R04", "xuong_ca", build_r04),
         ("R05", "dep_to_ong", build_r05), ("R06", "lon_bep", build_r06), ("R07", "ao_dau_cu", build_r07), ("R08", "ve_cuoc", build_r08),
         ("V01", "binh_su", build_v01), ("V02", "sach_co", build_v02), ("V03", "dong_ho", build_v03), ("V04", "kinh_thien_van", build_v04),
         ("V05", "thiet_bi_lien_lac", build_v05), ("V09", "la_bua", build_v09), ("V11", "quat_nan", build_v11), ("V12", "may_game", build_v12)]


def join(parts):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    return bpy.context.active_object


def clear_scene():
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    for m in list(bpy.data.meshes):
        bpy.data.meshes.remove(m)


bpy.ops.wm.read_factory_settings(use_empty=True)
for item_id, slug, builder in ITEMS:
    parts = builder()
    root = bpy.data.objects.new(item_id, None)
    bpy.context.scene.collection.objects.link(root)
    body = join(parts)
    body.name = "Body"
    minz = min(v.co.z for v in body.data.vertices)
    cx = (min(v.co.x for v in body.data.vertices) + max(v.co.x for v in body.data.vertices)) / 2
    cy = (min(v.co.y for v in body.data.vertices) + max(v.co.y for v in body.data.vertices)) / 2
    for v in body.data.vertices:
        v.co.z -= minz
        v.co.x -= cx
        v.co.y -= cy
    body.parent = root
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    body.select_set(True)
    bpy.context.view_layer.objects.active = root
    path = os.path.join(OUT, f"{item_id}_{slug}.glb")
    bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
    print(item_id, slug, "triangles:", sum(len(p.vertices) - 2 for p in body.data.polygons))
    clear_scene()

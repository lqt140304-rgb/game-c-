"""Thư viện dựng chung cho 4 rương v1 (chợ trời hẻm Việt Nam): Thùng Gỗ, Hộp Tôn, Bao Tải, Vali Đen.

Không chạy trực tiếp. Các script make_chest_thung_go.py, make_chest_hop_ton.py, make_chest_bao_tai.py,
make_chest_vali_den.py nạp file này. Cần `pip install bpy pillow` (Python 3.13, Blender 5.3 dạng module).

Quy ước (giống các rương cũ): mét, Blender Z lên (xuất glTF tự đổi thành Y lên), đáy ở z=0, mặt trước là -Y,
bản lề ở phía sau (+Y). Root `Chest_<Tên>` có các con `Body`, `Lid` (gốc tại bản lề, xoay quanh X, mở bằng góc ÂM),
`Glow`, và tùy rương `Seal`, `Binding`. Vật liệu đơn sắc (không texture ảnh), đa giác phẳng.
"""
import math
import os
import random
import sys

import bpy
import bmesh
from mathutils import Matrix, Vector

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
CHEST_DIR = os.path.join(REPO, "Assets", "_Project", "Art", "Chests")
FONT_BOLD = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"


def out_path(default_name):
    """Đường xuất: đối số sau `--` nếu có, ngược lại Assets/_Project/Art/Chests/<default_name>."""
    if "--" in sys.argv and len(sys.argv) > sys.argv.index("--") + 1:
        return sys.argv[sys.argv.index("--") + 1]
    return os.path.join(CHEST_DIR, default_name)


def V(p):
    return p if isinstance(p, Vector) else Vector(p)


class Scene:
    """Giữ bảng màu và vật liệu của một rương."""

    def __init__(self, palette):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        self.palette = palette
        self._mats = {}

    def mat(self, name):
        if name not in self._mats:
            spec = self.palette[name]
            m = bpy.data.materials.new(name)
            m.use_nodes = True
            b = m.node_tree.nodes["Principled BSDF"]
            c = tuple(x ** 2.2 for x in spec["c"])
            b.inputs["Base Color"].default_value = (*c, 1)
            b.inputs["Roughness"].default_value = spec.get("r", 0.8)
            b.inputs["Metallic"].default_value = spec.get("m", 0.0)
            if spec.get("e"):
                b.inputs["Emission Color"].default_value = (*c, 1)
                b.inputs["Emission Strength"].default_value = spec["e"]
            self._mats[name] = m
        return self._mats[name]

    def part(self, name):
        return Part(self, name)


class Part:
    """Một lớp mesh (Body, Lid...) dựng bằng bmesh; mỗi mặt gán vật liệu theo tên."""

    def __init__(self, scene, name):
        self.scene = scene
        self.name = name
        self.bm = bmesh.new()
        self.idx = {}

    def mi(self, mat):
        if mat not in self.idx:
            self.idx[mat] = len(self.idx)
        return self.idx[mat]

    # --- mặt cơ bản ---------------------------------------------------------
    def poly(self, mat, pts, toward=None, away=None):
        """Một mặt phẳng nhiều đỉnh. toward: hướng pháp tuyến mong muốn; away: điểm mà mặt phải quay ra xa."""
        pts = [V(p) for p in pts]
        n = Vector()
        for i, p in enumerate(pts):
            q = pts[(i + 1) % len(pts)]
            n += Vector(((p.y - q.y) * (p.z + q.z), (p.z - q.z) * (p.x + q.x), (p.x - q.x) * (p.y + q.y)))
        if toward is not None:
            d = V(toward)
        elif away is not None:
            d = sum(pts, Vector()) / len(pts) - V(away)
        else:
            d = n
        if n.dot(d) < 0:
            pts.reverse()
        try:
            f = self.bm.faces.new([self.bm.verts.new(p) for p in pts])
        except ValueError:
            return None
        f.material_index = self.mi(mat)
        return f

    def fan(self, mat, center, ring, toward):
        """Quạt tam giác từ tâm tới vòng đỉnh (cho mặt lõm hoặc nhiều cạnh)."""
        c = V(center)
        for i in range(len(ring)):
            self.poly(mat, [c, ring[i], ring[(i + 1) % len(ring)]], toward=toward)

    def box(self, mat, lo, hi, tf=None, mats=None):
        """Hộp chữ nhật từ lo tới hi, tf: Matrix biến đổi sau cùng. mats: dict hướng ('+x','-y','+z'...) -> vật liệu riêng."""
        lo, hi = V(lo), V(hi)
        xs, ys, zs = (lo.x, hi.x), (lo.y, hi.y), (lo.z, hi.z)
        v = [Vector((xs[i & 1], ys[(i >> 1) & 1], zs[(i >> 2) & 1])) for i in range(8)]
        c = (lo + hi) / 2
        if tf is not None:
            v = [tf @ p for p in v]
            c = tf @ c
        faces = {"-x": (0, 4, 6, 2), "+x": (1, 3, 7, 5), "-y": (0, 1, 5, 4), "+y": (2, 6, 7, 3),
                 "-z": (0, 2, 3, 1), "+z": (4, 5, 7, 6)}
        for k, ids in faces.items():
            self.poly((mats or {}).get(k, mat), [v[i] for i in ids], away=c)

    def cyl(self, mat, a, b, r, n=6, r2=None, caps=True, cap_mat=None):
        """Trụ (hoặc nón cụt) từ a tới b."""
        a, b = V(a), V(b)
        d = b - a
        q = d.to_track_quat("Z", "Y")
        r2 = r if r2 is None else r2
        ra = [a + q @ Vector((r * math.cos(2 * math.pi * i / n), r * math.sin(2 * math.pi * i / n), 0)) for i in range(n)]
        rb = [b + q @ Vector((r2 * math.cos(2 * math.pi * i / n), r2 * math.sin(2 * math.pi * i / n), 0)) for i in range(n)]
        for i in range(n):
            j = (i + 1) % n
            self.poly(mat, [ra[i], ra[j], rb[j], rb[i]], away=(a + b) / 2)
        if caps:
            cm = cap_mat or mat
            if r > 1e-6:
                self.fan(cm, a, ra, toward=-d)
            if r2 > 1e-6:
                self.fan(cm, b, rb, toward=d)

    def beam(self, mat, a, b, w, h, up=(0, 0, 1)):
        """Thanh chữ nhật tiết diện w x h đi từ a tới b (w theo phương ngang vuông góc, h theo `up`)."""
        a, b = V(a), V(b)
        d = b - a
        L = d.length
        dn = d.normalized()
        upv = V(up)
        if abs(dn.dot(upv.normalized())) > 0.99:
            upv = Vector((0, 1, 0))
        s = dn.cross(upv).normalized()      # phương ngang của tiết diện
        u = s.cross(dn).normalized()        # phương "lên" của tiết diện
        m = Matrix(((s.x, dn.x, u.x, a.x), (s.y, dn.y, u.y, a.y), (s.z, dn.z, u.z, a.z), (0, 0, 0, 1)))
        self.box(mat, (-w / 2, 0, -h / 2), (w / 2, L, h / 2), tf=m)

    # --- khối tròn xoay, vòng, lăng trụ ---------------------------------------
    def ring_band(self, mat, a, b, flip=False):
        """Dải mặt giữa hai vòng đỉnh a, b (cùng số đỉnh, đi ngược chiều kim đồng hồ nhìn từ +Z, b ở trên a thì quay ra ngoài)."""
        n = len(a)
        for i in range(n):
            j = (i + 1) % n
            pts = [a[i], a[j], b[j], b[i]]
            self.poly(mat, pts[::-1] if flip else pts)

    def lathe(self, mat, profile, n, scale=(1, 1), start=0.0, jitter=None, z_off=0.0):
        """Mặt tròn xoay quanh trục Z: profile = [(r, z), ...] từ dưới lên. Trả về các vòng đỉnh."""
        rings = []
        for r, z in profile:
            ring = []
            for i in range(n):
                a = start + 2 * math.pi * i / n
                rr = r * (1 + (jitter(i, z) if jitter else 0.0))
                ring.append(Vector((rr * math.cos(a) * scale[0], rr * math.sin(a) * scale[1], z + z_off)))
            rings.append(ring)
        for k in range(len(rings) - 1):
            self.ring_band(mat, rings[k], rings[k + 1])
        return rings

    # --- chữ tiếng Việt thành mesh phẳng ---------------------------------------
    def text(self, mat, txt, size, loc, facing="-y", rot_z=0.0, bold=True):
        """Chữ phẳng (mesh) đặt tại loc; facing: '-y' (nhìn từ phía trước), '+z' (nhìn từ trên xuống)."""
        font = bpy.data.fonts.load(FONT_BOLD) if bold else None
        c = bpy.data.curves.new("txt", "FONT")
        c.body = txt
        if font:
            c.font = font
        c.size = size
        c.resolution_u = 1
        c.align_x = "CENTER"
        c.align_y = "CENTER"
        o = bpy.data.objects.new("txt", c)
        bpy.context.scene.collection.objects.link(o)
        bpy.context.view_layer.objects.active = o
        o.select_set(True)
        bpy.ops.object.convert(target="MESH")
        me = o.data
        if facing == "-y":
            tf = Matrix.Translation(V(loc)) @ Matrix.Rotation(rot_z, 4, "Y") @ Matrix.Rotation(math.radians(90), 4, "X")
            toward = Vector((0, -1, 0))
        else:
            tf = Matrix.Translation(V(loc)) @ Matrix.Rotation(rot_z, 4, "Z")
            toward = Vector((0, 0, 1))
        for p in me.polygons:
            pts = [tf @ me.vertices[i].co for i in p.vertices]
            self.poly(mat, pts, toward=toward)
        bpy.data.objects.remove(o)
        bpy.data.curves.remove(c)

    # --- xuất object ---------------------------------------------------------------
    def build(self, origin=(0, 0, 0)):
        """Chốt thành object, đặt gốc tọa độ tại `origin` (bản lề với Lid)."""
        me = bpy.data.meshes.new(self.name)
        self.bm.to_mesh(me)
        self.bm.free()
        for m in sorted(self.idx, key=self.idx.get):
            me.materials.append(self.scene.mat(m))
        for p in me.polygons:
            p.use_smooth = False
        o = bpy.data.objects.new(self.name, me)
        bpy.context.scene.collection.objects.link(o)
        p = V(origin)
        for v in me.vertices:
            v.co -= p
        o.location = p
        return o


def rrect(w, d, c):
    """8 điểm hình chữ nhật cắt góc (ngược chiều kim đồng hồ nhìn từ +Z), bắt đầu ở mép trước bên phải."""
    hw, hd = w / 2, d / 2
    return [(hw - c, -hd), (hw, -hd + c), (hw, hd - c), (hw - c, hd),
            (-hw + c, hd), (-hw, hd - c), (-hw, -hd + c), (-hw + c, -hd)]


def ring_at(pts, z):
    return [Vector((x, y, z)) for x, y in pts]


def shell(part, mats, w, d, c, z0, z1, wall, floor_z=None, cap_bottom=True):
    """Hộp vỏ chữ nhật cắt góc, hở miệng ở z1. mats: dict outer, rim, inner. floor_z None = hộp kín phía trên (có mặt trên)."""
    outer0 = ring_at(rrect(w, d, c), z0)
    outer1 = ring_at(rrect(w, d, c), z1)
    part.ring_band(mats["outer"], outer0, outer1)
    if cap_bottom:
        part.poly(mats.get("bottom", mats["outer"]), outer0[::-1], toward=(0, 0, -1))
    if floor_z is None:
        part.poly(mats.get("top", mats["outer"]), outer1, toward=(0, 0, 1))
        return outer0, outer1, None
    inner1 = ring_at(rrect(w - 2 * wall, d - 2 * wall, max(c - wall, 0.002)), z1)
    innerf = ring_at(rrect(w - 2 * wall, d - 2 * wall, max(c - wall, 0.002)), floor_z)
    for i in range(len(outer1)):
        j = (i + 1) % len(outer1)
        part.poly(mats["rim"], [outer1[i], outer1[j], inner1[j], inner1[i]], toward=(0, 0, 1))
        mid = (inner1[i] + inner1[j]) / 2
        part.poly(mats["inner"], [inner1[i], inner1[j], innerf[j], innerf[i]], toward=Vector((-mid.x, -mid.y, 0)))
    part.poly(mats["inner"], innerf, toward=(0, 0, 1))
    return outer0, outer1, inner1


def patch(part, mat, center, normal, radius, n=7, seed=1, squash=1.0, up=(0, 0, 1), flat_off=0.0015):
    """Mảng phẳng méo (gỉ, vết bẩn, miếng vá) dán trên bề mặt: tâm, pháp tuyến, bán kính."""
    rng = random.Random(seed)
    nv = V(normal).normalized()
    t = V(up).cross(nv)
    if t.length < 1e-6:
        t = Vector((1, 0, 0))
    t.normalize()
    u = nv.cross(t).normalized()
    c = V(center) + nv * flat_off
    ring = []
    for i in range(n):
        a = 2 * math.pi * i / n
        r = radius * (0.65 + 0.35 * rng.random())
        ring.append(c + t * (r * math.cos(a)) + u * (r * math.sin(a) * squash))
    part.fan(mat, c, ring, toward=nv)


def export(scene, root_name, parts, hinge_part="Lid", out=None):
    """Gắn các lớp vào root, xuất GLB, in số tam giác và kích thước."""
    root = bpy.data.objects.new(root_name, None)
    bpy.context.scene.collection.objects.link(root)
    tris = {}
    for o in parts:
        o.parent = root
        o.data.calc_loop_triangles()
        tris[o.name] = len(o.data.loop_triangles)
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for c in root.children:
        c.select_set(True)
    os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=out, export_format="GLB", use_selection=True, export_apply=True)
    # kích thước ở tư thế đóng, bỏ Glow
    lo = Vector((1e9, 1e9, 1e9))
    hi = Vector((-1e9, -1e9, -1e9))
    for o in parts:
        if o.name == "Glow":
            continue
        for v in o.data.vertices:
            p = o.matrix_world @ v.co
            lo = Vector((min(lo.x, p.x), min(lo.y, p.y), min(lo.z, p.z)))
            hi = Vector((max(hi.x, p.x), max(hi.y, p.y), max(hi.z, p.z)))
    size = hi - lo
    total = sum(tris.values())
    no_glow = total - tris.get("Glow", 0)
    print("triangles:", tris, "total:", total, "(không Glow:", no_glow, ")")
    print("size (x rộng, y sâu, z cao) m: %.3f x %.3f x %.3f" % (size.x, size.y, size.z), " z đáy: %.3f" % lo.z)
    lid = next(o for o in parts if o.name == hinge_part)
    ys = [lid.matrix_world @ v.co for v in lid.data.vertices]
    y0, y1 = min(p.y for p in ys), max(p.y for p in ys)
    frac = (lid.location.y - y0) / (y1 - y0) if y1 > y0 else 0
    print("bản lề Lid: y=%.3f, nắp y %.3f..%.3f, %.0f%% từ mép trước" % (lid.location.y, y0, y1, frac * 100))
    print("->", out)
    return tris, size

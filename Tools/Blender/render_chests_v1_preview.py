"""Ảnh xem trước Cycles cho 4 rương v1: đóng, hé, mở toang (hàng trên) + chính diện, hông, sau 3/4 (hàng dưới).

Chạy: python render_chests_v1_preview.py -- <thung_go|hop_ton|bao_tai|vali_den> [mẫu Cycles=48] [rộng mỗi khung px=640]
Ra: Assets/_Project/Art/Source/render_chest_<tên>_open.png. Dựng ngoại tuyến bằng Blender, KHÔNG phải ảnh chụp từ Unity.
"""
import math
import os
import sys
import tempfile

import bpy
from mathutils import Vector
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
CHESTS = {
    # khóa: (file glb, file ảnh, màu đèn lòng rương, độ cao mục tiêu camera, góc mở toang)
    "thung_go": ("Chest_ThungGo.glb", "render_chest_thung_go_open.png", (1.0, 0.78, 0.40), 0.16, 105),
    "hop_ton": ("Chest_HopTon.glb", "render_chest_hop_ton_open.png", (0.70, 0.90, 1.0), 0.16, 105),
    "bao_tai": ("Chest_BaoTai.glb", "render_chest_bao_tai_open.png", (1.0, 0.80, 0.45), 0.19, 105),
    "vali_den": ("Chest_ValiDen.glb", "render_chest_vali_den_open.png", (1.0, 0.45, 0.30), 0.17, 105),
}
args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
KEY = args[0]
SAMPLES = int(args[1]) if len(args) > 1 else 48
W = int(args[2]) if len(args) > 2 else 640
H = int(W * 0.75)
GLB, PNG, LCOL, TZ, OPEN = CHESTS[KEY]

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT, "Assets/_Project/Art/Chests", GLB))
sc = bpy.context.scene
OB = bpy.data.objects
lid = OB["Lid"]
lid.rotation_mode = "XYZ"
glow = OB["Glow"]
gs0 = tuple(glow.scale)
sc.render.engine = "CYCLES"
sc.cycles.device = "CPU"
sc.cycles.samples = SAMPLES
sc.cycles.use_denoising = True
sc.render.resolution_x = W
sc.render.resolution_y = H
sc.view_settings.view_transform = "AgX"
w = bpy.data.worlds.new("w")
sc.world = w
w.use_nodes = True
w.node_tree.nodes["Background"].inputs["Color"].default_value = (0.10, 0.095, 0.13, 1)
w.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.35
bpy.ops.mesh.primitive_plane_add(size=8)
fl = bpy.context.active_object
fm = bpy.data.materials.new("f")
fm.use_nodes = True
fm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.09, 0.085, 0.10, 1)
fl.data.materials.append(fm)


def light(loc, e, size, col=(1, 1, 1)):
    d = bpy.data.lights.new("a", "AREA")
    d.energy, d.size, d.color = e, size, col
    o = bpy.data.objects.new("a", d)
    sc.collection.objects.link(o)
    o.location = loc
    o.rotation_euler = (Vector((0, 0, 0.2)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()


light((-1.0, -1.2, 1.6), 200, 1.4, (1, .93, .85))
light((1.2, 0.6, 1.2), 60, 1.6, (.7, .8, 1))
light((0, 1.2, 1.4), 50, 1.2)
pl = bpy.data.lights.new("in", "POINT")
pl.color = LCOL
pl.shadow_soft_size = 0.03
po = bpy.data.objects.new("in", pl)
sc.collection.objects.link(po)
po.location = (0, 0, TZ)
cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam"))
sc.collection.objects.link(cam)
sc.camera = cam
cam.data.lens = 50


def pose(open_deg, g):
    lid.rotation_euler = (math.radians(-open_deg), 0, 0)
    glow.scale = tuple(s * max(1e-4, g) for s in gs0)
    glow.hide_render = g < 0.01
    pl.energy = 6 * g


def view(az, el, dist=1.7):
    a, e = math.radians(az), math.radians(el)
    t = Vector((0, 0, TZ))
    cam.location = t + Vector((math.sin(a) * math.cos(e), -math.cos(a) * math.cos(e), math.sin(e))) * dist
    cam.rotation_euler = (t - cam.location).to_track_quat("-Z", "Y").to_euler()


tmp = tempfile.mkdtemp()


def shot(name):
    p = os.path.join(tmp, name + ".png")
    sc.render.filepath = p
    bpy.ops.render.render(write_still=True)
    return Image.open(p).convert("RGB")


panels = []
view(28, 24); pose(0, 0); panels.append(("Đóng nắp", shot("closed")))
pose(28, 0.5); panels.append(("Hé nắp", shot("ajar")))
view(18, 34); pose(OPEN, 1); panels.append(("Mở toang", shot("open")))
pose(0, 0)
view(0, 6); panels.append(("Chính diện (đóng)", shot("front")))
view(90, 8); panels.append(("Hông (đóng)", shot("side")))
view(205, 28); panels.append(("Sau 3/4 (đóng)", shot("back")))
F = ImageFont.truetype("DejaVuSans.ttf", 16)
sheet = Image.new("RGB", (W * 3, (H + 28) * 2), (20, 19, 24))
d = ImageDraw.Draw(sheet)
for i, (t, im) in enumerate(panels):
    x, y = (i % 3) * W, (i // 3) * (H + 28)
    sheet.paste(im, (x, y + 28))
    d.text((x + 10, y + 6), t, fill=(230, 225, 210), font=F)
out = os.path.join(ROOT, "Assets/_Project/Art/Source", PNG)
sheet.save(out)
print("->", out)

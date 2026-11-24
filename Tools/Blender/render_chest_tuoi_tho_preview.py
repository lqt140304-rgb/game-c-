"""Ảnh xem trước Cycles cho Chest_TuoiTho.glb: đóng, hé, mở toang + 3 góc hình bóng.
Chạy: python render_chest_tuoi_tho_preview.py -- <samples> <rộng px>   (mặc định 48 640)
Ra: Assets/_Project/Art/Source/render_chest_tuoi_tho_open.png
"""
import bpy, math, os, sys, tempfile
from mathutils import Vector
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
SAMPLES = int(args[0]) if args else 48
W = int(args[1]) if len(args) > 1 else 640
H = int(W * 0.75)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT, "Assets/_Project/Art/Chests/Chest_TuoiTho.glb"))
sc = bpy.context.scene; OB = bpy.data.objects
OB["Lid"].rotation_mode = "XYZ"
glow = OB["Glow"]; gs0 = tuple(glow.scale)
sc.render.engine = "CYCLES"; sc.cycles.device = "CPU"; sc.cycles.samples = SAMPLES; sc.cycles.use_denoising = True
sc.render.resolution_x = W; sc.render.resolution_y = H
sc.view_settings.view_transform = "AgX"
w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True
w.node_tree.nodes["Background"].inputs["Color"].default_value = (0.10, 0.095, 0.13, 1)
w.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.35
bpy.ops.mesh.primitive_plane_add(size=8); fl = bpy.context.active_object
fm = bpy.data.materials.new("f"); fm.use_nodes = True
fm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.07, 0.065, 0.085, 1)
fl.data.materials.append(fm)
def light(loc, e, size, col=(1, 1, 1)):
    d = bpy.data.lights.new("a", "AREA"); d.energy = e; d.size = size; d.color = col
    o = bpy.data.objects.new("a", d); sc.collection.objects.link(o); o.location = loc
    o.rotation_euler = (Vector((0, 0, 0.2)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
light((-1.0, -1.2, 1.6), 200, 1.4, (1, .93, .85)); light((1.2, 0.6, 1.2), 60, 1.6, (.7, .8, 1)); light((0, 1.2, 1.4), 50, 1.2)
pl = bpy.data.lights.new("in", "POINT"); pl.color = (1, .78, .4); pl.shadow_soft_size = 0.03
po = bpy.data.objects.new("in", pl); sc.collection.objects.link(po); po.location = (0, 0, 0.16)
cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera = cam
cam.data.lens = 50

def pose(lid, g):
    OB["Lid"].rotation_euler = (math.radians(-lid), 0, 0)
    glow.scale = tuple(s * max(1e-4, g) for s in gs0); glow.hide_render = g < 0.01
    pl.energy = 6 * g

def view(az, el, dist=1.75, tgt=(0, 0, 0.17)):
    a, e = math.radians(az), math.radians(el); t = Vector(tgt)
    cam.location = t + Vector((math.sin(a) * math.cos(e), -math.cos(a) * math.cos(e), math.sin(e))) * dist
    cam.rotation_euler = (t - cam.location).to_track_quat("-Z", "Y").to_euler()

tmp = tempfile.mkdtemp()
def shot(name, silhouette=False):
    fl.hide_render = silhouette; sc.render.film_transparent = silhouette
    sc.cycles.samples = 4 if silhouette else SAMPLES
    p = os.path.join(tmp, name + ".png"); sc.render.filepath = p
    bpy.ops.render.render(write_still=True)
    im = Image.open(p).convert("RGBA")
    if silhouette:
        a = im.split()[3].point(lambda v: 255 if v > 30 else 0)
        bg = Image.new("RGB", im.size, (236, 232, 222)); bg.paste((30, 28, 34), mask=a); return bg
    return im.convert("RGB")

panels = []
view(28, 24); pose(0, 0); panels.append(("Đóng nắp", shot("closed")))
pose(28, 0.5); panels.append(("Hé nắp", shot("ajar")))
view(18, 34); pose(105, 1); panels.append(("Mở toang", shot("open")))
pose(0, 0)
view(0, 4); panels.append(("Hình bóng: chính diện", shot("s_front", True)))
view(90, 4); panels.append(("Hình bóng: cạnh", shot("s_side", True)))
view(35, 55); panels.append(("Hình bóng: 3/4 từ trên", shot("s_top", True)))
F = ImageFont.truetype("DejaVuSans.ttf", 16)
sheet = Image.new("RGB", (W * 3, (H + 28) * 2), (20, 19, 24)); d = ImageDraw.Draw(sheet)
for i, (t, im) in enumerate(panels):
    x, y = (i % 3) * W, (i // 3) * (H + 28)
    sheet.paste(im, (x, y + 28)); d.text((x + 10, y + 6), t, fill=(230, 225, 210), font=F)
out = os.path.join(ROOT, "Assets/_Project/Art/Source/render_chest_tuoi_tho_open.png")
sheet.save(out); print("->", out)

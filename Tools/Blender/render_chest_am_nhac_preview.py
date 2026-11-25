"""Cycles preview sheet for Chest_AmNhac.glb (lighting and camera follow cycles_chest_preview_setup.py).

Run: python render_chest_am_nhac_preview.py -- <Chest_AmNhac.glb> <out.png> [samples] [tile width]
Tiles: closed / ajar / wide open (record turned to show the spin) on the top row,
silhouettes from the front, the side and the back three-quarter on the bottom row.
"""
import math, os, sys, tempfile
import bpy
from mathutils import Vector
from PIL import Image, ImageDraw, ImageFont

args = sys.argv[sys.argv.index("--") + 1:]
GLB, OUT = args[0], args[1]
SAMPLES = int(args[2]) if len(args) > 2 else 64
TW = int(args[3]) if len(args) > 3 else 640
TH = int(TW * 0.75)

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=GLB)
sc = bpy.context.scene
OB = bpy.data.objects
for n in ("Lid", "Record", "Seal", "Binding", "Glow"):
    OB[n].rotation_mode = "XYZ"
glow = OB["Glow"]
gs0 = tuple(glow.scale)
seal0, bind0 = tuple(OB["Seal"].location), tuple(OB["Binding"].location)

sc.render.engine = "CYCLES"
sc.cycles.device = "CPU"
sc.cycles.samples = SAMPLES
sc.cycles.use_denoising = True
sc.render.resolution_x, sc.render.resolution_y = TW, TH
names = [t.identifier for t in sc.view_settings.bl_rna.properties["view_transform"].enum_items]
sc.view_settings.view_transform = "AgX" if "AgX" in names else "Standard"
w = bpy.data.worlds.new("w")
sc.world = w
w.use_nodes = True
bg = w.node_tree.nodes["Background"]
bg.inputs["Color"].default_value = (0.10, 0.095, 0.13, 1)
bg.inputs["Strength"].default_value = 0.35
bpy.ops.mesh.primitive_plane_add(size=6, location=(0, 0, 0))
fm = bpy.data.materials.new("floor")
fm.use_nodes = True
fb = fm.node_tree.nodes["Principled BSDF"]
fb.inputs["Base Color"].default_value = (0.07, 0.065, 0.085, 1)
fb.inputs["Roughness"].default_value = 0.85
bpy.context.active_object.data.materials.append(fm)


def light(kind, loc, energy, size, color=(1, 1, 1)):
    d = bpy.data.lights.new(kind, kind)
    d.energy, d.color = energy, color
    if kind == "AREA":
        d.size = size
    o = bpy.data.objects.new(kind, d)
    sc.collection.objects.link(o)
    o.location = loc
    o.rotation_euler = (Vector((0, 0, 0.25)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
    return o


light("AREA", (-0.8, -1.0, 1.4), 140, 1.2, (1, .93, .85))
light("AREA", (1.0, 0.6, 1.0), 40, 1.5, (.7, .8, 1))
light("AREA", (0, 0.9, 1.2), 30, 1.0)
inner = bpy.data.lights.new("in", "POINT")
inner.color, inner.shadow_soft_size = (1, .8, .45), 0.03
inl = bpy.data.objects.new("in", inner)
sc.collection.objects.link(inl)
inl.location = (-0.02, -0.01, 0.30)
cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam"))
sc.collection.objects.link(cam)
sc.camera = cam
cam.data.lens = 50


def pose(lid, glw, seal, bind, spin=0.0):
    OB["Lid"].rotation_euler = (math.radians(-lid), 0, 0)
    glow.scale = tuple(s * max(1e-4, glw) for s in gs0)
    glow.hide_render = glw < 0.01
    inner.energy = 6 * glw
    OB["Record"].rotation_euler = (0, 0, math.radians(spin))
    OB["Seal"].hide_render = seal
    OB["Binding"].hide_render = bind


def camera(az, el=0.42, dist=1.45, tz=0.22):
    a = math.radians(az)
    tgt = Vector((0, 0, tz))
    cam.location = tgt + Vector((math.sin(a) * dist, -math.cos(a) * dist, el * dist))
    cam.rotation_euler = (tgt - cam.location).to_track_quat("-Z", "Y").to_euler()


tiles = [
    ("Đóng nắp", dict(lid=0, glw=0, seal=False, bind=False), dict(az=28)),
    ("Hé nắp (đã gỡ niêm, dây)", dict(lid=24, glw=0.45, seal=True, bind=True, spin=40), dict(az=28)),
    ("Mở toang, đĩa quay", dict(lid=108, glw=1, seal=True, bind=True, spin=120), dict(az=28, el=0.75, dist=1.5)),
    ("Chính diện", dict(lid=0, glw=0, seal=False, bind=False), dict(az=0, el=0.12)),
    ("Bên hông", dict(lid=0, glw=0, seal=False, bind=False), dict(az=90, el=0.12)),
    ("Sau 3/4", dict(lid=0, glw=0, seal=False, bind=False), dict(az=145, el=0.35)),
]
tmp = tempfile.mkdtemp()
imgs = []
for i, (label, p, c) in enumerate(tiles):
    pose(**p)
    camera(**c)
    sc.render.filepath = os.path.join(tmp, f"t{i}.png")
    bpy.ops.render.render(write_still=True)
    imgs.append((label, Image.open(sc.render.filepath).convert("RGB")))

sheet = Image.new("RGB", (TW * 3, TH * 2), (20, 19, 26))
draw = ImageDraw.Draw(sheet)
try:
    font = ImageFont.truetype("DejaVuSans.ttf", max(14, TW // 28))
except OSError:
    font = ImageFont.load_default()
for i, (label, im) in enumerate(imgs):
    x, y = (i % 3) * TW, (i // 3) * TH
    sheet.paste(im, (x, y))
    draw.text((x + 12, y + 10), label, fill=(235, 230, 220), font=font)
sheet.save(OUT)
print("saved", OUT)

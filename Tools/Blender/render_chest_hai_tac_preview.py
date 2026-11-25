"""Cycles preview sheet for Chest_HaiTac (offline render, not a Unity screenshot).

Run: python render_chest_hai_tac_preview.py -- [samples] [width]   (defaults 48, 640)
Writes Assets/_Project/Art/Source/render_chest_hai_tac_open.png:
  top row    closed / cracked open / wide open, three-quarter front view, with the inner glow
  bottom row closed from the front, the side and the back three-quarter (silhouette check)
Lighting follows Tools/Blender/cycles_chest_preview_setup.py.
"""
import math, os, sys, tempfile
import bpy
from mathutils import Vector
from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
GLB = os.path.join(ROOT, "Assets", "_Project", "Art", "Chests", "Chest_HaiTac.glb")
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Source", "render_chest_hai_tac_open.png")
args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
SAMPLES = int(args[0]) if len(args) > 0 else 48
WIDTH = int(args[1]) if len(args) > 1 else 640

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=GLB)
sc = bpy.context.scene
OB = bpy.data.objects
for n in ("Lid", "Key", "Glow", "Binding", "Seal"):
    OB[n].rotation_mode = "XYZ"
glow = OB["Glow"]
gs0 = tuple(glow.scale)
for m in glow.data.materials:
    b = m.node_tree.nodes.get("Principled BSDF")
    if b and b.inputs["Emission Strength"].default_value > 0:
        b.inputs["Emission Strength"].default_value = 0.9

sc.render.engine = "CYCLES"
sc.cycles.device = "CPU"
sc.cycles.samples = SAMPLES
sc.cycles.use_denoising = True
sc.render.resolution_x = WIDTH
sc.render.resolution_y = int(WIDTH * 0.75)
names = [t.identifier for t in sc.view_settings.bl_rna.properties["view_transform"].enum_items]
sc.view_settings.view_transform = "AgX" if "AgX" in names else "Standard"

w = bpy.data.worlds.new("w")
sc.world = w
w.use_nodes = True
bg = w.node_tree.nodes["Background"]
bg.inputs["Color"].default_value = (0.10, 0.095, 0.13, 1)
bg.inputs["Strength"].default_value = 0.35
bpy.ops.mesh.primitive_plane_add(size=6, location=(0, 0, 0))
fl = bpy.context.active_object
fm = bpy.data.materials.new("floor")
fm.use_nodes = True
fb = fm.node_tree.nodes["Principled BSDF"]
fb.inputs["Base Color"].default_value = (0.07, 0.065, 0.085, 1)
fb.inputs["Roughness"].default_value = 0.85
fl.data.materials.append(fm)


def light(kind, loc, energy, size, color=(1, 1, 1)):
    d = bpy.data.lights.new(kind, kind)
    d.energy = energy
    d.color = color
    if kind == "AREA":
        d.size = size
    o = bpy.data.objects.new(kind, d)
    sc.collection.objects.link(o)
    o.location = loc
    o.rotation_euler = (Vector((0, 0, 0.2)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
    return o


light("AREA", (-0.8, -1.0, 1.4), 140, 1.2, (1, .93, .85))
light("AREA", (1.0, 0.6, 1.0), 40, 1.5, (.7, .8, 1))
light("AREA", (0, 0.9, 1.2), 30, 1.0)
inner = bpy.data.lights.new("in", "POINT")
inner.color = (1, .8, .45)
inner.shadow_soft_size = 0.03
inl = bpy.data.objects.new("in", inner)
sc.collection.objects.link(inl)
inl.location = (0, 0.0, 0.36)
cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam"))
sc.collection.objects.link(cam)
sc.camera = cam
cam.data.lens = 50


def set_pose(lid, glw, bound=True):
    OB["Lid"].rotation_euler = (math.radians(-lid), 0, 0)      # hinge at the back (+Y), front is -Y
    glow.scale = tuple(s * max(1e-4, glw) for s in gs0)
    glow.hide_render = glw < 0.01
    inner.energy = 6 * glw
    for n in ("Binding", "Seal", "Key"):
        OB[n].hide_render = not bound


def set_cam(az, dist=1.35, height=0.42, tz=0.22):
    a = math.radians(az)
    tgt = Vector((0, 0, tz))
    cam.location = tgt + Vector((math.sin(a) * dist, -math.cos(a) * dist, height * dist))
    cam.rotation_euler = (tgt - cam.location).to_track_quat("-Z", "Y").to_euler()


shots = [  # label, lid angle, glow, binding on, camera azimuth, camera distance
    ("Dong nap", 0, 0.0, True, -28, 1.35),
    ("He nap", 12, 0.4, False, -28, 1.35),
    ("Mo toang", 105, 1.0, False, -28, 1.45),
    ("Truoc", 0, 0.0, True, 0, 1.35),
    ("Ben canh", 0, 0.0, True, 90, 1.35),
    ("Sau 3/4", 0, 0.0, True, 145, 1.35),
]
tmp = tempfile.mkdtemp()
frames = []
for i, (label, lid, glw, bound, az, dist) in enumerate(shots):
    set_pose(lid, glw, bound)
    set_cam(az, dist)
    sc.render.filepath = os.path.join(tmp, f"f{i}.png")
    bpy.ops.render.render(write_still=True)
    frames.append((label, Image.open(sc.render.filepath).convert("RGB")))

fw, fh = frames[0][1].size
sheet = Image.new("RGB", (fw * 3, fh * 2), (20, 19, 26))
draw = ImageDraw.Draw(sheet)
try:
    font = ImageFont.truetype("DejaVuSans-Bold.ttf", max(14, WIDTH // 30))
except OSError:
    font = ImageFont.load_default()
for i, (label, im) in enumerate(frames):
    x, y = (i % 3) * fw, (i // 3) * fh
    sheet.paste(im, (x, y))
    draw.text((x + 12, y + 10), label, fill=(235, 225, 200), font=font)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
sheet.save(OUT)
print("saved", OUT)

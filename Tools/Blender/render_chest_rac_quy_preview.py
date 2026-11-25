"""Cycles preview sheet for Chest_RacQuy.glb (lighting/camera based on cycles_chest_preview_setup.py).

Run: python render_chest_rac_quy_preview.py -- <work dir> [samples=48] [width=640]
Writes Assets/_Project/Art/Source/render_chest_rac_quy_open.png:
  row 1  closed / lid ajar (25 deg) / wide open (110 deg), three-quarter front view
  row 2  closed seen from the front, the side and the back (silhouette check)
  row 3  black silhouettes next to the existing low-poly boxes H1 H2 H3 RD
Offline renders only: not a Unity screenshot.
"""
import math, os, sys, glob
import bpy
from mathutils import Vector
from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.normpath(os.path.join(HERE, "..", ".."))
ART = os.path.join(REPO, "Assets", "_Project", "Art")
ARGS = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
WORK = ARGS[0] if ARGS else "/tmp/racquy_render"
SAMPLES = int(ARGS[1]) if len(ARGS) > 1 else 48
W = int(ARGS[2]) if len(ARGS) > 2 else 640
os.makedirs(WORK, exist_ok=True)


def base_scene(samples, w, h):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    sc = bpy.context.scene
    sc.render.engine = "CYCLES"
    sc.cycles.device = "CPU"
    sc.cycles.samples = samples
    sc.cycles.use_denoising = True
    sc.render.resolution_x, sc.render.resolution_y = w, h
    names = [t.identifier for t in sc.view_settings.bl_rna.properties["view_transform"].enum_items]
    sc.view_settings.view_transform = "AgX" if "AgX" in names else "Standard"
    wd = bpy.data.worlds.new("w")
    sc.world = wd
    wd.use_nodes = True
    return sc, wd.node_tree.nodes["Background"]


def light(sc, kind, loc, energy, size, color=(1, 1, 1), aim=(0, 0, 0.2)):
    d = bpy.data.lights.new(kind, kind)
    d.energy, d.color = energy, color
    if kind == "AREA":
        d.size = size
    o = bpy.data.objects.new(kind, d)
    sc.collection.objects.link(o)
    o.location = loc
    o.rotation_euler = (Vector(aim) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
    return o


def camera(sc, lens=50, ortho=None):
    cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam"))
    sc.collection.objects.link(cam)
    sc.camera = cam
    cam.data.lens = lens
    if ortho:
        cam.data.type = "ORTHO"
        cam.data.ortho_scale = ortho
    return cam


def aim(cam, tgt, az, dist, elev):
    a = math.radians(az)
    e = math.radians(elev)
    tgt = Vector(tgt)
    cam.location = tgt + Vector((math.sin(a) * math.cos(e), -math.cos(a) * math.cos(e), math.sin(e))) * dist
    cam.rotation_euler = (tgt - cam.location).to_track_quat("-Z", "Y").to_euler()


def render(path):
    bpy.context.scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    return path


# ------------------------------------------------------------------ rows 1 and 2: the chest in Cycles
H = int(W * 0.8)
sc, bg = base_scene(SAMPLES, W, H)
bg.inputs["Color"].default_value = (0.10, 0.095, 0.13, 1)
bg.inputs["Strength"].default_value = 0.35
bpy.ops.import_scene.gltf(filepath=os.path.join(ART, "Chests", "Chest_RacQuy.glb"))
OB = bpy.data.objects
lid, glow = OB["Lid"], OB["Glow"]
lid.rotation_mode = "XYZ"
gs0 = tuple(glow.scale)
bpy.ops.mesh.primitive_plane_add(size=6)
fl = bpy.context.active_object
fm = bpy.data.materials.new("floor")
fm.use_nodes = True
fb = fm.node_tree.nodes["Principled BSDF"]
fb.inputs["Base Color"].default_value = (0.07, 0.065, 0.085, 1)
fb.inputs["Roughness"].default_value = 0.85
fl.data.materials.append(fm)
light(sc, "AREA", (-0.8, -1.0, 1.4), 140, 1.2, (1, .93, .85))
light(sc, "AREA", (1.0, 0.6, 1.0), 45, 1.5, (.7, .8, 1))
light(sc, "AREA", (0, 0.9, 1.2), 35, 1.0)
inner = bpy.data.lights.new("in", "POINT")
inner.color, inner.shadow_soft_size = (0.75, 1.0, 0.45), 0.03
inl = bpy.data.objects.new("in", inner)
sc.collection.objects.link(inl)
inl.location = (0, 0, 0.33)
cam = camera(sc, 50)


def pose(lid_deg, g):
    lid.rotation_euler = (math.radians(-lid_deg), 0, 0)
    glow.scale = tuple(s * max(1e-4, g) for s in gs0)
    glow.hide_render = g < 0.01
    inner.energy = 6 * g


shots = []
for tag, lid_deg, g in (("closed", 0, 0), ("ajar", 25, 0.45), ("open", 110, 1.0)):
    pose(lid_deg, g)
    aim(cam, (0, 0, 0.27), 28, 1.65, 22)
    shots.append((render(os.path.join(WORK, f"a_{tag}.png")), {"closed": "Đóng nắp", "ajar": "Hé nắp", "open": "Mở toang"}[tag]))
pose(0, 0)
for tag, az, label in (("front", 0, "Trước"), ("side", 90, "Bên phải"), ("back", 200, "Sau (bản lề, xương cá)")):
    aim(cam, (0, 0, 0.22), az, 1.45, 8)
    shots.append((render(os.path.join(WORK, f"b_{tag}.png")), label))

# ------------------------------------------------------------------ row 3: silhouettes beside the existing boxes
SW, SH = W * 3, int(W * 0.62)
sc, bg = base_scene(8, SW, SH)
bg.inputs["Color"].default_value = (0.93, 0.92, 0.89, 1)
bg.inputs["Strength"].default_value = 1.0
black = bpy.data.materials.new("black")
black.use_nodes = True
nt = black.node_tree
nt.nodes.clear()
em = nt.nodes.new("ShaderNodeEmission")
em.inputs["Color"].default_value = (0.02, 0.02, 0.025, 1)
out = nt.nodes.new("ShaderNodeOutputMaterial")
nt.links.new(em.outputs[0], out.inputs[0])
files = [os.path.join(ART, "Items", "LowPoly", "Boxes", f"{n}.glb") for n in ("H1", "H2", "H3", "RD")] + [os.path.join(ART, "Chests", "Chest_RacQuy.glb")]
for i, f in enumerate(files):
    before = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=f)
    new = [o for o in bpy.data.objects if o not in before]
    roots = [o for o in new if o.parent is None]
    for o in new:
        if o.name.startswith("Glow"):
            o.hide_render = True
        if o.type == "MESH":
            o.data.materials.clear()
            o.data.materials.append(black)
    for r in roots:
        r.location.x = (i - 2) * 0.5
cam = camera(sc, ortho=2.55)
aim(cam, (0, 0, 0.215), 0, 4.0, 4)
sil = render(os.path.join(WORK, "c_sil.png"))

# ------------------------------------------------------------------ compose the sheet
fonts = glob.glob("/usr/share/fonts/**/DejaVuSans-Bold.ttf", recursive=True)
font = ImageFont.truetype(fonts[0], max(16, W // 28)) if fonts else ImageFont.load_default()
pad = 8
LBL = max(28, W // 14)
sheet = Image.new("RGB", (W * 3 + pad * 4, H * 2 + SH + LBL + pad * 4), (24, 22, 30))
d = ImageDraw.Draw(sheet)
for k, (p, label) in enumerate(shots):
    x, y = pad + (k % 3) * (W + pad), pad + (k // 3) * (H + pad)
    sheet.paste(Image.open(p).convert("RGB"), (x, y))
    d.text((x + 12, y + 10), label, fill=(240, 236, 220), font=font, stroke_width=2, stroke_fill=(0, 0, 0))
ys = pad + 2 * (H + pad)
sheet.paste(Image.open(sil).convert("RGB").resize((W * 3 + pad * 2, SH)), (pad, ys))
for i, n in enumerate(("H1", "H2", "H3", "RD", "Rác Quý")):
    cx = pad + int((W * 3 + pad * 2) * (0.5 + (i - 2) * 0.5 / 2.55))
    d.text((cx, ys + SH + LBL // 2 + 2), n, fill=(240, 236, 220), font=font, anchor="mm")
dst = os.path.join(ART, "Source", "render_chest_rac_quy_open.png")
os.makedirs(os.path.dirname(dst), exist_ok=True)
sheet.save(dst)
print("sheet ->", dst)

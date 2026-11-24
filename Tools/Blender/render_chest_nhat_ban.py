"""Ảnh xem trước Cycles cho Chest_NhatBan.glb: đóng / hé / mở toang + 3 hình bóng.
Chạy: python Tools/Blender/render_chest_nhat_ban.py [samples=48] [width=640]
Ra:   Assets/_Project/Art/Source/render_chest_nhat_ban_open.png"""
import bpy, math, os, sys, tempfile
from mathutils import Vector
from PIL import Image
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
args = [a for a in sys.argv[1:] if a.isdigit()]
SAMPLES = int(args[0]) if args else 48; W = int(args[1]) if len(args) > 1 else 640; H = int(W * 0.8)
OUT = os.path.join(ROOT, "Assets/_Project/Art/Source/render_chest_nhat_ban_open.png")
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT, "Assets/_Project/Art/Chests/Chest_NhatBan.glb"))
sc = bpy.context.scene; OB = bpy.data.objects
lid = OB["Lid"]; lid.rotation_mode = "XYZ"; glow = OB["Glow"]; gs0 = tuple(glow.scale)
sc.render.engine = "CYCLES"; sc.cycles.device = "CPU"; sc.cycles.samples = SAMPLES; sc.cycles.use_denoising = True
sc.render.resolution_x = W; sc.render.resolution_y = H
w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True
bg = w.node_tree.nodes["Background"]
bpy.ops.mesh.primitive_plane_add(size=6); fl = bpy.context.active_object
fm = bpy.data.materials.new("f"); fm.use_nodes = True; fl.data.materials.append(fm)
fb = fm.node_tree.nodes["Principled BSDF"]; fb.inputs["Roughness"].default_value = 0.85
def light(loc, e, s, c=(1, 1, 1)):
    d = bpy.data.lights.new("a", "AREA"); d.energy = e; d.size = s; d.color = c
    o = bpy.data.objects.new("a", d); sc.collection.objects.link(o); o.location = loc
    o.rotation_euler = (Vector((0, 0, 0.2)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
light((-0.8, -1.0, 1.4), 140, 1.2, (1, .93, .85)); light((1.0, 0.6, 1.0), 40, 1.5, (.7, .8, 1)); light((0, 0.9, 1.2), 30, 1.0)
pt = bpy.data.lights.new("in", "POINT"); pt.color = (1, .8, .45); pt.shadow_soft_size = 0.03
po = bpy.data.objects.new("in", pt); sc.collection.objects.link(po); po.location = (0, 0, 0.30)
cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera = cam
cam.data.type = "ORTHO"; cam.data.ortho_scale = 0.62
def shot(path, lid_deg, glw, az, el, sil=False):
    lid.rotation_euler = (math.radians(-lid_deg), 0, 0)
    glow.scale = tuple(s * max(1e-4, glw) for s in gs0); glow.hide_render = glw < 0.01; pt.energy = 4 * glw
    a, e = math.radians(az), math.radians(el); t = Vector((0, 0, 0.22))
    cam.location = t + 2 * Vector((math.sin(a) * math.cos(e), -math.cos(a) * math.cos(e), math.sin(e)))
    cam.rotation_euler = (t - cam.location).to_track_quat("-Z", "Y").to_euler()
    if sil:   # black silhouette on white
        sc.render.engine = "CYCLES"   # EEVEE needs a GPU/EGL, not available headless
        bg.inputs["Color"].default_value = (1, 1, 1, 1); bg.inputs["Strength"].default_value = 1
        fl.hide_render = True; sc.render.film_transparent = False
        sc.view_layers[0].material_override = blk
    else:
        sc.render.engine = "CYCLES"; sc.view_layers[0].material_override = None
        bg.inputs["Color"].default_value = (0.10, 0.095, 0.13, 1); bg.inputs["Strength"].default_value = 0.35
        fl.hide_render = False; fb.inputs["Base Color"].default_value = (0.07, 0.065, 0.085, 1)
    sc.render.filepath = path; bpy.ops.render.render(write_still=True)
blk = bpy.data.materials.new("blk"); blk.use_nodes = True
blk.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0, 0, 0, 1)
blk.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = 1
blk.node_tree.nodes["Principled BSDF"].inputs["Specular IOR Level"].default_value = 0
tmp = tempfile.mkdtemp(); shots = [
    ("closed", 0, 0, 24, 18, False), ("ajar", 28, 0.5, 24, 18, False), ("open", 105, 1, 24, 30, False),
    ("sil_front", 0, 0, 0, 0, True), ("sil_side", 0, 0, 90, 0, True), ("sil_34", 0, 0, 40, 25, True)]
ims = []
for n, *a in shots:
    p = os.path.join(tmp, n + ".png"); shot(p, *a); ims.append(Image.open(p).convert("RGB"))
sheet = Image.new("RGB", (W * 3, H * 2))
for i, im in enumerate(ims): sheet.paste(im, ((i % 3) * W, (i // 3) * H))
os.makedirs(os.path.dirname(OUT), exist_ok=True); sheet.save(OUT); print("->", OUT)

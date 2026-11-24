"""Cycles preview sheet for Chest_TrungHoa: closed / ajar / open, plus 3 silhouette angles.
Run: python render_chest_trung_hoa_preview.py -- <glb> <out.png> [samples]"""
import bpy, math, sys, os, tempfile
from mathutils import Vector
from PIL import Image, ImageDraw
a = sys.argv[sys.argv.index("--") + 1:]; GLB, OUT = a[0], a[1]; SAMPLES = int(a[2]) if len(a) > 2 else 48
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=GLB)
sc = bpy.context.scene; OB = bpy.data.objects; lid = OB["Lid"]; glow = OB["Glow"]; lid.rotation_mode = "XYZ"; g0 = tuple(glow.scale)
sc.render.engine = "CYCLES"; sc.cycles.device = "CPU"; sc.cycles.samples = SAMPLES; sc.cycles.use_denoising = True
sc.render.resolution_x, sc.render.resolution_y = 480, 400
w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True
bg = w.node_tree.nodes["Background"]; bg.inputs["Color"].default_value = (0.10, 0.095, 0.13, 1); bg.inputs["Strength"].default_value = 0.35
bpy.ops.mesh.primitive_plane_add(size=6); fl = bpy.context.active_object
fm = bpy.data.materials.new("f"); fm.use_nodes = True; fm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.07, 0.065, 0.085, 1); fl.data.materials.append(fm)
def light(loc, e, s, c=(1, 1, 1)):
    d = bpy.data.lights.new("l", "AREA"); d.energy = e; d.size = s; d.color = c
    o = bpy.data.objects.new("l", d); sc.collection.objects.link(o); o.location = loc
    o.rotation_euler = (Vector((0, 0, 0.2)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
light((-0.8, -1.0, 1.4), 140, 1.2, (1, .93, .85)); light((1.0, 0.6, 1.0), 40, 1.5, (.7, .8, 1)); light((0, 0.9, 1.2), 30, 1.0)
pt = bpy.data.lights.new("p", "POINT"); pt.color = (1, .8, .45); po = bpy.data.objects.new("p", pt); sc.collection.objects.link(po); po.location = (0, 0, 0.32)
cam = bpy.data.objects.new("c", bpy.data.cameras.new("c")); sc.collection.objects.link(cam); sc.camera = cam; cam.data.lens = 50
def shot(lid_deg, glw, az, el, dist, sil=False):
    lid.rotation_euler = (math.radians(-lid_deg), 0, 0)
    glow.scale = tuple(s * max(1e-4, glw) for s in g0); glow.hide_render = glw < 0.01; pt.energy = 3 * glw
    t = Vector((0, 0, 0.24)); r = math.radians(az); e = math.radians(el)
    cam.location = t + Vector((math.sin(r) * math.cos(e), -math.cos(r) * math.cos(e), math.sin(e))) * dist
    cam.rotation_euler = (t - cam.location).to_track_quat("-Z", "Y").to_euler()
    for o in OB:
        if o.type == "MESH" and o != fl: o.is_holdout = False
    sc.render.film_transparent = sil; fl.hide_render = sil
    f = os.path.join(tempfile.gettempdir(), f"th_{len(frames)}.png"); sc.render.filepath = f; bpy.ops.render.render(write_still=True)
    im = Image.open(f).convert("RGBA")
    if sil:
        bgc = Image.new("RGBA", im.size, (235, 230, 220, 255)); mask = im.split()[3].point(lambda v: 255 if v >= 128 else 0)
        bgc.paste((30, 28, 34, 255), (0, 0), mask); im = bgc
    frames.append(im.convert("RGB"))
frames = []
shot(0, 0, 28, 18, 1.25); shot(22, 0.5, 28, 18, 1.25); shot(105, 1, 28, 30, 1.25)
shot(0, 0, 0, 4, 1.4, True); shot(0, 0, 90, 4, 1.4, True); shot(0, 0, 45, 25, 1.4, True)
W, H = frames[0].size; sheet = Image.new("RGB", (W * 3, H * 2 + 24), (20, 20, 24)); d = ImageDraw.Draw(sheet)
labels = ["dong nap", "he nap", "mo toang", "bong: truoc", "bong: canh", "bong: 3/4"]
for i, (im, lb) in enumerate(zip(frames, labels)):
    sheet.paste(im, ((i % 3) * W, (i // 3) * (H + 12) + 12)); d.text(((i % 3) * W + 8, (i // 3) * (H + 12)), lb, fill=(220, 220, 220))
sheet.save(OUT); print("->", OUT)

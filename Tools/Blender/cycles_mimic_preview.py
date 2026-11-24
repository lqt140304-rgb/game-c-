"""Cycles preview of Chest_Mimic.glb: closed / ajar / open mouth (+ optional source pose), composited into one PNG.
Run: python cycles_mimic_preview.py -- <glb> <out.png> [samples=48] [width=640] [angles "0,12,60"] [azimuth=24] [distance=1.35] [elevation deg=16] [target z=0.17] [fbx scale=0.2796]
Lighting follows cycles_chest_preview_setup.py (soft studio, area lights, AgX), with a warm rim light since the Mimic is dark.
The lid opens with rotation_euler.x = -angle (same as BoxOpenSequence with lidOpenSign = 1)."""
import bpy, math, sys, os
from mathutils import Vector
from PIL import Image
a = sys.argv[sys.argv.index("--") + 1:]
glb, out = a[0], a[1]
samples = int(a[2]) if len(a) > 2 else 48
W = int(a[3]) if len(a) > 3 else 640
angles = [float(x) for x in (a[4] if len(a) > 4 else "0,12,60").split(",")]
az = float(a[5]) if len(a) > 5 else 24
dist = float(a[6]) if len(a) > 6 else 1.35
el = math.radians(float(a[7]) if len(a) > 7 else 16)
tz = float(a[8]) if len(a) > 8 else 0.17
bpy.ops.wm.read_factory_settings(use_empty=True)
sc = bpy.context.scene; OB = bpy.data.objects
if glb.lower().endswith(".fbx"):   # before/after: render the untouched Meshy FBX in its own (open) pose, scaled like the GLB
    k = float(a[9]) if len(a) > 9 else 0.2796
    bpy.ops.import_scene.fbx(filepath=glb)
    m = [o for o in sc.objects if o.type == "MESH"][0]; m.parent = None
    bpy.context.view_layer.objects.active = m; bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    zs = [v.co.z for v in m.data.vertices]; m.scale = (k, k, k); m.location = (0, 0, -min(zs) * k)
    lid = bpy.data.objects.new("Lid", None); sc.collection.objects.link(lid)
    glow = bpy.data.objects.new("Glow", None); sc.collection.objects.link(glow)
    glow.hide_render = True
else:
    bpy.ops.import_scene.gltf(filepath=glb)
    lid, glow = OB["Lid"], OB["Glow"]
lid.rotation_mode = "XYZ"; gs0 = tuple(glow.scale)
for m in (glow.data.materials if glow.type == "MESH" else []):
    b = m.node_tree.nodes.get("Principled BSDF")
    if b: b.inputs["Emission Strength"].default_value = 3.0
sc.render.engine = "CYCLES"; sc.cycles.device = "CPU"; sc.cycles.samples = samples; sc.cycles.use_denoising = True
try: sc.cycles.denoiser = "OPENIMAGEDENOISE"
except Exception as e: print("denoise?", e)
sc.render.resolution_x = W; sc.render.resolution_y = int(W * 0.75)
sc.view_settings.view_transform = "AgX"; sc.view_settings.exposure = -0.5
w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True
bg = w.node_tree.nodes["Background"]; bg.inputs["Color"].default_value = (0.12, 0.115, 0.15, 1); bg.inputs["Strength"].default_value = 0.6
bpy.ops.mesh.primitive_plane_add(size=6, location=(0, 0, 0)); fl = bpy.context.active_object
fm = bpy.data.materials.new("f"); fm.use_nodes = True; fb = fm.node_tree.nodes["Principled BSDF"]
fb.inputs["Base Color"].default_value = (0.09, 0.085, 0.105, 1); fb.inputs["Roughness"].default_value = 0.85; fl.data.materials.append(fm)
def light(kind, loc, energy, size, color=(1, 1, 1)):
    d = bpy.data.lights.new(kind, kind); d.energy = energy; d.color = color
    if kind == "AREA": d.size = size
    o = bpy.data.objects.new(kind, d); sc.collection.objects.link(o); o.location = loc
    o.rotation_euler = (Vector((0, 0, 0.2)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
light("AREA", (-0.8, -1.0, 1.4), 220, 1.2, (1, .93, .85)); light("AREA", (1.0, 0.6, 1.0), 70, 1.5, (.7, .8, 1))
light("AREA", (0, 0.9, 1.2), 50, 1.0); light("AREA", (0.0, -0.9, 0.5), 60, 1.0, (1, .85, .7))
inner = bpy.data.lights.new("in", "POINT"); inner.color = (1, .3, .1); inner.shadow_soft_size = 0.03
inl = bpy.data.objects.new("in", inner); sc.collection.objects.link(inl); inl.location = (0, -0.02, 0.12)
cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera = cam; cam.data.lens = 50
tgt = Vector((0, 0, tz)); A = math.radians(az)
cam.location = tgt + Vector((math.sin(A) * math.cos(el), -math.cos(A) * math.cos(el), math.sin(el))) * dist
cam.rotation_euler = (tgt - cam.location).to_track_quat("-Z", "Y").to_euler()
tmp = os.path.dirname(os.path.abspath(out)); frames = []
for i, ang in enumerate(angles):
    lid.rotation_euler = (math.radians(-ang), 0, 0)
    g = 0.0 if ang < 1 else min(1.0, 0.25 + ang / 50)
    glow.scale = tuple(s * max(1e-4, g) for s in gs0); glow.hide_render = g < 0.01
    inner.energy = 6 * g
    p = os.path.join(tmp, f"_pv{i}.png"); sc.render.filepath = p; bpy.ops.render.render(write_still=True); frames.append(p)
ims = [Image.open(p).convert("RGB") for p in frames]
sheet = Image.new("RGB", (W * len(ims), ims[0].height))
for i, im in enumerate(ims): sheet.paste(im, (i * W, 0))
sheet.save(out)
for p in frames: os.remove(p)
print("saved", out)

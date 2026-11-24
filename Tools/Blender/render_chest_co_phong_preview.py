"""Cycles preview for Chest_CoPhong.glb: closed, ajar, wide open + 3 silhouettes.
Run: python render_chest_co_phong_preview.py -- <glb> <out png> [samples] [width]"""
import bpy, math, sys, os
from mathutils import Vector
a = sys.argv[sys.argv.index("--") + 1:]
GLB, OUTP = a[0], a[1]; SAMP = int(a[2]) if len(a) > 2 else 48; W = int(a[3]) if len(a) > 3 else 520
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=GLB)
sc = bpy.context.scene; OB = bpy.data.objects
lid, glow, seal = OB["Lid"], OB["Glow"], OB["Seal"]; lid.rotation_mode = "XYZ"
sc.render.engine = "CYCLES"; sc.cycles.device = "CPU"; sc.cycles.samples = SAMP; sc.cycles.use_denoising = True
sc.render.resolution_x = W; sc.render.resolution_y = int(W * 1.0)
w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True
bg = w.node_tree.nodes["Background"]; bg.inputs["Color"].default_value = (0.10, 0.095, 0.13, 1); bg.inputs["Strength"].default_value = 0.35
bpy.ops.mesh.primitive_plane_add(size=6); fl = bpy.context.active_object
fm = bpy.data.materials.new("f"); fm.use_nodes = True
fm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.07, 0.065, 0.085, 1); fl.data.materials.append(fm)
def light(loc, e, s, c=(1, 1, 1)):
    d = bpy.data.lights.new("l", "AREA"); d.energy = e; d.size = s; d.color = c
    o = bpy.data.objects.new("l", d); sc.collection.objects.link(o); o.location = loc
    o.rotation_euler = (Vector((0, 0, 0.2)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
light((-0.8, -1.0, 1.4), 140, 1.2, (1, .93, .85)); light((1.0, 0.6, 1.0), 40, 1.5, (.7, .8, 1)); light((0, 0.9, 1.2), 30, 1.0)
pd = bpy.data.lights.new("in", "POINT"); pd.color = (1, .8, .45); pin = bpy.data.objects.new("in", pd); sc.collection.objects.link(pin); pin.location = (0, 0, 0.36)
cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera = cam; cam.data.lens = 50
gs = tuple(glow.scale)
def shot(name, ang, g, az, elev=0.45, sil=False):
    lid.rotation_euler = (math.radians(-ang), 0, 0); seal.hide_render = ang > 1
    glow.hide_render = g < 0.01; glow.scale = tuple(s * max(g, 1e-3) for s in gs); pd.energy = 4 * g
    for o in OB:
        if o.type == "MESH" and o is not fl: o.is_holdout = False
    fl.hide_render = sil
    bg.inputs["Color"].default_value = (0.9, 0.9, 0.88, 1) if sil else (0.10, 0.095, 0.13, 1)
    bg.inputs["Strength"].default_value = 1.0 if sil else 0.35
    t = Vector((0, 0, 0.22)); r = math.radians(az); d = 1.25
    cam.location = t + Vector((math.sin(r) * d, -math.cos(r) * d, elev * d)); cam.rotation_euler = (t - cam.location).to_track_quat("-Z", "Y").to_euler()
    if sil:
        for o in OB:
            if o.type == "MESH" and o is not fl:
                o["_m"] = 1
        sc.render.film_transparent = False
    p = os.path.join(os.path.dirname(OUTP), "_cp_" + name + ".png"); sc.render.filepath = p
    if sil:  # black silhouette: override materials
        ov = bpy.data.materials.new("blk"); ov.use_nodes = True
        ov.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0, 0, 0, 1)
        ov.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = 1
        sc.view_layers[0].material_override = ov; glow.hide_render = True
    else:
        sc.view_layers[0].material_override = None
    bpy.ops.render.render(write_still=True); return p
shots = [shot("closed", 0, 0, 24), shot("ajar", 22, 0.5, 24), shot("open", 105, 1, 24),
         shot("sil_front", 0, 0, 0, 0.12, True), shot("sil_side", 0, 0, 90, 0.12, True), shot("sil_34", 0, 0, 40, 0.3, True)]
from PIL import Image, ImageDraw
ims = [Image.open(p).convert("RGB") for p in shots]; w0, h0 = ims[0].size
sheet = Image.new("RGB", (w0 * 3, h0 * 2 + 30), (20, 19, 26)); dr = ImageDraw.Draw(sheet)
for i, (im, lab) in enumerate(zip(ims, ["dong nap", "he nap", "mo toang", "bong: truoc", "bong: canh", "bong: 3/4"])):
    sheet.paste(im, ((i % 3) * w0, (i // 3) * h0 + 30 * (i // 3 == 0) * 0 + 30)); dr.text(((i % 3) * w0 + 8, (i // 3) * h0 + 8 + 30 * (i // 3)), lab, fill=(230, 230, 230))
sheet.save(OUTP); [os.remove(p) for p in shots]; print("->", OUTP)

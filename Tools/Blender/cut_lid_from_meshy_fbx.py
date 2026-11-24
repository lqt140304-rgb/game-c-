"""Turn a closed Meshy chest (FBX + PBR maps) into a game-ready chest that BoxOpenSequence can open.

Result (GLB): root <name> -> Body, Lid (origin on the back hinge, rotates round X), Glow (light inside, hidden by scale)
and, when a keyhole position is given, Key (a silver key already in the "inserted" pose, shaft along local Y, bow outside).
Both cut faces get a dark-wood rim plus a bevelled burgundy lining pad (raised in Body, hung under the lid) instead of a bare filled cap.
There is no Seal or Binding layer: the lock and bands are baked into the Meshy texture (BoxOpenSequence skips missing layers).

Run:  python cut_lid_from_meshy_fbx.py -- <dir with fbx + png maps> <out.glb> <root name> <seam z in source units> <target height m> [tex size] ["x,y,z" keyhole in source units]
  Source FBX imports Z-up, centred on the origin. The seam is the height (source units) where the lid meets the body;
  find it with a vertex-height histogram or a side render. Front of the chest must face -Y (lock side); the hinge is at +Y.
Needs Blender (`pip install bpy` works) and Pillow.
"""
import sys, os, glob, tempfile, math
import bpy
import bmesh
import mathutils
from PIL import Image

src_dir, out, name, seam_src, target_h = sys.argv[sys.argv.index("--") + 1:][:5]
_a = sys.argv[sys.argv.index("--") + 1:] + ["1024", ""]
tex_size = int(_a[5]); keyhole_arg = _a[6]
seam_src = float(seam_src); target_h = float(target_h)

fbx = glob.glob(os.path.join(src_dir, "*.fbx"))[0]
stem = fbx[:-4]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=fbx)
obj = [o for o in bpy.context.scene.objects if o.type == "MESH"][0]
obj.parent = None
bpy.context.view_layer.objects.active = obj
obj.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

zs = [v.co.z for v in obj.data.vertices]
zmin, zmax = min(zs), max(zs)
scale = target_h / (zmax - zmin)
xs = [v.co.x for v in obj.data.vertices]; ys = [v.co.y for v in obj.data.vertices]
dx = -(min(xs) + max(xs)) / 2 * scale; dy = -(min(ys) + max(ys)) / 2 * scale
for v in obj.data.vertices:
    v.co = mathutils.Vector((v.co.x * scale + dx, v.co.y * scale + dy, (v.co.z - zmin) * scale))
seam = (seam_src - zmin) * scale
print("height", target_h, "seam z", seam)

# ---- materials -------------------------------------------------------------
tmp = tempfile.mkdtemp()
def small(path, mode=None):
    im = Image.open(path)
    if mode: im = im.convert(mode)
    im = im.resize((tex_size, tex_size), Image.LANCZOS)
    p = os.path.join(tmp, os.path.basename(path)); im.save(p); return p

def tex_node(nodes, path, noncolor):
    n = nodes.new("ShaderNodeTexImage"); n.image = bpy.data.images.load(path)
    if noncolor: n.image.colorspace_settings.name = "Non-Color"
    return n

mat = bpy.data.materials.new(name + "_Wood"); mat.use_nodes = True
nt = mat.node_tree; nodes = nt.nodes; links = nt.links
bsdf = nodes["Principled BSDF"]
base = tex_node(nodes, small(stem + ".png", "RGB"), False)
links.new(base.outputs["Color"], bsdf.inputs["Base Color"])
if os.path.exists(stem + "_normal.png"):
    nm = nodes.new("ShaderNodeNormalMap")
    links.new(tex_node(nodes, small(stem + "_normal.png", "RGB"), True).outputs["Color"], nm.inputs["Color"])
    links.new(nm.outputs["Normal"], bsdf.inputs["Normal"])
r_path, m_path = stem + "_roughness.png", stem + "_metallic.png"
if os.path.exists(r_path) and os.path.exists(m_path):
    r = Image.open(r_path).convert("L").resize((tex_size,) * 2, Image.LANCZOS)
    m = Image.open(m_path).convert("L").resize((tex_size,) * 2, Image.LANCZOS)
    mr = os.path.join(tmp, "mr.png")
    Image.merge("RGB", (Image.new("L", r.size, 255), r, m)).save(mr)   # glTF: G = roughness, B = metallic
    sep = nodes.new("ShaderNodeSeparateColor")
    links.new(tex_node(nodes, mr, True).outputs["Color"], sep.inputs["Color"])
    links.new(sep.outputs["Green"], bsdf.inputs["Roughness"])
    links.new(sep.outputs["Blue"], bsdf.inputs["Metallic"])
if os.path.exists(stem + "_emission.png"):
    links.new(tex_node(nodes, small(stem + "_emission.png", "RGB"), False).outputs["Color"], bsdf.inputs["Emission Color"])
    bsdf.inputs["Emission Strength"].default_value = 1.0

def flat_mat(mname, color, rough, metal=0.0):
    m = bpy.data.materials.new(mname); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = color; b.inputs["Roughness"].default_value = rough; b.inputs["Metallic"].default_value = metal
    m.use_backface_culling = False   # lining is seen from inside: glTF doubleSided
    return m
interior = flat_mat(name + "_Lining", (0.16, 0.018, 0.03, 1), 0.85)   # burgundy velvet lining
rim = flat_mat(name + "_Rim", (0.16, 0.085, 0.04, 1), 0.7)            # dark wood on the cut rim

glow_mat = bpy.data.materials.new(name + "_Glow"); glow_mat.use_nodes = True
gb = glow_mat.node_tree.nodes["Principled BSDF"]
gb.inputs["Base Color"].default_value = (1.0, 0.82, 0.45, 1)
gb.inputs["Emission Color"].default_value = (1.0, 0.82, 0.45, 1); gb.inputs["Emission Strength"].default_value = 3.0

obj.data.materials.clear(); obj.data.materials.append(mat); obj.data.materials.append(interior); obj.data.materials.append(rim)

# ---- cut ---------------------------------------------------------------------
def hollow(o, keep):
    """The filled cut face becomes a dark-wood rim; on top of it goes a bevelled burgundy lining pad (raised in Body, hung under the lid in Lid)."""
    cap = [p for p in o.data.polygons if all(abs(o.data.vertices[i].co.z - seam) < 1e-4 for i in p.vertices)]
    cv = [o.data.vertices[i].co for p in cap for i in p.vertices]
    x0, x1 = min(v.x for v in cv), max(v.x for v in cv); y0, y1 = min(v.y for v in cv), max(v.y for v in cv)
    for p in cap: p.material_index = 2
    sx, sy = (x1 - x0) * 0.80, (y1 - y0) * 0.72
    zc = seam + (0.011 if keep == "below" else -0.011)
    bpy.ops.mesh.primitive_cube_add(size=1, location=((x0 + x1) / 2, (y0 + y1) / 2, zc))
    pad = bpy.context.active_object; pad.scale = (sx, sy, 0.026)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bev = pad.modifiers.new("b", "BEVEL"); bev.width = 0.012; bev.segments = 2
    bpy.ops.object.modifier_apply(modifier="b")
    pad.data.materials.append(interior)
    for p in pad.data.polygons: p.use_smooth = True
    bpy.ops.object.select_all(action="DESELECT"); o.select_set(True); pad.select_set(True)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.join()

def bisect_copy(source, keep):
    dup = source.copy(); dup.data = source.data.copy()
    bpy.context.scene.collection.objects.link(dup)
    bpy.ops.object.select_all(action="DESELECT")
    bpy.context.view_layer.objects.active = dup; dup.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.bisect(plane_co=(0, 0, seam), plane_no=(0, 0, 1), clear_inner=(keep == "above"), clear_outer=(keep == "below"), use_fill=True)
    bpy.ops.object.mode_set(mode="OBJECT")
    hollow(dup, keep)
    return dup

body = bisect_copy(obj, "below"); body.name = "Body"
lid = bisect_copy(obj, "above"); lid.name = "Lid"
bpy.data.objects.remove(obj, do_unlink=True)

hinge = mathutils.Vector((0, max(v.co.y for v in lid.data.vertices), seam))   # back edge (front is -Y)
for v in lid.data.vertices:
    v.co -= hinge
lid.location = hinge

# ---- glow (hidden by scale until the lid cracks open) -------------------------
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=1.0, location=(0, 0, seam + 0.03))
glow = bpy.context.active_object; glow.name = "Glow"
w = (max(xs) - min(xs)) * scale; d = (max(ys) - min(ys)) * scale
glow.scale = (w * 0.30, d * 0.30, 0.02)
glow.data.materials.append(glow_mat)

# ---- key (already in the inserted pose; BoxOpenSequence pulls it back, brings it in and turns it) ----------
key = None
if keyhole_arg:
    kx, ky, kz = [float(v) for v in keyhole_arg.split(",")]
    kpos = mathutils.Vector((kx * scale + dx, ky * scale + dy, (kz - zmin) * scale))
    gold = flat_mat(name + "_KeySilver", (0.84, 0.88, 0.95, 1), 0.22, 1.0)   # silver, so it stands out against the gold lock
    parts = []
    def prim(op, **kw):
        op(**kw); o = bpy.context.active_object; o.data.materials.append(gold); parts.append(o); return o
    L = 0.062                                                         # key length from tip to bow
    prim(bpy.ops.mesh.primitive_cylinder_add, vertices=10, radius=0.0032, depth=L, location=(0, -L / 2 + 0.012, 0), rotation=(math.pi / 2, 0, 0))
    prim(bpy.ops.mesh.primitive_torus_add, major_radius=0.0125, minor_radius=0.0042, major_segments=16, minor_segments=8, location=(0, -L + 0.012 - 0.0125, 0), rotation=(math.pi / 2, 0, 0))
    for yy, ww in ((0.004, 0.011), (0.0095, 0.008)):                   # two teeth, sticking out sideways
        prim(bpy.ops.mesh.primitive_cube_add, size=1, location=(ww / 2, yy, -0.0012))
        bpy.context.active_object.scale = (ww, 0.0036, 0.0075)
    prim(bpy.ops.mesh.primitive_cylinder_add, vertices=10, radius=0.0052, depth=0.006, location=(0, -0.024, 0), rotation=(math.pi / 2, 0, 0))   # collar
    bpy.ops.object.select_all(action="DESELECT")
    for o in parts: o.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    key = bpy.context.active_object; key.name = "Key"
    key.scale = (1.45, 1.45, 1.45)   # a readable size next to the lock
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    key.location = kpos
    for pl in key.data.polygons: pl.use_smooth = True

root = bpy.data.objects.new(name, None); bpy.context.scene.collection.objects.link(root)
for o in [x for x in (body, lid, glow, key) if x]:
    o.parent = root
    for p in o.data.polygons: p.use_smooth = o is glow
bpy.ops.object.select_all(action="SELECT")
os.makedirs(os.path.dirname(out), exist_ok=True)
bpy.ops.export_scene.gltf(filepath=out, export_format="GLB", use_selection=True, export_apply=True)
print("tris body", sum(len(p.vertices) - 2 for p in body.data.polygons), "lid", sum(len(p.vertices) - 2 for p in lid.data.polygons), "->", out)

"""Turn a closed Meshy chest (FBX + PBR maps) into a game-ready chest that BoxOpenSequence can open.

Result (GLB): root <name> -> Body, Lid (origin on the back hinge, rotates round X), Glow (light inside, hidden by scale).
There is no Seal or Binding layer: the lock and bands are baked into the Meshy texture (BoxOpenSequence skips missing layers).

Run:  python cut_lid_from_meshy_fbx.py -- <dir with fbx + png maps> <out.glb> <root name> <seam z in source units> <target height m> [tex size]
  Source FBX imports Z-up, centred on the origin. The seam is the height (source units) where the lid meets the body;
  find it with a vertex-height histogram or a side render. Front of the chest must face -Y (lock side); the hinge is at +Y.
Needs Blender (`pip install bpy` works) and Pillow.
"""
import sys, os, glob, tempfile
import bpy
import mathutils
from PIL import Image

src_dir, out, name, seam_src, target_h = sys.argv[sys.argv.index("--") + 1:][:5]
tex_size = int((sys.argv[sys.argv.index("--") + 1:] + ["1024"])[5])
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

interior = bpy.data.materials.new(name + "_Interior"); interior.use_nodes = True
ib = interior.node_tree.nodes["Principled BSDF"]
ib.inputs["Base Color"].default_value = (0.06, 0.04, 0.03, 1); ib.inputs["Roughness"].default_value = 0.9

glow_mat = bpy.data.materials.new(name + "_Glow"); glow_mat.use_nodes = True
gb = glow_mat.node_tree.nodes["Principled BSDF"]
gb.inputs["Base Color"].default_value = (1.0, 0.82, 0.45, 1)
gb.inputs["Emission Color"].default_value = (1.0, 0.82, 0.45, 1); gb.inputs["Emission Strength"].default_value = 3.0

obj.data.materials.clear(); obj.data.materials.append(mat); obj.data.materials.append(interior)

# ---- cut ---------------------------------------------------------------------
def bisect_copy(source, keep):
    dup = source.copy(); dup.data = source.data.copy()
    bpy.context.scene.collection.objects.link(dup)
    bpy.ops.object.select_all(action="DESELECT")
    bpy.context.view_layer.objects.active = dup; dup.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.bisect(plane_co=(0, 0, seam), plane_no=(0, 0, 1), clear_inner=(keep == "above"), clear_outer=(keep == "below"), use_fill=True)
    bpy.ops.object.mode_set(mode="OBJECT")
    for p in dup.data.polygons:   # the filled cut faces get the dark interior material
        if all(abs(dup.data.vertices[i].co.z - seam) < 1e-4 for i in p.vertices):
            p.material_index = 1
    return dup

body = bisect_copy(obj, "below"); body.name = "Body"
lid = bisect_copy(obj, "above"); lid.name = "Lid"
bpy.data.objects.remove(obj, do_unlink=True)

hinge = mathutils.Vector((0, max(v.co.y for v in lid.data.vertices), seam))   # back edge (front is -Y)
for v in lid.data.vertices:
    v.co -= hinge
lid.location = hinge

# ---- glow (hidden by scale until the lid cracks open) -------------------------
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=1.0, location=(0, 0, seam + 0.004))
glow = bpy.context.active_object; glow.name = "Glow"
w = (max(xs) - min(xs)) * scale; d = (max(ys) - min(ys)) * scale
glow.scale = (w * 0.30, d * 0.30, 0.02)
glow.data.materials.append(glow_mat)

root = bpy.data.objects.new(name, None); bpy.context.scene.collection.objects.link(root)
for o in (body, lid, glow):
    o.parent = root
    for p in o.data.polygons: p.use_smooth = o is glow
bpy.ops.object.select_all(action="SELECT")
os.makedirs(os.path.dirname(out), exist_ok=True)
bpy.ops.export_scene.gltf(filepath=out, export_format="GLB", use_selection=True, export_apply=True)
print("tris body", sum(len(p.vertices) - 2 for p in body.data.polygons), "lid", sum(len(p.vertices) - 2 for p in lid.data.polygons), "->", out)

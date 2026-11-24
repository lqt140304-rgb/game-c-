"""Turn the Meshy "Mimic" chest (FBX, already OPEN, one mesh, 359k tris, 4K texture) into a game-ready chest
that BoxOpenSequence can drive.

Unlike cut_lid_from_meshy_fbx.py there is no seam plane to cut at (the lid is not lying closed), so:
  1. Split lid / body by topology: vertices above a height threshold that stay connected through the big lid shell
     are the lid; everything else is body; the thin contact strip in between is shared out by simultaneous BFS.
  2. Pivot = the round hinge boss on the lid side (found by hand from a side render: y,z in source units).
  3. Rotate the lid about X so the model starts CLOSED (the source pose is ~33 degrees open).
  4. Decimate both halves (Collapse), shrink the texture, add a red-orange Glow in the mouth, export GLB.
Result: root <name> -> Body, Lid (origin on the back hinge, rotates about X, front is -Y, opens with rotation_euler.x < 0), Glow.
No Key layer: a mimic has no lock to open.

Run:  python make_mimic_chest.py -- <dir with fbx+png> <out.glb> [name=Chest_Mimic] [target closed height m=0.42]
        [lid tris=7000] [body tris=7000] [tex size=2048] [close angle deg=33] [seam z=-0.2] [hinge "y,z"=0.625,-0.186]
Needs Blender (`pip install bpy` works) and Pillow.
"""
import sys, os, glob, math, tempfile
from collections import deque
import numpy as np
import bpy, bmesh
from mathutils import Vector
from PIL import Image

a = sys.argv[sys.argv.index("--") + 1:]
src_dir, out = a[0], a[1]
name = a[2] if len(a) > 2 else "Chest_Mimic"
target_h = float(a[3]) if len(a) > 3 else 0.42
lid_tris = int(a[4]) if len(a) > 4 else 7000
body_tris = int(a[5]) if len(a) > 5 else 7000
tex_size = int(a[6]) if len(a) > 6 else 2048
close_deg = float(a[7]) if len(a) > 7 else 33.0
seam_z = float(a[8]) if len(a) > 8 else -0.2
hy, hz = [float(v) for v in (a[9] if len(a) > 9 else "0.625,-0.186").split(",")]

fbx = glob.glob(os.path.join(src_dir, "*.fbx"))[0]
stem = fbx[:-4]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=fbx)
src = [o for o in bpy.context.scene.objects if o.type == "MESH"][0]
src.parent = None
bpy.context.view_layer.objects.active = src
src.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
me = src.data
n = len(me.vertices)
V = np.empty(n * 3, np.float32); me.vertices.foreach_get("co", V); V = V.reshape(-1, 3)
E = np.empty(len(me.edges) * 2, np.int32); me.edges.foreach_get("vertices", E); E = E.reshape(-1, 2)
adj = [[] for _ in range(n)]
for p, q in E:
    adj[p].append(q); adj[q].append(p)

# ---- 1. lid / body split ---------------------------------------------------------------------------
LID, BODY = 1, 2
lab = np.zeros(n, np.int8)
above = V[:, 2] > seam_z
comp = -np.ones(n, np.int32); sizes = []
for s in np.where(above)[0]:
    if comp[s] >= 0: continue
    cid = len(sizes); comp[s] = cid; cnt = 0; dq = deque([s])
    while dq:
        u = dq.popleft(); cnt += 1
        for w in adj[u]:
            if above[w] and comp[w] < 0: comp[w] = cid; dq.append(w)
    sizes.append(cnt)
big = int(np.argmax(sizes))
lab[comp == big] = LID                      # the lid shell (teeth of the lid included)
lab[above & (comp != big)] = BODY           # body fangs that poke above the seam height
lab[V[:, 2] < seam_z - 0.07] = BODY         # clearly below: body (the lid's lowest point is ~ -0.25)
dq = deque(np.where(lab > 0)[0])
while dq:                                    # share out the thin contact strip
    u = dq.popleft()
    for w in adj[u]:
        if lab[w] == 0: lab[w] = lab[u]; dq.append(w)
print("lid verts", int((lab == LID).sum()), "body verts", int((lab == BODY).sum()))

def extract(label, nm):
    o = src.copy(); o.data = src.data.copy(); o.name = nm
    bpy.context.scene.collection.objects.link(o)
    bm = bmesh.new(); bm.from_mesh(o.data); bm.verts.ensure_lookup_table()
    drop = [f for f in bm.faces if sum(1 for v in f.verts if lab[v.index] == label) < 2]
    bmesh.ops.delete(bm, geom=drop, context="FACES")
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context="VERTS")
    bm.to_mesh(o.data); bm.free()
    return o
lid = extract(LID, "Lid"); body = extract(BODY, "Body")
bpy.data.objects.remove(src, do_unlink=True)
print("tris before", len(lid.data.polygons), len(body.data.polygons))

# ---- 2. decimate ---------------------------------------------------------------------------------------
def decimate(o, target):
    bpy.ops.object.select_all(action="DESELECT"); o.select_set(True); bpy.context.view_layer.objects.active = o
    m = o.modifiers.new("d", "DECIMATE"); m.decimate_type = "COLLAPSE"
    m.ratio = min(1.0, target / len(o.data.polygons)); m.use_collapse_triangulate = True
    bpy.ops.object.modifier_apply(modifier="d")
    # drop shards that the collapse left floating
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.remove_doubles(threshold=1e-5); bpy.ops.object.mode_set(mode="OBJECT")
    print(o.name, "tris", len(o.data.polygons))
decimate(lid, lid_tris); decimate(body, body_tris)

# ---- 3. close the lid, scale, centre -------------------------------------------------------------------------
hinge = Vector((0, hy, hz))
th = math.radians(close_deg)
c, s_ = math.cos(th), math.sin(th)
for v in lid.data.vertices:                  # rotation about the X axis through the hinge: +angle = closing
    y, z = v.co.y - hy, v.co.z - hz
    v.co = Vector((v.co.x, hy + y * c - z * s_, hz + y * s_ + z * c))
allv = [v.co for o in (lid, body) for v in o.data.vertices]
zmin = min(v.z for v in allv); zmax = max(v.z for v in allv)
xs = [v.x for v in allv]; ys = [v.y for v in allv]
scale = target_h / (zmax - zmin)
dx = -(min(xs) + max(xs)) / 2 * scale; dy = -(min(ys) + max(ys)) / 2 * scale
for o in (lid, body):
    for v in o.data.vertices:
        v.co = Vector((v.co.x * scale + dx, v.co.y * scale + dy, (v.co.z - zmin) * scale))
hinge = Vector((0, hy * scale + dy, (hz - zmin) * scale))
for v in lid.data.vertices: v.co -= hinge   # lid origin on the hinge, so it turns about X
lid.location = hinge
print("closed size", (max(xs) - min(xs)) * scale, (max(ys) - min(ys)) * scale, target_h, "scale", scale, "hinge", tuple(hinge))

# ---- 4. material ---------------------------------------------------------------------------------------------
tmp = tempfile.mkdtemp()
im = Image.open(stem + ".png").convert("RGB").resize((tex_size, tex_size), Image.LANCZOS)
tp = os.path.join(tmp, name + "_Base.png"); im.save(tp)
mat = bpy.data.materials.new(name + "_Mat"); mat.use_nodes = True
bsdf = mat.node_tree.nodes["Principled BSDF"]
tn = mat.node_tree.nodes.new("ShaderNodeTexImage"); tn.image = bpy.data.images.load(tp)
mat.node_tree.links.new(tn.outputs["Color"], bsdf.inputs["Base Color"])
bsdf.inputs["Roughness"].default_value = 0.6
for o in (lid, body):
    o.data.materials.clear(); o.data.materials.append(mat)
    for p in o.data.polygons: p.use_smooth = True

# ---- 5. glow in the mouth (red-orange; hidden by scale until the lid cracks open) ----------------------------------
gm = bpy.data.materials.new(name + "_Glow"); gm.use_nodes = True
gb = gm.node_tree.nodes["Principled BSDF"]
gb.inputs["Base Color"].default_value = (1.0, 0.10, 0.015, 1)
gb.inputs["Emission Color"].default_value = (1.0, 0.10, 0.015, 1); gb.inputs["Emission Strength"].default_value = 4.0
w = (max(xs) - min(xs)) * scale; d = (max(ys) - min(ys)) * scale
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=1.0, location=(0, 0, (-0.50 - zmin) * scale))
glow = bpy.context.active_object; glow.name = "Glow"
glow.scale = (w * 0.30, d * 0.28, 0.022)
glow.data.materials.append(gm)

root = bpy.data.objects.new(name, None); bpy.context.scene.collection.objects.link(root)
for o in (body, lid, glow): o.parent = root
bpy.ops.object.select_all(action="SELECT")
os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
bpy.ops.export_scene.gltf(filepath=out, export_format="GLB", use_selection=True, export_apply=True,
                          export_image_format="JPEG", export_jpeg_quality=88)
print("tris body", len(body.data.polygons), "lid", len(lid.data.polygons), "->", out, os.path.getsize(out) // 1024, "KB")

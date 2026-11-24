"""Cut the lid off a Meshy box mesh: import GLB, bisect at the lid seam, split into Body and Lid (origin on the back hinge)."""
import sys, os
import bpy
import mathutils

src, out, seam_gltf_y, target_h = sys.argv[sys.argv.index("--") + 1:][:4]
seam_gltf_y = float(seam_gltf_y); target_h = float(target_h)

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src)
meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
obj = meshes[0]
for o in list(bpy.context.scene.objects):
    if o is not obj:
        o.select_set(False)
bpy.context.view_layer.objects.active = obj
obj.select_set(True)
obj.parent = None
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

zs = [v.co.z for v in obj.data.vertices]
zmin, zmax = min(zs), max(zs)
scale = target_h / (zmax - zmin)
for v in obj.data.vertices:
    v.co *= scale
xs = [v.co.x for v in obj.data.vertices]; ys = [v.co.y for v in obj.data.vertices]; zs = [v.co.z for v in obj.data.vertices]
dx = -(min(xs) + max(xs)) / 2; dy = -(min(ys) + max(ys)) / 2; dz = -min(zs)
for v in obj.data.vertices:
    v.co += mathutils.Vector((dx, dy, dz))
seam = (seam_gltf_y * 1.0 - (zmin)) * scale  # glTF y (up) became Blender z; shift like the vertices did
seam = (seam_gltf_y - zmin) * scale
print("height", target_h, "seam z", seam)

def bisect_copy(source, keep):
    dup = source.copy(); dup.data = source.data.copy()
    bpy.context.scene.collection.objects.link(dup)
    bpy.ops.object.select_all(action="DESELECT")
    bpy.context.view_layer.objects.active = dup
    dup.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.bisect(plane_co=(0, 0, seam), plane_no=(0, 0, 1), clear_inner=(keep == "above"), clear_outer=(keep == "below"), use_fill=True)
    bpy.ops.object.mode_set(mode="OBJECT")
    return dup

body = bisect_copy(obj, "below"); body.name = "Body"
lid = bisect_copy(obj, "above"); lid.name = "Lid"
bpy.data.objects.remove(obj, do_unlink=True)

ys = [v.co.y for v in lid.data.vertices]
hinge = mathutils.Vector((0, max(ys), seam))          # back edge of the seam (front is -Y)
for v in lid.data.vertices:
    v.co -= hinge
lid.location = hinge

root = bpy.data.objects.new("H1", None)
bpy.context.scene.collection.objects.link(root)
body.parent = root; lid.parent = root
for o in (body, lid):
    for p in o.data.polygons:
        p.use_smooth = False
bpy.ops.object.select_all(action="DESELECT")
root.select_set(True); body.select_set(True); lid.select_set(True)
bpy.context.view_layer.objects.active = root
bpy.ops.export_scene.gltf(filepath=out, export_format="GLB", use_selection=True, export_apply=True)
print("tris body", sum(len(p.vertices)-2 for p in body.data.polygons), "lid", sum(len(p.vertices)-2 for p in lid.data.polygons))

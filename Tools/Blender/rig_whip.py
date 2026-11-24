"""Rig a straight whip/rod mesh (a GLB whose long axis is X) with a chain of bones and animate it like a real whip.

Run: python rig_whip.py -- <in.glb> <out.glb> <handle_end: min|max> [bones=16] [seconds=2]
The handle end stays rigid; the further along the whip, the more each bone bends, so a wave travels from the handle
to the tip. Output: skinned mesh + armature + one looping clip "WhipWave" (glTF animation).
Vertex weights are computed along the axis (not by heat maps), so it works on any straight, thin mesh.
"""
import math, sys
import bpy
import mathutils

src, out, handle_end = sys.argv[sys.argv.index("--") + 1:][:3]
rest = sys.argv[sys.argv.index("--") + 4:]
N = int(rest[0]) if len(rest) > 0 else 16
SECONDS = float(rest[1]) if len(rest) > 1 else 2.0
FPS = 30
FRAMES = int(SECONDS * FPS)

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src)
mesh_obj = [o for o in bpy.context.scene.objects if o.type == "MESH"][0]
mesh_obj.parent = None
bpy.context.view_layer.objects.active = mesh_obj
mesh_obj.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
for o in list(bpy.context.scene.objects):
    if o is not mesh_obj and o.type != "MESH":
        bpy.data.objects.remove(o, do_unlink=True)

xs = [v.co.x for v in mesh_obj.data.vertices]
x0, x1 = (min(xs), max(xs)) if handle_end == "min" else (max(xs), min(xs))   # x0 = handle end, x1 = tip
cy = sum(v.co.y for v in mesh_obj.data.vertices) / len(mesh_obj.data.vertices)
cz = sum(v.co.z for v in mesh_obj.data.vertices) / len(mesh_obj.data.vertices)
length = x1 - x0

# ---- armature: N bones along the whip, handle to tip
bpy.ops.object.armature_add(location=(0, 0, 0))
arm_obj = bpy.context.active_object
arm_obj.name = "WhipArmature"
arm = arm_obj.data
bpy.ops.object.mode_set(mode="EDIT")
for b in list(arm.edit_bones):
    arm.edit_bones.remove(b)
bone_names = []
prev = None
for i in range(N):
    eb = arm.edit_bones.new(f"Seg{i:02d}")
    eb.head = (x0 + length * i / N, cy, cz)
    eb.tail = (x0 + length * (i + 1) / N, cy, cz)
    eb.roll = 0.0
    if prev is not None:
        eb.parent = prev
        eb.use_connect = True
    prev = eb
    bone_names.append(eb.name)
bpy.ops.object.mode_set(mode="OBJECT")

# ---- skin weights computed along the axis: each vertex blends the two nearest bones
for name in bone_names:
    mesh_obj.vertex_groups.new(name=name)
for v in mesh_obj.data.vertices:
    u = (v.co.x - x0) / length
    u = min(max(u, 0.0), 1.0) * N
    i = min(int(u), N - 1)
    f = u - i
    if i == N - 1:
        mesh_obj.vertex_groups[bone_names[i]].add([v.index], 1.0, "REPLACE")
    else:
        mesh_obj.vertex_groups[bone_names[i]].add([v.index], 1.0 - f, "REPLACE")
        mesh_obj.vertex_groups[bone_names[i + 1]].add([v.index], f, "REPLACE")
mod = mesh_obj.modifiers.new("Armature", "ARMATURE")
mod.object = arm_obj
mesh_obj.parent = arm_obj

# ---- animation: a travelling wave, rigid handle, growing amplitude toward the tip
bpy.context.view_layer.objects.active = arm_obj
bpy.ops.object.mode_set(mode="POSE")
action = bpy.data.actions.new("WhipWave")
arm_obj.animation_data_create()
arm_obj.animation_data.action = action
RIGID = max(2, N // 6)
for f in range(FRAMES + 1):
    phase = 2 * math.pi * f / FRAMES
    for i, name in enumerate(bone_names):
        pb = arm_obj.pose.bones[name]
        pb.rotation_mode = "XYZ"
        if i < RIGID:
            ax = az = 0.0
        else:
            k = (i - RIGID) / max(1, N - RIGID - 1)
            amp = 0.07 + 0.34 * (k ** 1.3)
            ax = amp * math.sin(phase - 0.5 * i) * 0.6
            az = amp * math.sin(phase - 0.5 * i + math.pi / 2)
        pb.rotation_euler = (ax, 0.0, az)
        pb.keyframe_insert("rotation_euler", frame=f + 1)
bpy.ops.object.mode_set(mode="OBJECT")
bpy.context.scene.frame_start, bpy.context.scene.frame_end = 1, FRAMES + 1
bpy.context.scene.render.fps = FPS

bpy.ops.object.select_all(action="DESELECT")
arm_obj.select_set(True)
mesh_obj.select_set(True)
bpy.context.view_layer.objects.active = arm_obj
bpy.ops.export_scene.gltf(filepath=out, export_format="GLB", use_selection=True, export_animations=True,
                          export_skins=True, export_apply=False)
print("rigged", N, "bones,", FRAMES + 1, "frames ->", out)

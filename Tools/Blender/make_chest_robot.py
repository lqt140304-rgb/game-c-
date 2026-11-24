"""Temporary Robot chest: a squat sitting wind-up tin robot, built procedurally with Blender (bpy).

Run: python make_chest_robot.py -- <output dir>
Exports Chest_Robot.glb: root Chest_Robot -> Body, Lid (the whole dome head, origin on the back hinge,
rotates around X, front is -Y), Glow (light in the mouth, hide with scale).
Bottom centre at origin, metres, Blender Z up (glTF export converts to Y up).
"""
import math, sys, os
import bpy, mathutils

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "."
os.makedirs(OUT, exist_ok=True)
R90 = math.radians(90)
PALETTE = {  # sRGB
    "tin": (0.55, 0.60, 0.62), "tin_dk": (0.34, 0.38, 0.41), "tin_lt": (0.70, 0.73, 0.74),
    "rubber": (0.16, 0.17, 0.19), "orange": (0.88, 0.45, 0.16), "brass": (0.66, 0.53, 0.24),
    "eye": (0.95, 0.90, 0.72), "pupil": (0.08, 0.08, 0.09), "interior": (0.06, 0.06, 0.07),
    "glow": (0.55, 0.85, 1.0),
}
_m = {}
def mat(n):
    if n in _m: return _m[n]
    m = bpy.data.materials.new(n); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]; c = tuple(x ** 2.2 for x in PALETTE[n])
    b.inputs["Base Color"].default_value = (*c, 1)
    b.inputs["Roughness"].default_value = 0.55 if n.startswith("tin") or n == "brass" else 0.85
    b.inputs["Metallic"].default_value = 0.6 if n.startswith("tin") or n == "brass" else 0.0
    if n in ("glow", "orange"):
        b.inputs["Emission Color"].default_value = (*c, 1)
        b.inputs["Emission Strength"].default_value = 2.0 if n == "glow" else 0.4
    _m[n] = m; return m

def finish(o, material):
    bpy.context.view_layer.objects.active = o; o.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    o.data.materials.clear(); o.data.materials.append(mat(material))
    for p in o.data.polygons: p.use_smooth = False
    o.select_set(False); return o

def cyl(r, d, loc, m, v=16, rot=(0, 0, 0), scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=v, radius=r, depth=d, location=loc, rotation=rot)
    o = bpy.context.active_object; o.scale = scale; return finish(o, m)

def cone(r1, r2, d, loc, m, v=16, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=v, radius1=r1, radius2=r2, depth=d, location=loc, rotation=rot)
    return finish(bpy.context.active_object, m)

def sphere(r, loc, m, seg=12, rings=6, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=r, location=loc)
    o = bpy.context.active_object; o.scale = scale; return finish(o, m)

def dome(r, h, z, m, seg=16, rings=10):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=r, location=(0, 0, 0))
    o = bpy.context.active_object
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.bisect(plane_co=(0, 0, 0), plane_no=(0, 0, 1), clear_inner=True, use_fill=True)
    bpy.ops.object.mode_set(mode="OBJECT")
    o.scale = (1, 1, h / r); o.location = (0, 0, z); return finish(o, m)

def box(size, loc, m, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    o = bpy.context.active_object; o.scale = size; return finish(o, m)

def torus(R, r, loc, m, rot=(0, 0, 0), seg=16):
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, major_segments=seg, minor_segments=4, location=loc, rotation=rot)
    return finish(bpy.context.active_object, m)

P = {"Body": [], "Lid": [], "Glow": []}
def add(layer, o): P[layer].append(o); return o

# ---- dimensions
RB = 0.15            # body radius
Z0, Z1 = 0.035, 0.27  # body bottom/top (rim)
# ---- body: barrel-bellied tin can
add("Body", cyl(RB * 0.92, 0.03, (0, 0, Z0 + 0.0), "tin_dk"))
add("Body", cone(RB * 0.95, RB, 0.06, (0, 0, Z0 + 0.045), "tin"))
add("Body", cyl(RB, 0.12, (0, 0, Z0 + 0.135), "tin"))
add("Body", cone(RB, RB * 0.96, 0.04, (0, 0, Z1 - 0.02), "tin"))
add("Body", torus(RB * 1.0, 0.009, (0, 0, Z0 + 0.075), "tin_dk"))      # rivet belt
add("Body", torus(RB * 0.96, 0.011, (0, 0, Z1), "tin_lt"))             # mouth rim
add("Body", cyl(RB * 0.90, 0.01, (0, 0, Z1 - 0.045), "interior", v=16))   # dark mouth floor
add("Body", cyl(RB * 0.93, 0.045, (0, 0, Z1 - 0.022), "interior", v=16, scale=(1, 1, 1)))
# chest panel: round dial + orange button + grille slots
add("Body", cyl(0.045, 0.012, (0, -RB + 0.002, Z0 + 0.15), "tin_lt", v=12, rot=(R90, 0, 0)))
add("Body", cyl(0.022, 0.012, (0, -RB - 0.006, Z0 + 0.15), "orange", v=10, rot=(R90, 0, 0)))
for i, x in enumerate((-0.03, 0, 0.03)):
    add("Body", box((0.016, 0.01, 0.006), (x, -RB * 0.985, Z0 + 0.085), "rubber"))
# arms: shoulder ball + stubby tube + pincer
for s in (-1, 1):
    add("Body", sphere(0.032, (s * (RB + 0.01), 0, Z0 + 0.18), "tin_dk", seg=10, rings=5))
    add("Body", cyl(0.022, 0.09, (s * (RB + 0.045), -0.03, Z0 + 0.135), "tin", v=8,
                   rot=(math.radians(25), s * math.radians(-35), 0)))
    add("Body", torus(0.022, 0.009, (s * (RB + 0.07), -0.065, Z0 + 0.10), "rubber",
                     rot=(R90 * 0.6, s * 0.4, 0), seg=8))
# feet sticking out to the front (sitting pose) + legs
for s in (-1, 1):
    add("Body", cyl(0.03, 0.07, (s * 0.07, -0.11, Z0 + 0.0), "tin_dk", v=8, rot=(R90, 0, 0)))
    add("Body", box((0.07, 0.09, 0.035), (s * 0.075, -0.165, 0.0175), "rubber"))
    add("Body", cyl(0.012, 0.03, (s * 0.075, -0.21, 0.02), "rubber", v=8, rot=(0, R90, 0)))
# wind-up key on the back
add("Body", cyl(0.012, 0.06, (0, RB + 0.03, Z0 + 0.14), "brass", v=8, rot=(R90, 0, 0)))
for s in (-1, 1):
    add("Body", cyl(0.04, 0.012, (s * 0.04, RB + 0.065, Z0 + 0.14), "brass", v=10, rot=(R90, 0, 0), scale=(1, 1, 0.75)))
# hinge knuckle on the back of the rim
add("Body", cyl(0.012, 0.07, (0, RB * 0.97, Z1 + 0.004), "tin_dk", v=8, rot=(0, R90, 0)))

# ---- lid: dome head with eyes and spring antenna
HZ = Z1 + 0.008
add("Lid", cyl(RB * 0.99, 0.03, (0, 0, HZ + 0.015), "tin_dk"))          # neck band
add("Lid", dome(RB * 0.97, 0.12, HZ + 0.03, "tin"))
add("Lid", cyl(0.012, 0.08, (0, RB * 0.97, HZ + 0.004), "tin_dk", v=8, rot=(0, R90, 0)))
for s in (-1, 1):  # big round eyes, tilted to look forward-up
    ex, ey, ez = s * 0.055, -0.11, HZ + 0.085
    add("Lid", cyl(0.04, 0.03, (ex, ey, ez), "tin_dk", v=12, rot=(math.radians(62), 0, 0)))
    add("Lid", cyl(0.031, 0.03, (ex, ey - 0.006, ez - 0.003), "eye", v=12, rot=(math.radians(62), 0, 0)))
    add("Lid", cyl(0.014, 0.03, (ex + s * -0.006, ey - 0.012, ez - 0.006), "pupil", v=8, rot=(math.radians(62), 0, 0)))
    add("Lid", cone(0.02, 0.012, 0.03, (s * (RB * 0.95), 0, HZ + 0.06), "orange", v=8, rot=(0, s * R90, 0)))  # ear bolts
add("Lid", cyl(0.008, 0.03, (0, 0.02, HZ + 0.16), "tin_dk", v=6))
for i in range(4):  # spring coils
    add("Lid", torus(0.013, 0.0035, (0, 0.02, HZ + 0.18 + i * 0.014), "tin_lt", seg=8))
add("Lid", sphere(0.022, (0, 0.02, HZ + 0.25), "orange", seg=10, rings=6))

# ---- glow: soft disc in the mouth
add("Glow", cyl(RB * 0.82, 0.004, (0, 0, Z1 - 0.012), "glow", v=16))
add("Glow", cone(RB * 0.8, RB * 0.35, 0.06, (0, 0, Z1 + 0.02), "glow", v=12))

def join(objs):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]; bpy.ops.object.join(); return objs[0]

def set_origin(o, p):
    p = mathutils.Vector(p)
    for v in o.data.vertices: v.co -= p
    o.location = p

root = bpy.data.objects.new("Chest_Robot", None); bpy.context.scene.collection.objects.link(root)
tris = {}
for layer, objs in P.items():
    o = join(objs); o.name = layer
    if layer == "Lid": set_origin(o, (0, RB * 0.97, HZ + 0.004))
    if layer == "Glow": set_origin(o, (0, 0, Z1))
    o.parent = root
    tris[layer] = sum(len(p.vertices) - 2 for p in o.data.polygons)
bpy.ops.object.select_all(action="DESELECT"); root.select_set(True)
for c in root.children: c.select_set(True)
path = os.path.join(OUT, "Chest_Robot.glb")
bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True, export_apply=True)
print("TRIS", tris, "total(no glow)", tris["Body"] + tris["Lid"], "->", path)

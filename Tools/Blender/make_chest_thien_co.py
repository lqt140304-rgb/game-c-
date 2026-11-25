"""Thien Co chest (temporary test version): a split mechanical rocket-pod (sphere hull with a nose-cone lid, three fins), built with bpy.

Run:   python make_chest_thien_co.py -- <out.glb> [<preview.png> <samples> <width>]
Layers: Chest_ThienCo (root) -> Body, Lid (origin on the back hinge, rotates about X, front is -Y), Glow (cyan halo
at the mouth, hide by scale), Seal (front lock dial). Bottom centre at the origin, metres, Blender Z up.
With a preview path, also renders a Cycles sheet: closed / ajar / open + 3 silhouette angles.
"""
import math, os, sys
import bpy, bmesh, mathutils
from mathutils import Vector

ARGS = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
OUT = ARGS[0] if ARGS else "Chest_ThienCo.glb"
PREVIEW = ARGS[1] if len(ARGS) > 1 else None
SAMPLES = int(ARGS[2]) if len(ARGS) > 2 else 48
WIDTH = int(ARGS[3]) if len(ARGS) > 3 else 640
R90 = math.radians(90)

PALETTE = {  # sRGB
    "shell": (0.62, 0.62, 0.60), "shell_dk": (0.36, 0.37, 0.39), "iron": (0.20, 0.21, 0.23),
    "brass": (0.66, 0.55, 0.30), "interior": (0.06, 0.07, 0.08), "cyan": (0.25, 0.95, 1.0), "glow": (0.45, 1.0, 1.0),
}
EMIT = {"cyan": 3.0, "glow": 2.0}
METAL = {"shell": 0.5, "shell_dk": 0.6, "iron": 0.6, "brass": 0.8}
_mats = {}


def mat(name):
    if name not in _mats:
        m = bpy.data.materials.new(name)
        m.use_nodes = True
        b = m.node_tree.nodes["Principled BSDF"]
        c = tuple(x ** 2.2 for x in PALETTE[name])
        b.inputs["Base Color"].default_value = (*c, 1)
        b.inputs["Roughness"].default_value = 0.55
        b.inputs["Metallic"].default_value = METAL.get(name, 0.0)
        if name in EMIT:
            b.inputs["Emission Color"].default_value = (*c, 1)
            b.inputs["Emission Strength"].default_value = EMIT[name]
        _mats[name] = m
    return _mats[name]


def finish(o, material):
    bpy.context.view_layer.objects.active = o
    o.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    o.data.materials.clear()
    o.data.materials.append(mat(material))
    for p in o.data.polygons:
        p.use_smooth = False
    o.select_set(False)
    return o


def box(size, loc, m, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = size
    return finish(o, m)


def cyl(r, d, loc, m, verts=8, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=d, location=loc, rotation=rot)
    return finish(bpy.context.active_object, m)


def cone(r1, r2, d, loc, m, verts=8, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=d, location=loc, rotation=rot)
    return finish(bpy.context.active_object, m)


def hemi(r, cz, m, upper, seg=16, rings=8, scale_z=1.0):
    """Closed half of a UV sphere centred at height cz (upper or lower half), flat cap at the cut."""
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=r, location=(0, 0, cz))
    o = bpy.context.active_object
    o.scale = (1, 1, scale_z)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bm = bmesh.new()
    bm.from_mesh(o.data)
    geom = bm.verts[:] + bm.edges[:] + bm.faces[:]
    res = bmesh.ops.bisect_plane(bm, geom=geom, plane_co=(0, 0, cz), plane_no=(0, 0, 1),
                                 clear_inner=upper, clear_outer=not upper)
    cut = [e for e in res["geom_cut"] if isinstance(e, bmesh.types.BMEdge)]
    bmesh.ops.edgeloop_fill(bm, edges=cut)
    bm.to_mesh(o.data)
    bm.free()
    return finish(o, m)


def ring(r_out, r_in, h, cz, m, verts=24):
    """Flat washer (equator band)."""
    bm = bmesh.new()
    bmesh.ops.create_circle(bm, cap_ends=False, radius=r_out, segments=verts)
    outer = bm.verts[:]
    bmesh.ops.create_circle(bm, cap_ends=False, radius=r_in, segments=verts)
    inner = bm.verts[len(outer):]
    for i in range(verts):
        j = (i + 1) % verts
        bm.faces.new((outer[i], outer[j], inner[j], inner[i]))
    bmesh.ops.solidify(bm, geom=bm.faces[:], thickness=h)
    bmesh.ops.translate(bm, verts=bm.verts[:], vec=(0, 0, cz + h / 2))
    me = bpy.data.meshes.new("ring")
    bm.to_mesh(me)
    bm.free()
    o = bpy.data.objects.new("ring", me)
    bpy.context.scene.collection.objects.link(o)
    return finish(o, m)


def gear(r, h, loc, m, teeth=10, rot=(0, 0, 0), tooth=0.012):
    parts = [cyl(r, h, (0, 0, 0), m, verts=teeth * 2)]
    for i in range(teeth):
        a = 2 * math.pi * i / teeth
        parts.append(box((tooth, tooth * 1.1, h), (math.cos(a) * (r + tooth * 0.4), math.sin(a) * (r + tooth * 0.4), 0), m,
                         rot=(0, 0, a)))
    o = join(parts)
    o.rotation_euler = rot
    o.location = loc
    return finish(o, m)


def join(objs):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    if len(objs) > 1:
        bpy.ops.object.join()
    o = bpy.context.active_object
    o.select_set(False)
    return o


# ----------------------------------------------------------------------------------------------- build
R, CZ, NZ = 0.14, 0.21, 1.8            # radius, equator height, nose stretch of the lid -> rocket ~0.48 m
TOP = CZ + R * NZ
HINGE = Vector((0, R + 0.012, CZ))
L = {"Body": [], "Lid": [], "Glow": [], "Seal": []}
add = lambda layer, o: L[layer].append(o)


def build():
    # stand: octagonal foot, three claws hugging the lower half
    add("Body", cone(0.07, 0.10, 0.08, (0, 0, 0.06), "shell_dk", verts=8))      # engine nozzle under the hull
    add("Body", cyl(0.05, 0.012, (0, 0, 0.026), "cyan", verts=8))                   # nozzle glow ring
    for k in range(3):                                                             # three swept rocket fins
        a = math.radians(90 + 120 * k)
        d = Vector((math.cos(a), math.sin(a), 0))
        fin = box((0.13, 0.016, 0.21), d * (R * 0.95) + Vector((0, 0, 0.105)), "iron", rot=(0, 0, a))
        for v in fin.data.vertices:                                                # sweep: top outer edge slides in
            r = Vector((v.co.x, v.co.y, 0)).dot(d)
            if v.co.z > 0.15 and r > R * 0.95:
                v.co -= d * 0.09
        add("Body", fin)
        add("Body", box((0.03, 0.024, 0.024), d * (R * 0.95 + 0.065) + Vector((0, 0, 0.012)), "brass", rot=(0, 0, a)))
    # lower hemisphere + equator gear band
    add("Body", hemi(R, CZ, "shell", upper=False))
    add("Body", hemi(R - 0.012, CZ + 0.001, "interior", upper=False))   # dark cut face reads as cavity
    add("Body", ring(R + 0.012, R - 0.012, 0.02, CZ - 0.02, "shell_dk", verts=24))
    for i in range(24):                                                 # gear teeth on the band
        a = 2 * math.pi * i / 24
        add("Body", box((0.012, 0.014, 0.016), (math.cos(a) * (R + 0.017), math.sin(a) * (R + 0.017), CZ - 0.01), "brass",
                         rot=(0, 0, a)))
    # little energy core inside (TC3 echo)
    add("Body", cyl(0.035, 0.05, (0, 0, CZ + 0.02), "iron", verts=8))
    add("Body", cyl(0.022, 0.06, (0, 0, CZ + 0.03), "cyan", verts=8))
    # rear hinge knuckles on the body
    for sx in (-1, 1):
        add("Body", cyl(0.014, 0.03, (sx * 0.045, HINGE.y, HINGE.z), "iron", verts=8, rot=(0, R90, 0)))

    # lid: upper hemisphere shell with gear plates and a cyan eye lens on the front
    add("Lid", hemi(R, CZ + 0.002, "shell", upper=True, scale_z=NZ))
    add("Lid", hemi(R - 0.012, CZ + 0.001, "interior", upper=True, scale_z=NZ))   # dark inside face of the lid
    add("Lid", ring(R + 0.012, R - 0.012, 0.016, CZ + 0.002, "shell_dk", verts=24))
    # latitude band (bánh răng dẹt ôm vòm)
    lat = math.radians(48)
    add("Lid", ring(R * math.cos(lat) + 0.008, R * math.cos(lat) - 0.01, 0.014, CZ + R * NZ * math.sin(lat) - 0.007, "iron", verts=16))
    # top dome gear + finial
    add("Lid", cyl(0.018, 0.03, (0, 0, TOP - 0.004), "iron", verts=8))
    add("Lid", cone(0.012, 0.002, 0.05, (0, 0, TOP + 0.034), "shell_dk", verts=6))
    add("Lid", cyl(0.008, 0.014, (0, 0, TOP + 0.012), "cyan", verts=8))
    # side gear on the right flank
    add("Lid", gear(0.04, 0.014, (R * 0.93, 0, CZ + 0.055), "brass", teeth=8, rot=(0, R90, 0), tooth=0.01))
    # the eye: tilted bezel + iris on the front
    el = math.radians(28)
    c = Vector((0, -R * math.cos(el), CZ + R * NZ * math.sin(el)))
    n = Vector((0, -math.cos(el) / R, math.sin(el) / (R * NZ))).normalized()   # ellipsoid normal
    c += n * 0.004
    erot = n.to_track_quat("Z", "Y").to_euler()
    add("Lid", cyl(0.055, 0.024, c, "iron", verts=12, rot=erot))
    add("Lid", cyl(0.042, 0.01, c + n * 0.014, "shell_dk", verts=12, rot=erot))
    add("Lid", cyl(0.028, 0.01, c + n * 0.018, "cyan", verts=12, rot=erot))
    add("Lid", cyl(0.011, 0.01, c + n * 0.022, "interior", verts=8, rot=erot))
    # lid hinge knuckle
    add("Lid", cyl(0.014, 0.05, (0, HINGE.y, HINGE.z), "iron", verts=8, rot=(0, R90, 0)))

    # glow: flat cyan halo at the mouth
    add("Glow", cyl(R - 0.02, 0.01, (0, 0, CZ + 0.012), "glow", verts=16))
    # seal: front lock dial on the equator
    add("Seal", cyl(0.032, 0.016, (0, -R - 0.024, CZ - 0.002), "brass", verts=8, rot=(R90, 0, 0)))
    add("Seal", cyl(0.018, 0.012, (0, -R - 0.034, CZ - 0.002), "cyan", verts=8, rot=(R90, 0, 0)))
    add("Seal", box((0.006, 0.01, 0.026), (0, -R - 0.041, CZ - 0.002), "iron"))


def set_origin(o, p):
    off = o.matrix_world.inverted() @ Vector(p)
    o.data.transform(mathutils.Matrix.Translation(-off))
    o.location = Vector(p)


bpy.ops.wm.read_factory_settings(use_empty=True)
build()
root = bpy.data.objects.new("Chest_ThienCo", None)
bpy.context.scene.collection.objects.link(root)
objs, tris = {}, {}
for layer, parts in L.items():
    o = join(parts)
    o.name = layer
    if layer == "Lid":
        set_origin(o, HINGE)
    elif layer in ("Glow", "Seal"):
        bb = [o.matrix_world @ Vector(c) for c in o.bound_box]
        set_origin(o, sum(bb, Vector()) / 8)
    o.parent = root
    objs[layer] = o
    tris[layer] = sum(len(p.vertices) - 2 for p in o.data.polygons)
zmax = max((o.matrix_world @ v.co).z for o in objs.values() for v in o.data.vertices)
print("triangles:", tris, "total:", sum(tris.values()), "height m: %.3f" % zmax)
bpy.ops.object.select_all(action="DESELECT")
root.select_set(True)
for o in objs.values():
    o.select_set(True)
os.makedirs(os.path.dirname(os.path.abspath(OUT)), exist_ok=True)
bpy.ops.export_scene.gltf(filepath=OUT, export_format="GLB", use_selection=True, export_apply=True)
print("->", OUT)

# ----------------------------------------------------------------------------------------------- preview
if PREVIEW:
    from PIL import Image, ImageDraw
    sc = bpy.context.scene
    sc.render.engine = "CYCLES"
    sc.cycles.device = "CPU"
    sc.cycles.samples = SAMPLES
    sc.cycles.use_denoising = True
    sc.render.resolution_x, sc.render.resolution_y = WIDTH, int(WIDTH * 0.75)
    w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True
    bg = w.node_tree.nodes["Background"]
    bpy.ops.mesh.primitive_plane_add(size=6)
    fl = bpy.context.active_object
    fm = bpy.data.materials.new("f"); fm.use_nodes = True
    fm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.07, 0.065, 0.085, 1)
    fl.data.materials.append(fm)
    lights = []
    for loc, e, col in (((-0.8, -1.0, 1.4), 140, (1, .93, .85)), ((1.0, 0.6, 1.0), 40, (.7, .8, 1)), ((0, 0.9, 1.2), 30, (1, 1, 1))):
        d = bpy.data.lights.new("a", "AREA"); d.energy = e; d.size = 1.2; d.color = col
        lo = bpy.data.objects.new("a", d); sc.collection.objects.link(lo); lo.location = loc
        lo.rotation_euler = (Vector((0, 0, 0.2)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
        lights.append(lo)
    pd = bpy.data.lights.new("in", "POINT"); pd.color = (0.4, 1, 1); pd.shadow_soft_size = 0.03
    pl = bpy.data.objects.new("in", pd); sc.collection.objects.link(pl); pl.location = (0, 0, CZ + 0.06)
    cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera = cam
    cam.data.lens = 50
    lid, glow = objs["Lid"], objs["Glow"]
    g0 = tuple(glow.scale)

    def shot(path, lid_deg, glw, az, el=0.42, dist=1.35, silhouette=False):
        lid.rotation_euler = (math.radians(-lid_deg), 0, 0)
        glow.scale = tuple(s * max(1e-4, glw) for s in g0); glow.hide_render = glw < 0.01
        pd.energy = 4 * glw
        a = math.radians(az)
        tgt = Vector((0, 0, 0.22))
        cam.location = tgt + Vector((math.sin(a) * dist, -math.cos(a) * dist, el * dist))
        cam.rotation_euler = (tgt - cam.location).to_track_quat("-Z", "Y").to_euler()
        fl.hide_render = silhouette
        for lo in lights:
            lo.hide_render = silhouette
        sc.render.film_transparent = silhouette
        bg.inputs["Color"].default_value = (0.10, 0.095, 0.13, 1); bg.inputs["Strength"].default_value = 0.35
        sc.render.filepath = path
        bpy.ops.render.render(write_still=True)

    tmp = os.path.join(os.path.dirname(os.path.abspath(PREVIEW)), "_tc_tmp")
    os.makedirs(tmp, exist_ok=True)
    plan = [("Đóng nắp", 0, 0, 28, False), ("Hé nắp", 28, 0.5, 28, False), ("Mở toang", 105, 1, 28, False),
            ("Bóng: trước", 0, 0, 0, True), ("Bóng: cạnh", 0, 0, 90, True), ("Bóng: 3/4 sau", 0, 0, 145, True)]
    tiles = []
    for i, (label, ld, gl, az, sil) in enumerate(plan):
        p = os.path.join(tmp, f"{i}.png")
        shot(p, ld, gl, az, el=0.15 if sil else 0.42, silhouette=sil)
        im = Image.open(p).convert("RGBA")
        if sil:   # flatten to a pure black silhouette on light grey
            a = im.split()[3].point(lambda v: 255 if v > 40 else 0)
            im = Image.new("RGBA", im.size, (225, 225, 220, 255))
            im.paste((20, 20, 24, 255), mask=a)
        from PIL import ImageFont
        try:
            fnt = ImageFont.truetype("DejaVuSans.ttf", max(14, WIDTH // 32))
        except OSError:
            fnt = ImageFont.load_default()
        ImageDraw.Draw(im).text((10, 8), label, font=fnt, fill=(255, 255, 255, 255) if not sil else (0, 0, 0, 255))
        tiles.append(im)
    W, H = tiles[0].size
    sheet = Image.new("RGB", (W * 3, H * 2))
    for i, t in enumerate(tiles):
        sheet.paste(t.convert("RGB"), ((i % 3) * W, (i // 3) * H))
    sheet.save(PREVIEW)
    for f in os.listdir(tmp):
        os.remove(os.path.join(tmp, f))
    os.rmdir(tmp)
    print("preview ->", PREVIEW)

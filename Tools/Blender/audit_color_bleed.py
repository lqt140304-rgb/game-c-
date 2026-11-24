"""Kiem tra loang mau va render can canh cho mo hinh LowPoly Meshy (Docs/Art/KiemTra3D.md, muc "Loang mau").

Chay (tung file mot):
    python3 Tools/Blender/audit_color_bleed.py audit  <file.glb> [ket_qua.json]
    python3 Tools/Blender/audit_color_bleed.py render <file.glb> <anh.png> [--mark]
    python3 Tools/Blender/audit_color_bleed.py compare <truoc.glb> <sau.glb> <anh.jpg>

audit: voi moi tam giac lay 7 mau texture trong vung UV cua no (Lab), roi bao
  - outlier  : mat lech dE76 > 20 voi MOI mat ke tren luoi (vet mau la, mat tram lo sai mau);
  - stretch  : tam giac UV gian > 25 lan trung vi, phu > 40 texel (UV vat qua khe giua cac dao);
  - bleed_mipN: so mat ma o muc mipmap N (1/2^N) mau loc bi lech dE > 10 so voi mau chi lay tu
    texel thuoc dao UV cua mon nay -> GPU dang hut mau cua dao ben canh hoac nen atlas.
render: 5 goc (truoc, phai, sau, tren cheo, duoi cheo), camera truc giao om sat vat, chieu sang deu
  (mau texture x he so huong mat, khong bong), Cycles CPU. --mark to mau hong cac mat outlier.
compare: render ca hai ban, ghep hang tren = truoc, hang duoi = sau; o cuoi moi hang la anh phong to
  2,5 lan vung khac nhau nhieu nhat giua hai ban (de soi can).
"""
import bpy, sys, os, json, math
import numpy as np
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from color_bleed_lib import mesh_arrays, image_array, analyze

VIEWS = [(0, 12), (90, 12), (180, 12), (35, 55), (215, -40)]  # (phuong vi, do cao) do


def load(path):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=path)
    objs = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    return objs


def texture_of(o):
    return next(n.image for n in o.active_material.node_tree.nodes if n.type == "TEX_IMAGE")


def audit(path, out=None):
    objs = load(path)
    res = {"file": path.split("Items/LowPoly/")[-1]}
    for o in objs:
        P, T, UV = mesh_arrays(o)
        r = analyze(P, T, UV, image_array(texture_of(o)))
        res.update({k: v for k, v in r.items() if not k.startswith("_")})
    print("AUDIT", json.dumps({k: v for k, v in res.items() if not k.endswith("_idx")}), flush=True)
    if out:
        json.dump(res, open(out, "w"))
    return res


def render(path, out, mark=False, size=300, tiles_out=None, views=None):
    objs = load(path)
    sc = bpy.context.scene
    flagged = {}
    if mark:
        for o in objs:
            P, T, UV = mesh_arrays(o)
            flagged[o.name] = analyze(P, T, UV, image_array(texture_of(o)), levels=())["outlier_idx"]
    for o in objs:
        img = texture_of(o)
        m = bpy.data.materials.new("flat"); m.use_nodes = True
        nt = m.node_tree; nt.nodes.clear()
        it = nt.nodes.new("ShaderNodeTexImage"); it.image = img
        lw = nt.nodes.new("ShaderNodeLayerWeight"); lw.inputs["Blend"].default_value = 0.5
        mr = nt.nodes.new("ShaderNodeMath"); mr.operation = "MULTIPLY_ADD"
        mr.inputs[1].default_value = -0.45; mr.inputs[2].default_value = 1.0
        mix = nt.nodes.new("ShaderNodeMix"); mix.data_type = "RGBA"; mix.blend_type = "MULTIPLY"
        mix.inputs["Factor"].default_value = 1.0
        em = nt.nodes.new("ShaderNodeEmission"); outn = nt.nodes.new("ShaderNodeOutputMaterial")
        nt.links.new(lw.outputs["Facing"], mr.inputs[0])
        nt.links.new(it.outputs["Color"], mix.inputs["A"])
        nt.links.new(mr.outputs[0], mix.inputs["B"])
        nt.links.new(mix.outputs["Result"], em.inputs["Color"])
        nt.links.new(em.outputs[0], outn.inputs[0])
        o.data.materials.clear(); o.data.materials.append(m)
        if flagged.get(o.name):
            mk = bpy.data.materials.new("mark"); mk.use_nodes = True
            mk.node_tree.nodes.clear()
            e = mk.node_tree.nodes.new("ShaderNodeEmission"); e.inputs["Color"].default_value = (1, 0, 1, 1)
            oo = mk.node_tree.nodes.new("ShaderNodeOutputMaterial")
            mk.node_tree.links.new(e.outputs[0], oo.inputs[0])
            o.data.materials.append(mk)
            o.data.calc_loop_triangles()
            for f in flagged[o.name]:
                o.data.polygons[o.data.loop_triangles[f].polygon_index].material_index = 1

    pts = [o.matrix_world @ Vector(c) for o in objs for c in o.bound_box]
    mn = Vector([min(p[i] for p in pts) for i in range(3)]); mx = Vector([max(p[i] for p in pts) for i in range(3)])
    ctr = (mn + mx) / 2; rad = max((mx - mn).length / 2, 1e-4)
    world = bpy.data.worlds.new("w"); sc.world = world
    world.color = (0.18, 0.18, 0.2)
    cam_d = bpy.data.cameras.new("cam"); cam_d.type = "ORTHO"
    cam_d.ortho_scale = 2.1 * rad; cam_d.clip_start = rad * 0.01; cam_d.clip_end = rad * 50
    cam = bpy.data.objects.new("cam", cam_d); sc.collection.objects.link(cam); sc.camera = cam
    sc.render.engine = "CYCLES"; sc.cycles.device = "CPU"; sc.cycles.samples = 24
    sc.cycles.max_bounces = 0; sc.cycles.use_denoising = False
    sc.view_settings.view_transform = "Standard"
    sc.render.resolution_x = sc.render.resolution_y = size
    sc.render.image_settings.file_format = "PNG"
    from PIL import Image
    tiles = []
    tmp = out + ".tmp.png"
    for az, el in (views or VIEWS):
        a, e = math.radians(az), math.radians(el)
        d = Vector((math.sin(a) * math.cos(e), -math.cos(a) * math.cos(e), math.sin(e)))
        cam.location = ctr + d * rad * 10
        cam.rotation_euler = (-d).to_track_quat("-Z", "Y").to_euler()
        sc.render.filepath = tmp
        bpy.ops.render.render(write_still=True)
        tiles.append(Image.open(tmp).convert("RGB"))
    os.remove(tmp)
    if tiles_out is not None:
        tiles_out.extend(tiles)
        return
    strip = Image.new("RGB", (size * len(tiles), size))
    for i, t in enumerate(tiles):
        strip.paste(t, (i * size, 0))
    strip.save(out)
    print("RENDER", out, flush=True)


def compare(before, after, out, size=600, tile=300):
    from PIL import Image, ImageChops, ImageDraw
    tb, ta = [], []
    render(before, out, size=size, tiles_out=tb)
    render(after, out, size=size, tiles_out=ta)
    # vung khac nhau nhieu nhat (cua so 240 px) tren tat ca cac goc
    best = (-1, 0, 0, 0); c = 240
    for i, (b, a) in enumerate(zip(tb, ta)):
        d = np.asarray(ImageChops.difference(b, a).convert("L"), np.float32)
        cs = np.pad(d, ((1, 0), (1, 0))).cumsum(0).cumsum(1)
        win = cs[c:, c:] - cs[:-c, c:] - cs[c:, :-c] + cs[:-c, :-c]
        y, x = np.unravel_index(np.argmax(win[::8, ::8]), win[::8, ::8].shape)
        if win[y * 8, x * 8] > best[0]:
            best = (win[y * 8, x * 8], i, x * 8, y * 8)
    _, vi, x, y = best
    n = len(tb) + 1
    sheet = Image.new("RGB", (tile * n, tile * 2), (40, 40, 44))
    for r, row in enumerate((tb, ta)):
        for i, t in enumerate(row):
            im = t.resize((tile, tile), Image.LANCZOS)
            if i == vi:
                ImageDraw.Draw(im).rectangle([x * tile // size, y * tile // size, (x + c) * tile // size,
                                              (y + c) * tile // size], outline=(255, 220, 0))
            sheet.paste(im, (i * tile, r * tile))
        sheet.paste(row[vi].crop((x, y, x + c, y + c)).resize((tile, tile), Image.LANCZOS), ((n - 1) * tile, r * tile))
    dr = ImageDraw.Draw(sheet)
    dr.text((6, 4), "TRUOC", fill=(255, 255, 255)); dr.text((6, tile + 4), "SAU", fill=(255, 255, 255))
    dr.text(((n - 1) * tile + 6, 4), "phong to", fill=(255, 220, 0))
    sheet.save(out, quality=85)
    print("COMPARE", out, flush=True)


if __name__ == "__main__":
    mode, a = sys.argv[1], sys.argv[2:]
    if mode == "audit":
        audit(a[0], a[1] if len(a) > 1 else None)
    elif mode == "compare":
        compare(a[0], a[1], a[2])
    else:
        render(a[0], a[1], mark="--mark" in a)

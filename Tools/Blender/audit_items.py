"""Kiem tra hinh hoc va mau sac cua moi mo hinh .glb.

Chay: python3 Tools/Blender/audit_items.py <thu_muc_glb> <thu_muc_ket_qua>
(can goi bpy: pip install bpy). Ghi report.json va anh render <ten>.png.
"""
import bpy, bmesh, sys, os, json, math, colorsys
import numpy as np
from mathutils import Vector

src, out = sys.argv[-2], sys.argv[-1]
os.makedirs(out, exist_ok=True)


def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def tex_stats(img):
    if img is None or img.size[0] == 0:
        return None
    w, h = img.size
    px = np.array(img.pixels[:], dtype=np.float32).reshape(-1, 4)
    step = max(1, len(px) // 200000)
    px = px[::step]
    rgb = px[:, :3]
    mean = rgb.mean(0)
    hsv = np.array([colorsys.rgb_to_hsv(*c) for c in rgb[:: max(1, len(rgb) // 5000)]])
    return {
        "size": [w, h],
        "mean_rgb": [round(float(x), 3) for x in mean],
        "mean_sat": round(float(hsv[:, 1].mean()), 3),
        "mean_val": round(float(hsv[:, 2].mean()), 3),
        "pct_black": round(float((rgb.max(1) < 0.02).mean()) * 100, 1),
        "pct_magenta": round(float(((rgb[:, 0] > .9) & (rgb[:, 1] < .1) & (rgb[:, 2] > .9)).mean()) * 100, 1),
        "has_alpha": bool((px[:, 3] < 0.99).any()),
    }


def audit(path):
    reset()
    bpy.ops.import_scene.gltf(filepath=path)
    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    r = {"file": path.split("Art/Items/")[-1], "mb": round(os.path.getsize(path) / 1e6, 2),
         "objects": len(meshes), "tris": 0, "verts": 0, "non_manifold_edges": 0,
         "boundary_edges": 0, "degenerate_faces": 0, "loose_verts": 0,
         "no_uv_objects": 0, "inverted_normals_pct": 0.0, "materials": {}, "issues": []}
    mins, maxs = Vector((1e9,) * 3), Vector((-1e9,) * 3)
    dg = bpy.context.evaluated_depsgraph_get()
    inward, total_area = 0.0, 0.0
    for o in meshes:
        me = o.evaluated_get(dg).to_mesh()
        bm = bmesh.new(); bm.from_mesh(me); bm.transform(o.matrix_world)
        bmesh.ops.triangulate(bm, faces=bm.faces[:])
        bmesh.ops.remove_doubles(bm, verts=bm.verts[:], dist=1e-5)  # glTF tach dinh o seam UV
        r["tris"] += len(bm.faces); r["verts"] += len(bm.verts)
        for e in bm.edges:
            ll = e.link_loops
            if len(ll) == 2 and ll[0].vert == ll[1].vert:
                r["flipped_edges"] = r.get("flipped_edges", 0) + 1
            if not e.is_manifold:
                r["non_manifold_edges"] += 1
                if e.is_boundary:
                    r["boundary_edges"] += 1
        r["loose_verts"] += sum(1 for v in bm.verts if not v.link_edges)
        c = sum((v.co for v in bm.verts), Vector()) / max(1, len(bm.verts))
        for f in bm.faces:
            a = f.calc_area()
            if a < 1e-10:
                r["degenerate_faces"] += 1
                continue
            total_area += a
            if f.normal.dot(f.calc_center_median() - c) < 0:
                inward += a
        for v in bm.verts:
            for i in range(3):
                mins[i] = min(mins[i], v.co[i]); maxs[i] = max(maxs[i], v.co[i])
        if not me.uv_layers:
            r["no_uv_objects"] += 1
        bm.free(); o.evaluated_get(dg).to_mesh_clear()
        for slot in o.material_slots:
            m = slot.material
            if not m or m.name in r["materials"]:
                continue
            info = {"base_color": None, "metallic": None, "roughness": None, "textures": {}}
            if m.use_nodes:
                for n in m.node_tree.nodes:
                    if n.type == "BSDF_PRINCIPLED":
                        bc = n.inputs["Base Color"]
                        info["base_color"] = [round(x, 3) for x in bc.default_value[:3]]
                        info["metallic"] = round(n.inputs["Metallic"].default_value, 2)
                        info["roughness"] = round(n.inputs["Roughness"].default_value, 2)
                        for sock in ("Base Color", "Normal", "Metallic", "Roughness", "Emission Color"):
                            s = n.inputs.get(sock)
                            if s and s.is_linked:
                                stack = [s.links[0].from_node]
                                while stack:
                                    x = stack.pop()
                                    if x.type == "TEX_IMAGE":
                                        info["textures"][sock] = tex_stats(x.image) if sock == "Base Color" else {"size": list(x.image.size)}
                                        break
                                    stack += [l.from_node for i in x.inputs for l in i.links]
            r["materials"][m.name] = info
    dims = maxs - mins
    r["dims_m"] = [round(d, 3) for d in dims]
    r["inverted_normals_pct"] = round(inward / max(total_area, 1e-9) * 100, 1)

    # --- danh gia ---
    iss = r["issues"]
    if not meshes: iss.append("Khong co mesh")
    if r["tris"] > 20000: iss.append(f"Qua nhieu tam giac cho low-poly ({r['tris']})")
    if r["mb"] > 10: iss.append(f"File nang {r['mb']} MB")
    if max(dims) > 3 or max(dims) < 0.02: iss.append(f"Ty le la: {r['dims_m']} m")
    if r["boundary_edges"] > 0: iss.append(f"Mesh ho: {r['boundary_edges']} canh bien")
    if r["non_manifold_edges"] - r["boundary_edges"] > 0:
        iss.append(f"Canh non-manifold: {r['non_manifold_edges'] - r['boundary_edges']}")
    if r["degenerate_faces"]: iss.append(f"Mat suy bien: {r['degenerate_faces']}")
    if r["loose_verts"]: iss.append(f"Dinh roi: {r['loose_verts']}")
    if r["no_uv_objects"] and any(i["textures"] for i in r["materials"].values()):
        iss.append("Co texture nhung thieu UV")
    if r.get("flipped_edges"): iss.append(f"Phap tuyen khong nhat quan: {r['flipped_edges']} canh")
    if r["inverted_normals_pct"] > 60: iss.append(f"Phap tuyen co the lat ra trong ({r['inverted_normals_pct']}%)")
    if not r["materials"]: iss.append("Khong co vat lieu")
    for name, i in r["materials"].items():
        t = i["textures"].get("Base Color")
        if t:
            if max(t["size"]) > 2048: iss.append(f"{name}: texture {t['size']} qua lon")
            if t["pct_magenta"] > 1: iss.append(f"{name}: texture bi thieu (magenta)")
            if t["pct_black"] > 30: iss.append(f"{name}: {t['pct_black']}% texture den")
            if t["mean_val"] < 0.12: iss.append(f"{name}: texture qua toi")
        elif i["base_color"] and max(i["base_color"]) < 0.03:
            iss.append(f"{name}: mau nen gan den, khong texture")
    render(meshes, dims, mins, os.path.join(out, os.path.splitext(os.path.basename(path))[0] + "__" + os.path.basename(os.path.dirname(path)) + ".png"))
    r["render"] = os.path.basename(path)
    return r


def render(meshes, dims, mins, fn):
    sc = bpy.context.scene
    center = mins + dims / 2
    size = max(dims) or 1
    cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam)
    cam.location = center + Vector((1.2, -1.6, 0.9)).normalized() * size * 2.4
    cam.rotation_euler = (center - cam.location).to_track_quat("-Z", "Y").to_euler()
    sc.camera = cam
    for loc, e in (((3, -3, 5), 3), ((-4, 2, 2), 1.5)):
        l = bpy.data.objects.new("sun", bpy.data.lights.new("sun", "SUN")); l.data.energy = e
        l.rotation_euler = (Vector((0, 0, 0)) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
        sc.collection.objects.link(l)
    w = bpy.data.worlds.new("w"); w.use_nodes = True
    w.node_tree.nodes["Background"].inputs[0].default_value = (0.6, 0.6, 0.62, 1)
    w.node_tree.nodes["Background"].inputs[1].default_value = 0.6
    sc.world = w
    sc.render.engine = "CYCLES"; sc.cycles.samples = 12; sc.cycles.device = "CPU"
    sc.cycles.use_denoising = False
    sc.render.resolution_x = sc.render.resolution_y = 320
    sc.render.filepath = fn
    bpy.ops.render.render(write_still=True)


files = sorted(os.path.join(d, f) for d, _, fs in os.walk(src) for f in fs if f.endswith(".glb"))
res = []
for f in files:
    try:
        res.append(audit(f))
    except Exception as e:
        res.append({"file": os.path.relpath(f), "issues": [f"Loi khi nap: {e}"]})
    print("OK", f, res[-1].get("tris"), res[-1]["issues"], flush=True)
json.dump(res, open(os.path.join(out, "report.json"), "w"), ensure_ascii=False, indent=1)

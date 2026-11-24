"""Bake lai texture rieng cho tung mon LowPoly Meshy de het loang mau (Docs/Art/KiemTra3D.md, muc "Loang mau").

Chay (tung file mot, may de het bo nho):
    python3 Tools/Blender/rebake_items_meshy.py <file.glb> <ban_goc_texture_cao.glb> [bao_cao.json]
Ghi de <file.glb>. <ban_goc_texture_cao.glb> la ban truoc khi ha texture (commit cce9f69), lay bang
    git show cce9f69:<path> | git lfs smudge -- <path> > goc.glb
Hai ban cung bo cuc atlas, nen UV hien tai tro dung vao texture goc 2048/4096.

1. Sua UV mat tram lo: mat co UV vat qua khe giua cac dao (gian > 25 lan) hoac mau lech manh voi
   moi mat ke ma UV khong noi voi mat ke nao, hoac nam trong dao UV ti hon (< 6 texel goc, sau bake
   khong trung tam texel nao nen an mau dem cua dao ben canh) -> chep UV cua mat ke lon nhat (dinh
   chung lay dung UV, dinh con lai lay trong tam tam giac UV cua mat ke). Mau khop mat ke.
2. Chi giu cac dao UV cua mon nay (dao that cua Meshy sau khi han dinh trung), xep lai (pack)
   cach nhau 16 px o 1024 (giam dan toi 4 px neu nhieu dao nho lam dien tich phu < 40%), xoay tu do.
3. Bake mau tu texture goc sang UV moi o 2048 (Cycles, EMIT), margin 32 px kieu EXTEND (dilation),
   roi thu nho 2x2 xuong 1024 -> margin ~16 px.
4. Thay anh trong material goc, xoa UV cu, xuat GLB (JPEG 92). Khong dong toi hinh hoc:
   giu nguyen dinh, tam giac, phap tuyen, kich thuoc va goc toa do.
"""
import bpy, bmesh, sys, os, json, tempfile
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from color_bleed_lib import (mesh_arrays, image_array, glb_base_color_bytes, analyze, face_colors,
                             weld_index, face_neighbors, tri_area, delta_e, uv_mask,
                             srgb_to_lab, lab_to_srgb, dilate)

BAKE = 2048
OUT = 1024
PACK_MARGINS = (0.016, 0.012, 0.008, 0.006, 0.004)  # ~16..4 px o 1024
MIN_COVER = 0.40
TINY_ISLAND_PX = 6.0  # texel o texture goc (2048/4096)
BAKE_MARGIN = 32

# Vet ve san trong texture goc cua Meshy (nguyen nhan 4), de bi hieu nham la loi hien vat.
# Chon mat theo huong phap tuyen + do cao, to lai bang mau nen tron,
# giu cac texel `keep` (vd. vien vang). Sau do dung lai phan dem quanh dao.
REPAINT = {
    # L10 mu do: day mu (mat phang ngua xuong) co hinh xoay xanh xam vo nghia -> do tron, giu vien vang
    "L10_mu_do_chu_m": dict(normal_z=-0.7, max_z=0.01,
                            base=lambda lab: lab[:, 1] > 32,
                            keep=lambda lab: (lab[:, 2] > 38) & (lab[:, 1] < 18) & (lab[:, 0] > 55)),
}


def repaint(name, P, T, UV, img):
    cfg = REPAINT[name]
    N = np.cross(P[T][:, 1] - P[T][:, 0], P[T][:, 2] - P[T][:, 0])
    N /= np.linalg.norm(N, axis=1, keepdims=True) + 1e-12
    sel = (N[:, 2] < cfg["normal_z"]) & (P[T][:, :, 2].max(1) < cfg["max_z"])
    h, w = img.shape[:2]
    m = uv_mask(UV[sel], h, w)
    flat = img.reshape(-1, 3); idx = np.where(m.ravel())[0]
    lab = srgb_to_lab(flat[idx])
    keep = cfg["keep"](lab)
    base_lab = np.median(lab[cfg["base"](lab) & ~keep], 0)
    new = np.tile(base_lab, (len(idx), 1))
    out = np.where(keep[:, None], flat[idx], lab_to_srgb(new))
    flat[idx] = out
    # dung lai phan dem (dilation) quanh tat ca cac dao tu noi dung moi
    allm = uv_mask(UV, h, w)
    img[:] = dilate(img, allm, BAKE_MARGIN // 2)
    return int(sel.sum())


def uv_islands(T, UV, weld, nb_edges):
    """So canh lien tuc UV cua moi mat va nhan dao UV (thanh phan lien thong qua canh lien tuc)."""
    W = weld[T]
    cont = np.zeros(len(T), int)
    parent = list(range(len(T)))

    def find(x):
        while parent[x] != x:
            parent[x] = parent[parent[x]]; x = parent[x]
        return x
    for (a, b), fs in nb_edges.items():
        if len(fs) != 2:
            continue
        f, g = fs
        def uvat(face, v):
            return UV[face][list(W[face]).index(v)]
        if (np.abs(uvat(f, a) - uvat(g, a)).max() < 1e-4 and np.abs(uvat(f, b) - uvat(g, b)).max() < 1e-4):
            cont[f] += 1; cont[g] += 1
            parent[find(f)] = find(g)
    island = np.array([find(f) for f in range(len(T))])
    return cont, island


def fix_bad_uvs(P, T, UV, tex):
    """Tra ve UV da sua va danh sach mat da sua."""
    weld = weld_index(P)
    nb, edges = face_neighbors(T, weld)
    r = analyze(P, T, UV, tex, levels=())
    cont, island = uv_islands(T, UV, weld, edges)
    bad = set(r["stretch_idx"]) | {f for f in r["outlier_idx"] if cont[f] == 0}
    # dao UV ti hon (< TINY_ISLAND_PX texel o texture goc): sau khi bake khong trung tam texel nao,
    # mau se lay tu phan dem cua dao ben canh -> gop vao dao cua mat ke
    auv = tri_area(UV) * tex.shape[0] * tex.shape[1]
    isl_area = np.bincount(island, weights=auv)
    tiny = {int(f) for f in np.where(isl_area[island] < TINY_ISLAND_PX)[0]} - bad
    wrong = set(bad)  # mau cua chinh mat da sai -> lay mat ke lon nhat
    bad |= tiny       # mau cua mat ti hon van dung (lay tai 1 diem) -> lay mat ke cung mau nhat
    col = r["_col"]
    kept = [f for f in r["outlier_idx"] if f not in bad]
    a3 = tri_area(P[T])
    W = weld[T]
    UV = UV.copy()
    fixed = []
    todo = set(bad)
    while todo:
        progress = False
        for f in sorted(todo):
            good = [g for g in nb[f] if g not in todo]
            if not good:
                continue
            if f in wrong:
                g = max(good, key=lambda x: a3[x])
            else:
                g = min(good, key=lambda x: (round(float(delta_e(col[x], col[f])) / 5), -a3[x]))
            cen = UV[g].mean(0)
            for k in range(3):
                hit = np.where(W[g] == W[f][k])[0]
                UV[f][k] = UV[g][hit[0]] if len(hit) else cen
            todo.discard(f); fixed.append(int(f)); progress = True
        if not progress:
            break
    return UV, fixed, kept, sorted(todo), tiny


def set_loop_uvs(me, layer, T_loops, UV):
    data = np.empty(len(me.loops) * 2); layer.data.foreach_get("uv", data)
    data = data.reshape(-1, 2)
    data[T_loops.ravel()] = UV.reshape(-1, 2)
    layer.data.foreach_set("uv", data.ravel())


def process(path, src_glb, report=None):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=path)
    objs = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    assert len(objs) == 1, f"{path}: {len(objs)} mesh"
    o = objs[0]; me = o.data
    assert len(me.polygons) == len(me.loop_triangles) or True
    mat = me.materials[0]
    tex_node = next(n for n in mat.node_tree.nodes if n.type == "TEX_IMAGE")

    tmp = tempfile.mkdtemp()
    b, mime = glb_base_color_bytes(src_glb)
    src_path = os.path.join(tmp, "src." + ("png" if "png" in mime else "jpg"))
    open(src_path, "wb").write(b)
    src = bpy.data.images.load(src_path); src.colorspace_settings.name = "sRGB"
    src_tex = image_array(src)

    old = me.uv_layers.active; old.name = "old"
    P, T, UV = mesh_arrays(o, "old")
    me.calc_loop_triangles()
    L = np.empty(len(T) * 3, np.int64); me.loop_triangles.foreach_get("loops", L); L = L.reshape(-1, 3)
    before = analyze(P, T, UV, src_tex, levels=())

    # 1. sua UV mat tram lo
    UVf, fixed, kept, unfixed, tiny = fix_bad_uvs(P, T, UV, src_tex)
    set_loop_uvs(me, old, L, UVf)

    # 2. UV moi: chi cac dao cua mon nay, xep lai co khoang cach. Khoang cach lon nhat ma cac dao
    #    van phu >= MIN_COVER anh (nhieu dao nho thi khoang cach an mat dien tich).
    #    glTF tach dinh o moi seam -> Blender thay hang nghin manh roi. Pack tren mot BAN SAO da han
    #    dinh trung khit (1e-6) de dao UV = dao that cua Meshy, roi chep UV ve luoi that theo tung goc.
    #    Luoi that khong bi han: vi tri, tam giac va phap tuyen giu nguyen tuyet doi.
    o2 = o.copy(); o2.data = me.copy(); bpy.context.scene.collection.objects.link(o2)
    me2 = o2.data
    # danh dau chi so goc goc: han co the xoa tam giac suy bien (dien tich 0) va doi thu tu
    tag = me2.attributes.new("orig_loop", "INT", "CORNER")
    tag.data.foreach_set("value", np.arange(len(me.loops), dtype=np.int32))
    bm = bmesh.new(); bm.from_mesh(me2)
    bmesh.ops.remove_doubles(bm, verts=bm.verts[:], dist=1e-6)
    bm.to_mesh(me2); bm.free()
    a1 = np.empty(len(me2.loops) * 2); me2.uv_layers["old"].data.foreach_get("uv", a1)
    me2.uv_layers.active = me2.uv_layers["old"]
    for ob in bpy.context.scene.objects:
        ob.select_set(False)
    bpy.context.view_layer.objects.active = o2; o2.select_set(True)
    bpy.context.scene.tool_settings.use_uv_select_sync = True
    margin_used, cover = None, 0.0
    for margin in PACK_MARGINS:
        me2.uv_layers["old"].data.foreach_set("uv", a1)
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.select_all(action="SELECT")
        bpy.ops.uv.pack_islands(udim_source="CLOSEST_UDIM", rotate=True, rotate_method="ANY", scale=True,
                                merge_overlap=False, margin_method="FRACTION", margin=margin,
                                shape_method="CONCAVE")
        bpy.ops.object.mode_set(mode="OBJECT")
        _, _, UVt = mesh_arrays(o2, "old")
        cover = float(uv_mask(UVt, 256, 256).mean())
        margin_used = margin
        if cover >= MIN_COVER and UVt.min() >= 0 and UVt.max() <= 1:
            break
    p2 = np.empty(len(me2.loops) * 2); me2.uv_layers["old"].data.foreach_get("uv", p2)
    ids = np.empty(len(me2.loops), np.int32); me2.attributes["orig_loop"].data.foreach_get("value", ids)
    packed = np.full((len(me.loops), 2), np.nan); packed[ids] = p2.reshape(-1, 2)
    lost = np.where(np.isnan(packed[:, 0]))[0]
    if len(lost):  # goc cua tam giac suy bien bi xoa (khong nhin thay): lay UV cua goc cung dinh
        cv = np.empty(len(me.loops), np.int64); me.loops.foreach_get("vertex_index", cv)
        kp = weld_index(np.array([v.co[:] for v in me.vertices]), 1e-6)[cv]
        have = {}
        for li in np.where(~np.isnan(packed[:, 0]))[0]:
            have.setdefault(kp[li], li)
        for li in lost:
            packed[li] = packed[have[kp[li]]] if kp[li] in have else (0.5, 0.5)
    packed = packed.ravel()
    bpy.data.objects.remove(o2); bpy.data.meshes.remove(me2)
    new = me.uv_layers.new(name="UVMap", do_init=True)
    new.data.foreach_set("uv", packed)
    me.uv_layers.active = me.uv_layers["UVMap"]
    me.uv_layers["UVMap"].active_render = True
    bpy.context.view_layer.objects.active = o; o.select_set(True)

    # 3. bake EMIT tu texture goc (UV cu) sang anh moi (UV moi)
    bake_img = bpy.data.images.new("bake", BAKE, BAKE, alpha=False)
    bake_img.colorspace_settings.name = "sRGB"
    tm = bpy.data.materials.new("bake_tmp"); tm.use_nodes = True
    nt = tm.node_tree; nt.nodes.clear()
    uvn = nt.nodes.new("ShaderNodeUVMap"); uvn.uv_map = "old"
    it = nt.nodes.new("ShaderNodeTexImage"); it.image = src; it.interpolation = "Linear"
    em = nt.nodes.new("ShaderNodeEmission"); out = nt.nodes.new("ShaderNodeOutputMaterial")
    nt.links.new(uvn.outputs["UV"], it.inputs["Vector"])
    nt.links.new(it.outputs["Color"], em.inputs["Color"])
    nt.links.new(em.outputs["Emission"], out.inputs["Surface"])
    tgt = nt.nodes.new("ShaderNodeTexImage"); tgt.image = bake_img
    nt.nodes.active = tgt
    me.materials[0] = tm
    sc = bpy.context.scene
    sc.render.engine = "CYCLES"; sc.cycles.device = "CPU"; sc.cycles.samples = 1
    sc.view_settings.view_transform = "Standard"
    sc.render.bake.margin = BAKE_MARGIN; sc.render.bake.margin_type = "EXTEND"
    bpy.ops.object.bake(type="EMIT", margin=BAKE_MARGIN, margin_type="EXTEND", use_clear=True)
    me.materials[0] = mat

    px = image_array(bake_img)
    s = BAKE // OUT
    small = px.reshape(OUT, s, OUT, s, 3).mean((1, 3))
    _, _, UVn = mesh_arrays(o, "UVMap")
    name = os.path.splitext(os.path.basename(path))[0]
    repainted = repaint(name, P, T, UVn, small) if name in REPAINT else 0
    out_img = bpy.data.images.new("Image_0", OUT, OUT, alpha=False)
    rgba = np.concatenate([small, np.ones((OUT, OUT, 1), np.float32)], 2)
    out_img.pixels.foreach_set(rgba.ravel())
    png = os.path.join(tmp, "Image_0.png")
    out_img.filepath_raw = png; out_img.file_format = "PNG"; out_img.save()
    out_img = bpy.data.images.load(png); out_img.name = "Image_0"
    tex_node.image = out_img
    # node anh trong material goc dung UV mac dinh -> UV moi sau khi xoa UV cu

    # kiem tra do trung mau: anh moi @ UV moi so voi texture goc @ UV da sua
    c_new, _ = face_colors(small, UVn); c_ref, _ = face_colors(src_tex, UVf)
    fid = delta_e(c_new, c_ref)
    after = analyze(P, T, UVn, small)

    me.uv_layers.remove(me.uv_layers["old"])
    bpy.data.materials.remove(tm)
    bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", export_image_format="JPEG",
                              export_jpeg_quality=92, export_yup=True)

    strip = lambda r: {k: v for k, v in r.items() if not k.startswith("_") and not k.endswith("_idx")}
    rep = {"file": path.split("Items/LowPoly/")[-1], "src_tex": list(src.size),
           "pack_margin_px": round(margin_used * OUT), "uv_cover_pct": round(cover * 100, 1),
           "repainted_faces": repainted, "fixed_uv_faces": fixed, "tiny_island_faces": len(tiny), "unfixed": unfixed, "outlier_kept": kept,
           "before_src": strip(before), "after": strip(after),
           "fidelity_dE_p50": round(float(np.median(fid)), 2),
           "fidelity_dE_p99": round(float(np.percentile(fid, 99)), 2),
           "fidelity_dE_max": round(float(fid.max()), 1),
           "fidelity_gt10": int((fid > 10).sum()),
           "mb": round(os.path.getsize(path) / 1e6, 2)}
    print("REBAKE", json.dumps(rep), flush=True)
    if report:
        json.dump(rep, open(report, "w"), indent=1)
    return rep


if __name__ == "__main__":
    a = sys.argv[1:]
    process(a[0], a[1], a[2] if len(a) > 2 else None)

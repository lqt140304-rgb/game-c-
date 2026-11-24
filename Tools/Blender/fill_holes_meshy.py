"""Han khe nut va tram lo cho mo hinh LowPoly Meshy (xem Docs/Art/KiemTra3D.md).

Chay: python3 Tools/Blender/fill_holes_meshy.py <file.glb> [...]   (ghi de file)
Khong dung cho Boxes_Meshy: mieng hop ho la chu y (than va nap tach roi).

1. Han dinh trung (1e-5) va han cac dinh BIEN gan nhau (<= 0,4% kich thuoc vat, toi da 1,5 mm).
2. Tram moi vong bien kin con lai, tam giac hoa dep.
3. Mat moi lay UV tu dinh lan can, nen mau khop xung quanh.
4. Tinh lai phap tuyen huong ra ngoai, lam min theo goc 35 do.
"""
import bpy, bmesh, sys, os, math
from mathutils import Vector


def boundary_count(bm):
    return sum(1 for e in bm.edges if e.is_boundary)


def process(path):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=path)
    out = {}
    for o in [o for o in bpy.context.scene.objects if o.type == "MESH"]:
        me = o.data
        bpy.context.view_layer.objects.active = o; o.select_set(True)
        if getattr(me, "has_custom_normals", False):
            bpy.ops.mesh.customdata_custom_splitnormals_clear()
        size = max(o.dimensions) or 1.0
        bm = bmesh.new(); bm.from_mesh(me)
        uv = bm.loops.layers.uv.active
        bmesh.ops.remove_doubles(bm, verts=bm.verts[:], dist=1e-5)
        before = boundary_count(bm)

        # 1. han khe: chi dinh bien
        tol = min(0.004 * size / o.scale.x if o.scale.x else 0.004 * size, 0.0015 / max(o.scale.x, 1e-9))
        bverts = list({v for e in bm.edges if e.is_boundary for v in e.verts})
        bmesh.ops.remove_doubles(bm, verts=bverts, dist=tol)
        # xoa mat suy bien do han sinh ra
        bmesh.ops.dissolve_degenerate(bm, dist=1e-7, edges=bm.edges[:])
        # 1b. khau T-junction: dinh bien nam sat giua mot canh bien khac
        ttol = min(0.012 * size, 0.002 / max(o.scale.x, 1e-9))
        for _ in range(4):
            be = [e for e in bm.edges if e.is_boundary]
            bv = {v for e in be for v in e.verts}
            pairs = []
            for v in bv:
                best = None
                for e in be:
                    if v in e.verts or not e.is_valid:
                        continue
                    a, b = e.verts[0].co, e.verts[1].co
                    ab = b - a
                    if ab.length_squared == 0:
                        continue
                    t = (v.co - a).dot(ab) / ab.length_squared
                    if t <= 0.02 or t >= 0.98:
                        continue  # gan dau mut: buoc han dinh da lo
                    d = (a + ab * t - v.co).length
                    if d < ttol and (best is None or d < best[0]):
                        best = (d, e, t)
                if best:
                    pairs.append((v, best[1], best[2]))
            done = set()
            n = 0
            for v, e, t in pairs:
                if e in done or not e.is_valid or not v.is_valid:
                    continue
                done.add(e)
                ne, nv = bmesh.utils.edge_split(e, e.verts[0], t)
                bmesh.ops.pointmerge(bm, verts=[nv, v], merge_co=v.co.copy())
                n += 1
            bmesh.ops.remove_doubles(bm, verts=bm.verts[:], dist=1e-6)
            if not n:
                break
        bmesh.ops.dissolve_degenerate(bm, dist=1e-7, edges=bm.edges[:])

        # 1c. xoa "vay": dao <= 4 mat, < 0,2% dien tich, bam vao canh co > 2 mat
        total = sum(f.calc_area() for f in bm.faces) or 1.0
        seen, fins = set(), []
        for f in bm.faces:
            if f in seen:
                continue
            comp, st = [], [f]; seen.add(f)
            while st and len(comp) <= 6:
                x = st.pop(); comp.append(x)
                for e in x.edges:
                    if len(e.link_faces) == 2:
                        for g in e.link_faces:
                            if g not in seen:
                                seen.add(g); st.append(g)
            if st:
                continue
            touches_nm = any(len(e.link_faces) > 2 for x in comp for e in x.edges)
            if touches_nm and len(comp) <= 4 and sum(x.calc_area() for x in comp) < 0.002 * total:
                fins += comp
        removed_fins = len(fins)
        if fins:
            bmesh.ops.delete(bm, geom=fins, context="FACES_ONLY")
            bmesh.ops.delete(bm, geom=[e for e in bm.edges if not e.link_faces], context="EDGES")
            bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_edges], context="VERTS")
        after_weld = boundary_count(bm)

        # 2. tram vong kin
        old_faces = set(bm.faces)
        res = bmesh.ops.holes_fill(bm, edges=[e for e in bm.edges if e.is_boundary], sides=0)
        new_faces = res["faces"]
        if new_faces:
            tri = bmesh.ops.triangulate(bm, faces=new_faces, quad_method="BEAUTY", ngon_method="BEAUTY")
            new_faces = tri["faces"]
            # 3. UV cho mat moi: lay UV trung binh cua dinh o cac mat cu
            if uv:
                for f in new_faces:
                    for l in f.loops:
                        src = [x[uv].uv for x in l.vert.link_loops if x.face in old_faces]
                        if src:
                            l[uv].uv = sum(src, Vector((0, 0))) / len(src)
            bmesh.ops.beautify_fill(bm, faces=new_faces, edges=list({e for f in new_faces for e in f.edges}))
        # 4. phap tuyen: giu nguyen mat goc, chi lat mat moi cho cung chieu voi mat ke
        #    (tinh lai toan bo luoi co the doan sai huong ca mot vung, vd. X02)
        pending = set(f for f in new_faces if f.is_valid)
        for _ in range(len(pending) + 1):
            changed = False
            for f in list(pending):
                for l in f.loops:
                    e = l.edge
                    nb = [x for x in e.link_loops if x.face is not f and x.face not in pending]
                    if len(e.link_faces) == 2 and nb:
                        if nb[0].vert == l.vert:  # cung chieu di qua canh -> nguoc huong
                            f.normal_flip()
                        pending.discard(f); changed = True
                        break
            if not changed:
                break
        final = boundary_count(bm)
        bm.to_mesh(me); bm.free()
        bpy.ops.object.shade_smooth_by_angle(angle=math.radians(35))
        out[o.name] = dict(bien_truoc=before, sau_han=after_weld, mat_tram=len(new_faces), vay_xoa=removed_fins, bien_con=final)
    bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", export_image_format="AUTO",
                              export_jpeg_quality=92, export_yup=True)
    print("HOLES", os.path.basename(path), out, flush=True)


for p in sys.argv[1:]:
    if p.endswith(".glb"):
        process(p)

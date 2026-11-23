import math
import os
import shutil
import sys
import tempfile
import time

import bpy
import bmesh
import numpy as np
from mathutils import Euler, Matrix, Vector

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
if SCRIPT_DIR not in sys.path:
    sys.path.insert(0, SCRIPT_DIR)

import build_and_preview_sprint1 as base

REPO_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, "..", ".."))
ART_DIR = os.path.join(REPO_ROOT, "Assets", "_Project", "Art", "Items")
TEX_DIR = os.path.join(REPO_ROOT, "Assets", "_Project", "Resources", "Blender", "Textures")
MODEL_DIR = os.path.join(REPO_ROOT, "Assets", "_Project", "Resources", "Blender", "Models")

MAX_TRIS_PER_ITEM = 24000
MAX_TEX_DIM = 1024

# Official 24-model roster from Docs/Art/BanGiao3D.md & Docs/Content/data/content.json
# Box tuple: (id, rel_glb, target_span, seal_mat_key, cut_ratio, legacy_fnames)
BOX_SPECS = [
    ("H1", os.path.join("Boxes", "H1.glb"), 0.38, "wax_red", 0.82, ("SM_Box_WoodCrate.fbx", "SM_Box_WoodCrate_Open.fbx")),
    ("H2", os.path.join("Boxes", "H2.glb"), 0.38, "cyan_glow", 0.76, ("SM_Box_HoloSciFi.fbx", "SM_Box_HoloSciFi_Open.fbx")),
    ("H3", os.path.join("Boxes", "H3.glb"), 0.38, "wax_red", 0.56, ("SM_Box_RunicArcane.fbx", "SM_Box_RunicArcane_Open.fbx")),
    ("RD", os.path.join("Boxes", "RD.glb"), 0.42, "gold", 0.65, ("SM_Box_BrassChest.fbx", "SM_Box_BrassChest_Open.fbx")),
]

ITEM_SPECS = [
    ("V01", "V01.glb", "Item_V01.fbx", 0.24, False, ()),
    ("V02", "V02.glb", "Item_V02.fbx", 0.24, False, ()),
    ("V03", "V03.glb", "Item_V03.fbx", 0.24, False, ()),
    ("V04", "V04.glb", "Item_V04.fbx", 0.22, True, ()),
    ("V05", "V05.glb", "Item_V05.fbx", 0.22, False, ()),
    ("V06", "V06.glb", "Item_V06.fbx", 0.24, False, ("cyan_core",)),
    ("V07", "V07.glb", "Item_V07.fbx", 0.25, False, ()),
    ("V08", "V08.glb", "Item_V08.fbx", 0.24, True, ()),
    ("V09", "V09.glb", "Item_V09.fbx", 0.22, True, ()),
    ("V10", "V10.glb", "Item_V10.fbx", 0.24, True, ()),
]

ARTIFACT_SPECS = [
    ("CP1", os.path.join("Artifacts", "CP1.glb"), "Artifact_CP1.fbx", 0.22, False, ()),
    ("CP2", os.path.join("Artifacts", "CP2.glb"), "Artifact_CP2.fbx", 0.22, False, ()),
    ("CP3", os.path.join("Artifacts", "CP3.glb"), "Artifact_CP3.fbx", 0.20, False, ()),
    ("TC1", os.path.join("Artifacts", "TC1.glb"), "Artifact_TC1.fbx", 0.20, False, ()),
    ("TC2", os.path.join("Artifacts", "TC2.glb"), "Artifact_TC2.fbx", 0.22, False, ("cyan_core",)),
    ("TC3", os.path.join("Artifacts", "TC3.glb"), "Artifact_TC3.fbx", 0.24, False, ("cyan_core",)),
]

HEIRLOOM_SPECS = [
    ("GT1", os.path.join("Heirlooms", "GT1.glb"), "Heirloom_GT1.fbx", 0.24, False, ()),
    ("GT2", os.path.join("Heirlooms", "GT2.glb"), "Heirloom_GT2.fbx", 0.24, False, ("flame_top",)),
    ("BK1", os.path.join("Heirlooms", "BK1.glb"), "Heirloom_BK1.fbx", 0.24, False, ()),
    ("BK2", os.path.join("Heirlooms", "BK2.glb"), "Heirloom_BK2.fbx", 0.24, False, ("glass_lens",)),
]

METALLIC_IDS = {"H2", "V05", "V06", "CP3", "TC1", "TC2", "TC3"}


def _save_scaled_image_png(src_img, out_path, max_dim=MAX_TEX_DIM, is_normal=False):
    if src_img is None or src_img.size[0] == 0 or src_img.size[1] == 0:
        return False
    tmp = src_img.copy()
    w, h = tmp.size[0], tmp.size[1]
    if w > max_dim or h > max_dim:
        scale = max_dim / float(max(w, h))
        nw = max(64, int(round(w * scale)))
        nh = max(64, int(round(h * scale)))
        tmp.scale(nw, nh)
        w, h = nw, nh

    buf = np.empty(w * h * 4, dtype=np.float32)
    tmp.pixels.foreach_get(buf)

    out_img = bpy.data.images.new(os.path.basename(out_path), width=w, height=h, alpha=True, float_buffer=False)
    if is_normal:
        out_img.colorspace_settings.name = "Non-Color"
    out_img.pixels.foreach_set(buf)
    out_img.filepath_raw = out_path
    out_img.file_format = "PNG"
    out_img.save()

    bpy.data.images.remove(out_img)
    bpy.data.images.remove(tmp)
    return os.path.exists(out_path)


def _find_upstream_image(socket, depth=0):
    if depth > 5 or not socket or not socket.is_linked:
        return None
    for link in socket.links:
        node = link.from_node
        if node.type == "TEX_IMAGE" and node.image is not None:
            return node.image
        for inp in node.inputs:
            found = _find_upstream_image(inp, depth + 1)
            if found is not None:
                return found
    return None


def _extract_glb_textures(meshes, item_id):
    albedo_img = None
    normal_img = None
    seen_mats = set()

    for m in meshes:
        for slot in m.material_slots:
            mat = slot.material
            if not mat or not mat.use_nodes or mat.name in seen_mats:
                continue
            seen_mats.add(mat.name)
            bsdf = next((n for n in mat.node_tree.nodes if n.type == "BSDF_PRINCIPLED"), None)
            if bsdf:
                if albedo_img is None:
                    albedo_img = _find_upstream_image(bsdf.inputs.get("Base Color"))
                if normal_img is None:
                    normal_img = _find_upstream_image(bsdf.inputs.get("Normal"))

    # Fallback: largest packed image in scene if node traversal missed
    if albedo_img is None:
        candidates = [img for img in bpy.data.images if img.size[0] > 0 and "normal" not in img.name.lower()]
        if candidates:
            candidates.sort(key=lambda i: i.size[0] * i.size[1], reverse=True)
            albedo_img = candidates[0]

    os.makedirs(TEX_DIR, exist_ok=True)
    albedo_path = os.path.join(TEX_DIR, f"T_Glb_{item_id}_Albedo.png")
    normal_path = os.path.join(TEX_DIR, f"T_Glb_{item_id}_Normal.png")

    has_albedo = _save_scaled_image_png(albedo_img, albedo_path, MAX_TEX_DIM, is_normal=False)
    has_normal = _save_scaled_image_png(normal_img, normal_path, MAX_TEX_DIM, is_normal=True)
    return (albedo_path if has_albedo else None), (normal_path if has_normal else None)


def _create_glb_blender_material(item_id, albedo_path, normal_path):
    mat_name = f"Mat_Glb_{item_id}"
    if mat_name in bpy.data.materials:
        bpy.data.materials.remove(bpy.data.materials[mat_name], do_unlink=True)
    mat = bpy.data.materials.new(name=mat_name)
    mat.use_nodes = True
    nt = mat.node_tree
    bsdf = nt.nodes.get("Principled BSDF")
    is_metal = item_id in METALLIC_IDS
    bsdf.inputs["Metallic"].default_value = 0.45 if is_metal else 0.08
    bsdf.inputs["Roughness"].default_value = 0.36 if is_metal else 0.55

    if albedo_path and os.path.exists(albedo_path):
        tex = nt.nodes.new("ShaderNodeTexImage")
        tex.image = bpy.data.images.load(albedo_path, check_existing=True)
        nt.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
    if normal_path and os.path.exists(normal_path):
        ntex = nt.nodes.new("ShaderNodeTexImage")
        nimg = bpy.data.images.load(normal_path, check_existing=True)
        nimg.colorspace_settings.name = "Non-Color"
        ntex.image = nimg
        nmap = nt.nodes.new("ShaderNodeNormalMap")
        nt.links.new(ntex.outputs["Color"], nmap.inputs["Color"])
        nt.links.new(nmap.outputs["Normal"], bsdf.inputs["Normal"])
    return mat


def _import_joined_glb_mesh(glb_path, item_id):
    base.clear_scene()
    col = base.get_or_create_collection("ImportGLB")
    bpy.ops.import_scene.gltf(filepath=glb_path)

    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    if not meshes:
        raise RuntimeError(f"No meshes imported from {glb_path}")

    albedo_path, normal_path = _extract_glb_textures(meshes, item_id)
    clean_mat = _create_glb_blender_material(item_id, albedo_path, normal_path)

    # Bake every mesh's world matrix directly into its mesh vertices so glTF parent empties can be dropped safely
    for m in meshes:
        wmat = m.matrix_world.copy()
        m.parent = None
        m.matrix_world = Matrix.Identity(4)
        m.data.transform(wmat)
        m.data.materials.clear()
        m.data.materials.append(clean_mat)
        base.link_obj(m, col)

    for obj in list(bpy.context.scene.objects):
        if obj.type != "MESH":
            bpy.data.objects.remove(obj, do_unlink=True)

    bpy.ops.object.select_all(action="DESELECT")
    for m in meshes:
        m.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    if len(meshes) > 1:
        bpy.ops.object.join()
    joined = bpy.context.view_layer.objects.active
    joined.name = f"Body_{item_id}"

    # Weld split UV/chunk boundary seams BEFORE decimation so collapse never tears holes across the surface
    bm_weld = bmesh.new()
    bm_weld.from_mesh(joined.data)
    bmesh.ops.remove_doubles(bm_weld, verts=bm_weld.verts, dist=0.00008)
    bmesh.ops.recalc_face_normals(bm_weld, faces=bm_weld.faces)
    bm_weld.to_mesh(joined.data)
    bm_weld.free()
    joined.data.update()

    # Decimate heavy Meshy meshes down to <= MAX_TRIS_PER_ITEM while preserving UV coords
    poly_count = len(joined.data.polygons)
    if poly_count > MAX_TRIS_PER_ITEM:
        mod = joined.modifiers.new(name="Decimate", type="DECIMATE")
        mod.decimate_type = "COLLAPSE"
        mod.ratio = max(0.025, float(MAX_TRIS_PER_ITEM) / float(poly_count))
        mod.use_collapse_triangulate = False
        bpy.ops.object.modifier_apply(modifier=mod.name)

    for p in joined.data.polygons:
        p.use_smooth = True

    return joined, clean_mat, col


def _recenter_bottom_and_scale(mesh_obj, target_size, match_horizontal_only=False, is_flat_upright=False):
    bm = bmesh.new()
    bm.from_mesh(mesh_obj.data)
    if not bm.verts:
        bm.free()
        return Vector((0.2, 0.2, 0.2))

    xs = [v.co.x for v in bm.verts]
    ys = [v.co.y for v in bm.verts]
    zs = [v.co.z for v in bm.verts]
    min_co = Vector((min(xs), min(ys), min(zs)))
    max_co = Vector((max(xs), max(ys), max(zs)))
    dims = max_co - min_co

    # Rotate thin flat relics (e.g. V04, V08, V09, V10) so broad face aims -Y (facing viewer) with gentle 12-deg display tilt
    if is_flat_upright:
        center = (min_co + max_co) * 0.5
        bmesh.ops.translate(bm, verts=bm.verts, vec=-center)
        min_axis = min((dims.x, "x"), (dims.y, "y"), (dims.z, "z"))[1]
        if min_axis == "z":
            bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(90.0), 3, "X"))
        elif min_axis == "x":
            bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(90.0), 3, "Z"))
        bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(-12.0), 3, "X"))
        xs = [v.co.x for v in bm.verts]
        ys = [v.co.y for v in bm.verts]
        zs = [v.co.z for v in bm.verts]
        min_co = Vector((min(xs), min(ys), min(zs)))
        max_co = Vector((max(xs), max(ys), max(zs)))
        dims = max_co - min_co

    span = max(dims.x, dims.y) if match_horizontal_only else max(dims.x, dims.y, dims.z)
    scale_factor = target_size / max(span, 1e-4)
    center_xy = Vector(((min_co.x + max_co.x) * 0.5, (min_co.y + max_co.y) * 0.5, min_co.z))
    bmesh.ops.translate(bm, verts=bm.verts, vec=-center_xy)
    bmesh.ops.scale(bm, verts=bm.verts, vec=Vector((scale_factor, scale_factor, scale_factor)))
    if is_flat_upright:
        bmesh.ops.translate(bm, verts=bm.verts, vec=Vector((0.0, 0.0, 0.018)))

    bm.to_mesh(mesh_obj.data)
    bm.free()
    mesh_obj.data.update()

    return Vector((dims.x * scale_factor, dims.y * scale_factor, dims.z * scale_factor))


def _export_fbx_root(root, filename):
    os.makedirs(MODEL_DIR, exist_ok=True)
    objs = [root] + list(root.children_recursive)
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = root

    out_path = os.path.join(MODEL_DIR, filename)
    tmp_path = os.path.join(tempfile.gettempdir(), f"chodola_glb_{filename}")
    bpy.ops.export_scene.fbx(
        filepath=tmp_path,
        use_selection=True,
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        use_space_transform=True,
        bake_space_transform=True,
        axis_forward="Z",
        axis_up="Y",
        mesh_smooth_type="FACE",
        use_mesh_modifiers=True,
        bake_anim=False,
        path_mode="AUTO",
    )
    for attempt in range(5):
        try:
            shutil.copy2(tmp_path, out_path)
            break
        except OSError:
            if attempt == 4:
                raise
            time.sleep(0.4)
    print(f"EXPORTED_GLB_FBX: {out_path}")
    return out_path


def _add_flat_display_stand(root, dims, mats, col):
    stand_w = max(0.08, min(0.16, dims.x * 0.72))
    base.add_box("StandBase", root, (0.0, 0.008, 0.009), (stand_w, 0.075, 0.018), mats["wood_dark"], bevel=0.003, col=col)
    base.add_box("StandBackLip", root, (0.0, 0.036, 0.024), (stand_w * 0.78, 0.016, 0.032), mats["wood_dark"], rot=(math.radians(-12.0), 0.0, 0.0), bevel=0.002, col=col)
    base.add_box("StandFrontLip", root, (0.0, -0.022, 0.018), (stand_w * 0.78, 0.012, 0.012), mats["brass"], bevel=0.002, col=col)


def _apply_special_fixes(root, dims, special_fixes, mats, col):
    if "glass_lens" in special_fixes:
        lens_r = max(0.032, min(dims.x, dims.z) * 0.28)
        base.add_cyl("GlassLens", root, (0.0, -0.004, dims.z * 0.52), lens_r, 0.012, mats["glass"], rot=(math.pi / 2, 0.0, 0.0), verts=24, bevel=0.002, col=col)
        base.add_torus_ring("LensBrassRing", root, (0.0, -0.008, dims.z * 0.52), lens_r, 0.005, mats["gold"], rot=(math.pi / 2, 0.0, 0.0), col=col)
    if "cyan_core" in special_fixes:
        core_r = max(0.016, min(dims.x, dims.y, dims.z) * 0.16)
        base.add_sphere("CyanCoreGlow", root, (0.0, 0.0, dims.z * 0.52), core_r, mats["cyan_glow"], segs=16, col=col)
        base.add_torus_ring("CyanHaloRing", root, (0.0, 0.0, dims.z * 0.52), core_r * 1.45, 0.004, mats["cyan_glow"], rot=(math.radians(35.0), 0.0, 0.0), col=col)
    if "flame_top" in special_fixes:
        base.add_cone_frustum("OilLampFlame", root, (0.0, 0.0, dims.z * 0.96), 0.014, 0.002, 0.038, mats["warm_glow"], verts=12, col=col)
        base.add_sphere("OilLampEmber", root, (0.0, 0.0, dims.z * 0.945), 0.013, mats["warm_glow"], segs=12, col=col)


def _bisect_box_with_bmesh(src_mesh, cut_z, keep_upper):
    new_mesh = src_mesh.data.copy()
    bm = bmesh.new()
    bm.from_mesh(new_mesh)
    geom = bm.verts[:] + bm.edges[:] + bm.faces[:]
    ret = bmesh.ops.bisect_plane(
        bm,
        geom=geom,
        plane_co=(0.0, 0.0, cut_z),
        plane_no=(0.0, 0.0, 1.0),
        clear_outer=not keep_upper,
        clear_inner=keep_upper,
    )
    cut_edges = [g for g in ret.get("geom_cut", []) if isinstance(g, bmesh.types.BMEdge) and g.is_valid]
    if cut_edges:
        try:
            bmesh.ops.triangle_fill(bm, edges=cut_edges, use_beauty=False)
        except Exception:
            try:
                bmesh.ops.holes_fill(bm, edges=cut_edges, sides=0)
            except Exception:
                pass
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(new_mesh)
    bm.free()
    new_mesh.update()
    return new_mesh


def _apply_solidify_inward(obj, thickness=0.016):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    mod = obj.modifiers.new(name="Solidify", type="SOLIDIFY")
    mod.thickness = thickness
    mod.offset = -1.0
    mod.use_rim = True
    bpy.ops.object.modifier_apply(modifier=mod.name)


def process_box_model(item_id, rel_glb, target_span, seal_mat_key, cut_ratio, legacy_fnames):
    glb_path = os.path.join(ART_DIR, rel_glb)
    if not os.path.exists(glb_path):
        print(f"SKIP missing GLB: {glb_path}")
        return

    body_mesh_obj, _, col = _import_joined_glb_mesh(glb_path, item_id)
    mats = base.create_materials()
    dims = _recenter_bottom_and_scale(body_mesh_obj, target_span, match_horizontal_only=True, is_flat_upright=False)

    cut_z = dims.z * cut_ratio
    lid_data = _bisect_box_with_bmesh(body_mesh_obj, cut_z, keep_upper=True)
    body_data = _bisect_box_with_bmesh(body_mesh_obj, cut_z, keep_upper=False)

    root = bpy.data.objects.new(f"Box_{item_id}", None)
    base.link_obj(root, col)

    body_obj = bpy.data.objects.new("BoxBody", body_data)
    body_obj.parent = root
    base.link_obj(body_obj, col)
    _apply_solidify_inward(body_obj, thickness=0.016)

    # Interior dark wood rim + recessed plush velvet cushion + 4 raised side velvet bolsters
    rim_w = max(0.14, dims.x * 0.90)
    rim_d = max(0.12, dims.y * 0.90)
    base.add_box("VelvetRim", root, (0.0, 0.0, cut_z - 0.005), (rim_w, rim_d, 0.020), mats["wood_dark"], bevel=0.002, col=col)
    base.add_box("VelvetInterior", root, (0.0, 0.0, cut_z - 0.002), (rim_w * 0.88, rim_d * 0.88, 0.018), mats["velvet"], bevel=0.003, col=col)
    bolster_h = 0.018
    for sx in (-1.0, 1.0):
        base.add_box(f"VelvetBolsterX_{sx}", root, (sx * rim_w * 0.41, 0.0, cut_z + 0.003), (rim_w * 0.08, rim_d * 0.82, bolster_h), mats["velvet"], bevel=0.003, col=col)
    for sy in (-1.0, 1.0):
        base.add_box(f"VelvetBolsterY_{sy}", root, (0.0, sy * rim_d * 0.41, cut_z + 0.003), (rim_w * 0.84, rim_d * 0.08, bolster_h), mats["velvet"], bevel=0.003, col=col)

    # Brass hinge plates on back edge (+Y)
    for hx_s in (-1.0, 1.0):
        base.add_box(f"HingePlate_{hx_s}", root, (hx_s * dims.x * 0.28, dims.y * 0.47, cut_z), (0.036, 0.014, 0.026), mats["brass"], bevel=0.002, col=col)

    # Watertight solid LidPivot + solid framed wood & crimson velvet underside plug (never transparent when opened!)
    lid_obj = bpy.data.objects.new("LidPivot", lid_data)
    lid_obj.parent = root
    base.link_obj(lid_obj, col)
    _apply_solidify_inward(lid_obj, thickness=0.016)

    lid_plug_w = max(0.12, dims.x * 0.88)
    lid_plug_d = max(0.10, dims.y * 0.88)
    lid_wood_plug = base.add_box("LidInnerPanel", None, (0.0, 0.0, cut_z + 0.011), (lid_plug_w, lid_plug_d, 0.020), mats["wood_dark"], bevel=0.002, col=col)
    lid_velvet_plug = base.add_box("LidVelvetInlay", None, (0.0, 0.0, cut_z + 0.004), (lid_plug_w * 0.84, lid_plug_d * 0.84, 0.010), mats["velvet"], bevel=0.002, col=col)
    for plug in (lid_wood_plug, lid_velvet_plug):
        bpy.ops.object.select_all(action="DESELECT")
        plug.select_set(True)
        bpy.context.view_layer.objects.active = plug
        for mod in list(plug.modifiers):
            bpy.ops.object.modifier_apply(modifier=mod.name)

    bpy.ops.object.select_all(action="DESELECT")
    lid_obj.select_set(True)
    lid_wood_plug.select_set(True)
    lid_velvet_plug.select_set(True)
    bpy.context.view_layer.objects.active = lid_obj
    bpy.ops.object.join()

    hinge_y = dims.y * 0.46
    bm_lid = bmesh.new()
    bm_lid.from_mesh(lid_obj.data)
    bmesh.ops.translate(bm_lid, verts=bm_lid.verts, vec=Vector((0.0, -hinge_y, -cut_z)))
    bm_lid.to_mesh(lid_obj.data)
    bm_lid.free()
    lid_obj.location = Vector((0.0, hinge_y, cut_z))

    # Front Seal / Clasp at -Y facing the camera/player
    seal_obj = base.add_cyl(
        "SealPart",
        root,
        (0.0, -dims.y * 0.49, cut_z),
        0.028,
        0.012,
        mats[seal_mat_key],
        rot=(math.pi / 2, 0.0, 0.0),
        verts=16,
        bevel=0.002,
        col=col,
    )

    bpy.data.objects.remove(body_mesh_obj, do_unlink=True)

    # Apply transforms cleanly before export (only LidPivot keeps hinge location)
    closed_fbx = f"Box_{item_id}.fbx"
    open_fbx = f"Box_{item_id}_Open.fbx"
    closed_path = _export_fbx_root(root, closed_fbx)

    # Open-lid variant
    lid_obj.rotation_euler = Euler((math.radians(-102.0), 0.0, 0.0))
    bpy.data.objects.remove(seal_obj, do_unlink=True)
    open_path = _export_fbx_root(root, open_fbx)

    # Maintain backward compatibility aliases (SM_Box_*) so existing Unity references continue working seamlessly
    shutil.copy2(closed_path, os.path.join(MODEL_DIR, legacy_fnames[0]))
    shutil.copy2(open_path, os.path.join(MODEL_DIR, legacy_fnames[1]))


def process_standard_glb_model(item_id, rel_glb, out_fbx, target_size, is_flat_upright, special_fixes):
    glb_path = os.path.join(ART_DIR, rel_glb)
    if not os.path.exists(glb_path):
        print(f"SKIP missing GLB: {glb_path}")
        return

    mesh_obj, _, col = _import_joined_glb_mesh(glb_path, item_id)
    mats = base.create_materials()
    dims = _recenter_bottom_and_scale(mesh_obj, target_size, match_horizontal_only=False, is_flat_upright=is_flat_upright)

    root = bpy.data.objects.new(os.path.splitext(out_fbx)[0], None)
    base.link_obj(root, col)
    mesh_obj.parent = root

    if is_flat_upright:
        _add_flat_display_stand(root, dims, mats, col)
    if special_fixes:
        _apply_special_fixes(root, dims, special_fixes, mats, col)

    _export_fbx_root(root, out_fbx)


def export_all_24_glb_models():
    os.makedirs(TEX_DIR, exist_ok=True)
    os.makedirs(MODEL_DIR, exist_ok=True)

    for spec in BOX_SPECS:
        process_box_model(*spec)
    for spec in ITEM_SPECS:
        process_standard_glb_model(*spec)
    for spec in ARTIFACT_SPECS:
        process_standard_glb_model(*spec)
    for spec in HEIRLOOM_SPECS:
        process_standard_glb_model(*spec)
    print("ALL_24_GLB_MODELS_OPTIMIZED_AND_EXPORTED")


if __name__ == "__main__":
    export_all_24_glb_models()

import math
import os
import random
import sys
import bpy
import bmesh
from mathutils import Vector, Euler, Matrix

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
if SCRIPT_DIR not in sys.path:
    sys.path.insert(0, SCRIPT_DIR)

import build_and_preview_sprint1 as base
import hanoi_highcraft_arch as arch
import import_and_optimize_glb_items as glb_opt

REPO_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, "..", ".."))
TEX_DIR = os.path.join(REPO_ROOT, "Assets", "_Project", "Resources", "Blender", "Textures")
MODEL_DIR = os.path.join(REPO_ROOT, "Assets", "_Project", "Resources", "Blender", "Models")


def bake_pbr_texture_pair(mat, out_name, size=256, vertical_wall=True, plane_w=2.0, plane_h=2.0, bake_normal=True):
    """Bakes seamless Albedo (and optionally Tangent Normal) PNGs from a Cycles procedural material."""
    bpy.ops.wm.read_factory_settings(use_empty=True)
    mats = base.create_materials()
    target_mat = mats[mat]

    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 4
    scene.cycles.use_denoising = False

    mesh = bpy.data.meshes.new("BakePlane_Mesh")
    bm = bmesh.new()
    if vertical_wall:
        v0 = bm.verts.new((0.0, 0.0, 0.0))
        v1 = bm.verts.new((plane_w, 0.0, 0.0))
        v2 = bm.verts.new((plane_w, 0.0, plane_h))
        v3 = bm.verts.new((0.0, 0.0, plane_h))
    else:
        v0 = bm.verts.new((0.0, 0.0, 0.0))
        v1 = bm.verts.new((plane_w, 0.0, 0.0))
        v2 = bm.verts.new((plane_w, plane_h, 0.0))
        v3 = bm.verts.new((0.0, plane_h, 0.0))

    face = bm.faces.new((v0, v1, v2, v3))
    uv_layer = bm.loops.layers.uv.verify()
    for loop, uv in zip(face.loops, [(0.0, 0.0), (1.0, 0.0), (1.0, 1.0), (0.0, 1.0)]):
        loop[uv_layer].uv = uv
    bm.to_mesh(mesh)
    bm.free()

    obj = bpy.data.objects.new("BakePlane", mesh)
    scene.collection.objects.link(obj)
    obj.data.materials.append(target_mat)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)

    nt = target_mat.node_tree
    img_node = nt.nodes.new("ShaderNodeTexImage")
    nt.nodes.active = img_node

    img_albedo = bpy.data.images.new(f"{out_name}_Albedo", width=size, height=size)
    img_node.image = img_albedo
    scene.cycles.bake_type = "DIFFUSE"
    scene.render.bake.use_pass_direct = False
    scene.render.bake.use_pass_indirect = False
    scene.render.bake.use_pass_color = True
    bpy.ops.object.bake(type="DIFFUSE")

    albedo_path = os.path.join(TEX_DIR, f"{out_name}_Albedo.png")
    img_albedo.filepath_raw = albedo_path
    img_albedo.file_format = "PNG"
    img_albedo.save()

    if bake_normal:
        img_norm = bpy.data.images.new(f"{out_name}_Normal", width=size, height=size)
        img_norm.colorspace_settings.name = "Non-Color"
        img_node.image = img_norm
        scene.cycles.bake_type = "NORMAL"
        scene.render.bake.normal_space = "TANGENT"
        bpy.ops.object.bake(type="NORMAL")

        norm_path = os.path.join(TEX_DIR, f"{out_name}_Normal.png")
        img_norm.filepath_raw = norm_path
        img_norm.file_format = "PNG"
        img_norm.save()


def bake_all_textures_if_missing():
    os.makedirs(TEX_DIR, exist_ok=True)
    specs = [
        ("cobble", "T_Cobble", False, 2.5, 2.5, True, False),
        ("terrazzo_paving", "T_HanoiTerrazzo", False, 2.5, 2.5, True, True),
        ("vn_red_tile", "T_VnRedTile", False, 2.5, 2.5, True, True),
        ("vn_street_matte", "T_VnStreetMatte", False, 2.5, 2.5, True, True),
        ("plaster_weathered", "T_WeatheredPlaster", True, 2.4, 3.0, True, False),
        ("brick_exposed", "T_ExposedBrick", True, (0.42 / 2.2) * 8, (2 * 0.17 / 2.2) * 8, True, False),
        ("brick_glazed_teal", "T_GlazedBrickTeal", True, (0.46 / 2.8) * 8, (2 * 0.20 / 2.8) * 8, True, False),
        ("brick_whitewash", "T_WhitewashedBrick", True, (0.45 / 2.4) * 8, (2 * 0.19 / 2.4) * 8, True, False),
        ("ashlar_stone", "T_AshlarStone", True, (0.85 / 1.25) * 4, (2 * 0.40 / 1.25) * 4, True, False),
        ("wood_aged_floor", "T_AgedWood", False, 2.0, 2.0, True, False),
        ("workshop_floor", "T_WorkshopFloor", False, 2.4, 2.4, True, False),
        ("roof_tile", "T_RoofTile", True, (0.38 / 2.4) * 8, (2 * 0.24 / 2.4) * 6, False, False),
        ("roof_slate", "T_RoofSlate", True, (0.42 / 2.4) * 8, (2 * 0.24 / 2.4) * 6, False, False),
    ]
    for mat_key, out_name, is_vert, pw, ph, do_norm, force in specs:
        a_exists = os.path.exists(os.path.join(TEX_DIR, f"{out_name}_Albedo.png"))
        n_exists = (not do_norm) or os.path.exists(os.path.join(TEX_DIR, f"{out_name}_Normal.png"))
        if (not force) and a_exists and n_exists:
            continue
        tex_sz = 512 if force else 256
        bake_pbr_texture_pair(mat_key, out_name, size=tex_sz, vertical_wall=is_vert, plane_w=pw, plane_h=ph, bake_normal=do_norm)
    generate_hanoi_encaustic_tile_texture()


def generate_hanoi_encaustic_tile_texture():
    """Generates seamless 512x512 Indochine floral diamond encaustic tile Albedo + Normal PNGs matching concept_2."""
    albedo_path = os.path.join(TEX_DIR, "T_HanoiEncausticTile_Albedo.png")
    normal_path = os.path.join(TEX_DIR, "T_HanoiEncausticTile_Normal.png")
    size = 512
    half = size // 2
    img_a = bpy.data.images.new("T_HanoiEncausticTile_Albedo", width=size, height=size)
    img_n = bpy.data.images.new("T_HanoiEncausticTile_Normal", width=size, height=size)
    pixels_a = [0.0] * (size * size * 4)
    pixels_n = [0.0] * (size * size * 4)

    cream = (0.88, 0.82, 0.70)
    blue = (0.22, 0.34, 0.48)
    gold = (0.76, 0.56, 0.22)
    grout = (0.28, 0.24, 0.19)

    for py in range(size):
        for px in range(size):
            # 2x2 sub-tiles per 512px texture
            u = (px % half) / float(half) - 0.5
            v = (py % half) / float(half) - 0.5
            ax, ay = abs(u), abs(v)
            edge = max(ax, ay)
            diag = ax + ay
            ang = math.atan2(v, u)
            r = math.hypot(u, v)

            col = cream
            nx, ny = 0.0, 0.0
            if edge > 0.482:
                col = grout
                nx = -0.35 if u < -0.48 else (0.35 if u > 0.48 else 0.0)
                ny = -0.35 if v < -0.48 else (0.35 if v > 0.48 else 0.0)
            elif abs(diag - 0.50) < 0.045:
                col = gold
            elif abs(r - 0.18 * (1.0 + 0.55 * abs(math.cos(2.0 * ang)))) < 0.055 and r < 0.34:
                col = blue
            elif r < 0.07:
                col = gold
            elif edge > 0.445:
                col = blue

            # Slight aged stone speckle
            grain = (((px * 73 + py * 139) % 19) - 9) * 0.0025
            idx = (py * size + px) * 4
            pixels_a[idx] = max(0.0, min(1.0, col[0] + grain))
            pixels_a[idx + 1] = max(0.0, min(1.0, col[1] + grain))
            pixels_a[idx + 2] = max(0.0, min(1.0, col[2] + grain))
            pixels_a[idx + 3] = 1.0
            pixels_n[idx] = 0.5 + nx * 0.5
            pixels_n[idx + 1] = 0.5 + ny * 0.5
            pixels_n[idx + 2] = 1.0
            pixels_n[idx + 3] = 1.0

    img_a.pixels.foreach_set(pixels_a)
    img_a.filepath_raw = albedo_path
    img_a.file_format = "PNG"
    img_a.save()

    img_n.colorspace_settings.name = "Non-Color"
    img_n.pixels.foreach_set(pixels_n)
    img_n.filepath_raw = normal_path
    img_n.file_format = "PNG"
    img_n.save()


def build_extra_artifacts(name, key, mats, col):
    if key == "Amulet_Jade":
        root = bpy.data.objects.new(name, None)
        base.link_obj(root, col)
        base.add_cyl("JadeDisc", root, (0, 0, 0.10), 0.075, 0.018, mats["jade"], rot=(0, math.pi / 2, 0), verts=32, col=col)
        base.add_torus_ring("GoldRim", root, (0, 0, 0.10), 0.075, 0.010, mats["gold"], rot=(0, math.pi / 2, 0), col=col)
        base.add_cyl("CenterSeal", root, (0, 0, 0.10), 0.025, 0.022, mats["gold"], rot=(0, math.pi / 2, 0), verts=16, col=col)
        base.add_torus_ring("TopBail", root, (0, 0, 0.19), 0.018, 0.005, mats["gold"], rot=(0, math.pi / 2, 0), col=col)
        return root
    if key == "GearCore_Steam":
        root = bpy.data.objects.new(name, None)
        base.link_obj(root, col)
        base.add_gear_wheel("MainWheel", root, (0, 0, 0.11), 0.085, 0.022, 10, mats["brass"], rot=(0, math.pi / 2, 0), col=col)
        base.add_gear_wheel("SubWheel", root, (0.015, 0.045, 0.065), 0.045, 0.016, 8, mats["gold"], rot=(0, math.pi / 2, 0.3), col=col)
        base.add_sphere("FurnaceCore", root, (0, 0, 0.11), 0.032, mats["warm_glow"], col=col)
        return root
    return base.build_artifact(name, key, (0, 0, 0), mats, col, parent=None, scale=1.0)


def convert_and_apply_modifiers(root, uv_scale=0.5):
    """Deletes lights/cameras, converts FONT to MESH, applies Bevel modifiers, and assigns box-projected UVs."""
    bpy.context.view_layer.update()
    for o in list(root.children_recursive):
        if o.type in ("LIGHT", "CAMERA"):
            bpy.data.objects.remove(o, do_unlink=True)

    for o in list(root.children_recursive):
        if o.type == "FONT":
            bpy.ops.object.select_all(action="DESELECT")
            o.select_set(True)
            bpy.context.view_layer.objects.active = o
            bpy.ops.object.convert(target="MESH")

    for o in list(root.children_recursive):
        if o.type != "MESH":
            continue
        if len(o.modifiers) > 0:
            bpy.ops.object.select_all(action="DESELECT")
            o.select_set(True)
            bpy.context.view_layer.objects.active = o
            for mod in list(o.modifiers):
                if mod.type == "BEVEL":
                    o.modifiers.remove(mod)
                    continue
                try:
                    bpy.ops.object.modifier_apply(modifier=mod.name)
                except Exception:
                    pass

        bm = bmesh.new()
        bm.from_mesh(o.data)
        uv_layer = bm.loops.layers.uv.verify()
        for face in bm.faces:
            face.smooth = False
            n = face.normal
            ax, ay, az = abs(n.x), abs(n.y), abs(n.z)
            for loop in face.loops:
                co = loop.vert.co
                if az >= ax and az >= ay:
                    u, v = co.x * uv_scale, co.y * uv_scale
                elif ax >= ay:
                    u, v = co.y * uv_scale, co.z * uv_scale
                else:
                    u, v = co.x * uv_scale, co.z * uv_scale
                loop[uv_layer].uv = (u, v)
        bm.to_mesh(o.data)
        bm.free()


def join_empty_into_single_mesh(empty_obj, target_root):
    """Joins all mesh children of an EMPTY into one mesh at the empty's origin, preserving materials."""
    meshes = [o for o in empty_obj.children_recursive if o.type == "MESH"]
    if not meshes:
        return None
    pivot_world = empty_obj.matrix_world.to_translation().copy()
    bpy.ops.object.select_all(action="DESELECT")
    for m in meshes:
        m.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    if len(meshes) > 1:
        bpy.ops.object.join()
    joined = bpy.context.view_layer.objects.active
    joined.name = empty_obj.name
    wmat = joined.matrix_world.copy()
    joined.parent = target_root
    joined.matrix_world = wmat
    bpy.context.scene.cursor.location = pivot_world
    bpy.ops.object.select_all(action="DESELECT")
    joined.select_set(True)
    bpy.context.view_layer.objects.active = joined
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return joined


def flatten_to_root(root, animated_empty_prefixes=()):
    """
    Eliminates 2-level nested EMPTY objects (which Blender's FBX bake_space_transform rotates 90 deg)
    by joining animated sub-assemblies at their empty pivot, reparenting remaining meshes directly
    to root, deleting intermediate empties, and applying rot/scale.
    """
    bpy.context.view_layer.update()
    convert_and_apply_modifiers(root)

    # Join named animated empty assemblies while root transform is still active (preserving pivot + multi-materials)
    if animated_empty_prefixes:
        # Deepest-first order so child empties join cleanly
        empties = [o for o in root.children_recursive if o.type != "MESH"]
        for e in empties:
            if any(e.name.startswith(p) for p in animated_empty_prefixes):
                join_empty_into_single_mesh(e, root)

    meshes = [o for o in root.children_recursive if o.type == "MESH"]
    empties = [o for o in root.children_recursive if o.type != "MESH"]
    world_mats = {m: m.matrix_world.copy() for m in meshes}

    root.rotation_euler = Euler((0.0, 0.0, 0.0))
    root.scale = Vector((1.0, 1.0, 1.0))
    bpy.context.view_layer.update()

    for m in meshes:
        m.parent = root
        m.matrix_world = world_mats[m]

    for e in empties:
        bpy.data.objects.remove(e, do_unlink=True)
    bpy.context.view_layer.update()

    for m in meshes:
        bpy.ops.object.select_all(action="DESELECT")
        m.select_set(True)
        bpy.context.view_layer.objects.active = m
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)


def export_clean_fbx(root, filename):
    import shutil
    import tempfile
    import time

    os.makedirs(MODEL_DIR, exist_ok=True)
    objs = [root] + list(root.children_recursive)
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = root

    out_path = os.path.join(MODEL_DIR, filename)
    tmp_path = os.path.join(tempfile.gettempdir(), f"chodola_export_{filename}")
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
            time.sleep(0.5)
    print("EXPORTED_FBX:", out_path)


def remove_children_by_prefix(root, prefixes):
    for child in list(root.children_recursive):
        if any(child.name.startswith(p) for p in prefixes):
            bpy.data.objects.remove(child, do_unlink=True)


def build_and_export_blind_box(variant, fname, open_lid=False):
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    b = base.build_blind_box(f"Box_{variant}", variant, (0, 0, 0), mats, col)
    # Rotate box -90 deg around Blender Z so front SealPart faces -Y (Unity -Z) and back hinge is +Y (Unity +Z)
    b.rotation_euler = Euler((0, 0, -math.pi / 2))
    bpy.context.view_layer.update()
    convert_and_apply_modifiers(b)

    # Identify LidPivot empty & its child meshes BEFORE flattening
    lid_empty = next((o for o in b.children_recursive if o.name == "LidPivot"), None)
    lid_meshes = [o for o in lid_empty.children_recursive if o.type == "MESH"] if lid_empty else []

    # Flatten all meshes directly to b and bake their world rotation & scale
    meshes = [o for o in b.children_recursive if o.type == "MESH"]
    world_mats = {m: m.matrix_world.copy() for m in meshes}
    b.rotation_euler = Euler((0.0, 0.0, 0.0))
    bpy.context.view_layer.update()
    for m in meshes:
        m.parent = b
        m.matrix_world = world_mats[m]
    if lid_empty:
        bpy.data.objects.remove(lid_empty, do_unlink=True)
    bpy.context.view_layer.update()

    for m in meshes:
        bpy.ops.object.select_all(action="DESELECT")
        m.select_set(True)
        bpy.context.view_layer.objects.active = m
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)

    # Join lid meshes into one single Mesh named "LidPivot" with origin right on the back hinge (0, +0.14, 0.19)
    if lid_meshes:
        bpy.ops.object.select_all(action="DESELECT")
        for lm in lid_meshes:
            lm.select_set(True)
        bpy.context.view_layer.objects.active = lid_meshes[0]
        if len(lid_meshes) > 1:
            bpy.ops.object.join()
        joined_lid = bpy.context.view_layer.objects.active
        joined_lid.name = "LidPivot"
        bpy.context.scene.cursor.location = Vector((0.0, 0.14, 0.19))
        bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        if open_lid:
            joined_lid.rotation_euler = Euler((math.radians(-105.0), 0.0, 0.0))

    if open_lid:
        seal = next((o for o in b.children_recursive if o.name == "SealPart"), None)
        if seal:
            bpy.data.objects.remove(seal, do_unlink=True)

    export_clean_fbx(b, fname)


def build_and_export_street_lantern():
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    root = bpy.data.objects.new("StreetLantern", None)
    base.link_obj(root, col)

    # Turned octagonal granite pedestal & cast-iron fluted colonnade lamp post (Indochine heritage lamp)
    base_prof = [(0.0, 0.0), (0.20, 0.0), (0.20, 0.14), (0.16, 0.22), (0.16, 0.42), (0.11, 0.48), (0.0, 0.48)]
    arch.add_lathe_mesh("StreetBase", root, base_prof, (0, 0, 0), mats["stone_dark"], segments=8, smooth=False, col=col)
    post_prof = [(0.095, 0.48), (0.070, 0.72), (0.058, 1.85), (0.078, 1.92), (0.052, 2.00), (0.048, 2.74), (0.09, 2.82)]
    arch.add_lathe_mesh("StreetPost", root, post_prof, (0, 0, 0), mats["iron"], segments=14, col=col)
    arch.add_bezier_scroll_mesh("ScrollL", root, [(0, 0, 2.35), (0, -0.14, 2.55), (0, -0.08, 2.75), (0, 0, 2.82)], 0.014, (0, 0, 0), mats["brass"], col=col)
    arch.add_bezier_scroll_mesh("ScrollR", root, [(0, 0, 2.35), (0, 0.14, 2.55), (0, 0.08, 2.75), (0, 0, 2.82)], 0.014, (0, 0, 0), mats["brass"], col=col)
    cage_prof = [(0.0, 2.82), (0.12, 2.86), (0.155, 3.12), (0.09, 3.18), (0.0, 3.18)]
    arch.add_lathe_mesh("StreetLantern", root, cage_prof, (0, 0, 0), mats["warm_glow"], segments=6, smooth=False, col=col)
    cap_prof = [(0.21, 3.17), (0.09, 3.28), (0.025, 3.36), (0.0, 3.42)]
    arch.add_lathe_mesh("StreetCap", root, cap_prof, (0, 0, 0), mats["iron"], segments=6, smooth=False, col=col)

    flatten_to_root(root)
    export_clean_fbx(root, "SM_StreetLantern.fbx")


def build_and_export_plaza_street_details(rng):
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    root = bpy.data.objects.new("Plaza_StreetDetails", None)
    base.link_obj(root, col)

    # Narrowed by half between storefronts (X = -7.2 .. +7.2, 14.4m wide total), Z = 0.0 .. 111.8 (Y = -45.0 .. +66.8)
    sidewalk_slabs = [
        ("Sidewalk_Plaza_W", (-5.9, 0.0, 0.045), (2.6, 28.0, 0.09)),
        ("Sidewalk_Plaza_E", (5.9, 0.0, 0.045), (2.6, 28.0, 0.09)),
        ("Sidewalk_South_W", (-5.6, -29.5, 0.045), (3.2, 31.0, 0.09)),
        ("Sidewalk_South_E", (5.6, -29.5, 0.045), (3.2, 31.0, 0.09)),
        ("Sidewalk_North_W", (-5.6, 40.4, 0.045), (3.2, 52.8, 0.09)),
        ("Sidewalk_North_E", (5.6, 40.4, 0.045), (3.2, 52.8, 0.09)),
    ]
    for sname, sloc, ssz in sidewalk_slabs:
        base.add_box(sname, root, sloc, ssz, mats["terrazzo_paving"], bevel=0.010, col=col)

    for rname, rx in (("RedRunner_W", -5.8), ("RedRunner_E", 5.8)):
        base.add_box(rname, root, (rx, 10.9, 0.092), (1.25, 111.0, 0.012), mats["vn_red_tile"], bevel=0.004, col=col)

    for drain_x in (-4.4, 4.4):
        for dy in (-36.0, -22.0, -8.0, 8.0, 22.0, 38.0, 54.0):
            base.add_box(f"DrainGrate_{drain_x}_{dy}", root, (drain_x, dy, 0.008), (0.34, 0.78, 0.016), mats["steel_black"], bevel=0.003, col=col)

    # Crisp Granite Curbstones along narrowed street + plaza transition cross-curbs
    curb_segments = [
        (-4.58, -13.8, 13.8),
        (4.58, -13.8, 13.8),
        (-3.98, -44.8, -14.0),
        (3.98, -44.8, -14.0),
        (-3.98, 14.0, 66.5),
        (3.98, 14.0, 66.5),
    ]
    c_idx = 0
    for cx, y_min, y_max in curb_segments:
        y = y_min
        while y <= y_max:
            m = mats["ashlar_stone"] if c_idx % 3 != 0 else mats["stone_dark"]
            base.add_box(f"Curb_{c_idx}", root, (cx, y, 0.065), (0.24, 0.96, 0.14), m, bevel=0.010, col=col)
            y += 1.00
            c_idx += 1

    for tname, tx, ty in (("CW_S", -4.28, -14.0), ("CE_S", 4.28, -14.0), ("CW_N", -4.28, 14.0), ("CE_N", 4.28, 14.0)):
        base.add_box(f"CurbJoin_{tname}", root, (tx, ty, 0.065), (0.85, 0.24, 0.14), mats["ashlar_stone"], bevel=0.008, col=col)

    flatten_to_root(root)
    export_clean_fbx(root, "SM_Plaza_StreetDetails.fbx")


def export_hanoi_assets(rng):
    # 0. Modern Hanoi Pedestrian Park Island (replaces old central fountain!)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    font = base.load_unicode_font()
    island = base.build_hanoi_center_island(mats, font, rng, col)
    flatten_to_root(island)
    export_clean_fbx(island, "SM_Hanoi_CenterIsland.fbx")
    # A. Hanoi Street Tree (Cây Sấu / Xà Cừ trong bồn đá)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    tree = base.build_hanoi_street_tree("SM_Hanoi_StreetTree", (0, 0, 0), mats, rng, col, scale=1.0)
    flatten_to_root(tree)
    export_clean_fbx(tree, "SM_Hanoi_StreetTree.fbx")

    # B. Quán Phở Bò Hà Nội Hiện Đại
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    font = base.load_unicode_font()
    pho = base.build_hanoi_pho_shop("SM_Hanoi_PhoShop", (0, 0, 0), True, mats, font, rng, col)
    flatten_to_root(pho)
    export_clean_fbx(pho, "SM_Hanoi_PhoShop.fbx")

    # C. Quầy Trà Đá & Vé Số Vỉa Hè
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    font = base.load_unicode_font()
    trada = base.build_hanoi_trada_veso("SM_Hanoi_TraDaVeSo", (0, 0, 0), True, mats, font, rng, col)
    flatten_to_root(trada)
    export_clean_fbx(trada, "SM_Hanoi_TraDaVeSo.fbx")

    # D. Modern Hanoi Multi-Story Tube Shophouse Block
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    font = base.load_unicode_font()
    block = base.build_hanoi_shophouse_block("SM_Hanoi_ShophouseBlock", (0, 0, 0), True, mats, font, rng, col)
    flatten_to_root(block)
    export_clean_fbx(block, "SM_Hanoi_ShophouseBlock.fbx")

    # E. Hanoi Crossroad Car Road Network, Sidewalks & Street Furniture
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    font = base.load_unicode_font()
    road = base.build_hanoi_car_road_network(mats, font, col)
    flatten_to_root(road)
    export_clean_fbx(road, "SM_Hanoi_CarRoadNetwork.fbx")


def build_and_export_market_props_and_npcs():
    """Exports high-craft detached market environment props (NO NPCs or vehicles)."""
    # 1. SM_Prop_PlasticStool.fbx (Hanoi red low plastic stool)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    root = bpy.data.objects.new("Prop_PlasticStool", None)
    base.link_obj(root, col)
    base.add_box("Seat", root, (0, 0, 0.30), (0.28, 0.28, 0.03), mats["plastic_red"], bevel=0.008, col=col)
    for sx in (-0.10, 0.10):
        for sy in (-0.10, 0.10):
            base.add_cyl(f"Leg_{sx}_{sy}", root, (sx, sy, 0.15), 0.018, 0.30, mats["plastic_red"], col=col)
    flatten_to_root(root)
    export_clean_fbx(root, "SM_Prop_PlasticStool.fbx")

    # 2. SM_Prop_PlasticTable.fbx (Low sidewalk tea table with turned ceramic teapot & cups)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    root = bpy.data.objects.new("Prop_PlasticTable", None)
    base.link_obj(root, col)
    base.add_box("Top", root, (0, 0, 0.46), (0.56, 0.42, 0.03), mats["plastic_blue"], bevel=0.008, col=col)
    for sx in (-0.23, 0.23):
        for sy in (-0.16, 0.16):
            base.add_cyl(f"Leg_{sx}_{sy}", root, (sx, sy, 0.23), 0.020, 0.46, mats["plastic_blue"], col=col)
    arch.add_lathe_mesh("Teapot", root, [(0, 0.475), (0.045, 0.48), (0.068, 0.52), (0.055, 0.565), (0.028, 0.585), (0, 0.595)], (-0.08, 0.02, 0), mats["wall_cream"], segments=16, col=col)
    arch.add_lathe_mesh("Cup1", root, [(0, 0.475), (0.022, 0.48), (0.028, 0.525), (0.024, 0.525), (0, 0.49)], (0.09, -0.06, 0), mats["dial_white"], segments=12, col=col)
    arch.add_lathe_mesh("Cup2", root, [(0, 0.475), (0.022, 0.48), (0.028, 0.525), (0.024, 0.525), (0, 0.49)], (0.12, 0.07, 0), mats["dial_white"], segments=12, col=col)
    flatten_to_root(root)
    export_clean_fbx(root, "SM_Prop_PlasticTable.fbx")

    # 3. SM_Prop_BambooBasket.fbx (Lathe-curved woven bamboo market basket with folded cloth)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    root = bpy.data.objects.new("Prop_BambooBasket", None)
    base.link_obj(root, col)
    basket_prof = [(0, 0), (0.15, 0), (0.21, 0.16), (0.24, 0.34), (0.255, 0.38), (0.22, 0.38), (0.20, 0.34), (0.14, 0.06), (0, 0.05)]
    arch.add_lathe_mesh("BasketBody", root, basket_prof, (0, 0, 0), mats["wood_light"], segments=20, col=col)
    base.add_box("Cloth", root, (0, 0, 0.30), (0.26, 0.26, 0.06), mats["parchment"], bevel=0.015, col=col)
    flatten_to_root(root)
    export_clean_fbx(root, "SM_Prop_BambooBasket.fbx")

    # 4. SM_Prop_Chalkboard.fbx (A-frame sidewalk price chalkboard)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    root = bpy.data.objects.new("Prop_Chalkboard", None)
    base.link_obj(root, col)
    base.add_box("PanelF", root, (0, -0.14, 0.54), (0.52, 0.03, 0.96), mats["wood_dark"], rot=(math.radians(14), 0, 0), bevel=0.008, col=col)
    base.add_box("SlateF", root, (0, -0.155, 0.58), (0.44, 0.012, 0.68), mats["stone_dark"], rot=(math.radians(14), 0, 0), col=col)
    base.add_box("PanelB", root, (0, 0.14, 0.54), (0.52, 0.03, 0.96), mats["wood_dark"], rot=(math.radians(-14), 0, 0), bevel=0.008, col=col)
    flatten_to_root(root)
    export_clean_fbx(root, "SM_Prop_Chalkboard.fbx")

    # 5. SM_Prop_PottedPlant.fbx (Turned terracotta pot with faceted organic foliage)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    root = bpy.data.objects.new("Prop_PottedPlant", None)
    base.link_obj(root, col)
    pot_prof = [(0, 0), (0.14, 0), (0.19, 0.18), (0.20, 0.32), (0.225, 0.36), (0.225, 0.39), (0.185, 0.39), (0.175, 0.30), (0, 0.28)]
    arch.add_lathe_mesh("Pot", root, pot_prof, (0, 0, 0), mats["brick_exposed"], segments=18, col=col)
    base.add_sphere("LeafA", root, (0, 0, 0.54), 0.22, mats["tree_foliage"], scale=(1.0, 1.0, 1.15), col=col)
    base.add_sphere("LeafB", root, (0.10, 0.06, 0.64), 0.15, mats["moss_clump"], col=col)
    base.add_sphere("LeafC", root, (-0.11, -0.05, 0.60), 0.14, mats["tree_foliage"], col=col)
    flatten_to_root(root)
    export_clean_fbx(root, "SM_Prop_PottedPlant.fbx")

    # 6. SM_Prop_LanternGarland.fbx (True curved catenary wire with turned Indochine silk & glowing lanterns)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    root = bpy.data.objects.new("Prop_LanternGarland", None)
    base.link_obj(root, col)
    span = 7.4
    cord_pts = []
    for k in range(13):
        u = -0.5 + k / 12.0
        lx = u * span
        sag = -0.44 * (1.0 - (u * 2.0) ** 2)
        cord_pts.append((lx, 0.0, sag))
    arch.add_bezier_scroll_mesh("CordCatenary", root, cord_pts, 0.012, (0, 0, 0), mats["iron"], resolution=6, col=col)
    for i in range(7):
        lx = -span * 0.42 + i * (span * 0.84 / 6.0)
        sag = -0.44 * (1.0 - (lx / (span * 0.5)) ** 2)
        l_mat = mats["warm_glow"] if i % 2 == 0 else mats["wax_red"]
        base.add_cyl(f"Hang_{i}", root, (lx, 0, sag - 0.10), 0.008, 0.20, mats["iron"], col=col)
        lantern_prof = [(0.0, sag - 0.48), (0.06, sag - 0.45), (0.135, sag - 0.34), (0.14, sag - 0.26), (0.075, sag - 0.16), (0.0, sag - 0.14)]
        arch.add_lathe_mesh(f"Lantern_{i}", root, lantern_prof, (lx, 0, 0), l_mat, segments=12, col=col)
        arch.add_lathe_mesh(f"Cap_{i}", root, [(0, sag - 0.14), (0.07, sag - 0.14), (0.07, sag - 0.11), (0, sag - 0.11)], (lx, 0, 0), mats["gold"], segments=12, col=col)
    flatten_to_root(root)
    export_clean_fbx(root, "SM_Prop_LanternGarland.fbx")


def export_all_models():
    rng = random.Random(20261123)

    # 1. Walk-in House 1: Shop_BlindBox_House (Modern Hanoi Shophouse)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    font = base.load_unicode_font()
    h1 = base.build_blindbox_house_unique((0, 0, 0), mats, font, rng, col)
    remove_children_by_prefix(h1, ("Display_",))
    flatten_to_root(h1)
    export_clean_fbx(h1, "SM_House_BlindBox.fbx")

    # 3. Walk-in House 2: Shop_OpenBox_House
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    font = base.load_unicode_font()
    h2 = base.build_openbox_house_unique((0, 0, 0), mats, font, rng, col)
    remove_children_by_prefix(h2, ("Art_", "ReadyBox_"))
    flatten_to_root(h2, animated_empty_prefixes=("Ext_GableGear_", "FloorPit_Gear", "Wall_Gear_", "Shelf_"))
    export_clean_fbx(h2, "SM_House_OpenBox.fbx")

    # 3B. Walk-in House 3: Shop_KimKhuy_House (Kho Tạo Tác Kim Khuy)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    font = base.load_unicode_font()
    h3 = base.build_kimkhuy_house_unique((0, 0, 0), mats, font, rng, col)
    flatten_to_root(h3)
    export_clean_fbx(h3, "SM_House_KimKhuy.fbx")

    # 3C. Walk-in House 4: Shop_AnhBay_House (Tiệm Ký Gửi & Thu Nợ Anh Bảy)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    font = base.load_unicode_font()
    h4 = base.build_anhbay_house_unique((0, 0, 0), mats, font, rng, col)
    flatten_to_root(h4)
    export_clean_fbx(h4, "SM_House_AnhBay.fbx")

    # 4. Optimize & Export All 24 Official GLB Models (Boxes + Open Variants, Items, Artifacts, Heirlooms/Bí Khế)
    glb_opt.export_all_24_glb_models()

    # 5. Blender Street Lantern, Modern Hanoi Sidewalks & Hanoi Neighborhood Assets (No People / No Vehicles)
    build_and_export_street_lantern()
    build_and_export_plaza_street_details(rng)
    export_hanoi_assets(rng)
    build_and_export_market_props_and_npcs()


def render_blindbox_verification_previews():
    """Renders high-craft Exterior (concept_1 angle) and Interior (concept_2 angle) Cycles verification shots."""
    out_dir = os.path.join(REPO_ROOT, "Docs", "Art", "Concepts")
    os.makedirs(out_dir, exist_ok=True)

    base.clear_scene()
    rng = random.Random(20261123)
    col = base.get_or_create_collection("VerifyPreview")
    mats = base.create_materials()

    # Bind baked encaustic tile texture for faithful floor preview in Blender Cycles
    enc_path = os.path.join(TEX_DIR, "T_HanoiEncausticTile_Albedo.png")
    if os.path.exists(enc_path) and mats["encaustic_tile"].use_nodes:
        nt = mats["encaustic_tile"].node_tree
        bsdf = nt.nodes.get("Principled BSDF")
        if bsdf:
            tex = nt.nodes.new("ShaderNodeTexImage")
            tex.image = bpy.data.images.load(enc_path, check_existing=True)
            uv_node = nt.nodes.new("ShaderNodeTexCoord")
            mp = nt.nodes.new("ShaderNodeMapping")
            mp.inputs["Scale"].default_value = (12.0, 14.0, 1.0)
            nt.links.new(uv_node.outputs["Generated"], mp.inputs["Vector"])
            nt.links.new(mp.outputs["Vector"], tex.inputs["Vector"])
            nt.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])

    font = base.load_unicode_font()
    base.setup_world_and_sun(col)

    # Bright Sunny Daylight Stylized Low Poly world & sun (matching reference screenshots)
    world = bpy.context.scene.world
    if world and world.use_nodes:
        bg = world.node_tree.nodes.get("Background")
        if bg:
            bg.inputs["Color"].default_value = (0.48, 0.76, 0.98, 1.0)
            bg.inputs["Strength"].default_value = 1.0
    sun = bpy.data.objects.get("Sun_GoldenHour")
    if sun and sun.data:
        sun.data.color = (1.0, 0.96, 0.88)
        sun.data.energy = 4.2
        sun.rotation_euler = Euler((math.radians(44), math.radians(-28), math.radians(-38)))

    # Foreground wet stone street & sidewalk curb context for exterior shot
    base.add_box("PreviewStreetRoad", None, (16.0, 0.0, -0.05), (22.0, 34.0, 0.10), mats["cobble"], bevel=0.0, col=col)
    base.add_box("PreviewSidewalk", None, (10.5, 0.0, 0.045), (3.2, 26.0, 0.09), mats["terrazzo_paving"], bevel=0.008, col=col)
    arch.add_organic_hanoi_tree("PreviewTree_S", (11.5, -13.5, 0.0), mats, rng, col, scale=1.02)
    arch.add_organic_hanoi_tree("PreviewTree_N", (11.5, 13.5, 0.0), mats, rng, col, scale=1.04)

    base.build_blindbox_house_unique((0, 0, 0), mats, font, rng, col)

    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 24
    scene.cycles.use_denoising = True
    scene.render.resolution_x = 1280
    scene.render.resolution_y = 720
    scene.render.image_settings.file_format = "PNG"

    ext_path = os.path.join(out_dir, "render_verify_nha_niem_phong_ext.png")
    int_path = os.path.join(out_dir, "render_verify_nha_niem_phong_int.png")
    unbox_path = os.path.join(out_dir, "render_verify_sanh_khui_hom.png")
    base.render_camera("VerifyCam_Ext", (21.5, -11.5, 4.2), (8.5, 0.5, 4.5), 28.0, ext_path)
    print("RENDERED_VERIFY:", ext_path)
    base.render_camera("VerifyCam_Int", (8.2, 0.0, 1.75), (-4.5, 0.0, 2.15), 24.0, int_path)
    print("RENDERED_VERIFY:", int_path)

    # Populate Sảnh Khui trays with the newly exported Box_H1_Open, Box_H2_Open, Box_RD_Open for direct Cycles inspection
    for b_fbx, ty in (("Box_H1_Open.fbx", -9.45), ("Box_H2_Open.fbx", -8.60), ("Box_RD_Open.fbx", -7.75)):
        fp = os.path.join(MODEL_DIR, b_fbx)
        if os.path.exists(fp):
            before = set(bpy.context.scene.objects)
            bpy.ops.import_scene.fbx(filepath=fp)
            for ob in bpy.context.scene.objects:
                if ob not in before and ob.parent is None:
                    ob.location = Vector((3.15, ty, 1.135))
                    ob.rotation_euler = Euler((ob.rotation_euler.x, ob.rotation_euler.y, ob.rotation_euler.z + math.pi / 2))

    base.render_camera("VerifyCam_Unbox", (5.3, -4.4, 2.15), (2.9, -9.2, 1.35), 24.0, unbox_path)
    print("RENDERED_VERIFY:", unbox_path)


if __name__ == "__main__":
    bake_all_textures_if_missing()
    export_all_models()
    render_blindbox_verification_previews()
    print("ALL_BLENDER_EXPORTS_DONE")

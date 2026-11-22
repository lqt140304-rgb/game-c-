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
        ("cobble", "T_Cobble", False, 2.5, 2.5, True),
        ("plaster_weathered", "T_WeatheredPlaster", True, 2.4, 3.0, True),
        ("brick_exposed", "T_ExposedBrick", True, (0.42 / 2.2) * 8, (2 * 0.17 / 2.2) * 8, True),
        ("brick_glazed_teal", "T_GlazedBrickTeal", True, (0.46 / 2.8) * 8, (2 * 0.20 / 2.8) * 8, True),
        ("brick_whitewash", "T_WhitewashedBrick", True, (0.45 / 2.4) * 8, (2 * 0.19 / 2.4) * 8, True),
        ("ashlar_stone", "T_AshlarStone", True, (0.85 / 1.25) * 4, (2 * 0.40 / 1.25) * 4, True),
        ("wood_aged_floor", "T_AgedWood", False, 2.0, 2.0, True),
        ("workshop_floor", "T_WorkshopFloor", False, 2.4, 2.4, True),
        ("roof_tile", "T_RoofTile", True, (0.38 / 2.4) * 8, (2 * 0.24 / 2.4) * 6, False),
        ("roof_slate", "T_RoofSlate", True, (0.42 / 2.4) * 8, (2 * 0.24 / 2.4) * 6, False),
    ]
    for mat_key, out_name, is_vert, pw, ph, do_norm in specs:
        a_exists = os.path.exists(os.path.join(TEX_DIR, f"{out_name}_Albedo.png"))
        n_exists = (not do_norm) or os.path.exists(os.path.join(TEX_DIR, f"{out_name}_Normal.png"))
        if a_exists and n_exists:
            continue
        bake_pbr_texture_pair(mat_key, out_name, size=256, vertical_wall=is_vert, plane_w=pw, plane_h=ph, bake_normal=do_norm)


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
                try:
                    bpy.ops.object.modifier_apply(modifier=mod.name)
                except Exception:
                    pass

        bm = bmesh.new()
        bm.from_mesh(o.data)
        uv_layer = bm.loops.layers.uv.verify()
        for face in bm.faces:
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

    root.rotation_euler = Euler((0.0, 0.0, 0.0))
    root.scale = Vector((1.0, 1.0, 1.0))
    bpy.context.view_layer.update()

    # Join named animated empty assemblies first (preserving pivot + multi-materials)
    if animated_empty_prefixes:
        # Deepest-first order so child empties join cleanly
        empties = [o for o in root.children_recursive if o.type != "MESH"]
        for e in empties:
            if any(e.name.startswith(p) for p in animated_empty_prefixes):
                join_empty_into_single_mesh(e, root)

    meshes = [o for o in root.children_recursive if o.type == "MESH"]
    empties = [o for o in root.children_recursive if o.type != "MESH"]

    world_mats = {m: m.matrix_world.copy() for m in meshes}
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
    os.makedirs(MODEL_DIR, exist_ok=True)
    objs = [root] + list(root.children_recursive)
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = root

    out_path = os.path.join(MODEL_DIR, filename)
    bpy.ops.export_scene.fbx(
        filepath=out_path,
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
    print("EXPORTED_FBX:", out_path)


def remove_children_by_prefix(root, prefixes):
    for child in list(root.children_recursive):
        if any(child.name.startswith(p) for p in prefixes):
            bpy.data.objects.remove(child, do_unlink=True)


def build_and_export_blind_box(variant, fname):
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

    export_clean_fbx(b, fname)


def build_and_export_street_lantern():
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    root = bpy.data.objects.new("StreetLantern", None)
    base.link_obj(root, col)

    base.add_cyl("StreetBase", root, (0, 0, 0.18), 0.14, 0.36, mats["stone_dark"], verts=8, col=col)
    base.add_cyl("StreetPost", root, (0, 0, 1.55), 0.055, 2.6, mats["iron"], col=col)
    base.add_cyl("StreetLantern", root, (0, 0, 2.95), 0.13, 0.24, mats["warm_glow"], verts=6, col=col)
    base.add_cone_frustum("StreetCap", root, (0, 0, 3.16), 0.20, 0.02, 0.18, mats["iron"], verts=6, smooth=False, col=col)

    flatten_to_root(root)
    export_clean_fbx(root, "SM_StreetLantern.fbx")


def build_and_export_plaza_street_details(rng):
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    root = bpy.data.objects.new("Plaza_StreetDetails", None)
    base.link_obj(root, col)

    # Raised ashlar sidewalks behind ALL curbstones (Plaza + South street + North street)
    sidewalk_slabs = [
        ("Sidewalk_Plaza_W", (-7.2, 0.0, 0.045), (5.5, 10.4, 0.09)),
        ("Sidewalk_Plaza_E", (7.2, 0.0, 0.045), (5.5, 10.4, 0.09)),
        ("Sidewalk_South_W", (-6.5, -12.0, 0.045), (6.6, 13.2, 0.09)),
        ("Sidewalk_South_E", (6.5, -12.0, 0.045), (6.6, 13.2, 0.09)),
        ("Sidewalk_North_W", (-6.5, 11.4, 0.045), (6.6, 12.2, 0.09)),
        ("Sidewalk_North_E", (6.5, 11.4, 0.045), (6.6, 12.2, 0.09)),
    ]
    for sname, sloc, ssz in sidewalk_slabs:
        base.add_box(sname, root, sloc, ssz, mats["ashlar_stone"], bevel=0.012, col=col)

    # Beveled curbstones & moss clumps framing the Plaza (X = ±4.42, Y in [-5.2, +5.2]) and South/North Main Street (X = ±3.12)
    curb_segments = [
        (-4.42, -5.0, 5.2, 1),   # West Plaza curb
        (4.42, -5.0, 5.2, -1),   # East Plaza curb
        (-3.12, -18.5, -5.4, 1), # West MainStreet_S curb
        (3.12, -18.5, -5.4, -1), # East MainStreet_S curb
        (-3.12, 5.4, 17.5, 1),   # West MainStreet_N curb
        (3.12, 5.4, 17.5, -1),   # East MainStreet_N curb
    ]
    c_idx = 0
    for cx, y_min, y_max, moss_dir in curb_segments:
        y = y_min
        while y <= y_max:
            m = mats["stone_dark"] if c_idx % 4 != 0 else mats["wet_stone"]
            base.add_box(f"Curb_{c_idx}", root,
                         (cx + rng.uniform(-0.01, 0.01), y, 0.065 + rng.uniform(-0.005, 0.005)),
                         (0.24, 0.68, 0.14), m,
                         rot=(rng.uniform(-0.02, 0.02), 0, rng.uniform(-0.02, 0.02)), bevel=0.015, col=col)
            if c_idx % 3 == 0:
                base.add_moss_cluster(f"CurbMoss_{c_idx}", root, (cx + moss_dir * 0.15, y, 0.02), (0.6, 0.6, 0.5), mats, rng, col)
            y += 0.72
            c_idx += 1

    # Scattered irregular 3D stepping flagstones along Plaza & Main Street
    for i in range(44):
        py = rng.uniform(-17.0, 15.0)
        x_lim = 3.8 if -5.0 <= py <= 5.0 else 2.5
        px = rng.uniform(-x_lim, x_lim)
        if math.hypot(px, py) < 3.95:
            continue
        w = rng.uniform(0.38, 0.72)
        d = rng.uniform(0.32, 0.62)
        m = mats["ashlar_stone"] if i % 3 != 0 else mats["wet_stone"]
        base.add_box(f"RoadSlab_{i}", root,
                     (px, py, rng.uniform(0.006, 0.020)), (w, d, 0.036), m,
                     rot=(rng.uniform(-0.022, 0.022), rng.uniform(-0.022, 0.022), rng.uniform(0, math.pi)),
                     bevel=0.010, col=col)

    flatten_to_root(root)
    export_clean_fbx(root, "SM_Plaza_StreetDetails.fbx")


def export_all_models():
    rng = random.Random(20261123)

    # 1. Central Fountain + Plaza Compass Mosaic (remove static WaterJet sticks — Unity renders live flowing water!)
    base.clear_scene()
    col = base.get_or_create_collection("Export")
    mats = base.create_materials()
    fountain = base.build_fountain(mats, rng, col)
    remove_children_by_prefix(fountain, ("WaterJet_",))
    base.add_cyl("Plaza_WetApron", fountain, (0, 0, 0.012), 2.55, 0.024, mats["wet_stone"], verts=24, bevel=0.006, smooth=False, col=col)
    base.add_torus_ring("Plaza_RuneRing_Brass", fountain, (0, 0, 0.022), 2.82, 0.045, mats["brass"], major_segs=48, col=col)
    base.add_torus_ring("Plaza_RuneRing_Glow", fountain, (0, 0, 0.020), 2.94, 0.025, mats["arcane_glow"], major_segs=48, col=col)
    for i in range(16):
        ang = i * (2 * math.pi / 16)
        r = 3.45
        sx, sy = math.cos(ang) * r, math.sin(ang) * r
        m = mats["ashlar_stone"] if i % 2 == 0 else mats["stone_dark"]
        tilt = rng.uniform(-0.018, 0.018)
        base.add_box(f"PlazaMosaic_{i}", fountain, (sx, sy, rng.uniform(0.008, 0.020)), (0.85, 0.56, 0.04), m, rot=(tilt, tilt * 0.5, ang), bevel=0.012, col=col)
    flatten_to_root(fountain)
    # Join each LevitatingRuneStone_k + LevitatingRuneGlow_k into a centered LevitatingRune_k mesh
    for k in range(4):
        stone = next((o for o in fountain.children if o.name == f"LevitatingRuneStone_{k}"), None)
        glow = next((o for o in fountain.children if o.name == f"LevitatingRuneGlow_{k}"), None)
        if stone and glow:
            bpy.ops.object.select_all(action="DESELECT")
            stone.select_set(True)
            glow.select_set(True)
            bpy.context.view_layer.objects.active = stone
            bpy.ops.object.join()
            stone.name = f"LevitatingRune_{k}"
            bpy.ops.object.origin_set(type="ORIGIN_GEOMETRY", center="MEDIAN")
    export_clean_fbx(fountain, "SM_Fountain_Central.fbx")

    # 2. Walk-in House 1: Shop_BlindBox_House
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

    # 4. Four Blind Boxes (joined single-mesh LidPivot at back hinge)
    for variant, fname in [
        ("WoodCrate_Old", "SM_Box_WoodCrate.fbx"),
        ("BrassChest_Steam", "SM_Box_BrassChest.fbx"),
        ("RunicBox_Arcane", "SM_Box_RunicArcane.fbx"),
        ("HoloPod_SciFi", "SM_Box_HoloSciFi.fbx"),
    ]:
        build_and_export_blind_box(variant, fname)

    # 5. Ten Detailed Artifacts
    for key, fname in [
        ("PocketWatch_Brass", "SM_Art_PocketWatch.fbx"),
        ("Chalice_Royal", "SM_Art_Chalice.fbx"),
        ("HoloCube_SciFi", "SM_Art_HoloCube.fbx"),
        ("RunicMask_Arcane", "SM_Art_RunicMask.fbx"),
        ("FossilAmber_Ancient", "SM_Art_AmberFossil.fbx"),
        ("Compass_Starlight", "SM_Art_StarlightCompass.fbx"),
        ("Amulet_Jade", "SM_Art_JadeAmulet.fbx"),
        ("GearCore_Steam", "SM_Art_GearCore.fbx"),
        ("AutomatonBird_Brass", "SM_Art_AutomatonBird.fbx"),
        ("VoidPrism_Alien", "SM_Art_VoidPrism.fbx"),
    ]:
        base.clear_scene()
        col = base.get_or_create_collection("Export")
        mats = base.create_materials()
        art = build_extra_artifacts(f"Art_{key}", key, mats, col)
        art.rotation_euler = Euler((0, 0, math.pi / 2))
        flatten_to_root(art)
        export_clean_fbx(art, fname)

    # 6. Blender Street Lantern & Sidewalks/Curbs/Flagstones
    build_and_export_street_lantern()
    build_and_export_plaza_street_details(rng)


if __name__ == "__main__":
    bake_all_textures_if_missing()
    export_all_models()
    print("ALL_BLENDER_EXPORTS_DONE")

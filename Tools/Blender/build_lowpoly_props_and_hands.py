"""
Stylized Low Poly 3D Prop, Hands, Blind Box & Artifact Exporter for Chợ Đồ Lạ Vertical Slice.
Generates 100% cohesive Flat-Shaded Low Poly models matching build_lowpoly_chodola_map.py:
  - LP_FP_CarryHands.fbx  (Sculpted two-hand low-poly arms cradling box sides with sleeves, cuffs, palms, thumbs & 4 fingers)
  - LP_Box_H1.fbx, LP_Box_H2.fbx, LP_Box_H3.fbx, LP_Box_RD.fbx
      (Each uses 1-level direct MESH children: BoxBody + LidPivot at rear hinge (0, +0.135, 0.185) + SealPart at front latch)
  - LP_Item_V01.fbx .. LP_Item_V10.fbx (10 flat-shaded hero artifacts matching Sprint1Catalog.cs 1:1)
"""

import math
import os
import sys

import bpy
from mathutils import Vector

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
if SCRIPT_DIR not in sys.path:
    sys.path.insert(0, SCRIPT_DIR)

import build_lowpoly_chodola_map as lp


def _extra_prop_materials(mats):
    mats = dict(mats)
    mats.update({
        "cyan_glow": lp.make_lp_mat("LP_CyanGlow", (0.28, 0.84, 0.96), roughness=0.25, emission=(0.24, 0.84, 0.96), emission_strength=0.95),
        "arcane_glow": lp.make_lp_mat("LP_ArcaneGlow", (0.58, 0.34, 0.88), roughness=0.28, emission=(0.56, 0.30, 0.88), emission_strength=0.85),
        "obsidian": lp.make_lp_mat("LP_Obsidian", (0.14, 0.13, 0.20), roughness=0.35, metallic=0.22),
        "porcelain_jade": lp.make_lp_mat("LP_PorcelainJade", (0.48, 0.72, 0.62), roughness=0.32),
        "cinnabar": lp.make_lp_mat("LP_CinnabarRed", (0.76, 0.18, 0.14), roughness=0.65),
        "copper": lp.make_lp_mat("LP_CopperTrim", (0.74, 0.42, 0.22), roughness=0.42, metallic=0.58),
    })
    return mats


def _join_container_to_mesh(container_empty, target_root, out_name):
    """Joins all MESH descendants inside container_empty into ONE multi-material Mesh child preserving container's world origin."""
    bpy.context.view_layer.update()
    meshes = [c for c in container_empty.children_recursive if c.type == "MESH"]
    if not meshes:
        return None
    pivot_world = container_empty.matrix_world.to_translation().copy()

    bpy.ops.object.select_all(action="DESELECT")
    for m in meshes:
        bpy.context.view_layer.objects.active = m
        m.select_set(True)
        bpy.ops.object.parent_clear(type="CLEAR_KEEP_TRANSFORM")
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)

    bpy.ops.object.select_all(action="DESELECT")
    for m in meshes:
        m.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    if len(meshes) > 1:
        bpy.ops.object.join()

    joined = bpy.context.view_layer.objects.active
    delta = joined.location - pivot_world
    if delta.length > 1e-6:
        for v in joined.data.vertices:
            v.co += delta
        joined.location = pivot_world

    joined.parent = target_root
    joined.matrix_parent_inverse = target_root.matrix_world.inverted()
    joined.location = pivot_world
    joined.rotation_euler = (0.0, 0.0, 0.0)
    joined.scale = (1.0, 1.0, 1.0)
    joined.name = out_name
    for poly in joined.data.polygons:
        poly.use_smooth = False

    bpy.data.objects.remove(container_empty, do_unlink=True)
    return joined


def _new_group(name, loc=(0.0, 0.0, 0.0), col=None):
    g = bpy.data.objects.new(name, None)
    g.location = Vector(loc)
    lp.link_obj(g, col)
    return g


# ==================== 1. SCULPTED STYLIZED LOW-POLY CARRY HANDS ====================

def build_carry_hands(mats, col):
    """
    Two sculpted low-poly stylized hands cradling the carried blind box centered at (0,0,0).
    In Unity (axis_forward=Z, axis_up=Y): Blender -Y = toward player camera, +Y = forward away from camera.
    """
    root = _new_group("LP_FP_CarryHands", col=col)
    grp = _new_group("HandsGroup", col=col)

    skin = mats["carry_skin"]
    cuff = mats["pure_white"]
    sleeve = mats["teal_wainscot"]

    for side in (-1.0, 1.0):
        # Upper forearm + rolled teal craftsman sleeve extending toward camera (-Y)
        arm_start = (side * 0.235, -0.44, -0.115)
        arm_mid = (side * 0.175, -0.22, -0.052)
        cuff_pos = (side * 0.148, -0.145, -0.028)
        palm_pos = (side * 0.128, -0.015, 0.004)

        lp.add_limb(f"Sleeve_{side}", grp, arm_start, arm_mid, 0.044, 0.052, sleeve, segments=6, col=col)
        lp.add_limb(f"Cuff_{side}", grp, arm_mid, cuff_pos, 0.055, 0.052, cuff, segments=6, col=col)
        lp.add_limb(f"Wrist_{side}", grp, cuff_pos, (side * 0.132, -0.075, -0.008), 0.036, 0.038, skin, segments=6, col=col)

        # Faceted sculpted palm pressed cleanly along outer left/right box wall (|x|=0.126, clear of box |x|=0.112)
        lp.add_box(f"Palm_{side}", grp, palm_pos, (0.032, 0.118, 0.074), skin, rot=(0.12, -side * 0.08, side * 0.06), col=col)
        lp.add_box(f"UnderCradle_{side}", grp, (side * 0.095, -0.015, -0.042), (0.065, 0.092, 0.024), skin, rot=(0.06, side * 0.25, 0.0), col=col)

        # 4 distinct low-poly fingers wrapping around front/lower side corners of the crate!
        for fi in range(4):
            fz = 0.028 - fi * 0.019
            fx_prox = side * (0.124 - fi * 0.002)
            fy_prox = 0.058 + (0.006 if fi in (1, 2) else 0.0)
            lp.add_box(f"FinProx_{side}_{fi}", grp, (fx_prox, fy_prox, fz), (0.022, 0.042, 0.016), skin, rot=(0.06, 0.0, side * 0.22), col=col)
            # Inward distal fingertips gripping front corners without penetrating box center
            lp.add_box(f"FinTip_{side}_{fi}", grp, (side * 0.106, fy_prox + 0.032, fz - 0.002), (0.022, 0.026, 0.015), skin, rot=(0.0, 0.0, side * 0.72), col=col)

        # Sculpted opposable thumb pressing atop the upper side rim!
        lp.add_box(f"ThumbProx_{side}", grp, (side * 0.118, -0.018, 0.048), (0.024, 0.052, 0.022), skin, rot=(-0.22, -side * 0.35, -side * 0.15), col=col)
        lp.add_box(f"ThumbTip_{side}", grp, (side * 0.098, 0.012, 0.066), (0.024, 0.036, 0.018), skin, rot=(-0.10, -side * 0.65, -side * 0.35), col=col)

    _join_container_to_mesh(grp, root, "CarryHands_Mesh")
    return root


# ==================== 2. FOUR FLAT-SHADED BLIND BOXES (WITH WORKING HINGE PIVOTS) ====================

def build_lp_blind_box(variant, mats, col):
    """
    Cohesive Flat-Shaded Low Poly Blind Box.
    Outer base size ~0.36 x 0.28 x 0.19m (min_z = 0.0).
    Direct 1-level Mesh children: BoxBody, LidPivot (hinge at rear +0.135, 0.185), SealPart (front latch -0.145, 0.175).
    """
    root = _new_group(f"LP_Box_{variant}", col=col)

    if variant == "H1":
        body_m, trim_m, band_m, seal_m = mats["honey_wood"], mats["wood_dark"], mats["crimson_plank"], mats["wax_red"]
        brass_m = mats["brass_gold"]
    elif variant == "H2":
        body_m, trim_m, band_m, seal_m = mats["slate_floor"], mats["charcoal"], mats["cyan_glow"], mats["inox"]
        brass_m = mats["brass_gold"]
    elif variant == "H3":
        body_m, trim_m, band_m, seal_m = mats["walnut_dark"], mats["jade_glass"], mats["brass_gold"], mats["amber_glass"]
        brass_m = mats["brass_gold"]
    else:  # RD
        body_m, trim_m, band_m, seal_m = mats["box_purple"], mats["brass_gold"], mats["crimson_velvet"], mats["wax_red"]
        brass_m = mats["brass_gold"]

    # --- Stationary BoxBody ---
    b_grp = _new_group("BoxBodyGrp", (0.0, 0.0, 0.0), col=col)
    lp.add_box("BodyCore", b_grp, (0.0, 0.0, 0.092), (0.34, 0.26, 0.165), body_m, col=col)
    lp.add_box("BasePlinth", b_grp, (0.0, 0.0, 0.016), (0.365, 0.285, 0.032), trim_m, col=col)
    lp.add_box("RimBand", b_grp, (0.0, 0.0, 0.172), (0.358, 0.278, 0.022), trim_m, col=col)
    # 4 corner timber/metal brackets + decorative side planks
    for sx in (-0.166, 0.166):
        for sy in (-0.126, 0.126):
            lp.add_box(f"Corner_{sx:.2f}_{sy:.2f}", b_grp, (sx, sy, 0.094), (0.032, 0.032, 0.178), trim_m, col=col)
    for sy_b in (-0.132, 0.132):
        lp.add_box(f"FrontBackStrap_{sy_b:.2f}", b_grp, (0.0, sy_b, 0.096), (0.28, 0.012, 0.042), band_m, col=col)
    for sx_b in (-0.172, 0.172):
        lp.add_box(f"SideStrap_{sx_b:.2f}", b_grp, (sx_b, 0.0, 0.096), (0.012, 0.20, 0.042), band_m, col=col)
    # Rear brass hinges at Y=+0.136, Z=0.185
    for hx in (-0.10, 0.10):
        lp.add_cyl(f"Hinge_{hx:.2f}", b_grp, (hx, 0.136, 0.185), 0.014, 0.014, 0.045, brass_m, verts=6, rot=(0, math.pi * 0.5, 0), col=col)
    # Recessed velvet bed rim visible when lid swings open
    lp.add_box("InnerVelvet", b_grp, (0.0, 0.0, 0.176), (0.28, 0.20, 0.012), mats["crimson_velvet"], col=col)
    _join_container_to_mesh(b_grp, root, "BoxBody")

    # --- Hinged LidPivot (origin at rear hinge (0.0, +0.135, 0.185)) ---
    hinge = (0.0, 0.135, 0.185)
    l_grp = _new_group("LidGrp", hinge, col=col)
    lp.add_box("LidSlab", l_grp, (0.0, -0.135, 0.022), (0.362, 0.282, 0.044), body_m, col=col)
    lp.add_box("LidBevelTop", l_grp, (0.0, -0.135, 0.048), (0.320, 0.240, 0.016), trim_m, col=col)
    lp.add_box("LidStrapCenter", l_grp, (0.0, -0.135, 0.056), (0.075, 0.255, 0.012), band_m, col=col)
    if variant in ("H3", "RD"):
        lp.add_box("LidCrest", l_grp, (0.0, -0.135, 0.062), (0.078, 0.078, 0.012), brass_m, rot=(0, 0, math.pi * 0.25), col=col)
    else:
        lp.add_box("LidPlaque", l_grp, (0.0, -0.135, 0.062), (0.11, 0.06, 0.012), brass_m, col=col)
    _join_container_to_mesh(l_grp, root, "LidPivot")

    # --- Front SealPart (origin at front latch (0.0, -0.146, 0.175)) ---
    s_grp = _new_group("SealGrp", (0.0, -0.146, 0.175), col=col)
    lp.add_box("ClaspBar", s_grp, (0.0, 0.004, 0.0), (0.068, 0.016, 0.052), brass_m, col=col)
    lp.add_cyl("WaxSealDisc", s_grp, (0.0, -0.006, 0.0), 0.030, 0.026, 0.016, seal_m, verts=8, rot=(math.pi * 0.5, 0, 0), col=col)
    _join_container_to_mesh(s_grp, root, "SealPart")
    return root


# ==================== 3. TEN FLAT-SHADED HERO ARTIFACTS (V01..V10) ====================

def build_lp_artifact(item_id, mats, col):
    """Distinct, sculpted Stylized Low Poly silhouette for each catalog item (~0.17-0.22m, min_z = 0.0)."""
    root = _new_group(f"LP_Item_{item_id}", col=col)
    grp = _new_group("ArtGrp", col=col)

    brass = mats["brass_gold"]
    wood = mats["honey_wood"]
    walnut = mats["walnut_dark"]
    cream = mats["parchment"]
    slate = mats["slate_floor"]
    cyan = mats["cyan_glow"]
    amber = mats["amber_glass"]
    jade = mats["porcelain_jade"]
    wax = mats["wax_red"]

    if item_id == "V01":  # Bình sứ men xanh
        lp.add_cyl("Foot", grp, (0, 0, 0.012), 0.045, 0.048, 0.024, brass, verts=8, col=col)
        lp.add_cyl("LowerBelly", grp, (0, 0, 0.056), 0.045, 0.078, 0.064, jade, verts=8, col=col)
        lp.add_cyl("UpperShoulder", grp, (0, 0, 0.114), 0.078, 0.036, 0.052, jade, verts=8, col=col)
        lp.add_cyl("Neck", grp, (0, 0, 0.156), 0.034, 0.046, 0.036, jade, verts=8, col=col)
        lp.add_cyl("LipBrass", grp, (0, 0, 0.178), 0.049, 0.049, 0.012, brass, verts=8, col=col)

    elif item_id == "V02":  # Sách cổ bìa da
        lp.add_box("BackCover", grp, (0, 0.018, 0.095), (0.145, 0.016, 0.190), walnut, col=col)
        lp.add_box("Pages", grp, (0.005, 0.0, 0.095), (0.128, 0.028, 0.174), cream, col=col)
        lp.add_box("FrontCover", grp, (0, -0.018, 0.095), (0.145, 0.016, 0.190), mats["crimson_plank"], col=col)
        lp.add_box("Spine", grp, (-0.068, 0.0, 0.095), (0.022, 0.052, 0.192), walnut, col=col)
        for rz in (0.035, 0.095, 0.155):
            lp.add_box(f"SpineRib_{rz:.2f}", grp, (-0.074, 0.0, rz), (0.014, 0.056, 0.014), brass, col=col)
        lp.add_box("Medallion", grp, (0.01, -0.028, 0.095), (0.052, 0.008, 0.052), brass, rot=(0, math.pi * 0.25, 0), col=col)

    elif item_id == "V03":  # Đồng hồ quả lắc cũ
        lp.add_box("Base", grp, (0, 0, 0.016), (0.135, 0.088, 0.032), walnut, col=col)
        lp.add_box("Case", grp, (0, 0, 0.105), (0.110, 0.068, 0.148), wood, col=col)
        lp.add_box("GableCap", grp, (0, 0, 0.190), (0.126, 0.080, 0.026), walnut, col=col)
        lp.add_cyl("Bezel", grp, (0, -0.035, 0.135), 0.038, 0.038, 0.014, brass, verts=8, rot=(math.pi * 0.5, 0, 0), col=col)
        lp.add_cyl("Dial", grp, (0, -0.041, 0.135), 0.030, 0.030, 0.008, cream, verts=8, rot=(math.pi * 0.5, 0, 0), col=col)
        lp.add_box("HourHand", grp, (-0.006, -0.046, 0.141), (0.005, 0.004, 0.018), mats["charcoal"], rot=(0, 0.55, 0), col=col)
        lp.add_box("MinHand", grp, (0.008, -0.047, 0.144), (0.004, 0.004, 0.024), mats["charcoal"], rot=(0, -0.45, 0), col=col)
        lp.add_cyl("PendRod", grp, (0, -0.036, 0.068), 0.004, 0.004, 0.052, brass, verts=4, col=col)
        lp.add_cyl("PendBob", grp, (0, -0.038, 0.044), 0.016, 0.016, 0.010, brass, verts=8, rot=(math.pi * 0.5, 0, 0), col=col)

    elif item_id == "V04":  # Mảnh kính đồng hồ thiên văn
        lp.add_cyl("Pedestal", grp, (0, 0, 0.014), 0.058, 0.046, 0.028, walnut, verts=8, col=col)
        lp.add_cyl("Stem", grp, (0, 0, 0.042), 0.012, 0.012, 0.036, brass, verts=6, col=col)
        lp.add_cyl("OuterRing", grp, (0, 0, 0.115), 0.078, 0.078, 0.020, brass, verts=12, rot=(math.pi * 0.5, 0, 0), col=col)
        lp.add_cyl("SkyGlass", grp, (0, 0, 0.115), 0.062, 0.062, 0.012, mats["glass_sky"], verts=12, rot=(math.pi * 0.5, 0, 0), col=col)
        lp.add_box("CrossArcA", grp, (0, 0, 0.115), (0.145, 0.016, 0.012), brass, rot=(0, 0.45, 0), col=col)
        lp.add_cyl("Finial", grp, (0, 0, 0.200), 0.016, 0.006, 0.022, brass, verts=6, col=col)

    elif item_id == "V05":  # Thiết bị liên lạc hỏng
        lp.add_box("Chassis", grp, (0, 0, 0.080), (0.110, 0.064, 0.155), slate, col=col)
        lp.add_box("ScreenBezel", grp, (0, -0.032, 0.108), (0.086, 0.012, 0.064), mats["charcoal"], col=col)
        lp.add_box("ScreenGlow", grp, (0, -0.036, 0.108), (0.072, 0.008, 0.050), cyan, col=col)
        lp.add_box("Crack", grp, (0, -0.041, 0.108), (0.062, 0.006, 0.007), mats["charcoal"], rot=(0, 0.42, 0), col=col)
        for dx in (-0.026, 0.026):
            lp.add_cyl(f"Dial_{dx:.2f}", grp, (dx, -0.036, 0.044), 0.014, 0.014, 0.014, brass, verts=6, rot=(math.pi * 0.5, 0, 0), col=col)
        lp.add_limb("AntLow", grp, (0.036, 0, 0.155), (0.036, 0, 0.195), 0.008, 0.007, mats["inox"], segments=6, col=col)
        lp.add_limb("AntBent", grp, (0.036, 0, 0.195), (0.062, -0.018, 0.228), 0.007, 0.004, mats["inox"], segments=6, col=col)

    elif item_id == "V06":  # Bộ pin lượng tử
        lp.add_cyl("CapBot", grp, (0, 0, 0.016), 0.062, 0.062, 0.032, mats["inox"], verts=6, col=col)
        lp.add_cyl("CapTop", grp, (0, 0, 0.172), 0.062, 0.062, 0.032, mats["inox"], verts=6, col=col)
        lp.add_cyl("CoreCyan", grp, (0, 0, 0.094), 0.038, 0.038, 0.130, cyan, verts=6, col=col)
        lp.add_cyl("CoilMid", grp, (0, 0, 0.094), 0.050, 0.050, 0.022, mats["copper"], verts=6, col=col)
        for ci in range(3):
            ang = ci * (2.0 * math.pi / 3.0)
            lp.add_box(f"Strut_{ci}", grp, (math.cos(ang) * 0.054, math.sin(ang) * 0.054, 0.094), (0.014, 0.014, 0.135), brass, rot=(0, 0, ang), col=col)

    elif item_id == "V07":  # Tinh thể lam cầm tay
        lp.add_ico("BaseRock", grp, (0, 0, 0.032), (0.072, 0.066, 0.034), slate, subdivisions=1, col=col)
        crystals = (
            ((0.0, 0.0, 0.125), 0.032, 0.155, (0.08, -0.05, 0.2), cyan),
            ((-0.036, 0.018, 0.098), 0.022, 0.105, (0.22, 0.28, 0.4), mats["arcane_glow"]),
            ((0.034, -0.020, 0.102), 0.024, 0.115, (-0.18, -0.24, -0.3), cyan),
            ((0.012, 0.036, 0.088), 0.018, 0.088, (-0.28, 0.12, 0.1), mats["arcane_glow"]),
        )
        for idx, (loc, rad, h, rot, mat) in enumerate(crystals):
            lp.add_cyl(f"Shard_{idx}", grp, loc, rad, 0.004, h, mat, verts=6, rot=rot, col=col)

    elif item_id == "V08":  # Gương thủy tinh cổ tích
        lp.add_box("Pommel", grp, (0, 0, 0.014), (0.024, 0.020, 0.028), brass, col=col)
        lp.add_cyl("Handle", grp, (0, 0, 0.055), 0.014, 0.018, 0.068, wood, verts=6, col=col)
        lp.add_cyl("FrameRim", grp, (0, 0, 0.145), 0.066, 0.066, 0.018, brass, verts=10, rot=(math.pi * 0.5, 0, 0), col=col)
        lp.add_cyl("MirrorGlass", grp, (0, -0.006, 0.145), 0.052, 0.052, 0.010, mats["glass_sky"], verts=10, rot=(math.pi * 0.5, 0, 0), col=col)
        lp.add_box("GemTop", grp, (0, -0.010, 0.214), (0.020, 0.012, 0.020), wax, rot=(0, math.pi * 0.25, 0), col=col)

    elif item_id == "V09":  # Lá bùa giấy vàng
        lp.add_box("ScrollPaper", grp, (0, 0, 0.100), (0.095, 0.014, 0.175), mats["book_ochre"], rot=(0.06, 0, 0), col=col)
        lp.add_cyl("TopRod", grp, (0, -0.005, 0.192), 0.014, 0.014, 0.128, walnut, verts=6, rot=(0, math.pi * 0.5, 0), col=col)
        lp.add_cyl("BotRod", grp, (0, 0.005, 0.012), 0.014, 0.014, 0.128, walnut, verts=6, rot=(0, math.pi * 0.5, 0), col=col)
        for rzi in (0.065, 0.105, 0.145):
            lp.add_box(f"Rune_{rzi:.2f}", grp, (0, -0.009, rzi), (0.048, 0.008, 0.016), mats["cinnabar"], col=col)
        lp.add_box("RuneVert", grp, (0, -0.009, 0.105), (0.012, 0.008, 0.095), mats["cinnabar"], col=col)

    else:  # V10: Mảnh vảy long
        lp.add_cyl("MountBase", grp, (0, 0, 0.012), 0.054, 0.042, 0.024, slate, verts=6, col=col)
        for li, (w, d, h, z, m) in enumerate((
            (0.115, 0.026, 0.145, 0.095, mats["emerald_shutter"]),
            (0.088, 0.032, 0.110, 0.120, mats["jade_glass"]),
            (0.055, 0.036, 0.072, 0.148, brass),
        )):
            lp.add_box(f"ScaleLayer_{li}", grp, (0, 0, z), (w, d, h), m, rot=(0.08, 0, 0.0), col=col)

    _join_container_to_mesh(grp, root, "ArtifactMesh")
    return root


def export_all_props_and_hands():
    lp.clear_scene()
    col = lp.get_or_create_collection("Export")
    mats = _extra_prop_materials(lp.create_lp_materials())

    root = build_carry_hands(mats, col)
    lp.export_fbx(root, "LP_FP_CarryHands.fbx")

    for var in ("H1", "H2", "H3", "RD"):
        lp.clear_scene()
        col = lp.get_or_create_collection("Export")
        mats = _extra_prop_materials(lp.create_lp_materials())
        box_root = build_lp_blind_box(var, mats, col)
        lp.export_fbx(box_root, f"LP_Box_{var}.fbx")

    for idx in range(1, 11):
        vid = f"V{idx:02d}"
        lp.clear_scene()
        col = lp.get_or_create_collection("Export")
        mats = _extra_prop_materials(lp.create_lp_materials())
        art_root = build_lp_artifact(vid, mats, col)
        lp.export_fbx(art_root, f"LP_Item_{vid}.fbx")


if __name__ == "__main__":
    export_all_props_and_hands()

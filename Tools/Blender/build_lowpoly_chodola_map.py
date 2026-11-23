"""
Stylized Low Poly 3D Masterpiece Map Builder for Chợ Đồ Lạ (Flat Shading + soft sunny light / cool blue shadows).
Matches concept_lowpoly_map_overview_birdseye_1795435659758.jpg & concept_lowpoly_interior_1795435641485.jpg,
plus a completely distinct Clockwork & Brass Appraisal Atelier inside Tiệm Mở Hòm:
  - LP_Shop_NhaNiemPhong.fbx   (Warm Honey-Wood Mezzanine Hall, A-frame roof trusses, honeycomb floor tiles, 6 paneled pedestals & 18-cubby Dì Mận wall)
  - LP_Shop_TiemMoHom.fbx      (Deep Teal & Brass Clockwork Atelier: slate/brass floor, overhead skylight & gear orrery, tool pegboard, alchemy shelves, Clockwork Workbench with loupe arm & banker lamp)
  - LP_Shop_AnhBay.fbx         (Corner Mint Shophouse with 4-sided Hip Roof / mái bánh ú, awnings & vault pawn interior)
  - LP_Shop_KimKhuy.fbx        (Indigo & 2-tier stepped yellow-terracotta shop with forge anvil & glowing crystal crucible)
  - LP_Stall_Pho.fbx           (Compact Phở sidewalk stall + plastic stools)
  - LP_Stall_TraDa.fbx         (Compact Trà Đá sidewalk stall + plastic stools)
  - LP_StreetTree.fbx          (Segmented 6-sided brown trunk, faceted canopy, hex planter with rocks & spiky grass)
  - LP_StreetPlaza.fbx         (Crossroads with diagonal rocky islands, gentle leftward curving cobblestone lane, lantern garlands, staggered roofline & clouds)
"""

import math
import os
import random
import shutil
import sys
import tempfile
import time

import bmesh
import bpy
from mathutils import Euler, Vector

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
REPO_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, "..", ".."))
MODEL_DIR = os.path.join(REPO_ROOT, "Assets", "_Project", "Resources", "Blender", "Models")
CONCEPT_DIR = os.path.join(REPO_ROOT, "Docs", "Art", "Concepts")

FONT_PATHS = [
    r"C:\Windows\Fonts\segoeuib.ttf",
    r"C:\Windows\Fonts\arialbd.ttf",
    r"C:\Windows\Fonts\tahomabd.ttf",
]


def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def get_or_create_collection(name):
    if name in bpy.data.collections:
        return bpy.data.collections[name]
    col = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(col)
    return col


def link_obj(obj, col):
    for c in list(obj.users_collection):
        c.objects.unlink(obj)
    (col or bpy.context.scene.collection).objects.link(obj)


def load_unicode_font():
    for fp in FONT_PATHS:
        if os.path.exists(fp):
            try:
                return bpy.data.fonts.load(fp)
            except Exception:
                pass
    return None


def make_lp_mat(name, rgb, roughness=0.86, metallic=0.0, emission=None, emission_strength=0.0, alpha=1.0, transmission=0.0):
    if name in bpy.data.materials:
        return bpy.data.materials[name]
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (*rgb, 1.0)
        bsdf.inputs["Roughness"].default_value = roughness
        bsdf.inputs["Metallic"].default_value = metallic
        if "Alpha" in bsdf.inputs:
            bsdf.inputs["Alpha"].default_value = alpha
        if "Transmission Weight" in bsdf.inputs:
            bsdf.inputs["Transmission Weight"].default_value = transmission
        if "Specular IOR Level" in bsdf.inputs:
            bsdf.inputs["Specular IOR Level"].default_value = 0.22 if metallic == 0.0 else 0.50
        if emission is not None and "Emission Color" in bsdf.inputs:
            bsdf.inputs["Emission Color"].default_value = (*emission, 1.0)
            bsdf.inputs["Emission Strength"].default_value = emission_strength
    return mat


def create_lp_materials():
    """Grounded 60-30-10 Stylized Low Poly palette with warm/cool harmony & zero neon/chalk clip."""
    return {
        "cobble_light": make_lp_mat("LP_CobbleLight", (0.40, 0.43, 0.47), roughness=0.88),
        "cobble_dark": make_lp_mat("LP_CobbleDark", (0.30, 0.33, 0.38), roughness=0.90),
        "sidewalk_cream": make_lp_mat("LP_SidewalkCream", (0.66, 0.61, 0.52), roughness=0.86),
        "curb_stone": make_lp_mat("LP_CurbStone", (0.54, 0.50, 0.43), roughness=0.88),
        "ground_skirt": make_lp_mat("LP_GroundSkirt", (0.26, 0.44, 0.20), roughness=0.90),
        "crimson_plank": make_lp_mat("LP_CrimsonPlank", (0.58, 0.14, 0.14), roughness=0.80),
        "crimson_plank_b": make_lp_mat("LP_CrimsonPlankB", (0.49, 0.11, 0.12), roughness=0.82),
        "crimson_dark": make_lp_mat("LP_CrimsonDark", (0.36, 0.08, 0.09), roughness=0.84),
        "sunny_yellow": make_lp_mat("LP_SunnyYellow", (0.76, 0.58, 0.24), roughness=0.82),
        "pure_white": make_lp_mat("LP_PureWhite", (0.84, 0.81, 0.74), roughness=0.78),
        "emerald_shutter": make_lp_mat("LP_EmeraldShutter", (0.15, 0.44, 0.28), roughness=0.78),
        "terracotta_roof": make_lp_mat("LP_TerracottaRoof", (0.64, 0.30, 0.19), roughness=0.78),
        "terracotta_dark": make_lp_mat("LP_TerracottaDark", (0.48, 0.21, 0.14), roughness=0.80),
        "teal_shop": make_lp_mat("LP_TealShop", (0.17, 0.46, 0.44), roughness=0.80),
        "indigo_shop": make_lp_mat("LP_IndigoShop", (0.24, 0.27, 0.48), roughness=0.82),
        "mint_shop": make_lp_mat("LP_MintShop", (0.52, 0.68, 0.60), roughness=0.82),
        "honey_wood": make_lp_mat("LP_HoneyWood", (0.58, 0.38, 0.21), roughness=0.78),
        "timber_light": make_lp_mat("LP_TimberLight", (0.68, 0.49, 0.30), roughness=0.80),
        "oak_plank_a": make_lp_mat("LP_OakPlankA", (0.56, 0.37, 0.21), roughness=0.82),
        "oak_plank_b": make_lp_mat("LP_OakPlankB", (0.48, 0.31, 0.17), roughness=0.84),
        "wood_dark": make_lp_mat("LP_WarmWoodDark", (0.33, 0.20, 0.11), roughness=0.80),
        "walnut_dark": make_lp_mat("LP_WalnutDark", (0.21, 0.13, 0.08), roughness=0.82),
        "warm_cream": make_lp_mat("LP_WarmCream", (0.78, 0.72, 0.61), roughness=0.86),
        "floor_ochre": make_lp_mat("LP_FloorOchre", (0.72, 0.54, 0.25), roughness=0.80),
        "slate_floor": make_lp_mat("LP_SlateFloor", (0.25, 0.28, 0.33), roughness=0.84),
        "slate_tile_alt": make_lp_mat("LP_SlateTileAlt", (0.33, 0.36, 0.42), roughness=0.82),
        "teal_wainscot": make_lp_mat("LP_TealWainscot", (0.12, 0.33, 0.34), roughness=0.80),
        "blueprint_blue": make_lp_mat("LP_BlueprintBlue", (0.18, 0.36, 0.58), roughness=0.80),
        "crimson_velvet": make_lp_mat("LP_CrimsonVelvet", (0.50, 0.11, 0.13), roughness=0.88),
        "brass_gold": make_lp_mat("LP_BrassGold", (0.78, 0.58, 0.20), roughness=0.38, metallic=0.58),
        "copper": make_lp_mat("LP_CopperTrim", (0.74, 0.42, 0.22), roughness=0.42, metallic=0.58),
        "wax_red": make_lp_mat("LP_WaxRed", (0.64, 0.12, 0.13), roughness=0.52),
        "charcoal": make_lp_mat("LP_CharcoalMetal", (0.16, 0.18, 0.22), roughness=0.60, metallic=0.34),
        "glass_sky": make_lp_mat("LP_GlassSky", (0.72, 0.86, 0.94), roughness=0.06, metallic=0.05, alpha=0.26, transmission=0.82),
        "inox": make_lp_mat("LP_InoxSteel", (0.74, 0.78, 0.82), roughness=0.34, metallic=0.68),
        "warm_lantern": make_lp_mat("LP_WarmLantern", (1.0, 0.84, 0.48), roughness=0.35, emission=(1.0, 0.80, 0.42), emission_strength=0.95),
        "amber_glass": make_lp_mat("LP_AmberGlass", (0.86, 0.50, 0.14), roughness=0.24, emission=(0.86, 0.50, 0.14), emission_strength=0.38),
        "jade_glass": make_lp_mat("LP_JadeGlass", (0.16, 0.62, 0.42), roughness=0.24, emission=(0.16, 0.62, 0.42), emission_strength=0.28),
        "banker_green": make_lp_mat("LP_BankerGreen", (0.13, 0.48, 0.25), roughness=0.36, emission=(0.16, 0.60, 0.28), emission_strength=0.45),
        "sunbeam_floor": make_lp_mat("LP_SunbeamFloor", (0.94, 0.84, 0.54), roughness=0.78, emission=(0.94, 0.82, 0.46), emission_strength=0.14),
        "box_purple": make_lp_mat("LP_BoxPurple", (0.42, 0.26, 0.58), roughness=0.78),
        "box_teal": make_lp_mat("LP_BoxTeal", (0.20, 0.48, 0.54), roughness=0.78),
        "lantern_red": make_lp_mat("LP_LanternRed", (0.82, 0.18, 0.15), roughness=0.46, emission=(0.82, 0.18, 0.15), emission_strength=0.32),
        "lantern_yellow": make_lp_mat("LP_LanternYellow", (0.90, 0.68, 0.18), roughness=0.46, emission=(0.90, 0.68, 0.18), emission_strength=0.32),
        "lantern_green": make_lp_mat("LP_LanternGreen", (0.20, 0.60, 0.26), roughness=0.46, emission=(0.20, 0.60, 0.26), emission_strength=0.28),
        "lantern_purple": make_lp_mat("LP_LanternPurple", (0.50, 0.26, 0.70), roughness=0.46, emission=(0.50, 0.26, 0.70), emission_strength=0.32),
        "tree_bark": make_lp_mat("LP_TreeBark", (0.36, 0.22, 0.13), roughness=0.86),
        "tree_leaf_bright": make_lp_mat("LP_TreeLeafBright", (0.34, 0.58, 0.20), roughness=0.82),
        "tree_leaf_mid": make_lp_mat("LP_TreeLeafMid", (0.20, 0.42, 0.16), roughness=0.84),
        "rock_grey": make_lp_mat("LP_RockGrey", (0.48, 0.49, 0.52), roughness=0.88),
        "plastic_red": make_lp_mat("LP_PlasticRed", (0.74, 0.18, 0.17), roughness=0.52),
        "plastic_blue": make_lp_mat("LP_PlasticBlue", (0.18, 0.40, 0.74), roughness=0.52),
        "cloud_white": make_lp_mat("LP_CloudWhite", (0.88, 0.89, 0.90), roughness=0.92),
        "book_navy": make_lp_mat("LP_BookNavy", (0.18, 0.26, 0.44), roughness=0.82),
        "book_forest": make_lp_mat("LP_BookForest", (0.16, 0.38, 0.24), roughness=0.82),
        "book_ochre": make_lp_mat("LP_BookOchre", (0.72, 0.50, 0.18), roughness=0.80),
        "burlap_tan": make_lp_mat("LP_BurlapTan", (0.64, 0.54, 0.38), roughness=0.90),
        "ceramic_sage": make_lp_mat("LP_CeramicSage", (0.46, 0.62, 0.54), roughness=0.68),
        "parchment": make_lp_mat("LP_Parchment", (0.80, 0.73, 0.58), roughness=0.86),
        "carry_skin": make_lp_mat("LP_CarrySkin", (0.84, 0.65, 0.49), roughness=0.82),
    }


# ==================== FLAT-SHADED BMESH PRIMITIVES ====================

def _finalize_bm(name, parent, bm, loc, rot, mat, col):
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    uv_layer = bm.loops.layers.uv.verify()
    zs = [v.co.z for v in bm.verts]
    min_z = min(zs) if zs else 0.0
    max_z = max(zs) if zs else 1.0
    dz = max(0.001, max_z - min_z)
    for face in bm.faces:
        face.smooth = False
        for loop in face.loops:
            co = loop.vert.co
            u = (co.x + co.y) * 0.5
            v = max(0.0, min(1.0, (co.z - min_z) / dz))
            loop[uv_layer].uv = (u, v)

    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm.to_mesh(mesh)
    bm.free()

    obj = bpy.data.objects.new(name, mesh)
    obj.location = Vector(loc)
    obj.rotation_euler = Euler(rot)
    if mat:
        obj.data.materials.append(mat)
    if parent:
        obj.parent = parent
    link_obj(obj, col)
    return obj


def add_box(name, parent, loc, size, mat, rot=(0, 0, 0), col=None):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.scale(bm, vec=Vector(size), verts=bm.verts)
    return _finalize_bm(name, parent, bm, loc, rot, mat, col)


def add_cyl(name, parent, loc, r1, r2, depth, mat, verts=8, rot=(0, 0, 0), col=None):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=verts, radius1=r1, radius2=r2, depth=depth)
    return _finalize_bm(name, parent, bm, loc, rot, mat, col)


def add_ico(name, parent, loc, scale_xyz, mat, subdivisions=1, rot=(0, 0, 0), jitter=0.0, col=None):
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, subdivisions=subdivisions, radius=1.0)
    for v in bm.verts:
        k = 1.0 + jitter * math.sin(v.co.x * 3.9 + loc[0]) * math.cos(v.co.y * 4.3 + loc[1])
        v.co.x *= scale_xyz[0] * k
        v.co.y *= scale_xyz[1] * k
        v.co.z *= scale_xyz[2] * k
    return _finalize_bm(name, parent, bm, loc, rot, mat, col)


def add_prism_gable(name, parent, loc, width_x, width_y, height_z, mat, col=None):
    bm = bmesh.new()
    hx = width_x * 0.5
    hy = width_y * 0.5
    v0 = bm.verts.new((-hx, -hy, 0.0))
    v1 = bm.verts.new((hx, -hy, 0.0))
    v2 = bm.verts.new((0.0, -hy, height_z))
    v3 = bm.verts.new((-hx, hy, 0.0))
    v4 = bm.verts.new((hx, hy, 0.0))
    v5 = bm.verts.new((0.0, hy, height_z))
    bm.faces.new((v0, v1, v2))
    bm.faces.new((v4, v3, v5))
    bm.faces.new((v0, v2, v5, v3))
    bm.faces.new((v1, v4, v5, v2))
    bm.faces.new((v3, v4, v1, v0))
    return _finalize_bm(name, parent, bm, loc, (0, 0, 0), mat, col)


def add_limb(name, parent, p0, p1, r0, r1, mat, segments=6, col=None):
    v0, v1 = Vector(p0), Vector(p1)
    vec = v1 - v0
    length = vec.length
    if length < 1e-4:
        return None
    mid = (v0 + v1) * 0.5
    rot_q = Vector((0, 0, 1)).rotation_difference(vec.normalized())
    return add_cyl(name, parent, mid, r0, r1, length, mat, verts=segments, rot=rot_q.to_euler(), col=col)


def add_gear_wheel(name, parent, loc, radius, thickness, teeth, mat_rim, mat_hub, rot=(0, 0, 0), col=None):
    """Chunky Stylized Low Poly gear wheel with distinct spokes & teeth for workshop animation."""
    g = bpy.data.objects.new(name, None)
    g.location = Vector(loc)
    g.rotation_euler = Euler(rot)
    if parent:
        g.parent = parent
    link_obj(g, col)
    add_cyl("Rim", g, (0, 0, 0), radius * 0.84, radius * 0.84, thickness, mat_rim, verts=12, col=col)
    add_cyl("Core", g, (0, 0, 0), radius * 0.34, radius * 0.34, thickness * 1.25, mat_hub, verts=8, col=col)
    for ti in range(teeth):
        ang = 2.0 * math.pi * ti / teeth
        tx, ty = math.cos(ang) * radius, math.sin(ang) * radius
        add_box(f"Tooth_{ti}", g, (tx, ty, 0), (radius * 0.24, radius * 0.18, thickness * 0.92), mat_rim, rot=(0, 0, ang), col=col)
    return g


def add_3d_text_mesh(name, parent, text_str, loc, rot, size, mat, font_data=None, extrude=0.014, col=None):
    curve = bpy.data.curves.new(name=name + "_Curve", type="FONT")
    curve.body = text_str
    curve.align_x = "CENTER"
    curve.align_y = "CENTER"
    curve.size = size
    curve.extrude = extrude
    if font_data is not None:
        curve.font = font_data
    obj = bpy.data.objects.new(name, curve)
    obj.location = Vector(loc)
    obj.rotation_euler = Euler(rot)
    if mat:
        obj.data.materials.append(mat)
    if parent:
        obj.parent = parent
    link_obj(obj, col)

    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.convert(target="MESH")
    for face in obj.data.polygons:
        face.use_smooth = False
    return obj


# ==================== SCULPTED LOW-POLY PROP LIBRARY ====================

def add_lathe(name, parent, loc, profile_rz, mat, segments=8, rot=(0, 0, 0), col=None):
    """Revolves a (radius, z) profile into a closed flat-shaded low-poly mesh."""
    bm = bmesh.new()
    rings = []
    for r, z in profile_rz:
        if r <= 1e-4:
            rings.append([bm.verts.new((0.0, 0.0, z))])
        else:
            ring = []
            for si in range(segments):
                ang = 2.0 * math.pi * si / segments
                ring.append(bm.verts.new((math.cos(ang) * r, math.sin(ang) * r, z)))
            rings.append(ring)

    if len(rings[0]) > 1:
        bm.faces.new(list(reversed(rings[0])))
    if len(rings[-1]) > 1:
        bm.faces.new(rings[-1])

    for i in range(len(rings) - 1):
        r0, r1 = rings[i], rings[i + 1]
        if len(r0) == 1 and len(r1) > 1:
            for si in range(segments):
                bm.faces.new((r0[0], r1[si], r1[(si + 1) % segments]))
        elif len(r0) > 1 and len(r1) == 1:
            for si in range(segments):
                bm.faces.new((r0[si], r0[(si + 1) % segments], r1[0]))
        elif len(r0) > 1 and len(r1) > 1:
            for si in range(segments):
                ni = (si + 1) % segments
                bm.faces.new((r0[si], r0[ni], r1[ni], r1[si]))

    return _finalize_bm(name, parent, bm, loc, rot, mat, col)


def add_sculpted_vase(name, parent, loc, height, radius, body_mat, mats, with_handles=False, rot_z=0.0, col=None):
    """Multi-ring turned ceramic/porcelain amphora resting flush at local z=0."""
    v = bpy.data.objects.new(name, None)
    v.location = Vector(loc)
    v.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        v.parent = parent
    link_obj(v, col)

    profile = [
        (radius * 0.50, 0.0),
        (radius * 0.58, height * 0.06),
        (radius * 0.42, height * 0.16),
        (radius * 0.94, height * 0.44),
        (radius * 1.00, height * 0.58),
        (radius * 0.66, height * 0.76),
        (radius * 0.40, height * 0.87),
        (radius * 0.56, height * 0.96),
        (radius * 0.48, height),
    ]
    add_lathe(f"{name}_Body", v, (0, 0, 0), profile, body_mat, segments=8, col=col)
    add_cyl(f"{name}_InnerHollow", v, (0, 0, height * 0.96), radius * 0.34, radius * 0.34, height * 0.07, mats["walnut_dark"], verts=8, col=col)
    add_cyl(f"{name}_BrassCollar", v, (0, 0, height * 0.85), radius * 0.44, radius * 0.44, height * 0.04, mats["brass_gold"], verts=8, col=col)

    if with_handles:
        for sgn, htag in ((-1.0, "L"), (1.0, "R")):
            add_limb(f"{name}_HandleLo_{htag}", v, (0, sgn * radius * 0.90, height * 0.56), (0, sgn * radius * 1.32, height * 0.72), radius * 0.10, radius * 0.09, mats["brass_gold"], segments=6, col=col)
            add_limb(f"{name}_HandleHi_{htag}", v, (0, sgn * radius * 1.32, height * 0.72), (0, sgn * radius * 0.44, height * 0.85), radius * 0.09, radius * 0.09, mats["brass_gold"], segments=6, col=col)
    return v


def add_apothecary_bottle(name, parent, loc, height, radius, glass_mat, mats, stopper_mat=None, col=None):
    """Curved apothecary glass bottle with neck bead, cork/brass stopper & parchment label resting at z=0."""
    b = bpy.data.objects.new(name, None)
    b.location = Vector(loc)
    if parent:
        b.parent = parent
    link_obj(b, col)

    profile = [
        (radius * 0.76, 0.0),
        (radius * 0.95, height * 0.09),
        (radius * 1.00, height * 0.36),
        (radius * 0.82, height * 0.50),
        (radius * 0.30, height * 0.62),
        (radius * 0.25, height * 0.74),
        (radius * 0.36, height * 0.78),
        (radius * 0.32, height * 0.82),
    ]
    add_lathe(f"{name}_Glass", b, (0, 0, 0), profile, glass_mat, segments=8, col=col)
    s_mat = stopper_mat or mats["honey_wood"]
    add_cyl(f"{name}_Stopper", b, (0, 0, height * 0.89), radius * 0.22, radius * 0.28, height * 0.18, s_mat, verts=6, col=col)
    add_box(f"{name}_Label", b, (radius * 0.98, 0, height * 0.26), (0.012, radius * 0.85, height * 0.22), mats["parchment"], col=col)
    return b


def add_mini_treasure_chest(name, parent, loc, size_xyz, body_mat, mats, rot_z=0.0, col=None):
    """Sculpted low-poly casket with stepped domed lid, brass straps & front lock clasp at z=0."""
    ch = bpy.data.objects.new(name, None)
    ch.location = Vector(loc)
    ch.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        ch.parent = parent
    link_obj(ch, col)

    sx, sy, sz = size_xyz
    add_box(f"{name}_Plinth", ch, (0, 0, sz * 0.07), (sx * 1.05, sy * 1.05, sz * 0.14), mats["wood_dark"], col=col)
    add_box(f"{name}_Body", ch, (0, 0, sz * 0.40), (sx, sy, sz * 0.52), body_mat, col=col)
    add_box(f"{name}_LidLip", ch, (0, 0, sz * 0.72), (sx * 1.08, sy * 1.08, sz * 0.12), mats["wood_dark"], col=col)
    add_box(f"{name}_LidMid", ch, (0, 0, sz * 0.84), (sx * 0.86, sy * 1.04, sz * 0.12), body_mat, col=col)
    add_box(f"{name}_LidCrown", ch, (0, 0, sz * 0.95), (sx * 0.56, sy * 1.02, sz * 0.10), body_mat, col=col)
    for si, b_y in enumerate((-sy * 0.30, sy * 0.30)):
        add_box(f"{name}_Strap_{si}", ch, (0, b_y, sz * 0.52), (sx * 1.09, sy * 0.08, sz * 0.82), mats["brass_gold"], col=col)
        add_box(f"{name}_StrapTop_{si}", ch, (0, b_y, sz * 0.92), (sx * 0.88, sy * 0.08, sz * 0.18), mats["brass_gold"], col=col)
    add_box(f"{name}_LockPlate", ch, (sx * 0.54, 0, sz * 0.65), (0.018, sy * 0.22, sz * 0.24), mats["brass_gold"], col=col)
    add_box(f"{name}_LockSlot", ch, (sx * 0.55, 0, sz * 0.64), (0.010, sy * 0.06, sz * 0.10), mats["charcoal"], col=col)
    return ch


def add_scroll_bundle(name, parent, loc, length, radius, mat_paper, mats, rot_z=0.0, col=None):
    """Rolled parchment scroll with dark recessed ends, wooden roller knobs & wax-red tie ribbon at z=0."""
    sc = bpy.data.objects.new(name, None)
    sc.location = Vector(loc)
    sc.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        sc.parent = parent
    link_obj(sc, col)

    zc = radius + 0.004
    add_cyl(f"{name}_Paper", sc, (0, 0, zc), radius, radius, length, mat_paper, verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_box(f"{name}_EdgeFlap", sc, (0, radius * 0.45, zc + radius * 0.72), (length * 0.96, radius * 0.65, 0.008), mat_paper, rot=(0.35, 0, 0), col=col)
    add_cyl(f"{name}_HollowCore", sc, (0, 0, zc), radius * 0.58, radius * 0.58, length + 0.006, mats["wood_dark"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl(f"{name}_Rod", sc, (0, 0, zc), radius * 0.26, radius * 0.26, length + 0.08, mats["honey_wood"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)
    for k_sgn, ktag in ((-1.0, "L"), (1.0, "R")):
        add_cyl(f"{name}_Knob_{ktag}", sc, (k_sgn * (length * 0.5 + 0.042), 0, zc), radius * 0.38, radius * 0.24, 0.022, mats["brass_gold"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl(f"{name}_TieBand", sc, (0, 0, zc), radius + 0.004, radius + 0.004, length * 0.11, mats["wax_red"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    return sc


def add_banker_lamp(name, parent, loc, mats, rot_z=0.0, col=None):
    """Lathed brass pedestal + arched gooseneck + hollow faceted green banker shade + bulb & pull chain at z=0."""
    bl = bpy.data.objects.new(name, None)
    bl.location = Vector(loc)
    bl.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        bl.parent = parent
    link_obj(bl, col)

    base_prof = [
        (0.115, 0.0), (0.115, 0.018), (0.095, 0.030), (0.068, 0.048), (0.034, 0.065),
        (0.024, 0.145), (0.038, 0.162), (0.024, 0.180), (0.022, 0.255),
    ]
    add_lathe(f"{name}_Pedestal", bl, (0, 0, 0), base_prof, mats["brass_gold"], segments=8, col=col)
    add_limb(f"{name}_NeckArm", bl, (0.0, 0.0, 0.25), (0.048, 0.0, 0.395), 0.020, 0.016, mats["brass_gold"], segments=6, col=col)
    add_cyl(f"{name}_YokeBar", bl, (0.06, 0.0, 0.395), 0.014, 0.014, 0.34, mats["brass_gold"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)

    # Multi-panel beveled green banker shade hood angled over desk (+X)
    hood_loc = (0.075, 0.0, 0.395)
    add_box(f"{name}_HoodTop", bl, (hood_loc[0], 0.0, hood_loc[2] + 0.045), (0.12, 0.32, 0.016), mats["banker_green"], rot=(0, 0.25, 0), col=col)
    add_box(f"{name}_HoodFront", bl, (hood_loc[0] + 0.072, 0.0, hood_loc[2] + 0.005), (0.095, 0.32, 0.016), mats["banker_green"], rot=(0, 0.85, 0), col=col)
    add_box(f"{name}_HoodBack", bl, (hood_loc[0] - 0.065, 0.0, hood_loc[2] + 0.015), (0.075, 0.32, 0.016), mats["banker_green"], rot=(0, -0.78, 0), col=col)
    for esgn, etag in ((-1.0, "L"), (1.0, "R")):
        add_box(f"{name}_HoodCap_{etag}", bl, (hood_loc[0] + 0.01, esgn * 0.162, hood_loc[2] + 0.01), (0.19, 0.018, 0.085), mats["brass_gold"], rot=(0, 0.25, 0), col=col)
    add_cyl(f"{name}_Bulb", bl, (hood_loc[0] + 0.015, 0.0, hood_loc[2] + 0.005), 0.028, 0.022, 0.16, mats["warm_lantern"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)
    add_limb(f"{name}_PullChain", bl, (0.045, -0.05, 0.38), (0.045, -0.05, 0.23), 0.005, 0.005, mats["brass_gold"], segments=4, col=col)
    add_ico(f"{name}_PullBead", bl, (0.045, -0.05, 0.22), (0.015, 0.015, 0.022), mats["brass_gold"], subdivisions=1, col=col)
    return bl


def add_brass_scale(name, parent, loc, mats, rot_z=0.0, col=None):
    """Antique brass balance scale with stepped base, weights, beam pointer, triple wires & concave pans at z=0."""
    bs = bpy.data.objects.new(name, None)
    bs.location = Vector(loc)
    bs.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        bs.parent = parent
    link_obj(bs, col)

    add_cyl(f"{name}_Plinth", bs, (0, 0, 0.010), 0.125, 0.118, 0.020, mats["wood_dark"], verts=8, col=col)
    pillar_prof = [
        (0.105, 0.020), (0.092, 0.038), (0.050, 0.056), (0.026, 0.080),
        (0.020, 0.220), (0.036, 0.236), (0.018, 0.255), (0.018, 0.390),
        (0.030, 0.405), (0.016, 0.435), (0.002, 0.485),
    ]
    add_lathe(f"{name}_Pillar", bs, (0, 0, 0), pillar_prof, mats["brass_gold"], segments=8, col=col)

    # Beveled fulcrum triangle & main balance beam along Y
    add_box(f"{name}_Fulcrum", bs, (0, 0, 0.390), (0.034, 0.045, 0.045), mats["brass_gold"], rot=(0.78, 0, 0), col=col)
    add_box(f"{name}_Beam", bs, (0, 0, 0.392), (0.024, 0.43, 0.022), mats["brass_gold"], col=col)
    add_limb(f"{name}_Needle", bs, (0.016, 0, 0.388), (0.016, 0, 0.310), 0.007, 0.002, mats["charcoal"], segments=4, col=col)

    pan_prof = [
        (0.045, 0.0), (0.075, 0.006), (0.095, 0.020), (0.088, 0.022), (0.045, 0.008), (0.001, 0.006),
    ]
    for sgn, ptag in ((-1.0, "L"), (1.0, "R")):
        py = sgn * 0.21
        add_cyl(f"{name}_HookCap_{ptag}", bs, (0, py, 0.405), 0.016, 0.016, 0.025, mats["brass_gold"], verts=6, col=col)
        add_lathe(f"{name}_Pan_{ptag}", bs, (0, py, 0.145), pan_prof, mats["brass_gold"], segments=8, col=col)
        for ci in range(3):
            ang = 2.0 * math.pi * ci / 3.0 + 0.3
            wx, wy = math.cos(ang) * 0.086, py + math.sin(ang) * 0.086
            add_limb(f"{name}_Chain_{ptag}_{ci}", bs, (0, py, 0.385), (wx, wy, 0.165), 0.0045, 0.0045, mats["brass_gold"], segments=4, col=col)

    for wi, (wy_w, wr, wh) in enumerate(((-0.038, 0.018, 0.026), (0.032, 0.014, 0.022))):
        add_cyl(f"{name}_Weight_{wi}", bs, (0.070, wy_w, 0.020 + wh * 0.5), wr, wr * 0.85, wh, mats["brass_gold"], verts=6, col=col)
        add_cyl(f"{name}_WeightKnob_{wi}", bs, (0.070, wy_w, 0.020 + wh + 0.006), wr * 0.45, wr * 0.45, 0.010, mats["brass_gold"], verts=6, col=col)
    return bs


def add_wax_seal_kit(name, parent, loc, mats, rot_z=0.0, col=None):
    """Melting tripod warmer, copper pot, turned wooden stamp, wax sticks & sealed envelope at z=0."""
    wk = bpy.data.objects.new(name, None)
    wk.location = Vector(loc)
    wk.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        wk.parent = parent
    link_obj(wk, col)

    # Melting stand + glowing candle + copper bowl with wax pool + handle
    mx, my = -0.04, 0.11
    add_cyl(f"{name}_WarmRing", wk, (mx, my, 0.052), 0.052, 0.052, 0.012, mats["brass_gold"], verts=8, col=col)
    for li in range(3):
        ang = 2.0 * math.pi * li / 3.0
        add_box(f"{name}_WarmLeg_{li}", wk, (mx + math.cos(ang) * 0.044, my + math.sin(ang) * 0.044, 0.026), (0.012, 0.012, 0.052), mats["brass_gold"], col=col)
    add_cyl(f"{name}_Candle", wk, (mx, my, 0.016), 0.020, 0.020, 0.028, mats["pure_white"], verts=6, col=col)
    add_ico(f"{name}_Flame", wk, (mx, my, 0.038), (0.011, 0.011, 0.016), mats["warm_lantern"], subdivisions=1, col=col)
    add_cyl(f"{name}_MeltPot", wk, (mx, my, 0.078), 0.036, 0.050, 0.044, mats["copper"], verts=8, col=col)
    add_cyl(f"{name}_WaxPool", wk, (mx, my, 0.098), 0.040, 0.040, 0.010, mats["wax_red"], verts=8, col=col)
    add_limb(f"{name}_PotHandle", wk, (mx - 0.045, my, 0.088), (mx - 0.115, my - 0.035, 0.112), 0.011, 0.009, mats["honey_wood"], segments=6, col=col)

    # Turned brass & honey-wood seal stamp
    sx, sy = 0.055, -0.02
    stamp_prof = [(0.030, 0.0), (0.030, 0.016), (0.018, 0.028), (0.015, 0.045)]
    add_lathe(f"{name}_StampDie", wk, (sx, sy, 0), stamp_prof, mats["brass_gold"], segments=8, col=col)
    handle_prof = [(0.015, 0.044), (0.022, 0.065), (0.026, 0.095), (0.018, 0.122), (0.008, 0.130)]
    add_lathe(f"{name}_StampHandle", wk, (sx, sy, 0), handle_prof, mats["honey_wood"], segments=8, col=col)
    add_ico(f"{name}_StampCrown", wk, (sx, sy, 0.132), (0.011, 0.011, 0.009), mats["brass_gold"], subdivisions=1, col=col)

    # Two crimson wax sticks & sealed envelope
    add_box(f"{name}_WaxStickA", wk, (-0.05, -0.10, 0.008), (0.11, 0.018, 0.016), mats["wax_red"], rot=(0, 0, 0.25), col=col)
    add_box(f"{name}_WaxStickB", wk, (-0.06, -0.12, 0.018), (0.10, 0.018, 0.016), mats["crimson_plank"], rot=(0, 0.08, -0.35), col=col)
    add_box(f"{name}_Envelope", wk, (0.075, 0.12, 0.004), (0.145, 0.20, 0.008), mats["parchment"], rot=(0, 0, 0.20), col=col)
    add_cyl(f"{name}_EnvSeal", wk, (0.075, 0.12, 0.010), 0.022, 0.020, 0.006, mats["wax_red"], verts=8, col=col)
    return wk


def add_open_ledger_and_quill(name, parent, loc, mats, rot_z=0.0, col=None):
    """Open arched hardcover ledger with layered pages, ribbon, inkwell & sculpted feather quill at z=0."""
    ol = bpy.data.objects.new(name, None)
    ol.location = Vector(loc)
    ol.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        ol.parent = parent
    link_obj(ol, col)

    add_cyl(f"{name}_Spine", ol, (0, 0, 0.012), 0.018, 0.018, 0.42, mats["wood_dark"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)
    for sgn, ptag in ((-1.0, "L"), (1.0, "R")):
        tilt = -sgn * 0.08
        add_box(f"{name}_Cover_{ptag}", ol, (0, sgn * 0.115, 0.007), (0.34, 0.22, 0.014), mats["wood_dark"], rot=(tilt, 0, 0), col=col)
        add_box(f"{name}_Pages_{ptag}", ol, (0, sgn * 0.108, 0.025), (0.31, 0.20, 0.024), mats["parchment"], rot=(tilt, 0, 0), col=col)
        add_box(f"{name}_TopPage_{ptag}", ol, (0, sgn * 0.102, 0.039), (0.30, 0.19, 0.006), mats["pure_white"], rot=(tilt, 0, 0), col=col)
    add_box(f"{name}_Ribbon", ol, (0.165, 0.02, 0.012), (0.10, 0.022, 0.005), mats["wax_red"], rot=(0, 0.18, 0.25), col=col)

    # Octagonal inkwell + brass screw collar + sculpted angled feather quill
    ix, iy = 0.07, 0.28
    ink_prof = [(0.042, 0.0), (0.046, 0.012), (0.038, 0.044), (0.022, 0.056), (0.026, 0.068)]
    add_lathe(f"{name}_Inkwell", ol, (ix, iy, 0), ink_prof, mats["charcoal"], segments=8, col=col)
    add_cyl(f"{name}_InkNeck", ol, (ix, iy, 0.062), 0.027, 0.027, 0.014, mats["brass_gold"], verts=8, col=col)

    q_base = (ix, iy, 0.064)
    q_mid = (ix - 0.045, iy + 0.06, 0.175)
    q_tip = (ix - 0.095, iy + 0.125, 0.285)
    add_limb(f"{name}_QuillShaft", ol, q_base, q_tip, 0.006, 0.0025, mats["pure_white"], segments=4, col=col)
    add_box(f"{name}_FeatherLow", ol, (ix - 0.048, iy + 0.065, 0.180), (0.048, 0.006, 0.095), mats["pure_white"], rot=(-0.42, -0.34, 0.48), col=col)
    add_box(f"{name}_FeatherHi", ol, (ix - 0.075, iy + 0.100, 0.240), (0.034, 0.005, 0.080), mats["parchment"], rot=(-0.42, -0.34, 0.48), col=col)
    return ol


def add_bracket_lantern(name, parent, wall_loc, out_dir, mats, col=None):
    """Wrought-iron bracket lantern bolted flush to wall_loc; zero cord sticking above bracket arm."""
    blan = bpy.data.objects.new(name, None)
    blan.location = Vector(wall_loc)
    blan.rotation_euler = Euler((0, 0, math.atan2(out_dir[1], out_dir[0])))
    if parent:
        blan.parent = parent
    link_obj(blan, col)

    # Flush wall plate + brass rivets
    add_box(f"{name}_WallPlate", blan, (0.012, 0.0, 0.0), (0.036, 0.095, 0.30), mats["charcoal"], col=col)
    for bz in (-0.11, 0.11):
        add_cyl(f"{name}_Bolt_{bz:.2f}", blan, (0.030, 0.0, bz), 0.014, 0.014, 0.014, mats["brass_gold"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)

    # Horizontal iron arm + diagonal lower scroll strut + curl finial
    add_limb(f"{name}_Arm", blan, (0.01, 0.0, 0.04), (0.44, 0.0, 0.04), 0.020, 0.018, mats["charcoal"], segments=6, col=col)
    add_limb(f"{name}_Brace", blan, (0.01, 0.0, -0.11), (0.26, 0.0, 0.04), 0.015, 0.015, mats["charcoal"], segments=6, col=col)
    add_ico(f"{name}_Tip", blan, (0.46, 0.0, 0.04), (0.026, 0.020, 0.026), mats["charcoal"], subdivisions=1, col=col)

    # Short vertical iron hang link STRICTLY below arm (z=+0.02 down to -0.07)
    add_limb(f"{name}_HookLink", blan, (0.40, 0.0, 0.03), (0.40, 0.0, -0.07), 0.012, 0.012, mats["charcoal"], segments=4, col=col)

    # Multi-tier hexagonal lantern cage + glowing core
    lx = 0.40
    add_cyl(f"{name}_TopFinial", blan, (lx, 0.0, -0.075), 0.045, 0.015, 0.04, mats["brass_gold"], verts=6, col=col)
    add_cyl(f"{name}_CapTop", blan, (lx, 0.0, -0.135), 0.21, 0.075, 0.09, mats["charcoal"], verts=6, col=col)
    add_cyl(f"{name}_BrassRim", blan, (lx, 0.0, -0.180), 0.22, 0.22, 0.024, mats["brass_gold"], verts=6, col=col)
    add_cyl(f"{name}_GlowBody", blan, (lx, 0.0, -0.295), 0.145, 0.190, 0.22, mats["warm_lantern"], verts=6, col=col)
    for ri in range(6):
        ang = ri * (math.pi / 3.0)
        cx0, cy0 = lx + math.cos(ang) * 0.195, math.sin(ang) * 0.195
        cx1, cy1 = lx + math.cos(ang) * 0.155, math.sin(ang) * 0.155
        add_limb(f"{name}_CageRib_{ri}", blan, (cx0, cy0, -0.18), (cx1, cy1, -0.41), 0.010, 0.010, mats["charcoal"], segments=4, col=col)
    add_cyl(f"{name}_CapBot", blan, (lx, 0.0, -0.435), 0.065, 0.160, 0.07, mats["charcoal"], verts=6, col=col)
    add_cyl(f"{name}_BotDrop", blan, (lx, 0.0, -0.485), 0.012, 0.032, 0.045, mats["brass_gold"], verts=6, col=col)
    return blan


def add_window_flowerbox(name, parent, wall_center, axis, out_dir_sign, box_len, mats, col=None):
    """Hollow timber flowerbox bolted flush into exterior wall with 2 corbel brackets & 3 potted herbs."""
    fb = bpy.data.objects.new(name, None)
    yaw = 0.0 if (axis == "x" and out_dir_sign > 0) else (math.pi if axis == "x" else (-math.pi * 0.5 if out_dir_sign < 0 else math.pi * 0.5))
    fb.location = Vector(wall_center)
    fb.rotation_euler = Euler((0, 0, yaw))
    if parent:
        fb.parent = parent
    link_obj(fb, col)

    # Flush-against-wall trough (x=0.00..0.32 -> back edge cleanly touches outer facade/clapboards!)
    bx = 0.16
    add_box(f"{name}_Bottom", fb, (bx, 0, -0.095), (0.32, box_len, 0.04), mats["wood_dark"], col=col)
    add_box(f"{name}_BackPlank", fb, (0.015, 0, 0.0), (0.05, box_len, 0.22), mats["honey_wood"], col=col)
    add_box(f"{name}_FrontPlank", fb, (0.30, 0, 0.0), (0.04, box_len, 0.22), mats["honey_wood"], col=col)
    for esgn, etag in ((-1.0, "L"), (1.0, "R")):
        add_box(f"{name}_EndPlank_{etag}", fb, (bx, esgn * (box_len * 0.5 - 0.025), 0.0), (0.32, 0.05, 0.22), mats["honey_wood"], col=col)
    add_box(f"{name}_RimF", fb, (0.30, 0, 0.115), (0.07, box_len + 0.05, 0.035), mats["wood_dark"], col=col)
    add_box(f"{name}_SoilBed", fb, (bx, 0, 0.015), (0.25, box_len - 0.10, 0.06), mats["walnut_dark"], col=col)

    # 2 sculpted corbel brackets bolted under trough into exterior wall
    for b_sgn, btag in ((-1.0, "L"), (1.0, "R")):
        by = b_sgn * box_len * 0.32
        add_box(f"{name}_Cleat_{btag}", fb, (0.01, by, -0.20), (0.06, 0.07, 0.24), mats["wood_dark"], col=col)
        add_box(f"{name}_Shelf_{btag}", fb, (bx, by, -0.13), (0.30, 0.07, 0.045), mats["wood_dark"], col=col)
        add_limb(f"{name}_Strut_{btag}", fb, (0.02, by, -0.30), (0.27, by, -0.14), 0.028, 0.028, mats["wood_dark"], segments=4, col=col)

    # 3 sculpted terracotta herb pots resting cleanly on SoilBed top (z=0.045)
    for pi, py_pot in enumerate((-box_len * 0.30, 0.0, box_len * 0.30)):
        _add_potted_herb(f"{name}_Pot_{pi}", fb, (bx, py_pot, 0.045), mats["terracotta_roof"], mats, scale=0.98, col=col)
    return fb


def add_pendant_lamp(name, parent, ceil_loc, shade_z, mats, shade_mat=None, col=None):
    """Multi-tier ceiling pendant lamp with canopy rose, rod, brass socket, stepped lathed shade & recessed bulb."""
    cx, cy, ceil_z = ceil_loc
    pl = bpy.data.objects.new(name, None)
    pl.location = Vector((cx, cy, shade_z))
    if parent:
        pl.parent = parent
    link_obj(pl, col)

    h_drop = max(0.25, ceil_z - shade_z)
    canopy_prof = [(0.085, 0.025), (0.085, 0.0), (0.055, -0.022), (0.024, -0.042)]
    add_lathe(f"{name}_Canopy", pl, (0, 0, h_drop), canopy_prof, mats["charcoal"], segments=8, col=col)
    add_cyl(f"{name}_CanopyCollar", pl, (0, 0, h_drop - 0.048), 0.028, 0.022, 0.026, mats["brass_gold"], verts=8, col=col)
    add_limb(f"{name}_Rod", pl, (0, 0, h_drop - 0.04), (0, 0, 0.145), 0.015, 0.015, mats["charcoal"], segments=6, col=col)

    add_cyl(f"{name}_Socket", pl, (0, 0, 0.145), 0.040, 0.032, 0.075, mats["brass_gold"], verts=8, col=col)
    shade_prof = [
        (0.052, 0.125),
        (0.082, 0.105),
        (0.215, 0.038),
        (0.275, -0.010),
        (0.285, -0.028),
        (0.258, -0.032),
        (0.085, 0.075),
    ]
    add_lathe(f"{name}_Shade", pl, (0, 0, 0), shade_prof, shade_mat or mats["brass_gold"], segments=10, col=col)
    add_cyl(f"{name}_ShadeRim", pl, (0, 0, -0.020), 0.296, 0.290, 0.022, mats["charcoal"], verts=10, col=col)
    bulb_prof = [(0.028, 0.072), (0.068, 0.018), (0.074, -0.045), (0.046, -0.095), (0.002, -0.115)]
    add_lathe(f"{name}_Bulb", pl, (0, 0, 0), bulb_prof, mats["warm_lantern"], segments=8, col=col)
    return pl


def add_table_lantern(name, parent, loc, mats, rot_z=0.0, scale=1.0, col=None):
    """Grounded storm/carriage tabletop lantern resting flush at z=0 (total height ~0.35 * scale)."""
    tl = bpy.data.objects.new(name, None)
    tl.location = Vector(loc)
    tl.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        tl.parent = parent
    link_obj(tl, col)

    s = scale
    add_cyl(f"{name}_BasePlinth", tl, (0, 0, 0.014 * s), 0.112 * s, 0.105 * s, 0.028 * s, mats["charcoal"], verts=8, col=col)
    add_cyl(f"{name}_BaseStep", tl, (0, 0, 0.036 * s), 0.096 * s, 0.086 * s, 0.018 * s, mats["brass_gold"], verts=8, col=col)
    add_cyl(f"{name}_GlassCore", tl, (0, 0, 0.138 * s), 0.066 * s, 0.074 * s, 0.184 * s, mats["warm_lantern"], verts=8, col=col)

    for ci in range(4):
        ang = ci * (math.pi * 0.5) + math.pi * 0.25
        px, py = math.cos(ang) * 0.088 * s, math.sin(ang) * 0.088 * s
        add_limb(f"{name}_Post_{ci}", tl, (px, py, 0.042 * s), (px * 0.96, py * 0.96, 0.236 * s), 0.008 * s, 0.008 * s, mats["charcoal"], segments=4, col=col)

    roof_prof = [
        (0.106 * s, 0.228 * s),
        (0.108 * s, 0.244 * s),
        (0.068 * s, 0.285 * s),
        (0.036 * s, 0.305 * s),
        (0.014 * s, 0.320 * s),
    ]
    add_lathe(f"{name}_Dome", tl, (0, 0, 0), roof_prof, mats["charcoal"], segments=8, col=col)
    add_cyl(f"{name}_VentCollar", tl, (0, 0, 0.292 * s), 0.044 * s, 0.044 * s, 0.014 * s, mats["brass_gold"], verts=8, col=col)
    add_limb(f"{name}_HandleL", tl, (-0.078 * s, 0, 0.244 * s), (-0.045 * s, 0, 0.350 * s), 0.007 * s, 0.007 * s, mats["brass_gold"], segments=4, col=col)
    add_limb(f"{name}_HandleTop", tl, (-0.045 * s, 0, 0.350 * s), (0.045 * s, 0, 0.350 * s), 0.008 * s, 0.008 * s, mats["brass_gold"], segments=4, col=col)
    add_limb(f"{name}_HandleR", tl, (0.045 * s, 0, 0.350 * s), (0.078 * s, 0, 0.244 * s), 0.007 * s, 0.007 * s, mats["brass_gold"], segments=4, col=col)
    return tl


def add_oil_lamp(name, parent, loc, mats, rot_z=0.0, col=None):
    """Vintage kerosene oil lamp with copper/brass reservoir, finger handle, burner & translucent chimney at z=0."""
    ol = bpy.data.objects.new(name, None)
    ol.location = Vector(loc)
    ol.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        ol.parent = parent
    link_obj(ol, col)

    body_prof = [
        (0.078, 0.0), (0.078, 0.014), (0.048, 0.030), (0.092, 0.062),
        (0.095, 0.088), (0.056, 0.112), (0.044, 0.135), (0.052, 0.148),
    ]
    add_lathe(f"{name}_Reservoir", ol, (0, 0, 0), body_prof, mats["copper"], segments=10, col=col)
    add_cyl(f"{name}_BurnerGallery", ol, (0, 0, 0.152), 0.048, 0.044, 0.028, mats["brass_gold"], verts=10, col=col)
    add_cyl(f"{name}_WickKnob", ol, (0.060, 0, 0.132), 0.014, 0.014, 0.024, mats["brass_gold"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)

    # Side finger ring handle (-X)
    add_limb(f"{name}_GripTop", ol, (-0.052, 0, 0.112), (-0.118, 0, 0.104), 0.008, 0.008, mats["brass_gold"], segments=4, col=col)
    add_limb(f"{name}_GripOut", ol, (-0.118, 0, 0.104), (-0.118, 0, 0.036), 0.008, 0.008, mats["brass_gold"], segments=4, col=col)
    add_limb(f"{name}_GripBot", ol, (-0.118, 0, 0.036), (-0.072, 0, 0.042), 0.008, 0.008, mats["brass_gold"], segments=4, col=col)

    # Tall translucent flared glass chimney + warm teardrop flame inside
    chim_prof = [
        (0.042, 0.160), (0.068, 0.215), (0.054, 0.265), (0.036, 0.330), (0.042, 0.365),
    ]
    add_lathe(f"{name}_GlassChimney", ol, (0, 0, 0), chim_prof, mats["glass_sky"], segments=10, col=col)
    flame_prof = [(0.002, 0.165), (0.024, 0.195), (0.018, 0.228), (0.002, 0.258)]
    add_lathe(f"{name}_Flame", ol, (0, 0, 0), flame_prof, mats["warm_lantern"], segments=6, col=col)
    return ol


def _add_hollow_ring(prefix, parent, center, radius, thickness, mat, rot_euler=(0, 0, 0), segments=8, col=None):
    """Creates an unmistakably hollow faceted ring from `segments` struts inside `parent`."""
    ring_grp = bpy.data.objects.new(f"{prefix}_Grp", None)
    ring_grp.location = Vector(center)
    ring_grp.rotation_euler = Euler(rot_euler)
    if parent:
        ring_grp.parent = parent
    link_obj(ring_grp, col)
    pts = [
        (math.cos(2.0 * math.pi * i / segments) * radius, math.sin(2.0 * math.pi * i / segments) * radius, 0.0)
        for i in range(segments)
    ]
    for i in range(segments):
        add_limb(f"{prefix}_S{i}", ring_grp, pts[i], pts[(i + 1) % segments], thickness, thickness, mat, segments=4, col=col)
    return ring_grp


def add_armillary_globe(name, parent, loc, mats, scale=1.0, rot_z=0.0, col=None):
    """Astronomical brass armillary sphere with meridian arc, intersecting rings, polar axis & globe at z=0."""
    ag = bpy.data.objects.new(name, None)
    ag.location = Vector(loc)
    ag.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        ag.parent = parent
    link_obj(ag, col)

    s = scale
    ped_prof = [
        (0.118 * s, 0.0), (0.118 * s, 0.018 * s), (0.088 * s, 0.036 * s),
        (0.034 * s, 0.068 * s), (0.024 * s, 0.125 * s), (0.052 * s, 0.145 * s),
    ]
    add_lathe(f"{name}_Pedestal", ag, (0, 0, 0), ped_prof, mats["wood_dark"], segments=8, col=col)
    add_cyl(f"{name}_Collar", ag, (0, 0, 0.145 * s), 0.042 * s, 0.036 * s, 0.025 * s, mats["brass_gold"], verts=8, col=col)

    zc = 0.320 * s
    r_outer = 0.175 * s
    # Outer vertical meridian C-arc cradling sphere
    arc_pts = [
        (0.0, math.sin(ang) * r_outer, zc - math.cos(ang) * r_outer)
        for ang in (-2.5, -1.7, -0.9, 0.0, 0.9, 1.7, 2.5)
    ]
    for ai in range(len(arc_pts) - 1):
        add_limb(f"{name}_Arc_{ai}", ag, arc_pts[ai], arc_pts[ai + 1], 0.011 * s, 0.011 * s, mats["brass_gold"], segments=4, col=col)
    add_limb(f"{name}_AxisRod", ag, (0.0, -0.145 * s, zc - 0.115 * s), (0.0, 0.145 * s, zc + 0.115 * s), 0.008 * s, 0.008 * s, mats["inox"], segments=4, col=col)

    _add_hollow_ring(f"{name}_Ecliptic", ag, (0, 0, zc), 0.155 * s, 0.010 * s, mats["brass_gold"], rot_euler=(0.45, 0.25, 0.0), segments=10, col=col)
    _add_hollow_ring(f"{name}_Equator", ag, (0, 0, zc), 0.142 * s, 0.009 * s, mats["copper"], rot_euler=(-0.55, -0.30, 0.4), segments=10, col=col)
    add_ico(f"{name}_CoreGlobe", ag, (0, 0, zc), (0.062 * s, 0.062 * s, 0.062 * s), mats["amber_glass"], subdivisions=1, col=col)
    return ag


def add_workshop_stool(name, parent, loc, mats, height=0.52, rot_z=0.0, col=None):
    """Grounded 4-legged wooden craftsman stool with beveled seat & iron footrest ring at z=0."""
    st = bpy.data.objects.new(name, None)
    st.location = Vector(loc)
    st.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        st.parent = parent
    link_obj(st, col)

    seat_prof = [
        (0.155, height - 0.052), (0.195, height - 0.040), (0.205, height - 0.014),
        (0.190, height), (0.001, height),
    ]
    add_lathe(f"{name}_Seat", st, (0, 0, 0), seat_prof, mats["honey_wood"], segments=10, col=col)
    add_cyl(f"{name}_ApronRing", st, (0, 0, height - 0.068), 0.152, 0.152, 0.036, mats["walnut_dark"], verts=8, col=col)

    r_top, r_bot = 0.125, 0.205
    r_mid = r_top + (r_bot - r_top) * ((height - 0.05 - 0.17) / (height - 0.05))
    foot_pts = []
    for li in range(4):
        ang = li * (math.pi * 0.5) + math.pi * 0.25
        ca, sa = math.cos(ang), math.sin(ang)
        add_limb(f"{name}_Leg_{li}", st, (ca * r_top, sa * r_top, height - 0.05), (ca * r_bot, sa * r_bot, 0.0), 0.022, 0.017, mats["wood_dark"], segments=6, col=col)
        add_cyl(f"{name}_Foot_{li}", st, (ca * r_bot, sa * r_bot, 0.012), 0.022, 0.020, 0.024, mats["charcoal"], verts=6, col=col)
        foot_pts.append((ca * r_mid, sa * r_mid, 0.17))
    for li in range(4):
        add_limb(f"{name}_Brace_{li}", st, foot_pts[li], foot_pts[(li + 1) % 4], 0.011, 0.011, mats["charcoal"], segments=4, col=col)
    _add_hollow_ring(f"{name}_FootRing", st, (0, 0, 0.17), r_mid * 0.96, 0.009, mats["charcoal"], segments=8, col=col)
    return st


def add_workbench_loupe(name, parent, loc, mats, rot_z=0.0, col=None):
    """Articulated benchtop jeweler's magnifying lamp with hollow brass bezel & translucent lens at z=0."""
    lp = bpy.data.objects.new(name, None)
    lp.location = Vector(loc)
    lp.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        lp.parent = parent
    link_obj(lp, col)

    base_prof = [(0.102, 0.0), (0.098, 0.020), (0.068, 0.042), (0.042, 0.072)]
    add_lathe(f"{name}_ClampBase", lp, (0, 0, 0), base_prof, mats["charcoal"], segments=8, col=col)
    add_cyl(f"{name}_SwivelCollar", lp, (0, 0, 0.080), 0.036, 0.036, 0.026, mats["brass_gold"], verts=8, col=col)

    elbow = (-0.06, 0.04, 0.42)
    wrist = (0.28, -0.14, 0.58)
    add_limb(f"{name}_ArmLow", lp, (0, 0, 0.088), elbow, 0.022, 0.018, mats["brass_gold"], segments=6, col=col)
    add_cyl(f"{name}_ElbowJoint", lp, elbow, 0.030, 0.030, 0.048, mats["charcoal"], verts=8, rot=(math.pi * 0.5, 0, 0.4), col=col)
    add_limb(f"{name}_ArmTop", lp, elbow, wrist, 0.018, 0.016, mats["brass_gold"], segments=6, col=col)

    ring_c = (wrist[0] + 0.13, wrist[1] - 0.05, wrist[2] - 0.02)
    add_limb(f"{name}_YokeArm", lp, wrist, (ring_c[0] - 0.125, ring_c[1] + 0.045, ring_c[2] + 0.025), 0.015, 0.015, mats["charcoal"], segments=4, col=col)
    lens_rot = (0.32, 0.24, 0.0)
    _add_hollow_ring(f"{name}_Bezel", lp, ring_c, 0.145, 0.018, mats["brass_gold"], rot_euler=lens_rot, segments=10, col=col)
    add_cyl(f"{name}_Lens", lp, ring_c, 0.132, 0.132, 0.012, mats["glass_sky"], verts=10, rot=lens_rot, col=col)
    return lp


def add_burlap_sack(name, parent, loc, mats, scale=1.0, rot_z=0.0, col=None):
    """Sculpted tied burlap cargo sack with pinched neck, red cord tie & ruffled mouth at z=0."""
    sk = bpy.data.objects.new(name, None)
    sk.location = Vector(loc)
    sk.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        sk.parent = parent
    link_obj(sk, col)

    s = scale
    sack_prof = [
        (0.155 * s, 0.0),
        (0.215 * s, 0.055 * s),
        (0.230 * s, 0.145 * s),
        (0.185 * s, 0.245 * s),
        (0.088 * s, 0.335 * s),
        (0.132 * s, 0.410 * s),
        (0.100 * s, 0.415 * s),
        (0.055 * s, 0.355 * s),
    ]
    add_lathe(f"{name}_Body", sk, (0, 0, 0), sack_prof, mats["burlap_tan"], segments=9, col=col)
    add_cyl(f"{name}_CordTie", sk, (0, 0, 0.335 * s), 0.098 * s, 0.098 * s, 0.024 * s, mats["wax_red"], verts=9, col=col)
    add_limb(f"{name}_CordEndA", sk, (0.085 * s, 0, 0.335 * s), (0.140 * s, 0.035 * s, 0.245 * s), 0.008 * s, 0.006 * s, mats["wax_red"], segments=4, col=col)
    add_limb(f"{name}_CordEndB", sk, (0.085 * s, 0, 0.335 * s), (0.132 * s, -0.045 * s, 0.255 * s), 0.008 * s, 0.006 * s, mats["wax_red"], segments=4, col=col)
    return sk


def add_pegboard_tools(name, parent, board_face_y, peg_z, mats, col=None):
    """5 immediately recognizable hanging workshop tools mounted on iron pegs protruding from board_face_y (+Y)."""
    pt = bpy.data.objects.new(name, None)
    pt.location = Vector((0.0, board_face_y, peg_z))
    if parent:
        pt.parent = parent
    link_obj(pt, col)

    # Tool Y center in front of pegboard (+0.048m in front of backing plate)
    ty = 0.048
    def _peg(ptag, px, pz):
        add_limb(f"{name}_Peg_{ptag}", pt, (px, -0.015, pz), (px, ty + 0.032, pz), 0.012, 0.012, mats["charcoal"], segments=6, col=col)

    # 1. Claw Hammer (X = -1.86)
    hx = -1.86
    _peg("HammerL", hx - 0.06, 0.25)
    _peg("HammerR", hx + 0.06, 0.25)
    add_box(f"{name}_HammerGrip", pt, (hx, ty, -0.06), (0.048, 0.034, 0.44), mats["honey_wood"], col=col)
    add_box(f"{name}_HammerCollar", pt, (hx, ty, 0.18), (0.056, 0.042, 0.06), mats["charcoal"], col=col)
    add_box(f"{name}_HammerPoll", pt, (hx - 0.04, ty, 0.28), (0.20, 0.055, 0.085), mats["inox"], col=col)
    add_limb(f"{name}_HammerClawTop", pt, (hx + 0.05, ty - 0.012, 0.295), (hx + 0.16, ty - 0.012, 0.215), 0.015, 0.007, mats["inox"], segments=4, col=col)
    add_limb(f"{name}_HammerClawBot", pt, (hx + 0.05, ty + 0.012, 0.295), (hx + 0.16, ty + 0.012, 0.215), 0.015, 0.007, mats["inox"], segments=4, col=col)

    # 2. Serrated Hand Saw with Open D-Handle (X = -1.32)
    sx = -1.32
    _peg("Saw", sx, 0.34)
    add_box(f"{name}_SawBlade", pt, (sx - 0.01, ty, -0.04), (0.145, 0.016, 0.56), mats["inox"], col=col)
    for ti in range(6):
        tz = -0.27 + ti * 0.088
        add_cyl(f"{name}_SawTooth_{ti}", pt, (sx + 0.075, ty, tz), 0.028, 0.002, 0.045, mats["inox"], verts=3, rot=(0, math.pi * 0.5, 0), col=col)
    # Open walnut D-handle grip loop
    add_box(f"{name}_SawHandleTop", pt, (sx, ty, 0.36), (0.18, 0.046, 0.045), mats["walnut_dark"], col=col)
    add_box(f"{name}_SawHandleBot", pt, (sx, ty, 0.24), (0.18, 0.046, 0.045), mats["walnut_dark"], col=col)
    add_box(f"{name}_SawHandleBack", pt, (sx - 0.075, ty, 0.30), (0.045, 0.046, 0.16), mats["walnut_dark"], col=col)
    add_box(f"{name}_SawHandleGrip", pt, (sx + 0.070, ty, 0.30), (0.040, 0.046, 0.16), mats["walnut_dark"], col=col)
    for rz in (0.26, 0.34):
        add_cyl(f"{name}_SawRivet_{rz:.2f}", pt, (sx - 0.02, ty + 0.026, rz), 0.014, 0.014, 0.012, mats["brass_gold"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)

    # 3. Hollow Hand Magnifying Glass (X = -0.74)
    mx = -0.74
    _peg("Mag", mx, 0.36)
    _add_hollow_ring(f"{name}_MagRim", pt, (mx, ty, 0.22), 0.155, 0.020, mats["brass_gold"], rot_euler=(math.pi * 0.5, 0, 0), segments=10, col=col)
    add_cyl(f"{name}_MagLens", pt, (mx, ty, 0.22), 0.142, 0.142, 0.014, mats["glass_sky"], verts=10, rot=(math.pi * 0.5, 0, 0), col=col)
    add_cyl(f"{name}_MagFerrule", pt, (mx, ty, 0.045), 0.028, 0.024, 0.06, mats["brass_gold"], verts=8, col=col)
    add_cyl(f"{name}_MagHandle", pt, (mx, ty, -0.14), 0.030, 0.022, 0.31, mats["walnut_dark"], verts=8, col=col)

    # 4. Fluted Wood Chisel (X = -0.22)
    cx = -0.22
    _peg("Chisel", cx, 0.18)
    add_cyl(f"{name}_ChiselGrip", pt, (cx, ty, 0.24), 0.038, 0.028, 0.22, mats["honey_wood"], verts=8, col=col)
    add_cyl(f"{name}_ChiselFerrule", pt, (cx, ty, 0.11), 0.030, 0.026, 0.045, mats["brass_gold"], verts=8, col=col)
    add_box(f"{name}_ChiselBlade", pt, (cx, ty, -0.06), (0.062, 0.016, 0.30), mats["inox"], col=col)
    add_box(f"{name}_ChiselBevel", pt, (cx, ty, -0.22), (0.068, 0.012, 0.04), mats["inox"], col=col)

    # 5. Double-Open-End Iron Wrench (X = +0.24)
    wx = 0.24
    _peg("Wrench", wx, 0.24)
    add_box(f"{name}_WrenchShank", pt, (wx, ty, 0.0), (0.056, 0.024, 0.38), mats["charcoal"], col=col)
    for w_sgn, wtag in ((1.0, "Top"), (-1.0, "Bot")):
        zc = w_sgn * 0.22
        add_box(f"{name}_WrenchJawL_{wtag}", pt, (wx - 0.048, ty, zc + w_sgn * 0.025), (0.032, 0.028, 0.11), mats["charcoal"], col=col)
        add_box(f"{name}_WrenchJawR_{wtag}", pt, (wx + 0.048, ty, zc + w_sgn * 0.025), (0.032, 0.028, 0.11), mats["charcoal"], col=col)
        add_box(f"{name}_WrenchBridge_{wtag}", pt, (wx, ty, zc - w_sgn * 0.025), (0.125, 0.028, 0.040), mats["charcoal"], col=col)
    return pt


def add_abacus(name, parent, loc, mats, rot_z=0.0, col=None):
    """Antique wooden merchant abacus with center beam, 5 brass rods & faceted sliding beads at z=0."""
    ab = bpy.data.objects.new(name, None)
    ab.location = Vector(loc)
    ab.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        ab.parent = parent
    link_obj(ab, col)

    fw, fl, fh = 0.22, 0.34, 0.034
    zf = fh * 0.5
    add_box(f"{name}_RailL", ab, (-fw * 0.46, 0.0, zf), (0.022, fl, fh), mats["walnut_dark"], col=col)
    add_box(f"{name}_RailR", ab, (fw * 0.46, 0.0, zf), (0.022, fl, fh), mats["walnut_dark"], col=col)
    add_box(f"{name}_EndS", ab, (0.0, -fl * 0.47, zf), (fw, 0.022, fh), mats["walnut_dark"], col=col)
    add_box(f"{name}_EndN", ab, (0.0, fl * 0.47, zf), (fw, 0.022, fh), mats["walnut_dark"], col=col)
    add_box(f"{name}_CrossBeam", ab, (fw * 0.14, 0.0, zf), (0.018, fl - 0.03, fh * 0.92), mats["wood_dark"], col=col)

    for ri in range(5):
        ry = -0.115 + ri * 0.0575
        add_cyl(f"{name}_Rod_{ri}", ab, (0.0, ry, zf), 0.005, 0.005, fw - 0.02, mats["brass_gold"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)
        # Upper bead (above beam, +X)
        ux = 0.058 if ri % 2 == 0 else 0.078
        add_cyl(f"{name}_BeadHi_{ri}", ab, (ux, ry, zf), 0.014, 0.014, 0.016, mats["wax_red"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)
        # 3 Lower beads (-X)
        for bi in range(3):
            lx = -0.072 + bi * 0.024 + (0.014 if (ri + bi) % 3 == 0 else 0.0)
            add_cyl(f"{name}_BeadLo_{ri}_{bi}", ab, (lx, ry, zf), 0.014, 0.014, 0.016, mats["honey_wood"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)
    return ab


def add_tea_set(name, parent, loc, mats, rot_z=0.0, col=None):
    """Dark wood serving tray with lathed ceramic teapot (spout, lid, handle) & 2 teacups at z=0."""
    ts = bpy.data.objects.new(name, None)
    ts.location = Vector(loc)
    ts.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        ts.parent = parent
    link_obj(ts, col)

    tw, tl = 0.25, 0.36
    add_box(f"{name}_TrayFloor", ts, (0, 0, 0.009), (tw, tl, 0.018), mats["walnut_dark"], col=col)
    for esgn, etag in ((-1.0, "L"), (1.0, "R")):
        add_box(f"{name}_TrayRimX_{etag}", ts, (esgn * tw * 0.47, 0, 0.022), (0.016, tl, 0.028), mats["wood_dark"], col=col)
        add_box(f"{name}_TrayRimY_{etag}", ts, (0, esgn * tl * 0.47, 0.022), (tw, 0.016, 0.028), mats["wood_dark"], col=col)

    # Lathed Ceramic Teapot at (0, -0.055, 0.018)
    px, py, pz = 0.0, -0.055, 0.018
    pot_prof = [
        (0.044, 0.0), (0.082, 0.045), (0.085, 0.082), (0.052, 0.118),
        (0.058, 0.125), (0.044, 0.145), (0.002, 0.152),
    ]
    add_lathe(f"{name}_Teapot", ts, (px, py, pz), pot_prof, mats["ceramic_sage"], segments=8, col=col)
    add_cyl(f"{name}_LidKnob", ts, (px, py, pz + 0.158), 0.013, 0.010, 0.018, mats["brass_gold"], verts=6, col=col)
    add_limb(f"{name}_Spout", ts, (px, py + 0.072, pz + 0.062), (px, py + 0.132, pz + 0.125), 0.016, 0.009, mats["ceramic_sage"], segments=6, col=col)
    add_limb(f"{name}_HandleHi", ts, (px, py - 0.054, pz + 0.105), (px, py - 0.118, pz + 0.085), 0.010, 0.010, mats["ceramic_sage"], segments=4, col=col)
    add_limb(f"{name}_HandleLo", ts, (px, py - 0.118, pz + 0.085), (px, py - 0.072, pz + 0.042), 0.010, 0.010, mats["ceramic_sage"], segments=4, col=col)

    cup_prof = [(0.020, 0.0), (0.032, 0.022), (0.035, 0.046), (0.028, 0.046), (0.020, 0.018)]
    for ci, (cx_c, cy_c) in enumerate(((-0.052, 0.105), (0.052, 0.105))):
        add_lathe(f"{name}_Cup_{ci}", ts, (cx_c, cy_c, pz), cup_prof, mats["pure_white"], segments=8, col=col)
        add_cyl(f"{name}_Tea_{ci}", ts, (cx_c, cy_c, pz + 0.036), 0.026, 0.026, 0.006, mats["walnut_dark"], verts=8, col=col)
    return ts


def add_wall_clock(name, parent, wall_loc, out_dir, mats, col=None):
    """Octagonal vintage pendulum clock mounted flush against wall_loc facing out_dir (+X local)."""
    wc = bpy.data.objects.new(name, None)
    wc.location = Vector(wall_loc)
    wc.rotation_euler = Euler((0, 0, math.atan2(out_dir[1], out_dir[0])))
    if parent:
        wc.parent = parent
    link_obj(wc, col)

    # Molded octagonal walnut clock head + lower pendulum box (back face at local x = -0.005)
    add_cyl(f"{name}_CaseHead", wc, (0.045, 0, 0.06), 0.275, 0.255, 0.10, mats["walnut_dark"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_box(f"{name}_PendBox", wc, (0.038, 0, -0.34), (0.086, 0.26, 0.42), mats["walnut_dark"], col=col)
    add_box(f"{name}_PendRecess", wc, (0.072, 0, -0.35), (0.028, 0.18, 0.34), mats["charcoal"], col=col)
    add_box(f"{name}_PendFinial", wc, (0.045, 0, -0.57), (0.105, 0.30, 0.045), mats["honey_wood"], col=col)

    # White dial face + hollow brass bezel + cardinal hour markers + hands (~10:10)
    add_cyl(f"{name}_DialFace", wc, (0.092, 0, 0.06), 0.195, 0.195, 0.016, mats["pure_white"], verts=12, rot=(0, math.pi * 0.5, 0), col=col)
    _add_hollow_ring(f"{name}_BrassBezel", wc, (0.098, 0, 0.06), 0.205, 0.014, mats["brass_gold"], rot_euler=(0, math.pi * 0.5, 0), segments=12, col=col)
    for hi, (hy_off, hz_off) in enumerate(((0, 0.155), (0.155, 0), (0, -0.155), (-0.155, 0))):
        add_box(f"{name}_Tick_{hi}", wc, (0.102, hy_off, 0.06 + hz_off), (0.008, 0.016, 0.026), mats["walnut_dark"], col=col)
    add_limb(f"{name}_HandHour", wc, (0.104, 0, 0.06), (0.104, -0.078, 0.06 + 0.055), 0.009, 0.005, mats["charcoal"], segments=4, col=col)
    add_limb(f"{name}_HandMin", wc, (0.106, 0, 0.06), (0.106, 0.105, 0.06 + 0.095), 0.007, 0.004, mats["charcoal"], segments=4, col=col)
    add_cyl(f"{name}_HandPin", wc, (0.107, 0, 0.06), 0.014, 0.014, 0.012, mats["brass_gold"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)

    # Visible brass pendulum rod & bob hanging inside lower box
    add_limb(f"{name}_PendRod", wc, (0.090, 0.018, -0.18), (0.090, 0.018, -0.44), 0.006, 0.006, mats["brass_gold"], segments=4, col=col)
    add_cyl(f"{name}_PendBob", wc, (0.092, 0.018, -0.44), 0.052, 0.052, 0.016, mats["brass_gold"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    return wc


def add_candlestick(name, parent, loc, mats, rot_z=0.0, col=None):
    """Brass chamber candlestick with saucer dish, finger loop, wax candle & glowing flame at z=0."""
    cs = bpy.data.objects.new(name, None)
    cs.location = Vector(loc)
    cs.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        cs.parent = parent
    link_obj(cs, col)

    cup_prof = [(0.072, 0.0), (0.080, 0.018), (0.032, 0.022), (0.024, 0.058), (0.034, 0.068)]
    add_lathe(f"{name}_BrassCup", cs, (0, 0, 0), cup_prof, mats["brass_gold"], segments=8, col=col)
    add_limb(f"{name}_RingTop", cs, (-0.070, 0, 0.018), (-0.108, 0, 0.048), 0.006, 0.006, mats["brass_gold"], segments=4, col=col)
    add_limb(f"{name}_RingBot", cs, (-0.108, 0, 0.048), (-0.066, 0, 0.010), 0.006, 0.006, mats["brass_gold"], segments=4, col=col)
    add_cyl(f"{name}_Wax", cs, (0, 0, 0.125), 0.020, 0.018, 0.13, mats["pure_white"], verts=8, col=col)
    add_box(f"{name}_Drip", cs, (0.019, 0, 0.145), (0.008, 0.010, 0.055), mats["pure_white"], col=col)
    add_cyl(f"{name}_Wick", cs, (0, 0, 0.194), 0.003, 0.002, 0.012, mats["charcoal"], verts=4, col=col)
    flame_prof = [(0.002, 0.198), (0.014, 0.215), (0.010, 0.232), (0.002, 0.248)]
    add_lathe(f"{name}_Flame", cs, (0, 0, 0), flame_prof, mats["warm_lantern"], segments=6, col=col)
    return cs


def add_chart_details(prefix, parent, loc, face_dir_y, width, height, style, mats, col=None):
    """Adds raised low-poly ink lines, compass crosses & caption bars so wall charts read clearly at a glance."""
    x, y, z = loc
    yf = y + face_dir_y * 0.018
    line_mat = mats["pure_white"] if style == "blueprint" else mats["walnut_dark"]
    acc_mat = mats["brass_gold"] if style == "blueprint" else mats["crimson_plank"]

    # Inner border margin lines
    for dz_b, btag in ((height * 0.38, "T"), (-height * 0.38, "B")):
        add_box(f"{prefix}_Line_{btag}", parent, (x, yf, z + dz_b), (width * 0.82, 0.006, 0.016), line_mat, col=col)
    # Schematic box + compass/grid mark on left, caption lines on top right
    add_box(f"{prefix}_DiagV", parent, (x - width * 0.18, yf, z + 0.02), (0.016, 0.006, height * 0.44), acc_mat, col=col)
    add_box(f"{prefix}_DiagH", parent, (x - width * 0.18, yf, z + 0.02), (width * 0.30, 0.006, 0.016), acc_mat, col=col)
    add_cyl(f"{prefix}_DiagNode", parent, (x - width * 0.18, yf, z + 0.02), 0.048, 0.048, 0.008, acc_mat, verts=8, rot=(math.pi * 0.5, 0, 0), col=col)
    for li, (lw, lz) in enumerate(((0.28, 0.16), (0.24, 0.08), (0.30, 0.00), (0.18, -0.08))):
        add_box(f"{prefix}_Row_{li}", parent, (x + width * 0.16, yf, z + lz), (lw * width, 0.006, 0.018), line_mat, col=col)


def add_bank_vault_door(name, parent, wall_loc, out_dir, mats, col=None):
    """Heavy reinforced steel vault door with hollow 6-spoke brass wheel, combo dial, hinges & bolts mounted flush at x=0, z=0."""
    vd = bpy.data.objects.new(name, None)
    vd.location = Vector(wall_loc)
    vd.rotation_euler = Euler((0, 0, math.atan2(out_dir[1], out_dir[0])))
    if parent:
        vd.parent = parent
    link_obj(vd, col)

    # Outer steel casing (extends -0.02..+0.14 in local X so back embeds cleanly into wall plaster)
    add_box(f"{name}_FrameOuter", vd, (0.06, 0.0, 1.29), (0.16, 2.18, 2.58), mats["charcoal"], col=col)
    add_box(f"{name}_DoorLeaf", vd, (0.13, 0.0, 1.25), (0.18, 1.84, 2.34), mats["slate_floor"], col=col)
    add_box(f"{name}_LeafInset", vd, (0.20, 0.0, 1.25), (0.05, 1.54, 2.04), mats["charcoal"], col=col)

    # Perimeter brass armor bolts around frame
    for bz in (0.24, 0.88, 1.54, 2.18, 2.46):
        for by_s in (-1.00, 1.00):
            add_cyl(f"{name}_Bolt_{by_s:.0f}_{bz:.2f}", vd, (0.145, by_s, bz), 0.024, 0.024, 0.022, mats["brass_gold"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)

    # Right hinge barrels (+Y) + 4 left locking slide bolts (-Y)
    for hz in (0.68, 1.88):
        add_cyl(f"{name}_Hinge_{hz:.2f}", vd, (0.21, 0.94, hz), 0.058, 0.058, 0.36, mats["charcoal"], verts=8, col=col)
        add_cyl(f"{name}_HingeCap_{hz:.2f}", vd, (0.21, 0.94, hz + 0.19), 0.062, 0.048, 0.04, mats["brass_gold"], verts=8, col=col)
        add_box(f"{name}_HingeStrap_{hz:.2f}", vd, (0.22, 0.68, hz), (0.03, 0.52, 0.11), mats["charcoal"], col=col)
    for lbz in (0.62, 1.05, 1.48, 1.91):
        add_cyl(f"{name}_LockBolt_{lbz:.2f}", vd, (0.19, -0.92, lbz), 0.028, 0.028, 0.16, mats["inox"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)

    # Sculpted hollow 6-spoke brass vault wheel + combination dial + plaque
    wy, wz = -0.08, 1.30
    _add_hollow_ring(f"{name}_WheelRim", vd, (0.29, wy, wz), 0.34, 0.024, mats["brass_gold"], rot_euler=(0, math.pi * 0.5, 0), segments=12, col=col)
    add_cyl(f"{name}_WheelHub", vd, (0.26, wy, wz), 0.082, 0.068, 0.09, mats["brass_gold"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl(f"{name}_WheelCap", vd, (0.31, wy, wz), 0.044, 0.044, 0.03, mats["copper"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    for si in range(6):
        ang = si * (math.pi / 3.0)
        py, pz = wy + math.cos(ang) * 0.335, wz + math.sin(ang) * 0.335
        add_limb(f"{name}_Spoke_{si}", vd, (0.27, wy, wz), (0.29, py, pz), 0.016, 0.016, mats["brass_gold"], segments=4, col=col)

    add_cyl(f"{name}_ComboDial", vd, (0.24, 0.44, 1.66), 0.090, 0.082, 0.036, mats["brass_gold"], verts=10, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl(f"{name}_ComboKnob", vd, (0.265, 0.44, 1.66), 0.045, 0.038, 0.032, mats["charcoal"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_box(f"{name}_Nameplate", vd, (0.23, 0.0, 2.04), (0.025, 0.48, 0.11), mats["brass_gold"], col=col)
    return vd


def add_coin_stacks_and_bell(name, parent, loc, mats, rot_z=0.0, col=None):
    """Banker desk coin stacks, tied coin pouch & reception brass bell resting flush at z=0."""
    cb = bpy.data.objects.new(name, None)
    cb.location = Vector(loc)
    cb.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        cb.parent = parent
    link_obj(cb, col)

    # 4 tiered coin stacks + 2 stray coins
    stack_specs = ((0.06, 0.05, 0.064, "brass_gold"), (0.11, 0.01, 0.088, "copper"), (0.04, -0.02, 0.042, "brass_gold"), (0.10, 0.08, 0.052, "brass_gold"))
    for si, (sx, sy, sh, sm) in enumerate(stack_specs):
        add_cyl(f"{name}_Stack_{si}", cb, (sx, sy, sh * 0.5), 0.028, 0.028, sh, mats[sm], verts=8, col=col)
        add_cyl(f"{name}_StackTop_{si}", cb, (sx, sy, sh + 0.002), 0.025, 0.025, 0.004, mats["copper"] if sm == "brass_gold" else mats["brass_gold"], verts=8, col=col)
    for ci, (cx, cy) in enumerate(((0.00, 0.09), (0.14, -0.05))):
        add_cyl(f"{name}_Coin_{ci}", cb, (cx, cy, 0.004), 0.024, 0.024, 0.008, mats["brass_gold"], verts=8, col=col)

    # Small tied coin pouch (z=0..0.115)
    pouch_prof = [(0.044, 0.0), (0.064, 0.036), (0.056, 0.065), (0.026, 0.090), (0.038, 0.112), (0.020, 0.114)]
    add_lathe(f"{name}_Pouch", cb, (0.03, -0.10, 0), pouch_prof, mats["burlap_tan"], segments=8, col=col)
    add_cyl(f"{name}_PouchTie", cb, (0.03, -0.10, 0.090), 0.030, 0.030, 0.012, mats["wax_red"], verts=8, col=col)

    # Brass counter service bell (-0.12, -0.04, 0)
    bx, by = -0.12, -0.04
    add_cyl(f"{name}_BellBase", cb, (bx, by, 0.008), 0.054, 0.052, 0.016, mats["charcoal"], verts=10, col=col)
    bell_prof = [(0.046, 0.016), (0.044, 0.032), (0.030, 0.050), (0.012, 0.060), (0.004, 0.062)]
    add_lathe(f"{name}_BellDome", cb, (bx, by, 0), bell_prof, mats["brass_gold"], segments=10, col=col)
    add_cyl(f"{name}_BellStem", cb, (bx, by, 0.070), 0.006, 0.006, 0.018, mats["inox"], verts=6, col=col)
    add_cyl(f"{name}_BellButton", cb, (bx, by, 0.082), 0.016, 0.016, 0.008, mats["brass_gold"], verts=8, col=col)
    return cb


def add_round_side_table(name, parent, loc, mats, height=0.72, radius=0.42, col=None):
    """Cozy octagonal corner side/tea table with turned pedestal and 4 splayed feet resting flush at z=0."""
    rt = bpy.data.objects.new(name, None)
    rt.location = Vector(loc)
    if parent:
        rt.parent = parent
    link_obj(rt, col)

    add_cyl(f"{name}_Top", rt, (0, 0, height - 0.025), radius, radius, 0.050, mats["honey_wood"], verts=10, col=col)
    add_cyl(f"{name}_Apron", rt, (0, 0, height - 0.070), radius * 0.86, radius * 0.86, 0.045, mats["walnut_dark"], verts=10, col=col)
    col_prof = [(0.058, 0.085), (0.044, 0.18), (0.062, 0.34), (0.038, 0.48), (0.054, height - 0.08)]
    add_lathe(f"{name}_Column", rt, (0, 0, 0), col_prof, mats["wood_dark"], segments=8, col=col)
    for fi in range(4):
        ang = fi * (math.pi * 0.5) + math.pi * 0.25
        fx, fy = math.cos(ang) * radius * 0.68, math.sin(ang) * radius * 0.68
        add_limb(f"{name}_Foot_{fi}", rt, (math.cos(ang) * 0.03, math.sin(ang) * 0.03, 0.12), (fx, fy, 0.008), 0.022, 0.016, mats["wood_dark"], segments=6, col=col)
        add_cyl(f"{name}_FootCap_{fi}", rt, (fx, fy, 0.010), 0.020, 0.018, 0.020, mats["brass_gold"], verts=6, col=col)
    return rt


def _add_trapezoid_ingot(name, parent, loc, length_x, width_y, height_z, mat, band_mat=None, rot_z=0.0, col=None):
    """Flat-shaded trapezoidal gold bullion ingot resting flush at loc z."""
    bm = bmesh.new()
    hx0, hy0 = length_x * 0.5, width_y * 0.5
    hx1, hy1 = length_x * 0.36, width_y * 0.34
    v0 = bm.verts.new((-hx0, -hy0, 0.0))
    v1 = bm.verts.new((hx0, -hy0, 0.0))
    v2 = bm.verts.new((hx0, hy0, 0.0))
    v3 = bm.verts.new((-hx0, hy0, 0.0))
    u0 = bm.verts.new((-hx1, -hy1, height_z))
    u1 = bm.verts.new((hx1, -hy1, height_z))
    u2 = bm.verts.new((hx1, hy1, height_z))
    u3 = bm.verts.new((-hx1, hy1, height_z))
    bm.faces.new((v3, v2, v1, v0))
    bm.faces.new((u0, u1, u2, u3))
    bm.faces.new((v0, v1, u1, u0))
    bm.faces.new((v1, v2, u2, u1))
    bm.faces.new((v2, v3, u3, u2))
    bm.faces.new((v3, v0, u0, u3))
    obj = _finalize_bm(name, parent, bm, loc, (0, 0, rot_z), mat, col)
    if band_mat is not None:
        add_box(f"{name}_Stamp", obj, (0, 0, height_z + 0.0015), (length_x * 0.38, width_y * 0.32, 0.003), band_mat, col=col)
    return obj


def add_gold_bullion_stack(name, parent, loc, mats, rot_z=0.0, col=None):
    """Pyramid stack of 6 beveled gold bullion ingots resting flush at z=0."""
    gs = bpy.data.objects.new(name, None)
    gs.location = Vector(loc)
    gs.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        gs.parent = parent
    link_obj(gs, col)

    bw, bl, bh = 0.068, 0.145, 0.036
    # Bottom row (3), middle row (2), top bar (1)
    for bi, by in enumerate((-0.076, 0.0, 0.076)):
        _add_trapezoid_ingot(f"{name}_Bot_{bi}", gs, (0, by, 0.0), bl, bw, bh, mats["brass_gold"], mats["copper"], col=col)
    for mi, by in enumerate((-0.038, 0.038)):
        _add_trapezoid_ingot(f"{name}_Mid_{mi}", gs, (0, by, bh), bl, bw, bh, mats["brass_gold"], mats["copper"], col=col)
    _add_trapezoid_ingot(f"{name}_Top", gs, (0, 0.0, bh * 2.0), bl, bw, bh, mats["brass_gold"], mats["copper"], col=col)
    return gs


def add_cash_briefcase(name, parent, loc, mats, rot_z=0.0, is_open=True, col=None):
    """Matte-black mafia briefcase stuffed with strapped green cash bundles resting flush at z=0."""
    bc = bpy.data.objects.new(name, None)
    bc.location = Vector(loc)
    bc.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        bc.parent = parent
    link_obj(bc, col)

    # Lower shell + brass corner trim & front latches
    add_box(f"{name}_BaseShell", bc, (0, 0, 0.042), (0.30, 0.42, 0.084), mats["charcoal"], col=col)
    add_box(f"{name}_VelvetLining", bc, (0, 0, 0.080), (0.26, 0.38, 0.012), mats["crimson_velvet"], col=col)
    for cy in (-0.195, 0.195):
        for cx in (-0.135, 0.135):
            add_box(f"{name}_Corner_{cx:.2f}_{cy:.2f}", bc, (cx, cy, 0.042), (0.042, 0.042, 0.090), mats["brass_gold"], col=col)
    for ly in (-0.11, 0.11):
        add_box(f"{name}_Clasp_{ly:.2f}", bc, (0.154, ly, 0.068), (0.014, 0.036, 0.032), mats["brass_gold"], col=col)
    add_box(f"{name}_Grip", bc, (0.168, 0.0, 0.042), (0.026, 0.11, 0.022), mats["wood_dark"], col=col)

    # 6 stacked strapped cash bundles inside (+X/+Y grid) + 2 loose fanned bills
    idx = 0
    for rx in (-0.062, 0.062):
        for ry in (-0.115, 0.0, 0.115):
            add_box(f"{name}_Cash_{idx}", bc, (rx, ry, 0.098), (0.105, 0.064, 0.026), mats["emerald_shutter"], rot=(0, 0, 0.05 * (idx % 3 - 1)), col=col)
            add_box(f"{name}_CashBand_{idx}", bc, (rx, ry, 0.099), (0.024, 0.066, 0.028), mats["pure_white"], rot=(0, 0, 0.05 * (idx % 3 - 1)), col=col)
            idx += 1
    add_box(f"{name}_BillFanA", bc, (0.02, -0.04, 0.115), (0.105, 0.060, 0.004), mats["emerald_shutter"], rot=(0.06, -0.05, 0.35), col=col)
    add_box(f"{name}_BillFanB", bc, (-0.02, 0.06, 0.116), (0.105, 0.060, 0.004), mats["emerald_shutter"], rot=(-0.05, 0.04, -0.28), col=col)

    # Hinged upper lid anchored at rear edge (-0.15, 0, 0.084)
    lid_grp = bpy.data.objects.new(f"{name}_LidGrp", None)
    lid_grp.location = Vector((-0.15, 0.0, 0.084))
    lid_grp.rotation_euler = Euler((0.0, -1.24 if is_open else 0.0, 0.0))
    lid_grp.parent = bc
    link_obj(lid_grp, col)
    add_box(f"{name}_LidOuter", lid_grp, (0.15, 0.0, 0.018), (0.30, 0.42, 0.036), mats["charcoal"], col=col)
    add_box(f"{name}_LidInner", lid_grp, (0.15, 0.0, 0.004), (0.26, 0.38, 0.012), mats["crimson_velvet"], col=col)
    for hy in (-0.13, 0.13):
        add_cyl(f"{name}_Hinge_{hy:.2f}", bc, (-0.152, hy, 0.084), 0.010, 0.010, 0.036, mats["brass_gold"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)
    return bc


def add_whiskey_and_cigar_set(name, parent, loc, mats, rot_z=0.0, col=None):
    """Dark wood tray with cut-glass whiskey decanter, 2 bourbon tumblers & brass ashtray with lit cigar at z=0."""
    wc = bpy.data.objects.new(name, None)
    wc.location = Vector(loc)
    wc.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        wc.parent = parent
    link_obj(wc, col)

    tw, tl = 0.25, 0.36
    add_box(f"{name}_Tray", wc, (0, 0, 0.009), (tw, tl, 0.018), mats["walnut_dark"], col=col)
    for sgn, ttag in ((-1.0, "L"), (1.0, "R")):
        add_box(f"{name}_RimX_{ttag}", wc, (sgn * tw * 0.47, 0, 0.020), (0.015, tl, 0.024), mats["wood_dark"], col=col)
        add_box(f"{name}_RimY_{ttag}", wc, (0, sgn * tl * 0.47, 0.020), (tw, 0.015, 0.024), mats["wood_dark"], col=col)

    # Crystal Bourbon Decanter (-0.02, -0.075, 0.018)
    dx, dy, dz = -0.02, -0.075, 0.018
    add_box(f"{name}_DecBase", wc, (dx, dy, dz + 0.009), (0.095, 0.095, 0.018), mats["glass_sky"], col=col)
    add_box(f"{name}_DecBody", wc, (dx, dy, dz + 0.076), (0.088, 0.088, 0.118), mats["amber_glass"], col=col)
    add_cyl(f"{name}_DecNeck", wc, (dx, dy, dz + 0.155), 0.026, 0.018, 0.040, mats["glass_sky"], verts=8, col=col)
    add_cyl(f"{name}_DecLip", wc, (dx, dy, dz + 0.176), 0.024, 0.024, 0.010, mats["brass_gold"], verts=8, col=col)
    add_ico(f"{name}_DecStopper", wc, (dx, dy, dz + 0.200), (0.024, 0.024, 0.024), mats["glass_sky"], subdivisions=1, col=col)

    # 2 Lowball Tumblers filled with amber whiskey
    for gi, (gx, gy) in enumerate(((0.048, 0.055), (-0.048, 0.100))):
        add_cyl(f"{name}_Glass_{gi}", wc, (gx, gy, dz + 0.036), 0.030, 0.033, 0.072, mats["glass_sky"], verts=8, col=col)
        add_cyl(f"{name}_Liquid_{gi}", wc, (gx, gy, dz + 0.024), 0.024, 0.025, 0.038, mats["amber_glass"], verts=8, col=col)

    # Brass Ashtray & Glowing Cigar (+0.055, -0.095, 0.018)
    ax, ay = 0.055, -0.095
    add_cyl(f"{name}_Ashtray", wc, (ax, ay, dz + 0.010), 0.046, 0.050, 0.020, mats["brass_gold"], verts=8, col=col)
    add_cyl(f"{name}_AshPool", wc, (ax, ay, dz + 0.019), 0.036, 0.036, 0.004, mats["charcoal"], verts=8, col=col)
    add_limb(f"{name}_Cigar", wc, (ax + 0.042, ay + 0.032, dz + 0.022), (ax - 0.018, ay - 0.016, dz + 0.025), 0.008, 0.007, mats["wood_dark"], segments=6, col=col)
    add_ico(f"{name}_CigarEmber", wc, (ax - 0.022, ay - 0.019, dz + 0.025), (0.007, 0.007, 0.007), mats["lantern_red"], subdivisions=1, col=col)
    return wc


def add_poker_table(name, parent, loc, mats, height=0.76, radius=0.62, col=None):
    """Octagonal underworld gambling poker table with green baize felt, chip stacks, cards & whiskey glass at z=0."""
    pt = bpy.data.objects.new(name, None)
    pt.location = Vector(loc)
    if parent:
        pt.parent = parent
    link_obj(pt, col)

    # Heavy walnut pedestal + 4 splayed claw feet
    col_prof = [(0.080, 0.10), (0.062, 0.26), (0.088, 0.44), (0.056, 0.58), (0.110, height - 0.08)]
    add_lathe(f"{name}_Column", pt, (0, 0, 0), col_prof, mats["walnut_dark"], segments=8, col=col)
    for fi in range(4):
        ang = fi * (math.pi * 0.5) + math.pi * 0.25
        fx, fy = math.cos(ang) * radius * 0.66, math.sin(ang) * radius * 0.66
        add_limb(f"{name}_Foot_{fi}", pt, (math.cos(ang) * 0.05, math.sin(ang) * 0.05, 0.14), (fx, fy, 0.010), 0.032, 0.022, mats["walnut_dark"], segments=6, col=col)
        add_cyl(f"{name}_FootCap_{fi}", pt, (fx, fy, 0.012), 0.026, 0.024, 0.024, mats["brass_gold"], verts=6, col=col)

    # Leather base deck + recessed-clear emerald felt + raised hollow 8-sided walnut bumper rim
    add_cyl(f"{name}_Apron", pt, (0, 0, height - 0.078), radius * 0.90, radius * 0.90, 0.052, mats["wood_dark"], verts=8, col=col)
    add_cyl(f"{name}_LeatherRim", pt, (0, 0, height - 0.040), radius, radius * 1.02, 0.056, mats["walnut_dark"], verts=8, col=col)
    zf = height - 0.002
    add_cyl(f"{name}_GreenFelt", pt, (0, 0, zf - 0.008), radius * 0.88, radius * 0.88, 0.016, mats["emerald_shutter"], verts=8, col=col)
    _add_hollow_ring(f"{name}_BrassTrim", pt, (0, 0, zf - 0.002), radius * 0.875, 0.014, mats["brass_gold"], segments=8, col=col)
    _add_hollow_ring(f"{name}_Bumper", pt, (0, 0, height + 0.010), radius * 0.945, 0.036, mats["walnut_dark"], segments=8, col=col)

    # Poker chip stacks & playing cards & center pot + whiskey tumbler on felt
    chip_specs = (
        (-0.24, -0.22, 0.065, "plastic_red"),
        (-0.18, -0.28, 0.048, "pure_white"),
        (0.24, -0.22, 0.072, "plastic_blue"),
        (0.18, -0.28, 0.040, "brass_gold"),
        (-0.02, 0.02, 0.055, "plastic_red"),
        (0.05, -0.02, 0.045, "plastic_blue"),
        (0.02, 0.28, 0.058, "pure_white"),
    )
    for ci, (cx, cy, ch, cm) in enumerate(chip_specs):
        add_cyl(f"{name}_Chips_{ci}", pt, (cx, cy, zf + ch * 0.5), 0.032, 0.032, ch, mats[cm], verts=8, col=col)
        add_cyl(f"{name}_ChipTop_{ci}", pt, (cx, cy, zf + ch + 0.002), 0.026, 0.026, 0.004, mats["brass_gold"] if cm != "brass_gold" else mats["pure_white"], verts=8, col=col)

    for ki, (kx, ky, krot) in enumerate(((-0.22, -0.10, 0.25), (-0.16, -0.08, -0.18), (0.20, -0.10, 0.42), (0.25, -0.07, -0.12), (-0.04, 0.16, 0.10), (0.04, 0.17, -0.25))):
        add_box(f"{name}_Card_{ki}", pt, (kx, ky, zf + 0.003), (0.055, 0.082, 0.004), mats["pure_white"], rot=(0, 0, krot), col=col)
        add_box(f"{name}_CardPip_{ki}", pt, (kx, ky, zf + 0.006), (0.018, 0.022, 0.002), mats["wax_red"] if ki % 2 == 0 else mats["charcoal"], rot=(0, 0, krot), col=col)

    add_cyl(f"{name}_RumGlass", pt, (-0.34, 0.12, zf + 0.036), 0.030, 0.033, 0.072, mats["glass_sky"], verts=8, col=col)
    add_cyl(f"{name}_RumFill", pt, (-0.34, 0.12, zf + 0.024), 0.024, 0.025, 0.036, mats["amber_glass"], verts=8, col=col)
    add_cyl(f"{name}_Ashtray", pt, (0.34, 0.12, zf + 0.010), 0.044, 0.046, 0.020, mats["brass_gold"], verts=8, col=col)
    return pt


def add_weapon_rack(name, parent, wall_loc, face_dir_y, mats, col=None):
    """Mahogany wall weapon rack displaying studded bat, ceremonial machete & iron crowbar flush on wall_loc."""
    wr = bpy.data.objects.new(name, None)
    wr.location = Vector(wall_loc)
    wr.rotation_euler = Euler((0, 0, 0.0 if face_dir_y >= 0.0 else math.pi))
    if parent:
        wr.parent = parent
    link_obj(wr, col)

    # Local +Y = into the room; back face embeds cleanly at y = -0.005
    add_box(f"{name}_Plaque", wr, (0, 0.020, 0.0), (1.34, 0.05, 0.78), mats["walnut_dark"], col=col)
    add_box(f"{name}_Velvet", wr, (0, 0.044, 0.0), (1.20, 0.016, 0.64), mats["crimson_velvet"], col=col)
    for cx, cz in ((-0.62, 0.34), (0.62, 0.34), (-0.62, -0.34), (0.62, -0.34)):
        add_cyl(f"{name}_Stud_{cx:.2f}_{cz:.2f}", wr, (cx, 0.048, cz), 0.022, 0.022, 0.016, mats["brass_gold"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)

    wy = 0.092
    for hx, hz in ((-0.34, 0.20), (0.30, 0.20), (-0.32, 0.0), (0.30, 0.0), (-0.26, -0.20), (0.28, -0.20)):
        add_limb(f"{name}_Hook_{hx:.2f}_{hz:.2f}", wr, (hx, 0.044, hz), (hx, wy + 0.022, hz + 0.014), 0.010, 0.009, mats["brass_gold"], segments=4, col=col)

    # 1. Top: Turned & Iron-Banded Mafia Baseball Bat (along X, barrel at +X)
    bat_prof = [(0.026, -0.44), (0.017, -0.40), (0.016, -0.18), (0.028, 0.05), (0.042, 0.36), (0.040, 0.44), (0.002, 0.46)]
    add_lathe(f"{name}_Bat", wr, (0.0, wy, 0.205), bat_prof, mats["honey_wood"], segments=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl(f"{name}_BatBand", wr, (0.26, wy, 0.205), 0.041, 0.039, 0.06, mats["charcoal"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl(f"{name}_BatGrip", wr, (-0.28, wy, 0.205), 0.018, 0.018, 0.22, mats["wax_red"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)

    # 2. Middle: Ceremonial Syndicate Blade / Machete
    add_box(f"{name}_BladeBody", wr, (0.08, wy, 0.005), (0.64, 0.014, 0.068), mats["inox"], col=col)
    add_cyl(f"{name}_BladeTip", wr, (0.43, wy, 0.005), 0.036, 0.002, 0.10, mats["inox"], verts=4, rot=(0, math.pi * 0.5, 0), col=col)
    add_box(f"{name}_Guard", wr, (-0.24, wy, 0.005), (0.026, 0.032, 0.145), mats["brass_gold"], col=col)
    add_cyl(f"{name}_Hilt", wr, (-0.35, wy, 0.005), 0.020, 0.018, 0.20, mats["wax_red"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_ico(f"{name}_Pommel", wr, (-0.46, wy, 0.005), (0.026, 0.024, 0.028), mats["brass_gold"], subdivisions=1, col=col)

    # 3. Bottom: Heavy Forged Steel Crowbar
    add_cyl(f"{name}_CrowShank", wr, (0.02, wy, -0.20), 0.016, 0.016, 0.62, mats["charcoal"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)
    add_limb(f"{name}_CrowNeck", wr, (-0.28, wy, -0.20), (-0.38, wy, -0.11), 0.016, 0.014, mats["charcoal"], segments=6, col=col)
    add_limb(f"{name}_CrowHook", wr, (-0.38, wy, -0.11), (-0.44, wy, -0.18), 0.014, 0.007, mats["inox"], segments=4, col=col)
    add_limb(f"{name}_CrowPry", wr, (0.33, wy, -0.20), (0.42, wy, -0.23), 0.016, 0.006, mats["inox"], segments=4, col=col)
    return wr


def add_boss_throne_chair(name, parent, loc, mats, rot_z=0.0, col=None):
    """High-back mafia boss executive armchair in dark walnut & crimson velvet facing +X at z=0."""
    ch = bpy.data.objects.new(name, None)
    ch.location = Vector(loc)
    ch.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        ch.parent = parent
    link_obj(ch, col)

    for lx in (-0.23, 0.23):
        for ly in (-0.24, 0.24):
            add_cyl(f"{name}_Leg_{lx:.2f}_{ly:.2f}", ch, (lx, ly, 0.21), 0.028, 0.034, 0.42, mats["walnut_dark"], verts=6, col=col)
            add_cyl(f"{name}_Foot_{lx:.2f}_{ly:.2f}", ch, (lx, ly, 0.015), 0.030, 0.028, 0.030, mats["brass_gold"], verts=6, col=col)
    add_box(f"{name}_SeatFrame", ch, (0.0, 0.0, 0.44), (0.54, 0.56, 0.07), mats["walnut_dark"], col=col)
    add_box(f"{name}_SeatCushion", ch, (0.02, 0.0, 0.505), (0.48, 0.50, 0.08), mats["crimson_velvet"], col=col)

    # Tall tufted backrest (-X side facing +X) + Brass Crown Studs
    add_box(f"{name}_BackFrame", ch, (-0.24, 0.0, 0.88), (0.08, 0.58, 0.82), mats["walnut_dark"], col=col)
    add_box(f"{name}_BackPad", ch, (-0.19, 0.0, 0.88), (0.06, 0.48, 0.68), mats["crimson_velvet"], col=col)
    add_box(f"{name}_CrownBar", ch, (-0.24, 0.0, 1.31), (0.095, 0.62, 0.065), mats["brass_gold"], col=col)
    for ay in (-0.26, 0.26):
        add_box(f"{name}_ArmPost_{ay:.2f}", ch, (0.20, ay, 0.56), (0.06, 0.06, 0.24), mats["walnut_dark"], col=col)
        add_box(f"{name}_ArmRest_{ay:.2f}", ch, (-0.01, ay, 0.69), (0.50, 0.075, 0.055), mats["walnut_dark"], col=col)
        add_box(f"{name}_ArmPad_{ay:.2f}", ch, (-0.01, ay, 0.72), (0.42, 0.065, 0.025), mats["crimson_velvet"], col=col)
    return ch


def add_floor_safe(name, parent, loc, mats, rot_z=0.0, col=None):
    """Heavy steel floor safe with brass 3-spoke wheel, combo dial & flat top (exact z=0.720) at z=0."""
    fs = bpy.data.objects.new(name, None)
    fs.location = Vector(loc)
    fs.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        fs.parent = parent
    link_obj(fs, col)

    for fx in (-0.22, 0.22):
        for fy in (-0.22, 0.22):
            add_cyl(f"{name}_Foot_{fx:.2f}_{fy:.2f}", fs, (fx, fy, 0.022), 0.042, 0.048, 0.044, mats["charcoal"], verts=8, col=col)
    add_box(f"{name}_Body", fs, (0, 0, 0.382), (0.54, 0.54, 0.676), mats["charcoal"], col=col)
    add_box(f"{name}_Door", fs, (0.275, 0, 0.382), (0.04, 0.44, 0.58), mats["slate_floor"], col=col)
    add_cyl(f"{name}_Dial", fs, (0.30, 0.08, 0.52), 0.045, 0.042, 0.024, mats["brass_gold"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl(f"{name}_HandleHub", fs, (0.30, -0.06, 0.36), 0.036, 0.032, 0.036, mats["brass_gold"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    for si in range(3):
        ang = si * (2.0 * math.pi / 3.0)
        add_limb(f"{name}_Spoke_{si}", fs, (0.312, -0.06, 0.36), (0.312, -0.06 + math.cos(ang) * 0.11, 0.36 + math.sin(ang) * 0.11), 0.011, 0.009, mats["brass_gold"], segments=4, col=col)
    for hz in (0.20, 0.54):
        add_cyl(f"{name}_Hinge_{hz:.2f}", fs, (0.275, 0.23, hz), 0.024, 0.024, 0.09, mats["brass_gold"], verts=6, col=col)
    return fs


def add_artificer_forge(name, parent, loc, mats, rot_z=0.0, col=None):
    """Brick/stone artificer hearth with glowing ember bed, tapered chimney hood, flue, bellows & tongs at z=0."""
    fg = bpy.data.objects.new(name, None)
    fg.location = Vector(loc)
    fg.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        fg.parent = parent
    link_obj(fg, col)

    # Lower stone masonry hearth plinth (0.90 x 1.50 x 0.90) with dark front coal arch
    for py_sgn, ptag in ((-1.0, "L"), (1.0, "R")):
        add_box(f"{name}_Pier_{ptag}", fg, (0.02, py_sgn * 0.54, 0.44), (0.86, 0.40, 0.88), mats["curb_stone"], col=col)
    add_box(f"{name}_PlinthBack", fg, (-0.22, 0.0, 0.44), (0.40, 0.68, 0.88), mats["terracotta_dark"], col=col)
    add_box(f"{name}_CoalRecess", fg, (0.04, 0.0, 0.36), (0.36, 0.64, 0.56), mats["walnut_dark"], col=col)
    for ci, (cx, cy) in enumerate(((0.12, -0.14), (0.16, 0.04), (0.08, 0.16))):
        add_ico(f"{name}_CoalLump_{ci}", fg, (cx, cy, 0.12), (0.12, 0.11, 0.09), mats["charcoal"], subdivisions=1, col=col)
    add_box(f"{name}_HearthDeck", fg, (0.04, 0.0, 0.90), (0.94, 1.56, 0.08), mats["slate_floor"], col=col)

    # Recessed fire pit with glowing red/amber coals
    add_box(f"{name}_PitRim", fg, (0.08, 0.0, 0.95), (0.64, 0.82, 0.06), mats["charcoal"], col=col)
    add_box(f"{name}_EmberBed", fg, (0.08, 0.0, 0.96), (0.50, 0.68, 0.04), mats["lantern_red"], col=col)
    for ei, (ex, ey, ez, em) in enumerate((
        (0.06, -0.14, 0.99, "warm_lantern"), (0.14, 0.02, 1.00, "lantern_red"),
        (-0.02, 0.14, 0.99, "warm_lantern"), (0.10, -0.06, 1.01, "warm_lantern"),
        (-0.04, -0.02, 0.99, "charcoal"), (0.12, 0.18, 0.98, "charcoal"),
    )):
        add_ico(f"{name}_Coal_{ei}", fg, (ex, ey, ez), (0.09, 0.08, 0.06), mats[em], subdivisions=1, col=col)

    # Tapered charcoal hood & rear masonry/iron chimney posts up through 2nd floor ceiling
    for sy_p, htag in ((-0.52, "L"), (0.52, "R")):
        add_box(f"{name}_HoodPost_{htag}", fg, (-0.18, sy_p, 1.32), (0.12, 0.12, 0.80), mats["charcoal"], col=col)
    add_cyl(f"{name}_Hood", fg, (-0.02, 0.0, 2.02), 0.72, 0.34, 0.78, mats["charcoal"], verts=4, rot=(0, 0, math.pi * 0.25), col=col)
    add_box(f"{name}_Flue", fg, (-0.18, 0.0, 4.18), (0.50, 0.56, 3.60), mats["slate_floor"], col=col)
    add_box(f"{name}_LoftCollar", fg, (-0.18, 0.0, 3.04), (0.58, 0.64, 0.10), mats["brass_gold"], col=col)

    # Leather & honey-wood accordion forge bellows on left (-Y) + hanging tongs on right (+Y)
    add_box(f"{name}_BelPadB", fg, (0.04, -0.96, 0.74), (0.36, 0.44, 0.03), mats["honey_wood"], rot=(0.24, 0, 0), col=col)
    add_box(f"{name}_BelPadT", fg, (0.04, -0.96, 0.92), (0.36, 0.44, 0.03), mats["honey_wood"], rot=(-0.20, 0, 0), col=col)
    add_box(f"{name}_BelBelly", fg, (0.04, -0.96, 0.83), (0.32, 0.38, 0.16), mats["crimson_dark"], col=col)
    add_limb(f"{name}_BelNozzle", fg, (0.04, -0.76, 0.86), (0.08, -0.34, 0.94), 0.030, 0.018, mats["brass_gold"], segments=6, col=col)
    add_box(f"{name}_BelStand", fg, (0.04, -0.96, 0.38), (0.14, 0.14, 0.76), mats["wood_dark"], col=col)

    add_limb(f"{name}_TongL", fg, (0.28, 0.62, 0.90), (0.28, 0.62, 0.34), 0.011, 0.008, mats["charcoal"], segments=4, col=col)
    add_limb(f"{name}_TongR", fg, (0.28, 0.68, 0.90), (0.28, 0.68, 0.34), 0.011, 0.008, mats["charcoal"], segments=4, col=col)
    return fg


def add_blacksmith_anvil(name, parent, loc, mats, rot_z=0.0, col=None):
    """Turned oak stump + horned steel anvil + resting smith hammer & water quench bucket at z=0."""
    av = bpy.data.objects.new(name, None)
    av.location = Vector(loc)
    av.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        av.parent = parent
    link_obj(av, col)

    stump_prof = [(0.26, 0.0), (0.25, 0.24), (0.24, 0.56), (0.001, 0.56)]
    add_lathe(f"{name}_Stump", av, (0, 0, 0), stump_prof, mats["wood_dark"], segments=8, col=col)
    for hz in (0.14, 0.46):
        add_cyl(f"{name}_StumpHoop_{hz:.2f}", av, (0, 0, hz), 0.258, 0.258, 0.036, mats["charcoal"], verts=8, col=col)

    # Anvil foot, narrow waist, broad steel top table, tapered conical horn (+X) & square heel (-X)
    add_box(f"{name}_AnvFoot", av, (0, 0, 0.59), (0.34, 0.24, 0.06), mats["charcoal"], col=col)
    add_box(f"{name}_AnvWaist", av, (0, 0, 0.68), (0.20, 0.14, 0.14), mats["charcoal"], col=col)
    add_box(f"{name}_AnvBody", av, (-0.02, 0, 0.81), (0.38, 0.19, 0.14), mats["inox"], col=col)
    add_cyl(f"{name}_AnvHorn", av, (0.26, 0, 0.82), 0.068, 0.012, 0.24, mats["inox"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_box(f"{name}_AnvHeel", av, (-0.23, 0, 0.84), (0.10, 0.16, 0.08), mats["charcoal"], col=col)
    # Resting forging hammer on anvil face (z=0.88)
    add_box(f"{name}_HammerHead", av, (-0.02, 0.04, 0.905), (0.055, 0.125, 0.050), mats["charcoal"], rot=(0, 0, 0.28), col=col)
    add_limb(f"{name}_HammerHandle", av, (-0.02, 0.04, 0.90), (-0.18, -0.10, 0.88), 0.014, 0.012, mats["honey_wood"], segments=6, col=col)

    # Stave water quench tub beside stump (0, 0.48, 0)
    qb_prof = [(0.17, 0.0), (0.205, 0.38), (0.185, 0.38), (0.155, 0.30), (0.001, 0.30)]
    add_lathe(f"{name}_QuenchTub", av, (0, 0.48, 0), qb_prof, mats["honey_wood"], segments=8, col=col)
    for qz in (0.10, 0.30):
        add_cyl(f"{name}_TubHoop_{qz:.2f}", av, (0, 0.48, qz), 0.198, 0.202, 0.028, mats["charcoal"], verts=8, col=col)
    add_cyl(f"{name}_Water", av, (0, 0.48, 0.32), 0.175, 0.175, 0.016, mats["glass_sky"], verts=8, col=col)
    return av


def add_drafting_table(name, parent, loc, mats, rot_z=0.0, col=None):
    """Slanted artificer drafting table with raised-line blueprint, brass T-square & side tray at z=0."""
    dt = bpy.data.objects.new(name, None)
    dt.location = Vector(loc)
    dt.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        dt.parent = parent
    link_obj(dt, col)

    # Trestle legs grounded at z=0 (+X is lower front toward seated artificer)
    for sy in (-0.56, 0.56):
        add_limb(f"{name}_LegF_{sy:.2f}", dt, (0.28, sy, 0.0), (0.10, sy, 0.80), 0.028, 0.026, mats["wood_dark"], segments=6, col=col)
        add_limb(f"{name}_LegB_{sy:.2f}", dt, (-0.28, sy, 0.0), (-0.10, sy, 0.86), 0.028, 0.026, mats["wood_dark"], segments=6, col=col)
        add_box(f"{name}_FootBar_{sy:.2f}", dt, (0.0, sy, 0.22), (0.48, 0.045, 0.045), mats["wood_dark"], col=col)
        add_cyl(f"{name}_TiltKnob_{sy:.2f}", dt, (0.0, sy + (0.04 if sy > 0 else -0.04), 0.82), 0.032, 0.032, 0.035, mats["brass_gold"], verts=8, rot=(math.pi * 0.5, 0, 0), col=col)
    add_box(f"{name}_Stretcher", dt, (0.0, 0.0, 0.22), (0.06, 1.12, 0.05), mats["wood_dark"], col=col)

    # Slanted drafting board group tilted around Y so front (+X) dips down cleanly
    brd = bpy.data.objects.new(f"{name}_BoardGrp", None)
    brd.location = Vector((0.0, 0.0, 0.88))
    brd.rotation_euler = Euler((0.0, 0.22, 0.0))
    brd.parent = dt
    link_obj(brd, col)

    add_box(f"{name}_Board", brd, (0, 0, 0), (0.84, 1.42, 0.042), mats["honey_wood"], col=col)
    add_box(f"{name}_StopLedge", brd, (0.40, 0, 0.032), (0.035, 1.42, 0.028), mats["wood_dark"], col=col)
    add_box(f"{name}_Blueprint", brd, (0.02, -0.08, 0.024), (0.64, 0.94, 0.006), mats["blueprint_blue"], col=col)
    add_box(f"{name}_BPBorderT", brd, (-0.26, -0.08, 0.029), (0.012, 0.84, 0.004), mats["pure_white"], col=col)
    add_box(f"{name}_BPBorderB", brd, (0.30, -0.08, 0.029), (0.012, 0.84, 0.004), mats["pure_white"], col=col)
    add_box(f"{name}_BPCrossH", brd, (0.02, -0.18, 0.029), (0.44, 0.012, 0.004), mats["pure_white"], col=col)
    add_box(f"{name}_BPCrossV", brd, (0.02, -0.18, 0.029), (0.012, 0.44, 0.004), mats["pure_white"], col=col)
    add_cyl(f"{name}_BPGearRing", brd, (0.02, -0.18, 0.029), 0.14, 0.14, 0.004, mats["pure_white"], verts=10, col=col)
    add_cyl(f"{name}_BPGearHole", brd, (0.02, -0.18, 0.030), 0.10, 0.10, 0.004, mats["blueprint_blue"], verts=10, col=col)
    add_box(f"{name}_TSquareHead", brd, (0.04, -0.52, 0.032), (0.28, 0.035, 0.010), mats["brass_gold"], col=col)
    add_box(f"{name}_TSquareBlade", brd, (0.04, -0.16, 0.032), (0.035, 0.72, 0.008), mats["brass_gold"], col=col)
    add_scroll_bundle(f"{name}_Roll", brd, (0.0, 0.52, 0.021), 0.46, 0.045, mats["parchment"], mats, rot_z=(math.pi * 0.5 + 0.08), col=col)
    return dt


def add_brass_telescope(name, parent, loc, mats, rot_z=0.0, col=None):
    """Astronomical brass telescope on 3-legged wooden tripod aimed +X pitched upward at z=0."""
    ts = bpy.data.objects.new(name, None)
    ts.location = Vector(loc)
    ts.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        ts.parent = parent
    link_obj(ts, col)

    for li in range(3):
        ang = li * (2.0 * math.pi / 3.0)
        fx, fy = math.cos(ang) * 0.34, math.sin(ang) * 0.34
        add_limb(f"{name}_Leg_{li}", ts, (math.cos(ang) * 0.04, math.sin(ang) * 0.04, 1.06), (fx, fy, 0.0), 0.022, 0.014, mats["wood_dark"], segments=6, col=col)
        add_cyl(f"{name}_Tip_{li}", ts, (fx, fy, 0.016), 0.018, 0.015, 0.032, mats["brass_gold"], verts=6, col=col)
    add_cyl(f"{name}_Hub", ts, (0, 0, 1.08), 0.068, 0.056, 0.08, mats["brass_gold"], verts=8, col=col)
    add_box(f"{name}_Yoke", ts, (0, 0, 1.17), (0.06, 0.14, 0.11), mats["brass_gold"], col=col)

    tube_grp = bpy.data.objects.new(f"{name}_TubeGrp", None)
    tube_grp.location = Vector((0.0, 0.0, 1.22))
    tube_grp.rotation_euler = Euler((0.0, -0.22, 0.0))
    tube_grp.parent = ts
    link_obj(tube_grp, col)

    tube_prof = [
        (0.016, -0.44), (0.024, -0.34), (0.042, -0.30), (0.044, -0.06),
        (0.056, 0.00), (0.058, 0.28), (0.072, 0.34), (0.074, 0.50),
        (0.062, 0.50), (0.060, 0.44),
    ]
    add_lathe(f"{name}_Barrel", tube_grp, (0, 0, 0), tube_prof, mats["brass_gold"], segments=10, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl(f"{name}_GripBand", tube_grp, (-0.18, 0, 0), 0.048, 0.048, 0.18, mats["copper"], verts=10, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl(f"{name}_ObjLens", tube_grp, (0.45, 0, 0), 0.058, 0.058, 0.014, mats["glass_sky"], verts=10, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl(f"{name}_Finder", tube_grp, (0.08, 0, 0.095), 0.016, 0.022, 0.28, mats["copper"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    for fx in (0.0, 0.16):
        add_box(f"{name}_FinderMount_{fx:.2f}", tube_grp, (fx, 0, 0.068), (0.02, 0.02, 0.045), mats["brass_gold"], col=col)
    return ts


def add_clockwork_chandelier(name, parent, ceil_loc, drop_z, mats, col=None):
    """Grand concentric brass gear orrery chandelier suspended from ceil_loc down to drop_z."""
    cx, cy, ceil_z = ceil_loc
    ch = bpy.data.objects.new(name, None)
    ch.location = Vector((cx, cy, drop_z))
    if parent:
        ch.parent = parent
    link_obj(ch, col)

    h_drop = max(0.45, ceil_z - drop_z)
    add_cyl(f"{name}_Canopy", ch, (0, 0, h_drop), 0.11, 0.07, 0.06, mats["charcoal"], verts=8, col=col)
    add_limb(f"{name}_DropRod", ch, (0, 0, h_drop), (0, 0, 0.25), 0.022, 0.022, mats["charcoal"], segments=6, col=col)
    add_cyl(f"{name}_Crown", ch, (0, 0, 0.26), 0.08, 0.04, 0.12, mats["brass_gold"], verts=8, col=col)

    _add_hollow_ring(f"{name}_OuterRing", ch, (0, 0, 0.0), 0.72, 0.024, mats["brass_gold"], segments=12, col=col)
    _add_hollow_ring(f"{name}_InnerRing", ch, (0, 0, 0.10), 0.44, 0.020, mats["copper"], rot_euler=(0.35, 0.20, 0), segments=10, col=col)
    add_ico(f"{name}_CoreOrb", ch, (0, 0, 0.08), (0.11, 0.11, 0.14), mats["amber_glass"], subdivisions=1, col=col)

    for ai in range(4):
        ang = ai * (math.pi * 0.5) + math.pi * 0.25
        ax, ay = math.cos(ang) * 0.72, math.sin(ang) * 0.72
        add_limb(f"{name}_Arm_{ai}", ch, (0, 0, 0.24), (ax, ay, 0.0), 0.015, 0.015, mats["brass_gold"], segments=4, col=col)
        add_cyl(f"{name}_LanCap_{ai}", ch, (ax, ay, -0.02), 0.12, 0.04, 0.06, mats["charcoal"], verts=6, col=col)
        add_cyl(f"{name}_LanGlow_{ai}", ch, (ax, ay, -0.11), 0.075, 0.10, 0.14, mats["warm_lantern"], verts=6, col=col)
        add_cyl(f"{name}_LanBot_{ai}", ch, (ax, ay, -0.20), 0.03, 0.07, 0.05, mats["brass_gold"], verts=6, col=col)
    return ch


# ==================== LOW POLY COMPONENT HELPERS ====================

def add_window(name, parent, center, axis, win_w, win_h, mats, with_shutters=False, frame_mat=None, col=None):
    f_mat = frame_mat or mats["pure_white"]
    t_glass = 0.04
    t_frame = 0.11
    border = 0.10
    mull = 0.055
    cx, cy, cz = center

    if axis == "x":
        add_box(f"{name}_Glass", parent, (cx, cy, cz), (t_glass, win_w - border * 0.8, win_h - border * 0.8), mats["glass_sky"], col=col)
        add_box(f"{name}_Top", parent, (cx, cy, cz + win_h * 0.5), (t_frame + 0.04, win_w + border * 1.4, border), f_mat, col=col)
        add_box(f"{name}_Bot", parent, (cx, cy, cz - win_h * 0.5), (t_frame + 0.06, win_w + border * 1.6, border), f_mat, col=col)
        add_box(f"{name}_Left", parent, (cx, cy - win_w * 0.5, cz), (t_frame, border, win_h), f_mat, col=col)
        add_box(f"{name}_Right", parent, (cx, cy + win_w * 0.5, cz), (t_frame, border, win_h), f_mat, col=col)
        add_box(f"{name}_MullV", parent, (cx, cy, cz), (t_frame * 0.82, mull, win_h - border * 0.5), f_mat, col=col)
        add_box(f"{name}_MullH", parent, (cx, cy, cz + 0.06), (t_frame * 0.72, win_w - border * 0.5, mull), f_mat, col=col)
        if with_shutters:
            sw = 0.34
            off_y = win_w * 0.5 + border * 0.5 + sw * 0.54
            for s_name, sy, ang in (("ShutL", cy - off_y, -0.22), ("ShutR", cy + off_y, 0.22)):
                add_box(f"{name}_{s_name}", parent, (cx + 0.04, sy, cz), (0.06, sw, win_h), mats["emerald_shutter"], rot=(0, 0, ang), col=col)
    else:
        add_box(f"{name}_Glass", parent, (cx, cy, cz), (win_w - border * 0.8, t_glass, win_h - border * 0.8), mats["glass_sky"], col=col)
        add_box(f"{name}_Top", parent, (cx, cy, cz + win_h * 0.5), (win_w + border * 1.4, t_frame + 0.04, border), f_mat, col=col)
        add_box(f"{name}_Bot", parent, (cx, cy, cz - win_h * 0.5), (win_w + border * 1.6, t_frame + 0.06, border), f_mat, col=col)
        add_box(f"{name}_Left", parent, (cx - win_w * 0.5, cy, cz), (border, t_frame, win_h), f_mat, col=col)
        add_box(f"{name}_Right", parent, (cx + win_w * 0.5, cy, cz), (border, t_frame, win_h), f_mat, col=col)
        add_box(f"{name}_MullV", parent, (cx, cy, cz), (mull, t_frame * 0.82, win_h - border * 0.5), f_mat, col=col)
        add_box(f"{name}_MullH", parent, (cx, cy, cz + 0.06), (win_w - border * 0.5, t_frame * 0.72, mull), f_mat, col=col)
        if with_shutters:
            sw = 0.34
            off_x = win_w * 0.5 + border * 0.5 + sw * 0.54
            sy_off = cy - 0.05 if cy < 0.0 else cy + 0.05
            s_ang = 0.18 if cy < 0.0 else -0.18
            for s_name, sx, ang in (("ShutL", cx - off_x, s_ang), ("ShutR", cx + off_x, -s_ang)):
                add_box(f"{name}_{s_name}", parent, (sx, sy_off, cz), (sw, 0.06, win_h), mats["emerald_shutter"], rot=(0, 0, ang), col=col)


def add_pitched_terracotta_roof(name, parent, eave_z, hx, hy, ridge_h, mats, gable_mat=None, interior_lining_mat=None, col=None):
    overhang_x = 0.44
    overhang_y = 0.28
    span_x = hx + overhang_x
    span_y = hy + overhang_y
    slope_ang = math.atan2(ridge_h, hx)
    slope_len = math.hypot(span_x, ridge_h)

    add_prism_gable(f"{name}_GableWall", parent, (0, 0, eave_z), hx * 2.0 - 0.12, hy * 2.0 - 0.16, ridge_h, gable_mat or mats["sunny_yellow"], col=col)

    for sgn in (-1.0, 1.0):
        sx = sgn * (span_x * 0.5)
        sz = eave_z + ridge_h * 0.5 - 0.02
        add_box(f"{name}_Slab_{sgn}", parent, (sx, 0, sz), (slope_len, span_y * 2.0, 0.14), mats["terracotta_roof"], rot=(0, sgn * slope_ang, 0), col=col)
        if interior_lining_mat is not None:
            add_box(f"{name}_IntPlank_{sgn}", parent, (sgn * (hx * 0.5), 0, eave_z + ridge_h * 0.5 - 0.11), (math.hypot(hx, ridge_h), hy * 2.0 - 0.36, 0.08), interior_lining_mat, rot=(0, sgn * slope_ang, 0), col=col)
        add_box(f"{name}_EaveTrim_{sgn}", parent, (sgn * (hx + overhang_x * 0.86), 0, eave_z - 0.02), (0.14, span_y * 2.0 + 0.04, 0.16), mats["pure_white"], col=col)

        num_ribs = max(9, int(span_y * 2.4))
        for ri in range(num_ribs):
            ry = -span_y + 0.22 + ri * ((span_y * 2.0 - 0.44) / max(1, num_ribs - 1))
            add_box(f"{name}_Rib_{sgn}_{ri}", parent, (sx, ry, sz + 0.085), (slope_len - 0.04, 0.12, 0.07), mats["terracotta_dark"], rot=(0, sgn * slope_ang, 0), col=col)

        for step_i in range(4):
            u = (step_i + 0.5) / 4.0
            rx = sgn * (span_x * u)
            rz = eave_z + ridge_h * (1.0 - u) + 0.05
            add_box(f"{name}_Course_{sgn}_{step_i}", parent, (rx, 0, rz), (0.26, span_y * 2.0, 0.05), mats["terracotta_roof"], rot=(0, sgn * slope_ang, 0), col=col)

    add_box(f"{name}_RidgeCap", parent, (0, 0, eave_z + ridge_h + 0.08), (0.32, span_y * 2.0 + 0.04, 0.16), mats["terracotta_dark"], col=col)
    for end_sgn in (-1.0, 1.0):
        ey = end_sgn * (hy + overhang_y * 0.90)
        for sgn in (-1.0, 1.0):
            add_box(f"{name}_Barge_{end_sgn}_{sgn}", parent, (sgn * span_x * 0.5, ey, eave_z + ridge_h * 0.5), (slope_len, 0.13, 0.17), mats["pure_white"], rot=(0, sgn * slope_ang, 0), col=col)


def add_hip_terracotta_roof(name, parent, eave_z, hx, hy, ridge_h, mats, col=None):
    """4-sided Pyramid/Hip Roof (mái bánh ú) matching Anh Bảy corner house in overview image!"""
    ox, oy = hx + 0.42, hy + 0.42
    rx, ry = 0.0, hy * 0.22
    bm = bmesh.new()
    v0 = bm.verts.new((-ox, -oy, eave_z))
    v1 = bm.verts.new((ox, -oy, eave_z))
    v2 = bm.verts.new((ox, oy, eave_z))
    v3 = bm.verts.new((-ox, oy, eave_z))
    r0 = bm.verts.new((-rx, -ry, eave_z + ridge_h))
    r1 = bm.verts.new((rx, ry, eave_z + ridge_h))
    bm.faces.new((v0, v1, r1, r0))  # South
    bm.faces.new((v1, v2, r1))      # East
    bm.faces.new((v2, v3, r0, r1))  # North
    bm.faces.new((v3, v0, r0))      # West
    bm.faces.new((v3, v2, v1, v0))
    _finalize_bm(f"{name}_HipBody", parent, bm, (0, 0, 0), (0, 0, 0), mats["terracotta_roof"], col)
    # White eave belt
    add_box(f"{name}_EaveBelt", parent, (0, 0, eave_z), (ox * 2.0 + 0.04, oy * 2.0 + 0.04, 0.15), mats["pure_white"], col=col)
    # 4 corner hip ridge ribs + peak ridge bar in white/dark terracotta matching overview
    add_box(f"{name}_RidgeTop", parent, (0, 0, eave_z + ridge_h + 0.05), (0.28, ry * 2.0 + 0.28, 0.14), mats["pure_white"], col=col)
    for cx, cy in ((-ox, -oy), (ox, -oy), (ox, oy), (-ox, oy)):
        py = -ry if cy < 0 else ry
        add_limb(f"{name}_HipRib_{cx:.1f}_{cy:.1f}", parent, (cx, cy, eave_z + 0.06), (0.0, py, eave_z + ridge_h + 0.05), 0.08, 0.07, mats["pure_white"], segments=4, col=col)


def add_porch_steps(name, parent, step_edge_x, direction_x, center_y, step_w, step_top_z, mats, step_mat=None, col=None):
    s_mat = step_mat or mats["sidewalk_cream"]
    step_d = 0.34
    base_z = 0.082
    rise = (step_top_z - 0.006) - base_z
    for i in range(3):
        top_i = base_z + rise * ((3 - i) / 3.0)
        sx = step_edge_x + direction_x * (i * step_d + step_d * 0.5)
        add_box(f"{name}_Step_{i}", parent, (sx, center_y, top_i * 0.5), (step_d - 0.004, step_w, top_i), s_mat, col=col)


def add_hex_planter(name, parent, loc, radius, mats, col=None):
    p_root = bpy.data.objects.new(name, None)
    p_root.location = Vector(loc)
    if parent:
        p_root.parent = parent
    link_obj(p_root, col)

    add_cyl("Curb", p_root, (0, 0, 0.16), radius, radius * 1.04, 0.32, mats["curb_stone"], verts=6, col=col)
    add_cyl("TrimTop", p_root, (0, 0, 0.34), radius * 1.08, radius * 1.08, 0.06, mats["pure_white"], verts=6, col=col)
    add_cyl("Soil", p_root, (0, 0, 0.28), radius * 0.82, radius * 0.82, 0.10, mats["tree_leaf_mid"], verts=6, col=col)

    add_ico("Rock_A", p_root, (-0.18 * radius, 0.16 * radius, 0.44), (0.28 * radius, 0.22 * radius, 0.22 * radius), mats["rock_grey"], subdivisions=1, rot=(0.3, 0.5, 0.8), col=col)
    add_ico("Rock_B", p_root, (0.22 * radius, -0.10 * radius, 0.42), (0.22 * radius, 0.19 * radius, 0.18 * radius), mats["rock_grey"], subdivisions=1, rot=(-0.2, 0.4, 1.5), col=col)

    for gi in range(7):
        ang = gi * (2.0 * math.pi / 7.0) + 0.2
        gx = math.cos(ang) * 0.24 * radius
        gy = math.sin(ang) * 0.24 * radius
        tilt = 0.26 + (gi % 2) * 0.14
        add_cyl(f"Grass_{gi}", p_root, (gx, gy, 0.54), 0.06, 0.005, 0.54, mats["tree_leaf_bright"], verts=3, rot=(math.sin(ang) * tilt, -math.cos(ang) * tilt, ang), col=col)
    return p_root


def add_grass_tuft(name, parent, loc, scale_k, mats, col=None):
    tuft = bpy.data.objects.new(name, None)
    tuft.location = Vector(loc)
    if parent:
        tuft.parent = parent
    link_obj(tuft, col)
    for gi in range(8):
        ang = gi * (2.0 * math.pi / 8.0) + 0.15
        gx = math.cos(ang) * 0.15 * scale_k
        gy = math.sin(ang) * 0.15 * scale_k
        tilt = 0.28 + (gi % 3) * 0.10
        add_cyl(f"Blade_{gi}", tuft, (gx, gy, 0.26 * scale_k), 0.06 * scale_k, 0.005, 0.56 * scale_k, mats["tree_leaf_bright"], verts=3, rot=(math.sin(ang) * tilt, -math.cos(ang) * tilt, ang), col=col)
    return tuft


def add_wooden_crate(name, parent, loc, size_xyz, mats, rot=(0, 0, 0), trim_mat=None, col=None):
    c = bpy.data.objects.new(name, None)
    c.location = Vector(loc)
    c.rotation_euler = Euler(rot)
    if parent:
        c.parent = parent
    link_obj(c, col)
    sx, sy, sz = size_xyz
    tmat = trim_mat or mats["honey_wood"]
    add_box(f"{name}_Core", c, (0, 0, 0), (sx * 0.88, sy * 0.88, sz * 0.88), mats["wood_dark"], col=col)
    add_box(f"{name}_FrameT", c, (0, 0, sz * 0.5 - 0.0325), (sx, sy, 0.065), tmat, col=col)
    add_box(f"{name}_FrameB", c, (0, 0, -sz * 0.5 + 0.0325), (sx, sy, 0.065), tmat, col=col)
    for ix in (-sx * 0.44, sx * 0.44):
        for iy in (-sy * 0.44, sy * 0.44):
            add_box(f"{name}_Post_{ix:.2f}_{iy:.2f}", c, (ix, iy, 0), (0.068, 0.068, sz), tmat, col=col)
            add_ico(f"{name}_RivetT_{ix:.2f}_{iy:.2f}", c, (ix * 1.04, iy * 1.04, sz * 0.5 - 0.0325), (0.018, 0.018, 0.018), mats["charcoal"], subdivisions=1, col=col)
            add_ico(f"{name}_RivetB_{ix:.2f}_{iy:.2f}", c, (ix * 1.04, iy * 1.04, -sz * 0.5 + 0.0325), (0.018, 0.018, 0.018), mats["charcoal"], subdivisions=1, col=col)
    # Diagonal cross-braces on front/back (±X) & side (±Y) recessed panels
    for sx_s in (-1.0, 1.0):
        px = sx_s * sx * 0.445
        add_limb(f"{name}_DiagX_{sx_s:.0f}", c, (px, -sy * 0.36, -sz * 0.36), (px, sy * 0.36, sz * 0.36), 0.024, 0.024, tmat, segments=4, col=col)
    for sy_s in (-1.0, 1.0):
        py = sy_s * sy * 0.445
        add_limb(f"{name}_DiagY_{sy_s:.0f}", c, (-sx * 0.36, py, -sz * 0.36), (sx * 0.36, py, sz * 0.36), 0.024, 0.024, tmat, segments=4, col=col)
    return c


def add_faceted_barrel(name, parent, loc, r, h, mats, col=None):
    b = bpy.data.objects.new(name, None)
    b.location = Vector(loc)
    if parent:
        b.parent = parent
    link_obj(b, col)
    barrel_prof = [
        (r * 0.80, 0.0),
        (r * 0.90, h * 0.14),
        (r * 0.98, h * 0.34),
        (r * 1.00, h * 0.50),
        (r * 0.98, h * 0.66),
        (r * 0.90, h * 0.86),
        (r * 0.82, h * 0.96),
        (r * 0.82, h),
        (r * 0.72, h),
        (r * 0.72, h * 0.93),
        (0.001, h * 0.93),
    ]
    add_lathe(f"{name}_Staves", b, (0, 0, 0), barrel_prof, mats["honey_wood"], segments=8, col=col)
    add_cyl(f"{name}_TopHead", b, (0, 0, h * 0.935), r * 0.72, r * 0.72, 0.016, mats["wood_dark"], verts=8, col=col)
    for hi, (hz, hr) in enumerate(((0.18, 0.93), (0.50, 1.02), (0.82, 0.93))):
        add_cyl(f"{name}_Hoop_{hi}", b, (0, 0, h * hz), r * hr, r * hr, 0.045, mats["charcoal"], verts=8, col=col)
    return b


def add_lowpoly_lantern(name, parent, loc, mat_glow, mats, col=None):
    lan = bpy.data.objects.new(name, None)
    lan.location = Vector(loc)
    if parent:
        lan.parent = parent
    link_obj(lan, col)
    add_cyl("Cord", lan, (0, 0, 0.24), 0.015, 0.015, 0.48, mats["charcoal"], verts=4, col=col)
    add_cyl("CapTop", lan, (0, 0, 0.08), 0.22, 0.09, 0.12, mats["wood_dark"], verts=6, col=col)
    add_cyl("Body", lan, (0, 0, -0.09), 0.16, 0.22, 0.24, mat_glow, verts=6, col=col)
    add_cyl("CapBot", lan, (0, 0, -0.24), 0.07, 0.16, 0.08, mats["wood_dark"], verts=6, col=col)
    return lan


def add_plastic_stool(name, parent, loc, mat_col, rot_z=0.0, col=None):
    st = bpy.data.objects.new(name, None)
    st.location = Vector(loc)
    st.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        st.parent = parent
    link_obj(st, col)
    add_box("Seat", st, (0, 0, 0.42), (0.30, 0.30, 0.05), mat_col, col=col)
    for lx in (-0.11, 0.11):
        for ly in (-0.11, 0.11):
            add_box(f"Leg_{lx:.2f}_{ly:.2f}", st, (lx, ly, 0.21), (0.05, 0.05, 0.42), mat_col, col=col)
    return st


def add_striped_awning(name, parent, loc, depth, width, tilt_rad, mat_a, mat_b, rot_z=0.0, col=None):
    awn = bpy.data.objects.new(name, None)
    awn.location = Vector(loc)
    awn.rotation_euler = Euler((0, tilt_rad, rot_z))
    if parent:
        awn.parent = parent
    link_obj(awn, col)
    strips = max(6, int(width / 0.32))
    sw = width / strips
    for i in range(strips):
        sy = -width * 0.5 + (i + 0.5) * sw
        add_box(f"Strip_{i}", awn, (0, sy, 0), (depth, sw * 0.98, 0.07), mat_a if i % 2 == 0 else mat_b, col=col)
    return awn


def add_pedestal_oct(name, parent, loc, top_z, mats, is_cream=False, col=None):
    """Chunky paneled octagonal pedestal with dark wood plinth/cap & brass collar."""
    h = top_z - 0.24
    panel_mat = mats["warm_cream"] if is_cream else mats["honey_wood"]
    trim_mat = mats["honey_wood"] if is_cream else mats["wood_dark"]
    add_cyl(f"{name}_BasePlinth", parent, (loc[0], loc[1], 0.24 + 0.07), 0.46, 0.42, 0.14, mats["wood_dark"], verts=8, col=col)
    add_cyl(f"{name}_Shaft", parent, (loc[0], loc[1], 0.24 + h * 0.5), 0.33, 0.33, h - 0.22, trim_mat, verts=8, col=col)
    add_cyl(f"{name}_Panels", parent, (loc[0], loc[1], 0.24 + h * 0.5), 0.35, 0.35, h - 0.36, panel_mat, verts=4, rot=(0, 0, math.pi * 0.25), col=col)
    add_cyl(f"{name}_BrassRing", parent, (loc[0], loc[1], top_z - 0.13), 0.39, 0.39, 0.04, mats["brass_gold"], verts=8, col=col)
    add_cyl(f"{name}_Cap", parent, (loc[0], loc[1], top_z - 0.055), 0.41, 0.46, 0.11, trim_mat, verts=8, col=col)


# ==================== PARAMETERIZED WALK-IN SHOP ENVELOPE ====================

def build_shop_shell(
    name, depth_x, width_y, f1_h, f2_h, door_w, door_h, facing_east,
    f1_mat, f2_mat, mats, col, door_y=0.0, corner_mat=None,
    open_pitched_ceiling=False, int_wall_mat=None, int_base_mat=None, floor_base_mat=None
):
    root = bpy.data.objects.new(name, None)
    link_obj(root, col)

    c_mat = corner_mat or mats["pure_white"]
    i_wall = int_wall_mat or mats["warm_cream"]
    i_base = int_base_mat or mats["honey_wood"]
    fl_mat = floor_base_mat or mats["warm_cream"]

    hx = depth_x * 0.5
    hy = width_y * 0.5
    dir_x = 1.0 if facing_east else -1.0
    front_x = dir_x * (hx - 0.14)
    back_x = -dir_x * (hx - 0.14)
    total_h = f1_h + f2_h

    add_box("BasePlinth", root, (0, 0, 0.12), (depth_x, width_y, 0.24), mats["curb_stone"], col=col)
    add_box("FloorBase", root, (0, 0, 0.241), (depth_x - 0.32, width_y - 0.32, 0.016), fl_mat, col=col)

    for sgn, sname in ((-1.0, "S"), (1.0, "N")):
        wy = sgn * (hy - 0.14)
        add_box(f"Wall_Side_{sname}_F1", root, (0, wy, 0.24 + f1_h * 0.5), (depth_x - 0.20, 0.24, f1_h), f1_mat, col=col)
        add_box(f"Wall_Side_{sname}_F2", root, (0, wy, 0.24 + f1_h + f2_h * 0.5), (depth_x - 0.20, 0.24, f2_h), f2_mat, col=col)
        add_box(f"Int_Wall_{sname}", root, (0, sgn * (hy - 0.25), 0.24 + total_h * 0.49), (depth_x - 0.50, 0.06, total_h * 0.96), i_wall, col=col)
        add_box(f"Int_Baseboard_{sname}", root, (0, sgn * (hy - 0.29), 0.24 + 0.16), (depth_x - 0.50, 0.06, 0.32), i_base, col=col)

    add_box("Wall_Back_F1", root, (back_x, 0, 0.24 + f1_h * 0.5), (0.24, width_y - 0.20, f1_h), f1_mat, col=col)
    add_box("Wall_Back_F2", root, (back_x, 0, 0.24 + f1_h + f2_h * 0.5), (0.24, width_y - 0.20, f2_h), f2_mat, col=col)
    add_box("Int_Wall_Back", root, (back_x + dir_x * 0.12, 0, 0.24 + total_h * 0.49), (0.06, width_y - 0.50, total_h * 0.96), i_wall, col=col)
    add_box("Int_Baseboard_Back", root, (back_x + dir_x * 0.16, 0, 0.24 + 0.16), (0.06, width_y - 0.50, 0.32), i_base, col=col)

    d_min = door_y - door_w * 0.5
    d_max = door_y + door_w * 0.5
    pier_s_w = max(0.35, d_min - (-hy))
    pier_s_y = -hy + pier_s_w * 0.5
    pier_n_w = max(0.35, hy - d_max)
    pier_n_y = hy - pier_n_w * 0.5

    for sname, py_c, pw in (("S", pier_s_y, pier_s_w), ("N", pier_n_y, pier_n_w)):
        add_box(f"FrontPier_{sname}", root, (front_x, py_c, 0.24 + f1_h * 0.5), (0.26, pw, f1_h), f1_mat, col=col)
        add_box(f"Int_FrontPier_{sname}", root, (front_x - dir_x * 0.13, py_c, 0.24 + f1_h * 0.5), (0.06, max(0.2, pw - 0.14), f1_h), i_wall, col=col)

    lintel_h = max(0.35, f1_h - door_h)
    add_box("FrontLintel_F1", root, (front_x, door_y, 0.24 + door_h + lintel_h * 0.5), (0.26, door_w + 0.14, lintel_h), f1_mat, col=col)
    add_box("Wall_Front_F2", root, (front_x, 0, 0.24 + f1_h + f2_h * 0.5), (0.26, width_y - 0.20, f2_h), f2_mat, col=col)
    add_box("Int_Front_F2", root, (front_x - dir_x * 0.13, 0, 0.24 + f1_h + f2_h * 0.5), (0.06, width_y - 0.48, f2_h), i_wall, col=col)

    tx = dir_x * hx
    add_box("DoorFrame_L", root, (tx, d_min - 0.08, 0.24 + door_h * 0.5), (0.34, 0.20, door_h), mats["pure_white"], col=col)
    add_box("DoorFrame_R", root, (tx, d_max + 0.08, 0.24 + door_h * 0.5), (0.34, 0.20, door_h), mats["pure_white"], col=col)
    add_box("DoorFrame_Top", root, (tx, door_y, 0.24 + door_h + 0.09), (0.38, door_w + 0.40, 0.22), mats["pure_white"], col=col)

    belt_z = 0.24 + f1_h
    # Build belt cornice strictly as 4 thin perimeter moldings (never a solid slab blocking the interior!)
    add_box("Belt_SideS", root, (0.0, -hy - 0.02, belt_z), (depth_x + 0.22, 0.26, 0.18), c_mat, col=col)
    add_box("Belt_SideN", root, (0.0, hy + 0.02, belt_z), (depth_x + 0.22, 0.26, 0.18), c_mat, col=col)
    add_box("Belt_Front", root, (front_x + dir_x * 0.16, 0.0, belt_z), (0.26, width_y + 0.22, 0.18), c_mat, col=col)
    add_box("Belt_Back", root, (back_x - dir_x * 0.16, 0.0, belt_z), (0.26, width_y + 0.22, 0.18), c_mat, col=col)
    for cx in (-hx + 0.08, hx - 0.08):
        for cy in (-hy + 0.08, hy - 0.08):
            add_box(f"Pilaster_{cx:.1f}_{cy:.1f}", root, (cx, cy, 0.24 + total_h * 0.5), (0.26, 0.26, total_h), c_mat, col=col)

    ceil_z = 0.24 + total_h - 0.06
    if not open_pitched_ceiling:
        add_box("Int_CeilingDeck", root, (0, 0, ceil_z + 0.06), (depth_x - 0.40, width_y - 0.40, 0.12), mats["honey_wood"], col=col)
        for beam_y in (-hy * 0.55, 0.0, hy * 0.55):
            add_box(f"Int_Beam_{beam_y:.1f}", root, (0, beam_y, ceil_z - 0.10), (depth_x - 0.50, 0.22, 0.22), mats["wood_dark"], col=col)

    return root, hx, hy, dir_x, belt_z, ceil_z


# ==================== 1. NHÀ NIÊM PHONG (WARM HONEY-WOOD MEZZANINE EMPORIUM) ====================

def build_nha_niem_phong(mats, font, col):
    """
    COZY VERTICAL SLICE ("Góc Mẫu #2") — Nhà Niêm Phong (Dì Mận's Sealed-Goods Emporium):
    - Real cut-through 6-pane display windows on East front, South street wall & North side wall + flower boxes & sill props.
    - 3D vertical crimson clapboard siding, warm half-timbered golden-ochre F2 story with emerald shutters & Hero Wax Seal Crest.
    - Raised timber front porch deck + 2 wide steps + dark charcoal sloped handrails + terracotta door canopy & dual lanterns.
    - Warm staggered oak plank floor + central crimson & brass runner rug + subtle ochre hex tiles + sunbeams.
    - Honey-wood wainscoting, Cathedral A-frame roof trusses, U-mezzanine stuffed with sealed cargo,
      4 ceiling-hung brass pendant lamps, wall storytelling charts, Grand Dì Mận counter & 18-cubby treasure wall.
    """
    depth_x, width_y, f1_h, f2_h = 10.0, 12.0, 3.55, 2.85
    hx, hy = depth_x * 0.5, width_y * 0.5
    door_y, door_w, door_h = 0.0, 3.0, 2.95
    belt_z = 0.24 + f1_h
    eave_z = belt_z + f2_h
    ridge_h = 2.35
    total_h = f1_h + f2_h
    txt_rot = (math.pi * 0.5, 0.0, math.pi * 0.5)

    root = bpy.data.objects.new("LP_Shop_NhaNiemPhong", None)
    link_obj(root, col)

    # 0. Foundation & Staggered Warm Oak Plank Floor + Central Runner Rug + Hex Accents + Sunbeams
    add_box("BasePlinth", root, (0.0, 0.0, 0.12), (depth_x, width_y, 0.24), mats["curb_stone"], col=col)
    add_box("FloorBase", root, (0.0, 0.0, 0.236), (depth_x - 0.30, width_y - 0.30, 0.016), mats["walnut_dark"], col=col)
    for row_i in range(17):
        py = -5.44 + row_i * 0.68
        if abs(py) > 5.42:
            continue
        for col_i in range(6):
            px_raw = -3.85 + col_i * 1.66 + (0.36 if row_i % 2 == 1 else -0.18)
            x0 = max(-hx + 0.30, px_raw - 0.80)
            x1 = min(hx - 0.30, px_raw + 0.80)
            if x1 - x0 < 0.40:
                continue
            p_mat = mats["oak_plank_a"] if (row_i + col_i) % 3 != 0 else mats["oak_plank_b"]
            add_box(f"OakPlank_{row_i}_{col_i}", root, ((x0 + x1) * 0.5, py, 0.246), (x1 - x0, 0.64, 0.014), p_mat, col=col)

    # Woven Central Crimson & Brass Runner Rug leading from doorway to Dì Mận counter
    add_box("Hall_RugTrim", root, (0.90, 0.0, 0.254), (6.60, 2.50, 0.008), mats["brass_gold"], col=col)
    add_box("Hall_RugBody", root, (0.90, 0.0, 0.258), (6.36, 2.26, 0.008), mats["crimson_velvet"], col=col)

    # Subtle hexagonal ochre floor accent tiles framing the pedestal aisles (|Y| > 1.65)
    hex_dx, hex_dy = 0.92, 0.80
    for ix in range(9):
        tx = -2.4 + ix * hex_dx
        if tx > 4.0:
            break
        for iy in range(14):
            ty = -5.0 + iy * hex_dy + (0.40 if ix % 2 == 1 else 0.0)
            if abs(ty) < 1.70 or abs(ty) > 4.65:
                continue
            if (ix + iy) % 3 == 0:
                add_cyl(f"HexTile_{ix}_{iy}", root, (tx, ty, 0.254), 0.34, 0.34, 0.008, mats["floor_ochre"], verts=6, col=col)

    # Diagonal afternoon sunbeam pools inside East doorway & South windows
    for si, (sbx, sby) in enumerate(((3.4, 0.0), (1.8, -0.35), (2.1, -4.3), (-1.6, -4.3))):
        add_box(f"Sunbeam_{si}", root, (sbx, sby, 0.264), (1.50, 0.80, 0.003), mats["sunbeam_floor"], rot=(0, 0, 0.35), col=col)

    # 1. Back Wall (-X) — Seamless Vertical Plaster Joint (Z=1.32..3.80 overlaps Int_F2_Back Z=3.78..6.62)
    back_x = -hx + 0.14
    add_box("Wall_Back_F1", root, (back_x, 0.0, 0.24 + f1_h * 0.5), (0.24, width_y - 0.20, f1_h), mats["crimson_dark"], col=col)
    _add_clapboard_strip_y("Clap_Back", root, -hx, -hy + 0.22, hy - 0.22, 0.24, belt_z, mats, dir_x=-1.0, col=col)
    add_box("Wall_Back_F2", root, (back_x, 0.0, belt_z + f2_h * 0.5), (0.24, width_y - 0.20, f2_h), mats["sunny_yellow"], col=col)
    add_box("Int_Wain_Back", root, (back_x + 0.15, 0.0, 0.24 + 0.52), (0.05, width_y - 0.50, 1.04), mats["honey_wood"], col=col)
    add_box("Int_Chair_Back", root, (back_x + 0.18, 0.0, 1.28), (0.08, width_y - 0.50, 0.09), mats["wood_dark"], col=col)
    add_box("Int_Upper_Back", root, (back_x + 0.14, 0.0, 2.56), (0.05, width_y - 0.50, 2.48), mats["warm_cream"], col=col)
    add_box("Int_F2_Back", root, (back_x + 0.14, 0.0, 5.20), (0.05, width_y - 0.50, 2.84), mats["warm_cream"], col=col)

    # 2. East Front Wall (+X) — Doorway + 2 Real Cut-Through 6-Pane Display Windows (sill top Z=1.230)
    front_x = hx - 0.14
    for p_name, y0, y1 in (("S", -5.90, -4.70), ("MidS", -2.60, -1.50), ("MidN", 1.50, 2.60), ("N", 4.70, 5.90)):
        pw = y1 - y0
        yc = (y0 + y1) * 0.5
        add_box(f"FrontPier_{p_name}", root, (front_x, yc, 0.24 + f1_h * 0.5), (0.26, pw, f1_h), mats["crimson_dark"], col=col)
        _add_clapboard_strip_y(f"Clap_Front_{p_name}", root, hx, max(-hy + 0.22, y0), min(hy - 0.22, y1), 0.24, belt_z, mats, dir_x=1.0, col=col)
        add_box(f"Int_Wain_F_{p_name}", root, (front_x - 0.15, yc, 0.24 + 0.52), (0.05, max(0.2, pw - 0.06), 1.04), mats["honey_wood"], col=col)
        add_box(f"Int_Chair_F_{p_name}", root, (front_x - 0.18, yc, 1.28), (0.08, max(0.2, pw - 0.06), 0.09), mats["wood_dark"], col=col)
        add_box(f"Int_Upper_F_{p_name}", root, (front_x - 0.14, yc, 2.56), (0.05, max(0.2, pw - 0.06), 2.48), mats["warm_cream"], col=col)

    for w_name, wy_c in (("Win_FrontS", -3.65), ("Win_FrontN", 3.65)):
        ww = 2.10
        add_box(f"Wall_Front_Bot_{w_name}", root, (front_x, wy_c, 0.705), (0.26, ww, 0.93), mats["crimson_dark"], col=col)
        _add_clapboard_strip_y(f"Clap_Front_Bot_{w_name}", root, hx, wy_c - ww * 0.5, wy_c + ww * 0.5, 0.24, 1.15, mats, dir_x=1.0, col=col)
        add_box(f"Int_Wain_Bot_{w_name}", root, (front_x - 0.15, wy_c, 0.705), (0.05, ww, 0.93), mats["honey_wood"], col=col)
        add_box(f"Wall_Front_Top_{w_name}", root, (front_x, wy_c, 3.255), (0.26, ww, 1.07), mats["crimson_dark"], col=col)
        _add_clapboard_strip_y(f"Clap_Front_Top_{w_name}", root, hx, wy_c - ww * 0.5, wy_c + ww * 0.5, 2.74, belt_z, mats, dir_x=1.0, col=col)
        add_box(f"Int_Upper_Top_{w_name}", root, (front_x - 0.14, wy_c, 3.26), (0.05, ww, 1.08), mats["warm_cream"], col=col)
        _add_cut_window(w_name, root, (front_x, wy_c, 1.945), "x", ww, 1.55, 0.28, mats, cols=3, rows=2, col=col)

    # Doorway Lintel & Thick Cream Framing
    add_box("FrontLintel_Door", root, (front_x, door_y, 0.24 + door_h + (f1_h - door_h) * 0.5), (0.26, door_w + 0.10, f1_h - door_h), mats["crimson_dark"], col=col)
    _add_clapboard_strip_y("Clap_DoorLintel", root, hx, door_y - door_w * 0.5, door_y + door_w * 0.5, 0.24 + door_h + 0.06, belt_z, mats, dir_x=1.0, col=col)
    add_box("Int_Upper_DoorLintel", root, (front_x - 0.14, door_y, 3.495), (0.05, door_w + 0.04, 0.61), mats["warm_cream"], col=col)
    add_box("DoorFrame_L", root, (hx, -door_w * 0.5 - 0.08, 0.24 + door_h * 0.5), (0.36, 0.20, door_h), mats["pure_white"], col=col)
    add_box("DoorFrame_R", root, (hx, door_w * 0.5 + 0.08, 0.24 + door_h * 0.5), (0.36, 0.20, door_h), mats["pure_white"], col=col)
    add_box("DoorFrame_Top", root, (hx, door_y, 0.24 + door_h + 0.09), (0.40, door_w + 0.40, 0.22), mats["pure_white"], col=col)

    # 3. South (-Y) & North (+Y) Walls with Real Cut-Through Windows (sill top Z=1.270)
    for sgn, sname in ((-1.0, "South"), (1.0, "North")):
        wy = sgn * (hy - 0.14)
        face_y = sgn * hy
        in_d = -sgn
        for p_name, x0, x1 in (("W", -4.90, -2.55), ("Mid", -0.65, 1.15), ("E", 3.05, 4.90)):
            pw = x1 - x0
            xc = (x0 + x1) * 0.5
            add_box(f"Wall_{sname}_{p_name}", root, (xc, wy, 0.24 + f1_h * 0.5), (pw, 0.24, f1_h), mats["crimson_dark"], col=col)
            _add_clapboard_strip_x(f"Clap_{sname}_{p_name}", root, face_y, max(-hx + 0.22, x0), min(hx - 0.22, x1), 0.24, belt_z, mats, dir_y=sgn, col=col)
            add_box(f"Int_Wain_{sname}_{p_name}", root, (xc, wy + in_d * 0.15, 0.24 + 0.52), (max(0.2, pw - 0.06), 0.05, 1.04), mats["honey_wood"], col=col)
            add_box(f"Int_Chair_{sname}_{p_name}", root, (xc, wy + in_d * 0.18, 1.28), (max(0.2, pw - 0.06), 0.08, 0.09), mats["wood_dark"], col=col)
            add_box(f"Int_Upper_{sname}_{p_name}", root, (xc, wy + in_d * 0.14, 2.56), (max(0.2, pw - 0.06), 0.05, 2.48), mats["warm_cream"], col=col)

        for w_tag, wx_c in (("W", -1.60), ("E", 2.10)):
            ww = 1.90
            add_box(f"Wall_{sname}_Bot_{w_tag}", root, (wx_c, wy, 0.725), (ww, 0.24, 0.97), mats["crimson_dark"], col=col)
            _add_clapboard_strip_x(f"Clap_{sname}_Bot_{w_tag}", root, face_y, wx_c - ww * 0.5, wx_c + ww * 0.5, 0.24, 1.19, mats, dir_y=sgn, col=col)
            add_box(f"Int_Wain_{sname}_Bot_{w_tag}", root, (wx_c, wy + in_d * 0.15, 0.725), (ww, 0.05, 0.97), mats["honey_wood"], col=col)
            add_box(f"Wall_{sname}_Top_{w_tag}", root, (wx_c, wy, 3.25), (ww, 0.24, 1.08), mats["crimson_dark"], col=col)
            _add_clapboard_strip_x(f"Clap_{sname}_Top_{w_tag}", root, face_y, wx_c - ww * 0.5, wx_c + ww * 0.5, 2.73, belt_z, mats, dir_y=sgn, col=col)
            add_box(f"Int_Upper_{sname}_Top_{w_tag}", root, (wx_c, wy + in_d * 0.14, 3.255), (ww, 0.05, 1.09), mats["warm_cream"], col=col)
            _add_cut_window(
                f"Win_{sname}{w_tag}", root, (wx_c, wy, 1.96), "y", ww, 1.50, 0.26, mats, cols=3, rows=2,
                sill_depth=(0.32 if sgn > 0 else None), col=col
            )

    # 4. Upper Half-Timbered Golden-Ochre F2 Story, Interior Cornice Trim & Roof
    add_box("Wall_Front_F2", root, (front_x, 0.0, belt_z + f2_h * 0.5), (0.26, width_y - 0.20, f2_h), mats["sunny_yellow"], col=col)
    add_box("Int_F2_Front", root, (front_x - 0.14, 0.0, 5.20), (0.05, width_y - 0.50, 2.84), mats["warm_cream"], col=col)
    for sgn, sname in ((-1.0, "South"), (1.0, "North")):
        wy = sgn * (hy - 0.14)
        add_box(f"Wall_{sname}_F2", root, (0.0, wy, belt_z + f2_h * 0.5), (depth_x - 0.20, 0.24, f2_h), mats["sunny_yellow"], col=col)
        add_box(f"Int_F2_{sname}", root, (0.0, wy - sgn * 0.14, 5.20), (depth_x - 0.50, 0.05, 2.84), mats["warm_cream"], col=col)

    add_box("Int_Cornice_Back", root, (back_x + 0.18, 0.0, 3.76), (0.08, width_y - 0.46, 0.10), mats["wood_dark"], col=col)
    add_box("Int_Cornice_Front", root, (front_x - 0.18, 0.0, 3.76), (0.08, width_y - 0.46, 0.10), mats["wood_dark"], col=col)
    add_box("Int_Cornice_South", root, (0.0, -hy + 0.32, 3.76), (depth_x - 0.46, 0.08, 0.10), mats["wood_dark"], col=col)
    add_box("Int_Cornice_North", root, (0.0, hy - 0.32, 3.76), (depth_x - 0.46, 0.08, 0.10), mats["wood_dark"], col=col)

    # Perimeter Belt Cornice & Corner Posts
    add_box("BeltTrim_F", root, (hx + 0.05, 0.0, belt_z), (0.20, width_y + 0.16, 0.18), mats["pure_white"], col=col)
    add_box("BeltTrim_B", root, (-hx - 0.05, 0.0, belt_z), (0.20, width_y + 0.16, 0.18), mats["pure_white"], col=col)
    add_box("BeltTrim_S", root, (0.0, -hy - 0.05, belt_z), (depth_x + 0.16, 0.20, 0.18), mats["pure_white"], col=col)
    add_box("BeltTrim_N", root, (0.0, hy + 0.04, belt_z), (depth_x + 0.16, 0.16, 0.18), mats["pure_white"], col=col)
    for cx in (-hx + 0.08, hx - 0.08):
        for cy in (-hy + 0.08, hy - 0.08):
            add_box(f"CornerPost_{cx:.1f}_{cy:.1f}", root, (cx, cy, 0.24 + total_h * 0.5), (0.24, 0.24, total_h + 0.06), mats["pure_white"], col=col)

    # Half-Timbered Walnut Studs & F2 Windows (strictly clear of openings & zero Z-fighting!)
    for fy_stud in (-5.2, -2.1, 2.1, 5.2):
        add_box(f"F2_FrontStud_{fy_stud:.1f}", root, (hx + 0.02, fy_stud, belt_z + f2_h * 0.5), (0.06, 0.14, f2_h - 0.10), mats["wood_dark"], col=col)
    for wy_u in (-3.65, 3.65):
        add_window(f"F2_FrontWin_{wy_u:.1f}", root, (hx + 0.05, wy_u, belt_z + 1.40), "x", 1.25, 1.45, mats, with_shutters=True, col=col)

    for fx_stud in (-3.8, 0.25, 3.9):
        add_box(f"F2_SouthStud_{fx_stud:.1f}", root, (fx_stud, -hy - 0.02, belt_z + f2_h * 0.5), (0.14, 0.06, f2_h - 0.10), mats["wood_dark"], col=col)
    for wx_u in (-1.60, 2.10):
        add_window(f"F2_SouthWin_{wx_u:.1f}", root, (wx_u, -hy - 0.05, belt_z + 1.40), "y", 1.25, 1.40, mats, with_shutters=True, col=col)
        add_window(f"F2_NorthWin_{wx_u:.1f}", root, (wx_u, hy + 0.05, belt_z + 1.40), "y", 1.25, 1.40, mats, with_shutters=False, col=col)

    add_pitched_terracotta_roof("Roof", root, eave_z, hx, hy, ridge_h, mats, gable_mat=mats["sunny_yellow"], interior_lining_mat=mats["honey_wood"], col=col)
    for rti in range(15):
        rty = -5.5 + rti * (11.0 / 14.0)
        add_box(f"Ext_RafterTail_{rti}", root, (hx + 0.30, rty, eave_z - 0.14), (0.64, 0.11, 0.14), mats["wood_dark"], rot=(0, 0.35, 0), col=col)

    # 5. Grand Porch, Stairs, Balustraded Handrails, Door Hood, Mounted Signboard, Wax Seal & Flush Flowerboxes
    add_box("Porch_SubBase", root, (5.52, 0.0, 0.14), (1.04, 4.26, 0.12), mats["wood_dark"], col=col)
    for pi in range(5):
        px_d = 5.09 + pi * 0.21
        add_box(f"Porch_DeckPlank_{pi}", root, (px_d, 0.0, 0.22), (0.19, 4.30, 0.04), mats["timber_light"] if pi % 2 == 0 else mats["honey_wood"], col=col)

    add_box("Porch_StairUpper", root, (6.23, 0.0, 0.12), (0.38, 3.40, 0.08), mats["timber_light"], col=col)
    add_box("Porch_StairLower", root, (6.61, 0.0, 0.08), (0.38, 3.40, 0.04), mats["honey_wood"], col=col)
    add_box("Porch_StairStringL", root, (6.41, -1.72, 0.12), (0.82, 0.09, 0.16), mats["wood_dark"], rot=(0, 0.20, 0), col=col)
    add_box("Porch_StairStringR", root, (6.41, 1.72, 0.12), (0.82, 0.09, 0.16), mats["wood_dark"], rot=(0, 0.20, 0), col=col)

    for r_sgn, ry_rail, ry_ret in (("L", -1.72, -2.10), ("R", 1.72, 2.10)):
        add_box(f"Rail_TopPost_{r_sgn}", root, (5.96, ry_rail, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_box(f"Rail_BotPost_{r_sgn}", root, (6.78, ry_rail, 0.50), (0.09, 0.09, 0.84), mats["charcoal"], col=col)
        add_box(f"Rail_WallPost_{r_sgn}", root, (5.04, ry_ret, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_box(f"Rail_CornerPost_{r_sgn}", root, (5.96, ry_ret, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        for p_tag, px_cap, py_cap, pz_cap in (
            ("Top", 5.96, ry_rail, 1.155), ("Bot", 6.78, ry_rail, 0.935),
            ("Wall", 5.04, ry_ret, 1.155), ("Cor", 5.96, ry_ret, 1.155),
        ):
            add_box(f"Rail_Cap_{r_sgn}_{p_tag}", root, (px_cap, py_cap, pz_cap), (0.125, 0.125, 0.035), mats["brass_gold"], col=col)
        add_limb(f"Rail_SlopedBar_{r_sgn}", root, (5.96, ry_rail, 1.12), (6.78, ry_rail, 0.90), 0.036, 0.036, mats["charcoal"], segments=6, col=col)
        add_limb(f"Rail_MidBar_{r_sgn}", root, (5.96, ry_rail, 0.72), (6.78, ry_rail, 0.52), 0.022, 0.022, mats["charcoal"], segments=6, col=col)
        for bi, t in enumerate((0.25, 0.50, 0.75)):
            bx_bal = 5.96 + (6.78 - 5.96) * t
            z_top_b = 1.12 + (0.90 - 1.12) * t
            z_bot_b = 0.24 - 0.14 * t
            add_limb(f"Rail_StairBal_{r_sgn}_{bi}", root, (bx_bal, ry_rail, z_bot_b), (bx_bal, ry_rail, z_top_b), 0.016, 0.016, mats["charcoal"], segments=4, col=col)
        add_limb(f"Rail_ReturnBar_{r_sgn}", root, (5.04, ry_ret, 1.12), (5.96, ry_ret, 1.12), 0.032, 0.032, mats["charcoal"], segments=6, col=col)
        add_limb(f"Rail_FrontReturn_{r_sgn}", root, (5.96, ry_ret, 1.12), (5.96, ry_rail, 1.12), 0.032, 0.032, mats["charcoal"], segments=6, col=col)
        for bi, t in enumerate((0.33, 0.67)):
            add_limb(f"Rail_RetBal_{r_sgn}_{bi}", root, (5.04 + 0.92 * t, ry_ret, 0.24), (5.04 + 0.92 * t, ry_ret, 1.12), 0.015, 0.015, mats["charcoal"], segments=4, col=col)

    add_box("DoorHood_Roof", root, (5.45, 0.0, 3.36), (0.92, 4.10, 0.12), mats["terracotta_roof"], rot=(0, 0.24, 0), col=col)
    add_box("DoorHood_Trim", root, (5.86, 0.0, 3.26), (0.10, 4.14, 0.10), mats["wood_dark"], col=col)
    for cby in (-1.72, 1.72):
        add_limb(f"DoorHood_Bracket_{cby:.1f}", root, (hx + 0.06, cby, 2.88), (hx + 0.74, cby, 3.26), 0.055, 0.055, mats["wood_dark"], segments=6, col=col)

    add_box("Sign_Back", root, (hx + 0.09, 0.0, 3.98), (0.16, 4.10, 0.84), mats["honey_wood"], col=col)
    add_box("Sign_Face", root, (hx + 0.16, 0.0, 3.98), (0.10, 3.84, 0.64), mats["wood_dark"], col=col)
    for sy_br, btag in ((-1.65, "L"), (1.65, "R")):
        add_box(f"Sign_Bracket_{btag}", root, (hx + 0.01, sy_br, 3.98), (0.14, 0.12, 0.88), mats["charcoal"], col=col)
    for cz in (3.62, 4.34):
        for cy in (-1.94, 1.94):
            add_cyl(f"Sign_Stud_{cz:.2f}_{cy:.2f}", root, (hx + 0.18, cy, cz), 0.032, 0.032, 0.04, mats["brass_gold"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)
    add_3d_text_mesh("Sign_Text", root, "NHÀ NIÊM PHONG", (hx + 0.222, 0.0, 3.96), txt_rot, 0.32, mats["brass_gold"], font_data=font, col=col)

    seal_z = belt_z + 1.45
    add_cyl("WaxSeal_Base", root, (hx + 0.10, 0.0, seal_z), 0.58, 0.54, 0.12, mats["wax_red"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl("WaxSeal_Inner", root, (hx + 0.17, 0.0, seal_z), 0.36, 0.36, 0.06, mats["brass_gold"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl("WaxSeal_CenterDot", root, (hx + 0.21, 0.0, seal_z), 0.14, 0.14, 0.04, mats["wax_red"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_box("WaxRibbon_L", root, (hx + 0.07, -0.22, seal_z - 0.62), (0.05, 0.20, 0.54), mats["crimson_plank"], rot=(0.24, 0, 0), col=col)
    add_box("WaxRibbon_R", root, (hx + 0.07, 0.22, seal_z - 0.62), (0.05, 0.20, 0.54), mats["crimson_plank"], rot=(-0.24, 0, 0), col=col)

    for ly_p, ltag in ((-1.68, "L"), (1.68, "R")):
        add_bracket_lantern(f"PorchLan_{ltag}", root, (hx, ly_p, 2.90), (1.0, 0.0), mats, col=col)

    add_limb("Flag_Pole", root, (hx + 0.02, -2.28, 3.25), (hx + 0.92, -2.28, 3.72), 0.032, 0.026, mats["wood_dark"], segments=6, col=col)
    add_box("Flag_ClothA", root, (hx + 0.70, -2.28, 3.24), (0.48, 0.03, 0.46), mats["crimson_plank"], rot=(0, 0.15, 0), col=col)
    add_box("Flag_Stripe", root, (hx + 0.70, -2.28, 3.24), (0.14, 0.04, 0.46), mats["brass_gold"], rot=(0, 0.15, 0), col=col)

    add_window_flowerbox("FlowerBox_EastS", root, (hx, -3.65, 0.985), "x", 1.0, 1.96, mats, col=col)
    add_window_flowerbox("FlowerBox_EastN", root, (hx, 3.65, 0.985), "x", 1.0, 1.96, mats, col=col)
    add_window_flowerbox("FlowerBox_SouthW", root, (-1.60, -hy, 0.985), "y", -1.0, 1.80, mats, col=col)
    add_window_flowerbox("FlowerBox_SouthE", root, (2.10, -hy, 0.985), "y", -1.0, 1.80, mats, col=col)

    add_wooden_crate("Porch_CrateL", root, (5.56, -5.20, 0.36), (0.58, 0.58, 0.56), mats, rot=(0, 0, 0.14), col=col)
    add_faceted_barrel("Porch_BarrelL", root, (5.54, -5.85, 0.08), 0.26, 0.72, mats, col=col)
    add_wooden_crate("Porch_CrateR", root, (5.56, 5.20, 0.36), (0.58, 0.58, 0.56), mats, rot=(0, 0, -0.14), col=col)
    add_faceted_barrel("Porch_BarrelR", root, (5.54, 5.85, 0.08), 0.26, 0.72, mats, col=col)
    add_grass_tuft("Porch_CornerGrass", root, (hx + 0.40, -hy - 0.22, 0.08), 0.85, mats, col=col)

    # === 6. COZY CATHEDRAL MEZZANINE INTERIOR, PENDANTS, PEDESTALS, STORYTELLING, COUNTER & 18-CUBBY WALL ===
    # Interior dark timber wall pilasters (flanking corners & window bays, clear of wall charts/curio shelves)
    for px, py in (
        (-4.68, -5.66), (-4.68, 5.66), (4.68, -5.66), (4.68, 5.66),
        (-2.52, -5.66), (-0.62, -5.66), (1.12, -5.66), (3.08, -5.66),
        (-2.52, 5.66), (-0.62, 5.66), (1.12, 5.66), (3.08, 5.66),
    ):
        add_box(f"Int_TimberPost_{px:.2f}_{py:.2f}", root, (px, py, 0.24 + 1.68), (0.18, 0.18, 3.36), mats["wood_dark"], col=col)

    # Cathedral Exposed A-Frame Timber Roof Trusses
    add_box("Truss_RidgeBeam", root, (0.0, 0.0, eave_z + ridge_h - 0.22), (0.24, width_y - 0.50, 0.26), mats["wood_dark"], col=col)
    truss_ang = math.atan2(ridge_h - 0.25, hx - 0.3)
    rafter_len = math.hypot(hx - 0.3, ridge_h - 0.25)
    for ty in (-3.6, -1.2, 1.2, 3.6):
        add_box(f"Truss_TieBeam_{ty:.1f}", root, (0.0, ty, eave_z - 0.12), (depth_x - 0.56, 0.22, 0.24), mats["wood_dark"], col=col)
        add_box(f"Truss_KingPost_{ty:.1f}", root, (0.0, ty, eave_z + (ridge_h - 0.25) * 0.5), (0.20, 0.20, ridge_h - 0.25), mats["honey_wood"], col=col)
        for rsgn in (-1.0, 1.0):
            add_box(f"Truss_Rafter_{ty:.1f}_{rsgn}", root, (rsgn * hx * 0.48, ty, eave_z + ridge_h * 0.44), (rafter_len, 0.18, 0.20), mats["honey_wood"], rot=(0, rsgn * truss_ang, 0), col=col)

    # Clean Non-Overlapping U-Shaped Mezzanine (side rails terminate cleanly at CornerNewel X=-3.05!)
    mez_z = 3.55
    for sgn in (-1.0, 1.0):
        by_mid = sgn * (hy - 1.15)
        ry_edge = sgn * (hy - 1.98)
        add_box(f"Mez_Deck_{sgn}", root, (0.525, by_mid, mez_z), (7.25, 1.80, 0.16), mats["honey_wood"], col=col)
        add_box(f"Mez_Fascia_{sgn}", root, (0.55, ry_edge, mez_z - 0.02), (7.20, 0.14, 0.22), mats["wood_dark"], col=col)
        add_box(f"Mez_RailTop_{sgn}", root, (0.55, ry_edge, mez_z + 0.92), (7.20, 0.14, 0.10), mats["honey_wood"], col=col)
        add_box(f"Mez_CornerNewel_{sgn}", root, (-3.05, ry_edge, mez_z + 0.48), (0.18, 0.18, 0.98), mats["wood_dark"], col=col)
        for bx in (-2.32, -1.59, -0.86, -0.13, 1.30, 2.00, 2.70, 3.40):
            add_cyl(f"Mez_Bal_{sgn}_{bx:.2f}", root, (bx, ry_edge, mez_z + 0.47), 0.045, 0.055, 0.80, mats["honey_wood"], verts=6, col=col)
        for px in (-3.05, 0.6, 4.10):
            add_box(f"Mez_Post_{sgn}_{px:.2f}", root, (px, ry_edge, 0.24 + (mez_z - 0.24) * 0.5), (0.20, 0.20, mez_z - 0.24), mats["wood_dark"], col=col)

    add_box("Mez_Deck_Back", root, (-hx + 1.10, 0.0, mez_z), (1.80, width_y - 3.6, 0.16), mats["honey_wood"], col=col)
    add_box("Mez_Fascia_Back", root, (-3.05, 0.0, mez_z - 0.02), (0.14, 8.04, 0.22), mats["wood_dark"], col=col)
    add_box("Mez_RailTop_Back", root, (-3.05, 0.0, mez_z + 0.92), (0.14, 8.04, 0.10), mats["honey_wood"], col=col)
    for by in (-3.2, -2.4, -1.6, -0.8, 0.0, 0.8, 1.6, 2.4, 3.2):
        add_cyl(f"Mez_Bal_Back_{by:.1f}", root, (-3.05, by, mez_z + 0.47), 0.045, 0.055, 0.80, mats["honey_wood"], verts=6, col=col)

    # Flush-Grounded Mezzanine Cargo & Stacked Crate #6 directly atop Crate #0 (Mez top Z = 3.630)
    mez_top = mez_z + 0.08
    mez_crates = (
        (-4.0, -2.2, "box_purple", 0.48),
        (-4.0, 2.2, "box_teal", 0.48),
        (1.2, 4.85, "crimson_plank", 0.48),
        (-1.4, -4.85, "sunny_yellow", 0.48),
        (2.8, -4.75, "box_purple", 0.42),
        (-2.4, 4.85, "brass_gold", 0.44),
    )
    for mi, (mx, my, m_k, sz) in enumerate(mez_crates):
        ch = sz * 0.92
        add_wooden_crate(f"Mez_Crate_{mi}", root, (mx, my, mez_top + ch * 0.5), (sz, sz, ch), mats, trim_mat=mats[m_k], rot=(0, 0, mi * 0.15), col=col)
    c0_top = mez_top + 0.48 * 0.92
    add_wooden_crate("Mez_Crate_6", root, (-4.0, -2.2, c0_top + 0.18), (0.36, 0.36, 0.36), mats, trim_mat=mats["honey_wood"], rot=(0, 0, 0.42), col=col)

    _add_book_cluster("Mez_Books_S", root, (0.2, -4.75, mez_top), 5, mats, seed=3, rot_z=math.pi * 0.5, col=col)
    _add_book_cluster("Mez_Books_N", root, (-0.8, 4.75, mez_top), 4, mats, seed=6, rot_z=math.pi * 0.5, col=col)
    for ji, (jx, jy, jm, jh) in enumerate(((-3.7, 1.1, "ceramic_sage", 0.42), (-3.6, -3.2, "terracotta_roof", 0.40), (2.6, 4.8, "ceramic_sage", 0.42), (-2.9, -4.8, "amber_glass", 0.36))):
        add_sculpted_vase(f"Mez_Jar_{ji}", root, (jx, jy, mez_top), jh, 0.14, mats[jm], mats, with_handles=(ji % 2 == 0), col=col)
    for si, sy_scroll in enumerate((-0.42, 0.0, 0.42)):
        add_scroll_bundle(f"Mez_Scroll_{si}", root, (-3.6, sy_scroll, mez_top), 0.46, 0.064, mats["blueprint_blue"] if si % 2 == 0 else mats["parchment"], mats, rot_z=0.06 * (si - 1), col=col)

    # Suspended Wax-Sealed Hero Crate — WaxSeal & Eyebolts parented cleanly to rotated crate!
    sus_crate = add_wooden_crate("Sus_HeroCrate", root, (0.2, 0.0, 4.45), (0.84, 0.84, 0.72), mats, rot=(0.06, 0.10, 0.14), col=col)
    add_cyl("Sus_WaxSeal", sus_crate, (0.425, 0.0, 0.0), 0.18, 0.18, 0.05, mats["wax_red"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl("Sus_WaxRing", sus_crate, (0.452, 0.0, 0.0), 0.11, 0.11, 0.02, mats["brass_gold"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    bpy.context.view_layer.update()
    for eye_y, rtag in ((-0.32, "A"), (0.32, "B")):
        add_cyl(f"Sus_EyeBolt_{rtag}", sus_crate, (0.0, eye_y, 0.37), 0.045, 0.045, 0.05, mats["brass_gold"], verts=6, col=col)
        eye_root = (sus_crate.matrix_world @ Vector((0.0, eye_y, 0.37)))
        add_limb(f"Sus_Rope{rtag}", root, (0.2, eye_y * 1.08, eave_z + ridge_h - 0.22), (eye_root.x, eye_root.y, eye_root.z), 0.024, 0.024, mats["charcoal"], segments=4, col=col)

    roof_ang = math.atan2(ridge_h, hx)
    for pi, (lx, ly) in enumerate(((1.6, -2.5), (1.6, 2.5), (-1.2, -2.5), (-1.2, 2.5))):
        z_ceil = eave_z + ridge_h * (1.0 - abs(lx) / hx) - 0.11 - 0.04 / math.cos(roof_ang)
        add_pendant_lamp(f"Pendant_{pi}", root, (lx, ly, z_ceil), 2.92, mats, shade_mat=(mats["brass_gold"] if pi % 2 == 0 else mats["copper"]), col=col)

    # Flush Interior Window Sill Micro-Props (East sill top Z=1.230, South/North sill top Z=1.270)
    _add_potted_herb("Sill_FrontS", root, (front_x - 0.12, -4.15, 1.230), mats["ceramic_sage"], mats, scale=0.92, col=col)
    add_apothecary_bottle("Sill_Flask_FrontS", root, (front_x - 0.12, -3.10, 1.230), 0.24, 0.065, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    _add_book_cluster("Sill_FrontN", root, (front_x - 0.12, 3.65, 1.230), 3, mats, seed=1, col=col)
    _add_potted_herb("Sill_SouthW", root, (-2.05, -hy + 0.26, 1.270), mats["terracotta_roof"], mats, scale=0.92, col=col)
    add_apothecary_bottle("Sill_Jar_SouthW", root, (-1.15, -hy + 0.26, 1.270), 0.24, 0.065, mats["jade_glass"], mats, stopper_mat=mats["honey_wood"], col=col)
    _add_book_cluster("Sill_SouthE", root, (2.10, -hy + 0.26, 1.270), 3, mats, seed=4, rot_z=math.pi * 0.5, col=col)
    _add_potted_herb("Sill_NorthW", root, (-1.60, hy - 0.26, 1.270), mats["ceramic_sage"], mats, scale=0.90, col=col)
    add_apothecary_bottle("Sill_Flask_NorthE", root, (2.10, hy - 0.26, 1.270), 0.24, 0.065, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)

    # 6 Paneled Octagonal Display Pedestals (velvet cushion pad flush at Z=1.060)
    for idx, (px, py) in enumerate(((2.0, -2.5), (0.0, -2.5), (-1.6, -2.5), (2.0, 2.5), (0.0, 2.5), (-1.6, 2.5))):
        add_pedestal_oct(f"Pedestal_{idx + 1}", root, (px, py, 0.0), 1.048, mats, is_cream=(idx % 2 == 1), col=col)
        add_cyl(f"Pedestal_VelvetPad_{idx + 1}", root, (px, py, 1.054), 0.36, 0.36, 0.012, mats["crimson_velvet"], verts=8, col=col)

    # Side Credenzas (top Z=1.015) packed with Sculpted Everyday Decor (keeping Cred_Chest untouched!)
    for sgn, sname in ((-1.0, "S"), (1.0, "N")):
        cy_c = sgn * 5.25
        add_box(f"SideCredenza_{sname}", root, (1.2, cy_c, 0.24 + 0.36), (4.6, 0.65, 0.72), mats["honey_wood"], col=col)
        add_box(f"SideCredenzaTop_{sname}", root, (1.2, cy_c, 0.98), (4.7, 0.72, 0.07), mats["wood_dark"], col=col)
        add_candlestick(f"Cred_Candle_{sname}", root, (-0.92, cy_c, 1.015), mats, rot_z=(0.4 * -sgn), col=col)
        _add_book_cluster(f"Cred_Books_{sname}", root, (-0.35, cy_c, 1.015), 4, mats, seed=(2 if sgn < 0 else 5), rot_z=math.pi * 0.5, col=col)
        add_sculpted_vase(f"Cred_Vase_{sname}", root, (0.35, cy_c, 1.015), 0.34, 0.10, mats["ceramic_sage"] if sgn < 0 else mats["terracotta_roof"], mats, with_handles=True, col=col)
        add_apothecary_bottle(f"Cred_Flask_{sname}", root, (0.90, cy_c, 1.015), 0.26, 0.07, mats["amber_glass"] if sgn < 0 else mats["jade_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
        add_scroll_bundle(f"Cred_Scroll_{sname}", root, (1.72, cy_c, 1.015), 0.38, 0.055, mats["parchment"], mats, rot_z=0.14 * sgn, col=col)
        add_mini_treasure_chest(f"Cred_Chest_{sname}", root, (2.65, cy_c, 1.015), (0.28, 0.34, 0.22), mats["crimson_velvet"] if sgn < 0 else mats["box_teal"], mats, rot_z=(math.pi * 0.5 if sgn < 0 else -math.pi * 0.5), col=col)
    add_tea_set("Cred_TeaSet_S", root, (3.24, -5.25, 1.015), mats, rot_z=0.12, col=col)
    add_table_lantern("Cred_Lantern_N", root, (3.24, 5.25, 1.015), mats, rot_z=-0.25, col=col)

    # Flush-Mounted Wall Storytelling (Lined Charts, Pendulum Wall Clock, Charter, Bracket Sconces & Curio Shelves)
    for sgn, sname in ((-1.0, "South"), (1.0, "North")):
        wy_wall = sgn * (hy - 0.29)
        in_d = -sgn
        for ci, (px, ch_mat) in enumerate(((0.25, "parchment"), (-3.65, "blueprint_blue"))):
            add_box(f"ChartFrame_{sname}_{ci}", root, (px, wy_wall, 2.05), (1.02, 0.05, 0.82), mats["wood_dark"], col=col)
            add_box(f"ChartCanvas_{sname}_{ci}", root, (px, wy_wall + in_d * 0.025, 2.05), (0.86, 0.03, 0.66), mats[ch_mat], col=col)
            add_chart_details(f"ChartDet_{sname}_{ci}", root, (px, wy_wall + in_d * 0.025, 2.05), in_d, 0.86, 0.66, ("map" if ci == 0 else "blueprint"), mats, col=col)
            add_cyl(f"ChartSeal_{sname}_{ci}", root, (px + 0.24, wy_wall + in_d * 0.042, 1.88), 0.09, 0.09, 0.02, mats["wax_red"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)

        add_bracket_lantern(f"WallSconce_{sname}", root, (0.25, sgn * (hy - 0.28), 2.88), (0.0, in_d), mats, col=col)

        wy_shelf = sgn * (hy - 0.44)
        add_box(f"WallCurioShelf_{sname}", root, (-3.65, wy_shelf, 2.75), (1.16, 0.32, 0.05), mats["honey_wood"], col=col)
        for bx_cor, btag in ((-4.05, "L"), (-3.25, "R")):
            add_box(f"WallCurioCleat_{sname}_{btag}", root, (bx_cor, wy_shelf + sgn * 0.13, 2.60), (0.07, 0.06, 0.24), mats["wood_dark"], col=col)
            add_limb(f"WallCurioStrut_{sname}_{btag}", root, (bx_cor, wy_shelf + sgn * 0.13, 2.50), (bx_cor, wy_shelf - sgn * 0.12, 2.73), 0.028, 0.028, mats["wood_dark"], segments=4, col=col)
        _add_book_cluster(f"WallShelfBooks_{sname}", root, (-3.85, wy_shelf, 2.775), 3, mats, seed=(2 if sgn < 0 else 4), rot_z=math.pi * 0.5, col=col)
        add_sculpted_vase(f"WallShelfJar_{sname}", root, (-3.32, wy_shelf, 2.775), 0.30, 0.088, mats["amber_glass"] if sgn < 0 else mats["ceramic_sage"], mats, with_handles=False, col=col)

    # Grand Pendulum Wall Clock & Framed Guild Charter on open Back West bays (flush on inner face X=-4.695)
    add_wall_clock("WallClock_BackS", root, (back_x + 0.165, -3.25, 2.28), (1.0, 0.0), mats, col=col)
    add_box("Charter_Frame_BackN", root, (back_x + 0.19, 3.25, 2.22), (0.05, 1.02, 0.82), mats["wood_dark"], col=col)
    add_box("Charter_Canvas_BackN", root, (back_x + 0.215, 3.25, 2.22), (0.03, 0.86, 0.66), mats["parchment"], col=col)
    add_box("Charter_Header_BackN", root, (back_x + 0.232, 3.25, 2.42), (0.008, 0.66, 0.028), mats["crimson_plank"], col=col)
    for li, lz in enumerate((2.32, 2.23, 2.14, 2.05)):
        add_box(f"Charter_Line_BackN_{li}", root, (back_x + 0.232, 3.16, lz), (0.008, 0.48 - li * 0.05, 0.018), mats["walnut_dark"], col=col)
    add_cyl("Charter_Seal_BackN", root, (back_x + 0.236, 3.52, 2.04), 0.09, 0.09, 0.02, mats["wax_red"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)

    # Grand Dì Mận Checkout Counter (-3.10, 0.0, top Z=1.070) — keeping (-3.08, ±0.55) open for Unity LP_Item_V06/V10!
    add_box("Counter_Body", root, (-3.10, 0.0, 0.24 + 0.38), (0.94, 3.70, 0.76), mats["honey_wood"], col=col)
    add_box("Counter_Top", root, (-3.10, 0.0, 1.03), (1.10, 3.90, 0.08), mats["wood_dark"], col=col)
    add_cyl("Counter_FootRail", root, (-2.50, 0.0, 0.34), 0.028, 0.028, 3.55, mats["brass_gold"], verts=8, rot=(math.pi * 0.5, 0, 0), col=col)
    for cy in (-1.2, 0.0, 1.2):
        add_box(f"Counter_Trim_{cy:.1f}", root, (-2.62, cy, 0.62), (0.04, 0.92, 0.54), mats["warm_cream"], col=col)

    add_banker_lamp("CounterBanker", root, (-3.15, -1.55, 1.070), mats, rot_z=0.18, col=col)
    add_brass_scale("CounterScale", root, (-3.06, -1.02, 1.070), mats, rot_z=0.0, col=col)
    add_wax_seal_kit("CounterWaxKit", root, (-2.96, 0.04, 1.070), mats, rot_z=0.0, col=col)
    add_open_ledger_and_quill("CounterLedger", root, (-3.04, 1.08, 1.070), mats, rot_z=-0.08, col=col)
    add_abacus("CounterAbacus", root, (-3.04, 1.64, 1.070), mats, rot_z=-0.12, col=col)

    # 18-Cubby Mahogany Relic Cabinet (Back front face -4.52, shelves X=-4.40 front -4.18 -> 34cm clear depth!)
    add_box("Cabinet_Back", root, (-4.62, 0.0, 0.24 + 1.56), (0.20, 4.40, 3.12), mats["wood_dark"], col=col)
    for sz in (1.02, 1.72, 2.42, 3.12):
        add_box(f"Cab_Shelf_{sz:.2f}", root, (-4.40, 0.0, sz), (0.44, 4.20, 0.06), mats["honey_wood"], col=col)
    for sy in (-2.08, -1.04, 0.0, 1.04, 2.08):
        add_box(f"Cab_Div_{sy:.2f}", root, (-4.40, sy, 1.78), (0.42, 0.06, 2.74), mats["wood_dark"], col=col)
    add_box("Cabinet_Crown", root, (-4.38, 0.0, 3.38), (0.50, 4.48, 0.12), mats["brass_gold"], col=col)
    add_box("Cabinet_TopGlow", root, (-4.38, 0.0, 3.24), (0.16, 4.00, 0.06), mats["warm_lantern"], col=col)

    # Sculpted Cabinet Props flush at shelf tops (1.750 outer bays & 2.450 top bays; middle showcase bays reserved for Unity)
    _add_book_cluster("Cab_Books_L", root, (-4.35, -1.56, 1.750), 4, mats, seed=1, col=col)
    add_sculpted_vase("Cab_Vase_R1", root, (-4.35, 1.40, 1.750), 0.34, 0.10, mats["ceramic_sage"], mats, with_handles=True, col=col)
    add_apothecary_bottle("Cab_Bottle_R2", root, (-4.35, 1.74, 1.750), 0.30, 0.075, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)

    for ci, (sy, tm) in enumerate(((-1.56, "box_purple"), (-0.52, "box_teal"), (0.52, "crimson_plank"), (1.56, "sunny_yellow"))):
        add_mini_treasure_chest(f"Cab_TopChest_{ci}", root, (-4.35, sy - 0.14, 2.450), (0.24, 0.30, 0.22), mats[tm], mats, col=col)
        if ci % 2 == 0:
            add_scroll_bundle(f"Cab_TopScroll_{ci}", root, (-4.35, sy + 0.22, 2.450), 0.28, 0.048, mats["parchment"], mats, rot_z=0.10, col=col)
        else:
            add_apothecary_bottle(f"Cab_TopBottle_{ci}", root, (-4.35, sy + 0.22, 2.450), 0.26, 0.065, mats["jade_glass"], mats, stopper_mat=mats["honey_wood"], col=col)
    return root


# ==================== 2. TIỆM MỞ HÒM (COZY VERTICAL SLICE: REF #2 TIMBER CRAFT + CLOCKWORK WORKBENCH) ====================

def _add_clapboard_strip_y(prefix, parent, face_x, y_min, y_max, z_bot, z_top, mats, dir_x=1.0, step_w=0.24, col=None):
    """Real 3D vertical wooden clapboard slats along an East/West exterior facade for crisp shadow grooves."""
    span = y_max - y_min
    if span < 0.12 or (z_top - z_bot) < 0.12:
        return
    count = max(1, int(round(span / step_w)))
    bw = span / count
    cz = (z_bot + z_top) * 0.5
    bh = z_top - z_bot
    for i in range(count):
        by = y_min + (i + 0.5) * bw
        m = mats["crimson_plank"] if i % 2 == 0 else mats["crimson_plank_b"]
        add_box(f"{prefix}_{i}", parent, (face_x + dir_x * 0.022, by, cz), (0.044, bw * 0.88, bh), m, col=col)


def _add_clapboard_strip_x(prefix, parent, face_y, x_min, x_max, z_bot, z_top, mats, dir_y=1.0, step_w=0.24, col=None):
    """Real 3D vertical wooden clapboard slats along a North/South exterior facade."""
    span = x_max - x_min
    if span < 0.12 or (z_top - z_bot) < 0.12:
        return
    count = max(1, int(round(span / step_w)))
    bw = span / count
    cz = (z_bot + z_top) * 0.5
    bh = z_top - z_bot
    for i in range(count):
        bx = x_min + (i + 0.5) * bw
        m = mats["crimson_plank"] if i % 2 == 0 else mats["crimson_plank_b"]
        add_box(f"{prefix}_{i}", parent, (bx, face_y + dir_y * 0.022, cz), (bw * 0.88, 0.044, bh), m, col=col)


def _add_cut_window(name, parent, center, axis, win_w, win_h, wall_thick, mats, cols=3, rows=2, frame_mat=None, sill_depth=None, col=None):
    """Open cut-through window with thick projecting cream sill, inner/outer mullions, and translucent glass."""
    f_mat = frame_mat or mats["pure_white"]
    cx, cy, cz = center
    depth = wall_thick + 0.14
    sill_depth = sill_depth if sill_depth is not None else (wall_thick + 0.34)
    border = 0.12
    mull = 0.055

    if axis == "x":
        add_box(f"{name}_Glass", parent, (cx, cy, cz), (0.03, win_w - 0.08, win_h - 0.08), mats["glass_sky"], col=col)
        add_box(f"{name}_Top", parent, (cx, cy, cz + win_h * 0.5), (depth, win_w + border * 1.4, border), f_mat, col=col)
        add_box(f"{name}_Bot", parent, (cx, cy, cz - win_h * 0.5), (sill_depth, win_w + border * 1.8, border), f_mat, col=col)
        add_box(f"{name}_Left", parent, (cx, cy - win_w * 0.5, cz), (depth, border, win_h), f_mat, col=col)
        add_box(f"{name}_Right", parent, (cx, cy + win_w * 0.5, cz), (depth, border, win_h), f_mat, col=col)
        for ci in range(1, cols):
            my = cy - win_w * 0.5 + win_w * (ci / float(cols))
            add_box(f"{name}_MV_{ci}", parent, (cx, my, cz), (depth * 0.65, mull, win_h - 0.06), f_mat, col=col)
        for ri in range(1, rows):
            mz = cz - win_h * 0.5 + win_h * (ri / float(rows))
            add_box(f"{name}_MH_{ri}", parent, (cx, cy, mz), (depth * 0.65, win_w - 0.06, mull), f_mat, col=col)
    else:
        add_box(f"{name}_Glass", parent, (cx, cy, cz), (win_w - 0.08, 0.03, win_h - 0.08), mats["glass_sky"], col=col)
        add_box(f"{name}_Top", parent, (cx, cy, cz + win_h * 0.5), (win_w + border * 1.4, depth, border), f_mat, col=col)
        add_box(f"{name}_Bot", parent, (cx, cy, cz - win_h * 0.5), (win_w + border * 1.8, sill_depth, border), f_mat, col=col)
        add_box(f"{name}_Left", parent, (cx - win_w * 0.5, cy, cz), (border, depth, win_h), f_mat, col=col)
        add_box(f"{name}_Right", parent, (cx + win_w * 0.5, cy, cz), (border, depth, win_h), f_mat, col=col)
        for ci in range(1, cols):
            mx = cx - win_w * 0.5 + win_w * (ci / float(cols))
            add_box(f"{name}_MV_{ci}", parent, (mx, cy, cz), (mull, depth * 0.65, win_h - 0.06), f_mat, col=col)
        for ri in range(1, rows):
            mz = cz - win_h * 0.5 + win_h * (ri / float(rows))
            add_box(f"{name}_MH_{ri}", parent, (cx, cy, mz), (win_w - 0.06, depth * 0.65, mull), f_mat, col=col)


def _add_book_cluster(prefix, parent, loc, count, mats, seed=0, rot_z=0.0, col=None):
    """Chunky low-poly standing + leaning hardcover books with leather spines & parchment page blocks."""
    book_mats = (mats["book_navy"], mats["crimson_plank"], mats["book_forest"], mats["book_ochre"], mats["teal_wainscot"])
    bx0, by0, bz0 = loc
    for i in range(count):
        bm = book_mats[(seed + i) % len(book_mats)]
        bh = 0.24 + ((seed + i * 3) % 4) * 0.032
        bt = 0.055 + ((seed + i) % 2) * 0.014
        bd = 0.20 + ((seed + i) % 3) * 0.02
        lean = 0.22 if (i == count - 1 and count >= 3) else 0.0
        step = (i - (count - 1) * 0.5) * 0.072 + (0.04 if lean > 0 else 0.0)
        bx = bx0 - math.sin(rot_z) * step
        by = by0 + math.cos(rot_z) * step
        dz = abs(math.sin(lean)) * (bt * 0.5)

        b_grp = bpy.data.objects.new(f"{prefix}_B_{i}", None)
        b_grp.location = Vector((bx, by, bz0 + dz))
        b_grp.rotation_euler = Euler((lean, 0.0, rot_z))
        if parent:
            b_grp.parent = parent
        link_obj(b_grp, col)

        add_box(f"{prefix}_Spine_{i}", b_grp, (bd * 0.44, 0, bh * 0.5), (bd * 0.14, bt, bh), bm, col=col)
        add_box(f"{prefix}_CoverL_{i}", b_grp, (-bd * 0.03, -bt * 0.42, bh * 0.5), (bd * 0.94, bt * 0.16, bh), bm, col=col)
        add_box(f"{prefix}_CoverR_{i}", b_grp, (-bd * 0.03, bt * 0.42, bh * 0.5), (bd * 0.94, bt * 0.16, bh), bm, col=col)
        add_box(f"{prefix}_Pages_{i}", b_grp, (-bd * 0.04, 0, bh * 0.49), (bd * 0.88, bt * 0.72, bh * 0.94), mats["parchment"], col=col)


def _add_potted_herb(name, parent, loc, pot_mat, mats, scale=1.0, col=None):
    """Turned terracotta/ceramic pot with dark soil, multi-leaf faceted foliage & warm blossom buds at z=0."""
    p = bpy.data.objects.new(name, None)
    p.location = Vector(loc)
    if parent:
        p.parent = parent
    link_obj(p, col)

    pot_prof = [
        (0.062 * scale, 0.0),
        (0.088 * scale, 0.125 * scale),
        (0.104 * scale, 0.130 * scale),
        (0.104 * scale, 0.162 * scale),
        (0.084 * scale, 0.162 * scale),
        (0.080 * scale, 0.140 * scale),
    ]
    add_lathe(f"{name}_Pot", p, (0, 0, 0), pot_prof, pot_mat, segments=8, col=col)
    add_cyl(f"{name}_Soil", p, (0, 0, 0.142 * scale), 0.080 * scale, 0.080 * scale, 0.016 * scale, mats["walnut_dark"], verts=8, col=col)

    leaf_specs = (
        (-0.042, 0.010, 0.215, 0.068),
        (0.044, 0.028, 0.232, 0.064),
        (0.008, -0.044, 0.205, 0.060),
        (-0.020, -0.025, 0.255, 0.052),
        (0.022, -0.015, 0.265, 0.048),
    )
    for li, (ox, oy, oz, sc) in enumerate(leaf_specs):
        add_ico(f"{name}_Leaf_{li}", p, (ox * scale, oy * scale, oz * scale),
                (sc * scale, sc * scale, sc * 1.15 * scale),
                mats["tree_leaf_bright"] if li % 2 == 0 else mats["tree_leaf_mid"], subdivisions=1, col=col)
    for bi, (bx, by, bz, bm) in enumerate(((-0.030, 0.020, 0.265, "sunny_yellow"), (0.032, -0.020, 0.275, "wax_red"))):
        add_ico(f"{name}_Bud_{bi}", p, (bx * scale, by * scale, bz * scale),
                (0.018 * scale, 0.018 * scale, 0.018 * scale), mats[bm], subdivisions=1, col=col)
    return p


def build_tiem_mo_hom(mats, font, col):
    """
    COZY VERTICAL SLICE ("Góc Mẫu") — Reference #2 Warm Timber Craft + Clockwork Appraisal Atelier:
    - Real cut-through 6-pane & 4-pane open windows pouring sunlight inside + deep sills with pots & vials.
    - 3D vertical crimson clapboard siding, warm cream half-timbered upper story, overhanging roof with exposed rafter tails.
    - Raised timber front porch deck + wide wooden steps + dark charcoal sloped handrails, dual warm lanterns & vintage flag.
    - Intimate eye-level vaulted timber rafter ceiling (Z=3.55..4.35m) + 3 hanging brass pendant lamps + oak plank floor.
    - Packed 4-tier West shelf (25+ books, jars, scrolls, chests), South tool pegboard & gears, North noticeboard & cargo,
      and Hero Clockwork Unboxing Workbench (preserving exact BoxSlot coordinates (-0.30, 1.073, -0.95/0/+0.95)).
    """
    depth_x, width_y, f1_h, f2_h = 9.0, 10.0, 3.40, 2.00
    hx, hy = depth_x * 0.5, width_y * 0.5
    door_y, door_w, door_h = 0.8, 2.6, 2.85
    belt_z = 0.24 + f1_h
    eave_z = belt_z + f2_h
    ridge_h = 2.05
    txt_rot = (math.pi * 0.5, 0.0, math.pi * 0.5)

    root = bpy.data.objects.new("LP_Shop_TiemMoHom", None)
    link_obj(root, col)

    # 0. Stone Plinth & Warm Staggered Oak Plank Floor (leading East->West inside!)
    add_box("BasePlinth", root, (0.0, 0.0, 0.12), (depth_x, width_y, 0.24), mats["curb_stone"], col=col)
    add_box("FloorBase", root, (0.0, 0.0, 0.236), (depth_x - 0.30, width_y - 0.30, 0.016), mats["walnut_dark"], col=col)
    for row_i in range(14):
        py = -4.55 + row_i * 0.70
        for col_i in range(5):
            px = -3.45 + col_i * 1.72 + (0.35 if row_i % 2 == 1 else -0.15)
            pw = min(1.66, (hx - 0.24) - (px - 0.83))
            if pw < 0.45:
                continue
            p_mat = mats["oak_plank_a"] if (row_i + col_i) % 3 != 0 else mats["oak_plank_b"]
            add_box(f"OakPlank_{row_i}_{col_i}", root, (px, py, 0.246), (1.64, 0.66, 0.014), p_mat, col=col)

    # 1. Back Wall (-X) & South Wall (-Y) — Solid Structural Envelope + Wainscoting
    back_x = -hx + 0.14
    total_h = f1_h + f2_h
    add_box("Wall_Back_F1", root, (back_x, 0.0, 0.24 + f1_h * 0.5), (0.24, width_y - 0.20, f1_h), mats["crimson_dark"], col=col)
    _add_clapboard_strip_y("Clap_Back", root, -hx, -hy + 0.22, hy - 0.22, 0.24, belt_z, mats, dir_x=-1.0, col=col)
    add_box("Wall_Back_F2", root, (back_x, 0.0, belt_z + f2_h * 0.5), (0.24, width_y - 0.20, f2_h), mats["warm_cream"], col=col)
    add_box("Int_Wain_Back", root, (back_x + 0.14, 0.0, 0.24 + 0.52), (0.05, width_y - 0.50, 1.04), mats["teal_wainscot"], col=col)
    add_box("Int_Chair_Back", root, (back_x + 0.17, 0.0, 1.28), (0.08, width_y - 0.50, 0.09), mats["wood_dark"], col=col)
    add_box("Int_Upper_Back", root, (back_x + 0.13, 0.0, 1.32 + 1.15), (0.05, width_y - 0.50, 2.26), mats["warm_cream"], col=col)

    wy_s = -hy + 0.14
    add_box("Wall_South_F1", root, (0.0, wy_s, 0.24 + f1_h * 0.5), (depth_x - 0.20, 0.24, f1_h), mats["crimson_dark"], col=col)
    _add_clapboard_strip_x("Clap_South", root, -hy, -hx + 0.22, hx - 0.22, 0.24, belt_z, mats, dir_y=-1.0, col=col)
    add_box("Wall_South_F2", root, (0.0, wy_s, belt_z + f2_h * 0.5), (depth_x - 0.20, 0.24, f2_h), mats["warm_cream"], col=col)
    add_box("Int_Wain_South", root, (0.0, wy_s + 0.14, 0.24 + 0.52), (depth_x - 0.50, 0.05, 1.04), mats["teal_wainscot"], col=col)
    add_box("Int_Chair_South", root, (0.0, wy_s + 0.17, 1.28), (depth_x - 0.50, 0.08, 0.09), mats["wood_dark"], col=col)
    add_box("Int_Upper_South", root, (0.0, wy_s + 0.13, 1.32 + 1.15), (depth_x - 0.50, 0.05, 2.26), mats["warm_cream"], col=col)

    # 2. North Wall (+Y) WITH REAL CUT-THROUGH SUN WINDOW at X=-0.35 (span -1.45..+0.75, Z=1.25..2.70)
    wy_n = hy - 0.14
    for seg_name, x0, x1 in (("NW", -hx + 0.10, -1.45), ("NE", 0.75, hx - 0.10)):
        sw = x1 - x0
        xc = (x0 + x1) * 0.5
        add_box(f"Wall_North_F1_{seg_name}", root, (xc, wy_n, 0.24 + f1_h * 0.5), (sw, 0.24, f1_h), mats["crimson_dark"], col=col)
        _add_clapboard_strip_x(f"Clap_North_{seg_name}", root, hy, max(-hx + 0.22, x0), min(hx - 0.22, x1), 0.24, belt_z, mats, dir_y=1.0, col=col)
        add_box(f"Int_Wain_N_{seg_name}", root, (xc, wy_n - 0.14, 0.24 + 0.52), (sw, 0.05, 1.04), mats["teal_wainscot"], col=col)
        add_box(f"Int_Chair_N_{seg_name}", root, (xc, wy_n - 0.17, 1.28), (sw, 0.08, 0.09), mats["wood_dark"], col=col)
        add_box(f"Int_Upper_N_{seg_name}", root, (xc, wy_n - 0.13, 1.32 + 1.15), (sw, 0.05, 2.26), mats["warm_cream"], col=col)
    # North window spandrel below (Z=0.24..1.25) & header above (Z=2.70..3.64)
    add_box("Wall_North_WinBot", root, (-0.35, wy_n, 0.745), (2.20, 0.24, 1.01), mats["crimson_dark"], col=col)
    _add_clapboard_strip_x("Clap_North_Bot", root, hy, -1.45, 0.75, 0.24, 1.22, mats, dir_y=1.0, col=col)
    add_box("Int_Wain_N_WinBot", root, (-0.35, wy_n - 0.14, 0.745), (2.20, 0.05, 1.01), mats["teal_wainscot"], col=col)
    add_box("Wall_North_WinTop", root, (-0.35, wy_n, 3.17), (2.20, 0.24, 0.94), mats["crimson_dark"], col=col)
    _add_clapboard_strip_x("Clap_North_Top", root, hy, -1.45, 0.75, 2.74, belt_z, mats, dir_y=1.0, col=col)
    add_box("Int_Upper_N_WinTop", root, (-0.35, wy_n - 0.13, 3.17), (2.20, 0.05, 0.94), mats["warm_cream"], col=col)
    add_box("Wall_North_F2", root, (0.0, wy_n, belt_z + f2_h * 0.5), (depth_x - 0.20, 0.24, f2_h), mats["warm_cream"], col=col)
    _add_cut_window("Win_NorthSun", root, (-0.35, wy_n, 1.975), "y", 2.20, 1.45, 0.26, mats, cols=3, rows=2, col=col)

    # 3. East Front Wall (+X) WITH DOORWAY + 2 REAL CUT-THROUGH MULTI-PANE WINDOWS!
    # Y layout: [-5.0..-3.775 PierS] [-3.775..-1.725 WinS] [-1.725..-0.50 PierMid] [-0.50..+2.10 Door] [+2.10..+2.80 PierR] [+2.80..+4.30 WinN] [+4.30..+5.00 PierN]
    front_x = hx - 0.14
    for p_name, y0, y1 in (("S", -hy + 0.10, -3.775), ("Mid", -1.725, -0.50), ("R", 2.10, 2.80), ("N", 4.30, hy - 0.10)):
        pw = y1 - y0
        yc = (y0 + y1) * 0.5
        add_box(f"FrontPier_{p_name}", root, (front_x, yc, 0.24 + f1_h * 0.5), (0.26, pw, f1_h), mats["crimson_dark"], col=col)
        _add_clapboard_strip_y(f"Clap_Front_{p_name}", root, hx, max(-hy + 0.22, y0), min(hy - 0.22, y1), 0.24, belt_z, mats, dir_x=1.0, col=col)
        add_box(f"Int_Wain_F_{p_name}", root, (front_x - 0.14, yc, 0.24 + 0.52), (0.05, max(0.2, pw - 0.08), 1.04), mats["teal_wainscot"], col=col)
        add_box(f"Int_Chair_F_{p_name}", root, (front_x - 0.17, yc, 1.28), (0.08, max(0.2, pw - 0.08), 0.09), mats["wood_dark"], col=col)
        add_box(f"Int_Upper_F_{p_name}", root, (front_x - 0.13, yc, 1.32 + 1.15), (0.05, max(0.2, pw - 0.08), 2.26), mats["warm_cream"], col=col)

    # Front Windows Spandrels & Headers + Cut Windows
    for w_name, wy_c, ww, w_cols in (("Win_EastS", -2.75, 2.05, 3), ("Win_EastN", 3.55, 1.50, 2)):
        add_box(f"Wall_Front_Bot_{w_name}", root, (front_x, wy_c, 0.695), (0.26, ww, 0.91), mats["crimson_dark"], col=col)
        _add_clapboard_strip_y(f"Clap_Front_Bot_{w_name}", root, hx, wy_c - ww * 0.5, wy_c + ww * 0.5, 0.24, 1.12, mats, dir_x=1.0, col=col)
        add_box(f"Int_Wain_Bot_{w_name}", root, (front_x - 0.14, wy_c, 0.695), (0.05, ww, 0.91), mats["teal_wainscot"], col=col)
        add_box(f"Wall_Front_Top_{w_name}", root, (front_x, wy_c, 3.17), (0.26, ww, 0.94), mats["crimson_dark"], col=col)
        _add_clapboard_strip_y(f"Clap_Front_Top_{w_name}", root, hx, wy_c - ww * 0.5, wy_c + ww * 0.5, 2.74, belt_z, mats, dir_x=1.0, col=col)
        add_box(f"Int_Upper_Top_{w_name}", root, (front_x - 0.13, wy_c, 3.17), (0.05, ww, 0.94), mats["warm_cream"], col=col)
        _add_cut_window(w_name, root, (front_x, wy_c, 1.925), "x", ww, 1.55, 0.28, mats, cols=w_cols, rows=2, col=col)

    # Door Lintel + Thick Cream Door Framing
    add_box("FrontLintel_Door", root, (front_x, door_y, 0.24 + door_h + (f1_h - door_h) * 0.5), (0.26, door_w + 0.10, f1_h - door_h), mats["crimson_dark"], col=col)
    _add_clapboard_strip_y("Clap_DoorLintel", root, hx, door_y - door_w * 0.5, door_y + door_w * 0.5, 0.24 + door_h + 0.06, belt_z, mats, dir_x=1.0, col=col)
    add_box("DoorFrame_L", root, (hx, door_y - door_w * 0.5 - 0.07, 0.24 + door_h * 0.5), (0.36, 0.18, door_h), mats["pure_white"], col=col)
    add_box("DoorFrame_R", root, (hx, door_y + door_w * 0.5 + 0.07, 0.24 + door_h * 0.5), (0.36, 0.18, door_h), mats["pure_white"], col=col)
    add_box("DoorFrame_Top", root, (hx, door_y, 0.24 + door_h + 0.09), (0.40, door_w + 0.36, 0.22), mats["pure_white"], col=col)

    # Upper F2 Half-Timbered Story + Belt Trim + Corner Timber Quoin Posts
    add_box("Wall_Front_F2", root, (front_x, 0.0, belt_z + f2_h * 0.5), (0.26, width_y - 0.20, f2_h), mats["warm_cream"], col=col)
    add_box("BeltTrim_F", root, (hx + 0.06, 0.0, belt_z), (0.20, width_y + 0.16, 0.16), mats["pure_white"], col=col)
    add_box("BeltTrim_S", root, (0.0, -hy - 0.06, belt_z), (depth_x + 0.16, 0.20, 0.16), mats["pure_white"], col=col)
    add_box("BeltTrim_N", root, (0.0, hy + 0.06, belt_z), (depth_x + 0.16, 0.20, 0.16), mats["pure_white"], col=col)
    for cx in (-hx + 0.08, hx - 0.08):
        for cy in (-hy + 0.08, hy - 0.08):
            add_box(f"CornerPost_{cx:.1f}_{cy:.1f}", root, (cx, cy, 0.24 + total_h * 0.5), (0.24, 0.24, total_h + 0.06), mats["pure_white"], col=col)
    for fy_stud in (-4.4, -1.2, 2.2, 4.4):
        add_box(f"F2_Stud_{fy_stud:.1f}", root, (hx + 0.02, fy_stud, belt_z + f2_h * 0.5), (0.06, 0.14, f2_h - 0.08), mats["wood_dark"], col=col)
    for wy_u in (-2.85, 3.55):
        _add_cut_window(f"F2_FrontWin_{wy_u:.1f}", root, (hx + 0.02, wy_u, belt_z + 1.05), "x", 1.20, 1.15, 0.20, mats, cols=2, rows=2, col=col)

    # Deep Terracotta Roof + Exposed Rafter Tails under Front Eave!
    add_pitched_terracotta_roof("Roof", root, eave_z, hx, hy, ridge_h, mats, gable_mat=mats["warm_cream"], interior_lining_mat=mats["walnut_dark"], col=col)
    for rti in range(13):
        rty = -4.6 + rti * (9.2 / 12.0)
        add_box(f"Ext_RafterTail_{rti}", root, (hx + 0.30, rty, eave_z - 0.14), (0.64, 0.11, 0.14), mats["wood_dark"], rot=(0, 0.35, 0), col=col)

    # 4. Exterior Porch Deck, Timber Stairs, Dark Charcoal Sloped Handrails (Ref #2 signature!), Sign & Props
    # Raised front timber porch deck outside doorway (X=4.50..5.65, Y=-1.10..+2.70, Z top=0.24)
    add_box("Porch_SubBase", root, (5.06, 0.80, 0.14), (1.14, 3.76, 0.12), mats["wood_dark"], col=col)
    for pi in range(5):
        px_d = 4.61 + pi * 0.22
        add_box(f"Porch_DeckPlank_{pi}", root, (px_d, 0.80, 0.22), (0.20, 3.80, 0.04), mats["timber_light"] if pi % 2 == 0 else mats["honey_wood"], col=col)
    # 2 wide stepped timber stairs descending cleanly to sidewalk (Z top 0.16, 0.08)
    add_box("Porch_StairUpper", root, (5.84, 0.80, 0.12), (0.38, 3.12, 0.08), mats["timber_light"], col=col)
    add_box("Porch_StairLower", root, (6.22, 0.80, 0.08), (0.38, 3.12, 0.04), mats["honey_wood"], col=col)
    add_box("Porch_StairStringL", root, (6.02, -0.78, 0.12), (0.82, 0.09, 0.16), mats["wood_dark"], rot=(0, 0.20, 0), col=col)
    add_box("Porch_StairStringR", root, (6.02, 2.38, 0.12), (0.82, 0.09, 0.16), mats["wood_dark"], rot=(0, 0.20, 0), col=col)

    # Sloped Dark Charcoal Handrails flanking BOTH sides of stairs & porch deck (Y=-0.82 and Y=+2.42)
    for r_sgn, ry_rail in (("L", -0.82), ("R", 2.42)):
        add_box(f"Rail_TopPost_{r_sgn}", root, (5.58, ry_rail, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_box(f"Rail_BotPost_{r_sgn}", root, (6.40, ry_rail, 0.50), (0.09, 0.09, 0.84), mats["charcoal"], col=col)
        add_box(f"Rail_WallPost_{r_sgn}", root, (4.62, ry_rail + (-0.24 if r_sgn == "L" else 0.24), 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_limb(f"Rail_SlopedBar_{r_sgn}", root, (5.58, ry_rail, 1.12), (6.40, ry_rail, 0.90), 0.036, 0.036, mats["charcoal"], segments=6, col=col)
        add_limb(f"Rail_MidBar_{r_sgn}", root, (5.58, ry_rail, 0.72), (6.40, ry_rail, 0.52), 0.022, 0.022, mats["charcoal"], segments=6, col=col)
        add_limb(f"Rail_DeckBar_{r_sgn}", root, (4.62, ry_rail, 1.12), (5.58, ry_rail, 1.12), 0.032, 0.032, mats["charcoal"], segments=6, col=col)
        ry_ret = -1.06 if r_sgn == "L" else 2.66
        add_limb(f"Rail_ReturnBar_{r_sgn}", root, (4.62, ry_ret, 1.12), (5.58, ry_ret, 1.12), 0.032, 0.032, mats["charcoal"], segments=6, col=col)
        add_box(f"Rail_CornerPost_{r_sgn}", root, (5.58, ry_ret, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_limb(f"Rail_FrontReturn_{r_sgn}", root, (5.58, ry_ret, 1.12), (5.58, ry_rail, 1.12), 0.032, 0.032, mats["charcoal"], segments=6, col=col)

    # Timber Door Hood Canopy with angled corbel brackets + Signboard + Gear + Flag + Flower Box!
    add_box("DoorHood_Roof", root, (hx + 0.42, 0.80, 3.30), (0.86, 3.60, 0.12), mats["terracotta_roof"], rot=(0, 0.24, 0), col=col)
    add_box("DoorHood_Trim", root, (hx + 0.80, 0.80, 3.21), (0.10, 3.64, 0.10), mats["wood_dark"], col=col)
    for cby in (-0.82, 2.42):
        add_limb(f"DoorHood_Bracket_{cby:.1f}", root, (hx + 0.06, cby, 2.86), (hx + 0.68, cby, 3.22), 0.055, 0.055, mats["wood_dark"], segments=6, col=col)

    add_box("Sign_Back", root, (hx + 0.09, 0.80, 3.92), (0.16, 3.80, 0.82), mats["honey_wood"], col=col)
    add_box("Sign_Face", root, (hx + 0.16, 0.80, 3.92), (0.10, 3.54, 0.62), mats["walnut_dark"], col=col)
    for sy_br, btag in ((-0.70, "L"), (2.30, "R")):
        add_box(f"Sign_Bracket_{btag}", root, (hx + 0.01, sy_br, 3.92), (0.14, 0.12, 0.86), mats["charcoal"], col=col)
    add_3d_text_mesh("Sign_Text", root, "TIỆM MỞ HÒM", (hx + 0.222, 0.80, 3.90), txt_rot, 0.32, mats["brass_gold"], font_data=font, col=col)
    add_gear_wheel("Ext_GableGear_Big", root, (hx + 0.14, 0.80, belt_z + 1.48), 0.56, 0.14, 8, mats["brass_gold"], mats["teal_shop"], rot=(0, math.pi * 0.5, 0), col=col)

    # Dual warm wrought-iron bracket lanterns flanking the doorway (zero protruding upper cord!)
    for ly_p, ltag in ((-0.75, "L"), (2.35, "R")):
        add_bracket_lantern(f"PorchLan_{ltag}", root, (hx, ly_p, 2.90), (1.0, 0.0), mats, col=col)

    # Angled hanging shop pennant flag on front pier (Ref #2 silhouette charm!)
    add_limb("Flag_Pole", root, (hx + 0.04, -1.35, 3.25), (hx + 0.92, -1.35, 3.72), 0.032, 0.026, mats["wood_dark"], segments=6, col=col)
    add_box("Flag_ClothA", root, (hx + 0.70, -1.35, 3.24), (0.48, 0.03, 0.46), mats["teal_shop"], rot=(0, 0.15, 0), col=col)
    add_box("Flag_Stripe", root, (hx + 0.70, -1.35, 3.24), (0.14, 0.04, 0.46), mats["sunny_yellow"], rot=(0, 0.15, 0), col=col)

    # Flush-mounted corbelled flower planter box under Win_EastS & North-East porch cargo cluster
    add_window_flowerbox("WinS_Flower", root, (hx, -2.75, 0.96), "x", 1.0, 1.95, mats, col=col)
    add_faceted_barrel("Ext_NE_Barrel", root, (hx + 0.55, 4.35, 0.08), 0.28, 0.72, mats, col=col)
    add_wooden_crate("Ext_NE_Crate", root, (hx + 0.58, 3.55, 0.36), (0.58, 0.58, 0.56), mats, rot=(0, 0, -0.18), col=col)

    # === COZY INTERIOR: EYE-VISIBLE EXPOSED TIMBER RAFTER CEILING (Z=3.52..4.32m), WAINSCOTING & RICH 3-TIER PROPS ===
    # 5. Eye-Level Warm Vaulted Ceiling & Heavy Timber Rafters (clearly framed in first-person camera!)
    ceil_eave_z = 3.52
    ceil_peak_z = 4.30
    ceil_ang = math.atan2(ceil_peak_z - ceil_eave_z, hx - 0.16)
    ceil_slope = math.hypot(hx - 0.16, ceil_peak_z - ceil_eave_z)
    for sgn in (-1.0, 1.0):
        add_box(f"Int_CeilDeck_{sgn}", root, (sgn * hx * 0.50, 0.0, (ceil_eave_z + ceil_peak_z) * 0.5 + 0.06),
                (ceil_slope + 0.18, width_y - 0.44, 0.10), mats["timber_light"], rot=(0, sgn * ceil_ang, 0), col=col)
    add_box("Int_RidgeBeam", root, (0.0, 0.0, ceil_peak_z - 0.08), (0.24, width_y - 0.44, 0.26), mats["wood_dark"], col=col)
    for ty in (-4.0, -2.0, 0.0, 2.0, 4.0):
        add_box(f"Ceil_TieBeam_{ty:.1f}", root, (0.0, ty, ceil_eave_z - 0.06), (depth_x - 0.52, 0.20, 0.22), mats["wood_dark"], col=col)
        add_box(f"Ceil_KingPost_{ty:.1f}", root, (0.0, ty, ceil_eave_z + 0.36), (0.18, 0.18, 0.72), mats["wood_dark"], col=col)
        for rsgn in (-1.0, 1.0):
            add_box(f"Ceil_Rafter_{ty:.1f}_{rsgn}", root, (rsgn * hx * 0.5, ty, (ceil_eave_z + ceil_peak_z) * 0.5 - 0.04),
                    (ceil_slope, 0.15, 0.18), mats["honey_wood"], rot=(0, rsgn * ceil_ang, 0), col=col)

    # Interior dark timber pilasters lining corners & mid-walls (clear of pegboard X=-2.50..+1.70!)
    for px, py in ((-4.22, -4.68), (-4.22, 4.68), (4.22, -4.68), (4.22, 4.68), (-2.58, -4.68), (1.8, -4.68), (-1.9, 4.68), (1.2, 4.68)):
        add_box(f"Int_TimberPost_{px:.1f}_{py:.1f}", root, (px, py, 0.24 + 1.66), (0.18, 0.18, 3.32), mats["wood_dark"], col=col)

    # 3 Sculpted Hanging Pendant Lamps seated flush on sloped vaulted ceiling over the Workbench!
    for pi, (lx, ly) in enumerate(((-0.30, -1.65), (-0.30, 0.0), (-0.30, 1.65))):
        z_ceil = (ceil_eave_z + ceil_peak_z) * 0.5 + 0.06 + (hx * 0.50 - abs(lx)) * math.tan(ceil_ang) - 0.045
        add_pendant_lamp(f"Pendant_{pi}", root, (lx, ly, z_ceil), 2.86, mats, shade_mat=(mats["brass_gold"] if pi == 1 else mats["copper"]), col=col)

    # 6. Window Sill Micro-Props flush on cut-window ledges (East top Z=1.210, North top Z=1.310)
    _add_potted_herb("Sill_Herb_S1", root, (hx - 0.12, -3.25, 1.210), mats["ceramic_sage"], mats, scale=0.95, col=col)
    add_apothecary_bottle("Sill_Flask_S", root, (hx - 0.12, -2.35, 1.210), 0.24, 0.065, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    _add_book_cluster("Sill_Books_N", root, (hx - 0.14, 3.55, 1.210), 3, mats, seed=2, rot_z=0.1, col=col)
    _add_potted_herb("Sill_Herb_NorthSun", root, (-0.85, wy_n - 0.12, 1.310), mats["terracotta_roof"], mats, scale=0.95, col=col)
    add_apothecary_bottle("Sill_Jar_NorthSun", root, (0.25, wy_n - 0.12, 1.310), 0.24, 0.068, mats["jade_glass"], mats, stopper_mat=mats["honey_wood"], col=col)

    # 7. Woven Crimson & Brass Rug anchoring Central Workbench
    add_box("Atelier_RugTrim", root, (-0.30, 0.0, 0.254), (3.10, 4.50, 0.008), mats["brass_gold"], col=col)
    add_box("Atelier_RugBody", root, (-0.30, 0.0, 0.258), (2.86, 4.26, 0.008), mats["crimson_velvet"], col=col)

    # 8. South Wall (Y=-4.30): Master Unsealing Tool Pegboard + Gears + Packed Side Craft Bench
    peg_y = -hy + 0.34
    add_box("ToolBoard_Back", root, (-0.4, peg_y, 2.38), (4.20, 0.08, 1.62), mats["walnut_dark"], col=col)
    add_box("ToolBoard_Frame", root, (-0.4, peg_y + 0.03, 2.38), (4.00, 0.04, 1.44), mats["slate_floor"], col=col)
    add_pegboard_tools("ToolBoard_Tools", root, peg_y + 0.05, 2.38, mats, col=col)
    add_gear_wheel("Wall_Gear_1", root, (0.95, peg_y + 0.08, 2.52), 0.42, 0.08, 8, mats["brass_gold"], mats["walnut_dark"], rot=(math.pi * 0.5, 0, 0), col=col)
    add_gear_wheel("Wall_Gear_2", root, (1.52, peg_y + 0.08, 2.15), 0.28, 0.08, 6, mats["inox"], mats["teal_wainscot"], rot=(math.pi * 0.5, 0, 0.3), col=col)

    add_box("SideToolBench", root, (-0.4, -hy + 0.72, 0.24 + 0.40), (3.80, 0.78, 0.80), mats["walnut_dark"], col=col)
    add_box("SideToolBenchTop", root, (-0.4, -hy + 0.72, 1.08), (3.92, 0.86, 0.08), mats["honey_wood"], col=col)
    # Detailed Craft Bench Clutter (flush at SideToolBenchTop Z=1.120 & floor Z=0.253)
    bp_sheet = add_box("Craft_BlueprintSheet", root, (-1.2, -hy + 0.70, 1.126), (0.85, 0.56, 0.012), mats["blueprint_blue"], rot=(0, 0, 0.08), col=col)
    add_box("Craft_BP_LineT", bp_sheet, (0, 0.22, 0.007), (0.74, 0.014, 0.004), mats["pure_white"], col=col)
    add_box("Craft_BP_LineB", bp_sheet, (0, -0.22, 0.007), (0.74, 0.014, 0.004), mats["pure_white"], col=col)
    add_box("Craft_BP_LineL", bp_sheet, (-0.36, 0, 0.007), (0.014, 0.44, 0.004), mats["pure_white"], col=col)
    add_box("Craft_BP_LineR", bp_sheet, (0.36, 0, 0.007), (0.014, 0.44, 0.004), mats["pure_white"], col=col)
    add_box("Craft_BP_Box", bp_sheet, (-0.12, 0.02, 0.007), (0.28, 0.24, 0.004), mats["pure_white"], col=col)
    add_box("Craft_BP_Ruler", bp_sheet, (0.20, -0.06, 0.010), (0.042, 0.34, 0.008), mats["brass_gold"], rot=(0, 0, -0.25), col=col)

    add_open_ledger_and_quill("Craft_OpenLedger", root, (0.08, -hy + 0.72, 1.120), mats, rot_z=(math.pi * 0.5 - 0.14), col=col)
    add_oil_lamp("Craft_OilLamp", root, (1.08, -hy + 0.72, 1.120), mats, rot_z=0.35, col=col)
    add_wooden_crate("Craft_UnderCrate", root, (1.94, -hy + 0.74, 0.253 + 0.23), (0.54, 0.54, 0.46), mats, col=col)
    add_workshop_stool("Craft_Stool", root, (0.80, -hy + 1.48, 0.253), mats, height=0.52, rot_z=0.22, col=col)

    # 9. Packed 4-Tier Back West Shelf (X=-3.85, Y=-2.6..+2.6, shelf tops Z=0.7525, 1.4325, 2.1125, 2.7925)
    add_box("AlchemyRack_Frame", root, (-4.05, 0.0, 0.24 + 1.48), (0.48, 5.20, 2.96), mats["walnut_dark"], col=col)
    for sz in (0.72, 1.40, 2.08, 2.76):
        add_box(f"Alchemy_Shelf_{sz:.2f}", root, (-3.85, 0.0, sz), (0.68, 4.96, 0.065), mats["honey_wood"], col=col)
    for sy_div in (-2.46, -0.82, 0.82, 2.46):
        add_box(f"Alchemy_Div_{sy_div:.2f}", root, (-3.85, sy_div, 1.74), (0.64, 0.06, 2.86), mats["wood_dark"], col=col)
    add_box("Alchemy_TopCrown", root, (-3.82, 0.0, 3.24), (0.74, 5.32, 0.14), mats["brass_gold"], col=col)

    # Keep Shelf_Books1..4 and Shelf_MiniChestA/B/C intact; upgrade decorative bottles, scrolls, vases, armillary & lantern!
    _add_book_cluster("Shelf_Books1", root, (-3.76, -1.95, 2.1125), 4, mats, seed=1, col=col)
    _add_book_cluster("Shelf_Books2", root, (-3.76, -0.25, 2.1125), 3, mats, seed=4, col=col)
    _add_book_cluster("Shelf_Books3", root, (-3.76, 1.45, 1.4325), 4, mats, seed=7, col=col)
    _add_book_cluster("Shelf_Books4", root, (-3.76, -1.35, 1.4325), 3, mats, seed=2, col=col)

    for bi, (by, bm) in enumerate(((-1.92, "blueprint_blue"), (-1.62, "parchment"), (1.66, "blueprint_blue"))):
        add_scroll_bundle(f"Shelf_Scroll_{bi}", root, (-3.67, by, 2.7925), 0.42, 0.056, mats[bm], mats, rot_z=(math.pi * 0.5 + 0.08 * (bi - 1)), col=col)

    flask_specs = (
        (-0.48, 1.4325, "amber_glass", "brass_gold", 0.28, 0.072),
        (-0.15, 1.4325, "jade_glass", "honey_wood", 0.32, 0.078),
        (0.20, 1.4325, "amber_glass", "copper", 0.26, 0.068),
        (-2.05, 0.7525, "jade_glass", "brass_gold", 0.32, 0.078),
        (0.45, 2.1125, "amber_glass", "brass_gold", 0.30, 0.074),
        (2.08, 2.1125, "jade_glass", "honey_wood", 0.28, 0.072),
    )
    for fi, (fy, fz, fm, sm, fh, fr) in enumerate(flask_specs):
        add_apothecary_bottle(f"AlchemyFlask_{fi}", root, (-3.67, fy, fz), fh, fr, mats[fm], mats, stopper_mat=mats[sm], col=col)

    # Keep miniature sealed chests untouched; replace crude cylinders/icosphere/hanging-cord lantern with sculpted grounded props
    add_wooden_crate("Shelf_MiniChestA", root, (-3.76, 1.35, 0.94), (0.42, 0.52, 0.36), mats, trim_mat=mats["brass_gold"], col=col)
    add_wooden_crate("Shelf_MiniChestB", root, (-3.76, -1.25, 2.98), (0.38, 0.44, 0.32), mats, trim_mat=mats["box_purple"], col=col)
    add_wooden_crate("Shelf_MiniChestC", root, (-3.76, -0.35, 0.94), (0.40, 0.48, 0.34), mats, trim_mat=mats["box_teal"], col=col)
    add_sculpted_vase("Shelf_VaseA", root, (-3.67, 2.02, 1.4325), 0.36, 0.10, mats["ceramic_sage"], mats, with_handles=True, col=col)
    add_sculpted_vase("Shelf_VaseB", root, (-3.67, 0.18, 2.7925), 0.34, 0.092, mats["terracotta_roof"], mats, with_handles=True, col=col)
    add_armillary_globe("Shelf_Armillary", root, (-3.64, 1.48, 2.1125), mats, scale=0.85, rot_z=0.18, col=col)
    add_table_lantern("Shelf_TableLantern", root, (-3.66, -0.42, 2.7925), mats, rot_z=0.22, scale=0.92, col=col)

    # 10. North Wall (+Y): Vintage Appraisal Noticeboard (lined notes) + Wall Clock + Flush Bracket Sconce + Cargo
    add_box("Notice_Frame", root, (-2.65, hy - 0.34, 2.18), (1.90, 0.08, 1.25), mats["honey_wood"], col=col)
    add_box("Notice_Slate", root, (-2.65, hy - 0.37, 2.18), (1.70, 0.04, 1.05), mats["slate_floor"], col=col)
    for ni, (nx, nz, nrot) in enumerate(((-3.12, 2.38, 0.08), (-2.55, 2.30, -0.06), (-2.08, 2.42, 0.05), (-2.80, 1.92, -0.10))):
        note_obj = add_box(f"Notice_Note_{ni}", root, (nx, hy - 0.40, nz), (0.34, 0.02, 0.38), mats["parchment"], rot=(0, nrot, 0), col=col)
        add_cyl(f"Notice_Pin_{ni}", note_obj, (0.0, -0.014, 0.13), 0.03, 0.03, 0.018, mats["wax_red"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)
        for li, (lw, lz) in enumerate(((0.22, 0.05), (0.18, -0.01), (0.24, -0.07))):
            add_box(f"Notice_Line_{ni}_{li}", note_obj, (0.0, -0.011, lz), (lw, 0.005, 0.018), mats["walnut_dark"], col=col)

    add_wooden_crate("North_CrateA", root, (-2.75, hy - 0.85, 0.56), (0.64, 0.64, 0.64), mats, trim_mat=mats["teal_wainscot"], col=col)
    add_wooden_crate("North_CrateB", root, (-1.92, hy - 0.82, 0.50), (0.52, 0.52, 0.52), mats, rot=(0, 0, 0.24), col=col)
    add_faceted_barrel("North_Barrel", root, (-2.75, hy - 0.85, 0.88), 0.24, 0.58, mats, col=col)
    add_faceted_barrel("North_CornerBarrel", root, (-3.65, hy - 0.78, 0.24), 0.28, 0.74, mats, col=col)
    add_bracket_lantern("North_WallSconce", root, (1.85, hy - 0.295, 2.86), (0.0, -1.0), mats, col=col)
    add_wall_clock("WallClock_TMH", root, (2.95, hy - 0.295, 2.38), (0.0, -1.0), mats, col=col)

    # 11. HERO CENTERPIECE: MASTER ANTIQUE UNBOXING WORKBENCH (X=-0.30, Y=0.0, top Z=1.04m)
    tx, ty = -0.30, 0.0
    add_box("Table_Deck", root, (tx, ty, 0.99), (1.52, 3.30, 0.10), mats["walnut_dark"], col=col)
    add_box("Table_TrimEdge", root, (tx, ty, 0.99), (1.56, 3.34, 0.04), mats["brass_gold"], col=col)
    add_box("Table_Apron", root, (tx, ty, 0.86), (1.30, 3.04, 0.16), mats["wood_dark"], col=col)
    add_box("Table_LowerShelf", root, (tx, ty, 0.44), (1.16, 2.76, 0.06), mats["honey_wood"], col=col)
    for lx in (tx - 0.60, tx + 0.60):
        for ly in (-1.42, 1.42):
            add_box(f"Table_Leg_{lx:.2f}_{ly:.2f}", root, (lx, ly, 0.24 + 0.36), (0.14, 0.14, 0.72), mats["wood_dark"], col=col)
            add_box(f"Table_BrassFoot_{lx:.2f}_{ly:.2f}", root, (lx, ly, 0.30), (0.18, 0.18, 0.10), mats["brass_gold"], col=col)
    # Under-table spare cargo & tied burlap grain sack flush on Table_LowerShelf (top Z=0.470, under apron 0.780)
    add_wooden_crate("Table_UnderCrateA", root, (tx, -0.78, 0.68), (0.48, 0.56, 0.42), mats, rot=(0, 0, 0.12), col=col)
    add_wooden_crate("Table_UnderCrateB", root, (tx + 0.10, 0.75, 0.65), (0.44, 0.44, 0.36), mats, trim_mat=mats["brass_gold"], rot=(0, 0, -0.15), col=col)
    add_burlap_sack("Table_UnderSack", root, (tx - 0.08, 0.05, 0.470), mats, scale=0.72, rot_z=0.35, col=col)

    add_gear_wheel("Workbench_SideGear_S", root, (tx, -1.58, 0.72), 0.30, 0.08, 8, mats["brass_gold"], mats["charcoal"], rot=(math.pi * 0.5, 0, 0), col=col)
    add_gear_wheel("Workbench_SideGear_N", root, (tx, 1.58, 0.72), 0.30, 0.08, 8, mats["brass_gold"], mats["charcoal"], rot=(math.pi * 0.5, 0, 0), col=col)

    # Crimson Velvet Mat + 3 Raised Brass Tray Bezels at exact slot positions (-0.95, 0.0, +0.95)
    add_box("Table_VelvetMat", root, (tx, ty, 1.050), (1.06, 2.86, 0.02), mats["crimson_velvet"], col=col)
    for si, slot_y in enumerate((-0.95, 0.0, 0.95)):
        add_box(f"SlotPad_Rim_{si}", root, (tx, slot_y, 1.062), (0.68, 0.68, 0.014), mats["brass_gold"], col=col)
        add_box(f"SlotPad_Center_{si}", root, (tx, slot_y, 1.066), (0.58, 0.58, 0.014), mats["crimson_velvet"], col=col)

    # Workbench West-Rim Decorative Tools (clear of the 3 center BoxSlots!)
    add_workbench_loupe("BenchLoupe", root, (tx - 0.62, 1.38, 1.040), mats, rot_z=-0.25, col=col)
    add_banker_lamp("BenchBanker", root, (tx - 0.58, -1.36, 1.040), mats, rot_z=0.15, col=col)

    # Sculpted Wood & Brass Craftsman Mallet flush at Table_Deck Z=1.040
    mal_grp = bpy.data.objects.new("Tool_Mallet", None)
    mal_grp.location = Vector((tx - 0.64, 0.34, 1.040))
    mal_grp.rotation_euler = Euler((0, 0, 0.16))
    mal_grp.parent = root
    link_obj(mal_grp, col)
    add_cyl("Tool_MalletHead", mal_grp, (0.0, 0.10, 0.038), 0.042, 0.042, 0.135, mats["walnut_dark"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    for esgn, etag in ((-1.0, "L"), (1.0, "R")):
        add_cyl(f"Tool_MalletCap_{etag}", mal_grp, (esgn * 0.072, 0.10, 0.038), 0.044, 0.040, 0.014, mats["brass_gold"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_limb("Tool_MalletHandle", mal_grp, (0.0, 0.10, 0.038), (0.0, -0.14, 0.014), 0.016, 0.013, mats["honey_wood"], segments=6, col=col)

    # Compact Open Field Notebook + Fountain Pen flush at Table_Deck Z=1.040 (clear of velvet mat at tx-0.53!)
    nb = bpy.data.objects.new("Table_Notebook", None)
    nb.location = Vector((tx - 0.65, -0.52, 1.040))
    nb.rotation_euler = Euler((0, 0, -0.08))
    nb.parent = root
    link_obj(nb, col)
    add_cyl("Table_NBSpine", nb, (0, 0, 0.008), 0.012, 0.012, 0.28, mats["wood_dark"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)
    for sgn, ptag in ((-1.0, "L"), (1.0, "R")):
        add_box(f"Table_NBCover_{ptag}", nb, (sgn * 0.046, 0, 0.005), (0.090, 0.275, 0.010), mats["wood_dark"], rot=(0, sgn * 0.06, 0), col=col)
        add_box(f"Table_NBPages_{ptag}", nb, (sgn * 0.044, 0, 0.016), (0.082, 0.255, 0.014), mats["pure_white"], rot=(0, sgn * 0.06, 0), col=col)
        for li, ly_l in enumerate((-0.075, -0.025, 0.025, 0.075)):
            add_box(f"Table_NBLine_{ptag}_{li}", nb, (sgn * 0.044, ly_l, 0.024), (0.062, 0.010, 0.003), mats["walnut_dark"], rot=(0, sgn * 0.06, 0), col=col)
    add_box("Table_NBRibbon", nb, (0.02, -0.135, 0.010), (0.014, 0.06, 0.004), mats["wax_red"], rot=(0, 0, 0.22), col=col)
    add_limb("Table_BenchPen", nb, (0.03, 0.02, 0.027), (0.08, 0.13, 0.027), 0.007, 0.003, mats["brass_gold"], segments=6, col=col)

    add_brass_scale("TableScale", root, (tx - 0.58, 0.92, 1.040), mats, rot_z=0.0, col=col)
    return root


# ==================== 3. ANH BẢY (CORNER HIP ROOF) & 4. KIM KHUY (2-TIER STEPPED ROOF) ====================

def _add_security_window_bars(prefix, parent, center, axis, win_w, win_h, offset_out, mats, col=None):
    """Heavy wrought-iron prison/syndicate window bars fitted neatly inside cut-through window openings."""
    cx, cy, cz = center
    b_h = win_h - 0.10
    if axis == "x":
        bx = cx + offset_out
        for bi, t in enumerate((-0.30, -0.10, 0.10, 0.30)):
            add_cyl(f"{prefix}_VBar_{bi}", parent, (bx, cy + win_w * t, cz), 0.020, 0.020, b_h, mats["charcoal"], verts=6, col=col)
        add_box(f"{prefix}_HBar", parent, (bx, cy, cz), (0.036, win_w - 0.08, 0.036), mats["charcoal"], col=col)
    else:
        by = cy + offset_out
        for bi, t in enumerate((-0.30, -0.10, 0.10, 0.30)):
            add_cyl(f"{prefix}_VBar_{bi}", parent, (cx + win_w * t, by, cz), 0.020, 0.020, b_h, mats["charcoal"], verts=6, col=col)
        add_box(f"{prefix}_HBar", parent, (cx, by, cz), (win_w - 0.08, 0.036, 0.036), mats["charcoal"], col=col)


def _add_syndicate_red_lantern(name, parent, top_loc, mats, col=None):
    """Hanging blood-crimson underworld lantern suspended downward from top_loc."""
    lx, ly, lz = top_loc
    add_limb(f"{name}_Cord", parent, (lx, ly, lz), (lx, ly, lz - 0.18), 0.010, 0.010, mats["charcoal"], segments=4, col=col)
    add_cyl(f"{name}_CapTop", parent, (lx, ly, lz - 0.21), 0.13, 0.06, 0.07, mats["brass_gold"], verts=8, col=col)
    add_cyl(f"{name}_Body", parent, (lx, ly, lz - 0.34), 0.15, 0.15, 0.20, mats["lantern_red"], verts=8, col=col)
    add_cyl(f"{name}_CapBot", parent, (lx, ly, lz - 0.46), 0.06, 0.12, 0.06, mats["brass_gold"], verts=8, col=col)
    add_cyl(f"{name}_Tassel", parent, (lx, ly, lz - 0.54), 0.024, 0.008, 0.11, mats["wax_red"], verts=6, col=col)


def build_anh_bay(mats, font, col):
    """Shop #3: Anh Bảy Gangster Syndicate / Underworld Loan-Shark Den (Sào Huyệt Xã Hội Đen).
    Moody dark slate/oxblood-crimson exterior, iron-barred sunlit windows, black-crimson awnings, crimson lanterns,
    gangster emblem crest, mahogany plank floor, crimson runner, executive boss desk (open cash briefcase, gold bullion,
    whiskey & cigar set, ledger & banker lamp), boss throne chair, reinforced 6-spoke steel vault door, poker gambling
    table, wall weapon rack (bat/machete/crowbar), floor safe, and red-string debtor target board.
    Preserves PawnCounter center (X=1.90, Y=0.0) & rear space (X=2.96) for NPC_AnhBay + AnhBay_InteractCounter.
    """
    depth_x, width_y, f1_h, f2_h = 9.0, 10.0, 3.45, 2.65
    hx, hy = depth_x * 0.5, width_y * 0.5
    door_y, door_w, door_h = 1.4, 2.6, 2.80
    belt_z = 0.24 + f1_h
    eave_z = belt_z + f2_h
    ridge_h = 2.35
    total_h = f1_h + f2_h
    txt_rot = (math.pi * 0.5, 0.0, -math.pi * 0.5)

    root = bpy.data.objects.new("LP_Shop_AnhBay", None)
    link_obj(root, col)

    # 0. Stone Plinth, Dark Walnut-Mahogany Floor Planks (top Z=0.253), Crimson-Gold Runner & Sunbeams
    add_box("BasePlinth", root, (0.0, 0.0, 0.12), (depth_x, width_y, 0.24), mats["charcoal"], col=col)
    add_box("FloorBase", root, (0.0, 0.0, 0.236), (depth_x - 0.30, width_y - 0.30, 0.016), mats["walnut_dark"], col=col)
    for row_i in range(14):
        py = -4.55 + row_i * 0.70
        for col_i in range(5):
            px_raw = -3.45 + col_i * 1.72 + (0.35 if row_i % 2 == 1 else -0.15)
            x0 = max(-hx + 0.24, px_raw - 0.82)
            x1 = min(hx - 0.24, px_raw + 0.82)
            if x1 - x0 < 0.45:
                continue
            p_mat = mats["wood_dark"] if (row_i + col_i) % 3 != 0 else mats["walnut_dark"]
            add_box(f"OakPlank_{row_i}_{col_i}", root, ((x0 + x1) * 0.5, py, 0.246), (x1 - x0, 0.66, 0.014), p_mat, col=col)

    add_box("Pawn_RugTrim", root, (-1.10, 0.70, 0.254), (4.70, 2.40, 0.008), mats["brass_gold"], col=col)
    add_box("Pawn_RugBody", root, (-1.10, 0.70, 0.258), (4.46, 2.16, 0.008), mats["crimson_velvet"], col=col)
    for si, (sbx, sby) in enumerate(((-2.8, 1.3), (-2.9, -2.2), (-1.85, -3.9), (1.85, -3.9))):
        add_box(f"Sunbeam_{si}", root, (sbx, sby, 0.264), (1.35, 0.76, 0.003), mats["sunbeam_floor"], rot=(0, 0, -0.32), col=col)

    # 1. East Back Wall (+X) — Dark Slate Outer, Walnut Wainscot & High-Contrast Stone Upper Plaster (inner face X=4.205)
    back_x = hx - 0.14
    add_box("Wall_Back_F1", root, (back_x, 0.0, 0.24 + f1_h * 0.5), (0.24, width_y - 0.20, f1_h), mats["slate_floor"], col=col)
    add_box("Wall_Back_F2", root, (back_x, 0.0, belt_z + f2_h * 0.5), (0.24, width_y - 0.20, f2_h), mats["crimson_dark"], col=col)
    add_box("Int_Wain_Back", root, (back_x - 0.14, 0.0, 0.24 + 0.52), (0.05, width_y - 0.46, 1.04), mats["walnut_dark"], col=col)
    add_box("Int_Chair_Back", root, (back_x - 0.17, 0.0, 1.28), (0.08, width_y - 0.46, 0.09), mats["charcoal"], col=col)
    add_box("Int_Upper_Back", root, (back_x - 0.13, 0.0, 2.505), (0.05, width_y - 0.46, 2.37), mats["curb_stone"], col=col)

    # 2. West Front Street Wall (-X) WITH Doorway (Y=0.10..2.70) + Iron-Barred Window Win_WestS (Y=-2.35, Z=1.945)
    front_x = -hx + 0.14
    for p_name, y0, y1 in (("S", -hy + 0.10, -3.45), ("Mid", -1.25, 0.10), ("N", 2.70, hy - 0.10)):
        pw = y1 - y0
        yc = (y0 + y1) * 0.5
        add_box(f"FrontPier_{p_name}", root, (front_x, yc, 0.24 + f1_h * 0.5), (0.26, pw, f1_h), mats["slate_floor"], col=col)
        add_box(f"Int_Wain_W_{p_name}", root, (front_x + 0.14, yc, 0.24 + 0.52), (0.05, pw, 1.04), mats["walnut_dark"], col=col)
        add_box(f"Int_Chair_W_{p_name}", root, (front_x + 0.17, yc, 1.28), (0.08, pw, 0.09), mats["charcoal"], col=col)
        add_box(f"Int_Upper_W_{p_name}", root, (front_x + 0.13, yc, 2.505), (0.05, pw, 2.37), mats["curb_stone"], col=col)

    add_box("Wall_West_Bot", root, (front_x, -2.35, 0.695), (0.26, 2.20, 0.91), mats["slate_floor"], col=col)
    add_box("Int_Wain_W_Bot", root, (front_x + 0.14, -2.35, 0.695), (0.05, 2.20, 0.91), mats["walnut_dark"], col=col)
    add_box("Wall_West_Top", root, (front_x, -2.35, 3.215), (0.26, 2.20, 0.95), mats["slate_floor"], col=col)
    add_box("Int_Upper_W_Top", root, (front_x + 0.13, -2.35, 3.215), (0.05, 2.20, 0.95), mats["curb_stone"], col=col)
    _add_cut_window("Win_WestS", root, (front_x, -2.35, 1.945), "x", 2.20, 1.55, 0.28, mats, cols=3, rows=2, frame_mat=mats["charcoal"], col=col)
    _add_security_window_bars("Bars_WestS", root, (front_x, -2.35, 1.945), "x", 2.20, 1.55, -0.10, mats, col=col)

    add_box("FrontLintel_Door", root, (front_x, door_y, 0.24 + door_h + (f1_h - door_h) * 0.5), (0.26, door_w + 0.10, f1_h - door_h), mats["slate_floor"], col=col)
    add_box("Int_Upper_DoorLintel", root, (front_x + 0.13, door_y, 0.24 + door_h + (f1_h - door_h) * 0.5), (0.05, door_w + 0.04, f1_h - door_h), mats["curb_stone"], col=col)
    add_box("DoorFrame_L", root, (-hx, door_y - door_w * 0.5 - 0.07, 0.24 + door_h * 0.5), (0.36, 0.18, door_h), mats["charcoal"], col=col)
    add_box("DoorFrame_R", root, (-hx, door_y + door_w * 0.5 + 0.07, 0.24 + door_h * 0.5), (0.36, 0.18, door_h), mats["charcoal"], col=col)
    add_box("DoorFrame_Top", root, (-hx, door_y, 0.24 + door_h + 0.09), (0.40, door_w + 0.36, 0.22), mats["charcoal"], col=col)
    for dt in (-1.20, 1.20):
        add_box(f"DoorTrimBrass_{dt:.1f}", root, (-hx - 0.19, door_y + dt, 1.65), (0.02, 0.08, 2.50), mats["brass_gold"], col=col)

    # 3. South Corner Street Wall (-Y) WITH 2 Iron-Barred 6-Pane Windows (sill top Z=1.255)
    wy_s = -hy + 0.14
    for p_name, x0, x1 in (("W", -hx + 0.10, -2.80), ("Mid", -0.90, 0.90), ("E", 2.80, hx - 0.10)):
        pw = x1 - x0
        xc = (x0 + x1) * 0.5
        add_box(f"Wall_South_{p_name}", root, (xc, wy_s, 0.24 + f1_h * 0.5), (pw, 0.24, f1_h), mats["slate_floor"], col=col)
        add_box(f"Int_Wain_S_{p_name}", root, (xc, wy_s + 0.14, 0.24 + 0.52), (pw, 0.05, 1.04), mats["walnut_dark"], col=col)
        add_box(f"Int_Chair_S_{p_name}", root, (xc, wy_s + 0.17, 1.28), (pw, 0.08, 0.09), mats["charcoal"], col=col)
        add_box(f"Int_Upper_S_{p_name}", root, (xc, wy_s + 0.13, 2.505), (pw, 0.05, 2.37), mats["curb_stone"], col=col)
    for w_name, wx_c in (("Win_SouthW", -1.85), ("Win_SouthE", 1.85)):
        ww = 1.90
        add_box(f"Wall_South_Bot_{w_name}", root, (wx_c, wy_s, 0.715), (ww, 0.24, 0.95), mats["slate_floor"], col=col)
        add_box(f"Int_Wain_S_Bot_{w_name}", root, (wx_c, wy_s + 0.14, 0.715), (ww, 0.05, 0.95), mats["walnut_dark"], col=col)
        add_box(f"Wall_South_Top_{w_name}", root, (wx_c, wy_s, 3.1925), (ww, 0.24, 0.995), mats["slate_floor"], col=col)
        add_box(f"Int_Upper_S_Top_{w_name}", root, (wx_c, wy_s + 0.13, 3.1925), (ww, 0.05, 0.995), mats["curb_stone"], col=col)
        _add_cut_window(w_name, root, (wx_c, wy_s, 1.945), "y", ww, 1.50, 0.26, mats, cols=3, rows=2, frame_mat=mats["charcoal"], col=col)
        _add_security_window_bars(f"Bars_{w_name}", root, (wx_c, wy_s, 1.945), "y", ww, 1.50, -0.10, mats, col=col)

    # 4. North Wall (+Y) WITH Iron-Barred Window Win_NorthMid at X=-0.80 (span -1.80..+0.20, sill top Z=1.255)
    wy_n = hy - 0.14
    for seg_name, x0, x1 in (("NW", -hx + 0.10, -1.80), ("NE", 0.20, hx - 0.10)):
        sw = x1 - x0
        xc = (x0 + x1) * 0.5
        add_box(f"Wall_North_F1_{seg_name}", root, (xc, wy_n, 0.24 + f1_h * 0.5), (sw, 0.24, f1_h), mats["slate_floor"], col=col)
        add_box(f"Int_Wain_N_{seg_name}", root, (xc, wy_n - 0.14, 0.24 + 0.52), (sw, 0.05, 1.04), mats["walnut_dark"], col=col)
        add_box(f"Int_Chair_N_{seg_name}", root, (xc, wy_n - 0.17, 1.28), (sw, 0.08, 0.09), mats["charcoal"], col=col)
        add_box(f"Int_Upper_N_{seg_name}", root, (xc, wy_n - 0.13, 2.505), (sw, 0.05, 2.37), mats["curb_stone"], col=col)
    add_box("Wall_North_WinBot", root, (-0.80, wy_n, 0.715), (2.00, 0.24, 0.95), mats["slate_floor"], col=col)
    add_box("Int_Wain_N_WinBot", root, (-0.80, wy_n - 0.14, 0.715), (2.00, 0.05, 0.95), mats["walnut_dark"], col=col)
    add_box("Wall_North_WinTop", root, (-0.80, wy_n, 3.1925), (2.00, 0.24, 0.995), mats["slate_floor"], col=col)
    add_box("Int_Upper_N_WinTop", root, (-0.80, wy_n - 0.13, 3.1925), (2.00, 0.05, 0.995), mats["curb_stone"], col=col)
    _add_cut_window("Win_NorthMid", root, (-0.80, wy_n, 1.945), "y", 2.00, 1.50, 0.26, mats, cols=3, rows=2, frame_mat=mats["charcoal"], sill_depth=0.34, col=col)
    _add_security_window_bars("Bars_NorthMid", root, (-0.80, wy_n, 1.945), "y", 2.00, 1.50, 0.10, mats, col=col)

    # 5. Upper Oxblood Crimson F2 Story, Charcoal Belt Trim & Corner Quoins + 4-Sided Hip Roof
    add_box("Wall_West_F2", root, (front_x, 0.0, belt_z + f2_h * 0.5), (0.26, width_y - 0.20, f2_h), mats["crimson_dark"], col=col)
    add_box("Wall_South_F2", root, (0.0, wy_s, belt_z + f2_h * 0.5), (depth_x - 0.20, 0.24, f2_h), mats["crimson_dark"], col=col)
    add_box("Wall_North_F2", root, (0.0, wy_n, belt_z + f2_h * 0.5), (depth_x - 0.20, 0.24, f2_h), mats["crimson_dark"], col=col)
    add_box("BeltTrim_W", root, (-hx - 0.06, 0.0, belt_z), (0.20, width_y + 0.16, 0.16), mats["charcoal"], col=col)
    add_box("BeltTrim_E", root, (hx + 0.06, 0.0, belt_z), (0.20, width_y + 0.16, 0.16), mats["charcoal"], col=col)
    add_box("BeltTrim_S", root, (0.0, -hy - 0.06, belt_z), (depth_x + 0.16, 0.20, 0.16), mats["charcoal"], col=col)
    add_box("BeltTrim_N", root, (0.0, hy + 0.06, belt_z), (depth_x + 0.16, 0.20, 0.16), mats["charcoal"], col=col)
    for cx in (-hx + 0.08, hx - 0.08):
        for cy in (-hy + 0.08, hy - 0.08):
            add_box(f"CornerPost_{cx:.1f}_{cy:.1f}", root, (cx, cy, 0.24 + total_h * 0.5), (0.24, 0.24, total_h + 0.06), mats["charcoal"], col=col)

    for wy in (-3.0, 3.45):
        add_window(f"F2_WestWin_{wy:.1f}", root, (-hx - 0.04, wy, belt_z + 1.32), "x", 1.25, 1.38, mats, with_shutters=True, frame_mat=mats["charcoal"], col=col)
    for sx in (-1.6, 1.6):
        add_window(f"F2_SouthWin_{sx:.1f}", root, (sx, -hy - 0.04, belt_z + 1.32), "y", 1.25, 1.38, mats, with_shutters=True, frame_mat=mats["charcoal"], col=col)

    add_hip_terracotta_roof("Roof", root, eave_z, hx, hy, ridge_h, mats, col=col)
    for rti in range(13):
        rty = -4.6 + rti * (9.2 / 12.0)
        add_box(f"Ext_RafterTailW_{rti}", root, (-hx - 0.24, rty, eave_z - 0.14), (0.54, 0.11, 0.14), mats["charcoal"], rot=(0, -0.32, 0), col=col)
    for rti in range(11):
        rtx = -4.0 + rti * (8.0 / 10.0)
        add_box(f"Ext_RafterTailS_{rti}", root, (rtx, -hy - 0.24, eave_z - 0.14), (0.11, 0.54, 0.14), mats["charcoal"], rot=(0.32, 0, 0), col=col)

    # 6. Exterior Porch, Black-Crimson Syndicate Awnings, Red Triad Lanterns, Crest Emblem & Signboard
    add_box("Porch_SubBase", root, (-5.06, door_y, 0.14), (1.14, 3.76, 0.12), mats["charcoal"], col=col)
    for pi in range(5):
        px_d = -4.61 - pi * 0.22
        add_box(f"Porch_DeckPlank_{pi}", root, (px_d, door_y, 0.22), (0.20, 3.80, 0.04), mats["walnut_dark"] if pi % 2 == 0 else mats["wood_dark"], col=col)
    add_box("Porch_StairUpper", root, (-5.84, door_y, 0.12), (0.38, 3.12, 0.08), mats["walnut_dark"], col=col)
    add_box("Porch_StairLower", root, (-6.22, door_y, 0.08), (0.38, 3.12, 0.04), mats["wood_dark"], col=col)
    add_box("Porch_StairStringL", root, (-6.02, door_y - 1.58, 0.12), (0.82, 0.09, 0.16), mats["charcoal"], rot=(0, -0.20, 0), col=col)
    add_box("Porch_StairStringR", root, (-6.02, door_y + 1.58, 0.12), (0.82, 0.09, 0.16), mats["charcoal"], rot=(0, -0.20, 0), col=col)

    for r_sgn, ry_rail in (("L", door_y - 1.62), ("R", door_y + 1.62)):
        add_box(f"Rail_TopPost_{r_sgn}", root, (-5.58, ry_rail, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_box(f"Rail_BotPost_{r_sgn}", root, (-6.40, ry_rail, 0.50), (0.09, 0.09, 0.84), mats["charcoal"], col=col)
        ry_ret = (door_y - 1.86) if r_sgn == "L" else (door_y + 1.86)
        add_box(f"Rail_WallPost_{r_sgn}", root, (-4.62, ry_ret, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_box(f"Rail_CornerPost_{r_sgn}", root, (-5.58, ry_ret, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_limb(f"Rail_SlopedBar_{r_sgn}", root, (-5.58, ry_rail, 1.12), (-6.40, ry_rail, 0.90), 0.036, 0.036, mats["charcoal"], segments=6, col=col)
        add_limb(f"Rail_MidBar_{r_sgn}", root, (-5.58, ry_rail, 0.72), (-6.40, ry_rail, 0.52), 0.022, 0.022, mats["charcoal"], segments=6, col=col)
        add_limb(f"Rail_ReturnBar_{r_sgn}", root, (-4.62, ry_ret, 1.12), (-5.58, ry_ret, 1.12), 0.032, 0.032, mats["charcoal"], segments=6, col=col)
        add_limb(f"Rail_FrontReturn_{r_sgn}", root, (-5.58, ry_ret, 1.12), (-5.58, ry_rail, 1.12), 0.032, 0.032, mats["charcoal"], segments=6, col=col)
        for cap_y, cap_z in ((ry_rail, 1.155), (ry_ret, 1.155)):
            add_box(f"Rail_Cap_{r_sgn}_{cap_y:.2f}", root, (-5.58, cap_y, cap_z), (0.12, 0.12, 0.035), mats["brass_gold"], col=col)
        add_box(f"Rail_CapBot_{r_sgn}", root, (-6.40, ry_rail, 0.935), (0.12, 0.12, 0.035), mats["brass_gold"], col=col)

    add_box("DoorHood_Roof", root, (-hx - 0.42, door_y, 3.28), (0.86, 3.60, 0.12), mats["charcoal"], rot=(0, -0.24, 0), col=col)
    add_box("DoorHood_Trim", root, (-hx - 0.80, door_y, 3.19), (0.10, 3.64, 0.10), mats["brass_gold"], col=col)
    for cby in (door_y - 1.62, door_y + 1.62):
        add_limb(f"DoorHood_Bracket_{cby:.1f}", root, (-hx - 0.04, cby, 2.84), (-hx - 0.68, cby, 3.20), 0.055, 0.055, mats["charcoal"], segments=6, col=col)

    add_box("Sign_Back", root, (-hx - 0.09, door_y, 3.92), (0.16, 3.40, 0.80), mats["crimson_dark"], col=col)
    add_box("Sign_Face", root, (-hx - 0.16, door_y, 3.92), (0.10, 3.14, 0.60), mats["charcoal"], col=col)
    for sy_br, btag in ((door_y - 1.35, "L"), (door_y + 1.35, "R")):
        add_box(f"Sign_Bracket_{btag}", root, (-hx - 0.01, sy_br, 3.92), (0.14, 0.12, 0.84), mats["brass_gold"], col=col)
    add_3d_text_mesh("Sign_Text", root, "ANH BẢY", (-hx - 0.222, door_y, 3.90), txt_rot, 0.34, mats["brass_gold"], font_data=font, col=col)

    # Gangster Syndicate Crest Medallion on F2 West Gable above Signboard (crossed gold blades on crimson/black shield)
    crest_x, crest_z = -hx - 0.06, belt_z + 1.48
    add_cyl("Crest_OuterHex", root, (crest_x, door_y, crest_z), 0.56, 0.56, 0.10, mats["charcoal"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl("Crest_Crimson", root, (crest_x - 0.06, door_y, crest_z), 0.44, 0.44, 0.06, mats["crimson_plank"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_cyl("Crest_Ring", root, (crest_x - 0.09, door_y, crest_z), 0.32, 0.32, 0.04, mats["brass_gold"], verts=8, rot=(0, math.pi * 0.5, 0), col=col)
    add_limb("Crest_SwordA", root, (crest_x - 0.12, door_y - 0.34, crest_z - 0.34), (crest_x - 0.12, door_y + 0.34, crest_z + 0.34), 0.028, 0.014, mats["inox"], segments=4, col=col)
    add_limb("Crest_SwordB", root, (crest_x - 0.12, door_y + 0.34, crest_z - 0.34), (crest_x - 0.12, door_y - 0.34, crest_z + 0.34), 0.028, 0.014, mats["inox"], segments=4, col=col)

    # Black & Crimson Syndicate Awnings + Suspended Crimson Triad Lanterns (clear of Stall_TraDa!)
    add_striped_awning("Awning_W", root, (-hx - 0.56, -2.35, 2.94), 1.15, 2.85, -0.26, mats["charcoal"], mats["crimson_plank"], col=col)
    for by in (-3.45, -1.25):
        add_limb(f"AwningW_Brace_{by:.1f}", root, (-hx - 0.02, by, 2.58), (-hx - 0.72, by, 2.86), 0.028, 0.024, mats["charcoal"], segments=6, col=col)
    add_striped_awning("Awning_S", root, (0.0, -hy - 0.56, 2.94), 1.15, 6.40, -0.26, mats["charcoal"], mats["crimson_plank"], rot_z=math.pi * 0.5, col=col)
    for bx in (-2.75, 0.0, 2.75):
        add_limb(f"AwningS_Brace_{bx:.1f}", root, (bx, -hy - 0.02, 2.58), (bx, -hy - 0.72, 2.86), 0.028, 0.024, mats["charcoal"], segments=6, col=col)

    _add_syndicate_red_lantern("TriadLan_WS", root, (-hx - 0.48, -3.55, 2.80), mats, col=col)
    _add_syndicate_red_lantern("TriadLan_WN", root, (-hx - 0.48, -1.15, 2.80), mats, col=col)
    _add_syndicate_red_lantern("TriadLan_S", root, (0.0, -hy - 0.48, 2.80), mats, col=col)

    for ly_p, ltag in ((door_y - 1.56, "L"), (door_y + 1.56, "R")):
        add_bracket_lantern(f"PorchLan_{ltag}", root, (-hx, ly_p, 2.90), (-1.0, 0.0), mats, col=col)

    add_limb("Flag_Pole", root, (-hx - 0.04, -0.65, 3.25), (-hx - 0.90, -0.65, 3.70), 0.032, 0.026, mats["charcoal"], segments=6, col=col)
    add_box("Flag_ClothA", root, (-hx - 0.68, -0.65, 3.24), (0.48, 0.03, 0.44), mats["crimson_plank"], rot=(0, -0.15, 0), col=col)
    add_box("Flag_Stripe", root, (-hx - 0.68, -0.65, 3.24), (0.14, 0.04, 0.44), mats["charcoal"], rot=(0, -0.15, 0), col=col)

    # Iron Sill Guards under F1 Barred Windows & Contraband NW Porch Cargo
    add_box("SillGuard_WestS", root, (-hx - 0.14, -2.35, 1.12), (0.16, 2.24, 0.08), mats["charcoal"], col=col)
    add_box("SillGuard_SouthW", root, (-1.85, -hy - 0.14, 1.12), (1.94, 0.16, 0.08), mats["charcoal"], col=col)
    add_box("SillGuard_SouthE", root, (1.85, -hy - 0.14, 1.12), (1.94, 0.16, 0.08), mats["charcoal"], col=col)
    add_faceted_barrel("Ext_NW_Barrel", root, (-hx - 0.55, 4.35, 0.08), 0.28, 0.72, mats, col=col)
    add_wooden_crate("Ext_NW_Crate", root, (-hx - 0.58, 3.60, 0.36), (0.56, 0.56, 0.56), mats, trim_mat=mats["crimson_plank"], rot=(0, 0, 0.18), col=col)

    # === COZY NOIR INTERIOR: DARK VAULTED RAFTERS, RED TRIAD LANTERNS, POKER DEN, WEAPONS, BOSS DESK & VAULT ===
    # 7. Vaulted Walnut Timber Rafter Ceiling + Interior Pilasters + 3 Hanging Pendants + 2 Triad Red Lanterns
    ceil_eave_z = 3.56
    ceil_peak_z = 4.30
    ceil_ang = math.atan2(ceil_peak_z - ceil_eave_z, hx - 0.16)
    ceil_slope = math.hypot(hx - 0.16, ceil_peak_z - ceil_eave_z)
    for sgn in (-1.0, 1.0):
        add_box(f"Int_CeilDeck_{sgn}", root, (sgn * hx * 0.50, 0.0, (ceil_eave_z + ceil_peak_z) * 0.5 + 0.06),
                (ceil_slope + 0.18, width_y - 0.44, 0.10), mats["wood_dark"], rot=(0, sgn * ceil_ang, 0), col=col)
    add_box("Int_RidgeBeam", root, (0.0, 0.0, ceil_peak_z - 0.08), (0.24, width_y - 0.44, 0.26), mats["walnut_dark"], col=col)
    for ty in (-4.0, -2.0, 0.0, 2.0, 4.0):
        add_box(f"Ceil_TieBeam_{ty:.1f}", root, (0.0, ty, ceil_eave_z - 0.06), (depth_x - 0.52, 0.20, 0.22), mats["walnut_dark"], col=col)
        add_box(f"Ceil_KingPost_{ty:.1f}", root, (0.0, ty, ceil_eave_z + 0.36), (0.18, 0.18, 0.72), mats["walnut_dark"], col=col)
        for rsgn in (-1.0, 1.0):
            add_box(f"Ceil_Rafter_{ty:.1f}_{rsgn}", root, (rsgn * hx * 0.5, ty, (ceil_eave_z + ceil_peak_z) * 0.5 - 0.04),
                    (ceil_slope, 0.15, 0.18), mats["wood_dark"], rot=(0, rsgn * ceil_ang, 0), col=col)

    for px, py in ((-4.22, -4.68), (-4.22, 4.68), (4.22, -4.68), (4.22, 4.68), (-2.90, -4.68), (2.90, -4.68), (-1.90, 4.68), (0.35, 4.68)):
        add_box(f"Int_TimberPost_{px:.1f}_{py:.1f}", root, (px, py, 0.24 + 1.66), (0.18, 0.18, 3.32), mats["walnut_dark"], col=col)

    for pi, (lx, ly) in enumerate(((-1.40, 1.00), (0.75, -1.00), (0.75, 1.00))):
        z_ceil = (ceil_eave_z + ceil_peak_z) * 0.5 + 0.06 + (hx * 0.50 - abs(lx)) * math.tan(ceil_ang) - 0.045
        add_pendant_lamp(f"Pendant_{pi}", root, (lx, ly, z_ceil), 2.88, mats, shade_mat=(mats["charcoal"] if pi == 0 else mats["brass_gold"]), col=col)

    _add_syndicate_red_lantern("Int_TriadLan_Poker", root, (-1.65, -2.85, 3.56), mats, col=col)
    _add_syndicate_red_lantern("Int_TriadLan_Door", root, (-2.80, 1.40, 3.56), mats, col=col)

    # 8. Flush Noir Interior Window Sill Props (liquor bottles, candles & ledgers on Z=1.230 & 1.255)
    add_candlestick("Sill_West_Candle", root, (front_x + 0.12, -2.90, 1.230), mats, rot_z=0.3, col=col)
    add_apothecary_bottle("Sill_West_Flask", root, (front_x + 0.12, -1.80, 1.230), 0.24, 0.065, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    _add_book_cluster("Sill_SouthW_Books", root, (-1.85, wy_s + 0.14, 1.255), 3, mats, seed=3, rot_z=math.pi * 0.5, col=col)
    add_candlestick("Sill_SouthE_Candle", root, (1.50, wy_s + 0.14, 1.255), mats, rot_z=-0.2, col=col)
    add_apothecary_bottle("Sill_SouthE_Jar", root, (2.25, wy_s + 0.14, 1.255), 0.24, 0.065, mats["jade_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    add_gold_bullion_stack("Sill_North_Gold", root, (-1.25, wy_n - 0.14, 1.255), mats, rot_z=0.1, col=col)
    add_apothecary_bottle("Sill_North_Flask", root, (-0.30, wy_n - 0.14, 1.255), 0.24, 0.065, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)

    # 9. Syndicate Boss Executive Desk (X=1.90, Y=0.0, top Z=1.110) + Boss Throne Chair + Contraband Cash & Gold
    add_box("PawnCounter", root, (1.90, 0.0, 0.253 + 0.39), (1.00, 3.40, 0.78), mats["walnut_dark"], col=col)
    add_box("PawnCounter_Top", root, (1.90, 0.0, 1.07), (1.16, 3.58, 0.08), mats["wood_dark"], col=col)
    add_cyl("PawnCounter_FootRail", root, (1.28, 0.0, 0.35), 0.028, 0.028, 3.30, mats["brass_gold"], verts=8, rot=(math.pi * 0.5, 0, 0), col=col)
    for cy in (-1.10, 0.0, 1.10):
        add_box(f"PawnCounter_Trim_{cy:.1f}", root, (1.39, cy, 0.64), (0.04, 0.88, 0.54), mats["crimson_velvet"], col=col)

    add_boss_throne_chair("Boss_ThroneChair", root, (3.46, 0.62, 0.253), mats, rot_z=(math.pi + 0.22), col=col)

    add_banker_lamp("Counter_BankerLamp", root, (1.82, -1.36, 1.110), mats, rot_z=(math.pi - 0.18), col=col)
    add_whiskey_and_cigar_set("Counter_Whiskey", root, (1.80, -0.78, 1.110), mats, rot_z=math.pi, col=col)
    add_open_ledger_and_quill("Counter_Ledger", root, (1.82, -0.02, 1.110), mats, rot_z=(math.pi + 0.06), col=col)
    add_cash_briefcase("Counter_Briefcase", root, (1.78, 0.78, 1.110), mats, rot_z=(math.pi - 0.14), is_open=True, col=col)
    add_gold_bullion_stack("Counter_Gold", root, (1.78, 1.38, 1.110), mats, rot_z=0.22, col=col)

    # 10. East Back Wall (inner face X=4.205): Reinforced Steel Bank Vault Door + Twin Dark Safe-Deposit Cabinets
    add_bank_vault_door("VaultDoor_Hero", root, (back_x - 0.155, 0.0, 0.253), (-1.0, 0.0), mats, col=col)

    for cy_cab, ctag in ((-3.15, "S"), (3.15, "N")):
        add_box(f"VaultCab_Back_{ctag}", root, (4.15, cy_cab, 0.253 + 1.43), (0.12, 2.08, 2.86), mats["walnut_dark"], col=col)
        for sz in (0.92, 1.62, 2.32):
            add_box(f"VaultCab_Shelf_{ctag}_{sz:.2f}", root, (3.83, cy_cab, sz), (0.62, 1.96, 0.06), mats["wood_dark"], col=col)
        for dy_off, dtag in ((-1.00, "L"), (0.0, "M"), (1.00, "R")):
            add_box(f"VaultCab_Div_{ctag}_{dtag}", root, (3.83, cy_cab + dy_off, 0.253 + 1.40), (0.60, 0.06, 2.80), mats["walnut_dark"], col=col)
        add_box(f"VaultCab_Crown_{ctag}", root, (3.82, cy_cab, 3.16), (0.68, 2.16, 0.11), mats["brass_gold"], col=col)

    add_gold_bullion_stack("CabS_Gold_Bot", root, (3.76, -3.65, 0.950), mats, rot_z=0.1, col=col)
    add_cash_briefcase("CabS_Case_Bot", root, (3.78, -2.65, 0.950), mats, rot_z=math.pi, is_open=False, col=col)
    _add_book_cluster("CabS_Books_Mid", root, (3.76, -3.65, 1.650), 4, mats, seed=1, col=col)
    add_apothecary_bottle("CabS_Liquor_Mid", root, (3.76, -2.65, 1.650), 0.28, 0.072, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    add_scroll_bundle("CabS_Scroll_Top", root, (3.76, -3.65, 2.350), 0.38, 0.052, mats["parchment"], mats, rot_z=(math.pi * 0.5 + 0.08), col=col)
    add_table_lantern("CabS_Lantern_Top", root, (3.76, -2.65, 2.350), mats, rot_z=-0.25, scale=0.90, col=col)

    add_cash_briefcase("CabN_Case_Bot", root, (3.78, 2.65, 0.950), mats, rot_z=math.pi, is_open=False, col=col)
    _add_book_cluster("CabN_Books_Bot", root, (3.76, 3.65, 0.950), 4, mats, seed=5, col=col)
    add_apothecary_bottle("CabN_LiquorA", root, (3.76, 2.52, 1.650), 0.28, 0.070, mats["jade_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    add_apothecary_bottle("CabN_LiquorB", root, (3.76, 2.86, 1.650), 0.24, 0.062, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    add_gold_bullion_stack("CabN_Gold_Mid", root, (3.76, 3.65, 1.650), mats, rot_z=-0.1, col=col)
    add_table_lantern("CabN_Lantern_Top", root, (3.76, 2.65, 2.350), mats, rot_z=0.20, scale=0.90, col=col)
    add_scroll_bundle("CabN_Scroll_Top", root, (3.76, 3.65, 2.350), 0.38, 0.052, mats["blueprint_blue"], mats, rot_z=(math.pi * 0.5 - 0.10), col=col)

    # 11. South-West Gambling Poker Corner + South Wall Weapon Rack & Floor Safe
    add_poker_table("Syndicate_PokerTable", root, (-1.65, -2.85, 0.253), mats, height=0.76, radius=0.62, col=col)
    add_workshop_stool("Poker_Stool_W", root, (-2.55, -2.85, 0.253), mats, height=0.50, rot_z=0.15, col=col)
    add_workshop_stool("Poker_Stool_N", root, (-1.65, -1.96, 0.253), mats, height=0.50, rot_z=-0.35, col=col)
    add_workshop_stool("Poker_Stool_E", root, (-0.75, -2.85, 0.253), mats, height=0.50, rot_z=0.45, col=col)

    add_weapon_rack("Syndicate_WeaponRack", root, (0.0, -hy + 0.295, 2.02), 1.0, mats, col=col)
    add_bracket_lantern("South_WallSconce", root, (0.0, -hy + 0.295, 2.86), (0.0, 1.0), mats, col=col)
    add_floor_safe("Syndicate_FloorSafe", root, (0.0, -4.34, 0.253), mats, rot_z=(math.pi * 0.5), col=col)
    add_gold_bullion_stack("Safe_Gold", root, (-0.11, -4.34, 0.973), mats, rot_z=0.2, col=col)
    add_oil_lamp("Safe_OilLamp", root, (0.15, -4.34, 0.973), mats, rot_z=-0.3, col=col)

    # 12. North Wall: NW Red-String Debtor Target Board + Contraband Cargo; NE Wall Clock, Banner & Liquor Credenza
    wy_wall_n = hy - 0.295
    add_box("DebtBoard_Frame", root, (-3.05, hy - 0.34, 2.18), (1.76, 0.08, 1.16), mats["walnut_dark"], col=col)
    add_box("DebtBoard_Slate", root, (-3.05, hy - 0.37, 2.18), (1.58, 0.04, 0.98), mats["slate_floor"], col=col)
    pin_pts = []
    for ni, (nx, nz, nrot) in enumerate(((-3.48, 2.38, 0.07), (-2.95, 2.30, -0.06), (-2.50, 2.40, 0.05), (-3.15, 1.92, -0.09))):
        note_obj = add_box(f"Debt_Note_{ni}", root, (nx, hy - 0.40, nz), (0.32, 0.02, 0.36), mats["parchment"], rot=(0, nrot, 0), col=col)
        add_cyl(f"Debt_Pin_{ni}", note_obj, (0.0, -0.014, 0.12), 0.028, 0.028, 0.018, mats["wax_red"], verts=6, rot=(math.pi * 0.5, 0, 0), col=col)
        for li, (lw, lz) in enumerate(((0.20, 0.05), (0.17, -0.01), (0.22, -0.07))):
            add_box(f"Debt_Line_{ni}_{li}", note_obj, (0.0, -0.011, lz), (lw, 0.005, 0.016), mats["walnut_dark"], col=col)
        pin_pts.append((nx, hy - 0.415, nz + 0.12))
    for si, (i0, i1) in enumerate(((0, 1), (1, 2), (0, 3), (2, 3))):
        add_limb(f"Debt_RedString_{si}", root, pin_pts[i0], pin_pts[i1], 0.005, 0.005, mats["wax_red"], segments=4, col=col)

    add_wooden_crate("North_CrateA", root, (-3.55, hy - 0.78, 0.253 + 0.29), (0.60, 0.60, 0.58), mats, trim_mat=mats["crimson_plank"], col=col)
    add_wooden_crate("North_CrateB", root, (-2.80, hy - 0.76, 0.253 + 0.24), (0.50, 0.50, 0.48), mats, rot=(0, 0, -0.22), col=col)
    add_burlap_sack("North_Sack", root, (-2.22, hy - 0.72, 0.253), mats, scale=0.92, rot_z=0.4, col=col)

    add_bracket_lantern("North_WallSconce", root, (1.10, wy_wall_n, 2.86), (0.0, -1.0), mats, col=col)
    add_wall_clock("WallClock_AnhBay", root, (2.15, wy_wall_n, 2.36), (0.0, -1.0), mats, col=col)

    # Vertical Crimson & Gold Syndicate Banner on NE pier (X=3.02, Y=4.68, clear of VaultCab_N at X=3.52)
    add_box("SyndicateBanner_Cloth", root, (3.02, hy - 0.32, 2.15), (0.48, 0.025, 1.04), mats["crimson_plank"], col=col)
    add_box("SyndicateBanner_Border", root, (3.02, hy - 0.335, 2.15), (0.08, 0.015, 0.92), mats["brass_gold"], col=col)
    add_cyl("SyndicateBanner_Rod", root, (3.02, hy - 0.33, 2.68), 0.020, 0.020, 0.62, mats["brass_gold"], verts=6, rot=(0, math.pi * 0.5, 0), col=col)

    add_box("Credenza_N", root, (1.70, 4.30, 0.253 + 0.36), (2.25, 0.62, 0.72), mats["walnut_dark"], col=col)
    add_box("CredenzaTop_N", root, (1.70, 4.30, 0.98), (2.35, 0.68, 0.07), mats["wood_dark"], col=col)
    add_whiskey_and_cigar_set("CredN_Whiskey", root, (0.92, 4.30, 1.015), mats, rot_z=math.pi, col=col)
    _add_book_cluster("CredN_Books", root, (1.48, 4.30, 1.015), 4, mats, seed=4, rot_z=math.pi * 0.5, col=col)
    add_gold_bullion_stack("CredN_Gold", root, (2.05, 4.30, 1.015), mats, rot_z=0.12, col=col)
    add_candlestick("CredN_Candle", root, (2.58, 4.30, 1.015), mats, rot_z=-0.3, col=col)
    return root


def _add_cut_window_bars_and_sill(root, wall_x_or_y, pos, axis, win_w, win_h, wall_t, mats, cols=3, rows=2, frame_mat=None, sill_depth=None, name="W", with_shutters=False, col=None):
    _add_cut_window(name, root, pos, axis, win_w, win_h, wall_t, mats, cols=cols, rows=rows, frame_mat=frame_mat, sill_depth=sill_depth, col=col)
    if with_shutters and axis == "x":
        cx, cy, cz = pos
        sw = 0.32
        off_y = win_w * 0.5 + 0.14 + sw * 0.5
        for s_tag, sy, ang in (("ShutL", cy - off_y, -0.18), ("ShutR", cy + off_y, 0.18)):
            add_box(f"{name}_{s_tag}", root, (cx - 0.20, sy, cz), (0.05, sw, win_h), mats["emerald_shutter"], rot=(0, 0, ang), col=col)


def build_kim_khuy(mats, font, col):
    """Shop #4 (Cozy Vertical Slice — Walkable 2-Story Atelier): Kho Tạo Tác & Lò Rèn Cơ Khí–Cổ Phong Kim Khuy.
    Indigo F1 & golden-ochre F2 exterior with signature 2-tier stepped terracotta roof, cut-through F1 & F2 windows,
    flush brass gear crest, porch/stairs & flowerboxes.
    - Floor 1 (Z=0.253): Indigo-gold rug, glowing stone Artificer Forge + bellows, horned Blacksmith Anvil & quench tub,
      Artificer Resonance Counter (CraftBench X=1.80, top Z=1.110) with 6 velvet/brass relic pads, spinning brass drive gears,
      specimen cabinets & walkable 14-step North Timber Staircase up to Floor 2!
    - Floor 2 (Z=3.250): L-shaped Drafting & Observatory Loft overlooking the double-height atrium & clockwork chandelier,
      with slanted Blueprint Drafting Table, Astronomical Brass Telescope aimed out F2 window, Armillary Resonance Table,
      Wall Clock & Tea Credenza.
    """
    depth_x, width_y, f1_h, f2_h = 9.0, 9.0, 3.25, 2.80
    hx, hy = depth_x * 0.5, width_y * 0.5
    door_y, door_w, door_h = 0.0, 2.6, 2.75
    belt_z = 0.24 + f1_h
    eave_z = belt_z + f2_h
    ridge_h = 2.05
    total_h = f1_h + f2_h
    f2_z = 3.250
    txt_rot = (math.pi * 0.5, 0.0, -math.pi * 0.5)

    root = bpy.data.objects.new("LP_Shop_KimKhuy", None)
    link_obj(root, col)

    # 0. Stone Plinth, Staggered Warm Oak Plank Floor 1 (top Z=0.253), Indigo-Gold Rug & Afternoon Sunbeams
    add_box("BasePlinth", root, (0.0, 0.0, 0.12), (depth_x, width_y, 0.24), mats["curb_stone"], col=col)
    add_box("FloorBase", root, (0.0, 0.0, 0.236), (depth_x - 0.30, width_y - 0.30, 0.016), mats["walnut_dark"], col=col)
    for row_i in range(13):
        py = -4.08 + row_i * 0.68
        for col_i in range(5):
            px_raw = -3.45 + col_i * 1.72 + (0.35 if row_i % 2 == 1 else -0.15)
            x0 = max(-hx + 0.24, px_raw - 0.82)
            x1 = min(hx - 0.24, px_raw + 0.82)
            if x1 - x0 < 0.45:
                continue
            p_mat = mats["oak_plank_a"] if (row_i + col_i) % 3 != 0 else mats["oak_plank_b"]
            add_box(f"OakPlank_{row_i}_{col_i}", root, ((x0 + x1) * 0.5, py, 0.246), (x1 - x0, 0.64, 0.014), p_mat, col=col)

    add_box("Atelier_RugTrim", root, (-1.05, 0.0, 0.254), (4.60, 2.30, 0.008), mats["brass_gold"], col=col)
    add_box("Atelier_RugBody", root, (-1.05, 0.0, 0.258), (4.36, 2.06, 0.008), mats["indigo_shop"], col=col)
    for si, (sbx, sby) in enumerate(((-2.8, 0.0), (-2.6, -2.5), (-2.6, 2.5), (-1.4, 2.8))):
        add_box(f"Sunbeam_{si}", root, (sbx, sby, 0.264), (1.35, 0.72, 0.003), mats["sunbeam_floor"], rot=(0, 0, 0.28), col=col)

    # 1. East Back Wall (+X) — Full 2-Story Structural Envelope + Seamless F1 & F2 Interior Plaster (inner face X=4.205)
    back_x = hx - 0.14
    add_box("Wall_Back_F1", root, (back_x, 0.0, 0.24 + f1_h * 0.5), (0.24, width_y - 0.20, f1_h), mats["indigo_shop"], col=col)
    add_box("Wall_Back_F2", root, (back_x, 0.0, belt_z + f2_h * 0.5), (0.24, width_y - 0.20, f2_h), mats["sunny_yellow"], col=col)
    add_box("Int_Wain_Back", root, (back_x - 0.14, 0.0, 0.24 + 0.50), (0.05, width_y - 0.44, 1.00), mats["teal_wainscot"], col=col)
    add_box("Int_Chair_Back", root, (back_x - 0.17, 0.0, 1.24), (0.08, width_y - 0.44, 0.08), mats["wood_dark"], col=col)
    add_box("Int_Upper_Back_F1", root, (back_x - 0.13, 0.0, 2.26), (0.05, width_y - 0.44, 2.04), mats["warm_cream"], col=col)
    add_box("Int_Wain_Back_F2", root, (back_x - 0.14, 0.0, f2_z + 0.45), (0.05, width_y - 0.44, 0.90), mats["honey_wood"], col=col)
    add_box("Int_Chair_Back_F2", root, (back_x - 0.17, 0.0, f2_z + 0.90), (0.08, width_y - 0.44, 0.08), mats["wood_dark"], col=col)
    add_box("Int_Upper_Back_F2", root, (back_x - 0.13, 0.0, f2_z + 1.98), (0.05, width_y - 0.44, 2.16), mats["warm_cream"], col=col)

    # 2. West Front Street Wall (-X): F1 Doorway + 2 Cut Windows & F2 2 Shuttered Cut Windows
    front_x = -hx + 0.14
    for p_name, y0, y1 in (("S", -hy + 0.10, -3.675), ("MidS", -2.025, -1.30), ("MidN", 1.30, 2.025), ("N", 3.675, hy - 0.10)):
        pw = y1 - y0
        yc = (y0 + y1) * 0.5
        add_box(f"FrontPier_{p_name}", root, (front_x, yc, 0.24 + f1_h * 0.5), (0.26, pw, f1_h), mats["indigo_shop"], col=col)
        add_box(f"Int_Wain_W_{p_name}", root, (front_x + 0.14, yc, 0.24 + 0.50), (0.05, pw, 1.00), mats["teal_wainscot"], col=col)
        add_box(f"Int_Chair_W_{p_name}", root, (front_x + 0.17, yc, 1.24), (0.08, pw, 0.08), mats["wood_dark"], col=col)
        add_box(f"Int_Upper_W_{p_name}", root, (front_x + 0.13, yc, 2.38), (0.05, pw, 2.24), mats["warm_cream"], col=col)

    for w_name, wy_c in (("Win_WestS", -2.85), ("Win_WestN", 2.85)):
        ww = 1.65
        add_box(f"Wall_West_Bot_{w_name}", root, (front_x, wy_c, 0.6825), (0.26, ww, 0.885), mats["indigo_shop"], col=col)
        add_box(f"Int_Wain_W_Bot_{w_name}", root, (front_x + 0.14, wy_c, 0.6825), (0.05, ww, 0.885), mats["teal_wainscot"], col=col)
        add_box(f"Wall_West_Top_{w_name}", root, (front_x, wy_c, 3.0325), (0.26, ww, 0.915), mats["indigo_shop"], col=col)
        add_box(f"Int_Upper_W_Top_{w_name}", root, (front_x + 0.13, wy_c, 3.0325), (0.05, ww, 0.915), mats["warm_cream"], col=col)
        _add_cut_window(w_name, root, (front_x, wy_c, 1.85), "x", ww, 1.45, 0.28, mats, cols=3, rows=2, col=col)

    add_box("FrontLintel_Door", root, (front_x, door_y, 0.24 + door_h + (f1_h - door_h) * 0.5), (0.26, door_w + 0.10, f1_h - door_h), mats["indigo_shop"], col=col)
    add_box("Int_Upper_DoorLintel", root, (front_x + 0.13, door_y, 0.24 + door_h + (f1_h - door_h) * 0.5), (0.05, door_w + 0.04, f1_h - door_h), mats["warm_cream"], col=col)
    add_box("DoorFrame_L", root, (-hx, -door_w * 0.5 - 0.07, 0.24 + door_h * 0.5), (0.36, 0.18, door_h), mats["pure_white"], col=col)
    add_box("DoorFrame_R", root, (-hx, door_w * 0.5 + 0.07, 0.24 + door_h * 0.5), (0.36, 0.18, door_h), mats["pure_white"], col=col)
    add_box("DoorFrame_Top", root, (-hx, door_y, 0.24 + door_h + 0.09), (0.40, door_w + 0.36, 0.22), mats["pure_white"], col=col)

    # F2 West Cut-Through Windows (-2.65 & +2.65, width 1.40 -> outer piers & center pier clear of Ext_ArtificerRose!)
    for p_name, y0, y1 in (("S", -hy + 0.10, -3.35), ("C", -1.95, 1.95), ("N", 3.35, hy - 0.10)):
        pw = y1 - y0
        yc = (y0 + y1) * 0.5
        add_box(f"Wall_West_F2_{p_name}", root, (front_x, yc, belt_z + f2_h * 0.5), (0.26, pw, f2_h), mats["sunny_yellow"], col=col)
        add_box(f"Int_Upper_W_F2_{p_name}", root, (front_x + 0.13, yc, belt_z + f2_h * 0.5), (0.05, pw, f2_h), mats["warm_cream"], col=col)
    for w_name, wy_u in (("F2_Win_WestS", -2.65), ("F2_Win_WestN", 2.65)):
        ww = 1.40
        add_box(f"Wall_West_F2_Bot_{w_name}", root, (front_x, wy_u, 3.805), (0.26, ww, 0.63), mats["sunny_yellow"], col=col)
        add_box(f"Int_W_F2_Bot_{w_name}", root, (front_x + 0.13, wy_u, 3.805), (0.05, ww, 0.63), mats["warm_cream"], col=col)
        add_box(f"Wall_West_F2_Top_{w_name}", root, (front_x, wy_u, 5.925), (0.26, ww, 0.73), mats["sunny_yellow"], col=col)
        add_box(f"Int_W_F2_Top_{w_name}", root, (front_x + 0.13, wy_u, 5.925), (0.05, ww, 0.73), mats["warm_cream"], col=col)
        _add_cut_window_bars_and_sill(root, front_x, (front_x, wy_u, 4.84), "x", ww, 1.44, 0.28, mats, cols=2, rows=2, name=w_name, with_shutters=True, col=col)

    # 3. South Alley Wall (-Y, flush exterior so zero clash across 0.70m gap with Shop_AnhBay; F2 clerestory window at E)
    wy_s = -hy + 0.14
    add_box("Wall_South_F1", root, (0.0, wy_s, 0.24 + f1_h * 0.5), (depth_x - 0.20, 0.24, f1_h), mats["indigo_shop"], col=col)
    add_box("Int_Wain_South", root, (0.0, wy_s + 0.14, 0.24 + 0.50), (depth_x - 0.44, 0.05, 1.00), mats["teal_wainscot"], col=col)
    add_box("Int_Chair_South", root, (0.0, wy_s + 0.17, 1.24), (depth_x - 0.44, 0.08, 0.08), mats["wood_dark"], col=col)
    add_box("Int_Upper_South_F1", root, (0.0, wy_s + 0.13, 2.38), (depth_x - 0.44, 0.05, 2.24), mats["warm_cream"], col=col)

    for p_name, x0, x1 in (("SW", -hx + 0.10, 1.30), ("SE", 2.90, hx - 0.10)):
        sw = x1 - x0
        xc = (x0 + x1) * 0.5
        add_box(f"Wall_South_F2_{p_name}", root, (xc, wy_s, belt_z + f2_h * 0.5), (sw, 0.24, f2_h), mats["sunny_yellow"], col=col)
        add_box(f"Int_Upper_S_F2_{p_name}", root, (xc, wy_s + 0.13, belt_z + f2_h * 0.5), (sw, 0.05, f2_h), mats["warm_cream"], col=col)
    add_box("Wall_South_F2_WinBot", root, (2.10, wy_s, 3.805), (1.60, 0.24, 0.63), mats["sunny_yellow"], col=col)
    add_box("Int_S_F2_WinBot", root, (2.10, wy_s + 0.13, 3.805), (1.60, 0.05, 0.63), mats["warm_cream"], col=col)
    add_box("Wall_South_F2_WinTop", root, (2.10, wy_s, 5.925), (1.60, 0.24, 0.73), mats["sunny_yellow"], col=col)
    add_box("Int_S_F2_WinTop", root, (2.10, wy_s + 0.13, 5.925), (1.60, 0.05, 0.73), mats["warm_cream"], col=col)
    _add_cut_window("F2_Win_SouthE", root, (2.10, wy_s, 4.84), "y", 1.60, 1.44, 0.26, mats, cols=3, rows=2, sill_depth=0.30, col=col)

    # 4. North Side Wall (+Y): F1 2 Cut-Through Stair & Counter Windows + F2 Cut-Through North Loft Window
    wy_n = hy - 0.14
    for seg_name, x0, x1 in (("W", -hx + 0.10, -2.35), ("Mid", -0.75, 1.30), ("E", 2.90, hx - 0.10)):
        sw = x1 - x0
        xc = (x0 + x1) * 0.5
        add_box(f"Wall_North_F1_{seg_name}", root, (xc, wy_n, 0.24 + f1_h * 0.5), (sw, 0.24, f1_h), mats["indigo_shop"], col=col)
        add_box(f"Int_Wain_N_{seg_name}", root, (xc, wy_n - 0.14, 0.24 + 0.50), (sw, 0.05, 1.00), mats["teal_wainscot"], col=col)
        add_box(f"Int_Chair_N_{seg_name}", root, (xc, wy_n - 0.17, 1.24), (sw, 0.08, 0.08), mats["wood_dark"], col=col)
        add_box(f"Int_Upper_N_{seg_name}", root, (xc, wy_n - 0.13, 2.38), (sw, 0.05, 2.24), mats["warm_cream"], col=col)
    for w_name, wx_c in (("Win_NorthW", -1.55), ("Win_NorthE", 2.10)):
        ww = 1.60
        add_box(f"Wall_North_Bot_{w_name}", root, (wx_c, wy_n, 0.6825), (ww, 0.24, 0.885), mats["indigo_shop"], col=col)
        add_box(f"Int_Wain_N_Bot_{w_name}", root, (wx_c, wy_n - 0.14, 0.6825), (ww, 0.05, 0.885), mats["teal_wainscot"], col=col)
        add_box(f"Wall_North_Top_{w_name}", root, (wx_c, wy_n, 3.0325), (ww, 0.24, 0.915), mats["indigo_shop"], col=col)
        add_box(f"Int_Upper_N_Top_{w_name}", root, (wx_c, wy_n - 0.13, 3.0325), (ww, 0.05, 0.915), mats["warm_cream"], col=col)
        _add_cut_window(w_name, root, (wx_c, wy_n, 1.85), "y", ww, 1.45, 0.26, mats, cols=3, rows=2, sill_depth=0.34, col=col)

    for seg_name, x0, x1 in (("NW", -hx + 0.10, 1.20), ("NE", 3.00, hx - 0.10)):
        sw = x1 - x0
        xc = (x0 + x1) * 0.5
        add_box(f"Wall_North_F2_{seg_name}", root, (xc, wy_n, belt_z + f2_h * 0.5), (sw, 0.24, f2_h), mats["sunny_yellow"], col=col)
        add_box(f"Int_Upper_N_F2_{seg_name}", root, (xc, wy_n - 0.13, belt_z + f2_h * 0.5), (sw, 0.05, f2_h), mats["warm_cream"], col=col)
    add_box("Wall_North_F2_WinBot", root, (2.10, wy_n, 3.805), (1.80, 0.24, 0.63), mats["sunny_yellow"], col=col)
    add_box("Int_N_F2_WinBot", root, (2.10, wy_n - 0.13, 3.805), (1.80, 0.05, 0.63), mats["warm_cream"], col=col)
    add_box("Wall_North_F2_WinTop", root, (2.10, wy_n, 5.925), (1.80, 0.24, 0.73), mats["sunny_yellow"], col=col)
    add_box("Int_N_F2_WinTop", root, (2.10, wy_n - 0.13, 5.925), (1.80, 0.05, 0.73), mats["warm_cream"], col=col)
    _add_cut_window("F2_Win_NorthE", root, (2.10, wy_n, 4.84), "y", 1.80, 1.44, 0.26, mats, cols=3, rows=2, sill_depth=0.34, col=col)

    # 5. Belt Cornice, Corner Quoin Posts, 2-Tier Stepped Terracotta Roof & Exposed Loft Trusses
    add_box("BeltTrim_W", root, (-hx - 0.06, 0.0, belt_z), (0.20, width_y + 0.16, 0.16), mats["pure_white"], col=col)
    add_box("BeltTrim_E", root, (hx + 0.06, 0.0, belt_z), (0.20, width_y + 0.16, 0.16), mats["pure_white"], col=col)
    add_box("BeltTrim_S", root, (0.0, -hy - 0.04, belt_z), (depth_x + 0.16, 0.16, 0.16), mats["pure_white"], col=col)
    add_box("BeltTrim_N", root, (0.0, hy + 0.06, belt_z), (depth_x + 0.16, 0.20, 0.16), mats["pure_white"], col=col)
    for cx in (-hx + 0.08, hx - 0.08):
        for cy in (-hy + 0.08, hy - 0.08):
            add_box(f"CornerPost_{cx:.1f}_{cy:.1f}", root, (cx, cy, 0.24 + total_h * 0.5), (0.24, 0.24, total_h + 0.06), mats["pure_white"], col=col)

    add_pitched_terracotta_roof("Roof", root, eave_z, hx, hy, ridge_h, mats, gable_mat=mats["sunny_yellow"], interior_lining_mat=mats["honey_wood"], col=col)
    # Pitched-bottom UpperStepTower resting cleanly above the sloped main roof (zero interior ceiling protrusion!)
    bm_ut = bmesh.new()
    ut_x0, ut_xm, ut_x1 = -0.85, 0.0, 3.65
    ut_y0, ut_y1 = -2.90, 2.90
    ut_zt = eave_z + 2.40
    ut_zb0 = eave_z + ridge_h * (1.0 - abs(ut_x0) / hx) - 0.02
    ut_zbm = eave_z + ridge_h - 0.02
    ut_zb1 = eave_z + ridge_h * (1.0 - abs(ut_x1) / hx) - 0.02
    vs = [
        bm_ut.verts.new((ut_x0, ut_y0, ut_zb0)), bm_ut.verts.new((ut_xm, ut_y0, ut_zbm)), bm_ut.verts.new((ut_x1, ut_y0, ut_zb1)),
        bm_ut.verts.new((ut_x0, ut_y0, ut_zt)),  bm_ut.verts.new((ut_xm, ut_y0, ut_zt)),  bm_ut.verts.new((ut_x1, ut_y0, ut_zt)),
        bm_ut.verts.new((ut_x0, ut_y1, ut_zb0)), bm_ut.verts.new((ut_xm, ut_y1, ut_zbm)), bm_ut.verts.new((ut_x1, ut_y1, ut_zb1)),
        bm_ut.verts.new((ut_x0, ut_y1, ut_zt)),  bm_ut.verts.new((ut_xm, ut_y1, ut_zt)),  bm_ut.verts.new((ut_x1, ut_y1, ut_zt)),
    ]
    for f_idx in (
        (0, 1, 4, 3), (1, 2, 5, 4),       # South wall (2 planar quads split at ridge x=0)
        (6, 9, 10, 7), (7, 10, 11, 8),    # North wall
        (0, 3, 9, 6),                     # West wall
        (2, 8, 11, 5),                    # East wall
        (3, 4, 10, 9), (4, 5, 11, 10),    # Top flat roof seat
        (0, 6, 7, 1), (1, 7, 8, 2),       # Pitched bottom following main roof slope
    ):
        bm_ut.faces.new(tuple(vs[i] for i in f_idx))
    bmesh.ops.recalc_face_normals(bm_ut, faces=bm_ut.faces)
    _finalize_bm("UpperStepTower", root, bm_ut, (0, 0, 0), (0, 0, 0), mats["crimson_plank"], col)

    ut_roof_root = bpy.data.objects.new("UpperStepRoof_Pivot", None)
    ut_roof_root.location = Vector((1.4, 0.0, 0.0))
    ut_roof_root.parent = root
    link_obj(ut_roof_root, col)
    add_pitched_terracotta_roof("UpperStepRoof", ut_roof_root, ut_zt, 2.25, 2.90, 1.25, mats, gable_mat=mats["sunny_yellow"], col=col)
    for dw in (-1.45, 1.45):
        add_window(f"UpperTowerWin_{dw:.1f}", root, (ut_x0 - 0.01, dw, eave_z + 2.02), "x", 0.85, 0.62, mats, with_shutters=False, col=col)

    for rti in range(11):
        rty = -4.0 + rti * (8.0 / 10.0)
        add_box(f"Ext_RafterTailW_{rti}", root, (-hx - 0.26, rty, eave_z - 0.14), (0.58, 0.11, 0.14), mats["wood_dark"], rot=(0, -0.34, 0), col=col)

    # Exposed Cathedral Roof Trusses spanning high Floor 2 ceiling (clear above Z=6.18)
    add_box("F2_RidgeBeam", root, (0.0, 0.0, eave_z + ridge_h - 0.24), (0.24, width_y - 0.44, 0.24), mats["wood_dark"], col=col)
    for ty in (-2.5, 0.0, 2.5):
        add_box(f"F2_TieBeam_{ty:.1f}", root, (0.0, ty, eave_z - 0.08), (depth_x - 0.46, 0.18, 0.20), mats["wood_dark"], col=col)
        add_box(f"F2_KingPost_{ty:.1f}", root, (0.0, ty, eave_z + (ridge_h - 0.25) * 0.5), (0.18, 0.18, ridge_h - 0.25), mats["honey_wood"], col=col)

    # 6. Exterior Porch, Stairs, Handrails, Canopy, Signboard, Brass Gear Crest, Lanterns & Flowerboxes
    add_box("Porch_SubBase", root, (-5.06, 0.0, 0.14), (1.14, 3.76, 0.12), mats["wood_dark"], col=col)
    for pi in range(5):
        px_d = -4.61 - pi * 0.22
        add_box(f"Porch_DeckPlank_{pi}", root, (px_d, 0.0, 0.22), (0.20, 3.80, 0.04), mats["timber_light"] if pi % 2 == 0 else mats["honey_wood"], col=col)
    add_box("Porch_StairUpper", root, (-5.84, 0.0, 0.12), (0.38, 3.12, 0.08), mats["timber_light"], col=col)
    add_box("Porch_StairLower", root, (-6.22, 0.0, 0.08), (0.38, 3.12, 0.04), mats["honey_wood"], col=col)
    add_box("Porch_StairStringL", root, (-6.02, -1.58, 0.12), (0.82, 0.09, 0.16), mats["wood_dark"], rot=(0, -0.20, 0), col=col)
    add_box("Porch_StairStringR", root, (-6.02, 1.58, 0.12), (0.82, 0.09, 0.16), mats["wood_dark"], rot=(0, -0.20, 0), col=col)

    for r_sgn, ry_rail, ry_ret in (("L", -1.62, -1.86), ("R", 1.62, 1.86)):
        add_box(f"Rail_TopPost_{r_sgn}", root, (-5.58, ry_rail, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_box(f"Rail_BotPost_{r_sgn}", root, (-6.40, ry_rail, 0.50), (0.09, 0.09, 0.84), mats["charcoal"], col=col)
        add_box(f"Rail_WallPost_{r_sgn}", root, (-4.62, ry_ret, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_box(f"Rail_CornerPost_{r_sgn}", root, (-5.58, ry_ret, 0.68), (0.09, 0.09, 0.92), mats["charcoal"], col=col)
        add_limb(f"Rail_SlopedBar_{r_sgn}", root, (-5.58, ry_rail, 1.12), (-6.40, ry_rail, 0.90), 0.036, 0.036, mats["charcoal"], segments=6, col=col)
        add_limb(f"Rail_MidBar_{r_sgn}", root, (-5.58, ry_rail, 0.72), (-6.40, ry_rail, 0.52), 0.022, 0.022, mats["charcoal"], segments=6, col=col)
        add_limb(f"Rail_ReturnBar_{r_sgn}", root, (-4.62, ry_ret, 1.12), (-5.58, ry_ret, 1.12), 0.032, 0.032, mats["charcoal"], segments=6, col=col)
        add_limb(f"Rail_FrontReturn_{r_sgn}", root, (-5.58, ry_ret, 1.12), (-5.58, ry_rail, 1.12), 0.032, 0.032, mats["charcoal"], segments=6, col=col)
        for cap_y in (ry_rail, ry_ret):
            add_box(f"Rail_Cap_{r_sgn}_{cap_y:.2f}", root, (-5.58, cap_y, 1.155), (0.12, 0.12, 0.035), mats["brass_gold"], col=col)
        add_box(f"Rail_CapBot_{r_sgn}", root, (-6.40, ry_rail, 0.935), (0.12, 0.12, 0.035), mats["brass_gold"], col=col)

    add_box("DoorHood_Roof", root, (-hx - 0.42, 0.0, 3.20), (0.86, 3.55, 0.12), mats["terracotta_roof"], rot=(0, -0.24, 0), col=col)
    add_box("DoorHood_Trim", root, (-hx - 0.80, 0.0, 3.11), (0.10, 3.59, 0.10), mats["wood_dark"], col=col)
    for cby in (-1.58, 1.58):
        add_limb(f"DoorHood_Bracket_{cby:.1f}", root, (-hx - 0.04, cby, 2.76), (-hx - 0.68, cby, 3.12), 0.055, 0.055, mats["wood_dark"], segments=6, col=col)

    add_box("Sign_Back", root, (-hx - 0.09, 0.0, 3.84), (0.16, 3.40, 0.80), mats["honey_wood"], col=col)
    add_box("Sign_Face", root, (-hx - 0.16, 0.0, 3.84), (0.10, 3.14, 0.60), mats["indigo_shop"], col=col)
    for sy_br, btag in ((-1.35, "L"), (1.35, "R")):
        add_box(f"Sign_Bracket_{btag}", root, (-hx - 0.01, sy_br, 3.84), (0.14, 0.12, 0.84), mats["charcoal"], col=col)
    add_3d_text_mesh("Sign_Text", root, "KIM KHUY", (-hx - 0.222, 0.0, 3.82), txt_rot, 0.34, mats["brass_gold"], font_data=font, col=col)

    # Center Artificer Gear Crest on F2 pier (Y=0.0, clear between F2_Win_WestS/N at ±2.65!)
    add_gear_wheel("Ext_ArtificerRose", root, (-hx - 0.08, 0.0, belt_z + 1.55), 0.46, 0.12, 8, mats["brass_gold"], mats["indigo_shop"], rot=(0, math.pi * 0.5, 0), col=col)
    add_ico("Ext_RoseCore", root, (-hx - 0.14, 0.0, belt_z + 1.55), (0.08, 0.12, 0.12), mats["amber_glass"], subdivisions=1, col=col)

    for ly_p, ltag in ((-1.56, "L"), (1.56, "R")):
        add_bracket_lantern(f"PorchLan_{ltag}", root, (-hx, ly_p, 2.84), (-1.0, 0.0), mats, col=col)

    add_limb("Flag_Pole", root, (-hx - 0.04, -2.02, 3.12), (-hx - 0.88, -2.02, 3.58), 0.032, 0.026, mats["wood_dark"], segments=6, col=col)
    add_box("Flag_ClothA", root, (-hx - 0.66, -2.02, 3.11), (0.48, 0.03, 0.44), mats["indigo_shop"], rot=(0, -0.15, 0), col=col)
    add_box("Flag_Stripe", root, (-hx - 0.66, -2.02, 3.11), (0.14, 0.04, 0.44), mats["brass_gold"], rot=(0, -0.15, 0), col=col)

    add_window_flowerbox("FlowerBox_WestS", root, (-hx, -2.85, 0.945), "x", -1.0, 1.58, mats, col=col)
    add_window_flowerbox("FlowerBox_WestN", root, (-hx, 2.85, 0.945), "x", -1.0, 1.58, mats, col=col)
    add_faceted_barrel("Porch_Barrel", root, (-hx - 0.55, 3.65, 0.08), 0.28, 0.72, mats, col=col)
    add_wooden_crate("Porch_Crate", root, (-hx - 0.58, 2.95, 0.36), (0.54, 0.54, 0.54), mats, trim_mat=mats["indigo_shop"], rot=(0, 0, 0.18), col=col)

    # === 7. FLOOR 1 INTERIOR: COUNTER + RELIC PADS, GLOWING FORGE, ANVIL, BACK SPECIMEN RACKS & STAIRCASE ===
    # Structural F2 Support Posts (strictly framing corners & catwalk opening, leaving center aisle & stair access wide open!)
    for px, py in ((0.85, -2.55), (0.85, 3.25), (-4.12, -2.55)):
        add_box(f"F2_SupportPost_{px:.2f}_{py:.2f}", root, (px, py, 0.253 + 1.46), (0.20, 0.20, 2.92), mats["wood_dark"], col=col)
    for px, py in ((-4.18, -4.18), (-4.18, 4.18), (4.18, -4.18), (4.18, 4.18)):
        add_box(f"Int_TimberPost_{px:.1f}_{py:.1f}", root, (px, py, 0.24 + total_h * 0.5), (0.18, 0.18, total_h), mats["wood_dark"], col=col)

    # Grand Artificer Display & Resonance Counter (X=1.80, Y=0.0, top Z=1.110; relic displays at (1.75, -1.25..+1.25))
    add_box("CraftBench", root, (1.80, 0.0, 0.253 + 0.39), (1.05, 3.40, 0.78), mats["honey_wood"], col=col)
    add_box("CraftBench_Top", root, (1.80, 0.0, 1.07), (1.20, 3.55, 0.08), mats["wood_dark"], col=col)
    add_cyl("CraftBench_FootRail", root, (1.18, 0.0, 0.35), 0.028, 0.028, 3.25, mats["brass_gold"], verts=8, rot=(math.pi * 0.5, 0, 0), col=col)
    for cy in (-1.10, 0.0, 1.10):
        add_box(f"CraftBench_Trim_{cy:.1f}", root, (1.29, cy, 0.64), (0.04, 0.88, 0.54), mats["indigo_shop"], col=col)
    for di in range(6):
        py_pad = -1.25 + di * 0.50
        add_cyl(f"RelicPad_Rim_{di}", root, (1.75, py_pad, 1.116), 0.18, 0.19, 0.012, mats["brass_gold"], verts=8, col=col)
        add_cyl(f"RelicPad_Velvet_{di}", root, (1.75, py_pad, 1.123), 0.15, 0.15, 0.008, mats["indigo_shop"] if di % 2 == 0 else mats["crimson_velvet"], verts=8, col=col)

    # Animated Clockwork Drive Gears cleanly spinning horizontally on flanking pedestals clear of relics!
    for gy_g, gname in ((-2.06, "CraftGearA"), (2.06, "CraftGearB")):
        add_box(f"{gname}_Ped", root, (1.80, gy_g, 0.253 + 0.29), (0.44, 0.44, 0.58), mats["wood_dark"], col=col)
        add_box(f"{gname}_PedTop", root, (1.80, gy_g, 0.84), (0.48, 0.48, 0.05), mats["honey_wood"], col=col)
        add_gear_wheel(gname, root, (1.80, gy_g, 0.90), 0.24, 0.05, 8, mats["brass_gold"], mats["charcoal"], col=col)

    add_banker_lamp("Counter_BankerLamp", root, (1.98, -1.58, 1.110), mats, rot_z=(math.pi - 0.2), col=col)
    add_workbench_loupe("Counter_Loupe", root, (1.95, 1.58, 1.110), mats, rot_z=(math.pi + 0.25), col=col)

    # Under-loft cozy pendants above CraftBench (canopy seated flush against F2 deck bottom Z=3.090)
    for pi, ly_p in enumerate((-1.15, 1.15)):
        add_pendant_lamp(f"Pendant_F1_{pi}", root, (1.55, ly_p, 3.090), 2.62, mats, shade_mat=mats["brass_gold"], col=col)

    # South-West Forge & Anvil Station + South Tool Bench
    add_artificer_forge("Artificer_Forge", root, (-1.10, -3.72, 0.253), mats, rot_z=(math.pi * 0.5), col=col)
    add_blacksmith_anvil("Artificer_Anvil", root, (-2.65, -2.55, 0.253), mats, rot_z=0.35, col=col)

    add_box("F1_SouthBench", root, (1.50, -3.86, 0.253 + 0.36), (1.70, 0.62, 0.72), mats["walnut_dark"], col=col)
    add_box("F1_SouthBenchTop", root, (1.50, -3.86, 0.98), (1.80, 0.68, 0.07), mats["honey_wood"], col=col)
    add_oil_lamp("F1_BenchLamp", root, (0.85, -3.86, 1.015), mats, rot_z=0.3, col=col)
    add_open_ledger_and_quill("F1_BenchLedger", root, (1.55, -3.86, 1.015), mats, rot_z=(math.pi * 0.5 - 0.1), col=col)
    add_sculpted_vase("F1_BenchVase", root, (2.18, -3.86, 1.015), 0.32, 0.088, mats["ceramic_sage"], mats, with_handles=True, col=col)
    add_bracket_lantern("F1_SouthSconce", root, (1.50, -4.205, 2.45), (0.0, 1.0), mats, col=col)
    add_box("F1_ToolBoardBack", root, (1.50, -4.18, 1.85), (1.50, 0.06, 0.95), mats["walnut_dark"], col=col)
    add_box("F1_ToolBoardPlate", root, (1.50, -4.155, 1.85), (1.36, 0.03, 0.82), mats["slate_floor"], col=col)
    for ti, (tx_t, tlen, tm) in enumerate(((1.05, 0.42, "inox"), (1.35, 0.50, "brass_gold"), (1.65, 0.46, "inox"), (1.95, 0.38, "charcoal"))):
        add_box(f"F1_RackTool_{ti}", root, (tx_t, -4.12, 1.85), (0.055, 0.025, tlen), mats[tm], col=col)
        add_box(f"F1_RackHandle_{ti}", root, (tx_t, -4.12, 2.08), (0.14 if ti == 0 else 0.065, 0.04, 0.08), mats["honey_wood"], col=col)

    # East Back Wall Specimen Racks on Floor 1 (X=3.85, Y=-3.6..-1.8 and +1.8..+3.6; center open for NPC_KimKhuy!)
    for sy_c, stag in ((-2.75, "S"), (2.75, "N")):
        add_box(f"SpecCab_Back_{stag}", root, (4.15, sy_c, 0.253 + 1.40), (0.12, 1.72, 2.80), mats["walnut_dark"], col=col)
        for sz in (0.92, 1.62, 2.32):
            add_box(f"SpecCab_Shelf_{stag}_{sz:.2f}", root, (3.85, sy_c, sz), (0.60, 1.62, 0.06), mats["honey_wood"], col=col)
        for dy_d, dtag in ((-0.82, "L"), (0.82, "R")):
            add_box(f"SpecCab_Div_{stag}_{dtag}", root, (3.85, sy_c + dy_d, 0.253 + 1.38), (0.58, 0.06, 2.76), mats["wood_dark"], col=col)
        add_box(f"SpecCab_Crown_{stag}", root, (3.84, sy_c, 3.04), (0.64, 1.78, 0.08), mats["brass_gold"], col=col)

    _add_book_cluster("SpecS_Books_Bot", root, (3.76, -3.10, 0.950), 4, mats, seed=2, col=col)
    add_apothecary_bottle("SpecS_Flask_Bot", root, (3.76, -2.38, 0.950), 0.28, 0.070, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    add_armillary_globe("SpecS_Armillary", root, (3.76, -3.05, 1.650), mats, scale=0.80, rot_z=0.25, col=col)
    add_sculpted_vase("SpecS_Vase", root, (3.76, -2.38, 1.650), 0.34, 0.092, mats["ceramic_sage"], mats, with_handles=True, col=col)
    add_table_lantern("SpecS_Lantern", root, (3.76, -2.75, 2.350), mats, rot_z=0.2, scale=0.88, col=col)

    add_sculpted_vase("SpecN_Vase_Bot", root, (3.76, 2.40, 0.950), 0.34, 0.092, mats["terracotta_roof"], mats, with_handles=True, col=col)
    _add_book_cluster("SpecN_Books_Bot", root, (3.76, 3.10, 0.950), 4, mats, seed=6, col=col)
    add_apothecary_bottle("SpecN_FlaskA", root, (3.76, 2.38, 1.650), 0.26, 0.066, mats["jade_glass"], mats, stopper_mat=mats["honey_wood"], col=col)
    add_apothecary_bottle("SpecN_FlaskB", root, (3.76, 2.75, 1.650), 0.30, 0.072, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    add_scroll_bundle("SpecN_Scroll", root, (3.76, 2.75, 2.350), 0.40, 0.054, mats["blueprint_blue"], mats, rot_z=(math.pi * 0.5 + 0.08), col=col)

    # F1 Window Sill Props (flush at Z=1.185)
    _add_potted_herb("Sill_F1_WestS", root, (front_x + 0.12, -2.85, 1.185), mats["terracotta_roof"], mats, scale=0.92, col=col)
    add_apothecary_bottle("Sill_F1_WestN", root, (front_x + 0.12, 2.85, 1.185), 0.24, 0.064, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    _add_potted_herb("Sill_F1_NorthE", root, (2.10, wy_n - 0.14, 1.185), mats["ceramic_sage"], mats, scale=0.92, col=col)

    # Walkable 14-Step North Timber Staircase (X=-3.40 -> +0.80, Y=+3.30..+4.14, Z=0.253 -> 3.250)
    stair_y = 3.72
    for sy_off, st_tag in ((-0.36, "S"), (0.36, "N")):
        add_limb(f"Stair_Stringer_{st_tag}", root, (-3.42, stair_y + sy_off, 0.253), (0.82, stair_y + sy_off, f2_z - 0.08), 0.07, 0.07, mats["walnut_dark"], segments=4, col=col)
    for si in range(14):
        t0, t1 = si / 14.0, (si + 1) / 14.0
        tx_s = -3.40 + (si + 0.5) * (4.20 / 14.0)
        top_s = 0.253 + (f2_z - 0.253) * t1
        add_box(f"Stair_Tread_{si}", root, (tx_s, stair_y, top_s - 0.03), (0.32, 0.84, 0.06), mats["honey_wood"], col=col)
    add_box("Stair_BotPost", root, (-3.40, 3.28, 0.253 + 0.48), (0.12, 0.12, 0.96), mats["wood_dark"], col=col)
    add_box("Stair_TopPost", root, (0.85, 3.28, f2_z + 0.48), (0.14, 0.14, 0.96), mats["wood_dark"], col=col)
    add_limb("Stair_Handrail", root, (-3.40, 3.28, 0.253 + 0.94), (0.85, 3.28, f2_z + 0.94), 0.034, 0.034, mats["honey_wood"], segments=6, col=col)
    for bi in range(1, 8):
        u = bi / 8.0
        bx = -3.40 + 4.25 * u
        zb = 0.253 + (f2_z - 0.253) * u
        add_cyl(f"Stair_Bal_{bi}", root, (bx, 3.28, zb + 0.47), 0.025, 0.025, 0.92, mats["wood_dark"], verts=6, col=col)

    # Under-Stair Storage Cargo (grounded flush at Z=0.253 beneath high East end of stairs)
    add_wooden_crate("UnderStair_CrateA", root, (-0.15, 3.75, 0.253 + 0.28), (0.58, 0.58, 0.56), mats, trim_mat=mats["indigo_shop"], col=col)
    add_faceted_barrel("UnderStair_Barrel", root, (0.52, 3.78, 0.253), 0.26, 0.70, mats, col=col)
    add_burlap_sack("UnderStair_Sack", root, (-0.82, 3.78, 0.253), mats, scale=0.90, rot_z=0.3, col=col)

    # === 8. FLOOR 2 WALKABLE ARTIFICER LOFT (top Z=3.250), CHANDELIER, DRAFTING DESK & TELESCOPE ===
    # L-Shaped Loft Deck: East Deck (X=0.80..4.25, Y=-4.25..4.25) + South Catwalk (X=-4.25..0.80, Y=-4.25..-2.55)
    add_box("F2_Deck_East", root, (2.525, 0.0, f2_z - 0.08), (3.45, 8.50, 0.16), mats["honey_wood"], col=col)
    add_box("F2_Deck_Catwalk", root, (-1.725, -3.40, f2_z - 0.08), (5.05, 1.70, 0.16), mats["honey_wood"], col=col)
    add_box("F2_Fascia_East", root, (0.84, 0.35, f2_z - 0.08), (0.14, 5.80, 0.20), mats["wood_dark"], col=col)
    add_box("F2_Fascia_Catwalk", root, (-1.725, -2.59, f2_z - 0.08), (5.05, 0.14, 0.20), mats["wood_dark"], col=col)

    # F2 Guard Balustrades (leaving Y=+3.28..+4.20 stair landing completely open!)
    add_box("F2_RailTop_East", root, (0.85, 0.35, f2_z + 0.92), (0.12, 5.82, 0.09), mats["honey_wood"], col=col)
    add_box("F2_CornerNewel_SE", root, (0.85, -2.56, f2_z + 0.48), (0.16, 0.16, 0.96), mats["wood_dark"], col=col)
    for bi in range(9):
        by_b = -2.20 + bi * (5.10 / 8.0)
        add_cyl(f"F2_Bal_East_{bi}", root, (0.85, by_b, f2_z + 0.46), 0.036, 0.042, 0.84, mats["honey_wood"], verts=6, col=col)

    add_box("F2_RailTop_Cat", root, (-1.70, -2.58, f2_z + 0.92), (5.10, 0.12, 0.09), mats["honey_wood"], col=col)
    add_box("F2_EndNewel_Cat", root, (-4.18, -2.58, f2_z + 0.48), (0.16, 0.16, 0.96), mats["wood_dark"], col=col)
    for bi in range(8):
        bx_b = -3.85 + bi * (4.35 / 7.0)
        add_cyl(f"F2_Bal_Cat_{bi}", root, (bx_b, -2.58, f2_z + 0.46), 0.036, 0.042, 0.84, mats["honey_wood"], verts=6, col=col)

    # Grand Double-Height Brass Clockwork Orrery Chandelier hanging over Atrium (-1.45, 0.20)
    z_atr_ceil = eave_z + ridge_h * (1.0 - 1.45 / hx) - 0.15
    add_clockwork_chandelier("Atrium_Chandelier", root, (-1.45, 0.20, z_atr_ceil), 4.70, mats, col=col)

    # Floor 2 Drafting Desk + Craftsman Stool (North-East Loft, top flush at f2_z=3.250)
    add_drafting_table("Loft_DraftingTable", root, (2.75, 1.55, f2_z), mats, rot_z=math.pi, col=col)
    add_workshop_stool("Loft_DraftStool", root, (1.88, 1.55, f2_z), mats, height=0.52, rot_z=0.22, col=col)

    # Floor 2 Astronomical Brass Telescope aimed out West Loft Window (F2_Win_WestS at -2.65)
    add_brass_telescope("Loft_Telescope", root, (-3.15, -3.22, f2_z), mats, rot_z=(math.pi - 0.14), col=col)

    # Floor 2 Resonance & Armillary Worktable (South-East Loft, X=2.75, Y=-1.65)
    add_box("Loft_ResTable", root, (2.75, -1.65, f2_z + 0.36), (1.10, 1.90, 0.72), mats["walnut_dark"], col=col)
    add_box("Loft_ResTableTop", root, (2.75, -1.65, f2_z + 0.76), (1.20, 2.02, 0.07), mats["honey_wood"], col=col)
    z_res_top = f2_z + 0.795
    add_armillary_globe("Loft_Armillary", root, (2.75, -1.25, z_res_top), mats, scale=1.05, rot_z=0.4, col=col)
    add_brass_scale("Loft_Scale", root, (2.72, -1.98, z_res_top), mats, rot_z=0.1, col=col)
    add_oil_lamp("Loft_OilLamp", root, (2.42, -2.38, z_res_top), mats, rot_z=0.3, col=col)
    add_open_ledger_and_quill("Loft_Ledger", root, (2.75, -0.82, z_res_top), mats, rot_z=math.pi, col=col)
    add_apothecary_bottle("Loft_Vial", root, (3.02, -2.28, z_res_top), 0.24, 0.062, mats["jade_glass"], mats, stopper_mat=mats["brass_gold"], col=col)

    # Floor 2 East Back Wall Storytelling (inner face X=4.205): Clock, Sconces, Blueprints & Tea Credenza
    add_wall_clock("WallClock_F2", root, (back_x - 0.155, 0.0, 5.15), (-1.0, 0.0), mats, col=col)
    add_bracket_lantern("Loft_Sconce_S", root, (back_x - 0.155, -2.10, 5.45), (-1.0, 0.0), mats, col=col)
    add_bracket_lantern("Loft_Sconce_N", root, (back_x - 0.155, 2.10, 5.45), (-1.0, 0.0), mats, col=col)

    add_box("Loft_Credenza", root, (3.82, 0.0, f2_z + 0.36), (0.62, 1.85, 0.72), mats["honey_wood"], col=col)
    add_box("Loft_CredenzaTop", root, (3.82, 0.0, f2_z + 0.76), (0.68, 1.96, 0.07), mats["wood_dark"], col=col)
    add_tea_set("Loft_TeaSet", root, (3.82, -0.45, f2_z + 0.795), mats, rot_z=-0.2, col=col)
    add_table_lantern("Loft_Lantern", root, (3.82, 0.45, f2_z + 0.795), mats, rot_z=0.2, scale=0.92, col=col)

    # Floor 2 Window Sill Props (exact sill top Z=4.180)
    _add_potted_herb("Sill_F2_WestS", root, (front_x + 0.12, -2.65, 4.180), mats["ceramic_sage"], mats, scale=0.88, col=col)
    add_apothecary_bottle("Sill_F2_WestN", root, (front_x + 0.12, 2.65, 4.180), 0.24, 0.062, mats["amber_glass"], mats, stopper_mat=mats["brass_gold"], col=col)
    _add_potted_herb("Sill_F2_NorthE", root, (2.10, wy_n - 0.14, 4.180), mats["terracotta_roof"], mats, scale=0.90, col=col)
    return root


# ==================== 5. COMPACT SIDEWALK STALLS & TREE ====================

def build_stall_pho(mats, font, col):
    root = bpy.data.objects.new("LP_Stall_Pho", None)
    link_obj(root, col)
    txt_rot = (math.pi * 0.5, 0.0, math.pi * 0.5)
    add_box("CartBody", root, (0.0, 0.0, 0.44), (0.95, 1.85, 0.88), mats["honey_wood"], col=col)
    add_box("CartTop", root, (0.0, 0.0, 0.90), (1.06, 1.98, 0.06), mats["inox"], col=col)
    add_cyl("BrothPot", root, (0.10, -0.45, 1.14), 0.22, 0.22, 0.40, mats["inox"], verts=8, col=col)
    for py in (-0.82, 0.82):
        add_box(f"PostB_{py:.1f}", root, (-0.38, py, 1.48), (0.07, 0.07, 1.20), mats["wood_dark"], col=col)
        add_box(f"PostF_{py:.1f}", root, (0.38, py, 1.48), (0.07, 0.07, 1.20), mats["wood_dark"], col=col)
    add_striped_awning("Awning", root, (0.18, 0.0, 2.08), 1.50, 2.15, 0.22, mats["teal_shop"], mats["pure_white"], col=col)
    add_box("Sign", root, (0.50, 0.0, 2.42), (0.08, 1.25, 0.42), mats["pure_white"], col=col)
    add_3d_text_mesh("SignText", root, "PHỞ", (0.56, 0.0, 2.40), txt_rot, 0.24, mats["wood_dark"], font_data=font, col=col)
    for idx, (sx, sy, m_k) in enumerate(((0.95, -0.65, "plastic_red"), (0.95, -0.05, "plastic_blue"), (0.95, 0.55, "plastic_red"), (0.55, 1.10, "plastic_blue"))):
        add_plastic_stool(f"Stool_{idx}", root, (sx, sy, 0.0), mats[m_k], rot_z=idx * 0.4, col=col)
    return root


def build_stall_trada(mats, font, col):
    root = bpy.data.objects.new("LP_Stall_TraDa", None)
    link_obj(root, col)
    txt_rot = (math.pi * 0.5, 0.0, -math.pi * 0.5)
    add_box("CartBody", root, (0.0, 0.0, 0.44), (0.95, 1.85, 0.88), mats["honey_wood"], col=col)
    add_box("CartTop", root, (0.0, 0.0, 0.90), (1.06, 1.98, 0.06), mats["pure_white"], col=col)
    add_cyl("TeaJugA", root, (-0.10, -0.40, 1.08), 0.13, 0.11, 0.28, mats["plastic_blue"], verts=8, col=col)
    add_cyl("TeaJugB", root, (-0.10, 0.35, 1.06), 0.11, 0.11, 0.24, mats["warm_lantern"], verts=6, col=col)
    for py in (-0.82, 0.82):
        add_box(f"PostB_{py:.1f}", root, (0.38, py, 1.48), (0.07, 0.07, 1.20), mats["wood_dark"], col=col)
        add_box(f"PostF_{py:.1f}", root, (-0.38, py, 1.48), (0.07, 0.07, 1.20), mats["wood_dark"], col=col)
    add_striped_awning("Awning", root, (-0.18, 0.0, 2.08), 1.50, 2.15, -0.22, mats["crimson_plank"], mats["pure_white"], col=col)
    add_box("Sign", root, (-0.50, 0.0, 2.42), (0.08, 1.45, 0.42), mats["pure_white"], col=col)
    add_3d_text_mesh("SignText", root, "TRÀ ĐÁ", (-0.56, 0.0, 2.40), txt_rot, 0.22, mats["wood_dark"], font_data=font, col=col)
    for idx, (sx, sy, m_k) in enumerate(((-0.95, -0.65, "plastic_red"), (-0.95, -0.05, "plastic_blue"), (-0.95, 0.55, "plastic_red"), (-0.55, -1.10, "plastic_blue"))):
        add_plastic_stool(f"Stool_{idx}", root, (sx, sy, 0.0), mats[m_k], rot_z=idx * 0.5, col=col)
    return root


def build_street_tree(mats, col):
    root = bpy.data.objects.new("LP_StreetTree", None)
    link_obj(root, col)
    add_hex_planter("Planter", root, (0, 0, 0), 0.80, mats, col=col)
    add_limb("Trunk_Seg0", root, (0.0, 0.0, 0.24), (0.06, -0.04, 1.15), 0.22, 0.17, mats["tree_bark"], segments=6, col=col)
    add_limb("Trunk_Seg1", root, (0.06, -0.04, 1.10), (-0.05, 0.05, 2.05), 0.17, 0.13, mats["tree_bark"], segments=6, col=col)
    add_limb("Trunk_Seg2", root, (-0.05, 0.05, 2.00), (0.04, 0.02, 2.90), 0.13, 0.10, mats["tree_bark"], segments=6, col=col)

    for bi, (p0, p1) in enumerate((((-0.02, 0.03, 2.30), (-0.85, -0.55, 3.45)), ((0.04, 0.02, 2.48), (0.88, 0.50, 3.60)), ((0.02, 0.04, 2.55), (0.15, -0.90, 3.70)), ((-0.02, 0.02, 2.65), (-0.45, 0.85, 3.80)))):
        add_limb(f"Branch_{bi}", root, p0, p1, 0.09, 0.05, mats["tree_bark"], segments=6, col=col)

    canopies = [
        ("TreeCanopy_Center", (0.04, 0.02, 4.10), (1.42, 1.34, 1.05), mats["tree_leaf_bright"], (0.1, 0.2, 0.5)),
        ("TreeCanopy_W", (-0.90, -0.55, 3.65), (1.05, 0.98, 0.82), mats["tree_leaf_mid"], (0.3, -0.2, 1.2)),
        ("TreeCanopy_E", (0.94, 0.50, 3.75), (1.08, 1.00, 0.84), mats["tree_leaf_bright"], (-0.2, 0.3, 2.1)),
        ("TreeCanopy_S", (0.18, -0.90, 3.85), (0.95, 0.90, 0.80), mats["tree_leaf_mid"], (0.2, 0.1, 0.9)),
        ("TreeCanopy_N", (-0.42, 0.85, 3.95), (0.98, 0.92, 0.82), mats["tree_leaf_bright"], (-0.3, -0.1, 1.6)),
        ("TreeCanopy_Top", (0.0, 0.08, 4.85), (0.90, 0.88, 0.78), mats["tree_leaf_bright"], (0.15, -0.25, 2.7)),
    ]
    for cname, cloc, cscale, cmat, crot in canopies:
        add_ico(cname, root, cloc, cscale, cmat, subdivisions=1, rot=crot, jitter=0.10, col=col)
    return root


# ==================== 7. WINDING OLD QUARTER STREET PLAZA & SKYLINE ====================

def build_backdrop_house(name, parent, loc, rot_z, w_x, w_y, wall_mat, mats, col=None):
    h = bpy.data.objects.new(name, None)
    h.location = Vector(loc)
    h.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        h.parent = parent
    link_obj(h, col)

    f1_h, f2_h = 3.0, 2.5
    add_box("Base", h, (0, 0, (f1_h + f2_h) * 0.5), (w_x, w_y, f1_h + f2_h), wall_mat, col=col)
    add_box("Belt", h, (0, 0, f1_h), (w_x + 0.16, w_y + 0.16, 0.14), mats["pure_white"], col=col)
    add_pitched_terracotta_roof("Roof", h, f1_h + f2_h, w_x * 0.5, w_y * 0.5, 1.85, mats, gable_mat=wall_mat, col=col)
    for wy in (-w_y * 0.25, w_y * 0.25):
        add_window(f"W_{wy:.1f}", h, (w_x * 0.5 + 0.04, wy, f1_h + 1.20), "x", 1.05, 1.15, mats, with_shutters=True, col=col)
    add_box("DoorTrim", h, (w_x * 0.5 + 0.04, 0.0, 1.15), (0.10, 1.50, 2.30), mats["pure_white"], col=col)
    add_box("DoorFill", h, (w_x * 0.5 + 0.06, 0.0, 1.10), (0.08, 1.20, 2.10), mats["wood_dark"], col=col)
    return h


def build_skyline_tree(name, parent, loc, sc, mats, col=None):
    t = bpy.data.objects.new(name, None)
    t.location = Vector(loc)
    t.scale = Vector((sc, sc, sc))
    t.parent = parent
    link_obj(t, col)
    add_cyl("Trunk", t, (0, 0, 1.4), 0.22, 0.14, 2.8, mats["tree_bark"], verts=6, col=col)
    add_ico("CrownA", t, (0, 0, 3.8), (1.7, 1.6, 1.3), mats["tree_leaf_mid"], subdivisions=1, jitter=0.10, col=col)
    add_ico("CrownB", t, (0.6, 0.4, 3.3), (1.1, 1.0, 0.9), mats["tree_leaf_bright"], subdivisions=1, jitter=0.08, col=col)
    return t


def build_street_plaza(mats, col):
    """
    Matches overview 1:1 with natural gently curving far lane (zero dead-end U-wall!):
      - Main lane X=-4..+4, Y=8.5..33 curves smoothly Westward from Y=33..54
      - Wide foreground East-West crossroads Y=-4..10
      - Non-overlapping raised cream sidewalks & curbs (zero dark corner overlap!)
      - Foreground diagonal rocky planter & grass tuft islands matching overview bottom-center
      - 4 festive red/yellow lantern strings, angled curving shophouses, dense tree skyline & 3 clouds.
    """
    root = bpy.data.objects.new("LP_StreetPlaza", None)
    link_obj(root, col)
    rng = random.Random(20261123)

    add_box("OuterLawnApron", root, (0.0, 24.0, -0.14), (120.0, 120.0, 0.24), mats["ground_skirt"], col=col)
    add_box("CobbleBed_Cross", root, (0.0, 2.5, -0.044), (52.0, 14.0, 0.08), mats["cobble_dark"], col=col)
    add_box("CobbleBed_Main", root, (0.0, 22.0, -0.042), (8.4, 28.0, 0.08), mats["cobble_dark"], col=col)

    # Gentle curved cobblestone road continuation for Y = 36..55 (offset below main bed to prevent Z-fighting)
    for seg_i in range(9):
        u0 = (seg_i + 0.5) / 9.0
        yc = 37.0 + seg_i * 2.1
        cx = -4.0 * (u0 ** 1.55)
        yaw = math.atan2(-4.0 * 1.55 * (u0 ** 0.55), 19.0) * 0.9
        add_box(f"CurveRoad_{seg_i}", root, (cx, yc, -0.046), (8.6, 2.35, 0.08), mats["cobble_dark"], rot=(0, 0, -yaw), col=col)

    # Non-overlapping cobblestone pavers: Crossroads + Main Straight + Winding Bend
    for ry_i in range(12):
        yc = -3.0 + ry_i * 1.02
        shift = 0.50 if (ry_i % 2 == 1) else 0.0
        for cx_i in range(28):
            xc = -16.0 + cx_i * 1.18 + shift
            if xc > 16.2:
                xc -= 33.04
            pw = rng.uniform(0.84, 0.96)
            pd = rng.uniform(0.76, 0.86)
            pz = 0.003 + 0.0012 * ((cx_i + ry_i) % 3)
            pmat = mats["cobble_light"] if (ry_i + cx_i) % 3 != 0 else mats["cobble_dark"]
            add_box(f"CrossPaver_{ry_i}_{cx_i}", root, (xc, yc, pz), (pw, pd, 0.010), pmat, col=col)

    for ry_i in range(44):
        yc = 9.0 + ry_i * 0.98
        u = max(0.0, (yc - 33.0) / 22.0)
        x_off = -4.2 * (u ** 1.55) if yc > 33.0 else 0.0
        yaw = -math.atan2(-4.2 * 1.55 * (max(0.05, u) ** 0.55), 22.0) * 0.8 if yc > 33.0 else 0.0
        shift = 0.48 if (ry_i % 2 == 1) else 0.0
        for cx_i in range(7):
            lx = -3.30 + cx_i * 1.10 + shift
            if lx > 3.55:
                lx -= 7.70
            pw = rng.uniform(0.82, 0.94)
            pd = rng.uniform(0.72, 0.82)
            pz = 0.004 + 0.0012 * ((cx_i + ry_i) % 3)
            pmat = mats["cobble_light"] if (ry_i + cx_i) % 3 != 0 else mats["cobble_dark"]
            add_box(f"LanePaver_{ry_i}_{cx_i}", root, (x_off + lx, yc, pz), (pw, pd, 0.010), pmat, rot=(0, 0, yaw), col=col)

    # Raised Cream Sidewalks & Non-Overlapping Curbs (Curb_EW cleanly stops short of Curb_NS!)
    for s_sgn, sname in ((-1.0, "W"), (1.0, "E")):
        add_box(f"Sidewalk_{sname}", root, (s_sgn * 13.0, 23.0, 0.04), (18.0, 29.0, 0.08), mats["sidewalk_cream"], col=col)
        add_box(f"Curb_NS_{sname}", root, (s_sgn * 4.15, 23.0, 0.045), (0.34, 29.0, 0.09), mats["curb_stone"], col=col)
        add_box(f"Curb_EW_{sname}", root, (s_sgn * 13.16, 8.67, 0.045), (17.64, 0.34, 0.09), mats["curb_stone"], col=col)
        for gi in range(14):
            gy = 9.8 + gi * 1.9
            add_box(f"Groove_{sname}_{gi}", root, (s_sgn * 5.6, gy, 0.082), (3.0, 0.05, 0.008), mats["curb_stone"], col=col)
    # Upper curved sidewalks behind Y=37 (offset Z slightly below main sidewalk top so zero Z-fighting!)
    add_box("Sidewalk_CurveW", root, (-14.5, 45.5, 0.036), (16.0, 18.0, 0.08), mats["sidewalk_cream"], rot=(0, 0, 0.24), col=col)
    add_box("Sidewalk_CurveE", root, (10.5, 44.5, 0.036), (16.0, 18.0, 0.08), mats["sidewalk_cream"], rot=(0, 0, 0.24), col=col)

    # Foreground Diagonal Rocky Planter Island + Grass Cluster Pad (matching overview bottom-center!)
    add_box("FG_Island_Pad", root, (-0.3, 5.2, 0.05), (2.2, 2.7, 0.10), mats["sidewalk_cream"], col=col)
    add_hex_planter("FG_Island_Hex", root, (-0.3, 5.4, 0.09), 0.92, mats, col=col)
    add_box("FG_Tuft_Pad", root, (-1.2, 2.6, 0.05), (1.6, 1.6, 0.10), mats["sidewalk_cream"], col=col)
    add_grass_tuft("FG_Tuft_Grass", root, (-1.2, 2.6, 0.09), 1.15, mats, col=col)

    # 4 Festive Red & Yellow Lantern Strings across the winding street (matching overview!)
    for gi, gy in enumerate((29.5, 32.5, 35.5, 38.5)):
        u = max(0.0, (gy - 33.0) / 22.0)
        x_off = -4.2 * (u ** 1.55) if gy > 33.0 else 0.0
        g_obj = bpy.data.objects.new(f"LanternGarland_{gi}", None)
        g_obj.location = Vector((x_off, gy, 5.25))
        g_obj.parent = root
        link_obj(g_obj, col)
        pts = [(-5.0 + i * (10.0 / 8.0), 0.0, -0.52 * math.sin(math.pi * i / 8.0)) for i in range(9)]
        for i in range(8):
            add_limb(f"Wire_{i}", g_obj, pts[i], pts[i + 1], 0.015, 0.015, mats["charcoal"], segments=4, col=col)
        for i in range(1, 8):
            lm = mats["lantern_red"] if (i + gi) % 2 == 0 else mats["lantern_yellow"]
            add_lowpoly_lantern(f"Lan_{i}", g_obj, (pts[i][0], pts[i][1], pts[i][2] - 0.18), lm, mats, col=col)

    # Curving Multi-Angle Old Quarter Terrace Houses (zero flat wall block-off!)
    backdrop_specs = [
        ("BD_Curve_W1", (-11.8, 39.4, 0.0), 0.16, 8.2, 7.6, mats["sunny_yellow"]),
        ("BD_Curve_W2", (-13.6, 46.8, 0.0), 0.30, 8.0, 7.6, mats["warm_cream"]),
        ("BD_Curve_E1", (10.0, 38.6, 0.0), math.pi + 0.18, 8.0, 6.8, mats["warm_cream"]),
        ("BD_Curve_E2", (7.5, 45.8, 0.0), math.pi + 0.34, 8.0, 6.8, mats["crimson_plank"]),
        ("BD_Far_End1", (-5.5, 53.5, 0.0), -math.pi * 0.5 + 0.35, 7.2, 8.5, mats["sunny_yellow"]),
        ("BD_Far_End2", (2.5, 51.8, 0.0), -math.pi * 0.5 + 0.25, 7.2, 8.2, mats["teal_shop"]),
        # Outer skyline depth layers matching overview left & right clusters
        ("BD_Back_W1", (-20.8, 16.5, 0.0), 0.0, 7.4, 9.0, mats["warm_cream"]),
        ("BD_Back_W2", (-20.8, 26.5, 0.0), 0.0, 7.4, 9.2, mats["teal_shop"]),
        ("BD_Back_W3", (-21.4, 37.0, 0.0), 0.15, 7.4, 9.2, mats["crimson_plank"]),
        ("BD_Back_E1", (20.8, 16.5, 0.0), math.pi, 7.4, 9.0, mats["sunny_yellow"]),
        ("BD_Back_E2", (20.8, 26.5, 0.0), math.pi, 7.4, 9.2, mats["indigo_shop"]),
        ("BD_Back_E3", (19.6, 36.8, 0.0), math.pi + 0.18, 7.4, 9.2, mats["mint_shop"]),
        ("BD_South_Spawn", (0.0, -14.5, 0.0), math.pi * 0.5, 7.0, 16.0, mats["warm_cream"]),
    ]
    for bname, bloc, brot, bwx, bwy, bmat in backdrop_specs:
        build_backdrop_house(bname, root, bloc, brot, bwx, bwy, bmat, mats, col=col)

    # Lush Skyline Trees positioned cleanly outside all building footprints (zero wall/roof penetration!)
    skyline_trees = [
        (-26.8, 16.0, 1.30), (-26.5, 27.5, 1.40), (-26.0, 39.0, 1.35),
        (-21.5, 51.0, 1.40), (-12.5, 57.5, 1.45), (-3.0, 59.0, 1.40), (7.5, 57.5, 1.45),
        (17.5, 51.0, 1.40), (26.0, 40.5, 1.35), (26.8, 29.0, 1.35), (26.8, 17.5, 1.30),
        (18.5, 9.5, 1.25), (-20.0, 9.5, 1.20), (14.5, 46.5, 1.25), (-15.5, 49.5, 1.25),
    ]
    for idx, (tx, ty, tsc) in enumerate(skyline_trees):
        build_skyline_tree(f"SkyTree_{idx}", root, (tx, ty, 0.0), tsc, mats, col=col)

    # 3 Chunky White Low-Poly Clouds framing the upper sky
    for cname, cloc, csc in (("Cloud_L", (-14.0, 52.0, 18.5), 1.45), ("Cloud_C", (1.0, 55.0, 20.5), 1.65), ("Cloud_R", (15.0, 50.0, 19.0), 1.50)):
        c_obj = bpy.data.objects.new(cname, None)
        c_obj.location = Vector(cloc)
        c_obj.scale = Vector((csc, csc, csc))
        c_obj.parent = root
        link_obj(c_obj, col)
        add_ico("Puff_C", c_obj, (0, 0, 0), (2.2, 1.4, 1.2), mats["cloud_white"], subdivisions=1, col=col)
        add_ico("Puff_L", c_obj, (-1.8, 0.2, -0.2), (1.4, 1.1, 0.9), mats["cloud_white"], subdivisions=1, col=col)
        add_ico("Puff_R", c_obj, (1.8, -0.1, -0.15), (1.5, 1.1, 0.95), mats["cloud_white"], subdivisions=1, col=col)

    return root


# ==================== EXPORT & CYCLES PREVIEW ====================

def _flatten_for_fbx(root):
    """
    Flattens all nested Empty containers so `root` contains strictly 1-level direct MESH children
    with zero rotation/scale distortion under Blender's bake_space_transform FBX exporter.
    Joins animated gear assemblies by exact name so MarketAmbientAnimator still animates them in Unity.
    """
    bpy.context.view_layer.update()

    gear_names = {
        "FloorPit_GearA", "FloorPit_GearB", "FloorPit_CyanConduit",
        "Wall_Gear_1", "Wall_Gear_2", "Ext_GableGear_Big", "CraftGearA", "CraftGearB",
    }
    gear_empties = [o for o in root.children_recursive if o.type == "EMPTY" and o.name in gear_names]
    for o in gear_empties:
        meshes = [c for c in o.children_recursive if c.type == "MESH"]
        if not meshes:
            continue
        pivot_world = o.matrix_world.to_translation().copy()
        bpy.ops.object.select_all(action="DESELECT")
        for m in meshes:
            m.select_set(True)
        bpy.context.view_layer.objects.active = meshes[0]
        if len(meshes) > 1:
            bpy.ops.object.join()
        joined = bpy.context.view_layer.objects.active
        mw = joined.matrix_world.copy()
        joined.parent = root
        joined.matrix_parent_inverse = root.matrix_world.inverted()
        joined.matrix_world = mw
        bpy.ops.object.select_all(action="DESELECT")
        joined.select_set(True)
        bpy.context.view_layer.objects.active = joined
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        delta = joined.location - pivot_world
        if delta.length > 1e-5:
            for v in joined.data.vertices:
                v.co += delta
            joined.location = pivot_world
        joined.name = o.name + "_TMP"
        bpy.data.objects.remove(o, do_unlink=True)
        joined.name = joined.name[:-4]

    bpy.context.view_layer.update()

    # Re-parent all remaining descendant meshes directly to root and apply their rot/scale
    for o in list(root.children_recursive):
        if o.type != "MESH":
            continue
        mw = o.matrix_world.copy()
        if o.parent != root:
            o.parent = root
            o.matrix_parent_inverse = root.matrix_world.inverted()
            o.matrix_world = mw
        if tuple(round(v, 5) for v in o.rotation_euler) != (0.0, 0.0, 0.0) or tuple(round(v, 5) for v in o.scale) != (1.0, 1.0, 1.0):
            bpy.ops.object.select_all(action="DESELECT")
            o.select_set(True)
            bpy.context.view_layer.objects.active = o
            bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)

    # Remove all now-empty child EMPTY nodes
    for o in list(root.children_recursive):
        if o.type == "EMPTY":
            bpy.data.objects.remove(o, do_unlink=True)


def export_fbx(root, filename):
    _flatten_for_fbx(root)
    os.makedirs(MODEL_DIR, exist_ok=True)
    objs = [root] + list(root.children_recursive)
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = root

    out_path = os.path.join(MODEL_DIR, filename)
    tmp_path = os.path.join(tempfile.gettempdir(), f"chodola_lp_{filename}")
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
        use_mesh_modifiers=False,
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
    print("EXPORTED_LP_FBX:", out_path)


def export_all_lowpoly_assets(only_files=None):
    builders = [
        ("LP_Shop_NhaNiemPhong.fbx", lambda m, f, c: build_nha_niem_phong(m, f, c)),
        ("LP_Shop_TiemMoHom.fbx", lambda m, f, c: build_tiem_mo_hom(m, f, c)),
        ("LP_Shop_AnhBay.fbx", lambda m, f, c: build_anh_bay(m, f, c)),
        ("LP_Shop_KimKhuy.fbx", lambda m, f, c: build_kim_khuy(m, f, c)),
        ("LP_Stall_Pho.fbx", lambda m, f, c: build_stall_pho(m, f, c)),
        ("LP_Stall_TraDa.fbx", lambda m, f, c: build_stall_trada(m, f, c)),
        ("LP_StreetTree.fbx", lambda m, f, c: build_street_tree(m, c)),
        ("LP_StreetPlaza.fbx", lambda m, f, c: build_street_plaza(m, c)),
    ]

    only_set = set(only_files) if only_files else None
    for fname, fn in builders:
        if only_set is not None and fname not in only_set:
            continue
        clear_scene()
        font = load_unicode_font()
        col = get_or_create_collection("Export")
        mats = create_lp_materials()
        root = fn(mats, font, col)
        export_fbx(root, fname)


def render_cam(name, loc, target, lens_mm, out_path):
    cam_data = bpy.data.cameras.new(name + "_Data")
    cam_data.lens = lens_mm
    cam_data.clip_end = 400.0
    cam_obj = bpy.data.objects.new(name, cam_data)
    cam_obj.location = Vector(loc)
    direction = Vector(target) - Vector(loc)
    cam_obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    bpy.context.scene.collection.objects.link(cam_obj)
    bpy.context.scene.camera = cam_obj
    bpy.context.scene.render.filepath = out_path
    bpy.ops.render.render(write_still=True)
    print("RENDERED_LP_PREVIEW:", out_path)


def render_full_map_verification_previews():
    os.makedirs(CONCEPT_DIR, exist_ok=True)
    clear_scene()
    col = get_or_create_collection("Preview")
    mats = create_lp_materials()
    font = load_unicode_font()

    build_street_plaza(mats, col)

    s1 = build_nha_niem_phong(mats, font, col)
    s1.location = Vector((-12.0, 18.0, 0.0))

    s2 = build_tiem_mo_hom(mats, font, col)
    s2.location = Vector((-11.5, 29.3, 0.0))

    st_pho = build_stall_pho(mats, font, col)
    st_pho.location = Vector((-5.6, 25.4, 0.08))

    s3 = build_anh_bay(mats, font, col)
    s3.location = Vector((11.5, 18.0, 0.0))

    st_trada = build_stall_trada(mats, font, col)
    st_trada.location = Vector((5.6, 14.5, 0.08))

    s4 = build_kim_khuy(mats, font, col)
    s4.location = Vector((11.5, 28.2, 0.0))

    for idx, tpos in enumerate(((-9.2, 9.8, 0.08), (6.4, 9.8, 0.08), (12.2, 10.2, 0.08))):
        t = build_street_tree(mats, col)
        t.name = f"FG_Tree_{idx}"
        t.location = Vector(tpos)
        t.rotation_euler = Euler((0, 0, idx * 0.9))

    # Cool blue sky shadows + balanced non-glaring golden sun + AgX tone mapping
    world = bpy.data.worlds.new("LP_DaySky")
    world.use_nodes = True
    bpy.context.scene.world = world
    bg = world.node_tree.nodes.get("Background")
    if bg:
        bg.inputs["Color"].default_value = (0.18, 0.58, 0.96, 1.0)
        bg.inputs["Strength"].default_value = 0.85

    sun_data = bpy.data.lights.new("Sun_LowPolyDay", type="SUN")
    sun_data.color = (1.0, 0.92, 0.78)
    sun_data.energy = 2.8
    sun_data.angle = math.radians(1.5)
    sun_obj = bpy.data.objects.new("Sun_LowPolyDay", sun_data)
    sun_obj.rotation_euler = Euler((math.radians(42), math.radians(-28), math.radians(-40)))
    col.objects.link(sun_obj)

    # Gentle warm interior lantern pools across cathedral heights (zero blowout!)
    for lx, ly, lz, e in (
        (-11.5, 18.0, 4.6, 80.0),
        (-15.0, 18.0, 2.8, 50.0),
        (-11.5, 29.3, 4.4, 70.0),
        (-12.2, 28.1, 2.2, 42.0),
        (-14.8, 29.3, 2.4, 48.0),
    ):
        pl = bpy.data.lights.new("PreviewIntLight", type="POINT")
        pl.color = (1.0, 0.86, 0.62)
        pl.energy = e
        pl.shadow_soft_size = 0.55
        po = bpy.data.objects.new("PreviewIntLight", pl)
        po.location = Vector((lx, ly, lz))
        col.objects.link(po)

    scene = bpy.context.scene
    scene.view_settings.view_transform = "AgX"
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.view_settings.exposure = -0.45
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 28
    scene.cycles.use_denoising = True
    scene.render.resolution_x = 1280
    scene.render.resolution_y = 720
    scene.render.image_settings.file_format = "PNG"

    # 1. Overview matching concept_lowpoly_map_overview_birdseye_1795435659758.jpg
    render_cam(
        "Cam_Overview",
        (3.8, -5.8, 12.6),
        (-1.2, 22.0, 2.8),
        22.0,
        os.path.join(CONCEPT_DIR, "render_lowpoly_map_overview.png"),
    )
    # 2. Street First-Person View looking toward Nhà Niêm Phong, Tiệm Mở Hòm & Phở
    render_cam(
        "Cam_StreetFP",
        (2.4, 9.8, 1.75),
        (-7.5, 22.5, 2.4),
        25.0,
        os.path.join(CONCEPT_DIR, "render_lowpoly_street_fp.png"),
    )
    # 3. Inside Nhà Niêm Phong (Honeycomb Floor, Mezzanine, Trusses, Suspended Crate, Pedestals & 18-Cubby Relic Wall)
    render_cam(
        "Cam_NhaNiemPhong_Int",
        (-7.3, 18.0, 1.95),
        (-15.5, 18.0, 2.95),
        19.0,
        os.path.join(CONCEPT_DIR, "render_lowpoly_nha_niem_phong_int.png"),
    )
    # 4. Inside Tiệm Mở Hòm (Clockwork Atelier: Slate Floor, Teal Wainscot, Overhead Gears & Skylight, Pegboard, Alchemy Rack & Workbench)
    render_cam(
        "Cam_TiemMoHom_Int",
        (-7.2, 30.8, 1.95),
        (-12.6, 28.6, 2.45),
        19.0,
        os.path.join(CONCEPT_DIR, "render_lowpoly_tiem_mo_hom.png"),
    )


if __name__ == "__main__":
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    only_files = []
    no_render = False
    i = 0
    while i < len(argv):
        arg = argv[i]
        if arg == "--only" and i + 1 < len(argv):
            only_files.extend([x.strip() for x in argv[i + 1].split(",") if x.strip()])
            i += 2
        elif arg.startswith("--only="):
            only_files.extend([x.strip() for x in arg.split("=", 1)[1].split(",") if x.strip()])
            i += 1
        elif arg == "--no-render":
            no_render = True
            i += 1
        else:
            i += 1
    export_all_lowpoly_assets(only_files=only_files or None)
    if not no_render:
        render_full_map_verification_previews()

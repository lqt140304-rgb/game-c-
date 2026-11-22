import math
import os
import random
import bpy
import bmesh
from mathutils import Vector, Euler

OUTPUT_DIR = r"C:\Users\Admin\.gemini\antigravity\brain\a18bdc9f-a36c-45a5-9fe0-70920c9a3002"
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
    col.objects.link(obj)


# ==================== PROCEDURAL PBR MATERIAL BUILDERS ====================

def _new_bsdf_mat(name):
    if name in bpy.data.materials:
        return bpy.data.materials[name], None, None
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nt = mat.node_tree
    bsdf = nt.nodes.get("Principled BSDF")
    return mat, nt, bsdf


def _wall_uv_node(nt):
    """Projects world XYZ onto (X+Y, Z, 0) so brick/stone courses align in meters on all wall orientations."""
    geom = nt.nodes.new("ShaderNodeNewGeometry")
    sep = nt.nodes.new("ShaderNodeSeparateXYZ")
    add = nt.nodes.new("ShaderNodeMath")
    add.operation = "ADD"
    comb = nt.nodes.new("ShaderNodeCombineXYZ")
    nt.links.new(geom.outputs["Position"], sep.inputs["Vector"])
    nt.links.new(sep.outputs["X"], add.inputs[0])
    nt.links.new(sep.outputs["Y"], add.inputs[1])
    nt.links.new(add.outputs["Value"], comb.inputs["X"])
    nt.links.new(sep.outputs["Z"], comb.inputs["Y"])
    return geom, sep, comb


def make_simple_mat(name, color, roughness=0.55, metallic=0.0, emission=None, emission_strength=0.0):
    mat, _, bsdf = _new_bsdf_mat(name)
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (*color, 1.0)
        bsdf.inputs["Roughness"].default_value = roughness
        bsdf.inputs["Metallic"].default_value = metallic
        if emission is not None and "Emission Color" in bsdf.inputs:
            bsdf.inputs["Emission Color"].default_value = (*emission, 1.0)
            bsdf.inputs["Emission Strength"].default_value = emission_strength
    return mat


def make_cobble_pbr():
    mat, nt, bsdf = _new_bsdf_mat("Mat_Cobble_PBR")
    if nt is None:
        return mat
    geom = nt.nodes.new("ShaderNodeNewGeometry")

    vor_edge = nt.nodes.new("ShaderNodeTexVoronoi")
    vor_edge.feature = "DISTANCE_TO_EDGE"
    vor_edge.inputs["Scale"].default_value = 3.2
    vor_edge.inputs["Randomness"].default_value = 0.78

    vor_col = nt.nodes.new("ShaderNodeTexVoronoi")
    vor_col.feature = "F1"
    vor_col.inputs["Scale"].default_value = 3.2
    vor_col.inputs["Randomness"].default_value = 0.78

    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 2.8
    noise.inputs["Detail"].default_value = 7.0

    ramp_grout = nt.nodes.new("ShaderNodeValToRGB")
    ramp_grout.color_ramp.elements[0].position = 0.0
    ramp_grout.color_ramp.elements[0].color = (0.0, 0.0, 0.0, 1.0)
    ramp_grout.color_ramp.elements[1].position = 0.10
    ramp_grout.color_ramp.elements[1].color = (1.0, 1.0, 1.0, 1.0)

    ramp_stone = nt.nodes.new("ShaderNodeValToRGB")
    ramp_stone.color_ramp.elements[0].position = 0.12
    ramp_stone.color_ramp.elements[0].color = (0.16, 0.15, 0.14, 1.0)
    ramp_stone.color_ramp.elements[1].position = 0.88
    ramp_stone.color_ramp.elements[1].color = (0.34, 0.31, 0.27, 1.0)

    mix_col = nt.nodes.new("ShaderNodeMix")
    mix_col.data_type = "RGBA"
    mix_col.inputs[6].default_value = (0.09, 0.13, 0.07, 1.0)  # deep mossy grout

    # Weathering grime patches across the street
    ramp_dirt = nt.nodes.new("ShaderNodeValToRGB")
    ramp_dirt.color_ramp.elements[0].position = 0.30
    ramp_dirt.color_ramp.elements[0].color = (1.0, 1.0, 1.0, 1.0)
    ramp_dirt.color_ramp.elements[1].position = 0.80
    ramp_dirt.color_ramp.elements[1].color = (0.55, 0.52, 0.45, 1.0)

    mix_dirt = nt.nodes.new("ShaderNodeMix")
    mix_dirt.data_type = "RGBA"
    mix_dirt.blend_type = "MULTIPLY"
    mix_dirt.inputs[0].default_value = 0.80

    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.55
    bump.inputs["Distance"].default_value = 0.035

    nt.links.new(geom.outputs["Position"], vor_edge.inputs["Vector"])
    nt.links.new(geom.outputs["Position"], vor_col.inputs["Vector"])
    nt.links.new(geom.outputs["Position"], noise.inputs["Vector"])
    nt.links.new(vor_edge.outputs["Distance"], ramp_grout.inputs["Fac"])
    nt.links.new(vor_col.outputs["Distance"], ramp_stone.inputs["Fac"])
    nt.links.new(ramp_grout.outputs["Color"], mix_col.inputs[0])
    nt.links.new(ramp_stone.outputs["Color"], mix_col.inputs[7])
    nt.links.new(noise.outputs["Fac"], ramp_dirt.inputs["Fac"])
    nt.links.new(mix_col.outputs[2], mix_dirt.inputs[6])
    nt.links.new(ramp_dirt.outputs["Color"], mix_dirt.inputs[7])
    nt.links.new(mix_dirt.outputs[2], bsdf.inputs["Base Color"])
    nt.links.new(ramp_grout.outputs["Color"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    bsdf.inputs["Roughness"].default_value = 0.78
    return mat


def make_weathered_plaster(name, base_col, stain_col):
    """Heavily weathered plaster with both rising ground damp/moss AND dark soot/rain streaks under eaves."""
    mat, nt, bsdf = _new_bsdf_mat(name)
    if nt is None:
        return mat
    geom, sep, _ = _wall_uv_node(nt)

    noise_big = nt.nodes.new("ShaderNodeTexNoise")
    noise_big.inputs["Scale"].default_value = 1.15
    noise_big.inputs["Detail"].default_value = 6.0
    noise_big.inputs["Roughness"].default_value = 0.72

    # Height profile: damp at bottom (Z<1.3m) and soot/rain stains near top eaves (Z>2.6m)
    map_z = nt.nodes.new("ShaderNodeMapRange")
    map_z.inputs["From Min"].default_value = 0.0
    map_z.inputs["From Max"].default_value = 3.5

    ramp_z = nt.nodes.new("ShaderNodeValToRGB")
    e0 = ramp_z.color_ramp.elements[0]
    e0.position = 0.08
    e0.color = (1.0, 1.0, 1.0, 1.0)
    e_mid = ramp_z.color_ramp.elements.new(0.45)
    e_mid.color = (0.35, 0.35, 0.35, 1.0)
    e1 = ramp_z.color_ramp.elements[1]
    e1.position = 0.88
    e1.color = (0.85, 0.85, 0.85, 1.0)

    mult = nt.nodes.new("ShaderNodeMath")
    mult.operation = "MULTIPLY"

    ramp_stain = nt.nodes.new("ShaderNodeValToRGB")
    ramp_stain.color_ramp.elements[0].position = 0.16
    ramp_stain.color_ramp.elements[0].color = (0.0, 0.0, 0.0, 1.0)
    ramp_stain.color_ramp.elements[1].position = 0.56
    ramp_stain.color_ramp.elements[1].color = (1.0, 1.0, 1.0, 1.0)

    mix_col = nt.nodes.new("ShaderNodeMix")
    mix_col.data_type = "RGBA"
    mix_col.inputs[6].default_value = (*base_col, 1.0)
    mix_col.inputs[7].default_value = (*stain_col, 1.0)

    noise_fine = nt.nodes.new("ShaderNodeTexNoise")
    noise_fine.inputs["Scale"].default_value = 14.0
    noise_fine.inputs["Detail"].default_value = 8.0
    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.28
    bump.inputs["Distance"].default_value = 0.018

    nt.links.new(geom.outputs["Position"], noise_big.inputs["Vector"])
    nt.links.new(geom.outputs["Position"], noise_fine.inputs["Vector"])
    nt.links.new(sep.outputs["Z"], map_z.inputs["Value"])
    nt.links.new(map_z.outputs["Result"], ramp_z.inputs["Fac"])
    nt.links.new(ramp_z.outputs["Color"], mult.inputs[0])
    nt.links.new(noise_big.outputs["Fac"], mult.inputs[1])
    nt.links.new(mult.outputs["Value"], ramp_stain.inputs["Fac"])
    nt.links.new(ramp_stain.outputs["Color"], mix_col.inputs[0])
    nt.links.new(mix_col.outputs[2], bsdf.inputs["Base Color"])
    nt.links.new(noise_fine.outputs["Fac"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    bsdf.inputs["Roughness"].default_value = 0.88
    return mat


def make_brick_pbr(name, col1, col2, mortar_col, scale=2.2, bw=0.45, rh=0.20, mortar_sz=0.022, brick_rough=0.72, bump_str=0.50):
    mat, nt, bsdf = _new_bsdf_mat(name)
    if nt is None:
        return mat
    geom, _, comb = _wall_uv_node(nt)

    brick = nt.nodes.new("ShaderNodeTexBrick")
    brick.inputs["Color1"].default_value = (*col1, 1.0)
    brick.inputs["Color2"].default_value = (*col2, 1.0)
    brick.inputs["Mortar"].default_value = (*mortar_col, 1.0)
    brick.inputs["Scale"].default_value = scale
    brick.inputs["Brick Width"].default_value = bw
    brick.inputs["Row Height"].default_value = rh
    brick.inputs["Mortar Size"].default_value = mortar_sz
    brick.inputs["Mortar Smooth"].default_value = 0.18

    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 1.4
    noise.inputs["Detail"].default_value = 6.0

    ramp_grime = nt.nodes.new("ShaderNodeValToRGB")
    ramp_grime.color_ramp.elements[0].position = 0.28
    ramp_grime.color_ramp.elements[0].color = (1.0, 1.0, 1.0, 1.0)
    ramp_grime.color_ramp.elements[1].position = 0.78
    ramp_grime.color_ramp.elements[1].color = (0.48, 0.45, 0.40, 1.0)

    mix_grime = nt.nodes.new("ShaderNodeMix")
    mix_grime.data_type = "RGBA"
    mix_grime.blend_type = "MULTIPLY"
    mix_grime.inputs[0].default_value = 0.88

    rough_map = nt.nodes.new("ShaderNodeMapRange")
    rough_map.inputs["From Min"].default_value = 0.0
    rough_map.inputs["From Max"].default_value = 1.0
    rough_map.inputs["To Min"].default_value = brick_rough
    rough_map.inputs["To Max"].default_value = 0.92

    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = bump_str
    bump.inputs["Distance"].default_value = 0.022
    bump.invert = True

    nt.links.new(comb.outputs["Vector"], brick.inputs["Vector"])
    nt.links.new(geom.outputs["Position"], noise.inputs["Vector"])
    nt.links.new(noise.outputs["Fac"], ramp_grime.inputs["Fac"])
    nt.links.new(brick.outputs["Color"], mix_grime.inputs[6])
    nt.links.new(ramp_grime.outputs["Color"], mix_grime.inputs[7])
    nt.links.new(mix_grime.outputs[2], bsdf.inputs["Base Color"])
    nt.links.new(brick.outputs["Fac"], rough_map.inputs["Value"])
    nt.links.new(rough_map.outputs["Result"], bsdf.inputs["Roughness"])
    nt.links.new(brick.outputs["Fac"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    return mat


def make_wood_pbr(name, col_a, col_b, roughness=0.62, bump_str=0.24):
    mat, nt, bsdf = _new_bsdf_mat(name)
    if nt is None:
        return mat
    geom = nt.nodes.new("ShaderNodeNewGeometry")
    mapping = nt.nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value = (1.5, 9.5, 9.5)

    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 3.2
    noise.inputs["Detail"].default_value = 7.0
    noise.inputs["Distortion"].default_value = 1.8

    ramp = nt.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].position = 0.25
    ramp.color_ramp.elements[0].color = (*col_a, 1.0)
    ramp.color_ramp.elements[1].position = 0.75
    ramp.color_ramp.elements[1].color = (*col_b, 1.0)

    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = bump_str
    bump.inputs["Distance"].default_value = 0.012

    nt.links.new(geom.outputs["Position"], mapping.inputs["Vector"])
    nt.links.new(mapping.outputs["Vector"], noise.inputs["Vector"])
    nt.links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
    nt.links.new(ramp.outputs["Color"], bsdf.inputs["Base Color"])
    nt.links.new(noise.outputs["Fac"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    bsdf.inputs["Roughness"].default_value = roughness
    return mat


def make_workshop_slab_floor():
    """Weathered industrial slate/stone slabs with oil stains & cracks (replaces clean checkerboard)."""
    mat, nt, bsdf = _new_bsdf_mat("Mat_WorkshopSlab_Floor")
    if nt is None:
        return mat
    geom = nt.nodes.new("ShaderNodeNewGeometry")

    vor = nt.nodes.new("ShaderNodeTexVoronoi")
    vor.feature = "DISTANCE_TO_EDGE"
    vor.inputs["Scale"].default_value = 2.1
    vor.inputs["Randomness"].default_value = 0.35

    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 1.9
    noise.inputs["Detail"].default_value = 7.0

    ramp_seam = nt.nodes.new("ShaderNodeValToRGB")
    ramp_seam.color_ramp.elements[0].position = 0.0
    ramp_seam.color_ramp.elements[0].color = (0.0, 0.0, 0.0, 1.0)
    ramp_seam.color_ramp.elements[1].position = 0.06
    ramp_seam.color_ramp.elements[1].color = (1.0, 1.0, 1.0, 1.0)

    ramp_oil = nt.nodes.new("ShaderNodeValToRGB")
    ramp_oil.color_ramp.elements[0].position = 0.25
    ramp_oil.color_ramp.elements[0].color = (0.14, 0.16, 0.17, 1.0)
    ramp_oil.color_ramp.elements[1].position = 0.78
    ramp_oil.color_ramp.elements[1].color = (0.34, 0.32, 0.29, 1.0)

    mix = nt.nodes.new("ShaderNodeMix")
    mix.data_type = "RGBA"
    mix.inputs[6].default_value = (0.07, 0.08, 0.07, 1.0)

    rough_map = nt.nodes.new("ShaderNodeMapRange")
    rough_map.inputs["From Min"].default_value = 0.25
    rough_map.inputs["From Max"].default_value = 0.75
    rough_map.inputs["To Min"].default_value = 0.22  # oily glossy sheen
    rough_map.inputs["To Max"].default_value = 0.80

    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.42
    bump.inputs["Distance"].default_value = 0.02

    nt.links.new(geom.outputs["Position"], vor.inputs["Vector"])
    nt.links.new(geom.outputs["Position"], noise.inputs["Vector"])
    nt.links.new(vor.outputs["Distance"], ramp_seam.inputs["Fac"])
    nt.links.new(noise.outputs["Fac"], ramp_oil.inputs["Fac"])
    nt.links.new(ramp_seam.outputs["Color"], mix.inputs[0])
    nt.links.new(ramp_oil.outputs["Color"], mix.inputs[7])
    nt.links.new(mix.outputs[2], bsdf.inputs["Base Color"])
    nt.links.new(noise.outputs["Fac"], rough_map.inputs["Value"])
    nt.links.new(rough_map.outputs["Result"], bsdf.inputs["Roughness"])
    nt.links.new(ramp_seam.outputs["Color"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    return mat


def make_moss_clump():
    mat, nt, bsdf = _new_bsdf_mat("Mat_MossClump")
    if nt is None:
        return mat
    geom = nt.nodes.new("ShaderNodeNewGeometry")
    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 18.0
    noise.inputs["Detail"].default_value = 8.0

    ramp = nt.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].position = 0.25
    ramp.color_ramp.elements[0].color = (0.08, 0.16, 0.06, 1.0)
    ramp.color_ramp.elements[1].position = 0.75
    ramp.color_ramp.elements[1].color = (0.20, 0.30, 0.10, 1.0)

    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.60
    bump.inputs["Distance"].default_value = 0.02

    nt.links.new(geom.outputs["Position"], noise.inputs["Vector"])
    nt.links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
    nt.links.new(ramp.outputs["Color"], bsdf.inputs["Base Color"])
    nt.links.new(noise.outputs["Fac"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    bsdf.inputs["Roughness"].default_value = 0.94
    return mat


def make_wet_stone():
    mat, nt, bsdf = _new_bsdf_mat("Mat_WetStone")
    if nt is None:
        return mat
    geom = nt.nodes.new("ShaderNodeNewGeometry")
    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 3.0
    noise.inputs["Detail"].default_value = 6.0

    ramp = nt.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].position = 0.3
    ramp.color_ramp.elements[0].color = (0.12, 0.13, 0.13, 1.0)
    ramp.color_ramp.elements[1].position = 0.8
    ramp.color_ramp.elements[1].color = (0.24, 0.24, 0.22, 1.0)

    rough = nt.nodes.new("ShaderNodeMapRange")
    rough.inputs["From Min"].default_value = 0.25
    rough.inputs["From Max"].default_value = 0.75
    rough.inputs["To Min"].default_value = 0.14
    rough.inputs["To Max"].default_value = 0.45

    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.35
    bump.inputs["Distance"].default_value = 0.02

    nt.links.new(geom.outputs["Position"], noise.inputs["Vector"])
    nt.links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
    nt.links.new(ramp.outputs["Color"], bsdf.inputs["Base Color"])
    nt.links.new(noise.outputs["Fac"], rough.inputs["Value"])
    nt.links.new(rough.outputs["Result"], bsdf.inputs["Roughness"])
    nt.links.new(noise.outputs["Fac"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    return mat


def create_materials():
    return {
        "cobble": make_cobble_pbr(),
        "wet_stone": make_wet_stone(),
        "moss_clump": make_moss_clump(),
        "plaster_weathered": make_weathered_plaster("Mat_WeatheredPlaster", (0.72, 0.64, 0.50), (0.24, 0.26, 0.17)),
        "plaster_alt": make_weathered_plaster("Mat_PlasterAlt", (0.58, 0.54, 0.46), (0.21, 0.22, 0.17)),
        "brick_exposed": make_brick_pbr("Mat_ExposedBrick", (0.44, 0.17, 0.11), (0.31, 0.12, 0.08), (0.48, 0.44, 0.38), scale=2.2, bw=0.42, rh=0.17, brick_rough=0.84, bump_str=0.60),
        "brick_glazed_teal": make_brick_pbr("Mat_GlazedBrick_Teal", (0.08, 0.28, 0.28), (0.05, 0.20, 0.21), (0.18, 0.18, 0.18), scale=2.8, bw=0.46, rh=0.20, brick_rough=0.14, bump_str=0.44),
        "brick_whitewash": make_brick_pbr("Mat_WhitewashedBrick", (0.72, 0.69, 0.62), (0.61, 0.58, 0.50), (0.44, 0.41, 0.35), scale=2.4, bw=0.45, rh=0.19, brick_rough=0.86, bump_str=0.38),
        "ashlar_stone": make_brick_pbr("Mat_AshlarStone", (0.44, 0.42, 0.38), (0.35, 0.34, 0.31), (0.22, 0.21, 0.19), scale=1.25, bw=0.85, rh=0.40, brick_rough=0.80, bump_str=0.46),
        "stone_dark": make_weathered_plaster("Mat_StoneDark", (0.24, 0.23, 0.21), (0.12, 0.15, 0.10)),
        "stone": make_weathered_plaster("Mat_Stone", (0.42, 0.40, 0.36), (0.20, 0.22, 0.15)),
        "workshop_floor": make_workshop_slab_floor(),
        "wood_aged_floor": make_wood_pbr("Mat_AgedWood_Floor", (0.30, 0.18, 0.09), (0.18, 0.10, 0.05), roughness=0.64, bump_str=0.30),
        "wood_light": make_wood_pbr("Mat_WoodLight_PBR", (0.42, 0.26, 0.14), (0.27, 0.16, 0.08), roughness=0.60, bump_str=0.24),
        "wood_dark": make_wood_pbr("Mat_WoodDark_PBR", (0.18, 0.10, 0.06), (0.10, 0.05, 0.03), roughness=0.58, bump_str=0.24),
        "roof_tile": make_brick_pbr("Mat_RoofTile", (0.46, 0.16, 0.10), (0.34, 0.12, 0.07), (0.18, 0.10, 0.07), scale=2.4, bw=0.38, rh=0.24, brick_rough=0.72, bump_str=0.45),
        "roof_slate": make_brick_pbr("Mat_RoofSlate", (0.16, 0.19, 0.23), (0.11, 0.13, 0.17), (0.08, 0.09, 0.11), scale=2.4, bw=0.42, rh=0.24, brick_rough=0.52, bump_str=0.38),
        "brass": make_simple_mat("Mat_Brass", (0.70, 0.50, 0.18), roughness=0.34, metallic=0.86),
        "gold": make_simple_mat("Mat_Gold", (0.90, 0.72, 0.22), roughness=0.24, metallic=0.94),
        "sign_gold": make_simple_mat("Mat_SignGold", (0.96, 0.78, 0.28), roughness=0.25, metallic=0.85, emission=(0.96, 0.75, 0.25), emission_strength=0.48),
        "sign_sub": make_simple_mat("Mat_SignSub", (0.92, 0.85, 0.70), roughness=0.6, emission=(0.85, 0.78, 0.62), emission_strength=0.25),
        "iron": make_simple_mat("Mat_Iron", (0.13, 0.14, 0.16), roughness=0.50, metallic=0.85),
        "velvet": make_simple_mat("Mat_Velvet", (0.42, 0.04, 0.07), roughness=0.92),
        "carpet_gold": make_simple_mat("Mat_CarpetGold", (0.68, 0.50, 0.16), roughness=0.86),
        "wax_red": make_simple_mat("Mat_WaxRed", (0.66, 0.07, 0.09), roughness=0.42),
        "dial_white": make_simple_mat("Mat_DialWhite", (0.92, 0.89, 0.81), roughness=0.38),
        "parchment": make_simple_mat("Mat_Parchment", (0.82, 0.74, 0.57), roughness=0.86),
        "blueprint": make_simple_mat("Mat_Blueprint", (0.11, 0.24, 0.44), roughness=0.76),
        "jade": make_simple_mat("Mat_Jade", (0.11, 0.55, 0.36), roughness=0.22, metallic=0.05),
        "amber": make_simple_mat("Mat_Amber", (0.86, 0.45, 0.06), roughness=0.14, emission=(0.86, 0.45, 0.06), emission_strength=0.45),
        "obsidian": make_simple_mat("Mat_Obsidian", (0.05, 0.04, 0.08), roughness=0.12, metallic=0.25),
        "water": make_simple_mat("Mat_Water", (0.09, 0.56, 0.64), roughness=0.06, metallic=0.10, emission=(0.10, 0.55, 0.68), emission_strength=0.65),
        "cyan_glow": make_simple_mat("Mat_CyanGlow", (0.20, 0.85, 1.0), roughness=0.2, emission=(0.20, 0.85, 1.0), emission_strength=3.4),
        "arcane_glow": make_simple_mat("Mat_ArcaneGlow", (0.62, 0.28, 0.96), roughness=0.2, emission=(0.62, 0.28, 0.96), emission_strength=3.2),
        "warm_glow": make_simple_mat("Mat_WarmGlow", (1.0, 0.76, 0.35), roughness=0.2, emission=(1.0, 0.72, 0.28), emission_strength=4.5),
        "glass": make_simple_mat("Mat_Glass", (0.68, 0.82, 0.90), roughness=0.06, metallic=0.15),
    }


# ==================== GEOMETRY HELPERS ====================

def add_bevel(obj, width=0.012, segments=2):
    mod = obj.modifiers.new(name="Bevel", type="BEVEL")
    mod.width = width
    mod.segments = segments
    mod.limit_method = "ANGLE"
    mod.angle_limit = math.radians(30)


def set_smooth(obj):
    if obj.type != "MESH":
        return
    for poly in obj.data.polygons:
        poly.use_smooth = True


def add_box(name, parent, loc, size, mat, rot=(0, 0, 0), bevel=0.012, col=None):
    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.scale(bm, vec=Vector(size), verts=bm.verts)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = Vector(loc)
    obj.rotation_euler = Euler(rot)
    if mat:
        obj.data.materials.append(mat)
    if parent:
        obj.parent = parent
    link_obj(obj, col or bpy.context.scene.collection)
    if bevel > 0:
        add_bevel(obj, width=min(bevel, min(size) * 0.22), segments=2)
    return obj


def add_cyl(name, parent, loc, radius, depth, mat, rot=(0, 0, 0), verts=24, bevel=0.008, smooth=True, col=None):
    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=verts, radius1=radius, radius2=radius, depth=depth)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = Vector(loc)
    obj.rotation_euler = Euler(rot)
    if mat:
        obj.data.materials.append(mat)
    if parent:
        obj.parent = parent
    link_obj(obj, col or bpy.context.scene.collection)
    if smooth:
        set_smooth(obj)
    if bevel > 0:
        add_bevel(obj, width=bevel, segments=2)
    return obj


def add_cone_frustum(name, parent, loc, r1, r2, depth, mat, rot=(0, 0, 0), verts=24, smooth=True, col=None):
    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=verts, radius1=r1, radius2=r2, depth=depth)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = Vector(loc)
    obj.rotation_euler = Euler(rot)
    if mat:
        obj.data.materials.append(mat)
    if parent:
        obj.parent = parent
    link_obj(obj, col or bpy.context.scene.collection)
    if smooth:
        set_smooth(obj)
    return obj


def add_sphere(name, parent, loc, radius, mat, scale=(1, 1, 1), segs=20, col=None):
    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=segs, v_segments=segs // 2, radius=radius)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = Vector(loc)
    obj.scale = Vector(scale)
    if mat:
        obj.data.materials.append(mat)
    if parent:
        obj.parent = parent
    link_obj(obj, col or bpy.context.scene.collection)
    set_smooth(obj)
    return obj


def add_torus_ring(name, parent, loc, major_r, minor_r, mat, rot=(0, 0, 0), major_segs=36, minor_segs=10, col=None):
    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm = bmesh.new()
    verts_grid = []
    for i in range(major_segs):
        theta = 2 * math.pi * i / major_segs
        cx, cy = math.cos(theta) * major_r, math.sin(theta) * major_r
        ring_v = []
        for j in range(minor_segs):
            phi = 2 * math.pi * j / minor_segs
            vx = cx + math.cos(theta) * math.cos(phi) * minor_r
            vy = cy + math.sin(theta) * math.cos(phi) * minor_r
            vz = math.sin(phi) * minor_r
            ring_v.append(bm.verts.new((vx, vy, vz)))
        verts_grid.append(ring_v)
    for i in range(major_segs):
        ni = (i + 1) % major_segs
        for j in range(minor_segs):
            nj = (j + 1) % minor_segs
            bm.faces.new((verts_grid[i][j], verts_grid[ni][j], verts_grid[ni][nj], verts_grid[i][nj]))
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = Vector(loc)
    obj.rotation_euler = Euler(rot)
    if mat:
        obj.data.materials.append(mat)
    if parent:
        obj.parent = parent
    link_obj(obj, col or bpy.context.scene.collection)
    set_smooth(obj)
    return obj


def add_gear_wheel(name, parent, loc, radius, thickness, teeth, mat, rot=(0, 0, 0), col=None):
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    root.rotation_euler = Euler(rot)
    if parent:
        root.parent = parent
    link_obj(root, col or bpy.context.scene.collection)
    add_torus_ring("Rim", root, (0, 0, 0), radius * 0.85, thickness * 0.5, mat, major_segs=28, col=col)
    add_cyl("Hub", root, (0, 0, 0), radius * 0.28, thickness * 1.3, mat, verts=16, col=col)
    for s in range(4):
        ang = s * (math.pi / 4)
        add_box(f"Spoke_{s}", root, (0, 0, 0), (radius * 1.65, radius * 0.14, thickness * 0.7), mat, rot=(0, 0, ang), bevel=0.004, col=col)
    for t in range(teeth):
        ang = 2 * math.pi * t / teeth
        tx, ty = math.cos(ang) * radius, math.sin(ang) * radius
        add_box(f"Tooth_{t}", root, (tx, ty, 0), (radius * 0.18, radius * 0.14, thickness), mat, rot=(0, 0, ang), bevel=0.003, col=col)
    return root


def add_moss_cluster(name, parent, loc, scale_xyz, mats, seed_rng, col):
    cluster = bpy.data.objects.new(name, None)
    cluster.location = Vector(loc)
    if parent:
        cluster.parent = parent
    link_obj(cluster, col)
    for i in range(4):
        ox = seed_rng.uniform(-0.18, 0.18) * scale_xyz[0]
        oy = seed_rng.uniform(-0.18, 0.18) * scale_xyz[1]
        oz = seed_rng.uniform(0.0, 0.12) * scale_xyz[2]
        r = seed_rng.uniform(0.10, 0.18) * max(scale_xyz[0], scale_xyz[1])
        add_sphere(f"MossBlob_{i}", cluster, (ox, oy, oz), r, mats["moss_clump"], scale=(1.2, 1.0, 0.55), segs=10, col=col)
    return cluster


def load_unicode_font():
    for fp in FONT_PATHS:
        if os.path.exists(fp):
            try:
                return bpy.data.fonts.load(fp)
            except Exception:
                pass
    return None


def add_3d_text(name, parent, text_str, loc, rot, size, mat, font_data=None, extrude=0.005, col=None):
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
    link_obj(obj, col or bpy.context.scene.collection)
    return obj


def add_point_light(name, parent, loc, color, energy, radius=0.25, col=None):
    ldata = bpy.data.lights.new(name=name, type="POINT")
    ldata.color = color
    ldata.energy = energy
    ldata.shadow_soft_size = radius
    lobj = bpy.data.objects.new(name, ldata)
    lobj.location = Vector(loc)
    if parent:
        lobj.parent = parent
    link_obj(lobj, col or bpy.context.scene.collection)
    return lobj


def add_spot_light(name, parent, loc, target_loc, color, energy, spot_deg=55, col=None):
    ldata = bpy.data.lights.new(name=name, type="SPOT")
    ldata.color = color
    ldata.energy = energy
    ldata.spot_size = math.radians(spot_deg)
    ldata.spot_blend = 0.35
    ldata.shadow_soft_size = 0.08
    lobj = bpy.data.objects.new(name, ldata)
    lobj.location = Vector(loc)
    direction = Vector(target_loc) - Vector(loc)
    lobj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    if parent:
        lobj.parent = parent
    link_obj(lobj, col or bpy.context.scene.collection)
    return lobj


# ==================== 1. ORGANIC PLAZA FLOOR, CURBSTONES & MULTIVERSE FOUNTAIN ====================

def build_street_and_plaza(mats, rng, col):
    add_box("Cobble_Road", None, (0, 0, -0.05), (48.0, 48.0, 0.10), mats["cobble"], bevel=0.0, col=col)

    # Wet dark stone apron & Radial Compass-Rose Mosaic around Fountain
    add_cyl("Plaza_WetApron", None, (0, 0, 0.012), 2.55, 0.024, mats["wet_stone"], verts=24, bevel=0.006, smooth=False, col=col)
    add_torus_ring("Plaza_RuneRing_Brass", None, (0, 0, 0.022), 2.82, 0.045, mats["brass"], major_segs=48, col=col)
    add_torus_ring("Plaza_RuneRing_Glow", None, (0, 0, 0.020), 2.94, 0.025, mats["arcane_glow"], major_segs=48, col=col)

    # 16 Radial weathered stone wedge slabs forming a compass-rose plaza floor
    for i in range(16):
        ang = i * (2 * math.pi / 16)
        r = 3.45
        sx, sy = math.cos(ang) * r, math.sin(ang) * r
        m = mats["ashlar_stone"] if i % 2 == 0 else mats["stone_dark"]
        tilt = rng.uniform(-0.018, 0.018)
        z_off = rng.uniform(0.008, 0.020)
        add_box(f"PlazaMosaic_{i}", None, (sx, sy, z_off), (0.85, 0.56, 0.04), m, rot=(tilt, tilt * 0.5, ang), bevel=0.012, col=col)

    # Scattered irregular 3D stepping stones along main road for organic surface relief
    for i in range(38):
        px = rng.uniform(-3.9, 3.9)
        py = rng.uniform(-11.0, 10.5)
        if math.hypot(px, py) < 3.95:
            continue
        w = rng.uniform(0.38, 0.72)
        d = rng.uniform(0.32, 0.62)
        m = mats["ashlar_stone"] if i % 3 != 0 else mats["wet_stone"]
        add_box(f"RoadSlab_{i}", None, (px, py, rng.uniform(0.006, 0.020)), (w, d, 0.036), m,
                rot=(rng.uniform(-0.025, 0.025), rng.uniform(-0.025, 0.025), rng.uniform(0, math.pi)), bevel=0.010, col=col)

    # Raised stone sidewalks + weathered tilted curbstones + moss clumps along curb joints
    for side_name, sx in (("Sidewalk_West", -10.5), ("Sidewalk_East", 10.5)):
        add_box(side_name, None, (sx, 0, 0.055), (12.2, 46.0, 0.11), mats["ashlar_stone"], bevel=0.012, col=col)
        curb_x = -4.38 if sx < 0 else 4.38
        for i in range(-18, 19):
            cy = i * 0.72 + rng.uniform(-0.015, 0.015)
            m = mats["stone_dark"] if i % 4 != 0 else mats["wet_stone"]
            add_box(f"{side_name}_Curb_{i}", None, (curb_x + rng.uniform(-0.01, 0.01), cy, 0.075 + rng.uniform(-0.006, 0.006)),
                    (0.24, 0.67, 0.15), m, rot=(rng.uniform(-0.02, 0.02), 0, rng.uniform(-0.02, 0.02)), bevel=0.015, col=col)
            if i % 3 == 0:
                add_moss_cluster(f"CurbMoss_{side_name}_{i}", None, (curb_x - (0.14 if sx > 0 else -0.14), cy, 0.02), (0.65, 0.65, 0.55), mats, rng, col)


def build_fountain(mats, rng, col):
    root = bpy.data.objects.new("Fountain_Central", None)
    link_obj(root, col)

    add_cyl("Plinth_Step1", root, (0, 0, 0.09), 2.35, 0.18, mats["wet_stone"], verts=8, bevel=0.02, smooth=False, col=col)
    add_cyl("Plinth_Step2", root, (0, 0, 0.24), 2.10, 0.14, mats["ashlar_stone"], verts=8, bevel=0.02, smooth=False, col=col)

    rim_r = 1.72
    seg_w = 2 * rim_r * math.tan(math.pi / 8) + 0.12
    for i in range(8):
        ang = i * (math.pi / 4)
        cx, cy = math.cos(ang) * rim_r, math.sin(ang) * rim_r
        add_box(f"BasinWall_{i}", root, (cx, cy, 0.56), (0.26, seg_w, 0.56), mats["ashlar_stone"], rot=(0, 0, ang), bevel=0.02, col=col)
        add_box(f"BasinCap_{i}", root, (cx, cy, 0.87), (0.34, seg_w + 0.04, 0.10), mats["wet_stone"], rot=(0, 0, ang), bevel=0.02, col=col)
        pang = ang + math.pi / 8
        px, py = math.cos(pang) * (rim_r + 0.06), math.sin(pang) * (rim_r + 0.06)
        add_box(f"CornerPost_{i}", root, (px, py, 0.62), (0.32, 0.32, 0.68), mats["stone_dark"], rot=(0, 0, pang), bevel=0.02, col=col)
        add_sphere(f"CornerBrassOrb_{i}", root, (px, py, 1.02), 0.085, mats["brass"], col=col)
        if i % 2 == 0:
            add_moss_cluster(f"FountainMoss_{i}", root, (px * 1.18, py * 1.18, 0.16), (0.7, 0.7, 0.7), mats, rng, col)

    add_cyl("WaterSurface_Lower", root, (0, 0, 0.68), 1.62, 0.06, mats["water"], verts=8, bevel=0.0, smooth=True, col=col)
    add_cyl("Pedestal_Base", root, (0, 0, 0.72), 0.52, 0.85, mats["wet_stone"], verts=16, bevel=0.015, col=col)
    add_cone_frustum("Pedestal_Flute", root, (0, 0, 1.32), 0.42, 0.28, 0.60, mats["ashlar_stone"], verts=16, col=col)
    add_cone_frustum("Bowl_Tier1", root, (0, 0, 1.68), 0.32, 0.96, 0.32, mats["ashlar_stone"], verts=24, col=col)
    add_cyl("Bowl_Tier1_Rim", root, (0, 0, 1.85), 0.98, 0.08, mats["stone_dark"], verts=24, bevel=0.015, col=col)
    add_cyl("WaterSurface_Tier1", root, (0, 0, 1.82), 0.88, 0.04, mats["water"], verts=24, bevel=0.0, col=col)
    add_cone_frustum("Pedestal_Upper", root, (0, 0, 2.15), 0.24, 0.16, 0.65, mats["ashlar_stone"], verts=16, col=col)
    add_cone_frustum("Bowl_Tier2", root, (0, 0, 2.52), 0.18, 0.52, 0.22, mats["ashlar_stone"], verts=24, col=col)
    add_cyl("WaterSurface_Tier2", root, (0, 0, 2.61), 0.46, 0.03, mats["water"], verts=24, bevel=0.0, col=col)
    add_cyl("Finial_Stem", root, (0, 0, 2.78), 0.07, 0.34, mats["brass"], verts=16, col=col)
    add_sphere("Finial_Orb", root, (0, 0, 3.00), 0.14, mats["cyan_glow"], col=col)

    # Multiverse Astrolabe / Orrery Brass Rings + Levitating Arcane Runestones around Spire!
    add_torus_ring("Orrery_RingA", root, (0, 0, 2.95), 0.62, 0.025, mats["brass"], rot=(math.radians(38), math.radians(22), 0), col=col)
    add_torus_ring("Orrery_RingB", root, (0, 0, 2.95), 0.48, 0.020, mats["gold"], rot=(math.radians(-42), math.radians(30), 0.4), col=col)
    for k in range(4):
        ang = k * (math.pi / 2) + 0.4
        rx, ry = math.cos(ang) * 0.78, math.sin(ang) * 0.78
        add_box(f"LevitatingRuneStone_{k}", root, (rx, ry, 2.88 + (k % 2) * 0.22), (0.14, 0.10, 0.24), mats["obsidian"], rot=(0.2, 0.3, ang), bevel=0.01, col=col)
        add_box(f"LevitatingRuneGlow_{k}", root, (rx, ry, 2.88 + (k % 2) * 0.22), (0.15, 0.04, 0.10), mats["arcane_glow"], rot=(0.2, 0.3, ang), bevel=0.004, col=col)

    for i in range(4):
        ang = i * (math.pi / 2)
        add_cyl(f"Spout_{i}", root, (math.cos(ang) * 0.42, math.sin(ang) * 0.42, 1.30), 0.04, 0.34, mats["brass"], rot=(0, math.radians(72), ang), col=col)
        add_cyl(f"WaterJet_{i}", root, (math.cos(ang) * 0.72, math.sin(ang) * 0.72, 1.02), 0.024, 0.62, mats["water"], rot=(0, math.radians(24), ang), bevel=0.0, col=col)

    add_point_light("Fountain_AquaLight", root, (0, 0, 1.25), (0.35, 0.92, 1.0), 55.0, radius=0.4, col=col)
    return root


# ==================== 2. BLIND BOXES & ARTIFACTS ====================

def build_blind_box(name, variant, loc, mats, col, parent=None):
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    if parent:
        root.parent = parent
    link_obj(root, col)

    if variant == "WoodCrate_Old":
        body_m, trim_m, seal_m = mats["wood_light"], mats["wood_dark"], mats["wax_red"]
    elif variant == "BrassChest_Steam":
        body_m, trim_m, seal_m = mats["wood_dark"], mats["brass"], mats["gold"]
    elif variant == "RunicBox_Arcane":
        body_m, trim_m, seal_m = mats["stone_dark"], mats["gold"], mats["arcane_glow"]
    else:
        body_m, trim_m, seal_m = mats["iron"], mats["stone_dark"], mats["cyan_glow"]

    add_box("Body", root, (0, 0, 0.10), (0.28, 0.36, 0.18), body_m, bevel=0.012, col=col)
    add_box("BaseRim", root, (0, 0, 0.018), (0.30, 0.38, 0.032), trim_m, bevel=0.008, col=col)
    for sx in (-1, 1):
        for sy in (-1, 1):
            add_box(f"Corner_{sx}_{sy}", root, (sx * 0.135, sy * 0.175, 0.10), (0.028, 0.028, 0.19), trim_m, bevel=0.005, col=col)

    lid = bpy.data.objects.new("LidPivot", None)
    lid.location = Vector((-0.14, 0, 0.19))
    lid.parent = root
    link_obj(lid, col)
    add_box("LidPlate", lid, (0.14, 0, 0.025), (0.295, 0.375, 0.05), body_m, bevel=0.012, col=col)
    add_box("LidBand", lid, (0.14, 0, 0.052), (0.26, 0.33, 0.014), trim_m, bevel=0.005, col=col)

    if variant == "BrassChest_Steam":
        add_cyl("Gauge", root, (0.145, -0.09, 0.105), 0.032, 0.016, mats["brass"], rot=(0, math.pi / 2, 0), col=col)
        add_cyl("GaugeFace", root, (0.153, -0.09, 0.105), 0.022, 0.008, mats["dial_white"], rot=(0, math.pi / 2, 0), col=col)
    elif variant == "RunicBox_Arcane":
        add_box("RuneTop", lid, (0.14, 0, 0.064), (0.09, 0.09, 0.014), mats["arcane_glow"], rot=(0, 0, math.radians(45)), bevel=0.004, col=col)
    elif variant == "HoloPod_SciFi":
        add_box("HoloStripL", root, (0, -0.185, 0.105), (0.20, 0.014, 0.025), mats["cyan_glow"], bevel=0.004, col=col)
        add_box("HoloStripR", root, (0, 0.185, 0.105), (0.20, 0.014, 0.025), mats["cyan_glow"], bevel=0.004, col=col)

    add_cyl("SealPart", root, (0.148, 0, 0.185), 0.032, 0.018, seal_m, rot=(0, math.pi / 2, 0), col=col)
    return root


def build_artifact(name, key, loc, mats, col, parent=None, scale=1.0):
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    root.scale = Vector((scale, scale, scale))
    if parent:
        root.parent = parent
    link_obj(root, col)

    if key == "PocketWatch_Brass":
        add_cyl("Case", root, (0, 0, 0.085), 0.08, 0.026, mats["brass"], rot=(0, math.pi / 2, 0), verts=32, col=col)
        add_cyl("Dial", root, (-0.014, 0, 0.085), 0.066, 0.008, mats["dial_white"], rot=(0, math.pi / 2, 0), verts=32, col=col)
        add_box("Hand1", root, (-0.020, 0.012, 0.098), (0.005, 0.007, 0.042), mats["iron"], rot=(math.radians(35), 0, 0), bevel=0.001, col=col)
        add_cyl("Crown", root, (0, 0, 0.175), 0.014, 0.026, mats["gold"], col=col)
    elif key == "Chalice_Royal":
        add_cyl("Foot", root, (0, 0, 0.015), 0.065, 0.028, mats["gold"], verts=28, col=col)
        add_cone_frustum("Stem", root, (0, 0, 0.065), 0.022, 0.016, 0.08, mats["brass"], verts=24, col=col)
        add_sphere("Knop", root, (0, 0, 0.072), 0.026, mats["gold"], scale=(1, 1, 0.7), col=col)
        add_cone_frustum("Cup", root, (0, 0, 0.150), 0.026, 0.075, 0.10, mats["gold"], verts=28, col=col)
        add_sphere("Gem", root, (-0.062, 0, 0.16), 0.014, mats["wax_red"], col=col)
    elif key == "HoloCube_SciFi":
        add_box("Frame", root, (0, 0, 0.095), (0.13, 0.13, 0.13), mats["iron"], bevel=0.012, col=col)
        add_box("Core", root, (0, 0, 0.095), (0.085, 0.085, 0.085), mats["cyan_glow"], rot=(0.5, 0.5, 0.5), bevel=0.006, col=col)
        add_box("RingH", root, (0, 0, 0.095), (0.145, 0.145, 0.018), mats["cyan_glow"], bevel=0.003, col=col)
    elif key == "RunicMask_Arcane":
        add_box("MaskPlate", root, (0, 0, 0.115), (0.045, 0.14, 0.19), mats["stone_dark"], bevel=0.012, col=col)
        add_box("EyeL", root, (-0.025, -0.032, 0.135), (0.012, 0.032, 0.014), mats["arcane_glow"], bevel=0.002, col=col)
        add_box("EyeR", root, (-0.025, 0.032, 0.135), (0.012, 0.032, 0.014), mats["arcane_glow"], bevel=0.002, col=col)
        add_box("Sigil", root, (-0.025, 0, 0.175), (0.012, 0.028, 0.028), mats["gold"], rot=(math.radians(45), 0, 0), bevel=0.002, col=col)
        add_box("HornL", root, (0, -0.062, 0.215), (0.024, 0.024, 0.065), mats["gold"], rot=(math.radians(-18), 0, 0), bevel=0.004, col=col)
        add_box("HornR", root, (0, 0.062, 0.215), (0.024, 0.024, 0.065), mats["gold"], rot=(math.radians(18), 0, 0), bevel=0.004, col=col)
    elif key == "AutomatonBird_Brass":
        add_cyl("Base", root, (0, 0, 0.012), 0.06, 0.024, mats["wood_dark"], col=col)
        add_cyl("Post", root, (0, 0, 0.055), 0.01, 0.07, mats["brass"], col=col)
        add_sphere("Body", root, (0, 0, 0.115), 0.055, mats["gold"], scale=(1.35, 0.85, 0.85), col=col)
        add_sphere("Head", root, (-0.055, 0, 0.155), 0.028, mats["brass"], col=col)
        add_cone_frustum("Beak", root, (-0.09, 0, 0.152), 0.010, 0.001, 0.028, mats["gold"], rot=(0, -math.pi / 2, 0), col=col)
        add_sphere("Eye", root, (-0.068, -0.022, 0.162), 0.008, mats["cyan_glow"], col=col)
    elif key == "VoidPrism_Alien":
        add_box("Crystal", root, (0, 0, 0.125), (0.08, 0.08, 0.16), mats["obsidian"], rot=(0.3, 0.3, 0.7), bevel=0.008, col=col)
        add_cyl("Ring1", root, (0, 0, 0.125), 0.10, 0.008, mats["arcane_glow"], rot=(0.45, 0.35, 0), verts=32, bevel=0.002, col=col)
        add_cyl("Ring2", root, (0, 0, 0.125), 0.088, 0.008, mats["cyan_glow"], rot=(-0.55, 0.25, 0.3), verts=32, bevel=0.002, col=col)
    elif key == "Compass_Starlight":
        add_cyl("Base", root, (0, 0, 0.022), 0.085, 0.032, mats["brass"], verts=28, col=col)
        add_cyl("Face", root, (0, 0, 0.040), 0.070, 0.006, mats["stone_dark"], verts=28, col=col)
        add_box("NeedleA", root, (0, 0, 0.048), (0.11, 0.016, 0.008), mats["gold"], rot=(0, 0, 0.4), bevel=0.002, col=col)
        add_box("NeedleB", root, (0, 0, 0.048), (0.016, 0.11, 0.008), mats["cyan_glow"], rot=(0, 0, 0.4), bevel=0.002, col=col)
    else:
        add_box("Stone", root, (0, 0, 0.02), (0.14, 0.12, 0.036), mats["stone_dark"], bevel=0.008, col=col)
        add_sphere("Amber", root, (0, 0, 0.095), 0.068, mats["amber"], scale=(1.0, 0.8, 1.15), col=col)
    return root


# ==================== 3. MULTIVERSE WALK-IN HOUSES (ASYMMETRIC & WEATHERED) ====================

def _build_house_shell(name, loc, facing_east, ext_low_mat, ext_up_mat, int_low_mat, int_up_mat, roof_mat, sign_title, sign_sub, sign_z, mats, font_data, rng, col):
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    if not facing_east:
        root.rotation_euler = Euler((0, 0, math.pi))
    link_obj(root, col)

    # Weathered stone foundation & stepped entryway
    add_box("Foundation", root, (0, 0, 0.14), (5.2, 6.2, 0.18), mats["wet_stone"], bevel=0.02, col=col)
    add_box("StepEntry", root, (2.78, 0, 0.12), (0.55, 2.7, 0.14), mats["ashlar_stone"], bevel=0.015, col=col)

    # Outer & Inner Shells
    add_box("Ext_BackLow", root, (-2.46, 0, 0.72), (0.14, 6.0, 1.05), ext_low_mat, bevel=0.012, col=col)
    add_box("Ext_BackUp", root, (-2.46, 0, 2.32), (0.12, 6.0, 2.16), ext_up_mat, bevel=0.01, col=col)
    add_box("Int_BackLow", root, (-2.33, 0, 0.72), (0.06, 5.75, 1.05), int_low_mat, bevel=0.008, col=col)
    add_box("Int_BackUp", root, (-2.33, 0, 2.32), (0.06, 5.75, 2.16), int_up_mat, bevel=0.008, col=col)

    for sy_name, sy, isign in (("SideL", -2.90, -1), ("SideR", 2.90, 1)):
        ext_y = sy + isign * 0.05
        int_y = sy - isign * 0.07
        for prefix, wy, thk in (("Ext", ext_y, 0.12), ("Int", int_y, 0.06)):
            low_m = ext_low_mat if prefix == "Ext" else int_low_mat
            up_m = ext_up_mat if prefix == "Ext" else int_up_mat
            add_box(f"{prefix}_{sy_name}_Low", root, (0, wy, 0.72), (4.95, thk, 1.05), low_m, bevel=0.01, col=col)
            add_box(f"{prefix}_{sy_name}_BackPier", root, (-1.60, wy, 2.32), (1.76, thk, 2.16), up_m, bevel=0.008, col=col)
            add_box(f"{prefix}_{sy_name}_FrontPier", root, (1.60, wy, 2.32), (1.76, thk, 2.16), up_m, bevel=0.008, col=col)
            add_box(f"{prefix}_{sy_name}_WinLintel", root, (0, wy, 2.90), (1.45, thk, 1.00), up_m, bevel=0.008, col=col)
        add_box(f"{sy_name}_WinSill", root, (0, sy, 1.24), (1.48, 0.28, 0.12), mats["wood_dark"], bevel=0.01, col=col)
        add_box(f"{sy_name}_Glass", root, (0, sy, 1.82), (1.30, 0.04, 1.10), mats["glass"], bevel=0.004, col=col)
        add_box(f"{sy_name}_MullionV", root, (0, sy, 1.82), (0.06, 0.14, 1.10), mats["wood_dark"], bevel=0.005, col=col)
        add_box(f"{sy_name}_MullionH", root, (0, sy, 1.82), (1.30, 0.14, 0.06), mats["wood_dark"], bevel=0.005, col=col)

    for side, fy in (("FrontL", -2.10), ("FrontR", 2.10)):
        add_box(f"Ext_{side}_Low", root, (2.46, fy, 0.72), (0.14, 1.80, 1.05), ext_low_mat, bevel=0.012, col=col)
        add_box(f"Ext_{side}_Up", root, (2.46, fy, 2.32), (0.12, 1.80, 2.16), ext_up_mat, bevel=0.01, col=col)
        add_box(f"Int_{side}_Low", root, (2.34, fy, 0.72), (0.06, 1.68, 1.05), int_low_mat, bevel=0.008, col=col)
        add_box(f"Int_{side}_Up", root, (2.34, fy, 2.32), (0.06, 1.68, 2.16), int_up_mat, bevel=0.008, col=col)

    add_box("Ext_DoorWall", root, (2.46, 0, 3.16), (0.14, 2.44, 0.56), ext_up_mat, bevel=0.01, col=col)
    add_box("Int_DoorWall", root, (2.34, 0, 3.16), (0.06, 2.44, 0.56), int_up_mat, bevel=0.008, col=col)
    add_box("DoorPostL", root, (2.44, -1.22, 1.56), (0.28, 0.16, 2.75), mats["wood_dark"], bevel=0.012, col=col)
    add_box("DoorPostR", root, (2.44, 1.22, 1.56), (0.28, 0.16, 2.75), mats["wood_dark"], bevel=0.012, col=col)
    add_box("DoorHeaderBeam", root, (2.44, 0, 2.92), (0.32, 2.76, 0.18), mats["wood_dark"], bevel=0.012, col=col)

    for cx in (-2.48, 2.48):
        for cy in (-2.94, 2.94):
            add_box(f"CornerBeam_{cx}_{cy}", root, (cx, cy, 1.82), (0.22, 0.22, 3.35), mats["wood_dark"], bevel=0.012, col=col)
    add_box("EaveBeamL", root, (0, -2.94, 3.48), (5.2, 0.22, 0.18), mats["wood_dark"], bevel=0.01, col=col)
    add_box("EaveBeamR", root, (0, 2.94, 3.48), (5.2, 0.22, 0.18), mats["wood_dark"], bevel=0.01, col=col)
    add_box("FrontTieBeam", root, (2.48, 0, 3.48), (0.22, 6.08, 0.18), mats["wood_dark"], bevel=0.01, col=col)
    add_box("BackTieBeam", root, (-2.48, 0, 3.48), (0.22, 6.08, 0.18), mats["wood_dark"], bevel=0.01, col=col)

    for gx_name, gx in (("FrontGable", 2.42), ("BackGable", -2.42)):
        for step in range(5):
            w = 5.4 - step * 1.05
            z = 3.63 + step * 0.30
            add_box(f"{gx_name}_{step}", root, (gx, 0, z), (0.16, w, 0.32), ext_up_mat, bevel=0.008, col=col)

    slope_angle = math.atan2(1.70, 3.30)
    slope_len = math.hypot(1.70, 3.30) + 0.35
    for side_sign, sname in ((-1, "RoofL"), (1, "RoofR")):
        ry = side_sign * 1.65
        rz = 4.33
        rot_x = -side_sign * slope_angle
        rot = (rot_x, 0, 0)
        add_box(f"{sname}_Base", root, (0, ry, rz), (5.65, slope_len, 0.14), mats["wood_dark"], rot=rot, bevel=0.01, col=col)
        for row in range(6):
            t_local = -0.40 + row * 0.16
            dist = t_local * slope_len
            wy = ry + dist * math.cos(rot_x)
            wz = rz + dist * math.sin(rot_x) + 0.08
            # Organic subtle sag/tilt variation on roof tile courses
            jitter_x = rot_x + rng.uniform(-0.022, 0.022)
            jitter_z = rng.uniform(-0.014, 0.014)
            add_box(f"{sname}_Tiles_{row}", root, (0, wy, wz), (5.75, 0.56, 0.08), roof_mat, rot=(jitter_x, 0, jitter_z), bevel=0.012, col=col)
    add_box("RoofRidgeCap", root, (0, 0, 5.26), (5.82, 0.28, 0.18), mats["stone_dark"], bevel=0.02, col=col)

    # Slightly weathered crooked ceiling rafters
    for rx in (-1.6, -0.5, 0.6, 1.7):
        add_box(f"CeilingRafter_{rx}", root, (rx, 0, 3.45), (0.14, 5.7, 0.16), mats["wood_dark"], rot=(rng.uniform(-0.02, 0.02), 0, 0), bevel=0.008, col=col)

    # Moderate-sized Signboard + Hanging Blade Sign
    add_box("MainSign_Frame", root, (2.64, 0, sign_z), (0.09, 2.24, 0.58), mats["brass"], bevel=0.012, col=col)
    add_box("MainSign_Board", root, (2.67, 0, sign_z), (0.07, 2.12, 0.48), mats["wood_dark"], bevel=0.008, col=col)
    txt_rot = (math.pi / 2, 0, math.pi / 2)
    add_3d_text("MainSign_Title", root, sign_title, (2.716, 0, sign_z + 0.08), txt_rot, 0.145, mats["sign_gold"], font_data=font_data, extrude=0.004, col=col)
    add_3d_text("MainSign_Sub", root, sign_sub, (2.716, 0, sign_z - 0.10), txt_rot, 0.092, mats["sign_sub"], font_data=font_data, extrude=0.003, col=col)

    add_box("BladeBracket", root, (2.72, -1.78, 2.76), (0.05, 1.16, 0.05), mats["iron"], bevel=0.006, col=col)
    add_cyl("BladeRingL", root, (2.72, -2.12, 2.70), 0.015, 0.12, mats["iron"], col=col)
    add_cyl("BladeRingR", root, (2.72, -1.58, 2.70), 0.015, 0.12, mats["iron"], col=col)
    add_box("BladeBoard", root, (2.72, -1.85, 2.46), (0.05, 0.88, 0.42), mats["wood_dark"], bevel=0.008, col=col)
    add_box("BladeTrim", root, (2.72, -1.85, 2.46), (0.035, 0.94, 0.48), mats["brass"], bevel=0.008, col=col)
    add_3d_text("Blade_TextF", root, sign_title, (2.752, -1.85, 2.46), txt_rot, 0.068, mats["sign_gold"], font_data=font_data, extrude=0.003, col=col)
    add_3d_text("Blade_TextB", root, sign_title, (2.688, -1.85, 2.46), (math.pi / 2, 0, -math.pi / 2), 0.068, mats["sign_gold"], font_data=font_data, extrude=0.003, col=col)

    for ly in (-1.35, 1.35):
        add_box(f"PorchBracket_{ly}", root, (2.66, ly, 2.63), (0.28, 0.05, 0.05), mats["iron"], bevel=0.005, col=col)
        add_cyl(f"PorchLantern_{ly}", root, (2.78, ly, 2.44), 0.09, 0.22, mats["warm_glow"], verts=8, col=col)
    add_point_light(f"{name}_PorchGlow", root, (3.1, 0, 2.48), (1.0, 0.78, 0.45), 38.0, radius=0.35, col=col)

    add_cyl("CeilingCord", root, (0, 0, 3.30), 0.02, 1.10, mats["iron"], col=col)
    add_cyl("CeilingLanternRing", root, (0, 0, 2.75), 0.34, 0.05, mats["brass"], verts=16, col=col)
    add_cyl("CeilingLanternCore", root, (0, 0, 2.68), 0.12, 0.18, mats["warm_glow"], verts=8, col=col)
    add_point_light(f"{name}_InteriorLight", root, (0.2, 0, 2.55), (1.0, 0.82, 0.55), 125.0, radius=0.5, col=col)

    return root


def build_blindbox_house_unique(loc, mats, font_data, rng, col):
    """
    House 1 (Dì Mận — Cổ Tự & Khe Nứt Dị Giới):
    - Asymmetric Octagonal Arcane Corner Turret with crooked witch-hat spire & floating rune halo!
    - Stepped Gothic stone archway with glowing arcane keystone.
    - Organic jagged exposed-brick clusters + 3D moss clumps on damp corners.
    - Interior: 32 staggered aged timber floorboards + Sunken Brass & Glowing Arcane Astrolabe Floor Medallion!
    """
    root = _build_house_shell(
        "Shop_BlindBox_House", loc, True,
        ext_low_mat=mats["brick_exposed"],
        ext_up_mat=mats["plaster_weathered"],
        int_low_mat=mats["wood_dark"],
        int_up_mat=mats["brick_exposed"],
        roof_mat=mats["roof_tile"],
        sign_title="QUẦY NIÊM PHONG",
        sign_sub="BÁN HỘP MÙ · DÌ MẬN",
        sign_z=3.30,
        mats=mats, font_data=font_data, rng=rng, col=col,
    )

    # Interior Floor: 32 Staggered Aged Wood Planks + Sunken Arcane Astrolabe Medallion!
    plank_mats = [mats["wood_aged_floor"], mats["wood_dark"], mats["wood_light"]]
    for row in range(16):
        py = -2.66 + row * 0.355
        split_x = rng.uniform(-0.8, 0.8)
        for seg, (cx, len_x) in enumerate((((-2.4 + split_x) * 0.5, split_x + 2.4 - 0.02), ((2.4 + split_x) * 0.5, 2.4 - split_x - 0.02))):
            m = plank_mats[(row + seg) % 3]
            add_box(f"FloorPlank_{row}_{seg}", root, (cx, py, 0.24 + rng.uniform(-0.003, 0.003)),
                    (len_x, 0.335, 0.032), m, rot=(rng.uniform(-0.008, 0.008), 0, 0), bevel=0.005, col=col)

    # Sunken Brass & Glowing Arcane Astrolabe Medallion embedded in floor center (X = +1.05, Y = 0)
    add_cyl("FloorMedallion_Slate", root, (1.05, 0, 0.256), 0.76, 0.020, mats["obsidian"], verts=28, bevel=0.003, col=col)
    add_torus_ring("FloorMedallion_OuterBrass", root, (1.05, 0, 0.268), 0.76, 0.028, mats["brass"], major_segs=32, col=col)
    add_torus_ring("FloorMedallion_GlowRing", root, (1.05, 0, 0.268), 0.54, 0.018, mats["arcane_glow"], major_segs=32, col=col)
    add_torus_ring("FloorMedallion_InnerBrass", root, (1.05, 0, 0.270), 0.30, 0.015, mats["gold"], major_segs=24, col=col)
    add_cyl("FloorMedallion_CenterHub", root, (1.05, 0, 0.271), 0.08, 0.016, mats["gold"], verts=16, col=col)
    for sp in range(4):
        add_box(f"MedallionSpoke_{sp}", root, (1.05, 0, 0.267), (1.36, 0.04, 0.010), mats["brass"], rot=(0, 0, sp * math.pi / 4), bevel=0.002, col=col)

    # Asymmetric Octagonal Arcane Corner Turret on front-left corner (X = +1.15, Y = -3.15)
    turret = bpy.data.objects.new("Arcane_Corner_Turret", None)
    turret.location = Vector((1.15, -3.15, 0.0))
    turret.parent = root
    link_obj(turret, col)
    add_cyl("Turret_Base", turret, (0, 0, 0.65), 1.05, 1.30, mats["ashlar_stone"], verts=8, bevel=0.02, smooth=False, col=col)
    add_cyl("Turret_Shaft", turret, (0, 0, 2.85), 0.92, 3.10, mats["plaster_weathered"], verts=8, bevel=0.02, smooth=False, col=col)
    add_cyl("Turret_Crown", turret, (0, 0, 4.48), 1.04, 0.24, mats["wood_dark"], verts=8, bevel=0.02, smooth=False, col=col)
    # Steep crooked witch-hat spire
    add_cone_frustum("Turret_Spire", turret, (0.04, -0.04, 5.75), 1.18, 0.05, 2.35, mats["roof_tile"], rot=(0.04, -0.05, 0.2), verts=8, smooth=False, col=col)
    add_sphere("Turret_Finial", turret, (0.08, -0.08, 6.95), 0.12, mats["arcane_glow"], col=col)
    add_torus_ring("Turret_LevitatingHalo", turret, (0.05, -0.05, 5.65), 0.92, 0.028, mats["arcane_glow"], rot=(0.25, -0.20, 0), col=col)
    # Glowing arcane slit windows on turret
    for wz in (2.5, 3.6):
        add_box(f"Turret_Slit_{wz}", turret, (0.85, -0.35, wz), (0.14, 0.18, 0.55), mats["arcane_glow"], rot=(0, 0, -0.35), bevel=0.005, col=col)

    # Stepped Gothic Stone Archway + Glowing Arcane Keystone around front doorway
    for i, (ay, az, rot_x) in enumerate(((-1.26, 2.35, 0.35), (-0.82, 2.75, 0.72), (0.0, 2.96, 0.0), (0.82, 2.75, -0.72), (1.26, 2.35, -0.35))):
        m = mats["arcane_glow"] if i == 2 else mats["ashlar_stone"]
        add_box(f"ArchVoussoir_{i}", root, (2.60, ay, az), (0.18, 0.40, 0.20), m, rot=(rot_x, 0, 0), bevel=0.012, col=col)

    # Stone Quoins & Organic Jagged Exposed Brick Patches + 3D Moss Clumps
    for qx in (-2.48, 2.48):
        for qy in (-2.94, 2.94):
            for step in range(6):
                qz = 0.50 + step * 0.48
                w_scale = 0.34 if step % 2 == 0 else 0.24
                add_box(f"Quoin_{qx}_{qy}_{step}", root, (qx, qy, qz), (w_scale, 0.28, 0.24), mats["ashlar_stone"], bevel=0.012, col=col)

    # Non-overlapping staggered brick clusters breaking out of damp plaster (distinct X depth avoids Z-fighting)
    for idx, (px, py, pz, sy, sz) in enumerate((
        (2.505, -2.18, 1.92, 0.85, 0.48),
        (2.512, -1.88, 2.26, 0.52, 0.24),
        (2.515, -2.35, 1.56, 0.48, 0.28),
        (2.505, 1.95, 1.58, 0.62, 0.42),
    )):
        add_box(f"JaggedBrick_{idx}", root, (px, py, pz), (0.04, sy, sz), mats["brick_exposed"], bevel=0.008, col=col)

    add_moss_cluster("House1_Moss_1", root, (2.55, -2.55, 0.24), (0.9, 1.1, 0.9), mats, rng, col)
    add_moss_cluster("House1_Moss_2", root, (2.55, 2.65, 0.24), (0.8, 0.9, 0.8), mats, rng, col)
    add_moss_cluster("House1_RoofMoss", root, (1.1, -1.8, 4.15), (1.2, 0.8, 0.5), mats, rng, col)

    # Brick Chimney & Dormer
    add_box("Chimney_Shaft", root, (-1.65, -1.65, 4.95), (0.62, 0.62, 1.85), mats["brick_exposed"], bevel=0.015, col=col)
    add_box("Chimney_Cap", root, (-1.65, -1.65, 5.92), (0.76, 0.76, 0.14), mats["stone_dark"], bevel=0.015, col=col)
    add_cyl("Chimney_Pot", root, (-1.65, -1.65, 6.12), 0.12, 0.28, mats["roof_tile"], col=col)

    add_box("Dormer_Body", root, (0.4, 1.75, 4.35), (1.15, 0.95, 0.90), mats["plaster_weathered"], bevel=0.01, col=col)
    add_box("Dormer_Roof", root, (0.4, 1.75, 4.92), (1.28, 1.10, 0.22), mats["roof_tile"], bevel=0.015, col=col)
    add_box("Dormer_Glass", root, (0.4, 2.23, 4.35), (0.65, 0.04, 0.55), mats["glass"], bevel=0.004, col=col)

    # Exterior Porch Props (crates & ceramic amphora)
    c = build_blind_box("Porch_Crate_1", "WoodCrate_Old", (2.82, 2.05, 0.20), mats, col, parent=root)
    c.rotation_euler = Euler((0, 0, 0.22))
    add_cyl("Porch_Amphora", root, (2.82, 2.48, 0.42), 0.16, 0.46, mats["roof_tile"], col=col)

    # Interior Carpet beside Medallion + Framed Map + Shelves + Counter
    add_box("Int_Carpet_Border", root, (0.15, -1.35, 0.264), (1.45, 2.15, 0.015), mats["carpet_gold"], rot=(0, 0, -0.06), bevel=0.004, col=col)
    add_box("Int_Carpet_Center", root, (0.15, -1.35, 0.270), (1.30, 1.98, 0.015), mats["velvet"], rot=(0, 0, -0.06), bevel=0.004, col=col)

    add_box("Map_Frame", root, (-1.35, -2.79, 2.05), (1.15, 0.05, 0.78), mats["wood_dark"], bevel=0.008, col=col)
    add_box("Map_Parchment", root, (-1.35, -2.76, 2.05), (1.02, 0.03, 0.65), mats["parchment"], bevel=0.003, col=col)

    add_box("BackShelf_PostL", root, (-2.05, -1.85, 1.32), (0.45, 0.08, 2.15), mats["wood_dark"], bevel=0.008, col=col)
    add_box("BackShelf_PostR", root, (-2.05, 1.85, 1.32), (0.45, 0.08, 2.15), mats["wood_dark"], bevel=0.008, col=col)
    for sz in (0.85, 1.45, 2.05):
        add_box(f"BackShelf_Plank_{sz}", root, (-2.05, 0, sz), (0.42, 3.65, 0.05), mats["wood_light"], bevel=0.008, col=col)

    add_box("Counter_Base", root, (-0.65, 0, 0.68), (0.88, 2.75, 0.88), mats["wood_dark"], bevel=0.015, col=col)
    add_box("Counter_Top", root, (-0.62, 0, 1.16), (1.04, 2.92, 0.08), mats["wood_aged_floor"], bevel=0.012, col=col)
    add_box("Counter_BrassTrim", root, (-0.14, 0, 1.08), (0.04, 2.78, 0.05), mats["brass"], bevel=0.006, col=col)

    variants = ["WoodCrate_Old", "BrassChest_Steam", "RunicBox_Arcane", "HoloPod_SciFi"]
    y_offsets = [-1.02, -0.34, 0.34, 1.02]
    for i, var in enumerate(variants):
        b = build_blind_box(f"Display_{var}", var, (-0.52, y_offsets[i], 1.20), mats, col, parent=root)
        b.rotation_euler = Euler((0, 0, (i - 1.5) * -0.10))

    build_blind_box("Shelf_Crate_1", "WoodCrate_Old", (-2.02, -1.10, 1.48), mats, col, parent=root)
    build_blind_box("Shelf_Chest_2", "BrassChest_Steam", (-2.02, 1.10, 1.48), mats, col, parent=root)
    add_cyl("Shelf_Potion_1", root, (-2.02, 0.10, 1.60), 0.10, 0.24, mats["jade"], col=col)
    add_cyl("Shelf_SoulJar_2", root, (-2.02, -0.42, 1.60), 0.09, 0.22, mats["arcane_glow"], col=col)
    add_cyl("Shelf_Scroll_2", root, (-2.02, -0.35, 2.14), 0.06, 0.34, mats["parchment"], rot=(math.pi / 2, 0, 0), col=col)
    add_point_light("CounterSpot", root, (0.35, 0, 2.20), (1.0, 0.85, 0.58), 55.0, radius=0.3, col=col)
    return root


def build_openbox_house_unique(loc, mats, font_data, rng, col):
    """
    House 2 (Chrono-Artisan Appraisal Workshop — Steampunk x Sci-Fi):
    - Asymmetric Brass & Glass Observatory Dome bursting from upper roof + Giant Interlocking Clockwork Gears!
    - Riveted Iron/Brass Portal Arch + Glowing Cyan Aether Condenser Tank & Steam Pipes.
    - Interior: Weathered Industrial Stone Slab Floor + Recessed Glowing Sub-Floor Chrono-Gear Grate!
    """
    root = _build_house_shell(
        "Shop_OpenBox_House", loc, False,
        ext_low_mat=mats["brick_glazed_teal"],
        ext_up_mat=mats["ashlar_stone"],
        int_low_mat=mats["brick_glazed_teal"],
        int_up_mat=mats["brick_whitewash"],
        roof_mat=mats["roof_slate"],
        sign_title="TIỆM MỞ & SOI",
        sign_sub="KHUI HỘP · ĐỊNH GIÁ",
        sign_z=3.45,
        mats=mats, font_data=font_data, rng=rng, col=col,
    )

    # Interior Floor: Weathered Oil-Stained Stone Slabs + Recessed Glowing Sub-Floor Chrono-Gear Grate!
    add_box("Workshop_BaseSlab", root, (0, 0, 0.232), (4.85, 5.75, 0.025), mats["workshop_floor"], bevel=0.005, col=col)
    # Individual beveled stone slabs around the floor perimeter for subtle physical relief
    for sx in (-1.75, -0.85, 0.05, 1.95):
        for sy in (-2.1, -1.05, 1.05, 2.1):
            add_box(f"StoneTile_{sx}_{sy}", root, (sx, sy, 0.246 + rng.uniform(-0.003, 0.003)),
                    (0.84, 0.96, 0.018), mats["workshop_floor"], rot=(0, 0, rng.uniform(-0.01, 0.01)), bevel=0.006, col=col)

    # Recessed Sub-Floor Chrono-Gear Viewport Pit at X = +0.95, Y = 0
    add_cyl("FloorPit_DarkBase", root, (0.95, 0, 0.244), 0.78, 0.014, mats["obsidian"], verts=28, col=col)
    add_torus_ring("FloorPit_BrassRim", root, (0.95, 0, 0.264), 0.78, 0.032, mats["brass"], major_segs=32, col=col)
    add_torus_ring("FloorPit_CyanConduit", root, (0.95, 0, 0.254), 0.58, 0.020, mats["cyan_glow"], major_segs=32, col=col)
    add_gear_wheel("FloorPit_GearA", root, (0.78, -0.15, 0.256), 0.34, 0.020, 12, mats["brass"], col=col)
    add_gear_wheel("FloorPit_GearB", root, (1.16, 0.20, 0.256), 0.24, 0.020, 10, mats["gold"], col=col)
    # Bolted brass floor grate bars over the pit
    for gx in (-0.35, 0.0, 0.35):
        add_box(f"FloorGrateBar_{gx}", root, (0.95 + gx, 0, 0.274), (0.04, 1.48, 0.012), mats["iron"], bevel=0.003, col=col)

    # Exterior Asymmetric Brass & Glass Observatory Dome bursting out of front-left roof!
    dome = bpy.data.objects.new("Observatory_Dome", None)
    dome.location = Vector((0.85, -2.05, 4.35))
    dome.parent = root
    link_obj(dome, col)
    add_cyl("Dome_Base", dome, (0, 0, 0), 1.05, 0.75, mats["iron"], verts=16, bevel=0.015, col=col)
    add_sphere("Dome_Glass", dome, (0, 0, 0.38), 0.96, mats["glass"], scale=(1.0, 1.0, 0.92), segs=24, col=col)
    add_sphere("Dome_InnerAether", dome, (0, 0, 0.42), 0.36, mats["cyan_glow"], segs=16, col=col)
    add_torus_ring("Dome_RibA", dome, (0, 0, 0.38), 0.97, 0.04, mats["brass"], rot=(math.pi / 2, 0, 0.4), col=col)
    add_torus_ring("Dome_RibB", dome, (0, 0, 0.38), 0.97, 0.04, mats["brass"], rot=(math.pi / 2, 0, 1.9), col=col)
    # Telescope/Brass Lens protruding toward the plaza!
    add_cyl("Dome_Telescope", dome, (0.85, 0.25, 0.65), 0.16, 1.15, mats["brass"], rot=(0, math.radians(68), 0.3), col=col)

    # Giant Interlocking Clockwork Gears mounted cleanly proud of the Front Tie Beam!
    add_gear_wheel("Ext_GableGear_Big", root, (2.63, 1.52, 3.92), 0.62, 0.08, 12, mats["brass"], rot=(0, math.pi / 2, 0.2), col=col)
    add_gear_wheel("Ext_GableGear_Small", root, (2.66, 2.25, 3.42), 0.38, 0.08, 9, mats["gold"], rot=(0, math.pi / 2, 0.5), col=col)

    # Riveted Iron Portal Arch + Glowing Cyan Conduit + Glass-Brass Entrance Canopy
    add_box("PortalArch_L", root, (2.52, -1.24, 1.56), (0.20, 0.18, 2.75), mats["iron"], bevel=0.012, col=col)
    add_box("PortalArch_R", root, (2.52, 1.24, 1.56), (0.20, 0.18, 2.75), mats["iron"], bevel=0.012, col=col)
    add_box("Portal_CyanStripL", root, (2.63, -1.24, 1.60), (0.03, 0.04, 2.40), mats["cyan_glow"], bevel=0.004, col=col)
    add_box("Portal_CyanStripR", root, (2.63, 1.24, 1.60), (0.03, 0.04, 2.40), mats["cyan_glow"], bevel=0.004, col=col)

    add_box("Canopy_Glass", root, (2.92, 0, 2.90), (0.92, 2.65, 0.045), mats["glass"], rot=(0, math.radians(14), 0), bevel=0.005, col=col)
    add_box("Canopy_RimF", root, (3.35, 0, 2.79), (0.06, 2.72, 0.07), mats["brass"], bevel=0.006, col=col)
    for py in (-1.28, 1.28):
        add_box(f"Canopy_Strut_{py}", root, (2.88, py, 2.90), (0.94, 0.06, 0.06), mats["iron"], rot=(0, math.radians(14), 0), bevel=0.005, col=col)

    # Glowing Cyan Aether Condenser Tank + Brass Steam Pipes & Pressure Gauge
    add_cyl("AetherTank_Base", root, (2.68, 2.12, 0.45), 0.24, 0.45, mats["iron"], col=col)
    add_cyl("AetherTank_Core", root, (2.68, 2.12, 1.05), 0.18, 0.80, mats["cyan_glow"], col=col)
    add_cyl("AetherTank_Cap", root, (2.68, 2.12, 1.50), 0.24, 0.14, mats["brass"], col=col)
    add_cyl("Ext_SteamPipeV", root, (2.56, 1.75, 1.95), 0.045, 2.4, mats["brass"], col=col)
    add_cyl("Ext_SteamPipeH", root, (2.56, 2.25, 2.65), 0.04, 1.05, mats["brass"], rot=(math.pi / 2, 0, 0), col=col)
    add_cyl("Ext_SteamGauge", root, (2.61, 1.75, 2.20), 0.11, 0.05, mats["brass"], rot=(0, math.pi / 2, 0), col=col)
    add_cyl("Ext_SteamDial", root, (2.64, 1.75, 2.20), 0.08, 0.02, mats["dial_white"], rot=(0, math.pi / 2, 0), col=col)

    add_moss_cluster("House2_Moss_1", root, (2.58, -2.65, 0.22), (0.7, 0.8, 0.6), mats, rng, col)

    # Interior: Toolboard nudged cleanly away from window (X = -1.58) + Overhead Gantry Rail!
    add_box("Toolboard_Panel", root, (-1.58, 2.78, 1.95), (1.28, 0.06, 0.85), mats["wood_dark"], bevel=0.008, col=col)
    add_box("Blueprint_Sheet", root, (-1.82, 2.74, 2.02), (0.60, 0.02, 0.48), mats["blueprint"], bevel=0.003, col=col)
    add_gear_wheel("Wall_Gear_1", root, (-1.22, 2.73, 2.08), 0.14, 0.03, 8, mats["brass"], rot=(math.pi / 2, 0, 0), col=col)
    add_gear_wheel("Wall_Gear_2", root, (-1.05, 2.73, 1.82), 0.09, 0.03, 6, mats["gold"], rot=(math.pi / 2, 0, 0), col=col)

    add_box("Gantry_Beam", root, (-0.55, 0, 3.12), (0.14, 3.2, 0.14), mats["iron"], bevel=0.008, col=col)
    add_cyl("Gantry_Hook", root, (-0.48, 0.10, 2.82), 0.025, 0.55, mats["brass"], col=col)

    add_box("BackShelf_PostL", root, (-2.05, -1.85, 1.32), (0.45, 0.08, 2.15), mats["iron"], bevel=0.008, col=col)
    add_box("BackShelf_PostR", root, (-2.05, 1.85, 1.32), (0.45, 0.08, 2.15), mats["iron"], bevel=0.008, col=col)
    for sz in (0.85, 1.45, 2.05):
        add_box(f"BackShelf_Plank_{sz}", root, (-2.05, 0, sz), (0.42, 3.65, 0.05), mats["wood_dark"], bevel=0.008, col=col)

    for lx in (-0.98, -0.12):
        for ly in (-1.18, 1.18):
            add_box(f"BenchLeg_{lx}_{ly}", root, (lx, ly, 0.64), (0.12, 0.12, 0.82), mats["iron"], bevel=0.01, col=col)
    add_box("BenchTop", root, (-0.55, 0, 1.08), (1.12, 2.65, 0.09), mats["wood_light"], bevel=0.015, col=col)
    add_box("VelvetMat", root, (-0.48, 0.10, 1.132), (0.58, 0.82, 0.018), mats["velvet"], bevel=0.006, col=col)

    add_cyl("LampBase", root, (-0.82, 0.85, 1.14), 0.11, 0.03, mats["brass"], col=col)
    add_cyl("LampStem", root, (-0.82, 0.85, 1.36), 0.018, 0.42, mats["brass"], col=col)
    add_cone_frustum("LampShade", root, (-0.66, 0.68, 1.56), 0.12, 0.05, 0.14, mats["brass"], rot=(0.45, 0.45, 0), col=col)
    add_sphere("LampBulb", root, (-0.64, 0.65, 1.52), 0.04, mats["warm_glow"], col=col)
    add_spot_light("WorkbenchLampSpot", root, (-0.62, 0.62, 1.50), (-0.48, 0.10, 1.14), (1.0, 0.90, 0.72), 95.0, spot_deg=60, col=col)

    add_cyl("MagLens", root, (-0.25, -0.45, 1.135), 0.075, 0.012, mats["glass"], col=col)
    add_cyl("MagRim", root, (-0.25, -0.45, 1.135), 0.082, 0.016, mats["brass"], col=col)
    add_cyl("MagHandle", root, (-0.16, -0.54, 1.135), 0.014, 0.14, mats["wood_dark"], rot=(math.pi / 2, 0, -0.78), col=col)

    w_ch = build_artifact("Art_Chalice", "Chalice_Royal", (-0.48, 0.10, 1.141), mats, col, parent=root, scale=1.35)
    w_ch.rotation_euler = Euler((0, 0, math.pi))
    w_wa = build_artifact("Art_Watch", "PocketWatch_Brass", (-0.36, -0.18, 1.141), mats, col, parent=root, scale=1.15)
    w_wa.rotation_euler = Euler((0, 0, math.pi * 0.85))
    w_co = build_artifact("Art_Compass", "Compass_Starlight", (-0.62, -0.22, 1.141), mats, col, parent=root, scale=1.15)
    w_co.rotation_euler = Euler((0, 0, math.pi))
    build_blind_box("ReadyBox_OnBench", "BrassChest_Steam", (-0.52, -0.92, 1.125), mats, col, parent=root)

    build_artifact("Shelf_HoloCube", "HoloCube_SciFi", (-2.00, -1.10, 1.475), mats, col, parent=root, scale=1.35)
    s_mask = build_artifact("Shelf_RunicMask", "RunicMask_Arcane", (-2.00, -0.30, 1.475), mats, col, parent=root, scale=1.35)
    s_mask.rotation_euler = Euler((0, 0, math.pi))
    s_bird = build_artifact("Shelf_Bird", "AutomatonBird_Brass", (-2.00, 0.45, 1.475), mats, col, parent=root, scale=1.45)
    s_bird.rotation_euler = Euler((0, 0, math.pi * 0.85))
    build_artifact("Shelf_VoidPrism", "VoidPrism_Alien", (-2.00, 1.15, 1.475), mats, col, parent=root, scale=1.40)
    return root


# ==================== 4. SURROUNDING INTERSPERSED BUILDINGS ====================

def build_surrounding_town(mats, rng, col):
    flanking_specs = [
        ("Bldg_NW_Brick", (-13.5, 9.5, 0.10), (6.0, 5.5, 5.2), mats["brick_exposed"], mats["roof_slate"]),
        ("Bldg_NW_Timber", (-7.6, 10.5, 0.10), (4.8, 4.2, 4.0), mats["plaster_alt"], mats["roof_tile"]),
        ("Bldg_NE_Stone", (13.5, 9.5, 0.10), (6.0, 5.5, 5.6), mats["ashlar_stone"], mats["roof_tile"]),
        ("Bldg_NE_Teal", (7.6, 10.5, 0.10), (4.8, 4.2, 4.2), mats["brick_glazed_teal"], mats["roof_slate"]),
        ("Bldg_W_Alley", (-14.5, 0.0, 0.10), (4.5, 6.5, 4.8), mats["plaster_weathered"], mats["roof_slate"]),
        ("Bldg_E_Alley", (14.5, 0.0, 0.10), (4.5, 6.5, 4.6), mats["brick_exposed"], mats["roof_tile"]),
        ("Bldg_SW_Side", (-12.5, -8.5, 0.10), (5.5, 5.0, 4.4), mats["plaster_weathered"], mats["roof_tile"]),
        ("Bldg_SE_Side", (12.5, -8.5, 0.10), (5.5, 5.0, 4.9), mats["ashlar_stone"], mats["roof_slate"]),
        ("Tower_North_Landmark", (0, 17.0, 0.0), (6.2, 6.2, 10.5), mats["ashlar_stone"], mats["roof_slate"]),
    ]

    for bname, loc, sz, wall_m, roof_m in flanking_specs:
        bx, by, bz = loc
        bw, bd, bh = sz
        broot = bpy.data.objects.new(bname, None)
        broot.location = Vector((bx, by, bz))
        link_obj(broot, col)
        add_box("Plinth", broot, (0, 0, 0.45), (bw + 0.15, bd + 0.15, 0.90), mats["wet_stone"], bevel=0.02, col=col)
        add_box("Walls", broot, (0, 0, 0.90 + (bh - 0.90) * 0.5), (bw, bd, bh - 0.90), wall_m, bevel=0.02, col=col)
        add_box("Eave", broot, (0, 0, bh + 0.08), (bw + 0.35, bd + 0.35, 0.16), mats["wood_dark"], bevel=0.02, col=col)
        add_cone_frustum("PitchedRoof", broot, (0, 0, bh + 0.72), max(bw, bd) * 0.74, 0.25, 1.15, roof_m, rot=(0, 0, math.pi / 4), verts=4, smooth=False, col=col)
        # Add subtle timber braces & moss at base of surrounding houses
        add_moss_cluster(f"{bname}_Moss", broot, (0, -bd * 0.52, 0.12), (1.0, 0.7, 0.7), mats, rng, col)

    # Multiverse crystals on North Tower Landmark
    add_box("Tower_RuneBand", None, (0, 13.85, 5.5), (4.2, 0.18, 0.25), mats["arcane_glow"], bevel=0.01, col=col)

    for sx, sy in ((-3.8, 5.2), (3.8, 5.2), (-3.8, -5.2), (3.8, -5.2)):
        add_cyl(f"StreetBase_{sx}_{sy}", None, (sx, sy, 0.18), 0.14, 0.36, mats["stone_dark"], verts=8, col=col)
        add_cyl(f"StreetPost_{sx}_{sy}", None, (sx, sy, 1.55), 0.055, 2.6, mats["iron"], col=col)
        add_cyl(f"StreetLantern_{sx}_{sy}", None, (sx, sy, 2.95), 0.13, 0.24, mats["warm_glow"], verts=6, col=col)
        add_cone_frustum(f"StreetCap_{sx}_{sy}", None, (sx, sy, 3.16), 0.20, 0.02, 0.18, mats["iron"], verts=6, smooth=False, col=col)
        add_point_light(f"StreetLight_{sx}_{sy}", None, (sx, sy, 2.95), (1.0, 0.78, 0.48), 55.0, radius=0.3, col=col)


# ==================== 5. LIGHTING & CAMERAS ====================

def setup_world_and_sun(col):
    world = bpy.data.worlds.new("PlazaWorld")
    bpy.context.scene.world = world
    world.use_nodes = True
    bg = world.node_tree.nodes.get("Background")
    if bg:
        bg.inputs["Color"].default_value = (0.24, 0.30, 0.42, 1.0)
        bg.inputs["Strength"].default_value = 0.70

    sun_data = bpy.data.lights.new(name="Sun_GoldenHour", type="SUN")
    sun_data.color = (1.0, 0.88, 0.72)
    sun_data.energy = 3.2
    sun_obj = bpy.data.objects.new("Sun_GoldenHour", sun_data)
    sun_obj.rotation_euler = Euler((math.radians(48), math.radians(-20), math.radians(-35)))
    link_obj(sun_obj, col)


def render_camera(name, loc, target, lens_mm, out_path):
    cam_data = bpy.data.cameras.new(name)
    cam_data.lens = lens_mm
    cam_obj = bpy.data.objects.new(name, cam_data)
    cam_obj.location = Vector(loc)
    direction = Vector(target) - Vector(loc)
    cam_obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    bpy.context.scene.collection.objects.link(cam_obj)
    bpy.context.scene.camera = cam_obj
    bpy.context.scene.render.filepath = out_path
    bpy.ops.render.render(write_still=True)


def main():
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    clear_scene()
    rng = random.Random(20261123)

    col_env = get_or_create_collection("Environment")
    col_shops = get_or_create_collection("Shops")

    mats = create_materials()
    font_data = load_unicode_font()

    setup_world_and_sun(col_env)
    build_street_and_plaza(mats, rng, col_env)
    build_surrounding_town(mats, rng, col_env)
    build_fountain(mats, rng, col_env)

    build_blindbox_house_unique((-7.2, 0.0, 0.0), mats, font_data, rng, col_shops)
    build_openbox_house_unique((7.2, 0.0, 0.0), mats, font_data, rng, col_shops)

    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 32
    scene.cycles.use_denoising = True
    scene.render.resolution_x = 960
    scene.render.resolution_y = 540
    scene.render.image_settings.file_format = "PNG"

    cameras = [
        ("Cam_Plaza_Overview", (0.0, -14.0, 6.8), (0.0, 1.5, 2.0), 25.0, "preview_1_plaza_overview.png"),
        ("Cam_BlindBox_Exterior", (-0.5, -5.2, 2.6), (-5.5, -0.5, 2.7), 28.0, "preview_2_blindbox_shop_exterior.png"),
        ("Cam_BlindBox_Interior", (-5.05, -1.75, 1.85), (-7.25, 0.15, 0.85), 24.0, "preview_3_blindbox_shop_interior.png"),
        ("Cam_OpenBox_Exterior", (0.5, -5.2, 2.6), (5.5, -0.5, 2.7), 28.0, "preview_4_openbox_shop_exterior.png"),
        ("Cam_OpenBox_Interior", (5.05, 1.75, 1.85), (7.25, -0.15, 0.85), 24.0, "preview_5_openbox_shop_interior.png"),
    ]

    for cname, cloc, ctarget, clens, fname in cameras:
        out_path = os.path.join(OUTPUT_DIR, fname)
        render_camera(cname, cloc, ctarget, clens, out_path)
        print("RENDERED:", out_path)


if __name__ == "__main__":
    main()

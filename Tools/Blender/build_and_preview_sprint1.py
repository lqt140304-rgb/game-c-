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


def make_hanoi_terrazzo_paving():
    """Authentic Vietnamese grey grooved sidewalk terrazzo pavers."""
    mat, nt, bsdf = _new_bsdf_mat("Mat_HanoiTerrazzo")
    if nt is None:
        return mat
    geom = nt.nodes.new("ShaderNodeNewGeometry")

    brick = nt.nodes.new("ShaderNodeTexBrick")
    brick.inputs["Color1"].default_value = (0.56, 0.55, 0.52, 1.0)
    brick.inputs["Color2"].default_value = (0.48, 0.47, 0.44, 1.0)
    brick.inputs["Mortar"].default_value = (0.24, 0.23, 0.22, 1.0)
    brick.inputs["Scale"].default_value = 2.4
    brick.inputs["Brick Width"].default_value = 0.50
    brick.inputs["Row Height"].default_value = 0.50
    brick.inputs["Mortar Size"].default_value = 0.018

    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 18.0
    noise.inputs["Detail"].default_value = 7.0

    mix = nt.nodes.new("ShaderNodeMix")
    mix.data_type = "RGBA"
    mix.blend_type = "MULTIPLY"
    mix.inputs[0].default_value = 0.20
    mix.inputs[7].default_value = (0.82, 0.80, 0.76, 1.0)

    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.28
    bump.inputs["Distance"].default_value = 0.014
    bump.invert = True

    nt.links.new(geom.outputs["Position"], brick.inputs["Vector"])
    nt.links.new(geom.outputs["Position"], noise.inputs["Vector"])
    nt.links.new(brick.outputs["Color"], mix.inputs[6])
    nt.links.new(mix.outputs[2], bsdf.inputs["Base Color"])
    nt.links.new(brick.outputs["Fac"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    bsdf.inputs["Roughness"].default_value = 0.84
    return mat


def make_vn_red_terrazzo():
    """Iconic Vietnamese terracotta-red sidewalk terrazzo tiles."""
    mat, nt, bsdf = _new_bsdf_mat("Mat_VnRedTile")
    if nt is None:
        return mat
    geom = nt.nodes.new("ShaderNodeNewGeometry")

    brick = nt.nodes.new("ShaderNodeTexBrick")
    brick.inputs["Color1"].default_value = (0.54, 0.22, 0.16, 1.0)
    brick.inputs["Color2"].default_value = (0.46, 0.19, 0.14, 1.0)
    brick.inputs["Mortar"].default_value = (0.25, 0.17, 0.14, 1.0)
    brick.inputs["Scale"].default_value = 2.6
    brick.inputs["Brick Width"].default_value = 0.45
    brick.inputs["Row Height"].default_value = 0.45
    brick.inputs["Mortar Size"].default_value = 0.020

    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.30
    bump.inputs["Distance"].default_value = 0.014
    bump.invert = True

    nt.links.new(geom.outputs["Position"], brick.inputs["Vector"])
    nt.links.new(brick.outputs["Color"], bsdf.inputs["Base Color"])
    nt.links.new(brick.outputs["Fac"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    bsdf.inputs["Roughness"].default_value = 0.84
    return mat


def make_vn_street_matte():
    """Matte natural dark Hanoi pedestrian walkway stone/asphalt (non-glaring)."""
    mat, nt, bsdf = _new_bsdf_mat("Mat_VnStreetMatte")
    if nt is None:
        return mat
    geom = nt.nodes.new("ShaderNodeNewGeometry")

    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 14.0
    noise.inputs["Detail"].default_value = 8.0

    ramp = nt.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].position = 0.25
    ramp.color_ramp.elements[0].color = (0.21, 0.21, 0.22, 1.0)
    ramp.color_ramp.elements[1].position = 0.78
    ramp.color_ramp.elements[1].color = (0.31, 0.30, 0.29, 1.0)

    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.24
    bump.inputs["Distance"].default_value = 0.015

    nt.links.new(geom.outputs["Position"], noise.inputs["Vector"])
    nt.links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
    nt.links.new(ramp.outputs["Color"], bsdf.inputs["Base Color"])
    nt.links.new(noise.outputs["Fac"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    bsdf.inputs["Roughness"].default_value = 0.88
    return mat


def make_foliage_pbr():
    mat, nt, bsdf = _new_bsdf_mat("Mat_TreeFoliage")
    if nt is None:
        return mat
    geom = nt.nodes.new("ShaderNodeNewGeometry")
    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 6.5
    noise.inputs["Detail"].default_value = 7.0
    ramp = nt.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].position = 0.25
    ramp.color_ramp.elements[0].color = (0.07, 0.26, 0.08, 1.0)
    ramp.color_ramp.elements[1].position = 0.78
    ramp.color_ramp.elements[1].color = (0.19, 0.46, 0.14, 1.0)
    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.45
    bump.inputs["Distance"].default_value = 0.04
    nt.links.new(geom.outputs["Position"], noise.inputs["Vector"])
    nt.links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
    nt.links.new(ramp.outputs["Color"], bsdf.inputs["Base Color"])
    nt.links.new(noise.outputs["Fac"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    bsdf.inputs["Roughness"].default_value = 0.72
    return mat


def create_materials():
    return {
        "cobble": make_cobble_pbr(),
        "terrazzo_paving": make_hanoi_terrazzo_paving(),
        "vn_red_tile": make_vn_red_terrazzo(),
        "vn_street_matte": make_vn_street_matte(),
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
        "sign_gold": make_simple_mat("Mat_SignGold", (0.94, 0.78, 0.30), roughness=0.35, metallic=0.35, emission=(0.94, 0.78, 0.30), emission_strength=0.45),
        "sign_sub": make_simple_mat("Mat_SignSub", (0.92, 0.88, 0.80), roughness=0.60, emission=(0.86, 0.82, 0.72), emission_strength=0.28),
        "iron": make_simple_mat("Mat_Iron", (0.13, 0.14, 0.16), roughness=0.50, metallic=0.85),
        "velvet": make_simple_mat("Mat_Velvet", (0.42, 0.04, 0.07), roughness=0.92),
        "carpet_gold": make_simple_mat("Mat_CarpetGold", (0.68, 0.50, 0.16), roughness=0.86),
        "wax_red": make_simple_mat("Mat_WaxRed", (0.66, 0.07, 0.09), roughness=0.42),
        "dial_white": make_simple_mat("Mat_DialWhite", (0.92, 0.89, 0.81), roughness=0.38),
        "parchment": make_simple_mat("Mat_Parchment", (0.82, 0.74, 0.57), roughness=0.86),
        "blueprint": make_simple_mat("Mat_Blueprint", (0.11, 0.24, 0.44), roughness=0.76),
        "jade": make_simple_mat("Mat_Jade", (0.11, 0.55, 0.36), roughness=0.22, metallic=0.05),
        "amber": make_simple_mat("Mat_Amber", (0.86, 0.45, 0.06), roughness=0.14, emission=(0.86, 0.45, 0.06), emission_strength=0.35),
        "obsidian": make_simple_mat("Mat_Obsidian", (0.05, 0.04, 0.08), roughness=0.12, metallic=0.25),
        "water": make_simple_mat("Mat_Water", (0.09, 0.56, 0.64), roughness=0.06, metallic=0.10, emission=(0.10, 0.55, 0.68), emission_strength=0.45),
        "cyan_glow": make_simple_mat("Mat_CyanGlow", (0.20, 0.85, 1.0), roughness=0.2, emission=(0.20, 0.85, 1.0), emission_strength=1.8),
        "arcane_glow": make_simple_mat("Mat_ArcaneGlow", (0.62, 0.28, 0.96), roughness=0.2, emission=(0.62, 0.28, 0.96), emission_strength=1.8),
        "warm_glow": make_simple_mat("Mat_WarmGlow", (1.0, 0.82, 0.48), roughness=0.3, emission=(1.0, 0.78, 0.40), emission_strength=2.2),
        "glass": make_simple_mat("Mat_Glass", (0.68, 0.84, 0.92), roughness=0.10, metallic=0.10),
        # Modern Hanoi materials
        "wall_cream": make_simple_mat("Mat_HanoiCreamWall", (0.88, 0.85, 0.79), roughness=0.84),
        "wall_yellow": make_simple_mat("Mat_HanoiYellowWall", (0.86, 0.73, 0.42), roughness=0.84),
        "steel_black": make_simple_mat("Mat_SteelBlack", (0.12, 0.13, 0.15), roughness=0.52, metallic=0.72),
        "inox_steel": make_simple_mat("Mat_InoxSteel", (0.74, 0.77, 0.80), roughness=0.28, metallic=0.90),
        "plastic_blue": make_simple_mat("Mat_PlasticBlue", (0.10, 0.36, 0.80), roughness=0.42),
        "plastic_red": make_simple_mat("Mat_PlasticRed", (0.78, 0.16, 0.15), roughness=0.42),
        "sign_red": make_simple_mat("Mat_HanoiRedSign", (0.66, 0.10, 0.10), roughness=0.55, emission=(0.60, 0.08, 0.08), emission_strength=0.22),
        "sign_blue": make_simple_mat("Mat_HanoiBlueSign", (0.10, 0.24, 0.54), roughness=0.55, emission=(0.08, 0.20, 0.48), emission_strength=0.20),
        "tree_bark": make_wood_pbr("Mat_TreeBark", (0.24, 0.17, 0.12), (0.14, 0.10, 0.07), roughness=0.88, bump_str=0.45),
        "tree_foliage": make_foliage_pbr(),
        "steam_mist": make_simple_mat("Mat_SteamMist", (0.94, 0.96, 0.98), roughness=0.45, emission=(0.90, 0.94, 0.98), emission_strength=0.65),
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


# ==================== 1. VIETNAMESE SIDEWALK TERRAZZO & HANOI CENTER PARK ISLAND ====================

def build_street_and_plaza(mats, rng, col):
    # Matte natural dark urban pedestrian street base (90m x 96m)
    add_box("Cobble_Road", None, (0, 0, -0.05), (90.0, 96.0, 0.10), mats["vn_street_matte"], bevel=0.0, col=col)

    # Subtle granite paver border & recessed tactile paving bands along pedestrian center lane
    for lx in (-2.95, 2.95):
        add_box(f"LaneBorder_{lx}", None, (lx, 0, 0.006), (0.36, 48.0, 0.012), mats["ashlar_stone"], bevel=0.004, col=col)

    # Classic Vietnamese Red-and-Grey Grooved Terrazzo Sidewalks + Curbs + Storm Drain Grates
    for side_name, sx, curb_x, red_x, drain_x in (
        ("Sidewalk_West", -10.5, -4.38, -7.2, -4.18),
        ("Sidewalk_East", 10.5, 4.38, 7.2, 4.18),
    ):
        add_box(side_name, None, (sx, 0, 0.045), (12.2, 56.0, 0.09), mats["terrazzo_paving"], bevel=0.010, col=col)
        # Signature terracotta-red Vietnamese sidewalk tile runner
        add_box(f"{side_name}_RedStrip", None, (red_x, 0, 0.092), (3.8, 42.0, 0.012), mats["vn_red_tile"], bevel=0.004, col=col)
        for i in range(-22, 23):
            cy = i * 0.72
            m = mats["ashlar_stone"] if i % 3 != 0 else mats["stone_dark"]
            add_box(f"{side_name}_Curb_{i}", None, (curb_x, cy, 0.068), (0.24, 0.69, 0.14), m, bevel=0.010, col=col)
        for dy in (-10.0, -3.5, 3.5, 10.0):
            add_box(f"{side_name}_Drain_{dy}", None, (drain_x, dy, 0.008), (0.34, 0.65, 0.016), mats["steel_black"], bevel=0.003, col=col)


def _add_vn_park_bench(name, parent, loc, rot_z, mats, col):
    """Authentic Vietnamese Terrazzo Park Bench (Ghế đá công viên Hà Nội)."""
    b = bpy.data.objects.new(name, None)
    b.location = Vector(loc)
    b.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        b.parent = parent
    link_obj(b, col)
    # Side leg supports
    for ly in (-0.58, 0.58):
        add_box(f"Leg_{ly}", b, (0, ly, 0.22), (0.46, 0.09, 0.44), mats["terrazzo_paving"], bevel=0.01, col=col)
    # Smooth seat slab & curved-backrest slab
    add_box("SeatSlab", b, (0.04, 0, 0.46), (0.48, 1.48, 0.07), mats["dial_white"], bevel=0.012, col=col)
    add_box("BackSlab", b, (-0.18, 0, 0.78), (0.07, 1.48, 0.42), mats["dial_white"], rot=(0, -0.18, 0), bevel=0.012, col=col)
    return b


def build_hanoi_center_island(mats, font_data, rng, col):
    """Modern Hanoi Pedestrian Park Island replacing old fountain: tree planter, stone benches & neighborhood bulletin board."""
    root = bpy.data.objects.new("Hanoi_CenterIsland", None)
    link_obj(root, col)

    # Raised Granite & Red-Brick Park Island Planter Bed
    add_box("Island_Apron", root, (0, 0, 0.025), (3.8, 5.6, 0.05), mats["vn_red_tile"], bevel=0.012, col=col)
    add_box("Island_Curb", root, (0, 0, 0.19), (2.7, 4.5, 0.34), mats["ashlar_stone"], bevel=0.015, col=col)
    add_box("Island_BrickBand", root, (0, 0, 0.17), (2.74, 4.54, 0.16), mats["brick_exposed"], bevel=0.008, col=col)
    add_box("Island_Soil", root, (0, 0, 0.31), (2.3, 4.1, 0.12), mats["wood_dark"], bevel=0.008, col=col)

    # Central Lush Shade Tree (Cây Sấu Hà Nội) anchored right in the median
    add_cone_frustum("Trunk_Low", root, (0, 0.15, 1.35), 0.24, 0.17, 2.25, mats["tree_bark"], verts=14, col=col)
    add_cone_frustum("Trunk_Mid", root, (0.02, 0.17, 2.85), 0.16, 0.10, 1.25, mats["tree_bark"], rot=(0.04, -0.03, 0), verts=14, col=col)
    for b_idx, (bx, by, bz, brot) in enumerate((
        (-0.36, -0.12, 3.25, (0.42, -0.45, 0.3)),
        (0.38, 0.36, 3.30, (-0.36, 0.45, 0.8)),
        (0.06, -0.24, 3.38, (0.50, 0.12, -0.4)),
    )):
        add_cone_frustum(f"Branch_{b_idx}", root, (bx, by, bz), 0.09, 0.035, 1.20, mats["tree_bark"], rot=brot, verts=10, col=col)
    add_sphere("TreeCanopy_Main", root, (0.04, 0.16, 4.45), 1.56, mats["tree_foliage"], scale=(1.15, 1.15, 0.80), segs=16, col=col)
    add_sphere("TreeCanopy_L", root, (-0.98, -0.35, 3.95), 0.98, mats["tree_foliage"], scale=(1.12, 1.05, 0.80), segs=14, col=col)
    add_sphere("TreeCanopy_R", root, (1.02, 0.68, 4.05), 1.05, mats["tree_foliage"], scale=(1.15, 1.08, 0.78), segs=14, col=col)
    add_sphere("TreeCanopy_Top", root, (-0.10, 0.55, 5.10), 0.95, mats["tree_foliage"], scale=(1.08, 1.05, 0.76), segs=14, col=col)

    # Low Ornamental Bushes inside the Median Bed
    for bi, (bx, by, br) in enumerate(((-0.55, -1.25, 0.42), (0.60, -1.10, 0.38), (-0.52, 1.32, 0.36), (0.58, 1.28, 0.40))):
        add_sphere(f"Bush_{bi}", root, (bx, by, 0.54), br, mats["tree_foliage"], scale=(1.2, 1.1, 0.75), segs=10, col=col)

    # Two Authentic Vietnamese Stone Park Benches (Ghế đá công viên) along the Island edges
    _add_vn_park_bench("GheDa_West", root, (-1.45, -0.35, 0.04), math.pi, mats, col)
    _add_vn_park_bench("GheDa_East", root, (1.45, 0.35, 0.04), 0.0, mats, col)

    # Classic Hanoi Neighborhood Notice Board ("BẢNG TIN PHỐ HÀ NỘI") at South end of Island facing incoming player (-Y)!
    add_cyl("Board_PostL", root, (-0.65, -1.78, 1.15), 0.045, 1.95, mats["steel_black"], col=col)
    add_cyl("Board_PostR", root, (0.65, -1.78, 1.15), 0.045, 1.95, mats["steel_black"], col=col)
    add_box("Board_Frame", root, (0, -1.78, 1.48), (1.42, 0.10, 0.96), mats["steel_black"], bevel=0.01, col=col)
    add_box("Board_Backing", root, (0, -1.815, 1.48), (1.30, 0.05, 0.84), mats["sign_red"], bevel=0.005, col=col)
    add_box("Board_Roof", root, (0, -1.80, 2.04), (1.58, 0.42, 0.14), mats["roof_tile"], rot=(0.22, 0, 0), bevel=0.01, col=col)
    add_3d_text("Board_Title", root, "BẢNG TIN PHỐ HÀ NỘI", (0, -1.845, 1.72), (math.pi / 2, 0, 0), 0.092, mats["sign_gold"], font_data=font_data, extrude=0.003, col=col)
    add_3d_text("Board_Sub", root, "KHU PHỐ VĂN MINH · SẠCH ĐẸP", (0, -1.845, 1.56), (math.pi / 2, 0, 0), 0.054, mats["sign_sub"], font_data=font_data, extrude=0.002, col=col)
    for ni, nx in enumerate((-0.38, 0.0, 0.38)):
        add_box(f"NoticeSheet_{ni}", root, (nx, -1.848, 1.24), (0.30, 0.02, 0.38), mats["dial_white"], bevel=0.002, col=col)

    # Low Steel Pedestrian Bollards at corners of Island
    for bx in (-1.75, 1.75):
        for by in (-2.55, 2.55):
            add_cyl(f"Bollard_{bx}_{by}", root, (bx, by, 0.36), 0.065, 0.68, mats["steel_black"], verts=12, col=col)
            add_cyl(f"BollardBand_{bx}_{by}", root, (bx, by, 0.58), 0.068, 0.08, mats["gold"], verts=12, col=col)
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


# ==================== 3. MODERN HANOI SHOPHOUSES (WALK-IN + MULTIVERSE ACCENTS) ====================

def _add_ac_unit(name, parent, loc, mats, col, rot_z=0.0):
    ac = bpy.data.objects.new(name, None)
    ac.location = Vector(loc)
    ac.rotation_euler = Euler((0, 0, rot_z))
    if parent:
        ac.parent = parent
    link_obj(ac, col)
    add_box("Case", ac, (0, 0, 0), (0.28, 0.72, 0.48), mats["dial_white"], bevel=0.015, col=col)
    add_cyl("FanGrille", ac, (0.145, -0.14, 0), 0.17, 0.02, mats["steel_black"], rot=(0, math.pi / 2, 0), verts=16, col=col)
    add_box("SideVent", ac, (0.145, 0.20, 0), (0.02, 0.18, 0.34), mats["steel_black"], bevel=0.004, col=col)
    return ac


def _add_potted_plant(name, parent, loc, mats, col, scale=1.0):
    p = bpy.data.objects.new(name, None)
    p.location = Vector(loc)
    p.scale = Vector((scale, scale, scale))
    if parent:
        p.parent = parent
    link_obj(p, col)
    add_cone_frustum("Pot", p, (0, 0, 0.16), 0.14, 0.19, 0.32, mats["stone_dark"], verts=16, col=col)
    add_sphere("LeafA", p, (0, 0, 0.42), 0.22, mats["tree_foliage"], scale=(1.1, 1.1, 0.85), segs=12, col=col)
    add_sphere("LeafB", p, (0.10, -0.06, 0.54), 0.15, mats["tree_foliage"], segs=10, col=col)
    return p


def _build_house_shell(name, loc, facing_east, ext_low_mat, ext_up_mat, int_low_mat, int_up_mat, sign_bg_mat, sign_title, sign_sub, sign_z, mats, font_data, rng, col):
    """
    Modern Hanoi 2.5-story tube shophouse shell preserving exact ground-floor doorway/interior
    bounds (5.2m x 6.2m) for Unity colliders, topped with cantilevered glass balconies,
    vertical wood-tone sun louvers, modern LED lightbox signs, AC condenser, and flat roof.
    """
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    if not facing_east:
        root.rotation_euler = Euler((0, 0, math.pi))
    link_obj(root, col)

    # Clean Granite Porch Plinth & Modern Stepped Threshold
    add_box("Foundation", root, (0, 0, 0.14), (5.2, 6.2, 0.18), mats["ashlar_stone"], bevel=0.015, col=col)
    add_box("StepEntry", root, (2.78, 0, 0.12), (0.55, 2.7, 0.14), mats["terrazzo_paving"], bevel=0.012, col=col)

    # Ground Floor Outer & Inner Shells (exact walk-in room envelope)
    add_box("Ext_BackLow", root, (-2.46, 0, 0.72), (0.14, 6.0, 1.05), ext_low_mat, bevel=0.010, col=col)
    add_box("Ext_BackUp", root, (-2.46, 0, 2.32), (0.12, 6.0, 2.16), ext_up_mat, bevel=0.008, col=col)
    add_box("Int_BackLow", root, (-2.33, 0, 0.72), (0.06, 5.75, 1.05), int_low_mat, bevel=0.006, col=col)
    add_box("Int_BackUp", root, (-2.33, 0, 2.32), (0.06, 5.75, 2.16), int_up_mat, bevel=0.006, col=col)

    for sy_name, sy, isign in (("SideL", -2.90, -1), ("SideR", 2.90, 1)):
        ext_y = sy + isign * 0.05
        int_y = sy - isign * 0.07
        for prefix, wy, thk in (("Ext", ext_y, 0.12), ("Int", int_y, 0.06)):
            low_m = ext_low_mat if prefix == "Ext" else int_low_mat
            up_m = ext_up_mat if prefix == "Ext" else int_up_mat
            add_box(f"{prefix}_{sy_name}_Low", root, (0, wy, 0.72), (4.95, thk, 1.05), low_m, bevel=0.008, col=col)
            add_box(f"{prefix}_{sy_name}_BackPier", root, (-1.60, wy, 2.32), (1.76, thk, 2.16), up_m, bevel=0.008, col=col)
            add_box(f"{prefix}_{sy_name}_FrontPier", root, (1.60, wy, 2.32), (1.76, thk, 2.16), up_m, bevel=0.008, col=col)
            add_box(f"{prefix}_{sy_name}_WinLintel", root, (0, wy, 2.90), (1.45, thk, 1.00), up_m, bevel=0.008, col=col)
        add_box(f"{sy_name}_WinFrame", root, (0, sy, 1.82), (1.46, 0.24, 0.08), mats["steel_black"], bevel=0.008, col=col)
        add_box(f"{sy_name}_Glass", root, (0, sy, 1.82), (1.36, 0.04, 1.12), mats["glass"], bevel=0.004, col=col)
        add_box(f"{sy_name}_MullionV", root, (0, sy, 1.82), (0.05, 0.12, 1.12), mats["steel_black"], bevel=0.004, col=col)

    for side, fy in (("FrontL", -2.10), ("FrontR", 2.10)):
        add_box(f"Ext_{side}_Low", root, (2.46, fy, 0.72), (0.14, 1.80, 1.05), ext_low_mat, bevel=0.010, col=col)
        add_box(f"Ext_{side}_Up", root, (2.46, fy, 2.32), (0.12, 1.80, 2.16), ext_up_mat, bevel=0.008, col=col)
        add_box(f"Int_{side}_Low", root, (2.34, fy, 0.72), (0.06, 1.68, 1.05), int_low_mat, bevel=0.006, col=col)
        add_box(f"Int_{side}_Up", root, (2.34, fy, 2.32), (0.06, 1.68, 2.16), int_up_mat, bevel=0.006, col=col)
        # Modern shopfront display glass panels flanking entry (glass proud of dark steel frame)
        add_box(f"{side}_ShowFrame", root, (2.535, fy, 1.80), (0.05, 1.22, 1.22), mats["steel_black"], bevel=0.005, col=col)
        add_box(f"{side}_ShowGlass", root, (2.560, fy, 1.80), (0.04, 1.08, 1.08), mats["glass"], bevel=0.003, col=col)
        add_box(f"{side}_ShowBar", root, (2.582, fy, 1.80), (0.02, 1.12, 0.04), mats["steel_black"], bevel=0.002, col=col)

    add_box("Ext_DoorWall", root, (2.46, 0, 3.16), (0.14, 2.44, 0.56), ext_up_mat, bevel=0.008, col=col)
    add_box("Int_DoorWall", root, (2.34, 0, 3.16), (0.06, 2.44, 0.56), int_up_mat, bevel=0.006, col=col)
    add_box("DoorPostL", root, (2.46, -1.22, 1.56), (0.24, 0.14, 2.75), mats["steel_black"], bevel=0.008, col=col)
    add_box("DoorPostR", root, (2.46, 1.22, 1.56), (0.24, 0.14, 2.75), mats["steel_black"], bevel=0.008, col=col)
    add_box("DoorHeaderBeam", root, (2.46, 0, 2.92), (0.26, 2.72, 0.14), mats["steel_black"], bevel=0.008, col=col)

    # Interior Modern Slab Ceiling & Floor 1->2 Structural Slab (Z = 3.48)
    add_box("Floor2_Slab", root, (0.10, 0, 3.52), (5.45, 6.16, 0.24), ext_up_mat, bevel=0.012, col=col)
    add_box("Int_CeilingTrackL", root, (0, -0.85, 3.36), (4.2, 0.06, 0.06), mats["steel_black"], bevel=0.004, col=col)
    add_box("Int_CeilingTrackR", root, (0, 0.85, 3.36), (4.2, 0.06, 0.06), mats["steel_black"], bevel=0.004, col=col)

    # Sleek Modern Cantilevered Entrance Canopy + LED Recessed Lights
    add_box("FrontCanopy_Slab", root, (2.98, 0, 2.98), (1.05, 5.85, 0.12), mats["steel_black"], bevel=0.010, col=col)
    add_box("Canopy_WarmStrip", root, (2.98, 0, 2.91), (0.22, 4.80, 0.03), mats["warm_glow"], bevel=0.004, col=col)
    add_point_light(f"{name}_PorchGlow", root, (3.15, 0, 2.55), (1.0, 0.90, 0.75), 42.0, radius=0.4, col=col)

    # Modern 2nd Floor Recessed Studio Box & Balcony Terrace
    add_box("F2_RoomBox", root, (-0.35, 0, 5.15), (4.35, 5.95, 3.05), ext_up_mat, bevel=0.012, col=col)
    add_box("F2_GlassCurtain", root, (1.84, 0, 5.05), (0.06, 4.60, 2.55), mats["glass"], bevel=0.004, col=col)
    for my in (-2.25, -0.75, 0.75, 2.25):
        add_box(f"F2_Mullion_{my}", root, (1.85, my, 5.05), (0.10, 0.08, 2.65), mats["steel_black"], bevel=0.004, col=col)

    # Vertical Architectural Wood-Tone Sun Louvers on 2nd floor wing (classic modern Hanoi nhà ống look!)
    for lv in range(7):
        ly = 1.35 + lv * 0.24
        add_box(f"F2_Louver_{lv}", root, (2.25, ly, 5.10), (0.18, 0.06, 2.75), mats["wood_light"], bevel=0.005, col=col)

    # 2nd Floor Cantilevered Balcony Floor + Glass & Black-Steel Railing
    add_box("F2_BalconyDeck", root, (2.38, 0, 3.66), (1.15, 5.95, 0.10), mats["ashlar_stone"], bevel=0.008, col=col)
    add_box("F2_BalconyGlass", root, (2.90, 0, 4.18), (0.04, 5.80, 0.92), mats["glass"], bevel=0.003, col=col)
    add_box("F2_BalconyRailTop", root, (2.90, 0, 4.66), (0.08, 5.88, 0.06), mats["steel_black"], bevel=0.006, col=col)
    for py in (-2.88, -1.44, 0.0, 1.44, 2.88):
        add_box(f"F2_RailPost_{py}", root, (2.90, py, 4.18), (0.07, 0.07, 1.00), mats["steel_black"], bevel=0.004, col=col)

    # Modern Flat Architectural Roof + Parapet Rim + Penthouse Pergola Frame
    add_box("Roof_Slab", root, (0.20, 0, 6.76), (5.55, 6.20, 0.22), ext_up_mat, bevel=0.012, col=col)
    add_box("Roof_TrimRim", root, (2.96, 0, 6.84), (0.16, 6.24, 0.36), mats["steel_black"], bevel=0.008, col=col)
    add_box("Roof_SideWingL", root, (0.20, -3.05, 6.84), (5.55, 0.14, 0.34), mats["steel_black"], bevel=0.008, col=col)
    add_box("Roof_SideWingR", root, (0.20, 3.05, 6.84), (5.55, 0.14, 0.34), mats["steel_black"], bevel=0.008, col=col)

    # Urban Hanoi Facade Details: AC Outdoor Unit & Balcony Planters
    _add_ac_unit(f"{name}_ACUnit", root, (2.02, -2.18, 4.25), mats, col, rot_z=0.0)
    _add_potted_plant(f"{name}_BalconyPlantL", root, (2.45, -1.35, 3.72), mats, col, scale=0.85)
    _add_potted_plant(f"{name}_BalconyPlantR", root, (2.45, 0.55, 3.72), mats, col, scale=0.90)
    _add_potted_plant(f"{name}_PorchPlantL", root, (2.82, -2.50, 0.23), mats, col, scale=0.95)
    _add_potted_plant(f"{name}_PorchPlantR", root, (2.82, 2.50, 0.23), mats, col, scale=0.95)

    # Modern LED Backlit Box Signboard proud of 2nd floor fascia (X = 2.92..3.00) + Vertical Hanoi Blade Sign
    add_box("MainSign_Frame", root, (2.92, 0, sign_z + 0.04), (0.12, 4.10, 0.76), mats["steel_black"], bevel=0.010, col=col)
    add_box("MainSign_Board", root, (2.96, 0, sign_z + 0.04), (0.08, 3.96, 0.64), sign_bg_mat, bevel=0.006, col=col)
    txt_rot = (math.pi / 2, 0, math.pi / 2)
    add_3d_text("MainSign_Title", root, sign_title, (3.015, 0, sign_z + 0.14), txt_rot, 0.185, mats["sign_gold"], font_data=font_data, extrude=0.006, col=col)
    add_3d_text("MainSign_Sub", root, sign_sub, (3.015, 0, sign_z - 0.10), txt_rot, 0.10, mats["sign_sub"], font_data=font_data, extrude=0.004, col=col)

    add_box("BladeBracket", root, (2.82, -2.70, 2.65), (0.06, 0.75, 0.06), mats["steel_black"], bevel=0.005, col=col)
    add_box("BladeTrim", root, (2.86, -2.76, 2.38), (0.05, 0.54, 0.92), mats["steel_black"], bevel=0.008, col=col)
    add_box("BladeBoard", root, (2.86, -2.76, 2.38), (0.06, 0.46, 0.84), sign_bg_mat, bevel=0.006, col=col)
    add_3d_text("Blade_TextF", root, sign_title[:10], (2.898, -2.76, 2.38), txt_rot, 0.062, mats["sign_gold"], font_data=font_data, extrude=0.003, col=col)

    # Interior Modern Pendant Track Lamp + Bright Warm-Daylight Wash
    add_cyl("CeilingCord", root, (0, 0, 3.16), 0.018, 0.48, mats["steel_black"], col=col)
    add_cyl("CeilingLanternRing", root, (0, 0, 2.90), 0.36, 0.045, mats["brass"], verts=24, col=col)
    add_cyl("CeilingLanternCore", root, (0, 0, 2.84), 0.14, 0.14, mats["warm_glow"], verts=16, col=col)
    add_point_light(f"{name}_InteriorLight", root, (0.2, 0, 2.65), (1.0, 0.90, 0.76), 140.0, radius=0.5, col=col)
    return root


def build_blindbox_house_unique(loc, mats, font_data, rng, col):
    """
    House 1 (Tiệm Hộp Mù Dì Mận — Phố Hà Nội Hiện Đại x Cổ Vật):
    - Crisp 2.5-story modern Hanoi shophouse: cream stucco, glazed teal ceramic dado, glass balcony, sun-louvers.
    - Modern rooftop corner glass showcase pedestal displaying a levitating arcane crystal halo!
    - Interior: Warm timber plank flooring + Sunken Brass & Glowing Arcane Astrolabe Medallion!
    """
    root = _build_house_shell(
        "Shop_BlindBox_House", loc, True,
        ext_low_mat=mats["brick_glazed_teal"],
        ext_up_mat=mats["wall_cream"],
        int_low_mat=mats["wood_dark"],
        int_up_mat=mats["wall_cream"],
        sign_bg_mat=mats["sign_red"],
        sign_title="TIỆM HỘP MÙ DÌ MẬN",
        sign_sub="SỐ 18 · KỲ VẬT PHỐ HÀ NỘI",
        sign_z=3.34,
        mats=mats, font_data=font_data, rng=rng, col=col,
    )

    # Interior Floor: 32 Warm Aged Wood Planks + Sunken Arcane Astrolabe Medallion
    plank_mats = [mats["wood_aged_floor"], mats["wood_dark"], mats["wood_light"]]
    for row in range(16):
        py = -2.66 + row * 0.355
        split_x = rng.uniform(-0.8, 0.8)
        for seg, (cx, len_x) in enumerate((((-2.4 + split_x) * 0.5, split_x + 2.4 - 0.02), ((2.4 + split_x) * 0.5, 2.4 - split_x - 0.02))):
            m = plank_mats[(row + seg) % 3]
            add_box(f"FloorPlank_{row}_{seg}", root, (cx, py, 0.24), (len_x, 0.335, 0.032), m, bevel=0.004, col=col)

    # Traditional Vietnamese Woven Entryway Carpet & Lounge Rug inside shop
    add_box("FloorRug_Border", root, (1.05, 0, 0.262), (1.35, 1.75, 0.012), mats["carpet_gold"], bevel=0.004, col=col)
    add_box("FloorRug_Core", root, (1.05, 0, 0.268), (1.18, 1.56, 0.012), mats["velvet"], bevel=0.004, col=col)

    # Rooftop Pergola Planter Accent (clean modern urban terrace)
    _add_potted_plant("RoofTerrace_Plant", root, (1.35, -2.05, 6.88), mats, col, scale=1.25)

    # Sleek Exterior Decorative Trim & Sidewalk Potted Bamboo/Shrubs
    add_box("Modern_EntryStepTrim", root, (2.78, 0, 0.19), (0.58, 2.74, 0.02), mats["brass"], bevel=0.004, col=col)

    # Interior Carpet beside Medallion + Framed Gallery Poster + Modern Display Shelves + Counter
    add_box("Int_Carpet_Border", root, (0.15, -1.35, 0.264), (1.45, 2.15, 0.015), mats["carpet_gold"], bevel=0.004, col=col)
    add_box("Int_Carpet_Center", root, (0.15, -1.35, 0.270), (1.30, 1.98, 0.015), mats["velvet"], bevel=0.004, col=col)

    add_box("Map_Frame", root, (-1.35, -2.79, 2.05), (1.15, 0.05, 0.78), mats["steel_black"], bevel=0.008, col=col)
    add_box("Map_Parchment", root, (-1.35, -2.76, 2.05), (1.02, 0.03, 0.65), mats["parchment"], bevel=0.003, col=col)

    add_box("BackShelf_PostL", root, (-2.05, -1.85, 1.32), (0.45, 0.08, 2.15), mats["steel_black"], bevel=0.008, col=col)
    add_box("BackShelf_PostR", root, (-2.05, 1.85, 1.32), (0.45, 0.08, 2.15), mats["steel_black"], bevel=0.008, col=col)
    for sz in (0.85, 1.45, 2.05):
        add_box(f"BackShelf_Plank_{sz}", root, (-2.05, 0, sz), (0.42, 3.65, 0.05), mats["wood_light"], bevel=0.008, col=col)

    add_box("Counter_Base", root, (-0.65, 0, 0.68), (0.88, 2.75, 0.88), mats["wood_dark"], bevel=0.012, col=col)
    add_box("Counter_Top", root, (-0.62, 0, 1.16), (1.04, 2.92, 0.08), mats["wood_aged_floor"], bevel=0.010, col=col)
    add_box("Counter_BrassTrim", root, (-0.14, 0, 1.08), (0.04, 2.78, 0.05), mats["brass"], bevel=0.006, col=col)

    variants = ["WoodCrate_Old", "BrassChest_Steam", "RunicBox_Arcane", "HoloPod_SciFi"]
    y_offsets = [-1.02, -0.34, 0.34, 1.02]
    for i, var in enumerate(variants):
        b = build_blind_box(f"Display_{var}", var, (-0.52, y_offsets[i], 1.20), mats, col, parent=root)
        b.rotation_euler = Euler((0, 0, (i - 1.5) * -0.10))

    build_blind_box("Shelf_Crate_1", "WoodCrate_Old", (-2.02, -1.10, 1.48), mats, col, parent=root)
    build_blind_box("Shelf_Chest_2", "BrassChest_Steam", (-2.02, 1.10, 1.48), mats, col, parent=root)
    add_cyl("Shelf_CeramicVase_1", root, (-2.02, 0.10, 1.60), 0.10, 0.24, mats["jade"], col=col)
    add_cyl("Shelf_BronzeUrn_2", root, (-2.02, -0.42, 1.60), 0.09, 0.22, mats["brass"], col=col)
    add_cyl("Shelf_Scroll_2", root, (-2.02, -0.35, 2.14), 0.06, 0.34, mats["parchment"], rot=(math.pi / 2, 0, 0), col=col)
    add_point_light("CounterSpot", root, (0.35, 0, 2.20), (1.0, 0.88, 0.65), 55.0, radius=0.3, col=col)
    return root


def build_openbox_house_unique(loc, mats, font_data, rng, col):
    """
    House 2 (Xưởng Khui Hộp & Thẩm Định — Phố Hà Nội Hiện Đại x Công Nghệ Dị Giới):
    - Sleek modern ashlar stone + white stucco studio with matte-black steel frames & blue LED signage.
    - Rooftop Glass Geodesic Skylight Pavilion (Observatory_Dome) + Kinetic Clockwork Facade Sculpture!
    - Interior: Industrial concrete studio slabs + Recessed Glowing Sub-Floor Chrono-Gear Pit!
    """
    root = _build_house_shell(
        "Shop_OpenBox_House", loc, False,
        ext_low_mat=mats["ashlar_stone"],
        ext_up_mat=mats["wall_cream"],
        int_low_mat=mats["brick_glazed_teal"],
        int_up_mat=mats["wall_cream"],
        sign_bg_mat=mats["sign_blue"],
        sign_title="XƯỞNG KHUI & ĐỊNH GIÁ",
        sign_sub="SỐ 22 · STUDIO THẨM ĐỊNH",
        sign_z=3.34,
        mats=mats, font_data=font_data, rng=rng, col=col,
    )

    # Interior Floor: Studio Concrete Slabs + Recessed Glowing Sub-Floor Chrono-Gear Pit
    add_box("Workshop_BaseSlab", root, (0, 0, 0.232), (4.85, 5.75, 0.025), mats["workshop_floor"], bevel=0.005, col=col)
    for sx in (-1.75, -0.85, 0.05, 1.95):
        for sy in (-2.1, -1.05, 1.05, 2.1):
            add_box(f"StoneTile_{sx}_{sy}", root, (sx, sy, 0.246), (0.84, 0.96, 0.018), mats["workshop_floor"], bevel=0.005, col=col)

    # Studio Entryway Runner Rug & Rooftop Greenery (pure modern Hanoi studio aesthetic)
    add_box("FloorPit_SlatePad", root, (0.95, 0, 0.258), (1.35, 1.65, 0.012), mats["ashlar_stone"], bevel=0.004, col=col)
    _add_potted_plant("RoofTerrace_StudioPlant", root, (-0.25, -1.35, 6.88), mats, col, scale=1.20)

    # Interior: Workbench, Notice/Tool Board, Shelves & Props (exact sprint coordinates!)
    add_box("Toolboard_Panel", root, (-1.58, 2.78, 1.95), (1.28, 0.06, 0.85), mats["wood_dark"], bevel=0.008, col=col)
    add_box("Blueprint_Sheet", root, (-1.82, 2.74, 2.02), (0.60, 0.02, 0.48), mats["dial_white"], bevel=0.003, col=col)
    add_box("Clipboard_Sheet", root, (-1.15, 2.74, 1.92), (0.32, 0.02, 0.42), mats["parchment"], bevel=0.003, col=col)

    add_box("BackShelf_PostL", root, (-2.05, -1.85, 1.32), (0.45, 0.08, 2.15), mats["steel_black"], bevel=0.008, col=col)
    add_box("BackShelf_PostR", root, (-2.05, 1.85, 1.32), (0.45, 0.08, 2.15), mats["steel_black"], bevel=0.008, col=col)
    for sz in (0.85, 1.45, 2.05):
        add_box(f"BackShelf_Plank_{sz}", root, (-2.05, 0, sz), (0.42, 3.65, 0.05), mats["wood_dark"], bevel=0.008, col=col)

    for lx in (-0.98, -0.12):
        for ly in (-1.18, 1.18):
            add_box(f"BenchLeg_{lx}_{ly}", root, (lx, ly, 0.64), (0.12, 0.12, 0.82), mats["steel_black"], bevel=0.01, col=col)
    add_box("BenchTop", root, (-0.55, 0, 1.08), (1.12, 2.65, 0.09), mats["wood_light"], bevel=0.015, col=col)
    add_box("VelvetMat", root, (-0.48, 0.52, 1.132), (0.58, 0.72, 0.018), mats["velvet"], bevel=0.006, col=col)

    add_cyl("LampBase", root, (-0.85, 1.08, 1.14), 0.11, 0.03, mats["brass"], col=col)
    add_cyl("LampStem", root, (-0.85, 1.08, 1.36), 0.018, 0.42, mats["brass"], col=col)
    add_cone_frustum("LampShade", root, (-0.68, 0.92, 1.56), 0.12, 0.05, 0.14, mats["brass"], rot=(0.45, 0.45, 0), col=col)
    add_sphere("LampBulb", root, (-0.66, 0.90, 1.52), 0.04, mats["warm_glow"], col=col)
    add_spot_light("WorkbenchLampSpot", root, (-0.64, 0.88, 1.50), (-0.48, 0.52, 1.14), (1.0, 0.90, 0.72), 95.0, spot_deg=60, col=col)

    add_cyl("MagLens", root, (-0.26, 0.98, 1.135), 0.075, 0.012, mats["glass"], col=col)
    add_cyl("MagRim", root, (-0.26, 0.98, 1.135), 0.082, 0.016, mats["brass"], col=col)
    add_cyl("MagHandle", root, (-0.17, 1.07, 1.135), 0.014, 0.14, mats["wood_dark"], rot=(math.pi / 2, 0, -0.78), col=col)

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


# ==================== 4. MODERN HANOI STREET TREES, PHO SHOP, TRA DA VE SO & TUBE HOUSES ====================

def build_hanoi_street_tree(name, loc, mats, rng, col, parent=None, scale=1.0):
    """Lush Hanoi urban street tree (Cây Sấu / Xà Cừ) in a granite seating planter."""
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    root.scale = Vector((scale, scale, scale))
    if parent:
        root.parent = parent
    link_obj(root, col)

    # Granite tree planter box & dark soil
    add_box("Planter_Base", root, (0, 0, 0.16), (1.45, 1.45, 0.32), mats["ashlar_stone"], bevel=0.015, col=col)
    add_box("Planter_Soil", root, (0, 0, 0.28), (1.16, 1.16, 0.10), mats["wood_dark"], bevel=0.008, col=col)

    # Smoothly tapered organic branching bark trunk & limbs
    add_cone_frustum("Trunk_Low", root, (0, 0, 1.05), 0.23, 0.16, 2.05, mats["tree_bark"], verts=14, col=col)
    add_cone_frustum("Trunk_Mid", root, (0.02, 0.01, 2.48), 0.16, 0.10, 1.15, mats["tree_bark"], rot=(0.03, -0.03, 0), verts=14, col=col)
    for b_idx, (bx, by, bz, brot) in enumerate((
        (-0.34, -0.22, 2.92, (0.42, -0.45, 0.3)),
        (0.36, 0.20, 2.98, (-0.36, 0.45, 0.8)),
        (0.08, -0.36, 3.06, (0.50, 0.12, -0.4)),
    )):
        add_cone_frustum(f"Branch_{b_idx}", root, (bx, by, bz), 0.09, 0.035, 1.15, mats["tree_bark"], rot=brot, verts=10, col=col)

    # Multi-layered leaf clusters (named TreeCanopy_* for gentle wind sway animation in Unity)
    add_sphere("TreeCanopy_Main", root, (0.05, 0.02, 4.15), 1.48, mats["tree_foliage"], scale=(1.15, 1.10, 0.82), segs=16, col=col)
    add_sphere("TreeCanopy_L", root, (-0.95, -0.58, 3.68), 0.96, mats["tree_foliage"], scale=(1.12, 1.05, 0.82), segs=14, col=col)
    add_sphere("TreeCanopy_R", root, (0.98, 0.55, 3.80), 1.02, mats["tree_foliage"], scale=(1.15, 1.08, 0.80), segs=14, col=col)
    add_sphere("TreeCanopy_Top", root, (-0.12, 0.48, 4.82), 0.92, mats["tree_foliage"], scale=(1.08, 1.05, 0.78), segs=14, col=col)
    return root


def build_hanoi_pho_shop(name, loc, facing_east, mats, font_data, rng, col):
    """Quán Phở Bò Hà Nội Hiện Đại: glass open front, inox broth counter with 2 steaming pots, dining tables, 2nd floor balcony."""
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    if not facing_east:
        root.rotation_euler = Euler((0, 0, math.pi))
    link_obj(root, col)

    # Ground plinth & clean bright cafe-style floor
    add_box("Pho_Plinth", root, (0, 0, 0.12), (5.2, 5.4, 0.24), mats["ashlar_stone"], bevel=0.015, col=col)
    add_box("Pho_TileFloor", root, (0, 0, 0.245), (4.9, 5.1, 0.02), mats["terrazzo_paving"], bevel=0.004, col=col)

    # Walls (open wide entrance facing +X)
    add_box("Pho_BackWall", root, (-2.42, 0, 1.85), (0.16, 5.1, 3.25), mats["wall_cream"], bevel=0.01, col=col)
    add_box("Pho_SideWallL", root, (0, -2.55, 1.85), (5.0, 0.16, 3.25), mats["wall_yellow"], bevel=0.01, col=col)
    add_box("Pho_SideWallR", root, (0, 2.55, 1.85), (5.0, 0.16, 3.25), mats["wall_yellow"], bevel=0.01, col=col)
    add_box("Pho_FrontPierL", root, (2.42, -2.18, 1.85), (0.18, 0.85, 3.25), mats["wall_cream"], bevel=0.008, col=col)
    add_box("Pho_FrontPierR", root, (2.42, 2.18, 1.85), (0.18, 0.85, 3.25), mats["wall_cream"], bevel=0.008, col=col)

    # 2nd Floor Modern Balcony, Glass Railing, Wood Sun Louvers & AC Unit
    add_box("Pho_F2Slab", root, (0.18, 0, 3.55), (5.5, 5.45, 0.22), mats["wall_cream"], bevel=0.01, col=col)
    add_box("Pho_F2Box", root, (-0.42, 0, 5.05), (4.1, 5.2, 2.85), mats["wall_cream"], bevel=0.01, col=col)
    add_box("Pho_F2Glass", root, (1.66, 0, 4.95), (0.06, 4.0, 2.35), mats["glass"], bevel=0.004, col=col)
    for lv in range(6):
        add_box(f"Pho_Louver_{lv}", root, (2.05, 1.05 + lv * 0.24, 5.00), (0.16, 0.06, 2.65), mats["wood_light"], bevel=0.004, col=col)
    add_box("Pho_BalGlass", root, (2.82, 0, 4.12), (0.04, 5.20, 0.88), mats["glass"], bevel=0.003, col=col)
    add_box("Pho_BalRail", root, (2.82, 0, 4.58), (0.08, 5.26, 0.06), mats["steel_black"], bevel=0.005, col=col)
    add_box("Pho_Roof", root, (0.18, 0, 6.56), (5.5, 5.45, 0.22), mats["steel_black"], bevel=0.012, col=col)
    _add_ac_unit("Pho_AC", root, (1.76, -2.10, 4.15), mats, col)
    _add_potted_plant("Pho_BalPlant", root, (2.38, -1.25, 3.68), mats, col, scale=0.8)

    # Prominent LED Lightbox Signboard mounted cleanly in front of Canopy/F2 fascia: PHỞ BÒ HÀ NỘI
    add_box("Pho_Canopy", root, (2.95, 0, 3.04), (1.05, 5.3, 0.12), mats["steel_black"], bevel=0.008, col=col)
    add_box("Pho_SignFrame", root, (3.44, 0, 3.36), (0.12, 4.3, 0.84), mats["steel_black"], bevel=0.008, col=col)
    add_box("Pho_SignBg", root, (3.48, 0, 3.36), (0.08, 4.16, 0.72), mats["sign_red"], bevel=0.006, col=col)
    txt_rot = (math.pi / 2, 0, math.pi / 2)
    add_3d_text("Pho_SignTitle", root, "PHỞ BÒ HÀ NỘI", (3.535, 0, 3.46), txt_rot, 0.24, mats["sign_gold"], font_data=font_data, extrude=0.006, col=col)
    add_3d_text("Pho_SignSub", root, "GIA TRUYỀN · NƯỚC DÙNG NGON", (3.535, 0, 3.16), txt_rot, 0.105, mats["sign_sub"], font_data=font_data, extrude=0.004, col=col)

    # Perpendicular Glowing Side Blade Sign
    add_box("Pho_BladeTrim", root, (2.82, -2.52, 2.35), (0.05, 0.56, 1.05), mats["steel_black"], bevel=0.006, col=col)
    add_box("Pho_BladeBg", root, (2.82, -2.52, 2.35), (0.06, 0.48, 0.96), mats["sign_red"], bevel=0.005, col=col)
    add_3d_text("Pho_BladeTxt", root, "PHỞ BÒ", (2.86, -2.52, 2.35), txt_rot, 0.082, mats["sign_gold"], font_data=font_data, extrude=0.003, col=col)

    # Signature Stainless-Steel (Inox) Broth Counter + Glass Cabinet + 2 Broth Pots + Rising Steam!
    add_box("Pho_CounterBody", root, (1.15, -1.05, 0.68), (0.92, 2.35, 0.86), mats["inox_steel"], bevel=0.012, col=col)
    add_box("Pho_GlassCaseFrame", root, (1.28, -1.05, 1.48), (0.58, 1.55, 0.72), mats["glass"], bevel=0.005, col=col)
    add_box("Pho_GlassFrameTrim", root, (1.28, -1.05, 1.85), (0.62, 1.59, 0.05), mats["steel_black"], bevel=0.004, col=col)

    for idx, py in enumerate((-0.62, -1.55)):
        add_cyl(f"PhoPot_{idx+1}", root, (0.96, py, 1.08), 0.28, 0.48, mats["inox_steel"], verts=24, bevel=0.01, col=col)
        add_cyl(f"PhoSoup_{idx+1}", root, (0.96, py, 1.31), 0.25, 0.02, mats["amber"], verts=20, col=col)
        add_cyl(f"PhoStoveGlow_{idx+1}", root, (0.96, py, 0.82), 0.22, 0.04, mats["warm_glow"], verts=16, col=col)

    # Billowing 3D Steam Plumes above Phở Broth Pots (animated by MarketAmbientAnimator!)
    add_sphere("PhoSteam_1", root, (0.96, -0.62, 1.56), 0.14, mats["steam_mist"], scale=(0.9, 0.9, 1.3), segs=10, col=col)
    add_sphere("PhoSteam_2", root, (0.98, -0.58, 1.84), 0.18, mats["steam_mist"], scale=(1.0, 1.0, 1.2), segs=10, col=col)
    add_sphere("PhoSteam_3", root, (0.96, -1.55, 1.66), 0.16, mats["steam_mist"], scale=(1.0, 1.0, 1.3), segs=10, col=col)

    # Stack of white porcelain Phở bowls on counter
    for bz in (1.16, 1.22, 1.28):
        add_cone_frustum(f"PhoBowl_{bz}", root, (1.30, -1.92, bz), 0.06, 0.11, 0.07, mats["dial_white"], verts=16, col=col)

    # Inox Dining Tables & Stools with Condiment Holders
    for ti, (tx, ty) in enumerate(((0.15, 1.15), (-1.25, 1.15), (1.75, 1.45))):
        add_box(f"PhoTable_{ti}", root, (tx, ty, 0.72), (0.76, 1.05, 0.04), mats["inox_steel"], bevel=0.008, col=col)
        add_cyl(f"PhoTableLeg_{ti}", root, (tx, ty, 0.48), 0.045, 0.46, mats["inox_steel"], col=col)
        add_cyl(f"ChiliJar_{ti}", root, (tx - 0.12, ty + 0.14, 0.78), 0.04, 0.09, mats["plastic_red"], col=col)
        add_cyl(f"ChopstickCup_{ti}", root, (tx + 0.14, ty - 0.12, 0.80), 0.045, 0.12, mats["wood_light"], col=col)
        for sy_off in (-0.42, 0.42):
            add_cyl(f"PhoStool_{ti}_{sy_off}", root, (tx, ty + sy_off, 0.42), 0.16, 0.36, mats["inox_steel"], verts=16, col=col)

    add_point_light(f"{name}_WarmLight", root, (0.6, 0, 2.55), (1.0, 0.88, 0.68), 115.0, radius=0.45, col=col)
    add_point_light(f"{name}_PorchLight", root, (2.95, 0, 2.65), (1.0, 0.86, 0.65), 48.0, radius=0.35, col=col)
    return root


def build_hanoi_trada_veso(name, loc, facing_east, mats, font_data, rng, col):
    """Quầy Trà Đá & Vé Số Vỉa Hè Hà Nội: ô che ngoài trời, bàn vé số Xổ Số Thủ Đô, phích Rạng Đông, bàn ghế nhựa xanh đỏ."""
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    if not facing_east:
        root.rotation_euler = Euler((0, 0, math.pi))
    link_obj(root, col)

    # Low stone sidewalk mat pad
    add_box("TraDa_Pad", root, (0, 0, 0.03), (3.4, 3.0, 0.06), mats["terrazzo_paving"], bevel=0.01, col=col)

    # Lottery Stall Table & Upright Glass Ticket Display Board
    add_box("VeSo_DeskBody", root, (-0.45, -0.55, 0.42), (0.68, 1.35, 0.80), mats["wood_light"], bevel=0.012, col=col)
    add_box("VeSo_DeskTop", root, (-0.42, -0.55, 0.84), (0.76, 1.44, 0.06), mats["wood_aged_floor"], bevel=0.01, col=col)
    add_box("VeSo_BoardBack", root, (-0.75, -0.55, 1.38), (0.06, 1.38, 1.05), mats["sign_blue"], bevel=0.008, col=col)
    add_box("VeSo_BoardFrame", root, (-0.75, -0.55, 1.38), (0.08, 1.46, 1.12), mats["steel_black"], bevel=0.008, col=col)
    txt_rot = (math.pi / 2, 0, math.pi / 2)
    add_3d_text("VeSo_Title", root, "TRÀ ĐÁ · VÉ SỐ", (-0.70, -0.55, 1.66), txt_rot, 0.145, mats["sign_gold"], font_data=font_data, extrude=0.004, col=col)
    add_3d_text("VeSo_Sub", root, "XỔ SỐ KIẾN THIẾT THỦ ĐÔ", (-0.70, -0.55, 1.42), txt_rot, 0.078, mats["sign_sub"], font_data=font_data, extrude=0.003, col=col)

    # Lottery ticket rows pinned on display & countertop
    for r in range(3):
        add_box(f"TicketSheet_{r}", root, (-0.70, -0.95 + r * 0.38, 1.12), (0.02, 0.28, 0.32),
                mats["plastic_red"] if r % 2 == 0 else mats["dial_white"], bevel=0.002, col=col)
    add_box("TicketStack", root, (-0.32, -0.85, 0.90), (0.24, 0.34, 0.06), mats["dial_white"], bevel=0.004, col=col)

    # Red Rạng Đông Thermos Flask & Candy/Sunflower Seed Jars on Tea Stall
    add_cyl("PhichNuoc_Body", root, (-0.30, -0.15, 1.06), 0.09, 0.36, mats["plastic_red"], verts=20, col=col)
    add_cyl("PhichNuoc_Neck", root, (-0.30, -0.15, 1.26), 0.06, 0.08, mats["inox_steel"], verts=16, col=col)
    add_cyl("CandyJar", root, (-0.32, -0.46, 0.99), 0.085, 0.24, mats["glass"], verts=16, col=col)
    add_cyl("CandyFill", root, (-0.32, -0.46, 0.95), 0.072, 0.16, mats["amber"], verts=14, col=col)

    # Signature Low Sidewalk Plastic Table + Green Tea Pitcher + Iced Tea Cups
    add_box("PlasticTable_Top", root, (0.58, 0.58, 0.34), (0.58, 0.58, 0.04), mats["plastic_blue"], bevel=0.01, col=col)
    for lx in (0.35, 0.81):
        for ly in (0.35, 0.81):
            add_box(f"PTLeg_{lx}_{ly}", root, (lx, ly, 0.18), (0.04, 0.04, 0.32), mats["plastic_blue"], bevel=0.004, col=col)
    add_cyl("TeaPitcher", root, (0.52, 0.58, 0.48), 0.075, 0.22, mats["jade"], verts=16, col=col)
    for gi, (gx, gy) in enumerate(((0.68, 0.48), (0.68, 0.68), (0.48, 0.72))):
        add_cyl(f"TeaCup_{gi}", root, (gx, gy, 0.40), 0.036, 0.09, mats["glass"], verts=12, col=col)

    # 5 Iconic Hanoi Blue & Red Low Plastic Sidewalk Stools (Ghế Nhựa Vỉa Hè)
    stool_specs = [
        (0.18, 0.58, mats["plastic_red"]),
        (0.58, 1.12, mats["plastic_blue"]),
        (1.06, 0.62, mats["plastic_red"]),
        (0.62, 0.02, mats["plastic_blue"]),
        (0.25, -0.58, mats["plastic_red"]),
    ]
    for si, (sx, sy, smat) in enumerate(stool_specs):
        add_box(f"PlasticStool_{si}", root, (sx, sy, 0.16), (0.28, 0.28, 0.24), smat, rot=(0, 0, si * 0.4), bevel=0.01, col=col)

    # Tilted Urban Street Umbrella / Parasol + Warm String Light Glow
    add_cyl("Parasol_Pole", root, (-0.15, 0.25, 1.35), 0.032, 2.55, mats["inox_steel"], col=col)
    add_cone_frustum("Parasol_Canopy", root, (-0.15, 0.25, 2.48), 1.68, 0.12, 0.48, mats["sign_blue"], rot=(0.08, -0.06, 0), verts=8, smooth=False, col=col)
    for bi in range(4):
        ang = bi * (math.pi / 2) + 0.3
        add_sphere(f"ParasolBulb_{bi}", root, (-0.15 + math.cos(ang) * 1.1, 0.25 + math.sin(ang) * 1.1, 2.24), 0.045, mats["warm_glow"], segs=8, col=col)
    add_point_light(f"{name}_BulbLight", root, (0.15, 0.25, 2.15), (1.0, 0.88, 0.62), 45.0, radius=0.35, col=col)
    return root


def build_hanoi_shophouse_block(name, loc, facing_east, mats, font_data, rng, col):
    """Trio of adjoining 3-4 story Modern Hanoi Tube-Houses (Nhà Ống Hiện Đại Hà Nội)."""
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    if not facing_east:
        root.rotation_euler = Euler((0, 0, math.pi))
    link_obj(root, col)

    specs = [
        ("TH_Cafe", -4.0, 5.2, 3.8, 9.6, mats["wall_cream"], mats["sign_blue"], "CÀ PHÊ PHỐ"),
        ("TH_TraChanh", 0.0, 5.4, 3.8, 12.2, mats["wall_yellow"], mats["sign_red"], "TRÀ CHANH VỈA HÈ"),
        ("TH_TapHoa", 4.0, 5.2, 3.8, 10.6, mats["ashlar_stone"], mats["sign_blue"], "BÁCH HÓA HÀ NỘI"),
    ]

    for hname, hy, hx, hw, hh, wall_m, sign_m, sign_txt in specs:
        add_box(f"{hname}_Body", root, (0, hy, hh * 0.5), (hx, hw, hh), wall_m, bevel=0.015, col=col)
        # Ground shopfront black-steel + glass window & awning + sign
        add_box(f"{hname}_ShopGlass", root, (hx * 0.5 + 0.02, hy, 1.55), (0.06, hw * 0.78, 2.40), mats["glass"], bevel=0.005, col=col)
        add_box(f"{hname}_ShopFrame", root, (hx * 0.5 + 0.03, hy, 1.55), (0.09, hw * 0.82, 0.08), mats["steel_black"], bevel=0.005, col=col)
        add_box(f"{hname}_Awning", root, (hx * 0.5 + 0.48, hy, 2.95), (0.95, hw * 0.92, 0.10), mats["steel_black"], bevel=0.008, col=col)
        add_box(f"{hname}_SignBg", root, (hx * 0.5 + 0.08, hy, 3.38), (0.10, hw * 0.84, 0.58), sign_m, bevel=0.006, col=col)
        add_3d_text(f"{hname}_SignTxt", root, sign_txt, (hx * 0.5 + 0.145, hy, 3.38), (math.pi / 2, 0, math.pi / 2), 0.18, mats["sign_gold"], font_data=font_data, extrude=0.004, col=col)

        # Upper story recessed balconies + glass guardrails + wood louvers + AC units
        num_floors = int((hh - 3.2) // 3.0)
        for fl in range(num_floors):
            fz = 4.6 + fl * 3.0
            add_box(f"{hname}_F{fl}_Win", root, (hx * 0.5 + 0.02, hy, fz), (0.06, hw * 0.68, 1.85), mats["glass"], bevel=0.004, col=col)
            add_box(f"{hname}_F{fl}_BalSlab", root, (hx * 0.5 + 0.38, hy, fz - 0.95), (0.78, hw * 0.90, 0.12), mats["ashlar_stone"], bevel=0.008, col=col)
            add_box(f"{hname}_F{fl}_BalGlass", root, (hx * 0.5 + 0.72, hy, fz - 0.48), (0.04, hw * 0.86, 0.84), mats["glass"], bevel=0.003, col=col)
            add_box(f"{hname}_F{fl}_Rail", root, (hx * 0.5 + 0.72, hy, fz - 0.05), (0.07, hw * 0.88, 0.06), mats["steel_black"], bevel=0.005, col=col)
            # Wood sun louvers on one side
            for lv in range(4):
                add_box(f"{hname}_F{fl}_Louver_{lv}", root, (hx * 0.5 + 0.18, hy + 0.95 + lv * 0.22, fz), (0.14, 0.05, 2.2), mats["wood_light"], bevel=0.004, col=col)
        # Rooftop parapet & Stainless Water Tank (Téc nước Inox Sơn Hà) on tallest house
        add_box(f"{hname}_Parapet", root, (0, hy, hh + 0.18), (hx + 0.2, hw, 0.36), mats["steel_black"], bevel=0.01, col=col)
        if hh > 11.5:
            add_cyl(f"{hname}_WaterTank", root, (-0.8, hy, hh + 0.88), 0.55, 1.20, mats["inox_steel"], rot=(math.pi / 2, 0, 0), verts=20, col=col)
            add_box(f"{hname}_TankStand", root, (-0.8, hy, hh + 0.42), (1.1, 0.9, 0.45), mats["steel_black"], bevel=0.005, col=col)

    return root


def build_surrounding_town(mats, font_data, rng, col):
    # Modern Hanoi 3-4 Story Tube Shophouse Blocks framing North, South, West, and East avenues!
    for bname, bloc, facing_e in (
        ("Block_NW", (-14.8, 12.5, 0.10), True),
        ("Block_NE", (14.8, 12.5, 0.10), False),
        ("Block_SW", (-14.8, -11.5, 0.10), True),
        ("Block_SE", (14.8, -11.5, 0.10), False),
    ):
        build_hanoi_shophouse_block(bname, bloc, facing_e, mats, font_data, rng, col)

    # Signature Modern Hanoi Neighborhood Highlights visible from main plaza!
    build_hanoi_pho_shop("Shop_Hanoi_Pho", (-7.4, -7.8, 0.0), True, mats, font_data, rng, col)
    build_hanoi_trada_veso("Stall_TraDa_VeSo", (6.0, -7.0, 0.0), False, mats, font_data, rng, col)

    # Lush Hanoi Street Trees (Cây Sấu / Xà Cừ) with stone seating planters along sidewalks & park plaza!
    tree_spots = [
        (-5.2, -3.9, 1.05),
        (-5.2, 4.4, 1.12),
        (5.2, -4.4, 1.08),
        (5.2, 4.4, 1.10),
        (6.4, -9.2, 1.18),   # giant shade tree next to Trà Đá & Vé Số!
        (-5.6, -12.8, 1.00),
        (5.6, -12.8, 1.04),
        (-6.2, 10.5, 1.12),
        (6.2, 10.5, 1.06),
        (-10.8, 4.8, 0.95),
        (10.8, 4.8, 0.95),
    ]
    for ti, (tx, ty, ts) in enumerate(tree_spots):
        build_hanoi_street_tree(f"HanoiTree_{ti}", (tx, ty, 0.0), mats, rng, col, scale=ts)

    # Background Modern Hanoi Tube-Houses closing North Avenue cleanly (zero plain greybox towers!)
    nb1 = build_hanoi_shophouse_block("Block_North_W", (-6.0, 18.2, 0.0), True, mats, font_data, rng, col)
    nb1.rotation_euler = Euler((0, 0, -math.pi / 2))
    nb2 = build_hanoi_shophouse_block("Block_North_E", (6.0, 18.2, 0.0), True, mats, font_data, rng, col)
    nb2.rotation_euler = Euler((0, 0, -math.pi / 2))

    for sx, sy in ((-3.8, 5.2), (3.8, 5.2), (-3.8, -5.2), (3.8, -5.2)):
        add_cyl(f"StreetBase_{sx}_{sy}", None, (sx, sy, 0.18), 0.14, 0.36, mats["stone_dark"], verts=8, col=col)
        add_cyl(f"StreetPost_{sx}_{sy}", None, (sx, sy, 1.55), 0.055, 2.6, mats["steel_black"], col=col)
        add_cyl(f"StreetLantern_{sx}_{sy}", None, (sx, sy, 2.95), 0.13, 0.24, mats["warm_glow"], verts=8, col=col)
        add_cone_frustum(f"StreetCap_{sx}_{sy}", None, (sx, sy, 3.16), 0.20, 0.02, 0.18, mats["steel_black"], verts=8, smooth=False, col=col)
        add_point_light(f"StreetLight_{sx}_{sy}", None, (sx, sy, 2.95), (1.0, 0.85, 0.60), 58.0, radius=0.3, col=col)


# ==================== 5. LIGHTING & CAMERAS ====================

def setup_world_and_sun(col):
    world = bpy.data.worlds.new("PlazaWorld")
    bpy.context.scene.world = world
    world.use_nodes = True
    bg = world.node_tree.nodes.get("Background")
    if bg:
        bg.inputs["Color"].default_value = (0.42, 0.54, 0.70, 1.0)
        bg.inputs["Strength"].default_value = 0.78

    sun_data = bpy.data.lights.new(name="Sun_GoldenHour", type="SUN")
    sun_data.color = (1.0, 0.94, 0.84)
    sun_data.energy = 2.8
    sun_obj = bpy.data.objects.new("Sun_GoldenHour", sun_data)
    sun_obj.rotation_euler = Euler((math.radians(44), math.radians(-24), math.radians(-30)))
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
    build_surrounding_town(mats, font_data, rng, col_env)
    build_hanoi_center_island(mats, font_data, rng, col_env)

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
        ("Cam_Plaza_Overview", (0.0, -14.8, 4.8), (0.0, 1.5, 2.2), 24.0, "preview_1_plaza_overview.png"),
        ("Cam_BlindBox_Exterior", (0.2, -5.8, 2.8), (-6.2, -1.0, 2.8), 26.0, "preview_2_blindbox_shop_exterior.png"),
        ("Cam_BlindBox_Interior", (-5.05, -1.75, 1.85), (-7.25, 0.15, 0.85), 24.0, "preview_3_blindbox_shop_interior.png"),
        ("Cam_OpenBox_Exterior", (-0.2, -5.8, 2.8), (6.2, -1.0, 2.8), 26.0, "preview_4_openbox_shop_exterior.png"),
        ("Cam_Pho_TraDa_Closeup", (-2.6, -7.8, 2.3), (-6.8, -8.3, 1.8), 26.0, "preview_5_hanoi_pho_shop.png"),
        ("Cam_TraDa_Closeup", (0.5, -9.8, 2.0), (5.8, -7.0, 1.3), 28.0, "preview_6_hanoi_trada_veso.png"),
    ]

    for cname, cloc, ctarget, clens, fname in cameras:
        out_path = os.path.join(OUTPUT_DIR, fname)
        render_camera(cname, cloc, ctarget, clens, out_path)
        print("RENDERED:", out_path)


if __name__ == "__main__":
    main()

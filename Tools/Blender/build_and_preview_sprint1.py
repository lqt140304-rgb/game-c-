import math
import os
import random
import sys
import bpy
import bmesh
from mathutils import Vector, Euler

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
if SCRIPT_DIR not in sys.path:
    sys.path.insert(0, SCRIPT_DIR)

import hanoi_highcraft_arch as arch

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
    """Stylized Low Poly bright sunny flat-shaded materials matching MarketVisualFactory.cs."""
    return {
        "cobble": make_simple_mat("Mat_LP_Cobble", (0.48, 0.52, 0.58), roughness=0.90),
        "terrazzo_paving": make_simple_mat("Mat_LP_Sidewalk", (0.78, 0.76, 0.70), roughness=0.86),
        "vn_red_tile": make_simple_mat("Mat_LP_RedTile", (0.78, 0.34, 0.24), roughness=0.85),
        "vn_street_matte": make_simple_mat("Mat_LP_StreetMatte", (0.36, 0.39, 0.44), roughness=0.92),
        "wet_stone": make_simple_mat("Mat_LP_WetStone", (0.44, 0.48, 0.52), roughness=0.82),
        "moss_clump": make_simple_mat("Mat_LP_Moss", (0.32, 0.72, 0.26), roughness=0.85),
        "plaster_weathered": make_simple_mat("Mat_LP_PlasterYellow", (0.96, 0.85, 0.46), roughness=0.86),
        "plaster_alt": make_simple_mat("Mat_LP_PlasterCream", (0.96, 0.92, 0.80), roughness=0.86),
        "brick_exposed": make_simple_mat("Mat_LP_BrickRed", (0.80, 0.24, 0.20), roughness=0.85),
        "brick_glazed_teal": make_simple_mat("Mat_LP_GlazedTeal", (0.14, 0.56, 0.54), roughness=0.55),
        "brick_whitewash": make_simple_mat("Mat_LP_WhiteTrim", (0.95, 0.94, 0.90), roughness=0.86),
        "ashlar_stone": make_simple_mat("Mat_LP_AshlarCream", (0.90, 0.89, 0.85), roughness=0.84),
        "stone_dark": make_simple_mat("Mat_LP_StoneDark", (0.40, 0.42, 0.46), roughness=0.86),
        "stone": make_simple_mat("Mat_LP_Stone", (0.72, 0.72, 0.68), roughness=0.86),
        "workshop_floor": make_simple_mat("Mat_LP_WorkshopFloor", (0.80, 0.78, 0.74), roughness=0.82),
        "wood_aged_floor": make_simple_mat("Mat_LP_WoodFloor", (0.74, 0.52, 0.30), roughness=0.78),
        "wood_light": make_simple_mat("Mat_LP_WoodLight", (0.84, 0.60, 0.34), roughness=0.76),
        "wood_dark": make_simple_mat("Mat_LP_WoodDark", (0.48, 0.28, 0.16), roughness=0.76),
        "roof_tile": make_simple_mat("Mat_LP_RoofTile", (0.82, 0.32, 0.22), roughness=0.78),
        "roof_slate": make_simple_mat("Mat_LP_RoofSlate", (0.26, 0.32, 0.40), roughness=0.72),
        "brass": make_simple_mat("Mat_Brass", (0.86, 0.66, 0.24), roughness=0.34, metallic=0.86),
        "gold": make_simple_mat("Mat_Gold", (0.96, 0.78, 0.26), roughness=0.24, metallic=0.94),
        "sign_gold": make_simple_mat("Mat_SignGold", (0.98, 0.82, 0.32), roughness=0.35, metallic=0.35, emission=(0.98, 0.82, 0.32), emission_strength=0.45),
        "sign_sub": make_simple_mat("Mat_SignSub", (0.96, 0.92, 0.84), roughness=0.60, emission=(0.90, 0.86, 0.76), emission_strength=0.28),
        "iron": make_simple_mat("Mat_Iron", (0.18, 0.20, 0.24), roughness=0.50, metallic=0.80),
        "velvet": make_simple_mat("Mat_Velvet", (0.68, 0.10, 0.14), roughness=0.90),
        "carpet_gold": make_simple_mat("Mat_CarpetGold", (0.82, 0.62, 0.20), roughness=0.86),
        "wax_red": make_simple_mat("Mat_WaxRed", (0.78, 0.12, 0.14), roughness=0.42),
        "dial_white": make_simple_mat("Mat_DialWhite", (0.96, 0.94, 0.88), roughness=0.42),
        "parchment": make_simple_mat("Mat_Parchment", (0.90, 0.82, 0.64), roughness=0.86),
        "blueprint": make_simple_mat("Mat_Blueprint", (0.14, 0.32, 0.58), roughness=0.76),
        "jade": make_simple_mat("Mat_Jade", (0.14, 0.68, 0.44), roughness=0.25, metallic=0.05),
        "amber": make_simple_mat("Mat_Amber", (0.92, 0.52, 0.08), roughness=0.18, emission=(0.92, 0.52, 0.08), emission_strength=0.35),
        "obsidian": make_simple_mat("Mat_Obsidian", (0.10, 0.10, 0.14), roughness=0.20, metallic=0.20),
        "water": make_simple_mat("Mat_Water", (0.12, 0.66, 0.78), roughness=0.08, metallic=0.10, emission=(0.12, 0.66, 0.78), emission_strength=0.40),
        "cyan_glow": make_simple_mat("Mat_CyanGlow", (0.24, 0.88, 1.0), roughness=0.2, emission=(0.24, 0.88, 1.0), emission_strength=1.5),
        "arcane_glow": make_simple_mat("Mat_ArcaneGlow", (0.66, 0.34, 0.98), roughness=0.2, emission=(0.66, 0.34, 0.98), emission_strength=1.5),
        "warm_glow": make_simple_mat("Mat_WarmGlow", (1.0, 0.88, 0.58), roughness=0.3, emission=(1.0, 0.86, 0.52), emission_strength=1.4),
        "glass": make_simple_mat("Mat_Glass", (0.72, 0.88, 0.96), roughness=0.12, metallic=0.10),
        # Bright Stylized Hanoi materials
        "wall_cream": make_simple_mat("Mat_HanoiCreamWall", (0.97, 0.94, 0.88), roughness=0.84),
        "wall_yellow": make_simple_mat("Mat_HanoiYellowWall", (0.96, 0.84, 0.42), roughness=0.84),
        "steel_black": make_simple_mat("Mat_SteelBlack", (0.18, 0.20, 0.24), roughness=0.52, metallic=0.65),
        "inox_steel": make_simple_mat("Mat_InoxSteel", (0.80, 0.84, 0.88), roughness=0.28, metallic=0.85),
        "plastic_blue": make_simple_mat("Mat_PlasticBlue", (0.16, 0.48, 0.90), roughness=0.45),
        "plastic_red": make_simple_mat("Mat_PlasticRed", (0.86, 0.22, 0.20), roughness=0.45),
        "sign_red": make_simple_mat("Mat_HanoiRedSign", (0.78, 0.14, 0.14), roughness=0.55, emission=(0.72, 0.12, 0.12), emission_strength=0.22),
        "sign_blue": make_simple_mat("Mat_HanoiBlueSign", (0.14, 0.34, 0.72), roughness=0.55, emission=(0.12, 0.28, 0.62), emission_strength=0.20),
        "tree_bark": make_simple_mat("Mat_LP_TreeBark", (0.46, 0.28, 0.18), roughness=0.88),
        "tree_foliage": make_simple_mat("Mat_LP_TreeFoliage", (0.32, 0.74, 0.24), roughness=0.82),
        "steam_mist": make_simple_mat("Mat_SteamMist", (0.94, 0.96, 0.98), roughness=0.45, emission=(0.90, 0.94, 0.98), emission_strength=0.65),
        "encaustic_tile": make_simple_mat("Mat_LP_EncausticTile", (0.92, 0.88, 0.78), roughness=0.76),
        "shutters_green": make_simple_mat("Mat_LP_ShuttersGreen", (0.18, 0.58, 0.38), roughness=0.75),
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


def add_cyl(name, parent, loc, radius, depth, mat, rot=(0, 0, 0), verts=12, bevel=0.0, smooth=False, col=None):
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
    return obj


def add_cone_frustum(name, parent, loc, r1, r2, depth, mat, rot=(0, 0, 0), verts=12, smooth=False, col=None):
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


def add_sphere(name, parent, loc, radius, mat, scale=(1, 1, 1), segs=10, col=None):
    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=segs, v_segments=max(5, segs // 2), radius=radius)
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
        body_m, trim_m, seal_m, liner_m = mats["wood_light"], mats["wood_dark"], mats["wax_red"], mats["parchment"]
    elif variant == "BrassChest_Steam":
        body_m, trim_m, seal_m, liner_m = mats["wood_dark"], mats["brass"], mats["gold"], mats["velvet"]
    elif variant == "RunicBox_Arcane":
        body_m, trim_m, seal_m, liner_m = mats["stone_dark"], mats["gold"], mats["arcane_glow"], mats["velvet"]
    else:
        body_m, trim_m, seal_m, liner_m = mats["iron"], mats["stone_dark"], mats["cyan_glow"], mats["obsidian"]

    # Hollow box body: bottom plate + 4 walls + inner velvet/cushion lining + back hinges
    add_box("BodyBottom", root, (0, 0, 0.022), (0.28, 0.36, 0.024), body_m, bevel=0.008, col=col)
    add_box("WallBack", root, (-0.128, 0, 0.108), (0.024, 0.36, 0.164), body_m, bevel=0.006, col=col)
    add_box("WallFront", root, (0.128, 0, 0.108), (0.024, 0.36, 0.164), body_m, bevel=0.006, col=col)
    add_box("WallLeft", root, (0, -0.168, 0.108), (0.24, 0.024, 0.164), body_m, bevel=0.006, col=col)
    add_box("WallRight", root, (0, 0.168, 0.108), (0.24, 0.024, 0.164), body_m, bevel=0.006, col=col)
    add_box("InnerLining", root, (0, 0, 0.044), (0.228, 0.308, 0.024), liner_m, bevel=0.006, col=col)
    for sy in (-0.095, 0.095):
        add_cyl(f"Hinge_{sy}", root, (-0.142, sy, 0.19), 0.011, 0.036, trim_m, rot=(math.pi / 2, 0, 0), col=col)

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


def _build_custom_shop_envelope(
    name, loc, facing_east, depth_x, width_y, door_w, ground_h, floors, roof_style,
    ext_low_mat, ext_up_mat, int_low_mat, int_up_mat, sign_bg_mat,
    sign_title, sign_sub, mats, font_data, col
):
    """
    Parameterized Hanoi shop shell supporting 1, 1.5, 2, or 3 stories, custom width/depth,
    custom doorway width, and distinct roof styles (flat, gable_red, loft_dome, pitched_tile, mansard_slate, tube_tank).
    """
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    if not facing_east:
        root.rotation_euler = Euler((0, 0, math.pi))
    link_obj(root, col)

    hx = depth_x * 0.5
    hy = width_y * 0.5
    txt_rot = (math.pi / 2, 0, math.pi / 2)

    # Plinth & Entrance Threshold Step
    add_box("Foundation", root, (0, 0, 0.14), (depth_x, width_y, 0.18), mats["ashlar_stone"], bevel=0.015, col=col)
    add_box("StepEntry", root, (hx + 0.18, 0, 0.12), (0.50, door_w + 0.35, 0.14), mats["terrazzo_paving"], bevel=0.012, col=col)

    low_h = 1.05
    low_zc = 0.23 + low_h * 0.5
    up_h = max(1.2, ground_h - 0.23 - low_h)
    up_zc = 0.23 + low_h + up_h * 0.5

    # Back Wall (Exterior + Interior)
    ext_bx = -hx + 0.06
    int_bx = -hx + 0.17
    add_box("Ext_BackLow", root, (ext_bx, 0, low_zc), (0.14, width_y - 0.12, low_h), ext_low_mat, bevel=0.010, col=col)
    add_box("Ext_BackUp", root, (ext_bx, 0, up_zc), (0.12, width_y - 0.12, up_h), ext_up_mat, bevel=0.008, col=col)
    add_box("Int_BackLow", root, (int_bx, 0, low_zc), (0.06, width_y - 0.32, low_h), int_low_mat, bevel=0.006, col=col)
    add_box("Int_BackUp", root, (int_bx, 0, up_zc), (0.06, width_y - 0.32, up_h), int_up_mat, bevel=0.006, col=col)

    # Side Walls with Multi-Bay Windows (scaled proportionally to building depth)
    num_side_bays = 3 if depth_x >= 14.0 else (2 if depth_x >= 9.0 else 1)
    bay_span = (depth_x - 0.8) / num_side_bays
    win_len = min(1.85, bay_span * 0.45)
    win_zc = min(2.05, ground_h - 0.90)
    win_h = max(1.05, min(1.55, ground_h - 1.95))

    for sy_name, sgn in (("SideL", -1.0), ("SideR", 1.0)):
        sy = sgn * (hy - 0.10)
        ext_y = sy + sgn * 0.05
        int_y = sy - sgn * 0.07
        for prefix, wy, thk in (("Ext", ext_y, 0.14), ("Int", int_y, 0.06)):
            low_m = ext_low_mat if prefix == "Ext" else int_low_mat
            up_m = ext_up_mat if prefix == "Ext" else int_up_mat
            add_box(f"{prefix}_{sy_name}_Low", root, (0, wy, low_zc), (depth_x - 0.22, thk, low_h), low_m, bevel=0.008, col=col)
            # Build continuous rhythmic bays along X
            start_x = -hx + 0.4
            for bi in range(num_side_bays):
                bx = start_x + (bi + 0.5) * bay_span
                pier_w = (bay_span - win_len) * 0.5
                if pier_w > 0.15:
                    add_box(f"{prefix}_{sy_name}_PierL_{bi}", root, (bx - (win_len + pier_w) * 0.5, wy, up_zc), (pier_w, thk, up_h), up_m, bevel=0.008, col=col)
                    add_box(f"{prefix}_{sy_name}_PierR_{bi}", root, (bx + (win_len + pier_w) * 0.5, wy, up_zc), (pier_w, thk, up_h), up_m, bevel=0.008, col=col)
                lint_h = max(0.35, ground_h - (win_zc + win_h * 0.5))
                lint_z = ground_h - lint_h * 0.5
                add_box(f"{prefix}_{sy_name}_WinLintel_{bi}", root, (bx, wy, lint_z), (win_len + 0.10, thk, lint_h), up_m, bevel=0.008, col=col)
        for bi in range(num_side_bays):
            bx = (-hx + 0.4) + (bi + 0.5) * bay_span
            add_box(f"{sy_name}_WinFrame_{bi}", root, (bx, sy, win_zc), (win_len, 0.22, 0.09), mats["steel_black"], bevel=0.006, col=col)
            add_box(f"{sy_name}_Glass_{bi}", root, (bx, sy, win_zc), (win_len - 0.10, 0.04, win_h), mats["glass"], bevel=0.004, col=col)
            if bi == 0:
                add_box(f"{sy_name}_Sill_{bi}", root, (bx, sy + sgn * 0.08, win_zc - win_h * 0.52), (win_len + 0.16, 0.16, 0.06), mats["ashlar_stone"], bevel=0.004, col=col)

    # Front Doorway Piers & Lintel (Human-scale First-Person grand doorway)
    front_pier_w = max(0.55, (width_y - door_w) * 0.5)
    front_pier_y = (door_w + front_pier_w) * 0.5
    ext_fx = hx - 0.06
    int_fx = hx - 0.18
    for side, fy in (("FrontL", -front_pier_y), ("FrontR", front_pier_y)):
        add_box(f"Ext_{side}_Low", root, (ext_fx, fy, low_zc), (0.14, front_pier_w, low_h), ext_low_mat, bevel=0.010, col=col)
        add_box(f"Ext_{side}_Up", root, (ext_fx, fy, up_zc), (0.12, front_pier_w, up_h), ext_up_mat, bevel=0.008, col=col)
        add_box(f"Int_{side}_Low", root, (int_fx, fy, low_zc), (0.06, front_pier_w - 0.08, low_h), int_low_mat, bevel=0.006, col=col)
        add_box(f"Int_{side}_Up", root, (int_fx, fy, up_zc), (0.06, front_pier_w - 0.08, up_h), int_up_mat, bevel=0.006, col=col)
        if front_pier_w >= 2.2:
            # Two large rhythmic Hanoi shopfront glass display windows + shutters on wide facades
            num_fw = 2 if front_pier_w >= 4.2 else 1
            for wi in range(num_fw):
                w_off = ((wi - (num_fw - 1) * 0.5) * (front_pier_w / num_fw))
                show_w = min(2.20, (front_pier_w / num_fw) * 0.62)
                wy_pos = fy + w_off
                add_box(f"{side}_ShowFrame_{wi}", root, (hx + 0.02, wy_pos, 2.05), (0.07, show_w, 1.55), mats["wood_dark"], bevel=0.006, col=col)
                add_box(f"{side}_ShowMullionV_{wi}", root, (hx + 0.05, wy_pos, 2.05), (0.04, 0.06, 1.48), mats["brass"], col=col)
                add_box(f"{side}_ShowMullionH_{wi}", root, (hx + 0.05, wy_pos, 2.05), (0.04, show_w - 0.12, 0.06), mats["brass"], col=col)
                add_box(f"{side}_ShowGlass_{wi}", root, (hx + 0.045, wy_pos, 2.05), (0.04, show_w - 0.16, 1.38), mats["glass"], bevel=0.003, col=col)
                add_box(f"{side}_ShowSill_{wi}", root, (hx + 0.08, wy_pos, 1.26), (0.18, show_w + 0.14, 0.08), mats["ashlar_stone"], bevel=0.004, col=col)
                add_box(f"{side}_ShowPediment_{wi}", root, (hx + 0.08, wy_pos, 2.88), (0.16, show_w + 0.22, 0.12), mats["wood_dark"], bevel=0.005, col=col)
        elif front_pier_w >= 1.15:
            show_w = front_pier_w * 0.65
            add_box(f"{side}_ShowFrame", root, (hx + 0.015, fy, 1.76), (0.05, show_w, 1.05), mats["steel_black"], bevel=0.005, col=col)
            add_box(f"{side}_ShowGlass", root, (hx + 0.038, fy, 1.76), (0.04, show_w - 0.12, 0.92), mats["glass"], bevel=0.003, col=col)

    door_h = min(3.45 if ground_h >= 4.2 else 2.68, ground_h - 0.35)
    door_top_z = 0.23 + door_h
    door_top_h = max(0.32, ground_h - door_top_z)
    add_box("Ext_DoorWall", root, (ext_fx, 0, ground_h - door_top_h * 0.5), (0.14, door_w + 0.10, door_top_h), ext_up_mat, bevel=0.008, col=col)
    add_box("Int_DoorWall", root, (int_fx, 0, ground_h - door_top_h * 0.5), (0.06, door_w + 0.10, door_top_h), int_up_mat, bevel=0.006, col=col)
    add_box("DoorPostL", root, (ext_fx, -door_w * 0.5, 0.23 + door_h * 0.5), (0.24, 0.16, door_h), mats["wood_dark"], bevel=0.008, col=col)
    add_box("DoorPostR", root, (ext_fx, door_w * 0.5, 0.23 + door_h * 0.5), (0.24, 0.16, door_h), mats["wood_dark"], bevel=0.008, col=col)
    add_box("DoorHeaderBeam", root, (ext_fx, 0, door_top_z), (0.26, door_w + 0.30, 0.16), mats["wood_dark"], bevel=0.008, col=col)

    # Doorway Brass Sconce Lanterns & Corner Structural Pilasters
    for sy_sconce in (-door_w * 0.5 - 0.42, door_w * 0.5 + 0.42):
        add_box(f"SconceArm_{sy_sconce}", root, (hx + 0.14, sy_sconce, 2.48), (0.26, 0.06, 0.06), mats["steel_black"], col=col)
        add_cyl(f"SconceLantern_{sy_sconce}", root, (hx + 0.24, sy_sconce, 2.34), 0.10, 0.20, mats["warm_glow"], verts=8, col=col)
        add_cone_frustum(f"SconceCap_{sy_sconce}", root, (hx + 0.24, sy_sconce, 2.48), 0.14, 0.02, 0.10, mats["brass"], verts=8, col=col)

    pilaster_h = ground_h + (3.2 * (floors - 1) if floors >= 2 else 0.0)
    for px_c in (-hx + 0.08, hx - 0.08):
        for py_c in (-hy + 0.08, hy - 0.08):
            add_box(f"Ext_Pilaster_{px_c}_{py_c}", root, (px_c, py_c, pilaster_h * 0.5 + 0.12), (0.28, 0.28, pilaster_h), mats["ashlar_stone"], bevel=0.012, col=col)

    # Interior Exposed Ceiling Timber Crossbeams for First-Person structural depth
    if depth_x >= 10.0:
        for bx in [x for x in range(int(-hx + 2), int(hx - 1), 3)]:
            add_box(f"Int_CeilBeam_{bx}", root, (bx, 0, ground_h - 0.14), (0.26, width_y - 0.4, 0.28), mats["wood_dark"], bevel=0.008, col=col)

    # Ground Ceiling Slab / Horizontal Cornice Molding & Grand Merchant Signboard
    add_box("Floor2_Slab", root, (0.06, 0, ground_h + 0.08), (depth_x + 0.28, width_y + 0.08, 0.22), ext_up_mat, bevel=0.010, col=col)
    add_box("Ext_CorniceGround", root, (0.0, 0, ground_h + 0.06), (depth_x + 0.34, width_y + 0.34, 0.16), mats["ashlar_stone"], bevel=0.010, col=col)
    canopy_w = min(width_y - 0.5, door_w + 4.8) if width_y >= 12.0 else width_y - 0.28
    canopy_z = door_top_z + 0.25 if ground_h >= 5.5 else ground_h - 0.18
    add_box("FrontCanopy_Slab", root, (hx + 0.58, 0, canopy_z), (1.18, canopy_w, 0.14), mats["steel_black"], rot=(0, 0.12 if floors == 1 else 0.0, 0), bevel=0.008, col=col)
    add_box("Canopy_WarmStrip", root, (hx + 0.58, 0, canopy_z - 0.07), (0.22, canopy_w - 0.80, 0.03), mats["warm_glow"], bevel=0.004, col=col)

    sign_w = min(width_y - 2.8, 7.6) if width_y >= 12.0 else min(width_y - 0.70, 4.30)
    sign_h = 1.05 if width_y >= 12.0 else 0.76
    sign_x = hx + 0.72
    sign_z = canopy_z + 0.68 if ground_h >= 5.5 else ground_h + 0.22
    add_box("MainSign_Frame", root, (sign_x, 0, sign_z), (0.14, sign_w + 0.22, sign_h), mats["wood_dark"], bevel=0.012, col=col)
    add_box("MainSign_Board", root, (sign_x + 0.04, 0, sign_z), (0.09, sign_w, sign_h - 0.14), sign_bg_mat, bevel=0.008, col=col)
    title_sz = 0.28 if width_y >= 12.0 else 0.185
    sub_sz = 0.125 if width_y >= 12.0 else 0.092
    add_3d_text("MainSign_Title", root, sign_title, (sign_x + 0.105, 0, sign_z + 0.15), txt_rot, title_sz, mats["sign_gold"], font_data=font_data, extrude=0.008, col=col)
    add_3d_text("MainSign_Sub", root, sign_sub, (sign_x + 0.105, 0, sign_z - 0.19), txt_rot, sub_sz, mats["sign_sub"], font_data=font_data, extrude=0.005, col=col)

    # Blade Sign on Left Facade Corner
    add_box("BladeBracket", root, (hx + 0.28, -hy + 0.35, 2.72), (0.06, 0.65, 0.06), mats["steel_black"], bevel=0.005, col=col)
    add_box("BladeBoard", root, (hx + 0.34, -hy + 0.32, 2.40), (0.06, 0.50, 0.90), sign_bg_mat, bevel=0.006, col=col)
    add_3d_text("Blade_TextF", root, sign_title[:10], (hx + 0.38, -hy + 0.32, 2.40), txt_rot, 0.062, mats["sign_gold"], font_data=font_data, extrude=0.003, col=col)

    # Upper Floors (when floors == 2 or 3) — Rhythmic multi-bay Hanoi/Indochine windows & iron balconies
    top_z = ground_h + 0.19
    if floors >= 2:
        num_up = 2 if floors == 3 else 1
        num_fbays = max(1, int(width_y // 4.2))
        fbay_w = (width_y - 1.2) / num_fbays
        for fl in range(num_up):
            fl_h = 3.15 if floors == 3 else 3.25
            fl_z = top_z + fl_h * 0.5
            up_wall = ext_up_mat if fl == 0 else mats["wall_yellow"]
            add_box(f"F{fl+2}_RoomBox", root, (-0.30, 0, fl_z), (depth_x - 0.80, width_y - 0.20, fl_h), up_wall, bevel=0.015, col=col)
            for fb in range(num_fbays):
                wy = (-hy + 0.6) + (fb + 0.5) * fbay_w
                fw = min(1.85, fbay_w * 0.52)
                add_box(f"F{fl+2}_WinGlass_{fb}", root, (hx - 0.65, wy, fl_z), (0.06, fw, fl_h - 0.85), mats["glass"], bevel=0.004, col=col)
                add_box(f"F{fl+2}_ShutterL_{fb}", root, (hx - 0.60, wy - fw * 0.58, fl_z), (0.06, 0.38, fl_h - 0.82), mats["brick_glazed_teal"], bevel=0.004, col=col)
                add_box(f"F{fl+2}_ShutterR_{fb}", root, (hx - 0.60, wy + fw * 0.58, fl_z), (0.06, 0.38, fl_h - 0.82), mats["brick_glazed_teal"], bevel=0.004, col=col)
            add_box(f"F{fl+2}_BalSlab", root, (hx - 0.15, 0, top_z + 0.06), (1.15, width_y - 0.16, 0.14), mats["ashlar_stone"], bevel=0.008, col=col)
            add_box(f"F{fl+2}_BalRailTop", root, (hx + 0.36, 0, top_z + 1.05), (0.08, width_y - 0.24, 0.07), mats["steel_black"], bevel=0.006, col=col)
            num_bal = max(6, int(width_y * 1.4))
            for bi in range(num_bal):
                py = -hy + 0.4 + bi * ((width_y - 0.8) / max(1, num_bal - 1))
                add_box(f"F{fl+2}_BalPost_{bi}", root, (hx + 0.36, py, top_z + 0.56), (0.04, 0.04, 0.94), mats["steel_black"], col=col)
            top_z += fl_h

    # Distinct Roof Silhouette per Shop Style (scaled to footprint depth hx)
    if roof_style == "flat":
        add_box("Roof_Parapet", root, (0.06, 0, top_z + 0.18), (depth_x + 0.26, width_y + 0.14, 0.36), mats["steel_black"], bevel=0.008, col=col)
    elif roof_style in ("gable_red", "pitched_tile"):
        ang = 0.20 if hx >= 6.0 else 0.34
        slope_len = (hx + 0.45) / math.cos(ang)
        rise = hx * math.sin(ang)
        if roof_style == "pitched_tile":
            add_box("Roof_Ceiling", root, (-0.10, 0, top_z + 0.08), (depth_x - 0.10, width_y - 0.06, 0.18), ext_up_mat, bevel=0.01, col=col)
        add_box("Roof_SlopeF", root, (hx * 0.48, 0, top_z + 0.18 + rise * 0.5), (slope_len, width_y + 0.30, 0.18), mats["roof_tile"], rot=(0, ang, 0), bevel=0.008, col=col)
        add_box("Roof_SlopeB", root, (-hx * 0.48, 0, top_z + 0.18 + rise * 0.5), (slope_len, width_y + 0.30, 0.18), mats["roof_tile"], rot=(0, -ang, 0), bevel=0.008, col=col)
        ridge_mat = mats["brass"] if roof_style == "pitched_tile" else mats["roof_tile"]
        add_box("Roof_Ridge", root, (0, 0, top_z + 0.22 + rise), (0.34, width_y + 0.34, 0.22), ridge_mat, bevel=0.008, col=col)
    elif roof_style == "mansard_slate":
        add_box("Roof_Cornice", root, (-0.10, 0, top_z + 0.12), (depth_x + 0.10, width_y + 0.10, 0.24), mats["ashlar_stone"], bevel=0.01, col=col)
        m_h = 1.45 if depth_x >= 10.0 else 0.85
        m_r1 = min(depth_x, width_y) * 0.56
        m_r2 = m_r1 * 0.70
        m = add_cone_frustum("Roof_Mansard", root, (-0.15, 0, top_z + 0.22 + m_h * 0.5), m_r1, m_r2, m_h, mats["roof_slate"], verts=4, smooth=False, rot=(0, 0, math.pi / 4), col=col)
        m.scale = Vector((depth_x / max(width_y, 1.0), 1.0, 1.0)) if width_y > depth_x else Vector((1.0, width_y / max(depth_x, 1.0), 1.0))
        add_cyl("Cupola_Base", root, (-0.15, 0, top_z + 0.24 + m_h + 0.25), 0.72 if depth_x >= 10.0 else 0.42, 0.50, mats["brass"], verts=8, col=col)
        add_sphere("Cupola_Glass", root, (-0.15, 0, top_z + 0.24 + m_h + 0.65), 0.52 if depth_x >= 10.0 else 0.32, mats["cyan_glow"], segs=14, col=col)
    elif roof_style == "tube_tank":
        add_box("Roof_Slab", root, (-0.10, 0, top_z + 0.10), (depth_x - 0.15, width_y - 0.10, 0.22), ext_up_mat, bevel=0.01, col=col)
        add_box("Roof_ParapetFront", root, (hx - 0.32, 0, top_z + 0.44), (0.18, width_y - 0.10, 0.52), mats["steel_black"], bevel=0.008, col=col)
        add_box("Roof_TankStand", root, (-1.85, 0, top_z + 0.46), (1.35, 1.55, 0.55), mats["steel_black"], bevel=0.005, col=col)
        add_cyl("Roof_WaterTank", root, (-1.85, 0, top_z + 1.05), 0.62, 1.55, mats["inox_steel"], rot=(math.pi / 2, 0, 0), verts=20, col=col)
    elif roof_style == "loft_dome":
        add_box("Roof_Terrace", root, (-0.10, 0, top_z + 0.10), (depth_x - 0.06, width_y - 0.06, 0.22), mats["steel_black"], bevel=0.01, col=col)
        dome_r = 2.45 if depth_x >= 12.0 else 1.35
        dome = bpy.data.objects.new("Observatory_Dome", None)
        dome.location = Vector((-0.35, 0, top_z + 0.20))
        dome.parent = root
        link_obj(dome, col)
        add_cyl("Dome_BaseRing", dome, (0, 0, 0.22), dome_r, 0.44, mats["brass"], verts=24, col=col)
        add_sphere("Dome_GlassCap", dome, (0, 0, 0.38), dome_r * 0.92, mats["glass"], scale=(1.0, 1.0, 0.85), segs=20, col=col)
        add_torus_ring("Dome_SteelRib", dome, (0, 0, 0.38), dome_r * 0.94, 0.07, mats["steel_black"], rot=(math.pi / 2, 0, 0), major_segs=24, col=col)
        add_sphere("Dome_InnerAether", dome, (0, 0, dome_r * 0.55), dome_r * 0.28, mats["cyan_glow"], segs=16, col=col)
        add_gear_wheel("Ext_GableGear_Big", root, (hx + 0.16, -2.55 if width_y >= 12.0 else -1.55, top_z + 0.95), 0.78 if depth_x >= 12.0 else 0.52, 0.12, 12, mats["brass"], rot=(0, math.pi / 2, 0), col=col)
        add_gear_wheel("Ext_GableGear_Small", root, (hx + 0.16, -1.45 if width_y >= 12.0 else -0.82, top_z + 0.58), 0.46 if depth_x >= 12.0 else 0.32, 0.12, 8, mats["gold"], rot=(0, math.pi / 2, 0.25), col=col)

    _add_potted_plant(f"{name}_PorchPlantL", root, (hx + 0.35, -hy + 0.65, 0.23), mats, col, scale=1.05)
    _add_potted_plant(f"{name}_PorchPlantR", root, (hx + 0.35, hy - 0.65, 0.23), mats, col, scale=1.05)
    return root


def _add_shelf_decor_prop(name, prop_kind, loc, mats, col, parent, rot_z=0.0):
    if prop_kind == "vase_jade":
        add_cyl(name, parent, (loc[0], loc[1], loc[2] + 0.13), 0.095, 0.26, mats["jade"], col=col)
    elif prop_kind == "vase_brass":
        add_cyl(name, parent, (loc[0], loc[1], loc[2] + 0.12), 0.085, 0.24, mats["brass"], col=col)
    elif prop_kind == "jar_amber":
        add_cyl(name, parent, (loc[0], loc[1], loc[2] + 0.11), 0.08, 0.22, mats["amber"], col=col)
    elif prop_kind == "jar_ceramic":
        add_cyl(name, parent, (loc[0], loc[1], loc[2] + 0.11), 0.085, 0.22, mats["dial_white"], col=col)
    elif prop_kind == "tin_red":
        add_cyl(name, parent, (loc[0], loc[1], loc[2] + 0.10), 0.085, 0.20, mats["plastic_red"], col=col)
    elif prop_kind == "tin_blue":
        add_cyl(name, parent, (loc[0], loc[1], loc[2] + 0.10), 0.085, 0.20, mats["plastic_blue"], col=col)
    elif prop_kind == "scroll":
        add_cyl(name, parent, (loc[0], loc[1], loc[2] + 0.06), 0.05, 0.30, mats["parchment"], rot=(math.pi / 2, 0, rot_z), col=col)
    elif prop_kind == "gift_box":
        add_box(name, parent, (loc[0], loc[1], loc[2] + 0.10), (0.24, 0.28, 0.18), mats["sign_red"], rot=(0, 0, rot_z), bevel=0.008, col=col)
        add_box(name + "_Band", parent, (loc[0], loc[1], loc[2] + 0.195), (0.25, 0.06, 0.015), mats["gold"], rot=(0, 0, rot_z), bevel=0.003, col=col)
    elif prop_kind == "gold_bar":
        add_box(name, parent, (loc[0], loc[1], loc[2] + 0.045), (0.16, 0.22, 0.08), mats["gold"], rot=(0, 0, rot_z), bevel=0.006, col=col)
    elif prop_kind in ("WoodCrate_Old", "BrassChest_Steam", "RunicBox_Arcane", "HoloPod_SciFi"):
        b = build_blind_box(name, prop_kind, loc, mats, col, parent=parent)
        b.rotation_euler = Euler((0, 0, rot_z))
    else:
        a = build_artifact(name, prop_kind, loc, mats, col, parent=parent, scale=1.22)
        a.rotation_euler = Euler((0, 0, rot_z))


def _add_wall_rack(root, tag, center_xy, length, is_along_y, tiers, plank_mat, items, mats, col):
    """Builds a clean minimalist wall rack along X (is_along_y=False) or Y (is_along_y=True) with curated decor."""
    cx, cy = center_xy
    top_z = max(tiers) + 0.20
    half_len = length * 0.5 - 0.08
    sz_post = (0.38, 0.06, top_z) if is_along_y else (0.06, 0.38, top_z)
    p1 = (cx, cy - half_len, top_z * 0.5 + 0.12) if is_along_y else (cx - half_len, cy, top_z * 0.5 + 0.12)
    p2 = (cx, cy + half_len, top_z * 0.5 + 0.12) if is_along_y else (cx + half_len, cy, top_z * 0.5 + 0.12)
    add_box(f"{tag}_PostL", root, p1, sz_post, mats["steel_black"], bevel=0.006, col=col)
    add_box(f"{tag}_PostR", root, p2, sz_post, mats["steel_black"], bevel=0.006, col=col)
    sz_plank = (0.38, length, 0.045) if is_along_y else (length, 0.38, 0.045)
    for tz in tiers:
        add_box(f"{tag}_Plank_{tz}", root, (cx, cy, tz), sz_plank, plank_mat, bevel=0.006, col=col)
    for idx, (kind, offset, pz) in enumerate(items):
        px, py = (cx, cy + offset) if is_along_y else (cx + offset, cy)
        rot_z = 0.0 if is_along_y else math.pi / 2
        custom = kind if kind.startswith("Shelf_") else f"{tag}_Prop_{idx}"
        prop_kind = {
            "Shelf_HoloCube": "HoloCube_SciFi",
            "Shelf_RunicMask": "RunicMask_Arcane",
            "Shelf_Bird": "AutomatonBird_Brass",
            "Shelf_VoidPrism": "VoidPrism_Alien",
        }.get(kind, kind)
        _add_shelf_decor_prop(custom, prop_kind, (px, py, pz), mats, col, root, rot_z=rot_z)


def build_blindbox_house_unique(loc, mats, font_data, rng, col):
    """
    Shop 1 (Nhà Niêm Phong — Dì Mận + Sảnh Khui Hòm): High-craft non-boxy rebuild matching
    concept_1_nha_niem_phong_exterior.jpg & concept_2_nha_niem_phong_interior.jpg:
    - Exterior: Yin-yang barrel-tile curved roof + upturned corner horns, profiled multi-step cornices,
      3D staggered exposed brick patches, heritage-green shopfront + arched luminous transom,
      carved signboard + sculpted crimson wax seal, wrought-iron S-scroll balconies + brass gears,
      arched shuttered French windows, S-scroll lamp brackets, bulged stave barrels, upper pulley crane.
    - Interior: Encaustic tile floor, 6 sculpted classical octagonal pedestals, 8-arm S-scroll chandelier
      + pulley chains + tilted giant wax-sealed crate, lathe-colonnette counter + brass balance scale,
      arched backlit mahogany sample cabinet, dual radial masonry warehouse arches into Khu C,
      mezzanine with lathe-turned balusters ('con tiện'), and South Unboxing Wing radial archway.
    """
    root = _build_custom_shop_envelope(
        "Shop_BlindBox_House", loc, True,
        depth_x=18.0, width_y=22.0, door_w=4.6, ground_h=6.60, floors=1, roof_style="flat",
        ext_low_mat=mats["brick_exposed"], ext_up_mat=mats["plaster_weathered"],
        int_low_mat=mats["wood_dark"], int_up_mat=mats["plaster_weathered"],
        sign_bg_mat=mats["wood_dark"], sign_title="NHÀ NIÊM PHONG", sign_sub="DÌ MẬN · CỔ VẬT ĐA THẾ GIỚI",
        mats=mats, font_data=font_data, col=col,
    )

    # Remove generic flat ceiling/roof/door/sconce boxes from the shared shell
    drop_exact = {
        "Floor2_Slab", "Roof_Parapet", "FrontCanopy_Slab", "Canopy_WarmStrip",
        "MainSign_Frame", "MainSign_Board", "MainSign_Title", "MainSign_Sub",
        "DoorPostL", "DoorPostR", "DoorHeaderBeam", "Ext_DoorWall", "Int_DoorWall",
        "Ext_CorniceGround",
    }
    drop_prefixes = ("Int_CeilBeam_", "SconceArm_", "SconceLantern_", "SconceCap_")
    for o in list(root.children_recursive):
        if o.name in drop_exact or o.name.startswith(drop_prefixes):
            bpy.data.objects.remove(o, do_unlink=True)

    hx, hy = 9.0, 11.0
    txt_rot = (math.pi / 2, 0, math.pi / 2)

    # === 1. UPPER WALL RING, PROFILED CORNICE BELTS & 3D PEELING BRICK RELIEFS ===
    add_box("F2_Ext_Front", root, (hx - 0.12, 0, 8.20), (0.24, 21.8, 3.20), mats["plaster_weathered"], bevel=0.01, col=col)
    add_box("F2_Ext_Back", root, (-hx + 0.12, 0, 8.20), (0.24, 21.8, 3.20), mats["plaster_weathered"], bevel=0.01, col=col)
    add_box("F2_Ext_SideS", root, (0, -hy + 0.12, 8.20), (17.8, 0.24, 3.20), mats["plaster_weathered"], bevel=0.01, col=col)
    add_box("F2_Ext_SideN", root, (0, hy - 0.12, 8.20), (17.8, 0.24, 3.20), mats["plaster_weathered"], bevel=0.01, col=col)

    # Profiled multi-step classical sandstone cornice belts around building perimeter
    cornice_prof = [(0.0, -0.09), (0.14, -0.09), (0.18, -0.02), (0.26, 0.04), (0.28, 0.12), (0.0, 0.12)]
    for cz, cname in ((4.88, "Belt_F2"), (7.38, "Belt_F3"), (9.80, "Belt_Eave")):
        ring_pts = [(-hx - 0.02, -hy - 0.02, cz), (hx + 0.02, -hy - 0.02, cz), (hx + 0.02, hy + 0.02, cz), (-hx - 0.02, hy + 0.02, cz)]
        arch.add_profiled_moulding(f"Ext_{cname}", root, ring_pts, cornice_prof, (0, 0, 0), 0.0, mats["ashlar_stone"], closed=True, col=col)

    # Clean Stylized Low-Poly facade (removed peeling brick relief patches for crisp clean look)

    # === 2. HERITAGE GREEN SHOPFRONT, INWARD DOORS, CARVED SIGNBOARD & SCULPTED WAX SEAL ===
    add_box("Ext_DoorLintelWall", root, (hx - 0.08, 0, 5.15), (0.22, 4.70, 2.85), mats["plaster_weathered"], bevel=0.008, col=col)
    add_box("Shopfront_GreenFrameL", root, (hx - 0.02, -2.42, 1.95), (0.30, 0.28, 3.55), mats["shutters_green"], bevel=0.01, col=col)
    add_box("Shopfront_GreenFrameR", root, (hx - 0.02, 2.42, 1.95), (0.30, 0.28, 3.55), mats["shutters_green"], bevel=0.01, col=col)
    add_box("Shopfront_GreenHeader", root, (hx + 0.02, 0.0, 3.72), (0.34, 5.12, 0.18), mats["shutters_green"], bevel=0.012, col=col)
    add_box("Shopfront_TransomGlow", root, (hx - 0.02, 0.0, 3.38), (0.08, 4.60, 0.56), mats["warm_glow"], col=col)
    for ti in range(-6, 7):
        add_box(f"TransomMullion_{ti}", root, (hx + 0.03, ti * 0.35, 3.38), (0.14, 0.05, 0.58), mats["shutters_green"], col=col)
    add_box("Shopfront_TransomBar", root, (hx + 0.04, 0.0, 3.38), (0.14, 4.70, 0.05), mats["shutters_green"], col=col)

    # Swinging Mahogany Double Doors with Carved Dado Panels & Turned Brass Handles
    for d_sgn, d_ang in ((-1.0, math.radians(82)), (1.0, math.radians(-82))):
        door = bpy.data.objects.new(f"OpenEntryDoor_{d_sgn}", None)
        door.location = Vector((hx - 0.16, d_sgn * 2.28, 0.24))
        door.rotation_euler = Euler((0, 0, d_ang))
        door.parent = root
        link_obj(door, col)
        add_box("Leaf", door, (0, -d_sgn * 0.62, 1.46), (0.09, 1.24, 2.92), mats["wood_dark"], bevel=0.012, col=col)
        add_box("DoorTrimTop", door, (0, -d_sgn * 0.62, 2.25), (0.11, 0.88, 0.92), mats["wood_light"], bevel=0.008, col=col)
        add_box("DoorTrimBot", door, (0, -d_sgn * 0.62, 0.72), (0.11, 0.88, 0.72), mats["wood_light"], bevel=0.008, col=col)
        arch.add_lathe_mesh("BrassKnob", door, [(0.015, -0.10), (0.024, -0.05), (0.016, 0.0), (0.024, 0.05), (0.015, 0.10)], (0.075, -d_sgn * 1.06, 1.38), mats["brass"], segments=12, col=col)

    # Carved Mahogany NHÀ NIÊM PHONG Signboard + Sculpted Dripping Crimson Wax Seal
    add_box("ConceptSign_OuterGold", root, (hx + 0.22, 0.0, 4.24), (0.14, 5.55, 1.18), mats["gold"], bevel=0.018, col=col)
    add_box("ConceptSign_DarkWood", root, (hx + 0.26, 0.0, 4.24), (0.14, 5.20, 0.96), mats["wood_dark"], bevel=0.012, col=col)
    add_3d_text("ConceptSign_Title", root, "NHÀ NIÊM PHONG", (hx + 0.36, 0.0, 4.22), txt_rot, 0.36, mats["sign_gold"], font_data=font_data, extrude=0.015, col=col)
    wax_prof = [(0.0, 0.0), (0.60, 0.0), (0.62, 0.06), (0.50, 0.10), (0.44, 0.06), (0.40, 0.12), (0.0, 0.12)]
    arch.add_lathe_mesh("WaxEmblem_Sculpted", root, wax_prof, (hx + 0.34, 0.0, 4.86), mats["wax_red"], segments=24, rot=(0, math.pi / 2, 0), col=col)
    add_torus_ring("WaxEmblem_GoldSeal", root, (hx + 0.44, 0.0, 4.86), 0.38, 0.028, mats["gold"], rot=(0, math.pi / 2, 0), col=col)

    # S-Scroll Wrought-Iron Entrance Lantern Brackets & Hexagonal Lanterns
    for ly in (-2.75, 2.75):
        arm_pts = [(hx + 0.05, ly, 2.75), (hx + 0.28, ly, 3.05), (hx + 0.56, ly, 3.12), (hx + 0.62, ly, 2.94)]
        arch.add_bezier_scroll_mesh(f"ExtLampScroll_{ly}", root, arm_pts, 0.024, (0, 0, 0), mats["steel_black"], resolution=8, col=col)
        arch.add_lathe_mesh(f"ExtLampCage_{ly}", root, [(0.0, 2.52), (0.11, 2.58), (0.14, 2.78), (0.08, 2.86), (0.0, 2.86)], (hx + 0.62, ly, 0.0), mats["warm_glow"], segments=6, smooth=False, col=col)
        arch.add_lathe_mesh(f"ExtLampCap_{ly}", root, [(0.19, 2.85), (0.04, 2.98), (0.0, 3.04)], (hx + 0.62, ly, 0.0), mats["steel_black"], segments=6, smooth=False, col=col)

    # Stacked Front Porch Slatted Crates + Curved Stave Wooden Barrels
    for cx, cy, cz, csz in ((9.55, -5.2, 0.46, (0.85, 0.85, 0.64)), (9.55, -4.3, 0.42, (0.72, 0.72, 0.56)), (9.55, -4.8, 1.02, (0.70, 0.70, 0.50)), (9.55, 5.2, 0.46, (0.88, 0.88, 0.64))):
        add_box(f"ExtCrate_{cy}_{cz}", root, (cx, cy, cz), csz, mats["wood_light"], bevel=0.015, col=col)
    arch.add_bulged_stave_barrel("ExtBarrel_R", root, 0.34, 0.84, (9.55, 6.2, 0.14), mats["wood_dark"], mats["steel_black"], col=col)
    arch.add_bulged_stave_barrel("ExtBarrel_L", root, 0.32, 0.80, (9.55, -6.2, 0.14), mats["wood_dark"], mats["steel_black"], col=col)

    # === 3. UPPER FLOORS: WROUGHT-IRON S-SCROLL BALCONIES + ARCHED SHUTTERS + PULLEY CRANE ===
    bal_edge_prof = [(0.0, -0.08), (0.10, -0.08), (0.14, 0.0), (0.16, 0.08), (0.0, 0.08)]
    for fz, fl_name in ((4.95, "F2"), (7.45, "F3")):
        add_box(f"{fl_name}_BalDeck", root, (hx + 0.45, 0, fz), (1.15, 20.8, 0.16), mats["ashlar_stone"], bevel=0.01, col=col)
        arch.add_profiled_moulding(f"{fl_name}_BalMould", root, [(hx + 1.02, -10.4, fz), (hx + 1.02, 10.4, fz)], bal_edge_prof, (0, 0, 0), 0.0, mats["ashlar_stone"], col=col)
        # Sculpted wrought-iron S-scroll balcony bays flanking toothed brass clockwork gears
        for bi, by in enumerate((-7.8, -3.9, 0.0, 3.9, 7.8)):
            arch.add_wrought_iron_balcony_bay(f"{fl_name}_IronBay_{bi}", root, 3.85, 0.98, (hx + 0.98, by, fz + 0.08), mats["steel_black"], mats["brass"], add_gear_wheel, col=col)
        for wi, wy in enumerate((-7.2, -3.6, 0.0, 3.6, 7.2)):
            is_open = (wi % 2 == 1)
            add_box(f"{fl_name}_WinBack_{wi}", root, (hx + 0.02, wy, fz + 1.32), (0.08, 1.35, 1.95), mats["warm_glow"] if is_open else mats["steel_black"], col=col)
            # Moulded pediment crown above each French window
            arch.add_profiled_moulding(f"{fl_name}_WinPed_{wi}", root, [(hx + 0.08, wy - 0.85, fz + 2.34), (hx + 0.08, wy + 0.85, fz + 2.34)], [(0, -0.05), (0.18, -0.05), (0.22, 0.04), (0, 0.08)], (0, 0, 0), 0.0, mats["ashlar_stone"], col=col)
            add_box(f"{fl_name}_ShutL_{wi}", root, (hx + 0.14, wy - 0.82, fz + 1.32), (0.08, 0.65, 1.98), mats["shutters_green"], rot=(0, 0, -0.55 if is_open else 0.0), bevel=0.005, col=col)
            add_box(f"{fl_name}_ShutR_{wi}", root, (hx + 0.14, wy + 0.82, fz + 1.32), (0.08, 0.65, 1.98), mats["shutters_green"], rot=(0, 0, 0.45 if is_open else 0.0), bevel=0.005, col=col)

    # Projecting Timber & Grooved Brass Pulley Crane with Braided Ropes & Suspended Cargo Crate
    add_box("ExtCrane_Beam", root, (hx + 1.15, 3.6, 7.15), (2.65, 0.26, 0.28), mats["wood_dark"], bevel=0.012, col=col)
    add_box("ExtCrane_Brace", root, (hx + 0.72, 3.6, 6.55), (1.75, 0.18, 0.18), mats["wood_dark"], rot=(0, -0.78, 0), col=col)
    sheave_prof = [(0.0, -0.05), (0.26, -0.05), (0.28, -0.025), (0.21, 0.0), (0.28, 0.025), (0.26, 0.05), (0.0, 0.05)]
    arch.add_lathe_mesh("ExtCrane_Wheel1", root, sheave_prof, (hx + 2.15, 3.6, 6.82), mats["brass"], segments=18, rot=(math.pi / 2, 0, 0), col=col)
    arch.add_lathe_mesh("ExtCrane_Wheel2", root, sheave_prof, (hx + 1.45, 3.6, 6.82), mats["brass"], segments=18, rot=(math.pi / 2, 0, 0), scale=(0.8, 0.8, 0.8), col=col)
    arch.add_bezier_scroll_mesh("ExtCrane_RopeL", root, [(hx + 2.02, 3.6, 6.72), (hx + 2.00, 3.6, 5.55), (hx + 1.96, 3.6, 4.45)], 0.024, (0, 0, 0), mats["parchment"], col=col)
    arch.add_bezier_scroll_mesh("ExtCrane_RopeR", root, [(hx + 1.58, 3.6, 6.72), (hx + 1.60, 3.6, 5.55), (hx + 1.64, 3.6, 4.45)], 0.024, (0, 0, 0), mats["parchment"], col=col)
    add_box("ExtCrane_Crate", root, (hx + 1.80, 3.6, 4.10), (0.95, 1.25, 0.75), mats["wood_light"], rot=(0.04, -0.06, 0.10), bevel=0.015, col=col)

    # === 4. CURVED YIN-YANG TILED ROOF ('NGÓI ÂM DƯƠNG') & CARVED EXPOSED TIMBER TRUSSES ===
    ridge_z = 11.95
    eave_z = 9.85
    arch.add_yinyang_tiled_roof("Concept_YinYangRoof", root, hx + 0.45, 22.6, eave_z, ridge_z, mats["roof_tile"], mats["wood_dark"], mats["wood_dark"], sag=0.14, col=col)
    # Carved exposed interior 'chồng rường - giá chiêng' timber roof trusses
    slope_ang = math.atan2(ridge_z - eave_z, hx + 0.4)
    slope_len = math.hypot(hx + 0.4, ridge_z - eave_z)
    for tx in (-6.8, -3.4, 0.0, 3.4, 6.8):
        add_box(f"TrussTie_{tx}", root, (tx, 0, 7.65), (0.28, 21.4, 0.28), mats["wood_dark"], bevel=0.01, col=col)
        add_box(f"TrussUpperCollar_{tx}", root, (tx, 0, 9.55), (0.22, 10.6, 0.22), mats["wood_dark"], bevel=0.008, col=col)
        arch.add_lathe_mesh(f"TrussLotusKing_{tx}", root, [(0.0, 0.0), (0.22, 0.0), (0.16, 0.28), (0.12, 1.8), (0.15, 2.2), (0.12, 3.0), (0.0, 3.0)], (tx, 0, 7.79), mats["wood_dark"], segments=12, col=col)
    for ry in (-8.2, -4.1, 0.0, 4.1, 8.2):
        add_box(f"RafterF_{ry}", root, (hx * 0.48, ry, (ridge_z + eave_z) * 0.5 - 0.16), (slope_len - 0.4, 0.22, 0.24), mats["wood_dark"], rot=(0, slope_ang, 0), col=col)
        add_box(f"RafterB_{ry}", root, (-hx * 0.48, ry, (ridge_z + eave_z) * 0.5 - 0.16), (slope_len - 0.4, 0.22, 0.24), mats["wood_dark"], rot=(0, -slope_ang, 0), col=col)

    # === 5. CONCEPT_2 SYMMETRICAL INTERIOR HALL ===
    add_box("Floor_EncausticTile", root, (0.0, 0.0, 0.24), (17.4, 21.4, 0.03), mats["encaustic_tile"], bevel=0.005, col=col)
    # Recessed brass runner strips framing central aisle
    for ay in (-1.95, 1.95):
        add_box(f"Floor_BrassInlay_{ay}", root, (2.2, ay, 0.256), (11.2, 0.05, 0.006), mats["brass"], col=col)

    # KHU A — 6 Classical Sculpted Alternating Wood & Carved-Stone Pedestals (topZ = 1.06m)
    ped_coords = [
        (5.2, -3.0), (2.4, -3.0), (-0.4, -3.0),
        (5.2, 3.0), (2.4, 3.0), (-0.4, 3.0),
    ]
    for pi, (px, py) in enumerate(ped_coords):
        p_mat = mats["wood_dark"] if pi % 2 == 0 else mats["ashlar_stone"]
        arch.add_classical_pedestal(f"Pedestal_{pi+1}", root, (px, py, 0.0), p_mat, mats["parchment"], mats["brass"], is_facing_neg_y=(py > 0), col=col)

    # HERO CENTERPIECE: 8-Arm Wrought-Iron & Brass S-Scroll Chandelier + Grooved Pulleys + Sagging Chains + Suspended Giant Crate
    arch.add_lathe_mesh("Hero_ChandRose", root, [(0.0, 0.0), (0.34, 0.0), (0.28, -0.08), (0.14, -0.18), (0.08, -0.30)], (1.2, 0.0, 7.70), mats["brass"], segments=18, col=col)
    arch.add_lathe_mesh("Hero_ChandStem", root, [(0.04, 0.0), (0.07, 0.4), (0.045, 1.5), (0.18, 1.95), (0.08, 2.2)], (1.2, 0.0, 5.45), mats["steel_black"], segments=14, col=col)
    add_torus_ring("Hero_ChandRing", root, (1.2, 0.0, 5.75), 1.32, 0.048, mats["brass"], col=col)
    for ci in range(8):
        ang = ci * (math.pi / 4.0)
        ca, sa = math.cos(ang), math.sin(ang)
        arm_pts = [
            (1.2 + ca * 0.14, sa * 0.14, 6.22),
            (1.2 + ca * 0.72, sa * 0.72, 5.95),
            (1.2 + ca * 1.18, sa * 1.18, 6.08),
            (1.2 + ca * 1.32, sa * 1.32, 5.82),
        ]
        arch.add_bezier_scroll_mesh(f"Hero_ChandArm_{ci}", root, arm_pts, 0.018, (0, 0, 0), mats["steel_black"], resolution=6, col=col)
        lx, ly = 1.2 + ca * 1.32, sa * 1.32
        arch.add_lathe_mesh(f"Hero_ChandLantern_{ci}", root, [(0.0, 5.36), (0.11, 5.42), (0.13, 5.64), (0.07, 5.70), (0.0, 5.70)], (lx, ly, 0.0), mats["warm_glow"], segments=6, smooth=False, col=col)
        arch.add_lathe_mesh(f"Hero_ChandCap_{ci}", root, [(0.16, 5.69), (0.03, 5.80), (0.0, 5.84)], (lx, ly, 0.0), mats["steel_black"], segments=6, smooth=False, col=col)

    for py in (-0.42, 0.42):
        arch.add_lathe_mesh(f"Hero_PulleyWheel_{py}", root, sheave_prof, (1.2, py, 5.05), mats["brass"], segments=18, rot=(0, math.pi / 2, 0), col=col)
        arch.add_bezier_scroll_mesh(f"Hero_Chain_{py}", root, [(1.2, py, 5.02), (1.16, py * 1.08, 4.48), (1.22, py * 1.15, 4.10)], 0.026, (0, 0, 0), mats["steel_black"], col=col)

    hero_crate = bpy.data.objects.new("Hero_CrateGroup", None)
    hero_crate.location = Vector((1.2, 0.0, 3.65))
    hero_crate.rotation_euler = Euler((0.06, -0.05, 0.14))
    hero_crate.parent = root
    link_obj(hero_crate, col)
    add_box("Hero_SuspendedCrate", hero_crate, (0, 0, 0), (1.88, 1.36, 1.18), mats["wood_aged_floor"], bevel=0.022, col=col)
    add_box("Hero_CrateBrassA", hero_crate, (0, 0, 0), (1.94, 0.18, 1.24), mats["brass"], bevel=0.008, col=col)
    add_box("Hero_CrateBrassB", hero_crate, (0, 0, 0), (0.18, 1.42, 1.24), mats["brass"], bevel=0.008, col=col)
    for sx in (0.98, -0.98):
        arch.add_lathe_mesh(f"Hero_WaxSeal_{sx}", hero_crate, [(0.0, 0.0), (0.29, 0.0), (0.31, 0.04), (0.22, 0.07), (0.0, 0.07)], (sx, 0.0, 0.0), mats["wax_red"], segments=18, rot=(0, math.pi / 2 if sx > 0 else -math.pi / 2, 0), col=col)

    # KHU B — Paneled Mahogany Counter with Turned Colonnettes, Brass Balance Scale & Illuminated Cabinet
    add_box("Counter_Base", root, (-3.4, 0.0, 0.64), (1.02, 4.42, 0.84), mats["wood_dark"], bevel=0.015, col=col)
    for cy_pan in (-1.45, 0.0, 1.45):
        add_box(f"Counter_Panel_{cy_pan}", root, (-2.86, cy_pan, 0.64), (0.04, 1.12, 0.56), mats["wood_light"], bevel=0.008, col=col)
    for cx_c in (-2.85, -3.90):
        for cy_c in (-2.22, 2.22):
            arch.add_turned_baluster(f"CounterCol_{cx_c}_{cy_c}", root, 0.86, (cx_c, cy_c, 0.24), mats["wood_dark"], radius=0.065, col=col)
    add_box("Counter_Top", root, (-3.35, 0.0, 1.10), (1.22, 4.65, 0.08), mats["wood_aged_floor"], bevel=0.014, col=col)
    add_box("Counter_BrassTrim", root, (-2.75, 0.0, 1.05), (0.04, 4.60, 0.05), mats["brass"], bevel=0.006, col=col)
    # Turned antique brass balance scale & dome desk lamp on counter
    arch.add_lathe_mesh("BrassScale_Base", root, [(0, 1.14), (0.12, 1.14), (0.09, 1.18), (0.024, 1.22), (0.020, 1.54), (0.04, 1.58), (0, 1.58)], (-3.10, 1.35, 0.0), mats["brass"], segments=16, col=col)
    add_box("BrassScale_Beam", root, (-3.10, 1.35, 1.54), (0.025, 0.44, 0.025), mats["brass"], col=col)
    for sy_pan in (1.15, 1.55):
        arch.add_lathe_mesh(f"ScalePan_{sy_pan}", root, [(0.0, 1.28), (0.075, 1.30), (0.085, 1.32), (0.078, 1.32), (0.0, 1.29)], (-3.10, sy_pan, 0.0), mats["brass"], segments=14, col=col)
    arch.add_lathe_mesh("DeskLamp_Base", root, [(0, 1.14), (0.11, 1.14), (0.07, 1.18), (0.02, 1.36), (0.14, 1.40), (0.08, 1.48), (0, 1.48)], (-3.10, 1.95, 0.0), mats["brass"], segments=16, col=col)
    add_sphere("DeskLamp_Glow", root, (-3.10, 1.95, 1.37), 0.065, mats["warm_glow"], col=col)

    # Illuminated Sample Cabinet behind Counter (X = -5.1, Y = 0.0)
    add_box("SampleCab_BackWall", root, (-5.20, 0.0, 2.05), (0.16, 4.30, 3.60), mats["wood_dark"], bevel=0.01, col=col)
    add_box("SampleCab_GlowBack", root, (-5.10, 0.0, 2.05), (0.04, 3.85, 2.40), mats["warm_glow"], col=col)
    add_box("SampleCab_Ledge", root, (-4.92, 0.0, 1.28), (0.64, 4.10, 0.08), mats["wood_light"], bevel=0.01, col=col)
    add_box("SampleCab_MidShelf", root, (-4.92, 0.0, 2.08), (0.60, 4.10, 0.07), mats["wood_light"], bevel=0.008, col=col)
    arch.add_profiled_moulding("SampleCab_Crown", root, [(-4.85, -2.25, 3.74), (-4.85, 2.25, 3.74)], [(-0.42, -0.10), (0.42, -0.10), (0.48, 0.08), (-0.42, 0.08)], (0, 0, 0), 0.0, mats["wood_dark"], col=col)
    for dy in (-2.05, -0.68, 0.68, 2.05):
        add_box(f"SampleCab_Col_{dy}", root, (-4.92, dy, 2.05), (0.62, 0.12, 3.55), mats["wood_dark"], bevel=0.008, col=col)

    # DUAL TRUE RADIAL STONE/WOOD ARCHWAYS INTO KHU C WAREHOUSE (Y = -4.6 & +4.6)
    for sgn in (-1.0, 1.0):
        add_box(f"PartPillar_Out_{sgn}", root, (-5.05, sgn * 8.4, 2.05), (0.30, 4.8, 3.65), mats["plaster_weathered"], bevel=0.012, col=col)
        arch.add_radial_stone_arch(f"PartArch_{sgn}", root, clear_w=2.80, clear_h=3.35, arch_thk=0.38, depth_x=0.38, pier_w=0.28, location=(-5.05, sgn * 4.6, 0.24), rot_z=0.0, wall_mat=mats["plaster_weathered"], voussoir_mat=mats["wood_dark"], num_voussoirs=11, col=col)

    # KHU C — Tall Warehouse Racks & Bulged Oak Barrels visible through both radial arches
    for ry in (-6.5, -1.8, 2.8, 7.5):
        _add_wall_rack(root, f"RearWarehouseRack_{ry}", (-8.15, ry), 4.20, True, (0.95, 1.95, 2.95), mats["wood_dark"], [
            ("WoodCrate_Old", -1.25, 1.00), ("BrassChest_Steam", 0.0, 1.00), ("WoodCrate_Old", 1.25, 1.00),
            ("jar_amber", -1.0, 2.00), ("BrassChest_Steam", 0.6, 2.00),
        ], mats, col)
    for by in (-5.4, -3.8, 3.8, 5.4):
        arch.add_bulged_stave_barrel(f"WarehouseBarrel_{by}", root, 0.35, 0.84, (-6.2, by, 0.24), mats["wood_dark"], mats["steel_black"], col=col)

    # KHU D — U-Shaped Dark-Mahogany Mezzanine with Lathe-Turned Balusters ('con tiện gỗ') & Curved Corbels
    add_box("Mezz_West", root, (-6.9, 0.0, 3.85), (3.6, 21.4, 0.22), mats["wood_dark"], bevel=0.012, col=col)
    add_box("Mezz_North", root, (1.4, 8.6, 3.85), (14.4, 4.2, 0.22), mats["wood_dark"], bevel=0.012, col=col)
    add_box("Mezz_South", root, (1.4, -8.6, 3.85), (14.4, 4.2, 0.22), mats["wood_dark"], bevel=0.012, col=col)
    add_box("Mezz_RailTopW", root, (-5.15, 0.0, 4.82), (0.16, 13.2, 0.10), mats["wood_dark"], bevel=0.008, col=col)
    add_box("Mezz_RailTopN", root, (1.4, 6.55, 4.82), (13.2, 0.16, 0.10), mats["wood_dark"], bevel=0.008, col=col)
    add_box("Mezz_RailTopS", root, (1.4, -6.55, 4.82), (13.2, 0.16, 0.10), mats["wood_dark"], bevel=0.008, col=col)
    for bi in range(18):
        by_w = -6.0 + bi * 0.70
        arch.add_turned_baluster(f"MezzBalW_{bi}", root, 0.84, (-5.15, by_w, 3.94), mats["wood_dark"], radius=0.045, col=col)
        bx_ns = -4.6 + bi * 0.72
        arch.add_turned_baluster(f"MezzBalN_{bi}", root, 0.84, (bx_ns, 6.55, 3.94), mats["wood_dark"], radius=0.045, col=col)
        arch.add_turned_baluster(f"MezzBalS_{bi}", root, 0.84, (bx_ns, -6.55, 3.94), mats["wood_dark"], radius=0.045, col=col)
    for lx in (-2.5, 1.5, 5.5):
        for ly in (-6.1, 6.1):
            arch.add_lathe_mesh(f"MezzLowLantern_{lx}_{ly}", root, [(0, 3.06), (0.12, 3.12), (0.14, 3.36), (0.08, 3.42), (0, 3.42)], (lx, ly, 0.0), mats["warm_glow"], segments=6, smooth=False, col=col)
            arch.add_lathe_mesh(f"MezzHighLantern_{lx}_{ly}", root, [(0, 5.42), (0.14, 5.48), (0.16, 5.76), (0.09, 5.84), (0, 5.84)], (lx, ly, 0.0), mats["warm_glow"], segments=6, smooth=False, col=col)

    # SẢNH KHUI & KIỂM TRA CỔ VẬT — Luxury VIP Unboxing Sanctuary (Y in [-10.8, -6.5], X in [0.0, 6.4])
    add_box("SouthWing_WallW", root, (-0.5, -6.5, 2.05), (3.8, 0.24, 3.60), mats["plaster_weathered"], bevel=0.01, col=col)
    add_box("SouthWing_WallE", root, (7.0, -6.5, 2.05), (3.4, 0.24, 3.60), mats["plaster_weathered"], bevel=0.01, col=col)
    arch.add_radial_stone_arch("SouthWing_Arch", root, clear_w=3.20, clear_h=3.45, arch_thk=0.36, depth_x=0.34, pier_w=0.32, location=(3.2, -6.5, 0.24), rot_z=math.pi / 2, wall_mat=mats["plaster_weathered"], voussoir_mat=mats["wood_dark"], num_voussoirs=11, col=col)

    # Crimson & Gold VIP Carpet Runner leading from main aisle through South Arch to the horizontal Unboxing Counter
    add_box("Unbox_CarpetBorder", root, (3.2, -8.10, 0.258), (3.85, 4.45, 0.010), mats["gold"], bevel=0.004, col=col)
    add_box("Unbox_CarpetRunner", root, (3.2, -8.10, 0.264), (3.60, 4.25, 0.012), mats["velvet"], bevel=0.005, col=col)

    # Luxury HORIZONTAL Paneled Mahogany Unboxing Workbench placed near South backdrop wall (wide along X)
    add_box("OpenTable_Top", root, (3.2, -9.55, 1.08), (3.35, 1.45, 0.09), mats["wood_aged_floor"], bevel=0.014, col=col)
    add_box("OpenTable_BrassEdge", root, (3.2, -9.55, 1.075), (3.38, 1.48, 0.03), mats["brass"], bevel=0.006, col=col)
    add_box("OpenTable_Apron", root, (3.2, -9.55, 0.90), (3.00, 1.20, 0.28), mats["wood_dark"], bevel=0.01, col=col)
    for tx_pan in (2.35, 3.20, 4.05):
        add_box(f"OpenTable_PanelN_{tx_pan}", root, (tx_pan, -8.93, 0.90), (0.62, 0.04, 0.18), mats["wood_light"], bevel=0.005, col=col)
        add_box(f"OpenTable_PanelS_{tx_pan}", root, (tx_pan, -10.17, 0.90), (0.62, 0.04, 0.18), mats["wood_light"], bevel=0.005, col=col)
    for tx_l in (1.72, 4.68):
        for ty_l in (-10.12, -8.98):
            arch.add_turned_baluster(f"OpenTableLeg_{tx_l}_{ty_l}", root, 0.72, (tx_l, ty_l, 0.24), mats["wood_dark"], radius=0.075, col=col)
            add_box(f"OpenTablePilaster_{tx_l}_{ty_l}", root, (tx_l, ty_l, 0.96), (0.12, 0.12, 0.14), mats["brass"], bevel=0.004, col=col)

    # 3 Recessed Brass & Plush Velvet Box Presentation Trays on the open front side (2.35/3.20/4.05, -9.45)
    for ti, tx_slot in enumerate((2.35, 3.20, 4.05)):
        add_box(f"UnboxTrayRim_{ti+1}", root, (tx_slot, -9.45, 1.124), (0.50, 0.48, 0.014), mats["brass"], bevel=0.004, col=col)
        add_box(f"UnboxTrayVelvet_{ti+1}", root, (tx_slot, -9.45, 1.130), (0.43, 0.41, 0.010), mats["velvet"], bevel=0.003, col=col)

    # Inner-Side Brass Inspection Turntable + Curved Loupe (right-back) & Seal Burner (left-back) leaving front wide open
    arch.add_lathe_mesh("OpenTable_Turntable", root, [(0, 1.126), (0.19, 1.126), (0.20, 1.138), (0.17, 1.145), (0, 1.145)], (4.58, -9.62, 0.0), mats["brass"], segments=24, col=col)
    arch.add_bezier_scroll_mesh("OpenTable_LoupeArm", root, [(4.76, -9.98, 1.13), (4.82, -9.86, 1.38), (4.68, -9.72, 1.48), (4.58, -9.62, 1.42)], 0.014, (0, 0, 0), mats["brass"], col=col)
    add_torus_ring("OpenTable_LoupeRim", root, (4.58, -9.62, 1.42), 0.10, 0.012, mats["brass"], rot=(math.radians(-15), 0, 0), col=col)
    add_cyl("OpenTable_LoupeGlass", root, (4.58, -9.62, 1.42), 0.092, 0.008, mats["glass"], rot=(math.radians(-15), 0, 0), verts=18, col=col)
    arch.add_lathe_mesh("OpenTable_Burner", root, [(0, 1.126), (0.08, 1.126), (0.055, 1.17), (0.075, 1.24), (0.03, 1.28), (0, 1.29)], (1.82, -9.85, 0.0), mats["brass"], segments=16, col=col)
    add_sphere("OpenTable_BurnerGlow", root, (1.82, -9.85, 1.31), 0.025, mats["warm_glow"], col=col)

    # Triple-Shade Brass & Amber Glass Unboxing Chandelier aligned horizontally above the 3 box trays
    add_box("OpenLamp_Beam", root, (3.2, -9.55, 2.86), (2.25, 0.12, 0.10), mats["steel_black"], bevel=0.008, col=col)
    for rx_rod in (2.50, 3.90):
        add_cyl(f"OpenLamp_Rod_{rx_rod}", root, (rx_rod, -9.55, 3.36), 0.022, 1.00, mats["steel_black"], col=col)
    for lx_sh in (2.35, 3.20, 4.05):
        arch.add_lathe_mesh(f"OpenLamp_Shade_{lx_sh}", root, [(0.03, 2.82), (0.06, 2.72), (0.28, 2.52), (0.30, 2.48), (0.26, 2.48), (0.03, 2.66)], (lx_sh, -9.55, 0.0), mats["brass"], segments=20, col=col)
        add_sphere(f"OpenLamp_Bulb_{lx_sh}", root, (lx_sh, -9.55, 2.50), 0.095, mats["warm_glow"], col=col)

    # Opulent South Sanctuary Backdrop Wall: Carved Mahogany Paneling + 3 Backlit Arched Showcase Niches + Gears
    add_box("UnboxWall_BackPanel", root, (3.2, -10.66, 2.02), (5.30, 0.14, 3.48), mats["wood_dark"], bevel=0.012, col=col)
    arch.add_profiled_moulding("UnboxWall_Crown", root, [(0.50, -10.56, 3.74), (5.90, -10.56, 3.74)], [(-0.18, -0.08), (0.18, -0.08), (0.22, 0.08), (-0.18, 0.08)], (0, 0, 0), 0.0, mats["wood_dark"], col=col)
    for ni, nx in enumerate((1.65, 3.20, 4.75)):
        add_box(f"UnboxNiche_Glow_{ni}", root, (nx, -10.56, 1.98), (0.98, 0.04, 1.25), mats["warm_glow"], col=col)
        add_box(f"UnboxNiche_Sill_{ni}", root, (nx, -10.46, 1.32), (1.12, 0.28, 0.08), mats["wood_light"], bevel=0.008, col=col)
        add_box(f"UnboxNiche_FrameL_{ni}", root, (nx - 0.54, -10.54, 1.98), (0.10, 0.14, 1.38), mats["brass"], bevel=0.004, col=col)
        add_box(f"UnboxNiche_FrameR_{ni}", root, (nx + 0.54, -10.54, 1.98), (0.10, 0.14, 1.38), mats["brass"], bevel=0.004, col=col)
    add_gear_wheel("Bench_GearA", root, (0.95, -10.52, 2.95), 0.38, 0.06, 10, mats["brass"], rot=(math.pi / 2, 0, 0), col=col)
    add_gear_wheel("Bench_GearB", root, (5.45, -10.52, 2.95), 0.34, 0.06, 9, mats["gold"], rot=(math.pi / 2, 0, 0.25), col=col)
    add_box("UnboxSign_Frame", root, (3.20, -10.52, 3.16), (2.75, 0.10, 0.44), mats["gold"], bevel=0.01, col=col)
    add_box("UnboxSign_Board", root, (3.20, -10.48, 3.16), (2.58, 0.08, 0.32), mats["wood_dark"], bevel=0.006, col=col)
    add_3d_text("UnboxSign_Title", root, "SẢNH KHUI NIÊM PHONG", (3.20, -10.43, 3.15), (math.pi / 2, 0, math.pi), 0.14, mats["sign_gold"], font_data=font_data, extrude=0.008, col=col)

    add_point_light("MainHall_Amber", root, (1.2, 0.0, 5.2), (1.0, 0.78, 0.42), 240.0, radius=0.9, col=col)
    add_point_light("Cabinet_BackAmber", root, (-4.3, 0.0, 2.1), (1.0, 0.74, 0.36), 140.0, radius=0.5, col=col)
    add_point_light("OpenWing_Spot", root, (3.2, -8.6, 2.40), (1.0, 0.86, 0.62), 140.0, radius=0.5, col=col)
    add_point_light("OpenWing_Wall", root, (3.2, -10.0, 2.20), (1.0, 0.76, 0.44), 95.0, radius=0.4, col=col)
    return root


def build_openbox_house_unique(loc, mats, font_data, rng, col):
    """
    Shop 3 (Viện Giám Định — Giáo Sư Lẫm): Grand French Colonial Hanoi research villa
    (17.0m deep x 22.0m wide x ~9.8m tall) with Glass Skylight Dome, Khu A (Đại sảnh giám định),
    Khu B (Bàn Giáo Sư Lẫm), Khu C (Phòng phân tích ánh xanh), Khu D (Thư viện lưu trữ).
    """
    root = _build_custom_shop_envelope(
        "Shop_OpenBox_House", loc, False,
        depth_x=17.0, width_y=22.0, door_w=4.2, ground_h=5.20, floors=2, roof_style="loft_dome",
        ext_low_mat=mats["ashlar_stone"], ext_up_mat=mats["wall_cream"],
        int_low_mat=mats["brick_glazed_teal"], int_up_mat=mats["wall_cream"],
        sign_bg_mat=mats["sign_blue"], sign_title="VIỆN GIÁM ĐỊNH", sign_sub="GIÁO SƯ LẪM · KHẢO CỨU CỔ VẬT",
        mats=mats, font_data=font_data, col=col,
    )

    add_box("Workshop_BaseSlab", root, (0, 0, 0.232), (16.4, 21.4, 0.028), mats["workshop_floor"], bevel=0.005, col=col)
    add_box("CenterDais_Pad", root, (0.5, 0, 0.258), (4.6, 5.2, 0.020), mats["ashlar_stone"], bevel=0.008, col=col)

    # KHU A — Đại sảnh giám định + Hero Appraisal Scanner Table in Center under Skylight Dome
    add_box("Appraisal_TableBase", root, (0.5, 0.0, 0.65), (1.80, 3.40, 0.84), mats["wood_dark"], bevel=0.015, col=col)
    add_box("Appraisal_TableTop", root, (0.5, 0.0, 1.09), (2.00, 3.65, 0.08), mats["wood_light"], bevel=0.012, col=col)
    add_box("Appraisal_VelvetMat", root, (0.5, 0.0, 1.14), (1.15, 1.65, 0.02), mats["velvet"], bevel=0.006, col=col)
    add_torus_ring("Hero_OculusRingA", root, (0.5, 0.0, 2.25), 1.05, 0.045, mats["brass"], rot=(math.pi / 4, 0.2, 0), col=col)
    add_gear_wheel("Hero_OculusGear", root, (0.5, 0.0, 2.75), 0.72, 0.06, 12, mats["gold"], rot=(math.pi / 2, 0, 0), col=col)
    # 4 Symmetrical Glass Specimen Vitrines framing Zone A
    for vx, vy in ((3.6, -3.2), (3.6, 3.2), (-2.2, -3.4), (-2.2, 3.4)):
        add_box(f"VitrineB_{vx}_{vy}", root, (vx, vy, 0.62), (1.10, 1.10, 0.78), mats["wood_dark"], bevel=0.01, col=col)
        add_box(f"VitrineG_{vx}_{vy}", root, (vx, vy, 1.48), (0.98, 0.98, 0.94), mats["glass"], bevel=0.005, col=col)
        add_box(f"VitrineCap_{vx}_{vy}", root, (vx, vy, 1.98), (1.12, 1.12, 0.08), mats["brass"], bevel=0.006, col=col)
    _add_shelf_decor_prop("Vitrine_Chalice", "Chalice_Royal", (3.6, -3.2, 1.04), mats, col, root)
    _add_shelf_decor_prop("Vitrine_Mask", "RunicMask_Arcane", (3.6, 3.2, 1.04), mats, col, root)
    _add_shelf_decor_prop("Vitrine_Compass", "Compass_Starlight", (-2.2, -3.4, 1.04), mats, col, root)
    _add_shelf_decor_prop("Vitrine_Watch", "PocketWatch_Brass", (-2.2, 3.4, 1.04), mats, col, root)

    # KHU B — Bàn Giáo Sư Lẫm (Rear Center X = -5.5, Y = 0)
    add_box("ProfDesk_Body", root, (-5.5, 0.0, 0.62), (1.20, 3.40, 0.80), mats["wood_dark"], bevel=0.012, col=col)
    add_box("ProfDesk_Top", root, (-5.5, 0.0, 1.05), (1.36, 3.60, 0.07), mats["wood_aged_floor"], bevel=0.010, col=col)
    add_cyl("Microscope_Base", root, (-5.3, 0.72, 1.12), 0.12, 0.05, mats["steel_black"], col=col)
    add_cyl("Microscope_Tube", root, (-5.3, 0.72, 1.32), 0.04, 0.36, mats["brass"], rot=(0.3, 0, 0), col=col)
    add_box("ResearchTome", root, (-5.4, -0.65, 1.11), (0.44, 0.58, 0.08), mats["wax_red"], bevel=0.006, col=col)

    # KHU C — Phòng Phân Tích Chuyên Sâu (North Wing Y in [+5.6, +10.2] with cool cyan energy scanner)
    add_box("Analysis_ScreenW", root, (0.0, 5.6, 2.05), (5.2, 0.14, 3.60), mats["steel_black"], bevel=0.008, col=col)
    add_box("Analysis_Glass", root, (0.0, 5.6, 2.15), (4.6, 0.06, 2.60), mats["glass"], col=col)
    add_cyl("Scanner_Pedestal", root, (0.0, 8.2, 0.72), 0.75, 0.96, mats["steel_black"], verts=16, col=col)
    add_torus_ring("Scanner_CyanRing1", root, (0.0, 8.2, 1.95), 0.92, 0.05, mats["cyan_glow"], rot=(math.pi / 3, 0, 0), col=col)
    add_torus_ring("Scanner_CyanRing2", root, (0.0, 8.2, 1.95), 0.78, 0.04, mats["cyan_glow"], rot=(-math.pi / 4, 0.4, 0), col=col)
    add_sphere("Scanner_CoreOrb", root, (0.0, 8.2, 1.95), 0.22, mats["cyan_glow"], col=col)

    # KHU D — Thư Viện Lưu Trữ (South Wing Y in [-10.2, -5.6]: Tall bookcases & archive desks)
    for lx in (-4.8, -1.0, 2.8):
        _add_wall_rack(root, f"LibraryShelf_{lx}", (lx, -9.8), 3.2, False, (0.85, 1.75, 2.65, 3.55), mats["wood_dark"], [
            ("scroll", -1.0, 1.80), ("jar_amber", 0.0, 1.80), ("vase_brass", 1.0, 1.80),
            ("scroll", -0.8, 2.70), ("BrassChest_Steam", 0.8, 2.70),
        ], mats, col)
    add_box("ArchiveMapTable", root, (0.5, -7.6, 0.65), (2.40, 1.20, 0.82), mats["wood_light"], bevel=0.01, col=col)
    add_box("ArchiveMapParchment", root, (0.5, -7.6, 1.07), (1.80, 0.92, 0.02), mats["parchment"], bevel=0.004, col=col)

    # Rich French Colonial Viện Giám Định Details: 4 Fluted Atrium Columns, Inlaid Brass Compass Rose, Pendant Chandeliers & Cyan Specimen Tubes
    for cx, cy in ((3.8, -5.0), (3.8, 5.0), (-2.8, -5.0), (-2.8, 5.0)):
        add_box(f"ColBase_{cx}_{cy}", root, (cx, cy, 0.44), (0.68, 0.68, 0.44), mats["ashlar_stone"], bevel=0.01, col=col)
        add_cyl(f"ColShaft_{cx}_{cy}", root, (cx, cy, 2.75), 0.24, 4.30, mats["wall_cream"], verts=16, col=col)
        add_box(f"ColCap_{cx}_{cy}", root, (cx, cy, 4.92), (0.68, 0.68, 0.26), mats["ashlar_stone"], bevel=0.01, col=col)
    add_torus_ring("Dais_CompassRim", root, (0.5, 0.0, 0.275), 2.15, 0.04, mats["brass"], col=col)
    for lx, ly in ((3.6, -3.2), (3.6, 3.2), (-2.2, -3.4), (-2.2, 3.4)):
        add_cyl(f"LabPendCord_{lx}_{ly}", root, (lx, ly, 4.35), 0.015, 1.60, mats["steel_black"], col=col)
        add_cone_frustum(f"LabPendShade_{lx}_{ly}", root, (lx, ly, 3.55), 0.28, 0.08, 0.22, mats["brass"], col=col)
        add_sphere(f"LabPendGlow_{lx}_{ly}", root, (lx, ly, 3.46), 0.09, mats["warm_glow"], col=col)
    for ti, tx in enumerate((-4.2, -2.2, 2.2, 4.2)):
        add_cyl(f"CyanTubeBase_{ti}", root, (tx, 9.6, 0.56), 0.34, 0.68, mats["steel_black"], verts=14, col=col)
        add_cyl(f"CyanTubeGlass_{ti}", root, (tx, 9.6, 1.75), 0.28, 1.75, mats["glass"], verts=14, col=col)
        add_cyl(f"CyanTubeCore_{ti}", root, (tx, 9.6, 1.75), 0.14, 1.55, mats["cyan_glow"], verts=12, col=col)

    add_point_light("Hall_NeutralWhite", root, (0.5, 0.0, 4.4), (0.98, 0.98, 1.0), 170.0, radius=0.8, col=col)
    add_point_light("Analysis_CyanSpot", root, (0.0, 8.2, 3.4), (0.35, 0.85, 1.0), 110.0, radius=0.5, col=col)
    return root


def build_kimkhuy_house_unique(loc, mats, font_data, rng, col):
    """
    Shop 2 (Kho Tạo Tác — Kim Khuy): High-end Indochine jewelry house (15.0m deep x 18.0m wide x 8.5m tall)
    with Khu A (jewelry glass vitrines & central resonance rings), Khu B (Kim Khuy crafting counter),
    Khu C (Tường Bí Khế lock/unlock shrine wall), Khu D (rear arcane-clockwork workshop).
    """
    root = _build_custom_shop_envelope(
        "Shop_KimKhuy_House", loc, True,
        depth_x=15.0, width_y=18.0, door_w=3.8, ground_h=4.80, floors=2, roof_style="mansard_slate",
        ext_low_mat=mats["stone_dark"], ext_up_mat=mats["wall_yellow"],
        int_low_mat=mats["brick_glazed_teal"], int_up_mat=mats["wall_cream"],
        sign_bg_mat=mats["sign_blue"], sign_title="KHO TẠO TÁC KIM KHUY", sign_sub="ĐÔNG DƯƠNG KIM HOÀN · CỔ TỰ & CỘNG HƯỞNG",
        mats=mats, font_data=font_data, col=col,
    )

    add_box("Marble_FloorSlab", root, (0.0, 0.0, 0.232), (14.4, 17.4, 0.028), mats["workshop_floor"], bevel=0.005, col=col)
    add_box("Gold_CenterDais", root, (1.2, 0.0, 0.262), (3.8, 3.8, 0.04), mats["ashlar_stone"], bevel=0.008, col=col)

    # HERO ASSET: Central Mechanical Resonance Ring Device on Dais
    add_cyl("Hero_ResPedestal", root, (1.2, 0.0, 0.65), 0.72, 0.78, mats["stone_dark"], verts=16, col=col)
    add_cyl("Hero_ResBrassCap", root, (1.2, 0.0, 1.06), 0.78, 0.06, mats["gold"], verts=16, col=col)
    add_torus_ring("Hero_ResRingOuter", root, (1.2, 0.0, 1.95), 0.75, 0.045, mats["gold"], rot=(0.45, 0.3, 0), col=col)
    add_torus_ring("Hero_ResRingInner", root, (1.2, 0.0, 1.95), 0.54, 0.035, mats["brass"], rot=(-0.55, 0.8, 0), col=col)
    add_sphere("Hero_ResCoreGem", root, (1.2, 0.0, 1.95), 0.18, mats["arcane_glow"], col=col)

    # KHU A — Jewelry Display Vitrines & 3 Daily Pedestals flanking center
    for py in (-2.8, 2.8):
        add_box(f"JewelVitrine_B_{py}", root, (3.4, py, 0.62), (1.30, 1.80, 0.80), mats["stone_dark"], bevel=0.01, col=col)
        add_box(f"JewelVitrine_G_{py}", root, (3.4, py, 1.44), (1.18, 1.68, 0.82), mats["glass"], bevel=0.004, col=col)
        add_box(f"JewelVitrine_Trim_{py}", root, (3.4, py, 1.88), (1.32, 1.82, 0.07), mats["gold"], bevel=0.006, col=col)
    for pi, (px, py) in enumerate(((1.2, -3.4), (1.2, 3.4), (3.8, 0.0))):
        if px == 3.8:
            continue
        add_cyl(f"TactPed_{pi}", root, (px, py, 0.68), 0.38, 0.90, mats["stone_dark"], verts=12, col=col)
        add_cyl(f"TactCap_{pi}", root, (px, py, 1.15), 0.42, 0.05, mats["gold"], verts=12, col=col)

    # KHU B — Quầy Kim Khuy (West Center X = -2.8, Y in [-2.8, +2.8]) + 6 Velvet Display Pads
    add_box("Counter_Base", root, (-2.8, 0, 0.66), (1.10, 5.60, 0.88), mats["stone_dark"], bevel=0.012, col=col)
    add_box("Counter_Top", root, (-2.75, 0, 1.14), (1.26, 5.80, 0.08), mats["wood_light"], bevel=0.010, col=col)
    add_box("Counter_GoldTrim", root, (-2.18, 0, 1.06), (0.05, 5.65, 0.06), mats["gold"], bevel=0.006, col=col)
    for i in range(6):
        py = -2.20 + i * 0.88
        add_box(f"DisplayPad_{i+1}", root, (-2.62, py, 1.19), (0.42, 0.42, 0.022), mats["velvet"], bevel=0.004, col=col)

    # KHU C — Tường Bí Khế (South Wall Y = -8.4, X in [-3.5, +4.5]): 6 Recessed Lock/Unlock Shrines
    add_box("BikheWall_Frame", root, (0.5, -8.35, 2.25), (8.2, 0.24, 3.60), mats["wood_dark"], bevel=0.012, col=col)
    for bi in range(6):
        bx = -2.6 + bi * 1.24
        n_mat = mats["cyan_glow"] if bi < 2 else mats["steel_black"]
        add_box(f"Bikhe_Niche_{bi}", root, (bx, -8.22, 2.25), (0.92, 0.14, 1.45), mats["ashlar_stone"], bevel=0.008, col=col)
        add_torus_ring(f"Bikhe_LockRing_{bi}", root, (bx, -8.14, 2.25), 0.28, 0.035, mats["gold"], rot=(math.pi / 2, 0, 0), col=col)
        add_sphere(f"Bikhe_Core_{bi}", root, (bx, -8.12, 2.25), 0.11, n_mat, col=col)

    # KHU D — Phòng Chế Tác Riêng (Rear X in [-7.1, -4.4] behind brass-framed glass screen)
    add_box("CraftScreen_Base", root, (-4.4, 0, 0.68), (0.14, 13.0, 0.90), mats["stone_dark"], bevel=0.008, col=col)
    add_box("CraftScreen_Glass", root, (-4.4, 0, 2.40), (0.06, 12.6, 2.45), mats["glass"], col=col)
    add_box("CraftBench", root, (-6.2, 0, 0.65), (1.2, 5.2, 0.85), mats["wood_dark"], bevel=0.01, col=col)
    add_gear_wheel("CraftGearA", root, (-6.2, -1.2, 1.65), 0.45, 0.08, 10, mats["brass"], rot=(0, math.pi / 2, 0), col=col)
    add_gear_wheel("CraftGearB", root, (-6.2, 1.2, 1.65), 0.45, 0.08, 10, mats["gold"], rot=(0, math.pi / 2, 0.2), col=col)

    # Rich Kho Tạo Tác Details: 4 Gold-Capped Marble Columns, Overhead Chandelier, Vitrine Jewelry & Wall Tapestry
    for cx, cy in ((4.2, -4.6), (4.2, 4.6), (-1.2, -4.6), (-1.2, 4.6)):
        add_box(f"KimColBase_{cx}_{cy}", root, (cx, cy, 0.42), (0.64, 0.64, 0.42), mats["stone_dark"], bevel=0.01, col=col)
        add_cyl(f"KimColShaft_{cx}_{cy}", root, (cx, cy, 2.55), 0.22, 3.90, mats["brick_glazed_teal"], verts=16, col=col)
        add_box(f"KimColCap_{cx}_{cy}", root, (cx, cy, 4.56), (0.66, 0.66, 0.22), mats["gold"], bevel=0.008, col=col)
    add_torus_ring("ChandelierRing", root, (1.2, 0.0, 4.05), 1.35, 0.055, mats["gold"], col=col)
    for gi in range(6):
        ang = gi * (math.pi / 3.0)
        add_sphere(f"ChandelierOrb_{gi}", root, (1.2 + math.cos(ang) * 1.35, math.sin(ang) * 1.35, 3.95), 0.11, mats["warm_glow"], col=col)
    for py in (-2.8, 2.8):
        add_cone_frustum(f"VitRingConeA_{py}", root, (3.4, py - 0.35, 1.12), 0.12, 0.02, 0.22, mats["velvet"], col=col)
        add_sphere(f"VitGemA_{py}", root, (3.4, py - 0.35, 1.26), 0.07, mats["arcane_glow"], col=col)
        add_cone_frustum(f"VitRingConeB_{py}", root, (3.4, py + 0.35, 1.12), 0.12, 0.02, 0.22, mats["velvet"], col=col)
        add_sphere(f"VitGemB_{py}", root, (3.4, py + 0.35, 1.26), 0.07, mats["cyan_glow"], col=col)
    add_box("Tapestry_Frame", root, (0.5, 8.62, 2.45), (3.20, 0.08, 1.85), mats["gold"], bevel=0.01, col=col)
    add_box("Tapestry_Cloth", root, (0.5, 8.57, 2.45), (2.90, 0.04, 1.55), mats["brick_glazed_teal"], bevel=0.005, col=col)
    add_torus_ring("Tapestry_Mandala", root, (0.5, 8.53, 2.45), 0.56, 0.04, mats["gold"], rot=(math.pi / 2, 0, 0), col=col)

    add_point_light("Jewelry_GoldSpot", root, (1.2, 0.0, 3.9), (1.0, 0.92, 0.72), 145.0, radius=0.6, col=col)
    return root


def build_anhbay_house_unique(loc, mats, font_data, rng, col):
    """
    Shop 5 (Tiệm Cầm Cố — Bà Cầm / Khu Sổ Cái): Deep narrow Hanoi tube shop (18.0m deep x 12.0m wide x ~9.6m tall)
    with Khu A (Phòng giao dịch & song sắt), Khu B (Kho tài sản & Hệ Tủ Trăm Ngăn), Khu C (Phòng hợp đồng phía sau).
    """
    root = _build_custom_shop_envelope(
        "Shop_AnhBay_House", loc, True,
        depth_x=18.0, width_y=12.0, door_w=3.2, ground_h=4.40, floors=3, roof_style="tube_tank",
        ext_low_mat=mats["brick_exposed"], ext_up_mat=mats["wall_cream"],
        int_low_mat=mats["wood_dark"], int_up_mat=mats["wall_cream"],
        sign_bg_mat=mats["sign_red"], sign_title="TIỆM CẦM CỐ · SỔ NỢ", sign_sub="BÀ CẦM & ANH BẢY · ĐỊNH GIÁ TÀI SẢN",
        mats=mats, font_data=font_data, col=col,
    )

    for row in range(22):
        py = -5.4 + row * 0.50
        add_box(f"FloorPlank_{row}", root, (0, py, 0.235), (17.4, 0.46, 0.03), mats["wood_aged_floor"] if row % 2 == 0 else mats["wood_dark"], bevel=0.004, col=col)

    # KHU A — Phòng Giao Dịch (Front X in [+1.5, +8.4]): Heavy counter with brass security grille & waiting bench
    add_box("Counter_Base", root, (2.4, -0.8, 0.66), (1.15, 5.40, 0.88), mats["wood_dark"], bevel=0.015, col=col)
    add_box("Counter_Top", root, (2.45, -0.8, 1.14), (1.30, 5.60, 0.08), mats["wood_aged_floor"], bevel=0.012, col=col)
    for gi in range(11):
        gy = -3.3 + gi * 0.50
        add_cyl(f"PawnGrilleBar_{gi}", root, (2.85, gy, 1.98), 0.025, 1.65, mats["steel_black"], verts=10, col=col)
    add_box("PawnGrilleTop", root, (2.85, -0.8, 2.82), (0.08, 5.40, 0.10), mats["brass"], bevel=0.005, col=col)
    add_box("LedgerCover", root, (2.52, -0.2, 1.19), (0.40, 0.54, 0.04), mats["wax_red"], bevel=0.005, col=col)
    add_box("LedgerPages", root, (2.52, -0.2, 1.215), (0.34, 0.48, 0.025), mats["parchment"], bevel=0.003, col=col)
    add_box("WaitBench", root, (5.2, 4.6, 0.46), (3.4, 0.65, 0.44), mats["wood_dark"], bevel=0.01, col=col)

    # KHU B — Kho Tài Sản & HERO ASSET: Hệ Tủ Lưu Trữ Trăm Ngăn Khóa Cơ Khí (Middle X in [-4.5, +0.8])
    add_box("VaultWall_Core", root, (-1.8, -5.2, 2.20), (6.2, 0.45, 3.80), mats["steel_black"], bevel=0.01, col=col)
    for col_i in range(6):
        for row_i in range(4):
            dx = -4.2 + col_i * 0.96
            dz = 0.72 + row_i * 0.82
            add_box(f"Drawer_{col_i}_{row_i}", root, (dx, -4.92, dz), (0.84, 0.14, 0.70), mats["wood_dark"], bevel=0.006, col=col)
            add_box(f"DrawerPlate_{col_i}_{row_i}", root, (dx, -4.84, dz), (0.28, 0.03, 0.14), mats["brass"], col=col)
    # Massive Circular Steel Vault Door on North-Mid Partition
    add_box("VaultFrame", root, (-3.6, 2.4, 2.05), (0.35, 3.4, 3.60), mats["stone_dark"], bevel=0.01, col=col)
    add_cyl("VaultDoorDisc", root, (-3.4, 2.4, 1.95), 1.05, 0.22, mats["iron"], rot=(0, math.pi / 2, 0), verts=24, col=col)
    add_cyl("VaultWheel", root, (-3.25, 2.4, 1.95), 0.36, 0.08, mats["brass"], rot=(0, math.pi / 2, 0), verts=16, col=col)

    # KHU C — Phòng Hợp Đồng Phía Sau (Rear X in [-8.5, -5.0])
    add_box("ContractDesk", root, (-6.8, -1.2, 0.65), (1.35, 3.20, 0.84), mats["wood_dark"], bevel=0.012, col=col)
    add_cyl("ContractLamp", root, (-6.6, -2.1, 1.38), 0.16, 0.12, mats["tree_foliage"], col=col)
    _add_wall_rack(root, "CollateralRack", (-8.3, 1.5), 4.2, True, (0.95, 1.85, 2.75), mats["steel_black"], [
        ("BrassChest_Steam", -1.2, 1.00), ("gold_bar", 0.0, 1.00), ("WoodCrate_Old", 1.2, 1.00),
    ], mats, col)

    # Rich Tiệm Cầm Cố Details: Grandfather Clock, North Debt Schedule Wall Board, Crimson Drapes, Hanging Cage Lamps & Gold Stacks
    add_box("Clock_Body", root, (-5.5, 4.8, 1.25), (0.65, 0.75, 2.05), mats["wood_dark"], bevel=0.012, col=col)
    add_cyl("Clock_Face", root, (-5.15, 4.8, 1.92), 0.22, 0.05, mats["dial_white"], rot=(0, math.pi / 2, 0), col=col)
    add_box("Pawn_DebtChartFrame", root, (4.8, 5.62, 2.25), (2.40, 0.08, 1.40), mats["wood_dark"], bevel=0.01, col=col)
    add_box("Pawn_DebtChartPaper", root, (4.8, 5.56, 2.25), (2.15, 0.03, 1.15), mats["parchment"], col=col)
    add_cyl("Counter_ScaleBase", root, (2.45, -1.8, 1.20), 0.12, 0.04, mats["brass"], col=col)
    add_cyl("Counter_ScalePost", root, (2.45, -1.8, 1.44), 0.025, 0.46, mats["brass"], col=col)
    add_box("Counter_ScaleArm", root, (2.45, -1.8, 1.65), (0.03, 0.56, 0.03), mats["brass"], rot=(0.08, 0, 0), col=col)
    add_box("PassageCurtainL", root, (-3.6, 0.4, 2.20), (0.18, 0.55, 3.60), mats["velvet"], col=col)
    for li, lx in enumerate((5.2, 1.5, -2.5)):
        add_cyl(f"PawnLampCord_{li}", root, (lx, 0.0, 3.95), 0.015, 0.80, mats["steel_black"], col=col)
        add_cone_frustum(f"PawnLampShade_{li}", root, (lx, 0.0, 3.52), 0.28, 0.08, 0.22, mats["brass"], col=col)
        add_sphere(f"PawnLampGlow_{li}", root, (lx, 0.0, 3.44), 0.10, mats["warm_glow"], col=col)

    add_point_light("Pawn_DimAmber", root, (3.2, 0.0, 3.4), (1.0, 0.84, 0.56), 115.0, radius=0.5, col=col)
    return root


# ==================== 4. MODERN HANOI STREET TREES, PHO SHOP, TRA DA VE SO & TUBE HOUSES ====================

def build_hanoi_street_tree(name, loc, mats, rng, col, parent=None, scale=1.0):
    """High-craft curved Bezier Hanoi shade tree (Xà Cừ / Sấu) with hexagonal stone curb."""
    return arch.add_organic_hanoi_tree(name, loc, mats, rng, col, parent=parent, scale=scale)


def build_hanoi_pho_shop(name, loc, facing_east, mats, font_data, rng, col):
    """Quán Phở Bò Hà Nội (6.4x7.6m, cozy 1-story gable red-tile shop): steaming inox broth counter, back pantry, side spice shelf & 3 dining tables."""
    root = _build_custom_shop_envelope(
        name, loc, facing_east,
        depth_x=6.4, width_y=7.6, door_w=3.4, ground_h=3.40, floors=1, roof_style="gable_red",
        ext_low_mat=mats["ashlar_stone"], ext_up_mat=mats["wall_yellow"],
        int_low_mat=mats["terrazzo_paving"], int_up_mat=mats["wall_cream"],
        sign_bg_mat=mats["sign_red"], sign_title="PHỞ BÒ HÀ NỘI", sign_sub="GIA TRUYỀN · NƯỚC DÙNG NGON",
        mats=mats, font_data=font_data, col=col,
    )

    add_box("Pho_TileFloor", root, (0, 0, 0.245), (6.0, 7.2, 0.025), mats["terrazzo_paving"], bevel=0.004, col=col)

    # Cozy Inox Broth Sales Counter on South side
    add_box("Pho_CounterBody", root, (1.25, -1.65, 0.68), (0.96, 2.85, 0.86), mats["inox_steel"], bevel=0.012, col=col)
    add_box("Pho_GlassCaseFrame", root, (1.38, -1.65, 1.48), (0.60, 1.75, 0.72), mats["glass"], bevel=0.005, col=col)
    for idx, py in enumerate((-1.10, -2.15)):
        add_cyl(f"PhoPot_{idx+1}", root, (1.05, py, 1.08), 0.28, 0.48, mats["inox_steel"], verts=24, bevel=0.01, col=col)
        add_cyl(f"PhoSoup_{idx+1}", root, (1.05, py, 1.31), 0.25, 0.02, mats["amber"], verts=20, col=col)
        add_sphere(f"PhoSteam_{idx+1}", root, (1.05, py, 1.62), 0.16, mats["steam_mist"], scale=(0.95, 0.95, 1.3), segs=10, col=col)

    _add_wall_rack(root, "Pho_BackShelf", (-2.65, -1.20), 2.95, True, (0.85, 1.45, 2.05), mats["wood_light"], [
        ("jar_ceramic", -0.95, 1.48), ("tin_red", 0.0, 1.48), ("jar_amber", 0.95, 1.48),
        ("gift_box", -0.80, 2.08), ("vase_jade", 0.80, 2.08),
    ], mats, col)

    # 3 Inox Dining Tables along North side
    for ti, (tx, ty) in enumerate(((0.55, 1.55), (-1.45, 1.55), (-0.45, 2.65))):
        add_box(f"PhoTable_{ti}", root, (tx, ty, 0.72), (0.78, 1.08, 0.04), mats["inox_steel"], bevel=0.008, col=col)
        add_cyl(f"PhoTableLeg_{ti}", root, (tx, ty, 0.48), 0.045, 0.46, mats["inox_steel"], col=col)
        add_cyl(f"ChiliJar_{ti}", root, (tx - 0.12, ty + 0.14, 0.78), 0.04, 0.09, mats["plastic_red"], col=col)
        for sy_off in (-0.42, 0.42):
            add_cyl(f"PhoStool_{ti}_{sy_off}", root, (tx, ty + sy_off, 0.42), 0.16, 0.36, mats["inox_steel"], verts=16, col=col)

    add_point_light(f"{name}_WarmLight", root, (0.6, 0, 2.65), (1.0, 0.88, 0.68), 115.0, radius=0.45, col=col)
    return root


def build_hanoi_trada_veso(name, loc, facing_east, mats, font_data, rng, col):
    """Tiệm Bách Hóa · Trà Đá & Vé Số (5.2x6.0m, cozy 1-story flat-roofed shop): sales counter, stocked shelves & plastic tea corner."""
    root = _build_custom_shop_envelope(
        name, loc, facing_east,
        depth_x=5.2, width_y=6.0, door_w=2.8, ground_h=3.20, floors=1, roof_style="flat",
        ext_low_mat=mats["ashlar_stone"], ext_up_mat=mats["wall_yellow"],
        int_low_mat=mats["brick_glazed_teal"], int_up_mat=mats["wall_cream"],
        sign_bg_mat=mats["sign_blue"], sign_title="BÁCH HÓA · TRÀ ĐÁ VÉ SỐ", sign_sub="SỐ 12 · ĐẶC SẢN & XỔ SỐ THỦ ĐÔ",
        mats=mats, font_data=font_data, col=col,
    )

    add_box("Shop_TileBase", root, (0, 0, 0.236), (4.85, 5.65, 0.025), mats["terrazzo_paving"], bevel=0.005, col=col)
    add_box("Entry_RedMat", root, (1.15, 0, 0.256), (1.10, 1.80, 0.014), mats["vn_red_tile"], bevel=0.004, col=col)

    _add_wall_rack(root, "BackShelf", (-2.10, 0.0), 3.90, True, (0.76, 1.30, 1.84), mats["wood_light"], [
        ("tin_red", -1.25, 1.325), ("gift_box", 0.0, 1.325), ("jar_ceramic", 1.25, 1.325),
        ("tin_blue", -0.95, 1.865), ("vase_jade", 0.95, 1.865),
    ], mats, col)

    # South Sales Counter with Phích Nước, Candy Jar & Vé Số board
    add_box("Counter_Base", root, (-0.42, -1.15, 0.64), (0.82, 2.05, 0.82), mats["wood_light"], bevel=0.012, col=col)
    add_box("Counter_Top", root, (-0.38, -1.15, 1.10), (0.94, 2.20, 0.07), mats["wood_aged_floor"], bevel=0.010, col=col)
    add_box("VeSo_BoardBack", root, (-0.65, -1.55, 1.54), (0.05, 0.96, 0.78), mats["sign_blue"], bevel=0.006, col=col)
    add_3d_text("VeSo_Title", root, "TRÀ ĐÁ · VÉ SỐ", (-0.60, -1.55, 1.74), (math.pi / 2, 0, math.pi / 2), 0.105, mats["sign_gold"], font_data=font_data, extrude=0.004, col=col)
    add_cyl("PhichNuoc_Body", root, (-0.28, -0.65, 1.32), 0.085, 0.34, mats["plastic_red"], verts=20, col=col)
    add_cyl("CandyJar", root, (-0.34, -0.35, 1.26), 0.08, 0.22, mats["glass"], verts=16, col=col)

    # North Cozy Tea Table & Red/Blue Stools
    add_box("PlasticTable_Top", root, (0.95, 1.45, 0.56), (0.56, 0.56, 0.04), mats["plastic_blue"], bevel=0.01, col=col)
    add_cyl("TeaPitcher", root, (0.93, 1.42, 0.70), 0.07, 0.20, mats["jade"], verts=16, col=col)
    for si, (sx, sy, smat) in enumerate(((0.55, 1.45, mats["plastic_red"]), (0.95, 1.90, mats["plastic_blue"]), (1.35, 1.45, mats["plastic_red"]))):
        add_box(f"PlasticStool_{si}", root, (sx, sy, 0.36), (0.25, 0.25, 0.22), smat, rot=(0, 0, si * 0.4), bevel=0.01, col=col)

    add_point_light("CounterSpot", root, (0.20, 0, 2.25), (1.0, 0.90, 0.72), 65.0, radius=0.3, col=col)
    return root


def build_hanoi_shophouse_block(name, loc, facing_east, mats, font_data, rng, col):
    """Trio of adjoining 3-4 story Modern Hanoi Tube-Houses (Nhà Ống Hiện Đại Hà Nội)."""
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    if not facing_east:
        root.rotation_euler = Euler((0, 0, math.pi))
    link_obj(root, col)

    specs = [
        ("TH_Cafe", -6.0, 8.2, 5.6, 11.2, mats["wall_cream"], mats["sign_blue"], "NHÀ ĐỎ · PHỐ CỔ"),
        ("TH_TraChanh", 0.0, 8.6, 5.8, 13.8, mats["wall_yellow"], mats["sign_red"], "NHÀ ĐẤU GIÁ HÀ THÀNH"),
        ("TH_TapHoa", 6.0, 8.2, 5.6, 12.2, mats["ashlar_stone"], mats["sign_blue"], "HỘI QUÁN ĐÔNG DƯƠNG"),
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


def _add_quarter_round_sidewalk(name, parent, cx, cy, r, start_deg, end_deg, height, mat, segs=16, col=None):
    """Builds a smooth quarter-circle sidewalk slab centered at (cx, cy) from start_deg to end_deg."""
    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm = bmesh.new()
    v_bot_c = bm.verts.new((cx, cy, 0.0))
    v_top_c = bm.verts.new((cx, cy, height))
    bot_ring = []
    top_ring = []
    for i in range(segs + 1):
        ang = math.radians(start_deg + (end_deg - start_deg) * (i / segs))
        px = cx + math.cos(ang) * r
        py = cy + math.sin(ang) * r
        bot_ring.append(bm.verts.new((px, py, 0.0)))
        top_ring.append(bm.verts.new((px, py, height)))
    for i in range(segs):
        bm.faces.new((v_top_c, top_ring[i], top_ring[i + 1]))
        bm.faces.new((v_bot_c, bot_ring[i + 1], bot_ring[i]))
        bm.faces.new((bot_ring[i], bot_ring[i + 1], top_ring[i + 1], top_ring[i]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    if mat:
        obj.data.materials.append(mat)
    if parent:
        obj.parent = parent
    link_obj(obj, col or bpy.context.scene.collection)
    return obj


def _add_curved_curb_arc(name, parent, cx, cy, r, start_deg, end_deg, width, height, mat, segs=16, col=None):
    """Builds a smooth continuous curved granite curbstone arc."""
    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm = bmesh.new()
    r_in = r - width
    r_out = r
    rings = []
    for i in range(segs + 1):
        ang = math.radians(start_deg + (end_deg - start_deg) * (i / segs))
        ca, sa = math.cos(ang), math.sin(ang)
        v_in_b = bm.verts.new((cx + ca * r_in, cy + sa * r_in, 0.0))
        v_out_b = bm.verts.new((cx + ca * r_out, cy + sa * r_out, 0.0))
        v_out_t = bm.verts.new((cx + ca * r_out, cy + sa * r_out, height))
        v_in_t = bm.verts.new((cx + ca * r_in, cy + sa * r_in, height))
        rings.append((v_in_b, v_out_b, v_out_t, v_in_t))
    for i in range(segs):
        a = rings[i]
        b = rings[i + 1]
        bm.faces.new((a[3], a[2], b[2], b[3]))  # top
        bm.faces.new((a[0], b[0], b[1], a[1]))  # bottom
        bm.faces.new((a[1], b[1], b[2], a[2]))  # outer curve
        bm.faces.new((a[3], b[3], b[0], a[0]))  # inner curve
    bm.faces.new((rings[0][0], rings[0][1], rings[0][2], rings[0][3]))
    bm.faces.new((rings[-1][3], rings[-1][2], rings[-1][1], rings[-1][0]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    if mat:
        obj.data.materials.append(mat)
    if parent:
        obj.parent = parent
    link_obj(obj, col or bpy.context.scene.collection)
    set_smooth(obj)
    add_bevel(obj, width=0.012, segments=2)
    return obj


def build_hanoi_car_road_network(mats, font_data, col):
    """
    Expanded Hanoi T-Intersection Car Road Network & Rounded Sidewalks (fits 150m x 165m Masterplan):
    - East-West Car Road: X in [-75, +75], Y in [116.0, 126.0] (10m wide)
    - North Car Road Branch: X in [-6.0, +6.0], Y in [126.0, 165.0] (12m wide)
    - Continuous unbroken South Sidewalk along Y in [111.8, 116.0] (no south crosswalk).
    """
    root = bpy.data.objects.new("Hanoi_CarRoadNetwork", None)
    link_obj(root, col)

    add_box("Road_EW_Asphalt", root, (0.0, 121.0, 0.014), (150.0, 10.0, 0.028), mats["stone_dark"], bevel=0.0, col=col)
    add_box("Road_NS_Asphalt", root, (0.0, 145.5, 0.014), (12.0, 39.0, 0.028), mats["stone_dark"], bevel=0.0, col=col)

    for lx in [x for x in range(-70, 72, 6) if abs(x) >= 12]:
        add_box(f"LaneDash_EW_{lx}", root, (lx, 121.0, 0.032), (3.0, 0.18, 0.012), mats["dial_white"], bevel=0.002, col=col)
    for ly in range(132, 162, 6):
        add_box(f"LaneDash_NS_{ly}", root, (0.0, ly, 0.032), (0.18, 3.0, 0.012), mats["dial_white"], bevel=0.002, col=col)

    # 3 White Zebra Crosswalks (North, West, East of T-junction)
    for idx in range(6):
        cx = -3.75 + idx * 1.5
        add_box(f"Crosswalk_N_{idx}", root, (cx, 125.15, 0.032), (0.64, 1.75, 0.012), mats["dial_white"], bevel=0.002, col=col)
    for idx in range(6):
        cy = 117.4 + idx * 1.44
        add_box(f"Crosswalk_W_{idx}", root, (-8.2, cy, 0.032), (1.75, 0.62, 0.012), mats["dial_white"], bevel=0.002, col=col)
        add_box(f"Crosswalk_E_{idx}", root, (8.2, cy, 0.032), (1.75, 0.62, 0.012), mats["dial_white"], bevel=0.002, col=col)

    # Continuous Unbroken South Sidewalk across Market Entrance (X = -75..+75, Y = 111.8..116.0)
    add_box("SW_EW_South_Full", root, (0.0, 113.9, 0.045), (150.0, 4.2, 0.09), mats["terrazzo_paving"], bevel=0.010, col=col)
    add_box("SW_EW_SouthRed_Full", root, (0.0, 113.7, 0.092), (149.0, 1.35, 0.012), mats["vn_red_tile"], bevel=0.004, col=col)

    c_idx = 0
    for x_pos in range(-74, 75, 2):
        m = mats["ashlar_stone"] if c_idx % 2 == 0 else mats["stone_dark"]
        add_box(f"CurbEW_S_{c_idx}", root, (x_pos, 115.88, 0.065), (1.94, 0.24, 0.135), m, bevel=0.008, col=col)
        c_idx += 1

    # North Sidewalks & Rounded Corners (R=3.0m)
    for tag, sx, sw, fill_x in (("W", -42.0, 66.0, -7.5), ("E", 42.0, 66.0, 7.5)):
        add_box(f"SW_EW_North_{tag}", root, (sx, 128.1, 0.045), (sw, 4.2, 0.09), mats["terrazzo_paving"], bevel=0.010, col=col)
        add_box(f"SW_EW_NorthFill_{tag}", root, (fill_x, 129.6, 0.045), (3.0, 1.2, 0.09), mats["terrazzo_paving"], bevel=0.008, col=col)
        add_box(f"SW_EW_NorthRed_{tag}", root, (sx, 128.3, 0.092), (sw - 0.5, 1.35, 0.012), mats["vn_red_tile"], bevel=0.004, col=col)

    _add_quarter_round_sidewalk("SW_Corner_NW", root, -9.0, 129.0, 3.0, 270.0, 360.0, 0.09, mats["terrazzo_paving"], segs=16, col=col)
    _add_quarter_round_sidewalk("SW_Corner_NE", root, 9.0, 129.0, 3.0, 180.0, 270.0, 0.09, mats["terrazzo_paving"], segs=16, col=col)
    _add_curved_curb_arc("Curb_Corner_NW", root, -9.0, 129.0, 3.0, 270.0, 360.0, 0.24, 0.135, mats["ashlar_stone"], segs=16, col=col)
    _add_curved_curb_arc("Curb_Corner_NE", root, 9.0, 129.0, 3.0, 180.0, 270.0, 0.24, 0.135, mats["ashlar_stone"], segs=16, col=col)

    for tag, sx in (("W", -8.25), ("E", 8.25)):
        add_box(f"SW_NS_{tag}", root, (sx, 147.6, 0.045), (4.5, 34.8, 0.09), mats["terrazzo_paving"], bevel=0.010, col=col)
        add_box(f"SW_NSRed_{tag}", root, (sx, 147.6, 0.092), (1.35, 34.4, 0.012), mats["vn_red_tile"], bevel=0.004, col=col)

    for x_pos in [x for x in range(-74, 75, 2) if abs(x) >= 10.0]:
        m = mats["ashlar_stone"] if c_idx % 2 == 0 else mats["stone_dark"]
        add_box(f"CurbEW_N_{c_idx}", root, (x_pos, 126.12, 0.065), (1.94, 0.24, 0.135), m, bevel=0.008, col=col)
        c_idx += 1
    for cx in (-6.12, 6.12):
        for y_pos in range(130, 165, 2):
            m = mats["ashlar_stone"] if c_idx % 2 == 0 else mats["stone_dark"]
            add_box(f"CurbNS_{c_idx}", root, (cx, y_pos, 0.065), (0.24, 1.94, 0.135), m, bevel=0.008, col=col)
            c_idx += 1

    sign_corners = [
        ("Sign_SW", -5.2, 115.2, 0.0, "PHỐ DỊ VẬT HÀ THÀNH"),
        ("Sign_SE", 5.2, 115.2, 0.0, "PHỐ DỊ VẬT HÀ THÀNH"),
        ("Sign_NW", -8.8, 129.0, math.pi, "ĐẠI LỘ HÀ NỘI"),
        ("Sign_NE", 8.8, 129.0, math.pi, "ĐẠI LỘ HÀ NỘI"),
    ]
    for sname, sx, sy, srot, street_label in sign_corners:
        post = bpy.data.objects.new(sname, None)
        post.location = Vector((sx, sy, 0.09))
        post.rotation_euler = Euler((0, 0, srot))
        post.parent = root
        link_obj(post, col)
        for bi in range(7):
            m = mats["plastic_red"] if bi % 2 == 0 else mats["dial_white"]
            add_cyl(f"Band_{bi}", post, (0, 0, 0.20 + bi * 0.36), 0.042, 0.36, m, verts=12, col=col)
        add_box("StreetPlaque", post, (0, -0.04, 2.08), (1.05, 0.04, 0.24), mats["sign_blue"], bevel=0.004, col=col)
        add_3d_text("StreetText", post, street_label, (0, -0.068, 2.08), (math.pi / 2, 0, 0), 0.072, mats["dial_white"], font_data=font_data, extrude=0.002, col=col)

    return root


def build_hanoi_car(name, mats, col):
    """Stylized Modern Hanoi Sedan (~3.8m x 1.7m x 1.45m) facing +Y (Unity +Z forward)."""
    root = bpy.data.objects.new(name, None)
    link_obj(root, col)

    # Main Lower Chassis & Cabin (plastic_blue body can be tinted cleanly per vehicle in Unity)
    add_box("Car_Body", root, (0, 0, 0.56), (1.72, 3.82, 0.60), mats["plastic_blue"], bevel=0.05, col=col)
    add_box("Car_Cabin", root, (0, -0.18, 1.06), (1.46, 1.98, 0.50), mats["plastic_blue"], bevel=0.04, col=col)
    add_box("Car_Windshield", root, (0, 0.76, 1.04), (1.36, 0.24, 0.44), mats["glass"], rot=(-0.48, 0, 0), bevel=0.01, col=col)
    add_box("Car_RearGlass", root, (0, -1.14, 1.04), (1.34, 0.22, 0.40), mats["glass"], rot=(0.52, 0, 0), bevel=0.01, col=col)
    add_box("Car_SideWinL", root, (-0.735, -0.18, 1.06), (0.04, 1.62, 0.36), mats["glass"], bevel=0.005, col=col)
    add_box("Car_SideWinR", root, (0.735, -0.18, 1.06), (0.04, 1.62, 0.36), mats["glass"], bevel=0.005, col=col)

    # Bumpers, Grille, Headlights & Taillights
    add_box("Car_FrontBumper", root, (0, 1.92, 0.36), (1.74, 0.14, 0.22), mats["steel_black"], bevel=0.02, col=col)
    add_box("Car_RearBumper", root, (0, -1.92, 0.36), (1.74, 0.14, 0.22), mats["steel_black"], bevel=0.02, col=col)
    add_box("Car_Grille", root, (0, 1.92, 0.58), (0.76, 0.06, 0.20), mats["inox_steel"], bevel=0.01, col=col)
    for sx in (-0.64, 0.64):
        add_box(f"Headlight_{sx}", root, (sx, 1.91, 0.62), (0.28, 0.06, 0.14), mats["warm_glow"], bevel=0.008, col=col)
        add_box(f"Taillight_{sx}", root, (sx, -1.91, 0.64), (0.28, 0.06, 0.14), mats["plastic_red"], bevel=0.008, col=col)

    # 4 Wheels with Alloy Rims
    for wx in (-0.82, 0.82):
        for wy in (-1.18, 1.18):
            add_cyl(f"Tire_{wx}_{wy}", root, (wx, wy, 0.32), 0.32, 0.22, mats["steel_black"], rot=(0, math.pi / 2, 0), verts=18, col=col)
            add_cyl(f"Hub_{wx}_{wy}", root, (wx + (0.04 if wx > 0 else -0.04), wy, 0.32), 0.18, 0.18, mats["inox_steel"], rot=(0, math.pi / 2, 0), verts=14, col=col)
    return root


def build_hanoi_motorbike(name, mats, col):
    """Stylized Vietnamese Scooter (~1.85m x 0.66m x 1.10m) facing +Y (Unity +Z forward)."""
    root = bpy.data.objects.new(name, None)
    link_obj(root, col)

    add_box("Bike_Deck", root, (0, 0.05, 0.34), (0.44, 0.88, 0.14), mats["steel_black"], bevel=0.02, col=col)
    add_box("Bike_RearBody", root, (0, -0.44, 0.58), (0.42, 0.72, 0.34), mats["plastic_red"], bevel=0.04, col=col)
    add_box("Bike_Seat", root, (0, -0.38, 0.80), (0.34, 0.68, 0.12), mats["steel_black"], bevel=0.03, col=col)
    add_box("Bike_FrontShield", root, (0, 0.48, 0.68), (0.38, 0.24, 0.58), mats["plastic_red"], rot=(-0.22, 0, 0), bevel=0.03, col=col)
    add_box("Bike_Handlebar", root, (0, 0.42, 1.04), (0.68, 0.10, 0.08), mats["steel_black"], bevel=0.015, col=col)
    add_cyl("Bike_Headlight", root, (0, 0.54, 0.94), 0.085, 0.08, mats["warm_glow"], rot=(math.pi / 2, 0, 0), verts=14, col=col)
    add_box("Bike_Taillight", root, (0, -0.82, 0.66), (0.16, 0.05, 0.10), mats["plastic_red"], bevel=0.008, col=col)
    for wy in (-0.58, 0.62):
        add_cyl(f"BikeWheel_{wy}", root, (0, wy, 0.26), 0.26, 0.11, mats["steel_black"], rot=(0, math.pi / 2, 0), verts=16, col=col)
        add_cyl(f"BikeHub_{wy}", root, (0, wy, 0.26), 0.14, 0.12, mats["inox_steel"], rot=(0, math.pi / 2, 0), verts=12, col=col)
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

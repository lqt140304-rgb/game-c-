import math
import random
import bpy
import bmesh
from mathutils import Euler, Matrix, Vector


def _ensure_link(obj, parent, col):
    if parent:
        obj.parent = parent
    target_col = col or bpy.context.scene.collection
    for c in list(obj.users_collection):
        c.objects.unlink(obj)
    target_col.objects.link(obj)


def _assign_box_uv_bm(bm, uv_scale=0.5):
    uv_layer = bm.loops.layers.uv.verify()
    for face in bm.faces:
        n = face.normal
        ax, ay, az = abs(n.x), abs(n.y), abs(n.z)
        for loop in face.loops:
            co = loop.vert.co
            if az >= ax and az >= ay:
                loop[uv_layer].uv = (co.x * uv_scale, co.y * uv_scale)
            elif ax >= ay:
                loop[uv_layer].uv = (co.y * uv_scale, co.z * uv_scale)
            else:
                loop[uv_layer].uv = (co.x * uv_scale, co.z * uv_scale)


def _finalize_bm_object(name, parent, bm, location, rot, scale, material, smooth=False, bevel=0.0, col=None):
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    _assign_box_uv_bm(bm)
    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm.to_mesh(mesh)
    bm.free()

    obj = bpy.data.objects.new(name, mesh)
    obj.location = Vector(location)
    obj.rotation_euler = Euler(rot)
    obj.scale = Vector(scale)
    if material:
        obj.data.materials.append(material)
    _ensure_link(obj, parent, col)

    # Stylized Low Poly: force flat shading unless explicitly requested
    for poly in obj.data.polygons:
        poly.use_smooth = bool(smooth)
    return obj


# ==================== 1. LATHE / SPIN TURNED PROFILES ====================

def add_lathe_mesh(name, parent, profile_rz, location, material, segments=18, scale=(1, 1, 1), rot=(0, 0, 0), smooth=True, col=None):
    """
    Revolves a 2D (radius, z) profile around local +Z using bmesh.ops.spin.
    Creates true turned balusters, ceramic urns, scale pans, lamp posts, and counter columns.
    """
    bm = bmesh.new()
    verts = []
    for r, z in profile_rz:
        verts.append(bm.verts.new((max(0.0, float(r)), 0.0, float(z))))

    edges = []
    for i in range(len(verts) - 1):
        edges.append(bm.edges.new((verts[i], verts[i + 1])))

    geom = verts + edges
    bmesh.ops.spin(
        bm,
        geom=geom,
        cent=(0.0, 0.0, 0.0),
        axis=(0.0, 0.0, 1.0),
        dvec=(0.0, 0.0, 0.0),
        angle=math.pi * 2.0,
        steps=segments,
        use_merge=True,
    )
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.0005)
    # Close open bottom/top holes if start/end radius > 0.002
    open_edges = [e for e in bm.edges if e.is_boundary]
    if open_edges:
        try:
            bmesh.ops.holes_fill(bm, edges=open_edges, sides=0)
        except Exception:
            pass
    return _finalize_bm_object(name, parent, bm, location, rot, scale, material, smooth=smooth, col=col)


def add_turned_baluster(name, parent, height, location, material, radius=0.048, segments=14, col=None):
    """Indochine turned wooden mezzanine baluster ('con tiện gỗ') profile."""
    s = height
    profile = [
        (0.0, 0.0),
        (radius * 1.15, 0.0),
        (radius * 1.15, 0.08 * s),
        (radius * 0.65, 0.14 * s),
        (radius * 0.55, 0.22 * s),
        (radius * 1.05, 0.36 * s),
        (radius * 1.10, 0.48 * s),
        (radius * 0.68, 0.64 * s),
        (radius * 0.50, 0.74 * s),
        (radius * 0.85, 0.82 * s),
        (radius * 0.58, 0.88 * s),
        (radius * 1.12, 0.94 * s),
        (radius * 1.12, 1.00 * s),
        (0.0, 1.00 * s),
    ]
    return add_lathe_mesh(name, parent, profile, location, material, segments=segments, col=col)


def add_classical_pedestal(name, parent, location, material, plaque_mat, trim_mat, is_facing_neg_y=True, col=None):
    """
    Sculpted classical Khu A display pedestal (not a stack of 3 cubes):
    - Multi-stepped chamfered base plinth + octagonal torus necking
    - Tapered shaft with recessed side dado panels + brass corner strips
    - Multi-stepped flared capital top (Z = 1.06m) + angled brass/parchment plaque
    """
    grp = bpy.data.objects.new(name, None)
    grp.location = Vector(location)
    _ensure_link(grp, parent, col)

    # Multi-tier classical lathe/octagonal plinth & capital combined cleanly with beveled dado core
    base_profile = [
        (0.0, 0.24),
        (0.56, 0.24),
        (0.56, 0.31),
        (0.50, 0.35),
        (0.45, 0.40),
        (0.40, 0.43),
        (0.0, 0.43),
    ]
    add_lathe_mesh(f"{name}_BasePlinth", grp, base_profile, (0, 0, 0), material, segments=8, rot=(0, 0, math.pi / 8), col=col)

    cap_profile = [
        (0.0, 0.92),
        (0.39, 0.92),
        (0.44, 0.96),
        (0.50, 1.00),
        (0.54, 1.04),
        (0.54, 1.06),
        (0.0, 1.06),
    ]
    add_lathe_mesh(f"{name}_CapPlinth", grp, cap_profile, (0, 0, 0), material, segments=8, rot=(0, 0, math.pi / 8), col=col)

    # Octagonal/Chamfered pedestal column + recessed brass frame inset
    col_profile = [
        (0.41, 0.42),
        (0.38, 0.67),
        (0.39, 0.93),
    ]
    add_lathe_mesh(f"{name}_Shaft", grp, col_profile, (0, 0, 0), material, segments=8, rot=(0, 0, math.pi / 8), smooth=False, col=col)

    # Brass collar ring beneath capital
    collar = [
        (0.415, 0.91),
        (0.435, 0.93),
        (0.415, 0.95),
    ]
    add_lathe_mesh(f"{name}_BrassCollar", grp, collar, (0, 0, 0), trim_mat, segments=16, col=col)

    # Angled cream-parchment price tag plate with brass bezel facing central aisle
    dy = 0.40 if not is_facing_neg_y else -0.40
    tilt = 0.28 if not is_facing_neg_y else -0.28
    bm_tag = bmesh.new()
    bmesh.ops.create_cube(bm_tag, size=1.0)
    bmesh.ops.scale(bm_tag, verts=bm_tag.verts, vec=Vector((0.36, 0.03, 0.16)))
    _finalize_bm_object(f"{name}_Tag", grp, bm_tag, (0, dy, 0.78), (tilt, 0, 0), (1, 1, 1), plaque_mat, bevel=0.005, col=col)
    return grp


# ==================== 2. 3D BEZIER CURVES & WROUGHT IRON ====================

def add_bezier_scroll_mesh(name, parent, points_3d, bevel_radius, location, material, resolution=8, closed=False, col=None):
    """Builds a smooth 3D Bezier tube mesh (for wrought iron scrolls, catenary cords, chandelier arms)."""
    curve = bpy.data.curves.new(name + "_Curve", type="CURVE")
    curve.dimensions = "3D"
    curve.bevel_depth = bevel_radius
    curve.bevel_resolution = 3
    curve.resolution_u = resolution
    curve.use_fill_caps = True

    spline = curve.splines.new("BEZIER")
    spline.bezier_points.add(len(points_3d) - 1)
    spline.use_cyclic_u = closed

    for idx, pt in enumerate(points_3d):
        bp = spline.bezier_points[idx]
        bp.co = Vector(pt)
        bp.handle_left_type = "AUTO"
        bp.handle_right_type = "AUTO"

    obj = bpy.data.objects.new(name, curve)
    obj.location = Vector(location)
    _ensure_link(obj, parent, col)

    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.convert(target="MESH")
    mesh_obj = bpy.context.view_layer.objects.active
    if material:
        mesh_obj.data.materials.clear()
        mesh_obj.data.materials.append(material)

    bm = bmesh.new()
    bm.from_mesh(mesh_obj.data)
    _assign_box_uv_bm(bm, uv_scale=1.0)
    bm.to_mesh(mesh_obj.data)
    bm.free()
    for p in mesh_obj.data.polygons:
        p.use_smooth = True
    return mesh_obj


def add_wrought_iron_balcony_bay(name, parent, width_y, height_z, location, iron_mat, brass_mat, gear_fn, col=None):
    """Sculpted wrought-iron balcony railing bay with forged S-scroll curves & central brass clockwork gear."""
    grp = bpy.data.objects.new(name, None)
    grp.location = Vector(location)
    _ensure_link(grp, parent, col)

    hy = width_y * 0.5
    # Moulded top handrail + bottom rail
    add_bezier_scroll_mesh(f"{name}_Top", grp, [(0, -hy, height_z), (0, 0, height_z), (0, hy, height_z)], 0.028, (0, 0, 0), iron_mat, resolution=4, col=col)
    add_bezier_scroll_mesh(f"{name}_Bot", grp, [(0, -hy, 0.08), (0, 0, 0.08), (0, hy, 0.08)], 0.020, (0, 0, 0), iron_mat, resolution=4, col=col)

    # Flanking S-scroll wrought-iron curves hugging central gear medallion
    for sgn in (-1.0, 1.0):
        pts_s1 = [
            (0.0, sgn * 0.42, 0.14),
            (0.0, sgn * (width_y * 0.24), height_z * 0.34),
            (0.0, sgn * (width_y * 0.36), height_z * 0.72),
            (0.0, sgn * 0.46, height_z - 0.12),
        ]
        add_bezier_scroll_mesh(f"{name}_ScrollA_{sgn}", grp, pts_s1, 0.015, (0, 0, 0), iron_mat, resolution=8, col=col)

        pts_c = [
            (0.0, sgn * (hy - 0.12), 0.12),
            (0.0, sgn * (hy - 0.30), height_z * 0.50),
            (0.0, sgn * (hy - 0.12), height_z - 0.10),
        ]
        add_bezier_scroll_mesh(f"{name}_ScrollB_{sgn}", grp, pts_c, 0.016, (0, 0, 0), iron_mat, resolution=8, col=col)

    # Central brass gear medallion
    gear_fn(f"{name}_Gear", grp, (0.0, 0.0, height_z * 0.52), 0.34, 0.045, 10, brass_mat, rot=(0, math.pi / 2, 0), col=col)
    return grp


# ==================== 3. TRUE RADIAL STONE/BRICK ARCHWAYS ====================

def add_radial_stone_arch(name, parent, clear_w, clear_h, arch_thk, depth_x, pier_w, location, rot_z, wall_mat, voussoir_mat, num_voussoirs=11, col=None):
    """
    True radial masonry arch with individual chamfered voussoir stones,
    raised center keystone ('viên khóa vòm'), moulded impost capitals, and spandrel infill.
    Built along local Y (clear_w along Y, depth along X, Z up).
    """
    grp = bpy.data.objects.new(name, None)
    grp.location = Vector(location)
    grp.rotation_euler = Euler((0, 0, rot_z))
    _ensure_link(grp, parent, col)

    r_in = clear_w * 0.5
    r_out = r_in + arch_thk
    spring_z = clear_h - r_in

    # Left & Right masonry piers + impost capitals
    for sgn, pname in ((-1.0, "PierL"), (1.0, "PierR")):
        py = sgn * (r_in + pier_w * 0.5)
        bm_p = bmesh.new()
        bmesh.ops.create_cube(bm_p, size=1.0)
        bmesh.ops.scale(bm_p, verts=bm_p.verts, vec=Vector((depth_x, pier_w, spring_z)))
        _finalize_bm_object(f"{name}_{pname}", grp, bm_p, (0, py, spring_z * 0.5), (0, 0, 0), (1, 1, 1), wall_mat, smooth=False, bevel=0.012, col=col)

        # Stepped impost capital at arch springing line
        bm_imp = bmesh.new()
        bmesh.ops.create_cube(bm_imp, size=1.0)
        bmesh.ops.scale(bm_imp, verts=bm_imp.verts, vec=Vector((depth_x + 0.08, pier_w + 0.10, 0.14)))
        _finalize_bm_object(f"{name}_{pname}_Cap", grp, bm_imp, (0, py, spring_z), (0, 0, 0), (1, 1, 1), voussoir_mat, smooth=False, bevel=0.012, col=col)

    # Radial Voussoirs + Projecting Center Keystone + Spandrel corner infill
    bm_arch = bmesh.new()
    mid_idx = num_voussoirs // 2
    for i in range(num_voussoirs):
        a0 = math.pi * (i / float(num_voussoirs)) + 0.008
        a1 = math.pi * ((i + 1) / float(num_voussoirs)) - 0.008
        is_key = (i == mid_idx)
        ro = r_out + (0.09 if is_key else (0.02 if i % 2 == 0 else 0.0))
        dx = (depth_x + 0.08) if is_key else (depth_x + 0.03)
        hx_v = dx * 0.5

        pts = [
            (-hx_v, math.cos(a0) * r_in, spring_z + math.sin(a0) * r_in),
            (-hx_v, math.cos(a0) * ro, spring_z + math.sin(a0) * ro),
            (-hx_v, math.cos(a1) * ro, spring_z + math.sin(a1) * ro),
            (-hx_v, math.cos(a1) * r_in, spring_z + math.sin(a1) * r_in),
            (hx_v, math.cos(a0) * r_in, spring_z + math.sin(a0) * r_in),
            (hx_v, math.cos(a0) * ro, spring_z + math.sin(a0) * ro),
            (hx_v, math.cos(a1) * ro, spring_z + math.sin(a1) * ro),
            (hx_v, math.cos(a1) * r_in, spring_z + math.sin(a1) * r_in),
        ]
        vs = [bm_arch.verts.new(p) for p in pts]
        bm_arch.faces.new((vs[0], vs[1], vs[2], vs[3]))
        bm_arch.faces.new((vs[7], vs[6], vs[5], vs[4]))
        for q in ((0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)):
            bm_arch.faces.new((vs[q[0]], vs[q[1]], vs[q[2]], vs[q[3]]))

    _finalize_bm_object(f"{name}_Voussoirs", grp, bm_arch, (0, 0, 0), (0, 0, 0), (1, 1, 1), voussoir_mat, smooth=False, bevel=0.008, col=col)

    # Solid spandrel masonry corners filling around the circular extrados up to horizontal header height
    top_z = spring_z + r_out + 0.25
    bm_sp = bmesh.new()
    steps = 10
    for sgn in (-1.0, 1.0):
        outer_y = sgn * (r_in + pier_w)
        hx_s = depth_x * 0.5
        for k in range(steps):
            t0 = k / float(steps)
            t1 = (k + 1) / float(steps)
            ang0 = (0.0 if sgn > 0 else math.pi) + (-1.0 if sgn < 0 else 1.0) * (t0 * math.pi * 0.5)
            ang1 = (0.0 if sgn > 0 else math.pi) + (-1.0 if sgn < 0 else 1.0) * (t1 * math.pi * 0.5)
            y0, z0 = math.cos(ang0) * r_out, spring_z + math.sin(ang0) * r_out
            y1, z1 = math.cos(ang1) * r_out, spring_z + math.sin(ang1) * r_out
            p = [
                (-hx_s, y0, z0), (-hx_s, outer_y, z0), (-hx_s, outer_y, z1), (-hx_s, y1, z1),
                (hx_s, y0, z0), (hx_s, outer_y, z0), (hx_s, outer_y, z1), (hx_s, y1, z1),
            ]
            vs = [bm_sp.verts.new(v) for v in p]
            bm_sp.faces.new((vs[0], vs[1], vs[2], vs[3]))
            bm_sp.faces.new((vs[7], vs[6], vs[5], vs[4]))
            for q in ((0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)):
                bm_sp.faces.new((vs[q[0]], vs[q[1]], vs[q[2]], vs[q[3]]))
    # Horizontal top header strip across full arch
    bm_hdr = bmesh.new()
    bmesh.ops.create_cube(bm_hdr, size=1.0)
    bmesh.ops.scale(bm_hdr, verts=bm_hdr.verts, vec=Vector((depth_x, clear_w + pier_w * 2.0, 0.25)))
    _finalize_bm_object(f"{name}_Spandrel", grp, bm_sp, (0, 0, 0), (0, 0, 0), (1, 1, 1), wall_mat, smooth=False, col=col)
    _finalize_bm_object(f"{name}_Header", grp, bm_hdr, (0, 0, top_z - 0.125), (0, 0, 0), (1, 1, 1), wall_mat, smooth=False, bevel=0.008, col=col)
    return grp


# ==================== 4. YIN-YANG TILED ROOF & EXPOSED TIMBER TRUSSES ====================

def add_yinyang_tiled_roof(name, parent, half_span_x, width_y, z_eave, z_ridge, tile_mat, ridge_mat, wood_mat, sag=0.14, col=None):
    """
    Traditional Hanoi/Indochine curved roof ('mái ngói âm dương'):
    - Parabolic concave slope curvature ('sag')
    - True rows of cylindrical barrel roof tiles ('ngói ống') + end drip caps ('ngói câu đầu')
    - Carved ridge beam ('bờ nóc') with curved upturned horn terminals ('đầu đao')
    - Exposed interior dark-timber trusses ('vì kèo chồng rường') visible from below
    """
    grp = bpy.data.objects.new(name, None)
    _ensure_link(grp, parent, col)

    def _roof_z(abs_x):
        t = min(1.0, abs(abs_x) / max(half_span_x, 0.01))
        # Subtle concave parabolic curve typical of traditional northern Vietnamese roofs
        linear_z = z_ridge + (z_eave - z_ridge) * t
        return linear_z - sag * (1.0 - (2.0 * t - 1.0) ** 2)

    # 1. Dark-timber curved roof plank decking visible looking up inside hall
    bm_deck = bmesh.new()
    hy = width_y * 0.5
    seg_x = 10
    for sgn in (-1.0, 1.0):
        rows = []
        for ix in range(seg_x + 1):
            x = sgn * (half_span_x * (ix / float(seg_x)))
            z = _roof_z(x)
            rows.append((
                bm_deck.verts.new((x, -hy, z - 0.06)),
                bm_deck.verts.new((x, hy, z - 0.06)),
                bm_deck.verts.new((x, -hy, z + 0.04)),
                bm_deck.verts.new((x, hy, z + 0.04)),
            ))
        for ix in range(seg_x):
            a = rows[ix]
            b = rows[ix + 1]
            # Underside, topside, side walls
            bm_deck.faces.new((a[0], a[1], b[1], b[0]))
            bm_deck.faces.new((a[3], a[2], b[2], b[3]))
            bm_deck.faces.new((a[0], b[0], b[2], a[2]))
            bm_deck.faces.new((a[1], a[3], b[3], b[1]))
    _finalize_bm_object(f"{name}_TimberDeck", grp, bm_deck, (0, 0, 0), (0, 0, 0), (1, 1, 1), wood_mat, smooth=False, col=col)

    # 2. Rows of half-cylindrical Yin-Yang barrel tiles along front/back slopes
    bm_tiles = bmesh.new()
    tile_pitch_y = 0.52
    num_rows = int(width_y / tile_pitch_y)
    start_y = -hy + tile_pitch_y * 0.5
    tile_r = 0.088
    prof_angles = [0.0, math.pi * 0.33, math.pi * 0.67, math.pi]

    for ir in range(num_rows):
        cy = start_y + ir * tile_pitch_y
        for sgn in (-1.0, 1.0):
            # Continuous overlapping cover tile ribs along curved roof slope
            ring_verts = []
            for ix in range(9):
                t = ix / 8.0
                cx = sgn * (0.18 + (half_span_x - 0.12) * t)
                cz = _roof_z(cx) + 0.05
                arc = []
                for ang in prof_angles:
                    vy = cy + math.cos(ang) * tile_r
                    vz = cz + math.sin(ang) * tile_r
                    arc.append(bm_tiles.verts.new((cx, vy, vz)))
                ring_verts.append(arc)
            for ix in range(8):
                for ia in range(len(prof_angles) - 1):
                    v0 = ring_verts[ix][ia]
                    v1 = ring_verts[ix][ia + 1]
                    v2 = ring_verts[ix + 1][ia + 1]
                    v3 = ring_verts[ix + 1][ia]
                    bm_tiles.faces.new((v0, v1, v2, v3) if sgn > 0 else (v3, v2, v1, v0))
            # Drip-edge circular cap disc ('ngói câu đầu') at the outer eave edge
            eave_x = sgn * (half_span_x + 0.06)
            eave_z = _roof_z(eave_x) + 0.05
            cap_v = [bm_tiles.verts.new((eave_x, cy + math.cos(a) * tile_r * 1.08, eave_z + math.sin(a) * tile_r * 1.08)) for a in (0, 1.2, 2.5, 3.8, 5.1)]
            bm_tiles.faces.new(cap_v if sgn > 0 else list(reversed(cap_v)))

    _finalize_bm_object(f"{name}_BarrelTiles", grp, bm_tiles, (0, 0, 0), (0, 0, 0), (1, 1, 1), tile_mat, smooth=True, col=col)

    # 3. Stepped ornamental ridge beam ('bờ nóc') + curved upturned horn terminals ('đầu đao')
    add_profiled_moulding(
        f"{name}_RidgeBeam", grp,
        [(0, -hy - 0.15, z_ridge + 0.12), (0, hy + 0.15, z_ridge + 0.12)],
        [(-0.22, -0.06), (0.22, -0.06), (0.18, 0.12), (0.11, 0.18), (0.11, 0.28), (-0.11, 0.28), (-0.11, 0.18), (-0.18, 0.12)],
        (0, 0, 0), 0.0, ridge_mat, col=col
    )
    for hy_sgn in (-1.0, 1.0):
        horn_pts = [
            (0.0, hy_sgn * (hy - 0.10), z_ridge + 0.22),
            (0.0, hy_sgn * (hy + 0.48), z_ridge + 0.36),
            (0.0, hy_sgn * (hy + 0.88), z_ridge + 0.72),
        ]
        add_bezier_scroll_mesh(f"{name}_Horn_{hy_sgn}", grp, horn_pts, 0.075, (0, 0, 0), ridge_mat, resolution=8, col=col)

    return grp


def add_profiled_moulding(name, parent, path_pts, profile_yz, location, rot_z, material, closed=False, col=None):
    """Extrudes a 2D cross-section profile cleanly along a 3D path (for cornices, ridge beams, plinths)."""
    bm = bmesh.new()
    rings = []
    n_pts = len(path_pts)
    for idx, p in enumerate(path_pts):
        p_vec = Vector(p)
        if idx < n_pts - 1:
            tan = (Vector(path_pts[idx + 1]) - p_vec).normalized()
        else:
            tan = (p_vec - Vector(path_pts[idx - 1])).normalized()
        if idx > 0 and idx < n_pts - 1:
            t_prev = (p_vec - Vector(path_pts[idx - 1])).normalized()
            tan = (t_prev + tan).normalized()

        # Right vector horizontal perpendicular to tangent in XY
        right = tan.cross(Vector((0.0, 0.0, 1.0)))
        if right.length < 1e-4:
            right = Vector((1.0, 0.0, 0.0))
        else:
            right.normalize()
        up = Vector((0.0, 0.0, 1.0))

        ring = []
        for u, v in profile_yz:
            ring.append(bm.verts.new(p_vec + right * u + up * v))
        rings.append(ring)

    seg_count = n_pts if closed else (n_pts - 1)
    prof_n = len(profile_yz)
    for r_i in range(seg_count):
        ra = rings[r_i]
        rb = rings[(r_i + 1) % n_pts]
        for j in range(prof_n):
            jn = (j + 1) % prof_n
            bm.faces.new((ra[j], ra[jn], rb[jn], rb[j]))

    if not closed and len(rings) >= 2:
        bm.faces.new(list(reversed(rings[0])))
        bm.faces.new(rings[-1])

    return _finalize_bm_object(name, parent, bm, location, (0, 0, rot_z), (1, 1, 1), material, smooth=False, col=col)


# ==================== 5. STAVE BARRELS & 3D PEELING BRICK RELIEF ====================

def add_bulged_stave_barrel(name, parent, radius, height, location, wood_mat, iron_mat, col=None):
    """Curved wooden stave barrel with lathe-bulged belly and iron tension hoops."""
    grp = bpy.data.objects.new(name, None)
    grp.location = Vector(location)
    _ensure_link(grp, parent, col)

    profile = [
        (0.0, 0.0),
        (radius * 0.80, 0.0),
        (radius * 0.88, height * 0.16),
        (radius * 0.98, height * 0.40),
        (radius * 1.00, height * 0.50),
        (radius * 0.98, height * 0.60),
        (radius * 0.88, height * 0.84),
        (radius * 0.80, height),
        (radius * 0.72, height),
        (radius * 0.72, height * 0.94),
        (0.0, height * 0.94),
    ]
    add_lathe_mesh(f"{name}_WoodBody", grp, profile, (0, 0, 0), wood_mat, segments=16, smooth=False, col=col)

    for hz, hr in ((height * 0.12, radius * 0.87), (height * 0.34, radius * 0.97), (height * 0.66, radius * 0.97), (height * 0.88, radius * 0.87)):
        hoop = [(hr - 0.005, hz - 0.022), (hr + 0.012, hz - 0.022), (hr + 0.012, hz + 0.022), (hr - 0.005, hz + 0.022)]
        add_lathe_mesh(f"{name}_Hoop_{hz:.2f}", grp, hoop, (0, 0, 0), iron_mat, segments=16, smooth=False, col=col)
    return grp


def add_staggered_brick_relief(name, parent, location, width, height, normal_axis, brick_mat, plaster_mat, seed=42, col=None):
    """
    Sculpted 3D peeling plaster frame with individual staggered projecting red bricks
    matching concept_1's weathered old-brick patches.
    normal_axis: '+x', '-y', or '+y'
    """
    rng = random.Random(seed)
    grp = bpy.data.objects.new(name, None)
    grp.location = Vector(location)
    if normal_axis == "-y":
        grp.rotation_euler = Euler((0, 0, -math.pi / 2))
    elif normal_axis == "+y":
        grp.rotation_euler = Euler((0, 0, math.pi / 2))
    _ensure_link(grp, parent, col)

    bm_bricks = bmesh.new()
    bm_mortar = bmesh.new()
    brick_w = 0.24
    brick_h = 0.095
    rows = max(4, int(height / brick_h))
    cols = max(3, int(width / brick_w))

    min_y, max_y = width * 0.5, -width * 0.5
    min_z, max_z = height * 0.5, -height * 0.5
    for r in range(rows):
        row_off = (brick_w * 0.5) if (r % 2 == 1) else 0.0
        for c in range(cols):
            # Organic jagged silhouette mask so patch does not look rectangular
            ny = (c + 0.5) / cols * 2.0 - 1.0
            nz = (r + 0.5) / rows * 2.0 - 1.0
            if (ny * ny * 0.9 + nz * nz * 0.8) > (0.85 + rng.uniform(-0.22, 0.28)):
                continue
            by = -width * 0.5 + c * brick_w + row_off
            bz = -height * 0.5 + r * brick_h
            min_y, max_y = min(min_y, by - brick_w * 0.55), max(max_y, by + brick_w * 0.55)
            min_z, max_z = min(min_z, bz - brick_h * 0.55), max(max_z, bz + brick_h * 0.55)
            proj = rng.uniform(0.006, 0.012)
            mat_loc = Matrix.Translation((0.003 + proj * 0.5, by, bz)) @ Matrix.Diagonal((proj, brick_w * 0.90, brick_h * 0.86, 1.0))
            bmesh.ops.create_cube(bm_bricks, size=1.0, matrix=mat_loc)
            mat_m = Matrix.Translation((0.002, by, bz)) @ Matrix.Diagonal((0.004, brick_w * 1.02, brick_h * 1.02, 1.0))
            bmesh.ops.create_cube(bm_mortar, size=1.0, matrix=mat_m)

    _finalize_bm_object(f"{name}_MortarBed", grp, bm_mortar, (0, 0, 0), (0, 0, 0), (1, 1, 1), plaster_mat, smooth=False, bevel=0.0, col=col)
    _finalize_bm_object(f"{name}_Bricks", grp, bm_bricks, (0, 0, 0), (0, 0, 0), (1, 1, 1), brick_mat, smooth=False, bevel=0.003, col=col)
    return grp


# ==================== 6. ORGANIC CURVED HANOI SHADE TREES ====================

def add_organic_hanoi_tree(name, loc, mats, rng, col, parent=None, scale=1.0):
    """
    Stylized Low Poly Hanoi shade tree (Flat Shading, bright sunny colors):
    - Hexagonal stone planter curb + soil
    - 6-sided segmented angular trunk & limbs (no smooth shading)
    - Crisp faceted icosphere (subdivisions=1) canopy clusters
    - Angular low-poly rocks & grass tufts around the planter base
    """
    root = bpy.data.objects.new(name, None)
    root.location = Vector(loc)
    root.scale = Vector((scale, scale, scale))
    _ensure_link(root, parent, col)

    # Hexagonal stone planter curb around root
    planter_prof = [
        (0.55, 0.0),
        (0.84, 0.0),
        (0.84, 0.30),
        (0.88, 0.34),
        (0.70, 0.34),
        (0.68, 0.22),
        (0.0, 0.22),
    ]
    add_lathe_mesh("TreePlanter_Stone", root, planter_prof, (0, 0, 0), mats["ashlar_stone"], segments=6, smooth=False, col=col)
    soil_prof = [(0.0, 0.23), (0.68, 0.23), (0.58, 0.27), (0.0, 0.27)]
    add_lathe_mesh("TreePlanter_Soil", root, soil_prof, (0, 0, 0), mats["wood_dark"], segments=6, smooth=False, col=col)

    def _add_faceted_limb(limb_name, p0, p1, r0, r1, mat):
        v0, v1 = Vector(p0), Vector(p1)
        vec = v1 - v0
        length = vec.length
        if length < 1e-4:
            return
        bm_l = bmesh.new()
        bmesh.ops.create_cone(bm_l, cap_ends=True, cap_tris=False, segments=6, radius1=r0, radius2=r1, depth=length)
        mid = (v0 + v1) * 0.5
        rot_q = Vector((0, 0, 1)).rotation_difference(vec.normalized())
        _finalize_bm_object(limb_name, root, bm_l, mid, rot_q.to_euler(), (1, 1, 1), mat, smooth=False, col=col)

    # Segmented 6-sided faceted main trunk (like Stylized Low Poly reference)
    trunk_segs = [
        ((0.0, 0.0, 0.22), (0.04, -0.02, 1.05), 0.20, 0.16),
        ((0.04, -0.02, 1.02), (-0.03, 0.04, 1.92), 0.16, 0.135),
        ((-0.03, 0.04, 1.88), (0.06, 0.02, 2.75), 0.135, 0.11),
    ]
    for si, (p0, p1, r0, r1) in enumerate(trunk_segs):
        _add_faceted_limb(f"Trunk_Seg_{si}", p0, p1, r0, r1, mats["tree_bark"])

    # Clean whitewashed lower collar (6-sided flat-shaded)
    lime_prof = [(0.215, 0.24), (0.185, 0.95), (0.175, 1.05)]
    add_lathe_mesh("Trunk_LimeBand", root, lime_prof, (0, 0, 0), mats["dial_white"], segments=6, smooth=False, col=col)

    # Faceted angular root buttresses
    for r_i in range(4):
        ang = r_i * (math.pi * 0.5) + 0.35
        _add_faceted_limb(
            f"RootFlare_{r_i}",
            (0.05 * math.cos(ang), 0.05 * math.sin(ang), 0.62),
            (0.42 * math.cos(ang), 0.42 * math.sin(ang), 0.22),
            0.075,
            0.045,
            mats["tree_bark"],
        )

    # Segmented 6-sided faceted branches
    branches = [
        [(0.04, 0.02, 2.55), (-0.42, -0.28, 3.25), (-0.95, -0.62, 3.85)],
        [(0.06, 0.02, 2.60), (0.48, 0.25, 3.30), (1.02, 0.58, 3.92)],
        [(0.03, 0.04, 2.65), (0.05, -0.52, 3.38), (0.18, -1.05, 4.02)],
        [(0.05, 0.02, 2.70), (-0.28, 0.48, 3.45), (-0.52, 0.96, 4.18)],
    ]
    for bi, bpts in enumerate(branches):
        _add_faceted_limb(f"Branch_{bi}_A", bpts[0], bpts[1], 0.085, 0.065, mats["tree_bark"])
        _add_faceted_limb(f"Branch_{bi}_B", bpts[1], bpts[2], 0.065, 0.042, mats["tree_bark"])

    # Crisp faceted Low Poly canopy clusters (subdivisions=1, flat shaded)
    canopy_clusters = [
        ("TreeCanopy_Main", (0.04, 0.02, 4.35), (1.62, 1.52, 1.08), (0.1, 0.2, 0.4)),
        ("TreeCanopy_W", (-1.05, -0.62, 3.92), (1.16, 1.06, 0.88), (0.3, -0.2, 1.1)),
        ("TreeCanopy_E", (1.08, 0.58, 4.00), (1.22, 1.10, 0.88), (-0.2, 0.3, 2.2)),
        ("TreeCanopy_S", (0.20, -1.05, 4.10), (1.06, 1.00, 0.85), (0.2, 0.1, 0.8)),
        ("TreeCanopy_N", (-0.48, 0.96, 4.25), (1.12, 1.06, 0.86), (-0.3, -0.1, 1.7)),
        ("TreeCanopy_Crown", (0.0, 0.12, 5.15), (1.02, 0.98, 0.82), (0.15, -0.25, 2.8)),
    ]
    for cname, cloc, cscale, crot in canopy_clusters:
        bm_c = bmesh.new()
        bmesh.ops.create_icosphere(bm_c, subdivisions=1, radius=1.0)
        for v in bm_c.verts:
            n_val = 1.0 + 0.12 * math.sin(v.co.x * 3.7 + cloc[0]) * math.cos(v.co.y * 4.1)
            v.co.x *= cscale[0] * n_val
            v.co.y *= cscale[1] * n_val
            v.co.z *= cscale[2] * n_val
        _finalize_bm_object(cname, root, bm_c, cloc, crot, (1, 1, 1), mats["tree_foliage"], smooth=False, col=col)

    # Angular Low-Poly rocks & grass tufts around planter base (matching reference)
    rock_specs = [
        ("TreeRock_0", (0.42, 0.28, 0.27), (0.16, 0.13, 0.10), (0.2, 0.4, 0.8)),
        ("TreeRock_1", (-0.38, 0.34, 0.26), (0.14, 0.11, 0.09), (0.5, -0.3, 1.6)),
        ("TreeRock_2", (-0.32, -0.40, 0.26), (0.13, 0.12, 0.08), (-0.3, 0.2, 2.4)),
    ]
    for rname, rloc, rscale, rrot in rock_specs:
        bm_r = bmesh.new()
        bmesh.ops.create_icosphere(bm_r, subdivisions=1, radius=1.0)
        for v in bm_r.verts:
            v.co.x *= rscale[0]
            v.co.y *= rscale[1]
            v.co.z *= rscale[2] * (0.75 if v.co.z < 0 else 1.0)
        _finalize_bm_object(rname, root, bm_r, rloc, rrot, (1, 1, 1), mats["ashlar_stone"], smooth=False, col=col)

    grass_specs = [
        (0.32, -0.36, 0.27, 0.4),
        (-0.44, -0.12, 0.27, 1.2),
        (0.18, 0.44, 0.27, 2.1),
        (-0.14, 0.46, 0.27, 2.9),
    ]
    for gi, (gx, gy, gz, grot) in enumerate(grass_specs):
        bm_g = bmesh.new()
        for bi in range(3):
            ang = bi * (2.0 * math.pi / 3.0) + grot
            bx, by = 0.03 * math.cos(ang), 0.03 * math.sin(ang)
            v0 = bm_g.verts.new((bx - 0.02 * math.sin(ang), by + 0.02 * math.cos(ang), 0.0))
            v1 = bm_g.verts.new((bx + 0.02 * math.sin(ang), by - 0.02 * math.cos(ang), 0.0))
            v2 = bm_g.verts.new((bx + 0.05 * math.cos(ang), by + 0.05 * math.sin(ang), 0.16 + 0.03 * (bi % 2)))
            bm_g.faces.new((v0, v1, v2))
        _finalize_bm_object(f"TreeGrass_{gi}", root, bm_g, (gx, gy, gz), (0, 0, grot), (1, 1, 1), mats["tree_foliage"], smooth=False, col=col)

    return root

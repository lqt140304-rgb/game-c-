"""Sua cac mo hinh LowPoly tao bang Meshy theo ket qua Docs/Art/KiemTra3D.md.

Chay: python3 Tools/Blender/fix_lowpoly_meshy.py <file.glb> [<file.glb> ...]
Ghi de file. Moi file: han dinh trung, va lo nho, tinh lai phap tuyen huong ra ngoai,
giam tam giac neu > MAX_TRIS, ha texture > MAX_TEX, xoay theo ROTATE neu co.
"""
import bpy, bmesh, sys, math, os

MAX_TRIS = 10000
TARGET_TRIS = 9000
MAX_TEX = 1024
# Xoay de truc dai dung thang (toa do Blender, Z len). Goc tinh bang find_axis().
ROTATE_UPRIGHT = {"L21_than_kiem_kusanagi"}
FLIP_AFTER_UPRIGHT = {"L21_than_kiem_kusanagi"}  # luoi kiem huong len


def tris(o):
    return sum(len(p.vertices) - 2 for p in o.data.polygons)


def upright(o):
    """Xoay de truc dai nhat cua mesh trung truc Z (doc theo PCA)."""
    import numpy as np
    from mathutils import Matrix, Vector
    pts = np.array([v.co[:] for v in o.data.vertices])
    pts = pts - pts.mean(0)
    w, v = np.linalg.eigh(np.cov(pts.T))
    axis = Vector(v[:, np.argmax(w)])
    if axis.z < 0:
        axis = -axis
    rot = axis.rotation_difference(Vector((0, 0, 1))).to_matrix().to_4x4()
    o.data.transform(rot)


def fix(path):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=path)
    name = os.path.splitext(os.path.basename(path))[0]
    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    log = {}
    for o in meshes:
        # dua transform vao du lieu de giu dung kich thuoc khi xuat
        o.data.transform(o.matrix_world); o.matrix_world.identity()
        before = tris(o)
        me = o.data
        if hasattr(me, "has_custom_normals") and me.has_custom_normals:
            bpy.context.view_layer.objects.active = o; o.select_set(True)
            bpy.ops.mesh.customdata_custom_splitnormals_clear()
        bm = bmesh.new(); bm.from_mesh(me)
        bmesh.ops.remove_doubles(bm, verts=bm.verts[:], dist=1e-5)
        holes = bmesh.ops.holes_fill(bm, edges=bm.edges[:], sides=12)
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
        bm.to_mesh(me); bm.free()
        if name in ROTATE_UPRIGHT:
            upright(o)
            if name in FLIP_AFTER_UPRIGHT:
                from mathutils import Matrix
                o.data.transform(Matrix.Rotation(math.pi, 4, "X"))
        if tris(o) > MAX_TRIS:
            mod = o.modifiers.new("dec", "DECIMATE"); mod.ratio = TARGET_TRIS / tris(o)
            bpy.context.view_layer.objects.active = o
            bpy.ops.object.modifier_apply(modifier=mod.name)
        bpy.context.view_layer.objects.active = o; o.select_set(True)
        bpy.ops.object.shade_smooth_by_angle(angle=math.radians(35))
        log[o.name] = (before, tris(o), len(holes["faces"]))

    # dat lai day y=0 (Blender z=0) va tam x/z ve goc
    from mathutils import Vector, Matrix
    pts = [v.co for o in meshes for v in o.data.vertices]
    mn = Vector([min(p[i] for p in pts) for i in range(3)])
    mx = Vector([max(p[i] for p in pts) for i in range(3)])
    off = Matrix.Translation(Vector((-(mn.x + mx.x) / 2, -(mn.y + mx.y) / 2, -mn.z)))
    for o in meshes:
        o.data.transform(off)

    for img in bpy.data.images:
        if img.size[0] > MAX_TEX:
            img.scale(MAX_TEX, MAX_TEX)
            log["tex"] = MAX_TEX
    bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", export_image_format="JPEG",
                              export_jpeg_quality=90, export_yup=True)
    print("FIXED", name, log, round(os.path.getsize(path) / 1e6, 2), "MB", flush=True)


for p in sys.argv[1:]:
    if p.endswith(".glb"):
        fix(p)

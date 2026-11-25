"""Import every Assets/_Project/Art/Chests/Chest_*.glb with Blender (bpy, headless) and print a layer table.

Checks a chest GLB is intact after a merge: it imports, has meshes, and has the layers BoxOpenSequence looks for
(direct children of the chest root named Body, Lid, Glow; optional Seal, Binding, Key, Record).
Run with the bpy wheel (pip install bpy, Python 3.13):  python Tools/Blender/check_chest_glbs.py
Exit code 1 if a file fails to import, is a Git LFS pointer, has no mesh, or has no Lid.
"""
import glob
import os
import sys

import bpy

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
FOLDER = os.path.join(REPO, "Assets", "_Project", "Art", "Chests")
LAYERS = ["Body", "Lid", "Glow", "Seal", "Binding", "Key", "Record"]


def clear():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def base(name):
    return name.split(".")[0]


def check(path):
    size = os.path.getsize(path)
    with open(path, "rb") as f:
        head = f.read(4)
    if head != b"glTF":
        return {"error": f"không phải GLB ({size} byte; con trỏ Git LFS? chạy git lfs pull)"}
    clear()
    try:
        bpy.ops.import_scene.gltf(filepath=path)
    except Exception as ex:  # noqa: BLE001
        return {"error": f"nhập lỗi: {ex}"}
    objs = list(bpy.context.scene.objects)
    meshes = [o for o in objs if o.type == "MESH"]
    tris = 0
    for o in meshes:
        o.data.calc_loop_triangles()
        tris += len(o.data.loop_triangles)
    # The chest root = the object whose children include Lid (falls back to any object named Lid).
    found = {}
    for o in objs:
        if base(o.name) in LAYERS:
            found.setdefault(base(o.name), o)
    lid = found.get("Lid")
    root = lid.parent.name if lid is not None and lid.parent is not None else "(gốc cảnh)"
    return {"size": size, "objects": len(objs), "meshes": len(meshes), "tris": tris, "root": root,
            "layers": [n for n in LAYERS if n in found]}


def main():
    files = sorted(glob.glob(os.path.join(FOLDER, "Chest_*.glb")))
    bad = 0
    print(f"{'Tệp':<20} {'KB':>6} {'Obj':>4} {'Mesh':>4} {'Tam giác':>8}  Gốc rương -> lớp")
    for path in files:
        r = check(path)
        name = os.path.basename(path)
        if "error" in r:
            bad += 1
            print(f"{name:<20} LỖI: {r['error']}")
            continue
        problems = []
        if r["meshes"] == 0:
            problems.append("không có mesh")
        if "Lid" not in r["layers"]:
            problems.append("không có Lid")
        if "Body" not in r["layers"]:
            problems.append("không có Body")
        if problems:
            bad += 1
        print(f"{name:<20} {r['size'] // 1024:>6} {r['objects']:>4} {r['meshes']:>4} {r['tris']:>8}  {r['root']} -> "
              f"{', '.join(r['layers'])}" + (f"   <-- {'; '.join(problems)}" if problems else ""))
    print(f"\n{len(files)} tệp, {bad} có vấn đề.")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())

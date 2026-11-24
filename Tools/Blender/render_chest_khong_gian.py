# Ảnh xem trước Cycles cho Chest_KhongGian: hàng trên đóng/hé/mở, hàng dưới 3 góc hình bóng.
# Run (từ gốc repo): python Tools/Blender/render_chest_khong_gian.py <thư mục tạm> Assets/_Project/Art/Source/render_chest_khong_gian_open.png
import bpy, math, sys
from mathutils import Vector
from PIL import Image
S=sys.argv[1]; OUTP=sys.argv[2]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath="Assets/_Project/Art/Chests/Chest_KhongGian.glb")
sc=bpy.context.scene; OB=bpy.data.objects
lid=OB["Lid"]; lid.rotation_mode="XYZ"; glow=OB["Glow"]; gs=tuple(glow.scale)
sc.render.engine="CYCLES"; sc.cycles.samples=48; sc.cycles.use_denoising=True
sc.render.resolution_x=480; sc.render.resolution_y=400
w=bpy.data.worlds.new("w"); sc.world=w; w.use_nodes=True; bg=w.node_tree.nodes["Background"]
bpy.ops.mesh.primitive_plane_add(size=6); fl=bpy.context.active_object
fm=bpy.data.materials.new("f"); fm.use_nodes=True; fm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value=(.06,.06,.08,1); fl.data.materials.append(fm)
def light(loc,e,col=(1,1,1)):
    d=bpy.data.lights.new("a","AREA"); d.energy=e; d.size=1.2; d.color=col
    o=bpy.data.objects.new("a",d); sc.collection.objects.link(o); o.location=loc
    o.rotation_euler=(Vector((0,0,.2))-Vector(loc)).to_track_quat("-Z","Y").to_euler()
light((-.8,-1,1.4),140,(1,.93,.85)); light((1,.6,1),40,(.7,.8,1)); light((0,.9,1.2),30)
cam=bpy.data.objects.new("c",bpy.data.cameras.new("c")); sc.collection.objects.link(cam); sc.camera=cam; cam.data.lens=50
def setcam(az,el,dist=1.35):
    a,e=math.radians(az),math.radians(el); t=Vector((0,0,.2))
    cam.location=t+Vector((math.sin(a)*math.cos(e),-math.cos(a)*math.cos(e),math.sin(e)))*dist
    cam.rotation_euler=(t-cam.location).to_track_quat("-Z","Y").to_euler()
def pose(deg,g):
    lid.rotation_euler=(math.radians(-deg),0,0); glow.scale=tuple(s*max(1e-4,g) for s in gs); glow.hide_render=g<.01
shots=[]
bg.inputs["Color"].default_value=(.10,.095,.13,1); bg.inputs["Strength"].default_value=.35
for i,(deg,g) in enumerate([(0,0),(30,.6),(110,1)]):
    pose(deg,g); setcam(28,22); sc.render.filepath=f"{S}/p{i}.png"; bpy.ops.render.render(write_still=True); shots.append(sc.render.filepath)
# silhouettes: black on white
pose(0,0); fl.hide_render=True
for o in OB:
    if o.type=="LIGHT": o.hide_render=True
bk=bpy.data.materials.new("k"); bk.use_nodes=True; bk.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value=(0,0,0,1); bk.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value=1
for o in OB:
    if o.type=="MESH" and o!=fl:
        for i in range(len(o.material_slots)): o.material_slots[i].material=bk
bg.inputs["Color"].default_value=(1,1,1,1); bg.inputs["Strength"].default_value=1; sc.cycles.samples=8
for i,(az,el) in enumerate([(0,5),(90,5),(35,35)]):
    setcam(az,el); sc.render.filepath=f"{S}/s{i}.png"; bpy.ops.render.render(write_still=True); shots.append(sc.render.filepath)
ims=[Image.open(p).convert("RGB") for p in shots]; W,H=ims[0].size
sheet=Image.new("RGB",(W*3,H*2))
for i,im in enumerate(ims): sheet.paste(im,((i%3)*W,(i//3)*H))
sheet.save(OUTP)

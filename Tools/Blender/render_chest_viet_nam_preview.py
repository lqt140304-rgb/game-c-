# Ảnh xem trước Cycles cho Chest_VietNam. Chạy: python render_chest_viet_nam_preview.py <thư mục ảnh>
import bpy, math, sys
from mathutils import Vector
S=sys.argv[-1]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath="/home/user/game-c-/Assets/_Project/Art/Chests/Chest_VietNam.glb")
sc=bpy.context.scene; OB=bpy.data.objects; lid=OB["Lid"]; lid.rotation_mode="XYZ"; glow=OB["Glow"]; gs=tuple(glow.scale)
for m in glow.data.materials:
    b=m.node_tree.nodes.get("Principled BSDF"); b.inputs["Emission Strength"].default_value=0.7
sc.render.engine="CYCLES"; sc.cycles.samples=48; sc.cycles.use_denoising=True
sc.render.resolution_x=480; sc.render.resolution_y=400
w=bpy.data.worlds.new("w"); sc.world=w; w.use_nodes=True; bg=w.node_tree.nodes["Background"]
bg.inputs["Color"].default_value=(0.10,0.095,0.13,1); bg.inputs["Strength"].default_value=0.35
bpy.ops.mesh.primitive_plane_add(size=6); fl=bpy.context.active_object
fm=bpy.data.materials.new("f"); fm.use_nodes=True; fm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value=(0.07,0.065,0.085,1); fl.data.materials.append(fm)
def light(loc,e,s,c=(1,1,1)):
    d=bpy.data.lights.new("a","AREA"); d.energy=e; d.size=s; d.color=c
    o=bpy.data.objects.new("a",d); sc.collection.objects.link(o); o.location=loc
    o.rotation_euler=(Vector((0,0,0.2))-Vector(loc)).to_track_quat("-Z","Y").to_euler()
light((-0.8,-1.0,1.4),140,1.2,(1,.93,.85)); light((1.0,0.6,1.0),40,1.5,(.7,.8,1)); light((0,0.9,1.2),30,1.0)
pl=bpy.data.lights.new("p","POINT"); pl.color=(1,.8,.45); po=bpy.data.objects.new("p",pl); sc.collection.objects.link(po); po.location=(0,0,0.24)
cam=bpy.data.objects.new("c",bpy.data.cameras.new("c")); sc.collection.objects.link(cam); sc.camera=cam
def shot(name,ang,g,az,el,silh=False):
    lid.rotation_euler=(math.radians(-ang),0,0)
    glow.scale=tuple(x*max(1e-4,g) for x in gs); glow.hide_render=g<0.01; pl.energy=5*g
    a=math.radians(az); d=1.35; t=Vector((0,0,0.2))
    cam.location=t+Vector((math.sin(a)*d,-math.cos(a)*d,math.tan(math.radians(el))*d)); cam.rotation_euler=(t-cam.location).to_track_quat("-Z","Y").to_euler()
    if silh:
        for o in OB:
            if o.type=="MESH" and o is not fl: o.hide_render=False
        sc.render.film_transparent=True; fl.hide_render=True
    sc.render.filepath=f"{S}/{name}.png"; bpy.ops.render.render(write_still=True)
bind=OB["Binding"]; seal=OB["Seal"]
shot("a_closed",0,0,24,22); bind.hide_render=seal.hide_render=True; shot("b_crack",12,0.4,24,22); shot("c_open",105,1,24,30)
bind.hide_render=seal.hide_render=False; glow.hide_render=True
shot("d_front",0,0,0,3,True); shot("e_side",0,0,90,3,True); shot("f_34",0,0,35,25,True)

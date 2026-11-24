import bpy, math, sys, mathutils
from mathutils import Vector
S="/tmp/claude-0/-home-user-game-c-/17b3666a-8289-5d56-8657-0055367ef0a9/scratchpad"
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath="/home/user/game-c-/Assets/_Project/Art/Chests/Chest_Golden.glb")
sc=bpy.context.scene; OB=bpy.data.objects
for n in ("Lid","Key"): OB[n].rotation_mode="XYZ"
glow=OB["Glow"]; gs0=tuple(glow.scale); key0=tuple(OB["Key"].location); ks0=tuple(OB["Key"].scale)
# make the glow a real light-emitting material
for m in glow.data.materials:
    b=m.node_tree.nodes.get("Principled BSDF")
    if b: b.inputs["Emission Strength"].default_value=0.7
sc.render.engine="CYCLES"; sc.cycles.device="CPU"; sc.cycles.samples=int(sys.argv[-2])
sc.cycles.use_denoising=True
try: sc.cycles.denoiser="OPENIMAGEDENOISE"
except Exception as e: print("denoise?",e)
sc.render.resolution_x=int(sys.argv[-1]); sc.render.resolution_y=int(int(sys.argv[-1])*0.75)
sc.view_settings.view_transform="AgX" if "AgX" in [t.identifier for t in sc.view_settings.bl_rna.properties["view_transform"].enum_items] else "Standard"
# world: soft dark-violet studio
w=bpy.data.worlds.new("w"); sc.world=w; w.use_nodes=True
bg=w.node_tree.nodes["Background"]; bg.inputs["Color"].default_value=(0.10,0.095,0.13,1); bg.inputs["Strength"].default_value=0.35
# floor
bpy.ops.mesh.primitive_plane_add(size=6,location=(0,0,0)); fl=bpy.context.active_object
fm=bpy.data.materials.new("f"); fm.use_nodes=True; fb=fm.node_tree.nodes["Principled BSDF"]; fb.inputs["Base Color"].default_value=(0.07,0.065,0.085,1); fb.inputs["Roughness"].default_value=0.85; fl.data.materials.append(fm)
def light(kind,loc,energy,size,color=(1,1,1)):
    d=bpy.data.lights.new(kind,kind); d.energy=energy; d.color=color
    if kind=="AREA": d.size=size
    o=bpy.data.objects.new(kind,d); sc.collection.objects.link(o); o.location=loc
    o.rotation_euler=(Vector((0,0,0.2))-Vector(loc)).to_track_quat("-Z","Y").to_euler(); return o
light("AREA",(-0.8,-1.0,1.4),140,1.2,(1,.93,.85)); light("AREA",(1.0,0.6,1.0),40,1.5,(.7,.8,1)); light("AREA",(0,0.9,1.2),30,1.0)
inner=bpy.data.lights.new("in","POINT"); inner.energy=0; inner.color=(1,.8,.45); inner.shadow_soft_size=0.03
inl=bpy.data.objects.new("in",inner); sc.collection.objects.link(inl); inl.location=(0,0.0,0.30)
cam=bpy.data.objects.new("cam",bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera=cam; cam.data.lens=50
def set_pose(lid,glw,kd,kt,ks,bob=0):
    OB["Lid"].rotation_euler=(math.radians(-lid),0,0)
    glow.scale=tuple(s*max(1e-4,glw) for s in gs0); glow.hide_render=glw<0.01
    inner.energy=5*glw
    k=OB["Key"]
    if kd is None: k.hide_render=True
    else:
        k.hide_render=False; k.location=(key0[0],key0[1]-kd,key0[2]+bob); k.rotation_euler=(0,math.radians(kt),0); k.scale=tuple(s*ks for s in ks0)
def set_cam(zoom,az=24):
    a=math.radians(az)
    tgt=Vector((0,0,0.13))*(1-zoom)+Vector((0,-0.20,0.17))*zoom
    dist=1.25*(1-zoom)+0.34*zoom
    cam.location=tgt+Vector((math.sin(a)*dist,-math.cos(a)*dist,0.45*dist*(1-zoom*0.4)))
    cam.rotation_euler=(tgt-cam.location).to_track_quat("-Z","Y").to_euler()

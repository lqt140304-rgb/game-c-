"""Cycles (CPU) preview rig for a chest GLB: exec() it from a driver script, then call set_pose / set_cam and render.
Env: CHEST_GLB = path of the chest (default Chest_Golden.glb; --glb wins if both are given). argv[-2] = samples, argv[-1] = width in px.
CHEST_EXPOSURE = EV offset (default 0; Magic uses about -1.3 because its pale stone burns out).
Light colour of the inner point light follows the Glow material, so purple chests glow purple.
"""
import bpy, math, sys, os, mathutils
from mathutils import Vector
# Usage (exec'd by a driver script that does the posing/rendering): argv ends with  [--glb <file>] [--size <chest width m>] <samples> <width px>
# Defaults = Golden chest. Chests without a Key child (e.g. Miner) are supported: set_pose just ignores the key arguments.
def _opt(flag, default):
    if flag in sys.argv:
        i=sys.argv.index(flag); v=sys.argv[i+1]; del sys.argv[i:i+2]; return v
    return default
GLB=_opt("--glb",os.environ.get("CHEST_GLB","/home/user/game-c-/Assets/_Project/Art/Chests/Chest_Golden.glb")); SZ=float(_opt("--size",0.6))/0.6   # 1.0 for Golden
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=GLB)
sc=bpy.context.scene; OB=bpy.data.objects
HAS_KEY="Key" in OB
for n in (("Lid","Key") if HAS_KEY else ("Lid",)): OB[n].rotation_mode="XYZ"
glow=OB["Glow"]; gs0=tuple(glow.scale)
if HAS_KEY: key0=tuple(OB["Key"].location); ks0=tuple(OB["Key"].scale)
# make the glow a real light-emitting material
for m in glow.data.materials:
    b=m.node_tree.nodes.get("Principled BSDF")
    if b: b.inputs["Emission Strength"].default_value=0.7
sc.render.engine="CYCLES"; sc.cycles.device="CPU"; sc.cycles.samples=int(sys.argv[-2])
sc.cycles.use_denoising=True
try: sc.cycles.denoiser="OPENIMAGEDENOISE"
except Exception as e: print("denoise?",e)
sc.render.resolution_x=int(sys.argv[-1]); sc.render.resolution_y=int(int(sys.argv[-1])*0.75)
sc.view_settings.exposure=float(os.environ.get("CHEST_EXPOSURE","0"))
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
light("AREA",(-0.8*SZ,-1.0*SZ,1.4),140*SZ*SZ,1.2*SZ,(1,.93,.85)); light("AREA",(1.0*SZ,0.6*SZ,1.0),40*SZ*SZ,1.5*SZ,(.7,.8,1)); light("AREA",(0,0.9*SZ,1.2),30*SZ*SZ,1.0*SZ)
_gb=glow.data.materials[0].node_tree.nodes.get("Principled BSDF")
_gc=tuple(_gb.inputs["Emission Color"].default_value)[:3] if _gb else (1,.8,.45)
inner=bpy.data.lights.new("in","POINT"); inner.energy=0; inner.color=_gc; inner.shadow_soft_size=0.03
inl=bpy.data.objects.new("in",inner); sc.collection.objects.link(inl); inl.location=(0,0.0,0.30*(SZ if HAS_KEY else 1.0)*(1 if HAS_KEY else 0.9))
cam=bpy.data.objects.new("cam",bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera=cam; cam.data.lens=50
def set_pose(lid,glw,kd,kt,ks,bob=0):
    OB["Lid"].rotation_euler=(math.radians(-lid),0,0)
    glow.scale=tuple(s*max(1e-4,glw) for s in gs0); glow.hide_render=glw<0.01
    inner.energy=5*glw
    if not HAS_KEY: return
    k=OB["Key"]
    if kd is None: k.hide_render=True
    else:
        k.hide_render=False; k.location=(key0[0],key0[1]-kd,key0[2]+bob); k.rotation_euler=(0,math.radians(kt),0); k.scale=tuple(s*ks for s in ks0)
def set_cam(zoom,az=24,tz_far=0.13,tz_near=0.17,dist_far=1.25,dist_near=0.34):
    a=math.radians(az)
    tgt=(Vector((0,0,tz_far))*(1-zoom)+Vector((0,-0.20,tz_near))*zoom)*SZ
    dist=(dist_far*(1-zoom)+dist_near*zoom)*SZ
    cam.location=tgt+Vector((math.sin(a)*dist,-math.cos(a)*dist,0.45*dist*(1-zoom*0.4)))
    cam.rotation_euler=(tgt-cam.location).to_track_quat("-Z","Y").to_euler()

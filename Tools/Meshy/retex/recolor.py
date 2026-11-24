import sys,colorsys,numpy as np,trimesh
from PIL import Image,ImageDraw
def load(p):
    s=trimesh.load(p);m=list(s.geometry.values())[0]
    return m,np.asarray(m.visual.material.baseColorTexture.convert("RGB")).astype(float)
def facemasks(m,W,H):
    uv=m.visual.uv[m.faces]; return [[(x*W,(1-y)*H) for x,y in t] for t in uv]
def face_mean(tex,poly):
    xs=[p[0] for p in poly];ys=[p[1] for p in poly]
    cx,cy=int(np.mean(xs)),int(np.mean(ys));H,W=tex.shape[:2]
    return tex[min(max(cy,0),H-1),min(max(cx,0),W-1)]
def paint(tex,polys,fn):
    H,W=tex.shape[:2];mk=Image.new("L",(W,H),0);d=ImageDraw.Draw(mk)
    for p in polys: d.polygon(p,fill=255)
    mk=np.asarray(mk.filter(__import__("PIL.ImageFilter").ImageFilter.MaxFilter(5)))>0
    tex[mk]=fn(tex[mk]);return tex
def save(m,tex,out):
    m.visual.material.baseColorTexture=Image.fromarray(np.clip(tex,0,255).astype(np.uint8))
    m.export(out)
def hsv(px):
    a=px/255.;mx=a.max(1);mn=a.min(1);s=np.where(mx>0,(mx-mn)/np.maximum(mx,1e-6),0)
    h=np.array([colorsys.rgb_to_hsv(*c)[0]*360 for c in a]);return h,s,mx

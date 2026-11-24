from recolor import *
import sys
m,tex=load(sys.argv[1]);H,W=tex.shape[:2];P=facemasks(m,W,H)
cols=np.array([face_mean(tex,p) for p in P]);h,s,v=hsv(cols)
oc=np.median(cols[(h>15)&(h<40)&(s>0.5)],0)
def fix(px):
    lum=px.mean(1);out=px.copy();out[lum>=95]=oc
    out[lum<95]=np.minimum(px[lum<95],40);return out
tex=paint(tex,P,fix);save(m,tex,sys.argv[2])
# remove stray number on back
m2,tex2=m,tex
cols=np.array([face_mean(tex2,p) for p in P]);dark=cols.mean(1)<70
c=m.triangles_center-m.bounds.mean(0);u=c/np.linalg.norm(c,axis=1)[:,None]
vd=u[dark].mean(0)
# front number dir = dark faces with largest cluster; use those with u.y in mid band
mid=dark&(np.abs(u[:,1])<0.35);fd=u[mid].mean(0);fd[1]=0;fd/=np.linalg.norm(fd)
print("front dir",fd.round(2))
back=[P[i] for i in range(len(P)) if (u[i]@fd< -0.1) and abs(u[i,1])<0.45]
def clean(px):
    out=px.copy();out[px.mean(1)<95]=oc;return out
tex2=paint(tex2,back,clean);save(m,tex2,sys.argv[2])

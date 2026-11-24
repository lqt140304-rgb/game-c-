from recolor import *
import sys
m,tex=load(sys.argv[1]);H,W=tex.shape[:2];P=facemasks(m,W,H)
cols=np.array([face_mean(tex,p) for p in P]);h,s,v=hsv(cols)
gold=(h>35)&(h<60)&(s>0.35);gc=np.median(cols[gold],0)
n=m.face_normals;c=m.triangles_center-m.bounds.mean(0)
vd=n[gold].mean(0);vd/=np.linalg.norm(vd)
fr=(c/np.linalg.norm(c,axis=1)[:,None])@vd
warm=((h<62)|(h>300))&(s>0.12)
yel=(h>30)&(h<66)&(s>0.08); pink=((h<32)|(h>300))&(s>0.12)
visor=(gold&(fr>0.3))|((yel|pink)&(fr>0.62))|(fr>0.8)
adj=m.face_adjacency
for it in range(4):
    cnt=np.zeros(len(visor));np.add.at(cnt,adj[:,0],visor[adj[:,1]]);np.add.at(cnt,adj[:,1],visor[adj[:,0]])
    visor=visor|((cnt>=2)&(fr>0.4))
    visor=visor&~((cnt==0))
print("visor faces",visor.sum())
def to_white(px):
    lum=px.mean(1,keepdims=True);return np.repeat(np.clip(lum*1.05+20,190,240),3,1)
def to_gold(px):
    lum=px.mean(1,keepdims=True);return gc*np.clip(lum/gc.mean(),0.8,1.15)
tex=paint(tex,[P[i] for i in range(len(P)) if not visor[i]],to_white)
tex=paint(tex,[P[i] for i in range(len(P)) if visor[i]],to_gold);save(m,tex,sys.argv[2])

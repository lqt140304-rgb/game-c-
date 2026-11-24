# Daruma (H35): to lai vet xanh trang (lem tu binh gom) o phan duoi than thanh mau do cua than.
# Chay: python3 fix_daruma.py vao.glb ra.glb
from recolor import *
import sys
m,tex=load(sys.argv[1]);H,W=tex.shape[:2];P=facemasks(m,W,H)
cols=np.array([face_mean(tex,p) for p in P]);h,s,v=hsv(cols)
red=(s>0.45)&((h<20)|(h>340));rc=np.median(cols[red],0)
b=m.bounds;y=(m.triangles_center[:,1]-b[0][1])/(b[1][1]-b[0][1])
bad=(y<0.35)&~red
print("red",rc.round(),"faces fixed",bad.sum())
tex=paint(tex,[P[i] for i in np.where(bad)[0]],lambda px:np.tile(rc,(len(px),1)))
save(m,tex,sys.argv[2])

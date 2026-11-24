from recolor import *
import sys
# L03: red patches on the blue cushion -> cushion blue
m,tex=load(sys.argv[1]);H,W=tex.shape[:2];P=facemasks(m,W,H)
cols=np.array([face_mean(tex,p) for p in P]);h,s,v=hsv(cols)
blue=(h>215)&(h<250)&(s>0.45);bc=np.median(cols[blue],0);print("cushion",bc)
def fix(px):
    hh,ss,vv=hsv(px);bad=((hh<40)|(hh>290))&(ss>0.25)
    out=px.copy();out[bad]=bc*np.clip(px[bad].max(1,keepdims=True)/bc.max(),0.75,1.15);return out
tex=paint(tex,P,fix);save(m,tex,sys.argv[2])

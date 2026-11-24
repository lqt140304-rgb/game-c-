# To lai loi mau nho cho mon Rac (khong ton credit).
# Chay: python3 fix_rac.py <ma> vao.glb ra.glb   (ma: R25)
from recolor import *
import sys
code,src,dst=sys.argv[1:4]
m,tex=load(src);H,W=tex.shape[:2];P=facemasks(m,W,H)
cols=np.array([face_mean(tex,p) for p in P]);h,s,v=hsv(cols)

if code=="R25":   # mu giay bac: xoa dom moc xanh (lem tu banh bao), to mau bac
    # dom nam lot trong mat lon nen chon theo tung diem anh, khong theo tam mat
    silver=np.median(cols,0)
    def fix(px):
        g=px[:,1]-np.maximum(px[:,0],px[:,2])  # xanh la troi hon do va lam
        out=px.copy();bad=g>6;out[bad]=silver;print("pixels",bad.sum(),"silver",silver.round());return out
    tex=paint(tex,P,fix)
save(m,np.clip(tex,0,255),dst)

# To lai loi mau nho cho mon Thuong (khong ton credit).
# Chay: python3 fix_thuong.py <ma> vao.glb ra.glb   (ma: V13 V14 V17 V24 V25 V26)
from recolor import *
import sys
code,src,dst=sys.argv[1:4]
m,tex=load(src);H,W=tex.shape[:2];P=facemasks(m,W,H)

cols=np.array([face_mean(tex,p) for p in P])
def tone(t,scale,tint=(1,1,1),contrast=1.0):
    mu=cols.mean()  # trung binh cua rieng mon (texture la tam gop chung 8 mon)
    t=(t-mu)*contrast+mu
    return t*scale*np.array(tint)

if code=="V13":   # radio: xoa vet num van sang mau o mat sau (-z), to mau go
    l=cols.mean(1);z=m.triangles_center[:,2];b=m.bounds
    back=z<b[0][2]+0.25*(b[1][2]-b[0][2]);wood=np.median(cols[back&(l<130)],0)
    bad=np.where(back&(l>=130))[0];print("faces",len(bad),"wood",wood.round())
    tex=paint(tex,[P[i] for i in bad],lambda px:np.tile(wood,(len(px),1)))
elif code=="V14": # xuc xac chi: xam chi toi, hoi ngả lam
    tex=tone(tex,0.62,(0.97,1.0,1.06),1.2)
elif code=="V24": # tay robot thiec: xam thiec toi hon, tang tuong phan
    tex=tone(tex,0.5,(1.0,1.0,1.04),1.5)
elif code=="V25": # da mat trang: xam tro
    tex=tone(tex,0.7,(1.02,1.0,0.97),1.3)
elif code=="V26": # tui giay bac: tang tuong phan cho ra anh kim loai
    tex=tone(tex,0.6,(1.0,1.0,1.03),2.5)
elif code=="V17": # bi ve: tang do bao hoa
    g=tex.mean(2,keepdims=True);tex=g+(tex-g)*3.0;tex=tex*0.9
save(m,np.clip(tex,0,255),dst)

# To lai loi mau nho cho mon Thuong (khong ton credit).
# Chay: python3 fix_thuong.py <ma> vao.glb ra.glb   (ma: V13 V14 V17 V24 V25 V26 V43 V43b V45 V46 V48 H40)
# V43b, V46, H40 (2026-11-24, ra soat cua chu du an) chay tren file da co trong repo.
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
elif code=="V43": # tay cam kieu cu: than trang bac -> xam
    def f(px):
        a=px/255.;mx=a.max(1);sa=(mx-a.min(1))/np.maximum(mx,1e-6);sel=(sa<0.2)&(mx>0.45)
        out=px.copy();out[sel]=np.array((150,150,155))*(0.6+0.45*mx[sel,None]);return out
    tex=paint(tex,P,f)
elif code=="V43b": # tay cam: day cung mau xam voi than, xoa vet lem mau (chi giu nut ABXY)
    c=m.triangles_center;lo=m.bounds[0];s=560/max(m.extents)
    ix,iy=(c[:,0]-lo[0])*s+30,(c[:,2]-lo[2])*s+30   # toa do anh nhin tu tren (cung cach ve so do)
    l=cols.mean(1);mx=cols.max(1);sa=(mx-cols.min(1))/np.maximum(mx,1)
    ctrl=ix>335;body=np.median(cols[ctrl&(sa<0.08)&(l>80)],0);print("body",body.round())
    cable=~ctrl
    def gray(px):  # giu sang toi cua texture, doi sang mau than
        lum=px.mean(1,keepdims=True);return np.clip(body*1.15*lum/max(body.mean(),1),0,255)
    tex=paint(tex,[P[i] for i in np.where(cable)[0]],gray)
    abxy=(np.hypot(ix-515,iy-170)<55)&(c[:,1]>lo[1]+0.5*m.extents[1])   # chi nua tren cua cum nut
    def unsmear(px):
        a=px/255.;mxp=a.max(1);sp=(mxp-a.min(1))/np.maximum(mxp,1e-6);bad=sp>0.15
        out=px.copy();out[bad&(mxp<0.35)]=(38,38,40);out[bad&(mxp>=0.35)]=gray(px[bad&(mxp>=0.35)]);return out
    tex=paint(tex,[P[i] for i in np.where(ctrl&~abxy)[0]],unsmear)
elif code=="V46": # dua: do son mai deu, dau ma vang ro (dau to = 17% chieu dai, xac dinh bang mat vang)
    c=m.triangles_center;ax=c[:,[0,2]]-c[:,[0,2]].mean(0);w,e=np.linalg.eigh(np.cov(ax.T));t=ax@e[:,1]
    h,sa,v=hsv(cols);yel=(h>35)&(h<70)&(v>0.55)
    if t[yel].mean()<0:t=-t
    L=t.max()-t.min();gold=t>t.max()-0.17*L;print("gold faces",gold.sum())
    rr=np.random.default_rng(3)
    def lacquer(px):return np.clip(np.array((150,18,16))*(0.93+0.07*rr.random((len(px),1))),0,255)
    def golden(px):return np.clip(np.array((215,160,45))*(0.9+0.1*rr.random((len(px),1))),0,255)
    tex=paint(tex,[P[i] for i in np.where(~gold)[0]],lacquer)
    tex=paint(tex,[P[i] for i in np.where(gold)[0]],golden)
    m.visual.material.roughnessFactor=0.3
elif code=="H40": # chan go: go nau bong (giu van), vong dong, day da (cac phan xam/trang)
    h,sa,v=hsv(cols)
    brass=(h>35)&(h<70)&(sa>0.25)&(v>0.55)
    c=m.triangles_center;lo=m.bounds[0];sc=560/max(m.extents)
    X,Y=(c[:,0]-lo[0])*sc+30,(c[:,2]-lo[2])*sc+30   # anh nhin tu tren (x, z)
    loops=(X>525)|((X>285)&(X<360)&(Y>182))|((X>400)&(X<525)&(Y>200))   # vong day tach khoi than (chon tay)
    leather=(sa<0.2)|loops
    wood=~brass&~leather;print("brass",brass.sum(),"leather",leather.sum(),"wood",wood.sum())
    wmu=cols[wood].mean()
    def woodf(px):return np.clip(np.array((112,60,30))*(px.mean(1,keepdims=True)/wmu)**1.3,0,255)
    def brassf(px):return np.clip(np.array((196,140,52))*(0.8+0.35*px.mean(1,keepdims=True)/255),0,255)
    def leatherf(px):return np.clip(np.array((58,38,28))*(0.75+0.5*px.mean(1,keepdims=True)/255),0,255)
    tex=paint(tex,[P[i] for i in np.where(wood)[0]],woodf)
    tex=paint(tex,[P[i] for i in np.where(leather)[0]],leatherf)
    tex=paint(tex,[P[i] for i in np.where(brass)[0]],brassf)
    m.visual.material.roughnessFactor=0.35
elif code=="V45": # nhan su da: dom vang sot tu can lightstick -> mau sa thach
    h,sa,v=hsv(cols);stone=np.median(cols,0);bad=np.where((h>35)&(h<65)&(sa>0.45))[0];print("faces",len(bad))
    tex=paint(tex,[P[i] for i in bad],lambda px:np.tile(stone,(len(px),1)))
elif code=="V48": # tan o (truoc khi lap can): manh toi sot tu hop bento o giua tan -> mau tre
    l=cols.mean(1);bad=np.where(l<70)[0];print("faces",len(bad))
    tex=paint(tex,[P[i] for i in bad],lambda px:np.tile((200,160,100),(len(px),1)))
save(m,np.clip(tex,0,255),dst)

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
def recolor_faces(faces,rgb,keep_dark=None):
    """To mat chon bang mau rgb, giu sang toi theo do sang cu (de con nep nhan)."""
    global tex
    def f(px):
        l=px.mean(1,keepdims=True)/255.;out=np.array(rgb)*(0.55+0.6*l)
        if keep_dark is not None: out=np.where(l<keep_dark,px,out)
        return out
    tex=paint(tex,[P[i] for i in faces],f)
def grey_bright(px,rgb,smax=0.25,vmin=0.5):
    """Diem anh trang bac (it mau, sang) -> mau rgb, giu sang toi."""
    a=px/255.;mx=a.max(1);s=(mx-a.min(1))/np.maximum(mx,1e-6);sel=(s<smax)&(mx>vmin)
    out=px.copy();out[sel]=np.array(rgb)*(0.6+0.45*mx[sel,None]);print("pixels",sel.sum());return out
def parts():
    t=trimesh.Trimesh(m.vertices,m.faces,process=False);t.merge_vertices(merge_tex=True,merge_norm=True,digits_vertex=5)
    return sorted(trimesh.graph.connected_components(t.face_adjacency,min_len=1),key=len,reverse=True)
allf=range(len(P))
if code=="R14":   # so lien lac: mep giay trang bac -> giay ngà
    tex=paint(tex,P,lambda px:grey_bright(px,(225,215,190)))
if code=="R16":   # goi om: vet bong trang bac -> vai kem
    tex=paint(tex,P,lambda px:grey_bright(px,(225,215,195),0.3,0.4))
if code=="R29":   # tay cam: than trang bac -> xam
    tex=paint(tex,P,lambda px:grey_bright(px,(150,150,155),0.2,0.45))
if code=="R32":   # bang doi truong: mat trong bac -> vang, giu chu C toi
    tex=paint(tex,P,lambda px:grey_bright(px,(235,205,60),0.35,0.45))
if code=="R26":   # tam pin: 2 bo day roi -> do va den
    pp=parts()
    recolor_faces(pp[1],(170,30,30));recolor_faces(pp[2],(35,35,38))
if code=="R31":   # coi: day deo con doan trang -> do; than coi (da giac XZ) giu nguyen
    c=m.triangles_center;lo=c.min(0);s=500/max(m.extents)
    q=(c[:,[0,2]]-lo[[0,2]])*s+50
    mk=Image.new("L",(600,600),0);ImageDraw.Draw(mk).polygon([(262,285),(440,45),(560,130),(410,380),(410,480),(300,480),(300,380)],fill=1)
    mk=np.asarray(mk);inside=mk[np.clip(q[:,1].astype(int),0,599),np.clip(q[:,0].astype(int),0,599)]>0
    recolor_faces(np.where(~inside)[0],(190,35,40))
# Chinh mau tung mon: (do sang, do dam mau, tong mau RGB, tuong phan)
TONE={
 "R27":(0.85,1.2,(1,1,1),1.1),        # the nho
 "R33":(0.8,1.5,(1,0.92,0.76),1.0),  # ve concert: giay ngả vang, dau do dam
 "R34":(0.95,1.2,(1,1,1),1.1),        # loa keo keo
 "R35":(0.78,1.6,(0.93,0.88,1),1.2),  # lightstick: nhua trang ngả tim
 "R36":(1.0,1.0,(1,1,1),1.1),         # toc gia
 "R38":(0.9,1.3,(1,1,1),1.1),         # ban cau co
 "R41":(0.82,1.0,(1,0.95,0.85),1.3),  # nhan su thach cao: trang ngả kem, lo vet ban
 "R42":(0.85,1.8,(1,1,1),1.1),        # chai gom xanh
 "R43":(0.8,1.0,(1,1,1),1.1),         # bit mat
 "R44":(0.9,1.3,(1,1,1),1.1),         # chan go
 "R46":(0.85,1.8,(1,1,1),1.1),        # bao li xi do
 "R47":(0.85,1.8,(1,1,1),1.1),        # dua do
 "R48":(0.8,1.3,(1,0.92,0.76),1.0),   # que xam: giay ngả vang
 "R49":(0.95,1.4,(1,1,1),1.1),        # hop bento
 "R50":(0.85,1.7,(1,1,1),1.1),        # o giay do
 # dot 3 (lam lai 13 mon + R26)
 "R14":(0.85,2.0,(0.88,0.95,1.1),1.15),# so lien lac: bia xanh dam hon
 "R15":(0.95,1.3,(1,1,1),1.1),        # vo bim bim
 "R16":(0.9,1.2,(1,0.96,0.86),1.1),   # goi om: trang ngả vang o
 "R17":(0.85,1.2,(1,0.94,0.82),1.1),  # poster: giay ngả vang
 "R26":(0.85,1.3,(1,1,1),1.2),        # tam pin chay
 "R28":(0.9,1.0,(1,1,1),1.15),        # dia game
 "R29":(0.9,1.0,(1,1,1),1.15),        # tay cam xam
 "R30":(0.85,1.0,(1,0.95,0.85),1.2),  # ve nhau: giay ngả vang
 "R31":(0.85,1.4,(1,1,1),1.1),        # coi, day do
 "R32":(0.9,1.5,(1,1,1),1.1),         # bang vang
 "R37":(0.85,1.0,(1,1,1),1.2),        # chuong gio kim loai
 "R39":(0.9,1.2,(1,1,1),1.1),         # lo gom
}
if code in TONE:
    k,sat,tint,con=TONE[code];mu=cols.mean()
    g=tex.mean(2,keepdims=True);tex=g+(tex-g)*sat          # do dam mau
    tex=((tex-mu)*con+mu)*k*np.array(tint)                  # tuong phan, do sang, tong
save(m,np.clip(tex,0,255),dst)

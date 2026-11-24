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
 "R13":(0.95,1.2,(1,0.97,0.9),1.1),   # que kem go
 "R14":(0.85,2.0,(0.88,0.95,1.1),1.15),# so lien lac: bia xanh dam hon
 "R15":(0.95,1.3,(1,1,1),1.1),        # vo bim bim
 "R16":(0.9,1.2,(1,0.96,0.86),1.1),   # goi om: trang ngả vang o
 "R17":(0.85,1.2,(1,0.94,0.82),1.1),  # poster: giay ngả vang
 "R20":(0.9,1.3,(1,0.93,0.8),1.1),    # mui go
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

# Tach mon dinh nhau sau Meshy (dot 7 Thuong + 5 Hiem moi, 2026-11-24): chon mat theo vung va mau.
# Chay trong thu muc lam viec co items_ht/c00..c19.glb (export.py xuat moi manh, kich thuoc 0.3) va thu muc cut/, final/.
# Toa do vung = toa do anh nhin tu tren: (tam mat - goc hop bao) * 500/canh dai nhat + 50.
# Mau cho dot khac: doi so manh va vung.
import sys,os;sys.path.insert(0,os.path.join(os.path.dirname(os.path.abspath(__file__)),'retex'))
from recolor import *
def ld(f):
    m,tex=load('items_ht/'+f+'.glb');H,W=tex.shape[:2];P=facemasks(m,W,H)
    cols=np.array([face_mean(tex,p) for p in P]);s=500/max(m.extents)
    q=(m.triangles_center-m.bounds[0])*s+50   # q[:,0]=x, q[:,2]=z (anh nhin tu tren), q[:,1]=y
    h,sa,v=hsv(cols);return m,q,cols,h,sa,v
def sub(m,sel,minfrac=0.02):
    g=m.submesh([np.where(sel)[0]],append=True)
    t=trimesh.Trimesh(g.vertices,g.faces,process=False);t.merge_vertices(merge_tex=True,merge_norm=True,digits_vertex=4)
    cc=trimesh.graph.connected_components(t.face_adjacency,min_len=1)
    keep=np.concatenate([c for c in cc if len(c)>=minfrac*len(g.faces)])  # bo mau vun le
    return g.submesh([np.sort(keep)],append=True)
def seg_dist(p,a,b):
    a,b=np.array(a,float),np.array(b,float);t=np.clip(((p-a)@(b-a))/((b-a)@(b-a)),0,1);return np.linalg.norm(p-(a+t[:,None]*(b-a)),axis=1)
out={}
# c00: nhan su + can lightstick (ong gan chia ra phia z lon)
m,q,c,h,sa,v=ld('c00');handle=q[:,2]>342
out['nhan_su']=sub(m,~handle);can=sub(m,handle)
# c12: o + hop bento. hop = phia x lon hon tan o, khong phai mau tre (vang nau)
m,q,c,h,sa,v=ld('c12');tre=(h>20)&(h<50)&(sa>0.25)&(v>0.45)
bento=(q[:,0]>290)&~tre
out['o']=sub(m,~bento);out['bento']=sub(m,bento)
# c16: chan go + chiec dua nam xuyen qua
m,q,c,h,sa,v=ld('c16');red=((h<15)|(h>340))&(sa>0.45)
p=q[red][:,[0,2]];k,b=np.polyfit(p[:,1],p[:,0],1)   # duong thang x=k*z+b qua cac mat do
d=np.abs(q[:,0]-(k*q[:,2]+b))/np.hypot(1,k);dua=red|((d<22)&(q[:,2]<420))
out['chan_go']=sub(m,~dua)
# c03: dau tron lightstick (bo day va khung), ghep can tu c00
m,q,c,h,sa,v=ld('c03');ball=np.hypot(q[:,0]-432,q[:,2]-396)<114*1.05
out['ls_dau']=sub(m,ball);out['ls_can']=can
for k,g in out.items():g.export('cut/%s.glb'%k);print(k,len(g.faces),g.extents.round(3))
# c12 lan 2: chi giu tan o (mat gan mat phang khop tu cac mat do), can tre lap bang Blender
m,q,c,h,sa,v=ld('c12');red=((h<15)|(h>340))&(sa>0.45)&(v>0.4)
P0=m.triangles_center;cen=P0[red].mean(0);u,sv,vt=np.linalg.svd(P0[red]-cen);n=vt[2]
dist=np.abs((P0-cen)@n);rad=np.linalg.norm((P0-cen)-np.outer((P0-cen)@n,n),axis=1);R=np.percentile(rad[red],98)
tan=sub(m,(dist<0.25*R)&(rad<1.05*R),0.05);tan.export('cut/o_tan.glb');print('o_tan',len(tan.faces),tan.extents.round(3),'normal',n.round(3))
# Xuat co that (tam giua, day y=0, canh dai nhat = kich thuoc)
def fin(g,size,name):
    g=g.copy();b=g.bounds;g.apply_translation([-(b[0][0]+b[1][0])/2,-b[0][1],-(b[0][2]+b[1][2])/2]);g.apply_scale(size/max(g.extents))
    g.export('final/%s.glb'%name);print(name,len(g.faces),g.extents.round(3))
L=lambda f:list(trimesh.load('items_ht/'+f+'.glb').geometry.values())[0]
fin(L('c07'),0.21,'V42_so_ho_khau')
fin(L('c02'),0.26,'V43_tay_cam_kieu_cu')
fin(out['nhan_su'],0.20,'V45_tuong_nhan_su_da')
d1=L('c13');d2=d1.copy();ax=d1.vertices[:,[0,2]];ax=ax-ax.mean(0);w,e=np.linalg.eigh(np.cov(ax.T));perp=e[:,0]   # doi dua: chiec thang c13 nhan doi, lech ngang 1,6 cm
d2.apply_translation([perp[0]*0.016/0.3*0.3,0,perp[1]*0.016/0.3*0.3]);fin(trimesh.util.concatenate([d1,d2]),0.24,'V46_dua_do_dau_vang')
fin(L('c11'),0.107,'H36_anh_polaroid_ngoai_hanh_tinh')
fin(out['chan_go'],0.55,'H40_chan_go_thuyen_truong')

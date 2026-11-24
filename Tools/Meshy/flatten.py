# lam phang nen tung o va to mo duong noi: python3 flatten.py vao.png ra.png cols rows
import sys;from PIL import Image;import numpy as np
a=np.asarray(Image.open(sys.argv[1]).convert('RGB')).astype(float);H,W,_=a.shape;C,R=int(sys.argv[3]),int(sys.argv[4])
xs=[round(W*i/C) for i in range(C+1)];ys=[round(H*j/R) for j in range(R+1)];out=a.copy();T=np.array([222,222,222.])
for i in range(C):
 for j in range(R):
  x0,x1,y0,y1=xs[i],xs[i+1],ys[j],ys[j+1];p=a[y0:y1,x0:x1];h,w,_=p.shape
  yy,xx=np.mgrid[0:h,0:w];m=np.zeros((h,w),bool);b=12;m[:b]=m[-b:]=True;m[:,:b]=m[:,-b:]=True
  A=np.c_[xx[m],yy[m],np.ones(m.sum())]
  for c in range(3):
   co=np.linalg.lstsq(A,p[...,c][m],rcond=None)[0];out[y0:y1,x0:x1,c]=p[...,c]-(co[0]*xx+co[1]*yy+co[2])+T[c]
for x in xs[1:-1]:
 l,r=out[:,x-8].copy(),out[:,x+8].copy()
 for k in range(-7,8): t=(k+8)/16;out[:,x+k]=(1-t)*l+t*r
for y in ys[1:-1]:
 u,d=out[y-8].copy(),out[y+8].copy()
 for k in range(-7,8): t=(k+8)/16;out[y+k]=(1-t)*u+t*d
Image.fromarray(np.clip(out,0,255).astype(np.uint8)).save(sys.argv[2])

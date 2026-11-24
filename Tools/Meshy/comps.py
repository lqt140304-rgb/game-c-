import sys,trimesh,numpy as np
from scipy.sparse import coo_matrix
from scipy.sparse.csgraph import connected_components
def comps(path):
    m=list(trimesh.load(path).geometry.values())[0]
    _,inv=np.unique(np.round(m.vertices,5),axis=0,return_inverse=True);inv=inv.ravel()
    f=inv[m.faces];n=inv.max()+1
    g=coo_matrix((np.ones(len(f)*2),(np.r_[f[:,0],f[:,1]],np.r_[f[:,1],f[:,2]])),shape=(n,n))
    k,lab=connected_components(g,directed=False);return m,lab[f[:,0]],k
if __name__=="__main__":
    for t in sys.argv[1:]:
        m,fl,k=comps(f"{t}_all.glb");cnt=np.bincount(fl);print(t,"faces",len(m.faces),"comps",k)
        for c in range(k):
            v=m.vertices[m.faces[fl==c].ravel()];print(" ",c,cnt[c],np.round(v.min(0),2),np.round(v.max(0),2))

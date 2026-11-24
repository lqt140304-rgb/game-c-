import sys,json,numpy as np,trimesh
sys.path.insert(0,'.')
from comps import comps
spec=json.loads(sys.argv[2])  # {comp: [name, size_m]}; comp may be "1+2" to merge pieces
m,fl,k=comps(sys.argv[1]+"_all.glb")
for c,(name,size) in spec.items():
    sub=m.submesh([np.where(np.isin(fl,[int(x) for x in c.split('+')]))[0]],append=True)
    b=sub.bounds;sub.apply_translation([-(b[0][0]+b[1][0])/2,-b[0][1],-(b[0][2]+b[1][2])/2])
    sub.apply_scale(size/max(sub.extents))
    sub.export(f"items_ht/{name}.glb");print(name,len(sub.faces),np.round(sub.extents,3))

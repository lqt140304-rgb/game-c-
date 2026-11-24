# Nen den (V32): Meshy ra mau xam, to toi lai toan bo texture (giu bac than nhat hon).
# Chay: python3 fix_candle.py vao.glb ra.glb
from recolor import *
import sys
m,tex=load(sys.argv[1])
tex=tex*0.35
save(m,tex,sys.argv[2])

import json,sys,time,urllib.request
out,items=sys.argv[1],sys.argv[2]
n12=len(sys.argv)>3 and sys.argv[3]=="12"  # 12 mon: luoi 4x3, anh 4:3
def call(m,p,b=None):
    r=urllib.request.Request("https://api.meshy.ai/openapi/"+p,method=m,data=json.dumps(b).encode() if b else None,headers={"Content-Type":"application/json"})
    return json.load(urllib.request.urlopen(r))
prompt=(("Twelve" if n12 else "Eight")+" separate low-poly stylized 3D game props laid out in a "+("4 by 3 grid arrangement (4 columns, 3 rows)" if n12 else "4 by 2 arrangement")+" on a plain light grey studio floor, "
"each object standing on the floor with a soft contact shadow, very wide empty gaps between objects so none touch or overlap, "
"faceted flat-shaded low-poly style, muted neutral colors, three-quarter view from slightly above, even soft lighting, no text, no labels, no grid lines. "
"Objects, "+("row by row from top-left to bottom-right" if n12 else "top row left to right then bottom row left to right")+": "+items)
t=call("POST","v1/text-to-image",{"ai_model":"nano-banana-pro","prompt":prompt,"aspect_ratio":"4:3" if n12 else "16:9"})["result"]
while True:
    s=call("GET","v1/text-to-image/"+t)
    if s["status"] in("SUCCEEDED","FAILED","CANCELED"):break
    time.sleep(5)
print(s["status"],s.get("consumed_credits"))
urllib.request.urlretrieve(s["image_urls"][0],out)

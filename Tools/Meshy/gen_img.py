import json,sys,time,urllib.request
out,items=sys.argv[1],sys.argv[2]
n=int(sys.argv[3]) if len(sys.argv)>3 else 8  # so mon: 8 (4x2, 16:9), 12 (4x3, 4:3), 14 (5x3, 16:9)
cols,ar={8:(4,"16:9"),12:(4,"4:3"),14:(5,"16:9")}[n];rows=-(-n//cols)
def call(m,p,b=None):
    r=urllib.request.Request("https://api.meshy.ai/openapi/"+p,method=m,data=json.dumps(b).encode() if b else None,headers={"Content-Type":"application/json"})
    return json.load(urllib.request.urlopen(r))
prompt=(f"{n} separate low-poly stylized 3D game props laid out in a grid of {cols} columns and {rows} rows on a plain light grey studio floor, "
"each object standing on the floor with a soft contact shadow, very wide empty gaps between objects so none touch or overlap, "
"faceted flat-shaded low-poly style, muted neutral colors, three-quarter view from slightly above, even soft lighting, no text, no labels, no grid lines. "
"Objects, row by row from top-left to bottom-right: "+items)
t=call("POST","v1/text-to-image",{"ai_model":"nano-banana-pro","prompt":prompt,"aspect_ratio":ar})["result"]
while True:
    s=call("GET","v1/text-to-image/"+t)
    if s["status"] in("SUCCEEDED","FAILED","CANCELED"):break
    time.sleep(5)
print(s["status"],s.get("consumed_credits"))
urllib.request.urlretrieve(s["image_urls"][0],out)

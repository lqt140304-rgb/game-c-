import sys,json,base64,time,urllib.request
img,tag=sys.argv[1],sys.argv[2]
def call(m,p,b=None):
    r=urllib.request.Request("https://api.meshy.ai/openapi/"+p,method=m,data=json.dumps(b).encode() if b else None,headers={"Content-Type":"application/json"})
    return json.load(urllib.request.urlopen(r))
uri="data:image/png;base64,"+base64.b64encode(open(img,"rb").read()).decode()
t=call("POST","v1/image-to-3d",{"image_url":uri,"ai_model":"latest","should_remesh":True,"target_polycount":30000,"topology":"triangle","should_texture":True,"enable_pbr":False,"target_formats":["glb"]})["result"]
while True:
    s=call("GET","v1/image-to-3d/"+t)
    if s["status"] in("SUCCEEDED","FAILED","CANCELED"):break
    time.sleep(8)
print(tag,s["status"],s.get("consumed_credits"),s.get("task_error"),flush=True)
if s["status"]=="SUCCEEDED": urllib.request.urlretrieve(s["model_urls"]["glb"],f"{tag}_all.glb")

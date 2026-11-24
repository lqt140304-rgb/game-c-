const {chromium}=require('playwright');const http=require('http'),fs=require('fs'),path=require('path');
const ids=process.argv.slice(2);
const srv=http.createServer((q,s)=>{const u=q.url.split('?')[0];const p=path.join(__dirname,decodeURIComponent(u==='/'?'/index.html':u));
 fs.readFile(p,(e,b)=>{if(e){s.writeHead(404);return s.end()}s.writeHead(200,{'Content-Type':p.endsWith('.js')?'text/javascript':p.endsWith('.html')?'text/html':'application/octet-stream'});s.end(b)})}).listen(8766,async()=>{
 const br=await chromium.launch({args:['--use-gl=swiftshader','--enable-unsafe-swiftshader','--no-sandbox']});
 for(const id of ids){const pg=await br.newPage({viewport:{width:1200,height:300}});
  await pg.goto('http://localhost:8766/?id='+id);await pg.waitForFunction('window.done',null,{timeout:180000});
  await pg.screenshot({path:`ht_${id}.png`});await pg.close();}
 await br.close();srv.close();});

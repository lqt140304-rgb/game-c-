// Nhu shot.js nhung camera cao (nhin gan tu tren xuong): node shot_top.js <ten> [do cao, mac dinh 2.5]
const {chromium}=require('playwright');const http=require('http'),fs=require('fs'),path=require('path');
const id=process.argv[2],el=process.argv[3]||'2.5';
const srv=http.createServer((q,s)=>{const u=q.url.split('?')[0];const p=path.join(__dirname,decodeURIComponent(u==='/'?'/top.html':u));
 fs.readFile(p,(e,b)=>{if(e){s.writeHead(404);return s.end()}s.writeHead(200,{'Content-Type':p.endsWith('.js')?'text/javascript':p.endsWith('.html')?'text/html':'application/octet-stream'});s.end(b)})}).listen(8767,async()=>{
 const br=await chromium.launch({args:['--use-gl=swiftshader','--enable-unsafe-swiftshader','--no-sandbox']});
 const pg=await br.newPage({viewport:{width:1200,height:300}});
 await pg.goto(`http://localhost:8767/?id=${id}&el=${el}&zm=${process.argv[4]||1.7}`);await pg.waitForFunction('window.done',null,{timeout:180000});
 await pg.screenshot({path:`top_${id}.png`});await br.close();srv.close();});

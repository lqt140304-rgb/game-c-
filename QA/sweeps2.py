import proposal_sim as P
N=3000
NAMES=["thận trọng","thận trọng + giám định chọn lọc","liều + giám định chọn lọc","combo nhỏ (CB1)","combo nhỏ + giám định","combo lớn (CB1 + CB3)","combo lớn + giám định"]
def run(label, **kw):
    saved={k:getattr(P,k) for k in kw}
    for k,v in kw.items(): setattr(P,k,v)
    r=P.measure(P.C1,NAMES,n=N)
    print("%-52s"%label+" ".join("%6.1f"%r[n]["win"] for n in NAMES),flush=True)
    for k,v in saved.items(): setattr(P,k,v)
print("cột: TT | TT+gđ | liều+gđ | nhỏ | nhỏ+gđ | lớn | lớn+gđ")
for cp1,cb1 in ((.10,.20),(.075,.15),(.05,.10),(.05,.15),(.0,.10)):
    run(f"phí 15%: CP1 +{cp1*100:g}%, CB1 +{cb1*100:g}%",APPR_RATE=.15,CP1_B=cp1,CB1_B=cb1)
print("-- CB3 mạnh hơn (phí 15%, CP1 +5%, CB1 +10%)")
for kw in (dict(),dict(CB3_FREE=99),dict(CB3_PRICE=.10),dict(CB3_PRICE=.15),dict(CB3_FREE=99,CB3_PRICE=.10),dict(CB3_FREE=99,TC3_BONUS=.20,CB3_PRICE=.10)):
    run("CB3 "+", ".join(f"{k}={v}" for k,v in kw.items()),APPR_RATE=.15,CP1_B=.05,CB1_B=.10,**kw)

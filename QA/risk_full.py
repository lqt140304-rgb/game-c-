"""Đo đầy đủ cơ chế phạt rủi ro O-01 của Claude 1 (hàng giả theo hộp) trên cả chín chiến thuật.
Dùng risk_penalty.py (Claude 1) và proposal_sim.py. Chạy: python3 risk_full.py (~4 phút)."""
import proposal_sim as P
import risk_penalty as R
from sprint3_sim import ART, Content, ITEMS

PKG = dict(APPR_RATE=.15, CERT_BONUS=.25, CP1_B=.05, CB1_B=.10, CB3_PRICE=.15, CB3_FREE=99)
N = 4000
BASE = {"V01": 210, "V02": 350, "V03": 450, "V04": 375, "V05": 280, "V06": 1675, "V07": 2450, "V08": 1445, "V09": 815, "V10": 2600}
X110 = {"V01": 230, "V02": 385, "V03": 495, "V04": 415, "V05": 310, "V06": 1845, "V07": 2695, "V08": 1590, "V09": 895, "V10": 2860}


def content(prices):
    return Content(item_mult={k: prices[k] / ITEMS[k][1] for k in ITEMS})


def scaled(f):
    return {k: round(v * f / 5) * 5 for k, v in BASE.items()}


if __name__ == "__main__":
    P.Run5.buy_open = R.buy_open
    P.Run5.p_real = R.p_real
    for k, v in PKG.items():
        setattr(P, k, v)
    ART["CP3"], ART["TC3"] = ("CP", 500), ("TC", 500)
    variants = [
        ("Gói đã chốt (giả 15% mọi hộp, giá hệ số 0,85)", {}, BASE),
        ("G1 của Claude 1: giả 12/15/22/30%, giá x1,10", dict(H1=.12, H2=.15, H3=.22, RD=.30), X110),
        ("G2 nhẹ hơn: giả 12/15/20/25%, giá x1,08", dict(H1=.12, H2=.15, H3=.20, RD=.25), scaled(1.08)),
        ("G3 mạnh hơn: giả 10/15/25/35%, giá x1,15", dict(H1=.10, H2=.15, H3=.25, RD=.35), scaled(1.15)),
    ]
    out = {}
    for label, rates, prices in variants:
        R.FAKE_BY_BOX.clear(); R.FAKE_BY_BOX.update(rates)
        c = content(prices)
        ev = R.box_ev(c)
        res = P.measure(c, n=N)
        out[label] = res
        print("== %s\n   lần giá hộp H1/H2/H3/RD: %s" % (label, " / ".join("%.2f" % ev[b] for b in ("H1", "H2", "H3", "RD"))))
        P.show(res, None)
        a, b = res["liều + giám định chọn lọc"]["win"], res["thận trọng + giám định chọn lọc"]["win"]
        print("   chênh liều và thận trọng (cùng giám định): %+.1f   cao nhất %.1f   thấp nhất %.1f\n" % (a - b, max(r["win"] for r in res.values()), min(r["win"] for r in res.values())), flush=True)

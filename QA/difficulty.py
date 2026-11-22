"""Quét độ khó: tìm bộ giá nền cho ván khó và không có chiến thuật thống trị. Claude 2.
Chạy: python3 difficulty.py
"""
import itertools
import sprint3_sim as sm
from sprint3_sim import *

QM = sum(p * f for _, p, f in QUALITY)


def ev(content):
    out = {}
    for b, (price, w) in BOXES.items():
        tot = sum(w.values()); e = 0
        for i, x in w.items():
            fp = FAKE_P_V09 if i == "V09" else FAKE_P
            e += x / tot * content.items[i][1] * QM * (1 - fp + fp * FAKE_X)
        out[b] = e / content.box_price[b]
    return out


def mk(g1, g2, g3, g4, s):
    m = {}
    for i in ("V01", "V02", "V03"): m[i] = g1
    for i in ("V04", "V05", "V06"): m[i] = g2
    m["V09"], m["V07"], m["V08"], m["V10"] = g3, g4, s, s
    return Content(item_mult=m)


def solve(t):
    best = None
    for g1 in [x / 100 for x in range(15, 150, 1)]:
        e = ev(mk(g1, 1, 1, 1, 1))["H1"]
        if best is None or abs(e - t["H1"]) < best[0]: best = (abs(e - t["H1"]), g1)
    g1 = best[1]; best = None
    for g2 in [x / 100 for x in range(60, 400, 1)]:
        e = ev(mk(1, g2, 1, 1, 1))["H2"]
        if best is None or abs(e - t["H2"]) < best[0]: best = (abs(e - t["H2"]), g2)
    g2 = best[1]; best = None
    for s in [x / 20 for x in range(14, 80)]:
        for g3 in [x / 20 for x in range(8, 120)]:
            if 400 * g3 > 0.6 * 1000 * s: continue
            for g4 in [x / 20 for x in range(10, 70)]:
                e = ev(mk(1, 1, g3, g4, s))
                d = abs(e["H3"] - t["H3"]) + abs(e["RD"] - t["RD"])
                if best is None or d < best[0] - 1e-9: best = (d, s, g3, g4)
    return (g1, g2, best[2], best[3], best[1])


def wr(content, st, n):
    return round(100 * sum(play(s, content, st).g.status == "won" for s in range(n)) / n, 1)


if __name__ == "__main__":
    sm.ALL_ORIGIN_BONUS, sm.BONUS_SCALE, sm.ART_PRICE_SCALE = True, 2, 1
    n = 1500
    print("H1   H2   H3   RD   | thận trọng  săn combo  liều | lệch")
    for r1, r2, r3, rd in itertools.product((1.8, 2.2), (1.3, 1.5), (1.1, 1.25), (0.95, 1.1)):
        c = mk(*solve(dict(H1=r1, H2=r2, H3=r3, RD=rd)))
        a, b, d = wr(c, careful, n), wr(c, combo_hunter, n), wr(c, bold, n)
        print("%.2f %.2f %.2f %.2f | %8s %10s %8s | %4.1f" % (r1, r2, r3, rd, a, b, d, max(a, b, d) - min(a, b, d)))

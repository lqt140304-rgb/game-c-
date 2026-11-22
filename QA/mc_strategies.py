"""Mô phỏng chiến thuật trên mô hình có luật nợ tồn. Chạy: python3 mc_strategies.py"""
from economy_model import *


def careful(g):
    due = DEBT[g.day - 1] + g.arrears
    out, cash = [], g.cash
    for k in ("vua", "nho", "nho"):
        if cash - BOX_PRICE[k] >= due:
            out.append(k); cash -= BOX_PRICE[k]
    return out


def mid(g):
    due = DEBT[g.day - 1] + g.arrears
    out, cash = [], g.cash
    for k in ("lon", "vua", "nho"):
        if cash - BOX_PRICE[k] >= due:
            out.append(k); cash -= BOX_PRICE[k]
    return out


def bold(g):
    return ["lon", "vua", "nho"]


def run(plan, seed):
    g = Game(seed=seed); shortfall = False
    while g.status == "playing":
        for k in plan(g):
            try:
                b = g.buy_box(k)
            except Rejected:
                break
            g.sell_item(g.open_box(b))
        g.end_day()
        if g.arrears > 0:
            shortfall = True
    return g, shortfall


if __name__ == "__main__":
    N = 20000
    print("chiến thuật  thắng%  thua_vì_nợ>1000%  thua_ngày7%  từng_thiếu%  thắng|từng_thiếu%")
    for name, plan in (("thận trọng", careful), ("cân bằng", mid), ("liều", bold)):
        w = l1 = l7 = sf = sfw = 0
        for s in range(N):
            g, short = run(plan, s)
            w += g.status == "won"
            l1 += g.lose_reason == "arrears_over_limit"
            l7 += g.lose_reason == "arrears_on_day7"
            sf += short
            sfw += short and g.status == "won"
        f = lambda x: round(100 * x / N, 1)
        print(f"{name:12} {f(w):6} {f(l1):14} {f(l7):12} {f(sf):12} {round(100*sfw/max(sf,1),1):10}")

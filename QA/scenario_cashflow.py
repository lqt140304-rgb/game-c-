"""Dòng tiền mẫu 7 ngày, seed cố định. Chạy: python3 scenario_cashflow.py"""
from economy_model import *


def play(seed, buy_plan):
    g = Game(seed=seed)
    rows = []
    for d in range(1, 8):
        spent = got = 0
        for kind in buy_plan(g):
            try:
                b = g.buy_box(kind)
            except Rejected:
                break
            spent += BOX_PRICE[kind]
            got += g.sell_item(g.open_box(b))
        start = g.cash
        r = g.end_day()
        rows.append((d, spent, got, DEBT[d - 1], g.cash, r["result"]))
        if g.status != "playing":
            break
    return rows


def careful(g):
    due = DEBT[g.day - 1]
    plan, cash = [], g.cash
    for k in ("vua", "nho", "nho"):
        if cash - BOX_PRICE[k] >= due:
            plan.append(k); cash -= BOX_PRICE[k]
    return plan


def bold(g):
    return ["lon", "vua", "nho"]


if __name__ == "__main__":
    for name, plan in (("thận trọng", careful), ("liều", bold)):
        for seed in (1, 2, 3):
            print(f"[{name}] seed {seed}")
            print("ngày  chi    thu    nợ    tiền sau nợ  kết quả")
            for r in play(seed, plan):
                print("%3d %6d %6d %5d %9d  %s" % r)

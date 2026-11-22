"""So sánh phương án chỉnh luật/số. Chạy: python3 variants.py"""
import economy_model as m
from mc_strategies import careful, mid, bold, run

N = 6000
BASE_LOOT = {k: list(v) for k, v in m.LOOT.items()}


def apply(keep=0, cash=1000, mult=1.0, limit=1000):
    m.KEEP_MIN_CASH = keep
    m.LOSE_ARREARS_OVER = limit
    m.LOOT.update({k: [(p, round(v * mult)) for p, v in t] for k, t in BASE_LOOT.items()})
    m.START_CASH = cash


def win(plan, cash):
    w = 0
    for s in range(N):
        g = m.Game(seed=s, cash=cash)
        while g.status == "playing":
            for k in plan(g):
                try:
                    b = g.buy_box(k)
                except m.Rejected:
                    break
                g.sell_item(g.open_box(b))
            g.end_day()
        w += g.status == "won"
    return round(100 * w / N, 1)


VAR = [
    ("A. Luật Claude 1 nguyên bản", dict()),
    ("B. Chừa lại 100 xu", dict(keep=100)),
    ("C. Vốn 1.500 xu", dict(cash=1500)),
    ("D. Giá bán x1,3", dict(mult=1.3)),
    ("E. Chừa 100 + giá bán x1,3", dict(keep=100, mult=1.3)),
    ("F. Chừa 100 + vốn 1.500", dict(keep=100, cash=1500)),
    ("G. Chừa 100 + vốn 1.500 + x1,3", dict(keep=100, cash=1500, mult=1.3)),
]
print("%-34s %9s %9s %9s" % ("phương án", "thận trọng", "cân bằng", "liều"))
for name, kw in VAR:
    apply(**dict(dict(keep=0, cash=1000, mult=1.0, limit=1000), **kw))
    c = kw.get("cash", 1000)
    print("%-34s %9s %9s %9s" % (name, win(careful, c), win(mid, c), win(bold, c)))

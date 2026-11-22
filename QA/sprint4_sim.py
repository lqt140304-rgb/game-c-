"""Sprint 4: ảnh hưởng của dịch vụ lên thanh khoản, nợ và tiến trình (Claude 2).

Dựa trên mô phỏng Sprint 3 (giá nền đề xuất, CP1/CB1 áp dụng mọi hiện vật x2). Mọi luật dịch vụ là
GIẢ ĐỊNH tham số hóa vì Claude 1 chưa chốt. Chạy: python3 sprint4_sim.py (khoảng 3 phút).
"""
import random
import sprint3_sim as sm
from sprint3_sim import *
from sprint3_report import PROPOSED
from economy_model import DEBT

N = 4000
ART_SELL_PRICE_RATIO = 0.5


class Cfg:
    def __init__(self, **kw):
        self.auction = None     # dict(prem, spread, fee, min_value)
        self.casino = None      # dict(p, max_bet, bets)
        self.pawn = None        # dict(ratio, fee, days)
        self.event = 0.0        # biên độ +- giá bán, xuất hiện 50% số ngày
        self.meta_cash = 0
        self.__dict__.update(kw)


class Run4(Run):
    def __init__(self, seed, content, strategy, cfg):
        super().__init__(seed, content, strategy)
        self.cfg = cfg
        self.pending = []       # (ngày nhận, số tiền)
        self.tickets = []       # vé cầm tạo tác: dict(aid, loan, cost, due)
        self.g.cash += cfg.meta_cash
        self.stats = dict(auction=0, bets=0, bet_wins=0, pawned=0, redeemed=0, forfeited=0)

    def ev_mult(self):
        if not self.cfg.event:
            return 1.0
        r = random.Random(f"{self.rng.seed if hasattr(self.rng,'seed') else 0}-{self.g.seed}-ev-{self.g.day}")
        if r.random() >= .5:
            return 1.0
        return 1 + self.cfg.event if r.random() < .5 else 1 - self.cfg.event

    def sell(self, it):
        origin, base = self.c.items[it["id"]]
        before = self.g.cash
        Run.sell(self, it)
        got = self.g.cash - before
        got = round(got * self.ev_mult())
        a = self.cfg.auction
        known = base * it["q"]                      # người chơi chưa biết hàng giả
        if a and self.g.day < len(DEBT) and known >= a["min_value"]:
            u = (self.rng.random() * 2 - 1) * a["spread"]
            pay = round(got * (1 + a["prem"] + u) * (1 - a["fee"]))
            self.pending.append((self.g.day + 1, pay))
            self.stats["auction"] += 1
            self.g.cash = before
        else:
            self.g.cash = before + got


def casino_recover(run):
    c = run.cfg.casino
    if not c:
        return
    g = run.g
    need = DEBT[g.day - 1] + g.arrears
    for _ in range(c["bets"]):
        if g.cash >= need or g.cash <= 0:
            return
        stake = min(g.cash, c["max_bet"], need - g.cash if need - g.cash > 0 else g.cash)
        stake = max(1, min(stake, g.cash))
        run.stats["bets"] += 1
        if run.rng.random() < c["p"]:
            g.cash += stake
            run.stats["bet_wins"] += 1
        else:
            g.cash -= stake


def pawn_hook(run):
    p = run.cfg.pawn
    if not p:
        return
    g = run.g
    # chuộc trước nếu đủ tiền sau khi chừa kỳ nợ
    for t in list(run.tickets):
        if g.day <= t["due"] and g.cash - t["cost"] >= DEBT[g.day - 1] + g.arrears:
            g.cash -= t["cost"]
            run.owned.append(t["aid"]); run.equipped.append(t["aid"]); run.tickets.remove(t)
            run.stats["redeemed"] += 1
            run.refresh()
    need = DEBT[g.day - 1] + g.arrears
    for aid in sorted(run.equipped, key=lambda a: -ART[a][1]):
        if g.cash >= need:
            break
        loan = round(ART[aid][1] * p["ratio"])
        g.cash += loan
        run.equipped.remove(aid); run.owned.remove(aid)
        run.tickets.append(dict(aid=aid, loan=loan, cost=round(loan * (1 + p["fee"])), due=g.day + p["days"]))
        run.stats["pawned"] += 1
    for t in list(run.tickets):
        if g.day > t["due"]:
            run.tickets.remove(t); run.stats["forfeited"] += 1


def play4(seed, content, strategy, cfg):
    run = Run4(seed, content, strategy, cfg)
    g = run.g
    while g.status == "playing":
        strategy(run)
        run.refresh()
        pawn_hook(run)
        casino_recover(run)
        g.end_day()
        if g.arrears > 0:
            run.shortfall_days += 1
        if g.status == "playing":
            for d, amt in list(run.pending):
                if d <= g.day:
                    g.cash += amt
                    run.pending.remove((d, amt))
    return run


def rates(cfg, n=N):
    out = {}
    for name, st in STRATS.items():
        w = sf = 0
        for s in range(n):
            r = play4(s, PROPOSED, st, cfg)
            w += r.g.status == "won"
            sf += r.shortfall_days
        out[name] = (round(100 * w / n, 1), round(sf / n, 2))
    return out


def row(label, cfg, base=None):
    r = rates(cfg)
    cells = []
    for name in STRATS:
        w, sf = r[name]
        d = "" if base is None else " (%+.1f)" % (w - base[name][0])
        cells.append("%5.1f%%%s, hụt %.2f" % (w, d, sf))
    print("%-46s %s" % (label, " | ".join(cells)))
    return r


if __name__ == "__main__":
    sm.ALL_ORIGIN_BONUS, sm.BONUS_SCALE, sm.ART_PRICE_SCALE = True, 2, 1
    print("%-46s %s" % ("(thắng% và số ngày hụt)", " | ".join(STRATS)))
    base = row("Cơ sở (không dịch vụ)", Cfg())

    print("\n== Đấu giá (nhận tiền ngày sau, đăng món từ 400 xu)")
    for prem, fee in ((0, .10), (.25, .10), (.25, .20), (.5, .10)):
        row(f"+{int(prem*100)}% kỳ vọng, phí {int(fee*100)}%", Cfg(auction=dict(prem=prem, spread=.5, fee=fee, min_value=400)), base)

    print("\n== Đấu giá nhắm món đắt (chỉ đăng món từ 1.500 xu, phí 10%)")
    for prem in (.25, .5, .75):
        row(f"+{int(prem*100)}% kỳ vọng, món từ 1.500", Cfg(auction=dict(prem=prem, spread=.5, fee=.10, min_value=1500)), base)

    print("\n== Sòng bạc (gỡ lại khi thiếu tiền, cược 1 ăn 1)")
    for p, mb in ((.40, 500), (.45, 500), (.50, 500), (.45, 200), (.45, 1000)):
        row(f"thắng {int(p*100)}%, cược tối đa {mb}", Cfg(casino=dict(p=p, max_bet=mb, bets=2)), base)

    print("\n== Cầm cố tạo tác (chỉ chiến thuật săn combo có tạo tác)")
    for ratio, fee in ((.5, .15), (.7, .15), (.7, .30)):
        row(f"vay {int(ratio*100)}%, phí chuộc {int(fee*100)}%, hạn 2 ngày", Cfg(pawn=dict(ratio=ratio, fee=fee, days=2)), base)

    print("\n== Biến cố (giá bán +- trong 50% số ngày)")
    for e in (.1, .2, .3, .5):
        row(f"biên độ {int(e*100)}%", Cfg(event=e), base)

    print("\n== Mở khóa meta: thêm vốn đầu ván")
    for m in (100, 200, 300, 500):
        row(f"+{m} xu vốn", Cfg(meta_cash=m), base)

"""Sprint 4: ảnh hưởng của dịch vụ theo ĐÚNG luật Claude 1 (Docs/Content/DichVuVaMeta.md, d6b597e).

Cơ sở: cấu hình ván khó do Claude 2 đề xuất (QA/profiles.py), chưa được chủ dự án duyệt. Hiệu ứng giá bán cộng theo phép cộng như quy tắc mới của Claude 1.
Chạy: python3 sprint4_sim.py   (khoảng 4 phút)
"""
import math
import random
import sprint3_sim as sm
from sprint3_sim import *
from profiles import HARD as PROPOSED, use_hard
from economy_model import DEBT

N = 4000
RARE = {"V07", "V10"}          # GIẢ ĐỊNH: "Rất hiếm" là V07, V10
EVENTS = ["E1", "E2", "E3", "E4", "E5", "E6"]
K_DIST = [(0.2, 0.9), (0.4, 1.1), (0.3, 1.3), (0.1, 1.5)]


class Cfg:
    def __init__(self, **kw):
        self.events = False
        self.auction = None       # dict(min_g, floor)
        self.pawn_art = False
        self.hold = None          # dict(min_g, mode='sell'|'pawn')
        self.casino = None        # dict(p, stakes)
        self.meta_cash = 0
        self.__dict__.update(kw)


class Run4(Run):
    def __init__(self, seed, content, strategy, cfg):
        super().__init__(seed, content, strategy)
        self.cfg, self.seed = cfg, seed
        self.pending, self.held, self.tickets, self.tickets_art = [], [], [], []
        self.auctions_today = 0
        self.g.cash += cfg.meta_cash
        self.stats = dict(auction=0, returned=0, bets=0, wins=0, pawned=0, redeemed=0, forfeited=0)

    def event(self):
        d = self.g.day
        if not self.cfg.events or d < 2:
            return None
        r = random.Random(f"{self.seed}-ev-{d}")
        if r.random() >= .5:
            return None
        return EVENTS[r.randrange(6)]

    def buy_open(self, box):
        if box == "H2" and self.event() == "E3":
            old = self.c.box_price["H2"]
            self.c.box_price["H2"] = round(old * .8)
            try:
                return Run.buy_open(self, box)
            finally:
                self.c.box_price["H2"] = old
        return Run.buy_open(self, box)

    def appraise(self, it):
        return False if self.event() == "E4" else Run.appraise(self, it)

    def restore(self, it):
        return False if self.event() == "E5" else Run.restore(self, it)

    def G(self, it):
        origin, base = self.c.items[it["id"]]
        p0 = base * it["q"] * (FAKE_X if it["fake"] else 1)
        b = 0.0
        if origin == QK or sm.ALL_ORIGIN_BONUS:
            if self.has("CP1"): b += .05 * sm.BONUS_SCALE
            if self.has("CP1", "CP2"): b += .10 * sm.BONUS_SCALE
        if origin == TT and self.has("CP3"):
            b += .10 * sm.BONUS_SCALE
        e = self.event()
        if e == "E1" and origin == QK: b += .15
        if e == "E2" and origin in (HD, TL): b -= .15
        if e == "E6" and it["id"] in RARE: b += .10
        return math.floor(p0 * (1 + b))

    def sell(self, it):
        g, cfg = self.g, self.cfg
        gval = self.G(it)
        known = self.c.items[it["id"]][1] * it["q"]
        h = cfg.hold
        if h and len(self.held) < 1 and gval >= h["min_g"] and g.day < len(DEBT):
            self.held.append(it)
            return
        a = cfg.auction
        if a and self.auctions_today < 1 and g.day < len(DEBT) and known >= a["min_g"]:
            fee = math.ceil(gval * .05)
            if g.cash >= fee:
                g.cash -= fee
                x, acc, k = self.rng.random(), 0.0, 1.5
                for p, v in K_DIST:
                    acc += p
                    if x <= acc:
                        k = v
                        break
                self.pending.append(dict(due=g.day + 1, G=gval, k=k, floor=a["floor"], it=it))
                self.auctions_today += 1
                self.stats["auction"] += 1
                return
        g.cash += gval


def end_of_day(run):
    g, cfg = run.g, run.cfg
    if cfg.pawn_art:
        pawn_art_hook_end(run)
    need = DEBT[g.day - 1] + g.arrears
    c = cfg.casino
    h = cfg.hold
    if h and run.held and g.cash < need:
        for it in list(run.held):
            if g.cash >= need:
                break
            gval = run.G(it)
            if h["mode"] == "sell":
                g.cash += gval
            else:
                loan = math.floor(gval * .6)
                if len(run.tickets) >= 2:
                    continue
                g.cash += loan
                run.tickets.append(dict(it=it, cost=math.ceil(loan * 1.15), due=g.day + 2))
                run.stats["pawned"] += 1
            run.held.remove(it)
    if c:
        for _ in range(3):
            if g.day < 2 or g.cash >= need or g.cash < min(c["stakes"]):
                break
            stake = max([s for s in c["stakes"] if s <= g.cash] or [0])
            if stake == 0:
                break
            run.stats["bets"] += 1
            if run.rng.random() < c["p"]:
                g.cash += stake; run.stats["wins"] += 1
            else:
                g.cash -= stake


def pawn_art_hook_end(run):
    g = run.g
    need = DEBT[g.day - 1] + g.arrears
    for aid in sorted(run.equipped, key=lambda a: -ART[a][1]):
        if g.cash >= need or len(run.tickets_art) >= 2:
            break
        loan = math.floor(ART[aid][1] * .6)
        g.cash += loan
        run.equipped.remove(aid); run.owned.remove(aid)
        run.tickets_art.append(dict(aid=aid, cost=math.ceil(loan * 1.15), due=g.day + 2))
        run.stats["pawned"] += 1


def pawn_art_hook_start(run):
    g = run.g
    for t in list(run.tickets_art):
        if g.day > t["due"]:
            run.tickets_art.remove(t); run.stats["forfeited"] += 1
        elif g.cash - t["cost"] >= DEBT[g.day - 1] + g.arrears and len(run.equipped) < run.slots():
            g.cash -= t["cost"]
            run.owned.append(t["aid"]); run.equipped.append(t["aid"]); run.tickets_art.remove(t)
            run.stats["redeemed"] += 1
            run.refresh()


def start_of_day(run):
    g = run.g
    run.auctions_today = 0
    if run.cfg.pawn_art:
        pawn_art_hook_start(run)
    for l in list(run.pending):
        if l["due"] <= g.day:
            run.pending.remove(l)
            price = round(l["G"] * l["k"])
            if l["floor"] is None or price >= round(l["G"] * l["floor"]):
                g.cash += price
            else:
                run.stats["returned"] += 1
                g.cash += run.G(l["it"])       # trả về rồi bán thường
    for t in list(run.tickets):
        if g.day > t["due"]:
            run.tickets.remove(t); run.stats["forfeited"] += 1
        elif g.cash >= t["cost"]:
            g.cash -= t["cost"]
            g.cash += run.G(t["it"])           # chuộc rồi bán
            run.tickets.remove(t); run.stats["redeemed"] += 1
    for it in list(run.held):                   # đệm không dùng thì bán sáng hôm sau
        g.cash += run.G(it); run.held.remove(it)


def play4(seed, content, strategy, cfg):
    run = Run4(seed, content, strategy, cfg)
    g = run.g
    while g.status == "playing":
        start_of_day(run)
        strategy(run)
        run.refresh()
        end_of_day(run)
        g.end_day()
        if g.arrears > 0:
            run.shortfall_days += 1
    return run


def rates(cfg, n=N):
    out = {}
    for name, st in STRATS.items():
        w = sf = 0
        for s in range(n):
            r = play4(s, PROPOSED, st, cfg)
            w += r.g.status == "won"; sf += r.shortfall_days
        out[name] = (round(100 * w / n, 1), round(sf / n, 2))
    return out


def row(label, cfg, base=None):
    r = rates(cfg)
    cells = []
    for name in STRATS:
        w, sf = r[name]
        d = "" if base is None else " (%+.1f)" % (w - base[name][0])
        cells.append("%5.1f%%%s, hụt %.2f" % (w, d, sf))
    print("%-44s %s" % (label, " | ".join(cells)))
    return r


if __name__ == "__main__":
    use_hard()
    print("%-44s %s" % ("(thắng%, ngày hụt TB)", " | ".join(STRATS)))
    base = row("Cơ sở (không dịch vụ)", Cfg())

    print("\n== Biến cố E1-E6 (50% số ngày từ ngày 2)")
    row("Có biến cố", Cfg(events=True), base)

    print("\n== Đấu giá (1 lô/ngày, phí 5%, k kỳ vọng 1,16, tiền về đầu ngày sau)")
    for t in (400, 1000, 1500):
        for f in (None, 1.0, 1.3):
            row(f"món đầu tiên từ {t} xu, sàn {f if f else 'không'}", Cfg(auction=dict(min_g=t, floor=f)), base)

    print("\n== Đấu giá: tăng kỳ vọng k (món từ 1.000 xu, sàn 1,0)")
    orig = list(K_DIST)
    for scale in (1.0, 1.15, 1.3):
        K_DIST[:] = [(p, round(k * scale, 3)) for p, k in orig]
        e = sum(p * k for p, k in K_DIST)
        row(f"k kỳ vọng {e:.2f}", Cfg(auction=dict(min_g=1000, floor=1.0)), base)
    K_DIST[:] = orig

    print("\n== Sòng bạc (gỡ lại khi thiếu tiền, cược 50/100/200, 3 lượt)")
    for p in (.45, .50, .55):
        row(f"xác suất thắng {int(p*100)}%", Cfg(casino=dict(p=p, stakes=(50, 100, 200))), base)
    row("cược tối đa 500 (khác đề xuất)", Cfg(casino=dict(p=.45, stakes=(50, 100, 200, 500))), base)

    print("\n== Cầm cố so với bán (giữ 1 món từ 600 xu làm đệm mỗi ngày)")
    row("giữ đệm, thiếu thì BÁN", Cfg(hold=dict(min_g=600, mode="sell")), base)
    row("giữ đệm, thiếu thì CẦM 60%", Cfg(hold=dict(min_g=600, mode="pawn")), base)

    print("\n== Cầm cố tạo tác (60% giá mua, phí chuộc 15%, hạn 2 ngày)")
    row("cầm tạo tác khi thiếu tiền", Cfg(pawn_art=True), base)

    print("\n== Meta")
    row("M2 +100 xu vốn", Cfg(meta_cash=100), base)
    sm.RD_FROM_DAY = 1
    row("M3 Rương Đen từ ngày 1", Cfg(), base)
    sm.RD_FROM_DAY = 4

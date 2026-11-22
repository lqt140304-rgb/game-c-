"""Đo các thay đổi Claude 1 đề xuất trong Docs/Content/DieuChinhSauCanBang.md (commit 6dc386a,
nhánh claude/relaxed-cerf-wgnu7l): TC3 mới, CB3 mới, GT1 mới (hoãn nợ), chứng nhận giám định,
mốc cộng hưởng mới, giá nền hệ số 0,85, đấu giá/cầm tạo tác theo DichVuVaMeta.md bản mới.

Chạy: python3 proposal_sim.py   (khoảng 5 phút)
GIẢ ĐỊNH của Claude 2 (luật chưa nói rõ):
- GT1: chỉ dùng khi sắp thiếu tiền và chưa dùng, hoãn kỳ hôm nay, kỳ hoãn thành khoản 'carry' (kỳ x 1,15),
  trả cùng kỳ ngày sau, không bị phạt 20%, không tính vào ngưỡng thua 1.000.
- Chứng nhận: hiện vật thật đã giám định +25% giá bán (cộng cùng các hiệu ứng giá bán khác).
- Phục chế nâng một bậc chất lượng, phí 25% giá nền.
- Giám định chọn lọc: giám định khi 0,25 x giá x chất lượng x xác suất thật > phí.
"""
import math
import random
import economy_model as em
import sprint3_sim as sm
from sprint3_sim import (Run, Content, ITEMS, BOXES, FAKE_P, FAKE_P_V09, FAKE_X, ART, DEBT,
                         QK, HD, TL, NHT, CT, TT)
from economy_model import Game

N = 4000
APPR_RATE, CERT_BONUS, TC3_BONUS, CB3_FREE = .10, .25, .10, 1   # tham số quét
CP1_B, CB1_B, CB3_PRICE = .10, .20, 0.0   # CP1, CB1 và thưởng giá bán của CB3 (tham số quét)
GT1_DAY = None            # None: chỉ dùng khi sắp thiếu tiền; số: dùng ở ngày đó
C1_PRICES = {"V01": 210, "V02": 350, "V03": 450, "V04": 375, "V05": 280,
             "V06": 1675, "V07": 2450, "V08": 1445, "V09": 815, "V10": 2600}
C1 = Content(item_mult={k: C1_PRICES[k] / ITEMS[k][1] for k in ITEMS})
LEVELS = [.3, .7, 1.0]
K_NEW = [(.15, .8), (.35, 1.2), (.35, 1.6), (.15, 2.0)]
K_BASE = list(K_NEW)


class S5(Game):
    run = None
    carry = 0

    def penalty_rate(self):
        return .20

    def end_day(self):
        self._check_playing()
        run = self.run
        D = DEBT[self.day - 1]
        post, run.postpone_now = run.postpone_now, False
        due = (0 if post else D) + self.arrears + self.carry
        self.carry = 0
        pay = min(self.cash, due)
        self.cash -= pay
        self.arrears = due - pay
        if self.arrears > 0:
            self.arrears += math.ceil(self.arrears * .2)
        if post:
            self.carry = math.ceil(D * 1.15)
        if self.arrears > em.LOSE_ARREARS_OVER:
            self.status, self.lose_reason = "lost", "arrears_over_limit"
            return
        if self.day == len(DEBT):
            self.status = "won" if self.arrears == 0 and self.carry == 0 else "lost"
            return
        self.day += 1
        self.bought_today = 0


class Run5(Run):
    def __init__(self, seed, content, strategy, gt=None, opt=None):
        super().__init__(seed, content, strategy, gt)
        self.g = S5(seed=seed)
        self.g.run = self
        self.opt = opt or {}
        self.postpone_now = False
        self.gt1_used = False
        self.free_cnt = {}
        self.apprs = 0
        self.free_apprs = 0
        self.pending, self.tickets = [], []
        self.auctions_today = 0
        self.stats = dict(auction=0, pawned=0, redeemed=0, bets=0)

    # --- tiện ích ---
    def need(self):
        g = self.g
        return DEBT[g.day - 1] + g.arrears + g.carry

    def cb3(self):
        return self.has("CP3", "TC3")

    def price_bonus(self, it):
        origin = self.c.items[it["id"]][0]
        b = 0.0
        if self.has("CP1"): b += CP1_B
        if self.has("CP1", "CP2"): b += CB1_B
        if self.cb3(): b += CB3_PRICE
        if origin == TT and self.has("CP3"): b += .10
        if origin in (HD, TL, NHT) and self.has("TC3"): b += TC3_BONUS
        ncp, ntc = self.n_he("CP"), self.n_he("TC")
        if ncp == 2 and origin in (QK, TT): b += .05
        if ntc == 2 and origin in (HD, TL): b += .05
        if ntc >= 3 and it.get("restored"): b += .10
        if it.get("cert") and not it["fake"]: b += CERT_BONUS
        return b

    def price(self, it):
        base = self.c.items[it["id"]][1]
        p0 = base * it["q"] * (FAKE_X if it["fake"] else 1)
        return math.floor(p0 * (1 + self.price_bonus(it)))

    def known_price(self, it):
        base = self.c.items[it["id"]][1]
        return math.floor(base * it["q"] * (1 + self.price_bonus(it)))

    def p_real(self, it):
        return 1 - (FAKE_P_V09 if it["id"] == "V09" else FAKE_P)

    def appraise_fee(self, base):
        fee = max(20, round(base * APPR_RATE))
        disc = .5 if self.bk2 else (.10 if self.has("CP2") else 0)
        return round(fee * (1 - disc))

    def restore_fee(self, base):
        return round(base * .25 * (.9 if self.has("TC1") else 1))

    # --- thao tác ---
    def appraise(self, it):
        base = self.c.items[it["id"]][1]
        free = self.cb3() and self.free_cnt.get(self.g.day, 0) < CB3_FREE
        if free:
            self.free_cnt[self.g.day] = self.free_cnt.get(self.g.day, 0) + 1
            self.free_apprs += 1
        else:
            fee = self.appraise_fee(base)
            if self.g.cash < fee:
                return False
            self.g.cash -= fee
        self.apprs += 1
        it["appraised"] = True
        it["cert"] = not it["fake"]
        if self.c.items[it["id"]][0] == TT:
            self.bk2_count += 1
            if self.bk2_count >= 2:
                self.bk2 = True
        return True

    def restore(self, it):
        base = self.c.items[it["id"]][1]
        fee = self.restore_fee(base)
        if it["q"] >= 1.0 or self.g.cash < fee:
            return False
        self.g.cash -= fee
        it["q"] = LEVELS[LEVELS.index(it["q"]) + 1]
        it["ql"] = "Nguyên" if it["q"] >= 1 else "Xước"
        it["restored"] = True
        return True

    def sell(self, it):
        g, a = self.g, self.opt.get("auction")
        if a and self.auctions_today < 1 and g.day < len(DEBT):
            gk = self.known_price(it)
            if gk >= a["min_g"]:
                fee = math.ceil(gk * .05)
                if g.cash >= fee:
                    g.cash -= fee
                    x, acc, k = self.rng.random(), 0.0, K_NEW[-1][1]
                    for p, v in K_NEW:
                        acc += p
                        if x <= acc:
                            k = v
                            break
                    self.pending.append(dict(due=g.day + 1, G=self.price(it), k=k, floor=a["floor"], it=it))
                    self.auctions_today += 1
                    self.stats["auction"] += 1
                    return
        g.cash += self.price(it)


def reserve5(run):
    return run.need()


def handle5(run, it, r, smart):
    g = run.g
    base = run.c.items[it["id"]][1]
    pr = run.p_real(it)
    avg = pr + (1 - pr) * FAKE_X
    if it["q"] < 1.0:
        nq = LEVELS[LEVELS.index(it["q"]) + 1]
        fee = run.restore_fee(base)
        if (nq - it["q"]) * base * avg > fee and g.cash - fee >= r:
            run.restore(it)
    if smart:
        free = run.cb3() and run.free_cnt.get(g.day, 0) < CB3_FREE
        fee = 0 if free else run.appraise_fee(base)
        gain = CERT_BONUS * base * it["q"] * pr - fee
        if gain > 0 and g.cash - fee >= r:
            run.appraise(it)
    run.sell(it)


def ladder5(run, keep_reserve, smart):
    g = run.g
    for _ in range(3):
        r = reserve5(run) if keep_reserve else 0
        if g.cash < r:
            r = 0
        for box in (("RD",) if g.day >= sm.RD_FROM_DAY else ()) + ("H3", "H2", "H1"):
            if g.cash - run.c.box_price[box] >= r:
                it = run.buy_open(box)
                if it:
                    handle5(run, it, r, smart)
                break


def make_strategy(kind, smart=False):
    plans = {"none": [], "small": ["CP1", "CP2"], "tc3": ["CP1", "CP2", "TC3"], "big": ["CP1", "CP2", "TC3", "CP3"]}

    def strat(run):
        g = run.g
        if kind == "bold":
            return ladder5(run, False, smart)
        if g.day <= 5:
            for aid in plans[kind]:
                if aid in run.owned:
                    continue
                price = ART[aid][1]
                if g.cash - price >= reserve5(run) + 100:
                    run.buy_artifact(aid)
                break
        ladder5(run, True, smart)
    return strat


STRATS = {
    "thận trọng": make_strategy("none"),
    "thận trọng + giám định chọn lọc": make_strategy("none", True),
    "liều": make_strategy("bold"),
    "liều + giám định chọn lọc": make_strategy("bold", True),
    "combo nhỏ (CB1)": make_strategy("small"),
    "combo nhỏ + giám định": make_strategy("small", True),
    "CB1 + TC3 (không CB3)": make_strategy("tc3"),
    "combo lớn (CB1 + CB3)": make_strategy("big"),
    "combo lớn + giám định": make_strategy("big", True),
}


def end_hooks(run):
    g, o = run.g, run.opt
    need = run.need()
    if o.get("pawn_art"):
        for aid in sorted(run.equipped, key=lambda a: -ART[a][1]):
            if g.cash >= need or len(run.tickets) >= 2:
                break
            loan = math.floor(ART[aid][1] * .6)
            g.cash += loan
            run.equipped.remove(aid); run.owned.remove(aid)
            run.tickets.append(dict(aid=aid, cost=math.ceil(loan * 1.15), due=g.day + 2))
            run.stats["pawned"] += 1
    if run.gt == "GT1" and not run.gt1_used and g.day < len(DEBT) and ((GT1_DAY is None and g.cash < need) or g.day == GT1_DAY):
        run.gt1_used = True
        run.postpone_now = True


def start_hooks(run):
    g = run.g
    run.auctions_today = 0
    for l in list(run.pending):
        if l["due"] <= g.day:
            run.pending.remove(l)
            price = round(l["G"] * l["k"])
            g.cash += price if (l["floor"] is None or price >= round(l["G"] * l["floor"])) else run.price(l["it"])
    for t in list(run.tickets):
        if g.day > t["due"]:
            run.tickets.remove(t)
        elif g.cash - t["cost"] >= run.need() and len(run.equipped) < run.slots():
            g.cash -= t["cost"]
            run.owned.append(t["aid"]); run.equipped.append(t["aid"]); run.tickets.remove(t)
            run.stats["redeemed"] += 1
            run.refresh()


def play5(seed, content, strat, gt=None, opt=None):
    run = Run5(seed, content, strat, gt, opt)
    g = run.g
    while g.status == "playing":
        start_hooks(run)
        strat(run)
        run.refresh()
        end_hooks(run)
        g.end_day()
        if g.arrears > 0 or g.carry > 0:
            run.shortfall_days += 1
    return run


def measure(content, names=None, gt=None, opt=None, n=N):
    rows = {}
    for name in (names or STRATS):
        st = STRATS[name]
        w = sf = cb1 = cb3 = bk1 = g1 = apr = free = 0
        for s in range(n):
            r = play5(s, content, st, gt, opt)
            w += r.g.status == "won"; sf += r.shortfall_days
            cb1 += "CB1" in r.seen; cb3 += "CB3" in r.seen; bk1 += r.bk1
            g1 += r.gt1_used; apr += r.apprs; free += r.free_apprs
        rows[name] = dict(win=round(100 * w / n, 1), sf=round(sf / n, 2), cb1=round(100 * cb1 / n),
                          cb3=round(100 * cb3 / n), bk1=round(100 * bk1 / n), gt1=round(100 * g1 / n),
                          apr=round(apr / n, 2), free=round(free / n, 2))
    return rows


def show(rows, base=None):
    print("%-34s %6s %6s %6s %6s %6s %6s %6s" % ("chiến thuật", "thắng%", "hụt", "CB1%", "CB3%", "BK1%", "giámđịnh", "miễnphí"))
    for n, r in rows.items():
        d = "" if base is None or n not in base else " (%+.1f)" % (r["win"] - base[n]["win"])
        print("%-34s %6s %6s %6s %6s %6s %6s %6s%s" % (n, r["win"], r["sf"], r["cb1"], r["cb3"], r["bk1"], r["apr"], r["free"], d))


if __name__ == "__main__":
    from profiles import HARD
    print("== A. Giá nền Claude 1 (hệ số 0,85), luật mới (TC3, CB3, GT1, chứng nhận)")
    base = measure(C1); show(base)
    print("\n== B. Giá nền ván khó của Claude 2 (DoKho.md), cùng luật mới")
    show(measure(HARD))
    print("\n== C. GT1 so với GT2 (giá Claude 1)")
    for gt in ("GT1", "GT2"):
        print(gt); show(measure(C1, ["thận trọng", "liều", "combo nhỏ (CB1)", "combo lớn (CB1 + CB3)"], gt=gt))
    print("\n== D. Dịch vụ theo DichVuVaMeta.md bản mới (giá Claude 1)")
    print("-- đấu giá k 1,40, món từ 1.300 xu, sàn 1,2 G")
    show(measure(C1, ["thận trọng", "liều", "combo nhỏ + giám định"], opt=dict(auction=dict(min_g=1300, floor=1.2))), base)
    print("-- đấu giá không sàn")
    show(measure(C1, ["thận trọng", "liều", "combo nhỏ + giám định"], opt=dict(auction=dict(min_g=1300, floor=None))), base)
    print("-- cầm tạo tác")
    show(measure(C1, ["combo nhỏ + giám định", "combo lớn + giám định"], opt=dict(pawn_art=True)), base)

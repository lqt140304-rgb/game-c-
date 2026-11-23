"""Ảnh hưởng của biến cố (nhất là E3 Thuyền Hàng Ngoài Giới) lên việc chọn loại hộp (Claude 2).

Luật biến cố theo DichVuVaMeta.md: từ ngày 2, 50% số ngày có 1 biến cố chọn đều trong E1..E6,
sổ công bố TRƯỚC khi mua hộp. E1 Quá khứ +15%, E2 Hiện đại và Tương lai −15%, E3 giá H2 −20%,
E4 không giám định, E5 không phục chế, E6 hiện vật Rất hiếm (V07, V10) +10%.
Người chơi 'thích nghi' tính biến cố vào giá trị kỳ vọng khi chọn hộp; 'ngây thơ' thì không.
Chạy: python3 event_sim.py (khoảng 10 phút)
"""
import random
import clue_sim as C
import proposal_sim as P
import sprint3_sim as sm
from sprint3_sim import QK, HD, TL, TT

ALL = ["E1", "E2", "E3", "E4", "E5", "E6"]
RARE = {"V07", "V10"}
CFG = dict(on=False, adaptive=False, allowed=ALL)
STATS = {}


def event_today(self):
    if not CFG["on"] or self.g.day < 2:
        return None
    r = random.Random(f"{self.g.seed}-ev-{self.g.day}")
    if r.random() >= .5:
        return None
    e = ALL[r.randrange(6)]
    return e if e in CFG["allowed"] else None


def event_bonus(self, iid):
    origin = self.c.items[iid][0]
    e = self.event_today()
    if e == "E1" and origin == QK: return .15
    if e == "E2" and origin in (HD, TL): return -.15
    if e == "E6" and iid in RARE: return .10
    return 0.0


_orig_bonus = P.Run5.price_bonus
_orig_app = P.Run5.appraise
_orig_res = P.Run5.restore


def price_bonus(self, it):
    return _orig_bonus(self, it) + event_bonus(self, it["id"])


def appraise(self, it):
    return False if self.event_today() == "E4" else _orig_app(self, it)


def restore(self, it):
    return False if self.event_today() == "E5" else _orig_res(self, it)


def box_price_today(self, box):
    base = self.c.box_price[box]
    return round(base * .8) if (box == "H2" and self.event_today() == "E3") else base


def item_mult(self, iid):
    return 1 + event_bonus(self, iid) if CFG["adaptive"] else 1.0


P.Run5.event_today = event_today
P.Run5.price_bonus = price_bonus
P.Run5.appraise = appraise
P.Run5.restore = restore
P.Run5.box_price_today = box_price_today
P.Run5.item_mult = item_mult


def _wrap_play():
    orig = P.Run5.buy_open

    return orig


def measure(content, n=2000, **skill):
    """Trả thắng% và thống kê chọn hộp theo ngày có E3 hay không."""
    old = dict(C.SKILL)
    C.SKILL.update(skill); C.SKILL["per_box"] = True
    out = {}
    for name, st in C.TURN_NAMES.items():
        w = 0
        mix = {True: {}, False: {}}
        days = {True: [0, 0.0], False: [0, 0.0]}
        for s in range(n):
            run = P.Run5(s, content, st)
            g = run.g
            while g.status == "playing":
                P.start_hooks(run)
                e3 = run.event_today() == "E3"
                cash0 = g.cash
                bought0 = {}
                orig = C.turn_choose

                def spy(r, rc, tiers, _orig=orig, _e3=e3):
                    it = _orig(r, rc, tiers)
                    if it:
                        mix[_e3][it["box"]] = mix[_e3].get(it["box"], 0) + 1
                    return it
                C.turn_choose = spy
                try:
                    st(run)
                finally:
                    C.turn_choose = orig
                run.refresh()
                if g.day >= 2:
                    days[e3][0] += 1
                    days[e3][1] += g.cash - cash0
                P.end_hooks(run)
                g.end_day()
                if g.arrears > 0 or g.carry > 0:
                    run.shortfall_days += 1
            w += g.status == "won"
        out[name] = dict(win=round(100 * w / n, 1), mix=mix,
                         profit={k: round(v[1] / v[0]) if v[0] else 0 for k, v in days.items()},
                         days={k: v[0] for k, v in days.items()})
    C.SKILL.clear(); C.SKILL.update(old)
    return out


def main():
    c = C.setup(True)
    names = list(C.TURN_NAMES)
    base = C.measure_turns(c, 2000, lam=.75, tau=1.0)
    print("Không biến cố".ljust(40) + " ".join("%5.1f" % base[k] for k in names), flush=True)
    for label, on, adaptive, allowed in (("Có E1-E6, người chơi NGÂY THƠ", True, False, ALL),
                                         ("Có E1-E6, người chơi THÍCH NGHI", True, True, ALL),
                                         ("Chỉ E3, THÍCH NGHI", True, True, ["E3"]),
                                         ("Chỉ E3, NGÂY THƠ", True, False, ["E3"])):
        CFG.update(on=on, adaptive=adaptive, allowed=allowed)
        r = measure(c, 2000, lam=.75, tau=1.0)
        print(label.ljust(40) + " ".join("%5.1f" % r[k]["win"] for k in names), flush=True)
        if allowed == ["E3"] and adaptive:
            for k in names:
                m = r[k]["mix"]
                tot = {e: sum(m[e].values()) or 1 for e in (True, False)}
                print("   %-26s ngày E3: %s | ngày thường: %s | lời/ngày E3 %d, thường %d" % (
                    k, " ".join("%s %2.0f%%" % (b, 100 * m[True].get(b, 0) / tot[True]) for b in ("H1", "H2", "H3", "RD")),
                    " ".join("%s %2.0f%%" % (b, 100 * m[False].get(b, 0) / tot[False]) for b in ("H1", "H2", "H3", "RD")),
                    r[k]["profit"][True], r[k]["profit"][False]), flush=True)
    for e in ("E1", "E2", "E4", "E5", "E6"):
        CFG.update(on=True, adaptive=True, allowed=[e])
        r = measure(c, 2000, lam=.75, tau=1.0)
        print(("Chỉ %s, THÍCH NGHI" % e).ljust(40) + " ".join("%5.1f" % r[k]["win"] for k in names), flush=True)
    CFG.update(on=False)


if __name__ == "__main__":
    main()

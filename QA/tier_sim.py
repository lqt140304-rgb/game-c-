"""Thử cấu hình lời tuyệt đối tăng theo giá hộp để việc chọn hộp thành đánh đổi thật (Claude 2).

Mục tiêu (từ BienCoE3.md mục 3): hiện H2 chi phối (66% đến 71% lượt mua) và Rương Đen không ai mua.
Cấu hình thử đổi giá nền hiện vật sao cho LỜI KỲ VỌNG MỖI HỘP tăng theo giá hộp, giữ nguyên tỷ lệ
rơi, tỷ lệ hàng giả theo hộp (G1) và mọi luật khác. Chạy: python3 tier_sim.py (khoảng 12 phút)
"""
import itertools
import clue_sim as C
import risk_penalty as R
import risk_full as F
import proposal_sim as P
import sprint3_sim as sm
from sprint3_sim import Content, ITEMS, BOXES

BOX_PRICE = {"H1": 100, "H2": 350, "H3": 800, "RD": 1500}
TARGETS = {   # lời kỳ vọng mỗi hộp (xu)
    "Hiện tại (G1)": None,
    "C1 lời 120/200/320/450": dict(H1=120, H2=200, H3=320, RD=450),
    "C2 lời 130/200/260/300": dict(H1=130, H2=200, H3=260, RD=300),
}


def mk(g1, g2, g3, g4, s):
    m = {}
    for i in ("V01", "V02", "V03"): m[i] = g1
    for i in ("V04", "V05", "V06"): m[i] = g2
    m["V09"], m["V07"], m["V08"], m["V10"] = g3, g4, s, s
    return Content(item_mult=m)


def solve(profit):
    """Tìm hệ số giá nền sao cho lời kỳ vọng mỗi hộp bằng `profit` (dưới hàng giả theo hộp)."""
    ratio = {b: 1 + profit[b] / BOX_PRICE[b] for b in BOX_PRICE}
    def err(c, boxes):
        ev = R.box_ev(c)
        return sum(abs(ev[b] - ratio[b]) for b in boxes)
    g1 = min((x / 100 for x in range(20, 200)), key=lambda x: err(mk(x, 1, 1, 1, 1), ["H1"]))
    g2 = min((x / 100 for x in range(60, 500)), key=lambda x: err(mk(1, x, 1, 1, 1), ["H2"]))
    best = None
    for s in (x / 20 for x in range(14, 120)):
        for g3 in (x / 20 for x in range(8, 150)):
            if 400 * g3 > 0.6 * 1000 * s:
                continue
            for g4 in (x / 20 for x in range(10, 90)):
                e = err(mk(1, 1, g3, g4, s), ["H3", "RD"])
                if best is None or e < best[0] - 1e-9:
                    best = (e, s, g3, g4)
    return mk(g1, g2, best[2], best[3], best[1])


def rounded(c):
    prices = {k: round(v[1], -1) for k, v in c.items.items()}
    return Content(item_mult={k: prices[k] / ITEMS[k][1] for k in ITEMS}), prices


def forced(tier, smart=True):
    def strat(run):
        g = run.g
        for _ in range(3):
            r = P.reserve5(run)
            if g.cash < r:
                r = 0
            it = C.turn_choose(run, r, (tier,)) if (tier != "RD" or g.day >= sm.RD_FROM_DAY) else None
            if not it:
                break
            P.handle5(run, it, r, smart)
    return strat


def win(content, st, n):
    return round(100 * sum(P.play5(s, content, st).g.status == "won" for s in range(n)) / n, 1)


def mix_by_bracket(content, st, n):
    """Tỷ lệ chọn từng loại hộp theo số tiền đang có khi quyết định."""
    brackets = [("dưới 600", 0, 600), ("600 đến 1.500", 600, 1500), ("trên 1.500", 1500, 10**9)]
    cnt = {b[0]: {} for b in brackets}
    orig = C.turn_choose

    def spy(run, rc, tiers):
        cash = run.g.cash
        it = orig(run, rc, tiers)
        if it:
            for name, lo, hi in brackets:
                if lo <= cash < hi:
                    cnt[name][it["box"]] = cnt[name].get(it["box"], 0) + 1
        return it
    C.turn_choose = spy
    try:
        for s in range(n):
            P.play5(s, content, st)
    finally:
        C.turn_choose = orig
    return cnt


def main(n=1500):
    C.setup(True)
    names = list(C.TURN_NAMES)
    for label, prof in TARGETS.items():
        c = F.content(F.X110) if prof is None else rounded(solve(prof))[0]
        ev = R.box_ev(c)
        prices = {k: v[1] for k, v in c.items.items()}
        print("== %s" % label)
        print("   giá nền:", " ".join("%s %d" % (k, prices[k]) for k in sorted(prices)))
        print("   lần giá hộp H1/H2/H3/RD:", " / ".join("%.2f" % ev[b] for b in BOX_PRICE),
              "| lời mỗi hộp:", " / ".join("%d" % round((ev[b] - 1) * BOX_PRICE[b]) for b in BOX_PRICE))
        base = C.measure_plain(c, n)
        print("   bậc thang cũ (không đọc):", " ".join("%s %.1f" % (k.split(" ")[0] + (" gđ" if "giám" in k else ""), base[k]) for k in base))
        r = C.measure_turns(c, n, lam=.75, tau=1.0)
        print("   chọn loại + đọc manh mối:", " ".join("%.1f" % r[k] for k in names))
        print("   ép một loại hộp (thận trọng + giám định): " + " ".join("%s %.1f" % (t, win(c, forced(t), n)) for t in ("H1", "H2", "H3", "RD")))
        m = mix_by_bracket(c, C.TURN_NAMES["thận trọng + giám định"], n)
        for b, d in m.items():
            tot = sum(d.values()) or 1
            print("   chọn hộp khi có %-14s " % b + " ".join("%s %2.0f%%" % (t, 100 * d.get(t, 0) / tot) for t in ("H1", "H2", "H3", "RD")) + "  (%d lượt)" % tot, flush=True)
        print(flush=True)


if __name__ == "__main__":
    main()

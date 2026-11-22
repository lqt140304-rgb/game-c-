"""Đo giá trị của kỹ năng đọc manh mối niêm phong (Claude 2).

GIẢ ĐỊNH (NoiDungMau.md mục 4 chưa nói rõ manh mối gắn với hiện vật thật thế nào):
- Mỗi hộp vật lý có 3 manh mối: 2 manh mối ĐÚNG gợi nhóm nguồn gốc của hiện vật bên trong,
  1 manh mối SAI gợi một nhóm khác (đều trong 5 nhóm còn lại). Người chơi thấy 1 manh mối ngẫu nhiên
  miễn phí khi xem hộp, mua thêm mỗi manh mối 30 xu, tối đa 3 (CB2 và Mốc 3 Cổ Phong thêm manh mối miễn phí).
- Người chơi xem được tối đa M hộp cùng loại ở quầy (M là tham số thiết kế chưa chốt) và chọn hộp tốt nhất.
- Bot tính xác suất hậu nghiệm đúng (Bayes) trên cấu trúc 2 đúng 1 sai, rồi mua hộp có giá trị kỳ vọng
  trên giá hộp cao nhất nếu vượt ngưỡng tau, không thì thử loại hộp rẻ hơn hoặc bỏ lượt.
Chạy: python3 clue_sim.py  (khoảng 6 phút)
"""
import itertools
import math
import proposal_sim as P
import risk_penalty as R
import risk_full as F
import sprint3_sim as sm
from sprint3_sim import BOXES, ITEMS, ART, FAKE_P, FAKE_P_V09, FAKE_X, DEBT, QK, HD, TL, NHT, CT, TT

GROUPS = [QK, HD, TL, NHT, CT, TT]
CLUE_PRICE = 30
SKILL = dict(M=3, tau=1.0, extra=0, n_true=2, must_buy=False, lam=0.0, clue_price=30, read=True)
NTRUE = {"H1": 2, "H2": 2, "H3": 1, "RD": 1}   # luật Claude 1: hộp đắt hay bị đánh lạc hướng


def n_true(box):
    return SKILL["n_true"] if SKILL["n_true"] is not None and not SKILL.get("per_box") else NTRUE[box]   # tham số kỹ năng, đổi khi quét
FAKE_BY_BOX = R.FAKE_BY_BOX                          # dùng chung với risk_penalty (G)


def fake_rate(box, iid):
    return R.fake_rate(box, iid)


def likelihood(obs, origin, nt=None):
    """P(tập manh mối quan sát | hiện vật có nguồn gốc `origin`). Cấu trúc 3 manh mối:
    n_true=2: [origin, origin, sai]; n_true=1: [origin, sai, sai]. Nhóm sai đồng đều trong 5 nhóm khác."""
    others = [g for g in GROUPS if g != origin]
    total, cnt = 0.0, 0
    nt = SKILL["n_true"] if nt is None else nt
    wrongs = [(w,) for w in others] if nt == 2 else [(a, b) for a in others for b in others]
    for ws in wrongs:
        L = [origin] * nt + list(ws)
        k = len(obs)
        combos = list(itertools.combinations(range(3), k))
        match = sum(1 for c in combos if sorted(L[i] for i in c) == sorted(obs))
        total += match / len(combos)
        cnt += 1
    return total / cnt


def posterior_ev(run, box, obs):
    price = run.c.box_price[box]
    weights = BOXES[box][1]
    qm = sum(p * f for _, p, f in run.c.quality)
    num = den = 0.0
    for iid, wgt in weights.items():
        origin, base = run.c.items[iid]
        l = likelihood(obs, origin, n_true(box)) if obs else 1.0
        fp = fake_rate(box, iid)
        val = base * qm * (1 - fp + fp * FAKE_X)
        num += wgt * l * val
        den += wgt * l
    return (num / den) / price if den else 0.0


def make_candidate(run, box):
    w = BOXES[box][1]
    iid = run.rng.choices(list(w), weights=list(w.values()))[0]
    r, acc, qf, ql = run.rng.random(), 0, 1.0, "Nguyên"
    for name, p, f in run.c.quality:
        acc += p
        if r <= acc:
            qf, ql = f, name
            break
    fake = run.rng.random() < fake_rate(box, iid)
    origin = run.c.items[iid][0]
    wrong = run.rng.choice([g for g in GROUPS if g != origin])
    nt = n_true(box)
    clues = [origin] * nt + [wrong] * (3 - nt)
    if nt == 1:
        clues[2] = run.rng.choice([g for g in GROUPS if g != origin])
    run.rng.shuffle(clues)
    return {"id": iid, "q": qf, "ql": ql, "fake": fake, "appraised": False, "box": box,
            "clues": clues, "seen": 1}


def free_clues(run):
    if SKILL.get("free_n"):
        return SKILL["free_n"]
    n = 1
    if run.has("TC1", "TC2"):
        n += 1                                   # CB2 thêm 1 manh mối miễn phí
    if run.n_he("CP") >= 3:
        n += 1                                   # Mốc 3 Cổ Phong
    return min(n, 3)


def choose_and_buy(run, box, reserve_cash):
    """Xem tối đa M hộp loại `box`, trả hiện vật nếu mua, None nếu bỏ qua."""
    g = run.g
    price = run.c.box_price[box]
    if g.bought_today >= 3 or g.cash - price < reserve_cash:
        return None
    cands = [make_candidate(run, box) for _ in range(SKILL["M"])]
    nfree = free_clues(run)
    for c in cands:
        c["seen"] = nfree
    ev = [posterior_ev(run, box, c["clues"][:c["seen"]]) for c in cands]
    # giá hộp theo manh mối nhìn thấy (Dì Mận cũng đọc manh mối): lam = 0 giá cố định, 1 giá đúng giá trị
    lam = SKILL["lam"]
    prior = posterior_ev(run, box, [])
    mult = [1 + lam * (e / prior - 1) for e in ev]
    ev = [e / m for e, m in zip(ev, mult)]
    order = sorted(range(len(cands)), key=lambda i: -ev[i])
    best = order[0]
    price = round(price * mult[best])
    # mua thêm manh mối cho hộp tốt nhất nếu còn mơ hồ quanh ngưỡng
    extra = SKILL["extra"]
    while extra > 0 and cands[best]["seen"] < 3:
        e = ev[best]
        if not (0.8 * SKILL["tau"] <= e <= 1.3 * SKILL["tau"]) or g.cash - price - SKILL["clue_price"] < reserve_cash:
            break
        g.cash -= SKILL["clue_price"]
        cands[best]["seen"] += 1
        ev[best] = posterior_ev(run, box, cands[best]["clues"][:cands[best]["seen"]]) / mult[best]
        order = sorted(range(len(cands)), key=lambda i: -ev[i])
        best = order[0]
        extra -= 1
    if ev[best] < SKILL["tau"] or g.cash < price:
        return None
    g.cash -= price
    g.bought_today += 1
    c = cands[best]
    return c


def ladder_skill(run, keep_reserve, smart):
    g = run.g
    for _ in range(3):
        r = P.reserve5(run) if keep_reserve else 0
        if g.cash < r:
            r = 0
        bought = False
        for box in (("RD",) if g.day >= sm.RD_FROM_DAY else ()) + ("H3", "H2", "H1"):
            if g.cash - run.c.box_price[box] < r:
                continue
            it = choose_and_buy(run, box, r)
            if it:
                P.handle5(run, it, r, smart)
                bought = True
                break
        if not bought:
            if SKILL["must_buy"] and g.bought_today == 0:
                tau, SKILL["tau"] = SKILL["tau"], -1.0
                for box in ("H2", "H1"):
                    it = choose_and_buy(run, box, 0)
                    if it:
                        P.handle5(run, it, 0, smart)
                        break
                SKILL["tau"] = tau
            break


def make_strategy(kind, smart=False):
    plans = {"none": [], "small": ["CP1", "CP2"], "big": ["CP1", "CP2", "TC3", "CP3"]}

    def strat(run):
        g = run.g
        if kind == "bold":
            return ladder_skill(run, False, smart)
        if g.day <= 5:
            for aid in plans[kind]:
                if aid in run.owned:
                    continue
                if g.cash - ART[aid][1] >= P.reserve5(run) + 100:
                    run.buy_artifact(aid)
                break
        ladder_skill(run, True, smart)
    return strat


NAMES = {
    "thận trọng": make_strategy("none"),
    "thận trọng + giám định": make_strategy("none", True),
    "liều": make_strategy("bold"),
    "liều + giám định": make_strategy("bold", True),
    "combo nhỏ + giám định": make_strategy("small", True),
    "combo lớn + giám định": make_strategy("big", True),
}


def measure_skill(content, n=3000, **skill):
    old = dict(SKILL)
    SKILL.update(skill)
    P.STRATS.update({k + " (đọc manh mối)": v for k, v in NAMES.items()})
    res = {}
    for k in NAMES:
        st = P.STRATS[k + " (đọc manh mối)"]
        w = sum(P.play5(s, content, st).g.status == "won" for s in range(n))
        res[k] = round(100 * w / n, 1)
    SKILL.clear(); SKILL.update(old)
    return res


def measure_plain(content, n=3000):
    res = {}
    for k in NAMES:
        base = {"thận trọng": "thận trọng", "thận trọng + giám định": "thận trọng + giám định chọn lọc",
                "liều": "liều", "liều + giám định": "liều + giám định chọn lọc",
                "combo nhỏ + giám định": "combo nhỏ + giám định", "combo lớn + giám định": "combo lớn + giám định"}[k]
        st = P.STRATS[base]
        w = sum(P.play5(s, content, st).g.status == "won" for s in range(n))
        res[k] = round(100 * w / n, 1)
    return res


def setup(g1=False):
    for k, v in F.PKG.items():
        setattr(P, k, v)
    ART["CP3"], ART["TC3"] = ("CP", 500), ("TC", 500)
    R.FAKE_BY_BOX.clear()
    if g1:
        R.FAKE_BY_BOX.update(dict(H1=.12, H2=.15, H3=.22, RD=.30))
        P.Run5.buy_open = R.buy_open
        P.Run5.p_real = R.p_real
        return F.content(F.X110)
    return F.content(F.BASE)


if __name__ == "__main__":
    for label, g1 in (("Gói đã chốt (hàng giả 15% mọi hộp)", False), ("G1 (hàng giả theo hộp, giá x1,10)", True)):
        c = setup(g1)
        plain = measure_plain(c)
        print("== %s" % label)
        print("%-28s %8s %8s %8s %8s %8s" % ("chiến thuật", "không đọc", "M=1", "M=3", "M=3+2 thêm", "M=5"))
        rows = {"M=1": measure_skill(c, M=1), "M=3": measure_skill(c, M=3),
                "M=3+": measure_skill(c, M=3, extra=2), "M=5": measure_skill(c, M=5)}
        for k in NAMES:
            print("%-28s %8.1f %8.1f %8.1f %8.1f %8.1f" % (k, plain[k], rows["M=1"][k], rows["M=3"][k], rows["M=3+"][k], rows["M=5"][k]), flush=True)
        print("-- ngưỡng tau (M=3), thận trọng + giám định / liều + giám định / combo lớn + giám định")
        for tau in (0.0, 1.0, 1.3, 1.6):
            r = measure_skill(c, M=3, tau=tau)
            print("tau %.1f  %6.1f %6.1f %6.1f" % (tau, r["thận trọng + giám định"], r["liều + giám định"], r["combo lớn + giám định"]), flush=True)
        print()


def sweep_controls():
    """Các cách hạ lợi thế của người đọc manh mối (cấu hình G1, ngưỡng tau 1,3). Chạy: python3 -c 'import clue_sim as C; C.sweep_controls()'"""
    c = setup(True)
    ks = ["thận trọng + giám định", "liều + giám định", "combo lớn + giám định"]
    plain = measure_plain(c, 2000)
    print("không đọc manh mối: " + " ".join("%5.1f" % plain[k] for k in ks))
    for label, kw in (("M=1, tau 1,3", dict(M=1, tau=1.3)), ("M=2, tau 1,3", dict(M=2, tau=1.3)), ("M=3, tau 1,3", dict(M=3, tau=1.3)),
                      ("M=3, tau 1,3, manh mối đúng 1/3", dict(M=3, tau=1.3, n_true=1)),
                      ("M=2, tau 1,3, manh mối đúng 1/3", dict(M=2, tau=1.3, n_true=1)),
                      ("M=3, tau 1,3, phải mua >= 1 hộp mỗi ngày", dict(M=3, tau=1.3, must_buy=True)),
                      ("M=2, tau 1,3, phải mua >= 1 hộp mỗi ngày", dict(M=2, tau=1.3, must_buy=True)),
                      ("M=2, tau 1,3, đúng 1/3, phải mua >= 1", dict(M=2, tau=1.3, n_true=1, must_buy=True))):
        r = measure_skill(c, n=2000, **kw)
        print("%-44s" % label + " ".join("%5.1f" % r[k] for k in ks), flush=True)


def sweep_pricing():
    """Giá hộp phụ thuộc manh mối (lam). Cấu hình G1. python3 -c 'import clue_sim as C; C.sweep_pricing()'"""
    c = setup(True)
    ks = ["thận trọng + giám định", "liều + giám định", "combo lớn + giám định"]
    for label, kw in (("lam 0 (giá cố định), tau 1,3", dict(M=3, tau=1.3)),
                      ("lam 0,5, tau 1,3", dict(M=3, tau=1.3, lam=.5)),
                      ("lam 0,5, tau 1,0", dict(M=3, tau=1.0, lam=.5)),
                      ("lam 1,0, tau 1,0", dict(M=3, tau=1.0, lam=1.0)),
                      ("lam 0,5, tau 1,3, manh mối đúng 1/3", dict(M=3, tau=1.3, lam=.5, n_true=1))):
        r = measure_skill(c, n=2000, **kw)
        print("%-44s" % label + " ".join("%5.1f" % r[k] for k in ks), flush=True)


# ---- Luật của Claude 1 (trả lời D-26): mỗi lượt Dì Mận bày 1 hộp mỗi loại, mua 1 hoặc dừng ----
def turn_choose(run, reserve_cash, tiers):
    g = run.g
    cands = []
    for box in tiers:
        tier_price = run.c.box_price[box]
        c = make_candidate(run, box)
        c["seen"] = free_clues(run) if SKILL["read"] else 0
        e = posterior_ev(run, box, c["clues"][:c["seen"]])
        prior = posterior_ev(run, box, [])
        c["mult"] = 1 + SKILL["lam"] * (e / prior - 1)
        c["price"] = max(1, round(tier_price * c["mult"]))
        c["tier_price"] = tier_price
        c["value"] = e * tier_price
        cands.append(c)
    ok = [c for c in cands if g.cash - c["price"] >= reserve_cash]
    if not ok:
        return None
    best = max(ok, key=lambda c: c["value"] - c["price"])
    # mua thêm manh mối cho hộp tốt nhất nếu còn mơ hồ quanh ngưỡng
    tau = SKILL["tau"]
    extra = SKILL["extra"]
    while extra > 0 and best["seen"] < 3 and SKILL["clue_price"] is not None:
        ratio = best["value"] / best["price"]
        if not (0.85 * tau <= ratio <= 1.25 * tau) or g.cash - best["price"] - SKILL["clue_price"] < reserve_cash:
            break
        g.cash -= SKILL["clue_price"]
        best["seen"] += 1
        best["value"] = posterior_ev(run, best["box"], best["clues"][:best["seen"]]) * best["tier_price"]
        extra -= 1
    if best["value"] / best["price"] < tau or g.cash < best["price"]:
        return None
    g.cash -= best["price"]
    g.bought_today += 1
    return best


def ladder_turns(run, keep_reserve, smart):
    g = run.g
    for _ in range(3):
        r = P.reserve5(run) if keep_reserve else 0
        if g.cash < r:
            r = 0
        tiers = (("RD",) if g.day >= sm.RD_FROM_DAY else ()) + ("H3", "H2", "H1")
        it = turn_choose(run, r, tiers)
        if not it:
            break
        P.handle5(run, it, r, smart)


def make_turn_strategy(kind, smart=False):
    plans = {"none": [], "small": ["CP1", "CP2"], "big": ["CP1", "CP2", "TC3", "CP3"]}

    def strat(run):
        g = run.g
        if kind == "bold":
            return ladder_turns(run, False, smart)
        if g.day <= 5:
            for aid in plans[kind]:
                if aid in run.owned:
                    continue
                if g.cash - ART[aid][1] >= P.reserve5(run) + 100:
                    run.buy_artifact(aid)
                break
        ladder_turns(run, True, smart)
    return strat


TURN_NAMES = {
    "thận trọng + giám định": make_turn_strategy("none", True),
    "liều + giám định": make_turn_strategy("bold", True),
    "combo nhỏ + giám định": make_turn_strategy("small", True),
    "combo lớn + giám định": make_turn_strategy("big", True),
}


def measure_turns(content, n=2000, **skill):
    old = dict(SKILL)
    SKILL.update(skill); SKILL["per_box"] = True
    res = {}
    for k, st in TURN_NAMES.items():
        w = sum(P.play5(s, content, st).g.status == "won" for s in range(n))
        res[k] = round(100 * w / n, 1)
    SKILL.clear(); SKILL.update(old)
    return res


def sweep_rules():
    """Luật Claude 1: H1/H2 2 đúng 1 sai, H3/RD 1 đúng 2 sai, M=1 mỗi hộp mỗi loại, giá theo manh mối đầu."""
    c = setup(True)
    base = measure_plain(c, 2000)
    ks = list(TURN_NAMES)
    print("không đọc manh mối" + " " * 30 + " ".join("%5.1f" % base[k] for k in ks), flush=True)
    for label, kw in (("lam 0, tau 1,3 (giá cố định)", dict(lam=0.0, tau=1.3)),
                      ("lam 0,75, tau 1,0", dict(lam=.75, tau=1.0)),
                      ("lam 0,75, tau 1,15", dict(lam=.75, tau=1.15)),
                      ("lam 0,75, tau 1,3", dict(lam=.75, tau=1.3)),
                      ("lam 0,75, tau 1,15, +2 manh mối 30 xu", dict(lam=.75, tau=1.15, extra=2, clue_price=30)),
                      ("lam 0,75, tau 1,15, +2 manh mối 20 xu", dict(lam=.75, tau=1.15, extra=2, clue_price=20)),
                      ("lam 0,75, tau 1,15, +2 manh mối 15 xu", dict(lam=.75, tau=1.15, extra=2, clue_price=15)),
                      ("lam 0,75, tau 1,3, +2 manh mối 15 xu", dict(lam=.75, tau=1.3, extra=2, clue_price=15)),
                      ("lam 0,9, tau 1,15", dict(lam=.9, tau=1.15))):
        r = measure_turns(c, 2000, **kw)
        print("%-48s" % label + " ".join("%5.1f" % r[k] for k in ks), flush=True)


def sweep_isolate():
    """Tách riêng giá trị của manh mối khỏi giá trị của việc chọn loại hộp (mỗi lượt có 4 loại để chọn)."""
    c = setup(True)
    base = measure_plain(c, 2000)
    ks = list(TURN_NAMES)
    print("bậc thang cũ (hộp đắt nhất mua được)       " + " ".join("%5.1f" % base[k] for k in ks), flush=True)
    for label, kw in (("chọn loại hộp, KHÔNG đọc manh mối", dict(read=False, lam=0.0, tau=1.0)),
                      ("chọn loại hộp, đọc manh mối, giá cố định", dict(read=True, lam=0.0, tau=1.0)),
                      ("đọc manh mối, giá theo manh mối lam 0,5", dict(read=True, lam=.5, tau=1.0)),
                      ("đọc manh mối, giá theo manh mối lam 0,75", dict(read=True, lam=.75, tau=1.0)),
                      ("đọc manh mối, giá theo manh mối lam 1,0", dict(read=True, lam=1.0, tau=1.0)),
                      ("lam 0,75, thêm 2 manh mối 30 xu", dict(read=True, lam=.75, tau=1.0, extra=2, clue_price=30)),
                      ("lam 0,75, thêm 2 manh mối 15 xu", dict(read=True, lam=.75, tau=1.0, extra=2, clue_price=15)),
                      ("lam 0, thêm 2 manh mối 15 xu", dict(read=True, lam=0.0, tau=1.0, extra=2, clue_price=15))):
        r = measure_turns(c, 2000, **kw)
        print("%-44s" % label + " ".join("%5.1f" % r[k] for k in ks), flush=True)


def sweep_oracle():
    """Cận trên giá trị manh mối thêm: nhìn đủ 3 manh mối miễn phí."""
    c = setup(True)
    ks = list(TURN_NAMES)
    for label, kw in (("lam 0,75, 1 manh mối miễn phí", dict(lam=.75, tau=1.0)),
                      ("lam 0,75, 3 manh mối miễn phí", dict(lam=.75, tau=1.0, free_n=3)),
                      ("lam 0, 1 manh mối miễn phí", dict(lam=0.0, tau=1.0)),
                      ("lam 0, 3 manh mối miễn phí", dict(lam=0.0, tau=1.0, free_n=3))):
        r = measure_turns(c, 2000, **kw)
        print("%-44s" % label + " ".join("%5.1f" % r[k] for k in ks), flush=True)

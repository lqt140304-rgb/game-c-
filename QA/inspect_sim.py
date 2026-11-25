"""Sprint 5 phần 3 (Claude 2), BẢN 2: đo lại soi lỗi và trả giá theo LUẬT SOI MỚI (chủ dự án chốt 2026-11-24).
Bản cũ (lượt soi, nhiễu, phí soi thêm) đã bị thay thế; xem Docs/Balance/Sprint5SoiLoi.md.

Luật soi mới: người chơi tự tìm chỗ lỗi (di chuột đúng chỗ mới hiện chữ, chỗ không lỗi không hiện gì), bấm vào lỗi thì giá
trừ theo lỗi; bảng món đồ cho chọn thật hay giả, hiện giá trị và thông tin cơ bản.
Mô hình dưới đây là GIẢ ĐỊNH của Claude 2 theo BanGiaoSprint5SoiLoi.md (chưa chốt, các con số là tham số RULES):
  1. Lỗi tình trạng thay cho hệ số tình trạng: Nguyên không lỗi; Xước n lỗi nhẹ (T1 H2 X3 L3) tổng trừ 30%;
     Hỏng n lỗi nhẹ + 1 vết hỏng lớn, tổng trừ 70% (nhẹ 30% + lớn 40%). Giá cuối vẫn là giá nền x hệ số tình trạng.
  2. Món giả có `marks` dấu hàng giả (T1 H1 X2 L2); món thật có "dấu lạ" giống dấu giả với xác suất `odd`.
     Người chơi suy ra xác suất giả theo Bayes từ số dấu đã thấy rồi chọn thật hay giả trên bảng.
  3. Kỹ năng s = xác suất tìm ra mỗi lỗi hoặc dấu: giỏi 95%, thường 75%, ẩu 45%, bỏ qua soi 0%. Soi không giới hạn thời gian.
  4. Lỗi bỏ sót: cửa hàng kiểm tra lại thì vẫn trừ giá, cộng phạt `miss_pen` x giá thẩm định mỗi lỗi sót.
  5. Trả giá (Hiếm trở lên): mở màn ở giá bảng sau khi đã trừ lỗi (open = 1,0). Mỗi vòng thắng với xác suất
     (kỹ năng trả giá c) x (tỉ lệ lỗi đã tìm); thắng +up, thua -down, giá chốt trong [floor, cap]. Dừng lúc nào cũng được.
  5b. Nhà vá lỗi (chủ dự án chốt 2026-11-25): lỗi đã tìm ra thì sửa được, mỗi lỗi một khoản phí theo giá nền (`repair_light`,
     `repair_big`); lỗi sót không sửa được. Người chơi sửa khi lời kỳ vọng lớn hơn phí. Phục chế cũ (nâng một bậc tình trạng) vẫn có.
  6. Gợi ý của Giáo Sư Lẫm: chỉ ra một dấu hàng giả chưa tìm thấy (nếu món có), không thì một lỗi chưa tìm thấy;
     phí hint_rate x giá nền, tối thiểu 200.000 đ; combo CB3 vẫn một lần miễn phí mỗi ngày.
  Giữ nguyên luật thật: khai giả chỉ nhận giá hàng giả (fake_x), giả mà khai thật bị phạt `fine` (= fake_x thì tịch thu),
  ngày 1 miễn phạt. Tiền tính theo đơn vị 10.000 đ, in qua currency.fmt_vnd.
Bỏ bấm một lỗi đã thấy được phép (chốt 2026-11-25); khi người thu mua soi thấy thì trừ tiền, cộng phạt, nên mô hình coi như lỗi bị sót.
"""
import math
import chest_sim as CS
import clue_sim as C
import proposal_sim as P
import sprint3_sim as sm
from currency import fmt_vnd

TIERS = ("T", "H", "X", "L")
RULES = dict(
    n_light=dict(T=1, H=2, X=3, L=3),         # số lỗi nhẹ của món Xước (món Hỏng thêm 1 vết lớn)
    light_cut=.30, big_cut=.40,               # Xước trừ 30%; Hỏng trừ 30% + 40% = 70%
    marks=dict(T=1, H=1, X=2, L=2),           # số dấu hàng giả trên món giả
    odd=.15,                                  # xác suất món thật có một "dấu lạ" giống dấu giả
    miss_pen=.10,                             # phạt mỗi lỗi bị sót, theo giá thẩm định
    open=1.0, up=.08, down=.05,               # giá chào mở màn = giá bảng đã trừ lỗi; thắng +8%, thua -5%
    rounds=3, floor=.70, cap=1.05,
    fake_x=.20, fine=.20, fine_T=None, day1_fine=0.0,
    hint_rate=.05,                            # gợi ý của Giáo Sư Lẫm: 5% giá nền, tối thiểu 200.000 đ
    repair_light=.12, repair_big=.30,         # nhà vá lỗi: phí mỗi lỗi sửa, theo giá nền (lỗi nhẹ 12%, vết hỏng lớn 30%)
    cert=False,
)
PROFILES = {
    # s: xác suất tìm ra mỗi lỗi hoặc dấu; c: kỹ năng trả giá; hint: có mua gợi ý;
    # rule: "opt" quyết theo giá trị kỳ vọng, "half" khai giả khi xác suất giả > 50%, "real" luôn khai thật.
    "giỏi": dict(s=.95, c=.95, rule="opt", hint=True, skip_haggle=False),
    "thường": dict(s=.75, c=.75, rule="half", hint=False, skip_haggle=False),
    "ẩu": dict(s=.45, c=.50, rule="half", hint=False, skip_haggle=False),
    "bỏ qua soi": dict(s=0, c=0, rule="real", hint=False, skip_haggle=True),
}
CFG = dict(rules=RULES, profile="thường")
TIER_OF = {}
LEVELS = [.3, .7, 1.0]


def apply(design=None, rules=None, profile="thường"):
    """Nạp bảng rương (mặc định chest_sim.FINAL) rồi thay bước bán bằng soi lỗi và trả giá."""
    c = CS.apply(design or CS.FINAL)
    C.setup_done = True
    CFG["rules"] = dict(RULES, **(rules or {}))
    CFG["profile"] = profile
    TIER_OF.clear()
    for ch in CS.STATE["design"]["chests"].values():
        for t, pool in ch["pools"].items():
            for i in pool:
                TIER_OF[i] = t
    if not CFG["rules"]["cert"]:
        P.CERT_BONUS = 0.0
    P.handle5 = handle
    return c


def restore():
    CS.restore()


# --- các phép tính một món -----------------------------------------------------------------------------
def n_defects(q, tier, rl):
    """Số lỗi tình trạng: Nguyên 0; Xước n nhẹ; Hỏng n nhẹ + 1 lớn."""
    if q >= 1.0:
        return 0
    return rl["n_light"][tier] + (1 if q <= .3 else 0)


def binom(n, k, s):
    return math.comb(n, k) * s ** k * (1 - s) ** (n - k) if 0 <= k <= n else 0.0


def lik(prior, k, m, s, rl, hint=None):
    """(độ hợp lý giả, thật) khi tự soi thấy k dấu giả trên món có m dấu; hint: None / 'shown' / 'none'
    (gợi ý Giáo Sư Lẫm chỉ ra một dấu chưa tìm thấy nếu món có)."""
    o = rl["odd"]
    lf = binom(m, k, s)
    if hint == "shown":
        lf *= 1.0 if m > k else 0.0
    elif hint == "none":
        lf *= 1.0 if m == k else 0.0
    if k == 0:
        lr = {None: 1 - o * s, "shown": o * (1 - s), "none": 1 - o}[hint]
    elif k == 1:
        lr = 0.0 if hint == "shown" else o * s
    else:
        lr = 0.0
    return prior * lf, (1 - prior) * lr


def posterior(prior, k, m, s, rl, hint=None):
    """P(giả | k dấu giả đã thấy) theo Bayes."""
    if prior <= 0:
        return 0.0
    lf, lr = lik(prior, k, m, s, rl, hint)
    return lf / (lf + lr) if lf + lr > 0 else prior


def n_rounds(rl, tier):
    r = rl["rounds"]
    return r[tier] if isinstance(r, dict) else r


def haggle_win(pf, found, total):
    """Xác suất thắng một vòng trả giá = kỹ năng trả giá x tỉ lệ lỗi đã tìm (món không lỗi: đủ)."""
    return pf["c"] * (found / total if total else 1.0)


def haggle_ev(pw, rl, tier="H"):
    """Hệ số giá kỳ vọng khi trả giá (dừng ngay nếu vòng không có lợi)."""
    if pw * rl["up"] - (1 - pw) * rl["down"] <= 0:
        return rl["open"]
    m = rl["open"] + n_rounds(rl, tier) * (pw * rl["up"] - (1 - pw) * rl["down"])
    return max(rl["floor"], min(rl["cap"], m))


def real_value(tier, pf, rl, found, total, missed):
    """Hệ số giá kỳ vọng nếu khai thật và món thật: giá đã trừ lỗi, trả giá nếu có lợi, trừ phạt lỗi sót."""
    if tier == "T" or pf["skip_haggle"]:
        m = rl["open"]
    else:
        m = haggle_ev(haggle_win(pf, found, total), rl, tier)
    return m - rl["miss_pen"] * missed


def best_value(p, tier, fine, pf, rl, found, total, missed):
    """(giá trị khai giả, giá trị khai thật) theo hệ số của V, với xác suất giả p."""
    r = real_value(tier, pf, rl, found, total, missed)
    return rl["fake_x"], (1 - p) * r + p * (rl["fake_x"] - fine)


def decide_fake(p, tier, fine, pf, rl, found=0, total=0, missed=0):
    """True nếu người chơi khai giả."""
    if pf["rule"] == "real" or p <= 0:
        return False
    if pf["rule"] == "half":
        return p > .5
    vf, vr = best_value(p, tier, fine, pf, rl, found, total, missed)
    return vr < vf


def hint_voi(prior, k, m, s, tier, fine, pf, rl, found, total, missed):
    """Giá trị thông tin (theo hệ số của V) của gợi ý Giáo Sư Lẫm, theo ước lượng của chính người chơi."""
    opt = dict(pf, rule="opt")

    def best(p):
        vf, vr = best_value(p, tier, fine, opt, rl, found, total, missed)
        return max(vf, vr)
    p0 = posterior(prior, k, m, s, rl)
    out = 0.0
    n0 = sum(lik(prior, k, m, s, rl))
    for h in ("shown", "none"):
        lf, lr = lik(prior, k, m, s, rl, h)
        if lf + lr > 0 and n0 > 0:
            out += (lf + lr) / n0 * best(lf / (lf + lr))
    return out - best(p0)


def haggle(run, pf, rl, tier, found, total):
    """Trả giá kiểu Ace Attorney trên món thật được khai thật. Trả (hệ số giá, số vòng, đúng, sai)."""
    rng = run.rng
    pw = haggle_win(pf, found, total)
    m, ok, bad, rounds = rl["open"], 0, 0, 0
    for _ in range(n_rounds(rl, tier)):
        if pw * rl["up"] - (1 - pw) * rl["down"] <= 0:
            break
        rounds += 1
        if rng.random() < pw:
            m = min(rl["cap"], m + rl["up"])
            ok += 1
        else:
            m = max(rl["floor"], m - rl["down"])
            bad += 1
    return m, rounds, ok, bad


def stats(run):
    if not hasattr(run, "ins"):
        run.ins = dict(items=0, hints=0, haggles=0, rounds=0, quick=0, repairs=0, repair_spend=0, defects=0, found=0, missed=0, pen=0,
                       real_as_fake=0, fake_as_real=0, fines=0, loss_real_as_fake=0, revenue=0, fair=0)
    return run.ins


def _pay_hint(run, it, fee):
    if run.g.cash < fee:
        return False
    run.g.cash -= fee
    run.apprs += 1
    it["appraised"] = True
    return True


def repair_shop(run, it, r, tier, p_fake, base, nl, nb, fl, fb, rl, st, V):
    """Nhà vá lỗi: sửa các lỗi đã tìm ra khi có lời (lời kỳ vọng = (1 - xác suất giả) x phần giá lỗi đó trừ > phí).
    Lỗi sót không sửa được. Trả giá thẩm định mới (hệ số tình trạng tăng đúng phần lỗi đã sửa)."""
    g = run.g
    q0 = it["q"]
    it["q"] = 1.0
    full = run.known_price(it)           # giá nếu không còn lỗi (đã gồm hiệu ứng tạo tác)
    it["q"] = q0
    cut_l = rl["light_cut"] / nl if nl else 0.0
    fixed_cut = 0.0
    for count, cut, rate in ((fl, cut_l, rl["repair_light"]), (fb, rl["big_cut"], rl["repair_big"])):
        fee = max(5, round(base * rate))
        for _ in range(count):
            if (1 - p_fake) * cut * full > fee and g.cash - fee >= r:
                g.cash -= fee
                st["repairs"] += 1
                st["repair_spend"] += fee
                fixed_cut += cut
    if fixed_cut:
        it["q"] = min(1.0, q0 + fixed_cut)
        it["restored"] = True
        return run.known_price(it)
    return V


# --- thay bước bán ------------------------------------------------------------------------------------
def handle(run, it, r, smart):
    """Thay P.handle5: hộp sắp sẵn, rác, tạo tác như cũ; hiện vật thì soi lỗi, chọn thật/giả, trả giá."""
    run.g.cash += CS.script_refund(run, it)
    if it["id"] in CS.JUNK or it["id"].startswith("AX_"):
        run.sell(it)
        return
    g, rl, pf = run.g, CFG["rules"], PROFILES[CFG["profile"]]
    rng = run.rng
    st = stats(run)
    base = run.c.items[it["id"]][1]
    tier = TIER_OF.get(it["id"], "T")
    prior = CS.fake_rate(it["box"], it["id"])
    fine = rl["fine"] if tier != "T" or rl["fine_T"] is None else rl["fine_T"]
    if g.day == 1:
        fine = min(fine, rl["day1_fine"])
    # phục chế như lõi cũ (xóa lỗi tình trạng)
    pr = 1 - prior
    avg = pr + (1 - pr) * rl["fake_x"]
    if it["q"] < 1.0:
        nq = LEVELS[LEVELS.index(it["q"]) + 1]
        fee = run.restore_fee(base)
        if (nq - it["q"]) * base * avg > fee and g.cash - fee >= r:
            run.restore(it)
    # soi: mỗi lỗi và mỗi dấu được tìm ra với xác suất s
    s = pf["s"]
    nd = n_defects(it["q"], tier, rl)
    nb = 1 if it["q"] <= .3 else 0                    # vết hỏng lớn (chỉ món Hỏng)
    nl = nd - nb                                       # lỗi nhẹ
    fl = sum(rng.random() < s for _ in range(nl))     # lỗi nhẹ đã tìm ra
    fb = 1 if nb and rng.random() < s else 0           # vết lớn đã tìm ra
    found = fl + fb
    if it["fake"]:
        m = rl["marks"][tier]
        k = sum(rng.random() < s for _ in range(m))
        odd_unfound = False
    else:
        m = rl["marks"][tier]
        has_odd = rng.random() < rl["odd"]
        k = 1 if has_odd and rng.random() < s else 0
        odd_unfound = has_odd and k == 0
    k_unfound = (m - k) if it["fake"] else (1 if odd_unfound else 0)
    # gợi ý của Giáo Sư Lẫm: chỉ ra dấu chưa tìm thấy (nếu có), không thì một lỗi
    hint = None
    if smart and tier != "T" and pf["hint"]:
        fee = max(20, round(base * rl["hint_rate"]))
        free = run.cb3() and run.free_cnt.get(g.day, 0) < P.CB3_FREE
        V0 = run.known_price(it)
        voi = hint_voi(prior, k, m, s, tier, fine, pf, rl, found, nd, nd - found) * V0
        if (free or voi > fee) and g.cash - (0 if free else fee) >= r:
            ok = run.appraise(it) if free else _pay_hint(run, it, fee)
            if ok:
                st["hints"] += 1
                if k_unfound > 0:
                    hint = "shown"
                    k_unfound -= 1
                else:
                    hint = "none"
                    if fl < nl:
                        fl += 1
                    elif fb < nb:
                        fb += 1
                    found = fl + fb
    missed = nd - found
    st["items"] += 1
    st["defects"] += nd
    st["found"] += found
    st["missed"] += missed
    # chọn thật hay giả: Bayes trên số dấu giả đã thấy (gợi ý chỉ thêm thông tin, dấu do gợi ý vẫn tính là đã thấy)
    p = posterior(prior, k, m, s, rl, hint)
    claim_fake = decide_fake(p, tier, fine, pf, rl, found, nd, missed)
    V = run.known_price(it)              # giá thẩm định: giá nền x tình trạng x hiệu ứng tạo tác
    if not claim_fake and found and rl["repair_light"] is not None:
        V = repair_shop(run, it, r, tier, p, base, nl, nb, fl, fb, rl, st, V)   # nhà vá lỗi: sửa lỗi đã biết
    st["fair"] += V if not it["fake"] else math.floor(V * rl["fake_x"])
    pen_unit = math.ceil(V * rl["miss_pen"])
    if claim_fake:
        amount = math.floor(V * rl["fake_x"])
        st["quick"] += 1
        if not it["fake"]:
            st["real_as_fake"] += 1
            st["loss_real_as_fake"] += V - amount
    elif it["fake"]:
        st["fake_as_real"] += 1
        amount = math.floor(V * rl["fake_x"]) - math.ceil(V * fine)
        st["fines"] += math.ceil(V * fine)
    else:
        if tier == "T" or pf["skip_haggle"] or haggle_ev(haggle_win(pf, found, nd), rl, tier) <= rl["open"]:
            mult = rl["open"]
            st["quick"] += 1
        else:
            mult, rounds, ok_, bad = haggle(run, pf, rl, tier, found, nd)
            st["haggles"] += 1
            st["rounds"] += rounds
        amount = math.floor(V * mult) - missed * pen_unit
        st["pen"] += missed * pen_unit
    st["revenue"] += amount
    if amount >= 0:
        g.cash += amount
    else:                                # phạt lớn hơn tiền đang có: phần thiếu thành nợ tồn
        pay = min(g.cash, -amount)
        g.cash -= pay
        g.arrears += -amount - pay


# --- đo ------------------------------------------------------------------------------------------------
BOT = ("thận trọng", "liều")
SKILLED = ("thận trọng + giám định", "liều + giám định", "combo nhỏ + giám định", "combo lớn + giám định")


def _play(content, strat, n, skilled=False):
    old = dict(C.SKILL)
    if skilled:
        C.SKILL.update(lam=.75, tau=1.0, per_box=True)
    w = 0
    agg = {}
    try:
        for s in range(n):
            run = P.play5(s, content, strat)
            w += run.g.status == "won"
            for k, v in stats(run).items():
                agg[k] = agg.get(k, 0) + v
    finally:
        C.SKILL.clear()
        C.SKILL.update(old)
    return round(100 * w / n, 1), agg


def summarize(agg, n):
    it = max(1, agg.get("items", 0))
    return dict(found_rate=round(100 * agg["found"] / max(1, agg["defects"])), missed_per_item=round(agg["missed"] / it, 2),
                pen_per_run=round(agg["pen"] / n), repairs_per_run=round(agg["repairs"] / n, 2),
                repair_spend_per_run=round(agg["repair_spend"] / n), hints_per_run=round(agg["hints"] / n, 2),
                haggle_share=round(100 * agg["haggles"] / it),
                real_as_fake=round(100 * agg["real_as_fake"] / it, 1), fake_as_real=round(100 * agg["fake_as_real"] / it, 1),
                fines_per_run=round(agg["fines"] / n), loss_per_run=round(agg["loss_real_as_fake"] / n),
                rev_vs_fair=round(agg["revenue"] / max(1, agg["fair"]), 3))


def evaluate(rules=None, profiles=("thường", "giỏi", "ẩu", "bỏ qua soi"), n=1000, design=None, skilled_profile="giỏi",
             bots=BOT, skilled=SKILLED):
    """Số đo một bộ luật: bot thường (bậc thang) theo từng hồ sơ soi, người chơi tính toán hoàn hảo với hồ sơ giỏi."""
    out = {"bot": {}, "skilled": {}}
    for prof in profiles:
        c = apply(design, rules, prof)
        for b in bots:
            win, agg = _play(c, P.STRATS[b], n)
            out["bot"][(b, prof)] = dict(win=win, **summarize(agg, n))
    if skilled:
        for prof in ((skilled_profile,) if isinstance(skilled_profile, str) else skilled_profile):
            c = apply(design, rules, prof)
            for k in skilled:
                win, agg = _play(c, C.TURN_NAMES[k], n, skilled=True)
                out["skilled"][(k, prof)] = dict(win=win, **summarize(agg, n))
    restore()
    return out


def day1(rules=None, profile="thường", design=None, n=1000, strat="liều"):
    c = apply(design, rules, profile)
    skilled = strat in C.TURN_NAMES
    st = C.TURN_NAMES[strat] if skilled else P.STRATS[strat]
    old = dict(C.SKILL)
    if skilled:
        C.SKILL.update(lam=.75, tau=1.0, per_box=True)
    try:
        r = CS.day1_stats(c, st, n)
    finally:
        C.SKILL.clear()
        C.SKILL.update(old)
    restore()
    return r


def fmt(out):
    lines = []
    for grp in ("bot", "skilled"):
        for (k, prof), v in out[grp].items():
            lines.append("%-8s %-24s %-10s thắng %5.1f%%  tìm lỗi %d%%  sót/món %.2f  phạt sót/ván %s  gợi ý/ván %.2f  trả giá %d%%  "
                         "thật→giả %.1f%%  giả→thật %.1f%%  phạt giả/ván %s  lỗ/ván %s  thu/giá đúng %.3f" % (
                             grp, k, prof, v["win"], v["found_rate"], v["missed_per_item"], fmt_vnd(v["pen_per_run"]),
                             v["hints_per_run"], v["haggle_share"], v["real_as_fake"], v["fake_as_real"],
                             fmt_vnd(v["fines_per_run"]), fmt_vnd(v["loss_per_run"]), v["rev_vs_fair"]))
    return "\n".join(lines)


if __name__ == "__main__":
    import sys
    n = int(sys.argv[1]) if len(sys.argv) > 1 else 300
    print(fmt(evaluate(n=n)))

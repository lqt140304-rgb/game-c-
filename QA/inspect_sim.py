"""Sprint 5 phần 3 (Claude 2): cân bằng soi lỗi và trả giá trên nền 4 rương cơ bản đã chốt (chest_sim.FINAL).

Spec luật: docs/superpowers/specs/2026-11-23-soi-loi-tra-gia-design.md; số lỗi theo bậc: tier-do-trong-ruong mục 4.
Spec cách đo và giả định: docs/superpowers/specs/2026-11-24-can-bang-soi-loi-design.md. Báo cáo: Docs/Balance/Sprint5SoiLoi.md.

Cách làm: giữ nguyên lõi đã duyệt (chest_sim nạp bảng 5 bậc vào proposal_sim / clue_sim / swing_sim) và chỉ thay bước
"mở xong thì bán" (P.handle5) bằng chuỗi soi lỗi -> phân loại -> trả giá. Rác và tạo tác đi như cũ.

LUẬT ĐỀ XUẤT (RULES, các con số cần chốt; tiền theo đơn vị 10.000 đ, in qua currency.fmt_vnd):
  - looks: lượt soi miễn phí theo bậc. Thường chỉ soi nhanh 1 lượt rồi bán nhanh (không trả giá).
  - points: số điểm lỗi có nghĩa trên món (tier-do-trong-ruong mục 4: Thường 1-2, Hiếm 2-3, Rất hiếm 3, Huyền thoại 3-4).
  - extra_fee / max_extra: phí mỗi lượt soi thêm theo bậc, tối đa bao nhiêu lượt thêm mỗi món.
  - quick: bán nhanh = quick x giá thẩm định V (V = giá nền x tình trạng x hiệu ứng tạo tác, như giá bán của lõi cũ).
  - open: giá chào mở màn trả giá (giá ép khi không có bằng chứng). Mỗi bằng chứng đưa đúng +up, đưa sai -down,
    tối đa `rounds` vòng, giá chốt trong [floor, cap] x V.
  - fake_x: món bị cho là giả (đúng hay sai) bán còn fake_x x V (mức lỗ "thật thành giả" = 1 - fake_x so với giá thật).
  - fine: giả mà khai thật thì cửa hàng kiểm tra lại, trả fake_x x V rồi PHẠT thêm fine x V (thiếu thì thành nợ tồn).
    Đề xuất fine = fake_x = 20%: món bị tịch thu, không nhận đồng nào.
  - fine_T: mức phạt riêng cho món Thường (chỉ soi nhanh 1 lượt, bán nhanh); None = như fine.
  - day1_fine: mức phạt ngày 1 (spec: ngày 1 giảm nhẹ hoặc miễn phạt).
  - cert: giữ chứng nhận +25% của giám định cũ hay không. Mặc định False: soi lỗi THAY giám định bằng chữ; Giáo Sư Lẫm
    chỉ bán "gợi ý một lỗi" (một bằng chứng chắc đúng), phí hint_rate x giá nền, tối thiểu 200.000 đ
    (None = phí giám định cũ 15%). Combo CB3 vẫn cho một lần miễn phí mỗi ngày như lõi cũ.
  - skip_quick: giá "bán không soi" nếu có tùy chọn bỏ qua hoàn toàn soi lỗi (None = như bán nhanh).
  Bỏ trả giá giữa chừng = chốt giá chào hiện tại (không quay về giá bán nhanh), nên đưa sai bằng chứng có giá thật.

MÔ HÌNH DẤU HIỆU (giả định nội dung, Claude 1 / Antigravity làm thật):
  - Mỗi lượt soi: với xác suất h (kỹ năng) trúng một điểm lỗi có nghĩa chưa soi, còn lại soi vào chỗ nhiễu.
  - Điểm có nghĩa: món giả hiện dấu giả với xác suất sig_fake; món thật hiện "dấu lạ" giống giả với xác suất sig_real.
  - Chỗ nhiễu hiện dấu giả với xác suất noise_fake bất kể thật giả. Người chơi đọc nhầm (lật) với xác suất e.
  - Người chơi tính xác suất giả theo Bayes với tỉ lệ hàng giả của rương (UI hiển thị) và kỹ năng của chính mình.

HỒ SƠ NGƯỜI CHƠI (PROFILES, giả định về người): giỏi / thường (bot thường, không soi kỹ) / ẩu / bỏ qua soi.
"""
import math
import chest_sim as CS
import clue_sim as C
import proposal_sim as P
import sprint3_sim as sm
from currency import fmt_vnd

TIERS = ("T", "H", "X", "L")
RULES = dict(
    looks=dict(T=1, H=3, X=4, L=4),           # lượt soi miễn phí; Thường chỉ soi nhanh 1 lượt
    points=dict(T=2, H=3, X=3, L=4),          # số điểm lỗi có nghĩa (tier-do-trong-ruong mục 4)
    extra_fee=dict(T=0, H=20, X=30, L=50),    # 200.000 / 300.000 / 500.000 đ mỗi lượt soi thêm
    max_extra=2,
    quick=1.0,                                # bán nhanh = 100% giá thẩm định (Thường luôn bán nhanh)
    open=.95, up=.08, down=.05,               # giá chào mở màn 95% (bị ép khi chưa đưa bằng chứng), +8% / -5% mỗi lần
    rounds=3, floor=.70, cap=1.12,            # 3 vòng, giá chốt trong 70% đến 112% giá thẩm định
    fake_x=.20,                               # khai giả: bán bằng giá hàng giả (lỗ 80% nếu món thật)
    fine=.20, fine_T=None, day1_fine=0.0,     # giả mà khai thật: phạt 20% = đúng giá hàng giả, tức bị tịch thu (0 đ); ngày 1 miễn phạt
    hint_rate=.05,                            # gợi ý một lỗi của Giáo Sư Lẫm: 5% giá nền, tối thiểu 200.000 đ (None = phí giám định cũ 15%)
    cert=False, skip_quick=None,              # skip_quick: giá "bán không soi" (None = như bán nhanh)
    sig_fake=.85, sig_real=.10, noise_fake=.25,
)
PROFILES = {
    # h: tỉ lệ lượt soi trúng điểm có nghĩa; e: đọc nhầm; c: đưa đúng bằng chứng khi trả giá; cond: nhầm tình trạng;
    # rule: "opt" quyết theo giá trị kỳ vọng, "half" khai giả khi xác suất giả > 50%, "real" luôn khai thật.
    # looks_cap: số lượt soi tối đa người đó chịu dùng; extra: có mua lượt soi thêm; skip_haggle: luôn bán nhanh.
    "giỏi": dict(h=.90, e=.03, c=.95, cond=0.0, rule="opt", looks_cap=9, extra=True, skip_haggle=False),
    "thường": dict(h=.65, e=.10, c=.75, cond=.05, rule="half", looks_cap=9, extra=False, skip_haggle=False),
    "ẩu": dict(h=.40, e=.20, c=.50, cond=.15, rule="half", looks_cap=1, extra=False, skip_haggle=False),
    "bỏ qua soi": dict(h=0, e=0, c=0, cond=0.0, rule="real", looks_cap=0, extra=False, skip_haggle=True),
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
def sign_probs(pf, rl):
    """(P(dấu giả | giả), P(dấu giả | thật)) cho một lượt soi của người có hồ sơ pf."""
    h, e = pf["h"], pf["e"]

    def read(x):
        return x * (1 - e) + (1 - x) * e
    return (h * read(rl["sig_fake"]) + (1 - h) * read(rl["noise_fake"]),
            h * read(rl["sig_real"]) + (1 - h) * read(rl["noise_fake"]))


def posterior(prior, n_fs, n_cl, pf, rl, sure_fs=0, sure_cl=0):
    """P(giả | n_fs dấu giả, n_cl dấu sạch) theo Bayes; sure_* là bằng chứng chắc đúng (gợi ý của Giáo Sư Lẫm)."""
    if prior <= 0:
        return 0.0
    a, b = sign_probs(pf, rl)
    lf = prior * a ** n_fs * (1 - a) ** n_cl * rl["sig_fake"] ** sure_fs * (1 - rl["sig_fake"]) ** sure_cl
    lr = (1 - prior) * b ** n_fs * (1 - b) ** n_cl * rl["sig_real"] ** sure_fs * (1 - rl["sig_real"]) ** sure_cl
    return lf / (lf + lr)


def n_rounds(rl, tier):
    r = rl["rounds"]
    return r[tier] if isinstance(r, dict) else r


def haggle_ev(good, pf, rl, tier="H"):
    """Hệ số giá kỳ vọng khi trả giá với `good` bằng chứng tốt (người chơi tự ước lượng)."""
    k = min(good, n_rounds(rl, tier))
    m = rl["open"] + k * (pf["c"] * rl["up"] - (1 - pf["c"]) * rl["down"])
    return max(rl["floor"], min(rl["cap"], m))


def real_value(tier, good, pf, rl):
    """Hệ số giá kỳ vọng nếu khai thật và món thật: bán nhanh hoặc trả giá, chọn cái tốt hơn."""
    if tier == "T" or pf["skip_haggle"]:
        return rl["quick"]
    return max(rl["quick"], haggle_ev(good, pf, rl, tier))


def decide_fake(p, tier, good, fine, pf, rl):
    """True nếu người chơi khai giả."""
    if pf["rule"] == "real" or p <= 0:
        return False
    if pf["rule"] == "half":
        return p > .5
    v_real = (1 - p) * real_value(tier, good, pf, rl) + p * (rl["fake_x"] - fine)
    return v_real < rl["fake_x"]


class Look:
    """Trạng thái soi của một món: số dấu giả / sạch đã thấy, bằng chứng tốt, điểm có nghĩa còn lại."""
    def __init__(self, n_points):
        self.fs = self.cl = self.sure_fs = self.sure_cl = 0
        self.good = 0          # điểm có nghĩa đọc đúng chiều "thật" (dùng phản bác khi món thật)
        self.noise = 0         # lượt soi vào chỗ nhiễu (người ẩu có thể đem ra trả giá và bị bắt bẻ)
        self.left = n_points
        self.n = 0


def one_look(run, it, lk, pf, rl):
    rng = run.rng
    lk.n += 1
    if lk.left > 0 and rng.random() < pf["h"]:
        lk.left -= 1
        x = rl["sig_fake"] if it["fake"] else rl["sig_real"]
        shows = rng.random() < x
        read = shows if rng.random() >= pf["e"] else not shows
        if read:
            lk.fs += 1
        else:
            lk.cl += 1
            if not it["fake"]:
                lk.good += 1
    else:
        lk.noise += 1
        shows = rng.random() < rl["noise_fake"]
        read = shows if rng.random() >= pf["e"] else not shows
        if read:
            lk.fs += 1
        else:
            lk.cl += 1


def look_voi(prior, lk, tier, fine, pf, rl):
    """Giá trị thông tin (theo hệ số của V) của thêm một lượt soi, theo ước lượng của chính người chơi."""
    def best(p, good):
        fake = decide_fake(p, tier, good, fine, dict(pf, rule="opt"), rl)
        return rl["fake_x"] if fake else (1 - p) * real_value(tier, good, pf, rl) + p * (rl["fake_x"] - fine)
    p = posterior(prior, lk.fs, lk.cl, pf, rl, lk.sure_fs, lk.sure_cl)
    now = best(p, lk.good)
    a, b = sign_probs(pf, rl)
    pfs = p * a + (1 - p) * b
    p1 = posterior(prior, lk.fs + 1, lk.cl, pf, rl, lk.sure_fs, lk.sure_cl)
    p0 = posterior(prior, lk.fs, lk.cl + 1, pf, rl, lk.sure_fs, lk.sure_cl)
    g_up = 1 if lk.left > 0 else 0   # một lượt "sạch" trên món thật có thể là bằng chứng thêm
    after = pfs * best(p1, lk.good) + (1 - pfs) * best(p0, lk.good + g_up * pf["h"])
    return after - now


def haggle(run, it, lk, pf, rl, tier):
    """Trả giá kiểu Ace Attorney trên món thật được khai thật. Trả (hệ số giá, số vòng, đúng, sai)."""
    rng = run.rng
    m, ok, bad, rounds = rl["open"], 0, 0, 0
    good, noise = lk.good + lk.sure_cl, lk.noise
    for _ in range(n_rounds(rl, tier)):
        if good > 0:
            good -= 1
            rounds += 1
            if rng.random() < pf["c"]:
                m += rl["up"]
                ok += 1
            else:
                m -= rl["down"]
                bad += 1
        elif pf["rule"] != "opt" and noise > 0 and rng.random() > pf["c"]:
            noise -= 1                 # đem "bằng chứng" nhiễu ra, bị bắt bẻ
            rounds += 1
            m -= rl["down"]
            bad += 1
        else:
            break
    return max(rl["floor"], min(rl["cap"], m)), rounds, ok, bad


def stats(run):
    if not hasattr(run, "ins"):
        run.ins = dict(items=0, looks=0, extra=0, extra_fee=0, hints=0, haggles=0, rounds=0, quick=0,
                       real_as_fake=0, fake_as_real=0, fake_caught=0, fines=0, loss_real_as_fake=0, cond_err=0,
                       revenue=0, fair=0)
    return run.ins


def _pay_hint(run, it, fee):
    if run.g.cash < fee:
        return False
    run.g.cash -= fee
    run.apprs += 1
    it["appraised"] = True
    return True


# --- thay bước bán ------------------------------------------------------------------------------------
def handle(run, it, r, smart):
    """Thay P.handle5: hộp sắp sẵn, rác, tạo tác như cũ; hiện vật thì soi lỗi, phân loại, trả giá."""
    run.g.cash += CS.script_refund(run, it)
    if it["id"] in CS.JUNK or it["id"].startswith("AX_"):
        run.sell(it)
        return
    g, rl, pf = run.g, CFG["rules"], PROFILES[CFG["profile"]]
    st = stats(run)
    base = run.c.items[it["id"]][1]
    tier = TIER_OF.get(it["id"], "T")
    prior = CS.fake_rate(it["box"], it["id"])
    fine = rl["fine"] if tier != "T" or rl["fine_T"] is None else rl["fine_T"]
    if g.day == 1:
        fine = min(fine, rl["day1_fine"])
    # phục chế như lõi cũ
    pr = 1 - prior
    avg = pr + (1 - pr) * rl["fake_x"]
    if it["q"] < 1.0:
        nq = LEVELS[LEVELS.index(it["q"]) + 1]
        fee = run.restore_fee(base)
        if (nq - it["q"]) * base * avg > fee and g.cash - fee >= r:
            run.restore(it)
    # soi lỗi
    lk = Look(rl["points"][tier])
    for _ in range(min(rl["looks"][tier], pf["looks_cap"])):
        one_look(run, it, lk, pf, rl)
    if pf["extra"] and tier != "T":
        V0 = run.known_price(it)
        for _ in range(rl["max_extra"]):
            fee = rl["extra_fee"][tier]
            if g.cash - fee < r or look_voi(prior, lk, tier, fine, pf, rl) * V0 <= fee:
                break
            g.cash -= fee
            st["extra"] += 1
            st["extra_fee"] += fee
            one_look(run, it, lk, pf, rl)
    # gợi ý một lỗi của Giáo Sư Lẫm (thay giám định chọn lọc của lõi cũ)
    if smart and tier != "T" and not pf["skip_haggle"]:
        fee = run.appraise_fee(base) if rl["hint_rate"] is None else max(20, round(base * rl["hint_rate"]))
        free = run.cb3() and run.free_cnt.get(g.day, 0) < P.CB3_FREE
        V0 = run.known_price(it)
        p = posterior(prior, lk.fs, lk.cl, pf, rl, lk.sure_fs, lk.sure_cl)
        gain = (1 - p) * pf["c"] * rl["up"] * V0 if lk.good + lk.sure_cl < n_rounds(rl, tier) else 0
        gain += look_voi(prior, lk, tier, fine, dict(pf, h=1.0, e=0.0), rl) * V0
        if (free or gain > fee) and g.cash - (0 if free else fee) >= r:
            ok = run.appraise(it) if rl["hint_rate"] is None or free else _pay_hint(run, it, fee)
            if ok:
                st["hints"] += 1
                shows = run.rng.random() < (rl["sig_fake"] if it["fake"] else rl["sig_real"])
                if shows:
                    lk.sure_fs += 1
                else:
                    lk.sure_cl += 1
    st["items"] += 1
    st["looks"] += lk.n
    # phân loại
    p = posterior(prior, lk.fs, lk.cl, pf, rl, lk.sure_fs, lk.sure_cl)
    claim_fake = decide_fake(p, tier, lk.good + lk.sure_cl, fine, pf, rl)
    q_true = it["q"]
    q_paid = q_true
    cred = 0.0
    if pf["cond"] and run.rng.random() < pf["cond"]:
        st["cond_err"] += 1
        i = LEVELS.index(q_true)
        j = min(2, max(0, i + run.rng.choice((-1, 1))))
        if j < i:
            q_paid = LEVELS[j]          # khai thấp hơn thật: bị trả theo mức khai
        elif j > i:
            cred = rl["down"]           # khai cao hơn thật: bị bắt bẻ, mất uy tín
    it["q"] = q_paid
    V = run.known_price(it)              # giá thẩm định: giá nền x tình trạng x hiệu ứng tạo tác (cert nếu còn)
    it["q"] = q_true
    fair = run.known_price(it)
    st["fair"] += fair if not it["fake"] else math.floor(fair * rl["fake_x"])
    if claim_fake:
        amount = math.floor(V * rl["fake_x"])
        st["quick"] += 1
        if not it["fake"]:
            st["real_as_fake"] += 1
            st["loss_real_as_fake"] += fair - amount
    elif it["fake"]:
        st["fake_as_real"] += 1
        st["fake_caught"] += 1
        amount = math.floor(V * rl["fake_x"]) - math.ceil(V * fine)
        st["fines"] += math.ceil(V * fine)
    else:
        if tier == "T" or pf["skip_haggle"] or real_value(tier, lk.good + lk.sure_cl, pf, rl) <= rl["quick"]:
            m = rl["quick"]
            if pf["looks_cap"] == 0 and rl["skip_quick"] is not None:
                m = rl["skip_quick"]
            st["quick"] += 1
        else:
            m, rounds, ok, bad = haggle(run, it, lk, pf, rl, tier)
            st["haggles"] += 1
            st["rounds"] += rounds
        amount = math.floor(V * max(rl["floor"], m - cred))
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
    return dict(looks_per_item=round(agg["looks"] / it, 2), extra_per_run=round(agg["extra"] / n, 2),
                hints_per_run=round(agg["hints"] / n, 2), haggle_share=round(100 * agg["haggles"] / it),
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
            lines.append("%-8s %-24s %-10s thắng %5.1f%%  soi/món %.2f  thêm/ván %.2f  gợi ý/ván %.2f  trả giá %d%%  "
                         "thật→giả %.1f%%  giả→thật %.1f%%  phạt/ván %s  lỗ/ván %s  thu/giá đúng %.3f" % (
                             grp, k, prof, v["win"], v["looks_per_item"], v["extra_per_run"], v["hints_per_run"], v["haggle_share"],
                             v["real_as_fake"], v["fake_as_real"], fmt_vnd(v["fines_per_run"]), fmt_vnd(v["loss_per_run"]), v["rev_vs_fair"]))
    return "\n".join(lines)


if __name__ == "__main__":
    import sys
    n = int(sys.argv[1]) if len(sys.argv) > 1 else 300
    print(fmt(evaluate(n=n)))

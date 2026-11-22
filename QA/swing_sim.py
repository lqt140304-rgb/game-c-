"""Đo phương án C (giá hộp dao động theo ngày) bằng bộ chiến thuật đầy đủ, luật dịch vụ và biến cố (Claude 2).

Luật (NoiDungMau.md mục 5.3, 5.4; DieuChinhSauCanBang.md mục K): giá hôm nay của từng loại hộp = giá gốc x (1 + u),
u đều trong [-a, +a], xác định theo (seed, ngày, loại hộp), công bố đầu ngày, áp dụng từ ngày 1. E3 giảm thêm 20% giá H2.
Giá niêm yết = giá hôm nay x hệ số theo manh mối đầu (lam 0,75). Lời mách Dì Mận: trả 15 xu biết chắc nhóm nguồn gốc thật.
Chạy: python3 swing_sim.py  (khoảng 40 phút, nên chạy nền)
"""
import random
import clue_sim as C
import event_sim as E
import proposal_sim as P
import risk_full as F
import sprint3_sim as sm
from sprint3_sim import Content, ITEMS

TIERS = ("H1", "H2", "H3", "RD")
SW = dict(amp=0.0, base=dict(H1=100, H2=350, H3=800, RD=1500))
_orig_start = P.start_hooks
_orig_end = P.end_hooks


def swing(seed, day, box):
    if SW["amp"] == 0:
        return 1.0
    r = random.Random(f"{seed}-boxprice-{day}-{box}")
    return 1 + SW["amp"] * (2 * r.random() - 1)


def start_hooks(run):
    _orig_start(run)
    for b in TIERS:
        run.c.box_price[b] = max(1, round(SW["base"][b] * swing(run.g.seed, run.g.day, b)))


def end_hooks(run):
    _orig_end(run)
    if run.opt.get("casino") and run.g.day >= 2:
        g = run.g
        need = run.need()
        for _ in range(2):
            if g.cash >= need or g.cash < 100:
                break
            stake = max(s for s in (100, 250, 500) if s <= g.cash)
            if run.rng.random() < .45:
                g.cash += stake
            else:
                g.cash -= stake


P.start_hooks = start_hooks
P.end_hooks = end_hooks
P.K_NEW[:] = [(.2, .9), (.4, 1.1), (.3, 1.3), (.1, 1.5)]      # k kỳ vọng 1,16 theo DichVuVaMeta.md bản chốt


def content(rd_price=1500):
    SW["base"]["RD"] = rd_price
    return Content(item_mult={k: F.X110[k] / ITEMS[k][1] for k in ITEMS}, box_price=dict(SW["base"]))


def mixes(c, st, n):
    mix = {b: 0 for b in TIERS}
    orig = C.turn_choose

    def spy(run, rc, tiers, _o=orig):
        it = _o(run, rc, tiers)
        if it:
            mix[it["box"]] += 1
        return it
    C.turn_choose = spy
    try:
        for s in range(n):
            P.play5(s, c, st)
    finally:
        C.turn_choose = orig
    tot = sum(mix.values()) or 1
    return " ".join("%s %2.0f%%" % (b, 100 * mix[b] / tot) for b in TIERS)


def run_config(amp, rd_price, n=1500, events=False, opt=None, label=None):
    SW["amp"] = amp
    E.CFG.update(on=events, adaptive=True, allowed=E.ALL)
    c = content(rd_price)
    C.setup(True)
    ks = list(C.TURN_NAMES)
    lad = P.measure(c, n=n, opt=opt)
    free = C.measure_turns(c, n, lam=.75, tau=1.0)
    mach = C.measure_turns(c, n, lam=.75, tau=1.0, paid_true=True, clue_price=15)
    tag = label or ("±%d%%, RD %d%s" % (round(100 * amp), rd_price, ", có biến cố" if events else ""))
    print("== %s" % tag, flush=True)
    print("   bậc thang (mua hộp đắt nhất): " + " | ".join("%s %.1f" % (k.replace(" giám định chọn lọc", " gđ"), lad[k]["win"]) for k in lad), flush=True)
    print("   người chơi chọn loại + manh mối miễn phí: " + " / ".join("%.1f" % free[k] for k in ks), flush=True)
    print("   + lời mách Dì Mận 15 xu:                 " + " / ".join("%.1f" % mach[k] for k in ks), flush=True)
    print("   cách chọn hộp (thận trọng + gđ): " + mixes(c, C.TURN_NAMES["thận trọng + giám định"], min(n, 800)), flush=True)
    return lad, free, mach


def main(n=1500):
    for rd in (1500, 1250):
        for amp in (0.0, 0.15, 0.20):
            run_config(amp, rd, n)
    print("\n== Luật dịch vụ (±20%, RD 1500)", flush=True)
    for label, opt in (("đấu giá k 1,16, món từ 1.300, sàn 1,0", dict(auction=dict(min_g=1300, floor=1.0))),
                       ("cầm tạo tác", dict(pawn_art=True)),
                       ("sòng bạc 100/250/500, 2 lượt", dict(casino=True))):
        run_config(0.20, 1500, n, opt=opt, label="dịch vụ: " + label)
    print("\n== Có biến cố E1-E6 (người chơi thích nghi)", flush=True)
    run_config(0.20, 1500, n, events=True)
    run_config(0.20, 1250, n, events=True)


if __name__ == "__main__":
    main()

"""Thử giá hộp dao động theo ngày cho D-29 và D-30 (Claude 1), trên mô hình của Claude 2.

Ý tưởng: giữ nguyên lời kỳ vọng trung bình của từng loại hộp, nhưng mỗi ngày giá mỗi loại lệch ngẫu nhiên
trong khoảng cộng trừ a (công bố đầu ngày, xác định theo seed, ngày, loại hộp). Khi đó loại hộp tốt nhất
đổi theo ngày, người chơi phải tính lại mỗi ngày thay vì có một đáp án cố định.
Đo: cách chọn loại hộp, tỷ lệ thắng của bot thường (mua hộp đắt nhất mua được) và người chơi tối ưu
(chọn hộp có lời tuyệt đối cao nhất). Chạy trong thư mục QA (khoảng 6 phút): python3 box_price_swing.py
"""
import random
import clue_sim as C
import proposal_sim as P
import risk_penalty as R
from sprint3_sim import Content, ITEMS
import risk_full as F

N = 1200
TIERS = ("H1", "H2", "H3", "RD")
BASE_PRICE = dict(H1=100, H2=350, H3=800, RD=1500)
AMP = {"amp": 0.0}
_orig_start = P.start_hooks


def swing(seed, day, box):
    if AMP["amp"] == 0:
        return 1.0
    r = random.Random(f"{seed}-boxprice-{day}-{box}")
    return 1 + AMP["amp"] * (2 * r.random() - 1)


def start_hooks(run):
    _orig_start(run)
    for b in TIERS:
        run.c.box_price[b] = max(1, round(BASE_PRICE[b] * swing(run.g.seed, run.g.day, b)))


P.start_hooks = start_hooks


def content():
    return Content(item_mult={k: F.X110[k] / ITEMS[k][1] for k in ITEMS}, box_price=dict(BASE_PRICE))


def run_amp(a):
    AMP["amp"] = a
    C.setup(True)
    c = content()
    mix = {b: 0 for b in TIERS}
    days = {"ngày 1-3": (1, 3), "ngày 4-7": (4, 7)}
    dmix = {k: {b: 0 for b in TIERS} for k in days}
    orig = C.turn_choose

    def spy(run, rc, tiers, _o=orig):
        it = _o(run, rc, tiers)
        if it:
            mix[it["box"]] += 1
            for k, (lo, hi) in days.items():
                if lo <= run.g.day <= hi:
                    dmix[k][it["box"]] += 1
        return it

    C.turn_choose = spy
    try:
        skilled = C.measure_turns(c, N, lam=.75, tau=1.0)
    finally:
        C.turn_choose = orig
    plain = C.measure_plain(c, N)
    tot = sum(mix.values()) or 1
    print("== biên độ ±%d%%" % round(100 * a))
    print("   người chơi tối ưu chọn: " + " / ".join("%s %2.0f%%" % (b, 100 * mix[b] / tot) for b in TIERS))
    for k in days:
        t = sum(dmix[k].values()) or 1
        print("      %-9s " % k + " / ".join("%s %2.0f%%" % (b, 100 * dmix[k][b] / t) for b in TIERS))
    sk = [k for k in skilled if not k.endswith("[mua]")]
    print("   thắng% người chơi tối ưu: " + " / ".join("%s %.1f" % (k.split(" +")[0], skilled[k]) for k in sk))
    print("   thắng%% bot thường (mua hộp đắt nhất): thận trọng %.1f / liều %.1f / thận trọng+gđ %.1f / liều+gđ %.1f"
          % (plain["thận trọng"], plain["liều"], plain["thận trọng + giám định"], plain["liều + giám định"]), flush=True)


if __name__ == "__main__":
    for a in (0.0, 0.10, 0.20, 0.30):
        run_amp(a)

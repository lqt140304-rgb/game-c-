"""Thử cân bằng lại giá các loại hộp cho D-29 và D-30 (Claude 1), trên mô hình clue_sim.py của Claude 2.

Vấn đề: ở cấu hình đã duyệt, lời tuyệt đối mỗi hộp là H1 150, H2 206, H3 192, Rương Đen 15, nên người chơi
chọn theo lời tuyệt đối luôn chọn H2 (66 đến 71% lượt mua) và không bao giờ mua Rương Đen.
Ở đây chỉ đổi GIÁ hộp (hiện vật, tỷ lệ rơi, tỷ lệ hàng giả giữ nguyên) và đo cách chọn hộp cùng tỷ lệ thắng.
Người chơi tối ưu: mỗi lượt chọn hộp có lời tuyệt đối cao nhất trong số hộp mua được (clue_sim.turn_choose).
Chạy trong thư mục QA (khoảng 8 phút):  python3 box_tiers.py
"""
import clue_sim as C
import risk_penalty as R
from sprint3_sim import Content, ITEMS
import risk_full as F

N = 1200
TIERS = ("H1", "H2", "H3", "RD")
PROFILES = [
    ("0. Hiện tại: 100 / 350 / 800 / 1500", dict(H1=100, H2=350, H3=800, RD=1500)),
    ("1. 120 / 400 / 700 / 1200", dict(H1=120, H2=400, H3=700, RD=1200)),
    ("2. 130 / 350 / 670 / 1065 (mục tiêu Claude 2)", dict(H1=130, H2=350, H3=670, RD=1065)),
    ("3. 100 / 400 / 650 / 1000", dict(H1=100, H2=400, H3=650, RD=1000)),
    ("4. 120 / 450 / 700 / 1100", dict(H1=120, H2=450, H3=700, RD=1100)),
]


def content_with(prices):
    return Content(item_mult={k: F.X110[k] / ITEMS[k][1] for k in ITEMS}, box_price=prices)


def run_profile(label, prices):
    c = C.setup(True)
    c = content_with(prices)
    ev = R.box_ev(c)
    profit = {b: round(ev[b] * prices[b] - prices[b]) for b in TIERS}
    mix = {b: 0 for b in TIERS}
    orig = C.turn_choose

    def spy(run, rc, tiers, _o=orig):
        it = _o(run, rc, tiers)
        if it:
            mix[it["box"]] += 1
        return it

    C.turn_choose = spy
    try:
        res = C.measure_turns(c, N, lam=.75, tau=1.0)
    finally:
        C.turn_choose = orig
    tot = sum(mix.values()) or 1
    print("== %s" % label)
    print("   lời kỳ vọng mỗi hộp: " + " / ".join("%s %d" % (b, profit[b]) for b in TIERS)
          + "   lần giá hộp: " + " / ".join("%.2f" % ev[b] for b in TIERS))
    print("   tỷ lệ được chọn:      " + " / ".join("%s %2.0f%%" % (b, 100 * mix[b] / tot) for b in TIERS))
    names = [k for k in res if not k.endswith("[mua]")]
    print("   thắng%%: " + " / ".join("%s %.1f" % (k.split(" +")[0], res[k]) for k in names), flush=True)


if __name__ == "__main__":
    for label, prices in PROFILES:
        run_profile(label, prices)

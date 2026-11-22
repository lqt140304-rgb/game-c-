"""Sinh toàn bộ số liệu cho Docs/Balance/Sprint3.md. Chạy: python3 sprint3_report.py (mất ~1 phút)."""
import economy_model as em
import sprint3_sim as sm
from sprint3_sim import *

N = 10000
# Giá nền đề xuất của Claude 2 (đã làm tròn). Mục tiêu giá trị kỳ vọng sau chất lượng và hàng giả:
# H1 2,6 lần giá hộp, H2 1,7 lần, H3 1,4 lần, RD 1,25 lần.
PRICES = {"V01": 250, "V02": 410, "V03": 530, "V04": 440, "V05": 330,
          "V06": 1970, "V07": 2880, "V08": 1700, "V09": 960, "V10": 3060}
PROPOSED = Content(item_mult={k: PRICES[k] / ITEMS[k][1] for k in ITEMS})


def ev(content):
    qm = sum(p * f for _, p, f in content.quality)
    out = {}
    for b, (price, w) in BOXES.items():
        tot = sum(w.values()); e = 0
        for i, x in w.items():
            fp = FAKE_P_V09 if i == "V09" else FAKE_P
            e += x / tot * content.items[i][1] * qm * (1 - fp + fp * FAKE_X)
        out[b] = "%d xu = %.2f lần" % (round(e), e / content.box_price[b])
    return out


def wr(content, strat, n=N, gt=None):
    return round(100 * sum(play(s, content, strat, gt).g.status == "won" for s in range(n)) / n, 1)


def setcombo(all_origin=False, bonus=1, price=1):
    sm.ALL_ORIGIN_BONUS, sm.BONUS_SCALE, sm.ART_PRICE_SCALE = all_origin, bonus, price


if __name__ == "__main__":
    print("== Giá trị kỳ vọng mỗi hộp")
    print("Gốc   :", ev(Content()))
    print("Đề xuất:", ev(PROPOSED))

    setcombo()
    report(Content(), N, "\n== A. Nội dung gốc của Claude 1")
    report(PROPOSED, N, "\n== B. Giá nền đề xuất, tạo tác giữ nguyên")
    setcombo(True, 2, 1)
    report(PROPOSED, N, "\n== C. Giá nền đề xuất, CP1 và CB1 áp dụng mọi hiện vật, hiệu ứng x2 (+10% và +20%)")

    setcombo()
    print("\n== D. Phương án chỉnh riêng lẻ (giá nền đề xuất), thắng% thận trọng / săn combo / liều")
    row = lambda l: print("%-52s %5s %5s %5s" % (l, wr(PROPOSED, careful), wr(PROPOSED, combo_hunter), wr(PROPOSED, bold)))
    row("Luật nợ Claude 1 nguyên bản")
    em.KEEP_MIN_CASH = 100
    row("Chừa lại 100 xu khi thu nợ")
    em.KEEP_MIN_CASH = 0
    setcombo(False, 1, .5); row("Giá tạo tác x0,5")
    setcombo(True, 1, 1);   row("Hiệu ứng bán giá áp dụng mọi hiện vật")
    setcombo(True, 2, 1);   row("Mọi hiện vật và hiệu ứng x2")
    setcombo(True, 2, .5);  row("Mọi hiện vật, hiệu ứng x2, giá tạo tác x0,5")
    setcombo()

    print("\n== E. Giá trị gia truyền GT1 (phạt 10%) so với GT2, thắng% thận trọng")
    for kp in (0, 100):
        em.KEEP_MIN_CASH = kp
        print("chừa", kp, "GT1", wr(PROPOSED, careful, gt="GT1"), "GT2", wr(PROPOSED, careful, gt="GT2"))
    em.KEEP_MIN_CASH = 0

    print("\n== F. Doanh thu thêm nhờ hiệu ứng so với chi tạo tác (săn combo)")
    for lab, args in (("tạo tác giữ nguyên", (False, 1, 1)), ("mọi hiện vật, x2", (True, 2, 1))):
        setcombo(*args)
        rs = [play(s, PROPOSED, combo_hunter) for s in range(N)]
        print(lab, "chi", round(sum(r.art_spent for r in rs) / N), "thu thêm", round(sum(r.bonus_rev for r in rs) / N))
    setcombo()

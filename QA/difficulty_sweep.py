"""Quét độ khó: nhân giá nền đề xuất của Claude 2 với một hệ số và xem tỷ lệ thắng của bot.

Do Claude 1 thêm để chọn mục tiêu độ khó (Docs/Content/DieuChinhSauCanBang.md).
Bot không đọc manh mối, không soi, không giữ hàng chờ giá, nên tỷ lệ thắng của nó là SÀN.
Người chơi biết tính toán phải thắng cao hơn bot. Chạy trong thư mục QA (khoảng 30 giây):

    python3 difficulty_sweep.py
"""
import economy_model as em
import sprint3_sim as sm
from sprint3_sim import Content, play, careful, combo_hunter, bold, ITEMS

PRICES = {"V01": 250, "V02": 410, "V03": 530, "V04": 440, "V05": 330,
          "V06": 1970, "V07": 2880, "V08": 1700, "V09": 960, "V10": 3060}
N = 3000


def rates(scale):
    c = Content(item_mult={k: scale * PRICES[k] / ITEMS[k][1] for k in ITEMS})
    out = []
    for strat in (careful, combo_hunter, bold):
        out.append(100 * sum(play(s, c, strat).g.status == "won" for s in range(N)) / N)
    return out


def box_ev(scale):
    qm = sum(p * f for _, p, f in Content().quality)
    res = {}
    for b, (price, w) in sm.BOXES.items():
        tot = sum(w.values())
        e = 0
        for i, x in w.items():
            fp = sm.FAKE_P_V09 if i == "V09" else sm.FAKE_P
            e += x / tot * scale * PRICES[i] * qm * (1 - fp + fp * sm.FAKE_X)
        res[b] = e / price
    return res


if __name__ == "__main__":
    em.KEEP_MIN_CASH = 0
    for label, allorigin, bonus in (("tạo tác giữ nguyên", False, 1), ("CP1/CB1 mọi hiện vật, x2", True, 2)):
        sm.ALL_ORIGIN_BONUS, sm.BONUS_SCALE, sm.ART_PRICE_SCALE = allorigin, bonus, 1
        print("== %s" % label)
        print("%-8s %-28s %s" % ("hệ số", "lần giá hộp H1/H2/H3/RD", "thắng% thận trọng / săn combo / liều"))
        for scale in (1.0, 0.9, 0.8, 0.7, 0.6):
            ev = box_ev(scale)
            r = rates(scale)
            print("%-8s %-28s %5.1f / %5.1f / %5.1f" % (
                scale, "/".join("%.2f" % ev[b] for b in ("H1", "H2", "H3", "RD")), *r), flush=True)

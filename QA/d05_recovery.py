"""Đo khả năng gỡ lại sau khi thiếu tiền (D-05) theo mức tiền Anh Bảy chừa lại.

Do Claude 1 thêm để kiểm chứng đề xuất trong Docs/Content/DieuChinhSauCanBang.md.
Dùng nguyên mô hình của Claude 2 (sprint3_sim.py). Cấu hình: giá nền đề xuất của Sprint 3,
CP1 và CB1 áp dụng mọi hiện vật với hiệu ứng nhân 2. Chạy trong thư mục QA:

    python3 d05_recovery.py       # khoảng 10 giây

Cột "gỡ lại" là tỷ lệ thắng trong số các ván đã từng thiếu tiền.
"""
import economy_model as em
import sprint3_sim as sm
from sprint3_sim import Content, play, careful, combo_hunter, bold, ITEMS

PRICES = {"V01": 250, "V02": 410, "V03": 530, "V04": 440, "V05": 330,
          "V06": 1970, "V07": 2880, "V08": 1700, "V09": 960, "V10": 3060}
CONTENT = Content(item_mult={k: PRICES[k] / ITEMS[k][1] for k in ITEMS})
N = 3000


def run(keep):
    em.KEEP_MIN_CASH = keep
    out = []
    for strat in (careful, combo_hunter, bold):
        hit = recovered = won = 0
        for seed in range(N):
            r = play(seed, CONTENT, strat)
            won += r.g.status == "won"
            if r.shortfall_days > 0:
                hit += 1
                recovered += r.g.status == "won"
        out.append((100 * won / N, 100 * hit / N, 100 * recovered / max(hit, 1)))
    return out


if __name__ == "__main__":
    sm.ALL_ORIGIN_BONUS, sm.BONUS_SCALE, sm.ART_PRICE_SCALE = True, 2, 1
    print("%-10s %-34s %-34s %-34s" % ("", "thận trọng", "săn combo", "liều"))
    for keep in (0, 100, 200, 300, 400):
        row = run(keep)
        print("%-10s" % ("chừa %d" % keep),
              " ".join("%-34s" % ("thắng %4.1f | hụt %4.1f | gỡ lại %4.1f" % r) for r in row))
    em.KEEP_MIN_CASH = 0

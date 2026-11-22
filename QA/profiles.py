"""Cấu hình cân bằng theo mục tiêu 'ván khó, nhiều đánh đổi'. Claude 2."""
import sprint3_sim as sm
from sprint3_sim import Content, ITEMS

# Giá nền đề xuất cho ván khó (làm tròn). Giá trị kỳ vọng mỗi hộp so với giá hộp: H1 2,19 H2 1,51 H3 1,25 RD 1,10.
HARD_PRICES = {"V01": 210, "V02": 340, "V03": 450, "V04": 390, "V05": 290,
               "V06": 1750, "V07": 2460, "V08": 1550, "V09": 800, "V10": 2790}
HARD = Content(item_mult={k: HARD_PRICES[k] / ITEMS[k][1] for k in ITEMS})
HARD_BONUS_SCALE = 1.25   # CP1 +6,25%, CB1 +12,5% giá bán, áp dụng mọi hiện vật


def use_hard():
    sm.ALL_ORIGIN_BONUS, sm.BONUS_SCALE, sm.ART_PRICE_SCALE = True, HARD_BONUS_SCALE, 1
    return HARD

"""Sprint 2: lịch nợ, combo, tiến độ Bí Khế, tải lại ván.

Luật nợ lấy từ đề xuất của Claude 1 (GDD mục 2, commit 643ae03). Combo và Bí Khế dùng
GIÁ TRỊ TẠM trong economy_model.py vì nội dung Sprint 2 của Claude 1 chưa có.
Khi nội dung chốt, chỉ cần sửa bảng ARTIFACTS, BI_KHE, COMBO_*.
"""
import unittest
from economy_model import *


def g_(**kw):
    return Game(seed=11, **kw)


def day(g, cash, d=1, arrears=0):
    g.day, g.cash, g.arrears = d, cash, arrears
    return g


class DebtSchedule(unittest.TestCase):
    """Bảy ca của Claude 1 (DL-1..DL-7) và các ca bổ sung."""

    def test_dl1_pay_on_time(self):
        g = day(g_(), 1000)
        g.end_day()
        self.assertEqual((g.cash, g.arrears, g.status), (800, 0, "playing"))

    def test_dl2_short_150_of_200(self):
        g = day(g_(), 150)
        g.end_day()
        self.assertEqual((g.cash, g.arrears), (0, 60))  # 50 + phạt 10

    def test_dl3_arrears_carry_into_next_period(self):
        g = day(g_(), 100, d=2, arrears=60)  # kỳ 350 + tồn 60 = 410
        g.end_day()
        self.assertEqual((g.cash, g.arrears), (0, 372))  # 310 + phạt 62

    def test_dl4_lose_when_arrears_over_limit(self):
        g = day(g_(), 0, d=3, arrears=900)  # 500 + 900 = 1400, phạt 280 -> 1680
        r = g.end_day()
        self.assertEqual((r["result"], g.status), ("lost", "lost"))

    def test_dl5_lose_end_day7_with_arrears(self):
        g = day(g_(), 2200, d=7, arrears=50)
        g.end_day()
        self.assertEqual((g.status, g.lose_reason), ("lost", "arrears_on_day7"))

    def test_dl6_win_day7_clean(self):
        g = day(g_(), 2200, d=7)
        g.end_day()
        self.assertEqual(g.status, "won")

    def test_dl7_unsold_items_do_not_pay(self):
        g = day(g_(), 0)
        g.items[g._id()] = {"value": 3000, "fake": False, "origin": "x", "appraised": False}
        g.end_day()
        self.assertEqual(g.arrears, 240)  # 200 + phạt 40

    def test_dl8_penalty_rounds_up(self):
        g = day(g_(), 199)
        g.end_day()
        self.assertEqual(g.arrears, 2)  # tồn 1, phạt ceil(0.2)=1

    def test_dl9_penalty_not_on_old_penalty(self):
        # Hai kỳ liên tiếp trả 0: phạt tính trên tồn mới, không tính lãi kép riêng.
        g = day(g_(), 0)
        g.end_day()                 # tồn 200 + 40 = 240
        g.end_day()                 # 350 + 240 = 590, phạt 118 -> 708
        self.assertEqual(g.arrears, 708)

    def test_dl10_threshold_is_strictly_greater(self):
        g2 = day(g_(), 0, d=3, arrears=333)  # 500+333=833, phạt ceil(166.6)=167 -> 1000
        g2.end_day()
        self.assertEqual((g2.arrears, g2.status), (1000, "playing"))
        g3 = day(g_(), 0, d=3, arrears=334)  # 834, phạt 167 -> 1001
        g3.end_day()
        self.assertEqual((g3.arrears, g3.status), (1001, "lost"))

    def test_dl11_recover_by_paying_all_arrears(self):
        g = day(g_(), 150)
        g.end_day()                 # tồn 60
        g.cash = 410
        g.end_day()                 # kỳ 350 + 60
        self.assertEqual((g.cash, g.arrears, g.day), (0, 0, 3))

    def test_dl12_after_shortfall_cash_zero_blocks_purchase(self):
        # Phát hiện D-05: thu hết tiền khiến người chơi không thể mua hộp để gỡ lại.
        g = day(g_(), 150)
        g.end_day()
        with self.assertRaises(Rejected):
            g.buy_box("nho")
        self.assertEqual((g.cash, g.arrears), (0, 60))

    def test_dl13_no_early_payment_still_rejected(self):
        g = day(g_(), 1000)
        with self.assertRaises(Rejected):
            g.pay_early(2)


class Combos(unittest.TestCase):
    def _g(self, *ids):
        g = g_()
        for i in ids:
            g.grant_artifact(i)
        return g

    def test_cb1_two_same_system_small_combo(self):
        g = self._g("A1", "A2")
        g.equip("A1"); g.equip("A2")
        self.assertEqual(g.active_combos(), [("Co", "nho")])

    def test_cb2_three_same_system_big_combo(self):
        g = self._g("A1", "A2", "A3")
        for a in ("A1", "A2", "A3"):
            g.equip(a)
        self.assertEqual(g.active_combos(), [("Co", "lon")])

    def test_cb3_mixed_systems_no_combo(self):
        g = self._g("A1", "B1")
        g.equip("A1"); g.equip("B1")
        self.assertEqual(g.active_combos(), [])

    def test_cb4_unequip_removes_combo(self):
        g = self._g("A1", "A2")
        g.equip("A1"); g.equip("A2"); g.unequip("A2")
        self.assertEqual(g.active_combos(), [])

    def test_cb5_slots_full_rejected(self):
        g = self._g("A1", "A2", "A3", "B1")
        for a in ("A1", "A2", "A3"):
            g.equip(a)
        with self.assertRaises(Rejected):
            g.equip("B1")
        self.assertEqual(len(g.equipped), 3)

    def test_cb6_double_equip_and_unowned_rejected(self):
        g = self._g("A1")
        g.equip("A1")
        with self.assertRaises(Rejected):
            g.equip("A1")
        with self.assertRaises(Rejected):
            g.equip("B3")

    def test_cb7_combo_counts_once_even_if_toggled(self):
        g = self._g("A1", "A2")
        g.equip("A1"); g.equip("A2"); g.unequip("A2"); g.equip("A2")
        self.assertEqual(g.combos_done, 1)


class BiKhe(unittest.TestCase):
    def _item(self, g, origin):
        i = g._id()
        g.items[i] = {"value": 100, "fake": False, "origin": origin, "appraised": False}
        return i

    def test_bk1_not_met_before_combo(self):
        g = g_(); g.grant_artifact("A1"); g.equip("A1")
        self.assertEqual((g.progress("BK1"), g.unlocked), ((0, 1), []))

    def test_bk1_met_when_combo_completes(self):
        g = g_(); g.grant_artifact("A1"); g.grant_artifact("A2")
        g.equip("A1"); g.equip("A2")
        self.assertEqual((g.progress("BK1"), g.unlocked), ((1, 1), ["BK1"]))

    def test_bk2_progress_counts_only_required_origin(self):
        g = g_()
        g.appraise(self._item(g, "Than thoai"))
        g.appraise(self._item(g, "Co tich"))
        self.assertEqual((g.progress("BK2"), "BK2" in g.unlocked), ((1, 2), False))

    def test_bk2_unlocks_at_goal(self):
        g = g_()
        g.appraise(self._item(g, "Than thoai"))
        g.appraise(self._item(g, "Than thoai"))
        self.assertEqual(g.unlocked, ["BK2"])

    def test_bk3_appraise_same_item_twice_no_extra_progress(self):
        g = g_(); i = self._item(g, "Than thoai")
        g.appraise(i)
        with self.assertRaises(Rejected):
            g.appraise(i)
        self.assertEqual(g.progress("BK2"), (1, 2))

    def test_bk4_unlock_happens_once(self):
        g = g_()
        for _ in range(3):
            g.appraise(self._item(g, "Than thoai"))
        self.assertEqual(g.unlocked.count("BK2"), 1)


class SaveLoad(unittest.TestCase):
    def test_sl1_round_trip(self):
        g = g_()
        b = g.buy_box("vua"); i = g.open_box(b)
        g.grant_artifact("A1"); g.equip("A1")
        h = Game.load(g.save())
        self.assertEqual(g.snapshot(), h.snapshot())
        self.assertEqual(g.save(), h.save())

    def test_sl2_loot_fixed_at_purchase_no_reroll(self):
        g = g_(); b = g.buy_box("lon")
        blob = g.save()
        first = g.items[g.open_box(b)]
        h = Game.load(blob)
        again = h.items[h.open_box(b)]
        self.assertEqual(first["value"], again["value"])
        self.assertEqual(first["fake"], again["fake"])

    def test_sl3_load_after_open_sell_once(self):
        g = g_(); b = g.buy_box("nho"); i = g.open_box(b)
        h = Game.load(g.save())
        cash = h.cash
        h.sell_item(i)
        with self.assertRaises(Rejected):
            h.sell_item(i)
        self.assertGreater(h.cash, cash - 1)

    def test_sl4_replayed_transaction_rejected_after_load(self):
        g = g_(); b = g.buy_box("nho"); i = g.open_box(b)
        g.apply("tx-sell-1", g.sell_item, i)
        cash = g.cash
        h = Game.load(g.save())
        with self.assertRaises(Rejected):
            h.apply("tx-sell-1", h.sell_item, i)
        self.assertEqual(h.cash, cash)

    def test_sl5_debt_state_survives_load(self):
        g = g_(); g.cash = 150
        g.end_day()
        h = Game.load(g.save())
        self.assertEqual((h.arrears, h.day, h.cash), (60, 2, 0))

    def test_sl6_bikhe_progress_and_unlock_survive_load(self):
        g = g_()
        for _ in range(2):
            i = g._id()
            g.items[i] = {"value": 1, "fake": False, "origin": "Than thoai", "appraised": False}
            g.appraise(i)
        h = Game.load(g.save())
        self.assertEqual((h.progress("BK2"), h.unlocked), ((2, 2), ["BK2"]))
        h.unlocked_before = list(h.unlocked)
        h._update_bikhe()
        self.assertEqual(h.unlocked, ["BK2"])

    def test_sl7_load_mid_day_keeps_purchase_count(self):
        g = g_()
        g.buy_box("nho"); g.buy_box("nho"); g.buy_box("nho")
        h = Game.load(g.save())
        with self.assertRaises(Rejected):
            h.buy_box("nho")

    def test_sl8_game_over_survives_load(self):
        g = g_(); g.cash = 0; g.day = 3; g.arrears = 900
        g.end_day()
        h = Game.load(g.save())
        self.assertEqual(h.status, "lost")
        with self.assertRaises(Rejected):
            h.buy_box("nho")

    def test_sl9_save_scum_is_possible_by_design_gap(self):
        # D-06: tải lại bản lưu cũ hoàn nguyên giao dịch. Nếu cho lưu tự do, người chơi
        # có thể hoàn tác một lần bán xấu. Cần chốt chính sách lưu ([MỞ] GDD mục 6).
        g = g_(); b = g.buy_box("nho"); i = g.open_box(b)
        blob = g.save()
        g.sell_item(i)
        h = Game.load(blob)
        self.assertIn(i, h.items)


if __name__ == "__main__":
    unittest.main(verbosity=2)

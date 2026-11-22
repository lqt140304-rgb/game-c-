import unittest
from economy_model import *


def fresh(**kw):
    return Game(seed=7, **kw)


class Purchase(unittest.TestCase):
    def test_bt01_fourth_box_rejected(self):
        g = fresh()
        for _ in range(3):
            g.buy_box("nho")
        before = g.snapshot()
        with self.assertRaises(Rejected):
            g.buy_box("nho")
        self.assertEqual(before, g.snapshot())

    def test_bt02_purchases_reset_next_day_not_accumulate(self):
        g = fresh()
        g.buy_box("nho")
        g.end_day()
        self.assertEqual(g.bought_today, 0)
        for _ in range(3):
            g.buy_box("nho")
        with self.assertRaises(Rejected):
            g.buy_box("nho")

    def test_bt03_insufficient_cash_no_slot_used(self):
        g = fresh(); g.cash = 99
        with self.assertRaises(Rejected):
            g.buy_box("nho")
        self.assertEqual((g.cash, g.bought_today), (99, 0))

    def test_bt04_exact_cash(self):
        g = fresh(); g.cash = 100
        g.buy_box("nho")
        self.assertEqual((g.cash, g.bought_today), (0, 1))


class Debt(unittest.TestCase):
    def test_bt05_early_payment_rejected(self):
        g = fresh()
        before = g.snapshot()
        with self.assertRaises(Rejected):
            g.pay_early(2)
        self.assertEqual(before, g.snapshot())

    def test_bt06_exact_payment(self):
        g = fresh(); g.cash = 200
        g.end_day()
        self.assertEqual((g.cash, g.paid, g.day), (0, [1], 2))

    def test_bt07_surplus_not_overcharged(self):
        g = fresh(); g.cash = 650
        g.end_day()
        self.assertEqual(g.cash, 450)

    def test_bt08_shortfall_follows_claude1_rule(self):
        # Luật nợ [ĐỀ XUẤT] của Claude 1: trả hết tiền có, nợ tồn + phạt 20% làm tròn lên.
        g = fresh(); g.day = 2; g.cash = 349
        r = g.end_day()
        self.assertEqual((g.cash, g.arrears, r["result"]), (0, 2, "next_day"))

    def test_bt09_end_day_is_not_idempotent_ui_must_guard(self):
        # Phát hiện D-01: gọi end_day hai lần liên tiếp thu nợ của HAI ngày,
        # vì lần hai thuộc ngày kế tiếp. Lớp UI/gameplay phải chặn bấm đúp.
        g = fresh(); g.cash = 1000
        g.end_day(); g.end_day()
        self.assertEqual((g.paid, g.day, g.cash), ([1, 2], 3, 450))


class Sale(unittest.TestCase):
    def _item(self, g, value=500, fake=False):
        iid = g._id()
        g.items[iid] = {"value": value, "fake": fake}
        return iid

    def test_bt10_double_sell_credits_once(self):
        g = fresh(); i = self._item(g, 500)
        g.sell_item(i)
        with self.assertRaises(Rejected):
            g.sell_item(i)
        self.assertEqual(g.cash, 1500)

    def test_bt11_sell_missing_item(self):
        g = fresh()
        with self.assertRaises(Rejected):
            g.sell_item(999)
        self.assertEqual(g.cash, 1000)

    def test_bt12_fake_sells_at_20_percent(self):
        g = fresh(); i = self._item(g, 1000, fake=True)
        self.assertEqual(g.sell_item(i), 200)

    def test_bt13_open_box_twice(self):
        g = fresh(); b = g.buy_box("nho")
        g.open_box(b)
        n = len(g.items)
        with self.assertRaises(Rejected):
            g.open_box(b)
        self.assertEqual(len(g.items), n)

    def test_bt14_full_inventory_rejects_purchase(self):
        g = fresh()
        for _ in range(INVENTORY_CAPACITY):
            self._item(g)
        cash = g.cash
        with self.assertRaises(Rejected):
            g.buy_box("nho")
        self.assertEqual((g.cash, g.bought_today), (cash, 0))


class Flow(unittest.TestCase):
    def test_bt16_win_on_day_7(self):
        g = fresh(); g.cash = sum(DEBT)
        for _ in range(7):
            r = g.end_day()
        self.assertEqual((r["result"], g.status, g.cash), ("won", "won", 0))

    def test_no_actions_after_game_over(self):
        g = fresh(); g.cash = 0
        g.end_day()
        with self.assertRaises(Rejected):
            g.buy_box("nho")

    def test_same_seed_reproduces(self):
        def play(seed):
            g = Game(seed=seed)
            ids = [g.buy_box("nho") for _ in range(3)]
            return [g.items[g.open_box(b)] for b in ids]
        self.assertEqual(play(3), play(3))
        self.assertNotEqual(play(3), play(4))


if __name__ == "__main__":
    unittest.main(verbosity=2)

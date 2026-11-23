"""GT1 Sổ Ghi Nợ Cũ (Claude 1, DieuChinhSauCanBang.md mục C): hoãn kỳ của ngày đó một lần mỗi ván.
Ba điểm chốt: kỳ x 1,15 trả cùng kỳ sau, không phạt 20% và không tính ngưỡng thua, chỉ hoãn kỳ của ngày đó."""
import unittest
from economy_model import Game, Rejected, DEBT


def G(day, cash):
    g = Game(seed=1); g.day, g.cash = day, cash
    return g


class GT1(unittest.TestCase):
    def test_c8_postpone_day4_then_pay_1050_plus_863(self):
        g = G(4, 500)
        g.use_gt1()
        g.end_day()
        self.assertEqual((g.day, g.cash, g.arrears, g.carry), (5, 500, 0, 863))   # 750 x 1,15 làm tròn lên
        g.cash = 5000
        g.end_day()
        self.assertEqual((g.cash, g.arrears, g.carry), (5000 - 1050 - 863, 0, 0))

    def test_c8b_postponed_amount_has_no_20_percent_penalty(self):
        g = G(4, 0)
        g.use_gt1(); g.end_day()
        self.assertEqual(g.arrears, 0)         # kỳ hoãn chưa đến hạn nên chưa bị phạt

    def test_c8c_not_counted_toward_loss_threshold(self):
        g = G(4, 0)
        g.use_gt1(); g.end_day()
        self.assertEqual(g.status, "playing")  # nếu tính carry 863 và nợ ngày sau thì chưa thua ở đây

    def test_c9_cannot_postpone_day7(self):
        g = G(7, 5000)
        with self.assertRaises(Rejected):
            g.use_gt1()
        self.assertFalse(g.gt1_used)

    def test_c10_only_once_per_game(self):
        g = G(2, 5000)
        g.use_gt1(); g.end_day()
        with self.assertRaises(Rejected):
            g.use_gt1()

    def test_c11_early_payment_of_future_period_still_rejected(self):
        g = G(2, 5000)
        with self.assertRaises(Rejected):
            g.pay_early(3)

    def test_unpaid_carry_is_penalized_next_collection(self):
        g = G(4, 0)
        g.use_gt1(); g.end_day()               # carry 863
        g.cash = 100
        g.end_day()                            # kỳ 1.050 + 863 = 1.913, trả 100, tồn 1.813 -> thua
        self.assertEqual(g.status, "lost")

    def test_carry_blocks_win_on_day7_only_if_unpaid(self):
        g = G(6, 0)
        g.use_gt1(); g.end_day()               # carry cho ngày 7 = ceil(1500*1,15) = 1.725
        self.assertEqual((g.day, g.carry), (7, 1725))
        g.cash = 2200 + 1725
        g.end_day()
        self.assertEqual(g.status, "won")

    def test_only_today_period_not_arrears(self):
        g = G(3, 100)
        g.arrears = 240
        g.use_gt1(); g.end_day()
        # nợ tồn 240 vẫn phải trả (trả 100, còn 140 + phạt 28), chỉ kỳ 500 bị hoãn
        self.assertEqual((g.cash, g.arrears, g.carry), (0, 168, 575))

    def test_save_load_keeps_gt1_state(self):
        g = G(4, 500)
        g.use_gt1(); g.end_day()
        h = Game.load(g.save())
        self.assertEqual((h.carry, h.gt1_used), (863, True))
        with self.assertRaises(Rejected):
            h.use_gt1()


if __name__ == "__main__":
    unittest.main(verbosity=2)

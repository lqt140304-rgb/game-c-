"""Khóa cấu hình 'lời tuyệt đối tăng theo giá hộp' (C3) để việc chọn hộp thành đánh đổi theo số tiền.
CHỜ CHỦ DỰ ÁN DUYỆT: đổi TIER_PROFIT_APPROVED thành True khi duyệt. Số liệu: Docs/Balance/LoiTheoGiaHop.md."""
import unittest
import clue_sim as C
import tier_sim as T
import risk_penalty as R
import proposal_sim as P

TIER_PROFIT_APPROVED = False
N = 600


@unittest.skipUnless(TIER_PROFIT_APPROVED, "chờ chủ dự án duyệt cấu hình C3")
class TierProfit(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        C.setup(True)
        cls.c, _ = T.rounded(T.solve(dict(H1=100, H2=160, H3=220, RD=260)))
        cls.mix = T.mix_by_bracket(cls.c, C.TURN_NAMES["thận trọng + giám định"], N)
        cls.r = C.measure_turns(cls.c, N, lam=.75, tau=1.0)

    def share(self, bracket, box):
        d = self.mix[bracket]
        return 100 * d.get(box, 0) / max(sum(d.values()), 1)

    def test_profit_increases_with_price(self):
        ev = R.box_ev(self.c)
        profit = [(ev[b] - 1) * T.BOX_PRICE[b] for b in ("H1", "H2", "H3", "RD")]
        self.assertTrue(profit == sorted(profit), profit)

    def test_choice_depends_on_cash(self):
        self.assertGreater(self.share("dưới 600", "H1") - self.share("trên 1.500", "H1"), 40)

    def test_red_chest_bought_when_rich_not_when_poor(self):
        self.assertTrue(15 <= self.share("trên 1.500", "RD") <= 50)
        self.assertEqual(self.share("dưới 600", "RD"), 0)

    def test_no_single_tier_dominates(self):
        best_forced = max(T.win(self.c, T.forced(t), N) for t in ("H2", "H3"))
        self.assertGreater(self.r["thận trọng + giám định"] + 1, best_forced)

    def test_hard(self):
        for k in C.TURN_NAMES:
            self.assertLessEqual(self.r[k], 38, (k, self.r[k]))


if __name__ == "__main__":
    unittest.main(verbosity=2)

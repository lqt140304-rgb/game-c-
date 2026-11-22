"""Kiểm tra mô hình manh mối của clue_sim.py (Claude 2): xác suất chuẩn hóa và hướng suy luận đúng."""
import unittest
import clue_sim as C
from sprint3_sim import Content, QK, HD, TL, NHT, CT, TT


class FakeRun:
    c = Content()


class Clues(unittest.TestCase):
    def test_likelihood_normalized_for_one_clue(self):
        for o in C.GROUPS:
            self.assertAlmostEqual(sum(C.likelihood([g], o) for g in C.GROUPS), 1.0, places=9)

    def test_likelihood_normalized_for_two_clues(self):
        import itertools
        for o in C.GROUPS:
            tot = sum(C.likelihood(sorted(c), o) for c in itertools.combinations_with_replacement(C.GROUPS, 2))
            self.assertAlmostEqual(tot, 1.0, places=9)

    def test_true_clue_more_likely_than_false(self):
        self.assertGreater(C.likelihood([HD], HD), 5 * C.likelihood([HD], TL))

    def test_h2_modern_clue_lowers_value_vs_future_clue(self):
        r = FakeRun()
        C.R.FAKE_BY_BOX.clear()
        self.assertLess(C.posterior_ev(r, "H2", [HD]), C.posterior_ev(r, "H2", [TL]))

    def test_h1_clue_is_uninformative(self):
        r = FakeRun()
        C.R.FAKE_BY_BOX.clear()
        self.assertAlmostEqual(C.posterior_ev(r, "H1", [QK]), C.posterior_ev(r, "H1", []), places=6)

    def test_three_clues_pin_down_origin(self):
        self.assertGreater(C.likelihood([HD, HD, TL], HD), 10 * C.likelihood([HD, HD, TL], TL))


if __name__ == "__main__":
    unittest.main(verbosity=2)

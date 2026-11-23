"""Khóa gói cân bằng khuyến nghị sau khi đo TC3, CB3, GT1 và chứng nhận giám định (Claude 2).
Xem Docs/Balance/DoLaiDeXuat.md. Bot chỉ là cận dưới, người chơi thật cao hơn."""
import unittest
import proposal_sim as P
from sprint3_sim import ART

N = 1500
PKG = dict(APPR_RATE=.15, CERT_BONUS=.25, CP1_B=.05, CB1_B=.10, CB3_PRICE=.15, CB3_FREE=99)


class Package(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.saved = {k: getattr(P, k) for k in PKG}
        cls.art = (ART["CP3"], ART["TC3"])
        for k, v in PKG.items():
            setattr(P, k, v)
        ART["CP3"], ART["TC3"] = ("CP", 500), ("TC", 500)
        cls.r = P.measure(P.C1, n=N)

    @classmethod
    def tearDownClass(cls):
        for k, v in cls.saved.items():
            setattr(P, k, v)
        ART["CP3"], ART["TC3"] = cls.art

    def w(self, n):
        return self.r[n]["win"]

    def test_plain_bots_are_hard(self):
        self.assertLessEqual(max(self.w("thận trọng"), self.w("liều")), 20)
        self.assertGreaterEqual(min(self.w("thận trọng"), self.w("liều")), 10)

    def test_appraisal_is_a_real_but_not_free_gain(self):
        gain = self.w("thận trọng + giám định chọn lọc") - self.w("thận trọng")
        self.assertTrue(4 <= gain <= 14, gain)

    def test_investing_is_worth_it_but_not_overwhelming(self):
        gap = self.w("combo nhỏ (CB1)") - self.w("thận trọng")
        self.assertTrue(4 <= gap <= 16, gap)

    def test_small_and_big_combo_are_both_viable(self):
        self.assertLessEqual(abs(self.w("combo lớn + giám định") - self.w("combo nhỏ + giám định")), 4)

    def test_nothing_is_easy(self):
        self.assertLessEqual(max(r["win"] for r in self.r.values()), 38)


if __name__ == "__main__":
    unittest.main(verbosity=2)

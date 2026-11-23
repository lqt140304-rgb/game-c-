"""Khóa cơ chế phạt rủi ro O-01 (hàng giả theo hộp, giá nền x1,10) do Claude 1 đề xuất ở
DieuChinhSauCanBang.md mục G. CHỜ CHỦ DỰ ÁN DUYỆT: đổi RISK_PENALTY_APPROVED thành True khi duyệt.
Số liệu đo: Docs/Balance/DoLaiDeXuat.md mục 10."""
import unittest
import proposal_sim as P
import risk_penalty as R
import risk_full as F
from sprint3_sim import ART

RISK_PENALTY_APPROVED = True
N = 1500


@unittest.skipUnless(RISK_PENALTY_APPROVED, "chờ chủ dự án duyệt mục G")
class RiskPenalty(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.saved = {k: getattr(P, k) for k in F.PKG}
        cls.art = (ART["CP3"], ART["TC3"])
        cls.run_buy, cls.run_pr = P.Run5.buy_open, P.Run5.p_real
        P.Run5.buy_open, P.Run5.p_real = R.buy_open, R.p_real
        for k, v in F.PKG.items():
            setattr(P, k, v)
        ART["CP3"], ART["TC3"] = ("CP", 500), ("TC", 500)
        R.FAKE_BY_BOX.clear(); R.FAKE_BY_BOX.update(dict(H1=.12, H2=.15, H3=.22, RD=.30))
        cls.c = F.content(F.X110)
        cls.r = P.measure(cls.c, n=N)

    @classmethod
    def tearDownClass(cls):
        for k, v in cls.saved.items():
            setattr(P, k, v)
        ART["CP3"], ART["TC3"] = cls.art
        P.Run5.buy_open, P.Run5.p_real = cls.run_buy, cls.run_pr
        R.FAKE_BY_BOX.clear()

    def w(self, n):
        return self.r[n]["win"]

    def test_risk_is_priced(self):
        gap = self.w("liều + giám định chọn lọc") - self.w("thận trọng + giám định chọn lọc")
        self.assertLessEqual(gap, 5, gap)

    def test_plain_bots_hard(self):
        self.assertTrue(10 <= self.w("thận trọng") <= 20 and 10 <= self.w("liều") <= 20)

    def test_investing_in_target_band(self):
        for n in ("combo nhỏ (CB1)", "combo lớn (CB1 + CB3)"):
            self.assertTrue(24 <= self.w(n) <= 36, (n, self.w(n)))

    def test_ceiling(self):
        self.assertLessEqual(max(r["win"] for r in self.r.values()), 41)

    def test_red_chest_is_nearly_break_even(self):
        self.assertLess(R.box_ev(self.c)["RD"], 1.1)


if __name__ == "__main__":
    unittest.main(verbosity=2)

"""Khóa phương án C: giá hộp dao động ±20% theo ngày, Rương Đen giữ 15.000.000 đ (1.500 đơn vị cân bằng).
Số liệu: Docs/Balance/GiaHopDaoDong.md. Bot chỉ là cận dưới/cận trên, người thật khác."""
import unittest
import clue_sim as C
import proposal_sim as P
import swing_sim as S

N = 700


class BoxPriceSwing(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        S.SW["amp"] = 0.20
        S.E.CFG.update(on=False)
        cls.c = S.content(1500)
        C.setup(True)
        cls.lad = P.measure(cls.c, n=N)
        cls.skilled = C.measure_turns(cls.c, N, lam=.75, tau=1.0)
        cls.mix = {b: 0 for b in S.TIERS}
        orig = C.turn_choose

        def spy(run, rc, tiers, _o=orig):
            it = _o(run, rc, tiers)
            if it:
                cls.mix[it["box"]] += 1
            return it
        C.turn_choose = spy
        try:
            for s in range(N):
                P.play5(s, cls.c, C.TURN_NAMES["thận trọng + giám định"])
        finally:
            C.turn_choose = orig
        S.SW["amp"] = 0.0

    def test_plain_bots_stay_hard(self):
        for k in ("thận trọng", "liều"):
            self.assertTrue(10 <= self.lad[k]["win"] <= 26, (k, self.lad[k]["win"]))

    def test_skill_gap_is_kept(self):
        gap = self.skilled["thận trọng + giám định"] - self.lad["thận trọng"]["win"]
        self.assertGreaterEqual(gap, 12, gap)

    def test_perfect_player_not_too_easy(self):
        for k in C.TURN_NAMES:
            self.assertLessEqual(self.skilled[k], 52, (k, self.skilled[k]))

    def test_red_chest_is_sometimes_bought(self):
        tot = sum(self.mix.values())
        self.assertTrue(2 <= 100 * self.mix["RD"] / tot <= 15, self.mix)

    def test_h2_no_longer_overwhelming(self):
        tot = sum(self.mix.values())
        self.assertLess(100 * self.mix["H2"] / tot, 62, self.mix)


if __name__ == "__main__":
    unittest.main(verbosity=2)

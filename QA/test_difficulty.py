"""Mục tiêu độ khó (chủ dự án): ván khó, thắng không dễ, nhiều đánh đổi cần tính toán.

Mã hóa thành tiêu chí đo được để mọi thay đổi số liệu sau này không làm game dễ đi hoặc để một
chiến thuật thống trị. Ba chiến thuật mô phỏng là cận dưới cho người chơi chưa biết soi, người chơi
thật có kỹ năng sẽ cao hơn.
"""
import unittest
import sprint3_sim as sm
from sprint3_sim import careful, combo_hunter, bold, play
from profiles import HARD, use_hard

N = 1500


def win(strat):
    return 100 * sum(play(s, HARD, strat).g.status == "won" for s in range(N)) / N


class Difficulty(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        use_hard()
        cls.w = {"thận trọng": win(careful), "săn combo": win(combo_hunter), "liều": win(bold)}

    def test_no_strategy_is_easy(self):
        self.assertLessEqual(max(self.w.values()), 32, self.w)

    def test_not_impossible(self):
        self.assertGreaterEqual(min(self.w.values()), 12, self.w)

    def test_no_dominant_strategy(self):
        self.assertLessEqual(max(self.w.values()) - min(self.w.values()), 7, self.w)

    def test_box_ladder_risk_is_not_free(self):
        # hộp càng đắt thì lời tuyệt đối càng cao nhưng tỷ lệ lời càng thấp: dồn tiền không lợi hơn vô điều kiện
        from difficulty import ev
        r = ev(HARD)
        self.assertTrue(r["H1"] > r["H2"] > r["H3"] > r["RD"], r)
        self.assertLess(r["RD"], 1.2)


if __name__ == "__main__":
    unittest.main(verbosity=2)

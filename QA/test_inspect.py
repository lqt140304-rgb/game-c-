"""Khóa Sprint 5 phần 3: luật soi lỗi và trả giá ĐỀ XUẤT (inspect_sim.RULES) trên 4 rương cơ bản đã chốt.
Số liệu đầy đủ (n = 2.000): Docs/Balance/Sprint5SoiLoi.md. Ở đây n nhỏ (khoảng 1 phút) nên so trung bình của hai chiến thuật bot, hạt giống cố định 0..N-1.
Số tiền trong mã tính theo đơn vị 10.000 đ (extra_fee H=20 là 200.000 đ)."""
import unittest
import chest_sim as CS
import clue_sim as C
import inspect_sim as I
import proposal_sim as P

N = 600
N_DAY1 = 300


class InspectRules(unittest.TestCase):
    """Luật và phép tính một món, không cần mô phỏng cả ván."""
    rl = I.RULES

    def test_money_is_multiple_of_10000_dong(self):
        for t, fee in self.rl["extra_fee"].items():
            self.assertIsInstance(fee, int, t)   # đơn vị 10.000 đ là số nguyên, nên luôn là bội số 10.000 đ

    def test_tier_shortcuts(self):
        """Thường: soi nhanh 1 lượt, bán nhanh. Hiếm trở lên: 3 đến 4 lượt, trả giá đầy đủ (spec mục 2, 5)."""
        self.assertEqual(self.rl["looks"]["T"], 1)
        for t in ("H", "X", "L"):
            self.assertTrue(3 <= self.rl["looks"][t] <= 4, t)
        pf = I.PROFILES["giỏi"]
        self.assertEqual(I.real_value("T", 3, pf, self.rl), self.rl["quick"])
        self.assertGreater(I.real_value("X", 3, pf, self.rl), self.rl["quick"])

    def test_errors_list_matches_tier_spec(self):
        """tier-do-trong-ruong mục 4: Thường 1-2, Hiếm 2-3, Rất hiếm 3, Huyền thoại 3-4 lỗi."""
        lo_hi = dict(T=(1, 2), H=(2, 3), X=(3, 3), L=(3, 4))
        for t, (lo, hi) in lo_hi.items():
            self.assertTrue(lo <= self.rl["points"][t] <= hi, t)

    def test_haggle_bounds_and_press(self):
        """Không có bằng chứng thì bị ép dưới giá bán nhanh; đủ bằng chứng thì lên tới trần, không vượt."""
        pf = I.PROFILES["giỏi"]
        self.assertLess(I.haggle_ev(0, pf, self.rl), self.rl["quick"])
        self.assertLessEqual(I.haggle_ev(9, dict(pf, c=1.0), self.rl), self.rl["cap"])
        self.assertGreater(I.haggle_ev(3, pf, self.rl), I.haggle_ev(1, pf, self.rl))
        self.assertLessEqual(self.rl["cap"], 1.15)

    def test_wrong_calls_have_consequences(self):
        """Thật mà khai giả thì chỉ nhận giá hàng giả; giả mà khai thật thì bị phạt thêm; ngày 1 miễn phạt."""
        self.assertEqual(self.rl["fake_x"], .20)
        self.assertGreater(self.rl["fine"], 0)
        self.assertEqual(self.rl["fine"], self.rl["fake_x"])   # đề xuất: món giả khai thật bị tịch thu (0 đ)
        self.assertIsNone(self.rl["fine_T"])
        self.assertEqual(self.rl["day1_fine"], 0)

    def test_doubt_means_claim_real(self):
        """Lỗ khi khai giả món thật (80%) lớn hơn phạt khi khai thật món giả, nên phải khá chắc mới khai giả."""
        pf, rl = I.PROFILES["giỏi"], self.rl
        self.assertFalse(I.decide_fake(.5, "X", 3, rl["fine"], pf, rl))
        self.assertTrue(I.decide_fake(.95, "X", 3, rl["fine"], pf, rl))

    def test_posterior_moves_with_evidence(self):
        pf, rl = I.PROFILES["thường"], self.rl
        p0 = I.posterior(.3, 0, 0, pf, rl)
        self.assertAlmostEqual(p0, .3)
        self.assertGreater(I.posterior(.3, 2, 0, pf, rl), p0)
        self.assertLess(I.posterior(.3, 0, 2, pf, rl), p0)
        self.assertEqual(I.posterior(0, 3, 0, pf, rl), 0)   # Kim Khí không có hàng giả
        self.assertGreater(I.posterior(.3, 0, 0, pf, rl, sure_fs=1), I.posterior(.3, 1, 0, pf, rl))

    def test_good_inspector_reads_better(self):
        a_g, b_g = I.sign_probs(I.PROFILES["giỏi"], self.rl)
        a_t, b_t = I.sign_probs(I.PROFILES["thường"], self.rl)
        a_a, b_a = I.sign_probs(I.PROFILES["ẩu"], self.rl)
        self.assertGreater(a_g / b_g, a_t / b_t)
        self.assertGreater(a_t / b_t, a_a / b_a)


class InspectBalance(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.core = dict(before=P.handle5)
        cls.w = {}
        for prof in ("thường", "giỏi", "ẩu", "bỏ qua soi"):
            c = I.apply(CS.FINAL, None, prof)
            for b in I.BOT:
                cls.w[(b, prof)] = I._play(c, P.STRATS[b], N)[0]
        c = I.apply(CS.FINAL, None, "giỏi")
        cls.skilled = {k: I._play(c, C.TURN_NAMES[k], N, skilled=True)[0] for k in I.SKILLED[:2]}
        cls.day1 = {prof: I.day1(None, prof, CS.FIRST_RUN, N_DAY1, "liều") for prof in ("thường", "ẩu", "bỏ qua soi")}
        I.restore()
        cls.core["after"] = P.handle5

    def test_restore_puts_core_back(self):
        self.assertIs(self.core["after"], self.core["before"])

    def test_plain_bot_in_target(self):
        """Bot thường (không soi kỹ) vẫn khoảng 15% đến 20% (n = 2.000: 15,2% và 16,2%)."""
        for b in I.BOT:
            self.assertTrue(12 <= self.w[(b, "thường")] <= 21, (b, self.w[(b, "thường")]))

    def test_perfect_player_not_too_easy(self):
        for k, v in self.skilled.items():
            self.assertLessEqual(v, 47, (k, v))

    def avg(self, prof):
        return sum(self.w[(b, prof)] for b in I.BOT) / len(I.BOT)

    def test_inspection_skill_pays(self):
        """Cùng cách mua rương: soi giỏi > soi thường > soi ẩu; bỏ qua soi kém soi thường (n = 2.000, trung bình hai bot: 22,5 / 15,7 / 12,0 / 14,1)."""
        g, t, a, s = (self.avg(p) for p in ("giỏi", "thường", "ẩu", "bỏ qua soi"))
        self.assertGreaterEqual(g - t, 3, (g, t))
        self.assertGreater(t, a, (t, a))
        self.assertGreater(t, s, (t, s))

    def test_careless_still_playable(self):
        """Soi ẩu bị phạt rõ nhưng vẫn còn đường thắng."""
        for b in I.BOT:
            self.assertGreater(self.w[(b, "ẩu")], 6, (b, self.w[(b, "ẩu")]))

    def test_first_run_day1_never_lost(self):
        for prof, d in self.day1.items():
            self.assertEqual(d["lost_day1"], 0, (prof, d))
            self.assertEqual(d["lost_before_day3"], 0, (prof, d))
            self.assertGreater(d["win"], 3, (prof, d))


if __name__ == "__main__":
    unittest.main(verbosity=2)

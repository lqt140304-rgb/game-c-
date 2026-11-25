"""Khóa Sprint 5 phần 3: luật soi MỚI (lỗi trừ giá) và trả giá ĐỀ XUẤT (inspect_sim.RULES) trên 4 rương cơ bản đã chốt.
Số liệu đầy đủ (n = 2.000): Docs/Balance/Sprint5SoiLoi.md. Ở đây n nhỏ (khoảng 1 phút) nên so trung bình của hai chiến thuật bot, hạt giống cố định 0..N-1.
Số tiền trong mã tính theo đơn vị 10.000 đ (phí gợi ý tối thiểu 20 là 200.000 đ)."""
import unittest
import chest_sim as CS
import clue_sim as C
import inspect_sim as I
import proposal_sim as P

N = 600
N_DAY1 = 300


class InspectRules(unittest.TestCase):
    """Luật soi mới và phép tính một món, không cần mô phỏng cả ván."""
    rl = I.RULES

    def test_defect_counts_match_condition(self):
        """Nguyên 0 lỗi; Xước n lỗi nhẹ; Hỏng n lỗi nhẹ + 1 vết lớn; trừ 30% và 70% như hệ số cũ (0,7 và 0,3)."""
        for t, n in dict(T=1, H=2, X=3, L=3).items():
            self.assertEqual(I.n_defects(1.0, t, self.rl), 0)
            self.assertEqual(I.n_defects(.7, t, self.rl), n)
            self.assertEqual(I.n_defects(.3, t, self.rl), n + 1)
        self.assertAlmostEqual(1 - self.rl["light_cut"], .7)
        self.assertAlmostEqual(1 - self.rl["light_cut"] - self.rl["big_cut"], .3)

    def test_fake_marks_by_tier(self):
        """Thường và Hiếm 1 dấu giả; Rất hiếm và Huyền thoại 2."""
        self.assertEqual(self.rl["marks"], dict(T=1, H=1, X=2, L=2))

    def test_haggle_bounds(self):
        """Trả giá có trần: dù thắng cả vòng cũng không vượt cap; thua nhiều không dưới floor; người không tìm lỗi thì dừng."""
        pf = I.PROFILES["giỏi"]
        self.assertLessEqual(I.haggle_ev(1.0, self.rl), self.rl["cap"])
        self.assertLessEqual(self.rl["cap"], 1.15)
        self.assertEqual(I.haggle_ev(0.0, self.rl), self.rl["open"])
        self.assertGreater(I.haggle_win(pf, 3, 3), I.haggle_win(pf, 1, 3))
        self.assertEqual(I.haggle_win(pf, 0, 0), pf["c"])

    def test_repair_shop(self):
        """Nhà vá lỗi: chỉ sửa lỗi đã tìm ra khi có lời; phí trừ vào tiền; hệ số tình trạng tăng đúng phần lỗi đã sửa."""
        rl = dict(self.rl, repair_light=.05, repair_big=.10)

        class G:
            cash = 1000

        class Run:
            g = G()

            def known_price(self, it):
                return int(100 * it["q"])

        def go(p_fake, fl, fb, cash=1000):
            run, it = Run(), dict(q=.3)
            run.g = G()
            run.g.cash = cash
            st = dict(repairs=0, repair_spend=0)
            V = I.repair_shop(run, it, 0, "X", p_fake, 100, 3, 1, fl, fb, rl, st, 30)
            return V, it, st, run.g.cash
        V, it, st, cash = go(0.0, 3, 1)          # Hỏng, tìm đủ 3 lỗi nhẹ và vết lớn, chắc là thật
        self.assertEqual(st["repairs"], 4)
        self.assertAlmostEqual(it["q"], 1.0)
        self.assertEqual(V, 100)
        self.assertEqual(cash, 1000 - st["repair_spend"])
        V, it, st, cash = go(0.0, 0, 0)          # không tìm ra lỗi nào thì không sửa được
        self.assertEqual((st["repairs"], V, it["q"]), (0, 30, .3))
        V, it, st, cash = go(1.0, 3, 1)          # chắc chắn là hàng giả thì không đáng vá
        self.assertEqual(st["repairs"], 0)
        V, it, st, cash = go(0.0, 3, 1, cash=0)  # hết tiền
        self.assertEqual(st["repairs"], 0)
        self.assertGreater(self.rl["repair_big"], self.rl["repair_light"])

    def test_repair_not_free(self):
        """Phí vá lỗi nhẹ lớn hơn phần giá lỗi lấy lại khi món có nhiều lỗi nhẹ (Rất hiếm: 30% / 3 = 10% mỗi lỗi)."""
        self.assertGreater(self.rl["repair_light"], self.rl["light_cut"] / self.rl["n_light"]["X"])

    def test_thuong_sells_fast(self):
        pf = I.PROFILES["giỏi"]
        self.assertEqual(I.real_value("T", pf, self.rl, 1, 1, 0), self.rl["open"])
        self.assertGreater(I.real_value("X", pf, self.rl, 3, 3, 0), self.rl["open"])

    def test_wrong_calls_have_consequences(self):
        """Thật mà khai giả chỉ nhận giá hàng giả; giả mà khai thật bị tịch thu; ngày 1 miễn phạt."""
        self.assertEqual(self.rl["fake_x"], .20)
        self.assertEqual(self.rl["fine"], self.rl["fake_x"])
        self.assertIsNone(self.rl["fine_T"])
        self.assertEqual(self.rl["day1_fine"], 0)

    def test_doubt_means_claim_real(self):
        pf, rl = I.PROFILES["giỏi"], self.rl
        self.assertFalse(I.decide_fake(.5, "X", rl["fine"], pf, rl, 3, 3, 0))
        self.assertTrue(I.decide_fake(.95, "X", rl["fine"], pf, rl, 3, 3, 0))

    def test_posterior_moves_with_evidence(self):
        rl, s = self.rl, .75
        self.assertAlmostEqual(I.posterior(.3, 0, 2, s, rl) > 0 and I.posterior(.3, 0, 2, s, rl) < .3, True)
        self.assertGreater(I.posterior(.3, 1, 2, s, rl), .3)
        self.assertEqual(I.posterior(0, 2, 2, s, rl), 0)   # Kim Khí không có hàng giả
        self.assertEqual(I.posterior(.3, 2, 2, s, rl), 1.0)  # hai dấu giả: thật không thể có hai dấu lạ
        self.assertEqual(I.posterior(.3, 1, 2, s, dict(rl, odd=0.0)), 1.0)  # không dấu lạ: thấy một dấu là chắc giả

    def test_hint_has_value_only_when_unsure(self):
        pf, rl = I.PROFILES["giỏi"], self.rl
        self.assertGreaterEqual(I.hint_voi(.3, 0, 2, .75, "X", rl["fine"], pf, rl, 2, 3, 1), -1e-9)
        self.assertAlmostEqual(I.hint_voi(0.0, 0, 2, .75, "X", rl["fine"], pf, rl, 3, 3, 0), 0.0)


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
        """Bot thường (soi thường) khoảng 12% đến 16% (khó hơn, chủ dự án chốt 2026-11-25) (n = 2.000: 12,7% và 14,2%)."""
        for b in I.BOT:
            self.assertTrue(9 <= self.w[(b, "thường")] <= 19, (b, self.w[(b, "thường")]))

    def test_perfect_player_not_too_easy(self):
        """Mục tiêu không quá 42% (n = 2.000: 40,8%). Ở n = 600 sai số khoảng 2 điểm nên ngưỡng test là 44."""
        for k, v in self.skilled.items():
            self.assertLessEqual(v, 44, (k, v))

    def avg(self, prof):
        return sum(self.w[(b, prof)] for b in I.BOT) / len(I.BOT)

    def test_inspection_skill_pays(self):
        """Cùng cách mua rương: soi giỏi > soi thường > soi ẩu; bỏ qua soi kém soi thường (n = 2.000, trung bình hai bot: 18,6 / 13,5 / 10,3 / 9,3)."""
        g, t, a, s = (self.avg(p) for p in ("giỏi", "thường", "ẩu", "bỏ qua soi"))
        self.assertGreaterEqual(g - t, 3, (g, t))
        self.assertGreater(t, a, (t, a))
        self.assertGreater(t, s, (t, s))

    def test_careless_still_playable(self):
        """Soi ẩu bị phạt rõ nhưng vẫn còn đường thắng."""
        for b in I.BOT:
            self.assertGreater(self.w[(b, "ẩu")], 5, (b, self.w[(b, "ẩu")]))

    def test_first_run_day1_never_lost(self):
        for prof, d in self.day1.items():
            self.assertEqual(d["lost_day1"], 0, (prof, d))
            self.assertEqual(d["lost_before_day3"], 0, (prof, d))
            self.assertGreater(d["win"], 1.5, (prof, d))


if __name__ == "__main__":
    unittest.main(verbosity=2)

"""Khóa Sprint 5 phần 1: bảng 4 rương cơ bản ĐỀ XUẤT (chest_sim.FINAL), 5 bậc, rác, 4 ô tạo tác.
Số liệu đầy đủ (n = 2.000): Docs/Balance/Sprint5Ruong.md. Ở đây n nhỏ để chạy nhanh (khoảng 1 đến 2 phút), biên rộng hơn.
Hạt giống cố định (0..N-1) nên kết quả lặp lại được. Số tiền trong mã tính theo đơn vị cân bằng 10.000 đ (Gỗ price=140 là 1.400.000 đ)."""
import unittest
import chest_sim as CS
import clue_sim as C
import proposal_sim as P

N = 400
N_DAY1 = 300


class ChestRules(unittest.TestCase):
    """Luật hai bước và bảng dữ liệu, không cần mô phỏng."""

    def test_two_step_weights(self):
        for name, ch in CS.FINAL_CHESTS.items():
            w = CS.weights(ch)
            tot = sum(w.values())
            self.assertAlmostEqual(tot, 1000, delta=1e-6, msg=name)
            junk = sum(v for k, v in w.items() if k in CS.JUNK)
            self.assertAlmostEqual(junk / 1000, ch["tiers"]["R"], places=9)
            for t, pool in ch["pools"].items():
                p = ch["tiers"].get(t, 0)
                for i in pool:   # mỗi món một rương: trọng số = tỉ lệ bậc / số món của bậc
                    self.assertAlmostEqual(w.get(i, 0) / 1000, p / len(pool), places=9, msg=(name, i))

    def test_each_item_in_one_chest(self):
        seen = {}
        for name, ch in CS.FINAL_CHESTS.items():
            for pool in ch["pools"].values():
                for i in pool:
                    self.assertNotIn(i, seen, (i, name, seen.get(i)))
                    seen[i] = name

    def test_junk_order(self):
        r = {n: ch["tiers"]["R"] for n, ch in CS.FINAL_CHESTS.items()}
        # Bảng chủ dự án duyệt 2026-11-24: rác Vải Niêm 30% (cao nhất), Đen 25%, Gỗ ít, Kim Khí vài món.
        # Lệch với "Đen nhiều nhất" của ruong-suu-tap: câu hỏi mở trong Sprint5Ruong.md.
        self.assertEqual(r, {"Gỗ": .18, "Vải Niêm": .30, "Kim Khí": .10, "Đen": .25})
        self.assertLess(r["Gỗ"], r["Vải Niêm"])
        self.assertLessEqual(r["Kim Khí"], r["Gỗ"])

    def test_wood_has_small_very_rare(self):
        """Chủ dự án: Gỗ có tỉ lệ nhỏ Rất hiếm; Đen giữ 12.000.000 đ (price=1200)."""
        self.assertTrue(0 < CS.FINAL_CHESTS["Gỗ"]["tiers"]["X"] <= .02)
        self.assertEqual(CS.FINAL_CHESTS["Đen"]["price"], 1200)

    def test_spec_start_table_is_a_trap(self):
        """Bảng khởi đầu của spec: Kim Khí chỉ ra tạo tác và Đen đều lỗ kỳ vọng."""
        CS.apply(CS.DEFAULT)
        try:
            t = CS.box_table()
        finally:
            CS.restore()
        self.assertLess(t["Kim Khí"]["ratio"], .6)
        self.assertLess(t["Đen"]["ratio"], .7)


class ChestBalance(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.c = CS.apply(CS.FINAL)
        C.setup_done = True
        cls.table = CS.box_table()
        cls.lad = P.measure(cls.c, ["thận trọng", "liều"], n=N)
        cls.skilled = C.measure_turns(cls.c, N, lam=.75, tau=1.0)
        cls.mix = CS.mix_of(cls.c, N)
        cls.day1 = CS.day1_stats(cls.c, P.STRATS["liều"], N_DAY1)
        cls.day1_script = {}
        for mode in ("any", "cheap"):
            cls.c = CS.apply(dict(CS.FINAL, script=mode))
            cls.day1_script[mode] = CS.day1_stats(cls.c, P.STRATS["liều"], N_DAY1)
        CS.restore()

    def test_every_chest_profitable_in_expectation(self):
        for n, r in self.table.items():
            self.assertGreater(r["ratio"], 1.0, (n, r))

    def test_cheap_chest_not_best_profit(self):
        """Gỗ lời theo tỉ lệ cao nhất (an toàn) nhưng không lời tuyệt đối nhiều nhất."""
        best = max(self.table, key=lambda n: self.table[n]["profit"])
        self.assertNotEqual(best, "Gỗ")

    def test_plain_bots_in_target(self):
        for k in ("thận trọng", "liều"):
            self.assertTrue(13 <= self.lad[k]["win"] <= 24, (k, self.lad[k]["win"]))

    def test_perfect_player_not_too_easy(self):
        for k, v in self.skilled.items():
            if not k.endswith("[mua]"):
                self.assertLessEqual(v, 47, (k, v))

    def test_skill_gap_is_kept(self):
        gap = self.skilled["thận trọng + giám định"] - self.lad["thận trọng"]["win"]
        self.assertGreaterEqual(gap, 12, gap)

    def test_chest_mix(self):
        for n, p in self.mix.items():
            self.assertLessEqual(p, 50, self.mix)
        self.assertTrue(5 <= self.mix["Đen"] <= 25, self.mix)

    def test_day1_never_lost(self):
        for d in [self.day1] + list(self.day1_script.values()):
            self.assertEqual(d["lost_day1"], 0, d)
            self.assertEqual(d["lost_before_day3"], 0, d)
            self.assertLess(d["arrears1_max"], 1000 * .3, d)   # dưới 3.000.000 đ, xa ngưỡng thua 10.000.000 đ (số trong mô phỏng tính theo đơn vị 10.000 đ)

    def test_scripted_run_still_winnable(self):
        """Lần chơi đầu (hộp sắp sẵn) khó hơn nhưng đường thắng vẫn còn."""
        for mode, d in self.day1_script.items():
            self.assertGreater(d["win"], 3, (mode, d))


if __name__ == "__main__":
    unittest.main(verbosity=2)

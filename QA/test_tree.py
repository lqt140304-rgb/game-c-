"""Khóa Sprint 5 phần 4: cây phát triển và điểm phát triển ĐỀ XUẤT (tree_sim.TREE, POINTS) trên nền phần 1 đến 3.
Số liệu đầy đủ: Docs/Balance/Sprint5CayPhatTrien.md. Ở đây n nhỏ (khoảng 1 phút), hạt giống cố định.
Số tiền trong mã tính theo đơn vị 10.000 đ."""
import unittest
import tree_sim as T

BRANCHES = ("Rương", "Sưu tập", "Thần lực", "Câu chuyện")
PLAY_NODES = {"R2", "R3", "R4", "R5", "M3", "M1", "M2"}   # nút có tác động lên ván (rương mới, tùy chọn, M1, M2)


class TreeShape(unittest.TestCase):
    def test_four_branches_parents_and_costs(self):
        ids = {n["id"] for n in T.TREE}
        self.assertEqual(len(ids), len(T.TREE))
        self.assertEqual({n["branch"] for n in T.TREE}, set(BRANCHES))
        for n in T.TREE:
            self.assertTrue(n["parent"] is None or n["parent"] in ids, n["id"])
            if n["parent"]:
                self.assertEqual(T.TREE_BY_ID[n["parent"]]["branch"], n["branch"], n["id"])
                self.assertGreaterEqual(n["cost"], T.TREE_BY_ID[n["parent"]]["cost"], n["id"])  # đi sâu thì đắt hơn
            self.assertIsInstance(n["cost"], int)
        for b in BRANCHES:
            self.assertEqual(sum(1 for n in T.TREE if n["branch"] == b and n["parent"] is None), 1, b)

    def test_no_play_buff_except_listed(self):
        """ruong-suu-tap mục 4: phần thưởng meta không buff chơi. Chỉ rương mới, M3 (tùy chọn), M1 (chọn), M2 có tác động."""
        self.assertEqual({n["id"] for n in T.TREE if n["effect"]}, PLAY_NODES)
        self.assertEqual(T.CASH_BONUS, 0)   # đề xuất: M2 không cộng vốn
        self.assertIsNone(T.TREE_BY_ID["M4"]["effect"])

    def test_old_unlocks_kept_as_nodes(self):
        for m in ("M1", "M2", "M3", "M4"):
            self.assertIn(m, T.TREE_BY_ID)

    def test_points_win_more_than_loss(self):
        p = T.POINTS
        self.assertGreater(p["loss"], 0)
        self.assertGreater(p["win"], p["loss"] + p["loss_day_cap"] * p["loss_day"])


class Pacing(unittest.TestCase):
    """Chuỗi ván của bot thường (soi thường) và người soi ẩu: nhịp mở nút."""

    @classmethod
    def setUpClass(cls):
        cls.bot = T.chain("thận trọng", players=240, runs=45, seed0=5_000_000)
        cls.weak = T.chain("thận trọng", players=160, runs=55, seed0=6_000_000, profile="ẩu")

    def test_first_node_after_a_few_runs(self):
        ms = T.milestones(*self.bot)
        self.assertTrue(2 <= ms["nút đầu"]["med"] <= 3, ms["nút đầu"])

    def test_half_and_full_tree(self):
        ms = T.milestones(*self.bot)
        self.assertTrue(8 <= ms["nửa cây"]["med"] <= 14, ms["nửa cây"])
        self.assertTrue(32 <= ms["cả cây"]["med"] <= 44, ms["cả cây"])

    def test_losing_player_still_progresses(self):
        m, h = self.weak
        ms = T.milestones(m, h)
        self.assertLessEqual(ms["nửa cây"]["med"], 16, ms)
        self.assertGreaterEqual(ms["cả cây"]["done"], 50, ms)
        gaps = []
        for mm in m:
            rs = sorted(mm.unlock_run.values())
            rs = [r for r in rs if r <= 30]
            gaps.append(max(b - a for a, b in zip([0] + rs, rs)) if rs else 99)
        gaps.sort()
        self.assertLessEqual(gaps[len(gaps) // 2], 6)   # trung vị: không quá 6 ván liền không mở nút nào (30 ván đầu)

    def test_bot_win_rate_stays_in_band(self):
        blocks = T.win_by_run(self.bot[1], width=15)
        for b in blocks:
            self.assertTrue(12.0 <= b <= 20.0, blocks)


class Ceiling(unittest.TestCase):
    """Cấu hình mạnh nhất mà cây cho phép (4 rương mở khóa, 2 chủ đề, M3, M1, không M2): người chơi giỏi dưới trần."""

    def test_full_tree_skilled_under_cap(self):
        w = T.static("liều + giám định", n=500, chests=T.UNLOCK_NAMES, n_theme=2, rd_early=T.RD_BEST.get("liều + giám định", False),
                     gt=T.GT_BEST.get("liều + giám định"))
        self.assertLessEqual(w, 49.0)   # trần 47%, cộng nhiễu ở n = 500

    def test_full_tree_bot_in_band(self):
        for b in ("thận trọng", "liều"):
            w = T.static(b, n=2000, chests=T.UNLOCK_NAMES, n_theme=2)
            self.assertTrue(13.0 <= w <= 20.0, (b, w))


if __name__ == "__main__":
    unittest.main()

"""Khóa Sprint 5 phần 4 (bản MỚI, 2026-11-25): cây kỹ năng CHỈ TRONG VÁN (tree_run.RUN_TREE), không tiến trình vĩnh viễn.
Số liệu đầy đủ: Docs/Balance/Sprint5CayPhatTrien.md. Bản chuỗi nhiều ván cũ (tree_sim.chain) đã bỏ, không còn test.
Số tiền trong mã tính theo đơn vị 10.000 đ. n nhỏ (khoảng 2 phút), hạt giống cố định, ngưỡng có biên cho nhiễu."""
import unittest
import tree_run as R

BRANCHES = ("Rương", "Sưu tập", "Thần lực", "Câu chuyện")
PLAY_NODES = {"R2", "R3", "R4", "R5", "M1", "M2"}   # nút có tác động lên ván


class TreeShape(unittest.TestCase):
    def test_four_branches_parents_and_costs(self):
        ids = {n["id"] for n in R.RUN_TREE}
        self.assertEqual(len(ids), len(R.RUN_TREE))
        self.assertEqual({n["branch"] for n in R.RUN_TREE}, set(BRANCHES))
        for n in R.RUN_TREE:
            self.assertTrue(n["parent"] is None or n["parent"] in ids, n["id"])
            if n["parent"]:
                self.assertEqual(R.BY_ID[n["parent"]]["branch"], n["branch"], n["id"])
                self.assertGreaterEqual(n["cost"], R.BY_ID[n["parent"]]["cost"], n["id"])
        for b in BRANCHES:
            self.assertEqual(sum(1 for n in R.RUN_TREE if n["branch"] == b and n["parent"] is None), 1, b)

    def test_only_listed_nodes_touch_play(self):
        self.assertEqual({n["id"] for n in R.RUN_TREE if n["effect"]}, PLAY_NODES)
        self.assertIsNone(R.BY_ID["M4"]["effect"])

    def test_no_cross_run_conditions(self):
        """Không điều kiện nào dựa vào ván trước (wins, runs, bk): cây làm lại mỗi ván."""
        for n in R.RUN_TREE:
            self.assertNotIn(n["cond"][0], ("wins", "runs", "bk"), n["id"])


class InRun(unittest.TestCase):
    """Cây trong một ván 7 ngày, giá nút một nửa, nguồn điểm 3 mỗi ngày, rương chỉ mở qua cây, không chủ đề."""
    KW = dict(source="ngày 3", scale=.5, themes=False)

    @classmethod
    def setUpClass(cls):
        cls.bot = R.run_static("thận trọng", 500, seed0=5_000_000, **cls.KW)
        cls.pro = R.run_static("liều + giám định", 500, seed0=5_000_000, **cls.KW)

    def test_first_node_early(self):
        for r in (self.bot, self.pro):
            pct, day = r["opened"]["R1"]
            self.assertGreaterEqual(pct, 75)
            self.assertTrue(2 <= day <= 3, r["opened"]["R1"])

    def test_tree_is_partial_in_one_run(self):
        """Một ván 7 ngày chỉ mở được khoảng 4 đến 7 nút trên 19."""
        for r in (self.bot, self.pro):
            self.assertTrue(4 <= r["nodes"] <= 7.5, r["nodes"])

    def test_late_nodes_rarely_open(self):
        self.assertLess(self.bot["opened"].get("M2", (0, 0))[0], 10)

    def test_bot_in_range(self):
        self.assertTrue(11 <= self.bot["win"] <= 21, self.bot["win"])

    def test_perfect_player_below_ceiling(self):
        """Trần 47% (n nhỏ, sai số ±2,2 điểm): khóa ở 50 để bắt hồi quy lớn; số đo n lớn ở báo cáo."""
        self.assertLessEqual(self.pro["win"], 50, self.pro["win"])


if __name__ == "__main__":
    unittest.main()

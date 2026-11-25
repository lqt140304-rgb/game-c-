"""Khóa Sprint 5 phần 2: rương mở khóa (chest_unlock.UNLOCK) và khung rương chủ đề (chest_unlock.THEME), ĐỀ XUẤT, chưa chốt.
Nền là 4 rương cơ bản đã chốt (chest_sim.FINAL). Số liệu đầy đủ: Docs/Balance/Sprint5RuongMoKhoa.md.
Ở đây đo kịch bản 'tất cả' (4 cơ bản + 4 mở khóa + 2 chủ đề đã mở, shop bày tối đa 4 rương mới mỗi ngày theo luật đã chốt) với n nhỏ (khoảng 2 phút), biên rộng hơn.
Số tiền trong mã tính theo đơn vị 10.000 đ (price=600 là 6.000.000 đ)."""
import unittest
import chest_sim as CS
import chest_unlock as U
import clue_sim as C
import proposal_sim as P

N = 400


class UnlockRules(unittest.TestCase):
    def setUp(self):
        self.sc = U.scenario("tat_ca")

    def test_weights_sum_and_art_system(self):
        for name, ch in self.sc["extra"].items():
            w = U.weights(ch)
            self.assertAlmostEqual(sum(w.values()), 1000, delta=1e-6, msg=name)
            arts = [k for k in w if k.startswith("AX_")]
            if ch.get("art_sys"):
                self.assertTrue(arts and all(k[3:5] == ch["art_sys"] for k in arts), (name, arts))
            else:
                self.assertEqual(arts, [], name)

    def test_tong_hop_only_basic_items(self):
        basic = {i for ch in CS.FINAL_CHESTS.values() for pool in ch["pools"].values() for i in pool}
        th = {i for pool in U.UNLOCK["Tổng Hợp"]["pools"].values() for i in pool}
        self.assertEqual(th, basic)

    def test_each_item_in_one_chest_except_tong_hop(self):
        seen = {}
        chests = dict(CS.FINAL_CHESTS, **{k: v for k, v in self.sc["extra"].items() if k != "Tổng Hợp"})
        for name, ch in chests.items():
            for pool in ch["pools"].values():
                for i in pool:
                    self.assertNotIn(i, seen, (i, name, seen.get(i)))
                    seen[i] = name

    def test_theme_legend_dearer_than_basic(self):
        basic_leg = max(CS.NEW_ITEMS[ch["pools"]["L"][0]][1] for ch in CS.FINAL_CHESTS.values())
        self.assertGreater(U.THEME_LEG, basic_leg)

    def test_prices_are_whole_units(self):
        for name, ch in list(U.UNLOCK.items()) + [("chủ đề", U.THEME)]:
            self.assertIsInstance(ch["price"], int, name)   # bội số 10.000 đ
        self.assertIsInstance(U.THEME_LEG, int)


class UnlockBalance(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        c = U.apply(U.scenario("tat_ca", shop_extra=4))   # luật shop đã chốt: bày tối đa 4 rương mới mỗi ngày
        C.setup_done = True
        try:
            cls.table = U.box_table()
            cls.lad = P.measure(c, ["thận trọng", "liều"], n=N)
            cls.skilled = C.measure_turns(c, N, lam=.75, tau=1.0)
            cls.mix = U.mix_of(c, N)
        finally:
            U.restore()

    def test_every_new_chest_profitable(self):
        for n, r in self.table.items():
            self.assertGreater(r["ratio"], 1.0, (n, r))

    def test_plain_bots_in_target(self):
        for k in ("thận trọng", "liều"):
            self.assertTrue(13 <= self.lad[k]["win"] <= 24, (k, self.lad[k]["win"]))

    def test_perfect_player_not_too_easy(self):
        for k, v in self.skilled.items():
            if not k.endswith("[mua]"):
                self.assertLessEqual(v, 48, (k, v))

    def test_no_chest_dominates(self):
        self.assertLessEqual(max(self.mix.values()), 50, self.mix)

    def test_restore_leaves_core_clean(self):
        self.assertFalse(any(k.startswith("U") for k in CS.BOXES))
        self.assertIs(P.ladder5, U._ORIG["ladder5"])


if __name__ == "__main__":
    unittest.main(verbosity=2)

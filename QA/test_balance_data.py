"""Kiểm tra Docs/Content/data/balance_sprint5.json (bảng số liệu cân bằng Sprint 5 cho Codex nạp) khớp mô phỏng.
Không chạy mô phỏng nặng: chỉ so với hằng số trong chest_sim, chest_unlock, inspect_sim, tree_run. Chạy dưới 5 giây.
Chạy: cd QA && python3 -m unittest -v test_balance_data
"""
import json
import os
import unittest

import chest_sim as CS
import chest_unlock as U
import inspect_sim as I
import tree_run as R

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
PATH = os.path.join(ROOT, "Docs", "Content", "data", "balance_sprint5.json")
with open(PATH, encoding="utf-8") as f:
    D = json.load(f)
UNIT = 10_000                      # đơn vị cân bằng của mô phỏng = 10.000 đ
STATUSES = {"ĐÃ CHỐT", "ĐỀ XUẤT", "GIẢ ĐỊNH"}
ALL_CHESTS = D["chests_basic"] + D["chests_unlock"]


def walk(o, path=""):
    """Mọi cặp (đường dẫn, khóa, giá trị) trong JSON."""
    if isinstance(o, dict):
        for k, v in o.items():
            yield path, k, v
            yield from walk(v, f"{path}/{k}")
    elif isinstance(o, list):
        for i, v in enumerate(o):
            yield from walk(v, f"{path}[{i}]")


class Money(unittest.TestCase):
    def test_vnd_fields_are_whole_dong(self):
        n = 0
        for p, k, v in walk(D):
            if k.endswith("_vnd"):
                vals = v.values() if isinstance(v, dict) else v if isinstance(v, list) else [v]
                if isinstance(v, dict) and "value" in v:
                    vals = [v["value"]]
                for x in vals:
                    xs = x.values() if isinstance(x, dict) else x if isinstance(x, list) else [x]
                    for y in xs:
                        if isinstance(y, (int, float)):
                            n += 1
                            self.assertIsInstance(y, int, f"{p}/{k}")
        self.assertGreater(n, 40)

    def test_prices_are_multiples_of_10000(self):
        for c in ALL_CHESTS + [D["chest_theme_frame"]]:
            self.assertEqual(c["price_vnd"] % UNIT, 0, c.get("name"))
        for it in list(D["items"].values()) + list(D["junk_items"].values()):
            self.assertEqual(it["base_price_vnd"] % UNIT, 0, it["id"])
        self.assertEqual(D["chest_theme_frame"]["legend_base_price_vnd"] % UNIT, 0)
        self.assertEqual(D["inspect"]["professor_hint"]["min_fee_vnd"] % UNIT, 0)
        self.assertEqual(D["economy"]["start_cash_vnd"]["value"], 10_000_000)
        for d in D["economy"]["debt_vnd"]["value"]:
            self.assertEqual(d % UNIT, 0)

    def test_no_xu_unit(self):
        import re
        text = json.dumps(D, ensure_ascii=False).lower()
        self.assertIsNone(re.search(r"\bxu\b", text))


class Statuses(unittest.TestCase):
    def test_every_block_has_status_and_source(self):
        blocks = 0
        for p, k, v in walk(D):
            if isinstance(v, dict) and "status" in v and isinstance(v["status"], str):
                blocks += 1
                self.assertIn(v["status"], STATUSES, p + "/" + k)
                self.assertTrue(v.get("source"), p + "/" + k)
        self.assertGreater(blocks, 25)

    def test_chests_and_items_have_status(self):
        for c in ALL_CHESTS + [D["chest_theme_frame"]]:
            self.assertIn(c["status"]["status"], STATUSES, c.get("name"))
            self.assertTrue(c["status"]["source"])
        for it in D["items"].values():
            self.assertIn(it["status"], STATUSES)
            self.assertTrue(it["source"])

    def test_basic_chests_locked_and_unlock_proposed(self):
        self.assertTrue(all(c["status"]["status"] == "ĐÃ CHỐT" for c in D["chests_basic"]))
        self.assertTrue(all(c["status"]["status"] == "ĐỀ XUẤT" for c in D["chests_unlock"]))


class Chests(unittest.TestCase):
    def test_rates_sum_to_one(self):
        for c in ALL_CHESTS + [D["chest_theme_frame"]]:
            tot = sum(c["tier_rates"].values()) + c["artifact_share"]
            self.assertAlmostEqual(tot, 1.0, places=6, msg=c.get("name", "chủ đề"))
            self.assertEqual(set(c["tier_rates"]), set("RTHXL"))

    def test_basic_match_chest_sim(self):
        by_name = {c["name"]: c for c in D["chests_basic"]}
        self.assertEqual(set(by_name), set(CS.FINAL_CHESTS))
        for name, sim in CS.FINAL_CHESTS.items():
            j = by_name[name]
            self.assertEqual(j["price_vnd"], sim["price"] * UNIT, name)
            self.assertEqual(j["fake_rate"], sim["fake"], name)
            self.assertEqual(j["true_clues_of_3"], sim["ntrue"], name)
            for t in "RTHXL":
                self.assertAlmostEqual(j["tier_rates"][t], sim["tiers"].get(t, 0), places=6, msg=f"{name} {t}")
            self.assertEqual(j["pools"], sim["pools"], name)
            if "art_mix" in sim:
                self.assertEqual(tuple(j["artifact_tier_mix"].values()), tuple(sim["art_mix"]))
                self.assertAlmostEqual(j["artifact_share"], 1 - sum(sim["tiers"].values()), places=6)
            else:
                self.assertEqual(j["artifact_share"], 0)

    def test_locked_basic_values(self):
        """Giá và bậc đã chốt (DaChot.md, Sprint5Ruong.md mục 1) ghi cứng, để sửa mô phỏng mà quên JSON thì đỏ."""
        want = {"Gỗ": (1_400_000, [.18, .795, .01, .01, .005]), "Vải Niêm": (3_500_000, [.30, .39, .25, .05, .01]),
                "Kim Khí": (4_500_000, [.10, .25, .22, .08, .01]), "Đen": (12_000_000, [.30, .03, .04, .58, .05])}
        for c in D["chests_basic"]:
            price, rates = want[c["name"]]
            self.assertEqual(c["price_vnd"], price)
            for t, r in zip("RTHXL", rates):
                self.assertAlmostEqual(c["tier_rates"][t], r, places=6, msg=c["name"])
        self.assertAlmostEqual(next(c for c in D["chests_basic"] if c["name"] == "Kim Khí")["artifact_share"], .34)

    def test_unlock_and_theme_match_chest_unlock(self):
        by_name = {c["name"]: c for c in D["chests_unlock"]}
        self.assertEqual(set(by_name), set(U.UNLOCK))
        for name, sim in U.UNLOCK.items():
            j = by_name[name]
            self.assertEqual(j["price_vnd"], sim["price"] * UNIT, name)
            self.assertEqual(j["fake_rate"], sim["fake"], name)
            for t in "RTHXL":
                self.assertAlmostEqual(j["tier_rates"][t], sim["tiers"].get(t, 0), places=6, msg=f"{name} {t}")
        th = D["chest_theme_frame"]
        self.assertEqual(th["price_vnd"], U.THEME["price"] * UNIT)
        self.assertEqual(th["legend_base_price_vnd"], U.THEME_LEG * UNIT)
        self.assertEqual(th["fake_rate"], U.THEME["fake"])
        for t in "RTHXL":
            self.assertAlmostEqual(th["tier_rates"][t], U.THEME["tiers"][t], places=6)

    def test_pool_items_exist_and_tiers_match(self):
        for c in D["chests_basic"]:
            for t, ids in c["pools"].items():
                for i in ids:
                    self.assertIn(i, D["items"], i)
                    self.assertEqual(D["items"][i]["tier"], t, i)
                    self.assertEqual(D["items"][i]["base_price_vnd"], CS.NEW_ITEMS[i][1] * UNIT, i)
        self.assertEqual(len(D["junk_items"]), len(CS.JUNK))
        for i in D["junk_items"].values():
            self.assertEqual(i["base_price_vnd"], CS.JUNK_BASE * UNIT)

    def test_every_chest_with_items_has_every_tier_it_can_roll(self):
        """Bậc có tỉ lệ > 0 thì bộ sưu tập phải có món ở bậc đó (trừ Rác dùng chung)."""
        for c in D["chests_basic"]:
            for t in "THXL":
                if c["tier_rates"][t] > 0:
                    self.assertTrue(c["pools"].get(t), f"{c['name']} thiếu món bậc {t}")

    def test_basic_expected_return_matches_report(self):
        """Thu kỳ vọng / giá tính chéo từ bảng bậc và giá món (hệ số tình trạng 0,79, hàng giả bán 20%) khớp báo cáo
        Sprint5Ruong.md mục 1: Vải Niêm 1,51, Đen 1,08. Gỗ tính chéo ra 2,56 còn báo cáo ghi 2,29 (điểm lệch, xem BangSoLieuSprint5.md).
        Kim Khí bỏ qua (tạo tác tính theo giá Kho, không nằm trong JSON này)."""
        q = sum(p * m for p, m in ((.5, 1.0), (.35, .7), (.15, .3)))
        pr = {}
        for c in D["chests_basic"]:
            if c["name"] == "Kim Khí":
                continue
            ev = c["tier_rates"]["R"] * D["junk_items"]["R01"]["base_price_vnd"]
            for t in "THXL":
                ids = c["pools"][t]
                ev += c["tier_rates"][t] * sum(D["items"][i]["base_price_vnd"] for i in ids) / len(ids)
            pr[c["name"]] = ev * q * (1 - .8 * c["fake_rate"]) / c["price_vnd"]
        self.assertAlmostEqual(pr["Vải Niêm"], 1.51, delta=.04)
        self.assertAlmostEqual(pr["Đen"], 1.08, delta=.04)
        self.assertGreater(pr["Gỗ"], 2.2)
        self.assertLess(pr["Gỗ"], 2.7)


class Economy(unittest.TestCase):
    def test_core_numbers(self):
        e = D["economy"]
        self.assertEqual(e["equip_slots"]["value"], CS.DEFAULT["slots"])
        self.assertEqual(e["equip_slots"]["with_bk1"], CS.DEFAULT["slots"] + 1)
        self.assertEqual(e["duplicate_artifact_refund_of_kho_price"]["value"], CS.DEFAULT["dup_frac"])
        self.assertEqual(e["box_price_swing"]["value"], CS.DEFAULT["amp"])
        den = next(c for c in D["chests_basic"] if c["name"] == "Đen")
        self.assertEqual(den["available_from_day"], CS.DEFAULT["rd_from_day"])
        self.assertEqual(e["junk_sell_price_vnd"]["value"], CS.JUNK_BASE * UNIT)

    def test_first_run_day1_shop(self):
        fr = D["first_run"]["day1_shop_only"]["value"]
        self.assertEqual(sorted(fr), sorted(["GO", "VAI_NIEM"]))
        self.assertEqual(sorted(D["chests_basic"][i]["name"] for i in (0, 1)), sorted(CS.FINAL["day1_shop"]))


class Inspect(unittest.TestCase):
    def test_rules_match_inspect_sim(self):
        s, r = D["inspect"], I.RULES
        self.assertEqual(s["light_defects_per_scratched_item"]["value"], r["n_light"])
        self.assertEqual(s["light_defect_cut"]["value"], r["light_cut"])
        self.assertEqual(s["big_defect_cut"]["value"], r["big_cut"])
        self.assertEqual(s["fake_marks_per_fake_item"]["value"], r["marks"])
        self.assertEqual(s["odd_mark_rate_on_genuine"]["value"], r["odd"])
        self.assertEqual(s["missed_defect_penalty_of_appraised_price"]["value"], r["miss_pen"])
        self.assertEqual(s["fake_sell_fraction"]["value"], r["fake_x"])
        self.assertEqual(s["fake_declared_genuine_fine"]["value"], r["fine"])
        self.assertEqual(s["day1_fine"]["value"], r["day1_fine"])
        self.assertEqual(s["repair_shop"]["light_fee_of_base_price"], r["repair_light"])
        self.assertEqual(s["repair_shop"]["big_fee_of_base_price"], r["repair_big"])
        h = s["haggle"]
        self.assertEqual((h["rounds"], h["open_fraction"], h["win_step"], h["lose_step"], h["floor_fraction"], h["cap_fraction"]),
                         (r["rounds"], r["open"], r["up"], r["down"], r["floor"], r["cap"]))
        self.assertEqual(s["professor_hint"]["rate_of_base_price"], r["hint_rate"])

    def test_profiles_match(self):
        for name, p in D["inspect"]["profiles"]["value"].items():
            self.assertEqual(p["find_rate"], I.PROFILES[name]["s"], name)
            self.assertEqual(p["haggle_skill"], I.PROFILES[name]["c"], name)
        self.assertEqual(set(D["inspect"]["profiles"]["value"]), set(I.PROFILES))

    def test_locked_values(self):
        s = D["inspect"]
        self.assertEqual(s["time_limit_seconds"]["value"], 120)
        self.assertEqual(s["time_limit_seconds"]["status"], "ĐÃ CHỐT")
        self.assertEqual(s["odd_mark_rate_on_genuine"]["value"], 0.15)
        self.assertTrue(s["repair_shop"]["exists"])
        self.assertTrue(s["skip_defect_click_allowed"]["value"])
        self.assertEqual(s["fake_sell_fraction"]["value"], 0.2)
        # Món Hỏng trừ 70%, Xước trừ 30%: khớp hệ số tình trạng 0,7 và 0,3
        q = s["condition_price_mult"]["value"]
        self.assertAlmostEqual(1 - s["light_defect_cut"]["value"], q["xuoc"])
        self.assertAlmostEqual(1 - s["light_defect_cut"]["value"] - s["big_defect_cut"]["value"], q["hong"])


class Tree(unittest.TestCase):
    T = D["tree"]

    def test_nodes_match_run_tree(self):
        sim = {n["id"]: n for n in R.RUN_TREE if n["branch"] != "Câu chuyện"}
        js = {n["id"]: n for n in self.T["nodes"]}
        self.assertEqual(set(js), set(sim))
        for i, n in js.items():
            self.assertEqual(n["cost_points"], sim[i]["cost"], i)
            self.assertEqual(n["parent"], sim[i]["parent"], i)
            self.assertEqual(n["branch"], sim[i]["branch"], i)

    def test_decisions_reflected(self):
        ids = {n["id"] for n in self.T["nodes"]}
        self.assertNotIn("M3", ids)
        self.assertFalse(ids & {"C1", "C2", "C3", "C4"})
        self.assertIn("Đời sống", self.T["branches"])
        self.assertEqual(self.T["empty_branches"], ["Đời sống"])
        self.assertFalse([n for n in self.T["nodes"] if n["branch"] == "Đời sống"])
        m2 = next(n for n in self.T["nodes"] if n["id"] == "M2")
        self.assertIsNone(m2["effect"])           # đã bỏ phần cộng vốn
        self.assertEqual(self.T["scope"]["value"], "in_run_only")
        self.assertEqual(self.T["reward_cash_cap_vnd"]["value"], 1_000_000)

    def test_tree_shape(self):
        by = {n["id"]: n for n in self.T["nodes"]}
        for n in by.values():
            self.assertTrue(n["parent"] is None or n["parent"] in by)
            if n["parent"]:
                self.assertEqual(by[n["parent"]]["branch"], n["branch"])
                self.assertGreaterEqual(n["cost_points"], by[n["parent"]]["cost_points"])
        self.assertEqual(sum(n["cost_points"] for n in by.values()), 79)
        # chỉ vài nút mở rương ở nhánh Rương
        chests = {n["effect"]["chest"] for n in by.values() if n["effect"] and "chest" in n["effect"]}
        self.assertEqual(chests, set(U.UNLOCK))

    def test_run_gives_few_points(self):
        """3 điểm mỗi ngày, ván 7 ngày kiếm tối đa 21 điểm, xa tổng cây 79: không mở hết được."""
        self.assertLess(self.T["points_per_day"]["value"] * 7, sum(n["cost_points"] for n in self.T["nodes"]))


if __name__ == "__main__":
    unittest.main()

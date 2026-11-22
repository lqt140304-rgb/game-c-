"""Kiểm tra dữ liệu nguồn Docs/Content/data/content.json khớp với tài liệu và mô hình QA (Claude 1).

Mục đích: số liệu chỉ có một chỗ gốc. Nếu ai đổi giá, tỷ lệ rơi, tỷ lệ hàng giả hoặc luật dịch vụ ở
một nơi mà quên nơi khác thì test này đỏ. Chạy trong thư mục QA: python3 -m unittest -v test_content_data
"""
import json
import math
import os
import re
import unittest

import economy_model as em
import risk_full
import services_model as sv
import sprint3_sim as sm

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
DOCS = os.path.join(ROOT, "Docs", "Content")
with open(os.path.join(DOCS, "data", "content.json"), encoding="utf-8") as f:
    D = json.load(f)

ORIGIN_NAME = {o["id"]: o["name"] for o in D["origins"]}
ITEMS = {i["id"]: i for i in D["items"]}
BOXES = {b["id"]: b for b in D["boxes"]}
ARTS = {a["id"]: a for a in D["artifacts"]}
SIM_ART = {"CP1": "A1", "CP2": "A2", "CP3": "A3", "TC1": "B1", "TC2": "B2", "TC3": "B3"}  # hệ Cổ Phong là A, Thiên Cơ là B


def table_rows(filename, first_cell_pattern):
    """Các dòng bảng Markdown có ô đầu khớp mẫu. Trả về danh sách ô đã bỏ khoảng trắng."""
    rows = []
    with open(os.path.join(DOCS, filename), encoding="utf-8") as f:
        for line in f:
            if line.startswith("|"):
                cells = [c.strip() for c in line.strip().strip("|").split("|")]
                if cells and re.fullmatch(first_cell_pattern, cells[0]):
                    rows.append(cells)
    return rows


def xu(text):
    return int(re.sub(r"[^\d]", "", text.split("xu")[0]))


def pct(text):
    return float(text.replace("%", "").replace(",", ".")) / 100


def fake_rate(box_id, item_id):
    r = BOXES[box_id]["fake_rate"]
    return max(r, D["item_fake_rate_floor"].get(item_id, 0)) if item_id in D["item_fake_rate_floor"] else r


def box_ev(box_id):
    qm = sum(q["p"] * q["price_mult"] for q in D["rules"]["quality"])
    b = BOXES[box_id]
    total = sum(d["weight"] for d in b["drops"])
    e = 0.0
    for d in b["drops"]:
        fr = fake_rate(box_id, d["item"])
        e += d["weight"] / total * ITEMS[d["item"]]["base_price"] * qm * (1 - fr + fr * D["rules"]["fake_price_factor"])
    return e / b["price"]


class Consistency(unittest.TestCase):
    def test_every_drop_table_sums_to_100_and_references_real_items(self):
        for b in D["boxes"]:
            self.assertEqual(100, sum(d["weight"] for d in b["drops"]), b["id"])
            for d in b["drops"]:
                self.assertIn(d["item"], ITEMS, (b["id"], d))

    def test_every_item_and_origin_is_reachable(self):
        dropped = {d["item"] for b in D["boxes"] for d in b["drops"]}
        self.assertEqual(set(ITEMS), dropped)
        for oid in ORIGIN_NAME:
            self.assertTrue(any(i["origin"] == oid for i in D["items"]), oid)
            self.assertTrue(any(oid in b["origins"] for b in D["boxes"]), oid)

    def test_box_value_ladder_makes_bigger_boxes_riskier(self):
        ev = {b: box_ev(b) for b in BOXES}
        self.assertTrue(ev["H1"] > ev["H2"] > ev["H3"] > ev["RD"], ev)
        self.assertLess(ev["RD"], 1.1)

    def test_combos_and_bikhe_reference_real_artifacts(self):
        for c in D["combos"]:
            for a in c["requires"]:
                self.assertIn(a, ARTS, c["id"])
        self.assertEqual(6, len(ARTS))
        self.assertEqual(2, len({a["set"] for a in D["artifacts"]}))

    def test_meta_start_cash_bonus_respects_cap(self):
        bonus = sum(u["effect"]["value"] for u in D["meta_unlocks"] if u["effect"]["type"] == "start_cash_bonus")
        self.assertLessEqual(bonus, D["meta_start_cash_bonus_cap"])

    def test_rare_items_are_the_two_most_expensive(self):
        rare = {i["id"] for i in D["items"] if i["rarity"] == "rat_hiem"}
        top2 = {i["id"] for i in sorted(D["items"], key=lambda i: -i["base_price"])[:2]}
        self.assertEqual(top2, rare)


class MatchesDocs(unittest.TestCase):
    def test_item_table_in_content_doc(self):
        rows = table_rows("NoiDungMau.md", r"V\d\d")
        rows = [r for r in rows if len(r) >= 6]   # bảng hiện vật có 6 cột
        self.assertEqual(10, len(rows))
        rarity = {"Thường": "thuong", "Hiếm": "hiem", "Rất hiếm": "rat_hiem"}
        for r in rows:
            it = ITEMS[r[0]]
            self.assertEqual(it["name"], r[1], r[0])
            self.assertEqual(ORIGIN_NAME[it["origin"]], r[2], r[0])
            self.assertEqual(it["rarity"], rarity[r[3]], r[0])
            self.assertEqual(it["base_price"], xu(r[4]), r[0])

    def test_box_table_in_content_doc(self):
        rows = [r for r in table_rows("NoiDungMau.md", r"H[123]|RD") if len(r) >= 6 and "%" in r[4]]
        self.assertEqual(4, len(rows))
        for r in rows:
            b = BOXES[r[0]]
            self.assertEqual(b["clue_true_count"], int(r[5].split()[0]), r[0])
            self.assertEqual(b["name"], r[1], r[0])
            self.assertEqual(b["price"], xu(r[2]), r[0])
            self.assertAlmostEqual(b["fake_rate"], pct(r[4]), msg=r[0])

    def test_expected_value_table_in_content_doc(self):
        rows = [r for r in table_rows("NoiDungMau.md", r"H[123]|RD") if len(r) == 3 and "lần" in r[2]]
        self.assertEqual(4, len(rows))
        for r in rows:
            doc = float(r[2].split("lần")[0].strip().replace(",", "."))
            self.assertAlmostEqual(doc, box_ev(r[0]), places=2, msg=r[0])

    def test_artifact_table_in_effects_doc(self):
        rows = [r for r in table_rows("GiaTruyenTaoTacBiKhe.md", r"(CP|TC)\d") if len(r) >= 8]
        self.assertEqual(6, len(rows))
        for r in rows:
            a = ARTS[r[0]]
            self.assertEqual(a["name"], r[1], r[0])
            self.assertEqual(a["tier"], int(r[3]), r[0])
            self.assertEqual(a["price"], xu(r[4]), r[0])


class MatchesSimulation(unittest.TestCase):
    def test_item_prices_match_the_approved_table_used_by_claude_2(self):
        for iid, price in risk_full.X110.items():
            self.assertEqual(price, ITEMS[iid]["base_price"], iid)

    def test_boxes_match_simulation(self):
        for bid, (price, weights) in sm.BOXES.items():
            self.assertEqual(price, BOXES[bid]["price"], bid)
            self.assertEqual(weights, {d["item"]: d["weight"] for d in BOXES[bid]["drops"]}, bid)

    def test_items_origin_match_simulation(self):
        for iid, (origin, _) in sm.ITEMS.items():
            self.assertEqual(origin, ORIGIN_NAME[ITEMS[iid]["origin"]], iid)

    def test_quality_and_fake_factors_match_simulation(self):
        sim = {name: (p, mult) for name, p, mult in sm.QUALITY}
        for q in D["rules"]["quality"]:
            self.assertEqual((q["p"], q["price_mult"]), sim[q["name"]], q["id"])
        self.assertEqual(sm.FAKE_X, D["rules"]["fake_price_factor"])
        self.assertEqual(sm.FAKE_P_V09, D["item_fake_rate_floor"]["V09"])

    def test_core_rules_match_reference_model(self):
        r = D["rules"]
        self.assertEqual(em.DEBT, r["debt"])
        self.assertEqual(em.START_CASH, r["start_cash"])
        self.assertEqual(em.MAX_BOXES_PER_DAY, r["max_boxes_per_day"])
        self.assertEqual(em.PENALTY_RATE, r["penalty_rate"])
        self.assertEqual(em.LOSE_ARREARS_OVER, r["lose_arrears_over"])
        self.assertEqual(em.EQUIP_SLOTS, r["equip_slots"])

    def test_services_match_service_model(self):
        s = D["services"]
        self.assertEqual(sv.AUCTION_FEE, s["auction"]["fee_rate"])
        self.assertEqual(sv.AUCTION_MIN_G, s["auction"]["min_sell_price"])
        self.assertEqual(sv.AUCTION_K, [(k["p"], k["k"]) for k in s["auction"]["k"]])
        self.assertAlmostEqual(1.16, sum(k["p"] * k["k"] for k in s["auction"]["k"]), places=2)
        self.assertEqual(sv.CASINO_STAKES, tuple(s["casino"]["stakes"]))
        self.assertEqual(sv.CASINO_P, s["casino"]["p_win"])
        self.assertEqual(sv.CASINO_PER_DAY, s["casino"]["per_day"])
        self.assertEqual(sv.CASINO_FROM_DAY, s["casino"]["from_day"])
        self.assertEqual(sv.PAWN_RATIO, s["pawn"]["loan_ratio"])
        self.assertAlmostEqual(sv.PAWN_FEE, s["pawn"]["redeem_multiplier"] - 1)
        self.assertEqual(sv.PAWN_DAYS, s["pawn"]["days_to_redeem"])
        self.assertEqual(sv.PAWN_MAX, s["pawn"]["max_active"])
        self.assertEqual(sv.EVENT_P, D["event_schedule"]["probability"])
        self.assertEqual(sv.EVENT_FROM_DAY, D["event_schedule"]["from_day"])
        self.assertEqual(sv.EVENTS, [e["id"] for e in D["events"]])

    def test_artifact_buy_prices_match_service_model(self):
        for aid, sim_id in SIM_ART.items():
            self.assertEqual(sv.ART_BUY_PRICE[sim_id], ARTS[aid]["price"], aid)

    def test_pawn_and_auction_examples_in_services_doc(self):
        # Các ví dụ số trong DichVuVaMeta.md tính từ giá nền đã duyệt.
        v10 = ITEMS["V10"]["base_price"]
        loan = math.floor(v10 * D["services"]["pawn"]["loan_ratio"])
        redeem = math.ceil(loan * D["services"]["pawn"]["redeem_multiplier"])
        with open(os.path.join(DOCS, "DichVuVaMeta.md"), encoding="utf-8") as f:
            text = f.read()
        for number in (loan, redeem, math.ceil(v10 * D["services"]["auction"]["fee_rate"])):
            self.assertIn("{:,}".format(number).replace(",", "."), text, number)


def recompute_tag_prices():
    """Tính lại giá niêm yết theo manh mối bằng mô hình Bayes của Claude 2 (clue_sim.py)."""
    import clue_sim as C

    class Run:
        c = risk_full.content(risk_full.X110)

    lam = D["shelf"]["price_follows_clue"]["lambda"]
    step = D["shelf"]["price_follows_clue"]["round_to"]
    saved_skill, saved_fake = dict(C.SKILL), dict(C.R.FAKE_BY_BOX)
    out = {}
    try:
        C.R.FAKE_BY_BOX.clear()
        C.R.FAKE_BY_BOX.update({b: BOXES[b]["fake_rate"] for b in BOXES})
        for box, b in BOXES.items():
            C.SKILL["n_true"] = b["clue_true_count"]
            prior = C.posterior_ev(Run, box, [])
            row = []
            for oid in D["shelf"]["price_follows_clue"]["origins_order"]:
                ratio = C.posterior_ev(Run, box, [ORIGIN_NAME[oid]]) / prior
                row.append(int(round(b["price"] * (1 + lam * (ratio - 1)) / step) * step))
            out[box] = row
    finally:
        C.SKILL.clear(); C.SKILL.update(saved_skill)
        C.R.FAKE_BY_BOX.clear(); C.R.FAKE_BY_BOX.update(saved_fake)
    return out


class DebtRule(unittest.TestCase):
    """Luật nợ ở GDD mục 2: phạt 20% tính trên TOÀN BỘ nợ còn lại, nợ tồn là một con số duy nhất."""

    def test_penalty_base_is_total_remaining(self):
        self.assertEqual("total_remaining", D["rules"]["penalty_base"])

    def test_price_rules_do_not_leak_fakes_and_round_down(self):
        self.assertEqual("floor", D["rules"]["price_round"])
        self.assertEqual("as_genuine", D["rules"]["unappraised_display_price"])

    def test_gdd_cases_2_3_and_8(self):
        g = em.Game(seed=1, cash=150)
        g.end_day()
        self.assertEqual(60, g.arrears)           # ca 2: trả 150, còn 50, phạt 10
        g.cash = 100
        g.end_day()
        self.assertEqual(372, g.arrears)          # ca 3: 410 trừ 100 còn 310, phạt 62
        g = em.Game(seed=1, cash=150)
        g.end_day()
        g.cash = 5
        g.end_day()
        self.assertEqual(486, g.arrears)          # ca 8: 410 trừ 5 còn 405, phạt 81 (tính cả phần phạt cũ)


class ClueRules(unittest.TestCase):
    def test_every_origin_has_enough_distinct_clues(self):
        vocab = D["clue_vocabulary"]
        self.assertEqual(set(ORIGIN_NAME), set(vocab))
        texts = [t for ts in vocab.values() for t in ts]
        self.assertEqual(len(texts), len(set(texts)))
        for oid, ts in vocab.items():
            self.assertGreaterEqual(len(ts), 2, oid)
        with open(os.path.join(DOCS, "NoiDungMau.md"), encoding="utf-8") as f:
            doc = f.read()
        for t in texts:
            self.assertIn(t, doc, t)

    def test_clue_reliability_per_box(self):
        self.assertEqual({"H1": 2, "H2": 2, "H3": 1, "RD": 1}, {b["id"]: b["clue_true_count"] for b in D["boxes"]})
        self.assertEqual(3, D["clues"]["per_box"])
        self.assertEqual(3, D["rules"]["max_clues_per_box"])

    def test_shelf_rules(self):
        sh = D["shelf"]
        self.assertEqual(1, sh["boxes_per_type_per_turn"])
        self.assertEqual(D["rules"]["max_boxes_per_day"], sh["turns_per_day"])
        self.assertFalse(sh["reroll_within_turn"])
        self.assertEqual(15, sh["extra_clue_price"])

    def test_tag_prices_follow_the_bayes_model(self):
        self.assertEqual(recompute_tag_prices(), D["shelf"]["price_follows_clue"]["tag_price"])

    def test_tag_price_table_in_content_doc(self):
        rows = [r for r in table_rows("NoiDungMau.md", r"H[123]|RD")
                if len(r) == 7 and all(c.endswith("xu") for c in r[1:])]
        self.assertEqual(4, len(rows))
        for r in rows:
            self.assertEqual(D["shelf"]["price_follows_clue"]["tag_price"][r[0]], [xu(c) for c in r[1:]], r[0])

    def test_informative_box_is_h2_and_h1_is_flat(self):
        tag = D["shelf"]["price_follows_clue"]["tag_price"]
        self.assertEqual({100}, set(tag["H1"]))
        self.assertGreater(max(tag["H2"]) - min(tag["H2"]), max(tag["H3"]) - min(tag["H3"]))


class KnownDrift(unittest.TestCase):
    """Chỗ mô hình tham chiếu chưa theo gói đã duyệt. Test chỉ in cảnh báo, không làm đỏ bộ test."""

    def test_report_reference_model_storage_capacity(self):
        if em.INVENTORY_CAPACITY != D["rules"]["inventory_slots"]:
            print("\n[CẢNH BÁO] economy_model.INVENTORY_CAPACITY = %s, gói đã duyệt là kho %s ô và bàn mở %s hộp."
                  % (em.INVENTORY_CAPACITY, D["rules"]["inventory_slots"], D["rules"]["table_slots"]))


if __name__ == "__main__":
    unittest.main(verbosity=2)

"""Ca kiểm thử biên Sprint 5 (BT-17 trở đi), phần kiểm chứng được trên MÔ HÌNH THAM CHIẾU (Claude 2).
Bộ ca đầy đủ và khuôn mẫu: Docs/Balance/CaKiemThuSprint5.md. Mỗi test ghi mã ca (BT-xx) và trạng thái luật.
Chỉ viết test cho ca mà mô hình (chest_sim, chest_unlock, inspect_sim, balance_sprint5.json, currency) kiểm chứng được.
Ca cần game thật (giao diện, lưu và tải thật, hẹn giờ 2 phút) KHÔNG có ở đây: chỉ ghi trong tài liệu để Codex viết.
Tiền trong mô hình tính theo đơn vị cân bằng 10.000 đ (Gỗ price=140 là 1.400.000 đ). Chạy dưới 10 giây.
Chạy: cd QA && python3 -m unittest -v test_sprint5_cases
"""
import json
import math
import os
import random
import types
import unittest
from unittest import mock

import chest_sim as CS
import clue_sim as C
import currency as CUR
import inspect_sim as I
import proposal_sim as P
import sprint3_sim as sm

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
with open(os.path.join(ROOT, "Docs", "Content", "data", "balance_sprint5.json"), encoding="utf-8") as f:
    D = json.load(f)
UNIT = CUR.UNIT_VND
ALL_CHESTS = D["chests_basic"] + D["chests_unlock"]
CHEST_KEY = {"Gỗ": "H1", "Vải Niêm": "H2", "Kim Khí": "H3", "Đen": "RD"}   # mã nội bộ theo giá tăng dần


def new_run(design, seed=1, day=1):
    """Một ván mô phỏng ở đầu ngày `day` (đã chạy luật đầu buổi sáng: giá dao động, cửa hàng ngày 1)."""
    c = CS.apply(design)
    C.setup_done = True
    run = P.Run5(seed, c, P.STRATS["liều"], None, None)
    run.g.day = day
    P.start_hooks(run)
    return run


# --- (a) Ngày 1 lần chơi đầu -----------------------------------------------------------------------------------
class FirstRunDay1(unittest.TestCase):
    def tearDown(self):
        CS.restore()

    def test_BT17_first_box_is_junk(self):
        """BT-17 [ĐÃ CHỐT: mo-man; giá rác 30.000 đ là ĐỀ XUẤT]. Hộp đầu của lần chơi đầu luôn ra rác (R01..R09), không giả, giá nền 30.000 đ."""
        seen = set()
        for seed in range(150):
            run = new_run(CS.FIRST_RUN, seed)
            for box in ("H1", "H2"):
                run = new_run(CS.FIRST_RUN, seed)
                it = run.buy_open(box)
                self.assertIn(it["id"], CS.JUNK, (seed, box, it))
                self.assertFalse(it["fake"])
                self.assertEqual(it["script"], "junk")
                # giá nền rác 30.000 đ; mô hình vẫn nhân hệ số tình trạng (Nguyên 30.000, Xước 20.000, Hỏng 0 đ): xem mục điểm lệch trong tài liệu
                self.assertEqual(run.price(it), math.floor(3 * it["q"]))
                self.assertLessEqual(run.price(it) * UNIT, 30_000)
                seen.add(it["id"])
        self.assertGreater(len(seen), 3)     # rác hài khác nhau giữa các ván, không phải một món cố định
        self.assertEqual(D["junk_items"]["R01"]["base_price_vnd"], 30_000)

    def test_BT18_second_box_is_sellable_common(self):
        """BT-18 [ĐÃ CHỐT: mo-man]. Hộp hai ra đồ Thường thật: bậc T của rương đã chọn, không giả, không Hỏng, bán được (> 0 đ)."""
        for box, name in (("H1", "Gỗ"), ("H2", "Vải Niêm")):
            pool = set(D["chests_basic"][0 if name == "Gỗ" else 1]["pools"]["T"])
            for seed in range(150):
                run = new_run(CS.FIRST_RUN, seed)
                run.buy_open(box)
                it = run.buy_open(box)
                self.assertIn(it["id"], pool, (name, seed, it))
                self.assertFalse(it["fake"], (seed, it))
                self.assertNotEqual(it["ql"], "Hỏng", (seed, it))
                self.assertEqual(it["script"], "common")
                self.assertGreater(run.price(it), 0)

    def test_BT19_third_box_is_random(self):
        """BT-19 [ĐÃ CHỐT]. Chỉ hai hộp đầu sắp sẵn; hộp thứ ba của ngày 1 quay bình thường (có cả rác lẫn không rác)."""
        kinds = set()
        for seed in range(300):
            run = new_run(CS.FIRST_RUN, seed)
            run.buy_open("H1")
            run.buy_open("H1")
            it = run.buy_open("H1")
            self.assertIsNone(it["script"])
            kinds.add(it["id"] in CS.JUNK)
        self.assertEqual(kinds, {True, False})

    def test_BT20_day1_shop_only_wood_and_cloth(self):
        """BT-20 [ĐÃ CHỐT: DaChot, Mở màn]. Ngày 1 lần chơi đầu: chỉ Gỗ và Vải Niêm mua được; Kim Khí và Đen không mua được."""
        self.assertEqual(D["first_run"]["day1_shop_only"]["value"], ["GO", "VAI_NIEM"])
        run = new_run(CS.FIRST_RUN, 3)
        for k in ("H1", "H2"):
            self.assertLess(run.c.box_price[k], CS.UNSOLD)
        for k in ("H3", "RD"):
            self.assertEqual(run.c.box_price[k], CS.UNSOLD)
            self.assertIsNone(run.buy_open(k))          # không đủ tiền mua, không trừ lượt
            self.assertEqual(run.g.bought_today, 0)
            self.assertEqual(run.g.cash, 1000)

    def test_BT21_later_days_and_normal_runs_unrestricted(self):
        """BT-21 [ĐÃ CHỐT]. Từ ngày 2 của lần chơi đầu, và cả ngày 1 của ván thường, cửa hàng bán đủ; hộp ngày 1 ván thường không sắp sẵn."""
        run = new_run(CS.FIRST_RUN, 3, day=2)
        self.assertTrue(all(p < CS.UNSOLD for p in run.c.box_price.values()))
        CS.restore()
        run = new_run(CS.FINAL, 3, day=1)
        self.assertTrue(all(p < CS.UNSOLD for p in run.c.box_price.values()))
        for count in (0, 1, 2):
            self.assertIsNone(CS._scripted(run, count))

    def test_BT22_day2_fully_random(self):
        """BT-22 [ĐÃ CHỐT]. Từ ngày 2 không còn hộp sắp sẵn: hộp đầu ngày 2 ra rác theo tỉ lệ rương (Gỗ 18%), không phải 100%."""
        run = new_run(CS.FIRST_RUN, 5, day=2)
        for count in (0, 1, 2):
            self.assertIsNone(CS._scripted(run, count))
        junk = 0
        n = 4000
        for i in range(n):
            iid = CS._draw(run, "H1", 0)[0]
            junk += iid in CS.JUNK
        self.assertAlmostEqual(junk / n, D["chests_basic"][0]["tier_rates"]["R"], delta=.03)

    def test_BT23_day1_cannot_lose(self):
        """BT-23 [ĐÃ CHỐT: ngày 1 không thua; số đo ở Sprint5Ruong mục 3]. Bot liều, lần chơi đầu: không ván nào thua ngày 1 hoặc trước ngày 3;
        nợ tồn xa ngưỡng thua 10.000.000 đ."""
        e = D["economy"]
        worst = e["debt_vnd"]["value"][0] * (1 + e["penalty_rate"]["value"])   # trễ hạn: nợ 2.000.000 đ + phạt 20% = 2.400.000 đ
        self.assertLess(worst, e["lose_arrears_over_vnd"]["value"])
        c = CS.apply(CS.FIRST_RUN)
        C.setup_done = True
        for strat in ("liều", "thận trọng"):
            d = CS.day1_stats(c, P.STRATS[strat], 120)
            self.assertEqual(d["lost_day1"], 0, (strat, d))
            self.assertEqual(d["lost_before_day3"], 0, (strat, d))
            self.assertLess(d["arrears1_max"] * UNIT, e["lose_arrears_over_vnd"]["value"], (strat, d))
            self.assertGreater(d["win"], 1.5, (strat, d))   # đường thắng còn

    def test_BT24_first_run_loosening_refund(self):
        """BT-24 [nới lần chơi đầu ĐÃ CHỐT; cách cài `cheap` chỉ ĐỀ XUẤT, Codex có thể đổi]. Hai hộp đầu chỉ tốn bằng giá rương rẻ nhất:
        hoàn phần chênh; hộp thứ ba không hoàn; chế độ không nới (`any`) không hoàn."""
        self.assertEqual(D["inspect"]["first_run_loosening"]["value"], "two_first_boxes_cost_cheapest_chest")
        run = new_run(CS.FIRST_RUN_LOOSE, 2)
        it = run.buy_open("H2")                    # Vải Niêm, hộp đầu (rác)
        cheapest = min(run.c.box_price.values())
        self.assertEqual(CS.script_refund(run, it), run.c.box_price["H2"] - cheapest)
        it3 = dict(it, script=None)
        self.assertEqual(CS.script_refund(run, it3), 0)
        CS.restore()
        run = new_run(CS.FIRST_RUN, 2)
        it = run.buy_open("H2")
        self.assertEqual(CS.script_refund(run, it), 0)


# --- (b) Hàng giả ------------------------------------------------------------------------------------------------
class FakeRates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rn = new_run(CS.FINAL, 11, day=4)

    @classmethod
    def tearDownClass(cls):
        CS.restore()

    def test_BT25_v09_fake_at_least_35_percent_everywhere(self):
        """BT-25 [luật nội dung hiện hành, content.json `item_fake_rate_floor.V09`; chưa có dòng riêng trong DaChot]. Lá bùa V09 hàng giả
        >= 35% ở mọi rương có bán nó (rương nào có tỉ lệ giả thấp hơn cũng bị nâng lên 35%)."""
        floor = D["items"]["V09"]["fake_rate_floor"]
        self.assertEqual(floor, .35)
        for box in ("H1", "H2", "RD"):
            self.assertGreaterEqual(CS.fake_rate(box, "V09"), floor, box)
        self.assertEqual(CS.fake_rate("H1", "V09"), .35)                # Gỗ 10% -> 35%
        # rương có tỉ lệ giả cao hơn sàn giữ nguyên tỉ lệ rương (không bị kéo xuống)
        with mock.patch.dict(CS.STATE["fake"], {"RD": .5}):
            self.assertEqual(CS.fake_rate("RD", "V09"), .5)
        # đo thật trên các lần quay của Gỗ: V09 giả khoảng 35%
        v09 = fake = 0
        for _ in range(9000):
            iid, _q, _ql, fk, _sc = CS._draw(self.rn, "H1", 5)
            if iid == "V09":
                v09 += 1
                fake += fk
        self.assertGreater(v09, 1500)
        self.assertAlmostEqual(fake / v09, .35, delta=.04)

    def test_BT26_junk_never_fake(self):
        """BT-26 [ĐỀ XUẤT: rác không giả, Sprint5Ruong mục 1]. Rác không bao giờ là hàng giả, ở mọi rương."""
        for box in ("H1", "H2", "H3", "RD"):
            for j in CS.JUNK:
                self.assertEqual(CS.fake_rate(box, j), 0.0)
        for box in ("H1", "H2", "H3", "RD"):
            for _ in range(1500):
                iid, _q, _ql, fk, _sc = CS._draw(self.rn, box, 5)
                if iid in CS.JUNK:
                    self.assertFalse(fk)

    def test_BT27_kim_khi_never_fake(self):
        """BT-27 [ĐÃ CHỐT: Kim Khí hàng giả 0%]. Món và tạo tác từ Kim Khí không bao giờ giả (trừ Lá bùa V09, xem BT-25: V09 không nằm
        trong bộ Kim Khí nên hai luật không đụng nhau)."""
        kk = D["chests_basic"][2]
        self.assertEqual(kk["fake_rate"], 0.0)
        in_kim_khi = {i for p in kk["pools"].values() for i in p}
        self.assertNotIn("V09", in_kim_khi)
        for i in in_kim_khi:
            self.assertEqual(CS.fake_rate("H3", i), 0.0, i)
        for _ in range(4000):
            iid, _q, _ql, fk, _sc = CS._draw(self.rn, "H3", 5)
            self.assertFalse(fk, iid)
        self.assertEqual(CS.fake_rate("H3", "AX_CP1"), 0.0)

    def test_BT28_fake_rates_match_table(self):
        """BT-28 [ĐÃ CHỐT: bảng 4 rương cơ bản]. Tỉ lệ giả mỗi rương: Gỗ 10%, Vải Niêm 15%, Kim Khí 0%, Đen 30% (món không phải V09)."""
        want = {"H1": .10, "H2": .15, "H3": 0.0, "RD": .30}
        for box, rate in want.items():
            self.assertEqual(CS.fake_rate(box, "V01"), rate, box)
        for ch, box in zip(D["chests_basic"], ("H1", "H2", "H3", "RD")):
            self.assertEqual(ch["fake_rate"], want[box])


# --- (c) Quay hai bước ------------------------------------------------------------------------------------------
class TwoStepRoll(unittest.TestCase):
    def test_BT29_tier_rates_sum_to_100_percent(self):
        """BT-29 [4 rương cơ bản ĐÃ CHỐT; 4 rương mở khóa và khung chủ đề ĐỀ XUẤT, có thể đổi]. Tỉ lệ bậc mỗi rương cộng 100%
        (rương Kim Khí và rương mở khóa ra tạo tác: bậc + phần tạo tác = 100%)."""
        for ch in ALL_CHESTS + [D["chest_theme_frame"]]:
            total = sum(ch["tier_rates"].values()) + ch["artifact_share"]
            self.assertAlmostEqual(total, 1.0, places=9, msg=ch.get("name", "chủ đề"))
        kk = D["chests_basic"][2]
        self.assertAlmostEqual(kk["artifact_share"], .34, places=9)

    def test_BT30_each_item_in_one_chest_junk_shared(self):
        """BT-30 [ĐÃ CHỐT: ruong-chu-de]. Mỗi món chỉ thuộc một rương cơ bản; Rác dùng chung cho mọi rương (9 món, rương nào cũng quay được)."""
        owner = {}
        for ch in D["chests_basic"]:
            for pool in ch["pools"].values():
                for i in pool:
                    self.assertNotIn(i, owner, (i, ch["id"], owner.get(i)))
                    owner[i] = ch["id"]
        self.assertEqual(len(D["junk_items"]), 9)
        for ch in CS.FINAL_CHESTS.values():
            w = CS.weights(ch)
            self.assertTrue(all(w.get(j, 0) > 0 for j in CS.JUNK))
            self.assertEqual(len({round(w[j], 9) for j in CS.JUNK}), 1)    # 9 món rác quay đều nhau
        tong_hop = D["chests_unlock"][0]
        self.assertEqual(tong_hop["pools"], "union_of_basic_chests")     # Tổng Hợp [ĐỀ XUẤT] ra đúng đồ rương cơ bản

    def test_BT31_two_steps_tier_then_uniform_item(self):
        """BT-31 [ĐÃ CHỐT: ruong-chu-de]. Quay bậc rồi quay đều một món trong bậc: xác suất mỗi món = tỉ lệ bậc / số món của bậc."""
        for ch in D["chests_basic"]:
            w = CS.weights(CS.FINAL_CHESTS[ch["name"]])
            for t, pool in ch["pools"].items():
                for i in pool:
                    self.assertAlmostEqual(w[i] / 1000, ch["tier_rates"][t] / len(pool), places=9, msg=(ch["id"], i))
        # đo tần suất bậc trên 20.000 lần quay của Gỗ
        run = new_run(CS.FINAL, 21, day=4)
        try:
            tier_of = {i: t for t, pool in D["chests_basic"][0]["pools"].items() for i in pool}
            cnt = {"R": 0, "T": 0, "H": 0, "X": 0, "L": 0}
            n = 20000
            for _ in range(n):
                iid = CS._draw(run, "H1", 5)[0]
                cnt["R" if iid in CS.JUNK else tier_of[iid]] += 1
            for t, p in D["chests_basic"][0]["tier_rates"].items():
                self.assertAlmostEqual(cnt[t] / n, p, delta=.012, msg=t)
        finally:
            CS.restore()

    def test_BT32_same_seed_same_result_after_save_load(self):
        """BT-32 [yêu cầu nghiệm thu của Codex; cách lưu hạt giống do Codex chọn]. Cùng hạt giống (hoặc trạng thái bộ sinh số lưu giữa chừng)
        cho cùng chuỗi kết quả quay. Mô hình chỉ chứng minh tính chất này của bộ quay; lưu và tải THẬT phải test ở game (xem tài liệu)."""
        run1 = new_run(CS.FINAL, 77, day=4)
        run2 = new_run(CS.FINAL, 77, day=4)
        seq1 = [CS._draw(run1, "RD", 5)[:4] for _ in range(40)]
        seq2 = [CS._draw(run2, "RD", 5)[:4] for _ in range(40)]
        self.assertEqual(seq1, seq2)
        # lưu giữa chừng rồi tải: tiếp tục từ trạng thái đã lưu cho cùng kết quả như chạy liền
        run3 = new_run(CS.FINAL, 77, day=4)
        part = [CS._draw(run3, "RD", 5)[:4] for _ in range(15)]
        saved = run3.rng.getstate()
        rest_direct = [CS._draw(run3, "RD", 5)[:4] for _ in range(25)]
        run3.rng.setstate(saved)
        rest_loaded = [CS._draw(run3, "RD", 5)[:4] for _ in range(25)]
        self.assertEqual(rest_direct, rest_loaded)
        self.assertEqual(part + rest_loaded, seq1)
        CS.restore()


# --- (d) Tạo tác --------------------------------------------------------------------------------------------------
class Artifacts(unittest.TestCase):
    def tearDown(self):
        CS.restore()

    def make(self):
        run = new_run(CS.FINAL, 4, day=2)
        run.owned, run.equipped, run.bk1 = [], [], False
        return run

    def test_BT33_duplicate_artifact_pays_half_kho_price(self):
        """BT-33 [ĐÃ CHỐT: DaChot, 4 rương cơ bản]. Tạo tác Kim Khí trùng không trang bị lần hai mà đổi 50% giá Kho Tạo Tác.
        Ví dụ: CP1 giá Kho 1.500.000 đ đổi 750.000 đ; CP2 4.000.000 đ đổi 2.000.000 đ."""
        self.assertEqual(D["economy"]["duplicate_artifact_refund_of_kho_price"]["value"], .5)
        for aid, kho_vnd, refund_vnd in (("CP1", 1_500_000, 750_000), ("TC1", 1_200_000, 600_000), ("CP2", 4_000_000, 2_000_000),
                                          ("TC2", 4_500_000, 2_250_000), ("CP3", 5_000_000, 2_500_000)):
            run = self.make()
            run.sell({"id": "AX_" + aid})                       # lần đầu: vào ô trống, không đổi tiền
            self.assertEqual(run.equipped, [aid])
            cash = run.g.cash
            run.sell({"id": "AX_" + aid})                       # trùng: đổi tiền
            self.assertEqual(run.equipped, [aid], "không trang bị trùng")
            self.assertEqual((run.g.cash - cash) * UNIT, refund_vnd, aid)
            self.assertEqual(sm.ART[aid][1] * UNIT, kho_vnd)

    def test_BT34_four_slots_default_fifth_after_bk1(self):
        """BT-34 [ĐÃ CHỐT: 4 ô mặc định, BK1 mở ô thứ 5]. Có 4 tạo tác: ô thứ 5 chưa mở nên tạo tác thứ 5 đổi tiền thay vì trang bị.
        Sau khi mở BK1 thì trang bị được ô thứ 5."""
        self.assertEqual((D["economy"]["equip_slots"]["value"], D["economy"]["equip_slots"]["with_bk1"]), (4, 5))
        run = self.make()
        self.assertEqual(run.slots(), 4)
        run.owned, run.equipped = ["CP1", "TC1", "CP2", "TC2"], ["CP1", "TC1", "CP2", "TC2"]
        cash = run.g.cash
        run.sell({"id": "AX_CP3"})
        self.assertEqual(len(run.equipped), 4)
        self.assertNotIn("CP3", run.equipped)
        self.assertEqual(run.g.cash - cash, round(.5 * sm.ART["CP3"][1]))         # đổi 50% giá Kho
        run.bk1 = True
        self.assertEqual(run.slots(), 5)
        with mock.patch.object(type(run), "refresh", lambda self: None):         # tách riêng luật ô, không tính lại BK1
            run.sell({"id": "AX_TC3"})
        self.assertEqual(len(run.equipped), 5)

    def test_BT35_artifact_only_from_kim_khi(self):
        """BT-35 [ĐÃ CHỐT]. Chỉ Kim Khí (cơ bản) ra tạo tác: 34% lượt mở; bậc 1/2/3 = 60%/35%/5%; 3 rương còn lại không ra tạo tác."""
        run = new_run(CS.FINAL, 8, day=4)
        n = 12000
        art = tiers = 0
        by_tier = {1: 0, 2: 0, 3: 0}
        for _ in range(n):
            iid = CS._draw(run, "H3", 5)[0]
            if iid.startswith("AX_"):
                art += 1
                by_tier[CS.ART_TIER[iid[3:]]] += 1
        self.assertAlmostEqual(art / n, .34, delta=.015)
        for t, p in zip((1, 2, 3), (.60, .35, .05)):
            self.assertAlmostEqual(by_tier[t] / art, p, delta=.03, msg=t)
        for box in ("H1", "H2", "RD"):
            self.assertTrue(all(not CS._draw(run, box, 5)[0].startswith("AX_") for _ in range(2000)), box)


# --- (e) Soi lỗi và trả giá -------------------------------------------------------------------------------------
PERFECT = dict(s=1.0, c=1.0, rule="real", hint=False, skip_haggle=False)   # tìm đủ lỗi, khai thật
BLIND = dict(s=0.0, c=0.0, rule="real", hint=False, skip_haggle=True)      # không tìm ra lỗi nào (hết giờ, hoặc thấy mà không bấm)


class InspectCases(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        I.PROFILES["_chắc"], I.PROFILES["_mù"] = PERFECT, BLIND

    @classmethod
    def tearDownClass(cls):
        I.PROFILES.pop("_chắc", None)
        I.PROFILES.pop("_mù", None)

    def tearDown(self):
        I.restore()

    def start(self, profile, day=2, rules=None):
        c = I.apply(CS.FINAL, rules, profile)
        C.setup_done = True
        run = P.Run5(5, c, P.STRATS["liều"], None, None)
        run.g.day = day
        P.start_hooks(run)
        run.g.cash = 100_000                                # đủ rộng để phạt không thành nợ tồn
        return run

    @staticmethod
    def item(iid, q=1.0, fake=False, box="H1"):
        return dict(id=iid, q=q, ql={1.0: "Nguyên", .7: "Xước", .3: "Hỏng"}[q], fake=fake, appraised=False, box=box, clues=["Quá khứ"] * 3, seen=1)

    def sell(self, run, it, r=10 ** 9):
        """Bán một món qua soi lỗi và trả giá. r lớn: không mua gợi ý, không phục chế, không vá lỗi (tách riêng từng luật)."""
        cash = run.g.cash
        V = run.known_price(it)
        I.handle(run, it, r, False)
        return run.g.cash - cash, V

    def test_BT36_rules_match_data_table(self):
        """BT-36 [số ĐÃ CHỐT và ĐỀ XUẤT lẫn lộn: đối chiếu từng số, xem cột trạng thái trong tài liệu]. Mô hình khớp bảng số liệu JSON."""
        R, J = I.RULES, D["inspect"]
        self.assertEqual(J["time_limit_seconds"]["value"], 120)
        self.assertEqual(R["cap"], J["haggle"]["cap_fraction"])
        self.assertEqual(R["floor"], J["haggle"]["floor_fraction"])
        self.assertEqual((R["rounds"], R["up"], R["down"], R["open"]), (J["haggle"]["rounds"], J["haggle"]["win_step"], J["haggle"]["lose_step"], 1.0))
        self.assertEqual(R["fake_x"], J["fake_sell_fraction"]["value"])
        self.assertEqual(R["fine"], J["fake_declared_genuine_fine"]["value"])
        self.assertEqual(R["day1_fine"], J["day1_fine"]["value"])
        self.assertEqual(R["miss_pen"], J["missed_defect_penalty_of_appraised_price"]["value"])
        self.assertEqual(R["odd"], J["odd_mark_rate_on_genuine"]["value"])
        self.assertEqual((R["repair_light"], R["repair_big"]), (J["repair_shop"]["light_fee_of_base_price"], J["repair_shop"]["big_fee_of_base_price"]))
        self.assertEqual(R["hint_rate"], J["professor_hint"]["rate_of_base_price"])
        self.assertEqual(J["professor_hint"]["min_fee_vnd"], 20 * UNIT)
        self.assertEqual(R["marks"], J["fake_marks_per_fake_item"]["value"])
        self.assertEqual(R["n_light"], J["light_defects_per_scratched_item"]["value"])

    def test_BT37_evidence_linked_to_haggle(self):
        """BT-37 [ĐÃ CHỐT: giữ nối bằng chứng với trả giá]. Xác suất thắng mỗi vòng = kỹ năng x tỉ lệ lỗi đã tìm: không tìm lỗi nào thì
        không trả giá được (giữ giá mở); tìm càng nhiều lỗi càng dễ thắng."""
        pf = I.PROFILES["giỏi"]
        self.assertEqual(I.haggle_win(pf, 0, 3), 0.0)
        self.assertLess(I.haggle_win(pf, 1, 3), I.haggle_win(pf, 2, 3))
        self.assertLess(I.haggle_win(pf, 2, 3), I.haggle_win(pf, 3, 3))
        self.assertEqual(I.haggle_win(pf, 0, 0), pf["c"])                  # món không lỗi: đủ bằng chứng
        self.assertEqual(I.haggle_ev(0.0, I.RULES), I.RULES["open"])        # không có bằng chứng: giữ giá mở, không trả giá

    def test_BT38_haggle_cap_and_floor(self):
        """BT-38 [trần 108% và sàn 70%: ĐỀ XUẤT, có thể đổi (dự phòng 105%)]. Thắng cả 3 vòng vẫn không vượt trần 108% giá đã trừ lỗi;
        thua vòng nào cũng không rơi dưới sàn 70%; mỗi vòng thắng +8%, thua -5%."""
        R = I.RULES
        run = types.SimpleNamespace(rng=types.SimpleNamespace(random=lambda: 0.0))       # thắng mọi vòng
        m, rounds, ok, bad = I.haggle(run, PERFECT, R, "X", 3, 3)
        self.assertEqual((m, rounds, ok, bad), (R["cap"], 3, 3, 0))
        self.assertEqual(R["open"] + 3 * R["up"], 1.24)                                   # không có trần thì sẽ là 124%
        run = types.SimpleNamespace(rng=types.SimpleNamespace(random=lambda: 0.99))      # thua mọi vòng
        m, rounds, ok, bad = I.haggle(run, dict(PERFECT, c=.5), R, "X", 3, 3)
        self.assertAlmostEqual(m, 1.0 - 3 * R["down"], places=9)
        self.assertGreaterEqual(m, R["floor"])
        self.assertEqual((ok, bad), (0, 3))
        one = types.SimpleNamespace(rng=types.SimpleNamespace(random=lambda: 0.0))
        self.assertEqual(I.haggle(one, PERFECT, dict(R, rounds=1), "X", 3, 3)[0], 1.0 + R["up"])

    def test_BT39_common_tier_sells_fast_no_haggle(self):
        """BT-39 [ĐỀ XUẤT: trả giá từ bậc Hiếm trở lên]. Món Thường bán nhanh: giá đúng giá thẩm định, không thưởng trả giá."""
        run = self.start("_chắc")
        got, V = self.sell(run, self.item("V01"))
        self.assertEqual(got, V)

    def test_BT40_real_called_fake_loses_80_percent(self):
        """BT-40 [ĐỀ XUẤT: khai giả nhận 20% giá]. Món THẬT mà người chơi khai GIẢ: chỉ nhận 20% giá thẩm định (lỗ 80%)."""
        run = self.start("_chắc")
        with mock.patch.object(I, "decide_fake", return_value=True):
            got, V = self.sell(run, self.item("N03"))
        self.assertEqual(got, math.floor(V * .2))
        self.assertEqual((V * UNIT, got * UNIT), (27_000_000, 5_400_000))   # Tượng Di Lặc 27.000.000 đ, chỉ nhận 5.400.000 đ

    def test_BT41_fake_called_real_confiscated(self):
        """BT-41 [ĐỀ XUẤT: phạt 20%, nhận 0 đ; ngày 1 miễn phạt]. Món GIẢ mà khai THẬT: bị tịch thu (giá hàng giả trừ phạt = 0 đ).
        Ngày 1 miễn phạt: nhận giá hàng giả 20%."""
        for iid in ("N03", "V09"):
            run = self.start("_chắc", day=2)
            got, V = self.sell(run, self.item(iid, fake=True))
            self.assertEqual(got, 0, (iid, "ngày 2", V))
        for iid in ("N03", "V09"):
            run = self.start("_chắc", day=1)
            got, V = self.sell(run, self.item(iid, fake=True))
            self.assertEqual(got, math.floor(V * .2), (iid, "ngày 1"))
            self.assertGreater(got, 0)

    def test_BT42_fake_called_fake_gets_fake_price(self):
        """BT-42 [ĐỀ XUẤT]. Món giả khai đúng là giả: nhận 20% giá, không phạt (ngày nào cũng vậy)."""
        for day in (1, 2):
            run = self.start("_chắc", day=day)
            with mock.patch.object(I, "decide_fake", return_value=True):
                got, V = self.sell(run, self.item("N03", fake=True))
            self.assertEqual(got, math.floor(V * .2))

    def test_BT43_missed_defects_penalty_and_timeout(self):
        """BT-43 [phạt 10% mỗi lỗi sót: ĐỀ XUẤT; 'thấy mà không bấm' tính là sót: ĐÃ CHỐT; 'hết giờ' tính là sót: GIẢ ĐỊNH của mô hình, DaChot chỉ chốt giới hạn 2 phút]. Món Xước Hiếm có 2 lỗi nhẹ,
        người chơi tìm ra 0 lỗi (hết giờ, hoặc thấy mà bỏ bấm): trừ phạt 10% giá thẩm định mỗi lỗi sót, không được trả giá.
        Mô hình không phân biệt 'chưa thấy' với 'thấy mà không bấm': cả hai là lỗi sót."""
        run = self.start("_mù")
        it = self.item("N01", q=.7)
        got, V = self.sell(run, it)
        self.assertEqual(got, V - 2 * math.ceil(V * .1))
        self.assertLess(got, V)

    def test_BT44_all_defects_found_no_penalty_and_haggle_up(self):
        """BT-44 [ĐỀ XUẤT]. Tìm đủ lỗi thì không bị phạt sót, trả giá thắng hết: nhận 108% giá thẩm định (đã trừ lỗi)."""
        run = self.start("_chắc")
        got, V = self.sell(run, self.item("N03", q=.7))
        self.assertEqual(V, 1889)                          # 27.000.000 đ x 0,7, làm tròn xuống: 18.890.000 đ
        self.assertEqual(got, math.floor(V * I.RULES["cap"]))
        self.assertEqual(got * UNIT, 20_400_000)           # x 1,08, làm tròn xuống bội 10.000 đ

    def test_BT45_repair_shop_only_found_defects(self):
        """BT-45 [có nhà vá lỗi: ĐÃ CHỐT; phí 12% và 30%: ĐỀ XUẤT]. Nhà vá chỉ sửa lỗi ĐÃ tìm ra và chỉ khi có lời; lỗi sót không sửa được.
        Món Hỏng Rất hiếm 27.000.000 đ: vết lớn (trừ 40% = 10.800.000 đ) đáng vá với phí 30% giá nền (8.100.000 đ); 3 lỗi nhẹ (mỗi lỗi 10% = 2.700.000 đ)
        không đáng vá với phí 12% (3.240.000 đ)."""
        run = self.start("_chắc")
        it = self.item("N03", q=.3)
        with mock.patch.object(run, "restore_fee", lambda base: 10 ** 9):   # tắt phục chế của quầy để thấy riêng nhà vá
            I.handle(run, it, 0, False)
        st = I.stats(run)
        self.assertEqual(st["repairs"], 1)
        self.assertEqual(st["repair_spend"], round(2700 * .30))            # 810 đơn vị
        self.assertEqual(st["repair_spend"] * UNIT, 8_100_000)
        self.assertAlmostEqual(it["q"], .7)                                # chỉ vá vết lớn: Hỏng lên Xước
        # không tìm ra lỗi nào thì không vá được dù có tiền
        run = self.start("_mù")
        it = self.item("N03", q=.3)
        with mock.patch.object(run, "restore_fee", lambda base: 10 ** 9):
            I.handle(run, it, 0, False)
        self.assertEqual(I.stats(run)["repairs"], 0)
        self.assertEqual(it["q"], .3)

    def test_BT46_skip_click_is_priced_in(self):
        """BT-46 [ĐÃ CHỐT: cho phép bỏ bấm lỗi đã thấy; trừ tiền và cộng phạt nếu người thu mua soi thấy]. Trong mô hình lỗi thấy mà
        không bấm là lỗi sót: giá đã trừ lỗi (tình trạng Xước x0,7) và còn phạt 10% mỗi lỗi, nên bỏ bấm không bao giờ có lợi hơn bấm."""
        blind, V1 = self.sell(self.start("_mù"), self.item("N01", q=.7))
        clicked, V2 = self.sell(self.start("_chắc"), self.item("N01", q=.7))
        self.assertEqual(V1, V2)
        self.assertGreater(clicked, blind)

    def test_BT47_hint_fee_minimum(self):
        """BT-47 [ĐỀ XUẤT]. Gợi ý của Giáo Sư Lẫm: phí 5% giá nền, tối thiểu 200.000 đ (đơn vị cân bằng 20)."""
        J = D["inspect"]["professor_hint"]
        self.assertEqual(max(20, round(230 * J["rate_of_base_price"])), 20)               # Bình sứ 2.300.000 đ: 5% = 115.000 đ -> tối thiểu 200.000 đ
        self.assertEqual(max(20, round(2700 * J["rate_of_base_price"])) * UNIT, 1_350_000)  # Tượng Di Lặc 27.000.000 đ: 1.350.000 đ (135 đơn vị)


# --- (f) Cây trong ván ------------------------------------------------------------------------------------------
class RunTree(unittest.TestCase):
    T = D["tree"]
    NODES = {n["id"]: n for n in T["nodes"]}

    def test_BT48_tree_is_per_run_in_run_only(self):
        """BT-48 [ĐÃ CHỐT: cây chỉ trong ván, trống đầu mỗi ván, điểm và nút làm lại mỗi ván]. Dữ liệu không có điều kiện dựa vào ván trước;
        điểm đầu ván bằng 0 và nút rẻ nhất tốn 3 điểm nên ngày 1 không thể mở nút nào."""
        self.assertEqual(self.T["scope"]["value"], "in_run_only")
        for n in self.T["nodes"]:
            self.assertNotIn(n["condition"][0], ("wins", "runs", "bk", "prev_run"), n["id"])
        self.assertEqual(self.T["points_per_day"]["value"], 3)
        self.assertGreater(min(n["cost_points"] for n in self.T["nodes"]), 0)

    def test_BT49_parent_before_child(self):
        """BT-49 [ĐÃ CHỐT: nút cha trước nút con; số điểm từng nút ĐỀ XUẤT]. Mỗi nút (trừ gốc) có cha cùng nhánh, không vòng,
        nút con không rẻ hơn nút cha; mỗi nhánh có đúng một gốc."""
        for n in self.T["nodes"]:
            if n["parent"] is None:
                continue
            p = self.NODES[n["parent"]]
            self.assertEqual(p["branch"], n["branch"], n["id"])
            self.assertGreaterEqual(n["cost_points"], p["cost_points"], n["id"])
            seen, cur = set(), n
            while cur["parent"]:
                self.assertNotIn(cur["id"], seen)
                seen.add(cur["id"])
                cur = self.NODES[cur["parent"]]
        for b in ("Rương", "Sưu tập", "Thần lực"):
            self.assertEqual(sum(1 for n in self.T["nodes"] if n["branch"] == b and n["parent"] is None), 1, b)

    def test_BT50_empty_life_branch_and_removed_nodes(self):
        """BT-50 [ĐÃ CHỐT: DaChot 2026-11-25]. Nhánh Đời sống để trống (không có nút); M3 và nhánh Câu chuyện C1..C4 đã bỏ; cây 15 nút, 79 điểm."""
        self.assertEqual(self.T["empty_branches"], ["Đời sống"])
        self.assertEqual([n for n in self.T["nodes"] if n["branch"] in ("Đời sống", "Câu chuyện")], [])
        for gone in ("M3", "C1", "C2", "C3", "C4"):
            self.assertNotIn(gone, self.NODES)
        self.assertEqual(len(self.T["nodes"]), 15)
        self.assertEqual(sum(n["cost_points"] for n in self.T["nodes"]), 79)

    def test_BT51_no_buff_and_reward_cap(self):
        """BT-51 [ĐÃ CHỐT: không buff chơi; thưởng có tác động tiền hoặc rương không quá khoảng 1.000.000 đ]. Nút chỉ được mở rương, cho chọn
        gia truyền (M1) hoặc không có tác động; không nút nào cộng tiền, giá, tỉ lệ hay combo."""
        allowed = {"chest", "heirloom_choice"}
        for n in self.T["nodes"]:
            eff = n["effect"] or {}
            self.assertLessEqual(set(eff), allowed, n["id"])
        self.assertEqual(self.T["reward_cash_cap_vnd"]["value"], 1_000_000)
        self.assertEqual(self.NODES["M2"]["effect"], None)                  # M2 bỏ phần cộng vốn

    def test_BT52_chests_unlock_only_in_run_through_tree(self):
        """BT-52 [ĐÃ CHỐT: rương mở khóa và chủ đề chỉ mở trong ván]. Bốn rương mở khóa chỉ do đúng một nút của cây mở; mọi nút Rương
        nằm dưới gốc R1; không nút nào ngoài nhánh Rương mở rương."""
        opens = {n["effect"]["chest"]: n["id"] for n in self.T["nodes"] if n["effect"] and "chest" in n["effect"]}
        self.assertEqual(set(opens), {"Rác Quý", "Tổng Hợp", "Cổ Phong", "Thiên Cơ"})
        self.assertEqual({c["name"] for c in D["chests_unlock"]}, set(opens))
        for chest, nid in opens.items():
            self.assertEqual(self.NODES[nid]["branch"], "Rương")
        self.assertEqual(self.NODES["R4"]["parent"], "R3")                  # Cổ Phong và Thiên Cơ phải qua Tổng Hợp
        self.assertEqual(self.NODES["R5"]["parent"], "R3")

    def test_BT53_tree_is_hard_not_fully_openable(self):
        """BT-53 [ĐÃ CHỐT: cây khó, 3 đến 4 nút mỗi ván; số điểm ĐỀ XUẤT]. Một ván 7 ngày kiếm tối đa 21 điểm (3 mỗi ngày) nên không mở hết
        cây 79 điểm; số nút tối đa mở được bằng 21 điểm, tôn trọng nút cha, là 6."""
        cost = {i: n["cost_points"] for i, n in self.NODES.items()}
        par = {i: n["parent"] for i, n in self.NODES.items()}
        best = 0
        for mask in range(1 << len(cost)):
            ids = [i for k, i in enumerate(cost) if mask >> k & 1]
            s = set(ids)
            if sum(cost[i] for i in s) <= 21 and all(par[i] is None or par[i] in s for i in s):
                best = max(best, len(s))
        self.assertEqual(best, 6)
        self.assertLess(21, sum(cost.values()))

    def test_BT54_shop_layout_and_unlock_day(self):
        """BT-54 [ĐÃ CHỐT: shop 4 rương cơ bản + tối đa 4 rương mở khóa hoặc chủ đề, xoay vòng theo hạt giống ván và ngày; rương mở
        chỉ có từ ngày được mở]. Rương mở khóa chưa tới ngày thì không có trong shop; mỗi ngày tối đa 4 rương mở khóa/chủ đề; 4 rương cơ bản
        luôn có (Đen từ ngày 4); cùng hạt giống và ngày cho cùng danh sách; khác ngày thì xoay."""
        import chest_unlock as U
        c = U.apply(U.scenario("tat_ca", shop_extra=4))                    # 4 rương mở khóa + 2 chủ đề = 6 rương phụ
        C.setup_done = True
        try:
            run = P.Run5(3, c, P.STRATS["liều"], None, None)
            extras = [k for k in U.BOXES if k.startswith("U")]
            self.assertEqual(len(extras), 6)
            lists = {}
            for d in range(1, 8):
                run.g.day = d
                shop = U.available(run)
                lists[d] = shop
                self.assertLessEqual(len([k for k in shop if k.startswith("U")]), 4, d)
                for k in ("H1", "H2", "H3"):
                    self.assertIn(k, shop, d)
                self.assertEqual("RD" in shop, d >= 4, d)
                self.assertEqual(shop, U.available(run))                    # cùng hạt giống và ngày: cùng danh sách
            self.assertGreater(len({tuple(sorted(v)) for v in lists.values()}), 2)   # xoay vòng giữa các ngày
            U.FROM.update({k: 3 for k in extras})                           # mọi rương phụ chỉ có từ ngày 3
            for d in (1, 2):
                run.g.day = d
                self.assertFalse([k for k in U.available(run) if k.startswith("U")], d)
            run.g.day = 3
            self.assertTrue([k for k in U.available(run) if k.startswith("U")])
        finally:
            U.restore()


# --- (g)(h) Tiền tệ, làm tròn, lưu và tải -------------------------------------------------------------------------
def walk(o, key=""):
    if isinstance(o, dict):
        for k, v in o.items():
            yield from walk(v, k)
    elif isinstance(o, list):
        for v in o:
            yield from walk(v, key)
    else:
        yield key, o


class Money(unittest.TestCase):
    def test_BT55_all_prices_fees_multiples_of_10000(self):
        """BT-55 [ĐÃ CHỐT: DaChot, Tiền tệ]. Mọi giá, phí, nợ, tiền thưởng trong bảng số liệu là bội số của 10.000 đ."""
        n = 0
        for k, v in walk(D):
            if k.endswith("_vnd") and isinstance(v, (int, float)):
                n += 1
                self.assertEqual(v % UNIT, 0, (k, v))
            elif k.endswith("_vnd"):
                self.fail((k, v))
        self.assertGreater(n, 40)

    def test_BT56_simulated_money_is_whole_units(self):
        """BT-56 [ĐÃ CHỐT]. Số tiền mọi bước bán trong mô hình là số nguyên đơn vị 10.000 đ: không có số lẻ nhỏ hơn 10.000 đ
        (giá thẩm định, bán còn 20%, trần 108%, phạt đều làm tròn xuống hoặc lên về đơn vị)."""
        I.PROFILES["_chắc"], I.PROFILES["_mù"] = PERFECT, BLIND
        try:
            for prof in ("_chắc", "_mù"):
                for day in (1, 2):
                    for iid, q, fake in (("N03", 1.0, False), ("N03", .7, True), ("V09", .7, False), ("N15", .3, False), ("N13", .7, True)):
                        c = I.apply(CS.FINAL, None, prof)
                        C.setup_done = True
                        run = P.Run5(2, c, P.STRATS["liều"], None, None)
                        run.g.day = day
                        P.start_hooks(run)
                        run.g.cash = 1_000_000
                        before = run.g.cash
                        it = dict(id=iid, q=q, ql="x", fake=fake, appraised=False, box="H2", clues=["Quá khứ"] * 3, seen=1)
                        I.handle(run, it, 10 ** 9, False)
                        self.assertIsInstance(run.g.cash, int, (prof, day, iid))
                        self.assertEqual((run.g.cash - before) * UNIT % UNIT, 0)
                        I.restore()
        finally:
            I.restore()
            I.PROFILES.pop("_chắc", None)
            I.PROFILES.pop("_mù", None)

    def test_BT58_box_price_swing_stays_on_10000_steps(self):
        """BT-58 [ĐÃ CHỐT: giá hộp dao động ±20%, bước 10.000 đ]. Giá hộp mỗi sáng là số nguyên đơn vị 10.000 đ (bội số 10.000 đ) và nằm trong
        80% đến 120% giá gốc, ở mọi ngày và mọi hạt giống."""
        import swing_sim as S
        try:
            for seed in range(40):
                for day in range(1, 8):
                    run = new_run(CS.FINAL, seed, day=day)
                    for k, p in run.c.box_price.items():
                        base = S.SW["base"][k]
                        self.assertIsInstance(p, int, (seed, day, k))
                        self.assertTrue(.8 * base - 1 <= p <= 1.2 * base + 1, (seed, day, k, p, base))
        finally:
            CS.restore()

    def test_BT57_vnd_conversion_roundtrip_and_odd_amounts(self):
        """BT-57 [làm tròn đến 10.000 đ ĐÃ CHỐT; hành vi khi TẢI số lẻ ĐANG MỞ]. Đổi qua lại đơn vị cân bằng và đồng không mất tiền với bội số
        10.000 đ. Số lẻ (vd 12.345 đ) mô hình tham chiếu từ chối (ValueError); game phải CHỌN: từ chối tải hoặc làm tròn đến 10.000 đ gần nhất
        (spec tien-te: 'làm tròn đến 10.000 đ gần nhất') và ghi vào DaChot."""
        for vnd in (0, 10_000, 1_400_000, 10_000_000, 22_000_000):
            self.assertEqual(CUR.to_vnd(CUR.from_vnd(vnd)), vnd)
        with self.assertRaises(ValueError):
            CUR.from_vnd(12_345)
        self.assertEqual(CUR.fmt_vnd(140), "1.400.000 đ")
        self.assertEqual(CUR.fmt_vnd(2200), "22.000.000 đ")
        self.assertNotIn("xu", CUR.fmt_vnd(140))


if __name__ == "__main__":
    unittest.main(verbosity=2)

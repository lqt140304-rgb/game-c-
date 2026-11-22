"""Sprint 4: ca kiểm thử luật dịch vụ của Claude 1 (DichVuVaMeta.md) và chống giao dịch lặp.

Mỗi ca "Ca N" của Claude 1 được ánh xạ thành một test có cùng số. Thêm các ca biên và fuzz.
"""
import math
import random
import unittest
from economy_model import Rejected
from services_model import *


def G(cash=5000, seed=3, day=1):
    g = ServiceGame(seed=seed); g.cash = cash; g.day = day
    return g


def item(g, value=2860, fake=False, origin="x", rare=False):
    i = g._id()
    g.items[i] = {"value": value, "fake": fake, "origin": origin, "appraised": False, "rare": rare}
    return i


def seed_for_event(day, ev):
    return next(s for s in range(5000) if G(seed=s, day=day).event() == ev)


def seed_for_k(k, floor_ok=True):
    # tìm seed cho k mong muốn với hiện vật mã 1
    for s in range(5000):
        g = G(seed=s, day=3); i = item(g)
        l = g.auction(i)
        if g.listings[l]["k"] == k:
            return s
    raise AssertionError


class Events(unittest.TestCase):
    def test_ev1_day1_never_has_event(self):
        for s in range(300):
            self.assertIsNone(G(seed=s, day=1).event())

    def test_ev2_e4_blocks_appraisal_next_day_ok(self):
        s = seed_for_event(3, "E4")
        g = G(seed=s, day=3); i = item(g)
        with self.assertRaises(Rejected):
            g.appraise(i)
        g.day = next(d for d in range(4, 200) if g.event(d) != "E4")  # giả lập ngày khác không E4
        self.assertTrue(g.appraise(i) in (True, False))

    def test_ev3_e1_adds_15_percent_to_past_items(self):
        s = seed_for_event(3, "E1")
        g = G(0, seed=s, day=3); i = item(g, 1000, origin="Qua khu")
        self.assertEqual(g.sell_item(i), 1150)
        j = item(g, 1000, origin="Hien dai")
        self.assertEqual(g.sell_item(j), 1000)

    def test_ev3b_e2_and_e6(self):
        s = seed_for_event(3, "E2")
        g = G(0, seed=s, day=3); self.assertEqual(g.sell_item(item(g, 1000, origin="Tuong lai")), 850)
        s = seed_for_event(3, "E6")
        g = G(0, seed=s, day=3); self.assertEqual(g.sell_item(item(g, 1000, rare=True)), 1100)

    def test_ev4_e3_h2_price_and_reload(self):
        s = seed_for_event(3, "E3")
        g = G(1000, seed=s, day=3)
        h = ServiceGame.load(g.save())
        self.assertEqual(h.event(), "E3")
        h.buy_box("vua")
        self.assertEqual(h.cash, 1000 - 280)

    def test_ev5_event_independent_of_actions(self):
        a, b = G(seed=11, day=4), G(seed=11, day=4)
        b.bet(100)
        self.assertEqual(a.event(), b.event())

    def test_ev6_event_frequency_near_half(self):
        n = sum(1 for s in range(4000) if G(seed=s, day=3).event())
        self.assertTrue(1800 < n < 2200)


class Pawn(unittest.TestCase):
    def test_pw1_pawn_v10_gets_1716(self):
        g = G(0); i = item(g, 2860)
        g.pawn(i)
        self.assertEqual(g.cash, 1716)

    def test_pw2_redeem_costs_1974(self):
        g = G(0, day=3); i = item(g, 2860)
        t = g.pawn(i); g.cash += 400
        g.day = 4
        g.redeem(t)
        self.assertEqual(g.cash, 1716 + 400 - 1974)
        self.assertIn(i, g.items)

    def test_pw3_lost_after_day5_deadline(self):
        g = G(100000, day=3); i = item(g, 2860)
        t = g.pawn(i)
        g.end_day(); g.end_day()           # tới ngày 5: vẫn chuộc được
        self.assertEqual(g.tickets[t]["state"], "open")
        g.end_day()                        # sang ngày 6
        self.assertEqual(g.tickets[t]["state"], "forfeited")
        with self.assertRaises(Rejected):
            g.redeem(t)

    def test_pw4_third_pawn_rejected(self):
        g = G(0)
        for _ in range(2):
            g.pawn(item(g))
        k = item(g)
        with self.assertRaises(Rejected):
            g.pawn(k)
        self.assertIn(k, g.items)

    def test_pw5_pawn_pays_debt_same_day(self):
        g = G(0, day=5); i = item(g, 1845)
        g.pawn(i)                          # 1.107
        g.end_day()
        self.assertEqual((g.cash, g.arrears), (57, 0))    # nợ 1.050 trả đủ từ tiền cầm, còn 57

    def test_pw6_reload_keeps_ticket_no_second_loan(self):
        g = G(0); i = item(g, 2860); g.pawn(i)
        h = ServiceGame.load(g.save())
        with self.assertRaises(Rejected):
            h.pawn(i)
        self.assertEqual(h.cash, 1716)

    def test_pw7_double_redeem_rejected(self):
        g = G(0); i = item(g, 2860); t = g.pawn(i); g.cash += 500
        g.redeem(t)
        c = g.cash
        with self.assertRaises(Rejected):
            g.redeem(t)
        self.assertEqual(g.cash, c)

    def test_pw8_redeem_without_cash_changes_nothing(self):
        g = G(0); i = item(g, 2860); t = g.pawn(i); g.cash = 1000
        snap = g.snapshot()
        with self.assertRaises(Rejected):
            g.redeem(t)
        self.assertEqual(snap, g.snapshot())

    def test_pw9_end_of_game_forfeits_open_tickets(self):
        g = G(0, day=7); i = item(g, 2860); t = g.pawn(i)
        g.end_day()               # nợ 2.200, có 1.560 từ tiền cầm
        self.assertEqual((g.tickets[t]["state"], g.status), ("forfeited", "lost"))

    def test_pw10_pawned_item_locked(self):
        g = G(0); i = item(g); g.pawn(i)
        for fn in (g.sell_item, g.pawn, g.auction, g.appraise):
            with self.assertRaises(Rejected):
                fn(i)


class PawnArtifact(unittest.TestCase):
    def _g(self, *ids):
        g = G(0)
        for a in ids:
            g.grant_artifact(a); g.equip(a)
        return g

    def test_pa1_cp3_gives_300_and_unequips(self):
        g = self._g("A3")                       # CP3 giá mua 500
        g.pawn_art("A3")
        self.assertEqual((g.cash, g.equipped, g.owned), (300, [], []))

    def test_pa2_pawn_breaks_combo(self):
        g = self._g("A1", "A2")
        self.assertTrue(g.active_combos())
        g.pawn_art("A2")
        self.assertEqual(g.active_combos(), [])

    def test_pa3_redeem_returns_to_owned_must_reequip(self):
        g = self._g("A3"); t = g.pawn_art("A3"); g.cash += 100
        g.redeem(t)                             # 345 = ceil(300*1,15)
        self.assertEqual((g.cash, g.owned, g.equipped), (55, ["A3"], []))
        g.equip("A3")
        self.assertEqual(g.equipped, ["A3"])

    def test_pa4_forfeit_loses_artifact(self):
        g = self._g("A3"); t = g.pawn_art("A3")
        g.cash = 100000
        for _ in range(3):
            g.end_day()
        self.assertEqual(g.tickets[t]["state"], "forfeited")
        self.assertEqual(g.owned, [])
        with self.assertRaises(Rejected):
            g.redeem(t)

    def test_pa5_cannot_pawn_unequipped_or_twice(self):
        g = self._g("A3")
        g.grant_artifact("B1")                  # có nhưng chưa trang bị
        with self.assertRaises(Rejected):
            g.pawn_art("B1")
        g.pawn_art("A3")
        with self.assertRaises(Rejected):
            g.pawn_art("A3")

    def test_pa6_shares_two_ticket_limit_with_items(self):
        g = self._g("A1", "A2", "A3")
        g.pawn(item(g)); g.pawn(item(g))
        with self.assertRaises(Rejected):
            g.pawn_art("A1")
        self.assertIn("A1", g.equipped)

    def test_pa7_reload_keeps_ticket_no_second_loan(self):
        g = self._g("A3"); g.pawn_art("A3")
        h = ServiceGame.load(g.save())
        with self.assertRaises(Rejected):
            h.pawn_art("A3")
        self.assertEqual(h.cash, 300)


class Auction(unittest.TestCase):
    def test_au1_fee_130_and_sale_3380_when_k13(self):
        s = seed_for_k(1.3)
        g = G(1000, seed=s, day=3); i = item(g, 2860)
        g.auction(i)
        self.assertEqual(g.cash, 857)       # phí 143
        g.cash += 5000
        before = g.cash
        g.end_day()
        gained = g.cash - (before - DEBT[2])
        self.assertEqual(gained, 3718)

    def test_au2_floor_not_met_item_returned_fee_lost(self):
        s = seed_for_k(1.1)
        g = G(1000, seed=s, day=3); i = item(g, 2860)
        g.auction(i, 1.3)
        g.cash = 5000
        g.end_day()
        self.assertIn(i, g.items)
        self.assertEqual(g.cash, 5000 - DEBT[2])  # không nhận tiền, phí đã trả trước

    def test_au3_day7_rejected(self):
        g = G(day=7); i = item(g)
        with self.assertRaises(Rejected):
            g.auction(i)

    def test_au4_pending_not_counted_for_debt(self):
        s = seed_for_k(1.5)
        g = G(300, seed=s, day=3); i = item(g, 5000)
        g.auction(i)                       # phí 250, còn 50
        g.end_day()                        # nợ 500, thiếu
        self.assertGreater(g.arrears, 0)

    def test_au5_reload_keeps_lock_fee_not_recharged(self):
        g = G(1000, day=3); i = item(g, 2860); g.auction(i)
        h = ServiceGame.load(g.save())
        cash = h.cash
        with self.assertRaises(Rejected):
            h.auction(i)
        self.assertEqual(h.cash, cash)
        self.assertEqual(len(h.listings), 1)

    def test_au6_one_per_day_resets(self):
        g = G(day=3); a, b = item(g), item(g)
        g.auction(a)
        with self.assertRaises(Rejected):
            g.auction(b)
        g.end_day()
        g.auction(b)

    def test_au7_proceeds_arrive_start_of_next_day(self):
        s = seed_for_k(1.5)
        g = G(2000, seed=s, day=3); i = item(g, 2860)
        g.auction(i)                       # phí 143, còn 1.857
        g.end_day()                        # trả nợ 500, rồi nhận 4.290 đầu ngày 4
        self.assertEqual((g.day, g.cash), (4, 1857 - 500 + 4290))

    def test_au8_result_fixed_by_seed_not_reroll(self):
        g = G(1000, day=3); blob = g.save()
        i = item(g, 2860); l = g.auction(i); k1 = g.listings[l]["k"]
        h = ServiceGame.load(blob)
        j = item(h, 2860)
        l2 = h.auction(j)
        self.assertEqual(k1, h.listings[l2]["k"])

    def test_au3b_below_min_g_rejected(self):
        g = G(day=3); i = item(g, 895)
        with self.assertRaises(Rejected):
            g.auction(i)
        self.assertIn(i, g.items)
        self.assertEqual(g.cash, 5000)

    def test_au9_invalid_floor_rejected(self):
        g = G(day=3); i = item(g)
        with self.assertRaises(Rejected):
            g.auction(i, 2.0)
        self.assertIn(i, g.items)

    def test_au10_expected_k_is_116(self):
        self.assertAlmostEqual(sum(p * k for p, k in AUCTION_K), 1.16, places=2)


class Casino(unittest.TestCase):
    def test_cs1_win_adds_stake(self):
        s = next(s for s in range(500) if G(1000, s, 2).bet(100))
        g = G(1000, s, 2); g.bet(100)
        self.assertEqual(g.cash, 1100)

    def test_cs2_lose_subtracts_stake(self):
        s = next(s for s in range(500) if not G(1000, s, 2).bet(100))
        g = G(1000, s, 2); g.bet(100)
        self.assertEqual(g.cash, 900)

    def test_cs3_third_bet_rejected_and_resets(self):
        g = G(1000, day=2)
        for _ in range(2):
            g.bet(100)
        with self.assertRaises(Rejected):
            g.bet(100)
        g.end_day()
        g.bet(100)

    def test_cs4_insufficient_cash(self):
        g = G(80, day=2)
        with self.assertRaises(Rejected):
            g.bet(100)
        self.assertEqual((g.cash, g.bets_today), (80, 0))

    def test_cs5_reload_cannot_reroll(self):
        g = G(1000, day=2); blob = g.save()
        w = g.bet(100)
        h = ServiceGame.load(blob); item(h)
        self.assertEqual(h.bet(100), w)

    def test_cs6_not_before_day2_and_stakes_fixed(self):
        with self.assertRaises(Rejected):
            G(1000, day=1).bet(100)
        g = G(1000, day=2)
        for amt in (0, 50, 200, 1000):
            with self.assertRaises(Rejected):
                g.bet(amt)

    def test_cs7_replayed_tx_rejected(self):
        g = G(1000, day=2); g.do("a", "bet", 100)
        c = g.cash
        with self.assertRaises(Rejected):
            g.do("a", "bet", 100)
        self.assertEqual((g.cash, g.bets_today), (c, 1))

    def test_cs8_expected_value_is_minus_10_percent(self):
        tot = 0
        for s in range(6000):
            g = G(1000, s, 2); b = g.cash; g.bet(100); tot += g.cash - b
        self.assertAlmostEqual(tot / 6000 / 100, -0.10, delta=0.03)


class MetaTests(unittest.TestCase):
    def test_mt1_collect_once(self):
        m = Meta(); m.inspect("V01"); m.inspect("V01")
        self.assertEqual(len(m.collected), 1)

    def test_mt2_five_unlocks_m1(self):
        m = Meta()
        for c in ("V01", "V02", "V03", "V04", "V05"):
            m.inspect(c)
        self.assertEqual(m.unlocked(), {"M1"})

    def test_mt3_loss_keeps_unlocks_via_save(self):
        m = Meta()
        for c in ("V01", "V02", "V03", "V04", "V05"):
            m.inspect(c)
        self.assertEqual(Meta.load(m.dump()).unlocked(), {"M1"})

    def test_mt4_win_counts_once_per_game(self):
        m = Meta()
        for _ in range(5):
            m.record_win("game-7")
        self.assertEqual(len(m.wins), 1)
        self.assertEqual(m.unlocked(), {"M3"})

    def test_mt5_start_cash_cap(self):
        m = Meta()
        for i in range(1, 11):
            m.inspect(f"V{i:02d}")
        self.assertEqual(m.start_cash(), 1100)
        self.assertLessEqual(m.start_cash() - 1000, 100)

    def test_mt6_m4_needs_all_secrets(self):
        m = Meta(); m.record_bikhe("BK1")
        self.assertNotIn("M4", m.unlocked())
        m.record_bikhe("BK2")
        self.assertIn("M4", m.unlocked())


class SaveLoad(unittest.TestCase):
    def test_sv1_tickets_and_listings_survive(self):
        g = G(1000, day=3); a, b = item(g), item(g)
        t = g.pawn(a); g.auction(b)
        h = ServiceGame.load(g.save())
        self.assertEqual((h.tickets, h.listings), (g.tickets, g.listings))

    def test_sv2_load_mid_auction_pays_once(self):
        g = G(5000, day=3); i = item(g, 2860); g.auction(i)
        h = ServiceGame.load(g.save())
        h.end_day(); h.end_day()
        self.assertEqual(sum(1 for r, d in h.ledger if r == "auction_sale"), 1)

    def test_sv3_forfeit_survives_load(self):
        g = G(100000, day=3); i = item(g); t = g.pawn(i)
        for _ in range(3):
            g.end_day()
        h = ServiceGame.load(g.save())
        with self.assertRaises(Rejected):
            h.redeem(t)


class Fuzz(unittest.TestCase):
    def test_fuzz_invariants_and_replay_safe(self):
        for seed in range(300):
            r = random.Random(seed)
            g = G(1000, seed=seed)
            n = 0
            for step in range(150):
                if g.status != "playing":
                    break
                kind = r.choice(["buy", "sell", "pawn", "redeem", "auction", "bet", "day", "load", "replay"])
                n += 1
                tx = f"t{n}"
                try:
                    if kind == "buy":
                        b = g.do(tx, "buy_box", r.choice(["nho", "vua"])); g.do(tx + "o", "open_box", b)
                    elif kind in ("sell", "pawn", "auction") and g.items:
                        iid = r.choice(list(g.items))
                        if kind == "auction":
                            g.do(tx, "auction", iid, r.choice([None, 1.0, 1.3]))
                        else:
                            g.do(tx, {"sell": "sell_item", "pawn": "pawn"}[kind], iid)
                    elif kind == "redeem" and g.tickets:
                        g.do(tx, "redeem", r.choice(list(g.tickets)))
                    elif kind == "bet":
                        g.do(tx, "bet", r.choice([100, 250, 500]))
                    elif kind == "day":
                        g.do(tx, "end_day")
                    elif kind == "load":
                        g = ServiceGame.load(g.save())
                    elif kind == "replay" and g.applied_tx:
                        before = g.save()
                        with self.assertRaises(Rejected):
                            g.do(r.choice(sorted(g.applied_tx)), "bet", 100)
                        self.assertEqual(before, g.save())
                except Rejected:
                    pass
                g.check_invariants()


if __name__ == "__main__":
    unittest.main(verbosity=2)

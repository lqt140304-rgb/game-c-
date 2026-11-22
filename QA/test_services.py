"""Sprint 4: chống giao dịch lặp cho cầm cố, đấu giá, sòng bạc, biến cố và lưu/tải.

Luật các dịch vụ là GIẢ ĐỊNH của Claude 2 (xem services_model.py), chưa được Claude 1 chốt.
Khi luật chốt, đổi hằng số và giữ nguyên ý nghĩa các test: mỗi giao dịch đổi trạng thái đúng một lần.
"""
import random
import unittest
from economy_model import Rejected
from services_model import *
import services_model as sm


def G(cash=1000, seed=3):
    g = ServiceGame(seed=seed); g.cash = cash
    return g


def item(g, value=1000, fake=False):
    i = g._id()
    g.items[i] = {"value": value, "fake": fake, "origin": "x", "appraised": False}
    return i


class Pawn(unittest.TestCase):
    def test_pw1_pawn_gives_loan_and_removes_item(self):
        g = G(0); i = item(g, 1000)
        g.pawn(i)
        self.assertEqual((g.cash, i in g.items), (500, False))

    def test_pw2_double_pawn_rejected(self):
        g = G(0); i = item(g, 1000)
        g.pawn(i)
        with self.assertRaises(Rejected):
            g.pawn(i)
        self.assertEqual(g.cash, 500)

    def test_pw3_redeem_once(self):
        g = G(0); i = item(g, 1000)
        t = g.pawn(i); g.cash += 100
        g.redeem(t)
        self.assertEqual((g.cash, i in g.items), (500 + 100 - 575, True))
        with self.assertRaises(Rejected):
            g.redeem(t)
        self.assertEqual(g.cash, 25)

    def test_pw4_redeem_needs_cash_no_change(self):
        g = G(0); i = item(g, 1000)
        t = g.pawn(i)           # còn 500, cần 575
        before = (g.cash, dict(g.items), g.tickets[t]["state"])
        with self.assertRaises(Rejected):
            g.redeem(t)
        self.assertEqual(before, (g.cash, dict(g.items), g.tickets[t]["state"]))

    def test_pw5_forfeited_after_deadline_cannot_redeem(self):
        g = G(5000); i = item(g, 1000)
        t = g.pawn(i)           # hạn ngày 3
        g.end_day(); g.end_day(); g.end_day()   # sang ngày 4
        self.assertEqual(g.tickets[t]["state"], "forfeited")
        with self.assertRaises(Rejected):
            g.redeem(t)
        self.assertNotIn(i, g.items)

    def test_pw6_redeem_on_last_day_allowed(self):
        g = G(5000); i = item(g, 1000)
        t = g.pawn(i)
        g.end_day(); g.end_day()   # ngày 3 = hạn
        g.redeem(t)
        self.assertIn(i, g.items)

    def test_pw7_fake_item_pawn_value_uses_fake_price(self):
        g = G(0); i = item(g, 1000, fake=True)
        g.pawn(i)
        self.assertEqual(g.cash, 100)   # 1000 * 0,2 * 0,5

    def test_pw8_pawned_item_cannot_be_sold(self):
        g = G(0); i = item(g)
        g.pawn(i)
        with self.assertRaises(Rejected):
            g.sell_item(i)


class Auction(unittest.TestCase):
    def test_au1_cash_arrives_next_day_once(self):
        g = G(5000); i = item(g, 1000)
        g.auction(i)
        before = g.cash
        g.end_day()
        gained = g.cash - (before - DEBT[0])
        self.assertGreater(gained, 0)
        g.end_day()   # không nhận lần hai
        self.assertEqual(g.cash, before - DEBT[0] + gained - DEBT[1])

    def test_au2_double_list_rejected(self):
        g = G(); i = item(g)
        g.auction(i)
        with self.assertRaises(Rejected):
            g.auction(i)
        self.assertEqual(len(g.listings), 1)

    def test_au3_not_available_day7(self):
        g = G(); g.day = 7; i = item(g)
        with self.assertRaises(Rejected):
            g.auction(i)
        self.assertIn(i, g.items)

    def test_au4_payout_fixed_at_listing_reload_same(self):
        g = G(); i = item(g, 1000)
        blob = g.save()
        l1 = g.auction(i); p1 = g.listings[l1]["payout"]
        h = ServiceGame.load(blob)
        l2 = h.auction(i)
        self.assertEqual(p1, h.listings[l2]["payout"])

    def test_au5_listed_item_cannot_be_sold_or_pawned(self):
        g = G(); i = item(g)
        g.auction(i)
        for fn in (g.sell_item, g.pawn):
            with self.assertRaises(Rejected):
                fn(i)

    def test_au6_proceeds_arrive_after_debt_collection(self):
        # Proceeds cannot pay today's debt (D-13): cash 100, debt 200 -> shortfall though listing worth more.
        g = G(100); i = item(g, 5000)
        g.auction(i)
        g.end_day()
        self.assertEqual(g.arrears, 120)   # 100 trả được, tồn 100 + phạt 20


class Casino(unittest.TestCase):
    def test_cs1_bet_changes_cash_by_exactly_stake(self):
        g = G(1000)
        w = g.bet(100)
        self.assertEqual(g.cash, 1100 if w else 900)

    def test_cs2_limits(self):
        g = G(1000)
        for amt in (0, -5, CASINO_MAX_BET + 1, 5000):
            with self.assertRaises(Rejected):
                g.bet(amt)
        self.assertEqual((g.cash, g.bets_today), (1000, 0))

    def test_cs3_bets_per_day_cap_and_reset(self):
        g = G(1000)
        g.bet(10); g.bet(10)
        with self.assertRaises(Rejected):
            g.bet(10)
        g.end_day()
        g.bet(10)

    def test_cs4_reload_cannot_reroll(self):
        g = G(1000); blob = g.save()
        w1 = g.bet(100)
        h = ServiceGame.load(blob)
        item(h)                    # thao tác khác không làm đổi kết quả
        self.assertEqual(h.bet(100), w1)

    def test_cs5_result_independent_of_other_actions(self):
        a = G(1000, seed=9); b = G(1000, seed=9)
        b.buy_box("nho"); b.sell_item(b.open_box(next(iter(b.boxes))))
        self.assertEqual(a.bet(50), b.bet(50))

    def test_cs6_replayed_tx_rejected(self):
        g = G(1000)
        g.do("tx1", "bet", 100)
        cash = g.cash
        with self.assertRaises(Rejected):
            g.do("tx1", "bet", 100)
        self.assertEqual((g.cash, g.bets_today), (cash, 1))

    def test_cs7_house_edge_over_many_seeds(self):
        total = 0
        for s in range(4000):
            g = G(1000, seed=s); total += (g.cash - 1000) if False else 0
            before = g.cash; g.bet(100); total += g.cash - before
        self.assertLess(total / 4000 / 100, 0)   # nhà cái có lợi thế


class Events(unittest.TestCase):
    def test_ev1_event_deterministic_per_day(self):
        g = G(seed=5)
        self.assertEqual([g.event(d) for d in range(1, 8)], [g.event(d) for d in range(1, 8)])

    def test_ev2_event_not_dependent_on_actions(self):
        a, b = G(seed=5), G(seed=5)
        b.bet(10)
        self.assertEqual(a.event(), b.event())

    def test_ev3_event_applies_to_sale_once(self):
        # tìm seed ngày 1 có biến cố tăng giá
        s = next(s for s in range(100) if G(seed=s).event() == 1.2)
        g = G(0, seed=s); i = item(g, 1000)
        self.assertEqual(g.sell_item(i), 1200)
        with self.assertRaises(Rejected):
            g.sell_item(i)

    def test_ev4_event_same_after_reload(self):
        g = G(seed=7); d = g.event()
        self.assertEqual(ServiceGame.load(g.save()).event(), d)


class SaveLoad(unittest.TestCase):
    def test_sv1_tickets_and_listings_survive(self):
        g = G(1000); a, b = item(g), item(g)
        t = g.pawn(a); l = g.auction(b)
        h = ServiceGame.load(g.save())
        self.assertEqual((h.tickets, h.listings), (g.tickets, g.listings))
        h.cash += 600
        h.redeem(t)

    def test_sv2_load_mid_auction_pays_once(self):
        g = G(5000); i = item(g, 1000); g.auction(i)
        h = ServiceGame.load(g.save())
        h.end_day()
        h.end_day()
        self.assertEqual(sum(1 for t, r, d in h.ledger if r == "auction"), 1)

    def test_sv3_forfeit_survives_load(self):
        g = G(5000); i = item(g); t = g.pawn(i)
        for _ in range(3):
            g.end_day()
        h = ServiceGame.load(g.save())
        with self.assertRaises(Rejected):
            h.redeem(t)


class Fuzz(unittest.TestCase):
    """Chuỗi thao tác ngẫu nhiên, thỉnh thoảng lưu/tải và phát lại giao dịch cũ."""

    def test_fuzz_invariants_and_replay_safe(self):
        for seed in range(300):
            r = random.Random(seed)
            g = G(1000, seed=seed)
            log = []
            n = 0
            for step in range(120):
                if g.status != "playing":
                    break
                kind = r.choice(["buy", "sell", "pawn", "redeem", "auction", "bet", "day", "load", "replay"])
                n += 1
                tx = f"t{n}"
                try:
                    if kind == "buy":
                        b = g.do(tx, "buy_box", r.choice(["nho", "vua"])); g.do(tx + "o", "open_box", b)
                    elif kind in ("sell", "pawn", "auction") and g.items:
                        g.do(tx, {"sell": "sell_item", "pawn": "pawn", "auction": "auction"}[kind], r.choice(list(g.items)))
                    elif kind == "redeem" and g.tickets:
                        g.do(tx, "redeem", r.choice(list(g.tickets)))
                    elif kind == "bet":
                        g.do(tx, "bet", r.choice([10, 50, 200]))
                    elif kind == "day":
                        g.do(tx, "end_day")
                    elif kind == "load":
                        g = ServiceGame.load(g.save())
                    elif kind == "replay" and g.applied_tx:
                        before = g.save()
                        with self.assertRaises(Rejected):
                            g.do(r.choice(sorted(g.applied_tx)), "bet", 10)
                        self.assertEqual(before, g.save())
                except Rejected:
                    pass
                g.check_invariants()


if __name__ == "__main__":
    unittest.main(verbosity=2)

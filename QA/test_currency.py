import unittest
import currency as C
from economy_model import START_CASH, DEBT, BOX_PRICE


class Currency(unittest.TestCase):
    def test_start_cash_is_ten_million(self):
        self.assertEqual(C.fmt_vnd(START_CASH), "10.000.000 đ")

    def test_debt_schedule_in_dong(self):
        self.assertEqual([C.to_vnd(d) for d in DEBT],
                         [2_000_000, 3_500_000, 5_000_000, 7_500_000, 10_500_000, 15_000_000, 22_000_000])
        self.assertEqual(C.fmt_vnd(sum(DEBT)), "65.500.000 đ")

    def test_box_prices(self):
        self.assertEqual([C.to_vnd(BOX_PRICE[k]) for k in ("nho", "vua", "lon")], [1_000_000, 3_500_000, 8_000_000])

    def test_round_trip_and_multiple_of_unit(self):
        self.assertEqual(C.from_vnd(C.to_vnd(1234)), 1234)
        with self.assertRaises(ValueError):
            C.from_vnd(15_000)

    def test_short_format(self):
        self.assertEqual(C.fmt_vnd(2200, short=True), "22 triệu")
        self.assertEqual(C.fmt_vnd(15, short=True), "150.000 đ")


if __name__ == "__main__":
    unittest.main(verbosity=2)

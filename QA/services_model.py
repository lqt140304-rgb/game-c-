"""Mô hình dịch vụ chợ cho Sprint 4 (Claude 2): cầm cố, đấu giá, sòng bạc, biến cố.

TẤT CẢ THAM SỐ LÀ GIẢ ĐỊNH CỦA CLAUDE 2. Claude 1 chưa chốt luật các dịch vụ này (Sprint 4).
Mô hình có hai mục đích:
1. Đặc tả hành vi chống giao dịch lặp (mỗi dịch vụ đổi trạng thái đúng một lần).
2. Cho phép đo ảnh hưởng của từng dịch vụ lên thanh khoản và nợ (xem sprint4_sim.py).
"""
import random
from economy_model import Game, Rejected, DEBT, FAKE_FACTOR

PAWN_RATIO, PAWN_FEE, PAWN_DAYS = 0.5, 0.15, 2      # vay 50% giá trị, chuộc +15%, hạn 2 ngày
AUCTION_PREMIUM, AUCTION_SPREAD, AUCTION_FEE = 0.25, 0.5, 0.10  # kỳ vọng +25%, dao động ±50%, phí 10%
CASINO_WIN_P, CASINO_MAX_BET, CASINO_BETS_PER_DAY = 0.45, 500, 2  # thắng ăn 1 đền 1, lợi thế nhà cái 10%
EVENT_P, EVENT_SWING = 0.5, 0.2                      # 50% ngày có biến cố, giá bán +-20%


class ServiceGame(Game):
    def __post_init__(self):
        super().__post_init__()
        self._s()

    # ----- thuộc tính trạng thái dịch vụ (khai báo lười để dataclass không đổi) -----
    def _s(self):
        d = self.__dict__
        d.setdefault("tickets", {})       # mã vé -> {item, loan, redeem_cost, due, state}
        d.setdefault("listings", {})      # mã lô -> {item, payout, due, state}
        d.setdefault("svc_id", 1)
        d.setdefault("bets_today", 0)
        d.setdefault("ledger", [])        # (tx, lý do, thay đổi tiền)
        return d

    def _sid(self, prefix):
        d = self._s()
        n = d["svc_id"]; d["svc_id"] += 1
        return f"{prefix}{n}"

    def _money(self, delta, reason, tx=None):
        self.cash += delta
        self._s()["ledger"].append((tx, reason, delta))

    # ----- biến cố: xác định theo (seed, ngày), không phụ thuộc thứ tự thao tác -----
    def event(self, day=None):
        r = random.Random(f"{self.seed}-event-{day or self.day}")
        if r.random() >= EVENT_P:
            return None
        return 1 + EVENT_SWING if r.random() < 0.5 else 1 - EVENT_SWING

    def sell_item(self, item_id):
        self._check_playing()
        if item_id not in self.items:
            raise Rejected("hiện vật không tồn tại hoặc đã bán")
        it = self.items.pop(item_id)
        p = it["value"] * (FAKE_FACTOR if it["fake"] else 1) * (self.event() or 1)
        p = round(p)
        self._money(p, "sell")
        return p

    # ----- cầm cố -----
    def pawn(self, item_id):
        self._check_playing(); self._s()
        if item_id not in self.items:
            raise Rejected("hiện vật không có trong kho")
        it = self.items.pop(item_id)
        v = it["value"] * (FAKE_FACTOR if it["fake"] else 1)
        loan = round(v * PAWN_RATIO)
        tid = self._sid("T")
        self.tickets[tid] = {"item": it, "loan": loan, "cost": round(loan * (1 + PAWN_FEE)),
                             "due": self.day + PAWN_DAYS, "state": "open", "item_id": item_id}
        self._money(loan, "pawn")
        return tid

    def redeem(self, tid):
        self._check_playing(); self._s()
        t = self.tickets.get(tid)
        if t is None or t["state"] != "open":
            raise Rejected("vé không tồn tại hoặc đã đóng")
        if self.day > t["due"]:
            raise Rejected("quá hạn chuộc")
        if self.cash < t["cost"]:
            raise Rejected("không đủ tiền chuộc")
        self._money(-t["cost"], "redeem")
        t["state"] = "redeemed"
        self.items[t["item_id"]] = t["item"]

    # ----- đấu giá -----
    def auction(self, item_id):
        self._check_playing(); self._s()
        if self.day >= len(DEBT):
            raise Rejected("ngày 7 không kịp nhận tiền")
        if item_id not in self.items:
            raise Rejected("hiện vật không có trong kho")
        it = self.items.pop(item_id)
        v = it["value"] * (FAKE_FACTOR if it["fake"] else 1)
        # kết quả xác định lúc đăng lô, không đổi khi tải lại
        r = random.Random(f"{self.seed}-auction-{item_id}")
        mult = 1 + AUCTION_PREMIUM + (r.random() * 2 - 1) * AUCTION_SPREAD
        lid = self._sid("L")
        self.listings[lid] = {"payout": round(v * mult * (1 - AUCTION_FEE)), "due": self.day + 1, "state": "open"}
        return lid

    # ----- sòng bạc -----
    def bet(self, amount):
        self._check_playing(); self._s()
        if amount <= 0 or amount > CASINO_MAX_BET:
            raise Rejected("mức cược không hợp lệ")
        if amount > self.cash:
            raise Rejected("không đủ tiền cược")
        if self.bets_today >= CASINO_BETS_PER_DAY:
            raise Rejected("hết lượt cược trong ngày")
        # kết quả xác định theo (seed, ngày, số thứ tự lượt) để không thể tải lại để đổi
        r = random.Random(f"{self.seed}-casino-{self.day}-{self.bets_today}")
        win = r.random() < CASINO_WIN_P
        self.bets_today += 1
        self._money(amount if win else -amount, "bet")
        return win

    # ----- ngày -----
    def end_day(self):
        self._s()
        res = super().end_day()
        if self.status != "playing":
            return res
        self.bets_today = 0
        for l in self.listings.values():
            if l["state"] == "open" and l["due"] <= self.day:
                self._money(l["payout"], "auction")
                l["state"] = "paid"
        for t in self.tickets.values():
            if t["state"] == "open" and self.day > t["due"]:
                t["state"] = "forfeited"
        return res

    # ----- giao dịch có mã -----
    def do(self, tx, name, *args):
        if tx in self.applied_tx:
            raise Rejected("giao dịch đã áp dụng")
        r = getattr(self, name)(*args)
        self.applied_tx.add(tx)
        return r

    # ----- bất biến để fuzz -----
    def check_invariants(self):
        s = self._s()
        assert self.cash >= 0, "tiền âm"
        ids = list(self.items)
        ids += [t["item_id"] for t in self.tickets.values() if t["state"] == "open"]
        assert len(ids) == len(set(ids)), "hiện vật trùng"
        for t in self.tickets.values():
            assert t["state"] in ("open", "redeemed", "forfeited")
        for l in self.listings.values():
            assert l["state"] in ("open", "paid")
        pawn = sum(d for _, r, d in s["ledger"] if r == "pawn")
        redeem = -sum(d for _, r, d in s["ledger"] if r == "redeem")
        assert pawn == sum(t["loan"] for t in self.tickets.values())
        assert redeem == sum(t["cost"] for t in self.tickets.values() if t["state"] == "redeemed")
        paid = sum(d for _, r, d in s["ledger"] if r == "auction")
        assert paid == sum(l["payout"] for l in self.listings.values() if l["state"] == "paid")

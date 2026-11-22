"""Mô hình dịch vụ chợ theo đề xuất của Claude 1 (Docs/Content/DichVuVaMeta.md, commit d6b597e,
nhánh claude/relaxed-cerf-wgnu7l): biến cố E1-E6, đấu giá, sòng bạc, cầm cố, meta.

Mọi con số là [ĐỀ XUẤT] của Claude 1. Mô hình dùng để (1) kiểm thử chống giao dịch lặp và
(2) đo ảnh hưởng ở sprint4_sim.py.
"""
import math
import random
from economy_model import Game, Rejected, DEBT, FAKE_FACTOR, BOX_PRICE

PAWN_RATIO, PAWN_FEE, PAWN_DAYS, PAWN_MAX = 0.60, 0.15, 2, 2
AUCTION_FEE = 0.05
AUCTION_K = [(0.2, 0.9), (0.4, 1.1), (0.3, 1.3), (0.1, 1.5)]   # kỳ vọng 1,16
CASINO_STAKES, CASINO_P, CASINO_PER_DAY, CASINO_FROM_DAY = (100, 250, 500), 0.45, 2, 2
AUCTION_MIN_G = 1300
ART_BUY_PRICE = {"A1": 150, "A2": 400, "A3": 500, "B1": 120, "B2": 450, "B3": 500}  # CP1..TC3, giá đã chốt
EVENT_P, EVENT_FROM_DAY = 0.5, 2
EVENTS = ["E1", "E2", "E3", "E4", "E5", "E6"]
RARE_CODES = ()   # hiện vật "Rất hiếm": đặt it["rare"] = True


class ServiceGame(Game):
    def __post_init__(self):
        super().__post_init__()
        self._s()

    def _s(self):
        d = self.__dict__
        d.setdefault("tickets", {})
        d.setdefault("listings", {})
        d.setdefault("svc_id", 1)
        d.setdefault("bets_today", 0)
        d.setdefault("auctions_today", 0)
        d.setdefault("ledger", [])
        return d

    def _sid(self, prefix):
        n = self.svc_id; self.svc_id += 1
        return f"{prefix}{n}"

    def _money(self, delta, reason):
        self.cash += delta
        self.ledger.append((reason, delta))

    # ----- biến cố -----
    def event(self, day=None):
        day = day or self.day
        if day < EVENT_FROM_DAY:
            return None
        r = random.Random(f"{self.seed}-event-{day}")
        if r.random() >= EVENT_P:
            return None
        return EVENTS[r.randrange(6)]

    def price_bonus(self, it):
        e = self.event()
        if e == "E1" and it.get("origin") == "Qua khu":
            return 0.15
        if e == "E2" and it.get("origin") in ("Hien dai", "Tuong lai"):
            return -0.15
        if e == "E6" and it.get("rare"):
            return 0.10
        return 0.0

    def G(self, it):
        """Giá bán thường ngay tại quầy, đã tính biến cố (theo phép cộng)."""
        return math.floor(it["value"] * (FAKE_FACTOR if it["fake"] else 1) * (1 + self.price_bonus(it)))

    def box_price(self, kind):
        if kind == "vua" and self.event() == "E3":
            return round(BOX_PRICE[kind] * 0.8)
        return BOX_PRICE[kind]

    def buy_box(self, kind):
        from economy_model import BOX_PRICE as BP
        old = BP[kind]
        BP[kind] = self.box_price(kind)
        try:
            return super().buy_box(kind)
        finally:
            BP[kind] = old

    def appraise(self, item_id):
        if self.event() == "E4":
            raise Rejected("Giáo Sư đi vắng")
        return super().appraise(item_id)

    def sell_item(self, item_id):
        self._check_playing()
        if item_id not in self.items:
            raise Rejected("hiện vật không tồn tại hoặc đã bán")
        it = self.items.pop(item_id)
        p = self.G(it)
        self._money(p, "sell")
        return p

    # ----- cầm cố -----
    def pawn(self, item_id):
        self._check_playing()
        if item_id not in self.items:
            raise Rejected("hiện vật không có trong kho")
        if sum(1 for t in self.tickets.values() if t["state"] == "open") >= PAWN_MAX:
            raise Rejected("đã cầm tối đa")
        it = self.items.pop(item_id)
        loan = math.floor(self.G(it) * PAWN_RATIO)
        tid = self._sid("T")
        self.tickets[tid] = {"item": it, "item_id": item_id, "loan": loan,
                             "cost": math.ceil(loan * (1 + PAWN_FEE)), "due": self.day + PAWN_DAYS, "state": "open"}
        self._money(loan, "pawn")
        return tid

    def pawn_art(self, aid):
        """Cầm tạo tác đang trang bị: 60% giá mua. Tạo tác bị tháo khỏi ô nên mất hiệu ứng và combo."""
        self._check_playing()
        if aid not in self.equipped:
            raise Rejected("tạo tác không ở trong ô trang bị")
        if sum(1 for t in self.tickets.values() if t["state"] == "open") >= PAWN_MAX:
            raise Rejected("đã cầm tối đa")
        loan = math.floor(ART_BUY_PRICE[aid] * PAWN_RATIO)
        self.equipped.remove(aid)
        self.owned.remove(aid)
        tid = self._sid("T")
        self.tickets[tid] = {"kind": "art", "aid": aid, "loan": loan, "cost": math.ceil(loan * (1 + PAWN_FEE)),
                             "due": self.day + PAWN_DAYS, "state": "open"}
        self._money(loan, "pawn")
        return tid

    def redeem(self, tid):
        self._check_playing()
        t = self.tickets.get(tid)
        if t is None or t["state"] != "open":
            raise Rejected("vé không tồn tại hoặc đã đóng")
        if self.day > t["due"]:
            raise Rejected("quá hạn chuộc")
        if self.cash < t["cost"]:
            raise Rejected("không đủ tiền chuộc")
        self._money(-t["cost"], "redeem")
        t["state"] = "redeemed"
        if t.get("kind") == "art":
            self.owned.append(t["aid"])        # chuộc về kho tạo tác, phải trang bị lại
        else:
            self.items[t["item_id"]] = t["item"]

    # ----- đấu giá -----
    def auction(self, item_id, floor=None):
        self._check_playing()
        if self.day >= len(DEBT):
            raise Rejected("ngày 7 không đấu giá")
        if self.auctions_today >= 1:
            raise Rejected("mỗi ngày một lô")
        if floor not in (None, 1.0, 1.3):
            raise Rejected("giá sàn không hợp lệ")
        if item_id not in self.items:
            raise Rejected("hiện vật không có trong kho")
        g = self.G(self.items[item_id])
        if g < AUCTION_MIN_G:
            raise Rejected("G dưới 1.300 xu không đấu giá")
        fee = math.ceil(g * AUCTION_FEE)
        if self.cash < fee:
            raise Rejected("không đủ tiền đăng ký")
        it = self.items.pop(item_id)
        self._money(-fee, "auction_fee")
        r = random.Random(f"{self.seed}-auction-{item_id}")
        x, acc, k = r.random(), 0.0, AUCTION_K[-1][1]
        for p, v in AUCTION_K:
            acc += p
            if x <= acc:
                k = v
                break
        lid = self._sid("L")
        self.listings[lid] = {"item": it, "item_id": item_id, "G": g, "floor": floor, "k": k,
                              "due": self.day + 1, "state": "open"}
        self.auctions_today += 1
        return lid

    # ----- sòng bạc -----
    def bet(self, amount):
        self._check_playing()
        if self.day < CASINO_FROM_DAY:
            raise Rejected("sòng bạc mở từ ngày 2")
        if amount not in CASINO_STAKES:
            raise Rejected("mức cược không hợp lệ")
        if amount > self.cash:
            raise Rejected("không đủ tiền cược")
        if self.bets_today >= CASINO_PER_DAY:
            raise Rejected("hết lượt trong ngày")
        r = random.Random(f"{self.seed}-casino-{self.day}-{self.bets_today}")
        win = r.random() < CASINO_P
        self.bets_today += 1
        self._money(amount if win else -amount, "bet")
        return win

    # ----- ngày -----
    def end_day(self):
        res = super().end_day()
        if self.status != "playing":
            # hết ván: hiện vật chưa chuộc coi như mất
            for t in self.tickets.values():
                if t["state"] == "open":
                    t["state"] = "forfeited"
            return res
        self.bets_today = 0
        self.auctions_today = 0
        for l in self.listings.values():       # đầu ngày mới: công bố kết quả
            if l["state"] == "open" and l["due"] <= self.day:
                price = round(l["G"] * l["k"])
                if l["floor"] is None or price >= round(l["G"] * l["floor"]):
                    self._money(price, "auction_sale")
                    l["state"] = "sold"
                else:
                    self.items[l["item_id"]] = l["item"]
                    l["state"] = "returned"
        for t in self.tickets.values():
            if t["state"] == "open" and self.day > t["due"]:
                t["state"] = "forfeited"
        return res

    def do(self, tx, name, *args):
        if tx in self.applied_tx:
            raise Rejected("giao dịch đã áp dụng")
        r = getattr(self, name)(*args)
        self.applied_tx.add(tx)
        return r

    def check_invariants(self):
        assert self.cash >= 0, "tiền âm"
        ids = list(self.items)
        ids += [t["item_id"] for t in self.tickets.values() if t["state"] == "open" and t.get("kind") != "art"]
        ids += [l["item_id"] for l in self.listings.values() if l["state"] == "open"]
        assert len(ids) == len(set(ids)), "hiện vật ở hai nơi"
        pawn = sum(d for r, d in self.ledger if r == "pawn")
        assert pawn == sum(t["loan"] for t in self.tickets.values())
        redeem = -sum(d for r, d in self.ledger if r == "redeem")
        assert redeem == sum(t["cost"] for t in self.tickets.values() if t["state"] == "redeemed")
        sale = sum(d for r, d in self.ledger if r == "auction_sale")
        assert sale == sum(round(l["G"] * l["k"]) for l in self.listings.values() if l["state"] == "sold")
        fee = -sum(d for r, d in self.ledger if r == "auction_fee")
        assert fee == sum(math.ceil(l["G"] * AUCTION_FEE) for l in self.listings.values())


class Meta:
    """Tiến trình giữa các ván (Sổ Sưu Tầm và mở khóa M1-M4)."""
    BIKHE = ("BK1", "BK2")

    def __init__(self):
        self.collected, self.wins, self.bikhe = set(), set(), set()

    def inspect(self, code):
        self.collected.add(code)

    def record_win(self, game_id):
        self.wins.add(game_id)

    def record_bikhe(self, bid):
        self.bikhe.add(bid)

    def unlocked(self):
        u = set()
        if len(self.collected) >= 5: u.add("M1")
        if len(self.collected) >= 10: u.add("M2")
        if self.wins: u.add("M3")
        if set(self.BIKHE) <= self.bikhe: u.add("M4")
        return u

    def start_cash(self):
        return 1000 + (100 if "M2" in self.unlocked() else 0)

    def dump(self):
        import json
        return json.dumps({"c": sorted(self.collected), "w": sorted(self.wins), "b": sorted(self.bikhe)})

    @classmethod
    def load(cls, s):
        import json
        d = json.loads(s)
        m = cls(); m.collected, m.wins, m.bikhe = set(d["c"]), set(d["w"]), set(d["b"])
        return m

"""Mô hình tham chiếu kinh tế CHỢ ĐỒ LẠ (Claude 2, Sprint 1).

Đây là bản thực thi của luật trong GDD và Docs/Balance/README.md, dùng để
kiểm chứng dòng tiền trước khi Codex có code gameplay. Codex có thể dùng
test_economy.py làm đặc tả: mỗi test là một hành vi mong đợi.

Giả định chưa chốt (đánh dấu [MỞ]) nằm ở hằng số hoặc ghi chú trong code.
"""
import random
from dataclasses import dataclass, field

DEBT = [200, 350, 500, 750, 1050, 1500, 2200]
START_CASH = 1000
MAX_BOXES_PER_DAY = 3
BOX_PRICE = {"nho": 100, "vua": 350, "lon": 800}
LOOT = {
    "nho": [(.35, 40), (.30, 120), (.20, 200), (.12, 400), (.03, 1000)],
    "vua": [(.30, 160), (.30, 400), (.20, 620), (.15, 1200), (.05, 2800)],
    "lon": [(.30, 450), (.30, 900), (.20, 1400), (.15, 2600), (.05, 5800)],
}
FAKE_RATE, FAKE_FACTOR = 0.15, 0.2
INVENTORY_CAPACITY = 10  # [MỞ] chưa chốt, đặt tạm để kiểm thử BT-14


class Rejected(Exception):
    """Hành vi bị từ chối. Trạng thái phải giữ nguyên."""


@dataclass
class Game:
    seed: int = 1
    cash: int = START_CASH
    day: int = 1
    bought_today: int = 0
    boxes: dict = field(default_factory=dict)   # id -> loại hộp, chưa mở
    items: dict = field(default_factory=dict)   # id -> {"value","fake"}
    paid: list = field(default_factory=list)    # ngày đã đóng nợ
    status: str = "playing"                     # playing | won | lost
    next_id: int = 1

    def __post_init__(self):
        self.rng = random.Random(self.seed)

    # --- tiện ích ---
    def _id(self):
        i = self.next_id
        self.next_id += 1
        return i

    def snapshot(self):
        return (self.cash, self.day, self.bought_today, dict(self.boxes),
                {k: dict(v) for k, v in self.items.items()}, list(self.paid),
                self.status)

    def _check_playing(self):
        if self.status != "playing":
            raise Rejected("ván đã kết thúc")

    def stored(self):
        return len(self.boxes) + len(self.items)  # [MỞ] hộp chưa mở có chiếm kho?

    # --- hành động ---
    def buy_box(self, kind):
        self._check_playing()
        if self.bought_today >= MAX_BOXES_PER_DAY:
            raise Rejected("hết lượt mua trong ngày")
        price = BOX_PRICE[kind]
        if self.cash < price:
            raise Rejected("không đủ tiền")
        if self.stored() >= INVENTORY_CAPACITY:
            raise Rejected("kho đầy")
        self.cash -= price
        self.bought_today += 1
        bid = self._id()
        self.boxes[bid] = kind
        return bid

    def open_box(self, box_id):
        self._check_playing()
        if box_id not in self.boxes:
            raise Rejected("hộp không tồn tại hoặc đã mở")
        kind = self.boxes.pop(box_id)
        x, acc, value = self.rng.random(), 0.0, LOOT[kind][-1][1]
        for p, v in LOOT[kind]:
            acc += p
            if x <= acc:
                value = v
                break
        fake = self.rng.random() < FAKE_RATE
        iid = self._id()
        self.items[iid] = {"value": value, "fake": fake}
        return iid

    def sell_item(self, item_id):
        self._check_playing()
        if item_id not in self.items:
            raise Rejected("hiện vật không tồn tại hoặc đã bán")
        it = self.items.pop(item_id)
        price = round(it["value"] * (FAKE_FACTOR if it["fake"] else 1))
        self.cash += price
        return price

    def pay_early(self, day):
        self._check_playing()
        if day != self.day:
            raise Rejected("chỉ thanh toán được khoản của ngày hiện tại")
        return self.end_day_collect_only()

    def end_day_collect_only(self):
        raise Rejected("thanh toán chỉ diễn ra khi kết thúc ngày")

    def end_day(self):
        self._check_playing()
        due = DEBT[self.day - 1]
        if self.day in self.paid:
            raise Rejected("khoản nợ ngày này đã đóng")
        if self.cash < due:
            # [MỞ] hậu quả thiếu tiền chưa chốt. Mô hình tạm: thua.
            self.status = "lost"
            return {"result": "shortfall", "short": due - self.cash}
        self.cash -= due
        self.paid.append(self.day)
        if self.day == len(DEBT):
            self.status = "won"
            return {"result": "won", "cash": self.cash}
        self.day += 1
        self.bought_today = 0
        return {"result": "next_day", "day": self.day}

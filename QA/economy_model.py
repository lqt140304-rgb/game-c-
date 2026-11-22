"""Mô hình tham chiếu kinh tế CHỢ ĐỒ LẠ (Claude 2, Sprint 1).

Đây là bản thực thi của luật trong GDD và Docs/Balance/README.md, dùng để
kiểm chứng dòng tiền trước khi Codex có code gameplay. Codex có thể dùng
test_economy.py làm đặc tả: mỗi test là một hành vi mong đợi.

Giả định chưa chốt (đánh dấu [MỞ]) nằm ở hằng số hoặc ghi chú trong code.
"""
import json
import math
import random
from dataclasses import dataclass, field

DEBT = [200, 350, 500, 750, 1050, 1500, 2200]
START_CASH = 1000
MAX_BOXES_PER_DAY = 3
ORIGINS = ["Qua khu", "Hien dai", "Tuong lai", "Ngoai hanh tinh", "Co tich", "Than thoai"]
BOX_PRICE = {"nho": 100, "vua": 350, "lon": 800}
LOOT = {
    "nho": [(.35, 40), (.30, 120), (.20, 200), (.12, 400), (.03, 1000)],
    "vua": [(.30, 160), (.30, 400), (.20, 620), (.15, 1200), (.05, 2800)],
    "lon": [(.30, 450), (.30, 900), (.20, 1400), (.15, 2600), (.05, 5800)],
}
FAKE_RATE, FAKE_FACTOR = 0.15, 0.2
INVENTORY_CAPACITY = 10  # [MỞ] chưa chốt, đặt tạm để kiểm thử BT-14

# Luật nợ [ĐỀ XUẤT] của Claude 1, commit 643ae03 (nhánh claude/relaxed-cerf-wgnu7l)
PENALTY_RATE = 0.20          # phạt trên phần nợ tồn sau mỗi lần thu
LOSE_ARREARS_OVER = 1000     # nợ tồn > ngưỡng này thì thua ngay
KEEP_MIN_CASH = 0            # PHƯƠNG ÁN thử: Anh Bảy chừa lại tối thiểu bấy nhiêu xu. 0 = đúng luật Claude 1

# Tạo tác và Bí Khế: GIÁ TRỊ TẠM, chờ Claude 1 soạn nội dung Sprint 2.
EQUIP_SLOTS = 3                            # [MỞ] số ô trang bị
COMBO_SMALL, COMBO_BIG = 2, 3              # [MỞ] số món cùng hệ để thành combo nhỏ / lớn
ARTIFACTS = {                              # id -> (hệ, tier). 6 món, 2 hệ theo GDD mục 7
    "A1": ("Co", 1), "A2": ("Co", 2), "A3": ("Co", 3),
    "B1": ("Cong", 1), "B2": ("Cong", 2), "B3": ("Cong", 3),
}
BI_KHE = {                                 # id -> điều kiện (loại, tham số, mục tiêu)
    "BK1": ("combo", None, 1),             # hoàn thành một combo
    "BK2": ("appraise_origin", "Than thoai", 2),  # giám định 2 hiện vật nhóm Thần thoại
}


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
    arrears: int = 0                            # nợ tồn
    lose_reason: str = ""
    owned: list = field(default_factory=list)   # tạo tác đang có
    equipped: list = field(default_factory=list)
    combos_done: int = 0                        # số combo đã hoàn thành (đếm một lần mỗi tổ hợp)
    combo_seen: set = field(default_factory=set)
    appraised: dict = field(default_factory=dict)  # nhóm nguồn gốc -> số lần giám định
    unlocked: list = field(default_factory=list)   # Bí Khế đã mở
    applied_tx: set = field(default_factory=set)   # mã giao dịch đã áp dụng
    carry: int = 0                              # kỳ nợ đã hoãn (GT1) phải trả cùng kỳ sau
    postpone_pending: bool = False
    gt1_used: bool = False

    def __post_init__(self):
        self.rng = random.Random(self.seed)
        self.rng_origin = random.Random(self.seed + 1)  # tách riêng để không đổi chuỗi xác suất rơi

    # --- tiện ích ---
    def _id(self):
        i = self.next_id
        self.next_id += 1
        return i

    def snapshot(self):
        return (self.cash, self.day, self.bought_today, dict(self.boxes),
                {k: dict(v) for k, v in self.items.items()}, list(self.paid),
                self.status, self.arrears, list(self.equipped), list(self.unlocked))

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
        # Kết quả xác định ngay khi mua để tải lại không đổi kết quả (chống reroll).
        x, acc, value = self.rng.random(), 0.0, LOOT[kind][-1][1]
        for p, v in LOOT[kind]:
            acc += p
            if x <= acc:
                value = v
                break
        fake = self.rng.random() < FAKE_RATE
        self.boxes[bid] = {"kind": kind, "value": value, "fake": fake,
                           "origin": ORIGINS[self.rng_origin.randrange(len(ORIGINS))]}
        return bid

    def open_box(self, box_id):
        self._check_playing()
        if box_id not in self.boxes:
            raise Rejected("hộp không tồn tại hoặc đã mở")
        b = self.boxes.pop(box_id)
        iid = self._id()
        self.items[iid] = {"value": b["value"], "fake": b["fake"],
                           "origin": b["origin"], "appraised": False}
        return iid

    def appraise(self, item_id):
        self._check_playing()
        it = self.items.get(item_id)
        if it is None:
            raise Rejected("hiện vật không tồn tại")
        if it["appraised"]:
            raise Rejected("đã giám định")
        it["appraised"] = True
        self.appraised[it["origin"]] = self.appraised.get(it["origin"], 0) + 1
        self._update_bikhe()
        return it["fake"]

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

    def penalty_rate(self):
        return PENALTY_RATE

    def use_gt1(self):
        """GT1 Sổ Ghi Nợ Cũ (đề xuất Claude 1): hoãn KỲ CỦA NGÀY NÀY một lần mỗi ván.
        Kỳ hoãn thành kỳ x 1,15 (làm tròn lên), trả cùng kỳ ngày sau, không bị phạt 20%,
        không tính vào ngưỡng thua, không hoãn nợ tồn, không hoãn kỳ 7."""
        self._check_playing()
        if self.gt1_used:
            raise Rejected("GT1 đã dùng")
        if self.day >= len(DEBT):
            raise Rejected("không hoãn kỳ 7")
        self.gt1_used = True
        self.postpone_pending = True

    def end_day(self):
        self._check_playing()
        D = DEBT[self.day - 1]
        post, self.postpone_pending = self.postpone_pending, False
        due_total = (0 if post else D) + self.arrears + self.carry
        self.carry = 0
        pay = min(max(self.cash - KEEP_MIN_CASH, 0), due_total)
        self.cash -= pay
        self.arrears = due_total - pay
        if self.arrears > 0:
            self.arrears += math.ceil(self.arrears * self.penalty_rate())
        elif not post:
            self.paid.append(self.day)
        if post:
            self.carry = math.ceil(D * 1.15)
        if self.arrears > LOSE_ARREARS_OVER:
            self.status, self.lose_reason = "lost", "arrears_over_limit"
            return {"result": "lost", "arrears": self.arrears}
        if self.day == len(DEBT):
            if self.arrears == 0 and self.carry == 0:
                self.status = "won"
                return {"result": "won", "cash": self.cash}
            self.status, self.lose_reason = "lost", "arrears_on_day7"
            return {"result": "lost", "arrears": self.arrears}
        self.day += 1
        self.bought_today = 0
        return {"result": "next_day", "day": self.day, "arrears": self.arrears}

    # --- tạo tác, combo, Bí Khế ---
    def grant_artifact(self, aid):
        # [MỞ] cách có tạo tác (mua, rơi) chưa chốt. Hàm này chỉ cấp để kiểm thử.
        if aid not in ARTIFACTS or aid in self.owned:
            raise Rejected("tạo tác không hợp lệ hoặc đã có")
        self.owned.append(aid)

    def equip(self, aid):
        self._check_playing()
        if aid not in self.owned:
            raise Rejected("chưa sở hữu")
        if aid in self.equipped:
            raise Rejected("đã trang bị")
        if len(self.equipped) >= EQUIP_SLOTS:
            raise Rejected("hết ô trang bị")
        self.equipped.append(aid)
        self._update_bikhe()

    def unequip(self, aid):
        self._check_playing()
        if aid not in self.equipped:
            raise Rejected("chưa trang bị")
        self.equipped.remove(aid)

    def active_combos(self):
        counts = {}
        for a in self.equipped:
            he = ARTIFACTS[a][0]
            counts[he] = counts.get(he, 0) + 1
        out = []
        for he, n in sorted(counts.items()):
            if n >= COMBO_BIG:
                out.append((he, "lon"))
            elif n >= COMBO_SMALL:
                out.append((he, "nho"))
        return out

    def _update_bikhe(self):
        for c in self.active_combos():
            self.combo_seen.add(c)
        self.combos_done = len(self.combo_seen)
        for bid, (kind, arg, goal) in BI_KHE.items():
            if bid in self.unlocked:
                continue
            if self.progress(bid)[0] >= goal:
                self.unlocked.append(bid)

    def progress(self, bid):
        kind, arg, goal = BI_KHE[bid]
        if kind == "combo":
            return (self.combos_done, goal)
        if kind == "appraise_origin":
            return (self.appraised.get(arg, 0), goal)

    # --- lưu và tải ---
    def apply(self, tx, fn, *args):
        """Chạy một hành động với mã giao dịch để chặn lặp lại sau tải ván."""
        if tx in self.applied_tx:
            raise Rejected("giao dịch đã áp dụng")
        r = fn(*args)
        self.applied_tx.add(tx)
        return r

    def save(self):
        d = {k: v for k, v in self.__dict__.items() if k not in ("rng", "rng_origin")}
        d["rng_origin_state"] = self.rng_origin.getstate()
        d["combo_seen"] = sorted(map(list, self.combo_seen))
        d["applied_tx"] = sorted(self.applied_tx)
        d["rng_state"] = self.rng.getstate()
        return json.dumps(d, default=list, sort_keys=True)

    @classmethod
    def load(cls, blob):
        d = json.loads(blob)
        st = d.pop("rng_state")
        so = d.pop("rng_origin_state")
        d["boxes"] = {int(k): v for k, v in d["boxes"].items()}
        d["items"] = {int(k): v for k, v in d["items"].items()}
        d["combo_seen"] = {tuple(x) for x in d["combo_seen"]}
        d["applied_tx"] = set(d["applied_tx"])
        g = cls()
        g.__dict__.update(d)
        g.rng.setstate((st[0], tuple(st[1]), st[2]))
        g.rng_origin.setstate((so[0], tuple(so[1]), so[2]))
        return g

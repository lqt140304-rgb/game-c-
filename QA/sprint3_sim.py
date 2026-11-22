"""Sprint 3: so sánh ba chiến thuật trên seed tái lập, dùng nội dung của Claude 1.

Nguồn nội dung: Docs/Content/NoiDungMau.md (commit d08130a) và
Docs/Content/GiaTruyenTaoTacBiKhe.md (commit d69864d) trên nhánh claude/relaxed-cerf-wgnu7l.
Luật nợ: đề xuất của Claude 1 (commit 643ae03). Phần chưa có luật ghi rõ là GIẢ ĐỊNH bên dưới.

GIẢ ĐỊNH của Claude 2 (chưa chốt):
- Chất lượng: Nguyên 50% (x1,0), Xước 35% (x0,7), Hỏng 15% (x0,3).
- Hàng giả 15% (riêng V09 Lá bùa 35%), bán còn 20%.
- Giá bán không phụ thuộc đã giám định hay chưa (giám định chỉ làm lộ thông tin).
- Phí giám định 15% giá nền, tối thiểu 20 xu. Phí phục chế 25% giá nền, đưa Hỏng lên Xước.
- Hiệu ứng thông tin (manh mối, dấu hiệu soi) không có giá trị tiền trong mô phỏng.
- Người chơi bán ngay mọi hiện vật (trừ khi phục chế hoặc giám định trước khi bán).
"""
import random
from economy_model import Game, DEBT, Rejected

QK, HD, TL, NHT, CT, TT = "Quá khứ", "Hiện đại", "Tương lai", "Ngoài hành tinh", "Cổ tích", "Thần thoại"
ITEMS = {  # mã: (nguồn gốc, giá nền)
    "V01": (QK, 300), "V02": (QK, 500), "V03": (QK, 650), "V04": (HD, 200), "V05": (TL, 150),
    "V06": (TL, 900), "V07": (NHT, 1200), "V08": (CT, 1000), "V09": (TT, 400), "V10": (TT, 1800),
}
BOXES = {  # mã: (giá, {hiện vật: trọng số})
    "H1": (100, {"V01": 40, "V02": 35, "V03": 25}),
    "H2": (350, {"V04": 30, "V06": 30, "V05": 40}),
    "H3": (800, {"V08": 40, "V09": 40, "V10": 20}),
    "RD": (1500, {"V07": 50, "V10": 30, "V08": 20}),
}
QUALITY = [("Nguyên", .50, 1.0), ("Xước", .35, .7), ("Hỏng", .15, .3)]
FAKE_P, FAKE_P_V09, FAKE_X = .15, .35, .2
APPR_RATE, APPR_MIN, RESTORE_RATE = .15, 20, .25
ART = {  # mã: (hệ, giá)
    "CP1": ("CP", 150), "CP2": ("CP", 400), "CP3": ("CP", 900),
    "TC1": ("TC", 120), "TC2": ("TC", 450), "TC3": ("TC", 1000),
}


RD_FROM_DAY = 4  # ngày Rương Đen xuất hiện (M3 cho phép từ ngày 1)
ALL_ORIGIN_BONUS = False  # phương án: hiệu ứng CP1/CB1 áp dụng mọi hiện vật
BONUS_SCALE = 1.0   # nhân các hiệu ứng tăng giá bán (thử phương án)
ART_PRICE_SCALE = 1.0


class Content:
    """Bảng giá trị có thể thay để thử phương án."""
    def __init__(self, item_mult=None, box_price=None, quality=None):
        self.items = {k: (o, round(b * (item_mult or {}).get(k, 1))) for k, (o, b) in ITEMS.items()}
        self.box_price = {k: (box_price or {}).get(k, v[0]) for k, v in BOXES.items()}
        self.quality = quality or QUALITY


class S3(Game):
    run = None

    def penalty_rate(self):
        return self.run.penalty_rate()


class Run:
    def __init__(self, seed, content, strategy, gt=None):
        self.rng = random.Random(seed)
        self.c = content
        self.strategy = strategy
        self.g = S3(seed=seed)
        self.g.run = self
        self.gt = self.rng.choice(["GT1", "GT2"])
        if gt:
            self.gt = gt
        self.bonus_rev = 0         # doanh thu thêm nhờ hiệu ứng bán giá
        self.owned, self.equipped = [], []
        self.bk1 = self.bk2 = False
        self.bk2_count = 0
        self.free_appraisal_used = False
        self.seen = set()          # combo đã từng kích hoạt
        self.art_spent = 0
        self.shortfall_days = 0
        self.lose_day = None

    # --- hiệu ứng ---
    def slots(self):
        return 4 if self.bk1 else 3

    def has(self, *a):
        return all(x in self.equipped for x in a)

    def n_he(self, he):
        return sum(1 for a in self.equipped if ART[a][0] == he)

    def penalty_rate(self):
        if self.gt == "GT1" or self.has("TC3") or self.n_he("TC") >= 3:
            return .10
        return .20

    def combo_active(self):
        out = set()
        if self.has("CP1", "CP2"): out.add("CB1")
        if self.has("TC1", "TC2"): out.add("CB2")
        if self.has("CP3", "TC3"): out.add("CB3")
        return out

    def refresh(self):
        act = self.combo_active()
        self.seen |= act
        if not self.bk1 and self.g.day < 5 and ({"CB1", "CB2"} & act):
            self.bk1 = True

    def appraise_fee(self, base):
        fee = max(APPR_MIN, round(base * APPR_RATE))
        if self.bk2:
            return round(fee * .5)
        disc = .10 if (self.has("CP2") or self.n_he("CP") >= 2) else 0
        return round(fee * (1 - disc))

    def restore_fee(self, base):
        disc = .10 if (self.has("TC1") or self.n_he("TC") >= 2) else 0
        return round(base * RESTORE_RATE * (1 - disc))

    # --- thao tác ---
    def buy_artifact(self, aid):
        price = round(ART[aid][1] * ART_PRICE_SCALE)
        if aid in self.owned or self.g.cash < price or len(self.equipped) >= self.slots():
            return False
        self.g.cash -= price
        self.art_spent += price
        self.owned.append(aid)
        self.equipped.append(aid)
        self.refresh()
        return True

    def buy_open(self, box):
        g = self.g
        price = self.c.box_price[box]
        if g.bought_today >= 3 or g.cash < price:
            return None
        g.cash -= price
        g.bought_today += 1
        w = BOXES[box][1]
        iid = self.rng.choices(list(w), weights=list(w.values()))[0]
        r, acc, qf, ql = self.rng.random(), 0, 1.0, "Nguyên"
        for name, p, f in self.c.quality:
            acc += p
            if r <= acc:
                qf, ql = f, name
                break
        fake = self.rng.random() < (FAKE_P_V09 if iid == "V09" else FAKE_P)
        return {"id": iid, "q": qf, "ql": ql, "fake": fake, "appraised": False}

    def appraise(self, it):
        base = self.c.items[it["id"]][1]
        if not self.free_appraisal_used and self.has("CP3", "TC3"):
            self.free_appraisal_used = True       # CB3
        else:
            fee = self.appraise_fee(base)
            if self.g.cash < fee:
                return False
            self.g.cash -= fee
        it["appraised"] = True
        if self.c.items[it["id"]][0] == TT:
            self.bk2_count += 1
            if self.bk2_count >= 2:
                self.bk2 = True
        return True

    def restore(self, it):
        base = self.c.items[it["id"]][1]
        fee = self.restore_fee(base)
        if it["ql"] != "Hỏng" or self.g.cash < fee:
            return False
        self.g.cash -= fee
        it["q"], it["ql"] = .7, "Xước"
        return True

    def sell(self, it):
        origin, base = self.c.items[it["id"]]
        p0 = base * it["q"] * (FAKE_X if it["fake"] else 1)
        p = p0
        if origin == QK or ALL_ORIGIN_BONUS:
            if self.has("CP1"): p *= 1 + .05 * BONUS_SCALE
            if self.has("CP1", "CP2"): p *= 1 + .10 * BONUS_SCALE
        if origin == TT and self.has("CP3"):
            p *= 1 + .10 * BONUS_SCALE
        self.bonus_rev += round(p) - round(p0)
        self.g.cash += round(p)


# --- chiến thuật ---
def reserve(run):
    return DEBT[run.g.day - 1] + run.g.arrears


def handle(run, it, reserve_cash, appraise_tt):
    if it["ql"] == "Hỏng" and run.g.cash - run.restore_fee(run.c.items[it["id"]][1]) >= reserve_cash:
        run.restore(it)
    if appraise_tt and not run.bk2 and run.c.items[it["id"]][0] == TT:
        free = not run.free_appraisal_used and run.has("CP3", "TC3")
        if free or run.g.cash - run.appraise_fee(run.c.items[it["id"]][1]) >= reserve_cash:
            run.appraise(it)
    run.sell(it)


def ladder(run, keep_reserve, appraise_tt):
    """Mỗi lượt mua hộp đắt nhất còn mua được. keep_reserve: chừa tiền cho kỳ nợ trong ngày."""
    g = run.g
    for _ in range(3):
        r = reserve(run) if keep_reserve else 0
        if g.cash < r:  # đã không đủ trả kỳ này, không còn gì để bảo vệ: dồn tiền gỡ lại
            r = 0
        order = (("RD",) if g.day >= RD_FROM_DAY else ()) + ("H3", "H2", "H1")
        for box in order:
            if g.cash - run.c.box_price[box] >= r:
                it = run.buy_open(box)
                if it:
                    handle(run, it, r, appraise_tt)
                break


def careful(run):
    ladder(run, True, False)


def combo_hunter(run):
    g = run.g
    if g.day <= 5:
        for aid in ("CP1", "CP2", "TC3", "CP3"):
            if aid in run.owned:
                continue
            if g.cash - round(ART[aid][1] * ART_PRICE_SCALE) >= reserve(run) + 100:
                run.buy_artifact(aid)
            break
    ladder(run, True, True)


def bold(run):
    ladder(run, False, False)


STRATS = {"thận trọng": careful, "săn combo": combo_hunter, "liều": bold}


def play(seed, content, strategy, gt=None):
    run = Run(seed, content, strategy, gt)
    g = run.g
    while g.status == "playing":
        strategy(run)
        run.refresh()
        g.end_day()
        if g.arrears > 0:
            run.shortfall_days += 1
    if g.status == "lost":
        run.lose_day = g.day
    return run


def report(content, n=5000, label=""):
    rows = []
    for name, st in STRATS.items():
        w = sf = 0
        cb = {"CB1": 0, "CB2": 0, "CB3": 0}
        bk1 = bk2 = 0
        spent = 0
        lose_days = []
        sfd = 0
        for s in range(n):
            r = play(s, content, st)
            w += r.g.status == "won"
            sf += r.shortfall_days > 0
            sfd += r.shortfall_days
            for k in cb: cb[k] += k in r.seen
            bk1 += r.bk1; bk2 += r.bk2
            spent += r.art_spent
            if r.g.status == "lost": lose_days.append(r.lose_day)
        f = lambda x: round(100 * x / n, 1)
        avg_lose = round(sum(lose_days) / len(lose_days), 1) if lose_days else None
        rows.append((name, f(w), f(sf), round(sfd / n, 2), avg_lose, f(cb["CB1"]), f(cb["CB2"]), f(cb["CB3"]), f(bk1), f(bk2), round(spent / n)))
    print(label)
    print("%-11s %6s %9s %10s %9s %5s %5s %5s %5s %5s %9s" % ("chiến thuật", "thắng%", "từng hụt%", "ngày hụt/ván", "thua TB", "CB1%", "CB2%", "CB3%", "BK1%", "BK2%", "chi tạo tác"))
    for r in rows:
        print("%-11s %6s %9s %10s %9s %5s %5s %5s %5s %5s %9s" % r)
    return rows


if __name__ == "__main__":
    report(Content(), label="Nội dung gốc của Claude 1")

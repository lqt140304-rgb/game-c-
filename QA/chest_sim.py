"""Sprint 5 phần 1: cân bằng 4 rương cơ bản (Gỗ, Vải Niêm, Kim Khí, Đen), 5 bậc, rác, 4 ô tạo tác (Claude 2).

Cách làm: GIỮ NGUYÊN lõi mô phỏng đã được chủ dự án duyệt (economy_model, proposal_sim, clue_sim, swing_sim: nợ, phạt 20%,
giá hộp dao động ±20%, người chơi đọc manh mối, giám định, combo) và chỉ THAY BẢNG DỮ LIỆU + vài luật mới bằng cách nạp đè:
  - bảng hiện vật (ITEMS) và bảng rương (BOXES) theo luật quay hai bước: quay bậc rồi quay món trong bộ sưu tập của rương
    (ruong-chu-de, 'Cơ chế mở rương'): trọng số một món = tỉ lệ bậc / số món của bậc đó trong rương;
  - rác dùng chung (9 món), bán vài đơn vị, không giả, không giám định;
  - Kim Khí ra tạo tác: tạo tác chưa có thì mang vào ô trống, trùng hoặc hết ô thì đổi tiền theo DUP_FRAC (GIẢ ĐỊNH, spec chưa chốt);
  - 4 ô tạo tác mặc định, BK1 mở ô thứ 5;
  - hộp ngày 1 sắp sẵn (mo-man): hộp đầu ra rác, hộp hai ra đồ Thường thật bán được.
Đơn vị cân bằng 10.000 đ (QA/currency.py đổi sang đồng khi in). Mã khóa nội bộ H1..RD xếp theo giá tăng dần
(H1 = rương rẻ nhất, RD = đắt nhất); tên thật ở CHESTS.

Dùng: import chest_sim as CS; CS.apply(CS.DEFAULT); CS.report(...)
"""
import copy
import random
import clue_sim as C
import proposal_sim as P
import risk_penalty as R
import risk_full as F
import sprint3_sim as sm
import swing_sim as S
from sprint3_sim import Content, ITEMS, BOXES, ART, QK, HD, TL, NHT, CT, TT

TIER_NAMES = ["R", "T", "H", "X", "L"]   # Rác, Thường, Hiếm, Rất hiếm, Huyền thoại
JUNK_BASE = 3                              # rác bán vài đơn vị (30.000 đ)
JUNK = {  # mã: (nhóm, tên). 9 món rác dùng chung (ruong-chu-de)
    "R01": (QK, "Giày cũ"), "R02": (HD, "Tất thối"), "R03": (HD, "Trứng thối"), "R04": (QK, "Xương cá"),
    "R05": (HD, "Dép tổ ong"), "R06": (HD, "Lon bẹp"), "R07": (HD, "Áo đấu cũ"), "R08": (HD, "Bao thuốc rồng"),
    "R09": (HD, "Vé cược nhàu"),
}
# Hiện vật: mã -> (nhóm, giá nền, tên). V = đã có (giá X110 đang dùng). N = món mới, GIÁ NỀN LÀ ĐỀ XUẤT của Claude 2 (xem báo cáo).
NEW_ITEMS = {
    "V01": (QK, 230, "Bình sứ men xanh"), "V02": (QK, 385, "Sách cổ bìa da"), "V03": (QK, 495, "Đồng hồ quả lắc cũ"),
    "V04": (HD, 415, "Mảnh kính thiên văn"), "V05": (TL, 310, "Thiết bị liên lạc hỏng"), "V06": (TL, 1845, "Bộ pin lượng tử"),
    "V07": (NHT, 2695, "Tinh thể lam"), "V09": (TT, 895, "Lá bùa giấy vàng"), "V10": (TT, 2860, "Mảnh vảy long"),
    "N01": (QK, 1550, "Ấm trà đất nứt"), "N02": (QK, 1750, "Hộp nữ trang gỗ khảm"), "N03": (TT, 2700, "Tượng Di Lặc gỗ"),
    "N04": (QK, 1700, "Tranh cuộn thủy mặc"), "N05": (QK, 1600, "Con dấu đồng"),
    "N06": (CT, 1900, "Máy ảnh phim cổ"), "N07": (HD, 350, "Radio bóng đèn"),
    "N08": (HD, 260, "Xúc xắc chì"), "N09": (QK, 220, "Bộ bài cũ"),
    "N10": (TL, 1650, "Bật lửa mạ vàng"), "N11": (HD, 1500, "Đồng hồ xách tay"), "N12": (CT, 3000, "Nhẫn kim cương không rõ nguồn"),
    # Huyền thoại cơ bản (rẻ hơn Huyền thoại chủ đề, ruong-chu-de mục 10)
    "L04": (HD, 4000, "Vé số độc đắc"), "N13": (QK, 5000, "Bản đồ cổ vẽ tay"),
    "N14": (TL, 5000, "Đồng hồ bỏ túi không hết pin"), "N15": (NHT, 7000, "Két sắt chưa ai mở được"),
}
ART_IDS = ("CP1", "TC1", "CP2", "TC2", "CP3", "TC3")
ART_TIER = {"CP1": 1, "TC1": 1, "CP2": 2, "TC2": 2, "CP3": 3, "TC3": 3}

CHESTS = {  # tên rương -> mô tả. tiers = tỉ lệ bậc (R,T,H,X,L). pools = bộ sưu tập theo bậc.
    "Gỗ": dict(price=120, fake=.10, ntrue=2, tiers=dict(R=.20, T=.70, H=.10, X=0, L=0),
               pools=dict(T=["V01", "V03", "V09"], H=["N01", "N02"], X=["N03"], L=["L04"])),
    "Vải Niêm": dict(price=350, fake=.15, ntrue=2, tiers=dict(R=.15, T=.55, H=.25, X=.05, L=0),
                     pools=dict(T=["V02", "V04"], H=["N04", "N05"], X=["V10"], L=["N13"])),
    "Kim Khí": dict(price=500, fake=0.0, ntrue=1, tiers=dict(R=.10, T=0, H=0, X=0, L=0), art_mix=(.60, .35, .05),
                    pools=dict(T=["V05", "N07"], H=["V06", "N06"], X=["V07"], L=["N14"])),
    "Đen": dict(price=1200, fake=.30, ntrue=1, tiers=dict(R=.30, T=.20, H=.25, X=.25, L=0),
                pools=dict(T=["N08", "N09"], H=["N10", "N11"], X=["N12"], L=["N15"])),
}
DEFAULT = dict(chests=CHESTS, slots=4, dup_frac=.5, art_value=1.0, script=True, rd_from_day=4, amp=.20)
# Bảng 4 rương cơ bản CHỦ DỰ ÁN ĐÃ DUYỆT 2026-11-24 (Docs/Balance/Sprint5Ruong.md): Đen giữ 12.000.000 đ,
# Gỗ có thêm Rất hiếm nhỏ rồi cân bằng lại, rác Vải Niêm lên 30%, rác Đen lên 30% và Rất hiếm Đen tăng nhẹ.
FINAL_CHESTS = {
    "Gỗ": dict(price=140, fake=.10, ntrue=2, tiers=dict(R=.18, T=.795, H=.01, X=.01, L=.005), pools=CHESTS["Gỗ"]["pools"]),
    "Vải Niêm": dict(price=350, fake=.15, ntrue=2, tiers=dict(R=.30, T=.39, H=.25, X=.05, L=.01), pools=CHESTS["Vải Niêm"]["pools"]),
    "Kim Khí": dict(price=450, fake=0.0, ntrue=1, tiers=dict(R=.10, T=.25, H=.22, X=.08, L=.01), art_mix=(.60, .35, .05),
                    pools=CHESTS["Kim Khí"]["pools"]),   # 34% còn lại là tạo tác
    "Đen": dict(price=1200, fake=.30, ntrue=1, tiers=dict(R=.30, T=.03, H=.04, X=.58, L=.05), pools=CHESTS["Đen"]["pools"]),
}
FINAL = dict(DEFAULT, chests=FINAL_CHESTS, script=False)
KEYS = ("H1", "H2", "H3", "RD")
STATE = {}
# Ảnh chụp trạng thái lõi cũ lúc nạp module, để restore() trả lại (các test khác dùng chung biến toàn cục).
_SNAP = dict(ITEMS=dict(ITEMS), BOXES=copy.deepcopy(BOXES), ART=dict(ART), NTRUE=dict(C.NTRUE), SW=copy.deepcopy(S.SW),
             RD_FROM_DAY=sm.RD_FROM_DAY, CFG=dict(S.E.CFG), PKG={k: getattr(P, k) for k in F.PKG},
             fns=[(R, "fake_rate", R.fake_rate), (C, "fake_rate", C.fake_rate), (P.Run5, "buy_open", P.Run5.__dict__.get("buy_open")),
                  (P.Run5, "p_real", P.Run5.__dict__.get("p_real")), (P.Run5, "sell", P.Run5.__dict__.get("sell")),
                  (sm.Run, "slots", sm.Run.__dict__.get("slots")), (C, "make_candidate", C.make_candidate), (P, "handle5", P.handle5)])


def restore():
    """Trả lõi mô phỏng về trạng thái trước apply()."""
    for name, tgt in (("ITEMS", ITEMS), ("BOXES", BOXES), ("ART", ART), ("NTRUE", C.NTRUE), ("SW", S.SW), ("CFG", S.E.CFG)):
        tgt.clear()
        tgt.update(copy.deepcopy(_SNAP[name]))
    sm.RD_FROM_DAY = _SNAP["RD_FROM_DAY"]
    for k, v in _SNAP["PKG"].items():
        setattr(P, k, v)
    for obj, attr, val in _SNAP["fns"]:
        if val is None:
            if attr in obj.__dict__:
                delattr(obj, attr)
        else:
            setattr(obj, attr, val)
    STATE.clear()


def key_of(design):
    """Mã nội bộ theo giá tăng dần."""
    order = sorted(design["chests"], key=lambda n: design["chests"][n]["price"])
    return dict(zip(order, KEYS)), dict(zip(KEYS, order))


def weights(chest, art_value=1.0):
    """Trọng số từng món theo luật hai bước. Trả (trọng số, ghi chú bậc)."""
    w, tiers = {}, dict(chest["tiers"])
    junk_p = tiers.pop("R", 0)
    for j in JUNK:
        w[j] = junk_p / len(JUNK)
    for t, p in tiers.items():
        pool = chest["pools"].get(t, [])
        for i in pool:
            w[i] = w.get(i, 0) + p / len(pool)
    if chest.get("art_mix"):
        share = 1 - sum(chest["tiers"].values())
        for a in ART_IDS:
            tier = ART_TIER[a]
            w["AX_" + a] = share * chest["art_mix"][tier - 1] / 2
    return {k: v * 1000 for k, v in w.items() if v > 0}


def apply(design):
    """Nạp bảng dữ liệu mới vào lõi mô phỏng cũ. Gọi trước khi đo."""
    STATE.clear()
    STATE.update(design=design)
    key, name = key_of(design)
    STATE["key"], STATE["name"] = key, name
    ART["CP3"], ART["TC3"] = ("CP", 500), ("TC", 500)
    ITEMS.clear()
    qm = sum(p * f for _, p, f in sm.QUALITY)
    nm = design.get("new_mult", 1.0)   # độ nhạy: nhân giá nền các món mới (mã N.., L04), món đã có giữ nguyên
    for i, (o, b, _) in NEW_ITEMS.items():
        ITEMS[i] = (o, round(b * nm) if i[0] in "NL" else b)
    for i, (o, _) in JUNK.items():
        ITEMS[i] = (o, JUNK_BASE)
    for a in ART_IDS:
        ITEMS["AX_" + a] = (HD, round(design["art_value"] * sm.ART[a][1] / qm))   # trừ phần chất lượng nhân sau
    BOXES.clear()
    for n, ch in design["chests"].items():
        BOXES[key[n]] = (ch["price"], weights(ch, design["art_value"]))
    C.NTRUE.clear()
    C.NTRUE.update({key[n]: ch["ntrue"] for n, ch in design["chests"].items()})
    STATE["fake"] = {key[n]: ch["fake"] for n, ch in design["chests"].items()}
    S.SW["amp"] = design["amp"]
    S.SW["base"] = {key[n]: ch["price"] for n, ch in design["chests"].items()}
    sm.RD_FROM_DAY = design["rd_from_day"]
    S.E.CFG.update(on=False)
    for k, v in F.PKG.items():
        setattr(P, k, v)
    ART["CP3"], ART["TC3"] = ("CP", 500), ("TC", 500)
    R.fake_rate = fake_rate
    C.fake_rate = fake_rate
    P.Run5.buy_open = buy_open
    P.Run5.p_real = p_real
    P.Run5.sell = sell
    sm.Run.slots = slots
    C.make_candidate = make_candidate
    P.handle5 = handle5
    return Content(box_price={k: v[0] for k, v in BOXES.items()})


# --- luật nạp đè ---------------------------------------------------------------------------------------
def fake_rate(box, iid):
    if iid in JUNK or iid.startswith("AX_"):
        return 0.0
    r = STATE["fake"][box]
    return max(r, sm.FAKE_P_V09) if iid == "V09" else r


def p_real(self, it):
    return 1 - fake_rate(it.get("box", "H1"), it["id"])


def slots(self):
    n = STATE["design"]["slots"]
    return n + 1 if self.bk1 else n


def _scripted(run, count):
    """Trả mã món sắp sẵn của hộp ngày 1 (count = số hộp đã mua trước đó trong ngày), hoặc None.
    Chế độ design['script']: False = không sắp sẵn; 'any' = hai hộp đầu sắp sẵn, trả giá rương đã chọn;
    'free1' = hộp đầu tặng (hoàn tiền), hộp hai trả giá; 'cheap' = hai hộp đầu chỉ tốn bằng giá rương rẻ nhất (cửa hàng ngày 1 chỉ bán Gỗ)."""
    if not STATE["design"]["script"] or run.g.day != 1 or count > 1:
        return None
    return "junk" if count == 0 else "common"


def script_refund(run, it):
    """Số tiền hoàn lại cho hộp sắp sẵn theo chế độ."""
    mode = STATE["design"]["script"]
    sc = it.get("script")
    if not sc or mode in (False, "any"):
        return 0
    price = run.c.box_price[it["box"]]
    if mode == "free1":
        return price if sc == "junk" else 0
    cheapest = min(run.c.box_price.values())
    return max(0, price - cheapest)


def _draw(run, box, count):
    w = BOXES[box][1]
    sc = _scripted(run, count)
    if sc == "junk":
        iid = run.rng.choice(list(JUNK))
    elif sc == "common":
        pool = STATE["design"]["chests"][STATE["name"][box]]["pools"]["T"]
        iid = run.rng.choice(pool)
    else:
        iid = run.rng.choices(list(w), weights=list(w.values()))[0]
    r, acc, qf, ql = run.rng.random(), 0, 1.0, "Nguyên"
    for name, p, f in run.c.quality:
        acc += p
        if r <= acc:
            qf, ql = f, name
            break
    if sc == "common" and ql == "Hỏng":
        qf, ql = .7, "Xước"
    fake = (run.rng.random() < fake_rate(box, iid)) and sc != "common"
    return iid, qf, ql, fake, sc


def buy_open(self, box):
    g = self.g
    price = self.c.box_price[box]
    if g.bought_today >= 3 or g.cash < price:
        return None
    count = g.bought_today
    g.cash -= price
    g.bought_today += 1
    iid, qf, ql, fake, sc = _draw(self, box, count)
    return {"id": iid, "q": qf, "ql": ql, "fake": fake, "appraised": False, "box": box, "script": sc}


def make_candidate(run, box):
    iid, qf, ql, fake, sc = _draw(run, box, run.g.bought_today)
    origin = run.c.items[iid][0]
    wrong = run.rng.choice([g for g in C.GROUPS if g != origin])
    nt = C.n_true(box)
    clues = [origin] * nt + [wrong] * (3 - nt)
    if nt == 1:
        clues[2] = run.rng.choice([g for g in C.GROUPS if g != origin])
    run.rng.shuffle(clues)
    return {"id": iid, "q": qf, "ql": ql, "fake": fake, "appraised": False, "box": box, "clues": clues, "seen": 1, "script": sc}


def sell(self, it):
    iid = it["id"]
    if iid.startswith("AX_"):
        aid = iid[3:]
        if aid not in self.owned and len(self.equipped) < self.slots():
            self.owned.append(aid)
            self.equipped.append(aid)
            self.refresh()
            self.stats["art_from_chest"] = self.stats.get("art_from_chest", 0) + 1
        else:
            self.g.cash += round(STATE["design"]["dup_frac"] * sm.ART[aid][1])
            self.stats["art_dup"] = self.stats.get("art_dup", 0) + 1
        return
    self.g.cash += self.price(it)


_orig_handle5 = P.handle5


def handle5(run, it, r, smart):
    run.g.cash += script_refund(run, it)
    if it["id"] in JUNK or it["id"].startswith("AX_"):
        run.sell(it)
        return
    _orig_handle5(run, it, r, smart)


# --- đo ------------------------------------------------------------------------------------------------
def box_table(design=None):
    """Lời kỳ vọng (đơn vị cân bằng) mỗi rương, chưa tính dao động giá và tạo tác."""
    design = design or STATE["design"]
    qm = sum(p * f for _, p, f in sm.QUALITY)
    out = {}
    for n, ch in design["chests"].items():
        k = STATE["key"][n]
        price, w = BOXES[k]
        tot = sum(w.values())
        ev = 0.0
        for i, x in w.items():
            f = fake_rate(k, i)
            base = ITEMS[i][1]
            ev += x / tot * base * qm * (1 - f + f * sm.FAKE_X)
        out[n] = dict(price=price, ev=ev, profit=ev - price, ratio=ev / price)
    return out


def measure_all(n=1500):
    """Bot thường (mua hộp đắt nhất), người chơi chọn loại, ép từng loại hộp, chọn hộp theo tiền."""
    c = Content(box_price={k: v[0] for k, v in BOXES.items()})
    C.setup_done = True
    res = {}
    lad = P.measure(c, ["thận trọng", "liều", "thận trọng + giám định chọn lọc", "combo lớn (CB1 + CB3)"], n=n)
    res["bot"] = {k: v["win"] for k, v in lad.items()}
    sk = C.measure_turns(c, n, lam=.75, tau=1.0)
    res["skilled"] = {k: v for k, v in sk.items() if not k.endswith("[mua]")}
    return c, res


def mix_of(content, n):
    """Tỉ lệ chọn từng rương của người chơi chọn loại (thận trọng + giám định)."""
    mix = {b: 0 for b in KEYS}
    orig = C.turn_choose

    def spy(run, rc, tiers, _o=orig):
        it = _o(run, rc, tiers)
        if it:
            mix[it["box"]] += 1
        return it
    C.turn_choose = spy
    try:
        for s in range(n):
            P.play5(s, content, C.TURN_NAMES["thận trọng + giám định"])
    finally:
        C.turn_choose = orig
    tot = sum(mix.values()) or 1
    return {STATE["name"][b]: round(100 * mix[b] / tot) for b in KEYS}


def evaluate(design, n=1000, with_mix=True):
    """Gói số đo chính của một cấu hình rương. Trả dict, chạy được trong tiến trình riêng."""
    c = apply(design)
    out = {"table": {k: {a: round(b, 2) for a, b in v.items()} for k, v in box_table().items()}}
    lad = P.measure(c, ["thận trọng", "liều", "thận trọng + giám định chọn lọc", "combo lớn (CB1 + CB3)"], n=n)
    out["bot"] = {k.split(" (")[0]: v["win"] for k, v in lad.items()}
    sk = C.measure_turns(c, n, lam=.75, tau=1.0)
    out["skilled"] = {k.replace(" + giám định", ""): v for k, v in sk.items() if not k.endswith("[mua]")}
    if with_mix:
        out["mix"] = mix_of(c, min(n, 700))
    return out


def day1_stats(content, strat, n=1000):
    """Đo ván đầu: tỉ lệ thua trước ngày 3, nợ tồn cuối ngày 1 (trung bình, xấu nhất), tiền mặt cuối ngày 1 (xấu nhất),
    ngày thua sớm nhất, tỉ lệ thắng. strat là hàm chiến thuật của proposal_sim (STRATS hoặc clue_sim.TURN_NAMES)."""
    lost_early = wins = 0
    arr1, cash1, lose_days = [], [], []
    for s in range(n):
        run = P.Run5(s, content, strat, None, None)
        g = run.g
        while g.status == "playing":
            P.start_hooks(run)
            strat(run)
            run.refresh()
            P.end_hooks(run)
            d = g.day
            g.end_day()
            if g.arrears > 0 or g.carry > 0:
                run.shortfall_days += 1
            if d == 1:
                arr1.append(g.arrears)
                cash1.append(g.cash)
            if g.status == "lost":
                lose_days.append(d)
        wins += g.status == "won"
        lost_early += g.status == "lost" and lose_days and lose_days[-1] <= 2
    return dict(win=round(100 * wins / n, 1), lost_before_day3=round(100 * lost_early / n, 2),
                lost_day1=sum(1 for d in lose_days if d == 1),
                arrears1_mean=round(sum(arr1) / len(arr1), 1), arrears1_max=max(arr1),
                arrears1_pos=round(100 * sum(1 for a in arr1 if a > 0) / len(arr1), 1),
                cash1_min=min(cash1), first_loss_day=min(lose_days) if lose_days else None)

"""[BẢN CHUỖI VÁN, ĐÃ BỎ NGÀY 2026-11-25: cây kỹ năng nay chỉ dùng trong ván, không có tiến trình vĩnh viễn. Giữ lại để tái hiện số đo cũ
và vì tree_run.py dùng chung apply(), UNLOCK_OVER, _Skill, static(). Bảng TREE, POINTS, Meta, chain() KHÔNG còn là đề xuất.]

Sprint 5 phần 4 (Claude 2): điểm phát triển của cây và chuỗi ván của một người chơi.

Cách làm: GIỮ NGUYÊN lõi đã duyệt và các phần trước:
  - 4 rương cơ bản đã chốt (chest_sim.FINAL), rương mở khóa và khung rương chủ đề (chest_unlock, phần 2, đề xuất);
  - soi lỗi và trả giá (inspect_sim, phần 3, đề xuất), hồ sơ soi theo người chơi.
Thêm một vòng ngoài: một người chơi chơi nhiều ván liên tiếp, cuối mỗi ván nhận điểm phát triển (thắng hoặc thua) và điểm
ghi Sổ Sưu Tập lần đầu, rồi mở nút cây giữa hai ván (luật ruong-suu-tap mục 4: cần nút cha, đủ điều kiện sưu tập, tốn điểm).
Nút có tác động lên ván sau:
  - Rương Tổng Hợp, Cổ Phong, Thiên Cơ, Rác Quý (nhánh Rương): có trong shop từ ván sau;
  - M3 Rương Đen từ ngày 1 (tùy chọn, người chơi bật hay không theo POLICY);
  - M1 chọn gia truyền (GT1 hoặc GT2 thay vì rút ngẫu nhiên);
  - M2 vốn đầu ván (+cash_bonus, mặc định 0 = bỏ, xem báo cáo);
  - M4 chỉ là thông tin (Sổ hiện điều kiện Bí Khế): mô phỏng đã giả định người chơi biết luật, nên tác động 0.
Rương chủ đề mở bằng THÀNH TỰU (ruong-chu-de, đã chốt: không tốn điểm), mô hình hóa bằng THEME_RULE (giả định).
Nút Thần lực, Câu chuyện và các nút Sưu tập không có số chỉ đổi nhịp tiêu điểm, không đổi ván.

Đơn vị tiền 10.000 đ (in qua currency.fmt_vnd). Báo cáo: Docs/Balance/Sprint5CayPhatTrien.md.
Dùng: import tree_sim as T; T.static(...) đo một cấu hình; T.chain(...) đo chuỗi ván.
"""
import copy
import random
from collections import defaultdict
import chest_sim as CS
import chest_unlock as U
import clue_sim as C
import inspect_sim as I
import proposal_sim as P
import sprint3_sim as sm

UNLOCK_NAMES = ("Tổng Hợp", "Cổ Phong", "Thiên Cơ", "Rác Quý")
BOT = {"thận trọng": "thận trọng", "liều": "liều"}
SKILLED = ("thận trọng + giám định", "liều + giám định", "combo nhỏ + giám định", "combo lớn + giám định")

# --- ĐỀ XUẤT: điểm phát triển ---------------------------------------------------------------------------
POINTS = dict(
    win=8,            # thắng ván
    loss=2,           # thua ván (luôn có)
    loss_day=1,       # thua: +1 mỗi ngày đã đi qua từ ngày 4 trở đi (đi xa hơn thì nhiều hơn), tối đa loss_day_cap
    loss_day_cap=3,
    book=1,           # mỗi mục mới trong Sổ Sưu Tập (hiện vật, tạo tác, rác: ruong-suu-tap mục 4)
    book_high=2,      # mục mới bậc Rất hiếm hoặc Huyền thoại
    book_cap=None,    # trần điểm Sổ mỗi ván (None = không trần)
)

# --- ĐỀ XUẤT: cây 4 nhánh --------------------------------------------------------------------------------
# cond: (loại, tham số). Loại: book (số mục Sổ), origins (số nhóm nguồn gốc đã có), junk (số loại rác), art (số tạo tác
# một hệ, tham số 'CP:2'), art_any (số tạo tác bất kỳ), wins (số ván thắng), runs (số ván đã chơi), legend (số Huyền thoại đã có), bk (cả hai Bí Khế).
# effect: tác động lên ván (None = không buff chơi).
TREE = [
    # Rương
    dict(id="R1", branch="Rương", parent=None, cost=6, cond=("book", 8), effect=None, name="Kệ rương"),
    dict(id="R2", branch="Rương", parent="R1", cost=8, cond=("junk", 5), effect=("chest", "Rác Quý"), name="Rương Rác Quý"),
    dict(id="R3", branch="Rương", parent="R1", cost=11, cond=("origins", 6), effect=("chest", "Tổng Hợp"), name="Rương Tổng Hợp"),
    dict(id="R4", branch="Rương", parent="R3", cost=13, cond=("art", "CP:2"), effect=("chest", "Cổ Phong"), name="Rương Cổ Phong"),
    dict(id="R5", branch="Rương", parent="R3", cost=13, cond=("art", "TC:2"), effect=("chest", "Thiên Cơ"), name="Rương Thiên Cơ"),
    dict(id="M3", branch="Rương", parent="R1", cost=10, cond=("wins", 1), effect=("rd_early", True), name="Thử thách Rương Đen"),
    # Sưu tập
    dict(id="S1", branch="Sưu tập", parent=None, cost=6, cond=("book", 8), effect=None, name="Trang sưu tập đầu tiên"),
    dict(id="S2", branch="Sưu tập", parent="S1", cost=8, cond=("book", 20), effect=None, name="Sổ tay người săn đồ"),
    dict(id="M1", branch="Sưu tập", parent="S2", cost=10, cond=("book", 25), effect=("gt", True), name="Chọn gia truyền"),
    dict(id="M2", branch="Sưu tập", parent="M1", cost=13, cond=("book", 35), effect=("cash", True), name="Vốn khởi nghiệp nhỏ"),
    dict(id="S3", branch="Sưu tập", parent="S2", cost=11, cond=("legend", 1), effect=None, name="Trang Huyền thoại"),
    dict(id="M4", branch="Sưu tập", parent="S3", cost=13, cond=("art_any", 4), effect=None, name="Bí Khế quen mặt"),
    # Thần lực
    dict(id="T1", branch="Thần lực", parent=None, cost=6, cond=("book", 10), effect=None, name="Tiếng thì thầm"),
    dict(id="T2", branch="Thần lực", parent="T1", cost=8, cond=("origins", 4), effect=None, name="Mèo chợ"),
    dict(id="T3", branch="Thần lực", parent="T2", cost=11, cond=("book", 30), effect=None, name="Chuông chợ"),
    dict(id="T4", branch="Thần lực", parent="T3", cost=14, cond=("legend", 2), effect=None, name="Lời thì thầm Huyền thoại"),
    # Câu chuyện
    dict(id="C1", branch="Câu chuyện", parent=None, cost=6, cond=("runs", 2), effect=None, name="Sổ của Dì Mận"),
    dict(id="C2", branch="Câu chuyện", parent="C1", cost=8, cond=("runs", 5), effect=None, name="Lời nhắn của Anh Bảy"),
    dict(id="C3", branch="Câu chuyện", parent="C2", cost=11, cond=("wins", 1), effect=None, name="Chương mới: phiên chợ đêm"),
    dict(id="C4", branch="Câu chuyện", parent="C3", cost=14, cond=("runs", 12), effect=None, name="Chương mới: món nợ cũ"),
]
CASH_BONUS = 0          # M2 (đơn vị 10.000 đ). Đề xuất 0 (bỏ buff), đo cả 100 = 1.000.000 đ như bản cũ
RD_EARLY_OPT = "best"   # M3 có bật không: "best" bật khi có lợi cho cách chơi đó (đo ở static), True, False
GT_BEST = {k: "GT1" for k in ("thận trọng", "liều") + SKILLED}   # đo ở static: GT1 hơn GT2 0,5 đến 1,6 điểm mọi cách chơi
# Rương chủ đề mở bằng thành tựu (GIẢ ĐỊNH, thành tựu chưa chốt): chủ đề thứ j mở khi Sổ có >= THEME_RULE[j] mục.
THEME_RULE = (15, 30)
SHOP_EXTRA = None       # None: bày mọi rương đã mở; k: mỗi ngày chỉ bày k rương mở khóa / chủ đề (phần 2, câu hỏi 2)

TREE_BY_ID = {n["id"]: n for n in TREE}
HIGH = ("X", "L")


# --- dựng cấu hình một ván -------------------------------------------------------------------------------
# ĐỀ XUẤT sửa bảng phần 2 khi có soi lỗi (phần 3): Tổng Hợp hàng giả 20% -> 10%, Hiếm 28% -> 25%, Rất hiếm 14% -> 13%,
# Thường 39% -> 43% (xem báo cáo mục 3).
UNLOCK_OVER = {"Tổng Hợp": dict(fake=.10, tiers=dict(R=.16, T=.43, H=.25, X=.13, L=.03))}


def scenario(chests=(), n_theme=0, rd_early=False, shop_extra=None):
    extra = {n: dict(U.UNLOCK[n], **UNLOCK_OVER.get(n, {})) for n in UNLOCK_NAMES if n in chests}
    for j in range(n_theme):
        tag = "Z%d" % (j + 1)
        items, pools = U.theme_items(tag, U.THEME.get("leg"), U.THEME.get("group"))
        extra["Chủ đề %d" % (j + 1)] = dict({a: b for a, b in U.THEME.items() if a not in ("leg", "group")}, pools=pools, items=items)
    base = dict(CS.FINAL, rd_from_day=1) if rd_early else CS.FINAL
    return dict(base=base, extra=copy.deepcopy(extra), from_day=1, shop_extra=shop_extra)


_P_ORIG = dict(init=P.Run5.__init__, cert=getattr(P, "CERT_BONUS", 0.0))
RUN = dict(cash=0)


def _init(self, *a, **k):
    _P_ORIG["init"](self, *a, **k)
    self.g.cash += RUN["cash"]
    self.book = set()


def _record(fn):
    def handle(run, it, r, smart):
        run.book.add(it["id"])
        return fn(run, it, r, smart)
    return handle


def apply(chests=(), n_theme=0, rd_early=False, cash=0, profile="thường", shop_extra=None):
    """Nạp 4 rương cơ bản + rương đã mở + soi lỗi (hồ sơ profile). Trả Content."""
    c = U.apply(scenario(chests, n_theme, rd_early, shop_extra))
    C.setup_done = True
    I.CFG["rules"] = dict(I.RULES)
    I.CFG["profile"] = profile
    I.TIER_OF.clear()
    for ch in CS.STATE["design"]["chests"].values():
        for t, pool in ch["pools"].items():
            for i in pool:
                I.TIER_OF[i] = t
    P.CERT_BONUS = 0.0
    P.handle5 = _record(I.handle)
    P.Run5.__init__ = _init
    RUN["cash"] = cash
    return c


def restore():
    U.restore()
    P.Run5.__init__ = _P_ORIG["init"]
    P.CERT_BONUS = _P_ORIG["cert"]
    RUN["cash"] = 0


def strat_of(name):
    return C.TURN_NAMES[name] if name in C.TURN_NAMES else P.STRATS[name]


class _Skill:
    """Người chơi tính toán hoàn hảo dùng C.SKILL lam = 0,75 (như phần 1 đến 3)."""
    def __init__(self, on):
        self.on = on

    def __enter__(self):
        self.old = dict(C.SKILL)
        if self.on:
            C.SKILL.update(lam=.75, tau=1.0, per_box=True)

    def __exit__(self, *a):
        C.SKILL.clear()
        C.SKILL.update(self.old)


def play(seed, content, name, gt=None):
    run = P.play5(seed, content, strat_of(name), gt)
    for a in run.owned:
        run.book.add("AX_" + a)
    return run


def static(name, n=1000, profile=None, gt=None, seed0=0, **cfg):
    """Tỉ lệ thắng (%) của một cách chơi trên một cấu hình cố định."""
    profile = profile or ("giỏi" if name in C.TURN_NAMES else "thường")
    c = apply(profile=profile, **cfg)
    try:
        with _Skill(name in C.TURN_NAMES):
            w = sum(play(seed0 + s, c, name, gt).g.status == "won" for s in range(n))
    finally:
        restore()
    return round(100 * w / n, 1)


# --- tiến trình giữa các ván -----------------------------------------------------------------------------
class Meta:
    def __init__(self):
        self.points = 0
        self.earned = 0
        self.book = set()
        self.unlocked = set()
        self.wins = self.runs = 0
        self.bk = set()
        self.unlock_run = {}     # nút -> ván thứ mấy (1-based) thì mở

    def origins(self, items):
        return {items[i][0] for i in self.book if i in items and not i.startswith("AX_") and i not in CS.JUNK}

    def check(self, cond, items):
        kind, arg = cond
        if kind == "book":
            return len(self.book) >= arg
        if kind == "origins":
            return len(self.origins(items)) >= arg
        if kind == "junk":
            return sum(1 for i in self.book if i in CS.JUNK) >= arg
        if kind == "art":
            he, k = arg.split(":")
            return sum(1 for i in self.book if i.startswith("AX_" + he)) >= int(k)
        if kind == "art_any":
            return sum(1 for i in self.book if i.startswith("AX_")) >= arg
        if kind == "wins":
            return self.wins >= arg
        if kind == "runs":
            return self.runs >= arg
        if kind == "legend":
            return sum(1 for i in self.book if I.TIER_OF.get(i) == "L") >= arg
        if kind == "bk":
            return len(self.bk) >= arg
        raise ValueError(kind)

    def n_theme(self):
        return sum(1 for t in THEME_RULE if len(self.book) >= t)

    def config(self, name):
        chests = tuple(n["effect"][1] for n in TREE if n["id"] in self.unlocked and n["effect"] and n["effect"][0] == "chest")
        rd = "M3" in self.unlocked and (RD_EARLY_OPT is True or (RD_EARLY_OPT == "best" and RD_BEST.get(name, False)))
        cash = CASH_BONUS if "M2" in self.unlocked else 0
        gt = GT_BEST.get(name) if "M1" in self.unlocked else None
        return (tuple(sorted(chests)), self.n_theme(), bool(rd), cash, gt)


RD_BEST = {}   # cách chơi -> có bật M3 hay không. Đo ở static: M3 làm mọi cách chơi thua nhiều hơn, nên không ai bật


def run_points(run, new_items, pts=None):
    pts = pts or POINTS
    g = run.g
    if g.status == "won":
        p = pts["win"]
    else:
        p = pts["loss"] + min(pts["loss_day_cap"], max(0, g.day - 3) * pts["loss_day"])
    b = sum(pts["book_high"] if I.TIER_OF.get(i) in HIGH else pts["book"] for i in new_items)
    if pts["book_cap"] is not None:
        b = min(b, pts["book_cap"])
    return p, b


def spend(m, items, order="cheap"):
    """Mở mọi nút đủ điều kiện mua được. order: 'cheap' (rẻ trước) hoặc 'play' (ưu tiên nút đổi ván, rồi rẻ trước)."""
    while True:
        ok = [n for n in TREE if n["id"] not in m.unlocked and (n["parent"] is None or n["parent"] in m.unlocked)
              and n["cost"] <= m.points and m.check(n["cond"], items)]
        if not ok:
            return
        if order == "play":
            ok.sort(key=lambda n: (n["effect"] is None, n["cost"]))
        else:
            ok.sort(key=lambda n: n["cost"])
        n = ok[0]
        m.points -= n["cost"]
        m.unlocked.add(n["id"])
        m.unlock_run[n["id"]] = m.runs


def chain(name, players=200, runs=30, profile=None, seed0=0, order=None, pts=None):
    """Chuỗi ván: players người chơi, mỗi người runs ván liên tiếp. Trả danh sách bản ghi từng người."""
    profile = profile or ("giỏi" if name in C.TURN_NAMES else "thường")
    order = order or ("play" if name in C.TURN_NAMES else "cheap")
    metas = [Meta() for _ in range(players)]
    hist = [[] for _ in range(players)]
    for k in range(runs):
        groups = defaultdict(list)
        for p, m in enumerate(metas):
            groups[m.config(name)].append(p)
        for (chests, nth, rd, cash, gt), ps in groups.items():
            c = apply(chests, nth, rd, cash, profile, SHOP_EXTRA)
            try:
                with _Skill(name in C.TURN_NAMES):
                    for p in ps:
                        m = metas[p]
                        run = play(seed0 + 1_000_003 * k + 7919 * p + 17, c, name, gt)
                        new = run.book - m.book
                        a, b = run_points(run, new, pts)
                        m.book |= run.book
                        m.wins += run.g.status == "won"
                        m.runs += 1
                        if run.bk1: m.bk.add("BK1")
                        if run.bk2: m.bk.add("BK2")
                        m.points += a + b
                        m.earned += a + b
                        hist[p].append(dict(win=run.g.status == "won", day=run.g.day, pts_run=a, pts_book=b, book=len(m.book),
                                            cfg=(chests, nth, rd, cash, gt)))
                        spend(m, c.items, order)
            finally:
                restore()
    return metas, hist


# --- tổng hợp -------------------------------------------------------------------------------------------
def total_cost():
    return sum(n["cost"] for n in TREE)


def milestones(metas, hist):
    """Số ván (trung vị, 90% người chơi) để mở: nút đầu, nửa cây (theo số nút), cả cây; và từng nút đổi ván."""
    import statistics as st
    N = len(TREE)

    def when(m, k):
        rs = sorted(m.unlock_run.values())
        return rs[k - 1] if len(rs) >= k else None

    out = {}
    for label, k in (("nút đầu", 1), ("nửa cây", (N + 1) // 2), ("cả cây", N)):
        xs = [when(m, k) for m in metas]
        done = [x for x in xs if x is not None]
        out[label] = dict(done=round(100 * len(done) / len(xs)), med=st.median(done) if len(done) * 2 >= len(xs) else None,
                          p90=sorted(done)[int(.9 * len(xs)) - 1] if len(done) >= .9 * len(xs) else None)
    for nid in ("R2", "R3", "R4", "R5", "M3", "M1", "M2", "M4"):
        xs = [m.unlock_run.get(nid) for m in metas]
        done = [x for x in xs if x is not None]
        out[nid] = dict(done=round(100 * len(done) / len(xs)), med=st.median(done) if len(done) * 2 >= len(xs) else None)
    return out


def win_by_run(hist, width=5):
    """Tỉ lệ thắng (%) theo khối ván: ván 1-5, 6-10..."""
    runs = len(hist[0])
    out = []
    for a in range(0, runs, width):
        xs = [h[k]["win"] for h in hist for k in range(a, min(runs, a + width))]
        out.append(round(100 * sum(xs) / len(xs), 1))
    return out

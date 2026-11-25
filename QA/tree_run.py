"""Sprint 5 phần 4, bản MỚI (chủ dự án chốt 2026-11-25): cây kỹ năng CHỈ DÙNG TRONG VÁN, không có tiến trình vĩnh viễn.
Mỗi ván bắt đầu với cây trống; điểm kiếm và tiêu ngay trong ván 7 ngày; hết ván thì mất.

Mô hình (GIẢ ĐỊNH của Claude 2, nguồn điểm chưa chốt nên đo nhiều phương án):
  - Điểm cộng khi bắt đầu mỗi ngày d >= 2 (tức là tổng kết ngày d - 1); người chơi mua nút ngay lúc đó (buổi sáng).
    Rương mới mở sáng ngày d có mặt trong shop từ ngày d.
  - Nguồn điểm: "ngày" (mỗi ngày qua), "sổ" (mỗi mục mới trong Sổ Sưu Tập của ván, Rất hiếm và Huyền thoại 2), "nợ" (mỗi kỳ trả đúng hạn,
    tức là cuối ngày không còn nợ tồn).
  - Sổ Sưu Tập tính trong ván (rỗng đầu ván). Điều kiện nút quy về thang một ván (cond_run). Nút Câu chuyện dùng điều kiện "qua ngày X".
  - Rương mở khóa chỉ có trong ván khi mở nút cây (kịch bản "trong ván"); rương chủ đề mở bằng thành tựu giả định "Sổ của ván có >= THEME_RUN mục".
  - Kịch bản "vĩnh viễn": mọi rương mở khóa và chủ đề có sẵn từ ngày 1 (đây cũng là CẬN TRÊN của kịch bản trong ván), đo bằng tree_sim.static.
  - M2: cộng vốn lúc mở nút (nếu bật). M1, M3: không mô phỏng trong ván, cận trên lấy từ tree_sim.static (GT1 +0,5 đến 1,6 điểm; M3 làm thua nhiều hơn).
Số tiền trong mã tính theo đơn vị 10.000 đ.
"""
import copy
import sys
from multiprocessing import Pool
import tree_sim as T
import chest_unlock as U
import chest_sim as CS
import clue_sim as C
import inspect_sim as I
import proposal_sim as P

UNLOCK_NAMES = T.UNLOCK_NAMES
THEME_RUN = (6, 10)     # chủ đề j mở khi Sổ của ván có >= THEME_RUN[j] mục (giả định)

# id, nhánh, cha, giá (điểm), điều kiện trong ván, tác động
RUN_TREE = [
    dict(id="R1", branch="Rương", parent=None, cost=3, cond=("book", 3), effect=None),
    dict(id="R2", branch="Rương", parent="R1", cost=4, cond=("junk", 2), effect=("chest", "Rác Quý")),
    dict(id="R3", branch="Rương", parent="R1", cost=6, cond=("origins", 3), effect=("chest", "Tổng Hợp")),
    dict(id="R4", branch="Rương", parent="R3", cost=7, cond=("art", "CP:1"), effect=("chest", "Cổ Phong")),
    dict(id="R5", branch="Rương", parent="R3", cost=7, cond=("art", "TC:1"), effect=("chest", "Thiên Cơ")),
    dict(id="S1", branch="Sưu tập", parent=None, cost=3, cond=("book", 3), effect=None),
    dict(id="S2", branch="Sưu tập", parent="S1", cost=4, cond=("book", 6), effect=None),
    dict(id="M1", branch="Sưu tập", parent="S2", cost=5, cond=("book", 8), effect=("gt", True)),
    dict(id="M2", branch="Sưu tập", parent="M1", cost=7, cond=("book", 10), effect=("cash", True)),
    dict(id="S3", branch="Sưu tập", parent="S2", cost=6, cond=("legend", 1), effect=None),
    dict(id="M4", branch="Sưu tập", parent="S3", cost=7, cond=("art_any", 2), effect=None),
    dict(id="T1", branch="Thần lực", parent=None, cost=3, cond=("book", 3), effect=None),
    dict(id="T2", branch="Thần lực", parent="T1", cost=4, cond=("origins", 3), effect=None),
    dict(id="T3", branch="Thần lực", parent="T2", cost=6, cond=("book", 9), effect=None),
    dict(id="T4", branch="Thần lực", parent="T3", cost=7, cond=("legend", 1), effect=None),
    dict(id="C1", branch="Câu chuyện", parent=None, cost=3, cond=("day", 2), effect=None),
    dict(id="C2", branch="Câu chuyện", parent="C1", cost=4, cond=("day", 3), effect=None),
    dict(id="C3", branch="Câu chuyện", parent="C2", cost=6, cond=("day", 5), effect=None),
    dict(id="C4", branch="Câu chuyện", parent="C3", cost=7, cond=("day", 6), effect=None),
]
BY_ID = {n["id"]: n for n in RUN_TREE}
SOURCES = {   # tên -> (điểm mỗi ngày, điểm mỗi mục Sổ thường, điểm mục Sổ Rất hiếm/Huyền thoại, điểm mỗi kỳ trả đúng hạn)
    "ngày 3": (3, 0, 0, 0),
    "ngày 2": (2, 0, 0, 0),
    "sổ 1/2": (0, 1, 2, 0),
    "nợ 3": (0, 0, 0, 3),
    "ngày 1 + sổ 1/2 + nợ 1": (1, 1, 2, 1),
}
CASH = dict(on=False, amount=100)   # M2 trong ván (100 = 1.000.000 đ)
STATE = dict(cash_node=None, cash_amount=0, source="ngày 3", scale=1.0, cash=False, order="cheap", chests="run", themes=True)


class RunBook:
    """Giống Meta nhưng cho một ván: Sổ của ván, ngày hiện tại."""
    def __init__(self, items):
        self.items = items
        self.book = set()
        self.day = 1
        self.wins = self.runs = 0
        self.bk = set()

    check = T.Meta.check
    origins = T.Meta.origins


def _check(tb, cond):
    if cond[0] == "day":
        return tb.day >= cond[1]
    return tb.check(cond, tb.items)


def _cost(n):
    return max(1, round(n["cost"] * STATE["scale"]))


def _update(run):
    """Gọi đầu mỗi ngày: cộng điểm ngày vừa qua, mua nút, mở rương."""
    g = run.g
    d = g.day
    tr = run.tree
    tb = tr["tb"]
    tb.day = d
    pday, pbook, pbook_hi, pdebt = SOURCES[STATE["source"]]
    if d >= 2:
        new = run.book - tr["seen"]
        tr["seen"] |= new
        pts = pday
        if g.arrears == 0 and g.carry == 0:
            pts += pdebt
        pts += sum(pbook_hi if I.TIER_OF.get(i) in T.HIGH else pbook for i in new)
        tr["points"] += pts
        tr["earned"] += pts
    tb.book = set(run.book)
    for a in run.owned:
        tb.book.add("AX_" + a)
    while True:
        ok = [n for n in RUN_TREE if n["id"] not in tr["open"] and (n["parent"] is None or n["parent"] in tr["open"])
              and _cost(n) <= tr["points"] and _check(tb, n["cond"])]
        if not ok:
            break
        if STATE["order"] == "play":
            ok.sort(key=lambda n: (n["effect"] is None, _cost(n)))
        else:
            ok.sort(key=lambda n: _cost(n))
        n = ok[0]
        tr["points"] -= _cost(n)
        tr["open"][n["id"]] = d
        if n["effect"] and n["effect"][0] == "chest" and STATE["chests"] == "run":
            U.FROM[CS.STATE["key"][n["effect"][1]]] = d
        if n["id"] == STATE["cash_node"]:
            g.cash += STATE["cash_amount"]      # thưởng vốn khi mở nút (thử mức thưởng hậu hĩnh)
        if n["id"] == "M2" and STATE["cash"]:
            g.cash += CASH["amount"]
    if STATE["themes"] and STATE["chests"] == "run":
        for j, t in enumerate(THEME_RUN):
            k = CS.STATE["key"]["Chủ đề %d" % (j + 1)]
            if len(run.book) >= t and U.FROM.get(k, 99) > d:
                U.FROM[k] = d


_START = P.start_hooks


def start_hooks(run):
    if hasattr(run, "tree"):
        _update(run)
    return _START(run)


def _make_init(base):
  def _init(self, *a, **k):
    base(self, *a, **k)
    self.tree = dict(tb=RunBook(self.c.items), seen=set(), points=0, earned=0, open={})
    if STATE["chests"] == "run":
        for kk in list(U.BOXES):
            if kk.startswith("U"):
                U.FROM[kk] = 99
  return _init


def run_static(name, n, seed0=0, chests="run", cash=False, source="ngày 3", scale=1.0, shop_extra=None, themes=True, theme_run=None, cash_node=None, cash_amount=0):
    """Đo n ván của một cách chơi. chests: 'run' (rương chỉ mở qua cây trong ván) hoặc 'perm' (mọi rương có từ ngày 1)."""
    global THEME_RUN
    THEME_RUN = theme_run or (6, 10)
    STATE.update(cash_node=cash_node, cash_amount=cash_amount, source=source, scale=scale, cash=cash, chests=chests, themes=themes,
                 order="play" if name in C.TURN_NAMES else "cheap")
    profile = "giỏi" if name in C.TURN_NAMES else "thường"
    c = T.apply(T.UNLOCK_NAMES, 2, False, 0, profile, shop_extra)
    P.start_hooks = start_hooks
    orig_init = P.Run5.__init__
    P.Run5.__init__ = _make_init(T._init)
    wins = 0
    pts, nodes, first, opened, play_open = [], [], [], {}, {}
    try:
        with T._Skill(name in C.TURN_NAMES):
            for s in range(n):
                run = T.play(seed0 + s, c, name)
                wins += run.g.status == "won"
                tr = run.tree
                pts.append(tr["earned"])
                nodes.append(len(tr["open"]))
                for nid, d in tr["open"].items():
                    opened.setdefault(nid, []).append(d)
    finally:
        P.start_hooks = _START
        P.Run5.__init__ = orig_init
        T.restore()
        U.FROM.clear()
    return dict(win=round(100 * wins / n, 1), pts=round(sum(pts) / n, 1), nodes=round(sum(nodes) / n, 1),
                opened={k: (round(100 * len(v) / n), round(sum(v) / len(v), 1)) for k, v in opened.items()}, n=n)


# --- chạy số đo cho báo cáo (python3 tree_run.py run|perm [n]) -----------------------------------------------
BOTS = ("thận trọng", "liều")
CONFIGS = {   # nhãn -> tham số run_static (kịch bản "trong ván")
    "A. chỉ cây, không chủ đề (giá nút một nửa)": dict(source="ngày 3", scale=.5, themes=False),
    "A2. như A, M2 cộng vốn 1.000.000 đ": dict(source="ngày 3", scale=.5, themes=False, cash=True),
    "A3. như A, giá nút đủ": dict(source="ngày 3", scale=1.0, themes=False),
    "A4. như A, nguồn điểm sổ 1/2": dict(source="sổ 1/2", scale=.5, themes=False),
    "A5. như A, nguồn điểm nợ 3": dict(source="nợ 3", scale=.5, themes=False),
    "B. cây + chủ đề mở khi Sổ ván >= 10 và 16": dict(source="ngày 3", scale=.5, theme_run=(10, 16)),
    "B2. cây + chủ đề mở khi Sổ ván >= 6 và 10": dict(source="ngày 3", scale=.5, theme_run=(6, 10)),
}


def _job(j):
    label, name, n, kw = j
    return label, name, run_static(name, n, **kw)


def main(n):
    jobs = []
    for label, kw in CONFIGS.items():
        for b in BOTS:
            jobs.append((label, b, 2 * n, kw))
        for s in T.SKILLED:
            jobs.append((label, s, n, kw))
    with Pool(4) as pool:
        res = pool.map(_job, jobs, chunksize=1)
    tab = {}
    for label, name, r in res:
        tab.setdefault(label, {})[name] = r
    for label, row in tab.items():
        print("== %s" % label)
        print("  bot %.1f / %.1f   giỏi %s   max %.1f" % (row["thận trọng"]["win"], row["liều"]["win"],
              " / ".join("%.1f" % row[s]["win"] for s in T.SKILLED), max(row[s]["win"] for s in T.SKILLED)))
        for nm in ("thận trọng", "liều", "thận trọng + giám định", "liều + giám định"):
            r = row[nm]
            print("  %-26s điểm %.1f, nút %.1f, mở: %s" % (nm, r["pts"], r["nodes"], dict(sorted(r["opened"].items(), key=lambda kv: kv[1][1]))))
    sys.stdout.flush()


if __name__ == "__main__":
    main(int(sys.argv[1]) if len(sys.argv) > 1 else 1000)

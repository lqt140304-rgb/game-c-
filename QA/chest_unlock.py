"""Sprint 5 phần 2 (Claude 2): rương mở khóa (Tổng Hợp, Cổ Phong, Thiên Cơ, Rác Quý) và khung rương chủ đề.

Cách làm: nạp bảng 4 rương cơ bản của phần 1 (chest_sim.FINAL, GIỮ NGUYÊN) rồi THÊM rương mới vào cùng lõi mô phỏng đã duyệt
(nợ, phạt 20%, giá dao động ±20%, manh mối, giám định, combo). Rương mới có mã nội bộ U1, U2... Thay đổi duy nhất ở lõi:
thứ tự rương của bot thường và danh sách rương người chơi giỏi xem mỗi lượt lấy theo rương đang có trong shop
(xếp theo giá gốc giảm dần), thay cho 4 mã cố định H1..RD.

Giả định (spec chưa có, xem docs/superpowers/specs/2026-11-24-can-bang-ruong-mo-khoa-design.md):
  - Cổ Phong, Thiên Cơ, Rác Quý chưa có bộ sưu tập: dùng món giữ chỗ giá bằng trung bình bậc của rương cơ bản (P..);
  - Cổ Phong / Thiên Cơ ra tạo tác của đúng hệ đó, luật trùng như Kim Khí (đổi DUP_FRAC giá Kho);
  - rương chủ đề: mọi đồ chủ đề cùng một nhóm nguồn gốc (nhóm của chủ đề), Huyền thoại chủ đề giá THEME_LEG;
  - Tổng Hợp: quay bậc rồi quay đều một món trong hợp bộ sưu tập 4 rương cơ bản ở bậc đó (ruong-chu-de mục 9).
Đơn vị cân bằng 10.000 đ; in ra bằng đồng qua currency.fmt_vnd.

Dùng: import chest_unlock as U; c = U.apply(U.scenario("tat_ca")); ...; U.restore()
"""
import copy
import random
import chest_sim as CS
import clue_sim as C
import proposal_sim as P
import sprint3_sim as sm
import swing_sim as S
from sprint3_sim import Content, ITEMS, BOXES, QK, HD, TL, NHT, CT, TT

BASIC = CS.FINAL_CHESTS

# Giá trung bình bậc của 4 rương cơ bản (đơn vị 10.000 đ), dùng cho món giữ chỗ.
def tier_avg():
    out = {}
    for t in ("T", "H", "X", "L"):
        ids = [i for ch in BASIC.values() for i in ch["pools"].get(t, [])]
        out[t] = round(sum(CS.NEW_ITEMS[i][1] for i in ids) / len(ids))
    return out


AVG = tier_avg()   # T 396, H 1687, X 2814, L 5250
THEME_LEG = 9000   # Huyền thoại chủ đề, ĐỀ XUẤT: 90.000.000 đ (Huyền thoại cơ bản 40 đến 70 triệu đ)
THEME_GROUP = NHT  # nhóm nguồn gốc giữ chỗ cho đồ chủ đề (khác nhóm của rác: Quá khứ, Hiện đại)

# Món giữ chỗ: mã -> (nhóm, giá nền, tên). Giá theo trung bình bậc; chưa phải giá chốt.
EXTRA_ITEMS = {
    # Cổ Phong: đồ cổ phong (Quá khứ, Thần thoại)
    "PC1": (QK, AVG["T"], "đồ Thường Cổ Phong"), "PC2": (TT, AVG["T"], "đồ Thường Cổ Phong 2"),
    "PC3": (TT, AVG["H"], "đồ Hiếm Cổ Phong"), "PC4": (TT, AVG["X"], "đồ Rất hiếm Cổ Phong"),
    # Thiên Cơ: đồ máy móc (Tương lai, Ngoài hành tinh)
    "PT1": (TL, AVG["T"], "đồ Thường Thiên Cơ"), "PT2": (NHT, AVG["T"], "đồ Thường Thiên Cơ 2"),
    "PT3": (TL, AVG["H"], "đồ Hiếm Thiên Cơ"), "PT4": (NHT, AVG["X"], "đồ Rất hiếm Thiên Cơ"),
    # Rác Quý: "rác" hóa ra có giá, trông như rác nên mang nhóm của rác (Hiện đại, Quá khứ): manh mối không tách được
    "PR1": (HD, AVG["T"], "rác quý Thường"), "PR2": (QK, AVG["T"], "rác quý Thường 2"),
    "PR3": (HD, AVG["H"], "rác quý Hiếm"), "PR4": (QK, AVG["X"], "rác quý Rất hiếm"),
    "PR5": (HD, AVG["L"], "rác quý Huyền thoại"),
}


def theme_items(tag, leg=None, group=None):
    """Bộ sưu tập giữ chỗ của một rương chủ đề: 2 Thường, 2 Hiếm, 2 Rất hiếm, 1 Huyền thoại chủ đề."""
    g = THEME_GROUP if group is None else group
    leg = THEME_LEG if leg is None else leg
    items = {f"{tag}T1": (g, AVG["T"], "Thường"), f"{tag}T2": (g, AVG["T"], "Thường"),
             f"{tag}H1": (g, AVG["H"], "Hiếm"), f"{tag}H2": (g, AVG["H"], "Hiếm"),
             f"{tag}X1": (g, AVG["X"], "Rất hiếm"), f"{tag}X2": (g, AVG["X"], "Rất hiếm"),
             f"{tag}L1": (g, leg, "Huyền thoại chủ đề")}
    pools = dict(T=[f"{tag}T1", f"{tag}T2"], H=[f"{tag}H1", f"{tag}H2"], X=[f"{tag}X1", f"{tag}X2"], L=[f"{tag}L1"])
    return items, pools


def union_pools():
    out = {}
    for ch in BASIC.values():
        for t, ids in ch["pools"].items():
            out.setdefault(t, []).extend(ids)
    return out


# Bảng ĐỀ XUẤT (Claude 2, Docs/Balance/Sprint5RuongMoKhoa.md). Chưa chốt: chờ chủ dự án duyệt.
UNLOCK = {
    "Tổng Hợp": dict(price=600, fake=.20, ntrue=2, tiers=dict(R=.15, T=.38, H=.28, X=.16, L=.03), pools=union_pools()),
    "Cổ Phong": dict(price=450, fake=.10, ntrue=2, tiers=dict(R=.10, T=.25, H=.22, X=.09, L=0), art_sys="CP",
                     art_mix=(.60, .35, .05), pools=dict(T=["PC1", "PC2"], H=["PC3"], X=["PC4"])),
    "Thiên Cơ": dict(price=450, fake=.10, ntrue=2, tiers=dict(R=.10, T=.25, H=.22, X=.09, L=0), art_sys="TC",
                     art_mix=(.60, .35, .05), pools=dict(T=["PT1", "PT2"], H=["PT3"], X=["PT4"])),
    "Rác Quý": dict(price=60, fake=.05, ntrue=2, tiers=dict(R=.86, T=.11, H=.025, X=0, L=.005),
                    pools=dict(T=["PR1", "PR2"], H=["PR3"], X=["PR4"], L=["PR5"])),
}
# Khung chung của một rương chủ đề (mọi chủ đề dùng chung bảng này, chỉ khác bộ sưu tập).
THEME = dict(price=800, fake=.20, ntrue=1, tiers=dict(R=.13, T=.30, H=.30, X=.24, L=.03))


def theme_chest(tag, **over):
    items, pools = theme_items(tag, over.pop("leg", None), over.pop("group", None))
    ch = dict(copy.deepcopy(THEME), pools=pools, items=items)
    for k, v in over.items():
        ch[k] = dict(v) if k == "tiers" else v
    return ch


def scenario(name, unlock=None, theme=None, n_theme=None, from_day=1, base=None, shop_extra=None, **top):
    """Cấu hình đo. name: 'co_ban' (chỉ 4 rương), một tên trong UNLOCK, 'mo_khoa' (cả 4 rương mở khóa),
    'chu_de_k' (k rương chủ đề), 'tat_ca' (4 mở khóa + 2 chủ đề)."""
    unlock = UNLOCK if unlock is None else unlock
    theme = THEME if theme is None else theme
    extra = {}
    if name in unlock:
        extra[name] = unlock[name]
    if name in ("mo_khoa", "tat_ca"):
        extra.update(unlock)
    k = n_theme if n_theme is not None else (int(name.split("_")[-1]) if name.startswith("chu_de_") else 2 if name == "tat_ca" else 0)
    for j in range(k):
        tag = "Z%d" % (j + 1)
        items, pools = theme_items(tag, theme.get("leg"), theme.get("group"))
        extra["Chủ đề %d" % (j + 1)] = dict({a: b for a, b in theme.items() if a not in ("leg", "group")}, pools=pools, items=items)
    d = dict(base or CS.FINAL, **top)
    return dict(base=d, extra=copy.deepcopy(extra), from_day=from_day, shop_extra=shop_extra)


def weights(chest):
    """Như chest_sim.weights, thêm art_sys (chỉ tạo tác của một hệ)."""
    if not chest.get("art_sys"):
        return CS.weights(chest)
    ch = dict(chest)
    ch.pop("art_mix")
    w = CS.weights(ch)
    share = 1 - sum(chest["tiers"].values())
    for t in (1, 2, 3):
        w["AX_%s%d" % (chest["art_sys"], t)] = 1000 * share * chest["art_mix"][t - 1]
    return {k: v for k, v in w.items() if v > 0}


_ORIG = dict(ladder5=P.ladder5, ladder_turns=C.ladder_turns, ladder_skill=C.ladder_skill, TIERS=S.TIERS)
FROM = {}


SHOP = dict(extra=None)   # None: bày mọi rương đã mở; k: mỗi ngày chỉ bày k rương mở khóa (xoay vòng theo hạt giống ván và ngày)


def available(run):
    """Rương có trong shop hôm nay, xếp theo giá gốc giảm dần (Rương Đen từ ngày RD_FROM_DAY)."""
    d = run.g.day
    ks = [k for k in BOXES if not (k == "RD" and d < sm.RD_FROM_DAY) and d >= FROM.get(k, 1)]
    k = SHOP["extra"]
    ext = [x for x in ks if x.startswith("U")]
    if k is not None and len(ext) > k:
        keep = set(random.Random(f"{run.g.seed}-shop-{d}").sample(sorted(ext), k))
        ks = [x for x in ks if not x.startswith("U") or x in keep]
    return tuple(sorted(ks, key=lambda k: -S.SW["base"][k]))


def ladder5(run, keep_reserve, smart):
    g = run.g
    for _ in range(3):
        r = P.reserve5(run) if keep_reserve else 0
        if g.cash < r:
            r = 0
        for box in available(run):
            if g.cash - run.c.box_price[box] >= r:
                it = run.buy_open(box)
                if it:
                    P.handle5(run, it, r, smart)
                break


def ladder_turns(run, keep_reserve, smart):
    g = run.g
    for _ in range(3):
        r = P.reserve5(run) if keep_reserve else 0
        if g.cash < r:
            r = 0
        it = C.turn_choose(run, r, available(run))
        if not it:
            break
        P.handle5(run, it, r, smart)


def apply(sc):
    """Nạp 4 rương cơ bản (chest_sim.apply) rồi thêm rương mới. Trả Content."""
    CS.apply(sc["base"])
    FROM.clear()
    design = copy.deepcopy(CS.STATE["design"])
    nm = sc["base"].get("new_mult", 1.0)
    for i, (o, b, _) in EXTRA_ITEMS.items():
        ITEMS[i] = (o, round(b * nm))
    keys = list(S.TIERS)
    for j, (name, ch) in enumerate(sc["extra"].items()):
        k = "U%d" % (j + 1)
        for i, (o, b, _) in ch.get("items", {}).items():
            ITEMS[i] = (o, round(b * nm))
        BOXES[k] = (ch["price"], weights(ch))
        C.NTRUE[k] = ch["ntrue"]
        CS.STATE["fake"][k] = ch["fake"]
        S.SW["base"][k] = ch["price"]
        CS.STATE["key"][name], CS.STATE["name"][k] = k, name
        FROM[k] = ch.get("from_day", sc.get("from_day", 1))
        design["chests"][name] = ch
        keys.append(k)
    CS.STATE["design"] = design
    SHOP["extra"] = sc.get("shop_extra")
    S.TIERS = tuple(keys)
    P.ladder5, C.ladder_turns = ladder5, ladder_turns
    return Content(box_price={k: v[0] for k, v in BOXES.items()})


def restore():
    CS.restore()
    FROM.clear()
    SHOP["extra"] = None
    P.ladder5, C.ladder_turns, C.ladder_skill, S.TIERS = (_ORIG[k] for k in ("ladder5", "ladder_turns", "ladder_skill", "TIERS"))


# --- đo ------------------------------------------------------------------------------------------------
def box_table():
    """Thu, lời kỳ vọng và tỉ lệ Huyền thoại mỗi rương (đơn vị cân bằng, chưa tính dao động giá).
    Tạo tác tính theo giá Kho x hệ số chất lượng như chest_sim."""
    out = CS.box_table()
    for n, r in out.items():
        r["L"] = CS.STATE["design"]["chests"][n]["tiers"].get("L", 0)
    return out


def mix_of(content, n, strat="thận trọng + giám định"):
    """Tỉ lệ chọn từng rương (phần trăm lượt mua) của người chơi giỏi."""
    mix = {k: 0 for k in BOXES}
    orig = C.turn_choose

    def spy(run, rc, tiers, _o=orig):
        it = _o(run, rc, tiers)
        if it:
            mix[it["box"]] += 1
        return it
    C.turn_choose = spy
    old = dict(C.SKILL)
    C.SKILL.update(lam=.75, tau=1.0, per_box=True)
    try:
        for s in range(n):
            P.play5(s, content, C.TURN_NAMES[strat])
    finally:
        C.turn_choose = orig
        C.SKILL.clear()
        C.SKILL.update(old)
    tot = sum(mix.values()) or 1
    return {CS.STATE["name"][k]: round(100 * v / tot, 1) for k, v in mix.items()}


def bot_mix(content, n, strat="thận trọng"):
    """Tỉ lệ lượt mua theo rương của bot thường."""
    mix = {k: 0 for k in BOXES}
    orig = P.Run5.buy_open

    def spy(self, box, _o=orig):
        it = _o(self, box)
        if it:
            mix[box] += 1
        return it
    P.Run5.buy_open = spy
    try:
        for s in range(n):
            P.play5(s, content, P.STRATS[strat])
    finally:
        P.Run5.buy_open = orig
    tot = sum(mix.values()) or 1
    return {CS.STATE["name"][k]: round(100 * v / tot, 1) for k, v in mix.items()}


HUNT_SET = {"CP": ("CP1", "CP2"), "TC": ("TC1", "TC2")}


def hunt_strategy(name, bold=False):
    """Người chơi giỏi 'săn hệ': ngày 1 đến 5, chưa đủ cặp combo của hệ (CB1 hoặc CB2) thì ưu tiên mua rương hệ `name`,
    rồi chơi như người chơi giỏi (chọn rương theo lời kỳ vọng, đọc manh mối, giám định)."""
    def strat(run):
        g = run.g
        k = CS.STATE["key"].get(name)
        need = HUNT_SET[CS.STATE["design"]["chests"][name]["art_sys"]] if k else ()
        while k and g.day <= 5 and g.day >= FROM.get(k, 1) and g.bought_today < 3 and not all(a in run.owned for a in need):
            r = 0 if bold else P.reserve5(run)
            if g.cash - run.c.box_price[k] < r:
                break
            it = run.buy_open(k)
            if not it:
                break
            P.handle5(run, it, r, True)
        ladder_turns(run, not bold, True)
    return strat


def measure_hunt(content, name, n):
    old = dict(C.SKILL)
    C.SKILL.update(lam=.75, tau=1.0, per_box=True)
    try:
        out = {}
        for lbl, bold in (("săn hệ thận trọng", False), ("săn hệ liều", True)):
            st = hunt_strategy(name, bold)
            out[lbl] = round(100 * sum(P.play5(s, content, st).g.status == "won" for s in range(n)) / n, 1)
        return out
    finally:
        C.SKILL.clear()
        C.SKILL.update(old)


def evaluate(sc, n=1000, with_mix=True):
    c = apply(sc)
    C.setup_done = True
    try:
        out = {"table": {k: {a: round(b, 3) for a, b in v.items()} for k, v in box_table().items()}}
        lad = P.measure(c, ["thận trọng", "liều"], n=n)
        out["bot"] = {k: v["win"] for k, v in lad.items()}
        sk = C.measure_turns(c, n, lam=.75, tau=1.0)
        out["skilled"] = {k.replace(" + giám định", ""): v for k, v in sk.items() if not k.endswith("[mua]")}
        for name, ch in sc["extra"].items():
            if ch.get("art_sys"):
                out.setdefault("hunt", {}).update({"%s %s" % (k, name): v for k, v in measure_hunt(c, name, n).items()})
        if with_mix:
            out["mix"] = mix_of(c, min(n, 600))
            out["mix_lieu"] = mix_of(c, min(n, 600), "liều + giám định")
            out["bot_mix"] = bot_mix(c, min(n, 600))
    finally:
        restore()
    return out

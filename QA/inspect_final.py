"""Sprint 5 phần 3 (Claude 2): tái hiện số đo của Docs/Balance/Sprint5SoiLoi.md trên bộ luật đề xuất inspect_sim.RULES.
Chạy song song 4 tiến trình (nên chạy nền với nohup):
  python3 inspect_final.py final   # n=2.000: bot thường theo 4 hồ sơ soi, người chơi giỏi đủ 4 chiến thuật,
                                   # lần chơi đầu (chest_sim.FIRST_RUN), số đo ngày 1 (khoảng 6 phút)
  python3 inspect_final.py sens    # n=1.000: độ nhạy mức phạt, trần, lượt soi, chứng nhận, dấu hiệu, hồ sơ
Mọi số tiền in ra đều bằng đồng (currency.fmt_vnd); bên trong mô phỏng vẫn tính theo đơn vị 10.000 đ."""
import json
import multiprocessing as mp
import sys
import chest_sim as CS
import clue_sim as C
import inspect_sim as I
import proposal_sim as P
from currency import fmt_vnd

PROFS = ("thường", "giỏi", "ẩu", "bỏ qua soi")
SK = I.SKILLED


def win_job(a):
    """(nhãn, luật, hồ sơ, chiến thuật, n, bảng rương, hồ sơ đè) -> số đo một ô."""
    label, rules, prof, strat, n, design, over = a
    saved = {k: dict(v) for k, v in I.PROFILES.items()}
    for k, v in (over or {}).items():
        I.PROFILES[k].update(v)
    try:
        c = I.apply(design, rules, prof)
        skilled = strat in C.TURN_NAMES   # tên combo có ở cả hai bảng; bảng người chơi chọn rương được ưu tiên
        st = C.TURN_NAMES[strat] if skilled else P.STRATS[strat]
        win, agg = I._play(c, st, n, skilled=skilled)
        out = dict(win=win, **I.summarize(agg, n))
        if strat == "liều" and design is not None and n >= 1000:
            out["day1"] = I.day1(rules, prof, design, min(n, 1500), strat)
        I.restore()
    finally:
        I.PROFILES.clear()
        I.PROFILES.update(saved)
    return label, prof, strat, out


def final_jobs(n=2000):
    J = []
    for prof in PROFS:
        for b in I.BOT:
            J.append(("ván thường", None, prof, b, n, CS.FINAL, None))
    for k in SK:
        J.append(("ván thường", None, "giỏi", k, n, CS.FINAL, None))
    for k in SK[:2]:
        J.append(("ván thường", None, "thường", k, n, CS.FINAL, None))
    for prof in PROFS:
        J.append(("lần chơi đầu", None, prof, "liều", 1500, CS.FIRST_RUN, None))
        J.append(("lần chơi đầu", None, prof, "thận trọng", 1500, CS.FIRST_RUN, None))
    J.append(("lần chơi đầu", None, "giỏi", SK[1], 1500, CS.FIRST_RUN, None))
    J.append(("lần chơi đầu", None, "giỏi", SK[0], 1500, CS.FIRST_RUN, None))
    J.append(("lần chơi đầu, ngày 1 phạt như ngày thường", dict(day1_fine=.20), "ẩu", "liều", 1500, CS.FIRST_RUN, None))
    J.append(("lần chơi đầu, ngày 1 phạt như ngày thường", dict(day1_fine=.20), "bỏ qua soi", "liều", 1500, CS.FIRST_RUN, None))
    return J


SENS = [
    ("phạt sót 0%", dict(miss_pen=0.0)), ("phạt sót 10%", dict(miss_pen=.10)),
    ("dấu lạ 0%", dict(odd=0.0)), ("dấu lạ 20%", dict(odd=.20)),
    ("phạt giả khai thật 0%", dict(fine=0.0, fine_T=0.0)), ("phạt giả khai thật 40%", dict(fine=.40)),
    ("trần 1,05", dict(cap=1.05)), ("trần 1,20", dict(cap=1.20)), ("2 vòng", dict(rounds=2)),
    ("thắng +12%", dict(up=.12)), ("thua -10%", dict(down=.10)),
    ("gợi ý 10% giá nền", dict(hint_rate=.10)), ("giữ chứng nhận +25%", dict(cert=True)),
    ("1 dấu giả mỗi món giả", dict(marks=dict(T=1, H=1, X=1, L=1))),
    ("3 dấu giả ở Rất hiếm và Huyền thoại", dict(marks=dict(T=1, H=1, X=3, L=3))),
]
SENS_PROF = [("người soi thường khá hơn (s 85%)", dict(thường=dict(s=.85, c=.8))),
             ("người soi thường kém hơn (s 60%)", dict(thường=dict(s=.60, c=.65))),
             ("người soi thường quyết theo giá trị kỳ vọng", dict(thường=dict(rule="opt")))]


def sens_jobs(n=1000):
    J = []
    for label, rules in SENS + [(l, None) for l, _ in SENS_PROF]:
        over = dict(SENS_PROF).get(label)
        for prof in (("thường", "bỏ qua soi") if over is None else ("thường",)):
            J.append((label, rules, prof, "thận trọng", n, CS.FINAL, over))
            J.append((label, rules, prof, "liều", n, CS.FINAL, over))
        if over is None:
            J.append((label, rules, "giỏi", "thận trọng", n, CS.FINAL, over))
            J.append((label, rules, "giỏi", SK[1], n, CS.FINAL, over))
    return J


def show(label, prof, strat, r):
    d = r.pop("day1", None)
    m = dict(r, fines_per_run=fmt_vnd(r["fines_per_run"]), loss_per_run=fmt_vnd(r["loss_per_run"]))
    print("== %s | %s | %s: %s" % (label, prof, strat, json.dumps(m, ensure_ascii=False)))
    if d:
        d = dict(d, arrears1_mean=fmt_vnd(round(d["arrears1_mean"])), arrears1_max=fmt_vnd(d["arrears1_max"]),
                 cash1_min=fmt_vnd(d["cash1_min"]))
        print("   ngày 1: %s" % json.dumps(d, ensure_ascii=False))
    sys.stdout.flush()


if __name__ == "__main__":
    which = sys.argv[1] if len(sys.argv) > 1 else "final"
    n = int(sys.argv[2]) if len(sys.argv) > 2 else None
    J = final_jobs(n or 2000) if which == "final" else sens_jobs(n or 1000)
    print("luật:", json.dumps(I.RULES, ensure_ascii=False))
    with mp.Pool(4) as p:
        for res in p.imap(win_job, J):
            show(*res)

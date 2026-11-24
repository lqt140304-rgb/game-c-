"""Sprint 5 phần 3 (Claude 2): tái hiện số đo của Docs/Balance/Sprint5SoiLoi.md trên bộ luật đề xuất inspect_sim.RULES.
Chạy song song 4 tiến trình (nên chạy nền với nohup):
  python3 inspect_final.py final   # n=2.000: bot thường theo 4 hồ sơ soi, người chơi giỏi đủ 4 chiến thuật,
                                   # lần chơi đầu (chest_sim.FIRST_RUN), số đo ngày 1 (khoảng 6 phút)
  python3 inspect_final.py sens    # n=1.000: độ nhạy mức phạt, trần, lượt soi, chứng nhận, dấu hiệu, hồ sơ (khoảng 10 phút)
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
    ("phạt 0%", dict(fine=0.0, fine_T=0.0)), ("phạt 10%", dict(fine=.10)), ("phạt 30%", dict(fine=.30)),
    ("phạt 40%", dict(fine=.40)), ("phạt 60%", dict(fine=.60)),
    ("trần 1,15", dict(cap=1.15)), ("trần 1,10", dict(cap=1.10)), ("trần 1,20", dict(cap=1.20)),
    ("2 vòng", dict(rounds=2)), ("giá chào mở màn 90%", dict(open=.90)),
    ("lượt soi 1/3/3/3", dict(looks=dict(T=1, H=3, X=3, L=3))), ("lượt soi 1/4/5/5", dict(looks=dict(T=1, H=4, X=5, L=5))),
    ("không bán lượt soi thêm", dict(max_extra=0)), ("lượt soi thêm miễn phí", dict(extra_fee=dict(T=0, H=0, X=0, L=0))),
    ("giữ chứng nhận +25%", dict(cert=True)),
    ("dấu hiệu rõ hơn (nhiễu 15%, dấu lạ 5%)", dict(noise_fake=.15, sig_real=.05)),
    ("dấu hiệu mờ hơn (nhiễu 35%, dấu lạ 15%)", dict(noise_fake=.35, sig_real=.15)),
    ("bán không soi 90%", dict(skip_quick=.90)),
]
SENS_PROF = [("người soi thường khá hơn (h 75%, đọc nhầm 7%)", dict(thường=dict(h=.75, e=.07, c=.8))),
             ("người soi thường kém hơn (h 55%, đọc nhầm 15%)", dict(thường=dict(h=.55, e=.15, c=.7))),
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

"""Sprint 5 phần 1 (Claude 2): tái hiện số đo của Docs/Balance/Sprint5Ruong.md trên bảng đề xuất chest_sim.FINAL.
Dùng (chạy song song 4 tiến trình, nên chạy nền với nohup):
  python3 chest_final.py final   # bảng cuối n=2.000 không sắp sẵn + 3 chế độ hộp ngày 1 sắp sẵn n=1.500, kèm số đo ngày 1 (~10 phút)
  python3 chest_final.py sens    # độ nhạy n=1.000: 3/5 ô, tạo tác trùng, giá trị tạo tác, Kim Khí chỉ tạo tác, giá món mới ±15% (~10 phút)
  python3 chest_final.py slots   # tỉ lệ đạt CB1/CB3/BK1 theo 3/4/5 ô, n=800 (~2 phút)"""
import json
import multiprocessing as mp
import sys
import chest_sim as CS
import clue_sim as C
import proposal_sim as P
from chest_explore import mk


def final(**top):
    return dict(CS.FINAL, **top)


def job(a):
    label, d, n, day1 = a
    r = CS.evaluate(d, n)
    if day1:
        c = CS.apply(d)
        C.setup_done = True
        r["day1_bot"] = CS.day1_stats(c, P.STRATS["thận trọng"], n)
        r["day1_bot_lieu"] = CS.day1_stats(c, P.STRATS["liều"], n)
        old = dict(C.SKILL)
        C.SKILL.update(lam=.75, tau=1.0, per_box=True)
        r["day1_skilled"] = CS.day1_stats(c, C.TURN_NAMES["liều + giám định"], n)
        C.SKILL.clear()
        C.SKILL.update(old)
    return label, r


def slots_job(s):
    c = CS.apply(final(slots=s))
    C.setup_done = True
    r = P.measure(c, ["thận trọng", "combo nhỏ (CB1)", "combo lớn (CB1 + CB3)"], n=800)
    return s, {k: dict(win=v["win"], cb1=v["cb1"], cb3=v["cb3"], bk1=v["bk1"]) for k, v in r.items()}


if __name__ == "__main__":
    which = sys.argv[1] if len(sys.argv) > 1 else "final"
    if which == "slots":
        with mp.Pool(3) as p:
            for s, r in p.imap(slots_job, (3, 4, 5)):
                print(s, "ô:", r)
                sys.stdout.flush()
        sys.exit()
    if which == "final":
        J = [("cuối, không sắp sẵn", final(), 2000, True)] + \
            [("cuối, sắp sẵn %s" % m, final(script=m), 1500, True) for m in ("any", "free1", "cheap")]
    else:
        J = [("3 ô", final(slots=3), 1000, False), ("5 ô", final(slots=5), 1000, False),
             ("tạo tác trùng 0", final(dup_frac=0), 1000, False), ("tạo tác trùng 1", final(dup_frac=1.0), 1000, False),
             ("giá trị tạo tác 0,5", final(art_value=.5), 1000, False),
             ("Kim Khí chỉ tạo tác (spec)", dict(final(), chests=dict(CS.FINAL_CHESTS, **{"Kim Khí": dict(
                 CS.FINAL_CHESTS["Kim Khí"], price=500, tiers=dict(R=.10, T=0, H=0, X=0, L=0))})), 1000, False),
             ("món mới -15%", final(new_mult=.85), 1000, False), ("món mới +15%", final(new_mult=1.15), 1000, False),
             ("bảng khởi đầu của spec", mk({}, dict(script=False)), 1000, False)]
    with mp.Pool(4) as p:
        for label, r in p.imap(job, J):
            print("==", label)
            print(json.dumps(r, ensure_ascii=False))
            sys.stdout.flush()

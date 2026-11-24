"""Công cụ dò cấu hình rương Sprint 5 (Claude 2): chạy song song 4 tiến trình trên chest_sim.evaluate.
Dùng: python3 chest_explore.py  (cấu hình ứng viên G1..G4 ở dưới, n=1500, khoảng 5 đến 10 phút)."""
import copy
import multiprocessing as mp
import sys
import chest_sim as CS


def mk(mods, top=None):
    d = copy.deepcopy(CS.DEFAULT)
    for k, v in (top or {}).items():
        d[k] = v
    for n, m in mods.items():
        for k, v in m.items():
            if k == "tiers":
                d["chests"][n]["tiers"] = dict(v)
            else:
                d["chests"][n][k] = v
    return d


def _ev(args):
    label, d, n = args
    return label, CS.evaluate(d, n)


def run_all(designs, n=1000):
    with mp.Pool(4) as p:
        for label, r in p.imap(_ev, [(l, d, n) for l, d in designs]):
            t = r["table"]
            print("== %s" % label)
            print("   " + " | ".join("%s %d: EV %d lời %d (%.2f)" % (k, v["price"], v["ev"], v["profit"], v["ratio"]) for k, v in t.items()))
            print("   bot", r["bot"])
            print("   skilled", r["skilled"])
            print("   chọn", r.get("mix"))
            sys.stdout.flush()


GO = dict(price=140, tiers=dict(R=.15, T=.83, H=.015, X=0, L=.005))
VAI = dict(tiers=dict(R=.20, T=.49, H=.25, X=.05, L=.01))
KIM = dict(price=500, tiers=dict(R=.10, T=.25, H=.22, X=.08, L=.01))   # phần còn lại 34% là tạo tác
DEN = dict(price=1200, tiers=dict(R=.25, T=.05, H=.20, X=.46, L=.04))


def base(**k):
    return dict({"Gỗ": GO, "Vải Niêm": VAI, "Kim Khí": KIM, "Đen": DEN}, **k)


if __name__ == "__main__":
    D = [("G1", mk(base(), dict(script=False))),
         ("G2 Đen khá hơn", mk(base(**{"Đen": dict(price=1200, tiers=dict(R=.25, T=.05, H=.15, X=.50, L=.05))}), dict(script=False))),
         ("G3 Kim 450", mk(base(**{"Kim Khí": dict(KIM, price=450)}), dict(script=False))),
         ("G4 Vải khá hơn", mk(base(**{"Vải Niêm": dict(tiers=dict(R=.20, T=.45, H=.27, X=.07, L=.01))}), dict(script=False)))]
    run_all(D, 1500)

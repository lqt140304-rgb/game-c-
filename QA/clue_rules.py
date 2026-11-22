"""Thử các luật manh mối và kệ hộp cho D-26 (Claude 1), trên mô hình clue_sim.py của Claude 2.

Mô hình là cận trên: bot đọc manh mối tối ưu (Bayes), người chơi thật sẽ kém hơn.
Mỗi cấu hình: M hộp cùng loại trên kệ, giá hộp đi theo manh mối (lam), số manh mối đúng theo từng loại hộp,
ngưỡng mua tau. Cấu hình nền: gói đã duyệt với phạt rủi ro G1 (giá x1,10).
Chạy trong thư mục QA (khoảng 10 phút):  python3 clue_rules.py
"""
import clue_sim as C

KS = ["thận trọng + giám định", "liều + giám định", "combo lớn + giám định"]
N = 1500
N_TRUE_BY_BOX = {}          # hộp -> số manh mối đúng; rỗng thì dùng SKILL["n_true"]

_orig_choose = C.choose_and_buy


def choose_and_buy(run, box, reserve_cash):
    if N_TRUE_BY_BOX:
        C.SKILL["n_true"] = N_TRUE_BY_BOX.get(box, 2)
    return _orig_choose(run, box, reserve_cash)


def run(label, content, **skill):
    C.choose_and_buy = choose_and_buy
    old = dict(C.SKILL)
    C.SKILL.update(skill)
    C.P.STRATS.update({k + " (đọc manh mối)": v for k, v in C.NAMES.items()})
    res = {}
    for k in KS:
        st = C.P.STRATS[k + " (đọc manh mối)"]
        res[k] = round(100 * sum(C.P.play5(s, content, st).g.status == "won" for s in range(N)) / N, 1)
    C.SKILL.clear(); C.SKILL.update(old)
    print("%-52s" % label + " ".join("%5.1f" % res[k] for k in KS), flush=True)
    return res


if __name__ == "__main__":
    c = C.setup(True)
    plain = C.measure_plain(c, N)
    print("%-52s %s" % ("cấu hình", "thận trọng+gđ / liều+gđ / combo lớn+gđ"))
    print("%-52s" % "không đọc manh mối (bot thường)" + " ".join("%5.1f" % plain[k] for k in KS), flush=True)
    TRUE12 = {"H1": 2, "H2": 2, "H3": 1, "RD": 1}
    configs = [
        ("A. M=2, lam 0,75, mọi hộp 2/3 đúng", dict(M=2, lam=.75, tau=1.3), {}),
        ("B. M=2, lam 0,75, H3 và RD 1/3 đúng", dict(M=2, lam=.75, tau=1.3), TRUE12),
        ("C. M=2, lam 0,9, H3 và RD 1/3 đúng", dict(M=2, lam=.9, tau=1.3), TRUE12),
        ("D. M=1, lam 0,75, H3 và RD 1/3 đúng", dict(M=1, lam=.75, tau=1.3), TRUE12),
        ("E. M=2, lam 1,0, H3 và RD 1/3 đúng", dict(M=2, lam=1.0, tau=1.3), TRUE12),
        ("F. M=2, lam 0,75, H2, H3, RD 1/3 đúng", dict(M=2, lam=.75, tau=1.3), {"H1": 2, "H2": 1, "H3": 1, "RD": 1}),
        ("G. M=3, lam 0,75, H3 và RD 1/3 đúng", dict(M=3, lam=.75, tau=1.3), TRUE12),
        ("B với tau 1,15 (đọc cẩn thận, ít kén)", dict(M=2, lam=.75, tau=1.15), TRUE12),
    ]
    for label, skill, nt in configs:
        N_TRUE_BY_BOX.clear(); N_TRUE_BY_BOX.update(nt)
        run(label, c, **skill)
    # giá mua thêm manh mối 10 xu thay vì 30
    N_TRUE_BY_BOX.clear(); N_TRUE_BY_BOX.update(TRUE12)
    C.CLUE_PRICE = 10
    run("B, mua thêm 2 manh mối với giá 10 xu", c, M=2, lam=.75, tau=1.3, extra=2)
    C.CLUE_PRICE = 30
    run("B, mua thêm 2 manh mối với giá 30 xu", c, M=2, lam=.75, tau=1.3, extra=2)

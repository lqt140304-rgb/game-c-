"""Dò bộ luật soi lỗi và trả giá (Claude 2). Chạy song song 4 tiến trình: python3 inspect_explore.py [n] [nhóm]
In tỉ lệ thắng của bot thường theo hồ sơ soi và của người chơi tính toán hoàn hảo (hồ sơ giỏi)."""
import multiprocessing as mp
import sys
import inspect_sim as I

A = dict(quick=1.0, open=.90)
Q1 = dict(A, fine=.20, fine_T=.10, open=.95, up=.08, cap=1.15)
GROUPS = {
    "a": [
        ("A quick 1,0 open ,90 fine ,30", dict(quick=1.0, open=.90)),
        ("B quick 1,0 open ,90 fine ,20", dict(quick=1.0, open=.90, fine=.20)),
        ("C quick ,95 open ,90 fine ,30", dict(quick=.95, open=.90)),
        ("D quick 1,0 open ,85 up ,12 fine ,30", dict(quick=1.0, open=.85, up=.12)),
        ("E quick 1,0 open ,90 fine ,50", dict(quick=1.0, open=.90, fine=.50)),
        ("F quick 1,0 open ,95 fine ,30", dict(quick=1.0, open=.95)),
        ("G quick 1,05 open ,90 fine ,30", dict(quick=1.05, open=.90)),
        ("H quick 1,0 open ,90 fine ,30 cert", dict(quick=1.0, open=.90, cert=True)),
    ],
    "fine": [("phạt %d%%" % round(100 * f), dict(A, fine=f)) for f in (0.0, .10, .20, .30, .40, .60)],
    "look": [
        ("A (lượt 1/3/4/4, phí thêm 20/30/50)", dict(A)),
        ("lượt 1/3/3/3", dict(A, looks=dict(T=1, H=3, X=3, L=3))),
        ("lượt 1/4/4/4", dict(A, looks=dict(T=1, H=4, X=4, L=4))),
        ("lượt 1/2/3/3", dict(A, looks=dict(T=1, H=2, X=3, L=3))),
        ("Thường soi 0 lượt", dict(A, looks=dict(T=0, H=3, X=4, L=4))),
        ("soi thêm miễn phí", dict(A, extra_fee=dict(T=0, H=0, X=0, L=0))),
        ("phí thêm gấp 3", dict(A, extra_fee=dict(T=0, H=60, X=90, L=150))),
        ("không soi thêm", dict(A, max_extra=0)),
    ],
    "ft": [
        ("phạt 30%, Thường 0%", dict(A, fine_T=0.0)),
        ("phạt 30%, Thường 10%", dict(A, fine_T=.10)),
        ("phạt 40%, Thường 0%", dict(A, fine=.40, fine_T=0.0)),
        ("phạt 40%, Thường 10%", dict(A, fine=.40, fine_T=.10)),
        ("phạt 50%, Thường 10%", dict(A, fine=.50, fine_T=.10)),
        ("phạt 40%, Thường 10%, trần 1,15", dict(A, fine=.40, fine_T=.10, cap=1.15)),
        ("phạt 30%, Thường 10%, trần 1,15", dict(A, fine_T=.10, cap=1.15)),
        ("phạt 40%, Thường 0%, trần 1,15", dict(A, fine=.40, fine_T=0.0, cap=1.15)),
    ],
    "cmp": [
        ("P1 open ,95 up ,08 trần 1,15", dict(A, fine_T=.10, open=.95, up=.08, cap=1.15)),
        ("P2 open ,95 up ,07 trần 1,15", dict(A, fine_T=.10, open=.95, up=.07, cap=1.15)),
        ("P3 open ,92 up ,08 trần 1,15", dict(A, fine_T=.10, open=.92, up=.08, cap=1.15)),
        ("P4 open ,95 up ,08 trần 1,20", dict(A, fine_T=.10, open=.95, up=.08, cap=1.20)),
        ("P5 open ,95 up ,10 trần 1,15", dict(A, fine_T=.10, open=.95, up=.10, cap=1.15)),
        ("P6 open ,95 up ,08 down ,03 trần 1,15", dict(A, fine_T=.10, open=.95, up=.08, down=.03, cap=1.15)),
        ("P7 P1 + phạt 40%", dict(A, fine_T=.10, open=.95, up=.08, cap=1.15, fine=.40)),
        ("P8 open 1,0 up ,06 trần 1,15", dict(A, fine_T=.10, open=1.0, up=.06, cap=1.15)),
    ],
    "q": [
        ("Q1 phạt 20% trần 1,15", dict(A, fine=.20, fine_T=.10, open=.95, up=.08, cap=1.15)),
        ("Q2 phạt 20% trần 1,10", dict(A, fine=.20, fine_T=.10, open=.95, up=.08, cap=1.10)),
        ("Q3 phạt 25% trần 1,15", dict(A, fine=.25, fine_T=.10, open=.95, up=.08, cap=1.15)),
        ("Q4 phạt 30% trần 1,15", dict(A, fine=.30, fine_T=.10, open=.95, up=.08, cap=1.15)),
        ("Q5 Q4 + dấu hiệu rõ hơn", dict(A, fine=.30, fine_T=.10, open=.95, up=.08, cap=1.15, noise_fake=.15, sig_real=.05)),
        ("Q6 Q1 + dấu hiệu rõ hơn", dict(A, fine=.20, fine_T=.10, open=.95, up=.08, cap=1.15, noise_fake=.15, sig_real=.05)),
        ("Q7 phạt 20% open ,90 up ,10 trần 1,15", dict(A, fine=.20, fine_T=.10, open=.90, up=.10, cap=1.15)),
        ("Q8 phạt 25% trần 1,10", dict(A, fine=.25, fine_T=.10, open=.95, up=.08, cap=1.10)),
    ],
    "r": [
        ("R1 Q1 + trần 1,12", dict(Q1, cap=1.12)),
        ("R2 Q1 + vòng H2 X3 L3", dict(Q1, rounds=dict(T=0, H=2, X=3, L=3))),
        ("R3 Q1 + up ,07", dict(Q1, up=.07)),
        ("R4 Q1 + vòng H2 X3 L3 + phạt 25%", dict(Q1, rounds=dict(T=0, H=2, X=3, L=3), fine=.25)),
    ],
    "hag": [
        ("up ,08 down ,05", dict(A, up=.08)),
        ("up ,12 down ,05", dict(A, up=.12)),
        ("up ,10 down ,10", dict(A, down=.10)),
        ("2 vòng", dict(A, rounds=2)),
        ("open ,80 up ,13", dict(A, open=.80, up=.13)),
        ("cap 1,10", dict(A, cap=1.10)),
        ("cap 1,30 up ,12", dict(A, cap=1.30, up=.12)),
        ("open ,90 quick ,95 Thường", dict(A, quick=.95)),
    ],
}


PROFS = ("thường", "giỏi", "ẩu", "bỏ qua soi")


def job(a):
    label, rules, n = a
    out = I.evaluate(rules, profiles=PROFS, n=n,
                     bots=("thận trọng", "liều"), skilled=("thận trọng + giám định", "liều + giám định"))
    return label, out


if __name__ == "__main__":
    n = int(sys.argv[1]) if len(sys.argv) > 1 else 500
    grp = sys.argv[2] if len(sys.argv) > 2 else "a"
    with mp.Pool(4) as p:
        for label, out in p.imap(job, [(l, r, n) for l, r in GROUPS[grp]]):
            b = out["bot"]
            s = out["skilled"]
            row = " | ".join("%s %s %.1f" % (k[0][:4], k[1][:4], v["win"]) for k, v in b.items())
            row2 = " | ".join("%s %.1f" % (k[0][:4], v["win"]) for k, v in s.items())
            thu = " ".join("%.3f" % v["rev_vs_fair"] for k, v in b.items() if k[1] == "thường")
            print("== %s\n   bot: %s\n   giỏi chọn rương: %s   thu/giá đúng (bot thường) %s" % (label, row, row2, thu), flush=True)

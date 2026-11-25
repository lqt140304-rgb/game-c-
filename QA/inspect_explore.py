"""Dò bộ luật soi lỗi và trả giá (Claude 2). Chạy song song 4 tiến trình: python3 inspect_explore.py [n] [nhóm]
In tỉ lệ thắng của bot thường theo hồ sơ soi và của người chơi tính toán hoàn hảo (hồ sơ giỏi)."""
import multiprocessing as mp
import sys
import inspect_sim as I

A = dict()
GROUPS = {
    "pen": [("phạt sót %d%%, dấu lạ %d%%" % (round(100 * p), round(100 * o)), dict(miss_pen=p, odd=o))
            for p in (0.0, .05, .10) for o in (0.0, .10, .20)],
    "fine": [("phạt khai thật nhầm giả %d%%" % round(100 * f), dict(fine=f)) for f in (0.0, .10, .20, .30, .40)],
    "hag": [
        ("up ,08 down ,05 trần 1,12 (đề xuất)", dict(A)),
        ("trần 1,05", dict(cap=1.05)), ("trần 1,20", dict(cap=1.20)), ("không trần (1,5)", dict(cap=1.5)),
        ("up ,05", dict(up=.05)), ("up ,12", dict(up=.12)), ("2 vòng", dict(rounds=2)), ("5 vòng", dict(rounds=5)),
    ],
    "day1": [("ngày 1 phạt giả như thường", dict(day1_fine=.20)), ("ngày 1 miễn phạt (đề xuất)", dict(A))],
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

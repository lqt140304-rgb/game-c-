"""Chạy số đo cho báo cáo Sprint5CayPhatTrien.md (Claude 2). Mỗi chế độ in một khối kết quả.

  python3 tree_final.py static [n]   # tác động của từng nút đổi ván, cấu hình cố định (người chơi giỏi n, bot 2n)
  python3 tree_final.py chain [người] [ván]   # chuỗi ván: nhịp mở nút, tỉ lệ thắng theo chuỗi
  python3 tree_final.py sens [người] [ván]    # độ nhạy điểm (thắng/thua, Sổ), M2, shop
"""
import sys
from multiprocessing import Pool
import tree_sim as T

SK = T.SKILLED
BOTS = ("thận trọng", "liều")
ALL4 = T.UNLOCK_NAMES
STATIC = {
    "nền (4 rương cơ bản)": dict(),
    "M2 vốn +1.000.000 đ": dict(cash=100),
    "M3 Rương Đen từ ngày 1": dict(rd_early=True),
    "M1 chọn GT1": dict(gt="GT1"),
    "M1 chọn GT2": dict(gt="GT2"),
    "+ Rác Quý, Tổng Hợp": dict(chests=("Rác Quý", "Tổng Hợp")),
    "+ 4 rương mở khóa": dict(chests=ALL4),
    "+ 2 chủ đề (chưa mở rương cây)": dict(n_theme=2),
    "+ 4 mở khóa + 2 chủ đề": dict(chests=ALL4, n_theme=2),
    "+ 4 mở khóa + 2 chủ đề + M3": dict(chests=ALL4, n_theme=2, rd_early=True),
    "+ 4 mở khóa + 2 chủ đề + M2": dict(chests=ALL4, n_theme=2, cash=100),
    "cả cây, M2 +1.000.000 đ": dict(chests=ALL4, n_theme=2, cash=100, rd_early=True),
    "+ 4 mở khóa, shop 2 rương mới": dict(chests=ALL4, shop_extra=2),
    "+ 4 mở khóa + 2 chủ đề, shop 2 rương mới": dict(chests=ALL4, n_theme=2, shop_extra=2),
    "cả cây, M2 +1.000.000 đ, shop 2": dict(chests=ALL4, n_theme=2, cash=100, rd_early=True, shop_extra=2),
}


def _static(job):
    label, name, n, cfg = job
    return label, name, T.static(name, n=n, **cfg)


def static(n):
    jobs = []
    for label, cfg in STATIC.items():
        for b in BOTS:
            jobs.append((label, b, 2 * n, cfg))
        for s in SK:
            jobs.append((label, s, n, cfg))
    with Pool(4) as pool:
        res = pool.map(_static, jobs, chunksize=1)
    tab = {}
    for label, name, w in res:
        tab.setdefault(label, {})[name] = w
    for label, row in tab.items():
        print("%-44s bot %5.1f / %5.1f   giỏi %s   max %.1f" % (label, row["thận trọng"], row["liều"],
              " / ".join("%.1f" % row[s] for s in SK), max(row[s] for s in SK)), flush=True)


def _chain(job):
    name, players, runs, seed0, over, profile = job
    for k, v in over.items():
        setattr(T, k, v)
    m, h = T.chain(name, players=players, runs=runs, seed0=seed0, profile=profile)
    return name, m, h


def chains(players, runs, names, over=None, label="", profile=None):
    jobs = []
    per = max(1, players // 4)
    for name in names:
        for j in range(4):
            jobs.append((name, per, runs, 10_000_000 * (j + 1), over or {}, profile))
    with Pool(4) as pool:
        res = pool.map(_chain, jobs, chunksize=1)
    out = {}
    for name, m, h in res:
        a = out.setdefault(name, ([], []))
        a[0].extend(m)
        a[1].extend(h)
    for name, (m, h) in out.items():
        report(label + name, m, h)
    return out


def report(name, m, h):
    ms = T.milestones(m, h)
    n = len(h)
    runs = len(h[0])
    pts = [round(sum(x[k]["pts_run"] + x[k]["pts_book"] for x in h) / n, 1) for k in range(runs)]
    book = [round(sum(x[k]["book"] for x in h) / n) for k in range(runs)]
    print("== %s (%d người, %d ván)" % (name, n, runs))
    print("   thắng theo khối 5 ván:", T.win_by_run(h))
    print("   điểm mỗi ván (ván 1, 2, 3, 5, 10, 20, cuối):", [pts[i] for i in (0, 1, 2, 4, 9, 19, runs - 1) if i < runs])
    print("   Sổ (ván 1, 3, 5, 10, 20, cuối):", [book[i] for i in (0, 2, 4, 9, 19, runs - 1) if i < runs])
    print("   mốc:", ms, flush=True)


if __name__ == "__main__":
    mode = sys.argv[1]
    if mode == "static":
        static(int(sys.argv[2]) if len(sys.argv) > 2 else 1500)
    elif mode == "chain":
        p = int(sys.argv[2]) if len(sys.argv) > 2 else 400
        r = int(sys.argv[3]) if len(sys.argv) > 3 else 40
        chains(p * 2, r, BOTS)
        chains(p, r, ("liều + giám định", "thận trọng + giám định"))
        chains(p * 2, r, ("thận trọng", "liều"), label="[soi ẩu] ", profile="ẩu")

"""Sprint 5 phần 2 (Claude 2): tái hiện số đo của Docs/Balance/Sprint5RuongMoKhoa.md trên bảng đề xuất chest_unlock.UNLOCK và THEME,
nền là 4 rương cơ bản đã chốt (chest_sim.FINAL). Chạy song song 4 tiến trình, nên chạy nền với nohup:
  python3 chest_unlock_final.py final   # nền và từng kịch bản shop, n=1.500 (khoảng 35 phút)
  python3 chest_unlock_final.py sens    # đối chứng, shop xoay vòng, mở khóa giữa ván, giá Huyền thoại chủ đề, n=1.000 (khoảng 25 phút)
Mọi số tiền in ra bằng đồng (currency.fmt_vnd); bên trong mô phỏng tính theo đơn vị 10.000 đ."""
import copy
import json
import multiprocessing as mp
import sys
import chest_unlock as U
from currency import fmt_vnd


def one(name, **over):
    u = copy.deepcopy(U.UNLOCK[name])
    u.update(over)
    return U.scenario(name, unlock={name: u})


def theme(**over):
    return U.scenario("chu_de_1", theme=dict(U.THEME, **over))


SETS = {
    "final": [(nm, U.scenario(nm)) for nm in ("co_ban", "Tổng Hợp", "Cổ Phong", "Thiên Cơ", "Rác Quý", "chu_de_1", "chu_de_3",
                                             "mo_khoa", "tat_ca")],
    "sens": [
        ("đối chứng: rương toàn rác", one("Rác Quý", tiers=dict(R=1.0, T=0, H=0, X=0, L=0))),
        ("đối chứng: rương không ai mua nổi", one("Rác Quý", price=99999)),
        ("tất cả, shop bày 2 rương mới mỗi ngày", U.scenario("tat_ca", shop_extra=2)),
        ("tất cả, có từ ngày 3 (mở khóa giữa ván)", U.scenario("tat_ca", from_day=3)),
        ("Tổng Hợp Huyền thoại 4%", one("Tổng Hợp", tiers=dict(R=.14, T=.38, H=.28, X=.16, L=.04))),
        ("Cổ Phong 4.000.000 đ, 50% tạo tác", one("Cổ Phong", price=400, tiers=dict(R=.10, T=.25, H=.12, X=.03, L=0))),
        ("Rác Quý Huyền thoại 1%", one("Rác Quý", tiers=dict(R=.85, T=.12, H=.02, X=0, L=.01))),
        ("chủ đề, Huyền thoại 70 triệu đ", theme(leg=7000)),
        ("chủ đề, Huyền thoại 120 triệu đ", theme(leg=12000)),
        ("chủ đề, Huyền thoại 120 triệu đ, tỉ lệ 2%", theme(leg=12000, tiers=dict(R=.14, T=.30, H=.30, X=.24, L=.02))),
        ("chủ đề, Huyền thoại 5%", theme(tiers=dict(R=.13, T=.30, H=.30, X=.22, L=.05))),
        ("chủ đề, manh mối đúng 2 trên 3", theme(ntrue=2)),
    ],
    "he": [("%s 4.500.000 đ, R%d T%d H%d X%d" % (nm, *[round(100 * v) for v in t[:4]]), one(nm, tiers=dict(zip("RTHXL", t))))
           for nm in ("Cổ Phong", "Thiên Cơ") for t in ((.15, .30, .15, .06, 0), (.12, .29, .17, .08, 0))],
}
_RQ0 = dict(copy.deepcopy(U.UNLOCK), **{"Rác Quý": dict(U.UNLOCK["Rác Quý"], tiers=dict(R=.865, T=.11, H=.025, X=0, L=0))})
SETS["xacnhan"] = [("mo_khoa", U.scenario("mo_khoa")), ("tat_ca", U.scenario("tat_ca")),
                   ("mo_khoa, Rác Quý không Huyền thoại", U.scenario("mo_khoa", unlock=_RQ0)),
                   ("tất cả, shop bày 2 rương mới mỗi ngày", U.scenario("tat_ca", shop_extra=2))]
N = {"final": 1500, "sens": 1000, "he": 1000, "xacnhan": 1500}


def job(a):
    label, sc, n = a
    return label, U.evaluate(sc, n)


if __name__ == "__main__":
    which = sys.argv[1] if len(sys.argv) > 1 else "final"
    n = int(sys.argv[2]) if len(sys.argv) > 2 else N[which]
    with mp.Pool(4) as p:
        for label, r in p.imap(job, [(l, s, n) for l, s in SETS[which]]):
            t = r.pop("table")
            print("==", label)
            print("   rương: " + " | ".join("%s %s thu %.2f lần, lời %s, HT %.1f%%" % (k, fmt_vnd(v["price"]), v["ratio"], fmt_vnd(round(v["profit"])), 100 * v["L"])
                                         for k, v in t.items()))
            print("   " + json.dumps(r, ensure_ascii=False))
            sys.stdout.flush()

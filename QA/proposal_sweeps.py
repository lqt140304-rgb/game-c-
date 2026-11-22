"""Quét tham số quanh đề xuất của Claude 1. Chạy: python3 proposal_sweeps.py (khoảng 8 phút)."""
import proposal_sim as P
from sprint3_sim import ART
import sprint3_sim as sm

N = 3000
NAMES = ["thận trọng", "thận trọng + giám định chọn lọc", "liều + giám định chọn lọc",
         "combo nhỏ (CB1)", "combo nhỏ + giám định", "combo lớn + giám định"]


def run(label, **kw):
    saved = {k: getattr(P, k) for k in kw}
    for k, v in kw.items():
        setattr(P, k, v)
    rows = P.measure(P.C1, NAMES, n=N)
    print("%-38s" % label + " ".join("%6.1f" % rows[n]["win"] for n in NAMES), flush=True)
    for k, v in saved.items():
        setattr(P, k, v)


print("%-38s" % "" + " ".join("%6s" % n[:6] for n in NAMES))
print("cột: thận trọng | +giám định | liều+gđ | combo nhỏ | nhỏ+gđ | lớn+gđ")
run("đề xuất của Claude 1")
print("-- phí giám định / thưởng chứng nhận")
for fee, cert in ((.10, .25), (.15, .25), (.20, .25), (.15, .35), (.20, .35), (.25, .45)):
    run(f"phí {int(fee*100)}%, chứng nhận +{int(cert*100)}%", APPR_RATE=fee, CERT_BONUS=cert)
print("-- TC3 và CB3")
for tc3, cb3 in ((.10, 1), (.20, 1), (.30, 1), (.10, 2), (.20, 2)):
    run(f"TC3 +{int(tc3*100)}%, CB3 miễn phí {cb3} lần/ngày", TC3_BONUS=tc3, CB3_FREE=cb3)
for p3, t3 in ((900, 700), (600, 700), (600, 500)):
    old = (ART["CP3"], ART["TC3"])
    ART["CP3"], ART["TC3"] = ("CP", p3), ("TC", t3)
    run(f"giá CP3 {p3}, TC3 {t3}")
    ART["CP3"], ART["TC3"] = old
print("-- GT1 hoãn nợ: dùng ở ngày cố định thay vì khi thiếu tiền")
for d in (None, 2, 3, 4, 5):
    P_rows = None
    saved = P.GT1_DAY; P.GT1_DAY = d
    rows = P.measure(P.C1, ["thận trọng", "combo nhỏ (CB1)"], gt="GT1", n=N)
    print("GT1 dùng ngày %-5s thận trọng %5.1f  combo nhỏ %5.1f" % (d if d else "thiếu", rows["thận trọng"]["win"], rows["combo nhỏ (CB1)"]["win"]), flush=True)
    P.GT1_DAY = saved
rows = P.measure(P.C1, ["thận trọng", "combo nhỏ (CB1)"], gt="GT2", n=N)
print("GT2 (không hiệu ứng tiền)           thận trọng %5.1f  combo nhỏ %5.1f" % (rows["thận trọng"]["win"], rows["combo nhỏ (CB1)"]["win"]))
print("-- đấu giá: k kỳ vọng (món từ 1.300 xu, sàn 1,2 G), thận trọng / liều / combo nhỏ+gđ")
base = P.measure(P.C1, ["thận trọng", "liều", "combo nhỏ + giám định"], n=N)
for scale in (0.83, 0.9, 1.0):
    P.K_NEW[:] = [(p, round(k * scale, 3)) for p, k in P.K_BASE]
    e = sum(p * k for p, k in P.K_NEW)
    r = P.measure(P.C1, ["thận trọng", "liều", "combo nhỏ + giám định"], opt=dict(auction=dict(min_g=1300, floor=1.2)), n=N)
    print("k kỳ vọng %.2f" % e + "  " + "  ".join("%s %5.1f (%+.1f)" % (n[:8], r[n]["win"], r[n]["win"] - base[n]["win"]) for n in r), flush=True)
P.K_NEW[:] = P.K_BASE

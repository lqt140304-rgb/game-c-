"""Thử cơ chế phạt rủi ro O-01: tỷ lệ hàng giả tăng theo giá hộp (Claude 1).

Dùng nguyên mô hình proposal_sim.py của Claude 2 với gói đề xuất ở Docs/Balance/DoLaiDeXuat.md
(phí giám định 15%, chứng nhận +25%, CP1 +5%, CB1 +10%, CB3 +15% giá bán và giám định miễn phí
không giới hạn, giá CP3 và TC3 là 500). Chỉ đổi tỷ lệ hàng giả theo hộp.
Người chơi biết loại hộp mình mở nên bot dùng đúng tỷ lệ của hộp khi tính có nên giám định.

Chạy trong thư mục QA (khoảng 1 phút):  python3 risk_penalty.py
"""
import proposal_sim as P
import sprint3_sim as sm
from sprint3_sim import ART, BOXES, FAKE_P_V09, FAKE_X

PKG = dict(APPR_RATE=.15, CERT_BONUS=.25, CP1_B=.05, CB1_B=.10, CB3_PRICE=.15, CB3_FREE=99)
N = 1500
NAMES = ["thận trọng", "thận trọng + giám định chọn lọc", "liều", "liều + giám định chọn lọc",
         "combo nhỏ (CB1)", "combo nhỏ + giám định"]

FAKE_BY_BOX = {}   # hộp -> tỷ lệ hàng giả; rỗng thì dùng mặc định của mô hình


def fake_rate(box, item_id):
    if not FAKE_BY_BOX:
        return FAKE_P_V09 if item_id == "V09" else sm.FAKE_P
    r = FAKE_BY_BOX[box]
    return max(r, FAKE_P_V09) if item_id == "V09" else r


def buy_open(self, box):
    g = self.g
    price = self.c.box_price[box]
    if g.bought_today >= 3 or g.cash < price:
        return None
    g.cash -= price
    g.bought_today += 1
    w = BOXES[box][1]
    iid = self.rng.choices(list(w), weights=list(w.values()))[0]
    r, acc, qf, ql = self.rng.random(), 0, 1.0, "Nguyên"
    for name, p, f in self.c.quality:
        acc += p
        if r <= acc:
            qf, ql = f, name
            break
    fake = self.rng.random() < fake_rate(box, iid)
    return {"id": iid, "q": qf, "ql": ql, "fake": fake, "appraised": False, "box": box}


def p_real(self, it):
    return 1 - fake_rate(it.get("box", "H1"), it["id"])


def box_ev(content):
    qm = sum(p * f for _, p, f in content.quality)
    out = {}
    for b, (price, w) in BOXES.items():
        tot = sum(w.values())
        e = sum(x / tot * content.items[i][1] * qm * (1 - fake_rate(b, i) + fake_rate(b, i) * FAKE_X)
                for i, x in w.items())
        out[b] = e / content.box_price[b]
    return out


if __name__ == "__main__":
    P.Run5.buy_open = buy_open
    P.Run5.p_real = p_real
    for k, v in PKG.items():
        setattr(P, k, v)
    ART["CP3"], ART["TC3"] = ("CP", 500), ("TC", 500)
    variants = [
        ("Gói của Claude 2 (hàng giả 15% mọi hộp)", {}),
        ("Hàng giả H1 10% / H2 15% / H3 22% / RD 30%", dict(H1=.10, H2=.15, H3=.22, RD=.30)),
        ("Hàng giả H1 10% / H2 15% / H3 25% / RD 35%", dict(H1=.10, H2=.15, H3=.25, RD=.35)),
        ("Hàng giả H1 08% / H2 15% / H3 28% / RD 40%", dict(H1=.08, H2=.15, H3=.28, RD=.40)),
    ]
    for label, rates in variants:
        FAKE_BY_BOX.clear()
        FAKE_BY_BOX.update(rates)
        ev = box_ev(P.C1)
        print("== %s" % label)
        print("   lần giá hộp H1/H2/H3/RD: " + " / ".join("%.2f" % ev[b] for b in ("H1", "H2", "H3", "RD")))
        res = P.measure(P.C1, NAMES, n=N)
        for n in NAMES:
            print("   %-34s thắng %5.1f%%  hụt %.2f" % (n, res[n]["win"], res[n]["sf"]), flush=True)

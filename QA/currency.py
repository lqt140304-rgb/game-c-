"""Tiền tệ: đồng Việt Nam (quyết định 2026-11-24, docs/superpowers/specs/2026-11-24-tien-te-design.md).

Mô phỏng và kiểm thử dùng ĐƠN VỊ CÂN BẰNG = 10.000 đ (bằng 1 xu cũ), nên mọi con số trong QA/ giữ nguyên.
Chỉ khi in cho người đọc mới đổi sang đồng bằng các hàm bên dưới.
"""
UNIT_VND = 10_000


def to_vnd(units):
    return int(round(units * UNIT_VND))


def from_vnd(vnd):
    if vnd % UNIT_VND:
        raise ValueError("số tiền không là bội của 10.000 đ: %d" % vnd)
    return vnd // UNIT_VND


def fmt_vnd(units, short=False):
    v = to_vnd(units)
    if short and abs(v) >= 1_000_000:
        m = v / 1_000_000
        return ("%g triệu" % round(m, 1)).replace(".", ",")
    return "{:,}".format(v).replace(",", ".") + " đ"

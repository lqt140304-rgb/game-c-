#!/usr/bin/env bash
# Biên dịch thử Rương Đói (MimicFeed*) — dùng check_all.sh (toàn bộ Scripts + Editor, stub UnityEngine, không cần Unity)
# rồi thêm bộ define Unity 6 (UNITY_6000_0_OR_NEWER: nhánh linearVelocity). Chỉ bắt lỗi cú pháp/kiểu.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
bash "$here/check_all.sh"
sed 's/for defs in "UNITY_EDITOR"/for defs in "UNITY_EDITOR;ENABLE_INPUT_SYSTEM;UNITY_6000_0_OR_NEWER" "UNITY_EDITOR"/' "$here/check_all.sh" > "$here/.check_mimic_feed_tmp.sh"
trap 'rm -f "$here/.check_mimic_feed_tmp.sh"' EXIT
bash "$here/.check_mimic_feed_tmp.sh"

#!/usr/bin/env bash
# Kiểm hiệu ứng rương không cần Unity: kiểm hạt (check_particle_velocity.sh) rồi biên dịch TOÀN BỘ Scripts + Editor bằng stub
# (check_all.sh). Trước đây script này biên dịch một tập con; sau khi dọn rương và thêm ChestKeySetup/CreateAllChestFxProfiles
# tập con kéo theo cả chuỗi phụ thuộc, nên giờ dùng chung check_all.sh. Cần mono-mcs (apt-get install mono-mcs).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
bash "$here/check_particle_velocity.sh"
bash "$here/check_all.sh"

#!/usr/bin/env bash
# Kiểm quy tắc "velocityOverLifetime chỉ đặt qua ChestFxFactory.SetVelocity" (không cần Unity, chỉ grep).
# Lý do: Unity báo lỗi "Particle (Orbital) Velocity curves must all be in the same mode" nếu x/y/z, orbitalX/Y/Z hoặc
# offsetX/Y/Z ở chế độ khác nhau. SetVelocity đặt cả 9 đường cong cùng chế độ TwoConstants.
# Nếu cần thật sự chạm velocityOverLifetime ở chỗ khác, thêm tham số vào SetVelocity thay vì tự đặt.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
bad=0
# 1) velocityOverLifetime chỉ được xuất hiện trong ChestFxFactory.cs (trong SetVelocity).
while IFS= read -r line; do
  echo "VI PHẠM: $line"; bad=1
done < <(grep -rnE "velocityOverLifetime|VelocityOverLifetimeModule" "$repo/Assets" --include=*.cs | grep -v "ChestFx/ChestFxFactory.cs" || true)
# 2) orbitalX/Y/Z và offsetX/Y/Z chỉ được gán trong ChestFxFactory.cs.
while IFS= read -r line; do
  echo "VI PHẠM: $line"; bad=1
done < <(grep -rnE "\.(orbital|offset)[XYZ]\s*=" "$repo/Assets" --include=*.cs | grep -v "ChestFx/ChestFxFactory.cs" || true)
# 3) Trong SetVelocity, 9 đường cong x/y/z, orbitalX/Y/Z, offsetX/Y/Z đều là TwoConstants (hai tham số).
body="$(sed -n '/public static void SetVelocity/,/^        }/p' "$repo/Assets/_Project/Scripts/ChestFx/ChestFxFactory.cs")"
for c in x y z orbitalX orbitalY orbitalZ offsetX offsetY offsetZ; do
  if ! grep -qE "v\.$c = new ParticleSystem\.MinMaxCurve\([^,()]+(\([^)]*\))?[^,()]*, [^()]+(\([^)]*\))?[^()]*\);" <<<"$body"; then
    echo "VI PHẠM: SetVelocity không đặt v.$c dạng TwoConstants"; bad=1
  fi
done
[ "$bad" = 0 ] && echo "OK: velocityOverLifetime chỉ đặt qua SetVelocity, cả 9 đường cong TwoConstants"
exit "$bad"

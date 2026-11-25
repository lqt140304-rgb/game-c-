#!/usr/bin/env bash
# Biên dịch thử (KHÔNG có Unity) kịch bản đào mỏ Chest_Miner cùng TOÀN BỘ script dự án (như check_all.sh).
# Chỉ bắt lỗi cú pháp/kiểu, không bắt sai khác API Unity thật. Cần mcs (apt-get install mono-mcs).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -E -e 's/public (static |abstract |sealed )?(class|struct) /public \1partial \2 /g' \
    -E -e 's/public enum (KeyCode|Key|TextAnchor|LightType|LightShadows|ParticleSystemStopBehavior|ParticleSystemShapeType|ParticleSystemRenderMode|PrimitiveType|ShadowCastingMode) \{[^}]*\}//g' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067,0108,0114 -out:"$out/UnityStub.dll" "$out/UnityStub.cs" "$here/UnityStubAll.cs"
mapfile -t src < <(find "$repo/Assets/_Project/Scripts" -name '*.cs' | sort)
mapfile -t -O "${#src[@]}" src < <(find "$repo/Assets/_Project/Editor" -name '*.cs' | sort)
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "NONE"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/All.dll" "${src[@]}"
done
echo OK

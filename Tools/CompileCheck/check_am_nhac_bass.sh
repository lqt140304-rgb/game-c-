#!/usr/bin/env bash
# Biên dịch thử (không Unity) kịch bản Chest_AmNhac "Loa Bass Kẹo Kéo" cùng TOÀN BỘ script dự án (như check_all.sh),
# trên stub chung + UnityStubAmNhacBass.cs. Bắt lỗi cú pháp/kiểu, KHÔNG bắt sai khác API Unity thật. Cần mono-mcs.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -E -e 's/public (static |abstract |sealed )?(class|struct) /public \1partial \2 /g' \
    -E -e 's/public enum (KeyCode|Key|TextAnchor|LightType|LightShadows|ParticleSystemStopBehavior|ParticleSystemShapeType|ParticleSystemRenderMode|PrimitiveType|ShadowCastingMode) \{[^}]*\}//g' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
sed -E -e 's/public (static |abstract |sealed )?(class|struct) /public \1partial \2 /g' "$here/UnityStubAll.cs" > "$out/UnityStubAll.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067,0108,0114 -out:"$out/UnityStub.dll" \
    "$out/UnityStub.cs" "$out/UnityStubAll.cs" "$here/UnityStubAmNhacBass.cs"
mapfile -t src < <(find "$repo/Assets/_Project/Scripts" -name '*.cs' | sort)
mapfile -t -O "${#src[@]}" src < <(find "$repo/Assets/_Project/Editor" -name '*.cs' | sort)
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "UNITY_EDITOR;UNITY_6000_0_OR_NEWER" "NONE"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/All.dll" "${src[@]}"
done
echo OK

#!/usr/bin/env bash
# Biên dịch thử (KHÔNG có Unity) file của Chest_Golden "Máy Xèng Kho Báu" với stub UnityEngine dùng chung
# (UnityStub.cs + UnityStubAll.cs, cách làm của check_all.sh) + GoldenStubExtra.cs (vài API còn thiếu).
# Chỉ bắt lỗi cú pháp/kiểu, không kiểm được hành vi thật trong Unity.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -E -e 's/public (static |abstract |sealed )?(class|struct) /public \1partial \2 /g' \
    -E -e 's/public enum (KeyCode|Key|TextAnchor|LightType|LightShadows|ParticleSystemStopBehavior|ParticleSystemShapeType|ParticleSystemRenderMode|PrimitiveType|ShadowCastingMode) \{[^}]*\}//g' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067,0108,0114 -out:"$out/UnityStub.dll" \
    "$out/UnityStub.cs" "$here/UnityStubAll.cs" "$here/GoldenStubExtra.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/GoldenSlot*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs" "$repo/Assets/_Project/Editor/GoldenSlotSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/Golden.dll" "${src[@]}"
done
echo OK

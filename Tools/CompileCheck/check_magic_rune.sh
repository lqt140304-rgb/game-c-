#!/usr/bin/env bash
# Biên dịch thử (không Unity) kịch bản Trận pháp Rune của Chest_Magic: lõi ChestFx + Chests/Magic*.cs + editor Magic*.
# Dùng stub chung như check_all.sh (UnityStub.cs chép ra, mọi kiểu thành partial, + UnityStubAll.cs). Cần mono mcs.
# Chỉ bắt lỗi cú pháp/kiểu, KHÔNG bắt lệch API Unity thật.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -E -e 's/public (static |abstract |sealed )?(class|struct) /public \1partial \2 /g' \
    -E -e 's/public enum (KeyCode|Key|TextAnchor|LightType|LightShadows|ParticleSystemStopBehavior|ParticleSystemShapeType|ParticleSystemRenderMode|PrimitiveType|ShadowCastingMode) \{[^}]*\}//g' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067,0108,0114 -out:"$out/UnityStub.dll" "$out/UnityStub.cs" "$here/UnityStubAll.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/Magic*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs"
     "$repo/Assets/_Project/Editor/MagicFxSetup.cs" "$repo/Assets/_Project/Editor/MagicRuneFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "NONE"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/MagicRune.dll" "${src[@]}"
done
echo OK

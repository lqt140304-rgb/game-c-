#!/usr/bin/env bash
# Biên dịch thử Chest_TheThao (kịch bản luân lưu: TheThaoHook, TheThaoBall, preset, Editor setup) cùng lõi ChestFx,
# với stub UnityStub.cs + UnityStubAll.cs (như check_chest_fx.sh). Chỉ bắt lỗi cú pháp/kiểu, không bắt lỗi lúc chạy. Cần mono-mcs.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -E -e 's/public (static |abstract |sealed )?(class|struct) /public \1partial \2 /g' \
    -E -e 's/public enum (KeyCode|Key|TextAnchor|LightType|LightShadows|ParticleSystemStopBehavior|ParticleSystemShapeType|ParticleSystemRenderMode|PrimitiveType|ShadowCastingMode) \{[^}]*\}//g' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067,0108,0114 -out:"$out/UnityStub.dll" "$out/UnityStub.cs" "$here/UnityStubAll.cs"
mapfile -t src < <(find "$repo/Assets/_Project/Scripts/ChestFx" "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs" -name '*.cs' | sort)
src+=("$repo/Assets/_Project/Editor/TheThaoFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "NONE"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/TheThao.dll" "${src[@]}"
done
echo OK

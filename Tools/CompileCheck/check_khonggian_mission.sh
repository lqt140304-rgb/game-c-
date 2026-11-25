#!/usr/bin/env bash
# Biên dịch thử (KHÔNG có Unity, chỉ stub) kịch bản mới của Chest_KhongGian: KhongGianMissionHook + KhongGianSynth
# cùng toàn bộ ChestFx, BoxOpenSequence và Editor/KhongGianFxSetup. Dùng stub đầy đủ như check_all.sh
# (UnityStub.cs đổi thành partial + UnityStubAll.cs) (đã thêm AudioClip.Create, ForceMode.Acceleration...).
# Chỉ bắt lỗi cú pháp/kiểu, không bắt lỗi API Unity thật. Cần mcs (apt-get install mono-mcs).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -E -e 's/public (static |abstract |sealed )?(class|struct) /public \1partial \2 /g' \
    -E -e 's/public enum (KeyCode|Key|TextAnchor|LightType|LightShadows|ParticleSystemStopBehavior|ParticleSystemShapeType|ParticleSystemRenderMode|PrimitiveType|ShadowCastingMode) \{[^}]*\}//g' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067,0108,0114 -out:"$out/UnityStub.dll" \
    "$out/UnityStub.cs" "$here/UnityStubAll.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/KhongGian*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs" "$repo/Assets/_Project/Editor/KhongGianFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "NONE"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/KG.dll" "${src[@]}"
done
echo OK

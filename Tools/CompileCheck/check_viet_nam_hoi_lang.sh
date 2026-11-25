#!/usr/bin/env bash
# Biên dịch thử (không Unity) kịch bản "Đêm hội làng" của Chest_VietNam: lõi ChestFx + VietNam*.cs + BoxOpenSequence +
# VietNamFxSetup + VietNamHoiLangSetup, dựng trên stub chung (UnityStub.cs làm partial + UnityStubAll.cs) như check_all.sh.
# (UnityStubAll.cs đã có ForceMode.Acceleration, Rigidbody.AddTorque/position, Collision.) Chỉ bắt lỗi cú pháp/kiểu.
# Cần mcs (apt-get install mono-mcs).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -E -e 's/public (static |abstract |sealed )?(class|struct) /public \1partial \2 /g' \
    -E -e 's/public enum (KeyCode|Key|TextAnchor|LightType|LightShadows|ParticleSystemStopBehavior|ParticleSystemShapeType|ParticleSystemRenderMode|PrimitiveType|ShadowCastingMode) \{[^}]*\}//g' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
cp "$here/UnityStubAll.cs" "$out/UnityStubAll.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067,0108,0114 -out:"$out/UnityStub.dll" "$out/UnityStub.cs" "$out/UnityStubAll.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/VietNam*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs"
     "$repo/Assets/_Project/Editor/VietNamFxSetup.cs" "$repo/Assets/_Project/Editor/VietNamHoiLangSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "NONE"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/ChestFx.dll" "${src[@]}"
done
echo OK

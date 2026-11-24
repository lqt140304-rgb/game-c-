#!/usr/bin/env bash
# Compile check of the Chest_KhongGian FX files WITHOUT Unity (stub only, catches syntax/type errors).
# Same as check_chest_fx.sh plus the khong_gian files; the stub copy gains PrefabUtility.SaveAsPrefabAsset.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -e 's|public static class PrefabUtility {|public static class PrefabUtility { public static GameObject SaveAsPrefabAsset(GameObject g, string p) => g;|' \
  -e "s|ParticleSystemStopBehavior { StopEmittingAndClear }|ParticleSystemStopBehavior { StopEmittingAndClear, StopEmitting }|" \
  "$here/UnityStub.cs" > "$out/UnityStub.cs"
grep -q SaveAsPrefabAsset "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -out:"$out/UnityStub.dll" "$out/UnityStub.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/KhongGian*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs" "$repo/Assets/_Project/Editor/ChestRevealTestSetup.cs"
     "$repo/Assets/_Project/Editor/KhongGianFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/ChestFx.dll" "${src[@]}"
done
echo OK

#!/usr/bin/env bash
# Compile check of the core chest FX + Chest_CoPhong files against the UnityEngine stub (no Unity needed).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
# The shared stub lacks PrefabUtility.SaveAsPrefabAsset and ParticleSystemStopBehavior.StopEmitting (real Unity API): patch a temporary copy, never the shared file.
sed -e 's/public static class PrefabUtility { /public static class PrefabUtility { public static GameObject SaveAsPrefabAsset(GameObject g, string path) => g; /' -e 's/ParticleSystemStopBehavior { StopEmittingAndClear }/ParticleSystemStopBehavior { StopEmittingAndClear, StopEmitting }/' "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -out:"$out/UnityStub.dll" "$out/UnityStub.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/CoPhong*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs" "$repo/Assets/_Project/Editor/ChestRevealTestSetup.cs"
     "$repo/Assets/_Project/Editor/CoPhongFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/ChestFx.dll" "${src[@]}"
done
echo OK

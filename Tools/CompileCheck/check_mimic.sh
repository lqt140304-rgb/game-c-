#!/usr/bin/env bash
# Compile check of the Mimic chest FX (core + Chests/Mimic* + MimicFxSetup) against the UnityEngine stub. No Unity needed.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -out:"$out/UnityStub.dll" "$here/UnityStub.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/Mimic*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs"
     "$repo/Assets/_Project/Editor/ChestRevealTestSetup.cs" "$repo/Assets/_Project/Editor/MimicFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/ChestFx.dll" "${src[@]}"
done
echo OK

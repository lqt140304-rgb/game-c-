#!/usr/bin/env bash
# Compile check of the Chest_ThienCo FX files with the shared stub (no Unity). The stub is copied to a temp dir
# and given the one Editor API ThienCoFxSetup uses that it lacks (PrefabUtility.SaveAsPrefabAsset).
# Needs mono's mcs (apt-get install mono-mcs).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed 's|public static UnityEngine.Object InstantiatePrefab(UnityEngine.Object o, Transform p) => o;|& public static GameObject SaveAsPrefabAsset(GameObject g, string path) => g;|' "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -out:"$out/UnityStub.dll" "$out/UnityStub.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/ThienCo*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs"
     "$repo/Assets/_Project/Editor/ChestRevealTestSetup.cs" "$repo/Assets/_Project/Editor/ThienCoFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/ChestFx.dll" "${src[@]}"
done
echo OK

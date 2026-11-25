#!/usr/bin/env bash
# Compile check for the Chest_VietNam FX files (VietNamFxPreset, VietNamHook, VietNamFxSetup) against the shared
# UnityEngine stub, with PrefabUtility.SaveAsPrefabAsset and Object.DestroyImmediate patched into a temp copy.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -e 's|public static UnityEngine.Object InstantiatePrefab(UnityEngine.Object o, Transform p) => o;|& public static GameObject SaveAsPrefabAsset(GameObject g, string p) => g;|' \
    -e 's|public void Emit(int n) {}|& public void Clear() {}|' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
grep -q "DestroyImmediate" "$out/UnityStub.cs" || echo "warning: stub has no DestroyImmediate"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -out:"$out/UnityStub.dll" "$out/UnityStub.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/VietNam*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs" "$repo/Assets/_Project/Editor/VietNamFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/ChestFx.dll" "${src[@]}"
done
echo OK

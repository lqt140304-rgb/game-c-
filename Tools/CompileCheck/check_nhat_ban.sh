#!/usr/bin/env bash
# Compile check of the Chest_NhatBan FX files with the shared stub (Tools/CompileCheck/UnityStub.cs), WITHOUT Unity.
# The stub copy gets PrefabUtility.SaveAsPrefabAsset and MonoBehaviour.StopCoroutine added on the fly (the shared stub is not edited).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -e 's|public static class PrefabUtility {|public static class PrefabUtility { public static GameObject SaveAsPrefabAsset(GameObject g, string p) => g;|' -e 's|public void StopAllCoroutines() {}|public void StopAllCoroutines() {} public void StopCoroutine(Coroutine c) {}|' "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -out:"$out/UnityStub.dll" "$out/UnityStub.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/NhatBan*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs" "$repo/Assets/_Project/Editor/NhatBanFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "NO_EDITOR"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/ChestFx.dll" "${src[@]}"
done
echo OK

#!/usr/bin/env bash
# Stub compile check (no Unity) of the core chest FX + the Chest_AmNhac files. Needs mono's mcs.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
# Copy of the shared stub with the few real Unity APIs the Am Nhac editor script also uses (shared file untouched).
sed -e 's/public void ApplyModifiedPropertiesWithoutUndo() {}/& public bool ApplyModifiedProperties() => true;/' \
    -e 's/public class SerializedProperty { /&public bool boolValue; /' \
    -e 's/public static class Undo { /&public static T AddComponent<T>(UnityEngine.GameObject g) where T : UnityEngine.Component => default; /' \
    -e 's/public T GetComponent<T>() => default;$/& public T GetComponentInParent<T>() => default; public T GetComponentInChildren<T>() => default;/' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -out:"$out/UnityStub.dll" "$out/UnityStub.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/AmNhac*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs"
     "$repo/Assets/_Project/Editor/ChestRevealTestSetup.cs" "$repo/Assets/_Project/Editor/AmNhacFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "NONE"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/AmNhac.dll" "${src[@]}"
done
echo OK

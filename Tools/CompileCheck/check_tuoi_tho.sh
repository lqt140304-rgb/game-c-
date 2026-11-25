#!/usr/bin/env bash
# Compile check of the Chest_TuoiTho FX files WITHOUT Unity (same idea as check_chest_fx.sh).
# Builds against a COPY of UnityStub.cs patched with the few APIs TuoiThoHook needs
# (ParticleSystem.EmitParams / Emit(EmitParams,int), Mathf.MoveTowards, Mathf.FloorToInt). The shared stub is not edited.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
sed -e 's|public void Emit(int n) {} }|public void Emit(int n) {} public struct EmitParams { public Vector3 position, velocity; } public void Emit(EmitParams p, int n) {} }|' \
    -e 's|public static bool Approximately(float a, float b) => true; }|public static bool Approximately(float a, float b) => true; public static float MoveTowards(float a, float b, float d) => a; public static int FloorToInt(float f) => 0; }|' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -out:"$out/UnityStub.dll" "$out/UnityStub.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/TuoiTho*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs" "$repo/Assets/_Project/Editor/ChestRevealTestSetup.cs"
     "$repo/Assets/_Project/Editor/TuoiThoFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/ChestFx.dll" "${src[@]}"
done
echo OK

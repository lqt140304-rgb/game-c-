#!/usr/bin/env bash
# Robot Bien Hinh: dung chung cach bien dich toan bo cua check_all.sh (stub UnityStub + UnityStubAll).
# One stub compile (no Unity) of EVERYTHING the chest work touches: all of Assets/_Project/Scripts (ChestFx, Chests,
# Interaction, Player, UI) and all of Assets/_Project/Editor, in ONE assembly, so duplicate class names and clashing members
# between branches show up. Builds against Tools/CompileCheck/UnityStub.cs (copied, every type made partial) plus
# UnityStubAll.cs (the extra API the whole set uses). Catches syntax and type errors only, not real Unity API mismatches.
# Needs mono's mcs (apt-get install mono-mcs).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
# Enums cannot be partial: the key enums are dropped here and redefined in full in UnityStubAll.cs.
sed -E -e 's/public (static |abstract |sealed )?(class|struct) /public \1partial \2 /g' \
    -E -e 's/public enum (KeyCode|Key|TextAnchor|LightType|LightShadows|ParticleSystemStopBehavior|ParticleSystemShapeType|ParticleSystemRenderMode|PrimitiveType|ShadowCastingMode) \{[^}]*\}//g' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067,0108,0114 -out:"$out/UnityStub.dll" "$out/UnityStub.cs" "$here/UnityStubAll.cs"
mapfile -t src < <(find "$repo/Assets/_Project/Scripts" -name '*.cs' | sort)
mapfile -t -O "${#src[@]}" src < <(find "$repo/Assets/_Project/Editor" -name '*.cs' | sort)
echo "files: ${#src[@]}"
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "NONE"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/All.dll" "${src[@]}"
done
echo OK

#!/usr/bin/env bash
# Biên dịch thử lõi hiệu ứng rương KHÔNG cần Unity: ChestFx (lõi + hook từng chủ đề), BoxOpenSequence và các Editor liên quan,
# với stub UnityEngine tự viết (UnityStub.cs + UnityStubAll.cs). Chỉ bắt lỗi cú pháp và kiểu, KHÔNG bắt sai khác API Unity thật
# hay lỗi lúc chạy. Cũng chạy check_particle_velocity.sh. Cần mono-mcs (apt-get install mono-mcs).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
bash "$here/check_particle_velocity.sh"
sed -E -e 's/public (static |abstract |sealed )?(class|struct) /public \1partial \2 /g' \
    -E -e 's/public enum (KeyCode|Key|TextAnchor|LightType|LightShadows|ParticleSystemStopBehavior|ParticleSystemShapeType|ParticleSystemRenderMode|PrimitiveType|ShadowCastingMode) \{[^}]*\}//g' \
    "$here/UnityStub.cs" > "$out/UnityStub.cs"
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067,0108,0114 -out:"$out/UnityStub.dll" "$out/UnityStub.cs" "$here/UnityStubAll.cs"
mapfile -t src < <(find "$repo/Assets/_Project/Scripts/ChestFx" "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs" -name '*.cs' | sort)
src+=("$repo/Assets/_Project/Editor/ChestRevealTestSetup.cs")
echo "files: ${#src[@]}"
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "NONE"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067 -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/ChestFx.dll" "${src[@]}"
done
echo OK

#!/usr/bin/env bash
# Kiểm bộ "chìa khoá" (kế hoạch 2026-11-26): biên dịch cả Scripts+Editor bằng check_all.sh, rồi kiểm cấu trúc bằng grep.
# Thoát != 0 nếu sai. Cần mono mcs (apt-get install mono-mcs).
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
chests="$repo/Assets/_Project/Scripts/ChestFx/Chests"
base="$repo/Assets/_Project/Scripts/ChestFx/ChestKeyHookBase.cs"
setup="$repo/Assets/_Project/Editor/ChestKeySetup.cs"
names=(Golden HaiTac RacQuy Magic Mimic Miner AmNhac VietNam TuoiTho Robot KhongGian Game TheThao TrungHoa NhatBan CoPhong ThienCo)
labels=("Golden" "Hai Tac" "Rac Quy" "Magic" "Mimic" "Miner" "Am Nhac" "Viet Nam" "Tuoi Tho" "Robot" "Khong Gian" "Game" "The Thao" "Trung Hoa" "Nhat Ban" "Co Phong" "Thien Co")
fail=0
bad() { echo "LOI: $*"; fail=1; }

echo "== (0) biên dịch toàn bộ"
bash "$here/check_all.sh" || bad "check_all.sh không qua"

echo "== (a) 17 lớp KeyHook và Preset"
for n in "${names[@]}"; do
  f="$chests/${n}KeyHook.cs"
  [ -f "$f" ] || { bad "thiếu $f"; continue; }
  grep -Eq "class ${n}KeyHook[[:space:]]*:[[:space:]]*ChestKeyHookBase" "$f" || bad "$n: không có 'class ${n}KeyHook : ChestKeyHookBase'"
  grep -Eq "static[[:space:]]+readonly[[:space:]]+KeyPreset[[:space:]]+Preset" "$f" || bad "$n: không có 'static readonly KeyPreset Preset'"
  if ! grep -q "KeyPreset.Neutral(" "$f"; then
    # Không dùng Neutral thì phải khởi tạo đủ từng trường.
    for fld in chestId displayName audioFolder filePrefix bedFile openAction squashMul racSquashMul jumpMul cycleDelta shakeScale; do
      grep -q "$fld" "$f" || bad "$n: Preset thiếu trường $fld"
    done
  fi
  grep -q "\"$n\"" "$f" || bad "$n: Preset.displayName không phải \"$n\""
done

echo "== (b) mã cấm trong *KeyHook.cs"
for f in "$chests"/*KeyHook.cs; do
  [ -f "$f" ] || continue
  if grep -nE 'UnityEngine\.Random|Rigidbody|ParticleSystem|new Light|AddComponent<Light>|Input\.|KeyCode|Time\.timeScale' "$f"; then
    bad "$(basename "$f") chứa mã cấm"
  fi
done

echo "== (c) ChestKeyHookBase"
if [ -f "$base" ] && grep -q "chia_khoa_xong" "$base"; then :; else bad "ChestKeyHookBase thiếu 'chia_khoa_xong'"; fi

for w in ThemeAction ThemeFly OnKeyImpact flyClip actionClip; do
  grep -q "$w" "$base" || bad "ChestKeyHookBase thiếu '$w'"
done
grep -q "public void PulseGlow" "$repo/Assets/_Project/Scripts/ChestFx/ChestRevealSequence.cs" || bad "ChestRevealSequence thiếu 'PulseGlow'"

echo "== (d) menu Setup trong ChestKeySetup.cs"
for l in "${labels[@]}"; do
  grep -q "Chia Khoa/Setup $l Chest" "$setup" || bad "thiếu menu 'Chia Khoa/Setup $l Chest'"
done
grep -q "Chia Khoa/Setup All" "$setup" || bad "thiếu menu 'Chia Khoa/Setup All'"

if [ "$fail" -ne 0 ]; then echo "KHONG QUA"; exit 1; fi
echo "OK chia khoa"

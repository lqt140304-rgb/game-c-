#!/usr/bin/env bash
# Kiểm LÕI mở rương: dữ liệu 17 rương, bảng đăng ký, hook âm thanh một dòng thời gian, an toàn shader. Không cần Unity.
# Gồm: biên dịch cả Scripts+Editor bằng check_all.sh, rồi kiểm cấu trúc bằng grep và kiểm tệp âm có thật.
# Thoát != 0 nếu sai. Cần mono mcs (apt-get install -y mono-mcs).
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
fx="$repo/Assets/_Project/Scripts/ChestFx"
chests="$fx/Chests"
names=(Golden HaiTac RacQuy Magic Mimic Miner AmNhac VietNam TuoiTho Robot KhongGian Game TheThao TrungHoa NhatBan CoPhong ThienCo)
fail=0
bad() { echo "LOI: $*"; fail=1; }

echo "== (0) biên dịch toàn bộ"
bash "$here/check_all.sh" || bad "check_all.sh không qua"

echo "== (a) 17 file dữ liệu Preset: đủ trường, không logic"
count=$(find "$chests" -name '*Preset.cs' | wc -l)
[ "$count" -eq 17 ] || bad "cần đúng 17 file *Preset.cs trong Chests/, đang có $count"
for n in "${names[@]}"; do
  f="$chests/${n}Preset.cs"
  [ -f "$f" ] || { bad "thiếu $f"; continue; }
  grep -Eq "static class ${n}Preset" "$f" || bad "$n: không có 'static class ${n}Preset'"
  grep -Eq "public static readonly ChestPreset Data" "$f" || bad "$n: không có 'public static readonly ChestPreset Data'"
  grep -q "displayName = \"$n\"" "$f" || bad "$n: displayName không phải \"$n\""
  for fld in chestId audioFolder filePrefix moveStyle shakeScale squashMul racSquashMul jumpMul cycleDelta bedFile accentFiles lockFiles; do
    grep -q "$fld" "$f" || bad "$n: thiếu trường $fld"
  done
  # lockFiles phải đủ 5 tên, accentFiles đủ 3 phần tử
  lk=$(grep -o '"[^"]*lock_[A-Za-z]*\.mp3"' "$f" | wc -l)
  [ "$lk" -eq 5 ] || bad "$n: lockFiles cần 5 tệp lock_*.mp3, đang có $lk"
  ac=$(grep 'accentFiles' "$f" | grep -v '///' | grep -o '"[^"]*"' | wc -l)
  [ "$ac" -eq 3 ] || bad "$n: accentFiles cần đúng 3 phần tử, đang có $ac"
  # chỉ dữ liệu: không rẽ nhánh, vòng lặp, ngẫu nhiên, thời gian, hàm
  if grep -vE '^\s*///' "$f" | grep -nE '\b(if|for|foreach|while|switch|return|Random|Time|UnityEngine|using)\b|=>'; then bad "$n: file dữ liệu chứa logic"; fi
done

echo "== (b) bảng đăng ký đủ 17 dòng"
reg="$fx/ChestPresetRegistry.cs"
for n in "${names[@]}"; do grep -q "${n}Preset.Data" "$reg" || bad "ChestPresetRegistry thiếu ${n}Preset.Data"; done

echo "== (c) tệp âm ghi trong dữ liệu phải có thật"
for n in "${names[@]}"; do
  f="$chests/${n}Preset.cs"
  folder=$(grep -o 'audioFolder = "[^"]*"' "$f" | sed 's/.*= "//; s/"$//')
  while read -r mp3; do
    [ -z "$mp3" ] && continue
    [ -f "$repo/$folder$mp3" ] || bad "$n: thiếu tệp $folder$mp3"
  done < <(grep -vE '^\s*///' "$f" | grep -E 'bedFile|accentFiles|lockFiles' | grep -o '"[^"]*\.mp3"' | tr -d '"')
done

echo "== (d) hook âm thanh: một dòng thời gian, nhân âm lượng một lần"
hook="$fx/ChestSoundHook.cs"
[ -f "$hook" ] || bad "thiếu ChestSoundHook.cs"
if grep -vE '^\s*///' "$hook" | grep -nE 'PlayOneShot|StartCoroutine|WaitForSeconds|unscaledTime|unscaledDeltaTime|UnityEngine\.Random'; then bad "ChestSoundHook dùng đồng hồ/phát riêng (PlayOneShot, coroutine, unscaledTime...)"; fi
mul=$(grep -vE '^\s*///' "$hook" | grep -c '\* volume\|volume \*')
[ "$mul" -le 1 ] || bad "ChestSoundHook nhân 'volume' ở nhiều hơn một chỗ ($mul)"
grep -q "private float Out(" "$hook" || bad "ChestSoundHook thiếu hàm Out (chỗ duy nhất nhân hệ số tổng)"
for ev in 'nac:' 'BopEvent' 'OnTierLocked' 'OnPress' 'OnReset'; do grep -q "$ev" "$hook" || bad "ChestSoundHook thiếu '$ev'"; done
grep -q 'announce' "$fx/ChestOpenScript.cs" || bad "ChestOpenScript không đặt announce (nấc/bóp méo không loan báo sự kiện)"

echo "== (e) mã cấm trong ChestFx"
# ChestRevealDemo và ChestFxLibrary.RandomItem chỉ chọn món minh hoạ cho bản demo (không phải hiệu ứng): được phép.
if grep -rnE 'UnityEngine\.Random|Random\.(Range|value|insideUnitSphere|insideUnitCircle|onUnitSphere)|Time\.timeScale\s*=' "$fx" --include=*.cs | grep -vE ':[0-9]+:\s*///' | grep -vE 'ChestRevealDemo\.cs|ChestFxLibrary\.cs'; then bad "mã cấm trong ChestFx"; fi

echo "== (f) không còn dấu vết vật mở khoá"
if grep -rnE 'ChestKey|KeyPreset|KeyHook|KeyContext|KeyAction|KeyAudioNames|chia_khoa|ThemeFly|ThemeAction|OnKeyImpact|flyFile|actionFile|keyFile' "$repo/Assets" --include=*.cs | grep -vE 'ChestOpenSetup\.cs.*(OldKeyHook|RemoveOldKeyHooks|KeyHooks|vật mở khoá)'; then bad "còn tham chiếu tới vật mở khoá trong Assets"; fi

echo "== (g) an toàn shader (không để hạt/mesh rơi vào shader lỗi màu tím)"
fac="$fx/ChestFxFactory.cs"
grep -q "ChestShaderResolver" "$fac" || bad "ChestFxFactory không dùng ChestShaderResolver"
if grep -n 'Shader\.Find(' "$fx"/*.cs | grep -v 'ChestShaderResolver.cs'; then bad "Shader.Find nằm ngoài ChestShaderResolver (tìm shader phải đi qua một chỗ có kiểm)"; fi
if grep -vE '^\s*///' "$fx"/*.cs | grep -n 'InternalErrorShader' | grep -v 'ChestShaderResolver'; then :; fi
for f in "$fx"/*.cs; do
  case "$f" in *ChestShaderResolver.cs) continue;; esac
  if grep -vE '^\s*///' "$f" | grep -n 'InternalErrorShader'; then bad "$(basename "$f") dùng Hidden/InternalErrorShader (sẽ ra màu tím)"; fi
done

if [ "$fail" -ne 0 ]; then echo "KHONG QUA"; exit 1; fi
echo "OK lõi mở rương"
